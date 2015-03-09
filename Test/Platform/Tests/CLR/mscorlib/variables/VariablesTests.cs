////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class VariablesTests : IMFTestInterface
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

        //Variables Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\Variables
        //S5_range_byte_0.cs,S5_range_byte_1.cs,S5_range_char_0.cs,S5_range_char_1.cs,S5_range_double_0.cs,S5_range_double_1.cs,S5_range_float_0.cs,S5_range_float_1.cs,S5_range_int_0.cs,S5_range_int_1.cs,S5_range_int_3.cs,S5_range_long_0.cs,S5_range_long_1.cs,S5_range_long_3.cs,S5_range_short_0.cs,S5_range_short_1.cs

        //Test Case Calls 
        [TestMethod]
        public MFTestResults Variables_S5_range_byte_0_Test()
        {
            Log.Comment("S5_range_byte_0.sc");
            Log.Comment("This is a variable range test:");
            Log.Comment("byte");
            Log.Comment("	CompilesOK:	1");
            Log.Comment("	Value:		255Y");
            if (Variables_TestClass_S5_range_byte_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Variables_S5_range_byte_1_Test()
        {
            Log.Comment("S5_range_byte_1.sc");
            Log.Comment("This is a variable range test:");
            Log.Comment("byte");
            Log.Comment("	CompilesOK:	1");
            Log.Comment("	Value:		0Y");
            if (Variables_TestClass_S5_range_byte_1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Variables_S5_range_char_0_Test()
        {
            Log.Comment("S5_range_char_0.sc");
            Log.Comment("This is a variable range test:");
            Log.Comment("char");
            Log.Comment("	CompilesOK:	1");
            Log.Comment("	Value:		65535");
            if (Variables_TestClass_S5_range_char_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Variables_S5_range_char_1_Test()
        {
            Log.Comment("S5_range_char_1.sc");
            Log.Comment("This is a variable range test:");
            Log.Comment("char");
            Log.Comment("	CompilesOK:	1");
            Log.Comment("	Value:		0");
            if (Variables_TestClass_S5_range_char_1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Variables_S5_range_double_0_Test()
        {
            Log.Comment("S5_range_double_0.sc");
            Log.Comment("This is a variable range test:");
            Log.Comment("double");
            Log.Comment("	CompilesOK:	1");
            Log.Comment("	Value:		1.7e308d");
            if (Variables_TestClass_S5_range_double_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Variables_S5_range_double_1_Test()
        {
            Log.Comment("S5_range_double_1.sc");
            Log.Comment("This is a variable range test:");
            Log.Comment("double");
            Log.Comment("	CompilesOK:	1");
            Log.Comment("	Value:		-1.7e308d");
            if (Variables_TestClass_S5_range_double_1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Variables_S5_range_float_0_Test()
        {
            Log.Comment("S5_range_float_0.sc");
            Log.Comment("This is a variable range test:");
            Log.Comment("float");
            Log.Comment("	CompilesOK:	1");
            Log.Comment("	Value:		3.4e38F");
            if (Variables_TestClass_S5_range_float_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Variables_S5_range_float_1_Test()
        {
            Log.Comment("S5_range_float_1.sc");
            Log.Comment("This is a variable range test:");
            Log.Comment("float");
            Log.Comment("	CompilesOK:	1");
            Log.Comment("	Value:		-3.4e38F");
            if (Variables_TestClass_S5_range_float_1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Variables_S5_range_int_0_Test()
        {
            Log.Comment("S5_range_int_0.sc");
            Log.Comment("This is a variable range test:");
            Log.Comment("int");
            Log.Comment("	CompilesOK:	1");
            Log.Comment("	Value:		2147483647");
            if (Variables_TestClass_S5_range_int_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Variables_S5_range_int_1_Test()
        {
            Log.Comment("S5_range_int_1.sc");
            Log.Comment("This is a variable range test:");
            Log.Comment("int");
            Log.Comment("	CompilesOK:	1");
            Log.Comment("	Value:		-2147483647");
            if (Variables_TestClass_S5_range_int_1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Variables_S5_range_int_3_Test()
        {
            Log.Comment("S5_range_int_3.sc");
            Log.Comment("This is a variable range test:");
            Log.Comment("int");
            Log.Comment("	CompilesOK:	0");
            Log.Comment("	Value:		-2147483648");
            if (Variables_TestClass_S5_range_int_3.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Variables_S5_range_long_0_Test()
        {
            Log.Comment("S5_range_long_0.sc");
            Log.Comment("This is a variable range test:");
            Log.Comment("long");
            Log.Comment("	CompilesOK:	1");
            Log.Comment("	Value:		9223372036854775807L");
            if (Variables_TestClass_S5_range_long_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Variables_S5_range_long_1_Test()
        {
            Log.Comment("S5_range_long_1.sc");
            Log.Comment("This is a variable range test:");
            Log.Comment("long");
            Log.Comment("	CompilesOK:	1");
            Log.Comment("	Value:		-9223372036854775807L");
            if (Variables_TestClass_S5_range_long_1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Variables_S5_range_long_3_Test()
        {
            Log.Comment("S5_range_long_3.sc");
            Log.Comment("This is a variable range test:");
            Log.Comment("long");
            Log.Comment("	CompilesOK:	0");
            Log.Comment("	Value:		-9223372036854775808L");
            if (Variables_TestClass_S5_range_long_3.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Variables_S5_range_short_0_Test()
        {
            Log.Comment("S5_range_short_0.sc");
            Log.Comment("This is a variable range test:");
            Log.Comment("short");
            Log.Comment("	CompilesOK:	1");
            Log.Comment("	Value:		32767S");
            if (Variables_TestClass_S5_range_short_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Variables_S5_range_short_1_Test()
        {
            Log.Comment("S5_range_short_1.sc");
            Log.Comment("This is a variable range test:");
            Log.Comment("short");
            Log.Comment("	CompilesOK:	1");
            Log.Comment("	Value:		-32767S");
            if (Variables_TestClass_S5_range_short_1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        public class Variables_TestClass_S5_range_byte_0
        {
            public static void Main_old()
            {
                byte var;
                byte var2;
                var = 255;
                var2 = var;
                return;
            }
            public static bool testMethod()
            {
                try
                {
                    Main_old();
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }
        public class Variables_TestClass_S5_range_byte_1
        {
            public static void Main_old()
            {
                byte var;
                byte var2;
                var = 0;
                var2 = var;
                return;
            }
            public static bool testMethod()
            {
                try
                {
                    Main_old();
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }
        public class Variables_TestClass_S5_range_char_0
        {
            public static void Main_old()
            {
                char var;
                char var2;
                var = (char)65535;
                var2 = var;
                return;
            }
            public static bool testMethod()
            {
                try
                {
                    Main_old();
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }
        public class Variables_TestClass_S5_range_char_1
        {
            public static void Main_old()
            {
                char var;
                char var2;
                var = (char)0;
                var2 = var;
                return;
            }
            public static bool testMethod()
            {
                try
                {
                    Main_old();
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }
        public class Variables_TestClass_S5_range_double_0
        {
            public static void Main_old()
            {
                double var;
                double var2;
                var = 1.7e308d;
                var2 = var;
                return;
            }
            public static bool testMethod()
            {
                try
                {
                    Main_old();
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }
        public class Variables_TestClass_S5_range_double_1
        {
            public static void Main_old()
            {
                double var;
                double var2;
                var = -1.7e308d;
                var2 = var;
                return;
            }
            public static bool testMethod()
            {
                try
                {
                    Main_old();
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }
        public class Variables_TestClass_S5_range_float_0
        {
            public static void Main_old()
            {
                float var;
                float var2;
                var = 3.4e38F;
                var2 = var;
                return;
            }
            public static bool testMethod()
            {
                try
                {
                    Main_old();
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }
        public class Variables_TestClass_S5_range_float_1
        {
            public static void Main_old()
            {
                float var;
                float var2;
                var = -3.4e38F;
                var2 = var;
                return;
            }
            public static bool testMethod()
            {
                try
                {
                    Main_old();
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }
        public class Variables_TestClass_S5_range_int_0
        {
            public static void Main_old()
            {
                int var;
                int var2;
                var = 2147483647;
                var2 = var;
                return;
            }
            public static bool testMethod()
            {
                try
                {
                    Main_old();
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }
        public class Variables_TestClass_S5_range_int_1
        {
            public static void Main_old()
            {
                int var;
                int var2;
                var = -2147483647;
                var2 = var;
                return;
            }
            public static bool testMethod()
            {
                try
                {
                    Main_old();
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }
        public class Variables_TestClass_S5_range_int_3
        {
            public static void Main_old()
            {
                int var;
                int var2;
                var = -2147483648;
                var2 = var;
                return;
            }
            public static bool testMethod()
            {
                try
                {
                    Main_old();
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }
        public class Variables_TestClass_S5_range_long_0
        {
            public static void Main_old()
            {
                long var;
                long var2;
                var = 9223372036854775807L;
                var2 = var;
                return;
            }
            public static bool testMethod()
            {
                try
                {
                    Main_old();
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }
        public class Variables_TestClass_S5_range_long_1
        {
            public static void Main_old()
            {
                long var;
                long var2;
                var = -9223372036854775807L;
                var2 = var;
                return;
            }
            public static bool testMethod()
            {
                try
                {
                    Main_old();
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }
        public class Variables_TestClass_S5_range_long_3
        {
            public static void Main_old()
            {
                long var;
                long var2;
                var = -9223372036854775808L;
                var2 = var;
                return;
            }
            public static bool testMethod()
            {
                try
                {
                    Main_old();
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }
        public class Variables_TestClass_S5_range_short_0
        {
            public static void Main_old()
            {
                short var;
                short var2;
                var = 32767;
                var2 = var;
                return;
            }
            public static bool testMethod()
            {
                try
                {
                    Main_old();
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }
        public class Variables_TestClass_S5_range_short_1
        {
            public static void Main_old()
            {
                short var;
                short var2;
                var = -32767;
                var2 = var;
                return;
            }
            public static bool testMethod()
            {
                try
                {
                    Main_old();
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }


    }
}
