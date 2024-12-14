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
        private readonly object _pipeSocketsLock = new object();
        private readonly Dictionary<Guid, PipeSocket> _pipeSockets;

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
            StopPipeSockets();

            try { _listener?.Close(); }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "On stop, closing listener error");
            }
            finally
            {
                _listener = null;
            }

            _logger?.LogInformation("Listening on {Address}:{Port} stoped", Address, Port);

        }

        private void StopPipeSockets()
        {
            PipeSocket[] pipeSockets;
            lock (_pipeSocketsLock)
                pipeSockets = _pipeSockets.Values.ToArray();

            foreach (var pipeSocket in pipeSockets)
            {
                try { pipeSocket.Dispose(); }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "On StopPipeSockets, dispose pipeSocket {PipeSocketKey} error", pipeSocket.Key);
                }
            }

            lock (_pipeSocketsLock)
                _pipeSockets.Clear();
        }

        public void OnAccept(IAsyncResult ar)
        {
            try
            {
                var newSocket = _listener?.EndAccept(ar);
                if (newSocket != null)
                {
                    var pipeSocket = new PipeSocket(_logger, _flowManager, newSocket, RemovePipeSocket);
                    Add(pipeSocket);
                    pipeSocket.StartHandshake();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "OnAccept new socket error");
            }

            try
            {
                //Restart Listening
                _listener?.BeginAccept(new AsyncCallback(this.OnAccept), _listener);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "BeginAccept listener");
            }
        }

        private void Add(PipeSocket pipeSocket)
        {
            lock (_pipeSocketsLock)
                _pipeSockets.Add(pipeSocket.Key, pipeSocket);
        }

        private void RemovePipeSocket(PipeSocket pipeSocket)
        {
            lock (_pipeSocketsLock)
                _pipeSockets.Remove(pipeSocket.Key);
        }
    }
}
