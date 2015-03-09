////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ArithmeticTests1 : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");

            // Add your functionality here.                

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
        }

        //Arithmetic Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\Arithmetic
        //arith001,arith003,arith005,arith007,arith023,arith024,arith028,arith029,arith030,arith031,arith032,arith033,arith034,arith035,arith036,arith037,arith038,arith039,arith040,arith041,arith042,arith043,arith044,arith045,arith046,arith047,arith048,arith049,arith050,arith051,arith052,arith053,arith054,arith055,arith056,arith057,arith058,arith059,arith060,arith061,arith062,arith064,arith065
        //opt001a,opt001b,opt002a,opt002b,opt003a,opt003b,opt004a,opt004b,mult0001,mult0002,mult0003,mult0004,mult0008,mult0009,mult0010,mult0011,mult0015,mult0016,mult0017,mult0018,mult0022,mult0023,mult0024,mult0025,mult0050,mult0051,mult0052,mult0053,mult0057,mult0058,mult0059,mult0060,mult0064,mult0065,mult0066,mult0067,mult0071,mult0072,mult0073,mult0074,div0001,div0002,div0008,div0009,div0015,div0016,div0022,div0023,div0050,div0051,div0057,div0058,div0064,div0065,div0071,div0072,rem0001,rem0002,rem0008,rem0009,rem0015,rem0016,rem0022,rem0023,rem0050,rem0051,rem0057,rem0058,rem0064,rem0065,rem0071,rem0072,add0001,add0002,add0003,add0007,add0008,add0009,add0013,add0014,add0015,add0037,add0038,add0039,add0043,add0044,add0045,add0049,add0050,add0051,sub0001,sub0002,sub0003,sub0007,sub0008,sub0009,sub0013,sub0014,sub0015,sub0037,sub0038,sub0039,sub0043,sub0044,sub0045,sub0049,sub0050,sub0051

        //Test Case Calls 
        [TestMethod]
        public MFTestResults Arith_arith001_Test()
        {
            Log.Comment("Section 7.4 ");
            Log.Comment("This code tests basic literal integral arthimetic additive expressions.");
            if (Arith_TestClass_arith001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith003_Test()
        {
            Log.Comment("Section 7.4 ");
            Log.Comment("This code tests basic literal integral arthimetic multiplicative expressions.");
            if (Arith_TestClass_arith003.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith005_Test()
        {
            Log.Comment("Section 7.4 ");
            Log.Comment("This code tests basic integral arthimetic additive expressions using variables.");
            if (Arith_TestClass_arith005.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith007_Test()
        {
            Log.Comment("Section 7.4 ");
            Log.Comment("This code tests basic integral arthimetic multiplicative expressions with variables.");
            if (Arith_TestClass_arith007.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith023_Test()
        {
            Log.Comment("Section 7.4 ");
            Log.Comment("This code tests enum additive expressions.");
            if (Arith_TestClass_arith023.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith024_Test()
        {
            Log.Comment("Section 7.4 ");
            Log.Comment("This code tests enum additive expressions.");
            if (Arith_TestClass_arith024.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith028_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith028.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith029_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith029.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith030_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith030.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith031_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith031.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith032_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith032.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith033_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith033.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith034_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith034.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith035_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith035.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith036_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith036.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith037_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith037.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith038_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith038.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith039_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith039.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith040_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith040.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith041_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith041.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith042_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith042.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith043_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith043.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith044_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith044.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith045_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith045.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith046_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith046.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith047_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith047.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith048_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith048.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith049_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith049.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith050_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith050.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith051_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith051.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith052_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith052.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith053_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith053.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith054_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith054.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith055_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith055.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith056_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith056.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith057_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith057.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith058_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith058.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith059_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith059.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith060_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith060.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith061_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith061.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith062_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith062.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith064_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith064.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_arith065_Test()
        {
            Log.Comment("Section 7.7");
            if (Arith_TestClass_arith065.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        //Compiled Test Cases 
        public class Arith_TestClass_arith001
        {
            public static int Main_old()
            {
                int intRet = 0;
                //Scenario 1: type int
                if (5 != (3 + 2))
                {
                    intRet = 1;
                    Log.Comment("Failure at Scenario 1");
                }
                //Scenario 2: type int
                if (4 != (9 - 5))
                {
                    Log.Comment("Failure at Scenario 2");
                    intRet = 1;
                }

                //Scenario 7: type long
                if (1047L != (999L + 48L))
                {
                    Log.Comment("Failure at Scenario 7");
                    intRet = 1;
                }
                //Scenario 8: type long
                if (441L != (786L - 345L))
                {
                    Log.Comment("Failure at Scenario 8");
                    intRet = 1;
                }
                return intRet;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith003
        {
            public static int Main_old()
            {
                int intRet = 0;
                //Scenario 1: type int
                if (12 != (4 * 3))
                {
                    Log.Comment("Failure at Scenario 1");
                    intRet = 1;
                }
                //Scenario 2: type int
                if (4 != (8 / 2))
                {
                    Log.Comment("Failure at Scenario 2");
                    intRet = 1;
                }
                //Scenario 3; type int
                if (14 != (64 % 25))
                {
                    Log.Comment("Failure at Scenario 3");
                    intRet = 1;
                }

                //Scenario 10: type long
                if (361362L != (458L * 789L))
                {
                    Log.Comment("Failure at Scenario 10");
                    intRet = 1;
                }
                //Scenario 11: type long
                if (36L != (32004L / 889L))
                {
                    Log.Comment("Failure at Scenario 11");
                    intRet = 1;
                }
                //Scenario 12: type long 
                if (29L != (985013L % 56L))
                {
                    Log.Comment("Failure at Scenario 12");
                    intRet = 1;
                }
                return intRet;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith005
        {
            public static int Main_old()
            {
                int intRet = 0;
                int i1 = 0;
                int i2 = 0;
                int i3 = 0;
                byte b1 = 0;
                byte b2 = 0;
                byte b3 = 0;
                short s1 = 0;
                short s2 = 0;
                short s3 = 0;

                long l1 = 0L;
                long l2 = 0L;
                long l3 = 0L;
                //Scenario 1: type int
                i1 = 7;
                i2 = 2;
                i3 = 5;
                if (i1 != (i2 + i3))
                {
                    Log.Comment("Failure at Scenario 1");
                    intRet = 1;
                }
                //Scenario 2: type int
                i1 = 16;
                i2 = 19;
                i3 = 3;
                if (i1 != (i2 - i3))
                {
                    Log.Comment("Failure at Scenario 2");
                    intRet = 1;
                }
                //Scenario 3: type byte
                b1 = 88;
                b2 = 86;
                b3 = 2;
                if (b1 != (b2 + b3))
                {
                    Log.Comment("Failure at Scenario 3");
                    intRet = 1;
                }
                //Scenario 4: type byte
                b1 = 62;
                b2 = 101;
                b3 = 39;
                if (b1 != (b2 - b3))
                {
                    Log.Comment("Failure at Scenario 4");
                    intRet = 1;
                }
                //Scenario 5: type short
                s1 = 45;
                s2 = 23;
                s3 = 22;
                if (s1 != (s2 + s3))
                {
                    Log.Comment("Failure at Scenario 5");
                    intRet = 1;
                }
                //Scenario 6: type short
                s1 = 87;
                s2 = 101;
                s3 = 14;
                if (s1 != (s2 - s3))
                {
                    Log.Comment("Failure at Scenario 6");
                    intRet = 1;
                }
                //Scenario 7: type long
                l1 = 5422L;
                l2 = 4567L;
                l3 = 855L;
                if (l1 != (l2 + l3))
                {
                    Log.Comment("Failure at Scenario 7");
                    intRet = 1;
                }
                //Scenario 8: type long
                l1 = 55423L;
                l2 = 192343L;
                l3 = 136920L;
                if (l1 != (l2 - l3))
                {
                    Log.Comment("Failure at Scenario 8");
                    intRet = 1;
                }
                return intRet;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith007
        {
            public static int Main_old()
            {
                int intRet = 0;
                int i1 = 0;
                int i2 = 0;
                int i3 = 0;
                byte b1 = 0;
                byte b2 = 0;
                byte b3 = 0;
                short s1 = 0;
                short s2 = 0;
                short s3 = 0;

                long l1 = 0L;
                long l2 = 0L;
                long l3 = 0L;
                //Scenario 1: type int
                i1 = 42;
                i2 = 7;
                i3 = 6;
                if (i1 != (i2 * i3))
                {
                    Log.Comment("Failure at Scenario 1");
                    intRet = 1;
                }
                //Scenario 2: type int
                i1 = 11;
                i2 = 154;
                i3 = 14;
                if (i1 != (i2 / i3))
                {
                    Log.Comment("Failure at Scenario 2");
                    intRet = 1;
                }
                //Scenario 3; type int
                i1 = 2;
                i2 = 95;
                i3 = 31;
                if (i1 != (i2 % i3))
                {
                    Log.Comment("Failure at Scenario 3");
                    intRet = 1;
                }
                //Scenario 4: type byte
                b1 = 94;
                b2 = 2;
                b3 = 47;
                if (b1 != (b2 * b3))
                {
                    Log.Comment("Failure at Scenario 4");
                    intRet = 1;
                }
                //Scenario 5: type byte
                b1 = 16;
                b2 = 96;
                b3 = 6;
                if (b1 != (b2 / b3))
                {
                    Log.Comment("Failure at Scenario 5");
                    intRet = 1;
                }
                //Scenario 6: type byte 
                b1 = 48;
                b2 = 220;
                b3 = 86;
                if (b1 != (b2 % b3))
                {
                    Log.Comment("Failure at Scenario 6");
                    intRet = 1;
                }
                //Scenario 7: type short
                s1 = 8890;
                s2 = 254;
                s3 = 35;
                if (s1 != (s2 * s3))
                {
                    Log.Comment("Failure at Scenario 7");
                    intRet = 1;
                }
                //Scenario 8: type short
                s1 = 896;
                s2 = 23296;
                s3 = 26;
                if (s1 != (s2 / s3))
                {
                    Log.Comment("Failure at Scenario 8");
                    intRet = 1;
                }
                //Scenario 9: type short
                s1 = 72;
                s2 = 432;
                s3 = 90;
                if (s1 != (s2 % s3))
                {
                    Log.Comment("Failure at Scenario 9");
                    intRet = 1;
                }
                //Scenario 10: type long
                l1 = 3724L;
                l2 = 38L;
                l3 = 98L;
                if (l1 != (l2 * l3))
                {
                    Log.Comment("Failure at Scenario 10");
                    intRet = 1;
                }
                //Scenario 11: type long
                l1 = 821L;
                l2 = 5747L;
                l3 = 7L;
                if (l1 != (l2 / l3))
                {
                    Log.Comment("Failure at Scenario 11");
                    intRet = 1;
                }
                //Scenario 12: type long 
                l1 = 89L;
                l2 = 22989L;
                l3 = 458L;
                if (l1 != (l2 % l3))
                {
                    Log.Comment("Failure at Scenario 12");
                    intRet = 1;
                }
                return intRet;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        enum Arith_TestClass_arith023_Enum { a = 1, b = 2 }
        public class Arith_TestClass_arith023
        {
            public static int Main_old()
            {
                int MyInt = Arith_TestClass_arith023_Enum.a - Arith_TestClass_arith023_Enum.b; //E-E
                if (MyInt == -1)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        enum Arith_TestClass_arith024_Enum { a = 1, b = 2 }
        public class Arith_TestClass_arith024
        {
            public static int Main_old()
            {
                Arith_TestClass_arith024_Enum MyEnum = Arith_TestClass_arith024_Enum.a + 3; //E+U
                if ((int)MyEnum == 4)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith028
        {
            public static int Main_old()
            {
                int i1 = 2;
                if ((0 * i1) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith029
        {
            public static int Main_old()
            {
                int i1 = 2;
                if ((i1 * 0) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith030
        {
            public static int Main_old()
            {
                byte b1 = 2;
                if ((b1 * 0) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith031
        {
            public static int Main_old()
            {
                sbyte sb1 = 2;
                if ((sb1 * 0) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith032
        {
            public static int Main_old()
            {
                short s1 = 2;
                if ((s1 * 0) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith033
        {
            public static int Main_old()
            {
                ushort us1 = 2;
                if ((us1 * 0) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith034
        {
            public static int Main_old()
            {
                uint ui1 = 2;
                if ((ui1 * 0) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith035
        {
            public static int Main_old()
            {
                long l1 = 2;
                if ((l1 * 0) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith036
        {
            public static int Main_old()
            {
                ulong ul1 = 2;
                if ((ul1 * 0) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith037
        {
            public static int Main_old()
            {
                char c1 = (char)2;
                if ((c1 * 0) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith038
        {
            public static int Main_old()
            {
                int i1 = 2;
                if ((0 / i1) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith039
        {
            public static int Main_old()
            {
                sbyte sb1 = 2;
                if ((0 / sb1) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith040
        {
            public static int Main_old()
            {
                byte b1 = 2;
                if ((0 / b1) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith041
        {
            public static int Main_old()
            {
                short s1 = 2;
                if ((0 / s1) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith042
        {
            public static int Main_old()
            {
                ushort us1 = 2;
                if ((0 / us1) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith043
        {
            public static int Main_old()
            {
                uint ui1 = 2;
                if ((0 / ui1) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith044
        {
            public static int Main_old()
            {
                long l1 = 2;
                if ((0 / l1) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith045
        {
            public static int Main_old()
            {
                ulong ul1 = 2;
                if ((0 / ul1) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith046
        {
            public static int Main_old()
            {
                char c1 = (char)2;
                if ((0 / c1) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith047
        {
            public static int Main_old()
            {

                int i1 = (int)(0x80000000 / -1);

                if (i1 == int.MinValue)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith048
        {
            public static int Main_old()
            {

                int i1 = (int)(0x80000000 % -1);

                if (i1 == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith049
        {
            public static int Main_old()
            {

                string s1 = "foo" + "bar";
                if (s1 == "foobar")
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith050
        {
            public static int Main_old()
            {

                string s1 = null + "foo";
                if (s1 == "foo")
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith051
        {
            public static int Main_old()
            {

                string s1 = "foo" + null;
                if (s1 == "foo")
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith052
        {
            public static int Main_old()
            {

                string s1 = "" + "foo";
                if (s1 == "foo")
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith053
        {
            public static int Main_old()
            {

                string s1 = "foo" + "";
                if (s1 == "foo")
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith054
        {
            public static int Main_old()
            {

                string s1 = ("foo" + "bar");
                if (s1 == "foobar")
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith055
        {
            public static int Main_old()
            {

                string s1 = ("f" + ("oo" + "bar"));
                if (s1 == "foobar")
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith056
        {
            public static int Main_old()
            {

                string s1 = (("f" + "oo") + "ba" + "r");
                if (s1 == "foobar")
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith057
        {
            public static int Main_old()
            {

                string s1 = "foo";
                string s2 = "bar";
                return 0;
                string str = s1 + s2 + "!";
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith058
        {
            public static int Main_old()
            {

                int intI = -1 / int.MaxValue;
                if (intI == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith059
        {
            public static int Main_old()
            {

                int intI = 1 / int.MaxValue;
                if (intI == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith060
        {
            public static int Main_old()
            {

                int intI = -1 / int.MinValue;
                if (intI == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith061
        {
            public static int Main_old()
            {

                int intI = 1 / int.MinValue;
                if (intI == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith062
        {
            public static int Main_old()
            {

                int intI = int.MaxValue / -1;
                if (intI == -2147483647)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith064
        {
            public static implicit operator string(Arith_TestClass_arith064 MC)
            {
                return "foo";
            }
            public static int Main_old()
            {
                Arith_TestClass_arith064 MC = new Arith_TestClass_arith064();
                string TestString1 = "bar";
                if ((MC + TestString1) == "foobar")
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Arith_TestClass_arith065
        {
            public static implicit operator string(Arith_TestClass_arith065 MC)
            {
                return "bar";
            }
            public static int Main_old()
            {
                Arith_TestClass_arith065 MC = new Arith_TestClass_arith065();
                string TestString1 = "foo";
                if ((TestString1 + MC) == "foobar")
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }



    }
}
