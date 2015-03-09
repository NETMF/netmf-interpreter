////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using Microsoft.Win32.SafeHandles;
using Microsoft.SPOT.Emulator.Com;
using Microsoft.SPOT.Emulator.Sockets;
using Microsoft.SPOT.Emulator.PKCS11;

namespace Microsoft.SPOT.Emulator.Sockets.Security
{
    internal class SslDriver : HalDriver<ISslDriver>, ISslDriver
    {
        internal class SslData
        {
            internal SslProtocols    m_sslVersion;
            internal SslVerification m_sslVerify;
            internal X509Certificate2 m_cert;
            internal X509Certificate2Collection m_caCollection = new X509Certificate2Collection();

            internal IntPtr  m_hSecCtx = IntPtr.Zero;
            internal IntPtr  m_hCred   = IntPtr.Zero;
        }

        internal class SslStreamData
        {
            internal enum SslAsyncState
            {
                Init,
                Processing,
                Finished,
                InUse,
                Failed,
            }

            internal class SslReadData
            {
                internal SslReadData(int socket, byte[] data)
                {
                    Socket = socket;
                    ManagedBuffer   = data;
                    Length = 0;
                }

                internal byte[] ManagedBuffer;
                internal int Socket;
                internal int Length;
            }

            internal SslStream m_stream;
            internal SslAsyncState m_state = SslAsyncState.Init;
            internal object m_asyncData = null;
        }

        Dictionary<int, SslData> _sslDataCollection = new Dictionary<int, SslData>();
        Dictionary<int, SslStreamData> _sslStreams = new Dictionary<int, SslStreamData>();

        RegisterTimeCallback _timeFunc;
        SocketsDriver _socketsDriver;

        int _sslDataCollectionIndex = 0;

        private bool GetSslData(int socketIndex, out SslStreamData sslData)
        {
            sslData = null;

            lock (_sslStreams)
            {
                if (!_sslStreams.ContainsKey(socketIndex)) return false;

                sslData = _sslStreams[socketIndex];
            }
            return true;
        }


        public SslDriver(SocketsDriver sockDriver)
        {
            _socketsDriver = sockDriver;
        }
        
        public bool Initialize()
        {
            return true;
        }

        public bool Uninitialize()
        {
            return true;
        }

        internal X509Certificate2 CreateCert(IntPtr certificate, int cert_len, string pwd)
        {
            X509Certificate2 cert = null;

            unsafe
            {
                byte[] data = new byte[cert_len];

                Marshal.Copy(certificate, data, 0, cert_len);

                if(string.IsNullOrEmpty(pwd))
                {
                    cert = new X509Certificate2(data);
                }
                else
                {
                    cert = new X509Certificate2(data, pwd, X509KeyStorageFlags.Exportable);
                }
            }

            return cert;
        }

        internal bool Init(int sslMode, int sslVerify, IntPtr certificate, int cert_len, IntPtr szCertPwd, ref int sslContextHandle)
        {
            SslData data = new SslData();

            data.m_sslVersion = (SslProtocols.None);

            if ((sslMode & 0x18) != 0)
            {
                data.m_sslVersion = SslProtocols.Default;
            }
            else if ((sslMode & 0x10) != 0)
            {
                data.m_sslVersion = SslProtocols.Tls;
            }
            else if ((sslMode & 0x8) != 0)
            {
                data.m_sslVersion = SslProtocols.Ssl3;
            }
            else if ((sslMode & 0x4) != 0)
            {
                data.m_sslVersion = SslProtocols.Ssl2;
            }

            data.m_sslVerify  = (SslVerification)sslVerify;
            if (certificate != IntPtr.Zero)
            {
                if (cert_len == 4)
                {
                    SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(sslContextHandle);

                    if (ctx == null) return false;

                    CryptokiObject obj = ctx.ObjectCtx.GetObject(Marshal.ReadInt32(certificate));

                    if (obj == null || obj.Type != CryptokiObjectType.Cert) return false;

                    data.m_cert = obj.Data as X509Certificate2;
                }
                else
                {
                    data.m_cert = CreateCert(certificate, cert_len, Marshal.PtrToStringAnsi(szCertPwd));
                }
            }

            lock (this)
            {
                _sslDataCollection[_sslDataCollectionIndex] = data;

                sslContextHandle = _sslDataCollectionIndex;

                System.Threading.Interlocked.Increment(ref _sslDataCollectionIndex);
            }

            return true;
        }

