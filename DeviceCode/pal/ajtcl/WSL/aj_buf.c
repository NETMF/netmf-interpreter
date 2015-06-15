/**
 * @file Buffer list implementation
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

#ifdef __cplusplus
extern "C" {
#endif

/**
 * Per-module definition of the current module for debug logging.  Must be defined
 * prior to first inclusion of aj_debug.h
 */
#define AJ_MODULE BUFLIST

#include "aj_target.h"
#include "aj_util.h"
#include "aj_buf.h"
#include "aj_wsl_target.h"
#include "aj_debug.h"
/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgBUFLIST = 0;
#endif

#ifndef NDEBUG
AJ_BUFLIST_FUNC_TABLE AJ_BUFLIST_OPS;
#endif
void AJ_BufList_ModuleInit()
{
    /*
     * Configure the indirection table with the default functions
     */
#ifndef NDEBUG
    AJ_BUFLIST_OPS.readByteFromWire = &AJ_BufListReadByteFromWire_Simulated;
    AJ_BUFLIST_OPS.writeToWire = &AJ_BufListWriteToWire_Simulated;
#endif
}
#ifndef NDEBUG
//DEBUG data: simulate SPI read/write using a fixed buffer
AJ_BUF_WIREBUFFER toTarget;
AJ_BUF_WIREBUFFER fromTarget;
#endif
AJ_BufList* AJ_BufListCreate(void)
{
    AJ_BufList* list;
    list = (AJ_BufList*)AJ_WSL_Malloc(sizeof(AJ_BufList));
    memset(list, 0, sizeof(AJ_BufList));
    return list;
}

AJ_BufList* AJ_BufListCreateCopy(AJ_BufList* listIn)
{
    AJ_BufList* list;
    AJ_BufNode* nodeA;
    uint16_t listInLength;
    list = (AJ_BufList*)AJ_WSL_Malloc(sizeof(AJ_BufList));
    memset(list, 0, sizeof(AJ_BufList));

    listInLength = AJ_BufListLengthOnWire(listIn);
    nodeA = AJ_BufListCreateNodeZero(listInLength, FALSE);
    AJ_BufListCopyBytes(listIn, listInLength, nodeA->bufferStart);

    AJ_BufListPushHead(list, nodeA);
    return list;
}

AJ_BufNode* AJ_BufListCreateNodeZero(uint16_t bufferSize, uint8_t zeroBuffer)
{
    AJ_BufNode* newNode = (AJ_BufNode*)AJ_WSL_Malloc(sizeof(AJ_BufNode));
    newNode->buffer = (uint8_t*)AJ_WSL_Malloc(bufferSize);
    if (zeroBuffer) {
        memset(newNode->buffer, 0, bufferSize);
    }
    newNode->length = bufferSize;
    newNode->next = NULL;
    newNode->bufferStart = newNode->buffer;
    newNode->flags = 0;
    return newNode;
}

AJ_BufNode* AJ_BufListCreateNode(uint16_t bufferSize)
{
    return AJ_BufListCreateNodeZero(bufferSize, 1);
}

AJ_BufNode* AJ_BufListCreateNodeExternalZero(uint8_t* buffer, uint16_t bufferSize, uint8_t zeroBuffer)
{
    AJ_BufNode* newNode = (AJ_BufNode*)AJ_WSL_Malloc(sizeof(AJ_BufNode));
    newNode->buffer = buffer;
    if (zeroBuffer) {
        memset(newNode->buffer, 0, bufferSize);
    }
    newNode->length = bufferSize;
    newNode->next = NULL;
    newNode->bufferStart = newNode->buffer;
    newNode->flags = AJ_BUFNODE_EXTERNAL_BUFFER;
    return newNode;
}


AJ_BufNode* AJ_BufNodeCreateAndTakeOwnership(AJ_BufNode* giving)
{
    AJ_BufNode* newNode = (AJ_BufNode*)AJ_WSL_Malloc(sizeof(AJ_BufNode));
    memcpy(newNode, giving, sizeof(AJ_BufNode));
    giving->flags = AJ_BUFNODE_EXTERNAL_BUFFER;
    return newNode;
}

AJ_BufNode* AJ_BufListCreateNodeExternalBuffer(uint8_t* buffer, uint16_t bufferSize)
{
    return AJ_BufListCreateNodeExternalZero(buffer, bufferSize, 1);
}

void AJ_BufListPushHead(AJ_BufList* list, AJ_BufNode* newNode)
{
    if (!list->head) {
        list->tail = newNode;
        list->head = newNode;
    } else {
        newNode->next = list->head;
        list->head = newNode;
    }
}

