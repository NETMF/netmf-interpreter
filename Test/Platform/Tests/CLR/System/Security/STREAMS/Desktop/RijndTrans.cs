//
//	Tests RigndaelTransform without CryptoStreams
//

using System;
using System.Security.Cryptography; 
using System.IO; 

class RijndaelTransform
{
    

	public static bool Test()
	{
		bool bRes = true;

		RijndaelManaged rm = new RijndaelManaged();
		ICryptoTransform cte = rm.CreateEncryptor();
		ICryptoTransform ctd = rm.CreateDecryptor();

		byte[] plain1 = new byte[]{0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15};
		byte[] cypher1 = cte.TransformFinalBlock(plain1, 0, plain1.Length);
		byte[] decr1 = ctd.TransformFinalBlock(cypher1, 0, cypher1.Length);

		if (!Compare(plain1, decr1)) {
			Console.WriteLine("Case #1 failed");
			bRes = false;
		}

		byte[] plain2 = new byte[] {0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31};
		byte[] cypher2 = cte.TransformFinalBlock(plain2, 0, plain2.Length);
		byte[] decr2 = ctd.TransformFinalBlock(cypher2, 0, cypher2.Length);

		if (!Compare(plain2, decr2)) {
			Console.WriteLine("Case #2 failed");
			bRes = false;
		}


   		byte[] plain3 = new byte[] {0,1,2};
		byte[] cypher3 = cte.TransformFinalBlock(plain3, 0, plain3.Length);
		byte[] decr3 = ctd.TransformFinalBlock(cypher3, 0, cypher3.Length);

		if (!Compare(plain3, decr3)) {
			Console.WriteLine("Case #3 failed");
			bRes = false;
		}


   		byte[] plain4 = new byte[] {0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16};
		byte[] cypher4 = cte.TransformFinalBlock(plain4, 0, plain4.Length);
		byte[] decr4 = ctd.TransformFinalBlock(cypher4, 0, cypher4.Length);

		if (!Compare(plain4, decr4)) {
			Console.WriteLine("Case #4 failed");
			bRes = false;
		}


   		byte[] plain5 = new byte[] {0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15};
   		byte[] cypher5tmp1 = new byte[rm.BlockSize/8];
   		int i5 = cte.TransformBlock(plain5, 0, plain5.Length, cypher5tmp1, 0);
   		if (i5 != rm.BlockSize/8) {
   			Console.WriteLine("Case #5 - TransformBlock returned value other than block size");
   			bRes = false;
   		}
		byte[] cypher5tmp2 = cte.TransformFinalBlock(new byte[0], 0, 0);
		byte[] cypher5 = new byte[cypher5tmp1.Length + cypher5tmp2.Length];
		Array.Copy(cypher5tmp1, cypher5, cypher5tmp1.Length);
		Array.Copy(cypher5tmp2, 0, cypher5, cypher5tmp1.Length, cypher5tmp2.Length);
		byte[] decr5 = ctd.TransformFinalBlock(cypher5, 0, cypher5.Length);

		if (!Compare(plain5, decr5)) {
			Console.WriteLine("Case #5 failed");
			bRes = false;
		}


   		byte[] plain6 = new byte[] {0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15};
   		byte[] cypher6tmp1 = new byte[rm.BlockSize/8];
   		int i6 = cte.TransformBlock(plain6, 0, plain6.Length, cypher6tmp1, 0);
   		if (i6 != rm.BlockSize/8) {
   			Console.WriteLine("Case #6 - TransformBlock returned value other than block size");
   			bRes = false;
   		}
		byte[] cypher6tmp2 = cte.TransformFinalBlock(new byte[0], 0, 0);
		byte[] cypher6 = new byte[cypher6tmp1.Length + cypher6tmp2.Length];
		Array.Copy(cypher6tmp1, cypher6, cypher6tmp1.Length);
		Array.Copy(cypher6tmp2, 0, cypher6, cypher6tmp1.Length, cypher6tmp2.Length);

		i6 = ctd.TransformBlock(cypher6, 0, rm.BlockSize/8, new byte[0], 0);
		if (i6 != 0) {
			Console.WriteLine("Case #6 - TransformBlock returned value other than 0");
			bRes = false;
		}
		byte[] decr6 = ctd.TransformFinalBlock(cypher6, rm.BlockSize/8, cypher6.Length-rm.BlockSize/8);

		if (!Compare(plain6, decr6)) {
			Console.WriteLine("Case #6 failed");
			bRes = false;
		}


   		byte[] plain7 = new byte[] {0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15};
   		byte[] cypher7tmp1 = new byte[rm.BlockSize/8];
   		int i7 = cte.TransformBlock(plain7, 0, plain7.Length, cypher7tmp1, 0);
   		if (i7 != rm.BlockSize/8) {
   			Console.WriteLine("Case #7 - TransformBlock returned value other than block size");
   			bRes = false;
   		}
		byte[] cypher7tmp2 = cte.TransformFinalBlock(new byte[0], 0, 0);
		byte[] cypher7 = new byte[cypher7tmp1.Length + cypher7tmp2.Length];
		Array.Copy(cypher7tmp1, cypher7, cypher7tmp1.Length);
		Array.Copy(cypher7tmp2, 0, cypher7, cypher7tmp1.Length, cypher7tmp2.Length);

		byte[] decr7 = new byte[16];
		i7 = ctd.TransformBlock(cypher7, 0, cypher7.Length, decr7, 0);
		if (i7 != 16) {
			Console.WriteLine("Case #7 - TransformBlock returned value other than 16");
			bRes = false;
		}
		ctd.TransformFinalBlock(new byte[0], 0, 0);

		if (!Compare(plain7, decr7)) {
			Console.WriteLine("Case #7 failed");
			bRes = false;
		}


   		byte[] plain8 = new byte[] {0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34};
		byte[] cypher8 = cte.TransformFinalBlock(plain8, 0, plain8.Length);

		byte[] decr8tmp1 = new byte[rm.BlockSize/8];
		int i8 = ctd.TransformBlock(cypher8, 0, rm.BlockSize/4, decr8tmp1, 0);
		if (i8 != rm.BlockSize/8) {
			Console.WriteLine("Case #8 - TransformBlock returned value other than 0");
			bRes = false;
		}
		byte[] decr8tmp2 = ctd.TransformFinalBlock(cypher8, rm.BlockSize/4, cypher8.Length-rm.BlockSize/4);
		byte[] decr8 = new byte[decr8tmp1.Length + decr8tmp2.Length];
		Array.Copy(decr8tmp1, decr8, decr8tmp1.Length);
		Array.Copy(decr8tmp2, 0, decr8, decr8tmp1.Length, decr8tmp2.Length);

		if (!Compare(plain8, decr8)) {
			Console.WriteLine("Case #8 failed");
			bRes = false;
		}


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

}
