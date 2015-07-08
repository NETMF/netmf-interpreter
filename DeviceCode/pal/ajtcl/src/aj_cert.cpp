/**
 * @file
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

/**
 * Per-module definition of the current module for debug logging.  Must be defined
 * prior to first inclusion of aj_debug.h
 */
#define AJ_MODULE CERTIFICATE

#include <stdarg.h>
#include "aj_debug.h"
#include "aj_cert.h"
#include "aj_util.h"

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgCERTIFICATE = 0;
#endif

/**
 * PEM encoding tags
 */
#define PEM_PRIV_BEG "-----BEGIN EC PRIVATE KEY-----"
#define PEM_PRIV_END "-----END EC PRIVATE KEY-----"
#define PEM_CERT_BEG "-----BEGIN CERTIFICATE-----"
#define PEM_CERT_END "-----END CERTIFICATE-----"

void HostU32ToBigEndianU8(uint32_t* u32, size_t len, uint8_t* u8)
{
    uint32_t x;
    size_t i;

    for (i = 0; i < len; i += sizeof (uint32_t)) {
        x = u32[i / sizeof (uint32_t)];
#if HOST_IS_LITTLE_ENDIAN
        x = AJ_ByteSwap32(x);
#endif
        memcpy(&u8[i], &x, sizeof (x));
    }
}

static void BigEndianU8ToHostU32(uint8_t* u8, uint32_t* u32, size_t len)
{
    uint32_t x;
    size_t i;

    for (i = 0; i < len; i += sizeof (uint32_t)) {
        memcpy(&x, &u8[i], sizeof (x));
#if HOST_IS_LITTLE_ENDIAN
        x = AJ_ByteSwap32(x);
#endif
        u32[i / sizeof (uint32_t)] = x;
    }
}

void AJ_BigEndianEncodePublicKey(ecc_publickey* publickey, uint8_t* b8)
{
    HostU32ToBigEndianU8((uint32_t*) publickey, sizeof (ecc_publickey), b8);
}

void AJ_BigEndianDecodePublicKey(ecc_publickey* publickey, uint8_t* b8)
{
    BigEndianU8ToHostU32(b8, (uint32_t*) publickey, sizeof (ecc_publickey));
}

static AJ_Status ASN1DecodeLength(DER_Element* der, DER_Element* out)
{
    size_t n;
    size_t len;

    if ((NULL == der->data) || (0 == der->size)) {
        return AJ_ERR_INVALID;
    }

    len = *(der->data)++;
    der->size--;
    if (0x80 & len) {
        n = len & 0x7F;
        if (n > sizeof (size_t)) {
            return AJ_ERR_INVALID;
        }
        len = 0;
        while (n && der->size) {
            len = (len << 8) + *(der->data)++;
            n--;
            der->size--;
        }
    }
    if (len > der->size) {
        return AJ_ERR_INVALID;
    }
    out->size = len;
    out->data = der->data;

    return AJ_OK;
}

/*
 * Currently, only UTF8String is supported in AJ certificates, so binary equivalence
 * is sufficient. If other ASN.1 string types are ever supported, make sure
 * DNs are stored internally in a canonical form that can still be checked for
 * binary equivalence, or this function will need to be updated to do the right things.
   . See RFC 5280 section 7.1 and RFC 4518 for equivalence between different string types.
 */
static uint32_t AJ_X509CompareNames(const X509DistinguishedName a, const X509DistinguishedName b)
{
    /* Only OU and CN are supported as elements in a DN in AllJoyn */
    if (a.ou.size != b.ou.size ||
        a.cn.size != b.cn.size) {
        return FALSE;
    }

    if (0 != memcmp(a.ou.data, b.ou.data, a.ou.size) ||
        0 != memcmp(a.cn.data, b.cn.data, a.cn.size)) {
        return FALSE;
    }

    return TRUE;

}

AJ_Status AJ_ASN1DecodeElement(DER_Element* der, uint8_t tag, DER_Element* out)
{
    uint8_t tmp;

    if ((NULL == der) || (NULL == out)) {
        return AJ_ERR_INVALID;
    }
    if ((NULL == der->data) || (0 == der->size)) {
        return AJ_ERR_INVALID;
    }

    /*
     * Decode tag and check it is what we expect
     */
    tmp = *(der->data)++;
    der->size--;
    if (ASN_CONTEXT_SPECIFIC != (tmp & ASN_CONTEXT_SPECIFIC)) {
        tmp &= 0x1F;
    }
    if (tmp != tag) {
        AJ_InfoPrintf(("AJ_ASN1DecodeElement(der=%p, tag=%x, out=%p): Tag error %x\n", der, tag, out, tmp));
        return AJ_ERR_INVALID;
    }
    /*
     * Decode size
     */
    if (AJ_OK != ASN1DecodeLength(der, out)) {
        AJ_InfoPrintf(("AJ_ASN1DecodeElement(der=%p, tag=%x, out=%p): Length error\n", der, tag, out));
        return AJ_ERR_INVALID;
    }
    der->data += out->size;
    der->size -= out->size;

    return AJ_OK;
}

