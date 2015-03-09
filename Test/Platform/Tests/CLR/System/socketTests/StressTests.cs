/*----------------------------------------------------------------------
*  StressTests.cs - file description
*  Version: 1.0
*  Author: REDMOND\a-grchat
*  Created: 1/7/2008 11:08:59 AM 
* 
* This tests sending large packets and large amounts of large packets
* ---------------------------------------------------------------------*/

using System;
using System.Reflection;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Microsoft.SPOT.Platform.Tests
{
    public class StressTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");

            try
            {
                // Check networking - we need to make sure we can reach our proxy server
                System.Net.Dns.GetHostEntry("itgproxy.dns.microsoft.com");
            }
            catch (Exception ex)
            {
                Log.Exception("Unable to get address for itgproxy.dns.microsoft.com", ex);
                return InitializeResult.Skip;
            }

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        //This is a large number greater than the max buffer size
        private const int LARGE_NUM = 5000;

        private class StressSocketPair : SocketPair
        {
            private AutoResetEvent evt;

            //--//

            public StressSocketPair(ProtocolType prot, SocketType st)
                : base(prot, st)
            {
                if (prot == ProtocolType.Tcp)
                {
                    socketClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, false);
                    socketServer.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, false);
                }
                evt = new AutoResetEvent( false );
            }

            public new void Startup(int portClient, int portServer)
            {
                socketClient.Bind(new IPEndPoint(IPAddress.Loopback, portClient));
                socketServer.Bind(new IPEndPoint(IPAddress.Loopback, portServer));

                epClient = (IPEndPoint)socketClient.LocalEndPoint;
                epServer = (IPEndPoint)socketServer.LocalEndPoint;

                NewData();
            }

            public void NewData()
            {
                NewData( LARGE_NUM );
            }

            public void NewData( int size )
            {
                bufSend    = new byte[size];
                bufReceive = new byte[size];

                new Random().NextBytes(bufSend);
            }

            public void StressTestAsyncThreadProc()
            {
                using(Socket sock = socketServer.Accept())
                {
                    int cToReceive = bufReceive.Length;

                    do
                    {
                        cToReceive -= sock.Receive( bufReceive, bufReceive.Length - cToReceive, cToReceive, SocketFlags.None );
                    }
                    while(cToReceive > 0);
                }
                
                evt.Set();
            }

            public AutoResetEvent SyncPoint
            {
                get
                {
                    return evt;
                }
            }
        }

        
        [TestMethod]
        public MFTestResults StressTest1_LargePacketUDP()
        {
            ///<summary>
            ///1. Starts a server socket listening for TCP
            ///2. Starts a client socket connected to the server
            ///3. Verifies that a large amount of data can be sent
            ///</summary>

            const int c_StartSize = 0x100;
            const int c_EndSize = 0x176F;

            bool testResult = true;
            int size        = c_StartSize;
            int increment   = 2;

            try
            {
                do
                {
                    StressSocketPair testSockets = new StressSocketPair( ProtocolType.Udp, 
                        SocketType.Dgram );
                
                    testSockets.Startup(0, 0);

                    testSockets.NewData( size );

                    int cBytes = testSockets.socketClient.SendTo(testSockets.bufSend, 
                        testSockets.epServer);

                    if (cBytes != testSockets.bufSend.Length)
                        throw new Exception("Send failed, wrong length");

                    int cToReceive = cBytes;

                    int spin = 0;

                    do
                    {
                        int avail = testSockets.socketServer.Available;

                        if(avail > 0)
                        {
                            EndPoint epClient = (EndPoint)testSockets.epClient;
                            cToReceive -= testSockets.socketServer.ReceiveFrom( 
                                testSockets.bufReceive, cBytes - cToReceive, cToReceive, 
                                SocketFlags.None, ref epClient );
                        }
                        else
                        {
                            Thread.Sleep( 100 );

                            ++spin;
                        }
                    }
                    while (cToReceive > 0 && spin < 20 );

                    if(spin == 20)
                        testResult = false;


                    testSockets.AssertDataReceived(cBytes);
                    testSockets.TearDown();
                    testSockets = null;

                    // clean up allocations
                    Debug.GC(true);

                    size *= increment;
                } while (size <= c_EndSize);
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected Exception", e);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
            }
            
            Log.Comment( "Max buffer size successfully sent and received was: " + 
                (size / increment).ToString() );

            return (testResult ? MFTestResults.Pass : MFTestResults.Pass);
        }

        
        [TestMethod]
        public MFTestResults StressTest2_LargePacketTCP()
        {
            ///<summary>
            ///1. Starts a server socket listening for TCP
            ///2. Starts a client socket connected to the server
            ///3. Verifies that a large amount of data can be sent
            ///</summary>

            const int c_StartSize = 0x100;
            const int c_EndSize   = 0x10000;

            MFTestResults testResult =  MFTestResults .Pass;
            int size        = c_StartSize;
            int increment   = 2;
            Thread server = null;

            try
            {
                do
                {
                    StressSocketPair testSockets = new StressSocketPair(
                        ProtocolType.Tcp, SocketType.Stream);

                    server = new Thread(new ThreadStart(
                        testSockets.StressTestAsyncThreadProc));

                    testSockets.Startup(0, 0);

                    testSockets.NewData(size);

                    testSockets.socketServer.Listen(1);

                    testSockets.socketClient.Connect(testSockets.epServer);

                    server.Start();

                    DateTime start = DateTime.Now;

                    int cBytes = testSockets.socketClient.Send(testSockets.bufSend);

                    if (cBytes != testSockets.bufSend.Length)
                        throw new Exception("Send failed, wrong length");

                    testSockets.SyncPoint.WaitOne();
                    DateTime end = DateTime.Now;

                    Log.Comment("Successfully received buffer of size " +
                        testSockets.bufReceive.Length);
                    int duration = (end - start).Milliseconds + 1;
                    Log.Comment("Approximate " + testSockets.bufReceive.Length / duration + " Kb/sec rate to send ");

                    testSockets.AssertDataReceived(cBytes);
                    testSockets.TearDown();
                    testSockets = null;

                    size *= increment;

                    // clean up allocations
                    Debug.GC(true);

                    server.Join();
                    server = null;
                } while (size <= c_EndSize);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                Log.Exception("Unexpected SocketException - ErrorCode: " + ((SocketException)e).ErrorCode, e);

                if (e.ErrorCode == (int)SocketError.MessageSize)
                {
                    testResult = MFTestResults.Pass;
                }
                else
                {
                    testResult = MFTestResults.Fail;
                }
            }
            catch (System.OutOfMemoryException e)
            {
                Log.Exception("System.OutOfMemoryException", e);
                testResult = MFTestResults.Pass;
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected Exception", e);
                testResult = MFTestResults.Fail;
            }
            finally
            {
                if (server != null && server.IsAlive)
                {
                    server.Abort();
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults StressTest3_LargePacketUDP()
        {
            /// <summary>
            /// 1. Starts a server socket listening for UDP
            /// 2. Starts a client socket sending UDP packets
            /// 3. Verifies that a large amount of data can be sent
            /// </summary>
            ///
            MFTestResults testResult = MFTestResults.Pass;

            StressSocketPair testSockets = new StressSocketPair(ProtocolType.Udp,
                SocketType.Dgram);

            try
            {
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                int cBytes = testSockets.socketClient.SendTo(testSockets.bufSend,
                    testSockets.epServer);

                if (cBytes != testSockets.bufSend.Length)
                    throw new Exception("Send failed, wrong length");


                EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
                cBytes = testSockets.socketServer.ReceiveFrom(testSockets.bufReceive,
                    ref epFrom);

                if (testSockets.epClient.Address.ToString() !=
                    ((IPEndPoint)epFrom).Address.ToString())
                    throw new Exception("Bad address");

            }
            catch (SocketException e)
            {
                Log.Exception("Unexpected Socket Excption - ErrorCode: " + e.ErrorCode.ToString(), e);

                if (e.ErrorCode == 10047)
                {
                    Log.Comment("Address family not supported by protocol family.");
                    Log.Comment("This exception is thrown by the device driver that doesnt' implement Loopback for UDP");
                }
                else
                {
                    testResult = MFTestResults.Fail;
                    Log.Comment("Fail for any other error codes that we don't know about");
                }
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected Exception", e);
                testResult = MFTestResults.Fail;
            }

            finally
            {
                testSockets.TearDown();
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults StressTest4_MarathonSend()
        {
            ///<summary>
            ///1. Starts a server socket listening for TCP
            ///2. Starts a client socket connected to the server
            ///3. Verifies that a large amount of data can be sent
            ///     many consecutive times.
            ///</summary>

            MFTestResults testResult = MFTestResults.Pass;

            StressSocketPair testSockets = new StressSocketPair(ProtocolType.Tcp, SocketType.Stream);
            try
            {
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                testSockets.socketClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 5 * 1024);

                testSockets.socketServer.Listen(1);

                testSockets.socketClient.Connect(testSockets.epServer);

                int cBytes = 0;

                using (Socket sock = testSockets.socketServer.Accept())
                {
                    sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 5 * 1024);

                    for (int i = 0; i < 100; i++)
                    {
                        cBytes = testSockets.socketClient.Send(testSockets.bufSend);

                        if (cBytes != testSockets.bufSend.Length)
                            throw new Exception("Send failed, wrong length");

                        if (sock.LocalEndPoint == null)
                            throw new Exception("LocalEndPoint is null");
                        //Fix?
                        if (testSockets.socketServer.LocalEndPoint.ToString() !=
                            sock.LocalEndPoint.ToString())
                            throw new Exception("LocalEndPoint is incorrect");

                        if (sock.RemoteEndPoint == null)
                            throw new Exception("RemoteEndPoint is null");
                        if (testSockets.socketClient.LocalEndPoint.ToString() !=
                            sock.RemoteEndPoint.ToString())
                            throw new Exception("RemoteEndPoint is incorrect");

                        int cToReceive = cBytes;

                        do
                        {
                            cToReceive -= sock.Receive(testSockets.bufReceive,
                                cBytes - cToReceive, cToReceive, SocketFlags.None);
                        }
                        while (cToReceive > 0);
                    }
                    testSockets.AssertDataReceived(cBytes);
                    testSockets.NewData();
                }
            }
            catch (System.OutOfMemoryException e)
            {
                Log.Exception("Out of Memory Exception is ok on devices with limited memory constraints.", e);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                Log.Exception("ErrorCode: " + ((SocketException)e).ErrorCode, e);
                testResult = MFTestResults.Fail;
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected Exception", e);
                testResult = MFTestResults.Fail;
            }
            finally
            {
                testSockets.TearDown();
            }

            return testResult;
        }
    }
}
