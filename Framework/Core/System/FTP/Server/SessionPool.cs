using System;
using System.Collections;

namespace Microsoft.SPOT.Net.Ftp
{
    // TODO: deal with concurrent issue
    /// <summary>
    /// A collection to store sessions
    /// </summary>
    internal class SessionPool : ICollection
    {
        // Fields
        private int m_PoolCapacity = 32;                    // the capacity of the session pool
        private int m_Count = 0;                            // the number of sessions 
        private FtpListenerSession[] m_Sessions = null;     // the underlying storage
        private int m_LastSessionId = 0;                    // the last added index of sessions
        private object m_SyncRoot = null;                   // the lock

        // Methods
        public SessionPool()
        {
            m_Sessions = new FtpListenerSession[m_PoolCapacity];
        }

        /// <summary>
        /// Specifying the capacity of this session pool
        /// </summary>
        /// <param name="capacity"></param>
        public SessionPool(int capacity)
            : this()
        {
            if (capacity < 0)
            {
                throw new ArgumentException("The capacity should never less than 0.");
            }
            m_PoolCapacity = capacity;
        }

        /// <summary>
        /// Add a new session into the session pool and add an unique id to it
        /// </summary>
        /// <param name="session">the session to be added</param>
        /// <returns>the id of that session, if no free id then return -1</returns>
        public int AddSession(FtpListenerSession session)
        {
            int newSessionId = 0;
            if (m_Sessions == null)
            {
                throw new NullReferenceException("Session pool initialization failed.");
            }
            for (int i = 0; i <= m_PoolCapacity; i++)
            {
                // start searching free id from previous result
                newSessionId = (i + m_LastSessionId) % m_PoolCapacity;
                if (m_Sessions[newSessionId] == null)    // TODO: check the status of the session
                {
                    session.SessionId = newSessionId;
                    m_Sessions[newSessionId] = session;
                    m_LastSessionId = newSessionId;
                    m_Count++;
                    return newSessionId;
                }
            }
            return -1;
        }

        /// <summary>
        /// Find the session id of that session
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public int SearchSession(FtpListenerSession session)
        {
            int sessionId = -1;

            for (int i = 0; i < m_PoolCapacity; i++)
            {
                if (m_Sessions[i] == session)
                {
                    sessionId = i;
                    break;
                }
            }
            return sessionId;
        }

        /// <summary>
        /// Remove a session from the session pool, no matter whether it exists or not
        /// </summary>
        /// <param name="session"></param>
        public void RemoveSession(FtpListenerSession session)
        {
            for (int i = 0; i < m_PoolCapacity; i++)
            {
                if (m_Sessions[i] == session)
                {
                    m_Sessions[i] = null;
                    m_Count--;
                    break;
                }
            }
            return;
        }

        /// <summary>
        /// Remove a session from the session pool based on session id
        /// </summary>
        /// <param name="idx"></param>
        public void RemoveAt(int idx)
        {
            if (idx >= m_PoolCapacity)
            {
                throw new IndexOutOfRangeException("Out of the range of the session pool");
            }
            else
            {
                m_Sessions[idx].Dispose();
                m_Sessions[idx] = null;
                m_Count--;
            }
            return;
        }

        public void Clear()
        {
            for (int i = 0; i < m_PoolCapacity; i++)
            {
                if (m_Sessions[i] != null)
                {
                    m_Sessions[i].Dispose();
                    m_Sessions[i] = null;
                    m_Count--;
                }
            }
            return;
        }

        public void CheckAlive()
        {
            for (int i = 0; i < m_PoolCapacity; i++)
            {
                if (m_Sessions[i] != null && !m_Sessions[i].IsAlive)
                {
                    m_Sessions[i].Dispose();
                    m_Sessions[i] = null;
                    m_Count--;
                }
            }
            return;
        }

        #region ICollection Members

        public virtual int Count
        {
            get { return m_Count; }
        }

        public virtual int Capacity
        {
            get { return m_PoolCapacity; }
        }

        public virtual void CopyTo(Array array, int index)
        {
            Array.Copy(m_Sessions, 0, array, index, m_Count);
        }

        public virtual bool IsSynchronized
        {
            get { return false; }
        }

        public virtual object SyncRoot
        {
            get
            {
                if (this.m_SyncRoot == null)
                {
                    m_SyncRoot = new object();
                }
                return m_SyncRoot;
            }
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return m_Sessions.GetEnumerator();
        }

        #endregion
    }
}
