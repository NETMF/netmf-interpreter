using System.Runtime.CompilerServices;
using System;
using System.Security.Cryptography;
using System.Collections;

namespace Microsoft.SPOT.Cryptoki
{
    /// <summary>
    /// Defines a container class for a Cryptoki session object.
    /// </summary>
    public abstract class SessionContainer : IDisposable
    {
        protected readonly Session m_session;
        protected readonly bool    m_ownsSession;
        protected bool m_isDisposed;
        protected bool m_isSessionClosing;

        private SessionContainer(){}

        /// <summary>
        /// Creates a session container object with the specified session context.
        /// </summary>
        /// <param name="session">The Cryptoki session context.</param>
        /// <param name="ownsSession">Determines if the container disposes the session object.</param>
        protected SessionContainer(Session session, bool ownsSession)
        {
            m_ownsSession = ownsSession;
            m_session = session;

            if (!ownsSession)
            {
                session.AddSessionObject(this);
            }
        }

        /// <summary>
        /// Determines if the session container has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return m_isDisposed; }
        }

        /// <summary>
        /// Gets the Cryptoki session context.
        /// </summary>
        public Session Session
        {
            get { return m_session; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SessionContainer()
        {
            Dispose(false);
        }

        internal void CloseManaged(bool bClosingSession)
        {
            try
            {
                m_isSessionClosing = true;
                Dispose();
            }
            finally
            {
                m_isSessionClosing = false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_isDisposed) throw new ObjectDisposedException();

                if (!m_isSessionClosing)
                {
                    m_session.RemoveSessionObject(this);
                }

                if (m_ownsSession)
                {
                    m_session.Dispose();
                }

                m_isDisposed = true;
            }
        }

        #endregion
    }

    /// <summary>
    /// Defines the Cryptoki session context.
    /// </summary>
    public class Session : IDisposable
    {
        /// <summary>
        /// Defines the user type for session login.
        /// </summary>
        public enum UserType
        {
            SecurityOfficer,
            User,
            ContextSpecific,
        }

        /// <summary>
        /// Defines the session state.
        /// </summary>
        public enum State
        {
            ReadOnlyPublic,
            ReadOnlyUser,
            ReadWritePublic,
            ReadWriteUser,
            ReadWriteSecurityOfficer,
        }

        /// <summary>
        /// Defines the session open properties
        /// </summary>
        [FlagsAttribute]
        public enum SessionFlag
        {
            ReadOnly  = 0x0000,
            ReadWrite = 0x0002,
        }

        /// <summary>
        /// Defines the session information.
        /// </summary>
        public class SessionInfo
        {
            public ulong SlotID;
            public State State;
            public SessionFlag Flag;
            public ulong DeviceError;
        }

        private int  m_handle;
        private bool m_disposed;
        private int m_maxProcessingBytes;
        private ArrayList m_objects;

        /// <summary>
        /// Creates a session object with specified service provider name and target mechanisms.
        /// </summary>
        /// <param name="providerName">Crypto service provider name.</param>
        /// <param name="mechanisms">Target mechanism types.</param>
        public Session(string providerName, params MechanismType[] mechanisms)
        {
            m_handle = -1;
            m_maxProcessingBytes = 1024;
            m_objects = new ArrayList();
            InitSession(providerName, mechanisms);
        }

        /// <summary>
        /// Adds a reference to a cryptoki object opened in the session context.
        /// </summary>
        /// <param name="obj">The cryptoki object to be referenced.</param>
        internal void AddSessionObject(SessionContainer obj)
        {
            lock (m_objects)
            {
                m_objects.Add(obj);
            }
        }

        /// <summary>
        /// Removes a session object reference.
        /// </summary>
        /// <param name="obj">The cryptoki object to be dereferenced.</param>
        internal void RemoveSessionObject(SessionContainer obj)
        {
            lock (m_objects)
            {
                m_objects.Remove(obj);
            }
        }

        /// <summary>
        /// Gets the session handle in byte array format.
        /// </summary>
        public byte[] Handle
        {
            get { return Utility.ConvertToBytes(m_handle); }
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void InitSession(string providerName, MechanismType[] mechanisms);

        /// <summary>
        /// Logs into the session.
        /// </summary>
        /// <param name="userType">User type</param>
        /// <param name="pin">Pin value</param>
        /// <returns>true if the login was successfull, false otherwise.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern bool Login(UserType userType, string pin);

        /// <summary>
        /// Logs out of the session.
        /// </summary>
        /// <returns>true if the logout was successfull, false otherwise.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern bool Logout();

        /// <summary>
        /// Gets the session informration.
        /// </summary>
        /// <param name="info">Session information to be assigned.</param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern void GetSessionInfo(ref SessionInfo info);

        /// <summary>
        /// Intializes the login pin.
        /// </summary>
        /// <param name="pin">Pin value</param>
        /// <returns>true if the pin intialization was successfull, false otherwise.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern bool InitializePin(string pin);

        /// <summary>
        /// Sets a new pin for the login.
        /// </summary>
        /// <param name="oldPin">The old pin value.</param>
        /// <param name="newPin">The new pin value.</param>
        /// <returns></returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern bool SetPin(string oldPin, string newPin);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void Close();

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Session()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_disposed || m_handle == -1) throw new ObjectDisposedException();

                lock (m_objects)
                {
                    for (int i = 0; i < m_objects.Count; i++)
                    {
                        SessionContainer disp = (SessionContainer)m_objects[i];

                        disp.CloseManaged(true);
                    }
                    m_objects.Clear();
                }
            }

            //if (m_mechanism.Parameter != null)
            //{
            //    Array.Clear(m_mechanism.Parameter, 0, m_mechanism.Parameter.Length);
            //}

            if (!m_disposed && m_handle != -1)
            {
                Close();
            }

            // this is only added so the compiler wont complain that this member is never used
            if (m_maxProcessingBytes != 1024)
            {
                m_maxProcessingBytes = 1024;
            }

            m_disposed = true;
            m_handle   = -1;
        }

        #endregion
 
    }

}