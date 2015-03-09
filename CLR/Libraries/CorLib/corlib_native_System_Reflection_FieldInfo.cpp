////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CorLib.h"


HRESULT Library_corlib_native_System_Reflection_FieldInfo::SetValue___VOID__OBJECT__OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_FieldDef_Instance   instFD;
    CLR_RT_TypeDef_Instance    instTD;
    CLR_RT_TypeDescriptor      instTDescObj;
    CLR_RT_TypeDef_Instance    instTDField;
    const CLR_RECORD_FIELDDEF* fd;
    CLR_RT_HeapBlock*          obj;
    bool                       fValueType;
    CLR_RT_HeapBlock&          srcVal = stack.Arg2();        
    CLR_RT_HeapBlock&          srcObj = stack.Arg1();        
    CLR_RT_HeapBlock           val; val.Assign( srcVal );    
    CLR_RT_ProtectFromGC       gc( val );


    TINYCLR_CHECK_HRESULT(Library_corlib_native_System_Reflection_FieldInfo::Initialize( stack, instFD, instTD, obj ));

    fd = instFD.m_target;
    
    if(fd->flags & CLR_RECORD_FIELDDEF::FD_NoReflection) // don't allow reflection for fields with NoReflection attribute
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_NOT_SUPPORTED);
    }

    TINYCLR_CHECK_HRESULT(instTDescObj.InitializeFromFieldDefinition(instFD));

    // make sure the right side object is of the same type as the left side        
    if(NULL != val.Dereference() && !CLR_RT_ExecutionEngine::IsInstanceOf(val, instTDescObj.m_handlerCls)) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

    fValueType = obj->IsAValueType();
    if(fValueType || (c_CLR_RT_DataTypeLookup[ obj->DataType() ].m_flags & CLR_RT_DataTypeLookup::c_OptimizedValueType))
    {
        if(val.Dereference() == NULL || !val.Dereference()->IsBoxed()) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    
        if(fValueType)
        {
            _ASSERTE(NULL != obj->Dereference());
            if(NULL == obj->Dereference()) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
            instTDField.InitializeFromIndex( obj->Dereference()->ObjectCls() );
        }
        else
        {
            instTDField.InitializeFromIndex( *c_CLR_RT_DataTypeLookup[ obj->DataType() ].m_cls );
        }
                    
        TINYCLR_CHECK_HRESULT(val.PerformUnboxing( instTDField ));
    }
    else
    {
#if defined(TINYCLR_APPDOMAINS)
        if(srcObj.IsTransparentProxy())
        {
            _ASSERTE(srcObj.DataType() == DATATYPE_OBJECT);
            _ASSERTE(srcObj.Dereference() != NULL && srcObj.Dereference()->DataType() == DATATYPE_TRANSPARENT_PROXY);

            TINYCLR_CHECK_HRESULT(srcObj.Dereference()->TransparentProxyValidate());
            TINYCLR_CHECK_HRESULT(srcObj.Dereference()->TransparentProxyAppDomain()->MarshalObject( val, val ));
        }
#endif
    }

    switch(obj->DataType())
    {
    case DATATYPE_DATETIME: // Special case.
    case DATATYPE_TIMESPAN: // Special case.
        obj->NumericByRef().s8 = val.NumericByRefConst().s8;
        break;

    default:
        obj->Assign( val );
        break;
    }

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT Library_corlib_native_System_Reflection_FieldInfo::Initialize( CLR_RT_StackFrame& stack, CLR_RT_FieldDef_Instance& instFD, CLR_RT_TypeDef_Instance& instTD, CLR_RT_HeapBlock*& obj )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    CLR_RT_HeapBlock* hbField = stack.Arg0().Dereference();

    if(CLR_RT_ReflectionDef_Index::Convert( *hbField, instFD ) == false ||
       instTD.InitializeFromField         (           instFD ) == false  )
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    if(instFD.m_target->flags & CLR_RECORD_FIELDDEF::FD_Static)
    {
        obj = CLR_RT_ExecutionEngine::AccessStaticField( instFD );
                
        if(obj == NULL) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }
    else
    {
        TINYCLR_CHECK_HRESULT(stack.Arg1().EnsureObjectReference( obj ));

        if(CLR_RT_ExecutionEngine::IsInstanceOf( *obj, instTD ) == false)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
        }
                
        obj = &obj[ instFD.CrossReference().m_offset ];
    }

    TINYCLR_NOCLEANUP();
}
