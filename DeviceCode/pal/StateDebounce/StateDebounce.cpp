////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"

/***************************************************************************/

void HAL_STATE_DEBOUNCE::Initialize( UINT32 debounce_uSec, HAL_CALLBACK_FPN isr )
{
    m_debounceTime_uSec = debounce_uSec;

    m_callback.InitializeForISR( isr );
}

void HAL_STATE_DEBOUNCE::Change( UINT32 state )
{
    ASSERT_IRQ_MUST_BE_OFF();

    m_callback.Abort();

    m_callback.SetArgument( (void*)(size_t)state );

    m_callback.EnqueueDelta( m_debounceTime_uSec );
}

void HAL_STATE_DEBOUNCE::Abort()
{
    ASSERT_IRQ_MUST_BE_OFF();

    m_callback.Abort();
}
