using System;
using System.Security.Cryptography;
using System.Security;
using System.IO;

class EncDec
{


	private static void EncryptData(String inName, String outName, byte[] desKey, byte[] desIV)
	 {
	     //Create the file streams to handle the input and output files.
	     FileStream fin = new FileStream(inName, FileMode.Open, FileAccess.Read);
	     FileStream fout = new FileStream(outName, FileMode.OpenOrCreate, FileAccess.Write);
	     fout.SetLength(0);

	     //Create variables to help with read and write.
	     byte[] bin = new byte[100]; //This is intermediate storage for the encryption.
	     long rdlen = 0;              //This is the total number of bytes written.
	     long totlen = fin.Length;    //This is the total length of the input file.
	     int len;                     //This is the number of bytes to be written at a time.

	     SymmetricAlgorithm des = new DESCryptoServiceProvider();
         des.Padding = PaddingMode.PKCS7;
	     CryptoStream encStream = new CryptoStream(fout, des.CreateEncryptor(desKey, desIV), CryptoStreamMode.Write);

	     Console.WriteLine("Encrypting...");

	     //Read from the input file, then encrypt and write to the output file.
	     while(rdlen < totlen)
	     {
	         len = fin.Read(bin, 0, 100);
	         encStream.Write(bin, 0, len);
	         rdlen = rdlen + len;
             Console.WriteLine("{0} bytes processed", rdlen);
         }
	     encStream.Close();
    }


	private static void DecryptData(String inName, String outName, byte[] desKey, byte[] desIV)
	 {
	     //Create the file streams to handle the input and output files.
	     FileStream fin = new FileStream(inName, FileMode.Open, FileAccess.Read);
	     FileStream fout = new FileStream(outName, FileMode.OpenOrCreate, FileAccess.Write);
	     fout.SetLength(0);

	     //Create variables to help with read and write.
	     byte[] bin = new byte[100]; //This is intermediate storage for the encryption.
	     long rdlen = 0;              //This is the total number of bytes written.
	     long totlen = fin.Length;    //This is the total length of the input file.
	     int len;                     //This is the number of bytes to be written at a time.

	     SymmetricAlgorithm des = new DESCryptoServiceProvider();
         des.Padding = PaddingMode.PKCS7;
	     CryptoStream encStream = new CryptoStream(fout, des.CreateDecryptor(desKey, desIV), CryptoStreamMode.Write);

	     Console.WriteLine("Decrypting...");

	     //Read from the input file, then encrypt and write to the output file.
	     while(rdlen < totlen)
	     {
	         len = fin.Read(bin, 0, 100);
	         encStream.Write(bin, 0, len);
	         rdlen = rdlen + len;
             Console.WriteLine("{0} bytes processed", rdlen);
	     }

	     encStream.Close();
    }
		
    public static void Main(String[] args)
    {
    	try {
    	
	    	DESCryptoServiceProvider dp = new DESCryptoServiceProvider();

	    	EncryptData( args[0], args[0] + ".encrypted", dp.Key, dp.IV );
		    DecryptData( args[0] + ".encrypted", args[0] + ".decrypted", dp.Key, dp.IV );
	    }
	    catch(Exception e) {
	    	Console.WriteLine("FAILED: \n" + e.ToString());
   		    Environment.ExitCode = 101;
   		    return;
	    }
	    Console.WriteLine("PASSED");
	    Environment.ExitCode = 100;
    }
}
