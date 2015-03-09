////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{
    using System.Net.Sockets;
    using System.Globalization;
    using System.Text;
    using Microsoft.SPOT.Net.NetworkInformation;
    /// <devdoc>
    ///    <para>Provides an internet protocol (IP) address.</para>
    /// </devdoc>
    [Serializable]
    public class IPAddress
    {

        public static readonly IPAddress Any = new IPAddress(0x0000000000000000);
        public static readonly IPAddress Loopback = new IPAddress(0x000000000100007F);
        internal long m_Address;

        public IPAddress(long newAddress)
        {
            if (newAddress < 0 || newAddress > 0x00000000FFFFFFFF)
            {
                throw new ArgumentOutOfRangeException();
            }

            m_Address = newAddress;
        }

        public IPAddress(byte[] newAddressBytes)
            : this(((((newAddressBytes[3] << 0x18) | (newAddressBytes[2] << 0x10)) | (newAddressBytes[1] << 0x08)) | newAddressBytes[0]) & ((long)0xFFFFFFFF))
        {
        }

        public override bool Equals(object obj)
        {
            IPAddress addr = obj as IPAddress;

            if (obj == null) return false;

            return this.m_Address == addr.m_Address;
        }

        public byte[] GetAddressBytes()
        {
            return new byte[]
            {
                (byte)(m_Address),
                (byte)(m_Address >> 8),
                (byte)(m_Address >> 16),
                (byte)(m_Address >> 24)
            };
        }

        public static IPAddress Parse(string ipString)
        {
            if (ipString == null)
                throw new ArgumentNullException();

            ulong ipAddress = 0L;
            int lastIndex = 0;
            int shiftIndex = 0;
            ulong mask = 0x00000000000000FF;
            ulong octet = 0L;
            int length = ipString.Length;

            for (int i = 0; i < length; ++i)
            {
                // Parse to '.' or end of IP address
                if (ipString[i] == '.' || i == length - 1)
                    // If the IP starts with a '.'
                    // or a segment is longer than 3 characters or shiftIndex > last bit position throw.
                    if (i == 0 || i - lastIndex > 3 || shiftIndex > 24)
                    {
                        throw new ArgumentException();
                    }
                    else
                    {
                        i = i == length - 1 ? ++i : i;
                        octet = (ulong)(ConvertStringToInt32(ipString.Substring(lastIndex, i - lastIndex)) & 0x00000000000000FF);
                        ipAddress = ipAddress + (ulong)((octet << shiftIndex) & mask);
                        lastIndex = i + 1;
                        shiftIndex = shiftIndex + 8;
                        mask = (mask << 8);
                    }
            }

            return new IPAddress((long)ipAddress);
        }

        public override string ToString()
        {
            return ((byte)(m_Address)).ToString() +
                    "." +
                    ((byte)(m_Address >> 8)).ToString() +
                    "." +
                    ((byte)(m_Address >> 16)).ToString() +
                    "." +
                    ((byte)(m_Address >> 24)).ToString();
        }

        //--//
        ////////////////////////////////////////////////////////////////////////////////////////
        // this method ToInt32 is part of teh Convert class which we will bring over later
        // at that time we will get rid of this code
        //

        /// <summary>
        /// Converts the specified System.String representation of a number to an equivalent
        /// 32-bit signed integer.
        /// </summary>
        /// <param name="value">A System.String containing a number to convert.</param>
        /// <returns>
        /// A 32-bit signed integer equivalent to the value of value.-or- Zero if value
        /// is null.
        /// </returns>
        /// <exception cref="System.OverflowException">
        /// Value represents a number less than System.Int32.MinValue or greater than
        /// System.Int32.MaxValue.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// The value parameter is null.
        /// </exception>
        /// <exception cref="System.FormatException">
        /// Value does not consist of an optional sign followed by a sequence of digits
        /// (zero through nine).
        /// </exception>
        private static int ConvertStringToInt32(string value)
        {
            char[] num = value.ToCharArray();
            int result = 0;

            bool isNegative = false;
            int signIndex = 0;

            if (num[0] == '-')
            {
                isNegative = true;
                signIndex = 1;
            }
            else if (num[0] == '+')
            {
                signIndex = 1;
            }

            int exp = 1;
            for (int i = num.Length - 1; i >= signIndex; i--)
            {
                if (num[i] < '0' || num[i] > '9')
                {
                    throw new ArgumentException();
                }

                result += ((num[i] - '0') * exp);
                exp *= 10;
            }

            return (isNegative) ? (-1 * result) : result;
        }

        // this method ToInt32 is part of teh Convert class which we will bring over later
        ////////////////////////////////////////////////////////////////////////////////////////


        public static IPAddress GetDefaultLocalAddress()
        {
            // Special conditions are implemented here because of a ptoblem with GetHostEntry
            // on the digi device and NetworkInterface from the emulator.
            // In the emulator we must use GetHostEntry.
            // On the device and Windows NetworkInterface works and is preferred.
            try
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                int cnt = interfaces.Length;
                for (int i = 0; i < cnt; i++)
                {
                    NetworkInterface ni = interfaces[i];

                    if (ni.IPAddress != "0.0.0.0" && ni.SubnetMask != "0.0.0.0")
                    {
                        return IPAddress.Parse(ni.IPAddress);
                    }
                }
            }
            catch
            {
            }

            try
            {
                IPAddress localAddress = null;
                IPHostEntry hostEntry = Dns.GetHostEntry("");

                int cnt = hostEntry.AddressList.Length;
                for (int i = 0; i < cnt; ++i)
                {
                    if ((localAddress = hostEntry.AddressList[i]) != null)
                    {
                        if(localAddress.m_Address != 0)
                        {
                            return localAddress;
                        }
                    }
                }
            }
            catch
            {
            }

            return IPAddress.Any;
        }
        
        
    } // class IPAddress
} // namespace System.Net


