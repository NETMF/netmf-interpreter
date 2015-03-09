using System;
using System.IO;
using System.Security.Cryptography;

public class CryptDeriveKeyTest
{
	public const int NO_PASSES		= 20;
	public const int MAX_PASS_LEN	= 300;
	public const int MAX_SALT_LEN	= 130;
	public const int MAX_COMP		= 10;

    static Random Rnd = new Random();

	static Boolean Compare(Byte[] rgb1, Byte[] rgb2) { 
        int     i;
        if (rgb1.Length != rgb2.Length) return false;
        for (i=0; i<rgb1.Length; i++) {
            if (rgb1[i] != rgb2[i]) return false;
        }
        return true;
    }

	static void PrintByteArray(Byte[] arr)
    {
        int i;
        Console.WriteLine("Length: " + arr.Length);
        for (i=0; i<arr.Length; i++) {
            Console.Write("{0:X}", arr[i]);
            Console.Write("    ");
            if ( (i+9)%8 == 0 ) Console.WriteLine();
        }
        if (i%8 != 0) Console.WriteLine();
    }


	public static Boolean Test()
	{
		Boolean bRes = false;

		bRes = TestRepeated();
		if (bRes == false) {
			Console.WriteLine("The 1st part of the test is failed");
		}
		
		if (TestKnown() == false) {
			Console.WriteLine("The 2nd part of the test is failed");
			bRes = false ;
		}

		return bRes;
	}

	public static Boolean TestRepeated()
	{
		Boolean bRes = true;
		int l, key_size;
		Char[] ach;
		String s;
		Byte[] salt, the_key, temp_key, iv = new Byte[8];
		PasswordDeriveBytes pdb;

		for(int i=0; i<NO_PASSES; i++) {
			l = Rnd.Next(MAX_PASS_LEN)+1;
			ach = new Char[l];
			for(int k=0; k<l; k++) ach[k] = (Char)(Rnd.Next(26)+65);
			s = new String(ach);
			salt = new Byte[Rnd.Next(MAX_SALT_LEN)];
			Rnd.NextBytes(salt);
			key_size = Rnd.Next(128);
			Rnd.NextBytes(iv);

			pdb = new PasswordDeriveBytes(s, salt);
			the_key = pdb.CryptDeriveKey("RC2", "SHA1", /*key_size*/ 128, iv);

			Console.WriteLine("--------------------------------------");
			PrintByteArray(the_key);

			for (int j=0; j<MAX_COMP;j++) {
				temp_key = pdb.CryptDeriveKey("RC2", "SHA1", /*key_size*/ 128, iv);
				Console.WriteLine("--------------------------------------");
				PrintByteArray(temp_key);
				if (!Compare(the_key, temp_key)) {
					bRes = false;
					Console.WriteLine("Two passes of CryptDeriveKey yielded different results");
					break;
				}
			}
			if (bRes == false) break;
		}

		return bRes;
	}


	public static Boolean TestKnown()
	{
		Boolean bRes = true;
		Byte[] IV = new Byte[8];
		Byte[] PlainText = {0,1,2,3,4,5,6,7};
		Byte[] KnownVector = {0x7A, 0x50, 0x39, 0x82, 0xB5, 0x0E, 0xB0, 0x0D, 0x1F, 0x37, 0x9D, 0xC8, 0x36, 0x09, 0xD3, 0xFF};

		PasswordDeriveBytes pdb = new PasswordDeriveBytes("simplepassword", null);
		Byte[] the_key = pdb.CryptDeriveKey("RC2", "MD5", 40, IV);
		
		RC2CryptoServiceProvider rc2 = new RC2CryptoServiceProvider();
        ICryptoTransform sse = rc2.CreateEncryptor(the_key, IV);
        MemoryStream ms = new MemoryStream();
        CryptoStream cs = new CryptoStream(ms, sse, CryptoStreamMode.Write);
        cs.Write(PlainText,0,PlainText.Length);
        cs.FlushFinalBlock();
        byte[] ciphertext = ms.ToArray();
        cs.Close();

		Console.WriteLine("--- Cipher Text : ----");
		PrintByteArray(ciphertext);
		Console.WriteLine("--- Known vector : ----");
		PrintByteArray(KnownVector);
		
		if(!Compare(ciphertext, KnownVector)) {
			Console.WriteLine("Known and calculated values differ!");
			bRes = false;
		}

		return bRes;
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
        return;

	}

}