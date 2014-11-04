using Sage.CRE.ComponentIdentification.Helpers;

namespace Sage.CRE.ComponentIdentification.Detectors.S100_Contractor
{
    /// <summary>
    /// Version detector for Sage 100 Contractor
    /// </summary>
    public class S100_Contractor_Detector : BaseMSIVersionDetector
    {
        /// <summary>
        /// Product Code Name for MSI Uninstall information for Sage 100 Contractor
        /// </summary>
        public override string ProductCode
        {
            get { return "{9E1998F6-E5F2-4DAC-BD90-ED7799CC7184}"; }
        }
    }
}
