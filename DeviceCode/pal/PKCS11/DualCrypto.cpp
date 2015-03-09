#include "CryptokiPAL.h"

/* Dual-function cryptographic operations */

/* C_DigestEncryptUpdate continues a multiple-part digesting
 * and encryption operation. */
CK_DEFINE_FUNCTION(CK_RV, C_DigestEncryptUpdate)
(
  CK_SESSION_HANDLE hSession,            /* session's handle */
  CK_BYTE_PTR       pPart,               /* the plaintext data */
  CK_ULONG          ulPartLen,           /* plaintext length */
  CK_BYTE_PTR       pEncryptedPart,      /* gets ciphertext */
  CK_ULONG_PTR      pulEncryptedPartLen  /* gets c-text length */
)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_DecryptDigestUpdate continues a multiple-part decryption and
 * digesting operation. */
CK_DEFINE_FUNCTION(CK_RV, C_DecryptDigestUpdate)
(
  CK_SESSION_HANDLE hSession,            /* session's handle */
  CK_BYTE_PTR       pEncryptedPart,      /* ciphertext */
  CK_ULONG          ulEncryptedPartLen,  /* ciphertext length */
  CK_BYTE_PTR       pPart,               /* gets plaintext */
  CK_ULONG_PTR      pulPartLen           /* gets plaintext len */
)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_SignEncryptUpdate continues a multiple-part signing and
 * encryption operation. */
CK_DEFINE_FUNCTION(CK_RV, C_SignEncryptUpdate)
(
  CK_SESSION_HANDLE hSession,            /* session's handle */
  CK_BYTE_PTR       pPart,               /* the plaintext data */
  CK_ULONG          ulPartLen,           /* plaintext length */
  CK_BYTE_PTR       pEncryptedPart,      /* gets ciphertext */
  CK_ULONG_PTR      pulEncryptedPartLen  /* gets c-text length */
)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_DecryptVerifyUpdate continues a multiple-part decryption and
 * verify operation. */
CK_DEFINE_FUNCTION(CK_RV, C_DecryptVerifyUpdate)
(
  CK_SESSION_HANDLE hSession,            /* session's handle */
  CK_BYTE_PTR       pEncryptedPart,      /* ciphertext */
  CK_ULONG          ulEncryptedPartLen,  /* ciphertext length */
  CK_BYTE_PTR       pPart,               /* gets plaintext */
  CK_ULONG_PTR      pulPartLen           /* gets p-text length */
)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}


