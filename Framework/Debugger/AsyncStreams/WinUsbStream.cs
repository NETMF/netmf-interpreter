////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using WinUsb;


namespace Microsoft.SPOT.Debugger
{
    public class WinUsb_AsyncUsbStream : UsbStream
    {

        // This GUID must either be registered from the INF file, or ideally using the OS extended COMPATID for WinUSB
        // plus an OS Extended Property Descriptor so that neither a custom driver nor INF is required. 
        private static readonly Guid NetmfDbgWinUsbGuid = new Guid( "{D32D1D64-963D-463E-874A-8EC8C8082CBF}" );
        private static readonly Guid UsbDeviceClassGuid = new Guid( "{A5DCBF10-6530-11D2-901F-00C04FB951ED}" );

        private WinUsbDevice m_winUsbDevice;

        //--//

        const byte USB_GET_DESCRIPTOR = 0x06;
        const byte MANUFACTURER_CONFIGURATION_REQUEST = 0xC1;

        const ushort USB_STRING_DESCRIPTOR_TYPE = 0x03;

        const ushort SERIAL_NUMBER_INDEX = 0;
        const ushort MANUFACTURER_NAME_INDEX = 1;
        const ushort PRODUCT_NAME_INDEX = 2;
        const ushort USB_DISPLAY_STRING_INDEX = 4;
        const ushort USB_FRIENDLY_STRING_INDEX = 5;


        // IOCTL codes
        public const int IOCTL_WINUSB_READ_AVAILABLE = 0;

        public const int IOCTL_WINUSB_DEVICE_HASH = USB_FRIENDLY_STRING_INDEX;
        //public const int IOCTL_WINUSB_MANUFACTURER = 2;
        //public const int IOCTL_WINUSB_PRODUCT = 3;
        //public const int IOCTL_WINUSB_SERIAL_NUMBER = 4;
        //public const int IOCTL_WINUSB_VENDOR_ID = 5;
        //public const int IOCTL_WINUSB_PRODUCT_ID = 6;
        //public const int IOCTL_WINUSB_DISPLAY_NAME = 7;
        //public const int IOCTL_WINUSB_PORT_NAME = 8;

        //--//

        public WinUsb_AsyncUsbStream( string port )
            : base( port )
        {
            m_winUsbDevice = new WinUsbDevice( );

            m_winUsbDevice.DeviceInfo.deviceHandle = ( Microsoft.Win32.SafeHandles.SafeFileHandle )m_handle;

            if( !m_winUsbDevice.InitializeDevice( ) )
            {
                throw new ArgumentException( );
            }

            s_textProperties[ DeviceHash ] = IOCTL_WINUSB_DEVICE_HASH;
        }

        protected override void Dispose( bool disposing )
        {
            // Nothing will be done differently based on whether we are disposing vs. finalizing.
            lock( syncRW )
            {
                if( m_handle != null && !m_handle.IsInvalid )
                {
                    if( disposing )
                        CancelPendingIO( );

                    m_winUsbDevice.CloseDeviceHandle( );
                    m_handle.SetHandleAsInvalid( );
                }
            }

            base.Dispose( disposing );
        }

        public override void Close( )
        {
            Dispose( true );
        }

        public override IAsyncResult BeginRead( byte[ ] buffer, int offset, int count, AsyncCallback callback, object state )
        {
            return BeginReadCore( buffer, offset, count, callback, state );
        }

        public override int EndRead( IAsyncResult asyncResult )
        {
            AsyncFileStream_AsyncResult afsar = CheckParameterForEnd( asyncResult, false );

            afsar.WaitCompleted( );

            m_outstandingRequests.Remove( afsar );

            // Now check for any error during the read.
            if( afsar.m_errorCode != 0 )
                throw new IOException( "Async Read failed", afsar.m_errorCode );

            return afsar.m_numBytes;
        }

