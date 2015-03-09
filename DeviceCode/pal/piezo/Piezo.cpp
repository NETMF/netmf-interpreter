////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "piezo.h"

//--//

#undef  TRACE_ALWAYS
#define TRACE_ALWAYS               0x00000001

#undef  DEBUG_TRACE
#define DEBUG_TRACE (0)
//#define DEBUG_TRACE (TRACE_ALWAYS)

/***************************************************************************/

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_Piezo_Driver"
#endif

Piezo_Driver g_Piezo_Driver;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

//--//

__inline void PWM_PiezoTone( UINT32 Frequency_Hertz, UINT32 DutyCycle, PWM_CONFIG* PWM_Config )
{
}

__inline void PWM_PolyphonicPiezoTone( PIEZO_POLY_TONE* Tone, PWM_CONFIG* PWM_Config )
{
}

//--//

void Piezo_Initialize()
{
    Piezo_Driver::Initialize();
}

void Piezo_Uninitialize()
{
    Piezo_Driver::Uninitialize();
}

BOOL Piezo_Tone( UINT32 Frequency_Hertz, UINT32 Duration_Milliseconds )
{
    return Piezo_Driver::Tone( Frequency_Hertz, Duration_Milliseconds );
}

BOOL Piezo_IsEnabled()
{
    return g_pPIEZO_Config->Header.Enable;
}

//--//

void Piezo_Driver::Initialize()
{
    HAL_CONFIG_BLOCK::ApplyConfig( g_pPIEZO_Config->GetDriverName(), g_pPIEZO_Config, sizeof(*g_pPIEZO_Config) );

    g_Piezo_Driver.m_ToneToPlay   .Initialize();
    g_Piezo_Driver.m_ToneToRelease.Initialize();

    g_Piezo_Driver.m_ToneDone   .InitializeForISR  ( ToneDone_ISR );
    g_Piezo_Driver.m_ToneRelease.InitializeCallback( ToneRelease  );

    g_Piezo_Driver.m_TonePlaying = NULL;

    // clear the hardware to proper disabled state
    for(int i = 0; i < 2; i++)
    {
        PWM_CONFIG* PWM_Config = &g_pPIEZO_Config->PWM_Config[i];

        if(PWM_Config->PWM_Output.Pin != GPIO_PIN_NONE)
        {
            CPU_GPIO_EnableOutputPin( PWM_Config->PWM_Output.Pin, PWM_Config->PWM_DisabledState );
        }
    }
}


void Piezo_Driver::Uninitialize()
{
    g_Piezo_Driver.m_ToneDone   .Abort();
    g_Piezo_Driver.m_ToneRelease.Abort();


    {
        GLOBAL_LOCK(irq);

        EmptyQueue();

        //
        // This purges the currently playing tone.
        //
        StartNext();
    }


    bool fEnabled = INTERRUPTS_ENABLED_STATE();

    if(!fEnabled) ENABLE_INTERRUPTS();

    ToneRelease( NULL );

    if(!fEnabled) DISABLE_INTERRUPTS();

    // restore the hardware to proper default state
    for(int i = 0; i < 2; i++)
    {
        PWM_CONFIG* PWM_Config = &g_pPIEZO_Config->PWM_Config[i];

        if(PWM_Config->PWM_Output.Pin != GPIO_PIN_NONE)
        {
            CPU_GPIO_EnableInputPin( PWM_Config->PWM_Output.Pin, FALSE, NULL, GPIO_INT_NONE, RESISTOR_PULLDOWN );
        }
    }
}


BOOL Piezo_Driver::Tone( UINT32 Frequency_Hertz, UINT32 Duration_Milliseconds )
{
    // Not sure why this is neccessary.  Calling the Piezo::Tone function from with-in an ISR
    // should be a valid scenario.  Lorenzo and I (Zach) can not determine
    // why this assert is needed; therefore, it is being commented out (for now).
    //ASSERT(!SystemState_Query( SYSTEM_STATE_ISR ));

    GLOBAL_LOCK(irq);

    // special to clear queue
    if(Frequency_Hertz == TONE_CLEAR_BUFFER)
    {
        // let active note to finish on it's own
        EmptyQueue();
    }
    else
    {
        // 0 length tone isn't wrong persay, but if a NULL op, so drop it gracefully, as a success
        if(Duration_Milliseconds > 0)
        {
            PIEZO_TONE* Tone = g_Piezo_Driver.m_ToneToRelease.ExtractFirstNode();

            if(Tone == NULL)
            {
                //
                // Re-enable interrupts when allocating memory.
                //
                irq.Release();

                Tone = (PIEZO_TONE*)private_malloc( sizeof(PIEZO_TONE) );

                Tone->Initialize();

                irq.Acquire();
            }

            if(Tone == NULL)
            {
                // No memory, just drop this tone and fail the call
                ASSERT(0);
                return FALSE;
            }

            Tone->Frequency_Hertz       = Frequency_Hertz;
            Tone->Duration_Milliseconds = Duration_Milliseconds;

            DEBUG_TRACE3(0, "Tone(%4d,%4d)=%d\r\n", Frequency_Hertz, Duration_Milliseconds, g_Piezo_Driver.m_ToneToPlay.NumOfNodes() );

            g_Piezo_Driver.m_ToneToPlay.LinkAtBack( Tone );

            if(g_Piezo_Driver.m_TonePlaying == NULL)
            {
                StartNext();
            }
        }
    }

    return TRUE;
}

