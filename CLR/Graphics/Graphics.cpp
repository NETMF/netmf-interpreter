////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Graphics.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

#define EXTRACT_WITHOUT_SHIFT(c,s0) \
    CLR_UINT32 c = s0

#define EXTRACT_WITH_SHIFT(c,s0,s1,shift,shiftN) \
    CLR_UINT32 c = (s0 >> shift) | (s1 << shiftN)

#define INSERT_WITH_SHIFT(d0,d1,c,mask,shift,shiftN) \
    d0 = (d0 &  mask) | (c << shift ); \
    d1 = (d1 & ~mask) | (c >> shiftN);

#define INSERT_WITH_SHIFT__NO_MASK(d0,d1,c,shift,shiftN) \
    d0 |= (c << shift ); \
    d1 |= (c >> shiftN);

#define PREPARE_MASK(msk,shift) const CLR_UINT32 msk = ~(0xFFFFFFFF << shift)

////////////////////////////////////////////////////////////////////////////////////////////////////

bool CLR_GFX_BitmapDescription::BitmapDescription_Initialize( int width, int height, int bitsPerPixel )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    if (width        <  0 || width        > c_MaxWidth ) return false;
    if (height       <  0 || height       > c_MaxHeight) return false;
    if (bitsPerPixel <  0 || bitsPerPixel > 32         ) return false; // 0 is valid, meaning c_NativeBpp

    // Make sure change in c_MaxWidth won't overflow widthInWords
    ASSERT((((CLR_UINT64)c_MaxWidth * 32 + 31) <= (CLR_UINT64)0xFFFFFFFF)); 

    m_bitsPerPixel = (CLR_UINT8 )bitsPerPixel;    
    m_width        = (CLR_UINT32)width;
    m_height       = (CLR_UINT32)height;
    m_flags        = 0;
    m_type         = CLR_GFX_BitmapDescription::c_TypeTinyCLRBitmap;

    if (GetTotalSize() < 0) return false;

    return true;
}

int CLR_GFX_BitmapDescription::GetWidthInWords() const
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    if (m_bitsPerPixel == CLR_GFX_BitmapDescription::c_NativeBpp)
    {
        return Graphics_GetWidthInWords( m_width );
    }

    return (m_width * m_bitsPerPixel + 31) / 32;
}

int CLR_GFX_BitmapDescription::GetTotalSize() const
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    //compressed 1bpp bitmaps are not currently supported.
    _ASSERTE((m_flags & CLR_GFX_BitmapDescription::c_Compressed) == 0);

    if (m_bitsPerPixel == CLR_GFX_BitmapDescription::c_NativeBpp)
    {
        return Graphics_GetSize( m_width, m_height );
    }

    return GetWidthInWords() * m_height * sizeof(CLR_UINT32);
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_GFX_Bitmap::Relocate()
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_palBitmap.data );
}

