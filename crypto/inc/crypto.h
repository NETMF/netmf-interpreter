////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#ifndef CRYPTO_H
#define CRYPTO_H

#ifndef ALIGN
#if defined(__arm)
#define ALIGN __align(8)
#else
#define ALIGN
#endif
#endif

#ifndef ARRAYSIZE
#define ARRAYSIZE(x) (sizeof(x)/sizeof(x[0]))
#endif

#define WATCH_ID_SIZE			5	// size in bytes of the watch ID string. It's obtained by getting the watch key signature and using the upper 40 bits
#define AES_KEY_SIZE_BYTES		16	// size in bytes of the keys for AES symmetric encryption
#define TEA_KEY_SIZE_BYTES		16	// size in bytes of the keys for TEA symmetric encryption

#define ACTIVATION_STRING_SIZE	17	// XXXXXXXXXXXXXXXX + zero termination
#define ACTIVATION_BUFFER_SIZE	(WATCH_ID_SIZE + 2 * sizeof(UINT16))
#define ACTIVATION_CODE_SIZE	5	// the number of bits encoded by one character
#define AES_BLOCK_SIZE_BYTES	AES_KEY_SIZE_BYTES
#define TEA_BLOCK_SIZE_BYTES	8

#ifdef USE_AES
#define CipherFunction			aes
#define BLOCK_SIZE				AES_BLOCK_SIZE_BYTES
#define KEY_SIZE_BYTES			AES_KEY_SIZE_BYTES
#else
#define CipherFunction			tea
#define BLOCK_SIZE				TEA_BLOCK_SIZE_BYTES
#define KEY_SIZE_BYTES			TEA_KEY_SIZE_BYTES
#endif

#define HASH_SIZE				16	// the number of bytes of the resulting hash 

#define FINGERPRINT_SIZE		KEY_SIZE_BYTES  // the number of bytes of the symmetric key fingerprint


#define RSA_KEY_SIZE_BITS		1024				// the size of N in a RSA key in bits (also exponent size; a complete key has both)
#define RSA_BLOCK_SIZE_BITS		1024				// the size of a block in bits (also D or N size; a complete key has both)

#define RSA_KEY_SIZE_BYTES		(RSA_KEY_SIZE_BITS/8)	// the size of N in bytes 
#define RSA_BLOCK_SIZE_BYTES	RSA_KEY_SIZE_BYTES
#define RSA_EXPONENT			65537
#define RSA_EXPONENT_SIZE_BYTES	4		// 65537
#define PKCS1_PAD_SIZE			8		// the minimum size of the random block of PKCS1 padding
#define PKCS1_OVERHEAD_SIZE		3		// 3 bytes: 0+2 at the beginning of the block, another 0 after the random part
#define RSA_PADDED_PLAINTEXT_SIZE_BYTES	(RSA_KEY_SIZE_BYTES - PKCS1_PAD_SIZE - PKCS1_OVERHEAD_SIZE)

#define roundup(x, y)((y) * (((x) + (y) - 1)/(y)))
#define RSAEncryptedSize(x)(roundup(x, RSA_PADDED_PLAINTEXT_SIZE_BYTES)/RSA_PADDED_PLAINTEXT_SIZE_BYTES*RSA_BLOCK_SIZE_BYTES)


// protocol-related info

#define PACKET_SIZE			128
#define PACKET_HASH_SIZE	6

#define FILETIME_SIZE		8

#ifdef __cplusplus

