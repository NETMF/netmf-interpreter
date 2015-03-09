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


#ifndef _SPOT_UPDATE_NATIVE_H_
#define _SPOT_UPDATE_NATIVE_H_

struct Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate
{
    TINYCLR_NATIVE_DECLARE(_cctor___STATIC__VOID);
    TINYCLR_NATIVE_DECLARE(Initialize___STATIC__I4__MicrosoftSPOTMFUpdateMFUpdateBase);
    TINYCLR_NATIVE_DECLARE(AuthCommand___STATIC__BOOLEAN__I4__I4__SZARRAY_U1__BYREF_SZARRAY_U1);
    TINYCLR_NATIVE_DECLARE(Authenticate___STATIC__BOOLEAN__I4__SZARRAY_U1);
    TINYCLR_NATIVE_DECLARE(Open___STATIC__BOOLEAN__I4);
    TINYCLR_NATIVE_DECLARE(Create___STATIC__BOOLEAN__I4);
    TINYCLR_NATIVE_DECLARE(GetMissingPackets___STATIC__VOID__I4__SZARRAY_U4);
    TINYCLR_NATIVE_DECLARE(GetUpdateProperty___STATIC__BOOLEAN__I4__STRING__BYREF_SZARRAY_U1);
    TINYCLR_NATIVE_DECLARE(SetUpdateProperty___STATIC__BOOLEAN__I4__STRING__SZARRAY_U1);
    TINYCLR_NATIVE_DECLARE(AddPacket___STATIC__BOOLEAN__I4__I4__SZARRAY_U1__SZARRAY_U1);
    TINYCLR_NATIVE_DECLARE(Validate___STATIC__BOOLEAN__I4__SZARRAY_U1);
    TINYCLR_NATIVE_DECLARE(Install___STATIC__BOOLEAN__I4__SZARRAY_U1);
    TINYCLR_NATIVE_DECLARE(DeleteUpdate___STATIC__VOID__I4);
    TINYCLR_NATIVE_DECLARE(SerializeParameter___STATIC__SZARRAY_U1__OBJECT);
    TINYCLR_NATIVE_DECLARE(DeserializeParameter___STATIC__BOOLEAN__SZARRAY_U1__OBJECT);

    //--//

};

struct Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFUpdateBase
{
    static const int FIELD__m_provider = 1;
    static const int FIELD__m_updateID = 2;
    static const int FIELD__m_updateVersion = 3;
    static const int FIELD__m_updateType = 4;
    static const int FIELD__m_updateSubType = 5;
    static const int FIELD__m_updateSize = 6;
    static const int FIELD__m_packetSize = 7;
    static const int FIELD__m_updateHandle = 8;


    //--//

};

struct Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFUpdateProperty
{
    static const int FIELD__Name = 1;
    static const int FIELD__Value = 2;


    //--//

};

//--//

extern const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_Update;

#endif  //_SPOT_UPDATE_NATIVE_H_
