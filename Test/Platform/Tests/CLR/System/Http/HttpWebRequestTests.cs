////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Net;

namespace Microsoft.SPOT.Platform.Tests
{
    public class HttpWebRequestTests : IMFTestInterface
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

        #region Test Cases

        [TestMethod]
        public MFTestResults ConstructorTestsValid()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Create string request");
                HttpWebRequest strRequest = (HttpWebRequest)HttpWebRequest.Create(HttpTests.MSUrl);

                Log.Comment("Create URI request");
                Uri uri = new Uri(HttpTests.MSUrl);
                HttpWebRequest uriRequest = (HttpWebRequest)HttpWebRequest.Create(uri);
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults ConstructorTestsNullArgs()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Create string request");
                string URL = null;
                try
                {
                    HttpWebRequest strRequest = (HttpWebRequest)HttpWebRequest.Create(URL);
                    Log.Exception("Expected ArgumentNullException");
                    result = MFTestResults.Fail;
                }
                catch (Exception ex)
                {
                    /* pass case */
                    if (!HttpTests.ValidateException(ex, typeof(ArgumentNullException)))
                        result = MFTestResults.Fail;
                }

                Log.Comment("Create URI request");
                Uri uri = null;
                try
                {
                    HttpWebRequest uriRequest = (HttpWebRequest)HttpWebRequest.Create(uri);
                    Log.Exception("Expected ArgumentNullException");
                    result = MFTestResults.Fail;
                }
                catch (Exception ex)
                {
                    /* pass case */
                    if (!HttpTests.ValidateException(ex, typeof(ArgumentNullException)))
                        result = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults ConstructorTestsEmptyArgs()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Create string request");
                string URL = "";
                try
                {
                    HttpWebRequest strRequest = (HttpWebRequest)HttpWebRequest.Create(URL);
                    Log.Exception("Expected ArgumentException");
                    result = MFTestResults.Fail;
                }
                catch (Exception ex)
                {
                    /* pass case */
                    if (!HttpTests.ValidateException(ex, typeof(ArgumentNullException)))
                        result = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                result = MFTestResults.KnownFailure;
            }
            return result;
        }

