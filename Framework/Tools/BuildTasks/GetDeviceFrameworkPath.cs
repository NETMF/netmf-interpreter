#region Using directives

using System;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Win32;
using System.Diagnostics;

#endregion

namespace Microsoft.SPOT.Tasks
{
    public sealed class GetDeviceFrameworkPaths : Task
    {
        internal class RegistryValues
        {
            public const string Default = "";
            public const string FrameworkRegistryBase = @"Software\Microsoft\.NETMicroFramework";
            public const string FrameworkRegistryBase32 = @"Software\Wow6432Node\Microsoft\.NETMicroFramework";
            public const string InstallRoot = "InstallRoot";
        }

        public class Directories
        {
            public const string Tools = "Tools";
            public const string Assemblies = "Assemblies";
        }

        class RegistryKeys
        {
            public const string AssemblyFoldersEx = "AssemblyFoldersEx";
        }

        internal class MfSdkInternalError : Exception
        {
            public MfSdkInternalError(string errMsg)
            : base ("MF SDK internal error: " + errMsg + "; this MF SDK installation is not valid.")
            {
            }
        }

        #region Properties

        private string runtimeVersion = null;
        private string toolsPath;
        private string frameworkAssembliesPath, frameworkAssembliesPathLE, frameworkAssembliesPathBE;
        private string installRoot;

        public string RuntimeVersion
        {
            get { return runtimeVersion; }
            set { runtimeVersion = value; }
        }

        [Output]
        public string FrameworkAssembliesPath
        {
            get { return frameworkAssembliesPath; }
            set { frameworkAssembliesPath = value; }
        }

        [Output]
        public string FrameworkAssembliesPathLE
        {
            get { return frameworkAssembliesPathLE; }
            set { frameworkAssembliesPathLE = value; }
        }

        [Output]
        public string FrameworkAssembliesPathBE
        {
            get { return frameworkAssembliesPathBE; }
            set { frameworkAssembliesPathBE = value; }
        }

        [Output]
        public string ToolsPath
        {
            get { return toolsPath; }
            set { toolsPath = value; }
        }

        [Output]
        public string InstallRoot
        {
            get { return installRoot; }
            set { installRoot = value; }
        }

        #endregion

        #region ITask Members

