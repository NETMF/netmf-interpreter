/*---------------------------------------------------------------------
* FaultsTests.cs - file description
* Version: 1.0
* Author: REDMOND\a-grchat
* Created: 11/14/2007 10:27:20 AM 
* ---------------------------------------------------------------------*/

using System;
using System.Text;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.IO;
using System.Xml;

namespace Microsoft.SPOT.Platform.Tests
{
    /*
     * This is an implementation of the Stream class, it does not read from an external source.
     * It creates an internal XML document of fragment and then streams it out on calls to Read.
     * 
     */
    public class TestStream : System.IO.Stream
    {
        public TestStream()
        {
            m_CanRead = true;
            m_CanSeek = false;
            m_CanWrite = false;
            //Debug.Print("Creating XML data");
            m_Xml = BuildXML();
            //Debug.Print(byteArrToString(m_Xml));
            Position = 0;
        }

        public void toFragment()
        {
            m_Xml = BuildXMLFragment();
            //Debug.Print(byteArrToString(m_Xml));                                                 
        }

        public void toNodeTypes()
        {
            m_Xml = BuildXMLNodeTypes();
            //Debug.Print(byteArrToString(m_Xml));
        }

        private bool m_CanRead;
        private bool m_CanSeek;
        private bool m_CanWrite;
        private long m_Position;
        private byte[] m_Xml;

        public override bool CanRead
        {
            get { return m_CanRead; }
        }


        public override bool CanSeek
        {
            get { return m_CanSeek; }
        }

        public override bool CanWrite
        {
            get { return m_CanWrite; }
        }

        public override void Flush()
        {
            m_Xml = null;
            m_Xml = new byte[1];
            Debug.Print("Recreating XML data");
            m_Xml = BuildXML();
            Position = 0;
        }

        public override long Length
        {
            get
            {
                return m_Xml.Length;
            }
        }

        public override long Position
        {
            get
            {
                return m_Position;
            }
            set
            {
                m_Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer.Length < offset + count)
            {
                return 0;
            }
            for (int i = 0; i < count; i++)
            {
                if (m_Position >= m_Xml.Length)
                    return i;
                buffer[offset + i] = m_Xml[(int)m_Position];
                m_Position++;
            }
            return count;
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            Debug.Print("CanSeek is set to false, this method should not be called");
            throw new Exception("The Seek method is not implemented.");
        }

        public override void SetLength(long value)
        {
            throw new Exception("The SetLength method is not implemented.");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Debug.Print("CanWrite is set to false, this method should not be called");
            throw new Exception("The Write method is not implemented.");
        }

        private static byte[] BuildXMLNodeTypes()
        {
            MemoryStream soapStream = new MemoryStream();
            XmlWriter xmlWriter = XmlWriter.Create(soapStream);

            xmlWriter.WriteRaw("<?xml version='1.0'?>");
            xmlWriter.WriteRaw("<TestElement1>");
            xmlWriter.WriteRaw("<![CDATA[my escaped text]]>");
            xmlWriter.WriteRaw("<!-- my comment -->");
            xmlWriter.WriteRaw("<item>");
            xmlWriter.WriteRaw("sampleText");
            xmlWriter.WriteRaw("</item>");
            xmlWriter.WriteRaw("<?pi test?>");
            xmlWriter.WriteRaw("<space xml:space=\"preserve\">");
            xmlWriter.WriteRaw("               ");
            xmlWriter.WriteRaw("</space>");
            xmlWriter.WriteRaw("<!error>");

            xmlWriter.Flush();
            byte[] soapBuffer = soapStream.ToArray();
            xmlWriter.Close();
            return soapBuffer;
        }

        private static byte[] BuildXMLFragment()
        {
            MemoryStream soapStream = new MemoryStream();
            XmlWriter xmlWriter = XmlWriter.Create(soapStream);

            xmlWriter.WriteStartElement("TestElement1");
            xmlWriter.WriteStartElement("ChildOfTestElement1");
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("TestElement2");
            xmlWriter.WriteString("TestElement2Data");
            xmlWriter.WriteEndElement();

            xmlWriter.Flush();
            byte[] soapBuffer = soapStream.ToArray();
            xmlWriter.Close();
            return soapBuffer;
        }


