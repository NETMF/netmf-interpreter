// test_win.cpp : Defines the entry point for the console application.
//

#include <windows.h>
#define TEST
#include "..\inc\crypto.h"

#include <stdlib.h>
#include <stdio.h>
#include <time.h>


#include "..\inc\rsa_dotNetMF.h"

#include "..\inc\bignum.h"
void Initialize();

//BYTE plainText[]="Hello, crypto, my good friend, it's nice to see you and all that!, 1; Hello, crypto, my good friend, it's nice to see you and all that! 2, Hello, crypto, my good friend, it's nice to see you and all that! 3, Hello, crypto, my good friend, it's nice to see you and all that! 12345";

KeySeed seed = 
{
	{
	213, 233, 128, 178, 234, 201, 204, 83, 191, 103, 214, 191,20,214 ,126 ,45
	},
	{
		45, 783
	}
};

BYTE plainText[] = 
{
	41, 35, 190, 132, 225, 108, 214, 174, 82, 144,73 ,241,241,187,233,235,179,166,219,60 ,135,12 ,62,
	153 ,36 ,94 ,13 ,28 ,6 ,183,71 ,222,179,18 ,77 ,200,67 ,187,139,166,31 ,3 ,90 ,125,9 ,56 ,37 ,31,
	93 ,212,203,252,150,245,69 ,59 ,19 ,13 ,137,10 ,28 ,219,174,50 ,32 ,154,80 ,238,64 ,120,54 ,253,18 ,73 ,50 ,246,158,125,73,
	220,173,79 ,20 ,242,68 ,64 ,102,208,107,196,48 ,183,50 ,59 ,161,34 ,246,34 ,145,157,225,139,31 ,218,176,202,153,2 ,185,114,
	157,73 ,44 ,128,126,197,153
};

