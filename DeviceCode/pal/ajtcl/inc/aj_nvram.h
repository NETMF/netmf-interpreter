#ifndef _AJ_NVRAM_H_
#define _AJ_NVRAM_H_

/**
 * @file aj_nvram.h
 * @defgroup aj_nvram Non-Volatile RAM Management
 * @{
 */
/******************************************************************************
 * Copyright (c) 2013-2014, AllSeen Alliance. All rights reserved.
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

#include "alljoyn.h"

#ifdef __cplusplus
extern "C" {
#endif

#define AJ_NVRAM_ID_CREDS_MAX        0x0FFF   /**< Last NVRAM ID reserved for AllJoyn credentials management */
#define AJ_NVRAM_ID_RESERVED_MAX     0x7FFF   /**< Last NVRAM ID reserved for AllJoyn framework and services use */
#define AJ_NVRAM_ID_FOR_APPS         0x8000   /**< First NVRAM ID available for application used */

#define AJ_NV_DATASET_MODE_READ      'r'      /**< Data set is in read mode */
#define AJ_NV_DATASET_MODE_WRITE     'w'      /**< Data set is in write mode */

#ifndef AJ_NVRAM_SIZE
#define AJ_NVRAM_SIZE (4096)
#endif

/**
 * AllJoyn NVRAM dataset handle. Applications should treat this an opaque data structure. The values
 * of the fields are implementation specific so cannot be relied on to have the same meaning across
 * different implementations.
 */
typedef struct _AJ_NV_DATASET {
    uint8_t mode;          /**< The access mode (read or write) of a data set */
    uint16_t curPos;       /**< The current read/write offset of a data set */
    uint16_t capacity;     /**< The capacity of the data set established by AJ_NVRAM_Open() */
    uint16_t id;           /**< The unique id of a data set */
    uint8_t* inode;        /**< Pointer or offset to a location of the data set in the NVRAM */
    void* internal;        /**< Implementation-specific state */
} AJ_NV_DATASET;

/**
 * Initialize NVRAM
 */
void AJ_NVRAM_Init();

/*
 * Get the number of bytes currently used in the NVRAM memory block
 *
 * @return      Number of bytes used
 */
uint32_t AJ_NVRAM_GetSize(void);

/*
 * Get the number of bytes unallocated in the NVRAM memory block
 *
 * @return      Number of free bytes remaining
 */
uint32_t AJ_NVRAM_GetSizeRemaining(void);

/**
 * Completely clear NVRAM
 */
void AJ_NVRAM_Clear();

/**
 * Open a data set
 *
 * @param id  A unique id for a data set. The value must not be 0.
 * @param mode C string containing a data set access mode. It can be:
 *    "r"  : read: Open data set for input operations. The data set must exist.
 *    "w"  : write: Create an empty data set for output operations. If a data set with the same id already exists, its contents are discarded.
 * @param capacity The reserved space size for the data set. Only used for "w" access mode.
 *
 * @return A handle that specifies the data set. NULL if the open operation fails.
 */
AJ_NV_DATASET* AJ_NVRAM_Open(uint16_t id, char* mode, uint16_t capacity);

/**
 * Write to the data set specified by a handle
 *
 * @param ptr   Pointer to a block of memory with a size of at least size bytes to be written to NVRAM.
 * @param size  Size, in bytes, to be written.
 * @param handle Pointer to an AJ_NV_DATASET object that specifies a data set.
 *
 * @return The number of byte of data written to the data set or -1 if the write failed.
 */
size_t AJ_NVRAM_Write(const void* ptr, uint16_t size, AJ_NV_DATASET* handle);

/**
 * Read from the data set specified by a handle
 *
 * @param ptr   Pointer to a block of memory with a size of at least size bytes to be read from NVRAM.
 * @param size  Size, in bytes, to be read.
 * @param handle Pointer to an AJ_NV_DATASET object that specifies a data set.
 *
 * @return The number of bytes of data read from the data set, or -1 if the read failed.
 */
size_t AJ_NVRAM_Read(void* ptr, uint16_t size, AJ_NV_DATASET* handle);

/**
 * Returns a pointer to data at the current read position of an NVRAM data set. This function may
 * not be supported by all implementations. If this function returns NULL the caller will have to
 * allocate a buffer and use AJ_NVRAM_Read() to load the data set into memory.
 *
 * Note: the caller cannot assume that the pointer value returned will remain valid after the data
 * set is closed.
 *
 * @param handle Pointer to an AJ_NV_DATASET object that has been opened for reading.
 *
 * @return  A pointer to the requested data or NULL if this function is not supported by the
 *          implementation or the data set was not opened for reading.
 */
const void* AJ_NVRAM_Peek(AJ_NV_DATASET* handle);

/**
 * Close the data set and release the handle
 *
 * @param handle Pointer to an AJ_NV_DATASET object that specifies a data set.
 *
 * @return AJ_ERR_INVALID if the handle is invalid, otherwise AJ_OK.
 */
AJ_Status AJ_NVRAM_Close(AJ_NV_DATASET* handle);

/**
 * Check if a data set with a unique id exists
 *
 * @param id A unique ID for a data set. A valid id must not be 0.
 *
 * @return 1 if a data set with the specified id exists
 *         0 if not.
 */
uint8_t AJ_NVRAM_Exist(uint16_t id);

/**
 * Delete a data set specified by the id
 *
 * @param id A unique id for a data set.
 *
 * @return AJ_OK if the data set is deleted successfully
 *         AJ_ERR_FAILURE if the data set does not exist.
 */
AJ_Status AJ_NVRAM_Delete(uint16_t id);

#ifdef __cplusplus
}
#endif
/**
 * @}
 */
#endif

