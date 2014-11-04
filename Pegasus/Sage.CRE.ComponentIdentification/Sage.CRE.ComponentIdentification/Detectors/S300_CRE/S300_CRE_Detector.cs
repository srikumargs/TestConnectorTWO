using Sage.CRE.ComponentIdentification.Helpers;

namespace Sage.CRE.ComponentIdentification.Detectors.S300_CRE
{
    /// <summary>
    /// Version detector for Sage 300 Construction and Real Estate
    /// </summary>
    public class S300_CRE_Detector : BaseMSIVersionDetector
    {
        /// <summary>
        /// Product Code Name for MSI Uninstall information for S300CRE
        /// </summary>
        public override string ProductCode
        {
            get { return "{AA4E2F21-B871-4DD4-B166-AA0BD8B61C53}"; }
        }
    }
}
