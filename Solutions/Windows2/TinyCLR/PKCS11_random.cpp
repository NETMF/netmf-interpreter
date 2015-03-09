#include "pkcs11.h"

CK_RV PKCS11_Random_Windows::SeedRandom(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pSeed, CK_ULONG ulSeedLen )
{
    if(!EmulatorNative::GetIRandomDriver()->SeedRandom((int)pSessionCtx->TokenCtx, (IntPtr)pSeed, ulSeedLen)) return CKR_FUNCTION_FAILED;

    return CKR_OK;
}

CK_RV PKCS11_Random_Windows::GenerateRandom(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pRandomData, CK_ULONG ulRandomLen )
{
    if(!EmulatorNative::GetIRandomDriver()->GenerateRandom((int)pSessionCtx->TokenCtx, (IntPtr)pRandomData, ulRandomLen)) return CKR_FUNCTION_FAILED;

    return CKR_OK;
}

