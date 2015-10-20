////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_COM_DIRECTOR_DECL_H_
#define _DRIVERS_COM_DIRECTOR_DECL_H_ 1

// The functionality of Generic ports is triggered by
// defining TOTAL_GENERIC_PORTS to a value > 0 in the
// solution's platform_selector.h header file
// Otherwise, the implementation of these functions is
// just a stub that returns values indicating failure
// as they should never be called.

BOOL GenericPort_Initialize( int portNum );
BOOL GenericPort_Uninitialize( int portNum );
int GenericPort_Write( int portNum, const char* Data, size_t size );
int GenericPort_Read( int portNum, char* Data, size_t size );
BOOL GenericPort_Flush( int portNum );
void GenericPort_ProtectPins( int portNum, BOOL On ); 
BOOL GenericPort_IsSslSupported( int portNum );
BOOL GenericPort_UpgradeToSsl( int portNum, const UINT8* pCACert, UINT32 caCertLen, const UINT8* pDeviceCert, UINT32 deviceCertLen, LPCSTR szTargetHost );
BOOL GenericPort_IsUsingSsl( int portNum );

// interface table for a generic port 
// any NULL entries in the table indicates default handling is desired
struct IGenericPort 
{
    // default returns TRUE
    BOOL (*Initialize)( void* pInstance );
    
    // default returns TRUE
    BOOL (*Uninitialize)( void* pInstance );
    
    // default return 0
    int (*Write)( void* pInstance, const char* Data, size_t size );
    
    // defualt return 0
    int (*Read)( void* pInstance, char* Data, size_t size );
    
    // default return TRUE
    BOOL (*Flush)( void* pInstance );
    
    // default do nothing
    void (*ProtectPins)( void* pInstance, BOOL On ); 
    
    // default return FALSE
    BOOL (*IsSslSupported)( void* pInstance );
    
    // default return FALSE
    BOOL (*UpgradeToSsl)( void* pInstance, const UINT8* pCACert, UINT32 caCertLen, const UINT8* pDeviceCert, UINT32 deviceCertLen, LPCSTR szTargetHost );
    
    // default return FALSE
    BOOL (*IsUsingSsl)( void* pInstance );
};

struct GenericPortTableEntry
{
    IGenericPort const& Port;
    void* pInstance;
};

#if TOTAL_GENERIC_PORTS > 0
// ConvertCOM_GenericPort( ComPortHandle ) produces an index into this table
// This table is provided by the solution to allow customization and extensibility
// at the solution level. Individual port "drivers" can provide a common
// GenericPortTableEntry structure implementation yet still remain independent of 
// the actual solution since the solution owns the table, and ordering and thus the
// handle definitions as well. 
extern GenericPortTableEntry const* const g_GenericPorts[ TOTAL_GENERIC_PORTS ];
#endif

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
