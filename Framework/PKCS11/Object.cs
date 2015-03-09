using System.Runtime.CompilerServices;
using System;
using System.Collections;
using System.Text;

namespace Microsoft.SPOT.Cryptoki
{
    /// <summary>
    /// Defines a Cryptoki object class.
    /// </summary>
    public class CryptokiObject : SessionContainer
    {
        protected int m_handle;

        protected CryptokiObject(Session session)
            : base(session, false)
        {
        }

        internal Session SessionValue
        {
            get { return m_session; }
        }

        /// <summary>
        /// Creates a object in the given Cryptoki session context with specified object atrributes.
        /// </summary>
        /// <param name="session">The Cryptoki session context.</param>
        /// <param name="template">The object attribute template.</param>
        /// <returns>The cryptoki object created.</returns>
        public static CryptokiObject CreateObject(Session session, CryptokiAttribute[] template)
        {
            CryptokiObject ret = CreateObjectInternal(session, template);

            session.AddSessionObject(ret);

            return ret;
        }

        /// <summary>
        /// Copies the cryptoki object.
        /// </summary>
        /// <param name="template">The object attribute template.</param>
        /// <returns>The new cryptoki object.</returns>
        public CryptokiObject Copy(CryptokiAttribute[] template)
        {
            CryptokiObject ret = CopyInternal(template);

            m_session.AddSessionObject(ret);

            return ret;
        }

        /// <summary>
        /// Saves the Cryptoki object as the given name to the specified store location.
        /// </summary>
        /// <param name="name">Name to save the crypto object as.</param>
        /// <param name="location">Location to store the object.</param>
        /// <param name="level">Secure storage priority level.</param>
        /// <returns>true if the save was successful, false otherwise.</returns>
        public virtual bool Save(string name, string location, SecureStorageLevel level)
        {
            CryptokiAttribute[] attribs = new CryptokiAttribute[]
            {
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.Label, UTF8Encoding.UTF8.GetBytes(location)), 
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.ObjectID, UTF8Encoding.UTF8.GetBytes(name)),
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.Persist, Utility.ConvertToBytes(1)),
            };

            return this.SetAttributeValues(attribs);
        }

        /// <summary>
        /// Deletes the Cryptoki object from the specified storage location.
        /// </summary>
        /// <param name="location">The storage location.</param>
        /// <returns>true if the delete was successful, false otherwise.</returns>
        public virtual bool Delete(string location)
        {
            CryptokiAttribute[] attribs = new CryptokiAttribute[]
            {
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.Label, UTF8Encoding.UTF8.GetBytes(location)), 
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.Persist, Utility.ConvertToBytes(0)),
            };

            return this.SetAttributeValues(attribs);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern CryptokiObject CreateObjectInternal(Session session, CryptokiAttribute[] template);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern CryptokiObject CopyInternal(CryptokiAttribute[] template);

        /// <summary>
        /// Gets the values for the specified object attributes.
        /// </summary>
        /// <param name="template">The attribute values to retrieve.</param>
        /// <returns>true if all the values were retrieved, false otherwise.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern bool GetAttributeValues(ref CryptokiAttribute[] template);

        /// <summary>
        /// Sets the values for the specified object attributes.
        /// </summary>
        /// <param name="template">The attribute values to set.</param>
        /// <returns>true if all the values were set, false otherwise.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern bool SetAttributeValues(CryptokiAttribute[] template);

        /// <summary>
        /// Gets the size of the cryptoki object, in bytes.
        /// </summary>
        public virtual extern int Size
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void Destroy();

        protected override void Dispose(bool disposing)
        {
            if (m_isDisposed) return;

            try
            {
                if (m_session != null)
                {
                    Destroy();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }

    /// <summary>
    /// Defines an object enueration for searching for Cryptoki objects within a session context.
    /// </summary>
    public class FindObjectEnum : IDisposable
    {
        Session m_session;

        /// <summary>
        /// Creates a FindObjectEnum object with the specified session context and object attribute template.
        /// </summary>
        /// <param name="session">The Cryptoki session context.</param>
        /// <param name="template">The object attribute search criteria.</param>
        public FindObjectEnum(Session session, CryptokiAttribute[] template)
        {
            m_session = session;
            FindObjectsInit(template);
        }

        /// <summary>
        /// Gets the next 'count' items that match the search criteria.
        /// </summary>
        /// <param name="count">The max number of objects to return.</param>
        /// <returns>An array of objects matching the search criteria.</returns>
        public CryptokiObject[] GetNext(int count)
        {
            return FindObjects(count);
        }

        /// <summary>
        /// Finalizes the search operation.
        /// </summary>
        public void Close()
        {
            FindObjectsFinal();
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        protected extern void FindObjectsInit(CryptokiAttribute[] template);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        protected extern CryptokiObject[] FindObjects(int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        protected extern void FindObjectsFinal();

        /// <summary>
        /// Gets the number of items matching the search criteria.
        /// </summary>
        public extern int Count
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Close();
        }

        ~FindObjectEnum()
        {
            Dispose(false);
        }

        #endregion
    }
}
