////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#ifndef _SPOT_INTEROPSAMPLE_NATIVE_MICROSOFT_SPOT_INTEROP_TESTNONSTATICFUNCTIONS__TESTINTERNALTYPE_H_
#define _SPOT_INTEROPSAMPLE_NATIVE_MICROSOFT_SPOT_INTEROP_TESTNONSTATICFUNCTIONS__TESTINTERNALTYPE_H_

namespace Microsoft
{
    namespace SPOT
    {
        namespace Interop
        {
            struct TestNonStaticFunctions_TestInternalType
            {
                // Helper Functions to access fields of managed object
                static INT64& Get_m_value( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_INT64( pMngObj, Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions__TestInternalType::FIELD__m_value ); }

                // Declaration of stubs. These functions are implemented by Interop code developers
                static INT64 GetValue( CLR_RT_HeapBlock* pMngObj, HRESULT &hr );
            };
        }
    }
}
#endif  //_SPOT_INTEROPSAMPLE_NATIVE_MICROSOFT_SPOT_INTEROP_TESTNONSTATICFUNCTIONS__TESTINTERNALTYPE_H_
