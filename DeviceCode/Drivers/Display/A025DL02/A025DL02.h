////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"

//--//

#ifndef _DRIVERS_DISPLAY_A025DL02_H_
#define _DRIVERS_DISPLAY_A025DL02_H_ 1

//////////////////////////////////////////////////////////////////////////////

extern SPI_CONFIGURATION g_A025DL02_SPI_Config;

//////////////////////////////////////////////////////////////////////////////

struct A025DL02_Driver
{
    //--//

    static const UINT32 c_CMD_REG0                   = 0;
    static const UINT32 c_CMD_REG0_VCOM_AC__mask     = 0x03;
    static const UINT32 c_CMD_REG0_VCOM_AC__shift    = 0;
    
    static const UINT32 c_CMD_REG1                   = 1;
    static const UINT32 c_CMD_REG0_VCOM_DC__mask     = 0x3f;
    static const UINT32 c_CMD_REG0_VCOM_DC__shift    = 0;
    static const UINT32 c_CMD_REG0_FLK_normal        = 0;     // normal mode
    static const UINT32 c_CMD_REG0_FLK_flicker       = 1;     // flicker pattern output
    static const UINT32 c_CMD_REG0_FLK__mask         = 0x40; 
    static const UINT32 c_CMD_REG0_FLK__shift        = 6; 
    
    static const UINT32 c_CMD_REG3                   = 3;
    static const UINT32 c_CMD_REG3_BRIGHTNESS__mask  = 0xFF;
    static const UINT32 c_CMD_REG3_BRIGHTNESS__shift = 0;

    static const UINT32 c_CMD_REG4                   = 4;
    static const UINT32 c_CMD_REG4_HDIR__mask        = 0x01;   // horizontal scan direction
    static const UINT32 c_CMD_REG4_HDIR__shift       = 0;   
    static const UINT32 c_CMD_REG4_HDIR_r2l          = 0x00;   // right to left
    static const UINT32 c_CMD_REG4_HDIR_l2r          = 0x01;   // left to right

    static const UINT32 c_CMD_REG4_VDIR__mask        = 0x02;   // vertical scan direction
    static const UINT32 c_CMD_REG4_VDIR__shift       = 1;  
    static const UINT32 c_CMD_REG4_VDIR__b2t         = 0x00;   // bottom to top
    static const UINT32 c_CMD_REG4_VDIR__t2b         = 0x01;   // top to bottom

    static const UINT32 c_CMD_REG4_NTSC_PAL__mask    = 0x0C;  
    static const UINT32 c_CMD_REG4_NTSC_PAL__shift   = 2;  
    static const UINT32 c_CMD_REG4_NTSC_PAL_pal      = 0x00;  // PAL
    static const UINT32 c_CMD_REG4_NTSC_PAL_ntsc     = 0x01;  // NTSC
    static const UINT32 c_CMD_REG4_NTSC_PAL_auto     = 0x02;  // AUTO detect

    static const UINT32 c_CMD_REG4_SEL__mask         = 0x30;  // input data timing format selection
    static const UINT32 c_CMD_REG4_SEL__shift        = 4;     // input data timing format selection

    static const UINT32 c_REG4_SETUP_DATA            =  (1 <<c_CMD_REG4_SEL__shift)                              | 
                                                        (c_CMD_REG4_NTSC_PAL_auto << c_CMD_REG4_NTSC_PAL__shift);
    
    static const UINT32 c_CMD_REG5                   = 5;
    static const UINT32 c_CMD_REG5_STB__mask         = 0x01;  // standby
    static const UINT32 c_CMD_REG5_STB__shift        = 0;
    static const UINT32 c_CMD_REG5_STB_standby       = 0x00; 
    static const UINT32 c_CMD_REG5_STB_normal        = 0x01; 

    static const UINT32 c_CMD_REG5_SHDB1__mask       = 0x02;  // shutdown for A025DL02 power converter
    static const UINT32 c_CMD_REG5_SHDB1__shift      = 1;
    static const UINT32 c_CMD_REG5_SHDB1_off         = 0x00;  // turn black power converter off
    static const UINT32 c_CMD_REG5_SHDB1_auto        = 0x01;  // controlled by on/off

    static const UINT32 c_CMD_REG5_SHDB2__mask       = 0x04;  // shutdown for VGH/VGL charge pump
    static const UINT32 c_CMD_REG5_SHDB2__shift      = 2;
    static const UINT32 c_CMD_REG5_SHDB2_off         = 0x00;  // turn charge pump off
    static const UINT32 c_CMD_REG5_SHDB2_auto        = 0x01;  // controlled by on/off

    static const UINT32 c_CMD_REG5_PWM_DUTY__mask    = 0x18;  // PWM duty cycle for backlight
    static const UINT32 c_CMD_REG5_PWM_DUTY__shift   = 3;
    static const UINT32 c_CMD_REG5_PWM_DUTY_50       = 0x00;  // 50%
    static const UINT32 c_CMD_REG5_PWM_DUTY_60       = 0x01;  // 60%
    static const UINT32 c_CMD_REG5_PWM_DUTY_65       = 0x02;  // 65% (default)
    static const UINT32 c_CMD_REG5_PWM_DUTY_70       = 0x03;  // 70%

