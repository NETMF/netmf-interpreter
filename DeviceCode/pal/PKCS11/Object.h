#include "cryptoki.h"

struct Object_Cryptoki
{
private:
    CK_OBJECT_HANDLE m_handle;
    
public:
    CK_RV Copy(CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount, Object_Cryptoki* pObject);
    CK_RV Destroy();
    CK_RV GetSize(CK_ULONG_PTR pulSize);
    CK_RV GetAttributeValue(CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount);
    CK_RV SetAttributeValue(CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount);    
}

