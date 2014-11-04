using Sage.CRE.ComponentIdentification.Helpers;

namespace Sage.CRE.ComponentIdentification.Detectors.SCA
{
    /// <summary>
    /// Version detection for Sage Construction Anywhere
    /// </summary>
    public class SCA_Detector : BaseMSIVersionDetector
    {
        /// <summary>
        /// Product Code Name for MSI Uninstall information for SCA
        /// </summary>
        public override string ProductCode
        {
            get { return "{837F7E18-DCEE-4DA1-B28B-8BD9110DF163}"; }
        }
    }
}
