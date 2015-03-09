using System;
using System.Security.Cryptography;

class MainClass
{
	public const int PassCode = 100;
	public const int FailCode = 101;
		
	public const int DefaultMaxLength = 1000;
	public const int DefaultIterations = 100;

	public const int BlockSizeBytes = 16; // Rijndael can have other values but this is required in standardized version
	public static int[] KeySizeBytes = new int[] { 16, 24, 32 };
	
	// Can't use padding mode None here as it puts too much restriction on block size.  Also can't use padding mode Zeros
	// here as the length of the decrypted text is not guaranteed (zeros not stripped)
	//
	public static PaddingMode[] PaddingModes = new PaddingMode[] { PaddingMode.PKCS7, PaddingMode.ANSIX923, PaddingMode.ISO10126};
	
	// Can't use CFB as AesManaged doesn't support it due to bug in underlying RijndaelManaged implementation.  Also can't use for
	// interop between RijndaelManaged and AESCryptoServiceProvider due to same bug (which cause padding error during decrypt).  The
	// only coverage we can get here is to round-trip AESCryptoServiceProvider with CFB.
	//
	public static CipherMode[] modes = new CipherMode[] { CipherMode.ECB, CipherMode.CBC };
	
	public static int Main(string[] args)
	{
		int maxLength = (args.Length >= 1) ? int.Parse(args[0]) : DefaultMaxLength;
		int iterations = (args.Length >= 2) ? int.Parse(args[1]) : DefaultIterations;

		if (!TestAlgorithmPair(new RijndaelManaged(), new AesManaged(), modes, maxLength, iterations))
		{
			Console.WriteLine("Test Failed");
			return FailCode;
		}

		if (AesCSPSupported() && !TestAlgorithmPair(new RijndaelManaged(), new AesCryptoServiceProvider(), modes, maxLength, iterations))
		{
			Console.WriteLine("Test Failed");
			return FailCode;
		}

		if (AesCSPSupported() && !TestAlgorithmPair(new AesManaged(), new AesCryptoServiceProvider(), modes, maxLength, iterations))
		{
			Console.WriteLine("Test Failed");
			return FailCode;
		}

		if (AesCSPSupported() && !TestAlgorithmPair(new AesCryptoServiceProvider(), new AesCryptoServiceProvider(), new CipherMode[] { CipherMode.CFB }, maxLength, iterations))
		{
			Console.WriteLine("Test Failed");
			return FailCode;
		}

		Console.WriteLine("Test Passed");
		return PassCode;
	}

	public static bool TestAlgorithmPair(SymmetricAlgorithm algorithm1, SymmetricAlgorithm algorithm2, CipherMode[] modes, int maxLength, int iterations)
	{
		return	TestAlgorithms(algorithm1, algorithm2, modes, maxLength, iterations) &&
				TestAlgorithms(algorithm2, algorithm1, modes, maxLength, iterations);
	}

	public static bool TestAlgorithms(SymmetricAlgorithm encAlgorithm, SymmetricAlgorithm decAlgorithm, CipherMode[] modes, int maxLength, int iterations)
	{
		Random rand = new Random();
		
		for (int i = 0; i < iterations; i++)
		{
			// Create random data, key, IV, mode
			//
			byte[] key = new byte[KeySizeBytes[rand.Next(KeySizeBytes.Length)]];
			rand.NextBytes(key);
			
			byte[] data = new byte[rand.Next(1, maxLength + 1)];
			rand.NextBytes(data);

			byte[] IV = new byte[BlockSizeBytes];
			rand.NextBytes(IV);
			
			CipherMode mode = modes[rand.Next(modes.Length)];
			PaddingMode padding = PaddingModes[new Random().Next(PaddingModes.Length)];

			// Encrypt the data
			//
			byte[] encryptedData;
			encAlgorithm.Key = key;
			encAlgorithm.IV = IV;
			encAlgorithm.Mode = mode;
			encAlgorithm.Padding = padding;

			ICryptoTransform transform = encAlgorithm.CreateEncryptor();
			encryptedData = transform.TransformFinalBlock(data, 0, data.Length);

			// Decrypt the data
			//
			byte[] decryptedData;
			decAlgorithm.Key = key;
			decAlgorithm.IV = IV;
			decAlgorithm.Mode = mode;
			decAlgorithm.Padding = padding;

			transform = decAlgorithm.CreateDecryptor();
			decryptedData = transform.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

			if (!CompareBytes(data, decryptedData))
			{
				Console.WriteLine("ERROR - roundtrip encrypt/decrypt failed!\n");
				Console.WriteLine("Encryption algorithm: {0}", encAlgorithm.ToString());
				Console.WriteLine("Decryption algorithm: {0}", decAlgorithm.ToString());
				Console.WriteLine("Original data: {0}", ByteArrayToString(data));
				Console.WriteLine("Roundtrip data: {0}", ByteArrayToString(decryptedData));
				Console.WriteLine("Key: {0}", ByteArrayToString(key));
				Console.WriteLine("IV: {0}", ByteArrayToString(IV));
				Console.WriteLine("Cipher mode: {0}", mode.ToString());
				Console.WriteLine("Padding mode: {0}", padding.ToString());
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

	public static bool CompareBytes(byte[] x, byte[] y)
	{
		if (x.Length != y.Length)
		{
			Console.WriteLine("Byte array length mismatch\n");
			return false;
		}

		for (int i = 0; i < x.Length; i++)
		{
			if (x[i] != y[i])
			{
				Console.WriteLine("Byte array content mismatch\n");
				return false;
			}
		}

		return true;
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
