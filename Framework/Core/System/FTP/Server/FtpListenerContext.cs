using Microsoft.SPOT.Net.Ftp;

namespace Microsoft.SPOT.Net
{
    /// <summary>
    /// A Context containing an ftp request and a response scratch that should be completed by listeners
    /// </summary>
    public class FtpListenerContext
    {
        /// Fields
        private FtpListenerRequest m_ClientRequest = null;              // listener request object
        private FtpListenerResponse m_ResponseToClient = null;          // listener response object
        private bool m_IsResponseExists = false;                        // "response ready" flag 
        private FtpListenerSession m_Session = null;                    // reference back to the session it belongs to

        // Methods
        internal FtpListenerContext(FtpListenerSession session, FtpListenerRequest request)
        {
            m_ResponseToClient = new FtpListenerResponse(request, session);
            m_ClientRequest = request;
            m_Session = session;
        }

        // Properties
        /// <summary>
        /// The request of this context
        /// </summary>
        public FtpListenerRequest Request
        {
            get { return m_ClientRequest; }
        }

        /// <summary>
        /// The user who made the request
        /// </summary>
        public UserInfo User
        {
            get { return m_Session.User; }
        }

        /// <summary>
        /// The response of this context
        /// </summary>
        public FtpListenerResponse Response
        {
            get
            {
                if (!m_IsResponseExists)
                {
                    m_ResponseToClient = new FtpListenerResponse(m_ClientRequest, m_Session);
                    m_IsResponseExists = true;
                }
                return m_ResponseToClient;
            }
        }
    }
}
