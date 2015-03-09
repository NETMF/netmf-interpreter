////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#define while //Lexical_TestClass_pre_009
#define while //Lexical_TestClass_pre_009

#define True1 //Lexical_TestClass_pre_012
#define True2
#define False1
#undef False1
#undef False1
#undef False2

//Lexical_TestClass_pre_013
#define foo// Should work fine
#define bar // As should this
#define aaa
#define bbb
#undef	aaa// Should Work
#undef	bbb// Should also work
#if !foo
#error !foo
#endif
#if !bar
#error !bar
#endif
#if aaa
#error aaa
#endif
#if bbb
#error bbb
#endif

#define TESTDEF //Lexical_TestClass_preproc_03

#define TESTDEF3 //Lexical_TestClass_preproc_04
#define TESTDEF2
#undef TESTDEF3


#define FOO //Lexical_TestClass_preproc_05
#if FOO
#define BAR
#endif

#define FOO2 //Lexical_TestClass_preproc_06
#undef FOO2
#undef FOO2

#define FOO3 //Lexical_TestClass_preproc_07
#undef BAR3


#define TEST //Lexical_TestClass_preproc_15-25,32

#define TEST2 //Lexical_TestClass_preproc_17-23,32


#define for //Lexical_TestClass_preproc_39

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class LexicalTests2 : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
        }

        //Lexical Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\Lexical

        //Test Case Calls 
        [TestMethod]
        public MFTestResults Lexical_pre_009_Test()
        {
            Log.Comment("Section 2.3Preprocessing");
            Log.Comment("Verify #define and #undef");
            if (Lexical_TestClass_pre_009.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Lexical_pre_012_Test()
        {
            Log.Comment("Section 2.3Preprocessing");
            Log.Comment("Verify #if operators and parens");
            if (Lexical_TestClass_pre_012.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Lexical_pre_013_Test()
        {
            Log.Comment("Section 2.3Preprocessing");
            Log.Comment("Verify # commands with comments");
            if (Lexical_TestClass_pre_013.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Lexical_preproc_03_Test()
        {
            Log.Comment("Section 2.3 Preprocessing");
            Log.Comment("#define/#undef - verify #define works");
            if (Lexical_TestClass_preproc_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Lexical_preproc_04_Test()
        {
            Log.Comment("Section 2.3 Preprocessing");
            Log.Comment("#define/#undef - verify #undef works");
            if (Lexical_TestClass_preproc_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Lexical_preproc_05_Test()
        {
            Log.Comment("Section 2.3 Preprocessing");
            Log.Comment("Exact example used in spec definition - 2.3.1");
            if (Lexical_TestClass_preproc_05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Lexical_preproc_06_Test()
        {
            Log.Comment("Section 2.3 Preprocessing");
            Log.Comment("Using #undef on a non-existing identifier compiles fine");
            if (Lexical_TestClass_preproc_06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Lexical_preproc_07_Test()
        {
            Log.Comment("Section 2.3 Preprocessing");
            Log.Comment("Nested #if's");
            if (Lexical_TestClass_preproc_07.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Lexical_preproc_15_Test()
        {
            Log.Comment("Section 2.3 Preprocessing");
            Log.Comment("Verify the ! operator on #identifiers");
            if (Lexical_TestClass_preproc_15.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Lexical_preproc_16_Test()
        {
            Log.Comment("Section 2.3 Preprocessing");
            Log.Comment("Verify the ! operator on #identifiers with parenthesis");
            if (Lexical_TestClass_preproc_16.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Lexical_preproc_17_Test()
        {
            Log.Comment("Section 2.3 Preprocessing");
            Log.Comment("Verify the double ampersand operator works");
            if (Lexical_TestClass_preproc_17.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Lexical_preproc_18_Test()
        {
            Log.Comment("Section 2.3 Preprocessing");
            Log.Comment("Verify the double ampersand operator works with parentheses");
            if (Lexical_TestClass_preproc_18.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Lexical_preproc_19_Test()
        {
            Log.Comment("Section 2.3 Preprocessing");
            Log.Comment("Verify the || operator works ");
            if (Lexical_TestClass_preproc_19.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Lexical_preproc_20_Test()
        {
            Log.Comment("Section 2.3 Preprocessing");
            Log.Comment("Verify the || operator works with parentheses");
            if (Lexical_TestClass_preproc_20.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Lexical_preproc_21_Test()
        {
            Log.Comment("Section 2.3 Preprocessing");
            Log.Comment("Verify the == operator works with/without parentheses");
            if (Lexical_TestClass_preproc_21.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Lexical_preproc_22_Test()
        {
            Log.Comment("Section 2.3 Preprocessing");
            Log.Comment("Verify the != operator works with/without parentheses");
            if (Lexical_TestClass_preproc_22.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Lexical_preproc_23_Test()
        {
            Log.Comment("Section 2.3 Preprocessing");
            Log.Comment("Grouping operators: ! double ampersand || != == true false");
            if (Lexical_TestClass_preproc_23.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Lexical_preproc_24_Test()
        {
            Log.Comment("Section 2.3 Preprocessing");
            Log.Comment("Verifying comments and #preprocessor items");
            if (Lexical_TestClass_preproc_24.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Lexical_preproc_25_Test()
        {
            Log.Comment("Section 2.3 Preprocessing");
            Log.Comment("Verifying comments and #preprocessor items");
            if (Lexical_TestClass_preproc_25.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Lexical_preproc_31_Test()
        {
            Log.Comment("Section 2.3 Preprocessing");
            Log.Comment("Verifying comments and #preprocessor items");
            if (Lexical_TestClass_preproc_31.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Lexical_preproc_32_Test()
        {
            Log.Comment("Section 2.3 Preprocessing");
            Log.Comment("Verify the usage of #elif");
            if (Lexical_TestClass_preproc_32.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Lexical_preproc_39_Test()
        {
            Log.Comment("Section 2.3 Preprocessing");
            Log.Comment("Verify that a # keyword (i.e. for) can be used as a #preprocessor identifier");
            if (Lexical_TestClass_preproc_39.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }


        //Compiled Test Cases 

        public class Lexical_TestClass_pre_009
        {
            public static int Main_old(string[] args)
    {
		int i = 2;
#if while
		while (--i > 0)
			;
#endif
		return(i > 0 ? 1 : 0);
    }
            public static bool testMethod()
            {
                return (Main_old(null) == 0);
            }
        }

        public class Lexical_TestClass_pre_012
        {
            public static int Main_old(string[] args)
    {
		int i = 6;
 #if True1 == true
		i--;
 #endif
#if False1 == false
		i--;
#endif
#if false
	#error #elif True2 == True1
#elif True2 == True1
		i--;
#else
	#error #else #elif True2 == True1
#endif
#if (True1 != false) && ((False1) == False2) && (true || false)
		i--;
#else
	#error #if (True != false) && ((False1) == False2) && (true || false)
#endif
#if ((true == True1) != (false && true))
		i--;
#else
#error ((true == True1) != (false && true))
#endif
#if !(!(!!(true))) != false
		i--;
#else
#error !(!(!!(true))) != false
#endif
		return(i > 0 ? 1 : 0);
    }
            public static bool testMethod()
            {
                return (Main_old(null) == 0);
            }
        }

        public class Lexical_TestClass_pre_013
        {
            public static int Main_old(string[] args)
    {
		int i = 0;
		return(i > 0 ? 1 : 0);
    }
            public static bool testMethod()
            {
                return (Main_old(null) == 0);
            }
        }

        public class Lexical_TestClass_preproc_03
        {
            public static void Main_old(String[] args)
            {
                Log.Comment("Starting!");
#if TESTDEF
                Log.Comment("Good");
#else
			Log.Comment("Bad");
#endif
            }
            public static bool testMethod()
            {
                Main_old(null);
                return true;
            }
        }

        public class Lexical_TestClass_preproc_04
        {
            public static void Main_old(String[] args)
            {
                Log.Comment("Starting!");
#if TESTDEF3
			Log.Comment("TESTDEF3 is defined");
#else
                Log.Comment("TESTDEF3 is not defined");
#endif
#if TESTDEF2
                Log.Comment("TESTDEF2 is defined");
#else
			Log.Comment("TESTDEF2 not defined");
#endif
            }
            public static bool testMethod()
            {
                Main_old(null);
                return true;
            }
        }


#if BAR
        class Lexical_TestClass_preproc_05
        {
            public static void Main_old(String[] args) { }
            public static bool testMethod()
            {
                Main_old(null);
                return true;
            }
        }
#endif



        class Lexical_TestClass_preproc_06
        {
            public static void Main_old(String[] args) { }
            public static bool testMethod()
            {
                Main_old(null);
                return true;
            }
        }


        class Lexical_TestClass_preproc_07
        {
            public static void Main_old(String[] args)
            {
                Log.Comment("Starting:");
#if FOO3
                Log.Comment("Inside FOO");
#if BAR3
			Log.Comment("Inside BAR");
#else
                Log.Comment("Inside BAR's else");
#endif
#endif
            }
            public static bool testMethod()
            {
                Main_old(null);
                return true;
            }
        }
        class Lexical_TestClass_preproc_15
        {
            public static void Main_old(String[] args)
            {
#if ! TEST
			Log.Comment("Problem");
#else
                Log.Comment("Good");
#endif
            }
            public static bool testMethod()
            {
                Main_old(null);
                return true;
            }
        }

        class Lexical_TestClass_preproc_16
        {
            public static void Main_old(String[] args)
            {
#if !(TEST)
			Log.Comment("Problem");
#else
                Log.Comment("Good");
#endif
            }
            public static bool testMethod()
            {
                Main_old(null);
                return true;
            }
        }

        class Lexical_TestClass_preproc_17
        {
            public static void Main_old(String[] args)
            {
#if TEST && TEST2
                Log.Comment("Good");
#endif
#if TEST && TEST3
			Log.Comment("Problem");
#endif
            }
            public static bool testMethod()
            {
                Main_old(null);
                return true;
            }
        }
        class Lexical_TestClass_preproc_18
        {
            public static void Main_old(String[] args)
            {
#if (TEST && TEST2)
                Log.Comment("Good");
#endif
#if (TEST && TEST3)
			Log.Comment("Problem");
#endif
            }
            public static bool testMethod()
            {
                Main_old(null);
                return true;
            }
        }
        class Lexical_TestClass_preproc_19
        {
            public static void Main_old(String[] args)
            {
#if TEST || TEST2
                Log.Comment("Good");
#endif
#if TEST3 || TEST2
                Log.Comment("Good");
#endif
#if TEST3 || TEST4
			Log.Comment("Problem");
#endif
            }
            public static bool testMethod()
            {
                Main_old(null);
                return true;
            }
        }
        class Lexical_TestClass_preproc_20
        {
            public static void Main_old(String[] args)
            {
#if (TEST || TEST2)
                Log.Comment("Good");
#endif
#if (TEST3 || TEST2)
                Log.Comment("Good");
#endif
#if (TEST3 || TEST4)
			Log.Comment("Problem");
#endif
            }
            public static bool testMethod()
            {
                Main_old(null);
                return true;
            }
        }
        class Lexical_TestClass_preproc_21
        {
            public static void Main_old(String[] args)
            {
#if TEST == TEST2
                Log.Comment("Good");
#endif
#if (TEST == TEST2)
                Log.Comment("Good");
#endif
#if TEST==TEST2
                Log.Comment("Good");
#endif
#if (TEST == TEST3)
			Log.Comment("Bad");
#endif
#if TEST3 == TEST
			Log.Comment("Bad");
#endif
            }
            public static bool testMethod()
            {
                Main_old(null);
                return true;
            }
        }
        class Lexical_TestClass_preproc_22
        {
            public static void Main_old(String[] args)
            {
#if TEST != TEST2
			Log.Comment("Bad");
#endif
#if (TEST != TEST2)
			Log.Comment("Bad");
#endif
#if TEST!=TEST2
			Log.Comment("Bad");
#endif
#if (TEST != TEST3)
                Log.Comment("Good");
#endif
#if TEST3 != TEST
                Log.Comment("Good");
#endif
#if TEST3!=TEST
                Log.Comment("Good");
#endif
            }
            public static bool testMethod()
            {
                Main_old(null);
                return true;
            }
        }
        class Lexical_TestClass_preproc_23
        {
            public static void Main_old(String[] args)
            {
#if (TEST && TEST2) || (TEST3 || TEST4)
                Log.Comment("1 - Good");
#endif
#if (TEST3 == TEST4) || (TEST == TEST2)
                Log.Comment("2 - Good");
#endif
#if (TEST != TEST3) && (TEST2 != TEST4)
                Log.Comment("3 - Good");
#endif
#if (! TEST4) && (TEST2 == TEST)
                Log.Comment("4 - Good");
#endif
#if (TEST == true) && (TEST2 != false)
                Log.Comment("5 - Good");
#endif
            }
            public static bool testMethod()
            {
                Main_old(null);
                return true;
            }
        }
        class Lexical_TestClass_preproc_24
        {
            public static void Main_old(String[] args)
            {
#if TEST
                Log.Comment("Good");
#endif
            }
            public static bool testMethod()
            {
                Main_old(null);
                return true;
            }
        }
        class Lexical_TestClass_preproc_25
        {
            public static void Main_old(String[] args)
            {
#if TEST
                Log.Comment("Good");
#endif
            }
            public static bool testMethod()
            {
                Main_old(null);
                return true;
            }
        }
        class Lexical_TestClass_preproc_31
        {
            public static void Main_old(String[] args)
            {
#if TEST
                Log.Comment("Bad");
#else
			Log.Comment("Good");
#endif
            }
            public static bool testMethod()
            {
                Main_old(null);
                return true;
            }
        }
        class Lexical_TestClass_preproc_32
        {
            public static void Main_old(String[] args)
            {
#if TEST3
			Log.Comment("Bad");
#elif TEST2 && TEST
                Log.Comment("Good");
#endif
            }
            public static bool testMethod()
            {
                Main_old(null);
                return true;
            }
        }
        class Lexical_TestClass_preproc_39
        {
            public static void Main_old(String[] args)
            {
#if for
                for (int x = 0; x < 3; x++)
                    Log.Comment("Worked");
#else
			Log.Comment("It should not be showing this!");
#endif
            }
            public static bool testMethod()
            {
                Main_old(null);
                return true;
            }
        }

    }
}
