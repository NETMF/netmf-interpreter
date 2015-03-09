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

using _DBG = Microsoft.SPOT.Debugger;

namespace Microsoft.SPOT.Tools
{
    class PortBooterHost
    {
        _DBG.Engine    m_eng;
        _DBG.PortBooter m_fl;
        uint           m_entrypoint;
        ArrayList      m_blocks = new ArrayList();

        PortBooterHost()
        {
        }

        uint ParseHex( string arg )
        {
            if(arg.StartsWith( "0x" )) arg = arg.Substring( 2 );
            if(arg.StartsWith( "0X" )) arg = arg.Substring( 2 );

            return UInt32.Parse( arg, System.Globalization.NumberStyles.HexNumber );
        }

        void Usage()
        {
            Console.WriteLine( "PortBooterHost <options>:" );
            Console.WriteLine( "" );
            Console.WriteLine( "  -nowait                     : immediately start transmission of data." );
            Console.WriteLine( "  -port <port>                : set COM port to use."                    );
            Console.WriteLine( "  -baudrate <speed>           : set speed of COM port to use."           );
            Console.WriteLine( "  -com1                       : use COM1 at 115200 baud."                );
            Console.WriteLine( "  -com2                       : use COM2 at 115200 baud."                );
            Console.WriteLine( "" );
            Console.WriteLine( "  -write <HEX file>           : program HEX file."                       );
            Console.WriteLine( "  -writeandexecute <HEX file> : program and execute HEX file."           );
            Console.WriteLine( "  -entrypoint <address>       : start execution from given location."    );
        }

        bool Process( string[] args )
        {
            string port     = null;
            uint   baudrate = 0;
            bool   fWait    = true;
            int    i;

            if(args.Length == 0)
            {
                Usage();
                return false;
            }

            for(i=0; i<args.Length; i++)
            {
                string arg = args[i].ToLower();

                if(arg == "-port"    ) { port     =               args[++i]  ; continue; }
                if(arg == "-baudrate") { baudrate = UInt32.Parse( args[++i] ); continue; }
                if(arg == "-nowait"  ) { fWait    = false                    ; continue; }

                if(arg == "-com1") { port = "COM1"; baudrate = 115200; continue; }
                if(arg == "-com2") { port = "COM2"; baudrate = 115200; continue; }

                if(arg == "-write")
                {
                    try
                    {
                        string file = args[++i];

                        Console.WriteLine( "Loading {0}...", file );

                        Microsoft.SPOT.Debugger.SRecordFile.Parse( file, m_blocks, null );

                        Console.WriteLine( "Loaded." );
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine( "{0}", e.ToString() );
                        return false;
                    }

                    continue;
                }

                if(arg == "-writeandexecute")
                {
                    try
                    {
                        string file = args[++i];

                        Console.WriteLine( "Loading {0}...", file );

                        m_entrypoint = Microsoft.SPOT.Debugger.SRecordFile.Parse( file, m_blocks, null );

                        Console.WriteLine( "Loaded." );
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine( "{0}", e.ToString() );
                        return false;
                    }

                    continue;
                }

                if(arg == "-entrypoint")
                {
                    m_entrypoint = ParseHex( args[++i] );

                    continue;
                }

                Usage();
                return false;
            }

            if(port == null || baudrate == 0)
            {
                Console.WriteLine( "No serial port specified!" );
                return false;
            }

            m_eng = new _DBG.Engine( new Microsoft.SPOT.Debugger.PortDefinition_Serial( port, port, baudrate ) );

            m_eng.Silent = true;

            m_eng.OnMessage += new _DBG.MessageEventHandler( OnMessage );

            m_eng.Start();

            if(fWait)
            {
                if(m_eng.TryToConnect( 5, 100 ))
                {
                    m_eng.RebootDevice();
                }
            }

            m_fl = new _DBG.PortBooter( m_eng );

            m_fl.OnProgress += new _DBG.PortBooter.ProgressEventHandler( this.OnProgress );

            m_fl.Start();

            if(fWait)
            {
                m_fl.WaitBanner( Int32.MaxValue, 2000 );
            }

            DateTime start = DateTime.Now;

            m_fl.Program( m_blocks );

            Console.WriteLine( "Execute: {0}", DateTime.Now - start );

            if(m_entrypoint != 0)
            {
                while(true)
                {
                    m_fl.Execute( m_entrypoint );

                    _DBG.PortBooter.Report r = m_fl.GetReport( 2000 );

                    if(r != null && r.type == _DBG.PortBooter.Report.State.EntryPoint)
                    {
                        break;
                    }
                }
            }

            m_fl.Stop();

            m_eng.Stop();

            return true;
        }

        void OnMessage( _DBG.WireProtocol.IncomingMessage msg, string text )
        {
            Console.Write( "{0}", text );
        }

        void OnProgress( _DBG.SRecordFile.Block bl, int offset, bool fLast )
        {
            if(fLast)
            {
                Console.Write( "\n" );
            }
            else
            {
                Console.Write( "Block: 0x{0:X8} - {1:d6} bytes  {2}%    \r", bl.address, bl.data.Length, offset * 100 / bl.data.Length );
            }
        }

        static void Main( string[] args )
        {
            try
            {
                PortBooterHost host = new PortBooterHost();

                host.Process( args );
            }
            catch(Exception e)
            {
                Console.WriteLine( "{0}", e.ToString() );
            }
        }
    }
}
