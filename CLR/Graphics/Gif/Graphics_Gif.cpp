////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "gif.h"
#include "lzwread.h"

// Initialization routine for GifDecoder struct. When it's finished, 
// the header field would be loaded already.
HRESULT GifDecoder::GifInitDecompress( const UINT8* src, UINT32 srcSize )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    TINYCLR_HEADER();

    colorTableSize = 0;

    isTransparentColorUnique = false;

    // set up the CLR_RT_ByteArrayReader
    TINYCLR_CHECK_HRESULT(source.Init( (UINT8*)src, srcSize ));

    TINYCLR_CHECK_HRESULT(source.Read( &header, sizeof(GifFileHeader) ));

    if (memcmp( header.signature, "GIF87a", 6 ) && memcmp( header.signature, "GIF89a", 6 ))
    {
        // Unrecognized signature
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    // If the file contains a global color table, we'll read it.
    if (header.globalcolortableflag)
    {
        colorTableSize = 1 << ((header.globalcolortablesize) + 1);

        TINYCLR_CHECK_HRESULT(ReadColorTable());
    }

    TINYCLR_NOCLEANUP();
}

// Takes in the output buffer, and do the decompression.
// Note that the output buffer _must_ be at least of size widthInBytes*height
// Also, GifInitDecompress() _must_ be called BEFORE GifStartDecompress()
HRESULT GifDecoder::GifStartDecompress( CLR_GFX_Bitmap* bitmap )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    TINYCLR_HEADER();

    if (!source.IsValid())
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    if (bitmap == NULL)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    if (bitmap->m_bm.m_width != header.LogicScreenWidth || 
        bitmap->m_bm.m_height != header.LogicScreenHeight)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    output = bitmap;

    while (true)
    {
        BYTE chunkType = 0;

        //Read in the chunk type.
        TINYCLR_CHECK_HRESULT(source.Read1Byte( &chunkType ));

        switch(chunkType)
        {
        case 0x2C:  //Image Chunk
            TINYCLR_SET_AND_LEAVE(ProcessImageChunk()); // We're done after reading in the first frame (no multi-frame support)
            break;

        case 0x3B:  //Terminator Chunk
            TINYCLR_SET_AND_LEAVE(S_OK);
            break;

        case 0x21:  //Extension
            //Read in the extension chunk type.
            TINYCLR_CHECK_HRESULT(source.Read1Byte( &chunkType ));

            switch(chunkType)
            {
            case 0xF9:
                TINYCLR_CHECK_HRESULT(ProcessGraphicControlChunk());
                break;

            case 0xFE: // Comment Chunk
            case 0x01: // Plain Text Chunk
            case 0xFF: // APplication Extension Chunk
                TINYCLR_CHECK_HRESULT(ProcessUnwantedChunk()); // We don't care about any of these chunks, so just jump past it
                break;

            default:
                // Unknown chunk type
                TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            }
            break;
        default:
            // Unknown chunk type
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }
    }

    TINYCLR_NOCLEANUP();
}

struct ProcessImageChunkHelperParam
{
    LZWDecompressor* lzwDec;
    bool flushing;
    int usedDecBufferSize;
    int currentDecBufferIndex;

    GifDecoder* decoder;

    bool needInit;
    bool done;
};

