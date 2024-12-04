using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace HttpPiping.Core
{
    public class PipeServer
    {
        private readonly ILogger? _logger;
        private readonly IFlowManager _flowManager;
        private readonly object _listenerLock = new object();

        public PipeServer(ILogger logger, IFlowManager flowManager, IPAddress address, ushort port)
        {
            _logger = logger;
            _flowManager = flowManager;
            Address = address;
            Port = port;
            _pipeSockets = new Dictionary<Guid, PipeSocket>();
        }

        public IPAddress Address { get; private set; }
        public ushort Port { get; private set; }

        private Socket? _listener;
        private Dictionary<Guid, PipeSocket> _pipeSockets;

        public PipeServer Start()
        {
            lock (_listenerLock)
            {
                if (_listener != null)
                    return this;

                _listener = new Socket(Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            }

            new Thread(() =>
            {
                _listener.Bind(new IPEndPoint(Address, Port));
                _listener.Listen(50);

                _logger?.LogInformation("Listening on {Address}:{Port} started", Address, Port);

                _listener.BeginAccept(new AsyncCallback(OnAccept), _listener);
            })
            { IsBackground = true, Name = $"Server {Port}" }.Start();

            return this;
        }

        public void Stop()
        {
            _listener.Close();
        }

        public void OnAccept(IAsyncResult ar)
        {
            try
            {
                Socket newSocket = _listener.EndAccept(ar);
                if (newSocket != null)
                {
                    var pipeSocket = new PipeSocket(_logger, _flowManager, newSocket, RemovePipeSocket);
                    Add(pipeSocket);
                    pipeSocket.StartHandshake();
                }
            }
            catch { }

            try
            {
                //Restart Listening
                _listener.BeginAccept(new AsyncCallback(this.OnAccept), _listener);
            }
            catch
            {
                //Dispose();
            }
        }

        private void Add(PipeSocket pipeSocket)
        {
            _pipeSockets.Add(pipeSocket.Key, pipeSocket);
        }

        private void RemovePipeSocket(PipeSocket pipeSocket)
        {
            _pipeSockets.Remove(pipeSocket.Key);
        }
    }
}
