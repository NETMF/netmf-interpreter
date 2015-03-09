////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Microsoft.SPOT.Platform.Tests
{
    public class NetTests : IMFTestInterface
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

        [TestMethod]
        public MFTestResults NetTest1_DNS()
        {
            /// <summary>
            /// 1. Creates an IPHostEntry for localhost
            /// 2. Verifies that it exists and contains the right data
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                IPHostEntry ipHostEntry = Dns.GetHostEntry("192.168.1.1");

                if (ipHostEntry.AddressList.Length != 1)
                    throw new Exception("GetHostEntry resturned wrong number of addresses");
                IPAddress address = ipHostEntry.AddressList[0];

                if (address == null)
                    throw new Exception("Address is null");

                if (address.ToString() != "192.168.1.1")
                    throw new Exception("Address is incorrect");
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception: " + e.Message);
                testResult = false;
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }


        [TestMethod]
        public MFTestResults NetTest2_IPAddressBasic()
        {
            /// <summary>
            /// 1. Creates 30 Random IPs between 0.0.0.0 and 255.255.255.127
            /// 2. Verifies that they can be constructed
            /// 3. Verifies that they have the correct data (GetAddressBytes)
            /// 4. Verifies ToString and GetHashcode
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                Random random = new Random();
                for (int i = 0; i <= 30; i++)
                {
                    int[] IPInts = { random.Next(256), random.Next(256), 
                        random.Next(256), random.Next(128) };
                    Log.Comment("Random IP " + IPInts[0] + "." + IPInts[1]
                        + "." + IPInts[2] + "." + IPInts[3]);
                    IPAddress address = new IPAddress((long)(
                        IPInts[0]
                        + IPInts[1] * 256
                        + IPInts[2] * 256 * 256
                        + IPInts[3] * 256 * 256 * 256));

                    if (address == null)
                        throw new Exception("Address is null");

                    Type typeOfAddress = address.GetType();
                    if (typeOfAddress != Type.GetType("System.Net.IPAddress"))
                        throw new Exception("Type is incorrect");

                    byte[] targetBytes = { (byte)IPInts[0], (byte)IPInts[1], 
                        (byte)IPInts[2], (byte)IPInts[3] };
                    byte[] addressBytes = address.GetAddressBytes();
                    if (addressBytes.Length != 4)
                        throw new Exception("GetAddressBytes returns wrong size");

                    for (int j = 0; j < 4; j++)
                        if (addressBytes[j] != targetBytes[j])
                            throw new Exception("GetAddressBytes returns wrong bytes");
                    IPAddress address2 = new IPAddress((long)(
                            IPInts[0]
                            + IPInts[1] * 256
                            + IPInts[2] * 256 * 256
                            + IPInts[3] * 256 * 256 * 256));

                    if (address.ToString() != address2.ToString())
                        throw new Exception("ToString returns differently for same data");

                    if (address.GetHashCode() != address2.GetHashCode())
                        throw new Exception("GetHasCode returns differently for same data");

                    address2 = new IPAddress((long)(
                        (IPInts[0] % 2 + 1)
                        + (IPInts[1] % 2 + 1 )* 256
                        + (IPInts[2] % 2 + 1 )* 256 * 256
                        + (IPInts[3] % 2 + 1 )* 256 * 256 * 256));
                    if (address.GetHashCode() == address2.GetHashCode())
                        throw new Exception("GetHasCode returns same for " + address.ToString() 
                            + " as " + address2.ToString());
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
        public MFTestResults NetTest3_IPAddressLoopback()
        {
            /// <summary>
            /// 1. Creates an IPHostEntry for localhost
            /// 2. Verifies that it exists and contains the right data
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                IPAddress address = IPAddress.Loopback;
                if (address == null)
                    throw new Exception("Address is null");

                if (address.ToString() != "127.0.0.1")
                    throw new Exception("Address is incorrect");

                Type typeOfAddress = address.GetType();
                if (typeOfAddress != Type.GetType("System.Net.IPAddress"))
                    throw new Exception("Type is incorrect");


                byte[] localhostBytes = {(byte)127,(byte)0,(byte)0,(byte)1};
                byte[] addressBytes = address.GetAddressBytes();
                if (addressBytes.Length != 4)
                    throw new Exception("GetAddressBytes returns wrong size");

                for (int i = 0; i < 4; i++)
                    if (addressBytes[i] != localhostBytes[i])
                        throw new Exception("GetAddressBytes returns wrong bytes");
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception: " + e.Message);
                testResult = false;
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        
        [TestMethod]
        public MFTestResults NetTest4_IPAddressAny()
        {
            /// <summary>
            /// 1. Creates an IPHostEntry for Any
            /// 2. Verifies that it exists and contains the right data
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                IPAddress address = IPAddress.Any;

                if (address == null)
                    throw new Exception("Address is null");

                if (address.ToString() != "0.0.0.0")
                    throw new Exception("Address is incorrect");

                Type typeOfAddress = address.GetType();
                if (typeOfAddress != Type.GetType("System.Net.IPAddress"))
                    throw new Exception("Type is incorrect");

                byte[] localhostBytes = {(byte)0,(byte)0,(byte)0,(byte)0};
                byte[] addressBytes = address.GetAddressBytes();
                if (addressBytes.Length != 4)
                    throw new Exception("GetAddressBytes returns wrong size");

                for (int i = 0; i < 4; i++)
                    if (addressBytes[i] != localhostBytes[i])
                        throw new Exception("GetAddressBytes returns wrong bytes");
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception: " + e.Message);
                testResult = false;
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }



        [TestMethod]
        public MFTestResults NetTest5_IPEndPointBasic()
        {
            /// <summary>
            /// 1. Creates 30 Random IPs between 0.0.0.0 and 255.255.255.127
            /// 2. Verifies that they can be constructed as IPEndPoints with both ctors
            /// 3. Verifies that their data, ToString and GetHashCode funcs return normally
            /// 4. Clones one with Create and verifies the above funcs again
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                Random random = new Random();
                for (int i = 0; i <= 30; i++)
                {
                    int[] IPInts = { random.Next(256), random.Next(256), 
                        random.Next(256), random.Next(128) };
                    int portInt = random.Next(65535) + 1;
                    long addressLong = (long)(
                        IPInts[0]
                        + IPInts[1] * 256
                        + IPInts[2] * 256 * 256
                        + IPInts[3] * 256 * 256 * 256);
                    Log.Comment("Random IP " + IPInts[0] + "." + IPInts[1]
                        + "." + IPInts[2] + "." + IPInts[3] + ":" + portInt);
                    IPAddress address = new IPAddress(addressLong);

                    Log.Comment("EndPoint1 created with IPAddress and int");
                    IPEndPoint endPoint1 = new IPEndPoint(address,portInt);
                    Log.Comment("EndPoint2 created with long and int"); 
                    IPEndPoint endPoint2 = new IPEndPoint(addressLong, portInt);
                    if (endPoint1 == null)
                        throw new Exception("EndPoint1 is null");
                    if (endPoint2 == null)
                        throw new Exception("EndPoint2 is null");

                    Type typeOfEndPoint = endPoint1.GetType();
                    if (typeOfEndPoint != Type.GetType("System.Net.IPEndPoint"))
                        throw new Exception("EndPoint1 Type is incorrect");
                    typeOfEndPoint = endPoint2.GetType();
                    if (typeOfEndPoint != Type.GetType("System.Net.IPEndPoint"))
                        throw new Exception("EndPoint2 Type is incorrect");

                    if (endPoint1.ToString() != endPoint2.ToString())
                        throw new Exception("ToString returns differently for same data");

                    if (!endPoint1.Equals(endPoint2))
                    {
                        throw new Exception("Equals returns false for same data");
                    }


                    int hashCode1 = endPoint1.GetHashCode();
                    int hashCode2 = endPoint2.GetHashCode();


                    if (hashCode1 != hashCode2)
                        throw new Exception("GetHasCode returns differently for same data");

                    if (endPoint1.Address.ToString() != endPoint2.Address.ToString()
                        || endPoint1.Address.ToString() != address.ToString()
                        || endPoint2.Address.ToString() != address.ToString())
                        throw new Exception("Address returns wrong data");

                    if (endPoint1.Port != endPoint2.Port
                        || endPoint1.Port != portInt
                        || endPoint2.Port != portInt)
                        throw new Exception("Port returns wrong data");
                    
                    Log.Comment("Cloning Enpoint1 into EndPoint2");
                    endPoint2 = (IPEndPoint)endPoint2.Create(endPoint1.Serialize());
                    typeOfEndPoint = endPoint2.GetType();
                    if (typeOfEndPoint != Type.GetType("System.Net.IPEndPoint"))
                        throw new Exception("EndPoint2 Type is incorrect after clone");

                    if (endPoint1.ToString() != endPoint2.ToString())
                        throw new Exception("ToString returns differently for cloned data");


                    //21295	GetHashCode returns differently for cloned data
                    if (endPoint1.GetHashCode() != endPoint2.GetHashCode())
                        throw new Exception("GetHashCode returns differently for cloned data");

                    if (endPoint1.Address.ToString() != endPoint2.Address.ToString()
                        || endPoint1.Address.ToString() != address.ToString()
                        || endPoint2.Address.ToString() != address.ToString())
                        throw new Exception("Address returns wrong data after clone");

                    if (endPoint1.Port != endPoint2.Port
                        || endPoint1.Port != portInt
                        || endPoint2.Port != portInt)
                        throw new Exception("Port returns wrong data after clone");

                    Log.Comment("Recreating EndPoint2 with new data");
                    int portInt2 = portInt % 2 + 1;
                    long addressLong2 = (long)(
                        (IPInts[0] % 2 + 1)
                        + (IPInts[1] % 2 + 1 )* 256
                        + (IPInts[2] % 2 + 1 )* 256 * 256
                        + (IPInts[3] % 2 + 1 )* 256 * 256 * 256);
                    endPoint2 = new IPEndPoint(addressLong2, portInt2);

                    if (endPoint1.GetHashCode() == endPoint2.GetHashCode())
                        throw new Exception("GetHashCode returns same for " 
                            + endPoint1.ToString()
                            + " as " + endPoint2.ToString());

                    if (endPoint1.Address == endPoint2.Address
                        || endPoint2.Address == address)
                        throw new Exception("Address returns wrong data after change");

                    if (endPoint1.Port == endPoint2.Port
                        || endPoint2.Port == portInt)
                        throw new Exception("Port returns wrong data after change");
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
        public MFTestResults NetTest5_IPHostEntryBasic()
        {
            /// <summary>
            /// 1. Creates an IPHostEntry for localhost
            /// 2. Verifies that it exists and contains the right data
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                IPHostEntry ipHostEntry = Dns.GetHostEntry("192.168.1.1");
                if (ipHostEntry == null)
                    throw new Exception("IPHostEntry is null");

                Type typeOfIPHostEntry = ipHostEntry.GetType();
                if (typeOfIPHostEntry != Type.GetType("System.Net.IPHostEntry"))
                    throw new Exception("IPHostEntry Type is incorrect");

                if (ipHostEntry.AddressList[0].ToString() != "192.168.1.1")
                    throw new Exception("AddressList[0] is incorrect");

                try
                {
                    ipHostEntry.AddressList[1].ToString();
                    throw new Exception("AddressList[1] is not null");
                }
                catch (System.IndexOutOfRangeException) { }

            }
            catch (Exception e)
            {
                Log.Comment("Caught exception: " + e.Message);
                testResult = false;
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults NetTest6_SocketAddressBasic()
        {
            /// <summary>
            /// 1. Creates 30 Random IPs between 0.0.0.0 and 255.255.255.127
            /// 2. Verifies that they can be constructed as SocketAddress
            /// 3. Verifies that they have the correct data (GetAddressBytes)
            /// 4. Verifies ToString and GetHashcode
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                Random random = new Random();
                for (int i = 0; i <= 30; i++)
                {
                    int[] IPInts = { random.Next(256), random.Next(256), 
                        random.Next(256), random.Next(128) };
                    Log.Comment("Random IP " + IPInts[0] + "." + IPInts[1]
                        + "." + IPInts[2] + "." + IPInts[3]);
                    IPAddress address = new IPAddress((long)(
                        IPInts[0]
                        + IPInts[1] * 256
                        + IPInts[2] * 256 * 256
                        + IPInts[3] * 256 * 256 * 256));
                    int portInt = random.Next(65536);
                    IPEndPoint ipEndpoint1 = new IPEndPoint(address, portInt);
                    SocketAddress socketAddress1 = ipEndpoint1.Serialize();
                    SocketAddress socketAddress2 = ipEndpoint1.Serialize();
                    if (socketAddress1 == null)
                        throw new Exception("socketAddress1 is null");
                    if (socketAddress2 == null)
                        throw new Exception("socketAddress2 is null");

                    Type typeOfSocketAddress = socketAddress1.GetType();
                    if (typeOfSocketAddress != Type.GetType("System.Net.SocketAddress"))
                        throw new Exception("socketAddress1 Type is incorrect");
                    typeOfSocketAddress = socketAddress2.GetType();
                    if (typeOfSocketAddress != Type.GetType("System.Net.SocketAddress"))
                        throw new Exception("socketAddress2 Type is incorrect");

                    if (socketAddress1.ToString() != socketAddress2.ToString())
                        throw new Exception("ToString returns differently for same data");

                    //21295	GetHashCode returns differently for cloned data
                    if (socketAddress1.GetHashCode() != socketAddress2.GetHashCode())
                        throw new Exception("GetHashCode returns differently for same data");

                    if (socketAddress1.Family != AddressFamily.InterNetwork) 
                        throw new Exception("socketAddress1 Family is incorrect");

                    if (socketAddress2.Family != AddressFamily.InterNetwork)
                        throw new Exception("socketAddress2 Family is incorrect");
                    /*
                     * Pending Resolution of 17428
                     * 
                    Log.Comment("Recreating socketAddress2 with new data");
                    int portInt2 = portInt % 2 + 1;
                    long addressLong2 = (long)(
                        (IPInts[0] % 2 + 1)
                        + (IPInts[1] % 2 + 1) * 256
                        + (IPInts[2] % 2 + 1) * 256 * 256
                        + (IPInts[3] % 2 + 1) * 256 * 256 * 256);

                    IPEndPoint ipEndpoint2 = new IPEndPoint(addressLong2, portInt2);
                    socketAddress2 = ipEndpoint2.Serialize();
                    socketAddress2.Family = AddressFamily.Chaos;
                    */
                    socketAddress2 = new SocketAddress(AddressFamily.Chaos, 8);
                    if (socketAddress1.GetHashCode() == socketAddress2.GetHashCode())
                        throw new Exception("GetHashCode returns same for " 
                            + socketAddress1.ToString()
                            + " as " + socketAddress2.ToString());

                    if (socketAddress1.ToString() == socketAddress2.ToString())
                        throw new Exception("ToString returns same for different data"); 
                }
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception: " + e.Message);
                testResult = false;
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.KnownFailure);
        }
    }
}
