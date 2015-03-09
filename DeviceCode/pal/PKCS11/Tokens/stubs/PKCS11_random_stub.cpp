#include "pkcs11_stub.h"

CK_RV PKCS11_Random_stub::SeedRandom(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pSeed, CK_ULONG ulSeedLen)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Random_stub::GenerateRandom(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pRandomData, CK_ULONG ulRandomLen)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

