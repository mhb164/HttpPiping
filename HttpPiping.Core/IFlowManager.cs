namespace HttpPiping.Core
{
    public interface IFlowManager
    {
        void Create(Guid key, string host, ushort port, bool keepAlive, IDestinationToSource destinationToSource);
        void SendQuery(Guid key, string query);
        void SendData(Guid key, byte[] data);
        void StartReceiveData(Guid key);
        void Destroy(Guid key);
    }
}
