#include <tinyhal.h>
#include <EVP\EVP.h>

extern "C"
{
BOOL Crypto_Encrypt(BYTE *Key, BYTE *IV, DWORD cbIVSize, BYTE* pPlainText, DWORD cbPlainText, BYTE *pCypherText, DWORD cbCypherText)
{
    INT32 len = cbCypherText, offset = 0;
    BOOL retVal = FALSE;
    EVP_CIPHER_CTX ctx;

    if(EVP_EncryptInit(&ctx, EVP_aes_256_cbc(), (const UINT8*)Key, (const UINT8*) IV) <= 0) return FALSE;
    
    if(EVP_EncryptUpdate(&ctx, pCypherText, &len, (const UINT8*)pPlainText, cbPlainText) > 0)
    {
        offset += len;
        len = cbPlainText - len;
        retVal = (EVP_EncryptFinal(&ctx, &pCypherText[len], &len) > 0);
    }

    return retVal;
}
}