RSAKey pubKey = 
{
	1,
	{
		0xa1, 0x2b, 0x03, 0x84, 0xe0, 0x22, 0x16, 0x26,
		0x7f, 0x37, 0x1e, 0xe6, 0x3e, 0xcf, 0x6f, 0x1d,
		0x1e, 0xe1, 0xdb, 0x08, 0xe0, 0xae, 0x19, 0xf9,
		0xe9, 0xe4, 0xd2, 0x49, 0x74, 0x08, 0x36, 0x12,
		0xc6, 0xaf, 0xe5, 0xb1, 0x77, 0xfd, 0x42, 0xf4,
		0x78, 0xb3, 0x65, 0xf1, 0x1a, 0xe9, 0x2c, 0x54,
		0x69, 0xd5, 0x5f, 0x48, 0x64, 0x1e, 0x55, 0xbf,
		0x1b, 0x2d, 0xa4, 0xca, 0xdc, 0xe3, 0xa5, 0x77,
		0xb6, 0x31, 0xea, 0x01, 0x25, 0x5b, 0xc0, 0x20,
		0x97, 0xb2, 0x07, 0xf2, 0xb9, 0xc4, 0x20, 0xc8,
		0x56, 0x04, 0x12, 0x4c, 0x99, 0xf3, 0x08, 0x2a,
		0xb6, 0xa3, 0x68, 0x8f, 0x84, 0x7e, 0x47, 0xf2,
		0x16, 0x62, 0x2c, 0x46, 0x88, 0x02, 0x75, 0x6f,
		0x66, 0x63, 0x35, 0x74, 0xc0, 0x40, 0xa3, 0x9f,
		0x49, 0xf9, 0xd1, 0xa5, 0x49, 0x4e, 0xf5, 0x03,
		0x92, 0x5a, 0x45, 0x6e, 0x87, 0x0d, 0xf8, 0x7f
	},
	{
		0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
	}
};
RSAKey privKey = 
{
	32,
	{
		0xa1, 0x2b, 0x03, 0x84, 0xe0, 0x22, 0x16, 0x26,
		0x7f, 0x37, 0x1e, 0xe6, 0x3e, 0xcf, 0x6f, 0x1d,
		0x1e, 0xe1, 0xdb, 0x08, 0xe0, 0xae, 0x19, 0xf9,
		0xe9, 0xe4, 0xd2, 0x49, 0x74, 0x08, 0x36, 0x12,
		0xc6, 0xaf, 0xe5, 0xb1, 0x77, 0xfd, 0x42, 0xf4,
		0x78, 0xb3, 0x65, 0xf1, 0x1a, 0xe9, 0x2c, 0x54,
		0x69, 0xd5, 0x5f, 0x48, 0x64, 0x1e, 0x55, 0xbf,
		0x1b, 0x2d, 0xa4, 0xca, 0xdc, 0xe3, 0xa5, 0x77,
		0xb6, 0x31, 0xea, 0x01, 0x25, 0x5b, 0xc0, 0x20,
		0x97, 0xb2, 0x07, 0xf2, 0xb9, 0xc4, 0x20, 0xc8,
		0x56, 0x04, 0x12, 0x4c, 0x99, 0xf3, 0x08, 0x2a,
		0xb6, 0xa3, 0x68, 0x8f, 0x84, 0x7e, 0x47, 0xf2,
		0x16, 0x62, 0x2c, 0x46, 0x88, 0x02, 0x75, 0x6f,
		0x66, 0x63, 0x35, 0x74, 0xc0, 0x40, 0xa3, 0x9f,
		0x49, 0xf9, 0xd1, 0xa5, 0x49, 0x4e, 0xf5, 0x03,
		0x92, 0x5a, 0x45, 0x6e, 0x87, 0x0d, 0xf8, 0x7f
	},
	{
		0x99, 0x62, 0x1e, 0xa3, 0x1a, 0x9c, 0x11, 0xa7,
		0x75, 0x3c, 0x72, 0xd6, 0x65, 0xb6, 0xfe, 0x9d,
		0xc8, 0x14, 0xe0, 0x12, 0xb8, 0xb7, 0x6b, 0xa6,
		0x66, 0x24, 0x95, 0x9d, 0x05, 0xe5, 0xf7, 0x7f,
		0x60, 0x2f, 0x06, 0xfd, 0x81, 0x97, 0x4e, 0x73,
		0x31, 0x9f, 0x14, 0xe7, 0x77, 0x93, 0x0c, 0x28,
		0xfe, 0x4d, 0x43, 0xdd, 0x98, 0x04, 0x61, 0xc0,
		0xef, 0x9b, 0x6e, 0xaa, 0xea, 0x45, 0x01, 0x64,
		0xca, 0x17, 0x82, 0xd1, 0xba, 0x55, 0x54, 0x34,
		0x84, 0x76, 0x8e, 0xea, 0xf1, 0xee, 0xca, 0x7b,
		0x6a, 0x9a, 0x7c, 0x5c, 0xd5, 0xd1, 0x9e, 0xd3,
		0xf6, 0xb5, 0xfe, 0xc1, 0x7d, 0xba, 0x0e, 0xf0,
		0x24, 0x37, 0xfe, 0x7f, 0xdb, 0x45, 0x59, 0x54,
		0x8d, 0x1d, 0x36, 0x04, 0xed, 0x5d, 0xd1, 0x48,
		0x68, 0xef, 0x60, 0x2d, 0xc9, 0x16, 0x32, 0x5b,
		0x57, 0x96, 0xd0, 0x4d, 0xa0, 0xe2, 0xee, 0x7a
	}
};

// test functions
void Dump(BYTE* pBuf, int cbSize)
{
	int i;
	for (i = 0; i < cbSize; i++)
	{
		if ((i%8) == 0)
			printf("\r\n");
		printf("0x%2.2x, ",pBuf[i]);
	}
	printf("\r\n\r\n");
}


#define DELTA 0x9E3779B9
#define ROUNDS 32

void XTEAEncryptBlock(unsigned long data[2], unsigned long *key)

{

      unsigned long y=data[0], z=data[1];

      unsigned long sum = DELTA<<5;

      while (sum)

      {

            z -= (y<<4 ^ y>>5) + y ^ sum + key[sum>>11 & 3];

            sum -= DELTA;

            y -= (z << 4 ^ z >> 5) + z ^ sum + key[sum & 3];

      }

      data[0] = y;

      data[1] = z;

}

 

void XTEAEncryptCBC(unsigned long *pData, unsigned long dwNumBlocks, unsigned long IV[2], unsigned long *pKey)

{
	unsigned dwBlk;
      for (dwBlk = 0; dwBlk < dwNumBlocks; dwBlk++, pData += 2)
      {
            pData[0] ^= IV[0];

            pData[1] ^= IV[1];

            XTEAEncryptBlock(pData, pKey);

            IV[0] = pData[0];

            IV[1] = pData[1];

      }

}

 

void ComputeFingerprint(unsigned char *pValue, unsigned char *pFingerprint)

