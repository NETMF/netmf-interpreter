using Microsoft.SPOT;
using Microsoft.SPOT.Net.NetworkInformation;

namespace Microsoft.NETMF.Networking
{
    /// <summary>Provides simplified smonitoring of network interface activity and IP address handling</summary>
    public class NetworkStateMonitor 
    {
        public NetworkStateMonitor()
        {
            // attach to network address change notifications 
            NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
        }

        /// <summary>Retrieves the current status of all NetworkInterfaces in the system</summary>
        /// <remarks>
        /// The return value from NetworkInterface.GetAllNetworkInterfaces( )
        /// is a snapshot of the current network interface configurations. In
        /// other words the elements are not dynamically updated with current
        /// information. Thus the call to GetAllNetworkInterfaces is done each
        /// time this property is read and the results should *not* be cached.
        /// </remarks>
        public NetworkInterface[ ] NetworkInterfaces
        {
            get { return NetworkInterface.GetAllNetworkInterfaces( ); }
        }

        /// <summary>Sleep wait for a network interface with a valid (e.g. not 0.0.0.0) IP address</summary>
        public void WaitForIpAddress( )
        {
            do
            {
                StatusChanged.Reset();
                foreach( var itf in NetworkInterfaces )
                {
                    if( itf.IPAddress == "0.0.0.0" || itf.IPAddress == "127.0.0.1")
                        continue;

                    Debug.Print( "Found IP: " + itf.IPAddress );
                    Debug.Print( "     MAC: " + itf.PhysicalAddress[ 0 ]
                                              + ":" + ((uint)itf.PhysicalAddress[ 1 ]).ToString( "X" )
                                              + ":" + ((uint)itf.PhysicalAddress[ 2 ]).ToString( "X" )
                                              + ":" + ((uint)itf.PhysicalAddress[ 3 ]).ToString( "X" )
                                              + ":" + ((uint)itf.PhysicalAddress[ 4 ]).ToString( "X" )
                                              + ":" + ((uint)itf.PhysicalAddress[ 5 ]).ToString( "X" )
                                              );
                    return;
                }
            }while( StatusChanged.WaitOne() );
        }

        private void NetworkChange_NetworkAddressChanged( object sender, EventArgs e )
        {
            Debug.Print( "A network address changed" );
            StatusChanged.Set( );
        }

        private readonly System.Threading.ManualResetEvent StatusChanged = new System.Threading.ManualResetEvent( false );
    }
}
