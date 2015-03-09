using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Runtime.CompilerServices;
using System.IO;
using System.Collections;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;

namespace Microsoft.SPOT.Emulator
{
    internal static class Utility
    {
        public static bool FImplies(bool premise, bool statement)
        {
            return !premise || statement;
        }

        public static byte[] MarshalBytes(IntPtr ptr, int size)
        {
            byte[] buffer = new byte[size];
            Marshal.Copy(ptr, buffer, 0, size);

            return buffer;
        }

        public static void MarshalBytes(IntPtr dest, int destCount, int offset, byte[] buf )
        {
            int ct = Math.Min(destCount, buf.Length - offset);

            if (ct > 0)
            {
                Marshal.Copy(buf, offset, dest, ct);
            }
        }

        public static ushort[] MarshalUshort(IntPtr ptr, int size)
        {
            ushort[] buffer = new ushort[size];
            
            for (int i = 0; i < size; i++)
            {
                buffer[i] = (ushort)Marshal.ReadInt16(ptr, i*2);
            }

            return buffer;
        }
                    
        public static void MarshalUshort(IntPtr dest, int destCount, int offset, ushort[] buf)
        {
            int ct = Math.Min(destCount, buf.Length - offset);

            for (int i = 0; i < ct; i++)
            {
                Marshal.WriteInt16(dest, 2 * i, (short)buf[i + offset]);
            }
        }

        static public void SetCurrentThreadAffinity()
        {
            IntPtr process = Native.GetCurrentProcess();
            Debug.Assert(process != IntPtr.Zero);
            UIntPtr lpProcessAffinityMask, lpSystemAffinityMask;

            bool ret = Native.GetProcessAffinityMask(process, out lpProcessAffinityMask, out lpSystemAffinityMask);
            Debug.Assert(ret);

            //find first available processor
            ulong affinity = lpProcessAffinityMask.ToUInt64();
            ulong affinityMask = 1;
            Debug.Assert(affinity != 0);

            while ((affinity & affinityMask) == 0)
            {
                affinityMask <<= 1;
            }

            IntPtr thread = Native.GetCurrentThread();
            Debug.Assert(thread != IntPtr.Zero);

            Native.SetThreadAffinityMask(thread, (UIntPtr)affinityMask);
        }
    }

    #region CorDebug.dll helpers -- perhaps move this to Debugger.dll??
    internal class COM_HResults
    {
        public const int S_OK = 0x00000000;
    }
    #endregion

    #region Debugger.dll helpers
    internal class FifoBuffer
    {
        byte[] m_buffer;
        int m_offset;
        int m_count;
        object m_lock;
        ManualResetEvent m_ready;

        public FifoBuffer()
        {
            m_buffer = new byte[1024];
            m_offset = 0;
            m_count = 0;
            m_lock = new object();
            m_ready = new ManualResetEvent(false);
        }

        public WaitHandle WaitHandle
        {
            get { return m_ready; }
        }

        public int Read(byte[] buf, int offset, int count)
        {
            int countRequested = count;

            lock (m_lock)
            {
                int len = m_buffer.Length;

                while (m_count > 0 && count > 0)
                {
                    int avail = m_count; if (avail + m_offset > len) avail = len - m_offset;

                    if (avail > count) avail = count;

                    Array.Copy(m_buffer, m_offset, buf, offset, avail);

                    m_offset += avail; if (m_offset == len) m_offset = 0;
                    offset += avail;

                    m_count -= avail;
                    count -= avail;
                }

                if (m_count == 0)
                {
                    //
                    // No pending data, resync to the beginning of the buffer.
                    //
                    m_offset = 0;

                    m_ready.Reset();
                }
            }

            return countRequested - count;
        }

