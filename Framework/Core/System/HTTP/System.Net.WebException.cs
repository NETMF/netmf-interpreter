////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{
    using System;

    /*++
    Copyright (c) 2000 Microsoft Corporation

    Abstract:
        Contains the defintion for the WebException object. This is a subclass of
        Exception that contains a WebExceptionStatus and possible a reference to a
        WebResponse.

    Original Author:
        Henry Sanders (henrysa) 03-Feb-2000

    Environment:
        COM+ Managed Code

    Revision History:
        03-Feb-2000 henrysa
            Created

     *  Feb 2009 - Updated for Micro Framework.
    --*/

    /// <summary>
    /// Defines the exception that is thrown by
    /// <see cref="System.Net.WebRequest"/> instances when an error occurs.
    /// </summary>
    /// <remarks>
    /// This class is a subclass of <itemref>InvalidOperationException</itemref>
    /// that contains a <itemref>WebExceptionStatus</itemref> and possibly a
    /// reference to a <itemref>WebResponse</itemref>.  The
    /// <itemref>WebResponse</itemref> is only present if there is a response
    /// from the remote server.
    /// </remarks>
    public class WebException : InvalidOperationException
    {

        private WebExceptionStatus m_Status;
        private WebResponse m_Response;

        /// <summary>
        /// The default constructor.
        /// </summary>
        public WebException()
        {

        }

        /// <summary>
        /// Constructs a <itemref>WebException</itemref> based on the specified
        /// message string.
        /// </summary>
        /// <param name="message">The message string for the exception.</param>
        public WebException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructs a <itemref>WebException</itemref> based on the specified
        /// message string and inner exception.
        /// </summary>
        /// <param name="message">The message string for the exception.</param>
        /// <param name="innerException">The exception that caused this
        /// exception.</param>
        public WebException(string message, Exception innerException) :
            base(message, innerException)
        {

        }

        /// <summary>
        /// Constructs a <itemref>WebException</itemref> based on the specified
        /// message string and <itemref>WebExceptionStatus</itemref>.
        /// </summary>
        /// <param name="message">The message string for the exception.</param>
        /// <param name="status">The network status of the exception.</param>
        public WebException(string message, WebExceptionStatus status)
            : base(message)
        {
            m_Status = status;
        }

        /// <summary>
        /// Constructs a <itemref>WebException</itemref> based on the specified
        /// message string, inner exception,
        /// <see cref="System.Net.WebExceptionStatus"/>, and
        /// <see cref="System.Net.WebResponse"/>.
        /// </summary>
        /// <param name="message">Message string for exception.</param>
        /// <param name="inner">The exception that caused this exception.
        /// </param>
        /// <param name="status">The network status of the exception.</param>
        /// <param name="response">The <itemref>WebResponse</itemref> we have.
        /// </param>
        public WebException(string message,
                            Exception inner,
                            WebExceptionStatus status,
                            WebResponse response)
            : base(message, inner)
        {
            m_Status = status;
            m_Response = response;
        }

        /// <summary>
        /// Gets the <itemref>WebExceptionStatus</itemref> code.
        /// </summary>
        /// <value>One of the <b>WebExceptionStatus</b> values.</value>
        public WebExceptionStatus Status
        {
            get
            {
                return m_Status;
            }
        }

        /// <summary>
        /// Gets the response that the remote host returned.
        /// </summary>
        /// <value>If a response is available from the Internet resource, a
        /// <itemref>WebResponse</itemref> instance that contains the error
        /// response from an Internet resource; otherwise,
        /// <itemref>null</itemref>.</value>
        public WebResponse Response
        {
            get
            {
                return m_Response;
            }
        }

    }; // class WebException

} // namespace System.Net


