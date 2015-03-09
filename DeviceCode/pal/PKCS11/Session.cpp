#include "CryptokiPAL.h"

// TODO: Find better way to handle session handle index (random, make sure no duplicates, etc)
static CK_SESSION_HANDLE  s_NextSessionHandle = 1;

static CryptokiSession g_CryptokiSessions[NETMF_CRYPTOKI_MAX_SESSION];
static UINT32 g_CryptokiSessionCount = 0;

void Cryptoki_InitializeSession()
{
    memset(g_CryptokiSessions, 0, sizeof(g_CryptokiSessions));
}

CK_RV Cryptoki_GetSlotIDFromSession(CK_SESSION_HANDLE session, CK_SLOT_ID_PTR pSlotID, CryptokiSession** ppSession)
{
    int sessionIdx;

    if(ppSession != NULL)
    {
        *ppSession = NULL;
    }
    
    if(!g_isCryptokiInitialized ) return CKR_CRYPTOKI_NOT_INITIALIZED;
    if(pSlotID == NULL          ) return CKR_ARGUMENTS_BAD;
    
    // use the hash value as the first entry
    sessionIdx = session % NETMF_CRYPTOKI_MAX_SESSION;

    // iterate through each item trying to find an open spot
    for(int last = (sessionIdx == 0 ? NETMF_CRYPTOKI_MAX_SESSION - 1 : sessionIdx - 1);
        sessionIdx != last && g_CryptokiSessions[sessionIdx].SessionHandle != session; 
        sessionIdx++)
    {
        if(sessionIdx >= NETMF_CRYPTOKI_MAX_SESSION) sessionIdx = 0;
    }

    if(g_CryptokiSessions[sessionIdx].SessionHandle != session) return CKR_SESSION_HANDLE_INVALID;
    if(g_CryptokiSessions[sessionIdx].Token == NULL           ) return CKR_TOKEN_NOT_PRESENT;

    *pSlotID = g_CryptokiSessions[sessionIdx].SlotID;

    if(ppSession != NULL)
    {
        *ppSession = &g_CryptokiSessions[sessionIdx];
    }

    return CKR_OK;
}


CK_DEFINE_FUNCTION(CK_RV, C_OpenSession)
(
  CK_SLOT_ID            slotID,        /* the slot's ID */
  CK_FLAGS              flags,         /* from CK_SESSION_INFO */
  CK_VOID_PTR           pApplication,  /* passed to callback */
  CK_NOTIFY             Notify,        /* callback function */
  CK_SESSION_HANDLE_PTR phSession      /* gets session handle */
)
{
    UINT32 sessionIdx;
    CryptokiSession* pSession;
    
    if(!g_isCryptokiInitialized                            ) return CKR_CRYPTOKI_NOT_INITIALIZED;
    if(slotID    >= g_CryptokiSlotCount                    ) return CKR_SLOT_ID_INVALID;
    if(phSession == NULL                                   ) return CKR_ARGUMENTS_BAD;
    if(NETMF_CRYPTOKI_MAX_SESSION == g_CryptokiSessionCount) return CKR_SESSION_COUNT;
    if((flags & CKF_SERIAL_SESSION) == 0                   ) return CKR_SESSION_PARALLEL_NOT_SUPPORTED;
    if(g_CryptokiSlots[slotID].Token->Mechanisms == NULL   ) return CKR_TOKEN_NOT_PRESENT;

    if(flags & CKF_RW_SESSION)
    {
        if(g_CryptokiSlots[slotID].Token->TokenInfo.ulRwSessionCount >= g_CryptokiSlots[slotID].Token->TokenInfo.ulMaxRwSessionCount) return CKR_SESSION_COUNT;        
    }
    else
    {
        if(g_CryptokiSlots[slotID].Token->TokenInfo.ulSessionCount   >= g_CryptokiSlots[slotID].Token->TokenInfo.ulMaxSessionCount  ) return CKR_SESSION_COUNT;
    }

    // use the hash value as the first entry
    sessionIdx = s_NextSessionHandle % NETMF_CRYPTOKI_MAX_SESSION;

    // iterate through each item trying to find an open spot
    for(int last = (sessionIdx == 0 ? NETMF_CRYPTOKI_MAX_SESSION - 1 : sessionIdx - 1);
        sessionIdx != last && g_CryptokiSessions[sessionIdx].SessionHandle != 0; 
        sessionIdx++)
    {
        if(sessionIdx >= NETMF_CRYPTOKI_MAX_SESSION) sessionIdx = 0;
    }

    pSession = &g_CryptokiSessions[sessionIdx];

    if(pSession->SessionHandle != 0)
    {
        ASSERT(NETMF_CRYPTOKI_MAX_SESSION==g_CryptokiSessionCount); // we shouldn't get here: g_CryptokiSessionCount must not be up to date or we didn't clear out the session data after closing the session
        return CKR_SESSION_COUNT;
    }

    if(g_CryptokiSlots[slotID].Token->SessionMgmt != NULL &&
       g_CryptokiSlots[slotID].Token->SessionMgmt->OpenSession != NULL)
    {
        CK_RV ret = g_CryptokiSlots[slotID].Token->SessionMgmt->OpenSession( &pSession->Context, (flags & CKF_RW_SESSION) != 0 );

        if(CKR_OK != ret && CKR_FUNCTION_NOT_SUPPORTED != ret) return ret;
    }

#ifdef _DEBUG
    memset(pSession->SessionObjects, 0xFF, sizeof(pSession->SessionObjects));
    pSession->SessionObjectCount = 0;
#endif

    g_CryptokiSlots[slotID].Token->TokenInfo.ulSessionCount++;

    if(0 != (flags & CKF_RW_SESSION))
    {
        g_CryptokiSlots[slotID].Token->TokenInfo.ulRwSessionCount++;
    }

    pSession->SessionHandle = s_NextSessionHandle++;
    pSession->ApplicationData = pApplication;
    pSession->Notify = Notify;
    pSession->SlotID = slotID;
    pSession->State  = (flags & CKF_RW_SESSION) ? RWPublic : ROPublic;
    pSession->IsLoginContext = FALSE;

    if(s_NextSessionHandle == CK_SESSION_HANDLE_INVALID)
    {
        s_NextSessionHandle = 0;
    }

    g_CryptokiSessionCount++;

    pSession->Token = g_CryptokiSlots[slotID].Token;
    
    *phSession = pSession->SessionHandle;

    return CKR_OK;
}