        public override bool Execute()
        {
            try
            {
                if (!string.IsNullOrEmpty(runtimeVersion) && !runtimeVersion.StartsWith("v"))
                    throw new MfSdkInternalError(String.Format("runtimeVersion string \"{0}\" is malformed", runtimeVersion));

                string build_root = Environment.GetEnvironmentVariable("BUILD_ROOT");

                if (!string.IsNullOrEmpty(runtimeVersion))
                {
                    Version ver = Version.Parse(runtimeVersion.TrimStart('v'));

                    installRoot = GetDeviceFrameworkValue("v" + ver.ToString(2), null, RegistryValues.InstallRoot);
                }

                if (installRoot == null || !Directory.Exists(installRoot) || 0 == string.Compare(Path.GetDirectoryName(Path.GetDirectoryName(installRoot)), Path.GetDirectoryName(build_root), true))
                {
                    // If there is no install-root value, perhaps it's because this is an internal development build.
                    // The SPOCLIENT environment variable should name a valid directory, and BUILD_TREE_CLIENT & BUILD_TREE_SERVER as well.
                    // Otherwise, it really is a broken installation

                    string spoclient = Environment.GetEnvironmentVariable(@"SPOCLIENT");
                    string build_tree_client = Environment.GetEnvironmentVariable(@"BUILD_TREE_CLIENT");
                    string build_tree_server = Environment.GetEnvironmentVariable(@"BUILD_TREE_SERVER");

                    if (String.IsNullOrEmpty(spoclient) || String.IsNullOrEmpty(build_tree_client) || String.IsNullOrEmpty(build_tree_server))
                    {
                        throw new MfSdkInternalError("The MF SDK does not appear to be available on this machine");
                    }

                    installRoot = build_tree_client;
                    toolsPath = Path.Combine(build_tree_server, @"DLL");
                    frameworkAssembliesPath = Path.Combine(build_tree_client, @"DLL");

                    frameworkAssembliesPathLE = frameworkAssembliesPath;
                    frameworkAssembliesPathBE = frameworkAssembliesPath;
                }
                else
                {
                    toolsPath = Path.Combine(installRoot, Directories.Tools);

                    // Check the AssemblyFolder subkey; this is used only internally to support the mfpseudoinstaller style of running MF SDK;
                    // not needed by the PK or by a real, installed, MF SDK. Not externally documented or supported.
                    frameworkAssembliesPath = GetDeviceFrameworkValue(runtimeVersion, "AssemblyFolder", RegistryValues.Default);
                    if (!string.IsNullOrEmpty(frameworkAssembliesPath))
                    {
                        if (!Directory.Exists(frameworkAssembliesPath))
                        {
                            Log.LogWarning("The directory \"{0}\" named by the AssemblyFolder key does not exist", frameworkAssembliesPath);
                            frameworkAssembliesPath = null;
                        }
                    }

                    frameworkAssembliesPath = Path.Combine(installRoot, Directories.Assemblies);

                    if (Directory.Exists(Path.Combine(frameworkAssembliesPath, "be")))
                    {
                        frameworkAssembliesPathLE = Path.Combine(frameworkAssembliesPath, "le");
                        frameworkAssembliesPathBE = Path.Combine(frameworkAssembliesPath, "be");
                        frameworkAssembliesPath = frameworkAssembliesPathLE;
                    }
                    else
                    {
                        frameworkAssembliesPathLE = frameworkAssembliesPath;
                        frameworkAssembliesPathBE = frameworkAssembliesPath;
                    }


                    FileInfo mscorlibLEInfo = new FileInfo(Path.Combine(frameworkAssembliesPathLE, @"mscorlib.dll"));
                    FileInfo mscorlibBEInfo = new FileInfo(Path.Combine(frameworkAssembliesPathBE, @"mscorlib.dll"));
                    FileInfo metadataProcessorInfo = new FileInfo(Path.Combine(toolsPath, @"MetadataProcessor.exe"));

                    if (!Directory.Exists(toolsPath) || !metadataProcessorInfo.Exists)
                        throw new MfSdkInternalError(String.Format("The directory \"{0}\" that should contain the MF SDK toolchain does not exist or is not fully installed", toolsPath));

                    if (!Directory.Exists(frameworkAssembliesPathLE) || !mscorlibLEInfo.Exists)
                        throw new MfSdkInternalError(String.Format("The directory \"{0}\" that should contain the MF SDK assemblies does not exist or is not fully installed", frameworkAssembliesPathLE));

                    if (!Directory.Exists(frameworkAssembliesPathBE) || !mscorlibBEInfo.Exists)
                        throw new MfSdkInternalError(String.Format("The directory \"{0}\" that should contain the MF SDK assemblies does not exist or is not fully installed", frameworkAssembliesPathBE));
                }

                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    Log.LogErrorFromException(ex);
                }
                catch{}
            }
            return false;
        }

        #endregion

        // This method by design does not catch any exceptions; it is intended only to be called by
        // public methods of this class, which ought necessarily to be handling their exceptions anyway.
        private static string GetDeviceFrameworkValue(string runtimeVersion, string subkey, string valueName)
        {
            // Look in HKCU first for the value
            string valueStr = GetDeviceFrameworkValue(Registry.CurrentUser, runtimeVersion, subkey, valueName);
            if ( valueStr != null )
                return valueStr;

            // Not there? try HKLM
            return GetDeviceFrameworkValue(Registry.LocalMachine, runtimeVersion, subkey, valueName);
        }

