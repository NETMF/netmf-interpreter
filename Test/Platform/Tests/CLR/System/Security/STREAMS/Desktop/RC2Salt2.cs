//
//	The test ensures that RC2 with salt encryption result matches manual-salt results
//

using System;
using System.Security.Cryptography;

public class RC2SaltTest2
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

		// encrypt by default
		RC2CryptoServiceProvider rc2 = new RC2CryptoServiceProvider();
		rc2.Key = new byte[]{5,4,3,2,1,0,0,0,0,0,0,0,0,0,0,0};	// salt only takes effect when we use a 40-bit key
		byte[] encrypted1 = (rc2.CreateEncryptor()).TransformFinalBlock(plain, 0, plain.Length);

		// encrypt with salt
		RC2CryptoServiceProvider rc2s = new RC2CryptoServiceProvider();
		rc2s.UseSalt = true;
		rc2s.Key = new byte[]{1,2,3,4,5};
		rc2s.IV = rc2.IV;
		byte[] encrypted2 = (rc2s.CreateEncryptor()).TransformFinalBlock(plain, 0, plain.Length);

		if (CompareSlow(encrypted1, encrypted2))
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
