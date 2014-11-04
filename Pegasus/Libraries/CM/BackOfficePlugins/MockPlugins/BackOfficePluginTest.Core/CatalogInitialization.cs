using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BackOfficePluginTest.Core
{
    [TestClass]
    public abstract class CatalogInitialization
    {
        /// <summary>
        /// Track whether Dispose has been called. 
        /// </summary>
        protected bool Disposed = false;

        /// <summary>
        /// The <see cref="CompositionContainer"/>
        /// </summary>
        protected CompositionContainer Container;

        [TestInitialize]
        public virtual void Initialize()
        {
            var safeCatalog = new SafeDirectoryCatalog(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            Container = new CompositionContainer(safeCatalog.Catalog);

            //Fill the imports of this object
            try
            {
                Container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                Debug.Print(compositionException.ToString());
            }

        }

        /// <summary>
        /// Dispose properly of this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method. 
            // Therefore, you should call GC.SupressFinalize to 
            // take this object off the finalization queue 
            // and prevent finalization code for this object 
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios. 
        /// If disposing equals true, the method has been called directly 
        /// or indirectly by a user's code. Managed and unmanaged resources 
        /// can be disposed. 
        /// If disposing equals false, the method has been called by the 
        /// runtime from inside the finalizer and you should not reference 
        /// other objects. Only unmanaged resources can be disposed. 
        /// </summary>
        /// <param name="disposing">When false, the method has been called by the 
        /// runtime from inside the finalizer and you should not reference 
        /// other objects. Only unmanaged resources can be disposed. 
        ///  </param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if (!Disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if (disposing)
                {
                    // Dispose managed resources.
                    Container.Dispose();
                }

                // Note disposing has been done.
                Disposed = true;

            }
        }

    }
}
