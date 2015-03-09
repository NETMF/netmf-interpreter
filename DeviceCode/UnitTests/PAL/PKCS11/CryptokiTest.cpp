#include <PKCS11\CryptokiPAL.h>

#define TEST_RESULT_OK    0
#define TEST_RESULT_SKIP -1

typedef INT32 TEST_RESULT;

#define TEST_SUITE_START() \
    BOOL res = TRUE; \
    TEST_RESULT tmp

#define TEST_SUITE_END() \
    return res

#define RUN_TEST(x) \
    tmp = x(); \
    if(tmp > TEST_RESULT_OK) { debug_printf("FAIL - (line %d) - %s\r\n", tmp, #x); res = FALSE; } \
    else debug_printf("%s - %s\r\n", tmp == TEST_RESULT_SKIP ? "SKIP" : "PASS", #x) 
    

#define TEST_CHECK(x, y) \
    t1 = (UINT32)(x); \
    t2 = (UINT32)(y); \
    if((t1) != (t2)) { if((t2) == CKR_FUNCTION_NOT_SUPPORTED){ debug_printf("   SkipLine=%d\r\n", __LINE__); res = TEST_RESULT_SKIP; } else { debug_printf("   errCode=0x%08X\r\n", t2); res = __LINE__; } goto END_TEST; }

#define TEST_START_NOINIT() \
    UINT32 t1, t2; \
    TEST_RESULT res = TEST_RESULT_OK

#define TEST_START() \
    UINT32 t1, t2; \
    TEST_RESULT res = TEST_RESULT_OK; \
    TEST_CHECK(CKR_OK, C_Initialize(NULL))

#define TEST_CLEANUP() \
END_TEST: 

#define TEST_END_CLEANUP() \
    if(CKR_OK != C_Finalize(NULL)) res = __LINE__; \
    return res


#define TEST_END() \
END_TEST: \
    if(CKR_OK != C_Finalize(NULL)) res = __LINE__; \
    return res

#define TEST_END_NOCLEANUP() \
END_TEST: \
    return res

static TEST_RESULT C_Initialize_ARGS_Not_supported()
{
    CK_C_INITIALIZE_ARGS args;
    TEST_START_NOINIT();

    // WE ARE NOT USING INIT ARGS AT THIS POINT - NO THREADING TO WORRY ABOUT
    TEST_CHECK(CKR_ARGUMENTS_BAD, C_Initialize(&args));

    TEST_END_NOCLEANUP();
}

static TEST_RESULT C_Initialize_NULL_PTR_ARGS_Normal_Usage()
{
    TEST_START_NOINIT();

    TEST_CHECK(CKR_OK, C_Initialize(NULL));

    TEST_CHECK(CKR_OK, C_Finalize(NULL));

    TEST_CHECK(CKR_OK, C_Initialize(NULL));

    TEST_END();
}

static TEST_RESULT C_Initialize_multiple_calls_Should_FAIL()
{
    TEST_START_NOINIT();
    
    TEST_CHECK(CKR_OK, C_Initialize(NULL));

    TEST_CHECK(CKR_CRYPTOKI_ALREADY_INITIALIZED, C_Initialize(NULL));

    TEST_END();
}

static TEST_RESULT C_Finalize_without_init_should_fail()
{
    return (C_Finalize(NULL) == CKR_CRYPTOKI_NOT_INITIALIZED) ? TEST_RESULT_OK : __LINE__;
}

static TEST_RESULT C_Finalize_arg_other_than_null_should_fail()
{
    TEST_START_NOINIT();
    int p = 3;

    TEST_CHECK(CKR_OK, C_Initialize(NULL));
    
    TEST_CHECK(CKR_ARGUMENTS_BAD, C_Finalize((CK_VOID_PTR)&p));

    TEST_END();
}

static TEST_RESULT C_Finalize_normal_should_succeed()
{
    TEST_START();

    TEST_CHECK(CKR_OK, C_Finalize(NULL));

    TEST_CHECK(CKR_OK, C_Initialize(NULL));

    TEST_END();
}

static TEST_RESULT C_GetInfo_NULL_parameter_should_fail()
{
    TEST_START();
    
    TEST_CHECK( CKR_ARGUMENTS_BAD, C_GetInfo(NULL));

    TEST_END();
}

static TEST_RESULT C_GetInfo_normal_usage()
{
    CK_INFO info;
    TEST_START();

    TEST_CHECK( CKR_OK, C_GetInfo(&info));

    TEST_CHECK( info.cryptokiVersion.major, CRYPTOKI_VERSION_MAJOR );
    TEST_CHECK( info.cryptokiVersion.minor, CRYPTOKI_VERSION_MINOR );

    debug_printf("   Library     : %s\r\n", info.libraryDescription );
    debug_printf("   Manufacturer: %s\r\n", info.manufacturerID );
    debug_printf("   Version     : %d.%d\r\n"  , info.libraryVersion.major, info.libraryVersion.minor );

    TEST_CHECK( NETMF_CRYPTOKI_VERSION_MAJOR, info.libraryVersion.major);
    TEST_CHECK( NETMF_CRYPTOKI_VERSION_MINOR, info.libraryVersion.minor);

    TEST_END();
}

static TEST_RESULT C_GetFunctionList_null_param_should_fail()
{
    TEST_START();

    TEST_CHECK( CKR_ARGUMENTS_BAD, C_GetFunctionList(NULL));

    TEST_END();
}

static TEST_RESULT C_GetFunctionList_call_before_c_init_should_succeed()
{
    CK_FUNCTION_LIST_PTR pFunctList;
    TEST_START_NOINIT();
    
    TEST_CHECK(CKR_OK, C_GetFunctionList(&pFunctList));

    TEST_END_NOCLEANUP();
}

static TEST_RESULT C_GetFunctionList_call_after_c_init_should_succeed()
{
    CK_FUNCTION_LIST_PTR pFunctList;
    TEST_START();
    
    TEST_CHECK(CKR_OK, C_GetFunctionList(&pFunctList));

    TEST_END();
}

static TEST_RESULT C_GetFunctionList_validate_function_ptrs()
{
    CK_FUNCTION_LIST_PTR pFunctList;
    TEST_START_NOINIT();
    
    TEST_CHECK(CKR_OK, C_GetFunctionList(&pFunctList));

    TEST_CHECK( pFunctList->version.major       , CRYPTOKI_VERSION_MAJOR );
    TEST_CHECK( pFunctList->version.minor       , CRYPTOKI_VERSION_MINOR );
    TEST_CHECK( pFunctList->C_Initialize        , C_Initialize);
    TEST_CHECK( pFunctList->C_Finalize          , C_Finalize);
    TEST_CHECK( pFunctList->C_GetInfo           , C_GetInfo);
    TEST_CHECK( pFunctList->C_GetFunctionList   , C_GetFunctionList);
    TEST_CHECK( pFunctList->C_GetSlotList       , C_GetSlotList);
    TEST_CHECK( pFunctList->C_GetSlotInfo       , C_GetSlotInfo);
    TEST_CHECK( pFunctList->C_GetTokenInfo      , C_GetTokenInfo);
    TEST_CHECK( pFunctList->C_GetMechanismList  , C_GetMechanismList);
    TEST_CHECK( pFunctList->C_GetMechanismInfo  , C_GetMechanismInfo);
    TEST_CHECK( pFunctList->C_InitToken         , C_InitToken);
    TEST_CHECK( pFunctList->C_InitPIN           , C_InitPIN);
    TEST_CHECK( pFunctList->C_SetPIN            , C_SetPIN);
    TEST_CHECK( pFunctList->C_OpenSession       , C_OpenSession);
    TEST_CHECK( pFunctList->C_CloseSession      , C_CloseSession);
    TEST_CHECK( pFunctList->C_CloseAllSessions  , C_CloseAllSessions);

    TEST_END_NOCLEANUP();
}

static TEST_RESULT C_GetSlotList_NULL_count_arg_should_fail()
{
    TEST_START();

    TEST_CHECK(CKR_ARGUMENTS_BAD, C_GetSlotList(TRUE, NULL, NULL));

    TEST_END();
}

static TEST_RESULT C_GetSlotList_buffer_too_small_error()
{
    CK_ULONG ulCount, ulTotal;
    CK_SLOT_ID slots[10];
    TEST_START();

    TEST_CHECK(CKR_OK, C_GetSlotList(FALSE, NULL, &ulTotal));

    ulCount = ulTotal-1;

    TEST_CHECK(CKR_BUFFER_TOO_SMALL, C_GetSlotList(FALSE, slots, &ulCount));

    TEST_CHECK(ulCount, ulTotal);  // C_GetSlotList should still return the 
    
    TEST_END();
}

static TEST_RESULT C_GetSlotList_normal_usage_more_slots()
{
    CK_ULONG ulCount, ulTotal;
    CK_SLOT_ID slots[10];
    TEST_START();

    TEST_CHECK(CKR_OK, C_GetSlotList(FALSE, NULL, &ulTotal));

    TEST_CHECK(CKR_OK, C_GetSlotList(FALSE, slots, &ulCount));

    TEST_CHECK(ulCount, ulTotal);
    
    TEST_END();
}

// TODO: GetSlotList with null then inject token and then GetSlotList with buffer should only show the tokens called during null arg
// See spec 

static TEST_RESULT C_GetSlotInfo_null_arg_should_fail()
{
    CK_SLOT_ID slot = 0;
    
    TEST_START();

    TEST_CHECK(CKR_ARGUMENTS_BAD, C_GetSlotInfo(slot, NULL));

    TEST_END();
}

static TEST_RESULT C_GetSlotInfo_bad_slot_id_should_fail()
{
    CK_SLOT_INFO info;
    
    TEST_START();

    TEST_CHECK(CKR_SLOT_ID_INVALID, C_GetSlotInfo((CK_ULONG)-1, &info)); 

    TEST_END();
}

static TEST_RESULT C_GetSlotInfo_normal_usage()
{
    CK_SLOT_ID slots[10];
    CK_SLOT_INFO info;
    CK_ULONG ulTotal = ARRAYSIZE(slots);
    
    TEST_START();

    TEST_CHECK(CKR_OK, C_GetSlotList(FALSE, slots, &ulTotal));

    for(CK_ULONG i=0; i<ulTotal; i++)
    {
        TEST_CHECK(CKR_OK, C_GetSlotInfo(slots[i], &info));

        debug_printf("   Manu : %s\r\n", info.manufacturerID);
        debug_printf("   Desc : %s\r\n", info.slotDescription);
        debug_printf("   Flags: 0x%08x\r\n", info.flags);
        debug_printf("   FwVer: %d.%d\r\n", info.firmwareVersion.major, info.firmwareVersion.minor);
        debug_printf("   HwVer: %d.%d\r\n", info.hardwareVersion.major, info.hardwareVersion.minor);
    }    

    TEST_END();    
}

static TEST_RESULT C_GetTokenInfo_null_arg_should_fail()
{
    TEST_START();

    TEST_CHECK(CKR_ARGUMENTS_BAD, C_GetTokenInfo(0, NULL));

    TEST_END();
}

static TEST_RESULT C_GetTokenInfo_invalid_slotID_should_fail()
{
    CK_TOKEN_INFO info;
    
    TEST_START();

    TEST_CHECK(CKR_SLOT_ID_INVALID, C_GetTokenInfo((CK_ULONG)-1, &info));

    TEST_END();
}


static TEST_RESULT C_GetTokenInfo_no_token_should_fail()
{
    CK_SLOT_ID slots[10], tokens[10];
    CK_ULONG ulCount = ARRAYSIZE(slots), ulTokens = ARRAYSIZE(tokens);
    CK_TOKEN_INFO info;

    TEST_START();

    TEST_CHECK(CKR_OK, C_GetSlotList(FALSE, slots , &ulCount));
    TEST_CHECK(CKR_OK, C_GetSlotList(TRUE , tokens, &ulTokens));

    if(ulTokens < ulCount)
    {
        int slotID = -1;
        
        for(CK_ULONG i=0; i<ulTokens; i++)
        {
            if(slots[i] != tokens[i])
            {
                slotID = slots[i];
                break;
            }
        }
        if(slotID == -1)
        {
            slotID = slots[ulTokens];
        }

        TEST_CHECK(CKR_TOKEN_NOT_PRESENT, C_GetTokenInfo(slotID, &info));
    }

    TEST_END();
}

static TEST_RESULT C_GetTokenInfo_normal_usage()
{
    CK_SLOT_ID tokens[10];
    CK_ULONG ulTokens = ARRAYSIZE(tokens);
    CK_TOKEN_INFO info;

    TEST_START();

    TEST_CHECK(CKR_OK, C_GetSlotList(TRUE , tokens, &ulTokens));

    if(ulTokens > 0)
    {
        for(CK_ULONG i=0; i<ulTokens; i++)
        {
            TEST_CHECK(CKR_OK, C_GetTokenInfo(tokens[i], &info));

            debug_printf("   Label  : %s\r\n", info.label);
            debug_printf("   Manu   : %s\r\n", info.manufacturerID);
            debug_printf("   Model  : %s\r\n", info.model);
            debug_printf("   S/N    : %s\r\n", info.serialNumber);
            debug_printf("   Flags  : 0x%08x\r\n", info.flags);
            debug_printf("   Utc    : %s\r\n", info.utcTime);
            debug_printf("   FwVer  : %d.%d\r\n", info.firmwareVersion.major, info.firmwareVersion.minor);
            debug_printf("   HwVer  : %d.%d\r\n", info.hardwareVersion.major, info.hardwareVersion.minor);
            debug_printf("   MaxSess: %d\r\n", info.ulMaxSessionCount);  
            debug_printf("   SessCnt: %d\r\n", info.ulSessionCount);     
            debug_printf("   RwMxCnt: %d\r\n", info.ulMaxRwSessionCount);
            debug_printf("   RwSCnt : %d\r\n", info.ulRwSessionCount);   
            debug_printf("   MaxPinL: %d\r\n", info.ulMaxPinLen);        
            debug_printf("   MinPinL: %d\r\n", info.ulMinPinLen);        
            debug_printf("   TPubMem: %d\r\n", info.ulTotalPublicMemory);
            debug_printf("   FPubMem: %d\r\n", info.ulFreePublicMemory); 
            debug_printf("   TPriMem: %d\r\n", info.ulTotalPrivateMemory);
            debug_printf("   FPriMem: %d\r\n", info.ulFreePrivateMemory);
        }        
    }

    TEST_END();
}

static TEST_RESULT C_GetMechanismList_null_arg_should_fail()
{
    TEST_START();

    TEST_CHECK(CKR_ARGUMENTS_BAD, C_GetMechanismList(0, NULL, NULL));

    TEST_END();
}

static TEST_RESULT C_GetMechanismList_invalid_slotID_should_fail()
{
    CK_MECHANISM_TYPE type;
    CK_ULONG ulCount = 1;
    
    TEST_START();

    TEST_CHECK(CKR_SLOT_ID_INVALID, C_GetMechanismList((CK_ULONG)-1, &type, &ulCount));

    TEST_END();
}

static TEST_RESULT C_GetMechanismList_buffer_too_small_should_fail()
{
    CK_MECHANISM_TYPE type;
    CK_ULONG ulCount = 1;
    
    TEST_START();

    TEST_CHECK(CKR_BUFFER_TOO_SMALL, C_GetMechanismList(0, &type, &ulCount));

    TEST_END();
}

static TEST_RESULT C_GetMechanismList_normal_usage()
{
    CK_MECHANISM_TYPE type[10];
    CK_ULONG ulCount = ARRAYSIZE(type);
    
    TEST_START();

    TEST_CHECK(CKR_OK, C_GetMechanismList(0, NULL, &ulCount));

    TEST_CHECK(CKR_OK, C_GetMechanismList(0, type, &ulCount));

    debug_printf( "   mechanisms:\r\n");
    for(CK_ULONG i=0; i<ulCount; i++)
    {
        debug_printf( "   0x%08x\r\n", type[i]);
    }

    TEST_END();
}

static TEST_RESULT C_GetMechanismInfo_null_arg_should_fail()
{
    TEST_START();

    TEST_CHECK(CKR_ARGUMENTS_BAD, C_GetMechanismInfo(0, CKM_AES_CBC, NULL));

    TEST_END();
}

static TEST_RESULT C_GetMechanismInfo_invalid_slotID_should_fail()
{   
    CK_MECHANISM_INFO info;
    TEST_START();

    TEST_CHECK(CKR_SLOT_ID_INVALID, C_GetMechanismInfo(-1, CKM_AES_CBC, &info));

    TEST_END();
}

static TEST_RESULT C_GetMechanismInfo_invalid_mech_should_fail()
{
    CK_MECHANISM_INFO info;
    TEST_START();

    TEST_CHECK(CKR_MECHANISM_INVALID, C_GetMechanismInfo(0, -1, &info));
    TEST_CHECK(CKR_MECHANISM_INVALID, C_GetMechanismInfo(0, CKM_BATON_COUNTER, &info));

    TEST_END();
}

static TEST_RESULT C_GetMechanismInfo_normal_usage()
{
    CK_MECHANISM_INFO info;
    CK_MECHANISM_TYPE type[10];
    CK_ULONG ulCount = ARRAYSIZE(type);
    
    TEST_START();

    TEST_CHECK(CKR_OK, C_GetMechanismList(0, type, &ulCount));

    for(CK_ULONG i=0; i<ulCount; i++)
    {
        debug_printf("   Mechanism: 0x%08X\r\n", type[i]);
        
        TEST_CHECK(CKR_OK, C_GetMechanismInfo(0, type[i], &info));

        debug_printf("   mechFlags: 0x%08X\r\n", info.flags);
        debug_printf("   maxKeyLen: 0x%08X\r\n", info.ulMaxKeySize);
        debug_printf("   minKeyLen: 0x%08X\r\n", info.ulMinKeySize);
    }

    TEST_END();
}

static TEST_RESULT C_InitToken__invalid_slotID_should_fail()
{   
    CK_UTF8CHAR pin[] = "INITTOKENPIN";
    CK_UTF8CHAR label[33] = "MyLabel8901234567890123456789012";
    
    TEST_START();

    TEST_CHECK(CKR_SLOT_ID_INVALID, C_InitToken(-1, pin, MAXSTRLEN(pin), label));

    TEST_END();
}

static TEST_RESULT C_InitToken__null_args_should_fail()
{       
    CK_UTF8CHAR label[33] = "MyLabel8901234567890123456789012";
    TEST_START();

    TEST_CHECK(CKR_ARGUMENTS_BAD, C_InitToken(0, NULL, 1, label));
    TEST_CHECK(CKR_ARGUMENTS_BAD, C_InitToken(0, label, 1, NULL));

    TEST_END();
}

static TEST_RESULT C_InitToken__short_label_should_fail()
{       
    CK_UTF8CHAR pin[]     = "INITTOKENPIN";
    CK_UTF8CHAR label[33] = "MyLabel8901234";
    TEST_START();

    TEST_CHECK(CKR_ARGUMENTS_BAD, C_InitToken(0, pin, MAXSTRLEN(pin), label));

    TEST_END();
}

static TEST_RESULT C_InitToken__with_open_session_should_fail()
{       
    CK_UTF8CHAR pin[]     = "INITTOKENPIN";
    CK_UTF8CHAR label[33] = "MyLabel8901234567890123456789012";
    CK_SLOT_ID slotID = 0;
    CK_SESSION_HANDLE hSession;
    
    TEST_START();

    TEST_CHECK( CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK( CKR_SESSION_EXISTS, C_InitToken(slotID, pin, MAXSTRLEN(pin), label));

    TEST_CLEANUP();

    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}


static TEST_RESULT C_InitToken__init_with_wrong_pin_should_fail()
{       
    CK_UTF8CHAR pin[]        = "INITTOKENPIN";
    CK_UTF8CHAR anotherPin[] = "ANOTHERPIN";
    CK_UTF8CHAR label[33]    = "MyLabel8901234567890123456789012";
    CK_SLOT_ID slotID = 0;
    CK_SESSION_HANDLE hSession;

    TEST_START();

    TEST_CHECK( CKR_OK           , C_InitToken(slotID, pin       , MAXSTRLEN(pin       ), label));
    TEST_CHECK( CKR_PIN_INCORRECT, C_InitToken(slotID, anotherPin, MAXSTRLEN(anotherPin), label));

    TEST_END();
}

static TEST_RESULT C_InitToken__should_remove_all_objects()
{       
    CK_UTF8CHAR pin[]     = "INITTOKENPIN";
    CK_UTF8CHAR label[33] = "MyLabel8901234567890123456789012";
    CK_SLOT_ID slotID = 0;
    CK_SESSION_HANDLE hSession;
    CK_OBJECT_HANDLE hObj;
    CK_ULONG ulObjCount;

    CK_OBJECT_CLASS cls = CKO_DATA;
    CK_BYTE data[] = { 0x00, 0x01, 0x02 };
    CK_UTF8CHAR label2[] = "Value1";
    
    CK_ATTRIBUTE dataTemplate[] = 
    {
        {CKA_CLASS, &cls, sizeof(cls) },
        {CKA_LABEL, label2, MAXSTRLEN(label2)},
        {CKA_VALUE, data, sizeof(data) },
    };
    
    TEST_START();

    TEST_CHECK( CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK( CKR_OK, C_CreateObject( hSession, dataTemplate, ARRAYSIZE(dataTemplate), &hObj ));

    TEST_CHECK( CKR_OK, C_CloseSession( hSession ) );

    TEST_CHECK( CKR_OK, C_InitToken(slotID, pin, MAXSTRLEN(pin), label));

    TEST_CHECK( CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK( CKR_OK, C_FindObjectsInit( hSession, NULL, 0 ) );
    TEST_CHECK( CKR_OK, C_FindObjects( hSession, &hObj, 1, &ulObjCount ) && ulObjCount == 0 );
    TEST_CHECK( CKR_OK, C_FindObjectsFinal( hSession ) );

    TEST_CLEANUP();

    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}


static TEST_RESULT C_InitToken__normal_usage()
{       
    CK_SLOT_ID slotID     = 0;
    CK_UTF8CHAR pin[]     = "INITTOKENPIN";
    CK_UTF8CHAR label[33] = "MyLabel8901234567890123456789012";
    
    TEST_START();

    TEST_CHECK(CKR_OK, C_InitToken(0, pin, MAXSTRLEN(pin), label));

    TEST_END();
}

static TEST_RESULT C_InitPin__invalid_session_should_fail()
{   
    CK_UTF8CHAR pin[] = "MYPIN21";
    
    TEST_START();

    TEST_CHECK(CKR_SESSION_HANDLE_INVALID, C_InitPIN(-1, pin, MAXSTRLEN(pin)));

    TEST_END();
}

static TEST_RESULT C_InitPin__null_args_should_fail()
{       
    CK_UTF8CHAR pin[] = "MYPIN21";
    CK_UTF8CHAR soPin[]   = "SOPIN";
    CK_USER_TYPE user = CKU_SO;
    CK_SESSION_HANDLE hSession;
    CK_SLOT_ID slotID = 0;

    TEST_START();

    TEST_CHECK( CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    // readonly session, should also fail
    TEST_CHECK( CKR_OK, C_Login( hSession, user, soPin, MAXSTRLEN(soPin)));

    TEST_CHECK(CKR_ARGUMENTS_BAD, C_InitPIN(hSession, NULL, MAXSTRLEN(pin)));

    TEST_CLEANUP();

    C_Logout( hSession );
    
    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}

static TEST_RESULT C_InitPin__no_login_should_fail()
{       
    CK_UTF8CHAR pin[] = "MYPIN21";
    CK_SESSION_HANDLE hSession;
    CK_SLOT_ID slotID = 0;

    TEST_START();

    TEST_CHECK( CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK( CKR_USER_NOT_LOGGED_IN, C_InitPIN(hSession, pin, MAXSTRLEN(pin)));

    TEST_CLEANUP();
    
    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}

static TEST_RESULT C_InitPin__wrong_login_should_fail()
{       
    CK_UTF8CHAR pin[] = "MYPIN21";
    CK_UTF8CHAR userPin[]   = "USERPIN";
    CK_SESSION_HANDLE hSession;
    CK_SLOT_ID slotID = 0;
    CK_USER_TYPE user = CKU_USER;

    TEST_START();

    TEST_CHECK( CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    // readonly session, should also fail
    TEST_CHECK( CKR_OK, C_Login( hSession, user, userPin, MAXSTRLEN(userPin)));

    TEST_CHECK( CKR_USER_NOT_LOGGED_IN, C_InitPIN(hSession, pin, MAXSTRLEN(pin)));

    TEST_CLEANUP();
    
    C_Logout( hSession );

    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}


static TEST_RESULT C_InitPin__normal_usage()
{       
    CK_UTF8CHAR pin[]     = "MYPIN21";
    CK_UTF8CHAR soPin[]   = "SOPIN";
    CK_SESSION_HANDLE hSession;
    CK_SLOT_ID slotID = 0;
    CK_USER_TYPE user = CKU_SO;
    CK_RV ret;

    
    TEST_START();

    TEST_CHECK( CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK( CKR_OK, C_Login( hSession, user, soPin, MAXSTRLEN(soPin)));

    TEST_CHECK( CKR_OK, C_InitPIN(hSession, pin, MAXSTRLEN(pin)));

    TEST_CLEANUP();

    C_Logout( hSession );
    
    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}

static TEST_RESULT C_SetPin__invalid_session_should_fail()
{
    CK_UTF8CHAR oldPin[] = "OldPIN";
    CK_UTF8CHAR newPin[] = "NewPIN";
    
    TEST_START();

    TEST_CHECK(CKR_SESSION_HANDLE_INVALID, C_SetPIN(-1, oldPin, MAXSTRLEN(oldPin), newPin, MAXSTRLEN(newPin)));

    TEST_END();
}

static TEST_RESULT C_SetPin__null_param_should_fail()
{
    CK_UTF8CHAR oldPin[] = "OldPIN";
    CK_UTF8CHAR newPin[] = "NewPIN";
    
    CK_SESSION_HANDLE hSession;
    CK_SLOT_ID slotID = 0;
    CK_TOKEN_INFO info;

    TEST_START();

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK(CKR_OK, C_GetTokenInfo( slotID, &info ));

    if(info.flags & CKF_PROTECTED_AUTHENTICATION_PATH)
    {
        // for auth path tokens, SetPin should succeed with the pin sent externally by touch pad or otherwise
        TEST_CHECK(CKR_OK, C_SetPIN(hSession, NULL, 0, NULL, 0));
    }
    else
    {
        TEST_CHECK(CKR_ARGUMENTS_BAD, C_SetPIN(hSession, NULL, 0, newPin, MAXSTRLEN(newPin)));
        TEST_CHECK(CKR_ARGUMENTS_BAD, C_SetPIN(hSession, oldPin, MAXSTRLEN(oldPin), NULL, 0));
    }

    TEST_CLEANUP();
    
    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}

static TEST_RESULT C_SetPin__read_only_session_should_fail()
{
    CK_UTF8CHAR oldPin[] = "OldPIN";
    CK_UTF8CHAR newPin[] = "NewPIN";
    
    CK_SESSION_HANDLE hSession;
    CK_SLOT_ID slotID = 0;
    CK_TOKEN_INFO info;

    TEST_START();

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION /*| CKF_RW_SESSION*/, NULL, NULL, &hSession ));

    TEST_CHECK(CKR_OK, C_GetTokenInfo( slotID, &info ));

    TEST_CHECK(CKR_SESSION_READ_ONLY, C_SetPIN(hSession, oldPin, MAXSTRLEN(oldPin), newPin, MAXSTRLEN(newPin)));

    TEST_CLEANUP();
    
    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}

static TEST_RESULT C_SetPin__normal_usage()
{
    CK_UTF8CHAR oldPin[] = "OldPIN";
    CK_UTF8CHAR newPin[] = "NewPIN";
    
    CK_SESSION_HANDLE hSession;
    CK_SLOT_ID slotID = 0;
    CK_TOKEN_INFO info;

    TEST_START();

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK(CKR_OK, C_GetTokenInfo( slotID, &info ));

    if(info.flags & CKF_PROTECTED_AUTHENTICATION_PATH)
    {
        // for auth path tokens, SetPin should succeed with the pin sent externally by touch pad or otherwise
        TEST_CHECK(CKR_OK, C_SetPIN(hSession, NULL, 0, NULL, 0));
    }
    else
    {
        TEST_CHECK(CKR_ARGUMENTS_BAD, C_SetPIN(hSession, oldPin, MAXSTRLEN(oldPin), newPin, MAXSTRLEN(newPin)));
    }

    TEST_CLEANUP();
    
    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}


static TEST_RESULT C_GetSessionInfo__invalid_session_should_fail()
{
    CK_SESSION_HANDLE hSession = -1;
    CK_SESSION_INFO   info;

    TEST_START();

    TEST_CHECK(CKR_SESSION_HANDLE_INVALID, C_GetSessionInfo( hSession, &info ));

    TEST_END();
}

static TEST_RESULT C_GetSessionInfo__null_arg_should_fail()
{
    CK_SESSION_HANDLE hSession;
    CK_SESSION_INFO   info;
    CK_SLOT_ID slotID = 0;

    TEST_START();

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK(CKR_ARGUMENTS_BAD, C_GetSessionInfo( hSession, NULL ));

    TEST_CLEANUP();
    
    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}

static TEST_RESULT C_GetSessionInfo__normal_usage()
{
    CK_SESSION_HANDLE hSession;
    CK_SESSION_INFO   info;
    CK_SLOT_ID slotID = 0;

    TEST_START();

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK(CKR_OK, C_GetSessionInfo( hSession, &info ));

    TEST_CHECK(CKF_SERIAL_SESSION | CKF_RW_SESSION, info.flags);
    TEST_CHECK(slotID                             , info.slotID);
    TEST_CHECK(CKS_RW_PUBLIC_SESSION              , info.state);
    TEST_CHECK(0                                  , info.ulDeviceError);

    TEST_CLEANUP();
    
    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}

static TEST_RESULT C_GetOperationState__NOT_IMPLEMENTED()
{
    CK_SESSION_HANDLE hSession;
    CK_SLOT_ID slotID = 0;
    CK_BYTE data[1024];
    CK_ULONG ulDataLen;

    TEST_START();

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK(CKR_OK, C_GetOperationState( hSession, NULL, &ulDataLen ));

    if(ulDataLen < ARRAYSIZE(data))
    {
        TEST_CHECK(CKR_OK, C_GetOperationState( hSession, data, &ulDataLen ));
    }
    
    TEST_CLEANUP();
    
    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}

static TEST_RESULT C_SetOperationState__NOT_IMPLEMENTED()
{
    CK_SESSION_HANDLE hSession;
    CK_SLOT_ID slotID = 0;
    CK_BYTE data[1024];
    CK_ULONG ulDataLen;

    TEST_START();

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK(CKR_OK, C_GetOperationState( hSession, NULL, &ulDataLen ));

    if(ulDataLen < ARRAYSIZE(data))
    {
        TEST_CHECK(CKR_OK, C_GetOperationState( hSession, data, &ulDataLen ));

        // TODO: if we support set/get operation state, add code to change the current state and update the keys below
        
        TEST_CHECK(CKR_OK, C_SetOperationState( hSession, data, ulDataLen, NULL, NULL ));
    }
    
    TEST_CLEANUP();
    
    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}

static TEST_RESULT C_Login__invalid_session_should_fail()
{
    CK_UTF8CHAR soPin[]   = "SOPIN";
    CK_SESSION_HANDLE hSession = -1;
    CK_USER_TYPE user = CKU_SO;
    
    TEST_START();

    TEST_CHECK(CKR_SESSION_HANDLE_INVALID, C_Login(hSession, user, soPin, MAXSTRLEN(soPin) ));

    TEST_END();
}

static TEST_RESULT C_Login__with_user_should_change_all_sessions()
{
    CK_UTF8CHAR soPin[]   = "USERPIN";
    CK_SESSION_HANDLE hSession, hSession2;
    CK_USER_TYPE user = CKU_USER;
    CK_SLOT_ID slotID = 0;
    CK_SESSION_INFO info;

    TEST_START();

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION, NULL, NULL, &hSession2 ));

    TEST_CHECK(CKR_OK, C_Login(hSession, user, NULL, MAXSTRLEN(soPin) ));

    TEST_CHECK(CKR_OK, C_GetSessionInfo( hSession, &info ));

    TEST_CHECK(CKF_SERIAL_SESSION | CKF_RW_SESSION, info.flags);
    TEST_CHECK(slotID                             , info.slotID);
    TEST_CHECK(CKS_RW_USER_FUNCTIONS              , info.state);

    TEST_CHECK(CKR_OK, C_GetSessionInfo( hSession2, &info ));

    TEST_CHECK(CKF_SERIAL_SESSION   , info.flags);
    TEST_CHECK(slotID               , info.slotID);
    TEST_CHECK(CKS_RO_USER_FUNCTIONS, info.state);
    

    TEST_CLEANUP();

    C_Logout( hSession );

    C_CloseSession( hSession2 );
    
    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}

static TEST_RESULT C_Login__with_SO_should_change_all_sessions()
{
    CK_UTF8CHAR soPin[]   = "SOPIN";
    CK_SESSION_HANDLE hSession, hSession2;
    CK_USER_TYPE user = CKU_SO;
    CK_SLOT_ID slotID = 0;
    CK_SESSION_INFO info;

    TEST_START();

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession2 ));

    TEST_CHECK(CKR_OK, C_Login(hSession, user, NULL, MAXSTRLEN(soPin) ));

    TEST_CHECK(CKR_OK, C_GetSessionInfo( hSession, &info ));

    TEST_CHECK(CKF_SERIAL_SESSION | CKF_RW_SESSION, info.flags);
    TEST_CHECK(slotID                             , info.slotID);
    TEST_CHECK(CKS_RW_SO_FUNCTIONS                , info.state);

    TEST_CHECK(CKR_OK, C_GetSessionInfo( hSession2, &info ));

    TEST_CHECK(CKF_SERIAL_SESSION | CKF_RW_SESSION, info.flags);
    TEST_CHECK(slotID                             , info.slotID);
    TEST_CHECK(CKS_RW_SO_FUNCTIONS                , info.state);


    TEST_CLEANUP();

    C_Logout( hSession );

    C_CloseSession( hSession2 );
    
    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}

static TEST_RESULT C_Login__with_SO_should_fail_if_RO_sessions_open()
{
    CK_UTF8CHAR soPin[]   = "SOPIN";
    CK_SESSION_HANDLE hSession, hSession2;
    CK_USER_TYPE user = CKU_SO;
    CK_SLOT_ID slotID = 0;
    CK_SESSION_INFO info;

    TEST_START();

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION, NULL, NULL, &hSession2 ));

    TEST_CHECK(CKR_SESSION_READ_ONLY_EXISTS, C_Login(hSession, user, NULL, MAXSTRLEN(soPin) ));

    TEST_CLEANUP();

    C_Logout( hSession );

    C_CloseSession( hSession2 );
    
    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}

static TEST_RESULT C_Login__with_CONTEXT_SPECIFIC_should_NOT_change_all_sessions()
{
    CK_UTF8CHAR soPin[]   = "CSPIN";
    CK_SESSION_HANDLE hSession, hSession2;
    CK_USER_TYPE user = CKU_CONTEXT_SPECIFIC;
    CK_SLOT_ID slotID = 0;
    CK_SESSION_INFO info;

    TEST_START();

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION, NULL, NULL, &hSession2 ));

    TEST_CHECK(CKR_OK, C_Login(hSession, user, NULL, MAXSTRLEN(soPin) ));

    TEST_CHECK(CKR_OK, C_GetSessionInfo( hSession, &info ));

    TEST_CHECK(CKF_SERIAL_SESSION | CKF_RW_SESSION, info.flags);
    TEST_CHECK(slotID                             , info.slotID);
    TEST_CHECK(CKS_RW_USER_FUNCTIONS              , info.state);

    TEST_CHECK(CKR_OK, C_GetSessionInfo( hSession2, &info ));

    TEST_CHECK(CKF_SERIAL_SESSION   , info.flags);
    TEST_CHECK(slotID               , info.slotID);
    TEST_CHECK(CKS_RO_PUBLIC_SESSION, info.state);

    TEST_CLEANUP();

    C_Logout( hSession );

    C_CloseSession( hSession2 );
    
    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}