CK_DEFINE_FUNCTION(CK_RV, C_InitPIN)
(
    CK_SESSION_HANDLE hSession,
    CK_UTF8CHAR_PTR pPin,
    CK_ULONG ulPinLen
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret != CKR_OK) return ret;

    if(pSession->State != RWSecurityOfficer)
    {
        return CKR_USER_NOT_LOGGED_IN;
    }
    
    if(pSession->Token->SessionMgmt != NULL && pSession->Token->SessionMgmt->InitPin != NULL)
    {
        ret = pSession->Token->SessionMgmt->InitPin(&pSession->Context, pPin, ulPinLen);
    }
    else
    {
        ret = CKR_FUNCTION_NOT_SUPPORTED;
    }
    
    return ret;
}

CK_DEFINE_FUNCTION(CK_RV, C_SetPIN)
(
  CK_SESSION_HANDLE hSession,  /* the session's handle */
  CK_UTF8CHAR_PTR   pOldPin,   /* the old PIN */
  CK_ULONG          ulOldLen,  /* length of the old PIN */
  CK_UTF8CHAR_PTR   pNewPin,   /* the new PIN */
  CK_ULONG          ulNewLen   /* length of the new PIN */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret != CKR_OK) return ret;

    switch(pSession->State)
    {
        case RWPublic:
        case RWUser:
        case RWSecurityOfficer:
            break;

        default:
            return CKR_SESSION_READ_ONLY;
    }

    if(0 == (pSession->Token->TokenInfo.flags & CKF_PROTECTED_AUTHENTICATION_PATH))
    {
        if(pOldPin == NULL || pNewPin == NULL) return CKR_ARGUMENTS_BAD;
    }
    
    if(pSession->Token->SessionMgmt != NULL && pSession->Token->SessionMgmt->SetPin != NULL)
    {
        ret = pSession->Token->SessionMgmt->SetPin(&pSession->Context, pOldPin, ulOldLen, pNewPin, ulNewLen);
    }
    else
    {
        ret = CKR_FUNCTION_NOT_SUPPORTED;
    }
    
    return ret;
}

