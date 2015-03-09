// Basic API tests for AESManaged
//
using System;
using System.Security.Cryptography;

public class MainClass
{
	public const int PassCode = 100;
	public const int FailCode = 101;
	
	// TestCreate needs to updated with this list as well since you can't call static methods dynamically.
	//
	public static Aes[] AesAlgorithms;
	
	public static int Main()
	{
		AesAlgorithms = new Aes[] { new AesManaged() };

		foreach (Aes aes in AesAlgorithms)
		{
			Console.WriteLine("Testing {0}", aes.ToString());
			
			if (!TestBlockSize(aes))
			{
				Console.WriteLine("TestBlockSize failed");
				return FailCode;
			}

			if (!TestKeySize(aes))
			{
				Console.WriteLine("TestKeySize failed");
				return FailCode;
			}

			if (!TestGenerate(aes))
			{
				Console.WriteLine("TestGenerate failed");
				return FailCode;
			}

			if (!TestValidKeySize(aes))
			{
				Console.WriteLine("TestValidKeySize failed");
				return FailCode;
			}

			if (!TestTransform(aes))
			{
				Console.WriteLine("TestTransform failed");
				return FailCode;
			}

			// This one should go last in the list because it may dispose any native resources
			//
			if (!TestClear(aes))
			{
				Console.WriteLine("TestClear failed");
				return FailCode;
			}
		}

		Console.WriteLine("Test Passed");
		return PassCode;
	}

	// AES only allows block size of 128.
	//
	public static bool TestBlockSize(Aes aes)
	{
		if (aes.BlockSize != 128)
		{
			Console.WriteLine("Error - Default BlockSize not 128");
			return false;
		}
		
		try
		{
			aes.BlockSize = 64;
			Console.WriteLine("Error - BlockSize set to 64 succeeded");
			return false;
		}
		catch (CryptographicException)
		{
		}

		aes.BlockSize = 128;

		if (aes.BlockSize != 128)
		{
			Console.WriteLine("Error - BlockSize not 128 after set 128");
			return false;
		}

		try
		{
			aes.BlockSize = 192;

			Console.WriteLine("Error - BlockSize set to 192 succeeded");
			return false;
		}
		catch (CryptographicException)
		{
		}

		if (aes.BlockSize != 128)
		{
			Console.WriteLine("Error - BlockSize not 128 after set 192");
			return false;
		}

		try
		{
			aes.BlockSize = 256;

			Console.WriteLine("Error - BlockSize set to 256 succeeded");
			return false;
		}
		catch (CryptographicException)
		{
		}

		if (aes.BlockSize != 128)
		{
			Console.WriteLine("Error - BlockSize not 128 after set 256");
			return false;
		}
		
		try
		{
			aes.BlockSize = 512;
			Console.WriteLine("Error - BlockSize set to 512 succeeded");
			return false;
		}
		catch (CryptographicException)
		{
		}

		KeySizes[] sizes = aes.LegalBlockSizes;

		if ((sizes.Length != 1) || (sizes[0].MinSize != 128) || (sizes[0].MaxSize != 128) || (sizes[0].SkipSize != 0))
		{
			Console.WriteLine("Error - Unexpected LegalBlockSizes");
			return false;
		}
		
		return true;
	}

	// AES only allows block sizes of 128, 192, 256.
	//
	public static bool TestKeySize(Aes aes)
	{
		if (aes.KeySize != 256)
		{
			Console.WriteLine("Error - Default KeySize not 256");
			return false;
		}
		
		try
		{
			aes.KeySize = 64;
			Console.WriteLine("Error - KeySize set to 64 succeeded");
			return false;
		}
		catch (CryptographicException)
		{
		}

		aes.KeySize = 128;

		if (aes.KeySize != 128)
		{
			Console.WriteLine("Error - KeySize not 128 after set 128");
			return false;
		}

		aes.KeySize = 192;

		if (aes.KeySize != 192)
		{
			Console.WriteLine("Error - KeySize not 192 after set 192");
			return false;
		}

		aes.KeySize = 256;

		if (aes.KeySize != 256)
		{
			Console.WriteLine("Error - KeySize not 256 after set 256");
			return false;
		}

		try
		{
			aes.KeySize = 512;
			Console.WriteLine("Error - KeySize set to 512 succeeded");
			return false;
		}
		catch (CryptographicException)
		{
		}

		KeySizes[] sizes = aes.LegalKeySizes;

		if ((sizes.Length != 1) || (sizes[0].MinSize != 128) || (sizes[0].MaxSize != 256) || (sizes[0].SkipSize != 64))
		{
			Console.WriteLine("Error - Unexpected LegalKeySizes");
			return false;
		}
		
		return true;
	}