    static const UINT32 c_CMD_REG5_GRB__mask         = 0x40;  // Register reset 
    static const UINT32 c_CMD_REG5_GRB__shift        = 6;
    static const UINT32 c_CMD_REG5_GRB_reset         = 0x00;  // reset all registers to default
    static const UINT32 c_CMD_REG5_GRB_normal        = 0x01;  // normal operation (default) 
    static const UINT32 c_CMD_REG5_DRV_FREQ__mask    = 0x80;
    static const UINT32 c_CMD_REG5_DRV_FREQ__shift   = 7;
    static const UINT32 c_CMD_REG5_DRV_FREQ_div64    = 0x00; // default
    static const UINT32 c_CMD_REG5_DRV_FREQ_div32    = 0x01;

    //0x96
    static const UINT32 c_LCD_START                  = ( c_CMD_REG5_DRV_FREQ_div32 << c_CMD_REG5_DRV_FREQ__shift ) | 
                                                       ( c_CMD_REG5_SHDB2_auto     << c_CMD_REG5_SHDB2__shift    ) |
                                                       ( c_CMD_REG5_SHDB1_auto     << c_CMD_REG5_SHDB1__shift    );

    //0xD7
    static const UINT32 c_LCD_NORMAL                 = ( c_CMD_REG5_DRV_FREQ_div32 << c_CMD_REG5_DRV_FREQ__shift ) |
                                                       ( c_CMD_REG5_GRB_normal     << c_CMD_REG5_GRB__shift      ) |
                                                       ( c_CMD_REG5_PWM_DUTY_70    << c_CMD_REG5_PWM_DUTY__shift ) |
                                                       ( c_CMD_REG5_SHDB2_auto     << c_CMD_REG5_SHDB2__shift    ) |
                                                       ( c_CMD_REG5_SHDB1_auto     << c_CMD_REG5_SHDB1__shift    ) |
                                                       ( c_CMD_REG5_STB_normal     << c_CMD_REG5_STB__shift      );

    //0xD6
    static const UINT32 c_LCD_STANDBY                = ( c_CMD_REG5_DRV_FREQ_div32 << c_CMD_REG5_DRV_FREQ__shift ) |
                                                       ( c_CMD_REG5_GRB_normal     << c_CMD_REG5_GRB__shift      ) |
                                                       ( c_CMD_REG5_PWM_DUTY_70    << c_CMD_REG5_PWM_DUTY__shift ) |
                                                       ( c_CMD_REG5_SHDB2_auto     << c_CMD_REG5_SHDB2__shift    ) |
                                                       ( c_CMD_REG5_SHDB1_auto     << c_CMD_REG5_SHDB1__shift    );

    //0xD4
    static const UINT32 c_LCD_POWERSAVE              = ( c_CMD_REG5_DRV_FREQ_div32 << c_CMD_REG5_DRV_FREQ__shift ) |
                                                       ( c_CMD_REG5_GRB_normal     << c_CMD_REG5_GRB__shift      ) |
                                                       ( c_CMD_REG5_PWM_DUTY_70    << c_CMD_REG5_PWM_DUTY__shift ) |
                                                       ( c_CMD_REG5_SHDB2_auto     << c_CMD_REG5_SHDB2__shift    );


    static const UINT32 c_LCD_BACKLIGHT_OFF          = ( c_CMD_REG5_DRV_FREQ_div32 << c_CMD_REG5_DRV_FREQ__shift ) |
                                                       ( c_CMD_REG5_GRB_normal     << c_CMD_REG5_GRB__shift      ) |
                                                       ( c_CMD_REG5_PWM_DUTY_70    << c_CMD_REG5_PWM_DUTY__shift ) |
                                                       ( c_CMD_REG5_SHDB2_auto     << c_CMD_REG5_SHDB2__shift    ) |
                                                       ( c_CMD_REG5_STB_normal     << c_CMD_REG5_STB__shift      );

    
    static const UINT32 c_CMD_REG6                   = 6;
    static const UINT32 c_CMD_REG6_VBLK__mask        = 0x1F;  // vertical blanking
    static const UINT32 c_CMD_REG6_VBLK__shift       = 0;
    static const UINT32 c_CMD_REG6_LED_CURRENT__mask = 0x60;  // adjust LED current
    static const UINT32 c_CMD_REG6_LED_CURRENT__shift= 0;

    static const UINT32 c_CMD_REG8                   = 8;
    static const UINT32 c_CMD_REG8_BL_DRV__mask      = 0xC0;  // backlight driving capability
    static const UINT32 c_CMD_REG8_BL_DRV__shift     = 6;
    static const UINT32 c_BL_DRV_DATA                = (0x3 << c_CMD_REG8_BL_DRV__shift) ;

