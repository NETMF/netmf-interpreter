//
// X509Store.cs
//

namespace System.Security.Cryptography.X509Certificates 
{
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using Microsoft.SPOT.Cryptoki;

    [Flags]
    // this enum defines the Open modes. Read/ReadWrite/MaxAllowed are mutually exclusive.
    public enum OpenFlags 
    {
        ReadOnly         = 0x00,
        ReadWrite        = 0x01,
        MaxAllowed       = 0x02,
        OpenExistingOnly = 0x04,
        IncludeArchived  = 0x08
    }

    public enum StoreName 
    {
        Disallowed,             // revoked certificates.
        My,                     // personal certificates.
        CA,                     // trusted root CAs.
    }

    /// <summary>
    /// Represents an X.509 store, which is a physical store where certificates are persisted and managed. This class cannot be inherited.
    /// </summary>
    public sealed class X509Store 
    {
        private string m_storeName;
        private X509Certificate2Collection m_certs;
        private Session m_session;

        /// <summary>
        /// Initializes a new instance of the X509Store class using the personal certificates of the current user store.
        /// </summary>
        /// <param name="session">Cryptoki session for which this store is to be used.</param>
        public X509Store (Session session) : this(session, StoreName.My) 
        {
        }

        /// <summary>
        /// Initializes a new instance of the X509Store class using the specified StoreName value.
        /// </summary>
        /// <param name="session">Cryptoki session for which this store is to be used.</param>
        /// <param name="storeName">One of the enumeration values that specifies the name of the X.509 certificate store.</param>
        public X509Store (Session session, StoreName storeName) 
        {
            m_session = session;
            switch (storeName) 
            {
                case StoreName.Disallowed:
                    m_storeName = "Disallowed";
                    break;
                case StoreName.My:
                    m_storeName = "My";
                    break;
                case StoreName.CA:
                    m_storeName = "CA";
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// Gets the name of the X.509 certificate store.
        /// </summary>
        public string Name 
        {
            get { return m_storeName; }
        }

        /// <summary>
        /// Returns a collection of certificates located in an X.509 certificate store.
        /// </summary>
        public X509Certificate2Collection Certificates
        {
            get
            {
                if (m_certs == null)
                {
                    m_certs = new X509Certificate2Collection();

                    CryptokiCertificate[] certs = CryptokiCertificate.LoadCertificates(m_session, m_storeName);

                    if (certs != null)
                    {
                        for (int i = 0; i < certs.Length; i++)
                        {
                            m_certs.Add(new X509Certificate2(certs[i]));
                        }
                    }
                }

                return m_certs;
            }
        }

        /// <summary>
        /// Opens an X.509 certificate store or creates a new store, depending on OpenFlags flag settings. (Not Implemented)
        /// </summary>
        /// <param name="flags">A bitwise combination of enumeration values that specifies the way to open the X.509 certificate store.</param>
        public void Open(OpenFlags flags) 
        {
        }

        /// <summary>
        /// Closes an X.509 certificate store. (Not Implemented)
        /// </summary>
        public void Close() 
        {
        }

        /// <summary>
        /// Adds a certificate to an X.509 certificate store.
        /// </summary>
        /// <param name="certificate">The certificate to add.</param>
        public void Add(X509Certificate2 certificate)
        {
            Add(certificate, "");
        }

        /// <summary>
        /// Adds a certificate with a friendly name to an X.509 certificate store.
        /// </summary>
        /// <param name="certificate">The certificate to add.</param>
        /// <param name="friendlyName">The friendly name to apply to the certificate.</param>
        public void Add(X509Certificate2 certificate, string friendlyName) 
        {
            if (certificate == null)
                throw new ArgumentNullException("certificate");

            if (!certificate.InternalCert.Save(friendlyName, m_storeName, SecureStorageLevel.HighestAvailable)) throw new Exception();

            if (m_certs != null)
            {
                // Saving a cert to a store can cause a new cert to be created
                // so clear the certs cache 
                m_certs = null;
            }
        }

        /// <summary>
        /// Removes a certificate from an X.509 certificate store.
        /// </summary>
        /// <param name="certificate">The certificate to remove.</param>
        public void Remove(X509Certificate2 certificate) 
        {
            if (certificate == null)
                throw new ArgumentNullException();

            if (!certificate.InternalCert.Delete(m_storeName)) throw new Exception();

            if (m_certs != null)
            {
                m_certs.Remove(certificate);
            }
        }
    }
}

