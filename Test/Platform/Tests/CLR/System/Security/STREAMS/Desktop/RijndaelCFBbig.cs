//
//	RijndaelCFBbig
//		The test does Rijndael in CFB mode roundtrip for a large byte array
//

using System;
using System.Security.Cryptography;
using System.IO;

public class RijndaelCFBbig
{

public static bool Test()
{
	bool bRes = true;
	Random rnd = new Random();

	byte[] Key = new byte[] {0x2b,0x7e,0x15,0x16,0x28,0xae,0xd2,0xa6,0xab,0xf7,0x15,0x88,0x09,0xcf,0x4f,0x3c};
	byte[] IV = new byte[] {0x00,0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0a,0x0b,0x0c,0x0d,0x0e,0x0f};

	int len = rnd.Next(100000, 2000000); // 100K to 2mils
	byte[] Plain = new byte[len];
	rnd.NextBytes(Plain);

	Console.WriteLine("Generated a random byte array of size " + len);

	//
	//	128 bit key
	//

	Console.WriteLine("Testing test vectors with 128bit key...");
	byte[] Encrypted = EncryptCFB(Key, IV, Plain);
	byte[] Decrypted = DecryptCFB(Key, IV, Encrypted);

	if (!Compare(Plain, Decrypted)) {
		Console.WriteLine("FAIL: pass with 128 bit key results differ!");
		//+ Environment.NewLine + "Expected:" + Environment.NewLine +
			//			BitConverter.ToString(Plain) + Environment.NewLine +
				//		"Computed:" + Environment.NewLine + BitConverter.ToString(Decrypted));
		bRes = false;
	}

	//
	//	192 bit key
	//

	Console.WriteLine("Testing test vectors with 192bit key...");
	Key = new byte[] {0x8e,0x73,0xb0,0xf7,0xda,0x0e,0x64,0x52,0xc8,0x10,0xf3,0x2b,0x80,0x90,0x79,0xe5,0x62,0xf8,0xea,0xd2,0x52,0x2c,0x6b,0x7b};
	Encrypted = EncryptCFB(Key, IV, Plain);
	Decrypted = DecryptCFB(Key, IV, Encrypted);
	if (!Compare(Plain, Decrypted)) {
		Console.WriteLine("FAIL: pass with 192 bit key results differ!");
		//+ Environment.NewLine + "Expected:" + Environment.NewLine +
			//			BitConverter.ToString(Plain) + Environment.NewLine +
				//		"Computed:" + Environment.NewLine + BitConverter.ToString(Decrypted));
		bRes = false;
	}


	//
	//	256 bit key
	//

	Console.WriteLine("Testing test vectors with 256bit key...");
	Key = new byte[] {0x60,0x3d,0xeb,0x10,0x15,0xca,0x71,0xbe,0x2b,0x73,0xae,0xf0,0x85,0x7d,0x77,0x81,0x1f,0x35,0x2c,0x07,0x3b,0x61,0x08,0xd7,0x2d,0x98,0x10,0xa3,0x09,0x14,0xdf,0xf4};
	Encrypted = EncryptCFB(Key, IV, Plain);
	Decrypted = DecryptCFB(Key, IV, Encrypted);
	if (!Compare(Plain, Decrypted)) {
		Console.WriteLine("FAIL: pass with 256 bit key results differ!" + Environment.NewLine + "Expected:" + Environment.NewLine +
						BitConverter.ToString(Plain) + Environment.NewLine +
						"Computed:" + Environment.NewLine + BitConverter.ToString(Decrypted));
		bRes = false;
	}

	return bRes;	
}

static Boolean Compare(Byte[] rgb1, Byte[] rgb2) { 
	int 	i;
	if (rgb1.Length != rgb2.Length) return false;
	for (i=0; i<rgb1.Length; i++) {
		if (rgb1[i] != rgb2[i]) return false;
	}
	return true;
}

public static byte[] EncryptCFB(byte[] Key, byte[] IV, byte[] data)
{
	RijndaelManaged r = new RijndaelManaged();
	r.Mode = CipherMode.CFB;
	MemoryStream ms = new MemoryStream();
	CryptoStream cs = new CryptoStream(ms, r.CreateEncryptor(Key, IV), CryptoStreamMode.Write);
	cs.Write(data, 0, data.Length);
	cs.Close();
	return ms.ToArray();
}

public static byte[] DecryptCFB(byte[] Key, byte[] IV, byte[] data)
{
	RijndaelManaged r = new RijndaelManaged();
	r.Mode = CipherMode.CFB;
	MemoryStream ms = new MemoryStream();
	CryptoStream cs = new CryptoStream(ms, r.CreateDecryptor(Key, IV), CryptoStreamMode.Write);
	cs.Write(data, 0, data.Length);
	cs.Close();
	return ms.ToArray();
}



public static void Main()
{
	try {
		   
		   if (Test()) {
			   Console.WriteLine("PASSED");
			   Environment.ExitCode = 100;
		   } else {
			   Console.WriteLine("FAILED");
			   Environment.ExitCode = 101;
		   }
	
	   }
	   catch(Exception e) {
		   Console.WriteLine();
		   Console.Write("Exception: {0}", e.ToString());
		   Environment.ExitCode = 101;
	   }

}



}

