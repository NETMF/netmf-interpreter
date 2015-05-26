using System.Collections.Generic;

namespace Microsoft.SPOT.Debugger
{
    /// <summary>Equality comparer to compare device names of USB ports independent of the interface</summary>
    /// <remarks>
    /// Port "file" names take a specific format that includes the interface as a GUID string at the end
    /// This is used to eliminate duplicates when allowing legacy permiscuous WinUSB support. This comparer
    /// strips the interface GUID from the device path to get at the common device path so that two interfaces
    /// on the same device are treated as the same device. 
    /// </remarks>
    internal class UsbDevicePortNameEqualityComparer 
        : IEqualityComparer< PortDefinition >
    {
        public bool Equals( PortDefinition x, PortDefinition y )
        {
            return GetBaseDeviceName( x ).Equals( GetBaseDeviceName( y ) );
        }

        public int GetHashCode( PortDefinition obj )
        {
            return GetBaseDeviceName( obj ).GetHashCode( );
        }

        public static string GetBaseDeviceName( PortDefinition portDef )
        {
            // drop the interface guid from the end of the port name
            return portDef.Port.Substring( 0, portDef.Port.LastIndexOf( '{' ) );
        }
    }
}
