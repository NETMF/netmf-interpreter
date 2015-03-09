using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;

using _DBG = Microsoft.SPOT.Debugger;
using _WP  = Microsoft.SPOT.Debugger.WireProtocol;

namespace Microsoft.SPOT.Tools
{
    class SerialDump : IDisposable
    {
        _DBG.Engine m_eng;
        FileStream  m_output;
        bool        m_fTimestamp;
        bool        m_fUsb;
        bool        m_fNewLine = true;

        SerialDump( string[] args )
        {
            _DBG.PortDefinition pd       = null;
            string              port     = null;
            uint                baudrate = 0;

            for(int i=0; i<args.Length; i++)
            {
                string arg = args[i];

                if(String.Compare( arg, "-dump", true ) == 0)
                {
                    m_output = new FileStream( args[++i], FileMode.Create, FileAccess.ReadWrite, FileShare.Read );

                    continue;
                }

                if(String.Compare( arg, "-timestamp", true ) == 0)
                {
                    m_fTimestamp = true;

                    continue;
                }

                if(String.Compare( arg, "-usb", true ) == 0)
                {
                    m_fUsb = true;

                    continue;
                }

                if(port     == null) { port     =               arg  ; continue; }
                if(baudrate == 0   ) { baudrate = UInt32.Parse( arg ); continue; }
            }

            if(m_fUsb)
            {
                _DBG.PortDefinition[] ports = _DBG.AsyncUsbStream.EnumeratePorts();

                if(port == null)
                {
                    if(ports.Length == 0)
                    {
                        System.Console.WriteLine( "No Usb SPOT device is present" );

                        throw new ApplicationException();
                    }
                    else if(ports.Length == 1)
                    {
                        pd = ports[0];
                    }
                    else
                    {
                        System.Console.WriteLine( "More than one USB SPOT device is present" );
                        System.Console.WriteLine( "To dump data from a specific device, choose your device from the list below and execute:" );
                        System.Console.WriteLine( "  serialdump -usb <device> " );
                        System.Console.WriteLine( "" );

                        //
                        // More than one usb device attached; dump list so user can choose
                        //
                        for(int i = 0; i < ports.Length; ++i)
                        {
                            System.Console.WriteLine( "Device " + i + ": " + ports[i].DisplayName );
                        }

                        throw new ApplicationException();
                    }
                }
                else
                {
                    foreach(_DBG.PortDefinition pd2 in ports)
                    {
                        if(port.Equals( pd2.DisplayName ))
                        {
                            pd = pd2;
                            break;
                        }
                    }
                }
            }

            if(pd == null)
            {
                if(port     == null) port     = "COM1";
                if(baudrate == 0   ) baudrate = 115200;

                pd = _DBG.PortDefinition.CreateInstanceForSerial( port, port, baudrate );
            }

            m_eng = new _DBG.Engine( pd );
        }

        void Run()
        {
            m_eng.Silent = true;
            m_eng.Start();

            try
            {
                m_eng.OnNoise += new _DBG.NoiseEventHandler(OnNoise);
                m_eng.OnMessage += new _DBG.MessageEventHandler(OnMessage);

                Console.WriteLine("####### PRESS RETURN TO EXIT #######");
                Console.ReadLine();
            }
            finally
            {
                m_eng.OnNoise -= new _DBG.NoiseEventHandler(OnNoise);
                m_eng.OnMessage -= new _DBG.MessageEventHandler(OnMessage);

                m_eng.Stop();
                m_eng = null;
            }
        }

        void OnNoise( byte[] buf, int offset, int count )
        {
            HandleOutput( buf, offset, count );
        }

        void OnMessage( _WP.IncomingMessage msg, string text )
        {
            byte[] buf = Encoding.ASCII.GetBytes( text );

            HandleOutput( buf, 0, buf.Length );
        }

        void HandleOutput( byte[] buf, int offset, int count )
        {
            while(count-- > 0)
            {
                if(m_fTimestamp)
                {
                    if(m_fNewLine)
                    {
                        HandleOutput( String.Format( "[{0:HH:mm:ss:fff}]  ", DateTime.Now ) );
                    }
                }

                char c = (char)buf[offset++];

                HandleOutput( new String( c, 1 ) );

                m_fNewLine = (c == '\n');
            }
        }

        void HandleOutput( string text )
        {
            Console.Write( text );

            byte[] buf = Encoding.ASCII.GetBytes( text );

            if(m_output != null) m_output.Write( buf, 0, buf.Length );
        }

        static void Main( string[] args )
        {
            try
            {
                if (String.Compare(args[0], "-?", true) == 0 || String.Compare(args[0], "/?", true) == 0)
                {
                    Console.WriteLine("Usage: SerialDump.exe [-dump <dump_file>] [-timestamp] [-usb] [port] [baudrate]");
                    return;
                }

                using (SerialDump o = new SerialDump(args))
                {
                    o.Run();
                }
            }
            catch (ApplicationException)
            {
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}", e.ToString());
            }
        }

        ~SerialDump()
        {
            Dispose(false);
        }

        protected void Dispose(bool disposing)
        {
            if (m_output != null)
            {
                m_output.Close();
                m_output.Dispose();
                m_output = null;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