	public static bool TestGenerate(Aes aes)
	{
		byte[] key = aes.Key;
		byte[] IV = aes.IV;

		aes.GenerateKey();
		aes.GenerateIV();

		if (aes.Key.Length != 32)
		{
			Console.WriteLine("Error - Aes.GenerateKey wrong length: {0}", aes.Key.Length);
			return false;
		}

		if (aes.IV.Length != 16)
		{
			Console.WriteLine("Error - Aes.GenerateIV wrong length: {0}", aes.IV.Length);
			return false;
		}

		if (CompareBytes(aes.Key, key))
		{
			Console.WriteLine("Error - Aes.GenerateKey did not change key");
			return false;
		}

		if (CompareBytes(aes.IV, IV))
		{
			Console.WriteLine("Error - Aes.GenerateIV did not change IV");
			return false;
		}

		return true;
	}

	public static bool CompareBytes(byte[] x, byte[] y)
	{
		if (x.Length != y.Length)
			return false;

		for (int i = 0; i < x.Length; i++)
			if (x[i] != y[i])
				return false;

		return true;
	}

	public static bool TestValidKeySize(Aes aes)
	{
		if (aes.ValidKeySize(64))
		{
			Console.WriteLine("Error - ValidKeySize(64) true");
			return false;
		}

		if (!aes.ValidKeySize(128))
		{
			Console.WriteLine("Error - ValidKeySize(128) false");
			return false;
		}

		if (!aes.ValidKeySize(192))
		{
			Console.WriteLine("Error - ValidKeySize(192) false");
			return false;
		}

		if (!aes.ValidKeySize(256))
		{
			Console.WriteLine("Error - ValidKeySize(256) false");
			return false;
		}

		if (aes.ValidKeySize(512))
		{
			Console.WriteLine("Error - ValidKeySize(512) true");
			return false;
		}

		if (aes.ValidKeySize(0))
		{
			Console.WriteLine("Error - ValidKeySize(0) true");
			return false;
		}

		if (aes.ValidKeySize(-128))
		{
			Console.WriteLine("Error - ValidKeySize(-128) true");
			return false;
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
	
	public static bool TestClear(Aes aes)
	{
		byte[] key = aes.Key;
		byte[] IV = aes.IV;
		
		try
		{
			aes.Clear();

			// AESCSP will throw an ObjectDisposedException after class is used after Clear is called.  This is reasonable behavior even though
			// it's different than DESCSP, etc.  If the class is still usable (ie the managed version) then make sure the key and IV are not
			// the same.
			//
			if (CompareBytes(aes.Key, key))
			{
				Console.WriteLine("Error - key not reset after Clear");
				return false;
			}

			if (CompareBytes(aes.IV, IV))
			{
				Console.WriteLine("Error - IV not reset after Clear");
				return false;
			}
		}
		catch (ObjectDisposedException)
		{
		}

		return true;
	}

	public static bool TestTransform(Aes aes)
	{
		ICryptoTransform encryptor;
		ICryptoTransform decryptor;

		encryptor = aes.CreateEncryptor();
		decryptor = aes.CreateDecryptor();

		if (encryptor.CanReuseTransform != true)
		{
			Console.WriteLine("Error - encryptor CanReuseTransform not true");
			return false;
		}

		if (decryptor.CanReuseTransform != true)
		{
			Console.WriteLine("Error - decryptor CanReuseTransform not true");
			return false;
		}

		if (encryptor.CanTransformMultipleBlocks != true)
		{
			Console.WriteLine("Error - encryptor CanTransformMultipleBlocks not true");
			return false;
		}

		if (decryptor.CanTransformMultipleBlocks != true)
		{
			Console.WriteLine("Error - decryptor CanTransformMultipleBlocks not true");
			return false;
		}

		if (encryptor.InputBlockSize != 16)
		{
			Console.WriteLine("Error - encryptor InputBlockSize not 16");
			return false;
		}

		if (decryptor.InputBlockSize != 16)
		{
			Console.WriteLine("Error - decryptor InputBlockSize not 16");
			return false;
		}

		if (encryptor.OutputBlockSize != 16)
		{
			Console.WriteLine("Error - encryptor OutputBlockSize not 16");
			return false;
		}

		if (decryptor.OutputBlockSize != 16)
		{
			Console.WriteLine("Error - decryptor OutputBlockSize not 16");
			return false;
		}

		return true;
	}
}