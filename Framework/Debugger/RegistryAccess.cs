using System;
using Microsoft.Win32;

namespace Microsoft.SPOT.Debugger
{
    public static class RegistryAccess
    {
        public readonly static string c_FRAMEWORK_REGISTRY_BASE = @"Software\Microsoft\.NETMicroFramework";

        public readonly static string c_FRAMEWORK_REGISTRY_BASE_64 = @"Software\Wow6432Node\Microsoft\.NETMicroFramework";

        private static RegistryKey GetBaseKey(string relativePath)
        {
            RegistryKey key = null;

            try
            {
                key = Registry.CurrentUser.OpenSubKey(String.Concat(c_FRAMEWORK_REGISTRY_BASE, relativePath), false);
                if ( null != key )
                    return key;

                key = Registry.CurrentUser.OpenSubKey(String.Concat(c_FRAMEWORK_REGISTRY_BASE_64, relativePath), false);
                if ( null != key )
                    return key;

                key = Registry.LocalMachine.OpenSubKey(String.Concat(c_FRAMEWORK_REGISTRY_BASE, relativePath), false);
                if ( null != key )
                    return key;

                key = Registry.LocalMachine.OpenSubKey(String.Concat(c_FRAMEWORK_REGISTRY_BASE_64, relativePath), false);
                if ( null != key )
                    return key;
            }

                // The following exception are not handled now
            catch (ArgumentException /*ex*/)
            {
                // c_FRAMEWORK_REGISTRY_BASE + relativePath is longer than 255! Handle as internal error.
            }
            catch (ObjectDisposedException /*ex*/)
            {
                // The RegistryKey is closed (closed keys cannot be accessed).
            }
            catch
            {
                // Anything else; treat as internal error or report adverse condition to user if necessary
            }

            return null;
        }

        // If the registry base + relativePath is not present or inaccessible, this will return null; if
        // the containing key is accessible, but not the specific subkey desired, we throw an exception.
        public static object GetValue(string relativePath, string keystring)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(String.Concat(c_FRAMEWORK_REGISTRY_BASE, relativePath));
                if ( null != key )
                {
                    return key.GetValue(keystring);
                }
            }

                //  The following exception are not handled now
            catch (ArgumentException /*ex*/)
            {
                // c_FRAMEWORK_REGISTRY_BASE + relativePath is longer than 255! Handle as internal error.
            }
            catch (ObjectDisposedException /*ex*/)
            {
                // The RegistryKey is closed (closed keys cannot be accessed).
            }
            catch
            {
                // Anything else; treat as internal error
            }

            return null;
        }

        // This version will not throw an exception; if the desired key is present and of the correct type, it
        // will return true and the returnValue argument will be set to the desired value. Any other condition
        // will return false, a situation we will report to the user in a later revision of this software.
        public static bool GetIntValue(string relativePath, string keystring, out int returnValue, int defaultValue)
        {
            returnValue = defaultValue;
            RegistryKey key = null;

            try
            {
                key = GetBaseKey(relativePath);
                if ( null != key )
                {
                    returnValue = (int)key.GetValue(keystring);
                    return true;
                }
            }
            catch(InvalidCastException)
            {
                // present in the registry but not the appropriate kind of registry value; probably user error
                // to inform the user
            }
            catch // very unexpected
            {
                //to report adverse condition to user if necessary
            }

            return false;
        }

        // This version will not throw an exception; if the desired key is present and of the correct type, it
        // will return true and the returnValue argument will be set to the desired value. Any other condition
        // will return false, a situation we will report to the user in a later revision of this software.
        public static bool GetBoolValue(string relativePath, string keystring, out bool returnValue, bool defaultValue)
        {
            returnValue = defaultValue;
            int tempInt;

            if ( GetIntValue(relativePath, keystring, out tempInt, 0) )
            {
                returnValue = 0 != tempInt;
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}