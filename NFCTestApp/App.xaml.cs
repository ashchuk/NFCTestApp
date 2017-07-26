using NFCTestApp.Services;
using Prism.Unity.Windows;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

namespace NFCTestApp
{
    sealed partial class App : PrismUnityApplication
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
            NavigationService.Navigate("TestNFC", null);
            return Task.FromResult<object>(null);
        }

        protected override Task OnActivateApplicationAsync(IActivatedEventArgs args)
        {
            NavigationService.Navigate("TestNFC", null);
            return Task.FromResult<object>(null);
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            RegisterTypeIfMissing(typeof(INFCService), typeof(NFCService), true);
        }
    }
}
