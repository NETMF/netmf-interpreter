namespace System.Security.Cryptography.X509Certificates 
{
    using System;
    using System.Collections;
    using System.Text;
    using System.Security;
    using System.Globalization;
    using System.Runtime.InteropServices;    
    using System.Security.Cryptography;
    using Microsoft.SPOT.Cryptoki;
    
    public class X509Certificate2 : X509Certificate
    {
        private const string NewLine = "\r\n";
        private byte[] m_serialNumber;
        private CryptoKey.KeyType m_keyAlgorithmOid;
        private MechanismType m_signatureAlgorithmOid;
        private byte[] m_rawData;
        private byte[] m_thumbprint;
        CryptokiCertificate m_cert;
        
        private X509Certificate2()
        { }
    
        /// <summary>
        /// Initializes a new instance of the X509Certificate2 class from a byte array
        /// </summary>
        /// <param name="session">Cryptoki session for which this certificate will be created</param>
        /// <param name="data">Data bytes for the certificate (PEM, DER, P12, etc.)</param>
        /// <param name="password">Password for decrypting the certificate data (optional)</param>
        public X509Certificate2(Session session, byte[] data, string password="")
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException();

            m_cert = CryptokiCertificate.LoadCertificate(session, data, password);

            Init();
        }

        /// <summary>
        /// Initializes a new instance of the X509Certificate2 class from a Cryptoki attribute array
        /// </summary>
        /// <param name="session"></param>
        /// <param name="template"></param>
        public X509Certificate2(Session session, CryptokiAttribute[] template)
        {
            m_cert = CryptokiCertificate.CreateCertificate(session, template);

            Init();
        }

        internal X509Certificate2(CryptokiCertificate cert)
        {
            m_cert = cert;

            Init();
        }

        private void Init()
        {
            m_keyAlgorithmOid = CryptoKey.KeyType.VENDOR_DEFINED;
            m_signatureAlgorithmOid = MechanismType.VENDOR_DEFINED;
            base.m_handle = m_cert.Handle;
            base.m_sessionHandle = m_cert.Session.Handle;
        }

        internal CryptokiCertificate InternalCert
        {
            get
            {
                return m_cert;
            }
        }

        /// <summary>
        /// Gets the subject distinguished name from the certificate. (Inherited from X509Certificate.)
        /// </summary>
        public override string Subject
        {
            get
            {
                if (m_subject == null)
                {
                    m_subject = m_cert.GetProperty(CertificateProperty.Subject) as string;
                }

                return m_subject;
            }
        }

        /// <summary>
        /// Gets the name of the certificate authority that issued the X.509v3 certificate. (Inherited from X509Certificate.)
        /// </summary>
        public override string Issuer
        {
            get
            {
                if (m_issuer == null)
                {
                    m_issuer = m_cert.GetProperty(CertificateProperty.Issuer) as string;
                }

                return m_issuer;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether an X509Certificate2 object contains a private key. 
        /// </summary>
        public bool HasPrivateKey
        {
            get
            {
                return m_cert.HasPrivateKey;
            }
        }

        /// <summary>
        /// Gets the serial number of a certificate.
        /// </summary>
        public string SerialNumber
        {
            get
            {
                if (m_serialNumber == null)
                    m_serialNumber = GetSerialNumber();

                return EncodeHexStringFromInt(m_serialNumber);
            }
        }

        /// <summary>
        /// Gets the date in local time on which a certificate becomes valid.
        /// </summary>
        public DateTime NotBefore
        {
            get
            {
                if (m_effectiveDate == DateTime.MinValue)
                {
                    m_effectiveDate = (DateTime)m_cert.GetProperty(CertificateProperty.EffectiveDate);
                }
                return m_effectiveDate;
            }
        }

        /// <summary>
        /// Gets the date in local time after which a certificate is no longer valid.
        /// </summary>
        public DateTime NotAfter
        {
            get
            {
                if (m_expirationDate == DateTime.MinValue)
                {
                    m_expirationDate = (DateTime)m_cert.GetProperty(CertificateProperty.ExpirationDate);
                }

                return m_expirationDate;
            }
        }

        /// <summary>
        /// Gets the thumbprint of a certificate.
        /// </summary>
        public string Thumbprint
        {
            get
            {
                return GetCertHashString();
            }
        }

        /// <summary>
        /// Gets the raw data of a certificate.
        /// </summary>
        public byte[] RawData 
        {
            get
            {
                if(m_rawData == null)
                {
                    m_rawData = m_cert.GetProperty(CertificateProperty.RawBytes) as byte[];
                }

                return m_rawData;
            }
        }

        /// <summary>
        /// Gets the algorithm used to create the signature of a certificate.
        /// </summary>
        public MechanismType SignatureAlgorithm
        {
            get
            {
                if (m_signatureAlgorithmOid == MechanismType.VENDOR_DEFINED)
                    m_signatureAlgorithmOid = (MechanismType)m_cert.GetProperty(CertificateProperty.SignatureAlgorithm);

                return m_signatureAlgorithmOid;
            }
        }

        /// <summary>
        /// Gets the AsymmetricAlgorithm object that represents the private key associated with a certificate.
        /// </summary>
        public AsymmetricAlgorithm PrivateKey
        {
            get 
            {
                if (!HasPrivateKey) throw new CryptographicException();

                return PublicKey;
            }
        }

        /// <summary>
        /// Gets a AsymmetricAlgorithm object that represents the public key associated with a certificate.
        /// </summary>
        public AsymmetricAlgorithm PublicKey
        {
            get 
            {
                if (m_signatureAlgorithmOid == MechanismType.VENDOR_DEFINED)
                    m_signatureAlgorithmOid = (MechanismType)m_cert.GetProperty(CertificateProperty.SignatureAlgorithm);

                switch (m_signatureAlgorithmOid)
                {
                    case MechanismType.SHA1_RSA_PKCS:
                        return new RSACryptoServiceProvider(m_cert);

                    case MechanismType.SHA256_RSA_PKCS:
                        {
                            RSACryptoServiceProvider csp = new RSACryptoServiceProvider(m_cert);
                            csp.HashAlgorithm = MechanismType.SHA256;
                            return csp;
                        }

                    case MechanismType.SHA384_RSA_PKCS:
                        {
                            RSACryptoServiceProvider csp = new RSACryptoServiceProvider(m_cert);
                            csp.HashAlgorithm = MechanismType.SHA384;
                            return csp;
                        }

                    case MechanismType.SHA512_RSA_PKCS:
                        {
                            RSACryptoServiceProvider csp = new RSACryptoServiceProvider(m_cert);
                            csp.HashAlgorithm = MechanismType.SHA512;
                            return csp;
                        }

                    case MechanismType.MD5_RSA_PKCS:
                        {
                            RSACryptoServiceProvider csp = new RSACryptoServiceProvider(m_cert);
                            csp.HashAlgorithm = MechanismType.MD5;
                            return csp;
                        }

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// Gets the KeyType (RSA, DSA, etc.) associated with a certificate.
        /// </summary>
        public CryptoKey.KeyType KeyType
        {
            get
            {
                if (m_keyAlgorithmOid == CryptoKey.KeyType.VENDOR_DEFINED)
                    m_keyAlgorithmOid = (CryptoKey.KeyType)m_cert.GetProperty(CertificateProperty.KeyType);

                return m_keyAlgorithmOid;
            }
        }


        /// <summary>
        /// Returns the raw data for the entire X.509v3 certificate. (Inherited from X509Certificate.)
        /// </summary>
        /// <returns>The raw data for the entire X.509v3 certificate.</returns>
        public override byte[] GetRawCertData()
        {
            return this.RawData;
        }


        /// <summary>
        /// Returns the serial number of the X.509v3 certificate.
        /// </summary>
        /// <returns>The serial number of the X.509v3 certificate.</returns>
        public virtual byte[] GetSerialNumber()
        {
            if (m_serialNumber == null)
                m_serialNumber = m_cert.GetProperty(CertificateProperty.SerialNumber) as byte[];
            
            return (byte[]) m_serialNumber.Clone();
        }

        /// <summary>
        /// Returns the effective date of this X.509v3 certificate.
        /// </summary>
        /// <returns>The effective date of this X.509v3 certificate.</returns>
        public virtual string GetEffectiveDateString()
        {
            return NotBefore.ToString();
        }

        /// <summary>
        /// Returns the expiration date of this X.509v3 certificate. 
        /// </summary>
        /// <returns>The expiration date of this X.509v3 certificate.</returns>
        public virtual string GetExpirationDateString()
        {
            return NotAfter.ToString();
        }

        /// <summary>
        /// Compares two X509Certificate2 objects for equality.
        /// </summary>
        /// <param name="other">An X509Certificate2 object to compare to the current object.</param>
        /// <returns>true if the current X509Certificate2 object is equal to the object specified by the other parameter; otherwise, false.</returns>
        public virtual bool Equals (X509Certificate2 other) 
        {
            if (other == null)
                return false;

            if (!this.Issuer.Equals(other.Issuer))
                return false;

            if (!this.SerialNumber.Equals(other.SerialNumber))
                return false;

            return true;
        }

        /// <summary>
        /// Returns the hash code for the X.509v3 certificate as an integer.
        /// </summary>
        /// <returns>The hash code for the X.509 certificate as an integer.</returns>
        public override int GetHashCode()
        {
            if (m_thumbprint == null) m_thumbprint = m_cert.GetProperty(CertificateProperty.KeyHash) as byte[];

            int value = 0;
            for (int i = 0; i < m_thumbprint.Length && i < 4; ++i)
            {
                value = value << 8 | m_thumbprint[i];
            }
            return value;
        }     
        
        /// <summary>
        /// Returns the hash value for the X.509v3 certificate as an array of bytes.
        /// </summary>
        /// <returns>The hexadecimal string representation of the X.509 certificate hash value.</returns>
        public virtual byte[] GetCertHash()
        {
            if (m_thumbprint == null) m_thumbprint = m_cert.GetProperty(CertificateProperty.KeyHash) as byte[];
            return (byte[])m_thumbprint.Clone();
        }
        
        /// <summary>
        /// Returns the SHA1 hash value for the X.509v3 certificate as a hexadecimal string. 
        /// </summary>
        /// <returns>The hexadecimal string representation of the X.509 certificate hash value.</returns>
        public virtual string GetCertHashString()
        {
            if (m_thumbprint == null) m_thumbprint = m_cert.GetProperty(CertificateProperty.KeyHash) as byte[];
            return CreateHexString(m_thumbprint);
        }
 
        /// <summary>
        /// Displays an X.509 certificate in text format.
        /// </summary>
        /// <returns>The certificate information.</returns>
        public override string ToString() 
        {
            return ToString(false);
        }
        
        /// <summary>
        /// Displays an X.509 certificate in text format.
        /// </summary>
        /// <param name="fVerbose">true to display the public key, private key, extensions, and so forth; false to display information that is similar to the X509Certificate2 class, including thumbprint, serial number, subject and issuer names, and so on. </param>
        /// <returns>The certificate information.</returns>
        public virtual string ToString (bool fVerbose) 
        {
            if (fVerbose == false)
                return GetType().FullName;

            //StringBuilder sb = new StringBuilder();
            string sb = "";

            // Subject
            sb += /*.Append*/("[Subject]" + NewLine + "  ");
            sb += /*.Append*/(this.Subject);

            // Issuer
            sb += /*.Append*/(NewLine + NewLine + "[Issuer]" + NewLine + "  ");
            sb += /*.Append*/(this.Issuer);

            // Serial Number
            sb += /*.Append*/(NewLine + NewLine + "[Serial Number]" + NewLine + "  ");
            sb += /*.Append*/(this.SerialNumber);

            // NotBefore
            sb += /*.Append*/(NewLine + NewLine + "[Not Before]" + NewLine + "  ");
            sb += /*.Append*/(this.NotBefore);

            // NotAfter
            sb += /*.Append*/(NewLine + NewLine + "[Not After]" + NewLine + "  ");
            sb += /*.Append*/(this.NotAfter);

            // Thumbprint
            sb += /*.Append*/(NewLine + NewLine + "[Thumbprint]" + NewLine + "  ");
            sb += /*.Append*/(this.GetCertHashString());

            sb += /*.Append*/(NewLine);
            return sb; //.ToString();
        }

        /// <summary>
        /// Performs a X.509 chain validation using basic validation policy.
        /// </summary>
        /// <returns>true if the validation succeeds; false if the validation fails.</returns>
        public bool Verify()
        {
            throw new NotImplementedException();
        }

#region Internal Helper Functions      
    
        static private char[]  hexValues = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        protected internal String CreateHexString(byte[] sArray) 
        {
            String result = null;
            if(sArray != null) 
            {
                char[] hexOrder = new char[sArray.Length * 2];
            
                int digit;
                for(int i = 0, j = 0; i < sArray.Length; i++) 
                {
                    digit = (sArray[i] & 0xf0) >> 4;
                    hexOrder[j++] = hexValues[digit];
                    digit = sArray[i] & 0x0f;
                    hexOrder[j++] = hexValues[digit];
                }
                result = new String(hexOrder);
            }
            return result;
        }

        internal static string EncodeHexStringFromInt(byte[] sArray) 
        {
            String result = null;
            if(sArray != null) 
            {
                char[] hexOrder = new char[sArray.Length * 2];
            
                int i = sArray.Length;
                int digit, j=0;
                while (i-- > 0)
                {
                    digit = (sArray[i] & 0xf0) >> 4;
                    hexOrder[j++] = HexDigit(digit);
                    digit = sArray[i] & 0x0f;
                    hexOrder[j++] = HexDigit(digit);
                }
                result = new String(hexOrder);
            }
            return result;
        }        

        // converts number to hex digit. Does not do any range checks.
        static char HexDigit(int num) 
        {
            return (char)((num < 10) ? (num + '0') : (num + ('A' - 10)));
        }             
     
#endregion        
    }
}