void Piezo_Driver::StartNext()
{
    ASSERT_IRQ_MUST_BE_OFF();

    g_Piezo_Driver.m_ToneDone.Abort();

    PIEZO_TONE* Tone = g_Piezo_Driver.m_ToneToPlay.ExtractFirstNode();

    g_Piezo_Driver.m_TonePlaying = Tone;

    if(Tone == NULL)
    {
        PWM_PiezoTone( 0, 0, &g_pPIEZO_Config->PWM_Config[0] );
    }
    else
    {
        PWM_PiezoTone( Tone->Frequency_Hertz, 50, &g_pPIEZO_Config->PWM_Config[0] );

        g_Piezo_Driver.m_ToneDone.EnqueueDelta( Tone->Duration_Milliseconds * 1000 );
    }
}

void Piezo_Driver::EmptyQueue()
{
    while(true)
    {
        PIEZO_TONE* Tone = g_Piezo_Driver.m_ToneToPlay.ExtractFirstNode(); if(!Tone) break;

        g_Piezo_Driver.m_ToneToRelease.LinkAtBack( Tone );
    }

    if(g_Piezo_Driver.m_ToneRelease.IsLinked() == false)
    {
        g_Piezo_Driver.m_ToneRelease.Enqueue();
    }
}

void Piezo_Driver::ToneDone_ISR( void* Param )
{
    ASSERT_IRQ_MUST_BE_OFF();

    PIEZO_TONE* Tone = g_Piezo_Driver.m_TonePlaying;
    if(Tone)
    {
        g_Piezo_Driver.m_ToneToRelease.LinkAtBack( Tone );

        if(g_Piezo_Driver.m_ToneRelease.IsLinked() == false)
        {
            g_Piezo_Driver.m_ToneRelease.Enqueue();
        }
    }

    StartNext();
}

void Piezo_Driver::ToneRelease( void* Param )
{
    ASSERT_IRQ_MUST_BE_ON();

    GLOBAL_LOCK(irq);

    while(true)
    {
        PIEZO_TONE* Tone = g_Piezo_Driver.m_ToneToRelease.ExtractFirstNode(); if(!Tone) break;

        //
        // Re-enable interrupts when releasing memory.
        //
        irq.Release();

        private_free( Tone );

        irq.Acquire();
    }

    g_Piezo_Driver.m_ToneRelease.Abort();
}

/***************************************************************************/

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_PolyphonicPiezo_Driver"
#endif

PolyphonicPiezo_Driver g_PolyphonicPiezo_Driver;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

//--//

void PolyphonicPiezo_Driver::Initialize()
{
    HAL_CONFIG_BLOCK::ApplyConfig( g_pPIEZO_Config->GetDriverName(), g_pPIEZO_Config, sizeof(*g_pPIEZO_Config) );

    g_PolyphonicPiezo_Driver.m_ToneToPlay   .Initialize();
    g_PolyphonicPiezo_Driver.m_ToneToRelease.Initialize();

    g_PolyphonicPiezo_Driver.m_ToneDone   .InitializeForISR  ( ToneDone_ISR );
    g_PolyphonicPiezo_Driver.m_ToneRelease.InitializeCallback( ToneRelease  );

    g_PolyphonicPiezo_Driver.m_TonePlaying = NULL;
}


void PolyphonicPiezo_Driver::Uninitialize()
{
    {
        GLOBAL_LOCK(irq);

        EmptyQueue();

        //
        // This purges the currently playing tone.
        //
        StartNext();
    }

    bool fEnabled = INTERRUPTS_ENABLED_STATE();

    if(!fEnabled) ENABLE_INTERRUPTS();

    ToneRelease( NULL );

    if(!fEnabled) DISABLE_INTERRUPTS();
}

