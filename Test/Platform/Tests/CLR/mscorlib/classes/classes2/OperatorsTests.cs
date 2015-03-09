////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class OperatorsTests : IMFTestInterface
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

        //Operators Test methods
        //All test methods ported from folder current\test\cases\client\CLR\Conformance\10_classes\Operators
        //The following tests were removed because they were build failure tests:
        //8-12,26-37,47-66,70-87

        [TestMethod]
        public MFTestResults Operators1_Test()
        {
            Log.Comment("Tests overriding unary plus");
            if (OperatorsTestClass1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators2_Test()
        {
            Log.Comment("Tests overriding unary minus");
            if (OperatorsTestClass2.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators3_Test()
        {
            Log.Comment("Tests overriding tilde");
            if (OperatorsTestClass3.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;

        }

        [TestMethod]
        public MFTestResults Operators4_Test()
        {
            Log.Comment("Tests overriding increment prefix");
            if (OperatorsTestClass4.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators5_Test()
        {
            Log.Comment("Tests overriding increment suffix");
            if (OperatorsTestClass5.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators6_Test()
        {
            Log.Comment("Tests overriding decrement prefix");
            if (OperatorsTestClass6.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators7_Test()
        {
            Log.Comment("Tests overriding decrement suffix");
            if (OperatorsTestClass7.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators13_Test()
        {
            Log.Comment("Tests overriding binary plus");
            if (OperatorsTestClass13.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators14_Test()
        {
            Log.Comment("Tests overriding binary minus");
            if (OperatorsTestClass14.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators15_Test()
        {
            Log.Comment("Tests overriding asterisk (multiply)");
            if (OperatorsTestClass15.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators16_Test()
        {
            Log.Comment("Tests overriding slash (division)");
            if (OperatorsTestClass16.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators17_Test()
        {
            Log.Comment("Tests overriding percent (modulus)");
            if (OperatorsTestClass17.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators18_Test()
        {
            Log.Comment("Tests overriding caret (xor)");
            if (OperatorsTestClass18.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Operators19_Test()
        {
            Log.Comment("Tests overriding ampersand");
            if (OperatorsTestClass19.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators20_Test()
        {
            Log.Comment("Tests overriding pipe (or)");
            if (OperatorsTestClass20.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators21_Test()
        {
            Log.Comment("Tests overriding double less-than (left shift)");
            if (OperatorsTestClass21.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators22_Test()
        {
            Log.Comment("Tests overriding double greater-than (right shift)");
            if (OperatorsTestClass22.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators23_Test()
        {
            Log.Comment("Tests overriding binary plus with 1 int parameter");
            if (OperatorsTestClass23.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators24_Test()
        {
            Log.Comment("Tests overriding double equals (equality comparison) and exclamation-equals (non-equality comparison)");
            if (OperatorsTestClass24.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators38_Test()
        {
            Log.Comment("Tests overriding binary plus with 1 int parameter");
            if (OperatorsTestClass38.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators39_Test()
        {
            Log.Comment("Tests overriding binary minus with 1 int parameter");
            if (OperatorsTestClass39.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators40_Test()
        {
            Log.Comment("Tests overriding asterisk (multiply) with 1 int parameter");
            if (OperatorsTestClass40.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators41_Test()
        {
            Log.Comment("Tests overriding slash (divide) with 1 int parameter");
            if (OperatorsTestClass41.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators42_Test()
        {
            Log.Comment("Tests overriding percent (modulus) with 1 int parameter");
            if (OperatorsTestClass42.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators43_Test()
        {
            Log.Comment("Tests overriding caret (xor) with 1 int parameter");
            if (OperatorsTestClass43.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators44_Test()
        {
            Log.Comment("Tests overriding ampersand with 1 int parameter");
            if (OperatorsTestClass44.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators45_Test()
        {
            Log.Comment("Tests overriding pipe (or) with 1 int parameter");
            if (OperatorsTestClass45.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators46_Test()
        {
            Log.Comment("Tests overriding double equals (equality comparison) and exclamation-equals ");
            Log.Comment("(non-equality comparison) with 1 int");
            if (OperatorsTestClass46.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators67_Test()
        {
            Log.Comment("Tests overriding unary exclamation (not)");
            if (OperatorsTestClass67.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators68_Test()
        {
            Log.Comment("Tests overriding true and false");
            if (OperatorsTestClass68.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators69_Test()
        {
            Log.Comment("Tests overriding true and false and ampersand");
            if (OperatorsTestClass69.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators88_Test()
        {
            Log.Comment("Tests true and false with ampersand");
            if (OperatorsTestClass88.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators89_Test()
        {
            Log.Comment("Tests true and false with double ampersand");
            if (OperatorsTestClass89.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators90_Test()
        {
            Log.Comment("Tests true and false with pipe (or)");
            if (OperatorsTestClass90.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators91_Test()
        {
            Log.Comment("Tests true and false with double pipe (or)");
            if (OperatorsTestClass91.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators92_Test()
        {
            Log.Comment("Tests true and false with caret (xor)");
            if (OperatorsTestClass92.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators93_Test()
        {
            Log.Comment("Tests numerical types with plus");
            if (OperatorsTestClass93.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators94_Test()
        {
            Log.Comment("Tests numerical types with minus");
            if (OperatorsTestClass94.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators95_Test()
        {
            Log.Comment("Tests numerical types with asterisk (multiply)");
            if (OperatorsTestClass95.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Operators96_Test()
        {
            Log.Comment("Tests numerical types with slash (divide)");
            if (OperatorsTestClass96.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        class OperatorsTestClass1
        {
            public int intI = 2;
            public static OperatorsTestClass1 operator +(OperatorsTestClass1 MyInt)
            {
                MyInt.intI = 3;
                return MyInt;
            }
            public static bool testMethod()
            {
                OperatorsTestClass1 Test = new OperatorsTestClass1();
                OperatorsTestClass1 temp = +Test;
                if (Test.intI == 3)
                {
                    return true;
                }
                else
                {
                    return false;

                }
            }
        }

        class OperatorsTestClass2
        {
            public int intI = 2;
            public static OperatorsTestClass2 operator -(OperatorsTestClass2 MyInt)
            {
                MyInt.intI = 3;
                return MyInt;
            }
            public static bool testMethod()
            {
                OperatorsTestClass2 Test = new OperatorsTestClass2();
                OperatorsTestClass2 temp = -Test;
                if (Test.intI == 3)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass3
        {
            public int intI = 2;
            public static OperatorsTestClass3 operator ~(OperatorsTestClass3 MyInt)
            {
                MyInt.intI = 3;
                return MyInt;
            }
            public static bool testMethod()
            {
                OperatorsTestClass3 Test = new OperatorsTestClass3();
                OperatorsTestClass3 temp = ~Test;
                if (Test.intI == 3)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass4
        {
            public int intI = 2;
            public static OperatorsTestClass4 operator ++(OperatorsTestClass4 MyInt)
            {
                OperatorsTestClass4 MC = new OperatorsTestClass4();
                MC.intI = 3;
                return MC;
            }
            public static bool testMethod()
            {
                OperatorsTestClass4 Test = new OperatorsTestClass4();
                OperatorsTestClass4 Test2 = ++Test;

                if ((Test.intI == 3) && (Test2.intI == 3))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass5
        {
            public int intI = 2;
            public static OperatorsTestClass5 operator ++(OperatorsTestClass5 MyInt)
            {
                OperatorsTestClass5 MC = new OperatorsTestClass5();
                MC.intI = 3;
                return MC;
            }
            public static bool testMethod()
            {
                OperatorsTestClass5 Test = new OperatorsTestClass5();
                OperatorsTestClass5 Test2 = Test++;

                if ((Test.intI == 3) && (Test2.intI == 2))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass6
        {
            public int intI = 2;
            public static OperatorsTestClass6 operator --(OperatorsTestClass6 MyInt)
            {
                OperatorsTestClass6 MC = new OperatorsTestClass6();
                MC.intI = 3;
                return MC;
            }
            public static bool testMethod()
            {
                OperatorsTestClass6 Test = new OperatorsTestClass6();
                OperatorsTestClass6 Test2 = --Test;

                if ((Test.intI == 3) && (Test2.intI == 3))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass7
        {
            public int intI = 2;
            public static OperatorsTestClass7 operator --(OperatorsTestClass7 MyInt)
            {
                OperatorsTestClass7 MC = new OperatorsTestClass7();
                MC.intI = 3;
                return MC;
            }
            public static bool testMethod()
            {
                OperatorsTestClass7 Test = new OperatorsTestClass7();
                OperatorsTestClass7 Test2 = Test--;

                if ((Test.intI == 3) && (Test2.intI == 2))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass13
        {
            public int intI = 2;
            public static OperatorsTestClass13 operator +(OperatorsTestClass13 MyInt, OperatorsTestClass13 MyInt2)
            {
                OperatorsTestClass13 MC = new OperatorsTestClass13();
                MC.intI = MyInt.intI + MyInt2.intI;
                return MC;
            }
            public static bool testMethod()
            {
                OperatorsTestClass13 Test1 = new OperatorsTestClass13();
                OperatorsTestClass13 Test2 = new OperatorsTestClass13();
                Test2.intI = 3;
                OperatorsTestClass13 Test = Test1 + Test2;
                if (Test.intI == 5)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass14
        {
            public int intI = 2;
            public static OperatorsTestClass14 operator -(OperatorsTestClass14 MyInt, OperatorsTestClass14 MyInt2)
            {
                OperatorsTestClass14 MC = new OperatorsTestClass14();
                MC.intI = MyInt.intI + MyInt2.intI;
                return MC;
            }
            public static bool testMethod()
            {
                OperatorsTestClass14 Test1 = new OperatorsTestClass14();
                OperatorsTestClass14 Test2 = new OperatorsTestClass14();
                Test2.intI = 3;
                OperatorsTestClass14 Test = Test1 - Test2;
                if (Test.intI == 5)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass15
        {
            public int intI = 2;
            public static OperatorsTestClass15 operator *(OperatorsTestClass15 MyInt, OperatorsTestClass15 MyInt2)
            {
                OperatorsTestClass15 MC = new OperatorsTestClass15();
                MC.intI = MyInt.intI + MyInt2.intI;
                return MC;
            }
            public static bool testMethod()
            {
                OperatorsTestClass15 Test1 = new OperatorsTestClass15();
                OperatorsTestClass15 Test2 = new OperatorsTestClass15();
                Test2.intI = 3;
                OperatorsTestClass15 Test = Test1 * Test2;
                if (Test.intI == 5)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass16
        {
            public int intI = 2;
            public static OperatorsTestClass16 operator /(OperatorsTestClass16 MyInt, OperatorsTestClass16 MyInt2)
            {
                OperatorsTestClass16 MC = new OperatorsTestClass16();
                MC.intI = MyInt.intI + MyInt2.intI;
                return MC;
            }
            public static bool testMethod()
            {
                OperatorsTestClass16 Test1 = new OperatorsTestClass16();
                OperatorsTestClass16 Test2 = new OperatorsTestClass16();
                Test2.intI = 3;
                OperatorsTestClass16 Test = Test1 / Test2;
                if (Test.intI == 5)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass17
        {
            public int intI = 2;
            public static OperatorsTestClass17 operator %(OperatorsTestClass17 MyInt, OperatorsTestClass17 MyInt2)
            {
                OperatorsTestClass17 MC = new OperatorsTestClass17();
                MC.intI = MyInt.intI + MyInt2.intI;
                return MC;
            }
            public static bool testMethod()
            {
                OperatorsTestClass17 Test1 = new OperatorsTestClass17();
                OperatorsTestClass17 Test2 = new OperatorsTestClass17();
                Test2.intI = 3;
                OperatorsTestClass17 Test = Test1 % Test2;
                if (Test.intI == 5)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass18
        {
            public int intI = 2;
            public static OperatorsTestClass18 operator ^(OperatorsTestClass18 MyInt, OperatorsTestClass18 MyInt2)
            {
                OperatorsTestClass18 MC = new OperatorsTestClass18();
                MC.intI = MyInt.intI + MyInt2.intI;
                return MC;
            }
            public static bool testMethod()
            {
                OperatorsTestClass18 Test1 = new OperatorsTestClass18();
                OperatorsTestClass18 Test2 = new OperatorsTestClass18();
                Test2.intI = 3;
                OperatorsTestClass18 Test = Test1 ^ Test2;
                if (Test.intI == 5)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass19
        {
            public int intI = 2;
            public static OperatorsTestClass19 operator &(OperatorsTestClass19 MyInt, OperatorsTestClass19 MyInt2)
            {
                OperatorsTestClass19 MC = new OperatorsTestClass19();
                MC.intI = MyInt.intI + MyInt2.intI;
                return MC;
            }
            public static bool testMethod()
            {
                OperatorsTestClass19 Test1 = new OperatorsTestClass19();
                OperatorsTestClass19 Test2 = new OperatorsTestClass19();
                Test2.intI = 3;
                OperatorsTestClass19 Test = Test1 & Test2;
                if (Test.intI == 5)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass20
        {
            public int intI = 2;
            public static OperatorsTestClass20 operator |(OperatorsTestClass20 MyInt, OperatorsTestClass20 MyInt2)
            {
                OperatorsTestClass20 MC = new OperatorsTestClass20();
                MC.intI = MyInt.intI + MyInt2.intI;
                return MC;
            }
            public static bool testMethod()
            {
                OperatorsTestClass20 Test1 = new OperatorsTestClass20();
                OperatorsTestClass20 Test2 = new OperatorsTestClass20();
                Test2.intI = 3;
                OperatorsTestClass20 Test = Test1 | Test2;
                if (Test.intI == 5)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass21
        {
            public int intI = 2;
            public static OperatorsTestClass21 operator <<(OperatorsTestClass21 MyInt, int MyInt2)
            {
                OperatorsTestClass21 MC = new OperatorsTestClass21();
                MC.intI = MyInt.intI + MyInt2;
                return MC;
            }
            public static bool testMethod()
            {
                OperatorsTestClass21 Test1 = new OperatorsTestClass21();
                OperatorsTestClass21 Test = Test1 << 3;
                if (Test.intI == 5)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass22
        {
            public int intI = 2;
            public static OperatorsTestClass22 operator >>(OperatorsTestClass22 MyInt, int MyInt2)
            {
                OperatorsTestClass22 MC = new OperatorsTestClass22();
                MC.intI = MyInt.intI + MyInt2;
                return MC;
            }
            public static bool testMethod()
            {
                OperatorsTestClass22 Test1 = new OperatorsTestClass22();
                OperatorsTestClass22 Test = Test1 >> 3;
                if (Test.intI == 5)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass23
        {
            public int intI = 2;
            public static OperatorsTestClass23 operator +(OperatorsTestClass23 MyInt, int MyInt2)
            {
                OperatorsTestClass23 MC = new OperatorsTestClass23();
                MC.intI = MyInt.intI + MyInt2;
                return MC;
            }
            public static bool testMethod()
            {
                OperatorsTestClass23 Test1 = new OperatorsTestClass23();
                OperatorsTestClass23 Test = Test1 + 3;
                if (Test.intI == 5)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass24
        {
            public int intI = 2;
            public static bool operator ==(OperatorsTestClass24 MyInt, OperatorsTestClass24 MyInt2)
            {
                if (MyInt.intI == MyInt2.intI)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            public static bool operator !=(OperatorsTestClass24 MyInt, OperatorsTestClass24 MyInt2)
            {
                return false;
            }
            public static bool testMethod()
            {
                OperatorsTestClass24 Test1 = new OperatorsTestClass24();
                OperatorsTestClass24 Test2 = new OperatorsTestClass24();
                OperatorsTestClass24 Test3 = new OperatorsTestClass24();

                Test2.intI = 3;
                if ((Test1 == Test3) && (!(Test1 == Test2)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass38
        {
            public int intI = 2;
            public static OperatorsTestClass38 operator +(OperatorsTestClass38 MyInt, int MyInt2)
            {
                OperatorsTestClass38 MC = new OperatorsTestClass38();
                MC.intI = MyInt.intI + MyInt2;
                return MC;
            }
            public static OperatorsTestClass38 operator +(int MyInt, OperatorsTestClass38 MyInt2)
            {
                OperatorsTestClass38 MC = new OperatorsTestClass38();
                MC.intI = MyInt + MyInt2.intI + 1;
                return MC;
            }
            public static bool testMethod()
            {
                OperatorsTestClass38 Test1 = new OperatorsTestClass38();
                OperatorsTestClass38 TestClass1 = Test1 + 1;
                OperatorsTestClass38 TestClass2 = 1 + Test1;
                if ((TestClass1.intI == 3) && (TestClass2.intI == 4))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass39
        {
            public int intI = 2;
            public static OperatorsTestClass39 operator -(OperatorsTestClass39 MyInt, int MyInt2)
            {
                OperatorsTestClass39 MC = new OperatorsTestClass39();
                MC.intI = MyInt.intI + MyInt2;
                return MC;
            }
            public static OperatorsTestClass39 operator -(int MyInt, OperatorsTestClass39 MyInt2)
            {
                OperatorsTestClass39 MC = new OperatorsTestClass39();
                MC.intI = MyInt + MyInt2.intI + 1;
                return MC;
            }
            public static bool testMethod()
            {
                OperatorsTestClass39 Test1 = new OperatorsTestClass39();
                OperatorsTestClass39 TestClass1 = Test1 - 1;
                OperatorsTestClass39 TestClass2 = 1 - Test1;
                if ((TestClass1.intI == 3) && (TestClass2.intI == 4))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass40
        {
            public int intI = 2;
            public static OperatorsTestClass40 operator *(OperatorsTestClass40 MyInt, int MyInt2)
            {
                OperatorsTestClass40 MC = new OperatorsTestClass40();
                MC.intI = MyInt.intI + MyInt2;
                return MC;
            }
            public static OperatorsTestClass40 operator *(int MyInt, OperatorsTestClass40 MyInt2)
            {
                OperatorsTestClass40 MC = new OperatorsTestClass40();
                MC.intI = MyInt + MyInt2.intI + 1;
                return MC;
            }
            public static bool testMethod()
            {
                OperatorsTestClass40 Test1 = new OperatorsTestClass40();
                OperatorsTestClass40 TestClass1 = Test1 * 1;
                OperatorsTestClass40 TestClass2 = 1 * Test1;
                if ((TestClass1.intI == 3) && (TestClass2.intI == 4))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass41
        {
            public int intI = 2;
            public static OperatorsTestClass41 operator /(OperatorsTestClass41 MyInt, int MyInt2)
            {
                OperatorsTestClass41 MC = new OperatorsTestClass41();
                MC.intI = MyInt.intI + MyInt2;
                return MC;
            }
            public static OperatorsTestClass41 operator /(int MyInt, OperatorsTestClass41 MyInt2)
            {
                OperatorsTestClass41 MC = new OperatorsTestClass41();
                MC.intI = MyInt + MyInt2.intI + 1;
                return MC;
            }
            public static bool testMethod()
            {
                OperatorsTestClass41 Test1 = new OperatorsTestClass41();
                OperatorsTestClass41 TestClass1 = Test1 / 1;
                OperatorsTestClass41 TestClass2 = 1 / Test1;
                if ((TestClass1.intI == 3) && (TestClass2.intI == 4))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass42
        {
            public int intI = 2;
            public static OperatorsTestClass42 operator %(OperatorsTestClass42 MyInt, int MyInt2)
            {
                OperatorsTestClass42 MC = new OperatorsTestClass42();
                MC.intI = MyInt.intI + MyInt2;
                return MC;
            }
            public static OperatorsTestClass42 operator %(int MyInt, OperatorsTestClass42 MyInt2)
            {
                OperatorsTestClass42 MC = new OperatorsTestClass42();
                MC.intI = MyInt + MyInt2.intI + 1;
                return MC;
            }
            public static bool testMethod()
            {
                OperatorsTestClass42 Test1 = new OperatorsTestClass42();
                OperatorsTestClass42 TestClass1 = Test1 % 1;
                OperatorsTestClass42 TestClass2 = 1 % Test1;
                if ((TestClass1.intI == 3) && (TestClass2.intI == 4))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass43
        {
            public int intI = 2;
            public static OperatorsTestClass43 operator ^(OperatorsTestClass43 MyInt, int MyInt2)
            {
                OperatorsTestClass43 MC = new OperatorsTestClass43();
                MC.intI = MyInt.intI + MyInt2;
                return MC;
            }
            public static OperatorsTestClass43 operator ^(int MyInt, OperatorsTestClass43 MyInt2)
            {
                OperatorsTestClass43 MC = new OperatorsTestClass43();
                MC.intI = MyInt + MyInt2.intI + 1;
                return MC;
            }
            public static bool testMethod()
            {
                OperatorsTestClass43 Test1 = new OperatorsTestClass43();
                OperatorsTestClass43 TestClass1 = Test1 ^ 1;
                OperatorsTestClass43 TestClass2 = 1 ^ Test1;
                if ((TestClass1.intI == 3) && (TestClass2.intI == 4))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass44
        {
            public int intI = 2;
            public static OperatorsTestClass44 operator &(OperatorsTestClass44 MyInt, int MyInt2)
            {
                OperatorsTestClass44 MC = new OperatorsTestClass44();
                MC.intI = MyInt.intI + MyInt2;
                return MC;
            }
            public static OperatorsTestClass44 operator &(int MyInt, OperatorsTestClass44 MyInt2)
            {
                OperatorsTestClass44 MC = new OperatorsTestClass44();
                MC.intI = MyInt + MyInt2.intI + 1;
                return MC;
            }
            public static bool testMethod()
            {
                OperatorsTestClass44 Test1 = new OperatorsTestClass44();
                OperatorsTestClass44 TestClass1 = Test1 & 1;
                OperatorsTestClass44 TestClass2 = 1 & Test1;
                if ((TestClass1.intI == 3) && (TestClass2.intI == 4))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass45
        {
            public int intI = 2;
            public static OperatorsTestClass45 operator |(OperatorsTestClass45 MyInt, int MyInt2)
            {
                OperatorsTestClass45 MC = new OperatorsTestClass45();
                MC.intI = MyInt.intI + MyInt2;
                return MC;
            }
            public static OperatorsTestClass45 operator |(int MyInt, OperatorsTestClass45 MyInt2)
            {
                OperatorsTestClass45 MC = new OperatorsTestClass45();
                MC.intI = MyInt + MyInt2.intI + 1;
                return MC;
            }
            public static bool testMethod()
            {
                OperatorsTestClass45 Test1 = new OperatorsTestClass45();
                OperatorsTestClass45 TestClass1 = Test1 | 1;
                OperatorsTestClass45 TestClass2 = 1 | Test1;
                if ((TestClass1.intI == 3) || (TestClass2.intI == 4))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass46
        {
            public int intI = 2;
            public static bool operator ==(OperatorsTestClass46 MyInt, int MyInt2)
            {
                return (MyInt.intI != MyInt2);
            }
            public static bool operator !=(OperatorsTestClass46 MyInt, int MyInt2)
            {
                return false;
            }
            public static bool testMethod()
            {
                OperatorsTestClass46 Test1 = new OperatorsTestClass46();
                if (((Test1 == 2) == false) && ((Test1 == 4) == true))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass67
        {
            public int intI = 2;
            public static bool operator !(OperatorsTestClass67 MyInt)
            {
                MyInt.intI = 3;
                return true;
            }
            public static bool testMethod()
            {
                OperatorsTestClass67 Test = new OperatorsTestClass67();

                if ((!Test) && (Test.intI == 3))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass68
        {
            public int intI = 2;
            public static bool operator true(OperatorsTestClass68 MyInt)
            {
                MyInt.intI = 3;
                return true;
            }
            public static bool operator false(OperatorsTestClass68 MyInt)
            {
                MyInt.intI = 4;
                return false;
            }
            public static bool testMethod()
            {
                OperatorsTestClass68 Test = new OperatorsTestClass68();
                if (Test)
                {
                    if (Test.intI == 3)
                    {
                        return true;
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
        }

        class OperatorsTestClass69
        {
            public int intI = 2;
            public static bool operator true(OperatorsTestClass69 MyInt)
            {
                MyInt.intI = 3;
                return true;
            }
            public static bool operator false(OperatorsTestClass69 MyInt)
            {
                MyInt.intI = 4;
                return false;
            }
            public static OperatorsTestClass69 operator &(OperatorsTestClass69 mc1, OperatorsTestClass69 mc2)
            {
                return new OperatorsTestClass69();
            }
            public static bool testMethod()
            {
                OperatorsTestClass69 Test1 = new OperatorsTestClass69();
                OperatorsTestClass69 Test2 = new OperatorsTestClass69();

                if (Test1 && Test2)
                {
                    if ((Test1.intI == 4) && (Test2.intI == 2))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return false;
            }
        }

        class OperatorsTestClass88
        {
            public static bool retTrue()
            {
                return true;
            }

            public static bool retFalse()
            {
                return false;
            }
            public static bool testMethod()
            {
                bool retVal = true;

                if ((true & true) != true)
                    retVal = false;
                if ((true & retTrue()) != true)
                    retVal = false;
                if ((retTrue() & true) != true)
                    retVal = false;
                if ((true & false) != false)
                    retVal = false;
                if ((retTrue() & false) != false)
                    retVal = false;
                if ((true & retFalse()) != false)
                    retVal = false;
                if ((false & true) != false)
                    retVal = false;
                if ((retFalse() & true) != false)
                    retVal = false;
                if ((false & retTrue()) != false)
                    retVal = false;
                if ((false & false) != false)
                    retVal = false;
                if ((retFalse() & false) != false)
                    retVal = false;
                if ((false & retFalse()) != false)
                    retVal = false;
                return retVal;
            }
        }

        class OperatorsTestClass89
        {
            public static bool retTrue()
            {
                return true;
            }

            public static bool retFalse()
            {
                return false;
            }
            public static bool testMethod()
            {
                bool retVal = true;

                if ((true && true) != true)
                    retVal = false;
                if ((true && retTrue()) != true)
                    retVal = false;
                if ((retTrue() && true) != true)
                    retVal = false;
                if ((true && false) != false)
                    retVal = false;
                if ((retTrue() && false) != false)
                    retVal = false;
                if ((true && retFalse()) != false)
                    retVal = false;
                if ((false && true) != false)
                    retVal = false;
                if ((retFalse() && true) != false)
                    retVal = false;
                if ((false && retTrue()) != false)
                    retVal = false;
                if ((false && false) != false)
                    retVal = false;
                if ((retFalse() && false) != false)
                    retVal = false;
                if ((false && retFalse()) != false)
                    retVal = false;
                return retVal;
            }
        }

        class OperatorsTestClass90
        {
            public static bool retTrue()
            {
                return true;
            }

            public static bool retFalse()
            {
                return false;
            }
            public static bool testMethod()
            {
                bool retVal = true;

                if ((true | true) != true)
                    retVal = false;
                if ((true | retTrue()) != true)
                    retVal = false;
                if ((retTrue() | true) != true)
                    retVal = false;
                if ((true | false) != true)
                    retVal = false;
                if ((retTrue() | false) != true)
                    retVal = false;
                if ((true | retFalse()) != true)
                    retVal = false;
                if ((false | true) != true)
                    retVal = false;
                if ((retFalse() | true) != true)
                    retVal = false;
                if ((false | retTrue()) != true)
                    retVal = false;
                if ((false | false) != false)
                    retVal = false;
                if ((retFalse() | false) != false)
                    retVal = false;
                if ((false | retFalse()) != false)
                    retVal = false;
                return retVal;
            }
        }

        class OperatorsTestClass91
        {
            public static bool retTrue()
            {
                return true;
            }

            public static bool retFalse()
            {
                return false;
            }
            public static bool testMethod()
            {
                bool retVal = true;

                if ((true || true) != true)
                    retVal = false;
                if ((true || retTrue()) != true)
                    retVal = false;
                if ((retTrue() || true) != true)
                    retVal = false;
                if ((true || false) != true)
                    retVal = false;
                if ((retTrue() || false) != true)
                    retVal = false;
                if ((true || retFalse()) != true)
                    retVal = false;
                if ((false || true) != true)
                    retVal = false;
                if ((retFalse() || true) != true)
                    retVal = false;
                if ((false || retTrue()) != true)
                    retVal = false;
                if ((false || false) != false)
                    retVal = false;
                if ((retFalse() || false) != false)
                    retVal = false;
                if ((false || retFalse()) != false)
                    retVal = false;
                return retVal;
            }
        }

        class OperatorsTestClass92
        {
            public static bool retTrue()
            {
                return true;
            }

            public static bool retFalse()
            {
                return false;
            }
            public static bool testMethod()
            {
                bool retVal = true;

                if ((true ^ true) != false)
                    retVal = false;
                if ((true ^ retTrue()) != false)
                    retVal = false;
                if ((retTrue() ^ true) != false)
                    retVal = false;
                if ((true ^ false) != true)
                    retVal = false;
                if ((retTrue() ^ false) != true)
                    retVal = false;
                if ((true ^ retFalse()) != true)
                    retVal = false;
                if ((false ^ true) != true)
                    retVal = false;
                if ((retFalse() ^ true) != true)
                    retVal = false;
                if ((false ^ retTrue()) != true)
                    retVal = false;
                if ((false ^ false) != false)
                    retVal = false;
                if ((retFalse() ^ false) != false)
                    retVal = false;
                if ((false ^ retFalse()) != false)
                    retVal = false;
                return retVal;
            }
        }

        class OperatorsTestClass93
        {
            public static bool testMethod()
            {
                bool retVal = true;
                sbyte sb = 2;
                byte b = 2;
                short s = 2;
                ushort us = 2;
                int i = 2;
                uint ui = 2;
                long l = 2;
                ulong ul = 2;
                if ((sb + 0) != sb)
                    retVal = false;
                if ((0 + sb) != sb)
                    retVal = false;
                if ((b + 0) != b)
                    retVal = false;
                if ((0 + b) != b)
                    retVal = false;
                if ((s + 0) != s)
                    retVal = false;
                if ((0 + s) != s)
                    retVal = false;
                if ((us + 0) != us)
                    retVal = false;
                if ((0 + us) != us)
                    retVal = false;
                if ((i + 0) != i)
                    retVal = false;
                if ((0 + i) != i)
                    retVal = false;
                if ((ui + 0) != ui)
                    retVal = false;
                if ((0 + ui) != ui)
                    retVal = false;
                if ((l + 0) != l)
                    retVal = false;
                if ((0 + l) != l)
                    retVal = false;
                if ((ul + 0) != ul)
                    retVal = false;
                if ((0 + ul) != ul)
                    retVal = false;

                return retVal;
            }
        }

        class OperatorsTestClass94
        {
            public static bool testMethod()
            {
                bool retVal = true;
                sbyte sb = 2;
                byte b = 2;
                short s = 2;
                ushort us = 2;
                int i = 2;
                uint ui = 2;
                long l = 2;
                ulong ul = 2;
                if ((sb - 0) != sb)
                    retVal = false;
                if ((b - 0) != b)
                    retVal = false;
                if ((0 - b) != -b)
                    retVal = false;
                if ((s - 0) != s)
                    retVal = false;
                if ((0 - s) != -s)
                    retVal = false;
                if ((us - 0) != us)
                    retVal = false;
                if ((i - 0) != i)
                    retVal = false;
                if ((0 - i) != -i)
                    retVal = false;
                if ((ui - 0) != ui)
                    retVal = false;
                if ((l - 0) != l)
                    retVal = false;
                if ((0 - l) != -l)
                    retVal = false;
                if ((ul - 0) != ul)
                    retVal = false;
                return retVal;
            }
        }

        class OperatorsTestClass95
        {
            public static bool testMethod()
            {
                bool retVal = true;
                sbyte sb = 2;
                byte b = 2;
                short s = 2;
                ushort us = 2;
                int i = 2;
                uint ui = 2;
                long l = 2;
                ulong ul = 2;
                if ((sb * 0) != 0)
                    retVal = false;
                if ((0 * sb) != 0)
                    retVal = false;
                if ((b * 0) != 0)
                    retVal = false;
                if ((0 * b) != 0)
                    retVal = false;
                if ((s * 0) != 0)
                    retVal = false;
                if ((0 * s) != 0)
                    retVal = false;
                if ((us * 0) != 0)
                    retVal = false;
                if ((0 * us) != 0)
                    retVal = false;
                if ((i * 0) != 0)
                    retVal = false;
                if ((0 * i) != 0)
                    retVal = false;
                if ((ui * 0) != 0)
                    retVal = false;
                if ((0 * ui) != 0)
                    retVal = false;
                if ((l * 0) != 0)
                    retVal = false;
                if ((0 * l) != 0)
                    retVal = false;
                if ((ul * 0) != 0)
                    retVal = false;
                if ((0 * ul) != 0)
                    retVal = false;

                return retVal;
            }
        }

        class OperatorsTestClass96
        {
            public static bool testMethod()
            {
                bool retVal = true;
                sbyte sb = 2;
                byte b = 2;
                short s = 2;
                ushort us = 2;
                int i = 2;
                uint ui = 2;
                long l = 2;
                ulong ul = 2;
                if ((0 / sb) != 0)
                    retVal = false;
                if ((0 / b) != 0)
                    retVal = false;
                if ((0 / s) != 0)
                    retVal = false;
                if ((0 / us) != 0)
                    retVal = false;
                if ((0 / i) != 0)
                    retVal = false;
                if ((0 / ui) != 0)
                    retVal = false;
                if ((0 / l) != 0)
                    retVal = false;
                if ((0 / ul) != 0)
                    retVal = false;

                return retVal;
            }
        }
    }
}
