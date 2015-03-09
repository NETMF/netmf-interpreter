using System;
using Microsoft.SPOT.Cryptoki;
using System.Collections;

namespace System.Security.Cryptography 
{
    /// <summary>
    /// Performs symmetric encryption and decryption using the Advanced Encryption Standard (AES) algorithm.
    /// </summary>
    public sealed class AesCryptoServiceProvider : SymmetricAlgorithm 
    {
        public const int DefaultKeySize = 256;

        private static  KeySizes[] s_legalBlockSizes = {
            new KeySizes(128, 128, 0)
        };
        private static  KeySizes[] s_legalKeySizes = {
            new KeySizes(128, 256, 64), 
        };

        private static Hashtable s_mechanismMap;

        /// <summary>
        /// Initializes a new instance of the AesCryptoServiceProvider class with the specified key size. 
        /// </summary>
        /// <param name="keySize">The size of the key for the asymmetric algorithm in bits.</param>
        public AesCryptoServiceProvider(int keySize = DefaultKeySize)
            : this("", keySize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the AesCryptoServiceProvider class for the specified service provider and key size.
        /// </summary>
        /// <param name="serviceProviderName">Service provider string that identifies the underlying crypto provider.</param>
        /// <param name="keySize">The size of the key for the asymmetric algorithm in bits.</param>
        public AesCryptoServiceProvider(string serviceProviderName, int keySize = DefaultKeySize) :
            base(new Session(serviceProviderName, MechanismType.AES_CBC_PAD, MechanismType.AES_KEY_GEN), true)
        {
            Init(null, keySize);
        }

        /// <summary>
        /// Initializes a new instance of the AesCryptoServiceProvider class for the specified Session and key size.
        /// </summary>
        /// <param name="session">The Session context for which this crypto algorithm will be used.</param>
        /// <param name="keySize">The size of the key for the asymmetric algorithm in bits.</param>
        public AesCryptoServiceProvider(Session session, int keySize = DefaultKeySize) :
            base(session, false)
        {
            Init(null, keySize);
        }

        /// <summary>
        /// Initializes a new instance of the AesCryptoServiceProvider class for the specified CryptoKey object.
        /// </summary>
        /// <param name="key">The key object which the Aes algorithm will for cryptographic operations.</param>
        public AesCryptoServiceProvider(CryptoKey key) :
            base(key.Session, false)
        {
            Init(key, -1);
        }

        private void Init(CryptoKey key, int keySize)
        {
            LegalBlockSizesValue = s_legalBlockSizes;
            LegalKeySizesValue   = s_legalKeySizes;
            BlockSizeValue       = 128;

            if (key == null)
            {
                KeySize = keySize;
                GenerateKey();
            }
            else
            {
                if (key.Type != CryptoKey.KeyType.AES && key.Type != CryptoKey.KeyType.GENERIC_SECRET) throw new ArgumentException();

                Key = key;
            }
        }

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
                                return MechanismType.AES_CBC;
                            case PaddingMode.PKCS7:
                                return MechanismType.AES_CBC_PAD;
                        }
                        break;
                    case CipherMode.CFB:
                        switch (FeedbackSize)
                        {
                            case 8:
                                return MechanismType.AES_CFB8;
                            case 64:
                                return MechanismType.AES_CFB64;
                            case 128:
                                return MechanismType.AES_CFB128;
                        }
                        break;
                    case CipherMode.CTS:
                        if (PaddingValue == PaddingMode.None) return MechanismType.AES_CTS;
                        break;
                    case CipherMode.ECB:
                        switch (PaddingValue)
                        {
                            case PaddingMode.None:
                                return MechanismType.AES_ECB;

                            case PaddingMode.PKCS7:
                                return MechanismType.AES_ECB_PAD;
                        }
                        break;
                    case CipherMode.OFB:
                        if (PaddingValue == PaddingMode.None) return MechanismType.AES_OFB;
                        break;
                }

                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Creates a symmetric AES decryptor object using the specified key and initialization vector (IV).
        /// </summary>
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
        /// Creates a symmetric encryptor object using the specified key and initialization vector (IV). 
        /// </summary>
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
        /// Generates a random key to use for the algorithm.
        /// </summary>
        public override void GenerateKey() 
        {
            CryptoKey key;
            Mechanism mech = new Mechanism(Microsoft.SPOT.Cryptoki.MechanismType.AES_KEY_GEN);
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
            byte[] iv = new byte[(BlockSizeValue + 7) / 8];

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
