//-----------------------------------------------------------------------------
//
//                   ** WARNING! ** 
//    This file was generated automatically by a tool.
//    Re-running the tool will overwrite this file.
//    You should copy this file to a custom location
//    before adding any customization in the copy to
//    prevent loss of your changes when the tool is
//    re-run.
//
//-----------------------------------------------------------------------------



#include "security_pkcs11.h"

HRESULT CryptokiHandleError(CLR_RT_StackFrame& stack, CLR_UINT32 result)
{
    if(result != CKR_OK)
    {
        CLR_RT_HeapBlock& res = stack.m_owningThread->m_currentException;
                                
        if((Library_corlib_native_System_Exception::CreateInstance( res, g_CLR_RT_WellKnownTypes.m_CryptoException, CLR_E_FAIL, &stack )) == S_OK)
        {
            res.Dereference()[Library_security_pkcs11_native_System_Security_Cryptography_CryptographicException::FIELD__m_errorCode].SetInteger( (CLR_INT32)result );
        }

        return CLR_E_PROCESS_EXCEPTION;
    }

    return S_OK;
}


HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Cryptoki::_cctor___STATIC__VOID( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CRYPTOKI_CHECK_RESULT(stack, C_Initialize(NULL));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Cryptoki::GetSlotsInternal___STATIC__SZARRAY_MicrosoftSPOTCryptokiSlot( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CK_ULONG                i;
    CK_SLOT_ID              slots[NETMF_CRYPTOKI_MAX_SLOTS];
    CK_ULONG                count = NETMF_CRYPTOKI_MAX_SLOTS;
    CLR_RT_HeapBlock_Array* pSlots;
    CLR_RT_HeapBlock        ref;
    CLR_RT_HeapBlock*       pSlotRef;

    CRYPTOKI_CHECK_RESULT(stack, C_GetSlotList(CK_FALSE, slots, &count));

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance(ref, (CLR_UINT32)count, g_CLR_RT_WellKnownTypes.m_CryptokiSlot));

    pSlots = ref.DereferenceArray();

    pSlotRef = (CLR_RT_HeapBlock*)pSlots->GetFirstElement();
            
    for(i=0; i<count; i++)
    {
        CLR_RT_HeapBlock* pSlot;
        
        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( pSlotRef[i], g_CLR_RT_WellKnownTypes.m_CryptokiSlot ));

        pSlot = pSlotRef[i].Dereference();
      
        pSlot[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot::FIELD__m_slotIndex    ].SetInteger        ((CLR_INT32)slots[i]);
        pSlot[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot::FIELD__m_disposed     ].SetBoolean        (false              );
        pSlot[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot::FIELD__m_evtDispatcher].SetObjectReference(NULL               );
        pSlot[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot::FIELD__m_slotEvent    ].SetObjectReference(NULL               );
        pSlot[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot::FIELD__m_slotInfo     ].SetObjectReference(NULL               );
    }

    stack.SetResult_Object(pSlots);

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Cryptoki::FindSlots___STATIC__SZARRAY_MicrosoftSPOTCryptokiSlot__STRING__SZARRAY_MicrosoftSPOTCryptokiMechanism( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}
