using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography ;
using System.Reflection ;
using System.Security.Permissions ; 
using System.Security ; 
using System.IO ; 
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Collections ; 
using System.Threading ; 
using System.Diagnostics ; 
using System.Text ; 

//Code coverage test for AsnEncodedDataCollection
public class Test
{
	static bool res = true ; 
	static AsnEncodedDataCollection asn = null ; 
	static bool Result
		{
		get
			{
			return res; 
			}
		set
			{
			if( !value ) 
				{
				Console.WriteLine( "*******************FAILURE*************" ) ; 
				StackTrace trace = new StackTrace(true) ; 
				StackFrame sf = trace.GetFrame(1) ; 
				Console.WriteLine( sf ) ; 
				}
			res = value && res; 
			}
		}
	static int Return 
		{
		get
			{
			Console.WriteLine( Result ? "Test Passed" : "Test Failed" ) ; 
			return Result ? 100 : 101 ; 
			}
		}
	public static int Main()
		{
		TestAdd() ; 
		TestRemove() ; 
		TestCopyTo() ; 
		return Return ; 
		}
	static void TestRemove()
		{
		asn = new AsnEncodedDataCollection() ; 
		try
			{
			asn.Remove( null ) ; 
			Result = false ; 
			}
		catch( ArgumentNullException )
			{
			Result = true ; 
			}
		catch( Exception ) 
			{
			Result = false ; 
			}
		}
	static void TestAdd()
		{
		asn = new AsnEncodedDataCollection() ; 
		try
			{
			asn.Add( null ) ; 
			Result = false ; 
			}
		catch( ArgumentNullException )
			{
			Result = true ; 
			}
		catch( Exception ) 
			{
			Result = false ; 
			}
		
		//add normal case
		AsnEncodedData data1 = new AsnEncodedData( oid , new byte[]{1,2,3} ) ; 
		AsnEncodedData data2 = new AsnEncodedData( oid , new byte[]{5,6,7} ) ; 
		asn = new AsnEncodedDataCollection(data1) ;
		asn.Add( data2 ) ;
		Result = asn.Count == 2 ; 

		//Add where OIDs don't match
		data2 = new AsnEncodedData(badOid , new byte[]{1,2,3,4} ) ; 
		asn = GetCollection(data1) ;
		try
			{
			asn.Add(data2) ; 
			Result = false ; 
			}
		catch(CryptographicException)
			{
			Result = true ; 
			}
		catch(Exception e)
			{
			Result = false ; 
			Console.WriteLine(e) ; 
			}

		//add w/ a empty Oid
		data2.Oid = new Oid() ; 
		asn = GetCollection(data1) ;
		try
			{
			asn.Add(data2) ; 
			Result = false ; 
			}
		catch(CryptographicException)
			{
			Result = true ; 
			}
		catch(Exception e)
			{
			Result = false ; 
			Console.WriteLine(e) ; 
			}
		}
	static AsnEncodedDataCollection GetCollection(AsnEncodedData data)
		{
		AsnEncodedDataCollection asn = new AsnEncodedDataCollection(data) ;
		FieldInfo fi = asn.GetType().GetField( "m_oid" , BindingFlags.NonPublic | BindingFlags.Instance ) ; 
		fi.SetValue( asn , oid ) ; 
		return asn ; 
		}
	static Oid oid = new Oid( "1.3.14.3.2.26" ) ; //SHA1
	static Oid badOid = new Oid( "1.3.14.3.2.27" ) ; //bogus OID
	static void TestCopyTo()
		{
		asn = GetCollection( new AsnEncodedData( oid , new byte[]{1,2,3} ) ); 
		AsnEncodedData[] array = new AsnEncodedData[asn.Count] ; 

		//straight up copy
		asn.CopyTo( array , 0 ) ; 
		Result = array[0] != null ; 

		//ary is null
		try
			{
			asn.CopyTo( null , 0 ) ;
			Result = false ; 
			}
		catch(ArgumentNullException)
			{
			Result = true ; 
			}
		catch(Exception e)
			{
			Result = false ; 
			Console.WriteLine(e) ;
			}

		//index is too big/small
		try
			{
			asn.CopyTo( array , -1 ) ;
			Result = false ; 
			}
		catch(ArgumentOutOfRangeException)
			{
			Result = true ; 
			}
		catch(Exception e)
			{
			Result = false ; 
			Console.WriteLine(e) ;
			}

		try
			{
			asn.CopyTo( array , 10 ) ;
			Result = false ; 
			}
		catch(ArgumentOutOfRangeException)
			{
			Result = true ; 
			}
		catch(Exception e)
			{
			Result = false ; 
			Console.WriteLine(e) ;
			}

		//count + length mistmatch
		try
			{
			asn.CopyTo( array , 1 ) ;
			Result = false ; 
			}
		catch(ArgumentException)
			{
			Result = true ; 
			}
		catch(Exception e)
			{
			Result = false ; 
			Console.WriteLine(e) ;
			}

		}
}