BOOL PolyphonicPiezo_Driver::Tone( const PIEZO_POLY_TONE& ToneRef )
{
    ASSERT(!SystemState_Query(SYSTEM_STATE_ISR));

    GLOBAL_LOCK(irq);

    // special to clear queue
    if(ToneRef.Period[0] == TONE_CLEAR_BUFFER)
    {
        // let active note to finish on it's own
        EmptyQueue();
    }
    else
    {
        // 0 length tone isn't wrong persay, but if a NULL op, so drop it gracefully, as a success
        if(ToneRef.Duration_MicroSeconds > 0)
        {
            PIEZO_POLY_TONE* Tone = g_PolyphonicPiezo_Driver.m_ToneToRelease.ExtractFirstNode();

            if(Tone == NULL)
            {
                //
                // Re-enable interrupts when allocating memory.
                //
                irq.Release();

                Tone = (PIEZO_POLY_TONE*)private_malloc( sizeof(PIEZO_POLY_TONE) );

                irq.Acquire();
            }

            if(Tone == NULL)
            {
                // No memory, just drop this tone and fail the call
                ASSERT(0);
                return FALSE;
            }

            *Tone = ToneRef;

            Tone->Initialize();

            DEBUG_TRACE2( 0, "Tone(%4d)=%d\r\n", ToneRef.Duration_MicroSeconds, g_PolyphonicPiezo_Driver.m_ToneToPlay.NumOfNodes() );

            g_PolyphonicPiezo_Driver.m_ToneToPlay.LinkAtBack( Tone );

            if(g_PolyphonicPiezo_Driver.m_TonePlaying == NULL)
            {
                StartNext();
            }
        }
    }

    return TRUE;
}

void PolyphonicPiezo_Driver::StartNext()
{
    ASSERT_IRQ_MUST_BE_OFF();

    g_PolyphonicPiezo_Driver.m_ToneDone.Abort();

    PIEZO_POLY_TONE* Tone = g_PolyphonicPiezo_Driver.m_ToneToPlay.ExtractFirstNode();

    g_PolyphonicPiezo_Driver.m_TonePlaying = Tone;

    PWM_PolyphonicPiezoTone( Tone, &g_pPIEZO_Config->PWM_Config[0] );

    if(Tone)
    {
        g_PolyphonicPiezo_Driver.m_ToneDone.EnqueueDelta( Tone->Duration_MicroSeconds );
    }
}

void PolyphonicPiezo_Driver::EmptyQueue()
{
    while(true)
    {
        PIEZO_POLY_TONE* Tone = g_PolyphonicPiezo_Driver.m_ToneToPlay.ExtractFirstNode(); if(!Tone) break;

        g_PolyphonicPiezo_Driver.m_ToneToRelease.LinkAtBack( Tone );
    }

    if(g_PolyphonicPiezo_Driver.m_ToneRelease.IsLinked() == false)
    {
        g_PolyphonicPiezo_Driver.m_ToneRelease.Enqueue();
    }
}

void PolyphonicPiezo_Driver::ToneDone_ISR( void* Param )
{
    ASSERT_IRQ_MUST_BE_OFF();

    Events_Set( SYSTEM_EVENT_FLAG_TONE_COMPLETE );

    PIEZO_POLY_TONE* Tone = g_PolyphonicPiezo_Driver.m_TonePlaying;
    if(Tone)
    {
        g_PolyphonicPiezo_Driver.m_ToneToRelease.LinkAtBack( Tone );

        if(g_PolyphonicPiezo_Driver.m_ToneRelease.IsLinked() == false)
        {
            g_PolyphonicPiezo_Driver.m_ToneRelease.Enqueue();
        }
    }
    else
    {
        Events_Set( SYSTEM_EVENT_FLAG_TONE_BUFFER_EMPTY );
    }

    StartNext();
}

void PolyphonicPiezo_Driver::ToneRelease( void* Param )
{
    ASSERT_IRQ_MUST_BE_ON();

    GLOBAL_LOCK(irq);

    while(true)
    {
        PIEZO_POLY_TONE* Tone = g_PolyphonicPiezo_Driver.m_ToneToRelease.ExtractFirstNode(); if(!Tone) break;

        //
        // Re-enable interrupts when releasing memory.
        //
        irq.Release();

        private_free( Tone );

        irq.Acquire();
    }

    g_PolyphonicPiezo_Driver.m_ToneRelease.Abort();
}
