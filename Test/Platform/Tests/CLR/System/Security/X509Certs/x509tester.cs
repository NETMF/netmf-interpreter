using System;
using System.Security.Cryptography.X509Certificates;
namespace X509Tester
{
	public class Test
	{
		public static void Diagnostic()
			{
			Worker w = new Worker( true , false , false ) ; //1 means that debug was turned on
			w.AddCRLs() ; 
			w.RemoveCRLs() ; 
			}
		public static void Main( string[] args )
		{
			Test t = new Test() ; 
			t.Run( args ) ; 
		}
		public void Run( string[] args )
			{
			Worker w = null ; 
			bool failed = false ; 
			try
				{
				bool TestAll = false , clean = true ; 

				int i = 0 , numTimes = 1 , start = 0 ; 

				if( args.Length == 0 )
				{
					Console.WriteLine( "Error no arguments" ) ;
					return ; 
				}

				if( args[0] == "secret" ) 
					{
					Diagnostic() ; 
					return ; 
					}

				if( args[0] == "-d" ) //turn on debug
				{
					i = 1 ;			
				}
				if( args.Length >= 2 && args[1] == "-nt" )
					{
					numTimes = 100 ; 
					i = 2 ;
					}
				start = i ; 
				w = new Worker( i == 1 , clean , numTimes == 100 ) ; //1 means that debug was turned on

				for( int j = 0 ; j < numTimes ; j++ )
				{
					Console.WriteLine( j.ToString() ) ; 
					GC.Collect() ; 
					for( i = start ; i < args.Length ; i++ )
					{
						if( args[i] == "-noclean" )
							w.clean = false ; 
						if( args[i] == "-all" )
							TestAll = true ; 

						if( args[i] == "-003" || TestAll )
						{
							w.Test003a() ; 
							//w.Test003b() ; 
						}
						if( args[i] == "-049" )
							{
							w.Test049() ; 
							}
						if( args[i] == "-300" )
							{
							w.Test300() ; 
							}
						if( args[i] == "-200" )
							{
							w.Test200() ; 
							}
						if( args[i] == "-201" )
							{
							w.Test201() ; 
							}
						if( args[i] == "-202" )
							{
							w.Test202() ; 
							}
						if( args[i] == "-100" || TestAll )
							w.Test100() ; 
						if( args[i] == "-101" || TestAll )
							w.Test101() ; 
						if( args[i] == "-102" || TestAll )
							w.Test102() ; 
						if( args[i] == "-103" || TestAll )
							w.Test103() ; 
						if( args[i] == "-104" || TestAll )
							w.Test104() ; 
						if( args[i] == "-105" || TestAll )
							w.Test105() ; 
						if( args[i] == "-106" || TestAll )
							w.Test106() ; 
						if( args[i] == "-090" || TestAll )
							w.Test090() ; 
						if( args[i] == "-080" || TestAll )
							w.Test080() ; 
						if( args[i] == "-081" || TestAll )
							w.Test081() ; 
						if( args[i] == "-082" || TestAll )
							w.Test082() ; 
						if( args[i] == "-083" || TestAll )
							w.Test083() ; 
						if( args[i] == "-070" || TestAll )
							w.Test070() ; 
						if( args[i] == "-060" || TestAll )
							w.Test060() ; 
						if( args[i] == "-061" || TestAll )
							w.Test061() ; 
						if( args[i] == "-050" || TestAll )
							w.Test050() ; 
						if( args[i] == "-051" || TestAll )
							w.Test051() ; 
						if( args[i] == "-005" || TestAll )
							w.Test005() ; 
						if( args[i] == "-008" || TestAll )
							w.Test008() ; 
						if( args[i] == "-009" || TestAll )
							w.Test009() ; 
						if( args[i] == "-011" || TestAll )
							w.Test011() ; 
						if( args[i] == "-014" || TestAll )
							w.Test014() ; 
						if( args[i] == "-015" || TestAll )
							w.Test015() ; 
						if( args[i] == "-016" || TestAll )
							w.Test016() ; 
						if( args[i] == "-017" || TestAll )
							w.Test017() ; 
						if( args[i] == "-019" || TestAll )
						{
							w.Test019b() ; 
						}
						if( args[i] == "-020" || TestAll )
						{
							w.Test020a() ; 
							w.Test020b() ; 
						}
						if( args[i] == "-021" || TestAll )
						{
							w.Test021a() ; 
							w.Test021b() ; 
							w.Test021c() ; 
						}
						if( args[i] == "-022" || TestAll )
						{
							w.Test022() ; 
						}
						if( args[i] == "-023" || TestAll )
						{
							w.Test023() ; 
							w.Test023b() ; 
						}
						if( args[i] == "-024" || TestAll )
						{
							w.Test024() ; 
						}
						if( args[i] == "-025" || TestAll )
						{
							w.Test025() ; 
						}
						if( args[i] == "-026"  )
						{
							w.Test026() ; 
							w.Test026b() ; 
						}
						if( args[i] == "-027" || TestAll )
						{
							w.Test027() ; 
						}
						if( args[i] == "-028" || TestAll )
						{
							w.Test028() ; 
						}
						if( args[i] == "-029" || TestAll )
						{
							w.Test029(StoreLocation.CurrentUser);
							w.SetLm() ; 
							w.Test029(StoreLocation.LocalMachine);
							w.Reset() ; 
						}
						if( args[i] == "-031" || TestAll )
						{
							w.Test031(StoreLocation.CurrentUser,false) ; 
							w.Test031(StoreLocation.CurrentUser,true);
							w.SetLm() ; 
							w.Test031(StoreLocation.LocalMachine,false) ; 
							w.Test031(StoreLocation.LocalMachine,true) ; 
							w.Reset() ; 
						}
						if( args[i] == "-032" || TestAll )
						{
							w.Test032() ; 
						}
						if( args[i] == "-033" || TestAll )
						{
							w.Test033() ; 
						}
						if( args[i] == "-034" || TestAll )
						{
							w.Test034() ; 
						}
						if( args[i] == "-035" || TestAll )
						{
	//						w.Test035("2.5.29.10") ; 
	//						w.Test035(null) ; 
							w.Test035() ; 
						}
						if( args[i] == "-036" || TestAll )
						{
							w.Test036() ; 
						}
						if( args[i] == "-037" || TestAll )
						{
							w.Test037() ; 
						}
						if( args[i] == "-040" || TestAll )
						{
							w.Test040() ; 
						}
						if( args[i] == "-041" || TestAll )
						{
							w.Test041() ; 
						}
						if( args[i] == "-042" || TestAll )
						{
							w.Test042() ; 
						}
						if( args[i] == "-043"  )
						{
						//	w.Test043(true) ; 
							w.Test043(false) ; 
						}
						if( args[i] == "-045" || TestAll )
						{
							w.Test045() ; 
						}
						if( args[i] == "-046" || TestAll )
						{
							w.Test046() ; 
						}
						if( args[i] == "-047" || TestAll )
						{
							w.Test047() ; 
						}

						if( TestAll )
							break ;
					}
				}
				}
			catch( Exception e )
				{
				Console.WriteLine( "\n\n\n**********Run caught an exception**********\n\n\n" ) ; 
				Console.WriteLine( e ) ; 
				failed = true ; 
				}

			w.DumpResults(failed) ; 
		}
	}
}
