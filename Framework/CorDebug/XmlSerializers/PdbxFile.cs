#pragma warning disable 0219

namespace Microsoft.SPOT.Debugger.Serialization.PdbxFile
{

    public class XmlSerializationWriterPdbxFile : System.Xml.Serialization.XmlSerializationWriter
    {

        public void Write12_PdbxFile(object o)
        {
            WriteStartDocument();
            if (o == null)
            {
                WriteNullTagLiteral(@"PdbxFile", @"");
                return;
            }
            TopLevelElement();
            Write11_PdbxFile(@"PdbxFile", @"", ((global::Microsoft.SPOT.Debugger.Pdbx.PdbxFile)o), true, false);
        }

        void Write11_PdbxFile(string n, string ns, global::Microsoft.SPOT.Debugger.Pdbx.PdbxFile o, bool isNullable, bool needType)
        {
            if ((object)o == null)
            {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType)
            {
                System.Type t = o.GetType();
                if (t == typeof(global::Microsoft.SPOT.Debugger.Pdbx.PdbxFile))
                {
                }
                else
                {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"PdbxFile", @"");
            Write10_Assembly(@"Assembly", @"", ((global::Microsoft.SPOT.Debugger.Pdbx.Assembly)o.@Assembly), false, false);
            WriteEndElement(o);
        }

        void Write10_Assembly(string n, string ns, global::Microsoft.SPOT.Debugger.Pdbx.Assembly o, bool isNullable, bool needType)
        {
            if ((object)o == null)
            {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType)
            {
                System.Type t = o.GetType();
                if (t == typeof(global::Microsoft.SPOT.Debugger.Pdbx.Assembly))
                {
                }
                else
                {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"Assembly", @"");
            WriteElementString(@"FileName", @"", ((global::System.String)o.@FileName));
            Write2_VersionStruct(@"Version", @"", ((global::Microsoft.SPOT.Debugger.Pdbx.Assembly.VersionStruct)o.@Version), false);
            Write4_Token(@"Token", @"", ((global::Microsoft.SPOT.Debugger.Pdbx.Token)o.@Token), false, false);
            {
                global::Microsoft.SPOT.Debugger.Pdbx.Class[] a = (global::Microsoft.SPOT.Debugger.Pdbx.Class[])((global::Microsoft.SPOT.Debugger.Pdbx.Class[])o.@Classes);
                if (a != null)
                {
                    WriteStartElement(@"Classes", @"", null, false);
                    for (int ia = 0; ia < a.Length; ia++)
                    {
                        Write9_Class(@"Class", @"", ((global::Microsoft.SPOT.Debugger.Pdbx.Class)a[ia]), true, false);
                    }
                    WriteEndElement();
                }
            }
            WriteEndElement(o);
        }

        void Write9_Class(string n, string ns, global::Microsoft.SPOT.Debugger.Pdbx.Class o, bool isNullable, bool needType)
        {
            if ((object)o == null)
            {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType)
            {
                System.Type t = o.GetType();
                if (t == typeof(global::Microsoft.SPOT.Debugger.Pdbx.Class))
                {
                }
                else
                {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"Class", @"");
            Write4_Token(@"Token", @"", ((global::Microsoft.SPOT.Debugger.Pdbx.Token)o.@Token), false, false);
            {
                global::Microsoft.SPOT.Debugger.Pdbx.Field[] a = (global::Microsoft.SPOT.Debugger.Pdbx.Field[])((global::Microsoft.SPOT.Debugger.Pdbx.Field[])o.@Fields);
                if (a != null)
                {
                    WriteStartElement(@"Fields", @"", null, false);
                    for (int ia = 0; ia < a.Length; ia++)
                    {
                        Write6_Field(@"Field", @"", ((global::Microsoft.SPOT.Debugger.Pdbx.Field)a[ia]), true, false);
                    }
                    WriteEndElement();
                }
            }
            {
                global::Microsoft.SPOT.Debugger.Pdbx.Method[] a = (global::Microsoft.SPOT.Debugger.Pdbx.Method[])((global::Microsoft.SPOT.Debugger.Pdbx.Method[])o.@Methods);
                if (a != null)
                {
                    WriteStartElement(@"Methods", @"", null, false);
                    for (int ia = 0; ia < a.Length; ia++)
                    {
                        Write8_Method(@"Method", @"", ((global::Microsoft.SPOT.Debugger.Pdbx.Method)a[ia]), true, false);
                    }
                    WriteEndElement();
                }
            }
            WriteEndElement(o);
        }

        void Write8_Method(string n, string ns, global::Microsoft.SPOT.Debugger.Pdbx.Method o, bool isNullable, bool needType)
        {
            if ((object)o == null)
            {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType)
            {
                System.Type t = o.GetType();
                if (t == typeof(global::Microsoft.SPOT.Debugger.Pdbx.Method))
                {
                }
                else
                {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"Method", @"");
            Write4_Token(@"Token", @"", ((global::Microsoft.SPOT.Debugger.Pdbx.Token)o.@Token), false, false);
            WriteElementStringRaw(@"HasByteCode", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@HasByteCode)));
            {
                global::Microsoft.SPOT.Debugger.Pdbx.IL[] a = (global::Microsoft.SPOT.Debugger.Pdbx.IL[])((global::Microsoft.SPOT.Debugger.Pdbx.IL[])o.@ILMap);
                if (a != null)
                {
                    WriteStartElement(@"ILMap", @"", null, false);
                    for (int ia = 0; ia < a.Length; ia++)
                    {
                        Write7_IL(@"IL", @"", ((global::Microsoft.SPOT.Debugger.Pdbx.IL)a[ia]), true, false);
                    }
                    WriteEndElement();
                }
            }
            WriteEndElement(o);
        }

        void Write7_IL(string n, string ns, global::Microsoft.SPOT.Debugger.Pdbx.IL o, bool isNullable, bool needType)
        {
            if ((object)o == null)
            {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType)
            {
                System.Type t = o.GetType();
                if (t == typeof(global::Microsoft.SPOT.Debugger.Pdbx.IL))
                {
                }
                else
                {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"IL", @"");
            WriteElementString(@"CLR", @"", ((global::System.String)o.@CLR_String));
            WriteElementString(@"TinyCLR", @"", ((global::System.String)o.@TinyCLR_String));
            WriteEndElement(o);
        }

        void Write4_Token(string n, string ns, global::Microsoft.SPOT.Debugger.Pdbx.Token o, bool isNullable, bool needType)
        {
            if ((object)o == null)
            {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType)
            {
                System.Type t = o.GetType();
                if (t == typeof(global::Microsoft.SPOT.Debugger.Pdbx.Token))
                {
                }
                else
                {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"Token", @"");
            WriteElementString(@"CLR", @"", ((global::System.String)o.@CLR_String));
            WriteElementString(@"TinyCLR", @"", ((global::System.String)o.@TinyCLR_String));
            WriteEndElement(o);
        }

        void Write6_Field(string n, string ns, global::Microsoft.SPOT.Debugger.Pdbx.Field o, bool isNullable, bool needType)
        {
            if ((object)o == null)
            {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType)
            {
                System.Type t = o.GetType();
                if (t == typeof(global::Microsoft.SPOT.Debugger.Pdbx.Field))
                {
                }
                else
                {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"Field", @"");
            Write4_Token(@"Token", @"", ((global::Microsoft.SPOT.Debugger.Pdbx.Token)o.@Token), false, false);
            WriteEndElement(o);
        }

        void Write2_VersionStruct(string n, string ns, global::Microsoft.SPOT.Debugger.Pdbx.Assembly.VersionStruct o, bool needType)
        {
            if (!needType)
            {
                System.Type t = o.GetType();
                if (t == typeof(global::Microsoft.SPOT.Debugger.Pdbx.Assembly.VersionStruct))
                {
                }
                else
                {
                    throw CreateUnknownTypeException(o);
                }
            }
            WriteStartElement(n, ns, o, false, null);
            if (needType) WriteXsiType(@"VersionStruct", @"");
            WriteElementStringRaw(@"Major", @"", System.Xml.XmlConvert.ToString((global::System.UInt16)((global::System.UInt16)o.@Major)));
            WriteElementStringRaw(@"Minor", @"", System.Xml.XmlConvert.ToString((global::System.UInt16)((global::System.UInt16)o.@Minor)));
            WriteElementStringRaw(@"Build", @"", System.Xml.XmlConvert.ToString((global::System.UInt16)((global::System.UInt16)o.@Build)));
            WriteElementStringRaw(@"Revision", @"", System.Xml.XmlConvert.ToString((global::System.UInt16)((global::System.UInt16)o.@Revision)));
            WriteEndElement(o);
        }

        protected override void InitCallbacks()
        {
        }
    }

    public class XmlSerializationReaderPdbxFile : System.Xml.Serialization.XmlSerializationReader
    {

        public object Read12_PdbxFile()
        {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element)
            {
                if (((object)Reader.LocalName == (object)id1_PdbxFile && (object)Reader.NamespaceURI == (object)id2_Item))
                {
                    o = Read11_PdbxFile(true, true);
                }
                else
                {
                    throw CreateUnknownNodeException();
                }
            }
            else
            {
                UnknownNode(null, @":PdbxFile");
            }
            return (object)o;
        }

        global::Microsoft.SPOT.Debugger.Pdbx.PdbxFile Read11_PdbxFile(bool isNullable, bool checkType)
        {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType)
            {
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)id1_PdbxFile && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item))
                {
                }
                else
                    throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::Microsoft.SPOT.Debugger.Pdbx.PdbxFile o;
            o = new global::Microsoft.SPOT.Debugger.Pdbx.PdbxFile();
            bool[] paramsRead = new bool[1];
            while (Reader.MoveToNextAttribute())
            {
                if (!IsXmlnsAttribute(Reader.Name))
                {
                    UnknownNode((object)o);
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement)
            {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations0 = 0;
            int readerCount0 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None)
            {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (!paramsRead[0] && ((object)Reader.LocalName == (object)id3_Assembly && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        o.@Assembly = Read10_Assembly(false, true);
                        paramsRead[0] = true;
                    }
                    else
                    {
                        UnknownNode((object)o, @":Assembly");
                    }
                }
                else
                {
                    UnknownNode((object)o, @":Assembly");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations0, ref readerCount0);
            }
            ReadEndElement();
            return o;
        }

        global::Microsoft.SPOT.Debugger.Pdbx.Assembly Read10_Assembly(bool isNullable, bool checkType)
        {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType)
            {
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)id3_Assembly && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item))
                {
                }
                else
                    throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::Microsoft.SPOT.Debugger.Pdbx.Assembly o;
            o = new global::Microsoft.SPOT.Debugger.Pdbx.Assembly();
            global::Microsoft.SPOT.Debugger.Pdbx.Class[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[4];
            while (Reader.MoveToNextAttribute())
            {
                if (!IsXmlnsAttribute(Reader.Name))
                {
                    UnknownNode((object)o);
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement)
            {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations1 = 0;
            int readerCount1 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None)
            {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (!paramsRead[0] && ((object)Reader.LocalName == (object)id4_FileName && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        {
                            o.@FileName = Reader.ReadElementString();
                        }
                        paramsRead[0] = true;
                    }
                    else if (!paramsRead[1] && ((object)Reader.LocalName == (object)id5_Version && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        o.@Version = Read2_VersionStruct(true);
                        paramsRead[1] = true;
                    }
                    else if (!paramsRead[2] && ((object)Reader.LocalName == (object)id6_Token && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        o.@Token = Read4_Token(false, true);
                        paramsRead[2] = true;
                    }
                    else if (((object)Reader.LocalName == (object)id7_Classes && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        if (!ReadNull())
                        {
                            global::Microsoft.SPOT.Debugger.Pdbx.Class[] a_3_0 = null;
                            int ca_3_0 = 0;
                            if ((Reader.IsEmptyElement))
                            {
                                Reader.Skip();
                            }
                            else
                            {
                                Reader.ReadStartElement();
                                Reader.MoveToContent();
                                int whileIterations2 = 0;
                                int readerCount2 = ReaderCount;
                                while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None)
                                {
                                    if (Reader.NodeType == System.Xml.XmlNodeType.Element)
                                    {
                                        if (((object)Reader.LocalName == (object)id8_Class && (object)Reader.NamespaceURI == (object)id2_Item))
                                        {
                                            a_3_0 = (global::Microsoft.SPOT.Debugger.Pdbx.Class[])EnsureArrayIndex(a_3_0, ca_3_0, typeof(global::Microsoft.SPOT.Debugger.Pdbx.Class)); a_3_0[ca_3_0++] = Read9_Class(true, true);
                                        }
                                        else
                                        {
                                            UnknownNode(null, @":Class");
                                        }
                                    }
                                    else
                                    {
                                        UnknownNode(null, @":Class");
                                    }
                                    Reader.MoveToContent();
                                    CheckReaderCount(ref whileIterations2, ref readerCount2);
                                }
                                ReadEndElement();
                            }
                            o.@Classes = (global::Microsoft.SPOT.Debugger.Pdbx.Class[])ShrinkArray(a_3_0, ca_3_0, typeof(global::Microsoft.SPOT.Debugger.Pdbx.Class), false);
                        }
                    }
                    else
                    {
                        UnknownNode((object)o, @":FileName, :Version, :Token, :Classes");
                    }
                }
                else
                {
                    UnknownNode((object)o, @":FileName, :Version, :Token, :Classes");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations1, ref readerCount1);
            }
            ReadEndElement();
            return o;
        }

        global::Microsoft.SPOT.Debugger.Pdbx.Class Read9_Class(bool isNullable, bool checkType)
        {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType)
            {
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)id8_Class && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item))
                {
                }
                else
                    throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::Microsoft.SPOT.Debugger.Pdbx.Class o;
            o = new global::Microsoft.SPOT.Debugger.Pdbx.Class();
            global::Microsoft.SPOT.Debugger.Pdbx.Field[] a_1 = null;
            int ca_1 = 0;
            global::Microsoft.SPOT.Debugger.Pdbx.Method[] a_2 = null;
            int ca_2 = 0;
            bool[] paramsRead = new bool[3];
            while (Reader.MoveToNextAttribute())
            {
                if (!IsXmlnsAttribute(Reader.Name))
                {
                    UnknownNode((object)o);
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement)
            {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations3 = 0;
            int readerCount3 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None)
            {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (!paramsRead[0] && ((object)Reader.LocalName == (object)id6_Token && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        o.@Token = Read4_Token(false, true);
                        paramsRead[0] = true;
                    }
                    else if (((object)Reader.LocalName == (object)id9_Fields && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        if (!ReadNull())
                        {
                            global::Microsoft.SPOT.Debugger.Pdbx.Field[] a_1_0 = null;
                            int ca_1_0 = 0;
                            if ((Reader.IsEmptyElement))
                            {
                                Reader.Skip();
                            }
                            else
                            {
                                Reader.ReadStartElement();
                                Reader.MoveToContent();
                                int whileIterations4 = 0;
                                int readerCount4 = ReaderCount;
                                while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None)
                                {
                                    if (Reader.NodeType == System.Xml.XmlNodeType.Element)
                                    {
                                        if (((object)Reader.LocalName == (object)id10_Field && (object)Reader.NamespaceURI == (object)id2_Item))
                                        {
                                            a_1_0 = (global::Microsoft.SPOT.Debugger.Pdbx.Field[])EnsureArrayIndex(a_1_0, ca_1_0, typeof(global::Microsoft.SPOT.Debugger.Pdbx.Field)); a_1_0[ca_1_0++] = Read6_Field(true, true);
                                        }
                                        else
                                        {
                                            UnknownNode(null, @":Field");
                                        }
                                    }
                                    else
                                    {
                                        UnknownNode(null, @":Field");
                                    }
                                    Reader.MoveToContent();
                                    CheckReaderCount(ref whileIterations4, ref readerCount4);
                                }
                                ReadEndElement();
                            }
                            o.@Fields = (global::Microsoft.SPOT.Debugger.Pdbx.Field[])ShrinkArray(a_1_0, ca_1_0, typeof(global::Microsoft.SPOT.Debugger.Pdbx.Field), false);
                        }
                    }
                    else if (((object)Reader.LocalName == (object)id11_Methods && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        if (!ReadNull())
                        {
                            global::Microsoft.SPOT.Debugger.Pdbx.Method[] a_2_0 = null;
                            int ca_2_0 = 0;
                            if ((Reader.IsEmptyElement))
                            {
                                Reader.Skip();
                            }
                            else
                            {
                                Reader.ReadStartElement();
                                Reader.MoveToContent();
                                int whileIterations5 = 0;
                                int readerCount5 = ReaderCount;
                                while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None)
                                {
                                    if (Reader.NodeType == System.Xml.XmlNodeType.Element)
                                    {
                                        if (((object)Reader.LocalName == (object)id12_Method && (object)Reader.NamespaceURI == (object)id2_Item))
                                        {
                                            a_2_0 = (global::Microsoft.SPOT.Debugger.Pdbx.Method[])EnsureArrayIndex(a_2_0, ca_2_0, typeof(global::Microsoft.SPOT.Debugger.Pdbx.Method)); a_2_0[ca_2_0++] = Read8_Method(true, true);
                                        }
                                        else
                                        {
                                            UnknownNode(null, @":Method");
                                        }
                                    }
                                    else
                                    {
                                        UnknownNode(null, @":Method");
                                    }
                                    Reader.MoveToContent();
                                    CheckReaderCount(ref whileIterations5, ref readerCount5);
                                }
                                ReadEndElement();
                            }
                            o.@Methods = (global::Microsoft.SPOT.Debugger.Pdbx.Method[])ShrinkArray(a_2_0, ca_2_0, typeof(global::Microsoft.SPOT.Debugger.Pdbx.Method), false);
                        }
                    }
                    else
                    {
                        UnknownNode((object)o, @":Token, :Fields, :Methods");
                    }
                }
                else
                {
                    UnknownNode((object)o, @":Token, :Fields, :Methods");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations3, ref readerCount3);
            }
            ReadEndElement();
            return o;
        }

        global::Microsoft.SPOT.Debugger.Pdbx.Method Read8_Method(bool isNullable, bool checkType)
        {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType)
            {
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)id12_Method && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item))
                {
                }
                else
                    throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::Microsoft.SPOT.Debugger.Pdbx.Method o;
            o = new global::Microsoft.SPOT.Debugger.Pdbx.Method();
            global::Microsoft.SPOT.Debugger.Pdbx.IL[] a_2 = null;
            int ca_2 = 0;
            bool[] paramsRead = new bool[3];
            while (Reader.MoveToNextAttribute())
            {
                if (!IsXmlnsAttribute(Reader.Name))
                {
                    UnknownNode((object)o);
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement)
            {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations6 = 0;
            int readerCount6 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None)
            {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (!paramsRead[0] && ((object)Reader.LocalName == (object)id6_Token && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        o.@Token = Read4_Token(false, true);
                        paramsRead[0] = true;
                    }
                    else if (!paramsRead[1] && ((object)Reader.LocalName == (object)id13_HasByteCode && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        {
                            o.@HasByteCode = System.Xml.XmlConvert.ToBoolean(Reader.ReadElementString());
                        }
                        paramsRead[1] = true;
                    }
                    else if (((object)Reader.LocalName == (object)id14_ILMap && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        if (!ReadNull())
                        {
                            global::Microsoft.SPOT.Debugger.Pdbx.IL[] a_2_0 = null;
                            int ca_2_0 = 0;
                            if ((Reader.IsEmptyElement))
                            {
                                Reader.Skip();
                            }
                            else
                            {
                                Reader.ReadStartElement();
                                Reader.MoveToContent();
                                int whileIterations7 = 0;
                                int readerCount7 = ReaderCount;
                                while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None)
                                {
                                    if (Reader.NodeType == System.Xml.XmlNodeType.Element)
                                    {
                                        if (((object)Reader.LocalName == (object)id15_IL && (object)Reader.NamespaceURI == (object)id2_Item))
                                        {
                                            a_2_0 = (global::Microsoft.SPOT.Debugger.Pdbx.IL[])EnsureArrayIndex(a_2_0, ca_2_0, typeof(global::Microsoft.SPOT.Debugger.Pdbx.IL)); a_2_0[ca_2_0++] = Read7_IL(true, true);
                                        }
                                        else
                                        {
                                            UnknownNode(null, @":IL");
                                        }
                                    }
                                    else
                                    {
                                        UnknownNode(null, @":IL");
                                    }
                                    Reader.MoveToContent();
                                    CheckReaderCount(ref whileIterations7, ref readerCount7);
                                }
                                ReadEndElement();
                            }
                            o.@ILMap = (global::Microsoft.SPOT.Debugger.Pdbx.IL[])ShrinkArray(a_2_0, ca_2_0, typeof(global::Microsoft.SPOT.Debugger.Pdbx.IL), false);
                        }
                    }
                    else
                    {
                        UnknownNode((object)o, @":Token, :HasByteCode, :ILMap");
                    }
                }
                else
                {
                    UnknownNode((object)o, @":Token, :HasByteCode, :ILMap");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations6, ref readerCount6);
            }
            ReadEndElement();
            return o;
        }

        global::Microsoft.SPOT.Debugger.Pdbx.IL Read7_IL(bool isNullable, bool checkType)
        {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType)
            {
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)id15_IL && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item))
                {
                }
                else
                    throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::Microsoft.SPOT.Debugger.Pdbx.IL o;
            o = new global::Microsoft.SPOT.Debugger.Pdbx.IL();
            bool[] paramsRead = new bool[2];
            while (Reader.MoveToNextAttribute())
            {
                if (!IsXmlnsAttribute(Reader.Name))
                {
                    UnknownNode((object)o);
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement)
            {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations8 = 0;
            int readerCount8 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None)
            {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (!paramsRead[0] && ((object)Reader.LocalName == (object)id16_CLR && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        {
                            o.@CLR_String = Reader.ReadElementString();
                        }
                        paramsRead[0] = true;
                    }
                    else if (!paramsRead[1] && ((object)Reader.LocalName == (object)id17_TinyCLR && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        {
                            o.@TinyCLR_String = Reader.ReadElementString();
                        }
                        paramsRead[1] = true;
                    }
                    else
                    {
                        UnknownNode((object)o, @":CLR, :TinyCLR");
                    }
                }
                else
                {
                    UnknownNode((object)o, @":CLR, :TinyCLR");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations8, ref readerCount8);
            }
            ReadEndElement();
            return o;
        }

        global::Microsoft.SPOT.Debugger.Pdbx.Token Read4_Token(bool isNullable, bool checkType)
        {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType)
            {
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)id6_Token && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item))
                {
                }
                else
                    throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::Microsoft.SPOT.Debugger.Pdbx.Token o;
            o = new global::Microsoft.SPOT.Debugger.Pdbx.Token();
            bool[] paramsRead = new bool[2];
            while (Reader.MoveToNextAttribute())
            {
                if (!IsXmlnsAttribute(Reader.Name))
                {
                    UnknownNode((object)o);
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement)
            {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations9 = 0;
            int readerCount9 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None)
            {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (!paramsRead[0] && ((object)Reader.LocalName == (object)id16_CLR && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        {
                            o.@CLR_String = Reader.ReadElementString();
                        }
                        paramsRead[0] = true;
                    }
                    else if (!paramsRead[1] && ((object)Reader.LocalName == (object)id17_TinyCLR && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        {
                            o.@TinyCLR_String = Reader.ReadElementString();
                        }
                        paramsRead[1] = true;
                    }
                    else
                    {
                        UnknownNode((object)o, @":CLR, :TinyCLR");
                    }
                }
                else
                {
                    UnknownNode((object)o, @":CLR, :TinyCLR");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations9, ref readerCount9);
            }
            ReadEndElement();
            return o;
        }

        global::Microsoft.SPOT.Debugger.Pdbx.Field Read6_Field(bool isNullable, bool checkType)
        {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType)
            {
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)id10_Field && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item))
                {
                }
                else
                    throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            global::Microsoft.SPOT.Debugger.Pdbx.Field o;
            o = new global::Microsoft.SPOT.Debugger.Pdbx.Field();
            bool[] paramsRead = new bool[1];
            while (Reader.MoveToNextAttribute())
            {
                if (!IsXmlnsAttribute(Reader.Name))
                {
                    UnknownNode((object)o);
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement)
            {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations10 = 0;
            int readerCount10 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None)
            {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (!paramsRead[0] && ((object)Reader.LocalName == (object)id6_Token && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        o.@Token = Read4_Token(false, true);
                        paramsRead[0] = true;
                    }
                    else
                    {
                        UnknownNode((object)o, @":Token");
                    }
                }
                else
                {
                    UnknownNode((object)o, @":Token");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations10, ref readerCount10);
            }
            ReadEndElement();
            return o;
        }

        global::Microsoft.SPOT.Debugger.Pdbx.Assembly.VersionStruct Read2_VersionStruct(bool checkType)
        {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (checkType)
            {
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)id18_VersionStruct && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item))
                {
                }
                else
                    throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            global::Microsoft.SPOT.Debugger.Pdbx.Assembly.VersionStruct o;
            try
            {
                o = (global::Microsoft.SPOT.Debugger.Pdbx.Assembly.VersionStruct)System.Activator.CreateInstance(typeof(global::Microsoft.SPOT.Debugger.Pdbx.Assembly.VersionStruct), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.CreateInstance | System.Reflection.BindingFlags.NonPublic, null, new object[0], null);
            }
            catch (System.MissingMethodException)
            {
                throw CreateInaccessibleConstructorException(@"global::Microsoft.SPOT.Debugger.Pdbx.Assembly.VersionStruct");
            }
            catch (System.Security.SecurityException)
            {
                throw CreateCtorHasSecurityException(@"global::Microsoft.SPOT.Debugger.Pdbx.Assembly.VersionStruct");
            }
            bool[] paramsRead = new bool[4];
            while (Reader.MoveToNextAttribute())
            {
                if (!IsXmlnsAttribute(Reader.Name))
                {
                    UnknownNode((object)o);
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement)
            {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations11 = 0;
            int readerCount11 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None)
            {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (!paramsRead[0] && ((object)Reader.LocalName == (object)id19_Major && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        {
                            o.@Major = System.Xml.XmlConvert.ToUInt16(Reader.ReadElementString());
                        }
                        paramsRead[0] = true;
                    }
                    else if (!paramsRead[1] && ((object)Reader.LocalName == (object)id20_Minor && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        {
                            o.@Minor = System.Xml.XmlConvert.ToUInt16(Reader.ReadElementString());
                        }
                        paramsRead[1] = true;
                    }
                    else if (!paramsRead[2] && ((object)Reader.LocalName == (object)id21_Build && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        {
                            o.@Build = System.Xml.XmlConvert.ToUInt16(Reader.ReadElementString());
                        }
                        paramsRead[2] = true;
                    }
                    else if (!paramsRead[3] && ((object)Reader.LocalName == (object)id22_Revision && (object)Reader.NamespaceURI == (object)id2_Item))
                    {
                        {
                            o.@Revision = System.Xml.XmlConvert.ToUInt16(Reader.ReadElementString());
                        }
                        paramsRead[3] = true;
                    }
                    else
                    {
                        UnknownNode((object)o, @":Major, :Minor, :Build, :Revision");
                    }
                }
                else
                {
                    UnknownNode((object)o, @":Major, :Minor, :Build, :Revision");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations11, ref readerCount11);
            }
            ReadEndElement();
            return o;
        }

        protected override void InitCallbacks()
        {
        }

        string id17_TinyCLR;
        string id9_Fields;
        string id13_HasByteCode;
        string id18_VersionStruct;
        string id14_ILMap;
        string id5_Version;
        string id22_Revision;
        string id10_Field;
        string id8_Class;
        string id19_Major;
        string id12_Method;
        string id6_Token;
        string id2_Item;
        string id20_Minor;
        string id1_PdbxFile;
        string id15_IL;
        string id21_Build;
        string id7_Classes;
        string id3_Assembly;
        string id16_CLR;
        string id11_Methods;
        string id4_FileName;

        protected override void InitIDs()
        {
            id17_TinyCLR = Reader.NameTable.Add(@"TinyCLR");
            id9_Fields = Reader.NameTable.Add(@"Fields");
            id13_HasByteCode = Reader.NameTable.Add(@"HasByteCode");
            id18_VersionStruct = Reader.NameTable.Add(@"VersionStruct");
            id14_ILMap = Reader.NameTable.Add(@"ILMap");
            id5_Version = Reader.NameTable.Add(@"Version");
            id22_Revision = Reader.NameTable.Add(@"Revision");
            id10_Field = Reader.NameTable.Add(@"Field");
            id8_Class = Reader.NameTable.Add(@"Class");
            id19_Major = Reader.NameTable.Add(@"Major");
            id12_Method = Reader.NameTable.Add(@"Method");
            id6_Token = Reader.NameTable.Add(@"Token");
            id2_Item = Reader.NameTable.Add(@"");
            id20_Minor = Reader.NameTable.Add(@"Minor");
            id1_PdbxFile = Reader.NameTable.Add(@"PdbxFile");
            id15_IL = Reader.NameTable.Add(@"IL");
            id21_Build = Reader.NameTable.Add(@"Build");
            id7_Classes = Reader.NameTable.Add(@"Classes");
            id3_Assembly = Reader.NameTable.Add(@"Assembly");
            id16_CLR = Reader.NameTable.Add(@"CLR");
            id11_Methods = Reader.NameTable.Add(@"Methods");
            id4_FileName = Reader.NameTable.Add(@"FileName");
        }
    }

    public abstract class XmlSerializer1 : System.Xml.Serialization.XmlSerializer
    {
        protected override System.Xml.Serialization.XmlSerializationReader CreateReader()
        {
            return new XmlSerializationReaderPdbxFile();
        }
        protected override System.Xml.Serialization.XmlSerializationWriter CreateWriter()
        {
            return new XmlSerializationWriterPdbxFile();
        }
    }

    public sealed class PdbxFileSerializer : XmlSerializer1
    {

        public override System.Boolean CanDeserialize(System.Xml.XmlReader xmlReader)
        {
            return xmlReader.IsStartElement(@"PdbxFile", @"");
        }

        protected override void Serialize(object o, System.Xml.Serialization.XmlSerializationWriter writer)
        {
            ((XmlSerializationWriterPdbxFile)writer).Write12_PdbxFile(o);
        }

        protected override object Deserialize(System.Xml.Serialization.XmlSerializationReader reader)
        {
            return ((XmlSerializationReaderPdbxFile)reader).Read12_PdbxFile();
        }
    }

    public class XmlSerializerContract : global::System.Xml.Serialization.XmlSerializerImplementation
    {
        public override global::System.Xml.Serialization.XmlSerializationReader Reader { get { return new XmlSerializationReaderPdbxFile(); } }
        public override global::System.Xml.Serialization.XmlSerializationWriter Writer { get { return new XmlSerializationWriterPdbxFile(); } }
        System.Collections.Hashtable readMethods = null;
        public override System.Collections.Hashtable ReadMethods
        {
            get
            {
                if (readMethods == null)
                {
                    System.Collections.Hashtable _tmp = new System.Collections.Hashtable();
                    _tmp[@"Microsoft.SPOT.Debugger.Pdbx+PdbxFile::"] = @"Read12_PdbxFile";
                    if (readMethods == null) readMethods = _tmp;
                }
                return readMethods;
            }
        }
        System.Collections.Hashtable writeMethods = null;
        public override System.Collections.Hashtable WriteMethods
        {
            get
            {
                if (writeMethods == null)
                {
                    System.Collections.Hashtable _tmp = new System.Collections.Hashtable();
                    _tmp[@"Microsoft.SPOT.Debugger.Pdbx+PdbxFile::"] = @"Write12_PdbxFile";
                    if (writeMethods == null) writeMethods = _tmp;
                }
                return writeMethods;
            }
        }
        System.Collections.Hashtable typedSerializers = null;
        public override System.Collections.Hashtable TypedSerializers
        {
            get
            {
                if (typedSerializers == null)
                {
                    System.Collections.Hashtable _tmp = new System.Collections.Hashtable();
                    _tmp.Add(@"Microsoft.SPOT.Debugger.Pdbx+PdbxFile::", new PdbxFileSerializer());
                    if (typedSerializers == null) typedSerializers = _tmp;
                }
                return typedSerializers;
            }
        }
        public override System.Boolean CanSerialize(System.Type type)
        {
            if (type == typeof(global::Microsoft.SPOT.Debugger.Pdbx.PdbxFile)) return true;
            return false;
        }
        public override System.Xml.Serialization.XmlSerializer GetSerializer(System.Type type)
        {
            if (type == typeof(global::Microsoft.SPOT.Debugger.Pdbx.PdbxFile)) return new PdbxFileSerializer();
            return null;
        }
    }
}

#pragma warning restore 0219 