{

      // The dotNetMF code assumes little-endianness, so first we 

      // massage the value if necessary

      unsigned char pPlainText[16], pPlainText2[16], i;

      unsigned long IV[2];

      unsigned long *pulVal = (unsigned long *)pValue;

 
      printf("Input data: ");

      for (i = 0; i < 16; i++)

      {

            printf("%02X", pValue[i]);

      }

      printf("\n");

 

      //for (i = 0; i < 4; i++)

      //{

      //      pulVal[i] = FixEndianness(pulVal[i]);

      //}

 

      printf("Input data after endian patch: ");

      for (i = 0; i < 16; i++)

      {

            printf("%02X", pValue[i]);

      }

      printf("\n");

 

      memcpy(pPlainText, "Microsoft SPOT ", 16);

      pulVal = (unsigned long *)pPlainText;

      //for (i = 0; i < 4; i++)

      //{

      //      pulVal[i] = FixEndianness(pulVal[i]);

      //}

 

      memcpy(pPlainText2, pPlainText, 16);

      IV[0] = IV[1] = 0;

      XTEAEncryptCBC((unsigned long *)pPlainText, 2, IV, (unsigned long *)pValue);

 

      printf("C1 value: ");

      for (i = 0; i < 16; i++)

      {

            printf("%02X", pPlainText[i]);

      }

      printf("\n");

}







void DoStuff()
{
	BYTE *buffer;
	int size;
	int i;
	BYTE temp;
	RSAKey publicKey;
	CRYPTO_RESULT result;
	BYTE signature[]=
	{
0x6	
,0x4d
,0xb2
,0x76
,0x5e
,0x1c
,0xf0
,0x58
,0x4	
,0x99
,0xe1
,0x15
,0x1a
,0x4b
,0x27
,0xa8
,0xe8
,0x3d
,0x28
,0xa0
,0xfb
,0xe7
,0x51
,0x8d
,0x6e
,0xd0
,0x73
,0x5f
,0xaa
,0xb9
,0x8a
,0xd4
,0x9f
,0xb	
,0x90
,0xa6
,0x1	
,0x22
,0x4	
,0x45
,0xe2
,0xba
,0xaf
,0xf6
,0x9e
,0xa	
,0x5e
,0xec
,0x89
,0xc0
,0xfe
,0x63
,0x1f
,0x1d
,0x7e
,0x5a
,0xd	
,0xbe
,0x5d
,0xb6
,0xf8
,0x12
,0x39
,0x95
,0x58
,0x88
,0x8e
,0x39
,0x4	
,0xfd
,0xad
,0x67
,0x80
,0x7c
,0xbb
,0x16
,0x82
,0xcf
,0xb9
,0x3b
,0xc8
,0x66
,0xa3
,0x83
,0x8f
,0x9f
,0x1f
,0xc5
,0xbf
,0x37
,0x95
,0xe0
,0x4f
,0x4c
,0x8	
,0x55
,0x2e
,0xc5
,0x7d
,0x1	
,0x1d
,0xcd
,0x7d
,0xdb
,0xee
,0x54
,0xa7
,0xf	
,0x58
,0xff
,0xb0
,0x8a
,0xe7
,0x6	
,0x21
,0xcf
,0x5c
,0x7a
,0x84
,0x14
,0x92
,0xcb
,0xae
,0x94
,0x8f
,0xb7
,0xe	
,0x4c
	};
	BYTE invSignature[sizeof(signature)];
	BYTE decryptedSignature[sizeof(signature)];
	//FILE *f = fopen("C:\\public\\protest\\Tetris.pe", "rb");
	//fseek(f, 0, SEEK_END);
	//size = (int)ftell(f);
	//buffer = malloc(size);
	//rewind(f);
	//fread(buffer, 1, size, f);
	//fclose(f);
	//f = fopen("C:\\public\\protest\\Tetrix.sign", "rb");
	//fread(signature, 1, RSA_BLOCK_SIZE_BYTES, f);
	//fclose(f);
	size = 117;
	buffer = malloc(size);
	memset(buffer, 0, size);
	buffer[0] = 1;

	Crypto_PublicKeyFromModulus(&dotNetMFPublicKeyModulus, &publicKey);

	result = Crypto_RSACompute(&publicKey, signature, sizeof(signature), decryptedSignature, sizeof(decryptedSignature), RSA_DECRYPT);
	if (memcmp(buffer, decryptedSignature, sizeof(buffer)) != 0)
	{
		printf ("Bug\r\n");
	 
	}
	printf ("Result is %d for goodbuf\r\n", result);
	free (buffer);

	//f = fopen("C:\\public\\protest\\Tetris.dll", "rb");
	//fseek(f, 0, SEEK_END);
	//size = (int)ftell(f);
	//buffer = malloc(size);
	//rewind(f);
	//fread(buffer, 1, size, f);
	//fclose(f);
	//	
	//result = Crypto_RSACompute(&publicKey, buffer, size, signature, RSA_BLOCK_SIZE_BYTES, RSA_VERIFYSIGNATURE);
	//printf ("Result is %d for goodbuf\r\n", result);

	//free(buffer);
	return;

}

