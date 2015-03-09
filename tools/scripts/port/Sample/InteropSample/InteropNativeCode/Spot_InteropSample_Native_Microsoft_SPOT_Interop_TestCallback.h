////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#ifndef _SPOT_INTEROPSAMPLE_NATIVE_MICROSOFT_SPOT_INTEROP_TESTCALLBACK_H_
#define _SPOT_INTEROPSAMPLE_NATIVE_MICROSOFT_SPOT_INTEROP_TESTCALLBACK_H_

namespace Microsoft
{
    namespace SPOT
    {
        namespace Interop
        {
            struct TestCallback
            {
                struct ManagedFields
                {
                    INT32 &m_CallbackReceived;
                    INT32 &m_CallbackRequired;

                    ManagedFields
                    ( 
                        INT32 &par_m_CallbackReceived,
                        INT32 &par_m_CallbackRequired
                    ) : 
                    m_CallbackReceived( par_m_CallbackReceived ),
                    m_CallbackRequired( par_m_CallbackRequired )

                    {}

                };
                 static void GenerateInterrupt( HRESULT &hr );
            };
        }
    }
}
extern const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_InteropSample_DriverProcs;
#endif  //_SPOT_INTEROPSAMPLE_NATIVE_MICROSOFT_SPOT_INTEROP_TESTCALLBACK_H_
