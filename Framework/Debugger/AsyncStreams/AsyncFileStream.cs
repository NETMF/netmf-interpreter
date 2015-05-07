using System;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.SPOT.Debugger
{
    public class AsyncFileStream : GenericAsyncStream
    {
        private string m_fileName = null;

        public AsyncFileStream(string file, System.IO.FileShare share)
            : base(OpenHandle(file, share))
        {
            m_fileName = file;
        }

        static private SafeFileHandle OpenHandle( string file, System.IO.FileShare share )
        {
            if(file == null || file.Length == 0)
            {
                throw new ArgumentNullException( "file" );
            }

            SafeFileHandle handle = NativeMethods.CreateFile(file, NativeMethods.GENERIC_READ | NativeMethods.GENERIC_WRITE, share, NativeMethods.NULL, System.IO.FileMode.Open, NativeMethods.FILE_FLAG_OVERLAPPED, NativeMethods.NULL);
            
            if(handle.IsInvalid)
            {
                throw new InvalidOperationException( String.Format( "Cannot open {0}", file ) );
            }

            return handle;
        }

        public String Name
        {
            get
            {
                return m_fileName;
            }
        }

        public unsafe override int AvailableCharacters
        {
            get
            {
                int bytesRead;
                int totalBytesAvail;
                int bytesLeftThisMessage;

                if(NativeMethods.PeekNamedPipe( m_handle.DangerousGetHandle(), (byte*)NativeMethods.NULL, 0, out bytesRead, out totalBytesAvail, out bytesLeftThisMessage ) == false)
                {
                    totalBytesAvail = 0;
                }

                return totalBytesAvail;
            }
        }
    }
}