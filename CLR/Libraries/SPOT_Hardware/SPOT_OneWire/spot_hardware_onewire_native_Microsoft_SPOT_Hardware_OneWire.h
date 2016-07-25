////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _SPOT_HARDWARE_ONEWIRE_NATIVE_MICROSOFT_SPOT_HARDWARE_ONEWIRE_H_
#define _SPOT_HARDWARE_ONEWIRE_NATIVE_MICROSOFT_SPOT_HARDWARE_ONEWIRE_H_

namespace Microsoft
{
    namespace SPOT
    {
        namespace Hardware
        {
             struct OneWire
             {
                 // Helper Functions to access fields of managed object
                 static UINT32& Get__pin( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_UINT32( pMngObj, Library_spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire::FIELD___pin ); }
                 static UINT32& Get__logicalPort( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_UINT32( pMngObj, Library_spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire::FIELD___logicalPort ); }
     
                 // Declaration of stubs. These functions are implemented by Interop code developers
                 static INT32 TouchReset( CLR_RT_HeapBlock* pMngObj, HRESULT &hr );
                 static INT32 TouchBit( CLR_RT_HeapBlock* pMngObj, INT32 param0, HRESULT &hr );
                 static INT32 TouchByte( CLR_RT_HeapBlock* pMngObj, INT32 param0, HRESULT &hr );
                 static INT32 WriteByte( CLR_RT_HeapBlock* pMngObj, INT32 param0, HRESULT &hr );
                 static INT32 ReadByte( CLR_RT_HeapBlock* pMngObj, HRESULT &hr );
                 static INT32 AcquireEx( CLR_RT_HeapBlock* pMngObj, HRESULT &hr );
                 static INT32 Release( CLR_RT_HeapBlock* pMngObj, HRESULT &hr );
                 static INT32 First( CLR_RT_HeapBlock* pMngObj, INT8 param0, INT8 param1, HRESULT &hr );
                 static INT32 Next( CLR_RT_HeapBlock* pMngObj, INT8 param0, INT8 param1, HRESULT &hr );
                 static INT32 SerialNum( CLR_RT_HeapBlock* pMngObj, CLR_RT_TypedArray_UINT8 param0, INT8 param1, HRESULT &hr );
             };
         }
    }
}
#endif  //_SPOT_HARDWARE_ONEWIRE_NATIVE_MICROSOFT_SPOT_HARDWARE_ONEWIRE_H_
