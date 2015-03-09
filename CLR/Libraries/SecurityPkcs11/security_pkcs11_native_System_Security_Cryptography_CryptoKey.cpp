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

static HRESULT CreateAssignKey( CLR_RT_HeapBlock* pSession, CLR_RT_HeapBlock& ref, CK_OBJECT_HANDLE keyPrivate, CK_OBJECT_HANDLE keyPublic )
{
    TINYCLR_HEADER();
    
    CLR_RT_HeapBlock* pKeyHandle;

    TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( ref, g_CLR_RT_WellKnownTypes.m_CryptoKey ));

    pKeyHandle = ref.Dereference();

    pKeyHandle[Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey::FIELD__m_length            ].NumericByRef().s4 = 0;
    pKeyHandle[Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey::FIELD__m_keyType           ].NumericByRef().s4 = -1;
    pKeyHandle[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_session         ].SetObjectReference(pSession);
    pKeyHandle[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_ownsSession     ].SetBoolean(false);
    pKeyHandle[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_isDisposed      ].SetBoolean(false);
    pKeyHandle[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_isSessionClosing].SetBoolean(false);
    if(keyPrivate != CK_OBJECT_HANDLE_INVALID && keyPublic == CK_OBJECT_HANDLE_INVALID)
        pKeyHandle[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiObject::FIELD__m_handle        ].NumericByRef().u4 = keyPrivate;
    else
        pKeyHandle[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiObject::FIELD__m_handle        ].NumericByRef().u4 = keyPublic;
    pKeyHandle[Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey::FIELD__m_privateKeyHandle  ].NumericByRef().u4 = keyPrivate;

    TINYCLR_NOCLEANUP();
}

static void GetMechansim(CLR_RT_HeapBlock* hbMech, CK_MECHANISM_PTR pMech)
{
    CLR_RT_HeapBlock_Array* pParamArray = hbMech[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Mechanism::FIELD__Parameter].DereferenceArray();
        
    pMech->mechanism = hbMech[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Mechanism::FIELD__Type].NumericByRef().u4;

    if(pParamArray != NULL && pParamArray->m_numOfElements > 0)
    {
        pMech->pParameter = pParamArray->GetFirstElement();
        pMech->ulParameterLen = pParamArray->m_numOfElements;
    }
    else
    {
        pMech->pParameter = NULL;
        pMech->ulParameterLen = 0;
    }    
}

static HRESULT SetupAttributes(CLR_RT_HeapBlock_Array* phbAttribs, CLR_INT32& hbAttribCnt, CK_ATTRIBUTE* attribs, const CLR_INT32 attribCnt)
{
    if(phbAttribs != NULL)
    {
        CLR_RT_HeapBlock* pAttrib;
        CLR_INT32 i;
        
        if((hbAttribCnt = (CLR_INT32)phbAttribs->m_numOfElements) > attribCnt) return CLR_E_OUT_OF_RANGE;

        pAttrib = (CLR_RT_HeapBlock*)phbAttribs->GetFirstElement();

        for(i=0; i<(CLR_INT32)phbAttribs->m_numOfElements; i++)
        {
            CLR_RT_HeapBlock*       pElement    = pAttrib->Dereference();
            CLR_RT_HeapBlock_Array* pValueArray = pElement[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiAttribute::FIELD__Value].DereferenceArray();
            
            attribs[i].type       = pElement[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiAttribute::FIELD__Type].NumericByRef().u4;
            attribs[i].pValue     = pValueArray->GetFirstElement();
            attribs[i].ulValueLen = pValueArray->m_numOfElements;

            pAttrib++;
        }
    }    

    return S_OK;
    
}