static CK_RV CloseSession(CryptokiSession* pSession, CK_SLOT_ID slotID)
{
    CK_RV retVal = CKR_OK;

    if(pSession == NULL || pSession->Token == NULL) return CKR_SESSION_HANDLE_INVALID;

#ifdef _DEBUG
    if(pSession->Token->ObjectMgmt != NULL && 
       pSession->Token->ObjectMgmt->DestroyObject != NULL)
    {
        int i;

        if(pSession->SessionObjectCount != 0)
        {
            debug_printf("!!!!!!!!! LEAKED CRYPTO RESOURCE !!!!!!!!!!!!");
        }

        for(i=0; i<ARRAYSIZE(pSession->SessionObjects); i++)
        {
            if(pSession->SessionObjectCount == 0) break;
            
            if(pSession->SessionObjects[i] != CK_OBJECT_HANDLE_INVALID)
            {
                if(pSession->Token->ObjectMgmt != NULL && pSession->Token->ObjectMgmt->DestroyObject)
                {
                    pSession->Token->ObjectMgmt->DestroyObject(&pSession->Context, pSession->SessionObjects[i]);

                    pSession->RemoveObjectHandle(pSession->SessionObjects[i]);
                }
            }
        }
    }
#endif

    if(pSession->Token->SessionMgmt != NULL && 
       pSession->Token->SessionMgmt->CloseSession != NULL)
    {
        retVal = pSession->Token->SessionMgmt->CloseSession(&pSession->Context);

        if(retVal == CKR_FUNCTION_NOT_SUPPORTED) retVal = CKR_OK;
    }

    pSession->Token->TokenInfo.ulSessionCount--;

    if(NETMF_CRYPTOKI_SESSION_IS_RW(pSession->State))
    {
        pSession->Token->TokenInfo.ulRwSessionCount--;
    }

    pSession->SessionHandle   = 0;
    pSession->SlotID          = 0;
    pSession->ApplicationData = NULL;
    pSession->Notify          = NULL;
    pSession->State           = ROPublic;
    pSession->Token           = NULL;
    pSession->IsLoginContext  = FALSE;
    
    g_CryptokiSessionCount--;

    return retVal;
}

/* C_CloseSession closes a session between an application and a
 * Token. */
CK_DEFINE_FUNCTION(CK_RV, C_CloseSession)
(
  CK_SESSION_HANDLE hSession  /* the session's handle */
)
{
    CryptokiSession* pSession = NULL;
    CK_SLOT_ID slotID;

    CK_RV ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret != CKR_OK)
    {
        return ret;
    }

    return CloseSession(pSession, slotID);
}


/* C_CloseAllSessions closes all sessions with a Token. */
CK_DEFINE_FUNCTION(CK_RV, C_CloseAllSessions)
(
  CK_SLOT_ID     slotID  /* the token's slot */
)
{
    if(!g_isCryptokiInitialized ) return CKR_CRYPTOKI_NOT_INITIALIZED;
    
    // iterate through each item trying to find an open spot
    for(int sessionIdx = 0; sessionIdx<NETMF_CRYPTOKI_MAX_SESSION; sessionIdx++)
    {
        CryptokiSession *pSession = &g_CryptokiSessions[sessionIdx];
        
        if(pSession->SlotID == slotID)
        {
            CloseSession(pSession, slotID);
        }
    }

    return CKR_OK;
}

CK_DEFINE_FUNCTION(CK_RV, C_GetSessionInfo)
(
  CK_SESSION_HANDLE   hSession,  /* the session's handle */
  CK_SESSION_INFO_PTR pInfo      /* receives session info */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;
    CK_ULONG devError = 0;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret != CKR_OK) return ret;
    if(pInfo == NULL) return CKR_ARGUMENTS_BAD;

    pInfo->slotID        = slotID;

    pInfo->state         = NETMF_CRYPTOKI_SESSION_STATE_CONVERT(pSession->State);
    pInfo->flags         = CKF_SERIAL_SESSION | (NETMF_CRYPTOKI_SESSION_IS_RW(pSession->State) ? CKF_RW_SESSION : 0);
    pInfo->ulDeviceError = 0;

    if(pSession->Token != NULL && pSession->Token->TokenState)
    {
        if(pSession->Token->TokenState->GetDeviceError != NULL) pSession->Token->TokenState->GetDeviceError(&devError);

        pInfo->ulDeviceError = devError;

        // if this session is not logged in and the CKU_SO or CKU_USER is logged in, then use the token wide
        // session state.
        if( !NETMF_CRYPTOKI_SESSION_IS_PUBLIC(pSession->Token->TokenWideState) && 
            NETMF_CRYPTOKI_SESSION_IS_PUBLIC(pSession->State))
        {
            // RWSecurityOfficer can not change a RO session
            if((pSession->Token->TokenWideState | ReadWriteFlag) != RWSecurityOfficer ||
                NETMF_CRYPTOKI_SESSION_IS_RW(pSession->State))
            {
                pInfo->state = NETMF_CRYPTOKI_SESSION_STATE_CONVERT_TOKEN(pSession->Token->TokenWideState, pSession->State);
            }
        }
    }

    return CKR_OK;
}

