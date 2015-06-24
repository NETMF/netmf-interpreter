////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#define AJ_MODULE SPOT_CLR_AJ

#include "spot_alljoyn.h"

extern AJ_Status MarshalDefaultProps(AJ_Message* msg);
extern AJ_Status MarshalObjectDescriptions(AJ_Message* msg);

#ifndef NDEBUG
uint8_t dbgSPOT_CLR_AJ = 0;
#endif

//--//

const int ARG_POOL_SIZE = 10;
const int MAX_PWD_LENGTH = 16;
const int MAX_HINT_LENGTH = 255;
const int MAX_CERT_LENGTH = 1024;
const int MAX_NUM_SECURITY_SUITES = 3;

//--//

static AJ_BusAttachment BusInstance;
static AJ_Arg           ArgPool[ARG_POOL_SIZE];
static bool             ArgInUse[ARG_POOL_SIZE] = {0};
static char             PwdText[MAX_PWD_LENGTH] = "";
static char             PskHint[MAX_HINT_LENGTH] = "";
static char             PskChar[MAX_CERT_LENGTH] = "";
static char             PskCert[MAX_CERT_LENGTH] = "";
static char             PemPrv[MAX_CERT_LENGTH] = "";
static char             PemX509[MAX_CERT_LENGTH] = "";

//--//

static CLR_UINT32 KeyExpiration = 0;
static ecc_privatekey Prv;
static X509CertificateChain* Chain = NULL;
static int SecuritySuites[MAX_NUM_SECURITY_SUITES] = {0};
static int NumSecuritySuites = 0;

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

void FreeInterfaceStorage( LPSTR iface[] )
{
    for ( int i=0; i<MAX_DIM_INTERFACE; i++ )
    {
        if ( iface[ i ] != NULL )
        {
            private_free( iface[ i ] );
            iface[ i ] = NULL;
        }
    }
}

// Storage for the interface string(s) is dynamically
// allocated. It must be freed using FreeInterfaceStorage
void DeserializeInterfaceString( LPCSTR data, LPSTR iface[] )
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
            //iface[i] = new char[size];
            iface[i] = ( char * ) private_malloc( size );

            for (int j=0; j<size; j++){
                iface[i][j] = cur[j];
            }
            iface[i][size-1] = '\0';
            
            i ++;            
            cur = p+1;
        }
        p ++;
    }
    iface[i - 1] = '\0';
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

void StartServiceCallback( void* context )
{
    DiscoveryContext* dc = (DiscoveryContext*)context;

    dc->_status = AJ_StartService( dc->_bus, dc->_pDaemonName, dc->_timeout, dc->_fConnected, dc->_port, dc->_serviceName, dc->_flags, NULL); 
}

static BOOL g_ajDiscoverStarted= FALSE;
HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::StartService___MicrosoftSPOTAllJoynAJStatus__U4__STRING__U4__I1__U2__STRING__U4( CLR_RT_StackFrame& stack )
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
    bool              fSignaled;
    OSTASK*           task        = NULL;
    DiscoveryContext* context     = NULL;
    AJ_Status         status      = AJ_OK;
 
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

        // has to set it longer than the AJ_CONNECT_TIMEOUT, otherwise it will timeout much quicker than the discovery time.
        // adding extra 300ms for extra factor.Extra 300 for overhead
        if (timeout < (AJ_CONNECT_TIMEOUT + 300))
            timeout = (AJ_CONNECT_TIMEOUT + 300);
           
        hbTimeout.SetInteger( timeout);        
        TINYCLR_CHECK_HRESULT(stack.SetupTimeout( hbTimeout, timeoutTicks ));
    }
    
    //
    // Push "state" onto the eval stac in the form of a OSTASK
    //
    if(stack.m_customState == 1)
    {   
        // there is StartService already started and not yet finished.
        // StartService/StartClient should be singleton 
        if (g_ajDiscoverStarted) 
        {
            TINYCLR_SET_AND_LEAVE( CLR_E_BUSY );
        }

        task    = (OSTASK*)private_malloc( sizeof(OSTASK) );        
        context = (DiscoveryContext*)private_malloc( sizeof(DiscoveryContext) ); 
        
        context->Initialize( bus, daemonName, timeout, fConnected, port, serviceName, flags ); 
        task   ->Initialize( StartServiceCallback,  context ); 
 
        //
        // we will keep track of task in our managed stack, context is attached to task
        //
        stack.PushValueAndClear(); 
        stack.m_evalStack[ 1 ].NumericByRef().u4 = (CLR_UINT32)task;
        stack.m_customState = 2;
        
        //
        // post to the underlying sub-system
        //
        // for cleaning the taskevent that may be set at previously.
        g_CLR_RT_ExecutionEngine.WaitEvents( stack.m_owningThread, *timeoutTicks, CLR_RT_ExecutionEngine::c_Event_OSTask, fSignaled );
        if (OSTASK_Post( task ) == FALSE)
             TINYCLR_SET_AND_LEAVE( CLR_E_FAIL );
        
        // when Task posted , set g_ajDiscoverStarted to TRUE;
        g_ajDiscoverStarted = TRUE; 
    }
        
    //
    // recover task and context instances
    //
    task    = (OSTASK*          )stack.m_evalStack[ 1 ].NumericByRef().u4;
    context = (DiscoveryContext*)task->GetArgument();
 
    //
    // wait for completion, fRes will tell us about timeout being expired
    //
    fSignaled = true;

    while(task->HasCompleted() == FALSE)
    {
        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.WaitEvents( stack.m_owningThread, *timeoutTicks, CLR_RT_ExecutionEngine::c_Event_OSTask, fSignaled ));

        // if timeout without c_Event_Otask, then the StartService was looped forever in unknown cause, 
        // has to kill the thread.
        if(fSignaled == false)
        {
            break;
        }
   }

    //
    // We either completed(with or without successfully established the conneciton) or timeout
    // 
 
    if ((fSignaled == false) && (!task->HasCompleted()))
    {
        // this is undesiable timeout that task thread is not time out and non-completed. 
        // we have to force a timeout, which may ends to an unrecoverable AJ-startService.
        status = AJ_ERR_TIMEOUT;
        
        //
        // timeout of waitEvent happened but not task completed, ie. the timeout of findBusandAttachement wasn't happened,
        // something goes very wrong.
        //
        // Should signal the task to kill itself, rather then kill it here.
        OSTASK_Cancel( task );
    }
    else 
    {
        status = context->_status;
        _ASSERTE( task->HasCompleted() ); 
        
    }

    // this task is now fully executed or foreced terminated, free it then.
    if( task->GetArgument() ) 
    {
        private_free( task->GetArgument() ); 
    }
    private_free( task );    
    
    stack.PopValue(); // task   
    stack.PopValue(); // Timeout
    stack.SetResult_I4( status );        

    //it is done.
    g_ajDiscoverStarted = FALSE; 

    TINYCLR_NOCLEANUP();
}


