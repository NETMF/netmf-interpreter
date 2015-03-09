////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Xml;
using Microsoft.SPOT.Platform.Test;
using System.IO;
using System.Text;
using SPOTXmlTests;

namespace Microsoft.SPOT.Platform.Tests
{
    public class XmlBasicTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for basic Xml tests.");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        [TestMethod]
        public MFTestResults SimpleXmlParsing()
        {
            XmlReader reader = null;
            try
            {
                String xml = @"<a attr='a'>abc</a>";

                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

                XmlReaderSettings settings = new XmlReaderSettings();

                reader = XmlReader.Create(stream, settings);

                VerifyReader(reader, new XmlReaderState { StateName = "1: None", NodeType = XmlNodeType.None, Depth = 0, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Initial, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "2: Element, Name=\"a\"", NodeType = XmlNodeType.Element, Depth = 0, Name = "a", Prefix = "", LocalName = "a", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "3: Attribute, Name=\"attr\", Value=\"a\"", NodeType = XmlNodeType.Attribute, Depth = 1, Name = "attr", Prefix = "", LocalName = "attr", NamespaceURI = "", Value = "a", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "4: Text, Value=\"abc\"", NodeType = XmlNodeType.Text, Depth = 1, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "abc", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "5: EndElement, Name=\"a\"", NodeType = XmlNodeType.EndElement, Depth = 0, Name = "a", Prefix = "", LocalName = "a", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "6: None", NodeType = XmlNodeType.None, Depth = 0, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.EndOfFile, EOF = true, });
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults MoreComplexXmlParsing()
        {
            XmlReader reader = null;
            try
            {
                String xml = Resource.GetString(Resource.StringResources.complexXml);

                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

                XmlReaderSettings settings = new XmlReaderSettings();

                reader = XmlReader.Create(stream, settings);

                VerifyReader(reader, new XmlReaderState { StateName = "1: None", NodeType = XmlNodeType.None, Depth = 0, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Initial, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "2: XmlDeclaration, Value=\"\"", NodeType = XmlNodeType.XmlDeclaration, Depth = 0, Name = "xml", Prefix = "", LocalName = "xml", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "3: Attribute, Name=\"version\", Value=\"1.0\"", NodeType = XmlNodeType.Attribute, Depth = 1, Name = "version", Prefix = "", LocalName = "version", NamespaceURI = "", Value = "1.0", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "4: Attribute, Name=\"encoding\", Value=\"UTF-8\"", NodeType = XmlNodeType.Attribute, Depth = 1, Name = "encoding", Prefix = "", LocalName = "encoding", NamespaceURI = "", Value = "UTF-8", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "5: Attribute, Name=\"standalone\", Value=\"yes\"", NodeType = XmlNodeType.Attribute, Depth = 1, Name = "standalone", Prefix = "", LocalName = "standalone", NamespaceURI = "", Value = "yes", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "6: Whitespace, Value=\"\n\"", NodeType = XmlNodeType.Whitespace, Depth = 0, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "7: ProcessingInstruction, Name=\"pi\", Value=\" ??????processing ??instruction?\"", NodeType = XmlNodeType.ProcessingInstruction, Depth = 0, Name = "pi", Prefix = "", LocalName = "pi", NamespaceURI = "", Value = " ??????processing ??instruction?", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "8: Whitespace, Value=\"\n\"", NodeType = XmlNodeType.Whitespace, Depth = 0, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "9: Comment, Value=\"comment <><><>- - - - ?&&&\"", NodeType = XmlNodeType.Comment, Depth = 0, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "comment <><><>- - - - ?&&&", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "10: Whitespace, Value=\"\n\"", NodeType = XmlNodeType.Whitespace, Depth = 0, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "11: Element, Name=\"ns:ln\"", NodeType = XmlNodeType.Element, Depth = 0, Name = "ns:ln", Prefix = "ns", LocalName = "ln", NamespaceURI = "http://tempuri.org", Value = "", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "12: Attribute, Name=\"xmlns:ns\", Value=\"http://tempuri.org\"", NodeType = XmlNodeType.Attribute, Depth = 1, Name = "xmlns:ns", Prefix = "xmlns", LocalName = "ns", NamespaceURI = "http://www.w3.org/2000/xmlns/", Value = "http://tempuri.org", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "13: Attribute, Name=\"ns:at\", Value=\".\"", NodeType = XmlNodeType.Attribute, Depth = 1, Name = "ns:at", Prefix = "ns", LocalName = "at", NamespaceURI = "http://tempuri.org", Value = ".", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "14: Attribute, Name=\"at2\", Value=\"abc defg\"", NodeType = XmlNodeType.Attribute, Depth = 1, Name = "at2", Prefix = "", LocalName = "at2", NamespaceURI = "", Value = "abc defg", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "15: Whitespace, Value=\"\n  \"", NodeType = XmlNodeType.Whitespace, Depth = 1, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n  ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "16: Element, Name=\"ab\"", NodeType = XmlNodeType.Element, Depth = 1, Name = "ab", Prefix = "", LocalName = "ab", NamespaceURI = "", Value = "", IsEmptyElement = true, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "17: Text, Value=\"<\n\n  ab]]c>]&'] \n  \"", NodeType = XmlNodeType.Text, Depth = 1, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "<\n\n  ab]]c>]&'] \n  ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "18: Element, Name=\"a\"", NodeType = XmlNodeType.Element, Depth = 1, Name = "a", Prefix = "", LocalName = "a", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "19: Text, Value=\"ck\"", NodeType = XmlNodeType.Text, Depth = 2, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "ck", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "20: EndElement, Name=\"a\"", NodeType = XmlNodeType.EndElement, Depth = 1, Name = "a", Prefix = "", LocalName = "a", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "21: Text, Value=\"\n  ]\"", NodeType = XmlNodeType.Text, Depth = 1, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n  ]", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "22: Element, Name=\"a\"", NodeType = XmlNodeType.Element, Depth = 1, Name = "a", Prefix = "", LocalName = "a", NamespaceURI = "", Value = "", IsEmptyElement = true, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "23: Text, Value=\"]\n  \"\"", NodeType = XmlNodeType.Text, Depth = 1, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "]\n  \"", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "24: Element, Name=\"a\"", NodeType = XmlNodeType.Element, Depth = 1, Name = "a", Prefix = "", LocalName = "a", NamespaceURI = "", Value = "", IsEmptyElement = true, AttributeCount = 1, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "25: Attribute, Name=\"a\", Value=\"><& ' k     中文內容 \"\"", NodeType = XmlNodeType.Attribute, Depth = 2, Name = "a", Prefix = "", LocalName = "a", NamespaceURI = "", Value = "><& ' k     中文內容 \"", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "26: Whitespace, Value=\"\n \"", NodeType = XmlNodeType.Whitespace, Depth = 1, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "27: Element, Name=\"a\"", NodeType = XmlNodeType.Element, Depth = 1, Name = "a", Prefix = "", LocalName = "a", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "28: Attribute, Name=\"a\", Value=\"\"", NodeType = XmlNodeType.Attribute, Depth = 2, Name = "a", Prefix = "", LocalName = "a", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "29: Element, Name=\"a\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "a", Prefix = "", LocalName = "a", NamespaceURI = "", Value = "", IsEmptyElement = true, AttributeCount = 1, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "30: Attribute, Name=\"a\", Value=\"\"", NodeType = XmlNodeType.Attribute, Depth = 3, Name = "a", Prefix = "", LocalName = "a", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "31: EndElement, Name=\"a\"", NodeType = XmlNodeType.EndElement, Depth = 1, Name = "a", Prefix = "", LocalName = "a", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "32: Whitespace, Value=\"\n \"", NodeType = XmlNodeType.Whitespace, Depth = 1, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "33: Element, Name=\"a\"", NodeType = XmlNodeType.Element, Depth = 1, Name = "a", Prefix = "", LocalName = "a", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "34: Attribute, Name=\"ns:a\", Value=\".\"", NodeType = XmlNodeType.Attribute, Depth = 2, Name = "ns:a", Prefix = "ns", LocalName = "a", NamespaceURI = "http://tempuri.org", Value = ".", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "35: Text, Value=\"bc\"", NodeType = XmlNodeType.Text, Depth = 2, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "bc", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "36: Element, Name=\"a\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "a", Prefix = "", LocalName = "a", NamespaceURI = "", Value = "", IsEmptyElement = true, AttributeCount = 1, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "37: Attribute, Name=\"a\", Value=\"><&    '\"\"", NodeType = XmlNodeType.Attribute, Depth = 3, Name = "a", Prefix = "", LocalName = "a", NamespaceURI = "", Value = "><&    '\"", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "38: Element, Name=\"a\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "a", Prefix = "", LocalName = "a", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.Default, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "39: Attribute, Name=\"xml:space\", Value=\"default\"", NodeType = XmlNodeType.Attribute, Depth = 3, Name = "xml:space", Prefix = "xml", LocalName = "space", NamespaceURI = "http://www.w3.org/XML/1998/namespace", Value = "default", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.Default, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "40: Whitespace, Value=\"     \"", NodeType = XmlNodeType.Whitespace, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "     ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.Default, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "41: EndElement, Name=\"a\"", NodeType = XmlNodeType.EndElement, Depth = 2, Name = "a", Prefix = "", LocalName = "a", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.Default, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "42: EndElement, Name=\"a\"", NodeType = XmlNodeType.EndElement, Depth = 1, Name = "a", Prefix = "", LocalName = "a", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "43: Whitespace, Value=\"\n \"", NodeType = XmlNodeType.Whitespace, Depth = 1, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "44: Element, Name=\"ns:a\"", NodeType = XmlNodeType.Element, Depth = 1, Name = "ns:a", Prefix = "ns", LocalName = "a", NamespaceURI = "http://tempuri.org", Value = "", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "en", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "45: Attribute, Name=\"xml:lang\", Value=\"en\"", NodeType = XmlNodeType.Attribute, Depth = 2, Name = "xml:lang", Prefix = "xml", LocalName = "lang", NamespaceURI = "http://www.w3.org/XML/1998/namespace", Value = "en", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "en", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "46: Element, Name=\"ns:b\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "ns:b", Prefix = "ns", LocalName = "b", NamespaceURI = "http://tempuri.org", Value = "", IsEmptyElement = true, AttributeCount = 1, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "en-GB", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "47: Attribute, Name=\"xml:lang\", Value=\"en-GB\"", NodeType = XmlNodeType.Attribute, Depth = 3, Name = "xml:lang", Prefix = "xml", LocalName = "lang", NamespaceURI = "http://www.w3.org/XML/1998/namespace", Value = "en-GB", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "en-GB", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "48: Element, Name=\"ns:b\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "ns:b", Prefix = "ns", LocalName = "b", NamespaceURI = "http://tempuri.org", Value = "", IsEmptyElement = true, AttributeCount = 1, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "de", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "49: Attribute, Name=\"xml:lang\", Value=\"de\"", NodeType = XmlNodeType.Attribute, Depth = 3, Name = "xml:lang", Prefix = "xml", LocalName = "lang", NamespaceURI = "http://www.w3.org/XML/1998/namespace", Value = "de", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "de", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "50: EndElement, Name=\"ns:a\"", NodeType = XmlNodeType.EndElement, Depth = 1, Name = "ns:a", Prefix = "ns", LocalName = "a", NamespaceURI = "http://tempuri.org", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "en", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "51: Whitespace, Value=\"\n\n  \"", NodeType = XmlNodeType.Whitespace, Depth = 1, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n\n  ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "52: Element, Name=\"space\"", NodeType = XmlNodeType.Element, Depth = 1, Name = "space", Prefix = "", LocalName = "space", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.Preserve, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "53: Attribute, Name=\"xml:space\", Value=\"preserve\"", NodeType = XmlNodeType.Attribute, Depth = 2, Name = "xml:space", Prefix = "xml", LocalName = "space", NamespaceURI = "http://www.w3.org/XML/1998/namespace", Value = "preserve", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.Preserve, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "54: SignificantWhitespace, Value=\"       \n\n\n\n \"", NodeType = XmlNodeType.SignificantWhitespace, Depth = 2, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "       \n\n\n\n ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.Preserve, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "55: Element, Name=\"空白\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "空白", Prefix = "", LocalName = "空白", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.Preserve, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "56: SignificantWhitespace, Value=\"     \n\n\n      \"", NodeType = XmlNodeType.SignificantWhitespace, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "     \n\n\n      ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.Preserve, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "57: EndElement, Name=\"空白\"", NodeType = XmlNodeType.EndElement, Depth = 2, Name = "空白", Prefix = "", LocalName = "空白", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.Preserve, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "58: SignificantWhitespace, Value=\"\n\n \"", NodeType = XmlNodeType.SignificantWhitespace, Depth = 2, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n\n ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.Preserve, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "59: Element, Name=\"space1\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "space1", Prefix = "", LocalName = "space1", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.Default, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "60: Attribute, Name=\"xml:space\", Value=\"default\"", NodeType = XmlNodeType.Attribute, Depth = 3, Name = "xml:space", Prefix = "xml", LocalName = "space", NamespaceURI = "http://www.w3.org/XML/1998/namespace", Value = "default", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.Default, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "61: Whitespace, Value=\"     \n\n\n      \"", NodeType = XmlNodeType.Whitespace, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "     \n\n\n      ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.Default, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "62: EndElement, Name=\"space1\"", NodeType = XmlNodeType.EndElement, Depth = 2, Name = "space1", Prefix = "", LocalName = "space1", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.Default, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "63: SignificantWhitespace, Value=\"\n \"", NodeType = XmlNodeType.SignificantWhitespace, Depth = 2, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.Preserve, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "64: Element, Name=\"space1\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "space1", Prefix = "", LocalName = "space1", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 2, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.Preserve, XmlLang = "en", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "65: Attribute, Name=\"xml:space\", Value=\"preserve\"", NodeType = XmlNodeType.Attribute, Depth = 3, Name = "xml:space", Prefix = "xml", LocalName = "space", NamespaceURI = "http://www.w3.org/XML/1998/namespace", Value = "preserve", IsEmptyElement = false, AttributeCount = 2, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.Preserve, XmlLang = "en", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "66: Attribute, Name=\"xml:lang\", Value=\"en\"", NodeType = XmlNodeType.Attribute, Depth = 3, Name = "xml:lang", Prefix = "xml", LocalName = "lang", NamespaceURI = "http://www.w3.org/XML/1998/namespace", Value = "en", IsEmptyElement = false, AttributeCount = 2, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.Preserve, XmlLang = "en", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "67: SignificantWhitespace, Value=\"     \n\n\n      \"", NodeType = XmlNodeType.SignificantWhitespace, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "     \n\n\n      ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.Preserve, XmlLang = "en", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "68: EndElement, Name=\"space1\"", NodeType = XmlNodeType.EndElement, Depth = 2, Name = "space1", Prefix = "", LocalName = "space1", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.Preserve, XmlLang = "en", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "69: SignificantWhitespace, Value=\"\n \"", NodeType = XmlNodeType.SignificantWhitespace, Depth = 2, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.Preserve, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "70: Element, Name=\"space1\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "space1", Prefix = "", LocalName = "space1", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.Preserve, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "71: SignificantWhitespace, Value=\"     \n\n   \"", NodeType = XmlNodeType.SignificantWhitespace, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "     \n\n   ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.Preserve, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "72: Element, Name=\"a\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "a", Prefix = "", LocalName = "a", NamespaceURI = "", Value = "", IsEmptyElement = true, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.Preserve, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "73: SignificantWhitespace, Value=\"    \"", NodeType = XmlNodeType.SignificantWhitespace, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "    ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.Preserve, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "74: Comment, Value=\"comment\"", NodeType = XmlNodeType.Comment, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "comment", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.Preserve, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "75: SignificantWhitespace, Value=\" \"", NodeType = XmlNodeType.SignificantWhitespace, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = " ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.Preserve, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "76: ProcessingInstruction, Name=\"pi\", Value=\" ???\"", NodeType = XmlNodeType.ProcessingInstruction, Depth = 3, Name = "pi", Prefix = "", LocalName = "pi", NamespaceURI = "", Value = " ???", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.Preserve, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "77: SignificantWhitespace, Value=\"\n      \"", NodeType = XmlNodeType.SignificantWhitespace, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n      ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.Preserve, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "78: EndElement, Name=\"space1\"", NodeType = XmlNodeType.EndElement, Depth = 2, Name = "space1", Prefix = "", LocalName = "space1", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.Preserve, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "79: SignificantWhitespace, Value=\"\n   \"", NodeType = XmlNodeType.SignificantWhitespace, Depth = 2, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n   ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.Preserve, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "80: EndElement, Name=\"space\"", NodeType = XmlNodeType.EndElement, Depth = 1, Name = "space", Prefix = "", LocalName = "space", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.Preserve, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "81: Whitespace, Value=\"\n\"", NodeType = XmlNodeType.Whitespace, Depth = 1, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "82: EndElement, Name=\"ns:ln\"", NodeType = XmlNodeType.EndElement, Depth = 0, Name = "ns:ln", Prefix = "ns", LocalName = "ln", NamespaceURI = "http://tempuri.org", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "83: Whitespace, Value=\"\n\n\"", NodeType = XmlNodeType.Whitespace, Depth = 0, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n\n", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "84: Comment, Value=\"comment \"", NodeType = XmlNodeType.Comment, Depth = 0, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "comment ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "85: Whitespace, Value=\"\n\"", NodeType = XmlNodeType.Whitespace, Depth = 0, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "86: ProcessingInstruction, Name=\"pi\", Value=\" pipipipipi\"", NodeType = XmlNodeType.ProcessingInstruction, Depth = 0, Name = "pi", Prefix = "", LocalName = "pi", NamespaceURI = "", Value = " pipipipipi", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "87: Whitespace, Value=\"\n\"", NodeType = XmlNodeType.Whitespace, Depth = 0, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "88: Comment, Value=\" comment2\"", NodeType = XmlNodeType.Comment, Depth = 0, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = " comment2", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "89: None", NodeType = XmlNodeType.None, Depth = 0, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.EndOfFile, EOF = true, });
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults SimpleServiceInputXmlParsing()
        {
            XmlReader reader = null;
            try
            {
                String xml = Resource.GetString(Resource.StringResources.soapIn);

                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

                XmlReaderSettings settings = new XmlReaderSettings();

                reader = XmlReader.Create(stream, settings);

                VerifyReader(reader, new XmlReaderState { StateName = "1: None", NodeType = XmlNodeType.None, Depth = 0, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Initial, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "2: Element, Name=\"s:Envelope\"", NodeType = XmlNodeType.Element, Depth = 0, Name = "s:Envelope", Prefix = "s", LocalName = "Envelope", NamespaceURI = "http://www.w3.org/2003/05/soap-envelope", Value = "", IsEmptyElement = false, AttributeCount = 2, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "3: Attribute, Name=\"xmlns:s\", Value=\"http://www.w3.org/2003/05/soap-envelope\"", NodeType = XmlNodeType.Attribute, Depth = 1, Name = "xmlns:s", Prefix = "xmlns", LocalName = "s", NamespaceURI = "http://www.w3.org/2000/xmlns/", Value = "http://www.w3.org/2003/05/soap-envelope", IsEmptyElement = false, AttributeCount = 2, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "4: Attribute, Name=\"xmlns:a\", Value=\"http://schemas.xmlsoap.org/ws/2004/08/addressing\"", NodeType = XmlNodeType.Attribute, Depth = 1, Name = "xmlns:a", Prefix = "xmlns", LocalName = "a", NamespaceURI = "http://www.w3.org/2000/xmlns/", Value = "http://schemas.xmlsoap.org/ws/2004/08/addressing", IsEmptyElement = false, AttributeCount = 2, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "5: Whitespace, Value=\"\n  \"", NodeType = XmlNodeType.Whitespace, Depth = 1, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n  ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "6: Element, Name=\"s:Header\"", NodeType = XmlNodeType.Element, Depth = 1, Name = "s:Header", Prefix = "s", LocalName = "Header", NamespaceURI = "http://www.w3.org/2003/05/soap-envelope", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "7: Whitespace, Value=\"\n    \"", NodeType = XmlNodeType.Whitespace, Depth = 2, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n    ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "8: Element, Name=\"a:Action\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "a:Action", Prefix = "a", LocalName = "Action", NamespaceURI = "http://schemas.xmlsoap.org/ws/2004/08/addressing", Value = "", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "9: Attribute, Name=\"s:mustUnderstand\", Value=\"1\"", NodeType = XmlNodeType.Attribute, Depth = 3, Name = "s:mustUnderstand", Prefix = "s", LocalName = "mustUnderstand", NamespaceURI = "http://www.w3.org/2003/05/soap-envelope", Value = "1", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "10: Text, Value=\"http://schemas.example.org/SimpleService/TwoWayRequest\"", NodeType = XmlNodeType.Text, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "http://schemas.example.org/SimpleService/TwoWayRequest", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "11: EndElement, Name=\"a:Action\"", NodeType = XmlNodeType.EndElement, Depth = 2, Name = "a:Action", Prefix = "a", LocalName = "Action", NamespaceURI = "http://schemas.xmlsoap.org/ws/2004/08/addressing", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "12: Whitespace, Value=\"\n    \"", NodeType = XmlNodeType.Whitespace, Depth = 2, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n    ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "13: Element, Name=\"a:From\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "a:From", Prefix = "a", LocalName = "From", NamespaceURI = "http://schemas.xmlsoap.org/ws/2004/08/addressing", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "14: Whitespace, Value=\"\n      \"", NodeType = XmlNodeType.Whitespace, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n      ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "15: Element, Name=\"a:Address\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "a:Address", Prefix = "a", LocalName = "Address", NamespaceURI = "http://schemas.xmlsoap.org/ws/2004/08/addressing", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "16: Text, Value=\"http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous\"", NodeType = XmlNodeType.Text, Depth = 4, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "17: EndElement, Name=\"a:Address\"", NodeType = XmlNodeType.EndElement, Depth = 3, Name = "a:Address", Prefix = "a", LocalName = "Address", NamespaceURI = "http://schemas.xmlsoap.org/ws/2004/08/addressing", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "18: Whitespace, Value=\"\n    \"", NodeType = XmlNodeType.Whitespace, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n    ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "19: EndElement, Name=\"a:From\"", NodeType = XmlNodeType.EndElement, Depth = 2, Name = "a:From", Prefix = "a", LocalName = "From", NamespaceURI = "http://schemas.xmlsoap.org/ws/2004/08/addressing", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "20: Whitespace, Value=\"\n    \"", NodeType = XmlNodeType.Whitespace, Depth = 2, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n    ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "21: Element, Name=\"a:ReplyTo\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "a:ReplyTo", Prefix = "a", LocalName = "ReplyTo", NamespaceURI = "http://schemas.xmlsoap.org/ws/2004/08/addressing", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "22: Whitespace, Value=\"\n      \"", NodeType = XmlNodeType.Whitespace, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n      ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "23: Element, Name=\"a:Address\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "a:Address", Prefix = "a", LocalName = "Address", NamespaceURI = "http://schemas.xmlsoap.org/ws/2004/08/addressing", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "24: Text, Value=\"http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous\"", NodeType = XmlNodeType.Text, Depth = 4, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "25: EndElement, Name=\"a:Address\"", NodeType = XmlNodeType.EndElement, Depth = 3, Name = "a:Address", Prefix = "a", LocalName = "Address", NamespaceURI = "http://schemas.xmlsoap.org/ws/2004/08/addressing", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "26: Whitespace, Value=\"\n    \"", NodeType = XmlNodeType.Whitespace, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n    ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "27: EndElement, Name=\"a:ReplyTo\"", NodeType = XmlNodeType.EndElement, Depth = 2, Name = "a:ReplyTo", Prefix = "a", LocalName = "ReplyTo", NamespaceURI = "http://schemas.xmlsoap.org/ws/2004/08/addressing", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "28: Whitespace, Value=\"\n    \"", NodeType = XmlNodeType.Whitespace, Depth = 2, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n    ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "29: Element, Name=\"a:MessageID\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "a:MessageID", Prefix = "a", LocalName = "MessageID", NamespaceURI = "http://schemas.xmlsoap.org/ws/2004/08/addressing", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "30: Text, Value=\"urn:uuid:85167211-529a-4eb1-89b0-5b7759df9cc3\"", NodeType = XmlNodeType.Text, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "urn:uuid:85167211-529a-4eb1-89b0-5b7759df9cc3", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "31: EndElement, Name=\"a:MessageID\"", NodeType = XmlNodeType.EndElement, Depth = 2, Name = "a:MessageID", Prefix = "a", LocalName = "MessageID", NamespaceURI = "http://schemas.xmlsoap.org/ws/2004/08/addressing", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "32: Whitespace, Value=\"\n    \"", NodeType = XmlNodeType.Whitespace, Depth = 2, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n    ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "33: Element, Name=\"a:To\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "a:To", Prefix = "a", LocalName = "To", NamespaceURI = "http://schemas.xmlsoap.org/ws/2004/08/addressing", Value = "", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "34: Attribute, Name=\"s:mustUnderstand\", Value=\"1\"", NodeType = XmlNodeType.Attribute, Depth = 3, Name = "s:mustUnderstand", Prefix = "s", LocalName = "mustUnderstand", NamespaceURI = "http://www.w3.org/2003/05/soap-envelope", Value = "1", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "35: Text, Value=\"http://localhost:8084/34c6b7e3-9494-4eb3-90bb-c946c2bcd775\"", NodeType = XmlNodeType.Text, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "http://localhost:8084/34c6b7e3-9494-4eb3-90bb-c946c2bcd775", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "36: EndElement, Name=\"a:To\"", NodeType = XmlNodeType.EndElement, Depth = 2, Name = "a:To", Prefix = "a", LocalName = "To", NamespaceURI = "http://schemas.xmlsoap.org/ws/2004/08/addressing", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "37: Whitespace, Value=\"\n  \"", NodeType = XmlNodeType.Whitespace, Depth = 2, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n  ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "38: EndElement, Name=\"s:Header\"", NodeType = XmlNodeType.EndElement, Depth = 1, Name = "s:Header", Prefix = "s", LocalName = "Header", NamespaceURI = "http://www.w3.org/2003/05/soap-envelope", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "39: Whitespace, Value=\"\n  \"", NodeType = XmlNodeType.Whitespace, Depth = 1, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n  ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "40: Element, Name=\"s:Body\"", NodeType = XmlNodeType.Element, Depth = 1, Name = "s:Body", Prefix = "s", LocalName = "Body", NamespaceURI = "http://www.w3.org/2003/05/soap-envelope", Value = "", IsEmptyElement = false, AttributeCount = 2, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "41: Attribute, Name=\"xmlns:xsi\", Value=\"http://www.w3.org/2001/XMLSchema-instance\"", NodeType = XmlNodeType.Attribute, Depth = 2, Name = "xmlns:xsi", Prefix = "xmlns", LocalName = "xsi", NamespaceURI = "http://www.w3.org/2000/xmlns/", Value = "http://www.w3.org/2001/XMLSchema-instance", IsEmptyElement = false, AttributeCount = 2, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "42: Attribute, Name=\"xmlns:xsd\", Value=\"http://www.w3.org/2001/XMLSchema\"", NodeType = XmlNodeType.Attribute, Depth = 2, Name = "xmlns:xsd", Prefix = "xmlns", LocalName = "xsd", NamespaceURI = "http://www.w3.org/2000/xmlns/", Value = "http://www.w3.org/2001/XMLSchema", IsEmptyElement = false, AttributeCount = 2, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "43: Whitespace, Value=\"\n    \"", NodeType = XmlNodeType.Whitespace, Depth = 2, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n    ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "44: Element, Name=\"TwoWayRequest\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "TwoWayRequest", Prefix = "", LocalName = "TwoWayRequest", NamespaceURI = "http://schemas.example.org/SimpleService", Value = "", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "45: Attribute, Name=\"xmlns\", Value=\"http://schemas.example.org/SimpleService\"", NodeType = XmlNodeType.Attribute, Depth = 3, Name = "xmlns", Prefix = "", LocalName = "xmlns", NamespaceURI = "http://www.w3.org/2000/xmlns/", Value = "http://schemas.example.org/SimpleService", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "46: Whitespace, Value=\"\n      \"", NodeType = XmlNodeType.Whitespace, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n      ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "47: Element, Name=\"X\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "X", Prefix = "", LocalName = "X", NamespaceURI = "http://schemas.example.org/SimpleService", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "48: Text, Value=\"1\"", NodeType = XmlNodeType.Text, Depth = 4, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "1", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "49: EndElement, Name=\"X\"", NodeType = XmlNodeType.EndElement, Depth = 3, Name = "X", Prefix = "", LocalName = "X", NamespaceURI = "http://schemas.example.org/SimpleService", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "50: Whitespace, Value=\"\n      \"", NodeType = XmlNodeType.Whitespace, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n      ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "51: Element, Name=\"Y\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "Y", Prefix = "", LocalName = "Y", NamespaceURI = "http://schemas.example.org/SimpleService", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "52: Text, Value=\"2\"", NodeType = XmlNodeType.Text, Depth = 4, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "2", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "53: EndElement, Name=\"Y\"", NodeType = XmlNodeType.EndElement, Depth = 3, Name = "Y", Prefix = "", LocalName = "Y", NamespaceURI = "http://schemas.example.org/SimpleService", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "54: Whitespace, Value=\"\n    \"", NodeType = XmlNodeType.Whitespace, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n    ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "55: EndElement, Name=\"TwoWayRequest\"", NodeType = XmlNodeType.EndElement, Depth = 2, Name = "TwoWayRequest", Prefix = "", LocalName = "TwoWayRequest", NamespaceURI = "http://schemas.example.org/SimpleService", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "56: Whitespace, Value=\"\n  \"", NodeType = XmlNodeType.Whitespace, Depth = 2, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n  ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "57: EndElement, Name=\"s:Body\"", NodeType = XmlNodeType.EndElement, Depth = 1, Name = "s:Body", Prefix = "s", LocalName = "Body", NamespaceURI = "http://www.w3.org/2003/05/soap-envelope", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "58: Whitespace, Value=\"\n\"", NodeType = XmlNodeType.Whitespace, Depth = 1, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "59: EndElement, Name=\"s:Envelope\"", NodeType = XmlNodeType.EndElement, Depth = 0, Name = "s:Envelope", Prefix = "s", LocalName = "Envelope", NamespaceURI = "http://www.w3.org/2003/05/soap-envelope", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "60: None", NodeType = XmlNodeType.None, Depth = 0, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.EndOfFile, EOF = true, });

            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults SimpleServiceOutputXmlParsing()
        {
            XmlReader reader = null;
            try
            {
                String xml = Resource.GetString(Resource.StringResources.soapOut);

                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

                XmlReaderSettings settings = new XmlReaderSettings();

                reader = XmlReader.Create(stream, settings);

                VerifyReader(reader, new XmlReaderState { StateName = "1: None", NodeType = XmlNodeType.None, Depth = 0, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Initial, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "2: Element, Name=\"soap:Envelope\"", NodeType = XmlNodeType.Element, Depth = 0, Name = "soap:Envelope", Prefix = "soap", LocalName = "Envelope", NamespaceURI = "http://www.w3.org/2003/05/soap-envelope", Value = "", IsEmptyElement = false, AttributeCount = 5, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "3: Attribute, Name=\"xmlns:soap\", Value=\"http://www.w3.org/2003/05/soap-envelope\"", NodeType = XmlNodeType.Attribute, Depth = 1, Name = "xmlns:soap", Prefix = "xmlns", LocalName = "soap", NamespaceURI = "http://www.w3.org/2000/xmlns/", Value = "http://www.w3.org/2003/05/soap-envelope", IsEmptyElement = false, AttributeCount = 5, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "4: Attribute, Name=\"xmlns:wsdp\", Value=\"http://schemas.xmlsoap.org/ws/2006/02/devprof\"", NodeType = XmlNodeType.Attribute, Depth = 1, Name = "xmlns:wsdp", Prefix = "xmlns", LocalName = "wsdp", NamespaceURI = "http://www.w3.org/2000/xmlns/", Value = "http://schemas.xmlsoap.org/ws/2006/02/devprof", IsEmptyElement = false, AttributeCount = 5, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "5: Attribute, Name=\"xmlns:wsd\", Value=\"http://schemas.xmlsoap.org/ws/2005/04/discovery\"", NodeType = XmlNodeType.Attribute, Depth = 1, Name = "xmlns:wsd", Prefix = "xmlns", LocalName = "wsd", NamespaceURI = "http://www.w3.org/2000/xmlns/", Value = "http://schemas.xmlsoap.org/ws/2005/04/discovery", IsEmptyElement = false, AttributeCount = 5, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "6: Attribute, Name=\"xmlns:wsa\", Value=\"http://schemas.xmlsoap.org/ws/2004/08/addressing\"", NodeType = XmlNodeType.Attribute, Depth = 1, Name = "xmlns:wsa", Prefix = "xmlns", LocalName = "wsa", NamespaceURI = "http://www.w3.org/2000/xmlns/", Value = "http://schemas.xmlsoap.org/ws/2004/08/addressing", IsEmptyElement = false, AttributeCount = 5, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "7: Attribute, Name=\"xmlns:smpl\", Value=\"http://schemas.example.org/SimpleService\"", NodeType = XmlNodeType.Attribute, Depth = 1, Name = "xmlns:smpl", Prefix = "xmlns", LocalName = "smpl", NamespaceURI = "http://www.w3.org/2000/xmlns/", Value = "http://schemas.example.org/SimpleService", IsEmptyElement = false, AttributeCount = 5, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "8: Whitespace, Value=\"\n  \"", NodeType = XmlNodeType.Whitespace, Depth = 1, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n  ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "9: Element, Name=\"soap:Header\"", NodeType = XmlNodeType.Element, Depth = 1, Name = "soap:Header", Prefix = "soap", LocalName = "Header", NamespaceURI = "http://www.w3.org/2003/05/soap-envelope", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "10: Whitespace, Value=\"\n    \"", NodeType = XmlNodeType.Whitespace, Depth = 2, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n    ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "11: Element, Name=\"wsa:To\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "wsa:To", Prefix = "wsa", LocalName = "To", NamespaceURI = "http://schemas.xmlsoap.org/ws/2004/08/addressing", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "12: Text, Value=\"http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous\"", NodeType = XmlNodeType.Text, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "13: EndElement, Name=\"wsa:To\"", NodeType = XmlNodeType.EndElement, Depth = 2, Name = "wsa:To", Prefix = "wsa", LocalName = "To", NamespaceURI = "http://schemas.xmlsoap.org/ws/2004/08/addressing", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "14: Whitespace, Value=\"\n    \"", NodeType = XmlNodeType.Whitespace, Depth = 2, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n    ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "15: Element, Name=\"wsa:Action\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "wsa:Action", Prefix = "wsa", LocalName = "Action", NamespaceURI = "http://schemas.xmlsoap.org/ws/2004/08/addressing", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "16: Text, Value=\"http://schemas.example.org/SimpleService/TwoWayResponse\"", NodeType = XmlNodeType.Text, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "http://schemas.example.org/SimpleService/TwoWayResponse", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "17: EndElement, Name=\"wsa:Action\"", NodeType = XmlNodeType.EndElement, Depth = 2, Name = "wsa:Action", Prefix = "wsa", LocalName = "Action", NamespaceURI = "http://schemas.xmlsoap.org/ws/2004/08/addressing", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "18: Whitespace, Value=\"\n    \"", NodeType = XmlNodeType.Whitespace, Depth = 2, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n    ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "19: Element, Name=\"wsa:RelatesTo\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "wsa:RelatesTo", Prefix = "wsa", LocalName = "RelatesTo", NamespaceURI = "http://schemas.xmlsoap.org/ws/2004/08/addressing", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "20: Text, Value=\"urn:uuid:33831893-d3be-4afd-8d0a-85b7e901366d\"", NodeType = XmlNodeType.Text, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "urn:uuid:33831893-d3be-4afd-8d0a-85b7e901366d", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "21: EndElement, Name=\"wsa:RelatesTo\"", NodeType = XmlNodeType.EndElement, Depth = 2, Name = "wsa:RelatesTo", Prefix = "wsa", LocalName = "RelatesTo", NamespaceURI = "http://schemas.xmlsoap.org/ws/2004/08/addressing", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "22: Whitespace, Value=\"\n    \"", NodeType = XmlNodeType.Whitespace, Depth = 2, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n    ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "23: Element, Name=\"wsa:MessageID\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "wsa:MessageID", Prefix = "wsa", LocalName = "MessageID", NamespaceURI = "http://schemas.xmlsoap.org/ws/2004/08/addressing", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "24: Text, Value=\"urn:uuid:ff99b19e-7db9-c8c9-8a8c-000000f15958\"", NodeType = XmlNodeType.Text, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "urn:uuid:ff99b19e-7db9-c8c9-8a8c-000000f15958", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "25: EndElement, Name=\"wsa:MessageID\"", NodeType = XmlNodeType.EndElement, Depth = 2, Name = "wsa:MessageID", Prefix = "wsa", LocalName = "MessageID", NamespaceURI = "http://schemas.xmlsoap.org/ws/2004/08/addressing", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "26: Whitespace, Value=\"\n    \"", NodeType = XmlNodeType.Whitespace, Depth = 2, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n    ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "27: Element, Name=\"wsd:AppSequence\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "wsd:AppSequence", Prefix = "wsd", LocalName = "AppSequence", NamespaceURI = "http://schemas.xmlsoap.org/ws/2005/04/discovery", Value = "", IsEmptyElement = true, AttributeCount = 3, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "28: Attribute, Name=\"InstanceId\", Value=\"1181949116\"", NodeType = XmlNodeType.Attribute, Depth = 3, Name = "InstanceId", Prefix = "", LocalName = "InstanceId", NamespaceURI = "", Value = "1181949116", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "29: Attribute, Name=\"SequenceId\", Value=\"urn:uuid:c883e4a8-9af4-4bf4-aaaf-06394151d6c0\"", NodeType = XmlNodeType.Attribute, Depth = 3, Name = "SequenceId", Prefix = "", LocalName = "SequenceId", NamespaceURI = "", Value = "urn:uuid:c883e4a8-9af4-4bf4-aaaf-06394151d6c0", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "30: Attribute, Name=\"MessageNumber\", Value=\"4\"", NodeType = XmlNodeType.Attribute, Depth = 3, Name = "MessageNumber", Prefix = "", LocalName = "MessageNumber", NamespaceURI = "", Value = "4", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "31: Whitespace, Value=\"\n  \"", NodeType = XmlNodeType.Whitespace, Depth = 2, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n  ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "32: EndElement, Name=\"soap:Header\"", NodeType = XmlNodeType.EndElement, Depth = 1, Name = "soap:Header", Prefix = "soap", LocalName = "Header", NamespaceURI = "http://www.w3.org/2003/05/soap-envelope", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "33: Whitespace, Value=\"\n  \"", NodeType = XmlNodeType.Whitespace, Depth = 1, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n  ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "34: Element, Name=\"soap:Body\"", NodeType = XmlNodeType.Element, Depth = 1, Name = "soap:Body", Prefix = "soap", LocalName = "Body", NamespaceURI = "http://www.w3.org/2003/05/soap-envelope", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "35: Whitespace, Value=\"\n    \"", NodeType = XmlNodeType.Whitespace, Depth = 2, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n    ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "36: Element, Name=\"smpl:TwoWayResponse\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "smpl:TwoWayResponse", Prefix = "smpl", LocalName = "TwoWayResponse", NamespaceURI = "http://schemas.example.org/SimpleService", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "37: Whitespace, Value=\"\n      \"", NodeType = XmlNodeType.Whitespace, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n      ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "38: Element, Name=\"smpl:Sum\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "smpl:Sum", Prefix = "smpl", LocalName = "Sum", NamespaceURI = "http://schemas.example.org/SimpleService", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "39: Text, Value=\"4\"", NodeType = XmlNodeType.Text, Depth = 4, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "4", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "40: EndElement, Name=\"smpl:Sum\"", NodeType = XmlNodeType.EndElement, Depth = 3, Name = "smpl:Sum", Prefix = "smpl", LocalName = "Sum", NamespaceURI = "http://schemas.example.org/SimpleService", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "41: Whitespace, Value=\"\n    \"", NodeType = XmlNodeType.Whitespace, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n    ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "42: EndElement, Name=\"smpl:TwoWayResponse\"", NodeType = XmlNodeType.EndElement, Depth = 2, Name = "smpl:TwoWayResponse", Prefix = "smpl", LocalName = "TwoWayResponse", NamespaceURI = "http://schemas.example.org/SimpleService", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "43: Whitespace, Value=\"\n  \"", NodeType = XmlNodeType.Whitespace, Depth = 2, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n  ", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "44: EndElement, Name=\"soap:Body\"", NodeType = XmlNodeType.EndElement, Depth = 1, Name = "soap:Body", Prefix = "soap", LocalName = "Body", NamespaceURI = "http://www.w3.org/2003/05/soap-envelope", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "45: Whitespace, Value=\"\n\"", NodeType = XmlNodeType.Whitespace, Depth = 1, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "\n", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "46: EndElement, Name=\"soap:Envelope\"", NodeType = XmlNodeType.EndElement, Depth = 0, Name = "soap:Envelope", Prefix = "soap", LocalName = "Envelope", NamespaceURI = "http://www.w3.org/2003/05/soap-envelope", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "47: None", NodeType = XmlNodeType.None, Depth = 0, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.EndOfFile, EOF = true, });
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults SampleMeshXmlParsing()
        {
            XmlReader reader = null;
            try
            {
                String xml = Resource.GetString(Resource.StringResources.mesh);

                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

                XmlReaderSettings settings = new XmlReaderSettings();

                settings.IgnoreWhitespace = true;

                reader = XmlReader.Create(stream, settings);

                VerifyReader(reader, new XmlReaderState { StateName = "1: None", NodeType = XmlNodeType.None, Depth = 0, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Initial, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "2: Element, Name=\"rss\"", NodeType = XmlNodeType.Element, Depth = 0, Name = "rss", Prefix = "", LocalName = "rss", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 2, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "3: Attribute, Name=\"xmlns:a10\", Value=\"http://www.w3.org/2005/Atom\"", NodeType = XmlNodeType.Attribute, Depth = 1, Name = "xmlns:a10", Prefix = "xmlns", LocalName = "a10", NamespaceURI = "http://www.w3.org/2000/xmlns/", Value = "http://www.w3.org/2005/Atom", IsEmptyElement = false, AttributeCount = 2, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "4: Attribute, Name=\"version\", Value=\"2.0\"", NodeType = XmlNodeType.Attribute, Depth = 1, Name = "version", Prefix = "", LocalName = "version", NamespaceURI = "", Value = "2.0", IsEmptyElement = false, AttributeCount = 2, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "5: Element, Name=\"channel\"", NodeType = XmlNodeType.Element, Depth = 1, Name = "channel", Prefix = "", LocalName = "channel", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "6: Attribute, Name=\"xml:base\", Value=\"https://user-ctp.windows.net/V0.1/\"", NodeType = XmlNodeType.Attribute, Depth = 2, Name = "xml:base", Prefix = "xml", LocalName = "base", NamespaceURI = "http://www.w3.org/XML/1998/namespace", Value = "https://user-ctp.windows.net/V0.1/", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "7: Element, Name=\"title\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "title", Prefix = "", LocalName = "title", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "8: Text, Value=\"Applications\"", NodeType = XmlNodeType.Text, Depth = 3, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "Applications", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "9: EndElement, Name=\"title\"", NodeType = XmlNodeType.EndElement, Depth = 2, Name = "title", Prefix = "", LocalName = "title", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "10: Element, Name=\"description\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "description", Prefix = "", LocalName = "description", NamespaceURI = "", Value = "", IsEmptyElement = true, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "11: Element, Name=\"a10:link\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "a10:link", Prefix = "a10", LocalName = "link", NamespaceURI = "http://www.w3.org/2005/Atom", Value = "", IsEmptyElement = true, AttributeCount = 3, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "12: Attribute, Name=\"rel\", Value=\"self\"", NodeType = XmlNodeType.Attribute, Depth = 3, Name = "rel", Prefix = "", LocalName = "rel", NamespaceURI = "", Value = "self", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "13: Attribute, Name=\"title\", Value=\"self\"", NodeType = XmlNodeType.Attribute, Depth = 3, Name = "title", Prefix = "", LocalName = "title", NamespaceURI = "", Value = "self", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "14: Attribute, Name=\"href\", Value=\"Mesh/Applications\"", NodeType = XmlNodeType.Attribute, Depth = 3, Name = "href", Prefix = "", LocalName = "href", NamespaceURI = "", Value = "Mesh/Applications", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "15: Element, Name=\"a10:link\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "a10:link", Prefix = "a10", LocalName = "link", NamespaceURI = "http://www.w3.org/2005/Atom", Value = "", IsEmptyElement = true, AttributeCount = 3, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "16: Attribute, Name=\"rel\", Value=\"edit\"", NodeType = XmlNodeType.Attribute, Depth = 3, Name = "rel", Prefix = "", LocalName = "rel", NamespaceURI = "", Value = "edit", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "17: Attribute, Name=\"title\", Value=\"edit\"", NodeType = XmlNodeType.Attribute, Depth = 3, Name = "title", Prefix = "", LocalName = "title", NamespaceURI = "", Value = "edit", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "18: Attribute, Name=\"href\", Value=\"Mesh/Applications\"", NodeType = XmlNodeType.Attribute, Depth = 3, Name = "href", Prefix = "", LocalName = "href", NamespaceURI = "", Value = "Mesh/Applications", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "19: Element, Name=\"a10:link\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "a10:link", Prefix = "a10", LocalName = "link", NamespaceURI = "http://www.w3.org/2005/Atom", Value = "", IsEmptyElement = true, AttributeCount = 3, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "20: Attribute, Name=\"rel\", Value=\"LiveFX/Subscription\"", NodeType = XmlNodeType.Attribute, Depth = 3, Name = "rel", Prefix = "", LocalName = "rel", NamespaceURI = "", Value = "LiveFX/Subscription", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "21: Attribute, Name=\"title\", Value=\"LiveFX/Subscription\"", NodeType = XmlNodeType.Attribute, Depth = 3, Name = "title", Prefix = "", LocalName = "title", NamespaceURI = "", Value = "LiveFX/Subscription", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "22: Attribute, Name=\"href\", Value=\"Mesh/Applications/Subscriptions\"", NodeType = XmlNodeType.Attribute, Depth = 3, Name = "href", Prefix = "", LocalName = "href", NamespaceURI = "", Value = "Mesh/Applications/Subscriptions", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "23: Element, Name=\"item\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "item", Prefix = "", LocalName = "item", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "24: Element, Name=\"guid\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "guid", Prefix = "", LocalName = "guid", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "25: Attribute, Name=\"isPermaLink\", Value=\"false\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "isPermaLink", Prefix = "", LocalName = "isPermaLink", NamespaceURI = "", Value = "false", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "26: Text, Value=\"5ab79699-8e11-4cdf-b3d0-534de5e44b44\"", NodeType = XmlNodeType.Text, Depth = 4, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "5ab79699-8e11-4cdf-b3d0-534de5e44b44", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "27: EndElement, Name=\"guid\"", NodeType = XmlNodeType.EndElement, Depth = 3, Name = "guid", Prefix = "", LocalName = "guid", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "28: Element, Name=\"author\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "author", Prefix = "", LocalName = "author", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "29: Text, Value=\"meshuser@live.com\"", NodeType = XmlNodeType.Text, Depth = 4, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "meshuser@live.com", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "30: EndElement, Name=\"author\"", NodeType = XmlNodeType.EndElement, Depth = 3, Name = "author", Prefix = "", LocalName = "author", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "31: Element, Name=\"category\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "category", Prefix = "", LocalName = "category", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "32: Attribute, Name=\"domain\", Value=\"http://user.windows.net/Resource\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "domain", Prefix = "", LocalName = "domain", NamespaceURI = "", Value = "http://user.windows.net/Resource", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "33: Text, Value=\"Application\"", NodeType = XmlNodeType.Text, Depth = 4, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "Application", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "34: EndElement, Name=\"category\"", NodeType = XmlNodeType.EndElement, Depth = 3, Name = "category", Prefix = "", LocalName = "category", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "35: Element, Name=\"title\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "title", Prefix = "", LocalName = "title", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "36: Text, Value=\"Collaborative Crossword\"", NodeType = XmlNodeType.Text, Depth = 4, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "Collaborative Crossword", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "37: EndElement, Name=\"title\"", NodeType = XmlNodeType.EndElement, Depth = 3, Name = "title", Prefix = "", LocalName = "title", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "38: Element, Name=\"pubDate\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "pubDate", Prefix = "", LocalName = "pubDate", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "39: Text, Value=\"Fri, 24 Oct 2008 21:56:33 Z\"", NodeType = XmlNodeType.Text, Depth = 4, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "Fri, 24 Oct 2008 21:56:33 Z", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "40: EndElement, Name=\"pubDate\"", NodeType = XmlNodeType.EndElement, Depth = 3, Name = "pubDate", Prefix = "", LocalName = "pubDate", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "41: Element, Name=\"a10:link\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "a10:link", Prefix = "a10", LocalName = "link", NamespaceURI = "http://www.w3.org/2005/Atom", Value = "", IsEmptyElement = true, AttributeCount = 3, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "42: Attribute, Name=\"rel\", Value=\"LiveFX/ApplicationPackage\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "rel", Prefix = "", LocalName = "rel", NamespaceURI = "", Value = "LiveFX/ApplicationPackage", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "43: Attribute, Name=\"title\", Value=\"LiveFX/ApplicationPackage\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "title", Prefix = "", LocalName = "title", NamespaceURI = "", Value = "LiveFX/ApplicationPackage", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "44: Attribute, Name=\"href\", Value=\"Mesh/Applications/TGLLOWQRR3PUZM6QKNG6LZCLIQ/Package/N4H3PCMTFFEUFE74LLEHCOAT3E-HTBKUY5U35CU7AIB2YASB7EOLI-Manifest.xml\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "href", Prefix = "", LocalName = "href", NamespaceURI = "", Value = "Mesh/Applications/TGLLOWQRR3PUZM6QKNG6LZCLIQ/Package/N4H3PCMTFFEUFE74LLEHCOAT3E-HTBKUY5U35CU7AIB2YASB7EOLI-Manifest.xml", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "45: Element, Name=\"a10:link\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "a10:link", Prefix = "a10", LocalName = "link", NamespaceURI = "http://www.w3.org/2005/Atom", Value = "", IsEmptyElement = true, AttributeCount = 3, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "46: Attribute, Name=\"rel\", Value=\"LiveFX/ApplicationLogo\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "rel", Prefix = "", LocalName = "rel", NamespaceURI = "", Value = "LiveFX/ApplicationLogo", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "47: Attribute, Name=\"title\", Value=\"LiveFX/ApplicationLogo\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "title", Prefix = "", LocalName = "title", NamespaceURI = "", Value = "LiveFX/ApplicationLogo", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "48: Attribute, Name=\"href\", Value=\"Mesh/Applications/TGLLOWQRR3PUZM6QKNG6LZCLIQ/Package/N4H3PCMTFFEUFE74LLEHCOAT3E-HTBKUY5U35CU7AIB2YASB7EOLI-logo.png\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "href", Prefix = "", LocalName = "href", NamespaceURI = "", Value = "Mesh/Applications/TGLLOWQRR3PUZM6QKNG6LZCLIQ/Package/N4H3PCMTFFEUFE74LLEHCOAT3E-HTBKUY5U35CU7AIB2YASB7EOLI-logo.png", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "49: Element, Name=\"a10:link\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "a10:link", Prefix = "a10", LocalName = "link", NamespaceURI = "http://www.w3.org/2005/Atom", Value = "", IsEmptyElement = true, AttributeCount = 3, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "50: Attribute, Name=\"rel\", Value=\"LiveFX/Subscription\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "rel", Prefix = "", LocalName = "rel", NamespaceURI = "", Value = "LiveFX/Subscription", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "51: Attribute, Name=\"title\", Value=\"LiveFX/Subscription\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "title", Prefix = "", LocalName = "title", NamespaceURI = "", Value = "LiveFX/Subscription", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "52: Attribute, Name=\"href\", Value=\"Mesh/Applications/TGLLOWQRR3PUZM6QKNG6LZCLIQ/Subscriptions\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "href", Prefix = "", LocalName = "href", NamespaceURI = "", Value = "Mesh/Applications/TGLLOWQRR3PUZM6QKNG6LZCLIQ/Subscriptions", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "53: Element, Name=\"a10:link\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "a10:link", Prefix = "a10", LocalName = "link", NamespaceURI = "http://www.w3.org/2005/Atom", Value = "", IsEmptyElement = true, AttributeCount = 3, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "54: Attribute, Name=\"rel\", Value=\"LiveFX/ResourceFeeds\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "rel", Prefix = "", LocalName = "rel", NamespaceURI = "", Value = "LiveFX/ResourceFeeds", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "55: Attribute, Name=\"title\", Value=\"LiveFX/ResourceFeeds\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "title", Prefix = "", LocalName = "title", NamespaceURI = "", Value = "LiveFX/ResourceFeeds", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "56: Attribute, Name=\"href\", Value=\"Mesh/Applications/TGLLOWQRR3PUZM6QKNG6LZCLIQ/ResourceFeeds\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "href", Prefix = "", LocalName = "href", NamespaceURI = "", Value = "Mesh/Applications/TGLLOWQRR3PUZM6QKNG6LZCLIQ/ResourceFeeds", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "57: Element, Name=\"a10:link\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "a10:link", Prefix = "a10", LocalName = "link", NamespaceURI = "http://www.w3.org/2005/Atom", Value = "", IsEmptyElement = true, AttributeCount = 3, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "58: Attribute, Name=\"rel\", Value=\"self\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "rel", Prefix = "", LocalName = "rel", NamespaceURI = "", Value = "self", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "59: Attribute, Name=\"title\", Value=\"self\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "title", Prefix = "", LocalName = "title", NamespaceURI = "", Value = "self", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "60: Attribute, Name=\"href\", Value=\"Mesh/Applications/TGLLOWQRR3PUZM6QKNG6LZCLIQ\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "href", Prefix = "", LocalName = "href", NamespaceURI = "", Value = "Mesh/Applications/TGLLOWQRR3PUZM6QKNG6LZCLIQ", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "61: Element, Name=\"a10:link\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "a10:link", Prefix = "a10", LocalName = "link", NamespaceURI = "http://www.w3.org/2005/Atom", Value = "", IsEmptyElement = true, AttributeCount = 3, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "62: Attribute, Name=\"rel\", Value=\"edit\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "rel", Prefix = "", LocalName = "rel", NamespaceURI = "", Value = "edit", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "63: Attribute, Name=\"title\", Value=\"edit\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "title", Prefix = "", LocalName = "title", NamespaceURI = "", Value = "edit", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "64: Attribute, Name=\"href\", Value=\"Mesh/Applications/TGLLOWQRR3PUZM6QKNG6LZCLIQ\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "href", Prefix = "", LocalName = "href", NamespaceURI = "", Value = "Mesh/Applications/TGLLOWQRR3PUZM6QKNG6LZCLIQ", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "65: Element, Name=\"a10:updated\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "a10:updated", Prefix = "a10", LocalName = "updated", NamespaceURI = "http://www.w3.org/2005/Atom", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "66: Text, Value=\"2008-10-25T23:12:55Z\"", NodeType = XmlNodeType.Text, Depth = 4, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "2008-10-25T23:12:55Z", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "67: EndElement, Name=\"a10:updated\"", NodeType = XmlNodeType.EndElement, Depth = 3, Name = "a10:updated", Prefix = "a10", LocalName = "updated", NamespaceURI = "http://www.w3.org/2005/Atom", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "68: Element, Name=\"a10:content\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "a10:content", Prefix = "a10", LocalName = "content", NamespaceURI = "http://www.w3.org/2005/Atom", Value = "", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "69: Attribute, Name=\"type\", Value=\"application/xml\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "type", Prefix = "", LocalName = "type", NamespaceURI = "", Value = "application/xml", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "70: Element, Name=\"ApplicationContent\"", NodeType = XmlNodeType.Element, Depth = 4, Name = "ApplicationContent", Prefix = "", LocalName = "ApplicationContent", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 2, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "71: Attribute, Name=\"xmlns:i\", Value=\"http://www.w3.org/2001/XMLSchema-instance\"", NodeType = XmlNodeType.Attribute, Depth = 5, Name = "xmlns:i", Prefix = "xmlns", LocalName = "i", NamespaceURI = "http://www.w3.org/2000/xmlns/", Value = "http://www.w3.org/2001/XMLSchema-instance", IsEmptyElement = false, AttributeCount = 2, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "72: Attribute, Name=\"xmlns\", Value=\"http://user.windows.net\"", NodeType = XmlNodeType.Attribute, Depth = 5, Name = "xmlns", Prefix = "", LocalName = "xmlns", NamespaceURI = "http://www.w3.org/2000/xmlns/", Value = "http://user.windows.net", IsEmptyElement = false, AttributeCount = 2, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "73: Element, Name=\"ApplicationId\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "ApplicationId", Prefix = "", LocalName = "ApplicationId", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "74: Text, Value=\"000000004800272D\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "000000004800272D", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "75: EndElement, Name=\"ApplicationId\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "ApplicationId", Prefix = "", LocalName = "ApplicationId", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "76: Element, Name=\"Approved\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "Approved", Prefix = "", LocalName = "Approved", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "77: Text, Value=\"true\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "true", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "78: EndElement, Name=\"Approved\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "Approved", Prefix = "", LocalName = "Approved", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "79: Element, Name=\"CurrentVersion\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "CurrentVersion", Prefix = "", LocalName = "CurrentVersion", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "80: Text, Value=\"1\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "1", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "81: EndElement, Name=\"CurrentVersion\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "CurrentVersion", Prefix = "", LocalName = "CurrentVersion", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "82: Element, Name=\"Description\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "Description", Prefix = "", LocalName = "Description", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "83: Text, Value=\"Play crosswords with friends and family.\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "Play crosswords with friends and family.", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "84: EndElement, Name=\"Description\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "Description", Prefix = "", LocalName = "Description", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "85: Element, Name=\"DisplayVersion\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "DisplayVersion", Prefix = "", LocalName = "DisplayVersion", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "86: Text, Value=\"0.94\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "0.94", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "87: EndElement, Name=\"DisplayVersion\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "DisplayVersion", Prefix = "", LocalName = "DisplayVersion", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "88: Element, Name=\"LogoName\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "LogoName", Prefix = "", LocalName = "LogoName", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "89: Text, Value=\"logo.png\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "logo.png", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "90: EndElement, Name=\"LogoName\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "LogoName", Prefix = "", LocalName = "LogoName", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "91: Element, Name=\"MultiInstance\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "MultiInstance", Prefix = "", LocalName = "MultiInstance", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "92: Text, Value=\"true\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "true", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "93: EndElement, Name=\"MultiInstance\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "MultiInstance", Prefix = "", LocalName = "MultiInstance", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "94: Element, Name=\"Name\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "Name", Prefix = "", LocalName = "Name", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "95: Text, Value=\"Collaborative Crossword\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "Collaborative Crossword", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "96: EndElement, Name=\"Name\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "Name", Prefix = "", LocalName = "Name", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "97: Element, Name=\"PackageName\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "PackageName", Prefix = "", LocalName = "PackageName", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "98: Text, Value=\"Manifest.xml\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "Manifest.xml", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "99: EndElement, Name=\"PackageName\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "PackageName", Prefix = "", LocalName = "PackageName", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "100: Element, Name=\"PrivacyPolicyLink\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "PrivacyPolicyLink", Prefix = "", LocalName = "PrivacyPolicyLink", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "101: Text, Value=\"http://privacy.microsoft.com/en-us/default.mspx\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "http://privacy.microsoft.com/en-us/default.mspx", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "102: EndElement, Name=\"PrivacyPolicyLink\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "PrivacyPolicyLink", Prefix = "", LocalName = "PrivacyPolicyLink", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "103: Element, Name=\"PublisherName\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "PublisherName", Prefix = "", LocalName = "PublisherName", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "104: Text, Value=\"www.microsoftstartuplabs.com\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "www.microsoftstartuplabs.com", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "105: EndElement, Name=\"PublisherName\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "PublisherName", Prefix = "", LocalName = "PublisherName", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "106: Element, Name=\"ReleaseStatus\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "ReleaseStatus", Prefix = "", LocalName = "ReleaseStatus", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "107: Text, Value=\"Production\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "Production", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "108: EndElement, Name=\"ReleaseStatus\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "ReleaseStatus", Prefix = "", LocalName = "ReleaseStatus", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "109: Element, Name=\"RootDocument\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "RootDocument", Prefix = "", LocalName = "RootDocument", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "110: Text, Value=\"index.html\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "index.html", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "111: EndElement, Name=\"RootDocument\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "RootDocument", Prefix = "", LocalName = "RootDocument", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "112: Element, Name=\"SupportLink\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "SupportLink", Prefix = "", LocalName = "SupportLink", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "113: Text, Value=\"http://www.microsoftstartuplabs.com/support/\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "http://www.microsoftstartuplabs.com/support/", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "114: EndElement, Name=\"SupportLink\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "SupportLink", Prefix = "", LocalName = "SupportLink", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "115: Element, Name=\"VendorLink\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "VendorLink", Prefix = "", LocalName = "VendorLink", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "116: Text, Value=\"http://www.microsoftstartuplabs.com/\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "http://www.microsoftstartuplabs.com/", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "117: EndElement, Name=\"VendorLink\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "VendorLink", Prefix = "", LocalName = "VendorLink", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "118: EndElement, Name=\"ApplicationContent\"", NodeType = XmlNodeType.EndElement, Depth = 4, Name = "ApplicationContent", Prefix = "", LocalName = "ApplicationContent", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "119: EndElement, Name=\"a10:content\"", NodeType = XmlNodeType.EndElement, Depth = 3, Name = "a10:content", Prefix = "a10", LocalName = "content", NamespaceURI = "http://www.w3.org/2005/Atom", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "120: EndElement, Name=\"item\"", NodeType = XmlNodeType.EndElement, Depth = 2, Name = "item", Prefix = "", LocalName = "item", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "121: Element, Name=\"item\"", NodeType = XmlNodeType.Element, Depth = 2, Name = "item", Prefix = "", LocalName = "item", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "122: Element, Name=\"guid\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "guid", Prefix = "", LocalName = "guid", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "123: Attribute, Name=\"isPermaLink\", Value=\"false\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "isPermaLink", Prefix = "", LocalName = "isPermaLink", NamespaceURI = "", Value = "false", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "124: Text, Value=\"bb44a24a-1bdf-452b-9a00-a4273befa694\"", NodeType = XmlNodeType.Text, Depth = 4, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "bb44a24a-1bdf-452b-9a00-a4273befa694", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "125: EndElement, Name=\"guid\"", NodeType = XmlNodeType.EndElement, Depth = 3, Name = "guid", Prefix = "", LocalName = "guid", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "126: Element, Name=\"author\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "author", Prefix = "", LocalName = "author", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "127: Text, Value=\"meshuser@live.com\"", NodeType = XmlNodeType.Text, Depth = 4, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "meshuser@live.com", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "128: EndElement, Name=\"author\"", NodeType = XmlNodeType.EndElement, Depth = 3, Name = "author", Prefix = "", LocalName = "author", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "129: Element, Name=\"category\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "category", Prefix = "", LocalName = "category", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "130: Attribute, Name=\"domain\", Value=\"http://user.windows.net/Resource\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "domain", Prefix = "", LocalName = "domain", NamespaceURI = "", Value = "http://user.windows.net/Resource", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "131: Text, Value=\"Application\"", NodeType = XmlNodeType.Text, Depth = 4, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "Application", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "132: EndElement, Name=\"category\"", NodeType = XmlNodeType.EndElement, Depth = 3, Name = "category", Prefix = "", LocalName = "category", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "133: Element, Name=\"title\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "title", Prefix = "", LocalName = "title", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "134: Text, Value=\"Corkboard\"", NodeType = XmlNodeType.Text, Depth = 4, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "Corkboard", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "135: EndElement, Name=\"title\"", NodeType = XmlNodeType.EndElement, Depth = 3, Name = "title", Prefix = "", LocalName = "title", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "136: Element, Name=\"pubDate\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "pubDate", Prefix = "", LocalName = "pubDate", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "137: Text, Value=\"Fri, 24 Oct 2008 19:57:44 Z\"", NodeType = XmlNodeType.Text, Depth = 4, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "Fri, 24 Oct 2008 19:57:44 Z", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "138: EndElement, Name=\"pubDate\"", NodeType = XmlNodeType.EndElement, Depth = 3, Name = "pubDate", Prefix = "", LocalName = "pubDate", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "139: Element, Name=\"a10:link\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "a10:link", Prefix = "a10", LocalName = "link", NamespaceURI = "http://www.w3.org/2005/Atom", Value = "", IsEmptyElement = true, AttributeCount = 3, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "140: Attribute, Name=\"rel\", Value=\"LiveFX/ApplicationPackage\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "rel", Prefix = "", LocalName = "rel", NamespaceURI = "", Value = "LiveFX/ApplicationPackage", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "141: Attribute, Name=\"title\", Value=\"LiveFX/ApplicationPackage\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "title", Prefix = "", LocalName = "title", NamespaceURI = "", Value = "LiveFX/ApplicationPackage", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "142: Attribute, Name=\"href\", Value=\"Mesh/Applications/JKREJO67DMVULGQAUQTTX35GSQ/Package/6PSURE3UVB3EVAVSWP6GDXHEYA-AJWV2NVTT7CEPOJF625JNDNOJI-Manifest.xml\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "href", Prefix = "", LocalName = "href", NamespaceURI = "", Value = "Mesh/Applications/JKREJO67DMVULGQAUQTTX35GSQ/Package/6PSURE3UVB3EVAVSWP6GDXHEYA-AJWV2NVTT7CEPOJF625JNDNOJI-Manifest.xml", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "143: Element, Name=\"a10:link\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "a10:link", Prefix = "a10", LocalName = "link", NamespaceURI = "http://www.w3.org/2005/Atom", Value = "", IsEmptyElement = true, AttributeCount = 3, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "144: Attribute, Name=\"rel\", Value=\"LiveFX/ApplicationLogo\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "rel", Prefix = "", LocalName = "rel", NamespaceURI = "", Value = "LiveFX/ApplicationLogo", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "145: Attribute, Name=\"title\", Value=\"LiveFX/ApplicationLogo\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "title", Prefix = "", LocalName = "title", NamespaceURI = "", Value = "LiveFX/ApplicationLogo", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "146: Attribute, Name=\"href\", Value=\"Mesh/Applications/JKREJO67DMVULGQAUQTTX35GSQ/Package/6PSURE3UVB3EVAVSWP6GDXHEYA-AJWV2NVTT7CEPOJF625JNDNOJI-logo.png\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "href", Prefix = "", LocalName = "href", NamespaceURI = "", Value = "Mesh/Applications/JKREJO67DMVULGQAUQTTX35GSQ/Package/6PSURE3UVB3EVAVSWP6GDXHEYA-AJWV2NVTT7CEPOJF625JNDNOJI-logo.png", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "147: Element, Name=\"a10:link\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "a10:link", Prefix = "a10", LocalName = "link", NamespaceURI = "http://www.w3.org/2005/Atom", Value = "", IsEmptyElement = true, AttributeCount = 3, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "148: Attribute, Name=\"rel\", Value=\"LiveFX/Subscription\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "rel", Prefix = "", LocalName = "rel", NamespaceURI = "", Value = "LiveFX/Subscription", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "149: Attribute, Name=\"title\", Value=\"LiveFX/Subscription\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "title", Prefix = "", LocalName = "title", NamespaceURI = "", Value = "LiveFX/Subscription", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "150: Attribute, Name=\"href\", Value=\"Mesh/Applications/JKREJO67DMVULGQAUQTTX35GSQ/Subscriptions\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "href", Prefix = "", LocalName = "href", NamespaceURI = "", Value = "Mesh/Applications/JKREJO67DMVULGQAUQTTX35GSQ/Subscriptions", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "151: Element, Name=\"a10:link\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "a10:link", Prefix = "a10", LocalName = "link", NamespaceURI = "http://www.w3.org/2005/Atom", Value = "", IsEmptyElement = true, AttributeCount = 3, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "152: Attribute, Name=\"rel\", Value=\"LiveFX/ResourceFeeds\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "rel", Prefix = "", LocalName = "rel", NamespaceURI = "", Value = "LiveFX/ResourceFeeds", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "153: Attribute, Name=\"title\", Value=\"LiveFX/ResourceFeeds\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "title", Prefix = "", LocalName = "title", NamespaceURI = "", Value = "LiveFX/ResourceFeeds", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "154: Attribute, Name=\"href\", Value=\"Mesh/Applications/JKREJO67DMVULGQAUQTTX35GSQ/ResourceFeeds\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "href", Prefix = "", LocalName = "href", NamespaceURI = "", Value = "Mesh/Applications/JKREJO67DMVULGQAUQTTX35GSQ/ResourceFeeds", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "155: Element, Name=\"a10:link\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "a10:link", Prefix = "a10", LocalName = "link", NamespaceURI = "http://www.w3.org/2005/Atom", Value = "", IsEmptyElement = true, AttributeCount = 3, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "156: Attribute, Name=\"rel\", Value=\"self\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "rel", Prefix = "", LocalName = "rel", NamespaceURI = "", Value = "self", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "157: Attribute, Name=\"title\", Value=\"self\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "title", Prefix = "", LocalName = "title", NamespaceURI = "", Value = "self", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "158: Attribute, Name=\"href\", Value=\"Mesh/Applications/JKREJO67DMVULGQAUQTTX35GSQ\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "href", Prefix = "", LocalName = "href", NamespaceURI = "", Value = "Mesh/Applications/JKREJO67DMVULGQAUQTTX35GSQ", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "159: Element, Name=\"a10:link\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "a10:link", Prefix = "a10", LocalName = "link", NamespaceURI = "http://www.w3.org/2005/Atom", Value = "", IsEmptyElement = true, AttributeCount = 3, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "160: Attribute, Name=\"rel\", Value=\"edit\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "rel", Prefix = "", LocalName = "rel", NamespaceURI = "", Value = "edit", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "161: Attribute, Name=\"title\", Value=\"edit\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "title", Prefix = "", LocalName = "title", NamespaceURI = "", Value = "edit", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "162: Attribute, Name=\"href\", Value=\"Mesh/Applications/JKREJO67DMVULGQAUQTTX35GSQ\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "href", Prefix = "", LocalName = "href", NamespaceURI = "", Value = "Mesh/Applications/JKREJO67DMVULGQAUQTTX35GSQ", IsEmptyElement = false, AttributeCount = 3, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "163: Element, Name=\"a10:updated\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "a10:updated", Prefix = "a10", LocalName = "updated", NamespaceURI = "http://www.w3.org/2005/Atom", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "164: Text, Value=\"2008-11-21T17:16:29Z\"", NodeType = XmlNodeType.Text, Depth = 4, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "2008-11-21T17:16:29Z", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "165: EndElement, Name=\"a10:updated\"", NodeType = XmlNodeType.EndElement, Depth = 3, Name = "a10:updated", Prefix = "a10", LocalName = "updated", NamespaceURI = "http://www.w3.org/2005/Atom", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "166: Element, Name=\"a10:content\"", NodeType = XmlNodeType.Element, Depth = 3, Name = "a10:content", Prefix = "a10", LocalName = "content", NamespaceURI = "http://www.w3.org/2005/Atom", Value = "", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "167: Attribute, Name=\"type\", Value=\"application/xml\"", NodeType = XmlNodeType.Attribute, Depth = 4, Name = "type", Prefix = "", LocalName = "type", NamespaceURI = "", Value = "application/xml", IsEmptyElement = false, AttributeCount = 1, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "168: Element, Name=\"ApplicationContent\"", NodeType = XmlNodeType.Element, Depth = 4, Name = "ApplicationContent", Prefix = "", LocalName = "ApplicationContent", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 2, HasAttributes = true, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToFirstAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "169: Attribute, Name=\"xmlns:i\", Value=\"http://www.w3.org/2001/XMLSchema-instance\"", NodeType = XmlNodeType.Attribute, Depth = 5, Name = "xmlns:i", Prefix = "xmlns", LocalName = "i", NamespaceURI = "http://www.w3.org/2000/xmlns/", Value = "http://www.w3.org/2001/XMLSchema-instance", IsEmptyElement = false, AttributeCount = 2, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.MoveToNextAttribute();
                VerifyReader(reader, new XmlReaderState { StateName = "170: Attribute, Name=\"xmlns\", Value=\"http://user.windows.net\"", NodeType = XmlNodeType.Attribute, Depth = 5, Name = "xmlns", Prefix = "", LocalName = "xmlns", NamespaceURI = "http://www.w3.org/2000/xmlns/", Value = "http://user.windows.net", IsEmptyElement = false, AttributeCount = 2, HasAttributes = true, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "171: Element, Name=\"ApplicationId\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "ApplicationId", Prefix = "", LocalName = "ApplicationId", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "172: Text, Value=\"0000000040003417\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "0000000040003417", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "173: EndElement, Name=\"ApplicationId\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "ApplicationId", Prefix = "", LocalName = "ApplicationId", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "174: Element, Name=\"Approved\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "Approved", Prefix = "", LocalName = "Approved", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "175: Text, Value=\"true\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "true", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "176: EndElement, Name=\"Approved\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "Approved", Prefix = "", LocalName = "Approved", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "177: Element, Name=\"CurrentVersion\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "CurrentVersion", Prefix = "", LocalName = "CurrentVersion", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "178: Text, Value=\"3\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "3", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "179: EndElement, Name=\"CurrentVersion\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "CurrentVersion", Prefix = "", LocalName = "CurrentVersion", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "180: Element, Name=\"Description\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "Description", Prefix = "", LocalName = "Description", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "181: Text, Value=\"Get a digital corkboard and start sharing notes across town or across the globe\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "Get a digital corkboard and start sharing notes across town or across the globe", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "182: EndElement, Name=\"Description\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "Description", Prefix = "", LocalName = "Description", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "183: Element, Name=\"DisplayVersion\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "DisplayVersion", Prefix = "", LocalName = "DisplayVersion", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "184: Text, Value=\"0.82\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "0.82", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "185: EndElement, Name=\"DisplayVersion\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "DisplayVersion", Prefix = "", LocalName = "DisplayVersion", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "186: Element, Name=\"LogoName\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "LogoName", Prefix = "", LocalName = "LogoName", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "187: Text, Value=\"logo.png\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "logo.png", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "188: EndElement, Name=\"LogoName\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "LogoName", Prefix = "", LocalName = "LogoName", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "189: Element, Name=\"MultiInstance\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "MultiInstance", Prefix = "", LocalName = "MultiInstance", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "190: Text, Value=\"true\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "true", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "191: EndElement, Name=\"MultiInstance\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "MultiInstance", Prefix = "", LocalName = "MultiInstance", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "192: Element, Name=\"Name\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "Name", Prefix = "", LocalName = "Name", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "193: Text, Value=\"Corkboard\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "Corkboard", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "194: EndElement, Name=\"Name\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "Name", Prefix = "", LocalName = "Name", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "195: Element, Name=\"PackageName\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "PackageName", Prefix = "", LocalName = "PackageName", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "196: Text, Value=\"Manifest.xml\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "Manifest.xml", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "197: EndElement, Name=\"PackageName\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "PackageName", Prefix = "", LocalName = "PackageName", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "198: Element, Name=\"PrivacyPolicyLink\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "PrivacyPolicyLink", Prefix = "", LocalName = "PrivacyPolicyLink", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "199: Text, Value=\"http://privacy.microsoft.com/en-us/default.mspx\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "http://privacy.microsoft.com/en-us/default.mspx", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "200: EndElement, Name=\"PrivacyPolicyLink\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "PrivacyPolicyLink", Prefix = "", LocalName = "PrivacyPolicyLink", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "201: Element, Name=\"PublisherName\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "PublisherName", Prefix = "", LocalName = "PublisherName", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "202: Text, Value=\"www.microsoftstartuplabs.com\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "www.microsoftstartuplabs.com", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "203: EndElement, Name=\"PublisherName\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "PublisherName", Prefix = "", LocalName = "PublisherName", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "204: Element, Name=\"ReleaseStatus\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "ReleaseStatus", Prefix = "", LocalName = "ReleaseStatus", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "205: Text, Value=\"Production\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "Production", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "206: EndElement, Name=\"ReleaseStatus\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "ReleaseStatus", Prefix = "", LocalName = "ReleaseStatus", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "207: Element, Name=\"RootDocument\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "RootDocument", Prefix = "", LocalName = "RootDocument", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "208: Text, Value=\"index.html\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "index.html", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "209: EndElement, Name=\"RootDocument\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "RootDocument", Prefix = "", LocalName = "RootDocument", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "210: Element, Name=\"SupportLink\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "SupportLink", Prefix = "", LocalName = "SupportLink", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "211: Text, Value=\"http://www.microsoftstartuplabs.com/support/\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "http://www.microsoftstartuplabs.com/support/", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "212: EndElement, Name=\"SupportLink\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "SupportLink", Prefix = "", LocalName = "SupportLink", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "213: Element, Name=\"VendorLink\"", NodeType = XmlNodeType.Element, Depth = 5, Name = "VendorLink", Prefix = "", LocalName = "VendorLink", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "214: Text, Value=\"http://www.microsoftstartuplabs.com/\"", NodeType = XmlNodeType.Text, Depth = 6, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "http://www.microsoftstartuplabs.com/", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = true, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "215: EndElement, Name=\"VendorLink\"", NodeType = XmlNodeType.EndElement, Depth = 5, Name = "VendorLink", Prefix = "", LocalName = "VendorLink", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "216: EndElement, Name=\"ApplicationContent\"", NodeType = XmlNodeType.EndElement, Depth = 4, Name = "ApplicationContent", Prefix = "", LocalName = "ApplicationContent", NamespaceURI = "http://user.windows.net", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "217: EndElement, Name=\"a10:content\"", NodeType = XmlNodeType.EndElement, Depth = 3, Name = "a10:content", Prefix = "a10", LocalName = "content", NamespaceURI = "http://www.w3.org/2005/Atom", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "218: EndElement, Name=\"item\"", NodeType = XmlNodeType.EndElement, Depth = 2, Name = "item", Prefix = "", LocalName = "item", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "219: EndElement, Name=\"channel\"", NodeType = XmlNodeType.EndElement, Depth = 1, Name = "channel", Prefix = "", LocalName = "channel", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "220: EndElement, Name=\"rss\"", NodeType = XmlNodeType.EndElement, Depth = 0, Name = "rss", Prefix = "", LocalName = "rss", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.Interactive, EOF = false, });
                reader.Read();
                VerifyReader(reader, new XmlReaderState { StateName = "221: None", NodeType = XmlNodeType.None, Depth = 0, Name = "", Prefix = "", LocalName = "", NamespaceURI = "", Value = "", IsEmptyElement = false, AttributeCount = 0, HasAttributes = false, HasValue = false, XmlSpace = XmlSpace.None, XmlLang = "", ReadState = ReadState.EndOfFile, EOF = true, });
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }

            return MFTestResults.Pass;
        }

        class XmlReaderState
        {
            public String StateName;

            public XmlNodeType NodeType;
            public int Depth;

            public string Name;
            public string Prefix;
            public string LocalName;
            public string NamespaceURI;
            public string Value;

            public bool IsEmptyElement;
            public int AttributeCount;
            public bool HasAttributes;
            public bool HasValue;

            public XmlSpace XmlSpace;
            public string XmlLang;

            public ReadState ReadState;
            public bool EOF;
        }

        private void VerifyReader(XmlReader reader, XmlReaderState state)
        {
            Log.Comment(state.StateName);

            Assert.AreEqual(state.NodeType, reader.NodeType, state.StateName + " [NodeType]");
            Assert.AreEqual(state.Depth, reader.Depth, state.StateName + " [Depth]");

            Assert.AreEqual(state.Name, reader.Name, state.StateName + " [Name]");
            Assert.AreEqual(state.Prefix, reader.Prefix, state.StateName + " [Prefix]");
            Assert.AreEqual(state.LocalName, reader.LocalName, state.StateName + " [LocalName]");
            Assert.AreEqual(state.NamespaceURI, reader.NamespaceURI, state.StateName + " [NamespaceURI]");
            Assert.AreEqual(state.Value, reader.Value, state.StateName + " [Value]");

            Assert.AreEqual(state.IsEmptyElement, reader.IsEmptyElement, state.StateName + " [IsEmptyElement]");
            Assert.AreEqual(state.AttributeCount, reader.AttributeCount, state.StateName + " [AttributeCount]");
            Assert.AreEqual(state.HasAttributes, reader.HasAttributes, state.StateName + " [HasAttributes]");
            Assert.AreEqual(state.HasValue, reader.HasValue, state.StateName + " [HasValue]");

            Assert.AreEqual(state.XmlSpace, reader.XmlSpace, state.StateName + " [XmlSpace]");
            Assert.AreEqual(state.XmlLang, reader.XmlLang, state.StateName + " [XmlLang]");

            Assert.AreEqual(state.ReadState, reader.ReadState, state.StateName + " [ReadState]");
            Assert.AreEqual(state.EOF, reader.EOF, state.StateName + " [EOF]");
        }
    }
}
