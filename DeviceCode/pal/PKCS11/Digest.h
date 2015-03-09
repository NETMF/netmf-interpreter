#include "cryptoki.h"

struct Digest_Cryptoki
{
private:
    
    

public:
    CK_RV Digest(CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pDigest, CK_ULONG_PTR pulDigestLen);
    CK_RV Update(CK_BYTE_PTR pData, CK_ULONG ulDataLen);
    CK_RV DigestKey(CK_OBJECT_HANDLE hKey);
    CK_RV Final(CK_BYTE_PTR pDigest, CK_ULONG_PTR pulDigestLen);
}


