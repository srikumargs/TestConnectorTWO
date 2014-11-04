using System;
using System.ComponentModel.Composition;
using Sage.Connector.Discovery.Contracts.BackOffice;
using Sage.Connector.Discovery.Contracts.BackOffice.Metadata;
using Sage.Connector.Discovery.Contracts.Data;
using Sage.Connector.DomainContracts.BackOffice;

namespace Sage.Connector.MockPlugin.Discovery
{
    /// <summary>
    /// Methods used by the connector and install systems to learn about the plugin.
    /// It is expected that this dll does not have external dependencies beyond .net and the 
    /// connector system. This dll needs to return reasonable results when the back office is not installed.
    /// This is the primary means the connector uses to know if a given back office is a candidate for being configured.
    /// The setup process will end up calling every discovery plug we install to know what is available to be installed.
    /// </summary>
    [Export(typeof(IDiscovery))]
    [BackOfficeConfigMetadataExport("Mock", "Mock Back Office Plugin", "AnyCPU")]
    class Discovery : IDiscovery
    {
        /// <summary>
        /// Return true if back office is installed and can be called by the plugin
        /// </summary>
        /// <param name="sessionContext"></param>
        /// <returns></returns>
        public Boolean IsBackOfficeInstalled(ISessionContext sessionContext)
        {
            //For real plugin check for the existence of the back office.
            return true ;
        }

        /// <summary>
        /// Get information about this plugin
        /// </summary>
        /// <param name="sessionContext"></param>
        /// <returns></returns>
        public PluginInformation GetPluginInformation(ISessionContext sessionContext)
        {
            PluginInformation plugInfo = new PluginInformation();

            plugInfo.BackOfficeId = "Mock";
            plugInfo.BackOfficeName = "Mock Back Office Product";

            //ProductId, ProductVersion and ComponentBaseName for use by auto update system
            //AutoUpdateProductVersion is expected to be the same as  plugInfo.BackOfficeVersion other then in exceptional situations.
            plugInfo.AutoUpdateProductId = "Sage.US.NA.SageDataCloud.Mock.Plugin";
            
            //Note that this version must be present on the AutoUpdate site and the back office plugin package must be associated with it for it to be found and update.
            //When a new version of the back office product is pushed out the AU site must be updated if the resulting AutoUpdateProductVersion is not already on the AU site.
            //This version must change when there is a breaking change in a new version of the back office product, to prevent breaking the older versions in the field.
            //This field is allowed to be null/empty when the back office is not installed.
            plugInfo.AutoUpdateProductVersion = "1.0";

            //Required to be of the form "BackOfficePugin.<BackOfficeId>."
            plugInfo.AutoUpdateComponentBaseName = "BackOfficePlugin.Mock.";

            //Get this from the back office if present for a real plugin, expected to be empty/null if back office is not present
            plugInfo.BackOfficeVersion = "0.1";
            
            //Get this from assembly, a real plugin, should be versioned with the overall back office plugin set.
            plugInfo.PluginVersion = "0.1";
            
            plugInfo.RunAsUserIsRequired = false;
            plugInfo.Platform = "X86";

            
            //Deprecated - Fields below here are deprecated and will eventually be removed.

            //Product ID for the plugin for your back office. This is used to get the plugins from Sage Auto Update.
            //This is just a made up ID for the mock.
            //plugInfo.BackOfficePluginProductId = "Sage.US.CS.Connector.Mock.Standard

            //Help system is not using this.
            //plugInfo.HelpUri = "http://na.sage.com/us";

            //may go away based on the new setup UX. once in place.
            //plugInfo.LoginAdministratorTerm = "Mock Administrator";

            return plugInfo;
        }
    }
}
