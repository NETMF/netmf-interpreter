using System.Runtime.CompilerServices;
using Microsoft.SPOT.Cryptoki;

namespace System.Security.Cryptography
{
    public class CryptoKey : CryptokiObject
    {
        /// <summary>
        /// Enumeration of the possible key types.
        /// </summary>
        public enum KeyType : uint
        {
            RSA             = 0x00000000,
            DSA             = 0x00000001,
            DH              = 0x00000002,
            
            ECDSA           = 0x00000003,
            EC              = 0x00000003,
            X9_42_DH        = 0x00000004,
            KEA             = 0x00000005,
            
            GENERIC_SECRET  = 0x00000010,
            RC2             = 0x00000011,
            RC4             = 0x00000012,
            DES             = 0x00000013,
            DES2            = 0x00000014,
            DES3            = 0x00000015,

            CAST            = 0x00000016,
            CAST3           = 0x00000017,

            CAST5           = 0x00000018,
            CAST128         = 0x00000018,
            RC5             = 0x00000019,
            IDEA            = 0x0000001A,
            SKIPJACK        = 0x0000001B,
            BATON           = 0x0000001C,
            JUNIPER         = 0x0000001D,
            CDMF            = 0x0000001E,
            AES             = 0x0000001F,

            BLOWFISH        = 0x00000020,
            TWOFISH         = 0x00000021,

            SECURID         = 0x00000022,
            HOTP            = 0x00000023,
            ACTI            = 0x00000024,

            CAMELLIA        = 0x00000025,
            ARIA            = 0x00000026,
            
            VENDOR_DEFINED  = 0x80000000,

            INVALID         = 0xffffffff,
        }

        /// <summary>
        /// Enumeration of the key class types
        /// </summary>
        public enum KeyClass : uint
        {
            Secret  = CryptokiClass.SECRET_KEY,
            Public  = CryptokiClass.PUBLIC_KEY,
            Private = CryptokiClass.PRIVATE_KEY,
            Other   = CryptokiClass.VENDOR_DEFINED
        }

        private int     m_length;
        private KeyType m_keyType;
        private int     m_privateKeyHandle;


        protected CryptoKey(Session session)
            : base(session)
        {
        }

        /// <summary>
        /// Creates a CryptoKey in the specfied session context with the specified key attribute template.
        /// </summary>
        /// <param name="session">The Cryptoki session context.</param>
        /// <param name="keyTemplate">The Cryptoki attribute template that specifies key properties.</param>
        /// <returns></returns>
        public static CryptoKey LoadKey(Session session, CryptokiAttribute[] keyTemplate)
        {
            CryptoKey key = CryptokiObject.CreateObject(session, keyTemplate) as CryptoKey;

            key.m_keyType = KeyType.INVALID;

            return key;
        }

        /// <summary>
        /// Gets the key handle in byte format
        /// </summary>
        public byte[] Handle
        {
            get
            {
                int hKey = m_handle;

                return Utility.ConvertToBytes(hKey);
            }
        }

        /// <summary>
        /// Generates a new CryptoKey within the specified session context with the specified key mechanism and key template.
        /// </summary>
        /// <param name="session">The Cryptoki session context.</param>
        /// <param name="mechanism">The key algorithm and parameters.</param>
        /// <param name="template">The key attribute template that defines the resulting key's properties.</param>
        /// <returns></returns>
        public static CryptoKey GenerateKey(Session session, Mechanism mechanism, CryptokiAttribute[] template)
        {
            CryptoKey ret = GenerateKeyInternal(session, mechanism, template);

            session.AddSessionObject(ret);

            return ret;
        }

