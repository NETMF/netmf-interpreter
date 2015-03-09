////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Graphics.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

CLR_UINT32 CLR_GFX_FontDescription::GetCharacterExSize() const
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    CLR_UINT32 size = 0;

    if(m_flags & c_FontEx)
    {
        size = ROUNDTOMULTIPLE(sizeof(CLR_GFX_FontCharacterEx) * (m_characters), CLR_UINT32);
    }

    return size;
}

CLR_UINT32 CLR_GFX_FontDescription::GetRangeExSize() const
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    CLR_UINT32 size = 0;

    if(m_flags & c_FontEx)
    {
        size = ROUNDTOMULTIPLE(sizeof(CLR_GFX_FontCharacterRangeEx) * (m_ranges), CLR_UINT32);
    }
    
    return size;        
}

CLR_UINT32 CLR_GFX_FontDescription::GetRangeSize() const
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    // We need the sentinel range at the end to ensure proper width calculation. 
    // (char width is determined by the difference of the subsequent offset and the current one)
    
    return sizeof(CLR_GFX_FontCharacterRange) * (m_ranges + 1);
}

CLR_UINT32 CLR_GFX_FontDescription::GetCharacterSize() const
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    // We need the sentinel character at the end to ensure proper width caclulation. 
    // (char width is determined by the difference of the subsequent offset and the current one)
    return sizeof(CLR_GFX_FontCharacter) * (m_characters + 1);
}

//--//

CLR_UINT32 CLR_GFX_FontDescriptionEx::GetAntiAliasSize() const
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    return ROUNDTOMULTIPLE(m_antiAliasSize, CLR_UINT32);
}

//--//

void CLR_GFX_Font::Relocate()
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_ranges                  );
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_chars                   );
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_rangesEx                );
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_charsEx                 );
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_antiAliasingData        );
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_bitmap.m_palBitmap.data );
}

void CLR_GFX_Font::RelocationHandler( CLR_RT_HeapBlock_BinaryBlob* ptr )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    CLR_GFX_Font* pThis = (CLR_GFX_Font*)ptr->GetData();

    pThis->Relocate();
}

void CLR_GFX_Font::Font_Initialize()
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    CLR_UINT8* buf = (CLR_UINT8*)&this[ 1 ];

    m_bitmap.Bitmap_Initialize();

    m_ranges        = (CLR_GFX_FontCharacterRange*)buf; buf += m_font.GetRangeSize    ();
    m_chars         = (CLR_GFX_FontCharacter*     )buf; buf += m_font.GetCharacterSize();
    
    if(m_font.m_flags & CLR_GFX_FontDescription::c_FontEx)
    {
        m_rangesEx = (CLR_GFX_FontCharacterRangeEx*)buf; buf += m_font.GetRangeExSize();
        m_charsEx  = (     CLR_GFX_FontCharacterEx*)buf; buf += m_font.GetCharacterExSize();    
        m_antiAliasingData =                        buf; buf += m_fontEx.GetAntiAliasSize();
    }
    
    m_bitmap.m_palBitmap.data = (CLR_UINT32*)buf; 
}

HRESULT CLR_GFX_Font::CreateInstance( CLR_RT_HeapBlock& ref, const CLR_UINT8* data, CLR_RT_Assembly* assm )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_BinaryBlob*     blob;
    CLR_GFX_Font*                    font;
    const CLR_GFX_FontDescription*   fd;
    const CLR_GFX_BitmapDescription* bm;

    fd         = (const CLR_GFX_FontDescription*  )data; data += sizeof(CLR_GFX_FontDescription  );
    bm         = (const CLR_GFX_BitmapDescription*)data; data += sizeof(CLR_GFX_BitmapDescription);

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_BinaryBlob::CreateInstance( ref, sizeof(CLR_GFX_Font), NULL, CLR_GFX_Font::RelocationHandler, CLR_RT_HeapBlock::HB_CompactOnFailure ));

    blob = ref.DereferenceBinaryBlob();
    font = (CLR_GFX_Font*)blob->GetData();

    font->m_font                   = *fd;
    font->m_bitmap.m_bm            = *bm;
    font->m_fontEx.m_antiAliasSize = 0;

    font->Font_Initialize();

    font->m_ranges        = (CLR_GFX_FontCharacterRange*)data; data += fd->GetRangeSize();
    font->m_chars         = (     CLR_GFX_FontCharacter*)data; data += fd->GetCharacterSize();
    font->m_bitmap.m_palBitmap.data = (      CLR_UINT32*)data; data += bm->GetTotalSize();

    font->m_bitmap.m_bm.m_flags |= CLR_GFX_BitmapDescription::c_ReadOnly;

    if(fd->m_flags & CLR_GFX_FontDescription::c_FontEx)
    {       
        font->m_fontEx           = *(( const CLR_GFX_FontDescriptionEx*)data); data += sizeof(CLR_GFX_FontDescriptionEx);
        font->m_rangesEx         =  (     CLR_GFX_FontCharacterRangeEx*)data; data += fd->GetRangeExSize();
        font->m_charsEx          =  (          CLR_GFX_FontCharacterEx*)data; data += fd->GetCharacterExSize();
        font->m_antiAliasingData =  (                        CLR_UINT8*)data; data += font->m_fontEx.GetAntiAliasSize();
    }

    blob->m_assembly = assm;

    font->GetCharInfo( c_UnicodeReplacementCharacter, font->m_defaultChar );

    TINYCLR_NOCLEANUP();
}

