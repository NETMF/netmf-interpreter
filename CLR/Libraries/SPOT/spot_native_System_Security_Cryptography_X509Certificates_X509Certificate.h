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


#ifndef _SPOT_NATIVE_SYSTEM_SECURITY_CRYPTOGRAPHY_X509CERTIFICATES_X509CERTIFICATE_H_
#define _SPOT_NATIVE_SYSTEM_SECURITY_CRYPTOGRAPHY_X509CERTIFICATES_X509CERTIFICATE_H_

namespace System
{
    namespace Security
    {
        namespace Cryptography
        {
            namespace X509Certificates
            {
                struct X509Certificate
                {
                    // Helper Functions to access fields of managed object
                    static UNSUPPORTED_TYPE& Get_m_certificate( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_UNSUPPORTED_TYPE( pMngObj, Library_spot_native_System_Security_Cryptography_X509Certificates_X509Certificate::FIELD__m_certificate ); }

                    static LPCSTR& Get_m_password( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_LPCSTR( pMngObj, Library_spot_native_System_Security_Cryptography_X509Certificates_X509Certificate::FIELD__m_password ); }

                    static LPCSTR& Get_m_issuer( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_LPCSTR( pMngObj, Library_spot_native_System_Security_Cryptography_X509Certificates_X509Certificate::FIELD__m_issuer ); }

                    static LPCSTR& Get_m_subject( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_LPCSTR( pMngObj, Library_spot_native_System_Security_Cryptography_X509Certificates_X509Certificate::FIELD__m_subject ); }

                    static UNSUPPORTED_TYPE& Get_m_effectiveDate( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_UNSUPPORTED_TYPE( pMngObj, Library_spot_native_System_Security_Cryptography_X509Certificates_X509Certificate::FIELD__m_effectiveDate ); }

                    static UNSUPPORTED_TYPE& Get_m_expirationDate( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_UNSUPPORTED_TYPE( pMngObj, Library_spot_native_System_Security_Cryptography_X509Certificates_X509Certificate::FIELD__m_expirationDate ); }

                    // Declaration of stubs. These functions are implemented by Interop code developers
                    static void ParseCertificate( CLR_RT_TypedArray_UINT8 param0, LPCSTR param1, LPCSTR * param2, LPCSTR * param3, UNSUPPORTED_TYPE * param4, UNSUPPORTED_TYPE param5, HRESULT &hr );
                };
            }
        }
    }
}
#endif  //_SPOT_NATIVE_SYSTEM_SECURITY_CRYPTOGRAPHY_X509CERTIFICATES_X509CERTIFICATE_H_
