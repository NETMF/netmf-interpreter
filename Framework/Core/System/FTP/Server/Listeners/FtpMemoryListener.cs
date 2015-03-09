using System.Collections;
using System.IO;
using System.Threading;
using System.Net;

namespace Microsoft.SPOT.Net.Ftp
{
    public sealed class FtpMemoryListener : FtpListener
    {
        private Thread m_worker = null;
        private ArrayList m_FileInfo = null;
        private ArrayList m_Logging = null;

        public FtpMemoryListener()
            : base(true)
        {
            m_FileInfo = new ArrayList();
            m_Logging = new ArrayList();
        }

        public override void Start()
        {
            m_worker = new Thread(WorkerThread);
            m_worker.Start();
            base.Start();
        }

        public void AddFile(FileInfo info)
        {
            m_FileInfo.Add(info);
        }

        private void WorkerThread()
        {
            for (; ; )
            {
                FtpListenerContext context = this.GetContext();
                string log = "Get new context. Request type: " + context.Request.Method + " Query string: " + context.Request.QueryString;
                m_Logging.Add(log);
                FtpResponseStream stream = context.Response.OutputStream;
                switch (context.Request.Method)
                {
                    case WebRequestMethodsEx.Ftp.ChangeDirectory:
                        bool accepted = false;
                        foreach (string prefix in m_Prefixes)
                        {
                            if (context.Request.QueryString == prefix)
                            {
                                accepted = true;
                                break;
                            }
                        }
                        if (accepted)
                            context.Response.StatusCode = FtpStatusCode.FileActionOK;
                        else
                            context.Response.StatusCode = FtpStatusCode.ActionNotTakenFileUnavailable;
                        break;

                    case WebRequestMethods.Ftp.ListDirectoryDetails:
                        foreach (FileInfo info in m_FileInfo)
                        {
                            stream.Write(info);
                        }
                        stream.Write(new FileInfo("\\log"));
                        context.Response.StatusCode = FtpStatusCode.ClosingData;
                        break;
                    case WebRequestMethods.Ftp.GetFileSize:
                        if (context.Request.QueryString.IndexOf("log") >= 0)
                        {
                            int length = 0;
                            foreach (string s in m_Logging)
                            {
                                length += s.Length + 2;
                            }
                            stream.Write(length.ToString());
                            context.Response.StatusCode = FtpStatusCode.FileStatus;
                        }
                        else
                        {
                            context.Response.StatusCode = FtpStatusCode.ActionNotTakenFileUnavailable;
                        }
                        break;
                    default:
                        context.Response.StatusCode = FtpStatusCode.ActionAbortedLocalProcessingError;
                        break;
                }
                stream.Close();
            }
        }
    }
}
