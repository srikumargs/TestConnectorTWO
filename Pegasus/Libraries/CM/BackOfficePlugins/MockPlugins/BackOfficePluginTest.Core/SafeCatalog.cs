using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BackOfficePluginTest.Core
{

    /*
     *  Example usage:
     * 
     *  var safeCatalog = new SafeDirectoryCatalog(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
     *  CompositionContainer Container = new CompositionContainer(safeCatalog.Catalog);
     * 
     */

    /// <summary>
    /// Safe directory catalog that filters out unloadable assemblies during the enumertion process
    /// vs. the composition process.
    /// </summary>
    public class SafeDirectoryCatalog : ComposablePartCatalog
    {
        #region Private Members

        private readonly AggregateCatalog _catalog = new AggregateCatalog();
        private readonly List<string> _loadedFiles = new List<string>();
        private readonly List<string> _unloadedFiles = new List<string>();
        private readonly string _path;

        #endregion

        #region Constructor

        /// <summary>
        /// Public constructor for the class.
        /// </summary>
        /// <param name="directory">The directory path to enumerate files for.</param>
        public SafeDirectoryCatalog(string directory)
        {
            if (String.IsNullOrEmpty(directory)) throw new ArgumentNullException("directory");

            _path = directory;

            foreach (var file in Directory.EnumerateFiles(_path, "*.dll", SearchOption.TopDirectoryOnly).ToList())
            {
                try
                {
                    var catalog = new AssemblyCatalog(file);

                    /* This is used to force a type load exception now vs. during the composition stage */
                    if (catalog.Parts.ToList().FirstOrDefault() != null)
                    {
                        _catalog.Catalogs.Add(catalog);
                        _loadedFiles.Add(file);
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    _unloadedFiles.Add(file);
                }
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the part definitions that are contained in the catalog.
        /// </summary>
        public override IQueryable<ComposablePartDefinition> Parts
        {
            get
            {
                return _catalog.Parts;
            }
        }

        /// <summary>
        /// Returns the path that was used to build the catalog from.
        /// </summary>
        public string Path
        {
            get
            {
                return _path;
            }
        }

        /// <summary>
        /// Returns the aggregate catalog.
        /// </summary>
        public AggregateCatalog Catalog
        {
            get
            {
                return _catalog;
            }
        }

        /// <summary>
        /// Returns the list of loaded files.
        /// </summary>
        public List<string> LoadedFiles
        {
            get
            {
                return _loadedFiles;
            }
        }

        /// <summary>
        /// Returns the list of unloaded files.
        /// </summary>
        public List<string> UnloadedFiles
        {
            get
            {
                return _unloadedFiles;
            }
        }

        #endregion
    }



}
