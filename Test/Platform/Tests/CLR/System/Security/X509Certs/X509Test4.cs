// Ctor : Create a X509Certificate2 object from a byte array,
// a BASE64 certificate file, a PFX/PKCS12 (make sure the 1st cert is selected) file
// and a PKCS7 signed file (make sure the signer's certificate is selected).
//
// Test Plan test cases covered: 1, 12(partially), 13

using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

public class X509Test4
{

public static bool Test(string[] args)
{
	bool bRes = true;
	
	// figure out the data file name
	string filename = "Certificates.xml";
	if (args.Length != 0) filename = args[0];

	// create an xml dirver object
	XmlDriver xd = new XmlDriver(filename);

	CertificateInfo[] certs = xd.Certificates;
	foreach(CertificateInfo ci in certs)
	{
		Console.WriteLine("* Working with " + ci.FileName + " (encoding = " + ci.Encoding + ") *" );
		if (!TestCert(ci)) {
			Console.WriteLine("ERROR");
			bRes = false;
		} else {
			Console.WriteLine("OK");
		}
	}
	return bRes;
}


public static bool TestCert(CertificateInfo ci)
{
	bool bRes = true;

	try {
		// read a certificate file to a byte array
		FileStream fs = new FileStream(ci.FileName, FileMode.Open, FileAccess.Read);
		byte[] certBytes = new byte[(int)fs.Length];
		fs.Read(certBytes, 0, certBytes.Length);
		
		X509Certificate2 cert;
		
		if (ci.Password != null)
			cert = new X509Certificate2(certBytes, ci.Password);
		else
			cert = new X509Certificate2(certBytes);
	
		if (!ci.Matches(cert)) bRes = false;

//		Console.WriteLine("ToString: " + cert.ToString());
//		Console.WriteLine("ToString(true): " + cert.ToString(true));

		X509Certificate2 certImp = new X509Certificate2();

		if (ci.Password != null)
			certImp.Import(certBytes, ci.Password, X509KeyStorageFlags.DefaultKeySet);
		else
			certImp.Import(certBytes);

		if (!ci.Matches(certImp)) bRes = false;
	
	}
	catch(Exception e)
	{
		bRes = false;
		Console.WriteLine("Exception is caught:" + Environment.NewLine + e.ToString());
	}

	return bRes;
}


public static int Main(string[] args)
{
	try {
		   
		   if (Test(args)) {
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
	   return Environment.ExitCode ; 
}

}

