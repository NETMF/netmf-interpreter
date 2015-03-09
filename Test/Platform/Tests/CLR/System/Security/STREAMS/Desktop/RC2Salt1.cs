//
//	The test ensures that RC2 Salt property has effect on encryption
//

using System;
using System.Security.Cryptography;

public class RC2SaltTest1
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

		// comparison benchmark
/*		byte[] ab = new byte[1000000];
		rnd.NextBytes(ab);
		DateTime dt1 = DateTime.Now;
		CompareSlow(ab, ab);
		Console.WriteLine("Slow: " + (DateTime.Now-dt1));
		dt1 = DateTime.Now;
		Compare(ab,ab);
		Console.WriteLine("Fast: " + (DateTime.Now -dt1));
*/		
		
		// create a random array of random bytes
		int len = rnd.Next(1000000);
		byte[] plain = new byte[len];
		rnd.NextBytes(plain);
		Console.Write("Working with " + len + " bytes of plaintext...");

		// encrypt by default
		RC2CryptoServiceProvider rc2 = new RC2CryptoServiceProvider();
		rc2.Key = new byte[]{1,2,3,4,5};	// salt only takes effect when we use a 40-bit key
		byte[] encrypted1 = (rc2.CreateEncryptor()).TransformFinalBlock(plain, 0, plain.Length);

		// encrypt with salt
		RC2CryptoServiceProvider rc2s = new RC2CryptoServiceProvider();
		rc2s.UseSalt = true;
		rc2s.Key = rc2.Key;
		rc2s.IV = rc2.IV;
		byte[] encrypted2 = (rc2s.CreateEncryptor()).TransformFinalBlock(plain, 0, plain.Length);

		if (!CompareSlow(encrypted1, encrypted2))
		{
			Console.WriteLine("OK.");
			return true;
		} else {
			Console.WriteLine("FAIL.");
			return false;
		}
	}

/*	public static unsafe bool Compare(byte[] a1, byte[] a2)
	{
		if ((a1 == null) || (a2 == null))
			return a1 == a2;

		if (a1.Length != a2.Length)
			return false;

		int i = 0;
		int l = a1.Length;

		fixed (byte* bp1 = a1)
		{
			UInt64* p1 = (UInt64*)bp1;
			fixed(byte* bp2 = a2)
			{
				UInt64* p2 = (UInt64*)bp2;
				while(l - i >= 8)
				{
					if (*p1 != *p2) return false;
					p1++;
					p2++;
					i += 8;
				}
					
			}
		}

		while (i < l) 
		{
			if (a1[i] != a2[i]) return false;
			i++;
		}

		return true;
	}
*/
	
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