static TEST_RESULT C_Login__duplicate_should_fail()
{
    CK_UTF8CHAR soPin[]   = "USERPIN";
    CK_SESSION_HANDLE hSession;
    CK_USER_TYPE user = CKU_USER;
    CK_SLOT_ID slotID = 0;

    TEST_START();

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK(CKR_OK, C_Login(hSession, user, NULL, MAXSTRLEN(soPin) ));

    TEST_CHECK(CKR_USER_ALREADY_LOGGED_IN, C_Login(hSession, user, NULL, MAXSTRLEN(soPin) ));

    TEST_CLEANUP();

    C_Logout( hSession );

    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}

static TEST_RESULT C_Login__another_session_logged_in_should_fail()
{
    CK_UTF8CHAR soPin[]   = "USERPIN";
    CK_SESSION_HANDLE hSession, hSession2;
    CK_USER_TYPE user = CKU_USER;
    CK_SLOT_ID slotID = 0;

    TEST_START();

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession2 ));

    TEST_CHECK(CKR_OK, C_Login(hSession, user, NULL, MAXSTRLEN(soPin) ));

    TEST_CHECK(CKR_USER_ANOTHER_ALREADY_LOGGED_IN, C_Login(hSession2, user, NULL, MAXSTRLEN(soPin) ));

    TEST_CLEANUP();

    C_Logout( hSession );

    C_CloseSession( hSession2 );

    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}