        // This private method by design does not catch any exceptions; it is intended only to be called by
        // public methods of this class, which ought necessarily to be handling their exceptions anyway.
        private static string GetDeviceFrameworkValue(RegistryKey topLevelKey, string runtimeVersion, string subkey, string valueName)
        {
            object value = null;

            RegistryKey key = OpenDeviceFrameworkKey(topLevelKey, runtimeVersion, subkey);
            if (key != null && (value = key.GetValue(valueName)) != null)
            {
                if (value is String)
                {
                    return value as String;
                }
                else
                {
                    throw new MfSdkInternalError(String.Format("The value of \"{0}\" at key \"{1}\" was not of type string", valueName, key.Name));
                }
            }
            return null;
        }

        public static string GetDeviceFrameworkValueOrThrow(string runtimeVersion, string subkey, string valueName)
        {
            string value = GetDeviceFrameworkValue(runtimeVersion, subkey, valueName);
            if ( value == null )
            {
                throw new MfSdkInternalError(
                    runtimeVersion == null
                        ? String.Format("\"{0}\" registry value not present at key \"{1}\" in the most recent version", valueName, subkey)
                        : String.Format("\"{0}\" registry value not present at key \"{1}\" for runtime version {2}", valueName, subkey, runtimeVersion)
                        );
            }
            return value;
        }

        public static RegistryKey OpenDeviceFrameworkKey(RegistryKey topLevelKey, string runtimeVersion, string subkey)
        {
            RegistryKey retVal = OpenDeviceFrameworkKey(topLevelKey, runtimeVersion, subkey, false);

            if (retVal == null)
            {
                retVal = OpenDeviceFrameworkKey(topLevelKey, runtimeVersion, subkey, true);
            }

            return retVal;
        }

        internal static RegistryKey OpenDeviceFrameworkKey(RegistryKey topLevelKey, string runtimeVersion, string subkey, bool fWow64)
        {
            if (runtimeVersion == null)
            {
                // attempt to get the 'Product' version of the current executing assembly first;
                // by convention we use the InformationalVersion attribute as the Product version
                System.Reflection.AssemblyInformationalVersionAttribute[] myInformationalVersionAttributes
                    = (System.Reflection.AssemblyInformationalVersionAttribute[]) System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false);

                if (null != myInformationalVersionAttributes && myInformationalVersionAttributes.Length > 0)
                {
                    string[] verParts = myInformationalVersionAttributes[0].InformationalVersion.Split(new Char[]{'.'});
                    if ( verParts == null || verParts.Length == 0)
                        runtimeVersion = "v4.3";
                    else if (verParts.Length == 1)
                        runtimeVersion = String.Format("v{0}.0", verParts[0]);
                    else
                        runtimeVersion = String.Format("v{0}.{1}", verParts[0], verParts[1]);
                }
            }

            if (runtimeVersion == null)
            {
                // Fall back to using the version of this individual assembly if the product-wide version is not present
                Version myVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                runtimeVersion = String.Format("v{0}.{1}", myVersion.Major, myVersion.Minor);
            }

            //call dispose on all these open keys?

            string frameworkRegistryBase = fWow64 ? RegistryValues.FrameworkRegistryBase32 : RegistryValues.FrameworkRegistryBase;

            // Find registry location
            RegistryKey hiveroot = topLevelKey.OpenSubKey(frameworkRegistryBase);

            if (hiveroot == null)
                return null;

            // Find latest version
            string version = "v0";
            RegistryKey vKey = null;
            RegistryKey key = null;

            foreach (string subkeyname in hiveroot.GetSubKeyNames())
            {
                if (runtimeVersion != null && subkeyname.Length < runtimeVersion.Length)
                    continue;
                if (runtimeVersion == null || subkeyname.Substring(0, runtimeVersion.Length) == runtimeVersion)
                {
                    if ((key = hiveroot.OpenSubKey(subkeyname)) == null)
                        continue;

                    if (subkey != null && subkey.Length > 0)
                    {
                        if ((key = key.OpenSubKey(subkey)) == null)
                            continue;
                    }

                    if (key != null && String.Compare(subkeyname, version) > 0)
                    {
                        version = subkeyname;
                        vKey = key;
                    }
                }
            }
            return vKey;
        }
    }
}