AJ_Status ClientConnectBus(AJ_BusAttachment* bus, LPCSTR daemonName, CLR_UINT32 timeout)
{
    AJ_Status   status = AJ_OK;
    CLR_UINT32  elapsed = 0;
    AJ_Time     timer;
        
    AJ_InitTimer(&timer);
    
    while (elapsed < timeout) {
        status = AJ_FindBusAndConnect(bus, daemonName, AJ_CONNECT_TIMEOUT);
        elapsed = AJ_GetElapsedTime(&timer, TRUE);
        if (status != AJ_OK) {
            elapsed += AJ_CONNECT_PAUSE;
            if (elapsed > timeout) {
                break;
            }
            AJ_WarnPrintf(("AJ_StartClient(): Failed to connect to bus, sleeping for %d seconds\n", AJ_CONNECT_PAUSE / 1000));
            AJ_Sleep(AJ_CONNECT_PAUSE);
            continue;
        }
        AJ_InfoPrintf(("AJ_StartClient(): AllJoyn client connected to bus\n"));
    }
    
    return status;
}


AJ_Status ClientFindService(AJ_BusAttachment* bus, LPCSTR name, const char ** interfaces, CLR_UINT32 timeout)
{
    AJ_Status status = AJ_OK;
    AJ_Time timer;
    char* rule;
    size_t ruleLen = 0;
    const char* base = "interface='org.alljoyn.About',sessionless='t'";
    const char* impl = ",implements='";
    const char** ifaces;

    uint32_t elapsed = 0;

    AJ_InitTimer(&timer);

    do { 
        if (name != NULL) {
            /*
             * Kick things off by finding the service names
             */
            status = AJ_BusFindAdvertisedName(bus, name, AJ_BUS_START_FINDING);
            debug_printf("AJ_StartClient(): AJ_BusFindAdvertisedName()\n");
        } else {
            /*
             * Kick things off by registering for the Announce signal.
             * Optionally add the implements clause per given interface
             */
            if (ruleLen == 0)
            {
                ruleLen = hal_strlen_s(base) + 1;
                if (interfaces != NULL) {
                    ifaces = interfaces;
                    while (*ifaces != NULL) {
                        ruleLen += hal_strlen_s(impl) + hal_strlen_s(*ifaces) + 1;
                        ifaces++;
                    }
                }
                rule = (char*) AJ_Malloc(ruleLen);
                if (rule == NULL) {
                    status = AJ_ERR_RESOURCES;
                    break;
                }
                hal_strcpy_s(rule, hal_strlen_s(base), base);
                if (interfaces != NULL) {
                    ifaces = interfaces;
                    while (*ifaces != NULL) {
                        strcat(rule, impl);
                        if ((*ifaces)[0] == '$') {
                            strcat(rule, &(*ifaces)[1]);
                        } else {
                            strcat(rule, *ifaces);
                        }
                        strcat(rule, "'");
                        ifaces++;
                    }
                }
            }
            status = AJ_BusSetSignalRule(bus, rule, AJ_BUS_SIGNAL_ALLOW);
            AJ_InfoPrintf(("AJ_StartClient(): Client SetSignalRule: %s\n", rule));
            AJ_Free(rule);
        }

        if (status == AJ_OK) {
            break;
        }
        
        elapsed = AJ_GetElapsedTime(&timer, TRUE);
    }while(elapsed < timeout) ;

    return status;
}


struct ConnectBusDiscoveryContext
{
    AJ_BusAttachment*   _bus; 
    char                _daemonName[ AJ_MAX_SERVICE_NAME_SIZE ];
    char *              _pDaemonName;
    CLR_INT32           _timeout;
    AJ_Status           _status;


    void Initialize( AJ_BusAttachment* bus, LPCSTR daemonName, CLR_INT32 timeout ) 
    {
        _bus = bus; 
        _pDaemonName = NULL;
        if ( daemonName )
        {
            hal_strcpy_s( _daemonName, sizeof(_daemonName), daemonName );
            _pDaemonName = _daemonName;
        }
        _timeout = timeout;

        _status = AJ_OK;
        
    }
    
};

