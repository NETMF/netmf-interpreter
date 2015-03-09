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



HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiVerify::VerifyInit___VOID__MicrosoftSPOTCryptokiSession__MicrosoftSPOTCryptokiMechanism__SystemSecurityCryptographyCryptoKey( CLR_RT_StackFrame& stack )
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
    FAULT_ON_NULL_ARG(pKeyHandle);

    hSession = (CK_SESSION_HANDLE)pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    if(hSession == CK_SESSION_HANDLE_INVALID) TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);

    mech.mechanism = pMech[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Mechanism::FIELD__Type].NumericByRef().u4;

    pArrParam = pMech[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Mechanism::FIELD__Parameter].DereferenceArray();

    if(pArrParam != NULL && pArrParam->m_numOfElements > 0)
    {
        mech.pParameter     = pArrParam->GetFirstElement();
        mech.ulParameterLen = pArrParam->m_numOfElements;
    }
    else
    {
        mech.pParameter     = NULL;
        mech.ulParameterLen = 0;
    }

    hKey = (CK_OBJECT_HANDLE)pKeyHandle[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiObject::FIELD__m_handle].NumericByRef().s4;

    CRYPTOKI_CHECK_RESULT(stack, C_VerifyInit(hSession, &mech, hKey));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiVerify::VerifyInternal___BOOLEAN__SZARRAY_U1__I4__I4__SZARRAY_U1__I4__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       pThis    = stack.This();
    CLR_RT_HeapBlock_Array* pData    = stack.Arg1().DereferenceArray(); 
    CLR_INT32               offset   = stack.Arg2().NumericByRef().s4;
    CLR_INT32               len      = stack.Arg3().NumericByRef().s4;
    CLR_RT_HeapBlock_Array* pSig     = stack.Arg4().DereferenceArray(); 
    CLR_INT32               sigOff   = stack.Arg5().NumericByRef().s4;
    CLR_INT32               sigLen   = stack.Arg6().NumericByRef().s4;
    CLR_RT_HeapBlock*       pSession = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_session].Dereference();
    CK_SESSION_HANDLE       hSession;
    bool                    retVal   = false;
    CK_RV                   result;

    FAULT_ON_NULL_ARG(pData);
    FAULT_ON_NULL_ARG(pSig);

    if((offset + len   ) > (CLR_INT32)pData->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
    if((sigOff + sigLen) > (CLR_INT32)pSig->m_numOfElements ) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    hSession = (CK_SESSION_HANDLE)pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    if(hSession == CK_SESSION_HANDLE_INVALID) TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);

    result = C_Verify(hSession, pData->GetElement(offset), len, pSig->GetElement(sigOff), sigLen);

    retVal = CKR_OK == result;

    stack.SetResult_Boolean(retVal);

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiVerify::VerifyUpdateInternal___VOID__SZARRAY_U1__I4__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       pThis    = stack.This();
    CLR_RT_HeapBlock_Array* pData    = stack.Arg1().DereferenceArray(); 
    CLR_INT32               offset   = stack.Arg2().NumericByRef().s4;
    CLR_INT32               len      = stack.Arg3().NumericByRef().s4;
    CLR_RT_HeapBlock*       pSession = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_session].Dereference();
    CK_SESSION_HANDLE       hSession;

    FAULT_ON_NULL_ARG(pData);

    if((offset + len) > (CLR_INT32)pData->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    hSession = pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    if(hSession == CK_SESSION_HANDLE_INVALID) TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);

    CRYPTOKI_CHECK_RESULT(stack, C_VerifyUpdate(hSession, pData->GetElement(offset), len));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiVerify::VerifyFinalInternal___BOOLEAN__SZARRAY_U1__I4__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       pThis    = stack.This();
    CLR_RT_HeapBlock_Array* pSig     = stack.Arg1().DereferenceArray(); 
    CLR_INT32               sigOff   = stack.Arg2().NumericByRef().s4;
    CLR_INT32               sigLen   = stack.Arg3().NumericByRef().s4;
    CLR_RT_HeapBlock*       pSession = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_session].Dereference();
    CK_SESSION_HANDLE       hSession;
    bool                    retVal   = false;
    CK_RV                   result;

    FAULT_ON_NULL_ARG(pSig);

    if((sigOff + sigLen) > (CLR_INT32)pSig->m_numOfElements ) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    hSession = pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    if(hSession == CK_SESSION_HANDLE_INVALID) TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);

    result = C_VerifyFinal(hSession, pSig->GetElement(sigOff), sigLen);

    switch(result)
    {
        case CKR_SIGNATURE_INVALID:
            retVal = false;
            break;
        case CKR_OK:
            retVal = true;
            break;

        case CKR_SIGNATURE_LEN_RANGE:
            TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
        default:
            CRYPTOKI_CHECK_RESULT(stack, result);
            break;
    }

    stack.SetResult_Boolean(retVal);

    TINYCLR_NOCLEANUP();
}
