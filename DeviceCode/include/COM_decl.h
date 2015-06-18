////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_COM_DIRECTOR_DECL_H_
#define _DRIVERS_COM_DIRECTOR_DECL_H_ 1

//--//

extern INT32 g_DebuggerPort_SslCtx_Handle;

BOOL DebuggerPort_Initialize  ( COM_HANDLE ComPortNum );
BOOL DebuggerPort_Uninitialize( COM_HANDLE ComPortNum );

// max retries is the number of retries if the first attempt fails, thus the maximum
// total number of attempts is maxRretries + 1 since it always tries at least once.
int  DebuggerPort_Write( COM_HANDLE ComPortNum, const char* Data, size_t size, int maxRetries = 99 );
int  DebuggerPort_Read ( COM_HANDLE ComPortNum, char*       Data, size_t size );
BOOL DebuggerPort_Flush( COM_HANDLE ComPortNum                                );
BOOL DebuggerPort_IsSslSupported( COM_HANDLE ComPortNum );
BOOL DebuggerPort_UpgradeToSsl( COM_HANDLE ComPortNum, UINT32 flags );
BOOL DebuggerPort_IsUsingSsl( COM_HANDLE ComPortNum );

struct IDebuggerPortSslConfig
{
    BOOL (*GetCertificateAuthority)( UINT8** caCert, UINT32* pCertLen );
    BOOL (*GetTargetHostName      )( LPCSTR* strTargHost );
    BOOL (*GetDeviceCertificate   )( UINT8** deviceCert, UINT32* pCertLen );
};

extern IDebuggerPortSslConfig g_DebuggerPortSslConfig;

void CPU_ProtectCommunicationGPIOs( BOOL On );

void CPU_InitializeCommunication();
void CPU_UninitializeCommunication();

#endif // _DRIVERS_COM_DIRECTOR_DECL_H_
