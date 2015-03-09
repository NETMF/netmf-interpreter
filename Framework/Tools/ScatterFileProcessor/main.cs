using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;

using Microsoft.SPOT.Tasks.ScatterFile;

namespace ScatterFileProcessor
{    
	class Bench : IEnvironment
	{
        ProcessStartInfo m_startInfo;

        public Bench()
        {
            m_startInfo = Process.GetCurrentProcess().StartInfo;
        }

        public string GetVariable( string name )
        {
            return m_startInfo.EnvironmentVariables[name];
        }
    
        static void Usage()
        {
            Console.WriteLine( "Valid options:\n"                                  );
            Console.WriteLine( "    -import <scatter file input> <xml output>\n"   );
            Console.WriteLine( "    -process <xml input> <scatter file output> \n" );
        }

        static int Main( string[] args )
        {
            try
            {
                bool fUsage = true;


                for(int i=0; i<args.Length; i++)
                {
                    string s = args[i].ToLower();

                    if(s == "-import")
                    {
                        if(i + 2 >= args.Length)
                        {
                            Console.WriteLine( "Missing parameter for {0}\n", args[i] );
                            fUsage = true;
                            break;
                        }

                        Document.Convert( args[i+1], args[i+2] );

                        i += 2;
                        fUsage = false;
                        continue;
                    }

                    if(s == "-process")
                    {
                        if(i + 2 >= args.Length)
                        {
                            Console.WriteLine( "Missing parameter for {0}\n", args[i] );
                            fUsage = true;
                            break;
                        }

                        
                        Document doc = Document.Load( args[i+1], new Bench() );

                        string[] res = doc.Execute();

                        using(StreamWriter sw = new StreamWriter( args[i+2] ))
                        {
                            foreach(string line in res)
                            {
                                sw.WriteLine( "{0}", line );
                            }
                        }

                        i += 2;
                        fUsage = false;
                        continue;
                    }

                    Console.WriteLine( "Unknown option: {0}\n", args[i] );
                    fUsage = true;
                    break;
                }

                if(fUsage) Usage();
            }
            catch(Exception e)
            {
                Console.WriteLine( "{0}", e.ToString() );
				return -1;
            }

            return 0;
        }
	}
}
