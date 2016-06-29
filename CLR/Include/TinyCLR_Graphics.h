////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _TINYCLR_GRAPHICS_H_
#define _TINYCLR_GRAPHICS_H_

#include <TinySupport.h>
#include <TinyCLR_Runtime.h>

/////////////////////////////////////////////////////////////////////////////

struct CLR_GFX_Bitmap;
struct CLR_GFX_Font;

//--//

struct CLR_GFX_BitmapDescription
{
    static const CLR_UINT16 c_ReadOnly     = 0x0001;
    static const CLR_UINT16 c_Compressed   = 0x0002;

    static const CLR_UINT8 c_CompressedRun              = 0x80;
    static const CLR_UINT8 c_CompressedRunSet           = 0x40;
    static const CLR_UINT8 c_CompressedRunLengthMask    = 0x3f;
    static const CLR_UINT8 c_UncompressedRunLength      = 7;
    static const CLR_UINT8 c_CompressedRunOffset        = c_UncompressedRunLength+1;

    // Note that these type definitions has to match the ones defined in Bitmap.BitmapImageType enum defined in Graphics.cs
    static const CLR_UINT8 c_TypeTinyCLRBitmap          = 0;
    static const CLR_UINT8 c_TypeGif                    = 1;
    static const CLR_UINT8 c_TypeJpeg                   = 2;
    static const CLR_UINT8 c_TypeBmp                    = 3; // The windows .bmp format

    // When m_bitsPerPixel == c_NativeBpp it means that the data in this bitmap is in the native PAL graphics 
    // format, the exact bit depth and format is something the CLR is abstracted away from. 
    static const int c_NativeBpp = 0;

    static const CLR_UINT32 c_MaxWidth  = 524287;  // 0x7ffff;
    static const CLR_UINT32 c_MaxHeight = 65535;   // 0x0ffff;

    // !!!!WARNING!!!!
    // These fields should correspond to CLR_GFX_BitmapDescription in GenerateResource.cs
    // and should be 4-byte aligned in size. When these fields are changed, the version number 
    // of the tinyresource file should be incremented, the tinyfnts should be updated (buildhelper -convertfont ...)
    // and the MMP should also be updated as well.
    CLR_UINT32 m_width;
    CLR_UINT32 m_height;
    CLR_UINT16 m_flags;
    CLR_UINT8  m_bitsPerPixel;
    CLR_UINT8  m_type;
    
    bool BitmapDescription_Initialize( int width, int height, int bitsPerPixel );

    int GetTotalSize   () const;
    int GetWidthInWords() const;
};

struct CLR_GFX_FontMetrics
{
    CLR_UINT16 m_height;
    CLR_INT16  m_offset; // The bitmap could be actually smaller than the logical font height.

    CLR_INT16  m_ascent;
    CLR_INT16  m_descent;

    CLR_INT16  m_internalLeading;
    CLR_INT16  m_externalLeading;

    CLR_INT16  m_aveCharWidth;
    CLR_INT16  m_maxCharWidth;
};

struct CLR_GFX_FontMetricsExtended : public CLR_GFX_FontMetrics
{
    int m_offsetX;
    int m_offsetY;
    int m_marginLeft;
    int m_marginRight;
};

struct CLR_GFX_FontDescription
{
    static const CLR_UINT16 c_Bold           = 0x0001;
    static const CLR_UINT16 c_Italic         = 0x0002;
    static const CLR_UINT16 c_Underline      = 0x0004;
    static const CLR_UINT16 c_FontEx         = 0x0008;
    static const CLR_UINT16 c_AntiAliasMask  = 0x00F0;
    static const CLR_UINT16 c_AntiAliasShift = 4;

    CLR_GFX_FontMetrics m_metrics;
    CLR_UINT16          m_ranges;
    CLR_UINT16          m_characters;
    CLR_UINT16          m_flags;
    CLR_UINT16          m_pad;
    
