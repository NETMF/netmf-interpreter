////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Graphics.h"


static const CLR_RT_MethodHandler method_lookup[] =
{
    NULL,
    NULL,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::_ctor___VOID__I4__I4,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::_ctor___VOID__SZARRAY_U1__MicrosoftSPOTBitmapBitmapImageType,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::Flush___VOID,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::Flush___VOID__I4__I4__I4__I4,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::Clear___VOID,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::DrawTextInRect___BOOLEAN__BYREF_STRING__BYREF_I4__BYREF_I4__I4__I4__I4__I4__U4__MicrosoftSPOTPresentationMediaColor__MicrosoftSPOTFont,
    NULL,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::SetClippingRectangle___VOID__I4__I4__I4__I4,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::get_Width___I4,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::get_Height___I4,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::DrawEllipse___VOID__MicrosoftSPOTPresentationMediaColor__I4__I4__I4__I4__I4__MicrosoftSPOTPresentationMediaColor__I4__I4__MicrosoftSPOTPresentationMediaColor__I4__I4__U2,
    NULL,
    NULL,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::DrawImage___VOID__I4__I4__MicrosoftSPOTBitmap__I4__I4__I4__I4__U2,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::RotateImage___VOID__I4__I4__I4__MicrosoftSPOTBitmap__I4__I4__I4__I4__U2,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::MakeTransparent___VOID__MicrosoftSPOTPresentationMediaColor,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::StretchImage___VOID__I4__I4__MicrosoftSPOTBitmap__I4__I4__U2,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::DrawLine___VOID__MicrosoftSPOTPresentationMediaColor__I4__I4__I4__I4__I4,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::DrawRectangle___VOID__MicrosoftSPOTPresentationMediaColor__I4__I4__I4__I4__I4__I4__I4__MicrosoftSPOTPresentationMediaColor__I4__I4__MicrosoftSPOTPresentationMediaColor__I4__I4__U2,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::DrawText___VOID__STRING__MicrosoftSPOTFont__MicrosoftSPOTPresentationMediaColor__I4__I4,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::SetPixel___VOID__I4__I4__MicrosoftSPOTPresentationMediaColor,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::GetPixel___MicrosoftSPOTPresentationMediaColor__I4__I4,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::GetBitmap___SZARRAY_U1,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::StretchImage___VOID__I4__I4__I4__I4__MicrosoftSPOTBitmap__I4__I4__I4__I4__U2,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::TileImage___VOID__I4__I4__MicrosoftSPOTBitmap__I4__I4__U2,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::Scale9Image___VOID__I4__I4__I4__I4__MicrosoftSPOTBitmap__I4__I4__I4__I4__U2,
    Library_spot_graphics_native_Microsoft_SPOT_Bitmap::Dispose___VOID__BOOLEAN,
    NULL,
    NULL,
    Library_spot_graphics_native_Microsoft_SPOT_Font::CharWidth___I4__CHAR,
    Library_spot_graphics_native_Microsoft_SPOT_Font::get_Height___I4,
    Library_spot_graphics_native_Microsoft_SPOT_Font::get_AverageWidth___I4,
    Library_spot_graphics_native_Microsoft_SPOT_Font::get_MaxWidth___I4,
    Library_spot_graphics_native_Microsoft_SPOT_Font::get_Ascent___I4,
    Library_spot_graphics_native_Microsoft_SPOT_Font::get_Descent___I4,
    Library_spot_graphics_native_Microsoft_SPOT_Font::get_InternalLeading___I4,
    Library_spot_graphics_native_Microsoft_SPOT_Font::get_ExternalLeading___I4,
    NULL,
    Library_spot_graphics_native_Microsoft_SPOT_Font::ComputeExtent___VOID__STRING__BYREF_I4__BYREF_I4__I4,
    NULL,
    NULL,
    Library_spot_graphics_native_Microsoft_SPOT_Font::ComputeTextInRect___VOID__STRING__BYREF_I4__BYREF_I4__I4__I4__I4__I4__U4,
    NULL,
    NULL,
    NULL,
    NULL,
};

const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_Graphics =
{
    "Microsoft.SPOT.Graphics", 
    0x46930647,
    method_lookup
};

