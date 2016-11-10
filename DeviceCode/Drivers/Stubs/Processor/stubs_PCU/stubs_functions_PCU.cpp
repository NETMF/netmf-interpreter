////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//
__weak void CPU_PCU_Regulator_Switcher1_On ()    {}

__weak void CPU_PCU_Regulator_Switcher1_Off()    {}
__weak BOOL CPU_PCU_Regulator_Switcher1_Status() { return TRUE; }

__weak void CPU_PCU_Regulator_Linear1_On ()    {}
__weak void CPU_PCU_Regulator_Linear1_Off()    {}
__weak BOOL CPU_PCU_Regulator_Linear1_Status() { return FALSE; }

__weak void CPU_PCU_Regulator_Linear2_On ()    {}
__weak void CPU_PCU_Regulator_Linear2_Off()    {}
__weak BOOL CPU_PCU_Regulator_Linear2_Status() { return FALSE; }