AJ_Status AJ_ASN1DecodeElements(DER_Element* der, const uint8_t* tags, size_t len, ...)
{
    AJ_Status status = AJ_OK;
    DER_Element* out;
    va_list argp;
    uint8_t tag;
    uint32_t tmp;

    AJ_InfoPrintf(("AJ_ASN1DecodeElements(der=%p, tags=%p, len=%zu)\n", der, tags, len));

    if ((NULL == der) || (NULL == tags)) {
        return AJ_ERR_INVALID;
    }

    va_start(argp, len);
    while ((AJ_OK == status) && len && (der->size)) {
        tag = *tags++;
        if (ASN_CONTEXT_SPECIFIC == tag) {
            tmp = va_arg(argp, uint32_t);
            tag = (ASN_CONTEXT_SPECIFIC | tmp);
        }
        out = va_arg(argp, DER_Element*);
        len--;
        status = AJ_ASN1DecodeElement(der, tag, out);
    }
    if (AJ_OK == status) {
        // If unset elements, fail
        if (len) {
            AJ_InfoPrintf(("AJ_ASN1DecodeElements(der=%p, tags=%p, len=%zu): Uninitialized elements\n", der, tags, len));
            status = AJ_ERR_INVALID;
        }
    }
    va_end(argp);

    return status;
}

// 1.2.840.10045.4.3.2
const uint8_t OID_SIG_ECDSA_SHA256[]  = { 0x2A, 0x86, 0x48, 0xCE, 0x3D, 0x04, 0x03, 0x02 };
// 1.2.840.10045.2.1
const uint8_t OID_KEY_ECC[]           = { 0x2A, 0x86, 0x48, 0xCE, 0x3D, 0x02, 0x01 };
// 1.2.840.10045.3.1.7
const uint8_t OID_CRV_PRIME256V1[]    = { 0x2A, 0x86, 0x48, 0xCE, 0x3D, 0x03, 0x01, 0x07 };
// 2.5.4.10
const uint8_t OID_DN_OU[]             = { 0x55, 0x04, 0x0B };
// 2.5.4.3
const uint8_t OID_DN_CN[]             = { 0x55, 0x04, 0x03 };
// 2.5.29.19
const uint8_t OID_BASIC_CONSTRAINTS[] = { 0x55, 0x1D, 0x13 };

uint8_t CompareOID(DER_Element* der, const uint8_t* oid, size_t len)
{
    if (der->size != len) {
        return 0;
    }
    return (0 == memcmp(der->data, oid, len));
}

AJ_Status AJ_DecodePrivateKeyDER(ecc_privatekey* key, DER_Element* der)
{
    AJ_Status status;
    DER_Element seq;
    DER_Element ver;
    DER_Element prv;
    DER_Element alg;
    const uint8_t tags1[] = { ASN_SEQ };
    const uint8_t tags2[] = { ASN_INTEGER, ASN_OCTETS, ASN_CONTEXT_SPECIFIC };

    status = AJ_ASN1DecodeElements(der, tags1, sizeof (tags1), &seq);
    if (AJ_OK != status) {
        return status;
    }
    status = AJ_ASN1DecodeElements(&seq, tags2, sizeof (tags2), &ver, &prv, 0, &alg);
    if (AJ_OK != status) {
        return status;
    }
    if ((1 != ver.size) || (1 != *ver.data)) {
        return AJ_ERR_INVALID;
    }
    if (KEY_ECC_PRV_SZ != prv.size) {
        return AJ_ERR_INVALID;
    }
    AJ_BigvalDecode(prv.data, key, KEY_ECC_SZ);

    return status;
}

