//
// KeyedHashAlgorithm.cs
//
using Microsoft.SPOT.Cryptoki;

namespace System.Security.Cryptography 
{
    /// <summary>
    /// Enumeration of the supported keyed hash algorithms for the KeyedHashAlgorithm class.
    /// </summary>
    public enum KeyedHashAlgorithmType : uint
    {
        HMACMD5 = MechanismType.MD5_HMAC,
        HMACRIPEMD160 = MechanismType.RIPEMD160_HMAC,

        HMACSHA1 = MechanismType.SHA_1_HMAC,
        HMACSHA256 = MechanismType.SHA256_HMAC,
        HMACSHA384 = MechanismType.SHA384_HMAC,
        HMACSHA512 = MechanismType.SHA512_HMAC,
    }

    /// <summary>
    /// Computes a Hash-based Message Authentication Code (HMAC) using the provided hash function type. 
    /// </summary>
    public class KeyedHashAlgorithm : HashAlgorithm 
    {
        protected CryptoKey KeyValue;
        protected bool      OwnsKey;

        /// <summary>
        /// Initializes a new instance of the KeyedHashAlgorithm class with the specified algorithm and session context.
        /// </summary>
        /// <param name="algorithm">The keyed hash algorithm to be used (HMACSHA1, HMACRIPEMD160, etc.)</param>
        /// <param name="session">The Cryptoki session context to associate with the keyed hash algorithm.</param>
        public KeyedHashAlgorithm(KeyedHashAlgorithmType algorithm, Session session)
            : base((HashAlgorithmType)algorithm, session)
        {
            GenerateKey(HashSize);
            OwnsKey = true;
        }

        /// <summary>
        /// Initializes a new instance of the KeyedHashAlgorithm class with the specified algorithm and crypto service provider.
        /// </summary>
        /// <param name="algorithm">The keyed hash algorithm to be used (HMACSHA1, HMACRIPEMD160, etc.)</param>
        /// <param name="serviceProvider">The crypto service provider that will be used for the keyed hash operation.</param>
        public KeyedHashAlgorithm(KeyedHashAlgorithmType algorithm, string serviceProvider="")
            : base((HashAlgorithmType)algorithm, serviceProvider)
        {
            GenerateKey(HashSize);
            OwnsKey = true;
        }

        /// <summary>
        /// Initializes a new instance of the KeyedHashAlgorithm class with the specified algorithm and key object.
        /// </summary>
        /// <param name="algorithm">The keyed hash algorithm to be used (HMACSHA1, HMACRIPEMD160, etc.)</param>
        /// <param name="key">The Cryptoki key object that will be used to sign the hashed value.</param>
        public KeyedHashAlgorithm(KeyedHashAlgorithmType algorithm, CryptoKey key)
            : base((HashAlgorithmType)algorithm, key.Session)
        {
            Key = key;
            OwnsKey = false;
        }

        /// <summary>
        /// Generates a random symmetric key for use in the signing process.
        /// </summary>
        /// <param name="keySize">Size of the symmetric key in bits</param>
        private void GenerateKey(int keySize)
        {
            CryptokiAttribute[] attribs = new CryptokiAttribute[]
            { 
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.ValueLen, Utility.ConvertToBytes( keySize ))
            };

            KeyValue = CryptoKey.GenerateKey(m_session, new Mechanism(MechanismType.GENERIC_SECRET_KEY_GEN), attribs);
            OwnsKey = true;

            m_mechanism.Parameter = KeyValue.Handle;
            Initialize();
        }

        /// <summary>
        /// Gets the keyed hash algorithm type
        /// </summary>
        public virtual MechanismType KeyedHashType
        {
            get
            {
                return m_mechanism.Type;
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (KeyValue != null && OwnsKey)
                    {
                        KeyValue.Dispose();
                    }

                    if (m_ownsSession && m_session != null)
                    {
                        m_session.Dispose();
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        //
        // public properties
        //

        /// <summary>
        /// Gets or sets the key used to sign the hash value.
        /// </summary>
        public virtual CryptoKey Key 
        {
            get { return KeyValue; }
            set 
            {
                if (State != 0)
                    throw new CryptographicException();

                if (KeyValue != null && OwnsKey)
                {
                    KeyValue.Dispose();
                }

                KeyValue = value;
                OwnsKey = false;

                m_mechanism.Parameter = KeyValue.Handle;
                Initialize();
            }
        }
    }
}