AJ_Status AJ_StartClientConnectBus(AJ_BusAttachment* bus,
                               const char* daemonName,
                               uint32_t timeout )
{
    AJ_Status status = AJ_OK;
    AJ_Time timer;
    uint32_t elapsed = 0;
    BOOL connected = FALSE;

    AJ_InitTimer(&timer);
    
    while (!connected) {
        status = AJ_FindBusAndConnect(bus, daemonName, AJ_CONNECT_TIMEOUT);
        elapsed = AJ_GetElapsedTime(&timer, TRUE);
        if (status != AJ_OK) {
            elapsed += AJ_CONNECT_PAUSE;
            if (elapsed > timeout) {
                status =  AJ_ERR_TIMEOUT;
                debug_printf(("AJ_StartClientConnectBus(): Client disconnecting from bus: status=%s.\n", AJ_StatusText(status)));
                AJ_Disconnect(bus);
                break;
            }
            //AJ_WarnPrintf(("AJ_StartClient(): Failed to connect to bus, sleeping for %d seconds\n", AJ_CONNECT_PAUSE / 1000));
            AJ_Sleep(AJ_CONNECT_PAUSE);
            continue;
        }else{
            debug_printf("AJ_StartClientConnectBus(): connected\n");
            connected = TRUE;
        }
    }
    return status;
}
void StartClientConnectBusCallback( void* context )
{
    ConnectBusDiscoveryContext* dc = (ConnectBusDiscoveryContext*)context;
    dc->_status = AJ_StartClientConnectBus( dc->_bus, dc->_pDaemonName, dc->_timeout );
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::ClientConnectBus___MicrosoftSPOTAllJoynAJStatus__U4__STRING__U4( CLR_RT_StackFrame& stack )
{    
    TINYCLR_HEADER();
    
    AJ_BusAttachment* bus         = NULL;    
    AJ_Status         status      = AJ_OK;
    LPCSTR            daemonName  = NULL;
    CLR_UINT32        timeout; 
    
    CLR_INT64*        timeoutTicks;
    bool              fSignaled;
    OSTASK*           task        = NULL;
    ConnectBusDiscoveryContext* context = NULL;

    
    TINYCLR_CHECK_HRESULT( RetrieveBus( stack, bus ) );
    
    daemonName  = stack.Arg2().RecoverString( );
    timeout     = stack.Arg3().NumericByRef().u4;

    if( daemonName && hal_strlen_s( daemonName ) > AJ_MAX_SERVICE_NAME_SIZE )
    {
        TINYCLR_SET_AND_LEAVE( CLR_E_INVALID_PARAMETER );
    }

    {
        CLR_RT_HeapBlock hbTimeout;

        // has to set it longer than the AJ_CONNECT_TIMEOUT, otherwise it will timeout much quicker than the discovery time.
        // adding extra 300ms for extra factor.Extra 300 for overhead
        if (timeout < (AJ_CONNECT_TIMEOUT + 300))
            timeout = (AJ_CONNECT_TIMEOUT + 300);
           
        hbTimeout.SetInteger( timeout );        
        TINYCLR_CHECK_HRESULT(stack.SetupTimeout( hbTimeout, timeoutTicks ));
    }
     //
     // Push "state" onto the eval stac in the form of a OSTASK
     //
     if(stack.m_customState == 1)
     {   
         // there is StartService already started and not yet finished.
         // StartService/StartClient should be singleton 
         if (g_ajDiscoverStarted) 
         {
             TINYCLR_SET_AND_LEAVE( CLR_E_BUSY );
         }

         task    = (OSTASK*)private_malloc( sizeof(OSTASK) );        
         context = (ConnectBusDiscoveryContext*)private_malloc( sizeof(ConnectBusDiscoveryContext) ); 
         
         context->Initialize( bus, daemonName, timeout );
         task   ->Initialize( StartClientConnectBusCallback,  context ); 

         //
         // we will keep track of task in our managed stack, context is attached to task
         //
         stack.PushValueAndClear(); 
         stack.m_evalStack[ 1 ].NumericByRef().u4 = (CLR_UINT32)task;
         stack.m_customState = 2;
         
         //
         // post to the underlying sub-system
         //
         // for cleaning the taskevent that may be set at previously.
         g_CLR_RT_ExecutionEngine.WaitEvents( stack.m_owningThread, *timeoutTicks, CLR_RT_ExecutionEngine::c_Event_OSTask, fSignaled );
         if (OSTASK_Post( task ) == FALSE)
              TINYCLR_SET_AND_LEAVE( CLR_E_FAIL );
         
         // when Task posted , set g_ajDiscoverStarted to TRUE;
         g_ajDiscoverStarted = TRUE; 
     }
         
     //
     // recover task and context instances
     //
     task    = (OSTASK*          )stack.m_evalStack[ 1 ].NumericByRef().u4;
     context = (ConnectBusDiscoveryContext*)task->GetArgument();

     //
     // wait for completion, fRes will tell us about timeout being expired
     //
     fSignaled = true;

     while(task->HasCompleted() == FALSE)
     {
         TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.WaitEvents( stack.m_owningThread, *timeoutTicks, CLR_RT_ExecutionEngine::c_Event_OSTask, fSignaled ));

         // if timeout without c_Event_Otask, then the StartService was looped forever in unknown cause, 
         // has to kill the thread.
         if(fSignaled == false)
         {
             break;
         }
     }

     //
     // We either completed(with or without successfully established the conneciton) or timeout
     // 
    if ((fSignaled == false) && (!task->HasCompleted()))
    {
         // this is undesiable timeout that task thread is not time out and non-completed. 
         // we have to force a timeout, which may ends to an unrecoverable AJ-startService.
         status = AJ_ERR_TIMEOUT;

         //
         // timeout of waitEvent happened but not task completed, ie. the timeout of findBusandAttachement wasn't happened,
         // something goes very wrong.
         //
         // Should signal the task to kill itself, rather then kill it here.
         OSTASK_Cancel( task );
     }
     else 
     {
         status = context->_status;
         _ASSERTE( task->HasCompleted() ); 
     }

      
     // this task is now fully executed or foreced terminated, free it then.
     if( task->GetArgument() ) 
     {
         private_free( task->GetArgument() ); 
     }
     private_free( task );    
     
     stack.PopValue(); // task   
     stack.PopValue(); // Timeout
     stack.SetResult_I4( status ); 
     
     //it is done.
     g_ajDiscoverStarted = FALSE; 
     
     TINYCLR_NOCLEANUP();

}
//--//
AJ_Status AJ_startClientFindService(AJ_BusAttachment* bus, const char* name, const char** interfaces,  uint32_t timeout )
{
    
    AJ_Status status = AJ_OK;
    AJ_Time timer;
    char* rule;
    size_t ruleLen = 0;
    const char* base = "interface='org.alljoyn.About',sessionless='t'";
    const char* impl = ",implements='";
    const char** ifaces;

    uint32_t elapsed = 0;

    AJ_InitTimer(&timer);

    do { 
        if (name != NULL) {
            /*
             * Kick things off by finding the service names
             */
            status = AJ_BusFindAdvertisedName(bus, name, AJ_BUS_START_FINDING);
            debug_printf("AJ_StartClient(): AJ_BusFindAdvertisedName()\n");
        } else {
            /*
             * Kick things off by registering for the Announce signal.
             * Optionally add the implements clause per given interface
             */
            if (ruleLen == 0)
            {
                ruleLen = hal_strlen_s(base) + 1;
                if (interfaces != NULL) {
                    ifaces = interfaces;
                    while (*ifaces != NULL) {
                        ruleLen += hal_strlen_s(impl) + hal_strlen_s(*ifaces) + 1;
                        ifaces++;
                    }
                }
                rule = (char*) AJ_Malloc(ruleLen);
                if (rule == NULL) {
                    status = AJ_ERR_RESOURCES;
                    break;
                }
                hal_strcpy_s(rule, hal_strlen_s(base), base);
                if (interfaces != NULL) {
                    ifaces = interfaces;
                    while (*ifaces != NULL) {
                        strcat(rule, impl);
                        if ((*ifaces)[0] == '$') {
                            strcat(rule, &(*ifaces)[1]);
                        } else {
                            strcat(rule, *ifaces);
                        }
                        strcat(rule, "'");
                        ifaces++;
                    }
                }
            }
            status = AJ_BusSetSignalRule(bus, rule, AJ_BUS_SIGNAL_ALLOW);
            AJ_InfoPrintf(("AJ_StartClient(): Client SetSignalRule: %s\n", rule));
            AJ_Free(rule);
        }

        if (status == AJ_OK) {
            break;
        }
        
        elapsed = AJ_GetElapsedTime(&timer, TRUE);
    }while(elapsed < timeout) ;

    return status;
}


HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::ClientFindServiceInner___MicrosoftSPOTAllJoynAJStatus__U4__STRING__STRING__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    AJ_BusAttachment * bus      = NULL;    
    LPCSTR name                 = NULL;
    LPCSTR ifaces               = NULL;
    CLR_UINT32 timeout          = 0;

    static char * interfaces[MAX_DIM_INTERFACE] = {0,};

    TINYCLR_CHECK_HRESULT( RetrieveBus( stack, bus ) );

    name        = stack.Arg2( ).RecoverString( );
    ifaces      = stack.Arg3( ).RecoverString( );
    timeout     = stack.Arg4( ).NumericByRef( ).u4;

    if (ifaces != NULL)
    {
        FreeInterfaceStorage( interfaces ); // clear out previous interfaces
        DeserializeInterfaceString( ifaces, interfaces );
    }

    AJ_Status status = ClientFindService( bus, name, (const char**)interfaces, timeout );
    SetResult_INT32( stack, status );

    TINYCLR_NOCLEANUP();
}

//--//

AJ_Status ClientConnectService( AJ_BusAttachment * bus, 
                                CLR_UINT32 timeout,
                                LPSTR serviceName,
                                CLR_UINT16 port,
                                CLR_UINT32 * sessionId,
                                AJ_SessionOpts * opts,
                                LPSTR fullName)
{
    bool clientStarted      = false;
    bool found              = false;
    AJ_Status status        = AJ_OK;
           
    *sessionId = 0;
    if (serviceName != NULL) {
        *serviceName = '\0';
    }

    uint32_t elapsed = 0;
    AJ_Time timer;
    
    AJ_InitTimer(&timer);

    while (!clientStarted && (status == AJ_OK) ) {
        AJ_Message msg;
        status = AJ_UnmarshalMsg(bus, &msg, AJ_UNMARSHAL_TIMEOUT);

        if ((status == AJ_ERR_TIMEOUT) && !found) {
            /*
             * Timeouts are expected until we find a name or service
             */
             
            elapsed = AJ_GetElapsedTime(&timer, TRUE);
            if ((timeout- elapsed) < AJ_UNMARSHAL_TIMEOUT) {
                return status;
            }
//            timeout -= AJ_UNMARSHAL_TIMEOUT;
            status = AJ_OK;
            continue;
        }
        if (status == AJ_ERR_NO_MATCH) {
            // Ignore unknown messages
            status = AJ_OK;
            continue;
        }
        if (status != AJ_OK) {
            debug_printf("AJ_StartClient(): status=%s\n", AJ_StatusText(status));
            break;
        }
        switch (msg.msgId) {

        case AJ_REPLY_ID(AJ_METHOD_FIND_NAME):
        case AJ_REPLY_ID(AJ_METHOD_FIND_NAME_BY_TRANSPORT):
            if (msg.hdr->msgType == AJ_MSG_ERROR) {
                debug_printf("AJ_StartClient(): AJ_METHOD_FIND_NAME: %s\n", msg.error);
                status = AJ_ERR_FAILURE;
            } else {
                uint32_t disposition;
                AJ_UnmarshalArgs(&msg, "u", &disposition);
                if ((disposition != AJ_FIND_NAME_STARTED) && (disposition != AJ_FIND_NAME_ALREADY)) {
                    debug_printf("AJ_StartClient(): AJ_ERR_FAILURE\n");
                    status = AJ_ERR_FAILURE;
                }
            }
            break;

        case AJ_SIGNAL_FOUND_ADV_NAME:
            {
                AJ_Arg arg;
                AJ_UnmarshalArg(&msg, &arg);
                debug_printf("FoundAdvertisedName(%s)\n", arg.val.v_string);
                if (!found) {
                    if (fullName) {
                        hal_strcpy_s( fullName, AJ_MAX_SERVICE_NAME_SIZE, arg.val.v_string );
                        fullName[arg.len] = '\0';
                    }
                    found = TRUE;
                    status = AJ_BusJoinSession(bus, arg.val.v_string, port, opts);
                }
            }
            break;

        case AJ_SIGNAL_ABOUT_ANNOUNCE:
            {
                uint16_t aboutVersion, aboutPort;
#ifdef ANNOUNCE_BASED_DISCOVERY
                status = AJ_AboutHandleAnnounce(&msg, &aboutVersion, &aboutPort, serviceName, &found);
                if (interfaces != NULL) {
                    found = TRUE;
                }
                if ((status == AJ_OK) && (found == TRUE)) {
                    debug_printf("AJ_StartClient(): AboutAnnounce from (%s) About Version: %d Port: %d\n", msg.sender, aboutVersion, aboutPort);
#else
                debug_printf("AJ_StartClient(): AboutAnnounce from (%s)\n", msg.sender);
                if (!found) {
                    found = TRUE;
                    AJ_UnmarshalArgs(&msg, "qq", &aboutVersion, &aboutPort);
                    if (serviceName != NULL) {
                        hal_strcpy_s( serviceName, AJ_MAX_SERVICE_NAME_SIZE, msg.sender );
                        serviceName[AJ_MAX_NAME_SIZE] = '\0';
                    }
#endif
                    /*
                     * Establish a session with the provided port.
                     * If port value is 0 use the About port unmarshalled from the Announcement instead.
                     */
                    if (port == 0) {
                        status = AJ_BusJoinSession(bus, msg.sender, aboutPort, opts);
                    } else {
                        status = AJ_BusJoinSession(bus, msg.sender, port, opts);
                    }
                    if (status != AJ_OK) {
                        debug_printf("AJ_StartClient(): BusJoinSession failed (%s)\n", AJ_StatusText(status));
                    }
                }
            }
            break;

        case AJ_REPLY_ID(AJ_METHOD_JOIN_SESSION):
            {
                uint32_t replyCode;

                if (msg.hdr->msgType == AJ_MSG_ERROR) {
                    debug_printf("AJ_StartClient(): AJ_METHOD_JOIN_SESSION: %s\n", msg.error);
                    status = AJ_ERR_FAILURE;
                } else {
                    status = AJ_UnmarshalArgs(&msg, "uu", &replyCode, sessionId);
                    if (replyCode == AJ_JOINSESSION_REPLY_SUCCESS) {
                        clientStarted = TRUE;
                    } else {
                        debug_printf("AJ_StartClient(): AJ_METHOD_JOIN_SESSION reply (%d)\n", replyCode);
                        status = AJ_ERR_FAILURE;
                    }
                }
            }
            break;

        case AJ_SIGNAL_SESSION_LOST_WITH_REASON:
            /*
             * Force a disconnect
             */
            {
                uint32_t id, reason;
                AJ_UnmarshalArgs(&msg, "uu", &id, &reason);
                AJ_InfoPrintf(("Session lost. ID = %u, reason = %u", id, reason));
            }
            debug_printf("AJ_StartClient(): AJ_SIGNAL_SESSION_LOST_WITH_REASON: AJ_ERR_READ\n");
            status = AJ_ERR_READ;
            break;

        default:
            /*
             * Pass to the built-in bus message handlers
             */
            debug_printf("AJ_StartClient(): AJ_BusHandleBusMessage()\n");
            status = AJ_BusHandleBusMessage(&msg);
            break;
        }
        AJ_CloseMsg(&msg);
        
        elapsed = AJ_GetElapsedTime(&timer, TRUE);
        if (elapsed >=timeout){
            status = AJ_ERR_NO_MATCH;
        debug_printf("Timeout %d, %d\r\n", elapsed, timeout);
            break;
        }
    }

        
    if (status != AJ_OK) {
        debug_printf("AJ_StartClient(): Client disconnecting from bus: status=%s\n", AJ_StatusText(status));
//        AJ_Disconnect(bus);
    }
    return status;
}


struct ConnectServiceDiscoveryContext
{

    typedef Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ_SessionOpts Managed_AJ_SessionOpts;

    AJ_BusAttachment*   _bus; 
    CLR_INT32           _timeout;
    CLR_UINT16          _port;
    char                _serviceName[ AJ_MAX_SERVICE_NAME_SIZE ]; 
    CLR_UINT32          _sessionId;
    AJ_SessionOpts      _opts;
    AJ_SessionOpts *    _pOpts;
    
    char                _fullServiceName[AJ_MAX_SERVICE_NAME_SIZE];
    char  *             _pFullName; 
    char**              _pInterfaces;
    AJ_Status           _status;


    void Initialize( AJ_BusAttachment* bus, CLR_INT32 timeout,  LPCSTR serviceName, CLR_UINT16 port, 
                    CLR_UINT32 sessionId, char** interfaces, CLR_RT_HeapBlock* pOpts, LPCSTR fullName  )
    {
        _bus = bus; 
        _timeout = timeout;
        _port = port;
        hal_strcpy_s( _serviceName, sizeof(_serviceName), serviceName ); 

        _sessionId = sessionId;
        
        for(int i =0; i <AJ_MAX_SERVICE_NAME_SIZE; i++)
            _fullServiceName[i] = 0;

        if (fullName)
            _pFullName = _fullServiceName;
        else
            _pFullName = NULL; 

        if( NULL != pOpts )
        {
            _opts.traffic      =  pOpts[ Managed_AJ_SessionOpts::FIELD__traffic      ].NumericByRef().u1;
            _opts.proximity    =  pOpts[ Managed_AJ_SessionOpts::FIELD__proximity    ].NumericByRef().u1;
            _opts.transports   =  pOpts[ Managed_AJ_SessionOpts::FIELD__transports   ].NumericByRef().u2;
            _opts.isMultipoint =  pOpts[ Managed_AJ_SessionOpts::FIELD__isMultipoint ].NumericByRef().u4;
            _pOpts = &_opts;
        }else
            _pOpts = NULL;

        _pInterfaces = interfaces;

        _status = AJ_OK;
        
    }
    
};

void StartClientConnectServiceCallback( void* context )
{
    ConnectServiceDiscoveryContext* dc = (ConnectServiceDiscoveryContext*)context;
    CLR_Debug::Printf("start Client ConnectBus\r\n");
    dc->_status = ClientConnectService( dc->_bus, dc->_timeout, dc->_serviceName, dc->_port, &dc->_sessionId, dc->_pOpts, dc->_pFullName); 
}


#if 0
HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::ClientConnectService___MicrosoftSPOTAllJoynAJStatus__U4__U4__STRING__U2__BYREF_U4__MicrosoftSPOTAllJoynAJSessionOpts__BYREF_STRING( CLR_RT_StackFrame& stack )
{
    typedef Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ_SessionOpts Managed_AJ_SessionOpts;
    
    TINYCLR_HEADER();

    char              fullServiceName[AJ_MAX_SERVICE_NAME_SIZE] = "";
    char              cliName[AJ_MAX_SERVICE_NAME_SIZE] = "";
    CLR_RT_HeapBlock  hbFullName;
    LPSTR             fullName     = NULL;
    CLR_RT_HeapBlock  hbSessionId;
    CLR_UINT32        sessionId = 0;    
    CLR_RT_HeapBlock* managedOpts = NULL;
    AJ_SessionOpts    opts = {0};
    AJ_BusAttachment* bus         = NULL;    
    AJ_Status         status      = AJ_OK;
    CLR_UINT32        timeout; 
    LPCSTR            clientName  = NULL;
    CLR_UINT16        port;    
    
    TINYCLR_CHECK_HRESULT( RetrieveBus( stack, bus ) );
    
    timeout     = stack.Arg2( ).NumericByRef( ).u4;
    clientName  = stack.Arg3( ).RecoverString( );
    port        = stack.Arg4( ).NumericByRef( ).s2;
    
    if ( clientName != NULL)
    {
        hal_strcpy_s( cliName, sizeof(cliName), clientName );
    }
    
    TINYCLR_CHECK_HRESULT( hbSessionId.LoadFromReference( stack.Arg5( ) ) );
    sessionId = hbSessionId.NumericByRef( ).u4;    
           
    managedOpts = stack.Arg6().Dereference( );
    
    if( NULL != managedOpts )
    {
        opts.traffic      =  managedOpts[ Managed_AJ_SessionOpts::FIELD__traffic      ].NumericByRef( ).u1;
        opts.proximity    =  managedOpts[ Managed_AJ_SessionOpts::FIELD__proximity    ].NumericByRef( ).u1;
        opts.transports   =  managedOpts[ Managed_AJ_SessionOpts::FIELD__transports   ].NumericByRef( ).u2;
        opts.isMultipoint =  managedOpts[ Managed_AJ_SessionOpts::FIELD__isMultipoint ].NumericByRef( ).u4;
    }

    hbFullName.LoadFromReference( stack.Arg7( ) );
    fullName = (LPSTR)hbFullName.RecoverString( );

    status = ClientConnectService( bus,
                                   timeout,
                                   cliName,
                                   port,
                                   &sessionId,
                                   managedOpts == NULL ? NULL : &opts,
                                   fullName == NULL ? NULL : fullServiceName );
        
    if( status == AJ_OK )
    {     
        hbSessionId.SetInteger( sessionId );
        TINYCLR_CHECK_HRESULT( hbSessionId.StoreToReference( stack.Arg5( ), 0 ) );   
        
        if(fullName)
        {
            TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_String::CreateInstance( hbFullName, fullServiceName ));
        }
        else
        {
            hbFullName.SetObjectReference( NULL );
        }
        
        TINYCLR_CHECK_HRESULT( hbFullName.StoreToReference( stack.Arg7( ), 0 ) );
    }

    stack.SetResult_I4( (CLR_INT32)status );
    
    TINYCLR_NOCLEANUP();
}