AJ_Status AJ_DecodePrivateKeyPEM(ecc_privatekey* key, const char* pem)
{
    AJ_Status status;
    const char* beg;
    const char* end;
    DER_Element der;
    uint8_t* buf = NULL;

    beg = strstr(pem, PEM_PRIV_BEG);
    if (NULL == beg) {
        return AJ_ERR_INVALID;
    }
    beg = pem + strlen(PEM_PRIV_BEG);
    end = strstr(beg, PEM_PRIV_END);
    if (NULL == end) {
        return AJ_ERR_INVALID;
    }

    der.size = 3 * (end - beg) / 4;
    der.data = (uint8_t*)AJ_Malloc(der.size);
    if (NULL == der.data) {
        return AJ_ERR_RESOURCES;
    }
    buf = der.data;
    status = AJ_B64ToRaw(beg, end - beg, der.data, der.size);
    if (AJ_OK != status) {
        goto Exit;
    }
    if ('=' == beg[end - beg - 1]) {
        der.size--;
    }
    if ('=' == beg[end - beg - 2]) {
        der.size--;
    }
    status = AJ_DecodePrivateKeyDER(key, &der);

Exit:
    if (buf) {
        AJ_Free(buf);
    }

    return status;
}

static AJ_Status DecodeCertificateName(X509DistinguishedName* dn, DER_Element* der)
{
    AJ_Status status = AJ_OK;
    DER_Element set;
    DER_Element seq;
    DER_Element oid;
    DER_Element tmp;

    memset(dn, 0, sizeof (X509DistinguishedName));

    while ((AJ_OK == status) && (der->size)) {
        status = AJ_ASN1DecodeElement(der, ASN_SET_OF, &set);
        if (AJ_OK != status) {
            return status;
        }
        status = AJ_ASN1DecodeElement(&set, ASN_SEQ, &seq);
        if (AJ_OK != status) {
            return status;
        }
        status = AJ_ASN1DecodeElement(&seq, ASN_OID, &oid);
        if (AJ_OK != status) {
            return status;
        }
        if (CompareOID(&oid, OID_DN_OU, sizeof (OID_DN_OU))) {
            // Only accept UTF8 strings
            status = AJ_ASN1DecodeElement(&seq, ASN_UTF8, &tmp);
            if (AJ_OK != status) {
                return status;
            }
            dn->ou.data = tmp.data;
            dn->ou.size = tmp.size;
        } else if (CompareOID(&oid, OID_DN_CN, sizeof (OID_DN_CN))) {
            // Only accept UTF8 strings
            status = AJ_ASN1DecodeElement(&seq, ASN_UTF8, &tmp);
            if (AJ_OK != status) {
                return status;
            }
            dn->cn.data = tmp.data;
            dn->cn.size = tmp.size;
        }
    }

    return status;
}

static AJ_Status DecodeCertificateTime(X509Validity* validity, DER_Element* der)
{
    AJ_Status status;
    DER_Element time;
    uint8_t fmt;

    memset(validity, 0, sizeof (X509Validity));

    if (!der->size) {
        return AJ_ERR_SECURITY;
    }
    fmt = *der->data;
    switch (fmt) {
    case ASN_UTC_TIME:
        status = AJ_ASN1DecodeElement(der, ASN_UTC_TIME, &time);
        if (AJ_OK != status) {
            return status;
        }
        validity->from = AJ_DecodeTime((char*) time.data, "%y%m%d%H%M%SZ");
        break;

    case ASN_GEN_TIME:
        status = AJ_ASN1DecodeElement(der, ASN_GEN_TIME, &time);
        if (AJ_OK != status) {
            return status;
        }
        validity->from = AJ_DecodeTime((char*) time.data, "%Y%m%d%H%M%SZ");
        break;

    default:
        return AJ_ERR_INVALID;
    }

    if (!der->size) {
        return AJ_ERR_SECURITY;
    }
    fmt = *der->data;
    switch (fmt) {
    case ASN_UTC_TIME:
        status = AJ_ASN1DecodeElement(der, ASN_UTC_TIME, &time);
        if (AJ_OK != status) {
            return status;
        }
        validity->to = AJ_DecodeTime((char*) time.data, "%y%m%d%H%M%SZ");
        break;

    case ASN_GEN_TIME:
        status = AJ_ASN1DecodeElement(der, ASN_GEN_TIME, &time);
        if (AJ_OK != status) {
            return status;
        }
        validity->to = AJ_DecodeTime((char*) time.data, "%Y%m%d%H%M%SZ");
        break;

    default:
        return AJ_ERR_INVALID;
    }

    return status;
}

