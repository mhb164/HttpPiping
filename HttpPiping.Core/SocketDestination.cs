using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HttpPiping.Core
{
    public class SocketDestination : IDestinationWorker
    {
        private readonly ILogger? _logger;
        public readonly Guid Key;
        public readonly string Host;
        public readonly ushort Port;
        public readonly bool KeepAlive;
        private readonly IDestinationToSource ToSource;

        private Socket DestinationSocket;
        private byte[] _buffer = new byte[1024];
        public SocketDestination(ILogger? logger, Guid key, string host, ushort port, bool keepAlive, IDestinationToSource toSource)
        {
            _logger = logger;
            Key = key;
            Host = host;
            Port = port;
            KeepAlive = keepAlive;
            ToSource = toSource;

            var DestinationEndPoint = new IPEndPoint(Dns.GetHostEntry(Host).AddressList[0], Port);
            DestinationSocket = new Socket(DestinationEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if (KeepAlive)
                DestinationSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);

            DestinationSocket.BeginConnect(DestinationEndPoint, new AsyncCallback(OnConnected), DestinationSocket);
        }

        private void OnConnected(IAsyncResult ar)
        {
            try
            {
                DestinationSocket.EndConnect(ar);
                ToSource?.OnDestinationConnected();
            }
            catch
            {
                Dispose();
            }
        }

        bool _disposing = false;
        bool _disposed = false;
        private void Dispose()
        {
            if (_disposing || _disposed)
                return;

            _disposing = true;

            try
            {
                DestinationSocket?.Shutdown(SocketShutdown.Both);
            }
            catch { }
            DestinationSocket = null;
            ToSource?.OnDestinationDispose();

            //Add destroy
            _disposed = true;
        }

        public void SendQuery(string query)
        {
            DestinationSocket.BeginSend(Encoding.ASCII.GetBytes(query), 0, query.Length, SocketFlags.None, new AsyncCallback(OnQuerySent), DestinationSocket);
        }

        private void OnQuerySent(IAsyncResult ar)
        {
            try
            {
                if (DestinationSocket.EndSend(ar) == -1)
                {
                    Dispose();
                    return;
                }
                ToSource?.OnDestinationQuerySent();
            }
            catch
            {
                Dispose();
            }
        }

        public void SendData(byte[] data)
        {
            DestinationSocket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(OnDataSent), DestinationSocket);
        }

        private void OnDataSent(IAsyncResult ar)
        {
            try
            {
                int Ret = DestinationSocket.EndSend(ar);
                if (Ret > 0)
                {
                    ToSource?.OnDestinationDataSent();
                    return;
                }
            }
            catch { }
            Dispose();
        }

        public void StartReceiveData()
        {
            DestinationSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(OnDataReceive), DestinationSocket);
        }

        protected void OnDataReceive(IAsyncResult ar)
        {
            try
            {
                int Ret = DestinationSocket.EndReceive(ar);
                if (Ret <= 0)
                {
                    Dispose();
                    return;
                }

                _logger?.LogTrace($"DestinationDataReceived [{Key}]: length: {Ret}");

                var data = new byte[Ret];
                Array.Copy(_buffer, data, Ret);

                ToSource?.OnDestinationDataReceived(data);
            }
            catch
            {
                Dispose();
            }
        }

        public void Destroy()
            => Dispose();
    }
}