void AJ_BufListPushTail(AJ_BufList* list, AJ_BufNode* newNode)
{
    if (!list->head || !list->tail) {
        list->tail = newNode;
        list->head = newNode;
    } else {
        list->tail->next = newNode;
        list->tail = newNode;
    }
}

void AJ_BufListCoalesce(AJ_BufNode* node)
{
    if (node->next) {
        AJ_BufNode* nextNode;
        uint8_t* biggerBuffer;
        nextNode = node->next;

        AJ_ASSERT(!(node->flags | nextNode->flags) & AJ_BUFNODE_EXTERNAL_BUFFER);

        if ((node->flags | nextNode->flags) & AJ_BUFNODE_EXTERNAL_BUFFER) {
            /* one of the nodes has an external buffer, don't coalesce */
            AJ_ASSERT((node->flags | nextNode->flags) & AJ_BUFNODE_EXTERNAL_BUFFER);
        }


        // create a new buffer with the data from two buffers
        biggerBuffer = (uint8_t*)AJ_WSL_Malloc(node->length + nextNode->length);
        memcpy(biggerBuffer, node->buffer, node->length);
        memcpy(biggerBuffer + node->length, nextNode->buffer, nextNode->length);

        AJ_WSL_Free(node->buffer);

        node->buffer = biggerBuffer;
        node->bufferStart = node->buffer;
        node->length += nextNode->length;

        // remove the following node
        node->next = nextNode->next;
        AJ_BufListFreeNodeAndBuffer(nextNode, NULL);
    }
}


AJ_EXPORT void AJ_BufNodePullBytes(AJ_BufNode* node, uint16_t count)
{
    // while there are bytes to remove, pull bytes out of the node.
    // then move to the next node if needed

    while ((node != NULL) &&  (count > 0)) {
        // taking less than the whole buffer
        if (count <= node->length) {
            node->length -= count;
            node->buffer += count;
            count = 0;
            break;
        } else {
            // shorten the list and free the buffer we have now.
            AJ_BufNode* nodeNext = node->next;
            count -= node->length;
            AJ_BufListFreeNodeAndBuffer(node, NULL);
            node = nodeNext;

        }
    }
//    AJ_ASSERT(count == 0);
}


AJ_EXPORT void AJ_BufListPullBytes(AJ_BufList* list, uint16_t count)
{
    // while there are bytes to remove, pull bytes out of the head node of the list.

    while ((list != NULL) &&  (count > 0)) {
        AJ_BufNode* node = list->head;

        // taking less than the whole buffer
        if (count <= node->length) {
            node->length -= count;
            node->buffer += count;
            count = 0;
            break;
        } else {
            // shorten the list and free the buffer we have now.
            list->head = node->next;
            count -= node->length;
            AJ_BufListFreeNodeAndBuffer(node, NULL);
        }
    }
//    AJ_ASSERT(count == 0);
}


/*
 * This function takes a AJ_BufList and copies it to a user buffer
 */
AJ_EXPORT void AJ_BufListCopyBytes(AJ_BufList* list, uint16_t count, uint8_t* userBuffer)
{
    // while there are bytes to remove, pull bytes out of the head node of the list.
    if (list != NULL) {
        AJ_BufNode* node = list->head;

        while ((node != NULL) &&  (count > 0)) {
            // taking less than the whole buffer
            if (count <= node->length) {
                memcpy(userBuffer, node->buffer, count);
                break;
            } else {
                // copy what is in this node and advance to the next.
                memcpy(userBuffer, node->buffer, node->length);
                userBuffer += node->length;
                count -= node->length;
                node = node->next;
            }
        }
    }
}

//DEBUG function
#ifndef NDEBUG
void AJ_BufListWriteToWire_Simulated(AJ_BufNode* node, AJ_BUF_WIREBUFFER* target)
{
    uint16_t iter = 0;
    memcpy(target->fakeWireWrite, node->buffer, node->length);
    target->fakeWireWrite += node->length;

#ifndef NDEBUG
    if (AJ_DbgLevel > AJ_DEBUG_INFO) {
        while (iter < node->length) {
            AJ_AlwaysPrintf(("%02x ", node->buffer[iter]));
            iter++;
        }
    }
#endif
}

//DEBUG function: simulate an SPI read using a stored data buffer
// read a byte from the buffer, advance the read pointer, return the byte.
uint8_t AJ_BufListReadByteFromWire_Simulated(AJ_BUF_WIREBUFFER* source)
{
    uint8_t byteRead = *source->fakeWireRead;
    source->fakeWireRead++;

//    AJ_AlwaysPrintf(("Byte Read %02x ", byteRead));
    return byteRead;
}

