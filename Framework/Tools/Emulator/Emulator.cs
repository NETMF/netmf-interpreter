////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using System.Reflection;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security;

using Microsoft.Build.Tasks.Deployment.ManifestUtilities;

using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Emulator;
using Microsoft.SPOT.Emulator.Com;
using Microsoft.SPOT.Emulator.Gpio;
using Microsoft.SPOT.Emulator.NamedPipes;
using Microsoft.SPOT.Emulator.Serial;
using Microsoft.SPOT.Emulator.Spi;
using Microsoft.SPOT.Emulator.Time;
using Microsoft.SPOT.Emulator.Usb;
using Microsoft.SPOT.Emulator.Memory;
using Microsoft.SPOT.Emulator.Lcd;
using Microsoft.SPOT.Emulator.I2c;
using Microsoft.SPOT.Emulator.Battery;
using Microsoft.SPOT.Emulator.TouchPanel;
using Microsoft.SPOT.Emulator.Watchdog;
using Microsoft.SPOT.Emulator.FS;
using Microsoft.SPOT.Emulator.BlockStorage;

namespace Microsoft.SPOT.Emulator
{
    public class Emulator : EmulatorComponent, IDisposable
    {
        IEmulator _emulatorNative;
        Thread _clrThread;

        Hal _managedHal;
        SpiBus _spiBus;
        TimingServices _timingServices;
        GpioCollection _gpioPorts;
        RamManager _ramManager;
        LcdDisplay _lcdDisplay;
        ComPortCollection _comPorts;
        SerialPortCollection _serialPorts;
        I2cBus _i2cBus;
        BatteryCell _battery;
        FileSystemCollection _fileSystems;
        BlockStorageCollection _blockStorageDevices;

        ITouchPanelDriver _touchDriver;
        IWatchdogDriver _watchdogDriver;

        object _isrLock;
        Thread _isrThread;
        int _isrLockCount;
        List<EmulatorComponent> _components;
        ManualResetEvent _mreShutdown;
        CommandLine _cmdLn;
        bool _verbose;

        internal bool Verbose
        {
            get { return _verbose; }
        }

        ConfigurationEngine _configEngine;

        public enum EmulatorState
        {
            Configuration,
            Setup,
            Initialize,
            Running,
            ShutDown,
            Uninitialize,
        }

        EmulatorState _emulatorState = EmulatorState.Configuration;

        public EmulatorState State
        {
            get { return _emulatorState; }
        }

        internal delegate void ExecuteWithInterruptsDisabledCallback();

        internal void ExecuteWithInterruptsDisabled(ExecuteWithInterruptsDisabledCallback callback)
        {
            try
            {
                this.DisableInterrupts();

                callback();
            }
            finally
            {
                this.EnableInterrupts();
            }
        }

        private bool IsDerivedFrom<T>( EmulatorComponent ec )
        {
            return (ec is T);
        }

        private EmulatorComponentCollection FindCollection( EmulatorComponent ec )
        {
            List<EmulatorComponent> collections = _components.FindAll( IsDerivedFrom<EmulatorComponentCollection> );

            return (EmulatorComponentCollection)collections.Find(
                delegate( EmulatorComponent ecc )
                {
                    return ((EmulatorComponentCollection)ecc).CollectionType.IsAssignableFrom( ec.GetType() );
                } );
        }

        public SpiBus SpiBus
        {
            [DebuggerHidden]
            get
            {
                if (_spiBus == null)
                {
                    _spiBus = (SpiBus)_components.Find( IsDerivedFrom<SpiBus> );
                }
                return _spiBus;
            }
        }

        public TimingServices TimingServices
        {
            get
            {
                if (_timingServices == null)
                {
                    _timingServices = (TimingServices)_components.Find( IsDerivedFrom<TimingServices> );
                }
                return _timingServices;
            }
        }

        public GpioCollection GpioPorts
        {
            [DebuggerHidden]
            get
            {
                if (_gpioPorts == null)
                {
                    _gpioPorts = (GpioCollection)_components.Find( IsDerivedFrom<GpioCollection> );
                }
                return _gpioPorts;
            }
        }

        internal ComPortCollection ComPortCollection
        {
            [DebuggerHidden]
            get
            {
                if (_comPorts == null)
                {
                    _comPorts = (ComPortCollection)_components.Find(IsDerivedFrom<ComPortCollection>);
                }
                return _comPorts;
            }
        }

