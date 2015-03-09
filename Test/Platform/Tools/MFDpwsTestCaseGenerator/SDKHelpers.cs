using System;
using System.Configuration;
using System.IO;
using System.Collections.Generic;
using Microsoft.Win32;

namespace MFDpwsTestCaseGenerator
{
    public interface ILocator
    {
        DirectoryInfo Tools { get; }
        DirectoryInfo Assemblies { get; }
        DirectoryInfo TestAssemblies { get; }
        FileInfo GetAssemblyFileInfo(string assemblyName);
    }

    public class LocatorFactory
    {
        public static bool IsDevEnvironment
        {
            get
            {
                string sporoot = Environment.GetEnvironmentVariable("sporoot", EnvironmentVariableTarget.Process);

                return (!string.IsNullOrEmpty(sporoot));
            }
        }

        public static bool IsSDKEnvironment
        {
            get { return !IsDevEnvironment; }
        }

        public static ILocator GetLocator(string buildTreeRoot)
        {
            if (IsDevEnvironment)
            {
                return new DevLocator(buildTreeRoot);
            }
            else
            {
                return new SDKLocator();
            }
        }
    }

    class SDKLocator : ILocator
    {
        private DirectoryInfo m_InstallRoot;

        private List<DirectoryInfo> m_AssemblyFoldersEx = new List<DirectoryInfo>();

        public SDKLocator()
        {
            AppSettingsReader reader = new AppSettingsReader();

            RegistryKey mfKey = Registry.LocalMachine.OpenSubKey((string)reader.GetValue("SDKRegKey", typeof(string)));

            string installRoot = (string)mfKey.GetValue(
                (string)reader.GetValue("SDKLocatorValue", typeof(string)));

            if(string.IsNullOrEmpty(installRoot))
            {
                throw new InvalidDataException("Unable to find SDK Install Root in registry");
            }

            if(!installRoot.EndsWith("\\"))
            {
                installRoot += "\\";
            }

            m_InstallRoot = new DirectoryInfo(installRoot);

            if (!m_InstallRoot.Exists)
            {
                throw new InvalidDataException("Unable to find SDK Install Root in registry");
            }

            RegistryKey assemblyFoldersExKey = mfKey.OpenSubKey((string)reader.GetValue("SDKAssemblyFoldersEx", typeof(string)));

            foreach (string subkey in assemblyFoldersExKey.GetSubKeyNames())
            {
                string assemblyFolder = (string)assemblyFoldersExKey.OpenSubKey(subkey).GetValue(null);

                if (!assemblyFolder.EndsWith("\\"))
                {
                    assemblyFolder += "\\";
                }

                var di = new DirectoryInfo(assemblyFolder);

                if (di.Exists)
                {
                    m_AssemblyFoldersEx.Add(di);
                }
            }
        }

        public DirectoryInfo Tools { get { return new DirectoryInfo(m_InstallRoot.FullName + "Tools\\"); } }
        public DirectoryInfo Assemblies { get { return new DirectoryInfo(m_InstallRoot.FullName + "Assemblies\\"); } }
        public DirectoryInfo TestAssemblies { get { return null; } }

        public FileInfo GetAssemblyFileInfo(string assemblyName)
        {
            if (!assemblyName.ToLower().EndsWith(".dll"))
            {
                assemblyName += ".dll";
            }

            if (File.Exists(Assemblies + assemblyName))
            {
                return new FileInfo(Assemblies.FullName + assemblyName);
            }

            foreach (DirectoryInfo di in m_AssemblyFoldersEx)
            {
                if (File.Exists(di.FullName + assemblyName))
                {
                    return new FileInfo(di.FullName + assemblyName);
                }
            }

            return null;
        }
    }

    public class DevLocator : ILocator
    {
        private string m_buildTreeRoot;

        public DevLocator(string buildTreeRoot)
        {
            m_buildTreeRoot = buildTreeRoot;
        }

        #region Locator Members

        public DirectoryInfo Tools
        {
            get { return new DirectoryInfo(m_buildTreeRoot + "\\SERVER\\dll\\"); }
        }

        public DirectoryInfo Assemblies
        {
            get { return new DirectoryInfo(m_buildTreeRoot + "\\CLIENT\\dll\\"); }
        }

        public DirectoryInfo TestAssemblies
        {
            get { return new DirectoryInfo(m_buildTreeRoot + "\\TEST\\CLIENT\\dll\\"); }
        }

        public FileInfo GetAssemblyFileInfo(string assemblyName)
        {
            if (!assemblyName.ToLower().EndsWith(".dll"))
            {
                assemblyName += ".dll";
            }

            if (File.Exists(Assemblies + assemblyName))
            {
                return new FileInfo(Assemblies.FullName + assemblyName);
            }


            if (File.Exists(TestAssemblies + assemblyName))
            {
                return new FileInfo(TestAssemblies.FullName + assemblyName);
            }

            return null;
        }

        #endregion
    }

}
