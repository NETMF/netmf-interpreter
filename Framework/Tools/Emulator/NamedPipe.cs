////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.SPOT.Emulator.Com;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace Microsoft.SPOT.Emulator.NamedPipes
{
    public class NamedPipeServer : EmulatorComponent
    {
        internal class NamedPipeConnectData //: IDisposable -- need to cleanup _pipe??
        {
            public SafeFileHandle _pipe;
            public AutoResetEvent _are;
            public Overlapped _overlapped;
            public bool _isConnected;
            public string _pipeName;
            public ComPortHandle _comPortHandle;
            public bool _isRegistered;
        }

        private const int _maxClients = 2;
        private AutoResetEvent _areClients;
        private Thread _threadPipeServer;
        private NamedPipeConnectData[] _connectData;

        private int FindConnectDataIndex( ComPortHandle handle )
        {
            for (int i = 0; i < _connectData.Length; i++)
            {
                NamedPipeConnectData connectData = _connectData[i];

                if (connectData != null && connectData._comPortHandle == handle)
                {
                    return i;
                }
            }

            return -1;
        }

        private void InitializeConnectData(NamedPipeConnectData connectData)
        {
            connectData._are = new AutoResetEvent(false);
            connectData._pipe = CreateNamedPipe(connectData._pipeName);
            connectData._overlapped = new Overlapped(0, 0, connectData._are.SafeWaitHandle.DangerousGetHandle(), null);
            connectData._isConnected = Native.ConnectNamedPipe(connectData._pipe.DangerousGetHandle(), connectData._overlapped);

            if (!connectData._isConnected && Native.GetLastError() == Native.ERROR_PIPE_CONNECTED)
            {
                connectData._isConnected = true;
            }
        }

        public void RegisterNamedPipe( string pipeName, ComPortHandle handle )
        {
            Debug.Assert( FindConnectDataIndex( handle ) == -1 );
            Debug.Assert(this.Emulator.State < Emulator.EmulatorState.Initialize); 
            //thread safety??

            NamedPipeConnectData connectData = new NamedPipeConnectData();

            connectData._comPortHandle = handle;
            connectData._pipeName = pipeName;

            InitializeConnectData( connectData );

            NamedPipeConnectData[] connectDataNew = new NamedPipeConnectData[_connectData.Length + 1];
            Array.Copy( _connectData, connectDataNew, _connectData.Length );

            connectDataNew[_connectData.Length] = connectData;

            _connectData = connectDataNew;

            _areClients.Set();
        }

        public override void SetupComponent()
        {
            base.SetupComponent();

            _areClients = new AutoResetEvent(true);
            _threadPipeServer = new Thread(new ThreadStart(PipeServerThreadProc));
            _connectData = new NamedPipeConnectData[0];

            //Should this be configurable via a property, which ports to open?
            //Allow Debug over sockets, and DebugRpc over named pipe? etc...

            RegisterNamedPipe( GetPipeName( 0 ), ComPortHandle.DebugPortRpc );
            RegisterNamedPipe( GetPipeName( 1 ), ComPortHandle.DebugPort );

            this.Emulator.ComPortCollection.CollectionChanged += new System.ComponentModel.CollectionChangeEventHandler(OnComPortCollectionChanged);
        }

        private void OnComPortCollectionChanged(object sender, CollectionChangeEventArgs args)
        {
            if (args.Action == CollectionChangeAction.Remove)
            {                
                UnregisterNamedPipePort(args.Element as ComPort);
            }
        }

        public override bool IsReplaceableBy(EmulatorComponent ec)
        {
            return (ec is NamedPipeServer);
        }

        private static string GetPipeName(int port)
        {
            return string.Format(@"\\.\pipe\TinyCLR_{0}_Port{1}", System.Diagnostics.Process.GetCurrentProcess().Id, port);
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        private void RegisterNamedPipePort(NamedPipeConnectData connectData)
        {
            VerifyAccess();

            AsyncFileStream stream = new AsyncFileStream(connectData._pipeName, connectData._pipe);

            ComPortToNamedPipe namedPipePort = new ComPortToNamedPipe(connectData._pipeName, stream);

            namedPipePort.ComPortHandle = connectData._comPortHandle;

            this.Emulator.RegisterComponent(namedPipePort);

            connectData._isRegistered = true;

            //The TinyCLR doesn't call Initialize for the debug NamedPipe ports always
            //and we are swapping out components anyway, so we need to force the
            //initialization here to start the reading
            namedPipePort.DeviceInitialize();
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        private void UnregisterNamedPipePort(ComPort port)
        {
            VerifyAccess();

            int idx = FindConnectDataIndex(port.ComPortHandle);

            if (idx != -1)
            {
                NamedPipeConnectData connectData = _connectData[idx];

                Debug.Assert(connectData._isConnected);

                connectData._isRegistered = false;
                connectData._isConnected = false;
                connectData._pipe.Dispose();

                //reset connectdata
                InitializeConnectData(connectData);

                port.DeviceUninitialize();

                _areClients.Set();
            }
        }

        private SafeFileHandle CreateNamedPipe(string pipeName)
        {
            CommonSecurityDescriptor sd = new CommonSecurityDescriptor(false, false, "D:(A;;GA;;;LS)(A;;GA;;;BA)(A;;GA;;;IU)");

            byte[] sdBytes = new byte[sd.BinaryLength];
            sd.GetBinaryForm(sdBytes, 0);
            GCHandle gcHandle = GCHandle.Alloc(sdBytes, GCHandleType.Pinned);

            Native.SECURITY_ATTRIBUTES securityAttributes = new Native.SECURITY_ATTRIBUTES();
            securityAttributes.nLength = Marshal.SizeOf(securityAttributes);
            securityAttributes.bInheritHandle = 0;
            securityAttributes.lpSecurityDescriptor = Marshal.UnsafeAddrOfPinnedArrayElement(sdBytes, 0);

            IntPtr handle = Native.CreateNamedPipe(
                pipeName,
                Native.PIPE_ACCESS_DUPLEX | Native.FILE_FLAG_OVERLAPPED | Native.FILE_FLAG_FIRST_PIPE_INSTANCE,
                Native.PIPE_TYPE_BYTE | Native.PIPE_READMODE_BYTE | Native.PIPE_WAIT,
                1,
                2048,
                2048,
                Native.NMPWAIT_USE_DEFAULT_WAIT,
                securityAttributes);

            gcHandle.Free();

            return new SafeFileHandle(handle, true);
        }

        public override void InitializeComponent()
        {
            base.InitializeComponent();

            if (_connectData.Length > 0)
            {
                _threadPipeServer.Start();
            }
        }

        private void ConnectToClients()
        {
            VerifyAccess();

            for (int iPort = 0; iPort < _connectData.Length; iPort++)
            {
                NamedPipeConnectData connectData = _connectData[iPort];

                if (connectData != null && connectData._isConnected && !connectData._isRegistered)
                {
                    RegisterNamedPipePort(connectData);                                        
                }                
            }
        }
        
        private void PipeServerThreadProc()
        {
            int shutdownHandleIndex = _connectData.Length;
            int moreClientsIndex = shutdownHandleIndex + 1;

            WaitHandle[] handles = new WaitHandle[moreClientsIndex + 1];

            handles[shutdownHandleIndex] = this.Emulator.ShutdownHandle;
            handles[moreClientsIndex] = _areClients;

            while (true)
            {
                lock (this)
                {
                    for (int iPort = 0; iPort < shutdownHandleIndex; iPort++)
                    {
                        NamedPipeConnectData connectData = _connectData[iPort];

                        handles[iPort] = connectData._are;
                    }
                }

                int waitResult = WaitHandle.WaitAny(handles);

                if (waitResult == shutdownHandleIndex)
                {
                    //Shutdown
                    break;
                }

                if (waitResult < shutdownHandleIndex)
                {
                    _connectData[waitResult]._isConnected = true;

                    this.Emulator.BeginInvoke(
                        delegate
                        {
                            ConnectToClients();
                        }
                    );
                }
            }
        }

    }    
}
