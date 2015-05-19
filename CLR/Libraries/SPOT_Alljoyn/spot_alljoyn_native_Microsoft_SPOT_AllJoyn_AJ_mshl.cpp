////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <windows.h>
#include "spot_alljoyn.h"


extern AJ_Status MarshalDefaultProps(AJ_Message* msg);
extern AJ_Status MarshalObjectDescriptions(AJ_Message* msg);

//--//

const int ARG_POOL_SIZE = 10;
const int MAX_PWD_LENGTH = 16;

//--//

static AJ_BusAttachment BusInstance;
static AJ_Arg           ArgPool[ARG_POOL_SIZE];
static bool             ArgInUse[ARG_POOL_SIZE] = {0};
static char             PwdText[MAX_PWD_LENGTH] = "";

//--//
//--//
//--//

//
// Helpers
//

HRESULT RetrieveBus( CLR_RT_StackFrame& stack, AJ_BusAttachment*& bus )
{
    TINYCLR_HEADER();
    
    bus = (AJ_BusAttachment*)stack.Arg1().NumericByRef().u4;  FAULT_ON_NULL(bus);
        
    TINYCLR_NOCLEANUP();    
}

//--//

void CopyFromManagedMsg(CLR_RT_HeapBlock* managedMsg, AJ_Message* msg)
{
    typedef Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ_Message Managed_AJ_Message;

    msg->msgId          =                     managedMsg[ Managed_AJ_Message::FIELD__msgId         ].NumericByRef().u4;
    msg->hdr            = (AJ_MsgHeader*)     managedMsg[ Managed_AJ_Message::FIELD__hdr           ].NumericByRef().u4;
    msg->replySerial    =                     managedMsg[ Managed_AJ_Message::FIELD__union0        ].NumericByRef().u4;
    msg->member         = (char*)             managedMsg[ Managed_AJ_Message::FIELD__union1        ].NumericByRef().u4;
    msg->iface          = (char*)             managedMsg[ Managed_AJ_Message::FIELD__iface         ].NumericByRef().u4;
    msg->sender         = (char*)             managedMsg[ Managed_AJ_Message::FIELD__sender        ].NumericByRef().u4;
    msg->destination    = (char*)             managedMsg[ Managed_AJ_Message::FIELD__destination   ].NumericByRef().u4;
    msg->signature      = (char*)             managedMsg[ Managed_AJ_Message::FIELD__signature     ].NumericByRef().u4;
    msg->sessionId      =                     managedMsg[ Managed_AJ_Message::FIELD__sessionId     ].NumericByRef().u4;
    msg->timestamp      =                     managedMsg[ Managed_AJ_Message::FIELD__timestamp     ].NumericByRef().u4;
    msg->ttl            =                     managedMsg[ Managed_AJ_Message::FIELD__ttl           ].NumericByRef().u4;
    msg->sigOffset      =                     managedMsg[ Managed_AJ_Message::FIELD__sigOffset     ].NumericByRef().u1;
    msg->varOffset      =                     managedMsg[ Managed_AJ_Message::FIELD__varOffset     ].NumericByRef().u1;
    msg->bodyBytes      =                     managedMsg[ Managed_AJ_Message::FIELD__bodyBytes     ].NumericByRef().u2;
    msg->bus            = (AJ_BusAttachment*) managedMsg[ Managed_AJ_Message::FIELD__bus           ].NumericByRef().u4;
    msg->outer          = (struct _AJ_Arg*)   managedMsg[ Managed_AJ_Message::FIELD__outer         ].NumericByRef().u4;        
}

void CopyToManagedMsg(CLR_RT_HeapBlock* managedMsg, AJ_Message* msg)
{
    typedef Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ_Message Managed_AJ_Message;
    
    managedMsg[ Managed_AJ_Message::FIELD__msgId         ].SetInteger(msg->msgId);
    managedMsg[ Managed_AJ_Message::FIELD__hdr           ].SetInteger((unsigned int)msg->hdr);
    managedMsg[ Managed_AJ_Message::FIELD__union0        ].SetInteger(msg->replySerial);
    managedMsg[ Managed_AJ_Message::FIELD__union1        ].SetInteger((unsigned int)msg->member);
    managedMsg[ Managed_AJ_Message::FIELD__iface         ].SetInteger((unsigned int)msg->iface);
    managedMsg[ Managed_AJ_Message::FIELD__sender        ].SetInteger((unsigned int)msg->sender);
    managedMsg[ Managed_AJ_Message::FIELD__destination   ].SetInteger((unsigned int)msg->destination);
    managedMsg[ Managed_AJ_Message::FIELD__signature     ].SetInteger((unsigned int)msg->signature);
    managedMsg[ Managed_AJ_Message::FIELD__sessionId     ].SetInteger(msg->sessionId);
    managedMsg[ Managed_AJ_Message::FIELD__timestamp     ].SetInteger(msg->timestamp);
    managedMsg[ Managed_AJ_Message::FIELD__ttl           ].SetInteger(msg->ttl);
    managedMsg[ Managed_AJ_Message::FIELD__sigOffset     ].SetInteger(msg->sigOffset);
    managedMsg[ Managed_AJ_Message::FIELD__varOffset     ].SetInteger(msg->varOffset);
    managedMsg[ Managed_AJ_Message::FIELD__bodyBytes     ].SetInteger(msg->bodyBytes);
    managedMsg[ Managed_AJ_Message::FIELD__bus           ].SetInteger((unsigned int)msg->bus);
    managedMsg[ Managed_AJ_Message::FIELD__outer         ].SetInteger((unsigned int)msg->outer);
}

//--//

AJ_Status AboutIconHandleGetContent(AJ_Message* msg, AJ_Message* reply, char* data, CLR_UINT32 dataSize)
{
    AJ_Status  status = AJ_OK;
    CLR_UINT32 u      = dataSize;

    status = AJ_MarshalReplyMsg(msg, reply);
    if (status != AJ_OK) {
        goto ErrorExit;
    }

    status = AJ_DeliverMsgPartial(reply, u + 4);
    if (status != AJ_OK) {
        goto ErrorExit;
    }

    status = AJ_MarshalRaw(reply, &u, 4);
    if (status != AJ_OK) {
        goto ErrorExit;
    }
    
    return AJ_MarshalRaw(reply, data, u);

ErrorExit:
    return status;
}

//--//

#define MAX_DIM_INTERFACE 100
#define MAX_NUM_INTERFACE 100

void DeserializeInterfaceString( LPCSTR data, LPSTR testinterface[] )
{
    const char * p = data;
    const char * cur = p;
    
    char delimiter = ',';
    int pos = 0;
    int i = 0;

    while(*p != NULL)
    {
        if (*p == delimiter)
        {
            // found a full string, copy it to array
            
            int size = p - cur + 1;  // extra byte for null
            testinterface[i] = new char[size];

            for (int j=0; j<size; j++){
                testinterface[i][j] = cur[j];
            }
            testinterface[i][size-1] = '\0';
            
            i ++;            
            cur = p+1;
        }
        p ++;
    }
    testinterface[i - 1] = '\0';
}

