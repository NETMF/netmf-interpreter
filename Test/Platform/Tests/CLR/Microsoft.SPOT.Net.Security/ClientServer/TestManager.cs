using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Platform.Tests;
using System.Collections;
using System.Threading;

namespace Microsoft.SPOT.Platform.Test
{
    public class TestManager
    {
        Socket listenSocket;
        Socket socket;
        TestProxy currentTestCase;

        //expose the server ipAddress so that it can be used to communicate back to the desktop.
        public IPAddress desktopIPAddress;

        /// <summary>
        /// Initializes the connection to the Client.  There is one main connection that is used to initialize all of the
        /// tests that need to be executed.
        /// </summary>
        public void Start()
        {
            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPHostEntry ipHostEntry = Dns.GetHostEntry("");
            IPAddress ipAddress = null;

            foreach (IPAddress Address in ipHostEntry.AddressList)
            {
                if (Address != null)
                {
                    ipAddress = Address;
                    Log.Comment("Device IPAddress is: " + ipAddress.ToString());
                }
            }

            int port = 15000;

            Log.Comment("Device Port number is: " + port);

            //Only wait for 30 seconds to send or receive.  Anything else is too long.
            listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 30000);
            listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 30000);
            listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new byte[] { 0, 0, 0, 0 });
            listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);

            listenSocket.Bind(new IPEndPoint(ipAddress, port));
            
            listenSocket.Listen(1);

            //block in listening mode until the desktop app connects.  Then we know we can continue.
            socket = listenSocket.Accept();

            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new byte[] { 0, 0, 0, 0 });
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);

            desktopIPAddress = ((IPEndPoint)socket.RemoteEndPoint).Address;
        }

        /// <summary>
        /// This executes the test case on the server.
        /// </summary>
        /// <param name="testToRun">The name of the test to execute.</param>
        /// <param name="serverTimeoutSeconds">The amount of time that the server should run the test case.  If it times out then something is wrong.</param>
        /// <param name="serverTestResult">The return value that the server sends back to the client.</param>
        public MFTestResults RunTest(string testToRun, int serverTimeoutSeconds, MFTestResults serverTestResult, int scenarioToRun)
        {
            MFTestResults testResult = MFTestResults.Fail;

            currentTestCase = new TestProxy();
            currentTestCase.testToRun = testToRun;
            currentTestCase.serverTimeoutSeconds = serverTimeoutSeconds;
            currentTestCase.serverTestResult = serverTestResult;
            currentTestCase.scenarioToRun = scenarioToRun;

            try
            {
                if (socket != null)
                {
                    try
                    {
                        Thread.Sleep(350);

                        //send the test that the client wants to execute to the server.
                        socket.Send(currentTestCase.Serialize());

                        testResult = MFTestResults.Pass;
                    }
                    catch (Exception e)
                    {
                        Debug.Print("Exception trying to send test to run with exception: " + e.ToString());
                    }
                }
            }
            catch (SocketException e)
            {
                Log.Comment("Exception trying to send/receive from server.");
                Log.Comment("Error Code:" + e.ErrorCode);
            }
            catch (Exception e)
            {
                Log.Comment("Exception:" + e.ToString());
            }
            return testResult;
        }

        /// <summary>
        /// This executes the test case on the server.
        /// </summary>
        public MFTestResults RunTest(string testToRun)
        {
            TestProxy tp = new TestProxy();

            //When calling this method pass in the TestProxy default values.
            return RunTest(testToRun, tp.serverTimeoutSeconds, tp.serverTestResult, tp.scenarioToRun);
        }

        /// <summary>
        /// This executes the test case on the server.
        /// </summary>
        public MFTestResults RunTest(string testToRun, int scenarioToRun)
        {
            TestProxy tp = new TestProxy();

            //When calling this method pass in the TestProxy default values.
            return RunTest(testToRun, tp.serverTimeoutSeconds, tp.serverTestResult, scenarioToRun);
        }

        public MFTestResults GetTestResult()
        {
            MFTestResults testResult = MFTestResults.Fail;

            if (socket != null)
            {
                byte[] receiveBuffer = new byte[1000];
                int len = 0;

                if (socket.Poll(-1, SelectMode.SelectRead))
                {
                    Thread.Sleep(50);

                    len = socket.Available;

                    if (len > receiveBuffer.Length) len = receiveBuffer.Length;

                    len = socket.Receive(receiveBuffer, len, SocketFlags.None);

                    if (len > 0)
                    {
                        TestProxy tp = new TestProxy(receiveBuffer);
                        testResult = tp.serverTestResult;
                    }
                }
            }
            else
                Log.Comment("Must Initialize and Start the TestManager before calling GetTestResult().");

            return testResult;
        }

        public void CloseClient()
        {
            if (listenSocket != null)
                listenSocket.Close();
            if (socket != null)
                socket.Close();
        }
    }
}