#endif

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::ClientConnectService___MicrosoftSPOTAllJoynAJStatus__U4__U4__STRING__U2__BYREF_U4__MicrosoftSPOTAllJoynAJSessionOpts__BYREF_STRING( CLR_RT_StackFrame& stack )
{
    
    TINYCLR_HEADER();

    char              fullServiceName[AJ_MAX_SERVICE_NAME_SIZE] = "";
    CLR_RT_HeapBlock  hbFullName;
    LPSTR             fullName     = NULL;
    CLR_RT_HeapBlock  hbSessionId;
    CLR_UINT32        sessionId = 0;    
    CLR_RT_HeapBlock* managedOpts = NULL;
    AJ_BusAttachment* bus         = NULL;    
    AJ_Status         status      = AJ_OK;
    CLR_UINT32        timeout; 
    LPCSTR            serviceName  = NULL;
    CLR_UINT16        port;

    CLR_INT64*        timeoutTicks;
    bool              fSignaled;
    OSTASK*           task        = NULL;
    ConnectServiceDiscoveryContext* context = NULL;
    
    
    TINYCLR_CHECK_HRESULT( RetrieveBus( stack, bus ) );
    
    timeout     = stack.Arg2( ).NumericByRef( ).u4;
    serviceName = stack.Arg3( ).RecoverString( ); // bad
    if( hal_strlen_s( serviceName ) > AJ_MAX_SERVICE_NAME_SIZE )
    {
        TINYCLR_SET_AND_LEAVE( CLR_E_INVALID_PARAMETER );
    }

    port        = stack.Arg4( ).NumericByRef( ).s2;

    TINYCLR_CHECK_HRESULT( hbSessionId.LoadFromReference( stack.Arg5( ) ) );
    sessionId = hbSessionId.NumericByRef( ).u4;    
    managedOpts = stack.Arg6().Dereference( );

    hbFullName.LoadFromReference( stack.Arg7( ) );
    fullName = (LPSTR)hbFullName.RecoverString( );

    {
        CLR_RT_HeapBlock hbTimeout;

        // has to set it longer than the AJ_CONNECT_TIMEOUT, otherwise it will timeout much quicker than the discovery time.
        // adding extra 300ms for extra factor.Extra 300 for overhead
        if (timeout < (AJ_UNMARSHAL_TIMEOUT + 300))
            timeout = (AJ_UNMARSHAL_TIMEOUT + 300);
           
        hbTimeout.SetInteger( timeout + 300);        
        TINYCLR_CHECK_HRESULT(stack.SetupTimeout( hbTimeout, timeoutTicks ));
    }

     //
     // Push "state" onto the eval stac in the form of a OSTASK
     //
     if(stack.m_customState == 1)
     {   
debug_printf(" in start ClientConnect service  1\r\n");


         task    = (OSTASK*)private_malloc( sizeof(OSTASK) );        
         context = (ConnectServiceDiscoveryContext*)private_malloc( sizeof(ConnectServiceDiscoveryContext) ); 
         
         context->Initialize( bus, timeout, serviceName, port, sessionId,  NULL, managedOpts, fullName ); 
         task   ->Initialize( StartClientConnectServiceCallback,  context ); 


         //
         // we will keep track of task in our managed stack, context is attached to task
         //
         stack.PushValueAndClear(); 
         stack.m_evalStack[ 1 ].NumericByRef().u4 = (CLR_UINT32)task;
         stack.m_customState = 2;
         
    //     Events_Clear(SYSTEM_EVENT_FLAG_OSTASK_TIMEOUT);
         //
         // post to the underlying sub-system
         //
         // for cleaning the taskevent that may be set at previously.
         g_CLR_RT_ExecutionEngine.WaitEvents( stack.m_owningThread, *timeoutTicks, CLR_RT_ExecutionEngine::c_Event_OSTask, fSignaled );
         if (OSTASK_Post( task ) == FALSE)
              TINYCLR_SET_AND_LEAVE( CLR_E_FAIL );
         
     }
         
     //
     // recover task and context instances
     //
     task    = (OSTASK*          )stack.m_evalStack[ 1 ].NumericByRef().u4;
     context = (ConnectServiceDiscoveryContext*)task->GetArgument();

     //
     // wait for completion, fRes will tell us about timeout being expired
     //
     fSignaled = true;

     while(task->HasCompleted()== FALSE)
     {
         TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.WaitEvents( stack.m_owningThread, *timeoutTicks, CLR_RT_ExecutionEngine::c_Event_OSTask, fSignaled ));

         // if timeout without c_Event_Otask, then the StartService was looped forever in unknown cause, 
         // has to kill the thread.
         if(fSignaled == false)
         {
//         Events_Set(SYSTEM_EVENT_FLAG_OSTASK_TIMEOUT);
//             break;
         }
     }

     //
     // We either completed(with or without successfully established the conneciton) or timeout
     // 