BOOL GenerateKeys(RSAKey* pPrivateKey, RSAKey *pPublicKey);

BOOL RSAEncrypt(RSAKey *key, BYTE plainText[], int cbPlainText, BYTE cipherText[], int cbCipherText)
{
	DigitBuffer dPlain;
	DigitBuffer modPlain;
	DigitBuffer dCrypt;
	DigitBuffer modCrypt;
//	DigitBuffer dDecrypt;
//	digit_t modDecrypt[BITS_TO_DIGITS(RSA_BLOCK_SIZE_BITS)];
	mp_modulus_t modulus;

#if 0
	create_modulus((digit_t*)key->module, KEY_SIZE_DIGITS, FROM_RIGHT, &modulus);
#else
	create_modulus((digit_t*)key->module, KEY_SIZE_DIGITS, FROM_LEFT, &modulus);
#endif

	memset(modPlain, 0, sizeof(modPlain));
	memset(dCrypt, 0, sizeof(dCrypt));
	memset(modCrypt, 0, sizeof(modCrypt));

	// make plainText a digit_t[] in modular representation
	dwords_to_digits((DWORD*)plainText, dPlain, cbPlainText/sizeof(DWORD));
	if (compare_same((digit_t*)key->module, dPlain, 32) <=0)
	{
		printf("Bad modulus!\r\n");
		return FALSE;
	}
	to_modular(dPlain, 32, modPlain, &modulus);
	if (mp_errno != 0)
	{
		printf("Bignum error during RSA encryption: %d\r\n", mp_errno);
		return FALSE;
	}

	// encrypt
	mod_exp(modPlain, (digit_t*)key->exponent, key->exponent_len, modCrypt, &modulus);
	if (mp_errno != 0)
	{
		printf("Bignum error during RSA encryption: %d\r\n", mp_errno);
		return FALSE;
	}
	from_modular(modCrypt, dCrypt, &modulus);
	if (mp_errno != 0)
	{
		printf("Bignum error during RSA encryption: %d\r\n", mp_errno);
		return FALSE;
	}
	memcpy(cipherText, (BYTE*) dCrypt, cbCipherText);
	return TRUE;
}

BOOL TestBignumRSA(RSAKey* publicKey, RSAKey *privateKey)
{
	int i;
	BYTE stressText[128];
	BYTE cipherText[128];
	BYTE decipherText[128];


	for (i = 0; i <sizeof(stressText) ; i++)
	{
		memset(stressText, 0, sizeof(stressText));
		memset(cipherText, 0, sizeof(cipherText));
		memset(decipherText, 0, sizeof(decipherText));

		memset(stressText, 0xFF, i);

		if (!RSAEncrypt(publicKey, stressText, sizeof(stressText), cipherText, sizeof(cipherText)))
		{
			printf("TestBignumRSA: Error during RSA encryption for i = %d\r\n", i);
			return FALSE;
		}

		if (!RSAEncrypt(privateKey, cipherText, sizeof(cipherText), decipherText, sizeof(decipherText)))
		{
			printf("TestBignumRSA: Error during RSA decryption for i = %d\r\n", i);
			return FALSE;
		}

		if (memcmp(stressText, decipherText, sizeof(decipherText)) != 0)
		{
			printf("TestBignumRSA: texts don't match for i = %d\r\n", i);
			return FALSE;
		}

	}
	return TRUE;
}

