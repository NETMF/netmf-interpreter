/* Written by Ben Laurie, 2001 */
/*
 * Copyright (c) 2001 The OpenSSL Project.  All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer. 
 *
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in
 *    the documentation and/or other materials provided with the
 *    distribution.
 *
 * 3. All advertising materials mentioning features or use of this
 *    software must display the following acknowledgment:
 *    "This product includes software developed by the OpenSSL Project
 *    for use in the OpenSSL Toolkit. (http://www.openssl.org/)"
 *
 * 4. The names "OpenSSL Toolkit" and "OpenSSL Project" must not be used to
 *    endorse or promote products derived from this software without
 *    prior written permission. For written permission, please contact
 *    openssl-core@openssl.org.
 *
 * 5. Products derived from this software may not be called "OpenSSL"
 *    nor may "OpenSSL" appear in their names without prior written
 *    permission of the OpenSSL Project.
 *
 * 6. Redistributions of any form whatsoever must retain the following
 *    acknowledgment:
 *    "This product includes software developed by the OpenSSL Project
 *    for use in the OpenSSL Toolkit (http://www.openssl.org/)"
 *
 * THIS SOFTWARE IS PROVIDED BY THE OpenSSL PROJECT ``AS IS'' AND ANY
 * EXPRESSED OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE OpenSSL PROJECT OR
 * ITS CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
 * OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#include "../e_os.h"
#include "ssl_tests.h"
#ifdef OPENSSL_SYS_WINDOWS
#include <stdio.h>
#include <string.h>
#endif

#include <openssl/opensslconf.h>
#include <openssl/evp.h>
#ifndef OPENSSL_NO_ENGINE
#include <openssl/engine.h>
#endif
#include <openssl/err.h>
#include <openssl/conf.h>

static void hexdump(TINYCLR_SSL_FILE *f,const char *title,const unsigned char *s,int l)
    {
    int n=0;

    TINYCLR_SSL_PRINTF("%s",title);
    for( ; n < l ; ++n)
	{
	if((n%16) == 0)
	    TINYCLR_SSL_PRINTF("\n%04x",n);
	TINYCLR_SSL_PRINTF(" %02x",s[n]);
	}
    TINYCLR_SSL_PRINTF("\n");
    }

static int convert(unsigned char *s)
    {
    unsigned char *d;

    for(d=s ; *s ; s+=2,++d)
	{
	unsigned int n;

	if(!s[1])
	    {
	    TINYCLR_SSL_PRINTF("Odd number of hex digits!");
	    return(4);
	    }
	sscanf((char *)s,"%2x",&n);
	*d=(unsigned char)n;
	}
    return s-d;
    }

static char *sstrsep(char **string, const char *delim)
    {
    char isdelim[256];
    char *token = *string;

    if (**string == 0)
        return NULL;

    TINYCLR_SSL_MEMSET(isdelim, 0, 256);
    isdelim[0] = 1;

    while (*delim)
        {
        isdelim[(unsigned char)(*delim)] = 1;
        delim++;
        }

    while (!isdelim[(unsigned char)(**string)])
        {
        (*string)++;
        }

    if (**string)
        {
        **string = 0;
        (*string)++;
        }

    return token;
    }

static unsigned char *ustrsep(char **p,const char *sep)
    { return (unsigned char *)sstrsep(p,sep); }

static int test1_exit(int ec)
	{
	return(ec);
	}

static void test1(const EVP_CIPHER *c,const unsigned char *key,int kn,
		  const unsigned char *iv,int in,
		  const unsigned char *plaintext,int pn,
		  const unsigned char *ciphertext,int cn,
		  int encdec)
    {
    EVP_CIPHER_CTX ctx;
    unsigned char out[4096];
    int outl,outl2;

    TINYCLR_SSL_PRINTF("Testing cipher %s%s\n",EVP_CIPHER_name(c),
	   (encdec == 1 ? "(encrypt)" : (encdec == 0 ? "(decrypt)" : "(encrypt/decrypt)")));
    hexdump(OPENSSL_TYPE__FILE_STDOUT,"Key",key,kn);
    if(in)
	hexdump(OPENSSL_TYPE__FILE_STDOUT,"IV",iv,in);
    hexdump(OPENSSL_TYPE__FILE_STDOUT,"Plaintext",plaintext,pn);
    hexdump(OPENSSL_TYPE__FILE_STDOUT,"Ciphertext",ciphertext,cn);
    
    if(kn != c->key_len)
	{
	TINYCLR_SSL_PRINTF("Key length doesn't match, got %d expected %lu\n",kn,
		(unsigned long)c->key_len);
	test1_exit(5);
	}
    EVP_CIPHER_CTX_init(&ctx);
    if (encdec != 0)
        {
	if(!EVP_EncryptInit_ex(&ctx,c,NULL,key,iv))
	    {
	    TINYCLR_SSL_PRINTF("EncryptInit failed\n");
	    ERR_print_errors_fp(OPENSSL_TYPE__FILE_STDERR);
	    test1_exit(10);
	    }
	EVP_CIPHER_CTX_set_padding(&ctx,0);

	if(!EVP_EncryptUpdate(&ctx,out,&outl,plaintext,pn))
	    {
	    TINYCLR_SSL_PRINTF("Encrypt failed\n");
	    ERR_print_errors_fp(OPENSSL_TYPE__FILE_STDERR);
	    test1_exit(6);
	    }
	if(!EVP_EncryptFinal_ex(&ctx,out+outl,&outl2))
	    {
	    TINYCLR_SSL_PRINTF("EncryptFinal failed\n");
	    ERR_print_errors_fp(OPENSSL_TYPE__FILE_STDERR);
	    test1_exit(7);
	    }

	if(outl+outl2 != cn)
	    {
	    TINYCLR_SSL_PRINTF("Ciphertext length mismatch got %d expected %d\n",
		    outl+outl2,cn);
	    test1_exit(8);
	    }

	if(TINYCLR_SSL_MEMCMP(out,ciphertext,cn))
	    {
	    TINYCLR_SSL_PRINTF("Ciphertext mismatch\n");
	    hexdump(OPENSSL_TYPE__FILE_STDERR,"Got",out,cn);
	    hexdump(OPENSSL_TYPE__FILE_STDERR,"Expected",ciphertext,cn);
	    test1_exit(9);
	    }
	}

    if (encdec <= 0)
        {
	if(!EVP_DecryptInit_ex(&ctx,c,NULL,key,iv))
	    {
	    TINYCLR_SSL_PRINTF("DecryptInit failed\n");
	    ERR_print_errors_fp(OPENSSL_TYPE__FILE_STDERR);
	    test1_exit(11);
	    }
	EVP_CIPHER_CTX_set_padding(&ctx,0);

	if(!EVP_DecryptUpdate(&ctx,out,&outl,ciphertext,cn))
	    {
	    TINYCLR_SSL_PRINTF("Decrypt failed\n");
	    ERR_print_errors_fp(OPENSSL_TYPE__FILE_STDERR);
	    test1_exit(6);
	    }
	if(!EVP_DecryptFinal_ex(&ctx,out+outl,&outl2))
	    {
	    TINYCLR_SSL_PRINTF("DecryptFinal failed\n");
	    ERR_print_errors_fp(OPENSSL_TYPE__FILE_STDERR);
	    test1_exit(7);
	    }

	if(outl+outl2 != pn)
	    {
	    TINYCLR_SSL_PRINTF("Plaintext length mismatch got %d expected %d\n",
		    outl+outl2,pn);
	    test1_exit(8);
	    }

	if(TINYCLR_SSL_MEMCMP(out,plaintext,pn))
	    {
	    TINYCLR_SSL_PRINTF("Plaintext mismatch\n");
	    hexdump(OPENSSL_TYPE__FILE_STDERR,"Got",out,pn);
	    hexdump(OPENSSL_TYPE__FILE_STDERR,"Expected",plaintext,pn);
	    test1_exit(9);
	    }
	}

    EVP_CIPHER_CTX_cleanup(&ctx);

    TINYCLR_SSL_PRINTF("\n");
    }

static int test_cipher(const char *cipher,const unsigned char *key,int kn,
		       const unsigned char *iv,int in,
		       const unsigned char *plaintext,int pn,
		       const unsigned char *ciphertext,int cn,
		       int encdec)
    {
    const EVP_CIPHER *c;

    c=EVP_get_cipherbyname(cipher);
    if(!c)
	return 0;

    test1(c,key,kn,iv,in,plaintext,pn,ciphertext,cn,encdec);

    return 1;
    }

static int test_digest(const char *digest,
		       const unsigned char *plaintext,int pn,
		       const unsigned char *ciphertext, unsigned int cn)
    {
    const EVP_MD *d;
    EVP_MD_CTX ctx;
    unsigned char md[EVP_MAX_MD_SIZE];
    unsigned int mdn;

    d=EVP_get_digestbyname(digest);
    if(!d)
	return 0;

    TINYCLR_SSL_PRINTF("Testing digest %s\n",EVP_MD_name(d));
    hexdump(OPENSSL_TYPE__FILE_STDOUT,"Plaintext",plaintext,pn);
    hexdump(OPENSSL_TYPE__FILE_STDOUT,"Digest",ciphertext,cn);

    EVP_MD_CTX_init(&ctx);
    if(!EVP_DigestInit_ex(&ctx,d, NULL))
	{
	TINYCLR_SSL_PRINTF("DigestInit failed\n");
	ERR_print_errors_fp(OPENSSL_TYPE__FILE_STDERR);
	return(100);
	}
    if(!EVP_DigestUpdate(&ctx,plaintext,pn))
	{
	TINYCLR_SSL_PRINTF("DigestUpdate failed\n");
	ERR_print_errors_fp(OPENSSL_TYPE__FILE_STDERR);
	return(101);
	}
    if(!EVP_DigestFinal_ex(&ctx,md,&mdn))
	{
	TINYCLR_SSL_PRINTF("DigestFinal failed\n");
	ERR_print_errors_fp(OPENSSL_TYPE__FILE_STDERR);
	return(101);
	}
    EVP_MD_CTX_cleanup(&ctx);

    if(mdn != cn)
	{
	TINYCLR_SSL_PRINTF("Digest length mismatch, got %d expected %d\n",mdn,cn);
	return(102);
	}

    if(TINYCLR_SSL_MEMCMP(md,ciphertext,cn))
	{
	TINYCLR_SSL_PRINTF("Digest mismatch\n");
	hexdump(OPENSSL_TYPE__FILE_STDERR,"Got",md,cn);
	hexdump(OPENSSL_TYPE__FILE_STDERR,"Expected",ciphertext,cn);
	return(103);
	}

    TINYCLR_SSL_PRINTF("\n");

    EVP_MD_CTX_cleanup(&ctx);

    return 1;
    }

int ssl_test_evp(int argc,char **argv)
    {
    const char *szTestFile;
    TINYCLR_SSL_FILE *f;

    if(argc != 2)
	{
	TINYCLR_SSL_PRINTF("%s <test file>\n",argv[0]);
	return(1);
	}
    CRYPTO_malloc_debug_init();
    CRYPTO_set_mem_debug_options(V_CRYPTO_MDEBUG_ALL);
    CRYPTO_mem_ctrl(CRYPTO_MEM_CHECK_ON);

    szTestFile=argv[1];

    f=TINYCLR_SSL_FOPEN(szTestFile,"r");
    if(!f)
	{
	TINYCLR_SSL_PERROR(szTestFile);
	return(2);
	}

    /* Load up the software EVP_CIPHER and EVP_MD definitions */
    OpenSSL_add_all_ciphers();
    OpenSSL_add_all_digests();
