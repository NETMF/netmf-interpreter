//-----------------------------------------------------------------------------
//
//                   ** WARNING! ** 
//    This file was generated automatically by a tool.
//    Re-running the tool will overwrite this file.
//    You should copy this file to a custom location
//    before adding any customization in the copy to
//    prevent loss of your changes when the tool is
//    re-run.
//
//-----------------------------------------------------------------------------


#ifndef _SECURITY_PKCS11_NATIVE_H_
#define _SECURITY_PKCS11_NATIVE_H_

struct Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiAttribute
{
    static const int FIELD__Type = 1;
    static const int FIELD__Value = 2;


    //--//

};

struct Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer
{
    static const int FIELD__m_session = 1;
    static const int FIELD__m_ownsSession = 2;
    static const int FIELD__m_isDisposed = 3;
    static const int FIELD__m_isSessionClosing = 4;


    //--//

};

struct Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiObject
{
    static const int FIELD__m_handle = 5;

    TINYCLR_NATIVE_DECLARE(get_Size___I4);
    TINYCLR_NATIVE_DECLARE(CopyInternal___MicrosoftSPOTCryptokiCryptokiObject__SZARRAY_MicrosoftSPOTCryptokiCryptokiAttribute);
    TINYCLR_NATIVE_DECLARE(GetAttributeValues___BOOLEAN__BYREF_SZARRAY_MicrosoftSPOTCryptokiCryptokiAttribute);
    TINYCLR_NATIVE_DECLARE(SetAttributeValues___BOOLEAN__SZARRAY_MicrosoftSPOTCryptokiCryptokiAttribute);
    TINYCLR_NATIVE_DECLARE(Destroy___VOID);
    TINYCLR_NATIVE_DECLARE(CreateObjectInternal___STATIC__MicrosoftSPOTCryptokiCryptokiObject__MicrosoftSPOTCryptokiSession__SZARRAY_MicrosoftSPOTCryptokiCryptokiAttribute);

    //--//

};

struct Library_security_pkcs11_native_System_Security_Cryptography_CryptoKey
{
    static const int FIELD__m_length = 6;
    static const int FIELD__m_keyType = 7;
    static const int FIELD__m_privateKeyHandle = 8;

    TINYCLR_NATIVE_DECLARE(DeriveKeyInternal___SystemSecurityCryptographyCryptoKey__MicrosoftSPOTCryptokiMechanism__SZARRAY_MicrosoftSPOTCryptokiCryptokiAttribute);
    TINYCLR_NATIVE_DECLARE(GenerateKeyInternal___STATIC__SystemSecurityCryptographyCryptoKey__MicrosoftSPOTCryptokiSession__MicrosoftSPOTCryptokiMechanism__SZARRAY_MicrosoftSPOTCryptokiCryptokiAttribute);
    TINYCLR_NATIVE_DECLARE(GenerateKeyPairInternal___STATIC__SystemSecurityCryptographyCryptoKey__MicrosoftSPOTCryptokiSession__MicrosoftSPOTCryptokiMechanism__SZARRAY_MicrosoftSPOTCryptokiCryptokiAttribute__SZARRAY_MicrosoftSPOTCryptokiCryptokiAttribute);
    TINYCLR_NATIVE_DECLARE(WrapKey___STATIC__SZARRAY_U1__MicrosoftSPOTCryptokiSession__MicrosoftSPOTCryptokiMechanism__SystemSecurityCryptographyCryptoKey__SystemSecurityCryptographyCryptoKey);
    TINYCLR_NATIVE_DECLARE(UnwrapKeyInternal___STATIC__SystemSecurityCryptographyCryptoKey__MicrosoftSPOTCryptokiSession__MicrosoftSPOTCryptokiMechanism__SystemSecurityCryptographyCryptoKey__SZARRAY_U1__SZARRAY_MicrosoftSPOTCryptokiCryptokiAttribute);

    //--//

};

struct Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_MechanismInfo
{
    static const int FIELD__MinKeySize = 1;
    static const int FIELD__MaxKeySize = 2;
    static const int FIELD__Flags = 3;


    //--//

};

struct Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Cryptoki
{
    static const int FIELD_STATIC__m_slotList = 0;

    TINYCLR_NATIVE_DECLARE(_cctor___STATIC__VOID);
    TINYCLR_NATIVE_DECLARE(GetSlotsInternal___STATIC__SZARRAY_MicrosoftSPOTCryptokiSlot);
    TINYCLR_NATIVE_DECLARE(FindSlots___STATIC__SZARRAY_MicrosoftSPOTCryptokiSlot__STRING__SZARRAY_MicrosoftSPOTCryptokiMechanism);

    //--//

};

struct Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiCertificate
{
    static const int FIELD__m_propertyBag = 9;


    //--//

};

struct Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiDigest
{
    static const int FIELD__m_hashSize = 5;
    static const int FIELD__m_mechanism = 6;
    static const int FIELD__m_isInit = 7;

