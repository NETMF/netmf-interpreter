//using System;
//using System.Runtime.InteropServices;

//namespace WinUsb
//{
//    ///<summary >
//    // API declarations relating to device management (SetupDixxx and 
//    // RegisterDeviceNotification functions).   
//    /// </summary>

//    sealed public partial class DeviceManagement
//    {
//        // from dbt.h

//        internal const Int32 DBT_DEVICEARRIVAL = 0X8000;
//        internal const Int32 DBT_DEVICEREMOVECOMPLETE = 0X8004;
//        internal const Int32 DBT_DEVTYP_DEVICEINTERFACE = 5;
//        internal const Int32 DBT_DEVTYP_HANDLE = 6;
//        internal const Int32 DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 4;
//        internal const Int32 DEVICE_NOTIFY_SERVICE_HANDLE = 1;
//        internal const Int32 DEVICE_NOTIFY_WINDOW_HANDLE = 0;
//        internal const Int32 WM_DEVICECHANGE = 0X219;

//        // from setupapi.h

//        internal const Int32 DIGCF_PRESENT = 2;
//        internal const Int32 DIGCF_DEVICEINTERFACE = 0X10;

//        // Two declarations for the DEV_BROADCAST_DEVICEINTERFACE structure.

//        // Use this one in the call to RegisterDeviceNotification() and
//        // in checking dbch_devicetype in a DEV_BROADCAST_HDR structure:

//        [StructLayout(LayoutKind.Sequential)]
//        internal class DEV_BROADCAST_DEVICEINTERFACE
//        {
//            internal Int32 dbcc_size;
//            internal Int32 dbcc_devicetype;
//            internal Int32 dbcc_reserved;
//            internal Guid dbcc_classguid;
//            internal Int16 dbcc_name;
//        }

//        // Use this to read the dbcc_name String and classguid:

//        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
//        internal class DEV_BROADCAST_DEVICEINTERFACE_1
//        {
//            internal Int32 dbcc_size;
//            internal Int32 dbcc_devicetype;
//            internal Int32 dbcc_reserved;
//            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
//            internal Byte[] dbcc_classguid;
//            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
//            internal Char[] dbcc_name;
//        }

//        [StructLayout(LayoutKind.Sequential)]
//        internal class DEV_BROADCAST_HDR
//        {
//            internal Int32 dbch_size;
//            internal Int32 dbch_devicetype;
//            internal Int32 dbch_reserved;
//        }
		
//        internal struct SP_DEVICE_INTERFACE_DATA
//        {
//            internal Int32 cbSize;
//            internal System.Guid InterfaceClassGuid;
//            internal Int32 Flags;
//            internal IntPtr Reserved;
//        }

//        internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
//        {
//            internal Int32 cbSize;
//            internal String DevicePath;
//        }

//        internal struct SP_DEVINFO_DATA
//        {
//            internal Int32 cbSize;
//            internal System.Guid ClassGuid;
//            internal Int32 DevInst;
//            internal Int32 Reserved;
//        }

//        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
//        internal static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, Int32 Flags);

//        [DllImport("setupapi.dll", SetLastError = true)]
//        internal static extern Int32 SetupDiCreateDeviceInfoList(ref System.Guid ClassGuid, Int32 hwndParent);

//        [DllImport("setupapi.dll", SetLastError = true)]
//        internal static extern Int32 SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

//        [DllImport("setupapi.dll", SetLastError = true)]
//        internal static extern Boolean SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, ref System.Guid InterfaceClassGuid, Int32 MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

//        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
//        internal static extern IntPtr SetupDiGetClassDevs(ref System.Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, Int32 Flags);

//        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
//        internal static extern Boolean SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, Int32 DeviceInterfaceDetailDataSize, ref Int32 RequiredSize, IntPtr DeviceInfoData);

//        [DllImport("user32.dll", SetLastError = true)]
//        internal static extern Boolean UnregisterDeviceNotification(IntPtr Handle);
//    }
//}
