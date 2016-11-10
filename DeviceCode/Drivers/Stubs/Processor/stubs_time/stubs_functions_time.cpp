////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//

__weak BOOL HAL_Time_Initialize()
{
    return FALSE;
}

__weak BOOL HAL_Time_Uninitialize()
{
    return FALSE;
}

__weak UINT64 HAL_Time_CurrentTicks()
{
    return 0;
}

__weak INT64 HAL_Time_TicksToTime( UINT64 Ticks )
{
    return 0;
}

__weak INT64 HAL_Time_CurrentTime()
{
    return 0;
}

__weak void HAL_Time_SetCompare( UINT64 CompareValue )
{
}

__weak void HAL_Time_Sleep_MicroSeconds( UINT32 uSec )
{
    
}

__weak void HAL_Time_Sleep_MicroSeconds_InterruptEnabled( UINT32 uSec )
{

}

__weak void HAL_Time_GetDriftParameters  ( INT32* a, INT32* b, INT64* c )
{
    *a = 1;
    *b = 1;
    *c = 0;
}

__weak UINT32 CPU_SystemClock()
{
    return 0;
}


__weak UINT32 CPU_TicksPerSecond()
{
    return 0;
}

__weak UINT64 CPU_MillisecondsToTicks( UINT64 Ticks )
{
    return 0;
}

__weak UINT64 CPU_MillisecondsToTicks( UINT32 Ticks32 )
{
    return 0;
}

__weak UINT64 CPU_MicrosecondsToTicks( UINT64 uSec )
{
    return 0;
}

__weak UINT32 CPU_MicrosecondsToTicks( UINT32 uSec )
{
    return 0;
}

__weak UINT32 CPU_MicrosecondsToSystemClocks( UINT32 uSec )
{
    return 0;
}

__weak UINT64 CPU_TicksToTime( UINT64 Ticks )
{
    return 0;
}

__weak UINT64 CPU_TicksToTime( UINT32 Ticks32 )
{
    return 0;
}
