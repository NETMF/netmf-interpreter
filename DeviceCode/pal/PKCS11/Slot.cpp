#include "CryptokiPAL.h"

static PAL_CRYPTOKI_SLOT_STATE s_Hal_Slot_State[NETMF_CRYPTOKI_MAX_SLOTS];

void Cryptoki_InitializeSlots()
{
    memset(s_Hal_Slot_State, 0, sizeof(s_Hal_Slot_State));
}

void Cryptoki_SetSlotEvent(CK_SLOT_ID slotID, CK_ULONG evt)
{
    if(slotID < NETMF_CRYPTOKI_MAX_SLOTS)
    {
        if(s_Hal_Slot_State[slotID].SlotEventCallback != NULL)
        {
            s_Hal_Slot_State[slotID].SlotEventCallback(s_Hal_Slot_State[slotID].Context, evt);
        }
    }
}


BOOL Cryptoki_ConnectSlotEventSink( int slotID, void* pContext, PFNSlotEvent pfnEvtHandler )
{
    if((slotID < 0) || (slotID >= NETMF_CRYPTOKI_MAX_SLOTS)) return FALSE;

    {
        GLOBAL_LOCK(irq);

        PAL_CRYPTOKI_SLOT_STATE& State = s_Hal_Slot_State[slotID];

        State.SlotEventCallback = pfnEvtHandler;
        State.Context           = pContext;
    }

    return TRUE;
}

CK_SLOT_ID Cryptoki_FindSlot(LPCSTR szProvider, CK_MECHANISM_TYPE_PTR mechs, INT32 mechCnt)
{
    CK_SLOT_ID slotList[10];
    CK_ULONG   slotCnt = ARRAYSIZE(slotList);
    UINT32 lenSP;
    CK_SLOT_ID slotID = CK_SLOT_ID_INVALID;
    UINT32      i;

    lenSP = szProvider == NULL ? 0 : hal_strlen_s(szProvider);
    
    if(CKR_OK == C_GetSlotList(TRUE, slotList, &slotCnt))
    {
        for(i=0; i<slotCnt; i++)
        {
            CK_TOKEN_INFO info;

            if(mechCnt == 0)
            {
                slotID = slotList[i];
                break;
            }
            else if(CKR_OK == C_GetTokenInfo(slotList[i], &info))
            {
                INT32 len2 = hal_strlen_s((const char*)info.label);
            
                if((lenSP == 0) || ((lenSP == len2) && (hal_strncmp_s(szProvider, (const char*)info.label, lenSP) == 0)))
                {
                    if(mechCnt > 0)
                    {
                        CK_MECHANISM_TYPE tokenMechs[100];
                        CK_ULONG cnt = 100;
                            
                        if(CKR_OK == C_GetMechanismList(slotList[i], tokenMechs, &cnt))
                        {
                            bool slotIsOK = false;
                            CK_ULONG t, s;

                            for(s = 0; (INT32)s < mechCnt; s++)
                            {
                                slotIsOK = false;

                                for(t = 0; t < cnt; t++)
                                {
                                    if(mechs[s] == tokenMechs[t])
                                    {
                                        slotIsOK = true;
                                        break;
                                    }
                                }

                                if(!slotIsOK) break;
                            }
                            
                            if(slotIsOK)
                            {
                                slotID = slotList[i];
                                break;
                            }
                        }
                    }
                    else 
                    {
                        slotID = slotList[i];
                        break;
                    }
                }
            }
        }
    }

    return slotID;
}


static CK_RV GetTokenFromSlotID(CK_SLOT_ID slotID, CryptokiToken** ppToken)
{
    CryptokiSlot* pSlot;
    
    ASSERT(ppToken != NULL);

    *ppToken = NULL;
    
    if(slotID >= g_CryptokiSlotCount) return CKR_SLOT_ID_INVALID;

    pSlot    = &g_CryptokiSlots[slotID];
    *ppToken = pSlot->Token;
    
    if((pSlot->SlotInfo->flags & CKF_TOKEN_PRESENT) == 0 ||  *ppToken == NULL)  return CKR_TOKEN_NOT_PRESENT;

    return CKR_OK;    
}

    
CK_DEFINE_FUNCTION(CK_RV, C_GetSlotList)
(
  CK_BBOOL       tokenPresent,  /* only slots with tokens? */
  CK_SLOT_ID_PTR pSlotList,     /* receives array of slot IDs */
  CK_ULONG_PTR   pulCount       /* receives number of slots */
)
{
    CK_ULONG idx = 0;
    CK_RV ret = CKR_OK;

    if(pulCount == NULL) return CKR_ARGUMENTS_BAD;

    for(CK_ULONG i=0; i<g_CryptokiSlotCount; i++)
    {
        if(tokenPresent)
        {
            if(g_CryptokiSlots[i].SlotInfo->flags & CKF_TOKEN_PRESENT)
            {
                if(idx<*pulCount && pSlotList != NULL)
                {
                    pSlotList[idx] = i;
                }

                idx++;
            }
        }
        else
        {
            if(idx<*pulCount && pSlotList != NULL)
            {
                pSlotList[idx] = i;
            }
            idx++;
        }
    }

    if(pSlotList != NULL && idx > *pulCount) ret = CKR_BUFFER_TOO_SMALL;

    *pulCount = idx;    

    return ret;
}

