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



HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_FindObjectEnum::FindObjectsInit___VOID__SZARRAY_MicrosoftSPOTCryptokiCryptokiAttribute( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    CLR_RT_HeapBlock*       pThis      = stack.This();
    CLR_RT_HeapBlock_Array* pAttribs   = stack.Arg1().DereferenceArray();
    CLR_RT_HeapBlock*       pAttrib;
    CLR_RT_HeapBlock*       pSession   = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_FindObjectEnum::FIELD__m_session].Dereference();
    CK_SESSION_HANDLE       hSession;
    CK_ATTRIBUTE            attribs[20];
    CLR_UINT32              i;

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

    CRYPTOKI_CHECK_RESULT(stack, C_FindObjectsInit(hSession, attribs, pAttribs->m_numOfElements));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_FindObjectEnum::FindObjects___SZARRAY_MicrosoftSPOTCryptokiCryptokiObject__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    CLR_RT_HeapBlock* pThis      = stack.This();
    CLR_RT_HeapBlock* pSession   = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_FindObjectEnum::FIELD__m_session].Dereference();
    CLR_RT_HeapBlock  ref, *pRef;
    CK_SESSION_HANDLE hSession;
    CK_ULONG          cntObj     = (CK_ULONG)stack.Arg1().NumericByRef().u4;
    CK_OBJECT_HANDLE  objs[128];
    CK_OBJECT_CLASS   objType = CKO_DATA;
    CLR_INT32         i;
    CLR_RT_TypeDef_Index objIndex = g_CLR_RT_WellKnownTypes.m_CryptokiObject;
    BOOL isKey = FALSE;
    CK_ATTRIBUTE      attribs[] =
    {
        { CKA_CLASS, &objType, sizeof(objType) },
    };

    FAULT_ON_NULL(pSession);

    if(cntObj > ARRAYSIZE(objs)) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    hSession = pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().u4;

    CRYPTOKI_CHECK_RESULT(stack, C_FindObjects(hSession, objs, cntObj, &cntObj ));

    CRYPTOKI_CHECK_RESULT(stack, C_GetAttributeValue(hSession, objs[0], attribs, ARRAYSIZE(attribs)));

    SwapEndianAndAssignIfBEc32(objType, objType);
    
    switch(objType)
    {
        case CKO_CERTIFICATE:
            objIndex = g_CLR_RT_WellKnownTypes.m_CryptokiCertificate;
            break;
    
        case CKO_PRIVATE_KEY:
        case CKO_PUBLIC_KEY:
        case CKO_SECRET_KEY:
        case CKO_OTP_KEY:
            objIndex = g_CLR_RT_WellKnownTypes.m_CryptoKey;
            isKey = TRUE;
            break;
    }

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance(ref, (CLR_UINT32)cntObj, objIndex));

    if(cntObj == 0)
    {
        stack.SetResult_Object(ref.DereferenceArray());
        TINYCLR_SET_AND_LEAVE(S_OK);
    }

    pRef = (CLR_RT_HeapBlock*)ref.DereferenceArray()->GetFirstElement();

    for(i=0; i<(INT32)cntObj; i++)
    {
        CLR_RT_HeapBlock *pObject;        

        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( *pRef, objIndex ));

        pObject = pRef->Dereference();
        pObject[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiObject::FIELD__m_handle            ].SetInteger((CLR_INT32)objs[i]);
        pObject[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_session         ].SetObjectReference(pSession);
        pObject[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_ownsSession     ].SetBoolean(false);
        pObject[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_isDisposed      ].SetBoolean(false);
        pObject[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_isSessionClosing].SetBoolean(false);

        if(isKey)
        {
            CK_ULONG keySize = (CK_ULONG)-1;
            CK_ULONG keyType = (CK_ULONG)-1;

            CK_ATTRIBUTE attribs[] =
            {
                { CKA_VALUE_BITS, &keySize, sizeof(keySize) },
                { CKA_KEY_TYPE  , &keyType, sizeof(keyType) },
            };

            pObject[Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey::FIELD__m_keyType].NumericByRef().s4 = SwapEndianIfBEc32(keyType);

            C_GetAttributeValue(hSession, objs[i], attribs, ARRAYSIZE(attribs));
            
            pObject[Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey::FIELD__m_length].NumericByRef().u4 = SwapEndianIfBEc32(keySize);

            switch(objType)
            {
                case CKO_PRIVATE_KEY:
                    pObject[Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey::FIELD__m_privateKeyHandle].NumericByRef().u4 = objs[i];
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

            C_GetAttributeValue(hSession, objs[i], attribs, ARRAYSIZE(attribs));            

            pObject[Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey::FIELD__m_keyType].NumericByRef().s4 = SwapEndianIfBEc32(keyType);

            pObject[Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey::FIELD__m_length].NumericByRef().u4 = SwapEndianIfBEc32(keySize);

            if(isPrivate == TRUE)
            {
                pObject[Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey::FIELD__m_privateKeyHandle].NumericByRef().u4 = objs[i];
            }
            else
            {
                pObject[Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey::FIELD__m_privateKeyHandle].NumericByRef().u4 = CK_OBJECT_HANDLE_INVALID;
            }
        }

        pRef++;
    }

    stack.SetResult_Object(ref.DereferenceArray());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_FindObjectEnum::FindObjectsFinal___VOID( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    CLR_RT_HeapBlock* pThis      = stack.This();
    CLR_RT_HeapBlock* pSession   = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_FindObjectEnum::FIELD__m_session].Dereference();
    CK_SESSION_HANDLE hSession;

    FAULT_ON_NULL(pSession);

    hSession = pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().u4;

    CRYPTOKI_CHECK_RESULT(stack, C_FindObjectsFinal(hSession));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_FindObjectEnum::get_Count___I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    CLR_RT_HeapBlock* pThis      = stack.This();
    CLR_RT_HeapBlock* pSession   = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_FindObjectEnum::FIELD__m_session].Dereference();
    CK_SESSION_HANDLE hSession;
    CK_ULONG          cntObj = 0; 

    FAULT_ON_NULL(pSession);

    hSession = pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().u4;

    CRYPTOKI_CHECK_RESULT(stack, C_FindObjects(hSession, NULL, 0xFFFF, &cntObj ));

    stack.SetResult_I4((CLR_INT32)cntObj);

    TINYCLR_NOCLEANUP();
}
