using System;
using System.Security.Cryptography;
using TestFW;
using TestFW.Logging;
using TestFW.Utility;

/// <summary>
///   This is a generic hashing test.  It takes a hash function, input, and expected output
/// </summary>
public class HashTestCase : TestCase
{
    private HashAlgorithm hasher;
    private byte[] input;
    private byte[] output;

    public HashTestCase(string name, HashAlgorithm hasher, byte[] input, byte[] output)
        : base(name)
    {
        SetImpl(new TestCaseImpl(RunTest));
        this.hasher = hasher;
        this.input = input;
        this.output = output;

        return;
    }

    /// <summary>
    ///   Perform the actual test
    /// </summary>
    /// <returns>true if running hasher on input produces output, false otherwise</returns>
    private bool RunTest()
    {
        byte[] hash1 = hasher.ComputeHash(input);
        byte[] hash2 = hasher.ComputeHash(input);

        // make sure that the hash is the same as the input, and is also consistent
        // (ie the same value is generated every time it is computed)
        return Util.CompareBytes(hash1, output) && Util.CompareBytes(hash1, hash2);
    }
}

/// <summary>
///   This is a generic hashing test.  It takes a hash function, input, and expected output, but allows for output to be truncated
/// </summary>
public class HashTestCaseTruncate : TestCase
{
    private HashAlgorithm hasher;
    private byte[] input;
    private byte[] output;
	private int truncateBits;

    public HashTestCaseTruncate(string name, HashAlgorithm hasher, byte[] input, byte[] output, int truncateBits)
        : base(name)
    {
        SetImpl(new TestCaseImpl(RunTest));
        this.hasher = hasher;
        this.input = input;
        this.output = output;
		this.truncateBits = truncateBits;

        return;
    }

    /// <summary>
    ///   Perform the actual test
    /// </summary>
    /// <returns>true if running hasher on input produces output, false otherwise</returns>
    private bool RunTest()
    {
        byte[] hash1 = hasher.ComputeHash(input);
        byte[] hash2 = hasher.ComputeHash(input);

		int truncateBytes = truncateBits / 8;
		byte[] hash1Truncated = new byte[truncateBytes];
		Array.Copy(hash1, hash1Truncated, truncateBytes);

        // make sure that the hash is the same as the input, and is also consistent
        // (ie the same value is generated every time it is computed)
        return Util.CompareBytes(hash1Truncated, output) && Util.CompareBytes(hash1, hash2);
    }
}
