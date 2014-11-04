
namespace Sage.Connector.Discovery.Contracts.Payload
{
    /// <summary>
    /// Information that the about the plug in and the back office.
    /// </summary>
    public class PluginInformation
    {
        /// <summary>
        /// ID of the back office this plug in serves
        /// </summary>
        /// <remarks>
        /// SHoudl this be meta data only?B
        /// Returns even if the back office is not installed
        /// </remarks>
        public string BackOfficeId { get; set; }

        /// <summary>
        /// User friendly name of the back office this plug in 
        /// Expected to work even if the back office is not installed
        /// </summary>
        public string BackOfficeName { get; set; }

        /// <summary>
        /// The version of the back office present. IF installed.
        /// This is expected to be empty if the bad office is not installed.
        /// </summary>
        /// <remarks>
        /// Version of the back office installed. If the back office is installed.
        /// Is empty string if the back office is not installed.
        /// </remarks>
        public string BackOfficeVersion { get; set; }

        /// <summary>
        /// Version of the discovery plugin
        /// </summary>
        public string PluginVersion { get; set; }

        /// <summary>
        /// URI used to provide help for the plugin configuration experience
        /// Expected to work even if the back office is not installed
        /// </summary>
        /// <remarks>
        /// Maybe get this from the cloud instead
        /// </remarks>
        public string HelpUri { get; set; }
        
        /// <summary>
        /// X86,AnyCpu etc what 
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// Does the plugin need to run in a user context?
        /// Would be best if none did but S100Contractor for example needs mapped drives
        /// </summary>
        /// <remarks>
        /// Maybe revised. This is not fully cooked. It is to help with the case
        /// of the plugin needing a mapped drive or to run in a user context
        /// Maybe make the plugin responsible for impersonation?
        /// </remarks>
        public bool RunAsUserIsRequired { get; set; }

        //maybe obsolete based on how new UX for configuration works
        /// <summary>
        /// The terminology for the administrator for the plugin
        /// </summary>
        public string LoginAdministratorTerm { get; set; }
    }
}
