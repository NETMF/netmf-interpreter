using System;
using System.Security;
using System.Security.Cryptography;
using TestFW;
using TestFW.Logging;
using TestFW.Utility;

/// <summary>
///     Tests for decrypting data in place
/// </summary>
public static class InPlace
{
    public const int PassCode = 100;
    public const int FailCode = 101;

    public static int Main()
    {
        TestGroup inPlace = new TestGroup("In place decryption", new TestCase[]
        {
            new TestCase("DES CSP", new TestCaseImpl(InPlaceDecrypt<DESCryptoServiceProvider>)),
            new TestCase("RC2 CSP", new TestCaseImpl(InPlaceDecrypt<RC2CryptoServiceProvider>)),
            new TestCase("AES", new TestCaseImpl(InPlaceDecrypt<RijndaelManaged>)),
            new TestCase("3DES CSP", new TestCaseImpl(InPlaceDecrypt<TripleDESCryptoServiceProvider>))
        });

        TestRunner runner = new TestRunner(new TestGroup[] { inPlace }, new ILogger[] { new ConsoleLogger(), new XmlFailureLogger() });
        return runner.Run() ? PassCode : FailCode;
    }

    private static bool InPlaceDecrypt<T>() where T : SymmetricAlgorithm, new()
    {
        // setup the encryption algorithm
        T algorithm = new T();
        algorithm.GenerateKey();
        algorithm.GenerateIV();
        algorithm.Padding = PaddingMode.None;
        algorithm.Mode = CipherMode.CBC;

        // create data to encrypt
        byte[] original = new byte[algorithm.BlockSize];
        byte[] originalCopy = new byte[algorithm.BlockSize];
        for(int i = 0; i < original.Length; i++)
        {
            original[i] = (byte)((i * 21) % Byte.MaxValue);
            originalCopy[i] = original[i];
        }

        // encrypt it to a second array, and then in place
        byte[] encrypted = new byte[algorithm.BlockSize];
        ICryptoTransform externalEncryptor = algorithm.CreateEncryptor();
        externalEncryptor.TransformBlock(original, 0, original.Length, encrypted, 0);
        ICryptoTransform inplaceEncryptor = algorithm.CreateEncryptor();
        inplaceEncryptor.TransformBlock(original, 0, original.Length, original, 0);

        if(!Util.CompareBytes(encrypted, original))
        {
            Console.Error.WriteLine("In place and external encryption differ");
            return false;
        }

        // now decrypt to a second array, and then in place
        byte[] roundTrip = new byte[algorithm.BlockSize];
        ICryptoTransform externalDecryptor = algorithm.CreateDecryptor();
        externalDecryptor.TransformBlock(encrypted, 0, encrypted.Length, roundTrip, 0);
        ICryptoTransform inplaceDecryptor = algorithm.CreateDecryptor();
        inplaceDecryptor.TransformBlock(original, 0, original.Length, original, 0);

        return  Util.CompareBytes(roundTrip, original) &&
                Util.CompareBytes(original, originalCopy);
    }
}
