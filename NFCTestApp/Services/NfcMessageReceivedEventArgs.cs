using System;

namespace NFCTestApp.Services
{
    public class NfcMessageReceivedEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}
