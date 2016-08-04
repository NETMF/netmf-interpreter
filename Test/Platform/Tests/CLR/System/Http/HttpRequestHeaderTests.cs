////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Net;

namespace Microsoft.SPOT.Platform.Tests
{
    public class HttpRequestHeaderTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");

            try
            {
                Microsoft.SPOT.Net.NetworkInformation.NetworkInterface[] nis = Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            }
            catch
            {
                return InitializeResult.Skip;
            }


            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");

            // TODO: Add your clean up steps here.
        }

        #region Helper methods
        private MFTestResults Verify(System.Net.WebHeaderCollection wrc, System.Net.WebHeaderCollection RequestHeaders)
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                Log.Comment("Get Headers - User-Agent");
                if (wrc["User-Agent"] != RequestHeaders["User-Agent"])
                {
                    Log.Exception("User-Agent property value is incorrect.");
                    result = MFTestResults.Fail;
                }

                Log.Comment("Get Headers - Connection");
                if (wrc["Connection"] != RequestHeaders["Connection"])
                {
                    Log.Exception("Connection property value is incorrect.");
                    result = MFTestResults.Fail;
                }

                Log.Comment("Get Headers - Host");
                if (wrc["Host"] != RequestHeaders["Host"])
                {
                    Log.Exception("Host property value is incorrect.");
                    result = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                if (!HttpTests.ValidateException(ex, typeof(InvalidOperationException)))
                    result = MFTestResults.Fail;
            }

            return result;
        }

        #endregion Helper methods


        #region Test
        [TestMethod]
        public MFTestResults ValidDefaultTestGetHTTPRequestHeaderAfterCreateHTTP1_1()
        {
            MFTestResults result = MFTestResults.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
            };

            try
            {
                // Setup server
                server.StartServer();
                System.Net.WebHeaderCollection wrc = wr.Headers;
                
                // Tests
                Verify(wrc, server.RequestHeaders);
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }

        [TestMethod]
        public MFTestResults ValidDefaultTestGetHTTPRequestHeaderAfterCreateHTTP1_1_Https()
        {
            MFTestResults result = MFTestResults.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("https://www.microsoft.com:443/");
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

                System.Net.WebHeaderCollection wrc = wr.Headers;

                // Tests
                Verify(wrc, server.RequestHeaders);
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                // Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public MFTestResults InValidDefaultTestGetHTTPRequestHeaderAfterCreateHTTP1_1_Https()
        {
            MFTestResults result = MFTestResults.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("https://"+ Utilities.GetLocalIpAddress() + ":443/");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Negative Test case 1:");
            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
            };

            try
            {
                // Setup server
                server.StartServer();

                System.Net.WebHeaderCollection wrc = wr.Headers;

                // Tests
                Verify(wrc, server.RequestHeaders);
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                // Stop server
                server.StopServer();
            }

            return result;
        }

        [TestMethod]
        public MFTestResults InvalidDefaultTestGetHTTPRequestHeaderAfterCreateHTTP1_1_FTP()
        {
            MFTestResults result = MFTestResults.Pass;
            UriProperties props = new UriProperties("ftp", "//ftp.microsoft.com");
            Uri uri = new Uri(props.OriginalUri);

            Log.Comment("Negative Test case 2:");
            Log.Comment("Create WebRequest with FTP uri");
            try
            {
                HttpWebRequest wrftp = (HttpWebRequest)WebRequest.Create(uri);
            }
            catch (System.NotSupportedException)
            {
                Log.Comment("Create WebRequest with FTP uri - Expected System.NotSupportedException");
            }

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://ftp.microsoft.com");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
            };

            try
            {
                // Setup server
                server.StartServer();

                System.Net.WebHeaderCollection wrc = wr.Headers;

                // Tests
                Verify(wrc, server.RequestHeaders);
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                // Stop server
                server.StopServer();
            }

            return result;
        }

        [TestMethod]
        public MFTestResults InvalidDefaultTestGetHTTPRequestHeaderAfterCreateHTTP1_1()
        {
            MFTestResults result = MFTestResults.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://ftp");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Negative Test case 3:");
            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
            };

            try
            {
                // Setup server
                server.StartServer();

                System.Net.WebHeaderCollection wrc = wr.Headers;

                // Tests
                Verify(wrc, server.RequestHeaders);
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                // Stop server
                server.StopServer();
            }

            return result;
        }
        

        [TestMethod]
        public MFTestResults ValidDefaultTestGetHTTPRequestHeaderAfterCreateHTTP1_0()
        {
            MFTestResults result = MFTestResults.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.0");
            wr.ProtocolVersion = new Version(1, 0);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
            };

            try
            {
                // Setup server
                server.StartServer();
                System.Net.WebHeaderCollection wrc = wr.Headers;

                // Tests
                Verify(wrc, server.RequestHeaders);
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                // Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public MFTestResults ValidDefaultTestGetHTTPRequestHeaderAfterCreateHTTP1_0_HTTPS()
        {
            MFTestResults result = MFTestResults.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("https://"+ Utilities.GetLocalIpAddress() + ":443/");
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
                System.Net.WebHeaderCollection wrc = wr.Headers;

                // Tests
                Verify(wrc, server.RequestHeaders);
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                // Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public MFTestResults InValidDefaultTestGetHTTPRequestHeaderAfterCreateHTTP1_0_HTTPS()
        {
            MFTestResults result = MFTestResults.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("https://"+ Utilities.GetLocalIpAddress() + ":443/");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Negative Test case 4:");
            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.0");
            wr.ProtocolVersion = new Version(1, 0);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
            };

            try
            {
                // Setup server
                server.StartServer();
                System.Net.WebHeaderCollection wrc = wr.Headers;

                // Tests
                Verify(wrc, server.RequestHeaders);
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                // Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public MFTestResults InValidDefaultTestGetHTTPRequestHeaderAfterCreateHTTP1_0_FTP()
        {
            MFTestResults result = MFTestResults.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://ftp");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Negative Test case 5:");
            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.0");
            wr.ProtocolVersion = new Version(1, 0);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
            };

            try
            {
                // Setup server
                server.StartServer();
                System.Net.WebHeaderCollection wrc = wr.Headers;

                // Tests
                Verify(wrc, server.RequestHeaders);
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                // Stop server
                server.StopServer();
            }

            return result;
        }


        #endregion Test
    }
}