        public SerialPortCollection SerialPorts
        {
            [DebuggerHidden]
            get
            {
                if (_serialPorts == null)
                {
                    _serialPorts = (SerialPortCollection)_components.Find( IsDerivedFrom<SerialPortCollection> );
                }
                return _serialPorts;
            }
        }

        public FileSystemCollection FileSystems
        {
            [DebuggerHidden]
            get
            {
                if (_fileSystems == null)
                {
                    _fileSystems = (FileSystemCollection)_components.Find(IsDerivedFrom<FileSystemCollection>);
                }
                return _fileSystems;
            }
        }

        public BlockStorageCollection BlockStorageDevices
        {
            [DebuggerHidden]
            get
            {
                if (_blockStorageDevices == null)
                {
                    _blockStorageDevices = (BlockStorageCollection)_components.Find(IsDerivedFrom<BlockStorageCollection>);
                }
                return _blockStorageDevices;
            }
        }

        public LcdDisplay LcdDisplay
        {
            [DebuggerHidden]
            get
            {
                if (_lcdDisplay == null)
                {
                    _lcdDisplay = (LcdDisplay)_components.Find( IsDerivedFrom<LcdDisplay> );
                }
                return _lcdDisplay;
            }
        }

        internal IHal ManagedHal
        {
            [DebuggerHidden]
            get
            {
                if (_managedHal == null)
                {
                    _managedHal = (Hal)_components.Find( IsDerivedFrom<Hal> );
                }
                return _managedHal;
            }
        }

        public RamManager RamManager
        {
            [DebuggerHidden]
            get
            {
                if (_ramManager == null)
                {
                    _ramManager = (RamManager)_components.Find(IsDerivedFrom<RamManager>);
                }
                return _ramManager;
            }
        }

        public I2cBus I2cBus
        {
            [DebuggerHidden]
            get
            {
                if (_i2cBus == null)
                {
                    _i2cBus = (I2cBus)_components.Find( IsDerivedFrom<I2cBus> );
                }
                return _i2cBus;
            }
        }

        public BatteryCell Battery
        {
            [DebuggerHidden]
            get
            {
                if (_battery == null)
                {
                    _battery = (BatteryCell)_components.Find( IsDerivedFrom<BatteryCell> );
                }
                return _battery;
            }
        }

        public ITouchPanelDriver TouchPanelDriver
        {
            get
            {
                if (_touchDriver == null)
                {
                    _touchDriver = (ITouchPanelDriver)_components.Find(IsDerivedFrom<ITouchPanelDriver>);
                }

                return _touchDriver;
            }
        }

        public IWatchdogDriver WatchdogDriver
        {
            get
            {
                if (_watchdogDriver == null)
                {
                    _watchdogDriver = (IWatchdogDriver)_components.Find(IsDerivedFrom<IWatchdogDriver>);
                }

                return _watchdogDriver;
            }
        }

        internal object IsrLock
        {
            [DebuggerHidden]
            get { return _isrLock; }
        }

        internal void DisableInterrupts()
        {
            bool wasTaken = false;
            Monitor.Enter(_isrLock, ref wasTaken);

            _isrThread = Thread.CurrentThread;
            if (wasTaken)
            {
                _isrLockCount++;
            }
        }

        internal void EnableInterrupts()
        {
            Debug.Assert(_isrThread == Thread.CurrentThread);

            _isrLockCount--;

            if (_isrLockCount == 0)
            {
                _isrThread = null;
            }

            Monitor.Exit(_isrLock);
        }

        internal bool AreInterruptsEnabled
        {
            get
            {
#if DEBUG
                Thread thread = _isrThread;

                Debug.Assert(thread == null || thread == Thread.CurrentThread);
#endif

                return _isrThread == null;
            }
        }

        public ManualResetEvent ShutdownHandle
        {
            get { return _mreShutdown; }
        }

        public Emulator(IEmulator hal)
        {
            SetEmulator(this);

            _components = new List<EmulatorComponent>();

            _mreShutdown = new ManualResetEvent(false);
            _isrLock = new object();

            _configEngine = new ConfigurationEngine( this );

            _verbose = false;

            //Add back a SerialPort class/collection to provide access to ComPortCollection a bit
            //easier?

            _emulatorNative = hal;
        }

        // Use the default, Microsoft-supplied, HAL implementation
        public Emulator()
            :this(LoadDefaultHAL())
        {
        }

