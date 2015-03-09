using System;
using Microsoft.SPOT.Platform.Test;
using System.Net;
using System.Net.Sockets;

namespace Microsoft.SPOT.Platform.Tests
{
    public class SocketTools
    {
        static private int m_portCounter = 0;
        static public int nextPort
        {
            get
            {
                int rangeOfPorts = 65000;
                int startPort = 1024;
                int retPort = m_portCounter % rangeOfPorts + startPort;
                m_portCounter++;
                return retPort;
            }
        }

        static public long DottedDecimalToIp(byte a1, byte a2, byte a3, byte a4)
        {
            return (long)((ulong)a4 << 24 | (ulong)a3 << 16 | (ulong)a2 << 8 | (ulong)a1);
        }

        static public IPAddress ParseAddress(string ipString)
        {
            if (ipString == null)
                throw new ArgumentNullException("WsdIPAddress.ipString must not be null.");

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
                    // or a segment is longer than 3 characters or shiftIndex > 
                    // last bit position throw.
                    if (i == 0 || i - lastIndex > 3 || shiftIndex > 24)
                        throw new Exception("invalid address format (###.###.###.###) " 
                            + ipString);
                    else
                    {
                        i = i == length - 1 ? ++i : i;

                        octet = (ulong)(ToInt32(ipString.Substring(lastIndex, i - lastIndex)) 
                            & 0x00000000000000FF);
                        ipAddress = ipAddress + (ulong)((octet << shiftIndex) & mask);
                        lastIndex = i + 1;
                        shiftIndex += 8;
                        mask <<= 8;
                    }
            }
            return new IPAddress((long)ipAddress);
        }

        static public bool ArrayEquals(bool[] array1, bool[] array2)
        {
            if (array1.Length != array2.Length)
                return false;
            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                    return false;
            }
            return true;
        }

        static public bool ArrayEquals(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
                return false;
            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                    return false;
            }
            return true;
        }

        //--//
        
        static private int ToInt32(string number)
        {
            if (number == null)
                throw new ArgumentNullException("number", "Must not be null.");
            
            int result = 0;
            int digit = 0;
            int exp = 1;
            int length = number.Length;
            char[] num = number.ToCharArray();

            switch(length)
            {
                case 1 : exp =   1; break;
                case 2 : exp =  10; break;
                case 3 : exp = 100; break;
                default: throw new ArgumentException( 
                    "any octet in an IP address can be up to 3 digits long" );
            }
            for (int i = 0; i < length; ++i)
            {
                digit = (int)(num[i] - '0');

                // Make sure argument number is valid. If not this is not a format specifier
                if (digit < 0 || digit > 9)
                    throw new ArgumentOutOfRangeException("Format_Argument", number);

                result = result + (digit * exp);

                exp /= 10;
            }
            return result;
        }
    }
}
