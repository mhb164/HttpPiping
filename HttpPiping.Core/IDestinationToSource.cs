namespace HttpPiping.Core
{
    public interface IDestinationToSource
    {
        void OnDestinationConnected();
        void OnDestinationQuerySent();
        void OnDestinationDataSent();
        void OnDestinationDataReceived(byte[] data);
        void OnDestinationDispose();
    }
}
