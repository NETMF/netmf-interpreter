////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

// we need this to force inclusion from library at link time
#pragma import(EntryPoint)

extern struct CompressedImage_Header PortBooterLoader_Dat;

typedef unsigned char UINT8;

int LZ77_Decompress( UINT8* inBuf, int inSize, UINT8* outBuf, int outSize );

HAL_DECLARE_NULL_HEAP();

extern "C"
{
void BootEntryLoader()
{
    struct CompressedImage_Header* hdr  = &PortBooterLoader_Dat;
    void*                          data = &hdr[1];

    LZ77_Decompress( (UINT8*)data, hdr->Compressed, (UINT8*)hdr->Destination, hdr->Uncompressed );

    CPU_FlushCaches();

    ((void(*)())hdr->Destination)();
}
}

void ApplicationEntryPoint()  
{
}

