// Exercise the various properties and methods of the SHA{256,384,512}CSP and MD5 and SHA* Cng classes
//
using System;
using System.Security.Cryptography;

class MainClass
{
	public const int PassCode = 100;
	public const int FailCode = 101;
	
	public const int SHA2SupportedMajorVer = 5;
	public const int SHA2SupportedMinorVer = 2;

	public enum PlatformTestStatus
	{
		PassSupported,
		PassUnsupported,
		Fail
	}
	
	public static int Main()
	{
		PlatformTestStatus SHA2PlatformStatus = SHA2PlatformTest();

		if (SHA2PlatformStatus == PlatformTestStatus.Fail)
		{
			Console.WriteLine("SHA2CSP platform test failed -- algorithm failed on Win2k3 or later");
			return FailCode;
		}

		if (SHA2PlatformStatus == PlatformTestStatus.PassUnsupported)
			Console.WriteLine("SHA2 CSP algorithms not supported on WinXP and earlier, skipping these tests");

		PlatformTestStatus CngPlatformStatus = CngPlatformTest();

		if (CngPlatformStatus == PlatformTestStatus.Fail)
		{
			Console.WriteLine("Cng platform test failed -- algorithm failed on Vista or later");
			return FailCode;
		}

		if (CngPlatformStatus == PlatformTestStatus.PassUnsupported)
			Console.WriteLine("Cng algorithms not supported on Win2k3 and earlier, skipping these tests");
		
		if ((SHA2PlatformStatus == PlatformTestStatus.PassSupported) && !ExerciseHash(new SHA256CryptoServiceProvider(), 256))
		{
			Console.WriteLine("SHA256CSP: Failed");
			return FailCode;
		}

		if ((SHA2PlatformStatus == PlatformTestStatus.PassSupported) && !ExerciseHash(new SHA384CryptoServiceProvider(), 384))
		{
			Console.WriteLine("SHA384CSP: Failed");
			return FailCode;
		}

		if ((SHA2PlatformStatus == PlatformTestStatus.PassSupported) && !ExerciseHash(new SHA512CryptoServiceProvider(), 512))
		{
			Console.WriteLine("SHA512CSP: Failed");
			return FailCode;
		}

		if ((CngPlatformStatus == PlatformTestStatus.PassSupported) && !ExerciseHash(new MD5Cng(), 128))
		{
			Console.WriteLine("MD5Cng: Failed");
			return FailCode;
		}

		if ((CngPlatformStatus == PlatformTestStatus.PassSupported) && !ExerciseHash(new SHA1Cng(), 160))
		{
			Console.WriteLine("SHA1Cng: Failed");
			return FailCode;
		}

		if ((CngPlatformStatus == PlatformTestStatus.PassSupported) && !ExerciseHash(new SHA256Cng(), 256))
		{
			Console.WriteLine("SHA256Cng: Failed");
			return FailCode;
		}

		if ((CngPlatformStatus == PlatformTestStatus.PassSupported) && !ExerciseHash(new SHA384Cng(), 384))
		{
			Console.WriteLine("SHA384Cng: Failed");
			return FailCode;
		}

		if ((CngPlatformStatus == PlatformTestStatus.PassSupported) && !ExerciseHash(new SHA512Cng(), 512))
		{
			Console.WriteLine("SHA512Cng: Failed");
			return FailCode;
		}

		Console.WriteLine("Test passed");
		return PassCode;
	}

