//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.Win32;
using Microsoft.SPOT.Tasks;

namespace Microsoft.SPOT.Debugger
{
    public class PlatformInfo
    {
        public class PortDefinition_PeristableEmulator : PortDefinition
        {
            Emulator m_emulator;

            public PortDefinition_PeristableEmulator(Emulator emulator)
                : base(emulator.name, null)
            {
                m_emulator = emulator;
            }

            public override string PersistName
            {
                get
                {
                    return m_emulator.persistableName;
                }
            }

            public override Stream Open()
            {
                throw new NotSupportedException();
            }

            public override string Port
            {
                get { throw new NotSupportedException(); }
            }

            public override Stream CreateStream()
            {
                throw new NotSupportedException();
            }
        }

        public class Emulator
        {
            public string persistableName;
            public string name;
            public string application;
            public string additionalOptions;
            public string config;
            public bool legacyCommandLine;

            public Emulator Clone()
            {
                return (Emulator)this.MemberwiseClone();
            }
        }

        class RegistryKeys
        {
            public const string AssemblyFoldersEx = "AssemblyFoldersEx";
            public const string Emulators = "Emulators";
        }

        class RegistryValues
        {
            public const string Default = "";
            public const string EmulatorConfig = "Config";
            public const string EmulatorName = "Name";
            public const string EmulatorOptions = "AdditionalCommandLineOptions";
            public const string EmulatorPath = "Path";
            public const string EmulatorLegacyCommandLine = "LegacyCommandLine";
        }

        string m_runtimeVersion;
        string m_runtimeVersionInstalled;  //The best match registration
        string m_assemblyFoldersList;
        string[] m_assemblyFolders;
        string m_frameworkAssembliesPath;
        string m_frameworkToolsPath;
        
        Emulator[] m_emulators;
        
        public PlatformInfo(string runtimeVersion)
        {
            if (!string.IsNullOrEmpty(runtimeVersion))
            {
                Version ver = Version.Parse(runtimeVersion.TrimStart('v'));
                m_runtimeVersion = "v" + ver.ToString(2);
            }
            else
            {
                m_runtimeVersion = "v4.4";
            }
        }

        private void AppendEmulators(List<Emulator> emulators, RegistryKey topLevelKey)
        {
            using (RegistryKey key = GetDeviceFrameworkPaths.OpenDeviceFrameworkKey(topLevelKey, m_runtimeVersionInstalled, RegistryKeys.Emulators))
            {
                if (key != null)
                {
                    string[] subkeyNames = key.GetSubKeyNames();

                    for (int iSubkey = 0; iSubkey < subkeyNames.Length; iSubkey++)
                    {
                        string subkeyName = subkeyNames[iSubkey];

                        using (RegistryKey subkey = key.OpenSubKey(subkeyName))
                        {
                            string path = subkey.GetValue(RegistryValues.EmulatorPath) as string;
                            string name = subkey.GetValue(RegistryValues.EmulatorName) as string;
                            string config = subkey.GetValue(RegistryValues.EmulatorConfig) as string;
                            string options = subkey.GetValue(RegistryValues.EmulatorOptions) as string;
                            int? legacyCommandLine = subkey.GetValue(RegistryValues.EmulatorLegacyCommandLine) as int?;
                            bool fLegacyCommandLine;
                            string persistableName = subkeyName;

                            if (string.IsNullOrEmpty(name))
                            {
                                name = persistableName;
                            }

                            fLegacyCommandLine = legacyCommandLine != null && legacyCommandLine.Value != 0;

                            Emulator emulator = new Emulator();
                            emulator.additionalOptions = options;
                            emulator.application = path;
                            emulator.config = config;
                            emulator.legacyCommandLine = fLegacyCommandLine;
                            emulator.name = name;
                            emulator.persistableName = persistableName;

                            emulators.Add(emulator);
                        }
                    }
                }
            }

        }

