////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using Microsoft.Win32.SafeHandles;
using Microsoft.SPOT.Emulator.Com;

namespace Microsoft.SPOT.Emulator.Sockets
{
    internal class SocketsDriver : HalDriver<ISocketsDriver>, ISocketsDriver
    {
        internal static class TinyCLRSockets
        {
            [StructLayout(LayoutKind.Sequential)]
            internal class fd_set
            {
                internal uint count;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
                internal int[] fd_array;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class SockAddr
        {
            public short  sin_family;
            public ushort sin_port;
            public uint   sin_addr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] sin_zero = new byte[8];

            public void MarshalFromNative(IntPtr native)
            {
                Marshal.PtrToStructure(native, this);

                sin_port = (ushort)(((sin_port & 0xFF) << 8) | ((sin_port >> 8) & 0xFF));
            }

            public void MarshalToNative(IntPtr native)
            {
                sin_port = (ushort)(((sin_port & 0xFF) << 8) | ((sin_port >> 8) & 0xFF));

                Marshal.StructureToPtr(this, native, true);
            }
        }

        internal static class NativeSockets
        {
            [StructLayout(LayoutKind.Sequential)]
            internal class fd_set
            {
                const int FD_SETSIZE = 64;

                internal uint count;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = FD_SETSIZE)]
                internal IntPtr[] fd_array;

                public fd_set()
                {
                    fd_array = new IntPtr[FD_SETSIZE];
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct WSAData
            {
                internal short wVersion;
                internal short wHighVersion;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 257)]
                internal string szDescription;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
                internal string szSystemStatus;
                internal short iMaxSockets;
                internal short iMaxUdpDg;
                internal IntPtr lpVendorInfo;
            }

            public const int FIONREAD = 0x4004667F;
            public const int FIONBIO = unchecked((int)0x8004667E);
            public const int FIOASYNC = unchecked((int)0x8004667D);
            public const int SIOGETEXTENSIONFUNCTIONPOINTER = unchecked((int)0xC8000006);
            public const int FD_ALL_EVENTS = (1 << 10) - 1;

            private const string WS2_32 = "Ws2_32.dll";


            internal class SafeSocketHandle : SafeHandleMinusOneIsInvalid
            {
                public SafeSocketHandle(IntPtr Handle) : base(false)
                {
                    this.SetHandle(Handle);
                }

                protected SafeSocketHandle() : base(true) { }

                protected override bool ReleaseHandle()
                {
                    // handle released by managed socket
                    return true;
                }
            }

            [DllImport(WS2_32)]
            public extern static int getaddrinfo(IntPtr nodename, IntPtr servname, IntPtr hints, out IntPtr res);

            [DllImport(WS2_32)]
            public extern static void freeaddrinfo(IntPtr ai);

            [DllImport(WS2_32)]
            public extern static int ioctlsocket(SafeSocketHandle s, int cmd, ref int argp);

            [DllImport(WS2_32)]
            public extern static int connect(SafeSocketHandle s, IntPtr name, int namelen);

            [DllImport(WS2_32)]
            public extern static void WSASetLastError(int iError);

            [DllImport(WS2_32)]
            public extern static int WSAGetLastError();