/* C_GetOperationState obtains the state of the cryptographic operation
 * in a session. */
CK_DEFINE_FUNCTION(CK_RV, C_GetOperationState)
(
  CK_SESSION_HANDLE hSession,             /* session's handle */
  CK_BYTE_PTR       pOperationState,      /* gets state */
  CK_ULONG_PTR      pulOperationStateLen  /* gets state length */
)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_SetOperationState restores the state of the cryptographic
 * operation in a session. */
CK_DEFINE_FUNCTION(CK_RV, C_SetOperationState)
(
  CK_SESSION_HANDLE hSession,            /* session's handle */
  CK_BYTE_PTR      pOperationState,      /* holds state */
  CK_ULONG         ulOperationStateLen,  /* holds state length */
  CK_OBJECT_HANDLE hEncryptionKey,       /* en/decryption key */
  CK_OBJECT_HANDLE hAuthenticationKey    /* sign/verify key */
)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

/* C_Login logs a user into a token. */
CK_DEFINE_FUNCTION(CK_RV, C_Login)
(
  CK_SESSION_HANDLE hSession,  /* the session's handle */
  CK_USER_TYPE      userType,  /* the user type */
  CK_UTF8CHAR_PTR   pPin,      /* the user's PIN */
  CK_ULONG          ulPinLen   /* the length of the PIN */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret != CKR_OK) return ret;
    if(userType > 2 ) return CKR_USER_TYPE_INVALID;

    if(pSession->State > RWPublic || pSession->State == ROUser)             return CKR_USER_ALREADY_LOGGED_IN;
    if(pSession->Token->TokenInfo.ulSessionCount > 0 && userType == CKU_SO) return CKR_SESSION_READ_ONLY_EXISTS;
    if(pSession->Token->TokenWideState != ROPublic)                         return CKR_USER_ANOTHER_ALREADY_LOGGED_IN;

    if(pSession->Token->SessionMgmt == NULL)
    {
        if(CKR_OK == (ret = pSession->Token->SessionMgmt->Login(&pSession->Context, userType, pPin, ulPinLen)))
        {
            // use |= to maintain the r/w flag
            pSession->State = (CryptokiSessionState)((pSession->State & ReadWriteFlag) | (userType == CKU_SO ? RWSecurityOfficer : ROUser));
        }
    }
    else
    {
        // TODO: blindly accept or check PIN with encrypted pin stored somewhere?
        pSession->State = (CryptokiSessionState)((pSession->State & ReadWriteFlag) | (userType == CKU_SO ? RWSecurityOfficer : ROUser));
    }

    if(userType == CKU_CONTEXT_SPECIFIC)
    {
        pSession->IsLoginContext = TRUE;
    }
    else
    {
        // don't store read/write since that is session specific
        pSession->Token->TokenWideState = (CryptokiSessionState)(userType == CKU_SO ? ROUser + 1 : ROUser);
    }
    

    return ret;
}

/* C_Logout logs a user out from a token. */
CK_DEFINE_FUNCTION(CK_RV, C_Logout)
(
  CK_SESSION_HANDLE hSession  /* the session's handle */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret != CKR_OK) return ret;

    if(pSession->State <= RWPublic && pSession->State != ROUser) return CKR_USER_NOT_LOGGED_IN;

    if(pSession->Token->SessionMgmt == NULL)
    {
        ret = pSession->Token->SessionMgmt->Logout(&pSession->Context);
    }

    // back to public session (ro or rw)
    pSession->State = (CryptokiSessionState)((pSession->State & ReadWriteFlag) | ROPublic);

    if(pSession->IsLoginContext)
    {
        pSession->IsLoginContext = FALSE;
    }
    else
    {
        pSession->Token->TokenWideState = ROPublic;
    }

    return ret;
}

