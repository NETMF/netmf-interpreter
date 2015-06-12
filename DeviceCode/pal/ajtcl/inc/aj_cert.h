#ifndef _AJ_CERT_H
#define _AJ_CERT_H

/**
 * @file aj_cert.h
 * @defgroup aj_cert Certificate Utilities
 * @{
 */

/******************************************************************************
 * Copyright AllSeen Alliance. All rights reserved.
 *
 *    Permission to use, copy, modify, and/or distribute this software for any
 *    purpose with or without fee is hereby granted, provided that the above
 *    copyright notice and this permission notice appear in all copies.
 *
 *    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 *    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 *    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 *    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 *    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 *    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 *    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 ******************************************************************************/

#include "aj_target.h"
#include "aj_status.h"
#include "aj_crypto_ecc.h"

#ifdef __cplusplus
extern "C" {
#endif

/**
 * DER encoding types.
 */
#define ASN_BOOLEAN          0x01
#define ASN_INTEGER          0x02
#define ASN_BITS             0x03
#define ASN_OCTETS           0x04
#define ASN_NULL             0x05
#define ASN_OID              0x06
#define ASN_UTF8             0x0C
#define ASN_SEQ              0x10
#define ASN_SET_OF           0x11
#define ASN_PRINTABLE        0x13
#define ASN_ASCII            0x16
#define ASN_UTC_TIME         0x17
#define ASN_GEN_TIME         0x18
#define ASN_CONTEXT_SPECIFIC 0xA0

/**
 * Structure for a DER encoded element.
 */
typedef struct _DER_Element {
    size_t size;
    uint8_t* data;
} DER_Element;

/**
 * Decode one element from a DER encoded blob.
 *
 * @param der The input DER encoded blob.
 * @param tag The expected element type.
 * @param out The output decoded element.
 *
 * @return  Return AJ_Status
 *          - AJ_OK on success
 *          - AJ_ERR_INVALID on all failures
 */
AJ_Status AJ_ASN1DecodeElement(DER_Element* der, uint8_t tag, DER_Element* out);

/**
 * Decode many elements from a DER encoded blob.
 * This is a non-recursive decoder.
 * Only a depth of one may be decoded in one call.
 *
 * @param der  The input DER encoded blob.
 * @param tags The expected element types.
 * @param len  The number of types to decode.
 * @param ...  The output decoded elements.
 *
 * @return  Return AJ_Status
 *          - AJ_OK on success
 *          - AJ_ERR_INVALID on all failures
 */
AJ_Status AJ_ASN1DecodeElements(DER_Element* der, const uint8_t* tags, size_t len, ...);

/**
 * OIDs used in X.509 certificates.
 */
extern const uint8_t OID_SIG_ECDSA_SHA256[];
extern const uint8_t OID_KEY_ECC[];
extern const uint8_t OID_CRV_PRIME256V1[];
extern const uint8_t OID_DN_OU[];
extern const uint8_t OID_DN_CN[];
extern const uint8_t OID_BASIC_CONSTRAINTS[];

typedef struct _X509Validity {
    uint64_t from;
    uint64_t to;
} X509Validity;

typedef struct _X509DistinguishedName {
    DER_Element ou;                      /**< Organizational Unit name */
    DER_Element cn;                      /**< Common name */
} X509DistinguishedName;

typedef struct _X509Extensions {
    uint32_t ca;                         /**< Certificate authority */
} X509Extensions;

typedef struct _X509TbsCertificate {
    DER_Element serial;                  /**< The serial number */
    X509DistinguishedName issuer;        /**< The issuer's identity */
    X509Validity validity;               /**< The validity period */
    X509DistinguishedName subject;       /**< The subject's identity */
    ecc_publickey publickey;             /**< The subject's public key */
    X509Extensions extensions;           /**< The certificate extensions */
} X509TbsCertificate;

/**
 * Structure for X.509 certificate.
 * Only useful for NISTP256 ECDSA signed certificates at the moment.
 * Can be modified to handle other types in the future.
 */
typedef struct _X509Certificate {
    DER_Element der;                     /**< Certificate DER encoding */
    DER_Element raw;                     /**< The raw tbs section */
    X509TbsCertificate tbs;              /**< The TBS section of the certificate */
    ecc_signature signature;             /**< The certificate signature */
} X509Certificate;

/**
 * Certificate chain: linked list of certificates
 */
typedef struct _X509CertificateChain {
    X509Certificate certificate;         /**< The certificate */
    struct _X509CertificateChain* next;  /**< Linked list pointer */
} X509CertificateChain;

/**
 * Decode a PEM encoded private key
 *
 * @param key         The output decoded key.
 * @param pem         The input PEM.
 *
 * @return  Return AJ_Status
 *          - AJ_OK on success
 *          - AJ_ERR_INVALID on all failures
 */
AJ_Status AJ_DecodePrivateKeyPEM(ecc_privatekey* key, const char* pem);

/**
 * Decode a ASN.1 DER encoded X.509 certificate.
 *
 * @param certificate The output decoded certificate.
 * @param der         The input encoded DER blob.
 *
 * @return  Return AJ_Status
 *          - AJ_OK on success
 *          - AJ_ERR_INVALID on all failures
 */
AJ_Status AJ_X509DecodeCertificateDER(X509Certificate* certificate, DER_Element* der);

/**
 * Decode a PEM encoded X.509 certificate.
 *
 * @param certificate The output decoded certificate.
 * @param pem         The input PEM.
 * @param len         The input PEM length.
 *
 * @return  Return AJ_Status
 *          - AJ_OK on success
 *          - AJ_ERR_RESOURCES on failure
 */
AJ_Status AJ_X509DecodeCertificatePEM(X509Certificate* certificate, const char* pem, size_t len);

/**
 * Decode a PEM encoded X.509 certificate chain.
 * The order of certificates is important.
 * This puts the child first, then parents follow.
 * That is the same order that should be in the pem.
 *
 * @param pem         The input PEM.
 *
 * @return  Return chain on success, NULL on failure
 */
X509CertificateChain* AJ_X509DecodeCertificateChainPEM(const char* pem);

/**
 * Verify a self-signed X.509 certificate.
 *
 * @param certificate The input certificate.
 *
 * @return  Return AJ_Status
 *          - AJ_OK on success
 *          - AJ_ERR_SECURITY on failure
 */
AJ_Status AJ_X509SelfVerify(const X509Certificate* certificate);

/**
 * Verify a signed X.509 certificate.
 *
 * @param certificate The input certificate.
 * @param key         The verification key.
 *
 * @return  Return AJ_Status
 *          - AJ_OK on success
 *          - AJ_ERR_SECURITY on failure
 */
AJ_Status AJ_X509Verify(const X509Certificate* certificate, const ecc_publickey* key);

/**
 * Verify a chain of X.509 certificates.
 * Root certificate is first.
 *
 * @param chain       The input certificate chain.
 * @param key         The verification key of the root. If this is NULL, don't verify the root.
 *
 * @return  Return AJ_Status
 *          - AJ_OK on success
 *          - AJ_ERR_SECURITY on failure
 */
AJ_Status AJ_X509VerifyChain(const X509CertificateChain* chain, const ecc_publickey* key);

/**
 * Convert unsigned 32-bit int array to network order (big endian) bytes.
 *
 * @param u32  Unsigned 32-bit array
 * @param len  Length of 32-bit array
 * @param u8   Unsigned 8-bit array
 *
 */
void HostU32ToBigEndianU8(uint32_t* u32, size_t len, uint8_t* u8);

/**
 * Old encoding of native public key.
 *
 * @param publickey  The ECC public key
 * @param[out] b8    Big endian byte array
 *
 */
void AJ_BigEndianEncodePublicKey(ecc_publickey* publickey, uint8_t* b8);

/**
 * Old decoding of native public key.
 *
 * @param[out] publickey  The ECC public key
 * @param b8              Big endian byte array
 *
 */
void AJ_BigEndianDecodePublicKey(ecc_publickey* publickey, uint8_t* b8);

#ifdef __cplusplus
}
#endif
/**
 * @}
 */
#endif
