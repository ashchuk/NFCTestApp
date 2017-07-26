using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using NFCTestApp.Services;

namespace NFCTestApp.ViewModels
{
    public class TestNFCPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly INFCService _nfcService;

        private string _nfcReaderStatus;
        public string NfcReaderStatus
        {
            get { return _nfcReaderStatus; }
            set { SetProperty(ref _nfcReaderStatus, value); }
        }

        private string _nfcMessage;
        public string NfcMessage
        {
            get { return _nfcMessage; }
            set { SetProperty(ref _nfcMessage, value); }
        }

        private string _receivedTag;
        public string ReceivedTag
        {
            get { return _receivedTag; }
            set { SetProperty(ref _receivedTag, value); }
        }

        private string _selectedTag;
        public string SelectedTag
        {
            get { return _selectedTag; }
            set { SetProperty(ref _selectedTag, value); }
        }

        private bool _isNfcWaiting;
        public bool IsNfcWaiting
        {
            get { return _isNfcWaiting; }
            set { SetProperty(ref _isNfcWaiting, value); }
        }

        public ICommand BackCommand => new DelegateCommand(() => _navigationService.GoBack());
        public ICommand ReadTagCommand => new DelegateCommand(ReadTag);
        public ICommand WriteTagCommand => new DelegateCommand(WriteTag);

        public TestNFCPageViewModel(INavigationService navigationService, INFCService nfcService)
        {
            _navigationService = navigationService;
            _nfcService = nfcService;
        }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            SelectedTag = "Message to write";

            (_nfcService as NFCService).TagProcessed += TagProcessed;
            (_nfcService as NFCService).TagSent += TagSent;

            _nfcService.InitNFCDevice();

            if (_nfcService.IsNfcSupported())
                NfcReaderStatus = _nfcService.IsNfcSupported() ? "Supported" : "Unsupported";
            else
                NfcMessage = "NFC not supported";

            _nfcService.SubscribeForNfcMessages();

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            _nfcService.StopSubscribeForNfcMessages();
            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private void TagSent(object sender, EventArgs e)
        {
            NfcMessage = "Tag successfully sent!";
            IsNfcWaiting = false;
        }

        private void TagProcessed(object sender, NfcMessageReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Message))
                ReceivedTag = e.Message;
            else
                ReceivedTag = "Empty tag";

            NfcMessage = "Tag successfully received!";
            IsNfcWaiting = false;
        }

        private void ReadTag()
        {
            IsNfcWaiting = true;
            _nfcService.WaitForNfcMessage();
        }

        private void WriteTag()
        {
            IsNfcWaiting = true;
            _nfcService.PublishMessageToNfcTag(SelectedTag);
        }
    }
}
