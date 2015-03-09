////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_BATTERYMEASUREMENT_AD7466_FUNCTIONS_H_
#define _DRIVERS_BATTERYMEASUREMENT_AD7466_FUNCTIONS_H_ 1

//--//
#include "AD7466.h"

 BOOL Battery_Initialize()
{
    return AD7466_Driver::Initialize();
}

 BOOL Battery_Uninitialize()
{
    return AD7466_Driver::Uninitialize();
}

 BATTERY_COMMON_CONFIG* Battery_Configuration()
{
    return &g_AD7466_Config.CommonConfig;
}

 BOOL Battery_Voltage( INT32& Millivolts )
{
    return AD7466_Driver::Voltage( Millivolts );
}

 BOOL Battery_Temperature( INT32& DegreesCelcius_x10 )
{
    return AD7466_Driver::Temperature( DegreesCelcius_x10 );
}

 void Battery_VoltageFilter_Reset()
{
    AD7466_Driver::VoltageFilter_Reset();
}

 INT32 Battery_MostRecent_DegreesCelcius_x10()
{
    return g_AD7466_Driver.m_MostRecent_DegreesCelcius_x10;
}

 INT32 Battery_MostRecent_Millivolts()
{
    return g_AD7466_Driver.m_MostRecent_Millivolts;
}

//--//

#endif // _DRIVERS_BATTERYMEASUREMENT_AD7466_FUNCTIONS_H_
