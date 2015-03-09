////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ValueArrayTests : IMFTestInterface
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

        //ValueArray Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\4_values\ValueArray
        //01,02,03,04,05,06,07,09,11,12,13,14,15
        //12 Failed

        //Test Case Calls 
        [TestMethod]
        public MFTestResults ValueArray01_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" byte is an alias for System.Byte");
            if (ValueArrayTestClass01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueArray02_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" char is an alias for System.Char");
            if (ValueArrayTestClass02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueArray03_Test()
                    {
            Log.Comment(" Section 4.1");
            Log.Comment(" short is an alias for System.Int16");
            if (ValueArrayTestClass03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueArray04_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" int is an alias for System.Int32");
            if (ValueArrayTestClass04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueArray05_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" long is an alias for System.Int64");
            if (ValueArrayTestClass05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueArray06_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" float is an alias for System.Single");
            if (ValueArrayTestClass06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueArray07_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" double is an alias for System.Double");
            if (ValueArrayTestClass07.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueArray09_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" bool is an alias for System.Boolean");
            if (ValueArrayTestClass09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults ValueArray12_Test()
        {
           Log.Comment(" Section 4.1");
            Log.Comment(" Because a simple type aliases a struct type, every simple type has members.");
            Log.Comment("This test is expected to fail");
            if (ValueArrayTestClass12.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueArray13_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" sbyte is an alias for System.SByte");
            if (ValueArrayTestClass13.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueArray14_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" ushort is an alias for System.UInt16");
            if (ValueArrayTestClass14.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueArray15_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" uint is an alias for System.UInt32");
            if (ValueArrayTestClass15.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }


        //Compiled Test Cases 
        public class ValueArrayTestClass01
        {
            public static bool testMethod()
            {
                byte[] b = {0} ;
                if (b.GetType() == Type.GetType("System.Byte[]"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueArrayTestClass02
        {
            public static bool testMethod()
            {
                char [] c = {'a'};
                if (c.GetType() == Type.GetType("System.Char[]"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueArrayTestClass03
        {
            public static bool testMethod()
            {
                short [] s = {0};
                if (s.GetType() == Type.GetType("System.Int16[]"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueArrayTestClass04
        {
            public static bool testMethod()
            {
                int [] i = {0};
                if (i.GetType() == Type.GetType("System.Int32[]"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueArrayTestClass05
        {
            public static bool testMethod()
            {
                long [] l = {0L};
                if (l.GetType() == Type.GetType("System.Int64[]"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueArrayTestClass06
        {
            public static bool testMethod()
            {
                float [] f = {0.0f};
                if (f.GetType() == Type.GetType("System.Single[]"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueArrayTestClass07
        {
            public static bool testMethod()
            {
                double [] d = {0.0d};
                if (d.GetType() == Type.GetType("System.Double[]"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueArrayTestClass09
        {
            public static bool testMethod()
            {
                bool [] b = {true};
                if (b.GetType() == Type.GetType("System.Boolean[]"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class ValueArrayTestClass12
        {
            public static bool testMethod()
            {
                string [] b = { "string" };
                if (b.GetType() == Type.GetType("System.String[]"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueArrayTestClass13
        {
            public static bool testMethod()
            {
                sbyte [] b = {0};
                if (b.GetType() == Type.GetType("System.SByte[]"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueArrayTestClass14
        {
            public static bool testMethod()
            {
                ushort [] s = {0};
                if (s.GetType() == Type.GetType("System.UInt16[]"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueArrayTestClass15
        {
            public static bool testMethod()
            {
                uint [] i = {0};
                if (i.GetType() == Type.GetType("System.UInt32[]"))
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