void CLR_GFX_Bitmap::RelocationHandler( CLR_RT_HeapBlock_BinaryBlob* ptr )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    CLR_GFX_Bitmap* pThis = (CLR_GFX_Bitmap*)ptr->GetData();

    pThis->Relocate();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_GFX_Bitmap::CreateInstance( CLR_RT_HeapBlock& ref, const CLR_GFX_BitmapDescription& bm )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    TINYCLR_HEADER();

    CLR_GFX_Bitmap*              bitmap;
    int                          size;

    size = sizeof(CLR_GFX_Bitmap) + bm.GetTotalSize();

    if (size + sizeof (CLR_RT_HeapBlock_BinaryBlob) < (sizeof(CLR_RT_HeapBlock) * CLR_RT_HeapBlock::HB_MaxSize / 2))
    {
        //The entire bitmap will fit in the managed heap, so put it there.
        CLR_RT_HeapBlock_BinaryBlob* blob;

        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_BinaryBlob::CreateInstance( ref, size, NULL, CLR_GFX_Bitmap::RelocationHandler, CLR_RT_HeapBlock::HB_CompactOnFailure ));
        blob   = ref.DereferenceBinaryBlob();
        bitmap = (CLR_GFX_Bitmap*)blob->GetData();
    }
    else
    {
        //The bitmap is too big to fit on the managed heap, so put it on the SimpleHeap
        bitmap = (CLR_GFX_Bitmap*)SimpleHeap_Allocate(size);

        ref.SetInteger((CLR_UINT32)bitmap);
        ref.PerformBoxingIfNeeded();
    }

    if (!bitmap) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_MEMORY);

    // Clear the memory
    CLR_RT_Memory::ZeroFill( bitmap, size );

    bitmap->m_bm = bm;

    bitmap->Bitmap_Initialize();

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_GFX_Bitmap::CreateInstance( CLR_RT_HeapBlock& ref, const CLR_UINT8* data, const CLR_UINT32 size, const CLR_UINT8 type )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    TINYCLR_HEADER();

    if (type == CLR_GFX_BitmapDescription::c_TypeJpeg)
    {
        TINYCLR_SET_AND_LEAVE(CreateInstanceJpeg( ref, data, size ));
    }
    else
    if (type == CLR_GFX_BitmapDescription::c_TypeGif)
    {
        TINYCLR_SET_AND_LEAVE(CreateInstanceGif( ref, data, size ));
    }
    else
    if (type == CLR_GFX_BitmapDescription::c_TypeBmp)
    {
        TINYCLR_SET_AND_LEAVE(CreateInstanceBmp( ref, data, size ));
    }
    else
    {
        // Unknown / unsupported types
        TINYCLR_SET_AND_LEAVE(CLR_E_NOT_SUPPORTED);
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_GFX_Bitmap::CreateInstance( CLR_RT_HeapBlock& ref, const CLR_UINT8* data, CLR_UINT32 size, CLR_RT_Assembly* assm )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock                 refUncompressed; refUncompressed.SetObjectReference(NULL);
    CLR_GFX_Bitmap*                  bitmap;
    CLR_GFX_Bitmap*                  bitmapNative;
    const CLR_GFX_BitmapDescription* bm;
    CLR_GFX_BitmapDescription        bmUncompressed;
    CLR_GFX_BitmapDescription        bmNative;
    CLR_UINT16                       flags;    
    bool                             unpinAssm = false;

    bm  = (const CLR_GFX_BitmapDescription*)data; 
    
    data += sizeof(CLR_GFX_BitmapDescription);
    size -= sizeof(CLR_GFX_BitmapDescription);

    if (bm->m_type == CLR_GFX_BitmapDescription::c_TypeJpeg || bm->m_type == CLR_GFX_BitmapDescription::c_TypeGif)
    {
        if (assm->m_pFile)
        {
            unpinAssm = !assm->m_pFile->IsPinned();
            assm->m_pFile->Pin();
        }

        TINYCLR_CHECK_HRESULT(CLR_GFX_Bitmap::CreateInstance( ref, data, size, bm->m_type ));

        TINYCLR_SET_AND_LEAVE(S_OK);
    }

    flags = bm->m_flags;

    if((bm->m_flags & CLR_GFX_BitmapDescription::c_Compressed) != 0)
    {
        /* When loading a Windows BMP, GIF, or JPEG file, it is converted in-place to the native BPP.
         * When loading a compressed TinyCLR Bitmap from a resource file, two bitmaps are needed to decompress, then convert.
         * This fragments the heap and wastes space until the next garbage collection is done.
         * When using the SimpleHeap, there is no relocation, so decompressing into a temp bitmap into the simpleheap wastes
         * memory 6.25% the size of the 16bpp bitmap that's saved.
         */

        if(!bmUncompressed.BitmapDescription_Initialize( bm->m_width, bm->m_height, bm->m_bitsPerPixel ))
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        flags &= ~CLR_GFX_BitmapDescription::c_Compressed;

        bmUncompressed.m_flags = flags;

        TINYCLR_CHECK_HRESULT(CLR_GFX_Bitmap::CreateInstance( refUncompressed, bmUncompressed ));

        TINYCLR_CHECK_HRESULT(CLR_GFX_Bitmap::GetInstanceFromHeapBlock (refUncompressed, bitmap));

        bitmap->Decompress( data, bm, size );

        data = (CLR_UINT8*)bitmap->m_palBitmap.data;
        bm   = &bitmap->m_bm;
    }

    // Since we're now bpp-agnostic we'll always do the convert
    {
        // -- possibly decompress and convertToNative at the same time?
        if(!bmNative.BitmapDescription_Initialize( bm->m_width, bm->m_height, CLR_GFX_BitmapDescription::c_NativeBpp ))
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        bmNative.m_flags = flags & ~CLR_GFX_BitmapDescription::c_ReadOnly;

        TINYCLR_CHECK_HRESULT(CLR_GFX_Bitmap::CreateInstance( ref, bmNative ));

        TINYCLR_CHECK_HRESULT(CLR_GFX_Bitmap::GetInstanceFromHeapBlock (ref, bitmapNative));

        bitmapNative->ConvertToNative( bm, (CLR_UINT32*)data );
    }

    TINYCLR_CLEANUP();

    /* Free the temporary bitmap created for the decompression process, if any: */
    DeleteInstance ( refUncompressed );

    if (unpinAssm) 
    {
        // Unpin the assembly if we pinned it earlier
        assm->m_pFile->Unpin();
    }

    TINYCLR_CLEANUP_END();
}

