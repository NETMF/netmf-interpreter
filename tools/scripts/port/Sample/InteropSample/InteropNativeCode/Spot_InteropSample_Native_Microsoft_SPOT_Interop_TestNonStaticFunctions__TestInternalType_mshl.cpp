////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "Spot_InteropSample_Native.h"
#include "Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions__TestInternalType.h"

using namespace Microsoft::SPOT::Interop;


HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions__TestInternalType::GetValue___I8( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_HeapBlock* pMngObj = Interop_Marshal_RetrieveManagedObject( stack );

        FAULT_ON_NULL(pMngObj);

        INT64 retVal = TestNonStaticFunctions_TestInternalType::GetValue( pMngObj,  hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT64( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}
