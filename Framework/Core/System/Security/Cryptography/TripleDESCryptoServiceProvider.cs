//
// TripleDESCryptoServiceProvider.cs
//
using Microsoft.SPOT.Cryptoki;

namespace System.Security.Cryptography 
{
    /// <summary>
    /// Defines a wrapper object to access the cryptographic service provider (CSP) version of the TripleDES algorithm. This class cannot be inherited.
    /// </summary>
    public sealed class TripleDESCryptoServiceProvider : SymmetricAlgorithm
    {
        public const int DefaultKeySize = 192;
        private static KeySizes[] s_legalBlockSizes = {
            new KeySizes(64, 64, 0)
        };
        private static KeySizes[] s_legalKeySizes = {
            new KeySizes(128, 192, 64)
        };

        //
        // public constructors
        //

        /// <summary>
        /// Initializes a new instance of the TripleDESCryptoServiceProvider class with the specified key size.
        /// </summary>
        /// <param name="keySize">The size of the key to use in bits.</param>
        public TripleDESCryptoServiceProvider(int keySize = DefaultKeySize)
            : this("", keySize) 
        {
        }

        /// <summary>
        /// Initializes a new instance of the TripleDESCryptoServiceProvider class with the specified crypto service provider and key size
        /// </summary>
        /// <param name="serviceProviderName">Defines the TripleDES crypto service provider to be used.</param>
        /// <param name="keySize">The size of the key to use in bits.</param>
        public TripleDESCryptoServiceProvider(string serviceProviderName, int keySize = DefaultKeySize)
            : base(new Session(serviceProviderName, MechanismType.DES3_CBC, MechanismType.DES3_KEY_GEN), true)
        {
            Init(null, keySize);
        }

        /// <summary>
        /// Initializes a new instance of the TripleDESCryptoServiceProvider class.
        /// </summary>
        /// <param name="session">Defines the session context to be used to perform TripleDES operations.</param>
        /// <param name="keySize">The size of the key to use in bits.</param>
        public TripleDESCryptoServiceProvider(Session session, int keySize = DefaultKeySize)
            : base(session, false)
        {
            Init(null, keySize);
        }

        /// <summary>
        /// Initializes a new instance of the TripleDESCryptoServiceProvider class.
        /// </summary>
        /// <param name="key">The key to be used for TripleDES operations.</param>
        public TripleDESCryptoServiceProvider(CryptoKey key)
            : base(key.Session, false)
        {
            Init(key, key.Size);
        }

        private void Init(CryptoKey key, int keySize)
        {
            LegalBlockSizesValue = s_legalBlockSizes;
            LegalKeySizesValue   = s_legalKeySizes;
            BlockSizeValue       = 64;

            if (key == null)
            {
                KeySizeValue = keySize;
                GenerateKey();
            }
            else
            {
                if (key.Type != CryptoKey.KeyType.DES3 && key.Type != CryptoKey.KeyType.GENERIC_SECRET) throw new ArgumentException();

                Key = key;
            }
        }

        /// <summary>
        /// The Cryptoki mechanism type for the TripleDES object
        /// </summary>
        protected override MechanismType MechanismType
        {
            get
            {
                switch (ModeValue)
                {
                    case CipherMode.CBC:
                        switch (PaddingValue)
                        {
                            case PaddingMode.None:
                                return MechanismType.DES3_CBC;
                            case PaddingMode.PKCS7:
                                return MechanismType.DES3_CBC_PAD;
                        }
                        break;
                    case CipherMode.ECB:
                        if (PaddingValue == PaddingMode.None) return MechanismType.DES3_ECB;
                        break;
                }

                throw new NotSupportedException();
            }
        }

        //
        // public methods
        //

        /// <summary>
        /// Creates a symmetric TripleDES decryptor object with the specified key (Key) and initialization vector (IV).
        /// </summary>
        /// <param name="key">The secret key to use for the symmetric algorithm.</param>
        /// <param name="iv">The initialization vector to use for the symmetric algorithm.</param>
        /// <returns>A symmetric TripleDES decryptor object.</returns>
        public override ICryptoTransform CreateDecryptor(CryptoKey key, byte[] iv)
        {
            Mechanism mech = new Mechanism(MechanismType);

            mech.Parameter = null;
            if (iv != null)
            {
                mech.Parameter = (byte[])iv.Clone();
            }

            return new Decryptor(m_session, mech, key, BlockSize, BlockSize);
        }

        /// <summary>
        /// Creates a symmetric TripleDES encryptor object with the specified key (Key) and initialization vector (IV).
        /// </summary>
        /// <param name="key">The secret key to use for the symmetric algorithm.</param>
        /// <param name="iv">The initialization vector to use for the symmetric algorithm.</param>
        /// <returns>A symmetric TripleDES encryptor object.</returns>
        public override ICryptoTransform CreateEncryptor(CryptoKey key, byte[] iv)
        {
            Mechanism mech = new Mechanism(MechanismType);

            mech.Parameter = null;
            if (iv != null)
            {
                mech.Parameter = (byte[])iv.Clone();
            }

            return new Encryptor(m_session, mech, key, BlockSize, BlockSize);
        }

        /// <summary>
        /// Generates a random Key to be used for the algorithm.
        /// </summary>
        public override void GenerateKey()
        {
            CryptoKey key;
            Mechanism mech = new Mechanism(Microsoft.SPOT.Cryptoki.MechanismType.DES3_KEY_GEN);
            CryptokiAttribute[] attribs = new CryptokiAttribute[]
            { 
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.ValueLen, Utility.ConvertToBytes( KeySizeValue ))
            };

            key = CryptoKey.GenerateKey(m_session, mech, attribs);

            if (KeyValue != null && OwnsKey)
            {
                KeyValue.Dispose();
            }

            KeyValue = key;
            OwnsKey = true;
        }

        /// <summary>
        /// Generates a random initialization vector (IV) to use for the algorithm.
        /// </summary>
        public override void GenerateIV()
        {
            byte[] iv = new byte[8];

            try
            {
                using (RNGCryptoServiceProvider rand = new RNGCryptoServiceProvider(m_session))
                {
                    rand.GetBytes(iv);
                }
            }
            catch
            {
            }

            if (IVValue != null)
            {
                Array.Clear(IVValue, 0, IVValue.Length);
            }

            IVValue = iv;
        }
    }
}
