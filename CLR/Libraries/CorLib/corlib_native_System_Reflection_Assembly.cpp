////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CorLib.h"


HRESULT Library_corlib_native_System_Reflection_Assembly::get_FullName___STRING( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    //The defines for maxFULLNAME and parameters for snprintf are currently not validated.
    CLR_RT_Assembly_Instance instance;
    CLR_RT_Assembly*         assm;
    char                     buffer[ TINYCLR_MAX_ASSEMBLY_NAME * 2 ];
    const CLR_RECORD_ASSEMBLY* header ;
    CLR_RT_HeapBlock*        hbAsm = stack.Arg0().Dereference();

    TINYCLR_CHECK_HRESULT(GetTypeDescriptor( *hbAsm, instance ));
   
    assm                              = instance.m_assm;
    header = assm->m_header;

    if(hal_strlen_s(assm->m_szName) > TINYCLR_MAX_ASSEMBLY_NAME)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }
    
    hal_snprintf( buffer, TINYCLR_MAX_ASSEMBLY_NAME * 2, 
                  "%s, Version=%d.%d.%d.%d", 
                  assm->m_szName, 
                  header->version.iMajorVersion, 
                  header->version.iMinorVersion, 
                  header->version.iBuildNumber, 
                  header->version.iRevisionNumber );
    
    stack.SetResult_String( buffer );

    TINYCLR_NOCLEANUP();
#undef MAXFULLNAME
}

