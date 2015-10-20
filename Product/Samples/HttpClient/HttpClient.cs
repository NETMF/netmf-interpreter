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
using Microsoft.SPOT;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

/// This program demonstrates how to use the .NET Micro Framework HTTP classes 
/// to create a simple HTTP client that retrieves pages from several different 
/// websites, including secure sites.
namespace HttpClientSample
{
    public static class MyHttpClient
    {
        // Ideally the preferred time service name should be stored in some form of persistent memory
        private const string TimeServiceName = "time.nist.gov";
        
        // Ideally the timeszone offset should be stored in some form of persistent memory
        private const int TimeZoneOffset = -7 * 60; // [ UTC -8 timezone (+1 for DST ) ]

        private static NetworkStateMonitor NetMonitor = new NetworkStateMonitor( );
        private static Decoder UTF8decoder = Encoding.UTF8.GetDecoder( );

        /// <summary>Retrieves pages from a Web servers, using a simple HTTP requests</summary>
        public static void Main( )
        {
            // Root CA Certificate needed to validate HTTPS servers.
            var validButIncorrectCert = new X509Certificate( Resources.GetBytes( Resources.BinaryResources.VerisignCA ) );
            var validAndCorrectCert = new X509Certificate( Resources.GetBytes( Resources.BinaryResources.DigiCert ) );

            X509Certificate[ ] willFailCerts = { validButIncorrectCert };
            X509Certificate[ ] willSucceedCerts = { validButIncorrectCert, validAndCorrectCert };

            NetMonitor.WaitForIpAddress();
            TimeServiceManager.InitTimeService( TimeServiceName, TimeZoneOffset );
            Debug.Print(" Time is: " + DateTime.Now.ToString());
            
            // Print the HTTP data from each of the following pages.

            // Response Should be a simple re-direct plus "moved" message
            Debug.Print( "Fetching data from: http://autos.msn.com/default.aspx" );
            PrintHttpData( "http://autos.msn.com/default.aspx", null );

            // Test SSL connection with no certificate verification
            // NOTE: This is not expected to generate an error or fail condition.
            // Since no certificates were provided no verification is performed.
            // This may or may not be an issue depending on circumstances. It is
            // certainly useful during development before a valid cert is available
            // for a server, however it is generally not recommended for production
            // systems as it opens the door to "man in the middle" type attacks. 
            Debug.Print( "Fetching data from: https://github.com/NETMF/netmf-interpreter no cert validation" );
            PrintHttpData( "https://github.com/NETMF/netmf-interpreter", null );

            // Read from secure webpages by using the provided Root
            // certificates that are stored in the Resource.resx file.
            Debug.Print( "Fetching data from: https://github.com/NETMF/netmf-interpreter with valid cert validation" );
            PrintHttpData( "https://github.com/NETMF/netmf-interpreter", willSucceedCerts );

            // This is expected to generate a WebException since the array of certificates
            // provided doesn't include any that can be used to validate the site's certificate
            try
            {
                Debug.Print( "Fetching data from: https://github.com/NETMF/netmf-interpreter with invalid cert validation" );
                PrintHttpData( "https://github.com/NETMF/netmf-interpreter", willFailCerts );
            }
            catch( WebException ex )
            {
                var innerException = ex.InnerException as SocketException;
                if( innerException != null && innerException.ErrorCode == 1 )
                {
                    Debug.Print( "Got expected Exception..." );
                }
            }
        }

        /// <summary>
        /// Prints the HTTP Web page from the given URL and status data while 
        /// receiving the page.
        /// </summary>
        /// <param name="url">The URL of the page to print.</param>
        /// <param name="caCerts">The root CA certificates that are required for 
        /// validating a secure website (HTTPS).</param>
        public static void PrintHttpData( string url, X509Certificate[ ] caCerts )
        {
            try
            {
                // Create an HTTP Web request.
                HttpWebRequest request = WebRequest.Create( url ) as HttpWebRequest;
                Debug.Assert( request != null );

                // Assign the certificates. If this is null, then no
                // validation of server certificates is performed.
                request.HttpsAuthentCerts = caCerts;

                // Get a response from the server.
                // process the response
                using( WebResponse resp = request.GetResponse( ) )
                {
                    ProcessResponse( resp );
                }

            }
            catch( WebException ex )
            {
                var innerException = ex.InnerException as SocketException;
                if( caCerts != null && innerException != null && innerException.ErrorCode == 1 )
                {
                    throw;
                }
                Debug.Print( ex.Message );
            }
        }

        private static void ProcessResponse( WebResponse resp )
        {
            // Get the network response stream to read the page data.
            Stream respStream = resp.GetResponseStream( );
            StringBuilder page = new StringBuilder( );
            byte[ ] byteData = new byte[ 4096 ];
            int bytesRead = 0;
            int totalBytes = 0;

            // allow up to 15 seconds as a timeout for reading the stream
            respStream.ReadTimeout = 15000;

            // If the content length was provided, read exactly that amount of 
            // data; otherwise, read until there is nothing left to read.
            if( resp.ContentLength != -1 )
            {
                for( int dataRem = ( int )resp.ContentLength; dataRem > 0; )
                {
                    bytesRead = ReadData( respStream, byteData );
                    if( bytesRead == 0 )
                    {
                        Debug.Print( "Error: Received " + ( resp.ContentLength - dataRem ) + " Out of " + resp.ContentLength );
                        break;
                    }
                    dataRem -= bytesRead;
                    totalBytes += bytesRead;
                    AppendPageData( page, byteData, bytesRead, totalBytes );
                }
            }
            else
            {
                // Read until the end of the data is reached.
                while( true )
                {
                    bytesRead = ReadData( respStream, byteData );

                    // Zero bytes indicates the connection has been closed 
                    // by the server or some other error, either way there's
                    // no more data to process.
                    if( bytesRead == 0 )
                    {
                        break;
                    }
                    totalBytes += bytesRead;
                    AppendPageData( page, byteData, bytesRead, totalBytes );
                }

                Debug.Print( "Total bytes downloaded in message body : " + totalBytes );
            }

            // Display the page results.
            Debug.Print( page.ToString( ) );
        }

        private static int ReadData( Stream respStream, byte[ ] byteData )
        {
            // If the Read method times out, it throws an exception, 
            // which is expected for Keep-Alive streams because the 
            // connection isn't terminated.
            int bytesRead;
            try
            {
                bytesRead = respStream.Read( byteData, 0, byteData.Length );
            }
            catch( IOException )
            {
                bytesRead = 0;
            }

            return bytesRead;
        }

        private static void  AppendPageData( StringBuilder page
                                           , byte[ ] byteData
                                           , int bytesRead
                                           , int totalBytes
                                           )
        {
            // Convert from bytes to chars, and add to the page.
            int byteUsed;
            int charUsed = 0;
            bool completed = false;
            var charData = new char[ 4096 ];

            while( !completed )
            {
                UTF8decoder.Convert( byteData
                                   , 0
                                   , bytesRead
                                   , charData
                                   , 0
                                   , bytesRead
                                   , true
                                   , out byteUsed
                                   , out charUsed
                                   , out completed
                                   );
            }
            page.Append( new string( charData, 0, charUsed ) );

            // Display the page download status.
            Debug.Print( "Bytes Read Now: " + bytesRead + " Total: " + totalBytes );
        }
    }
}
