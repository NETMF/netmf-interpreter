using System.Collections;
using System.Threading;
using System;
using Microsoft.SPOT.Net.Ftp;
using System.Net;

namespace Microsoft.SPOT.Net
{
    /// <summary>
    /// The base class for all Ftp listeners
    ///     implemented all basic functionalities like context queue management,
    ///     authentication events
    /// </summary>
    public class FtpListener : IDisposable
    {
        // Fields
        protected Queue m_ContextQueue = null;                  // context queue         
        protected AutoResetEvent m_ContextQueueLock
            = new AutoResetEvent(false);                        // event for context read/write syncronization
        protected ArrayList m_Prefixes = null;                  // list of listening prefixes
        private bool m_IsNeedAttach = true;                     // indicate whether this listener should be owned by the listener manager, if yes then the value should be true
        private bool m_IsRunning = false;                       // flag to show whether this listener is listening to requests

        public static event UserAuthenticator AuthenticationEvent;
                                                                // User authentication event, handled by API user's registered handlers

        // Methods
        public FtpListener()
            : this(true)
        {

        }

        /// <summary>
        /// Constructor with indication of flags whether it should be registerd in the listener manager
        /// </summary>
        /// <param name="registerWithListenerManger"></param>
        internal FtpListener(bool registerWithListenerManger)
        {
            m_Prefixes = new PrefixList();
            m_ContextQueue = new Queue();
            m_IsNeedAttach = registerWithListenerManger;
        }

        /// <summary>
        /// Start the listener
        /// </summary>
        public virtual void Start()
        {
            if (m_IsNeedAttach)
            {
                // Attach the listen to the listener manager, now the manager will take in control of this listener
                FtpListenerManager.AddListener(this);
            }
            FtpListenerManager.GetFtpManager().Start();
            m_IsRunning = true;
        }

        /// <summary>
        /// Stop the listener
        /// </summary>
        public virtual void Stop()
        {
            m_IsRunning = false;
            if (m_IsNeedAttach)
            {
                while (m_ContextQueue.Count > 0)
                {
                    // get all pending contexts and send them back to the listener manager
                    // the manager will add these context to some other listener's context queue
                    m_ContextQueue.Dequeue();
                    FtpListenerManager.GetFtpManager().AddContext(m_ContextQueue.Dequeue() as FtpListenerContext);
                }
                FtpListenerManager.RemoveListener(this);
            }
        }

        /// <summary>
        /// Get the oldest context from the context queue
        ///     the method would be block until there is a pending context
        /// </summary>
        /// <returns></returns>
        public FtpListenerContext GetContext()
        {
            FtpListenerContext context = null;
            if (m_ContextQueue == null)
            {
                throw new NullReferenceException("Context queue is empty");
            }
            // block the method until some context arrived
            m_ContextQueueLock.WaitOne();
            lock (m_ContextQueue.SyncRoot)
            {
                if (m_ContextQueue.Count > 0)
                    context = m_ContextQueue.Dequeue() as FtpListenerContext;
                // otherwise a null context will be returned;
            }
            return context;
        }

        /// <summary>
        /// Check whether the prefixes of this listener covers the input network path
        /// </summary>
        /// <param name="path"></param>
        /// <returns>the length of that matching prefix</returns>
        internal int CheckPrefix(string path)
        {
            foreach (string prefix in m_Prefixes)
            {
                if (path.Length >= prefix.Length && path.Substring(0, prefix.Length) == prefix)
                {
                    return prefix.Length;
                }
            }
            return -1;
        }
        
        /// <summary>
        /// Add a context to the context queue and raise a reset event
        /// </summary>
        /// <param name="context"></param>
        internal void AddContext(FtpListenerContext context)
        {
            if (m_IsRunning)
            {
                if (m_ContextQueue == null)
                {
                    m_ContextQueue = new Queue();
                }
                lock (m_ContextQueue.SyncRoot)
                {
                    m_ContextQueue.Enqueue(context);
                }
                m_ContextQueueLock.Set();
            }
            else
            {
                // reject all context if the listener is not running
                context.Response.StatusCode = FtpStatusCode.ActionAbortedLocalProcessingError;
                context.Response.OutputStream.Close();
            }

        }

        /// <summary>
        /// Static method to raise an authentication event from sessions
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        internal static void RaiseEvent(object o, UserAuthenticatorArgs e)
        {
            AuthenticationEvent(o, e);
        }

        /// <summary>
        /// TODO: a default anthentication handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void AnonymousAuthenticator(object sender, UserAuthenticatorArgs e)
        {
            if (e.User == "anonymous")
            {
                Logging.Print("hi anonymous");
                e.Result = UserAuthenticationResult.Approved;
            }
        }

        // Property
        /// <summary>
        /// List of prefixes that the listener is listening to
        /// </summary>
        public ArrayList Prefixes
        {
            get { return m_Prefixes; }
        }

        /// <summary>
        /// Flag indicates whether the listener is listening to contexts or not
        /// </summary>
        public bool IsListening
        {
            get { return m_IsRunning; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            this.Stop();
        }

        #endregion

    }

}
