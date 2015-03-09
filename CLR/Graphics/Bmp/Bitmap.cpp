////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "bmp.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_GFX_Bitmap::CreateInstanceBmp( CLR_RT_HeapBlock& ref, const CLR_UINT8* data, const CLR_UINT32 size )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    TINYCLR_HEADER();

    CLR_GFX_Bitmap*           bitmap    = NULL;
    CLR_GFX_BitmapDescription bm;
    BmpDecoder                decoder;

    TINYCLR_CHECK_HRESULT(decoder.BmpInitOutput( data, size ));

    if(bm.BitmapDescription_Initialize( decoder.width, decoder.height, CLR_GFX_BitmapDescription::c_NativeBpp ) == false)
    {
        // Failed if width / height / size is not within spec.
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    // Allocate the memory that the decoded bitmap would need
    TINYCLR_CHECK_HRESULT(CLR_GFX_Bitmap::CreateInstance( ref, bm ));

    TINYCLR_CHECK_HRESULT(CLR_GFX_Bitmap::GetInstanceFromHeapBlock (ref, bitmap));

    TINYCLR_CHECK_HRESULT(decoder.BmpStartOutput( bitmap ));
    
    TINYCLR_NOCLEANUP();
}