CK_DEFINE_FUNCTION(CK_RV, C_GetSlotInfo)
(
  CK_SLOT_ID       slotID,  /* the ID of the slot */
  CK_SLOT_INFO_PTR pInfo    /* receives the slot information */
)
{
    if(slotID >= g_CryptokiSlotCount) return CKR_SLOT_ID_INVALID;
    if(pInfo  == NULL               ) return CKR_ARGUMENTS_BAD;

    memcpy(pInfo, g_CryptokiSlots[slotID].SlotInfo, sizeof(CK_SLOT_INFO));

    return CKR_OK;
}

CK_DEFINE_FUNCTION(CK_RV, C_GetTokenInfo)
(
  CK_SLOT_ID        slotID,  /* ID of the token's slot */
  CK_TOKEN_INFO_PTR pInfo    /* receives the token information */
)
{
    CK_RV ret;
    CryptokiToken* pToken;
    
    if(pInfo == NULL) return CKR_ARGUMENTS_BAD;

    ret = GetTokenFromSlotID(slotID, &pToken);

    if(ret == CKR_OK && pToken != NULL)
    {
        memcpy(pInfo, &pToken->TokenInfo, sizeof(CK_TOKEN_INFO));
    }

    return ret;
}

CK_DEFINE_FUNCTION(CK_RV, Cryptoki_GetTokenMaxProcessingBytes)
(
  CK_SLOT_ID   slotID,          /* ID of token's slot */
  CK_ULONG_PTR pMaxProcessingBytes
)
{
    CK_RV ret;
    CryptokiToken* pToken;

    if(pMaxProcessingBytes == NULL) return CKR_ARGUMENTS_BAD;
    
    ret = GetTokenFromSlotID(slotID, &pToken);

    if(ret == CKR_OK && pToken != NULL)
    {
        *pMaxProcessingBytes = pToken->MaxProcessingBytes;
    }

    return ret;
    
}


CK_DEFINE_FUNCTION(CK_RV, C_GetMechanismList)
(
  CK_SLOT_ID            slotID,          /* ID of token's slot */
  CK_MECHANISM_TYPE_PTR pMechanismList,  /* gets mech. array */
  CK_ULONG_PTR          pulCount         /* gets # of mechs. */
)
{
    CK_RV ret;
    CryptokiToken* pToken;
    
    if(pulCount == NULL) return CKR_ARGUMENTS_BAD;

    ret = GetTokenFromSlotID(slotID, &pToken);

    if(ret != CKR_OK) return ret;
        
    if(pMechanismList  == NULL)
    {
        *pulCount = pToken->MechanismCount;
    }
    else
    {
        if(*pulCount < pToken->MechanismCount)
        {
            return CKR_BUFFER_TOO_SMALL;
        }

        for(CK_ULONG i=0; i<pToken->MechanismCount; i++)
        {
            pMechanismList[i] = pToken->Mechanisms[i].Mechanism;
        }

        *pulCount = pToken->MechanismCount;
    }

    return CKR_OK;
}

CK_DEFINE_FUNCTION(CK_RV, C_GetMechanismInfo)
(
  CK_SLOT_ID            slotID,  /* ID of the token's slot */
  CK_MECHANISM_TYPE     type,    /* type of mechanism */
  CK_MECHANISM_INFO_PTR pInfo    /* receives mechanism info */
)
{
    CryptokiToken* pToken;
    CK_RV ret;
    
    if(pInfo  == NULL) return CKR_ARGUMENTS_BAD;

    ret = GetTokenFromSlotID(slotID, &pToken);

    if(ret != CKR_OK) return ret;

    ret = CKR_MECHANISM_INVALID;

    for(UINT32 i=0; i<pToken->MechanismCount; i++)
    {
        if(pToken->Mechanisms[i].Mechanism == type)
        {
            memcpy(pInfo, &pToken->Mechanisms[i].MechanismInfo, sizeof(*pInfo));
            ret = CKR_OK;
            break;
        }
    }

    return ret;
}


CK_DEFINE_FUNCTION(CK_RV, C_InitToken)
(
    CK_SLOT_ID      slotID,
    CK_UTF8CHAR_PTR pPin,
    CK_ULONG        ulPinLen,
    CK_UTF8CHAR_PTR pLabel
)
{
    // pLabel is required to be exactly 32 bytes long per spec
    //const int c_REQUIRED_LABEL_LEN = 32;
    CK_RV ret;
    CryptokiToken *pToken;
    
    if(pPin == NULL || pLabel == NULL) return CKR_ARGUMENTS_BAD;

    ret = GetTokenFromSlotID(slotID, &pToken);

    if(ret != CKR_OK) return ret;

    if(pToken->TokenInfo.ulSessionCount > 0) return CKR_SESSION_EXISTS;

    // label is required to be 32 bytes wth ' ' padding and NOT null terminated
    /*
    for(i=0; i<c_REQUIRED_LABEL_LEN; i++)
    {
        if(pLabel[i] == 0)
        {
            return CKR_ARGUMENTS_BAD;
        }
    }*/

    // TODO: DESTROY ALL OBJECTS ON TOKEN

    if(pToken->TokenState != NULL && pToken->TokenState->InitializeToken != NULL)
    {
        return pToken->TokenState->InitializeToken(pPin, ulPinLen, pLabel, hal_strlen_s((LPCSTR)pLabel)); //c_REQUIRED_LABEL_LEN); 
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}


CK_DEFINE_FUNCTION(CK_RV, C_WaitForSlotEvent)
(
  CK_FLAGS flags,        /* blocking/nonblocking flag */
  CK_SLOT_ID_PTR pSlot,  /* location that receives the slot ID */
  CK_VOID_PTR pRserved   /* reserved.  Should be NULL_PTR */
)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}



