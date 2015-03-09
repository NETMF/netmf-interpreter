#include "cryptokiPAL.h"

BOOL g_isCryptokiInitialized = FALSE;


CK_FUNCTION_LIST g_CryptokiFunctionList = 
{
    {
        CRYPTOKI_VERSION_MAJOR, 
        CRYPTOKI_VERSION_MINOR,
    },
        
    C_Initialize,
    C_Finalize,
    C_GetInfo,
    C_GetFunctionList,
    C_GetSlotList,
    C_GetSlotInfo,
    C_GetTokenInfo,
    C_GetMechanismList,
    C_GetMechanismInfo,
    C_InitToken,
    C_InitPIN,
    C_SetPIN,
    C_OpenSession,
    C_CloseSession,
    C_CloseAllSessions,
    C_GetSessionInfo,
    C_GetOperationState,
    C_SetOperationState,
    C_Login,
    C_Logout,
    C_CreateObject,
    C_CopyObject,
    C_DestroyObject,
    C_GetObjectSize,
    C_GetAttributeValue,
    C_SetAttributeValue,
    C_FindObjectsInit,
    C_FindObjects,
    C_FindObjectsFinal,
    C_EncryptInit,
    C_Encrypt,
    C_EncryptUpdate,
    C_EncryptFinal,
    C_DecryptInit,
    C_Decrypt,
    C_DecryptUpdate,
    C_DecryptFinal,
    C_DigestInit,
    C_Digest,
    C_DigestUpdate,
    C_DigestKey,
    C_DigestFinal,
    C_SignInit,
    C_Sign,
    C_SignUpdate,
    C_SignFinal,
    C_SignRecoverInit,
    C_SignRecover,
    C_VerifyInit,
    C_Verify,
    C_VerifyUpdate,
    C_VerifyFinal,
    C_VerifyRecoverInit,
    C_VerifyRecover,
    C_DigestEncryptUpdate,
    C_DecryptDigestUpdate,
    C_SignEncryptUpdate,
    C_DecryptVerifyUpdate,
    C_GenerateKey,
    C_GenerateKeyPair,
    C_WrapKey,
    C_UnwrapKey,
    C_DeriveKey,
    C_SeedRandom,
    C_GenerateRandom,
    C_GetFunctionStatus,
    C_CancelFunction,
    C_WaitForSlotEvent,
};

static void OnRebootHandler()
{
    C_Finalize(NULL);
}


CK_DEFINE_FUNCTION(CK_RV, C_Initialize)(
    CK_VOID_PTR pInitArgs
)
{
    UINT32 i;
    if(pInitArgs != NULL)
    {
        //CK_C_INITIALIZE_ARGS_PTR pArgs = (CK_C_INITIALIZE_ARGS_PTR)pInitArgs;

        return CKR_ARGUMENTS_BAD;
    }

    if(g_isCryptokiInitialized) return CKR_CRYPTOKI_ALREADY_INITIALIZED;

    Cryptoki_InitializeSession();
    Cryptoki_InitializeSlots();

    for(i=0; i<g_CryptokiSlotCount; i++)
    {
        if(g_CryptokiSlots[i].Token->TokenState && g_CryptokiSlots[i].Token->TokenState->Initialize)
        {
            g_CryptokiSlots[i].Token->TokenState->Initialize();
        }
    }

    HAL_AddSoftRebootHandler(OnRebootHandler);
    
    g_isCryptokiInitialized = TRUE;

    return CKR_OK;
}

CK_DEFINE_FUNCTION(CK_RV, C_Finalize)(
    CK_VOID_PTR pReserved
)
{
    UINT32 i;
    if(!g_isCryptokiInitialized) return CKR_CRYPTOKI_NOT_INITIALIZED;
    if(pReserved != NULL        ) return CKR_ARGUMENTS_BAD;


    for(i=0; i<g_CryptokiSlotCount; i++)
    {
        C_CloseAllSessions(i);
        
        if(g_CryptokiSlots[i].Token->TokenState && g_CryptokiSlots[i].Token->TokenState->Uninitialize)
        {
            g_CryptokiSlots[i].Token->TokenState->Uninitialize();
        }
    }

    g_isCryptokiInitialized = FALSE;
    
    return CKR_OK;
}

static const CK_UTF8CHAR c_Manufacturer[] = "Microsoft";
static const CK_UTF8CHAR c_Library[]      = "MSCryptoki";


CK_DEFINE_FUNCTION(CK_RV, C_GetInfo)(
    CK_INFO_PTR pInfo
)
{
    if(!g_isCryptokiInitialized) return CKR_CRYPTOKI_NOT_INITIALIZED;
    if(pInfo == NULL) return CKR_ARGUMENTS_BAD;

    pInfo->cryptokiVersion.major = CRYPTOKI_VERSION_MAJOR;
    pInfo->cryptokiVersion.minor = CRYPTOKI_VERSION_MINOR;
    memcpy(&pInfo->manufacturerID, c_Manufacturer, ARRAYSIZE(c_Manufacturer));
    pInfo->flags = 0;
    memcpy(pInfo->libraryDescription, c_Library, ARRAYSIZE(c_Library));
    pInfo->libraryVersion.major = NETMF_CRYPTOKI_VERSION_MAJOR;
    pInfo->libraryVersion.minor = NETMF_CRYPTOKI_VERSION_MINOR;        
    
    return CKR_OK;
}

CK_DEFINE_FUNCTION(CK_RV, C_GetFunctionList)(
    CK_FUNCTION_LIST_PTR_PTR ppFunctionList
)
{
    if(ppFunctionList == NULL) return CKR_ARGUMENTS_BAD;
    
    *ppFunctionList = &g_CryptokiFunctionList;

    return CKR_OK;    
}

CK_DEFINE_FUNCTION(CK_RV, C_GetFunctionStatus)(
    CK_SESSION_HANDLE hSession
)
{
    return CKR_FUNCTION_NOT_PARALLEL;
}


CK_DEFINE_FUNCTION(CK_RV, C_CancelFunction)(
    CK_SESSION_HANDLE hSession
)
{
    return CKR_FUNCTION_NOT_PARALLEL;
}

UINT8* be_memcpy(void* dest, const void* src, size_t len)
{
    UINT8*       d = (UINT8*      )dest;
    const UINT8* s = (const UINT8*)src;

    while(len)
    {
        d[--len] = *s++;
    }

    return (UINT8*)s;
}