static TEST_RESULT C_Logout__invalid_session_should_fail()
{
    CK_UTF8CHAR soPin[]   = "SOPIN";
    CK_SESSION_HANDLE hSession = -1;
    CK_USER_TYPE user = CKU_SO;
    
    TEST_START();

    TEST_CHECK(CKR_SESSION_HANDLE_INVALID, C_Logout(hSession));

    TEST_END();
}


static TEST_RESULT C_Logout__valid_session_no_login_should_fail()
{
    CK_UTF8CHAR soPin[]   = "USERPIN";
    CK_SESSION_HANDLE hSession;
    CK_USER_TYPE user = CKU_USER;
    CK_SLOT_ID slotID = 0;

    TEST_START();

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK(CKR_USER_NOT_LOGGED_IN, C_Logout(hSession));

    TEST_CLEANUP();

    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}

static TEST_RESULT C_Logout__with_SO_should_change_all_sessions()
{
    CK_UTF8CHAR soPin[]   = "SOPIN";
    CK_SESSION_HANDLE hSession, hSession2;
    CK_USER_TYPE user = CKU_SO;
    CK_SLOT_ID slotID = 0;
    CK_SESSION_INFO info;

    TEST_START();

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession2 ));

    TEST_CHECK(CKR_OK, C_Login(hSession, user, NULL, MAXSTRLEN(soPin) ));

    TEST_CHECK(CKR_OK, C_GetSessionInfo( hSession, &info ));

    TEST_CHECK(CKF_SERIAL_SESSION | CKF_RW_SESSION, info.flags);
    TEST_CHECK(slotID                             , info.slotID);
    TEST_CHECK(CKS_RW_SO_FUNCTIONS                , info.state);

    TEST_CHECK(CKR_OK, C_GetSessionInfo( hSession2, &info ));

    TEST_CHECK(CKF_SERIAL_SESSION | CKF_RW_SESSION, info.flags);
    TEST_CHECK(slotID                             , info.slotID);
    TEST_CHECK(CKS_RW_SO_FUNCTIONS                , info.state);


    TEST_CHECK(CKR_OK, C_Logout( hSession ));


    TEST_CHECK(CKR_OK, C_GetSessionInfo( hSession, &info ));

    TEST_CHECK(CKF_SERIAL_SESSION | CKF_RW_SESSION, info.flags);
    TEST_CHECK(slotID                             , info.slotID);
    TEST_CHECK(CKS_RW_PUBLIC_SESSION              , info.state);

    TEST_CHECK(CKR_OK, C_GetSessionInfo( hSession2, &info ));

    TEST_CHECK(CKF_SERIAL_SESSION | CKF_RW_SESSION, info.flags);
    TEST_CHECK(slotID                             , info.slotID);
    TEST_CHECK(CKS_RW_PUBLIC_SESSION              , info.state);
    

    TEST_CLEANUP();

    C_CloseSession( hSession2 );
    
    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}

