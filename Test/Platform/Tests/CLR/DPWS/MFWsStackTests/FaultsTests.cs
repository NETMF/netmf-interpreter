/*---------------------------------------------------------------------
* FaultsTests.cs - file description
* Version: 1.0
* Author: REDMOND\a-grchat
* Created: 11/14/2007 10:27:20 AM 
* ---------------------------------------------------------------------*/

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using Ws.Services.Faults;

namespace Microsoft.SPOT.Platform.Tests
{
    public class FaultsTests : IMFTestInterface
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
        }

        [TestMethod]
        public MFTestResults FaultsTest_WsFault()
        {
            /// <summary>
            /// 1. Raises each type of FaultType
            /// 2. Verifies that the Byte[] returns and is of reasonable size
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                Ws.Services.WsaAddressing.WsWsaHeader testWWH = new Ws.Services.WsaAddressing.WsWsaHeader();
                WsFaultException testWF = new WsFaultException(testWWH, WsFaultType.ArgumentException);

                if( testWF.FaultType != WsFaultType.ArgumentException )
                    throw new Exception("Incorrect FaultType set");

                if( testWF.Header != testWWH )
                    throw new Exception("Incorrect Header Set");
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
