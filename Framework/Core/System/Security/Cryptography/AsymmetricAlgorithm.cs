using Microsoft.SPOT.Cryptoki;

namespace System.Security.Cryptography 
{
    /// <summary>
    /// Represents the abstract base class from which all implementations of asymmetric algorithms must inherit.
    /// </summary>
    public abstract class AsymmetricAlgorithm : SessionContainer 
    {
        protected CryptoKey  KeyPairValue;
        protected bool       OwnsKeyPair;
        protected int        KeySizeValue;
        protected KeySizes[] LegalKeySizesValue;

        protected AsymmetricAlgorithm(Session session, bool ownsSession) :
            base(session, ownsSession)
        { 
        }

        /// <summary>
        /// Releases all resources used by the AsymmetricAlgorithm class.
        /// </summary>
        public void Clear() 
        {
            ((IDisposable) this).Dispose();
        }

        /// <summary>
        /// Gets or sets the size, in bits, of the key modulus used by the asymmetric algorithm.
        /// </summary>
        public virtual int KeySize
        {
            get { return KeySizeValue; }
            set
            {
                int i;
                int j;

                if (value < 0) throw new ArgumentException();

                for (i = 0; i < LegalKeySizesValue.Length; i++)
                {
                    if (LegalKeySizesValue[i].SkipSize == 0)
                    {
                        if (LegalKeySizesValue[i].MinSize == value)
                        { // assume MinSize = MaxSize
                            KeySizeValue = value;
                            return;
                        }
                    }
                    else
                    {
                        for (j = LegalKeySizesValue[i].MinSize; j <= LegalKeySizesValue[i].MaxSize;
                             j += LegalKeySizesValue[i].SkipSize)
                        {
                            if (j == value)
                            {
                                KeySizeValue = value;
                                return;
                            }
                        }
                    }
                }
                throw new CryptographicException();
            }
        }

        /// <summary>
        /// Gets the key sizes that are supported by the asymmetric algorithm.
        /// </summary>
        public virtual KeySizes[] LegalKeySizes
        {
            get { return (KeySizes[])LegalKeySizesValue.Clone(); }
        }

        /// <summary>
        /// Gets the CryptoKey object representing the key pair for the AsymmetricAlgorithm
        /// </summary>
        public virtual CryptoKey KeyPair
        {
            get
            {
                if (KeyPairValue == null)
                {
                    GenerateKeyPair();
                    OwnsKeyPair = true;
                }

                return KeyPairValue;
            }

            set
            {
                if (value == null)
                {
                    GenerateKeyPair();
                    OwnsKeyPair = true;
                }
                else
                {
                    if (Session != value.Session) throw new InvalidOperationException();

                    if (KeyPairValue != null)
                    {
                        KeyPairValue.Dispose();
                    }

                    KeyPairValue = value;
                    KeySizeValue = KeyPairValue.Size;
                    OwnsKeyPair = false;
                }
            }
        }

        /// <summary>
        /// Generates a CryptoKey object representing the key pair for the SymmetricAlgorithm.
        /// </summary>
        protected abstract void GenerateKeyPair();

        /// <summary>
        /// Releases the unmanaged resources used by the AsymmetricAlgorithm class and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (OwnsKeyPair && KeyPairValue != null)
                    {
                        KeyPairValue.Dispose();
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        
        // TODO: Do we need these?  Should they be MechanismType return values
        //public abstract String SignatureAlgorithm 
        //{
        //    get;
        //}

        //public abstract String KeyExchangeAlgorithm 
        //{
        //    get;
        //}
    }
}    
