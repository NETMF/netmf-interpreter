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


#ifndef _SPOT_ALLJOYN_NATIVE_SERVICES_MICROSOFT_SPOT_ALLJOYN_SERVICES_AJ_SERVICES_H_
#define _SPOT_ALLJOYN_NATIVE_SERVICES_MICROSOFT_SPOT_ALLJOYN_SERVICES_AJ_SERVICES_H_

namespace Microsoft
{
    namespace SPOT
    {
        namespace AllJoyn
        {
            namespace Services
            {
                struct AJ_Services
                {
                    // Helper Functions to access fields of managed object
                    static UNSUPPORTED_TYPE& Get_AjInst( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_UNSUPPORTED_TYPE( pMngObj, Library_spot_alljoyn_native_services_Microsoft_SPOT_AllJoyn_Services_AJ_Services::FIELD__AjInst ); }

                    static UNSUPPORTED_TYPE& Get_propertyStore( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_UNSUPPORTED_TYPE( pMngObj, Library_spot_alljoyn_native_services_Microsoft_SPOT_AllJoyn_Services_AJ_Services::FIELD__propertyStore ); }

                    static UINT16& Get_AJNS_NotificationVersion( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_UINT16( pMngObj, Library_spot_alljoyn_native_services_Microsoft_SPOT_AllJoyn_Services_AJ_Services::FIELD__AJNS_NotificationVersion ); }

                    static UINT32& Get_NotificationId( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_UINT32( pMngObj, Library_spot_alljoyn_native_services_Microsoft_SPOT_AllJoyn_Services_AJ_Services::FIELD__NotificationId ); }

                    static UNSUPPORTED_TYPE& Get_LastSentNotifications( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_UNSUPPORTED_TYPE( pMngObj, Library_spot_alljoyn_native_services_Microsoft_SPOT_AllJoyn_Services_AJ_Services::FIELD__LastSentNotifications ); }

                    // Declaration of stubs. These functions are implemented by Interop code developers
                    static UNSUPPORTED_TYPE Initialize_NotificationService( CLR_RT_HeapBlock* pMngObj, LPCSTR param0, HRESULT &hr );
                    static UNSUPPORTED_TYPE SendNotifySignal( CLR_RT_HeapBlock* pMngObj, UINT32 param0, UNSUPPORTED_TYPE param1, UINT32 param2, UINT32 param3, LPCSTR param4, HRESULT &hr );
                };
            }
        }
    }
}
#endif  //_SPOT_ALLJOYN_NATIVE_SERVICES_MICROSOFT_SPOT_ALLJOYN_SERVICES_AJ_SERVICES_H_