    CLR_UINT32 GetRangeSize      () const;
    CLR_UINT32 GetCharacterSize  () const;
    CLR_UINT32 GetCharacterExSize() const;
    CLR_UINT32 GetRangeExSize    () const;
};

struct CLR_GFX_FontDescriptionEx
{
    CLR_UINT32 m_antiAliasSize;
    
    CLR_UINT32 GetAntiAliasSize() const;
};

struct CLR_GFX_FontCharacterRange
{
    CLR_UINT32 m_indexOfFirstFontCharacter;

    CLR_UINT16 m_firstChar;
    CLR_UINT16 m_lastChar;

    CLR_UINT32 m_rangeOffset; // the x-offset into the bitmap for this range.
};

struct CLR_GFX_FontCharacterRangeEx
{
    CLR_UINT32 m_offsetAntiAlias;
};

struct CLR_GFX_FontCharacter
{
    CLR_UINT16 m_offset;
    CLR_INT8   m_marginLeft;
    CLR_INT8   m_marginRight;
};

struct CLR_GFX_FontCharacterEx
{
    static const CLR_UINT16 c_offsetNoAntiAlias = 0xFFFF;    

    CLR_UINT16 m_offsetAntiAlias;
};

struct CLR_GFX_FontCharacterInfo
{
    bool       isValid;
    CLR_INT8   marginLeft;
    CLR_INT8   marginRight;
    CLR_UINT32 offset;
    CLR_UINT16 innerWidth;
    CLR_UINT16 width;
    CLR_UINT16 height;
    CLR_UINT8* antiAlias;
    CLR_UINT8  iAntiAlias;
};
//--//

struct CLR_GFX_Bitmap
{
    static const int FIELD__m_bitmap = 1;

    // These have to be kept in sync with the Microsft.SPOT.Bitmap.DT_ flags
    static const CLR_UINT32 c_DrawText_WordWrap         = 0x00000001;
    // Note - 0x00000002 is used for Alignment (look below)
    static const CLR_UINT32 c_DrawText_TruncateAtBottom = 0x00000004;
    // Note - 0x00000008 is used for Trimming (look below)
    static const CLR_UINT32 c_DrawText_IgnoreHeight     = 0x00000010;

    static const CLR_UINT32 c_DrawText_AlignmentLeft    = 0x00000000;
    static const CLR_UINT32 c_DrawText_AlignmentCenter  = 0x00000002;
    static const CLR_UINT32 c_DrawText_AlignmentRight   = 0x00000020;
    static const CLR_UINT32 c_DrawText_AlignmentUnused  = 0x00000022;
    static const CLR_UINT32 c_DrawText_AlignmentMask    = 0x00000022;
    
    static const CLR_UINT32 c_DrawText_TrimmingNone              = 0x00000000;
    static const CLR_UINT32 c_DrawText_TrimmingWordEllipsis      = 0x00000008;
    static const CLR_UINT32 c_DrawText_TrimmingCharacterEllipsis = 0x00000040;
    static const CLR_UINT32 c_DrawText_TrimmingUnused            = 0x00000048;
    static const CLR_UINT32 c_DrawText_TrimmingMask              = 0x00000048;

    CLR_GFX_BitmapDescription m_bm;         // Initialized by the caller!

    PAL_GFX_Bitmap            m_palBitmap;  

    //--//

    static HRESULT CreateInstance( CLR_RT_HeapBlock& ref, const CLR_GFX_BitmapDescription& bm );
    static HRESULT CreateInstance( CLR_RT_HeapBlock& ref, const CLR_UINT8* data, CLR_UINT32 size, CLR_RT_Assembly* assm  );
    static HRESULT CreateInstance( CLR_RT_HeapBlock& ref, const CLR_UINT8* data, const CLR_UINT32 size, const CLR_UINT8 type );

