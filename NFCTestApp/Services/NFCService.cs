using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Networking.Proximity;
using System;
using Windows.UI.Core;
using Prism.Logging;

namespace NFCTestApp.Services
{
    public class NFCService : INFCService
    {
        private readonly ILoggerFacade _loggingService;

        public event EventHandler<NfcMessageReceivedEventArgs> TagProcessed;
        public event EventHandler TagSent;

        private ProximityDevice _device;
        private long _subscriptionMessageId;
        private long _publishingMessageId;

        public NFCService(ILoggerFacade loggingService)
        {
            _loggingService = loggingService;
        }

        public void InitNFCDevice() =>
            _device = ProximityDevice.GetDefault();

        public bool IsNfcSupported() =>
            _device != null;

        public void SubscribeForNfcMessages()
        {
            if (_device == null) return;

            if (_subscriptionMessageId != 0) StopSubscribeForNfcMessages();

            _subscriptionMessageId = _device.SubscribeForMessage("NDEF", MessageReceivedHandler);
            _loggingService.Log($"NFCService subscribe for NFC messages", Category.Info, Priority.High);
        }

        public void StopSubscribeForNfcMessages()
        {
            if (_subscriptionMessageId != 0 && _device != null)
            {
                _device.StopSubscribingForMessage(_subscriptionMessageId);
                _subscriptionMessageId = 0;
            }
            _loggingService.Log($"NFCService stop subscribe for Nfc messages", Category.Info, Priority.High);
        }

        public void StopPublishingMessage()
        {
            if (_publishingMessageId != 0 && _device != null)
            {
                _device.StopPublishingMessage(_publishingMessageId);
                _publishingMessageId = 0;
            }
            _loggingService.Log($"NFCService stop publishing messages", Category.Info, Priority.High);
        }

        private async void MessageReceivedHandler(ProximityDevice sender, ProximityMessage message)
        {
            try
            {
                string rawMessage = System.Text.Encoding.UTF8.GetString(message.Data.ToArray());
                var splittedParameters = rawMessage.Split('#');
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                 () =>
                 {
                     string data;
                     if (splittedParameters.Length < 2)
                         data = "";
                     else
                         data = splittedParameters[1];
                     TagProcessed?.Invoke(this, new NfcMessageReceivedEventArgs() { Message = data });
                 });
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Exception in MessageReceivedHandler: {ex}", Category.Info, Priority.High);
            }
        }

        public void WaitForNfcMessage()
        {
            if (_device == null) return;

            if (_subscriptionMessageId != 0) StopSubscribeForNfcMessages();

            _subscriptionMessageId = _device.SubscribeForMessage("NDEF", InvokeTagProcessed);
        }

        private async void InvokeTagProcessed(ProximityDevice sender, ProximityMessage message)
        {
            string rawMessage = System.Text.Encoding.UTF8.GetString(message.Data.ToArray());
            var splittedParameters = rawMessage.Split('#');
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
             () =>
             {
                 string data;
                 if (splittedParameters.Length < 2)
                     data = "";
                 else
                     data = splittedParameters[1];
                 TagProcessed?.Invoke(this, new NfcMessageReceivedEventArgs() { Message = data });
             });
            StopSubscribeForNfcMessages();
        }

        public void PublishMessageToNfcTag(string data)
        {
            if (_device == null)
                return;

            StopPublishingMessage();
            StopSubscribeForNfcMessages();

            string launchAppMessage = string.Join("#", new string[] {
                    "MyApp",
                    "\tWindows\t",
                    data+"#"
                });

            var dataWriter = new Windows.Storage.Streams.DataWriter();
            dataWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf16LE;
            dataWriter.WriteString(launchAppMessage);
            _publishingMessageId = _device.PublishBinaryMessage("LaunchApp:WriteTag", dataWriter.DetachBuffer(), MessageWrittenHandler);
        }

        private async void MessageWrittenHandler(ProximityDevice sender, long messageid)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    TagSent?.Invoke(this, null);
                });
            StopPublishingMessage();
        }
    }
}
