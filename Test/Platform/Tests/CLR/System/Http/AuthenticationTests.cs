////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Net;

namespace Microsoft.SPOT.Platform.Tests
{
    public class AuthenticationTests : IMFTestInterface
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


        #region Test

        [TestMethod]
        public MFTestResults TestHTTPStatusCodeHTTP1_1_WWWAuthenticate()
        {
            MFTestResults result = MFTestResults.Pass;
            string userName = "Igor";
            string passWord = "MyPassword";

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://157.56.163.1/PasswordProtected");   //expect 401 - Unauthorized
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            wr.Credentials = new NetworkCredential(userName, passWord);
            //wr.PreAuthenticate = true; //to send WWW-authenticate HTTP header with requests  after authentication has taken place
            //wr.Headers.Set(HttpKnownHeaderNames.Authorization, "Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==");

            try
            {
                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();

                Log.Comment("Expect StatusCode - Unauthorized ");
                if (HttpStatusCode.Unauthorized != response.StatusCode)
                {
                    Log.Exception("Expect HttpStatusCode = Unauthorized but get " + response.StatusCode);
                    result = MFTestResults.Fail;

                }

                string responseString = response.Headers.ToString();
                Log.Comment("ResposeString = " + responseString);
                if (responseString.IndexOf("WWW-Authenticate: Basic realm=\".Net MF Example of Secure Area\"") > -1)
                {
                    wr.Headers.Set(HttpKnownHeaderNames.Authorization, "Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==");
                    if ((HttpStatusCode.OK != response.StatusCode))
                    {
                        Log.Exception("Expect HttpStatusCode = OK but get " + response.StatusCode);
                        result = MFTestResults.Fail;

                    }
                }

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }
            return result;
        }

        #endregion Test
    }
}
