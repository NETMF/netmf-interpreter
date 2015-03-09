////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Charger_Dualstatus.h"

//--//

BOOL Charger_Initialize()
{
    return Charger_DualStatus::Initialize();
}

BOOL Charger_Uninitialize()
{
    return Charger_DualStatus::Uninitialize();
}

BOOL Charger_Status( UINT32& Status )
{
    return Charger_DualStatus::Status( Status );
}

BOOL Charger_Shutdown( UINT32 FlagMask )
{
    return Charger_DualStatus::Shutdown( FlagMask );
}

BOOL Charger_Restart( UINT32 FlagMask )
{
    return Charger_DualStatus::Restart( FlagMask );
}

void Charger_SetTemperature( INT16 DegreesCelcius_x10 )
{
    Charger_DualStatus::SetTemperature( DegreesCelcius_x10 );
}

