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

void Cryptoki_PostSlotEvent( void* pContext, unsigned int event )
{
    if(pContext)
    {
        GLOBAL_LOCK(irq);    
        
        SaveNativeEventToHALQueue( (CLR_RT_HeapBlock_NativeEventDispatcher*)pContext, event, 0 );
    }
}

//--//  Serial Port Data Event 

static HRESULT Cryptoki_InitializeSlotEventSink( CLR_RT_HeapBlock_NativeEventDispatcher *pContext, UINT64 userData )
{
    CLR_UINT32 slotID = (CLR_UINT32)userData;

    if(pContext)
    {
        pContext->m_pDrvCustomData = (void*)slotID;

        return S_OK;
    }

    return CLR_E_FAIL;
}

static HRESULT Cryptoki_EnableDisableSlotEventSink( CLR_RT_HeapBlock_NativeEventDispatcher *pContext, bool fEnable )
{
    CLR_UINT32 slotID;

    if(pContext)
    {
        slotID = (CLR_UINT32)pContext->m_pDrvCustomData;

        if(!Cryptoki_ConnectSlotEventSink( slotID, pContext, (fEnable ? Cryptoki_PostSlotEvent : NULL) )) return CLR_E_INVALID_PARAMETER;
    }

    return S_OK;
}

static HRESULT Cryptoki_CleanupSlotEventSink( CLR_RT_HeapBlock_NativeEventDispatcher *pContext )
{
    CLR_UINT32 slotID;

    if(pContext)
    {
        slotID = (CLR_UINT32)pContext->m_pDrvCustomData;

        CleanupNativeEventsFromHALQueue( pContext );

        if(!Cryptoki_ConnectSlotEventSink( slotID, NULL, NULL )) return CLR_E_INVALID_PARAMETER;
    }

    return S_OK;
}

static const CLR_RT_DriverInterruptMethods s_CLR_AssemblyNative_Microsoft_SPOT_Cryptoki_SlotEventSink = 
{ 
    Cryptoki_InitializeSlotEventSink,
    Cryptoki_EnableDisableSlotEventSink,
    Cryptoki_CleanupSlotEventSink
};

const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_Cryptoki_SlotEvent =
{
    "CryptokiSlotEvent", 
    DRIVER_INTERRUPT_METHODS_CHECKSUM,
    &s_CLR_AssemblyNative_Microsoft_SPOT_Cryptoki_SlotEventSink
};


HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot::GetSlotInfoInternal___VOID__MicrosoftSPOTCryptokiSlotSlotInfo( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis     = stack.This();
    CLR_RT_HeapBlock* pVer;
    CK_SLOT_INFO      slotInfo;
    CLR_RT_HeapBlock* pSlotInfo = stack.Arg1().Dereference();
    CK_SLOT_ID        slotID    = (CK_SLOT_ID)pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot::FIELD__m_slotIndex].NumericByRef().u4;

    if(pSlotInfo == NULL) TINYCLR_SET_AND_LEAVE(CLR_E_ARGUMENT_NULL);

    CRYPTOKI_CHECK_RESULT(stack, C_GetSlotInfo(slotID, &slotInfo));

    
    pVer = pSlotInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot__SlotInfo::FIELD__FirmwareVersion].Dereference();

    if(pVer != NULL)
    {
        pVer[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiVersion::FIELD__Major].SetInteger((CLR_UINT32)slotInfo.firmwareVersion.major);
        pVer[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiVersion::FIELD__Minor].SetInteger((CLR_UINT32)slotInfo.firmwareVersion.minor);
    }

    pVer = pSlotInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot__SlotInfo::FIELD__HardwareVersion].Dereference();

    if(pVer != NULL)
    {
        pVer[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiVersion::FIELD__Major].SetInteger((CLR_UINT32)slotInfo.hardwareVersion.major);
        pVer[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiVersion::FIELD__Minor].SetInteger((CLR_UINT32)slotInfo.hardwareVersion.minor);
    }
    

    CLR_RT_HeapBlock_String::CreateInstance(pSlotInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot__SlotInfo::FIELD__Description  ], (LPCSTR)slotInfo.slotDescription);
    CLR_RT_HeapBlock_String::CreateInstance(pSlotInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot__SlotInfo::FIELD__ManufactureID], (LPCSTR)slotInfo.manufacturerID );

    pSlotInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot__SlotInfo::FIELD__Flags].SetInteger   ((CLR_UINT32)slotInfo.flags            );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot::GetTokenInfo___VOID__BYREF_MicrosoftSPOTCryptokiTokenInfo( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis      = stack.This();
    CK_TOKEN_INFO     tokenInfo;
    CLR_RT_HeapBlock* pTokenInfo = stack.Arg1().Dereference()->Dereference();
    CLR_RT_HeapBlock* pVer;

    CK_SLOT_ID slotId = (CK_SLOT_ID)pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot::FIELD__m_slotIndex].NumericByRef().u4;

    CRYPTOKI_CHECK_RESULT(stack, C_GetTokenInfo(slotId, &tokenInfo));

    pVer = pTokenInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_TokenInfo::FIELD__FirmwareVersion].Dereference();

    if(pVer != NULL)
    {
        pVer[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiVersion::FIELD__Major].SetInteger(tokenInfo.firmwareVersion.major);
        pVer[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiVersion::FIELD__Minor].SetInteger(tokenInfo.firmwareVersion.minor);
    }

    pVer = pTokenInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_TokenInfo::FIELD__HardwareVersion].Dereference();

    if(pVer != NULL)
    {
        pVer[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiVersion::FIELD__Major].SetInteger(tokenInfo.hardwareVersion.major);
        pVer[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiVersion::FIELD__Minor].SetInteger(tokenInfo.hardwareVersion.minor);
    }

    CLR_RT_HeapBlock_String::CreateInstance( pTokenInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_TokenInfo::FIELD__Label             ], (LPCSTR)tokenInfo.label         );
    CLR_RT_HeapBlock_String::CreateInstance( pTokenInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_TokenInfo::FIELD__Manufacturer      ], (LPCSTR)tokenInfo.manufacturerID);
    CLR_RT_HeapBlock_String::CreateInstance( pTokenInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_TokenInfo::FIELD__Model             ], (LPCSTR)tokenInfo.model         );
    CLR_RT_HeapBlock_String::CreateInstance( pTokenInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_TokenInfo::FIELD__SerialNumber      ], (LPCSTR)tokenInfo.serialNumber  );
    CLR_RT_HeapBlock_String::CreateInstance( pTokenInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_TokenInfo::FIELD__m_UtcTimeString   ], (LPCSTR)tokenInfo.utcTime       );

    pTokenInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_TokenInfo::FIELD__Flags             ].SetInteger((CLR_UINT32)tokenInfo.flags               );
    pTokenInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_TokenInfo::FIELD__FreePrivateMemory ].SetInteger((CLR_UINT32)tokenInfo.ulFreePrivateMemory );
    pTokenInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_TokenInfo::FIELD__FreePublicMemory  ].SetInteger((CLR_UINT32)tokenInfo.ulFreePublicMemory  );
    pTokenInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_TokenInfo::FIELD__MaxPinLen         ].SetInteger((CLR_UINT32)tokenInfo.ulMaxPinLen         );
    pTokenInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_TokenInfo::FIELD__MaxRwSessionCount ].SetInteger((CLR_UINT32)tokenInfo.ulMaxRwSessionCount );
    pTokenInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_TokenInfo::FIELD__MaxSessionCount   ].SetInteger((CLR_UINT32)tokenInfo.ulMaxSessionCount   );
    pTokenInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_TokenInfo::FIELD__MinPinLen         ].SetInteger((CLR_UINT32)tokenInfo.ulMinPinLen         );
    pTokenInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_TokenInfo::FIELD__SessionCount      ].SetInteger((CLR_UINT32)tokenInfo.ulSessionCount      );
    pTokenInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_TokenInfo::FIELD__TotalPrivateMemory].SetInteger((CLR_UINT32)tokenInfo.ulTotalPrivateMemory);
    pTokenInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_TokenInfo::FIELD__TotalPublicMemory ].SetInteger((CLR_UINT32)tokenInfo.ulTotalPublicMemory );
    

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot::get_SupportedMechanisms___SZARRAY_MicrosoftSPOTCryptokiMechanismType( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis  = stack.This();
    CK_SLOT_ID        slotId = (CK_SLOT_ID)pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot::FIELD__m_slotIndex].NumericByRef().u4;
    CLR_RT_HeapBlock  ref;
    CK_MECHANISM_TYPE mechs[256];
    CK_ULONG          count = ARRAYSIZE(mechs), i;
    CLR_INT32*        pMech;

    CRYPTOKI_CHECK_RESULT(stack, C_GetMechanismList(slotId, mechs, &count));

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance(ref, (CLR_UINT32)count, g_CLR_RT_WellKnownTypes.m_CryptokiMechanismType));

    pMech = (CLR_INT32*)ref.DereferenceArray()->GetFirstElement();

    for(i=0; i<count; i++)
    {
        pMech[i] =  (CLR_INT32)mechs[i];
    }

    stack.SetResult_Object(ref.Dereference());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot::GetMechanismInfo___VOID__MicrosoftSPOTCryptokiMechanismType__BYREF_MicrosoftSPOTCryptokiMechanismInfo( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis  = stack.This();
    CK_SLOT_ID        slotId = (CK_SLOT_ID)pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot::FIELD__m_slotIndex].NumericByRef().u4;
    CK_MECHANISM_TYPE mech   = (CK_MECHANISM_TYPE)stack.Arg1().NumericByRef().u4;
    CLR_RT_HeapBlock* pInfo  = stack.Arg2().Dereference()->Dereference();
    CK_MECHANISM_INFO info;

    CRYPTOKI_CHECK_RESULT(stack, C_GetMechanismInfo(slotId, mech, &info));

    pInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_MechanismInfo::FIELD__Flags     ].SetInteger((CLR_UINT32)info.flags       );
    pInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_MechanismInfo::FIELD__MaxKeySize].SetInteger((CLR_UINT32)info.ulMaxKeySize);
    pInfo[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_MechanismInfo::FIELD__MinKeySize].SetInteger((CLR_UINT32)info.ulMinKeySize);

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot::OpenSession___MicrosoftSPOTCryptokiSession__MicrosoftSPOTCryptokiSessionSessionFlag( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis  = stack.This();
    CLR_RT_HeapBlock* pSession;
    CLR_RT_HeapBlock  ref, refObjects;
    CK_SESSION_HANDLE hSession;
    CK_SLOT_ID        slotId = (CK_SLOT_ID)pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot::FIELD__m_slotIndex].NumericByRef().u4;
    CK_FLAGS          flags  = (CK_FLAGS)stack.Arg1().NumericByRef().u4;
    CK_ULONG          maxProcBytes;

    CRYPTOKI_CHECK_RESULT(stack, C_OpenSession(slotId, CKF_SERIAL_SESSION | flags, NULL, NULL, &hSession));

    if(CKR_OK != Cryptoki_GetTokenMaxProcessingBytes(slotId, &maxProcBytes)) maxProcBytes = 8*1024;

    TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( ref, g_CLR_RT_WellKnownTypes.m_CryptokiSession ));

    TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( refObjects, g_CLR_RT_WellKnownTypes.m_ArrayList ));
    TINYCLR_CHECK_HRESULT(CLR_RT_ArrayListHelper::PrepareArrayList   ( refObjects, 0, CLR_RT_ArrayListHelper::c_defaultCapacity ));

    pSession = ref.Dereference();

    pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle            ].SetInteger((CLR_UINT32)hSession    );
    pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_disposed          ].SetBoolean(false                   );
    pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_maxProcessingBytes].SetInteger((CLR_UINT32)maxProcBytes);
    pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_objects           ].SetObjectReference(refObjects.DereferenceArray());

    // set return value
    stack.SetResult_Object(pSession);
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot::InitializeToken___VOID__STRING__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*        pThis  = stack.This();
    CLR_RT_HeapBlock_String* pPin   = stack.Arg1().DereferenceString(); 
    CLR_RT_HeapBlock_String* pLabel = stack.Arg2().DereferenceString(); 

    LPCSTR            strPin; 
    int               lenPin;
    LPCSTR            strLabel;
    CK_SLOT_ID        slotId = (CK_SLOT_ID)pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot::FIELD__m_slotIndex].NumericByRef().u4;

    if(pPin != NULL)
    {
        strPin = pPin->StringText();
        lenPin = hal_strlen_s(strPin);
    }
    else
    {
        strPin = "";
        lenPin = 0;
    }

    if(pLabel != NULL)
    {
        strLabel = pLabel->StringText();
    }
    else
    {
        strLabel = "";
    }

    CRYPTOKI_CHECK_RESULT(stack, C_InitToken(slotId, (CK_UTF8CHAR_PTR)strPin, lenPin, (CK_UTF8CHAR_PTR)strLabel));

    TINYCLR_NOCLEANUP();
}
