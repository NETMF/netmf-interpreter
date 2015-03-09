using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Net.Security;
using Microsoft.SPOT.Platform.Test;
using System.Security.Cryptography.X509Certificates;


namespace Microsoft.SPOT.Platform.Tests
{
    class SslStreamTests : IMFTestInterface
    {
        public SslStreamTests()
        {
        }        

        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");
            
            try
            {
                X509Certificate cert = new X509Certificate(CertificatesAndCAs.caCert);
            }
            catch (NotSupportedException)
            {
                Log.Comment("If this feature throws an exception then it is assumed that it isn't supported on this device type");
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
        public MFTestResults TestMethod1()
        {
            MFTestResults testResult = MFTestResults.Fail;

            Log.Comment("Create the client and server sockets");

            SslServer sslServer = new SslServer();
            SslClient sslClient = new SslClient();

            try
            {
                // Set up the server here
                Log.Comment("Create server thread");
                sslServer.RunServer();

                // Set up the client here
                sslClient.serverEp = sslServer.serverEp;
                sslClient.RunClient();

                if( sslClient.messageSent == sslClient.messageReceived )
                    testResult = MFTestResults.Pass;
            }
            catch (Exception e)
            {
                Log.Comment("Incorrect Exception caught: " + e.ToString());
            }
            finally
            {
                try
                {
                    sslServer.Close();
                    sslClient.Close();
                }
                catch
                {
                }
            }

            Log.Comment("known issue: 20848	SSL tests don't work with loopback");
            return (testResult == MFTestResults.Fail ? MFTestResults.KnownFailure : testResult);
        }
    }
}
