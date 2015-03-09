// Tests the ProduceLegacyHmacValues property with the legacyHMACMode appcompat switch not set 
// (default setting - not legacy).  If the property is set it should always override the config setting.
//
using System;
using System.Security.Cryptography;

class TestClass
{
	// Test vector #1 from RFC 4231 for HMAC-SHA-384 and HMAC-SHA-512.  Includes both broken ("legacy") and correct hash values.
	//
	public const string KeyStr            = "0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b";
	public const string DataStr           = "4869205468657265";
	public const string Hash384CorrectStr = "afd03944d84895626b0825f4ab46907f15f9dadbe4101ec682aa034c7cebc59cfaea9ea9076ede7f4af152e8b2fa9cb6";
	public const string Hash384LegacyStr  = "0a046aaa0255e432912228f8ccda437c8a8363fb160afb0570ab5b1fd5ddc20eb1888b9ed4e5b6cb5bc034cd9ef70e40";
	public const string Hash512CorrectStr = "87aa7cdea5ef619d4ff0b4241a1d6cb02379f4e2ce4ec2787ad0b30545e17cdedaa833b7d6b8a702038b274eaea3f4e4be9d914eeb61f1702e696c203a126854";
	public const string Hash512LegacyStr  = "9656975ee5de55e75f2976ecce9a04501060b9dc22a6eda2eaef638966280182477fe09f080b2bf564649cad42af8607a2bd8d02979df3a980f15e2326a0a22a";

	public const int PassCode = 100;
	public const int FailCode = 101;
	
	public static int Main()
	{
		byte[] key = ParseHexBytes(KeyStr);
		byte[] data = ParseHexBytes(DataStr);
		byte[] hash384Correct = ParseHexBytes(Hash384CorrectStr);
		byte[] hash384Legacy  = ParseHexBytes(Hash384LegacyStr);
		byte[] hash512Correct = ParseHexBytes(Hash512CorrectStr);
		byte[] hash512Legacy  = ParseHexBytes(Hash512LegacyStr);
		
		// HMAC-SHA-384 with legacy property set -> legacy result
		//
		HMACSHA384 hm384Legacy = new HMACSHA384(key);
		hm384Legacy.ProduceLegacyHmacValues = true;
		byte[] result384Legacy = hm384Legacy.ComputeHash(data);

		if (!CompareBytes(result384Legacy, hash384Legacy))
		{
			Console.WriteLine("HMACSHA384 - ProductLegacyHmacValues=true failed");
			return FailCode;
		}
		
		// HMAC-SHA-384 with legacy property not set -> correct result
		//
		HMACSHA384 hm384Correct = new HMACSHA384(key);
		hm384Correct.ProduceLegacyHmacValues = false;
		byte[] result384Correct = hm384Correct.ComputeHash(data);

		if (!CompareBytes(result384Correct, hash384Correct))
		{
			Console.WriteLine("HMACSHA384 - ProduceLegacyHmacValues=false failed");
			return FailCode;
		}
		
		// HMAC-SHA-384 with legacy property not set -> default result (correct)
		//
		HMACSHA384 hm384Default = new HMACSHA384(key);
		byte[] result384Default = hm384Default.ComputeHash(data);
		
		if (!CompareBytes(result384Default, hash384Correct))
		{
			Console.WriteLine("HMACSHA384 - ProduceLegacyHmacValues=default failed");
			return FailCode;
		}

		// HMAC-SHA-512 with legacy property set -> legacy result
		//
		HMACSHA512 hm512Legacy = new HMACSHA512(key);
		hm512Legacy.ProduceLegacyHmacValues = true;
		byte[] result512Legacy = hm512Legacy.ComputeHash(data);

		if (!CompareBytes(result512Legacy, hash512Legacy))
		{
			Console.WriteLine("HMACSHA512 - ProduceLegacyHmacValues=true failed");
			return FailCode;
		}

		// HMAC-SHA-512 with legacy property not set -> correct result
		//
		HMACSHA512 hm512Correct = new HMACSHA512(key);
		hm512Correct.ProduceLegacyHmacValues = false;
		byte[] result512Correct = hm512Correct.ComputeHash(data);

		if (!CompareBytes(result512Correct, hash512Correct))
		{
			Console.WriteLine("HMACSHA512 - ProduceLegacyHmacValues=false failed");
			return FailCode;
		}

		// HMAC-SHA-512 with legacy property not set -> default result (correct)
		//
		HMACSHA512 hm512Default = new HMACSHA512(key);
		byte[] result512Default = hm512Default.ComputeHash(data);

		if (!CompareBytes(result512Default, hash512Correct))
		{
			Console.WriteLine("HMACSHA512 - ProduceLegacyHmacValues=default failed");
			return FailCode;
		}

		Console.WriteLine("Test passed");
		return PassCode;
	}

	// Convert a "hex number" string into a byte array
	//
	public static byte[] ParseHexBytes(string bytes)
	{
		byte[] parsedBytes = new byte[bytes.Length / 2];

		for (int i = 0; i < bytes.Length; i+= 2)
			parsedBytes[i / 2] = Byte.Parse(bytes.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);

		return parsedBytes;
	}

	// I still can't believe we don't have framework support for this! :-)
	//
	public static bool CompareBytes(byte[] x, byte[] y)
	{
		if (x.Length != y.Length)
			return false;

		for (int i = 0; i < x.Length; i++)
			if (x[i] != y[i])
				return false;

		return true;
	}
}

