////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ReferenceBoxingTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
        }

        //ReferenceBoxing Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\4_values\Reference\Boxing
        //01,02,03,04,05,06,07,09,11,12,13,15,16,17,18,19
        //All of these tests passed in the Baseline Document

        //Test Case Calls 
        [TestMethod]
        public MFTestResults ReferenceBoxing01_Test()
        {
            Log.Comment("Testing byte");
            if (ReferenceBoxingTestClass01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ReferenceBoxing02_Test()
        {
            Log.Comment("Testing short");

            if (ReferenceBoxingTestClass02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ReferenceBoxing03_Test()
        {
            Log.Comment("Testing int");

            if (ReferenceBoxingTestClass03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ReferenceBoxing04_Test()
        {
            Log.Comment("Testing long");

            if (ReferenceBoxingTestClass04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ReferenceBoxing05_Test()
        {
            Log.Comment("Testing char");

            if (ReferenceBoxingTestClass05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ReferenceBoxing06_Test()
        {
            Log.Comment("Testing float");

            if (ReferenceBoxingTestClass06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ReferenceBoxing07_Test()
        {
            Log.Comment("Testing double");

            if (ReferenceBoxingTestClass07.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ReferenceBoxing09_Test()
        {
            Log.Comment("Testing bool");

            if (ReferenceBoxingTestClass09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ReferenceBoxing11_Test()
        {
            Log.Comment("Testing enum");

            if (ReferenceBoxingTestClass11.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ReferenceBoxing12_Test()
        {
            Log.Comment("Testing struct");

            if (ReferenceBoxingTestClass12.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ReferenceBoxing13_Test()
        {
            Log.Comment("Testing class, boxed by interface");

            if (ReferenceBoxingTestClass13.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ReferenceBoxing15_Test()
        {
            Log.Comment("Testing sbyte");

            if (ReferenceBoxingTestClass15.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ReferenceBoxing16_Test()
        {
            Log.Comment("Testing ushort");
            if (ReferenceBoxingTestClass16.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ReferenceBoxing17_Test()
        {
            Log.Comment("Testing uint");
            if (ReferenceBoxingTestClass17.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ReferenceBoxing18_Test()
        {
            Log.Comment("Testing ulong");
            if (ReferenceBoxingTestClass18.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ReferenceBoxing19_Test()
        {
            Log.Comment("Testing that sbyte, ushort, uint, ulong keep typing info");
            if (ReferenceBoxingTestClass19.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        //Compiled Test Cases 
        public class ReferenceBoxingTestClass01
        {
            public static bool testMethod()
            {
                byte b1 = 2;
                object box = b1;
                object box2 = (byte)4;
                b1 = 3;

                if (((byte)box != (byte)2) || ((byte)box2 != (byte)4))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public class ReferenceBoxingTestClass02
        {
            public static bool testMethod()
            {
                short s1 = 2;
                object box = s1;
                object box2 = (short)4;
                s1 = 3;

                if (((short)box != (short)2) || ((short)box2 != (short)4))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public class ReferenceBoxingTestClass03
        {
            public static bool testMethod()
            {
                int i1 = 2;
                object box = i1;
                object box2 = 4;
                i1 = 3;

                if (((int)box != 2) || ((int)box2 != 4))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public class ReferenceBoxingTestClass04
        {
            public static bool testMethod()
            {
                long l1 = 2;
                object box = l1;
                object box2 = 4L;
                l1 = 3;

                if (((long)box != 2L) || ((long)box2 != 4L))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public class ReferenceBoxingTestClass05
        {
            public static bool testMethod()
            {
                char c1 = (char)2;
                object box = c1;
                object box2 = (char)4;
                c1 = (char)3;

                if (((char)box != (char)2) || ((char)box2 != (char)4))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public class ReferenceBoxingTestClass06
        {
            public static bool testMethod()
            {
                float f1 = 2.0f;
                object box = f1;
                object box2 = 4.0f;
                f1 = 3.0f;

                if (((float)box != 2.0f) || ((float)box2 != 4.0f))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public class ReferenceBoxingTestClass07
        {
            public static bool testMethod()
            {
                double d1 = 2.0d;
                object box = d1;
                object box2 = 4.0d;
                d1 = 3.0d;

                if (((double)box != 2.0d) || ((double)box2 != 4.0d))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public class ReferenceBoxingTestClass09
        {
            public static bool testMethod()
            {
                bool b1 = true;
                object box = b1;
                object box2 = false;
                b1 = false;

                if (((bool)box != true) || ((bool)box2 != false))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        enum ReferenceBoxingTestClass11_Enum { a = 2, b = 3 }
        public class ReferenceBoxingTestClass11
        {
            public static bool testMethod()
            {
                ReferenceBoxingTestClass11_Enum MyEnum = ReferenceBoxingTestClass11_Enum.a;
                object box = MyEnum;
                object box2 = ReferenceBoxingTestClass11_Enum.b;
                MyEnum = ReferenceBoxingTestClass11_Enum.b;

                if (((ReferenceBoxingTestClass11_Enum)box != ReferenceBoxingTestClass11_Enum.a) || ((ReferenceBoxingTestClass11_Enum)box2 != ReferenceBoxingTestClass11_Enum.b))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        struct ReferenceBoxingTestClass12_Struct
        {
            public int MyInt;
        }
        public class ReferenceBoxingTestClass12
        {
            public static bool testMethod()
            {
                ReferenceBoxingTestClass12_Struct MS = new ReferenceBoxingTestClass12_Struct();
                MS.MyInt = 3;
                object box = MS;
                MS.MyInt = 4;

                if (((ReferenceBoxingTestClass12_Struct)box).MyInt != 3)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        interface ReferenceBoxingTestClass13_Interface
        {
            void f();
        }
        struct ReferenceBoxingTestClass13_Struct : ReferenceBoxingTestClass13_Interface
        {
            public int MyInt;
            public void f() { }
        }
        public class ReferenceBoxingTestClass13
        {
            public static bool testMethod()
            {
                ReferenceBoxingTestClass13_Struct MS = new ReferenceBoxingTestClass13_Struct();
                MS.MyInt = 3;
                ReferenceBoxingTestClass13_Interface box = MS;
                MS.MyInt = 4;

                if (((ReferenceBoxingTestClass13_Struct)box).MyInt != 3)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public class ReferenceBoxingTestClass15
        {
            public static bool testMethod()
            {
                sbyte b1 = 2;
                object box = b1;
                object box2 = (sbyte)4;
                b1 = 3;

                if (((sbyte)box != (sbyte)2) || ((sbyte)box2 != (sbyte)4))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public class ReferenceBoxingTestClass16
        {
            public static bool testMethod()
            {
                ushort s1 = 2;
                object box = s1;
                object box2 = (ushort)4;
                s1 = 3;

                if (((ushort)box != (ushort)2) || ((ushort)box2 != (ushort)4))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public class ReferenceBoxingTestClass17
        {
            public static bool testMethod()
            {
                uint i1 = 2;
                object box = i1;
                object box2 = 4u;
                i1 = 3;

                if (((uint)box != 2) || ((uint)box2 != 4))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public class ReferenceBoxingTestClass18
        {
            public static bool testMethod()
            {
                ulong l1 = 2;
                object box = l1;
                object box2 = 4ul;
                l1 = 3;
                if (((ulong)box != 2ul) || ((ulong)box2 != 4ul))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public class ReferenceBoxingTestClass19
        {
            public static bool testMethod()
            {
                sbyte b1 = 2;
                ushort s1 = 3;
                uint i1 = 4;
                ulong l1 = 5;
                object box1 = b1;
                object box2 = s1;
                object box3 = i1;
                object box4 = l1;
                if ((box1 is sbyte) && (box2 is ushort) && (box3 is uint) && (box4 is ulong))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                return true;

            }
        }

    }
}
