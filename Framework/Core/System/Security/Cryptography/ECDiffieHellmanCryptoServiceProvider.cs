using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.SPOT.Cryptoki;

namespace System.Security.Cryptography
{
    /// <summary>
    /// Key derivation functions used to transform the raw secret agreement into key material
    /// </summary>
    public enum ECDiffieHellmanKeyDerivationFunction
    {
        Hash,
        Hmac,
        Tls,
        None
    }

    /// <summary>
    /// Wrapper for elliptic curve Diffie-Hellman key exchange
    /// </summary>
    public sealed class ECDiffieHellmanCryptoServiceProvider : AsymmetricAlgorithm
    {
        public const int DefaultKeySize = 521;

        private static KeySizes[] s_legalKeySizes = new KeySizes[] { new KeySizes(256, 384, 128), new KeySizes(521, 521, 0) };

        private MechanismType m_hashAlgorithm = MechanismType.SHA256_KEY_DERIVATION;
        private CryptoKey m_hmacKey;
        private ECDiffieHellmanKeyDerivationFunction m_kdf = ECDiffieHellmanKeyDerivationFunction.Hash;
        private byte[] m_label;
        private byte[] m_secretAppend;
        private byte[] m_secretPrepend;
        private byte[] m_seed;

        private struct ECDH1_PARAMS
        {
            public MechanismType KDF;
            public Byte[] SharedData;
            public Byte[] PeerPublicData;

            public byte[] ToArray()
            {
                int len = 3*4 + PeerPublicData.Length;
                int offset = 0;
                
                if(SharedData != null) len += SharedData.Length;

                byte[] ret = new byte[len];

                Utility.ConvertToBytes((int)KDF, ret, offset);
                offset += 4;

                if (SharedData != null && SharedData.Length > 0)
                {
                    Utility.ConvertToBytes(SharedData.Length, ret, offset);
                    offset += 4;
                    Array.Copy(SharedData, 0, ret, offset, SharedData.Length);
                    offset += SharedData.Length;
                }
                else
                {
                    // ret is initiliazed to zeros, so no need to copy data
                    offset += 4;
                }

                Utility.ConvertToBytes(PeerPublicData.Length, ret, offset);
                offset += 4;

                Array.Copy(PeerPublicData, 0, ret, offset, PeerPublicData.Length);

                return ret;
            }
        }

        //
        // Constructors
        //

