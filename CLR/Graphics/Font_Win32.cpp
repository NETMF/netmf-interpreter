////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Graphics.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

struct FONTFILEINFO {
    WORD dfVersion;                 // 0,1
    DWORD dfSize;                   // 2,3,4,5
    char dfCopyright[ 60 ];         // 6, ..., 65
    WORD dfType;                    // 66, 67
    WORD dfPoints;                  // 68, 69
    WORD dfVertRes;                 // 70, 71
    WORD dfHorizRes;                // 72, 73
    WORD dfAscent;                  // 74, 75
    WORD dfInternalLeading;         // 76, 77
    WORD dfExternalLeading;         // 78, 79
    BYTE dfItalic;                  // 80
    BYTE dfUnderline;               // 81
    BYTE dfStrikeOut;               // 82
    WORD dfWeight;                  // 83, 84
    BYTE dfCharSet;                 // 85
    WORD dfPixWidth;                // 86, 87
    WORD dfPixHeight;               // 88, 89
    BYTE dfPitchAndFamily;          // 90
    WORD dfAvgWidth;                // 91, 92
    WORD dfMaxWidth;                // 93, 94
    BYTE dfFirstChar;               // 95
    BYTE dfLastChar;                // 96
    BYTE dfDefaultChar;             // 97
    BYTE dfBreakChar;               // 98
    WORD dfWidthBytes;              // 99, 100
    DWORD dfDevice;                 // 101, 102, 103, 104
    DWORD dfFace;                   // 105, 106, 107, 108
    DWORD dfBitsPointer;            // 109, 110, 111, 112
    DWORD dfBitsOffset;             // 113, 114, 115, 116
    BYTE dfReserved;                // 117
    DWORD dfFlags;                // 118, 119, 120, 121
};

#define SIZEOF_FONTFILEINFO_2        118
#define SIZEOF_FONTFILEINFO_3       (SIZEOF_FONTFILEINFO_2 + 4 + 2 + 2 + 2 + 4 + 16)

struct GLYPHENTRY2 {
    union {
        DWORD Packed;
        struct {
            WORD geWidth;
            WORD geOffset;
        } u;
    };
};

struct GLYPHENTRY3 {
    BYTE geWidth[ 2 ];
    BYTE geOffset[ 4 ];
};

////////////////////////////////////////////////////////////////////////////////

#define NUMCHARS 96

struct  FONTINFO {
    LPCSTR szFilename;
    int nDescender;
    int nHeight;
    int nWidths[ NUMCHARS ];
};

typedef FONTINFO* PFONTINFO;

////////////////////////////////////////////////////////////////////////////////

static CLR_UINT16 ExtractUINT16( const void* data )
{
    const CLR_UINT8* ptr = (const CLR_UINT8*)data;

    return (ptr[ 1 ] << 8) | ptr[ 0 ];
}

static CLR_UINT32 ExtractUINT32( const void* data )
{
    const CLR_UINT8* ptr = (const CLR_UINT8*)data;

    return (ptr[ 3 ] << 24) | (ptr[ 2 ] << 16) | (ptr[ 1 ] << 8) | ptr[ 0 ];
}

//--//

static bool StringEndsWithSuffix( LPCWSTR str1, LPCWSTR str2 )
{
    size_t len1 = wcslen( str1 );
    size_t len2 = wcslen( str2 );

    return (len1 >= len2 && _wcsicmp( &str1[ len1-len2 ], str2 ) == 0);
}

static bool StringEqualsOrStartsWith( LPCWSTR str1, LPCWSTR str2, size_t maxLenStr2 )
{
    size_t len1 = wcslen( str1 );
    size_t len2 = wcslen( str2 );

    if(len2 < maxLenStr2) return _wcsicmp( str1, str2 ) == 0;
    if(len1 < len2      ) return false;

    return _wcsnicmp( str1, str2, maxLenStr2 ) == 0;
}

static void ReplaceChar( LPWSTR pstr, char cOld, char cNew )
{
    if(pstr)
    {
        while(pstr[ 0 ])
        {
            if(pstr[ 0 ] == cOld) pstr[ 0 ] = cNew;

            pstr++;
        }
    }
}

//--//

CLR_GFX_FontBuilder::AntiAliasHolder::AntiAliasHolder()
{
    Init();
}

CLR_GFX_FontBuilder::AntiAliasHolder::~AntiAliasHolder()
{
    Release();
}

void CLR_GFX_FontBuilder::AntiAliasHolder::Init()
{
    TINYCLR_CLEAR(*this);
}

void CLR_GFX_FontBuilder::AntiAliasHolder::Release()
{
    if(m_data)
    {
        free( m_data );

        m_data = NULL;
    }
}

HRESULT CLR_GFX_FontBuilder::AntiAliasHolder::AllocateData( int width, int height, int bitsPerPixel )
{
    TINYCLR_HEADER();
    
    Release();

    CLR_UINT32 size = (width * height * bitsPerPixel + 7) / 8;

    CLR_UINT8* buf = (CLR_UINT8*)malloc( size ); CHECK_ALLOCATION(buf);

    CLR_RT_Memory::ZeroFill( buf, size );
    
    m_data            = buf;
    m_cBytesAllocated = size;
    m_pixelsSet       = 0;
    m_bitsPerPixel    = bitsPerPixel;
    m_height          = height;
    m_width           = width;
    
    TINYCLR_NOCLEANUP();
}

CLR_GFX_FontBuilder::AntiAliasHolder::AntiAliasHolder( const AntiAliasHolder& aah )
{
    Init();

    *this = aah;
}

CLR_GFX_FontBuilder::AntiAliasHolder& CLR_GFX_FontBuilder::AntiAliasHolder::operator=( const AntiAliasHolder& aah )
{
    Release();

    if(aah.m_data)
    {
        if(FAILED(AllocateData( aah.m_width, aah.m_height, aah.m_bitsPerPixel ))) throw new std::bad_alloc();

        memcpy( m_data, aah.m_data, m_cBytesAllocated );
    }

    return *this;
}

void CLR_GFX_FontBuilder::AntiAliasHolder::AppendData( CLR_UINT32 data )
{
    _ASSERTE(data > 0);
    //This is called only when the pixel has a non-zero alpha value.
    //This lets us store 17 discrete alpha values in 4 bits of data
    data--;
    _ASSERTE(data < (1u << m_bitsPerPixel));

    //number of bits of anti-alias data set
    CLR_UINT32 bitsSet  = m_pixelsSet * m_bitsPerPixel;
    //index into m_data to the byte we are writing to
    CLR_UINT32 iByte  = bitsSet / 8;
    //How many pixels worth of anti-alias data per byte
    CLR_UINT8  pixelsPerByte = 8 / m_bitsPerPixel;
    //Index into the byte which we are writing.  For m_bitsPerPixel, iPixel = [1..0] 0xAB, iPixel = 1 for A, 0 for B
    CLR_UINT8  iPixel = pixelsPerByte - 1 - (m_pixelsSet % pixelsPerByte);
    //How many bits to shift by to move the data into place
    CLR_UINT8  shift = m_bitsPerPixel*iPixel;
    //The byte of anti-alias data we are going to set
    CLR_UINT8* ptr = m_data + iByte;
    //The current value
    CLR_UINT8 val = *ptr;
       
    //The new value
    val |= (data << shift);    
    *ptr = val;

    m_pixelsSet++;
    m_cBytesSet = iByte + 1;
}

//--//

CLR_GFX_FontBuilder::FontBitmap::FontBitmap()
{
    Init();
}

CLR_GFX_FontBuilder::FontBitmap::~FontBitmap()
{
    Release();
}

CLR_GFX_FontBuilder::FontBitmap::FontBitmap( const FontBitmap& fb )
{
    Init();

    *this = fb;
}

CLR_GFX_FontBuilder::FontBitmap& CLR_GFX_FontBuilder::FontBitmap::operator=( const FontBitmap& fb )
{
    Release();

    if(fb.m_data)
    {
        if(FAILED(AllocatePixels( fb.m_width, fb.m_height ))) throw new std::bad_alloc();

        memcpy( m_data, fb.m_data, m_size );
    }

    return *this;
}

void CLR_GFX_FontBuilder::FontBitmap::Init()
{
    m_data = NULL;
}

void CLR_GFX_FontBuilder::FontBitmap::Release()
{
    if(m_data)
    {
        free( m_data );

        m_data = NULL;
    }
}

