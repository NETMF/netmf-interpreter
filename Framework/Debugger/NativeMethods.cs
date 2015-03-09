////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.SPOT.Debugger
{
    public class AccurateTimer
    {
        private static double ratio = Initialize( );
        private static long start = GetTicks( );

        private static double Initialize( )
        {
            long ticksPerSecond;

            NativeMethods.QueryPerformanceFrequency( out ticksPerSecond );

            return 1 / ( double )ticksPerSecond;
        }

        // returns absolute time in milliseconds
        public static long GetTicks( )
        {
            long time;

            NativeMethods.QueryPerformanceCounter( out time );

            return time;
        }

        public static string GetTimestamp( )
        {
            double timestamp = ( GetTicks( ) - start ) * ratio;
            long hi = ( long )System.Math.Floor( timestamp );
            long low = ( long )System.Math.Floor( ( timestamp - hi ) * 1000000 );
            DateTime now = DateTime.Now;

            return string.Format( "{0,5}.{1,6:D6} {2:HH:mm:ss.fff}", hi, low, now );
        }
    }
    
    public sealed class ASCII
    {
        public const byte SOH = 0x01;
        public const byte STX = 0x02;
        public const byte ETX = 0x03;
        public const byte DLE = 0x10;
    }

    internal static class NativeMethods
    {
        // Error codes (not HRESULTS), from winerror.h
        public const int ERROR_BROKEN_PIPE       = 109;
        public const int ERROR_NO_DATA           = 232;
        public const int ERROR_HANDLE_EOF        = 38;
        public const int ERROR_INVALID_PARAMETER = 87;
        public const int ERROR_OPERATION_ABORTED = 995;
        public const int ERROR_IO_PENDING        = 997;
        public const int ERROR_INVALID_HANDLE    = 0x6;


        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct COMMTIMEOUTS
        {
            public int ReadIntervalTimeout;         /* Maximum time between read chars. */
            public int ReadTotalTimeoutMultiplier;  /* Multiplier of characters.        */
            public int ReadTotalTimeoutConstant;    /* Constant in milliseconds.        */
            public int WriteTotalTimeoutMultiplier; /* Multiplier of characters.        */
            public int WriteTotalTimeoutConstant;   /* Constant in milliseconds.        */

            public void Initialize()
            {
                ReadIntervalTimeout         =      0;
                ReadTotalTimeoutMultiplier  = 0xFFFF;
                ReadTotalTimeoutConstant    = 0xFFFF;
                WriteTotalTimeoutMultiplier = 0xFFFF;
                WriteTotalTimeoutConstant   = 0xFFFF;
            }
        }

        public const int DUPLICATE_SAME_ACCESS =                0x00000002;
        public const int FILE_ATTRIBUTE_NORMAL =                0x00000080;
        public const int FILE_FLAG_OVERLAPPED  =                0x40000000;
        public const int GENERIC_READ          = unchecked((int)0x80000000);
        public const int GENERIC_WRITE         =          (     0x40000000);
        public const int GENERIC_EXECUTE       =          (     0x20000000);
        public const int GENERIC_ALL           =          (     0x10000000);

        public const int FILE_DEVICE_UNKNOWN   =          (     0x00000022);
        public const int METHOD_BUFFERED       =          (     0x00000000);
        public const int FILE_ANY_ACCESS       =          (     0x00000000);

        public const int DIGCF_PRESENT         =          (     0x00000002);
        public const int DIGCF_DEVICEINTERFACE =          (     0x00000010);

        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);  // WinBase.h
        public static readonly IntPtr NULL                 = IntPtr.Zero;

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct DCB
        {
            //
            // DTR Control Flow Values.
            //
            public const uint DTR_CONTROL_DISABLE    = 0x00000000;
            public const uint DTR_CONTROL_ENABLE     = 0x00000010;
            public const uint DTR_CONTROL_HANDSHAKE  = 0x00000020;

            //
            // RTS Control Flow Values
            //
            public const uint RTS_CONTROL_DISABLE    = 0x00000000;
            public const uint RTS_CONTROL_ENABLE     = 0x00001000;
            public const uint RTS_CONTROL_HANDSHAKE  = 0x00002000;
            public const uint RTS_CONTROL_TOGGLE     = 0x00003000;

            public const uint mask_fBinary           = 0x00000001;
            public const uint mask_fParity           = 0x00000002;
            public const uint mask_fOutxCtsFlow      = 0x00000004;
            public const uint mask_fOutxDsrFlow      = 0x00000008;
            public const uint mask_fDtrControl       = 0x00000030;
            public const uint mask_fDsrSensitivity   = 0x00000040;
            public const uint mask_fTXContinueOnXoff = 0x00000080;
            public const uint mask_fOutX             = 0x00000100;
            public const uint mask_fInX              = 0x00000200;
            public const uint mask_fErrorChar        = 0x00000400;
            public const uint mask_fNull             = 0x00000800;
            public const uint mask_fRtsControl       = 0x00003000;
            public const uint mask_fAbortOnError     = 0x00004000;
            public const uint mask_fDummy2           = 0xFFFF8000;


            public uint  DCBlength;            /* sizeof(DCB)                          */
            public uint  BaudRate;             /* Baudrate at which running            */
            public uint  __BitField;
            public short wReserved;            /* Not currently used                   */
            public short XonLim;               /* Transmit X-ON threshold              */
            public short XoffLim;              /* Transmit X-OFF threshold             */
            public byte  ByteSize;             /* Number of bits/byte, 4-8             */
            public byte  Parity;               /* 0-4=None,Odd,Even,Mark,Space         */
            public byte  StopBits;             /* 0,1,2 = 1, 1.5, 2                    */
            public byte  XonChar;              /* Tx and Rx X-ON character             */
            public byte  XoffChar;             /* Tx and Rx X-OFF character            */
            public byte  ErrorChar;            /* Error replacement char               */
            public byte  EofChar;              /* End of Input character               */
            public byte  EvtChar;              /* Received Event character             */
            public short wReserved1;           /* Fill for now.                        */

            public void Initialize()
            {
                DCBlength  = (uint)Marshal.SizeOf(this);
                BaudRate   = 0;
                __BitField = 0;
                wReserved  = 0;
                XonLim     = 0;
                XoffLim    = 0;
                ByteSize   = 0;
                Parity     = 0;
                StopBits   = 0;
                XonChar    = 0;
                XoffChar   = 0;
                ErrorChar  = 0;
                EofChar    = 0;
                EvtChar    = 0;
                wReserved1 = 0;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct COMSTAT
        {
            public uint  __BitField;     // DWORD fCtsHold   :1;
            public uint cbInQue;         // DWORD cbInQue;
            public uint cbOutQue;        // DWORD cbOutQue;

            public void Initialize()
            {
                __BitField = 0;
                cbInQue    = 0;
                cbOutQue   = 0;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct SP_DEVICE_INTERFACE_DATA
        {
            public int    cbSize;
            public Guid   InterfaceClassGuid;
            public int    Flags;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct SP_DEVINFO_DATA
        {
            public int    cbSize;
            public Guid   ClassGuid;
            public int    DevInst;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
        public struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
                                                                    public int    cbSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=512)] public string DevicePath;
        }

        public const String KERNEL32 = "kernel32.dll";
        public const String SETUPAPI = "setupapi.dll";

        [DllImport(KERNEL32, CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool SetCommTimeouts( IntPtr handle, [In, Out] ref COMMTIMEOUTS ver);

        [DllImport(KERNEL32, CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool ClearCommError( IntPtr handle, out uint errors, ref COMSTAT ver );

        [DllImport(KERNEL32, CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool GetCommState( IntPtr handle, [In, Out] ref DCB dcb );

        [DllImport(KERNEL32, CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool SetCommState( IntPtr handle, [In, Out] ref DCB dcb );

        [DllImport(KERNEL32, SetLastError=true, CharSet=CharSet.Auto)]
        public static extern SafeFileHandle CreateFile( string    lpFileName            ,
                                                int       dwDesiredAccess       ,
                                                FileShare dwShareMode           ,
                                                IntPtr    lpSecurityAttributes  ,
                                                FileMode  dwCreationDisposition ,
                                                int       dwFlagsAndAttributes  ,
                                                IntPtr    hTemplateFile         );

        [DllImport(KERNEL32, SetLastError=true)]
        public unsafe static extern bool DeviceIoControl(     IntPtr            hDevice         ,
                                                              int               dwIoControlCode ,
                                                              byte*             lpInBuffer      ,
                                                              int               nInBufferSize   ,
                                                              byte*             lpOutBuffer     ,
                                                              int               nOutBufferSize  ,
                                                          out int               lpBytesReturned ,
                                                              NativeOverlapped* lpOverlapped    );

        [DllImport(KERNEL32)]
        public static extern bool DuplicateHandle(     IntPtr hSourceProcessHandle ,  // handle to source process
                                                       IntPtr hSourceHandle        ,  // handle to duplicate
                                                       IntPtr hTargetProcessHandle ,  // handle to target process
                                                   out IntPtr lpTargetHandle       ,  // duplicate handle
                                                       int    dwDesiredAccess      ,  // requested access
                                                       bool   bInheritHandle       ,  // handle inheritance option
                                                       int    dwOptions            ); // optional actions


        [DllImport(KERNEL32)]
        public static extern bool CloseHandle( IntPtr handle );

        [DllImport(KERNEL32, SetLastError=true)]
        public unsafe static extern bool ReadFile(     IntPtr            handle         ,
                                                       byte*             bytes          ,
                                                       int               numBytesToRead ,
                                                   out int               numBytesRead   ,
                                                       NativeOverlapped* overlapped     );

        [DllImport(KERNEL32 , SetLastError=true)]
        public unsafe static extern bool WriteFile(     IntPtr            handle          ,
                                                        byte*             bytes           ,
                                                        int               numBytesToWrite ,
                                                    out int               numBytesWritten ,
                                                        NativeOverlapped* lpOverlapped    );

        [DllImport(KERNEL32 , SetLastError=true)]
        public unsafe static extern bool PeekNamedPipe(     IntPtr handle               ,
                                                            byte*  buffer               ,
                                                            int    bufferSize           ,
                                                        out int    bytesRead            ,
                                                        out int    totalBytesAvail      ,
                                                        out int    bytesLeftThisMessage );

        [DllImport(KERNEL32)]
        public static extern bool QueryPerformanceCounter( out long value );

        [DllImport(KERNEL32)]
        public static extern bool QueryPerformanceFrequency( out long value );

        [DllImport(SETUPAPI, SetLastError=true)]
        public static extern IntPtr SetupDiGetClassDevs( ref Guid   ClassGuid  ,
                                                             string Enumerator ,
                                                             int    hwndParent ,
                                                             int    Flags      );

        [DllImport(SETUPAPI, SetLastError=true)]
        public unsafe static extern bool SetupDiEnumDeviceInterfaces(     IntPtr                   DeviceInfoSet       ,
                                                                          int                      DeviceInfoData      ,
                                                                      ref Guid                     InterfaceClassGuid  ,
                                                                          int                      MemberIndex         ,
                                                                      ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData );

        [DllImport(SETUPAPI, SetLastError=true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(                 IntPtr                          DeviceInfoSet                 ,
                                                                               ref SP_DEVICE_INTERFACE_DATA        DeviceInterfaceData           ,
                                                                   [ In, Out ] ref SP_DEVICE_INTERFACE_DETAIL_DATA DeviceInterfaceDetailData     ,
                                                                                   int                             DeviceInterfaceDetailDataSize ,
                                                                                   int                             RequiredSize                  ,
                                                                                   int                             DeviceInfoData                );

        [DllImport(SETUPAPI, SetLastError=true)]
        public static extern bool SetupDiDestroyDeviceInfoList( IntPtr DeviceInfoSet );

        static public void ThrowIOException( string msg )
        {
            int errorCode = Marshal.GetLastWin32Error();

            throw new IOException( msg, errorCode );
        }

        static public int ControlCode( int DeviceType, int Function, int Method, int Access )
        {
            return ((DeviceType) << 16) | ((Access) << 14) | ((Function) << 2) | (Method);
        }
    }
}
