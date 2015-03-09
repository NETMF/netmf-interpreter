using System.Collections;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Microsoft.SPOT.Debugger
{
    public class AsyncSerialStream : AsyncFileStream
    {
        public AsyncSerialStream( string port, uint baudrate ) : base( port, System.IO.FileShare.None )
        {
            NativeMethods.COMMTIMEOUTS cto = new NativeMethods.COMMTIMEOUTS(); cto.Initialize();
            NativeMethods.DCB          dcb = new NativeMethods.DCB         (); dcb.Initialize();

            NativeMethods.GetCommState( m_handle.DangerousGetHandle(), ref dcb );

            dcb.BaudRate = baudrate;
            dcb.ByteSize = 8;
            dcb.StopBits = 0;

            dcb.__BitField = 0;
            dcb.__BitField &= ~NativeMethods.DCB.mask_fDtrControl  ;
            dcb.__BitField &= ~NativeMethods.DCB.mask_fRtsControl  ;
            dcb.__BitField |=  NativeMethods.DCB.mask_fBinary      ;
            dcb.__BitField &= ~NativeMethods.DCB.mask_fParity      ;
            dcb.__BitField &= ~NativeMethods.DCB.mask_fOutX        ;
            dcb.__BitField &= ~NativeMethods.DCB.mask_fInX         ;
            dcb.__BitField &= ~NativeMethods.DCB.mask_fErrorChar   ;
            dcb.__BitField &= ~NativeMethods.DCB.mask_fNull        ;
            dcb.__BitField |=  NativeMethods.DCB.mask_fAbortOnError;

            NativeMethods.SetCommState( m_handle.DangerousGetHandle(), ref dcb );

            NativeMethods.SetCommTimeouts( m_handle.DangerousGetHandle(), ref cto );
        }

        public override int AvailableCharacters
        {
            get
            {
                NativeMethods.COMSTAT cs = new NativeMethods.COMSTAT(); cs.Initialize();
                uint           errors;

                NativeMethods.ClearCommError( m_handle.DangerousGetHandle(), out errors, ref cs );

                return (int)cs.cbInQue;
            }
        }

        protected override bool HandleErrorSituation( string msg, bool isWrite )
        {
            if(Marshal.GetLastWin32Error() == NativeMethods.ERROR_OPERATION_ABORTED)
            {
                NativeMethods.COMSTAT cs = new NativeMethods.COMSTAT(); cs.Initialize();
                uint           errors;

                NativeMethods.ClearCommError( m_handle.DangerousGetHandle(), out errors, ref cs );

                return true;
            }

            return base.HandleErrorSituation( msg, isWrite );
        }

        public void ConfigureXonXoff( bool fEnable )
        {
            NativeMethods.DCB dcb = new NativeMethods.DCB(); dcb.Initialize();

            NativeMethods.GetCommState( m_handle.DangerousGetHandle(), ref dcb );

            if(fEnable)
            {
                dcb.__BitField |= NativeMethods.DCB.mask_fOutX;
            }
            else
            {
                dcb.__BitField &= ~NativeMethods.DCB.mask_fOutX;
            }

            NativeMethods.SetCommState( m_handle.DangerousGetHandle(), ref dcb );
        }

        static public PortDefinition[] EnumeratePorts()
        {
            SortedList lst = new SortedList();

            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey( @"HARDWARE\DEVICEMAP\SERIALCOMM" );

                foreach(string name in key.GetValueNames())
                {
                    string         val = (string)key.GetValue( name );
                    PortDefinition pd  = PortDefinition.CreateInstanceForSerial( val, @"\\.\" + val, 115200 );

                    lst.Add( val, pd );
                }
            }
            catch
            {
            }

            ICollection      col = lst.Values;
            PortDefinition[] res = new PortDefinition[col.Count];

            col.CopyTo( res, 0 );

            return res;
        }
    }
}