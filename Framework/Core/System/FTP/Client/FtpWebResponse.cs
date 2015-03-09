using System.IO;

namespace System.Net
{
    /// <summary>
    /// Only contains a stream to write data
    /// </summary>
    public class FtpWebResponse : WebResponse
    {
        private Stream m_ResponseStream = null;
        //private FtpWebRequest m_Request = null;

        internal FtpWebResponse()
        {
        }

        internal FtpWebResponse(Stream stream)
        {
            m_ResponseStream = stream;
        }

        public override Stream GetResponseStream()
        {
            return m_ResponseStream;
        }

        public override void Close()
        {
            if (m_ResponseStream != null)
            {
                m_ResponseStream.Close();
            }
            //m_Request.Close();
        }

    }
}
