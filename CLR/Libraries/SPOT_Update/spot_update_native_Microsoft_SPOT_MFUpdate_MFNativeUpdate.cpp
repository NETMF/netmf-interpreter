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



#include "SPOT_Update.h"


HRESULT Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::_cctor___STATIC__VOID( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    MFUpdate_Initialize();

    TINYCLR_NOCLEANUP_NOLABEL();
}

static void MarshalStorageHeader(CLR_RT_HeapBlock* pUpdateBase, MFUpdateHeader& header)
{
    CLR_RT_HeapBlock* pVersion;

    pVersion = pUpdateBase[Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFUpdateBase::FIELD__m_updateVersion].Dereference();

    header.PacketSize     = pUpdateBase[Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFUpdateBase::FIELD__m_packetSize   ].NumericByRef().s4;
    header.UpdateID       = pUpdateBase[Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFUpdateBase::FIELD__m_updateID     ].NumericByRef().s4;
    header.UpdateSize     = pUpdateBase[Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFUpdateBase::FIELD__m_updateSize   ].NumericByRef().s4;
    header.UpdateType     = pUpdateBase[Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFUpdateBase::FIELD__m_updateType   ].NumericByRef().s2;
    header.UpdateSubType  = pUpdateBase[Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFUpdateBase::FIELD__m_updateSubType].NumericByRef().s2;
    
    header.Version.usMajor    = pVersion[Library_corlib_native_System_Version::FIELD___Major   ].NumericByRef().u2;
    header.Version.usMinor    = pVersion[Library_corlib_native_System_Version::FIELD___Minor   ].NumericByRef().u2;
    header.Version.usBuild    = pVersion[Library_corlib_native_System_Version::FIELD___Build   ].NumericByRef().u2;
    header.Version.usRevision = pVersion[Library_corlib_native_System_Version::FIELD___Revision].NumericByRef().u2;
}

HRESULT Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::Initialize___STATIC__I4__MicrosoftSPOTMFUpdateMFUpdateBase( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pUpdateBase = stack.Arg0().Dereference();
    MFUpdateHeader header;
    LPCSTR szProvider;

    FAULT_ON_NULL(pUpdateBase);

    szProvider = pUpdateBase[Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFUpdateBase::FIELD__m_provider].Dereference()->StringText();

    MarshalStorageHeader(pUpdateBase, header);

    stack.SetResult_I4(MFUpdate_InitUpdate(szProvider, header));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::AuthCommand___STATIC__BOOLEAN__I4__I4__SZARRAY_U1__BYREF_SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_INT32               handle = stack.Arg0().NumericByRef().s4;
    CLR_INT32               cmd    = stack.Arg1().NumericByRef().s4;
    CLR_RT_HeapBlock_Array* paArgs = stack.Arg2().DereferenceArray();
    CLR_RT_HeapBlock_Array* paResp = stack.Arg3().Dereference()->DereferenceArray();    
    CLR_UINT8*              pResp   = paResp == NULL ? NULL : paResp->GetFirstElement();
    CLR_INT32               respLen = paResp == NULL ? 0    : paResp->m_numOfElements;

    FAULT_ON_NULL_ARG(paArgs);

    stack.SetResult_Boolean(TRUE == MFUpdate_AuthCommand(handle, cmd, paArgs->GetFirstElement(), paArgs->m_numOfElements, pResp, respLen));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::Authenticate___STATIC__BOOLEAN__I4__SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_INT32               handle  = stack.Arg0().NumericByRef().s4;
    CLR_RT_HeapBlock_Array* paArgs  = stack.Arg1().DereferenceArray();
    CLR_UINT8*              pArgs   = paArgs == NULL ? NULL : paArgs->GetFirstElement();
    CLR_INT32               argsLen = paArgs == NULL ? 0    : paArgs->m_numOfElements;

    stack.SetResult_Boolean(TRUE == MFUpdate_Authenticate(handle, pArgs, argsLen));

    TINYCLR_NOCLEANUP_NOLABEL();
}



