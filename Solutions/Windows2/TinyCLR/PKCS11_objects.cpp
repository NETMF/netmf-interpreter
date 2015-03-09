#include "pkcs11.h"


CK_RV PKCS11_Objects_Windows::CreateObject(Cryptoki_Session_Context* pSessionCtx, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount, CK_OBJECT_HANDLE_PTR phObject)
{
    if(EmulatorNative::GetICryptokiObjectDriver()->CreateObject((int)pSessionCtx->TokenCtx, (IntPtr)pTemplate, (int)ulCount, (int%)*phObject))
    {
        return CKR_OK;
    }

    return CKR_FUNCTION_FAILED;
}

CK_RV PKCS11_Objects_Windows::CopyObject(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount, CK_OBJECT_HANDLE_PTR phNewObject)
{
    if(EmulatorNative::GetICryptokiObjectDriver()->CopyObject((int)pSessionCtx->TokenCtx, (int)hObject, (IntPtr)pTemplate, (int)ulCount, (int%)*phNewObject))
    {
        return CKR_OK;
    }

    return CKR_FUNCTION_FAILED;
}

CK_RV PKCS11_Objects_Windows::DestroyObject(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject)
{
    if(EmulatorNative::GetICryptokiObjectDriver()->DestroyObject((int)pSessionCtx->TokenCtx, (int)hObject))
    {
        return CKR_OK;
    }

    return CKR_FUNCTION_FAILED;
}

CK_RV PKCS11_Objects_Windows::GetObjectSize(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject, CK_ULONG_PTR pulSize)
{
    if(EmulatorNative::GetICryptokiObjectDriver()->GetObjectSize((int)pSessionCtx->TokenCtx, (int)hObject, (int%)*pulSize))
    {
        return CKR_OK;
    }

    return CKR_FUNCTION_FAILED;
}

CK_RV PKCS11_Objects_Windows::GetAttributeValue(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount)
{
    if(EmulatorNative::GetICryptokiObjectDriver()->GetAttributeValue((int)pSessionCtx->TokenCtx, (int)hObject, (IntPtr)pTemplate, (int)ulCount))
    {
        return CKR_OK;
    }

    return CKR_FUNCTION_FAILED;
}

CK_RV PKCS11_Objects_Windows::SetAttributeValue(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount) 
{
    if(EmulatorNative::GetICryptokiObjectDriver()->SetAttributeValue((int)pSessionCtx->TokenCtx, (int)hObject, (IntPtr)pTemplate, (int)ulCount))
    {
        return CKR_OK;
    }

    return CKR_FUNCTION_FAILED;
}

CK_RV PKCS11_Objects_Windows::FindObjectsInit(Cryptoki_Session_Context* pSessionCtx, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount)
{
    if(EmulatorNative::GetICryptokiObjectDriver()->FindObjectsInit((int)pSessionCtx->TokenCtx, (IntPtr)pTemplate, (int)ulCount))
    {
        return CKR_OK;
    }

    return CKR_FUNCTION_FAILED;
}

CK_RV PKCS11_Objects_Windows::FindObjects(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE_PTR phObjects, CK_ULONG ulMaxCount, CK_ULONG_PTR pulObjectCount)
{
    if(EmulatorNative::GetICryptokiObjectDriver()->FindObjects((int)pSessionCtx->TokenCtx, (IntPtr)phObjects, (int)ulMaxCount, (int%)*pulObjectCount))
    {
        return CKR_OK;
    }
    
    return CKR_FUNCTION_FAILED;
}

CK_RV PKCS11_Objects_Windows::FindObjectsFinal(Cryptoki_Session_Context* pSessionCtx)
{
    if(EmulatorNative::GetICryptokiObjectDriver()->FindObjectsFinal((int)pSessionCtx->TokenCtx))
    {
        return CKR_OK;
    }
    
    return CKR_FUNCTION_FAILED;
}

