namespace Microsoft.SPOT.Cryptoki
{
    using System;
    using System.Security.Cryptography;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Defines the cryptoki Random Number Generator (RNG) object.
    /// </summary>
    public class CryptokiRNG : SessionContainer
    {
        /// <summary>
        /// Creates a random number generator object from a specified Cryptoki session.
        /// </summary>
        /// <param name="session">The Cryptoki session context.</param>
        public CryptokiRNG(Session session)
            : base(session, false)
        {
        }

        /// <summary>
        /// Creates a random number generator object from the specified provider name and algorithm.
        /// </summary>
        /// <param name="providerName">The crypto service provider.</param>
        /// <param name="algorithmName">The RNG algorithm.</param>
        public CryptokiRNG(string providerName, string algorithmName)
            : base(new Session(providerName), true)
        {
        }

        /// <summary>
        /// Generates a set of random bytes.
        /// </summary>
        /// <param name="data">The output array were the random values will be stored.</param>
        /// <param name="index">The index for which the random value will be placed.</param>
        /// <param name="length">The length of the generated random data, in bytes</param>
        /// <param name="onlyNonZero">true to generate only non-zero bytes, false otherwise.</param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern void GenerateRandom(byte[] data, int index, int length, bool onlyNonZero);

        /// <summary>
        /// Seeds the random number generator.
        /// </summary>
        /// <param name="seed">The seed value for the random number generator.</param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern void SeedRandom(byte[] seed);
    }
}