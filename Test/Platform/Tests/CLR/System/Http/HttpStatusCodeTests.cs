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
    public class HttpStatusCodeTests : IMFTestInterface
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

        #region Test
        [TestMethod]
        public MFTestResults ValidDefaultTestHTTPStatusCodeHTTP1_1()
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
                ResponseString = "<html><body>ValidDefaultTestHTTPStatusCodeHTTP1_1</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();

                Log.Comment("Expect StatusCode - OK");
                if (HttpStatusCode.OK != response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = OK but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }

                Log.Comment("Receive " + response.StatusDescription);

                response.Close();
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
        public MFTestResults DefaultTestHTTPStatusCodeHTTP1_1_Unauthorized()
        {
            MFTestResults result = MFTestResults.Pass;

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/Unauth.html");  //expect 401 - Unauthorized 
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>DefaultTestHTTPStatusCodeHTTP1_1_Unauthorized</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();

                Log.Comment("Expect StatusCode - Unauthorized ");
                if (HttpStatusCode.Unauthorized != response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = Unauthorized but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }

                response.Close();
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
        public MFTestResults DefaultTestHTTPStatusCodeHTTP1_1_NotFound()
        {
            MFTestResults result = MFTestResults.Pass;

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/NotFound.html");  //expect 404 - NotFound  
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>DefaultTestHTTPStatusCodeHTTP1_1_NotFound</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();

                Log.Comment("Expect StatusCode - NotFound ");
                if (HttpStatusCode.NotFound != response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = NotFound but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }

                Log.Comment("Receive " + response.StatusDescription);

                response.Close();
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


        //Bug 68085...
        [TestMethod]
        public MFTestResults DefaultTestHTTPStatusCodeHTTP1_1_NotExistUriPath()
        {
            MFTestResults result = MFTestResults.Pass;

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/NotExistUriPath.html");      
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>DefaultTestHTTPStatusCodeHTTP1_1_NotExistUriPath</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();

                Log.Comment("Expect StatusCode - NotFound  ");
                if (HttpStatusCode.NotFound != response.StatusCode)
                {
                    if ("0" == response.StatusCode.ToString())
                        Log.Exception("Known Issue::Bug 68085 - HttpStatusCode = 0 was returned");
                    else
                        Log.Exception("Expect HttpStatusCode = NotFound but get " + response.StatusCode);
                    result = MFTestResults.Fail;
                }

                response.Close();
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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_SetChunked()
        {
            MFTestResults result = MFTestResults.Pass;

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/");  //expect 200 - OK
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);
            wr.SendChunked = true;

            try
            {
                wr.Headers.Set(HttpKnownHeaderNames.TransferEncoding, "chunked");

                result = MFTestResults.Fail;
            }
            catch (ArgumentException ex)
            {
                Log.Exception("Expected ArgumentException:  ", ex);
            }


            return result;
        }


        [TestMethod]
        public MFTestResults TestHTTPStatusCodeHTTP1_1_SendChunked_WithTransferEncoding()
        {
            MFTestResults result = MFTestResults.Pass;

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);
            wr.SendChunked = true;
            wr.TransferEncoding = "Quoted-printable";

            string requestString = "ContentLength: " + wr.ContentLength
            + " ContentType:" + wr.ContentType + " KeepAlive:" + wr.KeepAlive
            + " ProtocolVersion:" + wr.ProtocolVersion + " TransferEncoding:" + wr.TransferEncoding
            + " SendChunked:" + wr.SendChunked + " Expect:" + wr.Expect
            + " Timeout:" + wr.Timeout;

            Log.Comment(requestString);
            
            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_SendChunked_WithTransferEncoding</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();

                string ResponseString = "Server response: ContentEncoding:" + response.ContentEncoding + " ContentLength: " + response.ContentLength
                        + " ContentType:" + response.ContentType
                        + " ProtocolVersion:" + response.ProtocolVersion
                        + " StatusCode:" + response.StatusCode + " StatusDescription:" + response.StatusDescription;

                Log.Comment(ResponseString);

                Log.Comment("Expect StatusCode - OK");
                if (HttpStatusCode.OK != response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = OK but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }

                Log.Comment("Receive " + response.StatusDescription);

                response.Close();
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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_Ambiguous()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/index.html");  //expect 300 - Ambiguous
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_Ambiguous</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - Ambiguous");
                if ((int)HttpStatusCode.Ambiguous != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = Ambiguous but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_BadGateway()
        {
            MFTestResults result = MFTestResults.Pass;

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/Gateway.html");  //expect 502 - BadGateway
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_BadGateway</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - BadGateway");
                if ((int)HttpStatusCode.BadGateway != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = BadGateway but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_Conflict()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/conflict.html");  //expect 409 - Conflict
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_Conflict</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - Conflict");
                if ((int)HttpStatusCode.Conflict != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = Conflict but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_Moved()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/moved.html");  //expect 301 - Moved
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_Moved</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - Moved");
                if ((int)HttpStatusCode.Moved != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = Moved but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_Redirect()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/redirect.html");  //expect 302 - Redirect
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_Redirect</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - Redirect");
                if ((int)HttpStatusCode.Redirect != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = Redirect but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_PaymentRequired()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/payment.html");  //expect 402 - PaymentRequired
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_PaymentRequired</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - PaymentRequired");
                if ((int)HttpStatusCode.PaymentRequired != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = PaymentRequired but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_RequestTimeout()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/testTimeOut.html");  //expect 408 - RequestTimeout
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_RequestTimeout</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - RequestTimeout");
                if ((int)HttpStatusCode.RequestTimeout != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = RequestTimeout but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_RequestUriTooLong()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/local/webpages/mysubdir/index.html");  //expect 414 - RequestUriTooLong
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_RequestUriTooLong</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - RequestUriTooLong");
                if ((int)HttpStatusCode.RequestUriTooLong != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = RequestUriTooLong but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_ServiceUnavailable()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/service.html");  //expect 503 - ServiceUnavailable
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_ServiceUnavailable</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - ServiceUnavailable");
                if ((int)HttpStatusCode.ServiceUnavailable != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = ServiceUnavailable but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_TemporaryRedirect()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/tempRedir.html");  //expect 307 - TemporaryRedirect
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_TemporaryRedirect</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - TemporaryRedirect");
                if ((int)HttpStatusCode.TemporaryRedirect != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = TemporaryRedirect but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_Accepted()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/accepted.html");  //expect 202 - Accepted
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_Accepted</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - Accepted");
                if ((int)HttpStatusCode.Accepted != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = Accepted but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_BadRequest()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/badRequest.html");  //expect 400 - BadRequest
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_BadRequest</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - BadRequest");
                if ((int)HttpStatusCode.BadRequest != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = BadRequest but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_Continue()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/continue.html");  //expect 100 - Continue
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_Continue</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                wr.ContinueDelegate = delegate(int StatusCode, WebHeaderCollection httpHeaders){ };

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - Continue");
                if ((int)HttpStatusCode.Continue != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = Continue but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_Created()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/created.html");  //expect 201 - Created
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_Created</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - Created");
                if ((int)HttpStatusCode.Created != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = Created but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_ExpectationFailed()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/expfailed.html");  //expect 417 - ExpectationFailed
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_ExpectationFailed</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - ExpectationFailed");
                if ((int)HttpStatusCode.ExpectationFailed != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = ExpectationFailed but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_Forbidden()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/forbidden.html");  //expect 403 - Forbidden
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_Forbidden</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - Forbidden");
                if ((int)HttpStatusCode.Forbidden != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = Forbidden but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_Found()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/found.html");  //expect 302 - Found
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_Found</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - Found");
                if ((int)HttpStatusCode.Found != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = Found but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_GatewayTimeout()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/gateway.html");  //expect  - GatewayTimeout
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_GatewayTimeout</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - GatewayTimeout");
                if ((int)HttpStatusCode.GatewayTimeout != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = GatewayTimeout but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_Gone()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/gone.html");  //expect 410 - Gone
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_Gone</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - Gone");
                if ((int)HttpStatusCode.Gone != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = Gone but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_HttpVersionNotSupported()
        {
            MFTestResults result = MFTestResults.Pass;

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/httpversion.html");  //expect 505 - HttpVersionNotSupported
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_HttpVersionNotSupported</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - HttpVersionNotSupported");
                if ((int)HttpStatusCode.HttpVersionNotSupported != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = HttpVersionNotSupported but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_InternalServerError()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/iserror.html");  //expect 500 - InternalServerError
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_InternalServerError</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - InternalServerError");
                if ((int)HttpStatusCode.InternalServerError != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = InternalServerError but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_LengthRequired()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/lrequired.html");  //expect 411 - LengthRequired
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_LengthRequired</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - LengthRequired");
                if ((int)HttpStatusCode.LengthRequired != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = LengthRequired but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_MethodNotAllowed()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/method.html");  //expect 405 - MethodNotAllowed
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_MethodNotAllowed</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - MethodNotAllowed");
                if ((int)HttpStatusCode.MethodNotAllowed != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = MethodNotAllowed but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_NoContent()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/NoContent.html");  //expect 204 - NoContent
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_NoContent</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - NoContent");
                if ((int)HttpStatusCode.NoContent != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = NoContent but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_NonAuthoritativeInformation()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/nonauthInfo.html");  //expect 203 - NonAuthoritativeInformation
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_NonAuthoritativeInformation</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - NonAuthoritativeInformation");
                if ((int)HttpStatusCode.NonAuthoritativeInformation != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = NonAuthoritativeInformation but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_NotAcceptable()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/NotAccept.html");  //expect  - NotAcceptable
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_NotAcceptable</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - NotAcceptable");
                if ((int)HttpStatusCode.NotAcceptable != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = NotAcceptable but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_NotImplemented()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/NotImplement.html");  //expect 501 - NotImplemented
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_NotImplemented</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - NotImplemented");
                if ((int)HttpStatusCode.NotImplemented != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = NotImplemented but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_NotModified()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/NotModified.html");  //expect  - NotModified
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_NotModified</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - NotModified");
                if ((int)HttpStatusCode.NotModified != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = NotModified but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_PartialContent()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/PContent.html");  //expect 206 - PartialContent
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_PartialContent</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - PartialContent");
                if ((int)HttpStatusCode.PartialContent != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = PartialContent but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_PreconditionFailed()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/Precond.html");  //expect 412 - PreconditionFailed
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_PreconditionFailed</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - PreconditionFailed");
                if ((int)HttpStatusCode.PreconditionFailed != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = PreconditionFailed but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_ProxyAuthenticationRequired()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/ProxyAuth.html");  //expect  - ProxyAuthenticationRequired
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_ProxyAuthenticationRequired</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - ProxyAuthenticationRequired");
                if ((int)HttpStatusCode.ProxyAuthenticationRequired != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = ProxyAuthenticationRequired but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_RedirectMethod()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/RMethod.html");  //expect  - RedirectMethod
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_RedirectMethod</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - RedirectMethod");
                if ((int)HttpStatusCode.RedirectMethod != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = RedirectMethod but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_RequestedRangeNotSatisfiable()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/reqRange.html");  //expect 416 - RequestedRangeNotSatisfiable
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_RequestedRangeNotSatisfiable</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - RequestedRangeNotSatisfiable");
                if ((int)HttpStatusCode.RequestedRangeNotSatisfiable != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = RequestedRangeNotSatisfiable but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_RequestEntityTooLarge()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/reqEntity.html");  //expect 413 - RequestEntityTooLarge
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_RequestEntityTooLarge</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - RequestEntityTooLarge");
                if ((int)HttpStatusCode.RequestEntityTooLarge != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = RequestEntityTooLarge but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_ResetContent()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/ResetContent.html");  //expect  - ResetContent
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_ResetContent</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - ResetContent");
                if ((int)HttpStatusCode.ResetContent != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = ResetContent but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_SeeOther()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/SeeOther.html");  //expect  - SeeOther
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_SeeOther</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - SeeOther");
                if ((int)HttpStatusCode.SeeOther != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = SeeOther but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_SwitchingProtocols()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/SwitchProt.html");  //expect  - SwitchingProtocols
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_SwitchingProtocols</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - SwitchingProtocols");
                if ((int)HttpStatusCode.SwitchingProtocols != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = SwitchingProtocols but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_Unauthorized()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/Unauth.html");  //expect  - Unauthorized
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_Unauthorized</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - Unauthorized");
                if ((int)HttpStatusCode.Unauthorized != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = Unauthorized but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_Unused()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/Unused.html");  //expect  - Unused
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_Unused</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - Unused");
                if ((int)HttpStatusCode.Unused != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = Unused but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_1_UseProxy()
        {
            MFTestResults result = MFTestResults.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/webpages/UseProxy.html");  //expect  - UseProxy
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "GET";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_1_UseProxy</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect StatusCode - UseProxy");
                if ((int)HttpStatusCode.UseProxy != (int)response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = UseProxy but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }
                Log.Comment("Receive " + response.StatusDescription);
                response.Close();

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
        public MFTestResults TestHTTPStatusCodeHTTP1_0_SendChunked_WithTransferEncoding()
        {
            MFTestResults result = MFTestResults.Pass;

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/");  //expect 200 - OK
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.0");
            wr.ProtocolVersion = new Version(1, 0);
            wr.SendChunked = true;
            wr.TransferEncoding = "binary";

            string requestString = "ContentLength: " + wr.ContentLength
            + " ContentType:" + wr.ContentType + " KeepAlive:" + wr.KeepAlive
            + " ProtocolVersion:" + wr.ProtocolVersion + " TransferEncoding:" + wr.TransferEncoding
            + " SendChunked:" + wr.SendChunked + " Expect:" + wr.Expect
            + " Timeout:" + wr.Timeout;

            Log.Comment(requestString);


            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = "<html><body>TestHTTPStatusCodeHTTP1_0_SendChunked_WithTransferEncoding</body></html>"
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();

                string ResponseString = "Server response: ContentEncoding:" + response.ContentEncoding + " ContentLength: " + response.ContentLength
                        + " ContentType:" + response.ContentType
                        + " ProtocolVersion:" + response.ProtocolVersion + " Server:" + response.Server
                        + " StatusCode:" + response.StatusCode + " StatusDescription:" + response.StatusDescription;

                Log.Comment(ResponseString);

                Log.Comment("Expect StatusCode - OK");
                if (HttpStatusCode.OK != response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = OK but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }

                Log.Comment("Receive " + response.StatusDescription);

                response.Close();
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

        #endregion Test
    }
}
