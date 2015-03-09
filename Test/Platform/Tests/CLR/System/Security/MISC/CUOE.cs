using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;

public class CUOE {

public static void Main() {
	
	Boolean bRes = true;
	
	try {
		throw new CryptographicUnexpectedOperationException();
		bRes = false;
	}
	catch(Exception e) {
		Console.WriteLine("EXCEPTION : \n" + e.ToString());
	}

	try {
		throw new CryptographicUnexpectedOperationException("This is a message");
		bRes = false;
	}
	catch(Exception e) {
		Console.WriteLine("EXCEPTION : \n" + e.ToString());
	}

	try {
		throw new CryptographicUnexpectedOperationException("This is a format. ({0})", "This is an insert");
		bRes = false;
	}
	catch(Exception e) {
		Console.WriteLine("EXCEPTION : \n" + e.ToString());
	}

	try {
		throw new CryptographicUnexpectedOperationException("This one contains inner exception", new Exception("Inner exception") );
		bRes = false;
	}
	catch(Exception e) {
		Console.WriteLine("EXCEPTION : \n" + e.ToString());
	}

	try {
		throw new CryptographicUnexpectedOperationException();
		bRes = false;
	}
	catch(Exception e) {
		Console.WriteLine("EXCEPTION : \n" + e.ToString());
	}

        if (bRes) {
  	    Console.WriteLine("PASSED");
            Environment.ExitCode = 100;
        } else {
            Console.WriteLine("FAILED");
            Environment.ExitCode = 101;
        }
	

}

}