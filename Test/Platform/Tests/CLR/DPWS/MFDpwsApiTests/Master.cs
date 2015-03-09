using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Interop.SimpleService;
using Interop.EventingService;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Net;
using Microsoft.SPOT.Net.NetworkInformation;
using Dpws.Device;
using Ws.Services;
using Ws.Services.Binding;


namespace Microsoft.SPOT.Platform.Tests
{

    class MFDpwsApiTests : IMFTestInterface
    {

        public static void Main()
        {
            string[] args = { "MFDpwsApiTests" };
            MFTestRunner runner = new MFTestRunner(args);
        }


        SimpleServiceClient client;

        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");

            IPSettings.PrintIPSettings();

            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
            {
                Log.Comment("Run the tests on the emulator");
                return InitializeResult.ReadyToGo;
            }
            else
            {
                Log.Comment("Only run the discovery tests on the emulator.  They are not supported on devices per bug. 22016");
                return InitializeResult.Skip;
            }
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }


        [TestMethod]
        public MFTestResults Start()
        {
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                //System.Ext.Console.Verbose = true;
                // Also start a local client on this device

                ProtocolVersion ver = new ProtocolVersion10();

                WS2007HttpBinding binding = new WS2007HttpBinding(new HttpTransportBindingConfig("urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b51", 8084));

                Device.Initialize(binding, ver);

                // Set device information
                //Device.EndpointAddress = "urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b51";
                Device.ThisModel.Manufacturer = "Microsoft Corporation";
                Device.ThisModel.ManufacturerUrl = "http://www.microsoft.com/";
                Device.ThisModel.ModelName = "SimpleService Test Device";
                Device.ThisModel.ModelNumber = "1.0";
                Device.ThisModel.ModelUrl = "http://www.microsoft.com/";
                Device.ThisModel.PresentationUrl = "http://www.microsoft.com/";

                Device.ThisDevice.FriendlyName = "SimpleService";
                Device.ThisDevice.FirmwareVersion = "alpha";
                Device.ThisDevice.SerialNumber = "12345678";

                // Add a Host service type
                Device.Host = new SimpleDeviceHost();

                // Add DPWS hosted services to the device
                Device.HostedServices.Add(new SimpleService());
                Device.HostedServices.Add(new EventingService());
                Device.HostedServices.Add(new AttachmentService());

                Log.Comment("Start the Client");
                client = new SimpleServiceClient();
                client.IgnoreRequestFromThisIP = false;

                Thread.Sleep(500);

                // Set to true to ignore the local client's requests
                Device.IgnoreLocalClientRequest = false;

                // Start the device stack
                Log.Comment("Start the Device");
                ServerBindingContext ctx = new ServerBindingContext(ver);
                Device.Start(ctx);
                int timeOut = 600000;
                //  The Client should be done intergoating the service within 10 minutes              
                if (!client.arHello.WaitOne(timeOut, false))
                {
                    Log.Comment("Client not done interogating the service for '" + timeOut + "' milliseconds");
                }

            }
            catch (Exception e)
            {
                Log.Comment("Unexpected Exception e: " + e.ToString());
            }
            finally
            {
                try
                {
                    Log.Comment("Stopping the service");
                    Device.Stop();
                }
                catch (Exception ex)
                {
                    Log.Comment("Caught : " + ex.Message);
                }
                Log.Comment("Waiting and verifying client received messages");

                //  Sleep for 15 seconds to let the client receive the bye events.
                client.arBye.WaitOne(15000, false);

                if (client != null)
                {

                    if (!client.m_receivedHelloEvent)
                    {
                        Log.Comment("Did not get HelloEvent.");
                        testResult = MFTestResults.Fail;
                    }
                    if (!client.m_getMex)
                    {
                        Log.Comment("Did not get GetMex.");
                        testResult = MFTestResults.Fail;
                    }              
                    if (!client.m_twoWay)
                    {
                        Log.Comment("Did not get TwoWay.");
                        testResult = MFTestResults.Fail;
                    }
                    if (!client.m_receivedByeEvent)
                    {
                        Log.Comment("Did not get ByeEvent.");
                        testResult = MFTestResults.Fail;
                    }

                    client.Dispose();
                }
            }
            return testResult;
        }

    }
    public class IPSettings
    {
        public static void PrintIPSettings()
        {
            NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();

            for (int i = 0; i < nis.Length; i++)
            {
                NetworkInterface networkInterface = nis[i];

                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    string m_macAddress = networkInterface.PhysicalAddress[0].ToString();
                    for (int j = 1; j < networkInterface.PhysicalAddress.Length; ++j)
                    {
                        m_macAddress += "-" + networkInterface.PhysicalAddress[j].ToString();
                    }

                    string[] m_staticDnsAddresses = new string[] { "0.0.0.0", "0.0.0.0" };
                    for (int k = 0; k < networkInterface.DnsAddresses.Length; ++k)
                    {
                        m_staticDnsAddresses[k] = networkInterface.DnsAddresses[k];
                    }

                    Log.Comment("IP Settings for interface " + i.ToString() + " are:");
                    Log.Comment("IpAddress:            " + networkInterface.IPAddress);
                    Log.Comment("DHCP Enabled:         " + networkInterface.IsDhcpEnabled.ToString());
                    Log.Comment("DynamicDns Enabled:   " + networkInterface.IsDynamicDnsEnabled.ToString());
                    Log.Comment("Subnet Mask :         " + networkInterface.SubnetMask);
                    Log.Comment("Gateway Address:      " + networkInterface.GatewayAddress);
                    Log.Comment("Static Dns Address 1: " + m_staticDnsAddresses[0]);
                    Log.Comment("Static Dns Address 2: " + m_staticDnsAddresses[1]);
                    Log.Comment("MAC Address is:       " + m_macAddress);
                    Log.Comment("");
                }
            }
        }
    }

}
