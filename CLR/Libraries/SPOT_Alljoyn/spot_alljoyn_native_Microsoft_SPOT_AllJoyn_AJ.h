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


#ifndef _SPOT_ALLJOYN_NATIVE_MICROSOFT_SPOT_ALLJOYN_AJ_H_
#define _SPOT_ALLJOYN_NATIVE_MICROSOFT_SPOT_ALLJOYN_AJ_H_

namespace Microsoft
{
    namespace SPOT
    {
        namespace AllJoyn
        {
            struct AJ
            {
                
                //static LPCSTR& Get_m_PwdText( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_LPCSTR( pMngObj, Library_spot_alljoyn_native_Microsoft_SPOT_AllJoyn_AJ::FIELD__m_PwdText ); }

                // Declaration of stubs. These functions are implemented by Interop code developers
                static LPCSTR GetUniqueName( CLR_RT_HeapBlock* pMngObj, UINT32 param0, HRESULT &hr );
                static void Initialize( CLR_RT_HeapBlock* pMngObj, HRESULT &hr );
                static INT32 SetBusLinkTimeout( CLR_RT_HeapBlock* pMngObj, UINT32 param0, UINT32 param1, HRESULT &hr );
                static void NotifyLinkActive( CLR_RT_HeapBlock* pMngObj, HRESULT &hr );
                static INT32 BusLinkStateProc( CLR_RT_HeapBlock* pMngObj, UINT32 param0, HRESULT &hr );
                static INT32 SetIdleTimeouts( CLR_RT_HeapBlock* pMngObj, UINT32 param0, UINT32 param1, UINT32 param2, HRESULT &hr );
                static INT32 StartService( CLR_RT_HeapBlock* pMngObj, UINT32 param0, LPCSTR param1, UINT32 param2, INT8 param3, UINT16 param4, LPCSTR param5, UINT32 param6, HRESULT &hr );
                static INT32 StartClientByName( CLR_RT_HeapBlock* pMngObj, UINT32 param0, LPCSTR param1, UINT32 param2, UINT8 param3, LPCSTR param4, UINT16 param5, UINT32 * param6, UNSUPPORTED_TYPE param7, UNSUPPORTED_TYPE param8, HRESULT &hr );
                static void AlwaysPrintf( CLR_RT_HeapBlock* pMngObj, LPCSTR param0, HRESULT &hr );
                static INT32 AboutIconHandleGetContent( CLR_RT_HeapBlock* pMngObj, UNSUPPORTED_TYPE param0, UNSUPPORTED_TYPE param1, UNSUPPORTED_TYPE param2, HRESULT &hr );
                static INT32 BusHandleBusMessageInner( CLR_RT_HeapBlock* pMngObj, UNSUPPORTED_TYPE param0, HRESULT &hr );
                static INT32 BusCancelSessionless( CLR_RT_HeapBlock* pMngObj, UINT32 param0, UINT32 param1, HRESULT &hr );
                static INT32 MarshalReplyMsg( CLR_RT_HeapBlock* pMngObj, UNSUPPORTED_TYPE param0, UNSUPPORTED_TYPE param1, HRESULT &hr );
                static INT32 MarshalSignal( CLR_RT_HeapBlock* pMngObj, UINT32 param0, UNSUPPORTED_TYPE param1, UNSUPPORTED_TYPE param2, UINT32 param3, UINT32 param4, UINT32 param5, UINT8 param6, HRESULT &hr );
                static INT32 MarshalMethodCall( CLR_RT_HeapBlock* pMngObj, UINT32 param0, UNSUPPORTED_TYPE param1, UNSUPPORTED_TYPE param2, UINT32 param3, LPCSTR param4, UINT32 param5, UINT8 param6, HRESULT &hr );
                static INT32 UnmarshalArg( CLR_RT_HeapBlock* pMngObj, UNSUPPORTED_TYPE param0, UNSUPPORTED_TYPE param1, HRESULT &hr );
                static INT32 DeliverMsg( CLR_RT_HeapBlock* pMngObj, UNSUPPORTED_TYPE param0, HRESULT &hr );
                static void BusSetPasswordCallback( CLR_RT_HeapBlock* pMngObj, HRESULT &hr );
                static INT32 UnmarshalMsg( CLR_RT_HeapBlock* pMngObj, UINT32 param0, UNSUPPORTED_TYPE param1, UNSUPPORTED_TYPE param2, HRESULT &hr );
                static LPCSTR UnmarshalVariant( CLR_RT_HeapBlock* pMngObj, UNSUPPORTED_TYPE param0, HRESULT &hr );
                static INT32 MarshalVariant( CLR_RT_HeapBlock* pMngObj, UNSUPPORTED_TYPE param0, UNSUPPORTED_TYPE param1, HRESULT &hr );
                static INT32 UnmarshalArgs( CLR_RT_HeapBlock* pMngObj, UNSUPPORTED_TYPE param0, UNSUPPORTED_TYPE param1, LPCSTR param2, UINT16 param3, UINT32 param4, HRESULT &hr );
                static INT32 UnmarshalArgs( CLR_RT_HeapBlock* pMngObj, UNSUPPORTED_TYPE param0, UNSUPPORTED_TYPE param1, LPCSTR param2, HRESULT &hr );
                static LPCSTR UnmarshalArgs( CLR_RT_HeapBlock* pMngObj, UNSUPPORTED_TYPE param0, UNSUPPORTED_TYPE param1, HRESULT &hr );
                static INT32 UnmarshalArgs( CLR_RT_HeapBlock* pMngObj, UNSUPPORTED_TYPE param0, UNSUPPORTED_TYPE param1, LPCSTR param2, UINT32 * param3, HRESULT &hr );
                static LPCSTR UnmarshalPropertyArgs( CLR_RT_HeapBlock* pMngObj, UNSUPPORTED_TYPE param0, UNSUPPORTED_TYPE param1, HRESULT &hr );
                static INT32 MarshalArg( CLR_RT_HeapBlock* pMngObj, UNSUPPORTED_TYPE param0, LPCSTR param1, LPCSTR param2, HRESULT &hr );
                static INT32 MarshalArg( CLR_RT_HeapBlock* pMngObj, UNSUPPORTED_TYPE param0, UNSUPPORTED_TYPE param1, LPCSTR param2, HRESULT &hr );
                static INT32 MarshalArgs( CLR_RT_HeapBlock* pMngObj, UNSUPPORTED_TYPE param0, LPCSTR param1, LPCSTR param2, LPCSTR param3, LPCSTR param4, HRESULT &hr );
                static INT32 MarshalArgs( CLR_RT_HeapBlock* pMngObj, UNSUPPORTED_TYPE param0, UNSUPPORTED_TYPE param1, LPCSTR param2, LPCSTR param3, HRESULT &hr );
                static INT32 MarshalArgs( CLR_RT_HeapBlock* pMngObj, UNSUPPORTED_TYPE param0, UNSUPPORTED_TYPE param1, LPCSTR param2, LPCSTR param3, HRESULT &hr );
                static INT32 MarshalContainer( CLR_RT_HeapBlock* pMngObj, UNSUPPORTED_TYPE param0, UNSUPPORTED_TYPE param1, UINT32 param2, HRESULT &hr );
                static INT32 MarshalCloseContainer( CLR_RT_HeapBlock* pMngObj, UNSUPPORTED_TYPE param0, UNSUPPORTED_TYPE param1, HRESULT &hr );
                static INT32 BusReplyAcceptSession( CLR_RT_HeapBlock* pMngObj, UNSUPPORTED_TYPE param0, UNSUPPORTED_TYPE param1, HRESULT &hr );
                static void CloseMsg( CLR_RT_HeapBlock* pMngObj, UNSUPPORTED_TYPE param0, HRESULT &hr );
                static void Disconnect( CLR_RT_HeapBlock* pMngObj, UINT32 param0, HRESULT &hr );
                static void Sleep( CLR_RT_HeapBlock* pMngObj, UINT32 param0, HRESULT &hr );
                static void CreateBus( CLR_RT_HeapBlock* pMngObj, UINT32 * param0, HRESULT &hr );
                static INT32 MarshalObjectDescriptions( CLR_RT_HeapBlock* pMngObj, UNSUPPORTED_TYPE param0, HRESULT &hr );
                static INT32 MarshalDefaultProps( CLR_RT_HeapBlock* pMngObj, UNSUPPORTED_TYPE param0, HRESULT &hr );
                static UINT32 GetArgPtr( CLR_RT_HeapBlock* pMngObj, INT32 param0, HRESULT &hr );
                static LPCSTR GetArgString( CLR_RT_HeapBlock* pMngObj, INT32 param0, HRESULT &hr );
                static void UsePeerAuthentication( CLR_RT_HeapBlock* pMngObj, INT8 param0, HRESULT &hr );
                static void SetPassphrase( CLR_RT_HeapBlock* pMngObj, LPCSTR param0, HRESULT &hr );
                static INT32 AuthenticatePeer( CLR_RT_HeapBlock* pMngObj, UINT32 param0, LPCSTR param1, HRESULT &hr );
                static INT32 GetLocalGUID( CLR_RT_TypedArray_UINT8 param0, HRESULT &hr );
                static void RegisterObjects( LPCSTR param0, LPCSTR param1, UINT8 param2, INT32 param3, INT8 param4, INT8 param5, HRESULT &hr );
                static void PrintXML( LPCSTR param0, LPCSTR param1, UINT8 param2, INT32 param3, HRESULT &hr );
                static UINT32 AppMessageId( UINT32 param0, UINT32 param1, UINT32 param2, HRESULT &hr );
                static UINT32 BusMessageId( UINT32 param0, UINT32 param1, UINT32 param2, HRESULT &hr );
                static UINT32 PrxMessageId( UINT32 param0, UINT32 param1, UINT32 param2, HRESULT &hr );
                static UINT32 AppPropertyId( UINT32 param0, UINT32 param1, UINT32 param2, HRESULT &hr );
                static UINT32 BusPropertyId( UINT32 param0, UINT32 param1, UINT32 param2, HRESULT &hr );
                static UINT32 PrxPropertyId( UINT32 param0, UINT32 param1, UINT32 param2, HRESULT &hr );
            };
        }
    }
}
#endif  //_SPOT_ALLJOYN_NATIVE_MICROSOFT_SPOT_ALLJOYN_AJ_H_
