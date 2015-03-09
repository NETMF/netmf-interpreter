////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;
using System.Management;
using System.Net;

namespace Microsoft.SPOT.Debugger
{
    // This is an internal object implementing IAsyncResult with fields
    // for all of the relevant data necessary to complete the IO operation.
    // This is used by AsyncFSCallback and all of the async methods.
    unsafe internal class AsyncFileStream_AsyncResult : IAsyncResult
    {
        private unsafe static readonly IOCompletionCallback s_callback = new IOCompletionCallback( DoneCallback );

        internal AsyncCallback     m_userCallback;
        internal Object            m_userStateObject;
        internal ManualResetEvent  m_waitHandle;

        internal GCHandle          m_bufferHandle;    // GCHandle to pin byte[].
        internal bool              m_bufferIsPinned;  // Whether our m_bufferHandle is valid.

        internal bool              m_isWrite;         // Whether this is a read or a write
        internal bool              m_isComplete;
        internal bool              m_EndXxxCalled;    // Whether we've called EndXxx already.
        internal int               m_numBytes;        // number of bytes read OR written
        internal int               m_errorCode;
        internal NativeOverlapped* m_overlapped;
        
        internal AsyncFileStream_AsyncResult( AsyncCallback userCallback, Object stateObject, bool isWrite )
        {
            m_userCallback    = userCallback;
            m_userStateObject = stateObject;
            m_waitHandle      = new ManualResetEvent( false );

            m_isWrite         = isWrite;

            Overlapped overlapped = new Overlapped( 0, 0, IntPtr.Zero, this );

            m_overlapped = overlapped.Pack( s_callback, null );            
        }

        public virtual Object AsyncState
        {
            get { return m_userStateObject; }
        }

        public bool IsCompleted
        {
            get { return m_isComplete;  }
            set { m_isComplete = value; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return m_waitHandle; }
        }

        public bool CompletedSynchronously
        {
            get { return false; }
        }

        internal void SignalCompleted()
        {
            AsyncCallback userCallback = null;

            lock(this)
            {
                if(m_isComplete == false)
                {
                    userCallback = m_userCallback;

                    ManualResetEvent wh = m_waitHandle;
                    if(wh != null && wh.Set() == false)
                    {
                        NativeMethods.ThrowIOException( string.Empty );
                    }

                    // Set IsCompleted to true AFTER we've signalled the WaitHandle!
                    // Necessary since we close the WaitHandle after checking IsCompleted,
                    // so we could cause the SetEvent call to fail.
                    m_isComplete = true;

                    ReleaseMemory();
                }
            }

            if(userCallback != null)
            {
                userCallback( this );
            }
        }

        internal void WaitCompleted()
        {
            ManualResetEvent wh = m_waitHandle;
            if(wh != null)
            {
                if(m_isComplete == false)
                {
                    wh.WaitOne();
                    // There's a subtle race condition here.  In AsyncFSCallback,
                    // I must signal the WaitHandle then set _isComplete to be true,
                    // to avoid closing the WaitHandle before AsyncFSCallback has
                    // signalled it.  But with that behavior and the optimization
                    // to call WaitOne only when IsCompleted is false, it's possible
                    // to return from this method before IsCompleted is set to true.
                    // This is currently completely harmless, so the most efficient
                    // solution of just setting the field seems like the right thing
                    // to do.     -- BrianGru, 6/19/2000
                    m_isComplete = true;
                }
                wh.Close();
            }
        }

        internal NativeOverlapped* OverlappedPtr
        {
            get { return m_overlapped; }
        }

        internal unsafe void ReleaseMemory()
        {
            if(m_overlapped != null)
            {
                Overlapped.Free( m_overlapped );
                m_overlapped = null;
            }

            UnpinBuffer();
        }

        internal void PinBuffer( byte[] buffer )
        {
            m_bufferHandle   = GCHandle.Alloc( buffer, GCHandleType.Pinned );
            m_bufferIsPinned = true;
        }

        internal void UnpinBuffer()
        {
            if(m_bufferIsPinned)
            {
                m_bufferHandle.Free();
                m_bufferIsPinned = false;
            }
        }

        // this callback is called by a free thread in the threadpool when the IO operation completes.
        unsafe private static void DoneCallback( uint errorCode, uint numBytes, NativeOverlapped* pOverlapped )
        {
            if(errorCode == NativeMethods.ERROR_OPERATION_ABORTED)
            {
                numBytes  = 0;
                errorCode = 0;

                return;
            }

            // Unpack overlapped
            Overlapped overlapped = Overlapped.Unpack( pOverlapped );
            // Free the overlapped struct in EndRead/EndWrite.

            // Extract async result from overlapped
            AsyncFileStream_AsyncResult asyncResult = (AsyncFileStream_AsyncResult)overlapped.AsyncResult;


            asyncResult.m_numBytes  = (int)numBytes;
            asyncResult.m_errorCode = (int)errorCode;

            asyncResult.SignalCompleted();
        }
    }
}
