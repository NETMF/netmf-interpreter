////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#include <crypto.h>

////////////////////////////////////////////////////////////////////////////////////////////////////

#ifdef WIN32
ALIGN RSABuffer MSPublicKeyModulus;
#else
ALIGN const RSABuffer MSPublicKeyModulus = {0};
#endif

CRYPTO_RESULT Crypto_GetFingerprint(BYTE *key, BYTE *Signature, int cbSignatureSize)
{
    return CRYPTO_FAILURE;
}
	
BOOL Crypto_GetHash(BYTE *pBuffer, DWORD cbBufferSize, BYTE *pHash, DWORD cbHashSize)
{
    return FALSE;
}

BOOL Crypto_GetActivationStringFromSeed(char *pString, int cbStringSize, KeySeed *Seed, UINT16 region, UINT16 model)
{
    return FALSE;
}

// Encrypts a buffer using a symmetric algorithm.
BOOL Crypto_Encrypt(BYTE *Key, BYTE *IV, DWORD cbIVSize, BYTE* pPlainText, DWORD cbPlainText, BYTE *pCypherText, DWORD cbCypherText)
{
    return FALSE;
}

// Decrypts a buffer using a symmetric algorithm
BOOL Crypto_Decrypt(BYTE *Key, BYTE *IV, DWORD cbIVSize, BYTE *pCypherText, DWORD cbCypherText, BYTE* pPlainText, DWORD cbPlainText)
{
    return FALSE;
}

CRYPTO_RESULT Crypto_StartRSAOperationWithKey(enum RSAOperations operation, RSAKey *pRSAKey, BYTE *pSourceText, DWORD cbSourceText, BYTE *pDestText, DWORD cbDestText, void **ppHandle)
{
    return CRYPTO_SUCCESS;
}


CRYPTO_RESULT Crypto_StepRSAOperation(void** pHandle)
{
    return CRYPTO_SUCCESS;
}

CRYPTO_RESULT Crypto_AbortRSAOperation(void** pHandle)
{
    return CRYPTO_SUCCESS;
}

CRYPTO_RESULT Crypto_GeneratePrivateKey(KeySeed *pSeed, RSAKey* pPrivateKey)
{
    return CRYPTO_FAILURE;
}

CRYPTO_RESULT Crypto_PublicKeyFromPrivate(const RSAKey *privateKey, RSAKey*publicKey)
{
    return CRYPTO_FAILURE;
}

CRYPTO_RESULT Crypto_PublicKeyFromModulus(const RSABuffer *modulus, RSAKey *key)	// used for MS key which is only stored as a modulus
{
    return CRYPTO_FAILURE;
}

CRYPTO_RESULT GetC1Value(BYTE *key, BYTE *C1, int cbC1)
{
    return CRYPTO_FAILURE;
}

CRYPTO_RESULT Crypto_KeySeedFromLaser(const BYTE *laserData, KeySeed* keySeed)
{
    return CRYPTO_FAILURE;
}

CRYPTO_RESULT Crypto_CreatedotNetMFKey(BYTE* pSeed, UINT16 *pDelta1, UINT16 *pDelta2)
{
    return CRYPTO_FAILURE;
}

CRYPTO_RESULT Crypto_LaserFromKeySeed(const KeySeed* keySeed, BYTE *laserData)
{
    return CRYPTO_FAILURE;
}

BOOL Crypto_GetActivationStringFromBuffer(char *pString, int cbStringSize, BYTE *buffer, int cbBufferSize)
{
    return FALSE;
}

CRYPTO_RESULT Crypto_SignBuffer(BYTE* pBuffer, int cbBufferSize, RSAKey *pKey, BYTE* pSignature, int cbSignature)
{
    return CRYPTO_FAILURE;
}

CRYPTO_RESULT Crypto_DecodeActivationString(char *pString, int cbStringSize, BYTE WatchID[WATCH_ID_SIZE], UINT16 *pRegion, UINT16 *pModel)
{
    return CRYPTO_FAILURE;
}

CRYPTO_RESULT Crypto_RSACompute(const PRSAKey pRSAKey, BYTE *pSourceText, DWORD cbSourceText, BYTE *pDestText, DWORD cbDestText, enum RSAOperations op)
{
    return CRYPTO_FAILURE;
}

#ifdef WIN32

CRYPTO_RESULT Crypto_CreateZenithKey(BYTE* pSeed, UINT16 *pDelta1, UINT16 *pDelta2)
{
    return CRYPTO_FAILURE;
}

#endif

