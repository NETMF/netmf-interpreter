using System.IO;
using System.Text;
using System.Collections;

namespace System.Xml
{
    // Summary:
    //     Specifies the state of the System.Xml.XmlWriter.
    internal enum WriteState
    {
        // Summary:
        //     A Write method has not been called.
        Start = 0,
        //
        // Summary:
        //     The prolog is being written.
        Prolog = 1,
        //
        // Summary:
        //     An element start tag is being written.
        Element = 2,
        //
        // Summary:
        //     An attribute value is being written.
        Attribute = 3,
        //
        // Summary:
        //     The element content is being written.
        Content = 4,
        //
        // Summary:
        //     The System.Xml.XmlWriter.Close() method has been called.
        Closed = 5,
        //
        // Summary:
        //     An exception has been thrown, which has left the System.Xml.XmlWriter in
        //     an invalid state. You may call the System.Xml.XmlWriter.Close() method to
        //     put the System.Xml.XmlWriter in the System.Xml.WriteState.Closed state. Any
        //     other System.Xml.XmlWriter method calls results in an System.InvalidOperationException
        //     being thrown.
        Error = 6,
    }

    // Summary:
    //     Represents Element item used by the writer to provide context when writing Elements and
    //     EndElements.
    internal class ElementInfo
    {
        // Fields
        public string Name = null;
        public string NSPrefix = null;
        public string ChildNSPrefix = null;
        public string NameSpaceName = null;
        public WriteState State = WriteState.Element;
        public bool IsEmpty = true;
    }

    // Summary:
    //     Represents a writer that provides a fast, non-cached, forward-only means
    //     of generating streams or files containing XML data.
    public class XmlWriter : IDisposable
    {
        // Fields
        //private XmlWriterSettings _Settings;
        private WriteState _WriteState = WriteState.Start;
        private string _XmlLang = "en-US";
        private int _autoPrefixIndex = 1;

        // Element stack
        Stack _ElementStack = new Stack();

        // Internal Stream
        private Stream _Stream = null;

        // Disposeed flag
        private bool _Disposed = true;

        // Internal stream read buffer
        private byte[] _Buffer = null;
        private int _DefaultBufferSize = 2048;

        // The current position within the internal buffer
        private int _Position = 0;

        // Summary:
        //     Initializes a new instance of the System.Xml.XmlWriter class.
        protected XmlWriter()
        {
            _Buffer = new byte[_DefaultBufferSize];
            //_Settings = new XmlWriterSettings();
            _Disposed = false;
        }

        // Summary:
        //     Initializes a new instance of the System.Xml.XmlWriter class using supplied stream.
        protected XmlWriter(Stream output)
        {
            _Buffer = new byte[_DefaultBufferSize];
            //_Settings = new XmlWriterSettings();
            _Disposed = false;
            _Stream = output;
        }

        //
        // Summary:
        //     When overridden in a derived class, gets the current xml:lang scope.
        //
        // Returns:
        //     The current xml:lang or null if there is no xml:lang in the current scope.
        public virtual string XmlLang { get { return _XmlLang; } }

        // Summary:
        //     When overridden in a derived class, closes this stream and the underlying
        //     stream.
        //
        // Exceptions:
        //   System.InvalidOperationException:
        //     A call is made to write more output after Close has been called or the result
        //     of this call is an invalid XML document.
        public void Close()
        {
            _WriteState = WriteState.Closed;
            _Stream.Close();
        }

        //
        // Summary:
        //     Creates a new System.Xml.XmlWriter instance using the specified stream.
        //
        // Parameters:
        //   output:
        //     The stream to which you want to write. The System.Xml.XmlWriter writes XML
        //     1.0 text syntax and appends it to the specified stream.
        //
        // Returns:
        //     An System.Xml.XmlWriter object.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     The stream value is null.
        public static XmlWriter Create(Stream output)
        {
            if (output == null)
                throw new ArgumentNullException("output", "stream must not be null");

            XmlWriter writer = new XmlWriter(output);
            return writer;
        }