        public bool ServerInit ( int sslMode, int sslVerify, IntPtr certificate, int cert_len, IntPtr szCertPwd, ref int sslContextHandle )
        {
            return Init(sslMode, sslVerify, certificate, cert_len, szCertPwd, ref sslContextHandle);
        }

        public bool ClientInit ( int sslMode, int sslVerify, IntPtr certificate, int cert_len, IntPtr szCertPwd, ref int sslContextHandle )
        {
            return Init(sslMode, sslVerify, certificate, cert_len, szCertPwd, ref sslContextHandle);
        }

        public bool AddCertificateAuthority( int sslContextHandle, IntPtr certificate, int cert_len, IntPtr szCertPwd )
        {
            X509Certificate cert = CreateCert( certificate, cert_len, Marshal.PtrToStringAnsi(szCertPwd) );

            if (cert != null)
            {
                _sslDataCollection[sslContextHandle].m_caCollection.Add(cert);
            }

            return true;
        }

        public void ClearCertificateAuthority( int sslContextHandle )
        {
            _sslDataCollection[sslContextHandle].m_cert = null;
            _sslDataCollection[sslContextHandle].m_caCollection.Clear();
        }

        public bool ExitContext( int sslContextHandle )
        {
            _sslDataCollection.Remove(sslContextHandle);

            return true;
        }

        public int Accept( int socket, int sslContextHandle )
        {
            SslStreamData ssd;

            if (GetSslData(socket, out ssd))
            {
                switch (ssd.m_state)
                {
                    case SslStreamData.SslAsyncState.Processing:
                        return (int)SocketError.WouldBlock;

                    case SslStreamData.SslAsyncState.Finished:
                        ssd.m_state = SslStreamData.SslAsyncState.InUse;
                        _socketsDriver.SetSslSocket(socket);
                        return (int)SocketError.Success;

                    case SslStreamData.SslAsyncState.Failed:
                        ssd.m_state = SslStreamData.SslAsyncState.Init;
                        Console.Error.WriteLine("Accept Failed...");
                        return (int)SocketError.SocketError;

                    case SslStreamData.SslAsyncState.InUse:
                        Console.Error.WriteLine("Accept InUse...");
                        return (int)SocketError.SocketError;
                }
            }
            else
            {
                ssd = new SslStreamData();

                lock (_sslStreams)
                {
                    _sslStreams[socket] = ssd;
                }
            }

            ssd.m_state = SslStreamData.SslAsyncState.Processing;

            try
            {
                Socket sock = null;

                if(!_socketsDriver.GetSocket(socket, out sock)) return (int)SocketError.SocketError;

                NetworkStream ns = new NetworkStream(sock);
                SslStream stream = new SslStream(ns, true);

                ssd.m_stream = stream;

                SslData sd = _sslDataCollection[sslContextHandle];

                stream.BeginAuthenticateAsServer(sd.m_cert, sd.m_sslVerify == SslVerification.CertificateRequired, sd.m_sslVersion, false, new AsyncCallback(EndSslAccept), socket);
            }
            catch (Exception ae)
            {
                ssd.m_state = SslStreamData.SslAsyncState.Init;
                Console.Error.WriteLine("BeginAuthenticateAsServer " + ae.Message);
                return (int)SocketError.SocketError;
            }

            return (int)SocketError.WouldBlock;
        }

        void EndSslAccept(IAsyncResult iar)
        {
            int sock = (int)iar.AsyncState;

            SslStreamData ssd;

            if (GetSslData(sock, out ssd))
            {
                try
                {
                    ssd.m_stream.EndAuthenticateAsServer(iar);
                    ssd.m_state = SslStreamData.SslAsyncState.Finished;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("EndAuthenticateAsServer  " + e.Message);
                    ssd.m_state = SslStreamData.SslAsyncState.Failed;
                }
            }

            _socketsDriver.ClearSocketEvent(sock, true);
            _socketsDriver.ClearSocketEvent(sock, false);
        }