	public static bool ExerciseHash(HashAlgorithm hash, int size)
	{
		// Exercise the properties
		//
		if (hash.CanReuseTransform != true)
		{
			Console.WriteLine("CanReuseTransform != true");
			return false;
		}

		if (hash.CanTransformMultipleBlocks != true)
		{
			Console.WriteLine("CanTransformMultipleBlocks != true");
			return false;
		}

		if (hash.HashSize != size)
		{
			Console.WriteLine("HashSize, expected={0} actual={1}", size, hash.HashSize);
			return false;
		}

		if (hash.InputBlockSize != 1)
		{
			Console.WriteLine("InputBlockSize != 1");
			return false;
		}

		if (hash.OutputBlockSize != 1)
		{
			Console.WriteLine("OutputBlockSize != 1");
			return false;
		}

		// Exercise the Initialize method.  Test proper behavior both when it is and is not called.
		//
		byte[] bytesHalf1 = {0x00, 0x01, 0x02, 0x03};
		byte[] bytesHalf2 = {0xfc, 0xfd, 0xfe, 0xff};
		byte[] bytesFull  = {0x00, 0x01, 0x02, 0x03, 0xfc, 0xfd, 0xfe, 0xff};
		byte[] bytesExpected;
		byte[] bytesActual;

		// Initialize is called between partial hashes
		//
		hash.Initialize();
		bytesExpected = hash.ComputeHash(bytesHalf1);
		
		hash.Initialize();
		hash.TransformBlock(bytesHalf2, 0, bytesHalf2.Length, bytesHalf2, 0);
		hash.Initialize();
		hash.TransformFinalBlock(bytesHalf1, 0, bytesHalf1.Length);
		bytesActual = hash.Hash;

		if (!CompareBytes(bytesExpected, bytesActual))
		{
			Console.WriteLine("\nInitialize test failed");
			return false;
		}

		// Initialize is not called between partial hashes
		//
		hash.Initialize();
		bytesExpected = hash.ComputeHash(bytesFull);

		hash.Initialize();
		hash.TransformBlock(bytesHalf1, 0, bytesHalf1.Length, bytesHalf1, 0);
		hash.TransformFinalBlock(bytesHalf2, 0, bytesHalf2.Length);
		bytesActual = hash.Hash;
		
		if (!CompareBytes(bytesExpected, bytesActual))
		{
			Console.WriteLine("\nNo Initialize test failed");
			return false;
		}

		// Exercise the Clear method -- ensure object disposed
		//
		hash.Initialize();
		hash.Clear();

		try
		{
			hash.ComputeHash(bytesFull);
			Console.WriteLine("Clear test failed -- no exception thrown");
			return false;
		}
		catch (ObjectDisposedException)
		{
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

	public static bool CompareBytes(byte[] expected, byte[] actual)
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
			if (expected[i] != actual[i])
			{
				Console.WriteLine("Byte array mismatch\n");
				Console.WriteLine("Expected : {0}", ByteArrayToString(expected));
				Console.WriteLine("Actual   : {0}\n", ByteArrayToString(actual));
				return false;
			}
		}

		return true;
	}

	public static bool SHA2Supported()
	{
		int major = Environment.OSVersion.Version.Major;
		int minor = Environment.OSVersion.Version.Minor;

		if (major > SHA2SupportedMajorVer)
			return true;

		if ((major == SHA2SupportedMajorVer) && (minor >= SHA2SupportedMinorVer))
			return true;

		return false;
	}

	// Ensure the SHA2 CSP APIs are supported on the target platform.  Also ensure if the target platform is sufficient, the APIs do
	// indeed create.
	//
	public static PlatformTestStatus SHA2PlatformTest()
	{
		bool createFail = false;

		try
		{
			new SHA256CryptoServiceProvider();
			new SHA384CryptoServiceProvider();
			new SHA512CryptoServiceProvider();
		}
		catch (PlatformNotSupportedException)
		{
			createFail = true;
		}

		int major = Environment.OSVersion.Version.Major;
		int minor = Environment.OSVersion.Version.Minor;
		
		bool supportedPlatform = (major > SHA2SupportedMajorVer) || ((major == SHA2SupportedMajorVer) && (minor >= SHA2SupportedMinorVer));

		if (createFail)
			return (supportedPlatform) ? PlatformTestStatus.Fail : PlatformTestStatus.PassUnsupported;

		return PlatformTestStatus.PassSupported;
	}

	// Ensure the MD5/SHA* Cng APIs are supported on the target platform.  Also ensure if the target platform is sufficient, the APIs do
	// indeed create.
	//
	public static PlatformTestStatus CngPlatformTest()
	{
		bool createFail = false;

		try
		{
			new MD5Cng();
			new SHA1Cng();
			new SHA256Cng();
			new SHA384Cng();
			new SHA512Cng();
		}
		catch (PlatformNotSupportedException)
		{
			createFail = true;
		}

		bool supportedPlatform = (Environment.OSVersion.Version.Major >= 6);

		if (createFail)
			return (supportedPlatform) ? PlatformTestStatus.Fail : PlatformTestStatus.PassUnsupported;

		return PlatformTestStatus.PassSupported;
	}
}

