////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\Graphics.h"

#include "gif.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_GFX_Bitmap::CreateInstanceGif( CLR_RT_HeapBlock& ref, const CLR_UINT8* data, const CLR_UINT32 size )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    TINYCLR_HEADER();

    GifDecoder*               decoder;
    CLR_GFX_Bitmap*           bitmap   = NULL;
    CLR_GFX_BitmapDescription bm;

    // Allocate the decoder
    decoder = (GifDecoder*)CLR_RT_Memory::Allocate( sizeof(GifDecoder) );  CHECK_ALLOCATION(decoder);

    // Initialize the decompression engine
    TINYCLR_CHECK_HRESULT( decoder->GifInitDecompress( data, size ) );

    // At this point decoder would have all the correct output dimension info, so we initialized the CLR_GFX_BitmapDescription
    // with that information, note that we currently only support decompression into the native bpp
    if(bm.BitmapDescription_Initialize( decoder->header.LogicScreenWidth, decoder->header.LogicScreenHeight, CLR_GFX_BitmapDescription::c_NativeBpp ) == false)
    {
        // if the resulting bitmap doesn't match our constraints, stop the decompression
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    // Allocate the memory that the decompressed bitmap would need
    TINYCLR_CHECK_HRESULT(CLR_GFX_Bitmap::CreateInstance( ref, bm ));

    TINYCLR_CHECK_HRESULT(CLR_GFX_Bitmap::GetInstanceFromHeapBlock (ref, bitmap));

    TINYCLR_CHECK_HRESULT(decoder->GifStartDecompress( bitmap ));

    TINYCLR_CLEANUP();

    if (decoder) CLR_RT_Memory::Release( decoder );

    TINYCLR_CLEANUP_END();
}