        public int Connect( int socket, IntPtr szTargetHost, int sslContextHandle )
        {
            SslStreamData ssd;

            if (GetSslData(socket, out ssd))
            {
                switch (ssd.m_state)
                {
                    case SslStreamData.SslAsyncState.Processing:
                        return (int)SocketError.WouldBlock;

                    case SslStreamData.SslAsyncState.Finished:
                        ssd.m_state = SslStreamData.SslAsyncState.InUse;
                        _socketsDriver.SetSslSocket(socket);
                        return (int)SocketError.Success;

                    case SslStreamData.SslAsyncState.Failed:
                        ssd.m_state = SslStreamData.SslAsyncState.Init;
                        Console.Error.WriteLine("Connect Failed...");
                        return (int)SocketError.SocketError;

                    case SslStreamData.SslAsyncState.InUse:
                        Console.Error.WriteLine("Connect InUse...");
                        return (int)SocketError.Success;
                }
            }
            else
            {
                ssd = new SslStreamData();

                lock (_sslStreams)
                {
                    _sslStreams[socket] = ssd;
                }
            }

            ssd.m_state = SslStreamData.SslAsyncState.Processing;

            try
            {
                Socket sock = null;

                if (!_socketsDriver.GetSocket(socket, out sock)) return (int)SocketError.SocketError;

                NetworkStream ns = new NetworkStream(sock);
                SslStream stream = new SslStream( ns, true );

                ssd.m_stream = stream;

                SslData sd = _sslDataCollection[sslContextHandle];

                string targHost = Marshal.PtrToStringAnsi(szTargetHost);

                X509CertificateCollection certs = new X509CertificateCollection();

                if (sd.m_cert != null)
                {
                    certs.Add(sd.m_cert);
                }

                IAsyncResult iar = stream.BeginAuthenticateAsClient(targHost, certs, sd.m_sslVersion, false, new AsyncCallback(EndSslConnect), socket);

            }
            catch (Exception ae)
            {
                ssd.m_state = SslStreamData.SslAsyncState.Init;
                Console.Error.WriteLine("BeginAuthenticateAsClient " + ae.Message);
                return (int)SocketError.SocketError;
            }

            return (int)SocketError.WouldBlock;
        }

        void EndSslConnect(IAsyncResult iar)
        {
            int sock = (int)iar.AsyncState;

            SslStreamData ssd;

            if (GetSslData(sock, out ssd))
            {
                try
                {
                    ssd.m_stream.EndAuthenticateAsClient(iar);
                    ssd.m_state = SslStreamData.SslAsyncState.Finished;
                }
                catch(Exception e)
                {
                    Console.Error.WriteLine("EndAuthenticateAsClient " + e.Message);
                    ssd.m_state = SslStreamData.SslAsyncState.Failed;
                }
            }

            _socketsDriver.ClearSocketEvent(sock, true);
            _socketsDriver.ClearSocketEvent(sock, false);
        }

