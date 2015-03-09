/*---------------------------------------------------------------------
* ClientTests.cs - file description
* Version: 1.0
* Author: REDMOND\a-grchat
* Created: 11/2/2007 3:57:49 PM 
* ---------------------------------------------------------------------*/

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using Dpws.Client;
using Dpws.Client.Eventing;
using Dpws.Client.Discovery;
using Dpws.Device;
using Ws.Services;
using Ws.Services.Binding;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ClientTests : IMFTestInterface
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
        public MFTestResults ClientTest_DpwsClient_Properties()
        {
            /// <summary>
            /// 1. Verifies each of the proerties of a DPWSClient object
            /// 2. Sets and re-verifies for simple properties, checks input checking 
            /// on properties that currently support it.
            /// </summary>
            ///
            ProtocolVersion v = new ProtocolVersion10();
            bool testResult = true;
            int port = 50002;
            DpwsClient testClient = new DpwsClient(new WS2007HttpBinding(new HttpTransportBindingConfig("urn:uuid:" + Guid.NewGuid().ToString(), port)), v);
            try
            {
                Log.Comment("EndpointAddress");
                if (testClient.EndpointAddress != null)
                    if (testClient.EndpointAddress.GetType() != Type.GetType("System.String"))
                        throw new Exception("EndpointAddress wrong type");

                String uri = v.AnonymousUri;
                testClient.EndpointAddress = uri;
                if (testClient.EndpointAddress.GetType() != Type.GetType("System.String"))
                    throw new Exception("EndpointAddress wrong type after set");

                if (testClient.EndpointAddress != uri)
                    throw new Exception("EndpointAddress bad data");

                try
                {
                    testClient.EndpointAddress = " test";
                    throw new Exception("EndpointAddress failed to prevent bad data input");
                }
                catch (System.ArgumentException) { }

                Log.Comment("IgnoreRequestFromThisIP");
                if (testClient.IgnoreRequestFromThisIP.GetType() !=
                    Type.GetType("System.Boolean"))
                    throw new Exception("IgnoreRequestFromThisIP wrong type");

                testClient.IgnoreRequestFromThisIP = true;

                if (!testClient.IgnoreRequestFromThisIP)
                    throw new Exception("IgnoreRequestFromThisIP wrong data after set");

                Log.Comment("ServiceOperations");
                if (testClient.ServiceOperations != null)
                    if (testClient.ServiceOperations.GetType() !=
                        Type.GetType("Ws.Services.WsServiceOperations"))
                        throw new Exception("ServiceOperations wrong type");

                Log.Comment("TransportAddress");
                if (testClient.TransportAddress != null)
                    if (testClient.TransportAddress.GetType() != Type.GetType("System.String"))
                        throw new Exception("TransportAddress wrong type");

            }
            catch (Exception e)
            {
                testResult = false;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            finally
            {
                if( testClient != null) 
                    testClient.Dispose();
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults ClientTest_DpwsServiceType()
        {
            /// <summary>
            /// 1. Verifies each of the proerties of a DpwsServiceType object
            /// </summary>
            ///

            bool testResult = true;
            try
            {
                String typeName = "typeName";
                String namespaceUri = "namespaceUri";

                DpwsServiceType testDST = new DpwsServiceType(typeName, namespaceUri);

                Log.Comment("NamespaceUri");
                if (testDST.NamespaceUri != null)
                    if (testDST.NamespaceUri.GetType() != Type.GetType("System.String"))
                        throw new Exception("NamespaceUri wrong type");

                if (testDST.NamespaceUri != namespaceUri)
                    throw new Exception("NamespaceUri bad data");


                Log.Comment("TypeName");
                if (testDST.TypeName != null)
                    if (testDST.TypeName.GetType() != Type.GetType("System.String"))
                        throw new Exception("TypeName wrong type");

                if (testDST.TypeName != typeName)
                    throw new Exception("TypeName bad data");

            }
            catch (Exception e)
            {
                testResult = false;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults ClientTest_DpwsServiceTypes()
        {
            /// <summary>
            /// 1. Verifies the properties of a DpwsServiceTypes object
            /// 2. Adds elements to it
            /// 3. Re-verifies
            /// 4. Empties the object
            /// 5. Re-verifies
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                DpwsServiceTypes testDSTs = new DpwsServiceTypes();

                if (testDSTs.Count != 0)
                    throw new Exception("Count did not set correctly on new");

                testDSTs.Clear();

                if (testDSTs.Count != 0)
                    throw new Exception("Count did not set correctly after new ... clear");

                String typeName = "typeName";
                String namespaceUri = "namespaceUri";

                DpwsServiceType testDST1 = new DpwsServiceType(typeName, namespaceUri);

                String typeName2 = "typeName2";
                String namespaceUri2 = "namespaceUri2";

                DpwsServiceType testDST2 = new DpwsServiceType(typeName2, namespaceUri2);

                testDSTs.Add(testDST1);

                testDSTs.Add(testDST2);

                if (testDSTs.Count != 2)
                    throw new Exception("Count did not set correctly on new");


                testDSTs.Remove(testDST1);

                if (testDSTs.Count != 1)
                    throw new Exception("Count did not set correctly after remove");

                testDSTs.Clear();

                if (testDSTs.Count != 0)
                    throw new Exception("Count did not set correctly after new ... clear");
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