HRESULT Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::Open___STATIC__BOOLEAN__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_INT32 handle = stack.Arg0().NumericByRef().s4;

    stack.SetResult_I4(MFUpdate_Open(handle));

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::Create___STATIC__BOOLEAN__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_INT32 handle = stack.Arg0().NumericByRef().s4;

    stack.SetResult_I4(MFUpdate_Create(handle));

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::GetMissingPackets___STATIC__VOID__I4__SZARRAY_U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_INT32 handle = stack.Arg0().NumericByRef().s4;
    CLR_RT_HeapBlock_Array* pArray = stack.Arg1().DereferenceArray(); 
    CLR_INT32 pktCount;

    FAULT_ON_NULL(pArray);

    pktCount = pArray->m_numOfElements;

    if(!MFUpdate_GetMissingPackets(handle, (UINT32*)pArray->GetFirstElement(), &pktCount))
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::GetUpdateProperty___STATIC__BOOLEAN__I4__STRING__BYREF_SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_INT32  handle = stack.Arg0().NumericByRef().s4;
    LPCSTR szProperty = stack.Arg1().StringText(); 
    CLR_RT_HeapBlock* pRef;
    CLR_RT_HeapBlock_Array* pData;
    int dataLen;

    FAULT_ON_NULL(szProperty);

    pRef    = stack.Arg2().Dereference();  FAULT_ON_NULL_ARG(pRef);
    pData   = pRef->DereferenceArray();    FAULT_ON_NULL_ARG(pData);
    dataLen = pData->m_numOfElements;

    stack.SetResult_Boolean(TRUE == MFUpdate_GetProperty(handle, szProperty, pData->GetFirstElement(), &dataLen));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::SetUpdateProperty___STATIC__BOOLEAN__I4__STRING__SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_INT32  handle = stack.Arg0().NumericByRef().s4;
    LPCSTR szProperty = stack.Arg1().StringText(); 
    CLR_RT_HeapBlock_Array* pData;
    int dataLen;

    FAULT_ON_NULL(szProperty);

    pData   = stack.Arg2().DereferenceArray(); FAULT_ON_NULL_ARG(pData);
    dataLen = pData->m_numOfElements;

    stack.SetResult_Boolean(TRUE == MFUpdate_SetProperty(handle, szProperty, pData->GetFirstElement(), dataLen));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::AddPacket___STATIC__BOOLEAN__I4__I4__SZARRAY_U1__SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_INT32  handle       = stack.Arg0().NumericByRef().s4;
    CLR_INT32  pktIndex     = stack.Arg1().NumericByRef().s4;
    CLR_RT_HeapBlock_Array* pPacket     = stack.Arg2().DereferenceArray();
    CLR_RT_HeapBlock_Array* pValidation = stack.Arg3().DereferenceArray();
    CLR_UINT8* pValidData;
    CLR_INT32 validLen;

    FAULT_ON_NULL(pPacket);

    if(pValidation == NULL)
    {
        pValidData = NULL;
        validLen = 0;
    }
    else
    {
        pValidData = pValidation->GetFirstElement();
        validLen = pValidation->m_numOfElements;
    }

    stack.SetResult_Boolean(TRUE == MFUpdate_AddPacket(handle, pktIndex, pPacket->GetFirstElement(), pPacket->m_numOfElements, pValidData, validLen));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::Validate___STATIC__BOOLEAN__I4__SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_INT32               handle      = stack.Arg0().NumericByRef().s4;
    CLR_RT_HeapBlock_Array* pValidation = stack.Arg1().DereferenceArray();
    CLR_UINT8*              pValidData;
    CLR_INT32               validLen;

    if(pValidation == NULL)
    {
        pValidData = NULL;
        validLen = 0;
    }
    else
    {
        pValidData = pValidation->GetFirstElement();
        validLen = pValidation->m_numOfElements;
    }

    stack.SetResult_Boolean(TRUE == MFUpdate_Validate(handle, pValidData, validLen));
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::Install___STATIC__BOOLEAN__I4__SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_INT32               handle      = stack.Arg0().NumericByRef().s4;
    CLR_RT_HeapBlock_Array* pValidation = stack.Arg1().DereferenceArray();
    CLR_UINT8*              pValidData;
    CLR_INT32               validLen;

    if(pValidation == NULL)
    {
        pValidData = NULL;
        validLen = 0;
    }
    else
    {
        pValidData = pValidation->GetFirstElement();
        validLen = pValidation->m_numOfElements;
    }

    stack.SetResult_Boolean(TRUE == MFUpdate_Install(handle, pValidData, validLen));

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::DeleteUpdate___STATIC__VOID__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_INT32 handle = stack.Arg0().NumericByRef().s4;

    MFUpdate_Delete(handle);

    TINYCLR_NOCLEANUP_NOLABEL();
}

