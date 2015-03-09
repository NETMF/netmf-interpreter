using System.Runtime.CompilerServices;
using System;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Microsoft.SPOT.Cryptoki
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct DATE_TIME_INFO
    {
        public int  year;           /* year, AD                   */
        public int month;          /* 1 = January, 12 = December */
        public int day;            /* 1 = first of the month     */
        public int hour;           /* 0 = midnight, 12 = noon    */
        public int minute;         /* minutes past the hour      */
        public int second;         /* seconds in minute          */
        public int msec;           /* milliseconds in second     */

        public int dlsTime;        /* boolean; daylight savings time is in effect                      */
        public int tzOffset;       /* signed int; difference in seconds imposed by timezone (from GMT) */

        internal DATE_TIME_INFO(byte[] data)
        {
            if (data.Length < 36) throw new ArgumentException();

            year     = Utility.ConvertToInt32(data, 0);
            month    = Utility.ConvertToInt32(data, 4);
            day      = Utility.ConvertToInt32(data, 8);
            hour     = Utility.ConvertToInt32(data, 12);
            minute   = Utility.ConvertToInt32(data, 16);
            second   = Utility.ConvertToInt32(data, 20);
            msec     = Utility.ConvertToInt32(data, 24);
            dlsTime  = Utility.ConvertToInt32(data, 28);
            tzOffset = Utility.ConvertToInt32(data, 32);
        }
    }

    /// <summary>
    /// Enumeration of the supported X509 certificate property types
    /// </summary>
    public enum CertificateProperty : uint
    {
        Issuer             = CryptokiAttribute.CryptokiType.Issuer,
        Subject            = CryptokiAttribute.CryptokiType.Subject,
        EffectiveDate      = CryptokiAttribute.CryptokiType.StartDate,
        ExpirationDate     = CryptokiAttribute.CryptokiType.EndDate,
        SerialNumber       = CryptokiAttribute.CryptokiType.SerialNumber,
        KeyType            = CryptokiAttribute.CryptokiType.KeyType,
        SignatureAlgorithm = CryptokiAttribute.CryptokiType.MechanismType,
        KeyHash            = CryptokiAttribute.CryptokiType.HashOfSubjectPublicKey,
        DnsName            = CryptokiAttribute.CryptokiType.URL,
        RawBytes           = CryptokiAttribute.CryptokiType.Value,
    }

    /// <summary>
    /// Enumeration of the supported certificate types
    /// </summary>
    public enum CertificateType : uint
    {
        X_509           = 0,
        X_509_ATTR_CERT = 1,
        CKC_WTLS        = 2,
        VENDOR_DEFINED  = 0x80000000,
    }

    /// <summary>
    /// Enumeration of the supported certificate categories
    /// </summary>
    //public enum CertificateCategory : uint
    //{
    //    Unspecified = 0,
    //    User        = 1,
    //    Authority   = 2,
    //    Other       = 3,
    //}

    /// <summary>
    /// The representative class for a certificate object in PKCS11 (Cryptoki)
    /// </summary>
    public class CryptokiCertificate : CryptoKey
    {
        private Hashtable m_propertyBag;

        protected CryptokiCertificate(Session session)
            : base(session)
        {
            m_propertyBag = new Hashtable();
        }

        /// <summary>
        /// Gets the property value given the specified property name.
        /// </summary>
        /// <param name="propName">The property name</param>
        /// <returns>The property value object</returns>
        public object GetProperty(CertificateProperty propName)
        {
            object retVal = null;

            if (m_propertyBag == null) m_propertyBag = new Hashtable();

            if (m_propertyBag.Contains(propName))
            {
                retVal = m_propertyBag[propName] as Byte[];
            }
            else
            {
                byte[] propValue = new byte[2048];
                byte[] propLen = new byte[4];

                CryptokiAttribute[] props = new CryptokiAttribute[] { 
                    new CryptokiAttribute((CryptokiAttribute.CryptokiType)propName, propValue),
                    new CryptokiAttribute( CryptokiAttribute.CryptokiType.ValueLen, propLen  ),
                };

                if(GetAttributeValues(ref props))
                {
                    switch(propName)
                    {
                        case CertificateProperty.KeyType:
                            retVal = (KeyType)Utility.ConvertToInt32(propValue);
                            break;
                        case CertificateProperty.SignatureAlgorithm:
                            retVal = (MechanismType)Utility.ConvertToInt32(propValue);
                            break;
                        case CertificateProperty.Issuer:
                        case CertificateProperty.Subject:
                            retVal = new string(UTF8Encoding.UTF8.GetChars(propValue));
                            break;
                        case CertificateProperty.SerialNumber:
                        case CertificateProperty.RawBytes:
                            {
                                int len = Utility.ConvertToInt32(propLen);
                                if (len < propValue.Length)
                                {
                                    byte[] tmp = new byte[len];
                                    Array.Copy(propValue, tmp, len);
                                    propValue = tmp;
                                }
                            }
                            retVal = propValue;
                            break;
                        case CertificateProperty.EffectiveDate:
                        case CertificateProperty.ExpirationDate:
                            DATE_TIME_INFO dti = new DATE_TIME_INFO(propValue);
                            retVal = new DateTime(dti.year, dti.month, dti.day, dti.hour, dti.minute, dti.second, dti.msec);
                            break;
                    }
                }

                if (retVal != null)
                {
                    m_propertyBag[propName] = retVal;
                }
            }

            return retVal;
        }

        /// <summary>
        /// Gets the boolean value representing whether or not the certificate has a private key.
        /// </summary>
        public bool HasPrivateKey
        {
            get
            {
                return !PublicOnly;
            }
        }

        /// <summary>
        /// Creates a Cryptoki certificate object with the specified attribute array template and session context.
        /// </summary>
        /// <param name="session">The Cryptoki session context.</param>
        /// <param name="template">The attribute template that defines the certificate properties.</param>
        /// <returns>The created cryptoki certificate object</returns>
        public static CryptokiCertificate CreateCertificate(Session session, CryptokiAttribute[] template)
        {
            CryptokiCertificate ret = CreateObject(session, template) as CryptokiCertificate;

            ret.m_propertyBag = new Hashtable();

            session.AddSessionObject(ret);

            return ret;
        }

        /// <summary>
        /// Creates a Cryptoki certificate object with the specified byte array and password.
        /// </summary>
        /// <param name="session">The Cryptoki session context.</param>
        /// <param name="certData">The certificate data in PEM, P12, DER, etc. format.</param>
        /// <param name="password">The string password for decrypting the certificate</param>
        /// <returns>The loaded cryptoki certificate object</returns>
        public static CryptokiCertificate LoadCertificate(
            Session session, 
            //CertificateType type, 
            //CertificateCategory category, 
            //string name, 
            byte[] certData,
            string password
            )
        {
            CryptokiAttribute[] attribs = new CryptokiAttribute[]
            {
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.Class             , Utility.ConvertToBytes((int)CryptokiClass.CERTIFICATE)),
                //new CryptokiAttribute(CryptokiAttribute.CryptokiType.CertificateType   , Utility.ConvertToBytes((int)type                     )),
                //new CryptokiAttribute(CryptokiAttribute.CryptokiType.CertficateCategory, Utility.ConvertToBytes((int)category                 )),
                //new CryptokiAttribute(CryptokiAttribute.CryptokiType.Label             , UTF8Encoding.UTF8.GetBytes(name                      )),
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.Password          , UTF8Encoding.UTF8.GetBytes(password                  )),
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.Value             , certData                                              ),
            };

            return CreateCertificate(session, attribs);
        }

        /// <summary>
        /// Loads all Cryptoki certificates for the specified certificate store (if supported).
        /// </summary>
        /// <param name="session">The Cryptoki session context.</param>
        /// <param name="storeName">The certificate store moniker.</param>
        /// <returns>The loaded cryptoki certificate object array</returns>
        public static CryptokiCertificate[] LoadCertificates(Session session, string storeName)
        {
            CryptokiCertificate[] certs;
            CryptokiAttribute[] attribs = new CryptokiAttribute[]
            {
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.Class, Utility.ConvertToBytes((int)CryptokiClass.CERTIFICATE)),
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.Label, UTF8Encoding.UTF8.GetBytes(storeName)), 
            };

            FindObjectEnum objEnum = new FindObjectEnum(session, attribs);

            if (objEnum.Count == 0)
            {
                certs = new CryptokiCertificate[0];
            }
            else
            {
                Array ar = objEnum.GetNext(objEnum.Count);
                certs = ar as CryptokiCertificate[];
            }

            objEnum.Close();

            return certs;
        }
    }
}
