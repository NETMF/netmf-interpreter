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


HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiObject::get_Size___I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis    = stack.This();
    CLR_RT_HeapBlock* pSession = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_session].Dereference();
    CK_SESSION_HANDLE hSession;
    CK_OBJECT_HANDLE  hObject  = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiObject::FIELD__m_handle].NumericByRef().u4;
    CK_ULONG keySize;

    FAULT_ON_NULL(pSession);

    hSession = pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().u4;

    CRYPTOKI_CHECK_RESULT(stack, C_GetObjectSize(hSession, hObject, &keySize));

    stack.SetResult_I4((CLR_INT32)keySize);

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiObject::CopyInternal___MicrosoftSPOTCryptokiCryptokiObject__SZARRAY_MicrosoftSPOTCryptokiCryptokiAttribute( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiObject::GetAttributeValues___BOOLEAN__BYREF_SZARRAY_MicrosoftSPOTCryptokiCryptokiAttribute( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       pThis      = stack.This();
    CLR_RT_HeapBlock*       pAttribRef = stack.Arg1().Dereference();  
    CLR_RT_HeapBlock_Array* pAttribs   = pAttribRef->DereferenceArray();
    CLR_RT_HeapBlock*       pAttrib;
    CLR_RT_HeapBlock*       pSession   = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_session].Dereference();
    CK_SESSION_HANDLE       hSession;
    CK_OBJECT_HANDLE        hObject    = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiObject::FIELD__m_handle].NumericByRef().u4;
    CK_ATTRIBUTE            attribs[20];
    CLR_UINT32              i;
    bool                    fRet;

    FAULT_ON_NULL_ARG(pAttribs);
    FAULT_ON_NULL(pSession);

    if(pAttribs->m_numOfElements > ARRAYSIZE(attribs)) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    hSession = pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().u4;
    pAttrib  = (CLR_RT_HeapBlock*)pAttribs->GetFirstElement();

    for(i=0; i<pAttribs->m_numOfElements; i++)
    {
        CLR_RT_HeapBlock*       pElement    = pAttrib->Dereference();  FAULT_ON_NULL(pElement);
        CLR_RT_HeapBlock_Array* pValueArray = pElement[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiAttribute::FIELD__Value].DereferenceArray();  FAULT_ON_NULL(pValueArray);

        attribs[i].type       = pElement[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiAttribute::FIELD__Type].NumericByRef().u4;
        attribs[i].pValue     = pValueArray->GetFirstElement();
        attribs[i].ulValueLen = pValueArray->m_numOfElements;

        pAttrib++;
    }

    fRet = (CKR_OK == C_GetAttributeValue(hSession, hObject, attribs, pAttribs->m_numOfElements));

    stack.SetResult_Boolean(fRet);

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiObject::SetAttributeValues___BOOLEAN__SZARRAY_MicrosoftSPOTCryptokiCryptokiAttribute( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       pThis      = stack.This();
    CLR_RT_HeapBlock_Array* pAttribs   = stack.Arg1().DereferenceArray();
    CLR_RT_HeapBlock*       pAttrib;
    CLR_RT_HeapBlock*       pSession   = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_session].Dereference();
    CK_SESSION_HANDLE       hSession;
    CK_OBJECT_HANDLE        hObject    = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiObject::FIELD__m_handle].NumericByRef().u4;
    CK_ATTRIBUTE            attribs[20];
    CLR_UINT32              i;
    bool                    fRet;

    FAULT_ON_NULL_ARG(pAttribs);
    FAULT_ON_NULL(pSession);

    if(pAttribs->m_numOfElements > ARRAYSIZE(attribs)) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    hSession = pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().u4;
    pAttrib  = (CLR_RT_HeapBlock*)pAttribs->GetFirstElement();

    for(i=0; i<pAttribs->m_numOfElements; i++)
    {
        CLR_RT_HeapBlock*       pElement    = pAttrib->Dereference();  FAULT_ON_NULL(pElement);
        CLR_RT_HeapBlock_Array* pValueArray = pElement[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiAttribute::FIELD__Value].DereferenceArray();  FAULT_ON_NULL(pValueArray);

        attribs[i].type       = pElement[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiAttribute::FIELD__Type].NumericByRef().u4;
        attribs[i].pValue     = pValueArray->GetFirstElement();
        attribs[i].ulValueLen = pValueArray->m_numOfElements;

        pAttrib++;
    }

    fRet = (CKR_OK == C_SetAttributeValue(hSession, hObject, attribs, pAttribs->m_numOfElements));

    stack.SetResult_Boolean(fRet);

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiObject::Destroy___VOID( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis    = stack.This();
    CLR_RT_HeapBlock* pSession = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_session].Dereference();
    CK_SESSION_HANDLE hSession;
    CK_OBJECT_HANDLE  hObject  = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiObject::FIELD__m_handle].NumericByRef().u4;

    FAULT_ON_NULL(pSession);

    hSession = pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().u4;

    CRYPTOKI_CHECK_RESULT(stack, C_DestroyObject(hSession, hObject));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiObject::CreateObjectInternal___STATIC__MicrosoftSPOTCryptokiCryptokiObject__MicrosoftSPOTCryptokiSession__SZARRAY_MicrosoftSPOTCryptokiCryptokiAttribute( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       pSession = stack.Arg0().Dereference();
    CLR_RT_HeapBlock_Array* pAttribs = stack.Arg1().DereferenceArray();
    CLR_RT_HeapBlock*       pAttrib;
    CLR_RT_HeapBlock*       pObject;
    CK_SESSION_HANDLE       hSession;
    CK_ATTRIBUTE            attribs[20];
    CK_OBJECT_HANDLE        hObj;
    CLR_UINT32              i;
    CLR_INT32               objType = CKO_DATA;
    CLR_INT32               keyType = -1;
    CLR_INT32               objClass = -1;
    CLR_RT_TypeDef_Index    objIndex = g_CLR_RT_WellKnownTypes.m_CryptokiObject;

    FAULT_ON_NULL_ARG(pSession);
    FAULT_ON_NULL_ARG(pAttribs);

    if(pAttribs->m_numOfElements > ARRAYSIZE(attribs)) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    hSession = pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    pAttrib = (CLR_RT_HeapBlock*)pAttribs->GetFirstElement();

    for(i=0; i<pAttribs->m_numOfElements; i++)
    {
        CLR_RT_HeapBlock*       pElement    = pAttrib->Dereference(); FAULT_ON_NULL(pElement);
        CLR_RT_HeapBlock_Array* pValueArray = pElement[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiAttribute::FIELD__Value].DereferenceArray(); FAULT_ON_NULL(pValueArray);

        attribs[i].type       = pElement[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiAttribute::FIELD__Type].NumericByRef().u4;
        attribs[i].pValue     = pValueArray->GetFirstElement();
        attribs[i].ulValueLen = pValueArray->m_numOfElements;

        if(attribs[i].type == CKA_KEY_TYPE)
        {
            keyType = SwapEndianIfBEc32(*(CLR_INT32*)attribs[i].pValue);
        }
        else if(attribs[i].type == CKA_CLASS)
        {
            objClass = SwapEndianIfBEc32(*(CLR_INT32*)attribs[i].pValue);

            switch(objClass)
            {
                case CKO_CERTIFICATE:
                    objIndex = g_CLR_RT_WellKnownTypes.m_CryptokiCertificate;
                    objType = CKO_CERTIFICATE;
                    break;

                case CKO_PRIVATE_KEY:
                case CKO_PUBLIC_KEY:
                case CKO_SECRET_KEY:
                    objIndex = g_CLR_RT_WellKnownTypes.m_CryptoKey;
                    objType = CKO_OTP_KEY;
                    break;
            }
        }

        pAttrib++;
    }

    CRYPTOKI_CHECK_RESULT(stack, C_CreateObject(hSession, attribs, pAttribs->m_numOfElements, &hObj));

    TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( stack.PushValue(), objIndex ));

    pObject = stack.TopValue().Dereference();
    pObject[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiObject::FIELD__m_handle            ].SetInteger((CLR_INT32)hObj);
    pObject[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_session         ].SetObjectReference(pSession);
    pObject[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_ownsSession     ].SetBoolean(false);
    pObject[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_isDisposed      ].SetBoolean(false);
    pObject[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_isSessionClosing].SetBoolean(false);

    if(objType == CKO_OTP_KEY)
    {
        CK_ULONG keySize = (CK_ULONG)-1;

        CK_ATTRIBUTE attribs[] =
        {
            { CKA_VALUE_BITS, &keySize, sizeof(keySize) },
            { CKA_KEY_TYPE  , &keyType, sizeof(keyType) },
        };

        pObject[Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey::FIELD__m_keyType].NumericByRef().s4 = keyType;

        C_GetAttributeValue(hSession, hObj, attribs, ARRAYSIZE(attribs));
        
        pObject[Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey::FIELD__m_length].NumericByRef().u4 = keySize;

        switch(objClass)
        {
            case CKO_PRIVATE_KEY:
                pObject[Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey::FIELD__m_privateKeyHandle].NumericByRef().u4 = hObj;
                break;

            case CKO_PUBLIC_KEY:
            case CKO_SECRET_KEY:
            default:
                pObject[Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey::FIELD__m_privateKeyHandle].NumericByRef().u4 = CK_OBJECT_HANDLE_INVALID;
                break;
        }

    }
    else if(objType == CKO_CERTIFICATE)
    {
        CK_ULONG keySize   = (CK_ULONG)-1;
        CK_ULONG keyType   = (CK_ULONG)-1;
        BOOL     isPrivate = FALSE;
        
        CK_ATTRIBUTE attribs[] = 
        {
            { CKA_VALUE_BITS, &keySize, sizeof(keySize)},
            { CKA_KEY_TYPE  , &keyType, sizeof(keyType)},
            { CKA_PRIVATE   , &isPrivate, sizeof(isPrivate)},
        };

        C_GetAttributeValue(hSession, hObj, attribs, ARRAYSIZE(attribs));            

        pObject[Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey::FIELD__m_keyType].NumericByRef().s4 = keyType;

        pObject[Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey::FIELD__m_length].NumericByRef().u4 = keySize;

        if(SwapEndianIfBEc32(isPrivate))
        {
            pObject[Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey::FIELD__m_privateKeyHandle].NumericByRef().u4 = hObj;
        }
        else
        {
            pObject[Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey::FIELD__m_privateKeyHandle].NumericByRef().u4 = CK_OBJECT_HANDLE_INVALID;
        }
    }

    TINYCLR_NOCLEANUP();
}