        public override IAsyncResult BeginWrite( byte[ ] buffer, int offset, int count, AsyncCallback callback, object state )
        {
            return BeginWriteCore( buffer, offset, count, callback, state );
        }

        public override void EndWrite( IAsyncResult asyncResult )
        {
            AsyncFileStream_AsyncResult afsar = CheckParameterForEnd( asyncResult, true );

            //afsar.WaitCompleted();

            afsar.m_waitHandle.WaitOne( afsar.m_numBytes );

            m_outstandingRequests.Remove( afsar );

            // Now check for any error during the write.
            if( afsar.m_errorCode != 0 )
                throw new IOException( "Async Write failed", afsar.m_errorCode );
        }

        public override void Flush( )
        {
        }

        byte[ ] m_buffer = new byte[ 4096 ];
        uint m_bytesAvailable = 0;
        object syncRW = new object( );

        public override int AvailableCharacters
        {
            get
            {
                // sync on memory stream
                uint numBytesRead = 0;

                lock( syncRW )
                {
                    // WinUSB does not support peek! 
                    // we will read whatever is available from the device into a memory stream and then copy it to the application level buffer
                    bool res = m_winUsbDevice.ReadPipe( Convert.ToByte( m_winUsbDevice.DeviceInfo.bulkInPipe )
                                                      , m_buffer
                                                      , ( uint )m_buffer.Length
                                                      , ref numBytesRead
                                                      , IntPtr.Zero
                                                      );

                    if( res )
                        m_bytesAvailable += numBytesRead;
                }

                return ( int )m_bytesAvailable;
            }
        }

        private unsafe IAsyncResult BeginReadCore( byte[ ] array, int offset, int count, AsyncCallback userCallback, Object stateObject )
        {
            Debug.Assert( userCallback == null );
            Debug.Assert( stateObject == null );

            CheckParametersForBegin( array, offset, count );

            AsyncFileStream_AsyncResult asyncResult = new AsyncFileStream_AsyncResult( userCallback, stateObject, false );

            if( count == 0 )
            {
                asyncResult.SignalCompleted( );
            }
            else
            {
                // Keep the array in one location in memory until the OS writes the
                // relevant data into the array.  Free GCHandle later.
                asyncResult.PinBuffer( array );

                int totalRead = 0;
                int availableToTransfer = 0;
                fixed( byte* dst = array )
                {
                    // if we already have available data, then we need to use it
                    lock( syncRW )
                    {
                        System.Diagnostics.Debug.Assert( count == m_bytesAvailable );

                        if( m_bytesAvailable > 0 )
                        {

                            // truncate
                            availableToTransfer = Math.Min( count, ( int )m_bytesAvailable );

                            // copy over to the application buffer
                            Marshal.Copy( m_buffer, 0, new IntPtr( dst + offset ), availableToTransfer );

                            // update the available bytes count
                            m_bytesAvailable -= ( uint )availableToTransfer;
                            totalRead += availableToTransfer;

                            // adjust the buffering if there is a left over 
                            // this will never happen if application call AvailableBytes and the reads them 
                            if( m_bytesAvailable > 0 )
                            {
                                fixed( byte* copy = m_buffer )
                                {
                                    Marshal.Copy( new IntPtr( copy ), m_buffer, ( int )availableToTransfer, ( int )m_bytesAvailable );
                                }
                            }
                        }
                    }

                    // if we need to read more, then we should go to the stream directly
                    int remainder = count - availableToTransfer;

                    if( remainder > 0 )
                    {
                        byte[ ] byteArray = new byte[ remainder ];

                        Array.Clear( byteArray, 0, byteArray.Length );

                        fixed( byte* p = byteArray )
                        {
                            uint numBytesRead = 0;

                            var res = m_winUsbDevice.ReadPipe( Convert.ToByte( m_winUsbDevice.DeviceInfo.bulkInPipe )
                                                         , byteArray
                                                         , ( uint )remainder
                                                         , ref numBytesRead
                                                         , IntPtr.Zero
                                                         );

                            if( !res )
                            {
                                if( HandleErrorSituation( "BeginRead", false ) )
                                    asyncResult.SignalCompleted( );
                                else
                                    m_outstandingRequests.Add( asyncResult );
                            }
                            else
                            {
                                Marshal.Copy( byteArray, 0, new IntPtr( dst + totalRead + offset ), ( int )numBytesRead );
                                totalRead += ( int )numBytesRead;
                            }
                        }
                    }

                    if( totalRead > 0 )
                    {
                        asyncResult.m_numBytes = totalRead;
                        asyncResult.SignalCompleted( );
                    }
                }
            }

            return asyncResult;
        }

