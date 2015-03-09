//#define CLIENT_TESTS

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace Microsoft.SPOT.Platform.Test
{
    class SslTests
    {
        public IPAddress ipAddress = null;

        public SslTests(IPAddress ip)
        {
            ipAddress = ip;
        }

        /// <summary>
        /// This is a test to validate the TestManagerServer implementation.
        /// </summary>
        /// <returns></returns>
        //public MFTestResults TestManager_Test(int scenarioToRun)
        //{
        //    //test to verify the server functionality and logic of this test harness.
        //    //Just sleep a couple of seconds to simulate activity.
        //    //Thread.Sleep(2000);
        //    Console.WriteLine("Executing Scenario:" + scenarioToRun);
        //    return MFTestResults.Pass;
        //}

#if CLIENT_TESTS
        /// <summary>
        /// This is a test to validate all combinations of connections to be made.
        /// </summary>
        /// <returns></returns>
        public MFTestResults VerifyServerConnectionCombinations(int scenarioToRun)
        {
            SslTestTable sslTestTable = new SslTestTable(ipAddress);

            if (sslTestTable.sslServer.Length <= scenarioToRun)
            {
                Console.WriteLine("Trying to execute a test that isn't in the SslTestTable.  Make sure to run a valid scenario.");
                return MFTestResults.Fail;
            }

            SslServer sslServer = sslTestTable.sslServer[scenarioToRun];
            Console.WriteLine("Executing Scenario:" + scenarioToRun);
            MFTestResults testResult = MFTestResults.Pass;

            try
            {
                testResult = sslServer.RunServer();
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(
                    "An error has occurred while using or authenticating " +
                    "this connection.  Check to make sure you've exported " +
                    "your certificate and that it is correctly referenced " +
                    "in CreateFromCertFile above.");
                testResult = MFTestResults.Fail;
            }
            catch (AuthenticationException ex)
            {
                Console.WriteLine(ex.Message);
                testResult = MFTestResults.Fail;
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
                testResult = MFTestResults.Fail;
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                testResult = MFTestResults.Fail;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MFTestResults HeapTest(int scenarioToRun)
        {
            MFTestResults testResult = MFTestResults.Fail;
            SslServer sslServer = null;

            try
            {
                sslServer = new SslServer(new X509Certificate2(Resource1.desktop, "alden"), false, SslProtocols.Default, false);
                testResult = sslServer.RunServer();
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(
                    "An error has occurred while using or authenticating " +
                    "this connection.  Check to make sure you've exported " +
                    "your certificate and that it is correctly referenced " +
                    "in CreateFromCertFile above.");
                testResult = MFTestResults.Fail;
            }
            catch (AuthenticationException ex)
            {
                Console.WriteLine(ex.Message);
                testResult = MFTestResults.Fail;
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
                testResult = MFTestResults.Fail;
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                testResult = MFTestResults.Fail;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }
#endif
        /// <summary>
        /// This is a test to validate all combinations of connections to be made.
        /// </summary>
        /// <returns></returns>
        public MFTestResults VerifyClientConnectionCombinations(int scenarioToRun)
        {
            SslTestTable sslTestTable = new SslTestTable(ipAddress);

            if (sslTestTable.sslClient.Length <= scenarioToRun)
            {
                Console.WriteLine("Trying to execute a test that isn't in the SslTestTable.  Make sure to run a valid scenario.");
                return MFTestResults.Fail;
            }

            SslClient sslClient = sslTestTable.sslClient[scenarioToRun];
            Console.WriteLine("Executing Scenario:" + scenarioToRun);
            MFTestResults testResult = MFTestResults.Pass;

            try
            {
                testResult = sslClient.RunClient();
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(
                    "An error has occurred while using or authenticating " +
                    "this connection.  Check to make sure you've exported " +
                    "your certificate and that it is correctly referenced " +
                    "in CreateFromCertFile above.");
                testResult = MFTestResults.Fail;
            }
            catch (AuthenticationException ex)
            {
                Console.WriteLine(ex.Message);
                testResult = MFTestResults.Fail;
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
                testResult = MFTestResults.Fail;
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                testResult = MFTestResults.Fail;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }
    }
}
