using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sage.CRE.ComponentIdentification.Helpers;

namespace Sage.CRE.ComponentIdentification.Test
{
    [TestClass]
    public class Test_MSIUninstallReader
    {
        [TestMethod]
        public void RetrieveMSIUninstallInformation()
        {
            var uninstallInfo = new MSIUninstallInformation();
            uninstallInfo.PopulateFromUninstallRegistry("{C08257CE-4608-43FE-AFB9-241E6AD252D1}");

        }
    }
}
