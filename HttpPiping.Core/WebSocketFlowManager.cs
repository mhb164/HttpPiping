using Microsoft.Extensions.Logging;

namespace HttpPiping.Core
{
    public class WebSocketFlowManager : IFlowManager
    {
        private readonly ILogger? _logger;
        public readonly Uri WSAddress;

        private Dictionary<Guid, WebSocketDestination> _destinations = new Dictionary<Guid, WebSocketDestination>();


        public WebSocketFlowManager(ILogger? logger, string address, bool isSecure)
        {
            _logger = logger;

            WSAddress = isSecure ?
                new Uri($"wss://{address}/Destinations/ws") :
                new Uri($"ws://{address}/Destinations/ws");
        }

        public void Create(Guid key, string host, ushort port, bool keepAlive, IDestinationToSource destinationToSource)
        {
            _destinations.Add(key, new WebSocketDestination(_logger, WSAddress, key, host, port, keepAlive, destinationToSource));
        }

        public void SendQuery(Guid key, string query)
        {
            _destinations.TryGetValue(key, out var destination);
            destination?.SendQuery(query);
        }

        public void SendData(Guid key, byte[] data)
        {
            _destinations.TryGetValue(key, out var destination);
            destination?.SendData(data);
        }

        public void StartReceiveData(Guid key)
        {
            _destinations.TryGetValue(key, out var destination);
            destination?.StartReceiveData();
        }

        public void Destroy(Guid key)
        {
            _destinations.TryGetValue(key, out var destination);
            destination?.Destroy();
            _destinations.Remove(key);
        }
    }
}
