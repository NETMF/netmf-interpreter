////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"

static UINT32* g_FrameBuffer = NULL;
static UINT32 g_FrameBufferSize = 0;
static bool   g_LcdInitialized = false;

// required by core only if PLATFORM_WINDOWS is defined
// see: CLR_HW_Hardware::Hardware_Initialize() in Hardware.cpp
BOOL LCD_Initialize()
{
    return TRUE;
}

BOOL LCD_Uninitialize()
{
    return TRUE;
}

void LCD_Clear()
{
}

UINT32* LCD_GetFrameBuffer()
{
    return g_FrameBuffer;
}

void LCD_BitBlt( int width, int height, int widthInWords, UINT32 data[], BOOL fUseDelta )
{    
}

void LCD_BitBltEx( int x, int y, int width, int height, UINT32 data[] )
{
}

INT32 LCD_GetWidth()
{
    return 0;
}

INT32 LCD_GetHeight()
{
    return 0;
}

INT32 LCD_GetBitsPerPixel()
{
    return 0;   
}

INT32 LCD_GetOrientation()
{
    return 0;
}

UINT32 LCD_ConvertColor(UINT32 color)
{
    return color;
}


void lcd_printf(const char* format, ...)
{

}
