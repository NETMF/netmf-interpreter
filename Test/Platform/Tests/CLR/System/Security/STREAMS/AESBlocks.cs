using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

public static class BlockTest
{
    public const int PassCode = 100;
	public const int FailCode = 101;
	
	public static int Main()
    {
       List<byte[]>[] inputs = new List<byte[]>[]
        {
            GenerateInput(new int[] { 3, 1 }),
            GenerateInput(new int[] { 2, 2 }),
            GenerateInput(new int[] { 2, 1, 1 }),
            GenerateInput(new int[] { 1, 3}),
            GenerateInput(new int[] { 1, 2, 1}),
            GenerateInput(new int[] { 1, 1, 2 }),
            GenerateInput(new int[] { 1, 1, 1, 1 })
        };

			List<byte[]> results = new List<byte[]>();

			Aes aes1 = (Aes) new AesManaged();
			Aes aes2 = new AesManaged();

			aes1.Key = aes2.Key;
			aes1.IV = aes2.IV;

			for (int i = 0; i < inputs.Length; i++)
			{
				byte[] result = null;
				if (!TestRoundTrip(aes1, aes2, inputs[i], out result))
				{
					Console.WriteLine("Test Fail - roundtrip fails");
					return FailCode;
				}

				results.Add(result);
			}

			for(int i = 0; i < results.Count - 1; i++)
			{
				if (!CompareBytes(results[i], results[i + 1]))
				{
					Console.WriteLine("Test Fail - mismatch");
					Console.WriteLine("Result {0}: {1}", i, ByteArrayToString(results[i]));
					Console.WriteLine("Result {0}: {1}", i+1, ByteArrayToString(results[i+1]));
					Console.WriteLine("Key: {0}", ByteArrayToString(aes1.Key));
					Console.WriteLine("IV: {0}", ByteArrayToString(aes1.IV));
				   return FailCode;
				}
			}

			Console.WriteLine("Test Passed");

			return PassCode;
    }

    private static List<byte[]> GenerateInput(int[] sizes)
    {
        List<byte[]> input = new List<byte[]>();

        byte b = 0;
        foreach (int size in sizes) 
        {
            byte[] data = new byte[size * 16];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = b;
                b++;
            }

            input.Add(data);
        }

        if (b != 4 * 16)
            Console.WriteLine("Generated bad input");

        return input;
    }

    private static bool TestRoundTrip(SymmetricAlgorithm testAlgorithm, SymmetricAlgorithm baseline, List<byte[]> data, out byte[] result)
    {
        result = null;

        try
        {
            byte[] testCipherValue;
            byte[] baselineCipherValue;

            using (MemoryStream testEncrypted = new MemoryStream())
            using (MemoryStream baselineEncrypted = new MemoryStream())
            {
                using (CryptoStream testEncryptor = new CryptoStream(testEncrypted, testAlgorithm.CreateEncryptor(), CryptoStreamMode.Write))
                using (CryptoStream baselineEncryptor = new CryptoStream(baselineEncrypted, baseline.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    foreach (byte[] blocks in data)
                    {
                        testEncryptor.Write(blocks, 0, blocks.Length);
                        baselineEncryptor.Write(blocks, 0, blocks.Length);
                    }

                    testEncryptor.Close();
                    baselineEncryptor.Close();

                    testCipherValue = testEncrypted.ToArray();
                    baselineCipherValue = baselineEncrypted.ToArray();
                }
            }

            byte[] testRoundtrip;
            byte[] baselineRoundtrip;

            using (MemoryStream testDecrypted = new MemoryStream())
            using (MemoryStream baselineDecrypted = new MemoryStream())
            {
                using (CryptoStream testDecryptor = new CryptoStream(testDecrypted, testAlgorithm.CreateDecryptor(), CryptoStreamMode.Write))
                using (CryptoStream baselineDecryptor = new CryptoStream(baselineDecrypted, baseline.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    testDecryptor.Write(baselineCipherValue, 0, baselineCipherValue.Length);
                    testDecryptor.Close();

                    baselineDecryptor.Write(testCipherValue, 0, testCipherValue.Length);
                    baselineDecryptor.Close();

                    testRoundtrip = testDecrypted.ToArray();
                    baselineRoundtrip = baselineDecrypted.ToArray();
                }
            }

            if (!CompareBytes(testRoundtrip, baselineRoundtrip))
            {
                Console.WriteLine("Roundtrip bytes do not match");
                Console.WriteLine("Test data: {0}", ByteArrayToString(testRoundtrip));
				Console.WriteLine("Baseline data: {0}", ByteArrayToString(baselineRoundtrip));
				Console.WriteLine("Key: {0}", ByteArrayToString(testAlgorithm.Key));
				Console.WriteLine("IV: {0}", ByteArrayToString(testAlgorithm.IV));
				return false;
            }

            result = testRoundtrip;
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Got an exception, fail");
            Console.WriteLine(e);
            return false;
        }
    }

    private static bool CompareBytes(byte[] lhs, byte[] rhs)
    {
        if (lhs.Length != rhs.Length)
            return false;

        for (int i = 0; i < lhs.Length; i++)
        {
            if (lhs[i] != rhs[i])
                return false;
        }

        return true;
    }

	private static string ByteArrayToString(byte[] bytes)
	{
		string str = "";

		for (int i = 0; i < bytes.Length; i++)
			str = str + bytes[i].ToString("X2");

		return str;
	}
}

