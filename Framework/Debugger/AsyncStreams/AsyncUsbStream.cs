////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;
using System.Management;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.SPOT.Debugger
{
    public class AsyncUsbStream : UsbStream
    {        
        
        // IOCTL codes
        public const int IOCTL_SPOTUSB_READ_AVAILABLE = 0;
        public const int IOCTL_SPOTUSB_DEVICE_HASH = 1;
        public const int IOCTL_SPOTUSB_MANUFACTURER = 2;
        public const int IOCTL_SPOTUSB_PRODUCT = 3;
        public const int IOCTL_SPOTUSB_SERIAL_NUMBER = 4;
        public const int IOCTL_SPOTUSB_VENDOR_ID = 5;
        public const int IOCTL_SPOTUSB_PRODUCT_ID = 6;
        public const int IOCTL_SPOTUSB_DISPLAY_NAME = 7;
        public const int IOCTL_SPOTUSB_PORT_NAME = 8;

        // paths
        static readonly string SpotGuidKeyPath           = @"System\CurrentControlSet\Services\SpotUsb\Parameters";


        public AsyncUsbStream( string port ) : base( port )
        {
            s_textProperties[DeviceHash  ] = IOCTL_SPOTUSB_DEVICE_HASH;
            s_textProperties[Manufacturer] = IOCTL_SPOTUSB_MANUFACTURER;
            s_textProperties[Product     ] = IOCTL_SPOTUSB_PRODUCT;
            s_textProperties[SerialNumber] = IOCTL_SPOTUSB_SERIAL_NUMBER ;
            
            //--//            
            
            s_digitProperties[VendorId ] = IOCTL_SPOTUSB_VENDOR_ID;
            s_digitProperties[ProductId] = IOCTL_SPOTUSB_PRODUCT_ID;
        }

        public unsafe override int AvailableCharacters
        {
            get
            {
                int code = NativeMethods.ControlCode( NativeMethods.FILE_DEVICE_UNKNOWN, 0, NativeMethods.METHOD_BUFFERED, NativeMethods.FILE_ANY_ACCESS );
                int avail;
                int read;

                if(!NativeMethods.DeviceIoControl( m_handle.DangerousGetHandle(), code, null, IOCTL_SPOTUSB_READ_AVAILABLE, (byte*)&avail, sizeof(int), out read, null ) || read != sizeof(int))
                {
                    return 0;
                }

                return avail;
            }
        }

        public static PortDefinition[] EnumeratePorts()
        {
            SortedList lst = new SortedList();

            // enumerate each guid under the discovery key
            RegistryKey driverParametersKey = Registry.LocalMachine.OpenSubKey( SpotGuidKeyPath );

            // if no parameters key is found, it means that no USB device has ever been plugged into the host 
            // or no driver was installed
            if(driverParametersKey != null)
            {
                string inquiriesInterfaceGuid   = (string)driverParametersKey.GetValue( InquiriesInterface   );
                string driverVersion            = (string)driverParametersKey.GetValue( DriverVersion        );
            
                if((inquiriesInterfaceGuid != null) && (driverVersion != null))
                {
                    EnumeratePorts( new Guid( inquiriesInterfaceGuid ), driverVersion, lst ); 
                }
            }

            ICollection      col = lst.Values;
            PortDefinition[] res = new PortDefinition[col.Count];

            col.CopyTo( res, 0 );

            return res;
        }


        // The following procedure works with the USB device driver; upon finding all instances of USB devices
        // that match the requested Guid, the procedure checks the corresponding registry keys to find the unique
        // serial number to show to the user; the serial number is decided by the device driver at installation
        // time and stored in a registry key whose name is the hash of the laser etched security key of the device
        private static void EnumeratePorts( Guid inquiriesInterface, string driverVersion, SortedList lst )
        {
            IntPtr devInfo = NativeMethods.SetupDiGetClassDevs( ref inquiriesInterface, null, 0, NativeMethods.DIGCF_DEVICEINTERFACE | NativeMethods.DIGCF_PRESENT );

            if(devInfo == NativeMethods.INVALID_HANDLE_VALUE)
            {
                return;
            }

            NativeMethods.SP_DEVICE_INTERFACE_DATA interfaceData = new NativeMethods.SP_DEVICE_INTERFACE_DATA(); interfaceData.cbSize = Marshal.SizeOf(interfaceData);
            
            int index = 0;

            while(NativeMethods.SetupDiEnumDeviceInterfaces( devInfo, 0, ref inquiriesInterface, index++, ref interfaceData ))
            {
                NativeMethods.SP_DEVICE_INTERFACE_DETAIL_DATA detail = new NativeMethods.SP_DEVICE_INTERFACE_DETAIL_DATA();
                // explicit size of unmanaged structure must be provided, because it does not include transfer buffer
                // for whatever reason on 64 bit machines the detail size is 8 rather than 5, likewise the interfaceData.cbSize
                // is 32 rather than 28 for non 64bit machines, therefore, we make the detemination of the size based 
                // on the interfaceData.cbSize (kind of hacky but it works).
                if( interfaceData.cbSize == 32 )
                {
                    detail.cbSize = 8;
                }
                else
                {
                    detail.cbSize = 5;
                }
                

                if(NativeMethods.SetupDiGetDeviceInterfaceDetail( devInfo, ref interfaceData, ref detail, Marshal.SizeOf(detail) * 2, 0, 0 ))
                {
                    string port = detail.DevicePath.ToLower();

                    AsyncUsbStream s = null;

                    try
                    {
                        s = new AsyncUsbStream( port );

                        string displayName     = s.RetrieveStringFromDevice( IOCTL_SPOTUSB_DISPLAY_NAME ); 
                        string hash            = s.RetrieveStringFromDevice( IOCTL_SPOTUSB_DEVICE_HASH  ); 
                        string operationalPort = s.RetrieveStringFromDevice( IOCTL_SPOTUSB_PORT_NAME    ); 

                        if((operationalPort == null) || (displayName == null) || (hash == null))
                        {
                            continue;
                        }

                        // convert  kernel format to user mode format                        
                        // kernel   : @"\??\USB#Vid_beef&Pid_0009#5&4162af8&0&1#{09343630-a794-10ef-334f-82ea332c49f3}"
                        // user     : @"\\?\usb#vid_beef&pid_0009#5&4162af8&0&1#{09343630-a794-10ef-334f-82ea332c49f3}"
                        StringBuilder operationalPortUser = new StringBuilder();
                        operationalPortUser.Append( @"\\?" );
                        operationalPortUser.Append( operationalPort.Substring( 3 ) );

                        // change the display name if there is a collision (otherwise you will only be able to use one of the devices)
                        displayName += "_" + hash;
                        if (lst.ContainsKey(displayName))
                        {
                            int i = 2;
                            while (lst.ContainsKey(displayName + " (" + i + ")"))
                            {
                                i++;
                            }
                            displayName += " (" + i + ")";
                        }

                        PortDefinition pd  = PortDefinition.CreateInstanceForUsb( displayName, operationalPortUser.ToString() );
                        
                        RetrieveProperties( hash, ref pd, s );

                        lst.Add( pd.DisplayName, pd );
                    }
                    catch
                    {
                    }
                    finally
                    {
                        if(s != null) s.Close();
                    }
                }
            }

            NativeMethods.SetupDiDestroyDeviceInfoList( devInfo );
        }
    }
}
