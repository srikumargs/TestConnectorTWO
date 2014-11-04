using System;

namespace Sage.CRE.ComponentIdentification.Helpers
{
    public class MSIUninstallInformation
    {
        public string AuthorizedCDFPrefix { get; set; }
        public string Comment { get; set; }
        public string Contact { get; set; }
        public string DisplayName { get; set; }
        public string DisplayVersion { get; set; }
        public UInt32? EstimatedSize { get; set; }
        public string HelpLink { get; set; }
        public string HelpTelephone { get; set; }
        public string InstallDate { get; set; }
        public string InstallLocation { get; set; }
        public string InstallSource { get; set; }
        public UInt32? Language { get; set; }
        public string ModifyPath { get; set; }
        public UInt32? NoModify { get; set; }
        public UInt32? NoRepair { get; set; }
        public string Publisher { get; set; }
        public string Readme { get; set; }
        public string Size { get; set; }
        public string UninstallString { get; set; }
        public string URLInfoAbout { get; set; }
        public string URLUpdateInfo { get; set; }
        public UInt32? Version { get; set; }
        public UInt32? VersionMajor { get; set; }
        public UInt32? VersionMinor { get; set; }
        public UInt32? WindowsInstaller { get; set; }

        public void PopulateFromUninstallRegistry(string productKey)
        {
            string error;
            AuthorizedCDFPrefix = RegistryReader.GetMSIInstalledProductStringValue(productKey, "AuthorizedCDFPrefix", out error);
            Comment = RegistryReader.GetMSIInstalledProductStringValue(productKey, "Comment", out error);
            Contact = RegistryReader.GetMSIInstalledProductStringValue(productKey, "Contact", out error);
            DisplayName = RegistryReader.GetMSIInstalledProductStringValue(productKey, "DisplayName", out error);
            DisplayVersion = RegistryReader.GetMSIInstalledProductStringValue(productKey, "DisplayVersion", out error);
            EstimatedSize = RegistryReader.GetMSIInstalledProductDWordValue(productKey, "EstimatedSize", out error);
            HelpLink = RegistryReader.GetMSIInstalledProductStringValue(productKey, "HelpLink", out error);
            HelpTelephone = RegistryReader.GetMSIInstalledProductStringValue(productKey, "HelpTelephone", out error);
            InstallDate = RegistryReader.GetMSIInstalledProductStringValue(productKey, "InstallDate", out error);
            InstallLocation = RegistryReader.GetMSIInstalledProductStringValue(productKey, "InstallLocation", out error);
            InstallSource = RegistryReader.GetMSIInstalledProductStringValue(productKey, "InstallSource", out error);
            Language = RegistryReader.GetMSIInstalledProductDWordValue(productKey, "Language", out error);
            ModifyPath = RegistryReader.GetMSIInstalledProductStringValue(productKey, "ModifyPath", out error);
            NoModify = RegistryReader.GetMSIInstalledProductDWordValue(productKey, "NoModify", out error);
            NoRepair = RegistryReader.GetMSIInstalledProductDWordValue(productKey, "NoRepair", out error);
            Publisher = RegistryReader.GetMSIInstalledProductStringValue(productKey, "Publisher", out error);
            Readme = RegistryReader.GetMSIInstalledProductStringValue(productKey, "Readme", out error);
            Size = RegistryReader.GetMSIInstalledProductStringValue(productKey, "Size", out error);
            UninstallString = RegistryReader.GetMSIInstalledProductStringValue(productKey, "UninstallString", out error);
            URLInfoAbout = RegistryReader.GetMSIInstalledProductStringValue(productKey, "URLInfoAbout", out error);
            URLUpdateInfo = RegistryReader.GetMSIInstalledProductStringValue(productKey, "URLUpdateInfo", out error);
            Version = RegistryReader.GetMSIInstalledProductDWordValue(productKey, "Version", out error);
            VersionMajor = RegistryReader.GetMSIInstalledProductDWordValue(productKey, "VersionMajor", out error);
            VersionMinor = RegistryReader.GetMSIInstalledProductDWordValue(productKey, "VersionMinor", out error);
            WindowsInstaller = RegistryReader.GetMSIInstalledProductDWordValue(productKey, "WindowsInstaller", out error);
        }
    }
}
