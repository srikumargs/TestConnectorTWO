using System;
using System.Diagnostics;
using System.Text;
using Sage.PInvoke;

namespace Sage.CRE.CloudConnector.InstalledProductInfo
{
    /// <summary>
    /// Basic information about product
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Is product installed according to Windows Installer?
        /// </summary>
        public static Boolean IsInstalled
        {
            get
            {
                Boolean result = false;

                Trace.WriteLine(String.Format("PRODUCT_CODE: {0}", PRODUCT_CODE));
                Msi.INSTALLSTATE installState = (Msi.INSTALLSTATE)Msi.MsiQueryProductState(PRODUCT_CODE);
                if (installState == Msi.INSTALLSTATE.INSTALLSTATE_DEFAULT || installState == Msi.INSTALLSTATE.INSTALLSTATE_LOCAL)
                {
                    Trace.WriteLine(String.Format("The Sage Connector is installed.  MsiQueryProductState returned {0}", installState.ToString()));
                    result = true;
                }
                else
                {
                    Trace.WriteLine(String.Format("The Sage Connector is not installed.  MsiQueryProductState returned {0}", installState.ToString()));
                    result = false;
                }

                return result;
            }
        }

        /// <summary>
        /// What is the product version according to Windows Installer?
        /// </summary>
        public static Version InstalledProductVersion
        {
            get
            {
                Version result = new Version(0, 0, 0, 0);

                if (IsInstalled)
                {
                    String version = GetProductInfo(PRODUCT_CODE, Msi.INSTALLPROPERTY_VERSIONSTRING);
                    result = new Version(version);
                }

                return result;
            }
        }

        /// <summary>
        /// What is the product name according to Windows Installer?
        /// </summary>
        public static String InstalledProductName
        {
            get
            {
                String result = String.Empty;

                if (IsInstalled)
                {
                    result = GetProductInfo(PRODUCT_CODE, Msi.INSTALLPROPERTY_INSTALLEDPRODUCTNAME);
                }

                return result;
            }
        }

        private static String GetProductInfo(String ProductCode, String propertyName)
        {
            Int32 size = 1;
            StringBuilder result = new StringBuilder(size);
            try
            {
                Int32 msiResult = Msi.MsiGetProductInfo(ProductCode, propertyName, result, ref size);
                if (msiResult == Msi.ERROR_MORE_DATA)
                {
                    size++;
                    result.EnsureCapacity(size);
                    msiResult = Msi.MsiGetProductInfo(ProductCode, propertyName, result, ref size);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return result.ToString();
        }

        private static String PRODUCT_CODE
        { get { return String.Format("{{{0}}}", _productCode); } }

        /// <summary>
        /// The product code for the install
        /// </summary>
        /// <remarks>This should be the same guid as the guid in SageConnector.wxs -> Product element -> Id attribute.</remarks>
        private const String _productCode = "-=TAG_ProductCode=-";
    }
}
