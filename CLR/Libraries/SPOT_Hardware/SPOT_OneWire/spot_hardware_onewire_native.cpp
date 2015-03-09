////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "spot_hardware_onewire_native.h"


static const CLR_RT_MethodHandler method_lookup[] =
{
    Library_spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire::TouchReset___I4,
    Library_spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire::TouchBit___I4__I4,
    Library_spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire::TouchByte___I4__I4,
    Library_spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire::WriteByte___I4__I4,
    Library_spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire::ReadByte___I4,
    Library_spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire::AcquireEx___I4,
    Library_spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire::Release___I4,
    Library_spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire::FindFirstDevice___I4__BOOLEAN__BOOLEAN,
    Library_spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire::FindNextDevice___I4__BOOLEAN__BOOLEAN,
    Library_spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire::SerialNum___I4__SZARRAY_U1__BOOLEAN,
    NULL,
    NULL,
};

const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_Hardware_OneWire =
{
    "Microsoft.SPOT.Hardware.OneWire", 
    0x9B359F66,
    method_lookup
};

