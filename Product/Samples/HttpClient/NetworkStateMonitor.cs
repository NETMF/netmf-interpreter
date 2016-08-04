/*
The MIT License (MIT)

Copyright (c) 2015 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 THE SOFTWARE.
*/
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Net.NetworkInformation;

namespace HttpClientSample
{
    /// <summary>
    /// This class provides common support for monitoring and responding
    /// to network state changes on a device
    /// </summary>
    public class NetworkStateMonitor
    {
        public NetworkStateMonitor( )
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
                StatusChanged.Reset( );
                foreach( var itf in NetworkInterfaces )
                {
                    if( itf.IPAddress == "0.0.0.0" || itf.IPAddress == "127.0.0.1" )
                        continue;

                    Debug.Print( "Found IP: " + itf.IPAddress );
                    return;
                }
            } while( StatusChanged.WaitOne( ) );
        }

        private void NetworkChange_NetworkAddressChanged( object sender, EventArgs e )
        {
            Debug.Print( "A network address changed" );
            StatusChanged.Set( );
        }

        private readonly ManualResetEvent StatusChanged = new ManualResetEvent( false );
    }
}
