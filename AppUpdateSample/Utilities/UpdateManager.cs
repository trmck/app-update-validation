using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Services.Store;

namespace AppUpdateSample.Utilities
{

    public delegate void DownloadProgressEventHandler(StorePackageUpdateStatus status);
    public class UpdateManager
    {
        private static UpdateManager _instance;

        private StoreContext _storeContext;

        public static event DownloadProgressEventHandler OnDownloadProgress;

        public static int PendingUpdatesCount { private set; get; }

        public static UpdateManager shared
        {
            get
            {
                if(UpdateManager._instance == null)
                {
                    _instance = new UpdateManager();
                }

                return _instance;
            }
        }

        private UpdateManager() { }

        private static void InitStoreContext()
        {
            if(UpdateManager.shared._storeContext == null)
            {
                shared._storeContext = StoreContext.GetDefault();
            }
        }

        public static async Task<IEnumerable<StorePackageUpdate>> CheckUpdatesAsync()
        {
            InitStoreContext();

            IEnumerable<StorePackageUpdate> updates = await UpdateManager.shared._storeContext.GetAppAndOptionalStorePackageUpdatesAsync();

            UpdateManager.PendingUpdatesCount = updates.Count();
            System.Diagnostics.Debug.WriteLine("Found " + UpdateManager.PendingUpdatesCount + " available updates");
            foreach (StorePackageUpdate update in updates)
            {
                System.Diagnostics.Debug.WriteLine("Found an update for the following package: " + update.Package.Description);
            }

            return updates;
        }

        public static async Task<Boolean> DownloadPackageUpdatesAsync(IEnumerable<StorePackageUpdate> updates)
        {
            InitStoreContext();

            bool success = false;

            IAsyncOperationWithProgress<StorePackageUpdateResult, StorePackageUpdateStatus> downloadOperation = shared._storeContext.RequestDownloadStorePackageUpdatesAsync(updates);

            downloadOperation.Progress = (asyncInfo, progress) =>
            {
                LogProgress(progress);
                UpdateManager.OnDownloadProgress(progress);
            };

            StorePackageUpdateResult result = await downloadOperation.AsTask();

            switch(result.OverallState)
            {
                case StorePackageUpdateState.Completed:
                    success = true;
                    break;
                case StorePackageUpdateState.OtherError:
                    System.Diagnostics.Debug.WriteLine("Error code: " + downloadOperation.ErrorCode);
                    break;
                default:
                    break;
            }

            return success;
        }

        private static void LogProgress(StorePackageUpdateStatus progress)
        {
            string packageName = progress.PackageFamilyName;

            ulong bytesDownloaded = progress.PackageBytesDownloaded;
            ulong totalBytes = progress.PackageDownloadSizeInBytes;

            double downloadProgress = progress.PackageDownloadProgress;
            double overallDownloadProgress = progress.TotalDownloadProgress;

            switch (progress.PackageUpdateState)
            {
                case StorePackageUpdateState.Pending:
                    System.Diagnostics.Debug.WriteLine("Package download for " + packageName + " is pending");
                    break;
                case StorePackageUpdateState.Downloading:
                    System.Diagnostics.Debug.WriteLine("Package download for " + packageName + " is downloading");
                    break;
                case StorePackageUpdateState.Completed:
                    System.Diagnostics.Debug.WriteLine("Package download for " + packageName + " is completed");
                    break;
                case StorePackageUpdateState.Canceled:
                    System.Diagnostics.Debug.WriteLine("Package download for " + packageName + " is cancelled");
                    break;
                case StorePackageUpdateState.OtherError:
                    System.Diagnostics.Debug.WriteLine("Package download for " + packageName + " has encountered an error");
                    break;
                case StorePackageUpdateState.ErrorLowBattery:
                    System.Diagnostics.Debug.WriteLine("Package download for " + packageName + " has stopped due to low battery");
                    break;
                case StorePackageUpdateState.ErrorWiFiRecommended:
                    System.Diagnostics.Debug.WriteLine("Package download for " + packageName + " has stopped due to no Wi-Fi recommendation");
                    break;
                case StorePackageUpdateState.ErrorWiFiRequired:
                    System.Diagnostics.Debug.WriteLine("Package download for " + packageName + " has stopped due to Wi-Fi requirement");
                    break;
            }

        }
    }
}