        /// <summary>
        /// Generates a new CryptoKey object that represents a public/private key pair.
        /// </summary>
        /// <param name="session">The Cryptoki session context.</param>
        /// <param name="mechanism">The key algorithm and parameters.</param>
        /// <param name="publicKeyTemplate">The public key attribute template.</param>
        /// <param name="privateKeyTemplate">The private key attribute template.</param>
        /// <returns></returns>
        public static CryptoKey GenerateKeyPair(Session session, Mechanism mechanism, CryptokiAttribute[] publicKeyTemplate, CryptokiAttribute[] privateKeyTemplate)
        {
            CryptoKey ret = GenerateKeyPairInternal(session, mechanism, publicKeyTemplate, privateKeyTemplate);

            session.AddSessionObject(ret);

            return ret;
        }

        /// <summary>
        /// Derives a CryptoKey object with the specified key algorithm and key attribute template
        /// </summary>
        /// <param name="mechanism">The Cryptoki session context.</param>
        /// <param name="template">The key attribute template.</param>
        /// <returns></returns>
        public CryptoKey DeriveKey(Mechanism mechanism, CryptokiAttribute[] template)
        {
            if (m_isDisposed) throw new ObjectDisposedException();
            
            CryptoKey ret = DeriveKeyInternal(mechanism, template);

            m_session.AddSessionObject(ret);

            return ret;
        }

