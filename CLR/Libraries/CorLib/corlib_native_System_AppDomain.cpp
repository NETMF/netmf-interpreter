////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CorLib.h"

#if defined(TINYCLR_APPDOMAINS)

HRESULT Library_corlib_native_System_AppDomain::GetAssemblies___SZARRAY_SystemReflectionAssembly( CLR_RT_StackFrame& stack )
{   
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    
    CLR_RT_AppDomain* appDomainSav = NULL;
    CLR_RT_AppDomain* appDomain;    

    TINYCLR_CHECK_HRESULT(GetAppDomain( stack.ThisRef(), appDomain, appDomainSav, true ));

    TINYCLR_CHECK_HRESULT(appDomain->GetAssemblies( stack.PushValueAndClear() ));    

    TINYCLR_CLEANUP();

    g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( appDomainSav );    

    TINYCLR_CLEANUP_END();
}

HRESULT Library_corlib_native_System_AppDomain::LoadInternal___SystemReflectionAssembly__STRING__BOOLEAN__I4__I4__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*     pArgs        = &(stack.Arg1());
    CLR_RT_AppDomain*     appDomainSav;
    CLR_RT_AppDomain*     appDomain;
    CLR_RT_Assembly*      assembly;
    CLR_RT_Assembly_Index idx; 
    bool                  fVersion;
    CLR_INT16             maj, min, build, rev;
    LPCSTR                szAssembly;

    TINYCLR_CHECK_HRESULT(GetAppDomain( stack.ThisRef(), appDomain, appDomainSav, true ));

    szAssembly = pArgs[ 0 ].RecoverString(); FAULT_ON_NULL(szAssembly);    
    fVersion   = pArgs[ 1 ].NumericByRef().u8 != 0;
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

        ver.iMajorVersion   = (CLR_UINT16) maj;
        ver.iMinorVersion   = (CLR_UINT16) min;
        ver.iBuildNumber    = (CLR_UINT16) build;
        ver.iRevisionNumber = (CLR_UINT16) rev;
        
        assembly = g_CLR_RT_TypeSystem.FindAssembly( szAssembly, &ver, true ); FAULT_ON_NULL_HR(assembly, CLR_E_INVALID_PARAMETER);
    }
    else
    {    
        assembly = g_CLR_RT_TypeSystem.FindAssembly( szAssembly, NULL, false ); FAULT_ON_NULL_HR(assembly, CLR_E_INVALID_PARAMETER);
    }
    
    TINYCLR_CHECK_HRESULT(appDomain->LoadAssembly( assembly ));

    {
        CLR_RT_HeapBlock&     top = stack.PushValue();
        CLR_RT_HeapBlock*     hbObj;

        idx.Set( assembly->m_idx );

        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(top, g_CLR_RT_WellKnownTypes.m_Assembly));

        hbObj = top.Dereference();
        hbObj->SetReflection( idx );
    }

    TINYCLR_CLEANUP();

    g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( appDomainSav );

    TINYCLR_CLEANUP_END();
}

HRESULT Library_corlib_native_System_AppDomain::CreateDomain___STATIC__SystemAppDomain__STRING( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
   
    CLR_RT_AppDomain*    appDomain   = NULL;        
    CLR_RT_HeapBlock&    pArgs       = stack.Arg0();    
    CLR_RT_HeapBlock     res; res.SetObjectReference( NULL );
    CLR_RT_ProtectFromGC gc( res );
    LPCSTR               szName;

    szName = pArgs.RecoverString(); FAULT_ON_NULL(szName);
    
    TINYCLR_CHECK_HRESULT(CLR_RT_AppDomain::CreateInstance( szName, appDomain ));

    //load mscorlib
    TINYCLR_CHECK_HRESULT(appDomain->LoadAssembly( g_CLR_RT_TypeSystem.m_assemblyMscorlib ));        
    //load Native
    TINYCLR_CHECK_HRESULT(appDomain->LoadAssembly( g_CLR_RT_TypeSystem.m_assemblyNative   ));
    
    TINYCLR_CHECK_HRESULT(appDomain->GetManagedObject( res ));

    //Marshal the new AD to the calling AD.
    TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.GetCurrentAppDomain()->MarshalObject( res, res, appDomain ));

    stack.PushValueAndAssign( res );

    TINYCLR_CLEANUP();

    if(FAILED(hr))
    {
        if(appDomain)
        {
            appDomain->DestroyInstance();
        }        
    }

    TINYCLR_CLEANUP_END();
}