        [TestMethod]
        public MFTestResults AddRangeValidTests()
        {
            MFTestResults result = MFTestResults.Pass;

            HttpWebRequest wr;
            try
            {
                Log.Comment("AddRange(1)");
                wr = (HttpWebRequest)WebRequest.Create(HttpTests.MSUrl);
                wr.AddRange(1);
                if (wr.Headers["Range"] != "bytes=1-")
                {
                    Log.Exception("Expected bytes=1-, but got " + wr.Headers["Range"]);
                    result = MFTestResults.Fail;
                }

                Log.Comment("AddRange(-1)");
                wr = (HttpWebRequest)WebRequest.Create(HttpTests.MSUrl);
                wr.AddRange(-1);
                if (wr.Headers["Range"] != "bytes=-1")
                {
                    Log.Exception("Expected bytes=-1, but got " + wr.Headers["Range"]);
                    result = MFTestResults.Fail;
                }
                Log.Comment("AddRange(100, 500)");
                wr = (HttpWebRequest)WebRequest.Create(HttpTests.MSUrl);
                wr.AddRange(100, 500);
                if (wr.Headers["Range"] != "bytes=100-500")
                {
                    Log.Exception("Expected bytes=100-500, but got " + wr.Headers["Range"]);
                    result = MFTestResults.Fail;
                }

                Log.Comment("MultiRange - AddRange(1, 5) + AddRange(100, 500)");
                wr = (HttpWebRequest)WebRequest.Create(HttpTests.MSUrl);
                wr.AddRange(1, 5);
                wr.AddRange(100, 500);
                if (wr.Headers["Range"] != "bytes=1-5,100-500")
                {
                    Log.Exception("Expected bytes=1-5,100-500, but got " + wr.Headers["Range"]);
                    result = MFTestResults.Fail;
                }

                Log.Comment("AddRange(chars, 5000)");
                wr = (HttpWebRequest)WebRequest.Create(HttpTests.MSUrl);
                wr.AddRange("chars", 5000);
                if (wr.Headers["Range"] != "chars=5000-")
                {
                    Log.Exception("Expected chars=5000-, but got " + wr.Headers["Range"]);
                    result = MFTestResults.Fail;
                }

                Log.Comment("AddRange(strings, 800, 900)");
                wr = (HttpWebRequest)WebRequest.Create(HttpTests.MSUrl);
                wr.AddRange("strings", 800, 900);
                if (wr.Headers["Range"] != "strings=800-900")
                {
                    Log.Exception("Expected strings=800-900, but got " + wr.Headers["Range"]);
                    result = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults AddRangeInvalidTests()
        {
            MFTestResults result = MFTestResults.Pass;
            HttpWebRequest wr;

            try
            {
                Log.Comment("invalid from - AddRange(-1, 500)");
                wr = (HttpWebRequest)WebRequest.Create(HttpTests.MSUrl);
                try
                {
                    wr.AddRange(-1, 500);
                    Log.Exception("Expected ArgumentOutOfRangeException");
                    result = MFTestResults.Fail;
                }
                catch (Exception ex)
                {
                    /* pass case */
                    if (!HttpTests.ValidateException(ex, typeof(ArgumentOutOfRangeException)))
                        result = MFTestResults.Fail;
                }

                Log.Comment("invalid from - AddRange(1, -500)");
                wr = (HttpWebRequest)WebRequest.Create(HttpTests.MSUrl);
                try
                {
                    wr.AddRange(1, -500);
                    Log.Exception("Expected ArgumentOutOfRangeException");
                    result = MFTestResults.Fail;
                }
                catch (Exception ex)
                {
                    /* pass case */
                    if (!HttpTests.ValidateException(ex, typeof(ArgumentOutOfRangeException)))
                        result = MFTestResults.Fail;
                }

                Log.Comment("invalid specifier - AddRange(null, 500)");
                wr = (HttpWebRequest)WebRequest.Create(HttpTests.MSUrl);
                try
                {
                    wr.AddRange(null, 500);
                    Log.Exception("Expected ArgumentNullException");
                    result = MFTestResults.Fail;
                }
                catch (Exception ex)
                {
                    /* pass case */
                    if (!HttpTests.ValidateException(ex, typeof(ArgumentNullException)))
                        result = MFTestResults.Fail;
                }

                Log.Comment("invalid specifier - AddRange(null, 1, 500)");
                wr = (HttpWebRequest)WebRequest.Create(HttpTests.MSUrl);
                try
                {
                    wr.AddRange(null, 1, 500);
                    Log.Exception("Expected ArgumentNullException");
                    result = MFTestResults.Fail;
                }
                catch (Exception ex)
                {
                    /* pass case */
                    if (!HttpTests.ValidateException(ex, typeof(ArgumentNullException)))
                        result = MFTestResults.Fail;
                }

            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults CheckBadHeaders()
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                Log.Comment("low byte");
                if (!TestCheckBadHeaders("low byte \u0031 embedded string"))
                    result = MFTestResults.Fail;

                Log.Comment("high byte");
                if (!TestCheckBadHeaders("high byte \u0128 embedded string"))
                    result = MFTestResults.Fail;

                Log.Comment("bad nl wrap");
                if (!TestCheckBadHeaders("bad\nwrap"))
                    result = MFTestResults.Fail;

                Log.Comment("bad crlf wrap");
                if (!TestCheckBadHeaders("bad\r\nwrap"))
                    result = MFTestResults.Fail;

                Log.Comment("embedded tab");
                if (!TestCheckBadHeaders("embedded\ttab"))
                    result = MFTestResults.Fail;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }
            return result;
        }

        private bool TestCheckBadHeaders(string value)
        {
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(HttpTests.MSUrl); ;

            try
            {
                wr.Headers.Add("Test", value);
            }
            catch (Exception ex)
            {
                if (!HttpTests.ValidateException(ex, typeof(ArgumentException)))
                    return false;
            }
            return true;
        }

        [TestMethod]
        public MFTestResults AcceptHeaderTest()
        {
            MFTestResults result = MFTestResults.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(HttpTests.MSUrl); ;
            try
            {
                Log.Comment("Initial value should be null");
                if (wr.Accept != null)
                {
                    Log.Exception("Unexpected initial value: " + wr.Accept);
                    result = MFTestResults.Fail;
                }

                if (!ValidateAccept(wr, "text/plain"))
                    result = MFTestResults.Fail;

                if (!ValidateAccept(wr, "text/plain; q=0.5, text/html,\r\n\ttext/x-dvi; q=0.8, text/x-c"))
                    result = MFTestResults.Fail;

                if (!ValidateAccept(wr, "text/*;q=0.3, text/html;q=0.7, text/html;level=1,\r\n\ttext/html;level=2;q=0.4, */*;q=0.5"))
                    result = MFTestResults.Fail;

                Log.Comment("Clear w/null header");
                if (!ValidateAccept(wr, null))
                    result = MFTestResults.Fail;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        private bool ValidateAccept(HttpWebRequest wr, string mediaRange)
        {
            Log.Comment("Media-Range: " + mediaRange);
            wr.Accept = mediaRange;
            if (wr.Accept != mediaRange)
            {
                Log.Exception("Expected: " + mediaRange + ", but got: " + wr.Accept);
                return false;
            }
            return true;
        }

        [TestMethod]
        public MFTestResults ContentLengthHeader()
        {
            MFTestResults result = MFTestResults.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(HttpTests.MSUrl); ;

            try
            {
                Log.Comment("negative value should throw");
                try
                {
                    wr.ContentLength = -1;
                }
                catch (Exception ex)
                {
                    /* pass case */
                    if (!HttpTests.ValidateException(ex, typeof(ArgumentOutOfRangeException)))
                        result = MFTestResults.Fail;
                }

                wr.ContentLength = 1234567;
                if (wr.ContentLength != 1234567)
                {
                    Log.Exception("Expected ContentLength 1234567, but got " + wr.ContentLength);
                    result = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults ProtocolVersion()
        {
            MFTestResults result = MFTestResults.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(HttpTests.MSUrl);
            try
            {
                Log.Comment("Initial version: " + wr.ProtocolVersion);

                Log.Comment("Set Version 1.0");
                wr.ProtocolVersion = new Version(1, 0);
                if (wr.ProtocolVersion.Major != 1 && wr.ProtocolVersion.Minor != 0)
                {
                    Log.Exception("Expected version 1.0, but got " + wr.ProtocolVersion.ToString());
                    result = MFTestResults.Fail;
                }

                Log.Comment("Set Version 1.1");
                wr.ProtocolVersion = new Version(1, 1);
                if (wr.ProtocolVersion.Major != 1 && wr.ProtocolVersion.Minor != 1)
                {
                    Log.Exception("Expected version 1.0, but got " + wr.ProtocolVersion.ToString());
                    result = MFTestResults.Fail;
                }

                Log.Comment("Set Invalid Version 3.7");
                try
                {
                    wr.ProtocolVersion = new Version(3, 7);
                    Log.Exception("Expected Argument Exception");
                    result = MFTestResults.Fail;
                }
                catch (Exception ex)
                {
                    if (!HttpTests.ValidateException(ex, typeof(ArgumentException)))
                        result = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults SetHeadersAfterRequest()
        {
            MFTestResults result = MFTestResults.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://" + Utilities.GetLocalIpAddress() + ":" + HttpTestServer.s_CurrentPort.ToString() + "/");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            HttpTestServer server = new HttpTestServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>SetHeadersAfterRequest</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();
                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();

                // Tests
                try
                {
                    Log.Comment("ReadWriteTimeout");
                    wr.ReadWriteTimeout = 10;
                    Log.Exception("[Client] Expected InvalidOperationException");
                    result = MFTestResults.Fail;
                }
                catch (Exception ex)
                {
                    if (!HttpTests.ValidateException(ex, typeof(InvalidOperationException)))
                        result = MFTestResults.Fail;
                }

                try
                {
                    Log.Comment("ContentLength");
                    wr.ContentLength = 10;
                    Log.Exception("[Client] Expected InvalidOperationException");
                    result = MFTestResults.Fail;
                }
                catch (Exception ex)
                {
                    if (!HttpTests.ValidateException(ex, typeof(InvalidOperationException)))
                        result = MFTestResults.Fail;
                }

                try
                {
                    Log.Comment("Headers");
                    wr.Headers = new WebHeaderCollection();
                    Log.Exception("[Client] Expected InvalidOperationException");
                    result = MFTestResults.Fail;
                }
                catch (Exception ex)
                {
                    if (!HttpTests.ValidateException(ex, typeof(InvalidOperationException)))
                        result = MFTestResults.Fail;
                }

                try
                {
                    Log.Comment("Proxy");
                    wr.Proxy = null;
                    Log.Exception("[Client] Expected InvalidOperationException");
                    result = MFTestResults.Fail;
                }
                catch (Exception ex)
                {
                    if (!HttpTests.ValidateException(ex, typeof(InvalidOperationException)))
                        result = MFTestResults.Fail;
                }

                try
                {
                    Log.Comment("SendChunked");
                    wr.SendChunked = true;
                    Log.Exception("[Client] Expected InvalidOperationException");
                    result = MFTestResults.Fail;
                }
                catch (Exception ex)
                {
                    if (!HttpTests.ValidateException(ex, typeof(InvalidOperationException)))
                        result = MFTestResults.Fail;
                }
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
        public MFTestResults FunctionalChunkTests()
        {
            MFTestResults result = MFTestResults.Pass;
            string currentPort = HttpTestServer.s_CurrentPort.ToString();
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + currentPort + "/");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.KeepAlive = true;
            HttpTestServer server = new HttpTestServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = MFUtilities.GetRandomSafeString(5000),
            };

            try
            {
                // Setup server
                server.StartServer();

                Log.Comment("Send UnChunked");
                wr.SendChunked = false;
                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                HttpTests.PrintHeaders("Client", response.Headers);
                using (Stream responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        string page = HttpTests.ReadStream("Client", responseStream);
                        if (page != server.ResponseString)
                        {
                            result = MFTestResults.Fail;
                            Log.Exception("[Client] Send UnChunked - Corrupt Page!");
                            Log.Exception("[Client] Expected: " + server.ResponseString);
                            Log.Exception("[Client] Received: " + page);
                        }
                    }
                    else
                    {
                        result = MFTestResults.Fail;
                        Log.Exception("[Client] Expected stream, but got null");
                    }
                }

                Log.Comment("Send Chunked");
                wr = (HttpWebRequest)WebRequest.Create("http://" + Utilities.GetLocalIpAddress() + ":" + currentPort + "/");
                wr.UserAgent = ".Net Micro Framwork Device/4.0";
                wr.SendChunked = true;
                server.SendChunked = true;
                server.RequestHeaders = wr.Headers;
                response = (HttpWebResponse)wr.GetResponse();
                HttpTests.PrintHeaders("Client", response.Headers);
                using (Stream responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        string page = HttpTests.ReadStream("Client", responseStream);
                        if (page != server.ResponseString)
                        {
                            result = MFTestResults.Fail;
                            Log.Exception("[Client] Send Chunked - Corrupt Page!");
                            Log.Exception("[Client] Expected: " + server.ResponseString);
                            Log.Exception("[Client] Received: " + page);
                        }
                    }
                    else
                    {
                        result = MFTestResults.Fail;
                        Log.Exception("[Client] Expected stream, but got null");
                    }
                }
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
        public MFTestResults Skeleton()
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        #endregion Test Cases
    }
}