static AJ_Status DecodeCertificatePub(ecc_publickey* pub, DER_Element* der)
{
    AJ_Status status;
    DER_Element seq;
    DER_Element bit;
    DER_Element oid1;
    DER_Element oid2;
    const uint8_t tags1[] = { ASN_SEQ, ASN_BITS };
    const uint8_t tags2[] = { ASN_OID, ASN_OID };

    memset(pub, 0, sizeof (ecc_publickey));

    status = AJ_ASN1DecodeElements(der, tags1, sizeof (tags1), &seq, &bit);
    if (AJ_OK != status) {
        return status;
    }

    /*
     * We only accept NISTP256 ECC keys at the moment.
     */
    status = AJ_ASN1DecodeElements(&seq, tags2, sizeof (tags2), &oid1, &oid2);
    if (AJ_OK != status) {
        return status;
    }
    if (!CompareOID(&oid1, OID_KEY_ECC, sizeof (OID_KEY_ECC))) {
        return AJ_ERR_INVALID;
    }
    if (!CompareOID(&oid2, OID_CRV_PRIME256V1, sizeof (OID_CRV_PRIME256V1))) {
        return AJ_ERR_INVALID;
    }

    /*
     * We only accept uncompressed ECC points.
     */
    if ((2 + KEY_ECC_PUB_SZ) != bit.size) {
        return AJ_ERR_INVALID;
    }
    if ((0x00 != bit.data[0]) || (0x04 != bit.data[1])) {
        return AJ_ERR_INVALID;
    }
    bit.data += 2;
    bit.size -= 2;
    AJ_BigvalDecode(bit.data, &pub->x, KEY_ECC_SZ);
    bit.data += KEY_ECC_SZ;
    bit.size -= KEY_ECC_SZ;
    AJ_BigvalDecode(bit.data, &pub->y, KEY_ECC_SZ);

    return status;
}

static AJ_Status DecodeCertificateExt(X509Extensions* extensions, DER_Element* der)
{
    AJ_Status status;
    DER_Element tmp;
    DER_Element seq;
    DER_Element savedSeq;
    DER_Element boolVal;
    DER_Element intVal;
    DER_Element oid;
    DER_Element oct;
    const uint8_t tags[] = { ASN_OID, ASN_OCTETS };
    const uint8_t tagsWithCritical[] = { ASN_OID, ASN_BOOLEAN, ASN_OCTETS };
    const uint8_t tagsCAPathLen[] = { ASN_BOOLEAN, ASN_INTEGER };

    memset(extensions, 0, sizeof (X509Extensions));

    status = AJ_ASN1DecodeElement(der, ASN_SEQ, &tmp);
    if (AJ_OK != status) {
        return status;
    }
    der->size = tmp.size;
    der->data = tmp.data;
    while ((AJ_OK == status) && (der->size)) {
        status = AJ_ASN1DecodeElement(der, ASN_SEQ, &seq);
        if (AJ_OK != status) {
            return status;
        }
        savedSeq.size = seq.size;
        savedSeq.data = seq.data;

        status = AJ_ASN1DecodeElements(&seq, tagsWithCritical, sizeof (tagsWithCritical), &oid, &boolVal, &oct);
        if (AJ_OK != status) {
            status = AJ_ASN1DecodeElements(&savedSeq, tags, sizeof (tags), &oid, &oct);
            if (AJ_OK != status) {
                return status;
            }
        }
        if (CompareOID(&oid, OID_BASIC_CONSTRAINTS, sizeof (OID_BASIC_CONSTRAINTS))) {
            status = AJ_ASN1DecodeElement(&oct, ASN_SEQ, &seq);
            if (AJ_OK != status) {
                return status;
            }
            // Explicit boolean (non-empty sequence)
            if (seq.size) {
                savedSeq.size = seq.size;
                savedSeq.data = seq.data;
                status = AJ_ASN1DecodeElements(&seq, tagsCAPathLen, sizeof (tagsCAPathLen), &tmp, &intVal);
                if (AJ_OK != status) {
                    status = AJ_ASN1DecodeElement(&savedSeq, ASN_BOOLEAN, &tmp);
                    if (AJ_OK != status) {
                        return status;
                    }
                }
                if (tmp.size) {
                    extensions->ca = *tmp.data;
                }
            }
        }
    }

    return status;
}

