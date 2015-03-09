
namespace System.Net
{
    /// <summary>
    /// Creator for ftp webrequest
    /// </summary>
    internal class FTPWebRequestCreator : IWebRequestCreate
    {
        /// <summary>
        /// Constructor
        /// </summary>
        FTPWebRequestCreator()
        {
        }

        /// <summary>
        /// Static constructor
        /// </summary>
        static FTPWebRequestCreator()
        {
            WebRequest.RegisterPrefix("ftp:", new FTPWebRequestCreator());
        }

        #region IWebRequestCreate Members

        public WebRequest Create(Uri uri)
        {
            return new FtpWebRequest(uri);
        }

        #endregion
    }
}
