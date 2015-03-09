////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class Static_InstTests : IMFTestInterface
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

        //Static_Inst Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\Static_Inst

        //Test Case Calls 
        [TestMethod]
        public MFTestResults Static_Inst01_Test()
        {
            Log.Comment(" Section 10.2 ");
            Log.Comment(" When a static member is referenced in a member-access");
            Log.Comment(" of the form E.M, E must denote a type. It is an error for");
            Log.Comment(" E to denote an instance.");
            if (Static_InstTestClass01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Static_Inst07_Test()
        {
            Log.Comment(" Section 10.2 ");
            Log.Comment(" A static field identifies exactly one storage location.");
            Log.Comment(" No matter how many instances of a class are created,");
            Log.Comment(" there is only ever one copy of a static field.");
            if (Static_InstTestClass07.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Static_Inst14_Test()
        {
            Log.Comment(" Section 10.2 ");
            Log.Comment(" When an instance member is referenced in a member-access");
            Log.Comment(" of the form E.M, E must denote an instance. It is an error ");
            Log.Comment(" for E to denote a type.");
            if (Static_InstTestClass14.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Static_Inst18_Test()
        {
            Log.Comment(" Section 10.2 ");
            Log.Comment(" Every instance of a class contains a separate copy ");
            Log.Comment(" of all instance fields of the class.");
            if (Static_InstTestClass18.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Static_Inst19_Test()
        {
            Log.Comment(" Section 10.2 ");
            Log.Comment(" An instance function member (method, property ");
            Log.Comment(" accessor, indexer accessor, constructor, or ");
            Log.Comment(" destructor) operates on a given instance of ");
            Log.Comment(" the class, and this instance can be accessed as");
            Log.Comment(" this.");
            if (Static_InstTestClass19.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Static_Inst20_Test()
        {
            Log.Comment(" Section 10.2 ");
            Log.Comment(" An instance function member (method, property ");
            Log.Comment(" accessor, indexer accessor, constructor, or ");
            Log.Comment(" destructor) operates on a given instance of ");
            Log.Comment(" the class, and this instance can be accessed as");
            Log.Comment(" this.");
            if (Static_InstTestClass20.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Static_Inst21_Test()
        {
            Log.Comment(" Section 10.2 ");
            Log.Comment(" An instance function member (method, property ");
            Log.Comment(" accessor, indexer accessor, constructor, or ");
            Log.Comment(" destructor) operates on a given instance of ");
            Log.Comment(" the class, and this instance can be accessed as");
            Log.Comment(" this.");
            if (Static_InstTestClass21.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Static_Inst22_Test()
        {
            Log.Comment(" Section 10.2 ");
            Log.Comment(" An instance function member (method, property ");
            Log.Comment(" accessor, indexer accessor, constructor, or ");
            Log.Comment(" destructor) operates on a given instance of ");
            Log.Comment(" the class, and this instance can be accessed as");
            Log.Comment(" this.");
            if (Static_InstTestClass22.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Static_Inst23_Test()
        {
            Log.Comment(" Section 10.2 ");
            Log.Comment(" An instance function member (method, property ");
            Log.Comment(" accessor, indexer accessor, constructor, or ");
            Log.Comment(" destructor) operates on a given instance of ");
            Log.Comment(" the class, and this instance can be accessed as");
            Log.Comment(" this.");
            if (Static_InstTestClass23.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        //Compiled Test Cases 
        class Static_InstTestClass01
        {
            public static int intI = 1;
            public static int intJ()
            {
                return 2;
            }
            public static int intK
            {
                get
                {
                    return 3;
                }
            }
            public const int intL = 4;
            public class MyInner
            {
                public int intM = 5;
            }
            public static bool testMethod()
            {
                if (Static_InstTestClass01.intI != 1) return false;
                if (Static_InstTestClass01.intJ() != 2) return false;
                if (Static_InstTestClass01.intK != 3) return false;
                if (Static_InstTestClass01.intL != 4) return false;
                if (new Static_InstTestClass01.MyInner().intM != 5) return false;
                return true;
            }
        }
        class Static_InstTestClass07
        {
            public static int intI = 1;
            public void increment()
            {
                Static_InstTestClass07.intI++;
            }
            public static bool testMethod() {

		if (Static_InstTestClass07.intI != 1) return false;
		Static_InstTestClass07 test1 = new Static_InstTestClass07();
		test1.increment();
		if (Static_InstTestClass07.intI != 2) return false;			
		Static_InstTestClass07 test2 = new Static_InstTestClass07();
		test2.increment();
		if (Static_InstTestClass07.intI != 3) return false;
		Static_InstTestClass07 test3 = new Static_InstTestClass07();
		test3.increment();
		if (Static_InstTestClass07.intI != 4) return false;
		return true;
	}
        }
        class Static_InstTestClass14
        {
            public int intI = 1;
            public int intJ()
            {
                return 2;
            }
            public int intK
            {
                get
                {
                    return 3;
                }
            }
            public static bool testMethod()
            {
                Static_InstTestClass14 test = new Static_InstTestClass14();
                if (test.intI != 1) return false;
                if (test.intJ() != 2) return false;
                if (test.intK != 3) return false;
                return true;
                
            }
        }
        class Static_InstTestClass18
        {
            public int intI = 1;
            public void setInt(int intJ)
            {
                intI = intJ;
            }
            public static bool testMethod() {

		Static_InstTestClass18 test1 = new Static_InstTestClass18();
		Static_InstTestClass18 test2 = new Static_InstTestClass18();
		Static_InstTestClass18 test3 = new Static_InstTestClass18();
		test1.setInt(2);
		test2.setInt(3);
		test3.setInt(4);	
		if (test1.intI != 2) return false;
		if (test2.intI != 3) return false;
		if (test3.intI != 4) return false;
		return true;
	}
        }
        class Static_InstTestClass19
        {
            int intI;
            public void foo()
            {
                intI = 2;
            }
            public static bool testMethod()
            {
                Static_InstTestClass19 test = new Static_InstTestClass19();
                test.foo();
                if (test.intI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        class Static_InstTestClass20
        {
            int intI;
            public int intJ
            {
                get
                {
                    intI = 2;
                    return 3;
                }
            }
            public static bool testMethod()
            {
                Static_InstTestClass20 test = new Static_InstTestClass20();
                int intK = test.intJ;
                if (test.intI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        class Static_InstTestClass21
        {
            int intI;
            public int this[int intJ]
            {
                get
                {
                    intI = 2;
                    return 3;
                }
            }
            public static bool testMethod()
            {
                Static_InstTestClass21 test = new Static_InstTestClass21();
                int intK = test[1];
                if (test.intI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        class Static_InstTestClass22
        {
            int intI;
            public Static_InstTestClass22()
            {
                intI = 2;
            }
            public static bool testMethod()
            {
                Static_InstTestClass22 test = new Static_InstTestClass22();
                if (test.intI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        class Static_InstTestClass23
        {
            int intI;
            public void foo()
            {
                this.intI = 2;
            }
            public static bool testMethod()
            {
                Static_InstTestClass23 test = new Static_InstTestClass23();
                test.foo();
                if (test.intI == 2)
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

