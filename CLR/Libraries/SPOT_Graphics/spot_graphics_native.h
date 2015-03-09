////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#ifndef __SPOT_GRAPHICS_NATIVE_H__
#ifndef __SPOT_GRAPHICS_NATIVE_H__
#define __SPOT_GRAPHICS_NATIVE_H__  


struct Library_spot_graphics_native_Microsoft_SPOT_Font
{
    static const int FIELD__m_font = 1;

    TINYCLR_NATIVE_DECLARE(CharWidth___I4__CHAR);
    TINYCLR_NATIVE_DECLARE(get_Height___I4);
    TINYCLR_NATIVE_DECLARE(get_AverageWidth___I4);
    TINYCLR_NATIVE_DECLARE(get_MaxWidth___I4);
    TINYCLR_NATIVE_DECLARE(get_Ascent___I4);
    TINYCLR_NATIVE_DECLARE(get_Descent___I4);
    TINYCLR_NATIVE_DECLARE(get_InternalLeading___I4);
    TINYCLR_NATIVE_DECLARE(get_ExternalLeading___I4);
    TINYCLR_NATIVE_DECLARE(ComputeExtent___VOID__STRING__BYREF_I4__BYREF_I4__I4);
    TINYCLR_NATIVE_DECLARE(ComputeTextInRect___VOID__STRING__BYREF_I4__BYREF_I4__I4__I4__I4__I4__U4);

    //--//
        
    static HRESULT GetFont( CLR_RT_HeapBlock*  pThis, CLR_GFX_Font*& font );
    static HRESULT GetFont( CLR_RT_StackFrame& stack, CLR_GFX_Font*& font );

    //
    // compile time check to ensure the CLR_GFX_Font class knows about the interop definition
    // 
    struct CompileCheck
    {
        char buf1[ FIELD__m_font ==  CLR_GFX_Font::FIELD__m_font ? 1 : -1 ];        
    };
};

struct Library_spot_graphics_native_Microsoft_SPOT_Bitmap
{
    static const int FIELD_STATIC__MaxWidth  = 0;
    static const int FIELD_STATIC__MaxHeight = 1;
    static const int FIELD_STATIC__CenterX   = 2;
    static const int FIELD_STATIC__CenterY   = 3;

    static const int FIELD__m_bitmap = 1;

    TINYCLR_NATIVE_DECLARE(_ctor___VOID__I4__I4);
    TINYCLR_NATIVE_DECLARE(_ctor___VOID__SZARRAY_U1__MicrosoftSPOTBitmapBitmapImageType);
    TINYCLR_NATIVE_DECLARE(Flush___VOID);
    TINYCLR_NATIVE_DECLARE(Flush___VOID__I4__I4__I4__I4);
    TINYCLR_NATIVE_DECLARE(Clear___VOID);        
    TINYCLR_NATIVE_DECLARE(DrawTextInRect___BOOLEAN__BYREF_STRING__BYREF_I4__BYREF_I4__I4__I4__I4__I4__U4__MicrosoftSPOTPresentationMediaColor__MicrosoftSPOTFont);
    TINYCLR_NATIVE_DECLARE(SetClippingRectangle___VOID__I4__I4__I4__I4);
    TINYCLR_NATIVE_DECLARE(get_Width___I4);
    TINYCLR_NATIVE_DECLARE(get_Height___I4);
    TINYCLR_NATIVE_DECLARE(DrawEllipse___VOID__MicrosoftSPOTPresentationMediaColor__I4__I4__I4__I4__I4__MicrosoftSPOTPresentationMediaColor__I4__I4__MicrosoftSPOTPresentationMediaColor__I4__I4__U2);
    TINYCLR_NATIVE_DECLARE(DrawImage___VOID__I4__I4__MicrosoftSPOTBitmap__I4__I4__I4__I4__U2);
    TINYCLR_NATIVE_DECLARE(RotateImage___VOID__I4__I4__I4__MicrosoftSPOTBitmap__I4__I4__I4__I4__U2);
    TINYCLR_NATIVE_DECLARE(MakeTransparent___VOID__MicrosoftSPOTPresentationMediaColor);
    TINYCLR_NATIVE_DECLARE(StretchImage___VOID__I4__I4__MicrosoftSPOTBitmap__I4__I4__U2);
    TINYCLR_NATIVE_DECLARE(DrawLine___VOID__MicrosoftSPOTPresentationMediaColor__I4__I4__I4__I4__I4);
    TINYCLR_NATIVE_DECLARE(DrawRectangle___VOID__MicrosoftSPOTPresentationMediaColor__I4__I4__I4__I4__I4__I4__I4__MicrosoftSPOTPresentationMediaColor__I4__I4__MicrosoftSPOTPresentationMediaColor__I4__I4__U2);
    TINYCLR_NATIVE_DECLARE(DrawText___VOID__STRING__MicrosoftSPOTFont__MicrosoftSPOTPresentationMediaColor__I4__I4);
    TINYCLR_NATIVE_DECLARE(SetPixel___VOID__I4__I4__MicrosoftSPOTPresentationMediaColor);
    TINYCLR_NATIVE_DECLARE(GetPixel___MicrosoftSPOTPresentationMediaColor__I4__I4);
    TINYCLR_NATIVE_DECLARE(GetBitmap___SZARRAY_U1);
    TINYCLR_NATIVE_DECLARE(StretchImage___VOID__I4__I4__I4__I4__MicrosoftSPOTBitmap__I4__I4__I4__I4__U2);
    TINYCLR_NATIVE_DECLARE(TileImage___VOID__I4__I4__MicrosoftSPOTBitmap__I4__I4__U2);
    TINYCLR_NATIVE_DECLARE(Scale9Image___VOID__I4__I4__I4__I4__MicrosoftSPOTBitmap__I4__I4__I4__I4__U2);
    TINYCLR_NATIVE_DECLARE(Dispose___VOID__BOOLEAN);

    //--//

    static HRESULT GetBitmap( CLR_RT_HeapBlock*  pThis, bool fForWrite, CLR_GFX_Bitmap*& bitmap );
    static HRESULT GetBitmap( CLR_RT_StackFrame& stack, bool fForWrite, CLR_GFX_Bitmap*& bitmap );
            
    //
    // compile time check to ensure the CLR_GFX_Bitmap class knows about the interop definition
    // 
    struct CompileCheck
    {
        char buf1[ FIELD__m_bitmap ==  CLR_GFX_Bitmap::FIELD__m_bitmap ? 1 : -1 ];        
    };
};

extern const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_Graphics;

#endif  //  End #include wrap
