////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _SPOT_HARDWARE_ONEWIRE_NATIVE_H_
#define _SPOT_HARDWARE_ONEWIRE_NATIVE_H_

#include <TinyCLR_Interop.h>
struct Library_spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire
{
    static const int FIELD___pin = 1;
    static const int FIELD___logicalPort = 2;
    TINYCLR_NATIVE_DECLARE(TouchReset___I4);
    TINYCLR_NATIVE_DECLARE(TouchBit___I4__I4);
    TINYCLR_NATIVE_DECLARE(TouchByte___I4__I4);
    TINYCLR_NATIVE_DECLARE(WriteByte___I4__I4);
    TINYCLR_NATIVE_DECLARE(ReadByte___I4);
    TINYCLR_NATIVE_DECLARE(AcquireEx___I4);
    TINYCLR_NATIVE_DECLARE(Release___I4);
    TINYCLR_NATIVE_DECLARE(FindFirstDevice___I4__BOOLEAN__BOOLEAN);
    TINYCLR_NATIVE_DECLARE(FindNextDevice___I4__BOOLEAN__BOOLEAN);
    TINYCLR_NATIVE_DECLARE(SerialNum___I4__SZARRAY_U1__BOOLEAN);

    //--//

};



extern const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_Hardware_OneWire;

#endif  //_SPOT_HARDWARE_ONEWIRE_NATIVE_H_
