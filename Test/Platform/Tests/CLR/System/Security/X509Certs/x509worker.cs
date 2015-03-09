using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography ;
using System.Reflection ; 
using System.Security.AccessControl ;
using System.Security.Permissions ; 
using System.Security ; 
using System.Net ;
using System.IO ; 
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Collections ; 
using System.Threading ; 
using System.Diagnostics ; 
using System.Text ; 
using SCTUtils ; 
using System.Collections.Generic ; 
namespace X509Tester
{
	public class Worker
	{
		bool result ; 
		public string failures ; 
		public int tcCount , failCount ; 
		protected bool rv ;
		protected X509Certificate2 cert , certImp ;
		public bool Debug , clean ; 
		public string[] certs ;
		public X509ContentType[] types ; 
		public string storeName = "MyCoolStore" ;
		public string notSoCoolStore = "NotSoCoolStore" ; 
		public string aCoolStore = "ACoolStore" ; 
		public string storeName43 = "MyCoolBuildStore" ;
		public string lmStoreName43 = "MyCoolLocalMachineStore" ;
		string filename = "Certificates.xml";

		X509Store store = null ; 
		bool memLeakChk ;
		public Worker() : this(false,true,false)
			{
			}
		public void DumpResults(bool failed)
			{
			
			if(  failCount == 0 && !failed )
			{
				Console.WriteLine( "PASSED" )  ;
				Environment.ExitCode = 100 ; 
			}
			else
			{
				Console.WriteLine( failures ) ; 
				Environment.ExitCode = 101 ; 
			}
			Console.WriteLine( "Total Tests:  " + (tcCount ).ToString() ) ; 
			Console.WriteLine( "Passing Tests:  " + (tcCount-failCount).ToString() ) ; 
			Console.WriteLine( "Failing Tests:  " + failCount.ToString() ) ; 
			}
		public Worker(bool Debug, bool clean , bool memLeakChk )
		{
			this.clean = clean ;
			result = true ; 
			failures = "" ; 
			tcCount = 0 ; 
			
			failCount = 0 ; 
			rv = true ; 
			cert = null ;
			this.Debug = Debug ; 
			certs = new string[5] ; 
			certs[0] = "x509.cer" ; 
			certs[1] = ".\\x509tests\\test9\\test9.p12" ; 
			certs[2] = "tariks.sst" ; 
			certs[3] = "tariks.p7b" ; 
			certs[4] = "xenroll.dll" ; 
			types = new X509ContentType[5] ; 
			types[0] = (X509ContentType)0x01 ; 
			types[1] = (X509ContentType)0x03 ; 
			types[2] = (X509ContentType)0x04 ; 
			types[3] = (X509ContentType)0x05 ; 
			types[4] = (X509ContentType)0x06 ; 
			this.memLeakChk = memLeakChk ; 
		}
		[DllImport("Kernel32.dll")]
		public static extern int GetLastError();
		string provider = "Gemplus GemSAFE Card CSP v1.0" ; 
		
