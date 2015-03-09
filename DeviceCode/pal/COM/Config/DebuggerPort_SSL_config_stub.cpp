#include <tinyhal.h>
#include <com_decl.h>

static BOOL DebuggerPort_SSL_GetCACert( UINT8** caCert, UINT32* certLen )
{
    *caCert  = NULL;
    *certLen = 0;
    return FALSE;
}

static BOOL DebuggerPort_SSL_GetTargetHost( LPCSTR* strTargetHost )
{
    *strTargetHost = NULL;
    return TRUE;
}

static BOOL DebuggerPort_SSL_GetDeviceCert( UINT8** caCert, UINT32* certLen )
{
    *caCert  = NULL;
    *certLen = 0;
    return FALSE;
}

IDebuggerPortSslConfig g_DebuggerPortSslConfig =
{
    DebuggerPort_SSL_GetCACert,
    DebuggerPort_SSL_GetTargetHost,
    DebuggerPort_SSL_GetDeviceCert,
};