        private unsafe IAsyncResult BeginWriteCore( byte[ ] array, int offset, int count, AsyncCallback userCallback, Object stateObject )
        {
            Debug.Assert( userCallback == null );
            Debug.Assert( stateObject == null );

            CheckParametersForBegin( array, offset, count );

            AsyncFileStream_AsyncResult asyncResult = new AsyncFileStream_AsyncResult( userCallback, stateObject, true );

            if( count == 0 )
            {
                asyncResult.SignalCompleted( );
            }
            else
            {
                // Keep the array in one location in memory until the OS writes the
                // relevant data into the array.  Free GCHandle later.
                asyncResult.PinBuffer( array );

                fixed( byte* p = array )
                {
                    uint numBytesWritten = 0;
                    bool res;

                    byte[ ] byteArray = new byte[ count ];
                    Marshal.Copy( new IntPtr( p + offset ), byteArray, 0, count );

                    //res = Native.ReadFile(m_handle.DangerousGetHandle(), p + offset, count, out numBytesRead, asyncResult.OverlappedPtr);
                    res = m_winUsbDevice.WritePipe( Convert.ToByte( m_winUsbDevice.DeviceInfo.bulkOutPipe )
                                                  , byteArray
                                                  , ( uint )count
                                                  , ref numBytesWritten
                                                  , IntPtr.Zero
                                                  );

                    if( !res )
                    {
                        if( HandleErrorSituation( "BeginWrite", true ) )
                            asyncResult.SignalCompleted( );
                        else
                            m_outstandingRequests.Add( asyncResult );
                    }
                    else
                    {
                        asyncResult.m_numBytes = ( int )numBytesWritten;
                        asyncResult.SignalCompleted( );
                    }
                }
            }

            return asyncResult;
        }

        private static void DebugDump( WireProtocol.IncomingMessage m, string text )
        {
        }

        public static PortDefinition[ ] EnumeratePorts( bool findGenericDevices = true )
        {
            var ports = EnumeratePorts( NetmfDbgWinUsbGuid );
            if( findGenericDevices )
                ports = ports.Concat( EnumeratePorts( UsbDeviceClassGuid ) );

            // de-duplicate the list based on the base device path name (minus the interface GUID)
            var q = from portDef in ports.Distinct( new UsbDevicePortNameEqualityComparer( ) )
                    orderby portDef.DisplayName
                    select portDef;

            return q.ToArray( );
        }

