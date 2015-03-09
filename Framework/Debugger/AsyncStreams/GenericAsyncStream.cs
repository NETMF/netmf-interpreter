using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Microsoft.SPOT.Debugger
{
    public class GenericAsyncStream : System.IO.Stream, IDisposable, WireProtocol.IStreamAvailableCharacters
    {
        protected SafeHandle m_handle;
        protected ArrayList m_outstandingRequests;

        protected GenericAsyncStream( SafeHandle handle )
        {
            System.Diagnostics.Debug.Assert( handle != null );

            m_handle = handle;

            if( ThreadPool.BindHandle( m_handle ) == false )
            {
                throw new IOException( "BindHandle Failed" );
            }

            m_outstandingRequests = ArrayList.Synchronized( new ArrayList( ) );
        }

        ~GenericAsyncStream( )
        {
            Dispose( false );
        }

        public void CancelPendingIO( )
        {
            lock( m_outstandingRequests.SyncRoot )
            {
                for( int i = m_outstandingRequests.Count - 1; i >= 0; i-- )
                {
                    AsyncFileStream_AsyncResult asfar = ( AsyncFileStream_AsyncResult )m_outstandingRequests[ i ];
                    asfar.SignalCompleted( );
                }

                m_outstandingRequests.Clear( );
            }
        }

        protected override void Dispose( bool disposing )
        {
            // Nothing will be done differently based on whether we are disposing vs. finalizing.
            lock( this )
            {
                if( m_handle != null && !m_handle.IsInvalid )
                {
                    if( disposing )
                    {
                        CancelPendingIO( );
                    }

                    m_handle.Close( );
                    m_handle.SetHandleAsInvalid( );
                }
            }

            base.Dispose( disposing );
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { throw new NotSupportedException( "Not Supported" ); }
        }

        public override long Position
        {
            get { throw new NotSupportedException( "Not Supported" ); }
            set { throw new NotSupportedException( "Not Supported" ); }
        }

        public override IAsyncResult BeginRead( byte[ ] buffer, int offset, int count, AsyncCallback callback, object state )
        {
            return BeginReadCore( buffer, offset, count, callback, state );
        }

        public override IAsyncResult BeginWrite( byte[ ] buffer, int offset, int count, AsyncCallback callback, object state )
        {
            return BeginWriteCore( buffer, offset, count, callback, state );
        }

        public override void Close( )
        {
            Dispose( true );
        }

        public override int EndRead( IAsyncResult asyncResult )
        {
            AsyncFileStream_AsyncResult afsar = CheckParameterForEnd( asyncResult, false );

            afsar.WaitCompleted( );

            m_outstandingRequests.Remove( afsar );

            // Now check for any error during the read.
            if( afsar.m_errorCode != 0 )
                throw new IOException( "Async Read failed", afsar.m_errorCode );

            return afsar.m_numBytes;
        }

        public override void EndWrite( IAsyncResult asyncResult )
        {
            AsyncFileStream_AsyncResult afsar = CheckParameterForEnd( asyncResult, true );

            //afsar.WaitCompleted();

            afsar.m_waitHandle.WaitOne( afsar.m_numBytes );

            m_outstandingRequests.Remove( afsar );

            // Now check for any error during the write.
            if( afsar.m_errorCode != 0 )
                throw new IOException( "Async Write failed", afsar.m_errorCode );
        }

        public override void Flush( )
        {
        }

        public override int Read( byte[ ] buffer, int offset, int count )
        {
            IAsyncResult result = BeginRead( buffer, offset, count, null, null );
            return EndRead( result );
        }

        public override long Seek( long offset, SeekOrigin origin )
        {
            throw new NotSupportedException( "Not Supported" );
        }

        public override void SetLength( long value )
        {
            throw new NotSupportedException( "Not Supported" );
        }

        public override void Write( byte[ ] buffer, int offset, int count )
        {
            IAsyncResult result = BeginWrite( buffer, offset, count, null, null );
            EndWrite( result );
        }

        public SafeHandle Handle
        {
            get
            {
                return m_handle;
            }
        }

        public virtual int AvailableCharacters
        {
            get
            {
                return 0;
            }
        }

        internal void CheckParametersForBegin( byte[ ] array, int offset, int count )
        {
            if( array == null )
                throw new ArgumentNullException( "array" );

            if( offset < 0 )
                throw new ArgumentOutOfRangeException( "offset" );

            if( count < 0 || array.Length - offset < count )
                throw new ArgumentOutOfRangeException( "count" );

            if( m_handle.IsInvalid )
            {
                throw new ObjectDisposedException( null );
            }
        }

        internal AsyncFileStream_AsyncResult CheckParameterForEnd( IAsyncResult asyncResult, bool isWrite )
        {
            if( asyncResult == null )
                throw new ArgumentNullException( "asyncResult" );

            AsyncFileStream_AsyncResult afsar = asyncResult as AsyncFileStream_AsyncResult;
            if( afsar == null || afsar.m_isWrite != isWrite )
                throw new ArgumentException( "asyncResult" );
            if( afsar.m_EndXxxCalled )
                throw new InvalidOperationException( "EndRead called twice" );
            afsar.m_EndXxxCalled = true;

            return afsar;
        }

        private unsafe IAsyncResult BeginReadCore( byte[ ] array, int offset, int count, AsyncCallback userCallback, Object stateObject )
        {
            CheckParametersForBegin( array, offset, count );

            AsyncFileStream_AsyncResult asyncResult = new AsyncFileStream_AsyncResult( userCallback, stateObject, false );

            if( count == 0 )
            {
                asyncResult.SignalCompleted( );
            }
            else
            {
                // Keep the array in one location in memory until the OS writes the
                // relevant data into the array.  Free GCHandle later.
                asyncResult.PinBuffer( array );

                fixed( byte* p = array )
                {
                    int numBytesRead = 0;
                    bool res;

                    res = NativeMethods.ReadFile( m_handle.DangerousGetHandle( ), p + offset, count, out numBytesRead, asyncResult.OverlappedPtr );
                    if( res == false )
                    {
                        if( HandleErrorSituation( "BeginRead", false ) )
                        {
                            asyncResult.SignalCompleted( );
                        }
                        else
                        {
                            m_outstandingRequests.Add( asyncResult );
                        }
                    }
                }
            }

            return asyncResult;
        }

        private unsafe IAsyncResult BeginWriteCore( byte[ ] array, int offset, int count, AsyncCallback userCallback, Object stateObject )
        {
            CheckParametersForBegin( array, offset, count );

            AsyncFileStream_AsyncResult asyncResult = new AsyncFileStream_AsyncResult( userCallback, stateObject, true );

            if( count == 0 )
            {
                asyncResult.SignalCompleted( );
            }
            else
            {
                asyncResult.m_numBytes = count;

                // Keep the array in one location in memory until the OS writes the
                // relevant data into the array.  Free GCHandle later.
                asyncResult.PinBuffer( array );

                fixed( byte* p = array )
                {
                    int numBytesWritten = 0;
                    bool res;

                    res = NativeMethods.WriteFile( m_handle.DangerousGetHandle( ), p + offset, count, out numBytesWritten, asyncResult.OverlappedPtr );
                    if( res == false )
                    {
                        if( HandleErrorSituation( "BeginWrite", true ) )
                        {
                            asyncResult.SignalCompleted( );
                        }
                        else
                        {
                            m_outstandingRequests.Add( asyncResult );
                        }
                    }
                }
            }

            return asyncResult;
        }

        protected virtual bool HandleErrorSituation( string msg, bool isWrite )
        {
            int hr = Marshal.GetLastWin32Error( );

            // For invalid handles, detect the error and close ourselves
            // to prevent a malicious app from stealing someone else's file
            // handle when the OS recycles the handle number.
            if( hr == NativeMethods.ERROR_INVALID_HANDLE )
            {
                m_handle.Close( );
            }

            if( hr != NativeMethods.ERROR_IO_PENDING )
            {
                if( isWrite == false && hr == NativeMethods.ERROR_HANDLE_EOF )
                {
                    throw new EndOfStreamException( msg );
                }

                throw new Win32Exception( hr, msg );
            }

            return false;
        }


        #region IDisposable Members

        void IDisposable.Dispose( )
        {
            base.Dispose( true );

            Dispose( true );

            GC.SuppressFinalize( this );
        }

        #endregion
    }
}