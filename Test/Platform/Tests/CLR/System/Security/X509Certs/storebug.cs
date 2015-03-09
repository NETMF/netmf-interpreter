using System;
using Microsoft.Win32 ; 
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography ;
using System.Collections ; 
public class Test
{
	static string storeName = "Bug" ; 
	static string userCert = "exportable.pfx" ; 
	static string userPassword = "exportable" ; 
	static string machineCert = "foo.pfx" ; 
	static string machinePassword = "foo" ; 
	static StoreLocation StoreLocation = StoreLocation.LocalMachine ; 
	public static int Main(string[] args)
		{
		AddCertificatesToStore( userCert , userPassword , StoreLocation.CurrentUser ) ; 
		AddCertificatesToStore( machineCert , machinePassword , StoreLocation.LocalMachine ) ; 
		X509Certificate2Collection userCerts = Print( StoreLocation.CurrentUser ) ; 
		X509Certificate2Collection machineCerts = Print( StoreLocation.LocalMachine ) ; 
		bool res = Compare( StoreLocation.CurrentUser , userCerts ) ; 
		bool machineRes = Compare( StoreLocation.LocalMachine , machineCerts ) ; 
		res = res && machineRes ; 

		//Clean up now
		RemoveCertificatesFromStore( userCert , userPassword , StoreLocation.CurrentUser ) ; 
		RemoveCertificatesFromStore( machineCert , machinePassword , StoreLocation.LocalMachine ) ; 

		storeName = "NewlyCreatedStore" ; 
		AddCertificatesToStore( machineCert , machinePassword , StoreLocation.LocalMachine ) ; 
		try
			{
			AddCertificatesToStore( userCert , userPassword , StoreLocation.CurrentUser ) ; 			
			}
		catch( Exception e ) //bug caught here
			{
			Console.WriteLine( e.ToString() ) ; 
			res = false ; 
			}

		try
			{
			//for sanity let's just do the same Compare ops as before
			userCerts = Print( StoreLocation.CurrentUser ) ; 
			machineCerts = Print( StoreLocation.LocalMachine ) ; 
			res = Compare( StoreLocation.CurrentUser , userCerts ) && res ; 
			res = Compare( StoreLocation.LocalMachine , machineCerts ) && res ; 
			}
		catch( Exception e )
			{
			Console.WriteLine( e.ToString() ) ; 
			res = false ; 
			}

		try
			{
			//Clean up now for the last time
			RemoveCertificatesFromStore( userCert , userPassword , StoreLocation.CurrentUser ) ; 
			RemoveCertificatesFromStore( machineCert , machinePassword , StoreLocation.LocalMachine ) ; 
			}
		catch( Exception e )
			{
			Console.WriteLine( e.ToString() ) ; 
			res = false ; 
			}

		Console.WriteLine( res ? "Test Passed" : "Test Failed" ) ; 
		return res ? 100 : 101 ; 
		}
	static void RemoveCertificatesFromStore(string cert , string password , StoreLocation loc)
		{
		//Import the pfx certificates
		X509Certificate2Collection certificates = new X509Certificate2Collection() ; 
		certificates.Import( cert , password , X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
		
		//Add the Certificate
		X509Store store = new X509Store( storeName , loc) ; // , "Cool Store" ) ; 
		store.Open( OpenFlags.ReadWrite ) ;
		store.RemoveRange( certificates ) ; 			
		store.Close() ; 
		}
	static void AddCertificatesToStore(string cert , string password , StoreLocation loc)
		{
		//Import the pfx certificates
		X509Certificate2Collection certificates = new X509Certificate2Collection() ; 
		certificates.Import( cert , password , X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
		
		//Add the Certificate
		X509Store store = new X509Store( storeName , loc) ; // , "Cool Store" ) ; 
		store.Open( OpenFlags.ReadWrite ) ;
		store.AddRange( certificates ) ; 			
		store.Close() ; 
		}
	static X509Certificate2Collection Print(StoreLocation loc)
		{
		Console.WriteLine( String.Empty ) ; 
		Console.WriteLine( "Certificates returned from: " + loc.ToString() + "\\" + storeName ) ; 
		X509Store store = new X509Store( storeName , loc) ; 
		store.Open( OpenFlags.ReadOnly ) ;		
		X509Certificate2Collection certs = store.Certificates ; 
		foreach( X509Certificate2 cert in certs ) 
			{
			Console.WriteLine( cert.Thumbprint ) ; 
			}
		store.Close() ; 		
		return certs ; 
		}
	static bool Compare( StoreLocation loc , X509Certificate2Collection certs ) 
		{
		bool res = true ; 
		string regPath = "Software\\Microsoft\\SystemCertificates\\" + storeName + "\\Certificates" ; 
		Console.WriteLine( String.Empty ) ; 
		Console.WriteLine( "Certificates found in: " + loc.ToString() + "\\" + regPath ) ; 
		RegistryKey keyLocation = loc == StoreLocation.CurrentUser ? Registry.CurrentUser : Registry.LocalMachine ; 
		RegistryKey rk = keyLocation.OpenSubKey( regPath , true ) ; 
		string[] subKeys = rk.GetSubKeyNames() ; 

		for( int i = 0 ; i < subKeys.Length ; i++ ) 
			{
			Console.WriteLine( subKeys[i] ) ; 
			
			//Now make sure each subkey name exists in certs
			if( certs.Find( X509FindType.FindByThumbprint , subKeys[i] , false ).Count == 0 )
				res = false ; 
			else
				res &= true ; 
			}
		rk.Close() ; 
		return res ; 
		}
}

