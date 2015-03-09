////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Net_Security.h"
#include <Crypto.h>


extern "C"
{
    void ssl_rand_seed(const void *seed, int length);
}

struct SSL_SeedConfig
{
    HAL_DRIVER_CONFIG_HEADER Header;

    //--//

    UINT64 SeedCounter;
    UINT8  SslSeedKey[ 260 ];

    //--//
    static LPCSTR GetDriverName() { return "SSL_SEED_KEY"; }
};

struct SSL_SeedDriver
{
    SSL_SeedConfig  Config;
    
    BOOL            Initialized;
    HAL_COMPLETION  m_completion;
};

//--//

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_SSL_SeedData"
#endif

static SSL_SeedDriver g_SSL_SeedData;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

//--//

static void UpdateSslSeedValue(void* arg)
{
    if(!HAL_CONFIG_BLOCK::UpdateBlockWithName( g_SSL_SeedData.Config.GetDriverName(), &g_SSL_SeedData.Config, sizeof(g_SSL_SeedData.Config), TRUE ))
    {
        ASSERT(FALSE);
        CPU_Reset();
    }
}

static void GenerateNewSslSeed()
{
    UINT8   signature[ 128 ];
    UINT8   IVPtr[ BLOCK_SIZE ];
    BOOL    success;

    UINT64  data[ 2 ]  = { ++g_SSL_SeedData.Config.SeedCounter, HAL_Time_CurrentTicks() };

    memset( &IVPtr[ 0 ], 0, sizeof(IVPtr) );

    success = Crypto_Encrypt( (BYTE*)&g_SSL_SeedData.Config.SslSeedKey[ 0 ], (UINT8*)IVPtr, sizeof(IVPtr), (BYTE*)&data, sizeof(data), signature, sizeof(signature) ) == CRYPTO_SUCCESS ? S_OK : CLR_E_FAIL;

    ASSERT(success);

    ssl_rand_seed(signature, sizeof(signature));

    if(!g_SSL_SeedData.m_completion.IsLinked())
    {
        g_SSL_SeedData.m_completion.EnqueueDelta( 5 * 1000000ul ); // 5 seconds
    }
}

//--//

void Time_GetDateTime(DATE_TIME_INFO* dt)
{
    NATIVE_PROFILE_CLR_NETWORK();
    SYSTEMTIME st;

    CLR_INT64 lt = Time_GetLocalTime();    

    Time_ToSystemTime( lt, &st );

    dt->tzOffset = Time_GetTimeZoneOffset() * 60;

    dt->year   = st.wYear;
    dt->month  = st.wMonth;
    dt->day    = st.wDay;
    dt->hour   = st.wHour;
    dt->minute = st.wMinute;
    dt->second = st.wSecond;
    dt->msec   = st.wMilliseconds;

    dt->dlsTime  = 0;
 }


HRESULT Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::SecureServerInit___STATIC__I4__I4__I4__MicrosoftSPOTNativeSystemSecurityCryptographyX509CertificatesX509Certificate__SZARRAY_MicrosoftSPOTNativeSystemSecurityCryptographyX509CertificatesX509Certificate( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    return InitHelper( stack, true );
}

HRESULT Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::SecureClientInit___STATIC__I4__I4__I4__MicrosoftSPOTNativeSystemSecurityCryptographyX509CertificatesX509Certificate__SZARRAY_MicrosoftSPOTNativeSystemSecurityCryptographyX509CertificatesX509Certificate( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    return InitHelper( stack, false );
}

