namespace NFCTestApp.Services
{
    public interface INFCService
    {
        void InitNFCDevice();
        bool IsNfcSupported();
        void SubscribeForNfcMessages();
        void WaitForNfcMessage();
        void StopSubscribeForNfcMessages();

        void PublishMessageToNfcTag(string zoneID);
        void StopPublishingMessage();
    }
}
