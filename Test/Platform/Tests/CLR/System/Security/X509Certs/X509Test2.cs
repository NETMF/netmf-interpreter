//
//	This case tests the FriendlyName property
//		Test Plan test cases covered: 7

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

		string name = cert.FriendlyName;		

		if (ci.Encoding == "PKCS12")
		{
			if (name != "My Certificate")
			{
				Console.WriteLine("BAD: FriendlyName is not default : \"" + name + "\"");
				bRes = false;
			}
		}
		else 
		{
			if (name != String.Empty)
			{
				Console.WriteLine("BAD: FriendlyName is not empty for a file loaded cert.");
				bRes = false;
			}
		}

		// now change the FriendlyName and then read it back and verify

		cert.FriendlyName = "My Very Nice And Very Friendly Name";

		name = cert.FriendlyName;

		if (name != "My Very Nice And Very Friendly Name")
		{
			Console.WriteLine("BAD: failed to set FriendlyName!");
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

