/*---------------------------------------------------------------------
* DiscoveryTests.cs - file description
* Version: 1.0
* Author: REDMOND\a-grchat
* Created: 11/2/2007 3:58:04 PM 
* ---------------------------------------------------------------------*/

using Dpws.Client;
using Dpws.Client.Discovery;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System;
using System.IO;
using System.Xml;
using Ws.Services;
using Ws.Services.WsaAddressing;

namespace Microsoft.SPOT.Platform.Tests
{
    public class DiscoveryTests : IMFTestInterface
    {
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
        public MFTestResults DiscoveryTest_DpwsServiceDescription()
        {
            /// <summary>
            /// 1. Verifies each of the proerties of a ByeEventArgs object
            /// 2. Sets and re-verifies for simple properties.
            /// </summary>
            ///

            ProtocolVersion v = new ProtocolVersion10();
            bool testResult = true;
            try
            {
                WsWsaEndpointRef ep = new WsWsaEndpointRef(new System.Uri(v.AnonymousUri));
                DpwsServiceTypes st = new DpwsServiceTypes();
                DpwsServiceDescription testBEA = new DpwsServiceDescription(ep, st);

                Log.Comment("Endpoint");
                if (testBEA.Endpoint != null)
                    if (testBEA.Endpoint.GetType() != Type.GetType("Ws.Services.WsaAddressing.WsWsaEndpointRef"))
                        throw new Exception("Endpoint wrong type");

                if (testBEA.Endpoint != ep)
                    throw new Exception("Endpoint bad data");

                Log.Comment("MetadataVersion");
                if (testBEA.MetadataVersion != null)
                    if (testBEA.MetadataVersion.GetType() != Type.GetType("System.String"))
                        throw new Exception("MetadataVersion wrong type");

                if (testBEA.MetadataVersion != null)
                    throw new Exception("MetadataVersion bad data");

                Log.Comment("XAddrs");
                if (testBEA.XAddrs != null)
                    if (testBEA.XAddrs.GetType() != Type.GetType("System.String"))
                        throw new Exception("XAddrs wrong type");

                if (testBEA.XAddrs != null)
                    throw new Exception("XAddrs bad data");

                Log.Comment("ServiceTypes");
                if (testBEA.ServiceTypes != null)
                    if (testBEA.ServiceTypes.GetType() != Type.GetType("Dpws.Client.DpwsServiceTypes"))
                        throw new Exception("ServiceTypes wrong type");

                if (testBEA.ServiceTypes.Count != 0)
                    throw new Exception("ServiceTypes bad data");


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