HRESULT CLR_GFX_Bitmap::GetInstanceFromHeapBlock( const CLR_RT_HeapBlock& ref, CLR_GFX_Bitmap*& bitmap )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    TINYCLR_HEADER();
    CLR_RT_HeapBlock* blob;

    blob = ref.Dereference();

    if (!blob)
    {
        TINYCLR_SET_AND_LEAVE (CLR_E_OBJECT_DISPOSED);
    }

    switch (blob->DataType())
    {
        case DATATYPE_BINARY_BLOB_HEAD:
            bitmap = (CLR_GFX_Bitmap*)(((CLR_RT_HeapBlock_BinaryBlob*)blob)->GetData());
            break;
        case DATATYPE_VALUETYPE:
            if (blob->IsBoxed() && blob[ 1 ].DataType() == DATATYPE_U4)
            {
                bitmap = (CLR_GFX_Bitmap*)(void*)(blob[ 1 ].NumericByRefConst().u4);
            }
            else
            {
                //We were somehow passed an invalid boxed value.
                TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
            }
            break;
        default:
            TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
            break;
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_GFX_Bitmap::DeleteInstance( CLR_RT_HeapBlock& ref )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* blob;
    CLR_GFX_Bitmap*   bitmap;

    blob = ref.Dereference();

    if (!blob)
    {
        TINYCLR_SET_AND_LEAVE (S_OK);
    }

    switch (blob->DataType())
    {
        case DATATYPE_BINARY_BLOB_HEAD:
            ((CLR_RT_HeapBlock_BinaryBlob*)blob)->Release(false);
            break;
        case DATATYPE_VALUETYPE:
            if (blob->IsBoxed() && blob[ 1 ].DataType() == DATATYPE_U4)
            {
                bitmap = (CLR_GFX_Bitmap*)(blob[ 1 ].NumericByRefConst().u4);
                SimpleHeap_Release (bitmap);
            }
            else
            {
                //We were somehow passed an invalid boxed value.
                TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
            }
            break;
        default:
            TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
            break;
    }
    
    ref.SetObjectReference(NULL);

    TINYCLR_NOCLEANUP();
}


void CLR_GFX_Bitmap::Decompress( const CLR_UINT8* data, const CLR_GFX_BitmapDescription* bm, CLR_UINT32 size )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    CLR_UINT32  mask                = 0xffffffff;
    CLR_UINT32  cBitsDst            = 32;
    CLR_UINT32* pd                  = (CLR_UINT32*)m_palBitmap.data;
    CLR_UINT32* pbyteDst            = pd;
    CLR_UINT32  cBitsScanLine       = m_bm.m_width;
    CLR_UINT32  cLines              = m_bm.m_height;
    CLR_UINT32  cBytes              = size;
    CLR_UINT32  dSrc                = 0;
    CLR_UINT32  cBitsTotal;
    CLR_UINT32  widthInWords        = m_bm.GetWidthInWords();

    while(cBytes-- > 0)
    {
        CLR_UINT8 b    = *data++;
        bool      fRun = (b &   CLR_GFX_BitmapDescription::c_CompressedRun) != 0;
        /************/    b &= ~CLR_GFX_BitmapDescription::c_CompressedRun;

        if(fRun)
        {
            dSrc       = (b & CLR_GFX_BitmapDescription::c_CompressedRunSet) ? 0xffffffff : 0x0;
            cBitsTotal = (b & CLR_GFX_BitmapDescription::c_CompressedRunLengthMask) + CLR_GFX_BitmapDescription::c_CompressedRunOffset;
        }
        else
        {
            cBitsTotal = CLR_GFX_BitmapDescription::c_UncompressedRunLength;
        }

        while(cBitsTotal > 0)
        {
            if(!fRun)
            {
                int shift = (32 - cBitsDst) - (CLR_GFX_BitmapDescription::c_UncompressedRunLength - cBitsTotal);

                if(shift > 0) dSrc = b <<  shift;
                else          dSrc = b >> -shift;
            }

            CLR_UINT32 cBits = __min( cBitsScanLine, __min( cBitsTotal, cBitsDst ) );

            *pbyteDst = ((*pbyteDst) & ~mask) | (dSrc & mask);

            cBitsTotal    -= cBits;
            cBitsDst      -= cBits;
            cBitsScanLine -= cBits;
            mask         <<= cBits;

            if(cBitsScanLine == 0)
            {
                if(--cLines == 0) return;

                pd            += widthInWords;
                pbyteDst       = pd;
                cBitsScanLine  = m_bm.m_width;
                cBitsDst       = 32;
                mask           = 0xffffffff;
            }
            else if(cBitsDst == 0)
            {
                pbyteDst++;
                cBitsDst = 32;
                mask     = 0xffffffff;
            }
        }
    }
}

