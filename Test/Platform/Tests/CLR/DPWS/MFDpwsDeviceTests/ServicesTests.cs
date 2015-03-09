/*---------------------------------------------------------------------
* Micro Framework Test Case1.cs - file description
* Version: 1.0
* Author: REDMOND\a-grchat
* Created: 10/22/2007 4:53:35 PM 
* ---------------------------------------------------------------------*/

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using Dpws.Device;
using Dpws.Device.Services;
using Ws.Services;
using Ws.Services.Utilities;
using Ws.Services.WsaAddressing;
using System.IO;
using System.Ext.Xml;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ServicesTests : IMFTestInterface
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
        public MFTestResults ServicesTest_DPWSHostedService_Properties()
        {
            /// <summary>
            /// 1. Verifies each of the proerties of a DPWSHostedService object
            /// 2. Sets and re-verifies for simple properties, checks input checking 
            /// on properties that currently support it.
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                DpwsHostedService testDHS = Device.Host;

                Log.Comment("EndpointAddress");
                if (testDHS.EndpointAddress != null)
                    if (testDHS.EndpointAddress.GetType() != Type.GetType("System.String"))
                        throw new Exception("EndpointAddress wrong type");

                testDHS.EndpointAddress = "urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b51";
                if (testDHS.EndpointAddress.GetType() != Type.GetType("System.String"))
                    throw new Exception("EndpointAddress wrong type after set");

                if (testDHS.EndpointAddress != "urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b51")
                    throw new Exception("EndpointAddress bad data");

                try
                {
                    testDHS.EndpointAddress = "test1";
                    throw new Exception("EndpointAddress failed to prevent bad data input");
                }
                catch (System.ArgumentException) { }

                Log.Comment("EndpointRefs");
                if (testDHS.EndpointRefs != null)
                    if (testDHS.EndpointRefs.GetType() !=
                        Type.GetType("Ws.Services.WsaAddressing.WsWsaEndpointRefs"))
                        throw new Exception("EndpointRefs wrong type");

                if (testDHS.EndpointRefs != null)
                    if (testDHS.EndpointRefs.GetType() !=
                        Type.GetType("Ws.Services.WsaAddressing.WsWsaEndpointRefs"))
                        throw new Exception("EndpointRefs wrong type after set to new");

                Log.Comment("EventSources");
                if (testDHS.EventSources.GetType() !=
                    Type.GetType("Dpws.Device.Services.DpwsWseEventSources"))
                    throw new Exception("EventSources wrong type");

                testDHS.EventSources = new DpwsWseEventSources();
                if (testDHS.EventSources.GetType() !=
                    Type.GetType("Dpws.Device.Services.DpwsWseEventSources"))
                    throw new Exception("EventSources wrong type after set to new");

                Log.Comment("ServiceID");
                if (testDHS.ServiceID != null)
                    if (testDHS.ServiceID.GetType() != Type.GetType("System.String"))
                        throw new Exception("ServiceID wrong type");

                testDHS.ServiceID = "urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b51";
                if (testDHS.ServiceID.GetType() != Type.GetType("System.String"))
                    throw new Exception("ServiceID wrong type after set");

                if (testDHS.ServiceID != "urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b51")
                    throw new Exception("ServiceID bad data");

                try
                {
                    testDHS.ServiceID = "test2";
                    throw new Exception("ServiceID failed to prevent bad data input");
                }
                catch (System.ArgumentException) { }

                Log.Comment("ServiceNamespace");
                if (testDHS.ServiceNamespace != null)
                    if (testDHS.ServiceNamespace.GetType() !=
                        Type.GetType("Ws.Services.Xml.WsXmlNamespace"))
                        throw new Exception("ServiceNamespace wrong type");

                Log.Comment("WsServiceOperations");
                if (testDHS.ServiceOperations.GetType() !=
                    Type.GetType("Ws.Services.WsServiceOperations"))
                    throw new Exception("WsServiceOperations wrong type");

                Log.Comment("ServiceTypeName");
                if (testDHS.ServiceTypeName.GetType() != Type.GetType("System.String"))
                    throw new Exception("ServiceTypeName wrong type");

                testDHS.ServiceTypeName = "test3";
                if (testDHS.ServiceTypeName.GetType() != Type.GetType("System.String"))
                    throw new Exception("ServiceTypeName wrong type after set");

                if (testDHS.ServiceTypeName != "test3")
                    throw new Exception("ServiceTypeName wrong data");

            }
            catch (Exception e)
            {
                testResult = false;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }


        [TestMethod]
        public MFTestResults ServicesTest_DPWSHostedService_Methods()
        {
            /// <summary>
            /// This would test the methods in DPWSHostedService, but all of them are backend
            /// only methods.
            /// </summary>
            ///

            bool testResult = true;
            try
            {
                DpwsHostedService testDHS = Device.Host;

                //These methods are used only by the backend and are implicitly 
                //tested by other tests
                //testDHS.GetStatus(new Ws.Services.WsaAddressing.WsWsaHeader(), 
                //  new Ws.Services.Xml.WsXmlDocument());
                //testDHS.Renew(new Ws.Services.WsaAddressing.WsWsaHeader(), 
                //  new Ws.Services.Xml.WsXmlDocument());
                //testDHS.Subscribe(new Ws.Services.WsaAddressing.WsWsaHeader(), 
                //  new Ws.Services.Xml.WsXmlDocument());
                //testDHS.Unsubscribe(new Ws.Services.WsaAddressing.WsWsaHeader(), 
                //  new Ws.Services.Xml.WsXmlDocument());
            }
            catch (Exception e)
            {
                testResult = false;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }


        [TestMethod]
        public MFTestResults ServicesTest3_DpwsWseEventSource()
        {
            /// <summary>
            /// 1. Gets and verifies each of the properties of a DpwsWseEventSource object
            /// 2. Sets and re-verifies all properties
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                DpwsWseEventSource testDWES = new DpwsWseEventSource(
                    "testPrefix", "urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b51", "testName");


                if (testDWES.Name == null
                    || testDWES.Name.GetType() != Type.GetType("System.String")
                    || testDWES.Name != "testName")
                    throw new Exception("Name did not set correctly");

                if (testDWES.NamespaceURI == null
                    || testDWES.NamespaceURI.GetType() != Type.GetType("System.String")
                    || testDWES.NamespaceURI != "urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b51")
                    throw new Exception("NamespaceURI did not set correctly");

                if (testDWES.Operation == null
                    || testDWES.Operation.GetType() != Type.GetType("System.String")
                    || testDWES.Operation != "urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b51" + "/testName")
                    throw new Exception("Operation did not set correctly");

                if (testDWES.Prefix == null
                    || testDWES.Prefix.GetType() != Type.GetType("System.String")
                    || testDWES.Prefix != "testPrefix")
                    throw new Exception("Prefix did not set correctly");
            }
            catch (Exception e)
            {
                testResult = false;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }


        [TestMethod]
        public MFTestResults ServicesTest4_DpwsWseEventSources()
        {
            /// <summary>
            /// 1. Verifies the properties of a DpwsWseEventSources object
            /// 2. Adds elements to it
            /// 3. Re-verifies
            /// 4. Empties the object
            /// 5. Re-verifies
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                DpwsWseEventSources testDWESs = new DpwsWseEventSources();

                if (testDWESs.Count != 0)
                    throw new Exception("Count did not set correctly on new");

                try
                {
                    if (testDWESs.Count != 0)
                        throw new Exception("Current did not set correctly on new");
                }
                catch (IndexOutOfRangeException) { }
                catch (InvalidOperationException) { }


                testDWESs.Add(new DpwsWseEventSource(
                    "testPrefix1", "urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b51", "testName1"));

                testDWESs.Add(new DpwsWseEventSource(
                    "testPrefix2", "urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b52", "testName2"));

                if (testDWESs.Count != 2)
                    throw new Exception("Count did not set correctly on new");

            }
            catch (Exception e)
            {
                Log.Comment("Exception: " + e.ToString());
                testResult = false;
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
    }
}
