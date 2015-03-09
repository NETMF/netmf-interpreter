//
//	This case tests the Archived property
//		Test Plan test cases covered: 5(partially)

using System;
using System.Security.Cryptography.X509Certificates;

public class X509Test2
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

		// verify that the Arvived is False
		if (cert.Archived != false)
		{
			Console.WriteLine("BAD: Archived property is not false!");
			bRes = false;
		}

		// now set the archived property to true
		cert.Archived = true;

		// this should cover two code paths - PKCS12 certificates are loaded as a store handle vs. DER certs

		// verify that it reads true

		if (cert.Archived != true)
		{
			Console.WriteLine("BAD: Archived property is not true!");
			bRes = false;
		}

		// set it to false again and verify
		cert.Archived = false;

		if (cert.Archived != false)
		{
			Console.WriteLine("BAD: Archived property is not false (2)!");
			bRes = false;
		}
		
	}
	catch(Exception e)
	{
		bRes = false;
		Console.WriteLine("Exception is caught:" + Environment.NewLine + e.ToString());
	}

	return bRes;
}


public static void Main(string[] args)
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

}

}