    TINYCLR_NATIVE_DECLARE(Init___VOID__MicrosoftSPOTCryptokiMechanism);
    TINYCLR_NATIVE_DECLARE(DigestKeyInternal___VOID__SystemSecurityCryptographyCryptoKey);
    TINYCLR_NATIVE_DECLARE(DigestInternal___SZARRAY_U1__SZARRAY_U1__I4__I4);
    TINYCLR_NATIVE_DECLARE(DigestUpdateInternal___VOID__SZARRAY_U1__I4__I4);
    TINYCLR_NATIVE_DECLARE(DigestFinalInternal___SZARRAY_U1);

    //--//

};

struct Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiRNG
{
    TINYCLR_NATIVE_DECLARE(GenerateRandom___VOID__SZARRAY_U1__I4__I4__BOOLEAN);
    TINYCLR_NATIVE_DECLARE(SeedRandom___VOID__SZARRAY_U1);

    //--//

};

struct Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiSign
{
    static const int FIELD__m_signatureLength = 5;
    static const int FIELD__m_isInit = 6;
    static const int FIELD__m_mech = 7;
    static const int FIELD__m_key = 8;

    TINYCLR_NATIVE_DECLARE(SignInit___VOID__MicrosoftSPOTCryptokiSession__MicrosoftSPOTCryptokiMechanism__SystemSecurityCryptographyCryptoKey);
    TINYCLR_NATIVE_DECLARE(SignInternal___SZARRAY_U1__SZARRAY_U1__I4__I4);
    TINYCLR_NATIVE_DECLARE(SignUpdateInternal___VOID__SZARRAY_U1__I4__I4);
    TINYCLR_NATIVE_DECLARE(SignFinalInternal___SZARRAY_U1);

    //--//

};

struct Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiVerify
{
    static const int FIELD__m_isInit = 5;
    static const int FIELD__m_mech = 6;
    static const int FIELD__m_key = 7;

    TINYCLR_NATIVE_DECLARE(VerifyInit___VOID__MicrosoftSPOTCryptokiSession__MicrosoftSPOTCryptokiMechanism__SystemSecurityCryptographyCryptoKey);
    TINYCLR_NATIVE_DECLARE(VerifyInternal___BOOLEAN__SZARRAY_U1__I4__I4__SZARRAY_U1__I4__I4);
    TINYCLR_NATIVE_DECLARE(VerifyUpdateInternal___VOID__SZARRAY_U1__I4__I4);
    TINYCLR_NATIVE_DECLARE(VerifyFinalInternal___BOOLEAN__SZARRAY_U1__I4__I4);

    //--//

};

struct Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiVersion
{
    static const int FIELD__Major = 1;
    static const int FIELD__Minor = 2;


    //--//

};

struct Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_DATE_TIME_INFO
{
    static const int FIELD__year = 1;
    static const int FIELD__month = 2;
    static const int FIELD__day = 3;
    static const int FIELD__hour = 4;
    static const int FIELD__minute = 5;
    static const int FIELD__second = 6;
    static const int FIELD__msec = 7;
    static const int FIELD__dlsTime = 8;
    static const int FIELD__tzOffset = 9;


    //--//

};

struct Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Decryptor
{
    static const int FIELD__m_inputBlockSize = 5;
    static const int FIELD__m_outputBlockSize = 6;
    static const int FIELD__m_isInit = 7;
    static const int FIELD__m_mech = 8;
    static const int FIELD__m_key = 9;

    TINYCLR_NATIVE_DECLARE(DecryptInit___VOID__MicrosoftSPOTCryptokiSession__MicrosoftSPOTCryptokiMechanism__SystemSecurityCryptographyCryptoKey);
    TINYCLR_NATIVE_DECLARE(TransformBlockInternal___I4__SZARRAY_U1__I4__I4__SZARRAY_U1__I4);
    TINYCLR_NATIVE_DECLARE(TransformFinalBlockInternal___SZARRAY_U1__SZARRAY_U1__I4__I4);

    //--//

};

struct Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Encryptor
{
    static const int FIELD__m_inputBlockSize = 5;
    static const int FIELD__m_outputBlockSize = 6;
    static const int FIELD__m_isInit = 7;
    static const int FIELD__m_mech = 8;
    static const int FIELD__m_key = 9;

    TINYCLR_NATIVE_DECLARE(EncryptInit___VOID__MicrosoftSPOTCryptokiSession__MicrosoftSPOTCryptokiMechanism__SystemSecurityCryptographyCryptoKey);
    TINYCLR_NATIVE_DECLARE(TransformBlockInternal___I4__SZARRAY_U1__I4__I4__SZARRAY_U1__I4);
    TINYCLR_NATIVE_DECLARE(TransformFinalBlockInternal___SZARRAY_U1__SZARRAY_U1__I4__I4);

    //--//

};

struct Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_FindObjectEnum
{
    static const int FIELD__m_session = 1;

