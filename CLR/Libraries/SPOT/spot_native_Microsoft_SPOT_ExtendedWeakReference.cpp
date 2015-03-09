////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT.h"


HRESULT Library_spot_native_Microsoft_SPOT_ExtendedWeakReference::_ctor___VOID__OBJECT__mscorlibSystemType__U4__U4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    CLR_RT_HeapBlock* hbType = stack.Arg2().Dereference();

    const CLR_UINT32 c_validMask = CLR_RT_HeapBlock_WeakReference::WR_SurviveBoot | CLR_RT_HeapBlock_WeakReference::WR_SurvivePowerdown;

    CLR_RT_HeapBlock_WeakReference* weak = (CLR_RT_HeapBlock_WeakReference*)stack.This(); FAULT_ON_NULL(weak);

    if(CLR_RT_ReflectionDef_Index::Convert( *hbType, weak->m_identity.m_selectorHash ) == false)
    {
        weak->m_identity.m_selectorHash = 0;
    }

    weak->m_identity.m_id    =  stack.Arg3().NumericByRef().u4                                                                 ;
    weak->m_identity.m_flags = (stack.Arg4().NumericByRef().u4 & c_validMask) | CLR_RT_HeapBlock_WeakReference::WR_ExtendedType;

    if(weak->m_identity.m_flags & CLR_RT_HeapBlock_WeakReference::WR_SurvivePowerdown)
    {
        weak->m_identity.m_flags |= CLR_RT_HeapBlock_WeakReference::WR_SurviveBoot;
    }

    TINYCLR_SET_AND_LEAVE(weak->SetTarget( stack.Arg1() ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_ExtendedWeakReference::get_Selector___mscorlibSystemType( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock&               top  = stack.PushValueAndClear();
    CLR_RT_HeapBlock_WeakReference* weak = (CLR_RT_HeapBlock_WeakReference*)stack.This(); FAULT_ON_NULL(weak);

    if(weak->m_identity.m_selectorHash != 0)
    {
        CLR_RT_HeapBlock* hbObj;
        CLR_RT_ReflectionDef_Index reflex; reflex.InitializeFromHash( weak->m_identity.m_selectorHash );
        
        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(top, g_CLR_RT_WellKnownTypes.m_TypeStatic));
        hbObj = top.Dereference();
        hbObj->SetReflection( reflex );
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_ExtendedWeakReference::get_Id___U4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_WeakReference* weak = (CLR_RT_HeapBlock_WeakReference*)stack.This(); FAULT_ON_NULL(weak);

    stack.SetResult( weak->m_identity.m_id, DATATYPE_U4 );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_ExtendedWeakReference::get_Flags___U4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_WeakReference* weak = (CLR_RT_HeapBlock_WeakReference*)stack.This(); FAULT_ON_NULL(weak);

    stack.SetResult( weak->m_identity.m_flags, DATATYPE_U4 );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_ExtendedWeakReference::get_Priority___I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_WeakReference* weak = (CLR_RT_HeapBlock_WeakReference*)stack.This(); FAULT_ON_NULL(weak);

    stack.SetResult_I4( weak->m_identity.m_priority );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_ExtendedWeakReference::set_Priority___VOID__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_WeakReference* weak = (CLR_RT_HeapBlock_WeakReference*)stack.This(); FAULT_ON_NULL(weak);

    weak->m_identity.m_priority = stack.Arg1().NumericByRef().s4;

    weak->InsertInPriorityOrder();

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_ExtendedWeakReference::PushBackIntoRecoverList___VOID( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_WeakReference* weak = (CLR_RT_HeapBlock_WeakReference*)stack.This(); FAULT_ON_NULL(weak);

    weak->m_identity.m_flags |= CLR_RT_HeapBlock_WeakReference::WR_Restored;

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_ExtendedWeakReference::Recover___STATIC__MicrosoftSPOTExtendedWeakReference__mscorlibSystemType__U4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock& top = stack.PushValueAndClear();
    CLR_UINT32        hash;
    CLR_UINT32        id;
    CLR_RT_HeapBlock* hbType = stack.Arg0().Dereference();

    if(CLR_RT_ReflectionDef_Index::Convert( *hbType, hash ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    id = stack.Arg1().NumericByRef().u4;

    TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_WeakReference,weak,g_CLR_RT_ExecutionEngine.m_weakReferences)
    {
        if(weak->m_identity.m_selectorHash == hash && weak->m_identity.m_id == id && (weak->m_identity.m_flags & CLR_RT_HeapBlock_WeakReference::WR_Restored))
        {
            weak->m_identity.m_flags &= ~CLR_RT_HeapBlock_WeakReference::WR_Restored;

            if(weak->m_targetSerialized)
            {
                top.SetObjectReference( weak );
                break;
            }
        }
    }
    TINYCLR_FOREACH_NODE_END();

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_ExtendedWeakReference::FlushAll___STATIC__VOID( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    g_CLR_RT_Persistence_Manager.Flush();
    g_CLR_RT_Persistence_Manager.m_state = CLR_RT_Persistence_Manager::STATE_FlushNextObject;
    g_CLR_RT_Persistence_Manager.m_pending_object = NULL;
    g_CLR_RT_Persistence_Manager.Flush();

    TINYCLR_NOCLEANUP_NOLABEL();
}

