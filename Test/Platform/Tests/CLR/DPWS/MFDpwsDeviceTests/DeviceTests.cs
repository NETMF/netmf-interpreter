/*---------------------------------------------------------------------
* FeatureTests.cs - file description
* Version: 1.0
* Author: REDMOND\a-grchat
* Created: 10/22/2007 10:05:30 AM 
* ---------------------------------------------------------------------*/

using System;
using Dpws.Device;
using Dpws.Device.Services;
using Microsoft.SPOT.Platform.Test;
using Ws.Services;
using Ws.Services.Xml;
using Ws.Services.Binding;

namespace Microsoft.SPOT.Platform.Tests
{
    public class DeviceTests : IMFTestInterface
    {
        const int NUM_TESTS = 5;

        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            
            // Add your functionality here. 
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
        public MFTestResults DeviceTest_ThisDevice()
        {
            /// <summary>
            /// 1. Verifies that each of the properties of the Device.ThisModel object is the correct type
            /// 2. Sets settable properties
            /// 3. Verifies their type again, and their data for non-nullable properties
            /// </summary>
            ///

            Device.Initialize(new WS2007HttpBinding(new HttpTransportBindingConfig("urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b51", 8084)), new ProtocolVersion10());

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


            bool testResult = true;
            try
            {
                WsServiceEndpoints testWSEs = Device.HostedServices;
                if (testWSEs.GetType() != Type.GetType("Ws.Services.WsServiceEndpoints"))
                    throw new Exception("HostedServices bad type");

                string testString = Device.IPV4Address;
                if (testString.GetType() != Type.GetType("System.String"))
                    throw new Exception("IPV4Address bad type");

                int testInt = Device.MetadataVersion;
                if (testInt.GetType() != Type.GetType("System.Int32"))
                    throw new Exception("MetadataVersion bad type");

                Device.MetadataVersion = -12;
                if (Device.MetadataVersion != -12)
                    throw new Exception("MetadataVersion did not set to invalid int value");

                Device.MetadataVersion = 2;
                if (Device.MetadataVersion != 2)
                    throw new Exception("MetadataVersion did not set to valid value");

                testInt = Device.ProbeMatchDelay;
                if (testInt.GetType() != Type.GetType("System.Int32"))
                    throw new Exception("ProbeMatchDelay bad type");

                Device.ProbeMatchDelay = 2;
                if (Device.ProbeMatchDelay != 2)
                    throw new Exception("ProbeMatchDelay did not set to valid value");

                DpwsHostedService testDHS = Device.Host;
                if (testDHS != null)
                    if (testDHS.GetType() !=
                        Type.GetType("Dpws.Device.Services.DpwsHostedService"))
                        throw new Exception("DpwsHostedService bad type");

                Device.Host = new DpwsHostedService(new ProtocolVersion10());
                testDHS = Device.Host;
                if (testDHS != null)
                    if (testDHS.GetType() !=
                        Type.GetType("Dpws.Device.Services.DpwsHostedService"))
                        throw new Exception("DpwsHostedService bad type after set to new");

                DpwsWseSubscriptionMgr testDWSM = Device.SubscriptionManager;
                if (testDWSM != null)
                    if (testDWSM.GetType() !=
                        Type.GetType("Dpws.Device.Services.DpwsWseSubscriptionMgr"))
                        throw new Exception("DpwsWseSubscriptionMgr bad type");

                // EndpointAddress will be defined by each event sink
                Device.SubscriptionManager = new DpwsWseSubscriptionMgr(new WS2007HttpBinding(), new ProtocolVersion10());
                testDWSM = Device.SubscriptionManager;
                if (testDWSM != null)
                    if (testDWSM.GetType() !=
                        Type.GetType("Dpws.Device.Services.DpwsWseSubscriptionMgr"))
                        throw new Exception("DpwsWseSubscriptionMgr bad type after set to new");
            }
            catch (Exception e)
            {
                testResult = false;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults DeviceTest_ThisDevice_ThisModel()
        {
            /// <summary>
            /// 1. Sets each of the string properties of the Device object to test data
            /// 2. Verifies that each of these properties is correctly set
            /// </summary>
            ///

            bool testResult = true;
            try
            {
                Device.ThisModel.Manufacturer = "Test1";
                Device.ThisModel.ManufacturerUrl = "Test2";
                Device.ThisModel.ModelName = "Test3";
                Device.ThisModel.ModelNumber = "Test4";
                Device.ThisModel.ModelUrl = "Test5";
                Device.ThisModel.PresentationUrl = "Test6";

                Device.ThisDevice.FriendlyName = "Test7";
                Device.ThisDevice.FirmwareVersion = "Test8";
                Device.ThisDevice.SerialNumber = "Test9";

                if (Device.ThisModel.Manufacturer != "Test1")
                    throw new Exception("Device.ThisModel.Manufacturer did not set correctly");
                if (Device.ThisModel.ManufacturerUrl != "Test2")
                    throw new Exception("Device.ThisModel.ManufacturerUrl did not set correctly");
                if (Device.ThisModel.ModelName != "Test3")
                    throw new Exception("Device.ThisModel.ModelName did not set correctly");
                if (Device.ThisModel.ModelNumber != "Test4")
                    throw new Exception("Device.ThisModel.ModelNumber did not set correctly");
                if (Device.ThisModel.ModelUrl != "Test5")
                    throw new Exception("Device.ThisModel.ModelUrl did not set correctly");
                if (Device.ThisModel.PresentationUrl != "Test6")
                    throw new Exception("Device.ThisModel.PresentationUrl did not set correctly");

                if (Device.ThisDevice.FriendlyName != "Test7")
                    throw new Exception("Device.FriendlyName did not set correctly");
                if (Device.ThisDevice.FirmwareVersion != "Test8")
                    throw new Exception("Device.FirmwareVersion did not set correctly");
                if (Device.ThisDevice.SerialNumber != "Test9")
                    throw new Exception("Device.SerialNumber did not set correctly");
            }
            catch (Exception e)
            {
                testResult = false;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        class TestDeviceHost : DpwsHostedService
        {
            public TestDeviceHost(string guid, ProtocolVersion version) : base(version)
            {
                // Add ServiceNamespace. Set ServiceID and ServiceTypeName
                ServiceNamespace =
                    new WsXmlNamespace("sim", "http://schemas.example.org/SimpleService");
                ServiceID = "urn:uuid:" + guid;
                ServiceTypeName = "TestDeviceType";
            }

            public TestDeviceHost(string guid) : 
                this(guid, new ProtocolVersion10())
            {
            }
        }

        [TestMethod]
        public MFTestResults DeviceTest_Start_Stop()
        {
            /// <summary>
            /// 1. Sets up the minimum framework to allow a Device object to start
            /// 2. Sleeps for 10 Seconds
            /// 3. Stops the Device
            /// </summary>
            ///

            bool testResult = true;
            try
            {
                ProtocolVersion ver = new ProtocolVersion10();
                Device.Initialize(new WS2007HttpBinding(new HttpTransportBindingConfig("urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b51", 8084)), ver);
                //Device.EndpointAddress = "urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b51";
                Device.ThisModel.Manufacturer = "Microsoft";
                Device.ThisModel.ManufacturerUrl = "http://www.Microsoft.com/";
                Device.ThisModel.ModelName = "MFDpwsDeviceTests Interop Test Device";
                Device.ThisModel.ModelNumber = "1";
                Device.ThisModel.ModelUrl = "http://www.Microsoft.com/";
                Device.ThisModel.PresentationUrl = "http://www.Microsoft.com/";

                Device.ThisDevice.FriendlyName = "Test Device";
                Device.ThisDevice.FirmwareVersion = "alpha";
                Device.ThisDevice.SerialNumber = "1";

                Device.Host = new TestDeviceHost("3cb0d1ba-cc3a-46ce-b416-212ac2419b51");

                Log.Comment("Starting Device...");
                ServerBindingContext ctx = new ServerBindingContext(ver);
                Device.Start(ctx);
                Log.Comment("Device started, sleeping for 10 seconds...");
                System.Threading.Thread.Sleep(10000);
                Log.Comment("Sleep completed.  Stopping Device...");
                Device.Stop();
                Log.Comment("Device stopped");
            }
            catch (Exception e)
            {
                testResult = false;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
    }
}
