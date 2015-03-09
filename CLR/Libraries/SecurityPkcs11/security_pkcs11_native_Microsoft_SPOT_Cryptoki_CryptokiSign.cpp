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



HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiSign::SignInit___VOID__MicrosoftSPOTCryptokiSession__MicrosoftSPOTCryptokiMechanism__SystemSecurityCryptographyCryptoKey( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       pSession   = stack.Arg1().Dereference(); 
    CLR_RT_HeapBlock*       pMech      = stack.Arg2().Dereference(); 
    CLR_RT_HeapBlock*       pKeyHandle = stack.Arg3().Dereference();
    CLR_RT_HeapBlock_Array* pArrParam;
    CK_MECHANISM            mech;
    CK_OBJECT_HANDLE        hKey;
    CK_SESSION_HANDLE       hSession;

    FAULT_ON_NULL_ARG(pSession);
    FAULT_ON_NULL_ARG(pMech);

    hSession = (CK_SESSION_HANDLE)pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    mech.mechanism = pMech[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Mechanism::FIELD__Type].NumericByRef().u4;

    pArrParam = pMech[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Mechanism::FIELD__Parameter].DereferenceArray();

    if(pArrParam != NULL && pArrParam->m_numOfElements > 0)
    {
        mech.pParameter     = pArrParam->GetFirstElement();
        mech.ulParameterLen = pArrParam->m_numOfElements;
    }
    else
    {
        mech.pParameter = NULL;
        mech.ulParameterLen = 0;
    }

    hKey = (CK_OBJECT_HANDLE)pKeyHandle[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiObject::FIELD__m_handle].NumericByRef().s4;

    CRYPTOKI_CHECK_RESULT(stack, C_SignInit(hSession, &mech, hKey));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiSign::SignInternal___SZARRAY_U1__SZARRAY_U1__I4__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       pThis    = stack.This();
    CLR_RT_HeapBlock_Array* pData    = stack.Arg1().DereferenceArray();
    CLR_RT_HeapBlock_Array* pRes;
    CLR_INT32               offset   = stack.Arg2().NumericByRef().s4;
    CLR_INT32               len      = stack.Arg3().NumericByRef().s4;
    CLR_RT_HeapBlock*       pSession = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_session].Dereference();
    CK_SESSION_HANDLE       hSession;
    CK_ULONG                sigLen   = 0;
    CLR_RT_HeapBlock        hbRef;

    FAULT_ON_NULL(pSession);
    FAULT_ON_NULL_ARG(pData);

    if((offset + len) > (CLR_INT32)pData->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    hSession = (CK_SESSION_HANDLE)pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    if(hSession == CK_SESSION_HANDLE_INVALID) TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);

    CRYPTOKI_CHECK_RESULT(stack, C_Sign(hSession, pData->GetElement(offset), len, NULL, (CK_ULONG_PTR)&sigLen));

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance(hbRef, sigLen, g_CLR_RT_WellKnownTypes.m_UInt8));

    pRes = hbRef.DereferenceArray();

    CRYPTOKI_CHECK_RESULT(stack, C_Sign(hSession, pData->GetElement(offset), len, pRes->GetFirstElement(), (CK_ULONG_PTR)&sigLen));

    if(sigLen < pRes->m_numOfElements)
    {
        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance(stack.PushValue(), sigLen, g_CLR_RT_WellKnownTypes.m_UInt8));    

        memcpy(stack.TopValue().DereferenceArray()->GetFirstElement(), pRes->GetFirstElement(), sigLen);
    }
    else
    {
        stack.SetResult_Object(pRes);
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiSign::SignUpdateInternal___VOID__SZARRAY_U1__I4__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       pThis      = stack.This();
    CLR_RT_HeapBlock_Array* pData      = stack.Arg1().DereferenceArray(); 
    CLR_INT32               offset     = stack.Arg2().NumericByRef().s4;
    CLR_INT32               len        = stack.Arg3().NumericByRef().s4;
    CLR_RT_HeapBlock*       pSession   = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_session].Dereference();
    CLR_INT32               sigLen     = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiSign::FIELD__m_signatureLength].NumericByRef().s4;
    CK_SESSION_HANDLE       hSession;

    FAULT_ON_NULL_ARG(pData);
    FAULT_ON_NULL(pSession);

    if((offset + len) > (CLR_INT32)pData->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    hSession = (CK_SESSION_HANDLE)pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    // TODO: add code for processing chunks at a time if size is too big
    //maxProcessingBytes = pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_maxProcessingBytes].NumericByRef().s4;

    if(hSession == CK_SESSION_HANDLE_INVALID) TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);

    CRYPTOKI_CHECK_RESULT(stack, C_SignUpdate(hSession, pData->GetElement(offset), len));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiSign::SignFinalInternal___SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       pThis    = stack.This();
    CLR_RT_HeapBlock_Array* pRes;
    CLR_RT_HeapBlock*       pSession = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_session].Dereference();
    CK_SESSION_HANDLE       hSession;
    CLR_UINT32              sigLen   = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiSign::FIELD__m_signatureLength].NumericByRef().u4;   
    CLR_RT_HeapBlock        hbRef;

    FAULT_ON_NULL(pSession);

    hSession = (CK_SESSION_HANDLE)pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    if(hSession == CK_SESSION_HANDLE_INVALID) TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance(hbRef, sigLen, g_CLR_RT_WellKnownTypes.m_UInt8));

    pRes = hbRef.DereferenceArray();

    CRYPTOKI_CHECK_RESULT(stack, C_SignFinal(hSession, pRes->GetFirstElement(), (CK_ULONG_PTR)&sigLen));

    if(sigLen < pRes->m_numOfElements)
    {
        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance(stack.PushValue(), sigLen, g_CLR_RT_WellKnownTypes.m_UInt8));    

        memcpy(stack.TopValue().DereferenceArray()->GetFirstElement(), pRes->GetFirstElement(), sigLen);
    }
    else
    {
        stack.SetResult_Object(pRes);
    }

    TINYCLR_NOCLEANUP();
}
