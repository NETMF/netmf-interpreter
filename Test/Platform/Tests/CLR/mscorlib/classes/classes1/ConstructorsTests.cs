////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ConstructorsTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            // Add your functionality here.                

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
        }

        //Constructors Tests
        //All test methods ported from folder current\test\cases\client\CLR\Conformance\10_classes\constructors
        //The following tests were removed because they were build failure tests:
        //12,14,15,18,29,30,35-43,48,49,59,60-62,65,67,69-74
        //The following tests were removed because they require additional files to run
        //58,66,68. 58 Failed in the Baseline document, 66 and 68 were not included
        //Test 63 would not compile because it referenced System.Type.GetConstructors() which doesn't exist, 
        //it was skipped in the baseline document
        //Test 63a was removed because it caused Null reference exceptions, it failed in the Baseline doc
        //47,58,63a Failed

        [TestMethod]
        public MFTestResults Constructors1_Test()
        {
            //Ported from Const1.cs
            Log.Comment("Tests if assignments in a constructor function.");
            if (ConstructorsTestClass1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors2_Test()
        {
            //Ported from Const2.cs
            Log.Comment("Tests if assignments in a constructor function, when constructor is public.");
            if (ConstructorsTestClass2.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors3_Test()
        {
            //Ported from Const3.cs
            Log.Comment("Tests if assignments in a constructor function, when constructor is protected.");
            if (ConstructorsTestClass3.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;

        }

        [TestMethod]
        public MFTestResults Constructors5_Test()
        {
            //Ported from Const5.cs
            Log.Comment("Tests if assignments in a constructor function, when constructor is internal.");
            if (ConstructorsTestClass5.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors6_Test()
        {
            //Ported from Const6.cs
            Log.Comment("Tests if assignments in a constructor function, when constructor is private.");
            if (ConstructorsTestClass6.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors8_Test()
        {
            //Ported from Const8.cs
            Log.Comment("Tests if assignments in a constructor function, when constructor has one parameter.");
            if (ConstructorsTestClass8.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors10_Test()
        {
            //Ported from Const10.cs

            Log.Comment("Tests if assignments in a constructor function, when constructor is called with one parameter");
            Log.Comment("and is overloaded with an un-called zero parameter version");
            if (ConstructorsTestClass10.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        
        [TestMethod]
        public MFTestResults Constructors11_Test()
        {
            //Ported from Const11.cs
            Log.Comment("Tests if assignments in a constructor function, when constructor has two parameters.");
            if (ConstructorsTestClass11.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors13_Test()
        {
            //Ported from Const13.cs
            Log.Comment("Tests if assignments in a constructor function, when constructor has ten parameters.");
            if (ConstructorsTestClass13.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors16_Test()
        {
            //Ported from Const16.cs
            Log.Comment("Tests if assignments in a constructor function, when test class inherits constructor");

            Log.Comment("and extends it with base");
            if (ConstructorsTestClass16.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors17_Test()
        {
            //Ported from Const17.cs
            Log.Comment("Tests if assignments in a constructor function, when test class inherits 2 constructors");
            Log.Comment("and extends one of them with base");
            if (ConstructorsTestClass17.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors20_Test()
        {
            //Ported from Const20.cs
            Log.Comment("Tests if assignments in a constructor and its base are both functional");
            if (ConstructorsTestClass20.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors21_Test()
        {
            //Ported from Const21.cs
            Log.Comment("Tests if assignments in a constructor and its base, and its base's base are all functional");
            if (ConstructorsTestClass21.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors22_Test()
        {
            //Ported from Const22.cs
            Log.Comment("Tests if assignments in both a class' constructors are functional when a parametered constructor extends");
            Log.Comment("a not-parametered one with 'this'");
            if (ConstructorsTestClass22.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors23_Test()
        {
            //Ported from Const23.cs
            Log.Comment("Tests if assignments in both a class' constructors are functional when a not-parametered constructor extends");
            Log.Comment("a parametered one with 'this'");
            if (ConstructorsTestClass23.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors24_Test()
        {
            //Ported from Const24.cs
            Log.Comment("Tests if assignments in all a class' constructors are functional in a chain of extension using 'this'");
            if (ConstructorsTestClass24.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors25_Test()
        {
            //Ported from Const25.cs

            Log.Comment("Tests if assignments in all a class' constructors are functional when a parametered one extends a");
            Log.Comment("not-parametered one, which in turn extends the class' base class constructor");
            if (ConstructorsTestClass25.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors26_Test()
        {
            //Ported from Const26.cs            
            Log.Comment("Tests if assignments in all a class' constructors are functional when a not-parametered one extends a");
            Log.Comment("not-parametered one in its base class, which in turn extends a parametered one in the base class");
            if (ConstructorsTestClass26.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors27_Test()
        {
            //Ported from Const27.cs
            Log.Comment("Tests if assignments in both a class' constructors are functional when a two-parametered constructor extends");
            Log.Comment("a one-parametered one with 'this'");
            if (ConstructorsTestClass27.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors28_Test()
        {
            //Ported from Const28.cs
            Log.Comment("Tests if assignments in both a class' constructors are functional when a two-parametered constructor extends");
            Log.Comment("a one-parametered one with 'this' and calls that constructor with a static arg");
            if (ConstructorsTestClass28.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        
        [TestMethod]
        public MFTestResults Constructors31_Test()
        {
            //Ported from Const31.cs
            Log.Comment("Tests if assignments in both a class' constructors are functional when a not-parametered constructor extends");
            Log.Comment("a two-parametered one with 'this'");
            if (ConstructorsTestClass31.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors32_Test()
        {
            //Ported from Const32.cs

            Log.Comment("Tests if assignments in both a class' constructors are functional when a parametered constructor extends");
            Log.Comment("a not-parametered one that is private with 'this'");
            if (ConstructorsTestClass32.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors33_Test()
        {
            //Ported from Const33.cs

            Log.Comment("Tests if assignments in a class' constructor are functional when the constructor is static");
            if (ConstructorsTestClass33.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors34_Test()
        {
            //Ported from Const34.cs
            Log.Comment("Tests if assignments in a class' constructor are functional when one constructor is static");
            Log.Comment("and the other isn't");
            if (ConstructorsTestClass34.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors35_Test()
        {
            //From Bug# 16354/16719
            Log.Comment("Tests if handled exceptions in constructors continues execution");
            if (ConstructorsTestClass35.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

	[TestMethod]
        public MFTestResults Constructors44_Test()
        {
            //Ported from Const44.cs
            Log.Comment("Section 10.9.5");
            Log.Comment("When a class declares only private constructors it ");
            Log.Comment("is not possible for other classes to derive from");
            Log.Comment("the class or create instances of the class (an System.Exception");
            Log.Comment("being classes nested within the class).");
            if (ConstructorsTestClass44.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors45_Test()
        {
            //Ported from Const45.cs
            Log.Comment("Section 10.11.");
            Log.Comment("It is possible to construct circular dependencies that");
            Log.Comment("allow static fields with variable initializers to be");
            Log.Comment("observed in their default value state.");
            if (ConstructorsTestClass45.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        //We initialize static constructors on load rather when enumerated, so order is not guaranteed (unlike desktop)

        //[TestMethod]
        //public MFTestResults Constructors46_Test()
        //{
        //    //Ported from Const46.cs
        //    Log.Comment("Section 10.11.");
        //    Log.Comment("It is possible to construct circular dependencies that");
        //    Log.Comment("allow static fields with variable initializers to be");
        //    Log.Comment("observed in their default value state.");
        //    if (ConstructorsTestClass46.testMethod())
        //    {
        //        return MFTestResults.Pass;
        //    }
        //    return MFTestResults.Fail;
        //}

        //[TestMethod]
        //public MFTestResults Constructors47_Test()
        //{
        //    //Ported from Const47.cs
        //    Log.Comment("Section 10.11.");
        //    Log.Comment("It is possible to construct circular dependencies that");
        //    Log.Comment("allow static fields with variable initializers to be");
        //    Log.Comment("observed in their default value state.");
        //    Log.Comment("This test is expected to fail.");
        //    if (ConstructorsTestClass47.testMethod())
        //    {
        //        return MFTestResults.Fail;
        //    }
        //    return MFTestResults.Pass;
        //}
        
        [TestMethod]
        public MFTestResults Constructors50_Test()
        {
            //Ported from Const50.cs
            Log.Comment("The scope of the parameters given by the formal");
            Log.Comment("parameter list of a constructor includes the");
            Log.Comment("constructor initializer of that declaration.");
            Log.Comment("Thus, a constructor initializer is permitted to");
            Log.Comment("access the parameters of the constructor.");
            if (ConstructorsTestClass50.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors51_Test()
        {
            //Ported from Const51.cs
            Log.Comment("Section 10.9");
            Log.Comment("The scope of the parameters given by the formal");
            Log.Comment("parameter list of a constructor includes the");
            Log.Comment("constructor initializer of that declaration.");
            Log.Comment("Thus, a constructor initializer is permitted to");
            Log.Comment("access the parameters of the constructor.");
            if (ConstructorsTestClass51.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors52_Test()
        {
            Log.Comment("Testing a constructor with a '(params int[] values)' prototype, called with 3 ints");
            //Ported from Const52.cs
            if (ConstructorsTestClass52.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors54_Test()
        {
            //Ported from Const54.cs
            Log.Comment("Testing a constructor with a '(params int[] values)' prototype, called with 3 ints from its derived class");
            if (ConstructorsTestClass54.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors55_Test()
        {
            //Ported from Const55.cs

            Log.Comment("Testing a constructor with a '(params int[] values)' prototype, called with 3 ints");
            Log.Comment(" from its derived class, and both constructors are 'protected internal'");
            if (ConstructorsTestClass55.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors56_Test()
        {
            Log.Comment("Testing a constructor with a '(params int[] values)' prototype, called with 3 ints");
            Log.Comment(" from its derived class implicitly, and both constructors are 'internal'");
            //Ported from Const56.cs
            if (ConstructorsTestClass56.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors57_Test()
        {
            //Ported from Const57.cs
            Log.Comment("Testing a 'private' constructor with a '(params int[] values)' prototype, called with 3 ints");
            if (ConstructorsTestClass57.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Constructors64_Test()
        {
            //Ported from Const64.cs
            Log.Comment("Instance constructors, destructors, and static constructors are not inherited");
            if (ConstructorsTestClass64.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        //Constructors Test Classes
        class ConstructorsTestClass1
        {

            //no modifier
            ConstructorsTestClass1()
            {
                intI = 2;
            }

            int intI;

            public static bool testMethod()
            {
                ConstructorsTestClass1 test = new ConstructorsTestClass1();
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

        class ConstructorsTestClass2
        {
            //public
            public ConstructorsTestClass2()
            {
                intI = 2;
            }

            int intI;

            public static bool testMethod()
            {
                ConstructorsTestClass2 test = new ConstructorsTestClass2();
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

        class ConstructorsTestClass3
        {

            //protected
            protected ConstructorsTestClass3()
            {
                intI = 2;
            }

            int intI;

            public static bool testMethod()
            {
                ConstructorsTestClass3 test = new ConstructorsTestClass3();
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

        class ConstructorsTestClass5
        {

            //internal
            internal ConstructorsTestClass5()
            {
                intI = 2;
            }

            int intI;

            public static bool testMethod()
            {
                ConstructorsTestClass5 test = new ConstructorsTestClass5();
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

        class ConstructorsTestClass6
        {

            //private
            private ConstructorsTestClass6()
            {
                intI = 2;
            }

            int intI;

            public static bool testMethod()
            {
                ConstructorsTestClass6 test = new ConstructorsTestClass6();
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


        class ConstructorsTestClass8
        {

            //one parameter
            ConstructorsTestClass8(int intX)
            {
                intI = intX;
            }

            int intI;

            public static bool testMethod()
            {
                ConstructorsTestClass8 test = new ConstructorsTestClass8(3);
                if (test.intI == 3)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        class ConstructorsTestClass10
        {

            ConstructorsTestClass10()
            {
                intI = 2;
            }

            ConstructorsTestClass10(int intX)
            {
                intI = intX;
            }

            int intI;

            public static bool testMethod()
            {

                ConstructorsTestClass10 test = new ConstructorsTestClass10(3); //calling constructor with parameter
                if (test.intI == 3)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstructorsTestClass11
        {

            //multiple parameters
            ConstructorsTestClass11(int intX, int intY)
            {
                intI = intX + intY;
            }

            int intI;

            public static bool testMethod()
            {
                ConstructorsTestClass11 test = new ConstructorsTestClass11(3, 4);
                if (test.intI == 7)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        class ConstructorsTestClass13
        {	

        //multiple parameters
        ConstructorsTestClass13(int int1, int int2, int int3, int int4, int int5, int int6, int int7, int int8, int int9, int int10) {
	        intI = int1 + int2 + int3 + int4 + int5 + int6 + int7 + int8 + int9 + int10;
        }

        int intI;

        public static bool testMethod() {
	        ConstructorsTestClass13 test = new ConstructorsTestClass13(1,2,3,4,5,6,7,8,9,10);
	        if (test.intI == 55) {
		        return true;
	        }
	        else {
		        return false;
            }
	        }
        }




        class ConstructorsTestClass16_Base {

	        public int intI = 1;

	        protected ConstructorsTestClass16_Base() {
		        intI = 2;
	        }
        }

        class ConstructorsTestClass16 : ConstructorsTestClass16_Base {	

	        //base constructor initializer
	        ConstructorsTestClass16() : base() {
		        intI = intI * 2;
	        }

	        public static bool testMethod() {

		        ConstructorsTestClass16 Test = new ConstructorsTestClass16();
        			
		        if (Test.intI == 4) {
			        return true;
		        }
		        else {
			        return false;
                 }
		    }
        }

        class ConstructorsTestClass17_Base {

            public int intI = 1;

            protected ConstructorsTestClass17_Base() {
	            intI = 2;
            }
            protected ConstructorsTestClass17_Base(int intJ) {
	            intI = intJ;
            }
        }

        class ConstructorsTestClass17 : ConstructorsTestClass17_Base
        {	
            //base constructor initializer
            ConstructorsTestClass17() : base(3) {
	            intI = intI * 2;
            }

            public static bool testMethod() {

	            ConstructorsTestClass17 Test = new ConstructorsTestClass17();
        			
	            if (Test.intI == 6) {
		            return true;
	            }
	            else {
		            return false;
                }   
	        }
        }

        class ConstructorsTestClass20_Base
        {

            public int intI = 1;

            protected ConstructorsTestClass20_Base(int intJ, int intK)
            {
                intI = intJ + intK;
            }
        }

        class ConstructorsTestClass20 : ConstructorsTestClass20_Base
        {

            //base constructor initializer
            ConstructorsTestClass20()
                : base(3, 4)
            {
                intI = intI * 2;
            }

            public static bool testMethod()
            {

                ConstructorsTestClass20 Test = new ConstructorsTestClass20();

                if (Test.intI == 14)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstructorsTestClass21_Base1
        {
            public int intI = 1;

            protected ConstructorsTestClass21_Base1()
            {
                intI = 2;
            }
        }

        class ConstructorsTestClass21_Base2 : ConstructorsTestClass21_Base1
        {

            protected ConstructorsTestClass21_Base2()
                : base()
            {
                intI = intI * 2;
            }

        }

        class ConstructorsTestClass21 : ConstructorsTestClass21_Base2
        {

            //base constructor initializer
            ConstructorsTestClass21()
                : base()
            {
                intI = intI * 2;
            }

            public static bool testMethod()
            {
                ConstructorsTestClass21 test = new ConstructorsTestClass21();
                if (test.intI == 8)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstructorsTestClass22
        {

            int intI = 1;

            ConstructorsTestClass22()
            {
                intI = 2;
            }


            //this constructor initializer
            ConstructorsTestClass22(int intJ)
                : this()
            {
                intI = intI * intJ;
            }

            public static bool testMethod()
            {

                ConstructorsTestClass22 Test = new ConstructorsTestClass22(2);

                if (Test.intI == 4)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        class ConstructorsTestClass23
        {

            int intI = 1;

            ConstructorsTestClass23()
                : this(3)
            {
                intI = intI * 2;
            }

            //this constructor initializer
            ConstructorsTestClass23(int intJ)
            {
                intI = intJ;
            }

            public static bool testMethod()
            {

                ConstructorsTestClass23 Test = new ConstructorsTestClass23();

                if (Test.intI == 6)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        class ConstructorsTestClass24
        {

            int intI = 1;

            ConstructorsTestClass24()
            {
                intI = 2;
            }


            //this constructor initializer
            ConstructorsTestClass24(int intJ)
                : this()
            {
                intI = intI * intJ;
            }

            //this constructor initializer
            ConstructorsTestClass24(int intK, int intL)
                : this(3)
            {
                intI = (intI + intK) * intL;
            }



            public static bool testMethod()
            {

                ConstructorsTestClass24 Test = new ConstructorsTestClass24(3, 4);

                if (Test.intI == 36)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstructorsTestClass25_Base
        {

            public int intI = 1;

            protected ConstructorsTestClass25_Base()
            {
                intI = 2;
            }
        }

        class ConstructorsTestClass25 : ConstructorsTestClass25_Base
        {

            //base constructor initializer
            ConstructorsTestClass25()
                : base()
            {
                intI = intI * 2;
            }

            //this constructor initializer
            ConstructorsTestClass25(int intJ)
                : this()
            {
                intI = intI + intJ;
            }

            public static bool testMethod()
            {

                ConstructorsTestClass25 Test = new ConstructorsTestClass25(3);

                if (Test.intI == 7)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstructorsTestClass26_Base
        {

            public int intI = 1;

            //this constructor initializer
            protected ConstructorsTestClass26_Base()
                : this(3)
            {
                intI = intI * 2;
            }

            protected ConstructorsTestClass26_Base(int intJ)
            {
                intI = intJ;
            }

        }

        class ConstructorsTestClass26 : ConstructorsTestClass26_Base
        {

            //base constructor initializer
            ConstructorsTestClass26()
                : base()
            {
                intI = intI * 2;
            }

            public static bool testMethod()
            {

                ConstructorsTestClass26 Test = new ConstructorsTestClass26();

                if (Test.intI == 12)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstructorsTestClass27
        {

            int intI = 1;

            ConstructorsTestClass27(int intJ)
            {
                intI = intJ;
            }

            //this constructor initializer
            ConstructorsTestClass27(int intK, int intL)
                : this(intL)
            {
                intI = intI + intK;
            }



            public static bool testMethod()
            {

                ConstructorsTestClass27 Test = new ConstructorsTestClass27(3, 4);

                if (Test.intI == 7)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstructorsTestClass28
        {

            static int int3 = 3;

            int intI = 1;

            ConstructorsTestClass28(int intJ)
            {
                intI = intJ;
            }

            //this constructor initializer
            ConstructorsTestClass28(int intK, int intL)
                : this(ConstructorsTestClass28.int3)
            {
                intI = (intI + intK) * intL;
            }



            public static bool testMethod()
            {

                ConstructorsTestClass28 Test = new ConstructorsTestClass28(3, 4);

                if (Test.intI == 24)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        class ConstructorsTestClass31
        {

            int intI = 1;

            ConstructorsTestClass31()
                : this(3, 4)
            {
                intI = intI * 2;
            }

            //this constructor initializer
            ConstructorsTestClass31(int intK, int intL)
            {
                intI = intK + intL;
            }



            public static bool testMethod()
            {

                ConstructorsTestClass31 Test = new ConstructorsTestClass31();

                if (Test.intI == 14)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        class ConstructorsTestClass32
        {

            int intI = 1;

            private ConstructorsTestClass32()
            {
                intI = 2;
            }


            //this constructor initializer
            ConstructorsTestClass32(int intJ)
                : this()
            {
                intI = intI * intJ;
            }

            public static bool testMethod()
            {

                ConstructorsTestClass32 Test = new ConstructorsTestClass32(2);

                if (Test.intI == 4)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        class ConstructorsTestClass33
        {

            //static constructor
            static ConstructorsTestClass33()
            {
                intI = 2;
            }

            static int intI = 1;

            public static bool testMethod()
            {
                if (intI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstructorsTestClass34 {	

	        //static constructor
	        static ConstructorsTestClass34() {
		        intI = 2;
	        }

	        ConstructorsTestClass34() {
		        intI = 3;
	        }

	        static int intI = 1;

            public static bool testMethod()
            {

                bool RetVal = true;

                if (intI != 2)
                {
                    RetVal = false;
                }

                ConstructorsTestClass34 Test = new ConstructorsTestClass34();

                if (intI != 3)
                {
                    RetVal = false;
                }

                return RetVal;
            }
	    }

	class ConstructorsTestClass35 {

		// static constructor - with Exception
		static ConstructorsTestClass35() {
			try { throw new Exception(); }
			catch { }
			intI = 5;			
		}

		static int intI = 1;

		public static bool testMethod()
		{
			return (intI == 5);
		}
	}


        class ConstructorsTestClass44_Base
        {

            protected int MyInt = 2;

            private ConstructorsTestClass44_Base() { }
            public ConstructorsTestClass44_Base(int intI)
            {
                MyInt = intI;
            }
        }

        class ConstructorsTestClass44 : ConstructorsTestClass44_Base
        {

            public ConstructorsTestClass44() : base(1) { }

            public static bool testMethod()
            {
                ConstructorsTestClass44 test = new ConstructorsTestClass44();
                if (test.MyInt == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstructorsTestClass45
        {

            int intI;

            private ConstructorsTestClass45()
            {
                intI = 1;
            }

            public static bool testMethod()
            {
                return InnerClass.testMethod();
            }

            class InnerClass
            {
                public static bool testMethod()
                {
                    ConstructorsTestClass45 test = new ConstructorsTestClass45();
                    if (test.intI == 1)
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

        class ConstructorsTestClass46_sub
        {
            public static int X = ConstructorsTestClass46.Y + 1;
        }

        class ConstructorsTestClass46
        {
            public static int Y = ConstructorsTestClass46_sub.X + 1;
            public static bool testMethod()
            {
                if ((ConstructorsTestClass46_sub.X == 1) && (ConstructorsTestClass46.Y == 2))
                {
                    return true;
                }
                else
                {
                    Log.Exception("Expected X==1, got X=" + ConstructorsTestClass46_sub.X);
                    Log.Exception("Expected Y==2, got Y=" + ConstructorsTestClass46.Y);
                    return false;
                }
            }
        }

        class ConstructorsTestClass47
        {
            public static int X = ConstructorsTestClass47_sub.Y + 1;
            public static bool testMethod()
            {
                if ((ConstructorsTestClass47.X == 2) && (ConstructorsTestClass47_sub.Y == 1))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        
        class ConstructorsTestClass47_sub
        {
	        public static int Y = ConstructorsTestClass47.X + 1;
        }

        class ConstructorsTestClass50_Base
        {
            public int intI;
            public ConstructorsTestClass50_Base(int x, int y)
            {
                intI = x * 2 + y * 3;
            }
        }

        class ConstructorsTestClass50 : ConstructorsTestClass50_Base
        {
            public ConstructorsTestClass50(int x, int y) : base(x + y, x - y) { }
            public static bool testMethod()
            {
                ConstructorsTestClass50 test = new ConstructorsTestClass50(5, 3);
                if (test.intI == 22)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        class ConstructorsTestClass51
        {

            int intI;
            public ConstructorsTestClass51(int x, int y, int z)
            {
                intI = x * 2 + y * 3;
            }
            public ConstructorsTestClass51(int x, int y) : this(x + y, x - y, 0) { }

            public static bool testMethod()
            {
                ConstructorsTestClass51 test = new ConstructorsTestClass51(5, 3);
                if (test.intI == 22)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstructorsTestClass52
        {

            int intTest;

            public ConstructorsTestClass52(params int[] values)
            {
                intTest = values[0] + values[1] + values[2];
            }

            public static bool testMethod()
            {

                int intI = 1;
                int intJ = 2;
                int intK = 3;

                ConstructorsTestClass52 mc = new ConstructorsTestClass52(intI, intJ, intK);
                if (mc.intTest == 6)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstructorsTestClass53
        {

            int intTest;

            public ConstructorsTestClass53(params int[] values)
            {
                intTest = values[0] + values[1] + values[2];
            }

            public static bool testMethod()
            {

                int intI = 1;
                int intJ = 2;
                int intK = 3;

                ConstructorsTestClass53 mc = new ConstructorsTestClass53(intI, intJ, intK);
                if (mc.intTest == 6)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstructorsTestClass54_base
        {

            public int intTest;

            protected ConstructorsTestClass54_base(params int[] values)
            {
                intTest = values[0] + values[1] + values[2];
            }
        }

        class ConstructorsTestClass54 : ConstructorsTestClass54_base
        {

            protected ConstructorsTestClass54(params int[] values) : base(values) { }

            public static bool testMethod()
            {

                int intI = 1;
                int intJ = 2;
                int intK = 3;

                ConstructorsTestClass54 mc = new ConstructorsTestClass54(intI, intJ, intK);

                if (mc.intTest == 6)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstructorsTestClass55_Base
        {

            public int intTest;

            protected internal ConstructorsTestClass55_Base(params int[] values)
            {
                intTest = values[0] + values[1] + values[2];
            }
        }

        class ConstructorsTestClass55 : ConstructorsTestClass55_Base
        {

            protected internal ConstructorsTestClass55(params int[] values) : base(values) { }

            public static bool testMethod()
            {

                int intI = 1;
                int intJ = 2;
                int intK = 3;

                ConstructorsTestClass55 mc = new ConstructorsTestClass55(intI, intJ, intK);

                if (mc.intTest == 6)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstructorsTestClass56_Sub
        {

            public int intTest;

            internal ConstructorsTestClass56_Sub(params int[] values)
            {
                intTest = values[0] + values[1] + values[2];
            }


        }

        class ConstructorsTestClass56
        {

            public static bool testMethod()
            {

                int intI = 1;
                int intJ = 2;
                int intK = 3;

                ConstructorsTestClass56_Sub mc = new ConstructorsTestClass56_Sub(intI, intJ, intK);
                if (mc.intTest == 6)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        class ConstructorsTestClass57
        {

            int intTest;

            private ConstructorsTestClass57(params int[] values)
            {
                intTest = values[0] + values[1] + values[2];
            }

            public static bool testMethod()
            {

                int intI = 1;
                int intJ = 2;
                int intK = 3;

                ConstructorsTestClass57 mc = new ConstructorsTestClass57(intI, intJ, intK);
                if (mc.intTest == 6)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        public class ConstructorsTestClass64_Base
        {
	        public ConstructorsTestClass64_Base() 
	        {
	        }
	        ~ConstructorsTestClass64_Base() 
	        {
	        }
        }

        class ConstructorsTestClass64_Derived : ConstructorsTestClass64_Base
        {
        }

        class ConstructorsTestClass64
        {

            public static bool testMethod()
            {
                ConstructorsTestClass64_Derived d = new ConstructorsTestClass64_Derived();

                MethodInfo mi = d.GetType().GetMethod("Finalize", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

                if (null != mi)
                {

                    return false;
                }

                return true;
            }
        }
    }
}
