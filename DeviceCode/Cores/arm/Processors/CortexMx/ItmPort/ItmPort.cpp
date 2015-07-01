#include <tinyhal.h>

#include "cmsis_generic.h"

// only one "generic" port supported for ITM tracing messages to hardware debugger
// so pInstance is ignored
static int ItmPort_Write( void* pInstance, const char* Data, size_t size )
{
    for( int i = 0; i< size; ++i )
        ITM_SendChar( Data[i] );
    
    return size;
}

static IGenericPort const ItmPortItf =
{
    // default returns TRUE
    NULL, //BOOL (*Initialize)( void* pInstance );
    
    // default returns TRUE
    NULL, //BOOL (*Uninitialize)( void* pInstance );
    
    // default return 0
    ItmPort_Write, //int (*Write)( void* pInstance, const char* Data, size_t size );
    
    // defualt return 0
    NULL, //int (*Read)( void* pInstance, char* Data, size_t size );
    
    // default return TRUE
    NULL, //BOOL (*Flush)( void* pInstance );
    
    // default do nothing
    NULL, //void (*ProtectPins)( void* pInstance, BOOL On ); 
    
    // default return FALSE
    NULL, //BOOL (*IsSslSupported)( void* pInstance );
    
    // default return FALSE
    NULL, //BOOL (*UpgradeToSsl)( void* pInstance, const UINT8* pCACert, UINT32 caCertLen, const UINT8* pDeviceCert, UINT32 deviceCertLen, LPCSTR szTargetHost );
    
    // default return FALSE
    NULL, //BOOL (*IsUsingSsl)( void* pInstance );
};

extern const GenericPortTableEntry Itm0GenericPort =
{
    ItmPortItf,
    NULL
};
