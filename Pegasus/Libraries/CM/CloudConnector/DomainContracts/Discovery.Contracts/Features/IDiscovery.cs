using System;
using Sage.Connector.Discovery.Contracts.Payload;

namespace Sage.Connector.Discovery.Contracts.Features
{
    /// <summary>
    /// API to help install and configuration
    /// These APIs are expected to be callable with or without MAF/MEF if needed.
    /// </summary>
    /// <remarks>
    /// Note that Implementation of the plugin is dependent on domain contracts core for the BackOfficeConfigMetadataExportAttribute's inheritance chain.
    /// without that the implementation would be completely isolated.
    /// </remarks>
    public interface IDiscovery
    {
        /// <summary>
        /// Check that the back office software is installed and available to be used by the plug in
        /// </summary>
        /// <returns></returns>
        Boolean IsBackOfficeInstalled();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        PluginInformation GetPluginInformation();
    }
}
