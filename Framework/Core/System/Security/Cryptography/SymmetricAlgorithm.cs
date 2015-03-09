//
// SymmetricAlgorithm.cs
//

namespace System.Security.Cryptography 
{
    using Microsoft.SPOT.Cryptoki;
    using System.Collections;

    /// <summary>
    /// Represents the abstract base class from which all implementations of symmetric algorithms must inherit.
    /// </summary>
    public abstract class SymmetricAlgorithm : SessionContainer 
    {
        protected int         BlockSizeValue;
        protected int         FeedbackSizeValue;
        protected byte[]      IVValue;
        protected CryptoKey KeyValue;
        protected bool        OwnsKey;
        protected KeySizes[]  LegalBlockSizesValue;
        protected KeySizes[]  LegalKeySizesValue;
        protected int         KeySizeValue;
        protected CipherMode  ModeValue;
        protected PaddingMode PaddingValue;

        /// <summary>
        /// Initializes a new instance of the SymmetricAlgorithm class.
        /// </summary>
        /// <param name="session">The cryptoki session context for which the symmectric algorithm will execute.</param>
        /// <param name="ownsSession">true if the session should be closed by this base class, false otherwise.</param>
        protected SymmetricAlgorithm(Session session, bool ownsSession)
            : base(session, ownsSession)
        {
            // Default to cipher block chaining (CipherMode.CBC) and
            // PKCS-style padding (pad n bytes with value n)
            ModeValue = CipherMode.CBC;
            PaddingValue  = PaddingMode.PKCS7;
        }

        /// <summary>
        /// Gets the symmetric algorithm type
        /// </summary>
        /// <returns></returns>
        abstract protected MechanismType MechanismType
        {
            get;
        }

        /// <summary>
        /// Releases all resources used by the SymmetricAlgorithm class.
        /// </summary>
        public void Clear() 
        {
            ((IDisposable) this).Dispose();
        }