		[DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		static extern bool CryptReleaseContext( IntPtr hProv , uint dwFlags ) ; 

		[DllImport("WinScard.dll")]
		public static extern int SCardEstablishContext(uint dwScope, IntPtr pvReserved1 , 
			IntPtr pvReserved2, ref IntPtr phContext);

		[DllImport("WinScard.dll")]
		public static extern int SCardReleaseContext(IntPtr phContext);
		[DllImport("WinScard.dll")]
		public static extern int SCardListReaderGroups(IntPtr hContext, StringBuilder cGroups, ref int nStringSize);
		[DllImport("WinScard.dll")]
		public static extern int SCardListReaders(IntPtr hContext, string cGroups,	ref string cReaderLists, ref int nReaderCount);
		[DllImport("WinScard.dll")]
		public static extern int SCardFreeMemory(IntPtr hContext, ref string cResourceToFree);
		[DllImport("WinScard.dll")]
		public static extern int SCardConnect(IntPtr hContext, string cReaderName, uint dwShareMode, uint dwPrefProtocol, ref IntPtr phCard, ref uint ActiveProtocol);
		[DllImport("WinScard.dll")]
		public static extern int SCardDisconnect(IntPtr hCard, int Disposition);
		
		void SmartCardSetup()
			{
			IntPtr NULL = IntPtr.Zero ; 
			IntPtr phContext = IntPtr.Zero ; 
			SCardEstablishContext( 2 , NULL , NULL , ref phContext ) ; 

			//used to split null delimited strings into string arrays
			char[] delimiter = new char[1];
			delimiter[0] = Convert.ToChar(0);

			//Second step in using smart cards is SCardListReaderGroups() to determin reader to use
			int nStringSize = 50000;	//SCARD_AUTOALLOCATE
			StringBuilder cGroupList = new StringBuilder(nStringSize) ; 
			int nRetVal2 = SCardListReaderGroups(phContext, cGroupList, ref nStringSize);
			Console.WriteLine( nRetVal2 ) ; 

			string[] cGroups = cGroupList.ToString().Split(delimiter);

			string cReaderList = "" + Convert.ToChar(0) ; 
			int buffer = -1; 
			SCardListReaders(phContext, cGroups[0], ref cReaderList, ref buffer);
			
			string[] cReaders = cReaderList.ToString().Split(delimiter);

			uint nShareMode      = 1;			//exclusive
			uint nPrefProtocol   = 0x80000000;	//default PTS
			IntPtr hCard          = NULL ;
			uint nActiveProtocol = 0;

			Console.WriteLine( SCardConnect( phContext , cReaders[0] , nShareMode , nPrefProtocol , ref hCard , ref nActiveProtocol ) ) ; 
			Console.WriteLine( SCardDisconnect( hCard , 0 ) ); 

			Console.WriteLine( SCardFreeMemory( phContext , ref cReaderList ) ); 
			Console.WriteLine( SCardReleaseContext( phContext ) ); 
			}
		public unsafe void Test300()
			{
			SmartCardSetup() ; 
		
			IntPtr hProvider = IntPtr.Zero ;
			Console.WriteLine(
				CryptAcquireContext( &hProvider, null , provider , 1 , 0xF0000000 ) 
				) ; 
			
			Console.WriteLine(
				CryptReleaseContext( hProvider , 0 ) ) ; 
			/*
			CspParameters csp = new CspParameters( PROV_RSA_FULL ,provider) ; // "Infineon SICRYPT Base Smart Card CSP"  ) ; 
//			csp.KeyPassword = String2SecureString("97319") ; 
			csp.ParentWindowHandle = new IntPtr(0x40100) ;
			CspKeyContainerInfo info = new CspKeyContainerInfo(csp) ; 
		//	CryptoKeySecurity cks = info.CryptoKeySecurity ;
//			Console.WriteLine( info.UniqueKeyContainerName ) ; 
			Console.WriteLine( info.HardwareDevice ) ; 
			Console.WriteLine( info.Accessible ) ; */
			}
		#region Security Push test cases
		//Verify that OpenStore and EnumerateCertificates are called from X509Chain.Build
		public void Test202()
			{
			StorePermission sp = new StorePermission(StorePermissionFlags.OpenStore | StorePermissionFlags.EnumerateCertificates);
			sp.Deny() ;			
			Build("Verify that OpenStore and EnumerateCertificates are called from X509Chain.Build","dns.cer") ; 
			}
		//Verify that WebPermission gets demanded in X509Chain.Build if RevocationMode is Online
		public void Test201()
			{
			( new WebPermission( PermissionState.Unrestricted ) ).Deny() ; 
			Build("Verify that WebPermission gets demanded in X509Chain.Build if RevocationMode is Online","dns.cer") ; 
			}
		void Build(string test,string certPath)
			{			
			try
				{				
				X509Chain chain = new X509Chain() ; 
				chain.ChainPolicy.RevocationMode = X509RevocationMode.Online ; 
				rv = false ; 
				chain.Build( new X509Certificate2( certPath ) ) ; 
				}
			catch(SecurityException se)
				{
				rv = true ; 
				Console.WriteLine( se ) ;
				}
			catch(Exception e)
				{
				Console.WriteLine(e) ; 
				}
			finally
				{
				Eval( rv , test);
				}
			}
		public delegate void Import2<T>( T cert , string password , X509KeyStorageFlags flags ) ; 
		//Verify that KeyContainerPermission.Create is called if Import is called w/ PersistKeySet
		public void Test200()
			{
			FileStream fs = new FileStream(certs[1], FileMode.Open, FileAccess.Read);
			byte[] certBytes = new byte[(int)fs.Length];
			fs.Read(certBytes, 0, certBytes.Length);
			fs.Close() ; 
			cert = new X509Certificate2() ; 
			RunTest200<byte[]>( new Import2<byte[]>( cert.Import ) , certBytes, "Byte array import" ); 
			RunTest200<string>( new Import2<string>( cert.Import ) , certs[1] , "Byte array import" ); 
			}
		public void RunTest200<T>(Import2<T> Import , T cert , string Test)
			{
			( new KeyContainerPermission( KeyContainerPermissionFlags.Create ) ).Deny() ; 
			try
				{				
				rv = false ; 
				Import(cert,"password",X509KeyStorageFlags.PersistKeySet) ;
				}
			catch(SecurityException)
				{
				rv = true ; 
				}
			catch(Exception e)
				{
				Console.WriteLine(e) ; 
				}
			finally
				{
				Eval( rv , Test + ":  Verify that KeyContainerPermission.Create is called if Import is called w/ PersistKeySet" ) ; 
				}
			}
		#endregion
		#region Beta2 X509 DCR
		/*
		X509SubjectKeyIdentifierExtension(PublicKey key, Boolean critical) 
		X509SubjectKeyIdentifierExtension(PublicKey key, X509SubjectKeyIdentifierHashAlgorithm alg, Boolean critical) 
		 

		New X509SubjectKeyIdentifierHashAlgorithm enum 
			CapiSha1 – CAPI style SKI SHA1 hash over the full subject public key ASN.1 blob including tag, length, etc. 
			Sha1 – Defined in 3280. SHA1 hash of the value of the BIT STRING subjectPublicKey (excluding the tag, length, and number of unused bits).
			ShortSha1 – Defined in RFC3280 – 64 bits value constructed from 0100 + lower 60 bits of Sha1 hash in #2 		
		*/
		public void Test105()
			{
			/*
			1.  Constructor works
			2.  Encoding round trip works
			3.  CopyFrom works
			*/
			cert = new X509Certificate2( "dss.cer" ) ; 
			X509SubjectKeyIdentifierExtension ext = new X509SubjectKeyIdentifierExtension( cert.PublicKey , true ) ; 
			Eval( ext != null , "X509SubjectKeyIdentifierExtension ext = new X509SubjectKeyIdentifierExtension( cert.PublicKey )" ) ; 

			string s = ext.SubjectKeyIdentifier ; 
			X509SubjectKeyIdentifierExtension asn = new X509SubjectKeyIdentifierExtension( s , true ) ; 
			ext = new X509SubjectKeyIdentifierExtension() ;
			ext.CopyFrom( asn ) ; 
			Eval( asn.SubjectKeyIdentifier == ext.SubjectKeyIdentifier && s == asn.SubjectKeyIdentifier, String.Format( "{0}=={1}" , s , ext.SubjectKeyIdentifier ) ) ; 
			}
		public void Test106a(X509SubjectKeyIdentifierHashAlgorithm alg)
			{
			cert = new X509Certificate2( "dss.cer" ) ; 
			X509SubjectKeyIdentifierExtension ext = new X509SubjectKeyIdentifierExtension( cert.PublicKey , alg , true ) ; 
			Eval( ext != null , "X509SubjectKeyIdentifierExtension ext = new X509SubjectKeyIdentifierExtension( cert.PublicKey , alg )" ) ; 


			string s = ext.SubjectKeyIdentifier ; 
			X509SubjectKeyIdentifierExtension asn = new X509SubjectKeyIdentifierExtension( s , true ) ; 
			ext = new X509SubjectKeyIdentifierExtension() ;
			ext.CopyFrom( asn ) ; 
			Eval( asn.SubjectKeyIdentifier == ext.SubjectKeyIdentifier && s == asn.SubjectKeyIdentifier , String.Format( "{0}=={1}" , s , ext.SubjectKeyIdentifier ) ) ; 
			}
		public void Test106()
			{
			Test106a( X509SubjectKeyIdentifierHashAlgorithm.CapiSha1 ) ; 
			Test106a( X509SubjectKeyIdentifierHashAlgorithm.Sha1 ) ; 
			Test106a( X509SubjectKeyIdentifierHashAlgorithm.ShortSha1 ) ; 
			
			//Bug 357499
			X509Certificate2 cert = new X509Certificate2( "dss.Cer" ) ; 
			Console.WriteLine( cert.GetKeyAlgorithm() ) ; 
			Eval( cert.GetKeyAlgorithm() == "1.2.840.10040.4.1" , "VSWHIDBEY 357499:  X509Certificate.GetKeyAlgorithm() returns friendly name instead of OID." ) ; 
			}
		/*
		Change m_critical property on the X509Extension class from get to get/set so user does not have to re-create a new object to set the m_critical flag.

		1.  Set Critical = true, get returns true
		2.  Set Critical = false, get returns false
		3.  Set Critical = true|false, CopyFrom produces a copy with the same critical value
		*/
		public void Test100()
			{
			TestExtensionCritical( TestX509KeyUsageExtension , TestX509KeyUsageExtension ) ; 				
			TestExtensionCritical( TestX509BasicConstraintsExtension , TestX509BasicConstraintsExtension ) ; 
			TestExtensionCritical( TestX509EnhancedKeyUsageExtension , TestX509EnhancedKeyUsageExtension ) ; 
			TestExtensionCritical( TestX509SubjectKeyIdentifierExtension , TestX509SubjectKeyIdentifierExtension ) ; 
			TestExtensionCritical( new X509ExtensionTest() , new X509ExtensionTest() ) ; 
			}
		void TestExtensionCritical( X509Extension ext , X509Extension copy )
			{
			try
				{
				ext.Critical = true ; 
				Eval( ext.Critical , "Set Critical = true, get returns true" ) ; 

				copy.CopyFrom( ext ) ; 
				Eval( copy.Critical , "SSet Critical = true, CopyFrom produces a copy with the same critical value" ) ; 

				ext.Critical = false ; 
				Eval( !ext.Critical , "Set Critical = false, get returns false" ) ; 

				copy.CopyFrom( ext ) ; 
				Eval( !copy.Critical , "SSet Critical = false, CopyFrom produces a copy with the same critical value" ) ; 			
				}
			catch( Exception e )
				{
				Console.WriteLine( e ) ; 
				Eval( false , String.Format( "TestExtensionCritical threw an exception for {0}" , ext.ToString() ) ) ; 
				}
			}
		X509KeyUsageExtension TestX509KeyUsageExtension
			{
			get
				{
				return new X509KeyUsageExtension( X509KeyUsageFlags.DecipherOnly , false ) ; 
				}
			}
		X509BasicConstraintsExtension TestX509BasicConstraintsExtension
			{
			get
				{
				return new X509BasicConstraintsExtension( true , true , 300 , false ) ; 
				}
			}
		X509SubjectKeyIdentifierExtension TestX509SubjectKeyIdentifierExtension
			{
			get
				{
				byte[] bytes = { 0x10 , 0x20 ,  0x34 , 0x69 , 0xFF } ; 
				return new X509SubjectKeyIdentifierExtension( bytes , false ) ; 
				}
			}

		/*
		Additional methods for X509ExtensionCollection to make adding multiple certs more efficient, and remove certs from collection. 
		 

		void Remove(X509Extension extension) 
		void RemoveAll() 
		void AddRange(X509ExtensionCollection extensionCollection) 
		void AddRange(X509Extension[] extensions) 
		*/
		public void Test101()
			{
			//Not implemented in whidbey
			}
		/*
		X509Certificate2 – Change all password parameters from String to SecureStrings. 

		Test104:  X509Certificate2 (Byte[] rawData, SecureString password) 
		Test104:  X509Certificate2 (String fileName, SecureString password) 
		Test103:  X509Certificate2 (Byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags) 
		Test103:  void Import(string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags): 
		Test102:  X509Certificate2 (String fileName, SecureString password, X509KeyStorageFlags keyStorageFlags) 
		Test102:  void Import(byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags) 
		Test102:  byte[] Export(X509ContentType contentType, SecureString password); 
		*/
		public void Test104()
			{
			X509Certificate2 testCert = null ; 
			X509Certificate2 importCert = null ; 
			X509Certificate2 ctorCert = null ; 
			string path = null ; 
			for( int i = 0 ; i < certChain.Length ; i++ )
				{
				if( certChain[i] != String.Empty )
					{
					path = String.Format( "x509tests\\test{0}\\test{1}.p12"  , i.ToString() , i.ToString() ) ; 
					//get the baseline
					testCert = new X509Certificate2( path , Password ) ; 

					//get the imported cert
					importCert = new X509Certificate2(path , CertPassword ) ; 

					//get a cert w/ ctor
					ctorCert = new X509Certificate2( GetFileBytes( path ) , CertPassword ) ; 
					Eval( CertEquals( testCert , importCert ) && CertEquals( ctorCert , testCert ) , String.Format( "Test {0} and {1} for certificate {2}" , "X509Certificate2 (string filename, SecureString password) " , "X509Certificate2 (Byte[] rawData, SecureString password) " , path ) ) ; 
					}
				}
			}
		public void Test102()
			{
			for( int i = 0 ; i < certChain.Length ; i++ )
				{
					if( certChain[i] != String.Empty )
						{
						X509KeyStorageFlags flag = (X509KeyStorageFlags) 0x04 ; 
						string path = String.Format( "x509tests\\test{0}\\test{1}.p12"  , i.ToString() , i.ToString() ) ; 
						cert = new X509Certificate2( path , CertPassword , flag ) ; 
						Test014RoundTrip<SecureString>( true , cert , CertPassword , i , flag ) ; 

						flag = (X509KeyStorageFlags) 0 ; 
						cert = new X509Certificate2( path , CertPassword , flag ) ; 
						Test014RoundTrip<SecureString>( false , cert , CertPassword , i ,flag ) ; 
						}
				}
			}
		public void Test103()
			{
			X509Certificate2 testCert = null ; 
			X509Certificate2 importCert = null ; 
			X509Certificate2 ctorCert = null ; 
			X509KeyStorageFlags flag = (X509KeyStorageFlags) 0x04 ; 
			string path = null ; 
			for( int i = 0 ; i < certChain.Length ; i++ )
				{
				if( certChain[i] != String.Empty )
					{
					path = String.Format( "x509tests\\test{0}\\test{1}.p12"  , i.ToString() , i.ToString() ) ; 
					//get the baseline
					testCert = new X509Certificate2( path , Password , flag ) ; 

					//get the imported cert
					importCert = new X509Certificate2() ; 
					importCert.Import( path , CertPassword , flag ) ; 

					//get a cert w/ ctor
					ctorCert = new X509Certificate2( GetFileBytes( path ) , CertPassword , flag ) ; 
					Eval( CertEquals( testCert , importCert ) && CertEquals( ctorCert , testCert ) , String.Format( "Test {0} and {1} for certificate {2}" , "void Import(string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags)" , "X509Certificate2 (Byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags) " , path ) ) ; 
					}
				}
			}
		byte[] GetFileBytes( string file )
			{
			FileStream fs = new FileStream( file , FileMode.Open , FileAccess.Read ) ; 
			byte[] data = new byte[fs.Length] ; 
			fs.Read( data , 0 , data.Length ) ; 
			fs.Close() ; 
			return data ; 
			}
		#endregion
		#region M3.3 X509 DCR
		//Code coverage for X509Extension class
		public void Test062()
			{
			X509ExtensionTest test = new X509ExtensionTest(true) ; 
			Eval( test != null , "X509ExtensionTest test = new X509ExtensionTest() " ) ; 

			//Call into X509Extension w/ AsnEncodedData w/o oid
			X509KeyUsageExtension ku = new X509KeyUsageExtension( GetFlag(33 , true ) , true ) ;
			AsnEncodedData asn = new AsnEncodedData( ku.RawData ) ; 

			try
				{
				X509Extension ext = new X509Extension( asn,true ) ; 
				rv = false ; 
				}
			catch( ArgumentNullException )
				{
				rv = true ; 
				}
			catch
				{
				rv = false ; 
				}
			finally
				{
				Eval( rv , "Call into X509Extension w/ AsnEncodedData w/o oid" ) ; 
				}

			try
				{
				X509Extension ext = new X509Extension( String.Empty , asn.RawData , true ) ; 
				rv = false ; 
				}
			catch( ArgumentException )
				{
				rv = true ; 
				}
			catch
				{ 
				rv = false ; 
				}
			finally
				{
				Eval( rv , "X509Extension ext = new X509Extension( String.Empty , asn.RawData , true ) " ) ; 
				}
			}
		//X509ExtensionCollection constructor
		//Call into X509Extension tests
		public void Test060()
			{
			Test062() ; 
			X509ExtensionCollection exts = new X509ExtensionCollection() ; 
			Eval( exts != null , "X509ExtensionCollection exts = new X509ExtensionCollection()" ) ; 
			}
		//X509ExtensionCollection.Add
		//Add the following params:  null, custom Extension, X509SubjectKeyIdentifierExtension
		//Add to a newly created Collection
		//Add to a collection derived from a Cert
		public void Test061()
			{
			Test061(true) ;
			Test061(false) ;
			}
		void Test061(bool newExt)
			{
			//Add Null
			X509ExtensionCollection ext = GetExt(newExt) ;
			try
				{
				ext.Add(null) ;
				rv = false ; 
				}
			catch( ArgumentNullException )
				{
				rv = true ; 
				}
			catch( Exception e )
				{
				WriteLine( e ) ; 
				rv = false ; 
				}
			Eval( rv , "ext.Add(null)" ) ;

			//Add customExtension
			ext = GetExt( newExt ) ; 
			ext.Add( new X509ExtensionTest() ) ; 
			X509Extension xt = ext[X509ExtensionTest.MyOid] ; 
			Eval( xt != null , "ext.Add( new X509ExtensionTest() )" ) ; 

			//Add a built in Extension
			xt = new X509SubjectKeyIdentifierExtension() ; 
			ext.Add( xt ) ; 
			xt = ext[xt.Oid.Value] ; 
			Eval( xt != null , "ext.Add( xt )" ) ; 
			}
		X509ExtensionCollection GetExt( bool newExt )
			{
			if( newExt ) 
				return new X509ExtensionCollection() ; 
			return GetX509(0).Extensions ; 
			}
		/*AsnEncodedData( byte[] )
			Oid returns null
			RawData returns the proper value
			AsnEncodedData( AsnEncodedData ) constructor returns the same 
			CopyFrom works, also CopyFrom with an AsnEcoded data w/ Oid should reset m_oid
			Format works
			rawData is null - ArgumentNullException
			rawData is empty array - no Exception, Format returns String.Empty
		*/
		public void Test070()
			{
			byte[] rawData = new byte[0] ; 
			string str = "AsnEncodedData Test" ;
			Test070( rawData , "RawData is empty array:  " ) ; 
			rawData = System.Text.Encoding.Unicode.GetBytes( str.ToCharArray() ) ;
			Test070( rawData , "RawData is \"AsnEncodedData Test\":  " ) ; 

			try
				{
				AsnEncodedData asn = new AsnEncodedData( (byte[]) null ) ; 
				rv = false ; 
				}
			catch( ArgumentNullException )
				{
				rv = true ; 
				}
			catch( Exception )
				{
				rv = false ; 
				}
			Eval( rv , "AsnEncodedData asn = new AsnEncodedData( (byte[]) null )" ) ; 
			}
		void Test070( byte[] rawData , string description )
			{
			AsnEncodedData asn = null ; 
			AsnEncodedData asn2 = null ; 
			
			//Oid returns null
			asn = new AsnEncodedData( rawData ) ; 
			Eval( asn.Oid == null , description + "Oid returns null" ) ; 

			//RawData returns the proper value
			asn = new AsnEncodedData( rawData ) ; 
			Eval( asn.RawData.Length == rawData.Length ,description +  "RawData returns the proper value" ) ; 
			
			//AsnEncodedData( AsnEncodedData ) constructor returns the same 
			asn = new AsnEncodedData( rawData ) ; 
			asn2 = new AsnEncodedData( asn ) ; 
			Eval( asn.Oid == asn2.Oid && asn.RawData.Length == asn2.RawData.Length , description + "AsnEncodedData( AsnEncodedData ) constructor returns the same " ) ; 
			
			//CopyFrom with an AsnEcoded data w/ Oid should reset m_oid
			asn = new AsnEncodedData( rawData ) ; 
			asn2 = new AsnEncodedData( "1.2.345" , new byte[]{ 1 } ) ;
			asn2.CopyFrom( asn ) ; 
			Eval( asn2.Oid == null && asn2.RawData.Length == rawData.Length , description + "CopyFrom with an AsnEcoded data w/ Oid should reset m_oid" ) ; 

			//CopyFrom works
			asn = new AsnEncodedData( rawData ) ; 
			asn2 = new AsnEncodedData( new byte[]{ 1 } ) ;
			asn2.CopyFrom( asn ) ; 
			Eval( asn2.Oid == null && asn2.RawData.Length == rawData.Length , description + "CopyFrom with an AsnEcoded data w/o Oid works" ) ; 
			
			//Format works
			asn = new AsnEncodedData( rawData ) ; 
			string tStr = asn.Format( true ) ; 
			string fStr = asn.Format( false ) ; 			
//			Eval( tStr == asn.Format( true ) , description + "tStr == asn.Format( true )" ) ; 
//			Eval( fStr == asn2.Format( false ) , description + "fStr == asn2.Format( false )" ) ; 
			}
		//Create a KeyUsageExtension, create a AsnEncodedData from the RawData, create a new KeyUsageExtension and compare
		public void Test080()
			{
			for( int i = 0 ; i < 512 ; i++ )
				{
				X509KeyUsageExtension ku = new X509KeyUsageExtension( GetFlag( i , true ) , true ) ; 
				AsnEncodedData asn = new AsnEncodedData( (AsnEncodedData) ku ) ;
				X509KeyUsageExtension ku2 = new X509KeyUsageExtension( asn , true ) ; 
				Eval( ku.KeyUsages == ku2.KeyUsages , "Create a KeyUsageExtension, create a AsnEncodedData from the RawData, create a new KeyUsageExtension and compare:  i is " +  i.ToString() ) ; 

				//Regression for Bug when we call X509Extension::CopyFrom w/ an AsnEncodedData w/o Oid, the Oid gets overwritten with null
				ku = new X509KeyUsageExtension( GetFlag( i , true ) , true ) ; 
				asn = new AsnEncodedData( ku.RawData ) ;
				try
					{
					ku.CopyFrom( asn ) ; 
					rv = false ; 
					}
				catch( ArgumentException ) 
					{
					rv = true ; 
					}
				catch( Exception )
					{
					rv = false ; 
					}
				Eval( rv , "BUG 217597:  Regression for when we call X509Extension::CopyFrom w/ an AsnEncodedData w/o Oid, the Oid gets overwritten with null" ) ; 
				}

			}
		//Create BasicConstraintsExtension, then create a AsnEncodedData from the Extension and create new BasicConstraintsExtension
		public void Test081()
			{
			/*Test the following:
				certificateAuthority = true : false
				hasPathLengthConstraint = true : false 
				pathLengthConstraint = 0 , 100 , Integer.Max
			*/
			int[] path = { 0 , 100 , Int32.MaxValue } ;
			for( int i = 0 ; i < 2 ; i++ ) 
				{
				for( int j = 0 ; j < 2 ; j++ ) 
					{
					for( int k = 0 ; k < path.Length ; k++ ) 
						{
						try
							{
							X509BasicConstraintsExtension ext = new X509BasicConstraintsExtension( i == 0 , j == 0 , path[k] , k % 2 == 0 ) ; 
							AsnEncodedData asn = new AsnEncodedData( ext.RawData ) ; //byte Array only!!
							X509BasicConstraintsExtension extCopy = new X509BasicConstraintsExtension (asn , k % 2 == 0 ) ; 
							rv = extCopy.CertificateAuthority == ext.CertificateAuthority && 
								extCopy.HasPathLengthConstraint == ext.HasPathLengthConstraint &&
								extCopy.Critical == ext.Critical && 
								extCopy.PathLengthConstraint == ext.PathLengthConstraint ;
							}
						catch
							{
							rv = false ; 
							}
						Eval( rv , "Test X509BasicConstraintsExtension( AsnEncodedData , bool ) for { i , j , k } = { " + i.ToString() + " , " + j.ToString() + " , " + k.ToString() +  " }"  ) ; 
						}
					}
				}
			}
		//Test SubjectKeyIdentifier
		public void Test082()
			{
			/*
			X509SubjectKeyIdentifierExtension
				String->byte array, true / false
			*/
			char[] high = { (char) 0x1000 , (char) 0x2000 , (char) 0x2345 , (char) 0x6789 , (char) 0xFFFF } ; 
			char[] mid = { (char) 0x100 , (char) 0x101 , (char) 0x102 , (char) 0x103 , (char) 0x104 , (char) 0xFFF } ; 
			char[] mix = { (char) 0x65 , (char) 0x650 , (char) 0x6500 , (char) 0x66 , (char) 0x660 , (char) 0x6600 } ; 
			char[] low = { (char) 0x65 , (char) 0x66 , (char) 0x67 , (char) 0x68 , (char) 0x69 } ; 
			string s = String.Empty ; 
			for( int i = 0 ; i < 1000 ; i++ ) 
				{
				s += (char) (i % 26 + 0x65) ; 
				}
			string[] str =  { new String( high ) , 
				new String( mix ) , 
				new String( low ) , 
				new String( mid ) , 
				s } ; 
			byte[] bytes = null ; 
			for( int i = 0 ; i < str.Length ; i++ ) 
				{
				try
					{
					Console.WriteLine( str[i] ) ; 
					bytes = Encoding.Unicode.GetBytes(str[i]) ; 
					X509SubjectKeyIdentifierExtension ext = new X509SubjectKeyIdentifierExtension( bytes , i % 2 == 0 ) ; 
					AsnEncodedData asn = new AsnEncodedData( (AsnEncodedData) ext ) ;
					X509SubjectKeyIdentifierExtension extCopy = new X509SubjectKeyIdentifierExtension( asn , i % 2  == 0 ) ; 
					rv = ext.SubjectKeyIdentifier == extCopy.SubjectKeyIdentifier && ext.Critical == extCopy.Critical && ext.SubjectKeyIdentifier.Length > 0 ; 

					ext = new X509SubjectKeyIdentifierExtension( str[i] , i % 2 == 0 ) ; 
					asn = new AsnEncodedData( ext.RawData ) ;
					extCopy = new X509SubjectKeyIdentifierExtension( asn , i % 2  == 0 ) ; 
					bool rv2 = ext.SubjectKeyIdentifier == extCopy.SubjectKeyIdentifier && ext.Critical == extCopy.Critical && rv && ext.SubjectKeyIdentifier.Length > 0 ; 
					rv = rv && rv2 ; 
					}
				catch( Exception e )
					{
					WriteLine( e ) ; 
					rv = false ; 
					}
				Eval( rv , "Encoding X509SubjectKeyIdentifier(AsnEncodedData) for:  " + str[i] ) ; 
				}

			//Negative test cases for Encoding
			try
				{
				X509SubjectKeyIdentifierExtension ext = new X509SubjectKeyIdentifierExtension( (string) null , false ) ; 
				rv = false ; 
				}
			catch( ArgumentNullException )
				{
				rv = true ; 
				}
			catch
				{
				rv = false ; 
				}
			finally
				{
				Eval( rv , "X509SubjectKeyIdentifierExtension ext = new X509SubjectKeyIdentifierExtension( (string) null ) , false ) ; " ) ; 
				}
			try
				{
				X509SubjectKeyIdentifierExtension ext = new X509SubjectKeyIdentifierExtension( (byte[]) null , false ) ; 
				rv = false ; 
				}
			catch( ArgumentNullException )
				{
				rv = true ; 
				}
			catch
				{
				rv = false ; 
				}
			finally
				{
				Eval( rv , "X509SubjectKeyIdentifierExtension ext = new X509SubjectKeyIdentifierExtension( (byte[]) null ) , false ) ; " ) ; 
				}
			}
		public void Test083()
			{
			/*
			Empty oid collection
			OidCollection count = 1 
			OidCollection contains garbage OID
			OidCollection count of all key usages			
			*/
			string szOID_PKIX_KP_SERVER_AUTH      = "1.3.6.1.5.5.7.3.1";
			X509EnhancedKeyUsageExtension ext = new X509EnhancedKeyUsageExtension( new OidCollection() , true ) ; 
			Eval( Compare( Copy(ext,true) , ext ) , "Empty oid collection" ) ; 


			//OidCollection.Count == 1
			OidCollection oc = new OidCollection() ; 
			oc.Add( new Oid( szOID_PKIX_KP_SERVER_AUTH ) ) ; 
			ext = new X509EnhancedKeyUsageExtension( oc , false ) ; 
			Eval( Compare( Copy(ext) , ext ) , "OidCollection.Count == 1" ) ; 

			if( !Utility.IsWin9x )
				{
				//OidCollection contains garbage OID
				oc = new OidCollection() ; 
				oc.Add( new Oid( "garbage" ) ) ; 
				try
					{
					// Garbage OIDs cause a crypto exception to be thrown under Vista, which seems
					// reasonable, so just skip this part if it does.
					ext = new X509EnhancedKeyUsageExtension( oc , false ) ; 
					Eval( Compare( Copy(ext) , ext ) , "OidCollection contains garbage OID" ) ; 
					}
				catch (CryptographicException)
					{
					Console.WriteLine("Garbage OID threw exception (OK), skipping test");
					}

				}

			ext = TestX509EnhancedKeyUsageExtension ;
			Eval( Compare( Copy(ext) , ext ) , "OidCollection has several key usages" ) ; 
			}
		X509EnhancedKeyUsageExtension TestX509EnhancedKeyUsageExtension
			{
			get
				{
				//OidCollection has several key usages
				string szOID_PKIX_KP_SERVER_AUTH      = "1.3.6.1.5.5.7.3.1";
				string szOID_PKIX_KP_CLIENT_AUTH      = "1.3.6.1.5.5.7.3.2";
				string szOID_PKIX_KP_CODE_SIGNING     = "1.3.6.1.5.5.7.3.3";
				string szOID_PKIX_KP_EMAIL_PROTECTION = "1.3.6.1.5.5.7.3.4";
				string SPC_INDIVIDUAL_SP_KEY_PURPOSE_OBJID = "1.3.6.1.4.1.311.2.1.21";
				string SPC_COMMERCIAL_SP_KEY_PURPOSE_OBJID = "1.3.6.1.4.1.311.2.1.22";
				OidCollection oc = new OidCollection() ; 
				oc.Add( new Oid( szOID_PKIX_KP_SERVER_AUTH ) );
				oc.Add( new Oid( szOID_PKIX_KP_CLIENT_AUTH ) );
				oc.Add( new Oid( szOID_PKIX_KP_CODE_SIGNING )); 
				oc.Add( new Oid( szOID_PKIX_KP_EMAIL_PROTECTION  ) );
				oc.Add( new Oid( SPC_INDIVIDUAL_SP_KEY_PURPOSE_OBJID ) );
				oc.Add( new Oid( SPC_COMMERCIAL_SP_KEY_PURPOSE_OBJID  ) );
				return new X509EnhancedKeyUsageExtension( oc , false ) ; 
				}
			}
		X509EnhancedKeyUsageExtension Copy( X509EnhancedKeyUsageExtension source )
			{
			return Copy( source , false ) ; 
			}
		X509EnhancedKeyUsageExtension Copy( X509EnhancedKeyUsageExtension source , bool critical )
			{
			AsnEncodedData asn = new AsnEncodedData( (AsnEncodedData) source ) ;
			return new X509EnhancedKeyUsageExtension( asn , critical ) ; 
			}
		//Generate a key with the UserProtected flag (which means a popup will result from accessing the key) and NoPrompt, should get an exception
		public void Test090()
			{
			try
				{
				CspParameters csp = new CspParameters() ; 
				csp.Flags = CspProviderFlags.UseUserProtectedKey | CspProviderFlags.NoPrompt ; 
				RSACryptoServiceProvider rsa = new RSACryptoServiceProvider( csp ) ; 
				byte[] blob = rsa.ExportCspBlob(true) ; 
				rv = false ; 
				}
			catch( CryptographicException )
				{
				rv = true ; 
				}
			catch( Exception )
				{
				rv = false ; 
				}
			Eval( rv , "Generate a key with the UserProtected flag (which means a popup will result from accessing the key) and NoPrompt, should get an exception" ) ; 
			}
		#endregion
		#region M3.2 X509 additions X509: we should support encoding extensions as well as decoding from ASN.1
		//Test X509Extension encoding
		public void Test050()
			{
			TestKeyUsage() ; 
			TestBasicConstraints() ; 
			TestSubjectKey() ; 
			TestEnhanced() ; 
			}
		//Test X500Name class and Cert properties
		public void Test051()
			{

/*        
	 public X500DistinguishedName (byte[] encodedDistinguishedName) : base(new Oid(), encodedDistinguishedName) {}
        public X500DistinguishedName (AsnEncodedData encodedDistinguishedName) : base(encodedDistinguishedName) {}
        public X500DistinguishedName (X500DistinguishedName distinguishedName) : base((AsnEncodedData) distinguishedName) {
        public X500DistinguishedName (string distinguishedName) : this(distinguishedName, X500DistinguishedNameFlags.Reversed) {}
        public X500DistinguishedName (string distinguishedName, X500DistinguishedNameFlags flag) : base(new Oid(), Encode(distinguishedName, flag)) {
        public string Decode (X500DistinguishedNameFlags flag) 

        None                = 0x0000,
        Reversed            = 0x0001,
        UseSemicolons       = 0x0010,
        DoNotUsePlusSign    = 0x0020,
        DoNotUseQuotes      = 0x0040,
        UseCommas           = 0x0080,
        UseNewLines         = 0x0100,
        UseUTF8Encoding     = 0x1000,
        UseT61Encoding      = 0x2000
        ForceUTF8Encoding	= 0x4000,{
*/

			X500DistinguishedName x500 = null ; 

			try
				{
				x500 = new X500DistinguishedName((string)null) ; 
				rv = false ; 
				}
			catch( ArgumentNullException )
				{
				rv = true ; 
				}
			catch( Exception )
				{
				rv = false ; 
				}
			Eval( rv , "x500 = new X500DistinguishedName(null)" ) ; 

			x500 = new X500DistinguishedName( String.Empty ) ; 
			Eval( x500.Format( false ) == String.Empty , "x500 = new X500DistinguishedName( String.Empty )" ) ;
			
			
			
			cert = new X509Certificate2( "dss.cer" ) ; 
			x500 = new X500DistinguishedName( cert.SubjectName ) ; 
			Eval( x500.Name == cert.SubjectName.Name , "x500 = new X500DistinguishedName( cert.SubjectName ) ; " ) ; 

			x500 = new X500DistinguishedName( cert.SubjectName.RawData ) ; 
			Eval( x500.Name == cert.SubjectName.Name , "x500 = new X500DistinguishedName( cert.SubjectName.RawData )" ) ; 

			x500 = new X500DistinguishedName( cert.SubjectName as AsnEncodedData ) ; 
			Eval( x500.Name == cert.SubjectName.Name , "x500 = new X500DistinguishedName( cert.SubjectName as AsnEncodedData )" ) ; 

			x500 = new X500DistinguishedName( String.Empty ) ; 
			Eval( x500.Name == String.Empty , "x500 = new X500DistinguishedName( String.Empty )" ) ; 

			for( int i = 0 ; i < 512 ; i++ )
				{
				TestX500( x500 , GetX500Flag( i ) , true ) ;
				TestX500( cert.SubjectName , GetX500Flag( i ) , false ) ; 
				}

			//error condition in MapNameToString
			x500 = new X500DistinguishedName( cert.SubjectName ) ; 
			try
				{
				x500.Decode( (X500DistinguishedNameFlags) (0xFFFF) )  ;
				rv = false ; 
				}
			catch(ArgumentException )
				{
				rv = true ; 
				}
			catch(Exception e )
				{
				Console.WriteLine(e) ; 
				rv = false ; 
				}
			Eval( rv , "x500.Decode( (X500DistinguishedNameFlags) (0xFFFF) )  ;" ) ; 
			}
		void TestX500( X500DistinguishedName source , X500DistinguishedNameFlags flag , bool emptyString )
			{
			string s = "" ; 
			string decoded = "" ; 
			try
				{
				s = source.Decode( flag ) ;
				X500DistinguishedName x500 = new X500DistinguishedName( s  , flag ) ; 		
				decoded = x500.Decode(flag) ; 
				rv = decoded == s ; 
				rv = source.Format( true ) == x500.Format(true) && rv ;
				rv = source.Format( false ) == x500.Format(false) && rv ;
				}
			catch( Exception e )
				{
				Console.WriteLine( e.ToString() ) ; 
				Console.WriteLine( s ) ; 
				if( !Utility.IsWin9x )
					rv = false ; 
				}
			Eval( rv , "Encode and Decode with the following flag:  " + flag.ToString() + " and Name is\n     " + s + "\n     " + decoded ) ; 
			}
		X500DistinguishedNameFlags GetX500Flag( int n ) 
			{
			X500DistinguishedNameFlags rv = 0 ; 
			if( Check( n , 0x1 ) )
				rv |= X500DistinguishedNameFlags.Reversed ; 
			if( Check( n , 0x2 ) )
				rv |= X500DistinguishedNameFlags.UseSemicolons ; 
			if( Check( n , 0x4 ) )
				rv |= X500DistinguishedNameFlags.DoNotUsePlusSign ; 
			if( Check( n , 0x8 ) )
				rv |= X500DistinguishedNameFlags.DoNotUseQuotes ; 
			if( Check( n , 0x10 ) )
				rv |= X500DistinguishedNameFlags.UseCommas ; 
			if( Check( n , 0x20 ) )
				rv |= X500DistinguishedNameFlags.UseNewLines ; 
			if( Check( n , 0x40 ) )
				rv |= X500DistinguishedNameFlags.UseUTF8Encoding ; 
			if( Check( n , 0x80 ) )
				rv |= X500DistinguishedNameFlags.UseT61Encoding ; 
			if( Check( n , 0x100 ) )
				rv |= X500DistinguishedNameFlags.ForceUTF8Encoding ; 
			return rv ; 
			}
		bool Check( int n , int z )
			{
			return (n & z) != 0 ; 
			}
		void TestEnhanced()
			{
			/*
			Null Oid collection
			Empty oid collection
			OidCollection count = 1 
			OidCollection contains garbage OID
			OidCollection count of all key usages			
			*/
			string szOID_PKIX_KP_SERVER_AUTH      = "1.3.6.1.5.5.7.3.1";
			string szOID_PKIX_KP_CLIENT_AUTH      = "1.3.6.1.5.5.7.3.2";
			string szOID_PKIX_KP_CODE_SIGNING     = "1.3.6.1.5.5.7.3.3";
			string szOID_PKIX_KP_EMAIL_PROTECTION = "1.3.6.1.5.5.7.3.4";
			string SPC_INDIVIDUAL_SP_KEY_PURPOSE_OBJID = "1.3.6.1.4.1.311.2.1.21";
			string SPC_COMMERCIAL_SP_KEY_PURPOSE_OBJID = "1.3.6.1.4.1.311.2.1.22";

			try
				{
				X509EnhancedKeyUsageExtension exty = new X509EnhancedKeyUsageExtension( (OidCollection) null , true ) ; 
				rv = false ; 
				}
			catch( ArgumentNullException )
				{
				rv = true ; 
				}
			catch( Exception )
				{
				rv = false ; 
				}
			Eval( rv , "X509EnhancedKeyUsageExtension:  Null collection" ) ; 

			X509EnhancedKeyUsageExtension ext = new X509EnhancedKeyUsageExtension( new OidCollection() , true ) ; 
			X509EnhancedKeyUsageExtension extCopy = new X509EnhancedKeyUsageExtension() ; 
			extCopy.CopyFrom( ext ) ; 			
			Eval( Compare( extCopy , ext ) , "Empty oid collection" ) ; 


			//OidCollection.Count == 1
			OidCollection oc = new OidCollection() ; 
			oc.Add( new Oid( szOID_PKIX_KP_SERVER_AUTH ) ) ; 
			ext = new X509EnhancedKeyUsageExtension( oc , false ) ; 
			extCopy = new X509EnhancedKeyUsageExtension() ; 
			extCopy.CopyFrom( ext ) ; 			
			Eval( Compare( extCopy , ext ) , "OidCollection.Count == 1" ) ; 
			
			//OidCollection contains garbage OID
			if( !Utility.IsWin9x ) //Bug249427 - don't run this test in 9x
				{
				oc = new OidCollection() ; 
				oc.Add( new Oid( "garbage" ) ) ; 
				try
					{
					// Garbage OIDs cause a crypto exception to be thrown under Vista, which seems
					// reasonable, so just skip this part if it does.
					ext = new X509EnhancedKeyUsageExtension( oc , false ) ; 
					extCopy = new X509EnhancedKeyUsageExtension() ; 
					extCopy.CopyFrom( ext ) ; 			
					Eval( Compare( extCopy , ext ) , "OidCollection contains garbage OID" ) ; 
					}
				catch (CryptographicException)
					{
					Console.WriteLine("Garbage OID threw exception (OK), skipping test");
					}
				}

			//OidCollection has several key usages
			oc = new OidCollection() ; 
			oc.Add( new Oid( szOID_PKIX_KP_SERVER_AUTH ) );
			oc.Add( new Oid( szOID_PKIX_KP_CLIENT_AUTH ) );
			oc.Add( new Oid( szOID_PKIX_KP_CODE_SIGNING )); 
			oc.Add( new Oid( szOID_PKIX_KP_EMAIL_PROTECTION  ) );
			oc.Add( new Oid( SPC_INDIVIDUAL_SP_KEY_PURPOSE_OBJID ) );
			oc.Add( new Oid( SPC_COMMERCIAL_SP_KEY_PURPOSE_OBJID  ) );
			ext = new X509EnhancedKeyUsageExtension( oc , false ) ; 
			extCopy = new X509EnhancedKeyUsageExtension() ; 
			extCopy.CopyFrom( ext ) ; 			
			Eval( Compare( extCopy , ext ) , "OidCollection has several key usages" ) ; 
			}
		bool Compare( X509EnhancedKeyUsageExtension a , X509EnhancedKeyUsageExtension b )
			{
			try
				{
				bool contains = false ; 
				if( a.Critical != b.Critical ) 
					return false ; 
				if( a.EnhancedKeyUsages.Count != b.EnhancedKeyUsages.Count ) 
					return false ; 
				for( int i = 0 ; i < a.EnhancedKeyUsages.Count ; i++ ) 
					{
					for( int j = 0 ; j < b.EnhancedKeyUsages.Count ; j++ )
						{
						if( b.EnhancedKeyUsages[j].Value == a.EnhancedKeyUsages[i].Value )
							{
							contains = true ; 
							break ; 
							}
						}
					if( !contains ) 
						return false ; 
					contains = false ; 
					}
				}
			catch
				{
				return false ; 
				}
			return true ; 
			}
		public bool XpOrHigher
			{
			get
				{
				return !IsWin2kOrLower() ; 
				}
			}
		void TestSubjectKey()
			{
			/*
			X509SubjectKeyIdentifierExtension
				String->byte array, true / false
			*/
			char[] high = { (char) 0x1000 , (char) 0x2000 , (char) 0x2345 , (char) 0x6789 , (char) 0xFFFF } ; 
			char[] mid = { (char) 0x100 , (char) 0x101 , (char) 0x102 , (char) 0x103 , (char) 0x104 , (char) 0xFFF } ; 
			char[] mix = { (char) 0x65 , (char) 0x650 , (char) 0x6500 , (char) 0x66 , (char) 0x660 , (char) 0x6600 } ; 
			char[] low = { (char) 0x65 , (char) 0x66 , (char) 0x67 , (char) 0x68 , (char) 0x69 } ; 
			string s = String.Empty ; 
			for( int i = 0 ; i < 1000 ; i++ ) 
				{
				s += (char) (i % 26 + 0x65) ; 
				}
			string[] str =  { new String( high ) , 
				new String( mix ) , 
				new String( low ) , 
				new String( mid ) , 
				s , 
				String.Empty} ; 
			byte[] bytes = null ; 
			for( int i = 0 ; i < str.Length ; i++ ) 
				{
				try
					{
					Console.WriteLine( str[i] ) ; 
					bytes = Encoding.Unicode.GetBytes(str[i]) ; 
					X509SubjectKeyIdentifierExtension ext = new X509SubjectKeyIdentifierExtension( bytes , i % 2 == 0 ) ; 
					X509SubjectKeyIdentifierExtension extCopy = new X509SubjectKeyIdentifierExtension() ; 
					extCopy.CopyFrom( ext ) ; 
					rv = ext.SubjectKeyIdentifier == extCopy.SubjectKeyIdentifier && ext.Critical == extCopy.Critical && ext.SubjectKeyIdentifier.Length > 0 ; 

					ext = new X509SubjectKeyIdentifierExtension( str[i] , i % 2 == 0 ) ; 
					extCopy = new X509SubjectKeyIdentifierExtension() ; 
					extCopy.CopyFrom( ext ) ; 
					bool rv2 = ext.SubjectKeyIdentifier == extCopy.SubjectKeyIdentifier && ext.Critical == extCopy.Critical && rv && ext.SubjectKeyIdentifier.Length > 0 ; 

					if( str[i] == String.Empty ) 
						rv = false ; 
					else
						rv = rv && rv2 ; 
					}
				catch( Exception e )
					{
					if( str[i] == String.Empty && e is ArgumentException) 
						rv = true ; 
					else
						rv = false ; 
					}
				Eval( rv , "Encoding X509SubjectKeyIdentifier for:  " + str[i] ) ; 
				}
			}
		void TestBasicConstraints()
			{
			/*Test the following:
				certificateAuthority = true : false
				hasPathLengthConstraint = true : false 
				pathLengthConstraint = -1 (exception), 0 , 100 , Integer.Max
			*/
			int[] path = { -1 , 0 , 100 , Int32.MaxValue } ;
			for( int i = 0 ; i < 2 ; i++ ) 
				{
				for( int j = 0 ; j < 2 ; j++ ) 
					{
					for( int k = 0 ; k < path.Length ; k++ ) 
						{
						try
							{
							X509BasicConstraintsExtension ext = new X509BasicConstraintsExtension( i == 0 , j == 0 , path[k] , k % 2 == 0 ) ; 
							X509BasicConstraintsExtension extCopy = new X509BasicConstraintsExtension () ; 
							extCopy.CopyFrom( ext ) ; 
							rv = extCopy.CertificateAuthority == ext.CertificateAuthority && 
								extCopy.HasPathLengthConstraint == ext.HasPathLengthConstraint &&
								extCopy.Critical == ext.Critical && 
								extCopy.PathLengthConstraint == ext.PathLengthConstraint ;
							}
						catch
							{
							if( path[k] < 0 )
								rv = true ; 
							else 
								rv = false ; 
							}
						Eval( rv , "Test X509BasicConstraintsExtension for { i , j , k } = { " + i.ToString() + " , " + j.ToString() + " , " + k.ToString() +  " }"  ) ; 
						}
					}
				}
			}
		void TestKeyUsage()
			{
			for( int i = 0 ; i < 1024 ; i++ ) 
				{
				X509KeyUsageExtension ku = new X509KeyUsageExtension( GetFlag( i , true ) , true ) ; 
				X509KeyUsageExtension kuCopy = new X509KeyUsageExtension() ; 
				kuCopy.CopyFrom( ku ) ; 
				AsnEncodedData asn = new AsnEncodedData( ku.Oid , ku.RawData ) ; 
				X509KeyUsageExtension kuCopy2 = new X509KeyUsageExtension() ; 
				try
					{
					kuCopy2.CopyFrom( asn ) ; 
					rv = false ; 
					}
				catch( ArgumentException )
					{
					rv = true ; 
					}
				catch( Exception ) 
					{
					rv = false ; 
					}
				rv = ku.KeyUsages == kuCopy.KeyUsages && kuCopy.KeyUsages == GetFlag( i , false ) && kuCopy.Critical == ku.Critical && rv ;
				Eval( rv , "Test X509KeyUsageExtension encoding for:  " + GetFlag( i , true ).ToString() ) ; 
				}
			}
		X509KeyUsageFlags GetFlag( int n , bool error ) 
			{
			X509KeyUsageFlags rv = 0 ; 
			if( (n & 0x1) != 0 )
				rv |= X509KeyUsageFlags.EncipherOnly ; 
			if( (n & 0x2) != 0 )
				rv |= X509KeyUsageFlags.CrlSign ; 
			if( (n & 0x4) != 0 )
				rv |= X509KeyUsageFlags.KeyCertSign ; 
			if( (n & 0x8) != 0 )
				rv |= X509KeyUsageFlags.KeyAgreement ; 
			if( (n & 0x10) != 0 )
				rv |= X509KeyUsageFlags.DataEncipherment ; 
			if( (n & 0x20) != 0 )
				rv |= X509KeyUsageFlags.KeyEncipherment ; 
			if( (n & 0x40) != 0 )
				rv |= X509KeyUsageFlags.NonRepudiation ; 
			if( (n & 0x80) != 0 )
				rv |= X509KeyUsageFlags.DigitalSignature ; 
			if( (n & 0x100) != 0 )
				rv |= X509KeyUsageFlags.DecipherOnly ; 
			if( (n & 0x200) != 0 && error )
				rv |= (X509KeyUsageFlags) 0x40000000 ; //garbage
			return rv ; 
			}
		#endregion
		#region X509Certificate2 Test Cases
		/// <summary>
		/// Test the constructor:  X509Certificate2(X509Certificate2)
		/// Pass in null
		/// Empty object
		/// cert = new X509Certificate2(ci.FileName, ci.Password);
		///	cert = new X509Certificate2(ci.FileName); 
		/// </summary>
		/// <returns></returns>
		public void Test003a()
		{
			bool rv = true ; 
			X509Certificate2 cert = null , certImp = null ; 
			try
			{
				X509Certificate2 nullCert = null ;
				cert = new X509Certificate2( nullCert ) ; 
				rv = false ; 
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ;
				rv = true ; 
			}
			finally
			{
				Eval( rv , "X509Certificate2 cert = new X509Certificate2( null )" ) ; 
			}

			try
			{
				cert = new X509Certificate2( new X509Certificate2() ) ; 
				WriteLine( cert.ToString(true) ) ; 
				rv = true ; //we expect it to throw an exception because there'll be an invalid handle
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ;
				rv = false ; 
			}
			finally
			{
				Eval( rv , "X509Certificate2 cert = new X509Certificate2( new X509Certificate2() )" ) ; 
			}

			string filename = "Certificates.xml";
			XmlDriver xd = new XmlDriver(filename);
			CertificateInfo[] ci = xd.Certificates;
			try
			{
				certImp = new X509Certificate2(ci[1].FileName, ci[1].Password) ;
				cert = new X509Certificate2( certImp );
				rv = ci[1].Matches(cert) ;
			}
			catch( Exception e )
			{
				rv = false ; 
				WriteLine( e.ToString() ) ; 
			}
			finally
			{
				Eval( rv , "cert = new X509Certificate2( new X509Certificate2(ci[1].FileName, ci[1].Password) )" ) ;
			}

			try
			{
				certImp = new X509Certificate2(ci[0].FileName ) ;
				cert = new X509Certificate2( certImp );
				rv = ci[0].Matches(cert) ;
				//				WriteLine( cert.ToString( true ) ) ;
				//				WriteLine( certImp.ToString( true ) ) ;
			}
			catch( Exception e )
			{
				rv = false ; 
				WriteLine( e.ToString() ) ; 
			}
			finally
			{
				Eval( rv , "cert = new X509Certificate2( new X509Certificate2(ci[0].FileName )" ) ;
			}
		}
		/// <summary>
		/// Test Archived property, make sure value persists when set to true, does not persist when set to false
		/// </summary>
		public void Test005()
		{
			X509Store store = new X509Store(storeName) ; 
			cert = new X509Certificate2( "CoolCert.cer" ) ; 
			store.Open( OpenFlags.IncludeArchived | OpenFlags.ReadWrite ) ; 
			store.Add( cert ) ; 
			store.Certificates[0].Archived = true ; 
			Eval( store.Certificates[0].Archived , "cert.Archived = true" ) ; 
			store.Certificates[0].Archived = false ; 
			Eval( !store.Certificates[0].Archived , "Verify that X509Certificate2.Archived property cannot be set to false" ) ; 
			store.Close() ; 

			store.Open( OpenFlags.IncludeArchived | OpenFlags.ReadWrite ) ; 
			store.Remove( store.Certificates[0] ) ; 
			store.Close() ; 

			cert = GetX509( 0 ) ; 
			cert.Archived = true ; 
			Eval( cert.Archived , "Set X509Certificate2.Archived = true for a certificate not in a store" ) ; 

			//Negative test case for Archived
			try
				{
				cert = new X509Certificate2() ; 
				cert.Archived = false ; 
				rv = false ; 
				}
			catch( CryptographicException )
				{
				rv = true ; 
				}
			catch( Exception )
				{
				rv = false ; 
				}
			finally
				{
				Eval( rv , "Negative test case for Archived" ) ; 
				}

			//corner cases for friendly name
			cert = new X509Certificate2( "CoolCert.cer" ) ; 
			cert.FriendlyName = null ; 
			string res = cert.FriendlyName ; 
			Eval( res == String.Empty , "Corner case for FriendlyName" ) ; 

			//corner case for to string
			cert = new X509Certificate2( "emailnameinissuer.cer" ) ; 
			res = cert.ToString( true ).ToLower() ; 
			rv = res.IndexOf( "pkit@microsoft.com" ) != -1 ; 
			WriteLine( cert.ToString( true ) ) ; 
			Eval( rv , "ToString for EmailName in issuer" ) ; 

			string val = "VISHALA-DEV.ntdev.corp.microsoft.com" ;
			cert = new X509Certificate2( "dnsnameinsubaltname2.cer" ) ; 
			res = cert.ToString( true ).ToLower() ; 
			rv = res.IndexOf( val.ToLower() ) != -1 ; 
			WriteLine( cert.ToString( true ) ) ; 
			Eval( rv , "ToString for DnsName in subject" ) ; 

			NegativeNameTests() ; 
		}
		void NegativeNameTests()
			{
			cert = new X509Certificate2() ; 
			string s = "" ; 
			try
				{
				rv = false ; 
				cert.FriendlyName = "fpp" ; 
				}
			catch(CryptographicException)
				{
				rv = true ; 
				}
			catch(Exception)
				{}
			Eval(rv , "cert.FriendlyName = \"fpp\" ; " ) ; 

			try
				{
				rv = false ; 
				s = cert.SubjectName.ToString() ;
				}
			catch(CryptographicException)
				{
				rv = true ; 
				}
			catch(Exception)
				{}
			Eval(rv , "s = cert.SubjectName.ToString()" ) ; 


			try
				{
				rv = false ; 
				s = cert.IssuerName.ToString() ;
				}
			catch(CryptographicException)
				{
				rv = true ; 
				}
			catch(Exception)
				{}
			Eval(rv , "s = cert.ISsuerName.ToString()" ) ; 
			}
		/// <summary>
		/// Test Display function
		/// Empty instance
		/// Valid instance
		/// </summary>
		public void Test011()
		{
			bool rv = true ; 
			X509Certificate2 cert = null ;

			try
				{
				X509Certificate2UI.DisplayCertificate(null) ;
				rv =false ;
				}
			catch(ArgumentNullException)
				{
				rv = true ; 
				}
			catch(Exception e)
				{
				Console.WriteLine(e); 
				rv = false ; 
				}
			Eval( rv , "X509Certificate2UI.DisplayCeritificate(null)" ) ; 

			try
				{
				X509Certificate2UI.DisplayCertificate(null,IntPtr.Zero) ;
				rv =false ;
				}
			catch(ArgumentNullException)
				{
				rv = true ; 
				}
			catch(Exception e)
				{
				Console.WriteLine(e); 
				rv = false ; 
				}
			Eval( rv , "X509Certificate2UI.DisplayCeritificate(null,IntPtr.Zero)" ) ; 

			try
			{
				cert = new X509Certificate2() ; 
				X509Certificate2UI.DisplayCertificate(cert) ; 
				rv = false ; 
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ; 
				rv = true ; 
			}
			finally
			{
				Eval( rv , "(new X509Certificate2()).DisplayCertificate()" ) ; 
			}

			try
			{
				cert = new X509Certificate2() ; 
				X509Certificate2UI.DisplayCertificate(cert,IntPtr.Zero) ; 
				rv = false ; 
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ; 
				rv = true ; 
			}
			finally
			{
				Eval( rv , "(new X509Certificate2()).DisplayCertificate(cert,IntPtr.Zero)" ) ; 
			}

			string filename = "Certificates.xml";
			XmlDriver xd = new XmlDriver(filename);
			CertificateInfo[] ci = xd.Certificates;

			try
			{
				cert = new X509Certificate2(ci[1].FileName, ci[1].Password) ;
				X509Certificate2UI.DisplayCertificate(cert) ; 
				rv = true ; 
				//TODO figure out how to close window
			}
			catch( Exception e )
			{
				rv = false ; 
				WriteLine( e.ToString() ) ; 
			}
			finally
			{
				Eval( rv , "(X509Certificate2( new X509Certificate2(ci[1].FileName, ci[1].Password) ).DisplayCertificate()" ) ;
			}

			try
			{
				cert = new X509Certificate2(ci[0].FileName ) ;
				X509Certificate2UI.DisplayCertificate(cert) ; 
				rv = true ; 
				//TODO FIGURE OUT HOW TO CLOSE WINDOW
			}
			catch( Exception e )
			{
				rv = false ; 
				WriteLine( e.ToString() ) ; 
			}
			finally
			{
				Eval( rv , "(new X509Certificate2( new X509Certificate2(ci[0].FileName )).DisplayCertificate()" ) ;
			}

			Test027() ; 
		}
		/// <summary>
		/// Test the PrivateKey Property
		/// Get the PrivateKey where it is null
		/// Get PrivateKey when it should be returned
		/// </summary>
		public void Test008b()
			{
			X509Certificate2 cert = new X509Certificate2() ; 
			try
				{
				cert.PrivateKey = null ; 
				rv = false ; 
				}
			catch( CryptographicException ) 
				{
				rv = true ; 
				}
			catch
				{
				rv = false ; 
				}
			Eval( rv, "new X509Certificate2().PrivateKey==null throws CryptographicException" ) ; 

			//Set privateKey property to null
			XmlDriver xd = new XmlDriver(filename);
			CertificateInfo[] ci = xd.Certificates;

			cert = new X509Certificate2(ci[1].FileName , ci[1].Password , X509KeyStorageFlags.Exportable) ; 
			rv = cert.PrivateKey != null ; 
			cert.PrivateKey = null ; 
			cert.Import( cert.Export( X509ContentType.Cert ) ) ; 
			Eval( cert.PrivateKey == null && rv , "cert.PrivateKey = null" ) ; 

			cert = new X509Certificate2(ci[1].FileName , ci[1].Password , X509KeyStorageFlags.Exportable) ; 
			RSACryptoServiceProvider rsa = new RSACryptoServiceProvider() ; 
			try
				{
				cert.PrivateKey = rsa ; 
				rv = false ; 
				}
			catch( CryptographicUnexpectedOperationException ) 
				{
				rv = true ; 
				}
			catch
				{
				rv = false ; 
				}
			Eval( rv , "Set Invalid PrivateKey" ) ; 

			/*
			bool fOAEP = !IsWin2kOrLower() ; 
			byte[] test = { 30 , 31 , 32 , 33 , 34 } ; 
			byte[] vc = rsa.Decrypt( ((RSACryptoServiceProvider)cert.PrivateKey).Encrypt( test , fOAEP ) , fOAEP ) ; 
			rv = true ; 
			for( int i = 0 ; i < vc.Length ; i++ )
			{
				if( vc[i] != test[i] )
				{
					rv = false ; 
					break ; 
				}
			}
			Eval( rv , "cert.PrivateKey = rsa" ) ; */
			}
		public void Test008()
		{
			cert = GetX509( 10 ) ; 
			AsymmetricAlgorithm aa = cert.PrivateKey ; 
			RSACryptoServiceProvider rsa = (RSACryptoServiceProvider) aa ;
			bool fOAEP = !IsWin2kOrLower() ; 
			byte[] test = { 30 , 31 , 32 , 33 , 34 } ; 
			byte[] vc = rsa.Decrypt( rsa.Encrypt( test , fOAEP ) , fOAEP ) ; 
			rv = true ; 
			for( int i = 0 ; i < vc.Length ; i++ )
			{
				if( vc[i] != test[i] )
				{
					rv = false ; 
					break ; 
				}
			}
			Eval( cert.HasPrivateKey , "HasPrivateKey returns true" ) ; 
			Eval( rv , "Get PrivateKey when it should be returned" ) ; 

			try
			{
				XmlDriver xd = new XmlDriver(filename);
				CertificateInfo[] ci = xd.Certificates;

				cert = new X509Certificate2(ci[1].FileName , ci[1].Password , X509KeyStorageFlags.Exportable) ; 

				WriteLine(cert.ToString( true ) ); 
				aa = cert.PrivateKey ; 
				rsa = (RSACryptoServiceProvider) aa ;
				vc = rsa.Decrypt( rsa.Encrypt( test , false ) , false ) ; 
				rv = true ; 
				for( int i = 0 ; i < vc.Length ; i++ )
				{
					if( vc[i] != test[i] )
					{
						rv = false ; 
						break ; 
					}
				}
			}
			catch( Exception e)
			{
				WriteLine( e.ToString() ) ; 
				rv = false ; 
			}
			finally
			{
				Eval( rv , "Get PrivateKey when it should be returned, fOAEP=false" ) ; 
			}
			cert = GetX509( 0 ) ; 
			aa = cert.PrivateKey ; 
			Eval( !cert.HasPrivateKey , "HasPrivateKey returns false" ) ; 
			Eval( aa == null , "Get PrivateKey where it is null" ) ; 
			Test008b() ; 
			Test009() ; 
		}
		public void Test009()
			{
			//Test PublicKey for DSS
			cert = new X509Certificate2( "dss.cer" ) ; 
			string str = cert.ToString( true ) ; 
			Console.WriteLine( str ) ; 
			PublicKey pk = cert.PublicKey ; 
			DSACryptoServiceProvider dsa = (DSACryptoServiceProvider) pk.Key ;
			rv = dsa.KeySize == 1024 ; 
			Eval( rv , "Get DSA Public Key" ) ; 

			
			Test010() ; 
			}
		//Put PropertyGets where the handle is invalid
		public void Test010()
			{
			string fn ; 
			object o ; 

			cert = new X509Certificate2() ; 
			fn = cert.ToString( true ) ; 
			rv = fn == cert.ToString( false ) ; 
			Eval( rv , "ToString calls" ) ; 
			
			try
				{
				cert = new X509Certificate2() ; 
				Oid oid = cert.SignatureAlgorithm ; 
				rv = false ; 
				}
			catch( CryptographicException )
				{
				rv = true ; 
				} 
			Eval( rv , "Get cert.SignatureAlgorithm when m_safeCertContext.IsInvalid = true" ) ; 

			try
				{
				cert = new X509Certificate2() ; 
				rv = cert.Archived ; 
				rv = false ; 
				}
			catch( Exception )
				{
				rv = true ; 
				} 
			Eval( rv , "Test010:  Archived property" );

			try
				{
				cert = new X509Certificate2() ; 
				o=cert.Extensions ; 
				rv = false ; 
				}
			catch( Exception )
				{
				rv = true ; 
				} 
			Eval( rv , "Test010:  Extensions property" );
				
			try
				{
				cert = new X509Certificate2() ; 
				fn = cert.FriendlyName ; 
				rv = false ; 
				}
			catch( Exception )
				{
				rv = true ; 
				} 
			Eval( rv , "Test010:  FriendlyName property" );
				
			try
				{
				cert = new X509Certificate2() ; 
				o=cert.NotAfter ; 
				rv = false ; 
				}
			catch( Exception )
				{
				rv = true ; 
				} 
			Eval( rv , "Test010:  NotAfter property" );
				
			try
				{
				cert = new X509Certificate2() ; 
				o=cert.NotBefore ; 
				rv = false ; 
				}
			catch( Exception )
				{
				rv = true ; 
				} 
			Eval( rv , "Test010:  NotAfter property" );
				
			try
				{
				cert = new X509Certificate2() ; 
				o=cert.PublicKey ; 
				rv = false ; 
				}
			catch( Exception )
				{
				rv = true ; 
				} 
			Eval( rv , "Test010:  PublicKey property" );
				
			try
				{
				cert = new X509Certificate2() ; 
				o=cert.PrivateKey ; 
				rv = false ; 
				}
			catch( Exception )
				{
				rv = true ; 
				} 
			Eval( rv , "Test010:  PrivateKey property" );
				
			}
		/// <summary>
		/// Test Export round trip for all 3 types
		/// </summary>
		public void Test014()
		{
			SSTImport() ;  
			AuthenticodeImport() ;
			for( int i = 0 ; i < certChain.Length ; i++ )
				{
					if( certChain[i] != String.Empty )
						{
						Test014RoundTrip(i,X509ContentType.Cert) ;
						Test014RoundTrip(i,X509ContentType.SerializedCert) ;

						X509KeyStorageFlags flag = (X509KeyStorageFlags) 0x04 ; 
						string path = String.Format( "x509tests\\test{0}\\test{1}.p12"  , i.ToString() , i.ToString() ) ; 
						cert = new X509Certificate2( path , Password , flag ) ; 
						Test014RoundTrip<String>( true , cert ,  Password, i , flag  ) ; 

						flag = (X509KeyStorageFlags) 0 ; 
						cert = new X509Certificate2( path , Password , flag ) ; 
						Test014RoundTrip<String>( false , cert ,  Password , i , flag  ) ; 
						}
				}
		}
		SecureString CertPassword
			{
			get
				{
				return String2SecureString(Password) ; 
				}
			}
		SecureString String2SecureString(string s)
			{
			char[] chars = s.ToCharArray() ; 
			SecureString password = new SecureString() ; 
			for( int i = 0 ; i < chars.Length ; i++ )
				password.AppendChar( chars[i] ) ; 
			return password ; 
			}
		string Password
			{
			get
				{
				return "password" ; 
				}
			}
		protected void SSTImport()
			{
			X509Certificate2Collection certCollection = new X509Certificate2Collection() ;
			certCollection.Import( certs[2] ) ;
			byte[] sst = certCollection.Export( X509ContentType.SerializedStore ) ; 
			X509Certificate2Collection certCollection2 = new X509Certificate2Collection() ; 
			certCollection2.Import( sst ) ; 
			Eval( certCollection.Count==certCollection2.Count , "Export SST" ) ; 
			}
		protected void AuthenticodeImport()
			{
				cert = new X509Certificate2() ; 
				cert.Import( certs[4] ) ; 
				string ts = cert.ToString( true ) ; 
				Console.WriteLine( ts ) ; 
				int v = cert.Version ;
				rv = v == 3 ; 
				Eval( rv , "Import an Authenticode certificate" ) ; 
			}
		protected void Test014RoundTrip<T>( bool valid , X509Certificate2 cert , T Password , int id , X509KeyStorageFlags flag ) 
			{
				X509Certificate2 certImp = new X509Certificate2() ; 
				X509Certificate2 certImp2 = new X509Certificate2() ; 
				Import<T> funcPtrImport = Delegate.CreateDelegate( typeof(Import<T>) , certImp , certImp.GetType().GetMethod( "Import" , new Type[]{ typeof(byte[]) , Password.GetType() , typeof(X509KeyStorageFlags) }) ) as Import<T> ; 
				Export<T> funcPtrExport = null ; 
				byte[] export = null ; 
				byte[] importedExport = null ; 
				
				try
					{
					funcPtrExport = Delegate.CreateDelegate( typeof(Export<T>) , cert , cert.GetType().GetMethod( "Export" , new Type[]{ typeof(X509ContentType)  , Password.GetType() } ) ) as Export<T> ; 
					export = funcPtrExport( X509ContentType.Pfx , Password ) ; 
	//				export = cert.Export( X509ContentType.Pfx , password ) ;
					funcPtrImport( export , Password , flag ) ; 
	//				certImp.Import( export , password , flag) ; 
					funcPtrExport = Delegate.CreateDelegate( typeof(Export<T>) , certImp , certImp.GetType().GetMethod( "Export" , new Type[]{ typeof(X509ContentType)  , Password.GetType() } ) ) as Export<T> ; 
					importedExport = funcPtrExport( X509ContentType.Pfx , Password ) ;
					funcPtrImport = Delegate.CreateDelegate( typeof(Import<T>) , certImp2 , certImp2.GetType().GetMethod( "Import" , new Type[]{ typeof(byte[]) , Password.GetType() , typeof(X509KeyStorageFlags) }) ) as Import<T> ; 					
					funcPtrImport( export , Password , flag ) ; 
					rv = CertEquals( certImp , cert ) ;
					rv = CertEquals( cert , certImp2 ) && rv ; 
					}
				catch( Exception e )
					{
					WriteLine( e.ToString() ) ; 
					if( valid )
						Console.WriteLine( "Exception in Import/Export:  id=" + id.ToString() + ", val=" + flag.ToString() ) ; 
					rv = !valid ; 
					}
				Eval( rv , "X509Certificate2.Export Pfx round trip, test id=" + id.ToString() ) ; 

			}
		protected void Test014RoundTrip(int id , X509ContentType type)
			{
				string path = "x509tests\\test" + id.ToString() + "\\" + certChain[id] ; 
				cert = new X509Certificate2( path ) ; 
				X509Certificate2 certImp = new X509Certificate2() ; 
				byte[] export = null ; 
				byte[] import = null ; 

				try
					{
				export = cert.Export( type ) ;
				certImp.Import( export ) ; 
				import = certImp.Export( type ) ;
				//rv = ByteArrayEquals( export , import ) ; 
				rv = CertEquals( certImp , cert ) ;
					}
				catch( Exception e )
					{
					WriteLine( e.ToString() ) ; 
					Console.WriteLine( "Exception in Import/Export" ) ; 
					rv = false ; 
					}
				Eval( rv , "X509Certificate2.Export round trip, test id=" + id.ToString() ) ; 
			}
		protected bool CertEquals( X509Certificate2 a , X509Certificate2 b ) 
			{
				bool rv = true ;
				try
					{
					if( a.Archived != b.Archived )
						return Failure( "Archived" ) ; 
					if( a.Extensions.Count != b.Extensions.Count )
						return Failure( "Extensions.Count" ) ; 
					if( a.FriendlyName != b.FriendlyName )
						return Failure( "FriendlyName" ) ; 
					if( a.IssuerName.Name != b.IssuerName.Name )
						return Failure( "Issuer" ) ; 
					if( a.NotBefore != b.NotBefore )
						return Failure( "NotBefore" ) ; 
					if( a.NotAfter != b.NotAfter )
						return Failure( "NotAfter" ) ; 
					if( a.SerialNumber != b.SerialNumber )
						return Failure( "SerialNumber" ) ; 
					if( a.SubjectName.Name != b.SubjectName.Name )
						return Failure( "Subject" ) ; 
					if( a.Thumbprint != b.Thumbprint )
						return Failure( "Thumbprint" ) ; 
					if( a.Version != b.Version )
						return Failure( "Version" ) ; 
					if( !ByteArrayEquals( a.RawData , b.RawData ) )
						return Failure( "Archived" ) ; 
					if( a.SignatureAlgorithm.Value != b.SignatureAlgorithm.Value )
						return false ; 
					}
				catch( Exception )
					{
					Console.WriteLine( "Exception in CertEquals" ) ; 
					rv = false ; 
					}
				return rv ; 
			}
		protected bool Failure( string what )
			{
			Console.WriteLine( what + " check failed" ) ; 
			return false ; 
			}
		protected bool ByteArrayEquals( byte[] a , byte[] b )
			{
				if( a.Length != b.Length )
					{
					WriteLine( "ByteArrayEquals:  Different lengths" ) ; 
					return true ; 
					}
				for( int i = 0 ; i < a.Length ; i++ )
					{
					if( a[i] != b[i] )
						return false ; 
					}
				return true ; 
			}
		/// <summary>
		/// Negative Export test cases
		/// </summary>
		protected void Test015(X509ContentType type)
		{
			byte[] data = null ; 
			bool rv = false ; 
			try
				{
				cert = GetX509( 0 ) ; 
				data = cert.Export( type ) ; 
				}
			catch( CryptographicException ce )
				{
				WriteLine( ce.ToString() ) ; 
				rv = true ; 
				}
			catch( Exception )
				{
				rv = false ; 
				}
			Eval( rv , "NegativeExport:  " + type.ToString() ) ;
		}
		protected void Test015b()
			{
			byte[] data ; 
			rv = false ; 
			try
				{
				cert = GetX509( 10 ) ; 
				data = cert.Export( X509ContentType.Pfx , "password" ) ;  
				certImp = new X509Certificate2( data , "password" , X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable ) ; 
				rv = true ; 
				}
			catch( Exception e)
				{
				WriteLine( e.ToString() ) ; 
				rv = false ; 
				}
			Eval( rv , "certImp = new X509Certificate2( data , \"password\" , X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable )" ) ; 
			
			}
		public void Test015()
			{
			Test015( X509ContentType.Pkcs7 ) ;
			Test015( X509ContentType.SerializedStore ) ;
			Test015( X509ContentType.Authenticode ) ;
			Test015( X509ContentType.Unknown ) ;
			Test015b() ; 
			}
		/// <summary>
		/// Test GetNameInfo
		/// Valid instance
		/// Empty instance
		/// </summary>
		protected void Test016( X509NameType nameType , bool forIssuer , string expected , bool empty )
			{
			Test016(nameType,forIssuer,expected,empty,"dns.cer" ) ; 
			}
		protected void Test016( X509NameType nameType , bool forIssuer , string expected , bool empty , string certName)
		{

			string strRes = String.Empty ; 
			if( empty ) 
				cert = new X509Certificate2() ; 
			else
			{
				cert = new X509Certificate2(certName) ;				
			}

			try
			{
				strRes = cert.GetNameInfo( nameType , forIssuer ) ; 
				Console.WriteLine( strRes ) ; 
				rv = strRes == expected ; 
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ; 
				rv = empty ; 
			}
			Eval( rv , "GetNameInfo should return: " + expected + " and it returned " + strRes + " for " + nameType.ToString() ) ; 
		}
		public void Test016()
		{
			Test016( X509NameType.SimpleName , true , "" , true ) ; 
			Test016( X509NameType.EmailName , true , "" , true ) ; 
			Test016( X509NameType.UpnName , true , "" , true ) ; 
			Test016( X509NameType.DnsName , true , "" , true ) ; 
			Test016( X509NameType.UrlName , true , "" , true ) ; 
			Test016( X509NameType.SimpleName , false , "" , true ) ; 
			Test016( X509NameType.EmailName , false , "" , true ) ; 
			Test016( X509NameType.UpnName , false , "" , true ) ; 
			Test016( X509NameType.DnsName , false , "" , true ) ; 
			Test016( X509NameType.UrlName , false , "" , true ) ; 
			
			//Loop this for multiple certs
			Test016( X509NameType.SimpleName , true , "CA" , false ) ; 
			Test016( X509NameType.EmailName , true , "" , false ) ; 
			Test016( X509NameType.UpnName , true , "" , false ) ; 
			Test016( X509NameType.DnsName , true , "CA" , false ) ; 
			Test016( X509NameType.UrlName , true , "" , false ) ; 
			Test016( X509NameType.SimpleName , false , "all ext" , false ) ; 
			Test016( X509NameType.EmailName , false , "Email Name" , false ) ; 
			Test016( X509NameType.UpnName , false , "" , false ) ; 
			Test016( X509NameType.DnsName , false , "DNS name" , false ) ; 
			Test016( X509NameType.UrlName , false , "URL string" , false ) ; 

			//DnsFromALternative name
			Test016( X509NameType.DnsName , false , "JamieC Code Signing Cert - self" , false , "jamiec self cert.cer" ) ;
			Test016( X509NameType.DnsName , true , "JamieC Code Signing Cert - self" , false , "jamiec self cert.cer" ) ;
			Test016( X509NameType.DnsFromAlternativeName , false , String.Empty , false , "jamiec self cert.cer" ) ;
			Test016( X509NameType.DnsFromAlternativeName , true , String.Empty , false , "jamiec self cert.cer" ) ;
		}
		/// <summary>
		/// Test Reset - make sure protected properties are reset
		/// </summary>
		public void Test017()
		{
			int test = -1 ; 
			try
			{
				cert = GetX509(0) ; 
				test = cert.Version ; 
				rv = test != -1 ;
				cert.Reset() ; 
				test = cert.Version ;
				rv = false ; 
			}
			catch( CryptographicException ) 
			{
				rv = rv && true ; 
			}
			Eval( rv , "X509Certificate2.Reset()" ) ; 
		}
		/// <summary>
		/// Test GetCertContentType( string filename )
		/// Filename does not exist
		/// Filename for a content type
		/// Null
		/// </summary>
		public void Test019b()
		{
			string nullStr = null ;
			X509ContentType type = X509ContentType.Unknown ; 
			X509Certificate2 certImp = null ; 
			byte[] raw = new byte[0] ; 


			try
				{
				X509Certificate2.GetCertContentType( raw ) ; 
				rv = false ; 
				}
			catch( ArgumentException )
				{
				rv = true ; 
				}
			Eval( rv , "X509Certificate2.GetCertContentType( raw.Length = 0 )" ) ; 
			
			try
				{
				X509Certificate2.GetCertContentType( (byte[])null ) ; 
				rv = false ; 
				}
			catch( ArgumentException )
				{
				rv = true ; 
				}
			Eval( rv , "X509Certificate2.GetCertContentType( null )" ) ; 
			
			try
			{
				X509Certificate2.GetCertContentType( nullStr ) ;
				rv = false ; 
			}
			catch( ArgumentNullException )
			{
				rv = true ; 
			}
			catch( Exception )
			{
				rv = true ; 
			}
			finally
			{
				Eval( rv , "X509Certificate2.GetCertContentType( nullStr )" ) ; 
			}

			try
			{
				type = X509Certificate2.GetCertContentType( "test.txt" ) ;
				rv = false ; 
			}
			catch( Exception e)
			{
				WriteLine( e.ToString() ) ; 
				rv = true ; 
			}
			Eval( rv , "X509Certificate2.GetCertContentType( test.txt )" ) ; 

			for( int i = 0 ; i < certs.Length ; i++ )
			{
				try
				{
					type = X509Certificate2.GetCertContentType( certs[i] ) ; 
					rv = type == types[i] ;
				}
				catch( Exception e)
				{
					rv = false ;
					WriteLine( e.ToString() ) ; 
				}
				finally
				{
					Eval( rv , "X509Certificate2.GetCertContentType( " + certs[i] + " )" ) ;
				}
			}

			//Serialized cert, export a cert to a serialized cert
			cert = new X509Certificate2( certs[0] ) ; 
			certImp = new X509Certificate2( cert.Export( X509ContentType.SerializedCert ) ) ; 
			type = X509Certificate2.GetCertContentType( certImp.Export( X509ContentType.SerializedCert ) ) ; 
			Eval( type == X509ContentType.SerializedCert , "Byte -X509Certificate2.GetCertContentType( certImp.Export( X509ContenType.SerializedCert ) )" ) ; 

			type = X509Certificate2.GetCertContentType( (new X509Certificate2( certs[0] )).Export( X509ContentType.SerializedCert ) )  ;
			Eval( type == X509ContentType.SerializedCert , "Byte - X509Certificate2.GetCertContentType( (new X509Certificate2( certs[0] )).Export( X509ContentType.SerializedCert ) )" ) ; 

			X509Certificate2Collection exc = null ; 
			//repeat for Byte
			for( int i = 0 ; i < certs.Length ; i++ )
			{
				try
				{
					if( types[i] == X509ContentType.Cert )
						type = X509Certificate2.GetCertContentType( (new X509Certificate2(certs[i])).Export( types[i] ) ) ; 
					else if( types[i] == X509ContentType.SerializedStore || types[i] == X509ContentType.Pkcs7 )
						{
						exc = new X509Certificate2Collection() ; 
						exc.Import( certs[i] ) ; 
						type = X509Certificate2.GetCertContentType( exc.Export( types[i] ) ) ; 
						}
					else if( types[i] == X509ContentType.Authenticode )
						{
				//			FileStream fs = new FileStream(certs[i], FileMode.Open, FileAccess.Read);
			//				byte[] certBytes = new byte[fs.Length];
		//					fs.Read(certBytes, 0, certBytes.Length);
	//						fs.Close() ;

//							cert = new X509Certificate2( certBytes ) ; 
//							exc = new X509Certificate2Collection() ; 
	//						exc.Import( certBytes ) ; 
//							type = X509Certificate2.GetCertContentType( certBytes ) ; // exc.Export( types[i] ) ) ; //cert.Export( types[i] ) ) ; 
						}
					else if( types[i] == X509ContentType.Pfx )
						type = X509Certificate2.GetCertContentType( (new X509Certificate2(certs[i]  , "password" , X509KeyStorageFlags.Exportable) ).Export( types[i], "password"  ) ) ; 
					rv = type == types[i] ;
				}
				catch( Exception e)
				{
					rv = false ;
					WriteLine( e.ToString() ) ; 
				}
				finally
				{
					if( types[i] != X509ContentType.Authenticode )
						Eval( rv , "Byte-X509Certificate2.GetCertContentType( " + certs[i] + " )" ) ;
				}
			}

				//Negative test case to throw exception from QueryCertBlobType
				byte[] b = new byte[1] ; 
				try
					{
					X509Certificate2.GetCertContentType( b ) ; 
					rv = false ; 
					}
				catch( CryptographicException )
					{
					rv = true ; 
					}
				catch( Exception )
					{
					rv = false ; 
					}
				finally
					{
					Eval( rv , "Negative test case to throw exception from QueryCertBlobType" ) ; 
					}
		}
		#endregion
		#region X509Certificate2Collection Test Cases
		void Bug373917()
			{
			Console.WriteLine( "Bug373917" ) ; 
			KeyInfoX509Data data = new KeyInfoX509Data(new X509Certificate("jamiec self cert.cer"), X509IncludeOption.ExcludeRoot) ;			
			Eval( data.Certificates.Count == 1 , "Bug373917" ) ; 
			}
		/// <summary>
		/// Test Add()
		/// Add null - get ArgumentNullException
		/// Add object when list is empty
		/// Add object when list is not empty
		/// Add duplicate objects
		/// Add( this[0] )
		/// Verify with Contains( this[idx] ) and Contains( cert )
		/// Test AddRange( X509Certificate2Collection ) w/ multiple duplicate elements
		/// </summary>
		public void Test020a()
		{
			Bug373917() ; 
			X509Certificate2Collection exc = null , exc3 = null ;
			try
			{
				exc = new X509Certificate2Collection() ; 
				exc.Add( null ) ; 
				rv = false ; 
			}
			catch( ArgumentNullException )
			{
				rv = true ;
			}
			catch( Exception e)
			{
				WriteLine( e.ToString() ) ; 
				rv = false ;
			}
			finally
			{
				Eval( rv , "exc.Add( null )" ) ; 
			}

			try
			{
				exc3 = null ;
				exc = new X509Certificate2Collection( exc3 ) ; 
				rv = false ;
			}
			catch( ArgumentNullException )
			{
				rv = true ; 
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ; 
				rv = false ;
			}
			finally
			{
				Eval( rv , "exc.AddRange( null )" ) ; 
			}

			exc = new X509Certificate2Collection() ; 
			cert = GetX509( 0 ) ; 
			exc.Add( cert ) ; 
			rv = exc.Contains( cert ) && exc.Contains( exc[0] ) ;
			Eval( rv , "Add object when list is empty" ) ; 

			exc3 = new X509Certificate2Collection( exc ) ; 
			rv = exc.Contains( cert ) && exc.Contains( exc[0] ) ;
			Eval( rv , "AddRange:  Add object when list is empty" ) ; 

			//now continue by adding a 2nd object
			cert = GetX509( 1 ) ; 
			exc.Add( cert ) ; 
			rv = exc.Contains( cert ) && exc.Contains( exc[1] ) ;
			Eval( rv , "Add object when list contains elements" ) ; 

			exc3 = new X509Certificate2Collection( exc ) ; 
			rv = exc.Contains( cert ) && exc.Contains( exc[1] ) ;
			Eval( rv , "AddRange:  Add object when list contains elements" ) ; 

			//now continue with duplicates
			cert = GetX509( 0 ) ; 
			exc.Add( cert ) ; 
			rv = exc.Contains( cert ) && exc.Contains( exc[2] ) ;
			Eval( rv , "Add duplicate objects" ) ; 

			exc3 = new X509Certificate2Collection( exc ) ; 
			rv = exc.Contains( cert ) && exc.Contains( exc[2] ) ;
			Eval( rv , "AddRange:  Add duplicate objects" ) ; 

			//add self referential dupe
			exc.Add( exc[0] ) ; 
			rv = exc.Contains( exc[0] ) && exc.Contains( exc[3] ) ; 
			Eval( rv , "Add self refential duplicate" ) ; 

			//Test the AddRange
			exc = new X509Certificate2Collection( exc ) ; 
			rv = exc.Contains( exc[0] ) && exc.Contains( exc[3] ) && exc.Contains( GetX509( 0 ) ) && exc.Contains( GetX509( 1 ) ) && exc.Contains( exc[2] ) ;
			Eval( rv , "Test AddRange( X509Certificate2Collection ) w/ multiple duplicate elements" ) ; 
		}
		/// <summary>
		/// Null - get ArgumentNullException
		/// Add array when list is empty
		/// Add array when list is not empty
		/// Add array of null references
		/// Add array of null ref and objects
		/// Verify with Contains( this[idx] ) and Contains( cert )
		/// Test thru constructor
		/// </summary>
		public void Test020b()
		{
			X509Certificate2Collection exc = null ;
			X509Certificate2[] aEx = null ;

			try
			{
				exc = new X509Certificate2Collection( aEx ) ; 
				rv = false ;
			}
			catch( ArgumentException )
			{
				rv = true ; 
			}
			catch( Exception )
			{
				rv = false ; 
			}
			finally
			{
				Eval( rv , "AddRange( null ) throws ArgumentNullException" ) ; 
			}		
	
			aEx = new X509Certificate2[1] ; 
			aEx[0] = GetX509(0) ;
			exc = new X509Certificate2Collection( aEx ) ; 
			rv = exc.Contains( aEx[0] ) && exc.Contains( exc[0] ) ;
			Eval( rv , "Add array when list is empty" ) ; 

			//Now add to the list when its not empty
			aEx = new X509Certificate2[2] ; 
			aEx[0] = GetX509(0) ;
			aEx[1] = GetX509(1) ;
			exc.AddRange( aEx ) ; 
			rv = exc.Contains( aEx[0] ) && exc.Contains( aEx[1] ) && exc.Contains(exc[1]) && exc.Contains(exc[2]);
			Eval( rv , "Add array when list contains element" ) ; 

			//Add an array of null references
			try
			{
				aEx = new X509Certificate2[200] ; 
				exc = new X509Certificate2Collection( aEx ) ; 
				rv = false ; 
			}
			catch( ArgumentNullException )
			{
				rv = true ;
			}
			catch( Exception )
			{
				rv = false ; 
			}
			finally
			{
				Eval( rv , "Add an array of null references - get ArgumentNUllException" ) ; 
			}

			//Add an array with both nulla nd valid objects
			try
			{
				aEx = new X509Certificate2[3] ; 
				aEx[0] = GetX509( 0 ) ; 
				aEx[2] = GetX509( 1 ) ; 
				exc = new X509Certificate2Collection() ;
				exc.AddRange( aEx ) ; 
				rv = false ; 
			}
			catch( Exception )
			{
				rv = !exc.Contains( aEx[0] ) ; //this should not be here					 
				rv = rv && !exc.Contains( aEx[2] ) ; //this should not be here					 
			}
			finally
			{
				Eval( rv , "Add an array of null and valid references - get ArgumentNUllException and verify that there are no elements - transaction aborted" ) ; 
			}

			try
				{
				exc = new X509Certificate2Collection() ; 
				exc.Contains( null ) ; 
				rv = false ; 
				}
			catch( Exception )
				{
				rv = true ; 
				}
			Eval( rv , "exc.Contains( null )" ) ; 

/*Remove this test case
			//add an array with valid and invalid objects
			try
			{
				aEx = new X509Certificate2[3] ; 
				aEx[0] = GetX509( 0 ) ; 
				aEx[1] = new X509Certificate2() ; 
				aEx[2] = GetX509( 1 ) ; 
				exc = new X509Certificate2Collection() ;
				exc.AddRange( aEx ) ; 
				rv = !exc.Contains( aEx[0] ) ; //this should not be here					 
				//rv = false ; 
			}
			catch( Exception )
			{
				rv = false ; 
			}
			finally
			{
				Eval( rv , "Add an array of invalid and valid references - get CryptoGraphicException and verify that there are no elements - transaction aborted" ) ; 
			}*/
		}
		/// <summary>
		/// Test Remove()
		/// Remove null
		/// Remove when the Collection is empty
		/// Remove when the collection has one item
		///		Remove this[0]
		///		Remove GetX509( 0 ) 
		/// Remove when the collection has multiple items
		///		Remove this[0], twice
		///	When this[0] == this[1] == this[2], remove this[0], verify with this[1]
		///	When this[0] == this[1] == this[2], remove this[2], verify with this[1]
		/// </summary>
		public void Test021a()
		{
			X509Certificate2Collection exc = null ;
			
			//Remove null
			try
			{
				exc = new X509Certificate2Collection() ; 
				exc.Remove( null ) ;
				rv = false ;    
			}
			catch( ArgumentNullException )
			{
				rv = true ; 
			}
			catch( Exception )
			{
				rv = false ;
			}
			finally
			{
				Eval( rv , "exc.Remove( null )" ) ; 
			}

			try
			{
				exc = new X509Certificate2Collection() ; 
				exc.Remove( GetX509(0) ) ; 
				rv = false ; 
			}
			catch( ArgumentException )
			{
				rv = true ;
			}
			finally
			{
				Eval( rv , "Remove item not in collection" ) ; 
			}

			exc = GetCollection( 0 ) ; 
			exc.Remove( exc[0] ) ; 
			rv = !exc.Contains( GetX509(0) ) ; 
			Eval( rv , "Remove this[0] when the collection has one item" ) ; 

			try
			{
				cert = GetX509(0) ; 
				exc = GetCollection( -1 ) ; 
				exc.Add( cert ) ; 
				exc.Remove( cert ) ; 
				rv = !exc.Contains( cert ) ; 
			}
			catch( Exception )
			{
				rv = false ; 
			}
			finally
			{
				Eval( rv , "Remove GetX509(0) when the collection has one item" ) ; 
			}

			exc = GetCollection( 1 ) ; 
			exc.Remove( exc[0] )  ; 
			exc.Remove( exc[0] )  ; 
			rv = exc.Add( GetX509(0) ) == 0 ; //it's empty so let's add one
			Eval( rv , "Remove this[0] twice when the collection has two items" ) ; 

			X509Certificate2[] test = new X509Certificate2[2] ; 
			exc = GetCollection( 2 ) ; 
			test[0] = exc[1] ; 
			test[1] = exc[2] ; 
			exc.Remove( exc[0] ) ;
			rv = exc.Contains( test[1] ) && exc.Contains( test[0] ) ; //true
			Eval( rv , "When this[0] == this[1] == this[2], remove this[0]" ) ; 

			exc = GetCollection( 2 ) ; 
			test[0] = exc[0] ; 
			test[1] = exc[1] ; 
			exc.Remove( exc[2] ) ;
			rv = exc.Contains( test[1] ) && exc.Contains( test[0] ) ; //true
			Eval( rv , "When this[0] == this[1] == this[2], remove this[2]" ) ; 
		}
		/// <summary>
		/// Test RemoveRange( X509Certificate2[] )
		/// Remove Null
		///	Collection has multi items
		///	Broken transaction
		/// </summary>
		public void Test021b()
		{
			int i = 0 ; 
			X509Certificate2[] aEx = null ; 
			X509Certificate2Collection exc = null ; 
			try
			{
				exc = new X509Certificate2Collection() ; 
				exc.RemoveRange( aEx ) ;
				rv = false ;    
			}
			catch( ArgumentNullException )
			{
				rv = true ; 
			}
			catch( Exception )
			{
				rv = false ;
			}
			finally
			{
				Eval( rv , "exc.RemoveRange( nullArray )" ) ; 
			}

			exc = new X509Certificate2Collection() ; 
			aEx = new X509Certificate2[10] ; 
			for( i = 0 ; i < aEx.Length ; i++ )
				aEx[i] = GetX509( 0 ) ;
			exc.AddRange( aEx ) ; 
			exc.RemoveRange( aEx ) ; 
			for( i = 0 ; i < aEx.Length ; i++ )
			{
				rv = !exc.Contains( aEx[i] ) ; 
				if( !rv )
					break ; 
			}
			Eval( rv , "exc.RemoveRange( aEx )" ) ;  

			exc = new X509Certificate2Collection() ; 
			aEx = new X509Certificate2[10] ; 
			for( i = 0 ; i < aEx.Length - 1; i++ )
			{
				aEx[i] = GetX509( 0 ) ;
				exc.Add( aEx[i] ) ; 
			}
			try
			{
				exc.RemoveRange( aEx ) ; 
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ; 
				rv = true ; 
			}
			for( i = 0 ; i < aEx.Length - 1; i++ )
			{
				rv = exc.Contains( aEx[i] ) ; //should still contain them
				if( !rv )
					break ; 
			}
			Eval( rv , "X509Certificate2 transaction is invalid, don't remove items" ) ;  
		}
		/// <summary>
		/// Test RemoveRange( X509Certificate2Collection )
		/// Null
		/// this.RemoveRange(this)
		/// this.RemoveRange( this ) with an error
		/// </summary>
		public void Test021c()
		{
			// this.RemoveRange( this ) with an error
			X509Certificate2[] aEx = null ; 
			X509Certificate2Collection exc = null , exc2 = null; 
			
			try
			{
				exc = new X509Certificate2Collection() ;
				exc.RemoveRange( exc2 ) ;
				rv = false ;
			}
			catch( ArgumentNullException )
			{
				rv = true ; 
			}
			catch( Exception )
			{
				rv = false ; 
			}
			finally
			{
				Eval( rv , "RemoveRange(null) " ) ;
			}
			try
			{
				exc = new X509Certificate2Collection() ; 
				aEx = new X509Certificate2[3] ; 
				aEx[0] = GetX509( 0 ) ; 
				aEx[1] = GetX509( 0 ) ; 
				aEx[2] = GetX509( 0 ) ; 
				exc.AddRange( aEx ) ; 
				exc[2] = null ; 
				aEx[2] = null ; 
				exc.RemoveRange( exc ) ; 
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ; 
				rv = false ; 
			}
			finally
			{
				rv = exc.Contains( aEx[0] ) && exc.Contains( aEx[1] ) ; //should roll back and still have the items
				Eval( rv , "exc.RemoveRange( exc ) - with an error" ) ; 
			}

			try
			{
				exc = new X509Certificate2Collection() ;
				aEx = new X509Certificate2[3] ; 
				aEx[0] = GetX509( 0 ) ; 
				aEx[1] = GetX509( 0 ) ; 
				aEx[2] = GetX509( 0 ) ; 
				exc.AddRange( aEx ) ; 
				exc.RemoveRange( exc ) ; 
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ; 
				rv = false ; 
			}
			finally
			{
				rv = exc.Contains( aEx[0] ) && exc.Contains( aEx[1] ) ; //should roll back and still have the items
				Eval( rv , "exc.RemoveRange( exc )" ) ; 
			}
		}
		/// <summary>
		///Insert at 0, Remove at 0, verify w/ Contains
		///Insert at Length , Remove at Length , verify w/ Contains
		///Insert at Length + 1
		///Remove at Length + 1
		/// </summary>
		public void Test022()
		{
			X509Certificate2Collection exc = null ; 
			int Length = 3 ; 

			exc = GetCollection( 2 ) ;
			cert =  GetX509(2); 
			exc.Insert( 0 , cert ) ; 
			exc.RemoveAt( 0 ) ; 
			rv = !exc.Contains( cert ) ; 
			Eval( rv , "Insert at 0, Remove at 0, verify w/ Contains" ) ; 

			exc = GetCollection( 2 ) ;
			cert = GetX509( 2 ) ; 
			exc.Insert( Length , cert ) ; 
			exc.RemoveAt( Length ) ; 
			rv = !exc.Contains( cert ) ; 
			Eval( rv , "Insert at Length , Remove at Length+1, verify w/ Contains" ) ; 

			try
			{
				exc = GetCollection( 2 ) ;
				cert = GetX509( 1 ) ; 
				exc.Insert( Length + 1 , cert ) ; 
				rv = false ; 
			}
			catch( Exception )
			{
				rv = true ; 
			}
			Eval( rv , "Insert at Length + 1" ) ; 

			try
			{
				exc = new X509Certificate2Collection() ; 
				exc.Insert( 0 , null ) ; 
				rv = false ; 
			}
			catch( Exception )
			{
				rv = true ; 
			}
			Eval( rv , "exc.Insert( 0 , null )" ) ; 

			try
			{
				exc = GetCollection( 2 ) ;
				cert = GetX509( 1 ) ; 
				exc.Insert( 0 , cert ) ; 
				exc.RemoveAt( Length + 1 ) ; 
				rv = false ; 
			}
			catch( Exception )
			{
				rv = true ; 
			}
			Eval( rv , "RemoveAt Length + 1" ) ; 
		}
		/// <summary>
		/// Test GetEnumerator
		/// Empty collection, MoveNext returns false
		/// Empty collection, foreach doesn't go into loop
		/// Multiple item collection
		/// </summary>
		public void Test023()
		{
			X509Certificate2Collection exc = null ; 
			X509Certificate2Enumerator exEnum = null ; 
			int len = 0 ; 

			// Empty collection, MoveNext returns false
			exc = new X509Certificate2Collection() ; 
			exEnum = exc.GetEnumerator() ; 
			//reste when empty
			exEnum.Reset() ; 
			rv = !exEnum.MoveNext() ; 
			Eval( rv , "Empty collection, MoveNext returns false" ) ; 

			// Empty collection, foreach doesn't go into loop
			exc = new X509Certificate2Collection() ; 
			exEnum = exc.GetEnumerator() ; 
			rv = true ; 
			foreach( X509Certificate2 cert in exc )
			{
				cert.Verify() ; 
				rv = false ; 
			}
			Eval( rv , "Empty collection, foreach doesnt go into loop" ) ; 

			// Multiple item collection
			exc = GetCollection( 2 ) ; //length is 3
			exEnum = exc.GetEnumerator() ; 
			foreach( X509Certificate2 cert in exc )
			{
				cert.Verify() ; 
				len++ ; 
			}
			Eval( len == 3 , "X509Certificate2Enumerator:  Multiple item collection" ) ; 
		}
		public void Test023b()
			{
			X509Certificate2Collection exc = null ; 
			X509Certificate2Enumerator exEnum = null ; 
			int len = 0 ; 

			// Empty collection, MoveNext returns false
			exc = new X509Certificate2Collection() ; 
			exEnum = exc.GetEnumerator() ; 
			//reste when empty
			((IEnumerator)exEnum).Reset() ; 
			rv = !((IEnumerator)exEnum).MoveNext() ; 
			Eval( rv , "Empty collection, MoveNext returns false" ) ; 

			// Empty collection, foreach doesn't go into loop
			exc = new X509Certificate2Collection() ; 
			exEnum = exc.GetEnumerator() ; 
			rv = true ; 
			foreach( X509Certificate2 cert in exc )
			{
				cert.Verify() ; 
				rv = false ; 
			}
			Eval( rv , "Empty collection, foreach doesnt go into loop" ) ; 

			// Multiple item collection
			exc = GetCollection( 2 ) ; //length is 3
			exEnum = exc.GetEnumerator() ; 
			foreach( X509Certificate2 cert in exc )
			{
				cert.Verify() ; 
				len++ ; 
			}
			Eval( len == 3 , "X509Certificate2Enumerator:  Multiple item collection" ) ; 
			}
		/// <summary>
		/// Find tests
		/// Find in an empty list, return null
		/// findValue == null - ArgumentNUllException 
		/// find returns no value
		/// FindByThumbprint - exc[11].Thumbprint
		/// FindBySubjectName - exc[21].Subject
		/// FindByIssuerName - exc[42].Issuer
		/// FindBySerialNumber - exc[0].SerialNumber
		/// </summary>
		public void Test026()
		{
			X509Certificate2Collection exc = new X509Certificate2Collection() , found = null ;

			// Find in an empty list, return null
			found = exc.Find( X509FindType.FindByTimeValid , DateTime.Now , false ) ; 
			rv = true ;
			foreach( X509Certificate2 cert in found )
				rv = false ; 
			Eval( rv, "Find in an empty list, return null" ) ; 

			// findValue == null - ArgumentNUllException 
			try
			{
				found = exc.Find( X509FindType.FindByTimeValid , null , false ) ; 
				rv = false ; 
			}
			catch( ArgumentNullException )
			{
				rv = true ; 
			}
			finally
			{
				Eval( rv , "findValue == null - ArgumentNUllException " ) ; 
			}

			exc.Import( certs[3] ) ; //43 certs

			//Find returns null
			found = exc.Find( X509FindType.FindBySubjectName , "INVALID SUBJECT" , false ) ; 
			rv = true ;
			foreach( X509Certificate2 cert in found )
				rv = false ; 
			Eval( rv , "Find returns null" ) ; 

			try
			{
				found = exc.Find( X509FindType.FindBySubjectName , 1234 , false ) ; 
				rv = false ; 
			}
			catch( CryptographicException )
			{
				rv = true ;
			}
			Eval( rv , "exc.Find( X509FindType.FindBySubjectName , 1234 , false )" ) ; 

			//FindByThumbprint - exc[11].Thumbprint
			found = exc.Find( X509FindType.FindByThumbprint , exc[11].Thumbprint , false ) ; 
			rv = found.Count > 0 ; //  != null ; 
			foreach( X509Certificate2 cert in found )
			{
				rv = cert.Thumbprint == exc[11].Thumbprint && rv; 
			}
			Eval( rv , "FindByThumbprint - exc[11].Thumbprint" ) ; 

			//FindBySubjectName - exc[21].Subject
			found = exc.Find( X509FindType.FindBySubjectName , exc[21].SubjectName.Name.Substring( exc[21].SubjectName.Name.IndexOf( "CN=" ) + 3 , 1 ) , false ) ; 
			rv = found.Count > 0 ; //  != null ; 
			foreach( X509Certificate2 cert in found )
			{
				rv = cert.SubjectName.Name.ToLower().IndexOf( exc[21].SubjectName.Name.Substring( exc[21].SubjectName.Name.IndexOf( "CN=" ) + 3 , 1 ).ToLower() ) != -1 && rv; 
			}
			Eval( rv , "FindBySubjectName - exc[21].Subject" ) ; 

			//FindByIssuerName - exc[21].Issuer
			found = exc.Find( X509FindType.FindByIssuerName , exc[42].IssuerName.Name.Substring( exc[42].IssuerName.Name.IndexOf( "CN=" ) + 3 , 1 )  , false ) ; 
			rv = found.Count > 0 ; //  != null ; 
			WriteLine( "Issuer first letter:  " + exc[42].IssuerName.Name.Substring( exc[42].IssuerName.Name.IndexOf( "CN=" ) + 3 , 1 ) ) ; 
			foreach( X509Certificate2 cert in found )
			{
				rv = cert.IssuerName.Name.ToLower().IndexOf( exc[42].IssuerName.Name.Substring( exc[42].IssuerName.Name.IndexOf( "CN=" ) + 3 , 1 ).ToLower() ) != -1 && rv; 
				if( !rv )
					{
					WriteLine( cert.IssuerName.Name ) ; 
					}
			}
			Eval( rv , "FindByIssuerName - exc[42].Issuer" ) ; 

			//FindBySerialNumber - exc[0].SerialNumber
			found = exc.Find( X509FindType.FindBySerialNumber , exc[0].SerialNumber , false ) ; 
			rv = found.Count > 0 ; //  != null ; 
			foreach( X509Certificate2 cert in found )
			{
				rv = cert.SerialNumber == exc[0].SerialNumber && rv; 
			}
			Eval( rv , "FindBySerialNumber - exc[0].SerialNumber" ) ; 

			Test026Negative( exc ,  X509FindType.FindByIssuerName ) ; 
			Test026Negative( exc ,  X509FindType.FindByThumbprint ) ; 
			Test026Negative( exc ,  X509FindType.FindBySerialNumber ) ; 
			Test026Negative( exc ,  X509FindType.FindByTimeValid ) ; 
			Test026Negative( exc ,  X509FindType.FindByTimeNotYetValid ) ; 
			Test026Negative( exc ,  X509FindType.FindByTimeExpired ) ; 
			Test026Negative( exc ,  X509FindType.FindByExtension ) ; 
			Test026Negative( exc ,  X509FindType.FindByKeyUsage ) ; 
			Test026Negative( exc ,  (X509FindType) 0xFFF ) ; 

			X509Certificate2 certer = GetX509( 1 ) ; 
			exc[ exc.Count - 1 ] = certer ; 
			rv = exc.Contains(certer) ; 
			Eval( rv , "exc[ exc.Count - 1 ] = certer" ) ; 
		}
		void Test026Negative( X509Certificate2Collection exc , X509FindType ft)
			{
				try
				{
				object found = exc.Find( ft , -1.1 , false ) ; 
				rv = false ; 
				}
			catch( CryptographicException )
				{
				rv = true ; 
				}
			catch( Exception )
				{
				rv = false ; 
				}
			Eval( rv , ft.ToString() + " Negative" );
			}
		protected void Test026Date(X509Certificate2Collection exc)
		{
			//Test union of FindByTimeValid | FindByTimeNotValid | FindByTimeExpired
			X509Certificate2Collection found = exc.Find( X509FindType.FindByTimeValid , DateTime.Now , false ) ; 
			found.AddRange( exc.Find( X509FindType.FindByTimeNotYetValid , DateTime.Now , false ) ) ; 
			found.AddRange( exc.Find( X509FindType.FindByTimeExpired , DateTime.Now , false ) ) ; 
			rv = true ; 
			foreach( X509Certificate2 cert in exc )
			{
				if( !found.Contains( cert ) )
				{
					rv = false ; 
					break ;
				}
			}
			if( rv )
			{
				foreach( X509Certificate2 cert in found )
				{
					if( !exc.Contains( cert ) )
					{
						rv = false ; 
						break ;
					}
				}
			}
			Eval( rv , "Test union of FindByTimeValid | FindByTimeNotValid | FindByTimeExpired" ) ; 			
			}
		public void Test026b()
		{
			X509Certificate2Collection exc = Test043PreRun( false ) ; 
			exc.Add( new X509Certificate2( "dss.cer" ) ) ; 
			Test026Date( exc ) ; 
			Test026Oid( exc , "Key Usage" ) ; 
			Test026Oid( exc , "Basic Constraints" ) ; 
			Test026Oid( exc , "Enhanced Key Usage" ) ; 
			Test026Oid( exc , "Subject Key Identifier" ) ; 
			try
				{
				Test026FindEx( exc , "Not an extension" , false ) ; //bug 340647 get an exception now
				rv = false; 
				}
			catch( ArgumentException )
				{
				rv = true ; 
				}
			catch(Exception e)
				{
				Console.WriteLine(e) ; 
				rv = false  ;
				}
			Eval( rv , "Regression for bug 340647" ) ; 
			Test026FindEx( exc , "1.3.6.1.4.1.311.42.1.2" , false ) ; 
			Test026KeyUsage( exc , 0x80FF , true ) ;
			Test026KeyUsage( exc , "DigitalSignature" , 0x0080 , true ) ;
			Test026KeyUsage( exc , "NonRepudiation" , 0x0040 , true ) ;
			Test026KeyUsage( exc , "KeyEncipherment" , 0x0020 , true ) ;
			Test026KeyUsage( exc , "DataEncipherment" , 0x0010 , true ) ;
			Test026KeyUsage( exc , "KeyAgreement" , 0x0008 , true ) ;
			Test026KeyUsage( exc , "KeyCertSign" , 0x0004 , true ) ;
			Test026KeyUsage( exc , "CRLSign" , 0x0002 , true ) ;
			Test026KeyUsage( exc , "DecipherOnly" , 0x8000 /* X509KeyUsageFlags.DecipherOnly */, true ) ;
			Test026KeyUsage( exc , "EncipherOnly" , 0x0001 , true ) ;
			Test026CertPolicy( exc ) ; 
			Test026SKI( exc ) ; 
			Test026SDN( exc ) ; 
			Test026IDN( exc ) ; 
			Test026FAN( exc ) ; 
			Test026FTN( exc ) ; 
			Test043PostRun( false , exc ) ; 
		}
		protected void Test026FTN( X509Certificate2Collection exc ) 
			{
			string value = "UserSignature" ; 

			exc.Add( new X509Certificate2( "dss.cer" ) ) ; 
			X509Certificate2Collection found = exc.Find( X509FindType.FindByTemplateName , value , false ) ; 
			rv = found.Count > 0 ; 
			Eval( rv , "X509Certificate2Collection found = exc.Find( X509FindType.FindByTemplateName , value , true )" ) ; 

			found = exc.Find( X509FindType.FindByTemplateName , "" , true ) ; 
			rv = found.Count == 0 ; 
			Eval( rv , "X509Certificate2Collection found = exc.Find( X509FindType.FindByTemplateName , \"\" , true )" ) ; 

			//not a string
			try
				{
				found = exc.Find( X509FindType.FindByTemplateName , -1 , true ) ; 
				rv = false ; 
				}
			catch( CryptographicException )
				{
				rv = true ; 
				}
			Eval( rv , "X509Certificate2Collection found = exc.Find( X509FindType.FindByTemplateName , -1 , true )" ) ; 

			//Only run this on Xp or higher
			if( XpOrHigher )
				{
				//Find by template name V2
				value = "1.3.6.1.4.1.311.21.8.3692315854.1256661383.1690418588.4201632533.3914912987.3264497027" ;
				cert = new X509Certificate2( "templateext.cer" ) ;
				exc.Add( cert ) ; 
				found = exc.Find( X509FindType.FindByTemplateName , value , false ) ;
				rv = found.Contains( cert ) ; 
				Eval( rv , "FindByTemplateName for V2 template" ) ; 
				}			
			}
		protected void Test026FAN( X509Certificate2Collection exc ) 
			{
			string value = AppPol ; //"1.3.6.1.4.1.311.10.3.4" ; 

			X509Certificate2Collection found = exc.Find( X509FindType.FindByApplicationPolicy , value , true ) ; 
			rv = found.Count > 0 ; 
			Eval( rv , "X509Certificate2Collection found = exc.Find( X509FindType.FindByApplicationPolicy , value , true )" ) ; 


			try
				{
				found = exc.Find( X509FindType.FindByApplicationPolicy , String.Empty , true ) ; 
				rv = false ; 
				}
			catch(ArgumentException)
				{
				rv = true ; 
				}
			catch
				{
				rv = false ; 
				}
			Eval( rv , "X509Certificate2Collection found = exc.Find( X509FindType.FindByApplicationPolicy , \"\" , true )" ) ; 

			//not a string
			try
				{
				found = exc.Find( X509FindType.FindByApplicationPolicy , -1 , true ) ; 
				rv = false ; 
				}
			catch( CryptographicException )
				{
				rv = true ; 
				}
			Eval( rv , "X509Certificate2Collection found = exc.Find( X509FindType.FindByApplicationPolicy , -1 , true )" ) ; 
			}
		protected void Test026IDN( X509Certificate2Collection exc ) 
			{
			cert = exc[3] ; 
			X509Certificate2Collection found = exc.Find( X509FindType.FindByIssuerDistinguishedName , cert.IssuerName.Name , false ) ; 
			rv = found.Contains( cert ) ; 
			Eval( rv , "exc.Find( X509FindType.FindByIssuerDistinguishedName , cert.Issuer , false )" ) ; 

			found = exc.Find( X509FindType.FindByIssuerDistinguishedName , " " , false ) ; 
			rv = found.Count == 0 ; 

			//not a string
			try
				{
				found = exc.Find( X509FindType.FindByIssuerDistinguishedName , 0 , false ) ; 
				rv = false ; 
				}
			catch( CryptographicException )
				{
				rv = true ; 
				} 
			Eval( rv , "found = exc.Find( X509FindType.FindByIssuerDistinguishedName , 0 , false )" ) ; 
			}
		protected void Test026SDN( X509Certificate2Collection exc ) 
			{
			cert = exc[5] ; 
			X509Certificate2Collection found = exc.Find( X509FindType.FindBySubjectDistinguishedName , cert.SubjectName.Name , false ) ; 
			rv = found.Contains( cert ) ; 
			Eval( rv , "exc.Find( X509FindType.FindBySubjectDistinguishedName , cert.Subject , false )" ) ; 

			found = exc.Find( X509FindType.FindBySubjectDistinguishedName , " " , false ) ; 
			rv = found.Count == 0 ; 
			Eval( rv , "SDN: found.Count == 0" ) ; 

			//not a string
			try
				{
				found = exc.Find( X509FindType.FindBySubjectDistinguishedName , 0 , false ) ; 
				rv = false ; 
				}
			catch( CryptographicException )
				{
				rv = true ; 
				} 
			Eval( rv , "found = exc.Find( X509FindType.FindBySubjectDistinguishedName , 0 , false )" ) ; 
			}
		protected void Test026SKI( X509Certificate2Collection exc ) 
			{
			string friendlyName = "Subject Key Identifier" ;
			string value = "acdbfcf1bc052ed2" ;
			X509SubjectKeyIdentifierExtension  ski = null ; 

			X509Certificate2Collection found = exc.Find( X509FindType.FindBySubjectKeyIdentifier , value , true ) ; 
			rv = true ; 
			foreach( X509Certificate2 certy in found )
				{
				X509Extension _ext = certy.Extensions[friendlyName] ;
				ski = (X509SubjectKeyIdentifierExtension) _ext ;
				rv = rv && ski.SubjectKeyIdentifier.Replace( " " , "" ).ToLower() == value ;
				}
			Eval( rv , "FindBySubjectKeyIdentifier - positive test" ) ; 

			found = exc.Find( X509FindType.FindBySubjectKeyIdentifier , "0000000000000" , true ) ; 
			rv = true ; 
			foreach( X509Certificate2 certy in found )
				{
				rv = false ; 
				}
			Eval( rv , "FindBySubjectKeyIdentifier - negative test" ) ; 

			//not a string
			try
				{
				found = exc.Find( X509FindType.FindBySubjectKeyIdentifier , 0xacdb , true ) ; 
				rv = false ; 
				}
			catch( CryptographicException )
				{
				rv = true ; 
				}
			Eval( rv , "found = exc.Find( X509FindType.FindBySubjectKeyIdentifier , 0xacdb , true )" ) ; 
			}
		protected void Test026CertPolicy( X509Certificate2Collection exc ) 
			{
			string value = CertPol ; // "1.3.6.1.4.1.311.42.1.2" ; 

			X509Certificate2Collection found = exc.Find( X509FindType.FindByCertificatePolicy , value , true ) ; 
			rv = found.Count > 0 ; 
			Eval( rv , "X509Certificate2Collection found = exc.Find( X509FindType.FindByCertificatePolicy , value , true )" ) ; 

			try
				{
				found = exc.Find( X509FindType.FindByCertificatePolicy , "" , true ) ; 
				rv = found.Count == 0 ; 
				}
			catch(ArgumentException) //
				{
				rv = true ; 
				}
			catch(Exception)
				{
				rv = false ; 
				}
			Eval( rv , "X509Certificate2Collection found = exc.Find( X509FindType.FindByCertificatePolicy , \"\" , true )" ) ; 

			//not a string
			try
				{
				found = exc.Find( X509FindType.FindByCertificatePolicy , -1 , true ) ; 
				rv = false ; 
				}
			catch( CryptographicException )
				{
				rv = true ; 
				}
			Eval( rv , "X509Certificate2Collection found = exc.Find( X509FindType.FindByCertificatePolicy , -1 , true )" ) ; 
			}
		protected void Test026KeyUsage( X509Certificate2Collection exc , Object value, Object value2 , bool positive) 
			{
			X509Certificate2Collection found = exc.Find( X509FindType.FindByKeyUsage , value , false ) ;
			X509Certificate2Collection found2 = exc.Find( X509FindType.FindByKeyUsage , value2 , false ) ;
			rv = true ; 
			foreach( X509Certificate2 cer in found )
				{
				rv = found2.Contains( cer ) == positive ;
				}
			Eval( rv , "FindByKeyUsage " + value.ToString() + " , " + value2.ToString() + " yields correct results" ) ; 
			}
		protected void Test026KeyUsage( X509Certificate2Collection exc , Object value, bool positive) 
			{
			X509Certificate2Collection found = exc.Find( X509FindType.FindByKeyUsage , value , false ) ;
			rv = true ; 
			foreach( X509Certificate2 cer in found )
				{
				rv = exc.Contains( cer ) == positive ;
				}
			Eval( rv , "FindByKeyUsage " + value.ToString() + " yields correct results" ) ; 
			}
		protected void Test026Oid( X509Certificate2Collection exc , string friendlyName )
		{
			Oid oid = new Oid( friendlyName ) ;
			Test026FindEx( exc , oid.Value ) ; 
			Test026FindEx( exc , oid.FriendlyName ) ; 
		}
		protected void Test026FindEx( X509Certificate2Collection exc , string friendlyName )
		{
			Test026FindEx( exc , friendlyName , true ) ; 
		}
		protected void Test026FindEx( X509Certificate2Collection exc , string friendlyName , bool exp )
		{
			X509Certificate2Collection found = exc.Find( X509FindType.FindByExtension , friendlyName , false ) ; 
			rv = !exp ; 
			foreach( X509Certificate2 cert in found )
			{
				rv = cert.Extensions[friendlyName] != null ;
				if( !rv )
				{
					Console.WriteLine( "Failed" ) ; 
					break ; 
				}
			}
			Eval( rv , "FindByExtension " + friendlyName ) ; 
		}
		/// <summary>
		/// Test the Import function
		/// Import a PCKS 7 file
		/// Import to a collection which already has data- import from byte array
		/// Import to an empty collection, a Pfx w/ password
		/// </summary>
		public void Test024()
		{
			X509Certificate2Collection exc = new X509Certificate2Collection() , exc2 = null ; 
			try
				{
				X509Certificate2UI.SelectFromCollection( exc,null,null,(X509SelectionFlag)(-1)) ; 
				rv = false ; 
				}
			catch(ArgumentException)
				{
				rv = true ; 
				}
			catch(Exception)
				{
				rv = false ; 
				}
			Eval( rv , "exc.Select(null,null,(X509SelectionFlag)(-1)) ; " ) ; 


			exc = new X509Certificate2Collection(GetX509(0))  ;
			Eval(exc.Count==1,"exc = new X509Certificate2Collection(GetX509(0))") ; 

			exc = new X509Certificate2Collection()  ;
			//Import/Export/Import a PKCS7 file - string, byte test for bag of certs
			exc.Import( certs[4] ) ; //43 certs
			exc2 = new X509Certificate2Collection() ; 
			exc2.Import( exc.Export( X509ContentType.Pkcs7 ) ) ; 
			rv = true ; 
			foreach( X509Certificate2 certy in exc )
			{
				if( !exc2.Contains( certy ) )
				{
					rv = false ; 
					break ;
				}
			}
			if( rv )
			{
				foreach( X509Certificate2 certy in exc2 )
				{
					if( !exc.Contains( certy ) )
					{
						rv = false ; 
						break ;
					}
				}
			}
			Eval( rv , "X509Certificate2Collection:  Import/Export/Import a PKCS7 file" ) ; 

			//Import to a collection which already has data- import from byte array
			exc = new X509Certificate2Collection() ;
			exc.Import( certs[4] ) ; 
			cert = GetX509( 0 ) ; 
			exc.Import( cert.Export( X509ContentType.Cert ) ) ; 
			rv  = exc.Contains( cert ) ;
			Eval( rv, "Import to a collection which already has data- import from byte array" ) ; 

			//Import byte array into to an empty collection, a Pfx w/ password
			exc = new X509Certificate2Collection() ;
			cert = GetX509( 1 ) ; 
			//exc.Import( cert.Export( 
			exc.Import( cert.Export( X509ContentType.Cert ) , "password" , X509KeyStorageFlags.DefaultKeySet ) ; 
			rv = exc.Contains( cert ) ; 
			Eval( rv , "Import byte array into to an empty collection, a Pfx w/ password" ) ; 

			try
				{
				exc = new X509Certificate2Collection() ;
				exc.Import( (byte[]) null ) ; 
				rv = false ; 
				}
			catch( Exception )
				{
				rv = true ; 
				}
			Eval( rv , "exc.Import( (byte[]) null )" ) ; 

			try
				{
				exc = new X509Certificate2Collection() ;
				exc.Import( "fileNotHere.cert" ) ; 
				rv = false ; 
				}
			catch( Exception )
				{
				rv = true ; 
				}
			Eval( rv , "exc.Import( fileNotHere.cer )" ) ; 
		}
		/// <summary>
		/// Export tests
		/// Export Empty collection
		/// Export 100 certificates - ExportToMemoryStore will do something because it's limit is 99
		/// Export( X509ContentType.Authenticode) == Exception
		/// </summary>
		public void Test025()
		{
			X509Certificate2Collection exc = new X509Certificate2Collection() ; 
			byte[] exp = null ; 
			byte[] exp2 = null ; 

			// Export Empty collection
			rv = true ; 
			for( int i = 1 ; i <= 5 ; i++ )
			{
				try
				{
					exp = (new X509Certificate2Collection()).Export( (X509ContentType) i  ) ; 
					
					if( i == 1 || i == 2 )
					{
						rv = rv && exp == null ; 
					}
					else if( exp != null ) 
					{
						exc = new X509Certificate2Collection() ;
						exc.Import( exp ) ; 
						exp2 = exc.Export( (X509ContentType) i  ) ; 
						if( i == 3 )
						{
							rv = exp.Length == exp2.Length ; 
						}
						else
						{
							for( int j = 0 ; j < exp.Length ; j++ ) 
							{
								rv = exp[j] == exp2[j] ;
								if( !rv )
									break ; 
							}
						}
					}
					else 
					{
						rv = false ; 
					}
				}
				catch( Exception e )
				{
					WriteLine( e.ToString() ) ; 
					rv = false ; 
				}
				finally
				{
					Eval( rv , "new X509Certificate2Collection().Export(" + ((X509ContentType)i).ToString() + ")" ) ; 
				}
			}

			// Export( X509ContentType.Authenticode) == Exception
			try
			{
				(new X509Certificate2Collection()).Export( X509ContentType.Authenticode ) ;
				rv = false ;
			}
			catch( CryptographicException cg )
			{
				WriteLine( cg.ToString() ) ; 
				rv = true ; 
			}
			finally
			{
				Eval( rv , "(new X509Certificate2Collection()).Export( X509ContentType.Authenticode )" ) ; 
			}

			//export cert, serailzed cert
			exc = new X509Certificate2Collection() ; 
			cert = GetX509(0) ; 
			exc.Add( cert ) ; 
			byte[] raw = exc.Export( X509ContentType.Cert ) ; 
			exc = new X509Certificate2Collection() ; 
			exc.Import( raw ) ; 
			rv = exc.Contains( cert ) ; 
			Eval( rv , "Export Cert" ) ; 

			exc = new X509Certificate2Collection() ; 
			cert = GetX509(0) ; 
			exc.Add( cert ) ; 
			raw = exc.Export( X509ContentType.SerializedCert ) ; 
			exc = new X509Certificate2Collection() ; 
			exc.Import( raw ) ; 
			rv = exc.Contains( cert ) ; 
			Eval( rv , "Export SerializedCert" ) ; 

		}
		
		public void Test027()
			{
				X509Certificate2Collection exc = new X509Certificate2Collection() ; 
				rv = 0 == X509Certificate2UI.SelectFromCollection( exc, "Test027" , "Check this out" , X509SelectionFlag.MultiSelection , (IntPtr) 0 ).Count ; 
				Eval( rv , "Select null collection" ) ; 

				try
					{
				exc = new X509Certificate2Collection() ; 
				exc.Add( new X509Certificate2() ) ; 
				rv = 0 == X509Certificate2UI.SelectFromCollection( exc, null , null , X509SelectionFlag.MultiSelection , (IntPtr) 0 ).Count ; 
					}
				catch( Exception )
					{
					rv = true ;  //BUG 111005 Resolved Won't Fix
					}
				Eval( rv , "Select collection with empty cert" ) ; 

				exc = Test043PreRun( false ) ; 
				rv = 1 == X509Certificate2UI.SelectFromCollection( exc, "Select one" , null , X509SelectionFlag.SingleSelection ).Count ; 
				Eval( rv , "Select single" ) ; 
				rv = 1 <= X509Certificate2UI.SelectFromCollection( exc, "Select Multi" , null , X509SelectionFlag.MultiSelection ).Count ; 
				rv = 0 == X509Certificate2UI.SelectFromCollection( exc, "Press cancel" , null , X509SelectionFlag.SingleSelection ).Count ; 
				Eval( rv , "Select single - none " ) ; 
				rv = 0 == X509Certificate2UI.SelectFromCollection( exc, "Press Cancel" , null , X509SelectionFlag.MultiSelection ).Count ; 
				Eval( rv , "Select multi - none " ) ; 
				rv = X509Certificate2UI.SelectFromCollection(
						X509Certificate2UI.SelectFromCollection(
							X509Certificate2UI.SelectFromCollection( exc, "Select two" , null , X509SelectionFlag.MultiSelection ) ,
							"SelectOne" , 
							null , 
							X509SelectionFlag.MultiSelection) ,
						null , 
						null , 
						X509SelectionFlag.SingleSelection ).Count == 1;
				Eval( rv , "Select to one" ) ; 

				try
					{
					X509Certificate2UI.SelectFromCollection( null , null , null , X509SelectionFlag.MultiSelection , (IntPtr) 0 ) ;
					rv =false ; 
					}
				catch(ArgumentNullException)
					{
					rv = true ; 
					}
				catch(Exception)
					{
					rv = false ; 
					}
				finally
					{
					Eval(rv , "X509Certificate2UI.SelectFromCollection( null , null , null , X509SelectionFlag.MultiSelection , (IntPtr) 0 ) ;" ) ; 
					}
				Test043PostRun( false , exc ) ; 

			}
		#endregion
		#region X509Store test cases
		/// <summary>
		/// Test the X509Store constructors
		/// </summary>
		public void Test028()
		{
			X509Store store = null ; 

			store = new X509Store() ; 
			Eval( store.Name == "MY" && store.Location == StoreLocation.CurrentUser , "Call default constructor" ) ; 

			store = new X509Store( StoreName.AddressBook ) ; 
			Eval( store.Name == GetStoreName( StoreName.AddressBook ) , "Call public X509Store( StoreName )" ) ; 

			store = new X509Store( StoreName.AuthRoot ) ; 
			Eval( store.Name == GetStoreName( StoreName.AuthRoot) , "Call public X509Store( StoreName )" ) ; 

			store = new X509Store( StoreName.CertificateAuthority ) ; 
			Eval( store.Name == GetStoreName( StoreName.CertificateAuthority) , "Call public X509Store( StoreName )" ) ; 

			store = new X509Store( StoreName.Disallowed ) ; 
			Eval( store.Name == GetStoreName( StoreName.Disallowed ) , "Call public X509Store( StoreName )" ) ; 

			store = new X509Store( StoreName.My ) ; 
			Eval( store.Name == GetStoreName( StoreName.My ) , "Call public X509Store( StoreName )" ) ; 

			store = new X509Store( StoreName.Root ) ; 
			Eval( store.Name == GetStoreName( StoreName.Root ) , "Call public X509Store( StoreName )" ) ; 

			store = new X509Store( StoreName.TrustedPeople ) ; 
			Eval( store.Name == GetStoreName( StoreName.TrustedPeople ) , "Call public X509Store( StoreName )" ) ; 

			store = new X509Store( StoreName.TrustedPublisher) ; 
			Eval( store.Name == GetStoreName( StoreName.TrustedPublisher) , "Call public X509Store( StoreName )" ) ; 

			store = new X509Store( null , StoreLocation.CurrentUser ) ; 
			Eval( store.Name == null , "store = new X509Store( null , StoreLocation.CurrentUser )" ) ; 

			//StoreLocation tests
			store = new X509Store( StoreLocation.CurrentUser ) ; 
			store.Open( 0 ) ; 
			store.Open( 0 ) ; 
			X509Certificate2Collection c = store.Certificates ; 
			Eval( store.Location == StoreLocation.CurrentUser , "store = new X509Store( StoreLocation.CurrentUser )" ) ; 

			store = new X509Store( StoreLocation.LocalMachine ) ; 
			Eval( store.Location == StoreLocation.LocalMachine , "store = new X509Store( StoreLocation.LocalMachine )" ) ; 

			try
			{
				store = new X509Store( (StoreLocation) 0xFF ) ; // StoreLocation.LocalMachine | StoreLocation.CurrentUser ) ; 
				rv = false ; 
			}
			catch( Exception )
			{
				rv = true ; 
			}
			finally
			{
				Eval( rv , "store = new X509Store( (StoreLocation) 0xFF )" ) ; 
			}

			//invalid storename
			try
				{
				store = new X509Store( (StoreName) 0xFFFF ) ;
				rv = false ; 
				}
			catch( ArgumentException )
				{
				rv = true ; 
				}
			catch( Exception )
				{
				rv = false ; 
				}
			finally
				{
				Eval( rv , "store = new X509Store( (StoreName) 0xFFFF )" ) ; 
				}

			//Store from IntPtr
			store = new X509Store( StoreName.Root , StoreLocation.LocalMachine ) ; 
			store.Open( OpenFlags.MaxAllowed ) ; 
			X509Store createdFrom = new X509Store( store.StoreHandle ) ; 
			try
				{
				createdFrom.Open( OpenFlags.MaxAllowed ) ; 
				rv = false ; 
				}
			catch( ArgumentException )
				{
				rv = true ; 
				}
			catch(Exception e) 
				{
				Console.WriteLine(e) ; 
				rv = false ; 
				}
			finally
				{
				store.Close() ; 
				createdFrom.Close() ; 
				}
			Eval( rv , "Opening a store which is derived from a IntPtr throws ArgumentException" ) ; 


			//Store from IntPtr
			store = new X509Store( StoreName.Root , StoreLocation.LocalMachine ) ; 
			store.Open( OpenFlags.MaxAllowed ) ; 
			X509Certificate2Collection certs = null ; 
			createdFrom = new X509Store( store.StoreHandle ) ; 
			try
				{
				certs = createdFrom.Certificates ; 
				rv = certs != null && certs.Count == store.Certificates.Count ; 
				}
			catch(Exception e) 
				{
				Console.WriteLine(e) ; 
				rv = false ; 
				}
			finally
				{
				store.Close() ; 
				createdFrom.Close() ; 
				}
			Eval( rv , "Creating a store from handle works" ) ; 
			}
		/// <summary>
		/// Test X509Store.Open and X509Store.Close
		/// Open a non-existing store (create one) w/ readonly, then try to Add a cert, get exception
		/// Open a store w/ ReadWrite and add a cert, close the store and open it with ReadOnly and get the cert
		/// Open ExistingOnly throws an exception if the store does not exist
		/// Open ExistingOnly works on an existing store
		/// Try to create a store while denying the permission
		/// Deny Opening a store
		/// Add to a closed store
		/// Close a closed store, no exception
		/// </summary>
		public void Test029(StoreLocation location)
		{
			// Try to create a store while denying the permission
			try
			{
				store = new X509Store( notSoCoolStore , location) ; 
				StorePermission sp = new StorePermission( StorePermissionFlags.CreateStore ) ; 
				sp.Deny() ; 
				store.Open(OpenFlags.ReadOnly) ;
				rv = false ; 
			}
			catch( SecurityException e )
			{
				WriteLine( e.ToString() ) ; 
				rv = true ; 
			}
			finally
			{
				Eval( rv , "Try to create a store while denying the permission" ) ; 
				CodeAccessPermission.RevertDeny() ; 
			}

			// Open ExistingOnly throws an exception if the store does not exist
			try
			{
				store = new X509Store( notSoCoolStore , location) ; 
				store.Open( OpenFlags.OpenExistingOnly ) ; 
				rv = false ; 
			}
			catch( CryptographicException )
			{
				rv = true ; 
			}
			finally
			{
				Eval( rv , "Open(ExistingOnly) throws an exception if the store does not exist" ) ; 
			}

			// Open ExistingOnly works on an existing store
			try
			{
				store = new X509Store( aCoolStore , location) ; 
				store.Open( OpenFlags.ReadWrite ) ; 
				store.Close() ; 
				store.Open( OpenFlags.OpenExistingOnly ) ; 
				store.Close() ;
				rv = true ; 
			}
			catch( Exception e) 
			{
				WriteLine( e.ToString() ); 
				rv = false ; 
			}
			finally
			{
				Eval( rv , "Open(ExistingOnly) works on an existing store" ) ; 
			}

			try
			{
				store = new X509Store( storeName , location) ; 
				cert = GetX509(0) ; 
				store.Open( OpenFlags.ReadOnly ) ; 
				store.Add( cert ) ; 
				rv = false ; 
			}
			catch( CryptographicException ce )
			{
				WriteLine( ce.ToString() ) ; 
				rv = true ; 
			}
			finally
			{
				store.Close() ; 
				Eval( rv , "Open a non-existing store (create one) w/ readonly, then try to Add a cert, get exception" ) ; 
			}
			
			try
			{
				store = new X509Store( storeName , location) ; 
				cert = GetX509(0) ; 
				store.Open( OpenFlags.ReadWrite ) ; 
				store.Add( cert ) ; 
				store.Close() ;
				store.Open( OpenFlags.ReadOnly ) ; 
				rv = store.Certificates.Contains( cert ) ; 
				store.Close() ; 
			}
			catch( CryptographicException ce )
			{
				WriteLine( ce.ToString() ) ; 
				rv = false ; 
			}
			finally
			{
				//clean up the store
				store.Open( OpenFlags.ReadWrite ) ; 
				store.Remove( cert ) ; 
				store.Close() ;
				Eval( rv , "Open a store w/ ReadWrite and add a cert, close the store and open it with ReadOnly and get the cert" ) ; 
			}

			// Deny Opening a store
			try
			{
				StorePermission sp = new StorePermission( StorePermissionFlags.OpenStore ) ; 
				sp.Deny() ; 
				store = new X509Store( storeName , location) ; 
				store.Open( OpenFlags.ReadWrite ) ; 
				rv = false ; 
			}
			catch( SecurityException e )
			{
				WriteLine( e.ToString() ) ; 
				rv = true ; 
			}
			finally
			{
				Eval( rv , "Deny Opening a store" ) ; 
				CodeAccessPermission.RevertDeny() ; 
			}

			//Add to a closed store
			try
			{
				store = new X509Store( storeName , location) ; 
				store.Open( OpenFlags.ReadWrite ) ; 
				store.Close() ; 
				store.Add( new X509Certificate2() ) ;
				rv = false ; 
			}
			catch( CryptographicException e )
			{
				WriteLine( e.ToString() ) ; 
				rv = true ; 
			}
			finally
			{
				Eval( rv , "Add to a closed store" ) ; 
			}

			//Get certificates from a Closed store, exception
			try
			{
				store = new X509Store( storeName , location) ; 
				store.Open( OpenFlags.ReadWrite ) ; 
				store.Close() ; 
				X509Certificate2Collection exc = store.Certificates ;
				rv = true ; 
				foreach( X509Certificate2 c in exc )
					rv = false ; 
			}
			catch( CryptographicException e )
			{
				WriteLine( e.ToString() ) ; 
				rv = false ; 
			}
			catch( Exception e )
				{
				WriteLine( e.ToString() ) ; 
				rv = false ; 
				}
			finally
			{
				Eval( rv , "Get certificates from a Closed store, exception" ) ; 
			}

			//Close a closed store, no exception
			try
			{
				store = new X509Store( storeName , location) ; 
				store.Open( OpenFlags.ReadWrite ) ; 
				store.Close() ; 
				store.Close() ; 
				rv = true ; 
			}
			catch( CryptographicException e )
			{
				WriteLine( e.ToString() ) ; 
				rv = false ; 
			}
			finally
			{
				Eval( rv , "Close a closed store, no exception" ) ; 
			}
		}
		/// <summary>
		/// Test the X509Store properties, methods
		/// Deny AddToStore permission, Add to the store, exception
		/// Deny RemoveFromStore permission, Remove from the store, exception
		/// Deny EnumerateCertificates, get the Certificates property, exception
		/// Add( null ) throws ArgumentNullException
		/// Remove( null ) throws ArgumentNullException
		/// AddRange( null ) throws ArgumentNullException
		/// RemoveRange( null ) throws ArgumentNullException
		/// Certificates in an empty store
		/// Certificates in a filled store
		/// AddRange, RemoveRange verify with Certificates
		/// </summary>
		public void Test031(StoreLocation location, bool useCopy)
		{
			StorePermission sp = null ; 
			X509Store storeCopy = null ; 
			// Deny AddToStore permission, Add to the store, exception
			try
			{
				sp = new StorePermission( StorePermissionFlags.AddToStore ) ; 
				sp.Deny() ; 
				store = new X509Store( storeName , location) ; 
				store.Open( OpenFlags.ReadWrite ) ; 
				if( useCopy )
					{
					storeCopy = store ; 
					store = new X509Store( storeCopy.StoreHandle ) ; 
					storeCopy.Close();
					}
				store.Add( GetX509( 0 ) ) ; 
				rv = false ; 
			}
			catch( SecurityException e )
			{
				WriteLine( e.ToString() ) ; 
				rv = true ; 
			}
			finally
			{
				store.Close() ; 
				Eval( rv , "Deny AddToStore permission, Add to the store, exception" ) ; 
				CodeAccessPermission.RevertDeny() ; 
			}

			// Deny RemoveFromStore permission, Remove from the store, exception
			try
			{
				sp = new StorePermission( StorePermissionFlags.RemoveFromStore ) ; 
				sp.Deny() ; 
				store = new X509Store( storeName , location) ; 
				store.Open( OpenFlags.ReadWrite ) ; 
				if( useCopy )
					{
					storeCopy = store ; 
					store = new X509Store( storeCopy.StoreHandle ) ; 
					storeCopy.Close();
					}
				cert = GetX509(0) ; 
				store.Add( cert ) ; 
				store.Remove( cert ) ; 
				rv = false ; 
			}
			catch( SecurityException e )
			{
				WriteLine( e.ToString() ) ; 
				rv = true ; 
			}
			finally
			{
				store.Close() ; 
				Eval( rv , "Deny RemoveFromStore permission, Remove from the store, exception" ) ; 
				CodeAccessPermission.RevertDeny() ; 
			}

			//MyCoolStore should now have a cert in it, so Deny EnumerateCertificates
			try
			{
				sp = new StorePermission( StorePermissionFlags.EnumerateCertificates ) ; 
				sp.Deny() ; 
				store = new X509Store( storeName , location) ; 
				store.Open( OpenFlags.ReadWrite ) ; 
				if( useCopy )
					{
					storeCopy = store ; 
					store = new X509Store( storeCopy.StoreHandle ) ; 
					storeCopy.Close();
					}
				X509Certificate2Collection certs = store.Certificates ; 
				rv = false ; 
			}
			catch( SecurityException e )
			{
				WriteLine( e.ToString() ) ; 
				rv = true ; 
			}
			finally
			{
				//First Revert the DEny and then remove the certificate
				CodeAccessPermission.RevertDeny() ; 
				store.Remove( cert ) ; 
				store.Close() ; 
				Eval( rv , "Deny EnumerateCertificates permission, call X509Store.Certificates, exception" ) ; 
			}

			// Add( null ) throws ArgumentNullException
			try
			{
				store = new X509Store( storeName , location) ; 
				store.Open( OpenFlags.ReadWrite ) ; 
				if( useCopy )
					{
					storeCopy = store ; 
					store = new X509Store( storeCopy.StoreHandle ) ; 
					storeCopy.Close();
					}
				store.Add( null ) ; 
			}
			catch( ArgumentNullException )
			{
				rv = true ; 
			}
			catch( Exception ) 
			{
				//rv = true ; 
				rv = false ; 
			}
			finally
			{
				store.Close() ; 
				Eval( rv , "X509Store.Add( null ) throws ArgumentNullException" ) ; 
			}
			
			// Remove( null ) throws ArgumentNullException
			try
			{
				store = new X509Store( storeName, location ) ; 
				store.Open( OpenFlags.ReadWrite ) ; 
				if( useCopy )
					{
					storeCopy = store ; 
					store = new X509Store( storeCopy.StoreHandle ) ; 
					storeCopy.Close();
					}
				store.Remove( null ) ; 
			}
			catch( ArgumentNullException )
			{
				rv = true ; 
			}
			catch( Exception ) 
			{
				//rv = true ; 
				rv = false ; 
			}
			finally
			{
				store.Close() ; 
				Eval( rv , "X509Store.Remove( null ) throws ArgumentNullException" ) ; 
			}

			// AddRange( null ) throws ArgumentNullException
			try
			{
				store = new X509Store( storeName, location ) ; 
				store.Open( OpenFlags.ReadWrite ) ; 
				if( useCopy )
					{
					storeCopy = store ; 
					store = new X509Store( storeCopy.StoreHandle ) ; 
					storeCopy.Close();
					}
				store.AddRange( null ) ; 
			}
			catch( ArgumentNullException )
			{
				rv = true ; 
			}
			catch( Exception ) 
			{
				//rv = true ; 
				rv = false ; 
			}
			finally
			{
				store.Close() ; 
				Eval( rv , "X509Store.AddRange( null ) throws ArgumentNullException" ) ; 
			}

			// RemoveRange( null ) throws ArgumentNullException
			try
			{
				store = new X509Store( storeName, location ) ; 
				store.Open( OpenFlags.ReadWrite ) ; 
				if( useCopy )
					{
					storeCopy = store ; 
					store = new X509Store( storeCopy.StoreHandle ) ; 
					storeCopy.Close();
					}
				store.RemoveRange( null ) ; 
			}
			catch( ArgumentNullException )
			{
				rv = true ; 
			}
			catch( Exception ) 
			{
				//rv = true ; 
				rv = false ; 
			}
			finally
			{
				store.Close() ; 
				Eval( rv , "X509Store.RemoveRange( null ) throws ArgumentNullException" ) ; 
			}

			// Certificates in an empty store
			try
			{
				store = new X509Store( storeName, location ) ; 
				store.Open( OpenFlags.ReadWrite ) ; 
				if( useCopy )
					{
					storeCopy = store ; 
					store = new X509Store( storeCopy.StoreHandle ) ; 
					storeCopy.Close();
					}
				X509Certificate2Collection exc = store.Certificates ; 
				cert = exc[0] ;
			}
			catch( ArgumentOutOfRangeException ) 
			{
				rv = true ; 
			}
			finally
			{
				store.Close() ; 
				Eval( rv , "Certificates.Get in an empty store return is valid" ) ; 
			}

			// Certificates in a filled store
			try
			{
				store = new X509Store( storeName , location) ; 
				store.Open( OpenFlags.ReadWrite ) ; 
				if( useCopy )
					{
					storeCopy = store ; 
					store = new X509Store( storeCopy.StoreHandle ) ; 
					storeCopy.Close();
					}
				store.Add( GetX509(0) ) ; 
				store.Add( new X509Certificate2( "x509.cer" ) ) ; 
				X509Certificate2Collection exc = store.Certificates ; 
				rv = exc.Contains( exc[0] ) && exc.Contains( exc[1] ) ;
			}
			catch( Exception e ) 
			{
				WriteLine( e.ToString() ) ;
				rv = false ; 
			}
			finally
			{
				store.RemoveRange( store.Certificates ) ; 
				store.Close() ; 
				Eval( rv , "Certificates.Get in an empty store return is valid" ) ; 
			}

			// AddRange, RemoveRange verify with Certificates
			try
			{
				X509Certificate2Collection exc = new X509Certificate2Collection() ; 
				X509Certificate2Collection exc2 = new X509Certificate2Collection() ; 
				exc.Add( GetX509( 0 ) ) ; 
				exc.Add( new X509Certificate2( "x509.cer" ) ) ; 

				store = new X509Store( storeName , location) ; 
				store.Open( OpenFlags.ReadWrite ) ; 
				if( useCopy )
					{
					storeCopy = store ; 
					store = new X509Store( storeCopy.StoreHandle ) ; 
					storeCopy.Close();
					}
				store.AddRange( exc ) ;
				exc2 = store.Certificates ; 
				WriteLine( exc2[0].ToString( true ) ); 
				WriteLine( exc2[1].ToString( true ) ) ; 
				rv = store.Certificates.Contains( exc[0] ) && store.Certificates.Contains( exc[1] ) ;
				store.RemoveRange( exc ) ; 
				exc2 = store.Certificates ; 
				rv = rv && !store.Certificates.Contains( exc[0] ) && !store.Certificates.Contains( exc[1] ) ; 
			}
			catch( Exception e ) 
			{
				WriteLine( e.ToString() ) ;
				rv = false ; 
			}
			finally
			{
				store.Close() ; 
				Eval( rv , "AddRange, RemoveRange verify with Certificates" ) ; 
			}

			//AddRange/RemoveRange with Exception
			X509Certificate2Collection certsColl = new X509Certificate2Collection() ; 
			certsColl.Add( GetX509(0) ) ;
			certsColl.Add( new X509Certificate2() ) ; 

			try
				{
				store = new X509Store(StoreName.My,location) ; 
				store.Open(OpenFlags.ReadWrite) ; 
				if( useCopy )
					{
					storeCopy = store ; 
					store = new X509Store( storeCopy.StoreHandle ) ; 
					storeCopy.Close();
					}
				store.AddRange( certsColl ) ;
				rv = false ;
				}
			catch( Exception )
				{
				rv = true ; 
				}
			finally
				{
				rv = rv && !store.Certificates.Contains( certsColl[0] ) ; 
				store.Close() ; 
				Eval( rv , "AddRange with Exception and abort transaction" ) ; 
				}

			try
				{
				store = new X509Store(StoreName.My,location) ; 
				store.Open(OpenFlags.ReadWrite) ; 
				if( useCopy )
					{
					storeCopy = store ; 
					store = new X509Store( storeCopy.StoreHandle ) ; 
					storeCopy.Close();
					}
				store.Add( certsColl[0] ) ;
				store.Close() ; 
				if( useCopy )
					{
					storeCopy.Open( OpenFlags.ReadOnly ) ; 
					store = new X509Store( storeCopy.StoreHandle ) ; 
					storeCopy.Close();
					}
				else
					store.Open( OpenFlags.ReadOnly ) ; 
				store.RemoveRange( certsColl ) ;
				rv = false ;
				Console.WriteLine( "No exception" ) ; 
				}
			catch( Exception )
				{
				rv = true ; 
				}
			finally
				{
				rv = rv && store.Certificates.Contains( certsColl[0] ) ; 
				store.Close() ; 
				Eval( rv , "RemoveRange with Exception and abort transaction" ) ; 
				}
		}		
		#endregion
		#region Oid and OidCollection test cases
		/// <summary>
		/// Oid() get methods returns null
		/// Oid( "Encrypting File System" ) , verify Value and FriendlyName
		/// Oid( "invalid" ) , Value = "invalid", FriendlyName = String.Empty
		/// Oid( "test" , "test" ) , verify Value and FriendlyName
		/// Oid( new Oid( "test" , "test" ) , verify Value and FriendlyName
		/// Oid.Value = "invalid" , FriendlyName = "" 
		/// Oid.FriendlyName = "invalid" , Value = "" 
		/// Oid.Value = "1.3.6.1.5.5.7.3.4" , FriendlyName = SecureEmail
		/// Oid.FriendlyName = Client Authentication, Value = 1.3.6.1.5.5.7.3.2
		/// </summary>
		public void Test032()
		{
			Oid oid = null ; 
			string v = "" , f = "" ; 

			// Oid() get methods returns null
			oid = new Oid() ; 
			Eval( OidCheck( oid , null , null ) , "Oid() get methods returns null" ) ; 

			// Oid( "Encrypting File System" ) , verify Value and FriendlyName
			f = "Encrypting File System" ;
			v = "1.3.6.1.4.1.311.10.3.4" ;
			oid = new Oid( f ) ;
			Eval( OidCheck( oid , v , (new Oid(v)).FriendlyName ) , "Oid( \"Encrypting File System\" ) , verify Value and FriendlyName" ) ; 

			// Oid( "invalid" ) , FriendlyName = "invalid", Value = String.Empty
			f = "invalid" ;
			v = "invalid" ; 
			oid = new Oid( f ) ; 
			Eval( OidCheck( oid , v , null ) , "Oid( \"invalid\" ) , FriendlyName = null, Value = invalid" ) ; 

			// Oid( "test" , "test" ) , verify Value and FriendlyName
			f = "test100" ; 
			v = "test100" ; 
			oid = new Oid( v , f ) ; 
			Eval( OidCheck( oid , v , f ) , "Oid( \"test100\" , \"test100\" ) , verify Value and FriendlyName" ) ; 

			// Oid( new Oid( "test" , "test" ) , verify Value and FriendlyName
			f = "test100" ; 
			v = "test100" ; 
			oid = new Oid( new Oid( v , f ) ) ; 
			Eval( OidCheck( oid , v , f ) , "Oid( new Oid( \"test100\" , \"test100\" ) ), verify Value and FriendlyName" ) ; 

			// Oid.Value = "invalid" , FriendlyName = "" 
			f = null ; //String.Empty ; 
			v = "invalid" ; 
			oid = new Oid() ; 
			oid.Value = v ; 
			Eval( OidCheck( oid , v , f ) , "Oid.Value = \"invalid\" , FriendlyName = \"\"" ) ; 

			// Oid.FriendlyName = "invalid" , Value = "" 
			v = null ; //String.Empty ; 
			f = "invalid" ; 
			oid = new Oid() ; 
			oid.FriendlyName = f ; 
			Eval( OidCheck( oid , v , f ) , "Oid.FriendlyName = \"invalid\" , Value = \"\"" ) ; 

			// Oid.Value = "1.3.6.1.5.5.7.3.4" , FriendlyName = SecureEmail
			v = "1.3.6.1.5.5.7.3.4" ;
			f = "Secure Email" ;
			oid = new Oid() ;
			oid.Value = v ; 
			Eval( OidCheck( oid , v , (new Oid(v)).FriendlyName ) , "Oid.Value = \"1.3.6.1.5.5.7.3.4\" , FriendlyName = Secure Email" ) ; 

			// Oid.FriendlyName = Client Authentication, Value = 1.3.6.1.5.5.7.3.2
			v = "1.3.6.1.5.5.7.3.2" ;
			f = "Client Authentication" ;
			oid = new Oid() ;
			oid.FriendlyName = f ; 
			Eval( OidCheck( oid , v , new Oid(v).FriendlyName ) , "Oid.Value = \"1.3.6.1.5.5.7.3.2\" , FriendlyName = Client Authentication" ) ; 
		}
		/// <summary>
		/// Test CopyTo a single array element
		/// Test CopyTo a multi array element
		/// Test CopyTo array = null 
		/// CopyTo index == array.Length
		/// CopyTo index = array.Length - 1, Collection.Count == 2
		/// Count returns 0 when empty 
		/// Count returns valid number
		/// IsSynchronized returns false
		/// Object.ReferenceEquals( oidCollection.SyncRoot , oidCollection ) 
		/// this["test"] returns the right value
		/// this["Client Authentication"] returns the right value
		/// this["1.3.6.1.5.5.7.3.4"]
		/// </summary>
		public void Test033()
		{
			Oid[] aOid = new Oid[1] ; 
			OidCollection oc = null ; 
			int len = 0 ; 
			Oid oid = null ; 
			string test = "" ; 

			// this["test"] returns the right value
			oc = OidHelper( 3 ) ; 
			oid = new Oid( "test100" , "test100" ) ; 
			oc.Add( oid ) ; 
			Eval( OidEquals( oc["test100"] , oid ) , "this[\"test100\"] returns the right value" ) ; 

			// this["Client Authentication"] returns the right value
			oc = OidHelper( 0 ) ; 
			test = "Client Authentication" ;
			oid = new Oid( test ) ; 
			oc.Add( oid ) ; 
			Eval( OidEquals( oc[test] , oid ) , "this[\"Client Authentication\"] returns the right value" ) ; 

			// this["1.3.6.1.5.5.7.3.4"]
			oc = OidHelper( 0 ) ; 
			test = "1.3.6.1.5.5.7.3.4" ;
			oid = new Oid( test ) ; 
			oc.Add( oid ) ; 
			Eval( OidEquals( oc[test] , oid ) , "this[\"1.3.6.1.5.5.7.3.4\"] returns the right value" ) ; 

			// Test CopyTo a single array element
			try
			{
				oc = OidHelper(1) ; 
				oc.CopyTo( aOid , 0 ) ; 
				rv = OidEquals( aOid[0] , oc[0] ) ; 
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ; 
			}
			finally
			{
				Eval( rv , "Test OidCollection.CopyTo(), single element" ) ;
			}

			// Test CopyTo a multi array element
			try
			{
				len = 10 ; 
				oc = OidHelper(len) ; 
				aOid = new Oid[len] ; 
				oc.CopyTo( aOid , 0 ) ; 
				for( int i = 0 ; i < len ; i++ ) 
				{
					rv = OidEquals( aOid[i] , oc[i] ) ;
					if( !rv )
						break ; 
				}
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ; 
			}
			finally
			{
				Eval( rv , "Test OidCollection.CopyTo(), multiple elements" ) ;
			}

			// Test CopyTo array = null 
			try
			{
				oc = OidHelper( 1 ) ; 
				oc.CopyTo( null , 0 ) ; 
				rv = false ; 
			}
			catch( ArgumentNullException )
			{
				rv = true ; 
			}
			finally
			{
				Eval( rv , "Test CopyTo array = null" ) ; 
			}

			// CopyTo index == array.Length
			try
			{
				len = 1 ; 
				oc = OidHelper ( 3 ) ; 
				aOid = new Oid[ len ] ; 
				oc.CopyTo( aOid , len ) ; 
				rv = false ; 
			}
			catch( ArgumentOutOfRangeException )
			{
				rv = true ; 
			}
			finally
			{
				Eval( rv , "CopyTo index == array.Length" ) ; 
			}

			// CopyTo index = array.Length - 1, Collection.Count == 2
			try
			{
				len = 3 ; 
				oc = OidHelper ( len - 1 ) ; 
				aOid = new Oid[ len ] ; 
				oc.CopyTo( aOid , len - 1) ; 
				rv = false ; 
			}
			catch( ArgumentException )
			{
				rv = true ; 
			}
			finally
			{
				Eval( rv , "CopyTo index = array.Length - 1, Collection.Count == 2" ) ; 
			}

			// Count returns 0 when empty 
			oc = OidHelper( 0 ) ;
			Eval( oc.Count == 0 , "Count returns 0 when empty" ) ; 

			// Count returns valid number
			len = 25 ;
			oc = OidHelper( len ) ; 
			Eval( len == oc.Count , "Count returns valid number" ) ; 
			
			// IsSynchronized returns false
			oc = OidHelper( 1 ) ; 
			Eval( !oc.IsSynchronized , "OidCollection.IsSynchronized returns false" ) ; 

			// Object.ReferenceEquals( oidCollection.SyncRoot , oidCollection ) 
			oc = OidHelper( 3 ) ; 
			Eval( Object.ReferenceEquals( oc.SyncRoot , oc ) , "OidCollection.SyncRoot returns this" ) ; 
		}
		public OidCollection OidHelper( int test )
		{
			OidCollection oc = new OidCollection() ;  

			if( test > 0 )
			{
				for( int i = 0 ; i < test ; i++ )
				{
					oc.Add( new Oid( "test" + i.ToString() , "test" + i.ToString() ) ) ;
				}
			}
			return oc ; 
		}
		public bool OidEquals( Oid a , Oid b ) 
		{
			return a.Value == b.Value && a.FriendlyName == b.FriendlyName ;
		}
		public bool OidCheck( Oid oid , string val , string name )
		{
			return oid.Value == val && oid.FriendlyName == name ; 
		}
		#endregion
		#region X509Extension test cases
		/// <summary>
		/// Verify we can subclass X509Extension
		/// </summary>
		public void Test034()
		{
			X509ExtensionTest test = new X509ExtensionTest() ; 
			Eval( test.Test() , "Verify we can subclass X509Extension" ) ; 
		}
		/// <summary>
		/// Test for X509BasicConstraintsExtension
		/// </summary>
		public void Test035()
		{
			X509BasicConstraintsExtension ext = null ;
			string testStr = String.Empty ; 
			string friendlyName = "Basic Constraints" ;
			string oldOidValue = "2.5.29.10" ; 

			//Use NoSKI
			cert = new X509Certificate2( "NoSKI.cer" ) ; 
			ext = (X509BasicConstraintsExtension) cert.Extensions[friendlyName] ;
			rv = !ext.CertificateAuthority ; 
			rv = rv && !ext.HasPathLengthConstraint ; 
			rv = rv && ext.PathLengthConstraint == 0 ; 
			Eval( rv , "Get valid X509BasicConstraintsExtension from NoSKI.cer" ) ; 

			TestBasic( ext , "NoSKI.cer" , false) ; 

			//Use dns.cer
			cert = new X509Certificate2( "dns.cer" ) ; 
			ext = (X509BasicConstraintsExtension) cert.Extensions[friendlyName] ;
			rv = !ext.HasPathLengthConstraint ; 
			rv = rv && !ext.CertificateAuthority ; 
			rv = rv && ext.PathLengthConstraint == 0 ; 
			Eval( rv , "Get valid X509BasicConstraintsExtension from dns.cer" ) ; 

			TestBasic( ext , "dns.cer" ,false) ; 

			//Use coolcert.cer
			cert = new X509Certificate2( "CoolCert.cer" ) ; 
			ext = (X509BasicConstraintsExtension) cert.Extensions[friendlyName] ;
			rv = ext.PathLengthConstraint == 8 ; 
			rv = rv && ext.CertificateAuthority ; 
			rv = rv && ext.HasPathLengthConstraint ; 
			Eval( rv , "Get valid X509BasicConstraintsExtension from CoolCert.cer" ) ; 

			TestBasic( ext , "CoolCert.cer" ,true ) ; 

			//Use balor.crt will have no subject key
			cert = new X509Certificate2( "balor.crt" ) ; 
			ext = (X509BasicConstraintsExtension) cert.Extensions[friendlyName] ;
			Eval( ext == null , "Get X509BasicConstraintsExtension from balor.crt Extension Cllection, return null" ) ; 

			//use BS1.crt to get the information
			cert = new X509Certificate2( "bs1.cer" ) ; 
			ext = (X509BasicConstraintsExtension) cert.Extensions[oldOidValue] ;
			rv = !ext.HasPathLengthConstraint && ext.PathLengthConstraint == 0 ||  ext.HasPathLengthConstraint && ext.PathLengthConstraint > 0 ; 
			Eval( rv , "Call DecodeExtension with an old BasicConstraint Oid" )  ;
			
		}
		protected void TestBasic( X509BasicConstraintsExtension ext1 , string cert , bool critical )
		{
			X509BasicConstraintsExtension extCopy = null ; 
			string friendlyName = "Basic Constraints" ;

			Eval( critical == ext1.Critical , cert + ":  X509BasicConstraintsExtension.Critical returns false" ) ; 

			Oid oid = ext1.Oid ; 
			Eval( oid != null && oid.FriendlyName == friendlyName , cert + ":  X509BasicConstraintsExtension.Oid.FriendlyName is valid" ) ; 

			//Get the AsnEncodedData
			AsnEncodedData asn = ext1 ;  //TODO fix this
			Eval( asn != null , "X509BasicConstraintsExtension.EncodedExtension is valid" ) ; 

			//Copy the key
			extCopy = new X509BasicConstraintsExtension() ; 
			extCopy.CopyFrom( ext1 ) ; 
			rv = extCopy.Critical == ext1.Critical ;
			rv = rv && extCopy.Oid.FriendlyName == ext1.Oid.FriendlyName ;
			rv = rv && extCopy.CertificateAuthority == ext1.CertificateAuthority ;
			rv = rv && extCopy.HasPathLengthConstraint  == ext1.HasPathLengthConstraint ;
			rv = rv && extCopy.PathLengthConstraint  == ext1.PathLengthConstraint ;
			Eval( rv , "X509BasicConstraintsExtension.CopyFrom works" ) ; 
		}
		/// <summary>
		/// Test for X509EnhancedKeyUsageExtension
		/// </summary>
		public void Test036()
		{
			X509EnhancedKeyUsageExtension key = null , keyCopy = null ; 
			string testStr = String.Empty ; 
			string friendlyName = "Enhanced Key Usage" ;
		
			//Use dss.cer
			cert = new X509Certificate2( "dss.cer" ) ; //dss has two!
			key = (X509EnhancedKeyUsageExtension) cert.Extensions[ friendlyName ] ;
			rv = key.EnhancedKeyUsages["Secure Email"] != null ;
			rv = rv && null != key.EnhancedKeyUsages["1.3.6.1.5.5.7.3.2"] ;
			rv = rv && null == key.EnhancedKeyUsages["Invalid"] ;
			//We have a collection of enhanced key usages
			Eval( rv , "Get the X509EnhancedKeyUsageExtension from dss.cer" ) ; 

			//check to see if Critical
			Eval( !key.Critical , "X509EnhancedKeyUsageExtension.Critical returns false" ) ; 

			//Get the Oid Value
			Oid oid = key.Oid ; 
			Eval( oid != null && oid.FriendlyName == friendlyName , "X509SubjectKeyIdentifierExtension.Oid.FriendlyName is valid" ) ; 

			//Get the AsnEncodedData
			AsnEncodedData asn = key;//.EncodedExtension ;  TODO fix this
			Eval( asn != null , "X509EnhancedKeyUsageExtension.EncodedExtension is valid" ) ; 

			//Copy the key
			keyCopy = new X509EnhancedKeyUsageExtension() ; 
			keyCopy.CopyFrom( key ) ; 
			rv = keyCopy.EnhancedKeyUsages["Secure Email"] != null ;
			rv = rv && null != keyCopy.EnhancedKeyUsages["1.3.6.1.5.5.7.3.2"] ;
			rv = rv && null == keyCopy.EnhancedKeyUsages["Invalid"] ;
			Eval( rv , "X509EnhancedKeyUsageExtension.CopyFrom works" ) ; 

			//Use balor.crt will have no subject key
			cert = new X509Certificate2( "balor.crt" ) ; 
			key = (X509EnhancedKeyUsageExtension) cert.Extensions[friendlyName] ;
			Eval( key == null , "Get X509EnhancedKeyUsageExtension from balor.crt Extension Cllection, return null" ) ; 
		}
		/// <summary>
		/// Test for X509SubjectKeyIdentifierExtension
		/// </summary>
		public void Test037()
		{
			X509SubjectKeyIdentifierExtension key = null , keyCopy = null ; 
			string testStr = String.Empty ; 
			string friendlyName = "Subject Key Identifier" ;
		
			//Use balor.crt
			cert = new X509Certificate2( "balor.crt" ) ; 
			key = (X509SubjectKeyIdentifierExtension) cert.Extensions[ friendlyName ] ;
			testStr = key.SubjectKeyIdentifier ; 
			Eval( testStr == "8DA8F508C2DD1964" , "Get the SubjectKeyIdentifier from Balor.crt" ) ; 

			//check to see if Critical
			Eval( !key.Critical , "X509SubjectKeyIdentifierExtension.Critical returns false" ) ; 

			//Get the Oid Value
			Oid oid = key.Oid ; 
			Eval( oid != null && oid.FriendlyName == friendlyName , "X509SubjectKeyIdentifierExtension.Oid.FriendlyName is valid" ) ; 

			//Get the AsnEncodedData
			AsnEncodedData asn = key;//.EncodedExtension ; TODO fix this!!
			Eval( asn != null , "X509SubjectKeyIdentifierExtension.EncodedExtension is valid" ) ; 

			//Copy the key
			keyCopy = new X509SubjectKeyIdentifierExtension() ; 
			keyCopy.CopyFrom( key ) ; 
			Eval( keyCopy.SubjectKeyIdentifier == key.SubjectKeyIdentifier , "X509SubjectKeyIdentifierExtension.CopyFrom works" ) ; 

			//Use NoSKI.cer will have no subject key
			cert = new X509Certificate2( "NoSKI.cer" ) ; 
			key = (X509SubjectKeyIdentifierExtension) cert.Extensions[friendlyName] ;
			Eval( key == null , "Get SubjectKeyIdentifier from NoSKI.cer Extension Cllection, return null" ) ; 

			//Negative this tests
			try
				{
				cert = new X509Certificate2( "NoSKI.cer" ) ; 
				object o = cert.Extensions[-1] ; 
				rv = false ; 
				}
			catch( Exception )
				{
				rv = true ; 
				}
			finally
				{
				Eval( rv , "cert.Extensions[-1] ; " ) ; 
				}

			try
				{
				cert = new X509Certificate2( "NoSKI.cer" ) ; 
				object o = cert.Extensions[cert.Extensions.Count] ; 
				rv = false ; 
				}
			catch( Exception )
				{
				rv = true ; 
				}
			finally
				{
				Eval( rv , "cert.Extensions[cert.Extensions.Count]" ) ; 
				}

			try
				{
				cert = new X509Certificate2( "NoSKI.cer" ) ; 
				cert.Extensions[0].CopyFrom( null ) ;  
				rv = false ; 
				}
			catch( ArgumentException )
				{
				rv = true ; 
				}
			catch( Exception e )
				{
				Console.WriteLine( e ) ; 
				rv = false ; 
				}
			finally
				{
				Eval( rv , "cert.Extensions[0].CopyFrom( null )" ) ; 
				}
			
			Test038() ; 
		}
		//test the extension collection and enumerator
		public void Test038()
			{
			cert = GetX509( 0 ) ; 
			IEnumerator pie = ( (IEnumerable) cert.Extensions ).GetEnumerator() ; 
			pie.Reset() ; 
			while( pie.MoveNext() )
				{
				X509Extension ex = (X509Extension ) pie.Current ; 
				}
			rv = true ; 
			Eval( rv , "Test X509ExtensionEnumerator.Reset, Current" ) ; 

			rv = cert.Extensions.SyncRoot == cert.Extensions && !cert.Extensions.IsSynchronized ; 
			Eval( rv , "cert.Extensions.SyncRoot == cert.Extensions && !cert.Extensions.IsSynchronized" ) ; 

			X509Extension[] a = new X509Extension[0] ; 

			try
				{
				cert.Extensions.CopyTo( null , 0 ) ; 
				rv = false ; 
				}
			catch( ArgumentNullException ) 
				{
				rv = true  ; 
				}
			Eval( rv , "cert.Extensions.CopyTo( null , 0 )" ) ; 

			try
				{
				cert.Extensions.CopyTo( a , 0 ) ; 
				rv = false ; 
				}
			catch( ArgumentOutOfRangeException ) 
				{
				rv = true  ; 
				}
			Eval( rv , "cert.Extensions.CopyTo( a.Len = 0 , 0 )" ) ; 

			a = new X509Extension[1] ;
			try
				{
				cert.Extensions.CopyTo( a , 0 ) ; 
				rv = false ; 
				}
			catch( ArgumentException ) 
				{
				rv = true  ; 
				}
			Eval( rv , "cert.Extensions.CopyTo( a.Lenght = 1, 0 )" ) ; 

			a = new X509Extension[cert.Extensions.Count] ;
			try
				{
				cert.Extensions.CopyTo( a , 0 ) ; 
				rv = true ; 
				}
			catch( Exception )
				{
				rv = false ; 
				}
			Eval( rv , "cert.Extensions.CopyTo( a.Len == Count , 0 )" ) ; 
			}
		#endregion
		#region X509Chain test cases
		/// <summary>
		/// Set and Get the X509ChainPolicy object
		/// Setting test cases are covered in ChainPolicy cases, here we test to see how it influences the chain build
		/// </summary>
		public void Test040()
		{
			X509Chain chain = null ; 
			chain = X509Chain.Create() ; 
			Eval( chain != null , "chain = X509Chain.Create()" ) ;

			X509ChainStatus[] status = chain.ChainStatus ; 
			Eval( status.Length == 0 , "Test040:  Get an empty array" ) ; 

			//New X509Chian constructor w/ invalid handle			
			chain = new X509Chain(false) ; 
			try
				{
				chain = new X509Chain( chain.ChainContext ) ; 
				rv = false ; 
				}
			catch(ArgumentNullException)
				{
				rv = true ; 
				}
			catch(Exception e)
				{
				Console.WriteLine(e) ; 
				rv = false ; 
				}
			Eval( rv, "New X509Chain constructor w/ invalid handle" ) ; 

			try
				{
				chain = new X509Chain( IntPtr.Zero ) ; 
				rv = false ; 				
				}
			catch(ArgumentNullException)
				{
				rv = true ; 
				}
			catch(Exception e )
				{
				Console.WriteLine(e) ; 
				rv = false ; 
				}
			Eval( rv, "chain = new X509Chain( IntPtr.Zero )" ) ; 

			chain = new X509Chain() ; 
			chain.Build( new X509Certificate2("X509tests\\test14\\End Certificate CP.04.02.crt") ) ; //get a chain that doesnt build
			status = chain.ChainStatus ; 
			chain = new X509Chain(chain.ChainContext) ; 
			X509ChainStatus[] status2 = chain.ChainStatus ;
			Eval( status.Length==status2.Length && status.Length > 0  , "Duplicate chain context" ) ; 

			TestGetChainStatusInformation() ; 

			try
				{
				chain = X509Chain.Create() ; 
				chain.ChainPolicy = null ; 
				rv = false ; 
				}
			catch(ArgumentNullException)
				{
				rv = true ; 
				}
			catch(Exception e)
				{
				Console.WriteLine(e) ; 
				rv = false ; 
				}
			Eval( rv , "chain.ChainPolicy = null ; " ) ; 
		}
		void TestGetChainStatusInformation()
			{
			CallGetChainStatusInformation(0,0) ; 
			CallGetChainStatusInformation(0xFFFFFFFF,32) ; 
			}
		void CallGetChainStatusInformation(uint value , int len )
			{
			MethodInfo mi = typeof(X509Chain).GetMethod( "GetChainStatusInformation" , BindingFlags.Static | BindingFlags.NonPublic ) ;
			X509ChainStatus[] status =	(X509ChainStatus[])mi.Invoke(null , new object[]{value} ) ; 
			Eval( status.Length == len , String.Format( "CallGetChainStatusInformation( {0} , {1} )"  , value  , len ) ) ; 
			}
		/// <summary>
		/// ChainStatus tests cases
		/// </summary>
		public void Test041()
		{
			//Test additions to Chain.Build
			StorePermission sp = new StorePermission( StorePermissionFlags.OpenStore | StorePermissionFlags.EnumerateCertificates ) ; 
			sp.Deny() ; 
			TestBuild() ; 
		}
		void TestBuild() 
			{
			try
				{
				X509Chain.Create().Build( GetX509(0) ) ; 
				rv = false ; 
				}
			catch( SecurityException )
				{
				rv = true ; 
				}
			catch( Exception ) 
				{
				rv = false ; 
				}
			Eval( rv , "X509Chain.Build should demand X509StorePermission for Open and EnumerateCertificates" ) ; 
			}
		/// <summary>
		/// Test ChainElements property
		/// </summary>
		public void Test042()
		{
			//Get a ChainElement collection before building
			X509ChainElementCollection lmnt = null ; 

			lmnt = (new X509Chain(false)).ChainElements ; 
			Eval( lmnt.Count == 0 , "lmnt = (new X509Chain(false)).ChainElements" ) ; 

			lmnt = (new X509Chain(true)).ChainElements ; 
			Eval( lmnt.Count == 0 , "lmnt = (new X509Chain(true)).ChainElements" ) ; 

			//Get the enumeartor for an empty collection
			X509ChainElementEnumerator ceEnum = (new X509Chain(true)).ChainElements.GetEnumerator() ; 
			Eval( ceEnum != null , "Get the enumeartor for an empty collection" ) ; 

			ceEnum = (new X509Chain(false)).ChainElements.GetEnumerator() ; 
			Eval( ceEnum != null , "Get the enumeartor for an empty collection" ) ; 

			//error for this[int]
			try
				{
				lmnt = (new X509Chain(false)).ChainElements ; 
				object o = lmnt[-1];
				}
			catch(InvalidOperationException )
				{
				rv = true ; 
				}
			catch(Exception )
				{
				rv = false ; 
				}
			Eval( rv , "object o = lmnt[-1];" ) ; 

			try
				{
				lmnt =  (new X509Chain(false)).ChainElements ; 
				object o = lmnt[5000];
				}
			catch(ArgumentOutOfRangeException)
				{
				rv = true ; 
				}
			catch(Exception )
				{
				rv = false ; 
				}
			Eval( rv , "object o = lmnt[5000];" ) ; 
		}
		/// <summary>
		/// Build test cases
		/// Build an empty cert
		/// Build a cert w/ 1 level deep
		/// Build a chain w/ 1 element
		/// </summary>
		public void Test043(bool context)
		{
			if( Utility.IsWin9x ) 
				return ; 
			X509Chain chain = new X509Chain(context) ; 
//			X509ChainElementCollection cec = null ; 
			X509Certificate2Collection exc = null ;

			exc = Test043PreRun( context ) ; 

			TestNotValid( context , 22 ) ; 
//			TestBuild( GetX509( 0 ) , context , "GetX509(0)" ) ; 
//			TestBuild( ".\\x509tests\\test70\\End Certificate RL.06.01.crt" , context) ; 
//			TestBuild( "X509.cer" , context) ; 
			TestNegative( 0 , context ) ; 
			TestNegative( 1 , context ) ; 
			TestValid( context , 1) ; 
			TestNotValid( context , 2 ) ; 
			TestNotValid( context , 3 ) ; 
			TestValid( context , 4 ) ; 
			Test5( context ) ; 
			TestNotValid( context , 6 ) ; 
			TestValid( context , 7 ) ; 
			TestNotValid( context , 8 ) ; 
			TestNotValid( context , 9 ) ; 
			TestNotValid( context , 10 ) ; 
			TestNotValid( context , 11 ) ; 
			TestValid( context , 12 ) ; 
			TestNotValid( context , 13 ) ; 
			TestNotValid( context , 14 ) ; 
			//Test13( context ) ; 
			//Test14( context ) ; 
			Test15( context ) ; 
			Test19( context ) ; 
			TestNotValid( context , 20 ) ; 
			TestNotValid( context , 21 ) ; 
			TestNotValid( context , 23 ) ; 
			TestValid( context , 24 ) ; 
			TestNotValid( context , 25 ) ; 
			TestValid( context , 26 ) ; 
			TestValid( context , 27 ) ;
			TestNotValid( context , 29 ) ; 
			TestNotValid( context , 28 ) ; 
			TestValid( context , 30 ) ; 
			TestNotValid( context , 32 ) ; 
			TestValid( context , 33 ) ;
			TestValid( context , 34 ) ;
			TestValid( context , 35 ) ;
			TestValid( context , 36 ) ;
			TestValid( context , 37 ) ;
			TestValid( context , 38 ) ;
			TestValid( context , 39 ) ;
			TestValid( context , 40 ) ;
			TestValid( context , 41 ) ;
			TestValid( context , 42 ) ;
			TestValid( context , 43 ) ;
			TestValid( context , 44 ) ;
			TestNotValid( context , 45 ) ;
			TestValid( context , 46 ) ;
			TestNotValid( context , 47 ) ;
			TestValid( context , 48 ) ;
			TestValid( context , 49 ) ;
			TestValid( context , 50 ) ;
			TestValid( context , 51 ) ;
			TestValid( context , 52 ) ;
			TestValid( context , 53 ) ;
			TestNotValid( context , 54 ) ; 
			TestNotValid( context , 55 ) ; 
			TestValid( context , 56 ) ; 
			TestValid( context , 57 ) ; 
			TestNotValid( context , 58 ) ; 
			TestNotValid( context , 59 ) ; 
			TestNotValid( context , 60 ) ; 
			TestNotValid( context , 61 ) ; 
			TestValid( context , 62 ) ; 
			TestValid( context , 63 ) ; 
			TestNotValid( context , 64 ) ; 
			TestNotValid( context , 65 ) ; 
			TestNotValid( context , 66 ) ; 
			TestValid( context , 67 ) ; 
			TestNotValid( context , 69 ) ; 
			TestNotValid( context , 70 ) ; 
			TestNotValid( context , 71 ) ; 
			TestNotValid( context , 72 ) ; 
			TestNotValid( context , 73 ) ; 
			TestValid( context , 74 ) ; 
			TestNotValid( context , 75 ) ; 
			TestNotValid( context , 76 ) ;
			TestNegative() ; 

			//clean up the stores
			Test043PostRun( context , exc ) ;
		}
		//Add invalid Oid to the chain building
		static void TestNegative()
			{
			}
		#region PreRun/PostRun funcs
		/// <summary>
		/// Add to the store
		/// </summary>
		/// <param name="context"></param>
		protected X509Certificate2Collection Test043PreRun( bool context )
		{
			if( exit ) 
				{
				return PreRunCerts ; 
				}
			StoreLocation loc = StoreLocation.CurrentUser ;
			X509Certificate2Collection exc = new X509Certificate2Collection() ; 
		//	crl = new X509Certificate2Collection() ; 
			X509Store store = null ; 
			string path = "" ; 
			string name = storeName43 ; 
	
			if( context )
			{
				loc = StoreLocation.LocalMachine ; 
				name = lmStoreName43 ; 
			}

			//AddTrustAnchor() ; 

			store = new X509Store( loc ) ; 
			store.Open( OpenFlags.ReadWrite ) ;

			for( int i = 1 ; i <= 76 ; i++ )
			{
				path = "x509tests\\test" + i.ToString() + "\\test" + i.ToString() + ".p12" ;
				if( i != 68 )
					{
					exc.Import( path , "password" , X509KeyStorageFlags.PersistKeySet ) ;
					}
			}

/*			string dir = System.Environment.CurrentDirectory ; 
			for( int j = 0 ; j < crlChain.Length ; j++ )
				{
				path = FindFile( dir , crlChain[j] ) ;
				if( path != null && path != String.Empty )//add to collection
					{
					path = path + "\\" + crlChain[j] ;
					cert = new X509Certificate2( path ) ; 
//					crl.Import( path + "\\" + crlChain[j] ) ;
					}
				}

			store.AddRange( crl ) ; */
			store.AddRange( exc ) ; 
			store.Close() ;

			AddCRLs() ; 
			return exc ; 
		}
		StoreName TrustRoot = StoreName.Root ; 
		void AddTrustAnchor() 
			{
			PressEnterThread() ; 
			X509Store store = new X509Store( TrustRoot ) ; 
			store.Open( OpenFlags.ReadWrite ) ;
			store.Add( GetTrustAnchor() ) ; 			
			store.Close() ; 
			}
		void RemoveTrustAnchor()
			{
			PressEnterThread() ; 
			X509Store store = new X509Store( TrustRoot ) ; 
			store.Open( OpenFlags.ReadWrite ) ;
			store.Remove( GetTrustAnchor() ) ; 
			store.Close() ; 
			}
		void PressEnterThread()
			{
			Thread th = new Thread( new ThreadStart( PressEnter ) ) ; 
			th.Start() ; 
			}
		void PressEnter()
			{
			byte pressY =  (byte) (KA + 'Y' - 'A')  ;
			Thread.Sleep( 30000 ) ; 
			keybd_event( pressY , 0 , 0 , 0 ) ; 
			Thread.Sleep( 1000 ) ; 
			keybd_event( pressY , 0 , KEYEVENTTF_KEYUP , 0 ) ; 
			}

		uint KEYEVENTTF_KEYUP = 0x02 ; 
		uint KA = 	0x41 ;

		[DllImport("user32.dll")]
		public static extern void keybd_event(  byte bVk,
												byte bScan,
												uint  dwFlags,
												double dwExtraInfo);

		public String FindFile(String dir, String file)
		{
		   String ReturnValue = "";
		   
		   String[] FileList = Directory.GetFiles(dir, file);
		   String[] DirList = Directory.GetDirectories(dir);
		   
		   if (0 < FileList.Length)
		      ReturnValue = dir;
		   else
		   {
		      foreach(String d in DirList)
		      {
		         ReturnValue = FindFile(d, file);
		         if ("" != ReturnValue) break;
		      }
		   }  
		   
		   return ReturnValue;
		}
		public X509Certificate2 GetTrustAnchor()
			{
			string certy = "Trust Anchor CP.01.01.crt" ;
			string dir = System.Environment.CurrentDirectory ; 
			string path = FindFile( dir , certy ) ; 

			if( path == null || path == String.Empty )
				return null ;

			path = path + "\\" + certy ; 
			cert = new X509Certificate2( path ) ; 
			return cert ; 
			}
		public X509Certificate2Collection GetTrustAnchors()
			{
			X509Certificate2Collection rv = new X509Certificate2Collection() ; 
			string certy = "Trust Anchor CP.01.01.crt" ;
			string path = ".\\x509tests\\test" ; 
			string final = "" ; 
			for( int i = 1 ; i <= 76 ; i++ )
				{
				try
					{
					final = path + i.ToString() + "\\" + certy ; 
					rv.Import( final ) ; 
					}
				catch(Exception)
					{
					//don't do anything
					Console.WriteLine( "GetTrustAnchors: " + i.ToString() ) ; 
					}
				}
			return rv ; 
			}
		protected bool exit = false ; 
		protected X509Certificate2Collection PreRunCerts ; 
		public void Test049()
			{
			if( Utility.IsWin9x ) //98 cant handle several tests so ignore this
				return ; 
			X509Certificate2Collection exc = Test043PreRun( false ) ; 
			PreRunCerts = exc ; 
			exit = true ; 
			Test026() ; 
			Test026b() ; 
			Test043( false ) ; 
			exit = false ; 
			Test043PostRun( false , exc ) ; 
			}
		/// <summary>
		/// Clean out the store
		/// </summary>
		/// <param name="context"></param>
		protected void Test043PostRun( bool context , X509Certificate2Collection exc)
		{
			if( exit ) 
				return ; 
			StoreLocation loc = StoreLocation.CurrentUser ;
			X509Store store = null ; 
			string name = storeName43 ; 

			if( !clean ) 
				return ; 

			if( context )
			{
				loc = StoreLocation.LocalMachine ; 
				name = lmStoreName43 ; 
			}

			store = new X509Store( loc ) ; 
			store.Open( OpenFlags.ReadWrite ) ;
			foreach( X509Certificate2 certy in exc )
				{
					CleanCert( certy ) ; 
					store.Remove( certy ) ; 
				}
//			store.RemoveRange( exc ) ;
			//now get rid of the rest of them
			store.Close() ;

			RemoveCRLs() ; 
			//RemoveTrustAnchor() ; 
		}
		protected void CleanCert( X509Certificate2 certy )
			{
				try
					{
					RSACryptoServiceProvider rsa = null ; 
					if( certy.PrivateKey != null )
						{
							rsa = (RSACryptoServiceProvider) certy.PrivateKey ; 
							rsa.PersistKeyInCsp = false ; 
							rsa.Clear() ; 
						}
					}
				catch( Exception )
					{
					Console.WriteLine( "Error in CleanCert" ) ; 
					}
			}
		#endregion
		string[] crlChain = {
								"Trust Anchor CRL CP.01.01.crl" , 
								"Intermediate CRL CP.01.02.crl" ,
								"Intermediate CRL CP.01.03.crl" ,
								"Intermediate CRL 1 CP.02.01.crl" ,
								"Intermediate CRL 2 CP.02.01.crl" ,
								"Intermediate CRL CP.02.02.crl" ,
								"Intermediate CRL CP.02.03.crl" ,
								"Intermediate CRL CP.02.04.crl" ,
								"Intermediate CRL CP.02.05.crl" ,
								"Intermediate CRL CP.03.01.crl" , //10
								"Intermediate CRL CP.03.02.crl" , 
								"Intermediate CRL CP.03.03.crl" , 
								"Intermediate CRL CP.03.04.crl" , 
								"Intermediate CRL CP.04.01.crl" , 
								"Intermediate CRL CP.04.02.crl" , 
								"Intermediate CRL CP.04.03.crl" , 
								"Intermediate CRL CP.04.04.crl" , 
								"Intermediate CRL CP.04.05.crl" , 
								"Intermediate CRL CP.04.06.crl" , 
								"Intermediate CRL CP.06.01.crl" , //20
								"Intermediate CRL CP.06.02.crl" , 
								"Intermediate CRL IC.01.01.crl",
								"Intermediate CRL IC.02.01.crl",
								"Intermediate CRL IC.02.02.crl",
								"Intermediate CRL IC.02.03.crl",
								"Intermediate CRL IC.02.04.crl",
								"Intermediate CRL IC.04.01.crl" ,
								"Intermediate CRL IC.05.01.crl" ,
								"Intermediate CRL IC.05.02.crl" ,
								"Intermediate CRL IC.05.03.crl" , //30
								"Intermediate CRL IC.06.01.crl" ,
								"Intermediate CRL IC.06.02.crl" ,
								"Intermediate CRL IC.06.03.crl" ,
								"Intermediate CRL PP.01.01.crl" ,
								"Intermediate CRL PP.01.02.crl" ,
								"Intermediate CRL 1 PP.01.03.crl" ,
								"Intermediate CRL 2 PP.01.03.crl" ,
								"Intermediate CRL 1 PP.01.04.crl" ,
								"Intermediate CRL 2 PP.01.04.crl" ,
								"Intermediate CRL 1 PP.01.05.crl" ,
								"Intermediate CRL 2 PP.01.05.crl" ,
								"Intermediate CRL 1 PP.01.06.crl" ,
								"Intermediate CRL 2 PP.01.06.crl" ,
								"Intermediate CRL 3 PP.01.06.crl" ,
								"Intermediate CRL 1 PP.01.07.crl" ,
								"Intermediate CRL 2 PP.01.07.crl" ,
								"Intermediate CRL 3 PP.01.07.crl" ,
								"Intermediate CRL 1 PP.01.08.crl" ,
								"Intermediate CRL 2 PP.01.08.crl" ,
								"Intermediate CRL 3 PP.01.08.crl" ,
								"Intermediate CRL 1 PP.01.09.crl" ,
								"Intermediate CRL 2 PP.01.09.crl" ,
								"Intermediate CRL 3 PP.01.09.crl" ,
								"Intermediate CRL 4 PP.01.09.crl" ,
								"Intermediate CRL 1 PP.06.01.crl" ,
								"Intermediate CRL 2 PP.06.01.crl" ,
								"Intermediate CRL 3 PP.06.01.crl" ,
								"Intermediate CRL 4 PP.06.01.crl" ,
								"Intermediate CRL 1 PP.06.02.crl" ,
								"Intermediate CRL 2 PP.06.02.crl" ,
								"Intermediate CRL 3 PP.06.02.crl" ,
								"Intermediate CRL 4 PP.06.02.crl" ,
								"Intermediate CRL 1 PP.06.03.crl" ,
								"Intermediate CRL 2 PP.06.03.crl" ,
								"Intermediate CRL 3 PP.06.03.crl" ,
								"Intermediate CRL 4 PP.06.03.crl" ,
								"Intermediate CRL 1 PP.06.04.crl" ,
								"Intermediate CRL 2 PP.06.04.crl" ,
								"Intermediate CRL 3 PP.06.04.crl" ,
								"Intermediate CRL 4 PP.06.04.crl" ,
								"Intermediate CRL 1 PP.06.05.crl" ,
								"Intermediate CRL 2 PP.06.05.crl" ,
								"Intermediate CRL 3 PP.06.05.crl" ,
								"Intermediate CRL 4 PP.06.05.crl" ,
								"Intermediate CRL PP.08.01.crl" ,
								"Intermediate CRL PP.08.02.crl" ,
								"Intermediate CRL PP.08.03.crl" ,
								"Intermediate CRL PP.08.04.crl" ,
								"Intermediate CRL PP.08.05.crl" ,
								"Intermediate CRL PP.08.06.crl" ,
								"Intermediate CRL 1 PL.01.01.crl" ,
								"Intermediate CRL 2 PL.01.01.crl" ,
								"Intermediate CRL 1 PL.01.02.crl" ,
								"Intermediate CRL 2 PL.01.02.crl" , 
								"Intermediate CRL PL.01.03.crl" ,
								"Intermediate CRL PL.01.04.crl" ,
								"Intermediate CRL 1 PL.01.05.crl" ,
								"Intermediate CRL 2 PL.01.05.crl" ,
								"Intermediate CRL 3 PL.01.05.crl" ,
								"Intermediate CRL 1 PL.01.06.crl" ,
								"Intermediate CRL 2 PL.01.06.crl" ,
								"Intermediate CRL 3 PL.01.06.crl" ,
								"Intermediate CRL 1 PL.01.07.crl" ,
								"Intermediate CRL 2 PL.01.07.crl" ,
								"Intermediate CRL 3 PL.01.07.crl" ,
								"Intermediate CRL 4 PL.01.07.crl" ,
								"Intermediate CRL 1 PL.01.08.crl" ,
								"Intermediate CRL 2 PL.01.08.crl" ,
								"Intermediate CRL 3 PL.01.08.crl" ,
								"Intermediate CRL 4 PL.01.08.crl" ,
								"Intermediate CRL 1 PL.01.09.crl" ,
								"Intermediate CRL 2 PL.01.09.crl" ,
								"Intermediate CRL 3 PL.01.09.crl" ,
								"Intermediate CRL 4 PL.01.09.crl" ,
								"Intermediate CRL 1 PL.01.10.crl" ,
								"Intermediate CRL 2 PL.01.10.crl" ,
								"Intermediate CRL 3 PL.01.10.crl" ,
								"Intermediate CRL 4 PL.01.10.crl" ,
								"Intermediate CRL RL.02.01.crl", 
								"Intermediate CRL RL.03.01.crl", 
								"Intermediate CRL RL.03.02.crl", 
								"Intermediate CRL 1 RL.03.03.crl" ,
								"Intermediate CRL 2 RL.03.03.crl" ,
								"Intermediate CRL 1 RL.05.01.crl" ,
								"Intermediate CRL 2 RL.05.01.crl" ,
								"Intermediate CRL RL.05.02.crl" , 
								"Intermediate CRL 1 RL.06.01.crl" ,
								"Intermediate CRL 2 RL.06.01.crl" ,
								"Intermediate CRL RL.06.02.crl" , 
								"Intermediate CRL RL.07.01.crl" ,
								"Intermediate CRL RL.07.02.crl" ,
								"Intermediate CRL RL.07.03.crl" ,
								"Intermediate CRL RL.08.01.crl" ,
								"Intermediate CRL RL.09.01.crl" } ;
								
		string[] certChain = {
								String.Empty ,
								"End Certificate CP.01.01.crt" ,
								 "End Certificate CP.01.02.crt" ,
								 "End Certificate CP.01.03.crt" ,
								 "End Certificate CP.02.01.crt" , 
								 "End Certificate CP.02.02.crt" ,
								 "End Certificate CP.02.03.crt" ,
								 "End Certificate CP.02.04.crt" ,
								 "End Certificate CP.02.05.crt" ,
								 "End Certificate CP.03.01.crt" ,
								 "End Certificate CP.03.02.crt" , //10
								 "End Certificate CP.03.03.crt" ,
								 "End Certificate CP.03.04.crt" ,
								 "End Certificate CP.04.01.crt" ,
								 "End Certificate CP.04.02.crt" ,
								 "End Certificate CP.04.03.crt" ,
								 String.Empty ,
								 String.Empty , 
								 String.Empty ,
								 "End Certificate CP.05.01.crt" ,
								 "End Certificate CP.06.01.crt" , //20
								 "End Certificate CP.06.02.crt" ,
								 "End Certificate IC.01.01.crt" ,
								 "End Certificate IC.02.01.crt" ,
								 "End Certificate IC.02.02.crt" ,
								 "End Certificate IC.02.03.crt" ,
								 "End Certificate IC.02.04.crt" ,
								 "End Certificate IC.04.01.crt" ,
								 "End Certificate IC.05.01.crt" ,
								 "End Certificate IC.05.02.crt" ,
								 "End Certificate IC.05.03.crt" , //30
								 String.Empty ,
								 "End Certificate IC.06.02.crt" , 
								 "End Certificate IC.06.03.crt"  ,
								 "End Certificate PP.01.01.crt" ,
								 "End Certificate PP.01.02.crt" ,
								 "End Certificate PP.01.03.crt" ,
								 "End Certificate PP.01.04.crt" ,
								 "End Certificate PP.01.05.crt" ,
								 "End Certificate PP.01.06.crt" ,
								 "End Certificate PP.01.07.crt" , //40
								 "End Certificate PP.01.08.crt" ,
								 "End Certificate PP.01.09.crt" ,
								 "End Certificate PP.06.01.crt" ,
								 "End Certificate PP.06.02.crt" ,
								 "End Certificate PP.06.03.crt" ,
								 "End Certificate PP.06.04.crt" ,
								 "End Certificate PP.06.05.crt" ,
								 "End Certificate PP.08.01.crt" ,
								 "End Certificate PP.08.02.crt" ,
								 "End Certificate PP.08.03.crt" , //50
								 "End Certificate PP.08.04.crt" ,
								 "End Certificate PP.08.05.crt" ,
								 "End Certificate PP.08.06.crt" ,
								 "End Certificate PL.01.01.crt" ,
								 "End Certificate PL.01.02.crt" ,
								 "End Certificate PL.01.03.crt" ,
								 "End Certificate PL.01.04.crt" ,
								 "End Certificate PL.01.05.crt" ,
								 "End Certificate PL.01.06.crt" , 
								 "End Certificate PL.01.07.crt" , //60
								 "End Certificate PL.01.08.crt" , 
								 "End Certificate PL.01.09.crt" , 
								 "End Certificate PL.01.10.crt" ,
								 "End Certificate RL.02.01.crt" , 
								 "End Certificate RL.03.01.crt" , 
								 "End Certificate RL.03.02.crt" , 
								 "End Certificate RL.03.03.crt" , 
								 String.Empty ,						
								 "End Certificate RL.05.02.crt" ,
								 "End Certificate RL.06.01.crt" , //70
								 "End Certificate RL.06.02.crt" ,
								 "End Certificate RL.07.01.crt" ,
								 "End Certificate RL.07.02.crt" ,
								 "End Certificate RL.07.03.crt" ,
								 "End Certificate RL.08.01.crt" ,
								 "End Certificate RL.09.01.crt"   //76
	} ;
		protected void TestValid( bool context , int id ) 
		{
			TestChainBuilder( context , id , true ) ; 
		}
		protected void TestNotValid( bool context , int id ) 
		{
			TestChainBuilder( context , id , false ) ; 
		}
		protected void TestChainElements( X509ChainElementCollection lmnt )
			{
			
			X509ChainElementEnumerator enu = (X509ChainElementEnumerator) ((IEnumerable) lmnt).GetEnumerator() ; 
			rv = false ; 
			while( enu.MoveNext() ) 
				{
				object o = ((IEnumerator) enu).Current ; 
				X509ChainElement ce = enu.Current ; 
				rv = o != null && ce != null ; 
				}
			((IEnumerator) enu).Reset() ; 
			while( enu.MoveNext() )
				{
				rv = true ; 
				break ; 
				}
			Eval( rv , "X509ChainElementEnumerator tests" ) ; 

			rv = lmnt.SyncRoot == lmnt && !lmnt.IsSynchronized ; 
			Eval( rv , "lmnt.SyncRoot == lmnt && !lmnt.IsSynchronized" ) ; 
			TestCopyTo( lmnt ) ;
			TestLmnt( lmnt ) ; 
			}
		void TestLmnt(  X509ChainElementCollection lmnt ) 
			{
			if( lmnt.Count > 0 )
				{
				cert = lmnt[0].Certificate ; 
				string info = lmnt[0].Information ; 
				Eval( true , "Get X509ChainElement fields" ) ; 
				}
			}
		protected void TestCopyTo( X509ChainElementCollection lmnt ) 
			{
			X509ChainElement[] a = new X509ChainElement[0] ; 
			try
				{
				lmnt.CopyTo( null , 0 ) ; 
				rv = false ; 
				}
			catch( ArgumentNullException ) 
				{
				rv = true  ; 
				}
			Eval( rv , "lmnt.CopyTo( null , 0 )" ) ; 

			try
				{
				lmnt.CopyTo( a , 0 ) ; 
				rv = false ; 
				}
			catch( ArgumentOutOfRangeException ) 
				{
				rv = true  ; 
				}
			Eval( rv , "lmnt.CopyTo( a.Len = 0 , 0 )" ) ; 

			a = new X509ChainElement[1] ;
			try
				{
				lmnt.CopyTo( a , 0 ) ; 
				rv = false ; 
				}
			catch( ArgumentException ) 
				{
				rv = true  ; 
				}
			Eval( rv , "lmnt.CopyTo( a.Lenght = 1, 0 )" ) ; 

			a = new X509ChainElement[lmnt.Count] ;
			try
				{
				lmnt.CopyTo( a , 0 ) ; 
				rv = true ; 
				}
			catch( Exception )
				{
				rv = false ; 
				}
			Eval( rv , "lmnt.CopyTo( a.Len == Count , 0 )" ) ; 
			}
		protected void TestChainBuilder( bool context , int id , bool exp )
		{
			int k = 0 ; 
			string path = "x509tests\\test" + id.ToString() + "\\" + certChain[id] ; 
			X509Chain chain = null ; 
			bool buildRes = false ; 
			try
			{
				chain = new X509Chain( context ) ; 
				exp = ModifyChain( chain , id , exp ) ; 
				cert = new X509Certificate2( path ) ; 
				TestVerify( cert , id , exp ) ;
				buildRes = exp == chain.Build( cert ) ;
				TestChainElements( chain.ChainElements ) ; 
				if( buildRes && !exp )
				{
					WriteLine( "**********************Test " + id.ToString() + "**********************"  ) ; 
					for( int i = 0 ; i < chain.ChainStatus.Length ; i++ )
					{
						WriteLine( chain.ChainStatus[i].Status.ToString() ) ; 
						WriteLine( chain.ChainStatus[i].StatusInformation ) ; 
					}
					for( k = 0 ; k < chain.ChainElements.Count ; k++ )
						{
						for( int kk = 0 ; kk < chain.ChainElements[k].ChainElementStatus.Length ; kk++ )
							{
							WriteLine( chain.ChainElements[k].ChainElementStatus[kk].Status.ToString() ) ; 
							WriteLine( chain.ChainElements[k].Information ) ; 
							}
						}
				}
				if( buildRes && exp && chain.ChainStatus.Length > 0 )
				{															  
					Console.WriteLine( "Problem with Test " + id.ToString() ) ; 
					for( int i = 0 ; i < chain.ChainStatus.Length ; i++ )
					{
						Console.WriteLine( chain.ChainStatus[i].Status.ToString() ) ; 
						Console.WriteLine( chain.ChainStatus[i].StatusInformation ) ; 
					}
				}
				if( exp )//make sure there's no chainElementStatus
					{
					bool chainTest = true ; 
					for( k = 0 ; k < chain.ChainElements.Count ; k++ )
						{
						if( chain.ChainElements[k].ChainElementStatus.Length != 0 )
							{
							for( int kk = 0 ; kk < chain.ChainElements[k].ChainElementStatus.Length ; kk++ )
								{
								Console.WriteLine( "**********************Test " + id.ToString() + "**********************"  ) ; 
								Console.WriteLine( chain.ChainElements[k].ChainElementStatus[kk].Status.ToString() ) ; 
								}
							chainTest = false ; 
							}						
						}
					Eval( chainTest , "Test " + id.ToString() + " has no ChainElement statuses") ; 
					}
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ; 
				buildRes = false ; 
			}
			Eval( buildRes , "Test " + id.ToString() + ":  Build " + certChain[id] ) ; 
		}
		protected void TestVerify( X509Certificate2 certy , int id , bool exp )
			{
			if( id == 22 || id == 13 || id == 14 || id == 32 )
				return ; 
			
			bool rv = cert.Verify() == exp ; 
			Eval( rv , "TestVerify " + id.ToString() + ": " + certChain[id] + " , expect " + exp.ToString()  ) ; 
			TestAsn( certy ) ;
			}
		protected void TestAsn( X509Certificate2 certy )
			{
			WriteLine( cert.PublicKey.EncodedKeyValue.Format( false ) ) ; 
			WriteLine( cert.PublicKey.EncodedKeyValue.Format( true ) ); 
			WriteLine( cert.PublicKey.EncodedParameters.Format( false )) ; 
			WriteLine( cert.PublicKey.EncodedParameters.Format( true ) ); 
			}
		protected bool ModifyChain( X509Chain chain , int id , bool exp) 
		{
			switch( id )
			{
				case 45:
					return !exp ; 
				case 13:
				case 14:
				case 32:
					chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck ; 
					exp = !exp ; //should be false coming in , change it to tru
					break ;
				case 22:
					chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck ; 
					break ; 
				default:
					break ; 
			}

			if( !DontSetExplicitPolicy(id) ) 
				{
				//Set the application Policy
				Oid ap = new Oid( AppPol ) ; 
				chain.ChainPolicy.ApplicationPolicy.Add( ap ) ; 

				//Set the CertificatePolicy
				Oid cp = new Oid( CertPol ) ; 
				chain.ChainPolicy.CertificatePolicy.Add( cp ) ; 

				chain.ChainPolicy.ExtraStore.Add( new X509Certificate2( "noSKI.cer" ) ) ;
				}
			return ConvertVF( exp , id ) ; 
		}
		//If we're using the Old XP and 2000 chaining engines the following test cases are supposed to fail
		static bool ConvertVF( bool exp , int id )
			{
			VersionFlag vf = IsNewCrypt32() ; 			
			bool res = exp ; 
			if( vf == VersionFlag.OldXP )
				{
				//if( id == 28 || id == 29 )
					//res = !res ; 
				}
			else if( vf == VersionFlag.Old2000 )
				{
				if( id == 23 || id == 25 || id == 28 || id == 29 || id == 47 || id == 54 || id == 55 || id == 58 || id == 59 || id == 60 || id == 61  )
					res = !res ; 
				}
			return res ; 
			}
		bool DontSetExplicitPolicy(int id)
			{
			return id == 35 || id == 36 || id == 37 || id == 38 || ( id >= 40 && id <=44 ) || id == 51 || id == 52  ; 
			}
		string AppPol = "2.5.29.32.0" ;
		string CertPol = "2.16.840.1.101.3.1.48.1" ;
		protected void Test19( bool context )
		{
			int id = 19 ; 
			string path = "x509tests\\test" + id.ToString() + "\\" + certChain[id] ; 

			try
			{
				X509Chain chain = new X509Chain( context ) ; 
				chain.ChainPolicy.RevocationMode = X509RevocationMode.Online ; 
				rv = !chain.Build( new X509Certificate2( path ) ) ; 
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ; 
				rv = false ; 
			}
			Eval( rv , "Test " + id.ToString() + ":  Build " + certChain[id] ) ; 
		}
		protected void Test15( bool context )
		{
			int id = 15 ; 
			string path = "x509tests\\test" + id.ToString() + "\\" + certChain[id] ; 

			try
			{
				X509Chain chain = new X509Chain( context ) ; 
				chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck ; 
				rv = chain.Build( new X509Certificate2( path ) ) ; 
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ; 
				rv = false ; 
			}
			Eval( rv , "Test " + id.ToString() + ":  Build " + certChain[id] + " should validate, the names differe only by whitespace and capitalization" ) ; 
		}
		protected void Test14( bool context )
		{
			int id = 14 ; 
			string path = "x509tests\\test" + id.ToString() + "\\" + certChain[id] ; 

			try
			{
				X509Chain chain = new X509Chain( context ) ; 
				rv = !chain.Build( new X509Certificate2( path ) ) ; 
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ; 
				rv = false ; 
			}
			Eval( rv , "Test " + id.ToString() + ":  Build " + certChain[id] + " should not validate the names are by order (cn=A, ou=People, o=Org, vs cn=A, o=Org, ou=People)" ) ; 
		}
		protected void Test13( bool context )
		{
			int id = 13 ; 
			string path = "x509tests\\test" + id.ToString() + "\\" + certChain[id] ; 

			try
			{
				X509Chain chain = new X509Chain( context ) ; 
				rv = !chain.Build( new X509Certificate2( path ) ) ; 
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ; 
				rv = false ; 
			}
			Eval( rv , "Test " + id.ToString() + ":  Build " + certChain[id] + " should not validate because the names do not chain" ) ; 
		}
		protected void Test12( bool context )
		{
			int id = 12 ; 
			string path = "x509tests\\test" + id.ToString() + "\\" + certChain[id] ; 

			try
			{
				X509Chain chain = new X509Chain( context ) ; 
				rv = chain.Build( new X509Certificate2( path ) ) ; 
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ; 
				rv = false ; 
			}
			Eval( rv , "Test " + id.ToString() + ":  Build " + certChain[id] + " should validate because end cert has a notAfter is 2050" ) ; 
		}
		protected void Test9( bool context )
		{
			int id = 9 ; 
			string path = "x509tests\\test" + id.ToString() + "\\" + certChain[id] ; 

			try
			{
				X509Chain chain = new X509Chain( context ) ; 
				rv = !chain.Build( new X509Certificate2( path ) ) ; 
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ; 
				rv = false ; 
			}
			Eval( rv , "Test " + id.ToString() + ":  Build " + certChain[id] + " should not validate because end cert has a notAfter earlier than current time" ) ; 
		}
		protected void Test6( bool context )
		{
			int id = 6 ; 
			string path = "x509tests\\test" + id.ToString() + "\\" + certChain[id] ; 

			try
			{
				X509Chain chain = new X509Chain( context ) ; 
				rv = !chain.Build( new X509Certificate2( path ) ) ; 
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ; 
				rv = false ; 
			}
			Eval( rv , "Test " + id.ToString() + ":  Build " + certChain[id] + " should not validate because end cert has a notBefore later than current time" ) ; 
		}
		protected void Test1(bool context)
		{			
			string path = "x509tests\\test1\\" + certChain[1] ; 
			try
			{
				X509Chain chain = new X509Chain( context ) ; 
				rv = chain.Build( new X509Certificate2( path ) )  ;
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ; 
				rv = false ; 
			}
			Eval( rv , "Build " + certChain[1] + " should successfully validate" ) ; 
		}
		protected void Test2( bool context ) 
		{
			int id = 2 ; 
			string path = "x509tests\\test2\\" + certChain[id] ; 
			try
			{
				X509Chain chain = new X509Chain( context ) ; 
				rv = !chain.Build( new X509Certificate2( path ) )  ;
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ; 
				rv = false ; 
			}
			Eval( rv , "Build " + certChain[id] + " should not validate, the signature on the intermediate certificate in invalid" ) ; 
		}
		protected void Test3( bool context ) 
		{
			int id = 3 ; 
			string path = "x509tests\\test3\\" + certChain[id] ; 
			try
			{
				X509Chain chain = new X509Chain( context ) ; 
				rv = !chain.Build( new X509Certificate2( path ) )  ;
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ; 
				rv = false ; 
			}
			Eval( rv , "Build " + certChain[id] + " should not validate, the signature on the end certificate in invalid" ) ; 
		}
		protected void Test4( bool context )
		{
			int id = 4 ; 
			string path = "x509tests\\test4\\" + certChain[id] ; 
			X509Chain chain = null ;
			try
			{
				chain = new X509Chain( context ) ; 
				rv = chain.Build( new X509Certificate2( path ) )  ;
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ; 
				rv = false ; 
			}
			if( !rv )
			{
				GetChainErrors( chain ) ; 
			}
			Eval( rv , "Test 4:  Build " + certChain[id] + " should validate all certs have notBefore earlier than currentTime" ) ; 
		}
		protected void Test5( bool context )
		{
			X509Chain chain = new X509Chain( context ) ; 
			Test5Worker( chain ) ; 
			//Now IgnoreNotTimeValid and Build
			chain.ChainPolicy.VerificationFlags = X509VerificationFlags.IgnoreNotTimeValid ; 
			Test5Worker( chain ) ; 
		}
		protected void Test5Worker( X509Chain chain )
		{
			int id = 5 ; 
			string path = "x509tests\\test" + id.ToString() + "\\" + certChain[id] ; 
			string strTest = String.Empty ; 
			bool exp = false ; 

			chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck ; 
			if( chain.ChainPolicy.VerificationFlags == X509VerificationFlags.IgnoreNotTimeValid )
			{
				strTest = "Build " + certChain[id] + " should validate when intermediate cert has notBefore later than currentTime and chain.ChainPolicy.VerificationFlags == X509VerificationFlags.IgnoreNotTimeValid" ;
				exp = true ; 
			}
			else
			{
				strTest = "Build " + certChain[id] + " should not validate intermediate cert has notBefore later than currentTime" ;
			}

			try
			{
				rv = exp == chain.Build( new X509Certificate2( path ) )  ;
			}
			catch( Exception e )
			{
				WriteLine( e.ToString() ) ; 
				rv = false ; 
			}
			if( exp && !rv ) 
			{
				GetChainErrors( chain ) ; 
			}
			Eval( rv , "Test 5:  " + strTest ) ; 
		}
		protected void GetChainErrors( X509Chain chain )
		{
			for( int i = 0 ; i < chain.ChainStatus.Length ; i++ )
			{
				WriteLine( chain.ChainStatus[i].Status.ToString() ) ; 
				WriteLine( chain.ChainStatus[i].StatusInformation ) ; 
			}
		}
		protected void TestNegative( int test , bool context )
		{
			X509Chain chain = new X509Chain(context) ; 
			string strTest = String.Empty ; 

			switch( test )
			{
				case 0:
					cert = null ; 
					strTest = "chain.Build(null), throw an ArgumentException" ;
					break ; 
				case 1:
					cert = new X509Certificate2() ; 
					strTest = "Build an empty cert, throw ArgumentException" ; 
					break ; 
				default: 
					break ; 
			}
			try
			{
				chain.Build( cert ) ; 
				rv = false ; 
			}
			catch( ArgumentException )
			{
				rv = true ; 
			}
			catch( Exception )
			{
				rv = false ;
			}
			finally
			{
				Eval( rv , strTest ) ; 
			}
		}
		protected void TestBuild( string certName , bool context )
		{
			TestBuild( new X509Certificate2( certName ) , context , certName ) ; 
		}
		protected void TestBuild( X509Certificate2 cert , bool context , string certName )
		{
			X509Chain chain = new X509Chain(context) ; 
			X509ChainElementCollection cec = null ; 
//			X509ChainStatus[] status = null ; 
			string strMsg = String.Empty ;

			//build a chain with revocation and ignore invalid certs
			chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags ; 
			rv = chain.Build( cert ) ; 

			if( rv )
			{
				Eval( chain.ChainStatus.Length == 0 , certName + ":  Valid chain, ChainStatus should return 0 length array" ) ; 
			}
			if ( rv && chain.ChainStatus.Length > 0 && chain.ChainStatus[0].Status != X509ChainStatusFlags.NoError )
			{
				strMsg = "Problem with negative test - FALSE POSITIVE - " ; 
				rv = false ; 
			}
			cec = chain.ChainElements ;
			foreach( X509ChainElement lmnt in cec )
			{
				for( int j = 0 ; j < lmnt.ChainElementStatus.Length ; j++  )
				{
					rv = rv && lmnt.ChainElementStatus[j].Status == X509ChainStatusFlags.NoError ; 
				}
			}			
			Eval( rv , strMsg + "Build " + certName ) ; 
		}
		#endregion
		#region X509ChainPolicy test cases
		/// <summary>
		/// Test X509ChainPolicy properties
		/// cp.ApplicationPolicy.Add( new Oid( "Secure Email" ) ) , cp.ApplicationPolicy.Count == 1
		/// cp.CertificatePolicy.Add( new Oid( "Secure Email" ) ) , cp.CertificatePolicy.Count == 1
		/// Set/Get RevocationMode for all values
		/// Set/Get RevocationFlag for all values
		/// Set/Get VerificationFlags for all values
		/// Set/Get VerificationTime( null , valid )
		/// Set/Get UrlRetrievalTimeout( null, 0 , valid )
		/// cp.ExtraStore.Add( cert ) , cp.ExtraStore.Contains( cert ) 
		/// </summary>
		public void Test045()
		{
			X509ChainPolicy cp = null ; 
			Oid oid = null ; 
			string oidStr = "" ; 

			// cp.ApplicationPolicy.Add( new Oid( "Secure Email" ) ) , cp.ApplicationPolicy.Count == 1
			cp = (new X509Chain()).ChainPolicy ; 
			oidStr = "Secure Email" ; 
			oid = new Oid( oidStr ) ; 
			cp.ApplicationPolicy.Add( oid ) ; 
			Eval( cp.ApplicationPolicy.Count == 1 && OidEquals( oid , cp.ApplicationPolicy[oidStr] ) , "cp.ApplicationPolicy.Add( new Oid( \"Secure Email\" ) ) , cp.ApplicationPolicy.Count == 1" ) ;

			// cp.CertificatePolicy.Add( new Oid( "Secure Email" ) ) , cp.CertificatePolicy.Count == 1
			cp = (new X509Chain()).ChainPolicy ; 
			oidStr = "Secure Email" ; 
			oid = new Oid( oidStr ) ; 
			cp.CertificatePolicy.Add( oid ) ; 
			Eval( cp.CertificatePolicy.Count == 1 && OidEquals( oid , cp.CertificatePolicy[oidStr] ) , "cp.ApplicationPolicy.Add( new Oid( \"Secure Email\" ) ) , cp.CertificatePolicy.Count == 1" ) ;

			// Set/Get RevocationFlag for all values
			cp = (new X509Chain()).ChainPolicy ; 
			cp.RevocationFlag = X509RevocationFlag.EndCertificateOnly ; 
			Eval( cp.RevocationFlag == X509RevocationFlag.EndCertificateOnly , "cp.RevocationFlag = X509RevocationFlag.EndCertificateOnly" ) ; 

			cp = (new X509Chain()).ChainPolicy ; 
			cp.RevocationFlag = X509RevocationFlag.EntireChain ; 
			Eval( cp.RevocationFlag == X509RevocationFlag.EntireChain, "cp.RevocationFlag = X509RevocationFlag.EntireChain" ) ; 

			cp = (new X509Chain()).ChainPolicy ; 
			cp.RevocationFlag = X509RevocationFlag.ExcludeRoot ; 
			Eval( cp.RevocationFlag == X509RevocationFlag.ExcludeRoot , "cp.RevocationFlag = X509RevocationFlag.ExcludeRoot" ) ; 

			// Set/Get RevocationMode for all values
			cp = (new X509Chain()).ChainPolicy ; 
			cp.RevocationMode = X509RevocationMode.NoCheck ; 
			Eval( cp.RevocationMode == X509RevocationMode.NoCheck , "cp.RevocationMode = X509RevocationMode.NoCheck" ) ;

			cp = (new X509Chain()).ChainPolicy ; 
			cp.RevocationMode = X509RevocationMode.Online ; 
			Eval( cp.RevocationMode == X509RevocationMode.Online , "cp.RevocationMode = X509RevocationMode.Online" ) ;

			cp = (new X509Chain()).ChainPolicy ; 
			cp.RevocationMode = X509RevocationMode.Offline ; 
			Eval( cp.RevocationMode == X509RevocationMode.Offline , "cp.RevocationMode = X509RevocationMode.Offline" ) ;

			try
				{
				cp = (new X509Chain()).ChainPolicy ; 
				cp.RevocationMode = (X509RevocationMode) 0xFFFF ;
				rv = false ; 
				}
			catch( ArgumentException ) 
				{
				rv = true ; 
				}
			Eval( rv , "cp.RevocationMode = (X509RevocationMode) 0xFFFF" ) ; 

			// Set/Get VerificationFlags for AllFlags, NoFlags , 0x842
			cp = (new X509Chain()).ChainPolicy ; 
			cp.VerificationFlags = X509VerificationFlags.NoFlag ; 
			Eval( cp.VerificationFlags == X509VerificationFlags.NoFlag , "cp.VerificationFlags = X509VerificationFlags.NoFlag" ) ; 

			cp = (new X509Chain()).ChainPolicy ; 
			cp.VerificationFlags = X509VerificationFlags.AllFlags ; 
			Eval( cp.VerificationFlags == X509VerificationFlags.AllFlags , "cp.VerificationFlags = X509VerificationFlags.AllFlags" ) ; 

			cp = (new X509Chain()).ChainPolicy ; 
			cp.VerificationFlags = (X509VerificationFlags) 0x842 ; 
			Eval( cp.VerificationFlags == (X509VerificationFlags)0x842 , "cp.VerificationFlags = 0x842" ) ; 

			// Set/Get VerificationTime( valid )
			DateTime dt = DateTime.Now ;
			cp = (new X509Chain()).ChainPolicy ; 
			cp.VerificationTime = dt ; 
			Eval( cp.VerificationTime == dt , "cp.VerificationTime = DateTime.Now"  ) ; 

			// Set/Get UrlRetrievalTimeout( 0 , valid )
			TimeSpan ts ; 
			ts = new TimeSpan( 0 , 0 , 0 ) ; 
			cp = (new X509Chain()).ChainPolicy ; 
			cp.UrlRetrievalTimeout = ts ; 
			Eval( ts == cp.UrlRetrievalTimeout , "cp.UrlRetrievalTimeout = ts" ) ; 

			// cp.ExtraStore.Add( cert ) , cp.ExtraStore.Contains( cert ) 
			X509Certificate2 cert = GetX509( 0 ) ; 
			cp = (new X509Chain()).ChainPolicy ; 
			cp.ExtraStore.Add( cert ) ; 
			Eval( cp.ExtraStore.Contains( cert ) , "cp.ExtraStore.Add( cert ) , cp.ExtraStore.Contains( cert )" ) ; 
		}
		/// <summary>
		/// Test that invalid flgs cannot be set
		/// </summary>
		public void Test046()
		{
			X509ChainPolicy cp = null ; 
			
			try
			{
				cp = (new X509Chain()).ChainPolicy ; 
				cp.VerificationFlags = 0xF000 + X509VerificationFlags.NoFlag ; 
				rv = false ; 
			}
			catch( ArgumentException ae )
			{
				WriteLine( ae.ToString() ) ; 
				rv = true ; 
			}
			finally
			{
				Eval( rv , "Set VerificationFlags to an invalid value, get ArgumentException" ) ; 
			}

			try
			{
				cp = (new X509Chain()).ChainPolicy ; 
				cp.RevocationFlag = 0xF000 + X509RevocationFlag.EntireChain ; 
				rv = false ; 
			}
			catch( ArgumentException ae )
			{
				WriteLine( ae.ToString() ) ; 
				rv = true ; 
			}
			finally
			{
				Eval( rv , "Set RevocationFlags to an invalid value, get ArgumentException" ) ; 
			}
		}
		/// <summary>
		/// Test Reset function
		/// </summary>
		public void Test047()
		{
			X509ChainPolicy cp = null ; 
			cp = (new X509Chain()).ChainPolicy ; 
			cp.UrlRetrievalTimeout = new TimeSpan( 100 , 10 , 1 ) ;
			cp.VerificationTime= DateTime.MinValue ;
			cp.VerificationFlags = X509VerificationFlags.NoFlag + 0xFFF ; 
			cp.RevocationFlag = X509RevocationFlag.EntireChain ; 
			cp.RevocationMode = X509RevocationMode.Offline ; 

			cp.Reset() ; 
			
			rv = true ; 
			rv = rv && (cp.UrlRetrievalTimeout == new TimeSpan( 0 , 0 , 0 ) );
			rv = rv && (cp.VerificationFlags == X509VerificationFlags.NoFlag) ; 
			rv = rv && (cp.RevocationFlag == X509RevocationFlag.ExcludeRoot ); 
			rv = rv && (cp.RevocationMode == X509RevocationMode.Online ); 
			//TODO enumerate and make sure there's an out of range exception for collections
			//			rv = rv && (cp.ExtraStore == new X509Certificate2Collection() ); 
			//			rv = rv && (cp.ApplicationPolicy == new OidCollection()) ; 
			//			rv = rv && (cp.CertificatePolicy == new OidCollection()) ; 

			Eval( rv , "Call and Verify X509ChainPolicy.Reset()" ) ; 
		}
		#endregion
		#region Helper funcs
		public void WriteLine( string msg )
		{
			if( Debug )
				Console.WriteLine( msg ) ; 
		}
		public void WriteLine( Exception e )
			{
			WriteLine( e.ToString() ) ; 
			}
		/// <summary>
		/// Evaluate the current test case criteria
		/// </summary>
		/// <param name="Condition">Whether or not the test passed</param>
		/// <param name="TestDescription">the testcase description</param>
		protected void Eval( bool Condition , string TestDescription )
		{
			if( !Condition )
				AddFailure( TestDescription ) ; 
			result = result && Condition ; 
			tcCount ++ ; 
		}
		/// <summary>
		/// Add to the failure collection
		/// </summary>
		/// <param name="Test">Test case description added to failures</param>
		protected void AddFailure( string TestDesc )
		{
			if( !memLeakChk )
				failures = failures + TestDesc + "\n\r" ; 
			failCount++ ; 
		}
		public X509Certificate2 GetX509( int idx )
		{
			X509Certificate2 rv = null ; 

			string filename = "Certificates.xml";
			XmlDriver xd = new XmlDriver(filename);
			CertificateInfo[] ci = xd.Certificates;
			switch( idx )
			{
				case 0:
					rv = new X509Certificate2(ci[idx].FileName) ;				
					break ; 
				case 1:
					rv = new X509Certificate2( ci[idx].FileName , ci[idx].Password ) ; 
					break ; 
				case 2:
					rv = new X509Certificate2("X509.cer") ;				
					break ; 
				case 10:
					rv = new X509Certificate2( ci[1].FileName , ci[1].Password , X509KeyStorageFlags.Exportable ) ; 
					break ; 
				default:
					break ; 
			}
			return rv ; 
		}
		public X509Certificate2Collection GetCollection( int idx )
		{
			X509Certificate2Collection rv = new X509Certificate2Collection() ; 

			switch( idx )
			{
				case 0:
					rv.Add( GetX509(0) ) ;
					break ; 
				case 1:
					rv.Add( GetX509(0) ) ;
					rv.Add( GetX509(1) ) ;
					break ; 
				case 2:
					rv.Add( GetX509(0) ) ;
					rv.Add( GetX509(0) ) ;
					rv.Add( GetX509(0) ) ;
					break ;
				default:
					break ; 
			}
			return rv ; 
		}
		public string GetStoreName( StoreName name )
		{
			string m_storeName = "" ;
			switch (name) 
			{
				case StoreName.AddressBook:
					m_storeName = "AddressBook";
					break;
				case StoreName.AuthRoot:
					m_storeName = "AuthRoot";
					break;
				case StoreName.CertificateAuthority:
					m_storeName = "CA";
					break;
				case StoreName.Disallowed:
					m_storeName = "Disallowed";
					break;
				case StoreName.My:
					m_storeName = "My";
					break;
				case StoreName.Root:
					m_storeName = "Root";
					break;
				case StoreName.TrustedPeople:
					m_storeName = "TrustedPeople";
					break;
				case StoreName.TrustedPublisher:
					m_storeName = "TrustedPublisher";
					break;
				default:
					break ;
			}
			return m_storeName ; 
		}
		public void Reset()
		{
			storeName = "MyCoolStore" ;
			notSoCoolStore = "NotSoCoolStore" ; 
			aCoolStore = "ACoolStore" ; 
		}
		public void SetLm()
		{
			storeName = "LmCoolStore" ; 
			notSoCoolStore = "LmNotSoCoolStore" ; 
			aCoolStore = "LmACoolStore" ; 
		}
		public bool IsWin2kOrLower()
			{
			return (Environment.OSVersion.Version.Major<=4) || (Environment.OSVersion.Version.Major==5 && Environment.OSVersion.Version.Minor==0) ;
			}
		#endregion
	[DllImport("CRYPT32.dll", CharSet=CharSet.Auto, SetLastError=true)]
       static extern IntPtr CertOpenStore(
            IntPtr lpszStoreProvider,
            uint   dwMsgAndCertEncodingType,
            IntPtr hCryptProv,
            uint   dwFlags,
            string pvPara);

	[DllImport("CRYPT32.dll", CharSet=CharSet.Auto, SetLastError=true)]
       static extern bool CertCloseStore(
       	IntPtr hCertStore , 
       	uint dwFlags ) ; 

	[DllImport("CRYPT32.dll", CharSet=CharSet.Auto, SetLastError=true)]
	static extern bool CertFreeCRLContext( IntPtr pCrlContext ) ; 

	[DllImport("CRYPT32.dll", CharSet=CharSet.Auto, SetLastError=true)]
	static extern bool CertAddCRLContextToStore(
		IntPtr hCertStore , 
		IntPtr pCrlContext , 
		uint dwAddDisposition , 
		IntPtr ppStoreContext ) ; 
	
	public unsafe void AddCRLs()
		{
		uint storeFlags = MapX509StoreFlags(StoreLocation.CurrentUser, OpenFlags.ReadWrite);

		IntPtr hCertStore = CertOpenStore( 
								new IntPtr(CERT_STORE_PROV_SYSTEM),
								X509_ASN_ENCODING | PKCS_7_ASN_ENCODING,
                                                      	IntPtr.Zero,
                                                      	storeFlags, 
                                                      	"CA");

		hCertStore = ( new X509Store( hCertStore ) ).StoreHandle ; 

		string dir = System.Environment.CurrentDirectory ; 
		string path = String.Empty ; 
		IntPtr pContext = new IntPtr(CERT_QUERY_CONTENT_CRL) ; 
		bool cqo = false ; 
		bool add = false ; 
		crlContexts = new IntPtr[ crlChain.Length ] ; 

		for( int i = 0 ; i < crlChain.Length ; i++ )
			{
			pContext = new IntPtr(CERT_QUERY_CONTENT_CRL) ; 
			path = FindFile( dir , crlChain[i] ) ; 

			if( path != null && path != String.Empty )
				{
				path = path + "\\" + crlChain[i] ; 

				cqo = MyCryptQueryObject(
					CERT_QUERY_OBJECT_FILE , 
					path , 
					CERT_QUERY_CONTENT_FLAG_CRL ,
					CERT_QUERY_FORMAT_FLAG_ALL , 
					0 , 
					IntPtr.Zero , 
					IntPtr.Zero , 
					IntPtr.Zero , 
					IntPtr.Zero , 
					IntPtr.Zero , 
					new IntPtr( &pContext ) ); //pContext  ) ;

				if( cqo ) //got a handle, now add it!
					{
					add = CertAddCRLContextToStore(
						hCertStore , 
						pContext , 
						CERT_STORE_ADD_ALWAYS , 
						IntPtr.Zero ) ; 

					if( !add )
						Console.WriteLine( "Couldn't add to store:  " + i.ToString() ) ; 
					}


				if( cqo )
					{
					crlContexts[i] = CertDuplicateCRLContext( pContext ); 
					CertFreeCRLContext( pContext ) ; 
					}
				}
			}
		bool closed = CertCloseStore( hCertStore , 0 ) ; 
		}

	public unsafe void RemoveCRLs()
		{
		uint storeFlags = MapX509StoreFlags(StoreLocation.CurrentUser, OpenFlags.ReadWrite);
		IntPtr hCertStore = CertOpenStore( 
								new IntPtr(CERT_STORE_PROV_SYSTEM),
								X509_ASN_ENCODING | PKCS_7_ASN_ENCODING,
                                                      	IntPtr.Zero,
                                                      	storeFlags, 
                                                      	"CA");
		bool deleted = false ; 

		if( hCertStore != IntPtr.Zero ) //remove them if not there
			{
			for( int i = 0 ; i < crlContexts.Length ; i++ )
				{
				deleted = CertDeleteCRLFromStore( crlContexts[i] ) ; 
				if( !deleted )
					Console.WriteLine( "Unable to delete: " + i.ToString() ) ; 
				}
			}

		bool closed = CertCloseStore( hCertStore , 0 ) ; 
		}

	IntPtr[] crlContexts ; 

        static uint MapX509StoreFlags (StoreLocation storeLocation, OpenFlags flags) {
            uint dwFlags = 0;
            uint openMode = ((uint)flags) & 0x3;
            switch (openMode) {
            case (uint) OpenFlags.ReadOnly:
                dwFlags |= CERT_STORE_READONLY_FLAG;
                break;
            case (uint) OpenFlags.MaxAllowed:
                dwFlags |= CERT_STORE_MAXIMUM_ALLOWED_FLAG;
                break;
            }

            if ((flags & OpenFlags.OpenExistingOnly) == OpenFlags.OpenExistingOnly)
                dwFlags |= CERT_STORE_OPEN_EXISTING_FLAG;
            if ((flags & OpenFlags.IncludeArchived) == OpenFlags.IncludeArchived)
                dwFlags |= CERT_STORE_ENUM_ARCHIVED_FLAG;

            if ((storeLocation & StoreLocation.LocalMachine) == StoreLocation.LocalMachine)
                dwFlags |= CERT_SYSTEM_STORE_LOCAL_MACHINE;
            else if ((storeLocation & StoreLocation.CurrentUser) == StoreLocation.CurrentUser)
                dwFlags |= CERT_SYSTEM_STORE_CURRENT_USER;

            return dwFlags;
        }

       [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
       static extern unsafe bool CryptAcquireContext( 
       	[Out] IntPtr* phProv ,
       	string pszContainer , 
       	string pszProvider , 
       	uint dwProvType , 
       	uint dwFlags ) ; 
       			


       [DllImport("CRYPT32.dll", CharSet=CharSet.Auto, SetLastError=true)]
       	static extern IntPtr CertDuplicateCRLContext( IntPtr pCrlContext ) ; 

       [DllImport("CRYPT32.dll", CharSet=CharSet.Auto, SetLastError=true)]
       	static extern bool CertDeleteCRLFromStore( IntPtr pCrlContext ) ; 
       	
	
       [DllImport("CRYPT32.dll", CharSet=CharSet.Auto, SetLastError=true)]
       static extern bool CryptQueryObject (
            uint   dwObjectType,
            IntPtr pvObject,
            uint   dwExpectedContentTypeFlags,
            uint   dwExpectedFormatTypeFlags,
            uint   dwFlags,
            IntPtr pdwMsgAndCertEncodingType,
            IntPtr pdwContentType,
            IntPtr pdwFormatType,
            IntPtr phCertStore,
            IntPtr phMsg,
            IntPtr ppvContext);


        uint CERT_QUERY_OBJECT_FILE = 1;
	uint CERT_STORE_PROV_SYSTEM           = 10;
	uint X509_ASN_ENCODING           = 0x00000001;
	uint PKCS_7_ASN_ENCODING         = 0x00010000;
        // cert store flags
        internal const uint CERT_STORE_ADD_ALWAYS                              = 4;
        internal const uint CERT_STORE_ENUM_ARCHIVED_FLAG               = 0x00000200;
        internal const uint CERT_STORE_READONLY_FLAG                    = 0x00008000;
        internal const uint CERT_STORE_OPEN_EXISTING_FLAG               = 0x00004000;
        internal const uint CERT_STORE_MAXIMUM_ALLOWED_FLAG             = 0x00001000;

        // cert store location
        internal const uint CERT_SYSTEM_STORE_LOCATION_SHIFT                = 16;

        internal const uint CERT_SYSTEM_STORE_CURRENT_USER_ID               = 1;
        internal const uint CERT_SYSTEM_STORE_LOCAL_MACHINE_ID              = 2;

        internal const uint CERT_SYSTEM_STORE_CURRENT_USER                  = ((int) CERT_SYSTEM_STORE_CURRENT_USER_ID << (int) CERT_SYSTEM_STORE_LOCATION_SHIFT);
        internal const uint CERT_SYSTEM_STORE_LOCAL_MACHINE                 = ((int) CERT_SYSTEM_STORE_LOCAL_MACHINE_ID << (int) CERT_SYSTEM_STORE_LOCATION_SHIFT);

        // cert query content types.
        internal const uint CERT_QUERY_CONTENT_CRL                = 3;

        // cert query content flags.
        internal const uint CERT_QUERY_CONTENT_FLAG_CRL                     = (1 << (int) CERT_QUERY_CONTENT_CRL);
 
        internal const uint CERT_QUERY_FORMAT_BINARY                = 1;
        internal const uint CERT_QUERY_FORMAT_BASE64_ENCODED        = 2;
        internal const uint CERT_QUERY_FORMAT_ASN_ASCII_HEX_ENCODED = 3;

        internal const uint CERT_QUERY_FORMAT_FLAG_BINARY                   = (1 << (int) CERT_QUERY_FORMAT_BINARY);
        internal const uint CERT_QUERY_FORMAT_FLAG_BASE64_ENCODED           = (1 << (int) CERT_QUERY_FORMAT_BASE64_ENCODED);
        internal const uint CERT_QUERY_FORMAT_FLAG_ASN_ASCII_HEX_ENCODED    = (1 << (int) CERT_QUERY_FORMAT_ASN_ASCII_HEX_ENCODED);
        internal const uint CERT_QUERY_FORMAT_FLAG_ALL                      =
                                       (CERT_QUERY_FORMAT_FLAG_BINARY |
                                        CERT_QUERY_FORMAT_FLAG_BASE64_ENCODED |
                                        CERT_QUERY_FORMAT_FLAG_ASN_ASCII_HEX_ENCODED);

        bool MyCryptQueryObject (
            uint                     dwObjectType,
            object                   pvObject,
            uint                     dwExpectedContentTypeFlags,
            uint                     dwExpectedFormatTypeFlags,
            uint                     dwFlags,
            IntPtr                   pdwMsgAndCertEncodingType,
            IntPtr                   pdwContentType,
            IntPtr                   pdwFormatType,
            IntPtr                   phCertStore,
            IntPtr                   phMsg,
            IntPtr                   ppvContext) {

            bool result = false;
            GCHandle handle = GCHandle.Alloc(pvObject, GCHandleType.Pinned);
            IntPtr pbData = handle.AddrOfPinnedObject();

            try {
                if (pvObject == null)
                    throw new ArgumentNullException("pvObject");

                result = CryptQueryObject(dwObjectType,
                                                      pbData,
                                                      dwExpectedContentTypeFlags,
                                                      dwExpectedFormatTypeFlags,
                                                      dwFlags,
                                                      pdwMsgAndCertEncodingType,
                                                      pdwContentType,
                                                      pdwFormatType,
                                                      phCertStore,
                                                      phMsg,
                                                      ppvContext);
            }
            finally {
                if (handle.IsAllocated)
                   handle.Free();
            }

            return result;
        }
        static VersionFlag IsNewCrypt32()
        	{        	
        	string root = Environment.GetEnvironmentVariable( "SystemRoot" ) ; 
        	string file = root + "\\system32\\crypt32.dll" ; 
        	FileVersionInfo fi = FileVersionInfo.GetVersionInfo( file ) ; 
        	int major = fi.FileMajorPart ; 
        	int minor = fi.FileMinorPart ; 
        	int build = fi.FileBuildPart ; 
        	int priv = fi.FilePrivatePart ; 
        	//Win2k3
        	if(Environment.OSVersion.Version.Major==5 && Environment.OSVersion.Version.Minor==2)
        	 	{
        	 	return VersionFlag.Win2K3 ; 
        	 	}
        	//Win2k
        	if(Environment.OSVersion.Version.Major==5 && Environment.OSVersion.Version.Minor==0)
        	 	{
        	 	if( major == 5 && minor == 131 && build == 2195 && priv >= 6620 )
        	 		return VersionFlag.New2000 ; 
        	 	return VersionFlag.Old2000 ; 
        	 	}
        	//WinXP
        	if(Environment.OSVersion.Version.Major==5 && Environment.OSVersion.Version.Minor==1)
        		{
        	 	if( major == 5 && minor == 131 && build == 2600 && priv >= 1163 )
        	 		return VersionFlag.NewXP ; 
        	 	return VersionFlag.OldXP ; 
        	 	}

        	//Win9x
        	if( Utility.IsWin9x )
        		return VersionFlag.Old2000 ; 
      	 	return VersionFlag.Win2K3 ; //default
        	}
	}
	public enum VersionFlag
		{
		OldXP = 1 , 
		Old2000 = 2 , 
		NewXP = 3 , 
		New2000 = 4 , 
		Win2K3 = 5
		}
	public class X509ExtensionTest : X509Extension
	{
		public static byte[] MyData = {1,2,3,4,5} ; 
		public static string MyOid = "1.2.345" ; 
		public X509ExtensionTest() : base( MyOid , MyData , false )
			{
			}
		public X509ExtensionTest(bool s) : base()
			{
			}
		public bool Test()
		{
			return true ; 
		}
	}
}
public delegate void Import<T>( byte[] rawdata , T password , X509KeyStorageFlags flags ) ; 
public delegate byte[] Export<T>( X509ContentType type , T t ) ; 
