namespace System.Security.Cryptography 
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography.X509Certificates;
    using System.Diagnostics;
    using Microsoft.SPOT.Cryptoki;

    /// <summary>
    /// Contains the typical parameters for the DSA algorithm.
    /// </summary>
    [Serializable]
    public struct DSAParameters
    {
        /// <summary>
        /// Prime value
        /// </summary>
        public byte[] P;
        /// <summary>
        /// Subprime value
        /// </summary>
        public byte[] Q;
        /// <summary>
        /// Base value
        /// </summary>
        public byte[] G;
        /// <summary>
        /// Public value
        /// </summary>
        public byte[] Y;
        public byte[] J;
        /// <summary>
        /// Private value
        /// </summary>
        [NonSerialized]  public byte[] X;
        public byte[] Seed;
        public int Counter;
    }

    /// <summary>
    /// Defines a wrapper object to access the cryptographic service provider (CSP) implementation of the DSA algorithm. This class cannot be inherited. 
    /// </summary>
    public sealed class DSACryptoServiceProvider : AsymmetricAlgorithm
    {
        public const int DefaultKeySize = 1024;

        private Mechanism     m_keyGenMech;
        private Mechanism     m_signatureMech;
        private MechanismType m_hashAlgorithm;
        private static KeySizes[] s_KeySizes = new KeySizes[] { new KeySizes(512, 2048, 64) };
        
        //
        // public constructors
        //

        /// <summary>
        /// Initializes a new instance of the DSACryptoServiceProvider class with the specified key size.
        /// </summary>
        /// <param name="keySize">The size of the key for the asymmetric algorithm in bits.</param>
        public DSACryptoServiceProvider(int keySize = DefaultKeySize)
            : this("", keySize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DSACryptoServiceProvider class with the specified service provider string and key size.
        /// </summary>
        /// <param name="serviceProvider">Service provider string that identifies the underlying crypto provider.</param>
        /// <param name="keySize">The size of the key for the asymmetric algorithm in bits.</param>
        public DSACryptoServiceProvider(string serviceProvider, int keySize = DefaultKeySize)
            : base(new Session(serviceProvider, MechanismType.DSA), true)
        {
            Init(null, keySize);
        }

        /// <summary>
        /// Initializes a new instance of the DSACryptoServiceProvider class with the specified Session object and key size.
        /// </summary>
        /// <param name="session">The Session context for which this crypto algorithm will be used.</param>
        /// <param name="keySize">The size of the key for the asymmetric algorithm in bits.</param>
        public DSACryptoServiceProvider(Session session, int keySize = DefaultKeySize)
            : base(session, false)
        {
            Init(null, keySize);
        }

        /// <summary>
        /// Initializes a new instance of the DSACryptoServiceProvider class with the specified CryptoKey object.
        /// </summary>
        /// <param name="dsaKey">The key object which the Aes algorithm will use for cryptographic operations.</param>
        public DSACryptoServiceProvider(CryptoKey dsaKey)
            : base(dsaKey.Session, false)
        {
            Init(dsaKey, -1);
        }

        private void Init(CryptoKey key, int keySize)
        {
            LegalKeySizesValue = s_KeySizes;

            m_keyGenMech = new Mechanism(MechanismType.DSA_KEY_PAIR_GEN);
            m_signatureMech = new Mechanism(MechanismType.DSA);
            m_hashAlgorithm   = MechanismType.SHA_1;

            if (key == null)
            {
                KeySize = keySize;
            }
            else
            {
                if (key.Type != CryptoKey.KeyType.DSA) throw new ArgumentException();

                KeyPair = key;
            }
        }

        protected override void GenerateKeyPair()
        {
            CryptokiAttribute[] attribs = new CryptokiAttribute[]
            { 
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.ValueLen, Utility.ConvertToBytes( KeySizeValue ))
            };

            KeyPairValue = CryptoKey.GenerateKeyPair(m_session, m_keyGenMech, attribs, null);
            OwnsKeyPair = true;
        }


        //
        // public properties
        //

        /// <summary>
        /// Gets a value that indicates whether the DSACryptoServiceProvider object contains only a public key.
        /// </summary>
        public bool PublicOnly 
        { 
            get 
            {
                return KeyPair.PublicOnly;
            }
        }

        /// <summary>
        /// Hash algorithm to use when generating a signature over arbitrary data
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

        //
        // public methods
        //

        /// <summary>
        /// Imports the specified DSAParameters.
        /// </summary>
        /// <param name="parameters">The parameters for DSA.</param>
        public void ImportParameters(DSAParameters parameters)
        {
            bool fIncludePrivate = parameters.X != null;
            int cntAttribs = fIncludePrivate ? 7 : 6;

            CryptokiAttribute[] attribs = new CryptokiAttribute[cntAttribs];

            attribs[0] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.Class, 
                Utility.ConvertToBytes((int)(fIncludePrivate ? CryptokiClass.PRIVATE_KEY : CryptokiClass.PUBLIC_KEY)));
            attribs[1] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.KeyType, 
                Utility.ConvertToBytes((int)CryptoKey.KeyType.DSA));
            attribs[2] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.Prime, parameters.P);
            attribs[3] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.Subprime, parameters.Q);
            attribs[4] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.Base, parameters.G);
            attribs[5] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.PublicExponent, parameters.Y);

            if (fIncludePrivate)
            {
                attribs[6] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.PrivateExponent, parameters.X);
            }

            KeyPair = CryptoKey.LoadKey(m_session, attribs);
            OwnsKeyPair = true;
        }

        /// <summary>
        /// Exports the DSAParameters.
        /// </summary>
        /// <param name="includePrivateParameters">true to include the private key; otherwise, false.</param>
        /// <returns>The parameters for DSA.</returns>
        public DSAParameters ExportParameters(bool includePrivateParameters)
        {
            DSAParameters parms = new DSAParameters();

            int keySizeBytes = KeySizeValue / 8;
            int cntAttribs = includePrivateParameters ? 5 : 4;

            parms.P = new byte[keySizeBytes];
            parms.Q = new byte[160 / 8];
            parms.G = new byte[keySizeBytes];
            parms.Y = new byte[keySizeBytes];

            if (includePrivateParameters)
            {
                parms.X = new byte[160 / 8];
            }
           

            CryptokiAttribute[] attribs = new CryptokiAttribute[cntAttribs];
            
            attribs[0] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.Prime, parms.P);
            attribs[1] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.Subprime, parms.Q);
            attribs[2] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.Base, parms.G);
            attribs[3] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.PublicExponent, parms.Y);

            if (includePrivateParameters) 
            {
                attribs[4] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.PrivateExponent, parms.X);
            }

            if (!KeyPair.GetAttributeValues(ref attribs))
            {
                throw new CryptographicException();
            }

            return parms;
        }

        /// <summary>
        /// Computes the hash value of the specified input stream and signs the resulting hash value.
        /// </summary>
        /// <param name="inputStream">The input data for which to compute the hash.</param>
        /// <returns>The DSA signature for the specified data.</returns>
        public byte[] SignData(Stream inputStream) 
        {
            byte[] data = new byte[(int)inputStream.Length];

            int len = inputStream.Read(data, 0, data.Length);

            return SignData(data, 0, len);
        }

        /// <summary>
        /// Computes the hash value of the specified byte array and signs the resulting hash value.
        /// </summary>
        /// <param name="buffer">The input data for which to compute the hash.</param>
        /// <returns>The DSA signature for the specified data.</returns>
        public byte[] SignData(byte[] buffer) 
        {
            return SignData(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Computes the hash value of the specified byte array and signs the resulting hash value.
        /// </summary>
        /// <param name="buffer">The input data for which to compute the hash.</param>
        /// <param name="offset">The offset index into the buffer</param>
        /// <param name="count">The number of bytes from the offset to process for the hash operation.</param>
        /// <returns>The DSA signature for the specified data.</returns>
        public byte[] SignData(byte[] buffer, int offset, int count) 
        {
            byte[] retVal;

            if (buffer == null)
            {
                throw new ArgumentNullException();
            }
            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (count < 0 || count > buffer.Length - offset)
            {
                throw new ArgumentOutOfRangeException();
            }

            m_signatureMech.Parameter = Utility.ConvertToBytes((int)m_hashAlgorithm);

            using (CryptokiSign sig = new CryptokiSign(m_session, m_signatureMech, KeyPair))
            {
                retVal = sig.Sign(buffer, offset, count);
            }

            return retVal;
        }

        /// <summary>
        /// Verifies the specified signature data by comparing it to the signature computed for the specified data.
        /// </summary>
        /// <param name="rgbData">The data that was signed.</param>
        /// <param name="rgbSignature">The signature data to be verified.</param>
        /// <returns>true if the signature verifies as valid; otherwise, false.</returns>
        public bool VerifyData(byte[] rgbData, byte[] rgbSignature) 
        {
            if (rgbData == null || rgbSignature == null) throw new ArgumentNullException();

            bool isValid = false;

            m_signatureMech.Parameter = Utility.ConvertToBytes((int)m_hashAlgorithm);

            using (CryptokiVerify ver = new CryptokiVerify(m_session, m_signatureMech, KeyPair))
            {
                isValid = ver.Verify(rgbData, 0, rgbData.Length, rgbSignature, 0, rgbSignature.Length);
            }

            return isValid;
        }

        /// <summary>
        /// Creates the DSA signature for the specified data.
        /// </summary>
        /// <param name="rgbHash">The data to be signed.</param>
        /// <returns>The digital signature for the specified data.</returns>
        public byte[] CreateSignature(byte[] rgbHash) 
        {
            return SignHash(rgbHash, m_hashAlgorithm);
        }

        /// <summary>
        /// Verifies the DSA signature for the specified data.
        /// </summary>
        /// <param name="rgbHash">The data signed with rgbSignature.</param>
        /// <param name="rgbSignature">The signature to be verified for rgbData.</param>
        /// <returns>true if rgbSignature matches the signature computed using the specified hash algorithm and key on rgbHash; otherwise, false.</returns>
        public bool VerifySignature(byte[] rgbHash, byte[] rgbSignature) 
        {
            return VerifyHash(rgbHash, m_hashAlgorithm, rgbSignature);
        }

        /// <summary>
        /// Computes the signature for the specified hash value by encrypting it with the private key.
        /// </summary>
        /// <param name="rgbHash">The hash value of the data to be signed.</param>
        /// <param name="hashAlgorithm">The hash algorithm used to create the hash value of the data.</param>
        /// <returns>The DSA signature for the specified hash value.</returns>
        public byte[] SignHash(byte[] rgbHash, MechanismType hashAlgorithm) 
        {
            if (rgbHash == null)
                throw new ArgumentNullException();
            if (PublicOnly || rgbHash.Length == 0)
                throw new CryptographicException();

            byte[] sig = null;

            m_signatureMech.Parameter = Utility.ConvertToBytes((int)(hashAlgorithm | MechanismType.SIGN_NO_NODIGEST_FLAG));

            using (CryptokiSign sign = new CryptokiSign(m_session, m_signatureMech, KeyPair))
            {
                sig = sign.Sign(rgbHash, 0, rgbHash.Length);
            }

            return sig;
        }

        /// <summary>
        /// Verifies the specified signature data by comparing it to the signature computed for the specified hash value.
        /// </summary>
        /// <param name="rgbHash">The hash value of the data to be signed.</param>
        /// <param name="hashAlgorithm">The hash algorithm used to create the hash value of the data.</param>
        /// <param name="rgbSignature">The signature data to be verified.</param>
        /// <returns>true if the signature verifies as valid; otherwise, false.</returns>
        public bool VerifyHash(byte[] rgbHash, MechanismType hashAlgorithm, byte[] rgbSignature) 
        {
            if (rgbHash == null || rgbSignature == null)
                throw new ArgumentNullException();
            if (rgbHash.Length == 0)
                throw new CryptographicException();

            bool isValid = false;

            m_signatureMech.Parameter = Utility.ConvertToBytes((int)(hashAlgorithm | MechanismType.SIGN_NO_NODIGEST_FLAG));

            using (CryptokiVerify ver = new CryptokiVerify(m_session, m_signatureMech, KeyPair))
            {
                isValid = ver.Verify(rgbHash, 0, rgbHash.Length, rgbSignature, 0, rgbSignature.Length);
            }

            return isValid;
        }
    }
}