//--//
//--//
//--//

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::Initialize___VOID( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    AJ_Initialize();
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::SetBusLinkTimeout___MicrosoftSPOTAllJoynAJStatus__U4__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_UINT32        timeout;
    AJ_BusAttachment* bus       = NULL;
    AJ_Status         status    = AJ_OK;

    TINYCLR_CHECK_HRESULT( RetrieveBus( stack, bus ) );
    
    timeout = stack.Arg2().NumericByRef().u4;
    
    AJ_SetBusLinkTimeout( bus, timeout ); 
    
    stack.SetResult_I4( status );
    
    TINYCLR_NOCLEANUP();
} 
       


HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::NotifyLinkActive___VOID( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    AJ_NotifyLinkActive( );
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::BusLinkStateProc___MicrosoftSPOTAllJoynAJStatus__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    AJ_BusAttachment* bus    = NULL; 
    AJ_Status         status = AJ_OK;

    TINYCLR_CHECK_HRESULT( RetrieveBus( stack, bus ) );
    
    status = AJ_BusLinkStateProc( bus );

    stack.SetResult_I4( status );
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::SetIdleTimeouts___MicrosoftSPOTAllJoynAJStatus__U4__U4__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    AJ_BusAttachment* bus    = NULL; 
    AJ_Status         status = AJ_OK;
    CLR_UINT32        idleTo; 
    CLR_UINT32        probeTo; 
    
    TINYCLR_CHECK_HRESULT( RetrieveBus( stack, bus ) );

    idleTo  = stack.Arg2().NumericByRef().u4;
    probeTo = stack.Arg3().NumericByRef().u4;
    
    status = AJ_SetIdleTimeouts(bus, idleTo, probeTo );
    
    stack.SetResult_I4( (CLR_INT32)status );    
    
    TINYCLR_NOCLEANUP();
}


struct DiscoveryContext
{
    AJ_BusAttachment* _bus; 
    char              _daemonName[ AJ_MAX_SERVICE_NAME_SIZE ];
    char *            _pDaemonName;
    CLR_INT32         _timeout;
    bool              _fConnected;
    CLR_UINT16        _port;
    char              _serviceName[ AJ_MAX_SERVICE_NAME_SIZE ]; 
    CLR_UINT32        _flags;
    AJ_Status         _status;

    void Initialize( AJ_BusAttachment* bus, LPCSTR daemonName, CLR_INT32 timeout, bool fConnected, CLR_UINT16 port, LPCSTR serviceName, CLR_UINT32 flags )
    {
        _bus = bus; 
        _pDaemonName = NULL;
        if ( daemonName )
        {
            hal_strcpy_s( _daemonName, sizeof(_daemonName), daemonName );
            _pDaemonName = _daemonName;
        }
        _timeout = timeout;
        _fConnected = fConnected;
        _port = port;
        hal_strcpy_s( _serviceName, sizeof(_serviceName), serviceName ); 
        _flags = flags;
        _status = AJ_OK;
        
    }
};

AJ_Status CustomStartService(   AJ_BusAttachment*     bus,
                                LPCSTR                daemonName,
                                CLR_UINT32            timeout,
                                bool                  fConnected,
                                CLR_UINT16            port,
                                LPCSTR                name,
                                CLR_UINT32            flags,
                                const AJ_SessionOpts* opts )
{
    AJ_Time   timer;
    bool      fServiceStarted = false;
    AJ_Status status          = AJ_OK;

    AJ_InfoPrintf(("AJ_StartService(bus=0x%p, daemonName=\"%s\", timeout=%d., connected=%d., port=%d., name=\"%s\", flags=0x%x, opts=0x%p)\n",
                   bus, daemonName, timeout, connected, port, name, flags, opts));

    AJ_InitTimer(&timer);

    //
    // Connect to bus and establish session 
    //
    while(true) 
    {
        if( AJ_GetElapsedTime( &timer, TRUE ) > timeout ) 
        {
            return AJ_ERR_TIMEOUT;
        }

        //
        // Ensure connection
        //
        if(!fConnected) 
        {
            AJ_InfoPrintf(("AJ_StartService(): AJ_FindBusAndConnect()\n"));
            
            status = AJ_FindBusAndConnect( bus, daemonName, AJ_CONNECT_TIMEOUT );
            
            if(status != AJ_OK) 
            {
                AJ_WarnPrintf(("AJ_StartService(): connect failed: sleeping for %d seconds\n", AJ_CONNECT_PAUSE / 1000));
                
                AJ_Sleep(AJ_CONNECT_PAUSE);
                
                continue;
            }
            
            AJ_InfoPrintf(("AJ_StartService(): connected to bus\n"));
        }
        
        //
        // Bind a session 
        //
        AJ_InfoPrintf(("AJ_StartService(): AJ_BindSessionPort()\n"));
        
        status = AJ_BusBindSessionPort( bus, port, opts, 0 );
        
        if (status == AJ_OK) 
        {
            break;
        }
        
        AJ_ErrPrintf(("AJ_StartService(): AJ_Disconnect(): status=%s.\n", AJ_StatusText(status)));
        
        AJ_Disconnect(bus);
    }

    //
    //
    // 
    while( !fServiceStarted && ( status == AJ_OK ) ) 
    {
        AJ_Message msg;

        status = AJ_UnmarshalMsg(bus, &msg, AJ_UNMARSHAL_TIMEOUT);
        if (status == AJ_ERR_NO_MATCH) {
            // Ignore unknown messages
            status = AJ_OK;
            continue;
        }
        if (status != AJ_OK) {
            AJ_ErrPrintf(("AJ_StartService(): status=%s.\n", AJ_StatusText(status)));            
            break;
        }

        switch (msg.msgId) 
        {
            case AJ_REPLY_ID(AJ_METHOD_BIND_SESSION_PORT):
                if( msg.hdr->msgType == AJ_MSG_ERROR ) 
                {
                    AJ_ErrPrintf(("AJ_StartService(): AJ_METHOD_BIND_SESSION_PORT: %s\n", msg.error));
                    status = AJ_ERR_FAILURE;
                } 
                else 
                {
                    AJ_InfoPrintf(("AJ_StartService(): AJ_BusRequestName()\n"));
                    status = AJ_BusRequestName(bus, name, flags);
                }
                break;

            case AJ_REPLY_ID(AJ_METHOD_REQUEST_NAME):
                if( msg.hdr->msgType == AJ_MSG_ERROR ) 
                {
                    AJ_ErrPrintf(("AJ_StartService(): AJ_METHOD_REQUEST_NAME: %s\n", msg.error));
                    status = AJ_ERR_FAILURE;
                } 
                else 
                {
                    AJ_InfoPrintf(("AJ_StartService(): AJ_BusAdvertiseName()\n"));
                    status = AJ_BusAdvertiseName(bus, name, (opts != NULL) ? opts->transports : AJ_TRANSPORT_ANY, AJ_BUS_START_ADVERTISING, 0);
                }
                break;

            case AJ_REPLY_ID(AJ_METHOD_ADVERTISE_NAME):
                if( msg.hdr->msgType == AJ_MSG_ERROR ) 
                {
                    AJ_ErrPrintf(("AJ_StartService(): AJ_METHOD_ADVERTISE_NAME: %s\n", msg.error));
                    status = AJ_ERR_FAILURE;
                } 
                else 
                {
                    fServiceStarted = true;
                }
                break;

            default:
                //
                // Pass to the built-in bus message handlers
                //
                AJ_InfoPrintf(("AJ_StartService(): AJ_BusHandleBusMessage()\n"));
                
                status = AJ_BusHandleBusMessage(&msg);
                break;
        }
        
        AJ_CloseMsg(&msg);
    }

    if (status == AJ_OK) {
        //
        // Do not send About message here, let the managed app handle that
        //
        //status = AJ_AboutInit(bus, port);
    } else {
        AJ_WarnPrintf(("AJ_StartService(): AJ_Disconnect(): status=%s\n", AJ_StatusText(status)));
        
        AJ_Disconnect(bus);
    }
        
    return status;
}

