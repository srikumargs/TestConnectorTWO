using System;

namespace Sage.CRE.ComponentIdentification.Helpers
{
    /// <summary>
    /// Base helper to retrieve version information from MSI Uninstall information
    /// </summary>
    public abstract class BaseMSIVersionDetector
    {
        public abstract string ProductCode { get; }

        /// <summary>
        /// Utilized derived product code to retrieve version from uninstall MSI information
        /// </summary>
        /// <returns></returns>
        protected MSIUninstallInformation RetrieveUninstallInformation()
        {
            var uninstallInfo = new MSIUninstallInformation();
            uninstallInfo.PopulateFromUninstallRegistry(ProductCode);
            return uninstallInfo;
        }

        /// <summary>
        /// If installed, return the major/minor version number
        /// Null otherwise
        /// </summary>
        /// <returns></returns>
        public Version InstalledVersion()
        {
            var uninstallInformation = RetrieveUninstallInformation();
            if (null != uninstallInformation)
            {
                if (uninstallInformation.VersionMajor.HasValue && uninstallInformation.VersionMinor.HasValue)
                {
                    return new Version(
                        Convert.ToInt32(uninstallInformation.VersionMajor.Value),
                        Convert.ToInt32(uninstallInformation.VersionMinor.Value));
                }
            }

            return null;
        }
    }
}