static int GetTypeSize(CLR_DataType type)
{
    switch(type)
    {
        case DATATYPE_I1:
        case DATATYPE_U1:
        case DATATYPE_CHAR:
            return 1;

        case DATATYPE_I2:
        case DATATYPE_U2:
            return 2;

        case DATATYPE_I4:
        case DATATYPE_U4:
        case DATATYPE_R4:
            return 4;

        case DATATYPE_I8:
        case DATATYPE_U8:
        case DATATYPE_R8:
        case DATATYPE_DATETIME:
        case DATATYPE_TIMESPAN:
            return 8;
            
        default:
            return -1;
    }
}

static bool DeserializeIntrinsicType(CLR_RT_HeapBlock* pObj, CLR_UINT8* serData, CLR_INT32& serOffset, const CLR_INT32 serSize)
{
    int size = 0;
    int i = 0;
    CLR_INT64 val = 0;
    UINT8* tmp = (UINT8*)&val;

    CLR_DataType type = pObj->DataType(); 

    size = GetTypeSize(type);

    if(size <= 0) return false;

    while((serOffset % size) != 0)
    {
        serOffset++;
    }
    
    if(serOffset + size > serSize) return false;

    
    while(size--)
    {
#ifndef NETMF_TARGET_BIG_ENDIAN
        tmp[i++ ] = serData[serOffset++];
#else
        tmp[size] = serData[serOffset++];
#endif
    }

    pObj->SetInteger(val);
    pObj->ChangeDataType(type);
    
    return true;
}

static bool DeserializeObject(CLR_RT_HeapBlock* pObj, CLR_UINT8* serData, CLR_INT32& serOffset, const CLR_INT32 serSize )
{
    if(pObj == NULL)
    {
        return false;
    }

    switch(pObj->DataType())
    {
        case DATATYPE_OBJECT:
            if(!DeserializeObject(pObj->Dereference(), serData, serOffset, serSize)) return false;
            break;
            
        case DATATYPE_CLASS:
        case DATATYPE_VALUETYPE:
            {
                CLR_RT_TypeDef_Instance cls; cls.InitializeFromIndex( pObj->ObjectCls() );
                int                    totFields = cls.CrossReference().m_totalFields;

                while(totFields-- > 0)
                {
                    if(!DeserializeObject( ++pObj, serData, serOffset, serSize )) return false;
                }
            }
            break;

        case DATATYPE_SZARRAY:
            {
                CLR_RT_HeapBlock_Array* array = (CLR_RT_HeapBlock_Array*)pObj;
                int cnt = array->m_numOfElements;

                if(array->m_typeOfElement <= DATATYPE_LAST_NONPOINTER && cnt < 256)
                {
                    CLR_UINT8* pData = (CLR_UINT8*)array->GetFirstElement();

                    int nativeSize = GetTypeSize((CLR_DataType)array->m_typeOfElement);
                    
                    if(nativeSize <= 0) return false;

                    while(cnt--)
                    {
                        int size = nativeSize;
                        int i = 0;
                        
                        while(size--)
                        {
#ifndef NETMF_TARGET_BIG_ENDIAN
                            pData[i++ ] = serData[serOffset++];
#else
                            pData[size] = serData[serOffset++];
#endif
                        }

                        pData += array->m_sizeOfElement;
                    }
                }
                else
                {
                    return false;
                }
            }
            break;

        default:
            {
                CLR_UINT64 data = (CLR_UINT64)pObj->NumericByRef().u8;
                
                if(!DeserializeIntrinsicType(pObj, serData, serOffset, serSize)) return false;
            }
            break;               
    }

    return true;
}

