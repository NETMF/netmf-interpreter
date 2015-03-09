////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_LCD_DECL_H_
#define _DRIVERS_LCD_DECL_H_ 1

//--//

#define LCD_SCREEN_WIDTH    LCD_GetWidth()
#define LCD_SCREEN_HEIGHT   LCD_GetHeight()
#define LCD_SCREEN_BPP      LCD_GetBitsPerPixel()
#define LCD_SCREEN_ORIENTATION LCD_GetOrientation()

#define LCD_SCREEN_PIXELS_PER_WORD      (32/LCD_SCREEN_BPP)
#define LCD_SCREEN_WIDTH_IN_WORDS       ((LCD_SCREEN_WIDTH + LCD_SCREEN_PIXELS_PER_WORD - 1) / LCD_SCREEN_PIXELS_PER_WORD)
#define LCD_SCREEN_SIZE_IN_WORDS        (LCD_SCREEN_WIDTH_IN_WORDS * LCD_SCREEN_HEIGHT)
#define LCD_SCREEN_SIZE_IN_BYTES        (LCD_SCREEN_SIZE_IN_WORDS * 4)

#define LCD_NO_PIXEL_CLOCK_DIVIDER      0

struct DISPLAY_CONTROLLER_REGISTER_EXTRA
{
    UINT32 RegAddr;
    UINT32 RegValue;
};

struct DISPLAY_CONTROLLER_CONFIG
{
    UINT32 Width;
    UINT32 Height;
    BOOL EnableTFT;
    BOOL EnableColor;
    BOOL PixelPolarity;
    BOOL FirstLineMarkerPolarity;
    BOOL LinePulsePolarity;
    BOOL ShiftClockPolarity;
    BOOL OutputEnablePolarity;
    BOOL ClockIdleEnable;
    BOOL ClockSelectEnable;

    UINT8 PixelClockDivider;
    UINT8 BusWidth;
    UINT8 BitsPerPixel;
    UINT8 Orientation;

    UINT8 HorizontalSyncPulseWidth;
    UINT8 HorizontalSyncWait1;
    UINT8 HorizontalSyncWait2;
    UINT8 VerticalSyncPulseWidth;
    UINT8 VerticalSyncWait1;
    UINT8 VerticalSyncWait2;
    DISPLAY_CONTROLLER_REGISTER_EXTRA RegisterExtras[8];
};

extern DISPLAY_CONTROLLER_CONFIG g_LcdController_Config;

//--//  Display Driver  //--//

BOOL LCD_Initialize            (                                                                        );
BOOL LCD_Uninitialize          (                                                                        );
void LCD_Clear                 (                                                                        );
void LCD_BitBltEx              ( int x, int y, int width, int height, UINT32 data[]                     );
void LCD_BitBlt                ( int width, int height, int widthInWords, UINT32 data[], BOOL fUseDelta );
void LCD_WriteChar             ( unsigned char c, int row, int col                                      );
void LCD_WriteFormattedChar    ( unsigned char c                                                        );
INT32 LCD_GetWidth       ();
INT32 LCD_GetHeight      ();
INT32 LCD_GetBitsPerPixel();
INT32 LCD_GetOrientation ();
void LCD_PowerSave             ( BOOL On                                                                );
UINT32 LCD_ConvertColor(UINT32 color);


//--//  Display Controller  //--//

BOOL LCD_Controller_Initialize            ( DISPLAY_CONTROLLER_CONFIG& config                                  );
BOOL LCD_Controller_Uninitialize          (                                                                        );
BOOL LCD_Controller_Enable            ( BOOL fEnable                                  );
UINT32* LCD_GetFrameBuffer     (                                                                        );
    
//--//  Text Font (for WriteChar*) //--//

INT32 Font_Height();
INT32 Font_Width();
INT32 Font_TabWidth();
const UINT8* Font_GetGlyph( unsigned char c );

#endif // _DRIVERS_LCD_DECL_H_

