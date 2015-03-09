/*---------------------------------------------------------------------
* FaultsTests.cs - file description
* Version: 1.0
* Author: REDMOND\a-grchat
* Created: 11/14/2007 10:27:20 AM 
* ---------------------------------------------------------------------*/

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using Ws.Services.Transport;
using Ws.Services.Transport.HTTP;
using Ws.Services.Transport.UDP;
using Ws.Services;

namespace Microsoft.SPOT.Platform.Tests
{
    public class TransportTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }


        [TestMethod]
        public MFTestResults TransportTest_HTTP_WsHttpClient()
        {
            /// <summary>
            /// 1. Gets and verifies each of the properties of a WsHttpClient object
            /// 2. Sets and re-verifies all properties
            /// </summary>
            ///

            bool testResult = true;
            try
            {
                WsHttpClient testWHC = new WsHttpClient(new ProtocolVersion10());

                Log.Comment("SendTimeout");
                if (testWHC.SendTimeout.GetType() !=
                    Type.GetType("System.Int32"))
                    throw new Exception("SendTimeout wrong type");

                testWHC.SendTimeout = 100;

                if (testWHC.SendTimeout.GetType() !=
                    Type.GetType("System.Int32"))
                    throw new Exception("SendTimeout wrong type");

                if (testWHC.SendTimeout != 100)
                    throw new Exception("SendTimeout wrong data");

                Log.Comment("ReceiveTimeout");
                if (testWHC.ReceiveTimeout.GetType() !=
                    Type.GetType("System.Int32"))
                    throw new Exception("ReceiveTimeout wrong type");

                testWHC.ReceiveTimeout = 500;

                if (testWHC.ReceiveTimeout.GetType() !=
                    Type.GetType("System.Int32"))
                    throw new Exception("ReceiveTimeout wrong type");

                if (testWHC.ReceiveTimeout != 500)
                    throw new Exception("ReceiveTimeout wrong data");

            }
            catch (Exception e)
            {
                testResult = false;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
    }
}
