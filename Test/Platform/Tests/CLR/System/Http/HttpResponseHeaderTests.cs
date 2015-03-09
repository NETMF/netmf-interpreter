////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Net;

namespace Microsoft.SPOT.Platform.Tests
{
    public class HttpResponseHeaderTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            // Add your functionality here.   

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");

            // TODO: Add your clean up steps here.
        }


        #region Helper methods
        private MFTestResults StopServerListener(ref HttpServer server)
        {
            MFTestResults result = MFTestResults.Pass;
            HttpListener mylistener = server.Listener;
            try
            {
                mylistener.Stop();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception when stop listener: " + ex.Message.ToString());
                result = MFTestResults.Fail;
            }
            return result;
        }

        private MFTestResults SetCommonHttpResponseHeaders(ref System.Net.WebHeaderCollection wrs)
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                Log.Comment("Set Response Headers Properties");
                wrs.Set(HttpKnownHeaderNames.AcceptRanges.ToString(), "bytes");
                wrs.Set(HttpKnownHeaderNames.Age.ToString(), "0");
                wrs.Set(HttpKnownHeaderNames.Allow.ToString(), "GET, PUT");
                wrs.Set(HttpKnownHeaderNames.CacheControl.ToString(), "no-cache"); //Force intermediate caches to validate their copies directly with the origin server.
                wrs.Set(HttpKnownHeaderNames.Connection.ToString(), "close");
                wrs.Set(HttpKnownHeaderNames.ContentEncoding.ToString(), "gzip");
                wrs.Set(HttpKnownHeaderNames.ContentLanguage.ToString(), "mi, en");
                wrs.Set(HttpKnownHeaderNames.ContentLength.ToString(), "26012");
                wrs.Set(HttpKnownHeaderNames.ContentLocation.ToString(), "");
                wrs.Set(HttpKnownHeaderNames.ContentMD5.ToString(), "60e985979f1d55ab7542440fbb9659e5");
                wrs.Set(HttpKnownHeaderNames.ContentRange.ToString(), "bytes 21010-47021/47022");
                wrs.Set(HttpKnownHeaderNames.ContentType.ToString(), "text/plain, image/gif");
                wrs.Set(HttpKnownHeaderNames.Date.ToString(), System.DateTime.Today.ToString());
                wrs.Set(HttpKnownHeaderNames.ETag.ToString(), "W/");
                wrs.Set(HttpKnownHeaderNames.Expires.ToString(), "Thu, 01 Dec 1994 16:00:00 GMT");  //always force client cache validate on the request
                wrs.Set(HttpKnownHeaderNames.KeepAlive.ToString(), "");
                wrs.Set(HttpKnownHeaderNames.LastModified.ToString(), "Fri, 22 May 2009 12:43:31 GMT");
                wrs.Set(HttpKnownHeaderNames.Location.ToString(), "http://www.w3.org/pub/WWW/People.html");
                wrs.Set(HttpKnownHeaderNames.Pragma.ToString(), "no-cache");
                wrs.Set(HttpKnownHeaderNames.ProxyAuthenticate.ToString(), "NNNNNNNNNNNNNNNNN==");
                wrs.Set(HttpKnownHeaderNames.RetryAfter.ToString(), "120");
                wrs.Set(HttpKnownHeaderNames.SetCookie.ToString(), "http://www.w3.org/hypertext/DataSources/Overview.html");
                wrs.Set(HttpKnownHeaderNames.Trailer.ToString(), "Test Code");
                wrs.Set(HttpKnownHeaderNames.TransferEncoding.ToString(), "8BIT");
                wrs.Set(HttpKnownHeaderNames.Upgrade.ToString(), "HTTP/2.0, SHTTP/1.3");
                wrs.Set(HttpKnownHeaderNames.Vary.ToString(), "TestVary");
                wrs.Set(HttpKnownHeaderNames.Via.ToString(), "1.0 fred, 1.1 nowhere.com (Apache/1.1)");
                wrs.Set(HttpKnownHeaderNames.Warning.ToString(), "TestWarning");
                wrs.Set(HttpKnownHeaderNames.WWWAuthenticate.ToString(), "BASIC realm=\"executives\"");
            }
            catch (Exception ex)
            {
                if (!HttpTests.ValidateException(ex, typeof(InvalidOperationException)))
                    result = MFTestResults.Fail;
            }

            return result;
        }


        private MFTestResults SetCommonHttpResponseHeaders_1_0(ref System.Net.WebHeaderCollection wrs)
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                Log.Comment("Set Response Headers Properties");
                wrs.Set(HttpKnownHeaderNames.Allow.ToString(), "GET");
                wrs.Set(HttpKnownHeaderNames.ContentEncoding.ToString(), "gzip");
                wrs.Set(HttpKnownHeaderNames.ContentLength.ToString(), "26012");
                wrs.Set(HttpKnownHeaderNames.ContentType.ToString(), "text/plain, image/gif");
                wrs.Set(HttpKnownHeaderNames.Expires.ToString(), "Thu, 01 Dec 1994 16:00:00 GMT");  //always force client cache validate on the request
                wrs.Set(HttpKnownHeaderNames.KeepAlive.ToString(), "");
                wrs.Set(HttpKnownHeaderNames.LastModified.ToString(), "Fri, 22 May 2009 12:43:31 GMT");
                wrs.Set(HttpKnownHeaderNames.Location.ToString(), "http://www.w3.org/pub/WWW/People.html");
                wrs.Set(HttpKnownHeaderNames.WWWAuthenticate.ToString(), "BASIC realm=\"executives\"");
            }
            catch (Exception ex)
            {
                if (!HttpTests.ValidateException(ex, typeof(InvalidOperationException)))
                    result = MFTestResults.Fail;
            }

            return result;
        }


        private MFTestResults VerifyHttpResponseHeaders(System.Net.WebHeaderCollection wrc, System.Net.WebHeaderCollection wrs)
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                string[] headers = wrc.AllKeys;
                string sValue = String.Empty;

                for (int i = 0; i < wrc.Count; i++)
                {
                    sValue = wrc[headers[i]];
                    if (sValue != wrs[headers[i]])
                    {
                        Log.Exception(headers[i] + "property value is incorrect.");
                        result = MFTestResults.Fail;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!HttpTests.ValidateException(ex, typeof(InvalidOperationException)))
                    result = MFTestResults.Fail;
            }

            return result;
        }

        #endregion helper methods


        #region Test
        [TestMethod]
        public MFTestResults ValidDefaultTestGetHTTPResponseHeaderAfterCreateHTTP1_1()
        {
            MFTestResults result = MFTestResults.Pass;

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:8080/");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", 8080, ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>ValidDefaultTestGetHTTPResponseHeaderAfterCreateHTTP1_1</body></html>" 
            };

            try
            {             
                // Setup server
                server.StartServer();
                System.Net.WebHeaderCollection wrs = server.RequestHeaders;

                SetCommonHttpResponseHeaders(ref wrs);

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                WebHeaderCollection wrc = wr.Headers;

                VerifyHttpResponseHeaders(wrc, wrs);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                StopServerListener(ref server);
                //Stop server
                server.StopServer();
            }

            return result;
        }

        //This test case get System.NotSupportedException
        public MFTestResults ValidDefaultTestGetHTTPResponseHeaderAfterCreateHTTP1_1_https()
        {
            MFTestResults result = MFTestResults.Pass;

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("https://127.0.0.1:443/");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("https", 443, ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
            };

            try
            {
                // Setup server
                server.StartServer();
                System.Net.WebHeaderCollection wrs = server.RequestHeaders;

                SetCommonHttpResponseHeaders(ref wrs);

                //Get System.NotSupportedException when retrive response --  Need further investigation
                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                WebHeaderCollection wrc = wr.Headers;

                VerifyHttpResponseHeaders(wrc, wrs);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                StopServerListener(ref server);
                //Stop server
                server.StopServer();
            }

            return result;
        }


        public MFTestResults InValidDefaultTestGetHTTPResponseHeaderAfterCreateHTTP1_1_ftp()
        {
            MFTestResults result = MFTestResults.Pass;

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://ftp.microsoft.com");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.0");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", 8080, ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
            };

            try
            {
                // Setup server
                server.StartServer();
                System.Net.WebHeaderCollection wrs = server.RequestHeaders;

                SetCommonHttpResponseHeaders(ref wrs);

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                WebHeaderCollection wrc = wr.Headers;

                VerifyHttpResponseHeaders(wrc, wrs);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                StopServerListener(ref server);
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public MFTestResults ValidDefaultTestGetHTTPResponseHeaderAfterCreateHTTP1_0()
        {
            MFTestResults result = MFTestResults.Pass;

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:8080/");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.0

            Log.Comment("Set Version 1.0");
            wr.ProtocolVersion = new Version(1, 0);

            HttpServer server = new HttpServer("http", 8080, ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>ValidDefaultTestGetHTTPResponseHeaderAfterCreateHTTP1_0</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();
                System.Net.WebHeaderCollection wrs = server.RequestHeaders;

                SetCommonHttpResponseHeaders_1_0(ref wrs);

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                WebHeaderCollection wrc = wr.Headers;

                VerifyHttpResponseHeaders(wrc, wrs);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                StopServerListener(ref server);
                //Stop server
                server.StopServer();
            }

            return result;
        }


        //This test case get System.NotSupportedException
        public MFTestResults ValidDefaultTestGetHTTPResponseHeaderAfterCreateHTTP1_1_https1_0()
        {
            MFTestResults result = MFTestResults.Pass;

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("https://127.0.0.1:443/");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.0");
            wr.ProtocolVersion = new Version(1, 0);

            HttpServer server = new HttpServer("https", 443, ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
            };

            try
            {
                // Setup server
                server.StartServer();
                System.Net.WebHeaderCollection wrs = server.RequestHeaders;

                SetCommonHttpResponseHeaders_1_0(ref wrs);

                //Get System.NotSupportedException when retrive response --  Need further investigation
                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                WebHeaderCollection wrc = wr.Headers;

                VerifyHttpResponseHeaders(wrc, wrs);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                StopServerListener(ref server);
                //Stop server
                server.StopServer();
            }

            return result;
        }


        public MFTestResults InValidDefaultTestGetHTTPResponseHeaderAfterCreateHTTP1_1_ftp1_0()
        {
            MFTestResults result = MFTestResults.Pass;

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://ftp.microsoft.com");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.0");
            wr.ProtocolVersion = new Version(1, 0);

            HttpServer server = new HttpServer("http", 8080, ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
            };

            try
            {
                // Setup server
                server.StartServer();
                System.Net.WebHeaderCollection wrs = server.RequestHeaders;

                SetCommonHttpResponseHeaders_1_0(ref wrs);

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                WebHeaderCollection wrc = wr.Headers;

                VerifyHttpResponseHeaders(wrc, wrs);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                StopServerListener(ref server);
                //Stop server
                server.StopServer();
            }

            return result;
        }

        #endregion Test
    }
}