//--//
int CLR_GFX_Font::StringOut( LPCSTR str, int maxChars, CLR_INT32 kerning, CLR_GFX_Bitmap* bm, int xPos, int yPos, CLR_UINT32 color )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    CLR_RT_UnicodeHelper      uh; uh.SetInputUTF8( str );
    CLR_UINT16                buf[ 3 ];    
    CLR_UINT32                nTotWidth = 0;
    CLR_GFX_FontCharacterInfo chr;

    yPos += m_font.m_metrics.m_offset;

    while(maxChars-- != 0)
    {
        uh.m_outputUTF16      = buf;
        uh.m_outputUTF16_size = MAXSTRLEN(buf);

        if(uh.ConvertFromUTF8( 1, false ) == false) break;

        CLR_UINT16             c   = buf[ 0 ];
        
        GetCharInfo( c, chr );

        if(chr.isValid)
        {
            //It doesn't look like we support kerning??
            _ASSERTE(c_DefaultKerning == kerning);

            int nOffset = ((nTotWidth + (chr.width+1)/2) * kerning / c_DefaultKerning) - (chr.width+1)/2;

            nTotWidth = nOffset + chr.width;

            if(bm)
            {
                DrawChar( bm, chr, xPos + chr.marginLeft + nOffset, yPos, color );
            }                
        }
    }

    return xPos + nTotWidth;
}

struct DrawCharHelperParam
{
    int originalDstX;
    int originalDstY;
    int srcX;
    int srcY;

    CLR_UINT32* srcFirstWord;
    CLR_UINT32  srcFirstPixelMask;

    CLR_UINT32  srcCurPixelMask;
    CLR_UINT32* srcCurWord;

    CLR_UINT32  srcWidthInWords;

    CLR_UINT32 color;

    CLR_UINT8*  antiAlias;
    CLR_UINT8   iAntiAlias;
    CLR_UINT8   antiAliasStep;
    CLR_UINT8   antiAliasShift;
    CLR_UINT8   antiAliasShiftFirstPixel;
    CLR_UINT32  antiAliasMask;
    CLR_UINT32  antiAliasMaskFirstPixel;
};

