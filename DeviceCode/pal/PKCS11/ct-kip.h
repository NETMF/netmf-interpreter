/* ct-kip.h include file for the PKCS #11 Mechanisms for the
 * Cryptographic Token Key Initialization Protocol OTPS document.
 */

/* $Revision: 1.3 $ */

/* License to copy and use this software is granted provided that it is
 * identified as "RSA Security Inc. Cryptographic Token Key Initialization
 * Protocol (CT-KIP)" in all material mentioning or referencing this software.

 * RSA Security Inc. makes no representations concerning either the 
 * merchantability of this software or the suitability of this software for
 * any particular purpose. It is provided "as is" without express or implied
 * warranty of any kind.
 */

/* This file is preferably included after inclusion of pkcs11.h */

#ifndef _CT_KIP_H_
#define _CT_KIP_H_ 1

/* Are the definitions of this file already included in pkcs11t.h? */
#ifndef CKM_KIP_DERIVE

#ifdef __cplusplus
extern "C" {
#endif

/* Mechanism Identifiers */
#define CKM_KIP_DERIVE	    0x00000510
#define CKM_KIP_WRAP	    0x00000511
#define CKM_KIP_MAC	    0x00000512

/* Structures */
typedef struct CK_KIP_PARAMS {
    CK_MECHANISM_PTR  pMechanism;
    CK_OBJECT_HANDLE  hKey;
    CK_BYTE_PTR       pSeed;
    CK_ULONG          ulSeedLen;
} CK_KIP_PARAMS;

typedef CK_KIP_PARAMS CK_PTR CK_KIP_PARAMS_PTR;

#ifdef __cplusplus
}
#endif

#endif

#endif

