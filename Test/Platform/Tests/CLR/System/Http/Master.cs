/*---------------------------------------------------------------------
* Master.cs - file description
* Main class, responsible for running all of the other *Tests.cs files
* Version: 1.0
* Author: REDMOND\jamesweb
* Created: 2/27/2009 10:20:51 AM 
* ---------------------------------------------------------------------*/
using System;
using System.Net;
using System.IO;
using System.Text;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class HttpTests 
    {
        // manually set proxy server here - In future should get from config
        public static string Proxy = "itgproxy.redmond.corp.microsoft.com";
        public static string MSUrl = "http://www.microsoft.com";
        private static Decoder UTF8decoder = System.Text.Encoding.UTF8.GetDecoder();

        public static void Main()
        {
            string[] tests = {
                // not implmented
                //"ProtocolViolationExceptionTests",
                //"WebProxyTests",
                //"HttpWebResponseTests",
                //"WebHeaderCollectionTests",

                // in progress 

                // completed
                "HttpStatusCodeTests",
                "UriTests",
                "HttpVersionTests",
                "FunctionalTests",
                "Base64Tests",
                "WebRequestTests",
                "WebResponseTests",
                "HttpWebRequestTests",
                "HttpRequestHeaderTests",
                "HttpKnownHeaderNamesTests",
                //"AuthenticationTests",
                "WebExceptionTests",
            };

            MFTestRunner runner = new MFTestRunner(tests);
        }

        public static bool ValidateException(Exception ex, Type ExpectedException)
        {
            bool result = (ex.GetType() == ExpectedException);
            if (!result)
                Log.Exception("Expected exception type: " + ExpectedException.Name, ex);

            return result;
        }

        public static string ReadStream(string source, Stream Reader)
        {
            Reader.ReadTimeout = 1000;
            byte[] byteData = new byte[4096];
            char[] charData = new char[4096];
            string data = null;
            int bytesRead = 0;
            int totalBytes = 0;
            try
            {
                while ((bytesRead = Reader.Read(byteData, 0, byteData.Length)) > 0)
                {
                    int byteUsed, charUsed;
                    bool completed = false;
                    totalBytes += bytesRead;
                    UTF8decoder.Convert(byteData, 0, bytesRead, charData, 0, bytesRead, true, out byteUsed, out charUsed, out completed);
                    data = data + new String(charData, 0, charUsed);
                    Log.Comment("[" + source + "] Bytes Read Now: " + bytesRead + " Total: " + totalBytes);
                }
            }
            catch(System.Net.Sockets.SocketException se)
            {
                if (se.ErrorCode != (int)System.Net.Sockets.SocketError.TimedOut)
                {
                    throw se;
                }
            }
            Log.Comment("[" + source + "] Total bytes in message body : " + totalBytes);
            Log.Comment("[" + source + "] Received: " + data);
            return data;
         }

        public static void PrintHeaders(string source, WebHeaderCollection headers)
        {
            Log.Comment("[" + source + "] Rcvd Headers:");
            foreach (string header in headers.AllKeys)
            {
                Log.Comment("[" + source + "] " + header + ": " + headers[header]);
            }
        }
    }
}