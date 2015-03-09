////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;
#if SCHEMA_VALIDATION
using System.Xml.Schema;
#endif //SCHEMA_VALIDATION
using System.Collections;
#if XML_SECURITY
using System.Security.Policy;
#endif
using Microsoft.SPOT;

namespace System.Xml
{

    internal class XmlWrappingReader : XmlReader, IXmlLineInfo 
    {
        //
        // Fields
        //
        protected XmlReader reader;
        protected IXmlLineInfo readerAsIXmlLineInfo;

        //
        // Constructor
        //
        internal XmlWrappingReader(XmlReader baseReader)
        {
            Debug.Assert(baseReader != null);
            Reader = baseReader;
        }

        //
        // XmlReader implementation
        //
        public override XmlReaderSettings Settings { get { return reader.Settings; } }
        public override XmlNodeType NodeType { get { return reader.NodeType; } }
        public override string Name { get { return reader.Name; } }
        public override string LocalName { get { return reader.LocalName; } }
        public override string NamespaceURI { get { return reader.NamespaceURI; } }
        public override string Prefix { get { return reader.Prefix; } }
        public override bool HasValue { get { return reader.HasValue; } }
        public override string Value { get { return reader.Value; } }
        public override int Depth { get { return reader.Depth; } }
        public override string BaseURI { get { return reader.BaseURI; } }
        public override bool IsEmptyElement { get { return reader.IsEmptyElement; } }
        public override bool IsDefault { get { return reader.IsDefault; } }
        public override char QuoteChar { get { return reader.QuoteChar; } }
        public override XmlSpace XmlSpace { get { return reader.XmlSpace; } }
        public override string XmlLang { get { return reader.XmlLang; } }
        public override System.Type ValueType { get { return reader.ValueType; } }
        public override int AttributeCount { get { return reader.AttributeCount; } }
        public override string this[int i] { get { return this[i]; } }
        public override string this[string name] { get { return this[name]; } }
        public override string this[string name, string namespaceURI] { get { return this[name, namespaceURI]; } }
        public override bool CanResolveEntity { get { return reader.CanResolveEntity; } }
        public override bool EOF { get { return reader.EOF; } }
        public override ReadState ReadState { get { return reader.ReadState; } }
        public override bool HasAttributes { get { return reader.HasAttributes; } }
        public override XmlNameTable NameTable { get { return reader.NameTable; } }

        public override string GetAttribute(string name)
        {
            return reader.GetAttribute(name);
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            return reader.GetAttribute(name, namespaceURI);
        }

        public override string GetAttribute(int i)
        {
            return reader.GetAttribute(i);
        }

        public override bool MoveToAttribute(string name)
        {
            return reader.MoveToAttribute(name);
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            return reader.MoveToAttribute(name, ns);
        }

        public override void MoveToAttribute(int i)
        {
            reader.MoveToAttribute(i);
        }

        public override bool MoveToFirstAttribute()
        {
            return reader.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            return reader.MoveToNextAttribute();
        }

        public override bool MoveToElement()
        {
            return reader.MoveToElement();
        }

        public override bool Read()
        {
            return reader.Read();
        }

        public override void Close()
        {
            reader.Close();
        }

        public override void Skip()
        {
            reader.Skip();
        }

        public override string LookupNamespace(string prefix)
        {
            return reader.LookupNamespace(prefix);
        }

        public override void ResolveEntity()
        {
            reader.ResolveEntity();
        }

        public override bool ReadAttributeValue()
        {
            return reader.ReadAttributeValue();
        }

        //
        // IDisposable interface
        //
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (reader != null)
                {
                    reader.Dispose();
                    reader = null;
                }
            }
        }

        //
        // IXmlLineInfo members
        //
        public virtual bool HasLineInfo()
        {
            return (readerAsIXmlLineInfo == null) ? false : readerAsIXmlLineInfo.HasLineInfo();
        }

        public virtual int LineNumber
        {
            get
            {
                return (readerAsIXmlLineInfo == null) ? 0 : readerAsIXmlLineInfo.LineNumber;
            }
        }

        public virtual int LinePosition
        {
            get
            {
                return (readerAsIXmlLineInfo == null) ? 0 : readerAsIXmlLineInfo.LinePosition;
            }
        }

        //
        //  Protected methods
        //
        protected XmlReader Reader
        {
            get
            {
                return reader;
            }

            set
            {
                reader = value;
                readerAsIXmlLineInfo = value as IXmlLineInfo;
            }
        }
    }
}