        public void RegisterComponent( EmulatorComponent component )
        {
            if (!_components.Contains(component))
            {
                if (FindComponentById(component.ComponentId) != null)
                {
                    throw new Exception("Another emulator component with component ID \"" + component.ComponentId + "\" already exists.");
                }

                component.SetEmulator( this );
                _components.Add(component);

                if (_emulatorState >= EmulatorState.Setup)
                {
                    component.SetupComponent();
                }

                if (_emulatorState >= EmulatorState.Initialize)
                {
                    component.InitializeComponent();

                    EmulatorComponentCollection ecc;
                    if ((ecc = FindCollection(component)) != null)
                    {
                        ecc.RegisterInternal(component);
                    }
                }

                if (_emulatorState >= EmulatorState.ShutDown)
                {
                    throw new ApplicationException("Cannot add new Emulator component during Shutdown");
                }
            }
        }

        public void RegisterComponent( EmulatorComponent component, EmulatorComponent linkedBy )
        {
            RegisterComponent( component );

            if (linkedBy != null)
            {
                if (component.LinkedBy == null)
                {
                    component.LinkedBy = linkedBy;

                    linkedBy.LinkedComponents.Add( component );
                }
                else if (component.LinkedBy != linkedBy)
                {
                    throw new Exception( "This EmulatorComponent is already in " + component.LinkedBy.ComponentId + "." );
                }
            }
        }

        public void UnregisterComponent(EmulatorComponent component)
        {
            if (_components.Contains(component))
            {
                foreach (EmulatorComponent ec in component.LinkedComponents)
                {
                    UnregisterComponent(ec);
                }

                _components.Remove(component);

                if (_emulatorState > EmulatorState.Setup)
                {
                    EmulatorComponentCollection ecc;
                    if ((ecc = FindCollection(component)) != null)
                    {
                        ecc.UnregisterInternal(component);
                    }
                }

                if (_emulatorState > EmulatorState.Initialize)
                {
                    component.UninitializeComponent();
                }
            }
        }

        internal EmulatorComponent FindReplaceableComponent( EmulatorComponent component )
        {
            return _components.Find( delegate( EmulatorComponent ec ) { return ec.IsReplaceableBy( component ); } );
        }

        public EmulatorComponent FindComponentById( String componentId )
        {
            return _components.Find( delegate( EmulatorComponent ec ){ return ec.ComponentId == componentId; } );
        }

        internal IEmulator EmulatorNative
        {
            [DebuggerHidden]
            get { return _emulatorNative; }
        }

        public ConfigurationEngine ConfigurationEngine
        {
            [DebuggerHidden]
            get { return _configEngine; }
        }

        internal bool IsInvokeRequired
        {
            get
            {
                return _clrThread != null && _clrThread != Thread.CurrentThread;
            }
        }

        private class CommandLine
        {
            public List<String> configFiles;
            public List<String> assemblies;
            public bool waitForDebuggerOnStartup;
            public String commandLineArguments;
            public bool useDefaultConfig;
            public bool verbose;
            public bool noMessageBox;

            public CommandLine()
            {
                configFiles = new List<String>();
                assemblies = new List<String>();
                waitForDebuggerOnStartup = false;
                commandLineArguments = String.Empty;
                useDefaultConfig = true;
                verbose = false;
                noMessageBox = false;
            }
        }

        public virtual void Run()
        {
            _emulatorNative.Start();
        }

        public void LoadAssembly( String asm )
        {
            if (!File.Exists( asm ))
            {
                throw new Exception( String.Format( "Assembly file '{0}' does not exist! Load failed.", asm ) );
            }

            String extension = Path.GetExtension( asm ).ToLower();

            if (extension == ".pe")
            {
                _emulatorNative.LoadPE( asm );
            }
            else if (extension == ".dat")
            {
                _emulatorNative.LoadDatabase( asm );
            }
            else if (extension == ".manifest" || extension == ".exe" || extension == ".dll")
            {
                String manifestPath = asm;

                if (extension == ".exe" || extension == ".dll")
                {
                    manifestPath += ".manifest";
                }

                if (!File.Exists( manifestPath ))
                {
                    throw new Exception( String.Format("Manifest file '{0}' does not exist! Load failed.", manifestPath ) );
                }

                Manifest manifest = ManifestReader.ReadManifest( manifestPath, false );

                String manifestDirectoryPath = Path.GetDirectoryName( manifestPath );
                String exePePath = FullPathToPe( manifestDirectoryPath, manifest.EntryPoint.TargetPath );

                foreach (AssemblyReference ar in manifest.AssemblyReferences)
                {
                    if (ar.AssemblyIdentity.Name == "Microsoft.Windows.CommonLanguageRuntime")
                    {
                        continue; // the manifest will always include the desktop CLR, which we will to ignore.
                    }
                    String pePath = FullPathToPe( manifestDirectoryPath, ar.TargetPath );
                    if (pePath == exePePath) // Skip this one for now to do last
                    {
                        continue;
                    }

                    LoadAssembly( pePath );
                }

                LoadAssembly( exePePath );
            }
            else
            {
                throw new Exception( "Unrecognized assembly format: " + asm );
            }
        }

