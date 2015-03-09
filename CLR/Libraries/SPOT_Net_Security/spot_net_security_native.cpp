////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Net_Security.h"


static const CLR_RT_MethodHandler method_lookup[] =
{
    Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::SecureServerInit___STATIC__I4__I4__I4__MicrosoftSPOTNativeSystemSecurityCryptographyX509CertificatesX509Certificate__SZARRAY_MicrosoftSPOTNativeSystemSecurityCryptographyX509CertificatesX509Certificate,
    Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::SecureClientInit___STATIC__I4__I4__I4__MicrosoftSPOTNativeSystemSecurityCryptographyX509CertificatesX509Certificate__SZARRAY_MicrosoftSPOTNativeSystemSecurityCryptographyX509CertificatesX509Certificate,
    Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::UpdateCertificates___STATIC__VOID__I4__MicrosoftSPOTNativeSystemSecurityCryptographyX509CertificatesX509Certificate__SZARRAY_MicrosoftSPOTNativeSystemSecurityCryptographyX509CertificatesX509Certificate,
    Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::SecureAccept___STATIC__VOID__I4__OBJECT,
    Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::SecureConnect___STATIC__VOID__I4__STRING__OBJECT,
    Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::SecureRead___STATIC__I4__OBJECT__SZARRAY_U1__I4__I4__I4,
    Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::SecureWrite___STATIC__I4__OBJECT__SZARRAY_U1__I4__I4__I4,
    Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::SecureCloseSocket___STATIC__I4__OBJECT,
    Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::ExitSecureContext___STATIC__I4__I4,
    Library_spot_net_security_native_Microsoft_SPOT_Net_Security_SslNative::DataAvailable___STATIC__I4__OBJECT,
};

const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_Net_Security =
{
    "Microsoft.SPOT.Net.Security", 
    0x1DE2C292,
    method_lookup
};

