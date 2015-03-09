#include "SPOT_Update.h"


static const CLR_RT_MethodHandler method_lookup[] =
{
    Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::_cctor___STATIC__VOID,
    Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::Initialize___STATIC__I4__MicrosoftSPOTMFUpdateMFUpdateBase,
    Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::AuthCommand___STATIC__BOOLEAN__I4__I4__SZARRAY_U1__BYREF_SZARRAY_U1,
    Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::Authenticate___STATIC__BOOLEAN__I4__SZARRAY_U1,
    Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::Open___STATIC__BOOLEAN__I4,
    Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::Create___STATIC__BOOLEAN__I4,
    Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::GetMissingPackets___STATIC__VOID__I4__SZARRAY_U4,
    Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::GetUpdateProperty___STATIC__BOOLEAN__I4__STRING__BYREF_SZARRAY_U1,
    Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::SetUpdateProperty___STATIC__BOOLEAN__I4__STRING__SZARRAY_U1,
    Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::AddPacket___STATIC__BOOLEAN__I4__I4__SZARRAY_U1__SZARRAY_U1,
    Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::Validate___STATIC__BOOLEAN__I4__SZARRAY_U1,
    Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::Install___STATIC__BOOLEAN__I4__SZARRAY_U1,
    Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::DeleteUpdate___STATIC__VOID__I4,
    Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::SerializeParameter___STATIC__SZARRAY_U1__OBJECT,
    Library_spot_update_native_Microsoft_SPOT_MFUpdate_MFNativeUpdate::DeserializeParameter___STATIC__BOOLEAN__SZARRAY_U1__OBJECT,
    NULL,
    NULL,
};

const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_Update =
{
    "Microsoft.SPOT.Update", 
    0xE3DA53A0,
    method_lookup
};

