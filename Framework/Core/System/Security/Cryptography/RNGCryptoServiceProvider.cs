namespace System.Security.Cryptography 
{
    using System.Runtime.InteropServices;
    using Microsoft.SPOT.Cryptoki;

    /// <summary>
    /// Implements a cryptographic Random Number Generator (RNG) using the specified cryptographic service provider
    /// </summary>
	public class RNGCryptoServiceProvider : SessionContainer
    {
        protected CryptokiRNG m_rng;

        /// <summary>
        /// Initializes a new instance of the RandomNumberGenerator class with the specified crypto service provider.
        /// </summary>
        /// <param name="serviceProvider">The crypto service provider to be used to generate the random data.</param>
        public RNGCryptoServiceProvider(string serviceProvider="")
            : base(new Session(serviceProvider), true)
        {
            m_rng = new CryptokiRNG(m_session);
        }
        
        /// <summary>
        /// Initializes a new instance of the RandomNumberGenerator class with the specified session context.
        /// </summary>
        /// <param name="session">The Cryptoki session context to be used to generate the random data.</param>
        public RNGCryptoServiceProvider(Session session) 
            : base(session, false)
        {
            m_rng = new CryptokiRNG(m_session);
        }
    
        //
        // public methods
        //

        /// <summary>
        /// Fills an array of bytes with a cryptographically strong sequence of random values. 
        /// </summary>
        /// <param name="data">The array to fill with a cryptographically strong sequence of random values.</param>
        public virtual void GetBytes(byte[] data)
        {
            if (data == null) throw new ArgumentNullException();

            m_rng.GenerateRandom(data, 0, data.Length, false);
        }

        /// <summary>
        /// Fills an array of bytes with a cryptographically strong sequence of random nonzero values.
        /// </summary>
        /// <param name="data">The array to fill with a cryptographically strong sequence of random nonzero values.</param>
        public virtual void GetNonZeroBytes(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException();

            m_rng.GenerateRandom(data, 0, data.Length, true);
        }
    }
}
