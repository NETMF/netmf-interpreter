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



#include "SPOT.h"



HRESULT Library_spot_native_System_Security_Cryptography_X509Certificates_X509Certificate::ParseCertificate___STATIC__VOID__SZARRAY_U1__STRING__BYREF_STRING__BYREF_STRING__BYREF_mscorlibSystemDateTime__BYREF_mscorlibSystemDateTime( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* arrData    = stack.Arg0().DereferenceArray(); 
    CLR_UINT8*              certBytes;
    CLR_RT_HeapBlock        hbIssuer;
    CLR_RT_HeapBlock        hbSubject;
    CLR_RT_ProtectFromGC    gc1( hbIssuer  );
    CLR_RT_ProtectFromGC    gc2( hbSubject );
    X509CertData            cert;
    CLR_INT64*              val;
    CLR_INT64               tzOffset;
    SYSTEMTIME              st;
    INT32                   standardBias;
    CLR_RT_HeapBlock*       hbPwd     = stack.Arg1().DereferenceString();
    LPCSTR                  szPwd;


    FAULT_ON_NULL_ARG(hbPwd);

    szPwd = hbPwd->StringText();

    CLR_RT_Memory::ZeroFill( &cert, sizeof(cert) );

    FAULT_ON_NULL(arrData);

    certBytes = arrData->GetFirstElement();

    if(!SSL_ParseCertificate( (const char*)certBytes, arrData->m_numOfElements, szPwd, &cert )) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_String::CreateInstance( hbIssuer, cert.Issuer ));
    TINYCLR_CHECK_HRESULT(hbIssuer.StoreToReference( stack.Arg2(), 0 ));

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_String::CreateInstance( hbSubject, cert.Subject ));
    TINYCLR_CHECK_HRESULT(hbSubject.StoreToReference( stack.Arg3(), 0 ));

    st.wYear         = cert.EffectiveDate.year;
    st.wMonth        = cert.EffectiveDate.month;
    st.wDay          = cert.EffectiveDate.day;
    st.wHour         = cert.EffectiveDate.hour;
    st.wMinute       = cert.EffectiveDate.minute;
    st.wSecond       = cert.EffectiveDate.second;
    st.wMilliseconds = cert.EffectiveDate.msec;

    standardBias     = Time_GetTimeZoneOffset();
    standardBias    *= TIME_CONVERSION__ONEMINUTE;

    val = Library_corlib_native_System_DateTime::GetValuePtr( stack.Arg4() );
    *val = Time_FromSystemTime( &st );

    tzOffset = cert.EffectiveDate.tzOffset;

    // adjust for timezone differences
    if(standardBias != tzOffset)
    {
        *val += tzOffset - standardBias; 
    }

    st.wYear         = cert.ExpirationDate.year;
    st.wMonth        = cert.ExpirationDate.month;
    st.wDay          = cert.ExpirationDate.day;
    st.wHour         = cert.ExpirationDate.hour;
    st.wMinute       = cert.ExpirationDate.minute;
    st.wSecond       = cert.ExpirationDate.second;
    st.wMilliseconds = cert.ExpirationDate.msec;
    
    val = Library_corlib_native_System_DateTime::GetValuePtr( stack.ArgN( 5 ) );
    *val = Time_FromSystemTime( &st );

    tzOffset = cert.ExpirationDate.tzOffset;
    
    if(standardBias != tzOffset)
    {
       *val += tzOffset - standardBias; 
    }

    TINYCLR_NOCLEANUP();
}