debug_printf(" wait event completed.\r\n");
   
    if ((fSignaled == false) && (!task->HasCompleted()))
    {
         // this is undesiable timeout that task thread is not time out and non-completed. 
         // we have to force a timeout, which may ends to an unrecoverable AJ-startService.
         status = AJ_ERR_TIMEOUT;

         //
         // timeout of waitEvent happened but not task completed, ie. the timeout of findBusandAttachement wasn't happened,
         // something goes very wrong.
         //
         // Should signal the task to kill itself, rather then kill it here.
         OSTASK_Cancel( task );
     }
     else 
     {
         status = context->_status;
         _ASSERTE( task->HasCompleted() ); 
         
         if( status == AJ_OK )
         {     
             hbSessionId.SetInteger( context->_sessionId  );
             TINYCLR_CHECK_HRESULT( hbSessionId.StoreToReference( stack.Arg5( ), 0 ) );   
             
             if(context->_pFullName)
             {
                 TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_String::CreateInstance( hbFullName, context->_fullServiceName ));
             }
             else
             {
                 hbFullName.SetObjectReference( NULL );
             }
             TINYCLR_CHECK_HRESULT( hbFullName.StoreToReference( stack.Arg7( ), 0 ) );
         }
         
     }

    CLR_Debug::Printf(" status %d, id %x, full name %s \r\n", status,  context->_sessionId, context->_fullServiceName);
      
     // this task is now fully executed or foreced terminated, free it then.
     if( task->GetArgument() ) 
     {
         private_free( task->GetArgument() ); 
     }
     private_free( task );    

     stack.PopValue(); // task   
     stack.PopValue(); // Timeout
     stack.SetResult_I4( status ); 
     
     //it is done.
     g_ajDiscoverStarted = FALSE; 
     
     TINYCLR_NOCLEANUP();

