////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ValueTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");
            Log.Comment(" Section 4.1");
            Log.Comment(" Assignment to a variable of a value type creates a copy of the value");
            Log.Comment(" being assigned.  This differs from assignment to a variable of a ");
            Log.Comment(" reference type, which copies the reference but not the object identified");
            Log.Comment(" by the reference.");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
        }

        //Value Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\4_values\Value
        //7,8,9

        //Test Case Calls 
        [TestMethod]
        public MFTestResults Value7_Test()
        {
            if (ValueTestClass7.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Value8_Test()
        {
            if (ValueTestClass8.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Value9_Test()
        {
            if (ValueTestClass9.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        //Compiled Test Cases 
        public struct ValueTestClass7_Struct
        {
            public int MyInt;
        }
        public class ValueTestClass7
        {
            public static bool testMethod()
            {
                ValueTestClass7_Struct MS = new ValueTestClass7_Struct();
                ValueTestClass7_Struct MS2;

                MS.MyInt = 3;
                MS2 = MS;
                MS.MyInt = 4;

                if (MS2.MyInt == 3)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueTestClass8
        {
            public static bool testMethod()
            {
                int MyInt;
                int MyInt2;

                MyInt = 3;
                MyInt2 = MyInt;
                MyInt = 4;
                if (MyInt2 == 3)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        enum ValueTestClass9_Enum { a = 1, b = 2 }
        public class ValueTestClass9
        {
            public static bool testMethod()
            {
                ValueTestClass9_Enum Enum1;
                ValueTestClass9_Enum Enum2;
                Enum1 = ValueTestClass9_Enum.a;
                Enum2 = Enum1;
                Enum1 = ValueTestClass9_Enum.b;
                if ((int)Enum2 == (int)ValueTestClass9_Enum.a)
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
