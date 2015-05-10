////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;
using System.Threading;

using System.Collections;
using System.Diagnostics;
using System.Globalization;

using Microsoft.SPOT;

/////////////////////////////////
namespace System.Xml
{

    public class XmlTextReader : XmlReader, IXmlLineInfo
    {
        //
        // Private helper types
        //
        // ParsingFunction = what should the reader do when the next Read() is called
        enum ParsingFunction
        {
            ElementContent = 0,
            NoData,
            OpenUrl,
            SwitchToInteractive,
            SwitchToInteractiveXmlDecl,
            DocumentContent,
            MoveToElementContent,
            PopElementContext,
            PopEmptyElementContext,
            ResetAttributesRootLevel,
            Error,
            Eof,
            ReaderClosed,
            InIncrementalRead,
            FragmentAttribute,
            XmlDeclarationFragment,
            GoToEof,
            PartialTextValue,

            // these two states must be last; see InAttributeValueIterator property
            InReadAttributeValue,
            InReadValueChunk,
        }

        enum ParsingMode
        {
            Full,
            SkipNode,
            SkipContent,
        }

        enum EntityType
        {
            CharacterDec,
            CharacterHex,
            CharacterNamed,
            Expanded,
            ExpandedInAttribute,
            Skipped,
            Unexpanded,
            FakeExpanded,
        }

        enum EntityExpandType
        {
            OnlyGeneral,
            OnlyCharacter,
            All,
        }

        enum IncrementalReadState
        {
            // Following values are used in ReadText, ReadBase64 and ReadBinHex (V1 streaming methods)
            Text,
            StartTag,
            PI,
            CDATA,
            Comment,
            Attributes,
            AttributeValue,
            ReadData,
            EndElement,
            End,

            // Following values are used in ReadTextChunk, ReadContentAsBase64 and ReadBinHexChunk (V2 streaming methods)
            ReadValueChunk_OnCachedValue,
            ReadValueChunk_OnPartialValue,
        }

        //
        // Fields
        //
        // XmlCharType instance
        XmlCharType xmlCharType = XmlCharType.Instance;

        // current parsing state (aka. scanner data)
        ParsingState ps;

        // parsing function = what to do in the next Read() (3-items-long stack, usually used just 2 level)
        ParsingFunction parsingFunction;
        ParsingFunction nextParsingFunction;
        ParsingFunction nextNextParsingFunction;

        // stack of nodes
        NodeData[] nodes;

        // current node
        NodeData curNode;

        // current index
        int index = 0;

        // attributes info
        int curAttrIndex = -1;
        int attrCount;
        int attrHashtable;
        int attrDuplWalkCount;
        bool fullAttrCleanup;

        // name table
        XmlNameTable nameTable;
        bool nameTableFromSettings;

        // settings
        bool normalize;
        WhitespaceHandling whitespaceHandling;
        EntityHandling entityHandling;
        bool ignorePIs;
        bool ignoreComments;
        bool checkCharacters;
        int lineNumberOffset;
        int linePositionOffset;
        bool closeInput;

        // xml context (xml:space, xml:lang, default namespace)
        XmlContext xmlContext;

        // current node base uri and encoding
        string reportedBaseUri;
        Encoding reportedEncoding;

        // fragment parsing
        XmlNodeType fragmentType = XmlNodeType.Document;

        // incremental read
        IncrementalReadDecoder incReadDecoder;
        IncrementalReadState incReadState;
        int incReadDepth;
        int incReadLeftStartPos;
        int incReadLeftEndPos;
        LineInfo incReadLineInfo;
        IncrementalReadCharsDecoder readCharsDecoder;

        // XmlValidatingReader helpers
#if SCHEMA_VALIDATION
        ValidationEventHandler  validationEventHandler;
#endif //SCHEMA_VALIDATION

        // misc
        BufferBuilder stringBuilder;
        bool rootElementParsed;
        bool standalone;
        ParsingMode parsingMode;
        ReadState readState = ReadState.Initial;
#if SCHEMA_VALIDATION
        SchemaEntity    lastEntity;
#endif //SCHEMA_VALIDATION

        //bool afterResetState;
        int documentStartBytePos;
        int readValueOffset;
        //
        // Atomized string constants
        //
        private string Xml;
        private string XmlNs;

        //
        // Constants
        //
        private const int MaxBytesToMove = 128;
        private const int ApproxXmlDeclLength = 80;
        private const int NodesInitialSize = 8;
        private const int InitialAttributesCount = 4;
        private const int InitialParsingStateStackSize = 2;
        private const int InitialParsingStatesDepth = 2;
        private const int DtdChidrenInitialSize = 2;
        private const int MaxByteSequenceLen = 6;  // max bytes per character
        private const int MaxAttrDuplWalkCount = 250;
        private const int MinWhitespaceLookahedCount = 4096;

        private const string XmlDeclarationBegining = "<?xml";

        internal const int SurHighStart = 0xd800;
        internal const int SurHighEnd = 0xdbff;
        internal const int SurLowStart = 0xdc00;
        internal const int SurLowEnd = 0xdfff;

        //
        // Constructors
        //
        internal XmlTextReader()
        {
            curNode = new NodeData();
            parsingFunction = ParsingFunction.NoData;
        }

        // Initializes a new instance of the XmlTextReader class with the specified XmlNameTable.
        internal XmlTextReader(XmlNameTable nt)
        {
            Debug.Assert(nt != null);

            nameTable = nt;
            nt.Add("");
            Xml = nt.Add("xml");
            XmlNs = nt.Add("xmlns");

            Debug.Assert(index == 0);
            nodes = new NodeData[NodesInitialSize];
            nodes[0] = new NodeData();
            curNode = nodes[0];

            stringBuilder = new BufferBuilder();
            xmlContext = new XmlContext();

            parsingFunction = ParsingFunction.SwitchToInteractiveXmlDecl;
            nextParsingFunction = ParsingFunction.DocumentContent;

            entityHandling = EntityHandling.ExpandCharEntities;
            whitespaceHandling = WhitespaceHandling.All;
            closeInput = true;

            ps.lineNo = 1;
            ps.lineStartPos = -1;
        }

        // This constructor is used when creating XmlTextReader reader via "XmlReader.Create(..)"
        private XmlTextReader(XmlReaderSettings settings)
        {
            xmlContext = new XmlContext();

            // create nametable
            XmlNameTable nt = settings.NameTable;
            if (nt == null)
            {
                nt = new NameTable();
                Debug.Assert(nameTableFromSettings == false);
            }
            else
            {
                nameTableFromSettings = true;
            }

            nameTable = nt;

            nt.Add("");
            Xml = nt.Add("xml");
            XmlNs = nt.Add("xmlns");

            Debug.Assert(index == 0);

            nodes = new NodeData[NodesInitialSize];
            nodes[0] = new NodeData();
            curNode = nodes[0];

            stringBuilder = new BufferBuilder();

            entityHandling = EntityHandling.ExpandEntities;
            whitespaceHandling = (settings.IgnoreWhitespace) ? WhitespaceHandling.Significant : WhitespaceHandling.All;
            normalize = true;
            ignorePIs = settings.IgnoreProcessingInstructions;
            ignoreComments = settings.IgnoreComments;
            checkCharacters = settings.CheckCharacters;
            lineNumberOffset = settings.LineNumberOffset;
            linePositionOffset = settings.LinePositionOffset;
            ps.lineNo = lineNumberOffset + 1;
            ps.lineStartPos = -linePositionOffset - 1;
            curNode.SetLineInfo(ps.LineNo - 1, ps.LinePos - 1);

            parsingFunction = ParsingFunction.SwitchToInteractiveXmlDecl;
            nextParsingFunction = ParsingFunction.DocumentContent;

            switch (settings.ConformanceLevel)
            {
                case ConformanceLevel.Auto:
                    fragmentType = XmlNodeType.None;
                    break;
                case ConformanceLevel.Fragment:
                    fragmentType = XmlNodeType.Element;
                    break;
                case ConformanceLevel.Document:
                    fragmentType = XmlNodeType.Document;
                    break;
                default:
                    Debug.Assert(false);
                    goto case ConformanceLevel.Document;
            }
        }

        // Initializes a new instance of the XmlTextReader class with the specified stream, baseUri and nametable
        // This constructor is used when creating XmlTextReader for V1 XmlTextReader
        public XmlTextReader(Stream input)
            : this("", input, new NameTable())
        {
        }

        public XmlTextReader(Stream input, XmlNameTable nt)
            : this("", input, nt)
        {
        }

        internal XmlTextReader(string url, Stream input)
            : this(url, input, new NameTable())
        {
        }

        internal XmlTextReader(string url, Stream input, XmlNameTable nt)
            : this(nt)
        {
            if (url == null || url.Length == 0)
            {
                InitStreamInput(input);
            }
            else
            {
                InitStreamInput(url, input);
            }

            reportedBaseUri = ps.baseUriStr;
            reportedEncoding = ps.encoding;
        }

        // Initializes a new instance of the XmlTextReader class with the specified arguments.
        // This constructor is used when creating XmlTextReader via XmlReader.Create
        internal XmlTextReader(Stream stream, byte[] bytes, int byteCount, XmlReaderSettings settings, string baseUriStr,
                                    bool closeInput)
            : this(settings)
        {

            // init ParsingState
            InitStreamInput(baseUriStr, stream, bytes, byteCount);

            this.closeInput = closeInput;

            reportedBaseUri = ps.baseUriStr;
            reportedEncoding = ps.encoding;
        }

        //
        // XmlReader members
        //
        // Returns the current settings of the reader
        public override XmlReaderSettings Settings
        {
            get
            {

                XmlReaderSettings settings = new XmlReaderSettings();

                if (nameTableFromSettings)
                {
                    settings.NameTable = nameTable;
                }

                switch (fragmentType)
                {
                    case XmlNodeType.None: settings.ConformanceLevel = ConformanceLevel.Auto; break;
                    case XmlNodeType.Element: settings.ConformanceLevel = ConformanceLevel.Fragment; break;
                    case XmlNodeType.Document: settings.ConformanceLevel = ConformanceLevel.Document; break;
                    default: Debug.Assert(false); goto case XmlNodeType.None;
                }
                settings.CheckCharacters = checkCharacters;
                settings.LineNumberOffset = lineNumberOffset;
                settings.LinePositionOffset = linePositionOffset;
                settings.IgnoreWhitespace = (whitespaceHandling == WhitespaceHandling.Significant);
                settings.IgnoreProcessingInstructions = ignorePIs;
                settings.IgnoreComments = ignoreComments;
                settings.ReadOnly = true;
                return settings;
            }
        }

        // Returns the type of the current node.
        public override XmlNodeType NodeType
        {
            get
            {
                return curNode.type;
            }
        }

        // Returns the name of the current node, including prefix.
        public override string Name
        {
            get
            {
                return curNode.GetNameWPrefix(nameTable);
            }
        }

        // Returns local name of the current node (without prefix)
        public override string LocalName
        {
            get
            {
                return curNode.localName;
            }
        }

        // Returns namespace name of the current node.
        public override string NamespaceURI
        {
            get
            {
                // WsdModification - Adds support for namespace Uri's
                return curNode.ns;
            }
        }

        // Returns prefix associated with the current node.
        public override string Prefix
        {
            get
            {
                return curNode.prefix;
            }
        }

        // Returns true if the current node can have Value property != string.Empty.
        public override bool HasValue
        {
            get
            {
                return XmlReader.HasValueInternal(curNode.type);
            }
        }

        // Returns the text value of the current node.
        public override string Value
        {
            get
            {
                if (parsingFunction >= ParsingFunction.PartialTextValue)
                {
                    if (parsingFunction == ParsingFunction.PartialTextValue)
                    {
                        FinishPartialValue();
                        parsingFunction = nextParsingFunction;
                    }
                    else
                    {
                        FinishOtherValueIterator();
                    }
                }

                return curNode.StringValue;
            }
        }

        // Returns the depth of the current node in the XML element stack
        public override int Depth
        {
            get
            {
                return curNode.depth;
            }
        }

        // Returns the base URI of the current node.
        public override string BaseURI
        {
            get
            {
                return reportedBaseUri;
            }
        }

        // Returns true if the current node is an empty element (for example, <MyElement/>).
        public override bool IsEmptyElement
        {
            get
            {
                return curNode.IsEmptyElement;
            }
        }

        // Returns true of the current node is a default attribute declared in DTD.
        public override bool IsDefault
        {
            get
            {
                return curNode.IsDefaultAttribute;
            }
        }

        // Returns the quote character used in the current attribute declaration
        public override char QuoteChar
        {
            get
            {
                return curNode.type == XmlNodeType.Attribute ? curNode.quoteChar : '"';
            }
        }

        // Returns the current xml:space scope.
        public override XmlSpace XmlSpace
        {
            get
            {
                return xmlContext.xmlSpace;
            }
        }

        // Returns the current xml:lang scope.</para>
        public override string XmlLang
        {
            get
            {
                return xmlContext.xmlLang;
            }
        }

        // Returns the current read state of the reader
        public override ReadState ReadState
        {
            get
            {
                return readState;
            }
        }

        // Returns true if the reader reached end of the input data
        public override bool EOF
        {
            get
            {
                return parsingFunction == ParsingFunction.Eof;
            }
        }

        // Returns the XmlNameTable associated with this XmlReader
        public override XmlNameTable NameTable
        {
            get
            {
                return nameTable;
            }
        }

        // Returns true if the XmlReader knows how to resolve general entities
        public override bool CanResolveEntity
        {
            get
            {
                return false;
            }
        }

        // Returns the number of attributes on the current node.
        public override int AttributeCount
        {
            get
            {
                return attrCount;
            }
        }

        // Returns value of an attribute with the specified Name
        public override string GetAttribute(string name)
        {
            int i;
            if (name.IndexOf(':') == -1)
            {
                i = GetIndexOfAttributeWithoutPrefix(name);
            }
            else
            {
                i = GetIndexOfAttributeWithPrefix(name);
            }

            return (i >= 0) ? nodes[i].StringValue : null;
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            if (namespaceURI == null || namespaceURI.Length == 0)
            {
                return GetAttribute(name);
            }

            return null;
        }

        // Returns value of an attribute at the specified index (position)
        public override string GetAttribute(int i)
        {
            if (i < 0 || i >= attrCount)
            {
                throw new ArgumentOutOfRangeException("i");
            }

            return nodes[index + i + 1].StringValue;
        }

