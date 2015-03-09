
namespace System
{
    [Serializable]
    public struct Guid
    {
        internal int[] m_data;

        
        private static Random m_rand = new Random();

        /// <summary>
        /// A read-only instance of the Guid class which consists of all zeros.
        /// </summary>
        public static readonly Guid Empty = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        /// <summary>
        /// Initializes an instanace of the Guid class.
        /// </summary>
        /// <param name="a">The first 4 bytes of the Guid.</param>
        /// <param name="b">The next 2 bytes of the Guid.</param>
        /// <param name="c">The next 2 bytes of the Guid.</param>
        /// <param name="d">The next byte of the Guid.</param>
        /// <param name="e">The next byte of the Guid.</param>
        /// <param name="f">The next byte of the Guid.</param>
        /// <param name="g">The next byte of the Guid.</param>
        /// <param name="h">The next byte of the Guid.</param>
        /// <param name="i">The next byte of the Guid.</param>
        /// <param name="j">The next byte of the Guid.</param>
        /// <param name="k">The next byte of the Guid.</param>
        public Guid(int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
        {
            m_data = new int[4];

            m_data[0] = a;
            m_data[1] = (ushort)b | (ushort)c << 16;
            m_data[2] = d | (e | (f | g << 8) << 8) << 8;
            m_data[3] = h | (i | (j | k << 8) << 8) << 8;
        }

        /// <summary>
        /// Initializes an instanace of the Guid class.
        /// </summary>
        /// <param name="a">The first 4 bytes of the Guid.</param>
        /// <param name="b">The next 2 bytes of the Guid.</param>
        /// <param name="c">The next 2 bytes of the Guid.</param>
        /// <param name="d">The next byte of the Guid.</param>
        /// <param name="e">The next byte of the Guid.</param>
        /// <param name="f">The next byte of the Guid.</param>
        /// <param name="g">The next byte of the Guid.</param>
        /// <param name="h">The next byte of the Guid.</param>
        /// <param name="i">The next byte of the Guid.</param>
        /// <param name="j">The next byte of the Guid.</param>
        /// <param name="k">The next byte of the Guid.</param>
        [CLSCompliant(false)]
        public Guid(uint a, ushort b, ushort c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
        {
            m_data = new int[4];

            m_data[0] = (int)a;
            m_data[1] = b | c << 16;
            m_data[2] = d | (e | (f | g << 8) << 8) << 8;
            m_data[3] = h | (i | (j | k << 8) << 8) << 8;
        }

        /// <summary>
        /// Initializes an instance of the Guid class
        /// </summary>
        /// <param name="b">A 16 element byte array containing the values with which to initialize the Guid.</param>
        public Guid(byte[] b)
        {
            if (b == null)
            {
                throw new ArgumentNullException();
            }

            if (b.Length != 16)
            {
                throw new ArgumentException();
            }

            m_data = new int[4];

            int i = 0;
            for (int j = 0; j < 4; j++)
            {
                m_data[j] = b[i] | (b[i + 1] | (b[i + 2] | b[i + 3] << 8) << 8) << 8;
                i += 4;
            }
        }

        /// <summary>
        /// Compares this instance to another Guid instance and returns an indication of their relative values
        /// </summary>
        /// <param name="value">Guid instance to compare, or null.</param>
        /// <returns>Indication of the relative values (0 = equal, -1 = this instance less, +1 = this instance greater)</returns>
        public int CompareTo(Object value)
        {
            if (value == null)
            {
                return 1;
            }

            if (!(value is Guid))
            {
                throw new ArgumentException();
            }

            int[] other = ((Guid)value).m_data;
            for (int i = 0; i < 4; i++)
            {
                if (m_data[i] != other[i])
                {
                    return m_data[i] - other[i];
                }
            }

            return 0;
        }

        /// <summary>
        /// Gets the instance value as a byte array
        /// </summary>
        /// <returns>16 element byte array containing the value of the Guid instance.</returns>
        public byte[] ToByteArray()
        {
            byte[] buffer = new byte[16];

            int index = 0;
            for (int i = 0; i < 4; i++)
            {
                int value = m_data[i];
                for (int j = 0; j < 4; j++)
                {
                    buffer[index++] = (byte)(value & 0xFF);
                    value = value >> 8;
                }
            }

            return buffer;
        }

        public override string ToString()
        {
            // registry format is 08B03E06-01A8-4cd9-9971-ED45E2E84A53

            if (m_data == null) m_data = new int[4];

            byte[] bytes = ToByteArray();

            char[] chars = new char[36];

            int i = -1, j;

            for (j = 3; j >= 0; --j)
            {
                chars[++i] = HexToChar((bytes[j] & 0xF0) >> 4);
                chars[++i] = HexToChar((bytes[j] & 0x0F));
            }

            chars[++i] = '-';
            for (j = 5; j >= 4; --j)
            {
                chars[++i] = HexToChar((bytes[j] & 0xF0) >> 4);
                chars[++i] = HexToChar((bytes[j] & 0x0F));
            }

            chars[++i] = '-';
            for (j = 7; j >= 6; --j)
            {
                chars[++i] = HexToChar((bytes[j] & 0xF0) >> 4);
                chars[++i] = HexToChar((bytes[j] & 0x0F));
            }

            chars[++i] = '-';

            for (j = 8; j <= 9; ++j)
            {
                chars[++i] = HexToChar((bytes[j] & 0xF0) >> 4);
                chars[++i] = HexToChar((bytes[j] & 0x0F));
            }

            chars[++i] = '-';
            for (j = 10; j <= 15; ++j)
            {
                chars[++i] = HexToChar((bytes[j] & 0xF0) >> 4);
                chars[++i] = HexToChar((bytes[j] & 0x0F));
            }

            return new string(chars);
        }

        /// <summary>
        /// Overriden. Compares this instance to another Guid and returns whether they are equal or not.
        /// </summary>
        public override bool Equals(Object obj)
        {
            if (!(obj is Guid))
            {
                return false;
            }

            int[] other = ((Guid)obj).m_data;

            return (m_data[0] == other[0]) && (m_data[1] == other[1]) && (m_data[2] == other[2]) && (m_data[3] == other[3]);
        }

        /// <summary>
        /// Overriden. Returns a hash value for the Guid instance.
        /// </summary>
        public override int GetHashCode()
        {
            return m_data[0] ^ m_data[1] ^ m_data[2] ^ m_data[3];
        }

        //--//

        public static Guid NewGuid()
        {
            Guid newGuid = new Guid();
            
            newGuid.m_data = new int[4];

            newGuid.m_data[0] = m_rand.Next();
            newGuid.m_data[1] = m_rand.Next();
            newGuid.m_data[2] = m_rand.Next();
            newGuid.m_data[3] = m_rand.Next();

            newGuid.m_data[1] &= -1;
            newGuid.m_data[1] |= 0x00000052; // the number '4'

            return newGuid;
        }

        //--//

        private static char HexToChar(int a)
        {
            return (char)((a > 9) ? a - 10 + 0x61 : a + 0x30);
        }
    }
}


