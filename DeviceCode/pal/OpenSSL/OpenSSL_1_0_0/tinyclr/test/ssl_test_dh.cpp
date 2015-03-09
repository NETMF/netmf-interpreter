/* Copyright (C) 1995-1998 Eric Young (eay@cryptsoft.com)
 * All rights reserved.
 *
 * This package is an SSL implementation written
 * by Eric Young (eay@cryptsoft.com).
 * The implementation was written so as to conform with Netscapes SSL.
 * 
 * This library is free for commercial and non-commercial use as long as
 * the following conditions are aheared to.  The following conditions
 * apply to all code found in this distribution, be it the RC4, RSA,
 * lhash, DES, etc., code; not just the SSL code.  The SSL documentation
 * included with this distribution is covered by the same copyright terms
 * except that the holder is Tim Hudson (tjh@cryptsoft.com).
 * 
 * Copyright remains Eric Young's, and as such any Copyright notices in
 * the code are not to be removed.
 * If this package is used in a product, Eric Young should be given attribution
 * as the author of the parts of the library used.
 * This can be in the form of a textual message at program startup or
 * in documentation (online or textual) provided with the package.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. All advertising materials mentioning features or use of this software
 *    must display the following acknowledgement:
 *    "This product includes cryptographic software written by
 *     Eric Young (eay@cryptsoft.com)"
 *    The word 'cryptographic' can be left out if the rouines from the library
 *    being used are not cryptographic related :-).
 * 4. If you include any Windows specific code (or a derivative thereof) from 
 *    the apps directory (application code) you must include an acknowledgement:
 *    "This product includes software written by Tim Hudson (tjh@cryptsoft.com)"
 * 
 * THIS SOFTWARE IS PROVIDED BY ERIC YOUNG ``AS IS'' AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED.  IN NO EVENT SHALL THE AUTHOR OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 * 
 * The licence and distribution terms for any publically available version or
 * derivative of this code cannot be changed.  i.e. this code cannot simply be
 * copied and put under another distribution licence
 * [including the GNU Public Licence.]
 */

/* Until the key-gen callbacks are modified to use newer prototypes, we allow
 * deprecated functions for openssl-internal code */
#ifdef OPENSSL_NO_DEPRECATED
#undef OPENSSL_NO_DEPRECATED
#endif
#include "ssl_tests.h"
#include "../e_os.h"
#ifdef OPENSSL_SYS_WINDOWS
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#endif


#include <openssl/crypto.h>
#include <openssl/bio.h>
#include <openssl/bn.h>
#include <openssl/rand.h>
#include <openssl/err.h>

#ifdef OPENSSL_NO_DH
int ssl_test_dh(int argc, char *argv[])
{
    TINYCLR_SSL_PRINTF("No DH support\n");
    return(0);
}
#else
#include <openssl/dh.h>

#ifdef OPENSSL_SYS_WIN16
#define MS_CALLBACK	_far _loadds
#else
#define MS_CALLBACK
#endif

static int MS_CALLBACK cb(int p, int n, BN_GENCB *arg);

static const char rnd_seed[] = "string to make the random number generator think it has entropy";