BOOL GenerateKeys(RSAKey* pPrivateKey, RSAKey *pPublicKey)
{
	CRYPTO_RESULT result = CRYPTO_FAILURE;
	int i;
	// initialize seed with random values
	
	srand((int)time(NULL));
	do
	{
		for (i = 0; i < sizeof(seed.Seed); i++)
			seed.Seed[i] = rand()&0xFF;

	}
	while (CRYPTO_SUCCESS != Crypto_CreatedotNetMFKey(seed.Seed, &seed.delta[0], &seed.delta[1]));
	// we got the key seed
	// create the actual key
	result = Crypto_GeneratePrivateKey(&seed, pPrivateKey);
	if (CRYPTO_SUCCESS != result)
	{
		printf("Error creating asymmetric keys!\r\n");
		return FALSE;
	}

	return (CRYPTO_SUCCESS == Crypto_PublicKeyFromPrivate(pPrivateKey, pPublicKey));
}

BOOL VerifyKeys(RSAKey *privateKey, RSAKey *publicKey)
{
	DWORD dwExp[]={RSA_EXPONENT};
	digit_t dExp[1];

	// verify that the modulus is large enough

	if (memcmp(privateKey->module, publicKey->module, RSA_KEY_SIZE_BYTES) != 0)
	{
		printf("VerifyKeys: modules not equal\r\n");
		return FALSE;
	}
	
	if (privateKey->module[RSA_KEY_SIZE_BYTES] == 0)
	{
		printf("VerifyKeys: module MSB 0\r\n");
		return FALSE;
	}
	if (significant_digit_count((digit_t*)privateKey->exponent, KEY_SIZE_DIGITS) != privateKey->exponent_len)
	{
		printf("VerifyKeys: bad private exponent length\r\n");
		return FALSE;
	}

	if (publicKey->exponent_len != 1)
	{
		printf("VerifyKeys: bad public exponent length\r\n");
		return FALSE;
	}

	dwords_to_digits(dwExp, dExp, 1);

	if (compare_diff((digit_t*)publicKey->exponent, publicKey->exponent_len, dExp, 1) != 0)
	{
		printf("VerifyKeys: bad public exponent\r\n");
		return FALSE;
	}

	return TRUE;
}


int TestRSAEncryption(	RSAKey *privateKey, RSAKey *publicKey)
{
//	RSAKey privateKey, publicKey;
	BYTE cipherText[RSAEncryptedSize(sizeof(plainText))];
	BYTE decipherText[sizeof(plainText)];
	int i, nErrors;

	printf("\r\nTestRSAEncryption start\r\n");

	nErrors = 0;

	for (i = 0; i < 1; i++)
	{
		//printf("TestRSAEncryption loop %d, nErrors = %d\r\n", i, nErrors);

		memset(decipherText, 0, sizeof(decipherText));
		memset(cipherText, 0, sizeof(cipherText));

		//if (!TestBignumRSA(&publicKey, &privateKey))
		//{
		//	printf("Crypto_RSAEncrypt error in TestBignumRSA!\r\n");
		//	nErrors++;
		//	continue;
		//}

		// test RSA encryption
//		if (CRYPTO_SUCCESS != Crypto_RSAEncrypt(&publicKey, plainText, sizeof(plainText), cipherText, sizeof(cipherText)))


		if (CRYPTO_SUCCESS != Crypto_RSAEncrypt(publicKey, plainText, sizeof(plainText), cipherText, sizeof(cipherText)))
		{
			printf("Crypto_RSAEncrypt error in TestRSAEncryption!\r\n");
			nErrors++;
			continue;
		}

		// compute equivalent padded buffer

//		buffer[127] = 0;
//		buffer[126] = 2;
//		memset(buffer + 118, 0xFF, 8);
//		buffer[117] = 0;
//		memcpy(buffer, plainText, 117);
//
////		RSAEncrypt(&publicKey, plainText, 117, cipherText1, 128);
//		RSAEncrypt(publicKey, buffer, 128, cipherText1, 128);
//		if (memcmp(cipherText, cipherText1, 128) != 0)
//		{
//			printf("TestRSAEncryption: encryption mismatch!\r\n");
//			nErrors++;
//			//continue;
//		}


		if (CRYPTO_SUCCESS != Crypto_RSADecrypt(privateKey, decipherText, sizeof(decipherText), cipherText, sizeof(cipherText)))
//		if (CRYPTO_SUCCESS != Crypto_RSADecrypt(privateKey, decipherText, 117, cipherText, 128))
		{
			//TestBignumRSA(publicKey, privateKey);
			printf("Crypto_RSADecrypt error in TestRSAEncryption!\r\n");

			printf("\r\nPrivate key seed:\r\n");
			Dump(seed.Seed, sizeof(seed.Seed));
			printf("\r\nDeltas: %d, %d\r\n", seed.delta[0], seed.delta[1]);

			printf("\r\nPrivate key module:\r\n");
			Dump(privateKey->module, sizeof(privateKey->module));
			printf("\r\nPrivate key exponent:\r\n");
			Dump(privateKey->exponent, sizeof(privateKey->exponent));
			printf("\r\nPublic key exponent:\r\n");
			Dump(publicKey->exponent, sizeof(publicKey->exponent));
			printf("\r\nPlain Text:\r\n");
			Dump(plainText, sizeof(plainText));

			nErrors++;
			continue;
		}
		if (memcmp(plainText, decipherText, sizeof(plainText))!= 0)
//		if (strncmp(plainText, decipherText, 117)!= 0)
		{
			printf("Error in TestRSAEncryption!\r\n");
			nErrors++;
		}
	}
	printf("TestRSAEncryption done, %d errors\r\n", nErrors);
	return nErrors;
}


