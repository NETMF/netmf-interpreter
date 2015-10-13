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
using Microsoft.SPOT.Time;
using System;
using System.Net;
using System.Threading;

namespace HttpClientSample
{
    /// <summary>Implements common boilerplate code for using the TimeService</summary>
    static class TimeServiceManager
    {
        /// <summary>Initializes the TimeService and waits for a valid time from the server</summary>
        /// <param name="timeServiceName">Host name of the time service server to use</param>
        /// <param name="timeZoneOffset">Timezone offset (in minutes) to use for the service</param>
        public static void InitTimeService( string timeServiceName, int timeZoneOffset )
        {
            // Get the address for the time service.
            // It is considered bad practice to hard code the address
            // as the service will likely load balance to a variety
            // of physical IP addresses. 
            IPHostEntry entry = Dns.GetHostEntry( timeServiceName );
            if( entry == null || entry.AddressList == null )
                throw new ApplicationException( "DNS failure" );

            // It is possible (at least on the emulator) that
            // an address in the list may be null, so loop
            // over them until a valid one is found.
            IPAddress timeServiceAddress = null;
            for( int i = 0; i < entry.AddressList.Length; ++i )
            {
                timeServiceAddress = entry.AddressList[ i ];
                if( timeServiceAddress != null )
                    break;
            }

            // need to have a valid one to continue 
            if( timeServiceAddress == null )
                throw new ApplicationException( "DNS failure" );

            TimeService.Settings = new TimeServiceSettings
                { PrimaryServer = timeServiceAddress.GetAddressBytes( )
                , RefreshTime = 10
                , AutoDayLightSavings = true
                };

            TimeService.SetTimeZoneOffset( timeZoneOffset );

            // Start the service and wait for initial time update/sync from server
            TimeService.SystemTimeChanged += TimeService_SystemTimeChanged;
            TimeService.Start( );
            TimeChanged.WaitOne( );
            TimeService.SystemTimeChanged -= TimeService_SystemTimeChanged;
        }

        private static void TimeService_SystemTimeChanged( object sender, SystemTimeChangedEventArgs e )
        {
            TimeChanged.Set( );
        }
        static ManualResetEvent TimeChanged = new ManualResetEvent( false );
    }
}
