using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;

namespace HttpPiping.Core
{
    public class WebSocketDestination : IDestinationWorker
    {
        private readonly ILogger? _logger;
        public readonly Uri WSAddress;
        public readonly Guid Key;
        public readonly string Host;
        public readonly ushort Port;
        public readonly bool KeepAlive;
        private readonly IDestinationToSource ToSource;

        private ClientWebSocket destinationSocket;
        public WebSocketDestination(ILogger? logger, Uri wsAddress, Guid key, string host, ushort port, bool keepAlive, IDestinationToSource toSource)
        {
            _logger = logger;
            WSAddress = wsAddress;
            Key = key;
            Host = host;
            Port = port;
            KeepAlive = keepAlive;
            ToSource = toSource;

            Init();
        }

        private async Task Init()
        {
            destinationSocket = new ClientWebSocket();
            using var source = new CancellationTokenSource();
            source.CancelAfter(10_000);

            await destinationSocket.ConnectAsync(WSAddress, source.Token);
            var startCommand = string.Join(",", [Key.ToString("N"), Host, Port.ToString(), KeepAlive.ToString()]);
            await destinationSocket.SendAsync(Encoding.UTF8.GetBytes(startCommand), WebSocketMessageType.Text, true, source.Token);

            byte[] buffer = new byte[256];
            var receiveResult = await destinationSocket.ReceiveAsync(buffer, CancellationToken.None);
            var okRespond = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);

            if (okRespond == "OK")
                OnConnected();
            else
                Dispose();
        }

        private void OnConnected()
        {
            try
            {
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
                destinationSocket?.Dispose();
            }
            catch { }
            destinationSocket = null;
            ToSource?.OnDestinationDispose();

            _disposed = true;
        }

        public void SendQuery(string query)
        {
            destinationSocket.SendAsync(Encoding.ASCII.GetBytes(query), WebSocketMessageType.Binary, false, CancellationToken.None).Wait();
            OnQuerySent();
            //DestinationSocket.BeginSend(Encoding.ASCII.GetBytes(query), 0, query.Length, SocketFlags.None, new AsyncCallback(OnQuerySent), DestinationSocket);
        }

        private void OnQuerySent()
        {
            try
            {
                ToSource?.OnDestinationQuerySent();
            }
            catch
            {
                Dispose();
            }
        }

        public void SendData(byte[] data)
        {
            destinationSocket.SendAsync(data, WebSocketMessageType.Binary, false, CancellationToken.None);
            _logger?.LogTrace($"SendData [{Key}]: length: {data.Length}");
            if (data.Length == 7)
                _logger?.LogTrace($"SendData [{Key}]: {data.Length}> {string.Join("-", data.Select(x => $"{x:X2}"))}");


            OnDataSent();
            //DestinationSocket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(OnDataSent), DestinationSocket);
        }

        private void OnDataSent()
        {
            try
            {
                ToSource?.OnDestinationDataSent();
            }
            catch (Exception ex)
            {
            }
        }

        bool FirstStartReceiveDataCalled = false;

        //object _startReceiveDataCalledLock = new object();
        //bool _startReceiveDataCalled = false;
        //public bool StartReceiveDataCalled
        //{
        //    get
        //    {
        //        lock (_startReceiveDataCalledLock)
        //            return _startReceiveDataCalled;
        //    }
        //    set
        //    {
        //        lock (_startReceiveDataCalledLock)
        //            _startReceiveDataCalled = value;
        //    }
        //}

        public void StartReceiveData()
        {
            //StartReceiveDataCalled = true;

            if (!FirstStartReceiveDataCalled)
            {
                FirstStartReceiveDataCalled = true;
                new Thread(async () =>
                {
                    try
                    {
                        byte[] buffer = new byte[8192];
                        var receiveResult = await destinationSocket.ReceiveAsync(buffer, CancellationToken.None);
                        while (!receiveResult.CloseStatus.HasValue)
                        {
                            //StartReceiveDataCalled = false;

                            var data = new byte[receiveResult.Count];
                            Array.Copy(buffer, data, receiveResult.Count);

                            OnDataReceive(data);

                            //while (!StartReceiveDataCalled)
                            //    await Task.Delay(50);

                            receiveResult = await destinationSocket.ReceiveAsync(buffer, CancellationToken.None);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                })
                { IsBackground = true }.Start();
            }

            //DestinationSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(OnDataReceive), DestinationSocket);
        }

        protected void OnDataReceive(byte[] data)
        {
            try
            {
                _logger?.LogTrace($"DestinationDataReceived [{Key}]: length: {data.Length}");
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