HRESULT Library_corlib_native_System_Reflection_Assembly::GetType___SystemType__STRING( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    
    CLR_RT_Assembly_Instance assm;
    CLR_RT_TypeDef_Index     td;
    CLR_RT_HeapBlock*        hbRef;
    CLR_RT_HeapBlock*        hbObj;
    LPCSTR                   szClass = stack.Arg1().RecoverString(); 
    CLR_RT_HeapBlock*        hbAsm = stack.Arg0().Dereference();

    FAULT_ON_NULL(szClass);
    FAULT_ON_NULL(hbAsm);

    TINYCLR_CHECK_HRESULT(GetTypeDescriptor( *hbAsm, assm ));

    hbRef = &stack.PushValueAndClear();
    
    if(g_CLR_RT_TypeSystem.FindTypeDef( szClass, assm.m_assm, td ))
    {
        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(*hbRef, g_CLR_RT_WellKnownTypes.m_TypeStatic));
        
        hbObj = hbRef->Dereference();
        hbObj->SetReflection( td );
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Reflection_Assembly::GetTypes___SZARRAY_SystemType( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_Assembly_Instance assm;
    CLR_RT_HeapBlock*        hbAsm = stack.Arg0().Dereference();

    TINYCLR_CHECK_HRESULT(GetTypeDescriptor( *hbAsm, assm ));

    {
        CLR_RT_Assembly*  pASSM = assm.m_assm;
        CLR_UINT32        num   = pASSM->m_pTablesSize[ TBL_TypeDef ];
        CLR_RT_HeapBlock& top   = stack.PushValue();
        CLR_RT_HeapBlock* hbObj;

        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( top, num, g_CLR_RT_WellKnownTypes.m_TypeStatic ));

        if(num)
        {
            CLR_RT_HeapBlock* pArray = (CLR_RT_HeapBlock*)top.DereferenceArray()->GetFirstElement();

            for(CLR_UINT32 i=0; i<num; i++, pArray++)
            {
                CLR_RT_TypeDef_Index idx; idx.Set( pASSM->m_idx, i );

                TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(*pArray, g_CLR_RT_WellKnownTypes.m_TypeStatic));
                
                hbObj = pArray->Dereference();
                hbObj->SetReflection( idx );
            }
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Reflection_Assembly::GetVersion___VOID__BYREF_I4__BYREF_I4__BYREF_I4__BYREF_I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_StackFrame* caller = stack.Caller(); 
    if(caller == NULL) 
    {
        TINYCLR_SET_AND_LEAVE(S_OK);
    }
    else
    {
        CLR_RT_Assembly_Instance assm;
        CLR_RT_HeapBlock*        hbAsm = stack.Arg0().Dereference();
        
        TINYCLR_CHECK_HRESULT(GetTypeDescriptor( *hbAsm, assm ));

        const CLR_RECORD_VERSION& version = assm.m_assm->m_header->version;

        // we do not check for the reference not to be NULL because this is an internal method
        stack.Arg1().Dereference()->NumericByRef().s4 = version.iMajorVersion;
        stack.Arg2().Dereference()->NumericByRef().s4 = version.iMinorVersion;
        stack.Arg3().Dereference()->NumericByRef().s4 = version.iBuildNumber;
        stack.Arg4().Dereference()->NumericByRef().s4 = version.iRevisionNumber;
    }
    TINYCLR_NOCLEANUP();
}


HRESULT Library_corlib_native_System_Reflection_Assembly::GetManifestResourceNames___SZARRAY_STRING( CLR_RT_StackFrame& stack )
{        
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
            
    CLR_RT_Assembly_Instance assm;
    CLR_RT_HeapBlock*        hbAsm = stack.Arg0().Dereference();
    
    TINYCLR_CHECK_HRESULT(GetTypeDescriptor( *hbAsm, assm ));

    {
        CLR_RT_Assembly*  pAssm  = assm.m_assm;
        CLR_RT_HeapBlock& result = stack.PushValue();
        
        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( result, pAssm->m_pTablesSize[ TBL_ResourcesFiles ], g_CLR_RT_WellKnownTypes.m_String ));

        {
            CLR_RT_HeapBlock* pArray = (CLR_RT_HeapBlock*)result.Array()->GetFirstElement();

            for(int idxResourceFile = 0; idxResourceFile < pAssm->m_pTablesSize[ TBL_ResourcesFiles ]; idxResourceFile++)
            {
                const CLR_RECORD_RESOURCE_FILE* resourceFile = pAssm->GetResourceFile( idxResourceFile );

                TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_String::CreateInstance( *pArray, pAssm->GetString( resourceFile->name ) ));
                
                pArray++;
            }
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Reflection_Assembly::GetExecutingAssembly___STATIC__SystemReflectionAssembly( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock& top = stack.PushValueAndClear();
    CLR_RT_HeapBlock* hbObj;

    CLR_RT_StackFrame* caller = stack.Caller(); if(caller == NULL) TINYCLR_SET_AND_LEAVE(S_OK);

    {
        CLR_RT_Assembly_Index idx; idx.Set( caller->MethodCall().m_assm->m_idx );

        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(top, g_CLR_RT_WellKnownTypes.m_Assembly));
        
        hbObj = top.Dereference();
        hbObj->SetReflection( idx );
    }
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Reflection_Assembly::LoadInternal___STATIC__SystemReflectionAssembly__STRING__BOOLEAN__I4__I4__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

#if defined(TINYCLR_APPDOMAINS)
    CLR_RT_AppDomain*     appDomain = g_CLR_RT_ExecutionEngine.GetCurrentAppDomain();
#endif
    CLR_RT_HeapBlock*     pArgs     = &stack.Arg0();
    CLR_RT_HeapBlock*     hbObj;
    CLR_RT_Assembly*      assembly;
    LPCSTR                szAssembly;
    CLR_RT_Assembly_Index idx; 
    bool                  fVersion;
    CLR_INT16             maj, min, build, rev;
    CLR_RT_HeapBlock&     top = stack.PushValueAndClear();
    
    szAssembly = pArgs[ 0 ].RecoverString(); FAULT_ON_NULL(szAssembly );
    fVersion   = pArgs[ 1 ].NumericByRef().u1 != 0;    
    maj        = pArgs[ 2 ].NumericByRef().s4;    
    min        = pArgs[ 3 ].NumericByRef().s4;
    build      = pArgs[ 4 ].NumericByRef().s4;
    rev        = pArgs[ 5 ].NumericByRef().s4;

    if(fVersion && (maj == -1 || min == -1 || build == -1 || rev == -1))
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    if(fVersion)
    {
        CLR_RECORD_VERSION ver;

        ver.iMajorVersion   = (CLR_UINT16)maj;
        ver.iMinorVersion   = (CLR_UINT16)min;
        ver.iBuildNumber    = (CLR_UINT16)build;
        ver.iRevisionNumber = (CLR_UINT16)rev;
        
        assembly = g_CLR_RT_TypeSystem.FindAssembly( szAssembly, &ver, true ); FAULT_ON_NULL_HR(assembly, CLR_E_INVALID_PARAMETER);
    }
    else
    {
        assembly = g_CLR_RT_TypeSystem.FindAssembly( szAssembly, NULL, false ); FAULT_ON_NULL_HR(assembly, CLR_E_INVALID_PARAMETER);
    }
    
#if defined(TINYCLR_APPDOMAINS)
    TINYCLR_CHECK_HRESULT(appDomain->LoadAssembly( assembly ));
#endif
    idx.Set( assembly->m_idx );

    TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(top, g_CLR_RT_WellKnownTypes.m_Assembly));

    hbObj = top.Dereference();
    hbObj->SetReflection( idx );

    TINYCLR_CLEANUP();

    //Avoid exception handling in common case.  Just return NULL on failure.
    //Managed code decides to throw the exception or not.  
    hr = S_OK;

    TINYCLR_CLEANUP_END();
}