HRESULT GifDecoder::ProcessImageChunk()
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    TINYCLR_HEADER();

    LZWDecompressor* lzwDec = NULL;
    GifImageDescriptor currentImageDescriptor;

    TINYCLR_CHECK_HRESULT(source.Read( &currentImageDescriptor, sizeof(GifImageDescriptor) ));


    // If the image descriptor's width and height is different from the ones specified in the header,
    // we'll bail. Also, we ignore the imagedescriptor's top and left, and assume it's 0, 0 always.
    if (currentImageDescriptor.width != header.LogicScreenWidth ||
        currentImageDescriptor.height != header.LogicScreenHeight)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_NOT_SUPPORTED);
    }

    // We don't support interlaced GIFs
    if (currentImageDescriptor.interlaceflag)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_NOT_SUPPORTED);
    }

    if (currentImageDescriptor.localcolortableflag)
    {
        // If there's a local color table, then overwrite the global color table (if there was one)
        // Note that we're taking a shortcut here to simply overwrite the global table because
        // we don't support multi-frames/animated Gif.
        colorTableSize = 1 << ((currentImageDescriptor.localcolortablesize)+1);

        TINYCLR_CHECK_HRESULT(ReadColorTable());
    }

    if (colorTableSize == 0)
    {
        // We don't have a valid color table (global or local) 
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    // Read the codesize from the file.  The GIF decoder needs this to begin.
    BYTE lzwCodeSize;

    TINYCLR_CHECK_HRESULT(source.Read1Byte( &lzwCodeSize ));

    if ((lzwCodeSize < 2) || (lzwCodeSize > 8))
    {
        // Minimum code size should be within the range of 2 - 8
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    // Create a LZW decompressor object based on lzw code size
    // We Allocate the LZWDecompressor on the heap, rather than the stack, since it contains a 16kb token table.
    lzwDec = (LZWDecompressor*)CLR_RT_Memory::Allocate( sizeof(LZWDecompressor) );  CHECK_ALLOCATION(lzwDec);

    lzwDec->LZWDecompressorInit( lzwCodeSize );

    GFX_Rect rect;
    rect.left = 0;
    rect.top = 0;
    rect.right = currentImageDescriptor.width - 1;
    rect.bottom = currentImageDescriptor.height - 1;

    ProcessImageChunkHelperParam param;

    param.lzwDec = lzwDec;
    param.decoder = this;
    param.needInit = true;
    param.flushing = false;
    param.done = false;
    param.currentDecBufferIndex = 0;

    output->SetPixelsHelper( rect, PAL_GFX_Bitmap::c_SetPixelsConfig_NoClip | PAL_GFX_Bitmap::c_SetPixelsConfig_NoClipChecks, &ProcessImageChunkHelper, &param );
    
    TINYCLR_CLEANUP();

    // If we successfully allocated the decompressor, we need to release it.
    if (lzwDec) CLR_RT_Memory::Release( lzwDec );

    TINYCLR_CLEANUP_END();
}

void GifDecoder::SetupFlushing( void* p )
{
    ProcessImageChunkHelperParam* param = (ProcessImageChunkHelperParam*)p;
    param->flushing = true;
    param->usedDecBufferSize = param->decoder->decBufferSize - param->lzwDec->m_cbOut;
    param->currentDecBufferIndex = 0;
}

// Returns true if there are no more inputs to decode (i.e. we're done), false if otherwise
// Note that it'll return true if we ran into some error condition as well.
bool GifDecoder::DecodeUntilFlush( void* p )
{
    ProcessImageChunkHelperParam* param = (ProcessImageChunkHelperParam*)p;

    ASSERT(param->flushing == false);

    while (true)
    {
        if (param->lzwDec->m_fNeedOutput == true)
        {
            if (param->needInit == false) SetupFlushing( param );

            param->lzwDec->m_pbOut       = param->decoder->decBuffer;
            param->lzwDec->m_cbOut       = param->decoder->decBufferSize;
            param->lzwDec->m_fNeedOutput = false;

            if (param->needInit == false) return false;
        }

        if (param->lzwDec->m_fNeedInput == true)
        {
            // The decompressor needs more input, we'll read in the next 
            // data block, and populate the fields accordingly.
            BYTE blockSize;
            HRESULT hr;

            if (FAILED(hr = param->decoder->source.Read1Byte( &blockSize ))) return true;

            if (blockSize > 0) // we have a valid block
            {
                const UINT8* sourceCurPos = param->decoder->source.source;

                // Advance the marker for the stream
                if (FAILED(hr = param->decoder->source.Skip( blockSize ))) return true;

                param->lzwDec->m_pbIn       = sourceCurPos;
                param->lzwDec->m_cbIn       = blockSize;
                param->lzwDec->m_fNeedInput = false;
            }
            else
            {
                // We've reached the end of the data subblocks, flush out the already decompressed data and leave
                if (param->needInit == false)
                {
                    SetupFlushing( param );
                }
                return true;
            }
        }

        param->needInit = false;

        if (param->lzwDec->FProcess() == false)
        {
            SetupFlushing( param );
            return true;
        }
    }

}

UINT32 GifDecoder::ProcessImageChunkHelper( int x, int y, UINT32 flags, UINT16& opacity, void* param )
{
    ProcessImageChunkHelperParam* myParam = (ProcessImageChunkHelperParam*)param;

    // If we're not already in the middle of a flushing, try to decode more data
    if (myParam->flushing == false)
    {
        // However, if we've already decoded all that we can, just leave with invalid color
        if (myParam->done)
        {
            return PAL_GFX_Bitmap::c_InvalidColor;
        }

        if (DecodeUntilFlush( myParam ) == true)
        {
            myParam->done = true;

            // Check again in case DecodeUntilFlushing is return true because of an error
            if (myParam->flushing == false)
            {
                return PAL_GFX_Bitmap::c_InvalidColor;
            }
        }
    }

    if (flags & PAL_GFX_Bitmap::c_SetPixels_NewRow)
    {

    }

    opacity = PAL_GFX_Bitmap::c_OpacityOpaque;
    UINT32 color = myParam->decoder->colorTable[myParam->decoder->decBuffer[myParam->currentDecBufferIndex]];

    myParam->currentDecBufferIndex++;

    if(myParam->currentDecBufferIndex == myParam->usedDecBufferSize)
    {
        myParam->flushing = false;
    }

    return color;
}

HRESULT GifDecoder::ProcessGraphicControlChunk()
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    TINYCLR_HEADER();

    TINYCLR_CHECK_HRESULT(source.Read( &gce, sizeof(GifGraphicControlExtension) ));
    
    if (gce.transparentcolorflag)
    {
        int count = colorTableSize;

        while (!isTransparentColorUnique)
        {
            transparentColor += c_bigPrimeNumber;
            transparentColor &= 0x00FFFFFF; // make sure it's still a valid color
            isTransparentColorUnique = true;
            for (int i = 0; i < colorTableSize; i++)
            {
                if (colorTable[i] == transparentColor)
                {
                    isTransparentColorUnique = false;
                    break;
                }
            }
            count--;
            if (count <= 0)
            {
               TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            }
        }

        colorTable[gce.transparentcolorindex] = transparentColor;

        output->m_palBitmap.transparentColor = transparentColor;
    }

    TINYCLR_CHECK_HRESULT(source.Skip( 1 )); // skip the block terminator

    TINYCLR_NOCLEANUP();
}