        /// <summary>
        /// Initializes a new instance of the ECDiffieHellmanCng class with a random key pair, using the specified key size.
        /// </summary>
        /// <param name="keySize">The size of the key. Valid key sizes are 256, 384, and 521 bits.</param>
        public ECDiffieHellmanCryptoServiceProvider(int keySize = DefaultKeySize)
            : this("", keySize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ECDiffieHellmanCng class with a random key pair, using the specified service provider string and key size.
        /// </summary>
        /// <param name="serviceProvider">Service provider string that identifies the underlying crypto provider.</param>
        /// <param name="keySize">The size of the key. Valid key sizes are 256, 384, and 521 bits.</param>
        public ECDiffieHellmanCryptoServiceProvider(string serviceProvider, int keySize = DefaultKeySize) : 
            base(new Session(serviceProvider, MechanismType.ECDH1_DERIVE), true)
        {
            Init(null, keySize);
        }

        /// <summary>
        /// Initializes a new instance of the ECDiffieHellmanCng class with a random key pair, using the specified Cryptoki Session object and key size.
        /// </summary>
        /// <param name="session">The Session context for which this crypto algorithm will be used.</param>
        /// <param name="keySize">The size of the key. Valid key sizes are 256, 384, and 521 bits.</param>
        public ECDiffieHellmanCryptoServiceProvider(Session session, int keySize = DefaultKeySize)
            : base(session, false)
        {
            Init(null, keySize);
        }

        /// <summary>
        /// Initializes a new instance of the ECDiffieHellmanCng class with a random key pair, using the specified Key object.
        /// </summary>
        /// <param name="key">The key object which the ECDH algorithm will use for cryptographic operations.</param>
        public ECDiffieHellmanCryptoServiceProvider(CryptoKey key) 
            : base(key.Session, false)
        {
            Init(key, -1);
        }

        private void Init(CryptoKey key, int keySize)
        {
            LegalKeySizesValue = s_legalKeySizes;

            if (key == null)
            {
                KeySize = DefaultKeySize;
            }
            else
            {
                if (key.Type != CryptoKey.KeyType.EC)
                {
                    throw new ArgumentException();
                }

                KeyPair = key;
            }
        }

        /// <summary>
        /// Gets or sets the hash algorithm to use when generating key material.
        /// </summary>
        public MechanismType HashAlgorithm
        {
            get
            {
                return m_hashAlgorithm;
            }

            set
            {
                m_hashAlgorithm = value;
            }
        }

        /// <summary>
        /// Gets or sets the Hash-based Message Authentication Code (HMAC) key to use when deriving key material.
        /// </summary>
        public CryptoKey HmacKey
        {
            get { return m_hmacKey; }
            set { m_hmacKey = value; }
        }

        /// <summary>
        /// Gets or sets the key derivation function for the ECDiffieHellmanCng class.
        /// </summary>
        public ECDiffieHellmanKeyDerivationFunction KeyDerivationFunction
        {
            get
            {
                return m_kdf;
            }

            set
            {
                if (value < ECDiffieHellmanKeyDerivationFunction.Hash || value > ECDiffieHellmanKeyDerivationFunction.None)
                {
                    throw new ArgumentOutOfRangeException();
                }

                m_kdf = value;
            }
        }

        /// <summary>
        /// Gets or sets the label value that is used for key derivation.
        /// </summary>
        public byte[] Label
        {
            get { return m_label; }
            set { m_label = value; }
        }

        /// <summary>
        /// Gets or sets a value that will be appended to the secret agreement when generating key material.
        /// </summary>
        public byte[] SecretAppend
        {
            get { return m_secretAppend; }
            set { m_secretAppend = value; }
        }

        /// <summary>
        /// Gets or sets a value that will be added to the beginning of the secret agreement when deriving key material.
        /// </summary>
        public byte[] SecretPrepend
        {
            get { return m_secretPrepend; }
            set { m_secretPrepend = value; }
        }

        /// <summary>
        /// Gets or sets the seed value that will be used when deriving key material.
        /// </summary>
        public byte[] Seed
        {
            get { return m_seed; }
            set { m_seed = value; }
        }

        /// <summary>
        /// Gets the public key that can be used by another ECDiffieHellmanCng object to generate a shared secret agreement. 
        /// </summary>
        public byte[] PublicKey
        {
            get
            {
                byte[] retVal = new byte[(KeySizeValue+7)/8 * 2];
                byte[] len = new byte[4];

                CryptokiAttribute[] template = new CryptokiAttribute[] {
                    new CryptokiAttribute(CryptokiAttribute.CryptokiType.PublicExponent, retVal),
                    new CryptokiAttribute(CryptokiAttribute.CryptokiType.ValueLen, len)
                };

                KeyPair.GetAttributeValues(ref template);


                int size = Utility.ConvertToInt32(len);

                if (size < retVal.Length)
                {
                    byte[] tmp = new byte[size];

                    Array.Copy(retVal, tmp, tmp.Length);

                    retVal = tmp;
                }
                return retVal;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the secret agreement is used as a Hash-based Message Authentication Code (HMAC) key to derive key material.
        /// </summary>
        public bool UseSecretAgreementAsHmacKey
        {
            get { return HmacKey == null; }
        }

        /// <summary>
        /// Derives the key material that is generated from the secret agreement between two parties, given an ECDiffieHellmanPublicKey object that contains the second party's public key. 
        /// (Secret Prepend/Append Not Implemented yet)
        /// </summary>
        /// <param name="otherPartyPublicKey">A byte array that contains the public part of the Elliptic Curve Diffie-Hellman (ECDH) key from the other party in the key exchange.</param>
        /// <returns>A byte array that contains the key material. This information is generated from the secret agreement that is calculated from the current object's private key and the specified public key.</returns>
        public CryptoKey DeriveKeyMaterial(byte[] otherPartyPublicKey)
        {
            if (otherPartyPublicKey == null)
            {
                throw new ArgumentNullException();
            }
            ECDH1_PARAMS ecData = new ECDH1_PARAMS();

            ecData.PeerPublicData = otherPartyPublicKey;

            if (KeyDerivationFunction == ECDiffieHellmanKeyDerivationFunction.Hash)
            {
                //byte[] secretAppend = SecretAppend == null ? null : SecretAppend.Clone() as byte[];
                //byte[] secretPrepend = SecretPrepend == null ? null : SecretPrepend.Clone() as byte[];

                switch(m_hashAlgorithm)
                {
                    case MechanismType.SHA_1:
                    case MechanismType.SHA1_KEY_DERIVATION:
                        ecData.KDF = MechanismType.SHA1_KEY_DERIVATION;
                        break;

                    case MechanismType.SHA256_KEY_DERIVATION:
                    case MechanismType.SHA256:
                        ecData.KDF = MechanismType.SHA256_KEY_DERIVATION;
                        break;

                    case MechanismType.SHA384_KEY_DERIVATION:
                    case MechanismType.SHA384:
                        ecData.KDF = MechanismType.SHA256_KEY_DERIVATION;
                        break;


                    case MechanismType.SHA512_KEY_DERIVATION:
                    case MechanismType.SHA512:
                        ecData.KDF = MechanismType.SHA512_KEY_DERIVATION;
                        break;

                    case MechanismType.MD5_KEY_DERIVATION:
                    case MechanismType.MD5:
                        ecData.KDF = MechanismType.MD5_KEY_DERIVATION;
                        break;

                    default:
                        ecData.KDF = m_hashAlgorithm;
                        break;
                }
            }
            else if (KeyDerivationFunction == ECDiffieHellmanKeyDerivationFunction.Hmac)
            {
                //byte[] hmacKey = HmacKey == null ? null : HmacKey.Clone() as byte[];
                //byte[] secretAppend = SecretAppend == null ? null : SecretAppend.Clone() as byte[];
                //byte[] secretPrepend = SecretPrepend == null ? null : SecretPrepend.Clone() as byte[];

                ecData.KDF = MechanismType.SHA256_HMAC;
            }
            else if(KeyDerivationFunction == ECDiffieHellmanKeyDerivationFunction.Tls)
            {
                //byte[] label = Label == null ? null : Label.Clone() as byte[];
                //byte[] seed = Seed == null ? null : Seed.Clone() as byte[];

                //if (label == null || seed == null)
                //{
                //    throw new InvalidOperationException();
                //}

                ecData.KDF = MechanismType.TLS_MASTER_KEY_DERIVE_DH;
            }
            else
            {
                ecData.KDF = MechanismType.NULL_KEY_DERIVATION;
            }

            return KeyPair.DeriveKey(new Mechanism(MechanismType.ECDH1_DERIVE, ecData.ToArray()), null);
        }

        protected override void GenerateKeyPair()
        {
            CryptokiAttribute[] attribs = new CryptokiAttribute[]
            { 
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.ValueLen, Utility.ConvertToBytes( KeySizeValue ))
            };

            KeyPairValue = CryptoKey.GenerateKeyPair(m_session, new Mechanism(MechanismType.ECDH_KEY_PAIR_GEN), attribs, null);
            OwnsKeyPair = true;
        }
    }
}

