using System;
using System.Diagnostics;
using System.IO;

namespace Sage.CRE.ComponentIdentification.Helpers
{
    public static class FileReader
    {
        public static bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public static bool DirectoryExists(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }

        public static FileVersionInfo GetFileVersionInfo(string filePath)
        {
            try
            {
                return FileVersionInfo.GetVersionInfo(filePath);
            }
            catch (FileNotFoundException)
            {
            }
            catch (Exception)
            {
            }
            return null;
        }
    }
}