        // Moves to an attribute with the specified Name
        public override bool MoveToAttribute(string name)
        {
            int i;
            if (name.IndexOf(':') == -1)
            {
                i = GetIndexOfAttributeWithoutPrefix(name);
            }
            else
            {
                i = GetIndexOfAttributeWithPrefix(name);
            }

            if (i >= 0)
            {
                if (InAttributeValueIterator)
                {
                    FinishAttributeValueIterator();
                }

                curAttrIndex = i - index - 1;
                curNode = nodes[i];
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            if (ns == null || ns.Length == 0)
            {
                return MoveToAttribute(name);
            }

            return false;
        }

        // Moves to an attribute at the specified index (position)
        public override void MoveToAttribute(int i)
        {
            if (i < 0 || i >= attrCount)
            {
                throw new ArgumentOutOfRangeException("i");
            }

            if (InAttributeValueIterator)
            {
                FinishAttributeValueIterator();
            }

            curAttrIndex = i;
            curNode = nodes[index + 1 + curAttrIndex];
        }

        // Moves to the first attribute of the current node
        public override bool MoveToFirstAttribute()
        {
            if (attrCount == 0)
            {
                return false;
            }

            if (InAttributeValueIterator)
            {
                FinishAttributeValueIterator();
            }

            curAttrIndex = 0;
            curNode = nodes[index + 1];

            return true;
        }

        // Moves to the next attribute of the current node
        public override bool MoveToNextAttribute()
        {
            if (curAttrIndex + 1 < attrCount)
            {
                if (InAttributeValueIterator)
                {
                    FinishAttributeValueIterator();
                }

                curNode = nodes[index + 1 + ++curAttrIndex];
                return true;
            }

            return false;
        }

        // If on attribute, moves to the element that contains the attribute node
        public override bool MoveToElement()
        {
            if (InAttributeValueIterator)
            {
                FinishAttributeValueIterator();
            }
            else if (curNode.type != XmlNodeType.Attribute)
            {
                return false;
            }

            curAttrIndex = -1;
            curNode = nodes[index];

            return true;
        }

        // Reads next node from the input data
        public override bool Read()
        {
            for (; ; )
            {
                switch (parsingFunction)
                {
                    case ParsingFunction.ElementContent:
                        return ParseElementContent();
                    case ParsingFunction.DocumentContent:
                        return ParseDocumentContent();
                    case ParsingFunction.SwitchToInteractive:
                        Debug.Assert(!ps.appendMode);
                        readState = ReadState.Interactive;
                        parsingFunction = nextParsingFunction;
                        continue;
                    case ParsingFunction.SwitchToInteractiveXmlDecl:
                        readState = ReadState.Interactive;
                        parsingFunction = nextParsingFunction;
                        if (ParseXmlDeclaration())
                        {
                            reportedEncoding = ps.encoding;
                            return true;
                        }

                        reportedEncoding = ps.encoding;
                        continue;
                    case ParsingFunction.ResetAttributesRootLevel:
                        ResetAttributes();
                        curNode = nodes[index];
                        parsingFunction = (index == 0) ? ParsingFunction.DocumentContent : ParsingFunction.ElementContent;
                        continue;
                    case ParsingFunction.MoveToElementContent:
                        ResetAttributes();
                        index++;
                        curNode = AddNode(index, index);
                        parsingFunction = ParsingFunction.ElementContent;
                        continue;
                    case ParsingFunction.PopElementContext:
                        PopElementContext();
                        parsingFunction = nextParsingFunction;
                        Debug.Assert(parsingFunction == ParsingFunction.ElementContent ||
                                      parsingFunction == ParsingFunction.DocumentContent);
                        continue;
                    case ParsingFunction.PopEmptyElementContext:
                        curNode = nodes[index];
                        Debug.Assert(curNode.type == XmlNodeType.Element);
                        curNode.IsEmptyElement = false;
                        ResetAttributes();
                        PopElementContext();
                        parsingFunction = nextParsingFunction;
                        continue;
                    case ParsingFunction.InReadAttributeValue:
                        FinishAttributeValueIterator();
                        curNode = nodes[index];
                        continue;
                    case ParsingFunction.InIncrementalRead:
                        FinishIncrementalRead();
                        return true;
                    case ParsingFunction.FragmentAttribute:
                        return ParseFragmentAttribute();
                    case ParsingFunction.XmlDeclarationFragment:
                        ParseXmlDeclarationFragment();
                        parsingFunction = ParsingFunction.GoToEof;
                        return true;
                    case ParsingFunction.GoToEof:
                        OnEof();
                        return false;
                    case ParsingFunction.Error:
                    case ParsingFunction.Eof:
                    case ParsingFunction.ReaderClosed:
                        return false;
                    case ParsingFunction.NoData:
                        ThrowWithoutLineInfo(Res.Xml_MissingRoot);
                        return false;
                    case ParsingFunction.PartialTextValue:
                        SkipPartialTextValue();
                        continue;
                    case ParsingFunction.InReadValueChunk:
                        FinishReadValueChunk();
                        continue;
                    default:
                        Debug.Assert(false);
                        break;
                }
            }
        }

        // Closes the input stream ot TextReader, changes the ReadState to Closed and sets all properties to zero/string.Empty
        public override void Close()
        {
            Close(closeInput);
        }

        // Skips the current node. If on element, skips to the end tag of the element.
        public override void Skip()
        {
            if (readState != ReadState.Interactive)
                return;

            switch (parsingFunction)
            {
                case ParsingFunction.InReadAttributeValue:
                    FinishAttributeValueIterator();
                    curNode = nodes[index];
                    break;
                case ParsingFunction.InIncrementalRead:
                    FinishIncrementalRead();
                    break;
                case ParsingFunction.PartialTextValue:
                    SkipPartialTextValue();
                    break;
                case ParsingFunction.InReadValueChunk:
                    FinishReadValueChunk();
                    break;
            }

            switch (curNode.type)
            {
                // skip subtree
                case XmlNodeType.Element:
                    if (curNode.IsEmptyElement)
                    {
                        break;
                    }

                    int initialDepth = index;
                    parsingMode = ParsingMode.SkipContent;
                    // skip content
                    while (Read() && index > initialDepth) ;
                    Debug.Assert(curNode.type == XmlNodeType.EndElement);
                    Debug.Assert(parsingFunction != ParsingFunction.Eof);
                    parsingMode = ParsingMode.Full;
                    break;
                case XmlNodeType.Attribute:
                    MoveToElement();
                    goto case XmlNodeType.Element;
            }

            // move to following sibling node
            Read();
            return;
        }

        // WsdModifications...
        // Returns NamespaceURI associated with the specified prefix in the current namespace scope.
        public override String LookupNamespace(String prefix)
        {
            // WsdModification - Added support for namespaces
            XmlContext tempContext = xmlContext;
            XmlNamespace ns = tempContext.xmlNamespaces[prefix];
            while (ns == null && tempContext.previousContext != null)
            {
                tempContext = tempContext.previousContext;
                if ((ns = tempContext.xmlNamespaces[prefix]) != null)
                    break;
            }

            if (ns != null)
                return ns.NamespaceURI;

            return "";
        }

        // Iterates through the current attribute value's text and entity references chunks.
        public override bool ReadAttributeValue()
        {
            if (parsingFunction != ParsingFunction.InReadAttributeValue)
            {
                if (curNode.type != XmlNodeType.Attribute)
                {
                    return false;
                }

                if (readState != ReadState.Interactive || curAttrIndex < 0)
                {
                    return false;
                }

                if (parsingFunction == ParsingFunction.InReadValueChunk)
                {
                    FinishReadValueChunk();
                }

                if (curNode.nextAttrValueChunk == null || entityHandling == EntityHandling.ExpandEntities)
                {
                    NodeData simpleValueNode = AddNode(index + attrCount + 1, curNode.depth + 1);
                    simpleValueNode.SetValueNode(XmlNodeType.Text, curNode.StringValue);
                    simpleValueNode.lineInfo = curNode.lineInfo2;
                    simpleValueNode.depth = curNode.depth + 1;
                    curNode = simpleValueNode;
                    Debug.Assert(curNode.nextAttrValueChunk == null);
                }
                else
                {
                    curNode = curNode.nextAttrValueChunk;

                    // This will initialize the (index + attrCount + 1) place in nodes array
                    AddNode(index + attrCount + 1, index + 2);
                    nodes[index + attrCount + 1] = curNode;

                    fullAttrCleanup = true;
                }

                nextParsingFunction = parsingFunction;
                parsingFunction = ParsingFunction.InReadAttributeValue;
                return true;
            }
            else
            {
                if (curNode.nextAttrValueChunk != null)
                {
                    curNode = curNode.nextAttrValueChunk;
                    nodes[index + attrCount + 1] = curNode;
                    return true;
                }

                return false;
            }
        }

        // Resolves the current entity reference node
        public override void ResolveEntity()
        {
            throw new NotSupportedException();
        }

        public override bool CanReadBinaryContent
        {
            get
            {
                return false;
            }
        }

        // Returns true if ReadValue is supported
        public override bool CanReadValueChunk
        {
            get
            {
                return true;
            }
        }

        // Iterates over Value property and copies it into the provided buffer
        public override int ReadValueChunk(char[] buffer, int index, int count)
        {
            // throw on elements
            if (!XmlReader.HasValueInternal(curNode.type))
            {
                throw new InvalidOperationException(Res.GetString(Res.Xml_InvalidReadValueChunk, curNode.type));
            }

            // check arguments
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (buffer.Length - index < count)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            // first call of ReadValueChunk -> initialize incremental read state
            if (parsingFunction != ParsingFunction.InReadValueChunk)
            {
                if (readState != ReadState.Interactive)
                {
                    return 0;
                }

                if (parsingFunction == ParsingFunction.PartialTextValue)
                {
                    incReadState = IncrementalReadState.ReadValueChunk_OnPartialValue;
                }
                else
                {
                    incReadState = IncrementalReadState.ReadValueChunk_OnCachedValue;
                    nextNextParsingFunction = nextParsingFunction;
                    nextParsingFunction = parsingFunction;
                }

                parsingFunction = ParsingFunction.InReadValueChunk;
                readValueOffset = 0;
            }

            // read what is already cached in curNode
            int readCount = 0;
            int read = curNode.CopyTo(readValueOffset, buffer, index + readCount, count - readCount);
            readCount += read;
            readValueOffset += read;

            if (readCount == count)
            {
                return readCount;
            }

            // if on partial value, read the rest of it
            if (incReadState == IncrementalReadState.ReadValueChunk_OnPartialValue)
            {
                curNode.SetValue("");

                // read next chunk of text
                bool endOfValue = false;
                int startPos = 0;
                int endPos = 0;
                while (readCount < count && !endOfValue)
                {
                    int orChars = 0;
                    endOfValue = ParseText(out startPos, out endPos, ref orChars);

                    int copyCount = count - readCount;
                    if (copyCount > endPos - startPos)
                    {
                        copyCount = endPos - startPos;
                    }

                    Array.Copy(ps.chars, startPos, buffer, (index + readCount), copyCount);

                    readCount += copyCount;
                    startPos += copyCount;
                }

                incReadState = endOfValue ? IncrementalReadState.ReadValueChunk_OnCachedValue : IncrementalReadState.ReadValueChunk_OnPartialValue;
                readValueOffset = 0;

                curNode.SetValue(ps.chars, startPos, endPos - startPos);
            }

            return readCount;
        }

        //
        // IXmlLineInfo members
        //
        public bool HasLineInfo()
        {
            return true;
        }

        // Returns the line number of the current node
        public int LineNumber
        {
            get
            {
                return curNode.LineNo;
            }
        }

        // Returns the line position of the current node
        public int LinePosition
        {
            get
            {
                return curNode.LinePos;
            }
        }

        //
        // XmlTextReader members
        //
        // Disables or enables support of W3C XML 1.0 Namespaces
        internal bool Namespaces
        {
            get
            {
                return false;
            }

            set
            {
                throw new InvalidOperationException(Res.GetString(Res.Xml_InvalidOperation));
            }
        }

        // Enables or disables XML 1.0 normalization (incl. end-of-line normalization and normalization of attributes)
        internal bool Normalization
        {
            get
            {
                return normalize;
            }

            set
            {
                if (readState == ReadState.Closed)
                {
                    throw new InvalidOperationException(Res.GetString(Res.Xml_InvalidOperation));
                }

                normalize = value;
                ps.eolNormalized = !value;
            }
        }

        // Returns the Encoding of the XML document
        internal Encoding Encoding
        {
            get
            {
                return (readState == ReadState.Interactive) ? reportedEncoding : null;
            }
        }

        // Spefifies whitespace handling of the XML document, i.e. whether return all namespaces, only significant ones or none
        internal WhitespaceHandling WhitespaceHandling
        {
            get
            {
                return whitespaceHandling;
            }

            set
            {
                if (readState == ReadState.Closed)
                {
                    throw new InvalidOperationException(Res.GetString(Res.Xml_InvalidOperation));
                }

                if ((uint)value > (uint)WhitespaceHandling.None)
                {
                    throw new XmlException(Res.Xml_WhitespaceHandling, "");
                }

                whitespaceHandling = value;
            }
        }

        // Spefifies whether general entities should be automatically expanded or not
        internal EntityHandling EntityHandling
        {
            get
            {
                return entityHandling;
            }

            set
            {
                if (value != EntityHandling.ExpandEntities && value != EntityHandling.ExpandCharEntities)
                {
                    throw new XmlException(Res.Xml_EntityHandling, "");
                }

                entityHandling = value;
            }
        }

        // Reads the contents of an element including markup into a character buffer. Wellformedness checks are limited.
        // This method is designed to read large streams of embedded text by calling it successively.
        internal int ReadChars(char[] buffer, int index, int count)
        {

            if (parsingFunction == ParsingFunction.InIncrementalRead)
            {
                if (incReadDecoder != readCharsDecoder)
                { // mixing ReadChars with ReadBase64 or ReadBinHex
                    if (readCharsDecoder == null)
                    {
                        readCharsDecoder = new IncrementalReadCharsDecoder();
                    }

                    readCharsDecoder.Reset();
                    incReadDecoder = readCharsDecoder;
                }

                return IncrementalRead(buffer, index, count);
            }
            else
            {
                if (curNode.type != XmlNodeType.Element)
                {
                    return 0;
                }

                if (curNode.IsEmptyElement)
                {
                    Read();
                    return 0;
                }

                if (readCharsDecoder == null)
                {
                    readCharsDecoder = new IncrementalReadCharsDecoder();
                }

                InitIncrementalRead(readCharsDecoder);
                return IncrementalRead(buffer, index, count);
            }
        }

        //
        // Throw methods: Sets the reader current position to pos, sets the error state and throws exception
        //
        void Throw(int pos, int res, string arg)
        {
            ps.charPos = pos;
            Throw(res, arg);
        }

        void Throw(int pos, int res, string[] args)
        {
            ps.charPos = pos;
            Throw(res, args);
        }

        void Throw(int pos, int res)
        {
            ps.charPos = pos;
            Throw(res, "");
        }

        void Throw(int res)
        {
            Throw(res, "");
        }

        void Throw(int res, int lineNo, int linePos)
        {
            Throw(new XmlException(res, "", lineNo, linePos, ps.baseUriStr));
        }

        void Throw(int res, string arg)
        {
            Throw(new XmlException(res, arg, ps.LineNo, ps.LinePos, ps.baseUriStr));
        }

        void Throw(int res, string arg, int lineNo, int linePos)
        {
            Throw(new XmlException(res, arg, lineNo, linePos, ps.baseUriStr));
        }

        void Throw(int res, string[] args)
        {
            Throw(new XmlException(res, args, ps.LineNo, ps.LinePos, ps.baseUriStr));
        }

        void Throw(Exception e)
        {
            SetErrorState();
            XmlException xmlEx = e as XmlException;
            if (xmlEx != null)
            {
                curNode.SetLineInfo(xmlEx.LineNumber, xmlEx.LinePosition);
            }

            throw e;
        }

        void ReThrow(Exception e, int lineNo, int linePos)
        {
            Throw(new XmlException(e.Message, (Exception)null, lineNo, linePos, ps.baseUriStr));
        }

        void ThrowWithoutLineInfo(int res)
        {
            Throw(new XmlException(res, "", ps.baseUriStr));
        }

        void ThrowWithoutLineInfo(int res, string arg)
        {
            Throw(new XmlException(res, arg, ps.baseUriStr));
        }

        void ThrowWithoutLineInfo(int res, string[] args)
        {
            Throw(new XmlException(res, args, ps.baseUriStr));
        }

        void ThrowInvalidChar(int pos, char invChar)
        {
            Throw(pos, Res.Xml_InvalidCharacter, XmlException.BuildCharExceptionStr(invChar));
        }

        private void SetErrorState()
        {
            parsingFunction = ParsingFunction.Error;
            readState = ReadState.Error;
        }

        //
        // Private implementation methods & properties
        //
        private bool InAttributeValueIterator
        {
            get
            {
                return attrCount > 0 && parsingFunction >= ParsingFunction.InReadAttributeValue;
            }
        }

        private void FinishAttributeValueIterator()
        {
            Debug.Assert(InAttributeValueIterator);
            if (parsingFunction == ParsingFunction.InReadValueChunk)
            {
                FinishReadValueChunk();
            }

            if (parsingFunction == ParsingFunction.InReadAttributeValue)
            {
                parsingFunction = nextParsingFunction;
                nextParsingFunction = (index > 0) ? ParsingFunction.ElementContent : ParsingFunction.DocumentContent;
            }
        }

        private bool DtdValidation
        {
            get { return false; }
        }

        private void InitStreamInput(Stream stream)
        {
            InitStreamInput("", stream, null, 0);
        }

        private void InitStreamInput(string baseUriStr, Stream stream)
        {
            Debug.Assert(baseUriStr != null);
            InitStreamInput(baseUriStr, stream, null, 0);
        }

        private void InitStreamInput(string baseUriStr, Stream stream, byte[] bytes, int byteCount)
        {

            Debug.Assert(ps.charPos == 0 && ps.charsUsed == 0 && ps.textReader == null);
            ps.stream = stream;
            ps.baseUriStr = baseUriStr;

            // take over the byte buffer allocated in XmlReader.Create, if available
            int bufferSize;
            if (bytes != null)
            {
                ps.bytes = bytes;
                ps.bytesUsed = byteCount;
                bufferSize = ps.bytes.Length;
            }
            else
            {
                // allocate the byte buffer
                bufferSize = XmlReader.CalcBufferSize(stream);
                if (ps.bytes == null || ps.bytes.Length < bufferSize)
                {
                    ps.bytes = new byte[bufferSize];
                }
            }

            // allocate char buffer
            if (ps.chars == null || ps.chars.Length < bufferSize + 1)
            {
                ps.chars = new char[bufferSize + 1];
            }

            // make sure we have at least 4 bytes to detect the encoding (no preamble of System.Text supported encoding is longer than 4 bytes)
            ps.bytePos = 0;
            while (ps.bytesUsed < 4 && ps.bytes.Length - ps.bytesUsed > 0)
            {
                int read = stream.Read(ps.bytes, ps.bytesUsed, ps.bytes.Length - ps.bytesUsed);
                if (read == 0)
                {
                    ps.isStreamEof = true;
                    break;
                }

                ps.bytesUsed += read;
            }

            // detect & setup encoding
            ValidateEncoding();

            Encoding encoding = new UTF8Encoding();

            ps.encoding = encoding;
            ps.decoder = encoding.GetDecoder();

            // eat preamble
            byte[] preamble = { 0xEF, 0xBB, 0xBF }; // UTF8 preamble
            const int preambleLen = 3;
            int i;
            for (i = 0; i < 3 && i < ps.bytesUsed; i++)
            {
                if (ps.bytes[i] != preamble[i])
                {
                    break;
                }
            }

            if (i == preambleLen)
            {
                ps.bytePos = preambleLen;
            }

            documentStartBytePos = ps.bytePos;

            ps.eolNormalized = !normalize;

            // decode first characters
            ps.appendMode = true;
            ReadData();
        }

        private void InitStringInput(string baseUriStr, Encoding originalEncoding, string str)
        {
            Debug.Assert(ps.stream == null && ps.textReader == null);
            Debug.Assert(ps.charPos == 0 && ps.charsUsed == 0);
            Debug.Assert(baseUriStr != null);

            ps.baseUriStr = baseUriStr;

            char[] chars = str.ToCharArray();

            int len = chars.Length;
            ps.chars = new char[len + 1];

            Array.Copy(chars, ps.chars, len);

            ps.charsUsed = len;
            ps.chars[len] = (char)0;

            ps.encoding = originalEncoding;

            ps.eolNormalized = !normalize;
            ps.isEof = true;
        }

        // Stream input only: detect encoding from the first 4 bytes of the byte buffer starting at ps.bytes[ps.bytePos]
        // Validate that the input stream is of encoding that we support (UTF-8)
        private void ValidateEncoding()
        {
            Debug.Assert(ps.bytes != null);
            Debug.Assert(ps.bytePos == 0);

            if (ps.bytesUsed < 2)
            {
                return;
            }

            int first2Bytes = ps.bytes[0] << 8 | ps.bytes[1];
            int next2Bytes = (ps.bytesUsed >= 4) ? (ps.bytes[2] << 8 | ps.bytes[3]) : 0;

            switch (first2Bytes)
            {
                case 0x0000:
                    switch (next2Bytes)
                    {
                        case 0xFEFF:
                            Throw(Res.Xml_UnknownEncoding, "Ucs4Encoding.UCS4_Bigendian");
                            break;
                        case 0x003C:
                            Throw(Res.Xml_UnknownEncoding, "Ucs4Encoding.UCS4_Bigendian");
                            break;
                        case 0xFFFE:
                            Throw(Res.Xml_UnknownEncoding, "Ucs4Encoding.UCS4_2143");
                            break;
                        case 0x3C00:
                            Throw(Res.Xml_UnknownEncoding, "Ucs4Encoding.UCS4_2143");
                            break;
                    }
                    break;
                case 0xFEFF:
                    if (next2Bytes == 0x0000)
                    {
                        Throw(Res.Xml_UnknownEncoding, "Ucs4Encoding.UCS4_3412");
                        break;
                    }
                    else
                    {
                        Throw(Res.Xml_UnknownEncoding, "BigEndianUnicode");
                        break;
                    }
                case 0xFFFE:
                    if (next2Bytes == 0x0000)
                    {
                        Throw(Res.Xml_UnknownEncoding, "Ucs4Encoding.UCS4_Littleendian");
                        break;
                    }
                    else
                    {
                        Throw(Res.Xml_UnknownEncoding, "Unicode");
                        break;
                    }
                case 0x3C00:
                    switch (next2Bytes)
                    {
                        case 0x0000:
                            Throw(Res.Xml_UnknownEncoding, "Ucs4Encoding.UCS4_Littleendian");
                            break;
                        case 0x3F00:
                            Throw(Res.Xml_UnknownEncoding, "Unicode");
                            break;
                    }
                    break;
                case 0x003C:
                    switch (next2Bytes)
                    {
                        case 0x0000:
                            Throw(Res.Xml_UnknownEncoding, "Ucs4Encoding.UCS4_3412");
                            break;
                        case 0x003F:
                            Throw(Res.Xml_UnknownEncoding, "BigEndianUnicode");
                            break;
                    }
                    break;
                case 0x4C6F:
                    if (next2Bytes == 0xA794)
                    {
                        Throw(Res.Xml_UnknownEncoding, "ebcdic");
                    }
                    break;
                case 0xEFBB:
                    if ((next2Bytes & 0xFF00) == 0xBF00)
                    {
                        return;
                    }
                    break;
            }
        }

        private void SetupEncoding(Encoding encoding)
        {

        }

        // Reads more data to the character buffer, discarding already parsed chars / decoded bytes.
        int ReadData()
        {
            // Append Mode:  Append new bytes and characters to the buffers, do not rewrite them. Allocate new buffers
            //               if the current ones are full
            // Rewrite Mode: Reuse the buffers. If there is less than half of the char buffer left for new data, move
            //               the characters that has not been parsed yet to the front of the buffer. Same for bytes.

            if (ps.isEof)
            {
                return 0;
            }

            int charsRead;
            if (ps.appendMode)
            {
                // the character buffer is full -> allocate a new one
                if (ps.charsUsed == ps.chars.Length - 1)
                {
                    // invalidate node values kept in buffer - applies to attribute values only
                    for (int i = 0; i < attrCount; i++)
                    {
                        nodes[index + i + 1].OnBufferInvalidated();
                    }

                    char[] newChars = new char[ps.chars.Length * 2];
                    Array.Copy(ps.chars, 0, newChars, 0, ps.chars.Length);
                    ps.chars = newChars;
                }

                if (ps.stream != null)
                {
                    // the byte buffer is full -> allocate a new one
                    if (ps.bytesUsed - ps.bytePos < MaxByteSequenceLen)
                    {
                        if (ps.bytes.Length - ps.bytesUsed < MaxByteSequenceLen)
                        {
                            byte[] newBytes = new byte[ps.bytes.Length * 2];
                            Array.Copy(ps.bytes, 0, newBytes, 0, ps.bytesUsed);
                            ps.bytes = newBytes;
                        }
                    }
                }

                charsRead = ps.chars.Length - ps.charsUsed - 1;
                if (charsRead > ApproxXmlDeclLength)
                {
                    charsRead = ApproxXmlDeclLength;
                }
            }
            else
            {
                int charsLen = ps.chars.Length;
                if (charsLen - ps.charsUsed <= charsLen / 2)
                {
                    // invalidate node values kept in buffer - applies to attribute values only
                    for (int i = 0; i < attrCount; i++)
                    {
                        nodes[index + i + 1].OnBufferInvalidated();
                    }

                    // move unparsed characters to front, unless the whole buffer contains unparsed characters
                    int copyCharsCount = ps.charsUsed - ps.charPos;
                    if (copyCharsCount < charsLen - 1)
                    {
                        ps.lineStartPos = ps.lineStartPos - ps.charPos;
                        if (copyCharsCount > 0)
                        {
                            Array.Copy(ps.chars, ps.charPos, ps.chars, 0, copyCharsCount);
                        }

                        ps.charPos = 0;
                        ps.charsUsed = copyCharsCount;
                    }
                    else
                    {
                        char[] newChars = new char[ps.chars.Length * 2];
                        Array.Copy(ps.chars, 0, newChars, 0, ps.chars.Length);
                        ps.chars = newChars;
                    }
                }

                if (ps.stream != null)
                {
                    // move undecoded bytes to the front to make some space in the byte buffer
                    int bytesLeft = ps.bytesUsed - ps.bytePos;
                    if (bytesLeft <= MaxBytesToMove)
                    {
                        if (bytesLeft == 0)
                        {
                            ps.bytesUsed = 0;
                        }
                        else
                        {
                            Array.Copy(ps.bytes, ps.bytePos, ps.bytes, 0, bytesLeft);
                            ps.bytesUsed = bytesLeft;
                        }

                        ps.bytePos = 0;
                    }
                }

                charsRead = ps.chars.Length - ps.charsUsed - 1;
            }

            if (ps.stream != null)
            {
                if (!ps.isStreamEof)
                {
                    // read new bytes
                    if (ps.bytes.Length - ps.bytesUsed > 0)
                    {
                        int read = ps.stream.Read(ps.bytes, ps.bytesUsed, ps.bytes.Length - ps.bytesUsed);
                        if (read == 0)
                        {
                            ps.isStreamEof = true;
                        }

                        ps.bytesUsed += read;
                    }
                }

                int originalBytePos = ps.bytePos;

                // decode chars
                charsRead = GetChars(charsRead);
                if (charsRead == 0 && ps.bytePos != originalBytePos)
                {
                    // GetChars consumed some bytes but it was not enough bytes to form a character -> try again
                    return ReadData();
                }
            }
            else if (ps.textReader != null)
            {
                // read chars
                charsRead = ps.textReader.Read(ps.chars, ps.charsUsed, ps.chars.Length - ps.charsUsed - 1);
                ps.charsUsed += charsRead;
            }
            else
            {
                charsRead = 0;
            }

            if (charsRead == 0)
            {
                Debug.Assert(ps.charsUsed < ps.chars.Length);
                ps.isEof = true;
            }

            ps.chars[ps.charsUsed] = (char)0;
            return charsRead;
        }

        // Stream input only: read bytes from stream and decodes them according to the current encoding
        int GetChars(int maxCharsCount)
        {
            Debug.Assert(ps.stream != null && ps.decoder != null && ps.bytes != null);
            Debug.Assert(maxCharsCount <= ps.chars.Length - ps.charsUsed - 1);

            // determine the maximum number of bytes we can pass to the decoder
            int bytesCount = ps.bytesUsed - ps.bytePos;
            if (bytesCount == 0)
            {
                return 0;
            }

            int charsCount;
            bool completed;
            try
            {
                // decode chars
                ps.decoder.Convert(ps.bytes, ps.bytePos, bytesCount, ps.chars, ps.charsUsed, maxCharsCount, false,
                                    out bytesCount, out charsCount, out completed);
            }
            catch (ArgumentException)
            {
                InvalidCharRecovery(ref bytesCount, out charsCount);
            }

            // move pointers and return
            ps.bytePos += bytesCount;
            ps.charsUsed += charsCount;
            Debug.Assert(maxCharsCount >= charsCount);
            return charsCount;
        }

        private void InvalidCharRecovery(ref int bytesCount, out int charsCount)
        {
            int charsDecoded = 0;
            int bytesDecoded = 0;
            try
            {
                while (bytesDecoded < bytesCount)
                {
                    int chDec;
                    int bDec;
                    bool completed;
                    ps.decoder.Convert(ps.bytes, ps.bytePos + bytesDecoded, 1, ps.chars, ps.charsUsed + charsDecoded, 1, false, out bDec, out chDec, out completed);
                    charsDecoded += chDec;
                    bytesDecoded += bDec;
                }

                Debug.Assert(false, "We should get an exception again.");
            }
            catch (ArgumentException)
            {
            }

            if (charsDecoded == 0)
            {
                Throw(ps.charsUsed, Res.Xml_InvalidCharInThisEncoding);
            }

            charsCount = charsDecoded;
            bytesCount = bytesDecoded;
        }

        internal void Close(bool closeInput)
        {
            if (parsingFunction == ParsingFunction.ReaderClosed)
            {
                return;
            }

            ps.Close(closeInput);

            curNode = NodeData.None;
            parsingFunction = ParsingFunction.ReaderClosed;
            reportedEncoding = null;
            reportedBaseUri = "";
            readState = ReadState.Closed;
            fullAttrCleanup = false;
            ResetAttributes();
        }

        void ShiftBuffer(int sourcePos, int destPos, int count)
        {
            Array.Copy(ps.chars, sourcePos, ps.chars, destPos, count);
        }

        // Compares the given character interval and string and returns true if the characters are identical
        bool StrEqual(char[] chars, int strPos1, int strLen1, string str2)
        {
            if (strLen1 != str2.Length)
            {
                return false;
            }

            int i = 0;
            while (i < strLen1 && chars[strPos1 + i] == str2[i])
            {
                i++;
            }

            return i == strLen1;
        }

        // Parses the xml or text declaration and switched encoding if needed
        private bool ParseXmlDeclaration()
        {
            while (ps.charsUsed - ps.charPos < 6)
            {  // minimum "<?xml "
                if (ReadData() == 0)
                {
                    goto NoXmlDecl;
                }
            }

            if (!StrEqual(ps.chars, ps.charPos, 5, XmlDeclarationBegining) ||
                 xmlCharType.IsNameChar(ps.chars[ps.charPos + 5]))
            {
                goto NoXmlDecl;
            }

            curNode.SetLineInfo(ps.LineNo, ps.LinePos + 2);
            curNode.SetNamedNode(XmlNodeType.XmlDeclaration, Xml);

            ps.charPos += 5;

            // parsing of text declarations cannot change global stringBuidler or curNode as we may be in the middle of a text node
            Debug.Assert(stringBuilder.Length == 0);
            BufferBuilder sb = stringBuilder;

            // parse version, encoding & standalone attributes
            int xmlDeclState = 0;   // <?xml (0) version='1.0' (1) encoding='__' (2) standalone='__' (3) ?>

            for (; ; )
            {
                int originalSbLen = sb.Length;
                int wsCount = EatWhitespaces(xmlDeclState == 0 ? null : sb);

                // end of xml declaration
                if (ps.chars[ps.charPos] == '?')
                {
                    sb.Length = originalSbLen;

                    if (ps.chars[ps.charPos + 1] == '>')
                    {
                        if (xmlDeclState == 0)
                        {
                            Throw(Res.Xml_InvalidXmlDecl);
                        }

                        ps.charPos += 2;

                        curNode.SetValue(sb.ToString());
                        sb.Length = 0;

                        nextParsingFunction = parsingFunction;
                        parsingFunction = ParsingFunction.ResetAttributesRootLevel;

                        ps.appendMode = false;
                        return true;
                    }
                    else if (ps.charPos + 1 == ps.charsUsed)
                    {
                        goto ReadData;
                    }
                    else
                    {
                        ThrowUnexpectedToken("'>'");
                    }
                }

                if (wsCount == 0 && xmlDeclState != 0)
                {
                    ThrowUnexpectedToken("?>");
                }

                // read attribute name
                int nameEndPos = ParseName();

                NodeData attr = null;
                switch (ps.chars[ps.charPos])
                {
                    case 'v':
                        if (StrEqual(ps.chars, ps.charPos, nameEndPos - ps.charPos, "version") && xmlDeclState == 0)
                        {
                            attr = AddAttributeNoChecks("version", 0);
                            break;
                        }

                        goto default;
                    case 'e':
                        if (StrEqual(ps.chars, ps.charPos, nameEndPos - ps.charPos, "encoding") &&
                            (xmlDeclState == 1))
                        {
                            attr = AddAttributeNoChecks("encoding", 0);
                            xmlDeclState = 1;
                            break;
                        }

                        goto default;
                    case 's':
                        if (StrEqual(ps.chars, ps.charPos, nameEndPos - ps.charPos, "standalone") &&
                             (xmlDeclState == 1 || xmlDeclState == 2))
                        {
                            attr = AddAttributeNoChecks("standalone", 0);
                            xmlDeclState = 2;
                            break;
                        }

                        goto default;
                    default:
                        Throw(Res.Xml_InvalidXmlDecl);
                        break;
                }

                attr.SetLineInfo(ps.LineNo, ps.LinePos);

                sb.Append(ps.chars, ps.charPos, nameEndPos - ps.charPos);
                ps.charPos = nameEndPos;

                // parse equals and quote char;
                if (ps.chars[ps.charPos] != '=')
                {
                    EatWhitespaces(sb);
                    if (ps.chars[ps.charPos] != '=')
                    {
                        ThrowUnexpectedToken("=");
                    }
                }

                sb.Append('=');
                ps.charPos++;

                char quoteChar = ps.chars[ps.charPos];
                if (quoteChar != '"' && quoteChar != '\'')
                {
                    EatWhitespaces(sb);
                    quoteChar = ps.chars[ps.charPos];
                    if (quoteChar != '"' && quoteChar != '\'')
                    {
                        ThrowUnexpectedToken("\"", "'");
                    }
                }

                sb.Append(quoteChar);
                ps.charPos++;

                attr.quoteChar = quoteChar;
                attr.SetLineInfo2(ps.LineNo, ps.LinePos);

                // parse attribute value
                int pos = ps.charPos;
                char[] chars;
            Continue:
                chars = ps.chars;
                while (chars[pos] > XmlCharType.MaxAsciiChar || (xmlCharType.charProperties[chars[pos]] & XmlCharType.fAttrValue) != 0)
                {
                    pos++;
                }

                if (ps.chars[pos] == quoteChar)
                {
                    switch (xmlDeclState)
                    {
                        // version
                        case 0:
                            if (StrEqual(ps.chars, ps.charPos, pos - ps.charPos, "1.0"))
                            {
                                attr.SetValue(ps.chars, ps.charPos, pos - ps.charPos);
                                xmlDeclState = 1;
                            }
                            else
                            {
                                string badVersion = new string(ps.chars, ps.charPos, pos - ps.charPos);
                                Throw(Res.Xml_InvalidVersionNumber, badVersion);
                            }
                            break;
                        case 1:
                            string encName = new string(ps.chars, ps.charPos, pos - ps.charPos);
                            // Check Encoding
                            if (0 != String.Compare(encName.ToLower(), "utf-8"))
                            {
                                Throw(Res.Xml_UnknownEncoding, encName);
                            }

                            attr.SetValue(encName);
                            xmlDeclState = 2;
                            break;
                        case 2:
                            if (StrEqual(ps.chars, ps.charPos, pos - ps.charPos, "yes"))
                            {
                                this.standalone = true;
                            }
                            else if (StrEqual(ps.chars, ps.charPos, pos - ps.charPos, "no"))
                            {
                                this.standalone = false;
                            }
                            else
                            {
                                Throw(Res.Xml_InvalidXmlDecl, ps.LineNo, ps.LinePos - 1);
                            }

                            attr.SetValue(ps.chars, ps.charPos, pos - ps.charPos);
                            xmlDeclState = 3;
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }

                    sb.Append(chars, ps.charPos, pos - ps.charPos);
                    sb.Append(quoteChar);
                    ps.charPos = pos + 1;
                    continue;
                }
                else if (pos == ps.charsUsed)
                {
                    if (ReadData() != 0)
                    {
                        goto Continue;
                    }
                    else
                    {
                        Throw(Res.Xml_UnclosedQuote);
                    }
                }
                else
                {
                    Throw(Res.Xml_InvalidXmlDecl);
                }

            ReadData:
                if (ps.isEof || ReadData() == 0)
                {
                    Throw(Res.Xml_UnexpectedEOF1);
                }
            }

        NoXmlDecl:
            // no xml declaration
            parsingFunction = nextParsingFunction;
            ps.appendMode = false;
            return false;
        }

        // Parses the document content
        private bool ParseDocumentContent()
        {

            for (; ; )
            {
                bool needMoreChars = false;
                int pos = ps.charPos;
                char[] chars = ps.chars;

                // some tag
                if (chars[pos] == '<')
                {
                    needMoreChars = true;
                    if (ps.charsUsed - pos < 4) // minimum  "<a/>"
                        goto ReadData;
                    pos++;
                    switch (chars[pos])
                    {
                        // processing instruction
                        case '?':
                            ps.charPos = pos + 1;
                            if (ParsePI())
                            {
                                return true;
                            }
                            continue;
                        case '!':
                            pos++;
                            if (ps.charsUsed - pos < 2) // minimum characters expected "--"
                                goto ReadData;
                            // comment
                            if (chars[pos] == '-')
                            {
                                if (chars[pos + 1] == '-')
                                {
                                    ps.charPos = pos + 2;
                                    if (ParseComment())
                                    {
                                        return true;
                                    }
                                    continue;
                                }
                                else
                                {
                                    ThrowUnexpectedToken(pos + 1, "-");
                                }
                            }

                            // CDATA section
                            else if (chars[pos] == '[')
                            {
                                if (fragmentType != XmlNodeType.Document)
                                {
                                    pos++;
                                    if (ps.charsUsed - pos < 6)
                                    {
                                        goto ReadData;
                                    }

                                    if (StrEqual(chars, pos, 6, "CDATA["))
                                    {
                                        ps.charPos = pos + 6;
                                        ParseCData();
                                        if (fragmentType == XmlNodeType.None)
                                        {
                                            fragmentType = XmlNodeType.Element;
                                        }

                                        return true;
                                    }
                                    else
                                    {
                                        ThrowUnexpectedToken(pos, "CDATA[");
                                    }
                                }
                                else
                                {
                                    Throw(ps.charPos, Res.Xml_InvalidRootData);
                                }
                            }

                            // DOCTYPE declaration
                            else
                            {
                                Trace.Print("DTDs not supported");
                                throw new NotSupportedException();
                            }
                            break;
                        case '/':
                            Throw(pos + 1, Res.Xml_UnexpectedEndTag);
                            break;
                        // document element start tag
                        default:
                            if (rootElementParsed)
                            {
                                if (fragmentType == XmlNodeType.Document)
                                {
                                    Throw(pos, Res.Xml_MultipleRoots);
                                }

                                if (fragmentType == XmlNodeType.None)
                                {
                                    fragmentType = XmlNodeType.Element;
                                }
                            }

                            ps.charPos = pos;
                            rootElementParsed = true;
                            ParseElement();
                            return true;
                    }
                }
                else if (chars[pos] == '&')
                {
                    if (fragmentType == XmlNodeType.Document)
                    {
                        Throw(pos, Res.Xml_InvalidRootData);
                    }
                    else
                    {
                        if (fragmentType == XmlNodeType.None)
                        {
                            fragmentType = XmlNodeType.Element;
                        }

                        int i;
                        switch (HandleEntityReference(false, EntityExpandType.OnlyGeneral, out i))
                        {
                            case EntityType.Unexpanded:
                                Debug.Assert(false, "Found general entity reference in xml document");
                                throw new NotSupportedException();
                            case EntityType.CharacterDec:
                            case EntityType.CharacterHex:
                            case EntityType.CharacterNamed:
                                if (ParseText())
                                {
                                    return true;
                                }
                                continue;
                            default:
                                chars = ps.chars;
                                pos = ps.charPos;
                                continue;
                        }
                    }
                }

                // end of buffer
                else if (pos == ps.charsUsed)
                {
                    goto ReadData;
                }

                // something else -> root level whitespaces
                else
                {
                    if (fragmentType == XmlNodeType.Document)
                    {
                        if (ParseRootLevelWhitespace())
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (ParseText())
                        {
                            if (fragmentType == XmlNodeType.None && curNode.type == XmlNodeType.Text)
                            {
                                fragmentType = XmlNodeType.Element;
                            }

                            return true;
                        }
                    }

                    chars = ps.chars;
                    pos = ps.charPos;
                    continue;
                }

                Debug.Assert(pos == ps.charsUsed && !ps.isEof);

            ReadData:
                // read new characters into the buffer
                if (ReadData() != 0)
                {
                    pos = ps.charPos;
                }
                else
                {
                    if (needMoreChars)
                    {
                        Throw(Res.Xml_InvalidRootData);
                    }

                    Debug.Assert(index == 0);

                    if (!rootElementParsed && fragmentType == XmlNodeType.Document)
                    {
                        ThrowWithoutLineInfo(Res.Xml_MissingRoot);
                    }

                    if (fragmentType == XmlNodeType.None)
                    {
                        fragmentType = rootElementParsed ? XmlNodeType.Document : XmlNodeType.Element;
                    }

                    OnEof();
                    return false;
                }

                pos = ps.charPos;
                chars = ps.chars;
            }
        }

        // Parses element content
        private bool ParseElementContent()
        {

            for (; ; )
            {
                int pos = ps.charPos;
                char[] chars = ps.chars;

                switch (chars[pos])
                {
                    // some tag
                    case '<':
                        switch (chars[pos + 1])
                        {
                            // processing instruction
                            case '?':
                                ps.charPos = pos + 2;
                                if (ParsePI())
                                {
                                    return true;
                                }
                                continue;
                            case '!':
                                pos += 2;
                                if (ps.charsUsed - pos < 2)
                                    goto ReadData;
                                // comment
                                if (chars[pos] == '-')
                                {
                                    if (chars[pos + 1] == '-')
                                    {
                                        ps.charPos = pos + 2;
                                        if (ParseComment())
                                        {
                                            return true;
                                        }
                                        continue;
                                    }
                                    else
                                    {
                                        ThrowUnexpectedToken(pos + 1, "-");
                                    }
                                }

                                // CDATA section
                                else if (chars[pos] == '[')
                                {
                                    pos++;
                                    if (ps.charsUsed - pos < 6)
                                    {
                                        goto ReadData;
                                    }

                                    if (StrEqual(chars, pos, 6, "CDATA["))
                                    {
                                        ps.charPos = pos + 6;
                                        ParseCData();
                                        return true;
                                    }
                                    else
                                    {
                                        ThrowUnexpectedToken(pos, "CDATA[");
                                    }
                                }
                                else
                                {

                                    if (ParseUnexpectedToken(pos) == "DOCTYPE")
                                    {
                                        Throw(Res.Xml_BadDTDLocation);
                                    }
                                    else
                                    {
                                        ThrowUnexpectedToken(pos, "<!--", "<[CDATA[");
                                    }
                                }
                                break;
                            // element end tag
                            case '/':
                                ps.charPos = pos + 2;
                                
                                ParseEndElement();

                                return true;
                            default:
                                // end of buffer
                                if (pos + 1 == ps.charsUsed)
                                {
                                    goto ReadData;
                                }
                                else
                                {
                                    // element start tag
                                    ps.charPos = pos + 1;
                                    ParseElement();
                                    return true;
                                }
                        }
                        break;
                    case '&':
                        int i;
                        switch (HandleEntityReference(false, EntityExpandType.OnlyGeneral, out i))
                        {
                            case EntityType.Unexpanded:
                                Debug.Assert(false, "Found general entity in element content");
                                throw new NotSupportedException();
                            case EntityType.CharacterDec:
                            case EntityType.CharacterHex:
                            case EntityType.CharacterNamed:
                                if (ParseText())
                                {
                                    return true;
                                }
                                continue;
                            default:
                                chars = ps.chars;
                                pos = ps.charPos;
                                continue;
                        }
                    default:
                        // end of buffer
                        if (pos == ps.charsUsed)
                        {
                            goto ReadData;
                        }
                        else
                        {
                            // text node, whitespace or entity reference
                            if (ParseText())
                            {
                                return true;
                            }
                            continue;
                        }
                }

            ReadData:
                // read new characters into the buffer
                if (ReadData() == 0)
                {
                    if (ps.charsUsed - ps.charPos != 0)
                    {
                        ThrowUnclosedElements();
                    }

                    if (index == 0 && fragmentType != XmlNodeType.Document)
                    {
                        OnEof();
                        return false;
                    }

                    ThrowUnclosedElements();
                }
            }
        }

        private void ThrowUnclosedElements()
        {
            if (index == 0 && curNode.type != XmlNodeType.Element)
            {
                Throw(ps.charsUsed, Res.Xml_UnexpectedEOF1);
            }
            else
            {
                int i = (parsingFunction == ParsingFunction.InIncrementalRead) ? index : index - 1;
                stringBuilder.Length = 0;
                for (; i >= 0; i--)
                {
                    NodeData el = nodes[i];
                    if (el.type != XmlNodeType.Element)
                    {
                        continue;
                    }

                    stringBuilder.Append(el.GetNameWPrefix(nameTable));
                    if (i > 0)
                    {
                        stringBuilder.Append(", ");
                    }
                    else
                    {
                        stringBuilder.Append(".");
                    }
                }

                Throw(ps.charsUsed, Res.Xml_UnexpectedEOFInElementContent, stringBuilder.ToString());
            }
        }

        // Parses the element start tag
        private void ParseElement()
        {
            int pos = ps.charPos;
            char[] chars = ps.chars;
            int colonPos = -1;

            curNode.SetLineInfo(ps.LineNo, ps.LinePos);

            // PERF: we intentionally don't call ParseQName here to parse the element name unless a special
        // case occurs (like end of buffer, invalid name char)
        ContinueStartName:
            // check element name start char
            if (!(chars[pos] > XmlCharType.MaxAsciiChar || (xmlCharType.charProperties[chars[pos]] & XmlCharType.fNCStartName) != 0))
            {
                goto ParseQNameSlow;
            }

            pos++;

        ContinueName:
            // parse element name
            while (chars[pos] > XmlCharType.MaxAsciiChar || (xmlCharType.charProperties[chars[pos]] & XmlCharType.fNCName) != 0)
            {
                pos++;
            }

            // colon -> save prefix end position and check next char if it's name start char
            if (chars[pos] == ':')
            {
                if (colonPos != -1)
                {
                    pos++;
                    goto ContinueName;
                }
                else
                {
                    colonPos = pos;
                    pos++;
                    goto ContinueStartName;
                }
            }
            else if (pos < ps.charsUsed)
            {
                goto SetElement;
            }

        ParseQNameSlow:
            pos = ParseQName(out colonPos);
            chars = ps.chars;

        SetElement:
            //curNode.SetNamedNode( XmlNodeType.Element, nameTable.Add( chars, ps.charPos, pos - ps.charPos ));

            // WsdModification - Add support for element namespace
            if (colonPos > -1)
            {
                string nameWPrefix = nameTable.Add(chars, ps.charPos, pos - ps.charPos);
                string localName = new string(chars, colonPos + 1, pos - (colonPos + 1));
                string prefix = new string(chars, ps.charPos, colonPos - ps.charPos);
                curNode.SetNamedNode(XmlNodeType.Element, localName, prefix, nameWPrefix);
            }
            else
                curNode.SetNamedNode(XmlNodeType.Element, nameTable.Add(chars, ps.charPos, pos - ps.charPos));

            char ch = chars[pos];
            // white space after element name -> there are probably some attributes
            bool isWs = (ch <= XmlCharType.MaxAsciiChar && (xmlCharType.charProperties[ch] & XmlCharType.fWhitespace) != 0);
            if (isWs)
            {
                ps.charPos = pos;
                ParseAttributes();
                curNode.ns = LookupNamespace(curNode.prefix);

                return;
            }

            // no attributes
            else
            {
                // non-empty element
                if (ch == '>')
                {
                    ps.charPos = pos + 1;

                    // WsdModification - Add namespace lookup to resolve namespace for this node
                    //                    if (curNode.prefix != null && curNode.prefix != "")
                    curNode.ns = LookupNamespace(curNode.prefix);

                    parsingFunction = ParsingFunction.MoveToElementContent;
                }

                // empty element
                else if (ch == '/')
                {
                    if (pos + 1 == ps.charsUsed)
                    {
                        ps.charPos = pos;
                        if (ReadData() == 0)
                        {
                            Throw(pos, Res.Xml_UnexpectedEOF, ">");
                        }

                        pos = ps.charPos;
                        chars = ps.chars;
                    }

                    if (chars[pos + 1] == '>')
                    {
                        // even though there are no attributes, parse them anyway to setup
                        // the current element correctly.  Otherwise an empty element will only 
                        // show up as an EndElement.
                        ps.charPos = pos;
                        ParseAttributes();

                        curNode.ns = LookupNamespace(curNode.prefix);

                        return;
                    }
                    else
                    {
                        ThrowUnexpectedToken(pos, ">");
                    }
                }

                // something else after the element name
                else
                {
                    Throw(pos, Res.Xml_BadNameChar, XmlException.BuildCharExceptionStr(ch));
                }
            }
        }

        // parses the element end tag
        private void ParseEndElement()
        {
            // check if the end tag name equals start tag name
            NodeData startTagNode = nodes[index - 1];

            int prefLen = startTagNode.prefix.Length;
            int locLen = startTagNode.localName.Length;

            while (ps.charsUsed - ps.charPos < prefLen + locLen + 1)
            {
                if (ReadData() == 0)
                {
                    break;
                }
            }

            int nameLen;
            char[] chars = ps.chars;
            if (startTagNode.prefix.Length == 0)
            {
                if (!StrEqual(chars, ps.charPos, locLen, startTagNode.localName))
                {
                    ThrowTagMismatch(startTagNode);
                }

                nameLen = locLen;
            }
            else
            {
                int colonPos = ps.charPos + prefLen;
                if (!StrEqual(chars, ps.charPos, prefLen, startTagNode.prefix) ||
                        chars[colonPos] != ':' ||
                        !StrEqual(chars, colonPos + 1, locLen, startTagNode.localName))
                {
                    ThrowTagMismatch(startTagNode);
                }

                nameLen = locLen + prefLen + 1;
            }

            int pos;
            for (; ; )
            {
                pos = ps.charPos + nameLen;
                chars = ps.chars;

                if (pos == ps.charsUsed)
                {
                    goto ReadData;
                }

                if (chars[pos] > XmlCharType.MaxAsciiChar ||
                     (xmlCharType.charProperties[chars[pos]] & XmlCharType.fNCName) != 0 || chars[pos] == ':')
                {
                    ThrowTagMismatch(startTagNode);
                }

                // eat whitespaces
                while (chars[pos] <= XmlCharType.MaxAsciiChar && (xmlCharType.charProperties[chars[pos]] & XmlCharType.fWhitespace) != 0)
                {
                    pos++;
                }

                if (chars[pos] == '>')
                {
                    break;
                }
                else if (pos == ps.charsUsed)
                {
                    goto ReadData;
                }
                else
                {
                    ThrowUnexpectedToken(pos, ">");
                }

                Debug.Assert(false, "We should never get to this point.");

            ReadData:
                if (ReadData() == 0)
                {
                    ThrowUnclosedElements();
                }
            }

            Debug.Assert(index > 0);
            index--;
            curNode = nodes[index];

            // set the element data
            Debug.Assert(curNode == startTagNode);
            startTagNode.SetLineInfo(ps.LineNo, ps.LinePos);
            startTagNode.type = XmlNodeType.EndElement;

            ps.charPos = pos + 1;

            // set next parsing function
            nextParsingFunction = (index > 0) ? parsingFunction : ParsingFunction.DocumentContent;
            parsingFunction = ParsingFunction.PopElementContext;
        }

        private void ThrowTagMismatch(NodeData startTag)
        {
            if (startTag.type == XmlNodeType.Element)
            {
                // parse the bad name
                int colonPos;
                int endPos = ParseQName(out colonPos);

                string[] args = new string[3];
                args[0] = startTag.GetNameWPrefix(nameTable);
                ///////ISSUE: rswaney: SPOT string class doesn't have a culture-aware or case-insensitive compare

                args[1] = startTag.lineInfo.lineNo.ToString(/* CultureInfo.InvariantCulture */);
                args[2] = new string(ps.chars, ps.charPos, endPos - ps.charPos);
                Throw(Res.Xml_TagMismatch, args);
            }
            else
            {
                Debug.Assert(startTag.type == XmlNodeType.EntityReference);
                Throw(Res.Xml_UnexpectedEndTag);
            }
        }

        // Reads the attributes
        private void ParseAttributes()
        {
            int pos = ps.charPos;
            char[] chars = ps.chars;
            NodeData attr = null;

            Debug.Assert(attrCount == 0);

            for (; ; )
            {
                // eat whitespaces
                int lineNoDelta = 0;
                char tmpch0;

                while ((tmpch0 = chars[pos]) < XmlCharType.MaxAsciiChar && (xmlCharType.charProperties[tmpch0] & XmlCharType.fWhitespace) != 0)
                {
                    if (tmpch0 == (char)0xA)
                    {
                        OnNewLine(pos + 1);
                        lineNoDelta++;
                    }
                    else if (tmpch0 == (char)0xD)
                    {
                        if (chars[pos + 1] == (char)0xA)
                        {
                            OnNewLine(pos + 2);
                            lineNoDelta++;
                            pos++;
                        }
                        else if (pos + 1 != ps.charsUsed)
                        {
                            OnNewLine(pos + 1);
                            lineNoDelta++;
                        }
                        else
                        {
                            ps.charPos = pos;
                            goto ReadData;
                        }
                    }

                    pos++;
                }

                char tmpch1;
                bool isNCStartName = ((tmpch1 = chars[pos]) > XmlCharType.MaxAsciiChar || (xmlCharType.charProperties[tmpch1] & XmlCharType.fNCStartName) != 0);
                if (!isNCStartName)
                {
                    // element end
                    if (tmpch1 == '>')
                    {
                        Debug.Assert(curNode.type == XmlNodeType.Element);
                        ps.charPos = pos + 1;
                        parsingFunction = ParsingFunction.MoveToElementContent;
                        goto End;
                    }

                    // empty element end
                    else if (tmpch1 == '/')
                    {
                        Debug.Assert(curNode.type == XmlNodeType.Element);
                        if (pos + 1 == ps.charsUsed)
                        {
                            goto ReadData;
                        }

                        if (chars[pos + 1] == '>')
                        {
                            ps.charPos = pos + 2;
                            curNode.IsEmptyElement = true;
                            nextParsingFunction = parsingFunction;
                            parsingFunction = ParsingFunction.PopEmptyElementContext;
                            goto End;
                        }
                        else
                        {
                            ThrowUnexpectedToken(pos + 1, ">");
                        }
                    }
                    else if (pos == ps.charsUsed)
                    {
                        goto ReadData;
                    }
                    else if (tmpch1 != ':')
                    {
                        Throw(pos, Res.Xml_BadStartNameChar, XmlException.BuildCharExceptionStr(tmpch1));
                    }
                }

                if (pos == ps.charPos)
                {
                    Throw(Res.Xml_ExpectingWhiteSpace, ParseUnexpectedToken());
                }

                ps.charPos = pos;

                // save attribute name line position
                int attrNameLinePos = ps.LinePos;

#if DEBUG
                int attrNameLineNo = ps.LineNo;
#endif

                // parse attribute name
                int colonPos = -1;

                // PERF: we intentionally don't call ParseQName here to parse the element name unless a special
                // case occurs (like end of buffer, invalid name char)
                pos++; // start name char has already been checked

                // parse attribute name
            ContinueParseName:
                char tmpch2;

                while ((tmpch2 = chars[pos]) > XmlCharType.MaxAsciiChar || (xmlCharType.charProperties[tmpch2] & XmlCharType.fNCName) != 0)
                {
                    pos++;
                }

                // colon -> save prefix end position and check next char if it's name start char
                if (tmpch2 == ':')
                {
                    if (colonPos != -1)
                    {
                        pos++;
                        goto ContinueParseName;
                    }
                    else
                    {
                        colonPos = pos;
                        pos++;

                        if (chars[pos] > XmlCharType.MaxAsciiChar || (xmlCharType.charProperties[chars[pos]] & XmlCharType.fNCStartName) != 0)
                        {
                            pos++;
                            goto ContinueParseName;
                        }

                        pos = ParseQName(out colonPos);
                        chars = ps.chars;
                    }
                }
                else if (pos == ps.charsUsed)
                {
                    pos = ParseQName(out colonPos);
                    chars = ps.chars;
                }

                attr = AddAttribute(pos, colonPos);
                attr.SetLineInfo(ps.LineNo, attrNameLinePos);

#if DEBUG
                Debug.Assert(attrNameLineNo == ps.LineNo);
#endif

                // parse equals and quote char;
                if (chars[pos] != '=')
                {
                    ps.charPos = pos;
                    EatWhitespaces(null);
                    pos = ps.charPos;
                    if (chars[pos] != '=')
                    {
                        ThrowUnexpectedToken("=");
                    }
                }

                pos++;

                char quoteChar = chars[pos];
                if (quoteChar != '"' && quoteChar != '\'')
                {
                    ps.charPos = pos;
                    EatWhitespaces(null);
                    pos = ps.charPos;
                    quoteChar = chars[pos];
                    if (quoteChar != '"' && quoteChar != '\'')
                    {
                        ThrowUnexpectedToken("\"", "'");
                    }
                }

                pos++;
                ps.charPos = pos;

                attr.quoteChar = quoteChar;
                attr.SetLineInfo2(ps.LineNo, ps.LinePos);

                // parse attribute value
                char tmpch3;

                while ((tmpch3 = chars[pos]) > XmlCharType.MaxAsciiChar || (xmlCharType.charProperties[tmpch3] & XmlCharType.fAttrValue) != 0)
                {
                    pos++;
                }

                if (tmpch3 == quoteChar)
                {
                    attr.SetValue(chars, ps.charPos, pos - ps.charPos);
                    pos++;
                    ps.charPos = pos;
                }
                else
                {
                    ParseAttributeValueSlow(pos, quoteChar, attr);
                    pos = ps.charPos;
                    chars = ps.chars;
                }

                // handle special attributes:
                if (attr.prefix.Length != 0)
                {
                    // WsdModification - Added xmlns handling for namespaces
                    if (Ref.Equal(attr.prefix, Xml) || Ref.Equal(attr.prefix, XmlNs))
                    {
                        OnXmlReservedAttribute(attr);
                    }
                }

                // WsdModification - Added xmlns handling for local namespace attribs
                else if (attr.localName == XmlNs)
                {
                    attr.localName = string.Empty;
                    attr.prefix = string.Empty;
                    OnXmlReservedAttribute(attr);
                }

                continue;

            ReadData:
                ps.lineNo -= lineNoDelta;
                if (ReadData() != 0)
                {
                    pos = ps.charPos;
                    chars = ps.chars;
                }
                else
                {
                    ThrowUnclosedElements();
                }
            }

        End:
            // check duplicate attributes
            if (attrDuplWalkCount >= MaxAttrDuplWalkCount)
            {
                AttributeDuplCheck();
            }
        }

        private void AttributeDuplCheck()
        {

            for (int i = index + 1; i < index + 1 + attrCount; i++)
            {
                NodeData attr1 = nodes[i];
                for (int j = i + 1; j < index + 1 + attrCount; j++)
                {
                    if (Ref.Equal(attr1.localName, nodes[j].localName) && Ref.Equal(attr1.ns, nodes[j].ns))
                    {
                        Throw(Res.Xml_DupAttributeName, nodes[j].GetNameWPrefix(nameTable), nodes[j].LineNo, nodes[j].LinePos);
                    }
                }
            }
        }

        private static char[] WhitespaceChars = new char[] { ' ', '\t', '\n', '\r' };

        private void OnXmlReservedAttribute(NodeData attr)
        {
            switch (attr.localName)
            {
                // xml:space
                case "space":
                    if (!curNode.xmlContextPushed)
                    {
                        PushXmlContext();
                    }

                    switch (attr.StringValue.Trim(WhitespaceChars))
                    {
                        case "preserve":
                            xmlContext.xmlSpace = XmlSpace.Preserve;
                            break;
                        case "default":
                            xmlContext.xmlSpace = XmlSpace.Default;
                            break;
                        default:
                            Throw(Res.Xml_InvalidXmlSpace, attr.StringValue, attr.lineInfo.lineNo, attr.lineInfo.linePos);
                            break;
                    }
                    break;
                // xml:lang
                case "lang":
                    if (!curNode.xmlContextPushed)
                    {
                        PushXmlContext();
                    }

                    xmlContext.xmlLang = attr.StringValue;
                    break;
                // xmlns
                // WsdModification - Adds support for Namespaces
                default:
                    if (!curNode.xmlContextPushed)
                    {
                        PushXmlContext();
                    }

                    // Add this namespace attribute to the context namespaces collection
                    xmlContext.xmlNamespaces.Add(attr.localName, attr.StringValue);
                    break;
            }
        }

        private void ParseAttributeValueSlow(int curPos, char quoteChar, NodeData attr)
        {
            int pos = curPos;
            char[] chars = ps.chars;
            int valueChunkStartPos = 0;
            LineInfo valueChunkLineInfo = new LineInfo(ps.lineNo, ps.LinePos);
            NodeData lastChunk = null;

            Debug.Assert(stringBuilder.Length == 0);

            for (; ; )
            {
                // parse the rest of the attribute value
                while (chars[pos] > XmlCharType.MaxAsciiChar || (xmlCharType.charProperties[chars[pos]] & XmlCharType.fAttrValue) != 0)
                {
                    pos++;
                }

                if (pos - ps.charPos > 0)
                {
                    stringBuilder.Append(chars, ps.charPos, pos - ps.charPos);
                    ps.charPos = pos;
                }

                if (chars[pos] == quoteChar)
                {
                    break;
                }
                else
                {
                    switch (chars[pos])
                    {
                        // eol
                        case (char)0xA:
                            pos++;
                            OnNewLine(pos);
                            if (normalize)
                            {
                                stringBuilder.Append((char)0x20);  // CDATA normalization of 0xA
                                ps.charPos++;
                            }
                            continue;
                        case (char)0xD:
                            if (chars[pos + 1] == (char)0xA)
                            {
                                pos += 2;
                                if (normalize)
                                {
                                    stringBuilder.Append(ps.eolNormalized ? "\u0020\u0020" : "\u0020"); // CDATA normalization of 0xD 0xA
                                    ps.charPos = pos;
                                }
                            }
                            else if (pos + 1 < ps.charsUsed || ps.isEof)
                            {
                                pos++;
                                if (normalize)
                                {
                                    stringBuilder.Append((char)0x20);  // CDATA normalization of 0xD and 0xD 0xA
                                    ps.charPos = pos;
                                }
                            }
                            else
                            {
                                goto ReadData;
                            }

                            OnNewLine(pos);
                            continue;
                        // tab
                        case (char)0x9:
                            pos++;
                            if (normalize)
                            {
                                stringBuilder.Append((char)0x20);  // CDATA normalization of 0x9
                                ps.charPos++;
                            }
                            continue;
                        case '"':
                        case '\'':
                        case '>':
                            pos++;
                            continue;
                        // attribute values cannot contain '<'
                        case '<':
                            Throw(pos, Res.Xml_BadAttributeChar, XmlException.BuildCharExceptionStr('<'));
                            break;
                        // entity referece
                        case '&':
                            if (pos - ps.charPos > 0)
                            {
                                stringBuilder.Append(chars, ps.charPos, pos - ps.charPos);
                            }

                            ps.charPos = pos;

                            LineInfo entityLineInfo = new LineInfo(ps.lineNo, ps.LinePos + 1);
                            switch (HandleEntityReference(true, EntityExpandType.All, out pos))
                            {
                                case EntityType.CharacterDec:
                                case EntityType.CharacterHex:
                                case EntityType.CharacterNamed:
                                    break;
                                case EntityType.Unexpanded:
                                    Debug.Assert(false, "The document contains an entity reference");
                                    throw new NotSupportedException();

                                case EntityType.ExpandedInAttribute:
                                    Debug.Assert(false, "The document contains an entity reference");
                                    throw new NotSupportedException();
                                default:
                                    pos = ps.charPos;
                                    break;
                            }

                            chars = ps.chars;
                            continue;
                        default:
                            // end of buffer
                            if (pos == ps.charsUsed)
                            {
                                goto ReadData;
                            }

                            // surrogate chars
                            else
                            {
                                char ch = chars[pos];
                                if (ch >= SurHighStart && ch <= SurHighEnd)
                                {
                                    if (pos + 1 == ps.charsUsed)
                                    {
                                        goto ReadData;
                                    }

                                    pos++;
                                    if (chars[pos] >= SurLowStart && chars[pos] <= SurLowEnd)
                                    {
                                        pos++;
                                        continue;
                                    }
                                }

                                ThrowInvalidChar(pos, ch);
                                break;
                            }
                    }
                }

            ReadData:
                // read new characters into the buffer
                if (ReadData() == 0)
                {
                    if (ps.charsUsed - ps.charPos > 0)
                    {
                        if (ps.chars[ps.charPos] != (char)0xD)
                        {
                            Debug.Assert(false, "We should never get to this point.");
                            Throw(Res.Xml_UnexpectedEOF1);
                        }

                        Debug.Assert(ps.isEof);
                    }
                    else
                    {
                        if (fragmentType == XmlNodeType.Attribute)
                        {
                            break;
                        }

                        Throw(Res.Xml_UnclosedQuote);
                    }
                }

                pos = ps.charPos;
                chars = ps.chars;
            }

            if (attr.nextAttrValueChunk != null)
            {
                // construct last text value chunk
                int valueChunkLen = stringBuilder.Length - valueChunkStartPos;
                if (valueChunkLen > 0)
                {
                    NodeData textChunk = new NodeData();
                    textChunk.lineInfo = valueChunkLineInfo;
                    textChunk.depth = attr.depth + 1;
                    textChunk.SetValueNode(XmlNodeType.Text, stringBuilder.ToString(valueChunkStartPos, valueChunkLen));
                    AddAttributeChunkToList(attr, textChunk, ref lastChunk);
                }
            }

            ps.charPos = pos + 1;

            attr.SetValue(stringBuilder.ToString());
            stringBuilder.Length = 0;
        }

        private void AddAttributeChunkToList(NodeData attr, NodeData chunk, ref NodeData lastChunk)
        {
            if (lastChunk == null)
            {
                Debug.Assert(attr.nextAttrValueChunk == null);
                lastChunk = chunk;
                attr.nextAttrValueChunk = chunk;
            }
            else
            {
                lastChunk.nextAttrValueChunk = chunk;
                lastChunk = chunk;
            }
        }

        // Parses text or white space node.
        // Returns true if a node has been parsed and its data set to curNode.
        // Returns false when a white space has been parsed and ignored (according to current whitespace handling) or when parsing mode is not Full.
        private bool ParseText()
        {
            int startPos;
            int endPos;
            int orChars = 0;

            // skip over the text if not in full parsing mode
            if (parsingMode != ParsingMode.Full)
            {
                while (!ParseText(out startPos, out endPos, ref orChars)) ;
                return false;
            }

            curNode.SetLineInfo(ps.LineNo, ps.LinePos);
            Debug.Assert(stringBuilder.Length == 0);

            // the whole value is in buffer
            if (ParseText(out startPos, out endPos, ref orChars))
            {
                XmlNodeType nodeType = GetTextNodeType(orChars);
                if (nodeType == XmlNodeType.None)
                {
                    goto IgnoredWhitespace;
                }

                //Debug.Assert(endPos - startPos > 0);
                curNode.SetValueNode(nodeType, ps.chars, startPos, endPos - startPos);
                return true;
            }

            // only piece of the value was returned
            else
            {
                bool fullValue = false;

                // if it's a partial text value, not a whitespace -> return
                if (orChars > 0x20)
                {
                    Debug.Assert(endPos - startPos > 0);
                    curNode.SetValueNode(XmlNodeType.Text, ps.chars, startPos, endPos - startPos);
                    nextParsingFunction = parsingFunction;
                    parsingFunction = ParsingFunction.PartialTextValue;
                    return true;
                }

                // partial whitespace -> read more data (up to 4kB) to decide if it is a whitespace or a text node
                stringBuilder.Append(ps.chars, startPos, endPos - startPos);
                do
                {
                    fullValue = ParseText(out startPos, out endPos, ref orChars);
                    stringBuilder.Append(ps.chars, startPos, endPos - startPos);
                } while (!fullValue && orChars <= 0x20 && stringBuilder.Length < MinWhitespaceLookahedCount);

                // determine the value node type
                XmlNodeType nodeType = (stringBuilder.Length < MinWhitespaceLookahedCount) ? GetTextNodeType(orChars) : XmlNodeType.Text;
                if (nodeType == XmlNodeType.None)
                {
                    // ignored whitespace -> skip over the rest of the value unless we already read it all
                    stringBuilder.Length = 0;
                    if (!fullValue)
                    {
                        while (!ParseText(out startPos, out endPos, ref orChars)) ;
                    }

                    goto IgnoredWhitespace;
                }

                // set value to curNode
                curNode.SetValueNode(nodeType, stringBuilder.ToString());
                stringBuilder.Length = 0;

                // change parsing state if the full value was not parsed
                if (!fullValue)
                {
                    nextParsingFunction = parsingFunction;
                    parsingFunction = ParsingFunction.PartialTextValue;
                }

                return true;
            }

        IgnoredWhitespace:
            return false;
        }

        // Parses a chunk of text starting at ps.charPos.
        //   startPos .... start position of the text chunk that has been parsed (can differ from ps.charPos before the call)
        //   endPos ...... end position of the text chunk that has been parsed (can differ from ps.charPos after the call)
        //   ourOrChars .. all parsed character bigger or equal to 0x20 or-ed (|) into a single int. It can be used for whitespace detection
        //                 (the text has a non-whitespace character if outOrChars > 0x20).
        // Returns true when the whole value has been parsed. Return false when it needs to be called again to get a next chunk of value.
        private bool ParseText(out int startPos, out int endPos, ref int outOrChars)
        {
            char[] chars = ps.chars;
            int pos = ps.charPos;
            int rcount = 0;
            int rpos = -1;
            int orChars = outOrChars;
            char c;

            for (; ; )
            {
                // parse text content
                while ((c = chars[pos]) > XmlCharType.MaxAsciiChar || (xmlCharType.charProperties[c] & XmlCharType.fText) != 0)
                {
                    orChars |= (int)c;
                    pos++;
                }

                switch (c)
                {
                    case (char)0x9:
                        pos++;
                        continue;

                    // eol
                    case (char)0xA:
                    case (char)0xD:

                        // For SideShow we want to ignore linefeed and return chars in text content. The user
                        // must use <br/> elements to insert line breaks in text.
                        if (xmlContext.xmlSpace != XmlSpace.Preserve)
                        {

                            int skipCount = 1;
                            if (chars[pos] == (char)0xD && chars[pos + 1] == (char)0xA)
                            {
                                skipCount = 2;
                            }

                            if (pos - ps.charPos > 0)
                            {
                                if (rcount == 0)
                                {
                                    rcount = skipCount;
                                    rpos = pos;
                                }
                                else
                                {
                                    ShiftBuffer(rpos + rcount, rpos, pos - rpos - rcount);
                                    rpos = pos - rcount;
                                    rcount += skipCount;
                                }
                            }
                            else
                            {
                                ps.charPos += skipCount;
                            }

                            pos += skipCount;
                        }
                        else if (chars[pos] == (char)0xA)
                        {
                            pos++;
                        }
                        else if (chars[pos + 1] == (char)0xA)
                        {

                            if (!ps.eolNormalized && parsingMode == ParsingMode.Full)
                            {
                                if (pos - ps.charPos > 0)
                                {
                                    if (rcount == 0)
                                    {
                                        rcount = 1;
                                        rpos = pos;
                                    }
                                    else
                                    {
                                        ShiftBuffer(rpos + rcount, rpos, pos - rpos - rcount);
                                        rpos = pos - rcount;
                                        rcount++;
                                    }
                                }
                                else
                                {
                                    ps.charPos++;
                                }
                            }

                            pos += 2;
                        }
                        else if (pos + 1 < ps.charsUsed || ps.isEof)
                        {
                            if (!ps.eolNormalized)
                            {
                                chars[pos] = (char)0xA;             // EOL normalization of 0xD
                            }

                            pos++;
                        }
                        else
                        {
                            goto ReadData;
                        }

                        OnNewLine(pos);
                        continue;

                    // some tag
                    case '<':
                        goto ReturnPartialValue;
                    // entity reference
                    case '&':
                        // try to parse char entity inline
                        int charRefEndPos, charCount;
                        EntityType entityType;
                        if ((charRefEndPos = ParseCharRefInline(pos, out charCount, out entityType)) > 0)
                        {
                            if (rcount > 0)
                            {
                                ShiftBuffer(rpos + rcount, rpos, pos - rpos - rcount);
                            }

                            rpos = pos - rcount;
                            rcount += (charRefEndPos - pos - charCount);
                            pos = charRefEndPos;

                            if (!xmlCharType.IsWhiteSpace(chars[charRefEndPos - charCount]))
                            {
                                orChars |= 0xFF;
                            }
                        }
                        else
                        {
                            if (pos > ps.charPos)
                            {
                                goto ReturnPartialValue;
                            }

                            switch (HandleEntityReference(false, EntityExpandType.All, out pos))
                            {
                                case EntityType.Unexpanded:
                                    Debug.Assert(false, "Found general entity refernce in text");
                                    throw new NotSupportedException();

                                case EntityType.CharacterDec:
                                case EntityType.CharacterHex:
                                case EntityType.CharacterNamed:
                                    if (!xmlCharType.IsWhiteSpace(ps.chars[pos - 1]))
                                    {
                                        orChars |= 0xFF;
                                    }
                                    break;
                                default:
                                    pos = ps.charPos;
                                    break;
                            }

                            chars = ps.chars;
                        }
                        continue;
                    case ']':
                        if (ps.charsUsed - pos < 3 && !ps.isEof)
                        {
                            goto ReadData;
                        }

                        if (chars[pos + 1] == ']' && chars[pos + 2] == '>')
                        {
                            Throw(pos, Res.Xml_CDATAEndInText);
                        }

                        orChars |= ']';
                        pos++;
                        continue;
                    default:
                        // end of buffer
                        if (pos == ps.charsUsed)
                        {
                            goto ReadData;
                        }

                        // surrogate chars
                        else
                        {
                            char ch = chars[pos];
                            if (ch >= SurHighStart && ch <= SurHighEnd)
                            {
                                if (pos + 1 == ps.charsUsed)
                                {
                                    goto ReadData;
                                }

                                pos++;
                                if (chars[pos] >= SurLowStart && chars[pos] <= SurLowEnd)
                                {
                                    pos++;
                                    orChars |= ch;
                                    continue;
                                }
                            }

                            int offset = pos - ps.charPos;
                            ThrowInvalidChar(ps.charPos + offset, ch);
                            break;
                        }
                }

            ReadData:
                if (pos > ps.charPos)
                {
                    goto ReturnPartialValue;
                }

                // read new characters into the buffer
                if (ReadData() == 0)
                {
                    if (ps.charsUsed - ps.charPos > 0)
                    {
                        if (ps.chars[ps.charPos] != (char)0xD)
                        {
                            Debug.Assert(false, "We should never get to this point.");
                            Throw(Res.Xml_UnexpectedEOF1);
                        }

                        Debug.Assert(ps.isEof);
                    }
                    else
                    {
                        startPos = endPos = pos;
                        return true;
                    }
                }

                pos = ps.charPos;
                chars = ps.chars;
                continue;
            }

        ReturnPartialValue:
            if (parsingMode == ParsingMode.Full && rcount > 0)
            {
                ShiftBuffer(rpos + rcount, rpos, pos - rpos - rcount);
            }

            startPos = ps.charPos;
            endPos = pos - rcount;
            ps.charPos = pos;
            outOrChars = orChars;
            return c == '<';
        }

        // When in ParsingState.PartialTextValue, this method parses and caches the rest of the value and stores it in curNode.
        void FinishPartialValue()
        {
            Debug.Assert(stringBuilder.Length == 0);
            Debug.Assert(parsingFunction == ParsingFunction.PartialTextValue ||
                          (parsingFunction == ParsingFunction.InReadValueChunk && incReadState == IncrementalReadState.ReadValueChunk_OnPartialValue));

            curNode.CopyTo(readValueOffset, stringBuilder);

            int startPos;
            int endPos;
            int orChars = 0;
            while (!ParseText(out startPos, out endPos, ref orChars))
            {
                stringBuilder.Append(ps.chars, startPos, endPos - startPos);
            }

            stringBuilder.Append(ps.chars, startPos, endPos - startPos);

            Debug.Assert(stringBuilder.Length > 0);
            curNode.SetValue(stringBuilder.ToString());
            stringBuilder.Length = 0;
        }

        void FinishOtherValueIterator()
        {
            switch (parsingFunction)
            {
                case ParsingFunction.InReadAttributeValue:
                    // do nothing, correct value is already in curNode
                    break;
                case ParsingFunction.InReadValueChunk:
                    if (incReadState == IncrementalReadState.ReadValueChunk_OnPartialValue)
                    {
                        FinishPartialValue();
                        incReadState = IncrementalReadState.ReadValueChunk_OnCachedValue;
                    }
                    else
                    {
                        if (readValueOffset > 0)
                        {
                            curNode.SetValue(curNode.StringValue.Substring(readValueOffset));
                            readValueOffset = 0;
                        }
                    }
                    break;
            }
        }

        // When in ParsingState.PartialTextValue, this method skips over the rest of the partial value.
        void SkipPartialTextValue()
        {
            Debug.Assert(parsingFunction == ParsingFunction.PartialTextValue || parsingFunction == ParsingFunction.InReadValueChunk);
            int startPos;
            int endPos;
            int orChars = 0;

            parsingFunction = nextParsingFunction;
            while (!ParseText(out startPos, out endPos, ref orChars)) ;
        }

        void FinishReadValueChunk()
        {
            Debug.Assert(parsingFunction == ParsingFunction.InReadValueChunk);

            readValueOffset = 0;
            if (incReadState == IncrementalReadState.ReadValueChunk_OnPartialValue)
            {
                Debug.Assert((index > 0) ? nextParsingFunction == ParsingFunction.ElementContent : nextParsingFunction == ParsingFunction.DocumentContent);
                SkipPartialTextValue();
            }
            else
            {
                parsingFunction = nextParsingFunction;
                nextParsingFunction = nextNextParsingFunction;
            }
        }

        private bool ParseRootLevelWhitespace()
        {
            Debug.Assert(stringBuilder.Length == 0);

            XmlNodeType nodeType = GetWhitespaceType();

            if (nodeType == XmlNodeType.None)
            {
                EatWhitespaces(null);
                if (ps.chars[ps.charPos] == '<' || ps.charsUsed - ps.charPos == 0)
                {
                    return false;
                }
            }
            else
            {
                curNode.SetLineInfo(ps.LineNo, ps.LinePos);
                EatWhitespaces(stringBuilder);
                if (ps.chars[ps.charPos] == '<' || ps.charsUsed - ps.charPos == 0)
                {
                    if (stringBuilder.Length > 0)
                    {
                        curNode.SetValueNode(nodeType, stringBuilder.ToString());
                        stringBuilder.Length = 0;
                        return true;
                    }

                    return false;
                }
            }

            // Ignore null chars at the root level. This allows a provider to include a terminating null
            // without causing an invalid character exception.
            if (ps.chars[ps.charPos] == '\0')
            {
                ps.charPos++;
            }
            else if (xmlCharType.IsCharData(ps.chars[ps.charPos]))
            {
                Throw(Res.Xml_InvalidRootData);
            }
            else
            {
                ThrowInvalidChar(ps.charPos, ps.chars[ps.charPos]);
            }

            return false;
        }

        private EntityType HandleEntityReference(bool isInAttributeValue, EntityExpandType expandType, out int charRefEndPos)
        {
            Debug.Assert(ps.chars[ps.charPos] == '&');

            if (ps.charPos + 1 == ps.charsUsed)
            {
                if (ReadData() == 0)
                {
                    Throw(Res.Xml_UnexpectedEOF1);
                }
            }

            // numeric characters reference
            if (ps.chars[ps.charPos + 1] == '#')
            {
                EntityType entityType;
                charRefEndPos = ParseNumericCharRef(expandType != EntityExpandType.OnlyGeneral, null, out entityType);
                Debug.Assert(entityType == EntityType.CharacterDec || entityType == EntityType.CharacterHex);
                return entityType;
            }

            // named reference
            else
            {
                // named character reference
                charRefEndPos = ParseNamedCharRef(expandType != EntityExpandType.OnlyGeneral, null);
                if (charRefEndPos >= 0)
                {
                    return EntityType.CharacterNamed;
                }

                return EntityType.Unexpanded;
            }
        }

        private bool ParsePI()
        {
            return ParsePI(null);
        }

        // Parses processing instruction; if piInDtdStringBuilder != null, the processing instruction is in DTD and
        // it will be saved in the passed string builder (target, whitespace & value).
        private bool ParsePI(BufferBuilder piInDtdStringBuilder)
        {
            if (parsingMode == ParsingMode.Full)
            {
                curNode.SetLineInfo(ps.LineNo, ps.LinePos);
            }

            Debug.Assert(stringBuilder.Length == 0);

            // parse target name
            int nameEndPos = ParseName();
            string target = nameTable.Add(ps.chars, ps.charPos, nameEndPos - ps.charPos);
            ///////ISSUE: rswaney: SPOT string class doesn't have a culture-aware or case-insensitive compare

            if (string.Compare(target, "xml" /*,true , CultureInfo.InvariantCulture */) == 0)
            {
                Throw(target.Equals("xml") ? Res.Xml_XmlDeclNotFirst : Res.Xml_InvalidPIName, target);
            }

            ps.charPos = nameEndPos;

            if (piInDtdStringBuilder == null)
            {
                if (!ignorePIs && parsingMode == ParsingMode.Full)
                {
                    curNode.SetNamedNode(XmlNodeType.ProcessingInstruction, target);
                }
            }
            else
            {
                piInDtdStringBuilder.Append(target);
            }

            // check mandatory whitespace
            char ch = ps.chars[ps.charPos];
            Debug.Assert(ch != 0);
            if (EatWhitespaces(piInDtdStringBuilder) == 0)
            {
                if (ps.charsUsed - ps.charPos < 2)
                {
                    ReadData();
                }

                if (ch != '?' || ps.chars[ps.charPos + 1] != '>')
                {
                    Throw(Res.Xml_BadNameChar, XmlException.BuildCharExceptionStr(ch));
                }
            }

            // scan processing instruction value
            int startPos, endPos;
            if (ParsePIValue(out startPos, out endPos))
            {
                if (piInDtdStringBuilder == null)
                {
                    if (ignorePIs)
                    {
                        return false;
                    }

                    if (parsingMode == ParsingMode.Full)
                    {
                        curNode.SetValue(ps.chars, startPos, endPos - startPos);
                    }
                }
                else
                {
                    piInDtdStringBuilder.Append(ps.chars, startPos, endPos - startPos);
                }
            }
            else
            {
                BufferBuilder sb;
                if (piInDtdStringBuilder == null)
                {
                    if (ignorePIs || parsingMode != ParsingMode.Full)
                    {
                        while (!ParsePIValue(out startPos, out endPos)) ;
                        return false;
                    }

                    sb = stringBuilder;
                    Debug.Assert(stringBuilder.Length == 0);
                }
                else
                {
                    sb = piInDtdStringBuilder;
                }

                do
                {
                    sb.Append(ps.chars, startPos, endPos - startPos);
                } while (!ParsePIValue(out startPos, out endPos));
                sb.Append(ps.chars, startPos, endPos - startPos);

                if (piInDtdStringBuilder == null)
                {
                    curNode.SetValue(stringBuilder.ToString());
                    stringBuilder.Length = 0;
                }
            }

            return true;
        }

        private bool ParsePIValue(out int outStartPos, out int outEndPos)
        {
            // read new characters into the buffer
            if (ps.charsUsed - ps.charPos < 2)
            {
                if (ReadData() == 0)
                {
                    Throw(ps.charsUsed, Res.Xml_UnexpectedEOF, "PI");
                }
            }

            int pos = ps.charPos;
            char[] chars = ps.chars;
            int rcount = 0;
            int rpos = -1;
            for (; ; )
            {
                while (chars[pos] > XmlCharType.MaxAsciiChar || ((xmlCharType.charProperties[chars[pos]] & XmlCharType.fText) != 0 && chars[pos] != '?'))
                {
                    pos++;
                }

                switch (chars[pos])
                {
                    // possibly end of PI
                    case '?':
                        if (chars[pos + 1] == '>')
                        {
                            if (rcount > 0)
                            {
                                Debug.Assert(!ps.eolNormalized);
                                ShiftBuffer(rpos + rcount, rpos, pos - rpos - rcount);
                                outEndPos = pos - rcount;
                            }
                            else
                            {
                                outEndPos = pos;
                            }

                            outStartPos = ps.charPos;
                            ps.charPos = pos + 2;
                            return true;
                        }
                        else if (pos + 1 == ps.charsUsed)
                        {
                            goto ReturnPartial;
                        }
                        else
                        {
                            pos++;
                            continue;
                        }

                    // eol
                    case (char)0xA:
                        pos++;
                        OnNewLine(pos);
                        continue;
                    case (char)0xD:
                        if (chars[pos + 1] == (char)0xA)
                        {
                            if (!ps.eolNormalized && parsingMode == ParsingMode.Full)
                            {
                                // EOL normalization of 0xD 0xA
                                if (pos - ps.charPos > 0)
                                {
                                    if (rcount == 0)
                                    {
                                        rcount = 1;
                                        rpos = pos;
                                    }
                                    else
                                    {
                                        ShiftBuffer(rpos + rcount, rpos, pos - rpos - rcount);
                                        rpos = pos - rcount;
                                        rcount++;
                                    }
                                }
                                else
                                {
                                    ps.charPos++;
                                }
                            }

                            pos += 2;
                        }
                        else if (pos + 1 < ps.charsUsed || ps.isEof)
                        {
                            if (!ps.eolNormalized)
                            {
                                chars[pos] = (char)0xA;             // EOL normalization of 0xD
                            }

                            pos++;
                        }
                        else
                        {
                            goto ReturnPartial;
                        }

                        OnNewLine(pos);
                        continue;
                    case '<':
                    case '&':
                    case ']':
                    case (char)0x9:
                        pos++;
                        continue;
                    default:
                        // end of buffer
                        if (pos == ps.charsUsed)
                        {
                            goto ReturnPartial;
                        }

                        // surrogate characters
                        else
                        {
                            char ch = chars[pos];
                            if (ch >= SurHighStart && ch <= SurHighEnd)
                            {
                                if (pos + 1 == ps.charsUsed)
                                {
                                    goto ReturnPartial;
                                }

                                pos++;
                                if (chars[pos] >= SurLowStart && chars[pos] <= SurLowEnd)
                                {
                                    pos++;
                                    continue;
                                }
                            }

                            ThrowInvalidChar(pos, ch);
                            break;
                        }
                }

            }

        ReturnPartial:
            if (rcount > 0)
            {
                ShiftBuffer(rpos + rcount, rpos, pos - rpos - rcount);
                outEndPos = pos - rcount;
            }
            else
            {
                outEndPos = pos;
            }

            outStartPos = ps.charPos;
            ps.charPos = pos;
            return false;
        }

        private bool ParseComment()
        {
            if (ignoreComments)
            {
                ParsingMode oldParsingMode = parsingMode;
                parsingMode = ParsingMode.SkipNode;
                ParseCDataOrComment(XmlNodeType.Comment);
                parsingMode = oldParsingMode;
                return false;
            }
            else
            {
                ParseCDataOrComment(XmlNodeType.Comment);
                return true;
            }
        }

        private void ParseCData()
        {
            ParseCDataOrComment(XmlNodeType.CDATA);
        }

        // Parses CDATA section or comment
        private void ParseCDataOrComment(XmlNodeType type)
        {
            int startPos, endPos;

            if (parsingMode == ParsingMode.Full)
            {
                curNode.SetLineInfo(ps.LineNo, ps.LinePos);
                Debug.Assert(stringBuilder.Length == 0);
                if (ParseCDataOrComment(type, out startPos, out endPos))
                {
                    curNode.SetValueNode(type, ps.chars, startPos, endPos - startPos);
                }
                else
                {
                    do
                    {
                        stringBuilder.Append(ps.chars, startPos, endPos - startPos);
                    } while (!ParseCDataOrComment(type, out startPos, out endPos));
                    stringBuilder.Append(ps.chars, startPos, endPos - startPos);
                    curNode.SetValueNode(type, stringBuilder.ToString());
                    stringBuilder.Length = 0;
                }
            }
            else
            {
                while (!ParseCDataOrComment(type, out startPos, out endPos)) ;
            }
        }

        // Parses a chunk of CDATA section or comment. Returns true when the end of CDATA or comment was reached.
        private bool ParseCDataOrComment(XmlNodeType type, out int outStartPos, out int outEndPos)
        {
            if (ps.charsUsed - ps.charPos < 3)
            {
                // read new characters into the buffer
                if (ReadData() == 0)
                {
                    Throw(Res.Xml_UnexpectedEOF, (type == XmlNodeType.Comment) ? "Comment" : "CDATA");
                }
            }

            int pos = ps.charPos;
            char[] chars = ps.chars;
            int rcount = 0;
            int rpos = -1;
            char stopChar = (type == XmlNodeType.Comment) ? '-' : ']';

            for (; ; )
            {
                while (chars[pos] > XmlCharType.MaxAsciiChar ||
                        ((xmlCharType.charProperties[chars[pos]] & XmlCharType.fText) != 0 && chars[pos] != stopChar))
                {
                    pos++;
                }

                // possibly end of comment or cdata section
                if (chars[pos] == stopChar)
                {
                    if (chars[pos + 1] == stopChar)
                    {
                        if (chars[pos + 2] == '>')
                        {
                            if (rcount > 0)
                            {
                                Debug.Assert(!ps.eolNormalized);
                                ShiftBuffer(rpos + rcount, rpos, pos - rpos - rcount);
                                outEndPos = pos - rcount;
                            }
                            else
                            {
                                outEndPos = pos;
                            }

                            outStartPos = ps.charPos;
                            ps.charPos = pos + 3;
                            return true;
                        }
                        else if (pos + 2 == ps.charsUsed)
                        {
                            goto ReturnPartial;
                        }
                        else if (type == XmlNodeType.Comment)
                        {
                            Throw(pos, Res.Xml_InvalidCommentChars);
                        }
                    }
                    else if (pos + 1 == ps.charsUsed)
                    {
                        goto ReturnPartial;
                    }

                    pos++;
                    continue;
                }
                else
                {
                    switch (chars[pos])
                    {
                        // eol
                        case (char)0xA:
                            pos++;
                            OnNewLine(pos);
                            continue;
                        case (char)0xD:
                            if (chars[pos + 1] == (char)0xA)
                            {
                                // EOL normalization of 0xD 0xA - shift the buffer
                                if (!ps.eolNormalized && parsingMode == ParsingMode.Full)
                                {
                                    if (pos - ps.charPos > 0)
                                    {
                                        if (rcount == 0)
                                        {
                                            rcount = 1;
                                            rpos = pos;
                                        }
                                        else
                                        {
                                            ShiftBuffer(rpos + rcount, rpos, pos - rpos - rcount);
                                            rpos = pos - rcount;
                                            rcount++;
                                        }
                                    }
                                    else
                                    {
                                        ps.charPos++;
                                    }
                                }

                                pos += 2;
                            }
                            else if (pos + 1 < ps.charsUsed || ps.isEof)
                            {
                                if (!ps.eolNormalized)
                                {
                                    chars[pos] = (char)0xA;             // EOL normalization of 0xD
                                }

                                pos++;
                            }
                            else
                            {
                                goto ReturnPartial;
                            }

                            OnNewLine(pos);
                            continue;
                        case '<':
                        case '&':
                        case ']':
                        case (char)0x9:
                            pos++;
                            continue;
                        default:
                            // end of buffer
                            if (pos == ps.charsUsed)
                            {
                                goto ReturnPartial;
                            }

                            // surrogate characters
                            char ch = chars[pos];
                            if (ch >= SurHighStart && ch <= SurHighEnd)
                            {
                                if (pos + 1 == ps.charsUsed)
                                {
                                    goto ReturnPartial;
                                }

                                pos++;
                                if (chars[pos] >= SurLowStart && chars[pos] <= SurLowEnd)
                                {
                                    pos++;
                                    continue;
                                }
                            }

                            ThrowInvalidChar(pos, ch);
                            break;
                    }
                }

            ReturnPartial:
                if (rcount > 0)
                {
                    ShiftBuffer(rpos + rcount, rpos, pos - rpos - rcount);
                    outEndPos = pos - rcount;
                }
                else
                {
                    outEndPos = pos;
                }

                outStartPos = ps.charPos;

                ps.charPos = pos;
                return false; // false == parsing of comment or CData section is not finished yet, must be called again
            }
        }

        private int EatWhitespaces(BufferBuilder sb)
        {
            int pos = ps.charPos;
            int wsCount = 0;
            char[] chars = ps.chars;

            for (; ; )
            {
                for (; ; )
                {
                    switch (chars[pos])
                    {
                        case (char)0xA:
                            pos++;
                            OnNewLine(pos);
                            continue;
                        case (char)0xD:
                            if (chars[pos + 1] == (char)0xA)
                            {
                                int tmp1 = pos - ps.charPos;
                                if (sb != null && !ps.eolNormalized)
                                {
                                    if (tmp1 > 0)
                                    {
                                        stringBuilder.Append(chars, ps.charPos, tmp1);
                                        wsCount += tmp1;
                                    }

                                    ps.charPos = pos + 1;
                                }

                                pos += 2;
                            }
                            else if (pos + 1 < ps.charsUsed || ps.isEof)
                            {
                                if (!ps.eolNormalized)
                                {
                                    chars[pos] = (char)0xA;             // EOL normalization of 0xD
                                }

                                pos++;
                            }
                            else
                            {
                                goto ReadData;
                            }

                            OnNewLine(pos);
                            continue;
                        case (char)0x9:
                        case (char)0x20:
                            pos++;
                            continue;
                        default:
                            if (pos == ps.charsUsed)
                            {
                                goto ReadData;
                            }
                            else
                            {
                                int tmp2 = pos - ps.charPos;
                                if (tmp2 > 0)
                                {
                                    if (sb != null)
                                    {
                                        sb.Append(ps.chars, ps.charPos, tmp2);
                                    }

                                    ps.charPos = pos;
                                    wsCount += tmp2;
                                }

                                return wsCount;
                            }
                    }
                }

            ReadData:
                int tmp3 = pos - ps.charPos;
                if (tmp3 > 0)
                {
                    if (sb != null)
                    {
                        sb.Append(ps.chars, ps.charPos, tmp3);
                    }

                    ps.charPos = pos;
                    wsCount += tmp3;
                }

                if (ReadData() == 0)
                {
                    if (ps.charsUsed - ps.charPos == 0)
                    {
                        return wsCount;
                    }

                    if (ps.chars[ps.charPos] != (char)0xD)
                    {
                        Debug.Assert(false, "We should never get to this point.");
                        Throw(Res.Xml_UnexpectedEOF1);
                    }

                    Debug.Assert(ps.isEof);
                }

                pos = ps.charPos;
                chars = ps.chars;
            }
        }

        private int ParseCharRefInline(int startPos, out int charCount, out EntityType entityType)
        {
            Debug.Assert(ps.chars[startPos] == '&');
            if (ps.chars[startPos + 1] == '#')
            {
                return ParseNumericCharRefInline(startPos, true, null, out charCount, out entityType);
            }
            else
            {
                charCount = 1;
                entityType = EntityType.CharacterNamed;
                return ParseNamedCharRefInline(startPos, true, null);
            }
        }

        // Parses numeric character entity reference (e.g. &#32; &#x20;).
        //      - replaces the last one or two character of the entity reference (';' and the character before) with the referenced
        //        character or surrogates pair (if expand == true)
        //      - returns position of the end of the character reference, that is of the character next to the original ';'
        //      - if (expand == true) then ps.charPos is changed to point to the replaced character
        private int ParseNumericCharRef(bool expand, BufferBuilder internalSubsetBuilder, out EntityType entityType)
        {
            for (; ; )
            {
                int newPos;
                int charCount;
                switch (newPos = ParseNumericCharRefInline(ps.charPos, expand, internalSubsetBuilder, out charCount, out entityType))
                {
                    case -2:
                        // read new characters in the buffer
                        if (ReadData() == 0)
                        {
                            Throw(Res.Xml_UnexpectedEOF);
                        }

                        Debug.Assert(ps.chars[ps.charPos] == '&');
                        continue;
                    default:
                        if (expand)
                        {
                            ps.charPos = newPos - charCount;
                        }

                        return newPos;
                }
            }
        }

        // Parses numeric character entity reference (e.g. &#32; &#x20;).
        // Returns -2 if more data is needed in the buffer
        // Otherwise
        //      - replaces the last one or two character of the entity reference (';' and the character before) with the referenced
        //        character or surrogates pair (if expand == true)
        //      - returns position of the end of the character reference, that is of the character next to the original ';'
        private int ParseNumericCharRefInline(int startPos, bool expand, BufferBuilder internalSubsetBuilder, out int charCount, out EntityType entityType)
        {
            Debug.Assert(ps.chars[startPos] == '&' && ps.chars[startPos + 1] == '#');

            int val;
            int pos;
            char[] chars;

            val = 0;
            int badDigitExceptionString = Res.Xml_DefaultException;
            chars = ps.chars;
            pos = startPos + 2;
            charCount = 0;

            if (chars[pos] == 'x')
            {
                pos++;
                badDigitExceptionString = Res.Xml_BadHexEntity;
                for (; ; )
                {
                    char ch = chars[pos];
                    if (ch >= '0' && ch <= '9')
                        val = val * 16 + ch - '0';
                    else if (ch >= 'a' && ch <= 'f')
                        val = val * 16 + 10 + ch - 'a';
                    else if (ch >= 'A' && ch <= 'F')
                        val = val * 16 + 10 + ch - 'A';
                    else
                        break;
                    pos++;
                }

                entityType = EntityType.CharacterHex;
            }
            else if (pos < ps.charsUsed)
            {
                badDigitExceptionString = Res.Xml_BadDecimalEntity;
                while (chars[pos] >= '0' && chars[pos] <= '9')
                {
                    val = val * 10 + chars[pos] - '0';
                    pos++;
                }

                entityType = EntityType.CharacterDec;
            }
            else
            {
                // need more data in the buffer
                entityType = EntityType.Unexpanded;
                return -2;
            }

            if (chars[pos] != ';')
            {
                if (pos == ps.charsUsed)
                {
                    // need more data in the buffer
                    return -2;
                }
                else
                {
                    Throw(pos, badDigitExceptionString);
                }
            }

            // simple character
            if (val <= char.MaxValue)
            {
                char ch = (char)val;
                if ((!xmlCharType.IsCharData(ch) || (ch >= SurLowStart && ch <= 0xdeff)) && checkCharacters)
                {
                    ThrowInvalidChar((ps.chars[ps.charPos + 2] == 'x') ? ps.charPos + 3 : ps.charPos + 2, ch);
                }

                if (expand)
                {
                    if (internalSubsetBuilder != null)
                    {
                        internalSubsetBuilder.Append(ps.chars, ps.charPos, pos - ps.charPos + 1);
                    }

                    chars[pos] = ch;
                }

                charCount = 1;
                return pos + 1;
            }

            // surrogate
            else
            {
                int v = val - 0x10000;
                int low = SurLowStart + v % 1024;
                int high = SurHighStart + v / 1024;

                if (normalize)
                {
                    char ch = (char)high;
                    if (ch >= SurHighStart && ch <= SurHighEnd)
                    {
                        ch = (char)low;
                        if (ch >= SurLowStart && ch <= SurLowEnd)
                        {
                            goto Return;
                        }
                    }

                    ThrowInvalidChar((ps.chars[ps.charPos + 2] == 'x') ? ps.charPos + 3 : ps.charPos + 2, (char)val);
                }

            Return:
                Debug.Assert(pos > 0);
                if (expand)
                {
                    if (internalSubsetBuilder != null)
                    {
                        internalSubsetBuilder.Append(ps.chars, ps.charPos, pos - ps.charPos + 1);
                    }

                    chars[pos - 1] = (char)high;
                    chars[pos] = (char)low;
                }

                charCount = 2;
                return pos + 1;
            }
        }

        // Parses named character entity reference (&amp; &apos; &lt; &gt; &quot;).
        // Returns -1 if the reference is not a character entity reference.
        // Otherwise
        //      - replaces the last character of the entity reference (';') with the referenced character (if expand == true)
        //      - returns position of the end of the character reference, that is of the character next to the original ';'
        //      - if (expand == true) then ps.charPos is changed to point to the replaced character
        private int ParseNamedCharRef(bool expand, BufferBuilder internalSubsetBuilder)
        {
            for (; ; )
            {
                int newPos;
                switch (newPos = ParseNamedCharRefInline(ps.charPos, expand, internalSubsetBuilder))
                {
                    case -1:
                        return -1;
                    case -2:
                        // read new characters in the buffer
                        if (ReadData() == 0)
                        {
                            return -1;
                        }

                        Debug.Assert(ps.chars[ps.charPos] == '&');
                        continue;
                    default:
                        if (expand)
                        {
                            ps.charPos = newPos - 1;
                        }

                        return newPos;
                }
            }
        }

        // Parses named character entity reference (&amp; &apos; &lt; &gt; &quot;).
        // Returns -1 if the reference is not a character entity reference.
        // Returns -2 if more data is needed in the buffer
        // Otherwise
        //      - replaces the last character of the entity reference (';') with the referenced character (if expand == true)
        //      - returns position of the end of the character reference, that is of the character next to the original ';'
        private int ParseNamedCharRefInline(int startPos, bool expand, BufferBuilder internalSubsetBuilder)
        {
            Debug.Assert(startPos < ps.charsUsed);
            Debug.Assert(ps.chars[startPos] == '&');
            Debug.Assert(ps.chars[startPos + 1] != '#');

            int pos = startPos + 1;
            char[] chars = ps.chars;
            char ch;

            switch (chars[pos])
            {
                // &apos; or &amp;
                case 'a':
                    pos++;
                    // &amp;
                    if (chars[pos] == 'm')
                    {
                        if (ps.charsUsed - pos >= 3)
                        {
                            if (chars[pos + 1] == 'p' && chars[pos + 2] == ';')
                            {
                                pos += 3;
                                ch = '&';
                                goto FoundCharRef;
                            }
                            else
                            {
                                return -1;
                            }
                        }
                    }

                    // &apos;
                    else if (chars[pos] == 'p')
                    {
                        if (ps.charsUsed - pos >= 4)
                        {
                            if (chars[pos + 1] == 'o' && chars[pos + 2] == 's' &&
                                    chars[pos + 3] == ';')
                            {
                                pos += 4;
                                ch = '\'';
                                goto FoundCharRef;
                            }
                            else
                            {
                                return -1;
                            }
                        }
                    }
                    else if (pos < ps.charsUsed)
                    {
                        return -1;
                    }
                    break;
                // &guot;
                case 'q':
                    if (ps.charsUsed - pos >= 5)
                    {
                        if (chars[pos + 1] == 'u' && chars[pos + 2] == 'o' &&
                                chars[pos + 3] == 't' && chars[pos + 4] == ';')
                        {
                            pos += 5;
                            ch = '"';
                            goto FoundCharRef;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    break;
                // &lt;
                case 'l':
                    if (ps.charsUsed - pos >= 3)
                    {
                        if (chars[pos + 1] == 't' && chars[pos + 2] == ';')
                        {
                            pos += 3;
                            ch = '<';
                            goto FoundCharRef;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    break;
                // &gt;
                case 'g':
                    if (ps.charsUsed - pos >= 3)
                    {
                        if (chars[pos + 1] == 't' && chars[pos + 2] == ';')
                        {
                            pos += 3;
                            ch = '>';
                            goto FoundCharRef;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    break;
                default:
                    return -1;
            }

            // need more data in the buffer
            return -2;

        FoundCharRef:
            Debug.Assert(pos > 0);
            if (expand)
            {
                if (internalSubsetBuilder != null)
                {
                    internalSubsetBuilder.Append(ps.chars, ps.charPos, pos - ps.charPos);
                }

                ps.chars[pos - 1] = ch;
            }

            return pos;
        }

        private int ParseName()
        {
            int colonPos;
            return ParseQName(false, 0, out colonPos);
        }

        private int ParseQName(out int colonPos)
        {
            return ParseQName(true, 0, out colonPos);
        }

        private int ParseQName(bool isQName, int startOffset, out int colonPos)
        {
            int colonOffset = -1;
            int pos = ps.charPos + startOffset;

        ContinueStartName:
            char[] chars = ps.chars;

            // start name char
            if (!(chars[pos] > XmlCharType.MaxAsciiChar || (xmlCharType.charProperties[chars[pos]] & XmlCharType.fNCStartName) != 0))
            {
                if (pos == ps.charsUsed)
                {
                    if (ReadDataInName(ref pos))
                    {
                        goto ContinueStartName;
                    }

                    Throw(pos, Res.Xml_UnexpectedEOF, "Name");
                }

                if (chars[pos] != ':')
                {
                    Throw(pos, Res.Xml_BadStartNameChar, XmlException.BuildCharExceptionStr(chars[pos]));
                }
            }

            pos++;

        ContinueName:
            // parse name
            while (chars[pos] > XmlCharType.MaxAsciiChar || (xmlCharType.charProperties[chars[pos]] & XmlCharType.fNCName) != 0)
            {
                pos++;
            }

            // colon
            if (chars[pos] == ':')
            {
                colonOffset = pos - ps.charPos;
                pos++;
                goto ContinueStartName;
            }

            // end of buffer
            else if (pos == ps.charsUsed)
            {
                if (ReadDataInName(ref pos))
                {
                    chars = ps.chars;
                    goto ContinueName;
                }

                Throw(pos, Res.Xml_UnexpectedEOF, "Name");
            }

            // end of name
            colonPos = (colonOffset == -1) ? -1 : ps.charPos + colonOffset;
            return pos;
        }

        private bool ReadDataInName(ref int pos)
        {
            int offset = pos - ps.charPos;
            bool newDataRead = (ReadData() != 0);
            pos = ps.charPos + offset;
            return newDataRead;
        }

        private NodeData AddNode(int nodeIndex, int nodeDepth)
        {
            Debug.Assert(nodeIndex < nodes.Length);
            Debug.Assert(nodes[nodes.Length - 1] == null);

            NodeData n = nodes[nodeIndex];
            if (n != null)
            {
                n.depth = nodeDepth;
                return n;
            }

            return AllocNode(nodeIndex, nodeDepth);
        }

        private NodeData AllocNode(int nodeIndex, int nodeDepth)
        {
            Debug.Assert(nodeIndex < nodes.Length);
            if (nodeIndex >= nodes.Length - 1)
            {
                NodeData[] newNodes = new NodeData[nodes.Length * 2];
                Array.Copy(nodes, 0, newNodes, 0, nodes.Length);
                nodes = newNodes;
            }

            Debug.Assert(nodeIndex < nodes.Length);

            NodeData node = nodes[nodeIndex];
            if (node == null)
            {
                node = new NodeData();
                nodes[nodeIndex] = node;
            }

            node.depth = nodeDepth;
            return node;
        }

        private NodeData AddAttributeNoChecks(string name, int attrDepth)
        {
            NodeData newAttr = AddNode(index + attrCount + 1, attrDepth);
            newAttr.SetNamedNode(XmlNodeType.Attribute, nameTable.Add(name));
            attrCount++;
            return newAttr;
        }

        //private NodeData AddAttribute( int endNamePos, int colonPos ) {
        //    // setup attribute name
        //    string localName = nameTable.Add( ps.chars, ps.charPos, endNamePos - ps.charPos );
        //    return AddAttribute( localName, "", localName );
        //}

        // WsdModification - Added support for storage of prefix, localname and nameWPrefix
        private NodeData AddAttribute(int endNamePos, int colonPos)
        {
            // setup attribute name
            string nameWPrefix = nameTable.Add(ps.chars, ps.charPos, endNamePos - ps.charPos);
            if (colonPos > -1)
            {
                string localName = new string(ps.chars, colonPos + 1, endNamePos - (colonPos + 1));
                string prefix = new string(ps.chars, ps.charPos, colonPos - ps.charPos);
                return AddAttribute(localName, prefix, nameWPrefix);
            }

            return AddAttribute(nameWPrefix, "", nameWPrefix);
        }

        private NodeData AddAttribute(string localName, string prefix, string nameWPrefix)
        {
            NodeData newAttr = AddNode(index + attrCount + 1, index + 1);

            // set attribute name
            newAttr.SetNamedNode(XmlNodeType.Attribute, localName, prefix, nameWPrefix);

            // pre-check attribute for duplicate: hash by first local name char
            int attrHash = 1 << (localName[0] & 0x1F);
            if ((attrHashtable & attrHash) == 0)
            {
                attrHashtable |= attrHash;
            }
            else
            {
                // there are probably 2 attributes beginning with the same letter -> check all previous
                // attributes
                if (attrDuplWalkCount < MaxAttrDuplWalkCount)
                {
                    attrDuplWalkCount++;
                    for (int i = index + 1; i < index + attrCount + 1; i++)
                    {
                        NodeData attr = nodes[i];
                        Debug.Assert(attr.type == XmlNodeType.Attribute);
                        if (Ref.Equal(attr.localName, newAttr.localName))
                        {
                            attrDuplWalkCount = MaxAttrDuplWalkCount;
                            break;
                        }
                    }
                }
            }

            attrCount++;
            return newAttr;
        }

        private void PopElementContext()
        {
            // pop xml context
            if (curNode.xmlContextPushed)
            {
                PopXmlContext();
            }
        }

        private void OnNewLine(int pos)
        {
            ps.lineNo++;
            ps.lineStartPos = pos - 1;
        }

        private void OnEof()
        {
            Debug.Assert(ps.isEof);
            curNode = nodes[0];
            curNode.Clear(XmlNodeType.None);
            curNode.SetLineInfo(ps.LineNo, ps.LinePos);

            parsingFunction = ParsingFunction.Eof;
            readState = ReadState.EndOfFile;

            reportedEncoding = null;
        }

        private void ResetAttributes()
        {
            if (fullAttrCleanup)
            {
                FullAttributeCleanup();
            }

            curAttrIndex = -1;
            attrCount = 0;
            attrHashtable = 0;
            attrDuplWalkCount = 0;
        }

        private void FullAttributeCleanup()
        {
            for (int i = index + 1; i < index + attrCount + 1; i++)
            {
                NodeData attr = nodes[i];
                attr.nextAttrValueChunk = null;
                attr.IsDefaultAttribute = false;
            }

            fullAttrCleanup = false;
        }

        private void PushXmlContext()
        {
            xmlContext = new XmlContext(xmlContext);
            curNode.xmlContextPushed = true;
        }

        private void PopXmlContext()
        {
            Debug.Assert(curNode.xmlContextPushed);
            xmlContext = xmlContext.previousContext;
            curNode.xmlContextPushed = false;
        }

        // Returns the whitespace node type according to the current whitespaceHandling setting and xml:space
        private XmlNodeType GetWhitespaceType()
        {
            if (whitespaceHandling != WhitespaceHandling.None)
            {
                if (xmlContext.xmlSpace == XmlSpace.Preserve)
                {
                    return XmlNodeType.SignificantWhitespace;
                }

                if (whitespaceHandling == WhitespaceHandling.All)
                {
                    return XmlNodeType.Whitespace;
                }
            }

            return XmlNodeType.None;
        }

        private XmlNodeType GetTextNodeType(int orChars)
        {
            if (orChars > 0x20)
            {
                return XmlNodeType.Text;
            }
            else
            {
                return GetWhitespaceType();
            }
        }

        private void InitIncrementalRead(IncrementalReadDecoder decoder)
        {
            ResetAttributes();

            decoder.Reset();
            incReadDecoder = decoder;
            incReadState = IncrementalReadState.Text;
            incReadDepth = 1;
            incReadLeftStartPos = ps.charPos;
            incReadLineInfo.Set(ps.LineNo, ps.LinePos);

            parsingFunction = ParsingFunction.InIncrementalRead;
        }

        private int IncrementalRead(Array array, int index, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException((incReadDecoder is IncrementalReadCharsDecoder) ? "buffer" : "array");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException((incReadDecoder is IncrementalReadCharsDecoder) ? "count" : "len");
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException((incReadDecoder is IncrementalReadCharsDecoder) ? "index" : "offset");
            }

            if (array.Length - index < count)
            {
                throw new ArgumentException((incReadDecoder is IncrementalReadCharsDecoder) ? "count" : "len");
            }

            if (count == 0)
            {
                return 0;
            }

            curNode.lineInfo = incReadLineInfo;

            incReadDecoder.SetNextOutputBuffer(array, index, count);
            IncrementalRead();
            return incReadDecoder.DecodedCount;
        }

        private int IncrementalRead()
        {
            int charsDecoded = 0;

        OuterContinue:
            int charsLeft = incReadLeftEndPos - incReadLeftStartPos;
            if (charsLeft > 0)
            {
                int count;
                try
                {
                    count = incReadDecoder.Decode(ps.chars, incReadLeftStartPos, charsLeft);
                }
                catch (XmlException e)
                {
                    ReThrow(e, (int)incReadLineInfo.lineNo, (int)incReadLineInfo.linePos);
                    return 0;
                }

                if (count < charsLeft)
                {
                    incReadLeftStartPos += count;
                    incReadLineInfo.linePos += count; // we have never more then 1 line cached
                    return count;
                }
                else
                {
                    incReadLeftStartPos = 0;
                    incReadLeftEndPos = 0;
                    incReadLineInfo.linePos += count;
                    if (incReadDecoder.IsFull)
                    {
                        return count;
                    }
                }
            }

            int startPos = 0;
            int pos = 0;

            for (; ; )
            {

                switch (incReadState)
                {
                    case IncrementalReadState.Text:
                    case IncrementalReadState.Attributes:
                    case IncrementalReadState.AttributeValue:
                        break;
                    case IncrementalReadState.PI:
                        if (ParsePIValue(out startPos, out pos))
                        {
                            Debug.Assert(StrEqual(ps.chars, ps.charPos - 2, 2, "?>"));
                            ps.charPos -= 2;
                            incReadState = IncrementalReadState.Text;
                        }

                        goto Append;
                    case IncrementalReadState.Comment:
                        if (ParseCDataOrComment(XmlNodeType.Comment, out startPos, out pos))
                        {
                            Debug.Assert(StrEqual(ps.chars, ps.charPos - 3, 3, "-->"));
                            ps.charPos -= 3;
                            incReadState = IncrementalReadState.Text;
                        }

                        goto Append;
                    case IncrementalReadState.CDATA:
                        if (ParseCDataOrComment(XmlNodeType.CDATA, out startPos, out pos))
                        {
                            Debug.Assert(StrEqual(ps.chars, ps.charPos - 3, 3, "]]>"));
                            ps.charPos -= 3;
                            incReadState = IncrementalReadState.Text;
                        }

                        goto Append;
                    case IncrementalReadState.EndElement:
                        parsingFunction = ParsingFunction.PopElementContext;
                        nextParsingFunction = (index > 0 || fragmentType != XmlNodeType.Document) ? ParsingFunction.ElementContent
                                                                                                    : ParsingFunction.DocumentContent;
                        Read();
                        incReadState = IncrementalReadState.End;
                        goto case IncrementalReadState.End;
                    case IncrementalReadState.End:
                        return charsDecoded;
                    case IncrementalReadState.ReadData:
                        if (ReadData() == 0)
                        {
                            ThrowUnclosedElements();
                        }

                        incReadState = IncrementalReadState.Text;
                        startPos = ps.charPos;
                        pos = startPos;
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }

                Debug.Assert(incReadState == IncrementalReadState.Text ||
                              incReadState == IncrementalReadState.Attributes ||
                              incReadState == IncrementalReadState.AttributeValue);

                char[] chars = ps.chars;
                startPos = ps.charPos;
                pos = startPos;

                for (; ; )
                {
                    incReadLineInfo.Set(ps.LineNo, ps.LinePos);

                    char c;
                    if (incReadState == IncrementalReadState.Attributes)
                    {
                        while ((c = chars[pos]) > XmlCharType.MaxAsciiChar ||
                                ((xmlCharType.charProperties[c] & XmlCharType.fAttrValue) != 0 && c != '/'))
                        {
                            pos++;
                        }
                    }
                    else
                    {
                        while ((c = chars[pos]) > XmlCharType.MaxAsciiChar || (xmlCharType.charProperties[c] & XmlCharType.fAttrValue) != 0)
                        {
                            pos++;
                        }
                    }

                    if (chars[pos] == '&' || chars[pos] == (char)0x9)
                    {
                        pos++;
                        continue;
                    }

                    if (pos - startPos > 0)
                    {
                        goto AppendAndUpdateCharPos;
                    }

                    switch (chars[pos])
                    {
                        // eol
                        case (char)0xA:
                            pos++;
                            OnNewLine(pos);
                            continue;
                        case (char)0xD:
                            if (chars[pos + 1] == (char)0xA)
                            {
                                pos += 2;
                            }
                            else if (pos + 1 < ps.charsUsed)
                            {
                                pos++;
                            }
                            else
                            {
                                goto ReadData;
                            }

                            OnNewLine(pos);
                            continue;
                        // some tag
                        case '<':
                            if (incReadState != IncrementalReadState.Text)
                            {
                                pos++;
                                continue;
                            }

                            if (ps.charsUsed - pos < 2)
                            {
                                goto ReadData;
                            }

                            switch (chars[pos + 1])
                            {
                                // pi
                                case '?':
                                    pos += 2;
                                    incReadState = IncrementalReadState.PI;
                                    goto AppendAndUpdateCharPos;
                                // comment
                                case '!':
                                    if (ps.charsUsed - pos < 4)
                                    {
                                        goto ReadData;
                                    }

                                    if (chars[pos + 2] == '-' && chars[pos + 3] == '-')
                                    {
                                        pos += 4;
                                        incReadState = IncrementalReadState.Comment;
                                        goto AppendAndUpdateCharPos;
                                    }

                                    if (ps.charsUsed - pos < 9)
                                    {
                                        goto ReadData;
                                    }

                                    if (StrEqual(chars, pos + 2, 7, "[CDATA["))
                                    {
                                        pos += 9;
                                        incReadState = IncrementalReadState.CDATA;
                                        goto AppendAndUpdateCharPos;
                                    }
                                    else
                                    {
                                        ;//Throw( );
                                    }
                                    break;
                                // end tag
                                case '/':
                                    {
                                        Debug.Assert(ps.charPos - pos == 0);
                                        int colonPos;
                                        int endPos = ParseQName(true, 2, out colonPos);
                                        if (StrEqual(chars, ps.charPos + 2, endPos - ps.charPos - 2, curNode.GetNameWPrefix(nameTable)) &&
                                            (ps.chars[endPos] == '>' || xmlCharType.IsWhiteSpace(ps.chars[endPos])))
                                        {

                                            if (--incReadDepth > 0)
                                            {
                                                pos = endPos + 1;
                                                continue;
                                            }

                                            ps.charPos = endPos;
                                            if (xmlCharType.IsWhiteSpace(ps.chars[endPos]))
                                            {
                                                EatWhitespaces(null);
                                            }

                                            if (ps.chars[ps.charPos] != '>')
                                            {
                                                ThrowUnexpectedToken(">");
                                            }

                                            ps.charPos++;

                                            incReadState = IncrementalReadState.EndElement;
                                            goto OuterContinue;
                                        }
                                        else
                                        {
                                            pos = endPos;
                                            continue;
                                        }
                                    }

                                // start tag
                                default:
                                    {
                                        Debug.Assert(ps.charPos - pos == 0);
                                        int colonPos;
                                        int endPos = ParseQName(true, 1, out colonPos);
                                        if (StrEqual(ps.chars, ps.charPos + 1, endPos - ps.charPos - 1, curNode.localName) &&
                                            (ps.chars[endPos] == '>' || ps.chars[endPos] == '/' || xmlCharType.IsWhiteSpace(ps.chars[endPos])))
                                        {
                                            incReadDepth++;
                                            incReadState = IncrementalReadState.Attributes;
                                            pos = endPos;
                                            goto AppendAndUpdateCharPos;
                                        }

                                        pos = endPos;
                                        startPos = ps.charPos;
                                        chars = ps.chars;
                                        continue;
                                    }
                            }
                            break;
                        // end of start tag
                        case '/':
                            if (incReadState == IncrementalReadState.Attributes)
                            {
                                if (ps.charsUsed - pos < 2)
                                {
                                    goto ReadData;
                                }

                                if (chars[pos + 1] == '>')
                                {
                                    incReadState = IncrementalReadState.Text;
                                    incReadDepth--;
                                }
                            }

                            pos++;
                            continue;
                        // end of start tag
                        case '>':
                            if (incReadState == IncrementalReadState.Attributes)
                            {
                                incReadState = IncrementalReadState.Text;
                            }

                            pos++;
                            continue;
                        case '"':
                        case '\'':
                            switch (incReadState)
                            {
                                case IncrementalReadState.AttributeValue:
                                    if (chars[pos] == curNode.quoteChar)
                                    {
                                        incReadState = IncrementalReadState.Attributes;
                                    }
                                    break;
                                case IncrementalReadState.Attributes:
                                    curNode.quoteChar = chars[pos];
                                    incReadState = IncrementalReadState.AttributeValue;
                                    break;
                            }

                            pos++;
                            continue;
                        default:
                            // end of buffer
                            if (pos == ps.charsUsed)
                            {
                                goto ReadData;
                            }

                            // surrogate chars or invalid chars are ignored
                            else
                            {
                                pos++;
                                continue;
                            }
                    }
                }

            ReadData:
                incReadState = IncrementalReadState.ReadData;

            AppendAndUpdateCharPos:
                ps.charPos = pos;

            Append:
                // decode characters
                int charsParsed = pos - startPos;
                if (charsParsed > 0)
                {
                    int count;
                    try
                    {
                        count = incReadDecoder.Decode(ps.chars, startPos, charsParsed);
                    }
                    catch (XmlException e)
                    {
                        ReThrow(e, (int)incReadLineInfo.lineNo, (int)incReadLineInfo.linePos);
                        return 0;
                    }

                    Debug.Assert(count == charsParsed || incReadDecoder.IsFull, "Check if decoded consumed all characters unless it's full.");
                    charsDecoded += count;
                    if (incReadDecoder.IsFull)
                    {
                        incReadLeftStartPos = startPos + count;
                        incReadLeftEndPos = pos;
                        incReadLineInfo.linePos += count; // we have never more than 1 line cached
                        return charsDecoded;
                    }
                }
            }
        }

        private void FinishIncrementalRead()
        {
            incReadDecoder = new IncrementalReadDummyDecoder();
            IncrementalRead();
            Debug.Assert(IncrementalRead() == 0, "Previous call of IncrementalRead should eat up all characters!");
            incReadDecoder = null;
        }

        private bool ParseFragmentAttribute()
        {
            Debug.Assert(fragmentType == XmlNodeType.Attribute);

            // if first call then parse the whole attribute value
            if (curNode.type == XmlNodeType.None)
            {
                curNode.type = XmlNodeType.Attribute;
                curAttrIndex = 0;
                ParseAttributeValueSlow(ps.charPos, '"', curNode);
            }
            else
            {
                parsingFunction = ParsingFunction.InReadAttributeValue;
            }

            // return attribute value chunk
            if (ReadAttributeValue())
            {
                Debug.Assert(parsingFunction == ParsingFunction.InReadAttributeValue);
                parsingFunction = ParsingFunction.FragmentAttribute;
                return true;
            }
            else
            {
                OnEof();
                return false;
            }
        }

        private bool ParseAttributeValueChunk()
        {
            char[] chars = ps.chars;
            int pos = ps.charPos;

            curNode = AddNode(index + attrCount + 1, index + 2);
            curNode.SetLineInfo(ps.LineNo, ps.LinePos);

            Debug.Assert(stringBuilder.Length == 0);

            for (; ; )
            {
                while (chars[pos] > XmlCharType.MaxAsciiChar || (xmlCharType.charProperties[chars[pos]] & XmlCharType.fAttrValue) != 0)
                {
                    pos++;
                }

                switch (chars[pos])
                {
                    // eol D
                    case (char)0xD:
                        Debug.Assert(ps.eolNormalized, "Entity replacement text for attribute values should be EOL-normalized!");
                        pos++;
                        continue;
                    // eol A, tab
                    case (char)0xA:
                    case (char)0x9:
                        if (normalize)
                        {
                            chars[pos] = (char)0x20;  // CDATA normalization of 0xA and 0x9
                        }

                        pos++;
                        continue;
                    case '"':
                    case '\'':
                    case '>':
                        pos++;
                        continue;
                    // attribute values cannot contain '<'
                    case '<':
                        Throw(pos, Res.Xml_BadAttributeChar, XmlException.BuildCharExceptionStr('<'));
                        break;
                    // entity reference
                    case '&':
                        if (pos - ps.charPos > 0)
                        {
                            stringBuilder.Append(chars, ps.charPos, pos - ps.charPos);
                        }

                        ps.charPos = pos;

                        // expand char entities but not general entities
                        switch (HandleEntityReference(true, EntityExpandType.OnlyCharacter, out pos))
                        {
                            case EntityType.CharacterDec:
                            case EntityType.CharacterHex:
                            case EntityType.CharacterNamed:
                                chars = ps.chars;
                                if (normalize && xmlCharType.IsWhiteSpace(chars[ps.charPos]) && pos - ps.charPos == 1)
                                {
                                    chars[ps.charPos] = (char)0x20;  // CDATA normalization of character references in entities
                                }
                                break;
                            case EntityType.Unexpanded:
                                Debug.Assert(false, "Found general entity in attribute");
                                throw new NotSupportedException();

                            default:
                                Debug.Assert(false, "We should never get to this point.");
                                break;
                        }

                        chars = ps.chars;
                        continue;
                    default:
                        // end of buffer
                        if (pos == ps.charsUsed)
                        {
                            goto ReadData;
                        }

                        // surrogate chars
                        else
                        {
                            char ch = chars[pos];
                            if (ch >= SurHighStart && ch <= SurHighEnd)
                            {
                                if (pos + 1 == ps.charsUsed)
                                {
                                    goto ReadData;
                                }

                                pos++;
                                if (chars[pos] >= SurLowStart && chars[pos] <= SurLowEnd)
                                {
                                    pos++;
                                    continue;
                                }
                            }

                            ThrowInvalidChar(pos, ch);
                            break;
                        }
                }

            ReadData:
                if (pos - ps.charPos > 0)
                {
                    stringBuilder.Append(chars, ps.charPos, pos - ps.charPos);
                    ps.charPos = pos;
                }

                // read new characters into the buffer
                if (ReadData() == 0)
                {
                    if (stringBuilder.Length > 0)
                    {
                        goto ReturnText;
                    }
                    else
                    {
                        Debug.Assert(false, "We should never get to this point.");
                    }
                }

                pos = ps.charPos;
                chars = ps.chars;
            }

        ReturnText:
            if (pos - ps.charPos > 0)
            {
                stringBuilder.Append(chars, ps.charPos, pos - ps.charPos);
                ps.charPos = pos;
            }

            curNode.SetValueNode(XmlNodeType.Text, stringBuilder.ToString());
            stringBuilder.Length = 0;
            return true;
        }

        private void ParseXmlDeclarationFragment()
        {
            try
            {
                ParseXmlDeclaration();
            }
            catch (XmlException e)
            {
                ReThrow(e, e.LineNumber, e.LinePosition - 6); // 6 == strlen( "<?xml " );
            }
        }

        private void ThrowUnexpectedToken(int pos, string expectedToken)
        {
            ThrowUnexpectedToken(pos, expectedToken, null);
        }

        private void ThrowUnexpectedToken(string expectedToken1)
        {
            ThrowUnexpectedToken(expectedToken1, null);
        }

        private void ThrowUnexpectedToken(int pos, string expectedToken1, string expectedToken2)
        {
            ps.charPos = pos;
            ThrowUnexpectedToken(expectedToken1, expectedToken2);
        }

        private void ThrowUnexpectedToken(string expectedToken1, string expectedToken2)
        {
            string unexpectedToken = ParseUnexpectedToken();
            if (expectedToken2 != null)
            {
                Throw(Res.Xml_UnexpectedTokens2, new string[3] { unexpectedToken, expectedToken1, expectedToken2 });
            }
            else
            {
                Throw(Res.Xml_UnexpectedTokenEx, new string[2] { unexpectedToken, expectedToken1 });
            }
        }

        private string ParseUnexpectedToken(int pos)
        {
            ps.charPos = pos;
            return ParseUnexpectedToken();
        }

        private string ParseUnexpectedToken()
        {
            if (xmlCharType.IsNCNameChar(ps.chars[ps.charPos]))
            {
                int pos = ps.charPos + 1;
                while (xmlCharType.IsNCNameChar(ps.chars[pos]))
                {
                    pos++;
                }

                return new string(ps.chars, ps.charPos, pos - ps.charPos);
            }
            else
            {
                Debug.Assert(ps.charPos < ps.charsUsed);
                return new string(ps.chars, ps.charPos, 1);
            }
        }

        private int GetIndexOfAttributeWithoutPrefix(string name)
        {
            name = nameTable.Get(name);
            if (name == null)
            {
                return -1;
            }

            for (int i = index + 1; i < index + attrCount + 1; i++)
            {
                if (Ref.Equal(nodes[i].localName, name) && nodes[i].prefix.Length == 0)
                {
                    return i;
                }
            }

            return -1;
        }

        private int GetIndexOfAttributeWithPrefix(string name)
        {
            name = nameTable.Add(name);
            if (name == null)
            {
                return -1;
            }

            for (int i = index + 1; i < index + attrCount + 1; i++)
            {
                if (Ref.Equal(nodes[i].GetNameWPrefix(nameTable), name))
                {
                    return i;
                }
            }

            return -1;
        }

        bool MoveToNextContentNode(bool moveIfOnContentNode)
        {
            do
            {
                switch (curNode.type)
                {
                    case XmlNodeType.Attribute:
                        return !moveIfOnContentNode;
                    case XmlNodeType.Text:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                    case XmlNodeType.CDATA:
                        if (!moveIfOnContentNode)
                        {
                            return true;
                        }
                        break;
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.Comment:
                    case XmlNodeType.EndEntity:
                        // skip comments, pis and end entity nodes
                        break;
                    case XmlNodeType.EntityReference:
                        ResolveEntity();
                        break;
                    default:
                        return false;
                }

                moveIfOnContentNode = false;
            } while (Read());
            return false;
        }

#if NETCF_XVR_SUPPORT
        internal bool XmlValidatingReaderCompatibilityMode {
            set {
                validatingReaderCompatFlag = value;
            }
        }

#endif
#if SCHEMA_VALIDATION
        internal ValidationEventHandler ValidationEventHandler {
            set {
                validationEventHandler = value;
            }
        }

#endif //SCHEMA_VALIDATION

        internal XmlNodeType FragmentType
        {
            get
            {
                return fragmentType;
            }
        }

        internal void ChangeCurrentNodeType(XmlNodeType newNodeType)
        {
            Debug.Assert(curNode.type == XmlNodeType.Whitespace && newNodeType == XmlNodeType.SignificantWhitespace, "Incorrect node type change!");
            curNode.type = newNodeType;
        }

        internal object InternalTypedValue
        {
            get
            {
                return curNode.typedValue;
            }

            set
            {
                curNode.typedValue = value;
            }
        }

        internal bool StandAlone
        {
            get
            {
                return standalone;
            }
        }

        internal ConformanceLevel V1ComformanceLevel
        {
            get
            {
                return fragmentType == XmlNodeType.Element ? ConformanceLevel.Fragment : ConformanceLevel.Document;
            }
        }

        static internal void AdjustLineInfo(char[] chars, int startPos, int endPos, bool isNormalized, ref LineInfo lineInfo)
        {
            int lastNewLinePos = -1;
            int i = startPos;
            while (i < endPos)
            {
                switch (chars[i])
                {
                    case '\n':
                        lineInfo.lineNo++;
                        lastNewLinePos = i;
                        break;
                    case '\r':
                        if (isNormalized)
                        {
                            break;
                        }

                        lineInfo.lineNo++;
                        lastNewLinePos = i;
                        if (i + 1 < endPos && chars[i + 1] == '\n')
                        {
                            i++;
                            lastNewLinePos++;
                        }
                        break;
                }

                i++;
            }

            if (lastNewLinePos >= 0)
            {
                lineInfo.linePos = endPos - lastNewLinePos;
            }
        }

        //----------------------------------------------------------------------------------------------
        // included from "XmlTextReaderHelpers.cs" because partial class definitions not supported
        //----------------------------------------------------------------------------------------------

        //
        // ParsingState
        //
        // Parsing state (aka. scanner data) - holds parsing buffer and entity input data information
        private struct ParsingState
        {
            // character buffer
            internal char[] chars;
            internal int charPos;
            internal int charsUsed;
            internal Encoding encoding;
            internal bool appendMode;

            // input stream & byte buffer
            internal Stream stream;
            internal Decoder decoder;
            internal byte[] bytes;
            internal int bytePos;
            internal int bytesUsed;

            // text reader input
            internal TextReader textReader;

            // current line number & position
            internal int lineNo;
            internal int lineStartPos;

            // base uri of the current entity
            internal string baseUriStr;

            // eof flag of the entity
            internal bool isEof;
            internal bool isStreamEof;

            // normalization
            internal bool eolNormalized;

            internal void Clear()
            {
                chars = null;
                charPos = 0;
                charsUsed = 0;
                encoding = null;
                stream = null;
                decoder = null;
                bytes = null;
                bytePos = 0;
                bytesUsed = 0;
                textReader = null;
                lineNo = 1;
                lineStartPos = -1;
                baseUriStr = "";
                isEof = false;
                isStreamEof = false;
                eolNormalized = true;
            }

            internal void Close(bool closeInput)
            {
                if (closeInput)
                {
                    if (stream != null)
                    {
                        stream.Close();
                    }
                }
            }

            internal int LineNo
            {
                get
                {
                    return lineNo;
                }
            }

            internal int LinePos
            {
                get
                {
                    return charPos - lineStartPos;
                }
            }
        }

        //
        // XmlContext
        //
        private class XmlContext
        {
            internal XmlSpace xmlSpace;
            internal string xmlLang;
            // WsdModification - Add namesapce collection. We will maintain a namespace list per context.
            // Storing a namespace per context should provide a way to assend path of context when searching
            // for a namespace.
            internal XmlNamespaces xmlNamespaces;
            internal XmlContext previousContext;

            internal XmlContext()
            {
                xmlSpace = XmlSpace.None;
                xmlLang = "";
                previousContext = null;
                xmlNamespaces = new XmlNamespaces();
            }

            internal XmlContext(XmlContext previousContext)
            {
                this.xmlSpace = previousContext.xmlSpace;
                this.xmlLang = previousContext.xmlLang;
                this.previousContext = previousContext;
                this.xmlNamespaces = previousContext.xmlNamespaces;
            }
        }

        //
        // NodeData
        //
        private class NodeData : IComparable
        {
            // static instance with no data - is used when XmlTextReader is closed
            static NodeData s_None;

            // NOTE: Do not use this property for reference comparison. It may not be unique.
            internal static NodeData None
            {
                get
                {
                    if (s_None == null)
                    {
                        // no locking; s_None is immutable so it's not a problem that it may get initialized more than once
                        s_None = new NodeData();
                    }

                    return s_None;
                }
            }

            // type
            internal XmlNodeType type;

            // name
            internal string localName;
            internal string prefix;
            internal string ns;
            internal string nameWPrefix;

            // value:
            // value == null -> the value is kept in the 'chars' buffer starting at valueStartPos and valueLength long
            string value;
            char[] chars;
            int valueStartPos;
            int valueLength;

            // main line info
            internal LineInfo lineInfo;

            // second line info
            //ISSUE: rswaney - Haven't determined how to get rid of the "not initialized" warngin on this variable
            // so temporarily initializing it here. The CF versions disables the warning with a pragma
            internal LineInfo lineInfo2 = new LineInfo(0, 0);

            // quote char for attributes
            internal char quoteChar;

            // depth
            internal int depth;

            // empty element / default attribute
            bool isEmptyOrDefault;

            // helper members
            internal bool xmlContextPushed;

            // attribute value chunks
            internal NodeData nextAttrValueChunk;

            // type info
            internal object schemaType;
            internal object typedValue;

            internal NodeData()
            {
                Clear(XmlNodeType.None);
                xmlContextPushed = false;
            }

            internal int LineNo
            {
                get
                {
                    return lineInfo.lineNo;
                }
            }

            internal int LinePos
            {
                get
                {
                    return lineInfo.linePos;
                }
            }

            internal bool IsEmptyElement
            {
                get
                {
                    return type == XmlNodeType.Element && isEmptyOrDefault;
                }

                set
                {
                    Debug.Assert(type == XmlNodeType.Element);
                    isEmptyOrDefault = value;
                }
            }

            internal bool IsDefaultAttribute
            {
                get
                {
                    return type == XmlNodeType.Attribute && isEmptyOrDefault;
                }

                set
                {
                    Debug.Assert(type == XmlNodeType.Attribute);
                    isEmptyOrDefault = value;
                }
            }

            internal bool ValueBuffered
            {
                get
                {
                    return value == null;
                }
            }

            internal string StringValue
            {
                get
                {
                    Debug.Assert(valueStartPos >= 0 || this.value != null, "Value not ready.");

                    if (this.value == null)
                    {
                        this.value = new string(chars, valueStartPos, valueLength);
                    }

                    return this.value;
                }
            }

            internal void Clear(XmlNodeType type)
            {
                this.type = type;
                ClearName();
                value = "";
                valueStartPos = -1;
                nameWPrefix = "";
                schemaType = null;
                typedValue = null;
            }

            internal void ClearName()
            {
                localName = "";
                prefix = "";
                ns = "";
                nameWPrefix = "";
            }

            internal void SetLineInfo(int lineNo, int linePos)
            {
                lineInfo.Set(lineNo, linePos);
            }

            internal void SetLineInfo2(int lineNo, int linePos)
            {
                lineInfo2.Set(lineNo, linePos);
            }

            internal void SetValueNode(XmlNodeType type, string value)
            {
                Debug.Assert(value != null);

                this.type = type;
                ClearName();
                this.value = value;
                this.valueStartPos = -1;
            }

            internal void SetValueNode(XmlNodeType type, char[] chars, int startPos, int len)
            {
                this.type = type;
                ClearName();

                this.value = null;
                this.chars = chars;
                this.valueStartPos = startPos;
                this.valueLength = len;
            }

            internal void SetNamedNode(XmlNodeType type, string localName)
            {
                SetNamedNode(type, localName, "", localName);
            }

            internal void SetNamedNode(XmlNodeType type, string localName, string prefix, string nameWPrefix)
            {
                Debug.Assert(localName != null);
                Debug.Assert(localName.Length > 0);

                this.type = type;
                this.localName = localName;
                this.prefix = prefix;
                this.nameWPrefix = nameWPrefix;
                this.ns = "";
                this.value = "";
                this.valueStartPos = -1;
            }

            internal void SetValue(string value)
            {
                this.valueStartPos = -1;
                this.value = value;
            }

            internal void SetValue(char[] chars, int startPos, int len)
            {
                this.value = null;
                this.chars = chars;
                this.valueStartPos = startPos;
                this.valueLength = len;
            }

            internal void OnBufferInvalidated()
            {
                if (this.value == null)
                {
                    Debug.Assert(valueStartPos != -1);
                    Debug.Assert(chars != null);
                    this.value = new string(chars, valueStartPos, valueLength);
                }

                valueStartPos = -1;
            }

            internal string GetAtomizedValue(XmlNameTable nameTable)
            {
                if (this.value == null)
                {
                    Debug.Assert(valueStartPos != -1);
                    Debug.Assert(chars != null);
                    return nameTable.Add(chars, valueStartPos, valueLength);
                }
                else
                {
                    return nameTable.Add(this.value);
                }
            }

            internal void CopyTo(BufferBuilder sb)
            {
                CopyTo(0, sb);
            }

            internal void CopyTo(int valueOffset, BufferBuilder sb)
            {
                if (value == null)
                {
                    Debug.Assert(valueStartPos != -1);
                    Debug.Assert(chars != null);
                    sb.Append(chars, valueStartPos + valueOffset, valueLength - valueOffset);
                }
                else
                {
                    if (valueOffset <= 0)
                    {
                        sb.Append(value);
                    }
                    else
                    {
                        sb.Append(value, valueOffset, value.Length - valueOffset);
                    }
                }
            }

            internal int CopyTo(int valueOffset, char[] buffer, int offset, int length)
            {
                if (value == null)
                {
                    Debug.Assert(valueStartPos != -1);
                    Debug.Assert(chars != null);
                    int copyCount = valueLength - valueOffset;
                    if (copyCount > length)
                    {
                        copyCount = length;
                    }

                    Array.Copy(chars, (valueStartPos + valueOffset), buffer, offset, copyCount);
                    return copyCount;
                }
                else
                {
                    int copyCount = value.Length - valueOffset;
                    if (copyCount > length)
                    {
                        copyCount = length;
                    }

                    for (int i = 0; i < copyCount; i++)
                    {
                        buffer[offset + i] = value[valueOffset + i];
                    }

                    return copyCount;
                }
            }

            internal int CopyToBinary(IncrementalReadDecoder decoder, int valueOffset)
            {
                if (value == null)
                {
                    Debug.Assert(valueStartPos != -1);
                    Debug.Assert(chars != null);
                    return decoder.Decode(chars, valueStartPos + valueOffset, valueLength - valueOffset);
                }
                else
                {
                    return decoder.Decode(value, valueOffset, value.Length - valueOffset);
                }
            }

            internal void AdjustLineInfo(int valueOffset, bool isNormalized, ref LineInfo lineInfo)
            {
                if (valueOffset == 0)
                {
                    return;
                }

                if (valueStartPos != -1)
                {
                    XmlTextReader.AdjustLineInfo(chars, valueStartPos, valueStartPos + valueOffset, isNormalized, ref lineInfo);
                }
                else
                {
                    char[] chars = value.Substring(0, valueOffset).ToCharArray();
                    XmlTextReader.AdjustLineInfo(chars, 0, chars.Length, isNormalized, ref lineInfo);
                }
            }

            // This should be inlined by JIT compiler
            internal string GetNameWPrefix(XmlNameTable nt)
            {
                if (nameWPrefix != null)
                {
                    return nameWPrefix;
                }
                else
                {
                    return CreateNameWPrefix(nt);
                }
            }

            // WsdModification - Adds support for namespace prefix

            // WsdModification - Adds support for namespaceUri

            internal string CreateNameWPrefix(XmlNameTable nt)
            {
                Debug.Assert(nameWPrefix == null);
                if (prefix.Length == 0)
                {
                    nameWPrefix = localName;
                }
                else
                {
                    nameWPrefix = nt.Add(string.Concat(prefix, ":", localName));
                }

                return nameWPrefix;
            }

            int IComparable.CompareTo(object obj)
            {
                NodeData other = obj as NodeData;
                if (other != null)
                {
                    if (Ref.Equal(localName, other.localName))
                    {
                        if (Ref.Equal(ns, other.ns))
                        {
                            return 0;
                        }
                        else
                        {
                            return string.Compare(ns, other.ns);
                        }
                    }
                    else
                    {
                        return string.Compare(localName, other.localName);
                    }
                }
                else
                {
                    Debug.Assert(false, "We should never get to this point.");
                    return 1;
                }
            }
        }
    }
}