        private static byte[] BuildXML()
        {
            MemoryStream soapStream = new MemoryStream();
            XmlWriter xmlWriter = XmlWriter.Create(soapStream);

            xmlWriter.WriteStartElement("root");
            xmlWriter.WriteAttributeString("XT", "TestLocalName", "TestNS", "TestValue");
            xmlWriter.WriteStartElement("TestElement1");
            xmlWriter.WriteStartElement("ChildOfTestElement1");
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("TestElement2");
            xmlWriter.WriteString("TestElement2Data");
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();

            xmlWriter.Flush();
            byte[] soapBuffer = soapStream.ToArray();

            Log.Comment("Create Xml Doc: = \"" + new string(System.Text.UTF8Encoding.UTF8.GetChars(soapBuffer)) + "\"");
            xmlWriter.Close();
            return soapBuffer;
        }

        public static string byteArrToString(byte[] byteInput)
        {
            string stringOutput = "";
            for (int i = 0; i < byteInput.Length; i++)
            {
                stringOutput += (char)byteInput[i];
            }
            return stringOutput;
        }
    }


    public class SPOTXmlTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for SPOTXmlTests tests.");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        //int.MaxValue causes issues, this is used to generate random numbers for tests 
        public const int LARGE_NUM = 20000;

        [TestMethod]
        public MFTestResults SPOTXml_XmlReader_MethodsTest3()
        {
            /// <summary>
            /// Tests all of the XmlReader methods in SPOT.Xml
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                Log.Comment("Creating XmlReader with Stream");
                XmlReader testReader = XmlReader.Create(new TestStream());

                Log.Comment("Close");
                testReader.Close();
                if (testReader.ReadState != ReadState.Closed)
                {
                    testResult = false;
                    Log.Exception("Close did not function properly");
                }

                testReader = XmlReader.Create(new TestStream());

                Log.Comment("GetAttribute");
                try
                {
                    if("" == testReader.GetAttribute(0))
                    {
                        testResult = false;
                        Log.Exception("GetAttribute succeeded with 0 attributes");
                    }
                }
                catch (ArgumentOutOfRangeException e) {
                    Log.Exception("ArgumentOutOfRangeException caught in SPOTXml_XmlReader_MethodsTest3::GetAttribute(0): ", e);
                }

                //  Add Additional tests for the overloaded GetAttribute
                //  public abstract string GetAttribute(string name);
                //  public abstract string GetAttribute(string name, string namespaceURI);              

                Log.Comment("IsStartElement");
                if (!testReader.IsStartElement())
                {
                    testResult = false;
                    Log.Exception("IsStartElement returned false before read");
                }

                testReader.Read();
                if (!testReader.IsStartElement())
                {
                    testResult = false;
                    Log.Exception("IsStartElement returned false after read to 1st elt");
                }

                testReader.Read();
                testReader.Read();
                if (testReader.IsStartElement())
                {
                    testResult = false;
                    Log.Exception("IsStartElement returned true after read to an end elt");
                }

                testReader = XmlReader.Create(new TestStream());

                if (!testReader.IsStartElement("root"))
                {
                    testResult = false;
                    Log.Exception("IsStartElement(string) returned false for root");
                }

                if (testReader.IsStartElement("TestElement1"))
                {
                    testResult = false;
                    Log.Exception("IsStartElement(string) returned true for TestElement1");
                }


                if (testReader.IsStartElement("FakeData"))
                {
                    testResult = false;
                    Log.Exception("IsStartElement(string) returned true for FakeData");
                }

                testReader = XmlReader.Create(new TestStream());

                if (!testReader.IsStartElement("root", ""))
                {
                    testResult = false;
                    Log.Exception("IsStartElement(string,string) returned false for root, namespace");
                }

                if (testReader.IsStartElement("root", "FakeNamespaceURI"))
                {
                    testResult = false;
                    Log.Exception("IsStartElement(string,string) returned true for root, FakeNamespaceURI");
                }

                if (testReader.IsStartElement("TestElement1", ""))
                {
                    testResult = false;
                    Log.Exception("IsStartElement(string,string) returned true for TestElement1");
                }


                testReader = XmlReader.Create(new TestStream());

                if (testReader.LookupNamespace("XT") != null)
                {
                    testResult = false;
                    Log.Exception("LookupNamespace returned incorrect data");
                }

                testResult &= testReader.Read();

                if (testReader.LookupNamespace("XT") != "TestNS")
                {
                    testResult = false;
                    Log.Exception("LookupNamespace returned incorrect data after read");
                }

                if (testReader.LookupNamespace("") != "")
                {
                    testResult = false;
                    Log.Exception("LookupNamespace returned incorrect data for bad arg after read");
                }

                testReader = XmlReader.Create(new TestStream());

                Log.Comment("MoveToX methods");
                testReader.MoveToContent();
                if (testReader.Name != "root")
                {
                    testResult = false;
                    Log.Exception("MoveToContent went to wrong node");
                }

                testReader.MoveToElement();
                if (testReader.Name != "root")
                {
                    testResult = false;
                    Log.Exception("MoveToElementwent to wrong node");
                }

                testResult &= testReader.MoveToFirstAttribute();
                if (testReader.Name != "xmlns:XT")
                {
                    testResult = false;
                    Log.Exception("MoveToFirstAttribute went to wrong node");
                }

                testResult &= testReader.MoveToNextAttribute();
                if (testReader.Name != "XT:TestLocalName")
                {
                    testResult = false;
                    Log.Exception("MoveToFirstAttribute went to wrong node");
                }

                Log.Comment("ReadElementString");
                testReader = XmlReader.Create(new TestStream());

                //TODO: Exception testing here under investigation
                //testReader.ReadElementString();

                testReader.Read();
                testReader.Read();
                testReader.Read();
                if (testReader.ReadElementString() != "")
                {
                    testResult = false;
                    Log.Exception("ReadElementString returned bad data for empty node");
                }
                testReader.Read();
                if (testReader.ReadElementString() != "TestElement2Data")
                {
                    testResult = false;
                    Log.Exception("ReadElementString returned bad data for full node");
                }

                testReader = XmlReader.Create(new TestStream());

                testReader.Read();
                testReader.Read();
                testReader.Read();
                if (testReader.ReadString() != "")
                {
                    testResult = false;
                    Log.Exception("ReadString returned bad data for empty node");
                }
                testReader.Read();
                testReader.Read();
                if (testReader.ReadString() != "TestElement2Data")
                {
                    testResult = false;
                    Log.Exception("ReadString returned bad data for full node");
                }

                testReader = XmlReader.Create(new TestStream());

                testReader.Read();
                testReader.Read();
                testReader.Read();
                if (testReader.ReadElementString("ChildOfTestElement1") != "")
                {
                    testResult = false;
                    Log.Exception("ReadElementString returned bad data for empty node");
                }
                testReader.Read();
                if (testReader.ReadElementString("TestElement2") != "TestElement2Data")
                {
                    testResult = false;
                    Log.Exception("ReadElementString returned bad data for full node");
                }

                testReader = XmlReader.Create(new TestStream());

                testReader.Read();
                testReader.Read();
                testReader.Read();
                if (testReader.ReadElementString("ChildOfTestElement1", "") != "")
                {
                    testResult = false;
                    Log.Exception("ReadElementString returned bad data for empty node");
                }
                testReader.Read();
                if (testReader.ReadElementString("TestElement2", "") != "TestElement2Data")
                {
                    testResult = false;
                    Log.Exception("ReadElementString returned bad data for full node");
                }

                testReader = XmlReader.Create(new TestStream());
                //Add tests for overloaded ReadStartElement
                // public virtual void ReadStartElement(string name);
                // public virtual void ReadStartElement(string localname, string ns);
                testReader.ReadStartElement();
                if (testReader.Name != "TestElement1")
                {
                    testResult = false;
                    Log.Exception("ReadStartElement failed on start element");
                }

                try
                {
                    testReader.ReadEndElement();
                    testResult = false;
                    Log.Comment("FAILURE - ReadEndElement succeded on Start element ");
                }
                catch (XmlException e) {
                   Log.Exception("XMLException - ReadEndElement succeeded on Start element ", e);
                }
                testReader.Read();
                testReader.Read();
                testReader.ReadEndElement();
                if (testReader.Name != "TestElement2")
                {
                    testResult = false;
                    Log.Exception("ReadEndElement failed on end element");
                }

                testReader.Read();

                try
                {
                    testReader.ReadStartElement();
                    testResult = false;
                    Log.Comment("FAILURE - ReadStartElement succeeded on end element ");
                } 
                catch( XmlException e) {
                    Log.Exception("ReadStartElement failed on end element", e);
                }
      

                testReader = XmlReader.Create(new TestStream());
                //Add tests for the overloaded ReadToDescendant
                // public virtual bool ReadToDescendant(string localName, string namespaceURI);
                testReader.ReadToDescendant("ChildOfTestElement1");
                if (testReader.Name != "ChildOfTestElement1")
                {
                    testResult = false;
                    Log.Exception("ReadToDescendant read to wrong location");
                }

                testReader = XmlReader.Create(new TestStream());
                // public virtual bool ReadToDescendant(string Name) 
                // passing in empty string. 
                try
                {
                    testReader.ReadToDescendant("");
                }
                catch (ArgumentException e)
                {
                    if (testReader.Name != "")
                    {
                        testResult = false;
                        Log.Exception("ReadToDescendant read to a location erroneously ", e);
                    }
                }
                

                testReader = XmlReader.Create(new TestStream());
                testReader.ReadToDescendant("ChildOfTestElement2");
                if (testReader.Name != "")
                {
                    testResult = false;
                    Log.Exception("ReadToDescendant read to a location erroneously");
                }

                testReader = XmlReader.Create(new TestStream());
                testReader.ReadToDescendant("ChildOfTestElement1", "");
                if (testReader.Name != "ChildOfTestElement1")
                {
                    testResult = false;
                    Log.Exception("ReadToDescendant with namesapce read to wrong location");
                }

                testReader = XmlReader.Create(new TestStream());
                testReader.ReadToDescendant("ChildOfTestElement2", "");
                if (testReader.Name != "")
                {
                    testResult = false;
                    Log.Exception("ReadToDescendant with namesapce read to a location erroneously");
                }

                testReader = XmlReader.Create(new TestStream());
                //Add tests for the overloaded ReadToDescendant
                // public virtual bool ReadToFollowing(string localName, string namespaceURI);

                testReader.ReadToFollowing("ChildOfTestElement1");
                if (testReader.Name != "ChildOfTestElement1")
                {
                    testResult = false;
                    Log.Exception("ReadToFollowing read to wrong location");
                }

                testReader = XmlReader.Create(new TestStream());
                testReader.ReadToFollowing("ChildOfTestElement2");
                if (testReader.Name != "")
                {
                    testResult = false;
                    Log.Exception("ReadToFollowing read to a location erroneously");
                }

                testReader = XmlReader.Create(new TestStream());
                testReader.ReadToFollowing("ChildOfTestElement1", "");
                if (testReader.Name != "ChildOfTestElement1")
                {
                    testResult = false;
                    Log.Exception("ReadToFollowing with namesapce read to wrong location");
                }

                testReader = XmlReader.Create(new TestStream());
                testReader.ReadToFollowing("ChildOfTestElement2", "");
                if (testReader.Name != "")
                {
                    testResult = false;
                    Log.Exception("ReadToFollowing with namesapce read to a location erroneously");
                }


                Log.Comment("ReadToNextSibling method");
                testReader = XmlReader.Create(new TestStream());
                testReader.Read();
                testReader.Read();
                if (!testReader.ReadToNextSibling("TestElement2"))
                {
                    testResult = false;
                    Log.Exception("ReadToNextSibling return false for TestElement2");
                }

                testReader = XmlReader.Create(new TestStream());
                testReader.Read();
                if (testReader.ReadToNextSibling("TestElement1", ""))
                {
                    testResult = false;
                    Log.Exception("Bug 57189: ReadToNextSibling return true for TestElement1");
                }

                testReader = XmlReader.Create(new TestStream());
                testReader.Read();
                testReader.Read();
                if (!testReader.ReadToNextSibling("TestElement2", ""))
                {
                    testResult = false;
                    Log.Exception("ReadToNextSibling return false for TestElement2");
                }


                Log.Comment("Skip");
                testReader = XmlReader.Create(new TestStream());
                testReader.Read();
                testReader.Read();
                testReader.Skip();
                if (testReader.Name != "TestElement2")
                {
                    testResult = false;
                    Log.Exception("Skip failed to advance to correct node");
                }
            }
            catch (Exception e)
            {
                testResult = false;
                Log.Exception("Incorrect exception caught: ", e);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        public bool charArrEquals(char[] c1, char[] c2)
        {
            if (c1.Length != c2.Length)
                return false;
            for (int i = 0; i < c1.Length; i++)
            {
                if (c1[i] != c2[i])
                    return false;
            }
            return true;
        }


        [TestMethod]
        public MFTestResults SPOTXml_EnumsTest4()
        {
            /// <summary>
            /// Tests all of the used enumerations in SPOT.Xml
            /// Creates XmlReader using XmlReader.Create(Stream input, XmlReaderSettings settings) and
            /// Sets the ConformanceLevel to Auto (by default it's Document)
            /// Hence, XmlReader should be created using XmlReader.Create(Stream input) 
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                XmlReaderSettings testSettings = new XmlReaderSettings();
                TestStream fragment = new TestStream();

                // Add readstate test for ReadState.Error
                XmlReader testReader = XmlReader.Create(fragment, testSettings);
                if (testReader.ReadState != ReadState.Initial)
                {
                    testResult = false;
                    Log.Exception("ReadState failed to set to Initial on initialization");
                }

                testReader.Read();
                if (testReader.ReadState != ReadState.Interactive)
                {
                    testResult = false;
                    Log.Exception("ReadState failed to set to Interactive on read");
                }

                while (testReader.Read()) { }
                if (testReader.ReadState != ReadState.EndOfFile)
                {
                    testResult = false;
                    Log.Exception("ReadState failed to set to EndOfFile after read");
                }

                testReader.Close();
                if (testReader.ReadState != ReadState.Closed)
                {
                    testResult = false;
                    Log.Exception("ReadState failed to set to Closed on close");
                }

                Log.Comment("XmlNodeType");
                //Current implementations of XmlReader in SPOT.Xml do not use following XmlNodeTypes 
                //Document, DocumentFragment, Entity, EndEntity or Notation
                //EntityReference, DocumentType          

                TestStream nodeTypes = new TestStream();
                nodeTypes.toNodeTypes();
                testReader = XmlReader.Create(nodeTypes, testSettings);

                if (testReader.NodeType != XmlNodeType.None)
                {
                    testResult = false;
                    Log.Exception("NodeType was not none before read");
                }

                Log.Comment("XmlDeclaration");
                testReader.Read();
                if (testReader.NodeType != XmlNodeType.XmlDeclaration)
                {
                    testResult = false;
                    Log.Exception("NodeType did not match XmlDeclaration node");
                }

                testReader.Read();
                if (testReader.NodeType != XmlNodeType.Element)
                {
                    testResult = false;
                    Log.Exception("NodeType did not match Element node");
                }

                Log.Comment("CDATA");
                testReader.Read();
                if (testReader.NodeType != XmlNodeType.CDATA)
                {
                    testResult = false;
                    Log.Exception("NodeType did not match CDATA node");
                }

                Log.Comment("Comment");
                testReader.Read();
                if (testReader.NodeType != XmlNodeType.Comment)
                {
                    testResult = false;
                    Log.Exception("NodeType did not match Comment node");
                }

                Log.Comment("Element");
                testReader.Read();
                if (testReader.NodeType != XmlNodeType.Element)
                {
                    testResult = false;
                    Log.Exception("NodeType did not match Element node");
                }

                Log.Comment("Text");
                testReader.Read();
                if (testReader.NodeType != XmlNodeType.Text)
                {
                    testResult = false;
                    Log.Exception("NodeType did not match Text node");
                }

                Log.Comment("EndElement");
                testReader.Read();
                if (testReader.NodeType != XmlNodeType.EndElement)
                {
                    testResult = false;
                    Log.Exception("NodeType did not match EndElement node");
                }

                Log.Comment("ProcessingInstruction");
                testReader.Read();
                if (testReader.NodeType != XmlNodeType.ProcessingInstruction)
                {
                    testResult = false;
                    Log.Exception("NodeType did not match ProcessingInstruction node");
                }

                Log.Comment("Element");
                testReader.Read();
                if (testReader.NodeType != XmlNodeType.Element)
                {
                    testResult = false;
                    Log.Exception("NodeType did not match Element node");
                }

                Log.Comment("SignificantWhitespace");
                testReader.Read();
                if (testReader.NodeType != XmlNodeType.SignificantWhitespace)
                {
                    testResult = false;
                    Log.Exception("NodeType did not match SignificantWhitespace node");
                }

                Log.Comment("EndElement");
                testReader.Read();
                if (testReader.NodeType != XmlNodeType.EndElement)
                {
                    testResult = false;
                    Log.Exception("NodeType did not match EndElement node");
                }

                try
                {
                    testReader.Read();
                }
                catch (Exception e)
                {
                    if (testReader.ReadState != ReadState.Error)
                    {
                        testResult = false;
                        Log.Exception("ReadState failed to set to Error on parsing error ", e);
                    }
                }
            }
            catch (Exception e)
            {
                testResult = false;
                Log.Exception("Incorrect exception caught: ", e);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SPOTXml_NameTableTest5()
        {
            /// <summary>
            /// Tests the NameTable class under SPOT.Xml
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                XmlReader testReader = XmlReader.Create(new TestStream());

                XmlNameTable testNT = new XmlNameTable();
                object root = testNT.Add("root");
                object element1 = testNT.Add("TestElement1");
                testReader.Read();
                object localName = testReader.LocalName;
                if (!(localName == root))
                {
                    Log.Exception("1st reading, expected local name '" + root + "' but got '" + localName + "'");
                    testResult = false;
                }
                testReader.Read();
                localName = testReader.LocalName;
                if (!(localName == element1))
                {
                    Log.Exception("2nd reading, expected local name '" + element1 + "' but got '" + localName + "'");
                    testResult = false;
                }

                testNT.Add("test1");

                if (testNT.Get("root").GetType() != Type.GetType("System.String"))
                {
                    testResult = false;
                    Log.Exception("Get(string) got wrong type");
                }

                if (testNT.Get("root") != "root")
                {
                    testResult = false;
                    Log.Exception("Get(string) got wrong data");
                }
            }
            catch (Exception e)
            {
                testResult = false;
                Log.Exception("Incorrect exception caught: ", e);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SPOTXml_AttributeValueTest6()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Tests Attribute vaues with single/double quotes");
            MemoryStream soapStream = new MemoryStream(BuildXMLAttributeValue());            
            XmlReader xmlReader = XmlReader.Create(soapStream);

            xmlReader.Read();

            try
            {
                xmlReader.Read();
            }
            catch (Exception e)
            {
                Log.Exception("Failure reading element with single quote attribute value, Caught : ", e);
                testResult = MFTestResults.Fail;
            }
            try
            {
                xmlReader.Read();
            }
            catch (Exception e)
            {
                Log.Exception("Failure : caught upon trying to read element with double quoted attribute value ", e);
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        private static byte[] BuildXMLAttributeValue()
        {
            MemoryStream soapStream = new MemoryStream();
            XmlWriter xmlWriter = XmlWriter.Create(soapStream);

            xmlWriter.WriteRaw("<Test>");
            xmlWriter.WriteRaw("<TestElement1 LocalName1=\"\" LocalName2=\"'\" LocalName3=\"''\" LocalName4=\"'Single'\" />");
            xmlWriter.WriteRaw("<TestElement2 LocalName='\"DoubleQuote\"' />");
            xmlWriter.WriteRaw("</Test>");
            xmlWriter.Flush();

            byte[] soapBuffer = soapStream.ToArray();
            xmlWriter.Close();

            return soapBuffer;
        }

    }
}
