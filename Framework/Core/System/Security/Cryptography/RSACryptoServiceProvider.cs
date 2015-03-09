namespace System.Security.Cryptography 
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography.X509Certificates;
    using Microsoft.SPOT.Cryptoki;

    /// <summary>
    /// Represents the standard parameters for the RSA algorithm.
    /// </summary>
    [Serializable]
    public struct RSAParameters
    {
        /// <summary>
        /// Public exponent value
        /// </summary>
        public byte[] Exponent;
        /// <summary>
        /// Modulus value
        /// </summary>
        public byte[] Modulus;
        /// <summary>
        /// Prime1 value
        /// </summary>
        [NonSerialized]
        public byte[] P;
        /// <summary>
        /// Prime2 value
        /// </summary>
        [NonSerialized]
        public byte[] Q;
        /// <summary>
        /// Exponent1 value
        /// </summary>
        [NonSerialized]
        public byte[] DP;
        /// <summary>
        /// Exponent2 value
        /// </summary>
        [NonSerialized]
        public byte[] DQ;
        /// <summary>
        /// Coefficient value
        /// </summary>
        [NonSerialized]
        public byte[] InverseQ;
        /// <summary>
        /// Private exponent value
        /// </summary>
        [NonSerialized]
        public byte[] D;
    }

    /// <summary>
    /// Performs asymmetric encryption and decryption using the implementation of the RSA algorithm provided by the cryptographic service provider (CSP). This class cannot be inherited.
    /// </summary>
    public sealed class RSACryptoServiceProvider : AsymmetricAlgorithm
    {
        public const int DefaultKeySize = 1024;

        private Mechanism m_encryptionMech;
        private MechanismType m_hashMech;
        private Mechanism m_keyGenMech;

        private Mechanism m_signHashMech;
        private static KeySizes[] s_KeySizes = new KeySizes[] { new KeySizes(384, 4096, 8) };

        /// <summary>
        /// Initializes a new instance of the RSACryptoServiceProvider class with the specified key size.
        /// </summary>
        /// <param name="keySize">The size of the key to use in bits.</param>
        public RSACryptoServiceProvider(int keySize = DefaultKeySize)
            : this("", keySize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RSACryptoServiceProvider class with the specified crypto service provider and key size.
        /// </summary>
        /// <param name="serviceProvider">Defines the RSA crypto service provider to be used.</param>
        /// <param name="keySize">The size of the key to use in bits.</param>
        public RSACryptoServiceProvider(string serviceProvider, int keySize = DefaultKeySize)
            : base(new Session(serviceProvider, MechanismType.RSA_PKCS), true)
        {
            Init(null, keySize);
        }

        /// <summary>
        /// Initializes a new instance of the RSACryptoServiceProvider class with the specified session context and key size.
        /// </summary>
        /// <param name="session">Defines the session context to be used to perform RSA operations.</param>
        /// <param name="keySize">The size of the key to use in bits.</param>
        public RSACryptoServiceProvider(Session session, int keySize = DefaultKeySize)
            : base(session, false)
        {
            Init(null, keySize);
        }

        /// <summary>
        /// Initializes a new instance of the RSACryptoServiceProvider class with the specified key object.
        /// </summary>
        /// <param name="key">The key to be used for RSA operations.</param>
        public RSACryptoServiceProvider(CryptoKey key)
            : base(key.Session, false)
        {
            Init(key, -1);
        }

        private void Init(CryptoKey key, int keySize)
        {
            LegalKeySizesValue = s_KeySizes;

            m_signHashMech = new Mechanism(MechanismType.RSA_PKCS); 
            m_encryptionMech = new Mechanism(MechanismType.RSA_PKCS);
            m_keyGenMech = new Mechanism(MechanismType.RSA_PKCS_KEY_PAIR_GEN);
            HashAlgorithm = MechanismType.SHA_1;

            if (key == null)
            {
                KeySize = keySize;
            }
            else
            {
                if (key.Type != CryptoKey.KeyType.RSA) throw new ArgumentException();

                KeyPair = key;
            }
        }

        /// <summary>
        /// Gets or sets the hash algorithm to use when generating a signature over arbitrary data
        /// </summary>
        public MechanismType HashAlgorithm
        {
            get
            {
                return m_hashMech;
            }

            set
            {
                m_hashMech = value;
            }
        }

        /// <summary>
        /// Gets or sets the RSA encryption mechanism.
        /// </summary>
        public MechanismType EncryptionMechanism
        {
            get { return m_encryptionMech.Type; }
            set
            {
                if (value != m_encryptionMech.Type)
                {
                    switch(value)
                    {
                        case MechanismType.RSA_PKCS:
                        case MechanismType.RSA_PKCS_OAEP:
                        case MechanismType.RSA_PKCS_PSS:
                        case MechanismType.RSA_X_509:
                        case MechanismType.RSA_X9_31:
                            break;

                        default:
                            throw new ArgumentException();
                    }

                    // TODO: We may need to obtain a new session, if the current doesn't support the new enc mech.
                    m_encryptionMech.Type = value;
                }
            }
        }

        /// <summary>
        /// Generates a new RSA key pair.
        /// </summary>
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
        /// Gets a value that indicates whether the RSACryptoServiceProvider object contains only a public key.
        /// </summary>
        public bool PublicOnly 
        { 
            get 
            {
                return KeyPair.PublicOnly;
            }
        }

        /// <summary>
        /// Imports the specified RSAParameters.
        /// </summary>
        /// <param name="parameters">The parameters for RSA.</param>
        public void ImportParameters(RSAParameters parameters)
        {
            bool fIncludePrivate = parameters.D != null;
            int cntAttribs = fIncludePrivate ? 10 : 4;

            CryptokiAttribute[] attribs = new CryptokiAttribute[cntAttribs];

            attribs[0] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.Class,
                Utility.ConvertToBytes((int)(fIncludePrivate ? CryptokiClass.PRIVATE_KEY : CryptokiClass.PUBLIC_KEY)));
            attribs[1] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.KeyType,
                Utility.ConvertToBytes((int)CryptoKey.KeyType.RSA));
            attribs[2] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.Modulus       , parameters.Modulus );
            attribs[3] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.PublicExponent, parameters.Exponent);

            if (fIncludePrivate)
            {
                attribs[4] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.PrivateExponent, parameters.D);
                attribs[5] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.Exponent1      , parameters.DP);
                attribs[6] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.Exponent2      , parameters.DQ);
                attribs[7] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.Coefficent     , parameters.InverseQ);
                attribs[8] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.Prime1         , parameters.P);
                attribs[9] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.Prime2         , parameters.Q);
            }

            KeyPair = CryptoKey.LoadKey(m_session, attribs);
            OwnsKeyPair = true;
        }

        /// <summary>
        /// Exports the RSAParameters.
        /// </summary>
        /// <param name="includePrivateParameters">true to include private parameters; otherwise, false.</param>
        /// <returns>The parameters for RSA.</returns>
        public RSAParameters ExportParameters(bool includePrivateParameters)
        {
            RSAParameters parms = new RSAParameters();

            int keySizeBytes = KeySizeValue / 8;
            int cntAttribs = includePrivateParameters ? 8 : 2;

            parms.Modulus = new byte[keySizeBytes];
            parms.Exponent = new byte[3];

            if (includePrivateParameters)
            {
                int halfKeyBytes = keySizeBytes / 2;

                parms.D        = new byte[keySizeBytes];
                parms.DP       = new byte[halfKeyBytes];
                parms.DQ       = new byte[halfKeyBytes];
                parms.InverseQ = new byte[halfKeyBytes];
                parms.P        = new byte[halfKeyBytes];
                parms.Q        = new byte[halfKeyBytes];
            }


            CryptokiAttribute[] attribs = new CryptokiAttribute[cntAttribs];

            attribs[0] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.Modulus       , parms.Modulus );
            attribs[1] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.PublicExponent, parms.Exponent);

            if (includePrivateParameters)
            {
                attribs[2] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.PrivateExponent, parms.D);
                attribs[3] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.Exponent1      , parms.DP);
                attribs[4] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.Exponent2      , parms.DQ);
                attribs[5] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.Coefficent     , parms.InverseQ);
                attribs[6] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.Prime1         , parms.P);
                attribs[7] = new CryptokiAttribute(CryptokiAttribute.CryptokiType.Prime2         , parms.Q);
            }

            if (!KeyPair.GetAttributeValues(ref attribs))
            {
                throw new CryptographicException();
            }

            return parms;
        }

        /// <summary>
        /// Computes the hash value of the specified input stream using the default hash algorithm, and signs the resulting hash value.
        /// </summary>
        /// <param name="inputStream">The input data for which to compute the hash.</param>
        /// <returns>The RSA signature for the specified data.</returns>
        public byte[] SignData(Stream inputStream)
        {
            return SignData(inputStream, null);
        }

        /// <summary>
        /// Computes the hash value of the specified input stream using the specified hash algorithm, and signs the resulting hash value.
        /// </summary>
        /// <param name="inputStream">The input data for which to compute the hash.</param>
        /// <param name="hash">The hash algorithm to use to create the hash value.</param>
        /// <returns>The RSA signature for the specified data.</returns>
        public byte[] SignData(Stream inputStream, HashAlgorithm hash) 
        {
            byte[] retVal;
            byte[] data;

            if (hash != null)
            {
                m_signHashMech.Parameter = Utility.ConvertToBytes((int)hash.HashType);
            }
            else
            {
                m_signHashMech.Parameter = Utility.ConvertToBytes((int)m_hashMech);
            }

            // TODO: look at breaking this up for stream so that it i only reads x amount at a time
            data = new byte[(int)inputStream.Length];
            
            int len = inputStream.Read(data, 0, data.Length);

            using (CryptokiSign sig = new CryptokiSign(m_session, m_signHashMech, KeyPair))
            {
                retVal = sig.Sign(data, 0, len);
            }

            return retVal;
        }

        /// <summary>
        /// Computes the hash value of the specified byte array using the default hash algorithm, and signs the resulting hash value.
        /// </summary>
        /// <param name="buffer">The input data for which to compute the hash.</param>
        /// <returns>The RSA signature for the specified data.</returns>
        public byte[] SignData(byte[] buffer)
        {
            return SignData(buffer, null);
        }

        /// <summary>
        /// Computes the hash value of the specified byte array using the specified hash algorithm, and signs the resulting hash value.
        /// </summary>
        /// <param name="buffer">The input data for which to compute the hash.</param>
        /// <param name="hash">The hash algorithm to use to create the hash value.</param>
        /// <returns>The RSA signature for the specified data.</returns>
        public byte[] SignData(byte[] buffer, HashAlgorithm hash) 
        {
            return SignData(buffer, 0, buffer.Length, hash);
        }

        /// <summary>
        /// Computes the hash value of a subset of the specified byte array using the specified hash algorithm, and signs the resulting hash value.
        /// </summary>
        /// <param name="buffer">The input data for which to compute the hash.</param>
        /// <param name="offset">The offset into the array from which to begin using data.</param>
        /// <param name="count">The number of bytes in the array to use as data.</param>
        /// <param name="hash">The hash algorithm to use to create the hash value.</param>
        /// <returns>The RSA signature for the specified data.</returns>
        public byte[] SignData(byte[] buffer, int offset, int count, HashAlgorithm hash)
        {
            byte[] retVal;

            if (hash != null)
            {
                //return SignHash(hash.ComputeHash(buffer, offset, count));
                m_signHashMech.Parameter = Utility.ConvertToBytes((int)hash.HashType);
            }
            else
            {
                m_signHashMech.Parameter = Utility.ConvertToBytes((int)m_hashMech);
            }

            using (CryptokiSign sig = new CryptokiSign(m_session, m_signHashMech, KeyPair))
            {
                retVal = sig.Sign(buffer, offset, count);
            }

            return retVal;
        }

        /// <summary>
        /// Verifies that a digital signature is valid by determining the hash value in the signature using the provided public key and comparing it to the hash value of the provided data.
        /// </summary>
        /// <param name="buffer">The data that was signed.</param>
        /// <param name="signature">The signature data to be verified.</param>
        /// <returns>true if the signature is valid; otherwise, false.</returns>
        public bool VerifyData(byte[] buffer, byte[] signature)
        {
            return VerifyData(buffer, null, signature);
        }

        /// <summary>
        /// Verifies that a digital signature is valid by determining the hash value in the signature using the provided public key and comparing it to the hash value of the provided data.
        /// </summary>
        /// <param name="buffer">The data that was signed.</param>
        /// <param name="halg">The hash algorithm used to create the hash value of the data.</param>
        /// <param name="signature">The signature data to be verified.</param>
        /// <returns>true if the signature is valid; otherwise, false.</returns>
        public bool VerifyData(byte[] buffer, HashAlgorithm halg, byte[] signature) 
        {
            bool retVal;

            if (halg != null)
            {
                //return VerifyHash(halg.ComputeHash(buffer), signature);
                m_signHashMech.Parameter = Utility.ConvertToBytes((int)halg.HashType);
            }
            else
            {
                m_signHashMech.Parameter = Utility.ConvertToBytes((int)m_hashMech);
            }

            using (CryptokiVerify ver = new CryptokiVerify(m_session, m_signHashMech, KeyPair))
            {
                retVal = ver.Verify(buffer, 0, buffer.Length, signature, 0, signature.Length);
            }

            return retVal;
        }

        /// <summary>
        /// Computes the signature for the specified hash value by encrypting it with the private key.
        /// </summary>
        /// <param name="rgbHash">The hash value of the data to be signed.</param>
        /// <param name="algorithm">The hash algorithm used to create the hash value of the data.</param>
        /// <returns>The RSA signature for the specified hash value.</returns>
        public byte[] SignHash(byte[] rgbHash, MechanismType algorithm) 
        {
            if (rgbHash == null)
                throw new ArgumentNullException();
            if (PublicOnly)
                throw new CryptographicException();

            byte[] sig = null;

            m_signHashMech.Parameter = Utility.ConvertToBytes(((int)(algorithm | MechanismType.SIGN_NO_NODIGEST_FLAG)));

            using (CryptokiSign sign = new CryptokiSign(m_session, m_signHashMech, KeyPair))
            {
                sig = sign.Sign(rgbHash, 0, rgbHash.Length);
            }

            return sig;
        }

        /// <summary>
        /// Verifies that a digital signature is valid by determining the hash value in the signature using the provided public key and comparing it to the provided hash value.
        /// </summary>
        /// <param name="rgbHash">The hash value of the signed data.</param>
        /// <param name="algorithm">The hash algorithm used to create the hash value of the data.</param>
        /// <param name="rgbSignature">The signature data to be verified.</param>
        /// <returns>true if the signature is valid; otherwise, false.</returns>
        public bool VerifyHash(byte[] rgbHash, MechanismType algorithm, byte[] rgbSignature) 
        {
            if (rgbHash == null || rgbSignature == null)
                throw new ArgumentNullException();

            bool isValid = false;

            m_signHashMech.Parameter = Utility.ConvertToBytes(((int)(algorithm | MechanismType.SIGN_NO_NODIGEST_FLAG)));

            using (CryptokiVerify ver = new CryptokiVerify(m_session, m_signHashMech, KeyPair))
            {
                isValid = ver.Verify(rgbHash, 0, rgbHash.Length, rgbSignature, 0, rgbSignature.Length);
            }

            return isValid;
        }

        /// <summary>
        /// Encrypts data with the RSA algorithm.
        /// </summary>
        /// <param name="rgb">The data to be encrypted.</param>
        /// <returns>The encrypted data.</returns>
        public byte[] Encrypt(byte[] rgb)
        {
            if (rgb == null)
                throw new ArgumentNullException();

            byte[] data = null;

            using (Encryptor enc = new Encryptor(m_session, m_encryptionMech, KeyPair, KeyPair.Size, KeyPair.Size))
            {
                data = enc.TransformFinalBlock(rgb, 0, rgb.Length);
            }

            return data;
        }

        /// <summary>
        /// Decrypts data with the RSA algorithm.
        /// </summary>
        /// <param name="rgb">The data to be decrypted.</param>
        /// <returns>The decrypted data, which is the original plain text before encryption.</returns>
        public byte [] Decrypt(byte[] rgb)
        {
            if (rgb == null)
                throw new ArgumentNullException();

            byte[] data = null;

            using (Decryptor decrypt = new Decryptor(m_session, m_encryptionMech, KeyPair, KeyPair.Size, KeyPair.Size))
            {
                data = decrypt.TransformFinalBlock(rgb, 0, rgb.Length);
            }

            return data;
        }
    }
}