        public int Write( int socket, IntPtr Data, int size )
        {
            SslStreamData ssd;

            if (!GetSslData(socket, out ssd)) return (int)SocketError.SocketError;

            try
            {
                byte[] data = new byte[size];

                Marshal.Copy(Data, data, 0, size);

                ssd.m_stream.Write(data, 0, size);

                ssd.m_stream.Flush();
            }
            catch(Exception e)
            {
                Console.Error.WriteLine("Write Failed... " + e.Message);
                return (int)SocketError.SocketError;
            }

            _socketsDriver.ClearSocketEvent(socket, false);

            return size;
        }


        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public int Read(int socket, IntPtr Data, int size)
        {
            int len = 0;
            SslStreamData ssd;

            if (!GetSslData(socket, out ssd)) return (int)SocketError.SocketError;

            try
            {
                byte[] managedBuffer = new byte[size];

                len = ssd.m_stream.Read(managedBuffer, 0, size);

                if (len > 0)
                {
                    Marshal.Copy(managedBuffer, 0, Data, len);
                }
                else
                {
                    _socketsDriver.ClearSocketEvent(socket, true);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Write Failed... " + e.Message);
                return (int)SocketError.SocketError;
            }

            return len;
        }

        public int CloseSocket( int socket )
        {
            lock (_sslStreams)
            {
                if (!_sslStreams.ContainsKey(socket)) return (int)SocketError.Success;

                try
                {
                    _sslStreams[socket].m_stream.Close();
                    _sslStreams.Remove(socket);

                    _socketsDriver.UnregisterSocket(socket);
                }
                catch(Exception e)
                {
                    Console.Error.WriteLine("Close failed... " + e.Message);
                    return (int)SocketError.SocketError;
                }
            }

            return 0;
        }

        public void GetTime(IntPtr pdt)
        {
            if( _timeFunc == null)
            {
                DateTimeInfo dti = new DateTimeInfo();

                dti = (DateTimeInfo)Marshal.PtrToStructure(pdt, typeof(DateTimeInfo));
                    
                DateTime dt = DateTime.Now;

                dti.year   = (uint)dt.Year;
                dti.month  = (uint)dt.Month;
                dti.day    = (uint)dt.Day;
                dti.hour   = (uint)dt.Hour;
                dti.minute = (uint)dt.Minute;
                dti.second = (uint)dt.Second;
                dti.msec   = (uint)dt.Millisecond;

                dti.dlsTime  = (uint)(dt.IsDaylightSavingTime() ? 1 : 0);
                dti.tzOffset = (int)TimeZone.CurrentTimeZone.GetUtcOffset(dt).TotalSeconds;

                Marshal.StructureToPtr(dti, pdt, false);
            }
            else
            {
                _timeFunc(pdt);
            }
        }

        public void RegisterTimeCallback(RegisterTimeCallback pfn)
        {
            _timeFunc = pfn;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class _DATE_TIME_INFO
        {
            internal uint year;           /* year, AD                   */
            internal uint month;          /* 1 = January, 12 = December */
            internal uint day;            /* 1 = first of the month     */
            internal uint hour;           /* 0 = midnight, 12 = noon    */
            internal uint minute;         /* minutes past the hour      */
            internal uint second;         /* seconds in minute          */
            internal uint msec;           /* milliseconds in second     */

            internal uint dlsTime;        /* boolean; daylight savings time is in effect                      */
            internal int  tzOffset;       /* signed int; difference in seconds imposed by timezone (from GMT) */
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class _X509CertData
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            internal byte[] Issuer = new byte[256];
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            internal byte[] Subject = new byte[256];
            internal _DATE_TIME_INFO  EffectiveDate = new _DATE_TIME_INFO();
            internal _DATE_TIME_INFO  ExpirationDate = new _DATE_TIME_INFO();   
        }

        private _DATE_TIME_INFO ConvertToDateTimeInfo(DateTime dt)
        {
            _DATE_TIME_INFO dti = new _DATE_TIME_INFO();

            dti.year = (uint)dt.Year;
            dti.month = (uint)dt.Month;
            dti.day = (uint)dt.Day;
            dti.hour = (uint)dt.Hour;
            dti.minute = (uint)dt.Minute;
            dti.second = (uint)dt.Second;
            dti.msec = (uint)dt.Millisecond;

            dti.dlsTime = dt.IsDaylightSavingTime() ? 1u : 0u;
            dti.tzOffset = (int)Math.Round(TimeZone.CurrentTimeZone.GetUtcOffset(dt).TotalSeconds, 0);

            return dti;
        }

        public bool ParseCertificate(IntPtr certificate, int certLength, IntPtr szPwd, IntPtr certData)
        {
            try
            {
                X509Certificate cert = CreateCert(certificate, certLength, Marshal.PtrToStringAnsi(szPwd));

                _X509CertData cd = new _X509CertData();

                byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(cert.Issuer);

                Array.Copy(bytes, cd.Issuer, bytes.Length);

                bytes = System.Text.UTF8Encoding.UTF8.GetBytes(cert.Subject);

                Array.Copy(bytes, cd.Subject, bytes.Length);

                string date = cert.GetEffectiveDateString();

                DateTime dt = DateTime.Parse(date);

                cd.EffectiveDate = ConvertToDateTimeInfo(dt);

                date = cert.GetExpirationDateString();

                dt = DateTime.Parse(date);

                cd.ExpirationDate = ConvertToDateTimeInfo(dt);

                Marshal.StructureToPtr(cd, certData, true);
            }
            catch(Exception e)
            {
                Console.Error.WriteLine("ParseCertificate exception: " + e.Message);
                return false;
            }
            return true;
        }

        public int DataAvailable( int socket )
        {
            SslStreamData ssd;
            Socket sock = null;

            if (!GetSslData(socket, out ssd)) return (int)SocketError.SocketError;

            if(!_socketsDriver.GetSocket(socket, out sock)) return (int)SocketError.SocketError;

            return sock.Poll(0,SelectMode.SelectRead) ? 1024 : 0;
        }
    }    
}