HRESULT GifDecoder::ProcessUnwantedChunk()
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    TINYCLR_HEADER();

    UINT8 blockSize = 0;

    do // if we went pass the end of source, Read1Byte() or Skip() will complain and exit
    {
        TINYCLR_CHECK_HRESULT(source.Read1Byte( &blockSize ));
        TINYCLR_CHECK_HRESULT(source.Skip( blockSize ));
    } 
    while (blockSize > 0); // Keep reading data subblocks until we reach a terminator block (0x00)

    TINYCLR_NOCLEANUP();
}

// These functions Read in the 24bit RGB values from the source and 
// convert/store it as the native color format in the colorTable[]
HRESULT GifDecoder::ReadColorTable()
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    TINYCLR_HEADER();

    // Record the first entry in the source stream
    GifPaletteEntry* curEntry = (GifPaletteEntry*)source.source;

    // Advance the marker for source first 
    // (note that colorTableSize is filled in for us before the call of this function)
    TINYCLR_CHECK_HRESULT(source.Skip( colorTableSize * sizeof(GifPaletteEntry) ));

    // Since we're looping through all the colorTable entries already, we'll use
    // this chance to find a unique transparent color, if possible
    isTransparentColorUnique = true; // Assume it's true until found otherwise;
    transparentColor = c_defaultTransparentColor;

    // We can now walk through the entries and convert them as we go

    for (int i = 0; i < colorTableSize; i++)
    {
        colorTable[i] =  ((UINT32)curEntry->red   )        | 
                        (((UINT32)curEntry->green ) <<  8) | 
                        (((UINT32)curEntry->blue  ) << 16) ;
        if (colorTable[i] == transparentColor)
        {
            isTransparentColorUnique = false;
        }
        curEntry++;
    }

    TINYCLR_NOCLEANUP();
}
