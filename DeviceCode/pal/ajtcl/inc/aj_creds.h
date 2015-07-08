#ifndef _AJ_CREDS_H
#define _AJ_CREDS_H

/**
 * @file aj_creds.h
 * @defgroup aj_creads Credentials Management
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
#include "aj_guid.h"
#include "aj_status.h"
#include "aj_config.h"

#ifdef __cplusplus
extern "C" {
#endif

#define AJ_CRED_TYPE_GENERIC        1 /** < generic type */
#define AJ_CRED_TYPE_AES            2 /** < AES type */
#define AJ_CRED_TYPE_PRIVATE        3 /** < private key type */
#define AJ_CRED_TYPE_PEM            4 /** < PEM encoded type */
#define AJ_CRED_TYPE_PUBLIC         5 /** < public key type */
#define AJ_CRED_TYPE_SPKI_CERT      6 /** < SPKI style certificate type */
#define AJ_CRED_TYPE_DSA_PRIVATE    7 /** < DSA private key type */
#define AJ_CRED_TYPE_DSA_PUBLIC     8 /** < DSA public key type */

/**
 * Credentials for a remote peer
 */
typedef struct _AJ_PeerCred {
    uint16_t type;  /** < credential type */
    uint8_t idLen;  /** < the length of the id field */
    uint8_t* id;     /**< the id field, it can be GUID for the peer */
    uint32_t expiration;  /**< the expiry time expressed a number of seconds since Epoch */
    uint8_t associationLen;   /**< association length */
    uint8_t* association;   /**< association */
    uint16_t dataLen;   /**< data length */
    uint8_t* data;   /**< data */
} AJ_PeerCred;

/**
 * Write a peer credential to NVRAM
 *
 * @param peerCred  The credentials to write.
 *
 * @return
 *          - AJ_OK if the credentials were written.
 *          - AJ_ERR_RESOURCES if there is no space to write the credentials
 */
AJ_Status AJ_StoreCredential(AJ_PeerCred* peerCred);

/**
 * Store the peer secret
 *
 * @param peerGuid  The peer's GUID
 * @param secret  The peer's secret
 * @param len  The peer's secret's length
 * @param expiration  The expiration of the secret
 *
 * @return
 *          - AJ_OK if the credentials were written.
 *          - AJ_ERR_RESOURCES if there is no space to write the credentials
 */
AJ_Status AJ_StorePeerSecret(const AJ_GUID* peerGuid, const uint8_t* secret,
                             const uint8_t len, uint32_t expiration);

/**
 * Delete a peer credential from NVRAM
 *
 * @param peerGuid  The guid for the peer that has credentials to delete.
 *
 * @return
 *          - AJ_OK if the credentials were deleted.
 */
AJ_Status AJ_DeletePeerCredential(const AJ_GUID* peerGuid);

/**
 * Clears all peer credentials.
 *
 * @return
 *          - AJ_OK if all credentials have been deleted
 */
AJ_Status AJ_ClearCredentials(void);

/**
 * Get the credentials for a specific remote peer from NVRAM
 *
 * @param peerGuid  The GUID for the remote peer.
 * @param peerCredHolder Pointer to a credential object pointer.  This address will hold the new allocated credential object for the specific remote peer identified by a GUID
 *
 * @return
 *      - AJ_OK if the credentials for the specific remote peer exist and are copied into the buffer
 *      - AJ_ERR_FAILURE otherwise.
 */
AJ_Status AJ_GetPeerCredential(const AJ_GUID* peerGuid, AJ_PeerCred** peerCredHolder);

/**
 * Get the GUID for this peer. If this is the first time the GUID has been requested this function
 * will generate the GUID and store it in NVRAM
 *
 * @param[out] localGuid Pointer to a bufffer that has enough space to store the local GUID
 *
 * @return  AJ_OK if the local GUID is copied into the buffer.
 */
AJ_Status AJ_GetLocalGUID(AJ_GUID* localGuid);

/**
 * Free the memory allocation for this credential object.  The object itself
 * will also be freed.
 *
 * @param cred  Pointer to a credential object
 *
 * @return
 *      - AJ_OK if the deallocation process succeeds
 *      - AJ_ERR_FAILURE otherwise.
 */
AJ_Status AJ_FreeCredential(AJ_PeerCred* cred);

/**
 * Delete a credential from NVRAM
 *
 * @param credType  the credential type
 * @param id        the credential id
 * @param idLen     the credential id length
 *
 * @return
 *          - AJ_OK if the credentials were deleted.
 */
AJ_Status AJ_DeleteCredential(const uint16_t credType, const uint8_t* id, uint8_t idLen);

/**
 * Get the credentials for a specific id from NVRAM
 *
 * @param credType  the credential type to search
 * @param id        the credential id to search
 * @param idLen     the credential id length to search
 * @param credHolder Pointer to a credential object pointer.  This address will hold the new allocated credential object for the specific custom credential
 *
 * @return
 *      - AJ_OK if the credential is found
 *      - AJ_ERR_FAILURE otherwise.
 */
AJ_Status AJ_GetCredential(const uint16_t credType, const uint8_t* id, uint8_t idLen, AJ_PeerCred** credHolder);

/**
 * Get the local credentials for a specific id from NVRAM
 *
 * @param credType  the credential type to search
 * @param id        the credential id to search
 * @param[out] credHolder Pointer to a credential object pointer.  This address will hold the new allocated credential object for the specific custom credential
 *
 * @return
 *      - AJ_OK if the credential is found
 *      - AJ_ERR_FAILURE otherwise.
 */
AJ_Status AJ_GetLocalCredential(const uint16_t credType, const uint16_t id, AJ_PeerCred** credHolder);

/**
 * Store a local credential
 *
 * @param credType the credential type
 * @param id  The local id
 * @param data  The data
 * @param len  The data length
 * @param expiration  The expiration of the data
 *
 * @return
 *          - AJ_OK if the credentials were written.
 *          - AJ_ERR_RESOURCES if there is no space to write the credentials
 */
AJ_Status AJ_StoreLocalCredential(const uint16_t credType, const uint16_t id, const uint8_t* data, const uint8_t len, uint32_t expiration);

/**
 * Delete the local credentials for a specific id from NVRAM
 *
 * @param credType  the credential type to delete
 * @param id        the credential id to delete
 *
 * @return
 *      - AJ_OK if the credential is deleted
 *      - AJ_ERR_FAILURE otherwise.
 */
AJ_Status AJ_DeleteLocalCredential(const uint16_t credType, const uint16_t id);

/**
 * Checks a credential's expiry
 * @return
 *      - AJ_OK if the credential has not expired
 *      - AJ_ERR_KEY_EXPIRED if the credential has expired
 *      - AJ_ERR_INVALID if not clock is available
 */
AJ_Status AJ_CredentialExpired(AJ_PeerCred* cred);

#ifdef __cplusplus
}
#endif
/**
 * @}
 */
#endif
