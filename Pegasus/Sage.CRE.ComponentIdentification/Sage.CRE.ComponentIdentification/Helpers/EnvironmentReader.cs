using System;
using System.Security;

namespace Sage.CRE.ComponentIdentification.Helpers
{
    public static class EnvironmentReader
    {
        /// <summary>
        /// Returns the CLR version
        /// </summary>
        /// <returns></returns>
        public static Version GetCommonLanguageRuntimeVersion()
        {
            return Environment.Version;
        }

        /// <summary>
        /// Whether the current OS is 64-bit
        /// </summary>
        /// <returns></returns>
        public static Boolean Is64Bit()
        {
            return Environment.Is64BitOperatingSystem;
        }

        /// <summary>
        /// Returns the current machine name
        /// </summary>
        /// <returns></returns>
        public static string MachineName()
        {
            return Environment.MachineName;
        }

        /// <summary>
        /// Returns attributes of the current operating system
        /// </summary>
        /// <returns></returns>
        public static OperatingSystem OperatingSystemVersion()
        {
            return Environment.OSVersion;
        }

        /// <summary>
        /// Returns the number of processor of the current machine
        /// </summary>
        /// <returns></returns>
        public static int ProcessorCount()
        {
            return Environment.ProcessorCount;
        }

        public static String GetEnvironmentVariableValue(string key)
        {
            try
            {
                return Environment.GetEnvironmentVariable(key);
            }
            catch (ArgumentNullException)
            {
                
            }
            catch (SecurityException)
            {
                
            }

            return string.Empty;
        }
    }
}
