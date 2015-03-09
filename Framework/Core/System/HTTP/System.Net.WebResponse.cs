////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////namespace System.Net
namespace System.Net
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Provides a response from a Uniform Resource Identifier (URI).  This is
    /// an abstract class.
    /// </summary>
    /// <remarks>
    /// This is the abstract base class for all <itemref>WebResponse</itemref>
    /// objects.
    /// </remarks>
    public abstract class WebResponse : MarshalByRefObject, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <itemref>WebResponse</itemref>
        /// class.
        /// </summary>
        protected WebResponse()
        {
        }

        /// <summary>
        /// When overridden in a descendant class, gets or sets the content
        /// length of data being received.
        /// </summary>
        /// <value>The number of bytes returned from the Internet
        /// resource.</value>
        public virtual long ContentLength
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

        /// <summary>
        /// When overridden in a derived class, gets or sets the content type of
        /// the data being received.
        /// </summary>
        /// <value>A string that contains the content type of the
        /// response.</value>
        public virtual string ContentType
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

        /// <summary>
        /// When overridden in a descendant class, returns the data stream from
        /// the Internet resource.
        /// </summary>
        /// <returns>An instance of the <see cref="System.IO.Stream"/> class for
        /// reading data from the Internet resource.</returns>
        public virtual Stream GetResponseStream()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class, gets the URI of the Internet
        /// resource that actually responded to the request.
        /// </summary>
        /// <value>An instance of the <see cref="System.Uri"/> class that
        /// contains the URI of the Internet resource that actually responded to
        /// the request.</value>
        /// <remarks>
        /// This property gets the final Response URI, that includes any changes
        /// that may have transpired from the orginal request.
        /// </remarks>
        public virtual Uri ResponseUri
        {                           // read-only
            get
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets a collection of header
        /// name-value pairs associated with this request.
        /// </summary>
        /// <returns>An instance of the
        /// <see cref="System.Net.WebHeaderCollection"/> class that contains
        /// header values associated with this response.</returns>
        public virtual WebHeaderCollection Headers
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        ~WebResponse()
        {
            Dispose(false);
        }

        /// <summary>
        /// When overridden by a descendant class, closes the response stream.
        /// </summary>
        public virtual void Close()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            Close();
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    } // class WebResponse
} // namespace System.Net