        /// <summary>
        /// Opens a CryptoKey with the specified key name from the underlying key store.
        /// </summary>
        /// <param name="session">The Cryptoki session context.</param>
        /// <param name="keyName">The name of the key to be opened.</param>
        /// <returns>The CryptoKey for the specifed key name.</returns>
        public static CryptoKey OpenKey(Session session, string keyName, string keyStore="")
        {
            CryptokiAttribute[] template = new CryptokiAttribute[]
            {
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.Class, Utility.ConvertToBytes((int)CryptokiClass.OTP_KEY)),
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.Label, System.Text.UTF8Encoding.UTF8.GetBytes(keyStore)),
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.ObjectID, System.Text.UTF8Encoding.UTF8.GetBytes(keyName))
            };

            using (FindObjectEnum objects = new FindObjectEnum(session, template))
            {
                CryptokiObject[] objs = objects.GetNext(1);

                if (objs != null && objs.Length == 1 && objs[0] is CryptoKey)
                {
                    return (CryptoKey)objs[0];
                }
            }

            return null;
        }

        /// <summary>
        /// Opens the device key.
        /// </summary>
        /// <param name="session">The Cryptoki session context.</param>
        /// <returns>The device key.</returns>
        public static CryptoKey OpenDeviceKey(Session session)
        {
            return OpenKey(session, "NetMF_DeviceKey");
        }

        /// <summary>
        /// Opens the device authority key for validating device updates and key provisioning.
        /// </summary>
        /// <param name="session"></param>
        /// <returns>The device authority key.</returns>
        public static CryptoKey OpenDeviceAuthorityKey(Session session)
        {
            return OpenKey(session, "NetMF_DeviceAuthKey");
        }

        /// <summary>
        /// Unwraps the specified key data with the given wrapping key and mechanism.
        /// </summary>
        /// <param name="session">The Cryptoki session context.</param>
        /// <param name="mechanism">The key wrapping mechanism or algorithm.</param>
        /// <param name="wrappingKey">The key that will be used to unwrap the specifed keyData.</param>
        /// <param name="keyData">The encrypted key data.</param>
        /// <param name="keyTemplate">The key attribute template.</param>
        /// <returns>The unwrapped key object.</returns>
        public static CryptoKey UnwrapKey(Session session, Mechanism mechanism, CryptoKey wrappingKey, byte[] keyData, CryptokiAttribute[] keyTemplate)
        {
            CryptoKey ret = UnwrapKeyInternal(session, mechanism, wrappingKey, keyData, keyTemplate);

            if (ret != null)
            {
                session.AddSessionObject(ret);
            }

            return ret;
        }

        /// <summary>
        /// Imports a key of specifed type given the raw key bytes and key class.
        /// </summary>
        /// <param name="session">The Cryptoki session context.</param>
        /// <param name="keyData">The raw key data bytes.</param>
        /// <param name="keyClass">The class of key represented by the raw bytes.</param>
        /// <param name="keyType">The type of key represented by the raw bytes.</param>
        /// <param name="canBeExported">true if the key can be exported, false other wise.</param>
        /// <returns>The key created from the specified bytes.</returns>
        public static CryptoKey ImportKey(Session session, byte[] keyData, KeyClass keyClass, KeyType keyType, bool canBeExported)
        {
            CryptokiAttribute[] keyImport = new CryptokiAttribute[]
            {
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.Class  , Utility.ConvertToBytes((int)keyClass)),
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.KeyType, Utility.ConvertToBytes((int)keyType)),
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.Value, keyData),
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.Extractable, Utility.ConvertToBytes(canBeExported ? 1 : 0)),
            };

            CryptoKey ret = LoadKey(session, keyImport);

            session.AddSessionObject(ret);

            return ret;
        }

        /// <summary>
        /// Exports the key in standard Windows KeyBlob format.
        /// </summary>
        /// <param name="fIncludePrivate"></param>
        /// <returns></returns>
        public byte[] ExportKey(bool fIncludePrivate)
        {
            byte[] keyData = new byte[(Size + 7)/8];

            CryptokiAttribute[] keyImport = new CryptokiAttribute[]
            {
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.Private, Utility.ConvertToBytes((int)(fIncludePrivate? 1 : 0))),
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.Value, keyData),
            };

            if (GetAttributeValues(ref keyImport))
            {
                return keyData;
            }

            return null;
        }

        /// <summary>
        /// Gets the size of the key, in bits.
        /// </summary>
        public override int Size
        {
            get
            {
                if (m_isDisposed) throw new ObjectDisposedException();

                if (m_keyType == KeyType.INVALID)
                {
                    GetKeyAttribs();
                }

                return m_length;
            }
        }

        /// <summary>
        /// Gets the key type.
        /// </summary>
        public KeyType Type
        {
            get
            {
                if (m_isDisposed) throw new ObjectDisposedException();

                if (m_keyType == KeyType.INVALID)
                {
                    GetKeyAttribs();
                }

                return m_keyType;
            }
        }

        /// <summary>
        /// Gets the boolean value to determine if the key object only contains public data.
        /// </summary>
        public bool PublicOnly
        {
            get
            {
                if (m_isDisposed) throw new ObjectDisposedException();

                return m_privateKeyHandle == -1;
            }
        }

        private void GetKeyAttribs()
        {
            byte[] keyType = new byte[4];
            byte[] length = new byte[4];

            CryptokiAttribute[] keyTypeAttr = new CryptokiAttribute[] 
                    { 
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.KeyType, keyType), 
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.ValueBits, length),
                    };

            GetAttributeValues(ref keyTypeAttr);

            m_keyType = (KeyType)Utility.ConvertToInt32(keyType);
            m_length = Utility.ConvertToInt32(length);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern CryptoKey DeriveKeyInternal(Mechanism mechanism, CryptokiAttribute[] template);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern CryptoKey GenerateKeyInternal(Session session, Mechanism mechanism, CryptokiAttribute[] template);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern CryptoKey GenerateKeyPairInternal(Session session, Mechanism mechanism, CryptokiAttribute[] publicKeyTemplate, CryptokiAttribute[] privateKeyTemplate);

        /// <summary>
        /// Wraps the specified key with the given wrapping key and mechanism.
        /// </summary>
        /// <param name="session">The Cryptoki session context.</param>
        /// <param name="mechanism">The wrapping mechanism or algorithm.</param>
        /// <param name="wrappingKey">The key used to wrap the target key.</param>
        /// <param name="key">The key to be wrapped.</param>
        /// <returns>The encrypted bytes of the wrapped key.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern byte[] WrapKey(Session session, Mechanism mechanism, CryptoKey wrappingKey, CryptoKey key);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern CryptoKey UnwrapKeyInternal(Session session, Mechanism mechanism, CryptoKey unwrappingKey, byte[] wrappedKey, CryptokiAttribute[] keyTemplate);

    }
}
