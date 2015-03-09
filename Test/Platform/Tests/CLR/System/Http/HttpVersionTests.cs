////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Net;

namespace Microsoft.SPOT.Platform.Tests
{
    public class HttpVersionTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            try
            {
                // Check networking - we need to make sure we can reach our proxy server
                Dns.GetHostEntry(HttpTests.Proxy);
            }
            catch (Exception ex)
            {
                Log.Exception("Unable to get address for " + HttpTests.Proxy, ex);
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
        private MFTestResults Verify(HttpWebRequest Request, Version ExpectedVersion)
        {
            MFTestResults result = MFTestResults.Pass;

            // Assign Proxy
            Request.Proxy = new WebProxy(HttpTests.Proxy, true);

            HttpWebResponse myResponse = (HttpWebResponse)Request.GetResponse();
            Log.Comment("Version after assignment:" + Request.ProtocolVersion);
            Log.Comment("Response version:" + myResponse.ProtocolVersion);

            if (Request.ProtocolVersion.Major != ExpectedVersion.Major && Request.ProtocolVersion.Minor != ExpectedVersion.Major)
            {
                Log.Exception("Expected Request Version " + ExpectedVersion);
                result = MFTestResults.Fail;
            }

            if (myResponse.ProtocolVersion != HttpVersion.Version11)
            {
                Log.Exception("Expected Response Version 1.1");
                result = MFTestResults.Fail;
            }

            return result;
        }

        #endregion Helper methods

        #region Test Cases

        [TestMethod]
        public MFTestResults VerifyDefault()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create("http://www.microsoft.com");
                if (myRequest.ProtocolVersion != HttpVersion.Version11)
                {
                    Log.Exception("Expected Version 1.1, but got " + myRequest.ProtocolVersion);
                    return MFTestResults.Fail;
                }
                result = Verify(myRequest, HttpVersion.Version11);
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults VerifyChange10()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create("http://www.microsoft.com");
                myRequest.ProtocolVersion = HttpVersion.Version10;
                result = Verify(myRequest, HttpVersion.Version10);
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults VerifyChange11()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create("http://www.microsoft.com");
                myRequest.ProtocolVersion = HttpVersion.Version11;
                result = Verify(myRequest, HttpVersion.Version11);
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        #endregion Test Cases
    }
}
