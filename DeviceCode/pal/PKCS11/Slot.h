#include "cryptoki.h"

struct Slot_Cryptoki
{
public:
    static CK_RV GetInfo( CK_SLOT_INFO_PTR pSlotInfo );
    static CK_RV GetTokenInfo( CK_TOKEN_INFO_PTR pTokenInfo );
    static CK_RV GetMechanismList( CK_MECHANISM_TYPE_PTR pMechanismList, CK_ULONG_PTR pulCount );
    static CK_RV GetMechansimInfo( CK_MECHANISM_TYPE type, CK_MECHANISM_INFO_PTR pMechanismInfo );
    static CK_RV IntializeToken( CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen, CK_UTF8CHAR_PTR pLabel );
    static CK_RV OpenSession( CK_FLAGS flags, CK_VOID_PTR pApplication, CK_NOTIFY notify, Session_Cryptoki* pSession);// CK_SESSION_HANDLE_PTR phSession );
    static CK_RV CloseAllSessions();
}


