using System;
using System.Collections.Generic;
using System.Text;
using Vestris.ResourceLib;
using System.IO;
using System.Web;
//using NUnit.Framework;
using System.Reflection;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vestris.ResourceLibUnitTests
{
    [TestClass]
    public class MenuResourceTests
    {
        [TestMethod]
        public void TestLoadMenuResources()
        {
            Uri uri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            string uriPath = Path.GetDirectoryName(HttpUtility.UrlDecode(uri.AbsolutePath));
            string filename = Path.Combine(uriPath, @"Binaries\custom.exe");
            using (ResourceInfo ri = new ResourceInfo())
            {
                ri.Load(filename);
                Assert.AreEqual(7, ri[Kernel32.ResourceTypes.RT_MENU].Count);
                foreach (MenuResource rc in ri[Kernel32.ResourceTypes.RT_MENU])
                {
                    Console.WriteLine("MenuResource: {0}, {1}", rc.Name, rc.TypeName);
                    DumpResource.Dump(rc);
                }
                Assert.AreEqual(101, ri[Kernel32.ResourceTypes.RT_MENU][0].Name.Id.ToInt32());
                Assert.AreEqual(102, ri[Kernel32.ResourceTypes.RT_MENU][1].Name.Id.ToInt32());
            }
        }

        [TestMethod]
        public void TestReadWriteMenuMixedResourceBytes()
        {
            Uri uri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            string uriPath = Path.GetDirectoryName(HttpUtility.UrlDecode(uri.AbsolutePath));
            string filename = Path.Combine(uriPath, @"Binaries\custom.exe");
            using (ResourceInfo ri = new ResourceInfo())
            {
                ri.Load(filename);
                foreach (MenuResource rc in ri[Kernel32.ResourceTypes.RT_MENU])
                {
                    Console.WriteLine(rc.Name);
                    GenericResource genericResource = new GenericResource(
                        rc.Type,
                        rc.Name,
                        rc.Language);
                    genericResource.LoadFrom(filename);
                    byte[] data = rc.WriteAndGetBytes();
                    ByteUtils.CompareBytes(genericResource.Data, data);
                }
            }
        }

        [TestMethod]
        public void TestLoadMenuResourcesEx()
        {
            Uri uri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            string uriPath = Path.GetDirectoryName(HttpUtility.UrlDecode(uri.AbsolutePath));
            string filename = Path.Combine(uriPath, @"Binaries\custom.exe");
            using (ResourceInfo ri = new ResourceInfo())
            {
                ri.Load(filename);
                foreach (MenuResource rc in ri[Kernel32.ResourceTypes.RT_MENU])
                {
                    Console.WriteLine("MenuResource: {0}, {1}", rc.Name, rc.TypeName);
                    DumpResource.Dump(rc);
                }
            }
        }

        //[TestMethod]
        //public void TestLoadMixedMenuResources()
        //{
        //    Uri uri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
        //    string uriPath = Path.GetDirectoryName(HttpUtility.UrlDecode(uri.AbsolutePath));
        //    string filename = Path.Combine(uriPath, @"C:\Windows\explorer.exe");
        //    using (ResourceInfo ri = new ResourceInfo())
        //    {
        //        ri.Load(filename);
        //        foreach (MenuResource rc in ri[Kernel32.ResourceTypes.RT_MENU])
        //        {
        //            Console.WriteLine("MenuResource: {0}, {1}", rc.Name, rc.TypeName);
        //            DumpResource.Dump(rc);
        //        }
        //    }
        //}
    }
}
