//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class TestObjects
    {
        public static byte u1 = 255;
        public static sbyte s1 = -127;
        public static ushort u2 = 65530;
        public static short s2 = -32123;
        public static uint u4 = 0xDEADBEEF;
        public static int s4 = -2000000000;
        public static ulong u8 = 0xDEADBEEFABCDEF01;
        public static long s8 = 0x7EADBEEFABCDEF01;
        public static float f4 = 2.5f;
        public static double f8 = 3.14159;
        public static char c2 = 'S';
        public static String str = "Hello World!";
        public static DateTime dt = DateTime.Now;
        public static TimeSpan ts = new TimeSpan(2, 3, 4, 1, 2);
        public static Object o = new Object();
        public static TestStruct st;
        public static TestClass cl = new TestClass();
        public static Object nul = null;
        public static TestEnum en = TestEnum.Two;

        static TestObjects()
        {
            st.d1 = 1;
            st.d2 = 1.0;
            st.d3 = "1";
        }

        public struct TestStruct
        {
            public int d1;
            public double d2;
            public String d3;
        }

        public class TestClass
        {
            public int d1;
            public long d2;
            public String d3;
        }

        public enum TestEnum
        {
            One = 1,
            Two = 2,
            Three = 3,
        }
    }
}
