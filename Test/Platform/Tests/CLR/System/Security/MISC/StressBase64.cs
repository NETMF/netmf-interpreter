using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

public class stressbase64 {

	public const int NUMBER_OF_PASSES = 10;
	public const int MAX_LENGTH		 = 100000;

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

    static Boolean Compare(Byte[] rgb1, Byte[] rgb2) { 
        int     i;
        if (rgb1.Length != rgb2.Length) return false;
        for (i=0; i<rgb1.Length; i++) {
            if (rgb1[i] != rgb2[i]) return false;
        }
        return true;
    }
    
    public static Boolean Test()
    {
    	Random rnd = new Random();
    	int	size;
    	
    	for(int i=0; i<NUMBER_OF_PASSES; i++)
    	{
    		size = rnd.Next(MAX_LENGTH);
    		Byte[] arr = new Byte[size];
    		rnd.NextBytes(arr);
    		
    		if (!TryThis(arr)) return false;   	
    	}
    	
    	return true;    	
    }

    public static Boolean TryThis(Byte[] Data)
    {
    	Console.WriteLine("--------------------------");
       	Console.WriteLine("Original length : "  + Data.Length);
    	
        MemoryStream ms = new MemoryStream();
        CryptoStream cs = new CryptoStream(ms, new ToBase64Transform(), CryptoStreamMode.Write);
		cs.Write(Data, 0, Data.Length);
		cs.Close();
		
    	Console.WriteLine("Encoded length : " + ms.ToArray().Length);

        MemoryStream ms1 = new MemoryStream();
        CryptoStream cs1 = new CryptoStream(ms1, new FromBase64Transform(), CryptoStreamMode.Write);
		cs1.Write(ms.ToArray(), 0, (int)ms.ToArray().Length);
		cs1.Close();
    		
    	Console.WriteLine("Decoded length : " + ms1.ToArray().Length);

    	if (!Compare(Data, ms1.ToArray())) return false;
 
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