            [DllImport(WS2_32, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
            internal static extern SocketError WSAStartup(
                                               [In] short wVersionRequested,
                                               [Out] out WSAData lpWSAData
                                               );

            [DllImport(WS2_32, ExactSpelling = true, SetLastError = true)]
            internal static extern SafeSocketHandle socket(int af, int type, int protocol);

            [DllImport(WS2_32, ExactSpelling = true, SetLastError = true)]
            internal static extern int bind(SafeSocketHandle s, IntPtr name, int namelen);

            [DllImport(WS2_32, ExactSpelling = true, SetLastError = true)]
            internal static extern int listen(SafeSocketHandle s, int backlog);

            [DllImport(WS2_32, ExactSpelling = true, SetLastError = true)]
            internal static extern int select(int nfds, [In, Out] fd_set readfds, [In, Out] fd_set writefds, [In, Out] fd_set exceptfds, ref TimeVal timeout);

            [DllImport(WS2_32, ExactSpelling = true, SetLastError = true)]
            internal static extern int send(SafeSocketHandle s, IntPtr buf, int len, int flags);

            [DllImport(WS2_32, ExactSpelling = true, SetLastError = true)]
            internal static extern int recv(SafeSocketHandle s, IntPtr buf, int len, int flags);

            [DllImport(WS2_32, ExactSpelling = true, SetLastError = true)]
            internal static extern NativeSockets.SafeSocketHandle accept(SafeSocketHandle s, IntPtr name, int namelen);

            [DllImport(WS2_32, ExactSpelling = true, SetLastError = true)]
            internal static extern int closesocket(IntPtr s);

            [DllImport(WS2_32, ExactSpelling = true, SetLastError = true)]
            internal static extern int shutdown(SafeSocketHandle s, int how);

            [DllImport(WS2_32, ExactSpelling = true, SetLastError = true)]
            internal static extern int getsockopt(SafeSocketHandle s, int level, int optname, IntPtr optval, ref int optlen);

            [DllImport(WS2_32, ExactSpelling = true, SetLastError = true)]
            internal static extern int setsockopt(SafeSocketHandle s, int level, int optname, IntPtr optval, int optlen);

            [DllImport(WS2_32, ExactSpelling = true, SetLastError = true)]
            internal static extern int getpeername(SafeSocketHandle s, IntPtr name, ref int namelen);

            [DllImport(WS2_32, ExactSpelling = true, SetLastError = true)]
            internal static extern int getsockname(SafeSocketHandle s, IntPtr name, ref int namelen);

            [DllImport(WS2_32, ExactSpelling = true, SetLastError = true)]
            internal static extern int sendto(SafeSocketHandle s, IntPtr buf, int len, int flags, IntPtr to, ref int tolen);

            [DllImport(WS2_32, ExactSpelling = true, SetLastError = true)]
            internal static extern int recvfrom(SafeSocketHandle s, IntPtr buf, int len, int flags, IntPtr from, ref int fromlen);

            [DllImport(WS2_32, ExactSpelling = true, SetLastError = true)]
            internal static extern int WSAEventSelect(SafeSocketHandle S, SafeWaitHandle hEventObject, int lNetworkEvents);
        }

        private class SocketData
        {
            internal Socket Socket;
            internal bool   IsSelectable = false;
            internal bool   IsSsl        = false;
            internal bool   IsClosing    = false;

            public SocketData(Socket socket)
            {                
                Socket = socket;
            }
        }

        bool _isInitialized = false;
        Dictionary<int, SocketData> _sockets = new Dictionary<int, SocketData>();
        int _handleNext;
        const int MaxSockets = 62; //for the call to WaitAny
        AutoResetEvent _eventSignalThread;
        AutoResetEvent _eventSsl = new AutoResetEvent(false);
        Thread _threadWaitForEvents;
        int _dwLastError = (int)SocketError.SocketError;

        private bool GetSocketData(int socketIndex, out SocketData socketData)
        {
            socketData = null;

            lock(_sockets)
            {
                if(!_sockets.ContainsKey(socketIndex)) return false;

                socketData = _sockets[socketIndex];
            }
            return true;
        }

        internal bool GetSocket(int socketIndex, out Socket socket)
        {
            SocketData sd;

            socket = null;
            
            if(!GetSocketData(socketIndex, out sd)) return false;

            socket = sd.Socket;

            return true;
        }

        internal void SetSslSocket(int socketIndex)
        {
            lock(_sockets)
            {
                if (_sockets.ContainsKey(socketIndex))
                {
                    _sockets[socketIndex].IsSsl = true;
                }
            }
        }

        internal void SignalThread()
        {
            if (_isInitialized)
            {
                _evtSocketSend.SendTo(new byte[] { 0 }, _evtSocketRecv.LocalEndPoint);
            }
        }

        Socket _evtSocketRecv;
        Socket _evtSocketSend;

        internal void ClearSocketEvent(int socket, bool fRead)
        {
            SocketData sd;
            if (_isInitialized)
            {
                if (GetSocketData(socket, out sd))
                {
                    if (fRead)
                    {
                        lock (m_read)
                        {
                            if (m_read.Contains(sd.Socket))
                            {
                                m_read.Remove(sd.Socket);
                            }
                        }
                    }
                    else
                    {
                        lock (m_write)
                        {
                            if (m_write.Contains(sd.Socket))
                            {
                                m_write.Remove(sd.Socket);
                            }
                        }
                    }
                }

                SignalThread();
            }
        }

        bool _socketsShuttingDown = false;

        List<Socket> m_read = new List<Socket>();
        List<Socket> m_write = new List<Socket>();
        List<Socket> m_excpt = new List<Socket>();

        void WaitForNetworkEvents()
        {
            List<Socket> read = new List<Socket>();
            List<Socket> write = new List<Socket>();
            List<Socket> excpt = new List<Socket>();


            read.Add(_evtSocketRecv);

            //check for emulator exit..
            while (!this.Emulator.IsShuttingDown && !_socketsShuttingDown)
            {
                Socket.Select(read, write, excpt, 0x7fffffff );

                if (_socketsShuttingDown) return;

                if (read.Contains(_evtSocketRecv))
                {
                    EndPoint ep = _evtSocketSend.LocalEndPoint;
                    byte[] data = new byte[256];

                    _evtSocketRecv.ReceiveFrom(data, ref ep);

                    read.Remove(_evtSocketRecv);
                }

                lock (_sockets)
                {
                    lock (m_read)
                    {
                        for (int i = 0; i < read.Count; i++)
                        {
                            m_read.Add(read[i]);
                        }
                    }
                    lock (m_write)
                    {
                        for (int i = 0; i < write.Count; i++)
                        {
                            m_write.Add(write[i]);
                        }
                    }
                    lock (m_excpt)
                    {
                        for (int i = 0; i < excpt.Count; i++)
                        {
                            m_excpt.Add(excpt[i]);
                        }
                    }

                    List<int> removeList = new List<int>();

                    read.Clear();
                    write.Clear();
                    excpt.Clear();

                    read.Add(_evtSocketRecv);

                    foreach (int handle in _sockets.Keys)
                    {
                        SocketData socket = _sockets[handle];

                        if (socket.IsClosing)
                        {
                            removeList.Add(handle);

                            Socket sock = socket.Socket;

                            lock (m_read)
                            {
                                if (m_read.Contains(sock))
                                {
                                    m_read.Remove(sock);
                                }
                            }
                            lock (m_write)
                            {
                                if (m_write.Contains(sock))
                                {
                                    m_write.Remove(sock);
                                }
                            }
                            lock (m_excpt)
                            {
                                if (m_excpt.Contains(sock))
                                {
                                    m_excpt.Remove(sock);
                                }
                            }

                            continue;
                        }

                        if (socket.IsSelectable)
                        {
                            try
                            {
                                Socket sock = socket.Socket;

                                if (!m_read.Contains(sock))
                                {
                                    read.Add(sock);
                                }

                                if (!m_write.Contains(sock))
                                {
                                    write.Add(sock);
                                }

                                if (!m_excpt.Contains(sock))
                                {
                                    excpt.Add(sock);
                                }
                            }
                            catch
                            {
                            }
                        }
                    }

                    foreach (int handle in removeList)
                    {
                        try
                        {
                            _sockets[handle].Socket.Close();
                        }
                        catch
                        {
                        }
                        _sockets.Remove(handle);
                    }
                }

                this.Emulator.SetSystemEvents(Events.SystemEvents.SOCKET);
            }
        }
        
        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                short wVersion = 0x0202;
                NativeSockets.WSAData wsaData = new NativeSockets.WSAData();

                SocketError err = NativeSockets.WSAStartup(wVersion, out wsaData);

                if (err != SocketError.Success)
                    throw new SocketException((int)err);

                _eventSignalThread = new AutoResetEvent(false);

                _evtSocketRecv = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _evtSocketRecv.Bind(new IPEndPoint(IPAddress.Loopback, 0));

                _evtSocketSend = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _evtSocketSend.Bind(new IPEndPoint(IPAddress.Loopback, 0));

                _threadWaitForEvents = new Thread(new ThreadStart(WaitForNetworkEvents));
                _threadWaitForEvents.Start();

                _isInitialized = true;
            }
        }