static TEST_RESULT C_Logout__with_USER_should_change_all_sessions()
{
    CK_UTF8CHAR soPin[]   = "USERPIN";
    CK_SESSION_HANDLE hSession, hSession2;
    CK_USER_TYPE user = CKU_USER;
    CK_SLOT_ID slotID = 0;
    CK_SESSION_INFO info;

    TEST_START();

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION, NULL, NULL, &hSession2 ));

    TEST_CHECK(CKR_OK, C_Login(hSession, user, NULL, MAXSTRLEN(soPin) ));

    TEST_CHECK(CKR_OK, C_GetSessionInfo( hSession, &info ));

    TEST_CHECK(CKF_SERIAL_SESSION | CKF_RW_SESSION, info.flags);
    TEST_CHECK(slotID                             , info.slotID);
    TEST_CHECK(CKS_RW_USER_FUNCTIONS              , info.state);

    TEST_CHECK(CKR_OK, C_GetSessionInfo( hSession2, &info ));

    TEST_CHECK(CKF_SERIAL_SESSION   , info.flags);
    TEST_CHECK(slotID               , info.slotID);
    TEST_CHECK(CKS_RO_USER_FUNCTIONS, info.state);


    TEST_CHECK(CKR_OK, C_Logout( hSession ));


    TEST_CHECK(CKR_OK, C_GetSessionInfo( hSession, &info ));

    TEST_CHECK(CKF_SERIAL_SESSION | CKF_RW_SESSION, info.flags);
    TEST_CHECK(slotID                             , info.slotID);
    TEST_CHECK(CKS_RW_PUBLIC_SESSION              , info.state);

    TEST_CHECK(CKR_OK, C_GetSessionInfo( hSession2, &info ));

    TEST_CHECK(CKF_SERIAL_SESSION   , info.flags);
    TEST_CHECK(slotID               , info.slotID);
    TEST_CHECK(CKS_RO_PUBLIC_SESSION, info.state);
    

    TEST_CLEANUP();

    C_CloseSession( hSession2 );
    
    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}