    static HRESULT CreateInstanceJpeg( CLR_RT_HeapBlock& ref, const CLR_UINT8* data, const CLR_UINT32 size );
    static HRESULT CreateInstanceGif ( CLR_RT_HeapBlock& ref, const CLR_UINT8* data, const CLR_UINT32 size );
    static HRESULT CreateInstanceBmp ( CLR_RT_HeapBlock& ref, const CLR_UINT8* data, const CLR_UINT32 size );

    static HRESULT GetInstanceFromHeapBlock( const CLR_RT_HeapBlock& ref, CLR_GFX_Bitmap*& bitmap );
    //--//
    
    static HRESULT DeleteInstance( CLR_RT_HeapBlock& ref );

    //--//

    static UINT32 CreateInstanceJpegHelper  ( int x, int y, UINT32 flags, UINT16& opacity, void* param );

    static UINT32 ConvertToNative1BppHelper ( int x, int y, UINT32 flags, UINT16& opacity, void* param );
    static UINT32 ConvertToNative16BppHelper( int x, int y, UINT32 flags, UINT16& opacity, void* param );

    //--//

    void Bitmap_Initialize();

    void Clear();

    void SetClipping   ( GFX_Rect& rc );

    CLR_UINT32 GetPixel( int xPos, int yPos                   ) const;
    void       SetPixel( int xPos, int yPos, CLR_UINT32 color );

    //--//

    void DrawLine( const GFX_Pen& pen, int x0, int y0, int x1, int y1 );
    
    void DrawRectangle       ( const GFX_Pen& pen, const GFX_Brush& brush, const GFX_Rect& rectangle );
    void DrawRoundedRectangle( const GFX_Pen& pen, const GFX_Brush& brush, const GFX_Rect& rectangle, int radiusX, int radiusY );
    void DrawEllipse         ( const GFX_Pen& pen, const GFX_Brush& brush, int x, int y, int radiusX, int radiusY );

    void DrawImage( const GFX_Rect& dst, const CLR_GFX_Bitmap& bitmapSrc, const GFX_Rect& src, UINT16 opacity );
 
    void RotateImage( int angle, const GFX_Rect& dst, const CLR_GFX_Bitmap& bitmapSrc, const GFX_Rect& src, UINT16 opacity );

    void DrawText( LPCSTR str, CLR_GFX_Font& font, CLR_UINT32 color, int x, int y );
    
    static HRESULT DrawTextInRect( LPCSTR& szText, int& xRelStart, int& yRelStart, int& renderWidth, int& renderHeight, CLR_GFX_Bitmap* bm, int x, int y, int width, int height, CLR_UINT32 dtFlags, CLR_UINT32 color, CLR_GFX_Font* font );

    void SetPixelsHelper( const GFX_Rect& rect, UINT32 config, GFX_SetPixelsCallback callback, void* param );

    //--//

    void Relocate();

    static void RelocationHandler( CLR_RT_HeapBlock_BinaryBlob* ptr );

    //--//

private:
    void Decompress     ( const CLR_UINT8* data, const CLR_GFX_BitmapDescription* bm, CLR_UINT32 size );
    void ConvertToNative( const CLR_GFX_BitmapDescription* bmSrc, CLR_UINT32* dataSrc );

    //--//

    PROHIBIT_ALL_CONSTRUCTORS(CLR_GFX_Bitmap);
};

//--//

#if defined(_WIN32)

#include <vector>
#include <map>

// The font builder struct should probably be moved to managed code when we provide a more complete
// support for font. The struct's dependency on CLR_GFX_Bitmap's drawing capability is removed as part of 
// the moving drawing primitives to PAL. As a result, CLR_GFX_FontBuilder and one of its helper
// class, FontBitmap (used to call BitmapHolder) now contain a few simple 1bpp drawing methods to assist 
// in the construction of tinyfnt files.
struct CLR_GFX_FontBuilder
{
    struct FontBitmap
    {
        CLR_UINT32* m_data;
        CLR_UINT32  m_width;
        CLR_UINT32  m_height;
        CLR_UINT32  m_size;
        CLR_UINT32  m_widthInWords;

