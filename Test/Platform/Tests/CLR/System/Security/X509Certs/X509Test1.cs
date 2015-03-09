// Ctor : Create a X509Certificate2 object from a certificate file,
// a BASE64 certificate file, a PFX/PKCS12 (make sure the 1st cert is selected) file
// and a PKCS7 signed file (make sure the signer's certificate is selected).
//
// Test Plan test cases covered: 1, 2, 4, 6, 8(partially), 9, 10, 12(partially, other part covered by Test4)

using System;
using System.Security.Cryptography.X509Certificates;

public class X509Test1
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
		X509Certificate2 cert;
		if (ci.Password != null)
			cert = new X509Certificate2(ci.FileName, ci.Password);
		else
			cert = new X509Certificate2(ci.FileName);
	
		if (!ci.Matches(cert)) bRes = false;

//		Console.WriteLine("ToString: " + cert.ToString());
//		Console.WriteLine("ToString(true): " + cert.ToString(true));

		X509Certificate2 certImp = new X509Certificate2();
		if (ci.Password != null)
			certImp.Import(ci.FileName, ci.Password, X509KeyStorageFlags.DefaultKeySet);
		else
			certImp.Import(ci.FileName);
		
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
