////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CorLib.h"


HRESULT Library_corlib_native_System_RuntimeType::get_Assembly___SystemReflectionAssembly( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_TypeDef_Instance td;
    CLR_RT_HeapBlock*       hbType = stack.Arg0().Dereference();
    
    TINYCLR_CHECK_HRESULT(GetTypeDescriptor( *hbType, td, NULL ));

    {
        CLR_RT_Assembly_Index idx; idx.Set( td.Assembly() );
        CLR_RT_HeapBlock&     top = stack.PushValue();
        CLR_RT_HeapBlock*     hbObj;
        
        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(top, g_CLR_RT_WellKnownTypes.m_Assembly));
        hbObj = top.Dereference();
        hbObj->SetReflection( idx );
    }
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_RuntimeType::get_Name___STRING( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    CLR_RT_HeapBlock* hbType = stack.Arg0().Dereference();

    TINYCLR_CHECK_HRESULT(GetName( *hbType, false, stack.PushValueAndClear() ));
  
    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_RuntimeType::get_FullName___STRING( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    CLR_RT_HeapBlock* hbType = stack.Arg0().Dereference();

    TINYCLR_CHECK_HRESULT(GetName( *hbType, true, stack.PushValueAndClear() ));
 
    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_RuntimeType::get_BaseType___SystemType( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_TypeDef_Instance td;
    CLR_UINT32              levels;
    CLR_RT_HeapBlock&       top = stack.PushValueAndClear();
    CLR_RT_HeapBlock*       hbType = stack.Arg0().Dereference();

    TINYCLR_CHECK_HRESULT(GetTypeDescriptor( *hbType, td, &levels ));
        
    if(levels > 0)
    {
        CLR_RT_HeapBlock*     hbObj;
        
        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(top, g_CLR_RT_WellKnownTypes.m_TypeStatic));
        hbObj = top.Dereference();
        hbObj->SetReflection( g_CLR_RT_WellKnownTypes.m_Array );
    }
    else if(td.SwitchToParent())
    {
        CLR_RT_HeapBlock*     hbObj;
        
        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(top, g_CLR_RT_WellKnownTypes.m_TypeStatic));
        hbObj = top.Dereference();
        hbObj->SetReflection( td );
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_RuntimeType::GetMethods___SZARRAY_SystemReflectionMethodInfo__SystemReflectionBindingFlags( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    return Library_corlib_native_System_Type::GetMethods( stack, NULL, stack.Arg1().NumericByRef().s4, NULL, 0, true );
}

HRESULT Library_corlib_native_System_RuntimeType::GetField___SystemReflectionFieldInfo__STRING__SystemReflectionBindingFlags( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    return Library_corlib_native_System_Type::GetFields( stack, stack.Arg1().RecoverString(), stack.Arg2().NumericByRef().s4, false );
}

HRESULT Library_corlib_native_System_RuntimeType::GetFields___SZARRAY_SystemReflectionFieldInfo__SystemReflectionBindingFlags( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    return Library_corlib_native_System_Type::GetFields( stack, NULL, stack.Arg1().NumericByRef().s4, true );
}

HRESULT Library_corlib_native_System_RuntimeType::GetInterfaces___SZARRAY_SystemType( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_TypeDef_Instance td;    
    CLR_RT_HeapBlock& top = stack.PushValueAndClear();
    CLR_RT_HeapBlock* ptr;
    CLR_RT_HeapBlock* hbType = stack.Arg0().Dereference();
    
    TINYCLR_CHECK_HRESULT(GetTypeDescriptor( *hbType, td ));

    //
    // Scan the list of interfaces.
    //
    CLR_RT_SignatureParser          parser; parser.Initialize_Interfaces( td.m_assm, td.m_target );
    CLR_RT_SignatureParser::Element res;
    
    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( top, parser.Available(), g_CLR_RT_WellKnownTypes.m_TypeStatic ));
    
    ptr = (CLR_RT_HeapBlock*)top.DereferenceArray()->GetFirstElement();

    while(parser.Available() > 0)
    {
        CLR_RT_HeapBlock*     hbObj;

        TINYCLR_CHECK_HRESULT(parser.Advance( res ));        

        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(*ptr, g_CLR_RT_WellKnownTypes.m_TypeStatic));
        hbObj = ptr->Dereference();
        hbObj->SetReflection( res.m_cls );

        ptr++;
    }    

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_RuntimeType::GetElementType___SystemType( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
                
    CLR_RT_TypeDescriptor   desc;
    CLR_RT_TypeDescriptor   descSub;    
    CLR_RT_HeapBlock& top = stack.PushValueAndClear();
    CLR_RT_HeapBlock* hbType = stack.Arg0().Dereference();

    TINYCLR_CHECK_HRESULT(desc.InitializeFromReflection( hbType->ReflectionDataConst() ));

    if(desc.GetElementType( descSub ))
    {
        CLR_RT_HeapBlock*     hbObj;

        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(top, g_CLR_RT_WellKnownTypes.m_TypeStatic));
        hbObj = top.Dereference();
        hbObj->SetReflection( descSub.m_reflex );
    }
        
    TINYCLR_NOCLEANUP();
}

//--//

HRESULT Library_corlib_native_System_RuntimeType::GetTypeDescriptor( CLR_RT_HeapBlock& arg, CLR_RT_TypeDef_Instance& inst, CLR_UINT32* levels )
{
    NATIVE_PROFILE_CLR_CORE();
    return CLR_RT_ReflectionDef_Index::Convert( arg, inst, levels ) ? S_OK : CLR_E_NULL_REFERENCE;
}

HRESULT Library_corlib_native_System_RuntimeType::GetTypeDescriptor( CLR_RT_HeapBlock& arg, CLR_RT_TypeDef_Instance& inst )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_UINT32 levels;

    TINYCLR_CHECK_HRESULT(GetTypeDescriptor( arg, inst, &levels ));

    if(levels > 0)
    {
        inst.InitializeFromIndex( g_CLR_RT_WellKnownTypes.m_Array );
    }

    TINYCLR_NOCLEANUP();    
}

HRESULT Library_corlib_native_System_RuntimeType::GetName( CLR_RT_HeapBlock& arg, bool fFullName, CLR_RT_HeapBlock& res )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_TypeDef_Instance td;
    CLR_UINT32              levels;
    char                    rgBuffer[ 256 ];
    LPSTR                   szBuffer;
    size_t                  iBuffer;

    TINYCLR_CHECK_HRESULT(GetTypeDescriptor( arg, td, &levels ));
    
    szBuffer = rgBuffer;
    iBuffer  = MAXSTRLEN(rgBuffer);

    TINYCLR_CHECK_HRESULT(g_CLR_RT_TypeSystem.BuildTypeName( td, szBuffer, iBuffer, fFullName ? CLR_RT_TypeSystem::TYPENAME_FLAGS_FULL : 0, levels ));

    TINYCLR_SET_AND_LEAVE(CLR_RT_HeapBlock_String::CreateInstance( res, rgBuffer ));

    TINYCLR_NOCLEANUP();
}