        public void Write(byte[] buf, int offset, int count)
        {
            lock (m_lock)
            {
                if (count == 0)
                {
                    return;
                }

                while (count > 0)
                {
                    int len = m_buffer.Length;
                    int avail = len - m_count;

                    if (avail == 0) // Buffer full. Expand it.
                    {
                        byte[] buffer = new byte[len * 2];

                        //
                        // Double the buffer and copy all the data to the left side.
                        //
                        Array.Copy(m_buffer, m_offset, buffer, 0, len - m_offset);
                        Array.Copy(m_buffer, 0, buffer, len - m_offset, m_offset);

                        m_buffer = buffer;
                        m_offset = 0;
                        len *= 2;
                        avail = len;
                    }

                    int offsetWrite = m_offset + m_count; if (offsetWrite >= len) offsetWrite -= len;

                    if (avail + offsetWrite > len) avail = len - offsetWrite;

                    if (avail > count) avail = count;

                    Array.Copy(buf, offset, m_buffer, offsetWrite, avail);

                    offset += avail;
                    m_count += avail;
                    count -= avail;
                }
            }

            m_ready.Set();
        }

        public int Available
        {
            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            get
            {
                return m_count;
            }
        }
    }

    internal sealed class Native
    {
        // Error codes (not HRESULTS), from winerror.h
        public const int ERROR_BROKEN_PIPE = 109;
        public const int ERROR_NO_DATA = 232;
        public const int ERROR_HANDLE_EOF = 38;
        public const int ERROR_INVALID_PARAMETER = 87;
        public const int ERROR_OPERATION_ABORTED = 995;
        public const int ERROR_IO_PENDING = 997;
        public const int ERROR_INVALID_HANDLE = 0x6;
        public const int ERROR_PIPE_BUSY = 231;
        public const int ERROR_PIPE_NOT_CONNECTED = 233;
        public const int ERROR_PIPE_CONNECTED = 535;
        public const int ERROR_PIPE_LISTENING = 536;

        public const uint PIPE_ACCESS_OUTBOUND = 0x00000002;
        public const uint PIPE_ACCESS_DUPLEX = 0x00000003;
        public const uint PIPE_ACCESS_INBOUND = 0x00000001;

        public const uint PIPE_WAIT = 0x00000000;
        public const uint PIPE_NOWAIT = 0x00000001;
        public const uint PIPE_READMODE_BYTE = 0x00000000;
        public const uint PIPE_READMODE_MESSAGE = 0x00000002;
        public const uint PIPE_TYPE_BYTE = 0x00000000;
        public const uint PIPE_TYPE_MESSAGE = 0x00000004;

        public const uint PIPE_CLIENT_END = 0x00000000;
        public const uint PIPE_SERVER_END = 0x00000001;

        public const uint PIPE_UNLIMITED_INSTANCES = 255;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct COMMTIMEOUTS
        {
            public int ReadIntervalTimeout;         /* Maximum time between read chars. */
            public int ReadTotalTimeoutMultiplier;  /* Multiplier of characters.        */
            public int ReadTotalTimeoutConstant;    /* Constant in milliseconds.        */
            public int WriteTotalTimeoutMultiplier; /* Multiplier of characters.        */
            public int WriteTotalTimeoutConstant;   /* Constant in milliseconds.        */

            public void Initialize()
            {
                ReadIntervalTimeout = 0;
                ReadTotalTimeoutMultiplier = 0xFFFF;
                ReadTotalTimeoutConstant = 0xFFFF;
                WriteTotalTimeoutMultiplier = 0xFFFF;
                WriteTotalTimeoutConstant = 0xFFFF;
            }
        }

        public const int DUPLICATE_SAME_ACCESS = 0x00000002;
        public const int FILE_ATTRIBUTE_NORMAL = 0x00000080;
        public const int FILE_FLAG_OVERLAPPED = 0x40000000;
        public const int FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000;
        public const int FILE_FLAG_WRITE_THROUGH = unchecked((int)0x80000000);

        public const int GENERIC_READ = unchecked((int)0x80000000);
        public const int GENERIC_WRITE = (0x40000000);
        public const int GENERIC_EXECUTE = (0x20000000);
        public const int GENERIC_ALL = (0x10000000);

        public const int FILE_DEVICE_UNKNOWN = (0x00000022);
        public const int METHOD_BUFFERED = (0x00000000);
        public const int FILE_ANY_ACCESS = (0x00000000);

        public const int DIGCF_PRESENT = (0x00000002);
        public const int DIGCF_DEVICEINTERFACE = (0x00000010);

