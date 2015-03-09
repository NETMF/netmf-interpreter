using System;
using System.Text;

namespace Microsoft.SPOT.Platform.Test
{
    class TestUtilities
    {
        public static long DottedDecimalToIp(byte a1, byte a2, byte a3, byte a4)
        {
            return (long)((ulong)a4 << 24 | (ulong)a3 << 16 | (ulong)a2 << 8 | (ulong)a1);
        }
    }
}
