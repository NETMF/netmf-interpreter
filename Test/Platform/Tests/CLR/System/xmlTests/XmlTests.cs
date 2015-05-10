/*---------------------------------------------------------------------
* FaultsTests.cs - file description
* Version: 1.0
* Author: REDMOND\a-grchat
* Created: 11/14/2007 10:27:20 AM 
* ---------------------------------------------------------------------*/

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.IO;
using System.Text;
using System.Xml;

namespace Microsoft.SPOT.Platform.Tests
{
    /*
     * This is an implementation of the Stream class, it does not read from an external source.
     * It creates an internal XML document of fragment and then streams it out on calls to Read.
     * 
     */
    public class testStream : System.IO.Stream
    {
        public testStream()
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
            xmlWriter.WriteStartElement("TestElement1");
            xmlWriter.WriteAttributeString("XT", "TestLocalName", "TestNS", "TestValue");
            xmlWriter.WriteEndElement();
            xmlWriter.WriteRaw("<![CDATA[my escaped text]]>");
            xmlWriter.WriteRaw("<!-- my comment -->");
            //DTD is not supported, hence no DOCTYPE
            //xmlWriter.WriteRaw("<!DOCTYPE ...>");
            xmlWriter.WriteRaw("<item>");
            xmlWriter.WriteRaw("sampleText");
            xmlWriter.WriteRaw("</item>");
            //xmlWriter.WriteRaw("<ref &num>");           
            xmlWriter.WriteRaw("<?pi test?>");
            xmlWriter.WriteRaw("<space xml:space=\"preserve\">");
            xmlWriter.WriteRaw("               ");
            xmlWriter.WriteRaw("</space>");
            //xmlWriter.WriteRaw("sample  Text");
            xmlWriter.WriteRaw("       ");
            xmlWriter.WriteStartElement("TestElement2");
            xmlWriter.WriteAttributeString("Name1", "");
            xmlWriter.WriteAttributeString("Name2", "'");
            xmlWriter.WriteAttributeString("Name3", "''");
            xmlWriter.WriteAttributeString("Name4", "'Single'");
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("TestElement3");
            xmlWriter.WriteAttributeString("Name", "\"DoubleQuote\"");
            xmlWriter.WriteEndElement();
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
    public class stubXmlNameTable : XmlNameTable
    {
        public override string Add(string array)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override string Add(char[] array, int offset, int length)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override string Get(string array)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override string Get(char[] array, int offset, int length)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    public class XmlTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Note that XmlTestReader is tested by the XmlReader " +
                "tests as currently XmlReader is implemented by XmlTestReader internaly");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
        }

        //int.MaxValue causes issues, this is used to generate random numbers for tests 
        public const int LARGE_NUM = 20000;

        /*
         * Recursively calls the ctor with every combination of XmlReaderSettings
         * Should be called by an array of length 5, started with 0 depth
         */
        private void ctorTestHelper(bool[] boolSettings, int depth)
        {
            if (depth < boolSettings.Length)
            {
                ctorTestHelper(boolSettings, depth + 1);
                boolSettings[depth] = !boolSettings[depth];
                ctorTestHelper(boolSettings, depth + 1);
            }
            else //depth == boolSettings.Length
            {
                Log.Comment("Creating XmlReaders with the following non-enum settings:");
                XmlReaderSettings testSettings = new XmlReaderSettings();
                string testBaseUri = "http:\\test";

                testSettings.CheckCharacters = boolSettings[0];
                testSettings.CloseInput = boolSettings[1];
                testSettings.IgnoreComments = boolSettings[2];
                testSettings.IgnoreProcessingInstructions = boolSettings[3];
                testSettings.IgnoreWhitespace = boolSettings[4];
                Log.Comment("CheckCharacters: " + boolSettings[0]);
                Log.Comment("CloseInput: " + boolSettings[1]);
                Log.Comment("IgnoreComments: " + boolSettings[2]);
                Log.Comment("IgnoreProcessingInstructions: " + boolSettings[3]);
                Log.Comment("IgnoreWhitespace: " + boolSettings[4]);

                Random random = new Random();
                int lineNumberOffset = random.Next(LARGE_NUM) - random.Next(LARGE_NUM);
                int linePositionOffset = random.Next(LARGE_NUM) - random.Next(LARGE_NUM);
                Log.Comment("LineNumberOffset: " + lineNumberOffset);
                Log.Comment("LinePositionOffset: " + linePositionOffset);
                testSettings.LineNumberOffset = lineNumberOffset;
                testSettings.LinePositionOffset = linePositionOffset;


                Log.Comment("Enum Settings: ");
                Log.Comment("Validation: None");
                Log.Comment("ConformanceLevel: Auto");
                testSettings.ValidationType = ValidationType.None;
                testSettings.ConformanceLevel = System.Xml.ConformanceLevel.Auto;
                Log.Comment("Create with 2 args");
                XmlReader testReader = XmlReader.Create(new testStream(), testSettings);
                Log.Comment("Create with 3 args, valid baseURI");
                testReader = XmlReader.Create(new testStream(), testSettings, testBaseUri);


                Log.Comment("Enum Settings: ");
                Log.Comment("Validation: None");
                Log.Comment("ConformanceLevel: Document");
                testSettings.ValidationType = ValidationType.None;
                testSettings.ConformanceLevel = System.Xml.ConformanceLevel.Document;
                Log.Comment("Create with 2 args");
                testReader = XmlReader.Create(new testStream(), testSettings);
                Log.Comment("Create with 3 args, valid baseURI");
                testReader = XmlReader.Create(new testStream(), testSettings, testBaseUri);

                Log.Comment("Enum Settings: ");
                Log.Comment("Validation: None");
                Log.Comment("ConformanceLevel: Fragment");
                testSettings.ValidationType = ValidationType.None;
                testSettings.ConformanceLevel = System.Xml.ConformanceLevel.Fragment;
                Log.Comment("Create with 2 args");
                testReader = XmlReader.Create(new testStream(), testSettings);
                Log.Comment("Create with 3 args, valid baseURI");
                testReader = XmlReader.Create(new testStream(), testSettings, testBaseUri);

                Log.Comment("Enum Settings: ");
                Log.Comment("Validation: Schema");
                Log.Comment("ConformanceLevel: Auto");
                testSettings.ValidationType = ValidationType.Schema;
                testSettings.ConformanceLevel = System.Xml.ConformanceLevel.Auto;
                Log.Comment("Create with 2 args");
                testReader = XmlReader.Create(new testStream(), testSettings);
                Log.Comment("Create with 3 args, valid baseURI");
                testReader = XmlReader.Create(new testStream(), testSettings, testBaseUri);

                Log.Comment("Enum Settings: ");
                Log.Comment("Validation: Schema");
                Log.Comment("ConformanceLevel: Document");
                testSettings.ValidationType = ValidationType.Schema;
                testSettings.ConformanceLevel = System.Xml.ConformanceLevel.Document;
                Log.Comment("Create with 2 args");
                testReader = XmlReader.Create(new testStream(), testSettings);
                Log.Comment("Create with 3 args, valid baseURI");
                testReader = XmlReader.Create(new testStream(), testSettings, testBaseUri);

                Log.Comment("Enum Settings: ");
                Log.Comment("Validation: Schema");
                Log.Comment("ConformanceLevel: Fragment");
                testSettings.ValidationType = ValidationType.Schema;
                testSettings.ConformanceLevel = System.Xml.ConformanceLevel.Fragment;
                Log.Comment("Create with 2 args");
                testReader = XmlReader.Create(new testStream(), testSettings);
                Log.Comment("Create with 3 args, valid baseURI");
                testReader = XmlReader.Create(new testStream(), testSettings, testBaseUri);
            }
        }


