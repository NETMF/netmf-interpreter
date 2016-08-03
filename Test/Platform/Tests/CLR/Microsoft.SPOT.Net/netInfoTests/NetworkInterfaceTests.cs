////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.SPOT.Net.NetworkInformation;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class NetworkInterfaceTests : IMFTestInterface
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
        public MFTestResults NetworkInterfaceTest_GetAllNetworkInterfaces_Count()
        {
            //don't run these tests on the emulator.  The emulator is SKU # 3
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            try
            {
                NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
                if (nis.Length < 1)
                {
                    Log.Comment("Expected at least one NetworkInterface, but got " + nis.Length);
                    return MFTestResults.Fail;
                }
            }
            catch (Exception e)
            {
                Log.Comment("Exception: " + e.ToString());
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults NetworkInterfaceTest1_Types()
        {
            /// <summary>
            /// Typechecking for Network Interface
            /// This was to be Method checking but all of the methods currently throw
            /// exceptions on the emulator.
            /// </summary>
            ///
            MFTestResults testResult = MFTestResults.Fail;
            try
            {
                foreach (NetworkInterface testNI in NetworkInterface.GetAllNetworkInterfaces())
                {

                    if (testNI.IPAddress.GetType() != Type.GetType("System.String"))
                        throw new Exception("IPAddress type is incorrect");

                    if (testNI.IsDhcpEnabled.GetType() != Type.GetType("System.Boolean"))
                        throw new Exception("IsDhcpEnabled type is incorrect");

                    if (testNI.IsDynamicDnsEnabled.GetType() != Type.GetType("System.Boolean"))
                        throw new Exception("IsDynamicDnsEnabled type is incorrect");

                    if (testNI.NetworkInterfaceType.GetType() != Type.GetType(
                        "Microsoft.SPOT.Net.NetworkInformation.NetworkInterfaceType"))
                        throw new Exception("NetworkInterfaceType type is incorrect");

                    if (testNI.DnsAddresses.GetType() != typeof(System.String[]))
                        throw new Exception("DnsAddresses type is incorrect");

                    if (testNI.GatewayAddress.GetType() != Type.GetType("System.String"))
                        throw new Exception("GatewayAddress type is incorrect");

                    //See 18255 for why this looks different
                    if (testNI.PhysicalAddress.GetType() != typeof(System.Byte[]))
                        throw new Exception("PhysicalAddress type is incorrect");

                    if (testNI.SubnetMask.GetType() != Type.GetType("System.String"))
                        throw new Exception("SubnetMask type is incorrect");

                    testResult = MFTestResults.Pass;
                }
            }
            catch (Exception e)
            {
                Log.Comment("Caught exception: " + e.Message);
                testResult = MFTestResults.Fail;
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults NetworkInterfaceTest2_GetProperties()
        {
            /// <summary>
            /// These tests show the information about your machine's 
            /// Network Interfaces, for thorough verification check the data
            /// against an 'ipconfig /all'.
            /// </summary>
            ///
            bool testResult = true;

            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                try
                {
                    Log.Comment("networkInterface.IPAddress : " + networkInterface.IPAddress);
                }
                catch (Exception e)
                {
                    Log.Comment("Exception raised while reading IPAddress");
                    Log.Comment(e.ToString());
                    testResult = false;
                }

                try
                {
                    Log.Comment("networkInterface.GatewayAddress : " +
                        networkInterface.GatewayAddress);
                }
                catch (Exception e)
                {
                    Log.Comment("Exception raised while reading GatewayAddress");
                    Log.Comment(e.ToString());
                    testResult = false;
                }

                try
                {
                    byte[] physicalAddress = networkInterface.PhysicalAddress;

                    Log.Comment("its ok to have a networkinterface without a physical address.");
                    if (physicalAddress.Length != 0)
                    {
                        if (physicalAddress.Length != 6)
                        {
                            throw new Exception(
                                "Physical address doesn't have the right amount of digits");
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Comment("Exception raised while reading PhysicalAddress");
                    Log.Comment(e.ToString());

                    testResult = false;
                }
                try
                {
                    Log.Comment("networkInterface.SubnetMask : " + networkInterface.SubnetMask);
                }
                catch (Exception e)
                {
                    Log.Comment("Exception raised while reading SubnetMask");
                    Log.Comment(e.ToString());
                    testResult = false;
                }
                try
                {
                    // DNS Addresses
                    Log.Comment("DNS Addresses");
                    foreach (string dnsAddress in networkInterface.DnsAddresses)
                    {
                        Log.Comment("\t" + dnsAddress);
                    }
                }
                catch (Exception e)
                {
                    Log.Comment("Exception raised while reading DnsAddresses");
                    Log.Comment(e.ToString());
                    testResult = false;
                }
                try
                {
                    Log.Comment("networkInterface.IsDhcpEnabled : "
                        + networkInterface.IsDhcpEnabled);
                }
                catch (Exception e)
                {
                    Log.Comment("Exception raised while reading IsDhcpEnabled");
                    Log.Comment(e.ToString());
                    testResult = false;
                }

                try
                {
                    Log.Comment("networkInterface.IsDynamicDnsEnabled : "
                        + networkInterface.IsDynamicDnsEnabled);
                }
                catch (Exception e)
                {
                    Log.Comment("Exception raised while reading IsDynamicDnsEnabled");
                    Log.Comment(e.ToString());
                    testResult = false;
                }

                try
                {
                    Log.Comment("networkInterface.NetworkInterfaceType : "
                        + networkInterface.NetworkInterfaceType);
                }
                catch (Exception e)
                {
                    Log.Comment("Exception raised while reading NetworkInterfaceType");
                    Log.Comment(e.ToString());
                    testResult = false;
                }
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        //Validate the settings for Dynamic IP and Static DNS work.
        // Issue #320: The not supported combinations of Dynamic/Static DNS and Dynamic/Static IP do not cause exception
        [TestMethod]
        public MFTestResults NetworkInterfaceTest4_DynamicIP_StaticDNS()
        {
            MFTestResults testResult = MFTestResults.Fail;

            //don't run these tests on the emulator.  The emulator is SKU # 3
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            for (int i = 0; i < NetworkInterface.GetAllNetworkInterfaces().Length; ++i)
            {
                NetworkInterface testNI = NetworkInterface.GetAllNetworkInterfaces()[i];
                
                IPSettings ipSettings = new IPSettings(testNI);

                try
                {
                    Log.Comment("EnableDhcp");
                    testNI.EnableDhcp();

                    if (!testNI.IsDhcpEnabled)
                        throw new Exception("IsDhcpEnabled data incorrect after EnableDhcp");

                    try
                    {
                        // The combination of DHCP and StaticDNS is not supported at the moment
                        Log.Comment("EnableStaticDns should fail");
                        testNI.EnableStaticDns(new string[] { "157.54.14.146" });
                        testResult = MFTestResults.Fail;
                    }
                    catch (Exception e)
                    {
                        Log.Comment("Expected Exception: " + e.ToString());
                        Log.Comment("Trying to enable Dynamic DNS with a Static IP address correctly fails");
                        testResult = MFTestResults.Pass;
                    }

                    if (!testNI.IsDynamicDnsEnabled)
                        throw new Exception("IsDynamicDnsEnabled data incorrect");

                    testResult = MFTestResults.Pass;
                }

                catch (Exception e)
                {
                    Log.Comment("Exception: " + e.ToString());
                    testResult = MFTestResults.Fail;
                    break;
                }
                finally
                {
                    ipSettings.Restore(testNI);
                }
            }
            return testResult;
        }

        //Validate the settings for Dynamic IP and Dynamic DNS work.
        [TestMethod]
        public MFTestResults NetworkInterfaceTest3_DynamicIP_DynamicDNS()
        {
            MFTestResults testResult = MFTestResults.Fail;

            //don't run these tests on the emulator.  The emulator is SKU # 3
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            for (int i = 0; i < NetworkInterface.GetAllNetworkInterfaces().Length; ++i)
            {
                NetworkInterface testNI = NetworkInterface.GetAllNetworkInterfaces()[i];

                IPSettings ipSettings = new IPSettings(testNI);

                try
                {
                    Log.Comment("EnableDhcp");
                    testNI.EnableDhcp();

                    if (!testNI.IsDhcpEnabled)
                        throw new Exception("IsDhcpEnabled data incorrect after EnableDhcp");

                    Log.Comment("EnableDynamicDns");
                    testNI.EnableDynamicDns();

                    if (!testNI.IsDynamicDnsEnabled)
                        throw new Exception("IsDynamicDnsEnabled data incorrect after EnableDynamicDns");

                    testResult = MFTestResults.Pass;
                }
                catch (Exception e)
                {
                    Log.Comment("Exception: " + e.ToString());
                    testResult = MFTestResults.Fail;
                    break;
                }
                finally
                {
                    ipSettings.Restore(testNI);
                }
            }

            return testResult;
        }

        //Validate the settings for Static IP and Static DNS work.
        [TestMethod]
        public MFTestResults NetworkInterfaceTest5_StaticIP_StaticDNS()
        {
            MFTestResults testResult = MFTestResults.Fail;

            //don't run these tests on the emulator.  The emulator is SKU # 3
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            for (int i = 0; i < NetworkInterface.GetAllNetworkInterfaces().Length; ++i)
            {
                NetworkInterface testNI = NetworkInterface.GetAllNetworkInterfaces()[i];
                IPSettings ipSettings = new IPSettings(testNI);

                try
                {
                    Log.Comment("EnableStaticIP");
                    testNI.EnableStaticIP("157.54.14.192", testNI.SubnetMask, testNI.GatewayAddress);

                    if (testNI.IsDhcpEnabled)
                        throw new Exception("IsDhcpEnabled data incorrect after EnableStaticIP");

                    Log.Comment("EnableStaticDns");
                    testNI.EnableStaticDns(new string[] { "157.54.14.146" });

                    if (testNI.IsDynamicDnsEnabled)
                        throw new Exception("IsDynamicDnsEnabled data incorrect after EnableStaticDns");

                    testResult = MFTestResults.Pass;
                }
                catch (Exception e)
                {
                    Log.Comment("Exception: " + e.ToString());
                    testResult = MFTestResults.Fail;
                    break;
                }
                finally
                {
                    ipSettings.Restore(testNI);
                }
            }
            return testResult;
        }

        //Validate the settings for Static IP and Dynamic DNS fails.
        //Issue #320: The not supported combinations of Dynamic/Static DNS and Dynamic/Static IP do not cause exception
        [TestMethod]
        public MFTestResults NetworkInterfaceTest6_StaticIP_DynamicDNS()
        {
            MFTestResults testResult = MFTestResults.Fail;

            //don't run these tests on the emulator.  The emulator is SKU # 3
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            for (int i = 0; i < NetworkInterface.GetAllNetworkInterfaces().Length; ++i)
            {
                NetworkInterface testNI = NetworkInterface.GetAllNetworkInterfaces()[i];
                IPSettings ipSettings = new IPSettings(testNI);

                try
                {
                    Log.Comment("EnableStaticIP");
                    testNI.EnableStaticIP("157.54.14.192", testNI.SubnetMask, testNI.GatewayAddress);

                    if (testNI.IsDhcpEnabled)
                        throw new Exception("IsDhcpEnabled data incorrect after EnableStaticIP");

                    try
                    {
                        Log.Comment("EnableDynamicDns should fail");
                        testNI.EnableDynamicDns();
                        testResult = MFTestResults.Fail;
                    }
                    catch (Exception e)
                    {
                        Log.Comment("Expected Exception: " + e.ToString());
                        Log.Comment("Trying to enable Dynamic DNS with a Static IP address correctly fails");
                        testResult = MFTestResults.Pass;
                    }

                    if (testNI.IsDynamicDnsEnabled)
                        throw new Exception("IsDynamicDnsEnabled data incorrect");

                }
                catch (Exception e)
                {
                    Log.Comment("Exception: " + e.ToString());
                    testResult = MFTestResults.Fail;
                }
                finally
                {
                    ipSettings.Restore(testNI);
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults NetworkInterfaceTest7_DhcpRenewRelease()
        {
            /// <summary>
            /// 1. Save network configuration
            /// 2. Enable Dhcp (if not enabled)
            /// 3. Test Dhcp Release/Renew 
            /// 4. Restore network configuration
            /// </summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;

            //don't run these tests on the emulator.  The emulator is SKU # 3
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            try
            {
                for (int i = 0; i < NetworkInterface.GetAllNetworkInterfaces().Length; ++i)
                {
                    NetworkInterface testNI = NetworkInterface.GetAllNetworkInterfaces()[i];
                    IPSettings ipConfig = new IPSettings(testNI);

                    try
                    {
                        Log.Comment("EnableDhcp");
                        testNI.EnableDhcp();

                        Log.Comment(" make sure we can release");
                        testNI.ReleaseDhcpLease();

                        Log.Comment(" make sure we can renew");
                        testNI.RenewDhcpLease();
                    }
                    catch (Exception e)
                    {
                        Log.Comment("Caught exception: " + e.Message);
                        testResult = MFTestResults.Fail;
                    }
                    finally
                    {
                        ipConfig.Restore(testNI);
                    }
                }

            }
            catch (Exception e)
            {
                Log.Comment("Caught exception: " + e.Message);
                testResult = MFTestResults.Fail;
            }
            return testResult;
        }

        //[TestMethod]
        //public MFTestResults NetworkInterfaceTest8_GetHostEntry()
        //{
        //    /// <summary>
        //    /// 1. Save network configuration
        //    /// 2. Enable Static IP (bad address)
        //    /// 3. Enable Static DNS
        //    /// 4. Test GetHostEntry
        //    /// 5. Restore network configuration
        //    /// </summary>
        //    ///
        //    MFTestResults testResult = MFTestResults.Pass;

        //    //don't run these tests on the emulator.  The emulator is SKU # 3
        //    if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
        //        return MFTestResults.Skip;

        //    try
        //    {
        //        for (int i = 0; i < NetworkInterface.GetAllNetworkInterfaces().Length; ++i)
        //        {
        //            NetworkInterface testNI = NetworkInterface.GetAllNetworkInterfaces()[i];
        //            IPSettings ipConfig = new IPSettings(testNI);

        //            try
        //            {
        //                Log.Comment("EnableDhcp");
        //                testNI.EnableDhcp();

        //                // leave the addresses alone, they are supposed to be somewhat bogus 
        //                Log.Comment("misconfigure the static IP configuration");
        //                testNI.EnableStaticIP(testNI.IPAddress, testNI.SubnetMask, testNI.GatewayAddress);
        //                testNI.EnableStaticDns(new string[] { "157.54.14.146", "157.54.14.178" });

        //                Log.Comment("get a DNS server host entry");
        //                IPHostEntry entry = Dns.GetHostEntry("msw.dns.microsoft.com");
    
        //            }
        //            catch (SocketException e)
        //            {
        //                Log.Comment("SocketException error code: " + e.ErrorCode);
        //                Log.Comment(e.Message);
        //                testResult = MFTestResults.Fail;
        //            }
        //            catch (Exception e)
        //            {
        //                Log.Comment("Caught exception: " + e.Message);
        //                testResult = MFTestResults.Fail;
        //            }
        //            finally
        //            {
        //                ipConfig.Restore(testNI);
        //            }
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        Log.Comment("Caught exception: " + e.Message);
        //        testResult = MFTestResults.Fail;
        //    }

        //    return testResult;
        //}

        [TestMethod]
        public MFTestResults NetworkInterfaceTest0_EnableStaticDnsTwice()
        {
            //don't run these tests on the emulator.  The emulator is SKU # 3
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            MFTestResults testResult = MFTestResults.Pass;

            try
            {
                NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();

                for (int i = 0; i < nis.Length; i++)
                {
                    NetworkInterface testNI = nis[i];

                    IPSettings ipSettings = new IPSettings(testNI);

                    try
                    {
                        String [] dnsAddresses = new string[] { "0.0.0.0", "0.0.0.0" };
                        for (int j = 0; j < testNI.DnsAddresses.Length; ++j)
                        {
                            dnsAddresses[j] = testNI.DnsAddresses[j];
                        }

                        //This double call to EnableStaticDns() is for the following bug that was fixed.
                        //Do not remove.
                        //19756 EBS: Call EnableStaticDns() twice then set the staticDnsAddresses and the device will throw an Abort Data.
                        Log.Comment("EnableStaticDns");
                        testNI.EnableStaticDns(dnsAddresses);
                        Log.Comment("EnableStaticDns");
                        testNI.EnableStaticDns(dnsAddresses);
                    }
                    finally
                    {
                        ipSettings.Restore(testNI);
                        //grab the newly restored settings and display the information for debugging.
                        ipSettings = new IPSettings(testNI);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Comment("Exception: " + e.ToString());
                testResult = MFTestResults.Fail;
            }
            return testResult;
        }

        //Validate the physical address property.
        [TestMethod]
        public MFTestResults NetworkInterfaceTest9_PhysicalAddress()
        {
            MFTestResults testResult = MFTestResults.Pass;

            //don't run these tests on the emulator.  The emulator is SKU # 3
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                return MFTestResults.Skip;

            try
            {
                for (int i = 0; i < NetworkInterface.GetAllNetworkInterfaces().Length; ++i)
                {
                    NetworkInterface testNI = NetworkInterface.GetAllNetworkInterfaces()[i];
                    byte[] physicalAddress = testNI.PhysicalAddress;

                    Log.Comment("its ok to have a networkinterface without a physical address.");
                    if (physicalAddress.Length != 0)
                    {
                        if (physicalAddress.Length != 6)
                        {
                            String address = "";
                            for (int j = 0; j < physicalAddress.Length; j++)
                            {
                                address += physicalAddress[j] + ".";
                            }

                            throw new Exception("Physical address length invalid: " + physicalAddress.Length.ToString() + " Address: " + address);
                        }

                        byte[] newAddress = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 };
                        Log.Comment("Set PhysicalAddress");
                        testNI.PhysicalAddress = newAddress;

                        Log.Comment("Verify that setting the Physical Address stuck");
                        for (int k = 0; k < 6; ++k)
                        {
                            if (testNI.PhysicalAddress[k] != newAddress[k])
                            {
                                Log.Comment("Return the physical address to its original state");
                                testNI.PhysicalAddress = physicalAddress;
                                return MFTestResults.Fail;
                            }
                        }

                        try
                        {
                            byte[] newAddress2 = { 0x7, 0x6, 0x5, 0x4, 0x3, 0x2, 0X1 };

                            Log.Comment("try to set the physical address to an invalid one");
                            for (int l = 0; l < newAddress2.Length; ++l)
                            {
                                Log.Comment("Set PhysicalAddress");
                                testNI.PhysicalAddress[l] = newAddress2[l];
                            }
                            testResult = MFTestResults.Fail;
                        }
                        catch (Exception e)
                        {
                            Log.Comment("Exception:" + e.ToString());
                            Log.Comment("correctly threw exception trying to set invalid physicaladdress value");
                        }

                        Log.Comment("Return the physical address to its original state");
                        testNI.PhysicalAddress = physicalAddress;

                        Log.Comment("Test that tne physical address stuck");
                        for (int k = 0; k < 6; ++k)
                        {
                            if (testNI.PhysicalAddress[k] != physicalAddress[k])
                            {
                                Log.Comment("Resetting the physical address to the original failed");
                                return MFTestResults.Fail;
                            }
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Log.Comment("Exception: " + e.ToString());
                testResult = MFTestResults.Fail;
            }
            return testResult;
        }

        public class IPSettings
        {
            //store off the settings so that they can be reset to users settings upon completion of the test.
            bool     m_dhcp;
            bool     m_dynamicDns;
            string   m_ipAddress;
            string   m_subNetMask;
            string   m_gatewayAddress;
            string[] m_staticDnsAddresses;
            string   m_macAddress;

            //--//

            public IPSettings(NetworkInterface networkInterface)
            {
                Log.Comment("store off the settings so that they can be reset to users settings upon completion of the test.");

                m_dhcp           = networkInterface.IsDhcpEnabled;
                m_dynamicDns     = networkInterface.IsDynamicDnsEnabled;
                m_ipAddress      = networkInterface.IPAddress;
                m_subNetMask     = networkInterface.SubnetMask;
                m_gatewayAddress = networkInterface.GatewayAddress;
                
                m_macAddress = networkInterface.PhysicalAddress[0].ToString();
                for (int j = 1; j < networkInterface.PhysicalAddress.Length; ++j)
                {
                    m_macAddress += "-" + networkInterface.PhysicalAddress[j].ToString();
                }

                m_staticDnsAddresses = new string[] { "0.0.0.0", "0.0.0.0" };
                for (int i = 0; i < networkInterface.DnsAddresses.Length; ++i)
                {
                    m_staticDnsAddresses[i] = networkInterface.DnsAddresses[i];
                }
                PrintIPSettings();
            }

            public void PrintIPSettings()
            {
                Log.Comment("IP Settings are: ");
                Log.Comment("DHCP Enabled:         " + m_dhcp.ToString());
                Log.Comment("DynamicDns Enabled:   " + m_dynamicDns.ToString());
                Log.Comment("IpAddress:            " + m_ipAddress);
                Log.Comment("Subnet Mask :         " + m_subNetMask);
                Log.Comment("Gateway Address:      " + m_gatewayAddress);
                Log.Comment("Static Dns Address 1: " + m_staticDnsAddresses[0]);
                Log.Comment("Static Dns Address 2: " + m_staticDnsAddresses[1]);
                Log.Comment("MAC Address is:       " + m_macAddress);
            }

            public void Restore(NetworkInterface networkInterface)
            {
                Log.Comment("Return the IP settings to their original state before the test started");

                if (m_dhcp)
                {
                    Log.Comment("EnableDhcp");
                    networkInterface.EnableDhcp();
                }
                else
                {
                    Log.Comment("EnableStaticIP");
                    networkInterface.EnableStaticIP(m_ipAddress, m_subNetMask, m_gatewayAddress);
                }

                if (m_dynamicDns)
                {
                    Log.Comment("EnableDynamicDns");
                    networkInterface.EnableDynamicDns();
                }
                else
                {
                    Log.Comment("EnableStaticDns");
                    networkInterface.EnableStaticDns(m_staticDnsAddresses);
                }

            }
        }
    }
}