HRESULT Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::UpdateCertificates___STATIC__VOID__I4__MicrosoftSPOTNativeSystemSecurityCryptographyX509CertificatesX509Certificate__SZARRAY_MicrosoftSPOTNativeSystemSecurityCryptographyX509CertificatesX509Certificate( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    CLR_INT32               sslContext = stack.Arg0().NumericByRef().s4;
    CLR_RT_HeapBlock*       hbCert     = stack.Arg1().Dereference(); 
    CLR_RT_HeapBlock_Array* arrCA      = stack.Arg2().DereferenceArray(); 
    CLR_RT_HeapBlock_Array* arrCert;
    CLR_UINT8* sslCert;
    int        i;
    CLR_RT_HeapBlock*       hbPwd;
    LPCSTR  szPwd;

    FAULT_ON_NULL(hbCert);
    FAULT_ON_NULL(arrCA);

    arrCert    = hbCert[ Library_spot_native_System_Security_Cryptography_X509Certificates_X509Certificate::FIELD__m_certificate ].DereferenceArray(); FAULT_ON_NULL(arrCert);

    sslCert    = arrCert->GetFirstElement();

    hbPwd      = hbCert[ Library_spot_native_System_Security_Cryptography_X509Certificates_X509Certificate::FIELD__m_password ].Dereference(); FAULT_ON_NULL(hbPwd);

    szPwd      = hbPwd->StringText();
    
    SSL_ClearCertificateAuthority( sslContext );

    if(!SSL_AddCertificateAuthority( sslContext, (const char*)sslCert, arrCert->m_numOfElements, szPwd )) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

    for(i=0; i<(int)arrCA->m_numOfElements; i++)
    {
        hbCert = (CLR_RT_HeapBlock*)arrCA->GetElement( i ); FAULT_ON_NULL(arrCert);

        arrCert = hbCert[ Library_spot_native_System_Security_Cryptography_X509Certificates_X509Certificate::FIELD__m_certificate ].DereferenceArray();

        sslCert = arrCert->GetFirstElement();

        hbPwd      = hbCert[ Library_spot_native_System_Security_Cryptography_X509Certificates_X509Certificate::FIELD__m_password ].Dereference();
        
        szPwd      = hbPwd->StringText();
        
        if(!SSL_AddCertificateAuthority( sslContext, (const char*)sslCert, arrCert->m_numOfElements, szPwd )) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::SecureAccept___STATIC__VOID__I4__OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    CLR_INT32         sslContext = stack.Arg0().NumericByRef().s4;
    CLR_RT_HeapBlock* socket     = stack.Arg1().Dereference();
    CLR_INT32         timeout_ms = -1; // wait forever
    CLR_RT_HeapBlock  hbTimeout;

    int        result = 0;    
    CLR_INT32  handle;
    bool       fRes = true;
    CLR_INT64 *timeout;

    FAULT_ON_NULL(socket);

    handle = socket[ Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::FIELD__m_Handle ].NumericByRef().s4;

    /* Because we could have been a rescheduled call due to a prior call that would have blocked, we need to see
     * if our handle has been shutdown before continuing. */
    if (handle == Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::DISPOSED_HANDLE)
    {
        ThrowError(stack, CLR_E_OBJECT_DISPOSED);
        TINYCLR_SET_AND_LEAVE(CLR_E_PROCESS_EXCEPTION);
    }


    hbTimeout.SetInteger( timeout_ms );
        
    TINYCLR_CHECK_HRESULT(stack.SetupTimeout( hbTimeout, timeout ));


    // first make sure we have data to read or ability to write
    while(true)
    {
        result = SSL_Accept( handle, sslContext );

        if(result == SOCK_EWOULDBLOCK || result == SOCK_TRY_AGAIN)
        {
            // non-blocking - allow other threads to run while we wait for socket activity
            TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.WaitEvents( stack.m_owningThread, *timeout, CLR_RT_ExecutionEngine::c_Event_Socket, fRes ));
        }
        else
        {
            break;
        }
    }

    stack.PopValue();       // Timeout

    TINYCLR_CHECK_HRESULT(ThrowOnError( stack, result ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::SecureConnect___STATIC__VOID__I4__STRING__OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    CLR_INT32 sslContext     = stack.Arg0().NumericByRef().s4;
    CLR_RT_HeapBlock* hb     = stack.Arg1().DereferenceString();
    CLR_RT_HeapBlock* socket = stack.Arg2().Dereference();
    CLR_INT32         timeout_ms = -1; // wait forever
    CLR_RT_HeapBlock  hbTimeout;
    
    int        result;    
    LPCSTR     szName;
    CLR_INT32  handle;
    bool       fRes = true;
    CLR_INT64 *timeout;

    FAULT_ON_NULL(socket);

    handle = socket[ Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::FIELD__m_Handle ].NumericByRef().s4;

    /* Because we could have been a rescheduled call due to a prior call that would have blocked, we need to see
     * if our handle has been shutdown before continuing. */
    if (handle == Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::DISPOSED_HANDLE)
    {
        ThrowError( stack, CLR_E_OBJECT_DISPOSED );
        TINYCLR_SET_AND_LEAVE(CLR_E_PROCESS_EXCEPTION);
    }

    FAULT_ON_NULL_ARG(hb);

    szName = hb->StringText();

    hbTimeout.SetInteger( timeout_ms );
        
    TINYCLR_CHECK_HRESULT(stack.SetupTimeout( hbTimeout, timeout ));

    while(true)
    {
        result = SSL_Connect( handle, szName, sslContext );

        if(result == SOCK_EWOULDBLOCK || result == SOCK_TRY_AGAIN)
        {
            // non-blocking - allow other threads to run while we wait for socket activity
            TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.WaitEvents( stack.m_owningThread, *timeout, CLR_RT_ExecutionEngine::c_Event_Socket, fRes ));

            if(result < 0) break;
        }
        else
        {
            break;
        }
    }

    stack.PopValue();       // Timeout

    TINYCLR_CHECK_HRESULT(ThrowOnError( stack, result ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::SecureRead___STATIC__I4__OBJECT__SZARRAY_U1__I4__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    return ReadWriteHelper( stack, false );
}

HRESULT Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::SecureWrite___STATIC__I4__OBJECT__SZARRAY_U1__I4__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    return ReadWriteHelper( stack, true );
}

HRESULT Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::SecureCloseSocket___STATIC__I4__OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    int result;
    CLR_INT32 handle;

    CLR_RT_HeapBlock* socket = stack.Arg0().Dereference(); FAULT_ON_NULL(socket);

    handle = socket[ Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::FIELD__m_Handle ].NumericByRef().s4;

    result = SSL_CloseSocket( handle );

    stack.SetResult_I4( result );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::ExitSecureContext___STATIC__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    CLR_INT32 sslContext = stack.Arg0().NumericByRef().s4;

    int result = SSL_ExitContext( sslContext ) == TRUE ? 0 : -1;

    stack.SetResult_I4( result );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::DataAvailable___STATIC__I4__OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* socket = stack.Arg0().Dereference();
    int       result;    
    CLR_INT32 handle;

    FAULT_ON_NULL(socket);

    handle = socket[ Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::FIELD__m_Handle ].NumericByRef().s4;

    result = SSL_DataAvailable( handle );

    // ThrowOnError expects anything other than 0 to be a failure - so return 0 if we don't have an error
    TINYCLR_CHECK_HRESULT(ThrowOnError( stack, result >= 0 ? 0 : result ));

    stack.SetResult_I4( result );

    TINYCLR_NOCLEANUP();    
}

//--//

HRESULT Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::ReadWriteHelper( CLR_RT_StackFrame& stack, bool isWrite )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       socket     = stack.Arg0().Dereference();
    CLR_RT_HeapBlock_Array* arrData    = stack.Arg1().DereferenceArray(); 
    CLR_INT32               offset     = stack.Arg2().NumericByRef().s4;
    CLR_INT32               count      = stack.Arg3().NumericByRef().s4;
    CLR_INT32               timeout_ms = stack.Arg4().NumericByRef().s4;
    CLR_UINT8*              buffer;
    CLR_RT_HeapBlock        hbTimeout;

    CLR_INT32  totReadWrite;
    bool       fRes = true;
    CLR_INT64 *timeout;
    int        result = 0;
    CLR_INT32 handle;

    if(count == 0) 
    {
        stack.SetResult_I4( 0 );
        TINYCLR_SET_AND_LEAVE(S_OK);
    }

    FAULT_ON_NULL(socket);

    handle = socket[ Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::FIELD__m_Handle ].NumericByRef().s4;

    /* Because we could have been a rescheduled call due to a prior call that would have blocked, we need to see
     * if our handle has been shutdown before continuing. */
    if (handle == Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::DISPOSED_HANDLE)
    {
        ThrowError( stack, CLR_E_OBJECT_DISPOSED );
        TINYCLR_SET_AND_LEAVE(CLR_E_PROCESS_EXCEPTION);
    }


    FAULT_ON_NULL(arrData);

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

    buffer = arrData->GetElement( offset + totReadWrite );
    count -= totReadWrite;

    if((offset + count + totReadWrite) > (int)arrData->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_INDEX_OUT_OF_RANGE);    

    while(count > 0)
    {
        // first make sure we have data to read or ability to write
        while(fRes)
        {
            if(!isWrite)
            {
                // check SSL_DataAvailable() in case SSL has already read and buffered socket data
                result = SSL_DataAvailable(handle);

                if((result > 0) || ((result < 0) && (SOCK_getlasterror() != SOCK_EWOULDBLOCK)))
                {
                    break;
                }
            }

            result = Library_spot_net_native_Microsoft_SPOT_Net_SocketNative::Helper__SelectSocket( handle, isWrite ? 1 : 0 );

            if((result > 0) || ((result < 0) && (SOCK_getlasterror() != SOCK_EWOULDBLOCK)))
            {
                break;
            }

            // non-blocking - allow other threads to run while we wait for socket activity
            TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.WaitEvents( stack.m_owningThread, *timeout, CLR_RT_ExecutionEngine::c_Event_Socket, fRes ));

            // timeout expired 
            if(!fRes)
            {
                result = SOCK_SOCKET_ERROR;
                
                ThrowError(stack, SOCK_ETIMEDOUT);
            
                TINYCLR_SET_AND_LEAVE( CLR_E_PROCESS_EXCEPTION );
            }
        }

        // socket is in the excepted state, so let's bail out
        if(SOCK_SOCKET_ERROR == result)
        {
            break;
        }

        if(isWrite)
        {
            result = SSL_Write( handle, (const char*)buffer, count );
        }
        else
        {
            result = SSL_Read( handle, (char*)buffer, count );

            if(result == SSL_RESULT__WOULD_BLOCK)
            {
                continue;
            }
        }

        // ThrowOnError expects anything other than 0 to be a failure - so return 0 if we don't have an error
        if(result <= 0)
        {
            break;
        }

        buffer       += result;
        totReadWrite += result;
        count        -= result;


        // read is non-blocking if we have any data
        if(!isWrite && (totReadWrite > 0))
        {
            break;
        }

        stack.m_evalStack[ 1 ].NumericByRef().s4 = totReadWrite;        
    }

    stack.PopValue();       // totReadWrite
    stack.PopValue();       // Timeout

    if(result < 0)
    {
        TINYCLR_CHECK_HRESULT(ThrowOnError( stack, result ));
    }

    stack.SetResult_I4( totReadWrite );
    
    TINYCLR_NOCLEANUP();
    
}