        protected override void Dispose(bool disposing) 
        {
            try
            {
                if (disposing)
                {
                    // Note: we always want to zeroize the sensitive key material
                    if (KeyValue != null)
                    {
                        if (OwnsKey)
                        {
                            //Array.Clear(KeyValue, 0, KeyValue.Length);
                            KeyValue.Dispose();
                        }
                        KeyValue = null;
                    }
                    if (IVValue != null)
                    {
                        Array.Clear(IVValue, 0, IVValue.Length);
                        IVValue = null;
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
        /// Gets or sets the block size, in bits, of the cryptographic operation.
        /// </summary>
        public virtual int BlockSize 
        {
            get { return BlockSizeValue; }
            set 
            {
                int i;
                int j;

                for (i = 0; i < LegalBlockSizesValue.Length; i++)
                {
                    // If a cipher has only one valid key size, MinSize == MaxSize and SkipSize will be 0
                    if (LegalBlockSizesValue[i].SkipSize == 0)
                    {
                        if (LegalBlockSizesValue[i].MinSize == value)
                        { // assume MinSize = MaxSize
                            BlockSizeValue = value;
                            IVValue = null;
                            return;
                        }
                    }
                    else
                    {
                        for (j = LegalBlockSizesValue[i].MinSize; j <= LegalBlockSizesValue[i].MaxSize;
                            j += LegalBlockSizesValue[i].SkipSize)
                        {
                            if (j == value)
                            {
                                if (BlockSizeValue != value)
                                {
                                    BlockSizeValue = value;
                                    IVValue = null;      // Wrong length now
                                }
                                return;
                            }
                        }
                    }
                }
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Gets or sets the feedback size, in bits, of the cryptographic operation.
        /// </summary>
        public virtual int FeedbackSize 
        {
            get { return FeedbackSizeValue; }
            set 
            {
                if (value <= 0 || value > BlockSizeValue || (value % 8) != 0)
                    throw new InvalidOperationException();

               FeedbackSizeValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the initialization vector (IV) for the symmetric algorithm.
        /// </summary>
        public virtual byte[] IV 
        {
            get 
            { 
                if (IVValue == null) GenerateIV();
                return (byte[]) IVValue.Clone();
            }
            set 
            {
                if (value == null) throw new ArgumentNullException();
                if (value.Length * 8 != BlockSizeValue)
                    throw new InvalidOperationException();

                IVValue = (byte[]) value.Clone();
            }
        }

        /// <summary>
        /// Gets or sets the secret key for the symmetric algorithm.
        /// </summary>
        public virtual CryptoKey Key 
        {
            get 
            {
                if (KeyValue == null)
                {
                    GenerateKey();
                    OwnsKey = true;
                }

                return KeyValue;
            }
            set 
            {
                if (value == null)
                {
                    GenerateKey();
                    return;
                }

                if (!ValidKeySize(value.Size))
                    throw new CryptographicException();

                if (KeyValue != null)
                {
                    KeyValue.Dispose();
                }

                if (Session != value.Session)
                {
                    byte[] data = value.ExportKey(true);

                    KeyValue = CryptoKey.ImportKey(Session, data, CryptoKey.KeyClass.Secret, CryptoKey.KeyType.GENERIC_SECRET, true);

                    KeySizeValue = value.Size;
                    OwnsKey = true;
                }
                else
                {
                    // must convert bytes to bits
                    KeyValue = value;
                    OwnsKey = false;
                    KeySizeValue = value.Size;
                }
            }
        }

        /// <summary>
        /// Gets the block sizes, in bits, that are supported by the symmetric algorithm.
        /// </summary>
        public virtual KeySizes[] LegalBlockSizes 
        {
            get { return (KeySizes[]) LegalBlockSizesValue.Clone(); }
        }
    
        /// <summary>
        /// Gets the key sizes, in bits, that are supported by the symmetric algorithm.
        /// </summary>
        public virtual KeySizes[] LegalKeySizes 
        {
            get { return (KeySizes[]) LegalKeySizesValue.Clone(); }
        }
    
        /// <summary>
        /// Gets or sets the size, in bits, of the secret key used by the symmetric algorithm.
        /// </summary>
        public virtual int KeySize 
        {
            get { return KeySizeValue; }
            set 
            {
                if (!ValidKeySize(value))
                    throw new CryptographicException();

                KeySizeValue = value;
                GenerateKey();
            }
        }

        /// <summary>
        /// Gets or sets the mode for operation of the symmetric algorithm.
        /// </summary>
        public virtual CipherMode Mode
        {
            get { return ModeValue; }
            set
            {
                if ((value < CipherMode.CBC) || (CipherMode.CFB < value))
                    throw new ArgumentException();

                ModeValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the padding mode used in the symmetric algorithm.
        /// </summary>
        public virtual PaddingMode Padding
        {
            get { return PaddingValue; }
            set
            {
                if ((value < PaddingMode.None) || (PaddingMode.ISO10126 < value))
                    throw new ArgumentException();

                PaddingValue = value;
            }
        }

        //
        // public methods
        //

        /// <summary>
        /// Determines whether the specified key size is valid for the current algorithm.
        /// </summary>
        /// <param name="bitLength">The length, in bits, to check for a valid key size.</param>
        /// <returns>true if the specified key size is valid for the current algorithm; otherwise, false.</returns>
        public bool ValidKeySize(int bitLength)
        {
            KeySizes[] validSizes = this.LegalKeySizes;
            int i, j;

            if (validSizes == null) return false;
            for (i = 0; i < validSizes.Length; i++)
            {
                if (validSizes[i].SkipSize == 0)
                {
                    if (validSizes[i].MinSize == bitLength)
                    { // assume MinSize = MaxSize
                        return true;
                    }
                }
                else
                {
                    for (j = validSizes[i].MinSize; j <= validSizes[i].MaxSize; j += validSizes[i].SkipSize)
                    {
                        if (j == bitLength)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    
        /// <summary>
        /// Creates a symmetric encryptor object with the current Key property and initialization vector (IV).
        /// </summary>
        /// <returns>A symmetric encryptor object.</returns>
        public virtual ICryptoTransform CreateEncryptor() 
        {
            return CreateEncryptor(Key, IV);
        }

        /// <summary>
        /// When overridden in a derived class, creates a symmetric encryptor object with the specified Key property and initialization vector (IV).
        /// </summary>
        /// <param name="hKey">The secret key to use for the symmetric algorithm.</param>
        /// <param name="rgbIV">The initialization vector to use for the symmetric algorithm.</param>
        /// <returns>A symmetric encryptor object.</returns>
        public abstract ICryptoTransform CreateEncryptor(CryptoKey hKey, byte[] rgbIV);

        /// <summary>
        /// Creates a symmetric decryptor object with the current Key property and initialization vector (IV).
        /// </summary>
        /// <returns>A symmetric decryptor object.</returns>
        public virtual ICryptoTransform CreateDecryptor() 
        {
            return CreateDecryptor(Key, IV);
        }

        /// <summary>
        /// When overridden in a derived class, creates a symmetric decryptor object with the specified Key property and initialization vector (IV).
        /// </summary>
        /// <param name="hKey">The secret key to use for the symmetric algorithm.</param>
        /// <param name="rgbIV">The initialization vector to use for the symmetric algorithm.</param>
        /// <returns>A symmetric decryptor object.</returns>
        public abstract ICryptoTransform CreateDecryptor(CryptoKey hKey, byte[] rgbIV);
        
        /// <summary>
        /// When overridden in a derived class, generates a random key (Key) to use for the algorithm. 
        /// </summary>
        public abstract void GenerateKey();

        /// <summary>
        /// When overridden in a derived class, generates a random initialization vector (IV) to use for the algorithm.
        /// </summary>
        public abstract void GenerateIV();
    }
}
