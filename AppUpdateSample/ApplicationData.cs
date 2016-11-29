using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;

namespace AppUpdateSample
{
    public static class ApplicationData
    {
        public static string SystemFamily { get; }
        public static string SystemVersion { get; }
        public static string SystemArchitecture { get; }
        public static string ApplicationName { get; }
        public static string ApplicationVersion { get; }
        public static string DeviceManufacturer { get; }
        public static string DeviceModel { get; }
        public static string FirmwareVersion { get; }

        static ApplicationData()
        {
            AnalyticsVersionInfo versionInfo = Windows.System.Profile.AnalyticsInfo.VersionInfo;
            SystemFamily = versionInfo.DeviceFamily;
            string deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            ulong v = ulong.Parse(deviceFamilyVersion);
            ulong v1 = (v & 0xFFFF000000000000L) >> 48;
            ulong v2 = (v & 0x0000FFFF00000000L) >> 32;
            ulong v3 = (v & 0x00000000FFFF0000L) >> 16;
            ulong v4 = (v & 0x000000000000FFFFL);
            SystemVersion = $"{v1}.{v2}.{v3}.{v4}";

            Package package = Package.Current;
            SystemArchitecture = package.Id.Architecture.ToString();
            ApplicationName = package.DisplayName;
            PackageVersion packageVersion = package.Id.Version;
            ApplicationVersion = String.Format("{0}.{1}.{2}.{3}", packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);

            EasClientDeviceInformation clientDeviceInformation = new EasClientDeviceInformation();
            DeviceManufacturer = clientDeviceInformation.SystemManufacturer;
            DeviceModel = clientDeviceInformation.SystemProductName;
            FirmwareVersion = clientDeviceInformation.SystemFirmwareVersion;
        }
    }
}
