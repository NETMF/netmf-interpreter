using System.IO;
using System.Net;
using System;

namespace Microsoft.SPOT.Net
{
    /// <summary>
    /// The ftp request
    /// </summary>
    public class FtpListenerRequest
    {
        // Fields
        private string m_Method = null;                         // Ftp method of this request
        private string m_Parameter1 = null;                     // the first paramenter of the request
        private bool m_NeedRequestStream = false;               // whether the request stream is necessary for this method
        private bool m_IsStreamExists = false;                  // whether the request stream has been built
        private Stream m_RequestStream = null;                  // the request stream reference
        private FtpResponseStream m_InputStream = null;         // the response stream
        IDataManager m_Sender = null;

        // Methods
        internal FtpListenerRequest(string method, string parameter, IDataManager sender)
        {
            m_Method = method;
            m_Parameter1 = parameter;
            m_Sender = sender;
            if (method == WebRequestMethods.Ftp.UploadFile)
            {
                m_NeedRequestStream = true;
            }
        }

        // Properties
        public string Method
        {
            get { return m_Method; }
        }

        /// <summary>
        /// The parameter of the request command
        /// </summary>
        public string QueryString
        {
            get { return m_Parameter1; }
        }

        /// <summary>
        /// The stream that is passed to the response
        /// </summary>
        public FtpResponseStream InputStream
        {
            get
            {
                if (!m_NeedRequestStream)
                {
                    throw new NotSupportedException("This request does not need request stream.");
                }
                if (!m_IsStreamExists)
                {
                    m_Sender.DataChannelEstablished.WaitOne();          // block until data stream ready
                    if (!m_Sender.IsDataStreamAvailable)
                    {
                        return null;
                    }
                    m_RequestStream = m_Sender.DataStream;
                    m_InputStream = new FtpResponseStream(null, m_RequestStream);
                    m_IsStreamExists = true;
                }
                return m_InputStream;
            }
        }

        internal bool NeedRequestStream
        {
            get { return m_NeedRequestStream; }
        }

    }
}
