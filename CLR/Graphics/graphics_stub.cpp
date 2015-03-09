////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Graphics.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

//
// this api uses logical screen co-ordinates.
//
bool CLR_GFX_BitmapDescription::BitmapDescription_Initialize( int width, int height, int bitsPerPixel )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    return true;
}

int CLR_GFX_BitmapDescription::GetWidthInWords() const
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    return 0;
}

int CLR_GFX_BitmapDescription::GetTotalSize() const
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    return 0;
}

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_GFX_Bitmap::CreateInstance( CLR_RT_HeapBlock& ref, const CLR_GFX_BitmapDescription& bm )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_GFX_Bitmap::CreateInstance( CLR_RT_HeapBlock& ref, const CLR_UINT8* data, const CLR_UINT32 size, const CLR_UINT8 type )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_GFX_Bitmap::CreateInstance( CLR_RT_HeapBlock& ref, const CLR_UINT8* data, CLR_UINT32 size, CLR_RT_Assembly* assm )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_GFX_Bitmap::GetInstanceFromHeapBlock( const CLR_RT_HeapBlock& ref, CLR_GFX_Bitmap*& bitmap )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    bitmap = NULL;
    TINYCLR_FEATURE_STUB_RETURN();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_GFX_Bitmap::DeleteInstance( CLR_RT_HeapBlock& ref )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    TINYCLR_FEATURE_STUB_RETURN();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_GFX_Bitmap::Decompress( const CLR_UINT8* data, const CLR_GFX_BitmapDescription* bm, CLR_UINT32 size )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
}


void CLR_GFX_Bitmap::ConvertToNative( const CLR_GFX_BitmapDescription* bmSrc, CLR_UINT32* dataSrc )
{        
    NATIVE_PROFILE_CLR_GRAPHICS();
}

//--//

void CLR_GFX_Bitmap::Bitmap_Initialize()
{
    NATIVE_PROFILE_CLR_GRAPHICS();
}

void CLR_GFX_Bitmap::Clear()
{
    NATIVE_PROFILE_CLR_GRAPHICS();
}

void CLR_GFX_Bitmap::SetClipping( GFX_Rect& rc )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
}

CLR_UINT32 CLR_GFX_Bitmap::GetPixel( int xPos, int yPos ) const
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    return 0;
}

void CLR_GFX_Bitmap::SetPixel( int xPos, int yPos, CLR_UINT32 color )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
}

UINT32 ConvertToNative1BppHelper ( int x, int y, UINT32 flags, UINT16& opacity, void* param )
{
    return 0;
}

UINT32 ConvertToNative16BppHelper( int x, int y, UINT32 flags, UINT16& opacity, void* param )
{
    return 0;
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_GFX_Bitmap::DrawEllipse( const GFX_Pen& pen, const GFX_Brush& brush, int x, int y, int radiusX, int radiusY )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
}


void CLR_GFX_Bitmap::DrawImage( const GFX_Rect& dst, const CLR_GFX_Bitmap& bitmapSrc, const GFX_Rect& src, UINT16 opacity )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
}

void CLR_GFX_Bitmap::RotateImage( int angle, const GFX_Rect& dst, const CLR_GFX_Bitmap& bitmapSrc, const GFX_Rect& src, UINT16 opacity )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
}

void CLR_GFX_Bitmap::DrawLine( const GFX_Pen& pen, int x0, int y0, int x1, int y1 )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
}
    
void CLR_GFX_Bitmap::DrawRectangle( const GFX_Pen& pen, const GFX_Brush& brush, const GFX_Rect& rectangle )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
}

void CLR_GFX_Bitmap::DrawRoundedRectangle( const GFX_Pen& pen, const GFX_Brush& brush, const GFX_Rect& rectangle, int radiusX, int radiusY )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
}

void CLR_GFX_Bitmap::SetPixelsHelper( const GFX_Rect& rect, UINT32 config, GFX_SetPixelsCallback callback, void* param )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
}

void CLR_GFX_Bitmap::DrawText( LPCSTR str, CLR_GFX_Font& font, CLR_UINT32 color, int x, int y )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
}

HRESULT CLR_GFX_Bitmap::DrawTextInRect( LPCSTR& szText, int& xRelStart, int& yRelStart, int& renderWidth, int& renderHeight, CLR_GFX_Bitmap* bm, int x, int y, int width, int height, CLR_UINT32 dtFlags, CLR_UINT32 color, CLR_GFX_Font* font )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    TINYCLR_FEATURE_STUB_RETURN();
}

//--//

CLR_UINT32 CLR_GFX_FontDescription::GetCharacterExSize() const
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    return 0;
}

CLR_UINT32 CLR_GFX_FontDescription::GetRangeExSize() const
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    return 0;
}

CLR_UINT32 CLR_GFX_FontDescription::GetRangeSize() const
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    return 0;
}

CLR_UINT32 CLR_GFX_FontDescription::GetCharacterSize() const
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    return 0;
}

//--//

CLR_UINT32 CLR_GFX_FontDescriptionEx::GetAntiAliasSize() const
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    return 0;
}

void CLR_GFX_Font::Relocate()
{
    NATIVE_PROFILE_CLR_GRAPHICS();
}

void CLR_GFX_Font::RelocationHandler( CLR_RT_HeapBlock_BinaryBlob* ptr )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
}

void CLR_GFX_Font::Font_Initialize()
{
    NATIVE_PROFILE_CLR_GRAPHICS();
}

HRESULT CLR_GFX_Font::CreateInstance( CLR_RT_HeapBlock& ref, const CLR_UINT8* data, CLR_RT_Assembly* assm )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    TINYCLR_FEATURE_STUB_RETURN();
}

//--//

int CLR_GFX_Font::StringOut( LPCSTR str, int maxChars, CLR_INT32 kerning, CLR_GFX_Bitmap* bm, int xPos, int yPos, CLR_UINT32 color )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    return 0;
}

void CLR_GFX_Font::CountCharactersInWidth( LPCSTR str, int maxChars, int width, int& totWidth, bool fWordWrap, LPCSTR& strNext, int& numChars )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
}

int GetCharInfoCmp(const void* c, const void* r)
{
    NATIVE_PROFILE_CLR_GRAPHICS();
    return 0;
}

void CLR_GFX_Font::GetCharInfo( CLR_UINT16 c, CLR_GFX_FontCharacterInfo& chrEx )
{
    NATIVE_PROFILE_CLR_GRAPHICS();
}