        public Emulator[] Emulators
        {
            get
            {
                if (m_emulators == null)
                {
                    EnsureInitialization();

                    List<Emulator> emulators = new List<Emulator>();

                    AppendEmulators(emulators, Registry.LocalMachine);
                    AppendEmulators(emulators, Registry.CurrentUser);

                    m_emulators = emulators.ToArray();

                    //Add special for PRG_VW???
                }

                return m_emulators;
            }
        }

        public Emulator FindEmulator(string name)
        {            
            Emulator[] emulators = this.Emulators;
            Emulator emulator = null;

            for (int i = 0; i < emulators.Length; i++)
            {
                Emulator emulatorT = emulators[i];

                if (string.Equals(name, emulatorT.persistableName))
                {
                    emulator = emulatorT;
                    break;
                }
            }

            return emulator;
        }

        private void EnsureInitialization(string version)
        {
            GetDeviceFrameworkPaths deviceFrameworkPaths = new GetDeviceFrameworkPaths();

            while (true)
            {
                deviceFrameworkPaths.RuntimeVersion = version;

                if (deviceFrameworkPaths.Execute())
                {
                    m_frameworkAssembliesPath = deviceFrameworkPaths.FrameworkAssembliesPath;
                    m_frameworkToolsPath = deviceFrameworkPaths.ToolsPath;

                    m_runtimeVersionInstalled = version;
                    break;
                }

                Debug.Assert(version[0] == 'v');
                //remove the build number from the version, and try again
                int iDot = version.LastIndexOf('.');

                if (iDot < 0)
                    break;

                version = version.Substring(0, iDot);
            }        
        }

        private void EnsureInitialization()
        {
            if (m_frameworkAssembliesPath == null)
            {
                EnsureInitialization(m_runtimeVersion);
            }
        }

        public string FrameworkAssembliesPath
        {
            get
            {
                EnsureInitialization();
                return m_frameworkAssembliesPath;
            }
        }

        public string FrameworkToolsPath
        {
            get
            {
                EnsureInitialization();
                return m_frameworkToolsPath;
            }
        }

        private void AppendFolder(List<string> folders, string folder)
        {
            if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
            {
                folders.Add(folder);
            }
        }

        public string AssemblyFoldersList
        {
            get
            {
                if (m_assemblyFoldersList == null)
                {
                    EnsureInitialization();

                    string[] assemblyFolders = this.AssemblyFolders;
                    StringBuilder sb = new StringBuilder(512);

                    for (int iFolder = 0; iFolder < assemblyFolders.Length; iFolder++)
                    {
                        if (iFolder > 0)
                        {
                            sb.Append(';');
                        }

                        string folder = assemblyFolders[iFolder];
                        sb.Append(folder);
                    }

                    m_assemblyFoldersList = sb.ToString();
                }

                return m_assemblyFoldersList;
            }
        }

        private void AppendFolders(List<string> folders, RegistryKey topLevelKey)
        {
            //Add the AssemblyFoldersEx registry entries
            //HKCU settings as well?  Currently just using HKLM
            using (RegistryKey key = GetDeviceFrameworkPaths.OpenDeviceFrameworkKey(topLevelKey, m_runtimeVersionInstalled, RegistryKeys.AssemblyFoldersEx))
            {
                if (key != null)
                {
                    string[] subkeys = key.GetSubKeyNames();

                    for (int iSubKey = 0; iSubKey < subkeys.Length; iSubKey++)
                    {
                        using (RegistryKey subkey = key.OpenSubKey( subkeys[iSubKey] ))
                        {
                            AppendFolder( folders, (string)subkey.GetValue( RegistryValues.Default ) );
                        }
                    }
                }
            }
        }

        public string[] AssemblyFolders
        {
            get
            {
                if (m_assemblyFolders == null)
                {
                    EnsureInitialization();

                    List<string> folders = new List<string>();

                    //Add the framework assemblies
                    AppendFolder(folders, this.FrameworkAssembliesPath);

                    AppendFolders(folders, Registry.LocalMachine);
                    AppendFolders(folders, Registry.CurrentUser);

                    m_assemblyFolders = folders.ToArray();
                }

                return m_assemblyFolders;
            }
        }
    }
}