void StartServiceCallback( void* context )
{
    DiscoveryContext* dc = (DiscoveryContext*)context;

    dc->_status = CustomStartService( dc->_bus, dc->_pDaemonName, dc->_timeout, dc->_fConnected, dc->_port, dc->_serviceName, dc->_flags, NULL); 
}

DWORD WINAPI ThreadProc(LPVOID lpParameter)
{
    return 1;
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::StartService___MicrosoftSPOTAllJoynAJStatus__U4__STRING__I4__I1__U2__STRING__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    AJ_BusAttachment* bus         = NULL;
    LPCSTR            daemonName  = NULL;
    CLR_INT32         timeout;
    CLR_INT64*        timeoutTicks;
    bool              fConnected;
    CLR_UINT16        port;
    LPCSTR            serviceName = NULL;
    CLR_UINT32        flags;
    bool              fRes;
    OSTASK*           task        = NULL;
    DiscoveryContext* context     = NULL;

    TINYCLR_CHECK_HRESULT( RetrieveBus( stack, bus) );
                      
    daemonName  = stack.Arg2().RecoverString( );
    timeout     = stack.Arg3().NumericByRef().s4;
    fConnected  = stack.Arg4().NumericByRef().s1 != 0;
    port        = stack.Arg5().NumericByRef().s2;
    serviceName = stack.Arg6().RecoverString();
    flags       = stack.Arg7().NumericByRef().u4;

    if( daemonName && hal_strlen_s( daemonName ) > AJ_MAX_SERVICE_NAME_SIZE )
    {
        TINYCLR_SET_AND_LEAVE( CLR_E_INVALID_PARAMETER );
    }
    if( hal_strlen_s( serviceName ) > AJ_MAX_SERVICE_NAME_SIZE )
    {
        TINYCLR_SET_AND_LEAVE( CLR_E_INVALID_PARAMETER );
    }

    {
        CLR_RT_HeapBlock hbTimeout;
        hbTimeout.SetInteger( timeout );
        TINYCLR_CHECK_HRESULT(stack.SetupTimeout( hbTimeout, timeoutTicks ));
    }
    
    //
    // Push "state" onto the eval stac in the form of a OSTASK
    //
    if(stack.m_customState == 1)
    {   
        task    = (OSTASK*)private_malloc( sizeof(OSTASK) );
        context = (DiscoveryContext*)private_malloc( sizeof(DiscoveryContext) ); 
        
        context->Initialize( bus, daemonName, timeout, fConnected, port, serviceName, flags ); 
        task   ->Initialize( StartServiceCallback,  context ); 

        //
        // we will keep track of task and context in our managed stack
        //
        stack.PushValueI4( (CLR_UINT32)context );
        stack.PushValueI4( (CLR_UINT32)task    );

        OSTASK_Post( task ); 
        
        stack.m_customState = 2;
    }

    //
    // wait for completion, fRes will tell us about timeout being expired
    //
    fRes = true;
    while(fRes && task->HasCompleted() == FALSE)
    {
        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.WaitEvents( stack.m_owningThread, *timeoutTicks, CLR_RT_ExecutionEngine::c_Event_OSTask, fRes ));
    }

    TINYCLR_CLEANUP();
    
    if(hr != CLR_E_THREAD_WAITING)
    {
        //
        // we are done, cleanup
        // 
        
        //
        // Get results
        //
        task    = (OSTASK*          )stack.m_evalStack[ 1 ].NumericByRef().u4;
        context = (DiscoveryContext*)stack.m_evalStack[ 2 ].NumericByRef().u4;
        
        stack.PopValue(); // task
        stack.PopValue(); // context
        stack.PopValue(); // Timeout

        stack.SetResult_I4( fRes ? (CLR_INT32)context->_status : AJ_ERR_TIMEOUT );        

        private_free( task    );
        private_free( context );
    }

    TINYCLR_CLEANUP_END();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::StartClientByName___MicrosoftSPOTAllJoynAJStatus__U4__STRING__U4__U1__STRING__U2__BYREF_U4__MicrosoftSPOTAllJoynAJSessionOpts__BYREF_STRING( CLR_RT_StackFrame& stack )
{
    typedef Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ_SessionOpts Managed_AJ_SessionOpts;
    
    TINYCLR_HEADER();
    
    char              fullServiceName[AJ_MAX_SERVICE_NAME_SIZE] = "";
    CLR_RT_HeapBlock  hbFullName;
    LPSTR             fullName     = NULL;
    CLR_RT_HeapBlock  hbSessionId;
    CLR_UINT32        sessionId = 0;    
    CLR_RT_HeapBlock* managedOpts = NULL;
    AJ_SessionOpts    opts = {0};
    AJ_BusAttachment* bus         = NULL;    
    AJ_Status         status      = AJ_OK;
    LPCSTR            daemonName  = NULL;
    CLR_UINT32        timeout; 
    bool              fConnected;
    LPCSTR            clientName  = NULL;
    CLR_UINT16        port;
    
    TINYCLR_CHECK_HRESULT( RetrieveBus( stack, bus ) );
    
    daemonName  = stack.Arg2().RecoverString( );
    timeout     = stack.Arg3().NumericByRef().u4;
    fConnected  = stack.Arg4().NumericByRef().s1 != 0;
    clientName  = stack.Arg5().RecoverString();
    port        = stack.Arg6().NumericByRef().s2;
    
    TINYCLR_CHECK_HRESULT( hbSessionId.LoadFromReference( stack.Arg7( ) ) );
    sessionId = hbSessionId.NumericByRef().u4;    
           
    managedOpts = stack.ArgN(8).Dereference();                
    
    if( NULL != managedOpts )
    {
        opts.traffic      =  managedOpts[ Managed_AJ_SessionOpts::FIELD__traffic      ].NumericByRef().u1;
        opts.proximity    =  managedOpts[ Managed_AJ_SessionOpts::FIELD__proximity    ].NumericByRef().u1;
        opts.transports   =  managedOpts[ Managed_AJ_SessionOpts::FIELD__transports   ].NumericByRef().u2;
        opts.isMultipoint =  managedOpts[ Managed_AJ_SessionOpts::FIELD__isMultipoint ].NumericByRef().u4;
    }

    hbFullName.LoadFromReference( stack.ArgN(9) );
    fullName = (LPSTR)hbFullName.RecoverString();

    status = AJ_StartClientByName(  bus, 
                                    daemonName,
                                    timeout,
                                    fConnected,
                                    clientName,
                                    port,
                                    &sessionId,
                                    managedOpts == NULL ? NULL : &opts,
                                    fullName == NULL ? NULL : fullServiceName ); // should never be null since ref param

    if( status == AJ_OK )
    {     
        hbSessionId.SetInteger( sessionId );
        TINYCLR_CHECK_HRESULT( hbSessionId.StoreToReference( stack.Arg7( ), 0 ) );   
        
        if(fullName)
        {
            TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_String::CreateInstance( hbFullName, fullServiceName ));
        }
        else
        {
            hbFullName.SetObjectReference( NULL );
        }
        
        TINYCLR_CHECK_HRESULT( hbFullName.StoreToReference( stack.ArgN( 9 ), 0 ) );
    }

    stack.SetResult_I4( (CLR_INT32)status );
    
    TINYCLR_NOCLEANUP();
}

