namespace Microsoft.SPOT.Cryptoki
{
    using System;
    using System.Security.Cryptography;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// The Cryptoki wrapper class for digest (hash) operations.
    /// </summary>
    public class CryptokiDigest : SessionContainer
    {
        private int m_hashSize;
        private Mechanism m_mechanism;
        private bool m_isInit;

        /// <summary>
        /// Creates the Cryptoki digest object with specified crypto provider, digest algorithm, and hash size.
        /// </summary>
        /// <param name="providerName">The crypto provider which will perform the digest operations.</param>
        /// <param name="mechanism">The digest algorithm and paramters.</param>
        /// <param name="hashSize">The size of the resulting hash value, in bits.</param>
        public CryptokiDigest(string providerName, Mechanism mechanism, int hashSize)
            : base(new Session(providerName, mechanism.Type), true)
        {
            m_hashSize = (hashSize + 7) / 8;

            if (m_hashSize == 0) throw new ArgumentException();

            m_mechanism = mechanism;
        }

        /// <summary>
        /// Creates the Cryptoki digest object with specified session context, digest algorithm, and hash size.
        /// </summary>
        /// <param name="session">The Cryptoki session context.</param>
        /// <param name="mechanism">The digest algorithm and paramters.</param>
        /// <param name="hashSize">The size of the resulting hash value, in bits.</param>
        public CryptokiDigest(Session session, Mechanism mechanism, int hashSize)
            : base(session, false)
        {
            m_hashSize = (hashSize + 7) / 8;

            if (m_hashSize == 0) throw new ArgumentException();

            m_mechanism = mechanism;
        }

        /// <summary>
        /// Digests the value of a secret key.
        /// </summary>
        /// <param name="hKey"></param>
        public void DigestKey(CryptoKey hKey)
        {
            if (!m_isInit) Init(m_mechanism);

            m_isInit = true;

            DigestKeyInternal(hKey);
        }

        /// <summary>
        /// Performs a complete digest/hash of the specified data.
        /// </summary>
        /// <param name="input">The message data to be hashed.</param>
        /// <param name="index">The index of the data to begin the hash operation.</param>
        /// <param name="length">The size in bytes of the data to be hashed.</param>
        /// <returns>The hash value of the specified data.</returns>
        public byte[] Digest(byte[] input, int index, int length)
        {
            if (m_isInit) throw new InvalidOperationException();
                
            Init(m_mechanism);

            return DigestInternal(input, index, length);
        }

        /// <summary>
        /// Performs a partial digest/hash update of the specified data.
        /// </summary>
        /// <param name="input">The message data to be hashed.</param>
        /// <param name="index">The index of the data to begin the hash operation.</param>
        /// <param name="length">The size in bytes of the data to be hashed.</param>
        public void DigestUpdate(byte[] input, int index, int length)
        {
            if (!m_isInit) Init(m_mechanism);

            m_isInit = true;

            DigestUpdateInternal(input, index, length);
        }

        /// <summary>
        /// Completes the partial digest/hash update.
        /// </summary>
        /// <returns>The final hash value.</returns>
        public byte[] DigestFinal()
        {
            if (!m_isInit) throw new InvalidOperationException();

            m_isInit = false;

            return DigestFinalInternal();
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void Init(Mechanism mechanism);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void DigestKeyInternal(CryptoKey hKey);
        
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern byte[] DigestInternal(byte[] input, int index, int length);        
        
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void DigestUpdateInternal(byte[] input, int index, int length);
        
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern byte[] DigestFinalInternal();
    }
}
