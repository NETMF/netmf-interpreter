////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ValueFloatTests : IMFTestInterface
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

        //ValueFloat Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\4_values\Value\Float
        //01,03,04,05,06,07,08,09,10,11,12,13,14,
        //15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32
        //1,3,31,32 Were Skipped in the Baseline document, all others passed
        //2,33-36 Were removed because they test Float.NaN which is not implemented
        //They were skipped in the Baseline document


        //Test Case Calls 

        [TestMethod]
        public MFTestResults ValueFloat04_Test()
        {
            Log.Comment("Testing float type is kept after addition with other types");

            if (ValueFloatTestClass04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat05_Test()
        {
            Log.Comment("Testing double type is kept after addition with other types");
            if (ValueFloatTestClass05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat06_Test()
        {
            Log.Comment("Testing float type is kept after subtraction with other types");
            if (ValueFloatTestClass06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat07_Test()
        {
            Log.Comment("Testing double type is kept after subtraction with other types");
            if (ValueFloatTestClass07.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat08_Test()
        {
            Log.Comment("Testing float type is kept after multiplication with other types");
            if (ValueFloatTestClass08.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat09_Test()
        {
            Log.Comment("Testing double type is kept after maultiplication with other types");
            if (ValueFloatTestClass09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat10_Test()
        {
            Log.Comment("Testing float type is kept after division with other types");
            if (ValueFloatTestClass10.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat11_Test()
        {
            Log.Comment("Testing double type is kept after division with other types");
            if (ValueFloatTestClass11.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat12_Test()
        {
            Log.Comment("Testing float type is kept after modulus with other types");
            if (ValueFloatTestClass12.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat13_Test()
        {
            Log.Comment("Testing double type is kept after modulus with other types");
            if (ValueFloatTestClass13.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat14_Test()
        {
            Log.Comment("Testing that equality operations return bool type objects");
            if (ValueFloatTestClass14.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat15_Test()
        {
            Log.Comment("Testing that equality operations return bool type objects");
            if (ValueFloatTestClass15.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat16_Test()
        {
            Log.Comment("Testing that non-equality operations return bool type objects");
            if (ValueFloatTestClass16.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat17_Test()
        {
            Log.Comment("Testing that non-equality operations return bool type objects");
            if (ValueFloatTestClass17.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat18_Test()
        {
            Log.Comment("Testing that greater than operations return bool type objects");
            if (ValueFloatTestClass18.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat19_Test()
        {
            Log.Comment("Testing that greater than operations return bool type objects");
            if (ValueFloatTestClass19.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat20_Test()
        {
            Log.Comment("Testing that less than operations return bool type objects");
            if (ValueFloatTestClass20.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat21_Test()
        {
            Log.Comment("Testing that less than operations return bool type objects");
            if (ValueFloatTestClass21.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat22_Test()
        {
            Log.Comment("Testing that greater than or equal operations return bool type objects");
            if (ValueFloatTestClass22.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat23_Test()
        {
            Log.Comment("Testing that greater than or equal operations return bool type objects");
            if (ValueFloatTestClass23.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat24_Test()
        {
            Log.Comment("Testing that less than or equal operations return bool type objects");
            if (ValueFloatTestClass24.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat25_Test()
        {
            Log.Comment("Testing that less than or equal operations return bool type objects");
            if (ValueFloatTestClass25.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat26_Test()
        {
            Log.Comment("Testing that double keeps its type in all operations with float");
            if (ValueFloatTestClass26.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat27_Test()
        {
            Log.Comment("Testing that comparisons between floats and doubles return bools");
            if (ValueFloatTestClass27.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat28_Test()
        {
            Log.Comment("Testing that float keeps its type after any operation with a float");
            if (ValueFloatTestClass28.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat29_Test()
        {
            Log.Comment("Testing that comparisons between floats return bools");
            if (ValueFloatTestClass29.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ValueFloat30_Test()
        {
            Log.Comment("Testing float and double .epsilon values");
            if (ValueFloatTestClass30.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }



        public class ValueFloatTestClass04
        {
            public static bool testMethod()
            {
                int intRet = 0;
                float f1 = 11.0f;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;

                if ((f1 + s1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 + f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 + b1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 + f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 + i1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 + f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 + l1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((l1 + f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 + c1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((c1 + f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass05
        {
            public static bool testMethod()
            {
                int intRet = 0;
                double d1 = 11.0d;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;

                if ((d1 + s1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 + d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 + b1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 + d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 + i1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 + d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 + l1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((l1 + d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 + c1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((c1 + d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass06
        {
            public static bool testMethod()
            {
                int intRet = 0;
                float f1 = 11.0f;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;

                if ((f1 - s1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 - f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 - b1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 - f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 - i1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 - f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 - l1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((l1 - f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 - c1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((c1 - f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass07
        {
            public static bool testMethod()
            {
                int intRet = 0;
                double d1 = 11.0d;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;

                if ((d1 - s1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 - d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 - b1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 - d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 - i1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 - d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 - l1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((l1 - d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 - c1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((c1 - d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass08
        {
            public static bool testMethod()
            {
                int intRet = 0;
                float f1 = 11.0f;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;

                if ((f1 * s1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 * f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 * b1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 * f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 * i1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 * f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 * l1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((l1 * f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 * c1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((c1 * f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass09
        {
            public static bool testMethod()
            {
                int intRet = 0;
                double d1 = 11.0d;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;

                if ((d1 * s1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 * d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 * b1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 * d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 * i1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 * d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 * l1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((l1 * d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 * c1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((c1 * d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass10
        {
            public static bool testMethod()
            {
                int intRet = 0;
                float f1 = 11.0f;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;

                if ((f1 / s1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 / f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 / b1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 / f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 / i1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 / f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 / l1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((l1 / f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 / c1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((c1 / f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass11
        {
            public static bool testMethod()
            {
                int intRet = 0;
                double d1 = 11.0d;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;

                if ((d1 / s1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 / d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 / b1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 / d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 / i1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 / d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 / l1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((l1 / d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 / c1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((c1 / d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass12
        {
            public static bool testMethod()
            {
                int intRet = 0;
                float f1 = 11.0f;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;

                if ((f1 % s1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 % f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 % b1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 % f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 % i1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 % f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 % l1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((l1 % f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 % c1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((c1 % f1).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass13
        {
            public static bool testMethod()
            {
                int intRet = 0;
                double d1 = 11.0d;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;

                if ((d1 % s1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((s1 % d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 % b1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((b1 % d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 % i1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((i1 % d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 % l1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((l1 % d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 % c1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((c1 % d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass14
        {
            public static bool testMethod()
            {
                int intRet = 0;
                bool b = true;
                float f1 = 11.0f;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;

                if ((f1 == s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s1 == f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 == b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b1 == f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 == i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i1 == f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 == l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l1 == f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 == c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((c1 == f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass15
        {
            public static bool testMethod()
            {
                int intRet = 0;

                bool b = false;
                double d1 = 11.0d;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;

                if ((d1 == s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s1 == d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 == b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b1 == d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 == i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i1 == d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 == l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l1 == d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 == c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((c1 == d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass16
        {
            public static bool testMethod()
            {
                int intRet = 0;
                bool b = true;
                float f1 = 11.0f;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;

                if ((f1 != s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s1 != f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 != b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b1 != f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 != i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i1 != f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 != l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l1 != f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 != c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((c1 != f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass17
        {
            public static bool testMethod()
            {
                int intRet = 0;

                bool b = false;
                double d1 = 11.0d;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;

                if ((d1 != s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s1 != d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 != b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b1 != d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 != i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i1 != d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 != l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l1 != d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 != c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((c1 != d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass18
        {
            public static bool testMethod()
            {
                int intRet = 0;
                bool b = true;
                float f1 = 11.0f;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;

                if ((f1 > s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s1 > f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 > b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b1 > f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 > i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i1 > f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 > l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l1 > f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 > c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((c1 > f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass19
        {
            public static bool testMethod()
            {
                int intRet = 0;

                bool b = false;
                double d1 = 11.0d;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;

                if ((d1 > s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s1 > d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 > b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b1 > d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 > i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i1 > d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 > l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l1 > d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 > c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((c1 > d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass20
        {
            public static bool testMethod()
            {
                int intRet = 0;
                bool b = true;
                float f1 = 11.0f;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;

                if ((f1 < s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s1 < f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 < b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b1 < f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 < i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i1 < f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 < l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l1 < f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 < c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((c1 < f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass21
        {
            public static bool testMethod()
            {
                int intRet = 0;

                bool b = false;
                double d1 = 11.0d;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;

                if ((d1 < s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s1 < d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 < b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b1 < d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 < i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i1 < d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 < l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l1 < d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 < c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((c1 < d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass22
        {
            public static bool testMethod()
            {
                int intRet = 0;
                bool b = true;
                float f1 = 11.0f;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;

                if ((f1 >= s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s1 >= f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 >= b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b1 >= f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 >= i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i1 >= f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 >= l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l1 >= f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 >= c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((c1 >= f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass23
        {
            public static bool testMethod()
            {
                int intRet = 0;

                bool b = false;
                double d1 = 11.0d;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;

                if ((d1 >= s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s1 >= d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 >= b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b1 >= d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 >= i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i1 >= d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 >= l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l1 >= d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 >= c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((c1 >= d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass24
        {
            public static bool testMethod()
            {
                int intRet = 0;
                bool b = true;
                float f1 = 11.0f;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;

                if ((f1 <= s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s1 <= f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 <= b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b1 <= f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 <= i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i1 <= f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 <= l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l1 <= f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 <= c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((c1 <= f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass25
        {
            public static bool testMethod()
            {
                int intRet = 0;

                bool b = false;
                double d1 = 11.0d;
                short s1 = 2;
                byte b1 = 3;
                int i1 = 4;
                long l1 = 5L;
                char c1 = (char)6;

                if ((d1 <= s1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((s1 <= d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 <= b1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((b1 <= d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 <= i1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((i1 <= d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 <= l1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((l1 <= d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 <= c1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((c1 <= d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass26
        {
            public static bool testMethod()
            {
                int intRet = 0;
                double d1 = 11.0d;
                float f1 = 12.0f;

                if ((d1 + f1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 + d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 + d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 - f1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 - d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 - d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 * f1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 * d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 * d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 / f1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 / d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 / d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 % f1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 % d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                if ((d1 % d1).GetType() != d1.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass27
        {
            public static bool testMethod()
            {
                int intRet = 0;
                double d1 = 11.0d;
                float f1 = 12.0f;
                bool b = false;
                if ((d1 == f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 == d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 == d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 != f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 != d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 != d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 > f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 > d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 > d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 < f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 < d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 < d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 >= f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 >= d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 >= d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 <= f1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 <= d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((d1 <= d1).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass28
        {
            public static bool testMethod()
            {
                int intRet = 0;
                float f1 = 11.0f;
                float f2 = 12.0f;

                if ((f1 + f2).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 - f2).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 * f2).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 / f2).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                if ((f1 % f2).GetType() != f1.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass29
        {
            public static bool testMethod()
            {
                int intRet = 0;
                double f1 = 11.0d;
                float f2 = 12.0f;
                bool b = false;
                if ((f1 == f2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 != f2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 > f2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 < f2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 >= f2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                if ((f1 <= f2).GetType() != b.GetType())
                {
                    intRet = 1;
                }
                return (intRet == 0);
            }
        }
        public class ValueFloatTestClass30
        {
            public static bool testMethod()
            {
                float f1 = float.Epsilon;
                double d1 = double.Epsilon;
                if ((float)(f1 / 2.0f) != 0.0f)
                {
                    return false;
                }
                if ((float)(f1 * 0.5f) != 0.0f)
                {
                    return false;
                }
                if ((double)(d1 / 2.0d) != (double)0.0d)
                {
                    return false;
                }
                if ((double)(d1 * 0.5d) != (double)0.0d)
                {
                    return false;
                }
                return true;
            }
        }

        

    }
}