static CLR_UINT32 PasswordCallback( CLR_UINT8 * buffer, CLR_UINT32 bufLen )
{    
    int pwdLen = hal_strlen_s( PwdText );
    
    if ( pwdLen > bufLen )
    {
        pwdLen = bufLen;
    }
 
	// Always terminated with a '\0' for following AJ_Printf().

	PwdText[ pwdLen ] = '\0';
	for (int i=0; i<pwdLen; i ++)
    {
            buffer[ i ] = PwdText[ i ];
    }

    return pwdLen;
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::UsePeerAuthentication___VOID__BOOLEAN( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    bool usePeerAuth  = stack.Arg1().NumericByRef().s1 != 0;    
    
    if (usePeerAuth)
    {
        AJ_BusSetPasswordCallback( &BusInstance, PasswordCallback );
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::SetPassphrase___VOID__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    hr = S_OK;
    
    LPCSTR pwdText = stack.Arg1().RecoverString();     
    hal_strcpy_s( PwdText, sizeof(PwdText), pwdText );
        
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::AlwaysPrintf___VOID__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    LPCSTR message = stack.Arg1().RecoverString(); 

    //
    // TODO: enable debug printf or remove this API from manged app
    //AJ_AlwaysPrintf( message );
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::MarshalReplyMsg___MicrosoftSPOTAllJoynAJStatus__MicrosoftSPOTAllJoynAJMessage__MicrosoftSPOTAllJoynAJMessage( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        AJ_Message replyMsg;
        AJ_Message msg;

        CLR_RT_HeapBlock* managedMsg = stack.Arg1().Dereference();  FAULT_ON_NULL(managedMsg); 
        CopyFromManagedMsg(managedMsg, &msg);
        
        AJ_Status status = AJ_MarshalReplyMsg(&msg, &replyMsg);
        
        CLR_RT_HeapBlock* managedReplyMsg = stack.Arg2().Dereference();  FAULT_ON_NULL(managedReplyMsg);    
        
        CopyToManagedMsg(managedReplyMsg, &replyMsg);
        CopyToManagedMsg(managedMsg, &msg);
        
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT32( stack, status );
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::MarshalMethodCall___MicrosoftSPOTAllJoynAJStatus__U4__MicrosoftSPOTAllJoynAJMessage__U4__STRING__U4__U1__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    AJ_BusAttachment* bus        = NULL;    
    AJ_Status         status     = AJ_OK;   
    AJ_Message        msg;
    CLR_RT_HeapBlock* managedMsg = NULL;    
    CLR_UINT32        msgId; 
    LPCSTR            destination = NULL;
    AJ_SessionId      sessionId;     
    CLR_UINT8         flags; 
    CLR_UINT32        timeout; 
    
    TINYCLR_CHECK_HRESULT( RetrieveBus( stack, bus ) );

    managedMsg = stack.Arg2().Dereference();  FAULT_ON_NULL(managedMsg); 
    
    CopyFromManagedMsg(managedMsg, &msg);
    
    msgId       = stack.Arg3().NumericByRef().u4;
    destination = stack.Arg4().RecoverString( );  FAULT_ON_NULL(destination);
    sessionId   = stack.Arg5().NumericByRef().u4;    
    flags       = stack.Arg6().NumericByRef().u1;
    timeout     = stack.Arg7().NumericByRef().u4;
    
    status = AJ_MarshalMethodCall( bus, &msg, msgId, destination, sessionId, flags, timeout );

    if(status == AJ_OK) CopyToManagedMsg( managedMsg, &msg );
    
    stack.SetResult_I4( (CLR_INT32)status );
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::DeliverMsg___MicrosoftSPOTAllJoynAJStatus__MicrosoftSPOTAllJoynAJMessage( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    AJ_Message  replyMsg;
    AJ_Status   status = AJ_OK;
    
    CLR_RT_HeapBlock* managedReplyMsg = stack.Arg1().Dereference();  FAULT_ON_NULL(managedReplyMsg);            
                
    CopyFromManagedMsg(managedReplyMsg, &replyMsg);
        
    AJ_DeliverMsg(&replyMsg);
    
    stack.SetResult_I4( (CLR_INT32)status );
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::MarshalSignal___MicrosoftSPOTAllJoynAJStatus__U4__MicrosoftSPOTAllJoynAJMessage__U4__U4__U4__U1__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    AJ_BusAttachment*   bus         = NULL;
    AJ_Status           status      = AJ_OK;
    CLR_RT_HeapBlock*   managedMsg  = NULL;
    CLR_UINT32          msgId;
    LPCSTR              destination;
    AJ_SessionId        sessionId;
    CLR_UINT8           flags;
    CLR_UINT32          ttl;    
    AJ_Message          msg;        
        
    TINYCLR_CHECK_HRESULT( RetrieveBus( stack, bus ) );

    managedMsg  = stack.Arg2().Dereference();           FAULT_ON_NULL(managedMsg);    
    msgId       = stack.Arg3().NumericByRef().u4;
    destination = stack.Arg4().RecoverString();
    sessionId   = stack.Arg5().NumericByRef().u4;
    flags       = stack.Arg6().NumericByRef().u1;
    ttl         = stack.Arg7().NumericByRef().u4;    

	CopyFromManagedMsg(managedMsg, &msg);

    status = AJ_MarshalSignal(bus, &msg, msgId, destination, sessionId, flags, ttl);

    if(status == AJ_OK) CopyToManagedMsg( managedMsg, &msg );
        
    stack.SetResult_I4( (CLR_INT32)status );

    TINYCLR_NOCLEANUP();
}


HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::MarshalArg___MicrosoftSPOTAllJoynAJStatus__MicrosoftSPOTAllJoynAJMessage__STRING__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    AJ_Message               msg;
    AJ_Status                status     = AJ_OK;
    CLR_RT_HeapBlock*        managedMsg = NULL;
    CLR_RT_HeapBlock_String* signature  = NULL;
    CLR_UINT32               arg;

    managedMsg = stack.Arg1().Dereference();        FAULT_ON_NULL(managedMsg);
    signature  = stack.Arg2().DereferenceString();  FAULT_ON_NULL(signature);
    arg        = stack.Arg3().NumericByRef().u4;
                
    CopyFromManagedMsg(managedMsg, &msg);
    
    status = AJ_MarshalArgs(&msg, signature->StringText(), arg);

    if(status == AJ_OK) CopyToManagedMsg( managedMsg, &msg );
        
    stack.SetResult_I4( (CLR_INT32)status );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::MarshalArg___MicrosoftSPOTAllJoynAJStatus__MicrosoftSPOTAllJoynAJMessage__STRING__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    AJ_Message               msg;
    AJ_Status                status     = AJ_OK;
    CLR_RT_HeapBlock*        managedMsg = NULL;
    CLR_RT_HeapBlock_String* signature  = NULL;
    CLR_RT_HeapBlock_String* arg        = NULL;

    managedMsg = stack.Arg1().Dereference();        FAULT_ON_NULL(managedMsg);
    signature  = stack.Arg2().DereferenceString();  FAULT_ON_NULL(signature);
    arg        = stack.Arg3().DereferenceString();  FAULT_ON_NULL(arg);
                
    CopyFromManagedMsg(managedMsg, &msg);
    
    status = AJ_MarshalArgs(&msg, signature->StringText(), arg->StringText());

    if(status == AJ_OK) CopyToManagedMsg( managedMsg, &msg );
        
    stack.SetResult_I4( (CLR_INT32)status );

    TINYCLR_NOCLEANUP();


    
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::MarshalArgs___MicrosoftSPOTAllJoynAJStatus__MicrosoftSPOTAllJoynAJMessage__STRING__STRING__STRING__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    AJ_Message msg;
    CLR_RT_HeapBlock*        managedMsg = NULL;
    CLR_RT_HeapBlock_String* signature  = NULL;
    CLR_RT_HeapBlock_String* arg1       = NULL;
    CLR_RT_HeapBlock_String* arg2       = NULL;
    CLR_RT_HeapBlock_String* arg3       = NULL;
    AJ_Status                status     = AJ_OK;
    
    managedMsg = stack.Arg1().Dereference();  FAULT_ON_NULL(managedMsg);     

    CopyFromManagedMsg( managedMsg, &msg );
    
    signature  = stack.Arg2().DereferenceString();  FAULT_ON_NULL(signature);
    arg1       = stack.Arg3().DereferenceString();  FAULT_ON_NULL(arg1);
    arg2       = stack.Arg4().DereferenceString();  FAULT_ON_NULL(arg2);
    arg3       = stack.Arg5().DereferenceString();  FAULT_ON_NULL(arg3);

    LPCSTR a = signature->StringText();
    LPCSTR b = arg1     ->StringText();
    LPCSTR c = arg2     ->StringText();
    LPCSTR d = arg3     ->StringText();
            
    status = AJ_MarshalArgs(&msg, a, b, c, d);

    if( status == AJ_OK )
    {
        CopyToManagedMsg( managedMsg, &msg );
    }
    
    stack.SetResult_I4( (CLR_INT32)status );
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::MarshalArgs___MicrosoftSPOTAllJoynAJStatus__MicrosoftSPOTAllJoynAJMessage__STRING__STRING__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    AJ_Message msg;
    CLR_RT_HeapBlock*        managedMsg = NULL;
    CLR_RT_HeapBlock_String* signature  = NULL;
    CLR_RT_HeapBlock_String* arg1       = NULL;
    CLR_RT_HeapBlock_String* arg2       = NULL;
    AJ_Status                status     = AJ_OK;

    managedMsg = stack.Arg1().Dereference();  FAULT_ON_NULL(managedMsg);     

    CopyFromManagedMsg( managedMsg, &msg );
    
    signature  = stack.Arg2().DereferenceString();  FAULT_ON_NULL(signature);
    arg1       = stack.Arg3().DereferenceString();  FAULT_ON_NULL(arg1);
    arg2       = stack.Arg4().DereferenceString();  FAULT_ON_NULL(arg2);

    LPCSTR a = signature->StringText();
    LPCSTR b = arg1     ->StringText();
    LPCSTR c = arg2     ->StringText();
            
    status = AJ_MarshalArgs(&msg, a, b, c );
    
    if(status == AJ_OK) CopyToManagedMsg( managedMsg, &msg );
    
    stack.SetResult_I4( (CLR_INT32)status );
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::MarshalArgs___MicrosoftSPOTAllJoynAJStatus__MicrosoftSPOTAllJoynAJMessage__STRING__STRING__SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    AJ_Message msg;
    CLR_RT_HeapBlock*        managedMsg = NULL;
    CLR_RT_HeapBlock_String* signature  = NULL;
    CLR_RT_HeapBlock_String* arg1       = NULL;
    CLR_RT_HeapBlock_Array * arg2       = NULL;
    AJ_Status                status     = AJ_OK;

    managedMsg = stack.Arg1().Dereference();  FAULT_ON_NULL(managedMsg);     

    CopyFromManagedMsg( managedMsg, &msg );
    
    signature  = stack.Arg2().DereferenceString();  FAULT_ON_NULL(signature);
    arg1       = stack.Arg3().DereferenceString();  FAULT_ON_NULL(arg1);    
    arg2        = stack.Arg4().DereferenceArray() ;  FAULT_ON_NULL(arg2);    

    LPCSTR a = signature->StringText();
    LPCSTR b = arg1     ->StringText();
            
    status = AJ_MarshalArgs(&msg, a, b, (char*)arg2->GetFirstElement(), arg2->m_numOfElements );

    if(status == AJ_OK) CopyToManagedMsg( managedMsg, &msg );
    
    stack.SetResult_I4( (CLR_INT32)status );
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::AboutIconHandleGetContent___MicrosoftSPOTAllJoynAJStatus__MicrosoftSPOTAllJoynAJMessage__MicrosoftSPOTAllJoynAJMessage__SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    AJ_Message              msg;
    AJ_Message              replyMsg;
    CLR_RT_HeapBlock*       managedMsg      = NULL;
    CLR_RT_HeapBlock*       managedReplyMsg = NULL;
    CLR_RT_HeapBlock_Array* data            = NULL;        
    AJ_Status               status          = AJ_OK;
    
    managedMsg      = stack.Arg1().Dereference();       FAULT_ON_NULL(managedMsg);
    managedReplyMsg = stack.Arg2().Dereference();       FAULT_ON_NULL(managedReplyMsg);
    data            = stack.Arg3().DereferenceArray();  FAULT_ON_NULL(data);      

    CopyFromManagedMsg(managedMsg     , &msg     );
    CopyFromManagedMsg(managedReplyMsg, &replyMsg);
    
    status = AboutIconHandleGetContent( &msg,  &replyMsg, (char*)data->GetFirstElement(), data->m_numOfElements);

    if(status == AJ_OK) CopyToManagedMsg( managedMsg     , &msg      );
    if(status == AJ_OK) CopyToManagedMsg( managedReplyMsg, &replyMsg );
    
    stack.SetResult_I4( (CLR_INT32)status );

    TINYCLR_NOCLEANUP();
}


HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::GetLocalGUID___STATIC__MicrosoftSPOTAllJoynAJStatus__SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    AJ_GUID   guid;
    AJ_Status status     = AJ_OK;
    
    CLR_RT_HeapBlock_Array* data = stack.Arg0().DereferenceArray();  FAULT_ON_NULL(data);

    AJ_CreateNewGUID((uint8_t*)&guid, sizeof(AJ_GUID));

    char* a = (char*)data->GetFirstElement();
    char* p = (char*)&guid;

    memcpy( a, p, data->m_numOfElements ); 

    stack.SetResult_I4( (CLR_INT32)status );
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::GetArgPtr___U4__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    CLR_INT32  idx = stack.Arg1().NumericByRef().s4;
    CLR_UINT32 ret = (UINT32)&ArgPool[idx];    
        
    stack.SetResult_U4( ret );
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::GetArgString___STRING__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    CLR_INT32 idx    = stack.Arg1().NumericByRef().s4;    
    AJ_Arg&   arg    = ArgPool[idx];
    LPCSTR    retVal = arg.val.v_string;
        
    stack.SetResult_String( retVal );
    
    TINYCLR_NOCLEANUP_NOLABEL();
}


HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::MarshalContainer___MicrosoftSPOTAllJoynAJStatus__MicrosoftSPOTAllJoynAJMessage__U4__U1( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    AJ_Message        msg;
    AJ_Arg*           arg; 
    CLR_UINT8         typeId; 
    CLR_RT_HeapBlock* managedMsg = NULL;     
    AJ_Status         status     = AJ_OK;
    
    managedMsg =          stack.Arg1().Dereference();       FAULT_ON_NULL(managedMsg)
    arg        = (AJ_Arg*)stack.Arg2().NumericByRef().s4;        
    typeId     =          stack.Arg3().NumericByRef().u1;

    status = AJ_MarshalContainer(&msg, arg, typeId);

    if( status == AJ_OK)
    {
        CopyFromManagedMsg(managedMsg, &msg);
    }
    
    status = AJ_MarshalContainer(&msg, arg, typeId);

    if( status == AJ_OK) CopyToManagedMsg(managedMsg, &msg);    
                
    stack.SetResult_I4( (CLR_INT32)status );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::MarshalCloseContainer___MicrosoftSPOTAllJoynAJStatus__MicrosoftSPOTAllJoynAJMessage__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    AJ_Message  msg;        
    AJ_Arg *    arg    = NULL;
    AJ_Status   status = AJ_OK; 
    
    CLR_RT_HeapBlock* managedMsg =          stack.Arg1().Dereference();     FAULT_ON_NULL(managedMsg);     
    arg                          = (AJ_Arg*)stack.Arg2().NumericByRef().u4;                     
    
    CopyFromManagedMsg(managedMsg, &msg);
         
    status = AJ_MarshalCloseContainer(&msg, arg);

    if(status == AJ_OK) CopyToManagedMsg( managedMsg, &msg );
    
    stack.SetResult_I4( (CLR_INT32)status );
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::MarshalObjectDescriptions___MicrosoftSPOTAllJoynAJStatus__MicrosoftSPOTAllJoynAJMessage( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    AJ_Message msg;
    AJ_Status  status  = AJ_OK;
    
    CLR_RT_HeapBlock* managedMsg = stack.Arg1().Dereference();  FAULT_ON_NULL(managedMsg);     

    CopyFromManagedMsg(managedMsg, &msg);
    
    status = MarshalObjectDescriptions(&msg);
        
    if(status == AJ_OK) CopyToManagedMsg( managedMsg, &msg );
    
    stack.SetResult_I4( (CLR_INT32)status );
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::MarshalDefaultProps___MicrosoftSPOTAllJoynAJStatus__MicrosoftSPOTAllJoynAJMessage( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    AJ_Message msg;
    AJ_Status  status = AJ_OK;
        
    CLR_RT_HeapBlock* managedMsg = stack.Arg1().Dereference();  FAULT_ON_NULL(managedMsg);                
    
    CopyFromManagedMsg(managedMsg, &msg);
    
    status = MarshalDefaultProps(&msg);
    
    if(status == AJ_OK) CopyToManagedMsg( managedMsg, &msg );
        
    stack.SetResult_I4( (CLR_INT32)status );
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::RegisterObjects___STATIC__VOID__STRING__STRING__U1__I4__BOOLEAN__BOOLEAN( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 
    
    static char*     theInterface[MAX_DIM_INTERFACE] = {0,};

    CLR_RT_HeapBlock_String* pathS   = stack.Arg0().DereferenceString();        
    CLR_RT_HeapBlock_String* ifacesS = stack.Arg1().DereferenceString();            
    uint8_t flags                    = stack.Arg2().NumericByRef().u1;
    uint8_t context                  = stack.Arg3().NumericByRef().u4;    
    bool fUseProperties              = stack.Arg4().NumericByRef().u1 != 0;
    bool fLocal                      = stack.Arg5().NumericByRef().u1 != 0;

    LPCSTR path   = pathS  ->StringText(); 
    LPCSTR ifaces = ifacesS->StringText();
    
    DeserializeInterfaceString( ifaces, theInterface );
    
    if (fLocal == true)
    {
        if (fUseProperties == true)
        {
            static const AJ_InterfaceDescription localInterfaces[] = {
                AJ_PropertiesIface,     // This must be included for any interface that has properties. 
                theInterface,
                NULL
            };    
            
            static const AJ_Object appLocalObjects[] = {
                { path, localInterfaces,  AJ_OBJ_FLAG_ANNOUNCED | AJ_OBJ_FLAG_DESCRIBED  },  // make them announceable
                { NULL }
            };
            
            AJ_RegisterObjects(appLocalObjects, NULL);
            AJ_PrintXML(appLocalObjects);
        }
        else
        {
            static const AJ_InterfaceDescription localInterfaces[] = {
                theInterface,
                NULL
            };
            
            static const AJ_Object appLocalObjects[] = {
                { path, localInterfaces, AJ_OBJ_FLAG_ANNOUNCED | AJ_OBJ_FLAG_DESCRIBED  },  // make them announceable
                { NULL }
            };
            
            AJ_RegisterObjects(appLocalObjects, NULL);
        }
    }
    else
    {
        if (fUseProperties == true)
        {
            static const AJ_InterfaceDescription proxyInterfaces[] = {
                AJ_PropertiesIface,     // This must be included for any interface that has properties. 
                theInterface,
                NULL
            };    
            
            static const AJ_Object appProxyObjects[] = {
                { path, proxyInterfaces,  AJ_OBJ_FLAG_ANNOUNCED | AJ_OBJ_FLAG_DESCRIBED  },  // make them announceable
                { NULL }
            };
            
            AJ_RegisterObjects(NULL, appProxyObjects);
            AJ_PrintXML(appProxyObjects);
        }
        else
        {
            static const AJ_InterfaceDescription proxyInterfaces[] = {
                theInterface,
                NULL
            };
            
            static const AJ_Object appProxyObjects[] = {
                { path, proxyInterfaces, AJ_OBJ_FLAG_ANNOUNCED | AJ_OBJ_FLAG_DESCRIBED  },  // make them announceable
                { NULL }
            };
            
            AJ_RegisterObjects(NULL, appProxyObjects);
        }
    }

    TINYCLR_NOCLEANUP_NOLABEL();
}


HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::UnmarshalVariant___STRING__MicrosoftSPOTAllJoynAJMessage( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    AJ_Message msg;
    LPCSTR variant;
    AJ_Status status = AJ_OK;
    
    CLR_RT_HeapBlock* managedMsg = stack.Arg1().Dereference();  FAULT_ON_NULL(managedMsg);
    
    CopyFromManagedMsg(managedMsg, &msg);
        
    status = AJ_UnmarshalVariant(&msg, &variant);
    
    if(status == AJ_OK) CopyToManagedMsg( managedMsg, &msg );
    
    stack.SetResult_String( variant );
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::MarshalVariant___MicrosoftSPOTAllJoynAJStatus__MicrosoftSPOTAllJoynAJMessage__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    AJ_Message               msg;
    CLR_RT_HeapBlock*        managedMsg = NULL;
    CLR_RT_HeapBlock_String* signature  = NULL;
    AJ_Status                status     = AJ_OK;   

    managedMsg = stack.Arg1().Dereference();        FAULT_ON_NULL(managedMsg);                
    signature  = stack.Arg2().DereferenceString();  FAULT_ON_NULL(signature);

    CopyFromManagedMsg(managedMsg, &msg);        

    status = AJ_MarshalVariant(&msg, signature->StringText());
    
    if(status == AJ_OK) CopyToManagedMsg( managedMsg, &msg );    
    
    stack.SetResult_I4( (CLR_INT32)status );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::UnmarshalPropertyArgs___STRING__MicrosoftSPOTAllJoynAJMessage__BYREF_U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    AJ_Message       msg;
    LPCSTR           signature = NULL;
    CLR_RT_HeapBlock hbpropId;
    CLR_UINT32       propId    = 0;
    AJ_Status        status    = AJ_OK; 
    
    CLR_RT_HeapBlock* managedMsg = stack.Arg1().Dereference();  FAULT_ON_NULL(managedMsg);                
    
    CopyFromManagedMsg(managedMsg, &msg);

    hbpropId.LoadFromReference( stack.Arg2() ); 

    status = AJ_UnmarshalPropertyArgs( &msg, &propId, &signature );

    if( status == AJ_OK )
    {
        CopyToManagedMsg( managedMsg, &msg );

        hbpropId.SetInteger( propId ); 
        TINYCLR_CHECK_HRESULT( hbpropId.StoreToReference( stack.Arg2(), 0 ) ); 
    
        stack.SetResult_String( signature );    
    }
    else
    {
        stack.SetResult_String( NULL );    
    }
    
    TINYCLR_NOCLEANUP();
}


HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::BusSetPasswordCallback___VOID( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 
    
    TINYCLR_SET_AND_LEAVE( stack.NotImplementedStub() );
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::UnmarshalMsg___MicrosoftSPOTAllJoynAJStatus__U4__MicrosoftSPOTAllJoynAJMessage__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 
    
    AJ_Message msg;
    CLR_RT_HeapBlock* managedMsg = NULL;
    AJ_BusAttachment* bus        = NULL;
    AJ_Status         status     = AJ_OK; 
    CLR_UINT32        timeout;

    TINYCLR_CHECK_HRESULT( RetrieveBus( stack, bus ) );
    
    managedMsg = stack.Arg2().Dereference();        FAULT_ON_NULL(managedMsg);                    
    timeout    = stack.Arg3().NumericByRef().u4;
    
    CopyFromManagedMsg(managedMsg, &msg);
    
    status = AJ_UnmarshalMsg((AJ_BusAttachment *)bus, &msg, timeout);
        
    if(status == AJ_OK) CopyToManagedMsg( managedMsg, &msg );
    
    stack.SetResult_I4( (CLR_INT32)status );
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::UnmarshalArg___MicrosoftSPOTAllJoynAJStatus__MicrosoftSPOTAllJoynAJMessage__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    AJ_Message        msg;
    AJ_Status         status = AJ_OK;
    AJ_Arg*           pArg;
    CLR_RT_HeapBlock* managedMsg = NULL;

    managedMsg =          stack.Arg1().Dereference();  FAULT_ON_NULL(managedMsg);                    
    pArg       = (AJ_Arg*)stack.Arg2().NumericByRef().u4;
    
    CopyFromManagedMsg(managedMsg, &msg);
    
    status = AJ_UnmarshalArg( &msg, pArg );
    
    stack.SetResult_I4( status );
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::UnmarshalArgs___MicrosoftSPOTAllJoynAJStatus__MicrosoftSPOTAllJoynAJMessage__STRING__U2__U4__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    AJ_Message                msg;
    CLR_RT_HeapBlock*         managedMsg = NULL;    
    CLR_RT_HeapBlock_String*  param1     = NULL;
    CLR_UINT16                param2; 
    CLR_UINT32                param3; 
    CLR_UINT32                param4; 
    AJ_Status                 status     = AJ_OK;

    managedMsg = stack.Arg1().Dereference();        FAULT_ON_NULL(managedMsg);                    
    param1     = stack.Arg2().DereferenceString();  FAULT_ON_NULL(param1);                    
    param2     = stack.Arg3().NumericByRef().u2;
    param3     = stack.Arg4().NumericByRef().u4;
    param4     = stack.Arg5().NumericByRef().u4;

    CopyFromManagedMsg(managedMsg, &msg);

    status = AJ_UnmarshalArgs( &msg, param1->StringText(), &param2, &param3, &param4 );
    
    if(status == AJ_OK) CopyToManagedMsg(managedMsg, &msg);
        
    stack.SetResult_I4( (CLR_INT32)status );
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::UnmarshalArgs___MicrosoftSPOTAllJoynAJStatus__MicrosoftSPOTAllJoynAJMessage__STRING__BYREF_U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        AJ_Message msg;
        CLR_RT_HeapBlock * managedMsg = stack.Arg1().Dereference();  FAULT_ON_NULL(managedMsg);                
        
        CopyFromManagedMsg(managedMsg, &msg);
        
        CLR_RT_HeapBlock_String *  param1 = stack.Arg2().DereferenceString();

        UINT32 * param2;
        UINT8 heapblock2[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT32_ByRef( stack, heapblock2, 3, param2 ) );

        AJ_Status status = AJ_UnmarshalArgs(&msg, param1->StringText(), param2);
        
        CopyToManagedMsg(managedMsg, &msg);
        
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT32( stack, status );

        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock2, 3 ) );
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::UnmarshalArgs___STRING__MicrosoftSPOTAllJoynAJMessage__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        AJ_Message msg;
        CLR_RT_HeapBlock * managedMsg = stack.Arg1().Dereference();  FAULT_ON_NULL(managedMsg);                
        
        CopyFromManagedMsg(managedMsg, &msg);

        CLR_RT_HeapBlock_String *  sig = stack.Arg2().DereferenceString();
        
        const char* data;
        AJ_UnmarshalArgs(&msg, sig->StringText(), &data);
        
        CopyToManagedMsg(managedMsg, &msg);
        
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_LPCSTR( stack, data );
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::UnmarshalArgs___MicrosoftSPOTAllJoynAJStatus__MicrosoftSPOTAllJoynAJMessage__STRING__BYREF_U4__BYREF_U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        AJ_Message msg;
        CLR_RT_HeapBlock * managedMsg = stack.Arg1().Dereference();  FAULT_ON_NULL(managedMsg);                
        
        CopyFromManagedMsg(managedMsg, &msg);

        CLR_RT_HeapBlock_String *  sig = stack.Arg2().DereferenceString();
        
        UINT32 * arg1;
        UINT8 heapblock2[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT32_ByRef( stack, heapblock2, 3, arg1 ) );

        UINT32 * arg2;
        UINT8 heapblock3[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT32_ByRef( stack, heapblock3, 4, arg2 ) );

        AJ_Status status = AJ_UnmarshalArgs( &msg, sig->StringText(), arg1, arg2 );
        TINYCLR_CHECK_HRESULT( hr );
        

        CopyToManagedMsg(managedMsg, &msg);
        
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock2, 3 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock3, 4 ) );
        
    stack.SetResult_I4( (CLR_INT32)status );
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::BusReplyAcceptSession___MicrosoftSPOTAllJoynAJStatus__MicrosoftSPOTAllJoynAJMessage__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 
    
    AJ_Message        msg;
    CLR_RT_HeapBlock* managedMsg = NULL;
    AJ_Status         status     = AJ_OK;
    CLR_UINT32        param1;

    managedMsg = stack.Arg1().Dereference();        FAULT_ON_NULL(managedMsg);                
    param1     = stack.Arg2().NumericByRef().u4;

    CopyFromManagedMsg(managedMsg, &msg);
    
    status = AJ_BusReplyAcceptSession(&msg, param1);
    
    if(status == AJ_OK) CopyToManagedMsg(managedMsg, &msg);
    
    stack.SetResult_I4( (CLR_INT32)status );
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::SendNotification___VOID__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    //
    // TODO: fix or remove 
    //
    
    //CLR_RT_HeapBlock* pMngObj = Interop_Marshal_RetrieveManagedObject( stack );
    //
    //      FAULT_ON_NULL(pMngObj);

    //    CLR_RT_HeapBlock_String * text = stack.Arg1().DereferenceString();


    //  textToSend[0].key   = lang1;
    // textToSend[0].value = text->StringText();
    // SendNotification(&BusInstance);

    //  TINYCLR_CHECK_HRESULT( hr );
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::BusHandleBusMessageInner___MicrosoftSPOTAllJoynAJStatus__MicrosoftSPOTAllJoynAJMessage( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    AJ_Message msg;
    AJ_Status status = AJ_OK;
    
    CLR_RT_HeapBlock* managedMsg = stack.Arg1().Dereference();  FAULT_ON_NULL(managedMsg);                        
    
    CopyFromManagedMsg(managedMsg, &msg);
    
    status = AJ_BusHandleBusMessage(&msg);
                   
    CopyToManagedMsg(managedMsg, &msg);
    
    stack.SetResult_I4( status );
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::CloseMsg___VOID__MicrosoftSPOTAllJoynAJMessage( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    AJ_Message        msg;
    CLR_RT_HeapBlock* managedMsg = NULL;

    managedMsg = stack.Arg1().Dereference();  FAULT_ON_NULL(managedMsg);                        
    
    CopyFromManagedMsg(managedMsg, &msg);
    
    AJ_CloseMsg( &msg );
    
    CopyToManagedMsg(managedMsg, &msg);

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::Disconnect___VOID__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    AJ_BusAttachment* bus = NULL;

    TINYCLR_CHECK_HRESULT( RetrieveBus( stack, bus ) ); 
    
    AJ_Disconnect( bus );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::Sleep___VOID__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    CLR_UINT32 timeout = stack.Arg1().NumericByRef().u4;

    AJ_Sleep( timeout );
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::AppMessageId___STATIC__U4__U4__U4__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_UINT32 param0 = stack.Arg0().NumericByRef().u4;
    CLR_UINT32 param1 = stack.Arg1().NumericByRef().u4;
    CLR_UINT32 param2 = stack.Arg2().NumericByRef().u4;
    
    CLR_UINT32 retVal = AJ_APP_MESSAGE_ID(param0, param1, param2);
    
    stack.SetResult_U4( retVal );
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::BusMessageId___STATIC__U4__U4__U4__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_UINT32 param0 = stack.Arg0().NumericByRef().u4;
    CLR_UINT32 param1 = stack.Arg1().NumericByRef().u4;
    CLR_UINT32 param2 = stack.Arg2().NumericByRef().u4;
    
    CLR_UINT32 retVal = AJ_BUS_MESSAGE_ID(param0, param1, param2);
    
    stack.SetResult_U4( retVal );
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::PrxMessageId___STATIC__U4__U4__U4__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_UINT32 param0 = stack.Arg0().NumericByRef().u4;
    CLR_UINT32 param1 = stack.Arg1().NumericByRef().u4;
    CLR_UINT32 param2 = stack.Arg2().NumericByRef().u4;
    
    CLR_UINT32 retVal = AJ_PRX_MESSAGE_ID(param0, param1, param2);
    
    stack.SetResult_U4( retVal );
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::AppPropertyId___STATIC__U4__U4__U4__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    CLR_UINT32 param0 = stack.Arg0().NumericByRef().u4;
    CLR_UINT32 param1 = stack.Arg1().NumericByRef().u4;
    CLR_UINT32 param2 = stack.Arg2().NumericByRef().u4;
    
    CLR_UINT32 retVal = AJ_APP_PROPERTY_ID(param0, param1, param2);

    stack.SetResult_U4( retVal );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::BusPropertyId___STATIC__U4__U4__U4__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    CLR_UINT32 param0 = stack.Arg0().NumericByRef().u4;
    CLR_UINT32 param1 = stack.Arg1().NumericByRef().u4;
    CLR_UINT32 param2 = stack.Arg2().NumericByRef().u4;
    
    CLR_UINT32 retVal = AJ_BUS_PROPERTY_ID(param0, param1, param2);

    stack.SetResult_U4( retVal );
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::PrxPropertyId___STATIC__U4__U4__U4__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    CLR_UINT32 param0 = stack.Arg0().NumericByRef().u4;
    CLR_UINT32 param1 = stack.Arg1().NumericByRef().u4;
    CLR_UINT32 param2 = stack.Arg2().NumericByRef().u4;
    
    CLR_UINT32 retVal = AJ_PRX_PROPERTY_ID(param0, param1, param2);

    stack.SetResult_U4( retVal );
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::CreateBus___VOID__BYREF_U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock hbParam0;    
    CLR_UINT32       param0;

    TINYCLR_CHECK_HRESULT( hbParam0.LoadFromReference( stack.Arg1( ) ) );

    param0 = (UINT32)&BusInstance; 

    hbParam0.SetInteger( param0 );        
    TINYCLR_CHECK_HRESULT( hbParam0.StoreToReference( stack.Arg1( ), 0 ));

    TINYCLR_NOCLEANUP();
}

// FIXME not implemented
HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::PrintXML___STATIC__VOID__STRING__STRING__U1__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    //
    // TODO: remove
    // 
    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());
    
    TINYCLR_NOCLEANUP();
}


HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::GetUniqueName___STRING__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 
    
    AJ_BusAttachment* bus    = NULL;    
    const char*             retVal = NULL; 

    TINYCLR_CHECK_HRESULT( RetrieveBus( stack, bus) );
    
    retVal = AJ_GetUniqueName(bus);
    
    stack.SetResult_String( retVal );
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::BusCancelSessionless___MicrosoftSPOTAllJoynAJStatus__U4__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    AJ_BusAttachment * bus   = NULL;    
    AJ_Status         status = AJ_OK;
    UINT32            messageId;

    TINYCLR_CHECK_HRESULT( RetrieveBus( stack, bus) );
    
    messageId = stack.Arg2().NumericByRef().u4;
        
    status = AJ_BusCancelSessionless( bus, messageId );
        
    stack.SetResult_I4( (CLR_INT32)status );
    
    TINYCLR_NOCLEANUP();
}