//DEBUG function: simulate an SPI read using a stored data buffer
// read a byte from the buffer, advance the read pointer, return the byte.
void AJ_BufListReadBytesFromWire_Simulated(uint16_t numberToRead, uint8_t* output, AJ_BUF_WIREBUFFER* source)
{
    while (numberToRead > 0) {
        *output = AJ_BUFLIST_OPS.readByteFromWire(source);
        output++;
        numberToRead--;
    }
}

#endif

//DEBUG function: print node header info
void AJ_BufListNodePrint(AJ_BufNode* node, void* context)
{
    AJ_AlwaysPrintf(("BufList node %p, flags %x length %d, buffer %p, bufferStart %p\n", node, node->flags, node->length, node->buffer, node->bufferStart));
}

//DEBUG function: print node contents
void AJ_BufListNodePrintDump(AJ_BufNode* node, void* context)
{
    uint16_t iter = 0;
    AJ_BufListNodePrint(node, NULL);
    while (iter < node->length) {
        AJ_AlwaysPrintf(("%02x ", node->buffer[iter]));
        iter++;
    }
    AJ_AlwaysPrintf(("%s", "\n"));
}

// deprecate, perhaps
void AJ_BufNodeIterate(AJ_BufNodeFunc nodeFunc, AJ_BufList* list, void* context)
{
    if (list != NULL) {
        AJ_BufNode* node = list->head;
        while (node != NULL) {
            AJ_BufNode* nextNode = node->next;
            nodeFunc(node, context);
            node = nextNode;
        }
    }
}

AJ_Status AJ_BufListIterate(AJ_BufNodeFunc nodeFunc, AJ_BufList* list, void* context)
{
    AJ_Status status = AJ_OK;
    if (list != NULL) {
        AJ_BufNode* node = list->head;
        while ((status == AJ_OK) && (node != NULL)) {
            AJ_BufNode* nextNode = node->next;
            nodeFunc(node, context);
            node = nextNode;
        }
    }
    return status;
}

void AJ_BufListPrintDump(AJ_BufList* list)
{
    AJ_BufNode* node = list->head;
    while (node != NULL) {
        AJ_BufListNodePrintDump(node, NULL);
        node = node->next;
    }
}
void AJ_BufListPrintDumpContinuous(AJ_BufList* list)
{
    int i;
    AJ_BufNode* node = list->head;
    while (node != NULL) {
        for (i = 0; i < node->length; i++) {
            AJ_AlwaysPrintf(("%02x ", *(node->buffer + i)));
        }
        //AJ_BufListNodePrintDump(node, NULL);
        node = node->next;
    }
    AJ_AlwaysPrintf(("\n"));
}
#ifndef NDEBUG
//DEBUG function: print node header info
void AJ_BufListIterateOnWire(AJ_BufNodeFuncWireBuf nodeFunc, AJ_BufList* list, AJ_BUF_WIREBUFFER* wire)
{
    if (list != NULL) {
        AJ_BufNode* node = list->head;
        while (node != NULL) {
            AJ_BufNode* nextNode = node->next;
            nodeFunc(node, wire);
            node = nextNode;
        }
    }
}
#endif
uint16_t AJ_BufListLengthOnWire(AJ_BufList* list)
{
    uint16_t sum = 0;
    if (list != NULL) {
        AJ_BufNode* node = list->head;
        while (node != NULL) {
            sum += node->length;
            node = node->next;
        }
    }
    return sum;
}


void AJ_BufListFreeNode(AJ_BufNode* node, void* context)
{
    AJ_WSL_Free(node);
}

void AJ_BufListFreeNodeAndBuffer(AJ_BufNode* node, void* context)
{
    if (node && !(node->flags & AJ_BUFNODE_EXTERNAL_BUFFER)) {
        AJ_WSL_Free(node->bufferStart);
    }
    AJ_WSL_Free(node);
}


void AJ_BufListFree(AJ_BufList* list, uint8_t freeNodeBuffers)
{
    AJ_BufListIterate(freeNodeBuffers ? AJ_BufListFreeNodeAndBuffer : AJ_BufListFreeNode, list, NULL);
    AJ_WSL_Free(list);
}
uint32_t AJ_BufListGetSize(AJ_BufList* list)
{
    uint32_t size;
    AJ_BufNode* node;
    size = 0;
    node = list->head;
    while (node != NULL) {
        size += node->length;
        node = node->next;
    }
    return size;
}
#ifdef __cplusplus
}
#endif