static bool SerializeIntrinsicType(CLR_DataType type, CLR_UINT8* data, CLR_UINT8* serData, CLR_INT32& serOffset, CLR_INT32& serSize )
{
    int size = 0;
    
    size = GetTypeSize(type);

    if(size <= 0) return false;

    if(serData == NULL)
    {
        while((serSize % size) != 0)
        {
            serSize++;
        }

        serSize += size;
    }
    else
    {
        CLR_UINT8* tmp = (CLR_UINT8*)data;

        while((serOffset % size) != 0)
        {
            serOffset++;
        }
        
        if(serOffset + size > serSize) return false;

        while(size--)
        {
#ifndef NETMF_TARGET_BIG_ENDIAN
            serData[serOffset++] = *tmp++;
#else
            serData[serOffset++] = tmp[size];
#endif
        }
    }

    return true;    
}

static bool SerializeObject(CLR_RT_HeapBlock* pObj, CLR_UINT8* serData, CLR_INT32& serOffset, CLR_INT32& serSize )
{
    if(pObj == NULL)
    {
        return false;
    }

    switch(pObj->DataType())
    {
        case DATATYPE_OBJECT:
            if(!SerializeObject(pObj->Dereference(), serData, serOffset, serSize)) return false;
            break;
            
        case DATATYPE_CLASS:
        case DATATYPE_VALUETYPE:
            {
                CLR_RT_TypeDef_Instance cls; cls.InitializeFromIndex( pObj->ObjectCls() );
                int                    totFields = cls.CrossReference().m_totalFields;

                while(totFields-- > 0)
                {
                    if(!SerializeObject( ++pObj, serData, serOffset, serSize )) return false;
                }
            }
            break;

        case DATATYPE_SZARRAY:
            {
                CLR_RT_HeapBlock_Array* array = (CLR_RT_HeapBlock_Array*)pObj;
                int cnt = array->m_numOfElements;

                CLR_UINT8* pData;

                if(array->m_typeOfElement <= DATATYPE_LAST_NONPOINTER && cnt < 256)
                {
                    pData = (CLR_UINT8*)array->GetFirstElement();

                    while(cnt--)
                    {
                        if(!SerializeIntrinsicType( (CLR_DataType)array->m_typeOfElement, pData, serData, serOffset, serSize )) return false;

                        pData += array->m_sizeOfElement;
                    }
                }
                else
                {
                    return false;
                }
            }
            break;

        default:
            {
                CLR_UINT64 data = (CLR_UINT64)pObj->NumericByRef().u8;
                
                if(!SerializeIntrinsicType(pObj->DataType(), (CLR_UINT8*)&data, serData, serOffset, serSize)) return false;
            }
            break;               
    }

    return true;
}


HRESULT Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::SerializeParameter___STATIC__SZARRAY_U1__OBJECT( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    CLR_INT32 offset = 0, size = 0;
    CLR_RT_HeapBlock &ref = stack.PushValueAndClear();
    CLR_UINT8* pData;

    CLR_RT_HeapBlock& obj = stack.Arg0();

    if(!SerializeObject( &obj, NULL, offset, size ) || size <= 0) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance(ref, size, g_CLR_RT_WellKnownTypes.m_UInt8));

    pData = (CLR_UINT8*)ref.DereferenceArray()->GetFirstElement();

    offset = 0;

    if(!SerializeObject( &obj, pData, offset, size )) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::DeserializeParameter___STATIC__BOOLEAN__SZARRAY_U1__OBJECT( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_INT32 offset = 0;

    CLR_RT_HeapBlock_Array* array = stack.Arg0().DereferenceArray();
    CLR_RT_HeapBlock&         obj = stack.Arg1();

    if(!DeserializeObject( &obj, array->GetFirstElement(), offset, array->m_numOfElements )) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

    stack.SetResult_Boolean(true);

    TINYCLR_NOCLEANUP();
}