//void TestPrimality()
//{
//	int i, j, l;
//	BYTE buffer[64];
//
//	for (i = 0; i < 100; i++)
//	{
//		srand((int)time(NULL) + i);
//		for (l = 0; l < 64; l++)
//			buffer[l] = rand()&0xFF;
//		buffer[0] |= 1;
//		for (j = 0; j < 65534; j+=2)
//		{
//			if (Crypto_IsPrime(buffer, sizeof(buffer)))
//			{
//				printf("Found a prime in %i attempts\r\n", j/2 +1);
//				break;
//			}
//			if (CRYPTO_SUCCESS != Crypto_GetNextCandidate(buffer, sizeof(buffer)))
//			{
//				printf("Bad parms in Crypto_GetNextCandidate\r\n", j);
//			}
//		}
//		if (j >= 65534)
//			printf("Prime not found\r\n", j);
//	}
//}

int TestActivationString()
{
	BYTE WatchID[5], WatchID2[5];
	int  i, j;
	short region,model, decodedReg, decodedModel;
	char string[ACTIVATION_STRING_SIZE + 1];
	int nErrors = 0;

	BYTE key[] = 
	{
		0x7E,0x05,0x0D,0x59,0xCB,0x33,0x1F,0x19,0x22,0x97,0x39,0x22,0xE8,0x9B,0xBC,0xFD
	};

	KeySeed Seed;

	printf("\r\nTestActivationString\r\n");

	string[ACTIVATION_STRING_SIZE] = '\0';
	memcpy(Seed.Seed, key, sizeof(key));

	for (j = 0; j < 20; j++)
	{

		//region = rand();
		//model = rand();

		region = 0;
		model = 0;

		for (i = 0; i < sizeof(Seed.Seed); i++)
			Seed.Seed[i] = rand() & 0xFF;

		Crypto_GetActivationStringFromSeed(string, ACTIVATION_STRING_SIZE, key, 0, 0);
		Crypto_DecodeActivationString(string, ACTIVATION_STRING_SIZE, WatchID, &decodedReg, &decodedModel);
		Crypto_GetFingerprint(&Seed, WatchID2, sizeof(WatchID2));
		if (memcmp(WatchID, WatchID2, sizeof(WatchID)) != 0)
		{
			printf("Error in activation string computing\r\n");
			nErrors++;
		}
		if (region != decodedReg || model != decodedModel)
		{
			printf("Error in activation string computing\r\n");
			nErrors++;
		}
	}
	printf("TestActivationString done with %d errors\r\n", nErrors);
	return nErrors;
}

