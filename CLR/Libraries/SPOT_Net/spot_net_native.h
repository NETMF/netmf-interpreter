////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _SPOT_NET_NATIVE_H_
#define _SPOT_NET_NATIVE_H_

struct Library_system_sockets_System_Net_Sockets_SocketException
{
    // base class "Exception" takes up fields 1-4
    static const int FIELD___errorCode = 5;

    //--//

};

struct Library_spot_net_native_Microsoft_SPOT_Net_NetworkInformation_NetworkInterface
{
    static const int FIELD___interfaceIndex       = 1;
    static const int FIELD___flags                = 2;
    static const int FIELD___ipAddress            = 3;
    static const int FIELD___gatewayAddress       = 4;
    static const int FIELD___subnetMask           = 5;
    static const int FIELD___dnsAddress1          = 6;
    static const int FIELD___dnsAddress2          = 7;
    static const int FIELD___networkInterfaceType = 8;
    static const int FIELD___macAddress           = 9;

    TINYCLR_NATIVE_DECLARE(InitializeNetworkInterfaceSettings___VOID);
    TINYCLR_NATIVE_DECLARE(UpdateConfiguration___VOID__I4);
    TINYCLR_NATIVE_DECLARE(GetNetworkInterfaceCount___STATIC__I4);
    TINYCLR_NATIVE_DECLARE(GetNetworkInterface___STATIC__MicrosoftSPOTNetNetworkInformationNetworkInterface__U4);
    TINYCLR_NATIVE_DECLARE(IPAddressFromString___STATIC__U4__STRING);

    //--//

};

struct Library_spot_net_native_Microsoft_SPOT_Net_NetworkInformation_Wireless80211
{
    static const int FIELD__Authentication = 10;
    static const int FIELD__Encryption     = 11;
    static const int FIELD__Radio          = 12;
    static const int FIELD__PassPhrase     = 13;
    static const int FIELD__NetworkKey     = 14;
    static const int FIELD__ReKeyInternal  = 15;
    static const int FIELD__Ssid           = 16;
    static const int FIELD__Id             = 17;

    TINYCLR_NATIVE_DECLARE(UpdateConfiguration___STATIC__VOID__MicrosoftSPOTNetNetworkInformationWireless80211__BOOLEAN);
    TINYCLR_NATIVE_DECLARE(SaveAllConfigurations___STATIC__VOID);



    //--//

};

struct Library_spot_net_native_Microsoft_SPOT_Net_NetworkInformation_NetworkAvailabilityEventArgs
{
    static const int FIELD___isAvailable = 1;


    //--//

};

struct Library_spot_net_native_Microsoft_SPOT_Net_NetworkInformation_NetworkChange
{
    static const int FIELD_STATIC__NetworkAddressChanged      = 0;
    static const int FIELD_STATIC__NetworkAvailabilityChanged = 1;


    //--//

};

struct Library_spot_net_native_Microsoft_SPOT_Net_SocketNative
{
    TINYCLR_NATIVE_DECLARE(socket___STATIC__I4__I4__I4__I4);
    TINYCLR_NATIVE_DECLARE(bind___STATIC__VOID__OBJECT__SZARRAY_U1);
    TINYCLR_NATIVE_DECLARE(connect___STATIC__VOID__OBJECT__SZARRAY_U1__BOOLEAN);
    TINYCLR_NATIVE_DECLARE(send___STATIC__I4__OBJECT__SZARRAY_U1__I4__I4__I4__I4);
    TINYCLR_NATIVE_DECLARE(recv___STATIC__I4__OBJECT__SZARRAY_U1__I4__I4__I4__I4);
    TINYCLR_NATIVE_DECLARE(close___STATIC__I4__OBJECT);
    TINYCLR_NATIVE_DECLARE(listen___STATIC__VOID__OBJECT__I4);
    TINYCLR_NATIVE_DECLARE(accept___STATIC__I4__OBJECT);
    TINYCLR_NATIVE_DECLARE(getaddrinfo___STATIC__VOID__STRING__BYREF_STRING__BYREF_SZARRAY_SZARRAY_U1);
    TINYCLR_NATIVE_DECLARE(shutdown___STATIC__VOID__OBJECT__I4__BYREF_I4);
    TINYCLR_NATIVE_DECLARE(sendto___STATIC__I4__OBJECT__SZARRAY_U1__I4__I4__I4__I4__SZARRAY_U1);
    TINYCLR_NATIVE_DECLARE(recvfrom___STATIC__I4__OBJECT__SZARRAY_U1__I4__I4__I4__I4__BYREF_SZARRAY_U1);
    TINYCLR_NATIVE_DECLARE(getpeername___STATIC__VOID__OBJECT__BYREF_SZARRAY_U1);
    TINYCLR_NATIVE_DECLARE(getsockname___STATIC__VOID__OBJECT__BYREF_SZARRAY_U1);
    TINYCLR_NATIVE_DECLARE(getsockopt___STATIC__VOID__OBJECT__I4__I4__SZARRAY_U1);
    TINYCLR_NATIVE_DECLARE(setsockopt___STATIC__VOID__OBJECT__I4__I4__SZARRAY_U1);
    TINYCLR_NATIVE_DECLARE(poll___STATIC__BOOLEAN__OBJECT__I4__I4);
    TINYCLR_NATIVE_DECLARE(ioctl___STATIC__VOID__OBJECT__U4__BYREF_U4);

    //--//
    
    static HRESULT MarshalSockAddress( struct SOCK_sockaddr* addrDst, CLR_UINT32& addrLenDst, const CLR_RT_HeapBlock& blkSrc );
    static HRESULT MarshalSockAddress( CLR_RT_HeapBlock& blkDst, const struct SOCK_sockaddr* addrSrc, CLR_UINT32 addrLenSrc );
    static HRESULT SendRecvHelper( CLR_RT_StackFrame& stack, bool fSend, bool fAddress );
    static HRESULT SockOptHelper( CLR_RT_StackFrame& stack, bool fGet );
    static HRESULT SockNameHelper( CLR_RT_StackFrame& stack, bool fPeer );
    static HRESULT BindConnectHelper( CLR_RT_StackFrame& stack, bool fBind );
    static HRESULT ThrowOnError( CLR_RT_StackFrame& stack, CLR_INT32 err );
    static void    ThrowError( CLR_RT_StackFrame& stack, CLR_INT32 errorCode );
    
    static CLR_INT32 Helper__SelectSocket( CLR_INT32 socket, CLR_INT32 mode );

    
    /* WARNING!!!
     * The value of this constant is the offset for the m_Handle field in the System.Net.Sockets.Socket class.
     * It is defined here to avoid a circular reference issue.
     */
    static const int FIELD__m_Handle = 1;

    static const int DISPOSED_HANDLE = -1;
};

struct Library_spot_net_native_Microsoft_SPOT_Net_NetworkInformation_NetworkChange__NetworkEvent
{
    static const int FIELD__EventType = 3;
    static const int FIELD__Flags     = 4;
    static const int FIELD__Time      = 5;


    //--//
};

extern const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_Net;
#endif  //_SPOT_NET_NATIVE_H_