    static const UINT32 c_CMD_REG12                  = 12;
    static const UINT32 c_CMD_REG12_DCLKpol__mask    = 0x01;  // DCLK polarity
    static const UINT32 c_CMD_REG12_DCLKpol__shift   = 0;
    static const UINT32 c_CMD_REG12_DCLKpol_pos      = 0x00;  // positve polarity (default)
    static const UINT32 c_CMD_REG12_DCLKpol_neg      = 0x01;  // negative polarity
    static const UINT32 c_CMD_REG12_HDpol__mask      = 0x02;  // HSYNC polarity
    static const UINT32 c_CMD_REG12_HDpol__shift     = 1;
    static const UINT32 c_CMD_REG12_HDpol_pos        = 0x00;  // positve polarity
    static const UINT32 c_CMD_REG12_HDpol_neg        = 0x01;  // negative polarity (default)
    static const UINT32 c_CMD_REG12_VDpol__mask      = 0x04;  // VSYNC polarity
    static const UINT32 c_CMD_REG12_VDpol__shift     = 2;
    static const UINT32 c_CMD_REG12_VDpol_pos        = 0x00;  // positve polarity
    static const UINT32 c_CMD_REG12_VDpol_neg        = 0x01;  // negative polarity (default)
    static const UINT32 c_CMD_REG12_CSYNC__mask      = 0x20;  // separate SYNC or CSYNC input
    static const UINT32 c_CMD_REG12_CSYNC__shift     = 5;
    static const UINT32 c_CMD_REG12_CSYNC_csync      = 21;  // CSYNC input
    static const UINT32 c_CMD_REG12_CSYNC_sync       = 21;  // separate SYNC input
    static const UINT32 c_CMD_REG12_PAIR__mask       = 0xC0;  // vertical start time of odd/even frame
    static const UINT32 c_CMD_REG12_PAIR__shift      = 6;  // vertical start time of odd/even frame

    static const UINT32 c_CMD_REG13                  = 13;
    static const UINT32 c_CMD_REG13_CONTRAST__mask   = 0xFF;  // RGB contrast level
    static const UINT32 c_CMD_REG13_CONTRAST__shift  = 0;
    
    static const UINT32 c_CMD_REG14                        = 14;
    static const UINT32 c_CMD_REG14_SUBCONTRAST_R__mask    = 0x7F;  // R sub-contrast level
    static const UINT32 c_CMD_REG14_SUBCONTRAST_R__shift   = 0;

    static const UINT32 c_CMD_REG15                        = 15;
    static const UINT32 c_CMD_REG15_SUBBRIGHTNESS_R__mask  = 0x7F;  // R sub-brightness level
    static const UINT32 c_CMD_REG15_SUBBRIGHTNESS_R__shift = 0;

    static const UINT32 c_CMD_REG16                        = 16;
    static const UINT32 c_CMD_REG16_SUBCONTRAST_B__mask    = 0x7F;  // B sub-contrast level
    static const UINT32 c_CMD_REG16_SUBCONTRAST_B__shift   = 0;

    static const UINT32 c_CMD_REG17                        = 17;
    static const UINT32 c_CMD_REG17_SUBBRIGHTNESS_B__mask  = 0x7F;  // B sub-brightness level
    static const UINT32 c_CMD_REG17_SUBBRIGHTNESS_B__shift = 0;

    static const UINT32 c_CMD_REG18                        = 18;
    static const UINT32 c_CMD_REG18_GAMMA_VR1__mask        = 0xF0;
    static const UINT32 c_CMD_REG18_GAMMA_VR1__shift       = 0;
    static const UINT32 c_CMD_REG18_GAMMA_VR2__mask        = 0xF0;
    static const UINT32 c_CMD_REG18_GAMMA_VR2__shift       = 4;

    static const UINT32 c_CMD_REG19                        = 19;
    static const UINT32 c_CMD_REG19_GAMMA_VR3__mask        = 0xF0;
    static const UINT32 c_CMD_REG19_GAMMA_VR3__shift       = 0;
    static const UINT32 c_CMD_REG19_GAMMA_VR4__mask        = 0xF0;
    static const UINT32 c_CMD_REG19_GAMMA_VR4__shift       = 4;

    //--//

    UINT32 m_cursor;

    //--//
    
    static BOOL Initialize();

    static BOOL Uninitialize();

    static void PowerSave( BOOL On );

    static void Clear();

    static void BitBltEx( int x, int y, int width, int height, UINT32 data[] );

    static void BitBlt( int width, int height, int widthInWords, UINT32 data[], BOOL fUseDelta );

    static void WriteChar         ( unsigned char c, int row, int col );
    
    static void WriteFormattedChar( unsigned char c                   );

private:
    static UINT32 PixelsPerWord();
    static UINT32 TextRows();
    static UINT32 TextColumns();
    static UINT32 WidthInWords();
    static UINT32 SizeInWords();
    static UINT32 SizeInBytes();
};

extern A025DL02_Driver g_A025DL02_Driver;


#endif  // _DRIVERS_DISPLAY_A025DL02_H_
