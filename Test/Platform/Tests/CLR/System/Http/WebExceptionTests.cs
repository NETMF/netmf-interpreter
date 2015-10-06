////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Net;

namespace Microsoft.SPOT.Platform.Tests
{
    public class WebExceptionTests : IMFTestInterface
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

        private MFTestResults VerifyStream(HttpWebResponse response, HttpServer server)
        {
            MFTestResults result = MFTestResults.Pass;

            using (System.IO.Stream responseStream = response.GetResponseStream())
            {
                if (responseStream != null)
                {
                    string page = HttpTests.ReadStream("Client", responseStream);
                    if (page != server.ResponseString)
                    {
                        Log.Exception("Expect " + server.ResponseString + " but get " + responseStream.ToString());
                        result = MFTestResults.Fail;
                    }
                }
                else
                {
                    result = MFTestResults.Fail;
                    Log.Exception("[Client] Expected stream, but got null");
                }
            }

            return result;
        }

        #endregion Helper methods


        #region Test
        [TestMethod]
        public MFTestResults TestWebExceptionHTTP1_1_ConnectionClosed()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webexception/ConnClose.html");  //expect ConnectionClosed
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect ConnectionClosed");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public MFTestResults TestWebExceptionHTTP1_1_KeepAliveFailure()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webexception/KeepAliveFailure.html");  //expect KeepAliveFailure
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect KeepAliveFailure");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public MFTestResults TestWebExceptionHTTP1_1_Pending()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webexception/Pending.html");  //expect Pending
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect Pending");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }

        [TestMethod]
        public MFTestResults TestWebExceptionHTTP1_1_PipelineFailure()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webexception/PipelineFailure.html");  //expect PipelineFailure
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect PipelineFailure");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }

        [TestMethod]
        public MFTestResults TestWebExceptionHTTP1_1_ProxyNameResolutionFailure()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webexception/ProxyNameResolutionFailure.html");  //expect ProxyNameResolutionFailure
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect ProxyNameResolutionFailure");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public MFTestResults TestWebExceptionHTTP1_1_ReceiveFailure()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webexception/ReceiveFailure.html");  //expect ReceiveFailure
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect ReceiveFailure");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public MFTestResults TestWebExceptionHTTP1_1_RequestCanceled()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webexception/RequestCanceled.html");  //expect RequestCanceled
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect RequestCanceled");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public MFTestResults TestWebExceptionHTTP1_1_SecureChannelFailure()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webexception/SecureChannelFailure.html");  //expect SecureChannelFailure
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect SecureChannelFailure");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public MFTestResults TestWebExceptionHTTP1_1_SendFailure()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webexception/SendFailure.html");  //expect SendFailure
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect SendFailure");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public MFTestResults TestWebExceptionHTTP1_1_Success()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webexception/Success.html");  //expect Success
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect Success");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public MFTestResults TestWebExceptionHTTP1_1_Timeout()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webexception/Timeout.html");  //expect Timeout
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect Timeout");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public MFTestResults TestWebExceptionHTTP1_1_TrustFailure()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webexception/TrustFailure.html");  //expect TrustFailure
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect TrustFailure");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public MFTestResults TestWebExceptionHTTP1_1_ConnectFailure()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webexception/ConnectFailure.html");  //expect ConnectFailure
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect ConnectFailure");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public MFTestResults TestWebExceptionHTTP1_1_NameResolutionFailure()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webexception/NameResolutionFailure.html");  //expect NameResolutionFailure
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect NameResolutionFailure");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public MFTestResults TestWebExceptionHTTP1_1_ProtocolError()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webexception/ProtocolError.html");  //expect ProtocolError
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect ProtocolError");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public MFTestResults TestWebExceptionHTTP1_1_ServerProtocolViolation()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webexception/ServerProtocolViolation.html");  //expect ServerProtocolViolation
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect ServerProtocolViolation");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }

        #endregion Test
    }
}
