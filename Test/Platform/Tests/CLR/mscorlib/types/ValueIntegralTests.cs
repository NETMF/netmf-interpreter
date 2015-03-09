////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ValueIntegralTests : IMFTestInterface
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

        //ValueIntegral Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\4_values\Values\Integral
        //01,05,09,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,38,39,
        //42,45,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,70
        //These test all passed in the Baseline document

            //Test Case Calls 
        [TestMethod]
        public MFTestResults ValueIntegral01_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the +, - and ~ unary operators, of the operand is of type long, the");
            Log.Comment(" operation is performed using 64-bit precision, and the type of the result");
            Log.Comment(" is long.  Otherwise, the operand is converted to int, and operation is");
            Log.Comment(" performed using 32-bit precision, and the type of the result is int.");
            if (ValueIntegralTestClass01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral05_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the +, - and ~ unary operators, of the operand is of type long, the");
            Log.Comment(" operation is performed using 64-bit precision, and the type of the result");
            Log.Comment(" is long.  Otherwise, the operand is converted to int, and operation is");
            Log.Comment(" performed using 32-bit precision, and the type of the result is int.");
            if (ValueIntegralTestClass05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral09_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the +, - and ~ unary operators, of the operand is of type long, the");
            Log.Comment(" operation is performed using 64-bit precision, and the type of the result");
            Log.Comment(" is long.  Otherwise, the operand is converted to int, and operation is");
            Log.Comment(" performed using 32-bit precision, and the type of the result is int.");
            if (ValueIntegralTestClass09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral13_Test()
        {
            Log.Comment(" For the binary operators other than shift, if at least one");
            Log.Comment(" operand is of type long, then both operands are converted to long, the operation");
            Log.Comment(" is performed using 64-bit precision, and the type of the result is long or bool. ");
            Log.Comment(" Otherwise, both operands are converted to int, the operation is performed using ");
            Log.Comment(" 32-bit precision, and the type of the result is int or bool.");
            if (ValueIntegralTestClass13.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral14_Test()
        {
            Log.Comment(" For the binary operators other than shift, if at least one");
            Log.Comment(" operand is of type long, then both operands are converted to long, the operation");
            Log.Comment(" is performed using 64-bit precision, and the type of the result is long or bool. ");
            Log.Comment(" Otherwise, both operands are converted to int, the operation is performed using ");
            Log.Comment(" 32-bit precision, and the type of the result is int or bool.");
            if (ValueIntegralTestClass14.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral15_Test()
        {
            Log.Comment(" For the binary operators other than shift, if at least one");
            Log.Comment(" operand is of type long, then both operands are converted to long, the operation");
            Log.Comment(" is performed using 64-bit precision, and the type of the result is long or bool. ");
            Log.Comment(" Otherwise, both operands are converted to int, the operation is performed using ");
            Log.Comment(" 32-bit precision, and the type of the result is int or bool.");
            if (ValueIntegralTestClass15.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral16_Test()
        {
            Log.Comment(" For the binary operators other than shift, if at least one");
            Log.Comment(" operand is of type long, then both operands are converted to long, the operation");
            Log.Comment(" is performed using 64-bit precision, and the type of the result is long or bool. ");
            Log.Comment(" Otherwise, both operands are converted to int, the operation is performed using ");
            Log.Comment(" 32-bit precision, and the type of the result is int or bool.");
            if (ValueIntegralTestClass16.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral17_Test()
        {
            Log.Comment(" For the binary operators other than shift, if at least one");
            Log.Comment(" operand is of type long, then both operands are converted to long, the operation");
            Log.Comment(" is performed using 64-bit precision, and the type of the result is long or bool. ");
            Log.Comment(" Otherwise, both operands are converted to int, the operation is performed using ");
            Log.Comment(" 32-bit precision, and the type of the result is int or bool.");
            if (ValueIntegralTestClass17.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral18_Test()
        {
            Log.Comment(" For the binary operators other than shift, if at least one");
            Log.Comment(" operand is of type long, then both operands are converted to long, the operation");
            Log.Comment(" is performed using 64-bit precision, and the type of the result is long or bool. ");
            Log.Comment(" Otherwise, both operands are converted to int, the operation is performed using ");
            Log.Comment(" 32-bit precision, and the type of the result is int or bool.");
            if (ValueIntegralTestClass18.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral19_Test()
        {
            Log.Comment(" For the binary operators other than shift, if at least one");
            Log.Comment(" operand is of type long, then both operands are converted to long, the operation");
            Log.Comment(" is performed using 64-bit precision, and the type of the result is long or bool. ");
            Log.Comment(" Otherwise, both operands are converted to int, the operation is performed using ");
            Log.Comment(" 32-bit precision, and the type of the result is int or bool.");
            if (ValueIntegralTestClass19.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral20_Test()
        {
            Log.Comment(" For the binary operators other than shift, if at least one");
            Log.Comment(" operand is of type long, then both operands are converted to long, the operation");
            Log.Comment(" is performed using 64-bit precision, and the type of the result is long or bool. ");
            Log.Comment(" Otherwise, both operands are converted to int, the operation is performed using ");
            Log.Comment(" 32-bit precision, and the type of the result is int or bool.");
            if (ValueIntegralTestClass20.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral21_Test()
        {
            Log.Comment(" For the binary operators other than shift, if at least one");
            Log.Comment(" operand is of type long, then both operands are converted to long, the operation");
            Log.Comment(" is performed using 64-bit precision, and the type of the result is long or bool. ");
            Log.Comment(" Otherwise, both operands are converted to int, the operation is performed using ");
            Log.Comment(" 32-bit precision, and the type of the result is int or bool.");
            if (ValueIntegralTestClass21.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral22_Test()
        {
            Log.Comment(" For the binary operators other than shift, if at least one");
            Log.Comment(" operand is of type long, then both operands are converted to long, the operation");
            Log.Comment(" is performed using 64-bit precision, and the type of the result is long or bool. ");
            Log.Comment(" Otherwise, both operands are converted to int, the operation is performed using ");
            Log.Comment(" 32-bit precision, and the type of the result is int or bool.");
            if (ValueIntegralTestClass22.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral23_Test()
        {
            Log.Comment(" For the binary operators other than shift, if at least one");
            Log.Comment(" operand is of type long, then both operands are converted to long, the operation");
            Log.Comment(" is performed using 64-bit precision, and the type of the result is long or bool. ");
            Log.Comment(" Otherwise, both operands are converted to int, the operation is performed using ");
            Log.Comment(" 32-bit precision, and the type of the result is int or bool.");
            if (ValueIntegralTestClass23.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral24_Test()
        {
            Log.Comment(" For the binary operators other than shift, if at least one");
            Log.Comment(" operand is of type long, then both operands are converted to long, the operation");
            Log.Comment(" is performed using 64-bit precision, and the type of the result is long or bool. ");
            Log.Comment(" Otherwise, both operands are converted to int, the operation is performed using ");
            Log.Comment(" 32-bit precision, and the type of the result is int or bool.");
            if (ValueIntegralTestClass24.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral25_Test()
        {
            Log.Comment(" For the binary operators other than shift, if at least one");
            Log.Comment(" operand is of type long, then both operands are converted to long, the operation");
            Log.Comment(" is performed using 64-bit precision, and the type of the result is long or bool. ");
            Log.Comment(" Otherwise, both operands are converted to int, the operation is performed using ");
            Log.Comment(" 32-bit precision, and the type of the result is int or bool.");
            if (ValueIntegralTestClass25.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral26_Test()
        {
            Log.Comment(" For the binary operators other than shift, if at least one");
            Log.Comment(" operand is of type long, then both operands are converted to long, the operation");
            Log.Comment(" is performed using 64-bit precision, and the type of the result is long or bool. ");
            Log.Comment(" Otherwise, both operands are converted to int, the operation is performed using ");
            Log.Comment(" 32-bit precision, and the type of the result is int or bool.");
            if (ValueIntegralTestClass26.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral27_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the shift operators, if the left-hand operand is of type long,");
            Log.Comment(" the operation is performed using 64-bit precision, and the type of the result");
            Log.Comment(" is long.  Otherwise, the left hand-operand is converted to int, the operation is ");
            Log.Comment(" performed using 32-bit precision, and the type of the result is int.");
            if (ValueIntegralTestClass27.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral28_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the shift operators, if the left-hand operand is of type long,");
            Log.Comment(" the operation is performed using 64-bit precision, and the type of the result");
            Log.Comment(" is long.  Otherwise, the left hand-operand is converted to int, the operation is ");
            Log.Comment(" performed using 32-bit precision, and the type of the result is int.");
            if (ValueIntegralTestClass28.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral38_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" Constants of the char type must be written as character-literals.");
            Log.Comment(" Character constants can only be written as integer-literals");
            Log.Comment(" in combination with a cast. For example, (char)10 is the same as");
            Log.Comment(" '\x000A'.");
            if (ValueIntegralTestClass38.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral39_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the unary + and ~ operators, the operand is converted");
            Log.Comment(" to type T, where T is the first of int, uint, long, and");
            Log.Comment(" ulong that can fully represent all possible values of the");
            Log.Comment(" operand. The operation is then performed using the precision ");
            Log.Comment(" of type T, and the type of the result is T.");
            if (ValueIntegralTestClass39.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral42_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the unary + and ~ operators, the operand is converted");
            Log.Comment(" to type T, where T is the first of int, uint, long, and");
            Log.Comment(" ulong that can fully represent all possible values of the");
            Log.Comment(" operand. The operation is then performed using the precision ");
            Log.Comment(" of type T, and the type of the result is T.");
            if (ValueIntegralTestClass42.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral45_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the unary - operator, the operand is converted");
            Log.Comment(" to type T, where T is the first of int and long that ");
            Log.Comment(" can fully represent all possible values of the ");
            Log.Comment(" operand.  The operation is then performed using the");
            Log.Comment(" precision of type T, and the type of the result is T.");
            Log.Comment(" The unary - operator cannot be applied to operands of");
            Log.Comment(" type ulong.");
            if (ValueIntegralTestClass45.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral49_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the binary operators except shift, the operands");
            Log.Comment(" are converted to type T, where T is the first of int, uint, long, and ulong");
            Log.Comment(" that can fully represent all possible values of each operand. The operation");
            Log.Comment(" is then performed using the precision of type T, and the type of the result");
            Log.Comment(" is T (or bool for relational operators).");
            if (ValueIntegralTestClass49.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral50_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the binary operators except shift, the operands");
            Log.Comment(" are converted to type T, where T is the first of int, uint, long, and ulong");
            Log.Comment(" that can fully represent all possible values of each operand. The operation");
            Log.Comment(" is then performed using the precision of type T, and the type of the result");
            Log.Comment(" is T (or bool for relational operators).");
            if (ValueIntegralTestClass50.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral51_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the binary operators except shift, the operands");
            Log.Comment(" are converted to type T, where T is the first of int, uint, long, and ulong");
            Log.Comment(" that can fully represent all possible values of each operand. The operation");
            Log.Comment(" is then performed using the precision of type T, and the type of the result");
            Log.Comment(" is T (or bool for relational operators).");
            if (ValueIntegralTestClass51.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral52_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the binary operators except shift, the operands");
            Log.Comment(" are converted to type T, where T is the first of int, uint, long, and ulong");
            Log.Comment(" that can fully represent all possible values of each operand. The operation");
            Log.Comment(" is then performed using the precision of type T, and the type of the result");
            Log.Comment(" is T (or bool for relational operators).");
            if (ValueIntegralTestClass52.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral53_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the binary operators except shift, the operands");
            Log.Comment(" are converted to type T, where T is the first of int, uint, long, and ulong");
            Log.Comment(" that can fully represent all possible values of each operand. The operation");
            Log.Comment(" is then performed using the precision of type T, and the type of the result");
            Log.Comment(" is T (or bool for relational operators).");
            if (ValueIntegralTestClass53.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral54_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the binary operators except shift, the operands");
            Log.Comment(" are converted to type T, where T is the first of int, uint, long, and ulong");
            Log.Comment(" that can fully represent all possible values of each operand. The operation");
            Log.Comment(" is then performed using the precision of type T, and the type of the result");
            Log.Comment(" is T (or bool for relational operators).");
            if (ValueIntegralTestClass54.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral55_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the binary operators except shift, the operands");
            Log.Comment(" are converted to type T, where T is the first of int, uint, long, and ulong");
            Log.Comment(" that can fully represent all possible values of each operand. The operation");
            Log.Comment(" is then performed using the precision of type T, and the type of the result");
            Log.Comment(" is T (or bool for relational operators).");
            if (ValueIntegralTestClass55.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral56_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the binary operators except shift, the operands");
            Log.Comment(" are converted to type T, where T is the first of int, uint, long, and ulong");
            Log.Comment(" that can fully represent all possible values of each operand. The operation");
            Log.Comment(" is then performed using the precision of type T, and the type of the result");
            Log.Comment(" is T (or bool for relational operators).");
            if (ValueIntegralTestClass56.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral57_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the binary operators except shift, the operands");
            Log.Comment(" are converted to type T, where T is the first of int, uint, long, and ulong");
            Log.Comment(" that can fully represent all possible values of each operand. The operation");
            Log.Comment(" is then performed using the precision of type T, and the type of the result");
            Log.Comment(" is T (or bool for relational operators).");
            if (ValueIntegralTestClass57.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral58_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the binary operators except shift, the operands");
            Log.Comment(" are converted to type T, where T is the first of int, uint, long, and ulong");
            Log.Comment(" that can fully represent all possible values of each operand. The operation");
            Log.Comment(" is then performed using the precision of type T, and the type of the result");
            Log.Comment(" is T (or bool for relational operators).");
            if (ValueIntegralTestClass58.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral59_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the binary operators except shift, the operands");
            Log.Comment(" are converted to type T, where T is the first of int, uint, long, and ulong");
            Log.Comment(" that can fully represent all possible values of each operand. The operation");
            Log.Comment(" is then performed using the precision of type T, and the type of the result");
            Log.Comment(" is T (or bool for relational operators).");
            if (ValueIntegralTestClass59.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral60_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the binary operators except shift, the operands");
            Log.Comment(" are converted to type T, where T is the first of int, uint, long, and ulong");
            Log.Comment(" that can fully represent all possible values of each operand. The operation");
            Log.Comment(" is then performed using the precision of type T, and the type of the result");
            Log.Comment(" is T (or bool for relational operators).");
            if (ValueIntegralTestClass60.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral61_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the binary operators except shift, the operands");
            Log.Comment(" are converted to type T, where T is the first of int, uint, long, and ulong");
            Log.Comment(" that can fully represent all possible values of each operand. The operation");
            Log.Comment(" is then performed using the precision of type T, and the type of the result");
            Log.Comment(" is T (or bool for relational operators).");
            if (ValueIntegralTestClass61.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral62_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the binary operators except shift, the operands");
            Log.Comment(" are converted to type T, where T is the first of int, uint, long, and ulong");
            Log.Comment(" that can fully represent all possible values of each operand. The operation");
            Log.Comment(" is then performed using the precision of type T, and the type of the result");
            Log.Comment(" is T (or bool for relational operators).");
            if (ValueIntegralTestClass62.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral63_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the binary shift operators, the left operand");
            Log.Comment(" is converted to type T, where T is the first of int,");
            Log.Comment(" uint, long, and ulong that can fully represent all possible");
            Log.Comment(" values of the operand. The operation is then performed");
            Log.Comment(" using the precision of type T, and the type of the result");
            Log.Comment(" T.");
            if (ValueIntegralTestClass63.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral64_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" For the binary shift operators, the left operand");
            Log.Comment(" is converted to type T, where T is the first of int,");
            Log.Comment(" uint, long, and ulong that can fully represent all possible");
            Log.Comment(" values of the operand. The operation is then performed");
            Log.Comment(" using the precision of type T, and the type of the result");
            Log.Comment(" T.");
            if (ValueIntegralTestClass64.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueIntegral70_Test()
        {
            Log.Comment(" Section 4.1");
            Log.Comment(" Constants of the char type must be written as character-literals.");
            Log.Comment(" Character constants can only be written as integer-literals");
            Log.Comment(" in compination with a cast.  For example, (char)10 is the same ");
            Log.Comment(" as '\x000a'.");
            if (ValueIntegralTestClass70.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }


        //Compiled Test Cases 
        public class ValueIntegralTestClass01
        {
            public static bool testMethod()
            {
                short s1 = 2;
                short s2 = (short)-s1;
                byte b1 = 3;
                int b2 = -b1;
                int i1 = 4;
                int i2 = -i1;
                long l1 = 5L;
                long l2 = -l1;
                char c1 = (char)6;
                int c2 = -c1;

                if ((s2 == (short)-2) && (b2 == -3) && (i2 == -4) && (l2 == -5L) && (c2 == -6))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueIntegralTestClass05
        {
            public static bool testMethod()
            {
                short s1 = 2;
                short s2 = (short)+s1;
                byte b1 = 3;
                int b2 = +b1;
                int i1 = 4;
                int i2 = +i1;
                long l1 = 5;
                long l2 = +l1;
                char c1 = (char)6;
                int c2 = +c1;

                if ((s2 == (short)2) && (b2 == 3) && (i2 == 4) && (l2 == 5L) && (c2 == 6))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueIntegralTestClass09
        {
            public static bool testMethod()
            {
                short s1 = 2;
                short s2 = (short)~s1;
                byte b1 = 3;
                int b2 = ~b1;
                int i1 = 4;
                int i2 = ~i1;
                long l1 = 5L;
                long l2 = ~l1;
                char c1 = (char)6;
                int c2 = ~c1;

                if ((s2 == (short)-3) && (b2 == -4) && (i2 == -5) && (l2 == -6L) && (c2 == -7))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueIntegralTestClass13
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                //short
                if ((s1 + s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 + b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 + i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 + l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 + c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }

                //byte
                if ((b1 + b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 + i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 + l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 + c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                //int
                if ((i1 + i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 + l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 + c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                //long
                if ((l1 + l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((l1 + c1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                //char
                if ((c1 + c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }

                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass14
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                //short
                if ((s1 - s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 - b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 - i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 - l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 - c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }

                //byte
                if ((b1 - b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 - i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 - l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 - c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                //int
                if ((i1 - i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 - l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 - c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                //long
                if ((l1 - l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((l1 - c1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                //char
                if ((c1 - c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass15
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                //short
                if ((s1 * s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 * b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 * i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 * l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 * c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }

                //byte
                if ((b1 * b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 * i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 * l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 * c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                //int
                if ((i1 * i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 * l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 * c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                //long
                if ((l1 * l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((l1 * c1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                //char
                if ((c1 * c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass16
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                //short
                if ((s1 / s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 / b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 / i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 / l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 / c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }

                //byte
                if ((b1 / b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 / i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 / l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 / c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                //int
                if ((i1 / i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 / l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 / c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                //long
                if ((l1 / l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((l1 / c1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                //char
                if ((c1 / c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass17
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                //short
                if ((s1 % s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 % b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 % i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 % l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 % c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }

                //byte
                if ((b1 % b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 % i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 % l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 % c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                //int
                if ((i1 % i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 % l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 % c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                //long
                if ((l1 % l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((l1 % c1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                //char
                if ((c1 % c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass18
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                //short
                if ((s1 & s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 & b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 & i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 & l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 & c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }

                //byte
                if ((b1 & b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 & i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 & l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 & c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                //int
                if ((i1 & i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 & l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 & c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                //long
                if ((l1 & l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((l1 & c1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                //char
                if ((c1 & c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }

                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass19
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                //short
                if ((s1 ^ s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 ^ b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 ^ i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 ^ l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 ^ c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }

                //byte
                if ((b1 ^ b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 ^ i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 ^ l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 ^ c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                //int
                if ((i1 ^ i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 ^ l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 ^ c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                //long
                if ((l1 ^ l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((l1 ^ c1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                //char
                if ((c1 ^ c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }

                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass20
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                //short
                if ((s1 | s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 | b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 | i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 | l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 | c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }

                //byte
                if ((b1 | b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 | i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 | l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 | c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                //int
                if ((i1 | i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 | l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 | c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                //long
                if ((l1 | l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((l1 | c1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                //char
                if ((c1 | c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }

                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass21
        {
            public static bool testMethod()
            {
                int intRet = 0;
                bool TestBool = false;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                //short
                if ((s1 == s1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 == b1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 == i1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 == l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 == c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }

                //byte
                if ((b1 == b1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((b1 == i1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((b1 == l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((b1 == c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                //int
                if ((i1 == i1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((i1 == l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((i1 == c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                //long
                if ((l1 == l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((l1 == c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                //char
                if ((c1 == c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass22
        {
            public static bool testMethod()
            {
                int intRet = 0;
                bool TestBool = false;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                //short
                if ((s1 != s1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 != b1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 != i1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 != l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 != c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }

                //byte
                if ((b1 != b1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((b1 != i1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((b1 != l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((b1 != c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                //int
                if ((i1 != i1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((i1 != l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((i1 != c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                //long
                if ((l1 != l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((l1 != c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                //char
                if ((c1 != c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }

                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass23
        {
            public static bool testMethod()
            {
                int intRet = 0;
                bool TestBool = false;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                //short
                if ((s1 > s1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 > b1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 > i1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 > l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 > c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }

                //byte
                if ((b1 > b1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((b1 > i1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((b1 > l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((b1 > c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                //int
                if ((i1 > i1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((i1 > l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((i1 > c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                //long
                if ((l1 > l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((l1 > c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                //char
                if ((c1 > c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }

                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass24
        {
            public static bool testMethod()
            {
                int intRet = 0;
                bool TestBool = false;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                //short
                if ((s1 < s1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 < b1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 < i1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 < l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 < c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }

                //byte
                if ((b1 < b1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((b1 < i1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((b1 < l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((b1 < c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                //int
                if ((i1 < i1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((i1 < l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((i1 < c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                //long
                if ((l1 < l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((l1 < c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                //char
                if ((c1 < c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }

                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass25
        {
            public static bool testMethod()
            {
                int intRet = 0;
                bool TestBool = false;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                //short
                if ((s1 >= s1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 >= b1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 >= i1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 >= l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 >= c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }

                //byte
                if ((b1 >= b1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((b1 >= i1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((b1 >= l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((b1 >= c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                //int
                if ((i1 >= i1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((i1 >= l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((i1 >= c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                //long
                if ((l1 >= l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((l1 >= c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                //char
                if ((c1 >= c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }

                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass26
        {
            public static bool testMethod()
            {
                int intRet = 0;
                bool TestBool = false;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                //short
                if ((s1 <= s1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 <= b1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 <= i1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 <= l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((s1 <= c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }

                //byte
                if ((b1 <= b1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((b1 <= i1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((b1 <= l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((b1 <= c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                //int
                if ((i1 <= i1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((i1 <= l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((i1 <= c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                //long
                if ((l1 <= l1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                if ((l1 <= c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }
                //char
                if ((c1 <= c1).GetType() != TestBool.GetType())
                {
                    intRet = 1;
                }

                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass27
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                if ((s1 << 1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 << 1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 << 1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((l1 << 1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((c1 << 1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }

                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass28
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                if ((s1 >> 1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 >> 1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 >> 1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((l1 >> 1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((c1 >> 1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }

                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass38
        {
            public static bool testMethod()
            {
                char c = '\x000A';
                if (c == (char)10)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueIntegralTestClass39
        {
            public static bool testMethod()
            {
                ushort s1 = 2;
                ushort s2 = (ushort)+s1;
                sbyte b1 = 3;
                int b2 = +b1;
                uint i1 = 4;
                uint i2 = +i1;
                ulong l1 = 5ul;
                ulong l2 = +l1;

                if ((s2 == (ushort)2) && (b2 == 3) && (i2 == 4) && (l2 == 5ul))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueIntegralTestClass42
        {
            public static bool testMethod()
            {
                checked
                {
                    ushort s1 = 2;
                    int s2 = ~s1;
                    sbyte b1 = 3;
                    int b2 = ~b1;
                    uint i1 = 4;
                    uint i2 = ~i1;
                    ulong l1 = 5ul;
                    ulong l2 = ~l1;

                    if ((s2 == -3) && (b2 == -4) && (i2 == 4294967291u) && (l2 == 18446744073709551610ul))
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
        public class ValueIntegralTestClass45
        {
            public static bool testMethod()
            {
                ushort s1 = 2;
                int s2 = -s1;
                sbyte b1 = 3;
                int b2 = -b1;
                uint i1 = 4;
                long i2 = -i1;

                if ((s2 == -2) && (b2 == -3) && (i2 == -4))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueIntegralTestClass49
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                ushort s2 = 7;
                sbyte b2 = 8;
                uint i2 = 9;
                ulong l2 = 10;
                //ushort
                if ((s2 + s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 + b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 + i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 + l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 + c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 + s2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 + b2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 + i2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((s2 + l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }

                //sbyte
                if ((b2 + s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 + b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 + i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 + l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 + c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 + s2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 + b2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 + i2).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                //if ((b2 + l2).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                //uint
                if ((i2 + s1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 + b1).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 + i1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 + l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 + c1).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 + s2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 + b2).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 + i2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 + l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //ulong
                //if ((l2 + s1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 + b1).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 + i1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                //if ((l2 + l1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 + c1).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                if ((l2 + s2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 + b2).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 + i2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                if ((l2 + l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }

                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass50
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                ushort s2 = 7;
                sbyte b2 = 8;
                uint i2 = 9;
                ulong l2 = 10;
                //ushort
                if ((s2 - s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 - b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 - i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 - l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 - c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 - s2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 - b2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 - i2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((s2 - l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }

                //sbyte
                if ((b2 - s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 - b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 - i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 - l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 - c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 - s2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 - b2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 - i2).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                //if ((b2 - l2).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                //uint
                if ((i2 - s1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 - b1).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 - i1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 - l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 - c1).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 - s2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 - b2).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 - i2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 - l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //ulong
                //if ((l2 - s1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 - b1).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 - i1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                //if ((l2 - l1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 - c1).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                if ((l2 - s2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 - b2).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 - i2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                if ((l2 - l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }

                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass51
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                ushort s2 = 7;
                sbyte b2 = 8;
                uint i2 = 9;
                ulong l2 = 10;
                //ushort
                if ((s2 * s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 * b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 * i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 * l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 * c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 * s2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 * b2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 * i2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((s2 * l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }

                //sbyte
                if ((b2 * s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 * b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 * i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 * l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 * c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 * s2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 * b2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 * i2).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                //if ((b2 * l2).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                //uint
                if ((i2 * s1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 * b1).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 * i1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 * l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 * c1).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 * s2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 * b2).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 * i2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 * l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //ulong
                //if ((l2 * s1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 * b1).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 * i1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                //if ((l2 * l1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 * c1).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                if ((l2 * s2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 * b2).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 * i2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                if ((l2 * l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }

                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass52
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                ushort s2 = 7;
                sbyte b2 = 8;
                uint i2 = 9;
                ulong l2 = 10;
                //ushort
                if ((s2 / s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 / b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 / i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 / l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 / c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 / s2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 / b2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 / i2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((s2 / l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }

                //sbyte
                if ((b2 / s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 / b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 / i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 / l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 / c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 / s2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 / b2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 / i2).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                //if ((b2 / l2).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                //uint
                if ((i2 / s1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 / b1).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 / i1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 / l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 / c1).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 / s2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 / b2).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 / i2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 / l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //ulong
                //if ((l2 / s1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 / b1).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 / i1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                //if ((l2 / l1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 / c1).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                if ((l2 / s2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 / b2).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 / i2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                if ((l2 / l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }

                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass53
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                ushort s2 = 7;
                sbyte b2 = 8;
                uint i2 = 9;
                ulong l2 = 10;
                //ushort
                if ((s2 % s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 % b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 % i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 % l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 % c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 % s2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 % b2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 % i2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((s2 % l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }

                //sbyte
                if ((b2 % s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 % b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 % i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 % l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 % c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 % s2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 % b2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 % i2).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                //if ((b2 % l2).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                //uint
                if ((i2 % s1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 % b1).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 % i1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 % l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 % c1).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 % s2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 % b2).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 % i2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 % l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //ulong
                //if ((l2 % s1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 % b1).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 % i1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                //if ((l2 % l1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 % c1).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                if ((l2 % s2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 % b2).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 % i2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                if ((l2 % l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }

                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass54
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                ushort s2 = 7;
                sbyte b2 = 8;
                uint i2 = 9;
                ulong l2 = 10;
                //ushort
                if ((s2 & s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 & b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 & i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 & l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 & c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 & s2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 & b2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 & i2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((s2 & l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }

                //sbyte
                if ((b2 & s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 & b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 & i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 & l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 & c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 & s2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 & b2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 & i2).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                //if ((b2 & l2).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                //uint
                if ((i2 & s1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 & b1).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 & i1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 & l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 & c1).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 & s2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 & b2).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 & i2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 & l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //ulong
                //if ((l2 & s1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 & b1).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 & i1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                //if ((l2 & l1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 & c1).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                if ((l2 & s2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 & b2).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 & i2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                if ((l2 & l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }

                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass55
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                ushort s2 = 7;
                sbyte b2 = 8;
                uint i2 = 9;
                ulong l2 = 10;
                //ushort
                if ((s2 ^ s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 ^ b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 ^ i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 ^ l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 ^ c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 ^ s2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 ^ b2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 ^ i2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((s2 ^ l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }

                //sbyte
                if ((b2 ^ s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 ^ b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 ^ i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 ^ l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 ^ c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 ^ s2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 ^ b2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 ^ i2).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                //if ((b2 ^ l2).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                //uint
                if ((i2 ^ s1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 ^ b1).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 ^ i1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 ^ l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 ^ c1).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 ^ s2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 ^ b2).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 ^ i2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 ^ l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //ulong
                //if ((l2 ^ s1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 ^ b1).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 ^ i1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                //if ((l2 ^ l1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 ^ c1).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                if ((l2 ^ s2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 ^ b2).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 ^ i2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                if ((l2 ^ l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }

                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass56
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                ushort s2 = 7;
                sbyte b2 = 8;
                uint i2 = 9;
                ulong l2 = 10;
                //ushort
                if ((s2 | s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 | b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 | i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 | l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 | c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 | s2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 | b2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((s2 | i2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((s2 | l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }

                //sbyte
                if ((b2 | s1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 | b1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 | i1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 | l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 | c1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 | s2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 | b2).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((b2 | i2).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                //if ((b2 | l2).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                //uint
                if ((i2 | s1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 | b1).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 | i1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 | l1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 | c1).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 | s2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 | b2).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                if ((i2 | i2).GetType() != i2.GetType())
                {
                    intRet = 1;
                }
                if ((i2 | l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //ulong
                //if ((l2 | s1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 | b1).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 | i1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                //if ((l2 | l1).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 | c1).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                if ((l2 | s2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 | b2).GetType() != l2.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 | i2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }
                if ((l2 | l2).GetType() != l2.GetType())
                {
                    intRet = 1;
                }

                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass57
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                ushort s2 = 7;
                sbyte b2 = 8;
                uint i2 = 9;
                ulong l2 = 10;
                bool b = true;
                //ushort
                if ((s2 == s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 == b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 == i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 == l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 == c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 == s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 == b2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 == i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 == l2).GetType() != b.GetType())
                {
                    intRet = 1;
                }

                //sbyte
                if ((b2 == s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 == b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 == i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 == l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 == c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 == s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 == b2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 == i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //if ((b2 == l2).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                //uint
                if ((i2 == s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 == b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 == i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 == l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 == c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 == s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 == b2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 == i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 == l2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //ulong
                //if ((l2 == s1).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 == b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 == i1).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                //if ((l2 == l1).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 == c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l2 == s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 == b2).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 == i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l2 == l2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass58
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                ushort s2 = 7;
                sbyte b2 = 8;
                uint i2 = 9;
                ulong l2 = 10;
                bool b = true;
                //ushort
                if ((s2 != s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 != b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 != i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 != l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 != c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 != s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 != b2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 != i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 != l2).GetType() != b.GetType())
                {
                    intRet = 1;
                }

                //sbyte
                if ((b2 != s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 != b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 != i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 != l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 != c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 != s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 != b2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 != i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //if ((b2 != l2).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                //uint
                if ((i2 != s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 != b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 != i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 != l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 != c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 != s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 != b2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 != i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 != l2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //ulong
                //if ((l2 != s1).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 != b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 != i1).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                //if ((l2 != l1).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 != c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l2 != s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 != b2).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 != i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l2 != l2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass59
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                ushort s2 = 7;
                sbyte b2 = 8;
                uint i2 = 9;
                ulong l2 = 10;
                bool b = true;
                //ushort
                if ((s2 > s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 > b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 > i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 > l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 > c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 > s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 > b2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 > i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 > l2).GetType() != b.GetType())
                {
                    intRet = 1;
                }

                //sbyte
                if ((b2 > s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 > b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 > i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 > l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 > c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 > s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 > b2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 > i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //if ((b2 > l2).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                //uint
                if ((i2 > s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 > b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 > i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 > l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 > c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 > s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 > b2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 > i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 > l2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //ulong
                //if ((l2 > s1).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 > b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 > i1).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                //if ((l2 > l1).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 > c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l2 > s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 > b2).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 > i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l2 > l2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass60
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                ushort s2 = 7;
                sbyte b2 = 8;
                uint i2 = 9;
                ulong l2 = 10;
                bool b = true;
                //ushort
                if ((s2 < s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 < b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 < i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 < l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 < c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 < s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 < b2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 < i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 < l2).GetType() != b.GetType())
                {
                    intRet = 1;
                }

                //sbyte
                if ((b2 < s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 < b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 < i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 < l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 < c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 < s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 < b2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 < i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //if ((b2 < l2).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                //uint
                if ((i2 < s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 < b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 < i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 < l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 < c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 < s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 < b2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 < i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 < l2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //ulong
                //if ((l2 < s1).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 < b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 < i1).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                //if ((l2 < l1).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 < c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l2 < s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 < b2).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 < i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l2 < l2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass61
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                ushort s2 = 7;
                sbyte b2 = 8;
                uint i2 = 9;
                ulong l2 = 10;
                bool b = true;
                //ushort
                if ((s2 >= s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 >= b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 >= i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 >= l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 >= c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 >= s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 >= b2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 >= i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 >= l2).GetType() != b.GetType())
                {
                    intRet = 1;
                }

                //sbyte
                if ((b2 >= s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 >= b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 >= i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 >= l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 >= c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 >= s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 >= b2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 >= i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //if ((b2 >= l2).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                //uint
                if ((i2 >= s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 >= b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 >= i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 >= l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 >= c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 >= s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 >= b2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 >= i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 >= l2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //ulong
                //if ((l2 >= s1).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 >= b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 >= i1).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                //if ((l2 >= l1).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 >= c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l2 >= s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 >= b2).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 >= i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l2 >= l2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass62
        {
            public static bool testMethod()
            {
                int intRet = 0;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;
                ushort s2 = 7;
                sbyte b2 = 8;
                uint i2 = 9;
                ulong l2 = 10;
                bool b = true;
                //ushort
                if ((s2 <= s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 <= b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 <= i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 <= l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 <= c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 <= s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 <= b2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 <= i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s2 <= l2).GetType() != b.GetType())
                {
                    intRet = 1;
                }

                //sbyte
                if ((b2 <= s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 <= b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 <= i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 <= l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 <= c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 <= s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 <= b2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b2 <= i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //if ((b2 <= l2).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                //uint
                if ((i2 <= s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 <= b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 <= i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 <= l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 <= c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 <= s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 <= b2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 <= i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i2 <= l2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //ulong
                //if ((l2 <= s1).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 <= b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 <= i1).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                //if ((l2 <= l1).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 <= c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l2 <= s2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                //if ((l2 <= b2).GetType() != b.GetType()) {
                //	intRet = 1;
                //}
                if ((l2 <= i2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l2 <= l2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass63
        {
            public static bool testMethod()
            {
                int intRet = 0;
                ushort s1 = 2;
                sbyte b1 = 3;
                uint i1 = 4;
                ulong l1 = 5L;
                int i = 1;

                if ((s1 << 1).GetType() != i.GetType())
                {
                    intRet = 1;
                }
                if ((b1 << 1).GetType() != i.GetType())
                {
                    intRet = 1;
                }
                if ((i1 << 1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((l1 << 1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass64
        {
            public static bool testMethod()
            {
                int intRet = 0;
                ushort s1 = 2;
                sbyte b1 = 3;
                uint i1 = 4;
                ulong l1 = 5L;
                int i = 1;

                if ((s1 >> 1).GetType() != i.GetType())
                {
                    intRet = 1;
                }
                if ((b1 >> 1).GetType() != i.GetType())
                {
                    intRet = 1;
                }
                if ((i1 >> 1).GetType() != i1.GetType())
                {
                    intRet = 1;
                }
                if ((l1 >> 1).GetType() != l1.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueIntegralTestClass70
        {
            public static bool testMethod()
            {
                char c = (char)10;
                if (c == '\x000a')
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
