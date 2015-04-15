/**
 * @file Buffer list function declarations
 */
/******************************************************************************
 * Copyright (c) 2014, AllSeen Alliance. All rights reserved.
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


#ifndef AJ_BUF_H_
#define AJ_BUF_H_

#include "aj_target.h"
#include "aj_status.h"
#include "aj_wsl_target.h"

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __cplusplus
#pragma pack(push, 1)
#endif

void AJ_BufList_ModuleInit(void);


typedef struct _AJ_BufNode {
    uint16_t length;
    uint8_t flags;
    uint8_t* buffer; //moves around when we pull data out.
    uint8_t* bufferStart; // stays put
    struct _AJ_BufNode* next;
} AJ_BufNode;

/*
 * flag indicating the buffer is allocated elsewhere
 */
#define AJ_BUFNODE_EXTERNAL_BUFFER   (1 << 0)

/*
 * This structure is passed down each layer of the protocol stack.
 * It is populated at each layer.
 * When data is received, each element can point to an entry in a buffer.
 */
typedef struct _AJ_BufList {
    AJ_BufNode* head;
    AJ_BufNode* tail;
} AJ_BufList;

AJ_BufList* AJ_BufListCreate(void);
void AJ_BufListFree(AJ_BufList* list, uint8_t freeNodeBuffers);

AJ_BufList* AJ_BufListCreateCopy(AJ_BufList* listIn);

/*
 * Create a new node, allocate a payload buffer, and zero the memory contents
 */
AJ_BufNode* AJ_BufListCreateNodeZero(uint16_t bufferSize, uint8_t zeroBuffer);

/*
 * Create a new node, allocate a buffer.
 * Use when the full payload will be filled with known data
 */
AJ_BufNode* AJ_BufListCreateNode(uint16_t bufferSize);


AJ_BufNode* AJ_BufListCreateNodeExternalZero(uint8_t* buffer, uint16_t bufferSize, uint8_t zeroBuffer);
AJ_BufNode* AJ_BufListCreateNodeExternalBuffer(uint8_t* buffer, uint16_t bufferSize);


/*
 * Create a new node, and assign the buffer info from the giving node.
 * Use when the the underlying buffer will be used and released elsewhere
 */
AJ_BufNode* AJ_BufNodeCreateAndTakeOwnership(AJ_BufNode* giving);


/*
 * Free the parameter node, (!but not the payload!)
 */
void AJ_BufListFreeNode(AJ_BufNode* node, void* context);

/*
 * Free the parameter node, and the payload buffer
 */
void AJ_BufListFreeNodeAndBuffer(AJ_BufNode* node, void* context);

/*
 * create a new payload of the parameter node
 * by combining the payloads of the parameter node and its follower
 */
void AJ_BufListCoalesce(AJ_BufNode* node);



/*
 * Add a AJ_BufNode to the start of the AJ_BufList
 */
void AJ_BufListPushHead(AJ_BufList* list, AJ_BufNode* newNode);

/*
 * Add a AJ_BufNode to the end of the AJ_BufList
 */
void AJ_BufListPushTail(AJ_BufList* list, AJ_BufNode* newNode);

AJ_EXPORT void AJ_BufNodePullBytes(AJ_BufNode* node, uint16_t count);
AJ_EXPORT void AJ_BufListPullBytes(AJ_BufList* list, uint16_t count);

AJ_EXPORT void AJ_BufListCopyBytes(AJ_BufList* list, uint16_t count, uint8_t* userBuffer);

// prototype for functions that do something with a buffer list node
typedef void (*AJ_BufNodeFunc)(AJ_BufNode* node, void* context);
typedef AJ_Status (*AJ_BufListFunc)(AJ_BufList* node, void* context);

// perform operation on each node in a buffer list
AJ_Status AJ_BufListIterate(AJ_BufNodeFunc nodeFunc, AJ_BufList* list, void* context);

void AJ_BufNodeIterate(AJ_BufNodeFunc nodeFunc, AJ_BufList* list, void* context);

/*
 * AJ_BufList Helper functions
 */

/*
 * This function returns how many bytes will be sent on the wire for this AJ_BufList.
 */
uint16_t AJ_BufListLengthOnWire(AJ_BufList* list);

/*
 * This function returns the length of unconsumed data contained in the AJ_BufList.
 */
uint32_t AJ_BufListGetSize(AJ_BufList* list);

#ifndef NDEBUG
//DEBUG data: simulate SPI read/write using a fixed buffer
typedef struct _AJ_BUF_WIREBUFFER {
    uint8_t fakeWireBuffer[512];
    uint8_t* fakeWireCurr;
    uint8_t* fakeWireWrite;
    uint8_t* fakeWireRead;
} AJ_BUF_WIREBUFFER;

extern AJ_BUF_WIREBUFFER toTarget;
extern AJ_BUF_WIREBUFFER fromTarget;

// prototype for functions that do something with a buffer list node and a wirebuffer
typedef void (*AJ_BufNodeFuncWireBuf)(AJ_BufNode* node, AJ_BUF_WIREBUFFER* wireBuffer);


/*
 * Provide support for hooking the low level SPI functions
 */
typedef uint8_t (*AJ_BufListReadByteFromWire_Func)(AJ_BUF_WIREBUFFER* source);
typedef struct _AJ_BUFLIST_FUNC_TABLE {
    AJ_BufNodeFuncWireBuf writeToWire;
    AJ_BufListReadByteFromWire_Func readByteFromWire;
} AJ_BUFLIST_FUNC_TABLE;


//DEBUG functions
void AJ_BufListNodePrint(AJ_BufNode* node, void* context);
void AJ_BufListNodePrintDump(AJ_BufNode* node, void* context);
void AJ_BufListWriteToWire_Simulated(AJ_BufNode* node, AJ_BUF_WIREBUFFER* target);
uint8_t AJ_BufListReadByteFromWire_Simulated(AJ_BUF_WIREBUFFER* source);
void AJ_BufListReadBytesFromWire_Simulated(uint16_t numberToRead, uint8_t* output, AJ_BUF_WIREBUFFER* source);
void AJ_BufListIterateOnWire(AJ_BufNodeFuncWireBuf nodeFunc, AJ_BufList* list, AJ_BUF_WIREBUFFER* wire);

#endif
#ifndef __cplusplus
#pragma pack(pop)
#endif

#ifdef __cplusplus
}
#endif

#endif /* AJ_BUF_H_ */
