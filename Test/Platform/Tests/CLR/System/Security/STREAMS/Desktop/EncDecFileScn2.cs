using System;
using System.Security.Cryptography;
using System.Security;
using System.IO;
using System.Text;

class EncDec1
{


	private static void EncryptData(String inName, String outName, byte[] desKey, byte[] desIV)
	 {
        FileStream fs = new FileStream(inName, FileMode.Open, FileAccess.Read);
        // Create an instance of the DESCryptoServiceProvider cipher
        SymmetricAlgorithm aes = DESCryptoServiceProvider.Create();
        // set the key to be the derivedKey computed above
        aes.Key = desKey;
        // set the IV to be all zeros
        aes.IV = desIV;  // arrays are zero-initialized
        // now wrap an encryption transform around the filestream
        CryptoStream stream1 = new CryptoStream(fs, aes.CreateEncryptor(), CryptoStreamMode.Read);
        // The result of reading from stream1 is ciphertext, but we want it
        // base64-encoded, so wrap another transform around it
        CryptoStream stream2 = new CryptoStream(stream1, new ToBase64Transform(), CryptoStreamMode.Read);

        FileStream fsout = new FileStream(outName, FileMode.OpenOrCreate);
        byte[] buffer = new byte[1024];
        int bytesRead;
        do {
            bytesRead = stream2.Read(buffer,0,1024);
            fsout.Write(buffer,0,bytesRead);
        } while (bytesRead > 0);
        fsout.Flush();
        fsout.Close();
    }


	private static void DecryptData(String inName, String outName, byte[] desKey, byte[] desIV)
	 {
        FileStream fs = new FileStream(inName, FileMode.Open, FileAccess.Read);
        // Create an instance of the DESCryptoServiceProvider cipher
        SymmetricAlgorithm aes = DESCryptoServiceProvider.Create();
        // set the key to be the derivedKey computed above
        aes.Key = desKey;
        // set the IV to be all zeros
        aes.IV = desIV;  // arrays are zero-initialized
        // Base64-decode the ciphertext
        CryptoStream stream1 = new CryptoStream(fs, new FromBase64Transform(FromBase64TransformMode.IgnoreWhiteSpaces), CryptoStreamMode.Read);
        // now wrap a decryption transform around stream1
        CryptoStream stream2 = new CryptoStream(stream1, aes.CreateDecryptor(), CryptoStreamMode.Read);
        FileStream fsout = new FileStream(outName, FileMode.OpenOrCreate);
        byte[] buffer = new byte[1024];
        int bytesRead;
        UTF8Encoding utf8 = new UTF8Encoding();
        do {
            bytesRead = stream2.Read(buffer,0,1024);
            fsout.Write(buffer,0,bytesRead);
        } while (bytesRead > 0);
        fsout.Flush();
        fsout.Close();
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