//////////

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



///---////
static AJ_Status AuthListenerCallback(CLR_UINT32 authmechanism, CLR_UINT32 command, AJ_Credential * cred)
{
    AJ_Status status = AJ_ERR_INVALID;
    X509CertificateChain* node;

    CLR_Debug::Printf("AuthListenerCallback authmechanism %d command %d\n", authmechanism, command);

    switch (authmechanism) {
    case AUTH_SUITE_ECDHE_NULL:
        cred->expiration = KeyExpiration;
        status = AJ_OK;
        break;
    //------------------------------
        
    case AUTH_SUITE_ECDHE_PSK:
        switch (command) {
        case AJ_CRED_PUB_KEY:
            cred->data = (uint8_t*) PskHint;
            cred->len = hal_strlen_s(PskHint);
            cred->expiration = KeyExpiration;
            status = AJ_OK;
            break;

        case AJ_CRED_PRV_KEY:
            cred->data = (uint8_t*) PskChar;
            cred->len = hal_strlen_s(PskChar);
            cred->expiration = KeyExpiration;
            status = AJ_OK;
            break;
        }
        break;
    //------------------------------
    
    case AUTH_SUITE_ECDHE_ECDSA:
        switch (command) {
        case AJ_CRED_PRV_KEY:
            cred->len = sizeof (ecc_privatekey);
            status = AJ_DecodePrivateKeyPEM(&Prv, PemPrv);
            if (AJ_OK != status) {
                return status;
            }
            cred->data = (uint8_t*) &Prv;
            cred->expiration = KeyExpiration;
            break;

        case AJ_CRED_CERT_CHAIN:
            switch (cred->direction) {
            case AJ_CRED_REQUEST:
                // Free previous certificate chain
                while (Chain) {
                    node = Chain;
                    Chain = Chain->next;
                    AJ_Free(node->certificate.der.data);
                    AJ_Free(node);
                }
                Chain = AJ_X509DecodeCertificateChainPEM(PemX509);
                if (NULL == Chain) {
                    return AJ_ERR_INVALID;
                }
                cred->data = (uint8_t*) Chain;
                cred->expiration = KeyExpiration;
                status = AJ_OK;
                break;

            case AJ_CRED_RESPONSE:
                node = (X509CertificateChain*) cred->data;
                while (node) {
                    AJ_DumpBytes("CERTIFICATE", node->certificate.der.data, node->certificate.der.size);
                    node = node->next;
                }
                status = AJ_OK;
                break;
            }
            break;
        }
        break;
    //------------------------------
    
    default:
        break;
    //------------------------------
    }
    return status;
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

void AuthCallback(const void* context, AJ_Status status)
{
    *((AJ_Status*)context) = status;
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::UsePeerAuthentication___VOID__BOOLEAN( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    bool usePeerAuth  = stack.Arg1().NumericByRef().s1 != 0;    
    
    if (usePeerAuth)
    {
        AJ_BusSetPasswordCallback( &BusInstance, PasswordCallback );
    }
    else
    {
        AJ_BusSetPasswordCallback( &BusInstance, NULL );
    }

    TINYCLR_NOCLEANUP();
}


HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::EnableSecurity___MicrosoftSPOTAllJoynAJStatus__U4__SZARRAY_I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {        
        
        AJ_BusAttachment* bus                       = NULL;    
        CLR_RT_HeapBlock_Array * securitySuites     = NULL;
        
        TINYCLR_CHECK_HRESULT( RetrieveBus( stack, bus ) );        
        securitySuites = stack.Arg2().DereferenceArray() ;  FAULT_ON_NULL(securitySuites);   
        
        NumSecuritySuites = 0;
        for(int i=0; i<MAX_NUM_SECURITY_SUITES; i ++)
        {
            SecuritySuites[i] = 0;
        }
        
        NumSecuritySuites = securitySuites->m_numOfElements;
        for(int i=0; i<securitySuites->m_numOfElements; i ++)
        {
            SecuritySuites[i] = * (CLR_UINT32 *) securitySuites->GetElement(i);
        }
        
        AJ_Status status = AJ_BusEnableSecurity(bus, (const CLR_UINT32 *)SecuritySuites, NumSecuritySuites);
        
        AJ_BusSetAuthListenerCallback(bus, AuthListenerCallback);
        
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT32( stack, status );
        
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::ClearCredentials___MicrosoftSPOTAllJoynAJStatus( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        INT32 retVal = AJ_ClearCredentials( );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT32( stack, retVal );
    }
    TINYCLR_NOCLEANUP();
}

AJ_Status AuthPeerStatus = AJ_ERR_NULL;

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::AuthenticatePeer___MicrosoftSPOTAllJoynAJStatus__U4__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        AJ_BusAttachment * bus;
        LPCSTR fullServiceName = NULL;         
        
        TINYCLR_CHECK_HRESULT( RetrieveBus( stack, bus ) );
        fullServiceName = stack.Arg2().RecoverString();    
            
        AJ_Status status = AJ_BusAuthenticatePeer(bus, fullServiceName, AuthCallback, &AuthPeerStatus);
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT32( stack, status );
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::GetAuthStatus___MicrosoftSPOTAllJoynAJStatus( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {        
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT32( stack, AuthPeerStatus );
    }
    TINYCLR_NOCLEANUP();
}


HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::SetAuthStatus___VOID__MicrosoftSPOTAllJoynAJStatus( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        AJ_Status authStatus = (AJ_Status)stack.Arg1().NumericByRef().u4;            
        AuthPeerStatus = authStatus;
    }
    TINYCLR_NOCLEANUP();
}


HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::SetPskHint___VOID__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    LPCSTR pskHintText = stack.Arg1().RecoverString();
    FAULT_ON_NULL(pskHintText);
    
    if ( hal_strlen_s(pskHintText) >= MAX_HINT_LENGTH )
    {
        TINYCLR_SET_AND_LEAVE( CLR_E_INVALID_PARAMETER );
    }
    
    hal_strcpy_s( PskHint, sizeof(PskHint), pskHintText );
        
    TINYCLR_NOCLEANUP();  
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::SetPskString___VOID__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    LPCSTR pskText = stack.Arg1().RecoverString();
    FAULT_ON_NULL(pskText);
    
    if ( hal_strlen_s(pskText) >= MAX_CERT_LENGTH )
    {
        TINYCLR_SET_AND_LEAVE( CLR_E_INVALID_PARAMETER );
    }
    
    hal_strcpy_s( PskChar, sizeof(PskChar), pskText );
        
    TINYCLR_NOCLEANUP();  
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::SetPemPrivString___VOID__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    LPCSTR pemPrvText = stack.Arg1().RecoverString();
    FAULT_ON_NULL(pemPrvText);
    
    if ( hal_strlen_s(pemPrvText) >= MAX_CERT_LENGTH )
    {
        TINYCLR_SET_AND_LEAVE( CLR_E_INVALID_PARAMETER );
    }
    
    hal_strcpy_s( PemPrv, sizeof(PemPrv), pemPrvText );
        
    TINYCLR_NOCLEANUP();          
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::SetPemX509String___VOID__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    LPCSTR x509Text = stack.Arg1().RecoverString();
    FAULT_ON_NULL(x509Text);
    
    if ( hal_strlen_s(x509Text) >= MAX_CERT_LENGTH )
    {
        TINYCLR_SET_AND_LEAVE( CLR_E_INVALID_PARAMETER );
    }
    
    hal_strcpy_s( PemX509, sizeof(PemX509), x509Text );
        
    TINYCLR_NOCLEANUP();           
}

HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::SetKeyExpiration___VOID__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    KeyExpiration  = stack.Arg1().NumericByRef().u4;    

    TINYCLR_NOCLEANUP();
}


HRESULT Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::SetPassphrase___VOID__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    LPCSTR pwdText = stack.Arg1().RecoverString();
    
    if ( hal_strlen_s(pwdText) >= MAX_PWD_LENGTH )
    {
        TINYCLR_SET_AND_LEAVE( CLR_E_INVALID_PARAMETER );
    }
    
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
    
    static char*     ajIface[MAX_DIM_INTERFACE] = {0,};

    CLR_RT_HeapBlock_String* pathS   = stack.Arg0().DereferenceString();        
    CLR_RT_HeapBlock_String* ifacesS = stack.Arg1().DereferenceString();            
    uint8_t flags                    = stack.Arg2().NumericByRef().u1;
    uint8_t context                  = stack.Arg3().NumericByRef().u4;    
    bool fUseProperties              = stack.Arg4().NumericByRef().u1 != 0;
    bool fLocal                      = stack.Arg5().NumericByRef().u1 != 0;

    LPCSTR path   = pathS  ->StringText(); 
    LPCSTR ifaces = ifacesS->StringText();
    
    FreeInterfaceStorage( ajIface ); // clear out previous interfaces
    DeserializeInterfaceString( ifaces, ajIface );
    
    //debug_printf("Register Object %s , iface %s\n",(char *)theInterface[0], ifaces);
    
    if (fLocal == true)
    {
        if (fUseProperties == true)
        {
            static const AJ_InterfaceDescription localInterfaces[] = {
                AJ_PropertiesIface,     // This must be included for any interface that has properties. 
                ajIface,
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
                ajIface,
                NULL
            };
            
            static const AJ_Object appLocalObjects[] = {
                { path, localInterfaces /*, AJ_OBJ_FLAG_ANNOUNCED | AJ_OBJ_FLAG_DESCRIBED */  },  // make them announceable
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
                ajIface,
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
                ajIface,
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

