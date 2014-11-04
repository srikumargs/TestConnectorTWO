using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Vestris.ResourceLib;
using System.IO;
using System.Diagnostics;

namespace ResourceLibConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string filename = args[0];
            Debug.Assert(File.Exists(filename));

            VersionResource versionResource = new VersionResource();
            //versionResource.Language = ResourceUtil.USENGLISHLANGID;
            versionResource.Language = ResourceUtil.NEUTRALLANGID;
            versionResource.LoadFrom(filename);
            //DumpResource.Dump(versionResource);

            versionResource.FileVersion = args[2];
            versionResource.ProductVersion = args[2];
            versionResource.FileFlags = (uint)(Vestris.ResourceLib.Winver.FileFlags.VS_FF_PRIVATEBUILD | Vestris.ResourceLib.Winver.FileFlags.VS_FF_SPECIALBUILD);

            StringFileInfo stringFileInfo = (StringFileInfo)versionResource["StringFileInfo"];
            stringFileInfo["FileVersion"] = args[2];
            stringFileInfo["ProductVersion"] = args[2];
            if (!String.IsNullOrEmpty(args[3]))
            {
                stringFileInfo["FileDescription"] = String.Format("{0}({1})", stringFileInfo["FileDescription"].Replace('\0', ' '), args[3]);
            }




            ////Additional information that should be displayed for diagnostic purposes.
            //stringFileInfo["Comments"] = "Comments";

            ////Company that produced the file?for example, "Microsoft Corporation" or "Standard Microsystems Corporation, Inc." This string is required.
            //stringFileInfo["CompanyName"] = "CompanyName";

            ////File description to be presented to users. This string may be displayed in a list box when the user is choosing files to install?for example, "Keyboard Driver for AT-Style Keyboards". This string is required.
            //stringFileInfo["FileDescription"] = "FileDescription";

            ////Version number of the file?for example, "3.10" or "5.00.RC2". This string is required.
            //stringFileInfo["FileVersion"] = "FileVersion";


            ////Internal name of the file, if one exists?for example, a module name if the file is a dynamic-link library. If the file has no internal name, this string should be the original filename, without extension. This string is required.
            //stringFileInfo["InternalName"] = "InternalName";

            ////Copyright notices that apply to the file. This should include the full text of all notices, legal symbols, copyright dates, and so on. This string is optional.
            stringFileInfo["LegalCopyright"] = args[5];

            ////Trademarks and registered trademarks that apply to the file. This should include the full text of all notices, legal symbols, trademark numbers, and so on. This string is optional.
            //stringFileInfo["LegalTrademarks"] = "LegalTrademarks";

            //Original name of the file, not including a path. This information enables an application to determine whether a file has been renamed by a user. The format of the name depends on the file system for which the file was created. This string is required.
            //stringFileInfo["OriginalFilename"] = args[3];

            ////Information about a private version of the file?for example, "Built by TESTER1 on \TESTBED". This string should be present only if VS_FF_PRIVATEBUILD is specified in the fileflags parameter of the root block.
            //stringFileInfo["PrivateBuild"] = "PrivateBuild";

            ////Name of the product with which the file is distributed. This string is required.
            stringFileInfo["ProductName"] = args[4];

            ////Version of the product with which the file is distributed?for example, "3.10" or "5.00.RC2". This string is required.
            //stringFileInfo["ProductVersion"] = "ProductVersion";

            ////Text that indicates how this version of the file differs from the standard version?for example, "Private build for TESTER1 solving mouse problems on M250 and M250E computers". This string should be present only if VS_FF_SPECIALBUILD is specified in the fileflags parameter of the root block.
            //stringFileInfo["SpecialBuild"] = "SpecialBuild";

            //stringFileInfo["Comments"] = args[3];
            //stringFileInfo["NewValue"] = string.Format("{0}\0", Guid.NewGuid());

            string targetFilename = args[1];
            File.Copy(filename, targetFilename, true);
            File.SetAttributes(targetFilename, File.GetAttributes(targetFilename) & ~FileAttributes.ReadOnly);
            Console.WriteLine(targetFilename);
            versionResource.SaveTo(targetFilename);
        }
    }
}