struct ConvertToNativeHelperParam
{
    CLR_UINT32* srcFirstWord;
    CLR_UINT32  srcWidthInWords;

    union
    {
        CLR_UINT32* srcCur1BppWord;
        CLR_UINT16* srcCur16BppPixel;
    };

    CLR_UINT32  srcCur1BppPixelMask;
};

UINT32 CLR_GFX_Bitmap::ConvertToNative1BppHelper( int x, int y, UINT32 flags, UINT16& opacity, void* param )
{
    ConvertToNativeHelperParam* myParam = (ConvertToNativeHelperParam*)param;

    if (flags & PAL_GFX_Bitmap::c_SetPixels_NewRow)
    {
        myParam->srcCur1BppWord = myParam->srcFirstWord;
        myParam->srcCur1BppPixelMask = 0x1;

        myParam->srcFirstWord += myParam->srcWidthInWords;
    }

    UINT32 color = ((*(myParam->srcCur1BppWord)) & myParam->srcCur1BppPixelMask) ? 0x00000000 : 0x00FFFFFF;

    myParam->srcCur1BppPixelMask <<= 1;

    if (myParam->srcCur1BppPixelMask == 0)
    {
        myParam->srcCur1BppWord++;
        myParam->srcCur1BppPixelMask = 0x1;
    }

    opacity = PAL_GFX_Bitmap::c_OpacityOpaque;
    return color;
}

UINT32 CLR_GFX_Bitmap::ConvertToNative16BppHelper( int x, int y, UINT32 flags, UINT16& opacity, void* param )
{
    ConvertToNativeHelperParam* myParam = (ConvertToNativeHelperParam*)param;

    if (flags & PAL_GFX_Bitmap::c_SetPixels_NewRow)
    {
        myParam->srcCur16BppPixel = (CLR_UINT16*)myParam->srcFirstWord;
        myParam->srcFirstWord += myParam->srcWidthInWords;
    }

    UINT32 color16 = *(myParam->srcCur16BppPixel);

    UINT32 r = ( color16           >> 11) << 3; if ((r & 0x8) != 0) r |= 0x7;   // Copy LSB
    UINT32 g = ((color16 & 0x07E0) >>  5) << 2; if ((g & 0x4) != 0) g |= 0x3;   // Copy LSB
    UINT32 b =  (color16 & 0x001F)        << 3; if ((b & 0x8) != 0) b |= 0x7;   // Copy LSB

    myParam->srcCur16BppPixel++;

    opacity = PAL_GFX_Bitmap::c_OpacityOpaque;
    return (r | g << 8 | b << 16);
}

void CLR_GFX_Bitmap::ConvertToNative( const CLR_GFX_BitmapDescription* bmSrc, CLR_UINT32* dataSrc )
{
    NATIVE_PROFILE_CLR_GRAPHICS();

    GFX_Rect rect;

    rect.left = 0;
    rect.top = 0;
    rect.right = bmSrc->m_width - 1;
    rect.bottom = bmSrc->m_height - 1;

    ConvertToNativeHelperParam param;
    param.srcFirstWord = dataSrc;
    param.srcWidthInWords = bmSrc->GetWidthInWords();

    UINT32 config = PAL_GFX_Bitmap::c_SetPixelsConfig_NoClip | PAL_GFX_Bitmap::c_SetPixelsConfig_NoClipChecks;

    // We currently only support 1bpp or 16bpp bitmap resource (this is controlled by GenerateTinyResource build
    // task) 
    switch(bmSrc->m_bitsPerPixel)
    {
    case 1:
        SetPixelsHelper( rect, config, &ConvertToNative1BppHelper, &param );
        break;
    case 16:
        SetPixelsHelper( rect, config, &ConvertToNative16BppHelper, &param );
        break;
    }
}

//--//

