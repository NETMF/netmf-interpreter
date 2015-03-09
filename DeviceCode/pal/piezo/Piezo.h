////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _PIEZO_H_
#define _PIEZO_H_ 1

//--//

struct PWM_CONFIG
{
    GPIO_FLAG PWM_Output;
    UINT8     PWM_DisabledState;
    UINT8     VTU_Channel;
    UINT8     Prescalar;
};

struct PIEZO_CONFIG
{
    HAL_DRIVER_CONFIG_HEADER Header;

    //--//

    PWM_CONFIG PWM_Config[2];

    //--//

    static LPCSTR GetDriverName() { return "PIEZO_CONFIG"; }
};

extern PIEZO_CONFIG* const g_pPIEZO_Config;

//--//

struct PIEZO_TONE : public HAL_DblLinkedNode< PIEZO_TONE >
{

    UINT32 Frequency_Hertz;
    UINT32 Duration_Milliseconds;
};

//--//


struct PIEZO_POLY_TONE : public HAL_DblLinkedNode< PIEZO_POLY_TONE >
{

    UINT32        Duration_MicroSeconds;
    UINT32        ToneCount;
    UINT32        ToneRotator;
    const UINT32* Period;                 // array of values
    const UINT32* DutyCycle;              // array of values, paired with Period 
    UINT32        FixedDutyCycleCapture;  // global value used if DutyCycle is NULL
    UINT32        MinPeriodCapture;       // filter on shortest period duration allowed
};

//--//

struct Piezo_Driver
{
private:              
    HAL_COMPLETION                  m_ToneDone;
    HAL_CONTINUATION                m_ToneRelease;

    HAL_DblLinkedList< PIEZO_TONE > m_ToneToPlay;
    HAL_DblLinkedList< PIEZO_TONE > m_ToneToRelease;
    PIEZO_TONE*                     m_TonePlaying;

    //--//

    static void StartNext ();
    static void EmptyQueue();

    static void ToneDone_ISR( void* Param );
    static void ToneRelease ( void* Param );

public:
    static void Initialize();
    static void Uninitialize();

    static BOOL Tone( UINT32 Frequency_Hertz, UINT32 Duration_Milliseconds );
};

extern Piezo_Driver g_Piezo_Driver;

/***************************************************************************/

struct PolyphonicPiezo_Driver
{
private:
    HAL_COMPLETION                       m_ToneDone;
    HAL_CONTINUATION                     m_ToneRelease;

    HAL_DblLinkedList< PIEZO_POLY_TONE > m_ToneToPlay;
    HAL_DblLinkedList< PIEZO_POLY_TONE > m_ToneToRelease;
    PIEZO_POLY_TONE*                     m_TonePlaying;

    //--//

    static void StartNext ();
    static void EmptyQueue();

    static void ToneDone_ISR( void* Param );
    static void ToneRelease ( void* Param );

public:
    static void Initialize();
    static void Uninitialize();

    static BOOL Tone( const PIEZO_POLY_TONE& ToneRef );
};

extern PolyphonicPiezo_Driver g_PolyphonicPiezo_Driver;

/***************************************************************************/

#endif  // _PIEZO_H_