HRESULT Library_corlib_native_System_Reflection_Assembly::Load___STATIC__SystemReflectionAssembly__SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* array = NULL;
    CLR_RT_Assembly*        assm  = NULL;
    CLR_RT_HeapBlock&       top   = stack.PushValueAndClear();
    CLR_RT_HeapBlock*       hbObj;
    CLR_RECORD_ASSEMBLY*    header;

    array = stack.Arg0().DereferenceArray(); FAULT_ON_NULL(array);

    header = (CLR_RECORD_ASSEMBLY*)array->GetFirstElement();
    
    TINYCLR_CHECK_HRESULT(CLR_RT_Assembly::VerifyEndian(header));

    if(header->GoodAssembly())
    {
        //
        // Sorry, you'll have to reboot to load this assembly.
        //
        if(header->flags & CLR_RECORD_ASSEMBLY::c_Flags_NeedReboot)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_BUSY);
        }

        TINYCLR_CHECK_HRESULT(CLR_RT_Assembly::CreateInstance( header, assm ));

        assm->m_pFile = array;

        g_CLR_RT_TypeSystem.Link( assm );

        TINYCLR_CHECK_HRESULT(g_CLR_RT_TypeSystem.ResolveAll         ());
        TINYCLR_CHECK_HRESULT(g_CLR_RT_TypeSystem.PrepareForExecution());
    }

    if(assm)
    {
        CLR_RT_Assembly_Index idx; idx.Set( assm->m_idx );

        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(top, g_CLR_RT_WellKnownTypes.m_Assembly));
        
        hbObj = top.Dereference();
        hbObj->SetReflection( idx );
    }

    TINYCLR_CLEANUP();

    if(FAILED(hr))
    {
        if(assm)
        {
            assm->DestroyInstance();
        }
    }

    TINYCLR_CLEANUP_END();
}

//--//

HRESULT Library_corlib_native_System_Reflection_Assembly::GetTypeDescriptor( CLR_RT_HeapBlock& arg, CLR_RT_Assembly_Instance& inst )
{
    NATIVE_PROFILE_CLR_CORE();
    return CLR_RT_ReflectionDef_Index::Convert( arg, inst ) ? S_OK : CLR_E_NULL_REFERENCE;
}
