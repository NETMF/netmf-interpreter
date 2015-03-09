/*---------------------------------------------------------------------
* FaultsTests.cs - file description
* Version: 1.0
* Author: REDMOND\a-grchat
* Created: 11/14/2007 10:27:20 AM 
* ---------------------------------------------------------------------*/

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using Ws.Services;
using Ws.Services.WsaAddressing;
using Dpws.Device;

namespace Microsoft.SPOT.Platform.Tests
{
    public class WsaAddressingTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }


        [TestMethod]
        public MFTestResults WsaAddressingtest_WsWsaEndpointRef()
        {
            /// <summary>
            /// 1. Gets and verifies each of the properties of a WsWsaEndpointRef object
            /// 2. Sets and re-verifies all properties
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                WsWsaEndpointRef testWWER = new WsWsaEndpointRef(new System.Uri(WsWellKnownUri.SoapNamespaceUri));

                Log.Comment("Address");
                if (testWWER.Address != null)
                    if (testWWER.Address.GetType() !=
                        Type.GetType("System.Uri"))
                        throw new Exception("Address wrong type");

                if (testWWER.Address.AbsoluteUri != new System.Uri(WsWellKnownUri.SoapNamespaceUri).AbsoluteUri)
                    throw new Exception("Address wrong data");


                Log.Comment("RefProperties");
                if (testWWER.RefProperties != null)
                    if (testWWER.RefProperties.GetType() !=
                        Type.GetType("Ws.Services.Xml.WsXmlNodeList"))
                        throw new Exception("RefProperties wrong type");

                if (testWWER.RefProperties.ToString() != new Ws.Services.Xml.WsXmlNodeList().ToString())
                    throw new Exception("RefProperties wrong data");


                Log.Comment("RefParameters");
                if (testWWER.RefParameters != null)
                    if (testWWER.RefParameters.GetType() !=
                        Type.GetType("Ws.Services.Xml.WsXmlNodeList"))
                        throw new Exception("RefParameters wrong type");

                 if (testWWER.RefParameters.ToString() != new Ws.Services.Xml.WsXmlNodeList().ToString())
                    throw new Exception("RefParameters wrong data");

            }
            catch (Exception e)
            {
                testResult = false;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }


        [TestMethod]
        public MFTestResults WsaAddressingtest_WsWsaEndpointRefs()
        {
            /// <summary>
            /// 1. Verifies the properties of a WsWsaEndpointRefs object
            /// 2. Adds elements to it
            /// 3. Re-verifies
            /// 4. Empties the object
            /// 5. Re-verifies
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                WsWsaEndpointRefs testWWERs = new WsWsaEndpointRefs();

                if (testWWERs.Count != 0)
                    throw new Exception("Count did not set correctly on new");

                testWWERs.Add(new WsWsaEndpointRef(new System.Uri(WsWellKnownUri.SchemaNamespaceUri)));
                testWWERs.Add(new WsWsaEndpointRef(new System.Uri(WsWellKnownUri.SoapNamespaceUri)));

                if (testWWERs.Count != 2)
                    throw new Exception("Count did not set correctly on new");

                testWWERs.Clear();

                if (testWWERs.Count != 0)
                    throw new Exception("After removing count is not back to zero");


                testWWERs.Add(new WsWsaEndpointRef(new System.Uri(WsWellKnownUri.SchemaNamespaceUri)));
                WsWsaEndpointRef ep = new WsWsaEndpointRef(new System.Uri(WsWellKnownUri.SoapNamespaceUri));
                testWWERs.Add(ep);
                testWWERs.Remove(ep);

                if (testWWERs.Count != 1)
                    throw new Exception("Remove did not correctly remove the item");
            }
            catch (Exception e)
            {
                testResult = false;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }



        [TestMethod]
        public MFTestResults WsaAddressingtest_WsWsaHeader()
        {
            /// <summary>
            /// 1. Gets and verifies each of the properties of a WsWsaHeader object
            /// 2. Sets and re-verifies all properties
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                WsWsaHeader testWWH = new WsWsaHeader("Action", "RelatesTo", WsWellKnownUri.SchemaNamespaceUri, WsWellKnownUri.SchemaNamespaceUri, WsWellKnownUri.SchemaNamespaceUri, new Ws.Services.Xml.WsXmlNodeList());

                Log.Comment("Action");
                if (testWWH.Action != null)
                    if (testWWH.Action.GetType() !=
                        Type.GetType("System.String"))
                        throw new Exception("Action wrong type");

                if (testWWH.Action != "Action")
                    throw new Exception("Action wrong data");

                Log.Comment("FaultTo");
                if (testWWH.FaultTo != null)
                    if (testWWH.FaultTo.GetType() !=
                        Type.GetType("System.String"))
                        throw new Exception("FaultTo wrong type");

                if (testWWH.RelatesTo != "RelatesTo")
                    throw new Exception("RelatesTo wrong data");

                Log.Comment("ReplyTo");
                if (testWWH.ReplyTo != null)
                    if (testWWH.ReplyTo.GetType() !=
                        Type.GetType("Ws.Services.WsaAddressing.WsWsaEndpointRef"))
                        throw new Exception("RefProperties wrong type");

                if (testWWH.ReplyTo.ToString() != new WsWsaEndpointRef(new System.Uri(WsWellKnownUri.SchemaNamespaceUri)).ToString())
                    throw new Exception("From wrong data");

               
                Log.Comment("From");
                if (testWWH.From != null)
                    if (testWWH.From.GetType() !=
                        Type.GetType("Ws.Services.WsaAddressing.WsWsaEndpointRef"))
                        throw new Exception("From wrong type");

                if (testWWH.From.ToString() != new WsWsaEndpointRef(new System.Uri(WsWellKnownUri.SchemaNamespaceUri)).ToString())
                    throw new Exception("From wrong data");


                Log.Comment("To");
                if (testWWH.To != null)
                    if (testWWH.To.GetType() !=
                        Type.GetType("System.String"))
                        throw new Exception("To wrong type");

                if (testWWH.To != WsWellKnownUri.SchemaNamespaceUri)
                    throw new Exception("To wrong data");

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