static TEST_RESULT C_Logout__with_CONTEXT_SPECIFIC_should_NOT_change_all_sessions()
{
    CK_UTF8CHAR soPin[]   = "CSPIN";
    CK_SESSION_HANDLE hSession, hSession2;
    CK_USER_TYPE user = CKU_CONTEXT_SPECIFIC;
    CK_SLOT_ID slotID = 0;
    CK_SESSION_INFO info;

    TEST_START();

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION, NULL, NULL, &hSession2 ));

    TEST_CHECK(CKR_OK, C_Login(hSession, user, NULL, MAXSTRLEN(soPin) ));

    TEST_CHECK(CKR_OK, C_GetSessionInfo( hSession, &info ));

    TEST_CHECK(CKF_SERIAL_SESSION | CKF_RW_SESSION, info.flags);
    TEST_CHECK(slotID                             , info.slotID);
    TEST_CHECK(CKS_RW_USER_FUNCTIONS              , info.state);

    TEST_CHECK(CKR_OK, C_GetSessionInfo( hSession2, &info ));

    TEST_CHECK(CKF_SERIAL_SESSION   , info.flags);
    TEST_CHECK(slotID               , info.slotID);
    TEST_CHECK(CKS_RO_PUBLIC_SESSION, info.state);


    TEST_CHECK(CKR_OK, C_Logout( hSession ));


    TEST_CHECK(CKR_OK, C_GetSessionInfo( hSession, &info ));

    TEST_CHECK(CKF_SERIAL_SESSION | CKF_RW_SESSION, info.flags);
    TEST_CHECK(slotID                             , info.slotID);
    TEST_CHECK(CKS_RW_PUBLIC_SESSION              , info.state);

    TEST_CHECK(CKR_OK, C_GetSessionInfo( hSession2, &info ));

    TEST_CHECK(CKF_SERIAL_SESSION   , info.flags);
    TEST_CHECK(slotID               , info.slotID);
    TEST_CHECK(CKS_RO_PUBLIC_SESSION, info.state);
    

    TEST_CLEANUP();

    C_CloseSession( hSession2 );
    
    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}


