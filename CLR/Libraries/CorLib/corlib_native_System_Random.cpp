////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "CorLib.h"

HRESULT Library_corlib_native_System_Random::Next___I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_Random* rand;

    TINYCLR_CHECK_HRESULT(GetRandom( stack, rand ));

    stack.SetResult_I4( rand->Next() );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Random::Next___I4__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_Random* rand;
    CLR_INT32      maxValue;

    TINYCLR_CHECK_HRESULT(GetRandom( stack, rand ));

    maxValue = stack.Arg1().NumericByRef().s4;

    if(maxValue <= 0) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    stack.SetResult_I4( rand->Next() % maxValue );
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Random::NextDouble___R8( CLR_RT_StackFrame& stack )
{


    TINYCLR_HEADER();
    
    CLR_RT_Random* rand;

#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)

    double     nextDouble;
#else
    CLR_INT64  nextDouble;

#endif

    TINYCLR_CHECK_HRESULT(GetRandom( stack, rand ));

    nextDouble = rand->NextDouble();

    stack.SetResult_R8( nextDouble );

    TINYCLR_NOCLEANUP();

   
}

HRESULT Library_corlib_native_System_Random::NextBytes___VOID__SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    CLR_RT_Random* rand;
    CLR_RT_HeapBlock_Array* buffer;

    TINYCLR_CHECK_HRESULT(GetRandom( stack, rand ));

    buffer = stack.Arg1().DereferenceArray(); FAULT_ON_NULL(buffer);

    rand->NextBytes( buffer->GetFirstElement(), buffer->m_numOfElements );
        
    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Random::_ctor___VOID( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_Random* rand;

    TINYCLR_CHECK_HRESULT(GetRandom( stack, rand, true ));

    rand->Initialize();

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Random::_ctor___VOID__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_Random* rand;

    TINYCLR_CHECK_HRESULT(GetRandom( stack, rand, true ));

    rand->Initialize( stack.Arg1().NumericByRef().s4 );

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT Library_corlib_native_System_Random::GetRandom( CLR_RT_StackFrame& stack, CLR_RT_Random*& rand, bool create )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis = stack.This(); FAULT_ON_NULL(pThis);

    if(create)
    {
        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_BinaryBlob::CreateInstance( pThis[ FIELD___random ], sizeof(CLR_RT_Random), NULL, NULL, 0 ));
    }

    rand = (CLR_RT_Random*)pThis[ FIELD___random ].DereferenceBinaryBlob()->GetData();
    
    TINYCLR_NOCLEANUP();
}