        private string FullPathToPe(String basePath, string fileName)
        {
            // Given a base path, and a file name which may be any of a simple file, a relative path, or an absolute path
            // return an absolute path to the PE file corresponding.

            string absPath = Path.Combine(basePath, fileName);

            return Path.ChangeExtension(absPath, "pe");
        }

        private void LoadAssemblies()
        {
            if (_cmdLn.assemblies.Count == 0)
            {
                throw new Exception( "No assembly file is found. Please specify at least one exe, dll, manifest, or pe file to load." );
            }

            foreach (String asm in _cmdLn.assemblies)
            {
                this.LoadAssembly(asm);
            }
		}

        public override void Configure(XmlReader reader)
        {
            _configEngine.ConfigureEmulator(reader);
        }

        internal void ApplyConfig(string fileName)
        {
            XmlReader reader = XmlReader.Create(fileName);

            reader.ReadStartElement("Emulator");

            this.Configure(reader);
        }

        private static IEmulator LoadDefaultHAL()
        {
            IEmulator emu = null;
            string emulatorTypeName = "Microsoft.SPOT.Emulator.EmulatorNative";

            // Construct an assembly reference name using our own version number
            // and public key, since we use the same for both Emulator.dll and CLR.dll

            AssemblyName halWindowsName = new AssemblyName();
            Assembly exec = Assembly.GetExecutingAssembly();
            halWindowsName.Name = "Microsoft.SPOT.CLR";
            halWindowsName.CultureInfo =  System.Globalization.CultureInfo.InvariantCulture;
            halWindowsName.Version = exec.GetName().Version;
            halWindowsName.SetPublicKey(exec.GetName().GetPublicKey());

            try
            {
                Assembly halWindowsNativeAssembly = Assembly.LoadFile(Path.Combine(Path.GetDirectoryName(exec.Location), halWindowsName.Name + ".dll"));
                emu = halWindowsNativeAssembly.CreateInstance(emulatorTypeName) as IEmulator;
                if ( null == emu )
                    {
                        throw new Exception("Internal error: the type " + emulatorTypeName + " was not found in the assembly " + halWindowsName.Name);
                    }
            }
            catch (FileNotFoundException)
            {
                throw new Exception("Internal error: the emulator HAL DLL " + halWindowsName.Name + " could not be found.");
            }
            catch (FileLoadException)
            {
                throw new Exception("Internal error: the emulator HAL DLL " + halWindowsName.Name + " could not be loaded.");
            }
            catch (BadImageFormatException)
            {
                throw new Exception("Internal error: the emulator HAL DLL " + halWindowsName.Name + " is not a valid assembly.");
            }
            catch (SecurityException)
            {
                throw new Exception("Internal error: the emulator HAL DLL " + halWindowsName.Name + " was specified without the required WebPermission.");
            }
            catch (MissingMethodException)
            {
                throw new Exception("Internal error: the emulator HAL DLL " + halWindowsName.Name + " had no constructor for the type " + emulatorTypeName + ".");
            }

            return emu;
        }