HRESULT Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey::DeriveKeyInternal___SystemSecurityCryptographyCryptoKey__MicrosoftSPOTCryptokiMechanism__SZARRAY_MicrosoftSPOTCryptokiCryptokiAttribute( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       pThis    = stack.This();
    CLR_RT_HeapBlock*       pMech    = stack.Arg1().Dereference(); 
    CLR_RT_HeapBlock_Array* pAttribs = stack.Arg2().DereferenceArray();
    CLR_RT_HeapBlock*       pSession;
    CLR_RT_HeapBlock        ref;
    CK_ATTRIBUTE            attribs[20];
    CK_MECHANISM            mech;
    CK_OBJECT_HANDLE        keyHandle = CK_OBJECT_HANDLE_INVALID;
    CLR_INT32               attribCnt = 0;
    CK_SESSION_HANDLE       hSession;
    CK_OBJECT_HANDLE        hBaseKey;

    FAULT_ON_NULL_ARG(pMech);

    hBaseKey   = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiObject::FIELD__m_handle   ].NumericByRef().u4;
    pSession   = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_session].Dereference();  FAULT_ON_NULL(pSession);
    hSession   = pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle       ].NumericByRef().u4;

    GetMechansim(pMech, &mech);

    if(FAILED(SetupAttributes(pAttribs, attribCnt, attribs, ARRAYSIZE(attribs)))) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    CRYPTOKI_CHECK_RESULT(stack, C_DeriveKey( hSession, &mech, hBaseKey, attribs, attribCnt, &keyHandle ));
    
    TINYCLR_CHECK_HRESULT(CreateAssignKey(pSession, ref, keyHandle, CK_OBJECT_HANDLE_INVALID));

    stack.SetResult_Object( ref.Dereference() );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey::GenerateKeyInternal___STATIC__SystemSecurityCryptographyCryptoKey__MicrosoftSPOTCryptokiSession__MicrosoftSPOTCryptokiMechanism__SZARRAY_MicrosoftSPOTCryptokiCryptokiAttribute( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       pSession = stack.Arg0().Dereference();
    CLR_RT_HeapBlock*       hbMech   = stack.Arg1().Dereference(); 
    CLR_RT_HeapBlock_Array* pAttribs = stack.Arg2().DereferenceArray();
    CLR_RT_HeapBlock        ref;
    CK_ATTRIBUTE            attribs[20];
    CK_MECHANISM            mech;
    CK_OBJECT_HANDLE        keyHandle = CK_OBJECT_HANDLE_INVALID;
    CLR_INT32               attribCnt = 0;
    CK_SESSION_HANDLE       hSession;

    FAULT_ON_NULL_ARG(pSession);
    FAULT_ON_NULL_ARG(hbMech);

    hSession = (CK_SESSION_HANDLE)pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    if(hSession == CK_SESSION_HANDLE_INVALID) TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);

    GetMechansim(hbMech, &mech);

    if(FAILED(SetupAttributes(pAttribs, attribCnt, attribs, ARRAYSIZE(attribs)))) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    CRYPTOKI_CHECK_RESULT(stack, C_GenerateKey( hSession, &mech, attribs, attribCnt, &keyHandle ));

    TINYCLR_CHECK_HRESULT(CreateAssignKey(pSession, ref, keyHandle, CK_OBJECT_HANDLE_INVALID));

    stack.SetResult_Object( ref.Dereference() );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey::GenerateKeyPairInternal___STATIC__SystemSecurityCryptographyCryptoKey__MicrosoftSPOTCryptokiSession__MicrosoftSPOTCryptokiMechanism__SZARRAY_MicrosoftSPOTCryptokiCryptokiAttribute__SZARRAY_MicrosoftSPOTCryptokiCryptokiAttribute( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       pSession     = stack.Arg0().Dereference();
    CLR_RT_HeapBlock*       pMech        = stack.Arg1().Dereference(); 
    CLR_RT_HeapBlock*       pKey;
    CLR_RT_HeapBlock_Array* phbPubArray  = stack.Arg2().DereferenceArray();
    CLR_RT_HeapBlock_Array* phbPrivArray = stack.Arg3().DereferenceArray();
    CK_ATTRIBUTE            attribsPub[20], attribsPriv[20];
    CK_MECHANISM            mech;
    CK_SESSION_HANDLE       hSession;
    CK_OBJECT_HANDLE        hPriv, hPub;
    CLR_INT32               cntPub = 0, cntPriv = 0;

    FAULT_ON_NULL_ARG(pSession);
    FAULT_ON_NULL_ARG(pMech);

    hSession = (CK_SESSION_HANDLE)pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    if(hSession == CK_SESSION_HANDLE_INVALID) TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);

    GetMechansim(pMech, &mech);

    if(FAILED(SetupAttributes(phbPubArray , cntPub , attribsPub , ARRAYSIZE(attribsPub )))) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
    if(FAILED(SetupAttributes(phbPrivArray, cntPriv, attribsPriv, ARRAYSIZE(attribsPriv)))) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    CRYPTOKI_CHECK_RESULT(stack, C_GenerateKeyPair(hSession, &mech, attribsPub, cntPub, attribsPriv, cntPriv, &hPub, &hPriv));

    TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(stack.PushValue(), g_CLR_RT_WellKnownTypes.m_CryptoKey));

    pKey = &stack.TopValue();

    TINYCLR_CHECK_HRESULT(CreateAssignKey(pSession, *pKey, hPriv, hPub));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey::WrapKey___STATIC__SZARRAY_U1__MicrosoftSPOTCryptokiSession__MicrosoftSPOTCryptokiMechanism__SystemSecurityCryptographyCryptoKey__SystemSecurityCryptographyCryptoKey( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey::UnwrapKeyInternal___STATIC__SystemSecurityCryptographyCryptoKey__MicrosoftSPOTCryptokiSession__MicrosoftSPOTCryptokiMechanism__SystemSecurityCryptographyCryptoKey__SZARRAY_U1__SZARRAY_MicrosoftSPOTCryptokiCryptokiAttribute( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}
