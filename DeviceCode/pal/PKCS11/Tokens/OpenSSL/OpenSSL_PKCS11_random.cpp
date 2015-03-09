#include "OpenSSL_pkcs11.h"
#include <RAND\rand.h>

CK_RV PKCS11_Random_OpenSSL::SeedRandom(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pSeed, CK_ULONG ulSeedLen)
{
    RAND_seed(pSeed, (int)ulSeedLen);
    
    return CKR_OK;
}

CK_RV PKCS11_Random_OpenSSL::GenerateRandom(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pRandomData, CK_ULONG ulRandomLen)
{
    RAND_bytes(pRandomData, (int)ulRandomLen);
    
    return CKR_OK;
}