        public const int NMPWAIT_USE_DEFAULT_WAIT = 0x00000000;
        public const int NMPWAIT_WAIT_FOREVER = unchecked((int)0xffffffff);

        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);  // WinBase.h
        public static readonly IntPtr NULL = IntPtr.Zero;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct DCB
        {
            //
            // DTR Control Flow Values.
            //
            public const uint DTR_CONTROL_DISABLE = 0x00000000;
            public const uint DTR_CONTROL_ENABLE = 0x00000010;
            public const uint DTR_CONTROL_HANDSHAKE = 0x00000020;

            //
            // RTS Control Flow Values
            //
            public const uint RTS_CONTROL_DISABLE = 0x00000000;
            public const uint RTS_CONTROL_ENABLE = 0x00001000;
            public const uint RTS_CONTROL_HANDSHAKE = 0x00002000;
            public const uint RTS_CONTROL_TOGGLE = 0x00003000;

            public const uint mask_fBinary = 0x00000001;
            public const uint mask_fParity = 0x00000002;
            public const uint mask_fOutxCtsFlow = 0x00000004;
            public const uint mask_fOutxDsrFlow = 0x00000008;
            public const uint mask_fDtrControl = 0x00000030;
            public const uint mask_fDsrSensitivity = 0x00000040;
            public const uint mask_fTXContinueOnXoff = 0x00000080;
            public const uint mask_fOutX = 0x00000100;
            public const uint mask_fInX = 0x00000200;
            public const uint mask_fErrorChar = 0x00000400;
            public const uint mask_fNull = 0x00000800;
            public const uint mask_fRtsControl = 0x00003000;
            public const uint mask_fAbortOnError = 0x00004000;
            public const uint mask_fDummy2 = 0xFFFF8000;


            public uint DCBlength;            /* sizeof(DCB)                          */
            public uint BaudRate;             /* Baudrate at which running            */
            public uint __BitField;
            public short wReserved;            /* Not currently used                   */
            public short XonLim;               /* Transmit X-ON threshold              */
            public short XoffLim;              /* Transmit X-OFF threshold             */
            public byte ByteSize;             /* Number of bits/byte, 4-8             */
            public byte Parity;               /* 0-4=None,Odd,Even,Mark,Space         */
            public byte StopBits;             /* 0,1,2 = 1, 1.5, 2                    */
            public byte XonChar;              /* Tx and Rx X-ON character             */
            public byte XoffChar;             /* Tx and Rx X-OFF character            */
            public byte ErrorChar;            /* Error replacement char               */
            public byte EofChar;              /* End of Input character               */
            public byte EvtChar;              /* Received Event character             */
            public short wReserved1;           /* Fill for now.                        */

            public void Initialize()
            {
                DCBlength = (uint)Marshal.SizeOf(this);
                BaudRate = 0;
                __BitField = 0;
                wReserved = 0;
                XonLim = 0;
                XoffLim = 0;
                ByteSize = 0;
                Parity = 0;
                StopBits = 0;
                XonChar = 0;
                XoffChar = 0;
                ErrorChar = 0;
                EofChar = 0;
                EvtChar = 0;
                wReserved1 = 0;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct COMSTAT
        {
            public uint __BitField;     // DWORD fCtsHold   :1;
            public uint cbInQue;         // DWORD cbInQue;
            public uint cbOutQue;        // DWORD cbOutQue;

            public void Initialize()
            {
                __BitField = 0;
                cbInQue = 0;
                cbOutQue = 0;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid InterfaceClassGuid;
            public int Flags;
            private IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct SP_DEVINFO_DATA
        {
            public int cbSize;
            public Guid ClassGuid;
            public int DevInst;
            private IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public int cbSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
            public string DevicePath;
        }

        internal class PipeHandle : CriticalHandleMinusOneIsInvalid
        {
            internal PipeHandle() : base() { }

            internal PipeHandle(IntPtr handle)
                : base()
            {
                SetHandle(handle);
            }

            public IntPtr Handle
            {
                get { return handle; }
            }

            protected override bool ReleaseHandle()
            {
                return Native.CloseHandle(handle);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class SECURITY_ATTRIBUTES
        {
            internal int nLength = 0;
            internal IntPtr lpSecurityDescriptor = IntPtr.Zero;
            internal int bInheritHandle = 0;
        }

        public const String KERNEL32 = "kernel32.dll";
        public const String SETUPAPI = "setupapi.dll";
        public const String ADVAPI32 = "advapi32.dll";

        [DllImport(KERNEL32)]
        public static extern int GetLastError();

        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetCommTimeouts(IntPtr handle, [In, Out] ref COMMTIMEOUTS ver);

        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool ClearCommError(IntPtr handle, out uint errors, ref COMSTAT ver);

        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetCommState(IntPtr handle, [In, Out] ref DCB dcb);

        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetCommState(IntPtr handle, [In, Out] ref DCB dcb);

        [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern SafeFileHandle CreateFile(string lpFileName,
                                                int dwDesiredAccess,
                                                FileShare dwShareMode,
                                                IntPtr lpSecurityAttributes,
                                                FileMode dwCreationDisposition,
                                                int dwFlagsAndAttributes,
                                                IntPtr hTemplateFile);

        [DllImport(KERNEL32, SetLastError = true)]
        public unsafe static extern bool DeviceIoControl(IntPtr hDevice,
                                                              int dwIoControlCode,
                                                              byte* lpInBuffer,
                                                              int nInBufferSize,
                                                              byte* lpOutBuffer,
                                                              int nOutBufferSize,
                                                          out int lpBytesReturned,
                                                              NativeOverlapped* lpOverlapped);

        [DllImport(KERNEL32)]
        public static extern bool DuplicateHandle(IntPtr hSourceProcessHandle,  // handle to source process
                                                       IntPtr hSourceHandle,  // handle to duplicate
                                                       IntPtr hTargetProcessHandle,  // handle to target process
                                                   out IntPtr lpTargetHandle,  // duplicate handle
                                                       int dwDesiredAccess,  // requested access
                                                       bool bInheritHandle,  // handle inheritance option
                                                       int dwOptions); // optional actions


        [DllImport(KERNEL32)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport(KERNEL32, SetLastError = true)]
        public unsafe static extern bool ReadFile(IntPtr handle,
                                                       byte* bytes,
                                                       int numBytesToRead,
                                                   out int numBytesRead,
                                                       NativeOverlapped* overlapped);

        [DllImport(KERNEL32, SetLastError = true)]
        public unsafe static extern bool WriteFile(IntPtr handle,
                                                        byte* bytes,
                                                        int numBytesToWrite,
                                                    out int numBytesWritten,
                                                        NativeOverlapped* lpOverlapped);

        [DllImport(KERNEL32, SetLastError = true)]
        public unsafe static extern bool PeekNamedPipe(IntPtr handle,
                                                            byte* buffer,
                                                            int bufferSize,
                                                        out int bytesRead,
                                                        out int totalBytesAvail,
                                                        out int bytesLeftThisMessage);

        [DllImport(KERNEL32)]
        public static extern bool QueryPerformanceCounter(out long value);

        [DllImport(KERNEL32)]
        public static extern bool QueryPerformanceFrequency(out long value);

        [DllImport(SETUPAPI, SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid,
                                                             string Enumerator,
                                                             int hwndParent,
                                                             int Flags);

        [DllImport(SETUPAPI, SetLastError = true)]
        public unsafe static extern bool SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet,
                                                                          int DeviceInfoData,
                                                                      ref Guid InterfaceClassGuid,
                                                                          int MemberIndex,
                                                                      ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

        [DllImport(SETUPAPI, SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet,
                                                                               ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData,
                                                                   [In, Out] ref SP_DEVICE_INTERFACE_DETAIL_DATA DeviceInterfaceDetailData,
                                                                                  int DeviceInterfaceDetailDataSize,
                                                                                  int RequiredSize,
                                                                                  int DeviceInfoData);

        [DllImport(SETUPAPI, SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport(KERNEL32, SetLastError = true)]
        public static extern IntPtr CreateNamedPipe(string lpName,             // pipe name
                                                 uint dwOpenMode,            // pipe open mode
                                                 uint dwPipeMode,             // pipe-specific modes
                                                 uint nMaxInstances,          // maximum number of instances
                                                 uint nOutBufferSize,         // output buffer size
                                                 uint nInBufferSize,          // input buffer size
                                                 uint nDefaultTimeOut,       // time-out interval
                                                 SECURITY_ATTRIBUTES pipeSecurityDescriptor	//SecurityAttributes attr					 // SD
                                                 );


        [DllImport(KERNEL32, SetLastError = true)]
        public unsafe static extern bool ConnectNamedPipe(IntPtr hNamedPipe,        // handle to named pipe
                                            NativeOverlapped* lpOverlapped  // overlapped structure
                                            );

        public unsafe static bool ConnectNamedPipe(IntPtr hNamedPipe,        // handle to named pipe
                                                    Overlapped lpOverlapped  // overlapped structure
                                                    )
        {
            NativeOverlapped* overlappedPack = lpOverlapped.Pack(null, null);
            bool fRet = false;

            if (overlappedPack != null)
            {
                fRet = ConnectNamedPipe(hNamedPipe, overlappedPack);

                Overlapped.Free(overlappedPack);
            }

            return fRet;
        }

        [DllImport(KERNEL32, SetLastError = true)]
        public static extern bool DisconnectNamedPipe(IntPtr hNamedPipe);

        [DllImport(KERNEL32, SetLastError = true)]
        public static extern bool FlushFileBuffers(IntPtr hFile);

        [DllImport(ADVAPI32, SetLastError = true)]
        public static extern bool ImpersonateNamedPipeClient(IntPtr hNamedPipe        // handle to named pipe
                                                       );

        [DllImport(ADVAPI32)]
        public static extern bool RevertToSelf();

        [DllImport(KERNEL32, SetLastError = true)]
        public static extern bool WaitNamedPipe(String name,
                                                 int timeout);

        [DllImport(KERNEL32, SetLastError = true)]
        public static extern UIntPtr SetThreadAffinityMask(IntPtr hThread,UIntPtr dwThreadAffinityMask);

        [DllImport(KERNEL32, SetLastError = true)]
        public static extern IntPtr GetCurrentThread();

        [DllImport(KERNEL32, SetLastError = true)]
        public static extern bool GetProcessAffinityMask(IntPtr hProcess, out UIntPtr lpProcessAffinityMask, out UIntPtr lpSystemAffinityMask);

        [DllImport(KERNEL32, SetLastError = true)]
        public static extern IntPtr GetCurrentProcess();
                
        [DllImport(KERNEL32, SetLastError = true)]
        static extern bool SetProcessAffinityMask(IntPtr hProcess, UIntPtr dwProcessAffinityMask);


        static public void ThrowIOException(string msg)
        {
            int errorCode = Marshal.GetLastWin32Error();

            throw new IOException(msg, errorCode);
        }

        static public int ControlCode(int DeviceType, int Function, int Method, int Access)
        {
            return ((DeviceType) << 16) | ((Access) << 14) | ((Function) << 2) | (Method);
        }
    }
    #endregion

    #region Streams

    // This is an internal object implementing IAsyncResult with fields
    // for all of the relevant data necessary to complete the IO operation.
    // This is used by AsyncFSCallback and all of the async methods.
    unsafe internal class AsyncFileStream_AsyncResult : IAsyncResult
    {
        private unsafe static readonly IOCompletionCallback s_callback = new IOCompletionCallback(DoneCallback);

        internal AsyncCallback m_userCallback;
        internal Object m_userStateObject;
        internal ManualResetEvent m_waitHandle;

        internal GCHandle m_bufferHandle;    // GCHandle to pin byte[].
        internal bool m_bufferIsPinned;  // Whether our m_bufferHandle is valid.

        internal bool m_isWrite;         // Whether this is a read or a write
        internal bool m_isComplete;
        internal bool m_EndXxxCalled;    // Whether we've called EndXxx already.
        internal int m_numBytes;        // number of bytes read OR written
        internal int m_errorCode;
        internal NativeOverlapped* m_overlapped;

        internal AsyncFileStream_AsyncResult(AsyncCallback userCallback, Object stateObject, bool isWrite)
        {
            m_userCallback = userCallback;
            m_userStateObject = stateObject;
            m_waitHandle = new ManualResetEvent(false);

            m_isWrite = isWrite;

            Overlapped overlapped = new Overlapped(0, 0, IntPtr.Zero, this);

            m_overlapped = overlapped.Pack(s_callback, null);
        }

        public virtual Object AsyncState
        {
            get { return m_userStateObject; }
        }

        public bool IsCompleted
        {
            get { return m_isComplete; }
            set { m_isComplete = value; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return m_waitHandle; }
        }

        public bool CompletedSynchronously
        {
            get { return false; }
        }

        internal void SignalCompleted()
        {
            AsyncCallback userCallback = null;

            lock (this)
            {
                if (m_isComplete == false)
                {
                    userCallback = m_userCallback;

                    ManualResetEvent wh = m_waitHandle;
                    if (wh != null && wh.Set() == false)
                    {
                        Native.ThrowIOException(string.Empty);
                    }

                    // Set IsCompleted to true AFTER we've signalled the WaitHandle!
                    // Necessary since we close the WaitHandle after checking IsCompleted,
                    // so we could cause the SetEvent call to fail.
                    m_isComplete = true;

                    ReleaseMemory();
                }
            }

            if (userCallback != null)
            {
                userCallback(this);
            }
        }

        internal void WaitCompleted()
        {
            ManualResetEvent wh = m_waitHandle;
            if (wh != null)
            {
                if (m_isComplete == false)
                {
                    wh.WaitOne();
                    // There's a subtle race condition here.  In AsyncFSCallback,
                    // I must signal the WaitHandle then set _isComplete to be true,
                    // to avoid closing the WaitHandle before AsyncFSCallback has
                    // signalled it.  But with that behavior and the optimization
                    // to call WaitOne only when IsCompleted is false, it's possible
                    // to return from this method before IsCompleted is set to true.
                    // This is currently completely harmless, so the most efficient
                    // solution of just setting the field seems like the right thing
                    // to do.
                    m_isComplete = true;
                }
                wh.Close();
            }
        }

        internal NativeOverlapped* OverlappedPtr
        {
            get { return m_overlapped; }
        }

        internal unsafe void ReleaseMemory()
        {
            if (m_overlapped != null)
            {
                Overlapped.Free(m_overlapped);
                m_overlapped = null;
            }

            UnpinBuffer();
        }

        internal void PinBuffer(byte[] buffer)
        {
            m_bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            m_bufferIsPinned = true;
        }

        internal void UnpinBuffer()
        {
            if (m_bufferIsPinned)
            {
                m_bufferHandle.Free();
                m_bufferIsPinned = false;
            }
        }

        // this callback is called by a free thread in the threadpool when the IO operation completes.
        unsafe private static void DoneCallback(uint errorCode, uint numBytes, NativeOverlapped* pOverlapped)
        {
            if (errorCode == Native.ERROR_OPERATION_ABORTED)
            {
                numBytes = 0;
                errorCode = 0;

                return;
            }

            // Unpack overlapped
            Overlapped overlapped = Overlapped.Unpack(pOverlapped);
            // Free the overlapped struct in EndRead/EndWrite.

            // Extract async result from overlapped
            AsyncFileStream_AsyncResult asyncResult = (AsyncFileStream_AsyncResult)overlapped.AsyncResult;


            asyncResult.m_numBytes = (int)numBytes;
            asyncResult.m_errorCode = (int)errorCode;

            asyncResult.SignalCompleted();
        }
    }

    internal class GenericAsyncStream : System.IO.Stream, IDisposable
    {
        protected readonly SafeHandle m_handle;
        protected ArrayList m_outstandingRequests;

        protected GenericAsyncStream(SafeHandle handle)
        {
            System.Diagnostics.Debug.Assert(handle != null);

            m_handle = handle;

            if (ThreadPool.BindHandle(m_handle) == false)
            {
                throw new IOException("BindHandle Failed");
            }

            m_outstandingRequests = ArrayList.Synchronized(new ArrayList());
        }

        ~GenericAsyncStream()
        {
            Dispose(false);
        }

        public void CancelPendingIO()
        {
            lock (m_outstandingRequests.SyncRoot)
            {
                for (int i = m_outstandingRequests.Count - 1; i >= 0; i--)
                {
                    AsyncFileStream_AsyncResult asfar = (AsyncFileStream_AsyncResult)m_outstandingRequests[i];
                    asfar.SignalCompleted();
                }

                m_outstandingRequests.Clear();
            }
        }

        protected override void Dispose(bool disposing)
        {
            // Nothing will be done differently based on whether we are disposing vs. finalizing.
            lock (this)
            {
                if (m_handle != null && !m_handle.IsInvalid)
                {
                    if (disposing)
                    {
                        CancelPendingIO();
                    }

                    m_handle.Close();
                    m_handle.SetHandleAsInvalid();
                }
            }

            base.Dispose(disposing);
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { throw NotImplemented(); }
        }

        public override long Position
        {
            get { throw NotImplemented(); }
            set { throw NotImplemented(); }
        }

        public override IAsyncResult BeginRead(byte[] array, int offset, int count, AsyncCallback userCallback, Object stateObject)
        {
            return BeginReadCore(array, offset, count, userCallback, stateObject);
        }

        public override IAsyncResult BeginWrite(byte[] array, int offset, int count, AsyncCallback userCallback, Object stateObject)
        {
            return BeginWriteCore(array, offset, count, userCallback, stateObject);
        }

        public override void Close()
        {
            Dispose(true);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            AsyncFileStream_AsyncResult afsar = CheckParameterForEnd(asyncResult, false);

            afsar.WaitCompleted();

            m_outstandingRequests.Remove(afsar);

            // Now check for any error during the read.
            if (afsar.m_errorCode != 0) throw new IOException("Async Read failed", afsar.m_errorCode);

            return afsar.m_numBytes;
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            AsyncFileStream_AsyncResult afsar = CheckParameterForEnd(asyncResult, true);

            afsar.WaitCompleted();

            m_outstandingRequests.Remove(afsar);

            // Now check for any error during the write.
            if (afsar.m_errorCode != 0) throw new IOException("Async Write failed", afsar.m_errorCode);
        }

        public override void Flush()
        {
        }

        public override int Read([In, Out] byte[] array, int offset, int count)
        {
            IAsyncResult result = BeginRead(array, offset, count, null, null);
            return EndRead(result);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw NotImplemented();
        }

        public override void SetLength(long value)
        {
            throw NotImplemented();
        }

        public override void Write(byte[] array, int offset, int count)
        {
            IAsyncResult result = BeginWrite(array, offset, count, null, null);
            EndWrite(result);
        }

        public SafeHandle Handle
        {
            get
            {
                return m_handle;
            }
        }

        public virtual int AvailableCharacters
        {
            get
            {
                return 0;
            }
        }

        private Exception NotImplemented()
        {
            return new NotSupportedException("Not Supported");
        }

        private void CheckParametersForBegin(byte[] array, int offset, int count)
        {
            if (array == null) throw new ArgumentNullException("array");

            if (offset < 0) throw new ArgumentOutOfRangeException("offset");

            if (count < 0 || array.Length - offset < count) throw new ArgumentOutOfRangeException("count");

            if (m_handle.IsInvalid)
            {
                throw new ObjectDisposedException(null);
            }
        }

        private AsyncFileStream_AsyncResult CheckParameterForEnd(IAsyncResult asyncResult, bool isWrite)
        {
            if (asyncResult == null) throw new ArgumentNullException("asyncResult");

            AsyncFileStream_AsyncResult afsar = asyncResult as AsyncFileStream_AsyncResult;
            if (afsar == null || afsar.m_isWrite != isWrite) throw new ArgumentException("asyncResult");
            if (afsar.m_EndXxxCalled) throw new InvalidOperationException("EndRead called twice");
            afsar.m_EndXxxCalled = true;

            return afsar;
        }

        private unsafe IAsyncResult BeginReadCore(byte[] array, int offset, int count, AsyncCallback userCallback, Object stateObject)
        {
            CheckParametersForBegin(array, offset, count);

            AsyncFileStream_AsyncResult asyncResult = new AsyncFileStream_AsyncResult(userCallback, stateObject, false);

            if (count == 0)
            {
                asyncResult.SignalCompleted();
            }
            else
            {
                // Keep the array in one location in memory until the OS writes the
                // relevant data into the array.  Free GCHandle later.
                asyncResult.PinBuffer(array);

                fixed (byte* p = array)
                {
                    int numBytesRead = 0;
                    bool res;

                    res = Native.ReadFile(m_handle.DangerousGetHandle(), p + offset, count, out numBytesRead, asyncResult.OverlappedPtr);
                    if (res == false)
                    {
                        if (HandleErrorSituation("BeginRead", false))
                        {
                            asyncResult.SignalCompleted();
                        }
                        else
                        {
                            m_outstandingRequests.Add(asyncResult);
                        }
                    }
                }
            }

            return asyncResult;
        }

        private unsafe IAsyncResult BeginWriteCore(byte[] array, int offset, int count, AsyncCallback userCallback, Object stateObject)
        {
            CheckParametersForBegin(array, offset, count);

            AsyncFileStream_AsyncResult asyncResult = new AsyncFileStream_AsyncResult(userCallback, stateObject, true);

            if (count == 0)
            {
                asyncResult.SignalCompleted();
            }
            else
            {
                // Keep the array in one location in memory until the OS writes the
                // relevant data into the array.  Free GCHandle later.
                asyncResult.PinBuffer(array);

                fixed (byte* p = array)
                {
                    int numBytesWritten = 0;
                    bool res;

                    res = Native.WriteFile(m_handle.DangerousGetHandle(), p + offset, count, out numBytesWritten, asyncResult.OverlappedPtr);
                    if (res == false)
                    {
                        if (HandleErrorSituation("BeginWrite", true))
                        {
                            asyncResult.SignalCompleted();
                        }
                        else
                        {
                            m_outstandingRequests.Add(asyncResult);
                        }
                    }
                }
            }

            return asyncResult;
        }

        protected virtual bool HandleErrorSituation(string msg, bool isWrite)
        {
            int hr = Marshal.GetLastWin32Error();

            // For invalid handles, detect the error and close ourselves
            // to prevent a malicious app from stealing someone else's file
            // handle when the OS recycles the handle number.
            if (hr == Native.ERROR_INVALID_HANDLE)
            {
                m_handle.Close();
            }

            if (hr != Native.ERROR_IO_PENDING)
            {
                if (isWrite == false && hr == Native.ERROR_HANDLE_EOF)
                {
                    throw new EndOfStreamException(msg);
                }

                throw new IOException(msg, hr);
            }

            return false;
        }


        #region IDisposable Members

        void IDisposable.Dispose()
        {
            base.Dispose(true);

            Dispose(true);

            GC.SuppressFinalize(this);
        }

        #endregion
    }

    internal class AsyncFileStream : GenericAsyncStream
    {
        private string m_fileName = null;

        public AsyncFileStream(string file, System.IO.FileShare share) 
            : base(OpenHandle(file, share))
        {
            m_fileName = file;
        }

        public AsyncFileStream(string file, SafeHandle handle)
            : base(handle)
        { 
            m_fileName = file;
        }

        static private SafeFileHandle OpenHandle(string file, System.IO.FileShare share)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentNullException("file");
            }

            SafeFileHandle handle = Native.CreateFile(file, Native.GENERIC_READ | Native.GENERIC_WRITE, share, Native.NULL, System.IO.FileMode.Open, Native.FILE_FLAG_OVERLAPPED, Native.NULL);

            if (handle.IsInvalid)
            {
                Native.ThrowIOException(String.Format("Cannot open {0}", file));
            }

            return handle;
        }

        public String Name
        {
            get
            {
                return m_fileName;
            }
        }

        public unsafe override int AvailableCharacters
        {
            get
            {
                int bytesRead;
                int totalBytesAvail;
                int bytesLeftThisMessage;

                if (Native.PeekNamedPipe(m_handle.DangerousGetHandle(), (byte*)Native.NULL, 0, out bytesRead, out totalBytesAvail, out bytesLeftThisMessage) == false)
                {
                    totalBytesAvail = 1;
                }

                return totalBytesAvail;
            }
        }
    }

    #endregion
}