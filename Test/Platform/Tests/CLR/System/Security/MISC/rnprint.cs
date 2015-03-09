using System;
using System.IO;
using System.Security.Cryptography;

public class rnprint {

	static Random Rnd = new Random();

    static void PrintByteArray(Byte[] arr)
    {
    	int i;
    	for (i=0; i<arr.Length; i++) {
    		Console.Write(arr[i] + "    ");
    		if ( (i+9)%8 == 0 ) Console.WriteLine();
   		}
   		if (i%8 != 0) Console.WriteLine();
    }


	public static Boolean Test() {
		Byte[] barr1;
		RNG_CSP rndcsp1 = new RNG_CSP();
		int l;
		
		for (int i=0; i<1000; i++) {
			l = Rnd.Next()%1500+4;
			barr1 = new Byte[l];
			rndcsp1.GetNonZeroBytes(barr1);
			PrintByteArray(barr1);
		}

		return true;
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