HRESULT Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::InitHelper( CLR_RT_StackFrame& stack, bool isServer )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    CLR_INT32 sslContext            = -1;
    CLR_INT32 sslMode               = stack.Arg0().NumericByRef().s4;
    CLR_INT32 sslVerify             = stack.Arg1().NumericByRef().s4;
    CLR_RT_HeapBlock *hbCert        = stack.Arg2().Dereference(); 
    CLR_RT_HeapBlock_Array* arrCA   = stack.Arg3().DereferenceArray(); 
    CLR_RT_HeapBlock_Array* arrCert = NULL;
    CLR_UINT8*  sslCert             = NULL;
    int         result;
    int         i;
    bool        isFirstCall = false;
    LPCSTR      szPwd = "";

    if(!g_SSL_SeedData.Initialized)
    {
        BOOL fOK = FALSE;

        isFirstCall = true;

#if !defined(_WIN32) && !defined(WIN32) && !defined(_WIN32_WCE)
        int i;

        if(!HAL_CONFIG_BLOCK::ApplyConfig( g_SSL_SeedData.Config.GetDriverName(), &g_SSL_SeedData.Config, sizeof(g_SSL_SeedData.Config) ))
        {
            return CLR_E_NOT_SUPPORTED;
        }

        // validate the security key (make sure it isn't all 0x00 or all 0xFF
        for(i=1; i<sizeof(g_SSL_SeedData.Config.SslSeedKey) && !fOK; i++)
        {
            if( g_SSL_SeedData.Config.SslSeedKey[ i   ] != 0 && 
                g_SSL_SeedData.Config.SslSeedKey[ i   ] != 0xFF && 
                g_SSL_SeedData.Config.SslSeedKey[ i-1 ] != g_SSL_SeedData.Config.SslSeedKey[ i ])
            {
                fOK = TRUE;
            }
        }

        if(!fOK)
        {
            return CLR_E_NOT_SUPPORTED;
        }
#endif

        g_SSL_SeedData.m_completion.Initialize();
        
        g_SSL_SeedData.m_completion.InitializeForUserMode( UpdateSslSeedValue, NULL ); 

        g_SSL_SeedData.Initialized = TRUE;
    }


    if(hbCert != NULL)
    {
        arrCert = hbCert[ Library_spot_native_System_Security_Cryptography_X509Certificates_X509Certificate::FIELD__m_certificate ].DereferenceArray(); //FAULT_ON_NULL(arrCert);

        // If arrCert == NULL then the certificate is an X509Certificate2 which uses a certificate handle
        if(arrCert == NULL)
        {
            arrCert = hbCert[ Library_spot_native_System_Security_Cryptography_X509Certificates_X509Certificate::FIELD__m_handle ].DereferenceArray(); FAULT_ON_NULL(arrCert);    

            // pass the certificate handle as the cert data parameter
            sslCert = arrCert->GetFirstElement();

            arrCert = hbCert[ Library_spot_native_System_Security_Cryptography_X509Certificates_X509Certificate::FIELD__m_sessionHandle ].DereferenceArray(); FAULT_ON_NULL(arrCert);    

            // pass the session handle as the ssl context parameter
            sslContext = *(INT32*)arrCert->GetFirstElement();

            // the certificate has already been loaded so just pass an empty string
            szPwd = "";
        }
        else
        {
            arrCert->Pin();
        
            sslCert = arrCert->GetFirstElement();

            CLR_RT_HeapBlock *hbPwd = hbCert[ Library_spot_native_System_Security_Cryptography_X509Certificates_X509Certificate::FIELD__m_password ].Dereference();// FAULT_ON_NULL(hbPwd);

            szPwd = hbPwd->StringText();
        }
    }

    SSL_RegisterTimeCallback( Time_GetDateTime );

    if(isServer)
    {
        result = (SSL_ServerInit( sslMode, sslVerify, (const char*)sslCert, sslCert == NULL ? 0 : arrCert->m_numOfElements, szPwd, sslContext ) ? 0 : -1);
    }
    else
    {
        result = (SSL_ClientInit( sslMode, sslVerify, (const char*)sslCert, sslCert == NULL ? 0 : arrCert->m_numOfElements, szPwd, sslContext ) ? 0 : -1);
    }

    TINYCLR_CHECK_HRESULT(ThrowOnError( stack, result ));

    if(isFirstCall)
    {
        GenerateNewSslSeed();
    }

    if(arrCA != NULL)
    {
        for(i=0; i<(int)arrCA->m_numOfElements; i++)
        {
            hbCert = (CLR_RT_HeapBlock*)arrCA->GetElement( i ); FAULT_ON_NULL(hbCert);
            hbCert = hbCert->Dereference();                     FAULT_ON_NULL(hbCert);

            arrCert = hbCert[ Library_spot_native_System_Security_Cryptography_X509Certificates_X509Certificate::FIELD__m_certificate ].DereferenceArray(); //FAULT_ON_NULL(arrCert);

            // If arrCert == NULL then the certificate is an X509Certificate2 which uses a certificate handle
            if(arrCert == NULL)
            {
                CLR_INT32 sessionCtx = 0;

                arrCert = hbCert[ Library_spot_native_System_Security_Cryptography_X509Certificates_X509Certificate::FIELD__m_handle ].DereferenceArray(); FAULT_ON_NULL(arrCert);    

                sslCert = arrCert->GetFirstElement();

                arrCert = hbCert[ Library_spot_native_System_Security_Cryptography_X509Certificates_X509Certificate::FIELD__m_sessionHandle ].DereferenceArray(); FAULT_ON_NULL(arrCert);    

                sessionCtx = *(INT32*)arrCert->GetFirstElement();

                // pass the session handle down as the password paramter and the certificate handle as the data parameter
                result = (SSL_AddCertificateAuthority( sslContext, (const char*)sslCert, arrCert->m_numOfElements, (LPCSTR)&sessionCtx ) ? 0 : -1);
                
                TINYCLR_CHECK_HRESULT(ThrowOnError( stack, result ));
            }
            else
            {

                arrCert->Pin();

                sslCert = arrCert->GetFirstElement();

                CLR_RT_HeapBlock *hbPwd = hbCert[ Library_spot_native_System_Security_Cryptography_X509Certificates_X509Certificate::FIELD__m_password ].Dereference(); FAULT_ON_NULL(hbPwd);

                LPCSTR szCAPwd = hbPwd->StringText();
                
                result = (SSL_AddCertificateAuthority( sslContext, (const char*)sslCert, arrCert->m_numOfElements, szCAPwd ) ? 0 : -1);
                
                TINYCLR_CHECK_HRESULT(ThrowOnError( stack, result ));
            }
        }
    }

    stack.SetResult_I4( sslContext );    

    TINYCLR_CLEANUP();

    if(FAILED(hr) && (sslContext != -1))
    {
        SSL_ExitContext( sslContext );        
    }

    TINYCLR_CLEANUP_END();
}

void Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::ThrowError( CLR_RT_StackFrame& stack, int errorCode )
{        
    NATIVE_PROFILE_CLR_NETWORK();
    CLR_RT_HeapBlock& res = stack.m_owningThread->m_currentException;
                            
    if((Library_corlib_native_System_Exception::CreateInstance( res, g_CLR_RT_WellKnownTypes.m_SocketException, CLR_E_FAIL, &stack )) == S_OK)
    {
        res.Dereference()[ Library_system_sockets_System_Net_Sockets_SocketException::FIELD___errorCode ].SetInteger( errorCode );
    }
}

HRESULT Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::ThrowOnError( CLR_RT_StackFrame& stack, int res )
{        
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    if(res != 0)
    {   
        ThrowError( stack, res );

        TINYCLR_SET_AND_LEAVE( CLR_E_PROCESS_EXCEPTION );
    }

    TINYCLR_NOCLEANUP();
}