void CLR_GFX_FontBuilder::FontBitmap::SetPixel( int x, int y )
{
    CLR_UINT32* word = m_data + (y * m_widthInWords) + (x / 32);

    (*word) |= 1 << (x % 32);
}

CLR_UINT32 CLR_GFX_FontBuilder::FontBitmap::GetPixel( int x, int y )
{
    CLR_UINT32* word = m_data + (y * m_widthInWords) + (x / 32);

    return ((*word) >> (x % 32)) & 0x1;
}

HRESULT CLR_GFX_FontBuilder::FontBitmap::AllocatePixels( int width, int height )
{
    TINYCLR_HEADER();

    m_width = width;
    m_height = height;
    
    m_widthInWords = (width + 31) / 32;

    m_size = m_widthInWords * m_height * 4;

    //
    // In Windows, the bitmap could end on a page break.
    // The BitBlt functions can access one extra word, causing a fault.
    //
    m_size += sizeof(CLR_UINT32);

    Release();

    m_data = (CLR_UINT32*)malloc( m_size ); CHECK_ALLOCATION(m_data);

    CLR_RT_Memory::ZeroFill( m_data, m_size );
    
    TINYCLR_NOCLEANUP();
}

//--//

CLR_GFX_FontBuilder::GlyphDescription::GlyphDescription()
{
    m_char       = 0; // CLR_UINT16   m_char;
                      //
    m_cellWidth  = 0; // CLR_UINT16   m_cellWidth;
    m_cellHeight = 0; // CLR_UINT16   m_cellHeight;
                      //
    m_boxX       = 0; // CLR_INT16    m_boxX;
    m_boxY       = 0; // CLR_INT16    m_boxY;
    m_boxWidth   = 0; // CLR_UINT16   m_boxWidth;
    m_boxHeight  = 0; // CLR_UINT16   m_boxHeight;
                      // BitmapHolder m_boxPixels;
                      //
                      // GFX_Rect m_realBox;

    m_realBox.left   = 0;
    m_realBox.right  = 0;
    m_realBox.top    = 0;
    m_realBox.bottom = 0;
}

HRESULT CLR_GFX_FontBuilder::GlyphDescription::AllocatePixels()
{
    TINYCLR_HEADER();

    m_realBox.left   = m_boxWidth - 1;
    m_realBox.right  = 0;
    m_realBox.top    = m_boxHeight - 1;
    m_realBox.bottom = 0;

    TINYCLR_CHECK_HRESULT(m_boxPixels.AllocatePixels( m_boxWidth, m_boxHeight ));

    if(m_iAntiAlias > 1)
    {
        TINYCLR_CHECK_HRESULT(m_antiAliasData.AllocateData( m_boxWidth, m_boxHeight, m_iAntiAlias ));
    }
    
    TINYCLR_NOCLEANUP();
}

void CLR_GFX_FontBuilder::GlyphDescription::SetPixel( int x, int y )
{
    if(m_realBox.left   > x) m_realBox.left   = x;
    if(m_realBox.right  < x) m_realBox.right  = x;
    if(m_realBox.top    > y) m_realBox.top    = y;
    if(m_realBox.bottom < y) m_realBox.bottom = y;

    m_boxPixels.SetPixel( x, y ); // Font is stored in 1bbp bitmaps
}

//--//

CLR_GFX_FontBuilder::CLR_GFX_FontBuilder()
{
                         // GlyphMap                    m_characters;
                         // std::wstring                m_outputName;
                         // CLR_GFX_FontMetricsExtended m_adjust;
    m_iVerboseLevel = 0; // int                         m_iVerboseLevel;
    m_forceDefaultCharacter = true;
    m_iAntiAlias = 1;
    m_iAntiAliasPersist = 1;

    TINYCLR_CLEAR(m_adjust);
}

