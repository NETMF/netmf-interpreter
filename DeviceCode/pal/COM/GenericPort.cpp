#include <tinyhal.h>

// this implementation is intended to fit into
// existing solutions without additional modifications
// The functionality of Generic ports is triggered by
// defining TOTAL_GENERIC_PORTS to a value > 0 in the
// solution's platform_selector.h header file


BOOL GenericPort_Initialize( int portNum )
{
#if TOTAL_GENERIC_PORTS == 0
    return FALSE;
#else
    if( portNum >= TOTAL_GENERIC_PORTS )
        return FALSE;

    GenericPortTableEntry const& entry = *g_GenericPorts[ portNum ];
    if( entry.Port.Initialize == NULL )
        return TRUE;

    return entry.Port.Initialize( entry.pInstance );
#endif
}

BOOL GenericPort_Uninitialize( int portNum )
{
#if TOTAL_GENERIC_PORTS == 0
    return FALSE;
#else
    if( portNum >= TOTAL_GENERIC_PORTS )
        return FALSE;

    GenericPortTableEntry const& entry = *g_GenericPorts[ portNum ];
    if( entry.Port.Uninitialize == NULL )
        return TRUE;

    return entry.Port.Uninitialize( entry.pInstance );
#endif
}

int GenericPort_Write( int portNum, const char* Data, size_t size )
{
#if TOTAL_GENERIC_PORTS == 0
    return 0;
#else
    if( portNum >= TOTAL_GENERIC_PORTS )
        return 0;

    GenericPortTableEntry const& entry = *g_GenericPorts[ portNum ];
    if( entry.Port.Write == NULL )
        return 0;

    return entry.Port.Write( entry.pInstance, Data, size );
#endif
}

int GenericPort_Read( int portNum, char* Data, size_t size )
{
#if TOTAL_GENERIC_PORTS == 0
    return 0;
#else
    if( portNum >= TOTAL_GENERIC_PORTS )
        return 0;

    GenericPortTableEntry const& entry = *g_GenericPorts[ portNum ];
    if( entry.Port.Read )
        return 0;

    return entry.Port.Read( entry.pInstance, Data, size );
#endif
}

BOOL GenericPort_Flush( int portNum )
{
#if TOTAL_GENERIC_PORTS == 0
    return FALSE;
#else
    if( portNum >= TOTAL_GENERIC_PORTS )
        return FALSE;

    GenericPortTableEntry const& entry = *g_GenericPorts[ portNum ];
    if( entry.Port.Flush == NULL )
        return TRUE;

    return entry.Port.Flush( entry.pInstance );
#endif
}

void GenericPort_ProtectPins( int portNum, BOOL On )
{
#if TOTAL_GENERIC_PORTS == 0
    return;
#else
    if( portNum >= TOTAL_GENERIC_PORTS )
        return;

    GenericPortTableEntry const& entry = *g_GenericPorts[ portNum ];
    if( entry.Port.ProtectPins == NULL )
        return;

    entry.Port.ProtectPins( entry.pInstance, On );
#endif
}

BOOL GenericPort_IsSslSupported( int portNum )
{
#if TOTAL_GENERIC_PORTS == 0
    return FALSE;
#else
    if( portNum >= TOTAL_GENERIC_PORTS )
        return FALSE;

    GenericPortTableEntry const& entry = *g_GenericPorts[ portNum ];
    if( entry.Port.IsSslSupported )
        return FALSE;

    return entry.Port.IsSslSupported( entry.pInstance );
#endif
}

BOOL GenericPort_UpgradeToSsl( int portNum, const UINT8* pCACert, UINT32 caCertLen, const UINT8* pDeviceCert, UINT32 deviceCertLen, LPCSTR szTargetHost )
{
#if TOTAL_GENERIC_PORTS == 0
    return FALSE;
#else
    if( portNum >= TOTAL_GENERIC_PORTS )
        return FALSE;

    GenericPortTableEntry const& entry = *g_GenericPorts[ portNum ];
    if( entry.Port.UpgradeToSsl )
        return FALSE;

    return entry.Port.UpgradeToSsl( entry.pInstance, pCACert, caCertLen, pDeviceCert, deviceCertLen, szTargetHost );
#endif
}

BOOL GenericPort_IsUsingSsl( int portNum )
{
#if TOTAL_GENERIC_PORTS == 0
    return FALSE;
#else
    if( portNum >= TOTAL_GENERIC_PORTS )
        return FALSE;

    GenericPortTableEntry const& entry = *g_GenericPorts[ portNum ];
    if( entry.Port.IsUsingSsl == NULL )
        return FALSE;

    return entry.Port.IsUsingSsl( entry.pInstance );
#endif
}

