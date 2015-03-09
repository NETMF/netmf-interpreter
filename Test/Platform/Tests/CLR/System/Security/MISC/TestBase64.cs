using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

public class encbase64 {

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

    public static Boolean Test() {
    	String Text = "This is some test text";
    	
    	Console.WriteLine("Original text : "  + Text);
    	
        MemoryStream ms = new MemoryStream();
        CryptoStream cs = new CryptoStream(ms, new ToBase64Transform(), CryptoStreamMode.Write);
		cs.Write(Encoding.ASCII.GetBytes(Text), 0, Text.Length);
		cs.Close();
		
    	Console.WriteLine("Encoded : " + Encoding.ASCII.GetString(ms.ToArray()));

        MemoryStream ms1 = new MemoryStream();
        CryptoStream cs1 = new CryptoStream(ms1, new FromBase64Transform(), CryptoStreamMode.Write);
		cs1.Write(ms.ToArray(), 0, (int)ms.ToArray().Length);
		cs1.Close();
    		
    	Console.WriteLine("Decoded : " + Encoding.ASCII.GetString(ms1.ToArray()));

    	String mod = Encoding.ASCII.GetString((Byte[])ms.ToArray().Clone());
    	mod = mod.Insert(17, "\n").Insert(4, "  ").Insert(8,"\t");
    	Byte[] modified = Encoding.ASCII.GetBytes(mod);
    	
        MemoryStream ms2 = new MemoryStream();
        CryptoStream cs2 = new CryptoStream(ms2, new FromBase64Transform(), CryptoStreamMode.Write);
		cs2.Write(modified, 0, (int)modified.Length);
		cs2.Close();

    	Console.WriteLine("Decoded (with whitespaces) : " + Encoding.ASCII.GetString(ms2.ToArray()));
    	
    	if (!Compare(ms1.ToArray(), ms2.ToArray())) return false;
 
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