static AJ_Status DecodeCertificateTBS(X509TbsCertificate* tbs, DER_Element* der)
{
    AJ_Status status;
    DER_Element ver;
    DER_Element oid;
    DER_Element iss;
    DER_Element utc;
    DER_Element sub;
    DER_Element pub;
    DER_Element ext;
    DER_Element tmp;
    const uint8_t tags[] = { ASN_CONTEXT_SPECIFIC, ASN_INTEGER, ASN_SEQ, ASN_SEQ, ASN_SEQ, ASN_SEQ, ASN_SEQ, ASN_CONTEXT_SPECIFIC };

    memset(tbs, 0, sizeof (X509TbsCertificate));

    status = AJ_ASN1DecodeElements(der, tags, sizeof (tags), 0, &ver, &tbs->serial, &oid, &iss, &utc, &sub, &pub, 3, &ext);
    if (AJ_OK != status) {
        return status;
    }

    /*
     * We only accept X.509v3 certificates.
     */
    status = AJ_ASN1DecodeElement(&ver, ASN_INTEGER, &tmp);
    if (AJ_OK != status) {
        return status;
    }
    if ((0x1 != tmp.size) || (0x2 != *tmp.data)) {
        return AJ_ERR_INVALID;
    }

    /*
     * We only accept ECDSA-SHA256 signed certificates at the moment.
     */
    status = AJ_ASN1DecodeElement(&oid, ASN_OID, &tmp);
    if (AJ_OK != status) {
        return status;
    }
    if (!CompareOID(&tmp, OID_SIG_ECDSA_SHA256, sizeof (OID_SIG_ECDSA_SHA256))) {
        return AJ_ERR_INVALID;
    }

    status = DecodeCertificateName(&tbs->issuer, &iss);
    if (AJ_OK != status) {
        return status;
    }
    status = DecodeCertificateTime(&tbs->validity, &utc);
    if (AJ_OK != status) {
        return status;
    }
    status = DecodeCertificateName(&tbs->subject, &sub);
    if (AJ_OK != status) {
        return status;
    }
    status = DecodeCertificatePub(&tbs->publickey, &pub);
    if (AJ_OK != status) {
        return status;
    }
    status = DecodeCertificateExt(&tbs->extensions, &ext);
    if (AJ_OK != status) {
        return status;
    }

    return status;
}

static AJ_Status DecodeCertificateSig(ecc_signature* signature, DER_Element* der)
{
    AJ_Status status;
    DER_Element seq;
    DER_Element int1;
    DER_Element int2;
    const uint8_t tags[] = { ASN_INTEGER, ASN_INTEGER };

    status = AJ_ASN1DecodeElement(der, ASN_SEQ, &seq);
    if (AJ_OK != status) {
        return status;
    }
    status = AJ_ASN1DecodeElements(&seq, tags, sizeof (tags), &int1, &int2);
    if (AJ_OK != status) {
        return status;
    }

    /*
     * Skip over unused bits.
     */
    if ((0 < int1.size) && (0 == *int1.data)) {
        int1.data++;
        int1.size--;
    }
    if ((0 < int2.size) && (0 == *int2.data)) {
        int2.data++;
        int2.size--;
    }

    memset(signature, 0, sizeof (ecc_signature));
    AJ_BigvalDecode(int1.data, &signature->r, int1.size);
    AJ_BigvalDecode(int2.data, &signature->s, int2.size);

    return status;
}

AJ_Status AJ_X509DecodeCertificateDER(X509Certificate* certificate, DER_Element* der)
{
    AJ_Status status;
    DER_Element seq;
    DER_Element tbs;
    DER_Element tmp;
    DER_Element oid;
    DER_Element sig;
    const uint8_t tags1[] = { ASN_SEQ };
    const uint8_t tags2[] = { ASN_SEQ, ASN_SEQ, ASN_BITS };

    AJ_InfoPrintf(("AJ_X509DecodeCertificateDER(certificate=%p, der=%p)\n", certificate, der));

    if ((NULL == certificate) || (NULL == der)) {
        return AJ_ERR_INVALID;
    }

    status = AJ_ASN1DecodeElements(der, tags1, sizeof (tags1), &seq);
    if (AJ_OK != status) {
        return status;
    }
    status = AJ_ASN1DecodeElements(&seq, tags2, sizeof (tags2), &tbs, &tmp, &sig);
    if (AJ_OK != status) {
        return status;
    }

    /*
     * The signed TBS includes the sequence and length fields.
     */
    certificate->raw.data = tbs.data - 4;
    certificate->raw.size = tbs.size + 4;

    status = DecodeCertificateTBS(&certificate->tbs, &tbs);
    if (AJ_OK != status) {
        return status;
    }

    /*
     * We only accept ECDSA-SHA256 signed certificates at the moment.
     */
    status = AJ_ASN1DecodeElement(&tmp, ASN_OID, &oid);
    if (AJ_OK != status) {
        return status;
    }
    if (!CompareOID(&oid, OID_SIG_ECDSA_SHA256, sizeof (OID_SIG_ECDSA_SHA256))) {
        return AJ_ERR_INVALID;
    }

    /*
     * Remove the byte specifying unused bits, this should always be zero.
     */
    if ((0 == sig.size) || (0 != *sig.data)) {
        return AJ_ERR_INVALID;
    }
    sig.data++;
    sig.size--;
    status = DecodeCertificateSig(&certificate->signature, &sig);

    return status;
}