        //
        // Summary:
        //     Releases the unmanaged resources used by the System.Xml.XmlWriter and optionally
        //     releases the managed resources.
        //
        // Parameters:
        //   disposing:
        //     true to release both managed and unmanaged resources; false to release only
        //     unmanaged resources.
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_Stream != null)
                {
                    _Stream.Dispose();
                }
            }

            //_Settings = null;
            _XmlLang = null;
            _ElementStack = null;
            _Stream = null;
            _Buffer = null;
        }

        //
        // Summary:
        //     Releases all resources used by the System.IO.TextReader object.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~XmlWriter()
        {
            Dispose(false);
        }

        //
        // Summary:
        //     When overridden in a derived class, flushes whatever is in the buffer to
        //     the underlying streams and also flushes the underlying stream.
        public void Flush()
        {
            FlushBuffer(true);
            _Stream.Flush();
        }

        //
        // Summary:
        //     Checks to see if we are at the end of the write buffer.
        //     If so write _Buffer to the stream. If write is successful
        //     _Position is advanced by the number of characters written.
        //
        private void FlushBuffer(bool forceFlush)
        {

            // Check on the write buffer. if 0 based _curBufPos is at _curBufPos
            // it is sitting one beyond the actual buffer. Write _Buffer to stream.
            if (_Position == _DefaultBufferSize || forceFlush)
            {
                try
                {
                    if (_Position > 0)
                        _Stream.Write(_Buffer, 0, _Position);
                }
                catch (Exception e)
                {
                    throw new IOException("Internal FlushBuffer() _Stream.Write", e);
                }

                _Position = 0;
            }

            return;
        }

        //
        // Summary:
        //     When overridden in a derived class, returns the closest prefix defined in
        //     the current namespace scope for the namespace URI.
        //
        // Parameters:
        //   ns:
        //     The namespace URI whose prefix you want to find.
        //
        // Returns:
        //     The matching prefix or null if no matching namespace URI is found in the
        //     current scope.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     ns is either null or String.Empty.
        public string LookupPrefix(string ns)
        {
            if (ns == null)
                throw new ArgumentException("must not be null", "ns");
            return "";
            // need to add namspace list lookup
        }

        //
        // Summary:
        //     When overridden in a derived class, writes out the attribute with the specified
        //     local name and value.
        //
        // Parameters:
        //   localName:
        //     The local name of the attribute.
        //
        //   value:
        //     The value of the attribute.
        //
        // Exceptions:
        //   System.InvalidOperationException:
        //     The state of writer is not WriteState.Element or writer is closed.
        //
        //   System.ArgumentException:
        //     The xml:space attribute value is invalid.
        public void WriteAttributeString(string localName, string value)
        {
            this.WriteAttributeString(null, localName, null, value);
        }

        //
        // Summary:
        //     When overridden in a derived class, writes an attribute with the specified
        //     local name, namespace URI, and value.
        //
        // Parameters:
        //   localName:
        //     The local name of the attribute.
        //
        //   value:
        //     The value of the attribute.
        //
        //   ns:
        //     The namespace URI to associate with the attribute.
        //
        // Exceptions:
        //   System.InvalidOperationException:
        //     The state of writer is not WriteState.Element or writer is closed.
        //
        //   System.ArgumentException:
        //     The xml:space attribute value is invalid.
        public void WriteAttributeString(string localName, string ns, string value)
        {
            this.WriteAttributeString(null, localName, ns, value);
        }

        //
        // Summary:
        //     When overridden in a derived class, writes out the attribute with the specified
        //     prefix, local name, namespace URI, and value.
        //
        // Parameters:
        //   localName:
        //     The local name of the attribute.
        //
        //   prefix:
        //     The namespace prefix of the attribute.
        //
        //   value:
        //     The value of the attribute.
        //
        //   ns:
        //     The namespace URI of the attribute.
        //
        // Exceptions:
        //   System.InvalidOperationException:
        //     The state of writer is not WriteState.Element or writer is closed.
        //
        //   System.ArgumentException:
        //     The xml:space attribute value is invalid.
        public void WriteAttributeString(string prefix, string localName, string ns, string value)
        {
            if (_WriteState == WriteState.Closed)
                throw new InvalidOperationException("XmlWriter is closed");
            if (_WriteState != WriteState.Element)
                throw new InvalidOperationException("XmlWriter is not on an element");
            if (prefix == "xml" && localName == "space")
                if (value != "default" && value != "preserve")
                    throw new ArgumentException("invalid xml:space attribute");
            if (localName == null)
                throw new ArgumentNullException("localName", "must not be null");

            // Check to see if a started attribute need finished
            if (_WriteState == WriteState.Attribute)
                this.WriteEndAttribute();

            string localPrefix = "";

            // Check for special xmlns processing
            if (prefix == "xmlns" || (prefix == null && ns == "http://www.w3.org/2000/xmlns/"))
            {
                if (!(ns == null || ns == "http://www.w3.org/2000/xmlns/"))
                    throw new ArgumentException("Prefix \"xmlns\" is reserved for use by XML.");

                if (value == null)
                    throw new ArgumentNullException("value", "must not be null for xmlns");

                this.WriteRaw(" xmlns:" + localName + "=" + "\"" + value + "\"");
                ElementInfo ei2 = (ElementInfo)_ElementStack.Pop();
                ei2.State = _WriteState = WriteState.Element;
                _ElementStack.Push(ei2);
                return;
            }

            // if Prefix is null and namespace is set, auto generate a prefix
            // if we have a prefix and namespace, use prefix parameter,
            // prefix is blanked for all other conditions
            if (prefix == null && ns != null)
                localPrefix = "p" + _ElementStack.Count;
            else if (prefix != null && ns != null)
                localPrefix = prefix;
            else
                localPrefix = "";

            value = value.Trim('\"');

            if (localPrefix.Length == 0)
                this.WriteRaw(" " + localName + "=" + "\"" + value + "\"");
            else
                this.WriteRaw(" xmlns:" + localPrefix + "=" + "\"" + ns + "\"" + " " + localPrefix + ":" + localName + "=" + "\"" + value + "\"");
            ElementInfo ei = (ElementInfo)_ElementStack.Pop();
            ei.State = _WriteState = WriteState.Element;
            _ElementStack.Push(ei);

        }

        //
        // Summary:
        //     When overridden in a derived class, encodes the specified binary bytes as
        //     Base64 and writes out the resulting text.
        //
        // Parameters:
        //   count:
        //     The number of bytes to write.
        //
        //   buffer:
        //     Byte array to encode.
        //
        //   index:
        //     The position in the buffer indicating the start of the bytes to write.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     The buffer length minus index is less than count.
        //
        //   System.ArgumentNullException:
        //     buffer is null.
        //
        //   System.ArgumentOutOfRangeException:
        //     index or count is less than zero.
        public void WriteBase64(byte[] buffer, int index, int count)
        {
            this.WriteString(buffer != null ? System.Convert.ToBase64String(buffer, index, count) : "");
        }

        //
        // Summary:
        //     When overridden in a derived class, writes text one buffer at a time.
        //
        // Parameters:
        //   count:
        //     The number of characters to write.
        //
        //   buffer:
        //     Character array containing the text to write.
        //
        //   index:
        //     The position in the buffer indicating the start of the text to write.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     buffer is null.
        //
        //   System.ArgumentOutOfRangeException:
        //     index or count is less than zero. -or-The buffer length minus index is less
        //     than count; the call results in surrogate pair characters being split or
        //     an invalid surrogate pair being written.
        public void WriteChars(char[] buffer, int index, int count)
        {
            this.WriteRaw(buffer, index, count);
        }

        //
        // Summary:
        //     When overridden in a derived class, writes out a comment <!--...--> containing
        //     the specified text.
        //
        // Parameters:
        //   text:
        //     Text to place inside the comment.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     The text would result in a non-well formed XML document.
        public void WriteComment(string text)
        {
            this.WriteRaw("<!--" + text + "-->");
        }

        //
        // Summary:
        //     When overridden in a derived class, writes an element with the specified
        //     local name and value.
        //
        // Parameters:
        //   localName:
        //     The local name of the element.
        //
        //   value:
        //     The value of the element.
        //
        // Exceptions:
        //   System.InvalidOperationException:
        //     This results in an invalid XML document.
        public void WriteElementString(string localName, string value)
        {
            this.WriteElementString(null, localName, null, value);
        }

        //
        // Summary:
        //     When overridden in a derived class, writes an element with the specified
        //     local name, namespace URI, and value.
        //
        // Parameters:
        //   localName:
        //     The local name of the element.
        //
        //   value:
        //     The value of the element.
        //
        //   ns:
        //     The namespace URI to associate with the element.
        //
        // Exceptions:
        //   System.InvalidOperationException:
        //     This results in an invalid XML document.
        public void WriteElementString(string localName, string ns, string value)
        {
            this.WriteElementString(null, localName, ns, value);
        }

        //
        // Summary:
        //     Writes an element with the specified local name, namespace URI, and value.
        //
        // Parameters:
        //   localName:
        //     The local name of the element.
        //
        //   prefix:
        //     The prefix of the element.
        //
        //   value:
        //     The value of the element.
        //
        //   ns:
        //     The namespace URI of the element.
        //
        // Exceptions:
        //   System.InvalidOperationException:
        //     This results in an invalid XML document.
        public void WriteElementString(string prefix, string localName, string ns, string value)
        {
            // Can't write to a closed writer
            if (_WriteState == WriteState.Closed)
                throw new InvalidOperationException("writer is closed");
            // Local name can never be blank
            if (localName == null || localName.Length == 0)
                throw new ArgumentException("The empty string '' is not a valid local name.");
            // Prefix cannot be set when ns = null
            if ((prefix != null && prefix.Length != 0) && (ns == null || ns.Length == 0))
                throw new ArgumentException("Cannot use a prefix with an empty namespace.");
            // Xmlns is reserved and must  only be used internally
            if (prefix == "xmlns")
                throw new ArgumentException("Prefix \"xmlns\" is reserved for use by XML.");

            // Attempt to finish the last write operation
            // if the previous tag has unfinished attributes attempt to finish them
            if (_WriteState == WriteState.Attribute)
                this.WriteEndAttribute();

            // If a Element tag has been started close the tag
            if (_WriteState == WriteState.Element)
                this.WriteRaw(">");

            // if _WriteState == Content all is good

            string localPrefix = "";
            string localNS = "";
            string localvalue = "";

            // If theres a prefix but no namespace write xmlns=ns
            if ((prefix == null || prefix.Length == 0) && (ns != null && ns.Length != 0))
            {
                localNS = " xmlns" + "=" + "\"" + ns + "\"";
            }
            // if there's a prefix and a namespace, prefix tag and write xmlns:prefix=ns
            else if ((prefix != null && prefix.Length != 0) && (ns != null && ns.Length != 0))
            {
                localPrefix = prefix + ":";
                localNS = " xmlns:" + prefix + "=" + "\"" + ns + "\"";
            }
            // If value is blank append shorthand end to tag. else append ">Value</localName>"
            if (value == null || value.Length == 0)
            {
                localvalue = "/>";
            }
            else
            {
                localvalue = ">" + value + "</" + localName + ">";
            }

            // Write the element
            this.WriteRaw("<" + localPrefix + localName + localNS + localvalue);

            // No reason to push this Element on the ElementStack it is complete

            // Set WriterState
            if (_ElementStack.Count > 0)
            {
                ElementInfo ei = (ElementInfo)_ElementStack.Pop();
                ei.State = _WriteState = WriteState.Content;
                // Signal that this element has content
                ei.IsEmpty = false;
                _ElementStack.Push(ei);
            }
        }

        //
        // Summary:
        //     When overridden in a derived class, closes the previous System.Xml.XmlWriter.WriteStartAttribute(System.String,System.String)
        //     call.
        public void WriteEndAttribute()
        {
            if (_WriteState != WriteState.Attribute)
                throw new ArgumentException("no attribute to end");
            this.WriteRaw("\"");

            ElementInfo ei = (ElementInfo)_ElementStack.Pop();
            ei.State = _WriteState = WriteState.Element;
            _ElementStack.Push(ei);
        }

        //
        // Summary:
        //     When overridden in a derived class, closes one element and pops the corresponding
        //     namespace scope.
        //
        // Exceptions:
        //   System.InvalidOperationException:
        //     This results in an invalid XML document.
        public void WriteEndElement()
        {
            // Check to see if a started attribute need finished
            if (_WriteState == WriteState.Attribute)
                this.WriteEndAttribute();

            // Pop the last Element from the stack
            ElementInfo lastElement = (ElementInfo)_ElementStack.Pop();

            // Check to see if content was been writen. If so use full end tag else use shorthand
            if (lastElement.IsEmpty == false)
            {
                if (lastElement.NSPrefix == null || lastElement.NSPrefix == "")
                    this.WriteRaw("</" + lastElement.Name + ">");
                else
                    this.WriteRaw("</" + lastElement.NSPrefix + ":" + lastElement.Name + ">");
            }
            else if (lastElement.State == WriteState.Element)
            {
                this.WriteRaw("/>");
            }
            else
                throw new InvalidOperationException("An EndElement would create malformer XML");

        }

        //
        // Summary:
        //     When overridden in a derived class, writes out a processing instruction with
        //     a space between the name and text as follows: <?name text?>.
        //
        // Parameters:
        //   name:
        //     The name of the processing instruction.
        //
        //   text:
        //     The text to include in the processing instruction.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     The text would result in a non-well formed XML document.name is either null
        //     or String.Empty.This method is being used to create an XML declaration after
        //     System.Xml.XmlWriter.WriteStartDocument() has already been called.
        public void WriteProcessingInstruction(string name, string text)
        {
            if (name == null || name.Length == 0)
                throw new ArgumentException("must not be null or string.Empty", "name");
            // Write processing instruction
            this.WriteRaw("<?" + name + " " + text + "?>" + "\n");
        }

        //
        // Summary:
        //     When overridden in a derived class, writes raw markup manually from a string.
        //
        // Parameters:
        //   data:
        //     String containing the text to write.
        public void WriteRaw(string data)
        {
            if (_Disposed == true)
                throw new Exception("StreamReader, object is disposed");
            if (data == null)
                throw new ArgumentNullException("buffer", "must not be null");

            byte[] b = Encoding.UTF8.GetBytes(data);

            _Stream.Write(b, 0, b.Length);
        }

        //
        // Summary:
        //     When overridden in a derived class, writes raw markup manually from a character
        //     buffer.
        //
        // Parameters:
        //   count:
        //     The number of characters to write.
        //
        //   buffer:
        //     Character array containing the text to write.
        //
        //   index:
        //     The position within the buffer indicating the start of the text to write.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     buffer is null.
        //
        //   System.ArgumentOutOfRangeException:
        //     index or count is less than zero. -or-The buffer length minus index is less
        //     than count.
        public void WriteRaw(char[] buffer, int index, int count)
        {
            if (_Disposed == true)
                throw new Exception("StreamReader, object is disposed");
            if (buffer == null)
                throw new ArgumentNullException("buffer", "must not be null");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "must be > 0");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "must be > 0");
            if (buffer.Length - index < count)
                throw new ArgumentException("buffer.Length - index must > count");

            _Stream.Write(Encoding.UTF8.GetBytes(new string(buffer)), index, count);

            return;
        }

        //
        // Summary:
        //     Writes the start of an attribute with the specified local name.
        //
        // Parameters:
        //   localName:
        //     The local name of the attribute.
        public void WriteStartAttribute(string localName)
        {
            WriteStartAttribute(null, localName, null);
        }

        //
        // Summary:
        //     Writes the start of an attribute with the specified local name and namespace
        //     URI.
        //
        // Parameters:
        //   localName:
        //     The local name of the attribute.
        //
        //   ns:
        //     The namespace URI of the attribute.
        public void WriteStartAttribute(string localName, string ns)
        {
            WriteStartAttribute(null, localName, ns);
        }

        //
        // Summary:
        //     When overridden in a derived class, writes the start of an attribute with
        //     the specified prefix, local name, and namespace URI.
        //
        // Parameters:
        //   localName:
        //     The local name of the attribute.
        //
        //   prefix:
        //     The namespace prefix of the attribute.
        //
        //   ns:
        //     The namespace URI for the attribute.
        public void WriteStartAttribute(string prefix, string localName, string ns)
        {
            if (_WriteState == WriteState.Closed)
                throw new InvalidOperationException("XmlWriter is closed");
            if (_WriteState != WriteState.Element)
                throw new InvalidOperationException("XmlWriter is not on an element");
            if (localName == null)
                throw new ArgumentNullException("localName", "must not be null");
            if (_WriteState == WriteState.Attribute)
                throw new InvalidOperationException("Nested StartAttributes are not permitted");

            string localPrefix = "";

            if (prefix != null && prefix.Length == 0) prefix = null;

            // Check for special xmlns processing
            if (prefix == "xmlns" || ns == "http://www.w3.org/2000/xmlns/")
            {
                if ((ns != null && ns != "http://www.w3.org/2000/xmlns/") || (prefix != null && prefix != "xmlns"))
                    throw new ArgumentException("Prefix \"xmlns\" is reserved for use by XML.");

                this.WriteRaw(" xmlns:" + localName + "=\"");
                ElementInfo ei2 = (ElementInfo)_ElementStack.Pop();
                ei2.State = _WriteState = WriteState.Attribute;
                _ElementStack.Push(ei2);
                return;
            }

            // if Prefix is null and we have a namespace, auto generate a prefix
            // if we have a prefix and namespace, use prefix parameter
            // prefix is blanked for all other conditions
            if (prefix == null && ns != null)
                localPrefix = "p" + _ElementStack.Count;
            else if (prefix != null && ns != null)
                localPrefix = prefix;
            else
                localPrefix = "";

            if (localPrefix.Length == 0)
                this.WriteRaw(" " + localName + "=\"");
            else
                this.WriteRaw(" xmlns:" + localPrefix + "=" + "\"" + ns + "\"" + " " + localPrefix + ":" + localName + "=\"");

            ElementInfo ei = (ElementInfo)_ElementStack.Pop();
            ei.State = _WriteState = WriteState.Attribute;
            _ElementStack.Push(ei);

        }

        //
        // Summary:
        //     When overridden in a derived class, writes out a start tag with the specified
        //     local name.
        //
        // Parameters:
        //   localName:
        //     The local name of the element.
        //
        // Exceptions:
        //   System.InvalidOperationException:
        //     The writer is closed.
        public void WriteStartElement(string localName)
        {
            this.WriteStartElement(null, localName, null);
        }

        //
        // Summary:
        //     When overridden in a derived class, writes the specified start tag and associates
        //     it with the given namespace.
        //
        // Parameters:
        //   localName:
        //     The local name of the element.
        //
        //   ns:
        //     The namespace URI to associate with the element. If this namespace is already
        //     in scope and has an associated prefix, the writer automatically writes that
        //     prefix also.
        //
        // Exceptions:
        //   System.InvalidOperationException:
        //     The writer is closed.
        public string WriteStartElement(string localName, string ns)
        {
            return this.WriteStartElement(null, localName, ns);
        }

        //
        // Summary:
        //     When overridden in a derived class, writes the specified start tag and associates
        //     it with the given namespace and prefix.
        //
        // Parameters:
        //   localName:
        //     The local name of the element.
        //
        //   prefix:
        //     The namespace prefix of the element.
        //
        //   ns:
        //     The namespace URI to associate with the element.
        //
        // Exceptions:
        //   System.InvalidOperationException:
        //     The writer is closed.
        public string WriteStartElement(string prefix, string localName, string ns)
        {
            if (_WriteState == WriteState.Attribute)
                throw new InvalidOperationException("writer is closed");
            if (prefix == "xmlns")
                throw new ArgumentException("Prefix \"xmlns\" is reserved for use by XML.");
            if (localName == null || localName.Length == 0)
                throw new ArgumentException("The empty string '' is not a valid local name.");

            // Check to see if a started attribute need finished
            if (_WriteState == WriteState.Attribute)
                this.WriteEndAttribute();

            string localPrefix = "";
            string localNS = "";
            string childPrefix = "";

            if (prefix != null && prefix.Length == 0) prefix = null;

            // if prefix == null and ns == null, only localName will write
            // if prefix == null and ns != null, prefix is set to xmlns and ns will be writen
            // if prefix != null && ns == null, use prefix
            if (prefix != null && ns != null)
            {
                localPrefix = prefix + ":";
                localNS = " xmlns:" + prefix + "=" + "\"" + ns + "\"";
                childPrefix = prefix;
            }
            else if (prefix != null && ns == null)
                localPrefix = prefix + ":";
            else if (prefix == null && ns != null)
            {
                if (_ElementStack.Count > 0)
                {
                    if (((ElementInfo)_ElementStack.Peek()).NameSpaceName != ns)
                    {
                        childPrefix = "b" + _autoPrefixIndex++;
                        localNS = " xmlns:" + childPrefix + "=" + "\"" + ns + "\"";
                    }
                    else
                    {
                        prefix = ((ElementInfo)_ElementStack.Peek()).ChildNSPrefix;
                        if (prefix != null && prefix != "")
                        {
                            localPrefix = prefix + ":";
                            childPrefix = prefix;
                        }
                    }
                }
                else
                {
                    localNS = " xmlns" + "=" + "\"" + ns + "\"";
                }
            }
            else if (_ElementStack.Count > 0)
            {
                prefix = ((ElementInfo)_ElementStack.Peek()).ChildNSPrefix;
                if (prefix != null && prefix != "")
                {
                    localPrefix = prefix + ":";
                }
                else
                {
                    prefix = null;
                }
            }

            // Check to see if we need to close a previous start tag
            if (_ElementStack.Count > 0)
                if (((ElementInfo)_ElementStack.Peek()).State == WriteState.Element)
                    this.WriteRaw(">");

            // Write the element
            this.WriteRaw("<" + localPrefix + localName + localNS);

            // Create a new Element and Push the element onto the ElementStack
            ElementInfo currentElement = new ElementInfo();
            currentElement.Name = localName;
            currentElement.NSPrefix = prefix;
            currentElement.ChildNSPrefix = childPrefix;
            currentElement.NameSpaceName = ns;
            currentElement.IsEmpty = true;

            // Anytime a new Element is added the previous must have its last known state and IsEmpty
            // state updated to reflect we are adding Content and the element is no longer Empty
            if (_ElementStack.Count > 0)
            {
                ElementInfo ei = (ElementInfo)_ElementStack.Pop();
                ei.State = _WriteState = WriteState.Content;
                // Signal that this element has content
                ei.IsEmpty = false;
                _ElementStack.Push(ei);
            }

            // Update the local WriteState and new Element State
            currentElement.State = _WriteState = WriteState.Element;

            // Push the new element onto the stack
            _ElementStack.Push(currentElement);

            return prefix;
        }

        //
        // Summary:
        //     When overridden in a derived class, writes the given text content.
        //
        // Parameters:
        //   text:
        //     The text to write.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     The text string contains an invalid surrogate pair.
        public void WriteString(string text)
        {
            if (_WriteState == WriteState.Attribute || _WriteState == WriteState.Content)
                this.WriteRaw(text);
            else if (_WriteState == WriteState.Element)
            {
                this.WriteRaw(">" + text);

                // Set new write state
                ElementInfo ei = (ElementInfo)_ElementStack.Pop();
                ei.State = _WriteState = WriteState.Content;
                // Signal that this element has content
                ei.IsEmpty = false;
                _ElementStack.Push(ei);
            }

            return;
        }
    }

    public class XmlMemoryWriter : XmlWriter
    {
        MemoryStream _Stream;

        protected XmlMemoryWriter(MemoryStream stream) : base(stream)
        {
            _Stream = stream;
        }

        public MemoryStream InnerStream { get { return _Stream; } }

        public static XmlMemoryWriter Create()
        {
            return new XmlMemoryWriter(new MemoryStream());
        }

        protected override void Dispose(bool disposing)
        {
            _Stream = null;

            // _Stream disposed in XmlWriter dispose call
            base.Dispose(disposing);
        }

        public byte[] ToArray()
        {
            Flush();
            return _Stream.ToArray();
        }
    }
}
