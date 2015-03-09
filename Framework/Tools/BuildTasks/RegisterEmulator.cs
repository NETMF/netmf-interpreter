using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Win32;

namespace Microsoft.SPOT.Tasks
{
    public class RegisterEmulator : Task
    {
        String _name;
        String _config;
        String _frameworkVersion;
        String _path;
        String _additionalCommandLineOptions;
        bool _currentUser;
        String _subkeyName;

        class RegistryKeys
        {
            public const string Emulators = "Emulators";
        }

        class RegistryValues
        {
            public const string Default = "";
            public const string EmulatorConfig = "Config";
            public const string EmulatorName = "Name";
            public const string EmulatorOptions = "AdditionalCommandLineOptions";
            public const string EmulatorPath = "Path";
        }

        public override bool Execute()
        {
            //Log.LogWarning("Registering Emulator with key: " + _subkeyName + ": " + _frameworkVersion + ": " + RegistryKeys.Emulators);

            RegistryKey emulatorsKey = GetDeviceFrameworkPaths.OpenDeviceFrameworkKey(Registry.LocalMachine, _frameworkVersion, RegistryKeys.Emulators );

            //Log.LogWarning("Registering Emulator with root: " + emulatorsKey.ToString());

            if (_currentUser) // make sure the CurrentUser key for the framework exists
            {
                System.Diagnostics.Debug.Assert(emulatorsKey.Name.StartsWith(@"HKEY_LOCAL_MACHINE\"));
                String keyName = emulatorsKey.Name.Remove(0, @"HKEY_LOCAL_MACHINE\".Length);
                emulatorsKey.Close();
                emulatorsKey = Registry.CurrentUser.CreateSubKey(keyName);
            }

            RegistryKey emulatorKey = emulatorsKey.CreateSubKey(_subkeyName);
            
            emulatorKey.SetValue( RegistryValues.EmulatorName, _name );

            string config = null;
            if (!String.IsNullOrEmpty(_config))
            {
                config = System.IO.Path.Combine(Environment.CurrentDirectory, _config);
            }

            ConditionalSetValue( emulatorKey, RegistryValues.EmulatorConfig, config);
            ConditionalSetValue( emulatorKey, RegistryValues.EmulatorPath, System.IO.Path.Combine(Environment.CurrentDirectory,_path) );
            ConditionalSetValue( emulatorKey, RegistryValues.EmulatorOptions, _additionalCommandLineOptions );

            emulatorKey.Close();
            emulatorsKey.Close();

            return true;
        }

        private void ConditionalSetValue( RegistryKey key, String valueName, String value )
        {
            if (String.IsNullOrEmpty( value ))
            {
                key.DeleteValue( valueName, false );
            }
            else
            {
                key.SetValue( valueName, value );
            }
        }

        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public String Config
        {
            get { return _config; }
            set { _config = value; }
        }

        public String FrameworkVersion
        {
            get { return _frameworkVersion; }
            set { _frameworkVersion = value; }
        }

        public String Path
        {
            get { return _path; }
            set { _path = value; }
        }

        public String AddtionalCommandLineOptions
        {
            get { return _additionalCommandLineOptions; }
            set { _additionalCommandLineOptions = value; }
        }

        public bool CurrentUser
        {
            get { return _currentUser; }
            set { _currentUser = value; }
        }

        [Required]
        public String SubkeyName
        {
            get { return _subkeyName; }
            set { _subkeyName = value; }
        }

    }
}
