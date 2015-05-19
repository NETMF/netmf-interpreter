////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <LargeBuffer_decl.h>

static HAL_COMPLETION g_LargeBufferTestCompletion;
static BOOL g_LargeBufferCompletionInit = FALSE;

void LargeBuffer_PostEvent( UINT16 marshalId )
{
    PostManagedEvent(EVENT_LARGEBUFFER, 0, marshalId, 0);
}

void LargeBufferTest_Completion(void* arg)
{
    // Double cast to silence compiler warning about shortening pointer type
    // Ideally this would have used uintptr_t but that wasn't widely supported at the time the API was defined
    LargeBuffer_PostEvent( (UINT16)(UINT32)arg );
}

static UINT8 s_Data[800*600*2];
static int   s_LastSize = ARRAYSIZE(s_Data);

INT32 LargeBuffer_GetNativeBufferSize( UINT16 marshalId )
{
    return s_LastSize;
}

void LargeBuffer_NativeToManaged( UINT16 marshalId, BYTE* pData, size_t size )
{
    int len = (size > ARRAYSIZE(s_Data)) ? ARRAYSIZE(s_Data) : size;

    memcpy(pData, s_Data, len);
}

void LargeBuffer_ManagedToNative( UINT16 marshalId, const BYTE* pData, size_t size )
{
    if(!g_LargeBufferCompletionInit)
    {
        g_LargeBufferTestCompletion.InitializeForUserMode(LargeBufferTest_Completion, (void*)marshalId);
        g_LargeBufferCompletionInit = TRUE;
    }

    if(!g_LargeBufferTestCompletion.IsLinked()) 
    {
        g_LargeBufferTestCompletion.EnqueueDelta(1000);
    }

    int len = (size > ARRAYSIZE(s_Data)) ? ARRAYSIZE(s_Data) : size;
    
    for(int i=0; i<len; i++)
    {
        s_Data[i] = pData[size - 1 - i];
    }

    s_LastSize = len;
}

