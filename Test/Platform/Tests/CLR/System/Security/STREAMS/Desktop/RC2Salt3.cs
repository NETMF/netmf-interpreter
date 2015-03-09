//
//	The test ensures that RC2 with salt roundtrips properly
//

using System;
using System.Security.Cryptography;

public class RC2SaltTest3
{
	public static int Main()
	{
		bool bRes = true;

		for (int i = 0; i < 10; i++)
			bRes = Test() & bRes;

		if (bRes)
		{
			Console.WriteLine("PASSED");
			Environment.ExitCode = 100;
			return 100;
		}
		else
		{
			Console.WriteLine("FAILED");
			Environment.ExitCode = 123;
			return 123;
		}
	}

	public static bool Test()
	{
		Random rnd = new Random();
		
		// create a random array of random bytes
		int len = rnd.Next(1000000);
		byte[] plain = new byte[len];
		rnd.NextBytes(plain);
		Console.Write("Working with " + len + " bytes of plaintext...");

		// encrypt with salt
		RC2CryptoServiceProvider rc2s = new RC2CryptoServiceProvider();
		rc2s.UseSalt = true;
		rc2s.Key = new byte[]{1,2,3,4,5};
		byte[] encrypted = (rc2s.CreateEncryptor()).TransformFinalBlock(plain, 0, plain.Length);

		// decrypt with salt
		RC2CryptoServiceProvider rc2sd = new RC2CryptoServiceProvider();
		rc2sd.UseSalt = true;
		rc2sd.Key = rc2s.Key;
		rc2sd.IV = rc2s.IV;
		byte[] decrypted = (rc2sd.CreateDecryptor()).TransformFinalBlock(encrypted, 0, encrypted.Length);
		
		if (CompareSlow(plain, decrypted))
		{
			Console.WriteLine("OK.");
			return true;
		} else {
			Console.WriteLine("FAIL.");
			return false;
		}
	}

	
	public static bool CompareSlow(byte[] a1, byte[] a2)
	{
		if ((a1 == null) || (a2 == null))
			return a1 == a2;

		if (a1.Length != a2.Length)
			return false;

		for(int i = 0; i< a1.Length; i++)
			if (a1[i] != a2[i++]) return false;

		return true;
	}
}