UINT32 CLR_GFX_Font::DrawCharHelper( int x, int y, UINT32 flags, UINT16& opacity, void* param )
{
    DrawCharHelperParam* myParam = (DrawCharHelperParam*)param;

    if (flags & PAL_GFX_Bitmap::c_SetPixels_First)
    {
        if (!(myParam->antiAlias)) //adjust to clipping
        {
            myParam->srcX += x - myParam->originalDstX;
            myParam->srcY += y - myParam->originalDstY;
        }

        myParam->srcFirstWord += (myParam->srcY * myParam->srcWidthInWords) + (myParam->srcX / 32);
        myParam->srcFirstPixelMask = 1 << (myParam->srcX % 32);
    }

    if (flags & PAL_GFX_Bitmap::c_SetPixels_NewRow)
    {
        myParam->srcCurWord = myParam->srcFirstWord;
        myParam->srcCurPixelMask = myParam->srcFirstPixelMask;

        myParam->srcFirstWord += myParam->srcWidthInWords;
    }

    if (((*(myParam->srcCurWord)) & myParam->srcCurPixelMask))
    {
        if (myParam->antiAlias)
        {
            //Need to deal with clipping region now.  Anti-alias data has no alignment
            if(!(flags & PAL_GFX_Bitmap::c_SetPixels_PixelHidden))
            {
                //Get the alpha value of the pixel [0...iAntiAlias*iAntiAlias]
                CLR_UINT32 alpha    = ((*(myParam->antiAlias) & myParam->antiAliasMask) >> myParam->antiAliasShift) + 1;
                //Convert this into an opacity
                opacity  = alpha * myParam->antiAliasStep;
            }
            
            myParam->antiAliasMask >>= myParam->iAntiAlias;
            myParam->antiAliasShift -= myParam->iAntiAlias;

            if(myParam->antiAliasMask == 0)
            {
                myParam->antiAliasMask = myParam->antiAliasMaskFirstPixel;
                myParam->antiAlias++;
                myParam->antiAliasShift = myParam->antiAliasShiftFirstPixel;
            }
        }
        else
        {
            opacity = PAL_GFX_Bitmap::c_OpacityOpaque;
        }
    }
    else
    {
        opacity = PAL_GFX_Bitmap::c_OpacityTransparent;
    }

    myParam->srcCurPixelMask <<= 1;

    if (myParam->srcCurPixelMask == 0)
    {
        myParam->srcCurWord++;
        myParam->srcCurPixelMask = 0x1;
    }

    return myParam->color;
}

void CLR_GFX_Font::DrawChar( CLR_GFX_Bitmap* bitmap, CLR_GFX_FontCharacterInfo& chr, int xDst, int yDst, CLR_UINT32 color )
{       
    NATIVE_PROFILE_CLR_GRAPHICS();

    GFX_Rect rect;
    rect.left = xDst;
    rect.top = yDst;
    rect.right = xDst + chr.innerWidth - 1;
    rect.bottom = yDst + chr.height - 1;

    DrawCharHelperParam param;

    param.originalDstX = xDst;
    param.originalDstY = yDst;
    param.srcX = chr.offset;
    param.srcY = 0;
    param.color = color;

    param.srcWidthInWords = m_bitmap.m_bm.GetWidthInWords();

    // These will be adjusted to srcX / srcY the first time the callback is called
    param.srcFirstWord = m_bitmap.m_palBitmap.data;
    param.srcFirstPixelMask = 0;


    UINT32 config;
    if (chr.antiAlias)
    {
        param.iAntiAlias = chr.iAntiAlias;

        //Number of set pixels the anti-alias data represents
        CLR_UINT8   antiAliasPixelsPerByte = 8 / param.iAntiAlias;
        param.antiAlias = chr.antiAlias;
        //Amount to shift the antiAlias data to get it's value in the expected range [0...iAntiAlias*iAntiAlias]
        param.antiAliasShiftFirstPixel = param.iAntiAlias * (antiAliasPixelsPerByte - 1);
        param.antiAliasShift = param.antiAliasShiftFirstPixel;
        //Amount to multiply the antiAlias data to convert to opacity
        param.antiAliasStep = PAL_GFX_Bitmap::c_OpacityOpaque / (param.iAntiAlias * param.iAntiAlias);
        //Mask to read the antiAliasData.  Say
        param.antiAliasMaskFirstPixel = ((1 << param.iAntiAlias) - 1) << param.antiAliasShift;    
        param.antiAliasMask = param.antiAliasMaskFirstPixel;

        config = PAL_GFX_Bitmap::c_SetPixelsConfig_NoClip;
    }
    else
    {
        param.antiAlias = NULL;

        config = PAL_GFX_Bitmap::c_SetPixelsConfig_Clip;
    }

    bitmap->SetPixelsHelper( rect, config, &DrawCharHelper, &param );
}

