using System;
using System.Security.Cryptography;

class TestClass
{
	public const int PassCode = 100;
	public const int FailCode = 101;
	
	public static Aes[] AesAlgorithms;
	public const int BlockSizeBytes = 16;
	
	public static int Main()
	{
        if (!AesCSPSupported())
        {
            Console.WriteLine("AesCryptoServiceProvieder only supported on WinXP and higher, only testing AesManaged");
			AesAlgorithms = new Aes[] { new AesManaged() };
        }
		else
		{
			AesAlgorithms = new Aes[] { new AesManaged(), new AesCryptoServiceProvider() };
		}

		foreach (Aes aes in AesAlgorithms)
		{
			aes.BlockSize = BlockSizeBytes * 8;
			
			if (!PaddingTest(aes, PaddingMode.Zeros, -2, new byte[] {0x00, 0x00}))
				return FailCode;

			if (!PaddingTest(aes, PaddingMode.Zeros, -1, new byte[] {0x00}))
				return FailCode;

			if (!PaddingTest(aes, PaddingMode.Zeros, 0, new byte[] {}))
				return FailCode;

			if (!PaddingTest(aes, PaddingMode.Zeros, 1, new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}))
				return FailCode;

			if (!PaddingTest(aes, PaddingMode.Zeros, 2, new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}))
				return FailCode;

			if (!PaddingTest(aes, PaddingMode.PKCS7, -2, new byte[] {0x02, 0x02}))
				return FailCode;

			if (!PaddingTest(aes, PaddingMode.PKCS7, -1, new byte[] {0x01}))
				return FailCode;

			if (!PaddingTest(aes, PaddingMode.PKCS7, 0, new byte[] {0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10}))
				return FailCode;

			if (!PaddingTest(aes, PaddingMode.PKCS7, 1, new byte[] {0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f}))
				return FailCode;

			if (!PaddingTest(aes, PaddingMode.PKCS7, 2, new byte[] {0x0e, 0x0e, 0x0e, 0x0e, 0x0e, 0x0e, 0x0e, 0x0e, 0x0e, 0x0e, 0x0e, 0x0e, 0x0e, 0x0e}))
				return FailCode;

			if (!PaddingTest(aes, PaddingMode.ANSIX923, -2, new byte[] {0x00, 0x02}))
				return FailCode;

			if (!PaddingTest(aes, PaddingMode.ANSIX923, -1, new byte[] {0x01}))
				return FailCode;

			if (!PaddingTest(aes, PaddingMode.ANSIX923, 0, new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10}))
				return FailCode;

			if (!PaddingTest(aes, PaddingMode.ANSIX923, 1, new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0f}))
				return FailCode;

			if (!PaddingTest(aes, PaddingMode.ANSIX923, 2, new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0e}))
				return FailCode;
		
			// For ISO10126 mode, 0xff in the expected array represents a random digit
			//
			if (!PaddingTest(aes, PaddingMode.ISO10126, -2, new byte[] {0xff, 0x02}))
				return FailCode;

			if (!PaddingTest(aes, PaddingMode.ISO10126, -1, new byte[] {0x01}))
				return FailCode;

			if (!PaddingTest(aes, PaddingMode.ISO10126, 0, new byte[] {0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x10}))
				return FailCode;

			if (!PaddingTest(aes, PaddingMode.ISO10126, 1, new byte[] {0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x0f}))
				return FailCode;

			if (!PaddingTest(aes, PaddingMode.ISO10126, 2, new byte[] {0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x0e}))
				return FailCode;

		}

		Console.WriteLine("Test Passed");
		return PassCode;
	
	}

	public static bool PaddingTest(Aes aes, PaddingMode mode, int offset, byte[] expected)
	{
		if (!CompareBytes(GetPaddingBytes(aes, mode, offset), expected))
		{
			Console.WriteLine("Error - padding failure");
			Console.WriteLine("Algorithm: {0}", aes.ToString());
			Console.WriteLine("Padding Mode: {0}", mode.ToString());
			Console.WriteLine("Block Size: {0}", BlockSizeBytes + offset);
			return false;
		}

		return true;
	}
		
	public static byte[] GetPaddingBytes(Aes aes, PaddingMode mode, int offset)
	{
		byte[] originalData = new byte[BlockSizeBytes + offset];
		new Random().NextBytes(originalData);

		aes.Padding = mode;
		byte[] encryptedData = aes.CreateEncryptor().TransformFinalBlock(originalData, 0, originalData.Length);

		aes.Padding = PaddingMode.None;
		byte[] decryptedData = aes.CreateDecryptor().TransformFinalBlock(encryptedData, 0, encryptedData.Length);

		byte[] paddingBytes = new byte[decryptedData.Length - BlockSizeBytes - offset];
		Array.Copy(decryptedData, BlockSizeBytes + offset, paddingBytes, 0, decryptedData.Length - BlockSizeBytes - offset);
		
		return paddingBytes;
	}

	public static bool CompareBytes(byte[] actual, byte[] expected)
	{
		if (expected.Length != actual.Length)
		{
			Console.WriteLine("Byte array length mismatch\n");
			Console.WriteLine("Expected : {0}", expected.Length);
			Console.WriteLine("Actual   : {0}\n", actual.Length);
			return false;
		}

		for (int i = 0; i < expected.Length; i++)
		{
			// For ISO10126 mode, 0xff represents a random digit, so don't compare these...
			//
			if ((expected[i] != actual[i]) && (expected[i] != 0xff))
			{
				Console.WriteLine("Byte array mismatch\n");
				Console.WriteLine("Expected : {0}", ByteArrayToString(expected));
				Console.WriteLine("Actual   : {0}\n", ByteArrayToString(actual));
				return false;
			}
		}

		return true;
	}
	
	public static string ByteArrayToString(byte[] bytes)
	{
		string str = "";

		for (int i = 0; i < bytes.Length; i++)
			str = str + bytes[i].ToString("X2");

		return str;
	}

	// AesCryptoServiceProvider is only supported in WinXP and higher (v5.1+)
	//
	public const int AesCSPSupportedMajorVer = 5;
	public const int AesCSPSupportedMinorVer = 1;
	
	public static bool AesCSPSupported()
	{
		int major = Environment.OSVersion.Version.Major;
		int minor = Environment.OSVersion.Version.Minor;

		if (major > AesCSPSupportedMajorVer)
			return true;

		if ((major == AesCSPSupportedMajorVer) && (minor >= AesCSPSupportedMinorVer))
			return true;

		return false;
	}

}