static TEST_RESULT C_Logout__with_open_object_handles_should_invalidate_handles()
{
    CK_UTF8CHAR soPin[]   = "CSPIN";
    CK_SESSION_HANDLE hSession, hSession2;
    CK_USER_TYPE user = CKU_CONTEXT_SPECIFIC;
    CK_SLOT_ID slotID = 0;
    CK_SESSION_INFO info;

    CK_OBJECT_CLASS cls = CKO_DATA;
    CK_BYTE data[] = { 0x00, 0x01, 0x02 };
    CK_UTF8CHAR label2[] = "Value1";
    CK_OBJECT_HANDLE hObj;
    CK_ULONG ulSize;
        
    CK_ATTRIBUTE dataTemplate[] = 
    {
        {CKA_CLASS, &cls, sizeof(cls) },
        {CKA_LABEL, label2, MAXSTRLEN(label2)},
        {CKA_VALUE, data, sizeof(data) },
    };
    
    TEST_START();

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION, NULL, NULL, &hSession2 ));

    TEST_CHECK(CKR_OK, C_Login(hSession, user, NULL, MAXSTRLEN(soPin) ));

    // get private object's handle might need to search
    TEST_CHECK(CKR_OK, C_CreateObject( hSession, dataTemplate, ARRAYSIZE(dataTemplate), &hObj ));


    TEST_CHECK(CKR_OK, C_Logout( hSession ));

    // Try to access private object via handle, it should be invalid at this point
    TEST_CHECK(CKR_OBJECT_HANDLE_INVALID, C_GetObjectSize( hSession, hObj, &ulSize ));

    TEST_CLEANUP();

    C_CloseSession( hSession2 );
    
    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}

static TEST_RESULT C_CreateObject__invalid_session_should_fail()
{
    CK_SESSION_HANDLE hSession = -1;
    CK_OBJECT_CLASS cls = CKO_DATA;
    CK_BYTE data[] = { 0x00, 0x01, 0x02 };
    CK_UTF8CHAR label2[] = "Value1";
    CK_OBJECT_HANDLE hObject;
    CK_SLOT_ID slotID = 0;
    
    CK_ATTRIBUTE dataTemplate[] = 
    {
        {CKA_CLASS, &cls, sizeof(cls) },
        {CKA_LABEL, label2, MAXSTRLEN(label2)},
        {CKA_VALUE, data, sizeof(data) },
    };
    
    TEST_START();

    TEST_CHECK(CKR_SESSION_HANDLE_INVALID, C_CreateObject(hSession, dataTemplate, ARRAYSIZE(dataTemplate), &hObject));

    TEST_END();    
}


static TEST_RESULT C_CreateObject__null_param_should_fail()
{
    CK_SESSION_HANDLE hSession = -1;
    CK_SLOT_ID slotID = 0;
    CK_OBJECT_CLASS cls = CKO_DATA;
    CK_BYTE data[] = { 0x00, 0x01, 0x02 };
    CK_UTF8CHAR label2[] = "Value1";
    CK_OBJECT_HANDLE hObject;
    
    CK_ATTRIBUTE dataTemplate[] = 
    {
        {CKA_CLASS, &cls, sizeof(cls) },
        {CKA_LABEL, label2, MAXSTRLEN(label2)},
        {CKA_VALUE, data, sizeof(data) },
    };
    
    TEST_START();

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK(CKR_ARGUMENTS_BAD, C_CreateObject(hSession, NULL, 0, &hObject));
    TEST_CHECK(CKR_ARGUMENTS_BAD, C_CreateObject(hSession, dataTemplate, ARRAYSIZE(dataTemplate), NULL));

    TEST_CLEANUP();

    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}

static TEST_RESULT C_CreateObject__only_session_objects_can_be_created_in_RO()
{
    CK_SESSION_HANDLE hSession = -1;
    CK_SLOT_ID slotID = 0;
    CK_OBJECT_CLASS cls = CKO_DATA;
    CK_BYTE data[] = { 0x00, 0x01, 0x02 };
    CK_UTF8CHAR label2[] = "Value1";
    CK_OBJECT_HANDLE hObject;
    
    CK_ATTRIBUTE dataTemplate[] = 
    {
        {CKA_CLASS, &cls, sizeof(cls) },
        {CKA_LABEL, label2, MAXSTRLEN(label2)},
        {CKA_VALUE, data, sizeof(data) },
    };
    
    TEST_START();

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK(CKR_SESSION_READ_ONLY, C_CreateObject(hSession, dataTemplate, ARRAYSIZE(dataTemplate), &hObject));

    TEST_CLEANUP();

    C_DestroyObject( hSession, hObject );

    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}


static TEST_RESULT C_CreateObject__only_public_items_if_not_logged_in()
{
    CK_SESSION_HANDLE hSession = -1;
    CK_SLOT_ID slotID = 0;
    CK_OBJECT_HANDLE hKey = -1;
    CK_OBJECT_CLASS keyClass = CKO_PRIVATE_KEY;
    CK_KEY_TYPE keyType = CKK_RSA;
    CK_BBOOL tru = CK_TRUE;
    CK_BYTE modulus[] = 
    {
        0x4d,   0xca,   0x11,   0xe1,   0x10,   0xcb,   0x7a,   0x2a,   0xf3,   0xea,   0x2a,   0x78,   0x35,   0x9a,   0x2d,   0xc1,   0x33,   0x30,   0x85,   0x86,   0x41,   
        0xb4,   0xaa,   0x2f,   0x07,   0xa4,   0xee,   0x5b,   0xc8,   0x78,   0x34,   0x11,   0x26,   0x90,   0x9d,   0x0a,   0x71,   0x37,   0x3f,   0xe1,   0x60,   0x56,   
        0xad,   0x07,   0xe1,   0xe7,   0xc6,   0x9a,   0xc7,   0x32,   0x52,   0xb5,   0xf6,   0x97,   0xf3,   0xc1,   0x0a,   0xfa,   0x1e,   0xc0,   0x79,   0xce,   0x61,   
        0x5f,   0x24,   0x89,   0xec,   0x92,   0x98,   0x06,   0x47,   0xe0,   0x37,   0x9b,   0xb3,   0x78,   0x63,   0x0f,   0xfb,   0x29,   0xe3,   0x6d,   0x5f,   0xbf,   
        0x01,   0x31,   0xed,   0x4e,   0x9b,   0xe6,   0x12,   0x20,   0xb8,   0x23,   0xa0,   0x62,   0xc8,   0xbc,   0x66,   0x97,   0xb7,   0x80,   0x64,   0xed,   0x64,   
        0x66,   0x49,   0x28,   0xb6,   0x39,   0x3f,   0xd4,   0xe8,   0x69,   0x06,   0x7f,   0x5c,   0x04,   0xfa,   0xbd,   0xdf,   0x79,   0xbb,   0x8d,   0xd9,   0x93,   
        0x0b,   0xf9,
    };
        
    CK_BYTE exponent[] = 
    {
        0x01,   0xbc,   0xe8,   0x8f,   0x7b,   0x7b,   0x22,   0xa1,   0xaf,   0xe7,   0xe2,   0x01,   0x14,   0xec,   0x0e,   0xdb,   0x1f,   0x19,   0x5e,   0x69,   0x32,   
        0x5d,   0xa6,   0x41,   0x1d,   0xbb,   0x32,   0x65,   0xaa,   0xd8,   0x3b,   0x4c,   0x6a,   0xf4,   0x26,   0xae,   0x79,   0x44,   0x8d,   0x7e,   0x4b,   0xaf,   
        0xb4,   0xc4,   0xcf,   0x0b,   0x14,   0x25,   0x48,   0xb0,   0x68,   0x6c,   0x8a,   0x42,   0xd1,   0xea,   0xd7,   0x3b,   0x91,   0x41,   0xe8,   0x31,   0xf7,   
        0x33,   0x20,   0xc4,   0x7f,   0xd6,   0xea,   0xe1,   0x45,   0x82,   0x94,   0xd1,   0x71,   0x43,   0xec,   0x20,   0x85,   0xf3,   0x4b,   0xb6,   0xb9,   0x7e,   
        0x17,   0xd3,   0x45,   0xa2,   0x83,   0x90,   0xe2,   0xa6,   0x73,   0xff,   0xda,   0x4f,   0x74,   0x80,   0xd6,   0xa7,   0x9f,   0x94,   0x73,   0x9e,   0x5d,   
        0x2f,   0x5b,   0x7d,   0xab,   0x58,   0x26,   0x54,   0x93,   0xe5,   0xeb,   0x31,   0x3d,   0x4d,   0xde,   0xb6,   0x71,   0x99,   0x8e,   0x9c,   0x6d,   0x1b,   
        0x9d,   0x86,
    };
    
    CK_ATTRIBUTE keyTemplate[] = 
    {
        {CKA_CLASS           , &keyClass, sizeof(keyClass)},
        {CKA_KEY_TYPE        , &keyType , sizeof(keyType) },
        {CKA_WRAP            , &tru     , sizeof(tru)     },
        {CKA_MODULUS         , modulus  , sizeof(modulus) },
        {CKA_PRIVATE_EXPONENT, exponent , sizeof(exponent)}
    };


    CK_OBJECT_CLASS cls = CKO_DATA;
    CK_BYTE data[] = { 0x00, 0x01, 0x02 };
    CK_UTF8CHAR label2[] = "Value1";
    CK_OBJECT_HANDLE hObject = -1;
    
    CK_ATTRIBUTE dataTemplate[] = 
    {
        {CKA_CLASS, &cls, sizeof(cls) },
        {CKA_LABEL, label2, MAXSTRLEN(label2)},
        {CKA_VALUE, data, sizeof(data) },
    };
    
    
    TEST_START();

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK(CKR_OK, C_CreateObject(hSession, dataTemplate, ARRAYSIZE(dataTemplate), &hObject));

    TEST_CHECK(CKR_USER_NOT_LOGGED_IN, C_CreateObject(hSession, keyTemplate, ARRAYSIZE(keyTemplate), &hKey));

    TEST_CLEANUP();

    C_DestroyObject( hSession, hObject );

    C_DestroyObject( hSession, hKey );

    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}

