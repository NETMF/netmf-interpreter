/*---------------------------------------------------------------------
* SocketTests.cs - file description
* Version: 1.0
* Author: REDMOND\a-grchat
* Created: 9/12/2007 4:51:10 PM 
* 
* Tests the basic functionality of Socket objects 
* ---------------------------------------------------------------------*/

using System;
using System.Reflection;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Microsoft.SPOT.Net.NetworkInformation;

namespace Microsoft.SPOT.Platform.Tests
{

    public class SocketTests : IMFTestInterface
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
            Log.Comment("Cleaning up after the tests");
        }

        //[TestMethod]
        //public MFTestResults NetworkStream_Read()
        //{
        //    MFTestResults testResult = MFTestResults.Pass;

        //    IPHostEntry hostEntry = Dns.GetHostEntry("");
        //    IPAddress ipAddress = null;
        //    for (int i = 0; i < hostEntry.AddressList.Length; ++i)
        //    {
        //        try
        //        {
        //            Debug.Print(hostEntry.AddressList[i].ToString());
        //            ipAddress = hostEntry.AddressList[i];
        //        }
        //        catch { }
        //    }

        //    IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 80);

        //    Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //    s.Bind(ipEndPoint);
        //    s.Listen(1);

        //    NetworkStream ns = new NetworkStream(s.Accept(),false);

        //    byte[] msg = new Byte[25];
        //    int bytesRead= 0;

        //        bytesRead = ns.Read(msg, 0, msg.Length);

        //    s.Close();

        //    return testResult;
        //}


        //[TestMethod]
        //public MFTestResults SocketTest14_DPWSOptions()
        //{
        //    MFTestResults testResult = MFTestResults.Pass;

        //    IPHostEntry hostEntry = Dns.GetHostEntry("");
        //    IPAddress ipAddress = null;
        //    for (int i = 0; i < hostEntry.AddressList.Length; ++i)
        //    {
        //        try
        //        {
        //            Debug.Print(hostEntry.AddressList[i].ToString());
        //            ipAddress = hostEntry.AddressList[i];
        //        }
        //        catch { }
        //    }

        //    IPEndPoint endPoint = new IPEndPoint(ipAddress, 11000);

        //    Socket s = new Socket(AddressFamily.InterNetwork,
        //        SocketType.Dgram,
        //        ProtocolType.Udp);

        //    // Creates an IPEndPoint to capture the identity of the sending host.
        //    IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        //    EndPoint senderRemote = (EndPoint)sender;

        //    // Binding is required with ReceiveFrom calls.
        //    s.Bind(endPoint);

        //    byte[] msg = new Byte[256];
        //    Debug.Print("Waiting to receive datagrams from client...");
        //    // This call blocks. 
        //    s.ReceiveFrom(msg, msg.Length, SocketFlags.None, ref senderRemote);
        //    s.Close();


        //    return testResult;
        //}

        [TestMethod]
        public MFTestResults SocketTest0_MaxSockets()
        {
            ///<summary>
            ///1. Starts Numerous sockets with linger option to false.
            ///2. Should not fail because we should be able to reclaim those sockets if linger is true
            ///</summary>

            bool testResult = true;
            int i = 1;
            try
            {
                for (; i < 300; ++i)
                {
                    using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, false);
                    }
                }

                i = 1;
                for (; i < 300; ++i)
                {
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, false);
                    socket.Close();
                }

            }
            catch (Exception e)
            {
                Log.Comment("Incorrectly threw an exception trying to create " + i.ToString() + " sockets.");
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketTest1_BasicTCP()
        {
            ///<summary>
            ///1. Starts a server socket listening for TCP
            ///2. Starts a client socket connected to the server
            ///3. Verifies that data can be correctly sent and recieved
            ///</summary>

            bool testResult = true;
            SocketPair testSockets = null;
            try
            {
                testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                testSockets.socketServer.Listen(1);

                testSockets.socketClient.Connect(testSockets.epServer);

                int cBytes = testSockets.socketClient.Send(testSockets.bufSend);

                if (cBytes != testSockets.bufSend.Length)
                    throw new Exception("Send failed, wrong length");



                using (Socket sock = testSockets.socketServer.Accept())
                {
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

                    cBytes = sock.Available;

                    if (cBytes != testSockets.bufSend.Length)
                        throw new Exception("Send failed, wrong length");

                    cBytes = sock.Receive(testSockets.bufReceive);
                }
                testSockets.AssertDataReceived(cBytes);

            }
            catch (Exception e)
            {
                Log.Comment("Caught exception: " + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
            }
            finally
            {
                if (testSockets != null)
                {
                    testSockets.TearDown();
                    testSockets = null;
                }
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketTest1_TCP_LingerFalse()
        {
            ///<summary>
            ///1. Starts a server socket listening for TCP
            ///2. Starts a client socket connected to the server
            ///3. Verifies that data can be correctly sent and recieved
            ///4. Verifies linger is functional
            ///</summary>

            ///skip this test for the emulator since setting socket options is not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            MFTestResults testResult = MFTestResults.Pass;

            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Loopback, 3053);
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Loopback, 3054);
            SocketServer server = null;
            try
            {
                server = new SocketServer(TestType.Linger, AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server.LingerValue = false;
                server.Bind(serverEndPoint);
                server.Listen(1);
                server.Start();

                for (int i = 0; i < 2; ++i)
                {
                    Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, false);

                        Log.Comment("Testing with port 0");

                        Log.Comment("For linger verify that we can reuse the ipEndPoint multiple times");

                        client.Bind(clientEndPoint);

                        client.Connect(server.LocalEndPoint);

                        int bytesSent = client.Send(new byte[200]);

                        Log.Comment("Create another client to connect to the server endpoint directly after this client closes.");
                    }
                    catch (SocketException e)
                    {
                        Log.Comment("Exception " + e);
                        Log.Comment("ErrorCode: " + e.ErrorCode);
                        testResult = MFTestResults.Fail;
                        break;
                    }
                    catch (Exception e)
                    {
                        Log.Comment("Exception " + e);
                        testResult = MFTestResults.Fail;
                        break;
                    }
                    finally
                    {
                        client.Close();
                    }
                }
            }
            catch (SocketException e)
            {
                Log.Comment("Caught exception: " + e.Message);
                Log.Comment("ErrorCode: " + e.ErrorCode.ToString());
                testResult = MFTestResults.Fail;
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception: " + e.Message);
                testResult = MFTestResults.Fail;
            }
            finally
            {
                if (server != null)
                {
                    server.Stop();
                    server.Close();
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults SocketTest1_TCP_LingerTrue()
        {
            ///<summary>
            ///1. Starts a server socket listening for TCP
            ///2. Starts a client socket connected to the server
            ///3. Verifies that data can be correctly sent and recieved
            ///4. Verifies linger is functional
            ///</summary>

            ///skip this test for the emulator since setting socket options is not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            MFTestResults testResult = MFTestResults.Fail;

            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Loopback, 3055);
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Loopback, 3056);

            SocketServer server = null;

            try
            {
                server = new SocketServer(TestType.Linger, AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                server.LingerValue = true;
                server.Bind(serverEndPoint);
                server.Listen(1);
                server.Start();

                for (int i = 0; i < 2; ++i)
                {
                    Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        Log.Comment("Testing with port 0");
                        client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, 3000);

                        Log.Comment("For linger verify that we can reuse the ipEndPoint multiple times");

                        client.Bind(clientEndPoint);
                        client.Connect(server.LocalEndPoint);

                        int bytesSent = client.Send(new byte[200]);

                        Log.Comment("Create another client to connect to the server endpoint directly after this client closes.");

                    }
                    catch (SocketException e)
                    {
                        Log.Comment("Exception " + e);
                        Log.Comment("ErrorCode: " + e.ErrorCode);
                        testResult = MFTestResults.Pass;
                    }
                    catch (Exception e)
                    {
                        Log.Comment("Exception " + e);
                        testResult = MFTestResults.Fail;
                        break;
                    }
                    finally
                    {
                        client.Close();
                    }
                }
            }
            catch (SocketException e)
            {
                Log.Comment("Caught exception: " + e.Message);
                Log.Comment("ErrorCode: " + e.ErrorCode.ToString());
                testResult = MFTestResults.Fail;
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception: " + e.Message);
                testResult = MFTestResults.Fail;
            }
            finally
            {
                if (server != null)
                {
                    server.Stop();
                    server.Close();
                }
            }

            return testResult;
        }



        [TestMethod]
        public MFTestResults SocketTest1_TCP_NoDelay()
        {
            ///<summary>
            ///1. Starts a server socket listening for TCP
            ///2. Starts a client socket connected to the server
            ///3. Verifies that data can be correctly sent and recieved
            ///4. Verifies NoDelay is functional
            ///</summary>

            ///skip this test for the emulator since setting socket options is not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            MFTestResults testResult = MFTestResults.Pass;

            bool noDelay = false;
            int bytesReceivedNoDelay = 0;
            int bytesReceivedDelay = 0;

            do
            {
                SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);

                try
                {

                    testSockets.socketClient.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, noDelay);

                    Log.Comment("Testing with port 0");
                    testSockets.Startup(0, 0);

                    testSockets.socketServer.Listen(1);

                    testSockets.socketClient.Connect(testSockets.epServer);

                    Log.Comment("send lots of small packets.");
                    int bytesSent = 0;

                    for (int i = 0; i < 140; ++i)
                    {
                        bytesSent += testSockets.socketClient.Send(testSockets.bufSend);
                    }

                    using (Socket sock = testSockets.socketServer.Accept())
                    {
                        byte[] recBytes = new byte[486*testSockets.bufSend.Length];
                        int bytesReceived = sock.Available;
                        bytesReceived = sock.Receive(recBytes);

                        if (noDelay)
                            bytesReceivedNoDelay = bytesReceived;
                        else
                            bytesReceivedDelay = bytesReceived;

                        Log.Comment("BytesSent: " + bytesSent + " BytesReceived: " + bytesReceived);
                    }
                }
                catch (SocketException e)
                {
                    Log.Comment("Caught exception: " + e.Message);
                    Log.Comment("ErrorCode: " + e.ErrorCode.ToString());
                    testResult = MFTestResults.Fail;
                }
                catch (Exception e)
                {
                    Log.Comment("Caught exception: " + e.Message);
                    testResult = MFTestResults.Fail;
                }
                finally
                {
                    testSockets.TearDown();
                }
                noDelay = !noDelay;
            }
            while (noDelay);

            if (bytesReceivedNoDelay < bytesReceivedDelay)
            {
                testResult = MFTestResults.Fail;
                Log.Comment("We've received more bytes with nodelay enabled.");
            }
            return testResult;
        }
        
        [TestMethod]
        public MFTestResults SocketTest1_TCP_ReceiveTimeout()
        {
            ///<summary>
            ///1. Starts a server socket listening for TCP
            ///2. Starts a client socket connected to the server
            ///3. Verifies that data can be correctly sent and recieved
            ///4. Verifies Receive timeout is functional
            ///</summary>

            ///skip this test for the emulator since setting socket options is not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            MFTestResults testResult = MFTestResults.Fail;

            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                int receiveTimeout = (int)server.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout);
                Log.Comment("Get the receiveTimeout size on the server = " + receiveTimeout);

                int newTimeout = 1000;
                Log.Comment("Set the receiveTimeout size on the server = " + newTimeout);
                server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, newTimeout);

                int receiveTimeout2 = (int)server.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout);
                Log.Comment("The new receiveTimeout size on the server=" + receiveTimeout2);


                Log.Comment("Testing with port 0");
                client.Bind(new IPEndPoint(IPAddress.Loopback, 0));
                server.Bind(new IPEndPoint(IPAddress.Loopback, 0));

                IPEndPoint epClient = (IPEndPoint)client.LocalEndPoint;
                IPEndPoint epServer = (IPEndPoint)server.LocalEndPoint;

                server.Listen(1);

                client.Connect(epServer);

                using (Socket sock = server.Accept())
                {
                    byte[] recBytes = new byte[12000];

                    Log.Comment("Dont send any data.  This will cause receive to timeout.");
                    //int bytesSent = client.Send(new byte[1]);

                    try
                    {
                        Log.Comment("Receiving bytes.");
                        server.Receive(recBytes);
                        Log.Comment("Receive returned unexpectedly");
                    }
                    catch (SocketException e)
                    {
                        Log.Comment("correctly threw exception after Receive Timeout on server: " + e);
                        testResult = MFTestResults.Pass;
                    }
                    catch (Exception e)
                    {
                        Log.Comment("incorrectly threw exception: " + e);
                        testResult = MFTestResults.Fail;
                    }
                }
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == (int)SocketError.ProtocolOption)
                {
                    testResult = MFTestResults.Skip;
                }
                else
                {
                    Log.Comment("Caught exception: " + e.Message);
                    Log.Comment("ErrorCode: " + e.ErrorCode.ToString());
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception: " + e.Message);
                testResult = MFTestResults.Fail;
            }
            finally
            {
                server.Close();
                client.Close();
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults SocketTest1_TCP_SendTimeout()
        {
            ///<summary>
            ///1. Starts a server socket listening for TCP
            ///2. Starts a client socket connected to the server
            ///3. Verifies that data can be correctly sent and recieved
            ///4. Verifies Send timeout is functional
            ///</summary>

            ///skip this test for the emulator since setting socket options is not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            MFTestResults testResult = MFTestResults.Fail;

            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                int sendTimeout = (int)client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout);
                Log.Comment("Get the SendTimeout size on the server = " + sendTimeout);

                int newTimeout = 1000;
                Log.Comment("Set the receiveTimeout size on the server = " + newTimeout);
                client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, newTimeout);

                int sendTimeout2 = (int)client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout);
                Log.Comment("The new SendTimeout size on the server=" + sendTimeout2);


                Log.Comment("Testing with port 0");
                client.Bind(new IPEndPoint(IPAddress.Loopback, 0));
                server.Bind(new IPEndPoint(IPAddress.Loopback, 0));

                IPEndPoint epClient = (IPEndPoint)client.LocalEndPoint;
                IPEndPoint epServer = (IPEndPoint)server.LocalEndPoint;

                server.Listen(1);

                client.Connect(epServer);

                using (Socket sock = server.Accept())
                {
                    byte[] recBytes = new byte[12000];

                    Log.Comment("Send to a closed server to cause a timeout.");
                    try
                    {
                        for( int i=0; i<1000; ++i)
                        {
                            int bytesSent = client.Send(recBytes);
                        }
                        Log.Comment("Receiving bytes.");
                        testResult = MFTestResults.Fail;
                    }
                    catch (SocketException e)
                    {
                        Log.Comment("correctly threw exception after Send Timeout on server: " + e);
                        testResult = MFTestResults.Pass;
                    }
                    catch (Exception e)
                    {
                        Log.Comment("incorrectly threw exception: " + e);
                        testResult = MFTestResults.Fail;
                    }
                }
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == (int)SocketError.ProtocolOption)
                {
                    testResult = MFTestResults.Skip;
                }
                else
                {
                    Log.Comment("Caught exception: " + e.Message);
                    Log.Comment("ErrorCode: " + e.ErrorCode.ToString());
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception: " + e.Message);
                testResult = MFTestResults.Fail;
            }
            finally
            {
                server.Close();
                client.Close();
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults SocketTest2_BasicUDP()
        {
            /// <summary>
            /// 1. Starts a server socket listening for UDP
            /// 2. Starts a client socket sending UDP packets
            /// 3. Verifies that data can be correctly sent and recieved
            /// </summary>
            ///
            bool testResult = true;
            SocketPair testSockets = null;
            try
            {
                testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                int cBytes = testSockets.socketClient.SendTo(
                        testSockets.bufSend, testSockets.epServer);

                if (cBytes != testSockets.bufSend.Length)
                    throw new Exception("Send failed, wrong length");

                EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
                cBytes = testSockets.socketServer.ReceiveFrom(testSockets.bufReceive, ref epFrom);

                if (testSockets.epClient.Address.ToString() != ((IPEndPoint)epFrom).Address.ToString())
                    throw new Exception("Bad address");
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == 10047)
                {
                    Log.Comment("Address family not supported by protocol family.");
                    Log.Comment("This exception is thrown by the device driver that doesnt' implement Loopback for UDP");
                }
                else
                {
                    testResult = false;
                    Log.Comment("Fail for any other error codes that we don't know about");
                }
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception: " + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
            }
            finally
            {
                if (testSockets != null)
                {
                    testSockets.TearDown();
                    testSockets = null;
                }
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketTest3_SocketOption()
        {
            /// <summary>
            /// 1. Creates a pair of UDP sockets
            /// 3. Verifies that SocketOptions can be correctly set and got
            /// </summary>
            ///
            bool testResult = true;
            SocketPair testSockets = null;
            try
            {
                testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                int iSet = 0x1024;
                int iGet;
                testSockets.socketClient.SetSocketOption(
                    SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, iSet);
                iGet = (int)testSockets.socketClient.GetSocketOption(
                    SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer);

                if (iSet != iGet)
                    throw new Exception("Socket option flag ints differ");

                testSockets.socketClient.SetSocketOption(
                    SocketOptionLevel.Socket, SocketOptionName.SendBuffer, iSet);
                iGet = (int)testSockets.socketClient.GetSocketOption(
                    SocketOptionLevel.Socket, SocketOptionName.SendBuffer);

                if (iSet != iGet)
                    throw new Exception("Socket option flag ints differ");

                bool fSet = true;
                bool fGet;

                try
                {
                    testSockets.socketClient.SetSocketOption(
                        SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, fSet);

                    fGet = ((int)testSockets.socketClient.GetSocketOption(
                        SocketOptionLevel.Socket,
                        SocketOptionName.ReuseAddress) == 0x1);

                    if (fSet != fGet)
                        throw new Exception("Socket option flag bools differ");
                }
                catch (SocketException se)
                {
                    if (se.ErrorCode != (int)SocketError.ProtocolOption) throw;
                }
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception: " + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
            }
            finally
            {
                if (testSockets != null)
                {
                    testSockets.TearDown();
                    testSockets = null;
                }
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketTest5_BasicRAW()
        {
            /// <summary>
            /// 1. Starts a server socket listening for Raw
            /// 2. Starts a client socket connected to the server
            /// 3. Verifies that data can be correctly sent and recieved
            /// </summary>
            ///
            bool testResult = true;
            SocketPair testSockets = null;
            try
            {
                testSockets = new SocketPair(ProtocolType.Raw, SocketType.Raw);
                testSockets.Startup(0, 0);
                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                int cBytes = testSockets.socketClient.Send(testSockets.bufSend);

                if (cBytes != testSockets.bufSend.Length)
                    throw new Exception("Send failed, wrong length");

                using (Socket sock = testSockets.socketServer.Accept())
                {
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

                    cBytes = sock.Available;

                    if (cBytes != testSockets.bufSend.Length)
                        throw new Exception("Send failed, wrong length");

                    cBytes = sock.Receive(testSockets.bufReceive);
                }

                testSockets.AssertDataReceived(cBytes);
            }
            catch (SocketException e)
            {
                Log.Comment("Caught exception: " + e.Message);
                Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception: " + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
            }
            finally
            {
                if (testSockets != null)
                {
                    testSockets.TearDown();
                    testSockets = null;
                }
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketTest6_TCPRecieve()
        {
            /// <summary>
            /// 1. Starts a server socket listening for TCP
            /// 2. Starts a client socket connected to the server
            /// 3. Verifies that data can be correctly sent and recieved using 
            ///  each prototype of Recieve method
            /// 4. Verifies that exceptions are correctly thrown for Recieve calls that have
            ///  bad Offset or Size parameters
            /// </summary>
            ///
            bool testResult = true;
            SocketPair testSockets = null;

            try
            {
                testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                int cBytes = testSockets.socketClient.Send(testSockets.bufSend);
                using (Socket sock = testSockets.socketServer.Accept())
                {
                    cBytes = sock.Available;
                    cBytes = sock.Receive(testSockets.bufReceive, SocketFlags.None);
                }

                testSockets.AssertDataReceived(cBytes);
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception in Recieve with Byte Array, SocketFlags: "
                    + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
            }
            finally
            {
                if (testSockets != null)
                {
                    testSockets.TearDown();
                    testSockets = null;
                }
            }

            try
            {
                testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                int cBytes = testSockets.socketClient.Send(testSockets.bufSend);
                using (Socket sock = testSockets.socketServer.Accept())
                {
                    cBytes = sock.Available;
                    cBytes = sock.Receive(testSockets.bufReceive,
                        testSockets.bufSend.Length, SocketFlags.None);
                }

                testSockets.AssertDataReceived(cBytes);
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception in Recieve with Byte Array, Int, SocketFlags: "
                    + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
            }
            finally
            {
                if (testSockets != null)
                {
                    testSockets.TearDown();
                    testSockets = null;
                }
            }

            bool subResult = false;
            try
            {
                testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                int cBytes = testSockets.socketClient.Send(testSockets.bufSend);
                using (Socket sock = testSockets.socketServer.Accept())
                {
                    cBytes = sock.Available;
                    cBytes = sock.Receive(testSockets.bufReceive, testSockets.bufSend.Length
                        + 2, SocketFlags.None);
                }

                testSockets.AssertDataReceived(cBytes);
            }
            catch (System.IndexOutOfRangeException)
            {
                Log.Comment("IndexOutOfRangeException Successfully Caught");
                subResult = true;
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception in Recieve with Byte Array, Int, SocketFlags: "
                    + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
                subResult = true;
            }
            finally
            {
                if (testSockets != null)
                {
                    testSockets.TearDown();
                    testSockets = null;
                }
                if (!subResult)
                    Log.Comment("Erroneously succeeded with bad Int-Size parameter");
                testResult &= subResult;
            }

            try
            {
                testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);
                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                int cBytes = testSockets.socketClient.Send(testSockets.bufSend);
                using (Socket sock = testSockets.socketServer.Accept())
                {
                    cBytes = sock.Available;
                    cBytes = sock.Receive(testSockets.bufReceive, 0,
                        testSockets.bufSend.Length,
                        SocketFlags.None);
                }

                testSockets.AssertDataReceived(cBytes);
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception in Recieve with Byte Array, Int-Offset,"
                    + "Int-Size, SocketFlags: " + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
            }
            finally
            {
                if (testSockets != null)
                {
                    testSockets.TearDown();
                    testSockets = null;
                }
            }

            subResult = false;
            try
            {
                testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                int cBytes = testSockets.socketClient.Send(testSockets.bufSend);
                using (Socket sock = testSockets.socketServer.Accept())
                {
                    cBytes = sock.Available;
                    cBytes = sock.Receive(testSockets.bufReceive, 2,
                        testSockets.bufSend.Length,
                        SocketFlags.None);
                }

                testSockets.AssertDataReceived(cBytes);
            }
            catch (System.IndexOutOfRangeException)
            {
                Log.Comment("IndexOutOfRangeException Successfully Caught");
                subResult = true;
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception in Recieve with Byte Array," +
                    " Int-Offset, Int-Size, SocketFlags: " + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
                subResult = true;
            }
            finally
            {
                if (testSockets != null)
                {
                    testSockets.TearDown();
                    testSockets = null;
                }
                if (!subResult)
                    Log.Comment("Erroneously succeeded with bad Int-Offset parameter");
                testResult &= subResult;
            }

            try
            {
                testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                int cBytes = testSockets.socketClient.Send(testSockets.bufSend, 1,
                    testSockets.bufSend.Length - 1,
                    SocketFlags.None);
                int throwAway;
                using (Socket sock = testSockets.socketServer.Accept())
                {
                    throwAway = sock.Available;
                    cBytes = sock.Receive(testSockets.bufReceive, 1, 2, SocketFlags.None);


                    throwAway = testSockets.socketClient.Send(testSockets.bufSend, 0, 1,
                        SocketFlags.None);
                    throwAway = sock.Available;
                    cBytes += sock.Receive(testSockets.bufReceive, 0, 1, SocketFlags.None);

                }

                testSockets.AssertDataReceived(cBytes);
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception in Recieve with Byte Array, "
                    + "Int-Offset, Int-Size, SocketFlags: " + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
            }
            finally
            {
                if (testSockets != null)
                {
                    testSockets.TearDown();
                    testSockets = null;
                }
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketTest7_TCPSend()
        {
            /// <summary>
            /// 1. Starts a server socket listening for TCP
            /// 2. Starts a client socket connected to the server
            /// 3. Verifies that data can be correctly sent and recieved using 
            ///  each prototype of Send method
            /// 4. Verifies that exceptions are correctly thrown for Send calls that have
            ///  bad Offset or Size parameters
            /// </summary>
            ///
            bool testResult = true;
            SocketPair testSockets = null;
            try
            {
                testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                int cBytes = testSockets.socketClient.Send(testSockets.bufSend,
                    SocketFlags.None);
                using (Socket sock = testSockets.socketServer.Accept())
                {
                    cBytes = sock.Available;
                    cBytes = sock.Receive(testSockets.bufReceive);
                }

                testSockets.AssertDataReceived(cBytes);
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception in Send with Byte Array, SocketFlags: "
                    + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
            }
            finally
            {
                if (testSockets != null)
                {
                    testSockets.TearDown();
                    testSockets = null;
                }
            }

            try
            {
                testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                int cBytes = testSockets.socketClient.Send(testSockets.bufSend,
                    testSockets.bufSend.Length,
                    SocketFlags.None);
                using (Socket sock = testSockets.socketServer.Accept())
                {
                    cBytes = sock.Available;
                    cBytes = sock.Receive(testSockets.bufReceive);
                }

                testSockets.AssertDataReceived(cBytes);
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception in Send with Byte Array, Int, SocketFlags: "
                    + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
            }
            finally
            {
                if (testSockets != null)
                {
                    testSockets.TearDown();
                    testSockets = null;
                }
            }

            bool subResult = false;
            try
            {
                testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                int cBytes = testSockets.socketClient.Send(testSockets.bufSend,
                    testSockets.bufSend.Length + 2,
                    SocketFlags.None);
                using (Socket sock = testSockets.socketServer.Accept())
                {
                    cBytes = sock.Available;
                    cBytes = sock.Receive(testSockets.bufReceive);
                }

                testSockets.AssertDataReceived(cBytes);
            }
            catch (System.IndexOutOfRangeException)
            {
                Log.Comment("IndexOutOfRangeException Successfully Caught");
                subResult = true;
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception in Send with Byte Array, Int, SocketFlags: "
                    + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
                subResult = true;
            }
            finally
            {
                if (testSockets != null)
                {
                    testSockets.TearDown();
                    testSockets = null;
                }
                if (!subResult)
                    Log.Comment("Erroneously succeeded with bad Int-Size parameter");
                testResult &= subResult;
            }

            try
            {
                testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                int cBytes = testSockets.socketClient.Send(testSockets.bufSend, 0,
                    testSockets.bufSend.Length,
                    SocketFlags.None);
                using (Socket sock = testSockets.socketServer.Accept())
                {
                    cBytes = sock.Available;
                    cBytes = sock.Receive(testSockets.bufReceive);
                }

                testSockets.AssertDataReceived(cBytes);
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception in Send with Byte Array, Int-Offset, "
                + "Int-Size, SocketFlags: " + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
            }
            finally
            {
                if (testSockets != null)
                {
                    testSockets.TearDown();
                    testSockets = null;
                }
            }

            subResult = false;
            try
            {
                testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                int cBytes = testSockets.socketClient.Send(testSockets.bufSend, 2,
                    testSockets.bufSend.Length,
                    SocketFlags.None);
                using (Socket sock = testSockets.socketServer.Accept())
                {
                    cBytes = sock.Available;
                    cBytes = sock.Receive(testSockets.bufReceive);
                }

                testSockets.AssertDataReceived(cBytes);
            }
            catch (System.IndexOutOfRangeException)
            {
                Log.Comment("IndexOutOfRangeException Successfully Caught");
                subResult = true;
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception in Send with Byte Array, Int-Offset, " +
                "Int-Size, SocketFlags: " + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
                subResult = true;
            }
            finally
            {
                if (testSockets != null)
                {
                    testSockets.TearDown();
                    testSockets = null;
                }

                if (!subResult)
                    Log.Comment("Erroneously succeeded with bad Int-Offset parameter");
                testResult &= subResult;
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketTest8_TCPRecieveFrom()
        {
            /// <summary>
            /// 1. Starts a server socket listening for TCP
            /// 2. Starts a client socket connected to the server
            /// 3. Verifies that data can be correctly sent and Recieved using 
            ///  each prototype of RecieveFrom method
            /// 4. Verifies that exceptions are correctly thrown for RecieveFrom calls that have
            ///  bad Offset or Size parameters
            /// </summary>
            ///
            MFTestResults testResult = MFTestResults.Pass;
            SocketPair testSockets = null;

            try
            {
                testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                EndPoint remoteEndPoint = testSockets.epClient.Create(testSockets.epClient.Serialize());
                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                
                int cBytes = testSockets.socketClient.Send(testSockets.bufSend);
                using (Socket sock = testSockets.socketServer.Accept())
                {
                    cBytes = sock.ReceiveFrom(testSockets.bufReceive, 0, testSockets.bufSend.Length, SocketFlags.None, ref remoteEndPoint);
                }

                testSockets.AssertDataReceived(cBytes);
                testSockets.TearDown();
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception in RecieveFrom with Byte Array,"
                    + " Int, SocketFlags: " + e.Message);
                testResult = MFTestResults.Fail;
            }
            finally
            {
                if (testSockets != null)
                {
                    testSockets.TearDown();
                    testSockets = null;
                }
            }
            return testResult;
        }


        [TestMethod]
        public MFTestResults SocketTest9_TCPSendTo()
        {
            /// <summary>
            /// 1. Starts a server socket listening for TCP
            /// 2. Starts a client socket connected to the server
            /// 3. Verifies that data can be correctly sent and recieved using 
            ///  each prototype of SendTo method
            /// 4. Verifies that exceptions are correctly thrown for SendTo calls that have
            ///  bad Offset or Size parameters
            /// </summary>
            ///
            SocketPair testSockets = null;
            bool testResult = true;

            try
            {
                testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                EndPoint serverEndPoint = testSockets.epServer.Create(
                        testSockets.epServer.Serialize());
                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                int cBytes = testSockets.socketClient.SendTo(testSockets.bufSend,
                    SocketFlags.None, serverEndPoint);
                using (Socket sock = testSockets.socketServer.Accept())
                {
                    cBytes = sock.Available;
                    cBytes = sock.Receive(testSockets.bufReceive);
                }

                testSockets.AssertDataReceived(cBytes);
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception in SendTo with Byte Array, SocketFlags: "
                    + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
            }
            finally
            {
                if (testSockets != null)
                {
                    testSockets.TearDown();
                    testSockets = null;
                }
            }

            try
            {
                testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                EndPoint serverEndPoint = testSockets.epServer.Create(
                        testSockets.epServer.Serialize());
                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                int cBytes = testSockets.socketClient.SendTo(
                    testSockets.bufSend, testSockets.bufSend.Length,
                    SocketFlags.None, serverEndPoint);
                using (Socket sock = testSockets.socketServer.Accept())
                {
                    cBytes = sock.Available;
                    cBytes = sock.Receive(testSockets.bufReceive);
                }

                testSockets.AssertDataReceived(cBytes);
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception in SendTo with Byte Array, Int, SocketFlags: "
                    + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
            }
            finally
            {
                if (testSockets != null)
                {
                    testSockets.TearDown();
                    testSockets = null;
                }
            }

            bool subResult = false;
            try
            {
                testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                EndPoint serverEndPoint = testSockets.epServer.Create(
                        testSockets.epServer.Serialize());
                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                int cBytes = testSockets.socketClient.SendTo(testSockets.bufSend,
                    testSockets.bufSend.Length + 2,
                    SocketFlags.None, serverEndPoint);
                using (Socket sock = testSockets.socketServer.Accept())
                {
                    cBytes = sock.Available;
                    cBytes = sock.Receive(testSockets.bufReceive);
                }

                testSockets.AssertDataReceived(cBytes);
            }
            catch (System.IndexOutOfRangeException)
            {
                Log.Comment("IndexOutOfRangeException Successfully Caught");
                subResult = true;
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception in SendTo with Byte Array, Int, SocketFlags: "
                    + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
                subResult = true;
            }
            finally
            {
                if (testSockets != null)
                {
                    testSockets.TearDown();
                    testSockets = null;
                }
                if (!subResult)
                    Log.Comment("Erroneously succeeded with bad Int-Size parameter");
                testResult &= subResult;
            }

            try
            {
                testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                EndPoint serverEndPoint = testSockets.epServer.Create(
                        testSockets.epServer.Serialize());
                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                int cBytes = testSockets.socketClient.SendTo(
                    testSockets.bufSend, 0, testSockets.bufSend.Length,
                    SocketFlags.None, serverEndPoint);
                using (Socket sock = testSockets.socketServer.Accept())
                {
                    cBytes = sock.Available;
                    cBytes = sock.Receive(testSockets.bufReceive);
                }

                testSockets.AssertDataReceived(cBytes);
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception in SendTo with Byte Array,"
                    + " Int-Offset, Int-Size, SocketFlags: " + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
            }
            finally
            {
                if (testSockets != null)
                {
                    testSockets.TearDown();
                    testSockets = null;
                }
            }

            subResult = false;
            try
            {
                testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                EndPoint serverEndPoint = testSockets.epServer.Create(
                        testSockets.epServer.Serialize());
                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                int cBytes = testSockets.socketClient.SendTo(
                    testSockets.bufSend, 2, testSockets.bufSend.Length,
                    SocketFlags.None, serverEndPoint);
                using (Socket sock = testSockets.socketServer.Accept())
                {
                    cBytes = sock.Available;
                    cBytes = sock.Receive(testSockets.bufReceive);
                }

                testSockets.AssertDataReceived(cBytes);
                testSockets.TearDown();
                testSockets = null;
            }
            catch (System.IndexOutOfRangeException)
            {
                Log.Comment("IndexOutOfRangeException Successfully Caught");
                subResult = true;
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception in SendTo with Byte Array,"
                    + " Int-Offset, Int-Size, SocketFlags: " + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
                subResult = true;
            }
            finally
            {
                if (!subResult)
                    Log.Comment("Erroneously succeeded with bad Int-Offset parameter");
                testResult &= subResult;
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }


        [TestMethod]
        public MFTestResults SocketTest10_TCPPoll()
        {
            /// <summary>
            /// 1. Starts a server socket listening for TCP
            /// 2. Starts a client socket connected to the server
            /// 3. Transfers some data
            /// 4. Verifies that Poll returns correct results after each individual
            /// function is called.
            /// </summary>
            ///
            bool testResult = true;

            try
            {
                SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                testSockets.socketServer.Listen(1);
                Log.Comment("Listen called");
                
                testSockets.socketClient.Connect(testSockets.epServer);
                Log.Comment("Connect called");

                int cBytes = testSockets.socketClient.Send(testSockets.bufSend);
                Log.Comment("Send called");
                testResult = (testSockets.socketClient.Poll(1000, SelectMode.SelectWrite) == true);
                testResult = (testSockets.socketServer.Poll(1000, SelectMode.SelectRead) == true);

                using (Socket sock = testSockets.socketServer.Accept())
                {
                    Log.Comment("Accept called");
                    testResult = (testSockets.socketClient.Poll(1000, SelectMode.SelectWrite) == true);

                    cBytes = sock.Available;
                    Log.Comment("Available bytes assigned");
                    testResult = (testSockets.socketClient.Poll(1000, SelectMode.SelectWrite) == true);

                    cBytes = sock.Receive(testSockets.bufReceive);
                    Log.Comment("Recieve called");
                    testResult = (testSockets.socketClient.Poll(1000, SelectMode.SelectWrite) == true);
                }

                testSockets.AssertDataReceived(cBytes);
                testSockets.TearDown();
                testSockets = null;
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception: " + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " +
                        ((SocketException)e).ErrorCode); testResult = false;
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketTest11_SetSocketOptionBasic()
        {
            /// <summary>
            /// 1. Call SetSocketOption with each of its signatures
            /// </summary>
            ///
            bool isAnyCatch = false;
            try
            {
                SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);

                // check the bool
                testSockets.socketClient.SetSocketOption(
                    SocketOptionLevel.Socket, SocketOptionName.Linger, true);
                bool linger = (0 != (int)testSockets.socketClient.GetSocketOption(
                    SocketOptionLevel.Socket, SocketOptionName.Linger));

                if (!linger)
                    throw new Exception("Linger was not enabled");

                int lingerTime = 20;

                testSockets.socketClient.SetSocketOption(SocketOptionLevel.Socket,
                    SocketOptionName.Linger, lingerTime);
                int lingerResult = (int)testSockets.socketClient.GetSocketOption(
                    SocketOptionLevel.Socket, SocketOptionName.Linger);

                if (lingerResult != lingerTime)
                    throw new Exception("Linger time was not set correctly");

                // check the bool again for false
                testSockets.socketClient.SetSocketOption(SocketOptionLevel.Socket,
                    SocketOptionName.Linger, false);
                bool notLinger = (0 == (int)testSockets.socketClient.GetSocketOption(
                    SocketOptionLevel.Socket, SocketOptionName.Linger));

                if (!notLinger)
                    throw new Exception("Linger was not disabled");

                // proceed to bind
                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                int cBytes = testSockets.socketClient.Send(testSockets.bufSend);

                using (Socket sock = testSockets.socketServer.Accept())
                {
                    cBytes = sock.Receive(testSockets.bufReceive);
                    testSockets.AssertDataReceived(cBytes);
                }
                testSockets.TearDown();
                testSockets = null;
            }
            catch (Exception e)
            {
                isAnyCatch = true;
                Log.Comment("Exception caught: " + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
            }
            return (!isAnyCatch ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketTest13_DPWSOptions()
        {
            /// <summary>
            /// 1. Starts a server socket listening for UDP
            /// 2. Starts a client socket sending UDP packets
            /// 3. Verifies that data can be correctly sent and recieved
            /// </summary>
            ///

            //don't run on the emulator since this isn't supported
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            bool testResult = true;
            try
            {
                SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);

                NetworkInterface[] inter = NetworkInterface.GetAllNetworkInterfaces();

                IPAddress localAddress = SocketTools.ParseAddress(inter[0].IPAddress);
                try
                {
                    byte[] local = localAddress.GetAddressBytes();

                    byte[] multicastOpt = new byte[] {    224,      100,        1,        1,
                                                          local[0], local[1], local[2], local[3]};

                    testSockets.socketClient.SetSocketOption(SocketOptionLevel.IP,
                        SocketOptionName.AddMembership, multicastOpt);
                }
                catch (SocketException se)
                {
                    if (se.ErrorCode != (int)SocketError.ProtocolOption)
                    {
                        Log.Comment("Caught exception: " + se.Message);
                        if (se.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                            Log.Comment("ErrorCode: " + se.ErrorCode);
                        testResult = false;
                    }
                }
                catch (Exception e)
                {
                    Log.Comment("Caught exception: " + e.Message);
                    if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                        Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                    testResult = false;
                }
                try
                {
                    testSockets.socketClient.SetSocketOption(SocketOptionLevel.Socket,
                        SocketOptionName.ReuseAddress, true);
                }
                catch (SocketException se)
                {
                    if (se.ErrorCode != (int)SocketError.ProtocolOption)
                    {
                        Log.Comment("Caught exception: " + se.Message);
                        if (se.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                            Log.Comment("ErrorCode: " + se.ErrorCode);
                        testResult = false;
                    }
                }
                catch (Exception e)
                {
                    Log.Comment("Caught exception: " + e.Message);
                    if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                        Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                    testResult = false;
                }
                try
                {
                    byte [] bytes = localAddress.GetAddressBytes();
                    // given the address change it to what SetSocketOption needs
                    testSockets.socketClient.SetSocketOption(
                        SocketOptionLevel.IP,
                        SocketOptionName.MulticastInterface,
                        bytes);
                }
                catch (SocketException se)
                {
                    if (se.ErrorCode != (int)SocketError.ProtocolOption)
                    {
                        Log.Comment("Caught exception: " + se.Message);
                        if (se.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                            Log.Comment("ErrorCode: " + se.ErrorCode);
                        testResult = false;
                    }
                }
                catch (Exception e)
                {
                    Log.Comment("Caught exception: " + e.Message);
                    if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                        Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                    testResult = false;
                }
                try
                {
                    testSockets.socketClient.SetSocketOption(SocketOptionLevel.IP,
                        SocketOptionName.DontFragment, true);
                }
                catch (SocketException se)
                {
                    if (se.ErrorCode != (int)SocketError.ProtocolOption)
                    {
                        Log.Comment("Caught exception: " + se.Message);
                        if (se.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                            Log.Comment("ErrorCode: " + se.ErrorCode);
                        testResult = false;
                    }
                }
                catch (Exception e)
                {
                    Log.Comment("Caught exception: " + e.Message);
                    if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                        Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                    testResult = false;
                }
                try
                {
                    testSockets.socketClient.SetSocketOption(SocketOptionLevel.Socket,
                        SocketOptionName.Broadcast, false);
                }
                catch (SocketException se)
                {
                    if (se.ErrorCode != (int)SocketError.ProtocolOption)
                    {
                        Log.Comment("Caught exception: " + se.Message);
                        if (se.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                            Log.Comment("ErrorCode: " + se.ErrorCode);
                        testResult = false;
                    }
                }
                catch (Exception e)
                {
                    Log.Comment("Caught exception: " + e.Message);
                    if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                        Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                    testResult = false;
                }
                try
                {
                    testSockets.socketClient.SetSocketOption(SocketOptionLevel.Udp,
                        SocketOptionName.ExclusiveAddressUse, true);
                }
                catch (SocketException se)
                {
                    if (se.ErrorCode != (int)SocketError.ProtocolOption)
                    {
                        Log.Comment("Caught exception: " + se.Message);
                        if (se.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                            Log.Comment("ErrorCode: " + se.ErrorCode);
                        testResult = false;
                    }
                }
                catch (Exception e)
                {
                    Log.Comment("Caught exception: " + e.Message);
                    if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                        Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                    testResult = false;
                }
                try
                {
                    int error = (int)testSockets.socketClient.GetSocketOption(
                        SocketOptionLevel.Socket, SocketOptionName.Error);
                    Log.Comment("Current error: " + error.ToString());
                }
                catch (SocketException se)
                {
                    if (se.ErrorCode != (int)SocketError.ProtocolOption)
                    {
                        Log.Comment("Caught exception: " + se.Message);
                        if (se.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                            Log.Comment("ErrorCode: " + se.ErrorCode);
                        testResult = false;
                    }
                }
                catch (Exception e)
                {
                    Log.Comment("Caught exception: " + e.Message);
                    if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                        Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                    testResult = false;
                }
                try
                {
                    SocketType type = (SocketType)(
                        (int)testSockets.socketClient.GetSocketOption(SocketOptionLevel.Socket,
                        SocketOptionName.Type));
                    Log.Comment("Current error: " + type.ToString());
                }
                catch (SocketException se)
                {
                    if (se.ErrorCode != (int)SocketError.ProtocolOption)
                    {
                        Log.Comment("Caught exception: " + se.Message);
                        if (se.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                            Log.Comment("ErrorCode: " + se.ErrorCode);
                        testResult = false;
                    }
                }
                catch (Exception e)
                {
                    Log.Comment("Caught exception: " + e.Message);
                    if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                        Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                    testResult = false;
                }
                try
                {
                    int error =
                        (int)testSockets.socketClient.GetSocketOption(SocketOptionLevel.Socket,
                        SocketOptionName.Error);
                    Log.Comment("Current error: " + error.ToString());
                }
                catch (SocketException se)
                {
                    if (se.ErrorCode != (int)SocketError.ProtocolOption)
                    {
                        Log.Comment("Caught exception: " + se.Message);
                        if (se.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                            Log.Comment("ErrorCode: " + se.ErrorCode);
                        testResult = false;
                    }
                }
                catch (Exception e)
                {
                    Log.Comment("Caught exception: " + e.Message);
                    if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                        Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                    testResult = false;
                }
                try
                {
                    SocketType type = (SocketType)(
                        (int)testSockets.socketClient.GetSocketOption(SocketOptionLevel.Socket,
                        SocketOptionName.Type));
                    Log.Comment("Current error: " + type.ToString());
                }
                catch (SocketException se)
                {
                    if (se.ErrorCode != (int)SocketError.ProtocolOption)
                    {
                        Log.Comment("Caught exception: " + se.Message);
                        if (se.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                            Log.Comment("ErrorCode: " + se.ErrorCode);
                        testResult = false;
                    }
                }
                catch (Exception e)
                {
                    Log.Comment("Caught exception: " + e.Message);
                    if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                        Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                    testResult = false;
                }

                try
                {
                    byte[] local = localAddress.GetAddressBytes();

                    byte[] multicastOpt = new byte[] {     224,      100,        1,        1,
                                                          local[0], local[1], local[2], local[3]};

                    testSockets.socketClient.SetSocketOption(SocketOptionLevel.IP,
                        SocketOptionName.DropMembership, multicastOpt);
                }
                catch (SocketException se)
                {
                    if (se.ErrorCode != (int)SocketError.ProtocolOption)
                    {
                        Log.Comment("Caught exception: " + se.Message);
                        if (se.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                            Log.Comment("ErrorCode: " + se.ErrorCode);
                        testResult = false;
                    }
                }
                catch (Exception e)
                {
                    Log.Comment("Caught exception: " + e.Message);
                    if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                        Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                    testResult = false;
                }
                try
                {
                    testSockets.socketClient.SetSocketOption(SocketOptionLevel.Socket,
                        SocketOptionName.ReuseAddress, true);
                }
                catch (SocketException se)
                {
                    if (se.ErrorCode != (int)SocketError.ProtocolOption)
                    {
                        Log.Comment("Caught exception: " + se.Message);
                        if (se.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                            Log.Comment("ErrorCode: " + se.ErrorCode);
                        testResult = false;
                    }
                }
                catch (Exception e)
                {
                    Log.Comment("Caught exception: " + e.Message);
                    if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                        Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                    testResult = false;
                }
                try
                {
                    testSockets.socketClient.SetSocketOption(SocketOptionLevel.Socket,
                        SocketOptionName.ReceiveBuffer, 0x1024);
                }
                catch (SocketException se)
                {
                    if (se.ErrorCode != (int)SocketError.ProtocolOption)
                    {
                        Log.Comment("Caught exception: " + se.Message);
                        if (se.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                            Log.Comment("ErrorCode: " + se.ErrorCode);
                        testResult = false;
                    }
                }
                catch (Exception e)
                {
                    Log.Comment("Caught exception: " + e.Message);
                    if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                        Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                    testResult = false;
                }

                Log.Comment("Testing with port 0");
                testSockets.Startup(0, 0);

                int cBytes = testSockets.socketClient.SendTo(testSockets.bufSend,
                    testSockets.epServer);

                if (cBytes != testSockets.bufSend.Length)
                    throw new Exception("Send failed, wrong length");


                EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
                cBytes = testSockets.socketServer.ReceiveFrom(
                    testSockets.bufReceive, ref epFrom);

                if (testSockets.epClient.Address.ToString() !=
                    ((IPEndPoint)epFrom).Address.ToString())
                    throw new Exception("Bad address");

                testSockets.TearDown();
                testSockets = null;
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception: " + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
    }
}
