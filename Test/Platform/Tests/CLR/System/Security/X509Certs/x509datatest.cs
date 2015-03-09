using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography ;
using System.Security.Cryptography.Xml ;
using System.Security.Permissions ; 
using System.Security ; 

public class TestX509
{
	public static KeyInfoX509Data data  ;
	static bool m_rv = true ; 
	public static X509Certificate2 cert = null ; 
	static X509Certificate2 TestCert 
		{
		get
			{
			return new X509Certificate2( ".\\x509tests\\test1\\Trust Anchor CP.01.01.crt" ) ;  
			}
		}
	static X509Certificate2 EndCert
		{
		get
			{
			return new X509Certificate2( ".\\x509tests\\test1\\End Certificate CP.01.01.crt" ) ;
			}
		}
	public static bool rv 
		{
		get
			{
			return m_rv ; 
			}
		set
			{
			m_rv = value && m_rv ; 
			}
		}
	public static int Main(string[] args)
		{
		
		X509Certificate2 cert = null ; 
		X509Store store = null ; 
		ArrayList al = new ArrayList() ; 
		try
			{
			cert = TestCert ;		
			store = new X509Store( StoreName.My , StoreLocation.CurrentUser ) ; 
			store.Open( OpenFlags.ReadWrite ) ; 

			store.Add( cert ) ; 

			Test( X509IncludeOption.ExcludeRoot ) ; 
			Test( X509IncludeOption.WholeChain ) ; 
			Test( X509IncludeOption.EndCertOnly ) ; 
			Test( (X509IncludeOption) 0xFFFF ) ; 
			Test2() ; 
			Test3() ; 
			Test4() ; 
			Test5() ; 
			Test6() ; 
			Test7() ;
						
			store.Remove( cert ) ; 
			}
		catch( Exception e )
			{
			rv = false ; 
			Console.WriteLine( e.ToString() ) ; 
			}
		finally
			{
			store.Close() ; 
			}
		Console.WriteLine( rv ? "Test passed" : "Test failed" ) ; 
		return rv ? 100 : 101 ; 
		}
	public static void Test(X509IncludeOption include)
		{
		cert = EndCert ;
		X509Chain chain = new X509Chain() ; 
		chain.Build( cert ) ; 

		X509ChainElementCollection lmnts = chain.ChainElements ; 
		
		KeyInfoX509Data data = new KeyInfoX509Data( cert, include )  ; 	
		ArrayList al = data.Certificates ; 
		if( al == null ) return ; 
		for( int i = 0 ; i < al.Count ; i++ ) 
			{
			rv = lmnts[i].Certificate.ToString(true) == ((X509Certificate) al[i]).ToString(true) ;
			if( !rv ) 		
				Console.WriteLine( "i  = " + i.ToString() + " and include=" + include.ToString() ) ; 
			}
		Console.WriteLine( "*************************************************************" ) ; 
		}
	public static void Test2()
		{
		try
			{
			data = new KeyInfoX509Data( new X509Certificate2() , X509IncludeOption.WholeChain ) ;
			rv = false ; 
			}
		catch( Exception e ) 
			{
			Console.WriteLine( e.ToString() ) ; 
			rv = true ; 
			}
		}
	public static void Test3()
		{
		cert = TestCert ; 
		data = new KeyInfoX509Data( cert.Export( X509ContentType.Cert ) ) ;
		rv = ((X509Certificate2)data.Certificates[0]).ToString(true) == cert.ToString(true) ; 

		data = new KeyInfoX509Data( cert ) ; 
		rv = ((X509Certificate2)data.Certificates[0]).ToString(true) == cert.ToString(true) ; 
		}
	public static void Test4()
		{
		data = new KeyInfoX509Data() ; 
		data.AddSubjectKeyId( "SKI0" ) ; 
		data.AddSubjectKeyId( "SKI1" ) ; 
		rv = data.SubjectKeyIds.Count == 2 ; 

		data = new KeyInfoX509Data() ; 
		data.AddSubjectKeyId( new byte[]{1,2,3,4,5,6} ) ; 
		data.AddSubjectKeyId( new byte[]{7,8,9,10,11,12} ) ; 
		rv = data.SubjectKeyIds.Count == 2 ; 		
		}
	static void Test5()
		{
		data = new KeyInfoX509Data() ; 

		//add issuer serials
		data.AddIssuerSerial( TestCert.IssuerName.Name , TestCert.SerialNumber ) ; 
		data.AddIssuerSerial( EndCert.IssuerName.Name , EndCert.SerialNumber ) ; 
		rv = data.IssuerSerials.Count == 2; 

		byte[] b = { 100, 101 , 102 , 104 } ; 
		data = new KeyInfoX509Data() ; 
		data.CRL = b ;
		for( int i = 0 ; i < b.Length ; i++ ) 
			{
			rv = b[i] == data.CRL[i] ; 
			}
		}
	static void Test6() //Xml roundtrip
		{
		int i = 0 ; 
		data = new KeyInfoX509Data() ; 

		//add certs
		data.AddCertificate( TestCert ) ; 
		data.AddCertificate( EndCert ) ; 

		//add subject name
		data.AddSubjectName( TestCert.SubjectName.Name ) ; 
		data.AddSubjectName( EndCert.SubjectName.Name ) ; 
		
		//add subject keys
		data.AddSubjectKeyId( new byte[]{1,2,3,4,5,6} ) ; 
		data.AddSubjectKeyId( new byte[]{7,8,9,10,11,12} ) ; 

		//add issuer serials
		data.AddIssuerSerial( TestCert.IssuerName.Name , TestCert.SerialNumber ) ; 
		data.AddIssuerSerial( EndCert.IssuerName.Name , EndCert.SerialNumber ) ; 

		//add the crl
		byte[] b = { 100, 101 , 102 , 104 } ; 
		data.CRL = b ;

		KeyInfoX509Data rt = new KeyInfoX509Data() ; 
		rt.LoadXml( data.GetXml() ) ; 
		for( i = 0 ; i < rt.CRL.Length ; i++ ) 
			{
			rv = rt.CRL[i] == data.CRL[i] ; 
			}

		for( i = 0 ; i < rt.Certificates.Count ; i++ ) 
			{
			rv = rt.Certificates[i].ToString() == data.Certificates[i].ToString() ; 
			}
		for( i = 0 ; i < rt.SubjectKeyIds.Count ; i++ ) 
			{
			rv = rt.SubjectKeyIds[i].ToString() == data.SubjectKeyIds[i].ToString() ; 
			}
		for( i = 0 ; i < rt.SubjectNames.Count ; i++ ) 
			{
			rv = rt.SubjectNames[i].ToString() == data.SubjectNames[i].ToString() ;
			}
		}
	static void Test7() //negative LoadXml test
		{
		try
			{
			data = new KeyInfoX509Data() ; 
			data.LoadXml( data.GetXml() ) ; 
			rv = false ; 
			}
		catch( CryptographicException ce )
			{
			Console.WriteLine( ce.ToString() ) ; 
			rv = true ; 
			}
		catch( Exception e )
			{
			Console.WriteLine( e.ToString() ) ; 
			rv = false ; 
			}

		try
			{
			data = new KeyInfoX509Data() ; 
			data.LoadXml( null ) ; 
			rv = false ; 
			}
		catch
			{
			rv = true ; 
			}			
		}	
}

