////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "spot_io.h"


HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeFileStream::_ctor___VOID__STRING__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_String* hbPath;
    CLR_INT32                bufferSize;

    CLR_RT_HeapBlock* pThis =   stack.This();
    CLR_RT_HeapBlock* pArgs = &(stack.Arg1());

    hbPath     = pArgs[ 0 ].DereferenceString(); FAULT_ON_NULL(hbPath);
    bufferSize = pArgs[ 1 ].NumericByRef().s4;

    if (bufferSize < 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }
    
    TINYCLR_CHECK_HRESULT(CLR_RT_FileStream::CreateInstance( pThis[ FIELD__m_fs ], hbPath->StringText(), bufferSize ));
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeFileStream::Read___I4__SZARRAY_U1__I4__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(ReadWriteHelper( stack, TRUE ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeFileStream::Write___I4__SZARRAY_U1__I4__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(ReadWriteHelper( stack, FALSE ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeFileStream::ReadWriteHelper( CLR_RT_StackFrame& stack, BOOL isRead )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* bufHB = NULL;
    CLR_RT_FileStream*      fs;
    CLR_RT_HeapBlock*       nativeFileStreamHB = NULL;

    CLR_UINT8*              buffer;
    CLR_INT32               bufferLength;
    CLR_INT32               offset;
    CLR_INT32               count;
    CLR_RT_HeapBlock*       timeoutHB;
    CLR_INT64*              timeoutTicks;
    CLR_INT32               bytesProcessed = 0;
    bool                    fRes;

    bufHB        = stack.Arg1().DereferenceArray(); FAULT_ON_NULL_ARG(bufHB);
    buffer       = bufHB->GetFirstElement();
    bufferLength = (CLR_INT32)bufHB->m_numOfElements;
    offset       = stack.Arg2().NumericByRef().s4;
    count        = stack.Arg3().NumericByRef().s4;
    timeoutHB    = &(stack.Arg4());

    TINYCLR_CHECK_HRESULT(GetFileStream( stack, fs ));

    // Argument Validation
    if(offset < 0 || count < 0      ) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
    if(bufferLength - offset < count) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

    if(timeoutHB->NumericByRef().s4 == 0) // Use default timeout
    {
        int timeout = (isRead) ? fs->GetReadTimeout() : fs->GetWriteTimeout();
        timeoutHB->NumericByRef().s4 = (timeout != 0) ? timeout : FS_DEFAULT_TIMEOUT;
    }

    TINYCLR_CHECK_HRESULT(stack.SetupTimeout( *timeoutHB, timeoutTicks ));

    buffer += offset;

    if(fs->GetBufferingStrategy() == SYNC_IO) // I/O is synchronous and does not require buffering or pinning
    {
        while(count > 0)
        {
            int processed;

            if(isRead) { TINYCLR_CHECK_HRESULT(fs->Read ( buffer, count, &processed )); }
            else       { TINYCLR_CHECK_HRESULT(fs->Write( buffer, count, &processed )); }
            
            if(processed == 0) break;

            bytesProcessed += processed;
            buffer         += processed;
            count          -= processed;
        }
    }
    else
    {
        //
        // Push "bytesProcessed" onto the eval stack.
        //
        if(stack.m_customState == 1)
        {
            stack.PushValueI4( 0 );
            
            switch(fs->GetBufferingStrategy())
            {
            case DIRECT_IO:
                fs->AssignStorage( buffer, count, NULL, 0 );
                bufHB->Pin();

                break;
            case SYSTEM_BUFFERED_IO:
                nativeFileStreamHB = stack.This()[ FIELD__m_fs ].Dereference();
                nativeFileStreamHB->Pin();

                break;
            }

            stack.m_customState = 2;
        }

        bytesProcessed = stack.m_evalStack[ 1 ].NumericByRef().s4;

        buffer += bytesProcessed;
        count  -= bytesProcessed;

        fRes = true;

        while(fRes && count > 0)
        {
            int processed;

            if(isRead) { TINYCLR_CHECK_HRESULT(fs->Read ( buffer, count, &processed )); }
            else       { TINYCLR_CHECK_HRESULT(fs->Write( buffer, count, &processed )); }

            if(processed == 0)
            {
                stack.m_evalStack[ 1 ].NumericByRef().s4 = bytesProcessed;

                TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.WaitEvents( stack.m_owningThread, *timeoutTicks, CLR_RT_ExecutionEngine::c_Event_IO, fRes ));
            }
            else if(processed < 0)
            {
                // we've reached the end of the stream
                break;
            }
            else
            {
                buffer         += processed;
                bytesProcessed += processed;
                count          -= processed;
            }
        }

        stack.PopValue();       // bytesProcessed
        stack.PopValue();       // Timeout
    }

    stack.SetResult_I4( bytesProcessed );

    TINYCLR_CLEANUP();

    if(hr != CLR_E_THREAD_WAITING) // we need to clean up if this is not rescheduled
    {
        if(bufHB && bufHB->IsPinned()) bufHB->Unpin();
        else if(nativeFileStreamHB && nativeFileStreamHB->IsPinned()) nativeFileStreamHB->Unpin();
    }

    TINYCLR_CLEANUP_END();
}

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeFileStream::Seek___I8__I8__U4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    CLR_RT_FileStream*     fs;
    INT64                  offset;
    UINT32                 origin;
    INT64                  position = 0;

    CLR_RT_HeapBlock* pArgs = &(stack.Arg1());

    offset = pArgs[ 0 ].NumericByRef().s8;
    origin = pArgs[ 1 ].NumericByRef().u4;

    TINYCLR_CHECK_HRESULT(GetFileStream( stack, fs ));

    // Arugment Validation
    if(origin > SEEKORIGIN_END) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

    TINYCLR_CHECK_HRESULT(fs->Seek( offset, origin, &position ));

    stack.SetResult_I8( position );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeFileStream::Flush___VOID( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    CLR_RT_FileStream* fs;

    TINYCLR_CHECK_HRESULT(GetFileStream( stack, fs ));

    TINYCLR_CHECK_HRESULT(fs->Flush());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeFileStream::GetLength___I8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    CLR_RT_FileStream* fs;
    INT64              length = 0;

    CLR_RT_HeapBlock* pArgs = &(stack.Arg1());

    TINYCLR_CHECK_HRESULT(GetFileStream( stack, fs ));

    TINYCLR_CHECK_HRESULT(fs->GetLength( &length ));

    stack.SetResult_I8( length );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeFileStream::SetLength___VOID__I8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    CLR_RT_FileStream* fs;
    INT64              length;

    CLR_RT_HeapBlock* pArgs = &(stack.Arg1());

    length = pArgs[ 0 ].NumericByRef().s8;

    TINYCLR_CHECK_HRESULT(GetFileStream( stack, fs ));

    // Arugment Validation
    if(length < 0)                            TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    TINYCLR_CHECK_HRESULT(fs->SetLength( length ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeFileStream::GetStreamProperties___VOID__BYREF_BOOLEAN__BYREF_BOOLEAN__BYREF_BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    CLR_RT_FileStream* fs;
    CLR_RT_HeapBlock*  hbCanRead;
    CLR_RT_HeapBlock*  hbCanWrite;
    CLR_RT_HeapBlock*  hbCanSeek;

    CLR_RT_HeapBlock* pArgs = &(stack.Arg1());

    TINYCLR_CHECK_HRESULT(GetFileStream( stack, fs ));

    hbCanRead  = pArgs[ 0 ].Dereference(); FAULT_ON_NULL(hbCanRead);
    hbCanWrite = pArgs[ 1 ].Dereference(); FAULT_ON_NULL(hbCanWrite);
    hbCanSeek  = pArgs[ 2 ].Dereference(); FAULT_ON_NULL(hbCanSeek);

    hbCanRead ->SetBoolean( fs->CanRead () != 0 );
    hbCanWrite->SetBoolean( fs->CanWrite() != 0 );
    hbCanSeek ->SetBoolean( fs->CanSeek () != 0 );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeFileStream::Close___VOID( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    CLR_RT_FileStream* fs;
    CLR_RT_HeapBlock* pThis = stack.This(); FAULT_ON_NULL(pThis);
    
    hr = GetFileStream( stack, fs );

    if(FAILED(hr))
    {
        if(hr == CLR_E_OBJECT_DISPOSED)
        {
            hr = S_OK;
        }

        TINYCLR_LEAVE();
    }

    TINYCLR_CHECK_HRESULT(fs->Close());

    TINYCLR_CLEANUP();

    pThis[ FIELD__m_fs ].SetObjectReference( NULL );

    TINYCLR_CLEANUP_END();
}

//--//

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeFileStream::GetFileStream( CLR_RT_StackFrame& stack, CLR_RT_FileStream*& fs )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_BinaryBlob* blob;
    CLR_RT_HeapBlock* pThis = stack.This(); FAULT_ON_NULL(pThis);

    blob = pThis[ FIELD__m_fs ].DereferenceBinaryBlob(); 
    
    if(!blob || blob->DataType() != DATATYPE_BINARY_BLOB_HEAD)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

    fs = (CLR_RT_FileStream*)blob->GetData();
    
    TINYCLR_NOCLEANUP();
}

