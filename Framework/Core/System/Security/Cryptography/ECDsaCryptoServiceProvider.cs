using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using Microsoft.SPOT.Cryptoki;

namespace System.Security.Cryptography
{
    /// <summary>
    /// Provides the Elliptic Curve Digital Signature Algorithm (ECDSA). 
    /// </summary>
    public sealed class ECDsaCryptoServiceProvider : AsymmetricAlgorithm
    {
        public const int DefaultKeySize = 521;

        private static KeySizes[] s_legalKeySizes = new KeySizes[] { new KeySizes(256, 384, 128), new KeySizes(521, 521, 0) };

        private Mechanism     m_signMech;
        private MechanismType m_hashAlgorithm;

        //
        // Constructors
        //

        /// <summary>
        /// Initializes a new instance of the ECDsaCng class with a random key pair, using the specified key size.
        /// </summary>
        /// <param name="keySize">The size of the key. Valid key sizes are 256, 384, and 521 bits.</param>
        public ECDsaCryptoServiceProvider(int keySize = DefaultKeySize)
            : this("", keySize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ECDsaCng class with a random key pair, using the specified service provider and key size.
        /// </summary>
        /// <param name="serviceProvider">Service provider string that identifies the underlying crypto provider.</param>
        /// <param name="keySize">The size of the key. Valid key sizes are 256, 384, and 521 bits.</param>
        public ECDsaCryptoServiceProvider(string serviceProvider, int keySize = DefaultKeySize) 
            : base(new Session(serviceProvider, MechanismType.ECDSA), true)
        {
            Init(null, keySize);
        }

        /// <summary>
        /// Initializes a new instance of the ECDsaCng class with a random key pair, using the specified Session context and key size.
        /// </summary>
        /// <param name="session">The Session context for which this crypto algorithm will be used.</param>
        /// <param name="keySize">The size of the key. Valid key sizes are 256, 384, and 521 bits.</param>
        public ECDsaCryptoServiceProvider(Session session, int keySize = DefaultKeySize)
            : base(session, false)
        {
            Init(null, keySize);
        }

        /// <summary>
        /// Initializes a new instance of the ECDsaCng class with a random key pair, using the specified key object.
        /// </summary>
        /// <param name="key">The key object which the ECDsa algorithm will for cryptographic operations.</param>
        public ECDsaCryptoServiceProvider(CryptoKey key)
            : base(key.Session, false)
        {
            Init(key, -1);
        }

        private void Init(CryptoKey key, int keySize) 
        {
            LegalKeySizesValue = s_legalKeySizes;

            m_signMech    = new Mechanism(MechanismType.ECDSA);
            HashAlgorithm = MechanismType.SHA256;

            if (key == null)
            {
                KeySize = keySize;
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
        /// Gets or sets the hash algorithm to use when signing and verifying data.
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

        protected override void GenerateKeyPair()
        {
            CryptokiAttribute[] attribs = new CryptokiAttribute[]
            { 
                new CryptokiAttribute(CryptokiAttribute.CryptokiType.ValueLen, Utility.ConvertToBytes( KeySizeValue ))
            };

            KeyPairValue = CryptoKey.GenerateKeyPair(m_session, new Mechanism(MechanismType.EC_KEY_PAIR_GEN), attribs, null);
            OwnsKeyPair = true;
        }

        /// <summary>
        /// Generates a signature for the specified data.
        /// </summary>
        /// <param name="data">The message data to be signed.</param>
        /// <returns>A digital signature for the specified data.</returns>
        public byte[] SignData(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }

            return SignData(data, 0, data.Length);
        }

        /// <summary>
        /// Generates a signature for the specified data.
        /// </summary>
        /// <param name="data">The message data to be signed.</param>
        /// <param name="offset">The offset index in the message data to start signing.</param>
        /// <param name="count">The number of bytes from the offset to sign.</param>
        /// <returns>A digital signature for the specified data.</returns>
        public byte[] SignData(byte[] data, int offset, int count)
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }
            if (offset < 0 || offset > data.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (count < 0 || count > data.Length - offset)
            {
                throw new ArgumentOutOfRangeException();
            }

            byte[] retVal;

            m_signMech.Parameter = Utility.ConvertToBytes((int)m_hashAlgorithm);

            using (CryptokiSign sig = new CryptokiSign(m_session, m_signMech, KeyPair))
            {
                retVal = sig.Sign(data, offset, count);
            }

            return retVal;
        }

        /// <summary>
        /// Generates a signature for the specified data stream, reading to the end of the stream.
        /// </summary>
        /// <param name="inputStream">The data stream to be signed.</param>
        /// <returns>A digital signature for the specified data stream.</returns>
        public byte[] SignData(Stream inputStream)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException();
            }

