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



HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiDigest::Init___VOID__MicrosoftSPOTCryptokiMechanism( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       pThis    = stack.This(); 
    CLR_RT_HeapBlock*       pMech    = stack.Arg1().Dereference(); 
    CLR_RT_HeapBlock*       pSession = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_session].Dereference();
    CLR_RT_HeapBlock_Array* pParam;
    CK_MECHANISM            mech;
    CK_SESSION_HANDLE       hSession;

    FAULT_ON_NULL(pSession);
    FAULT_ON_NULL_ARG(pMech);

    hSession = (CK_SESSION_HANDLE)pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;
    pParam   = pMech[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Mechanism::FIELD__Parameter].DereferenceArray();

    if(hSession == CK_SESSION_HANDLE_INVALID) TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);

    mech.mechanism  = pMech[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Mechanism::FIELD__Type].NumericByRef().u4;

    if(pParam != NULL)
    {
        mech.pParameter     = pParam->GetFirstElement();
        mech.ulParameterLen = pParam->m_numOfElements;
    }
    else
    {
        mech.pParameter     = NULL;
        mech.ulParameterLen = 0;
    }

    CRYPTOKI_CHECK_RESULT(stack, C_DigestInit(hSession, &mech));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiDigest::DigestKeyInternal___VOID__SystemSecurityCryptographyCryptoKey( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis      = stack.This();
    CLR_RT_HeapBlock* phKey      = stack.Arg1().Dereference(); 
    CLR_RT_HeapBlock* pSession   = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_session].Dereference();
    CK_SESSION_HANDLE hSession;
    CK_OBJECT_HANDLE  hKey;

    FAULT_ON_NULL_ARG(phKey);
    FAULT_ON_NULL(pSession);

    hSession = (CK_SESSION_HANDLE)pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    if(hSession == CK_SESSION_HANDLE_INVALID) TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);

    hKey = (CK_OBJECT_HANDLE)phKey[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiObject::FIELD__m_handle].NumericByRef().s4;

    CRYPTOKI_CHECK_RESULT(stack, C_DigestKey(hSession, hKey));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiDigest::DigestInternal___SZARRAY_U1__SZARRAY_U1__I4__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       pThis      = stack.This();
    CLR_RT_HeapBlock_Array* pData      = stack.Arg1().DereferenceArray(); 
    CLR_INT32               offset     = stack.Arg2().NumericByRef().s4;
    CLR_INT32               len        = stack.Arg3().NumericByRef().s4;
    CLR_RT_HeapBlock*       pSession   = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_session].Dereference();
    CLR_INT32               digestSize = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiDigest::FIELD__m_hashSize].NumericByRef().s4;
    CK_SESSION_HANDLE       hSession;
    //CLR_INT32                     maxProcessingBytes;

    FAULT_ON_NULL_ARG(pData);
    FAULT_ON_NULL(pSession);

    if((offset + len) > (CLR_INT32)pData->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    hSession = (CK_SESSION_HANDLE)pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    if(hSession == CK_SESSION_HANDLE_INVALID) TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);

    // TODO: add code for processing chunks at a time if size is too big
    //maxProcessingBytes = pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_maxProcessingBytes].NumericByRef().s4;

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance(stack.PushValue(), digestSize, g_CLR_RT_WellKnownTypes.m_UInt8));

    CRYPTOKI_CHECK_RESULT(stack, C_Digest(hSession, pData->GetElement(offset), len, stack.TopValue().DereferenceArray()->GetFirstElement(), (CK_ULONG_PTR)&digestSize));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiDigest::DigestUpdateInternal___VOID__SZARRAY_U1__I4__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       pThis      = stack.This();
    CLR_RT_HeapBlock_Array* pData      = stack.Arg1().DereferenceArray(); 
    CLR_INT32               offset     = stack.Arg2().NumericByRef().s4;
    CLR_INT32               len        = stack.Arg3().NumericByRef().s4;
    CLR_RT_HeapBlock*       pSession   = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_session].Dereference();
    CLR_INT32               digestSize = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiDigest::FIELD__m_hashSize].NumericByRef().s4;
    CLR_INT32               hSession;
    //CLR_INT32                     maxProcessingBytes;

    FAULT_ON_NULL_ARG(pData);
    FAULT_ON_NULL(pSession);

    if((offset + len) > (CLR_INT32)pData->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    hSession = (CK_SESSION_HANDLE)pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    if(hSession == CK_SESSION_HANDLE_INVALID) TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);

    // TODO: add code for processing chunks at a time if size is too big
    //maxProcessingBytes = pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_maxProcessingBytes].NumericByRef().s4;

    CRYPTOKI_CHECK_RESULT(stack, C_DigestUpdate(hSession, pData->GetElement(offset), len));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiDigest::DigestFinalInternal___SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis      = stack.This();
    CLR_RT_HeapBlock* pSession   = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_session].Dereference();
    CLR_INT32         digestSize = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiDigest::FIELD__m_hashSize].NumericByRef().s4;
    CLR_INT32         hSession;
    //CLR_INT32                     maxProcessingBytes;

    FAULT_ON_NULL(pSession);

    hSession = (CK_SESSION_HANDLE)pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    if(hSession == CK_SESSION_HANDLE_INVALID) TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);

    // TODO: add code for processing chunks at a time if size is too big
    //maxProcessingBytes = pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_maxProcessingBytes].NumericByRef().s4;

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance(stack.PushValue(), digestSize, g_CLR_RT_WellKnownTypes.m_UInt8));

    CRYPTOKI_CHECK_RESULT(stack, C_DigestFinal(hSession, stack.TopValue().DereferenceArray()->GetFirstElement(), (CK_ULONG_PTR)&digestSize));

    TINYCLR_NOCLEANUP();
}