        //--//

        FontBitmap();
        ~FontBitmap();

        FontBitmap( const FontBitmap& bh );

        FontBitmap& operator=( const FontBitmap& bh );

        //--//

        void Init   ();
        void Release();

        HRESULT AllocatePixels( int width, int height );

        void SetPixel( int x, int y );
        CLR_UINT32 GetPixel( int x, int y );
    };
        
    struct AntiAliasHolder
    {
        CLR_UINT8* m_data;
        CLR_UINT32 m_cBytesAllocated;
        CLR_UINT32 m_pixelsSet;
        CLR_UINT32 m_cBytesSet;
        CLR_UINT32 m_width;
        CLR_UINT32 m_height;
        
        CLR_UINT32 m_bitsPerPixel;

        //--//

        AntiAliasHolder();
        ~AntiAliasHolder();

        AntiAliasHolder( const AntiAliasHolder& aah );

        AntiAliasHolder& operator=( const AntiAliasHolder& aah );
         
        //--//

        void Init   ();
        void Release();
        
        HRESULT AllocateData( int width, int height, int bitsPerPixel );

        void AppendData( CLR_UINT32 data );
    };

    struct GlyphDescription
    {
        CLR_UINT16                  m_char;
        CLR_GFX_FontMetricsExtended m_metrics;

        CLR_UINT16                  m_cellWidth;
        CLR_UINT16                  m_cellHeight;

        CLR_INT16                   m_boxX;
        CLR_INT16                   m_boxY;
        CLR_UINT16                  m_boxWidth;
        CLR_UINT16                  m_boxHeight;
        FontBitmap                  m_boxPixels;

        GFX_Rect                    m_realBox;
        AntiAliasHolder             m_antiAliasData;                                           
        int                         m_iAntiAlias;

        //--//

        GlyphDescription();

        HRESULT AllocatePixels();

        void SetPixel( int x, int y );
    };

    typedef std::map<CLR_UINT16,GlyphDescription> GlyphMap;
    typedef GlyphMap::iterator                    GlyphIter;

    GlyphMap                    m_characters;
    CLR_GFX_FontMetricsExtended m_adjust;
    int                         m_iVerboseLevel;
    bool                        m_forceDefaultCharacter;
    int                         m_iAntiAlias;
    int                         m_iAntiAliasPersist;

    //--//

    CLR_GFX_FontBuilder();

    HRESULT Initialize();

    // for LoadFromFNT / LoadFrom TTF, if getDefaultChar == true, then it maps the default character to mappedFirstChar, and ignores firstChar and lastChar params
    HRESULT LoadFromFNT   ( const CLR_RT_Buffer& buffer, CLR_UINT16 firstChar, CLR_UINT16 lastChar, CLR_UINT16 mappedFirstChar/*, bool getDefaultChar*/ );
    HRESULT LoadFromTTF   ( ENUMLOGFONTEXW&      elf   , CLR_UINT16 firstChar, CLR_UINT16 lastChar, CLR_UINT16 mappedFirstChar/*, bool getDefaultChar*/ );
    HRESULT LoadFromScript( const CLR_RT_Buffer& buffer                                                                        );

    HRESULT GenerateFont( CLR_GFX_Font*& font );

    static HRESULT EncodeDefinition                  ( const ENUMLOGFONTEXW& elf,       std::wstring&       buffer );
    static HRESULT DecodeDefinition                  (       ENUMLOGFONTEXW& elf, const std::wstring&       buffer );
    static HRESULT SelectFont                        (       ENUMLOGFONTEXW& elf,       bool                fInit  );
    static HRESULT InitializeEnumLogFontExFromLogFont(       ENUMLOGFONTEXW& elf,       LOGFONTW            lf     );
    static HRESULT VerifyFont                        ( const ENUMLOGFONTEXW& elf, const OUTLINETEXTMETRICW* otm    );
    static void VerifyFontWarning( const WCHAR* descriptor, const WCHAR* expected, const WCHAR* found );
    static void VerifyFontWarning( const WCHAR* descriptor, LONG expected, LONG found );
    static HRESULT UpdateScript( CLR_RT_Buffer& bufferOld );

