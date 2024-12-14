using System.Collections.Specialized;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace HttpPiping.Core
{
    public class PipeSocket : IDestinationToSource
    {
        public readonly Guid Key;
        private Socket _source;
        private readonly Action<PipeSocket> _destroyer;
        private readonly ILogger? _logger;
        private readonly IFlowManager _destinationManager;


        public PipeSocket(ILogger? logger, IFlowManager destinationManager, Socket source, Action<PipeSocket> destroyer)
        {
            _logger = logger;
            Key = Guid.NewGuid();
            _source = source;
            _destroyer = destroyer;
            _destinationManager = destinationManager;
        }

        private byte[] _buffer = new byte[4096]; //0<->4095 = 4096
        private string _httpQuery = string.Empty;
        private string HttpRequestType;
        private string HttpVersion;
        public string RequestedPath;
        private StringDictionary HeaderFields;
        private string m_HttpPost = null;

        internal void StartHandshake()
        {
            try
            {
                _source.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveQuery), _source);
            }
            catch
            {
                Dispose();
            }
        }

        private void OnReceiveQuery(IAsyncResult ar)
        {
            int Ret;
            try
            {
                Ret = _source?.EndReceive(ar) ?? 0;
            }
            catch
            {
                Ret = -1;
            }
            if (Ret <= 0)
            { //Connection is dead :(
                Dispose();
                return;
            }
            _httpQuery += Encoding.ASCII.GetString(_buffer, 0, Ret);
            //if received data is valid HTTP request...
            if (IsValidQuery(_httpQuery))
            {
                ProcessQuery(_httpQuery);
                //else, keep listening
            }
            else
            {
                try
                {
                    _source.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveQuery), _source);
                }
                catch
                {
                    Dispose();
                }
            }
        }

        private void ProcessQuery(string Query)
        {
            _logger?.LogTrace($"Query [{_source.RemoteEndPoint} - {Key}]: {Query}");

            HeaderFields = ParseQuery(Query);
            if (HeaderFields == null || !HeaderFields.ContainsKey("Host"))
            {
                SendBadRequest();
                return;
            }
            int Port;
            string Host;
            int Ret;

            var isHttps = false;
            if (HttpRequestType.ToUpper().Equals("CONNECT"))
            { //HTTPS
                isHttps = true;
                Ret = RequestedPath.IndexOf(":");
                if (Ret >= 0)
                {
                    Host = RequestedPath.Substring(0, Ret);
                    if (RequestedPath.Length > Ret + 1)
                        Port = int.Parse(RequestedPath.Substring(Ret + 1));
                    else
                        Port = 443;
                }
                else
                {
                    Host = RequestedPath;
                    Port = 443;
                }
            }
            else
            { //Normal HTTP
                Ret = ((string)HeaderFields["Host"]).IndexOf(":");
                if (Ret > 0)
                {
                    Host = ((string)HeaderFields["Host"]).Substring(0, Ret);
                    Port = int.Parse(((string)HeaderFields["Host"]).Substring(Ret + 1));
                }
                else
                {
                    Host = (string)HeaderFields["Host"];
                    Port = 80;
                }
                if (HttpRequestType.ToUpper().Equals("POST"))
                {
                    int index = Query.IndexOf("\r\n\r\n");
                    m_HttpPost = Query.Substring(index + 4);
                }
            }

            _logger?.LogInformation($"{(isHttps ? "Https>" : "Http>")} {Host}:{Port} ({HttpVersion})");

            try
            {
                var keepAlive = HeaderFields.ContainsKey("Proxy-Connection") && HeaderFields["Proxy-Connection"].ToLower().Equals("keep-alive");
                _destinationManager.Create(Key, Host, (ushort)Port, keepAlive, this as IDestinationToSource);
            }
            catch
            {
                SendBadRequest();
                return;
            }
        }

        private string RebuildQuery()
        {
            string ret = HttpRequestType + " " + RequestedPath + " " + HttpVersion + "\r\n";
            if (HeaderFields != null)
            {
                foreach (string sc in HeaderFields.Keys)
                {
                    if (sc.Length < 6 || !sc.Substring(0, 6).Equals("proxy-"))
                        ret += System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(sc) + ": " + (string)HeaderFields[sc] + "\r\n";
                }
                ret += "\r\n";
                if (m_HttpPost != null)
                    ret += m_HttpPost;
            }
            return ret;
        }

        public void OnDestinationConnected()
        {
            string rq;
            if (HttpRequestType.ToUpper().Equals("CONNECT"))
            { //HTTPS
                rq = HttpVersion + " 200 Connection established\r\n\r\n";
                _source.BeginSend(Encoding.ASCII.GetBytes(rq), 0, rq.Length, SocketFlags.None, new AsyncCallback(OnOkSent), _source);
            }
            else
            { //Normal HTTP
                rq = RebuildQuery();
                _destinationManager.SendQuery(Key, rq);
            }
        }

        public void OnDestinationQuerySent()
        {
            StartRelay();
        }

        private void StartRelay()
        {
            try
            {
                _source.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(OnSourceDataReceive), _source);
                _destinationManager.StartReceiveData(Key);
            }
            catch
            {
                Dispose();
            }
        }

        protected void OnSourceDataReceive(IAsyncResult ar)
        {
            try
            {
                int Ret = _source?.EndReceive(ar) ?? 0;
                if (Ret <= 0)
                {
                    Dispose();
                    return;
                }

                _logger?.LogTrace($"SourceDataReceive [{_source.RemoteEndPoint}-{Key}]: length: {Ret}");


                var data = new byte[Ret];
                Array.Copy(_buffer, data, Ret);

                _destinationManager.SendData(Key, data);
            }
            catch (Exception ex)
            {
                Dispose();
            }
        }

        public void OnDestinationDataSent()
        {
            _source?.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(OnSourceDataReceive), _source);
        }

        public void OnDestinationDataReceived(byte[] data)
        {
            _source?.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(OnSourceDataSent), _source);

        }

        protected void OnSourceDataSent(IAsyncResult ar)
        {
            try
            {
                int Ret = _source?.EndSend(ar) ?? 0;
                if (Ret > 0)
                {
                    _destinationManager.StartReceiveData(Key);
                    return;
                }
            }
            catch { }
            Dispose();
        }

        private void OnOkSent(IAsyncResult ar)
        {
            try
            {
                int Ret = _source?.EndSend(ar) ?? 0;
                if (Ret <= 0)
                {
                    Dispose();
                    return;
                }
                StartRelay();
            }
            catch
            {
                Dispose();
            }
        }

        private void OnErrorSent(IAsyncResult ar)
        {
            try
            {
                _source?.EndSend(ar);
            }
            catch { }
            Dispose();
        }

        private bool IsValidQuery(string Query)
        {
            int index = Query.IndexOf("\r\n\r\n");
            if (index == -1)
                return false;
            HeaderFields = ParseQuery(Query);
            if (HttpRequestType.ToUpper().Equals("POST"))
            {
                try
                {
                    int length = int.Parse((string)HeaderFields["Content-Length"]);
                    return Query.Length >= index + 6 + length;
                }
                catch
                {
                    SendBadRequest();
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        private StringDictionary ParseQuery(string Query)
        {
            StringDictionary retdict = new StringDictionary();
            string[] Lines = Query.Replace("\r\n", "\n").Split('\n');
            int Cnt, Ret;
            //Extract requested URL
            if (Lines.Length > 0)
            {
                //Parse the Http Request Type
                Ret = Lines[0].IndexOf(' ');
                if (Ret > 0)
                {
                    HttpRequestType = Lines[0].Substring(0, Ret);
                    Lines[0] = Lines[0].Substring(Ret).Trim();
                }
                //Parse the Http Version and the Requested Path
                Ret = Lines[0].LastIndexOf(' ');
                if (Ret > 0)
                {
                    HttpVersion = Lines[0].Substring(Ret).Trim();
                    RequestedPath = Lines[0].Substring(0, Ret);
                }
                else
                {
                    RequestedPath = Lines[0];
                }
                //Remove http:// if present
                if (RequestedPath.Length >= 7 && RequestedPath.Substring(0, 7).ToLower().Equals("http://"))
                {
                    Ret = RequestedPath.IndexOf('/', 7);
                    if (Ret == -1)
                        RequestedPath = "/";
                    else
                        RequestedPath = RequestedPath.Substring(Ret);
                }
            }
            for (Cnt = 1; Cnt < Lines.Length; Cnt++)
            {
                Ret = Lines[Cnt].IndexOf(":");
                if (Ret > 0 && Ret < Lines[Cnt].Length - 1)
                {
                    try
                    {
                        retdict.Add(Lines[Cnt].Substring(0, Ret), Lines[Cnt].Substring(Ret + 1).Trim());
                    }
                    catch { }
                }
            }
            return retdict;
        }

        private void SendBadRequest()
        {
            var badRequest = """
                   HTTP/1.1 400 Bad Request
                   Connection: close
                   Content-Type: text/html
                   
                   <html>

                   <head>
                       <title>400 Bad Request</title>
                   </head>

                   <body>
                       <div align="center">
                           Http Piping not possible!
                       </div>
                   </body>

                   </html>
                   """;
            try
            {
                _source.BeginSend(Encoding.ASCII.GetBytes(badRequest), 0, badRequest.Length, SocketFlags.None, new AsyncCallback(OnErrorSent), _source);
            }
            catch
            {
                Dispose();
            }
        }

        public void OnDestinationDispose()
        {
            Dispose();
        }

        bool _disposing = false;
        bool _disposed = false;
        public void Dispose()
        {
            if (_disposing || _disposed)
                return;

            _disposing = true;
            try
            {
                _source.Shutdown(SocketShutdown.Both);
            }
            catch { }
            try
            {
                _destinationManager.Destroy(Key);
                //DestinationSocket.Shutdown(SocketShutdown.Both);
            }
            catch { }
            //Close the sockets
            if (_source != null)
                _source.Close();
            //if (DestinationSocket != null)
            //    DestinationSocket.Close();
            //Clean up
            _source = null;
            //DestinationSocket = null;
            _destroyer?.Invoke(this);

            _disposed = true;
        }
    }
}
