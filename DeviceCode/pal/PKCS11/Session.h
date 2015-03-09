#include "cryptoki.h"


struct Session_Cryptoki
{
private:
    CK_SESSION_HANDLE_PTR m_pHandle;
    
    //CK_ATTRIBUTE_PTR* m_pTemplate;
    //CK_ULONG          m_ulCount;

public:
    CK_RV GetInfo(CK_SESSION_INFO_PTR pInfo);
    //CK_RV GetOperationState(CK_BYTE_PTR pOperationState, CK_ULONG_PTR pulOperationStateLen);
    //CK_RV SetOperationState(CK_BYTE_PTR pOperationState, CK_ULONG ulOperationStateLen, CK_OBJECT_HANDLE hEncryptionKey, CK_OBJECT_HANDLE hAuthenticationKey);

    // DO WE REALLY NEED DUAL CRYPTO
    //CK_RV DigestEncryptInit(Digest_Cryptoki* pDigest, Encrypt_Cryptoki* pEnc, DualPurpose_Cryptoki* pDualPurpose);
    //CK_RV SignEncryptInit(Signature_Cryptoki* pEnc, Encrypt_Cryptoki* pEnc, DualPurpose_Cryptoki* pDualPurpose);

    CK_RV Close();
}

