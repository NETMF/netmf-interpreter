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



HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Decryptor::DecryptInit___VOID__MicrosoftSPOTCryptokiSession__MicrosoftSPOTCryptokiMechanism__SystemSecurityCryptographyCryptoKey( CLR_RT_StackFrame& stack )
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

    CRYPTOKI_CHECK_RESULT(stack, C_DecryptInit(hSession, &mech, hKey));

    TINYCLR_NOCLEANUP();
}

// TODO: Make common functions for transformBlock for encrypt/decrypt when async logic is in place
HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Decryptor::TransformBlockInternal___I4__SZARRAY_U1__I4__I4__SZARRAY_U1__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       pThis      = stack.This();
    CLR_RT_HeapBlock_Array* pData      = stack.Arg1().DereferenceArray(); 
    CLR_INT32               dataOffset = stack.Arg2().NumericByRef().s4;
    CLR_INT32               dataLen    = stack.Arg3().NumericByRef().s4;
    CLR_RT_HeapBlock_Array* pOutput    = stack.Arg4().DereferenceArray(); 
    CLR_INT32               outOffset  = stack.Arg5().NumericByRef().s4;
    CLR_RT_HeapBlock*       pSession   = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_session].Dereference();
    CK_SESSION_HANDLE       hSession;
    CLR_UINT32              decrSize;   

    FAULT_ON_NULL_ARG(pData);
    FAULT_ON_NULL_ARG(pOutput);
    FAULT_ON_NULL_ARG(pSession);

    hSession = (CK_SESSION_HANDLE)pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    if(hSession == CK_SESSION_HANDLE_INVALID) TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);

    if((dataOffset + dataLen) > (CLR_INT32)pData->m_numOfElements  ) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
    if((outOffset           ) > (CLR_INT32)pOutput->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    decrSize = pOutput->m_numOfElements - outOffset;

    CRYPTOKI_CHECK_RESULT(stack, C_DecryptUpdate(hSession, pData->GetElement(dataOffset), dataLen, pOutput->GetElement(outOffset), (CK_ULONG_PTR)&decrSize));

    stack.SetResult_I4(decrSize);

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Decryptor::TransformFinalBlockInternal___SZARRAY_U1__SZARRAY_U1__I4__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       pThis     = stack.This();
    CLR_RT_HeapBlock_Array* pData     = stack.Arg1().DereferenceArray(); 
    CLR_INT32               offset    = stack.Arg2().NumericByRef().s4;
    CLR_INT32               len       = stack.Arg3().NumericByRef().s4;
    CLR_RT_HeapBlock*       pSession  = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_session].Dereference();
    CLR_UINT32              decrSize  = 0;
    CK_SESSION_HANDLE       hSession;
    CLR_RT_HeapBlock        hbRef;
    CLR_RT_HeapBlock*       pRet;
    CLR_UINT32              maxOut;

    FAULT_ON_NULL_ARG(pData);
    FAULT_ON_NULL_ARG(pSession);

    if((offset + len) > (CLR_INT32)pData->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    hSession  = (CK_SESSION_HANDLE)pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    if(hSession == CK_SESSION_HANDLE_INVALID) TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);

    CRYPTOKI_CHECK_RESULT(stack, C_Decrypt(hSession, pData->GetElement(offset), len, NULL, (CK_ULONG_PTR)&decrSize));

    maxOut = decrSize;

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance(hbRef, decrSize, g_CLR_RT_WellKnownTypes.m_UInt8));

    CRYPTOKI_CHECK_RESULT(stack, C_Decrypt(hSession, pData->GetElement(offset), len, hbRef.DereferenceArray()->GetFirstElement(), (CK_ULONG_PTR)&decrSize));

    if(decrSize < maxOut)
    {
        CLR_RT_HeapBlock refSmall;
        
        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance(refSmall, decrSize, g_CLR_RT_WellKnownTypes.m_UInt8));

        memcpy(refSmall.DereferenceArray()->GetFirstElement(), hbRef.DereferenceArray()->GetFirstElement(), decrSize);

        pRet = refSmall.Dereference();
    }
    else
    {
        pRet = hbRef.Dereference();
    }

    stack.SetResult_Object(pRet);

    TINYCLR_NOCLEANUP();
}
