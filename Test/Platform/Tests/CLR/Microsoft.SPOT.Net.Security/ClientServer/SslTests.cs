//#define RUN_TESTS
#define CLIENT_TESTS

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Net;
using Microsoft.SPOT.Net.Security;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Platform.Tests;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Cryptography.X509Certificates;


namespace Microsoft.SPOT.Platform.Tests
{
    class SslTests : IMFTestInterface
    {
        TestManager tmc = null;

        [SetUp]
        public InitializeResult Initialize()
        {
            // Start the Test Manager Client that dispatches tests to execute on the server.
            try
            {
                if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                {
                    Log.Comment("Do Not run the tests on the emulator since it doesn't have an SSL Stack.");
                    return InitializeResult.Skip;
                }

#if RUN_TESTS
                tmc = new TestManager();
                tmc.Start();
#endif
            }
            catch (Exception e)
            {
                Log.Comment("Unable to start the client connection to the Server.  Make sure the Server is running and actively accepting a connection.");
                Log.Comment("Error Code: " + e.ToString());
                Log.Comment("Fail the whole test suite since it can not connect to the server.");
                return InitializeResult.Skip;
            }

#if RUN_TESTS
            return InitializeResult.ReadyToGo;
#else
            return InitializeResult.Skip;
#endif
        }

        [TearDown]
        public void CleanUp()
        {
            tmc.CloseClient(); 
        }

        //[TestMethod]
        //public MFTestResults TestManager_Test()
        //{
        //    //This call is what launches the remote test case on the desktop.
        //    MFTestResults testResult = tmc.RunTest("TestManager_Test", 12);
        //    if (testResult == MFTestResults.Pass)
        //    {
        //        //This call gets the result from the remote test case.
        //        if (tmc.GetTestResult() == MFTestResults.Fail)
        //            testResult = MFTestResults.Fail;
        //    }
        //    return testResult;
        //}

        //[TestMethod]
        //public MFTestResults TestManager_Test2()
        //{
        //    //This call is what launches the remote test case on the desktop.
        //    MFTestResults testResult = tmc.RunTest("TestManager_Test");
        //    if (testResult == MFTestResults.Pass)
        //    {
        //        //This call gets the result from the remote test case.
        //        if (tmc.GetTestResult() == MFTestResults.Fail)
        //            testResult = MFTestResults.Fail;
        //    }
        //    return testResult;
        //}
#if CLIENT_TESTS
        /**/
        [TestMethod]
        public MFTestResults VerifyClientConnectionCombinations()
        {
            //Create the SslTable
            SslTestTable sslTestTable = new SslTestTable(tmc.desktopIPAddress);
            MFTestResults overAllTestResult = MFTestResults.Pass;

            for (int i = 0; i < sslTestTable.sslClient.Length; i++)
            {
                MFTestResults testResult = MFTestResults.Pass;

                //This call is what launches the remote test case on the desktop.
                Log.Comment("Running Test Scenario number: " + i);

                if (tmc.RunTest("VerifyServerConnectionCombinations", i) == MFTestResults.Fail)
                    return MFTestResults.Fail;

                SslClient sslClient = sslTestTable.sslClient[i];

                if (sslClient.RunClient() == MFTestResults.Fail)
                    testResult = MFTestResults.Fail;

                //This call gets the result from the remote test case. Also fails if the test previously failed somewhere prior to this.
                if (tmc.GetTestResult() == MFTestResults.Fail)
                    testResult = MFTestResults.Fail;


                if (testResult == MFTestResults.Pass)
                    Log.Comment("VerifyConnectionCombinations iteration number: " + i + " Passed.");
                else
                {
                    Log.Comment("VerifyConnectionCombinations iteration number: " + i + " Failed.");
                    overAllTestResult = MFTestResults.Fail;
                }
            }

            //if we get here we've passed all the tests successfully.
            return overAllTestResult;
        }

        [TestMethod]
        public MFTestResults HeapTest()
        {
            MFTestResults testResult = tmc.RunTest("HeapTest");
            if (testResult == MFTestResults.Pass)
            {
                SslClient sslClient = new SslClient(tmc.desktopIPAddress, "ebsnetinc", new X509Certificate(CertificatesAndCAs.device), new X509Certificate[] { new X509Certificate(CertificatesAndCAs.desktop) }, SslVerification.NoVerification, new SslProtocols[] { SslProtocols.SSLv3 }, false);

                //Make the string large to cause a potential heap overflow.
                sslClient.messageSent = MFUtilities.GetRandomSafeString(1700) + SslClient.TERMINATOR;

                Log.Comment("Sending string that is length: " + sslClient.messageSent.Length);

                if (sslClient.RunClient() == MFTestResults.Fail)
                    testResult = MFTestResults.Fail;

                if (tmc.GetTestResult() == MFTestResults.Fail)
                    testResult = MFTestResults.Fail;
            }
            return testResult;
        }
        /**/
#endif
        [TestMethod]
        public MFTestResults VerifyServerConnectionCombinations()
        {
            //Create the SslTable
            SslTestTable sslTestTable = new SslTestTable(tmc.desktopIPAddress);
            MFTestResults overAllTestResult = MFTestResults.Pass;

            for (int i = 0; i < sslTestTable.sslServer.Length; i++)
            {
                MFTestResults testResult = MFTestResults.Pass;

                //This call is what launches the remote test case on the desktop.
                Log.Comment("Running Test Scenario number: " + i);

                if (tmc.RunTest("VerifyClientConnectionCombinations", i) == MFTestResults.Fail)
                    return MFTestResults.Fail;

                SslServer sslServer = sslTestTable.sslServer[i];

                if (sslServer.RunServer() == MFTestResults.Fail)
                    testResult = MFTestResults.Fail;

                //This call gets the result from the remote test case. Also fails if the test previously failed somewhere prior to this.
                if (tmc.GetTestResult() == MFTestResults.Fail)
                    testResult = MFTestResults.Fail;


                if (testResult == MFTestResults.Pass)
                    Log.Comment("VerifyConnectionCombinations iteration number: " + i + " Passed.");
                else
                {
                    Log.Comment("VerifyConnectionCombinations iteration number: " + i + " Failed.");
                    overAllTestResult = MFTestResults.Fail;
                }
            }

            //if we get here we've passed all the tests successfully.
            return overAllTestResult;
        }

    }
}
