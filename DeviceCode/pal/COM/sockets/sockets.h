////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _SOCKETS_H_
#define _SOCKETS_H_ 1

//--//

#include "tinyhal.h"

//--//

extern NETWORK_CONFIG  g_NetworkConfig;
extern WIRELESS_CONFIG g_WirelessConfig;

//--//

#define SOCKET_CHECK_ENTER()  \
    INT32 ret=0               \

#define SOCKET_CLEANUP() \
    goto RETURN_OK;      \
    SOCKET_CHECK_RETURN: \

#define SOCKET_CHECK_EXIT_CLEANUP()   \
    RETURN_OK:                        \
    ASSERT(ret != SOCK_SOCKET_ERROR); \
    return ret;                       \

#define SOCKET_CHECK_EXIT()           \
    SOCKET_CLEANUP()                  \
    RETURN_OK:                        \
    ASSERT(ret != SOCK_SOCKET_ERROR); \
    return ret;                       \

#define SOCKET_CHECK_EXIT_BOOL()      \
    SOCKET_CLEANUP()                  \
    RETURN_OK:                        \
    return ret != SOCK_SOCKET_ERROR;  \

#define SOCKET_CHECK_EXIT_BOOL_CLEANUP()\
    RETURN_OK:                          \
    return ret != SOCK_SOCKET_ERROR;    \

#define SOCKET_CHECK_RESULT(x)        \
    if((x) == SOCK_SOCKET_ERROR)      \
    {                                 \
        goto SOCKET_CHECK_RETURN;     \
    }                                 \

#define SOCKET_CHECK_BOOL(x)          \
    if(!(x))                          \
    {                                 \
        ret = SOCK_SOCKET_ERROR;      \
        goto SOCKET_CHECK_RETURN;     \
    }                                 \

#define SOCKET_CHECK_EXIT_NORETURN() \
    RETURN_OK:                       \

struct Sockets_Driver
{
    static SOCK_SOCKET Socket( INT32 family, INT32 type, INT32 protocol, BOOL fDebug );
    static INT32 Connect(SOCK_SOCKET socket, const struct SOCK_sockaddr* address, INT32 addressLen) ;
    static INT32 Send(SOCK_SOCKET socket, const char* buf, INT32 len, INT32 flags) ;
    static INT32 Recv(SOCK_SOCKET socket, char* buf, INT32 len, INT32 flags);
    static INT32 Close(SOCK_SOCKET socket);
    static INT32 Listen( SOCK_SOCKET socket, INT32 backlog );
    static SOCK_SOCKET Accept( SOCK_SOCKET socket, struct SOCK_sockaddr* address, INT32* addressLen, BOOL fDebug );
    static INT32 Select( INT32 nfds, SOCK_fd_set* readfds, SOCK_fd_set* writefds, SOCK_fd_set* except, const struct SOCK_timeval* timeout );
    static INT32 RecvFrom( SOCK_SOCKET s, char* buf, INT32 len, INT32 flags, struct SOCK_sockaddr* from, INT32* fromlen );
    static INT32 SendTo( SOCK_SOCKET s, const char* buf, INT32 len, INT32 flags, const struct SOCK_sockaddr* to, INT32 tolen );
    static INT32 Shutdown( SOCK_SOCKET s, INT32 how);
    
    static BOOL  Initialize();
    static BOOL  Uninitialize();
    static INT32 Write( INT32 ComPortNum, const char* Data, size_t size );
    static INT32 Read ( INT32 ComPortNum, char*       Data, size_t size );
    static void  CloseConnections(BOOL fCloseDbg);

    static BOOL ProcessSocketActivity(SOCK_SOCKET signalSocket);

    static void SaveConfig(INT32 index, SOCK_NetworkConfiguration *cfg);

    void* GetSocketSslData(SOCK_SOCKET socket)
    {
        INT32 tmp;

        return GetSocketSslData(socket, tmp);
    }

    void* GetSocketSslData(SOCK_SOCKET socket, INT32 &sockIndex)
    {
        GLOBAL_LOCK_SOCKETS(x);
        
        for(int i=m_cntSockets-1; i>=0; i--)
        {
            if(socket == m_socketHandles[i].m_socket)
            {
                sockIndex = i;
                return m_socketHandles[i].m_sslData;
            }
        }
        
        return NULL;
    }

    void SetSocketSslData(SOCK_SOCKET socket, void* sslObj)
    {
        GLOBAL_LOCK_SOCKETS(x);

        for(int i=m_cntSockets-1; i>=0; i--)
        {
            if(socket == m_socketHandles[i].m_socket)
            {
                m_socketHandles[i].m_sslData = sslObj;
                
                break;
            }
        }
    }

    // required by SSL
    static void UnregisterSocket( INT32 index );

    static BOOL InitializeDbgListener( int ComPortNum );
    static BOOL UninitializeDbgListener( int ComPortNum );

    static void ApplyWirelessConfig();

    static void ClearStatusBitsForSocket(SOCK_SOCKET sock, BOOL fWrite);
    static BOOL InitializeMulticastDiscovery();

private:
    static void CloseDebuggerSocket();
    static void RegisterSocket( SOCK_SOCKET sock, BOOL selectable, BOOL fDebug );
    static void RegisterForSelect( SOCK_SOCKET sock, BOOL isSelectable=TRUE );
    static void ClearStatusBit( SOCK_SOCKET sock, SOCK_fd_set* set );
    static INT32  SocketCount();

    static INT32 SetSelectValues( SOCK_fd_set *reqSet, SOCK_fd_set *selectSet, BOOL fClearIfSet );
    
    static void ApplyConfig();    
    static void SaveWirelessConfig(INT32 index, SOCK_NetworkConfiguration *cfg);

    static void MulticastDiscoveryRespond();

    static void OnDebuggerTimeout(void* arg);

    //--//

    static void SignalNotifyThread();

    static HAL_COMPLETION s_DebuggerTimeoutCompletion;

    struct SocketRegisterMap
    {
        SOCK_SOCKET m_socket;
        UINT32      m_flags;
        void*       m_sslData;
        
        static const UINT32 c_SelectableSocket = 0x0001;
        static const UINT32 c_DebugSocket      = 0x0002;
        static const UINT32 c_CloseSocket      = 0x0004;
    };

    enum DebuggerState
    {
        DbgSock_Uninitialized = 0,
        DbgSock_Listening     = 1,
        DbgSock_Connected     = 2,
    } m_stateDebugSocket;

    SOCK_SOCKET    m_SocketDebugListener;
    SOCK_SOCKET    m_SocketDebugStream;
    SOCK_SOCKET    m_multicastSocket;

    BOOL           m_fShuttingDown;

    SOCK_fd_set m_nextReadSet;
    SOCK_fd_set m_nextWriteSet;
    SOCK_fd_set m_nextExcptSet;

    SOCK_fd_set m_fdsetWrite;
    SOCK_fd_set m_fdsetRead;
    SOCK_fd_set m_fdsetExcept;


    INT32  m_cntSockets;
    struct SocketRegisterMap m_socketHandles[SOCKETS_MAX_COUNT];

    //--//

    // s_initialize is require because Socket uninitialize can be called twice in a row if they call DHCP release
    // then DHCP renew
    static BOOL           s_initialized;
    static const INT32    c_WellKnownDebugPort = DEBUG_SOCKET_PORT;
    static BOOL           s_wirelessInitialized;
    static BOOL           s_discoveryInitialized;
};

extern Sockets_Driver g_Sockets_Driver;

#endif //_SOCKETS_H_