void CLR_GFX_Font::CountCharactersInWidth( LPCSTR str, int maxChars, int width, int& totWidth, bool fWordWrap, LPCSTR& strNext, int& numChars )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    CLR_RT_UnicodeHelper      uh;
    CLR_UINT16                buf[ 3 ];
    LPCSTR                    breakPoint = NULL;
    CLR_UINT16                lastChar   = 0;
    int                       breakWidth = 0;
    int                       breakIndex = 0;
    int                       num        = 0;
    CLR_GFX_FontCharacterInfo chr;

    totWidth = 0;

    uh.SetInputUTF8( str );

    while(maxChars != 0)
    {
        uh.m_outputUTF16      = buf;
        uh.m_outputUTF16_size = MAXSTRLEN(buf);

        if(uh.ConvertFromUTF8( 1, false ) == false) break;

        CLR_UINT16 c        = buf[ 0 ];
        bool       fNewLine = (c == '\n');
        CLR_UINT16 chrWidth = 0;

        GetCharInfo( c, chr );

        if (chr.isValid) chrWidth = chr.width;

        if(fNewLine || chrWidth > width)
        {
            if(fWordWrap)
            {
                // Immediate break (e.g., "...bar; baz...")
                //                              c=^
                if(c == ' ')
                {
                    break;
                }

                if(breakPoint)
                {
                    if(( fNewLine && lastChar == ' ') ||
                       (!fNewLine && c        != ' ')  )
                    {
                        totWidth = breakWidth;
                        str      = breakPoint;
                        num      = breakIndex;
                    }
                }
            }

            break;

        } // newline or too big

        if(c == ' ')
        {
            if(lastChar != c)
            {
                // Break to the left of a space, since the string wouldn't
                // be properly centered with an extra space at the end of a line
                //
                breakIndex = num;
                breakPoint = str;
                breakWidth = totWidth;
            }
        }

        width    -= chrWidth;
        totWidth += chrWidth;

        str = (LPCSTR)uh.m_inputUTF8;
        maxChars--;
        num++;

        // Break @ hyphens
        //
        if(c == '-')
        {
            if(lastChar != ' ') // e.g., "...foo -1000"
            {
                // Break to the right for a hyphen so that it stays part
                // of the current line
                //
                breakIndex = num;
                breakPoint = str;
                breakWidth = totWidth;
            }
        }

        lastChar = c;

    } // maxChars

    strNext  = str;
    numChars = num;
}

int GetCharInfoCmp(const void* c, const void* r)
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    CLR_UINT16*                 chr   = (CLR_UINT16*)c;
    CLR_GFX_FontCharacterRange* range = (CLR_GFX_FontCharacterRange*)r;

    if ((*chr) < range->m_firstChar) return -1;
    if ((*chr) > range->m_lastChar ) return  1;
    /******************************/ return  0;
}

void CLR_GFX_Font::GetCharInfo( CLR_UINT16 c, CLR_GFX_FontCharacterInfo& chrEx )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    CLR_GFX_FontCharacter*      chr;
    CLR_GFX_FontCharacterRange* range;

    chrEx.isValid = false;

    range = (CLR_GFX_FontCharacterRange*) bsearch( &c, m_ranges, m_font.m_ranges, sizeof(CLR_GFX_FontCharacterRange), GetCharInfoCmp );

    if (range)
    {
        int chrIndex = range->m_indexOfFirstFontCharacter + (c - range->m_firstChar);
        chr = &m_chars[ chrIndex ];

        if (chr)
        {
            chrEx.isValid     = true;
            chrEx.marginLeft  = chr->m_marginLeft;                 
            chrEx.marginRight = chr->m_marginRight;
            chrEx.offset      = range->m_rangeOffset + chr->m_offset;

            if (c == range->m_lastChar)
            {
                chrEx.innerWidth = (range[ 1 ].m_rangeOffset + (CLR_UINT32)chr[ 1 ].m_offset) - chrEx.offset;
            }
            else
            {
                chrEx.innerWidth = (CLR_UINT32)chr[ 1 ].m_offset - (CLR_UINT32)chr[ 0 ].m_offset;
            }

            chrEx.width      = chrEx.marginLeft + chrEx.marginRight + chrEx.innerWidth;
            chrEx.height     = m_bitmap.m_bm.m_height;
            chrEx.iAntiAlias = (this->m_font.m_flags & CLR_GFX_FontDescription::c_AntiAliasMask) >> CLR_GFX_FontDescription::c_AntiAliasShift;

            if(chrEx.iAntiAlias > 1 && m_charsEx[ chrIndex ].m_offsetAntiAlias != CLR_GFX_FontCharacterEx::c_offsetNoAntiAlias)
            {
                int iRange = (int)(range - m_ranges);  
                chrEx.antiAlias = m_antiAliasingData + this->m_rangesEx[ iRange ].m_offsetAntiAlias + m_charsEx[ chrIndex ].m_offsetAntiAlias;
            }
            else
            {
                chrEx.antiAlias  = NULL;
                chrEx.iAntiAlias = 1;
            }
        }
    }
    else
    {
        chrEx = m_defaultChar;
    }
}
