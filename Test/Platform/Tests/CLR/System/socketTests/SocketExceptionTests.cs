/*---------------------------------------------------------------------
* SocketExceptionsTests.cs - file description
* Version: 1.0
* Author: REDMOND\a-grchat
* Created: 9/12/2007 4:51:10 PM 
* ---------------------------------------------------------------------*/

using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;

namespace Microsoft.SPOT.Platform.Tests
{
    public class SocketExceptionTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            /// <summary>
            /// This file tests all of the SocketExceptions except:
            /// 
            /// The following ErrorCodes are not supported by the MF due to 
            /// the lack of non-blocking (Begin-End)Send and (Begin-End)Receive methods: 
            /// AlreadyInProgress, ConnectionAborted, Disconnecting
            ///
            /// The following ErrorCodes are not tested because they represent occurances 
            /// that the TestHarness cannot duplicate:
            /// NetworkDown, NetworkUnreachable, NetworkReset, ConnectionReset , HostDown
            /// ProcessLimit, SystemNotReady, VersionNotSupported, TryAgain, NoRecovery, NoData 
            ///
            /// The following ErrorCodes are not supported by the MF for other reasons:
            /// NotSocket, TypeNotFound -- VS returns typing errors instead
            /// DestinationAddressRequired -- VS throws AddressNotAvailable instead
            /// Shutdown -- MF does not implement Shutdown()
            /// NotInitialized -- C# doesn't use WSAStartup
            /// </summary>
            //Wait for GC
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

