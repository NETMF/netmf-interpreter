////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "system_xml.h"

//--//

void CLR_RT_XmlState_Base::ShiftBuffer( CLR_UINT8* newStart )
{
    CLR_UINT32 shiftAmount = newStart - bufferStart;
        
    memmove( (void*)bufferStart, (void*)newStart, bufferEnd - newStart );

    buffer    -= shiftAmount;
    bufferEnd -= shiftAmount;
}

//--//

void CLR_RT_XmlShiftHelper::Initialize()
{
    carRetChainNext  = & carRetChainStart[ 0 ];
    charRefChainNext = &charRefChainStart[ 0 ];
}

void CLR_RT_XmlShiftHelper::SetNextCarRet( CLR_UINT32 index, CLR_UINT8* next )
{
    carRetChainNext[ 0 ] = (CLR_UINT8)( index       & 0xff);
    carRetChainNext[ 1 ] = (CLR_UINT8)((index >> 8) & 0xff);

    carRetChainNext = next;
}

void CLR_RT_XmlShiftHelper::SetNextCharRef( CLR_UINT32 index, CLR_UINT8* next )
{
    charRefChainNext[ 0 ] = (CLR_UINT8)( index       & 0xff);
    charRefChainNext[ 1 ] = (CLR_UINT8)((index >> 8) & 0xff);

    charRefChainNext = next;
}

void CLR_RT_XmlShiftHelper::SaveRelativePositions( CLR_UINT8* value )
{
    // Convert carRetChainNext and charRefChainNext from absolute pointers to relative index, so we can re-set its position
    // after the buffer is shifted, (also if the state object is relocated during its trip to managed code)
    carRetChainNext  = (CLR_UINT8*)((carRetChainNext  == & carRetChainStart[ 0 ]) ? 0xFFFFFFFF : carRetChainNext  - value);
    charRefChainNext = (CLR_UINT8*)((charRefChainNext == &charRefChainStart[ 0 ]) ? 0xFFFFFFFF : charRefChainNext - value);
}

void CLR_RT_XmlShiftHelper::RestoreAbsolutePositions( CLR_UINT8* value )
{
    // Caculate the pointers based on the new position of value
    carRetChainNext  = (carRetChainNext  == (CLR_UINT8*)0xFFFFFFFF) ? & carRetChainStart[ 0 ] : carRetChainNext  + (CLR_UINT32)value;
    charRefChainNext = (charRefChainNext == (CLR_UINT8*)0xFFFFFFFF) ? &charRefChainStart[ 0 ] : charRefChainNext + (CLR_UINT32)value;
}

CLR_UINT32 CLR_RT_XmlShiftHelper::ShiftBuffer( CLR_UINT8* bufferStart, CLR_UINT8* bufferEnd, CLR_UINT8 carRetReplacement )
{
    CLR_UINT32 bufferLen = bufferEnd - bufferStart;

    // End the list by setting the last element to the index of bufferEnd 
    SetNextCarRet ( bufferEnd - bufferStart, NULL );
    SetNextCharRef( bufferEnd - bufferStart, NULL );

    // Start at the beginning of the chain
    CLR_UINT32 nextCarRet  = ((CLR_UINT32) carRetChainStart[ 0 ]) | (((CLR_UINT32) carRetChainStart[ 1 ]) << 8);
    CLR_UINT32 nextCharRef = ((CLR_UINT32)charRefChainStart[ 0 ]) | (((CLR_UINT32)charRefChainStart[ 1 ]) << 8);

    // determine if the first "node" is a carriage return or character reference
    bool isCarRet = nextCarRet < nextCharRef;

    // retrive that first "node"
    CLR_UINT8* current = (isCarRet) ? &bufferStart[ nextCarRet ] : &bufferStart[ nextCharRef ];
    
    CLR_UINT32 shiftAmount = 0;

    // loop until the we are at the end of the string
    while(current != bufferEnd)
    {
        CLR_UINT32 nextIndex = ((CLR_UINT32)current[ 0 ]) | (((CLR_UINT32)current[ 1 ] ) << 8);
        
        if(isCarRet)
        {
            nextCarRet = nextIndex;
        }
        else
        {
            nextCharRef = nextIndex;
        }
    
        bool  isNextCarRet = nextCarRet < nextCharRef;
        // next is the next node in line (can be either carRet or charRef, whichever comes first)
        CLR_UINT8* next    = (isNextCarRet) ? &bufferStart[ nextCarRet ] : &bufferStart[ nextCharRef ];
    
        if(isCarRet)
        {
            // perform the \r\n shift, and fill in the replacement character
            memmove( (void*)(current + 1 - shiftAmount), (void*)(current + 2), next - current - 2 );
            *(current - shiftAmount) = carRetReplacement;
            shiftAmount++;
        }
        else
        {
            // perform the character reference shift, based on the shift amount recorded
            CLR_UINT32 curShiftAmount = (CLR_UINT32)current[ 2 ];
            memmove( (void*)(current - shiftAmount), (void*)(current + curShiftAmount), next - current - curShiftAmount );
            shiftAmount += curShiftAmount;
        }
    
        current = next;
        isCarRet = isNextCarRet;
    }

    return bufferLen - shiftAmount;
}

//--//

void CLR_RT_XmlValueChunks::Initialize()
{
    totalLen = 0;
    head = NULL;
    tail = NULL;
}

HRESULT CLR_RT_XmlValueChunks::Add( CLR_UINT8* valueChunk, CLR_UINT32 len )
{
    TINYCLR_HEADER();
    
    CLR_RT_XmlValueChunk* newChunk;

    // Allocate the memory and copy
    newChunk = (CLR_RT_XmlValueChunk*)CLR_RT_Memory::Allocate( sizeof(CLR_RT_XmlValueChunk) + len ); CHECK_ALLOCATION(newChunk);

    newChunk->len = len;
    newChunk->next = NULL;

    memcpy( (void*)newChunk->GetValueChunk(), (void*)valueChunk, len );

    // hook it up to the end of the linked list
    if(tail)
    {
        tail->next = newChunk;
    }
    else
    {
        head = newChunk;
    }

    tail = newChunk;
    totalLen += len;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_XmlValueChunks::SetValueAndUninitialize( CLR_RT_HeapBlock_XmlReader* reader, CLR_UINT8* remainder, CLR_UINT32 remainderLen )
{
    TINYCLR_HEADER();
    
    CLR_RT_HeapBlock         strHB;
    CLR_RT_HeapBlock_String* valueString;
    LPSTR                    valueStringText;
    CLR_RT_XmlValueChunk*    current;

    valueString = CLR_RT_HeapBlock_String::CreateInstance( strHB, totalLen + remainderLen ); CHECK_ALLOCATION(valueString);

    valueStringText = (LPSTR)valueString->StringText();
    
    // walk the linked list and copy the content to valueString
    current = head;
    while(current)
    {
        CLR_UINT32 currentLen = current->len;
        CLR_RT_XmlValueChunk* next = current->next;
        
        memcpy( (void*)valueStringText, (void*)current->GetValueChunk(), currentLen );

        valueStringText += currentLen;

        CLR_RT_Memory::Release( current );

        current = next;
    }

    head = NULL;
    
    memcpy( valueStringText, remainder, remainderLen ); valueStringText[ remainderLen ] = 0;
    
    reader->SetValue( valueString );

    TINYCLR_NOCLEANUP();
}