#ifndef OPENSSL_NO_ENGINE
    /* Load all compiled-in ENGINEs */
    ENGINE_load_builtin_engines();
#endif
#if 0
    OPENSSL_config();
#endif
#ifndef OPENSSL_NO_ENGINE
    /* Register all available ENGINE implementations of ciphers and digests.
     * This could perhaps be changed to "ENGINE_register_all_complete()"? */
    ENGINE_register_all_ciphers();
    ENGINE_register_all_digests();
    /* If we add command-line options, this statement should be switchable.
     * It'll prevent ENGINEs being ENGINE_init()ialised for cipher/digest use if
     * they weren't already initialised. */
    /* ENGINE_set_cipher_flags(ENGINE_CIPHER_FLAG_NOINIT); */
#endif

    for( ; ; )
	{
	char line[4096];
	char *p;
	char *cipher;
	unsigned char *iv,*key,*plaintext,*ciphertext;
	int encdec;
	int kn,in,pn,cn;

	if(!TINYCLR_SSL_FGETS((char *)line,sizeof line,f))
	    break;
	if(line[0] == '#' || line[0] == '\n')
	    continue;
	p=line;
	cipher=sstrsep(&p,":");	
	key=ustrsep(&p,":");
	iv=ustrsep(&p,":");
	plaintext=ustrsep(&p,":");
	ciphertext=ustrsep(&p,":");
	if (p[-1] == '\n') {
	    p[-1] = '\0';
	    encdec = -1;
	} else {
	    encdec = atoi(sstrsep(&p,"\n"));
	}
	      

	kn=convert(key);
	in=convert(iv);
	pn=convert(plaintext);
	cn=convert(ciphertext);

	if(!test_cipher(cipher,key,kn,iv,in,plaintext,pn,ciphertext,cn,encdec)
	   && !test_digest(cipher,plaintext,pn,ciphertext,cn))
	    {
#ifdef OPENSSL_NO_AES
	    if (strstr(cipher, "AES") == cipher)
		{
		TINYCLR_SSL_PRINTF( "Cipher disabled, skipping %s\n", cipher); 
		continue;
		}
#endif
#ifdef OPENSSL_NO_DES
	    if (strstr(cipher, "DES") == cipher)
		{
		TINYCLR_SSL_PRINTF( "Cipher disabled, skipping %s\n", cipher); 
		continue;
		}
#endif
#ifdef OPENSSL_NO_RC4
	    if (strstr(cipher, "RC4") == cipher)
		{
		TINYCLR_SSL_PRINTF( "Cipher disabled, skipping %s\n", cipher); 
		continue;
		}
#endif
#ifdef OPENSSL_NO_CAMELLIA
	    if (strstr(cipher, "CAMELLIA") == cipher)
		{
		TINYCLR_SSL_PRINTF( "Cipher disabled, skipping %s\n", cipher); 
		continue;
		}
#endif
#ifdef OPENSSL_NO_SEED
	    if (strstr(cipher, "SEED") == cipher)
		{
		TINYCLR_SSL_PRINTF( "Cipher disabled, skipping %s\n", cipher); 
		continue;
		}
#endif
	    TINYCLR_SSL_PRINTF("Can't find %s\n",cipher);
	    return(3);
	    }
	}

#ifndef OPENSSL_NO_ENGINE
    ENGINE_cleanup();
#endif
    EVP_cleanup();
    CRYPTO_cleanup_all_ex_data();
    ERR_remove_thread_state(NULL);
    ERR_free_strings();
    CRYPTO_mem_leaks_fp(OPENSSL_TYPE__FILE_STDERR);

    return 0;
    }