static TEST_RESULT C_CreateObject__private_key_NOT_always_sensitive_NOT_never_extractable()
{
    CK_SESSION_HANDLE hSession = -1;
    CK_SLOT_ID slotID = 0;
    CK_OBJECT_HANDLE hKey;
    CK_OBJECT_CLASS keyClass = CKO_PRIVATE_KEY;
    CK_KEY_TYPE keyType = CKK_RSA;
    CK_BBOOL tru = CK_TRUE;
    CK_BYTE modulus[] = 
    {
        
        0x6b, 0xdf, 0x51, 0xef, 0xdb, 0x6f, 0x10, 0x5c, 0x32, 0xbf, 0x87, 0x1c, 0xd1, 0x4c, 0x24, 0x7e, 
        0xe7, 0x2a, 0x14, 0x10, 0x6d, 0xeb, 0x2c, 0xd5, 0x8c, 0x0b, 0x95, 0x7b, 0xc7, 0x5d, 0xc6, 0x87,
        0x12, 0xea, 0xa9, 0xcd, 0x57, 0x7d, 0x3e, 0xcb, 0xe9, 0x6a, 0x46, 0xd0, 0xe1, 0xae, 0x2f, 0x86,
        0xd9, 0x50, 0xf9, 0x98, 0x71, 0xdd, 0x39, 0xfc, 0x0e, 0x60, 0xa9, 0xd3, 0xf2, 0x38, 0xbb, 0x8d,
    };
        
    CK_BYTE exponent[] = 
    {
       
        0xc9, 0x53, 0x65, 0x80, 0xb7, 0x16, 0xf2, 0x5e, 0x5e, 0xde, 0x0b, 0x57, 0x47, 0x43, 0x86, 0x85, 
        0x8a, 0xfb, 0x37, 0xac, 0x66, 0x34, 0xba, 0x09, 0x1a, 0xb1, 0x21, 0x0b, 0xaa, 0xfa, 0x6c, 0xb7,
        0x75, 0xa7, 0x3e, 0x23, 0x18, 0x58, 0x95, 0x90, 0xb5, 0x29, 0xa4, 0x1e, 0x15, 0x76, 0x52, 0x56,
        0xbb, 0x3d, 0x6b, 0x1d, 0x2a, 0xd1, 0x9f, 0x5c, 0x8a, 0xc0, 0x55, 0xea, 0xc3, 0x29, 0xa2, 0x1e,
       
    };

    CK_BYTE prime1[] =
    {
        0x5d, 0x2c, 0xbc, 0x1e, 0xc3, 0x38, 0xfe, 0x00, 0x5e, 0xca, 0xcf, 0xcd, 0xb4, 0x13, 0x89, 0x16, 
        0xd2, 0x07, 0xbc, 0x9b, 0xe1, 0x20, 0x31, 0x0b, 0x81, 0x28, 0x17, 0x0c, 0xc7, 0x73, 0x94, 0xee
    };

    CK_BYTE prime2[] =
    {
        0x67, 0xbe, 0x7b, 0x78, 0x4e, 0xc7, 0x91, 0x73, 0xa8, 0x34, 0x5a, 0x24, 0x9d, 0x92, 0x0d, 0xe8, 
        0x91, 0x61, 0x24, 0xdc, 0xb5, 0xeb, 0xdf, 0x71, 0x66, 0xdc, 0xe1, 0x77, 0xd4, 0x78, 0x14, 0x98
    };

    CK_BYTE exp1[] =
    {
        0x79, 0x44, 0xb0, 0x19, 0xf6, 0xf0, 0x7d, 0x63, 0xcf, 0x62, 0x67, 0x78, 0xd0, 0x7b, 0x10, 0xae, 
        0x6b, 0xdb, 0x40, 0xb3, 0xb2, 0xeb, 0x2e, 0x9f, 0x31, 0x34, 0x2d, 0xcb, 0xbf, 0xa2, 0x6a, 0xa6
    };
    
    CK_BYTE exp2[] =
    {
        0x1f, 0xe9, 0x03, 0x42, 0xf2, 0x63, 0x9b, 0xb7, 0x33, 0xd0, 0xfe, 0x20, 0x83, 0x26, 0x1f, 0x56, 
        0xa8, 0x24, 0xf5, 0x6d, 0x19, 0x51, 0xa5, 0x92, 0x31, 0xe4, 0x2b, 0xbc, 0x11, 0xc8, 0x26, 0x75
    };

    CK_BYTE coeff[] =
    {
        0xa0, 0x51, 0xe9, 0x83, 0xca, 0xee, 0x4b, 0xf0, 0x59, 0xeb, 0xa4, 0x81, 0xd6, 0x1f, 0x49, 0x42, 
        0x2b, 0x75, 0x89, 0xa7, 0x9f, 0x84, 0x7f, 0x1f, 0xc3, 0x8f, 0x70, 0xb6, 0x7e, 0x06, 0x5e, 0x8b
    };

    UINT32 pubExponent = 0x00010001;
    
    CK_ATTRIBUTE keyTemplate[] = 
    {
        {CKA_CLASS           , &keyClass   , sizeof(keyClass)},
        {CKA_KEY_TYPE        , &keyType    , sizeof(keyType) },
        {CKA_WRAP            , &tru        , sizeof(tru)     },
        {CKA_MODULUS         , modulus     , sizeof(modulus) },
        {CKA_PUBLIC_EXPONENT , &pubExponent, sizeof(pubExponent)},
        {CKA_PRIVATE_EXPONENT, exponent    , sizeof(exponent)},
        {CKA_PRIME_1         , prime1      , sizeof(prime1)},
        {CKA_PRIME_2         , prime2      , sizeof(prime2)},
        {CKA_EXPONENT_1      , exp1        , sizeof(exp1)},
        {CKA_EXPONENT_2      , exp2        , sizeof(exp2)},
        {CKA_COEFFICIENT     , coeff       , sizeof(coeff)},
    };

    CK_BBOOL fAlwaysSensitive = TRUE, fNeverExtractable = TRUE, fLocal = TRUE;
    CK_ATTRIBUTE keyInfoTemplate[] = 
    {
        {CKA_LOCAL            , &fLocal           , sizeof(fLocal)           },
        {CKA_ALWAYS_SENSITIVE , &fAlwaysSensitive , sizeof(fAlwaysSensitive) },
        {CKA_NEVER_EXTRACTABLE, &fNeverExtractable, sizeof(fNeverExtractable)}
    };

    CK_USER_TYPE user = CKU_USER;
    CK_UTF8CHAR  userPin[] = "USERPIN";

    CK_MECHANISM signMech = 
    {
        CKM_SHA1_RSA_PKCS,
        NULL,
        0,
    };

    CK_BYTE sig[512];
    CK_ULONG sigLen = sizeof(sig);

    CK_BYTE IV[16];

    CK_MECHANISM encMech = 
    {
        CKM_AES_CBC,
        IV,
        sizeof(IV),
    };

    CK_BYTE encData[2*sizeof(modulus)];
    CK_ULONG encDataLen = sizeof(encData);

    CK_BYTE data[sizeof(modulus)];
    CK_ULONG dataLen = sizeof(data);

    CK_MECHANISM keyGenMech = 
    {
        CKM_AES_KEY_GEN,
        NULL, 
        0
    };

    CK_ATTRIBUTE aesKeyTemplate[] = 
    {
        {CKA_CLASS           , &keyClass   , sizeof(keyClass)},
    };

    CK_OBJECT_HANDLE hAesKey;
    
    TEST_START();

    memset(&IV, 0, sizeof(IV));

    memcpy(data, modulus, sizeof(modulus));
    //memcpy(&data[sizeof(modulus)], exponent, sizeof(exponent));

    TEST_CHECK(CKR_OK, C_OpenSession( slotID, CKF_SERIAL_SESSION | CKF_RW_SESSION, NULL, NULL, &hSession ));

    TEST_CHECK(CKR_OK, C_Login(hSession, user, userPin, MAXSTRLEN(userPin) ));

    TEST_CHECK(CKR_OK, C_CreateObject(hSession, keyTemplate, ARRAYSIZE(keyTemplate), &hKey));

    TEST_CHECK(CKR_OK, C_SignInit(hSession, &signMech, hKey));

    TEST_CHECK(CKR_OK, C_Sign(hSession, modulus, sizeof(modulus), sig, &sigLen));

    TEST_CHECK(CKR_OK, C_VerifyInit(hSession, &signMech, hKey));

    TEST_CHECK(CKR_OK, C_Verify(hSession, modulus, sizeof(modulus), sig, sigLen));


    TEST_CHECK(CKR_OK, C_GenerateKey(hSession, &keyGenMech, aesKeyTemplate, 0, &hAesKey));

    TEST_CHECK(CKR_OK, C_EncryptInit(hSession, &encMech, hAesKey));

    TEST_CHECK(CKR_OK, C_Encrypt(hSession, data, sizeof(data), encData, &encDataLen));

    TEST_CHECK(CKR_OK, C_DecryptInit(hSession, &encMech, hAesKey));

    TEST_CHECK(CKR_OK, C_Decrypt(hSession, encData, encDataLen, data, &dataLen));

    TEST_CHECK(0, memcmp(modulus, data, sizeof(modulus)));


/*
    TEST_CHECK(CKR_OK, C_GetAttributeValue(hSession, hKey, keyInfoTemplate, ARRAYSIZE(keyInfoTemplate)));

    TEST_CHECK(FALSE, fLocal);
    TEST_CHECK(FALSE, fAlwaysSensitive);
    TEST_CHECK(FALSE, fNeverExtractable);
*/    
    TEST_CLEANUP();

    C_DestroyObject( hSession, hKey );

    C_Logout( hSession );

    C_CloseSession( hSession );

    TEST_END_CLEANUP();
}


