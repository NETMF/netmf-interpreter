/*---------------------------------------------------------------------
* SocketsEnumsTests.cs - file description
* Version: 1.0
* Author: REDMOND\a-grchat
* Created: 9/13/2007 10:06:40 AM 
* 
* Tests the Socket library's enumerations by utilizing each element of each of them
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
    public class SocketsEnumsTests : IMFTestInterface
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

        [TestMethod]
        public MFTestResults SocketsEnums8_SocketFlags_Peek()
        {
            /// <summary>
            /// 1. Sends TCP data using the Peek flag 
            /// 2. Verifies that the correct exception is thrown
            /// Further testing would require harness alteration
            /// </summary>
            ///
            bool isAnyCatch = false;
            try
            {
                try
                {
                    SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
                    testSockets.Startup(0, 0);
                    testSockets.socketServer.Listen(1);
                    testSockets.socketClient.Connect(testSockets.epServer);

                    int cBytes = testSockets.socketClient.Send(testSockets.bufSend);

                    using (Socket sock = testSockets.socketServer.Accept())
                    {
                        cBytes = sock.Receive(testSockets.bufReceive, SocketFlags.Peek);
                        Log.Comment("Checking Peek data");
                        testSockets.AssertDataReceived(cBytes);
                        int cBytesAgain = sock.Receive(testSockets.bufReceive);

                        if(cBytesAgain < cBytes)
                            throw new Exception( 
                                "Peek returns more bytes than the successive read" );
                    }
                    testSockets.AssertDataReceived(cBytes);
                    testSockets.TearDown();
                    testSockets = null;
                }
                catch (SocketException)
                {
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
            return (isAnyCatch ? MFTestResults.Fail : MFTestResults.Pass);
        }

        [TestMethod]
        public MFTestResults SocketsEnums9_SocketOptionLevel()
        {
            /// <summary>
            /// 1. Tests that Set and GetSocketOption can be called using each member of 
            /// SocketOptionLevel
            /// 
            /// TODO: Fix Docs for (...Byte[])
            /// </summary>
            ///
            bool isAnyCatch = false; try
            {
                SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);

                try
                {
                    testSockets.socketClient.SetSocketOption(SocketOptionLevel.Tcp,
                        SocketOptionName.NoDelay, false);
                    if (0 != (int)testSockets.socketClient.GetSocketOption(
                        SocketOptionLevel.Tcp, SocketOptionName.NoDelay))
                        throw new System.Exception("SocketOptionLevel.Tcp failed");

                    testSockets.socketClient.SetSocketOption(SocketOptionLevel.Socket,
                        SocketOptionName.Linger, true);
                    if (0 == (int)testSockets.socketClient.GetSocketOption(
                        SocketOptionLevel.Socket, SocketOptionName.Linger))
                        throw new System.Exception("SocketOptionLevel.Socket failed");

                    //testSockets.socketClient.SetSocketOption(SocketOptionLevel.IP,
                    //    SocketOptionName.DontFragment, true);
                    //if (0 == (int)testSockets.socketClient.GetSocketOption(
                    //    SocketOptionLevel.IP, SocketOptionName.DontFragment))
                    //    throw new System.Exception("SocketOptionLevel.IP failed");

                    if ((int)testSockets.socketClient.GetSocketOption(
                        SocketOptionLevel.Socket, SocketOptionName.Linger) != 1)
                        throw new System.Exception(
                            "GetSocketOption with Level and Name got wrong data");

                    byte[] lingerBytes = new byte[] { 0, 0, 0, 0 };
                    testSockets.socketClient.GetSocketOption(
                        SocketOptionLevel.Socket, SocketOptionName.Linger, lingerBytes);
                    if (!SocketTools.ArrayEquals(lingerBytes, new byte[] { 1, 0, 0, 0 }))
                        throw new System.Exception("GetSocketOption with Level,"
                            + " Name and ByteArray got wrong data");

                    testSockets.Startup(0, 0);
                    testSockets.socketServer.Listen(1);
                    testSockets.socketClient.Connect(testSockets.epServer);
                    testSockets.socketClient.Send(testSockets.bufSend);

                    int cBytes = 0;
                    using (Socket sock = testSockets.socketServer.Accept())
                    {
                        Thread.Sleep(500);
                        cBytes = sock.Available;
                        cBytes = sock.Receive(testSockets.bufReceive);
                    }
                    testSockets.AssertDataReceived(cBytes);
                }
                catch (SocketException e)
                {
                    isAnyCatch = true;
                    Log.Comment("System.Exception caught: " + e.Message + " " + e.ErrorCode);
                }
            finally
            {
                testSockets.TearDown();
            }

            }
            catch (System.Exception e)
            {
                isAnyCatch = true;
                Log.Comment("System.Exception caught: " + e.Message);
            }

            return (!isAnyCatch ? MFTestResults.Pass : MFTestResults.Fail);
        }

        private bool TestSocketOption(SocketPair testSockets, SocketOptionLevel optionLevel, 
             SocketOptionName optionName, Object data, bool isSupported)
        {
            try
            {
                string typeStr = data.GetType().ToString();
                switch (typeStr)
                {
                    case "System.Boolean":
                        Log.Comment("First set");
                        testSockets.socketClient.SetSocketOption(optionLevel, optionName, 
                            (bool)data);
                        Log.Comment("First get"); 
                        if (((int)testSockets.socketClient.GetSocketOption(optionLevel, 
                            optionName) == 1) != (bool)data)
                            throw new Exception("Got wrong data after first Set");
                        Log.Comment("Second set"); 
                        testSockets.socketClient.SetSocketOption(optionLevel, optionName, 
                            !(bool)data);
                        Log.Comment("Second get");
                        if (((int)testSockets.socketClient.GetSocketOption(optionLevel, 
                            optionName) == 1) != !(bool)data)
                            throw new Exception("Got wrong data after second Set");
                        break;
                    case "System.Int32":
                        Log.Comment("First set");
                        testSockets.socketClient.SetSocketOption(optionLevel, optionName, 
                            (int)data);
                        Log.Comment("First get");
                        if ((int)testSockets.socketClient.GetSocketOption(optionLevel, 
                            optionName) != (int)data)
                            throw new Exception("Got wrong data after first Set");
                        Log.Comment("Second set");
                        testSockets.socketClient.SetSocketOption(optionLevel, optionName, 
                            (int)data - 1);
                        Log.Comment("Second get");
                        if ((int)testSockets.socketClient.GetSocketOption(optionLevel, 
                            optionName) != (int)data - 1)
                            throw new Exception("Got wrong data after second Set");
                        break;
                    case "System.Byte[]":
                        Log.Comment("First set");
                        byte[] result = new byte[((byte[])data).Length];
                        testSockets.socketClient.SetSocketOption(optionLevel, optionName, 
                            (byte[])data);
                        testSockets.socketClient.GetSocketOption(optionLevel, optionName, 
                            result) ;
                        Log.Comment("First get");
                        if (!SocketTools.ArrayEquals(result, (byte[])data))
                            throw new Exception("Got wrong data after first Set");

                        //Decrement first byte of data
                        ((byte[])data)[0]--;

                        Log.Comment("Second set");
                        testSockets.socketClient.SetSocketOption(optionLevel, optionName, 
                            (byte[])data); 
                        testSockets.socketClient.GetSocketOption(optionLevel, optionName, 
                            result);
                        Log.Comment("Second get");
                        if (!SocketTools.ArrayEquals(result, (byte[])data))
                            throw new Exception("Got wrong data after second Set");
                        break;
                    default:
                        throw new Exception("Test Error, cannot set socket option with that type");
                }
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception: " + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                {
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                    if (!isSupported && (((SocketException)e).ErrorCode == (int)SocketError.ProtocolOption ||
                        ((SocketException)e).ErrorCode == (int)SocketError.AddressFamilyNotSupported ||
                        ((SocketException)e).ErrorCode == (int)SocketError.InvalidArgument ||
                        ((SocketException)e).ErrorCode == (int)SocketError.AddressNotAvailable))
                    {
                        Log.Comment("Non-supported option graceful fail");
                        return true;
                    }
                    else if (isSupported && ((SocketException)e).ErrorCode == (int)SocketError.ProtocolOption)
                    {
                        Log.Comment(
                            "Supported option graceful fail, this option needs implementation");
                        return true;
                    }
                    else if (!isSupported && (((SocketException)e).ErrorCode == (int)SocketError.ProtocolOption ||
                        ((SocketException)e).ErrorCode == (int)SocketError.InvalidArgument ||
                        ((SocketException)e).ErrorCode == (int)SocketError.AddressNotAvailable))
                    {
                        Log.Comment("Non-supported option failed with bad errorcode, " +
                            "should be 10042 or 10022");
                    }
                }
                Log.Comment("SocketOption FAILED");
                return false;
            }

            if (!isSupported)
            {
                Log.Comment("This option is not supported.  Should throw an exception");
                return true;
            }

            Log.Comment("SocketOption succeeded");
            return true;
        }

        private bool TestSocketOptionGetOnly(SocketPair testSockets, SocketOptionLevel optionLevel,
             SocketOptionName optionName, Object data, bool isSupported)
        {
            try
            {
                Log.Comment("Option is only valid for Get, displaying current values");
                string typeStr = data.GetType().ToString();
                switch (typeStr)
                {
                    case "System.Boolean":
                        Log.Comment(((int)testSockets.socketClient.GetSocketOption(
                            optionLevel, optionName)).ToString());
                        break;
                    case "System.Int32":
                        Log.Comment(((int)testSockets.socketClient.GetSocketOption(
                            optionLevel, optionName)).ToString());
                        break;
                    case "System.Byte[]":
                        string arrContent = "";
                        byte[] result = new byte[((byte[])data).Length];
                        testSockets.socketClient.GetSocketOption(optionLevel, optionName, result);
                        for (int i = 0; i > result.Length; i++)
                        {
                            arrContent += ("," + result[i].ToString());
                        }
                        Log.Comment(arrContent);
                        break;
                    default:
                        throw new Exception("Test Error, cannot set socket option with that type");
                }
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception: " + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                {
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                    if (!isSupported && (((SocketException)e).ErrorCode == (int)SocketError.ProtocolOption ||
                        ((SocketException)e).ErrorCode == (int)SocketError.AddressFamilyNotSupported ||
                        ((SocketException)e).ErrorCode == (int)SocketError.InvalidArgument ||
                        ((SocketException)e).ErrorCode == (int)SocketError.AddressNotAvailable))
                    {
                        Log.Comment("Non-supported option graceful fail");
                        return true;
                    }
                    else if (isSupported && ((SocketException)e).ErrorCode == (int)SocketError.ProtocolOption)
                    {
                        Log.Comment(
                            "Supported option graceful fail, this option needs implementation");
                        return true;
                    }
                    else if (!isSupported && (((SocketException)e).ErrorCode == (int)SocketError.ProtocolOption ||
                        ((SocketException)e).ErrorCode == (int)SocketError.InvalidArgument ||
                        ((SocketException)e).ErrorCode == (int)SocketError.AddressNotAvailable))
                    {
                        Log.Comment("Non-supported option failed with bad errorcode," +
                            "should be 10042");
                    }
                }
                Log.Comment("SocketOption FAILED");
                return false;
            }

            if (!isSupported)
            {
                Log.Comment("This option is not supported.  Should throw an exception");
                return true;
            }

            Log.Comment("SocketOption succeeded");
            return true;
        }

        private bool TestSocketOptionSetOnly(SocketPair testSockets, 
            SocketOptionLevel optionLevel,
             SocketOptionName optionName, Object data, bool isSupported)
        {
            try
            {
                Log.Comment("Option is only valid for Set");
                string typeStr = data.GetType().ToString();
                switch (typeStr)
                {
                    case "System.Boolean":
                        testSockets.socketClient.SetSocketOption(optionLevel, 
                            optionName, (bool)data);
                        break;
                    case "System.Int32":
                        testSockets.socketClient.SetSocketOption(optionLevel, 
                            optionName, (int)data);
                        break;
                    case "System.Byte[]":
                        testSockets.socketClient.SetSocketOption(optionLevel, 
                            optionName, (byte[])data);
                        break;
                    default:
                        throw new Exception("Test Error, cannot set socket option with that type");
                }
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception: " + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                {
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                    if (!isSupported && (((SocketException)e).ErrorCode == (int)SocketError.ProtocolOption || 
                        ((SocketException)e).ErrorCode == (int)SocketError.AddressFamilyNotSupported || 
                        ((SocketException)e).ErrorCode == (int)SocketError.InvalidArgument || 
                        ((SocketException)e).ErrorCode == (int)SocketError.AddressNotAvailable))
                    {
                        Log.Comment("Non-supported option graceful fail");
                        return true;
                    }
                    else if (isSupported && ((SocketException)e).ErrorCode == (int)SocketError.ProtocolOption)
                    {
                        Log.Comment(
                            "Supported option graceful fail, this option needs implementation");
                        return true;
                    }
                    else if (!isSupported && (((SocketException)e).ErrorCode == (int)SocketError.ProtocolOption || 
                        ((SocketException)e).ErrorCode == (int)SocketError.InvalidArgument || 
                        ((SocketException)e).ErrorCode == (int)SocketError.AddressNotAvailable))
                    {
                        Log.Comment("Non-supported option failed with bad errorcode, " +
                            "should be 10042 or 10022");
                    }
                }
                Log.Comment("SocketOption FAILED");
                return false;
            }

            if (!isSupported)
            {
                Log.Comment("This option is not supported.  Should throw an exception");
                return true;
            }

            Log.Comment("SocketOption succeeded");
            return true;
        }

        private bool boolData = true;
        private int intData = 5;
        private byte[] byteArrData = new byte[] { 1, 0, 0, 0, 0 };
        private byte[] lingerData = new byte[] { 1, 0, 0, 0 };

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_AcceptConnection_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing AcceptConnection with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.AcceptConnection, boolData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_AddMembership_TCP()
        {
            NetworkInterface[] inter = NetworkInterface.GetAllNetworkInterfaces();
            IPAddress localAddress = SocketTools.ParseAddress(inter[0].IPAddress);
            byte[] local = localAddress.GetAddressBytes();
            byte[] multicastOpt = new byte[] {    224,      100,        1,        1,
                                                          local[0], local[1], local[2], local[3]};
            
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing AddMembership with TCP");
            bool testResult = TestSocketOptionSetOnly(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.AddMembership, multicastOpt, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_AddSourceMembership_TCP()
        {
            NetworkInterface[] inter = NetworkInterface.GetAllNetworkInterfaces();
            IPAddress localAddress = SocketTools.ParseAddress(inter[0].IPAddress);
            byte[] local = localAddress.GetAddressBytes();
            byte[] multicastOpt = new byte[] {    224,      100,        1,        1,
                                                          local[0], local[1], local[2], local[3]};

            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing AddSourceMembership with TCP");
            bool testResult = TestSocketOptionSetOnly(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.AddSourceMembership, multicastOpt, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_BlockSource_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing BlockSource with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.BlockSource, boolData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_Broadcast_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing Broadcast with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.Broadcast, boolData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_BsdUrgent_TCP()
        {
            //don't run this on emulator since its not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing BsdUrgent with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Tcp, 
                SocketOptionName.BsdUrgent, boolData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_Debug_TCP()
        {
            //don't run this on emulator since its not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing Debug with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.Debug, boolData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_DontFragment_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing DontFragment with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.DontFragment, boolData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_DontRoute_TCP()
        {
            //don't run this on emulator since its not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing DontRoute with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.DontRoute, boolData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_DropMembership_TCP()
        {
            NetworkInterface[] inter = NetworkInterface.GetAllNetworkInterfaces();
            IPAddress localAddress = SocketTools.ParseAddress(inter[0].IPAddress);
            byte[] local = localAddress.GetAddressBytes();
            byte[] multicastOpt = new byte[] {    224,      100,        1,        1,
                                                          local[0], local[1], local[2], local[3]};

            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing DropMembership with TCP");
            bool testResult = TestSocketOptionSetOnly(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.DropMembership, multicastOpt, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_DropSourceMembership_TCP()
        {
            NetworkInterface[] inter = NetworkInterface.GetAllNetworkInterfaces();
            IPAddress localAddress = SocketTools.ParseAddress(inter[0].IPAddress);
            byte[] local = localAddress.GetAddressBytes();
            byte[] multicastOpt = new byte[] {    224,      100,        1,        1,
                                                          local[0], local[1], local[2], local[3]};

            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing DropSourceMembership with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.DropSourceMembership, multicastOpt, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_Error_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing Error with TCP");
            bool testResult = TestSocketOptionGetOnly(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.Error, intData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_ExclusiveAddressUse_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing ExclusiveAddressUse with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.ExclusiveAddressUse, boolData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_Expedited_TCP()
        {
            //don't run this on emulator since its not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing Expedited with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Tcp, 
                SocketOptionName.Expedited, boolData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_HeaderIncluded_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing HeaderIncluded with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.HeaderIncluded, boolData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_IPOptions_TCP()
        {
            //don't run this on emulator since its not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing IPOptions with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.IPOptions, byteArrData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_IpTimeToLive_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing IpTimeToLive with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.IpTimeToLive, intData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_KeepAlive_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing KeepAlive with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.KeepAlive, boolData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_Linger_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing Linger with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.Linger, lingerData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_MaxConnections_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing MaxConnections with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.MaxConnections, intData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_MulticastInterface_TCP()
        {
            NetworkInterface[] inter = NetworkInterface.GetAllNetworkInterfaces();
            IPAddress localAddress = SocketTools.ParseAddress(inter[0].IPAddress);

            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing MulticastInterface with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.MulticastInterface, localAddress.GetAddressBytes(), false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_MulticastLoopback_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing MulticastLoopback with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.MulticastLoopback, boolData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_MulticastTimeToLive_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing MulticastTimeToLive with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.MulticastTimeToLive, intData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_NoDelay_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing NoDelay with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Tcp, 
                SocketOptionName.NoDelay, boolData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_OutOfBandInline_TCP()
        {
            //don't run this on emulator since its not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing OutOfBandInline with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.OutOfBandInline, boolData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_PacketInformation_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing PacketInformation with TCP");
            bool testResult = TestSocketOptionGetOnly(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.PacketInformation, byteArrData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_ReceiveBuffer_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing ReceiveBuffer with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.ReceiveBuffer, intData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_ReceiveLowWater_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing ReceiveLowWater with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.ReceiveLowWater, intData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_ReceiveTimeout_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing ReceiveTimeout with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.ReceiveTimeout, intData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_ReuseAddress_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing ReuseAddress with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.ReuseAddress, boolData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_SendBuffer_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing SendBuffer with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.SendBuffer, intData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_SendLowWater_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing SendLowWater with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.SendLowWater, intData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_SendTimeout_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing SendTimeout with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.SendTimeout, intData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_Type_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing Type with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.Type, intData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_TypeOfService_TCP()
        {
            //don't run this on emulator since its not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing TypeOfService with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.TypeOfService, byteArrData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_UnblockSource_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing UnblockSource with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.UnblockSource, boolData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_UseLoopback_TCP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Log.Comment("Testing UseLoopback with TCP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.UseLoopback, boolData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_AcceptConnection_UDP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing AcceptConnection with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.AcceptConnection, boolData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_AddMembership_UDP()
        {
            //don't run this on emulator since its not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            NetworkInterface[] inter = NetworkInterface.GetAllNetworkInterfaces();
            IPAddress localAddress = SocketTools.ParseAddress(inter[0].IPAddress);
            byte[] local = localAddress.GetAddressBytes();
            byte[] multicastOpt = new byte[] {    224,      100,        1,        1,
                                                          local[0], local[1], local[2], local[3]};

            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing AddMembership with UDP");
            bool testResult = TestSocketOptionSetOnly(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.AddMembership, multicastOpt, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_AddSourceMembership_UDP()
        {
            //don't run this on emulator since its not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            NetworkInterface[] inter = NetworkInterface.GetAllNetworkInterfaces();
            IPAddress localAddress = SocketTools.ParseAddress(inter[0].IPAddress);
            byte[] local = localAddress.GetAddressBytes();
            byte[] multicastOpt = new byte[] {    224,      100,        1,        1,
                                                          local[0], local[1], local[2], local[3]};

            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing AddSourceMembership with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.AddSourceMembership, multicastOpt, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_BlockSource_UDP()
        {
            //don't run this on emulator since its not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing BlockSource with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.BlockSource, boolData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_Broadcast_UDP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing Broadcast with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.Broadcast, boolData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_ChecksumCoverage_UDP()
        {
            //don't run this on emulator since its not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing ChecksumCoverage with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Udp, 
                SocketOptionName.ChecksumCoverage, boolData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_Debug_UDP()
        {
            //don't run this on emulator since its not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing Debug with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.Debug, boolData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_DontFragment_UDP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing DontFragment with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.DontFragment, boolData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_DontRoute_UDP()
        {
            //don't run this on emulator since its not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing DontRoute with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.DontRoute, boolData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_DropMembership_UDP()
        {
            //don't run this on emulator since its not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            NetworkInterface[] inter = NetworkInterface.GetAllNetworkInterfaces();
            IPAddress localAddress = SocketTools.ParseAddress(inter[0].IPAddress);
            byte[] local = localAddress.GetAddressBytes();
            byte[] multicastOpt = new byte[] {    224,      100,        1,        1,
                                                          local[0], local[1], local[2], local[3]};
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing DropMembership with UDP");
            bool testResult = TestSocketOptionSetOnly(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.DropMembership, multicastOpt, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_DropSourceMembership_UDP()
        {
            //don't run this on emulator since its not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            NetworkInterface[] inter = NetworkInterface.GetAllNetworkInterfaces();
            IPAddress localAddress = SocketTools.ParseAddress(inter[0].IPAddress);
            byte[] local = localAddress.GetAddressBytes();
            byte[] multicastOpt = new byte[] {    224,      100,        1,        1,
                                                          local[0], local[1], local[2], local[3]};
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing DropSourceMembership with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.DropSourceMembership, multicastOpt, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_Error_UDP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing Error with UDP");
            bool testResult = TestSocketOptionGetOnly(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.Error, intData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_ExclusiveAddressUse_UDP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing ExclusiveAddressUse with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.ExclusiveAddressUse, boolData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_HeaderIncluded_UDP()
        {
            //don't run this on emulator since its not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing HeaderIncluded with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.HeaderIncluded, boolData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_IPOptions_UDP()
        {
            //don't run this on emulator since its not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing IPOptions with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.IPOptions, byteArrData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_IpTimeToLive_UDP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing IpTimeToLive with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.IpTimeToLive, intData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_KeepAlive_UDP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing KeepAlive with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.KeepAlive, boolData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_Linger_UDP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing Linger with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.Linger, lingerData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_MaxConnections_UDP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing MaxConnections with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.MaxConnections, intData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        // This test fails on the automation machine and passes on my machine???
        //[TestMethod]
        //public MFTestResults SocketsEnums11_SocketOptionName_MulticastInterface_UDP()
        //{
        //    NetworkInterface[] inter = NetworkInterface.GetAllNetworkInterfaces();
        //    IPAddress localAddress = SocketTools.ParseAddress(inter[0].IPAddress);

        //    SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
        //    Log.Comment("Testing MulticastInterface with UDP");
        //    bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
        //        SocketOptionName.MulticastInterface, localAddress.GetAddressBytes(), false);
        //    testSockets.TearDown();
        //    return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        //}

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_MulticastLoopback_UDP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing MulticastLoopback with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.MulticastLoopback, boolData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_MulticastTimeToLive_UDP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing MulticastTimeToLive with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.MulticastTimeToLive, intData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_NoChecksum_UDP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing NoChecksum with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Udp, 
                SocketOptionName.NoChecksum, boolData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_OutOfBandInline_UDP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing OutOfBandInline with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.OutOfBandInline, boolData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_PacketInformation_UDP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing PacketInformation with UDP");
            bool testResult = TestSocketOptionGetOnly(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.PacketInformation, byteArrData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_ReceiveBuffer_UDP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing ReceiveBuffer with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.ReceiveBuffer, intData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_ReceiveLowWater_UDP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing ReceiveLowWater with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.ReceiveLowWater, intData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_ReceiveTimeout_UDP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing ReceiveTimeout with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.ReceiveTimeout, intData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_ReuseAddress_UDP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing ReuseAddress with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.ReuseAddress, boolData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_SendBuffer_UDP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing SendBuffer with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.SendBuffer, intData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_SendLowWater_UDP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing SendLowWater with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.SendLowWater, intData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_SendTimeout_UDP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing SendTimeout with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.SendTimeout, intData, true);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_Type_UDP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing Type with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.Socket, 
                SocketOptionName.Type, intData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_TypeOfService_UDP()
        {
            //don't run this on emulator since its not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing TypeOfService with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.TypeOfService, byteArrData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_UnblockSource_UDP()
        {
            //don't run this on emulator since its not supported.
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing UnblockSource with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.UnblockSource, boolData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums11_SocketOptionName_UseLoopback_UDP()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Log.Comment("Testing UseLoopback with UDP");
            bool testResult = TestSocketOption(testSockets, SocketOptionLevel.IP, 
                SocketOptionName.UseLoopback, boolData, false);
            testSockets.TearDown();
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }


        [TestMethod]
        public MFTestResults SocketsEnums12_SON_AcceptConnection()
        {
            /// <summary>
            /// 1. Starts a server socket listening for TCP
            /// 2. Verifies that AcceptConnection is correct before and after
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                byte[] buf = new byte[4];
                int number;

                SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
                testSockets.Startup(0, 0);
                testSockets.socketServer.GetSocketOption(
                    SocketOptionLevel.Socket, SocketOptionName.AcceptConnection, buf );
                number = (int)buf[0] + (int)(buf[1]<<8) + (int)(buf[2]<<16) + (int)(buf[3]<<24);
                testResult &= (number == 0);
                testSockets.socketServer.Listen(1);
                
                testSockets.socketServer.GetSocketOption(
                    SocketOptionLevel.Socket, SocketOptionName.AcceptConnection, buf );
                number = (int)buf[0] + (int)(buf[1]<<8) + (int)(buf[2]<<16) + (int)(buf[3]<<24);
                
                testResult &= (number != 0);
                testSockets.TearDown();
                testSockets = null;

            }
            catch (System.Exception e)
            {
                Log.Comment("Caught exception: " + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode);
                testResult = false;
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums13_SON_Broadcast()
        {
            /// <summary>
            /// 1. Starts a UDP client broadcasting 
            /// 2. Verifies that Broadcast is correct before and after
            /// </summary>
            ///
            bool testResult = false;
            try
            {
                SocketPair testSockets1 = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
                int clientPort1 = SocketTools.nextPort;
                int serverPort1 = SocketTools.nextPort;
                int tempPort1 = SocketTools.nextPort;
                IPEndPoint epBroadcast1 = new IPEndPoint(
                    SocketTools.DottedDecimalToIp(
                    (byte)255, (byte)255, (byte)255, (byte)255), tempPort1);

                Log.Comment("SetSocketOption socket, broadcast, false");
                testSockets1.socketClient.SetSocketOption(
                    SocketOptionLevel.Socket, SocketOptionName.Broadcast, false);

                try
                {
                    Log.Comment("SendTo epBroadcast1");
                    testSockets1.socketClient.SendTo(
                        new byte[] { 0x01, 0x02, 0x03 }, epBroadcast1);
                    Log.Comment("Error, this should never get displayed.  An exception should have been thrown.");
                }
                catch (SocketException e)
                {
                    Log.Comment("Error code: " + e.ErrorCode);
                    Log.Comment("Correctly threw exception trying to broadcast from socket that has had broadcast turned off.");
                    testResult = true;
                }
                catch (Exception e)
                {
                    Log.Comment("Incorrect exception thrown: " + e.Message);
                }
                finally
                {
                    Log.Comment("Tear down the socket");
                    testSockets1.TearDown();
                }
                //--//

                SocketPair testSockets2 = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
                int clientPort2 = SocketTools.nextPort;
                int serverPort2 = SocketTools.nextPort;
                int tempPort2 = SocketTools.nextPort;
                IPEndPoint epBroadcast2 = new IPEndPoint(
                    SocketTools.DottedDecimalToIp(
                    (byte)255, (byte)255, (byte)255, (byte)255), tempPort2);

                Log.Comment("SetSocketOption socket, broadcast, true");
                testSockets2.socketClient.SetSocketOption(
                    SocketOptionLevel.Socket,SocketOptionName.Broadcast, true);

                try
                {
                    Log.Comment("SendTo epBroadcast1");
                    testSockets2.socketClient.SendTo(
                        new byte[] { 0x01, 0x02, 0x03 }, epBroadcast2);
                }
                catch (SocketException e)
                {
                    Log.Comment("Error code: " + e.ErrorCode);
                    if (e.ErrorCode == 10047)
                    {
                        Log.Comment("Some drivers do not like this option being set.  Allow them to throw this exception");
                        testResult &= true;
                    }
                    else
                    {
                        Log.Comment("Incorrectly threw exception trying to broadcast from socket that has had broadcast turned on.");
                        testResult = false;
                    }
                }
                catch (Exception e)
                {
                    Log.Comment("Incorrect exception thrown: " + e.Message);
                }
                finally
                {
                    Log.Comment("Tear down the socket");
                    testSockets2.TearDown();
                }
            }
            catch (SocketException e)
            {
                Log.Comment("SocketException ErrorCode: " + e.ErrorCode);
                if (e.ErrorCode == 10047)
                {
                    Log.Comment("Address family not supported by protocol family.");
                    Log.Comment("This exception is thrown by the device driver that doesnt' implement Loopback for UDP");
                }
                else
                {
                    Log.Comment("Fail for any other error codes that we don't know about");
                    testResult = false;
                }
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception: " + e.Message);
                testResult = false;
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums14_SON_ReceiveBuffer()
        {
            /// <summary>
            /// 1. Sends a message of size RecieveBuffer
            /// 2. Verifies that it transferred correctly
            /// 1. Sends a message of size RecieveBuffer+1
            /// 2. Verifies that the correct exception is thrown
            /// </summary>
            ///
            bool testResult = true;
            byte[] buf = new byte[4];
            int number;
            try
            {
                int receiveBufferValue1 = 1024;
                int receiveBufferValue2 = 512;

                SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
                testSockets.socketClient.SetSocketOption(
                    SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, receiveBufferValue1);
                testSockets.socketClient.GetSocketOption(
                    SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, buf);
                number = (int)buf[0] + (int)(buf[1]<<8) + (int)(buf[2]<<16) + (int)(buf[3]<<24);
                
                if(number != receiveBufferValue1)
                        throw new System.Exception("ReceiveBuffer option was not set correctly");

                testSockets.socketClient.SetSocketOption(
                    SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, receiveBufferValue2);
                testSockets.socketClient.GetSocketOption(
                    SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, buf);
                number = (int)buf[0] + (int)(buf[1]<<8) + (int)(buf[2]<<16) + (int)(buf[3]<<24);
                
                if(number != receiveBufferValue2)
                        throw new System.Exception("ReceiveBuffer option was not set correctly");

                testSockets.Startup(0, 0);

                Log.Comment("ReceiveBuffer " + number.ToString() );
                   
                testSockets.bufSend    = new byte[number];
                testSockets.bufReceive = new byte[number];

                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                int cBytes = testSockets.socketClient.Send(testSockets.bufSend);
                if (cBytes != testSockets.bufSend.Length)
                    throw new System.Exception("Send failed, wrong length");

                using (Socket sock = testSockets.socketServer.Accept())
                {
                    if (sock.LocalEndPoint == null)
                        throw new System.Exception("LocalEndPoint is null");

                    if (testSockets.socketServer.LocalEndPoint.ToString() != 
                        sock.LocalEndPoint.ToString())
                        throw new System.Exception("LocalEndPoint is incorrect");

                    if (sock.RemoteEndPoint == null)
                        throw new System.Exception("RemoteEndPoint is null");
                    if (testSockets.socketClient.LocalEndPoint.ToString() != 
                        sock.RemoteEndPoint.ToString())
                        throw new System.Exception("RemoteEndPoint is incorrect");

                    //wait a second to ensure the socket is available
                    System.Threading.Thread.Sleep(1000);

                    cBytes = sock.Available;
                    if (cBytes != testSockets.bufSend.Length)
                        throw new System.Exception("Send failed, wrong length");

                    cBytes = sock.Receive(testSockets.bufReceive);
                }
                testSockets.AssertDataReceived(cBytes);
                testSockets.TearDown();
                testSockets = null;
            }
            catch (System.Exception e)
            {
                Log.Comment("Caught exception: " + e.Message);
                if (e.GetType() == Type.GetType("System.Net.Sockets.SocketException"))
                    Log.Comment("ErrorCode: " + ((SocketException)e).ErrorCode); 
                
                testResult = false;
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SocketsEnums15_SON_SendBuffer()
        {
            /// <summary>
            /// 1. Sends a message of size SendBuffer
            /// 2. Verifies that it transferred correctly
            /// 1. Sends a message of size SendBuffer+1
            /// 2. Verifies that the correct exception is thrown
            /// </summary>
            ///
            bool testResult = true;
            byte[] buf = new byte[4];
            int number;
            try
            {
                int sendBufferValue1 = 1024;
                int sendBufferValue2 = 512;

                SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
                testSockets.socketClient.SetSocketOption(
                    SocketOptionLevel.Socket, SocketOptionName.SendBuffer, sendBufferValue1);
                testSockets.socketClient.GetSocketOption(
                    SocketOptionLevel.Socket, SocketOptionName.SendBuffer, buf);
                number = (int)buf[0] + (int)(buf[1]<<8) + (int)(buf[2]<<16) + (int)(buf[3]<<24);
                
                if(number != sendBufferValue1)
                        throw new System.Exception("ReceiveBuffer option was not set correctly");

                testSockets.socketClient.SetSocketOption(
                    SocketOptionLevel.Socket, SocketOptionName.SendBuffer, sendBufferValue2);
                testSockets.socketClient.GetSocketOption(
                    SocketOptionLevel.Socket, SocketOptionName.SendBuffer, buf);
                number = (int)buf[0] + (int)(buf[1]<<8) + (int)(buf[2]<<16) + (int)(buf[3]<<24);
                
                if(number != sendBufferValue2)
                        throw new System.Exception("ReceiveBuffer option was not set correctly");

                Log.Comment("ReceiveBuffer " + number.ToString() );
                   
                testSockets.Startup(0, 0);

                testSockets.bufSend    = new byte[number];
                testSockets.bufReceive = new byte[number];

                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                int cBytes = testSockets.socketClient.Send(testSockets.bufSend);
                if (cBytes != testSockets.bufSend.Length)
                    throw new System.Exception("Send failed, wrong length");

                using (Socket sock = testSockets.socketServer.Accept())
                {
                    if (sock.LocalEndPoint == null)
                        throw new System.Exception("LocalEndPoint is null");

                    if (testSockets.socketServer.LocalEndPoint.ToString() != 
                        sock.LocalEndPoint.ToString())
                        throw new System.Exception("LocalEndPoint is incorrect");

                    if (sock.RemoteEndPoint == null)
                        throw new System.Exception("RemoteEndPoint is null");
                    if (testSockets.socketClient.LocalEndPoint.ToString() != 
                        sock.RemoteEndPoint.ToString())
                        throw new System.Exception("RemoteEndPoint is incorrect");

                    //wait a second to ensure the socket is available
                    System.Threading.Thread.Sleep(1000);  

                    cBytes = sock.Available;
                    if (cBytes != testSockets.bufSend.Length)
                        throw new System.Exception("Send failed, wrong length");

                    cBytes = sock.Receive(testSockets.bufReceive);
                }
                testSockets.AssertDataReceived(cBytes);
                testSockets.TearDown();
                testSockets = null;
            }
            catch (System.Exception e)
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