HRESULT CLR_GFX_FontBuilder::Initialize()
{
    TINYCLR_HEADER();

    m_characters.clear();    

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT CLR_GFX_FontBuilder::LoadFromFNT( const CLR_RT_Buffer& buffer, CLR_UINT16 firstChar, CLR_UINT16 lastChar, CLR_UINT16 mappedFirstChar )
{
    TINYCLR_HEADER();

    FONTFILEINFO     fi;
    GLYPHENTRY2*     ptr2;
    GLYPHENTRY3*     ptr3;
    GLYPHENTRY2*     pgeGlyphTable2 = NULL;
    GLYPHENTRY3*     pgeGlyphTable3 = NULL;
    FILE*            stream         = NULL;
    const CLR_UINT8* data           = &buffer[ 0 ];
    int              i;

    TINYCLR_CLEAR(fi);
    fi.dfVersion         = ExtractUINT16( &data[   0 ] );
    fi.dfAscent          = ExtractUINT16( &data[  74 ] );
    fi.dfInternalLeading = ExtractUINT16( &data[  76 ] );
    fi.dfExternalLeading = ExtractUINT16( &data[  78 ] );
    fi.dfPixWidth        = ExtractUINT16( &data[  86 ] );
    fi.dfPixHeight       = ExtractUINT16( &data[  88 ] );
    fi.dfFirstChar       =                 data[  95 ]  ;
    fi.dfLastChar        =                 data[  96 ]  ;
    fi.dfFlags           = ExtractUINT32( &data[ 118 ] );

    if(fi.dfVersion == 0x200)
    {
        pgeGlyphTable2 = (GLYPHENTRY2 *)&data[ SIZEOF_FONTFILEINFO_2 ];
    }
    else if(fi.dfVersion == 0x300)
    {
        pgeGlyphTable3 = (GLYPHENTRY3 *)&data[ SIZEOF_FONTFILEINFO_3 ];
    }
    else
    {
        //NOTE: Not a version we understand!
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    ptr2 = pgeGlyphTable2;
    ptr3 = pgeGlyphTable3;

    for(i=fi.dfFirstChar; i<fi.dfLastChar; i++, ptr2++, ptr3++)
    {
        int geOffset = pgeGlyphTable2 ? ptr2->u.geOffset : ExtractUINT32( &ptr3->geOffset[ 0 ] );

        if(i >= firstChar && i <= lastChar)
        {
            GlyphDescription& gd     = m_characters[ mappedFirstChar ];
            CLR_UINT16        width  = (pgeGlyphTable2 ? (CLR_UINT16)ptr2->u.geWidth : ExtractUINT16( &ptr3->geWidth[ 0 ] ));
            CLR_UINT16        height = fi.dfPixHeight;
            bool              fGot   = false;

            gd.m_char       = mappedFirstChar++;

            gd.m_cellWidth  = width;
            gd.m_cellHeight = height;

            gd.m_boxX       = m_adjust.m_offsetX;
            gd.m_boxY       = m_adjust.m_offsetY;
            gd.m_boxWidth   = width;
            gd.m_boxHeight  = height;

            TINYCLR_CHECK_HRESULT(gd.AllocatePixels());

            gd.m_metrics                   = m_adjust;
            gd.m_metrics.m_height          = height                      ;
            gd.m_metrics.m_ascent          = fi.dfAscent                 ;
            gd.m_metrics.m_descent         = fi.dfPixHeight - fi.dfAscent;
            gd.m_metrics.m_internalLeading = fi.dfInternalLeading        ;
            gd.m_metrics.m_externalLeading = fi.dfExternalLeading        ;
            gd.m_metrics.m_maxCharWidth    = width                       ;

            for(int x=0; x<width; x++)
            {
                const CLR_UINT8* pbits = &data[ geOffset + (height * (x / 8)) ];

                for(int y=0; y<height; y++, pbits++)
                {
                    if(*pbits >> (7 - (x%8)) & 0x1)
                    {
                        gd.SetPixel( x, y );
                        fGot = true;
                    }
                }
            }

            if(fGot == false) // Empty character!
            {
                gd.m_realBox.left   = 0;
                gd.m_realBox.right  = -1;
                gd.m_realBox.top    = 0;
                gd.m_realBox.bottom = -1;
            }

            //--//
        }
    }


    TINYCLR_NOCLEANUP();
}

void CLR_GFX_FontBuilder::VerifyFontWarning( const WCHAR* descriptor, const WCHAR* expected, const WCHAR* found )
{
    wprintf_s( L"Font cannot be found matching SelectFont descriptor %s. Expected '%s', found '%s'. \n", descriptor, expected, found );
}

void CLR_GFX_FontBuilder::VerifyFontWarning( const WCHAR* descriptor, LONG expected, LONG found )
{
    WCHAR sExpected[ 9 ];
    WCHAR sFound[ 9 ];

    _itow_s( expected, sExpected, ARRAYSIZE(sExpected), 10);
    _itow_s( found   , sFound   , ARRAYSIZE(sFound   ), 10);

    VerifyFontWarning( descriptor, sExpected, sFound );
}

HRESULT CLR_GFX_FontBuilder::VerifyFont( const ENUMLOGFONTEXW& elf, const OUTLINETEXTMETRICW* otm )
{
    TINYCLR_HEADER();

    const BYTE pitchAndFamilyMask = 0xF3;
    const BYTE boolMask = 0x1;

    if(elf.elfFullName          [ 0 ] && !StringEqualsOrStartsWith( (LPCWSTR)((const char *)otm + (size_t)otm->otmpFullName ), elf.elfFullName,           MAXSTRLEN(elf.elfFullName))           ) 
        VerifyFontWarning( L"FullName", elf.elfFullName, (LPCWSTR)((const char *)otm + (size_t)otm->otmpFullName ) );
    if(elf.elfStyle             [ 0 ] && !StringEqualsOrStartsWith( (LPCWSTR)((const char *)otm + (size_t)otm->otmpStyleName), elf.elfStyle,              MAXSTRLEN(elf.elfStyle))              ) 
        VerifyFontWarning( L"Style", elf.elfStyle, (LPCWSTR)((const char *)otm + (size_t)otm->otmpStyleName) );
    if(elf.elfLogFont.lfFaceName[ 0 ] && !StringEqualsOrStartsWith( (LPCWSTR)((const char *)otm + (size_t)otm->otmpFaceName ), elf.elfLogFont.lfFaceName, MAXSTRLEN(elf.elfLogFont.lfFaceName)) ) 
        VerifyFontWarning( L"FaceName", elf.elfLogFont.lfFaceName, (LPCWSTR)((const char *)otm + (size_t)otm->otmpFaceName) );
    if((elf.elfLogFont.lfItalic & boolMask) != (otm->otmTextMetrics.tmItalic & boolMask))
        VerifyFontWarning( L"Italic", elf.elfLogFont.lfItalic & boolMask, otm->otmTextMetrics.tmItalic & boolMask );
    if((elf.elfLogFont.lfWeight != 0) && (elf.elfLogFont.lfWeight != otm->otmTextMetrics.tmWeight)) 
        VerifyFontWarning( L"Weight", elf.elfLogFont.lfWeight, otm->otmTextMetrics.tmWeight );
    if((elf.elfLogFont.lfPitchAndFamily != 0) && ((elf.elfLogFont.lfPitchAndFamily & pitchAndFamilyMask)!= (otm->otmTextMetrics.tmPitchAndFamily & pitchAndFamilyMask))) 
        VerifyFontWarning( L"PitchAndFamily", elf.elfLogFont.lfPitchAndFamily & pitchAndFamilyMask, otm->otmTextMetrics.tmPitchAndFamily & pitchAndFamilyMask);
    if((elf.elfLogFont.lfUnderline & boolMask) != (otm->otmTextMetrics.tmUnderlined & boolMask)) 
        VerifyFontWarning( L"Underline", elf.elfLogFont.lfUnderline & boolMask, otm->otmTextMetrics.tmUnderlined & boolMask);
    if((elf.elfLogFont.lfCharSet != DEFAULT_CHARSET) && (elf.elfLogFont.lfCharSet != otm->otmTextMetrics.tmCharSet))
        VerifyFontWarning( L"CharSet", elf.elfLogFont.lfCharSet, otm->otmTextMetrics.tmCharSet );
    if((elf.elfLogFont.lfStrikeOut & boolMask) != (otm->otmTextMetrics.tmStruckOut & boolMask )) 
        VerifyFontWarning( L"StrikeOut", elf.elfLogFont.lfStrikeOut & boolMask, otm->otmTextMetrics.tmStruckOut & boolMask );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT CLR_GFX_FontBuilder::GetDefaultCharTTF( ENUMLOGFONTEXW& elf, CLR_UINT16& defaultChar )
{
    TINYCLR_HEADER();

    HDC                 hdc     = NULL;
    HBITMAP             bm      = NULL;
    HGDIOBJ             bmOld   = NULL;
    HFONT               font    = NULL;
    HGDIOBJ             fontOld = NULL;
    OUTLINETEXTMETRICW* otm     = NULL;
    ENUMLOGFONTEXDV     elfExDv; TINYCLR_CLEAR(elfExDv);
    DWORD               size;

    elfExDv.elfEnumLogfontEx = elf;

    //--//

    hdc = ::CreateCompatibleDC( NULL ); FAULT_ON_NULL(hdc);

    bm    = ::CreateBitmap( 1024, 128, 1, 1, NULL ); FAULT_ON_NULL(bm);
    bmOld = ::SelectObject( hdc, bm );

    font    = ::CreateFontIndirectExW( &elfExDv  ); FAULT_ON_NULL(font);
    fontOld = ::SelectObject         ( hdc, font );

    size = ::GetOutlineTextMetricsW( hdc, 0, NULL ); FAULT_ON_NULL(size);

    otm = (OUTLINETEXTMETRICW*)malloc( size ); FAULT_ON_NULL(otm);

    CLR_RT_Memory::ZeroFill( otm, size ); otm->otmSize = sizeof(*otm);

    size = ::GetOutlineTextMetricsW( hdc, size, otm ); if(!size) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

    TINYCLR_CHECK_HRESULT(VerifyFont( elf, otm ));

    defaultChar = otm->otmTextMetrics.tmDefaultChar;

    TINYCLR_CLEANUP();

    if(otm)
    {
        free( otm );
    }

    if(font)
    {
        ::SelectObject( hdc, fontOld );
        ::DeleteObject(      font    );
    }

    if(bm)
    {
        ::SelectObject( hdc, bmOld );
        ::DeleteObject(      bm    );
    }

    if(hdc)
    {
        ::DeleteDC( hdc );
    }

    TINYCLR_CLEANUP_END();

}

HRESULT CLR_GFX_FontBuilder::LoadFromTTF( ENUMLOGFONTEXW& elf, CLR_UINT16 firstChar, CLR_UINT16 lastChar, CLR_UINT16 mappedFirstChar )
{
    TINYCLR_HEADER();

    HDC                 hdc     = NULL;
    HBITMAP             bm      = NULL;
    HGDIOBJ             bmOld   = NULL;
    HFONT               font    = NULL;
    HGDIOBJ             fontOld = NULL;
    OUTLINETEXTMETRICW* otm     = NULL;
    GLYPHMETRICS        gm;
    MAT2                mat2   ; TINYCLR_CLEAR(mat2);
    ENUMLOGFONTEXDV     elfExDv; TINYCLR_CLEAR(elfExDv);
    DWORD               size;
    BYTE* buf           = NULL;
    UINT uFormat;
    DWORD widthInBytes;

    mat2.eM11.value = 1;    //identity matrix.
    mat2.eM22.value = 1;

    elfExDv.elfEnumLogfontEx = elf;

    //--//

    hdc = ::CreateCompatibleDC( NULL ); FAULT_ON_NULL(hdc);

    bm    = ::CreateBitmap( 1024, 128, 1, 1, NULL ); FAULT_ON_NULL(bm);
    bmOld = ::SelectObject( hdc, bm );

    font    = ::CreateFontIndirectExW( &elfExDv  ); FAULT_ON_NULL(font);
    fontOld = ::SelectObject         ( hdc, font );

    size = ::GetOutlineTextMetricsW( hdc, 0, NULL ); FAULT_ON_NULL(size);

    otm = (OUTLINETEXTMETRICW*)malloc( size ); FAULT_ON_NULL(otm);

    CLR_RT_Memory::ZeroFill( otm, size ); otm->otmSize = sizeof(*otm);

    size = ::GetOutlineTextMetricsW( hdc, size, otm ); if(!size) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

    TINYCLR_CHECK_HRESULT(VerifyFont( elf, otm ));

    if(firstChar < otm->otmTextMetrics.tmFirstChar) firstChar = otm->otmTextMetrics.tmFirstChar;
    if(lastChar  > otm->otmTextMetrics.tmLastChar ) lastChar  = otm->otmTextMetrics.tmLastChar;
        
    switch(m_iAntiAlias)
    {
        case 1: uFormat = GGO_BITMAP; break;
        case 2: uFormat = GGO_GRAY2_BITMAP; break;
        case 4: uFormat = GGO_GRAY4_BITMAP; break;
        case 8: uFormat = GGO_GRAY8_BITMAP; break;
        default: _ASSERTE(FALSE); TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }
                                    
    for(UINT uChar=firstChar; uChar<=lastChar; uChar++, mappedFirstChar++)
    {                
        size = ::GetGlyphOutlineW( hdc, uChar, uFormat, &gm, 0, NULL, &mat2 );
        if(size != GDI_ERROR)
        {
            buf = new BYTE[ size ]; memset( buf, 0xFF, size );

            size = ::GetGlyphOutlineW( hdc, uChar, uFormat, &gm, size, buf, &mat2 );
            if(size != GDI_ERROR)
            {
                GlyphDescription& gd   = m_characters[ mappedFirstChar ];

                gd.m_char       = mappedFirstChar;

                gd.m_cellWidth  = (CLR_UINT16)(                               gm.gmCellIncX                            );
                gd.m_cellHeight = (CLR_UINT16)(otm->otmTextMetrics.tmHeight                                            );

                gd.m_boxX       = (CLR_INT16 )(                               gm.gmptGlyphOrigin.x + m_adjust.m_offsetX);
                gd.m_boxY       = (CLR_INT16 )(otm->otmTextMetrics.tmAscent - gm.gmptGlyphOrigin.y + m_adjust.m_offsetY);
                gd.m_boxWidth   = (CLR_UINT16)                                gm.gmBlackBoxX;
                gd.m_boxHeight  = (CLR_UINT16)                                gm.gmBlackBoxY;
                gd.m_iAntiAlias  = m_iAntiAlias;

                TINYCLR_CHECK_HRESULT(gd.AllocatePixels());

                gd.m_metrics                   =             m_adjust;
                gd.m_metrics.m_height          = (CLR_UINT16)otm->otmTextMetrics.tmHeight         ;
                gd.m_metrics.m_ascent          = (CLR_UINT16)otm->otmTextMetrics.tmAscent         ;
                gd.m_metrics.m_descent         = (CLR_UINT16)otm->otmTextMetrics.tmDescent        ;
                gd.m_metrics.m_internalLeading = (CLR_UINT16)otm->otmTextMetrics.tmInternalLeading;
                gd.m_metrics.m_externalLeading = (CLR_UINT16)otm->otmTextMetrics.tmExternalLeading;
                gd.m_metrics.m_maxCharWidth    = (CLR_UINT16)otm->otmTextMetrics.tmMaxCharWidth   ;
                                
                if(size > 0)
                {
                    if(m_iAntiAlias == 1)
                    {
                        widthInBytes = 4*((gd.m_boxWidth + 31) / 32);
                    }
                    else
                    {
                        widthInBytes = 4*((gd.m_boxWidth + 3) / 4);
                    }

                    for(int y = 0; y < gd.m_boxHeight; y++)                
                    {
                        CLR_UINT8* ptr = &buf[ y*widthInBytes ];
                        for(int x = 0; x < gd.m_boxWidth; x++)                    
                        {
                            if(m_iAntiAlias == 1)
                            {
                                CLR_UINT8 data = *(ptr + x/8);
                                CLR_UINT8 shift = 7 - (x%8);
                                if(data & (1 << shift))
                                {                                                        
                                    gd.SetPixel( x, y );
                                }
                            }
                            else
                            {
                                CLR_UINT8 data = *(ptr + x);

                                if(data != 0)
                                {
                                    gd.SetPixel( x, y );
                                    gd.m_antiAliasData.AppendData(data);
                                }
                            }
                        }
                    }
                }
                else                
                {
                    _ASSERTE(size==0);
                    gd.m_realBox.left   = 0;
                    gd.m_realBox.right  = -1;
                    gd.m_realBox.top    = 0;
                    gd.m_realBox.bottom = -1;
                }

                //--//
            }
        }
    }


    TINYCLR_CLEANUP();

    if(otm)
    {
        free( otm );
    }

    if(font)
    {
        ::SelectObject( hdc, fontOld );
        ::DeleteObject(      font    );
    }

    if(bm)
    {
        ::SelectObject( hdc, bmOld );
        ::DeleteObject(      bm    );
    }

    if(hdc)
    {
        ::DeleteDC( hdc );
    }
        
    if(buf != NULL)
    {
        delete [] buf;
    }        

    TINYCLR_CLEANUP_END();
}

HRESULT CLR_GFX_FontBuilder::GenerateFont( CLR_GFX_Font*& font )
{
    TINYCLR_HEADER();

    CLR_GFX_FontDescription     fd; TINYCLR_CLEAR(fd);
    CLR_GFX_FontDescriptionEx   fdEx; TINYCLR_CLEAR(fdEx);
    CLR_GFX_BitmapDescription   bm; TINYCLR_CLEAR(bm);
    CLR_GFX_FontCharacterRange* ptrRanges;
    CLR_GFX_FontCharacterRangeEx* ptrRangesEx;
    CLR_GFX_FontCharacter*      ptrChars;
    CLR_GFX_FontCharacterEx*    ptrCharsEx;
    CLR_UINT8*                  ptrAntiAlias;
    GlyphIter                   itBegin    = m_characters.begin();
    GlyphIter                   itEnd      = m_characters.end  ();
    GlyphIter                   it;
    int                         nWidth     = 0;
    int                         rangeWidth = 0;
    CLR_UINT16                  lastChar   = 0xFFFF;
    int                         maxAscent  = 0;
    int                         maxDescent = 0;
    bool                        fFirst     = true;
    CLR_UINT32                  iAntiAliasData = 0;

    font = NULL;

    m_iAntiAlias = m_iAntiAliasPersist;

    for(it = itBegin; it != itEnd; it++)
    {
        GlyphDescription& gd          = it->second;
        int               cWidth      = gd.m_realBox.Width ();
        int               realAscent  = gd.m_metrics.m_ascent  - (gd.m_boxY      + gd.m_realBox.top   );
        int               realDescent = gd.m_metrics.m_descent - (gd.m_boxHeight - gd.m_realBox.bottom - 1);

        fd.m_characters++;
        fdEx.m_antiAliasSize += gd.m_antiAliasData.m_cBytesSet;

        if(lastChar == 0xFFFF || (lastChar + 1) != gd.m_char || rangeWidth + cWidth > 0xFFFF)
        {
            // start a new range
            fd.m_ranges++;
            rangeWidth = 0;
        }

        nWidth     += cWidth;
        rangeWidth += cWidth;

        CLR_GFX_FontBuilder::MaximizeValue( maxAscent,  realAscent  );
        CLR_GFX_FontBuilder::MaximizeValue( maxAscent, -realDescent );

        CLR_GFX_FontBuilder::MaximizeValue( maxDescent,  realDescent );
        CLR_GFX_FontBuilder::MaximizeValue( maxDescent, -realAscent  );

        if(fFirst)
        {
            fFirst = false;

            fd.m_metrics.m_ascent          = gd.m_metrics.m_ascent         ;
            fd.m_metrics.m_descent         = gd.m_metrics.m_descent        ;
            fd.m_metrics.m_internalLeading = gd.m_metrics.m_internalLeading;
            fd.m_metrics.m_externalLeading = gd.m_metrics.m_externalLeading;
            fd.m_metrics.m_maxCharWidth    = gd.m_metrics.m_maxCharWidth   ;
        }
        else
        {
            CLR_GFX_FontBuilder::MaximizeValue( fd.m_metrics.m_ascent         , gd.m_metrics.m_ascent         );
            CLR_GFX_FontBuilder::MaximizeValue( fd.m_metrics.m_descent        , gd.m_metrics.m_descent        );
            CLR_GFX_FontBuilder::MinimizeValue( fd.m_metrics.m_internalLeading, gd.m_metrics.m_internalLeading);
            CLR_GFX_FontBuilder::MaximizeValue( fd.m_metrics.m_externalLeading, gd.m_metrics.m_externalLeading);
            CLR_GFX_FontBuilder::MaximizeValue( fd.m_metrics.m_maxCharWidth   , gd.m_metrics.m_maxCharWidth   );
        }

        if (gd.m_char == CLR_GFX_Font::c_UnicodeReplacementCharacter) m_forceDefaultCharacter = false;

        lastChar = gd.m_char;
    }
    if(fd.m_characters == 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    if (lastChar > CLR_GFX_Font::c_UnicodeReplacementCharacter)
    {
        wprintf(L"ImportRange value exceeds Unicode range (beyond 0xFFFD).\r\n");
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    {
        CLR_GFX_FontMetrics before = fd.m_metrics;

        fd.m_metrics.m_ascent          += m_adjust.m_ascent;
        fd.m_metrics.m_descent         += m_adjust.m_descent;
        fd.m_metrics.m_internalLeading += m_adjust.m_internalLeading;
        fd.m_metrics.m_externalLeading += m_adjust.m_externalLeading;

        CLR_GFX_FontMetrics after  = fd.m_metrics;
        bool                fError = false;


        if((CLR_INT16)before.m_ascent          + (CLR_INT16)m_adjust.m_ascent          < 0) { wprintf( L"Font cannot have negative Ascent\n"          ); fError = true; }
        if((CLR_INT16)before.m_descent         + (CLR_INT16)m_adjust.m_descent         < 0) { wprintf( L"Font cannot have negative Descent\n"          ); fError = true; }
        if((CLR_INT16)before.m_internalLeading + (CLR_INT16)m_adjust.m_internalLeading < 0) { wprintf( L"Font cannot have negative InternalLeading\n" ); fError = true; }
        if((CLR_INT16)before.m_externalLeading + (CLR_INT16)m_adjust.m_externalLeading < 0) { wprintf( L"Font cannot have negative ExternalLeading\n" ); fError = true; }

        if(fError)
        {
            wprintf( L"Metrics         : A:%d D:%d I:%d E:%d\n", before.m_ascent, before.m_descent, before.m_internalLeading, before.m_externalLeading );
            wprintf( L"Adjusted Metrics: A:%d D:%d I:%d E:%d\n", after .m_ascent, after .m_descent, after .m_internalLeading, after .m_externalLeading );

            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }
    }


    fd.m_metrics.m_height       = fd.m_metrics.m_ascent + fd.m_metrics.m_descent;
    fd.m_metrics.m_offset       = fd.m_metrics.m_ascent - maxAscent;
    fd.m_metrics.m_aveCharWidth = nWidth / fd.m_characters;

    if(m_iAntiAlias > 1)
    {
        fd.m_flags |= CLR_GFX_FontDescription::c_FontEx;
        fd.m_flags |= (m_iAntiAlias << CLR_GFX_FontDescription::c_AntiAliasShift);
    }

    if (m_forceDefaultCharacter)
    {
        // Make room for the fall-back default character
        nWidth += fd.m_metrics.m_aveCharWidth;
        fd.m_characters++;
        fd.m_ranges++;
    }

    //if(fd.m_metrics.m_externalLeading < 1) fd.m_metrics.m_externalLeading = 1;

    if(bm.BitmapDescription_Initialize( nWidth, maxAscent + maxDescent, 1 ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    CLR_UINT32 size = sizeof(CLR_GFX_Font)    + 
                      fd.GetCharacterExSize() +
                      fd.GetCharacterSize()   +
                      fd.GetRangeExSize()     +
                      fd.GetRangeSize()       +
                      fdEx.GetAntiAliasSize() +
                      bm.GetTotalSize();

    if((font = (CLR_GFX_Font*)malloc( size )) == NULL)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }
    CLR_RT_Memory::ZeroFill( font, size );

    font->m_font        = fd;
    font->m_bitmap.m_bm = bm;
    font->m_fontEx      = fdEx;

    font->Font_Initialize();

    if(m_iVerboseLevel > 0)
    {
        printf( "Height          : %d\n"  , fd.m_metrics.m_height          );
        printf( "Offset          : %d\n"  , fd.m_metrics.m_offset          );
        printf( "Ascent          : %d\n"  , fd.m_metrics.m_ascent          );
        printf( "Descent         : %d\n"  , fd.m_metrics.m_descent         );
        printf( "InternalLeading : %d\n"  , fd.m_metrics.m_internalLeading );
        printf( "ExternalLeading : %d\n"  , fd.m_metrics.m_externalLeading );
        printf( "AveCharWidth    : %d\n"  , fd.m_metrics.m_aveCharWidth    );
        printf( "MaxCharWidth    : %d\n\n", fd.m_metrics.m_maxCharWidth    );

        printf( "Max Char Ascent : %d\n"  , maxAscent                      );
        printf( "Max Char Descent: %d\n\n", maxDescent                     );
    }

    lastChar = 0xFFFF;
    CLR_UINT32 totalOffset = 0;
            
    ptrRanges    = font->m_ranges;
    ptrRangesEx  = font->m_rangesEx;
    ptrChars     = font->m_chars;
    ptrCharsEx   = font->m_charsEx;
    ptrAntiAlias = font->m_antiAliasingData;
    
    for(it = itBegin; it != itEnd; it++)
    {
        GlyphDescription& gd      = it->second;
        int               cWidth  = gd.m_realBox.Width ();
        int               cHeight = gd.m_realBox.Height();
        int               cTop;


        if((lastChar != 0xFFFF && (lastChar + 1) != gd.m_char) || ((int)ptrChars[ 0 ].m_offset) + cWidth > 0xFFFF)
        {
            ptrRanges++;

            lastChar = 0xFFFF;

            if(ptrRangesEx) ptrRangesEx++;
        }

        if(lastChar == 0xFFFF)
        {
            ptrRanges->m_indexOfFirstFontCharacter = (CLR_UINT16)(ptrChars - font->m_chars);
            ptrRanges->m_firstChar = gd.m_char;

            ptrChars->m_offset = 0;

            if(ptrRangesEx) ptrRangesEx->m_offsetAntiAlias = iAntiAliasData; 
        }

        ptrRanges->m_lastChar = gd.m_char;
        lastChar              = gd.m_char;

        ptrChars[ 1 ].m_offset      = ptrChars[ 0 ].m_offset + cWidth;
        ptrChars[ 0 ].m_marginLeft  =                   gd.m_realBox.left      + gd.m_boxX  + gd.m_metrics.m_marginLeft ;
        ptrChars[ 0 ].m_marginRight = gd.m_cellWidth - (gd.m_realBox.right + 1 + gd.m_boxX) + gd.m_metrics.m_marginRight;

        totalOffset += cWidth;
        ptrRanges[ 1 ].m_rangeOffset = totalOffset;

        cTop = (fd.m_metrics.m_ascent - gd.m_metrics.m_ascent) - fd.m_metrics.m_offset + gd.m_realBox.top + gd.m_boxY;

        BitBlt( font->m_bitmap, ptrChars->m_offset + ptrRanges->m_rangeOffset, cTop, cWidth, cHeight, gd.m_boxPixels, gd.m_realBox.left, gd.m_realBox.top );

        if(ptrCharsEx)
        {
            _ASSERTE(ptrAntiAlias);
            _ASSERTE(ptrRangesEx);
            _ASSERTE(font->m_font.m_flags & CLR_GFX_FontDescription::c_FontEx);
            _ASSERTE(iAntiAliasData >= ptrRangesEx->m_offsetAntiAlias); 
            _ASSERTE(iAntiAliasData - ptrRangesEx->m_offsetAntiAlias < CLR_GFX_FontCharacterEx::c_offsetNoAntiAlias);

            CLR_UINT32 cBytes = gd.m_antiAliasData.m_cBytesSet;
           
            if(cBytes > 0)
            {
                _ASSERTE(gd.m_antiAliasData.m_bitsPerPixel == m_iAntiAlias);
                ptrCharsEx->m_offsetAntiAlias = iAntiAliasData - ptrRangesEx->m_offsetAntiAlias;

                memcpy(ptrAntiAlias, gd.m_antiAliasData.m_data, cBytes);
            }
            else
            {
                ptrCharsEx->m_offsetAntiAlias = CLR_GFX_FontCharacterEx::c_offsetNoAntiAlias;
            }

            iAntiAliasData += cBytes;                
            ptrAntiAlias += cBytes;
            ptrCharsEx++;
        }

        if(m_iVerboseLevel > 1)
        {
            printf( "Char: %c [Cell: %d x %d] [Margin: L:%d  R:%d]\n", gd.m_char, gd.m_cellWidth, gd.m_cellHeight, ptrChars[ 0 ].m_marginLeft, ptrChars[ 0 ].m_marginRight );

            for(int x=0; x<cWidth+2; x++) printf( "+" );
            printf( "\n" );

            for(int y=0; y<(int)font->m_bitmap.m_bm.m_height; y++)
            {
                printf( "+" );
                for(int x=0; x<cWidth; x++)
                {
                    printf( "%c", font->m_bitmap.GetPixel( ptrChars->m_offset + ptrRanges->m_rangeOffset + x, y ) == 0x1 ? '#' : ' ' );
                }
                printf( "+\n" );
            }

            for(int x=0; x<cWidth+2; x++) printf( "+" );
            printf( "\n" );
        }

        ptrChars++;
    }

    // Change the offset of the last character (the sentinel character in the sentinel range) to 0
    // since it has been accounted for in the range offset.
    ptrChars->m_offset = 0;

    if (m_forceDefaultCharacter)
    {
        if(ptrCharsEx)
        {
            //This is just a rectangle. Do we care about anti-aliasing the default char?
            ptrCharsEx->m_offsetAntiAlias = CLR_GFX_FontCharacterEx::c_offsetNoAntiAlias;
            ptrCharsEx++;
        }

        int defCharWidth = fd.m_metrics.m_aveCharWidth;

        ptrRanges++;

        ptrRanges[ 0 ].m_firstChar                 = CLR_GFX_Font::c_UnicodeReplacementCharacter;
        ptrRanges[ 0 ].m_lastChar                  = CLR_GFX_Font::c_UnicodeReplacementCharacter;
        ptrRanges[ 0 ].m_indexOfFirstFontCharacter = (CLR_UINT16)(ptrChars - font->m_chars);
        ptrRanges[ 0 ].m_rangeOffset               = totalOffset;
        ptrRanges[ 1 ].m_rangeOffset               = totalOffset + defCharWidth;

        ptrChars[ 0 ].m_marginLeft  = 1;
        ptrChars[ 0 ].m_marginRight = 1;
        ptrChars[ 1 ].m_offset      = 0;

        DrawDefaultCharacter( font->m_bitmap, totalOffset, 1, defCharWidth, fd.m_metrics.m_ascent );
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_GFX_FontBuilder::LoadFromScript( const CLR_RT_Buffer& buffer )
{
    TINYCLR_HEADER();

    std::vector<std::wstring> fonts;
    std::vector<std::wstring> argv;
    int                       argc;
    ENUMLOGFONTEXW            elf;
    bool                      elfValid = false;
    CLR_RT_Buffer             fntData;
    bool                      fntValid = false;
    bool                      importRangeAndMap;
    bool                      setAsDefaultCharacter;
    bool                      setDefaultCharacter;       

    CLR_RT_FileStore::ExtractTokens( buffer, argv );

    argc = (int)argv.size();

    for(int i=0; i<argc; i++)
    {
        LPCWSTR arg = argv[ i ].c_str();

        if(!_wcsicmp( arg, L"Verbosity" ) && i+1 < argc)
        {
            m_iVerboseLevel = _wtoi( argv[ ++i ].c_str() );

            continue;
        }

        if(!_wcsicmp( arg, L"AntiAlias" ) && i+1 < argc)
        {
            m_iAntiAlias = _wtoi( argv[ ++i ].c_str() );

            switch(m_iAntiAlias)
            {
                case 1:
                case 2:
                case 4:
                case 8:
                    break;
                default:
                    wprintf( L"AntiAliasing value not supported\n" );
                    TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            }

            if(m_iAntiAlias > 1)
            {
                if(m_iAntiAliasPersist > 1 && m_iAntiAlias != m_iAntiAliasPersist)
                {                                        
                    wprintf( L"Only one antiAliasing value per font\n" );
                    TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
                }

                m_iAntiAliasPersist = m_iAntiAlias;
            }

            continue;
        }

        if(!_wcsicmp( arg, L"AddFontToProcess" ) && i+1 < argc)
        {
            std::wstring& fontFile = argv[ ++i ];

            fonts.push_back( fontFile );

            if(::AddFontResourceExW( fontFile.c_str(), FR_PRIVATE, NULL ) == 0)
            {
                wprintf( L"Cannot load font specified in AddFontToProcess: %s\n", fontFile.c_str() );

                TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            }

            continue;
        }


        if(!_wcsicmp( arg, L"SelectFont" ) && i+1 < argc)
        {
            LPCWSTR arg2 = argv[ ++i ].c_str();

            if(StringEndsWithSuffix( arg2, L".fnt" ))
            {
                TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::LoadFile( arg2, fntData ));

                fntValid    = true;
                elfValid    = false;
            }
            else
            {
                TINYCLR_CHECK_HRESULT(CLR_GFX_FontBuilder::DecodeDefinition( elf, arg2 ));

                fntValid    = false;
                elfValid    = true;
            }
            continue;
        }

        importRangeAndMap     = false;
        setAsDefaultCharacter = false;
        setDefaultCharacter   = false;

        if((importRangeAndMap     = (!_wcsicmp( arg, L"ImportRangeAndMap"     ) && i+3 < argc)) == true ||
           (                         !_wcsicmp( arg, L"ImportRange"           ) && i+2 < argc)  == true ||
           (setAsDefaultCharacter = (!_wcsicmp( arg, L"SetAsDefaultCharacter" ) && i+1 < argc)) == true ||
           (setDefaultCharacter   =  !_wcsicmp( arg, L"SetDefaultCharacter"   ))                == true  )
        {
            if(elfValid == false && fntValid == false)
            {
                wprintf( L"SelectFont command not found in .FNTDEF file\n" );
                TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            }

            int firstChar, lastChar, mappedFirstChar;
            
            if (setDefaultCharacter)
            {
                CLR_UINT16 defChar;

                if (fntValid) GetDefaultCharFNT( fntData, defChar ) ;
                else TINYCLR_CHECK_HRESULT(GetDefaultCharTTF( elf, defChar ));

                firstChar = defChar;
            }
            else
            {
                firstChar = _wtoi( argv[ ++i ].c_str() );
            }

            if (setDefaultCharacter || setAsDefaultCharacter) lastChar = firstChar;
            else                                              lastChar = _wtoi( argv[ ++i ].c_str() );

            if      (importRangeAndMap                           ) mappedFirstChar = _wtoi( argv[ ++i ].c_str() );
            else if (setDefaultCharacter || setAsDefaultCharacter) mappedFirstChar = CLR_GFX_Font::c_UnicodeReplacementCharacter;
            else  /* ImportRange                                */ mappedFirstChar = firstChar;

            if(fntValid)
            {
                TINYCLR_CHECK_HRESULT(LoadFromFNT( fntData, (CLR_UINT16)firstChar, (CLR_UINT16)lastChar, (CLR_UINT16)mappedFirstChar ));
            }
            else
            {
                TINYCLR_CHECK_HRESULT(LoadFromTTF( elf, (CLR_UINT16)firstChar, (CLR_UINT16)lastChar, (CLR_UINT16)mappedFirstChar ));
            }
            continue;
        }

        if(!_wcsicmp( arg, L"OffsetX" ) && i+1 < argc)
        {
            m_adjust.m_offsetX = -_wtoi( argv[ ++i ].c_str() );
            continue;
        }

        if(!_wcsicmp( arg, L"OffsetY" ) && i+1 < argc)
        {
            m_adjust.m_offsetY = -_wtoi( argv[ ++i ].c_str() );
            continue;
        }

        if(!_wcsicmp( arg, L"AdjustLeftMargin" ) && i+1 < argc)
        {
            m_adjust.m_marginLeft = _wtoi( argv[ ++i ].c_str() );
            continue;
        }

        if(!_wcsicmp( arg, L"AdjustRightMargin" ) && i+1 < argc)
        {
            m_adjust.m_marginRight = _wtoi( argv[ ++i ].c_str() );
            continue;
        }

        if(!_wcsicmp( arg, L"AdjustAscent" ) && i+1 < argc)
        {
            m_adjust.m_ascent = _wtoi( argv[ ++i ].c_str() );
            continue;
        }

        if(!_wcsicmp( arg, L"AdjustDescent" ) && i+1 < argc)
        {
            m_adjust.m_descent = _wtoi( argv[ ++i ].c_str() );
            continue;
        }

        if(!_wcsicmp( arg, L"AdjustInternalLeading" ) && i+1 < argc)
        {
            m_adjust.m_internalLeading = _wtoi( argv[ ++i ].c_str() );
            continue;
        }

        if(!_wcsicmp( arg, L"AdjustExternalLeading" ) && i+1 < argc)
        {
            m_adjust.m_externalLeading = _wtoi( argv[ ++i ].c_str() );
            continue;
        }

        if(!_wcsicmp( arg, L"NoDefaultCharacter" ))
        {
            m_forceDefaultCharacter = false;
            continue;
        }

        //--//

        wprintf( L"Unknown option: %s\n", arg );
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }


    TINYCLR_CLEANUP();

    for(size_t j=0; j<fonts.size(); j++)
    {
        ::RemoveFontResourceExW( fonts[ j ].c_str(), FR_PRIVATE, NULL );
    }

    TINYCLR_CLEANUP_END();
}

HRESULT CLR_GFX_FontBuilder::UpdateScript( CLR_RT_Buffer& buffer )
{
    TINYCLR_HEADER();

    std::vector<std::wstring> fonts;
    std::vector<std::wstring> argv;
    int                       argc;
    ENUMLOGFONTEXW            elf;
    bool                      elfValid = false;
    CLR_RT_Buffer             fntData;
    bool                      fntValid = false;
    std::wstring              selectFontNew;
    std::wstring              selectFontOld;

    CLR_RT_FileStore::ExtractTokens( buffer, argv );

    argc = (int)argv.size();

    for(int i=0; i<argc; i++)
    {
        LPCWSTR arg = argv[ i ].c_str();

        if(!_wcsicmp( arg, L"AddFontToProcess" ) && i+1 < argc)
        {
            std::wstring& fontFile = argv[ ++i ];

            fonts.push_back( fontFile );

            if(::AddFontResourceExW( fontFile.c_str(), FR_PRIVATE, NULL ) == 0)
            {
                wprintf( L"Cannot load font specified in AddFontToProcess: %s\n", fontFile.c_str() );

                TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            }
        }
        else if(!_wcsicmp( arg, L"SelectFont" ) && i+1 < argc)
        {
            LPCWSTR arg2 = argv[ ++i ].c_str();

            selectFontOld = arg2;

            if(!StringEndsWithSuffix( arg2, L".fnt" ))
            {
                TINYCLR_CHECK_HRESULT(CLR_GFX_FontBuilder::DecodeDefinition( elf, arg2 ));

                //make sure that this hasn't been updated
                if(!elf.elfFullName[ 0 ] && !elf.elfScript[ 0 ] && !elf.elfStyle[ 0 ])
                {
                    TINYCLR_CHECK_HRESULT(CLR_GFX_FontBuilder::InitializeEnumLogFontExFromLogFont( elf, elf.elfLogFont ));

                    //select font, get real definition...update string
                    TINYCLR_CHECK_HRESULT(CLR_GFX_FontBuilder::EncodeDefinition( elf, selectFontNew ));

                    ReplaceChar( (LPWSTR)(&elf.elfLogFont.lfFaceName), '_', ' ' );

                    wprintf( L"SelectFont \"%s\"\n", selectFontNew.c_str() );
                }
            }
        }
    }

    TINYCLR_CLEANUP();

    for(size_t j=0; j<fonts.size(); j++)
    {
        ::RemoveFontResourceExW( fonts[ j ].c_str(), FR_PRIVATE, NULL );
    }

    TINYCLR_CLEANUP_END();
}

//--//

static void AppendPrintf( std::wstring& buffer, LPCWSTR format, ... )
{
    WCHAR   tmp[ 512 ];
    va_list arg;

    va_start( arg, format );

    _vsnwprintf_s( tmp, ARRAYSIZE(tmp), MAXSTRLEN(tmp), format, arg ); tmp[ MAXSTRLEN(tmp) ] = 0;

    va_end( arg );

    if(buffer.length() > 0) buffer += L",";

    buffer += tmp;
}

static bool ParseString( LPCWSTR src, size_t srcLen, LPCWSTR prefix, LPWSTR dest, size_t destLen )
{
    size_t prefixLen = wcslen( prefix );

    if(!_wcsnicmp( src, prefix, prefixLen ))
    {                
        wcsncpy_s( dest, destLen, src+prefixLen, min( srcLen - prefixLen, destLen) );

        return true;
    }

    return false;
}

template<typename T> bool ParseNumber( LPCWSTR src, LPCWSTR prefix, T& dest )
{
    size_t prefixLen = wcslen( prefix );

    if(!_wcsnicmp( src, prefix, prefixLen ))
    {
        int res;

        if(swscanf_s( src+prefixLen, L"%d", &res ) != 1) return false;

        dest = (T)res;
        return true;
    }

    return false;
}

//--//

HRESULT CLR_GFX_FontBuilder::EncodeDefinition( const ENUMLOGFONTEXW& elf, std::wstring& buffer )
{
    TINYCLR_HEADER();

    LOGFONTW lf = elf.elfLogFont;

    buffer.clear();
                                  AppendPrintf( buffer, L"FN:%s"      ,       lf.lfFaceName       );
    if( lf.lfHeight         != 0) AppendPrintf( buffer, L"HE:%d"      , (int) lf.lfHeight         );
    if( lf.lfWidth          != 0) AppendPrintf( buffer, L"WI:%d"      , (int) lf.lfWidth          );
    if( lf.lfEscapement     != 0) AppendPrintf( buffer, L"ES:%d"      , (int) lf.lfEscapement     );
    if( lf.lfOrientation    != 0) AppendPrintf( buffer, L"OR:%d"      , (int) lf.lfOrientation    );
    if( lf.lfWeight         != 0) AppendPrintf( buffer, L"WE:%d"      , (int) lf.lfWeight         );
    if( lf.lfItalic         != 0) AppendPrintf( buffer, L"IT:%d"      , (int) lf.lfItalic         );
    if( lf.lfUnderline      != 0) AppendPrintf( buffer, L"UN:%d"      , (int) lf.lfUnderline      );
    if( lf.lfStrikeOut      != 0) AppendPrintf( buffer, L"ST:%d"      , (int) lf.lfStrikeOut      );
    if( lf.lfCharSet        != 0) AppendPrintf( buffer, L"CS:%d"      , (int) lf.lfCharSet        );
    if( lf.lfOutPrecision   != 3) AppendPrintf( buffer, L"OP:%d"      , (int) lf.lfOutPrecision   );
    if( lf.lfClipPrecision  != 2) AppendPrintf( buffer, L"CP:%d"      , (int) lf.lfClipPrecision  );
    if( lf.lfQuality        != 1) AppendPrintf( buffer, L"QA:%d"      , (int) lf.lfQuality        );
    if( lf.lfPitchAndFamily != 0) AppendPrintf( buffer, L"PF:%d"      , (int) lf.lfPitchAndFamily );
    if(elf.elfFullName[ 0 ]       ) AppendPrintf( buffer, L"FullName:%s",      elf.elfFullName      );
    if(elf.elfScript  [ 0 ]       ) AppendPrintf( buffer, L"Script:%s"  ,      elf.elfScript        );
    if(elf.elfStyle   [ 0 ]       ) AppendPrintf( buffer, L"Style:%s"   ,      elf.elfStyle         );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT CLR_GFX_FontBuilder::DecodeDefinition( ENUMLOGFONTEXW& elf, const std::wstring& buffer )
{
    TINYCLR_HEADER();

    LPCWSTR src = buffer.c_str();

    TINYCLR_CLEAR(elf);

    LOGFONTW lf = elf.elfLogFont;

    lf.lfOutPrecision  = 3;   //OUT_STROKE_PRECIS
    lf.lfClipPrecision = 2;   //CLIP_STROKE_PRECIS
    lf.lfQuality       = 1;   //DRAFT_QUALITY
    lf.lfCharSet       = DEFAULT_CHARSET;

    while(src[ 0 ])
    {
        LPCWSTR end = wcschr( src, ',' );
        size_t  len = wcslen( src      );

        if(end)
        {
            len = end - src;
        }

        while(true)
        {
            if(ParseNumber( src,      L"HE:",       lf .lfHeight                                 )) break;
            if(ParseNumber( src,      L"WI:",       lf .lfWidth                                  )) break;
            if(ParseNumber( src,      L"ES:",       lf .lfEscapement                             )) break;
            if(ParseNumber( src,      L"OR:",       lf .lfOrientation                            )) break;
            if(ParseNumber( src,      L"WE:",       lf .lfWeight                                 )) break;
            if(ParseNumber( src,      L"IT:",       lf .lfItalic                                 )) break;
            if(ParseNumber( src,      L"UN:",       lf .lfUnderline                              )) break;
            if(ParseNumber( src,      L"ST:",       lf .lfStrikeOut                              )) break;
            if(ParseNumber( src,      L"CS:",       lf .lfCharSet                                )) break;
            if(ParseNumber( src,      L"OP:",       lf .lfOutPrecision                           )) break;
            if(ParseNumber( src,      L"CP:",       lf .lfClipPrecision                          )) break;
            if(ParseNumber( src,      L"QA:",       lf .lfQuality                                )) break;
            if(ParseNumber( src,      L"PF:",       lf .lfPitchAndFamily                         )) break;

            if(ParseString( src, len, L"FN:"      , lf .lfFaceName , MAXSTRLEN(lf .lfFaceName  ) )) break;
            if(ParseString( src, len, L"FullName:", elf.elfFullName, MAXSTRLEN(elf.elfFullName ) )) break;
            if(ParseString( src, len, L"Style:"   , elf.elfStyle   , MAXSTRLEN(elf.elfStyle    ) )) break;
            if(ParseString( src, len, L"Script:"  , elf.elfScript  , MAXSTRLEN(elf.elfScript   ) )) break;

            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        if(!end) break;

        src = end+1;
    }

    elf.elfLogFont = lf;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_GFX_FontBuilder::InitializeEnumLogFontExFromLogFont( ENUMLOGFONTEXW& elf, LOGFONTW lf )
{
    TINYCLR_HEADER();

    HDC                 hdc     = NULL;
    HFONT               font    = NULL;
    HGDIOBJ             fontOld = NULL;
    DWORD               size;
    OUTLINETEXTMETRICW* otm = NULL;

    TINYCLR_CLEAR(elf);

    hdc     = ::CreateCompatibleDC( NULL ); FAULT_ON_NULL(hdc);
    font    = ::CreateFontIndirectW( &lf ); FAULT_ON_NULL(font);
    fontOld = ::SelectObject       ( hdc, font );

    size = ::GetOutlineTextMetricsW( hdc, 0, NULL ); FAULT_ON_NULL(size);

    otm = (OUTLINETEXTMETRICW*)malloc( size ); FAULT_ON_NULL(otm);

    CLR_RT_Memory::ZeroFill( otm, size ); otm->otmSize = sizeof(*otm);

    size = ::GetOutlineTextMetricsW( hdc, size, otm ); FAULT_ON_NULL(size);

    //Use what we can from the realized font, not the logical font
    lf.lfItalic         = (otm->otmTextMetrics.tmItalic     != 0);
    lf.lfWeight         = (otm->otmTextMetrics.tmWeight         );
    lf.lfPitchAndFamily = (otm->otmTextMetrics.tmPitchAndFamily );
    lf.lfUnderline      = (otm->otmTextMetrics.tmUnderlined != 0);
    lf.lfCharSet        = (otm->otmTextMetrics.tmCharSet        );
    lf.lfStrikeOut      = (otm->otmTextMetrics.tmStruckOut  != 0);

    elf.elfLogFont = lf;

    wcsncpy_s( elf.elfLogFont.lfFaceName, ARRAYSIZE(elf.elfLogFont.lfFaceName), (LPCWSTR)((char *)otm + (size_t)otm->otmpFaceName ), MAXSTRLEN(elf.elfLogFont.lfFaceName) );
    wcsncpy_s( elf.elfFullName          , ARRAYSIZE(elf.elfFullName          ), (LPCWSTR)((char *)otm + (size_t)otm->otmpFullName ), MAXSTRLEN(elf.elfFullName          ) );
    wcsncpy_s( elf.elfStyle             , ARRAYSIZE(elf.elfStyle             ), (LPCWSTR)((char *)otm + (size_t)otm->otmpStyleName), MAXSTRLEN(elf.elfStyle             ) );

    lf .lfFaceName [ MAXSTRLEN(lf .lfFaceName ) ] = 0;
    elf.elfFullName[ MAXSTRLEN(elf.elfFullName) ] = 0;
    elf.elfStyle   [ MAXSTRLEN(elf.elfStyle   ) ] = 0;
    elf.elfStyle   [ MAXSTRLEN(elf.elfScript  ) ] = 0;

    TINYCLR_CLEANUP();

    if(otm)
    {
        free( otm );
    }

    if(font)
    {
        ::SelectObject( hdc, fontOld );
        ::DeleteObject(      font    );
    }

    if(hdc)
    {
        ::DeleteDC( hdc );
    }

   TINYCLR_CLEANUP_END();
}

HRESULT CLR_GFX_FontBuilder::SelectFont( ENUMLOGFONTEXW& elf, bool fInit )
{
    TINYCLR_HEADER();

    HDC                 hdc     = NULL;
    HFONT               font    = NULL;
    HGDIOBJ             fontOld = NULL;
    OUTLINETEXTMETRICW* otm = NULL;
    LOGFONTW            lf;

    CHOOSEFONTW cf; TINYCLR_CLEAR(cf); cf.lStructSize = sizeof(cf);

    lf = elf.elfLogFont;

    cf.lpLogFont = &lf;
    cf.Flags     = CF_SCREENFONTS;

    if(fInit)
    {
        cf.Flags |= CF_INITTOLOGFONTSTRUCT;
    }
    else
    {
        TINYCLR_CLEAR(lf);
    }

    if(::ChooseFontW( &cf ) == FALSE)
    {
        TINYCLR_SET_AND_LEAVE(E_ABORT);
    }

    TINYCLR_CHECK_HRESULT(CLR_GFX_FontBuilder::InitializeEnumLogFontExFromLogFont( elf, lf ));

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_GFX_Font::SaveToBuffer( CLR_RT_Buffer& buffer )
{
    TINYCLR_HEADER();

    int sizeHeader1 = sizeof(m_font);
    int sizeHeader2 = sizeof(m_bitmap.m_bm);
    int block1      = m_font       .GetRangeSize    ();
    int block2      = m_font       .GetCharacterSize();
    int block3      = m_bitmap.m_bm.GetTotalSize    ();
    int block4      = sizeof(m_fontEx);
    int block5      = m_font.GetRangeExSize();
    int block6      = m_font.GetCharacterExSize();
    int block7      = m_fontEx.GetAntiAliasSize();
    int size        = sizeHeader1 + sizeHeader2 + block1 + block2 + block3;
    bool fFontEx    = (m_font.m_flags & CLR_GFX_FontDescription::c_FontEx) != 0;

    if(fFontEx)
    {
        size += block4 + block5 + block6 + block7;
    }

    buffer.resize( size );

    CLR_UINT8* ptr = &buffer[ 0 ];

    memcpy( ptr, &m_font                   , sizeHeader1 ); ptr += sizeHeader1;
    memcpy( ptr, &m_bitmap.m_bm            , sizeHeader2 ); ptr += sizeHeader2;
    memcpy( ptr,  m_ranges                 , block1      ); ptr += block1;
    memcpy( ptr,  m_chars                  , block2      ); ptr += block2;
    memcpy( ptr,  m_bitmap.m_palBitmap.data, block3      ); ptr += block3;

    if(fFontEx)
    {
        memcpy( ptr, &m_fontEx       , block4      ); ptr += block4;
        memcpy( ptr,  m_rangesEx     , block5      ); ptr += block5;
        memcpy( ptr,  m_charsEx      , block6      ); ptr += block6;
        memcpy( ptr,  m_antiAliasingData, block7   ); ptr += block7;
    }

    TINYCLR_NOCLEANUP_NOLABEL();
}

void CLR_GFX_FontBuilder::BitBlt( const CLR_GFX_Bitmap& target, int xDst, int yDst, int nWidth, int nHeight, FontBitmap& src, int xSrc, int ySrc )
{
    int widthInWords = (target.m_bm.m_width + 31) / 32;

    for (int y = 0; y < nHeight; y++)
    {
        for (int x = 0; x < nWidth; x++)
        {
            if (src.GetPixel( xSrc + x, ySrc + y ))
            {
                SetPixel( target.m_palBitmap.data, xDst+x, yDst+y, widthInWords );
            }
        }
    }
}

void CLR_GFX_FontBuilder::DrawDefaultCharacter( const CLR_GFX_Bitmap& target, int x, int y, int width, int height )
{
    // The default character is a rectangle
    int widthInWords = (target.m_bm.m_width + 31) / 32;
    int i;

    // Draw the top and bottom lines
    for(i = 0; i < width; i++)
    {
        SetPixel( target.m_palBitmap.data, x + i, y             , widthInWords );
        SetPixel( target.m_palBitmap.data, x + i, y + height - 1, widthInWords );
    }

    // the left and right lines
    for(i = 0; i < height; i++)
    {
        SetPixel( target.m_palBitmap.data, x            , y + i, widthInWords );
        SetPixel( target.m_palBitmap.data, x + width - 1, y + i, widthInWords );
    }
}

void CLR_GFX_FontBuilder::SetPixel( CLR_UINT32* bitmap, int x, int y, int widthInWords )
{
    CLR_UINT32* word = bitmap + (y * widthInWords) + (x / 32);

    (*word) |= 1 << (x % 32);
}
