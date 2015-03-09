////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class SimpleTests : IMFTestInterface
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

        //Simple Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\Simple

        //decl_decl_01,decl_decl_02,decl_decl_03,decl_decl_04,decl_decl_05,decl_decl_06,decl_decl_07,decl_decl_08,decl_decl_09,decl_decl_10,decl_decl_11,decl_decl_12,decl_decl_13,decl_decl_14,decl_decl_15,decl_bounds_01,decl_bounds_02,decl_bounds_03,decl_index_01,decl_index_02,decl_index_03,decl_index_04,decl_index_05,decl_index_06,decl_index_07,decl_index_08,decl_index_09,decl_index_10,decl_index_11,decl_index_12,decl_syntax_03,init_init_a_01,init_init_a_02,init_init_a_03,init_init_a_04,init_init_a_05,ini



/*
JHS
t_init_a_06,init_init_a_08,init_init_a_09,init_init_a_10,init_init_b_01,init_init_b_02,init_init_b_03,init_init_b_04,init_init_b_05,init_init_b_06,init_init_b_08,init_init_b_09,init_init_b_10,init_init_c_01,init_init_c_02,init_init_c_03,init_init_c_04,init_init_c_05,init_init_c_06,init_init_c_08,init_init_c_09,init_init_c_10,init_init_d_01,init_init_d_02,init_init_d_03,init_init_d_04,init_init_d_05,init_init_d_06,init_init_d_08,init_init_d_09,init_init_d_10,init_init_e_01,init_init_e_02,init_init_e_03,init
_init_e_04,init_init_e_05,init_init_e_06,init_init_e_08,init_init_e_09,init_init_e_10,init_syntax_09,init_syntax_11,init_syntax_12,init_decl_02,init_decl_03,init_decl_04,init_decl_05,init_shorthand_01,init_constinit_01,init_constinit_02,init_constinit_03,acc_iter_length_01,acc_iter_bounds_01,acc_iter_bounds_02,acc_iter_bounds_03,acc_iter_bounds_04,acc_iter_idxtype_a_01,acc_iter_idxtype_a_02,acc_iter_idxtype_a_03,acc_iter_idxtype_a_04,acc_iter_idxtype_a_06,acc_iter_idxtype_b_01,acc_iter_idxtype_b_02,acc_ite
r_idxtype_b_03,acc_iter_idxtype_b_04,acc_iter_idxtype_b_06,acc_iter_iter_01,acc_iter_iter_02,assign_smpass_a_01,assign_smpass_a_02,assign_smpass_a_03,assign_smpass_a_04,assign_smpass_a_05,assign_smpass_a_06,assign_smpass_a_07,assign_smpass_a_08,assign_smpass_a_09,assign_smpass_b_01,assign_smpass_b_02,assign_smpass_b_03,assign_smpass_b_04,assign_smpass_b_05,assign_smpass_b_06,assign_smpass_b_07,assign_smpass_b_08,assign_smpass_b_09,assign_badass_01,assign_badass_03,assign_badass_04,assign_element_01,assign_
element_02,assign_argpass_01,assign_argpass_02,assign_argpass_03,object_sysarr_01,object_sysarr_02,object_sysarr_03,nonzerolb_decl_01,nonzerolb_decl_02,nonzerolb_error_04,nonzerolb_usage_01,nonzerolb_usage_02,nonzerolb_usage_03,nonzerolb_usage_04.cs) -- skipped
*/
        //Test Case Calls 
        [TestMethod]
        public MFTestResults Simple_decl_decl_01_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" Declare a simple array of type int");

            if (Simple_TestClass_decl_decl_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_decl_02_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" Declare a simple array of type byte");
            if (Simple_TestClass_decl_decl_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_decl_03_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" Declare a simple array of type short");

            if (Simple_TestClass_decl_decl_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_decl_04_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" Declare a simple array of type long");

            if (Simple_TestClass_decl_decl_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_decl_05_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" Declare a simple array of type char");

            if (Simple_TestClass_decl_decl_05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_decl_06_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" Declare a simple array of type double");

            if (Simple_TestClass_decl_decl_06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_decl_07_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" Declare a simple array of type float");

            if (Simple_TestClass_decl_decl_07.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_decl_08_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" Declare a simple array of type double");

            if (Simple_TestClass_decl_decl_08.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_decl_09_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" Declare a simple array of type bool");

            if (Simple_TestClass_decl_decl_09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_decl_10_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" Declare a simple array of a user-defined struct");

            if (Simple_TestClass_decl_decl_10.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_decl_11_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" Declare a simple array of a user-defined class");

            if (Simple_TestClass_decl_decl_11.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_decl_12_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" Declare a simple array of a user-defined interface");

            if (Simple_TestClass_decl_decl_12.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_decl_13_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" Declare a simple array of type object");

            if (Simple_TestClass_decl_decl_13.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_decl_14_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" Declare a simple array of delegates");

            if (Simple_TestClass_decl_decl_14.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_decl_15_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" Declare a simple array of type System.Array");

            if (Simple_TestClass_decl_decl_15.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_bounds_01_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" An array created as array[0] compiles successfully");

            if (Simple_TestClass_decl_bounds_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_bounds_02_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" An array created as array[maxint] compiles successfully");
            Log.Comment("This test is expected to generate");
            Log.Comment("Out Of Memory System.Exception");
            if (Simple_TestClass_decl_bounds_02.testMethod())
            {
                Log.Comment(" This failure indicates a test is now passing that previously failed by design.");
                Log.Comment(" It previously marked as known failure because of bug # 16823");
                Log.Comment(" The Test owner needs to verify that the change was intentional and remove the known failure.");
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;
        }
        [TestMethod]
        public MFTestResults Simple_decl_bounds_03_Test()
        {
            Log.Comment(" decl_bounds_03 ");
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" An array created as array[maxint+1] compiles successfully");
            if (Simple_TestClass_decl_bounds_03.testMethod())
            {
                Log.Comment(" This failure indicates a test is now passing that previously failed by design.");
                Log.Comment(" It previously marked as known failure because of bug # 16823");
                Log.Comment(" The Test owner needs to verify that the change was intentional and remove the known failure.");
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;
        }
        [TestMethod]
        public MFTestResults Simple_decl_index_01_Test()
        {
            Log.Comment(" decl_index_01 ");
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" A char variable as an array index should work");

            if (Simple_TestClass_decl_index_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_index_02_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" A byte variable as an array index should work");
            if (Simple_TestClass_decl_index_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_index_03_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" A short variable as an array index should work");
            if (Simple_TestClass_decl_index_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_index_04_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" A short literal as an array index should work");

            if (Simple_TestClass_decl_index_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_index_05_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" A byte literal as an array index should work");
            if (Simple_TestClass_decl_index_05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_index_06_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" A char literal as an array index should work");
            if (Simple_TestClass_decl_index_06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_index_07_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" A uint variable as an array index should work");
            if (Simple_TestClass_decl_index_07.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_index_08_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" A long variable as an array index should work");
            if (Simple_TestClass_decl_index_08.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_index_09_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" A ulong variable as an array index should work");
            if (Simple_TestClass_decl_index_09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_index_10_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" A uint literal as an array index should work");
            if (Simple_TestClass_decl_index_10.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_index_11_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" A long literal as an array index should work");
            if (Simple_TestClass_decl_index_11.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_index_12_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" A ulong literal as an array index should work");
            if (Simple_TestClass_decl_index_12.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_decl_syntax_03_Test()
        {
            Log.Comment(" Arrays - Declarations");
            Log.Comment(" Spaces after type and between brackets and comments do not affect arrays");
            if (Simple_TestClass_decl_syntax_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_a_01_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type int using new (longhand)");
            if (Simple_TestClass_init_init_a_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_a_02_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type short using new (longhand)");
            if (Simple_TestClass_init_init_a_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_a_03_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type byte using new (longhand)");
            if (Simple_TestClass_init_init_a_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_a_04_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type long using new (longhand)");
            if (Simple_TestClass_init_init_a_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_a_05_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type double using new (longhand)");
            if (Simple_TestClass_init_init_a_05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_a_06_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type float using new (longhand)");

            if (Simple_TestClass_init_init_a_06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_a_08_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type String using new (longhand)");
            if (Simple_TestClass_init_init_a_08.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_a_09_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type char using new (longhand)");
            if (Simple_TestClass_init_init_a_09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_a_10_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type bool using new (longhand)");
            if (Simple_TestClass_init_init_a_10.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_b_01_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type int using new (shorthand)");
            if (Simple_TestClass_init_init_b_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_b_02_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type short using new (shorthand)");
            if (Simple_TestClass_init_init_b_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_b_03_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type byte using new (shorthand)");
            if (Simple_TestClass_init_init_b_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_b_04_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type long using new (shorthand)");
            if (Simple_TestClass_init_init_b_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_b_05_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type double using new (shorthand)");
            if (Simple_TestClass_init_init_b_05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_b_06_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type float using new (shorthand)");
            if (Simple_TestClass_init_init_b_06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_b_08_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type String using new (shorthand)");
            if (Simple_TestClass_init_init_b_08.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_b_09_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type char using new (shorthand)");
            if (Simple_TestClass_init_init_b_09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_b_10_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type bool using new (shorthand)");

            if (Simple_TestClass_init_init_b_10.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_c_01_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type int using new (# init values sets array size)");
            if (Simple_TestClass_init_init_c_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_c_02_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type short using new (# init values sets array size)");
            if (Simple_TestClass_init_init_c_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_c_03_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type byte using new (# init values sets array size)");
            if (Simple_TestClass_init_init_c_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_c_04_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type long using new (# init values sets array size)");
            if (Simple_TestClass_init_init_c_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_c_05_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type double using new (# init values sets array size)");
            if (Simple_TestClass_init_init_c_05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_c_06_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type float using new (# init values sets array size)");
            if (Simple_TestClass_init_init_c_06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_c_08_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type String using new (# init values sets array size)");
            if (Simple_TestClass_init_init_c_08.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_c_09_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type char using new (# init values sets array size)");
            if (Simple_TestClass_init_init_c_09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_c_10_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type bool using new (# init values sets array size)");
            if (Simple_TestClass_init_init_c_10.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_d_01_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type int using new (longhand) separate from decl statement");
            if (Simple_TestClass_init_init_d_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_d_02_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type short using new (longhand) separate from decl statement");
            if (Simple_TestClass_init_init_d_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_d_03_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type byte using new (longhand) separate from decl statement");
            if (Simple_TestClass_init_init_d_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_d_04_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type long using new (longhand) separate from decl statement");
            if (Simple_TestClass_init_init_d_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_d_05_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type double using new (longhand) separate from decl statement");
            if (Simple_TestClass_init_init_d_05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_d_06_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type float using new (longhand) separate from decl statement");
            if (Simple_TestClass_init_init_d_06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_d_08_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type String using new (longhand) separate from decl statement");
            if (Simple_TestClass_init_init_d_08.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_d_09_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type char using new (longhand) separate from decl statement");
            if (Simple_TestClass_init_init_d_09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_d_10_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type bool using new (longhand) separate from decl statement");
            if (Simple_TestClass_init_init_d_10.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_e_01_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type int using new (# init values sets array size) separate from decl statement");
            if (Simple_TestClass_init_init_e_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_e_02_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type short using new (# init values sets array size) separate from decl statement");
            if (Simple_TestClass_init_init_e_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_e_03_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type byte using new (# init values sets array size) separate from decl statement");
            if (Simple_TestClass_init_init_e_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_e_04_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type long using new (# init values sets array size) separate from decl statement");
            if (Simple_TestClass_init_init_e_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_e_05_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type double using new (# init values sets array size) separate from decl statement");
            if (Simple_TestClass_init_init_e_05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_e_06_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type float using new (# init values sets array size) separate from decl statement");
            if (Simple_TestClass_init_init_e_06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_e_08_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type String using new (# init values sets array size) separate from decl statement");
            if (Simple_TestClass_init_init_e_08.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_e_09_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type char using new (# init values sets array size) separate from decl statement");
            if (Simple_TestClass_init_init_e_09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_init_e_10_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type bool using new (# init values sets array size) separate from decl statement");
            if (Simple_TestClass_init_init_e_10.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_syntax_09_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Syntax - Whitespace and comments in braces should not affect anything");
            if (Simple_TestClass_init_syntax_09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_syntax_11_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Syntax - zero elements in initializer should create zero length array");
            if (Simple_TestClass_init_syntax_11.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_syntax_12_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Syntax - zero elements in initializer should be allowed for 0-length array");
            if (Simple_TestClass_init_syntax_12.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_decl_02_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Declaration - if initializer is used, a provided index can be a const");
            if (Simple_TestClass_init_decl_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_decl_03_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Declaration - if initializer is used, a provided index can be a const short");
            if (Simple_TestClass_init_decl_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_decl_04_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Declaration - if initializer is used, a provided index can be a const byte");
            if (Simple_TestClass_init_decl_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_decl_05_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Declaration - if initializer is used, a provided index can be a const long casted to an int");
            if (Simple_TestClass_init_decl_05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_shorthand_01_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Initialization of arrays of type int using new (shorthand)");
            Log.Comment(" This is to verify bug #52958 doesn't regress");
            if (Simple_TestClass_init_shorthand_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_constinit_01_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Ensure all constant init optimization code paths are covered: all constants (VS7:124565 for more info)");
            if (Simple_TestClass_init_constinit_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_constinit_02_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Ensure all constant init optimization code paths are covered: all variables (VS7:124565 for more info)");

            if (Simple_TestClass_init_constinit_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_init_constinit_03_Test()
        {
            Log.Comment(" Arrays - Initialization");
            Log.Comment(" Ensure all constant init optimization code paths are covered: half variables and half constants (VS7:124565 for more info)");
            if (Simple_TestClass_init_constinit_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_acc_iter_length_01_Test()
        {
            Log.Comment(" acc_iter_length_01 ");
            Log.Comment(" Arrays - Access and Iteration");
            Log.Comment(" Declare simple arrays of various lengths and verify that array.Length is correct");
            if (Simple_TestClass_acc_iter_length_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_acc_iter_bounds_01_Test()
        {
            Log.Comment(" Arrays - Access and Iteration");
            Log.Comment(" Accessing an array's 0th element should work fine");
            if (Simple_TestClass_acc_iter_bounds_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_acc_iter_bounds_02_Test()
        {
            Log.Comment(" Arrays - Access and Iteration");
            Log.Comment(" Accessing an array's maxlength element should work fine");
            if (Simple_TestClass_acc_iter_bounds_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_acc_iter_bounds_03_Test()
        {
            Log.Comment(" Arrays - Access and Iteration");
            Log.Comment(" Accessing an array's -1 element should throw an System.Exception");
            if (Simple_TestClass_acc_iter_bounds_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_acc_iter_bounds_04_Test()
        {
            Log.Comment(" Arrays - Access and Iteration");
            Log.Comment(" Accessing an array's maxlength+1 element should throw an System.Exception");
            if (Simple_TestClass_acc_iter_bounds_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_acc_iter_idxtype_a_01_Test()
        {
            Log.Comment(" Arrays - Access and Iteration");
            Log.Comment(" Accessing an array's index with a variable of type int should work");
            if (Simple_TestClass_acc_iter_idxtype_a_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_acc_iter_idxtype_a_02_Test()
        {
            Log.Comment(" Arrays - Access and Iteration");
            Log.Comment(" Accessing an array's index with a variable of type short should work");
            if (Simple_TestClass_acc_iter_idxtype_a_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_acc_iter_idxtype_a_03_Test()
        {
            Log.Comment(" Arrays - Access and Iteration");
            Log.Comment(" Accessing an array's index with a variable of type byte should work");

            if (Simple_TestClass_acc_iter_idxtype_a_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_acc_iter_idxtype_a_04_Test()
        {
            Log.Comment(" Arrays - Access and Iteration");
            Log.Comment(" Accessing an array's index with a variable of type char should work");
            if (Simple_TestClass_acc_iter_idxtype_a_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_acc_iter_idxtype_a_06_Test()
        {
            Log.Comment(" Arrays - Access and Iteration");
            Log.Comment(" Accessing an array's index with a variable of type long should work");
            if (Simple_TestClass_acc_iter_idxtype_a_06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_acc_iter_idxtype_b_01_Test()
        {
            Log.Comment(" Arrays - Access and Iteration");
            Log.Comment(" Accessing an array's index with an int literal should work");

            if (Simple_TestClass_acc_iter_idxtype_b_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_acc_iter_idxtype_b_02_Test()
        {
            Log.Comment(" Arrays - Access and Iteration");
            Log.Comment(" Accessing an array's index with a short literal should work");
            if (Simple_TestClass_acc_iter_idxtype_b_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_acc_iter_idxtype_b_03_Test()
        {
            Log.Comment(" Arrays - Access and Iteration");
            Log.Comment(" Accessing an array's index with a byte literal should work");
            if (Simple_TestClass_acc_iter_idxtype_b_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_acc_iter_idxtype_b_04_Test()
        {
            Log.Comment(" Arrays - Access and Iteration");
            Log.Comment(" Accessing an array's index with a char literal should work");
            if (Simple_TestClass_acc_iter_idxtype_b_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_acc_iter_idxtype_b_06_Test()
        {
            Log.Comment(" Arrays - Access and Iteration");
            Log.Comment(" Accessing an array's index with a variable of type long should work");
            if (Simple_TestClass_acc_iter_idxtype_b_06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_acc_iter_iter_01_Test()
        {
            Log.Comment(" Arrays - Access and Iteration");
            Log.Comment(" Declare simple array of int, init it through iteration, verify values are correct");
            if (Simple_TestClass_acc_iter_iter_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_acc_iter_iter_02_Test()
        {
            Log.Comment(" Arrays - Access and Iteration");
            Log.Comment(" Declare simple array of char, init it through individual element assignments, verify correctness");
            if (Simple_TestClass_acc_iter_iter_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_smpass_a_01_Test()
        {
            Log.Comment(" Arrays - Assignments");
            Log.Comment(" Assigning int type literals into array elements should work");
            if (Simple_TestClass_assign_smpass_a_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_smpass_a_02_Test()
        {
            Log.Comment(" Arrays - Assignments");
            Log.Comment(" Assigning byte type literals into array elements should work");
            if (Simple_TestClass_assign_smpass_a_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_smpass_a_03_Test()
        {
            Log.Comment(" Arrays - Assignments");
            Log.Comment(" Assigning short type literals into array elements should work");
            if (Simple_TestClass_assign_smpass_a_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_smpass_a_04_Test()
        {
            Log.Comment(" Arrays - Assignments");
            Log.Comment(" Assigning long type literals into array elements should work");
            if (Simple_TestClass_assign_smpass_a_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_smpass_a_05_Test()
        {
            Log.Comment(" Arrays - Assignments");
            Log.Comment(" Assigning char type literals into array elements should work");
            if (Simple_TestClass_assign_smpass_a_05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_smpass_a_06_Test()
        {
            Log.Comment(" Arrays - Assignments");
            Log.Comment(" Assigning double type literals into array elements should work");
            if (Simple_TestClass_assign_smpass_a_06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_smpass_a_07_Test()
        {
            Log.Comment(" Arrays - Assignments");
            Log.Comment(" Assigning float type literals into array elements should work");
            if (Simple_TestClass_assign_smpass_a_07.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_smpass_a_08_Test()
        {
            Log.Comment(" Arrays - Assignments");
            Log.Comment(" Assigning double type literals into array elements should work");
            if (Simple_TestClass_assign_smpass_a_08.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_smpass_a_09_Test()
        {
            Log.Comment(" Arrays - Assignments");
            Log.Comment(" Assigning bool type literals into array elements should work");
            if (Simple_TestClass_assign_smpass_a_09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_smpass_b_01_Test()
        {
            Log.Comment(" Arrays - Assignments");
            Log.Comment(" Assigning int type variables into array elements should work");
            if (Simple_TestClass_assign_smpass_b_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_smpass_b_02_Test()
        {
            Log.Comment(" Arrays - Assignments");
            Log.Comment(" Assigning byte type variables into array elements should work");
            if (Simple_TestClass_assign_smpass_b_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_smpass_b_03_Test()
        {
            Log.Comment(" Arrays - Assignments");
            Log.Comment(" Assigning short type variables into array elements should work");
            if (Simple_TestClass_assign_smpass_b_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_smpass_b_04_Test()
        {
            Log.Comment(" Arrays - Assignments");
            Log.Comment(" Assigning long type variables into array elements should work");
            if (Simple_TestClass_assign_smpass_b_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_smpass_b_05_Test()
        {
            Log.Comment(" Arrays - Assignments");
            Log.Comment(" Assigning char type variables into array elements should work");
            if (Simple_TestClass_assign_smpass_b_05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_smpass_b_06_Test()
        {
            Log.Comment(" Arrays - Assignments");
            Log.Comment(" Assigning double type variables into array elements should work");
            if (Simple_TestClass_assign_smpass_b_06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_smpass_b_07_Test()
        {
            Log.Comment(" Arrays - Assignments");
            Log.Comment(" Assigning float type variables into array elements should work");

            if (Simple_TestClass_assign_smpass_b_07.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_smpass_b_08_Test()
        {
            Log.Comment(" Arrays - Assignments");
            Log.Comment(" Assigning double type variables into array elements should work");

            if (Simple_TestClass_assign_smpass_b_08.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_smpass_b_09_Test()
        {
            Log.Comment(" Arrays - Assignments");
            Log.Comment(" Assigning bool type variables into array elements should work");
            if (Simple_TestClass_assign_smpass_b_09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_badass_01_Test()
        {
            Log.Comment(" Arrays - Assignments");
            Log.Comment(" Assigning null to an array variable should work");
            if (Simple_TestClass_assign_badass_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_badass_03_Test()
        {
            Log.Comment(" Arrays - Assignments");
            Log.Comment(" Assigning a smaller array to a bigger array should work");
            if (Simple_TestClass_assign_badass_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_badass_04_Test()
        {
            Log.Comment(" Arrays - Assignments");
            Log.Comment(" Assigning a bigger array to a smaller array should work");
            if (Simple_TestClass_assign_badass_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_element_01_Test()
        {
            Log.Comment(" Arrays - Assignments");
            Log.Comment(" Assigning one element to another element of the same array should work");
            if (Simple_TestClass_assign_element_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_element_02_Test()
        {
            Log.Comment(" Arrays - Assignments");
            Log.Comment(" Assigning one element to another element of a different array should work");
            if (Simple_TestClass_assign_element_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_argpass_01_Test()
        {
            Log.Comment(" Arrays - Assignments - Passing elements to methods");
            Log.Comment(" Passing an element to a function should work");
            if (Simple_TestClass_assign_argpass_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_argpass_02_Test()
        {
            Log.Comment(" Arrays - Assignments - Passing elements to methods");
            Log.Comment(" Passing an element to a function as a ref parameter should work");
            if (Simple_TestClass_assign_argpass_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_assign_argpass_03_Test()
        {
            Log.Comment(" Arrays - Assignments - Passing elements to methods");
            Log.Comment(" Passing an element to a function as an out parameter should work");
            if (Simple_TestClass_assign_argpass_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_object_sysarr_01_Test()
        {
            Log.Comment(" Arrays - As Object and System.Array");
            Log.Comment(" Testing the System.Array methods and Properties: System.Array.Clear()");
            if (Simple_TestClass_object_sysarr_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Simple_object_sysarr_02_Test()
        {
            Log.Comment(" Arrays - As Object and System.Array");
            Log.Comment(" Testing the System.Array methods and Properties: Length property");
            if (Simple_TestClass_object_sysarr_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        //Compiled Test Cases 
        class Simple_TestClass_decl_decl_01
        {
            public static int Main_old()
            {
                int[] arr = new int[10];
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_decl_02
        {
            public static int Main_old()
            {
                byte[] arr = new byte[10];
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_decl_03
        {
            public static int Main_old()
            {
                short[] arr = new short[10];
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_decl_04
        {
            public static int Main_old()
            {
                long[] arr = new long[10];
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_decl_05
        {
            public static int Main_old()
            {
                char[] arr = new char[10];
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_decl_06
        {
            public static int Main_old()
            {
                double[] arr = new double[10];
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_decl_07
        {
            public static int Main_old()
            {
                float[] arr = new float[10];
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_decl_08
        {
            public static int Main_old()
            {
                double[] arr = new double[10];
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_decl_09
        {
            public static int Main_old()
            {
                bool[] arr = new bool[10];
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        struct myStruct
        {
            public int x ;
        }
        class Simple_TestClass_decl_decl_10
        {
            public static int Main_old()
            {
                myStruct[] arr = new myStruct[10];

                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class myClass
        {
            public int x = 0;
            void meth() { }
        }
        class Simple_TestClass_decl_decl_11
        {
            public static int Main_old()
            {
                myClass[] arr = new myClass[10];

                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        interface myInterface { }
        class Simple_TestClass_decl_decl_12
        {
            public static int Main_old()
            {
                myInterface[] arr = new myInterface[10];

                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_decl_13
        {
            public static int Main_old()
            {
                object[] arr = new object[10];

                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate void myDelegate(String myString);
        class Simple_TestClass_decl_decl_14
        {
            public static int Main_old()
            {
                myDelegate[] myDel = new myDelegate[10];
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_decl_15
        {
            public static int Main_old()
            {
                Array[] arr = new Array[10];
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_bounds_01
        {
            public static int Main_old()
            {
                int[] arr = new int[0];
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_bounds_02
        {
            public static int Main_old()
            {
                try
                {//bug 16823
                    //int[] arr = new int[2147483647];
                }
                catch (System.Exception)
                {
                    Log.Comment("Out Of Memory System.Exception");
                }
                return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_bounds_03
        {
            public static int Main_old()
            {
                try
                {//bug 16823
                    //int[] arr = new int[2147483648];
                }
                catch (System.Exception)
                {
                    return 0;
                }
                return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_index_01
        {
            public static int Main_old()
            {
                char x = 'a';
                int[] arr1 = new int[x];

                char y = 'b';
                int[] arr2 = new int[y];

                if (arr1.Length == (arr2.Length - 1))
                    return 0;
                else
                    return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_index_02
        {
            public static int Main_old()
            {
                byte x = 5;
                int[] arr1 = new int[x];
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_index_03
        {
            public static int Main_old()
            {
                short x = 5;
                int[] arr1 = new int[x];
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_index_04
        {
            public static int Main_old()
            {
                int[] arr1 = new int[(short)5];
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_index_05
        {
            public static int Main_old()
            {
                int[] arr1 = new int[(byte)5];
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_index_06
        {
            public static int Main_old()
            {
                int[] arr1 = new int['a'];
                int[] arr2 = new int['b'];

                Log.Comment(arr1.Length.ToString());
                Log.Comment(arr2.Length.ToString());
                if (arr1.Length == (arr2.Length - 1))
                    return 0;
                else
                    return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_index_07
        {
            public static int Main_old()
            {
                uint x = 5;
                int[] arr1 = new int[x];
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_index_08
        {
            public static int Main_old()
            {
                long x = 5;
                int[] arr1 = new int[x];
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_index_09
        {
            public static int Main_old()
            {
                ulong x = 5;
                int[] arr1 = new int[x];
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_index_10
        {
            public static int Main_old()
            {
                int[] arr1 = new int[5U];
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_index_11
        {
            public static int Main_old()
            {
                int[] arr1 = new int[5L];
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_index_12
        {
            public static int Main_old()
            {
                int[] arr1 = new int[5UL];
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_decl_syntax_03
        {
            public static int Main_old()
            {
                int[] arr = new int[10];
                int[ /* test comment */ ] arr2 = new int[5];
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_a_01
        {
            public static int Main_old()
            {
                int[] arr = new int[5] { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_a_02
        {
            public static int Main_old()
            {
                short[] arr = new short[5] { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_a_03
        {
            public static int Main_old()
            {
                byte[] arr = new byte[5] { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_a_04
        {
            public static int Main_old()
            {
                long[] arr = new long[5] { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_a_05
        {
            public static int Main_old()
            {
                double[] arr = new double[5] { 1.0, 2.0, 3.0, 4.0, 5.0 };
                int x = 0;
                if (arr[0] != 1.0)
                    x = 1;
                if (arr[1] != 2.0)
                    x = 1;
                if (arr[2] != 3.0)
                    x = 1;
                if (arr[3] != 4.0)
                    x = 1;
                if (arr[4] != 5.0)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_a_06
        {
            public static int Main_old()
            {
                float[] arr = new float[5] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };
                int x = 0;
                if (arr[0] != 1.0)
                    x = 1;
                if (arr[1] != 2.0)
                    x = 1;
                if (arr[2] != 3.0)
                    x = 1;
                if (arr[3] != 4.0)
                    x = 1;
                if (arr[4] != 5.0)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_a_08
        {
            public static int Main_old()
            {
                String[] arr = new String[5] { "one", "two", "three", "four", "five" };
                int x = 0;
                if (!arr[0].Equals("one"))
                    x = 1;
                if (!arr[1].Equals("two"))
                    x = 1;
                if (!arr[2].Equals("three"))
                    x = 1;
                if (!arr[3].Equals("four"))
                    x = 1;
                if (!arr[4].Equals("five"))
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_a_09
        {
            public static int Main_old()
            {
                char[] arr = new char[5] { '1', '2', '3', '4', '5' };
                int x = 0;
                if (arr[0] != '1')
                    x = 1;
                if (arr[1] != '2')
                    x = 1;
                if (arr[2] != '3')
                    x = 1;
                if (arr[3] != '4')
                    x = 1;
                if (arr[4] != '5')
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_a_10
        {
            public static int Main_old()
            {
                bool[] arr = new bool[5] { true, false, true, false, true };
                int x = 0;
                if (!arr[0])
                    x = 1;
                if (arr[1])
                    x = 1;
                if (!arr[2])
                    x = 1;
                if (arr[3])
                    x = 1;
                if (!arr[4])
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_b_01
        {
            public static int Main_old()
            {
                int[] arr = { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_b_02
        {
            public static int Main_old()
            {
                short[] arr = { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_b_03
        {
            public static int Main_old()
            {
                byte[] arr = { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_b_04
        {
            public static int Main_old()
            {
                long[] arr = { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_b_05
        {
            public static int Main_old()
            {
                double[] arr = { 1.0, 2.0, 3.0, 4.0, 5.0 };
                int x = 0;
                if (arr[0] != 1.0)
                    x = 1;
                if (arr[1] != 2.0)
                    x = 1;
                if (arr[2] != 3.0)
                    x = 1;
                if (arr[3] != 4.0)
                    x = 1;
                if (arr[4] != 5.0)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_b_06
        {
            public static int Main_old()
            {
                float[] arr = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };
                int x = 0;
                if (arr[0] != 1.0)
                    x = 1;
                if (arr[1] != 2.0)
                    x = 1;
                if (arr[2] != 3.0)
                    x = 1;
                if (arr[3] != 4.0)
                    x = 1;
                if (arr[4] != 5.0)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_b_08
        {
            public static int Main_old()
            {
                String[] arr = { "one", "two", "three", "four", "five" };
                int x = 0;
                if (!arr[0].Equals("one"))
                    x = 1;
                if (!arr[1].Equals("two"))
                    x = 1;
                if (!arr[2].Equals("three"))
                    x = 1;
                if (!arr[3].Equals("four"))
                    x = 1;
                if (!arr[4].Equals("five"))
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_b_09
        {
            public static int Main_old()
            {
                char[] arr = { '1', '2', '3', '4', '5' };
                int x = 0;
                if (arr[0] != '1')
                    x = 1;
                if (arr[1] != '2')
                    x = 1;
                if (arr[2] != '3')
                    x = 1;
                if (arr[3] != '4')
                    x = 1;
                if (arr[4] != '5')
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_b_10
        {
            public static int Main_old()
            {
                bool[] arr = { true, false, true, false, true };
                int x = 0;
                if (!arr[0])
                    x = 1;
                if (arr[1])
                    x = 1;
                if (!arr[2])
                    x = 1;
                if (arr[3])
                    x = 1;
                if (!arr[4])
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_c_01
        {
            public static int Main_old()
            {
                int[] arr = new int[] { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                if (arr.Length != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_c_02
        {
            public static int Main_old()
            {
                short[] arr = new short[] { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                if (arr.Length != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_c_03
        {
            public static int Main_old()
            {
                byte[] arr = new byte[] { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                if (arr.Length != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_c_04
        {
            public static int Main_old()
            {
                long[] arr = new long[] { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                if (arr.Length != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_c_05
        {
            public static int Main_old()
            {
                double[] arr = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
                int x = 0;
                if (arr[0] != 1.0)
                    x = 1;
                if (arr[1] != 2.0)
                    x = 1;
                if (arr[2] != 3.0)
                    x = 1;
                if (arr[3] != 4.0)
                    x = 1;
                if (arr[4] != 5.0)
                    x = 1;
                if (arr.Length != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_c_06
        {
            public static int Main_old()
            {
                float[] arr = new float[] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };
                int x = 0;
                if (arr[0] != 1.0)
                    x = 1;
                if (arr[1] != 2.0)
                    x = 1;
                if (arr[2] != 3.0)
                    x = 1;
                if (arr[3] != 4.0)
                    x = 1;
                if (arr[4] != 5.0)
                    x = 1;
                if (arr.Length != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_c_08
        {
            public static int Main_old()
            {
                String[] arr = new String[] { "one", "two", "three", "four", "five" };
                int x = 0;
                if (!arr[0].Equals("one"))
                    x = 1;
                if (!arr[1].Equals("two"))
                    x = 1;
                if (!arr[2].Equals("three"))
                    x = 1;
                if (!arr[3].Equals("four"))
                    x = 1;
                if (!arr[4].Equals("five"))
                    x = 1;
                if (arr.Length != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_c_09
        {
            public static int Main_old()
            {
                char[] arr = new char[] { '1', '2', '3', '4', '5' };
                int x = 0;
                if (arr[0] != '1')
                    x = 1;
                if (arr[1] != '2')
                    x = 1;
                if (arr[2] != '3')
                    x = 1;
                if (arr[3] != '4')
                    x = 1;
                if (arr[4] != '5')
                    x = 1;
                if (arr.Length != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_c_10
        {
            public static int Main_old()
            {
                bool[] arr = new bool[] { true, false, true, false, true };
                int x = 0;
                if (!arr[0])
                    x = 1;
                if (arr[1])
                    x = 1;
                if (!arr[2])
                    x = 1;
                if (arr[3])
                    x = 1;
                if (!arr[4])
                    x = 1;
                if (arr.Length != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_d_01
        {
            public static int Main_old()
            {
                int[] arr;
                arr = new int[5] { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_d_02
        {
            public static int Main_old()
            {
                short[] arr;
                arr = new short[5] { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_d_03
        {
            public static int Main_old()
            {
                byte[] arr;
                arr = new byte[5] { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_d_04
        {
            public static int Main_old()
            {
                long[] arr;
                arr = new long[5] { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_d_05
        {
            public static int Main_old()
            {
                double[] arr;
                arr = new double[5] { 1.0, 2.0, 3.0, 4.0, 5.0 };
                int x = 0;
                if (arr[0] != 1.0)
                    x = 1;
                if (arr[1] != 2.0)
                    x = 1;
                if (arr[2] != 3.0)
                    x = 1;
                if (arr[3] != 4.0)
                    x = 1;
                if (arr[4] != 5.0)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_d_06
        {
            public static int Main_old()
            {
                float[] arr;
                arr = new float[5] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };
                int x = 0;
                if (arr[0] != 1.0)
                    x = 1;
                if (arr[1] != 2.0)
                    x = 1;
                if (arr[2] != 3.0)
                    x = 1;
                if (arr[3] != 4.0)
                    x = 1;
                if (arr[4] != 5.0)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_d_08
        {
            public static int Main_old()
            {
                String[] arr;
                arr = new String[5] { "one", "two", "three", "four", "five" };
                int x = 0;
                if (!arr[0].Equals("one"))
                    x = 1;
                if (!arr[1].Equals("two"))
                    x = 1;
                if (!arr[2].Equals("three"))
                    x = 1;
                if (!arr[3].Equals("four"))
                    x = 1;
                if (!arr[4].Equals("five"))
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_d_09
        {
            public static int Main_old()
            {
                char[] arr;
                arr = new char[5] { '1', '2', '3', '4', '5' };
                int x = 0;
                if (arr[0] != '1')
                    x = 1;
                if (arr[1] != '2')
                    x = 1;
                if (arr[2] != '3')
                    x = 1;
                if (arr[3] != '4')
                    x = 1;
                if (arr[4] != '5')
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_d_10
        {
            public static int Main_old()
            {
                bool[] arr;
                arr = new bool[5] { true, false, true, false, true };
                int x = 0;
                if (!arr[0])
                    x = 1;
                if (arr[1])
                    x = 1;
                if (!arr[2])
                    x = 1;
                if (arr[3])
                    x = 1;
                if (!arr[4])
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_e_01
        {
            public static int Main_old()
            {
                int[] arr;
                arr = new int[] { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                if (arr.Length != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_e_02
        {
            public static int Main_old()
            {
                short[] arr;
                arr = new short[] { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                if (arr.Length != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_e_03
        {
            public static int Main_old()
            {
                byte[] arr;
                arr = new byte[] { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                if (arr.Length != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_e_04
        {
            public static int Main_old()
            {
                long[] arr;
                arr = new long[] { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                if (arr.Length != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_e_05
        {
            public static int Main_old()
            {
                double[] arr;
                arr = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
                int x = 0;
                if (arr[0] != 1.0)
                    x = 1;
                if (arr[1] != 2.0)
                    x = 1;
                if (arr[2] != 3.0)
                    x = 1;
                if (arr[3] != 4.0)
                    x = 1;
                if (arr[4] != 5.0)
                    x = 1;
                if (arr.Length != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_e_06
        {
            public static int Main_old()
            {
                float[] arr;
                arr = new float[] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };
                int x = 0;
                if (arr[0] != 1.0)
                    x = 1;
                if (arr[1] != 2.0)
                    x = 1;
                if (arr[2] != 3.0)
                    x = 1;
                if (arr[3] != 4.0)
                    x = 1;
                if (arr[4] != 5.0)
                    x = 1;
                if (arr.Length != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_e_08
        {
            public static int Main_old()
            {
                String[] arr;
                arr = new String[] { "one", "two", "three", "four", "five" };
                int x = 0;
                if (!arr[0].Equals("one"))
                    x = 1;
                if (!arr[1].Equals("two"))
                    x = 1;
                if (!arr[2].Equals("three"))
                    x = 1;
                if (!arr[3].Equals("four"))
                    x = 1;
                if (!arr[4].Equals("five"))
                    x = 1;
                if (arr.Length != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_e_09
        {
            public static int Main_old()
            {
                char[] arr;
                arr = new char[] { '1', '2', '3', '4', '5' };
                int x = 0;
                if (arr[0] != '1')
                    x = 1;
                if (arr[1] != '2')
                    x = 1;
                if (arr[2] != '3')
                    x = 1;
                if (arr[3] != '4')
                    x = 1;
                if (arr[4] != '5')
                    x = 1;
                if (arr.Length != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_init_e_10
        {
            public static int Main_old()
            {
                bool[] arr;
                arr = new bool[] { true, false, true, false, true };
                int x = 0;
                if (!arr[0])
                    x = 1;
                if (arr[1])
                    x = 1;
                if (!arr[2])
                    x = 1;
                if (arr[3])
                    x = 1;
                if (!arr[4])
                    x = 1;
                if (arr.Length != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_syntax_09
        {
            public static int Main_old()
            {
                int[] arr = new int[5] { 1, 2, 3, 4, 5 };
                int[] arr2 = new int[5] { 1, /* test comment */ 2, 3, 4, 5 };
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_syntax_11
        {
            public static int Main_old()
            {
                int[] arr = new int[] { };
                if (arr.Length == 0)
                    return 0;
                else
                    return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_syntax_12
        {
            public static int Main_old()
            {
                int[] arr = new int[0] { };
                if (arr.Length == 0)
                    return 0;
                else
                    return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_decl_02
        {
            const int myLength = 5;
            public static int Main_old()
            {
                int[] arr = new int[myLength] { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_decl_03
        {
            const short myLength = 5;
            public static int Main_old()
            {
                int[] arr = new int[myLength] { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_decl_04
        {
            const byte myLength = 5;
            public static int Main_old()
            {
                int[] arr = new int[myLength] { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_decl_05
        {
            const long myLength = 5;
            public static int Main_old()
            {
                int[] arr = new int[(int)myLength] { 1, 2, 3, 4, 5 };
                int x = 0;
                if (arr[0] != 1)
                    x = 1;
                if (arr[1] != 2)
                    x = 1;
                if (arr[2] != 3)
                    x = 1;
                if (arr[3] != 4)
                    x = 1;
                if (arr[4] != 5)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_shorthand_01
        {
            static int[] a = { 1, 2, 3, 4 };
            public static int Main_old()
            {
                for (int i = 0; i < 4; i++)
                {
                    int[] b = { 1, 2, 3, 4 };
                    Log.Comment(a[i].ToString());
                    Log.Comment(b[i].ToString());
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_constinit_01
        {
            int[] TestArray1 = new int[] { 1, 2, 3, 4 };
            static int Main_old()
            {
                int[] TestArray2 = new int[] { 1, 2, 3, 4 };

                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_constinit_02
        {
            int a = 1;
            int b = 2;
            int c = 3;
            int d = 4;
            void MyMethod()
            {
                int[] TestArray = new int[] { a, b, c, d };

            }
            static int Main_old()
            {
                Simple_TestClass_init_constinit_02 a = new Simple_TestClass_init_constinit_02();
                a.MyMethod();

                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_init_constinit_03
        {
            int a = 1;
            int b = 2;
            void MyMethod()
            {
                int[] TestArray = new int[] { 1, a, 2, b };

            }
            static int Main_old()
            {
                Simple_TestClass_init_constinit_03 a = new Simple_TestClass_init_constinit_03();
                a.MyMethod();

                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_acc_iter_length_01
        {
            public static int Main_old()
            {
                int[] arr1 = new int[10];
                double[] arr2 = new double[15];
                float[] arr3;
                arr3 = new float[1];
                char[] arr4;
                arr4 = new char[0];
                int x = 0;
                if (arr1.Length != 10)
                    x = 1;
                if (arr2.Length != 15)
                    x = 1;
                if (arr3.Length != 1)
                    x = 1;
                if (arr4.Length != 0)
                    x = 1;
                return x;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_acc_iter_bounds_01
        {
            public static int Main_old()
            {
                int[] arr = new int[10];
                arr[0] = 5;
                if (arr[0] == 5)
                    return 0;
                else
                    return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_acc_iter_bounds_02
        {
            public static int Main_old()
            {
                int[] arr = new int[10];
                arr[9] = 5;
                if (arr[9] == 5)
                    return 0;
                else
                    return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_acc_iter_bounds_03
        {
            public static int Main_old()
            {
                int[] arr = new int[10];
                try
                {
                    arr[-1] = 5;
                }
                catch (System.Exception)
                {
                    return 0;
                }
                return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_acc_iter_bounds_04
        {
            public static int Main_old()
            {
                int[] arr = new int[10];
                try
                {
                    arr[10] = 5;
                }
                catch (System.Exception)
                {
                    return 0;
                }
                return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_acc_iter_idxtype_a_01
        {
            public static int Main_old()
            {
                int[] arr = new int[10];
                arr[5] = 100;
                int idx = 5;
                if (arr[idx] == 100)
                    return 0;
                else
                    return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_acc_iter_idxtype_a_02
        {
            public static int Main_old()
            {
                int[] arr = new int[10];
                arr[5] = 100;
                short idx = 5;
                if (arr[idx] == 100)
                    return 0;
                else
                    return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_acc_iter_idxtype_a_03
        {
            public static int Main_old()
            {
                int[] arr = new int[10];
                arr[5] = 100;
                byte idx = 5;
                if (arr[idx] == 100)
                    return 0;
                else
                    return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_acc_iter_idxtype_a_04
        {
            public static int Main_old()
            {
                int[] arr = new int[10];
                arr[5] = 100;
                char idx = (char)5;
                if (arr[idx] == 100)
                    return 0;
                else
                    return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_acc_iter_idxtype_a_06
        {
            public static int Main_old()
            {
                int[] arr = new int[10];
                arr[5] = 100;
                long idx = 5L;
                if (arr[idx] == 100)
                    return 0;
                else
                    return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_acc_iter_idxtype_b_01
        {
            public static int Main_old()
            {
                int[] arr = new int[10];
                arr[5] = 100;
                if (arr[5] == 100)
                    return 0;
                else
                    return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_acc_iter_idxtype_b_02
        {
            public static int Main_old()
            {
                int[] arr = new int[10];
                arr[5] = 100;
                if (arr[(short)5] == 100)
                    return 0;
                else
                    return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_acc_iter_idxtype_b_03
        {
            public static int Main_old()
            {
                int[] arr = new int[10];
                arr[5] = 100;
                if (arr[(byte)5] == 100)
                    return 0;
                else
                    return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_acc_iter_idxtype_b_04
        {
            public static int Main_old()
            {
                int[] arr = new int[35];
                arr[32] = 100;
                if (arr[' '] == 100)
                    return 0;
                else
                    return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_acc_iter_idxtype_b_06
        {
            public static int Main_old()
            {
                int[] arr = new int[10];
                arr[5] = 100;
                if (arr[5L] == 100)
                    return 0;
                else
                    return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_acc_iter_iter_01
        {
            public static int Main_old()
            {
                int[] arr = new int[10];
                for (int x = 0; x < arr.Length; x++)
                {
                    arr[x] = x;
                }
                int result = 0;
                for (int y = 0; y < arr.Length; y++)
                {
                    if (arr[y] != y)
                        result = 1;
                }
                return result;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_acc_iter_iter_02
        {
            public static int Main_old()
            {
                char[] arr = new char[10];
                arr[0] = 'L';
                arr[1] = 'a';
                arr[2] = 'n';
                arr[3] = 'g';
                arr[4] = 'u';
                arr[5] = 'a';
                arr[6] = 'g';
                arr[7] = 'e';

                int result = 0;
                if (arr[0] != 'L') result = 1;
                if (arr[1] != 'a') result = 1;
                if (arr[2] != 'n') result = 1;
                if (arr[3] != 'g') result = 1;
                if (arr[4] != 'u') result = 1;
                if (arr[5] != 'a') result = 1;
                if (arr[6] != 'g') result = 1;
                if (arr[7] != 'e') result = 1;
                return result;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_smpass_a_01
        {
            public static int Main_old()
            {
                int[] arr = new int[5];
                arr[3] = 5;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_smpass_a_02
        {
            public static int Main_old()
            {
                byte[] arr = new byte[5];
                arr[3] = (byte)5;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_smpass_a_03
        {
            public static int Main_old()
            {
                short[] arr = new short[5];
                arr[3] = (short)5;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_smpass_a_04
        {
            public static int Main_old()
            {
                long[] arr = new long[5];
                arr[3] = (long)5;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_smpass_a_05
        {
            public static int Main_old()
            {
                char[] arr = new char[5];
                arr[3] = 'c';
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_smpass_a_06
        {
            public static int Main_old()
            {
                double[] arr = new double[5];
                arr[3] = 5.42;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_smpass_a_07
        {
            public static int Main_old()
            {
                float[] arr = new float[5];
                arr[3] = (float)1.00;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_smpass_a_08
        {
            public static int Main_old()
            {
                double[] arr = new double[5];
                arr[3] = (double)1.00;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_smpass_a_09
        {
            public static int Main_old()
            {
                bool[] arr = new bool[5];
                arr[3] = true;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_smpass_b_01
        {
            public static int Main_old()
            {
                int[] arr = new int[5];
                int x = 5;
                arr[3] = x;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_smpass_b_02
        {
            public static int Main_old()
            {
                byte[] arr = new byte[5];
                byte x = 5;

                arr[3] = x;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_smpass_b_03
        {
            public static int Main_old()
            {
                short[] arr = new short[5];
                short x = 5;
                arr[3] = x;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_smpass_b_04
        {
            public static int Main_old()
            {
                long[] arr = new long[5];
                long x = 5;
                arr[3] = x;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_smpass_b_05
        {
            public static int Main_old()
            {
                char[] arr = new char[5];
                char x = 'c';
                arr[3] = x;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_smpass_b_06
        {
            public static int Main_old()
            {
                double[] arr = new double[5];
                double x = 5.42;
                arr[3] = x;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_smpass_b_07
        {
            public static int Main_old()
            {
                float[] arr = new float[5];
                float x = (float)1.00;
                arr[3] = x;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_smpass_b_08
        {
            public static int Main_old()
            {
                double[] arr = new double[5];
                double x = (double)1.00;
                arr[3] = x;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_smpass_b_09
        {
            public static int Main_old()
            {
                bool[] arr = new bool[5];
                bool x = true;
                arr[3] = x;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_badass_01
        {
            public static int Main_old()
            {
                int[] arr = new int[5] { 1, 2, 3, 4, 5 };
                if (arr[2] != 3)
                    return 1;
                arr = null;
                try
                {
                    if (arr[2] == 3)
                        return 1;
                }
                catch (System.Exception)
                {
                    return 0;
                }
                return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_badass_03
        {
            public static int Main_old()
            {
                int[] arr1 = new int[5] { 1, 2, 3, 4, 5 };
                int[] arr2 = new int[3] { 6, 7, 8 };
                int result = 0;
                // Verify lengths are different
                if (arr1.Length == arr2.Length)
                    result = 1;
                // assign the small array to the larger array
                //   This is actually just making arr1 point to arr2, NOT copying
                arr1 = arr2;

                // verify the values are correct
                if (arr1[0] != 6) result = 1;
                if (arr1[1] != 7) result = 1;
                if (arr1[2] != 8) result = 1;

                if (arr1.Length != arr2.Length)
                    result = 1;
                return result;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_badass_04
        {
            public static int Main_old()
            {
                int[] arr1 = new int[3] { 6, 7, 8 };
                int[] arr2 = new int[5] { 1, 2, 3, 4, 5 };
                int result = 0;
                // Verify lengths are different
                if (arr1.Length == arr2.Length)
                    result = 1;
                // assign the larger array to the smaller array
                //   This is actually just making arr1 point to arr2, NOT copying
                arr1 = arr2;

                // verify the values are correct
                if (arr1[0] != 1) result = 1;
                if (arr1[1] != 2) result = 1;
                if (arr1[2] != 3) result = 1;

                if (arr1.Length != arr2.Length)
                    result = 1;
                return result;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_element_01
        {
            public static int Main_old()
            {
                int[] arr = new int[5] { 1, 2, 3, 4, 5 };
                arr[2] = arr[4];
                if (arr[2] == 5)
                    return 0;

                return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_element_02
        {
            public static int Main_old()
            {
                int[] arr1 = new int[5] { 1, 2, 3, 4, 5 };
                int[] arr2 = new int[5] { 6, 7, 8, 9, 10 };
                arr1[2] = arr2[4];
                if (arr1[2] == 10)
                    return 0;

                return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_argpass_01
        {
            public static int ElementTaker(int val)
            {
                return val;
            }

            public static int Main_old()
            {
                int[] arr = new int[5] { 1, 2, 3, 4, 5 };
                Log.Comment(ElementTaker(arr[2]).ToString());
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_argpass_02
        {
            public static int ElementTaker(ref int val)
            {
                val += 5;
                return val;
            }

            public static int Main_old()
            {
                int[] arr = new int[5] { 1, 2, 3, 4, 5 };
                Log.Comment(ElementTaker(ref arr[2]).ToString());
                if (arr[2] != 8)
                    return 1;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_assign_argpass_03
        {
            public static int ElementTaker(out int val)
            {
                val = 5;
                return val;
            }

            public static int Main_old()
            {
                int[] arr = new int[5] { 1, 2, 3, 4, 5 };
                Log.Comment(ElementTaker(out arr[2]).ToString());
                if (arr[2] != 5)
                    return 1;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_object_sysarr_01
        {
            public static int Main_old()
            {
                int[] arr = new int[5] { 1, 2, 3, 4, 5 };
                int result = 0;

                for (int x = 0; x < arr.Length; x++)
                    if (arr[x] != x + 1) result = 1;

                Array.Clear(arr, 0, 5);
                for (int x = 0; x < arr.Length; x++)
                    if (arr[x] != 0) result = 1;

                return result;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Simple_TestClass_object_sysarr_02
        {
            public static int Main_old()
            {
                int result = 0;
                int[] arr1 = new int[5] { 1, 2, 3, 4, 5 };
                if (arr1.Length != 5) result = 1;

                int[] arr2 = new int[] { 1, 2, 3, 4, 5 };
                if (arr2.Length != 5) result = 1;

                int[] arr3 = new int[] { };
                if (arr3.Length != 0) result = 1;
                int[] arr4 = new int[0];
                if (arr4.Length != 0) result = 1;
                return result;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }


    }
}
    

