#include "pkcs11_stub.h"

CK_RV PKCS11_Objects_stub::CreateObject(Cryptoki_Session_Context* pSessionCtx, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount, CK_OBJECT_HANDLE_PTR phObject)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Objects_stub::CopyObject(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount, CK_OBJECT_HANDLE_PTR phNewObject)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Objects_stub::DestroyObject(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Objects_stub::GetObjectSize(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject, CK_ULONG_PTR pulSize)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Objects_stub::GetAttributeValue(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Objects_stub::SetAttributeValue(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount) 
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Objects_stub::FindObjectsInit(Cryptoki_Session_Context* pSessionCtx, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Objects_stub::FindObjects(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE_PTR phObjects, CK_ULONG ulMaxCount, CK_ULONG_PTR pulObjectCount)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Objects_stub::FindObjectsFinal(Cryptoki_Session_Context* pSessionCtx)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