        // The following procedure works with the USB device driver; upon finding all instances of USB devices
        // that match the requested Guid, the procedure checks the corresponding registry keys to find the unique
        // serial number to show to the user; the serial number is decided by the device driver at installation
        // time and stored in a registry key whose name is the hash of the laser etched security key of the device
        private static IEnumerable<PortDefinition> EnumeratePorts( Guid guid )
        {
            var devInfo = NativeMethods.SetupDiGetClassDevs( ref guid, null, 0, NativeMethods.DIGCF_DEVICEINTERFACE | NativeMethods.DIGCF_PRESENT );

            if( devInfo == NativeMethods.INVALID_HANDLE_VALUE )
                yield break;

            try
            {
                var interfaceData = new NativeMethods.SP_DEVICE_INTERFACE_DATA( );
                interfaceData.cbSize = Marshal.SizeOf( interfaceData );

                var index = 0;

                while( NativeMethods.SetupDiEnumDeviceInterfaces( devInfo, 0, ref guid, index++, ref interfaceData ) )
                {
                    PortDefinition pd = null;
                    try
                    {
                        var detail = new NativeMethods.SP_DEVICE_INTERFACE_DETAIL_DATA( );
                        // explicit size of unmanaged structure must be provided, because it does not include transfer buffer
                        // for whatever reason on 64 bit machines the detail size is 8 rather than 5, likewise the interfaceData.cbSize
                        // is 32 rather than 28 for non 64bit machines, therefore, we make the detemination of the size based 
                        // on the interfaceData.cbSize (kind of hacky but it works).
                        detail.cbSize = interfaceData.cbSize == 32 ? 8 : 5;

                        // get device interface details to allow opening the port for querying the friendly name
                        if( !NativeMethods.SetupDiGetDeviceInterfaceDetail( devInfo, ref interfaceData, ref detail, Marshal.SizeOf( detail ) * 2, 0, 0 ) )
                            continue;

                        var port = detail.DevicePath.ToLower( );

                        using( var s = new WinUsb_AsyncUsbStream( port ) )
                        {
                            var displayName = s.RetrieveStringFromDevice( USB_DISPLAY_STRING_INDEX );

                            var hash = s.RetrieveStringFromDevice( USB_FRIENDLY_STRING_INDEX );
                            if( ( displayName == null ) || ( hash == null ) )
                                yield break;

                            displayName += "_" + hash;
                            pd = PortDefinition.CreateInstanceForWinUsb( displayName, port );
                            if( !pd.Properties.Contains( DeviceHash ) )
                                pd.Properties.Add( DeviceHash, hash );
                        }
                    }
                    catch(IOException)
                    {
                        // go to next device
                    }
                    if( pd != null )
                        yield return pd;
                }
            }
            finally
            {
                NativeMethods.SetupDiDestroyDeviceInfoList( devInfo );
            }
        }

        protected static void RetrieveProperties( string hash, ref PortDefinition pd, WinUsb_AsyncUsbStream s )
        {
            IDictionaryEnumerator dict;

            dict = s_textProperties.GetEnumerator( );

            while( dict.MoveNext( ) )
            {
                pd.Properties.Add( dict.Key, s.RetrieveStringFromDevice( ( ushort )dict.Value ) );
            }

            dict = s_digitProperties.GetEnumerator( );

            while( dict.MoveNext( ) )
            {
                pd.Properties.Add( dict.Key, s.RetrieveIntegerFromDevice( ( ushort )dict.Value ) );
            }
        }

        protected unsafe string RetrieveStringFromDevice( ushort index )
        {
            byte[ ] buffer = new byte[ c_DeviceStringBufferSize ];

            byte* payload = null;
            fixed( byte* p = buffer )
            {
                ushort wValue = ( ushort )( ( ushort )( ( USB_STRING_DESCRIPTOR_TYPE << 8 ) & 0xFF00 ) | ( ushort )( index & 0x00FF ) );
                int read = m_winUsbDevice.Do_Control_Read_Transfer( ref buffer, USB_GET_DESCRIPTOR, wValue );
                if( read != -1 )
                {
                    // null terminate the string
                    if( read > ( c_DeviceStringBufferSize - 2 ) )
                    {
                        read = c_DeviceStringBufferSize - 2;
                    }

                    p[ read ] = 0;
                    p[ read + 1 ] = 0;

                    // skip first two characters
                    payload = p + 2;
                }
            }

            return payload == null ? null : new string( ( char* )payload );
        }

        protected unsafe int RetrieveIntegerFromDevice( ushort index )
        {
            System.Diagnostics.Debug.Assert( false );
            int digits = 0;

            return digits;
        }
    }
}