int TestSymmetricEncryption()
{
	static BYTE cryptBuffer[1024];
	static BYTE decryptBuffer[1024];
	static BYTE IV0[BLOCK_SIZE + 10];
	static BYTE IV[BLOCK_SIZE + 10];
	static BYTE symmetricKey[16];
	int nErrors = 0;

	int i, j, k, l;

	printf("\r\nTestSymmetricEncryption \r\n");

	for (k = 0; k < 1; k++)
	{
		// random key
		for (l = 0; l < sizeof(symmetricKey); l++)
			symmetricKey[l] = rand()&0xFF;

		// random length
		for (i = BLOCK_SIZE; i <= sizeof(plainText); i++)
		{
			// random IV
			for (l = 0; l < sizeof(IV0); l++)
				IV0[l] = rand()&0xFF;

			memcpy(IV, IV0, sizeof(IV0));
			Crypto_Encrypt(symmetricKey, IV, BLOCK_SIZE, plainText, i, cryptBuffer, i);
			memcpy(IV, IV0, sizeof(IV0));
			Crypto_Decrypt(symmetricKey, IV, BLOCK_SIZE, cryptBuffer, i, decryptBuffer, i);
			if (memcmp(plainText, decryptBuffer, i) != 0)
			{
				printf("length error for %d\r\n", i);
				nErrors++;
			}
		}
		//printf("%i\r", k);
	}
	// different IVs

	for (j = 16; j < BLOCK_SIZE + 10; j++)
	{
		for (i = 8; i < 1024; i++)
		{
			memset(decryptBuffer, 0, 1024);
			memcpy(IV, IV0, sizeof(IV0));
			IV[j] = 34;
			Crypto_Encrypt(symmetricKey, IV, j, plainText, i, cryptBuffer, i);
			memcpy(IV, IV0, sizeof(IV0));
			IV[j] = 36;
			Crypto_Decrypt(symmetricKey, IV, j, cryptBuffer, i, decryptBuffer, i);
			if (memcmp(plainText, decryptBuffer, i) != 0)
			{
				printf("IV Error for %d\r\n", i);
				nErrors++;
			}
		}
	}
	printf("TestSymmetricEncryption done with %d errors\r\n", nErrors);
	return nErrors;
}

int TestSignatures(RSAKey *privateKey, RSAKey *publicKey)
{
	BYTE signature[RSA_BLOCK_SIZE_BYTES];
	BYTE save;
	int index;
	CRYPTO_RESULT result;
	void* pHandle;
	int nErrors = 0;
	printf ("\r\nTestSignatures started\r\n");
	result = Crypto_SignBuffer(plainText, sizeof(plainText), privateKey, signature, sizeof(signature));
	if (result != CRYPTO_SUCCESS)
	{
		printf ("Error in Crypto_SignBuffer!\r\n");
		nErrors++;
	}
	result = Crypto_StartRSAOperationWithKey(RSA_VERIFYSIGNATURE, publicKey, plainText, sizeof(plainText), signature, sizeof(signature), &pHandle);
	while (result == CRYPTO_CONTINUE)
	{
		result = Crypto_StepRSAOperation(pHandle);
	}
	if (result != CRYPTO_SUCCESS)
	{
		printf("Error 1 verifying signature\r\n");
		nErrors++;
	}

	index = rand()%sizeof(plainText);
	
	save = plainText[index];

	do 
	{
		plainText[index] = rand()%0xFF;
	}
	while (plainText[index] == save);
		
	result = Crypto_StartRSAOperationWithKey(RSA_VERIFYSIGNATURE, publicKey, plainText, sizeof(plainText), signature, RSA_BLOCK_SIZE_BYTES, &pHandle);
	while (result == CRYPTO_CONTINUE)
	{
		result = Crypto_StepRSAOperation(pHandle);
	}

	plainText[index] = save;
	if (result != CRYPTO_SIGNATUREFAIL)
	{
		printf("Error 2 verifying signature\r\n");
		nErrors++;
	}
	index = rand()%sizeof(signature);
	save = signature[index];
	do 
	{
		signature[index] = rand()%0xFF;
	}
	while (signature[index] == save);
	result = Crypto_StartRSAOperationWithKey(RSA_VERIFYSIGNATURE, publicKey, plainText, sizeof(plainText), signature, RSA_BLOCK_SIZE_BYTES, &pHandle);

	while (result == CRYPTO_CONTINUE)
	{
		result = Crypto_StepRSAOperation(pHandle);
	}
	if (result != CRYPTO_SIGNATUREFAIL)
	{
		printf("Error 3 verifying signature\r\n");
		nErrors++;
	}
	signature[index] = save;
	printf ("TestSignatures finished with %d errors\r\n", nErrors);
	return nErrors;
}

int TestFingerprint()
{

	BYTE key[KEY_SIZE_BYTES], fingerprint[5], i;

	for (i = 0; i < sizeof(key); i++)
	{
		key[i] = rand()&0xFF;
	}

	if (CRYPTO_SUCCESS != Crypto_GetFingerprint(key, fingerprint, sizeof(fingerprint)))
	{
		printf ("TestFingerprint error! \r\n");
		return 1;
	}
	return 0;
}

