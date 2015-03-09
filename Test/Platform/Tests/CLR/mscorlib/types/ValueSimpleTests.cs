////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ValueSimpleTests : IMFTestInterface
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

        //ValueSimple Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\4_values\ValueSimple
        //01,02,03,04,05,06,07,09,11,12,13,14,15
        //12 Failed

        //Test Case Calls 
        [TestMethod]
        public MFTestResults ValueSimple01_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" byte is an alias for System.Byte");
            if (ValueSimpleTestClass01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueSimple02_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" char is an alias for System.Char");
            if (ValueSimpleTestClass02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueSimple03_Test()
                    {
            Log.Comment(" Section 4.1");
            Log.Comment(" short is an alias for System.Int16");
            if (ValueSimpleTestClass03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueSimple04_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" int is an alias for System.Int32");
            if (ValueSimpleTestClass04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueSimple05_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" long is an alias for System.Int64");
            if (ValueSimpleTestClass05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueSimple06_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" float is an alias for System.Single");
            if (ValueSimpleTestClass06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueSimple07_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" double is an alias for System.Double");
            if (ValueSimpleTestClass07.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueSimple09_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" bool is an alias for System.Boolean");
            if (ValueSimpleTestClass09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueSimple11_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" A simple type and the structure type it aliases are completely indistinguishable.");
            Log.Comment(" In other words, writing the reserved work byte is exactly the same as writing ");
            Log.Comment(" System.Byte, and writing System.Int32 is exactly the same as writing the reserved");
            Log.Comment(" word int.");
            if (ValueSimpleTestClass11.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueSimple12_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" Because a simple type aliases a struct type, every simple type has members.");
            if (ValueSimpleTestClass12.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueSimple13_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" sbyte is an alias for System.SByte");
            if (ValueSimpleTestClass13.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueSimple14_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" ushort is an alias for System.UInt16");
            if (ValueSimpleTestClass14.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueSimple15_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" uint is an alias for System.UInt32");
            if (ValueSimpleTestClass15.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }


        //Compiled Test Cases 
        public class ValueSimpleTestClass01
        {
            public static bool testMethod()
            {
                byte b = 0;
                if (b.GetType() == Type.GetType("System.Byte"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueSimpleTestClass02
        {
            public static bool testMethod()
            {
                char c = 'a';
                if (c.GetType() == Type.GetType("System.Char"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueSimpleTestClass03
        {
            public static bool testMethod()
            {
                short s = 0;
                if (s.GetType() == Type.GetType("System.Int16"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueSimpleTestClass04
        {
            public static bool testMethod()
            {
                int i = 0;
                if (i.GetType() == Type.GetType("System.Int32"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueSimpleTestClass05
        {
            public static bool testMethod()
            {
                long l = 0L;
                if (l.GetType() == Type.GetType("System.Int64"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueSimpleTestClass06
        {
            public static bool testMethod()
            {
                float f = 0.0f;
                if (f.GetType() == Type.GetType("System.Single"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueSimpleTestClass07
        {
            public static bool testMethod()
            {
                double d = 0.0d;
                if (d.GetType() == Type.GetType("System.Double"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueSimpleTestClass09
        {
            public static bool testMethod()
            {
                bool b = true;
                if (b.GetType() == Type.GetType("System.Boolean"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueSimpleTestClass11
        {
            public static bool testMethod()
            {
                System.Byte b = 2;
                System.Int32 i = 2;
                if ((b == 2) && (i == 2))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueSimpleTestClass12
        {
            public static bool testMethod()
            {
                bool RetVal = true;
                int i = int.MaxValue;
                if (i != Int32.MaxValue)
                {
                    RetVal = false;
                }
                string s = i.ToString();
                if (!s.Equals(Int32.MaxValue.ToString()))
                {
                    RetVal = false;
                }
                i = 123;
                string t = 123.ToString();
                if (!t.Equals(i.ToString()))
                {
                    RetVal = false;
                }
                return RetVal;
            }
        }
        public class ValueSimpleTestClass13
        {
            public static bool testMethod()
            {
                sbyte b = 0;
                if (b.GetType() == Type.GetType("System.SByte"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueSimpleTestClass14
        {
            public static bool testMethod()
            {
                ushort s = 0;
                if (s.GetType() == Type.GetType("System.UInt16"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueSimpleTestClass15
        {
            public static bool testMethod()
            {
                uint i = 0;
                if (i.GetType() == Type.GetType("System.UInt32"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

    }
}
