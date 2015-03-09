using System.Collections;
using System.IO;
using System.Threading;
using System;
using System.Net;

namespace Microsoft.SPOT.Net.Ftp
{
    public class FtpDefaultListener : FtpListener
    {

        public FtpDefaultListener()
            : base(false)
        {
        }

        public override void Start()
        {
            m_Prefixes.Add("/");
            base.Start();

            m_worker = new Thread(Run);
            m_worker.Start();
        }

        public override void Stop()
        {
            base.Stop();
            m_ContextQueue.Clear();
            m_ContextQueueLock.Set();
            try
            {
                m_worker.Join(500);
            }
            catch
            {
            }
            m_worker = null;       
        }

        private Thread m_worker;
        protected virtual void Run()
        {
            for (; ; )
            {
                FtpListenerContext context = this.GetContext();
                if (context == null)
                {
                    // the listener has been closed
                    return;
                }
                FtpResponseStream stream = context.Response.OutputStream;
                switch (context.Request.Method)
                {
                    case WebRequestMethodsEx.Ftp.ChangeDirectory:
                        context.Response.StatusCode = DirectoryExists(context.Request.QueryString) ? FtpStatusCode.FileActionOK : FtpStatusCode.ActionNotTakenFileUnavailable;
                        break;
                    case WebRequestMethods.Ftp.ListDirectory:
                    case WebRequestMethods.Ftp.ListDirectoryDetails:
                        WriteDirInfo(context.Request.QueryString, stream);
                        context.Response.StatusCode = FtpStatusCode.ClosingData;
                        break;
                    case WebRequestMethods.Ftp.RemoveDirectory:
                        context.Response.StatusCode = FtpStatusCode.ActionNotTakenFileUnavailable;
                        break;
                    default:
                        context.Response.StatusCode = FtpStatusCode.ActionAbortedLocalProcessingError;
                        break;
                }
                stream.Close();
            }
        }

        private bool DirectoryExists(String path)
        {
            foreach (FtpListener item in FtpListenerManager.GetFtpManager().Listeners)
            {
                foreach (String prefix in item.Prefixes)
                {
                    if (prefix.Length >= path.Length && path.Equals(prefix.Substring(0, path.Length)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void WriteDirInfo(string path, FtpResponseStream stream)
        {
            ArrayList sent = new ArrayList();

            foreach (FtpListener item in FtpListenerManager.GetFtpManager().Listeners)
            {
                foreach (String prefix in item.Prefixes)
                {
                    if (prefix.Length >= path.Length && path.Equals(prefix.Substring(0, path.Length)))
                    {
                        //this is "our" directory
                        String rest = prefix.Substring(path.Length);

                        int idx = rest.IndexOf("/");
                        if (idx >= 0)
                        {
                            rest = rest.Substring(0, idx);
                        }

                        if (rest.Length < 1) continue;

                        bool alreadySent = false;
                        foreach (var itemSent in sent)
                        {
                            if (rest.Equals(itemSent))
                            {
                                alreadySent = true;
                                break;
                            }
                        }

                        if (alreadySent) continue;

                        sent.Add(rest);

                        DirectoryInfo dir = new DirectoryInfo(rest);
                        stream.Write(new DirectoryInfo(rest));
                    }
                }
            }
        }
    }
}
