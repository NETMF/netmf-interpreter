////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"

HRESULT SaveKeyToFile( std::wstring& fileName, RSAKey& key )
{
    TINYCLR_HEADER();

    BYTE* buf = (BYTE *)&key;

    TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::SaveFile( fileName.c_str(), buf, sizeof(RSAKey) ));

    TINYCLR_NOCLEANUP();
}

HRESULT LoadKeyFromFile( std::wstring& fileName, RSAKey& key )
{
    TINYCLR_HEADER();

    CLR_RT_Buffer buf;

    TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::LoadFile( fileName.c_str(), buf ));

    memcpy(&key, &buf[0], buf.size() );

    TINYCLR_NOCLEANUP();
}

bool GenerateKeyPair( RSAKey& privateKey, RSAKey& publicKey )
{
    KeySeed ks;
    UINT16  d0;
    UINT16  d1;
    int     i;

    srand((unsigned int)time(0));

    for(i=0; i<SEED_SIZE_BYTES; i++)
    {
        ks.Seed[i] = rand()%256;
    }

    if(Crypto_CreateZenithKey((BYTE *)&ks, &d0, &d1) != CRYPTO_SUCCESS)
    {
        return false;
    }
    
    ks.delta[0] = d0;
    ks.delta[1] = d1;

    if(Crypto_GeneratePrivateKey(&ks, &privateKey) != CRYPTO_SUCCESS)
    {
        return false;
    }

    if(Crypto_PublicKeyFromPrivate(&privateKey, &publicKey) != CRYPTO_SUCCESS)
    {
        return false;
    }

    return true;
}

bool SignData( UINT8* plainText, size_t plainTextLength, RSAKey& privateKey, UINT8* signature, size_t signatureLength )
{
    if(Crypto_SignBuffer( plainText, (int)plainTextLength, &privateKey, signature, (int)signatureLength ) != CRYPTO_SUCCESS)
    {
        return false;
    }

    return true;
}

bool VerifySignature( UINT8* plainText, size_t plainTextLength, RSAKey& publicKey, UINT8* signature, size_t signatureLength )
{
    void* h;

    CRYPTO_RESULT result = Crypto_StartRSAOperationWithKey(RSA_VERIFYSIGNATURE, &publicKey, plainText, (int)plainTextLength, signature, (int)signatureLength, &h);

    while (result == CRYPTO_CONTINUE)
    {
        result = Crypto_StepRSAOperation(&h);
    }

    if (result != CRYPTO_SUCCESS)
    {
        return false;
    }

    return true;
}