/*
static TEST_RESULT C_WaitForSlotEvent_null_arg_should_fail()
{
    TEST_START();

    TEST_CHECK(CKR_ARGUMENTS_BAD, C_GetTokenInfo(0, NULL));

    TEST_END();
}

static TEST_RESULT C_WaitForSlotEvent_invalid_slotID_should_fail()
{
    CK_TOKEN_INFO info;
    
    TEST_START();

    TEST_CHECK(CKR_SLOT_ID_INVALID, C_GetTokenInfo((CK_ULONG)-1, &info));

    TEST_END();
}


static TEST_RESULT C_EncryptInit_NO_C_Initialize()
{
    CK_MECHANISM encryptMech;

    encryptMech.mechanism = CKM_AES_CBC;
    encryptMech.pParameter = NULL;
    encryptMech.ulParameterLen = 0;

    CK_OBJECT_HANDLE hKey = 1;
    
    return CKR_CRYPTOKI_NOT_INITIALIZED, C_EncryptInit(encryptMech, hKey);
}
*/

BOOL TestEntry()
{
    TEST_SUITE_START();

    RUN_TEST(C_Initialize_ARGS_Not_supported);
    RUN_TEST(C_Initialize_NULL_PTR_ARGS_Normal_Usage);
    RUN_TEST(C_Initialize_multiple_calls_Should_FAIL);
    RUN_TEST(C_Finalize_without_init_should_fail);
    RUN_TEST(C_Finalize_arg_other_than_null_should_fail);
    RUN_TEST(C_Finalize_normal_should_succeed);
    RUN_TEST(C_GetInfo_NULL_parameter_should_fail);
    RUN_TEST(C_GetInfo_normal_usage);

    RUN_TEST(C_GetFunctionList_null_param_should_fail);
    RUN_TEST(C_GetFunctionList_call_before_c_init_should_succeed);
    RUN_TEST(C_GetFunctionList_call_after_c_init_should_succeed);
    RUN_TEST(C_GetFunctionList_validate_function_ptrs);

    RUN_TEST(C_GetSlotList_NULL_count_arg_should_fail);
    RUN_TEST(C_GetSlotList_buffer_too_small_error);
    RUN_TEST(C_GetSlotList_normal_usage_more_slots);

    RUN_TEST(C_GetSlotInfo_null_arg_should_fail);
    RUN_TEST(C_GetSlotInfo_bad_slot_id_should_fail);
    RUN_TEST(C_GetSlotInfo_normal_usage);

    RUN_TEST(C_GetTokenInfo_null_arg_should_fail);
    RUN_TEST(C_GetTokenInfo_invalid_slotID_should_fail);
    RUN_TEST(C_GetTokenInfo_no_token_should_fail);
    RUN_TEST(C_GetTokenInfo_normal_usage);
    
    RUN_TEST(C_GetMechanismList_null_arg_should_fail);
    RUN_TEST(C_GetMechanismList_invalid_slotID_should_fail);
    RUN_TEST(C_GetMechanismList_buffer_too_small_should_fail);
    RUN_TEST(C_GetMechanismList_normal_usage);

    RUN_TEST(C_GetMechanismInfo_null_arg_should_fail);
    RUN_TEST(C_GetMechanismInfo_invalid_slotID_should_fail);
    RUN_TEST(C_GetMechanismInfo_invalid_mech_should_fail);
    RUN_TEST(C_GetMechanismInfo_normal_usage);

    RUN_TEST(C_InitToken__invalid_slotID_should_fail);
    RUN_TEST(C_InitToken__null_args_should_fail);
    RUN_TEST(C_InitToken__short_label_should_fail);
    RUN_TEST(C_InitToken__with_open_session_should_fail);
    RUN_TEST(C_InitToken__init_with_wrong_pin_should_fail);
    RUN_TEST(C_InitToken__should_remove_all_objects);
    RUN_TEST(C_InitToken__normal_usage);

    RUN_TEST(C_InitPin__invalid_session_should_fail);
    RUN_TEST(C_InitPin__null_args_should_fail);
    RUN_TEST(C_InitPin__no_login_should_fail);
    RUN_TEST(C_InitPin__wrong_login_should_fail);
    RUN_TEST(C_InitPin__normal_usage);

    RUN_TEST(C_SetPin__invalid_session_should_fail);
    RUN_TEST(C_SetPin__null_param_should_fail);
    RUN_TEST(C_SetPin__read_only_session_should_fail);
    RUN_TEST(C_SetPin__normal_usage);

    RUN_TEST(C_GetSessionInfo__invalid_session_should_fail);
    RUN_TEST(C_GetSessionInfo__null_arg_should_fail);
    RUN_TEST(C_GetSessionInfo__normal_usage);

    RUN_TEST(C_GetOperationState__NOT_IMPLEMENTED);
    RUN_TEST(C_SetOperationState__NOT_IMPLEMENTED);
    
    RUN_TEST(C_Login__invalid_session_should_fail);
    RUN_TEST(C_Login__with_user_should_change_all_sessions);
    RUN_TEST(C_Login__with_SO_should_change_all_sessions);
    RUN_TEST(C_Login__with_SO_should_fail_if_RO_sessions_open);
    RUN_TEST(C_Login__with_CONTEXT_SPECIFIC_should_NOT_change_all_sessions);
    RUN_TEST(C_Login__duplicate_should_fail);
    RUN_TEST(C_Login__another_session_logged_in_should_fail);

    RUN_TEST(C_Logout__invalid_session_should_fail);
    RUN_TEST(C_Logout__valid_session_no_login_should_fail);
    RUN_TEST(C_Logout__with_SO_should_change_all_sessions);
    RUN_TEST(C_Logout__with_USER_should_change_all_sessions);
    RUN_TEST(C_Logout__with_CONTEXT_SPECIFIC_should_NOT_change_all_sessions);
    RUN_TEST(C_Logout__with_open_object_handles_should_invalidate_handles);

    RUN_TEST(C_CreateObject__invalid_session_should_fail);
    RUN_TEST(C_CreateObject__null_param_should_fail);
    RUN_TEST(C_CreateObject__only_public_items_if_not_logged_in);
    RUN_TEST(C_CreateObject__only_session_objects_can_be_created_in_RO);
    RUN_TEST(C_CreateObject__private_key_NOT_always_sensitive_NOT_never_extractable);
    
//    RUN_TEST();
    
    //RUN_TEST(C_EncryptInit_NO_C_Initialize);

    TEST_SUITE_END();
}
