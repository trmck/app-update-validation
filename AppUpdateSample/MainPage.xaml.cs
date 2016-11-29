using AppUpdateSample.Utilities;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AppUpdateSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            appName.Text = ApplicationData.ApplicationName;
            appVersion.Text = "Version " + ApplicationData.ApplicationVersion;
            deviceMake.Text = ApplicationData.DeviceManufacturer;
            deviceModel.Text = ApplicationData.DeviceModel;
            firmwareVersion.Text = ApplicationData.FirmwareVersion;
            systemFamily.Text = ApplicationData.SystemFamily;
            systemVersion.Text = ApplicationData.SystemVersion;

            UpdateManager.OnDownloadProgress += UpdateManager_OnDownloadProgress;
        }

        private void EnableCheckUpdateButton(bool enabled)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,() => {
                if (checkUpdatesButton != null)
                {
                    checkUpdatesButton.IsEnabled = enabled;
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private void UpdateManager_OnDownloadProgress(Windows.Services.Store.StorePackageUpdateStatus status)
        {
            switch (status.PackageUpdateState)
            {
                case Windows.Services.Store.StorePackageUpdateState.Pending:
                case Windows.Services.Store.StorePackageUpdateState.Downloading:
                case Windows.Services.Store.StorePackageUpdateState.Deploying:
                    EnableCheckUpdateButton(false);
                    break;
                default:
                    EnableCheckUpdateButton(true);
                    break;
            }
        }

        private async void CheckUpdateButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            EnableCheckUpdateButton(false);
            var updates = await UpdateManager.CheckUpdatesAsync();

            if(UpdateManager.PendingUpdatesCount == 0)
            {
                updateStatusBar.Background = new SolidColorBrush(Colors.Green);
                updateStatusText.Text = "App is up to date!";
            }
            else
            {
                updateStatusBar.Background = new SolidColorBrush(Colors.Red);
                updateStatusText.Text = "App has " + UpdateManager.PendingUpdatesCount + " pending update(s).";
            }

            bool updatesDownloaded = await UpdateManager.DownloadPackageUpdatesAsync(updates);
            if(updatesDownloaded)
            {
                var enumerator = updates.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var update = enumerator.Current;
                    System.Diagnostics.Debug.WriteLine(update);
                }
            }
            EnableCheckUpdateButton(true);
        }
    }
}
