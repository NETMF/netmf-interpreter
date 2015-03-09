////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Runtime.CompilerServices;

namespace System
{
    /// <summary>
    /// Represents a pseudo-random number generator, a device that produces a
    /// sequence of numbers that meet certain statistical requirements for
    /// randomness.
    /// </summary>
    public class Random
    {
        private object _random;

        /// <summary>
        /// Initializes a new instance of the Random class, using a time-
        /// dependent default seed value.
        /// </summary>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern Random();

        /// <summary>
        /// Initializes a new instance of the Random class, using the specified
        /// seed value.
        /// </summary>
        /// <param name="seed">A number used to calculate a starting value for
        /// the pseudo-random number sequence.</param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern Random(int seed);

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>A 32-bit signed integer greater than or equal to zero and
        /// less than MaxValue.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual int Next();

        /// <summary>
        /// Returns a nonnegative random number less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number
        /// to be generated. maxValue must be greater than or equal to zero.</param>
        /// <returns>A 32-bit signed integer greater than or equal to zero, and
        /// less than maxValue.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual int Next(int maxValue);

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>A double-precision floating point number greater than or equal
        /// to 0.0, and less than 1.0.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual double NextDouble();

        /// <summary>
        /// Fills the elements of a specified array of bytes with random numbers.
        /// </summary>
        /// <param name="buffer">An array of bytes to contain random numbers.</param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual void NextBytes(byte[] buffer);
    }
}


