////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Net.h"
#include <TinyCLR_endian.h>
     
HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::socket___STATIC__I4__I4__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    CLR_INT32 family   = stack.Arg0().NumericByRef().s4;
    CLR_INT32 type     = stack.Arg1().NumericByRef().s4;
    CLR_INT32 protocol = stack.Arg2().NumericByRef().s4;
    
    CLR_INT32 nonBlocking = 1;
    CLR_INT32 sock        = SOCK_socket( family, type, protocol );
    
    TINYCLR_CHECK_HRESULT(ThrowOnError( stack, sock ));
    
    TINYCLR_CHECK_HRESULT(ThrowOnError( stack, SOCK_ioctl( sock, SOCK_FIONBIO, &nonBlocking ) ));

    stack.SetResult_I4( sock );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::bind___STATIC__VOID__OBJECT__SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    return BindConnectHelper( stack, true );
}

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::connect___STATIC__VOID__OBJECT__SZARRAY_U1__BOOLEAN( CLR_RT_StackFrame& stack )
{        
    NATIVE_PROFILE_CLR_NETWORK();
    return BindConnectHelper( stack, false );
}

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::send___STATIC__I4__OBJECT__SZARRAY_U1__I4__I4__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    return SendRecvHelper( stack, true, false );
}

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::recv___STATIC__I4__OBJECT__SZARRAY_U1__I4__I4__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    return SendRecvHelper( stack, false, false );
}

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::close___STATIC__I4__OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    CLR_INT32 handle, ret;

    CLR_RT_HeapBlock* socket = stack.Arg0().Dereference(); FAULT_ON_NULL(socket);
    
    handle = socket[ FIELD__m_Handle ].NumericByRef().s4;

    ret = SOCK_close( handle );
    
    //If a socket gets closed, we need to make sure to wake up any threads that are waiting on it.
    Events_Set( SYSTEM_EVENT_FLAG_SOCKET );

    stack.SetResult_I4( ret );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::getaddrinfo___STATIC__VOID__STRING__BYREF_STRING__BYREF_SZARRAY_SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    LPCSTR szName = stack.Arg0().RecoverString();
    struct SOCK_addrinfo hints;
    struct SOCK_addrinfo* addr = NULL;
    struct SOCK_addrinfo* addrT;
    CLR_UINT32        cAddresses = 0;
    CLR_RT_HeapBlock* pAddress;
    CLR_INT32         timeout_ms = 30000;
    CLR_RT_HeapBlock  hbTimeout;
    CLR_INT32         ret;
    bool              fRes = true;
    CLR_INT64*        timeout;

    hbTimeout.SetInteger( timeout_ms );

    TINYCLR_CHECK_HRESULT(stack.SetupTimeout( hbTimeout, timeout ));

    do
    {
        memset( &hints, 0, sizeof(hints) );

        ret = SOCK_getaddrinfo( szName, NULL, &hints, &addr );

        if(ret == SOCK_SOCKET_ERROR)
        {
            if(SOCK_getlasterror() == SOCK_EWOULDBLOCK)
            {
                // non-blocking - allow other threads to run while we wait for handle activity
                TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.WaitEvents( stack.m_owningThread, *timeout, CLR_RT_ExecutionEngine::c_Event_Socket, fRes ));
            }
            else
            {
                break;
            }
        }
        else
        {
            break;
        }
    }
    while(fRes);
    
    // timeout expired
    if(!fRes)
    {
        ret = SOCK_SOCKET_ERROR;
        
        ThrowError( stack, SOCK_ETIMEDOUT );
    
        TINYCLR_SET_AND_LEAVE( CLR_E_PROCESS_EXCEPTION );
    }

    // getaddrinfo returns a winsock error code rather than SOCK_SOCKET_ERROR, so pass this on to the exception handling
    if(ret != 0)
    {
        ThrowError( stack, ret );
        TINYCLR_SET_AND_LEAVE(CLR_E_PROCESS_EXCEPTION);
    }

    {
        CLR_RT_HeapBlock  hbCanonicalName;
        CLR_RT_HeapBlock  hbAddresses;
        
        hbCanonicalName.SetObjectReference( NULL );
        CLR_RT_ProtectFromGC gc( hbCanonicalName );

        hbAddresses.SetObjectReference( NULL );
        CLR_RT_ProtectFromGC gc2( hbAddresses );

        for(int pass = 0; pass < 2; pass++)
        {                                    
            cAddresses = 0;

            for(addrT = addr; addrT != NULL; addrT = addrT->ai_next)
            {
                if(pass == 1)
                {
                    if(addrT->ai_canonname && addrT->ai_canonname[ 0 ])
                    {
                        //allocate return string
                        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_String::CreateInstance( hbCanonicalName, addrT->ai_canonname ));
                        TINYCLR_CHECK_HRESULT(hbCanonicalName.StoreToReference( stack.Arg1(), 0 ));
                    }

                    //allocate address and store into array
                    pAddress = (CLR_RT_HeapBlock*)hbAddresses.DereferenceArray()->GetElement( cAddresses );

                    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( *pAddress, (CLR_UINT32)addrT->ai_addrlen, g_CLR_RT_WellKnownTypes.m_UInt8 ));

                    //copy address.
                    memcpy( pAddress->DereferenceArray()->GetFirstElement(), addrT->ai_addr, addrT->ai_addrlen );
                }
                                
                cAddresses++;
            }
                            
            if(pass == 0)
            {
                //allocate array of byte arrays
                CLR_RT_ReflectionDef_Index idx;

                idx.m_kind               = REFLECTION_TYPE;
                idx.m_levels             = 2;
                idx.m_data.m_type.m_data = g_CLR_RT_WellKnownTypes.m_UInt8.m_data;

                TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( hbAddresses, cAddresses, idx ));

                TINYCLR_CHECK_HRESULT(hbAddresses.StoreToReference( stack.Arg2(), 0 ));                
            }
        }    
    }

    stack.PopValue();       // Timeout
    
    TINYCLR_CLEANUP();

    if( addr ) SOCK_freeaddrinfo( addr );

    TINYCLR_CLEANUP_END();
}

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::listen___STATIC__VOID__OBJECT__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* socket = stack.Arg0().Dereference();
    CLR_INT32 handle;
    CLR_INT32 backlog = stack.Arg1().NumericByRef().s4;
    CLR_INT32 ret;

    FAULT_ON_NULL(socket);
    handle = socket[ FIELD__m_Handle ].NumericByRef().s4;

    ret = SOCK_listen( handle, backlog );

    TINYCLR_CHECK_HRESULT(ThrowOnError( stack, ret ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::accept___STATIC__I4__OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* socket = stack.Arg0().Dereference();
    CLR_INT32 handle;
    CLR_INT32 ret;
    CLR_INT32 nonBlocking = 1;

    FAULT_ON_NULL(socket);
    handle = socket[ FIELD__m_Handle ].NumericByRef().s4;


    /* Because we could have been a rescheduled call due to a prior call that would have blocked, we need to see
         * if our handle has been shutdown before continuing. */
    if (handle == DISPOSED_HANDLE)
    {
        ThrowError(stack, CLR_E_OBJECT_DISPOSED);
        TINYCLR_SET_AND_LEAVE (CLR_E_PROCESS_EXCEPTION);
    }

    ret = SOCK_accept( handle, NULL, NULL );

    if(ret != SOCK_SOCKET_ERROR)
    {
        TINYCLR_CHECK_HRESULT(ThrowOnError( stack, SOCK_ioctl( ret, SOCK_FIONBIO, &nonBlocking ) ));
    }

    TINYCLR_CHECK_HRESULT(ThrowOnError( stack, ret ));

    stack.SetResult_I4( ret );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::shutdown___STATIC__VOID__OBJECT__I4__BYREF_I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* socket = stack.Arg0().Dereference();
    CLR_INT32 handle;
    CLR_INT32 how = stack.Arg1().NumericByRef().s4;
    CLR_INT32 ret;
    
    FAULT_ON_NULL(socket);
    handle = socket[ FIELD__m_Handle ].NumericByRef().s4;

    ret = SOCK_shutdown( handle, how );

    TINYCLR_CHECK_HRESULT(ThrowOnError( stack, ret ));

    TINYCLR_NOCLEANUP();
}

CLR_INT32 Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::Helper__SelectSocket(CLR_INT32 handle, CLR_INT32 mode )
{
    struct SOCK_timeval timeval;
    SOCK_fd_set* readfds   = NULL;
    SOCK_fd_set* writefds  = NULL;
    SOCK_fd_set* exceptfds = NULL;
    SOCK_fd_set fds;
    SOCK_fd_set fdsExcept;
    CLR_INT32   res = 0;

    switch(mode)
    {
        case 0:
            readfds = &fds;
            break;
        case 1:
            writefds = &fds;
            break;
        default:
            _ASSERTE(FALSE);
            // fall through
        case 2:
            exceptfds = &fds;
            break;
    }
    
    fds.fd_count      = 1;
    fds.fd_array[ 0 ] = handle;

    // This Poll method is a little handicapped in the sense that it only allows the caller to wait
    // for a read, a write or an except. This causes a problem when there is a socket exception.  The
    // poll will continue to block forever because the select statement wasn't looking for exceptions
    // Therefore, we will force the select call to look for the except case if it is not already doing it.
    if(exceptfds == NULL)
    {
        fdsExcept.fd_count      = 1;
        fdsExcept.fd_array[ 0 ] = handle;
        exceptfds               = &fdsExcept;
    }


    timeval.tv_sec  = 0;
    timeval.tv_usec = 0;

    res = SOCK_select( 1, readfds, writefds, exceptfds, &timeval );

    // socket is in the exception state (only return error if caller was NOT looking for the excepted state)
    if((mode != 2) && (fdsExcept.fd_count != 0))
    {
        // For read mode ignore exception if we have data to read
        if(!(mode == 0 && fds.fd_count != 0))
        {
            return SOCK_SOCKET_ERROR;
        }
    }

    return res;
    
}

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::poll___STATIC__BOOLEAN__OBJECT__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* socket = stack.Arg0().Dereference();
    CLR_INT32 handle;
    CLR_INT32 mode       = stack.Arg1().NumericByRef().s4;    
    CLR_INT32 timeout_us = stack.Arg2().NumericByRef().s4;
    
    CLR_RT_HeapBlock hbTimeout;
    CLR_INT32 timeout_ms;

    CLR_INT32 res = 0;
    bool fRes     = true;

    CLR_INT64* timeout;

    FAULT_ON_NULL(socket);
    handle = socket[ FIELD__m_Handle ].NumericByRef().s4;

    /* Because we could have been a rescheduled call due to a prior call that would have blocked, we need to see
     * if our handle has been shutdown before continuing. */
    if (handle == DISPOSED_HANDLE)
    {
        ThrowError( stack, CLR_E_OBJECT_DISPOSED );
        TINYCLR_SET_AND_LEAVE (CLR_E_PROCESS_EXCEPTION);
    }


    if(timeout_us < 0) timeout_ms = -1;
    else               timeout_ms = timeout_us / 1000;

    hbTimeout.SetInteger( timeout_ms );

    TINYCLR_CHECK_HRESULT(stack.SetupTimeout( hbTimeout, timeout ));

    while(fRes)
    {
        res = Helper__SelectSocket( handle, mode );

        if(res != 0) break;

        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.WaitEvents( stack.m_owningThread, *timeout, CLR_RT_ExecutionEngine::c_Event_Socket, fRes ));
    }

    stack.PopValue(); //timer

    TINYCLR_CHECK_HRESULT(ThrowOnError( stack, res ));

    stack.SetResult_Boolean( res != 0 );   

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::sendto___STATIC__I4__OBJECT__SZARRAY_U1__I4__I4__I4__I4__SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    return SendRecvHelper( stack, true, true );
}

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::recvfrom___STATIC__I4__OBJECT__SZARRAY_U1__I4__I4__I4__I4__BYREF_SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    return SendRecvHelper( stack, false, true );
}

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::getpeername___STATIC__VOID__OBJECT__BYREF_SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    return SockNameHelper( stack, true );
}

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::getsockname___STATIC__VOID__OBJECT__BYREF_SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    return SockNameHelper( stack, false );
}

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::getsockopt___STATIC__VOID__OBJECT__I4__I4__SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    return SockOptHelper( stack, true );
}

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::setsockopt___STATIC__VOID__OBJECT__I4__I4__SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    return SockOptHelper( stack, false );
}

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::ioctl___STATIC__VOID__OBJECT__U4__BYREF_U4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* socket = stack.Arg0().Dereference();
    CLR_INT32 handle;
    CLR_INT32 cmd     = stack.Arg1().NumericByRef().s4;
    CLR_RT_HeapBlock blkArg;
    CLR_INT32 ret;

    FAULT_ON_NULL(socket);
    handle = socket[ FIELD__m_Handle ].NumericByRef().s4;

    _SIDE_ASSERTE(SUCCEEDED(blkArg.LoadFromReference( stack.Arg2() )));

    ret = SOCK_ioctl( handle, cmd, (CLR_INT32*)&blkArg.NumericByRef().s4 );
    
    TINYCLR_CHECK_HRESULT(ThrowOnError( stack, ret ));

    _SIDE_ASSERTE(SUCCEEDED(blkArg.StoreToReference( stack.Arg2(), 0 )));

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::BindConnectHelper( CLR_RT_StackFrame& stack, bool fBind )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* socket = stack.Arg0().Dereference();
    CLR_INT32 handle;
    SOCK_sockaddr addr;
    CLR_UINT32 addrLen = sizeof(addr);
    CLR_INT32 ret;
    bool fThrowOnWouldBlock = false;

    FAULT_ON_NULL(socket);
    handle = socket[ FIELD__m_Handle ].NumericByRef().s4;

    TINYCLR_CHECK_HRESULT(MarshalSockAddress( &addr, addrLen, stack.Arg1() ));

    if(fBind)
    {
        ret = SOCK_bind( handle, &addr, addrLen );
    }
    else
    {
        ret = SOCK_connect( handle, &addr, addrLen );
        
        fThrowOnWouldBlock = (stack.Arg2().NumericByRefConst().s4 != 0);
        
        if(!fThrowOnWouldBlock && SOCK_getlasterror() == SOCK_EWOULDBLOCK)
        {
            TINYCLR_SET_AND_LEAVE(S_OK);            
        }
    }

    TINYCLR_CHECK_HRESULT(ThrowOnError( stack, ret ));

    TINYCLR_NOCLEANUP();
}


HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::MarshalSockAddress( CLR_RT_HeapBlock& blkDst, const struct SOCK_sockaddr* addrSrc, CLR_UINT32 addrLenSrc )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* arr = NULL;

    CLR_RT_HeapBlock blkArr; blkArr.SetObjectReference( NULL );
    CLR_RT_ProtectFromGC gc( blkArr );
    SOCK_sockaddr_in* dst;
    SOCK_sockaddr_in* src = (SOCK_sockaddr_in*)addrSrc;
        
    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( blkArr, addrLenSrc, g_CLR_RT_WellKnownTypes.m_UInt8 ));
    
    arr = blkArr.DereferenceArray();

    _ASSERTE(arr);

    dst = (SOCK_sockaddr_in*)arr->GetFirstElement();

    dst->sin_family           = SwapEndianIfBEc16(src->sin_family);
    dst->sin_port             = src->sin_port;
    dst->sin_addr.S_un.S_addr = src->sin_addr.S_un.S_addr;

    memcpy(dst->sin_zero, src->sin_zero, sizeof(dst->sin_zero));

    _ASSERTE(blkDst.DataType() == DATATYPE_BYREF || blkDst.DataType() == DATATYPE_ARRAY_BYREF);

    TINYCLR_CHECK_HRESULT(blkArr.StoreToReference( blkDst, 0 ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::MarshalSockAddress( struct SOCK_sockaddr* addrDst, CLR_UINT32& addrLen, const CLR_RT_HeapBlock& blkSockAddress )
{        
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* ptrSockAddress;
    SOCK_sockaddr_in* dst = (SOCK_sockaddr_in*)addrDst;
    SOCK_sockaddr_in* src;    

    ptrSockAddress = blkSockAddress.DereferenceArray();                    
    FAULT_ON_NULL(ptrSockAddress);

    if(ptrSockAddress->m_numOfElements > addrLen) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

    src = (SOCK_sockaddr_in*)ptrSockAddress->GetFirstElement();

    dst->sin_family           = SwapEndianIfBEc16(src->sin_family);
    dst->sin_port             = src->sin_port;
    dst->sin_addr.S_un.S_addr = src->sin_addr.S_un.S_addr; //already in network byte order

    memcpy(dst->sin_zero, src->sin_zero, sizeof(dst->sin_zero));

    addrLen = ptrSockAddress->m_numOfElements;

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::SendRecvHelper( CLR_RT_StackFrame& stack, bool fSend, bool fAddress )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       socket    = stack.Arg0().Dereference();
    CLR_INT32               handle;
    CLR_RT_HeapBlock_Array* arrData   = stack.Arg1().DereferenceArray(); 
    CLR_UINT32              offset    = stack.Arg2().NumericByRef().u4;
    CLR_UINT32              count     = stack.Arg3().NumericByRef().u4;
    CLR_INT32               flags     = stack.Arg4().NumericByRef().s4;
    CLR_INT32               timeout_ms = stack.Arg5().NumericByRef().s4;
    CLR_RT_HeapBlock        hbTimeout;

    CLR_INT64* timeout;
    CLR_UINT8* buf;
    bool       fRes = true;
    CLR_INT32  totReadWrite;
    CLR_INT32  ret = 0;

    FAULT_ON_NULL(socket);
    FAULT_ON_NULL(arrData);
    
    handle = socket[ FIELD__m_Handle ].NumericByRef().s4;

    /* Because we could have been a rescheduled call due to a prior call that would have blocked, we need to see
     * if our handle has been shutdown before continuing. */
    if (handle == DISPOSED_HANDLE)
    {
        ThrowError( stack, CLR_E_OBJECT_DISPOSED );
        TINYCLR_SET_AND_LEAVE (CLR_E_PROCESS_EXCEPTION);
    }

    if(offset + count > arrData->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_INDEX_OUT_OF_RANGE);    


    hbTimeout.SetInteger( timeout_ms );
        
    TINYCLR_CHECK_HRESULT(stack.SetupTimeout( hbTimeout, timeout ));

    //
    // Push "totReadWrite" onto the eval stack.
    //
    if(stack.m_customState == 1)
    {
        stack.PushValueI4( 0 );
        
        stack.m_customState = 2;
    }

    totReadWrite = stack.m_evalStack[ 1 ].NumericByRef().s4;

    buf    = arrData->GetElement( offset + totReadWrite );
    count -= totReadWrite;

    while(count > 0)
    {
        CLR_INT32 bytes = 0;

        // first make sure we have data to read or ability to write
        while(fRes)
        {
            ret = Helper__SelectSocket( handle, fSend ? 1 : 0 );

            if(ret != 0) break;

            // non-blocking - allow other threads to run while we wait for handle activity
            TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.WaitEvents( stack.m_owningThread, *timeout, CLR_RT_ExecutionEngine::c_Event_Socket, fRes ));
        }

        // timeout expired
        if(!fRes)
        {
            ret = SOCK_SOCKET_ERROR;
            
            ThrowError( stack, SOCK_ETIMEDOUT );

            TINYCLR_SET_AND_LEAVE( CLR_E_PROCESS_EXCEPTION );
        }

        // socket is in the excepted state, so let's bail out
        if(SOCK_SOCKET_ERROR == ret)
        {
            break;
        }

        if(fAddress)
        {
            struct SOCK_sockaddr addr;
            CLR_UINT32 addrLen = sizeof(addr);
            CLR_RT_HeapBlock& blkAddr = stack.Arg6();

            if(fSend)
            {
                TINYCLR_CHECK_HRESULT(MarshalSockAddress( &addr, addrLen, blkAddr ));

                bytes = SOCK_sendto( handle, (const char*)buf, count, flags, &addr, addrLen );
            }
            else
            {
                CLR_RT_HeapBlock* pBlkAddr = blkAddr.Dereference();
                
                TINYCLR_CHECK_HRESULT(MarshalSockAddress( &addr, addrLen, *pBlkAddr ));

                bytes = SOCK_recvfrom( handle, (char*)buf, count, flags, &addr, (int*)&addrLen );

                if(bytes != SOCK_SOCKET_ERROR)
                {
                    TINYCLR_CHECK_HRESULT(MarshalSockAddress( blkAddr, &addr, addrLen ));
                }
            }
        }
        else
        {
            if(fSend)
            {
                bytes = SOCK_send( handle, (const char*)buf, count, flags );
            }
            else
            {
                bytes = SOCK_recv( handle, (char*)buf, count, flags );
            }
        }

        // send/recv/sendto/recvfrom failed
        if(bytes == SOCK_SOCKET_ERROR)
        {
            CLR_INT32 err = SOCK_getlasterror();
            
            if(err != SOCK_EWOULDBLOCK)
            {
                ret = SOCK_SOCKET_ERROR;
                break;
            }
            
            continue;
        }
                // zero recv bytes indicates the handle has been closed.
        else if(!fSend && (bytes == 0)) 
        {
            break;
        }
        
        buf          += bytes;
        totReadWrite += bytes;
        count        -= bytes;

        stack.m_evalStack[ 1 ].NumericByRef().s4 = totReadWrite;        

        // receive returns immediately after receiving bytes.
        if(!fSend && (totReadWrite > 0))
        {
            break;
        }

    }

    stack.PopValue();       // totReadWrite
    stack.PopValue();       // Timeout
    
    TINYCLR_CHECK_HRESULT(ThrowOnError( stack, ret ));

    stack.SetResult_I4( totReadWrite );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::SockOptHelper( CLR_RT_StackFrame& stack, bool fGet )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* socket = stack.Arg0().Dereference();
    CLR_INT32 handle;
    CLR_INT32 level   = stack.Arg1().NumericByRef().s4;
    CLR_INT32 optname = stack.Arg2().NumericByRef().s4;
    CLR_RT_HeapBlock_Array* arrOpt = stack.Arg3().DereferenceArray(); 
    char* optval;
    CLR_INT32 optlen;
    CLR_INT32 ret;

    FAULT_ON_NULL(socket);
    handle = socket[ FIELD__m_Handle ].NumericByRef().s4;

    FAULT_ON_NULL(arrOpt);
    
    optval = (char*)arrOpt->GetFirstElement();
    optlen = arrOpt->m_numOfElements;

    if(fGet)
    {
        ret = SOCK_getsockopt( handle, level, optname, optval, &optlen );
        _ASSERTE( optlen <= (CLR_INT32)arrOpt->m_numOfElements ); 
    }
    else
    {
        ret = SOCK_setsockopt( handle, level, optname, optval, optlen );
    }

    TINYCLR_CHECK_HRESULT(ThrowOnError( stack, ret ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::SockNameHelper( CLR_RT_StackFrame& stack, bool fPeer )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* socket = stack.Arg0().Dereference();
    CLR_INT32 handle;
    CLR_INT32 ret;
    
    struct SOCK_sockaddr addr;
    CLR_INT32 addrLen = sizeof(addr);

    FAULT_ON_NULL(socket);
    handle = socket[ FIELD__m_Handle ].NumericByRef().s4;

    if(fPeer)
    {
        ret = SOCK_getpeername( handle, &addr, &addrLen );
    }
    else
    {
        ret = SOCK_getsockname( handle, &addr, &addrLen );
    }
    
    TINYCLR_CHECK_HRESULT(ThrowOnError( stack, ret ));

    TINYCLR_CHECK_HRESULT(MarshalSockAddress( stack.Arg1(), &addr, addrLen ));
    
    TINYCLR_NOCLEANUP();
}

void Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::ThrowError( CLR_RT_StackFrame& stack, CLR_INT32 errorCode )
{        
    NATIVE_PROFILE_CLR_NETWORK();
    CLR_RT_HeapBlock& res = stack.m_owningThread->m_currentException;
                            
    if((Library_corlib_native_System_Exception::CreateInstance( res, g_CLR_RT_WellKnownTypes.m_SocketException, CLR_E_FAIL, &stack )) == S_OK)
    {
        res.Dereference()[ Library_system_sockets_System_Net_Sockets_SocketException::FIELD___errorCode ].SetInteger( errorCode );
    }
}

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::ThrowOnError( CLR_RT_StackFrame& stack, CLR_INT32 res )
{        
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    if(res == SOCK_SOCKET_ERROR)
    {                
        CLR_INT32 err = SOCK_getlasterror();

        ThrowError( stack, err );

        TINYCLR_SET_AND_LEAVE( CLR_E_PROCESS_EXCEPTION );
    }

    TINYCLR_NOCLEANUP();
}