    TINYCLR_NATIVE_DECLARE(FindObjectsInit___VOID__SZARRAY_MicrosoftSPOTCryptokiCryptokiAttribute);
    TINYCLR_NATIVE_DECLARE(FindObjects___SZARRAY_MicrosoftSPOTCryptokiCryptokiObject__I4);
    TINYCLR_NATIVE_DECLARE(FindObjectsFinal___VOID);
    TINYCLR_NATIVE_DECLARE(get_Count___I4);

    //--//

};

struct Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Mechanism
{
    static const int FIELD__Type = 1;
    static const int FIELD__Parameter = 2;


    //--//

};

struct Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session
{
    static const int FIELD__m_handle = 1;
    static const int FIELD__m_disposed = 2;
    static const int FIELD__m_maxProcessingBytes = 3;
    static const int FIELD__m_objects = 4;

    TINYCLR_NATIVE_DECLARE(InitSession___VOID__STRING__SZARRAY_MicrosoftSPOTCryptokiMechanismType);
    TINYCLR_NATIVE_DECLARE(Login___BOOLEAN__MicrosoftSPOTCryptokiSessionUserType__STRING);
    TINYCLR_NATIVE_DECLARE(Logout___BOOLEAN);
    TINYCLR_NATIVE_DECLARE(GetSessionInfo___VOID__BYREF_MicrosoftSPOTCryptokiSessionSessionInfo);
    TINYCLR_NATIVE_DECLARE(InitializePin___BOOLEAN__STRING);
    TINYCLR_NATIVE_DECLARE(SetPin___BOOLEAN__STRING__STRING);
    TINYCLR_NATIVE_DECLARE(Close___VOID);

    //--//

};

struct Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot
{
    static const int FIELD__m_slotEvent = 1;
    static const int FIELD__m_slotIndex = 2;
    static const int FIELD__m_evtDispatcher = 3;
    static const int FIELD__m_disposed = 4;
    static const int FIELD__m_slotInfo = 5;

    TINYCLR_NATIVE_DECLARE(GetSlotInfoInternal___VOID__MicrosoftSPOTCryptokiSlotSlotInfo);
    TINYCLR_NATIVE_DECLARE(GetTokenInfo___VOID__BYREF_MicrosoftSPOTCryptokiTokenInfo);
    TINYCLR_NATIVE_DECLARE(get_SupportedMechanisms___SZARRAY_MicrosoftSPOTCryptokiMechanismType);
    TINYCLR_NATIVE_DECLARE(GetMechanismInfo___VOID__MicrosoftSPOTCryptokiMechanismType__BYREF_MicrosoftSPOTCryptokiMechanismInfo);
    TINYCLR_NATIVE_DECLARE(OpenSession___MicrosoftSPOTCryptokiSession__MicrosoftSPOTCryptokiSessionSessionFlag);
    TINYCLR_NATIVE_DECLARE(InitializeToken___VOID__STRING__STRING);

    //--//

};

struct Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_TokenInfo
{
    static const int FIELD__Label = 1;
    static const int FIELD__Manufacturer = 2;
    static const int FIELD__Model = 3;
    static const int FIELD__SerialNumber = 4;
    static const int FIELD__Flags = 5;
    static const int FIELD__MaxSessionCount = 6;
    static const int FIELD__SessionCount = 7;
    static const int FIELD__MaxRwSessionCount = 8;
    static const int FIELD__MaxPinLen = 9;
    static const int FIELD__MinPinLen = 10;
    static const int FIELD__TotalPublicMemory = 11;
    static const int FIELD__FreePublicMemory = 12;
    static const int FIELD__TotalPrivateMemory = 13;
    static const int FIELD__FreePrivateMemory = 14;
    static const int FIELD__HardwareVersion = 15;
    static const int FIELD__FirmwareVersion = 16;
    static const int FIELD__m_UtcTimeString = 17;


    //--//

};

struct Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session__SessionInfo
{
    static const int FIELD__SlotID = 1;
    static const int FIELD__State = 2;
    static const int FIELD__Flag = 3;
    static const int FIELD__DeviceError = 4;


    //--//

};

struct Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Slot__SlotInfo
{
    static const int FIELD__Description = 1;
    static const int FIELD__ManufactureID = 2;
    static const int FIELD__Flags = 3;
    static const int FIELD__HardwareVersion = 4;
    static const int FIELD__FirmwareVersion = 5;


    //--//

};

struct Library_security_pkcs11_native_System_Security_Cryptography_CryptographicException
{
    static const int FIELD__m_errorCode = 5;


    //--//

};

extern HRESULT CryptokiHandleError(CLR_RT_StackFrame& stack, CLR_UINT32 result);

#define CRYPTOKI_CHECK_RESULT(stack,x) \
    if(FAILED(hr = CryptokiHandleError(stack, x))) TINYCLR_LEAVE()


extern const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_Security_PKCS11;
extern const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_Cryptoki_SlotEvent;

#endif  //_SECURITY_PKCS11_NATIVE_H_