int TestHash()
{
	BYTE Hash[HASH_SIZE];
	Crypto_GetHash(plainText, sizeof(plainText), Hash, HASH_SIZE);
	return 0;
}

BOOL DoRSAGenerateKeysCommon()
{
      int i=0, j;
      KeySeed objSeed;
      CRYPTO_RESULT cResult;
      RSAKey rsaprivKey;
      RSAKey rsapubKey;
      BOOL bRet = TRUE;
      short sDelta1=0;
      short sDelta2=0;
      BYTE arrPlainText[118];
      BYTE arrCypherText[256];
      BYTE arrDecipherText[118];
      cResult = -1;

      for (i=0; i<2; i++)

      {

            for (j = 0; j < sizeof(arrPlainText); j++)
				arrPlainText[j] = rand()&0xFF;

             //first we have to generate the seed
            while (cResult != CRYPTO_SUCCESS)
            {
				  for (j = 0; j < sizeof(KeySeed); j++)
							((BYTE *) &objSeed)[j] = rand()&0xFF;
                  cResult = Crypto_CreatedotNetMFKey((BYTE *)&objSeed , &sDelta1, &sDelta2);
            }
 
            objSeed.delta[0] = sDelta1;
            objSeed.delta[1] = sDelta2;
            
            //lets generate the private key
            cResult = Crypto_GeneratePrivateKey(&objSeed , &rsaprivKey);
            if (cResult != CRYPTO_SUCCESS)
            {
                  bRet = FALSE;
                  break;
            }

            //lets generate the public key
            cResult = Crypto_PublicKeyFromPrivate(&rsaprivKey , &rsapubKey);
            if (cResult != CRYPTO_SUCCESS)
            {
                  bRet = FALSE;
                  break;
            }
 
            //we have all the keys now - its time to do some encryption and decryption
            //In this test - we will do it the easy way
            cResult = Crypto_RSAEncrypt(&rsaprivKey, arrPlainText, sizeof(arrPlainText),arrCypherText, sizeof(arrCypherText));
            if (cResult != CRYPTO_SUCCESS)
            {
                  bRet = FALSE;
                  break;
            }
 
            cResult = Crypto_RSADecrypt(&rsapubKey, arrDecipherText, sizeof(arrDecipherText),arrCypherText, sizeof(arrCypherText));
            if (cResult != CRYPTO_SUCCESS)
            {
                  bRet = FALSE;
                  break;
            }
 
      }
 
      return TRUE;
}

	void VerifyLaserBytes()
	{
		KeySeed seed, seed1;
		int i;
		BYTE Laser[19];
		for (i = 0; i < sizeof(seed.Seed); i++)
		{
			seed.Seed[i] = rand() & 0xFF;
		}
		seed.delta[0] = (UINT16)(rand() % 1024);
		seed.delta[1] = (UINT16)(rand() % 1024);
		Crypto_LaserFromKeySeed(&seed, Laser);
		Crypto_KeySeedFromLaser(Laser, &seed1);
		if (memcmp(&seed, &seed1, sizeof(KeySeed)) != 0)
			printf("BUG!\n");
		else
			printf("OK!\n");

	}



int main(int argc, char* argv[])
{
	RSAKey privateKey, publicKey;
	int nErrors = 0;
	int nTests = 1, i, j;
	BYTE WatchID[5];
	short decodedReg, decodedModel;
	char ActivationString[20];
	byte C1[16];

	Initialize();
	srand( (unsigned)time( NULL ) );


	//nErrors += TestFingerprint();
	DoStuff();
	return;

//	VerifyLaserBytes();


//	DoRSAGenerateKeysCommon();

	if (argc >1)
		nTests = atoi(argv[1]);
	for (j = 0; j< nTests; j++) 
	{
		printf("Test loop %d, %d accumulated errors\r\n", j, nErrors);

		GenerateKeys(&privateKey, &publicKey);
	
		if (!VerifyKeys(&privateKey, &publicKey))
		{
			printf("Error in VerifyKeys!\r\n");
			continue;
		}

		nErrors += TestRSAEncryption(&privateKey, &publicKey);

		nErrors += TestSignatures(&privateKey, &publicKey);

		if (nErrors == 0)
		{
			
		}

		// test encryption
		nErrors += TestSymmetricEncryption();

		// test activation string

		nErrors += TestActivationString();
		
		nErrors += TestFingerprint();
	}
	printf("%d tests done, total errors: %d!\r\n", nTests, nErrors);

}

