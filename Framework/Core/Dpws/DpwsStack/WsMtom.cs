using System;
using System.Net;
using System.Collections;
using System.Text;
using System.IO;

using Microsoft.SPOT;
using System.Ext;

namespace Ws.Services.Mtom
{
    /// <summary>
    /// Mtom parameters received in an http header/content-type
    /// </summary>
    internal class WsMtomParams
    {
        /// <summary>
        /// Creates an instance of an MtomParams class.
        /// </summary>
        public WsMtomParams()
        {
        }

        /// <summary>
        /// Use to Get or Set the boundary element of an mtom message.
        /// </summary>
        public string boundary;

        /// <summary>
        /// Use to Get or Set content type of an mtom message header.
        /// </summary>
        public string type;

        /// <summary>
        /// Use to Get or Set the start element of an mtom message header.
        /// </summary>
        public string start;

        /// <summary>
        /// Use to Get or set the start info element of an mtom message header.
        /// </summary>
        public string startInfo;
    }

    /// <summary>
    /// An Mtom body parts
    /// </summary>
    public class WsMtomBodyPart
    {

        /// <summary>
        /// Creates an instance of an MtomBodyPart class.
        /// </summary>
        public WsMtomBodyPart()
        {
        }

        /// <summary>
        /// Use to Get or Set the content ID property of an MtomBodyPart.
        /// </summary>
        public string ContentID;

        /// <summary>
        /// Use to Get or Set a MemoryStream property that contains the actual content of an MtomBodyPart.
        /// </summary>
        public MemoryStream Content;

        /// <summary>
        /// Use to Get or Set the content type property of an MtomBodyPart.
        /// </summary>
        public string ContentType;

        /// <summary>
        /// Use to Get or Set the content transfer encoding property of an MtomBodyPart.
        /// </summary>
        public string ContentTransferEncoding;
    }

    /// <summary>
    /// A collection of Mtom body parts
    /// </summary>
    public class WsMtomBodyParts
    {
        private ArrayList m_partsList;
        private string    m_boundary;
        private string    m_start;
        private object    m_threadLock;

        /// <summary>
        /// Creates an instance of an MtomBodyParts collection.
        /// </summary>
        public WsMtomBodyParts()
        {
            m_partsList  = new ArrayList();
            m_threadLock = new object();
        }

        /// <summary>
        /// Use to Get or Set an Mtom boundary element.
        /// </summary>
        public string Boundary { get { return m_boundary; } set { m_boundary = value; } }

        /// <summary>
        /// Use to get or set the content id of the start mtom element. This value is used in the http header.
        /// </summary>
        public string Start { get { return m_start; } set { m_start = value; } }

