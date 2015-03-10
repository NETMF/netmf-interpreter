#include <PAL\PKCS11\CryptokiPAL.h>

extern CK_SLOT_INFO  g_OpenSSL_SlotInfo;
extern CryptokiToken g_OpenSSL_Token;


CryptokiSlot  g_CryptokiSlots[] = 
{
    {
        &g_OpenSSL_SlotInfo,
        &g_OpenSSL_Token,
    },
};

const UINT32 g_CryptokiSlotCount = ARRAYSIZE(g_CryptokiSlots);