    HRESULT GetDefaultCharTTF( ENUMLOGFONTEXW&      elf   , CLR_UINT16& defaultChar );
    void    GetDefaultCharFNT( const CLR_RT_Buffer& buffer, CLR_UINT16& defaultChar )
    {
        const CLR_UINT8* data = &buffer[ 0 ];
        defaultChar = data[ 97 ];
    }

    template<typename T> void MaximizeValue( T& dst, T src ) { if(dst < src) dst = src; }
    template<typename T> void MinimizeValue( T& dst, T src ) { if(dst > src) dst = src; }

private:
    // We implemented these very simple 1bpp drawing functions for the sole use of the construction of the tinyfnt
    void BitBlt( const CLR_GFX_Bitmap& target, int xDst, int yDst, int nWidth, int nHeight, FontBitmap& src, int xSrc, int ySrc );
    void SetPixel( CLR_UINT32* bitmap, int x, int y, int widthInWords );
    void DrawDefaultCharacter( const CLR_GFX_Bitmap& target, int x, int y, int width, int height );
};

#endif

//--//

struct CLR_GFX_Font
{
    static const int FIELD__m_font = 1;

    // Must keep in sync with Microsoft.SPOT.Font.DefaultKerning
    static const CLR_INT32 c_DefaultKerning = 1024;

    static const CLR_UINT16 c_UnicodeReplacementCharacter = 0xFFFD;

    CLR_GFX_FontDescription       m_font;
    CLR_GFX_FontCharacterRange*   m_ranges;
    CLR_GFX_FontCharacter*        m_chars;
    CLR_GFX_Bitmap                m_bitmap;
    CLR_GFX_FontCharacterInfo     m_defaultChar;

    CLR_GFX_FontDescriptionEx     m_fontEx;
    CLR_GFX_FontCharacterRangeEx* m_rangesEx;
    CLR_GFX_FontCharacterEx*      m_charsEx;
    CLR_UINT8*                    m_antiAliasingData;

    //--//

    static HRESULT CreateInstance( CLR_RT_HeapBlock& ref, const CLR_UINT8* data, CLR_RT_Assembly* assm );

    void Font_Initialize();

    int StringOut( LPCSTR str, int maxChars, CLR_INT32 kerning, CLR_GFX_Bitmap* bm, int xPos, int yPos, CLR_UINT32 color );

    void CountCharactersInWidth( LPCSTR str, int maxChars, int width, int& totWidth, bool fWordWrap, LPCSTR& strNext, int& numChars );

    void DrawChar( CLR_GFX_Bitmap* bitmap, CLR_GFX_FontCharacterInfo& chr, int xDst, int yDst, CLR_UINT32 color );

    static UINT32 DrawCharHelper ( int x, int y, UINT32 flags, UINT16& opacity, void* param );

    //--//

    void GetCharInfo( CLR_UINT16 c, CLR_GFX_FontCharacterInfo& chrEx );

    //--//

    void Relocate();

    static void RelocationHandler( CLR_RT_HeapBlock_BinaryBlob* ptr );

    //--//

#if defined(_WIN32)

    HRESULT SaveToBuffer( CLR_RT_Buffer& buffer );
        
#endif

    //--//

    PROHIBIT_ALL_CONSTRUCTORS(CLR_GFX_Font);
};

//--//

#if defined(_WIN32)

struct CLR_GFX_Resources
{
    static HBITMAP GetBitmap( UINT id );
    static LPCWSTR GetString( UINT id );

    static void GetString( UINT id, std::wstring& str );
};

#endif

//--//

#endif // _TINYCLR_GRAPHICS_H_
