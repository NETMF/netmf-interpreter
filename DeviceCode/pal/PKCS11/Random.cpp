#include "CryptokiPAL.h"

/* Random number generation */

/* C_SeedRandom mixes additional seed material into the token's
 * random number generator. */
CK_DEFINE_FUNCTION(CK_RV, C_SeedRandom)
(
  CK_SESSION_HANDLE hSession,  /* the session's handle */
  CK_BYTE_PTR       pSeed,     /* the seed material */
  CK_ULONG          ulSeedLen  /* length of seed material */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret   != CKR_OK) return ret;
    if(pSeed == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->Random != NULL && pSession->Token->Random->SeedRandom != NULL)
    {
        return pSession->Token->Random->SeedRandom(&pSession->Context, pSeed, ulSeedLen);
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_GenerateRandom generates random data. */
CK_DEFINE_FUNCTION(CK_RV, C_GenerateRandom)
(
  CK_SESSION_HANDLE hSession,    /* the session's handle */
  CK_BYTE_PTR       pRandomData,  /* receives the random data */
  CK_ULONG          ulRandomLen  /* # of bytes to generate */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret         != CKR_OK) return ret;
    if(pRandomData == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->Random != NULL && pSession->Token->Random->GenerateRandom != NULL)
    {
        return pSession->Token->Random->GenerateRandom(&pSession->Context, pRandomData, ulRandomLen);
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}


