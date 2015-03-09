////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Hardware.h"

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_SPI::InternalWriteRead___VOID__SZARRAY_U2__I4__I4__SZARRAY_U2__I4__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    {
        CLR_RT_HeapBlock*       pThis           = stack.This();                    FAULT_ON_NULL(pThis);
        CLR_RT_HeapBlock_Array* writeBuffer     = stack.Arg1().DereferenceArray(); FAULT_ON_NULL(writeBuffer);
        CLR_INT32               writeOffset     = stack.Arg2().NumericByRef().s4;
        CLR_INT32               writeCount      = stack.Arg3().NumericByRef().s4;
        CLR_RT_HeapBlock_Array* readBuffer      = stack.Arg4().DereferenceArray(); 
        CLR_INT32               readOffset      = stack.Arg5().NumericByRef().s4;
        CLR_INT32               readCount       = stack.Arg6().NumericByRef().s4;
        CLR_UINT32              startReadOffset = stack.Arg7().NumericByRef().s4;
        
        SPI_CONFIGURATION       config;
        TINYCLR_CHECK_HRESULT(Library_spot_hardware_native_Microsoft_SPOT_Hardware_SPI__Configuration::GetInitialConfig( pThis[ FIELD__m_config ], config ));

        config.MD_16bits = TRUE;

        CPU_SPI_Initialize();
        
        if(!::CPU_SPI_nWrite16_nRead16(
                                    config,
                                    (CLR_UINT16*)writeBuffer->GetElement(writeOffset), 
                                    writeCount,
                                    readBuffer == NULL ? NULL : (CLR_UINT16*)readBuffer ->GetElement(readOffset), 
                                    readBuffer == NULL ? 0    : readCount,
                                    startReadOffset
                                  ))
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_OPERATION);
        }
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_SPI::InternalWriteRead___VOID__SZARRAY_U1__I4__I4__SZARRAY_U1__I4__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    {
        CLR_RT_HeapBlock*       pThis           = stack.This();                    FAULT_ON_NULL(pThis);
        CLR_RT_HeapBlock_Array* writeBuffer     = stack.Arg1().DereferenceArray(); FAULT_ON_NULL(writeBuffer);
        CLR_INT32               writeOffset     = stack.Arg2().NumericByRef().s4;
        CLR_INT32               writeCount      = stack.Arg3().NumericByRef().s4;
        CLR_RT_HeapBlock_Array* readBuffer      = stack.Arg4().DereferenceArray(); 
        CLR_INT32               readOffset      = stack.Arg5().NumericByRef().s4;
        CLR_INT32               readCount       = stack.Arg6().NumericByRef().s4;
        CLR_UINT32              startReadOffset = stack.Arg7().NumericByRef().s4;
        
        SPI_CONFIGURATION       config;
        TINYCLR_CHECK_HRESULT(Library_spot_hardware_native_Microsoft_SPOT_Hardware_SPI__Configuration::GetInitialConfig( pThis[ FIELD__m_config ], config ));

        config.MD_16bits = FALSE;

        CPU_SPI_Initialize();

        if(!::CPU_SPI_nWrite8_nRead8(
                                  config,
                                  writeBuffer->GetElement(writeOffset), 
                                  writeCount,
                                  readBuffer == NULL ? NULL : readBuffer ->GetElement(readOffset), 
                                  readBuffer == NULL ? 0    : readCount,
                                  startReadOffset
                                ))
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_OPERATION);
        }
    }
    TINYCLR_NOCLEANUP();
}

