/*---------------------------------------------------------------------
* FaultsTests.cs - file description
* Version: 1.0
* Author: REDMOND\a-grchat
* Created: 11/14/2007 10:27:20 AM 
* ---------------------------------------------------------------------*/

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using Dpws.Device;
using Ws.Services.Xml;

namespace Microsoft.SPOT.Platform.Tests
{
    public class XmlTests : IMFTestInterface
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
        public MFTestResults XmlTest_WsXmlAttribute()
        {
            /// <summary>
            /// 1. Gets and verifies each of the properties of a WsXmlAttribute object
            /// 2. Sets and re-verifies all properties
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                WsXmlAttribute testWXA = new WsXmlAttribute();

                Log.Comment("LocalName");
                if (testWXA.LocalName != null)
                    if (testWXA.LocalName.GetType() !=
                        Type.GetType("System.String"))
                        throw new Exception("LocalName wrong type");

                testWXA.LocalName = "test datum 1";

                if (testWXA.LocalName.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("LocalName wrong type after set");

                if (testWXA.LocalName != "test datum 1")
                    throw new Exception("LocalName wrong data");

                Log.Comment("NamespaceURI");
                if (testWXA.NamespaceURI != null)
                    if (testWXA.NamespaceURI.GetType() !=
                        Type.GetType("System.String"))
                        throw new Exception("NamespaceURI wrong type");

                testWXA.NamespaceURI = "test datum 3";

                if (testWXA.NamespaceURI.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("NamespaceURI wrong type after set");

                if (testWXA.NamespaceURI != "test datum 3")
                    throw new Exception("NamespaceURI wrong data");

                Log.Comment("Prefix");
                if (testWXA.Prefix != null)
                    if (testWXA.Prefix.GetType() !=
                        Type.GetType("System.String"))
                        throw new Exception("Prefix wrong type");

                testWXA.Prefix = "test datum 4";

                if (testWXA.Prefix.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("Prefix wrong type after set");

                if (testWXA.Prefix != "test datum 4")
                    throw new Exception("Prefix wrong data");

                Log.Comment("Value");
                if (testWXA.Value != null)
                    if (testWXA.Value.GetType() !=
                        Type.GetType("System.String"))
                        throw new Exception("Value wrong type");

                testWXA.Value = "test datum 5";

                if (testWXA.Value.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("Value wrong type after set");

                if (testWXA.Value != "test datum 5")
                    throw new Exception("Value wrong data");
            }
            catch (Exception e)
            {
                testResult = false;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }


        [TestMethod]
        public MFTestResults XmlTest_WsXmlAttributeCollection()
        {
            /// <summary>
            /// 1. Verifies the properties of a WsXmlAttributes object
            /// 2. Adds elements to it
            /// 3. Re-verifies
            /// 4. Empties the object
            /// 5. Re-verifies
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                WsXmlAttributeCollection testWXAs = new WsXmlAttributeCollection();

                if (testWXAs.Count != 0)
                    throw new Exception("Count did not set correctly on new");

                testWXAs.RemoveAll();

                if (testWXAs.Count != 0)
                    throw new Exception("Count did not set correctly after new ... clear");

                testWXAs.Append(new WsXmlAttribute());

                WsXmlAttribute testWXA = new WsXmlAttribute();
                testWXAs.Append(testWXA);

                if (testWXAs.Count != 2)
                    throw new Exception("Count did not set correctly on new");

                testWXAs.RemoveAll();

                if (testWXAs.Count != 0)
                    throw new Exception("Count did not set correctly after new ... clear");

            }
            catch (Exception e)
            {
                testResult = false;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults XmlTest_WsXmlElement()
        {
            /// <summary>
            /// 1. Gets and verifies each of the properties of a WsXmlElement object
            /// 2. Sets and re-verifies all properties
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                WsXmlElement testWXD = new WsXmlElement();

                Log.Comment("LocalName");
                if (testWXD.LocalName != null)
                    if (testWXD.LocalName.GetType() !=
                        Type.GetType("System.String"))
                        throw new Exception("LocalName wrong type");

                testWXD.LocalName = "test datum 1";

                if (testWXD.LocalName.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("LocalName wrong type after set");

                if (testWXD.LocalName != "test datum 1")
                    throw new Exception("LocalName wrong data");


                 Log.Comment("NamespaceURI");
                if (testWXD.NamespaceURI != null)
                    if (testWXD.NamespaceURI.GetType() !=
                        Type.GetType("System.String"))
                        throw new Exception("NamespaceURI wrong type");

                testWXD.NamespaceURI = "test datum 3";

                if (testWXD.NamespaceURI.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("NamespaceURI wrong type after set");

                if (testWXD.NamespaceURI != "test datum 3")
                    throw new Exception("NamespaceURI wrong data");

                Log.Comment("Prefix");
                if (testWXD.Prefix != null)
                    if (testWXD.Prefix.GetType() !=
                        Type.GetType("System.String"))
                        throw new Exception("Prefix wrong type");

                testWXD.Prefix = "test datum 4";

                if (testWXD.Prefix.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("Prefix wrong type after set");

                if (testWXD.Prefix != "test datum 4")
                    throw new Exception("Prefix wrong data");

                Log.Comment("Value");
                if (testWXD.Value != null)
                    if (testWXD.Value.GetType() !=
                        Type.GetType("System.String"))
                        throw new Exception("Value wrong type");

                testWXD.Value = "test datum 5";

                if (testWXD.Value.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("Value wrong type after set");

                if (testWXD.Value != "test datum 5")
                    throw new Exception("Value wrong data");

                Log.Comment("Attributes");
                if (testWXD.Attributes != null)
                    if (testWXD.Attributes.GetType() !=
                        Type.GetType("Ws.Services.Xml.WsXmlAttributeCollection"))
                        throw new Exception("Attributes wrong type");

                testWXD.AppendChild(new WsXmlNode());

                if (testWXD.ChildNodes[0].ToString() != new WsXmlNode().ToString())
                    throw new Exception("ChildNodes wrong data");

                if (testWXD.ChildNodes.Count != 1)
                    throw new Exception("Incorrect amount of child nodes.  should be 1 node");

            }
            catch (Exception e)
            {
                testResult = false;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults XmlTest_WsXmlNamespace()
        {
            /// <summary>
            /// 1. Gets and verifies each of the properties of a WsXmlNamespace object
            /// 2. Sets and re-verifies all properties
            /// </summary>
            ///

            bool testResult = true;
            try
            {
                WsXmlNamespace testWXN = new WsXmlNamespace("test datum 1", "test datum 2");

                Log.Comment("Prefix");
                if (testWXN.Prefix != null)
                    if (testWXN.Prefix.GetType() !=
                        Type.GetType("System.String"))
                        throw new Exception("Prefix wrong type");

                if (testWXN.Prefix.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("Prefix wrong type after set");

                if (testWXN.Prefix != "test datum 1")
                    throw new Exception("Prefix wrong data");

                Log.Comment("NamespaceURI");
                if (testWXN.NamespaceURI != null)
                    if (testWXN.NamespaceURI.GetType() !=
                        Type.GetType("System.String"))
                        throw new Exception("NamespaceURI wrong type");

                if (testWXN.NamespaceURI.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("NamespaceURI wrong type after set");

                if (testWXN.NamespaceURI != "test datum 2")
                    throw new Exception("NamespaceURI wrong data");

                Log.Comment("2 Arg Ctor");
                testWXN = new WsXmlNamespace("test datum 3", "test datum 4");

                Log.Comment("Prefix");
                if (testWXN.Prefix != null)
                    if (testWXN.Prefix.GetType() !=
                        Type.GetType("System.String"))
                        throw new Exception("Prefix wrong type");

                if (testWXN.Prefix != "test datum 3")
                    throw new Exception("Prefix wrong data");

                Log.Comment("NamespaceURI");
                if (testWXN.NamespaceURI != null)
                    if (testWXN.NamespaceURI.GetType() !=
                        Type.GetType("System.String"))
                        throw new Exception("NamespaceURI wrong type");

                if (testWXN.NamespaceURI != "test datum 4")
                    throw new Exception("NamespaceURI wrong data");


            }
            catch (Exception e)
            {
                testResult = false;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }


        [TestMethod]
        public MFTestResults XmlTest_XmlNode()
        {
            /// <summary>
            /// 1. Gets and verifies each of the properties of a WsXmlNode object
            /// 2. Sets and re-verifies all properties
            /// See 18325 for more info re: childNodes
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                WsXmlNode testWXN = new WsXmlNode();

                Log.Comment("LocalName");
                if (testWXN.LocalName != null)
                    if (testWXN.LocalName.GetType() !=
                        Type.GetType("System.String"))
                        throw new Exception("LocalName wrong type");

                testWXN.LocalName = "test datum 1";

                if (testWXN.LocalName.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("LocalName wrong type after set");

                if (testWXN.LocalName != "test datum 1")
                    throw new Exception("LocalName wrong data");


                Log.Comment("NamespaceURI");
                if (testWXN.NamespaceURI != null)
                    if (testWXN.NamespaceURI.GetType() !=
                        Type.GetType("System.String"))
                        throw new Exception("NamespaceURI wrong type");

                testWXN.NamespaceURI = "test datum 3";

                if (testWXN.NamespaceURI.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("NamespaceURI wrong type after set");

                if (testWXN.NamespaceURI != "test datum 3")
                    throw new Exception("NamespaceURI wrong data");

                Log.Comment("Prefix");
                if (testWXN.Prefix != null)
                    if (testWXN.Prefix.GetType() !=
                        Type.GetType("System.String"))
                        throw new Exception("Prefix wrong type");

                testWXN.Prefix = "test datum 4";

                if (testWXN.Prefix.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("Prefix wrong type after set");

                if (testWXN.Prefix != "test datum 4")
                    throw new Exception("Prefix wrong data");

                Log.Comment("Value");
                if (testWXN.Value != null)
                    if (testWXN.Value.GetType() !=
                        Type.GetType("System.String"))
                        throw new Exception("Value wrong type");

                testWXN.Value = "test datum 5";

                if (testWXN.Value.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("Value wrong type after set");

                if (testWXN.Value != "test datum 5")
                    throw new Exception("Value wrong data");

                Log.Comment("Attributes");
                if (testWXN.Attributes != null)
                    if (testWXN.Attributes.GetType() !=
                        Type.GetType("Ws.Services.Xml.WsXmlAttributeCollection"))
                        throw new Exception("Value wrong type");

                Log.Comment("ChildNodes");
                if (testWXN.ChildNodes != null)
                    if (testWXN.ChildNodes.GetType() !=
                        Type.GetType("Ws.Services.Xml.WsXmlNodeList"))
                        throw new Exception("ChildNodes wrong type");
            }
            catch (Exception e)
            {
                testResult = false;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }




        [TestMethod]
        public MFTestResults XmlTest5_WsXmlNodeList()
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
                WsXmlNodeList testWXNs = new WsXmlNodeList();

                if (testWXNs.Count != 0)
                    throw new Exception("Count did not set correctly on new");

                testWXNs.Add(new WsXmlNode());

                WsXmlNode testWXN = new WsXmlNode();
                testWXN.LocalName = "testWXN local name";
                testWXNs.Add(testWXN);

                if (testWXNs.Count != 2)
                    throw new Exception("Count did not set correctly on new");

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
