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

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::InitSession___VOID__STRING__SZARRAY_MicrosoftSPOTCryptokiMechanismType( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*        pThis  = stack.This();
    CLR_RT_HeapBlock_String* pName  = stack.Arg1().DereferenceString(); 
    CLR_RT_HeapBlock_Array*  pMechTypes = stack.Arg2().DereferenceArray(); 

    CK_MECHANISM_TYPE mechs[100];
    CK_SLOT_ID slotID = CK_SLOT_ID_INVALID;
    CK_SESSION_HANDLE hSession;
    CK_ULONG          maxProcBytes;
    CLR_UINT32        mechTypeCnt;
    LPCSTR            strServiceProvider; 
    CLR_UINT32        lenSP;
    CK_ULONG          mechCnt = 0;

    FAULT_ON_NULL_ARG(pMechTypes);

    if(pName != NULL)
    {
        strServiceProvider = pName->StringText();
        lenSP              = hal_strlen_s(strServiceProvider);
    }
    else
    {
        strServiceProvider = NULL;
        lenSP              = 0;
    }

    mechTypeCnt = pMechTypes->m_numOfElements;

    if(mechTypeCnt > 0)
    {
        CLR_UINT32* pMech = (CLR_UINT32*)pMechTypes->GetFirstElement();
        
        for(CLR_UINT32 i=0; i<mechTypeCnt; i++)
        {
            mechs[mechCnt++] = *pMech;
            pMech++;
        }
    }

    slotID = Cryptoki_FindSlot(strServiceProvider, mechs, mechCnt);

    if(slotID == CK_SLOT_ID_INVALID) TINYCLR_SET_AND_LEAVE(CLR_E_NOT_SUPPORTED);

    CRYPTOKI_CHECK_RESULT(stack, C_OpenSession(slotID, CKF_SERIAL_SESSION, NULL, NULL, &hSession));

    if(CKR_OK != Cryptoki_GetTokenMaxProcessingBytes(slotID, &maxProcBytes)) maxProcBytes = 8*1024;

    pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].SetInteger((CLR_INT32)hSession);

    pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_maxProcessingBytes].SetInteger((CLR_INT32)maxProcBytes);

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::Login___BOOLEAN__MicrosoftSPOTCryptokiSessionUserType__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*        pThis    = stack.This();
    CK_USER_TYPE             userType = (CK_USER_TYPE)stack.Arg1().NumericByRef().u4;
    CLR_RT_HeapBlock_String* pUser    = stack.Arg2().DereferenceString(); 
    CK_RV retVal;

    CK_SESSION_HANDLE hSession;
    LPCSTR            strUserPin; 
    CLR_INT32         lenUserPin;

    if(pUser != NULL)
    {
        strUserPin = pUser->StringText();
        lenUserPin = hal_strlen_s(strUserPin);
    }
    else
    {
        strUserPin = "";
        lenUserPin = 0;
    }

    hSession = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    if(hSession == CK_SESSION_HANDLE_INVALID) TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);

    switch(retVal = C_Login(hSession, userType, (CK_UTF8CHAR_PTR)strUserPin, lenUserPin))
    {
        case CKR_OK:
            stack.SetResult_Boolean(true);
            break;

        case CKR_PIN_EXPIRED:
        case CKR_PIN_INCORRECT:
        case CKR_PIN_INVALID:
        case CKR_PIN_LOCKED:
        case CKR_USER_ALREADY_LOGGED_IN:
            stack.SetResult_Boolean(false);
            break;
            
        case CKR_PIN_LEN_RANGE:
            stack.SetResult_Boolean(false);
            TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

        case CKR_USER_TYPE_INVALID:
            stack.SetResult_Boolean(false);
            TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
            
        default:
            stack.SetResult_Boolean(false);
            CRYPTOKI_CHECK_RESULT(stack, retVal);
            break;
    }            

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::Logout___BOOLEAN( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CK_RV retVal;
    CLR_RT_HeapBlock* pThis = stack.This();

    CK_SESSION_HANDLE hSession;

    hSession = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    if(hSession == CK_SESSION_HANDLE_INVALID) TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);

    switch(retVal = C_Logout(hSession))
    {
        case CKR_OK:
            stack.SetResult_Boolean(true);
            break;

        case CKR_USER_NOT_LOGGED_IN:
            stack.SetResult_Boolean(false);
            break;

        default:
            stack.SetResult_Boolean(false);
            CRYPTOKI_CHECK_RESULT(stack, retVal);
            break;
    }            


    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::GetSessionInfo___VOID__BYREF_MicrosoftSPOTCryptokiSessionSessionInfo( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis        = stack.This();
    CK_SESSION_HANDLE hSession;
    CK_SESSION_INFO   sessionInfo;
    CLR_RT_HeapBlock* pSessionInfo = stack.Arg1().Dereference()->Dereference();

    hSession = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    if(hSession == CK_SESSION_HANDLE_INVALID) TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);

    CRYPTOKI_CHECK_RESULT(stack, C_GetSessionInfo(hSession, &sessionInfo));

    pSessionInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session__SessionInfo::FIELD__DeviceError].SetInteger((CLR_UINT32)sessionInfo.ulDeviceError);
    pSessionInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session__SessionInfo::FIELD__Flag       ].SetInteger((CLR_UINT32)sessionInfo.flags        );
    pSessionInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session__SessionInfo::FIELD__SlotID     ].SetInteger((CLR_UINT32)sessionInfo.slotID       );
    pSessionInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session__SessionInfo::FIELD__State      ].SetInteger((CLR_UINT32)sessionInfo.state        );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::InitializePin___BOOLEAN__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CK_RV retVal;
    CLR_RT_HeapBlock*        pThis = stack.This();
    CLR_RT_HeapBlock_String* pNewPin = stack.Arg1().DereferenceString(); 

    CK_SESSION_HANDLE hSession;
    LPCSTR            strNewPin; 
    CLR_UINT32        lenNewPin;


    if(pNewPin != NULL)
    {
        strNewPin = pNewPin->StringText();
        lenNewPin = hal_strlen_s(strNewPin);
    }
    else
    {
        strNewPin = "";
        lenNewPin = 0;
    }

    hSession = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    if(hSession == CK_SESSION_HANDLE_INVALID) TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);

    switch(retVal = C_InitPIN(hSession, (CK_UTF8CHAR_PTR)strNewPin, lenNewPin))
    {
        case CKR_OK:
            stack.SetResult_Boolean(true);
            break;

        case CKR_PIN_INVALID:
        case CKR_PIN_LEN_RANGE:
            stack.SetResult_Boolean(false);
            break;
            
        case CKR_SESSION_READ_ONLY:
            stack.SetResult_Boolean(false);
            TINYCLR_SET_AND_LEAVE(CLR_E_UNAUTHORIZED_ACCESS);
            break;

        default:
            stack.SetResult_Boolean(false);
            CRYPTOKI_CHECK_RESULT(stack, retVal);
            break;                
    }            

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::SetPin___BOOLEAN__STRING__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    CK_RV retVal;
    CLR_RT_HeapBlock*        pThis = stack.This();
    CLR_RT_HeapBlock_String* pOldPin = stack.Arg1().DereferenceString(); 
    CLR_RT_HeapBlock_String* pNewPin = stack.Arg2().DereferenceString(); 

    CK_SESSION_HANDLE hSession;
    LPCSTR            strOldPin; 
    CLR_UINT32        lenOldPin;
    LPCSTR            strNewPin; 
    CLR_UINT32        lenNewPin;


    if(pOldPin != NULL)
    {
        strOldPin = pOldPin->StringText();
        lenOldPin = hal_strlen_s(strOldPin);
    }
    else
    {
        strOldPin = "";
        lenOldPin = 0;
    }

    if(pNewPin != NULL)
    {
        strNewPin = pNewPin->StringText();
        lenNewPin = hal_strlen_s(strNewPin);
    }
    else
    {
        strNewPin = "";
        lenNewPin = 0;
    }

    hSession = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    if(hSession == CK_SESSION_HANDLE_INVALID) TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);

    switch(retVal = C_SetPIN(hSession, (CK_UTF8CHAR_PTR)strOldPin, lenOldPin, (CK_UTF8CHAR_PTR)strNewPin, lenNewPin))
    {
        case CKR_OK:
            stack.SetResult_Boolean(true);
            break;

        case CKR_PIN_INCORRECT:
        case CKR_PIN_INVALID:
        case CKR_PIN_LOCKED:
        case CKR_PIN_LEN_RANGE:
            stack.SetResult_Boolean(false);
            break;
            
        case CKR_SESSION_READ_ONLY:
            stack.SetResult_Boolean(false);
            TINYCLR_SET_AND_LEAVE(CLR_E_UNAUTHORIZED_ACCESS);
            break;

        default:
            stack.SetResult_Boolean(false);
            CRYPTOKI_CHECK_RESULT(stack, retVal);
            break;
    }            

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::Close___VOID( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis = stack.This();
    CK_SESSION_INFO   sessionInfo;

    CK_SESSION_HANDLE hSession = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    if(hSession == CK_SESSION_HANDLE_INVALID) TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);

    if(CKR_OK == C_GetSessionInfo(hSession, &sessionInfo))
    {
        if(sessionInfo.state >= CKS_RW_USER_FUNCTIONS || sessionInfo.state == CKS_RO_USER_FUNCTIONS)
        {
            C_Logout(hSession);
        }
    }

    CRYPTOKI_CHECK_RESULT(stack, C_CloseSession(hSession));

    TINYCLR_NOCLEANUP();
}
