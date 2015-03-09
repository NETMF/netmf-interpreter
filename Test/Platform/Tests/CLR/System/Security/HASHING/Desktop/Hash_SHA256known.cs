// known test vectors test for SHA256
//	test vectors came from http://csrc.nist.gov/cryptval/shs/sha256-384-512.pdf
//
using System;
using System.Security.Cryptography; 
using System.IO; 


class Hash_SHA256known
{

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
        for (i=0; i<arr.Length; i++) {
            Console.Write(arr[i] + "    ");
            if ( (i+9)%8 == 0 ) Console.WriteLine();
        }
        if (i%8 != 0) Console.WriteLine();
    }

    static Boolean Test()
    {
        Boolean bRes = true;
        Byte[] abData1 = { (Byte)'a', (Byte)'b', (Byte)'c' };
        Byte[] abDigest1 = {0xba, 0x78, 0x16, 0xbf, 0x8f, 0x01, 0xcf, 0xea,
        					0x41, 0x41, 0x40, 0xde, 0x5d, 0xae, 0x22, 0x23,
        					0xb0, 0x03, 0x61, 0xa3, 0x96, 0x17, 0x7a, 0x9c,
        					0xb4, 0x10, 0xff, 0x61, 0xf2, 0x00, 0x15, 0xad};
        String sData2 = "abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq";
        Byte[] abData2 = new Byte[sData2.Length];
        for (int i=0; i<sData2.Length; i++) abData2[i] = (Byte)sData2[i];
        Byte[] abDigest2 = {0x24, 0x8d, 0x6a, 0x61, 0xd2, 0x06, 0x38, 0xb8,
        					0xe5, 0xc0, 0x26, 0x93, 0x0c, 0x3e, 0x60, 0x39,
        					0xa3, 0x3c, 0xe4, 0x59, 0x64, 0xff, 0x21, 0x67,
        					0xf6, 0xec, 0xed, 0xd4, 0x19, 0xdb, 0x06, 0xc1};

        Console.WriteLine("Testing SHA1 hash...");
        SHA256Managed   sha1 = new SHA256Managed();
        SHA256Managed   sha2 = new SHA256Managed();
        sha1.ComputeHash(abData1);
        sha2.ComputeHash(abData2);
        Console.WriteLine("The computed hash #1 is : ");
        PrintByteArray(sha1.Hash);
        Console.WriteLine("The correct hash #1 is : ");
        PrintByteArray(abDigest1);
        if(Compare(sha1.Hash, abDigest1)) {
            Console.WriteLine("CORRECT");
        } else {
            Console.WriteLine("INCORRECT");
            bRes = false;
        }
        Console.WriteLine("The computed hash #2 is : ");
        PrintByteArray(sha2.Hash);
        Console.WriteLine("The correct hash #2 is : ");
        PrintByteArray(abDigest2);
        if(Compare(sha2.Hash, abDigest2)) {
            Console.WriteLine("CORRECT");
        } else {
            Console.WriteLine("INCORRECT");
            bRes = false;
        }
        
        return bRes;
    }

    public static void Main(String[] args) 
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

