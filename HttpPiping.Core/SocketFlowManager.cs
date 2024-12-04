using Microsoft.Extensions.Logging;

namespace HttpPiping.Core
{
    public class SocketFlowManager : IFlowManager
    {
        private readonly ILogger? _logger;


        private Dictionary<Guid, SocketDestination> _destinations = new Dictionary<Guid, SocketDestination>();

        public SocketFlowManager(ILogger? logger)
        {
            _logger = logger;
        }

        public void Create(Guid key, string host, ushort port, bool keepAlive, IDestinationToSource destinationToSource)
        {
            _destinations.Add(key, new SocketDestination(_logger, key, host, port, keepAlive, destinationToSource));
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
