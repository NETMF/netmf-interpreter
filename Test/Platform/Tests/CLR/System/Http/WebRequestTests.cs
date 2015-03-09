////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Net;

namespace Microsoft.SPOT.Platform.Tests
{
    public class WebRequestTests : IMFTestInterface
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

        #region Test Cases

        [TestMethod]
        public MFTestResults ValidConstructorTests()
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                Log.Comment("string constructor");
                WebRequest wrStr = WebRequest.Create(HttpTests.MSUrl);
                if (wrStr.RequestUri.AbsoluteUri != HttpTests.MSUrl + "/")
                {
                    Log.Exception("Expected " + HttpTests.MSUrl + ", but got " + wrStr.RequestUri.AbsoluteUri);
                    result = MFTestResults.Fail;
                }

                Log.Comment("uri constructor");
                Uri uri = new Uri(HttpTests.MSUrl);
                WebRequest wrUri = WebRequest.Create(uri);
                if (wrUri.RequestUri.AbsoluteUri != HttpTests.MSUrl + "/")
                {
                    Log.Exception("Expected " + HttpTests.MSUrl + ", but got " + wrUri.RequestUri.AbsoluteUri);
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
        public MFTestResults InvalidConstructorTests()
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                Log.Comment("null string");
                string nullString = null;
                try { WebRequest nsWR = WebRequest.Create(nullString); }
                catch (Exception ex)
                {
                    if (!HttpTests.ValidateException(ex, typeof(ArgumentNullException)))
                        result = MFTestResults.Fail;
                }

                Log.Comment("null uri");
                Uri nullUri = null;
                try { WebRequest nuWr = WebRequest.Create(nullUri); }
                catch (Exception ex)
                {
                    if (!HttpTests.ValidateException(ex, typeof(ArgumentNullException)))
                        result = MFTestResults.Fail;
                }

                Log.Comment("invalud URI type");
                try { WebRequest inWr = WebRequest.Create("ftp://ftp.microsoft.com"); }
                catch (Exception ex)
                {
                    if (!HttpTests.ValidateException(ex, typeof(NotSupportedException)))
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
        public MFTestResults ValidPropertiesTests()
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                Log.Comment("WebRequest Properties");
                WebRequest wrStr = WebRequest.Create(HttpTests.MSUrl);

                Log.Comment("Get ContentType property");
                if (null != wrStr.ContentType)
                {
                    Log.Exception("Expected null but got an object");
                    result = MFTestResults.Fail;
                }

                Log.Comment("Get Method property");
                if (wrStr.Method == System.String.Empty || wrStr.Method.CompareTo("GET") != 0)
                {
                    Log.Exception("Expected Method string but get empty string");
                    result = MFTestResults.Fail;
                }

                Log.Comment("Get Proxy property - Expect null");
                if (wrStr.Proxy != null)
                {
                    Log.Exception("Expected Proxy property to be null but ");
                    result = MFTestResults.Fail;
                }


                Log.Comment("Set and Get Timeout property");
                wrStr.Timeout = 90000;
                if (wrStr.Timeout != 90000)
                {
                    Log.Exception("Failed to set TimeOut property");
                    result = MFTestResults.Fail;
                }

                wrStr.Timeout = 100000;
                if (wrStr.Timeout != 100000)
                {
                    Log.Exception("Failed to set TimeOut property");
                    result = MFTestResults.Fail;
                }

                Log.Comment("Set ConnectionGroupName property - should get System.NotSupportedException");
                try
                {
                    wrStr.ConnectionGroupName = "test";
                }
                catch (System.NotSupportedException e)
                {
                    Log.Exception("Get exception when set the value of ConnectionGroupName property. " + e.Message.ToString());
                }

                
                Log.Comment("Set PreAuthenticate property");
                try
                {
                    // BUILD BREAK - PreAuthenticate is not defined for HttpWebRequest 
                    //wrStr.PreAuthenticate = true;
                } 
                catch (Exception e)
                {
                    Log.Exception("Bug #61228:  Get exception when set the value of PreAuthenticate property. " + e.Message.ToString());
                }

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
