////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections;
using System.Text;
using System.Xml;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class RandomXmlTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for XmlNameTable tests.");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        [TestMethod]
        public MFTestResults RandomXmlTest()
        {
            Random random = new Random();
            int seed = 0;
            int readCount = 0;
            DateTime startTime = DateTime.Now;
            TimeSpan oneMinute = new TimeSpan(0, 1, 0);

            try
            {
                // Run through at least 1 minute of random XML.
                do
                {
                    seed = random.Next();

                    RandomXmlStream xmlStream = new RandomXmlStream(seed);

                    Log.Comment("RandomXmlStream, seed = " + seed.ToString());

                    XmlReader reader = XmlReader.Create(xmlStream);

                    readCount = 0;

                    while (reader.Read())
                    {
                        readCount++;
                    }
                    reader.Close();
                }
                while ((DateTime.Now - startTime) < oneMinute);
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception during random XML Test with seed " + seed.ToString() + ", after " + readCount.ToString() + " Read() calls: \r\n" + e);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }
    }

    public class FifoBuffer
    {
        byte[] m_buffer;
        int m_offset;
        int m_count;

        public FifoBuffer()
        {
            m_buffer = new byte[1024];
            m_offset = 0;
            m_count = 0;
        }

        public int Read(byte[] buf, int offset, int count)
        {
            int countRequested = count;

            int len = m_buffer.Length;

            while (m_count > 0 && count > 0)
            {
                int avail = m_count; if (avail + m_offset > len) avail = len - m_offset;

                if (avail > count) avail = count;

                Array.Copy(m_buffer, m_offset, buf, offset, avail);

                m_offset += avail; if (m_offset == len) m_offset = 0;
                offset += avail;

                m_count -= avail;
                count -= avail;
            }

            if (m_count == 0)
            {
                //
                // No pending data, resync to the beginning of the buffer.
                //
                m_offset = 0;
            }

            return countRequested - count;
        }

        public void Write(byte[] buf)
        {
            int count = buf.Length;
            int offset = 0;

            while (count > 0)
            {
                int len = m_buffer.Length;
                int avail = len - m_count;

                if (avail == 0) // Buffer full. Expand it.
                {
                    byte[] buffer = new byte[len * 2];

                    //
                    // Double the buffer and copy all the data to the left side.
                    //
                    Array.Copy(m_buffer, m_offset, buffer, 0, len - m_offset);
                    Array.Copy(m_buffer, 0, buffer, len - m_offset, m_offset);

                    m_buffer = buffer;
                    m_offset = 0;
                    len *= 2;
                    avail = len;
                }

                int offsetWrite = m_offset + m_count; if (offsetWrite >= len) offsetWrite -= len;

                if (avail + offsetWrite > len) avail = len - offsetWrite;

                if (avail > count) avail = count;

                Array.Copy(buf, offset, m_buffer, offsetWrite, avail);

                offset += avail;
                m_count += avail;
                count -= avail;
            }
        }

        public int Available
        {
            get
            {
                return m_count;
            }
        }
    }

    public class RandomXmlStream : System.IO.Stream
    {
        private enum XmlStage
        {
            BeforeMainDocument,
            InMainDocument,
            AfterMainDocument
        }

        private Stack _names;
        private FifoBuffer _buffer;
        private bool _isDone;
        private Random _rand;
        private int _nodeCount;
        private XmlStage _stage;

        public RandomXmlStream(int seed)
        {
            _names = new Stack();
            _buffer = new FifoBuffer();
            _isDone = false;
            _rand = new Random(seed);

            if (GetRandomBool())
            {
                BuildXmlDeclaration();
            }

            _stage = XmlStage.BeforeMainDocument;
            _nodeCount = GetRandomExp(6);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_isDone)
            {
                return 0;
            }

            if (_buffer.Available >= count)
            {
                return _buffer.Read(buffer, offset, count);
            }

            while (true)
            {
                BuildXmlParts();

                if (_isDone || _buffer.Available >= count)
                {
                    return _buffer.Read(buffer, offset, count);
                }
            }
        }

        private int GetRandom(int mod)
        {
            return _rand.Next(mod);
        }

        private int GetRandomExp(int mod)
        {
            return _rand.Next(1 << _rand.Next(mod));
        }

        private bool GetRandomBool()
        {
            return _rand.Next(2) == 0;
        }

        private void BuildXmlParts()
        {
            switch (_stage)
            {
                case XmlStage.BeforeMainDocument:
                    if (_nodeCount == 0)
                    {
                        _stage = XmlStage.InMainDocument;
                        _nodeCount = GetRandomExp(16);

                        BuildStartElement(_nodeCount == 0);

                        return;
                    }

                    _nodeCount--;

                    switch (GetRandom(3))
                    {
                        case 0: // Comment
                            BuildComment();
                            break;

                        case 1: // PI
                            BuildPI();
                            break;

                        case 2: // Space
                            BuildSpace();
                            break;
                    }

                    break;

                case XmlStage.InMainDocument:
                    if (_nodeCount == 0)
                    {
                        if (_names.Count > 0)
                        {
                            BuildEndElement();

                            return;
                        }

                        _stage = XmlStage.AfterMainDocument;
                        _nodeCount = GetRandomExp(6);
                        goto case XmlStage.AfterMainDocument;
                    }

                    _nodeCount--;

                    while (true)
                    {
                        switch (GetRandom(7))
                        {
                            case 0: // Comment
                                BuildComment();
                                return;

                            case 1: // PI
                                BuildPI();
                                return;

                            case 2: // Space
                                BuildSpace();
                                return;

                            case 3: // CDATA
                                BuildCDATA();
                                return;

                            case 4: // Text
                                BuildText();
                                return;

                            case 5: // StartElement
                                BuildStartElement(true);
                                return;

                            case 6: // EndElement
                                if (_names.Count > 1)
                                {
                                    BuildEndElement();
                                    return;
                                }
                                break;
                        }
                    }

                case XmlStage.AfterMainDocument:

                    if (_nodeCount == 0)
                    {
                        _isDone = true;
                        return;
                    }

                    _nodeCount--;

                    switch (GetRandom(3))
                    {
                        case 0: // Comment
                            BuildComment();
                            break;

                        case 1: // PI
                            BuildPI();
                            break;

                        case 2: // Space
                            BuildSpace();
                            break;
                    }

                    break;
            }
        }

        private void BuildXmlDeclaration()
        {
            String xmlDecl = "";
            switch (GetRandom(3))
            {
                case 0:
                    // no xml decl
                    return;
                case 1:
                    xmlDecl = "<?xml version=\"1.0\" encoding='UTF-8' ?>";
                    break;
                case 2:
                    xmlDecl = "<?xml version='1.0'?>";
                    break;
                case 3:
                    xmlDecl = "<?xml version='1.0' encoding='utf-8' standalone='yes'?>";
                    break;
            }

            _buffer.Write(Encoding.UTF8.GetBytes(xmlDecl));
        }

        private void BuildEndElement()
        {
            _buffer.Write(Encoding.UTF8.GetBytes("</" + (String)_names.Pop() + ">"));
        }

        static String[] CharRefs = { "&gt;", "&lt;", "&amp;", "&apos;", "&quot;", "&#99;", "&#x43;" };

        private void BuildText()
        {
            byte[] buffer;
            int bufferLen;
            for (int i = GetRandomExp(5); i >= 0; i--)
            {
                switch(GetRandom(3))
                {
                    case 0:
                        bufferLen = GetRandomExp(8);
                        buffer = new byte[bufferLen];

                        for (bufferLen--; bufferLen >= 0; bufferLen--)
                        {
                            buffer[bufferLen] = (byte)'t';
                        }
                        _buffer.Write(buffer);
                        break;
                    case 1:
                        _buffer.Write(new byte[] { (byte)'\r', (byte)'\n' });
                        break;
                    case 2:
                        _buffer.Write(Encoding.UTF8.GetBytes(CharRefs[GetRandom(7)]));
                        break;
                }
            }
        }

        private void BuildCDATA()
        {
            int bufferLen = GetRandomExp(9);
            byte[] buffer = new byte[bufferLen];

            for (bufferLen--; bufferLen >= 0; bufferLen--)
            {
                buffer[bufferLen] = (byte)'C';
            }

            _buffer.Write(Encoding.UTF8.GetBytes("<![CDATA[<&<"));
            _buffer.Write(buffer);
            _buffer.Write(Encoding.UTF8.GetBytes("]]>"));
        }

        private void BuildStartElement(bool emptyOK)
        {
            _buffer.Write(new byte[] { (byte)'<' });

            BuildNCName(true);

            BuildAttributes();

            if (emptyOK && GetRandomBool())
            {
                _names.Pop();
                _buffer.Write(new byte[] { (byte)'/', (byte)'>' });
            }
            else
            {
                _buffer.Write(new byte[] { (byte)'>' });
            }
        }

        static byte[] AttributeValue = new byte[] { (byte)'h', (byte)'t', (byte)'t', (byte)'p', (byte)':', (byte)'/', (byte)'/', 
            (byte)'t', (byte)'e', (byte)'m', (byte)'p', (byte)'u', (byte)'r', (byte)'i', (byte)'.', (byte)'o', (byte)'r', (byte)'g' };

        private void BuildAttributes()
        {
            int numAttributes = GetRandomExp(3);

            for (int i = 0; i < numAttributes; i++)
            {
                BuildSpace();

                BuildNCName(false);

                _buffer.Write(Encoding.UTF8.GetBytes(i.ToString()));
                _buffer.Write(new byte[] { (byte)'=' });

                byte[] quoteChar = new byte[] { (GetRandomBool()) ? (byte)'\'' : (byte)'"' };

                _buffer.Write(quoteChar);
                _buffer.Write(AttributeValue);
                _buffer.Write(quoteChar);
            }
        }

        static readonly char[] NCNameStartChars = { 'A', '_', 'a', (char)0xc0, (char)0xd6, (char)0xd8, (char)0xf6, 
                                            (char)0xf8, (char)0x2ff, (char)0x370, (char)0x37d, (char)0x37f, 
                                            (char)0x1fff, (char)0x200c, (char)0x200d, (char)0x2070, (char)0x218f, 
                                            (char)0x2c00, (char)0x2fef, (char)0x3001, (char)0xd7ff, (char)0xf900, 
                                            (char)0xfdcf, (char)0xfdf0, (char)0xfffd };

        static readonly char[] NCNameChars = { 'A', '_', 'a', (char)0xc0, (char)0xd6, (char)0xd8, (char)0xf6, 
                                            (char)0xf8, (char)0x2ff, (char)0x370, (char)0x37d, (char)0x37f, 
                                            (char)0x1fff, (char)0x200c, (char)0x200d, (char)0x2070, (char)0x218f, 
                                            (char)0x2c00, (char)0x2fef, (char)0x3001, (char)0xd7ff, (char)0xf900, 
                                            (char)0xfdcf, (char)0xfdf0, (char)0xfffd,
                                            '-', '.', '0', '9', (char)0xb7, (char)0x300, (char)0x36f, (char)0x203f, 
                                            (char)0x2040 };

        private void BuildNCName(bool addToNames)
        {
            int nameLen = GetRandomExp(5) + 1;

            char[] name = new char[nameLen];

            name[0] = NCNameStartChars[GetRandom(NCNameStartChars.Length)];

            for (int i = 1; i < nameLen - 1; i++)
            {
                name[i] = NCNameChars[GetRandom(NCNameChars.Length)];
            }

            name[nameLen - 1] = 'b';

            String nameStr = new String(name);

            if (addToNames) _names.Push(nameStr);

            _buffer.Write(Encoding.UTF8.GetBytes(nameStr));
        }

        static byte[] Spaces = new byte[] { (byte)' ', (byte)'\t', (byte)'\r', (byte)'\n' };
        private void BuildSpace()
        {
            switch (GetRandom(4))
            {
                case 0:
                    _buffer.Write(Encoding.UTF8.GetBytes(new String(' ', GetRandomExp(5) + 1)));
                    return;
                case 1:
                    _buffer.Write(new byte[] { (byte)'\r', (byte)'\n' });
                    return;
                case 2:
                    _buffer.Write(new byte[] { (byte)' ' });
                    return;
                case 3:
                    int len = GetRandomExp(5) + 1;
                    byte[] buf = new byte[len];

                    for (int i = 0; i < len; i++)
                    {
                        buf[i] = Spaces[GetRandom(4)];
                    }

                    _buffer.Write(buf);
                    return;
            }
        }

        private void BuildPI()
        {
            _buffer.Write(Encoding.UTF8.GetBytes("<?pi PI?>"));
        }

        private void BuildComment()
        {
            _buffer.Write(Encoding.UTF8.GetBytes("<!-- comments -->"));
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