            Log.Comment("The following tests are located in SocketExceptionTests.cs");
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");

        }

        [TestMethod]
        public MFTestResults SocketExceptionTest2_AddressAlreadyInUse()
        {
            /// <summary>
            /// 1. Causes an AddressAlreadyInUse error
            /// </summary>
            ///
            bool isCorrectCatch = false;
            bool isAnyCatch = false;
            try
            {
                Socket socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Socket socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {

                    socketClient.Bind(new IPEndPoint(IPAddress.Loopback, 10));
                    socketServer.Bind(new IPEndPoint(IPAddress.Loopback, 10));
                }
                catch (SocketException e)
                {
                    if (e.ErrorCode.ToString() != SocketError.AddressAlreadyInUse.ToString())
                        throw new System.Exception("Incorrect ErrorCode in SocketException "
                            + e.ErrorCode, e);
                    isCorrectCatch = true;
                    isAnyCatch = true;
                }
                finally
                {
                    socketClient.Close();
                    socketServer.Close();
                }
            }
            catch (System.Exception e)
            {
                isAnyCatch = true;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }

            if (!isAnyCatch)
            {
                Log.Comment("No exception caught");
            }

            return (isCorrectCatch ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketExceptionTest3_Protocol_Address_FamilyNotSupported()
        {
            /// <summary>
            /// 1. Causes an Address or ProtocolFamilyNotSupported error
            /// According to MSDN these Exceptions are "interchangeable in most cases"
            /// </summary>
            ///
            bool isAnyCatch = false;
            
            try
            {
                try
                {
                    Socket socketTest = new Socket(AddressFamily.AppleTalk, 
                        SocketType.Stream, ProtocolType.Udp);
                }
                catch (SocketException e)
                {
                    if (e.ErrorCode != (int)SocketError.ProtocolFamilyNotSupported  && 
                        e.ErrorCode != (int)SocketError.AddressFamilyNotSupported)
                        throw new System.Exception("Incorrect ErrorCode in SocketException "
                            + e.ErrorCode, e);
                    isAnyCatch = true;
                }
            }
            catch (System.Exception e)
            {
                isAnyCatch = true;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }

            if (!isAnyCatch)
            {
                Log.Comment("No exception caught");
            }

            return (isAnyCatch ? MFTestResults.Pass : MFTestResults.Fail);
        }


        [TestMethod]
        public MFTestResults SocketExceptionTest4_ProtocolNotSupported()
        {
            /// <summary>
            /// 1. Causes a ProtocolNotSupported error
            /// This test currently fails see 17577
            /// </summary>
            ///
            bool isAnyCatch = false;

            try
            {
                try
                {
                    Socket socketTest = new Socket(AddressFamily.InterNetwork, 
                        SocketType.Stream, ProtocolType.Udp);
                }
                catch (SocketException e)
                {
                    if (e.ErrorCode != (int)SocketError.ProtocolNotSupported)
                        throw new System.Exception("Incorrect ErrorCode in SocketException "
                            + e.ErrorCode, e);
                    isAnyCatch = true;
                }
            }
            catch (System.Exception e)
            {
                isAnyCatch = true;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            if (!isAnyCatch)
            {
                Log.Comment("No exception caught");
            }
            
            return (isAnyCatch ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketExceptionTest6_IsConnected()
        {
            /// <summary>
            /// 1. Causes a IsConnected error
            /// </summary>
            ///
            bool isCorrectCatch = false;
            bool isAnyCatch = false;
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            try
            {
                try
                {
                    testSockets.Startup(0, 0);
                    testSockets.socketServer.Listen(1);
                    testSockets.socketClient.Connect(testSockets.epServer);
                    testSockets.socketClient.Connect(testSockets.epServer);
                }
                catch (SocketException)
                {
                    isCorrectCatch = true;
                    isAnyCatch = true;
                }
            }
            catch (System.Exception e)
            {
                isAnyCatch = true;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            finally
            {
                testSockets.TearDown();
            }
            if (!isAnyCatch)
            {
                Log.Comment("No exception caught");
            }

            return (isCorrectCatch ? MFTestResults.Pass : MFTestResults.Fail);
        }

 
        [TestMethod]
        public MFTestResults SocketExceptionTest11_AccessDenied()
        {
            /// <summary>
            /// 1. Causes a AccessDenied error
            /// </summary>
            ///
            bool isCorrectCatch = false;
            bool isAnyCatch = false;
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            try
            {
                try
                {
                    int clientPort = SocketTools.nextPort;
                    int serverPort = SocketTools.nextPort;
                    int tempPort = serverPort;
                    testSockets.socketClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, false);
                    testSockets.Startup(clientPort, serverPort);
                    IPEndPoint epBroadcast = new IPEndPoint(SocketTools.DottedDecimalToIp((byte)255, (byte)255, (byte)255, (byte)255), tempPort);
                    EndPoint serverEndPoint = epBroadcast.Create(epBroadcast.Serialize());
                    testSockets.socketClient.SendTo(testSockets.bufSend, serverEndPoint);
                }
                catch (SocketException)
                {
                    isCorrectCatch = true;
                    isAnyCatch = true;
                }
            }
            catch (System.Exception e)
            {
                isAnyCatch = true;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            finally
            {
                testSockets.TearDown();
            }
            if (!isAnyCatch)
            {
                Log.Comment("No exception caught");
            }

            return (isCorrectCatch ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketExceptionTest12_NotConnected()
        {
            /// <summary>
            /// 1. Causes a NotConnected error
            /// </summary>
            ///
            bool isCorrectCatch = false;
            bool isAnyCatch = false;
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            try
            {
                try
                {
                    testSockets.Startup(0, 0);
                    Socket socketTemp = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp);
                    socketTemp.Bind(testSockets.socketServer.RemoteEndPoint);
                    socketTemp.Send(new byte[2]);
                }
                catch (SocketException)
                {
                    isCorrectCatch = true;
                    isAnyCatch = true;
                }
            }
            catch (System.Exception e)
            {
                isAnyCatch = true;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            finally
            {
                testSockets.TearDown();
            }
            if (!isAnyCatch)
            {
                Log.Comment("No exception caught");
            }
            return (isCorrectCatch ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketExceptionTest13_InvalidArgument()
        {
            /// <summary>
            /// 1. Causes a InvalidArgument error
            /// </summary>
            ///
            bool isCorrectCatch = false;
            bool isAnyCatch = false;
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            try
            {
                try
                {
                    int clientPort = SocketTools.nextPort;
                    int serverPort = SocketTools.nextPort;
                    int tempPort = clientPort;
                    testSockets.Startup(clientPort, serverPort);
                    testSockets.socketServer.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.Broadcast, true);
                }
                catch (SocketException)
                {
                    isCorrectCatch = true;
                    isAnyCatch = true;
                }
            }
            catch (System.Exception e)
            {
                isAnyCatch = true;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            finally
            {
                testSockets.TearDown();
            }
            if (!isAnyCatch)
            {
                Log.Comment("No exception caught");
            }
            return (isCorrectCatch ? MFTestResults.Pass : MFTestResults.Fail);
        }


        [TestMethod]
        public MFTestResults SocketExceptionTest14_AddressNotAvailable()
        {
            /// <summary>
            /// 1. Causes a AddressNotAvailable error
            /// Due to loopback this method causes an InvalidArgument
            /// SocketException erroneously
            /// </summary>
            ///
            bool isCorrectCatch = false;
            bool isAnyCatch = false;
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            try
            {
                try
                {
                    int clientPort = SocketTools.nextPort;
                    int serverPort = SocketTools.nextPort;
                    int tempPort = clientPort;
                    testSockets.Startup(clientPort, serverPort);

                    testSockets.socketClient.Bind(new IPEndPoint(new IPAddress(SocketTools.DottedDecimalToIp((byte)192, (byte)168, (byte)192, (byte)168)), tempPort));
                }
                catch (SocketException)
                {
                    isCorrectCatch = true;
                    isAnyCatch = true;
                }
            }
            catch (System.Exception e)
            {
                isAnyCatch = true;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            finally
            {
                testSockets.TearDown();
            }
            if (!isAnyCatch)
            {
                Log.Comment("No exception caught");
            }
            return (isCorrectCatch ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketExceptionTest16_HostNotFound()
        {
            /// <summary>
            /// 1. Causes a HostNotFound error
            /// </summary>
            ///
            bool isCorrectCatch = false;
            bool isAnyCatch = false;

            try
            {
                try
                {
                    IPHostEntry ipHostEntry = Dns.GetHostEntry("fakeHostName");
                }
                catch (SocketException)
                {
                    isCorrectCatch = true;
                    isAnyCatch = true;
                }
            }
            catch (System.Exception e)
            {
                isAnyCatch = true;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            if (!isAnyCatch)
            {
                Log.Comment("No exception caught");
            }

            return (isCorrectCatch ? MFTestResults.Pass : MFTestResults.Fail);
        }



        [TestMethod]
        public MFTestResults SocketExceptionTest17_SocketError()
        {
            /// <summary>
            /// 1. Causes a SocketError error
            /// This currently succeeds but will need re-writing if 17577 is addressed
            /// </summary>
            ///
            bool isCorrectCatch = false;
            try
            {
                SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Stream);
            }
            catch (SocketException)
            {
                    isCorrectCatch = true;
            }

            return (isCorrectCatch ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketExceptionTest18_Fault()
        {
            /// <summary>
            /// 1. Causes a Fault error
            /// </summary>
            ///
            bool isCorrectCatch = false;
            bool isAnyCatch = false;
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            try
            {
                try
                {
                    testSockets.socketClient.SetSocketOption(SocketOptionLevel.Socket,
                        SocketOptionName.Linger, new byte[] { (byte)0 });
                    testSockets.Startup(0, 0);
                }
                catch (SocketException)
                {
                    isCorrectCatch = true;
                    isAnyCatch = true;
                }
            }
            catch (System.Exception e)
            {
                isAnyCatch = true;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            finally
            {
                testSockets.TearDown();
            }
            if (!isAnyCatch)
            {
                Log.Comment("No exception caught");
            }
            return (isCorrectCatch ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketExceptionTest19_ProtocolOption()
        {
            /// <summary>
            /// 1. Causes a ProtocolOption error
            /// </summary>
            ///
            bool isCorrectCatch = false;
            bool isAnyCatch = false;
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            try
            {
                try
                {
                    testSockets.Startup(0, 0);
                    testSockets.socketClient.GetSocketOption(SocketOptionLevel.IP,
                        SocketOptionName.Linger);
                }
                catch (SocketException)
                {
                    isCorrectCatch = true;
                    isAnyCatch = true;
                }
            }
            catch (System.Exception e)
            {
                isAnyCatch = true;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            if (!isAnyCatch)
            {
                Log.Comment("No exception caught");
            }
            testSockets.TearDown();
            testSockets = null;
            return (isCorrectCatch ? MFTestResults.Pass : MFTestResults.Fail);
        }


        [TestMethod]
        public MFTestResults SocketExceptionTest20_OperationNotSupported()
        {
            /// <summary>
            /// 1. Causes a OperationNotSupported error
            /// </summary>
            ///
            bool isCorrectCatch = false;
            bool isAnyCatch = false;
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            try
            {
                try
                {
                    testSockets.Startup(0, 0);

                    testSockets.socketServer.Listen(1);
                    testSockets.socketClient.Connect(testSockets.epServer);
                    testSockets.socketClient.Send(testSockets.bufSend);

                    using (Socket sock = testSockets.socketServer.Accept())
                    {
                        sock.Receive(testSockets.bufReceive, SocketFlags.DontRoute);
                    }

                    isCorrectCatch = true;
                }
                catch (SocketException)
                {
                    isCorrectCatch = true;
                    isAnyCatch = true;
                }
            }
            catch (System.Exception e)
            {
                isAnyCatch = true;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            if (!isAnyCatch)
            {
                Log.Comment("No exception caught");
            }
            testSockets.TearDown();
            testSockets = null;
            return (isCorrectCatch ? MFTestResults.Pass : MFTestResults.Fail);
        }
    }
}