HRESULT Library_corlib_native_System_AppDomain::Unload___STATIC__VOID__SystemAppDomain( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_AppDomain* appDomainSav;
    CLR_RT_AppDomain* appDomain;
    CLR_RT_HeapBlock  hbTimeout;
    CLR_INT64*        timeout;
    bool              fRes;
    
    TINYCLR_CHECK_HRESULT(GetAppDomain( stack.ThisRef(), appDomain, appDomainSav, false ));

    hbTimeout.SetInteger( 5 * 1000 );
    
    if(stack.m_customState == 0)
    {        
        //Attempt to unload the AppDomain only once
        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.UnloadAppDomain( appDomain, stack.m_owningThread ));            
    }

    TINYCLR_CHECK_HRESULT(stack.SetupTimeout( hbTimeout, timeout ));

    fRes = true;
    while(fRes)
    {
        //Check to make sure this AppDomain is the one that caused the event to fire
        if(appDomain->m_state == CLR_RT_AppDomain::AppDomainState_Unloaded) break;

        _ASSERTE(CLR_EE_IS(UnloadingAppDomain));

        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.WaitEvents( stack.m_owningThread, *timeout, CLR_RT_ExecutionEngine::c_Event_AppDomain, fRes ));
    }

    if(!fRes) TINYCLR_SET_AND_LEAVE(CLR_E_TIMEOUT);

    appDomain->DestroyInstance();

    stack.PopValue();
    
    TINYCLR_CLEANUP();

    g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( appDomainSav );    

    TINYCLR_CLEANUP_END();
}

//--//

HRESULT Library_corlib_native_System_AppDomain::GetAppDomain( CLR_RT_HeapBlock& ref, CLR_RT_AppDomain*& appDomain, CLR_RT_AppDomain*& appDomainSav, bool fCheckForUnloadingAppDomain )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*            obj;
    CLR_RT_ObjectToEvent_Source* src;

    //Setting up appDomainSav is guaranteed to be initialized correctly by this function
    appDomainSav = g_CLR_RT_ExecutionEngine.GetCurrentAppDomain();
    
    _ASSERTE(ref.DataType() == DATATYPE_OBJECT);
    
    obj = ref.Dereference(); FAULT_ON_NULL(obj);
    
    if(obj->DataType() == DATATYPE_TRANSPARENT_PROXY)
    {
        appDomain = obj->TransparentProxyAppDomain();

        if(!appDomain) TINYCLR_SET_AND_LEAVE(CLR_E_APPDOMAIN_EXITED);

        obj = obj->TransparentProxyDereference(); FAULT_ON_NULL(obj);
    }

    _ASSERTE(obj->DataType()         == DATATYPE_CLASS                            );
    _ASSERTE(obj->ObjectCls().m_data == g_CLR_RT_WellKnownTypes.m_AppDomain.m_data);
    
    src       = CLR_RT_ObjectToEvent_Source::ExtractInstance( obj[ FIELD__m_appDomain ] ); FAULT_ON_NULL(src);
    appDomain = (CLR_RT_AppDomain*)src->m_eventPtr                                       ; FAULT_ON_NULL(appDomain);
    
    _ASSERTE(appDomain->DataType() == DATATYPE_APPDOMAIN_HEAD);

    if(fCheckForUnloadingAppDomain)
    {
        if(!appDomain->IsLoaded()) TINYCLR_SET_AND_LEAVE(CLR_E_APPDOMAIN_EXITED);        
    }

    (void)g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( appDomain );

    TINYCLR_NOCLEANUP();
}
#else  //#if defined(TINYCLR_APPDOMAINS)

HRESULT Library_corlib_native_System_AppDomain::GetAssemblies___SZARRAY_SystemReflectionAssembly( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    
    CLR_RT_HeapBlock& top = stack.PushValueAndClear();

    int               count  = 0;
    CLR_RT_HeapBlock* pArray = NULL;

    for(int pass=0; pass<2; pass++)
    {
        TINYCLR_FOREACH_ASSEMBLY(g_CLR_RT_TypeSystem)
        {
            if(pASSM->m_header->flags & CLR_RECORD_ASSEMBLY::c_Flags_Patch) continue;

            if(pass == 0)
            {
                count++;
            }
            else
            {
                CLR_RT_HeapBlock* hbObj;
                CLR_RT_Assembly_Index idx; idx.Set( pASSM->m_idx );

                TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(*pArray, g_CLR_RT_WellKnownTypes.m_Assembly));
                hbObj = pArray->Dereference();
                
                hbObj->SetReflection( idx ); 

                pArray++;
            }
        }
        TINYCLR_FOREACH_ASSEMBLY_END();

        if(pass == 0)
        {
            TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( top, count, g_CLR_RT_WellKnownTypes.m_Assembly ));

            pArray = (CLR_RT_HeapBlock*)top.DereferenceArray()->GetFirstElement();
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_AppDomain::LoadInternal___SystemReflectionAssembly__STRING__BOOLEAN__I4__I4__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}


HRESULT Library_corlib_native_System_AppDomain::CreateDomain___STATIC__SystemAppDomain__STRING( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_AppDomain::Unload___STATIC__VOID__SystemAppDomain( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

#endif //#if defined(TINYCLR_APPDOMAINS)

