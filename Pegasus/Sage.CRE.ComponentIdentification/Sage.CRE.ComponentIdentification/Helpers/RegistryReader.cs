using System;
using System.IO;

using Microsoft.Win32;
using System.Security;

namespace Sage.CRE.ComponentIdentification.Helpers
{
    /// <summary>
    /// Helper for accessing registry values
    /// </summary>
    public static class RegistryReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productKey"></param>
        /// <param name="valueName"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static UInt32? GetMSIInstalledProductDWordValue(Guid productKey, string valueName, out string error)
        {
            string adjustedKey = "{" + productKey.ToString().ToUpper() + "}";
            return GetMSIInstalledProductDWordValue(adjustedKey, valueName, out error);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productKey"></param>
        /// <param name="valueName"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static UInt32? GetMSIInstalledProductDWordValue(string productKey, string valueName, out string error)
        {
            object retVal = GetMSIInstalledProductObjectValue(productKey, valueName, out error);
            if (null != retVal)
                return Convert.ToUInt32(retVal);

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productKey"></param>
        /// <param name="valueName"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static string GetMSIInstalledProductStringValue(Guid productKey, string valueName, out string error)
        {
            string adjustedKey = "{" + productKey.ToString().ToUpper() + "}";
            return GetMSIInstalledProductStringValue(adjustedKey, valueName, out error);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productKey"></param>
        /// <param name="valueName"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static string GetMSIInstalledProductStringValue(string productKey, string valueName, out string error)
        {
            object retVal = GetMSIInstalledProductObjectValue(productKey, valueName, out error);
            if (null != retVal)
                return retVal.ToString();

            return String.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        public static string MSIUninstallBaseKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productKey"></param>
        /// <param name="valueName"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static object GetMSIInstalledProductObjectValue(string productKey, string valueName, out string error)
        {
            string augmentedKey = MSIUninstallBaseKey + @"\" + productKey;
            object retVal = GetMSIBaseObjectValue(RegistryHive.LocalMachine, RegistryView.Registry64,
                                                  augmentedKey, valueName, out error);
            if (null != retVal)
                return retVal;

            retVal = GetMSIBaseObjectValue(RegistryHive.LocalMachine, RegistryView.Registry32,
                                           augmentedKey, valueName, out error);
            if (null != retVal)
                return retVal;

            retVal = GetMSIBaseObjectValue(RegistryHive.CurrentUser, RegistryView.Registry64,
                                           augmentedKey, valueName, out error);

            if (null != retVal)
                return retVal;

            retVal = GetMSIBaseObjectValue(RegistryHive.CurrentUser, RegistryView.Registry32,
                                           augmentedKey, valueName, out error);

            return retVal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseHive"></param>
        /// <param name="viewKind"></param>
        /// <param name="key"></param>
        /// <param name="valueName"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        private static object GetMSIBaseObjectValue(RegistryHive baseHive, RegistryView viewKind, string key, string valueName, out string error)
        {
            error = string.Empty;

            try
            {
                using (RegistryKey regHive = RegistryKey.OpenBaseKey(baseHive, viewKind))
                {
                    using (RegistryKey regKey = regHive.OpenSubKey(key))
                    {
                        if (regKey != null)
                        {
                            return regKey.GetValue(valueName);
                        }
                    }
                }
            }
            catch (ArgumentException)
            {
                error = Resource1.ReadRegistry_ArgumentException;
            }
            catch (UnauthorizedAccessException)
            {
                error = Resource1.ReadRegistry_UnauthorizedAccessException;
            }
            catch (SecurityException)
            {
                error = Resource1.ReadRegistry_SecurityException;
            }
            catch (ObjectDisposedException)
            {
                error = Resource1.ReadRegistry_ObjectDisposedException;
            }
            catch (IOException)
            {
                error = Resource1.ReadRegistry_IOException;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static string GetLocalMachineStringValue(string key, out string error)
        {
            object oGV = GetLocalMachineValue(key, out error);
            return (null == oGV) ? string.Empty : oGV.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static object GetLocalMachineValue(string key, out string error)
        {
            error = string.Empty;

            try
            {
                return Registry.LocalMachine.GetValue(key);
            }
            catch (SecurityException)
            {
                error = Resource1.ReadRegistry_SecurityException;
            }
            catch (ObjectDisposedException)
            {
                error = Resource1.ReadRegistry_ArgumentException;
            }
            catch (IOException)
            {
                error = Resource1.ReadRegistry_IOException;
            }
            catch (UnauthorizedAccessException)
            {
                error = Resource1.ReadRegistry_UnauthorizedAccessException;
            }
            catch (Exception)
            {
                error = Resource1.ReadRegistry_Exception;
            }

            return null;
        }

        /// <summary>
        /// Access a string value given a key and value.  Note that certain
        /// registry key values may have restricted read permission protections.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static UInt32? GetLongValue(string key, string value, out string error)
        {
            object oGV = GetValue(key, value, out error);
            if (null != oGV)
                return Convert.ToUInt32(oGV);
            return null;
        }

        /// <summary>
        /// Access a string value given a key and value.  Note that certain
        /// registry key values may have restricted read permission protections.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static string GetStringValue(string key, string value, out string error)
        {
            object oGV = GetValue(key, value, out error);
            return (null == oGV) ? string.Empty : oGV.ToString();
        }

        /// <summary>
        /// Access an object value given a key and value.  Note that certain
        /// registry key values may have restricted read permission protections.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static object GetValue(string key, string value, out string error)
        {
            error = string.Empty;

            try
            {
                return Registry.GetValue(key, value, null);
            }
            catch (SecurityException)
            {
                error = Resource1.ReadRegistry_SecurityException;
            }
            catch (IOException)
            {
                error = Resource1.ReadRegistry_IOException;
            }
            catch (ArgumentException)
            {
                error = Resource1.ReadRegistry_ArgumentException;
            }
            catch (Exception )
            {
                error = Resource1.ReadRegistry_Exception;
            }

            return null;
        }
    }
}