        int RegisterSocket(Socket socket)
        {
            return RegisterSocket(socket, false);
        }


        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        int RegisterSocket( Socket socket, bool isSelectable)
        {
            if (_sockets.Count == MaxSockets)
            {
                return ReturnError(SocketError.TooManyOpenSockets);
            }

            int handle;

            lock(_sockets)
            {
                handle = ++_handleNext;

                _sockets[handle] = new SocketData( socket );

                _sockets[handle].IsSelectable = (isSelectable || socket.ProtocolType != ProtocolType.Tcp);
            }
            
            SignalThread();

            return handle;
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        internal void UnregisterSocket(int handle)
        {
            lock(_sockets)
            {
                if(!_sockets.ContainsKey(handle)) return;

                _sockets[handle].IsClosing = true;
                
                SignalThread();
            }
        }

        internal int ReturnError(SocketError error)
        {
            _dwLastError = (int)error;
            return (int)SocketError.SocketError;
        }

        void StoreLastError()
        {
            _dwLastError = NativeSockets.WSAGetLastError();
        }

        TinyCLRSockets.fd_set IntPtrToTinyCLRFdSet(IntPtr fds)
        {
            TinyCLRSockets.fd_set fd_set = null;

            if (fds != IntPtr.Zero)
            {
                fd_set = new TinyCLRSockets.fd_set();

                Marshal.PtrToStructure(fds, fd_set);
            }

            return fd_set;
        }

        bool TinyCLRFdSetToFdSet(TinyCLRSockets.fd_set fd_setsrc, out List<Socket> fd_setDst)
        {
            bool fRes = true;
            fd_setDst = null;

            if (fd_setsrc != null)
            {
                fd_setDst = new List<Socket>();

                for (int i = 0; i < fd_setsrc.count; i++)
                {
                    SocketData sd;

                    if(!GetSocketData(fd_setsrc.fd_array[i], out sd))
                    {
                        fRes = false;
                        break;
                    }
                    
                    if(sd.IsSelectable)
                    {
                        fd_setDst.Add(sd.Socket);
                    }
                }
            }

            return fRes;
        }

        bool FilterFd_set(List<Socket> fdNative, TinyCLRSockets.fd_set fdCLR)
        {
            if (fdNative != null)
            {
                Debug.Assert(fdCLR != null);

                for (int i = 0; i < fdCLR.count; )
                {
                    bool fSet = false;
                    Socket sock = null;

                    if(!GetSocket(fdCLR.fd_array[i], out sock)) return false;

                    for (int j = 0; j < fdNative.Count; j++)
                    {
                        if (fdNative[j] == sock)
                        {
                            fSet = true;
                            break;
                        }
                    }

                    if (!fSet)
                    {
                        Array.Copy(fdCLR.fd_array, i + 1, fdCLR.fd_array, i, fdCLR.count - i - 1);
                        fdCLR.count--;
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            return true;
        }

        void TinyCLRFdSetToIntPtr(TinyCLRSockets.fd_set fd_set, IntPtr fds)
        {
            if (fds != IntPtr.Zero)
            {
                Marshal.StructureToPtr(fd_set, fds, true);
            }
        }

        private uint IPAddressToUint(IPAddress address)
        {
            uint ret = 0;

            if (address != null)
            {
                byte[] bytes = address.GetAddressBytes();

                Debug.Assert(address.AddressFamily == AddressFamily.InterNetwork);
                Debug.Assert(bytes != null);
                Debug.Assert(bytes.Length == 4);

                ret = BitConverter.ToUInt32(bytes, 0);
            }

            return ret;
        }

        private bool IsIPAddressIPv4(IPAddress address)
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
                return true;

            return false;
        }

        #region ISocketsDriver
        
        bool ISocketsDriver.Initialize()
        {
            EnsureInitialized();
            return true;
        }

        bool ISocketsDriver.Uninitialize()
        {
            if (_isInitialized)
            {
                _socketsShuttingDown = true;

                SignalThread();

                lock (_sockets)
                {
                    foreach (SocketData sd in _sockets.Values)
                    {
                        try
                        {
                            sd.Socket.Close();
                        }
                        catch
                        {
                        }
                    }
                    _sockets.Clear();
                }
                try
                {
                    _evtSocketRecv.Close();
                    _evtSocketSend.Close();
                }
                catch
                {
                }
                _isInitialized = false;
            }
            return true;
        }

        

        int ISocketsDriver.socket(int family, int type, int protocol)
        {
            Socket socket = null;

            try
            {
                socket = new Socket((AddressFamily)family, (SocketType)type, (ProtocolType)protocol);

                // always make emulator sockets blocking so that the NetworkStream will work (for SSL)
                socket.Blocking = true;
            }
            catch( SocketException se )
            {
                return ReturnError(se.SocketErrorCode);
            }
            catch
            {
                return ReturnError(SocketError.SocketError);
            }

            if (socket == null) return ReturnError(SocketError.SocketError);

            return RegisterSocket(socket, ((ProtocolType)protocol != ProtocolType.Tcp));
        }

        int ISocketsDriver.bind(int socket, IntPtr address, int addressLen)
        {
            SocketData sd = null;
            
            if (!GetSocketData(socket, out sd)) return ReturnError(SocketError.NotSocket);

            try
            {
                SockAddr sa = new SockAddr();

                sa.MarshalFromNative(address);

                if (sa.sin_port == 80)
                {
                    sa.sin_port++;
                }

                IPEndPoint ep = new IPEndPoint(sa.sin_addr, sa.sin_port);

                sd.Socket.Bind(ep);
            }
            catch (SocketException se)
            {
                return ReturnError(se.SocketErrorCode);
            }
            catch
            {
                return ReturnError(SocketError.SocketError);
            }

            return (int)SocketError.Success;
        }

        int ISocketsDriver.connect(int socket, IntPtr address, int addressLen)
        {
            Socket sock = null;
            
            if (!GetSocket(socket, out sock)) return ReturnError(SocketError.NotSocket);

            try
            {
                SockAddr sa = new SockAddr();

                sa.MarshalFromNative(address);

                sock.BeginConnect(new IPEndPoint(sa.sin_addr, sa.sin_port), new AsyncCallback(EndSockConnect), socket);
            }
            catch( SocketException se )
            {
                return ReturnError(se.SocketErrorCode);
            }
            catch
            {
                return ReturnError(SocketError.SocketError);
            }

            return ReturnError(SocketError.WouldBlock);
        }

        void EndSockConnect(IAsyncResult iar)
        {
            int socket = (int)iar.AsyncState;

            SocketData sd = null;
            
            if (!GetSocketData(socket, out sd)) return;

            try
            {
                sd.IsSelectable = true;
                sd.Socket.EndConnect(iar);
            }
            catch
            {
            }

            SignalThread();
        }

        int ISocketsDriver.send(int socket, IntPtr buf, int len, int flags)
        {
            SocketData sd = null;
            
            if (!GetSocketData(socket, out sd)) return ReturnError(SocketError.NotSocket);

            try
            {
                if (sd.Socket.Poll(0, SelectMode.SelectWrite))
                {
                    byte[] data = new byte[len];
                    int sent = 0;

                    Marshal.Copy(buf, data, 0, len);

                    sent = sd.Socket.Send(data, 0, len, (SocketFlags)flags);

                    ClearSocketEvent(socket, false);

                    return sent;
                }
            }
            catch (SocketException se)
            {
                return ReturnError((SocketError)se.ErrorCode);
            }

            return ReturnError(SocketError.WouldBlock);
        }

        int ISocketsDriver.recv(int socket, IntPtr buf, int len, int flags)
        {
            int ret = ReturnError(SocketError.WouldBlock);

            SocketData sd = null;
            
            if (!GetSocketData(socket, out sd)) return ReturnError(SocketError.NotSocket);

            try
            {
                if (sd.Socket.Poll(0, SelectMode.SelectRead))
                {
                    byte[] data = new byte[len];
                    int read = 0;
                    read = sd.Socket.Receive(data, 0, len, (SocketFlags)flags);

                    Marshal.Copy(data, 0, buf, read);

                    return read;
                }
            }
            catch (SocketException se)
            {
                return ReturnError((SocketError)se.ErrorCode);
            }
            finally
            {
                ClearSocketEvent(socket, true);
            }

            return ReturnError(SocketError.WouldBlock);
        }

        int ISocketsDriver.close(int socket)
        {
            int ret = (int)SocketError.Success;

            SocketData sd = null;
            
            if (!GetSocketData(socket, out sd)) return ReturnError(SocketError.NotSocket);

            try
            {
                if (sd.IsSsl)
                {
                    Hal.Ssl.CloseSocket(socket);
                }

                UnregisterSocket(socket);
            }
            catch (SocketException se)
            {
                ret = ReturnError(se.SocketErrorCode);
            }
            catch
            {
                ret = ReturnError(SocketError.SocketError);
            }

            return ret;
        }

        int ISocketsDriver.accept(int socket, IntPtr address, ref int addressLen)
        {
            Socket sock = null;
            
            if (!GetSocket(socket, out sock)) return ReturnError(SocketError.NotSocket);

            try
            {
                IAsyncResult iar = sock.BeginAccept(null, null);

                iar.AsyncWaitHandle.WaitOne();

                Socket sockAccept = sock.EndAccept(iar);

                int newSock = RegisterSocket(sockAccept, true);

                ClearSocketEvent(socket, true);

                return newSock;
            }
            catch (SocketException se)
            {
                return ReturnError(se.SocketErrorCode);
            }
            catch
            {
                return ReturnError(SocketError.SocketError);
            }
        }

        int ISocketsDriver.listen(int socket, int backlog)
        {
            SocketData sd = null;
            
            if (!GetSocketData(socket, out sd)) return ReturnError(SocketError.NotSocket);

            int ret = (int)SocketError.Success;

            try
            {
                sd.Socket.Listen(backlog);

                sd.IsSelectable = true;

                SignalThread();
            }
            catch (SocketException se)
            {
                ret = ReturnError(se.SocketErrorCode);
            }
            catch
            {
                ret = ReturnError(SocketError.SocketError);
            }

            return ret;
        }

        int ISocketsDriver.shutdown(int socket, int how)
        {
            int ret = (int)SocketError.Success;

            Socket sock = null;
            
            if (!GetSocket(socket, out sock)) return ReturnError(SocketError.NotSocket);

            try
            {
                sock.Shutdown( (SocketShutdown)how );

                SignalThread();
            }
            catch (SocketException se)
            {
                ret = ReturnError(se.SocketErrorCode);
            }
            catch
            {
                ret = ReturnError(SocketError.SocketError);
            }

            return ret;
        }
        
        int ISocketsDriver.getaddrinfo(IntPtr nodename, IntPtr servname, IntPtr hints, out IntPtr res)
        {
            res = IntPtr.Zero;

            int ret = NativeSockets.getaddrinfo(nodename, servname, hints, out res);

            StoreLastError();

            return ret;
        }

        void ISocketsDriver.freeaddrinfo(IntPtr ai)
        {
            NativeSockets.freeaddrinfo(ai);
        }

        int ISocketsDriver.ioctl(int socket, int cmd, ref int data)
        {
            Socket sock = null;
            
            if (!GetSocket(socket, out sock)) return ReturnError(SocketError.NotSocket);

            int ret = (int)SocketError.Success;

            try
            {
                if (cmd == NativeSockets.FIONBIO)
                {
                }
                else if (cmd == NativeSockets.FIONREAD)
                {
                    data = sock.Available;
                }
                else
                {
                    ret = (int)SocketError.SocketError;
                }
            }
            catch (SocketException se)
            {
                ret = ReturnError(se.SocketErrorCode);
            }
            catch
            {
                ret = ReturnError(SocketError.SocketError);
            }

            return ret;
        }

        int ISocketsDriver.getlasterror()
        {
            return _dwLastError;
        }

        private void InternalSelect(List<Socket> read, List<Socket> write, List<Socket> excpt)
        {
            if (read != null)
            {
                for (int i = read.Count - 1; i >= 0; i--)
                {
                    if (!m_read.Contains(read[i]))
                    {
                        read.RemoveAt(i);
                    }
                }
            }
            if (write != null)
            {
                for (int i = write.Count - 1; i >= 0; i--)
                {
                    if (!m_write.Contains(write[i]))
                    {
                        write.RemoveAt(i);
                    }
                }
            }
            if (excpt != null)
            {
                for (int i = excpt.Count - 1; i >= 0; i--)
                {
                    if (!m_excpt.Contains(excpt[i]))
                    {
                        excpt.RemoveAt(i);
                    }
                }
            }
        }


        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        int ISocketsDriver.select(int nfds, IntPtr readfds, IntPtr writefds, IntPtr exceptfds, ref TimeVal timeout)
        {
            List<Socket> readList;
            List<Socket> writeList;
            List<Socket> exceptList;

            int ret = 0;

            TinyCLRSockets.fd_set read = IntPtrToTinyCLRFdSet(readfds);
            TinyCLRSockets.fd_set write = IntPtrToTinyCLRFdSet(writefds);
            TinyCLRSockets.fd_set except = IntPtrToTinyCLRFdSet(exceptfds);

            if (!TinyCLRFdSetToFdSet(read, out readList)) return ReturnError(SocketError.NotSocket);
            if (!TinyCLRFdSetToFdSet(write, out writeList)) return ReturnError(SocketError.NotSocket);
            if (!TinyCLRFdSetToFdSet(except, out exceptList)) return ReturnError(SocketError.NotSocket);

            if ((readList   != null && readList.Count   > 0) ||
                (writeList  != null && writeList.Count  > 0) ||
                (exceptList != null && exceptList.Count > 0))
            {
                try
                {
                    //Socket.Select(readList, writeList, exceptList, timeout.tv_usec);
                    InternalSelect(readList, writeList, exceptList);
                }
                catch (SocketException se)
                {
                    ret = ReturnError(se.SocketErrorCode);
                }
            }

            if (!FilterFd_set(readList, read)) return ReturnError(SocketError.NotSocket);
            if (!FilterFd_set(writeList, write)) return ReturnError(SocketError.NotSocket);
            if (!FilterFd_set(exceptList, except)) return ReturnError(SocketError.NotSocket);

            TinyCLRFdSetToIntPtr(read, readfds);
            TinyCLRFdSetToIntPtr(write, writefds);
            TinyCLRFdSetToIntPtr(except, exceptfds);

            if(ret != (int)SocketError.SocketError)
            {
                if (readList   != null) ret += readList.Count;
                if (writeList  != null) ret += writeList.Count;
                if (exceptList != null) ret += exceptList.Count;
            }

            return ret;
        }

        int ISocketsDriver.setsockopt(int socket, int level, int optname, IntPtr optval, int optlen)
        {
            Socket sock = null;
            
            if (!GetSocket(socket, out sock)) return ReturnError(SocketError.NotSocket);

            int ret = (int)SocketError.Success;

            try
            {
                byte []data = new byte[optlen];

                if (level == (int)SocketOptionLevel.Socket && optname == (int)SocketOptionName.Linger && optlen == 4)
                {
                    int linger = Marshal.ReadInt32(optval);

                    LingerOption li = new LingerOption(linger != -1, linger < 0 ? 10 : linger);

                    Array.Copy(BitConverter.GetBytes((UInt16)(li.Enabled ? 1 : 0)), 0, data, 0, 2);
                    Array.Copy(BitConverter.GetBytes((UInt16)li.LingerTime       ), 0, data, 2, 2);
                }
                else
                {
                    Marshal.Copy(optval, data, 0, optlen);
                }

                sock.SetSocketOption((SocketOptionLevel)level, (SocketOptionName)optname, data);
            }
            catch (SocketException se)
            {
                ret = ReturnError(se.SocketErrorCode);
            }

            return ret;
        }

        int ISocketsDriver.getsockopt(int socket, int level, int optname, IntPtr optval, ref int optlen)
        {
            Socket sock = null;
            
            if (!GetSocket(socket, out sock)) return ReturnError(SocketError.NotSocket);

            int ret = (int)SocketError.Success;

            try
            {
                byte[] data = new byte[optlen];

                sock.GetSocketOption((SocketOptionLevel)level, (SocketOptionName)optname, data);
                
                if (level == (int)SocketOptionLevel.Socket && optname == (int)SocketOptionName.Linger && optlen == 4)
                {
                    // if linger option is off, then we translate to -1 for the MF option
                    if(data[0] == 0 && data[1] == 0)
                    {
                        Marshal.WriteInt32(optval, -1);
                    }
                    else
                    {
                        // convert back to a single integer value
                        Marshal.WriteInt32(optval, (int)BitConverter.ToInt16(data, 2));
                    }
                }
                else
                {
                    Marshal.Copy(data, 0, optval, optlen);
                }
            }
            catch (SocketException se)
            {
                ret = ReturnError(se.SocketErrorCode);
            }

            return ret;
        }

        int ISocketsDriver.getpeername(int socket, IntPtr name, ref int namelen)
        {
            Socket sock = null;
            
            if (!GetSocket(socket, out sock)) return ReturnError(SocketError.NotSocket);

            int ret = (int)SocketError.Success;

            try
            {
                IPEndPoint ep = sock.RemoteEndPoint as IPEndPoint;
                SockAddr addr = new SockAddr();

                addr.sin_addr = IPAddressToUint(ep.Address);
                addr.sin_family = (short)ep.AddressFamily;
                addr.sin_port   = (ushort)ep.Port;

                addr.MarshalToNative(name);
            }
            catch (SocketException se)
            {
                ret = ReturnError(se.SocketErrorCode);
            }

            return ret;
        }

        int ISocketsDriver.getsockname(int socket, IntPtr name, ref int namelen)
        {
            Socket sock = null;
            
            if (!GetSocket(socket, out sock)) return ReturnError(SocketError.NotSocket);

            int ret = (int)SocketError.Success;

            try
            {
                IPEndPoint ep = sock.LocalEndPoint as IPEndPoint;
                SockAddr addr = new SockAddr();

                if (IPAddress.IsLoopback(ep.Address))
                {
                    addr.sin_addr = IPAddressToUint(ep.Address);
                }
                else
                {
                    IPHostEntry ipEntry = Dns.GetHostEntry(ep.Address);

                    for (int i = 0; i < ipEntry.AddressList.Length; i++)
                    {
                        if (ipEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                        {
                            addr.sin_addr = IPAddressToUint(ipEntry.AddressList[i]);
                        }
                    }
                }
                
                addr.sin_family = (short)ep.AddressFamily;
                addr.sin_port = (ushort)ep.Port;

                addr.MarshalToNative(name);
            }
            catch (SocketException se)
            {
                ret = ReturnError(se.SocketErrorCode);
            }

            return ret;
        }

        int ISocketsDriver.sendto(int socket, IntPtr buf, int len, int flags, IntPtr to, ref int tolen)
        {
            SocketData sd = null;
            
            if (!GetSocketData(socket, out sd)) return ReturnError(SocketError.NotSocket);

            try
            {
                if (sd.Socket.Poll(0, SelectMode.SelectWrite))
                {
                    byte[] data = new byte[len];
                    int sent = 0;

                    Marshal.Copy(buf, data, 0, len);

                    SockAddr toAddr = new SockAddr();

                    toAddr.MarshalFromNative(to);

                    IPEndPoint toEP = new IPEndPoint(toAddr.sin_addr, toAddr.sin_port);

                    sent = sd.Socket.SendTo(data, 0, len, (SocketFlags)flags, toEP);

                    ClearSocketEvent(socket, false);

                    return sent;
                }
            }
            catch (SocketException se)
            {
                return ReturnError((SocketError)se.ErrorCode);
            }

            return ReturnError(SocketError.WouldBlock);
        }

        int ISocketsDriver.recvfrom(int socket, IntPtr buf, int len, int flags, IntPtr from, ref int fromlen)
        {
            SocketData sd = null;
            
            if (!GetSocketData(socket, out sd)) return ReturnError(SocketError.NotSocket);

            try
            {
                if (sd.Socket.Poll(0, SelectMode.SelectRead))
                {
                    byte[] data = new byte[len];
                    int read = 0;

                    SockAddr fromAddr = new SockAddr();

                    fromAddr.MarshalFromNative(from);

                    EndPoint fromEP = new IPEndPoint(fromAddr.sin_addr, fromAddr.sin_port);


                    read = sd.Socket.ReceiveFrom(data, 0, len, (SocketFlags)flags, ref fromEP);

                    Marshal.Copy(data, 0, buf, read);

                    fromAddr.sin_addr = IPAddressToUint(((IPEndPoint)fromEP).Address);

                    fromAddr.sin_port = (ushort)((IPEndPoint)fromEP).Port;

                    fromAddr.sin_family = (short)((IPEndPoint)fromEP).AddressFamily;

                    fromAddr.MarshalToNative(from);

                    return read;
                }
            }
            catch (SocketException se)
            {
                return ReturnError((SocketError)se.ErrorCode);

            }
            finally
            {
                ClearSocketEvent(socket, true);
            }

            return ReturnError(SocketError.WouldBlock);
        }

        uint ISocketsDriver.NetworkAdapterCount 
        {
            get
            {
                return (uint)NetworkInterface.GetAllNetworkInterfaces().Length;
            }
        }

        int ISocketsDriver.LoadAdapterConfiguration(uint interfaceIndex, ref NetworkAdapterConfiguration config)
        {
            NetworkInterface networkInterface = NetworkInterface.GetAllNetworkInterfaces()[interfaceIndex];
            IPInterfaceProperties props = networkInterface.GetIPProperties();
            IPv4InterfaceProperties ipv4props = null;

            try
            {
                ipv4props = props.GetIPv4Properties();
            }
            catch(NetworkInformationException)
            {
            }

            
            if (ipv4props != null)
            {
                if (props.IsDynamicDnsEnabled)
                {
                    config.flags |= NetworkAdapterConfiguration.FLAGS_DYNAMIC_DNS;
                }

                if (ipv4props.IsDhcpEnabled)
                {
                    config.flags |= NetworkAdapterConfiguration.FLAGS_DHCP;
                }

                for (int iAddress = 0; iAddress < props.UnicastAddresses.Count; iAddress++)
                {
                    UnicastIPAddressInformation unicastAddress = props.UnicastAddresses[iAddress];

                    if (IsIPAddressIPv4(unicastAddress.Address))
                    {
                        config.ipaddr = this.IPAddressToUint(unicastAddress.Address);
                        config.subnetmask = this.IPAddressToUint(unicastAddress.IPv4Mask);

                        break;
                    }
                }

                for (int iAddress = 0; iAddress < props.GatewayAddresses.Count; iAddress++)
                {
                    GatewayIPAddressInformation gatewayAddress = props.GatewayAddresses[iAddress];

                    if (IsIPAddressIPv4(gatewayAddress.Address))
                    {
                        config.gateway = this.IPAddressToUint(gatewayAddress.Address);

                        break;
                    }
                }

                int dnsServerIndex = 0;
                for (int iAddress = 0; iAddress < props.DnsAddresses.Count && dnsServerIndex < 2; iAddress++)
                {
                    IPAddress address = props.DnsAddresses[iAddress];

                    if (IsIPAddressIPv4(address))
                    {
                        switch(dnsServerIndex)
                        {
                            case 0:
                                config.dnsServer1 = this.IPAddressToUint(address);
                                break;
                            case 1:
                                config.dnsServer2 = this.IPAddressToUint(address);
                                break;
                        }

                        dnsServerIndex++;
                    }
                }

                PhysicalAddress macAddress = networkInterface.GetPhysicalAddress();
                byte[] macBytes = macAddress.GetAddressBytes();
                
                config.networkInterfaceType = (uint)networkInterface.NetworkInterfaceType;                
                config.networkAddressLen = (uint)macBytes.Length;

                unsafe
                {
                    fixed (byte* p = config.networkAddressBuf)
                    {
                        Marshal.Copy(macBytes, 0, new IntPtr(p), macBytes.Length);
                    }
                }
            }
        
            return 0;
        }

        int ISocketsDriver.UpdateAdapterConfiguration(uint interfaceIndex, uint updateFlags, ref NetworkAdapterConfiguration config)
        {
            //CLR_E_FAIL.  Update with better error?
            unchecked { return (int)(uint)CLR_ERRORCODE.CLR_E_FAIL; }
        }

        #endregion
    }

    internal class ComPortSocket : ComPortToAsyncStream
    {
        Socket _socket;

        public ComPortSocket(Socket socket)
            : base(new NetworkStream(socket))
        {
            _socket = socket;         
        }

        public override int AvailableBytes
        {
            get
            {
                return _socket.Available;
            }
        }
    }

    internal class TcpListenerForDebugAPI : EmulatorComponent
    {
        //defined in unmanaged code somewhere??
        //also in debugger.dll somewhere??
        //Should Emulator depend on Debugger.dll?  Can then use PortDefinition_Tcp.DefaultPort
        //and use Streams, instead of copy/pasted code?
        const int DEBUG_PORT = 26000;

        TcpListener _tcpListener;
        TcpClient _tcpClient;
        int _port = DEBUG_PORT;
        ComPortHandle _comPortHandle = ComPortHandle.DebugPort;

        public override void SetupComponent()
        {
            //Is this right???
            base.SetupComponent();

            _tcpListener = new TcpListener(IPAddress.Any, _port);

            this.Emulator.ComPortCollection.CollectionChanged += new System.ComponentModel.CollectionChangeEventHandler(OnComPortCollectionChanged);

            StartListener();
        }

        private void OnComPortCollectionChanged(object sender, CollectionChangeEventArgs args)
        {
            if (args.Action == CollectionChangeAction.Remove)
            {
                ComPort comPort = (ComPort)args.Element;
                ComPortHandle handle = comPort.ComPortHandle;

                if (handle == _comPortHandle)
                {
                    StartListener();
                }
            }
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        void StartListener()
        {
            try
            {
                //should this support both debug ports? Or just one per port?
                _tcpListener.Start(1);

                _tcpListener.BeginAcceptSocket(new AsyncCallback(AcceptSocket), null);
            }
            catch
            {
            }
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public override void UninitializeComponent()
        {
            base.UninitializeComponent();

            try
            {
                _tcpListener.Stop();

                if (_tcpClient != null)
                {
                    _tcpClient.Close();
                    _tcpClient = null;
                }
            }
            catch
            {
            }
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        private void AcceptSocket(IAsyncResult ar)
        {
            try
            {
                // End the operation and display the received data on 
                // the console.
                Socket client = _tcpListener.EndAcceptSocket(ar);

                client.NoDelay = true;

                ComPortSocket comPortSocket = new ComPortSocket(client/*, this*/);

                comPortSocket.ComPortHandle = _comPortHandle;

                //This doesn't seem right.  ComPort should have an Initialize that starts read? 
                //who cares if the Device calls initialize??????

                comPortSocket.DeviceInitialize();

                this.Emulator.RegisterComponent(comPortSocket);

                _tcpListener.Stop();
            }
            catch
            {
            }
        }
    }    
}