        /// <summary>
        /// Gets the number of MtomBodyParts in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return m_partsList.Count;
            }
        }

        /// <summary>
        /// Gets or sets the MtomBodybody part at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the body part to get or set.</param>
        /// <returns>The MtomBodyPart object at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If index is less than zero.-or- index is equal to or greater than collection count.
        /// </exception>
        public WsMtomBodyPart this[int index]
        {
            get
            {
                return (WsMtomBodyPart)m_partsList[index];
            }
        }

        public WsMtomBodyPart this[String contentID]
        {
            get
            {
                lock (m_threadLock)
                {
                    int count = m_partsList.Count;
                    WsMtomBodyPart part;
                    for (int i = 0; i < count; i++)
                    {
                        part = (WsMtomBodyPart)m_partsList[i];
                        if (part.ContentID == contentID)
                        {
                            return part;
                        }
                    }

                    return null;
                }
            }
        }

        /// <summary>
        /// Adds an MtomodyPart object to the end of the collection.
        /// </summary>
        /// <param name="value">The MtomBodyPart object to be added to the end of the collection.
        /// The value can be null.
        /// </param>
        /// <returns>The index at which the MtomBodyPart has been added.</returns>
        public int Add(WsMtomBodyPart value)
        {
            lock (m_threadLock)
            {
                return m_partsList.Add(value);
            }
        }

        /// <summary>
        /// Adds an MtomodyPart object to the beginning of the collection.
        /// </summary>
        /// <param name="value">The MtomBodyPart object to be added to the beginning of the collection.
        /// The value can be null.
        /// </param>
        public void AddStart(WsMtomBodyPart value)
        {
            lock(m_threadLock)
            {
                m_partsList.Insert(0, value);
            }
        }

        /// <summary>
        /// Removes all MtomBodyParts from the collection.
        /// </summary>
        public void Clear()
        {
            lock (m_threadLock)
            {
                m_partsList.Clear();
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific MtomBodyPart oject from the collection.
        /// </summary>
        /// <param name="bodyPart">The MtomBodyPart object to remove from the collection. The value can be null.</param>
        public void Remove(WsMtomBodyPart bodyPart)
        {
            lock (m_threadLock)
            {
                m_partsList.Remove(bodyPart);
            }
        }
    }

    /// <summary>
    /// Use to parse and store Mtom body parts
    /// </summary>
    internal class WsMtom
    {
        private WsMtomBodyParts m_bodyParts;
        private int             m_curPos;
        private int             m_length;
        private byte[]          m_buffer;
        private int             m_growSize;
        private int             m_capacity;
        private int             m_maxCapacity;
        private UTF8Encoding    m_utf8Encoding;

        /// <summary>
        /// WsMtom - Mtom parser and generator
        /// </summary>
        public WsMtom()
        {
            m_curPos       = 0;
            m_length       = 0;
            m_buffer       = null;
            m_growSize     = 0xFFFF;
            m_capacity     = 0xFFFF;
            m_maxCapacity  = 0xFFFFF;
            m_utf8Encoding = new UTF8Encoding();            
            m_bodyParts    = new WsMtomBodyParts();
        }

        /// <summary>
        /// WsMtom - Mtom parser and generator
        /// </summary>
        /// <param name="buffer">byte array containing raw mtom message</param>
        public WsMtom(byte[] buffer) : this()
        {
            if (buffer == null)
                throw new NullReferenceException(); // "Mtom buffer must not be null");
            if (buffer.Length > m_maxCapacity)
                throw new ArgumentOutOfRangeException(); // "Mtom buffer", "Exceeds max capacity.");
            m_buffer = buffer;
            m_length = buffer.Length;
            m_bodyParts = new WsMtomBodyParts();
        }

        public WsMtomBodyParts BodyParts { get { return m_bodyParts; } }
        public int Capacity { get { return m_maxCapacity; } set { m_maxCapacity = value; } }

        /// <summary>
        /// Create an mtom message from body parts list
        /// </summary>
        /// <param name="bodyParts">Body Parts collection used to create the Mtom message</param>
        /// <returns>byte array containing an mtom message</returns>
        public byte[] CreateMessage(WsMtomBodyParts bodyParts)
        {
            if (bodyParts == null)
                throw new ArgumentNullException(); // "Mtom body parts collection must not be null.");
            if (bodyParts.Count == 0)
                throw new ArgumentException(); // "Mtom bodyParts must at least contain a soap message body part.");

            // Initialize the internal buffer, position and length
            m_buffer = new byte[m_growSize];
            m_curPos = 0;
            m_length = 0;

            // If mtom body parts boundary is null generate one
            if (bodyParts.Boundary == null)
                m_bodyParts.Boundary = Guid.NewGuid().ToString() + '-' + Guid.NewGuid().ToString().Substring(0, 33);
            else
                m_bodyParts.Boundary = bodyParts.Boundary;

            // If start is null set to the first body part content id.
            if (bodyParts.Start == null)
                m_bodyParts.Start = bodyParts[0].ContentID;
            else
                m_bodyParts.Start = bodyParts.Start;

            WriteLine("--" + bodyParts.Boundary);
            for (int i = 0; i < bodyParts.Count; ++i)
            {
                WriteLine(HttpKnownHeaderNames.ContentType + ": " + bodyParts[i].ContentType);
                WriteLine(HttpKnownHeaderNames.ContentTransferEncoding + ": " + bodyParts[i].ContentTransferEncoding);
                WriteLine(HttpKnownHeaderNames.ContentID + ": " + bodyParts[i].ContentID);
                WriteLine("");
                WriteBytes(bodyParts[i].Content.ToArray());
                WriteLine("");
                if (i == bodyParts.Count - 1)
                    WriteString("--" + bodyParts.Boundary + "--");
                else
                    WriteLine("--" + bodyParts.Boundary);
            }

            // Create new buffer at the exact length
            byte[] tempBuff = new byte[m_length];
            Array.Copy(m_buffer, tempBuff, m_length);
            return tempBuff;
        }

        /// <summary>
        /// GrowBuffer - grows the internal buffer by growSize.
        /// Creates a new bufferequal to the current buff size plus internal grow size.
        /// Copies content from existing buffer to new buffer, nulls existing buffer
        /// and set existing buffer to new buffer.
        /// </summary>
        /// <param name="reqSize">Integer representing the buffer size required</param>
        ///
        private void GrowBuffer(int reqSize)
        {
            // Grow the buffer in chuncks
            while (m_capacity < reqSize)
                m_capacity = m_capacity + m_growSize;

            if (m_capacity > m_maxCapacity)
                throw new ArgumentOutOfRangeException(); // "WsMtom internal buffer", "Exceeds max capacity.");

            byte[] newBuff = new byte[m_capacity];
            Array.Copy(m_buffer, 0, newBuff, 0, m_length);
            m_buffer = newBuff;
            return;
        }

        /// <summary>
        /// Parse the mtom message. Validate and store body parts
        /// </summary>
        /// <param name="boundary">A string contining the Mtom boundary element.</param>
        /// <returns>byte arrary containing the root body part message</returns>
        public byte[] ParseMessage(string boundary)
        {
            if (m_buffer == null)
                throw new ArgumentNullException(); //"BodyParts", "Must use WsMtom(byte[] buffer) constructor with ParseMessage.");
            if (boundary == null)
                throw new ArgumentNullException(); // "boudary", "must not be null");

            byte[] soapMessage = null;
            bool soapPartFound = false;
            string readBuffer = null;

            // Set the boundary element
            m_bodyParts.Boundary = boundary;

            // Read to first boundary sentinal
            while ((readBuffer = ReadLine()) != null)
            {
                // Make sure there's nothing but blank lines upto first boundary
                if (readBuffer.Length == 0)
                    continue;

                // If a boundary line is found break
                if (readBuffer == "--" + boundary)
                    break;
                else
                    throw new System.Xml.XmlException(); // "Invalid Mtom pre-boundary characters.");

            }

            // Read to the first Mtom header line
            while ((readBuffer = ReadLine()) != null && readBuffer.Length == 0);

            // Read Mtom header
            while (m_curPos < m_length)
            {
                WsMtomBodyPart bodyPart = new WsMtomBodyPart();
                byte[] bodyPartContent = null;

                // Read the body part header
                while (readBuffer.Length != 0)
                {
                    // Parse the body part header
                    int idx = readBuffer.IndexOf(':');

                    if(idx != -1)
                    {
                        string headerEntry = readBuffer.Substring(0, idx);
                        string headerValue = readBuffer.Substring(idx+1);

                        switch (headerEntry)
                        {
                            case HttpKnownHeaderNames.ContentType:
                                bodyPart.ContentType = headerValue.Trim(' ');
                                break;
                            case HttpKnownHeaderNames.ContentTransferEncoding:
                                bodyPart.ContentTransferEncoding = headerValue.Trim(' ');
                                break;
                            case HttpKnownHeaderNames.ContentID:
                                bodyPart.ContentID = headerValue.Trim(' ');
                                break;
                            default:
                                throw new System.Xml.XmlException(); // "Invalid Mtom header value \"" + readBuffer + "\"");
                        }
                    }

                    // Read the next header line.
                    readBuffer = ReadLine();
                }

                // Make sure all of the body part header entries are valid
                if (bodyPart.ContentID == null)
                    throw new ArgumentException(); // "Mtom Content-ID header missing.");
                if (bodyPart.ContentType == null)
                    throw new ArgumentException(); // "Mtom Content-Type header missing.");
                if (bodyPart.ContentTransferEncoding == null)
                    throw new ArgumentException(); // "Mtom Content-Transfer-Encoding header missing.");

                // Only support binary encoding
                if (bodyPart.ContentTransferEncoding != "binary" && bodyPart.ContentTransferEncoding != "8bit")
                    throw new ArgumentException(); // "Mtom Content-Transfer-Encoding must be \"binary\"");

                // Read the Mtom body parts
                if ((bodyPartContent = ReadBinaryBodyPart(boundary)) == null)
                    throw new ArgumentException(); // "Malformed Mtom body part. A valid end boundar is missing.");

                // If this is the soap message convert the soap message to a byte[]
                // and store for return
                if (!soapPartFound)
                {
                    if (bodyPart.ContentType.ToLower().IndexOf("type=\"application/soap+xml\"") != -1)
                    {
                        soapMessage = bodyPartContent;
                        soapPartFound = true;
                    }
                    else
                        throw new ArgumentException(); // "First Mtom body part must contain the soap message.");
                }

                // Store the body part memory stream
                bodyPart.Content = new MemoryStream(bodyPartContent);

                //  Store the bodyPart in the bodyParts collection
                m_bodyParts.Add(bodyPart);

                // If at least two characters don't follow the boundary it is an error.
                // A mid boundary marker will be followed by \r\n. An end boundary will be
                // followed by --\r\n.
                if (m_length - m_curPos > 2)
                {
                    // Check to see if this was the end boundary.
                    if (m_buffer[m_curPos + 1] == '-' && m_buffer[m_curPos + 2] == '-')
                        break;
                    // If not eat the \r\n characters.
                    else if (m_buffer[m_curPos + 1] == '\r' && m_buffer[m_curPos + 2] == '\n')
                        m_curPos += 3;
                    else
                        throw new ArgumentException(); // "Invalid Mtom boundary element. Expected cr-lf after boundary.");

                }
                else
                    throw new ArgumentException(); // "Invalid Mtom boundary element. Expected end boundary trailer or cr-lf.");

                // read the next header line or blank line
                readBuffer = ReadLine();
            }

            // Return the soap message body part
            return soapMessage;
        }

        /// <summary>
        /// ReadLine - Read buffer upto first \r\n chars
        /// </summary>
        /// <returns>Return string containing: Bytes upto first \r\n characters.
        /// or remainder of buffer if end of buffer is reached but no \r\n found
        /// or empty string if \r\n found at first position
        /// or null if at end of buffer on call
        /// </returns>
        private string ReadLine()
        {
            int startPos = m_curPos;
            int lastBufPos = m_length - 1;
            int endPos = m_curPos;

            if(m_curPos == m_length) return null;
            
            int idx = Array.IndexOf(m_buffer, '\r', startPos, m_length - startPos);

            if(idx >= 0)
            {
                if(idx < lastBufPos && m_buffer[idx+1] == '\n')
                {
                    m_curPos = idx+2;
                }
                else
                {
                    m_curPos = idx;
                }

                endPos = idx;
            }
            else
            {
                m_curPos = lastBufPos;
                endPos   = lastBufPos;
            }

            if(startPos == endPos) return "";

            int length = endPos - startPos;

            byte[] tempBuff = new byte[length];

            Array.Copy(m_buffer, startPos, tempBuff, 0, length);

            return new string(m_utf8Encoding.GetChars(tempBuff), 0, length);
        }

        /// <summary>
        /// Reads an Mtom body part and return a body part array
        /// </summary>
        /// <param name="boundary">String representing the boundary identifier</param>
        /// <returns>Byte array containing the body part</returns>
        private byte[] ReadBinaryBodyPart(string boundary)
        {
            // Try to aviod walking string for optimization on .NET_MF
            byte[] boundaryBuff = m_utf8Encoding.GetBytes(boundary);
            int boundaryLen = boundary.Length;
            byte[] contentBuff = null;
            int startPos = m_curPos;

            // If there's not enough remaining bytes to read a boundary return null
            if (m_curPos + boundaryLen >= m_length)
                return null;

            // Walk the buffer until new boundary or end boundary is found
            // fast search is not implemented
            for (; m_curPos < m_length; ++m_curPos)
            {
                // Mtom boundaries always start with --
                if (m_buffer[m_curPos] == '-')
                {
                    ++m_curPos;
                    if (m_buffer[m_curPos] == '-')
                    {
                        ++m_curPos;
                        int remainder = m_length - 1 - m_curPos;

                        // Look for boundary. Skip first -- characters
                        int ii = 2;
                        for (; ii < remainder; ++ii)
                            if (m_buffer[m_curPos + ii] != boundaryBuff[ii] || ii == boundaryLen - 1)
                                break;

                        // If boundary match found update current position, and return content byte array
                        if (ii == boundaryLen - 1)
                        {
                            // When buffer length is calculated subtract two because we incremented m_curPos
                            // past the -- start sentinals and subtract two more because we need to eat the \r\n
                            // at the beginning of the bundary per sepc
                            contentBuff = new byte[m_curPos - startPos - 4];

                            // Copy the body section to a byte array. Remove the last \r\n beacuse
                            // it's considered part og the boundry marker
                            Array.Copy(m_buffer, startPos, contentBuff, 0, m_curPos - startPos - 4);
                            m_curPos += ii;
                            return contentBuff;
                        }

                        // If we get here decrement m_curPos so we don't skip a character in the base for loop
                        --m_curPos;
                    }
                }
            }

            return contentBuff;
        }

        /// <summary>
        /// Writes a string an Mtom body part
        /// </summary>
        /// <param name="text">String containing the body part text</param>
        private void WriteString(string text)
        {
            // Check on the write buffer. If we are at the end of the buffer
            // try to grow it.
            if (m_curPos + text.Length >= m_capacity)
            {
                GrowBuffer(m_curPos + text.Length);
            }

            // Copy string to buffer and add \r\n
            int stringLength = text.Length;
            byte[] textBuffer = m_utf8Encoding.GetBytes(text);
            Array.Copy(textBuffer, 0, m_buffer, m_curPos, stringLength);
            m_curPos += stringLength;
            m_length = m_curPos;
        }

        /// <summary>
        /// WriteBytes - Write byte array into buffer starting at current position
        /// </summary>
        /// <param name="buffer">byte array containing raw mtom message</param>
        ///
        private void WriteBytes(byte[] buffer)
        {
            // Check on the read buffer. If we are at the end of the buffer
            // try to grow it.
            if (m_curPos + buffer.Length >= m_capacity)
            {
                GrowBuffer(m_curPos + buffer.Length);
            }

            // Copy array to buffer
            int length = buffer.Length;
            Array.Copy(buffer, 0, m_buffer, m_curPos, length);
            m_curPos += length;
            m_length = m_curPos;
        }

        /// <summary>
        /// WriteLine - Write line of text at the current position into buffer and adds \r\n
        /// </summary>
        /// <param name="text">String containing text line</param>
        ///
        private void WriteLine(string text)
        {
            WriteString(text + "\r\n");
        }
    }
}


