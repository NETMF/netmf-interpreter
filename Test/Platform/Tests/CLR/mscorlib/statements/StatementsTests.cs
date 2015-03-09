////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class StatementsTests : IMFTestInterface
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

        //Statements Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\Statements
        //Label_001,Label_002,Label_004,Decl_001,Decl_002,Decl_003,Decl_004,Decl_007,Decl_009,Decl_010,Decl_012,Decl_014,Decl_016,Block_001,Empty_001,Expr_002,Expr_003,Expr_004,Expr_006,if_001,if_003,if_005,if_007,if_009,switch_001,switch_002,switch_003,switch_004,switch_005,switch_006,switch_007,switch_010,switch_012,switch_013,switch_015,switch_016,switch_017,switch_018,switch_019,switch_023,switch_030,switch_031,switch_032,switch_033,switch_034,switch_035,switch_036,switch_037,switch_038,switch_039,switch_040,switch_041,switch_042,switch_044,switch_047,switch_049,switch_string_001,dowhile_001,dowhile_002,dowhile_003,dowhile_004,dowhile_005,dowhile_006,for_001,for_003,for_004,for_006,for_007,for_008,for_009,for_010,for_011,for_013,for_014,char_in_string_s01,char_in_string_ex01,while_001,while_002,while_003,while_004,while_005,while_006,break_001,break_002,break_003,break_006,break_007,break_010,continue_001,continue_002,continue_006,continue_007,continue_010,goto_001,goto_008,goto_009,goto_010,goto_014,goto_017,goto_018,return_001,return_004,return_006,return_008,return_009,return_010,return_013,return_014,throw_001,throw_005,trycatch_001,trycatch_006,trycatch_007,tryfinally_001,tryfinally_002,tryfinally_003,tryfinally_004,tryfinally_006,tryfinally_007,tryfinally_008,tryfinally_009,tryfinally_010,tryfinally_011,tryfinally_012,tryfinally_013,Using_001,Using_002,Using_003,Using_005,Using_009,Using_010,Using_011,Using_012,Using_013,Using_014,Using_015,Using_017,Using_018,lock001,lock003,lock004,lock005,lock007,enum_002

        //Test Case Calls 
        [TestMethod]
        public MFTestResults Statements_Label_001_Test()
        {
            Log.Comment("Label_001.sc");
            Log.Comment("Make sure labels can be declared");
            if (Statements_TestClass_Label_001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Label_002_Test()
        {
            Log.Comment("Label_002.sc");
            Log.Comment("Make sure labels can be referenced. Assumes 'goto'");
            if (Statements_TestClass_Label_002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Label_004_Test()
        {
            Log.Comment("Label_004.sc");
            Log.Comment("Make sure labels can be associated with an empty statement");
            if (Statements_TestClass_Label_004.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Decl_001_Test()
        {
            Log.Comment("Decl_001.sc");
            Log.Comment("Declare a local variable of an intrinsic type");
            if (Statements_TestClass_Decl_001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Decl_002_Test()
        {
            Log.Comment("Decl_002.sc");
            Log.Comment("Declare a local variable of an intrinsic type and initialize it");
            if (Statements_TestClass_Decl_002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Decl_003_Test()
        {
            Log.Comment("Decl_003.sc");
            Log.Comment("Declare a local variable of an intrinsic type and initialize it");
            Log.Comment("with an expression.");
            if (Statements_TestClass_Decl_003.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Decl_004_Test()
        {
            Log.Comment("Decl_004.sc");
            Log.Comment("Declare a local variable of an external object type");
            if (Statements_TestClass_Decl_004.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Decl_007_Test()
        {
            Log.Comment("Decl_007.sc");
            Log.Comment("Declare a series of local variables of an intrinsic type with commas");
            if (Statements_TestClass_Decl_007.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Decl_009_Test()
        {
            Log.Comment("Decl_009.sc");
            Log.Comment("Declare a series of local variables of an intrinsic type with commas and");
            Log.Comment("initial assignments.");
            if (Statements_TestClass_Decl_009.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Decl_010_Test()
        {
            Log.Comment("Decl_010.sc");
            Log.Comment("Declare a local variable of an intrinsic type as an array");
            if (Statements_TestClass_Decl_010.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Decl_012_Test()
        {
            Log.Comment("Decl_012.sc");
            Log.Comment("Declare a local variable of an intrinsic type as an array, allocate and reference it.");
            if (Statements_TestClass_Decl_012.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Decl_014_Test()
        {
            Log.Comment("Decl_014.sc");
            Log.Comment("Declare a local variable of an intrinsic type as an initialized array");
            if (Statements_TestClass_Decl_014.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Decl_016_Test()
        {
            Log.Comment("Decl_016.sc");
            Log.Comment("Correctly declare a local variable of a type that has no default constructor");
            Log.Comment("as an array.");
            if (Statements_TestClass_Decl_016.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Block_001_Test()
        {
            Log.Comment("Block_001.sc");
            Log.Comment("Statements_TestClass_? Several types of statement blocks.  Statement blocks");
            Log.Comment("are so fundamental, that most can be tested in one pass.");
            Log.Comment("Note that by the nature of this code, many warnings");
            Log.Comment("could/should be generated about items that are never reached.");
            if (Statements_TestClass_Block_001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Empty_001_Test()
        {
            Log.Comment("Empty_001.sc");
            Log.Comment("Statements_TestClass_? Several scenarios for empty statement.  Emtpy statements");
            Log.Comment("are so fundamental, that most can be tested in one pass.");
            Log.Comment("Note that by the nature of this code, many warnings");
            Log.Comment("could/should be generated about items that are never reached.");
            if (Statements_TestClass_Empty_001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Expr_002_Test()
        {
            Log.Comment("Expr_002.sc");
            Log.Comment("Use an expression with side effects.");
            if (Statements_TestClass_Expr_002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Expr_003_Test()
        {
            Log.Comment("Expr_003.sc");
            Log.Comment("Use an expression with side effects and multiple l-values.");
            if (Statements_TestClass_Expr_003.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Expr_004_Test()
        {
            Log.Comment("Expr_004.sc");
            Log.Comment("Run a quick test of common operator/assignment combos");
            
            if (Statements_TestClass_Expr_004.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Expr_006_Test()
        {
            Log.Comment("   complex assignment");
            if (Statements_TestClass_Expr_006.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_if_001_Test()
        {
            Log.Comment("if_001.sc");
            Log.Comment("Simple boolean if with a single statement");
            if (Statements_TestClass_if_001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_if_003_Test()
        {
            Log.Comment("if_003.sc");
            Log.Comment("Simple boolean if with a block statement");
            if (Statements_TestClass_if_003.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_if_005_Test()
        {
            Log.Comment("if_005.sc");
            Log.Comment("Simple boolean if with a single statement and else");
            if (Statements_TestClass_if_005.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_if_007_Test()
        {
            Log.Comment("if_007.sc");
            Log.Comment("Simple boolean if with a block statement");
            if (Statements_TestClass_if_007.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_if_009_Test()
        {
            Log.Comment("if_009.sc");
            Log.Comment("Nest ifs with elses without blocks. Statements_TestClass_? that the 'else' ambiguity from");
            Log.Comment("C/C++ is handled the same way (else bound to closest if)");
            if (Statements_TestClass_if_009.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_001_Test()
        {
            Log.Comment("switch_001.sc");
            Log.Comment("Empty switch");
            if (Statements_TestClass_switch_001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_002_Test()
        {
            Log.Comment("switch_002.sc");
            Log.Comment("Default only switch");
            if (Statements_TestClass_switch_002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_003_Test()
        {
            Log.Comment("switch_003.sc");
            Log.Comment("Switch with single case without break - no default");
            if (Statements_TestClass_switch_003.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_004_Test()
        {
            Log.Comment("switch_004.sc");
            Log.Comment("Switch with one case, using break");
            if (Statements_TestClass_switch_004.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_005_Test()
        {
            Log.Comment("switch_005.sc");
            Log.Comment("Switch with two cases, using break");
            if (Statements_TestClass_switch_005.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_006_Test()
        {
            Log.Comment("switch_006.sc");
            Log.Comment("Switch with one case and a default");
            if (Statements_TestClass_switch_006.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_007_Test()
        {
            Log.Comment("switch_007.sc");
            Log.Comment("Switch with two cases and a default");
            if (Statements_TestClass_switch_007.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_010_Test()
        {
            Log.Comment("switch_010.sc");
            Log.Comment("Switch with a const variable in a case");
            if (Statements_TestClass_switch_010.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_012_Test()
        {
            Log.Comment("switch_012.sc");
            Log.Comment("Multiple case labels");
            if (Statements_TestClass_switch_012.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_013_Test()
        {
            Log.Comment("switch_013.sc");
            Log.Comment("test goto all over");
            Log.Comment("Expected Output");
            if (Statements_TestClass_switch_013.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_015_Test()
        {
            Log.Comment("switch_015.sc");
            Log.Comment("Run a switch over a specific type: byte");
            if (Statements_TestClass_switch_015.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_016_Test()
        {
            Log.Comment("switch_016.sc");
            Log.Comment("Run a switch over a specific type: char");
            if (Statements_TestClass_switch_016.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_017_Test()
        {
            Log.Comment("switch_017.sc");
            Log.Comment("Run a switch over a specific type: short");
            if (Statements_TestClass_switch_017.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_018_Test()
        {
            Log.Comment("switch_018.sc");
            Log.Comment("Run a switch over a specific type: int");
            if (Statements_TestClass_switch_018.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_019_Test()
        {
            Log.Comment("switch_019.sc");
            Log.Comment("Run a switch over a specific type: long");
            if (Statements_TestClass_switch_019.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_023_Test()
        {
            Log.Comment("switch_023.sc");
            Log.Comment("Run a switch over a specific type: enum");
            if (Statements_TestClass_switch_023.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_030_Test()
        {
            Log.Comment("   switch on int variable, float case");
            if (Statements_TestClass_switch_030.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_031_Test()
        {
            Log.Comment("   switch with holes in range");
            if (Statements_TestClass_switch_031.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_032_Test()
        {
            Log.Comment("   switch: default case at top");
            if (Statements_TestClass_switch_032.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_033_Test()
        {
            Log.Comment("   switch: default case in middle");
            if (Statements_TestClass_switch_033.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_034_Test()
        {
            Log.Comment("   switch: default case in middle");
            if (Statements_TestClass_switch_034.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_035_Test()
        {
            Log.Comment("Otherwise, exactly one user-defined implicit conversion (§6.4) must exist from the type of ");
            Log.Comment("the switch expression to one of the following possible governing types: sbyte, byte, short,");
            Log.Comment("ushort, int, uint, long, ulong, char, string. If no such implicit conversion exists, or if ");
            Log.Comment("more than one such implicit conversion exists, a compile-time error occurs.");
            if (Statements_TestClass_switch_035.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_036_Test()
        {
            Log.Comment("Otherwise, exactly one user-defined implicit conversion (§6.4) must exist from the type of ");
            Log.Comment("the switch expression to one of the following possible governing types: sbyte, byte, short,");
            Log.Comment("ushort, int, uint, long, ulong, char, string. If no such implicit conversion exists, or if ");
            Log.Comment("more than one such implicit conversion exists, a compile-time error occurs.");
            if (Statements_TestClass_switch_036.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_037_Test()
        {
            Log.Comment("Otherwise, exactly one user-defined implicit conversion (§6.4) must exist from the type of ");
            Log.Comment("the switch expression to one of the following possible governing types: sbyte, byte, short,");
            Log.Comment("ushort, int, uint, long, ulong, char, string. If no such implicit conversion exists, or if ");
            Log.Comment("more than one such implicit conversion exists, a compile-time error occurs.");
            if (Statements_TestClass_switch_037.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_038_Test()
        {
            Log.Comment("Otherwise, exactly one user-defined implicit conversion (§6.4) must exist from the type of ");
            Log.Comment("the switch expression to one of the following possible governing types: sbyte, byte, short,");
            Log.Comment("ushort, int, uint, long, ulong, char, string. If no such implicit conversion exists, or if ");
            Log.Comment("more than one such implicit conversion exists, a compile-time error occurs.");
            if (Statements_TestClass_switch_038.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_039_Test()
        {
            Log.Comment("Otherwise, exactly one user-defined implicit conversion (§6.4) must exist from the type of ");
            Log.Comment("the switch expression to one of the following possible governing types: sbyte, byte, short,");
            Log.Comment("ushort, int, uint, long, ulong, char, string. If no such implicit conversion exists, or if ");
            Log.Comment("more than one such implicit conversion exists, a compile-time error occurs.");
            if (Statements_TestClass_switch_039.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_040_Test()
        {
            Log.Comment("Otherwise, exactly one user-defined implicit conversion (§6.4) must exist from the type of ");
            Log.Comment("the switch expression to one of the following possible governing types: sbyte, byte, short,");
            Log.Comment("ushort, int, uint, long, ulong, char, string. If no such implicit conversion exists, or if ");
            Log.Comment("more than one such implicit conversion exists, a compile-time error occurs.");
            if (Statements_TestClass_switch_040.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_041_Test()
        {
            Log.Comment("Otherwise, exactly one user-defined implicit conversion (§6.4) must exist from the type of ");
            Log.Comment("the switch expression to one of the following possible governing types: sbyte, byte, short,");
            Log.Comment("ushort, int, uint, long, ulong, char, string. If no such implicit conversion exists, or if ");
            Log.Comment("more than one such implicit conversion exists, a compile-time error occurs.");
            if (Statements_TestClass_switch_041.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_042_Test()
        {
            Log.Comment("Otherwise, exactly one user-defined implicit conversion (§6.4) must exist from the type of ");
            Log.Comment("the switch expression to one of the following possible governing types: sbyte, byte, short,");
            Log.Comment("ushort, int, uint, long, ulong, char, string. If no such implicit conversion exists, or if ");
            Log.Comment("more than one such implicit conversion exists, a compile-time error occurs.");
            if (Statements_TestClass_switch_042.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_044_Test()
        {
            Log.Comment("Otherwise, exactly one user-defined implicit conversion (§6.4) must exist from the type of ");
            Log.Comment("the switch expression to one of the following possible governing types: sbyte, byte, short,");
            Log.Comment("ushort, int, uint, long, ulong, char, string. If no such implicit conversion exists, or if ");
            Log.Comment("more than one such implicit conversion exists, a compile-time error occurs.");
            if (Statements_TestClass_switch_044.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_047_Test()
        {
            Log.Comment("Otherwise, exactly one user-defined implicit conversion (§6.4) must exist from the type of ");
            Log.Comment("the switch expression to one of the following possible governing types: sbyte, byte, short,");
            Log.Comment("ushort, int, uint, long, ulong, char, string. If no such implicit conversion exists, or if ");
            Log.Comment("more than one such implicit conversion exists, a compile-time error occurs.");
            Log.Comment("Ensure error is emmited on when more than one implicit conversion to an acceptable governing type is defined");
            if (Statements_TestClass_switch_047.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_049_Test()
        {
            Log.Comment("warning CS1522: Empty switch block");
            if (Statements_TestClass_switch_049.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_switch_string_001_Test()
        {
            Log.Comment("   switch on string: null");
            if (Statements_TestClass_switch_string_001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_dowhile_001_Test()
        {
            Log.Comment("dowhile_001.sc");
            Log.Comment("do/while with a single statement");
            if (Statements_TestClass_dowhile_001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_dowhile_002_Test()
        {
            Log.Comment("dowhile_002.sc");
            Log.Comment("do/while with a compound statement");
            if (Statements_TestClass_dowhile_002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_dowhile_003_Test()
        {
            Log.Comment("dowhile_003.sc");
            Log.Comment("verify known false condition executes only once with single statement");
            if (Statements_TestClass_dowhile_003.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_dowhile_004_Test()
        {
            Log.Comment("dowhile_004.sc");
            Log.Comment("verify known true condition executes with single statement");
            if (Statements_TestClass_dowhile_004.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_dowhile_005_Test()
        {
            Log.Comment("dowhile_005.sc");
            Log.Comment("verify known false condition executes once with compound statements");
            if (Statements_TestClass_dowhile_005.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_dowhile_006_Test()
        {
            Log.Comment("dowhile_006.sc");
            Log.Comment("verify known true condition executes with compound statements");
            if (Statements_TestClass_dowhile_006.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_for_001_Test()
        {
            Log.Comment("for_001.sc");
            Log.Comment("empty for loop");
            if (Statements_TestClass_for_001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_for_003_Test()
        {
            Log.Comment("for_003.sc");
            Log.Comment("empty initializer in for loop");
            if (Statements_TestClass_for_003.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_for_004_Test()
        {
            Log.Comment("for_004.sc");
            Log.Comment("empty iterator in for loop");
            if (Statements_TestClass_for_004.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_for_006_Test()
        {
            Log.Comment("for_006.sc");
            Log.Comment("Full normal for loop");
            if (Statements_TestClass_for_006.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_for_007_Test()
        {
            Log.Comment("for_007.sc");
            Log.Comment("Full normal for loop with a compound statement");
            if (Statements_TestClass_for_007.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_for_008_Test()
        {
            Log.Comment("for_008.sc");
            Log.Comment("Multiple declarations in initializer");
            if (Statements_TestClass_for_008.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_for_009_Test()
        {
            Log.Comment("for_009.sc");
            Log.Comment("Statements_TestClass_? statement expression lists in for initializer");
            if (Statements_TestClass_for_009.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_for_010_Test()
        {
            Log.Comment("for_010.sc");
            Log.Comment("Statements_TestClass_? statement expression lists in for iterator");
            if (Statements_TestClass_for_010.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_for_011_Test()
        {
            Log.Comment("for_011.sc");
            Log.Comment("Statements_TestClass_? statement expression lists in for initializer and iterator");
            if (Statements_TestClass_for_011.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_for_013_Test()
        {
            Log.Comment("for_013.sc");
            Log.Comment("Verify conditional evaluates before iterator");
            if (Statements_TestClass_for_013.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_for_014_Test()
        {
            Log.Comment("for_014.sc");
            Log.Comment("Verify method calls work ok in all for loop areas");
            if (Statements_TestClass_for_014.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_char_in_string_s01_Test()
        {
            Log.Comment("Optimization to foreach (char c in String) by treating String as a char array");
            if (Statements_TestClass_char_in_string_s01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_char_in_string_ex01_Test()
        {
            Log.Comment("Optimization to foreach (char c in String) by treating String as a char array");
            if (Statements_TestClass_char_in_string_ex01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_while_001_Test()
        {
            Log.Comment("while_001.sc");
            Log.Comment("while with a single statement");
            if (Statements_TestClass_while_001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_while_002_Test()
        {
            Log.Comment("while_002.sc");
            Log.Comment("while with a compound statement");
            if (Statements_TestClass_while_002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_while_003_Test()
        {
            Log.Comment("while_003.sc");
            Log.Comment("verify known false condition doesn't execute with single statement");
            if (Statements_TestClass_while_003.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_while_004_Test()
        {
            Log.Comment("while_004.sc");
            Log.Comment("verify known true condition executes with single statement");
            if (Statements_TestClass_while_004.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_while_005_Test()
        {
            Log.Comment("while_005.sc");
            Log.Comment("verify known false condition doesn't execute with compound statements");
            if (Statements_TestClass_while_005.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_while_006_Test()
        {
            Log.Comment("while_006.sc");
            Log.Comment("verify known true condition executes with compound statements");
            if (Statements_TestClass_while_006.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_break_001_Test()
        {
            Log.Comment("break_001.sc");
            Log.Comment("Make sure break works in all basic single statement loops");
            if (Statements_TestClass_break_001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_break_002_Test()
        {
            Log.Comment("break_002.sc");
            Log.Comment("Make sure break works in all basic compound statement loops");
            if (Statements_TestClass_break_002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_break_003_Test()
        {
            Log.Comment("break_003.sc");
            Log.Comment("Make sure break optional on end of switch");
            if (Statements_TestClass_break_003.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_break_006_Test()
        {
            Log.Comment("break_006.sc");
            Log.Comment("break in an if successfully breaks loop");
            if (Statements_TestClass_break_006.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_break_007_Test()
        {
            Log.Comment("break_007.sc");
            Log.Comment("break in a blocked if successfully breaks loop");
            if (Statements_TestClass_break_007.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_break_010_Test()
        {
            Log.Comment("break_010.sc");
            Log.Comment("Make sure break correctly when nested");
            if (Statements_TestClass_break_010.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_continue_001_Test()
        {
            Log.Comment("continue_001.sc");
            Log.Comment("Make sure continue works in all basic single statement loops");
            if (Statements_TestClass_continue_001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_continue_002_Test()
        {
            Log.Comment("continue_002.sc");
            Log.Comment("Make sure continue works in all basic compound statement loops");
            Log.Comment("Expected Output");
            if (Statements_TestClass_continue_002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_continue_006_Test()
        {
            Log.Comment("continue_006.sc");
            Log.Comment("continue in an if successfully continues loop");
            if (Statements_TestClass_continue_006.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_continue_007_Test()
        {
            Log.Comment("continue_007.sc");
            Log.Comment("continue in a block if successfully continues loop");
            if (Statements_TestClass_continue_007.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_continue_010_Test()
        {
            Log.Comment("continue_010.sc");
            Log.Comment("Make sure continue works correctly when nested");
            if (Statements_TestClass_continue_010.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_goto_001_Test()
        {
            Log.Comment("goto_001.sc");
            Log.Comment("simple goto to adjust flow control");
            if (Statements_TestClass_goto_001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_goto_008_Test()
        {
            Log.Comment("goto_008.sc");
            Log.Comment("goto currect case");
            if (Statements_TestClass_goto_008.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_goto_009_Test()
        {
            Log.Comment("goto_009.sc");
            Log.Comment("goto a different case");
            Log.Comment("Expected Output");
            if (Statements_TestClass_goto_009.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_goto_010_Test()
        {
            Log.Comment("goto_010.sc");
            Log.Comment("goto default correctly");
            if (Statements_TestClass_goto_010.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_goto_014_Test()
        {
            Log.Comment("goto_014.sc");
            Log.Comment("simple gotos to test jumping to parent process.");
            if (Statements_TestClass_goto_014.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_goto_017_Test()
        {
            Log.Comment("   some gotos");
            if (Statements_TestClass_goto_017.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_goto_018_Test()
        {
            Log.Comment("   try/catch/finally with goto");
            if (Statements_TestClass_goto_018.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_return_001_Test()
        {
            Log.Comment("return_001.sc");
            Log.Comment("simple void return on a void method");
            if (Statements_TestClass_return_001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_return_004_Test()
        {
            Log.Comment("return_004.sc");
            Log.Comment("simple return a normal type, assigning, and ignoring return value");
            if (Statements_TestClass_return_004.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_return_006_Test()
        {
            Log.Comment("return_006.sc");
            Log.Comment("simple return a type mismatch that has an implicit conversion");
            if (Statements_TestClass_return_006.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_return_008_Test()
        {
            Log.Comment("return_008.sc");
            Log.Comment("simple return a type mismatch that has an explicit convertion conversion,");
            Log.Comment("applying the cast");
            if (Statements_TestClass_return_008.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_return_009_Test()
        {
            Log.Comment("return_009.sc");
            Log.Comment("return of a struct");
            if (Statements_TestClass_return_009.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_return_010_Test()
        {
            Log.Comment("return_010.sc");
            Log.Comment("return of a class");
            if (Statements_TestClass_return_010.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_return_013_Test()
        {
            Log.Comment("return_013.sc");
            Log.Comment("simple falloff on a void method");
            if (Statements_TestClass_return_013.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_return_014_Test()
        {
            Log.Comment("return_014.sc");
            Log.Comment("verify that a 'throw' is adequate for flow control analysis of return type");
            if (Statements_TestClass_return_014.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_throw_001_Test()
        {
            Log.Comment("throw_001.sc");
            Log.Comment("simple throw");
            if (Statements_TestClass_throw_001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_throw_005_Test()
        {
            Log.Comment("throw_005.sc");
            Log.Comment("simple throw with output");
            if (Statements_TestClass_throw_005.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_trycatch_001_Test()
        {
            Log.Comment("trycatch_001.sc");
            Log.Comment("simple throw");
            if (Statements_TestClass_trycatch_001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_trycatch_006_Test()
        {
            Log.Comment("trycatch_006.sc");
            Log.Comment("simple system generated System.Exception");
            if (Statements_TestClass_trycatch_006.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_trycatch_007_Test()
        {
            Log.Comment("trycatch_007.sc");
            Log.Comment("simple re-throw");
            if (Statements_TestClass_trycatch_007.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_tryfinally_001_Test()
        {
            Log.Comment("tryfinally_001.sc");
            Log.Comment("simple finally");
            if (Statements_TestClass_tryfinally_001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_tryfinally_002_Test()
        {
            Log.Comment("tryfinally_002.sc");
            Log.Comment("simple finally inside try/catch");
            if (Statements_TestClass_tryfinally_002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_tryfinally_003_Test()
        {
            Log.Comment("tryfinally_003.sc");
            Log.Comment("simple finally outside try/catch");
            if (Statements_TestClass_tryfinally_003.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_tryfinally_004_Test()
        {
            Log.Comment("tryfinally_004.sc");
            Log.Comment("simple finally passed 'over' by a goto");
            if (Statements_TestClass_tryfinally_004.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_tryfinally_006_Test()
        {
            Log.Comment("tryfinally_006.sc");
            Log.Comment("simple finally exited by throw");
            if (Statements_TestClass_tryfinally_006.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_tryfinally_007_Test()
        {
            Log.Comment("tryfinally_007.sc");
            Log.Comment("simple finally exited by throw in a called method");
            if (Statements_TestClass_tryfinally_007.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_tryfinally_008_Test()
        {
            Log.Comment("tryfinally_008.sc");
            Log.Comment("simple finally exited by return");
            if (Statements_TestClass_tryfinally_008.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_tryfinally_009_Test()
        {
            Log.Comment("tryfinally_009.sc");
            Log.Comment("simple finally exited by continue");
            if (Statements_TestClass_tryfinally_009.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_tryfinally_010_Test()
        {
            Log.Comment("tryfinally_010.sc");
            Log.Comment("simple finally exited by break");
            if (Statements_TestClass_tryfinally_010.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_tryfinally_011_Test()
        {
            Log.Comment("tryfinally_011.sc");
            Log.Comment("simple finally exited by break (where break is outside try)");
            if (Statements_TestClass_tryfinally_011.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_tryfinally_012_Test()
        {
            Log.Comment("tryfinally_012.sc");
            Log.Comment("simple finally exited by system System.Exception");
            if (Statements_TestClass_tryfinally_012.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_tryfinally_013_Test()
        {
            if (Statements_TestClass_tryfinally_013.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Using_001_Test()
        {
            Log.Comment("using_001.cs");
            Log.Comment("Statements_TestClass_? the using statement.");
            Log.Comment("Cast a class to IDisposable explicitly, use that in the using statement. (1.a)");
            if (Statements_TestClass_Using_001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Using_002_Test()
        {
            Log.Comment("using_002.cs");
            Log.Comment("Statements_TestClass_? the using statement.");
            Log.Comment("Use a class directly in using (1.b)");
            if (Statements_TestClass_Using_002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Using_003_Test()
        {
            Log.Comment("using_003.cs");
            Log.Comment("Statements_TestClass_? the using statement.");
            Log.Comment("Creation of class as part of using statement (1.c)");
            if (Statements_TestClass_Using_003.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Using_005_Test()
        {
            Log.Comment("using_005.cs");
            Log.Comment("Statements_TestClass_? the using statement.");
            Log.Comment("A class that explicitly implements IDisposable. (1.e)");
            if (Statements_TestClass_Using_005.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Using_009_Test()
        {
            Log.Comment("using_009.cs");
            Log.Comment("Statements_TestClass_? the using statement.");
            Log.Comment("Statements_TestClass_? the behavior if the used variable is nulled-out in the using block (4)");
            if (Statements_TestClass_Using_009.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Using_010_Test()
        {
            Log.Comment("using_010.cs");
            Log.Comment("Statements_TestClass_? the using statement.");
            Log.Comment("Dispose() called during normal exit (5.a)");
            if (Statements_TestClass_Using_010.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Using_011_Test()
        {
            Log.Comment("using_011.cs");
            Log.Comment("Statements_TestClass_? the using statement.");
            Log.Comment("Dispose() called after throw (5.b)");
            Log.Comment("Expected Output");
            if (Statements_TestClass_Using_011.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Using_012_Test()
        {
            Log.Comment("using_012.cs");
            Log.Comment("Statements_TestClass_? the using statement.");
            Log.Comment("Dispose() called for two objects during normal exit. (5.c)");
            if (Statements_TestClass_Using_012.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Using_013_Test()
        {
            Log.Comment("using_013.cs");
            Log.Comment("Statements_TestClass_? the using statement.");
            Log.Comment("Dispose() called for first objects with System.Exception thrown before second block. (5.d)");
            if (Statements_TestClass_Using_013.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Using_014_Test()
        {
            Log.Comment("using_014.cs");
            Log.Comment("Statements_TestClass_? the using statement.");
            Log.Comment("Dispose() called for first objects with System.Exception thrown after second block. (5.e)");
            if (Statements_TestClass_Using_014.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Using_015_Test()
        {
            Log.Comment("using_015.cs");
            Log.Comment("Statements_TestClass_? the using statement.");
            Log.Comment("Dispose() called for both objects when System.Exception thrown inside second block. (5.f)");
            if (Statements_TestClass_Using_015.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Using_017_Test()
        {
            Log.Comment("using_017.cs");
            Log.Comment("Statements_TestClass_? the using statement.");
            Log.Comment("Dispose() called for both objects when System.Exception thrown in compound case (5.h)");
            if (Statements_TestClass_Using_017.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_Using_018_Test()
        {
            Log.Comment("using_018.cs");
            Log.Comment("Statements_TestClass_? the using statement.");
            Log.Comment("Dispose() called for both objects in compound using. (5.g)");
            if (Statements_TestClass_Using_018.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_lock001_Test()
        {
            Log.Comment("The expression of a lock statement must denote a value of a reference-type");
            if (Statements_TestClass_lock001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_lock003_Test()
        {
            Log.Comment("The System.Type object of a class can conveniently be used as the mutual-exclusion lock for static methods of the class");
            if (Statements_TestClass_lock003.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_lock004_Test()
        {
            Log.Comment("possible mistaken null statement when semi-column appears directly after lock()");
            if (Statements_TestClass_lock004.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_lock005_Test()
        {
            Log.Comment("this as the lock expression in a reference type");
            if (Statements_TestClass_lock005.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_lock007_Test()
        {
            Log.Comment("nested lock statements");
            if (Statements_TestClass_lock007.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Statements_enum_002_Test()
        {
            Log.Comment("   enum: comparing constant casted to an enum type to a variable");
            if (Statements_TestClass_enum_002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }



        public class Res1 : IDisposable
        {
            public void Dispose()
            {
                Log.Comment("Res1.Dispose()");
            }
            public void Func()
            {
                Log.Comment("Res1.Func()");
            }
            public void Throw()
            {
                throw (new System.Exception("Res1"));
            }
        }

        public class Res2 : IDisposable
        {
            public void Dispose()
            {
                Log.Comment("Res2.Dispose()");
            }
            public void Func()
            {
                Log.Comment("Res2.Func()");
            }
            public void Throw()
            {
                throw (new System.Exception("Res2"));
            }
        }

        // IDispose implemented explicitly
        public class ResExplicit : IDisposable
        {
            void IDisposable.Dispose()
            {
                Log.Comment("ResExplicit.Dispose()");
            }
            public void Func()
            {
                Log.Comment("ResExplicit.Func()");
            }
            public void Throw()
            {
                throw (new System.Exception("ResExplicit"));
            }
        }

        // A class that doesn't implement IDisposable.
        public class NonRes1
        {
            public void GarbageDisposal()
            {
                Log.Comment("NonRes1.GarbageDisposal()");
            }
            public void Func()
            {
                Log.Comment("NonRes1.Func()");
            }
            public void Throw()
            {
                throw (new System.Exception("NonRes1"));
            }
        }

        // Doesn't implement IDisposable, but has a Dispose() function...
        public class NonRes2
        {
            public void Dispose()
            {
                Log.Comment("NonRes2.Dispose()");
            }
            public void Func()
            {
                Log.Comment("NonRes2.Func()");
            }
            public void Throw()
            {
                throw (new System.Exception("NonRes2"));
            }
        }

        //Compiled Test Cases 
        public class Statements_TestClass_Label_001
        {
            public static int Main_old(string[] args)
            {
            Label:
                return (0);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_Label_002
        {
            public static int Main_old(string[] args)
            {
                goto Label;
                return (1);
            Label:
                return (0);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_Label_004
        {
            public static int Main_old(string[] args)
            {
                Method();
                return (0);
            }
            public static void Method()
            {
                goto Label;
            Label: ;
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_Decl_001
        {
            public static int Main_old(string[] args)
            {
                int i;
                return (0);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_Decl_002
        {
            public static int Main_old(string[] args)
            {
                int i = 99;
                if (i != 99)
                    return (1);
                return (0);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_Decl_003
        {
            public static int Main_old(string[] args)
            {
                int i = 99;
                int j = i + 1;
                if (i != 99)
                    return (1);
                if (j != 100)
                    return (1);
                return (0);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_Decl_004
        {
            public static int Main_old(string[] args)
            {
                System.Exception r;
                return (0);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_Decl_007
        {
            public static int Main_old(string[] args)
            {
                int i, j, k;
                return (0);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_Decl_009
        {
            public static int Main_old(string[] args)
            {
                int i = 1, j = i + 1, k = i + j + 3;
                if ((i != 1) || (j != 2) || (k != 6))
                    return (1);
                return (0);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_Decl_010
        {
            public static int Main_old(string[] args)
            {
                int[] i;
                return (0);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_Decl_012
        {
            public static int Main_old(string[] args)
            {
                int[] i = new int[30];
                int j = i[23];
                if (j != 0)
                {
                    return (1);
                }
                return (0);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_Decl_014
        {
            public static int Main_old(string[] args)
            {
                int[] i = new int[] { 0, 1, 2, 3, 4 };
                int j = i[2];
                if (j != 2)
                {
                    return (1);
                }
                return (0);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Foo
        {
            public Foo(int i)
            {
                m_i = i;
            }

            public int GetInt() { return (m_i); }
            int m_i;
        }
        public class Statements_TestClass_Decl_016
        {
            public static int Main_old(string[] args)
            {
                Foo[] f = new Foo[30]; // 30 null'd foos
                Foo foo = f[23];
                for (int i = 0; i < f.Length; i++)
                {
                    foo = f[i];
                    if (foo != null)
                        return (1);
                    f[i] = foo = new Foo(i);
                    if (foo.GetInt() != i)
                    {
                        Log.Comment("new Foo() didn't take");
                        return (1);
                    }
                    if (f[i].GetInt() != i)
                    {
                        Log.Comment("Array didn't get updated");
                        return (1);
                    }
                    if (f[i] != foo)
                    {
                        Log.Comment("Array element and foo are different");
                        return (i);
                    }
                }
                return (0);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_Block_001
        {
            public static int Main_old(string[] args)
            {
                int status = 0;
                // arbitrary nesting
                {
                    int i;
                    i = 0;
                    i++;
                }
                // empty nesting
                {
                    {
                        {
                            {
                                {
                                    {
                                        {
                                            {
                                                {
                                                    {
                                                        {
                                                            {
                                                                {
                                                                    {
                                                                        {
                                                                            {
                                                                                {
                                                                                    {
                                                                                        {
                                                                                            {
                                                                                                {
                                                                                                    {
                                                                                                        {
                                                                                                            {
                                                                                                                {
                                                                                                                    {
                                                                                                                        {
                                                                                                                            {
                                                                                                                                {
                                                                                                                                    {
                                                                                                                                        {
                                                                                                                                            {
                                                                                                                                                {
                                                                                                                                                    {
                                                                                                                                                        {
                                                                                                                                                            {
                                                                                                                                                                {
                                                                                                                                                                    {
                                                                                                                                                                        {
                                                                                                                                                                            {
                                                                                                                                                                                {
                                                                                                                                                                                    {
                                                                                                                                                                                        {
                                                                                                                                                                                            {
                                                                                                                                                                                                {
                                                                                                                                                                                                    {
                                                                                                                                                                                                        {
                                                                                                                                                                                                            {
                                                                                                                                                                                                                {
                                                                                                                                                                                                                    {
                                                                                                                                                                                                                        {
                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                    {
                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                                    {
                                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                                                    {
                                                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                                                                    {
                                                                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                                                                                    {
                                                                                                                                                                                                                                                                                                        int i;
                                                                                                                                                                                                                                                                                                        i = 0;
                                                                                                                                                                                                                                                                                                        i++;
                                                                                                                                                                                                                                                                                                    }
                                                                                                                                                                                                                                                                                                }
                                                                                                                                                                                                                                                                                            }
                                                                                                                                                                                                                                                                                        }
                                                                                                                                                                                                                                                                                    }
                                                                                                                                                                                                                                                                                }
                                                                                                                                                                                                                                                                            }
                                                                                                                                                                                                                                                                        }
                                                                                                                                                                                                                                                                    }
                                                                                                                                                                                                                                                                }
                                                                                                                                                                                                                                                            }
                                                                                                                                                                                                                                                        }
                                                                                                                                                                                                                                                    }
                                                                                                                                                                                                                                                }
                                                                                                                                                                                                                                            }
                                                                                                                                                                                                                                        }
                                                                                                                                                                                                                                    }
                                                                                                                                                                                                                                }
                                                                                                                                                                                                                            }
                                                                                                                                                                                                                        }
                                                                                                                                                                                                                    }
                                                                                                                                                                                                                }
                                                                                                                                                                                                            }
                                                                                                                                                                                                        }
                                                                                                                                                                                                    }
                                                                                                                                                                                                }
                                                                                                                                                                                            }
                                                                                                                                                                                        }
                                                                                                                                                                                    }
                                                                                                                                                                                }
                                                                                                                                                                            }
                                                                                                                                                                        }
                                                                                                                                                                    }
                                                                                                                                                                }
                                                                                                                                                            }
                                                                                                                                                        }
                                                                                                                                                    }
                                                                                                                                                }
                                                                                                                                            }
                                                                                                                                        }
                                                                                                                                    }
                                                                                                                                }
                                                                                                                            }
                                                                                                                        }
                                                                                                                    }
                                                                                                                }
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                // empty hanging clause
                if (true)
                {
                }
                else
                {
                    status = 1;
                }
                while (false)
                {
                }
                do
                {
                } while (false);

                switch (status)
                {
                }
                for (; false; )
                {
                }
            Label: { }
                return (status);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_Empty_001
        {
            public static int Main_old(string[] args)
            {
                int status = 0;
                // empty series
                ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ;
                int i;
                i = 0;
                i++;
                ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ;
                // empty hanging clause
                if (true)
                    ;
                else
                {
                    status = 1;
                }
                while (false)
                    ;
                do ; while (false);

                switch (status)
                {
                    default: break;
                }
                for (; false; )
                    ;
            Label: ;
                return (status);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_Expr_002
        {
            public static int Main_old(string[] args)
            {
                bool b = false;
                b = true || false || b;
                if (!b)
                    return (1);
                return (0);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_Expr_003
        {
            public static int Main_old(string[] args)
    {
		bool b = false;
		bool b1;
		bool b2;
		b = b1 = b2 = true || false || b;
		if (!b || !b1 || !b2)
			return(1);
        return(false ? 1 : 0);
    }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_Expr_004
        {
            public static int Main_old(string[] args)
    {
		int i = 0;
		Log.Comment("Adding 5");
		i += 5;  // i == 5
		if (i != 5) return(1);
		Log.Comment("Subtracting 3");
		i -= 3;  // i == 2
		if (i != 2) return(1);
		Log.Comment("Multiplying by 4");
		i *= 4;  // i == 8
		if (i != 8) return(1);
		Log.Comment("Dividing by 2");
		i /= 2;  // i == 4
		if (i != 4) return(1);
		Log.Comment("Left Shifting 3");
		i <<= 3; // i == 32
		if (i != 32) return(1);
		Log.Comment("Right Shifting 2");
		i >>= 2; // i == 8
		if (i != 8) return(1);
		Log.Comment("ANDing against logical not");
		i &= ~i; // i = 0
		if (i != 0) return(1);
		Log.Comment("ORing by 0xBeaf");
		i |= 48815; // i = 0xBeaf
		if (i != 0xBeaf) return(1);
        return(true ? 0 : 1);
    }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_Expr_006
        {
            public static int Main_old(string[] args)
            {
                int b, c, d, e;
                e = 1;
                int a = b = c = d = e++ + 1;
                b = a = d + e * 2;
                if ((a == 6) && (a == b) && (c == 2) && (c == d) && (d == e))
                    return 0;
                else
                    return 1;
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_if_001
        {
            public static int Main_old(string[] args)
            {
                if (true)
                    return (0);
                return (1);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_if_003
        {
            public static int Main_old(string[] args)
            {
                if (true)
                {
                    int i = 0;
                    return (i);
                }
                return (1);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_if_005
        {
            public static int Main_old(string[] args)
            {
                int ret = 0;
                if (true)
                    ret = ret;
                else
                    ret = 1;
                if (false)
                    ret = 1;
                else
                    ret = ret;
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_if_007
        {
            public static int Main_old(string[] args)
            {
                int ret = 0;
                if (true)
                {
                    int i = ret;
                    ret = i;
                }
                else
                {
                    int i = 1;
                    ret = i;
                }
                if (false)
                {
                    int i = 1;
                    ret = i;
                }
                else
                {
                    int i = ret;
                    ret = i;
                }
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_if_009
        {
            public static int Main_old(string[] args)
            {
                int ret = 1; // default to fail
                if (true)
                    if (false)
                        return (1);
                    else // if this else if associated with the 1st if, it won't execute.
                        ret = 0;
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_switch_001
        {
            public static int Main_old(string[] args)
            {
                switch (true)
                {
                }
                return (0);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_switch_002
        {
            public static int Main_old(string[] args)
            {
                int ret = 1;
                switch (true)
                {
                    default:
                        ret = 0;
                        break;
                }
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_switch_003
        {
            public static int Main_old(string[] args)
            {
                int ret = 1;
                switch (true)
                {
                    case true:
                        ret = 0;
                        break;
                }
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_switch_004
        {
            public static int Main_old(string[] args)
            {
                int ret = 1;
                switch (true)
                {
                    case true:
                        ret = 0;
                        break;
                }
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_switch_005
        {
            public static int Main_old(string[] args)
            {
                int ret = 1;
                switch (true)
                {
                    case true:
                        ret = 0;
                        break;
                    case false:
                        ret = 1;
                        break;
                }
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_switch_006
        {
            public static int Main_old(string[] args)
            {
                int ret = 1;
                switch (false)
                {
                    default:
                        ret = 0;
                        break;
                    case true:
                        ret = 1;
                }
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_switch_007
        {
            public static int Main_old(string[] args)
            {
                int ret = 1;
                switch (23)
                {
                    default:
                        ret = 0;
                        break;
                    case 1:
                        ret = 1;
                        break;
                    case -2:
                        ret = 1;
                        break;
                }
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_switch_010
        {
            public static int Main_old(string[] args)
            {
                int ret = 1;
                int value = 23;
                switch (value)
                {
                    case kValue:
                        ret = 0;
                        break;
                    default:
                        ret = 1;
                        break;
                }
                return (ret);
            }
            const int kValue = 23;
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_switch_012
        {
            public static int Main_old(string[] args)
            {
                int ret = 3;
                for (int i = 0; i < 3; i++)
                {
                    switch (i)
                    {
                        case 1:
                        case 0:
                        case 2:
                            ret--;
                            break;
                        default:
                            return (1);
                    }
                }
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_switch_013
        {
            public static int Main_old(string[] args)
            {
                int ret = 6;
                switch (ret)
                {
                    case 0:
                        ret--; // 2
                        Log.Comment("case 0: ");
                        Log.Comment(ret.ToString());
                        goto case 9999;
                    case 2:
                        ret--; // 4
                        Log.Comment("case 2: ");
                        Log.Comment(ret.ToString());
                        goto case 255;
                    case 6:			// start here
                        ret--; // 5
                        Log.Comment("case 5: ");
                        Log.Comment(ret.ToString());
                        goto case 2;
                    case 9999:
                        ret--; // 1
                        Log.Comment("case 9999: ");
                        Log.Comment(ret.ToString());
                        goto default;
                    case 0xff:
                        ret--; // 3
                        Log.Comment("case 0xff: ");
                        Log.Comment(ret.ToString());
                        goto case 0;
                    default:
                        ret--;
                        Log.Comment("Default: ");
                        Log.Comment(ret.ToString());
                        if (ret > 0)
                        {
                            goto case -1;
                        }
                        break;
                    case -1:
                        ret = 999;
                        Log.Comment("case -1: ");
                        Log.Comment(ret.ToString());
                        break;
                }
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_switch_015
        {
            public static int Main_old(string[] args)
            {
                int ret = 0;
                ret = DoByte();
                return (ret);
            }
            private static int DoByte()
            {
                int ret = 2;
                byte b = 2;
                switch (b)
                {
                    case 1:
                    case 2:
                        ret--;
                        break;
                    case 3:
                        break;
                    default:
                        break;
                }
                switch (b)
                {
                    case 1:
                    case 3:
                        break;
                    default:
                        ret--;
                        break;
                }
                if (ret > 0)
                    Log.Comment("byte failed");
                return (ret);
            }


            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_switch_016
        {
            public static int Main_old(string[] args)
            {
                int ret = 0;
                ret = DoChar();
                return (ret);
            }
            private static int DoChar()
            {
                int ret = 2;
                char c = '2';
                switch (c)
                {
                    case '1':
                    case '2':
                        ret--;
                        break;
                    case '3':
                        break;
                    default:
                        break;
                }
                switch (c)
                {
                    case '1':
                    case '3':
                        break;
                    default:
                        ret--;
                        break;
                }
                if (ret > 0)
                    Log.Comment("char failed");
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_switch_017
        {
            public static int Main_old(string[] args)
            {
                int ret = 0;
                ret = DoShort();
                return (ret);
            }
            private static int DoShort()
            {
                int ret = 2;
                short s = 0x7fff;
                switch (s)
                {
                    case 1:
                    case 32767:
                        ret--;
                        break;
                    case -1:
                        break;
                    default:
                        break;
                }
                switch (s)
                {
                    case 1:
                    case -1:
                        break;
                    default:
                        ret--;
                        break;
                }
                if (ret > 0)
                    Log.Comment("short failed");
                return (ret);
            }


            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_switch_018
        {
            public static int Main_old(string[] args)
            {
                int ret = 0;
                ret = DoInt();
                return (ret);
            }
            private static int DoInt()
            {
                int ret = 2;
                int i = 0x7fffffff;
                switch (i)
                {
                    case 1:
                    case 2147483647:
                        ret--;
                        break;
                    case -1:
                        break;
                    default:
                        break;
                }
                switch (i)
                {
                    case 1:
                    case -1:
                        break;
                    default:
                        ret--;
                        break;
                }
                if (ret > 0)
                    Log.Comment("int failed");
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_switch_019
        {
            public static int Main_old(string[] args)
            {
                int ret = 0;
                ret = DoLong();
                return (ret);
            }
            private static int DoLong()
            {
                int ret = 2;
                long l = 0x7fffffffffffffffL;
                switch (l)
                {
                    case 1L:
                    case 9223372036854775807L:
                        ret--;
                        break;
                    case -1L:
                        break;
                    default:
                        break;
                }
                switch (l)
                {
                    case 1L:
                    case -1L:
                        break;
                    default:
                        ret--;
                        break;
                }
                if (ret > 0)
                    Log.Comment("long failed");
                return (ret);
            }

            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_switch_023
        {
            enum eTypes
            {
                kFirst,
                kSecond,
                kThird,
            };
            public static int Main_old(string[] args)
            {
                int ret = 0;
                ret = DoEnum();
                return (ret);
            }

            private static int DoEnum()
            {
                int ret = 2;
                eTypes e = eTypes.kSecond;
                switch (e)
                {
                    case eTypes.kThird:
                    case eTypes.kSecond:
                        ret--;
                        break;
                    case (eTypes)(-1):
                        break;
                    default:
                        break;
                }
                switch (e)
                {
                    case (eTypes)100:
                    case (eTypes)(-1):
                        break;
                    default:
                        ret--;
                        break;
                }
                if (ret > 0)
                    Log.Comment("enum	failed");
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_switch_030
        {
            public static int Main_old(string[] args)
            {
                int i = 5;
                switch (i)
                {
                    case (int)5.0f:
                        return 0;
                    default:
                        return 1;
                }
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_switch_031
        {
            public static int Main_old(string[] args)
            {
                int i = 5;
                switch (i)
                {
                    case 1:
                    case 2:
                    case 3:
                        return 1;
                    case 1001:
                    case 1002:
                    case 1003:
                        return 2;
                }
                return 0;
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_switch_032
        {
            public static int Main_old(string[] args)
            {
                string s = "hello";
                switch (s)
                {
                    default:
                        return 1;
                    case null:
                        return 1;
                    case "hello":
                        return 0;
                }
                return 1;
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_switch_033
        {
            public static int Main_old(string[] args)
            {
                string s = "hello";
                switch (s)
                {
                    case null:
                        return 1;
                    default:
                        return 1;
                    case "hello":
                        return 0;
                }
                return 1;
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_switch_034
        {
            public static implicit operator int(Statements_TestClass_switch_034 val)
            {
                return 1;
            }
            public static implicit operator float(Statements_TestClass_switch_034 val)
            {
                return 2.1f;
            }
            public static int Main_old(string[] args)
            {
                Statements_TestClass_switch_034 t = new Statements_TestClass_switch_034();
                switch (t)
                {
                    case 1:
                        Log.Comment("a");
                        return 0;
                    default:
                        return 1;
                }
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        class Statements_TestClass_switch_035
        {
            public sbyte x;

            public Statements_TestClass_switch_035(sbyte i)
            {
                x = i;
            }
            public static implicit operator sbyte(Statements_TestClass_switch_035 C)
            {
                return C.x;
            }

            public static int Main_old()
            {
                Statements_TestClass_switch_035 C = new Statements_TestClass_switch_035(12);
                switch (C)
                {
                    case 12: return 0;
                    default: return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Statements_TestClass_switch_036
        {
            public byte x;

            public Statements_TestClass_switch_036(byte i)
            {
                x = i;
            }
            public static implicit operator byte(Statements_TestClass_switch_036 C)
            {
                return C.x;
            }

            public static int Main_old()
            {
                Statements_TestClass_switch_036 C = new Statements_TestClass_switch_036(12);
                switch (C)
                {
                    case 12: return 0;
                    default: return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Statements_TestClass_switch_037
        {
            public short x;

            public Statements_TestClass_switch_037(short i)
            {
                x = i;
            }
            public static implicit operator short(Statements_TestClass_switch_037 C)
            {
                return C.x;
            }

            public static int Main_old()
            {
                Statements_TestClass_switch_037 C = new Statements_TestClass_switch_037(12);
                switch (C)
                {
                    case 12: return 0;
                    default: return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Statements_TestClass_switch_038
        {
            public ushort x;

            public Statements_TestClass_switch_038(ushort i)
            {
                x = i;
            }
            public static implicit operator ushort(Statements_TestClass_switch_038 C)
            {
                return C.x;
            }

            public static int Main_old()
            {
                Statements_TestClass_switch_038 C = new Statements_TestClass_switch_038(12);
                switch (C)
                {
                    case 12: return 0;
                    default: return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Statements_TestClass_switch_039
        {
            public int x;

            public Statements_TestClass_switch_039(int i)
            {
                x = i;
            }
            public static implicit operator int(Statements_TestClass_switch_039 C)
            {
                return C.x;
            }

            public static int Main_old()
            {
                Statements_TestClass_switch_039 C = new Statements_TestClass_switch_039(12);
                switch (C)
                {
                    case 12: return 0;
                    default: return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Statements_TestClass_switch_040
        {
            public uint x;

            public Statements_TestClass_switch_040(uint i)
            {
                x = i;
            }
            public static implicit operator uint(Statements_TestClass_switch_040 C)
            {
                return C.x;
            }

            public static int Main_old()
            {
                Statements_TestClass_switch_040 C = new Statements_TestClass_switch_040(12);
                switch (C)
                {
                    case 12: return 0;
                    default: return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Statements_TestClass_switch_041
        {
            public long x;

            public Statements_TestClass_switch_041(long i)
            {
                x = i;
            }
            public static implicit operator long(Statements_TestClass_switch_041 C)
            {
                return C.x;
            }

            public static int Main_old()
            {
                Statements_TestClass_switch_041 C = new Statements_TestClass_switch_041(12);
                switch (C)
                {
                    case 12: return 0;
                    default: return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Statements_TestClass_switch_042
        {
            public ulong x;

            public Statements_TestClass_switch_042(ulong i)
            {
                x = i;
            }
            public static implicit operator ulong(Statements_TestClass_switch_042 C)
            {
                return C.x;
            }

            public static int Main_old()
            {
                Statements_TestClass_switch_042 C = new Statements_TestClass_switch_042(12);
                switch (C)
                {
                    case 12: return 0;
                    default: return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Statements_TestClass_switch_044
        {
            public static implicit operator string(Statements_TestClass_switch_044 C)
            {
                return "true";
            }

            public static int Main_old()
            {
                Statements_TestClass_switch_044 C = new Statements_TestClass_switch_044();
                switch (C)
                {
                    case "true": return 0;
                    default: return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class X { }
        class Statements_TestClass_switch_047
        {

            public static implicit operator int(Statements_TestClass_switch_047 C)
            {
                return 1;
            }

            public static implicit operator X(Statements_TestClass_switch_047 C)
            {
                return new X();
            }

            public static int Main_old()
            {
                Statements_TestClass_switch_047 C = new Statements_TestClass_switch_047();
                switch (C)
                {
                    case 1: return 0;
                    default: return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Statements_TestClass_switch_049
        {
            public static int Main_old()
            {
                int i = 6;
                switch (i) { }   // CS1522
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Statements_TestClass_switch_string_001
        {
            public static int Main_old(string[] args)
            {
                string s = null;
                switch (s)
                {
                    case null:
                        Log.Comment("null");
                        return 0;
                    default:
                        return 1;
                }
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_dowhile_001
        {
            public static int Main_old(string[] args)
            {
                int i = 0;
                int j = 10;
                // post decrement test as well
                do
                    i++;
                while (--j > 0);
                if (i == 10)
                    return (0);
                return (1);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_dowhile_002
        {
            public static int Main_old(string[] args)
            {
                int i = 0;
                int j = 10;
                // post decrement test as well
                do
                {
                    j--;
                    i++;
                } while (j > 0);
                if (i == 10)
                    return (0);
                return (1);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_dowhile_003
        {
            public static int Main_old(string[] args)
            {
                bool bFalse = false;
                int ret = 1;
                do
                    ret--;
                while (bFalse);
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_dowhile_004
        {
            public static int Main_old(string[] args)
            {
                bool bTrue = true;
                do
                    return (0);
                while (bTrue);
                return (1);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_dowhile_005
        {
            public static int Main_old(string[] args)
            {
                bool bFalse = false;
                int ret = 1;
                do
                {
                    Log.Comment("Hello World");
                    ret--;
                } while (bFalse);
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_dowhile_006
        {
            public static int Main_old(string[] args)
            {
                bool bTrue = true;
                do
                {
                    Log.Comment("Hello World");
                    return (0);
                } while (bTrue);
                return (1);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_for_001
        {
            public static int Main_old(string[] args)
            {
                for (; ; )
                    return (0);
                return (1);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_for_003
        {
            public static int Main_old(string[] args)
            {
                int i = 0;
                int ret = 10;
                for (; i < 10; i++)
                    ret--;
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_for_004
        {
            public static int Main_old(string[] args)
            {
                for (int i = 0; i < 10; )
                    i++;
                return (0);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_for_006
        {
            public static int Main_old(string[] args)
            {
                int ret = 10;
                for (int i = 0; i < 10; i++)
                    ret--;
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_for_007
        {
            public static int Main_old(string[] args)
            {
                int ret1 = 10;
                int ret2 = -10;
                for (int i = 0; i < 10; i++)
                {
                    ret1--;
                    ret2++;
                }
                return (ret1 | ret2); // bit or
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_for_008
        {
            public static int Main_old(string[] args)
            {
                int ret1 = 10;
                int ret2 = 0;
                for (int i = 0, j = -10; i < 10; i++)
                {
                    ret1--;
                    j++;
                    ret2 = j;
                }
                return (ret1 | ret2); // bit or
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_for_009
        {
            public static int Main_old(string[] args)
            {
                int ret = 10;
                int i, j, k;
                for (i = 0, j = i + 1, k = j + 1; i < 10; i++)
                {
                    ret--;
                    k++;
                    j++;
                }
                if (i + 1 != j)
                {
                    Log.Comment("Failure: i + 1 != j");
                    return (1);
                }
                if (j + 1 != k)
                {
                    Log.Comment("Failure: j + 1 != k");
                    return (1);
                }
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_for_010
        {
            public static int Main_old(string[] args)
            {
                int ret = 10;
                int i, j = 1, k = 2;
                for (i = 0; i < 10; i++, j++, k++)
                {
                    ret--;
                }
                if (i + 1 != j)
                {
                    Log.Comment("Failure: i + 1 != j");
                    return (1);
                }
                if (j + 1 != k)
                {
                    Log.Comment("Failure: j + 1 != k");
                    return (1);
                }
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_for_011
        {
            public static int Main_old(string[] args)
            {
                int ret = 10;
                int i, j, k = 2;
                for (i = 0, j = i + 1; i < 10; i++, k++)
                {
                    ret--;
                    j++;
                }
                if (i + 1 != j)
                {
                    Log.Comment("Failure: i + 1 != j");
                    return (1);
                }
                if (j + 1 != k)
                {
                    Log.Comment("Failure: j + 1 != k");
                    return (1);
                }
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_for_013
        {
            public static int Main_old(string[] args)
            {
                int ret = 0;
                for (; false; ret++)
                    ;
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_for_014
        {
            public static int Main_old(string[] args)
            {
                int ret = 10;
                for (Initializer(); Conditional(); Iterator())
                    ret = Body(ret);
                return (ret);
            }
            public static int Initializer()
            {
                m_i = 0;
                return (0);
            }
            public static bool Conditional()
            {
                return (m_i < 10);
            }
            public static void Iterator()
            {
                m_i++;
            }
            public static int Body(int ret)
            {
                return (--ret);
            }
            private static int m_i;
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_char_in_string_s01
        {
            public static int Main_old()
            {
                String Str = new String('\0', 1024);
                int i = 0;
                foreach (char C in Str)
                {
                    i++;
                    if (C != '\0')
                        return 1;

                }

                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Statements_TestClass_char_in_string_ex01
        {
            public static int Main_old()
            {
                String Str = null;
                try
                {
                    foreach (char C in Str)
                    {
                        return 1;
                        Log.Comment("Fail");
                    }
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
    

        public class Statements_TestClass_while_001
        {
            public static int Main_old(string[] args)
            {
                int i = 0;
                int j = 10;
                // post decrement test as well
                while (j-- > 0)
                    i++;
                if (i == 10)
                    return (0);
                return (1);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_while_002
        {
            public static int Main_old(string[] args)
            {
                int i = 0;
                int j = 10;
                // post decrement test as well
                while (j > 0)
                {
                    j--;
                    i++;
                }
                if (i == 10)
                    return (0);
                return (1);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_while_003
        {
            public static int Main_old(string[] args)
            {
                bool bFalse = false;
                while (bFalse)
                    return (1);
                return (0);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_while_004
        {
            public static int Main_old(string[] args)
            {
                bool bTrue = true;
                while (bTrue)
                    return (0);
                return (1);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_while_005
        {
            public static int Main_old(string[] args)
            {
                bool bFalse = false;
                while (bFalse)
                {
                    Log.Comment("Hello World");
                    return (1);
                }
                return (0);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_while_006
        {
            public static int Main_old(string[] args)
            {
                bool bTrue = true;
                while (bTrue)
                {
                    Log.Comment("Hello World");
                    return (0);
                }
                return (1);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_break_001
        {
            public static int Main_old(string[] args)
            {
                while (true)
                    break;
                do
                    break;
                while (true);
                for (; true; )
                    break;
                int[] iArr = new int[20];
                foreach (int i in iArr)
                    break;
                return (0);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_break_002
        {
            public static int Main_old(string[] args)
            {
                int ret = 0;
                while (true)
                {
                    break;
                    ret++;
                }
                do
                {
                    break;
                    ret++;
                } while (true);
                for (; true; )
                {
                    break;
                    ret++;
                }
                int[] iArr = new int[20];
                foreach (int i in iArr)
                {
                    break;
                    ret++;
                }
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_break_003
        {
            public static int Main_old(string[] args)
            {
                int ret = 1;
                switch (ret)
                {
                    case 1:
                        ret = 0;
                        break;
                }
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_break_006
        {
            public static int Main_old(string[] args)
            {
                int ret = 0;
                while (true)
                {
                    if (ret == 0)
                        break;
                    ret = 1;
                }
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_break_007
        {
            public static int Main_old(string[] args)
            {
                int ret = 0;
                while (true)
                {
                    if (ret == 0)
                    {
                        break;
                        ret = 1;
                    }
                    ret = 1;
                }
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_break_010
        {
            public static int Main_old(string[] args)
            {
                int ret = 3;
                while (true)
                {
                    do
                    {
                        for (ret--; true; ret--)
                        {
                            break;
                        }
                        ret--;
                        break;
                    } while (true);
                    ret--;
                    break;
                }
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_continue_001
        {
            public static int Main_old(string[] args)
            {
                int i = 5;
                int ret = 0;
                Log.Comment("Doing while");
                while (--i != 0)
                    continue;
                if (i != 0)
                    return (1);
                Log.Comment("Doing do/while");
                i = 5;
                do
                    continue;
                while (--i != 0);
                if (i != 0)
                    return (1);
                Log.Comment("Doing for");
                for (i = 5; i != 0; i--)
                    continue;
                int[] iArr = new int[20];
                foreach (int i2 in iArr)
                    continue;
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_continue_002
        {
            public static int Main_old(string[] args)
            {
                int i = 1;
                int ret = 6;
                Log.Comment(ret.ToString());
                while (i-- > 0)
                {
                    ret--;
                    continue;
                    ret--;
                }
                Log.Comment(ret.ToString());
                i = 1;
                do
                {
                    ret--;
                    continue;
                    ret--;
                } while (--i > 0);
                Log.Comment(ret.ToString());
                for (i = 1; i > 0; i--)
                {
                    ret--;
                    continue;
                    ret--;
                }
                int[] iArr = new int[3];
                i = 0;
                Log.Comment(ret.ToString());
                foreach (int i2 in iArr)
                {
                    ret--;
                    continue;
                    ret--;
                }
                Log.Comment(ret.ToString());
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_continue_006
        {
            public static int Main_old(string[] args)
            {
                int ret = 10;
                while (ret > 0)
                {
                    if (--ret >= 0)
                        continue;
                    return (1);
                }
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_continue_007
        {
            public static int Main_old(string[] args)
            {
                int ret = 1;
                while (ret != 0)
                {
                    if (ret == 1)
                    {
                        ret = 0;
                        continue;
                        ret = 1;
                    }
                    ret = 1;
                }
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_continue_010
        {
            public static int Main_old(string[] args)
            {
                int ret = 125;
                int iWhile = 5, iDo = 5, iFor = 6;
                while (iWhile-- > 0)
                {
                    if (iDo != 5)
                        return (1);
                    do
                    {
                        if (iFor != 6)
                            return (1);
                        for (iFor--; iFor > 0; iFor--)
                        {
                            ret--;
                            continue;
                            return (1);
                        }
                        iFor = 6;
                        iDo--;
                        continue;
                        return (1);
                    } while (iDo > 0);
                    iDo = 5;
                    continue;
                    return (1);
                }
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_goto_001
        {
            public static int Main_old(string[] args)
            {
                goto Label2; // jump ahead
                return (1);
            Label1:
                goto Label3; // end it
                return (1);
            Label2:
                goto Label1; // jump backwards
                return (1);
            Label3:
                return (0);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_goto_008
        {
            public static int Main_old(string[] args)
            {
                int s = 23;
                int ret = 5;
                switch (s)
                {
                    case 21:
                        break;
                    case 23:
                        if (--ret > 0)
                            goto case 23;
                        break;
                }
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_goto_009
        {
            public static int Main_old(string[] args)
            {
                int ret = 6;
                switch (32)
                {
                    case 1:
                        if (--ret > 0)
                        {
                            Log.Comment("Case 1:" + ret.ToString());
                            goto case 32;
                        }
                        break;
                    case 32:
                        if (--ret > 0)
                        {
                            Log.Comment("Case 32:" + ret.ToString());
                            goto case 1;
                        }
                        break;
                }
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_goto_010
        {
            public static int Main_old(string[] args)
            {
                int s = 23;
                int ret = 5;
                switch (s)
                {
                    case 21:
                        break;
                    default:
                        if (--ret > 0)
                            goto default;
                        break;
                    case 23:
                        goto default;
                }
                return (ret);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_goto_014
        {
            public static int Main_old(string[] args)
            {
                bool bLoopOnce = true;
                int i = 0;
            Top:
                for (i = 0; i < 10; i++)
                {
                    if (i > 5)
                        return (1);
                    if (i == 5)
                        goto LeaveFor;
                }
            LeaveFor:
                i = 0;
                do
                {
                    if (++i > 5)
                        return (1);
                    if (i == 5)
                    {
                        goto LeaveDo;
                    }
                } while (i < 10);
            LeaveDo:
                i = 0;
                while (i < 10)
                {
                    if (++i > 5)
                        return (1);
                    if (i == 5)
                        goto LeaveWhile;
                }
            LeaveWhile:
                if (bLoopOnce)
                {
                    bLoopOnce = !bLoopOnce;

                    while (true)
                    {
                        do
                        {
                            for (; ; )
                            {
                                goto Top;
                            }
                        } while (true);
                    }
                }
                return (0);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_goto_017
        {
            public static int Main_old(string[] args)
            {
                string target = "label1";
            label1:
            label2:
                if (target == "label1")
                {
                    target = "label2";
                    goto label1;
                }
                else if (target == "label2")
                {
                    target = "exit";
                    goto label2;
                }
                return 0;
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_goto_018
        {
            public static int Main_old(string[] args)
            {
                string target = "label1";
            label1:
                try
                {
                    if (target == "label1")
                    {
                        target = "exit";
                        goto label1;
                    }
                }
                catch
                {
                }
                finally
                {
                }
                return 0;
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_return_001
        {
            public static int Main_old(string[] args)
            {
                Statements_TestClass_return_001.sMethod();
                Statements_TestClass_return_001 t = new Statements_TestClass_return_001();
                t.Method();
                return (0);
            }
            private static void sMethod()
            {
                return;
            }
            private void Method()
            {
                return;
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_return_004
        {
            public static int Main_old(string[] args)
    {
		int i;
		i = Statements_TestClass_return_004.sMethod();
		Statements_TestClass_return_004.sMethod();
		Statements_TestClass_return_004 t = new Statements_TestClass_return_004();
		i = t.Method();
		t.Method();
		return(i == 0 ? 0 : 1);
    }
            private static int sMethod()
            {
                return (1);
            }
            private int Method()
            {
                return (0);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_return_006
        {
            public static int Main_old(string[] args)
    {
		int i;
		i = Statements_TestClass_return_006.sMethod();
		if (i != 0)
			return(1);
		Statements_TestClass_return_006 t = new Statements_TestClass_return_006();
		i = t.Method();
		return(i == 0 ? 0 : 1);
    }
            private static int sMethod()
            {
                short s = 0;
                return (s);
            }
            private int Method()
            {
                short s = 0;
                return (s);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_return_008
        {
            public static int Main_old(string[] args)
    {
		short s;
		s = Statements_TestClass_return_008.sMethod();
		if (s != 0)
			return(1);
		Statements_TestClass_return_008 t = new Statements_TestClass_return_008();
		s = t.Method();
		return(s == 0 ? 0 : 1);
    }
            private static short sMethod()
            {
                int i = 0;
                return ((short)i);
            }
            private short Method()
            {
                int i = 0;
                return ((short)i);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        struct S
        {
            public int i;
            public string s;
        }
        public class Statements_TestClass_return_009
        {
            public static int Main_old(string[] args)
    {
		S s;
		s = Statements_TestClass_return_009.sMethod();
		if (s.i.ToString().CompareTo(s.s) != 0) {
			return(1);
		}
		Statements_TestClass_return_009.sMethod();
		Statements_TestClass_return_009 t = new Statements_TestClass_return_009();
		s = t.Method();
		t.Method();
		if (s.i.ToString().CompareTo(s.s) != 0) {
			return(1);
		}
		return(s.i == 0 ? 0 : 1);
    }
            private static S sMethod()
            {
                S s;
                s.i = 1;
                s.s = s.i.ToString();
                return (s);
            }
            private S Method()
            {
                S s;
                s.i = 0;
                s.s = s.i.ToString();
                return (s);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        class C
        {
            public int i;
            public string s;
        }
        public class Statements_TestClass_return_010
        {
            public static int Main_old(string[] args)
    {
		C c;
		c = Statements_TestClass_return_010.sMethod();
		if (c.i.ToString().CompareTo(c.s) != 0) {
			return(1);
		}
		Statements_TestClass_return_010.sMethod();
		Statements_TestClass_return_010 t = new Statements_TestClass_return_010();
		c = t.Method();
		t.Method();
		if (c.i.ToString().CompareTo(c.s) != 0) {
			return(1);
		}
		return(c.i == 0 ? 0 : 1);
    }
            private static C sMethod()
            {
                C c = new C();
                c.i = 1;
                c.s = c.i.ToString();
                return (c);
            }
            private C Method()
            {
                C c = new C();
                c.i = 0;
                c.s = c.i.ToString();
                return (c);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_return_013
        {
            public static int Main_old(string[] args)
            {
                Statements_TestClass_return_013.sMethod();
                Statements_TestClass_return_013 t = new Statements_TestClass_return_013();
                t.Method();
                return (0);
            }
            private static void sMethod()
            {
            }
            private void Method()
            {
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_return_014
        {
            public static int Main_old(string[] args)
    {
		int i;
		i = Statements_TestClass_return_014.sMethod(1);
		Statements_TestClass_return_014.sMethod(2);
		Statements_TestClass_return_014 t = new Statements_TestClass_return_014();
		i = t.Method(3);
		t.Method(4);
		return(i == 0 ? 0 : 1);
    }
            private static int sMethod(int i)
            {
                if (i > 0)
                    return (1);
                throw new System.Exception();
            }
            private int Method(int i)
            {
                if (i == 0)
                {
                    return (0);
                }
                else
                {
                    if (i > 0)
                    {
                        switch (i)
                        {
                            case 1:
                                return (0);
                            case 3:
                                while (i > 0)
                                {
                                    return (0);
                                }
                                for (; i < 0; )
                                {
                                    return (1);
                                }
                                throw new System.Exception();
                            default:
                                return (0);
                        }
                    }
                    else
                    {
                        return (0);
                    }
                }
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_throw_001
        {
            public static int Main_old(string[] args)
    {
		int i;
		i = Statements_TestClass_throw_001.sMethod();
		if (i > 0)
			return(1);
		Statements_TestClass_throw_001.sMethod();
		Statements_TestClass_throw_001 t = new Statements_TestClass_throw_001();
		i = t.Method();
		t.Method();
		return(i == 0 ? 0 : 1);
    }
            private static int sMethod()
            {
                try
                {
                    throw new System.Exception();
                }
                catch (System.Exception e)
                {
                    return (0);
                }
                return (1);
            }
            private int Method()
            {
                try
                {
                    throw new System.Exception();
                }
                catch (System.Exception e)
                {
                    return (0);
                }
                return (1);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_throw_005
        {
            public static int Main_old(string[] args)
    {
		int i;
		i = Statements_TestClass_throw_005.sMethod();
		return(i == 0 ? 0 : 1);
    }
            private static int sMethod()
            {
                int i = 1;
                try
                {
                    System.Exception e = new System.Exception();
                    throw (e);
                }
                catch (System.Exception e)
                {
                    Log.Comment(e.ToString());
                    return (0);
                }
                return (i);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_trycatch_001
        {
            public static int Main_old(string[] args)
    {
		int i;
		Statements_TestClass_trycatch_001 t = new Statements_TestClass_trycatch_001();
		i = t.Method();
		return(i == 0 ? 0 : 1);
    }
            private int Method()
            {
                try
                {
                    throw new System.Exception();
                }
                catch (System.Exception e)
                {
                    return (0);
                }
                return (1);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_trycatch_006
        {
            public static int Main_old(string[] args)
    {
		int i;
		Statements_TestClass_trycatch_006 t = new Statements_TestClass_trycatch_006();
		i = t.Method(0);
		return(i == 0 ? 0 : 1);
    }
            private int Method(int i)
            {
                try
                {
                    int x = 1 / i;
                    Log.Comment(x.ToString()); // prevent it being optimized away
                }
                catch (System.Exception e)
                {
                    return (0);
                }
                return (1);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_trycatch_007
        {
            public static int Main_old(string[] args)
    {
		int i;
		Statements_TestClass_trycatch_007 t = new Statements_TestClass_trycatch_007();
		i = t.Method();
		int tt = i == 0 ? 0 : 1;
		Log.Comment("Value is" + tt);
		return(i == 0 ? 0 : 1);
    }
            private int Method()
            {
                try
                {
                    try
                    {
                        Thrower();
                        //throw new System.Exception();
                    }
                    catch (System.Exception e)
                    {
                        Log.Comment("Rethrow");
                        throw;
                    }
                }
                catch (System.Exception e)
                {
                    Log.Comment("Recatch");
                    return (0);
                }
                return (1);
            }
            private void Thrower()
            {
                throw new System.Exception();
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_tryfinally_001
        {
            public static int Main_old(string[] args)
    {
		int i;
		Statements_TestClass_tryfinally_001 t = new Statements_TestClass_tryfinally_001();
		i = t.Method(2);
		return(i == 0 ? 0 : 1);
    }
            private int Method(int i)
            {
                try
                {
                    i--;
                }
                finally
                {
                    i--;
                }

                return (i);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_tryfinally_002
        {
            public static int Main_old(string[] args)
    {
		int i;
		Statements_TestClass_tryfinally_002 t = new Statements_TestClass_tryfinally_002();
		i = t.Method(1);
		return(i == 0 ? 0 : 1);
    }
            private int Method(int i)
            {
                try
                {
                    try
                    {
                        throw new System.Exception();
                    }
                    finally
                    {
                        i--;
                    }
                }
                catch
                {

                }
                return (i);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_tryfinally_003
        {
            public static int Main_old(string[] args)
    {
		int i;
		Statements_TestClass_tryfinally_003 t = new Statements_TestClass_tryfinally_003();
		i = t.Method(1);
		return(i == 0 ? 0 : 1);
    }
            private int Method(int i)
            {
                try
                {
                    try
                    {
                        throw new System.Exception();
                    }
                    catch
                    {
                    }
                }
                finally
                {
                    i--;
                }
                return (i);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_tryfinally_004
        {
            public static int Main_old(string[] args)
    {
		int i;
		Statements_TestClass_tryfinally_004 t = new Statements_TestClass_tryfinally_004();
		i = t.Method(1);
		return(i == 0 ? 0 : 1);
    }
            private int Method(int i)
            {
                try
                {
                    goto TheEnd;
                }
                finally
                {
                    i--;
                }
            TheEnd:
                return (i);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_tryfinally_006
        {
            public static int Main_old(string[] args)
    {
		int i = 0;
		Statements_TestClass_tryfinally_006 t = new Statements_TestClass_tryfinally_006();
		try {
			i = t.Method(1);
		} catch {}
		return(i == 0 ? 0 : 1);
    }
            private int Method(int i)
            {
                try
                {
                    throw new System.Exception();
                }
                finally
                {
                    i--;
                }
                return (i);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_tryfinally_007
        {
            public static int Main_old(string[] args)
    {
		int i = 0;
		Statements_TestClass_tryfinally_007 t = new Statements_TestClass_tryfinally_007();
		try {
			i = t.Method(1);
		} catch {}
		return(i == 0 ? 0 : 1);
    }
            private int Method(int i)
            {
                try
                {
                    DeepMethod(i);
                }
                finally
                {
                    i--;
                }
                return (i);
            }
            private int DeepMethod(int i)
            {
                throw new System.Exception();
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_tryfinally_008
        {
            public static int Main_old(string[] args)
    {
		int i = 1;
		Statements_TestClass_tryfinally_008 t = new Statements_TestClass_tryfinally_008();
		try {
			i = t.Method(0);
		} catch {}
		return(i == 0 ? 0 : 1);
    }
            private int Method(int i)
            {
                try
                {
                    return (i);
                }
                finally
                {
                    i++;
                }
                return (i);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_tryfinally_009
        {
            public static int Main_old(string[] args)
    {
		int i = 1;
		Statements_TestClass_tryfinally_009 t = new Statements_TestClass_tryfinally_009();
		try {
			i = t.Method(1);
		} catch {}
		return(i == 0 ? 0 : 1);
    }
            private int Method(int i)
            {
                bool b = true;
                while (b)
                {
                    try
                    {
                        b = false;
                        continue;
                    }
                    finally
                    {
                        i--;
                    }
                }
                return (i);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_tryfinally_010
        {
            public static int Main_old(string[] args)
    {
		int i = 1;
		Statements_TestClass_tryfinally_010 t = new Statements_TestClass_tryfinally_010();
		try {
			i = t.Method(1);
		} catch {}
		return(i == 0 ? 0 : 1);
    }
            private int Method(int i)
            {
                while (true)
                {
                    try
                    {
                        break;
                    }
                    finally
                    {
                        i--;
                    }
                }
                return (i);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_tryfinally_011
        {
            public static int Main_old(string[] args)
    {
		int i = 1;
		Statements_TestClass_tryfinally_011 t = new Statements_TestClass_tryfinally_011();
		try {
			i = t.Method(0);
		} catch {}
		return(i == 0 ? 0 : 1);
    }
            private int Method(int i)
            {
                while (true)
                {
                    break;
                    try
                    {
                        break;
                    }
                    finally
                    {
                        i++;
                    }
                }
                return (i);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public class Statements_TestClass_tryfinally_012
        {
            public static int Main_old(string[] args)
    {
		int i = 0;
		bool bCatch = false;
		Statements_TestClass_tryfinally_012 t = new Statements_TestClass_tryfinally_012();
		try {
			i = t.Method(1);
		} catch { bCatch = true; }
		if (!bCatch)
			i = 1;
		return(i == 0 ? 0 : 1);
    }
            private int Method(int i)
            {
                try
                {
                    Log.Comment((10 / (i - 1)).ToString());
                }
                finally
                {
                    i--;
                }
                return (i);
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        class Statements_TestClass_tryfinally_013
        {
            static void Main_old(string[] args)
            {
            before_try:
                try
                {
                }
                catch (Exception)
                {
                    goto before_try;
                }
                finally
                {

                before_inner_try:
                    try
                    {
                    }
                    catch (Exception)
                    {
                        goto before_inner_try;
                    }
                    finally
                    {
                    }
                }
            }
            public static bool testMethod()
            {
                Main_old(null);
                return true;
            }
        }
        // two normal classes...
        public class Statements_TestClass_Using_001
        {
            public static void Main_old()
            {
                Res1 res1 = new Res1();
                IDisposable id = (IDisposable)res1;
                using (id)
                {
                    res1.Func();
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        // two normal classes...
        public class Statements_TestClass_Using_002
        {
            public static void Main_old()
            {
                Res1 res1 = new Res1();
                using (res1)
                {
                    res1.Func();
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        // two normal classes...
        public class Statements_TestClass_Using_003
        {
            public static void Main_old()
            {
                using (Res1 res1 = new Res1())
                {
                    res1.Func();
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        // two normal classes...
        public class Statements_TestClass_Using_005
        {
            public static void Main_old()
            {
                using (ResExplicit resExplicit = new ResExplicit())
                {
                    resExplicit.Func();
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        // two normal classes...
        public class Statements_TestClass_Using_009
        {
            public static void Main_old()
            {
                Res1 res1 = new Res1();
                using (res1)
                {
                    res1.Func();
                    res1 = null;
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        // two normal classes...
        public class Statements_TestClass_Using_010
        {
            public static void Main_old()
            {
                using (Res1 res1 = new Res1())
                {
                    res1.Func();
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        // two normal classes...
        public class Statements_TestClass_Using_011
        {
            public static void Main_old()
            {
                try
                {
                    using (Res1 res1 = new Res1())
                    {
                        res1.Throw();
                    }
                }
                catch
                {
                    Log.Comment("System.Exception caught");
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        // two normal classes...
        public class Statements_TestClass_Using_012
        {
            public static void Main_old()
            {
                using (Res1 res1 = new Res1())
                {
                    res1.Func();
                    using (Res2 res2 = new Res2())
                    {
                        res2.Func();
                    }
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        // two normal classes...
        public class Statements_TestClass_Using_013
        {
            public static void Main_old()
            {
                try
                {
                    using (Res1 res1 = new Res1())
                    {
                        res1.Func();
                        res1.Throw();
                        using (Res2 res2 = new Res2())
                        {
                            res2.Func();
                        }
                    }
                }
                catch
                {
                    Log.Comment("System.Exception caught");
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        // two normal classes...
        public class Statements_TestClass_Using_014
        {
            public static void Main_old()
            {
                try
                {
                    using (Res1 res1 = new Res1())
                    {
                        res1.Func();
                        using (Res2 res2 = new Res2())
                        {
                            res2.Func();
                        }
                        res1.Throw();
                    }
                }
                catch
                {
                    Log.Comment("System.Exception caught");
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        // two normal classes...
        public class Statements_TestClass_Using_015
        {
            public static void Main_old()
            {
                try
                {
                    using (Res1 res1 = new Res1())
                    {
                        res1.Func();
                        using (Res2 res2 = new Res2())
                        {
                            res2.Func();
                            res1.Throw();
                        }
                    }
                }
                catch
                {
                    Log.Comment("System.Exception caught");
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        // two normal classes...
        public class Statements_TestClass_Using_017
        {
            public static void Main_old()
            {
                try
                {
                    using (Res1 res1 = new Res1(),
                        res1a = new Res1())
                    {
                        res1.Func();
                        res1.Func();
                        res1.Throw();
                    }
                }
                catch
                {
                    Log.Comment("System.Exception caught");
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        // two normal classes...
        public class Statements_TestClass_Using_018
        {
            public static void Main_old()
            {
                using (Res1 res1 = new Res1(),
                    res1a = new Res1())
                {
                    res1.Func();
                    res1a.Func();
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        class Statements_TestClass_lock001
        {
            public static int Main_old()
            {
                Statements_TestClass_lock001 C = new Statements_TestClass_lock001();
                lock (C)
                {
                    return 0;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Statements_TestClass_lock003
        {
            public static int Main_old()
            {
                lock (typeof(Statements_TestClass_lock003))
                {
                    return 0;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Statements_TestClass_lock004
        {
            public static int Main_old()
            {
                lock (typeof(Statements_TestClass_lock004)) ;
                {
                    return 0;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Statements_TestClass_lock005
        {

            public void TryMe()
            {
                lock (this) { }
            }
            public static int Main_old()
            {
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Statements_TestClass_lock007
        {
            public static int Main_old()
            {
                lock (typeof(Statements_TestClass_lock007))
                {
                    Statements_TestClass_lock007 C = new Statements_TestClass_lock007();
                    lock (C)
                    {
                        return 0;
                    }
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Statements_TestClass_enum_002
        {
            private const int CONST = 2;
            public static int Main_old(string[] args)
            {
                int i = 2;
                if (i < (int)(MyEnum)CONST)
                    return 1;
                if (i == (int)(MyEnum)0)
                    return 1;
                return 0;
            }
            public static bool testMethod()
{
	return (Main_old(null) == 0);
}

        }
        public enum MyEnum
        {
            aaa,
            bbb,
            ccc
        }



    }
}
