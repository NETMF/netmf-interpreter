////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\Graphics.h"

#include <setjmp.h>

extern "C"
{
#include "jpeglib.h"
#include "jerror.h"
}

////////////////////////////////////////////////////////////////////////////////////////////////////


// JPEG Error handling struct / routine
struct JPEGErrorManager 
{
    jpeg_error_mgr pub;
#if defined(ARM_V3_0) || defined(ARM_V3_1)
    __int64    setjmpBuffer[48];

#elif defined(GCCOP) || defined(__RENESAS__)
   jmp_buf     setjmpBuffer;

#elif 1
   int         setjmpBuffer[32];
#else 
    jmp_buf setjmpBuffer;
#endif
};




void JPEGErrorHandler( j_common_ptr cinfo )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    JPEGErrorManager* errorManager = (JPEGErrorManager*) cinfo->err;
#if 0
    longjmp((long long *)errorManager->setjmpBuffer, 1);
#else
    longjmp(errorManager->setjmpBuffer, 1);
#endif
}

struct CreateInstanceJpegHelperParam
{
    CLR_UINT8* curBuffer;
    int        index;
    JSAMPARRAY buffer;
    jpeg_decompress_struct* cinfo;
};

HRESULT CLR_GFX_Bitmap::CreateInstanceJpeg( CLR_RT_HeapBlock& ref, const CLR_UINT8* data, const CLR_UINT32 size )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    TINYCLR_HEADER();

    jpeg_decompress_struct        cinfo;
    CLR_GFX_Bitmap*               bitmap = NULL;
    CreateInstanceJpegHelperParam param;
    GFX_Rect                      rect;

    param.curBuffer = NULL;

    // Set up the error handling
    JPEGErrorManager errorManager;
    cinfo.err                   = jpeg_std_error( &errorManager.pub );
    errorManager.pub.error_exit = JPEGErrorHandler;

#if 0
    if(setjmp( (long long *)errorManager.setjmpBuffer )) 
#else
    if(setjmp(errorManager.setjmpBuffer )) 
#endif
    {
        if (cinfo.err->msg_code == JERR_OUT_OF_MEMORY)
        {
            TINYCLR_SET_AND_LEAVE( CLR_E_OUT_OF_MEMORY );
        }
        else
        {
            TINYCLR_SET_AND_LEAVE( CLR_E_FAIL );
        }
    }

    // Create the decompression engine
    jpeg_create_decompress( &cinfo );
    jpeg_byte_array_src   ( &cinfo, (UINT8*)data, size );
    jpeg_read_header      ( &cinfo, TRUE );

    // Set output to 16bit 5-6-5 RGB format
    // We can add support for other bit-depth in the future
    cinfo.out_color_space     = JCS_RGB;
    cinfo.do_fancy_upsampling = FALSE;

    jpeg_start_decompress( &cinfo );

    // At this point cinfo would have all the correct output dimension info
    CLR_GFX_BitmapDescription bm;
    if(bm.BitmapDescription_Initialize( cinfo.output_width, cinfo.output_height, CLR_GFX_BitmapDescription::c_NativeBpp ) == false)
    {
        // if the resulting bitmap doesn't match our constraints, stop the decompression
        TINYCLR_SET_AND_LEAVE( CLR_E_INVALID_PARAMETER );
    }

    // Allocate the memory that the decompressed bitmap would need
    TINYCLR_CHECK_HRESULT(CLR_GFX_Bitmap::CreateInstance( ref, bm ));

    TINYCLR_CHECK_HRESULT(CLR_GFX_Bitmap::GetInstanceFromHeapBlock (ref, bitmap));

    // Do the actual decompression
    rect.left = 0;
    rect.top = 0;
    rect.right = cinfo.output_width - 1;
    rect.bottom = cinfo.output_height - 1;

    param.curBuffer = (CLR_UINT8*)CLR_RT_Memory::Allocate( cinfo.output_width * 3 );  CHECK_ALLOCATION(param.curBuffer);
    param.buffer    = (JSAMPARRAY)&(param.curBuffer);
    param.cinfo     = &cinfo;

    bitmap->SetPixelsHelper( rect, PAL_GFX_Bitmap::c_SetPixelsConfig_NoClip | PAL_GFX_Bitmap::c_SetPixelsConfig_NoClipChecks, &CreateInstanceJpegHelper, &param );

    TINYCLR_CLEANUP();

    if (param.curBuffer) CLR_RT_Memory::Release( param.curBuffer );
    jpeg_destroy_decompress( &cinfo );

    TINYCLR_CLEANUP_END();
}

UINT32 CLR_GFX_Bitmap::CreateInstanceJpegHelper( int x, int y, UINT32 flags, UINT16& opacity, void* param )
{
    CreateInstanceJpegHelperParam* myParam = (CreateInstanceJpegHelperParam*)param;

    if (flags & PAL_GFX_Bitmap::c_SetPixels_NewRow)
    {
        jpeg_read_scanlines( myParam->cinfo, myParam->buffer, 1 );

        myParam->index = 0;
    }

    opacity = PAL_GFX_Bitmap::c_OpacityOpaque;

    UINT32 r = myParam->curBuffer[myParam->index];
    UINT32 g = myParam->curBuffer[myParam->index+1];
    UINT32 b = myParam->curBuffer[myParam->index+2];

    myParam->index += 3;

    return r | (g << 8) | (b << 16);
}
