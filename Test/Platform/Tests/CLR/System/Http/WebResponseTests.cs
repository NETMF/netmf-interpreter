////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Net;

namespace Microsoft.SPOT.Platform.Tests
{
    public class WebResponseTests : IMFTestInterface
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

        [TestMethod]
        public MFTestResults NotSupportExceptionTest()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("WebResponse Test");
                HttpWebRequest wrStr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/");

                HttpServer server = new HttpServer("http", ref result)
                {
                    RequestUri = wrStr.RequestUri,
                    RequestHeaders = wrStr.Headers,
                    ResponseString = "<html><body>WebResponse Test</body></html>"
                };

                server.StartServer();

                WebRequest wr = wrStr;

                WebResponse wresp = wr.GetResponse();

                Log.Comment("Check ResponseUri property");
                if (wresp.ResponseUri.AbsoluteUri != wr.RequestUri.AbsoluteUri)
                {
                    Log.Exception("Expected " + HttpTests.MSUrl + ", but got " + wresp.ResponseUri.AbsoluteUri);
                    result = MFTestResults.Fail;
                }

                Log.Comment("Check ContentType property");
                if (wresp.ContentType != "")
                {
                    Log.Exception("Expected: " + wr.ContentType);
                    result = MFTestResults.Fail;
                }

                Log.Comment("Invoke WebResponse.Close()");
                try
                {
                    wresp.Close();
                }
                catch (System.NotSupportedException e)
                {
                    Log.Exception("Get exception when invoke WebResponse.Close(). " + e.Message.ToString());
                    result = MFTestResults.Fail;
                }
                finally
                {
                    //Stop server
                    server.StopServer();
                }

            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }
    }
}
