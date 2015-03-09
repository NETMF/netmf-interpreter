////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;

namespace DumpTypeHashes
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    class Engine
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine( "Usage: DumpTypeHashes <assembly> ..." );
                return;
            }

            foreach(string s in args)
            {
                Assembly   assm = Assembly.LoadFrom( s );
                SortedList sl   = new SortedList();

                foreach(Type t in assm.GetTypes())
                {
                    if(sl.ContainsKey( t.FullName ) == false)
                    {
                        sl.Add( t.FullName, Microsoft.SPOT.Debugger.BinaryFormatter.LookupHash( t ) );
                    }
                }

                foreach(string s2 in sl.Keys)
                {
                    Console.WriteLine( "{2} {1:X8} {0}", s2, sl[ s2 ], assm.GetName() );
                }
            }
        }
    }
}
