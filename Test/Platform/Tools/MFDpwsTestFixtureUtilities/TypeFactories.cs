using System;
using Microsoft.SPOT;

namespace MFDpwsTestFixtureUtilities
{
    public class DateTimeFactory
    {
        private static DateTime[] m_Objects = 
        { 
            DateTime.MinValue, 
            DateTime.MaxValue, 
            DateTime.Today, //Midnight
            DateTime.Today.AddHours(5.0 + 3.1415926535 ), // Morning
            DateTime.Today.AddHours(17.0 + 3.1415926535 ), // Evening
            DateTime.Today.AddHours(12.0), //Noon
            DateTime.Today.AddDays(-1.0), // Yesterday
            DateTime.Today.AddDays(1.0), // Tomorrow
            DateTime.Now, 
            DateTime.UtcNow 
        };

        private static int m_NextObject = -1;

        public static System.DateTime GetObject()
        {
            m_NextObject++;

            if (m_NextObject == m_Objects.Length)
            {
                m_NextObject = 0;
            }

            return m_Objects[m_NextObject];
        }

        public static System.DateTime[] GetArray()
        {
            return m_Objects;  
        }
    }

    public class TimeSpanFactory
    {
        private static TimeSpan[] m_Objects = 
        { 
            new TimeSpan(1, 1, 1, 1, 1),
            new TimeSpan(10, 10, 10, 10, 10),
            new TimeSpan(100, 100, 100, 100, 100),
            new TimeSpan(TimeSpan.MinValue.Ticks + 1), 
            TimeSpan.MaxValue, 
            new TimeSpan(0), 
            DateTime.Now.Subtract(DateTime.Today) 
        };

        private static int m_NextObject = -1;

        public static TimeSpan GetObject()
        {
            m_NextObject++;

            if (m_NextObject == m_Objects.Length)
            {
                m_NextObject = 0;
            }

            return m_Objects[m_NextObject];
        }

        public static TimeSpan[] GetArray()
        {
            return m_Objects;
        }
    }

    public class BooleanFactory
    {
        private static bool[] m_Objects = { true, false };
        private static int m_NextObject = -1;

        public static bool GetObject()
        {
            m_NextObject++;

            if (m_NextObject == m_Objects.Length)
            {
                m_NextObject = 0;
            }

            return m_Objects[m_NextObject];
        }

        public static bool[] GetArray()
        {
            return m_Objects;
        }
    }

    public class ByteFactory
    {
        private static byte[] m_Objects = { byte.MinValue, 0, byte.MaxValue };
        private static int m_NextObject = -1;

        public static byte GetObject()
        {
            m_NextObject++;

            if (m_NextObject == m_Objects.Length)
            {
                m_NextObject = 0;
            }

            return m_Objects[m_NextObject];
        }

        public static byte[] GetArray()
        {
            return m_Objects;
        }
    }

    public class SingleFactory
    {
        private static float[] m_Objects = 
        { 
            float.Epsilon, 
            float.MinValue, 
            -3.40282347E+38f, 
            float.MaxValue, 
            3.40282347E+38f, 
            0f, 
            1f, 
            1.1f, 
            1e1f, 
            .1f,
            3.1415926535f,
            -3.1415926535f
        };

        private static int m_NextObject = -1;

        public static float GetObject()
        {
            m_NextObject++;

            if (m_NextObject == m_Objects.Length)
            {
                m_NextObject = 0;
            }

            return m_Objects[m_NextObject];
        }

        public static float[] GetArray()
        {
            return m_Objects;
        }
    }

    public class DoubleFactory
    {
        private static double[] m_Objects = 
        { 
            Double.Epsilon, 
            Double.MinValue, 
            -1.7976931348623157E+308, 
            Double.MaxValue, 
            1.7976931348623157E+308, 
            0, 
            1, 
            1.1, 
            1e1, 
            //.1,
            3.1415926535,
            -3.1415926535
        };
        private static int m_NextObject = -1;

        public static double GetObject()
        {
            m_NextObject++;

            if (m_NextObject == m_Objects.Length)
            {
                m_NextObject = 0;
            }

            return m_Objects[m_NextObject];
        }

        public static double[] GetArray()
        {
            return m_Objects;
        }
    }

    public class StringFactory
    {
        private static string[] m_Objects = { string.Empty, "", "The quick brown fox jumped over the lazy dog", "Foo", "Bar", "\0\r\n" };
        private static int m_NextObject = -1;

        public static string GetObject()
        {
            m_NextObject++;

            if (m_NextObject == m_Objects.Length)
            {
                m_NextObject = 0;
            }

            return m_Objects[m_NextObject];
        }

        public static string[] GetArray()
        {
            return m_Objects;
        }
    }

    public class Int64Factory
    {
        private static long[] m_Objects = { long.MinValue, 0, long.MaxValue };
        private static int m_NextObject = -1;

        public static long GetObject()
        {
            m_NextObject++;

            if (m_NextObject == m_Objects.Length)
            {
                m_NextObject = 0;
            }

            return m_Objects[m_NextObject];
        }

