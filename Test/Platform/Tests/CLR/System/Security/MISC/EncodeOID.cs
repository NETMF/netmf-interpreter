using System;
using System.Security.Cryptography;

public class EncodeOIDTest
{

	public static bool Test()
	{
		bool bRes = true;

		byte[][] abEnc = new byte[2][];
		abEnc[0] = new byte[5] { 0x06, 0x03, 0x55, 0x04, 0x03};
		abEnc[1] = new byte[7] { 0x06, 0x05, 0x2B, 0x0E, 0x03, 0x02, 0x1A};
		string[] sOID = new string[2] {"2.5.4.3", "1.3.14.3.2.26"};

		for(int i=0; i<2; i++)
		{
			byte[] abTmpEnc = CryptoConfig.EncodeOID(sOID[i]);
			Console.WriteLine("Encoding calculated for OID " + sOID[i] + " is");
			PrintByteArray(abTmpEnc);
			if (!Compare(abEnc[i], abTmpEnc)) 
			{
				Console.WriteLine("WRONG result for OID " + sOID[i] + ", expected");
				PrintByteArray(abEnc[i]);
				bRes = false;
			}
		}


		return bRes;
	}

	
	static void PrintByteArray(Byte[] arr)
	{
		int i;
		// Console.WriteLine("Length: " + arr.Length);
		for (i=0; i<arr.Length; i++) 
		{
			Console.Write("{0:X}", arr[i]);
			Console.Write("    ");
			if ( (i+9)%8 == 0 ) Console.WriteLine();
		}
		if (i%8 != 0) Console.WriteLine();
	}

	static Boolean Compare(Byte[] rgb1, Byte[] rgb2) 
	{ 
		int     i;
		if (rgb1.Length != rgb2.Length) return false;
		for (i=0; i<rgb1.Length; i++) 
		{
			if (rgb1[i] != rgb2[i]) return false;
		}
		return true;
	}

	public static void Main(String[] args) 
	{
		try 
		{
			if (Test()) 
			{
				Console.WriteLine("PASSED");
				Environment.ExitCode = 100;
			} 
			else 
			{
				Console.WriteLine("FAILED");
				Environment.ExitCode = 101;
			}

		}
		catch(Exception e) 
		{
			Console.WriteLine();
			Console.Write("Exception: {0}", e.ToString());
			Environment.ExitCode = 101;
		}
		return;
	}

}

