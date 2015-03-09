
namespace System.Security.Cryptography 
{
    using System;
    using System.IO;

    /// <summary>
    /// Defines the basic operations of cryptographic transformations.
    /// </summary>
    public interface ICryptoTransform : IDisposable 
    {
        /// <summary>
        /// Gets the input block size, in bytes.
        /// </summary>
        int InputBlockSize { get; }

        /// <summary>
        /// Gets the output block size, in bytes.
        /// </summary>
        int OutputBlockSize { get; }

        /// <summary>
        /// Gets a value indicating whether multiple blocks can be transformed.
        /// </summary>
        bool CanTransformMultipleBlocks { get; }

        /// <summary>
        /// Gets a value indicating whether the current transform can be reused.
        /// </summary>
        bool CanReuseTransform { get; }

        /// <summary>
        /// Transforms the specified region of the input byte array and copies the resulting transform to the specified region of the output byte array.
        /// </summary>
        /// <param name="inputBuffer">The input for which to compute the transform.</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
        /// <param name="outputBuffer">The output to which to write the transform.</param>
        /// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
        /// <returns>The number of bytes written.</returns>
        int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset);

        /// <summary>
        /// Transforms the specified region of the specified byte array.
        /// </summary>
        /// <param name="inputBuffer">The input for which to compute the transform.</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
        /// <returns>The computed transform.</returns>
        byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount);
    }
}
