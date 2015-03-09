////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Hardware.h"

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Battery__ChargerModel::_ctor___VOID( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*      pThis = stack.This();
    BATTERY_COMMON_CONFIG* cfg   = Battery_Configuration();

#define SETFIELD(field,lvalue,rvalue) pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Battery__ChargerModel::field ].NumericByRef().lvalue = rvalue

    //--//

    SETFIELD(FIELD__Charge_Min       , u4, cfg->Battery_Life_Min       );
    SETFIELD(FIELD__Charge_Low       , u4, cfg->Battery_Life_Low       );
    SETFIELD(FIELD__Charge_Medium    , u4, cfg->Battery_Life_Med       );
    SETFIELD(FIELD__Charge_Full      , u4, cfg->Battery_Life_Max       );
    SETFIELD(FIELD__Charge_FullMin   , u4, cfg->Battery_Life_FullMin   );
    SETFIELD(FIELD__Charge_Hysteresis, u4, cfg->Battery_Life_Hysteresis);

    SETFIELD(FIELD__Timeout_Charging , s8, cfg->Battery_Timeout_Charging  * ((CLR_INT64)TIME_CONVERSION__ONEMINUTE * (CLR_INT64)TIME_CONVERSION__TO_SECONDS));
    SETFIELD(FIELD__Timeout_Charged  , s8, cfg->Battery_Timeout_Charged   * ((CLR_INT64)TIME_CONVERSION__ONEMINUTE * (CLR_INT64)TIME_CONVERSION__TO_SECONDS));
    SETFIELD(FIELD__Timeout_Charger  , s8, cfg->Battery_Timeout_Charger   * ((CLR_INT64)TIME_CONVERSION__ONESECOND * (CLR_INT64)TIME_CONVERSION__TO_SECONDS));
    SETFIELD(FIELD__Timeout_Backlight, s8, cfg->Battery_Timeout_Backlight * ((CLR_INT64)TIME_CONVERSION__ONESECOND * (CLR_INT64)TIME_CONVERSION__TO_SECONDS));

    TINYCLR_NOCLEANUP_NOLABEL();
}
