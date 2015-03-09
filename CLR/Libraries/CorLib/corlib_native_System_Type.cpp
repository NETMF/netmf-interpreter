////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CorLib.h"


HRESULT Library_corlib_native_System_Type::get_DeclaringType___SystemType( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_TypeDef_Instance td;
    CLR_RT_HeapBlock&       top = stack.PushValueAndClear();
    CLR_RT_HeapBlock*       hbType = stack.Arg0().Dereference();
    
    TINYCLR_CHECK_HRESULT(Library_corlib_native_System_RuntimeType::GetTypeDescriptor( *hbType, td ));

    if(td.m_target->enclosingType != CLR_EmptyIndex)
    {
        CLR_RT_HeapBlock*     hbObj;
        td.Set( td.Assembly(), td.m_target->enclosingType );

        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(top, g_CLR_RT_WellKnownTypes.m_TypeStatic));
        hbObj = top.Dereference();
        hbObj->SetReflection( td );
    } 

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Type::GetMethod___SystemReflectionMethodInfo__STRING__SystemReflectionBindingFlags( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    LPCSTR szText = stack.Arg1().RecoverString(); FAULT_ON_NULL(szText);

    TINYCLR_SET_AND_LEAVE(GetMethods( stack, szText, stack.Arg2().NumericByRef().s4, NULL, 0, false ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Type::IsInstanceOfType___BOOLEAN__OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_TypeDescriptor descTarget;
    CLR_RT_TypeDescriptor desc;
    CLR_RT_HeapBlock*     hbType = stack.Arg0().Dereference();

    if(hbType->DataType() != DATATYPE_REFLECTION) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

    TINYCLR_CHECK_HRESULT(descTarget.InitializeFromReflection( hbType->ReflectionDataConst() ));
    TINYCLR_CHECK_HRESULT(desc      .InitializeFromObject    ( stack.Arg1()                  ));

    stack.SetResult_Boolean( CLR_RT_ExecutionEngine::IsInstanceOf( desc, descTarget ) );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Type::InvokeMember___OBJECT__STRING__SystemReflectionBindingFlags__SystemReflectionBinder__OBJECT__SZARRAY_OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Type::GetConstructor___SystemReflectionConstructorInfo__SZARRAY_SystemType( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* array = stack.Arg1().DereferenceArray(); FAULT_ON_NULL(array);
    CLR_RT_HeapBlock*       pParams;
    int                     iParams;

    pParams = (CLR_RT_HeapBlock*)array->GetFirstElement();
    iParams =             array->m_numOfElements;

    TINYCLR_SET_AND_LEAVE(GetMethods( stack, NULL, c_BindingFlags_CreateInstance | c_BindingFlags_Instance | c_BindingFlags_Public | c_BindingFlags_NonPublic, pParams, iParams, false ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Type::GetMethod___SystemReflectionMethodInfo__STRING__SZARRAY_SystemType( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* array = stack.Arg2().DereferenceArray();
    CLR_RT_HeapBlock*       pParams;
    int                     iParams;

    if(array)
    {
        pParams = (CLR_RT_HeapBlock*)array->GetFirstElement();
        iParams =                    array->m_numOfElements;
    }
    else
    {
        pParams = NULL;
        iParams = 0;
    }

    LPCSTR szText = stack.Arg1().RecoverString(); FAULT_ON_NULL(szText);

    TINYCLR_SET_AND_LEAVE(GetMethods( stack, szText, c_BindingFlags_DefaultLookup, pParams, iParams, false ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Type::GetMethod___SystemReflectionMethodInfo__STRING( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    LPCSTR szText = stack.Arg1().RecoverString(); FAULT_ON_NULL(szText);

    TINYCLR_SET_AND_LEAVE(GetMethods( stack, szText, c_BindingFlags_DefaultLookup, NULL, 0, false ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Type::get_IsNotPublic___BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(CheckFlags( stack, CLR_RECORD_TYPEDEF::TD_Scope_Mask, CLR_RECORD_TYPEDEF::TD_Scope_NotPublic ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Type::get_IsPublic___BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(CheckFlags( stack, CLR_RECORD_TYPEDEF::TD_Scope_Mask, CLR_RECORD_TYPEDEF::TD_Scope_Public ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Type::get_IsClass___BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(CheckFlags( stack, CLR_RECORD_TYPEDEF::TD_Semantics_Mask, CLR_RECORD_TYPEDEF::TD_Semantics_Class ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Type::get_IsInterface___BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(CheckFlags( stack, CLR_RECORD_TYPEDEF::TD_Semantics_Mask, CLR_RECORD_TYPEDEF::TD_Semantics_Interface ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Type::get_IsValueType___BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(CheckFlags( stack, CLR_RECORD_TYPEDEF::TD_Semantics_Mask, CLR_RECORD_TYPEDEF::TD_Semantics_ValueType ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Type::get_IsAbstract___BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(CheckFlags( stack, CLR_RECORD_TYPEDEF::TD_Abstract, CLR_RECORD_TYPEDEF::TD_Abstract ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Type::get_IsEnum___BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(CheckFlags( stack, CLR_RECORD_TYPEDEF::TD_Semantics_Mask, CLR_RECORD_TYPEDEF::TD_Semantics_Enum ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Type::get_IsSerializable___BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(CheckFlags( stack, CLR_RECORD_TYPEDEF::TD_Serializable, CLR_RECORD_TYPEDEF::TD_Serializable ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Type::get_IsArray___BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_TypeDef_Instance td;    
    CLR_RT_HeapBlock*       hbType = stack.Arg0().Dereference();
    
    TINYCLR_CHECK_HRESULT(Library_corlib_native_System_RuntimeType::GetTypeDescriptor( *hbType, td ));
    
    stack.SetResult_Boolean(td.m_data == g_CLR_RT_WellKnownTypes.m_Array.m_data);    

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Type::GetTypeInternal___STATIC__SystemType__STRING__STRING__BOOLEAN__SZARRAY_I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    LPCSTR szClass, szAssembly;
    CLR_RT_HeapBlock_Array* verArray;
    CLR_RT_Assembly* assm;
    CLR_RT_TypeDef_Index td;
    CLR_INT32* ver;
    bool fVersion;

    CLR_RECORD_VERSION version;
    
    CLR_RT_HeapBlock& top = stack.PushValueAndClear();
    CLR_RT_HeapBlock* hbObj;
    szClass               = stack.Arg0().RecoverString(); FAULT_ON_NULL(szClass);
    szAssembly            = stack.Arg1().RecoverString();
    
    fVersion  = stack.Arg2().NumericByRef().u1 != 0;    
    
    if(fVersion)
    {
        verArray  = stack.Arg3().DereferenceArray(); FAULT_ON_NULL(verArray);
        ver = (CLR_INT32*)verArray->GetFirstElement();

        version.iMajorVersion   = ver[0];
        version.iMinorVersion   = ver[1];
        version.iBuildNumber    = ver[2];
        version.iRevisionNumber = ver[3];
    }
    else
    {
        memset( &version, 0, sizeof(CLR_RECORD_VERSION));
    }

    if(szAssembly) 
    {
        assm = g_CLR_RT_TypeSystem.FindAssembly( szAssembly, fVersion ? &version : NULL, fVersion );
    }
    else
    {
        assm = NULL;
    }
    
    if(g_CLR_RT_TypeSystem.FindTypeDef( szClass, assm, td ))
    {
#if defined(TINYCLR_APPDOMAINS)
        CLR_RT_TypeDef_Instance inst; 

        inst.InitializeFromIndex( td );
        if(!g_CLR_RT_ExecutionEngine.GetCurrentAppDomain()->FindAppDomainAssembly( inst.m_assm )) TINYCLR_SET_AND_LEAVE(S_OK);
#endif
        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(top, g_CLR_RT_WellKnownTypes.m_TypeStatic));
        hbObj = top.Dereference();
        hbObj->SetReflection( td );
    }
    else
    {
        CLR_RT_ReflectionDef_Index reflex;
        if(g_CLR_RT_TypeSystem.FindTypeDef( szClass, assm, reflex ))
        {
#if defined(TINYCLR_APPDOMAINS)
            CLR_RT_TypeDef_Instance inst; 

            inst.InitializeFromIndex( reflex.m_data.m_type );
            if(!g_CLR_RT_ExecutionEngine.GetCurrentAppDomain()->FindAppDomainAssembly( inst.m_assm )) TINYCLR_SET_AND_LEAVE(S_OK);
#endif
            TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(top, g_CLR_RT_WellKnownTypes.m_TypeStatic));
            hbObj = top.Dereference();
            hbObj->SetReflection( reflex );
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Type::GetTypeFromHandle___STATIC__SystemType__SystemRuntimeTypeHandle( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock& pThis = stack.Arg0();
    CLR_RT_HeapBlock& top = stack.PushValue();
    CLR_RT_HeapBlock* hbObj;

    TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(top, g_CLR_RT_WellKnownTypes.m_TypeStatic));
    hbObj = top.Dereference();
    hbObj->Assign(pThis); // RuntimeTypeHandle and Type have the same representation.    
    
    TINYCLR_NOCLEANUP();
}

//--//

HRESULT Library_corlib_native_System_Type::CheckFlags( CLR_RT_StackFrame& stack, CLR_UINT32 mask, CLR_UINT32 flag )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_TypeDef_Instance td;
    bool                    fRes;
    CLR_RT_HeapBlock*       hbType = stack.Arg0().Dereference();

    TINYCLR_CHECK_HRESULT(Library_corlib_native_System_RuntimeType::GetTypeDescriptor( *hbType, td ));

    if((td.m_target->flags & mask) == flag)
    {
        fRes = true;
    }
    //
    // Special case, an enum is a valuetype, so let's check for that one.
    //
    else if(mask == CLR_RECORD_TYPEDEF::TD_Semantics_Mask && flag == CLR_RECORD_TYPEDEF::TD_Semantics_ValueType)
    {
        TINYCLR_SET_AND_LEAVE(CheckFlags( stack, CLR_RECORD_TYPEDEF::TD_Semantics_Mask, CLR_RECORD_TYPEDEF::TD_Semantics_Enum ));
    }
    else
    {
        fRes = false;
    }

    stack.SetResult_Boolean( fRes );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Type::GetFields( CLR_RT_StackFrame& stack, LPCSTR szText, CLR_UINT32 bindingFlags, bool fAllMatches )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_TypeDef_Instance td;    
    CLR_RT_TypeDef_Instance tdArg;
    int iField;
    CLR_RT_HeapBlock*       hbType = stack.Arg0().Dereference();
   
    if(bindingFlags == c_BindingFlags_Default) bindingFlags = c_BindingFlags_DefaultLookup;

    TINYCLR_CHECK_HRESULT(Library_corlib_native_System_RuntimeType::GetTypeDescriptor( *hbType, tdArg ));

    {
        CLR_RT_HeapBlock& top = stack.PushValueAndClear();

        for(int pass=0; pass < 2; pass++)
        {
            td     = tdArg;
            iField = 0;
            
            do
            {
                CLR_RT_Assembly*           assm = td.m_assm;
                const CLR_RECORD_TYPEDEF*  tdR  = td.m_target;
                const CLR_RECORD_FIELDDEF* fd   = td.m_assm->GetFieldDef( tdR->sFields_First );
                int                        iTot = tdR->iFields_Num + tdR->sFields_Num;
                int                        i;
                CLR_RT_FieldDef_Index      idx; 

                for(i=0; i<iTot; i++, fd++)
                {
                    LPCSTR fieldName = assm->GetString( fd->name );

                    if(fd->flags & CLR_RECORD_FIELDDEF::FD_NoReflection)
                    {
                        continue;
                    }

                    if(fd->flags & CLR_RECORD_FIELDDEF::FD_Static)
                    {
                        if((bindingFlags & c_BindingFlags_Static) == 0) continue;
                    }
                    else
                    {
                        if((bindingFlags & c_BindingFlags_Instance) == 0) continue;
                    }

                    if((fd->flags & CLR_RECORD_FIELDDEF::FD_Scope_Mask) == CLR_RECORD_FIELDDEF::FD_Scope_Public)
                    {
                        if((bindingFlags & c_BindingFlags_Public) == 0) continue;
                    }
                    else
                    {
                        if((bindingFlags & c_BindingFlags_NonPublic) == 0) continue;
                    }

                    // In this block we check if requested name szText is the same as examined field name.
                    // We check if compare is case insensitive.
                    if (bindingFlags & c_BindingFlags_IgnoreCase)
                    {   
                        // If strings are not eqaul - continue
                        if(szText != NULL && hal_stricmp( fieldName, szText )) continue;
                    }
                    else // Case sensitive compare
                    {   
                        // If strings are not eqaul - continue
                        if(szText != NULL && strcmp( fieldName, szText )) continue;
                    }
                    
                    idx.Set( td.Assembly(), i + tdR->sFields_First );

                    if(!fAllMatches)
                    {
                        CLR_RT_HeapBlock*     hbObj;
                        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(top, g_CLR_RT_WellKnownTypes.m_FieldInfo));
                        hbObj = top.Dereference();
                        hbObj->SetReflection( idx );

                        TINYCLR_SET_AND_LEAVE(S_OK);
                    }
                    else if(pass == 1)
                    {
                        CLR_RT_HeapBlock* elem = (CLR_RT_HeapBlock*)top.DereferenceArray()->GetElement(iField);
                        CLR_RT_HeapBlock* hbObj;
                        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(*elem, g_CLR_RT_WellKnownTypes.m_FieldInfo));
                        hbObj = elem->Dereference();
                        TINYCLR_CHECK_HRESULT(hbObj->SetReflection( idx ));
                    }

                    iField++;                    
                }
                        
                if(bindingFlags & c_BindingFlags_DeclaredOnly) break;
            }
            while(td.SwitchToParent());

            if(pass == 0)
            {
                if(!fAllMatches) TINYCLR_SET_AND_LEAVE(S_OK);

                TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( top, iField, g_CLR_RT_WellKnownTypes.m_FieldInfo ));
            }
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Type::GetMethods( CLR_RT_StackFrame& stack, LPCSTR szText, CLR_UINT32 bindingFlags, CLR_RT_HeapBlock* pParams, int iParams, bool fAllMatches )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_MethodDef_Instance inst; inst.Clear();
    CLR_RT_TypeDef_Instance   td;    
    CLR_RT_TypeDef_Instance   tdArg;    
    int iMethod;
    CLR_RT_HeapBlock& top = stack.PushValueAndClear();
    CLR_RT_HeapBlock*       hbType = stack.Arg0().Dereference();
    
    if(bindingFlags == c_BindingFlags_Default) bindingFlags = c_BindingFlags_DefaultLookup;

    TINYCLR_CHECK_HRESULT(Library_corlib_native_System_RuntimeType::GetTypeDescriptor( *hbType, tdArg ));
    
    for(int pass = 0; pass < 2; pass++)
    {
        td = tdArg;
        iMethod = 0;

        do
        {
            CLR_RT_Assembly*            assm = td.m_assm;
            const CLR_RECORD_TYPEDEF*   tdR  = td.m_target;
            const CLR_RECORD_METHODDEF* md   = assm->GetMethodDef( tdR->methods_First );
            int                         iTot = tdR->sMethods_Num + tdR->iMethods_Num + tdR->vMethods_Num;
            int                         i;

            for(i=0; i<iTot; i++, md++)
            {
                if(md->flags & CLR_RECORD_METHODDEF::MD_Static)
                {
                    if((bindingFlags & c_BindingFlags_Static) == 0) continue;
                }
                else
                {
                    if((bindingFlags & c_BindingFlags_Instance) == 0) continue;
                }

                if((md->flags & CLR_RECORD_METHODDEF::MD_Scope_Mask) == CLR_RECORD_METHODDEF::MD_Scope_Public)
                {
                    if((bindingFlags & c_BindingFlags_Public) == 0) continue;
                }
                else
                {
                    if((bindingFlags & c_BindingFlags_NonPublic) == 0) continue;
                }

                //--//

                if(md->flags & CLR_RECORD_METHODDEF::MD_Constructor)
                {
                    if((bindingFlags & c_BindingFlags_CreateInstance) == 0) continue;
                }
                else
                {
                    if((bindingFlags & c_BindingFlags_CreateInstance) != 0) continue;

                    if(szText != NULL && !strcmp( assm->GetString( md->name ), szText ) == false) continue;
                }

                if(pParams)
                {
                    CLR_RT_SignatureParser          parserLeft ; parserLeft .Initialize_MethodSignature( assm, md               );
                    CLR_RT_SignatureParser          parserRight; parserRight.Initialize_Objects        ( pParams, iParams, true );                    
                    CLR_RT_SignatureParser::Element res;
                    
                    //
                    // Skip return value.
                    //
                    TINYCLR_CHECK_HRESULT(parserLeft.Advance( res ));

                    if(CLR_RT_TypeSystem::MatchSignatureDirect( parserLeft, parserRight, true ) == false) continue;
                }

                CLR_RT_MethodDef_Index    idx;   idx.Set( td.Assembly(), i + tdR->methods_First );
                CLR_RT_MethodDef_Instance inst2; inst2.InitializeFromIndex( idx );
                
                if(fAllMatches)
                {
                    if(pass==1)
                    {                                                
                        CLR_RT_HeapBlock* elem = (CLR_RT_HeapBlock*)top.DereferenceArray()->GetElement(iMethod);
                        CLR_RT_HeapBlock* hbObj;
                        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(*elem, g_CLR_RT_WellKnownTypes.m_MethodInfo));
                        hbObj = elem->Dereference();
                        TINYCLR_CHECK_HRESULT(hbObj->SetReflection( idx ));
                    }

                    iMethod++;                                
                }
                else
                {
                    CLR_RT_HeapBlock* hbObj;
                    
                    if(TINYCLR_INDEX_IS_VALID(inst))
                    {                                    
                        CLR_RT_SignatureParser parserLeft ; parserLeft .Initialize_MethodSignature( inst .m_assm, inst .m_target );
                        CLR_RT_SignatureParser parserRight; parserRight.Initialize_MethodSignature( inst2.m_assm, inst2.m_target );

                        CLR_RT_SignatureParser::Element resLeft;
                        CLR_RT_SignatureParser::Element resRight;                        

                        //
                        // Skip return value.
                        //
                        TINYCLR_CHECK_HRESULT(parserLeft.Advance ( resLeft  ));
                        TINYCLR_CHECK_HRESULT(parserRight.Advance( resRight ));

                        if(!pParams)
                        {                    
                            if(CLR_RT_TypeSystem::MatchSignatureDirect( parserLeft, parserRight, false ) == false)
                            {
                                //Two methods with different signatures, we cannot distinguish between the two.                        
                                TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
                            }          

                            //If they are identical signatures, the first one wins (subclass takes precendence)
                            continue;
                        }
                        else
                        {                                            
                            bool  fLeftBetterMatch  = false;
                            bool  fRightBetterMatch = false;

                            while(parserLeft.Available() > 0)
                            {
                                TINYCLR_CHECK_HRESULT(parserLeft.Advance ( resLeft  ));
                                TINYCLR_CHECK_HRESULT(parserRight.Advance( resRight ));
                
                                bool fRightBetterMatchT = CLR_RT_TypeSystem::MatchSignatureElement( resLeft , resRight, true );
                                bool fLeftBetterMatchT  = CLR_RT_TypeSystem::MatchSignatureElement( resRight, resLeft , true );

                                //If fLeftBetterMatchT && fRightBetterMatchT, one is assignable from the other, they must be the same
                                //  !fLeftBetterMatchT && !fRightBetterMatchT cannot happen, since this signature matches pParams

                                if(fLeftBetterMatchT && !fRightBetterMatchT) fLeftBetterMatch  = true;
                                if(!fLeftBetterMatchT && fRightBetterMatchT) fRightBetterMatch = true;
                            }

                            if(fLeftBetterMatch && fRightBetterMatch)
                            {
                                //If the params match both Foo(Super, Sub) and Foo(Sub, Super) 
                                //we cannot choose between the two                            
                                TINYCLR_SET_AND_LEAVE( CLR_E_INVALID_PARAMETER );
                            }

                            //If they are identical signatures, the first one wins (subclass takes precendence)
                            //Only if Right is better do we have a strictly better match
                            if(!fRightBetterMatch) continue;
                    
                            //Found a better match    
                        }
                    }
                                        
                    inst.InitializeFromIndex( idx );

                    TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(top, g_CLR_RT_WellKnownTypes.m_MethodInfo));
                    hbObj = top.Dereference();
                    hbObj->SetReflection( inst );
                }                               
            }

            if(bindingFlags & (c_BindingFlags_DeclaredOnly | c_BindingFlags_CreateInstance)) break;
        }
        while(td.SwitchToParent());   

        if(pass == 0)
        {
            if(!fAllMatches) TINYCLR_SET_AND_LEAVE(S_OK);

            TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( top, iMethod, g_CLR_RT_WellKnownTypes.m_MethodInfo ));
        }
    }

    TINYCLR_NOCLEANUP();
}
