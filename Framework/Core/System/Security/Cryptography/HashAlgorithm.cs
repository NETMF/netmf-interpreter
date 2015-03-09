namespace System.Security.Cryptography 
{
    using System.IO;
    using Microsoft.SPOT.Cryptoki;

    /// <summary>
    /// Enumeration of the supported hash algorithms for the HashAlgorithm class.
    /// </summary>
    public enum HashAlgorithmType : uint
    {
        SHA1   = MechanismType.SHA_1,
        SHA256 = MechanismType.SHA256,
        SHA384 = MechanismType.SHA384,
        SHA512 = MechanismType.SHA512,

        RIPEMD160 = MechanismType.RIPEMD160,
        MD5       = MechanismType.MD5,
    }

    /// <summary>
    /// Represents the class from which all implementations of cryptographic hash algorithms must derive.
    /// </summary>
    public class HashAlgorithm : SessionContainer, ICryptoTransform
    {
        protected CryptokiDigest m_digest;
        protected Mechanism m_mechanism;
        protected byte[] HashValue;
        protected int State = 0;
        protected int m_hashSize;

        private bool m_bDisposed = false;

        /// <summary>
        /// Initializes a new instance of the HashAlgorithm class.
        /// </summary>
        /// <param name="serviceProvider">The name of the service provider which implements the hash algorithm</param>
        /// <param name="mechanism">The hash algorithm type</param>
        public HashAlgorithm(HashAlgorithmType hashAlgorithm, string serviceProvider = "")
            : base(new Session(serviceProvider, (MechanismType)hashAlgorithm), true)
        {
            m_mechanism = new Mechanism((MechanismType)hashAlgorithm);
            m_hashSize = -1;
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the HashAlgorithm class.
        /// </summary>
        /// <param name="session">The Cryptoki session context the hash algorithm will execute in.</param>
        /// <param name="mechanism">The hash algorithm type</param>
        public HashAlgorithm(HashAlgorithmType hashAlgorithm, Session session)
            : base(session, false)
        {
            m_mechanism = new Mechanism((MechanismType)hashAlgorithm);
            m_hashSize = -1;
            Initialize();
        }

        //
        // public properties
        //

        /// <summary>
        /// Gets the size, in bits, of the computed hash code.
        /// </summary>
        public virtual int HashSize
        {
            get 
            {
                if (m_hashSize == -1)
                {
                    switch (m_mechanism.Type)
                    {
                        case MechanismType.MD5:
                        case MechanismType.MD5_HMAC:
                            m_hashSize = 128;
                            break;

                        case MechanismType.SHA_1:
                        case MechanismType.SHA_1_HMAC:
                            m_hashSize = 160;
                            break;
                        case MechanismType.SHA256:
                        case MechanismType.SHA256_HMAC:
                            m_hashSize = 256;
                            break;
                        case MechanismType.SHA384:
                        case MechanismType.SHA384_HMAC:
                            m_hashSize = 384;
                            break;
                        case MechanismType.SHA512:
                        case MechanismType.SHA512_HMAC:
                            m_hashSize = 512;
                            break;

                        case MechanismType.RIPEMD160:
                        case MechanismType.RIPEMD160_HMAC:
                            m_hashSize = 160;
                            break;

                        default:
                            throw new NotSupportedException();
                    }
                }

                return m_hashSize; 
            }
        }

        /// <summary>
        /// Gets the value of the computed hash code.
        /// </summary>
        public virtual byte[] Hash 
        {
            get 
            {
                if (m_bDisposed) 
                    throw new ObjectDisposedException( );    // Environment.GetResourceString("ObjectDisposed_Generic"));
                if (State != 0)
                    throw new CryptographicException();
                return (byte[]) HashValue.Clone();
            }
        }

        //
        // public methods
        //

        /// <summary>
        /// Computes the hash value for the specified Stream object.
        /// </summary>
        /// <param name="inputStream">The input to compute the hash code for.</param>
        /// <returns>The computed hash code.</returns>
        public byte[] ComputeHash(Stream inputStream) 
        {
            if (m_bDisposed) 
                throw new ObjectDisposedException( ); 

            int bufSize = HashSize;
            byte[] buffer = new byte[bufSize];
            int bytesRead;
            do 
            {
                bytesRead = inputStream.Read(buffer, 0, bufSize);
                if (bytesRead > 0) 
                {
                    m_digest.DigestUpdate(buffer, 0, bytesRead);
                }
            } while (bytesRead > 0);

            HashValue = m_digest.DigestFinal();
            byte[] Tmp = (byte[]) HashValue.Clone();
            State = 0;
            return(Tmp);
        }

        /// <summary>
        /// Computes the hash value for the specified byte array.
        /// </summary>
        /// <param name="buffer">The input to compute the hash code for.</param>
        /// <returns>The computed hash code.</returns>
        public byte[] ComputeHash(byte[] buffer) 
        {
            if (m_bDisposed) 
                throw new ObjectDisposedException();    // Environment.GetResourceString("ObjectDisposed_Generic"));

            // Do some validation
            if (buffer == null) throw new ArgumentNullException();

            HashValue = m_digest.Digest(buffer, 0, buffer.Length);
            byte[] Tmp = (byte[]) HashValue.Clone();
            State = 0;
            return(Tmp);
        }

        /// <summary>
        /// Computes the hash value for the specified region of the specified byte array.
        /// </summary>
        /// <param name="buffer">The input to compute the hash code for.</param>
        /// <param name="offset">The offset into the byte array from which to begin using data.</param>
        /// <param name="count">The number of bytes in the array to use as data.</param>
        /// <returns>The computed hash code.</returns>
        public byte[] ComputeHash(byte[] buffer, int offset, int count) 
        {
            if (m_bDisposed) 
                throw new ObjectDisposedException();    // Environment.GetResourceString("ObjectDisposed_Generic"));

            // Do some validation
            if (buffer == null) throw new ArgumentNullException();
            if (offset < 0 || count < 0 || (count + offset > buffer.Length)) throw new ArgumentOutOfRangeException();    // "offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));

            HashValue = m_digest.Digest(buffer, offset, count);
            byte[] Tmp = (byte[]) HashValue.Clone();
            State = 0;
            return(Tmp);
        }

        // ICryptoTransform methods

        /// <summary>
        /// When overridden in a derived class, gets the input block size.
        /// </summary>
        public virtual int InputBlockSize 
        {
            // we assume any HashAlgorithm can take input a byte at a time
            get { return (1); }
        }

        /// <summary>
        /// When overridden in a derived class, gets the output block size.
        /// </summary>
        public virtual int OutputBlockSize 
        {
            get { return(1); }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether multiple blocks can be transformed.
        /// </summary>
        public virtual bool CanTransformMultipleBlocks 
        { 
            get { return(true); }
        }

        /// <summary>
        /// Gets a value indicating whether the current transform can be reused.
        /// </summary>
        public virtual bool CanReuseTransform 
        { 
            get { return(true); }
        }

        /// <summary>
        /// Computes the hash value for the specified region of the input byte array and copies the specified region of the input byte array to the specified region of the output byte array.
        /// </summary>
        /// <param name="inputBuffer">The input to compute the hash code for.</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
        /// <param name="outputBuffer">A copy of the part of the input array used to compute the hash code.</param>
        /// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
        /// <returns>The number of bytes written.</returns>
        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) 
        {
            if (m_bDisposed) 
                throw new ObjectDisposedException( null );    // Environment.GetResourceString("ObjectDisposed_Generic"));

            // Do some validation, we let BlockCopy do the destination array validation
            if (inputBuffer == null) throw new ArgumentNullException();
            if (inputOffset < 0 || inputCount < 0 || (inputCount + inputOffset > inputBuffer.Length)) throw new ArgumentOutOfRangeException();    // "inputOffset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));

            // Change the State value
            State = 1;
            m_digest.DigestUpdate(inputBuffer, inputOffset, inputCount);
            if ((inputBuffer != outputBuffer) || (inputOffset != outputOffset))
                Array.Copy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
            return inputCount;
        }

        /// <summary>
        /// Computes the hash value for the specified region of the specified byte array.
        /// </summary>
        /// <param name="inputBuffer">The input to compute the hash code for.</param>
        /// <param name="inputOffset">The offset into the byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the byte array to use as data.</param>
        /// <returns>An array that is a copy of the part of the input that is hashed.</returns>
        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) 
        {
            if (m_bDisposed) 
                throw new ObjectDisposedException( null );    // Environment.GetResourceString("ObjectDisposed_Generic"));

            // Do some validation
            if (inputBuffer == null) throw new ArgumentNullException();
            if (inputOffset < 0 || inputCount < 0 || (inputCount + inputOffset > inputBuffer.Length)) throw new ArgumentOutOfRangeException();

            if (inputCount > 0)
            {
                m_digest.DigestUpdate(inputBuffer, inputOffset, inputCount);
            }
            
            HashValue = m_digest.DigestFinal();
            
            // reset the State value
            State = 0;

            return HashValue;
        }

        /// <summary>
        /// Gets the hash algorithm type
        /// </summary>
        public MechanismType HashType
        {
            get
            {
                return m_mechanism.Type;
            }
        }

        // IDisposable methods

        /// <summary>
        /// Releases all resources used by the HashAlgorithm class.
        /// </summary>
        public void Clear() 
        {
            ((IDisposable) this).Dispose();
        }

        /// <summary>
        /// Releases the unmanaged resources used by the HashAlgorithm and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing) 
        {
            if (disposing) 
            {
                if (HashValue != null)
                {
                    Array.Clear(HashValue, 0, HashValue.Length);
                    HashValue = null;
                }
                if (m_digest != null)
                {
                    m_digest.Dispose();
                } 
                
                m_bDisposed = true;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Initializes an implementation of the HashAlgorithm class.
        /// </summary>
        public void Initialize()
        {
            if (HashValue != null)
            {
                Array.Clear(HashValue, 0, HashValue.Length);
                HashValue = null;
            }
            if (State != 0)
            {
                m_digest.DigestFinal();
                State = 0;
            }

            if (m_hashSize == -1)
            {
                m_hashSize = HashSize;
            }

            m_digest = new CryptokiDigest(m_session, m_mechanism, m_hashSize);
        }
    }
}