        public static long[] GetArray()
        {
            return m_Objects;
        }
    }

    public class Int32Factory
    {
        private static int[] m_Objects = { int.MinValue, 0, int.MaxValue };
        private static int m_NextObject = -1;

        public static int GetObject()
        {
            m_NextObject++;

            if (m_NextObject == m_Objects.Length)
            {
                m_NextObject = 0;
            }

            return m_Objects[m_NextObject];
        }

        public static int[] GetArray()
        {
            return m_Objects;
        }
    }

    public class Int16Factory
    {
        private static short[] m_Objects = { short.MinValue, 0, short.MaxValue };
        private static int m_NextObject = -1;

        public static short GetObject()
        {
            m_NextObject++;

            if (m_NextObject == m_Objects.Length)
            {
                m_NextObject = 0;
            }

            return m_Objects[m_NextObject];
        }

        public static short[] GetArray()
        {
            return m_Objects;
        }
    }

    public class UInt64Factory
    {
        private static ulong[] m_Objects = { ulong.MinValue, 0, ulong.MaxValue };
        private static int m_NextObject = -1;

        public static ulong GetObject()
        {
            m_NextObject++;

            if (m_NextObject == m_Objects.Length)
            {
                m_NextObject = 0;
            }

            return m_Objects[m_NextObject];
        }

        public static ulong[] GetArray()
        {
            return m_Objects;
        }
    }

    public class UInt32Factory
    {
        private static uint[] m_Objects = { uint.MinValue, 0, uint.MaxValue };
        private static int m_NextObject = -1;

        public static uint GetObject()
        {
            m_NextObject++;

            if (m_NextObject == m_Objects.Length)
            {
                m_NextObject = 0;
            }

            return m_Objects[m_NextObject];
        }

        public static uint[] GetArray()
        {
            return m_Objects;
        }
    }

    public class UInt16Factory
    {
        private static ushort[] m_Objects = { ushort.MinValue, 0, ushort.MaxValue };
        private static int m_NextObject = -1;

        public static ushort GetObject()
        {
            m_NextObject++;

            if (m_NextObject == m_Objects.Length)
            {
                m_NextObject = 0;
            }

            return m_Objects[m_NextObject];
        }

        public static ushort[] GetArray()
        {
            return m_Objects;
        }
    }

    public class ObjectFactory
    {
        private static object[] m_Objects = { new object(), null };
        private static int m_NextObject = -1;

        public static object GetObject()
        {
            m_NextObject++;

            if (m_NextObject == m_Objects.Length)
            {
                m_NextObject = 0;
            }

            return m_Objects[m_NextObject];
        }

        public static object[] GetArray()
        {
            return m_Objects;
        }
    }

    public class SByteFactory
    {
        private static sbyte[] m_Objects = { sbyte.MinValue, 0, sbyte.MaxValue };
        private static int m_NextObject = -1;

        public static sbyte GetObject()
        {
            m_NextObject++;

            if (m_NextObject == m_Objects.Length)
            {
                m_NextObject = 0;
            }

            return m_Objects[m_NextObject];
        }

        public static sbyte[] GetArray()
        {
            return m_Objects;
        }
    }

    public class WsXmlNodeFactory
    {

        public static Ws.Services.Xml.WsXmlNode GetObject()
        {
            return GetObject("te1");
        }

        private static Ws.Services.Xml.WsXmlNode GetObject(string value)
        {
            Ws.Services.Xml.WsXmlNode node = new Ws.Services.Xml.WsXmlNode();
            node.LocalName = "AnyElement_" + value;
            node.Prefix = value;
            node.NamespaceURI = "http://schemas.example.org/" + value;
            node.Value = "Any Element value = " + value;
            node.Attributes.Append(WsXmlAttributeFactory.GetObject());
            return node;
        }

        public static Ws.Services.Xml.WsXmlNode[] GetArray()
        {
            Ws.Services.Xml.WsXmlNode[] nodes = new Ws.Services.Xml.WsXmlNode[2];
            nodes[0] = GetObject("te1");
            nodes[1] = GetObject("te2");
            return nodes;
        }
    }

    public class WsXmlAttributeFactory
    {
        public static Ws.Services.Xml.WsXmlAttribute GetObject()
        {
            return GetObject("at1");
        }

        private static Ws.Services.Xml.WsXmlAttribute GetObject(string value)
        {
            Ws.Services.Xml.WsXmlAttribute attrib = new Ws.Services.Xml.WsXmlAttribute();
            attrib.LocalName = "AnyAttribute_" + value;
            attrib.Value = "Any attibute value = " + value;
            attrib.NamespaceURI = "http://schemas.example.org/" + value;
            attrib.Prefix = value;
            return attrib;
        }

        public static Ws.Services.Xml.WsXmlAttribute[] GetArray()
        {
            Ws.Services.Xml.WsXmlAttribute[] attribs = new Ws.Services.Xml.WsXmlAttribute[2];
            attribs[0] = GetObject("at1");
            attribs[1] = GetObject("at2");
            return attribs;
        }
    }
}
