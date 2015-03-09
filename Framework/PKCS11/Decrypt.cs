using System.Runtime.CompilerServices;
using System;
using System.Security.Cryptography;

namespace Microsoft.SPOT.Cryptoki
{
    /// <summary>
    /// The Cryptoki wrapper class for decryption operations.
    /// </summary>
    public class Decryptor : SessionContainer, ICryptoTransform
    {
        private int m_inputBlockSize;
        private int m_outputBlockSize;
        private bool m_isInit;
        private Mechanism m_mech;
        private CryptoKey m_key;

        /// <summary>
        /// Creates the decryptor object with the specified session context, decryption algorithm, key, and input/output block sizes
        /// </summary>
        /// <param name="session">The Cryptoki session context.</param>
        /// <param name="mechanism">The decryption algorithm and paramters.</param>
        /// <param name="key">The key that will be used to perform the decryption.</param>
        /// <param name="inputBlockSize">The input block size, in bits.</param>
        /// <param name="outputBlockSize">The output block size, in bits.</param>
        public Decryptor(Session session, Mechanism mechanism, CryptoKey key, int inputBlockSize, int outputBlockSize) :
            base(session, false)
        {
            m_inputBlockSize  = (inputBlockSize + 7) / 8;
            m_outputBlockSize = (outputBlockSize+ 7) / 8;

            m_mech = mechanism;
            m_key  = key;
        }

#region ICryptoTransform Members

        /// <summary>
        /// Gets the input block size, in bytes.
        /// </summary>
        public int InputBlockSize
        {
            get
            {
                return m_inputBlockSize;
            }
        }

        /// <summary>
        /// Gets the output block size, in bytes.
        /// </summary>
        public int OutputBlockSize
        {
            get
            {
                return m_outputBlockSize;
            }
        }

        /// <summary>
        /// Gets a value indicating whether multiple blocks can be transformed.
        /// </summary>
        public bool CanTransformMultipleBlocks
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the current transform can be reused.
        /// </summary>
        public bool CanReuseTransform
        {
            get { return true; }
        }

        /// <summary>
        /// Transforms the specified region of the input byte array and copies the resulting transform to the specified region of the output byte array.
        /// </summary>
        /// <param name="inputBuffer">The input for which to compute the transform.</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
        /// <param name="outputBuffer">The output to which to write the transform.</param>
        /// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
        /// <returns>The number of bytes written.</returns>
        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if(inputBuffer.Length == 0) return 0;

            if (!m_isInit) DecryptInit(m_session, m_mech, m_key);

            m_isInit = true;

            return TransformBlockInternal(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }

        /// <summary>
        /// Transforms the specified region of the specified byte array.
        /// </summary>
        /// <param name="inputBuffer">The input for which to compute the transform.</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
        /// <returns>The computed transform.</returns>
        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            if(inputBuffer.Length == 0) return new byte[0];

            if (!m_isInit) DecryptInit(m_session, m_mech, m_key);

            m_isInit = false;

            return TransformFinalBlockInternal(inputBuffer, inputOffset, inputCount);
        }

#endregion //ICryptoTransform

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void DecryptInit(Session session, Mechanism mechanism, CryptoKey key);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern int TransformBlockInternal(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern byte[] TransformFinalBlockInternal(byte[] inputBuffer, int inputOffset, int inputCount);
    }
}