void CLR_GFX_Bitmap::Bitmap_Initialize()
{
    NATIVE_PROFILE_CLR_GRAPHICS();

    m_palBitmap.width = m_bm.m_width;
    m_palBitmap.height = m_bm.m_height;
    m_palBitmap.data = (CLR_UINT32*)&this[ 1 ];
    m_palBitmap.transparentColor = PAL_GFX_Bitmap::c_InvalidColor;

    PAL_GFX_Bitmap::ResetClipping( m_palBitmap );
}

void CLR_GFX_Bitmap::Clear()
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    if(m_bm.m_flags & CLR_GFX_BitmapDescription::c_ReadOnly) return;

    Graphics_Clear( m_palBitmap );
}

void CLR_GFX_Bitmap::SetClipping( GFX_Rect& rc )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    //
    // Make sure the clip rect is smaller than the bitmap.
    //
    if     (rc.left   <  0                 ) rc.left   = 0;
    else if(rc.left   >= (int)m_bm.m_width ) rc.left   = m_bm.m_width;

    if     (rc.right  <  0                 ) rc.right  = 0;
    else if(rc.right  >= (int)m_bm.m_width ) rc.right  = m_bm.m_width;

    if     (rc.top    <  0                 ) rc.top    = 0;
    else if(rc.top    >= (int)m_bm.m_height) rc.top    = m_bm.m_height;

    if     (rc.bottom <  0                 ) rc.bottom = 0;
    else if(rc.bottom >= (int)m_bm.m_height) rc.bottom = m_bm.m_height;

    m_palBitmap.clipping.left   = rc.left;
    m_palBitmap.clipping.right  = rc.right;
    m_palBitmap.clipping.top    = rc.top;
    m_palBitmap.clipping.bottom = rc.bottom;
}

CLR_UINT32 CLR_GFX_Bitmap::GetPixel( int xPos, int yPos ) const
{
    NATIVE_PROFILE_CLR_GRAPHICS();

    return Graphics_GetPixel( m_palBitmap, xPos, yPos );
}

void CLR_GFX_Bitmap::SetPixel( int xPos, int yPos, CLR_UINT32 color )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    Graphics_SetPixel( m_palBitmap, xPos, yPos, color );
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_GFX_Bitmap::DrawEllipse( const GFX_Pen& pen, const GFX_Brush& brush, int x, int y, int radiusX, int radiusY )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    Graphics_DrawEllipse( m_palBitmap, pen, brush, x, y, radiusX, radiusY );
}

void CLR_GFX_Bitmap::DrawImage( const GFX_Rect& dst, const CLR_GFX_Bitmap& bitmapSrc, const GFX_Rect& src, UINT16 opacity )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    Graphics_DrawImage( m_palBitmap, dst, bitmapSrc.m_palBitmap, src, opacity );
}

void CLR_GFX_Bitmap::RotateImage( int angle, const GFX_Rect& dst, const CLR_GFX_Bitmap& bitmapSrc, const GFX_Rect& src, UINT16 opacity )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    Graphics_RotateImage( angle, m_palBitmap, dst, bitmapSrc.m_palBitmap, src, opacity );
}

void CLR_GFX_Bitmap::DrawLine( const GFX_Pen& pen, int x0, int y0, int x1, int y1 )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    Graphics_DrawLine( m_palBitmap, pen, x0, y0, x1, y1 );
}

void CLR_GFX_Bitmap::DrawRectangle( const GFX_Pen& pen, const GFX_Brush& brush, const GFX_Rect& rectangle )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    Graphics_DrawRectangle( m_palBitmap, pen, brush, rectangle );
}

void CLR_GFX_Bitmap::DrawRoundedRectangle( const GFX_Pen& pen, const GFX_Brush& brush, const GFX_Rect& rectangle, int radiusX, int radiusY )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    Graphics_DrawRoundedRectangle( m_palBitmap, pen, brush, rectangle, radiusX, radiusY );
}

void CLR_GFX_Bitmap::SetPixelsHelper( const GFX_Rect& rect, UINT32 config, GFX_SetPixelsCallback callback, void* param )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    Graphics_SetPixelsHelper( m_palBitmap, rect, config, callback, param );
}

void CLR_GFX_Bitmap::DrawText( LPCSTR str, CLR_GFX_Font& font, CLR_UINT32 color, int x, int y )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    // This is not implemented: need vectors for text orientation as parameters, and these need to be transformed.

    font.StringOut( str, -1, CLR_GFX_Font::c_DefaultKerning, this, x, y, color );
}