        private void StartInternal()
        {
            Utility.SetCurrentThreadAffinity();

            // Set the verbose flag
            if (_cmdLn.verbose)
            {
                _verbose = true;
            }

            // 1) Create all the EmulatorComponents first and configure them properly based on the config file
            if (_cmdLn.useDefaultConfig)
            {
                LoadDefaultComponents();
            }

            for (int iConfig = 0; iConfig < _cmdLn.configFiles.Count; iConfig++)
            {
                string configFileName = _cmdLn.configFiles[iConfig];
                ApplyConfig(configFileName);
            }

            string defaultConfigFileName = Application.ExecutablePath + ".emulatorconfig";
            if (File.Exists(defaultConfigFileName))
            {
                ApplyConfig(defaultConfigFileName);
            }

            // 2) Calling Setup() on all EmulatorComponents to allow each component to check for the
            //    validity of each properties, allocate memory, ... etc.
            SetupComponent();

            CheckForRequiredComponents();

            // 3) Now that the components are in the correct state. Register each component with its collection,
            //    if there is one.
            RegisterComponentsWithCollection();

            try
            {
                  //Initialization of EmulatorNative required before EmulatorComponents
                  //TimingServices, for example, is dependent on EmulatorNative initialization
                _emulatorNative.Initialize(this.ManagedHal);

                // 4) Call InitializeComponent() on all EmulatorComponents to put each component into a
                //    working / running state.
                InitializeComponent();

                // 5) Load the assemblies
                LoadAssemblies();

                // 6) Get into the initial state
                if (_cmdLn.waitForDebuggerOnStartup)
                {
                    _emulatorNative.WaitForDebuggerOnStartup();
                }

                if (String.IsNullOrEmpty(_cmdLn.commandLineArguments) == false)
                {
                    _emulatorNative.SetCommandLineArguments(_cmdLn.commandLineArguments);
                }

                _emulatorState = EmulatorState.Running;

                //start time -- this allows components to hook into events before execution starts
                TimingServices.ResumeTime();

                // 7) We start the emulator.

                this.Run();
            }
            catch (EmulatorException e)
            {
                // if we are shutting down and received the unexpected shut down exception, we'll handle the exception
                if (e.ErrorCode == CLR_ERRORCODE.CLR_E_SHUTTING_DOWN)
                {
                    Debug.Assert(this.State >= EmulatorState.ShutDown);
                }
                else
                {
                    // otherwise, re-throw.
                    throw;
                }
            }
            finally
            {
                bool needUnintialization = (this.State >= EmulatorState.Initialize);

                Stop();

                if (needUnintialization)
                {
                    UninitializeComponent();
                }
            }
        }

        private void CheckForRequiredComponents()
        {
            ThrowIfNull( this.ManagedHal, "Hal" );
            ThrowIfNull( this.SpiBus, "SpiBus" );
            ThrowIfNull( this.GpioPorts, "GpioCollection" );
            ThrowIfNull( this.TimingServices, "TimingServices" );
            ThrowIfNull( this.RamManager, "RamManager" );
            ThrowIfNull( this.ComPortCollection, "ComPortCollection" );
            ThrowIfNull( this.I2cBus, "I2cBus" );
            ThrowIfNull( this.SerialPorts, "SerialPortCollection" );
            ThrowIfNull( this.FileSystems, "FileSystemCollection" );
            ThrowIfNull( this.BlockStorageDevices, "BlockStorageCollection" );
        }

        private void ThrowIfNull( Object o, String name )
        {
            if (o == null)
            {
                throw new Exception( "Emulator requires a(n)" + name + " EmulatorComponent to run." );
            }
        }

        protected virtual void LoadDefaultComponents()
        {
            RegisterComponent( new Hal() );
            RegisterComponent( new ComPortCollection() );
            RegisterComponent( new GpioCollection() );
            RegisterComponent( new SpiBus() );
            RegisterComponent( new RamManager() );
            RegisterComponent( new TimingServices() );
            RegisterComponent( new I2cBus() );
            RegisterComponent( new SerialPortCollection() );
            RegisterComponent( new FileSystemCollection() );
            RegisterComponent( new BlockStorageCollection() );

            //debugging.  Put in configuration somewhere?
            //Add a DebuggingComponent to control creation the NamedPipeServer or TcpListener?
            //
            //  <Debuggers><Debugger><Port>Debug1</><Type>NamedPipe</></></> ????
            RegisterComponent( new NamedPipeServer() );
        }

        private void ParseCommandLineFromFile(string fileName, CommandLine commandLine)
        {
            List<String> moreArgs = new List<String>();

            using (StreamReader sr = new StreamReader(Environment.ExpandEnvironmentVariables(fileName)))
            {
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();

                    if (line.Length > 0 && line[0] != '#')
                    {
                        moreArgs.Add(line);
                    }
                }
            }

            ParseCommandLine(moreArgs.ToArray(), commandLine);
        }

