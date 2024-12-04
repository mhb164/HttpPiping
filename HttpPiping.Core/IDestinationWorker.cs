namespace HttpPiping.Core
{
    public interface IDestinationWorker
    {
        void SendQuery(string query);
        void SendData(byte[] data);
        void StartReceiveData();
        void Destroy();
    }
}
