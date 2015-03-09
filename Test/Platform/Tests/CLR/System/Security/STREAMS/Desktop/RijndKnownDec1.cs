//
//	Rijndael Decrypting Comparison with known vectors in ECB mode.
//
//	!!! Note that Plain/Cipher meanings are reversed here
//	(that way it was easier to port from other test).

using System;
using System.Security.Cryptography; 
using System.IO; 

class RijndaelKnown1
{
    static void PrintByteArray(Byte[] arr)
    {
    	if (arr==null) {
    		Console.WriteLine("null");
    		return;
    	}
        int i;
        for (i=0; i<arr.Length; i++) {
            Console.Write("{0:X} ", arr[i]);
            if ( (i+9)%8 == 0 ) Console.WriteLine();
        }
        if (i%8 != 0) Console.WriteLine();
    }

    static Boolean Compare(Byte[] rgb1, Byte[] rgb2) { 
        int     i;
        if (rgb1.Length != rgb2.Length) return false;
        for (i=0; i<rgb1.Length; i++) {
            if (rgb1[i] != rgb2[i]) return false;
        }
        return true;
    }

    static Boolean TestKnownEnc(Byte[] Key, Byte[] IV, Byte[] Cipher, Byte[] Plain, CipherMode mode, PaddingMode padding)
    {

        Byte[]  CipherCalculated;
        
        Console.WriteLine("Encrypting the following bytes:");
        PrintByteArray(Plain);
        Console.WriteLine("With the following Key:");
        PrintByteArray(Key);
        Console.WriteLine("and IV:");
        PrintByteArray(IV);
        Console.WriteLine("mode: " + mode + "     padding: " + padding);
		Console.WriteLine("Expecting this ciphertext:");
		PrintByteArray(Cipher);        
        
        RijndaelManaged     rjnd = new RijndaelManaged();

        rjnd.Mode = mode;
		rjnd.Padding = padding;
//		rjnd.BlockSize = 128;
//		rjnd.KeySize = 128;

        ICryptoTransform sse = rjnd.CreateDecryptor(Key, IV);
        MemoryStream 	ms = new MemoryStream();
        CryptoStream    cs = new CryptoStream(ms, sse, CryptoStreamMode.Write);
        cs.Write(Plain,0,Plain.Length);
        cs.FlushFinalBlock();
        CipherCalculated = ms.ToArray();
        cs.Close();

        Console.WriteLine("Computed this cyphertext:");
        PrintByteArray(CipherCalculated);
        

        if (!Compare(Cipher, CipherCalculated)) {
        	Console.WriteLine("ERROR: result is different from the expected");
        	return false;
        }
        
        Console.WriteLine("OK");
        return true;
    }

    
    public static Boolean Test()
    {
    	Boolean bRes = true;
    	
    	Byte[] Key = {0x00,0x01,0x02,0x03,0x05,0x06,0x07,0x08,0x0A,0x0B,0x0C,0x0D,0x0F,0x10,0x11,0x12};
    	Byte[] IV  = null;
    	Byte[] Plain = {0x50,0x68,0x12,0xA4,0x5F,0x08,0xC8,0x89,0xB9,0x7F,0x59,0x80,0x03,0x8B,0x83,0x59};
    	Byte[] Cipher= {0xD8,0xF5,0x32,0x53,0x82,0x89,0xEF,0x7D,0x06,0xB5,0x06,0xA4,0xFD,0x5B,0xE9,0xC9};
		bRes = TestKnownEnc(Key, IV, Plain, Cipher, CipherMode.ECB, PaddingMode.None) && bRes;

    	Byte[] Key1 = {0x00,0x01,0x02,0x03,0x05,0x06,0x07,0x08,0x0A,0x0B,0x0C,0x0D,0x0F,0x10,0x11,0x12,0x14,0x15,0x16,0x17,0x19,0x1A,0x1B,0x1C,0x1E,0x1F,0x20,0x21,0x23,0x24,0x25,0x26};
    	Byte[] IV1  = null;
		Byte[] Plain1 = {0x83,0x4E,0xAD,0xFC,0xCA,0xC7,0xE1,0xB3,0x06,0x64,0xB1,0xAB,0xA4,0x48,0x15,0xAB};
    	Byte[] Cipher1= {0x19,0x46,0xDA,0xBF,0x6A,0x03,0xA2,0xA2,0xC3,0xD0,0xB0,0x50,0x80,0xAE,0xD6,0xFC};
		bRes = TestKnownEnc(Key1, IV1, Plain1, Cipher1, CipherMode.ECB, PaddingMode.None) && bRes;

    	Byte[] Key2 = {0x80,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
    	Byte[] IV2  = null;
		Byte[] Plain2 = {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
    	Byte[] Cipher2= {0x0E,0xDD,0x33,0xD3,0xC6,0x21,0xE5,0x46,0x45,0x5B,0xD8,0xBA,0x14,0x18,0xBE,0xC8};
		bRes = TestKnownEnc(Key2, IV2, Plain2, Cipher2, CipherMode.ECB, PaddingMode.None) && bRes;

    	Byte[] Key3 = {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
    	Byte[] IV3  = null;
		Byte[] Plain3 = {0x80,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
    	Byte[] Cipher3= {0x3A,0xD7,0x8E,0x72,0x6C,0x1E,0xC0,0x2B,0x7E,0xBF,0xE9,0x2B,0x23,0xD9,0xEC,0x34};
		bRes = TestKnownEnc(Key3, IV3, Plain3, Cipher3, CipherMode.ECB, PaddingMode.None) && bRes;

    	Byte[] Key4 = {0x80,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
    	Byte[] IV4  = null;
		Byte[] Plain4 = {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
    	Byte[] Cipher4= {0xDE,0x88,0x5D,0xC8,0x7F,0x5A,0x92,0x59,0x40,0x82,0xD0,0x2C,0xC1,0xE1,0xB4,0x2C};
		bRes = TestKnownEnc(Key4, IV4, Plain4, Cipher4, CipherMode.ECB, PaddingMode.None) && bRes;

    	Byte[] Key5 = {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
    	Byte[] IV5  = null;
		Byte[] Plain5 = {0x80,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
    	Byte[] Cipher5= {0x6C,0xD0,0x25,0x13,0xE8,0xD4,0xDC,0x98,0x6B,0x4A,0xFE,0x08,0x7A,0x60,0xBD,0x0C};
		bRes = TestKnownEnc(Key5, IV5, Plain5, Cipher5, CipherMode.ECB, PaddingMode.None) && bRes;

		return bRes;
    }
    
    
    public static void Main(String[] args) 
    {

        try {
            
            if (Test())
            {
                Console.WriteLine("PASSED");
                Environment.ExitCode = (100);
            } else {
                Console.WriteLine("FAILED");
                Environment.ExitCode = (123);
            }

        }
        catch(Exception e) {
            Console.Write("Exception: {0}", e.ToString());
            Environment.ExitCode = (123);
        }
        return;
    }
}
