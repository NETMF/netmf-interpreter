namespace System.Security.Cryptography 
{
    /// <summary>
    /// Determines the set of valid key sizes for the symmetric cryptographic algorithms.
    /// </summary>
    public sealed class KeySizes 
    {
        private int m_minSize;
        private int m_maxSize;
        private int m_skipSize;

        /// <summary>
        /// Specifies the minimum key size in bits.
        /// </summary>
        public int MinSize 
        {
            get { return m_minSize; }
        }

        /// <summary>
        /// Specifies the maximum key size in bits.
        /// </summary>
        public int MaxSize 
        {
            get { return m_maxSize; }
        }

        /// <summary>
        /// Specifies the interval between valid key sizes in bits.
        /// </summary>
        public int SkipSize 
        {
            get { return m_skipSize; }
        }

        /// <summary>
        /// Initializes a new instance of the KeySizes class with the specified key values.
        /// </summary>
        /// <param name="minSize">The minimum valid key size.</param>
        /// <param name="maxSize">The maximum valid key size.</param>
        /// <param name="skipSize">The interval between valid key sizes.</param>
        public KeySizes(int minSize, int maxSize, int skipSize) 
        {
            m_minSize  = minSize; 
            m_maxSize  = maxSize; 
            m_skipSize = skipSize;
        }
    }
       
    /// <summary>
    /// The exception that is thrown when an unexpected operation occurs during a cryptographic operation.
    /// </summary>
    public class CryptographicUnexpectedOperationException : CryptographicException 
    {
        /// <summary>
        /// Initializes a new instance of the CryptographicUnexpectedOperationException class with default properties.
        /// </summary>
        public CryptographicUnexpectedOperationException() 
        {
        }
    
        /// <summary>
        /// Initializes a new instance of the CryptographicUnexpectedOperationException class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public CryptographicUnexpectedOperationException(String message) 
            : base(message) 
        {
        }
    }
}