        [TestMethod]
        public MFTestResults XmlTest_EmptyElement()
        {
            ///
            /// You should be able to read an empty element (previously we skipped over the element).
            /// This test assures that the soap:Body element can be read with ReadStartElement() as 
            /// required by DPWS
            /// 
            string xml =
@"<?xml version='1.0' encoding='utf-8'?>
<soap:Envelope
   xmlns:soap='http://www.w3.org/2003/05/soap-envelope'
   xmlns:wsa='http://schemas.xmlsoap.org/ws/2004/08/addressing'>
   <soap:Header>
     <wsa:To>urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b51</wsa:To>
     <wsa:Action>http://schemas.xmlsoap.org/ws/2004/09/transfer/Get</wsa:Action>
     <wsa:MessageID>urn:uuid:2faedc71-7358-4ac7-b806-79edef96484f</wsa:MessageID>
     <wsa:ReplyTo>
       <wsa:Address>http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous</wsa:Address>
     </wsa:ReplyTo>
     <wsa:From>
       <wsa:Address>urn:uuid:e229bb6b-2120-424d-809d-9eeaf141daa9</wsa:Address>
     </wsa:From>
   </soap:Header>
  <soap:Body/>
</soap:Envelope>";

            try
            {
                XmlReader reader = XmlReader.Create(new MemoryStream(Encoding.UTF8.GetBytes(xml)));

                reader.ReadStartElement("Envelope",
                "http://www.w3.org/2003/05/soap-envelope");

                //  header.ParseHeader(reader);
                reader.ReadToDescendant("Header",
                "http://www.w3.org/2003/05/soap-envelope");

                reader.ReadToNextSibling("Body",
                "http://www.w3.org/2003/05/soap-envelope");

                reader.ReadStartElement("Body", "http://www.w3.org/2003/05/soap-envelope");

                reader.ReadEndElement();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught while parsing soap message with empty body", ex);

                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults XmlTest1_XmlReader_Create()
        {
            /// <summary>
            /// Creates XmlReaders with all different args using XmlReader.Create
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                Log.Comment("Creating XmlReader with Stream");
                XmlReader testReader = XmlReader.Create(new testStream());

                Log.Comment("Creating XmlReader with Stream, XmlReaderSettings "
                    + "and Stream, XmlReaderSettings, string");
                Log.Comment("Uses all combinations of XmlReaderSettings fields");
                ctorTestHelper(new bool[] { false, false, false, false, false }, 0);
            }
            catch (Exception e)
            {
                testResult = false;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults XmlTest2_XmlReader_Properties()
        {
            /// <summary>
            /// Tests the data and type of all of the XmlReader properties
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                // ctor is tested above as the MF defaults to creating an XmlTextReader
                Log.Comment("Creating XmlReader with Stream");
                string testBaseUri = "http:\\test";
                XmlReaderSettings defaultSettings = new XmlReaderSettings();
                XmlReader testReader = XmlReader.Create(new testStream(),
                    defaultSettings, testBaseUri);

                testResult &= testReader.Read();
                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                Log.Comment("AttributeCount");
                if (testReader.AttributeCount.GetType() !=
                    Type.GetType("System.Int32"))
                    throw new Exception("AttributeCount wrong type");

                if (testReader.AttributeCount != 0)
                    throw new Exception("AttributeCount wrong data");

                testResult &= testReader.Read();

                if (testReader.AttributeCount != 2)
                    throw new Exception("AttributeCount wrong data");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                Log.Comment("Depth");
                if (testReader.Depth.GetType() !=
                    Type.GetType("System.Int32"))
                    throw new Exception("Depth wrong type");

                if (testReader.Depth != 0)
                    throw new Exception("Depth wrong data");

                while (testReader.Read())
                {
                    if (testReader.Value == "")
                    {
                        switch (testReader.Name)
                        {
                            case "root":
                                if (testReader.Depth != 0)
                                    throw new Exception("Depth property returned bad data");
                                break;
                            case "TestElement1":
                                if (testReader.Depth != 1)
                                    throw new Exception("Depth property returned bad data");
                                break;
                            case "TestElement2":
                                if (testReader.Depth != 1)
                                    throw new Exception("Depth property returned bad data");
                                break;
                            case "ChildOfTestElement1":
                                if (testReader.Depth != 2)
                                    throw new Exception("Depth property returned bad data");
                                break;

                            default:
                                throw new Exception(
                                    "Read return wrong name during Depth property check");
                        }
                    }
                }

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                Log.Comment("CanReadBinaryContent");
                if (testReader.CanReadBinaryContent.GetType() !=
                    Type.GetType("System.Boolean"))
                    throw new Exception("CanReadBinaryContent wrong type");

                if (testReader.CanReadBinaryContent != false)
                    throw new Exception("CanReadBinaryContent wrong data");

                testResult &= testReader.Read();

                if (testReader.CanReadBinaryContent != false)
                    throw new Exception("CanReadBinaryContent wrong data");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                Log.Comment("CanReadValueChunk");
                if (testReader.CanReadValueChunk.GetType() !=
                    Type.GetType("System.Boolean"))
                    throw new Exception("CanReadValueChunk wrong type");

                if (testReader.CanReadValueChunk != true)
                    throw new Exception("CanReadValueChunk wrong data");

                testResult &= testReader.Read();

                if (testReader.CanReadValueChunk != true)
                    throw new Exception("CanReadValueChunk wrong data");


                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                Log.Comment("CanResolveEntity");
                if (testReader.CanResolveEntity.GetType() !=
                    Type.GetType("System.Boolean"))
                    throw new Exception("CanResolveEntity wrong type");

                if (testReader.CanResolveEntity != false)
                    throw new Exception("CanResolveEntity wrong data");

                testResult &= testReader.Read();

                if (testReader.CanResolveEntity != false)
                    throw new Exception("CanResolveEntity wrong data");


                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                Log.Comment("EOF");
                if (testReader.EOF.GetType() !=
                    Type.GetType("System.Boolean"))
                    throw new Exception("EOF wrong type");

                if (testReader.EOF != false)
                    throw new Exception("EOF wrong data");

                int counter = 0;
                while (testReader.Read())
                {
                    if (testReader.EOF != false)
                        break;
                    counter++;
                    if (counter > LARGE_NUM)
                    {
                        throw new Exception("EOF not found");
                    }
                }

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                Log.Comment("HasAttributes");
                if (testReader.HasAttributes.GetType() !=
                    Type.GetType("System.Boolean"))
                    throw new Exception("HasAttributes wrong type");

                if (testReader.HasAttributes != false)
                    throw new Exception("HasAttributes wrong data");

                testResult &= testReader.Read();

                if (testReader.HasAttributes != true)
                    throw new Exception("HasAttributes wrong data");


                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                Log.Comment("HasValue");
                if (testReader.HasValue.GetType() !=
                    Type.GetType("System.Boolean"))
                    throw new Exception("HasValue wrong type");

                if (testReader.HasValue != false)
                    throw new Exception("HasValue wrong data");

                while (testReader.Read())
                {
                    if (testReader.Name == "" && !testReader.HasValue)
                        throw new Exception(
                            "HasValue did not return proper data for text element");

                    if (testReader.Name != "" && testReader.HasValue)
                        throw new Exception(
                            "HasValue did not return proper data for Valueless element");
                }

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                Log.Comment("IsDefault");
                if (testReader.IsDefault.GetType() !=
                    Type.GetType("System.Boolean"))
                    throw new Exception("IsDefault wrong type");

                if (testReader.IsDefault != false)
                    throw new Exception("IsDefault wrong data");

                testResult &= testReader.Read();

                if (testReader.IsDefault != false)
                    throw new Exception("IsDefault wrong data");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                Log.Comment("IsEmptyElement");
                if (testReader.IsEmptyElement.GetType() !=
                    Type.GetType("System.Boolean"))
                    throw new Exception("IsEmptyElement wrong type");

                if (testReader.IsEmptyElement != false)
                    throw new Exception("IsEmptyElement wrong data");

                while (testReader.Read())
                {
                    if (testReader.Name != "ChildOfTestElement1" && testReader.IsEmptyElement)
                        throw new Exception("IsEmptyElement true for non-empty element");

                    if (testReader.Name == "ChildOfTestElement1" && !testReader.IsEmptyElement)
                        throw new Exception("IsEmptyElement false for empty element");
                }

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                Log.Comment("BaseURI");
                if (testReader.BaseURI.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("BaseURI wrong type");

                if (testReader.BaseURI != testBaseUri)
                    throw new Exception("BaseURI wrong data");

                testResult &= testReader.Read();

                if (testReader.BaseURI != testBaseUri)
                    throw new Exception("BaseURI wrong data");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                Log.Comment("LocalName");
                if (testReader.LocalName.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("LocalName wrong type");

                if (testReader.LocalName != "")
                    throw new Exception("LocalName wrong data");

                testResult &= testReader.Read();

                if (testReader.LocalName != "root")
                    throw new Exception("LocalName wrong data");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                Log.Comment("Name");
                if (testReader.Name.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("Name wrong type");

                if (testReader.Name != "")
                    throw new Exception("Name wrong data");

                testResult &= testReader.Read();

                if (testReader.Name != "root")
                    throw new Exception("Name wrong data");


                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                Log.Comment("NamespaceURI");
                if (testReader.NamespaceURI.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("NamespaceURI wrong type");

                if (testReader.NamespaceURI != "")
                    throw new Exception("NamespaceURI wrong data");

                testResult &= testReader.Read();

                if (testReader.NamespaceURI != "")
                    throw new Exception("NamespaceURI wrong data");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                Log.Comment("Prefix");
                if (testReader.Prefix.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("Prefix wrong type");

                if (testReader.Prefix != "")
                    throw new Exception("Prefix wrong data");

                testResult &= testReader.Read();

                if (testReader.Prefix != "")
                    throw new Exception("Prefix wrong data");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                Log.Comment("Value");
                if (testReader.Value.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("Value wrong type");

                if (testReader.Value != "")
                    throw new Exception("Value wrong data");

                while (testReader.Read())
                {
                    if (testReader.Name == "" && testReader.Value != "TestElement2Data")
                        throw new Exception(
                            "Value did not return proper data for Text element");

                    if (testReader.Name != "" && testReader.Value != "")
                        throw new Exception(
                            "Value did not return proper data for Valueless element");
                }

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                Log.Comment("XmlLang");
                if (testReader.XmlLang.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("XmlLang wrong type");

                if (testReader.XmlLang != "")
                    throw new Exception("XmlLang wrong data");

                testResult &= testReader.Read();

                if (testReader.XmlLang != "")
                    throw new Exception("XmlLang wrong data");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                Log.Comment("NameTable");
                if (testReader.NameTable.GetType().BaseType.FullName !=
                    "System.Xml.XmlNameTable")
                    throw new Exception("NameTable wrong type");

                Log.Comment("NodeType");
                if (testReader.NodeType.GetType().FullName !=
                    "System.Xml.XmlNodeType")
                    throw new Exception("NodeType wrong type");

                if (testReader.NodeType != XmlNodeType.None)
                    throw new Exception("NodeType wrong data");

                testResult &= testReader.Read();

                if (testReader.NodeType != XmlNodeType.Element)
                    throw new Exception("NodeType wrong data");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                Log.Comment("QuoteChar");
                if (testReader.QuoteChar.GetType() !=
                    Type.GetType("System.Char"))
                    throw new Exception("QuoteChar wrong type");

                if (testReader.QuoteChar != '"')
                    throw new Exception("QuoteChar wrong data");

                testResult &= testReader.Read();

                if (testReader.QuoteChar != '"')
                    throw new Exception("QuoteChar wrong data");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                Log.Comment("ReadState");
                if (testReader.ReadState.GetType().FullName !=
                    "System.Xml.ReadState")
                    throw new Exception("ReadState wrong type");

                if (testReader.ReadState != ReadState.Initial)
                    throw new Exception("ReadState wrong data");

                testResult &= testReader.Read();

                if (testReader.ReadState != ReadState.Interactive)
                    throw new Exception("ReadState wrong data");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                Log.Comment("Settings");
                if (testReader.Settings.GetType().FullName !=
                    "System.Xml.XmlReaderSettings")
                    throw new Exception("Settings wrong type");

                //Object equality problem
                if (testReader.Settings.CheckCharacters != defaultSettings.CheckCharacters)
                    throw new Exception("Settings wrong data");

                testResult &= testReader.Read();

                if (testReader.Settings.CloseInput != defaultSettings.CloseInput)
                    throw new Exception("Settings wrong data");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                Log.Comment("ValueType");
                if (testReader.ValueType.GetType().BaseType !=
                    Type.GetType("System.Type"))
                    throw new Exception("ValueType wrong type");

                if (testReader.ValueType != Type.GetType("System.String"))
                    throw new Exception("ValueType wrong data");

                testResult &= testReader.Read();

                if (testReader.ValueType != Type.GetType("System.String"))
                    throw new Exception("ValueType wrong data");

                Log.Comment("XmlSpace");
                if (testReader.XmlSpace.GetType().FullName !=
                    "System.Xml.XmlSpace")
                    throw new Exception("ValueType wrong type");

                if (testReader.XmlSpace != System.Xml.XmlSpace.None)
                    throw new Exception("XmlSpace wrong data");

                testResult &= testReader.Read();

                if (testReader.XmlSpace != System.Xml.XmlSpace.None)
                    throw new Exception("XmlSpace wrong data");
            }
            catch (Exception e)
            {
                testResult = false;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }


        [TestMethod]
        public MFTestResults XmlTest3_XmlReader_Methods()
        {
            /// <summary>
            /// Tests all of the XmlReader methods
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                Log.Comment("Creating XmlReader with Stream");
                string testBaseUri = "http:\\test";
                XmlReaderSettings defaultSettings = new XmlReaderSettings();
                XmlReader testReader = XmlReader.Create(
                    new testStream(), defaultSettings, testBaseUri);

                Log.Comment("Close");
                testReader.Close();
                if (testReader.ReadState != ReadState.Closed)
                    throw new Exception("Close did not function properly");

                Log.Comment("GetAttribute : Gets the value of an attribute");
                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);
                try
                {
                    testReader.GetAttribute(0);
                    throw new Exception("Failure : GetAttribute succeeded with 0 attributes");
                }
                catch (ArgumentOutOfRangeException) { }

                //Was bugged see 18670                
                testReader.Read();
                Debug.Print("local name " + testReader.LocalName);
                if (testReader.AttributeCount != 2)
                    throw new Exception("Failure Element Attr. Count : Expected '2' but got '" + testReader.AttributeCount + "'");

                if (testReader.GetAttribute(0) != "TestNS")
                    throw new Exception("GetAttribute(i) failed Getting 1st attribute");
                if (testReader.GetAttribute(1) != "TestValue")
                    throw new Exception("GetAttribute(i) failed Getting 2nd attribute");
                if (testReader.GetAttribute("xmlns:XT") != "TestNS")
                    throw new Exception("GetAttribute(name) failed Getting 2nd attribute");
                if (testReader.GetAttribute("XT:TestLocalName") != "TestValue")
                    throw new Exception("GetAttribute(name) failed Getting 2nd attribute");

                Log.Comment("IsStartElement");
                if (!testReader.IsStartElement())
                    throw new Exception("IsStartElement returned false before read");

                testReader.Read();
                if (!testReader.IsStartElement())
                    throw new Exception("IsStartElement returned false after read to 1st elt");

                testReader.Read();
                testReader.Read();
                if (testReader.IsStartElement())
                    throw new Exception("IsStartElement returned true after read to an end elt");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                if (!testReader.IsStartElement("root"))
                    throw new Exception("IsStartElement(string) returned false for root");

                if (testReader.IsStartElement("TestElement1"))
                    throw new Exception("IsStartElement(string) returned true for TestElement1");


                if (testReader.IsStartElement("FakeData"))
                    throw new Exception("IsStartElement(string) returned true for FakeData");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                if (!testReader.IsStartElement("root", ""))
                    throw new Exception(
                        "IsStartElement(string,string) returned false for root, namespace");

                if (testReader.IsStartElement("root", "FakeNamespaceURI"))
                    throw new Exception(
                        "IsStartElement(string,string) returned false for root, FakeNamespaceURI");

                if (testReader.IsStartElement("TestElement1", ""))
                    throw new Exception(
                        "IsStartElement(string,string) returned true for TestElement1");


                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                if (testReader.LookupNamespace("XT") != "")
                    throw new Exception("LookupNamespace returned incorrect data");

                testResult &= testReader.Read();

                if (testReader.LookupNamespace("XT") != "TestNS")
                    throw new Exception("LookupNamespace returned incorrect data after read");

                if (testReader.LookupNamespace("") != "")
                    throw new Exception(
                        "LookupNamespace returned incorrect data for bad arg after read");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                testReader.MoveToContent();
                testReader.MoveToAttribute(1);
                if (testReader.Name != "XT:TestLocalName")
                    throw new Exception("MoveToAttribute(1) moved to incorrect node");

                testReader.MoveToAttribute(0);
                if (testReader.Name != "xmlns:XT")
                    throw new Exception("MoveToAttribute(0) moved to incorrect node");

                testReader.MoveToAttribute("xmlns:XT");
                if (testReader.LocalName != "XT")
                    throw new Exception("MoveToAttribute(\"xmlns:XT\") moved to incorrect node");

                testReader.MoveToAttribute("XT:TestLocalName");
                if (testReader.LocalName != "TestLocalName")
                    throw new Exception("MoveToAttribute(\"XT:TestLocalName\") moved to incorrect node");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);
                Log.Comment("MoveToX methods");
                testReader.MoveToContent();
                if (testReader.Name != "root")
                    throw new Exception("MoveToContent went to wrong node");

                testReader.MoveToElement();
                if (testReader.Name != "root")
                    throw new Exception("MoveToElementwent to wrong node");

                testResult &= testReader.MoveToFirstAttribute();
                if (testReader.Name != "xmlns:XT")
                    throw new Exception("MoveToFirstAttribute went to wrong node");

                testResult &= testReader.ReadAttributeValue();
                if (testReader.Name != "")
                    throw new Exception("MoveToFirstAttribute went to wrong node");

                testResult &= testReader.MoveToNextAttribute();
                if (testReader.Name != "XT:TestLocalName")
                    throw new Exception("MoveToFirstAttribute went to wrong node");

                testResult &= testReader.ReadAttributeValue();
                if (testReader.Name != "")
                    throw new Exception("MoveToFirstAttribute went to wrong node");

                Log.Comment("ReadContentAsObject");
                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);
                testReader.Read();
                try
                {
                    testReader.ReadContentAsObject();
                    throw new Exception(
                        "Erroneous ReadContentAsObject did not throw InvalidOperationException");
                }
                catch (InvalidOperationException) { }
                testReader.Read();
                testReader.Read();
                testReader.Read();
                if (testReader.ReadContentAsObject().ToString() != "")
                    throw new Exception("ReadContentAsObject returned bad data for empty node");
                testReader.Read();
                testReader.Read();
                if (testReader.ReadContentAsObject().ToString() != "TestElement2Data")
                    throw new Exception("ReadContentAsObject returned bad data");

                Log.Comment("ReadElementString");
                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                testReader.Read();
                testReader.Read();
                testReader.Read();
                if (testReader.ReadElementString() != "")
                    throw new Exception("ReadContentAsObject returned bad data for empty node");
                testReader.Read();
                if (testReader.ReadElementString() != "TestElement2Data")
                    throw new Exception("ReadContentAsObject returned bad data for full node");
                try
                {
                    testReader.ReadElementString();
                    Log.Comment("Failure : calling ReadElementString on Complex/Non-empty content should throw Exception");
                    testResult = false;
                }
                catch (Exception)
                {
                    Log.Comment("Correctly throw Exception upon calling ReadElementString on Complex/Non-empty content");
                }

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                testReader.Read();
                testReader.Read();
                testReader.Read();
                if (testReader.ReadString() != "")
                    throw new Exception("ReadString returned bad data for empty node");
                testReader.Read();
                testReader.Read();
                if (testReader.ReadString() != "TestElement2Data")
                    throw new Exception("ReadString returned bad data for full node");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                testReader.Read();
                testReader.Read();
                testReader.Read();
                if (testReader.ReadElementString("ChildOfTestElement1") != "")
                    throw new Exception("ReadContentAsObject returned bad data for empty node");
                testReader.Read();
                if (testReader.ReadElementString("TestElement2") != "TestElement2Data")
                    throw new Exception("ReadContentAsObject returned bad data for full node");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                testReader.Read();
                testReader.Read();
                testReader.Read();
                if (testReader.ReadElementString("ChildOfTestElement1", "") != "")
                    throw new Exception("ReadContentAsObject returned bad data for empty node");
                testReader.Read();
                if (testReader.ReadElementString("TestElement2", "") != "TestElement2Data")
                    throw new Exception("ReadContentAsObject returned bad data for full node");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                testReader.ReadStartElement();
                if (testReader.Name != "TestElement1")
                    throw new Exception("ReadStartElement failed on start element");
                try
                {
                    testReader.ReadEndElement();
                    throw new Exception("ReadEndElement succeeded on start element");
                }
                catch (XmlException) { }
                testReader.Read();
                testReader.Read();
                testReader.ReadEndElement();
                if (testReader.Name != "TestElement2")
                    throw new Exception("ReadEndElement failed on end element");
                testReader.Read();
                try
                {
                    testReader.ReadStartElement();
                    throw new Exception("ReadStartElement succeeded on end element");
                }
                catch (XmlException) { }

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);
                testReader.ReadToDescendant("ChildOfTestElement1");
                if (testReader.Name != "ChildOfTestElement1")
                    throw new Exception("ReadToDescendant read to wrong location");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);
                testReader.ReadToDescendant("ChildOfTestElement2");
                if (testReader.Name != "")
                    throw new Exception("ReadToDescendant read to a location erroneously");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);
                testReader.ReadToDescendant("ChildOfTestElement1", "");
                if (testReader.Name != "ChildOfTestElement1")
                    throw new Exception("ReadToDescendant with namesapce read to wrong location");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);
                testReader.ReadToDescendant("ChildOfTestElement2", "");
                if (testReader.Name != "")
                    throw new Exception(
                        "ReadToDescendant with namesapce read to a location erroneously");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);
                testReader.ReadToFollowing("ChildOfTestElement1");
                if (testReader.Name != "ChildOfTestElement1")
                    throw new Exception("ReadToFollowing read to wrong location");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);
                testReader.ReadToFollowing("ChildOfTestElement2");
                if (testReader.Name != "")
                    throw new Exception("ReadToFollowing read to a location erroneously");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);
                testReader.ReadToFollowing("ChildOfTestElement1", "");
                if (testReader.Name != "ChildOfTestElement1")
                    throw new Exception("ReadToFollowing with namesapce read to wrong location");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);
                testReader.ReadToFollowing("ChildOfTestElement2", "");
                if (testReader.Name != "")
                    throw new Exception(
                        "ReadToFollowing with namesapce read to a location erroneously");

                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);
                testReader.ReadToFollowing("TestElement2");
                try
                {
                    testReader.ReadValueChunk(new char[16], 0, 16);
                    throw new Exception(
                        "ReadValueChunk returned failed to throw an InvalidOperationException");
                }
                catch (InvalidOperationException) { }
                testReader.Read();
                char[] charBuffer = new char[16];
                testReader.ReadValueChunk(charBuffer, 0, 16);
                if (!charArrEquals(charBuffer, "TestElement2Data".ToCharArray()))
                    throw new Exception("ReadValueChunk returned bad data");


                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);
                testReader.Read();
                testReader.Read();
                testReader.Skip();
                if (testReader.Name != "TestElement2")
                    throw new Exception("Skip failed to advance to correct node");

                Log.Comment("Unsupported methods");
                testReader = XmlReader.Create(new testStream(), defaultSettings, testBaseUri);

                try
                {
                    testReader.ReadContentAsBase64(new byte[10], 0, 10);
                    throw new Exception("ReadContentAsBase64 did not throw NotSupportedException");
                }
                catch (NotSupportedException) { }

                try
                {
                    testReader.ReadContentAsBinHex(new byte[10], 0, 10);
                    throw new Exception("ReadContentAsBase64 did not throw NotSupportedException");
                }
                catch (NotSupportedException) { }

                try
                {
                    testReader.ReadElementContentAsBase64(new byte[10], 0, 10);
                    throw new Exception("ReadContentAsBase64 did not throw NotSupportedException");
                }
                catch (NotSupportedException) { }

                try
                {
                    testReader.ReadElementContentAsBinHex(new byte[10], 0, 10);
                    throw new Exception("ReadContentAsBase64 did not throw NotSupportedException");
                }
                catch (NotSupportedException) { }
            }
            catch (Exception e)
            {
                testResult = false;
                Log.Comment("Incorrect exception caught: " + e.Message);
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
        public MFTestResults XmlTest4_Enums()
        {
            /// <summary>
            /// Tests all of the used enumerations in System.Xml
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                Log.Comment("ConformanceLevel");
                string testBaseUri = "http:\\test";
                XmlReaderSettings testSettings = new XmlReaderSettings();
                testStream fragment = new testStream();
                fragment.toFragment();

                Log.Comment("Parsing fragment with Auto");
                testSettings.ConformanceLevel = System.Xml.ConformanceLevel.Auto;
                XmlReader testReader = XmlReader.Create(fragment, testSettings, testBaseUri);
                testReader.Read();

                Log.Comment("Parsing fragment with Fragment");
                testSettings.ConformanceLevel = System.Xml.ConformanceLevel.Fragment;
                testReader = XmlReader.Create(fragment, testSettings, testBaseUri);
                testReader.Read();

                try
                {
                    Log.Comment("Parsing fragment with Document");
                    testSettings.ConformanceLevel = System.Xml.ConformanceLevel.Document;
                    testReader = XmlReader.Create(fragment, testSettings, testBaseUri);
                    testReader.Read();
                    throw new Exception("Read failed to throw XmlException on rootless Document");
                }
                catch (XmlException)
                {
                    Log.Comment("Correctly throw XmlException upon Read to rootless Document");
                }

                Log.Comment("ReadState");
                testSettings.ConformanceLevel = System.Xml.ConformanceLevel.Auto;
                fragment = new testStream();
                fragment.toFragment();

                testReader = XmlReader.Create(fragment, testSettings, testBaseUri);
                if (testReader.ReadState != ReadState.Initial)
                    throw new Exception("ReadState failed to set to Initial on initialization");

                testReader.Read();
                if (testReader.ReadState != ReadState.Interactive)
                    throw new Exception("ReadState failed to set to Interactive on read");

                while (testReader.Read()) { }
                if (testReader.ReadState != ReadState.EndOfFile)
                    throw new Exception("ReadState failed to set to EndOfFile after read");

                testReader.Close();
                if (testReader.ReadState != ReadState.Closed)
                    throw new Exception("ReadState failed to set to Closed on close");

                testSettings.ConformanceLevel = System.Xml.ConformanceLevel.Document;
                testReader = XmlReader.Create(fragment, testSettings, testBaseUri);
                try
                {
                    testReader.Read();
                }
                catch (XmlException)
                {
                    if (testReader.ReadState != ReadState.Error)
                        throw new Exception("ReadState failed to set to Error on parsing error");
                }

                Log.Comment("XmlNodeType");
                //Current implementations of XmlTextReader do not use the XmlNodeTypes 
                //Document, DocumentFragment, Entity, EndEntity or Notation
                testSettings.ConformanceLevel = System.Xml.ConformanceLevel.Auto;
                testStream nodeTypes = new testStream();
                nodeTypes.toNodeTypes();
                testReader = XmlReader.Create(nodeTypes, testSettings, testBaseUri);

                if (testReader.NodeType != XmlNodeType.None)
                    throw new Exception("NodeType was not none before read");

                Log.Comment("XmlDeclaration");
                testReader.Read();
                if (testReader.NodeType != XmlNodeType.XmlDeclaration)
                    throw new Exception("NodeType did not match XmlDeclaration node");

                testReader.Read();
                testReader.MoveToAttribute(0);
                if (testReader.NodeType != XmlNodeType.Attribute)
                    throw new Exception("NodeType did not match Attribute node");

                Log.Comment("CDATA");
                testReader.Read();
                if (testReader.NodeType != XmlNodeType.CDATA)
                    throw new Exception("NodeType did not match CDATA node");

                Log.Comment("Comment");
                testReader.Read();
                if (testReader.NodeType != XmlNodeType.Comment)
                    throw new Exception("NodeType did not match Comment node");

                Log.Comment("DTD is not supported, hence no DocumentType.");
                //try
                //{
                //    testReader.Read();
                //    if (testReader.NodeType != XmlNodeType.DocumentType)
                //        throw new Exception("NodeType did not match DocumentType node");
                //    throw new Exception("DocumentType failed to throw NotSupportedException");
                //}
                //catch (NotSupportedException)
                //{
                //}

                Log.Comment("Element");
                testReader.Read();
                if (testReader.NodeType != XmlNodeType.Element)
                    throw new Exception("NodeType did not match Element node");

                Log.Comment("Text");
                testReader.Read();
                if (testReader.NodeType != XmlNodeType.Text)
                    throw new Exception("NodeType did not match Text node");

                Log.Comment("EndElement");
                testReader.Read();
                if (testReader.NodeType != XmlNodeType.EndElement)
                    throw new Exception("NodeType did not match EndElement node");

                //Log.Comment("EntityReference");
                //testReader.Read();
                //if (testReader.NodeType != XmlNodeType.EntityReference)
                //    throw new Exception("NodeType did not match EntityReference node");

                Log.Comment("ProcessingInstruction");
                testReader.Read();
                if (testReader.NodeType != XmlNodeType.ProcessingInstruction)
                    throw new Exception("NodeType did not match ProcessingInstruction node");

                Log.Comment("SignificantWhitespace");
                testReader.Read();
                testReader.Read();
                if (testReader.NodeType != XmlNodeType.SignificantWhitespace)
                    throw new Exception("NodeType did not match SignificantWhitespace node");

                testReader.Read();
                Log.Comment("Whitespace");
                testReader.Read();
                if (testReader.NodeType != XmlNodeType.Whitespace)
                    throw new Exception("NodeType did not match Whitespace node");

                Log.Comment("Different Attribute value");
                Log.Comment("Single Quoted Attribute value");
                testReader.Read();
                if (testReader.GetAttribute(0) != "")
                    throw new Exception("GetAttribute(0) failed Getting 1st attribute value");
                if (testReader.GetAttribute(1) != "'")
                    throw new Exception("GetAttribute(1) failed Getting 2nd attribute value");
                if (testReader.GetAttribute(2) != "''")
                    throw new Exception("GetAttribute(2) failed Getting 3rd attribute value");
                if (testReader.GetAttribute(3) != "'Single'")
                    throw new Exception("GetAttribute(3) failed Getting 4th attribute value");
                Log.Comment("Double Quoted Attribute value");
                try
                {
                    testReader.Read();
                }
                catch (Exception ex)
                {
                    Log.Comment("Caught '" + ex.Message + "' upon trying to read elt. with double quoted attribute value");
                    return MFTestResults.Fail;
                }
            }
            catch (Exception e)
            {
                testResult = false;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults XmlTest5_NameTable()
        {
            /// <summary>
            /// Tests the NameTable class, which implements XmlNameTable
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                XmlReader testReader = XmlReader.Create(new testStream());

                NameTable testNT = new NameTable();
                object root = testNT.Add("root");
                object element1 = testNT.Add("TestElement1");
                testReader.Read();
                object localName = testReader.LocalName;
                if (!(localName == root))
                {
                    Log.Comment("1st reading, expected local name '" + root + "' but got '" + localName + "'");
                    testResult = false;
                }
                testReader.Read();
                localName = testReader.LocalName;
                if (!(localName == element1))
                {
                    Log.Comment("2nd reading, expected local name '" + element1 + "' but got '" + localName + "'");
                    testResult = false;
                }

                testNT.Add("test1");

                if (testNT.Get("test1").GetType() != Type.GetType("System.String"))
                    throw new Exception("Get(string) got wrong type");

                if (testNT.Get("test1") != "test1")
                    throw new Exception("Get(string) got wrong data");

                if (testNT.Get("test1".ToCharArray(), 0, 5).GetType() != Type.GetType("System.String"))
                    throw new Exception("Get(string) got wrong type");

                if (testNT.Get("test1".ToCharArray(), 0, 5) != "test1")
                    throw new Exception("Get(string) got wrong data");

                testNT.Add("test2".ToCharArray(), 0, 5);

                if (testNT.Get("test2").GetType() != Type.GetType("System.String"))
                    throw new Exception("Get(string) got wrong type");

                if (testNT.Get("test2") != "test2")
                    throw new Exception("Get(string) got wrong data");

                if (testNT.Get("test2".ToCharArray(), 0, 5).GetType() != Type.GetType("System.String"))
                    throw new Exception("Get(string) got wrong type");

                if (testNT.Get("test2".ToCharArray(), 0, 5) != "test2")
                    throw new Exception("Get(string) got wrong data");

            }
            catch (Exception e)
            {
                testResult = false;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        //Unsupported.  Functionality removed from xml namespace.
        //[TestMethod]
        //public MFTestResults XmlTest6_XmlNamespace()
        //{
        //    /// <summary>
        //    /// Tests the XmlNamespace
        //    /// </summary>
        //    ///
        //    bool testResult = true;
        //    try
        //    {

        //        Log.Comment("0 arg ctor");
        //        XmlNamespace testXN = new XmlNamespace();

        //        Log.Comment("NamespaceURI");
        //        if (testXN.NamespaceURI != null)
        //            if (testXN.NamespaceURI.GetType() !=
        //                Type.GetType("System.String"))
        //                throw new Exception("NamespaceURI wrong type");

        //        testXN.NamespaceURI = "test datum 1";

        //        if (testXN.NamespaceURI.GetType() !=
        //            Type.GetType("System.String"))
        //            throw new Exception("NamespaceURI wrong type after set");

        //        if (testXN.NamespaceURI != "test datum 1")
        //            throw new Exception("NamespaceURI wrong data");

        //        Log.Comment("Prefix");
        //        if (testXN.Prefix != null)
        //            if (testXN.Prefix.GetType() !=
        //                Type.GetType("System.String"))
        //                throw new Exception("Prefix wrong type");

        //        testXN.Prefix = "test datum 1";

        //        if (testXN.Prefix.GetType() !=
        //            Type.GetType("System.String"))
        //            throw new Exception("Prefix wrong type after set");

        //        if (testXN.Prefix != "test datum 1")
        //            throw new Exception("Prefix wrong data");

        //        Log.Comment("2 arg ctor");
        //        testXN = new XmlNamespace("TestPrefix","TestNSU");

        //        if (testXN.NamespaceURI.GetType() !=
        //            Type.GetType("System.String"))
        //            throw new Exception("NamespaceURI wrong type after set");

        //        if (testXN.NamespaceURI != "TestNSU")
        //            throw new Exception("NamespaceURI wrong data");

        //        if (testXN.Prefix.GetType() !=
        //            Type.GetType("System.String"))
        //            throw new Exception("Prefix wrong type after set");

        //        if (testXN.Prefix != "TestPrefix")
        //            throw new Exception("Prefix wrong data");


        //    }
        //    catch (Exception e)
        //    {
        //        testResult = false;
        //        Log.Comment("Incorrect exception caught: " + e.Message);
        //    }
        //    return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        //}
    }
}