HRESULT CLR_GFX_Bitmap::DrawTextInRect( LPCSTR& szText, int& xRelStart, int& yRelStart, int& renderWidth, int& renderHeight, CLR_GFX_Bitmap* bm, int x, int y, int width, int height, CLR_UINT32 dtFlags, CLR_UINT32 color, CLR_GFX_Font* font )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    TINYCLR_HEADER();

    LPCSTR            szTextLast;
    LPCSTR            szTextNext;
    int               cLineAvailable;
    int               dHeight;
    int               dHeightLine;
    LPCSTR            szEllipsis = "...";
    bool              fDrawEllipsis = false;
    int               ellipsisWidth;
    int               nHeight;
    int               nSkip;
    int               totWidth;
    int               num;
    bool              fFirstLine = true;
    bool              fWordWrap;
    CLR_UINT32        alignment;
    CLR_UINT32        trimming;

    //--//

    alignment = dtFlags & CLR_GFX_Bitmap::c_DrawText_AlignmentMask;
    trimming  = dtFlags & CLR_GFX_Bitmap::c_DrawText_TrimmingMask;

    if(((dtFlags & CLR_GFX_Bitmap::c_DrawText_TruncateAtBottom) && (trimming != CLR_GFX_Bitmap::c_DrawText_TrimmingNone)) ||
        (alignment == CLR_GFX_Bitmap::c_DrawText_AlignmentUnused) ||
        (trimming == CLR_GFX_Bitmap::c_DrawText_TrimmingUnused)) 
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    nHeight = font->m_font.m_metrics.m_height;
    nSkip   = font->m_font.m_metrics.m_externalLeading;

    dHeight        =  height  - yRelStart;
    dHeightLine    =  nHeight + nSkip;
    cLineAvailable =  (dHeight + nSkip + ((dtFlags & CLR_GFX_Bitmap::c_DrawText_TruncateAtBottom)?dHeightLine-1:0)) / dHeightLine;

    renderWidth = 0;
    renderHeight = yRelStart;

    fWordWrap = (dtFlags & CLR_GFX_Bitmap::c_DrawText_WordWrap) != 0;

    while((dtFlags & CLR_GFX_Bitmap::c_DrawText_IgnoreHeight) || --cLineAvailable >= 0)
    {
        szTextLast = szText;

        if(!fFirstLine)
        {
            xRelStart  = 0;
            yRelStart += dHeightLine;
        }

        font->CountCharactersInWidth( szText, -1, width - xRelStart, totWidth, fWordWrap, szTextNext, num);

        if ((xRelStart + totWidth) > renderWidth) renderWidth = xRelStart + totWidth;
        renderHeight += dHeightLine;

        //If we didn't make it...(disregarding any trailing spaces)
        if((trimming != CLR_GFX_Bitmap::c_DrawText_TrimmingNone) && (cLineAvailable == 0) && szTextNext[ 0 ] != 0)
        {
            font->CountCharactersInWidth( szEllipsis, -1, 65536, ellipsisWidth, fWordWrap, szTextNext, num );
            font->CountCharactersInWidth( szText, -1, width - xRelStart - ellipsisWidth, totWidth, (trimming == CLR_GFX_Bitmap::c_DrawText_TrimmingWordEllipsis), szTextNext, num );

            totWidth      += ellipsisWidth;
            fDrawEllipsis  = true;
        }

        if(alignment == CLR_GFX_Bitmap::c_DrawText_AlignmentCenter)
        {
            xRelStart = (width - totWidth + xRelStart) / 2;
        }
        else if (alignment == CLR_GFX_Bitmap::c_DrawText_AlignmentRight)
        {
            xRelStart = width - totWidth;
        }

        if(bm)
        {
            /***************/ xRelStart = font->StringOut( szText    , num, CLR_GFX_Font::c_DefaultKerning, bm, x + xRelStart, y + yRelStart, color ) - x;
            if(fDrawEllipsis) xRelStart = font->StringOut( szEllipsis,  -1, CLR_GFX_Font::c_DefaultKerning, bm, x + xRelStart, y + yRelStart, color ) - x;
        }

        szText = szTextNext;

        if(fWordWrap && szText[ 0 ] == ' ' ) szText++;
        if(             szText[ 0 ] == '\n') szText++; // Eat just one new line.

        if(szTextLast == szText || szText[ 0 ] == 0) break; // No progress made or finished, bail out...

        fFirstLine = false;
    }

    TINYCLR_NOCLEANUP();
}