AJ_Status AJ_X509DecodeCertificatePEM(X509Certificate* certificate, const char* pem, size_t len)
{
    AJ_Status status;

    AJ_InfoPrintf(("AJ_X509DecodeCertificatePEM(certificate=%p, pem=%p, len=%zu)\n", certificate, pem, len));

    certificate->der.size = 3 * len / 4;
    certificate->der.data = (uint8_t*)AJ_Malloc(certificate->der.size);
    if (NULL == certificate->der.data) {
        return AJ_ERR_RESOURCES;
    }
    status = AJ_B64ToRaw(pem, len, certificate->der.data, certificate->der.size);
    if (AJ_OK != status) {
        AJ_Free(certificate->der.data);
        return status;
    }
    if ('=' == pem[len - 1]) {
        certificate->der.size--;
    }
    if ('=' == pem[len - 2]) {
        certificate->der.size--;
    }

    return AJ_OK;
}

X509CertificateChain* AJ_X509DecodeCertificateChainPEM(const char* pem)
{
    AJ_Status status;
    X509CertificateChain* head = NULL;
    X509CertificateChain* curr = NULL;
    X509CertificateChain* node;
    const char* beg = pem;
    const char* end;

    beg = strstr(beg, PEM_CERT_BEG);
    while (beg) {
        beg = beg + strlen(PEM_CERT_BEG);
        end = strstr(beg, PEM_CERT_END);
        if (NULL == end) {
            return NULL;
        }
        node = (X509CertificateChain*) AJ_Malloc(sizeof (X509CertificateChain));
        if (NULL == node) {
            /* Free the cert chain */
            X509CertificateChain* tmp;
            while (head) {
                tmp = head;
                head = head->next;
                if (tmp) {
                    AJ_Free(tmp);
                }
            }
            return NULL;
        }
        status = AJ_X509DecodeCertificatePEM(&node->certificate, beg, end - beg);
        if (AJ_OK != status) {
            /* Free the cert chain */
            X509CertificateChain* tmp;
            while (head) {
                tmp = head;
                head = head->next;
                if (tmp) {
                    AJ_Free(tmp);
                }
            }
            return NULL;
        }
        // Push on the tail
        node->next = NULL;
        if (curr) {
            curr->next = node;
            curr = node;
        } else {
            head = node;
            curr = node;
        }
        beg = strstr(beg, PEM_CERT_BEG);
    }

    return head;
}

AJ_Status AJ_X509SelfVerify(const X509Certificate* certificate)
{
    AJ_InfoPrintf(("AJ_X509SelfVerify(certificate=%p)\n", certificate));
    return AJ_DSAVerify(certificate->raw.data, certificate->raw.size, &certificate->signature, &certificate->tbs.publickey);
}

AJ_Status AJ_X509Verify(const X509Certificate* certificate, const ecc_publickey* key)
{
    AJ_InfoPrintf(("AJ_X509Verify(certificate=%p, key=%p)\n", certificate, key));
    return AJ_DSAVerify(certificate->raw.data, certificate->raw.size, &certificate->signature, key);
}

AJ_Status AJ_X509VerifyChain(const X509CertificateChain* chain, const ecc_publickey* key)
{
    AJ_Status status;

    AJ_InfoPrintf(("AJ_X509VerifyChain(chain=%p, key=%p)\n", chain, key));

    while (chain) {
        if (key) {
            status = AJ_X509Verify(&chain->certificate, key);
            if (AJ_OK != status) {
                return status;
            }
        }
        /* The subject field of the current certificate must equal the issuer field of the next certificate
         * in the chain.
         */
        if (NULL != chain->next) {
            if (!AJ_X509CompareNames(chain->certificate.tbs.subject, chain->next->certificate.tbs.issuer)) {
                return AJ_ERR_SECURITY;
            }
        }
        key = &chain->certificate.tbs.publickey;
        chain = chain->next;
    }

    return AJ_OK;
}
