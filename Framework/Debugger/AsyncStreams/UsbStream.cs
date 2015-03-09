////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;
using System.Management;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.SPOT.Debugger
{

    public class UsbStream : AsyncFileStream
    {
        // discovery keys
        static public readonly string InquiriesInterface = "InquiriesInterface";
        static public readonly string DriverVersion = "DriverVersion";

        // mandatory property keys                 
        static public readonly string DeviceHash = "DeviceHash";
        static public readonly string DisplayName = "DisplayName";

        // optional property keys 
        static public readonly string Manufacturer = "Manufacturer";
        static public readonly string Product = "Product";
        static public readonly string SerialNumber = "SerialNumber";
        static public readonly string VendorId = "VendorId";
        static public readonly string ProductId = "ProductId";

        internal const int c_DeviceStringBufferSize = 260;

        static internal Hashtable s_textProperties = new Hashtable();
        static internal Hashtable s_digitProperties = new Hashtable();

        static UsbStream()
        {
        }

        internal UsbStream(string port)
            : base(port, System.IO.FileShare.None)
        {
        }

        protected static void RetrieveProperties(string hash, ref PortDefinition pd, UsbStream s)
        {
            IDictionaryEnumerator dict;

            dict = s_textProperties.GetEnumerator();

            while (dict.MoveNext())
            {
                pd.Properties.Add(dict.Key, s.RetrieveStringFromDevice((int)dict.Value));
            }

            dict = s_digitProperties.GetEnumerator();

            while (dict.MoveNext())
            {
                pd.Properties.Add(dict.Key, s.RetrieveIntegerFromDevice((int)dict.Value));
            }
        }

        protected unsafe string RetrieveStringFromDevice(int controlCode)
        {
            int code = NativeMethods.ControlCode(NativeMethods.FILE_DEVICE_UNKNOWN, controlCode, NativeMethods.METHOD_BUFFERED, NativeMethods.FILE_ANY_ACCESS);

            string data;
            int read;
            byte[] buffer = new byte[c_DeviceStringBufferSize];

            fixed (byte* p = buffer)
            {
                if (!NativeMethods.DeviceIoControl(m_handle.DangerousGetHandle(), code, null, 0, p, buffer.Length, out read, null) || (read <= 0))
                {
                    data = null;
                }
                else
                {
                    if (read > (c_DeviceStringBufferSize - 2))
                    {
                        read = c_DeviceStringBufferSize - 2;
                    }

                    p[read] = 0;
                    p[read + 1] = 0;

                    data = new string((char*)p);
                }
            }

            return data;
        }

        protected unsafe int RetrieveIntegerFromDevice(int controlCode)
        {
            int code = NativeMethods.ControlCode(NativeMethods.FILE_DEVICE_UNKNOWN, controlCode, NativeMethods.METHOD_BUFFERED, NativeMethods.FILE_ANY_ACCESS);

            int read;
            int digits = 0;

            if (!NativeMethods.DeviceIoControl(m_handle.DangerousGetHandle(), code, null, 0, (byte*)&digits, sizeof(int), out read, null) || (read <= 0))
            {
                digits = -1;
            }

            return digits;
        }
    }
}