            byte[] data = new byte[(int)inputStream.Length];

            int len = inputStream.Read(data, 0, data.Length);

            return SignData(data, 0, len);
        }

        /// <summary>
        /// Generates a signature for the specified hash value.
        /// </summary>
        /// <param name="hash">The hash value of the data to be signed.</param>
        /// <param name="hashAlgorithm">The hash algorithm used to create the hash value of the data.</param>
        /// <returns>A digital signature for the specified hash value.</returns>
        public byte[] SignHash(byte[] hash, MechanismType hashAlgorithm)
        {
            if (hash == null)
            {
                throw new ArgumentNullException();
            }

            byte[] sig = null;

            m_signMech.Parameter = Utility.ConvertToBytes((int)(hashAlgorithm | MechanismType.SIGN_NO_NODIGEST_FLAG));

            using (CryptokiSign sign = new CryptokiSign(m_session, m_signMech, KeyPair))
            {
                sig = sign.Sign(hash, 0, hash.Length);
            }

            return sig;
        }

        //
        // Signature verification
        //

        /// <summary>
        /// Verifies the digital signature of the specified data. 
        /// </summary>
        /// <param name="data">The data that was signed.</param>
        /// <param name="signature">The signature to be verified.</param>
        /// <returns>true if the signature is valid; otherwise, false.</returns>
        public bool VerifyData(byte[] data, byte[] signature)
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }

            return VerifyData(data, 0, data.Length, signature);
        }

        /// <summary>
        /// Verifies the digital signature of the specified data. 
        /// </summary>
        /// <param name="data">The data that was signed.</param>
        /// <param name="offset">The offset index in the message data to start verifying</param>
        /// <param name="count">The number of bytes from the offset to sign.</param>
        /// <param name="signature">The signature data to be verified.</param>
        /// <returns>true if the signature is valid; otherwise, false.</returns>
        public bool VerifyData(byte[] data, int offset, int count, byte[] signature)
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }
            if (offset < 0 || offset > data.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (count < 0 || count > data.Length - offset)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (signature == null)
            {
                throw new ArgumentNullException();
            }

            bool isValid = false;

            m_signMech.Parameter = Utility.ConvertToBytes((int)m_hashAlgorithm);

            using (CryptokiVerify ver = new CryptokiVerify(m_session, m_signMech, KeyPair))
            {
                isValid = ver.Verify(data, offset, count, signature, 0, signature.Length);
            }

            return isValid;
        }

        /// <summary>
        /// Verifies the digital signature of the specified data stream, reading to the end of the stream.
        /// </summary>
        /// <param name="inputStream">The data stream that was signed.</param>
        /// <param name="signature">The signature to be verified.</param>
        /// <returns>true if the signature is valid; otherwise, false.</returns>
        public bool VerifyData(Stream inputStream, byte[] signature)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException();
            }
            if (signature == null)
            {
                throw new ArgumentNullException();
            }

            byte[] data = new byte[(int)inputStream.Length];

            int len = inputStream.Read(data, 0, data.Length);

            return VerifyData(data, 0, len, signature);
        }

        /// <summary>
        /// Verifies the specified digital signature against a specified hash value.
        /// </summary>
        /// <param name="hash">The hash value of the data to be verified.</param>
        /// <param name="hashAlgorithm">The hash algorithm used to create the hash value of the data.</param>
        /// <param name="signature">The digital signature of the data to be verified against the hash value.</param>
        /// <returns>true if the signature is valid; otherwise, false.</returns>
        public bool VerifyHash(byte[] hash, MechanismType hashAlgorithm, byte[] signature)
        {
            if (hash == null)
            {
                throw new ArgumentNullException();
            }
            if (signature == null)
            {
                throw new ArgumentNullException();
            }

            bool isValid = false;

            m_signMech.Parameter = Utility.ConvertToBytes((int)(hashAlgorithm | MechanismType.SIGN_NO_NODIGEST_FLAG));

            using (CryptokiVerify ver = new CryptokiVerify(m_session, m_signMech, KeyPair))
            {
                isValid = ver.Verify(hash, 0, hash.Length, signature, 0, signature.Length);
            }

            return isValid;
        }
    }
}