int ssl_test_dh(int argc, char *argv[])
	{
	BN_GENCB _cb;
	DH *a;
	DH *b=NULL;
	char buf[12];
	unsigned char *abuf=NULL,*bbuf=NULL;
	int i,alen,blen,aout,bout,ret=1;
	BIO *out;

	CRYPTO_malloc_debug_init();
	CRYPTO_dbg_set_options(V_CRYPTO_MDEBUG_ALL);
	CRYPTO_mem_ctrl(CRYPTO_MEM_CHECK_ON);

#ifdef OPENSSL_SYS_WIN32
	CRYPTO_malloc_init();
#endif

	RAND_seed(rnd_seed, sizeof rnd_seed);

#ifndef OPENSSL_SYS_WINDOWS
		out = BIO_new(BIO_s_mem());
		if (out == NULL) return(1);
	
#else
		out=BIO_new(BIO_s_file());
		if (out == NULL) EXIT(1);
		BIO_set_fp(out,OPENSSL_TYPE__FILE_STDOUT,BIO_NOCLOSE);
#endif

	BN_GENCB_set(&_cb, &cb, out);
	if(((a = DH_new()) == NULL) || !DH_generate_parameters_ex(a, 64,
				DH_GENERATOR_5, &_cb))
		goto err;

	if (!DH_check(a, &i)) goto err;
	if (i & DH_CHECK_P_NOT_PRIME)
		TINYCLR_SSL_PRINTF( "p value is not prime\n");
	if (i & DH_CHECK_P_NOT_SAFE_PRIME)
		TINYCLR_SSL_PRINTF( "p value is not a safe prime\n");
	if (i & DH_UNABLE_TO_CHECK_GENERATOR)
		TINYCLR_SSL_PRINTF( "unable to check the generator value\n");
	if (i & DH_NOT_SUITABLE_GENERATOR)
		TINYCLR_SSL_PRINTF( "the g value is not a generator\n");

	TINYCLR_SSL_PRINTF("\np    =");
	BN_print(out,a->p);
	TINYCLR_SSL_PRINTF("\ng    =");
	BN_print(out,a->g);
	TINYCLR_SSL_PRINTF("\n");

	b=DH_new();
	if (b == NULL) goto err;

	b->p=BN_dup(a->p);
	b->g=BN_dup(a->g);
	if ((b->p == NULL) || (b->g == NULL)) goto err;

	/* Set a to run with normal modexp and b to use constant time */
	a->flags &= ~DH_FLAG_NO_EXP_CONSTTIME;
	b->flags |= DH_FLAG_NO_EXP_CONSTTIME;

	if (!DH_generate_key(a)) goto err;
	TINYCLR_SSL_PRINTF("pri 1=");
	BN_print(out,a->priv_key);
	TINYCLR_SSL_PRINTF("\npub 1=");
	BN_print(out,a->pub_key);
	TINYCLR_SSL_PRINTF("\n");

	if (!DH_generate_key(b)) goto err;
	TINYCLR_SSL_PRINTF("pri 2=");
	BN_print(out,b->priv_key);
	TINYCLR_SSL_PRINTF("\npub 2=");
	BN_print(out,b->pub_key);
	TINYCLR_SSL_PRINTF("\n");

	alen=DH_size(a);
	abuf=(unsigned char *)OPENSSL_malloc(alen);
	aout=DH_compute_key(abuf,b->pub_key,a);

	TINYCLR_SSL_PRINTF("key1 =");
	for (i=0; i<aout; i++)
		{
		TINYCLR_SSL_SNPRINTF(buf,sizeof(buf),"%02X",abuf[i]);
		TINYCLR_SSL_PRINTF(buf);
		}
	TINYCLR_SSL_PRINTF("\n");

	blen=DH_size(b);
	bbuf=(unsigned char *)OPENSSL_malloc(blen);
	bout=DH_compute_key(bbuf,a->pub_key,b);

	TINYCLR_SSL_PRINTF("key2 =");
	for (i=0; i<bout; i++)
		{
		TINYCLR_SSL_SNPRINTF(buf,sizeof(buf),"%02X",bbuf[i]);
		TINYCLR_SSL_PRINTF(buf);
		}
	TINYCLR_SSL_PRINTF("\n");
	if ((aout < 4) || (bout != aout) || (TINYCLR_SSL_MEMCMP(abuf,bbuf,aout) != 0))
		{
		TINYCLR_SSL_PRINTF("Error in DH routines\n");
		ret=1;
		}
	else
		ret=0;
err:
	ERR_print_errors_fp(OPENSSL_TYPE__FILE_STDERR);

	if (abuf != NULL) OPENSSL_free(abuf);
	if (bbuf != NULL) OPENSSL_free(bbuf);
	if(b != NULL) DH_free(b);
	if(a != NULL) DH_free(a);
	BIO_free(out);
#ifdef OPENSSL_SYS_NETWARE
    if (ret) TINYCLR_SSL_PRINTF("ERROR: %d\n", ret);
#endif
	return(ret);
	}

static int MS_CALLBACK cb(int p, int n, BN_GENCB *arg)
	{
	char c='*';

	if (p == 0) c='.';
	if (p == 1) c='+';
	if (p == 2) c='*';
	if (p == 3) c='\n';
	BIO_write((BIO*)arg->arg,&c,1);
	(void)BIO_flush((BIO*)arg->arg);
#ifdef LINT
	p=n;
#endif
	return 1;
	}
#endif