        private void ParseCommandLineOption(string command, CommandLine commandLine)
        {
            int indexColon = command.IndexOf(':');
            string argument = string.Empty;

            if (indexColon != -1)
            {
                argument = command.Substring(indexColon + 1);
                command = command.Substring(0, indexColon);

                argument = Environment.ExpandEnvironmentVariables(argument);
            }

            switch (command.ToLower())
            {
                case "config":
                    commandLine.configFiles.Add(argument);
                    break;
                case "load":
                    commandLine.assemblies.Add(argument);
                    break;
                case "waitfordebugger":
                    commandLine.waitForDebuggerOnStartup = true;
                    break;
                case "commandlinearguments":
                    commandLine.commandLineArguments = argument;
                    break;
                case "nodefaultconfig":
                    commandLine.useDefaultConfig = false;
                    break;
                case "verbose":
                    commandLine.verbose = true;
                    break;
                case "nomessagebox":
                    commandLine.noMessageBox = true;
                    break;
                default:
                    // unrecognized commandline options will get passed down to EmulatorNative
                    throw new Exception("Unknown command line option: " + command);
            }
        }

        private void ParseCommandLine( String[] args, CommandLine commandLine )
        {
            foreach (String s in args)
            {
                char commandChar = s[0];
                String command = s.Substring(1);

                switch (commandChar)
                {
                    case '@':
                        ParseCommandLineFromFile(command, commandLine);
                        break;
                    case '/':
                        ParseCommandLineOption(command, commandLine);
                        break;
                    default:
                        // If s is a valid file, we'll treat it as a /load: command
                        if (File.Exists(s))
                        {
                            ParseCommandLineOption( "load:" + s, commandLine );
                        }
                        else
                        {
                            throw new Exception("Unknown command line option: " + s);
                        }
                        break;
                }
            }
        }

        internal void SetSystemEvents(Events.SystemEvents events)
        {
            if (events != Events.SystemEvents.NONE)
            {
                this.ManagedHal.Events.Set((uint)events);
            }
        }

        private delegate void ComponentCallback(EmulatorComponent component);

        private void CallComponents( ComponentCallback callback )
        {
            List<EmulatorComponent> components = new List<EmulatorComponent>( _components );

            for (int i = 0; i < components.Count; i++)
            {
                EmulatorComponent component = components[i];

                callback( component );
            }
        }

        public override void InitializeComponent()
        {
            _emulatorState = EmulatorState.Initialize;

            CallComponents( delegate( EmulatorComponent component ) { component.InitializeComponent(); } );
        }

        public override void UninitializeComponent()
        {
            _emulatorState = EmulatorState.Uninitialize;

            CallComponents( delegate( EmulatorComponent component ) { component.UninitializeComponent(); } );
        }

        public override void SetupComponent()
        {
            _emulatorState = EmulatorState.Setup;

            CallComponents( delegate( EmulatorComponent component ) { component.SetupComponent(); } );
        }

        private void RegisterComponentsWithCollection()
        {
            CallComponents(
                delegate( EmulatorComponent ec )
                {
                    EmulatorComponentCollection ecc = this.FindCollection( ec );
                    if (ecc != null)
                    {
                        ecc.RegisterInternal( ec );
                    }
                } );
        }

        public bool IsShuttingDown
        {
            get { return (_emulatorState >= EmulatorState.ShutDown); }
        }

        public void Stop()
        {
            _emulatorState = EmulatorState.ShutDown;

            if (null != _emulatorNative) _emulatorNative.Shutdown();
            _mreShutdown.Set();
        }


        public void Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            string[] newArgs = new string[args.Length-1];
            Array.Copy(args, 1, newArgs, 0, newArgs.Length);

            this.Start(newArgs);
        }

        public virtual void Start( String[] args )
        {

            _cmdLn = new CommandLine();
            _clrThread = Thread.CurrentThread;

            try
            {
                ParseCommandLine( args, _cmdLn );
                StartInternal();
            }
            catch (Exception e)
            {
                String errorMessage = String.Empty;

                while (e != null)
                {
                    if (_cmdLn.verbose == true)
                    {
                        errorMessage += e.ToString() + "\r\n\r\n";
                    }
                    else
                    {
                        // Typically, we'd care only about the source of the error (i.e. the innermost exception) only.
                        errorMessage = "Error: " + e.Message + "\r\n";
                    }

                    e = e.InnerException;
                }

                if (_cmdLn.noMessageBox == false)
                {
                    MessageBox.Show( errorMessage, "Emulator" );
                }

                Console.Error.WriteLine( errorMessage );
            }
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            Stop();
        }

        #endregion
    }
}
