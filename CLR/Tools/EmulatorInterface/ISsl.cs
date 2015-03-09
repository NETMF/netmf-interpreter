////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace Microsoft.SPOT.Emulator.Sockets.Security
{
    [StructLayout(LayoutKind.Sequential)]
    unsafe public struct DateTimeInfo
    {
        public uint  year;           /* year, AD                   */
        public uint  month;          /* 1 = January, 12 = December */
        public uint  day;            /* 1 = first of the month     */
        public uint  hour;           /* 0 = midnight, 12 = noon    */
        public uint  minute;         /* minutes past the hour      */
        public uint  second;         /* seconds in minute          */
        public uint  msec;           /* milliseconds in second     */

        /* These two fields help  */
        /* interpret the absolute */
        /* time meaning of the    */
        /* above values.          */
        public uint  dlsTime;            /* boolean; daylight savings time is in effect                      */
        public int   tzOffset;           /* signed int; difference in seconds imposed by timezone (from GMT) */
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct X509CertificateData
    {
        public fixed char Issuer[256];
        public fixed char Subject[256];
        public DateTimeInfo EffectiveDate;
        public DateTimeInfo ExpirationDate;   
    }

    [FlagsAttribute]
    public enum SslVerification
    {
        NoVerification      = 1,
        VerifyPeer          = 2,
        CertificateRequired = 4,
        VerifyClientOnce    = 8,
    }

    public delegate void RegisterTimeCallback(IntPtr dt);
    
    public interface ISslDriver
    {
        bool Initialize();
        bool Uninitialize();

        bool ServerInit ( int sslProtocols, int sslVerify, IntPtr certificate, int cert_len, IntPtr szCertPwd, ref int sslContextHandle );
        bool ClientInit ( int sslProtocols, int sslVerify, IntPtr certificate, int cert_len, IntPtr szCertPwd, ref int sslContextHandle );
        bool AddCertificateAuthority( int sslContextHandle, IntPtr certificate, int cert_len, IntPtr szCertPwd );
        void ClearCertificateAuthority( int sslContextHandle );
        bool ExitContext( int sslContextHandle );
        int  Accept     ( int socket, int sslContextHandle );
        int  Connect    ( int socket, IntPtr szTargetHost, int sslContextHandle );
        int  Write      ( int socket, IntPtr Data, int size );
        int  Read       ( int socket, IntPtr Data, int size );
        int  CloseSocket( int socket );
        void GetTime(IntPtr pdt);
        void RegisterTimeCallback(RegisterTimeCallback pfn);
        bool ParseCertificate(IntPtr certificate, int certLength, IntPtr szPwd, IntPtr certData);
        int  DataAvailable( int socket );
    }
}