extern "C" 
{
#endif

typedef BYTE RSABuffer[RSA_BLOCK_SIZE_BYTES];

// keygen related info

#define SEED_SIZE_BYTES	16

typedef struct tagKeySeed
{
	BYTE Seed[SEED_SIZE_BYTES];
	UINT16 delta[2];
}KeySeed, *PKeySeed;

typedef struct tagRSAKey
{
	DWORD exponent_len;
	RSABuffer module;
	RSABuffer exponent;
} RSAKey, *PRSAKey;

typedef enum tagCRYPTO_RESULT
{
	CRYPTO_CONTINUE = 1,	// operation should continue
	CRYPTO_SUCCESS = 0,		// alles gute
	CRYPTO_SIGNATUREFAIL = -1,
	CRYPTO_BADPARMS = -2,
	CRYPTO_KEYEXPIRED = -3,
	CRYPTO_UNKNOWNKEY = -4,
	CRYPTO_UNKNOWNERROR = -5,
	CRYPTO_NOMEMORY = -6,
	CRYPTO_ACTIVATIONBADSYNTAX = -7,
	CRYPTO_ACTIVATIONBADCONTROLCHAR = -8,
	CRYPTO_FAILURE = -9
} CRYPTO_RESULT;

// this function computes a symmetric key signature based on a symmetric key
// there are maximum BLOCK_SIZE bytes in the signature (16 bytes for AES and XTEA)
CRYPTO_RESULT Crypto_GetFingerprint(BYTE *key, BYTE *Signature, int cbSignatureSize);
	
// this function generates a hash using the currently chosen hash algorithm

BOOL Crypto_GetHash(BYTE *pBuffer, DWORD cbBufferSize, BYTE *pHash, DWORD cbHashSize);

//BOOL Crypto_GetUniqueInfoFromKey(WatchUniqueInfo *pWatchInfo, SymmetricKey *pKey);

// this function returns a string encoding the watch ID; pString must be at least ACTIVATION_KEY_SIZE
// it computes the activation string from a received watch seed
BOOL Crypto_GetActivationStringFromSeed(char *pString, int cbStringSize, KeySeed *Seed, UINT16 region, UINT16 model);

// Encrypts a buffer using a symmetric algorithm.
BOOL Crypto_Encrypt(BYTE *Key, BYTE *IV, DWORD cbIVSize, BYTE* pPlainText, DWORD cbPlainText, BYTE *pCypherText, DWORD cbCypherText);

// Decrypts a buffer using a symmetric algorithm
BOOL Crypto_Decrypt(BYTE *Key, BYTE *IV, DWORD cbIVSize, BYTE *pCypherText, DWORD cbCypherText, BYTE* pPlainText, DWORD cbPlainText);

// RSA functions

enum RSAOperations
{
	RSA_ENCRYPT,
	RSA_DECRYPT,
	RSA_VERIFYSIGNATURE
};

#ifndef NO_RSA

#if defined(WIN32) && defined(TESTKEYS)
#pragma message("Replace with real keys!!")
#endif

// the MS public key
#ifdef WIN32
extern ALIGN RSABuffer MSPublicKeyModulus;
#else
extern ALIGN const RSABuffer MSPublicKeyModulus;
#endif

// the dotNetMF public key
#ifdef WIN32
extern ALIGN RSABuffer dotNetMFPublicKeyModulus;
#else
extern ALIGN const RSABuffer dotNetMFPublicKeyModulus;
#endif

//
// this call initiates a RSA operation; it returns an error code(< 0), CRYPTO_SUCCESS (= 0), or CRYPTO_CONTINUE (=1)
// For RSA_VERIFYSIGNATURE pass the buffer in pSourceText and the signature in pDestText
//

CRYPTO_RESULT Crypto_StartRSAOperationWithKey( enum RSAOperations operation
                                             , RSAKey *pRSAKey
                                             , BYTE *pSourceText
                                             , DWORD cbSourceText
                                             , BYTE *pDestText
                                             , DWORD cbDestText
                                             , void **ppHandle
                                             );


//
// this function continues a RSA operation (using the handle returned by a previous call to  
// Crypto_StartRsaOperationWithKey or Crypto_StepRsaOperation). It returns an error code(< 0), CRYPTO_SUCCESS (= 0), or CRYPTO_CONTINUE (=1)
// For the client, the caller may either call this function in a loop itself and use the return value to check for the end of the process 
// or use the continuation mechanism and check the contents of pResult parameter at intervals.
// The continuation mechanism is stubbed out on the server, so the caller needs to keep calling the function until it stops returning CRYPTO_CONTINUE
// !!! Mar 12: removed continuation mechanism - coscor !!!

CRYPTO_RESULT Crypto_StepRSAOperation(void** pHandle);

CRYPTO_RESULT Crypto_AbortRSAOperation(void** pHandle);

CRYPTO_RESULT Crypto_GeneratePrivateKey(KeySeed *pSeed, RSAKey* pPrivateKey);

CRYPTO_RESULT Crypto_PublicKeyFromPrivate(const RSAKey *privateKey, RSAKey*publicKey);

CRYPTO_RESULT Crypto_PublicKeyFromModulus(const RSABuffer *modulus, RSAKey *key);	// used for MS key which is only stored as a modulus

CRYPTO_RESULT GetC1Value(BYTE *key, BYTE *C1, int cbC1);

CRYPTO_RESULT Crypto_KeySeedFromLaser(const BYTE *laserData, KeySeed* keySeed);

#ifdef WIN32

// this function receives a 16 byte string (128 bits), generates a dotNetMF key and returns the 
// two deltas in pDelta1 and pDelta2.
// The return value is CRYPTO_SUCCESS if the function succeeded (it found a good pair of primes in the 
// acceptable interval) and CRYPTO_FAILURE otherwise

CRYPTO_RESULT Crypto_CreateZenithKey(BYTE* pSeed, UINT16 *pDelta1, UINT16 *pDelta2);
#endif

#if defined(WIN32) || defined (TESTLIB)

CRYPTO_RESULT Crypto_LaserFromKeySeed(const KeySeed* keySeed, BYTE *laserData);

//// this function returns a string encoding the watch ID; pString must be at least ACTIVATION_KEY_SIZE
//// it computes the activation string from a received 9 byte buffer that contains the watch ID + region + model
BOOL Crypto_GetActivationStringFromBuffer(char *pString, int cbStringSize, BYTE *buffer, int cbBufferSize);

// this function signs a buffer and returns the buffer signature
CRYPTO_RESULT Crypto_SignBuffer(BYTE* pBuffer, int cbBufferSize, RSAKey *pKey, BYTE* pSignature, int cbSignature);

// this function decodes a watch activation string and returns a 40 bit ID
CRYPTO_RESULT Crypto_DecodeActivationString(char *pString, int cbStringSize, BYTE WatchID[WATCH_ID_SIZE], UINT16 *pRegion, UINT16 *pModel);

// the following functions/macros perform RSA operations without slicing. Use if you don't care about the time or if
// you know it won't take long (for example, if your exponent is small)

CRYPTO_RESULT Crypto_RSACompute(const PRSAKey pRSAKey, BYTE *pSourceText, DWORD cbSourceText, BYTE *pDestText, DWORD cbDestText, enum RSAOperations op);

#define Crypto_RSAEncrypt(pKey, pPlainText, cbPlainText, pCipherText, cbCipherText) \
	Crypto_RSACompute(pKey, pPlainText, cbPlainText, pCipherText,cbCipherText, RSA_ENCRYPT)
#define Crypto_RSADecrypt(pKey, pPlainText, cbPlainText, pCipherText, cbCipherText) \
	Crypto_RSACompute(pKey, pCipherText, cbCipherText, pPlainText,cbPlainText, RSA_DECRYPT)

#endif	// win32 or TESTLIB

#endif // NO_RSA


#ifdef __cplusplus
}
#endif // __cplusplus

#endif // CRYPTO_H

