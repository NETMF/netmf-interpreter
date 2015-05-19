////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class EnumTests : IMFTestInterface
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

        //Enum Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\Enum
        //enum01,enum02,enum07,enum09,enum10,enum11,enum27,enum28,enum29,enum30,enum31,enum33,enum34,enum35,enum36,enum37,enum38,enum39,enum40,enum41,enum42,enum43,enum43u,enum44,enum45,enum46,enum46u,enum47,enum47u,enum48,enum48u,enum54,enum55,enum56,enum57,enum58,enum62,enum63,enum64,enum65,enum66,enum67,enum68,enum69,enum70,enum71,enum72,enum73,enum74,enum75,enum77,enum78,enum83,enum86,enum93,enum94,enum_flags01,enum_flags02,enum_flags03,enum_flags04

        //Test Case Calls 
        [TestMethod]
        public MFTestResults Enum_enum01_Test()
        {
            Log.Comment("Make sure that a basic enum declaration, definition, and assignment work.");
            if (Enum_TestClass_enum01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum02_Test()
        {
            Log.Comment("Make sure that basic enum-with-base-type declarations, definitions, and assignments work.");
            if (Enum_TestClass_enum02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum07_Test()
        {
            Log.Comment("Make sure that basic enum-with-base-type declarations, definitions, and assignments work.");
            if (Enum_TestClass_enum07.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum09_Test()
        {
            Log.Comment("Make sure that basic enum-with-base-type declarations, definitions, and assignments work.");
            if (Enum_TestClass_enum09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum10_Test()
        {
            Log.Comment("Make sure that basic enum-with-base-type declarations, definitions, and assignments work.");
            if (Enum_TestClass_enum10.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum11_Test()
        {
            Log.Comment("Make sure that basic enum-with-base-type declarations, definitions, and assignments work.");
            if (Enum_TestClass_enum11.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum27_Test()
        {
            Log.Comment("Check that enumerator values are initialized as expected");
            if (Enum_TestClass_enum27.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum28_Test()
        {
            Log.Comment("Check that enumerator values are initialized as expected");
            if (Enum_TestClass_enum28.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum29_Test()
        {
            Log.Comment("The values of the enumerators need not be distinct");
            if (Enum_TestClass_enum29.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum30_Test()
        {
            Log.Comment("Check the point of definition of an enumerator");
            if (Enum_TestClass_enum30.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum31_Test()
        {
            Log.Comment("Check the point of definition of an enumerator");
            if (Enum_TestClass_enum31.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum33_Test()
        {
            Log.Comment("Enums obey local scope rules.  An enum of the same name may be defined in an inner scope.");
            if (Enum_TestClass_enum33.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum34_Test()
        {
            Log.Comment("Enums can be converted to int.");
            if (Enum_TestClass_enum34.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum35_Test()
        {
            Log.Comment("If no enumerator-definitions with = appear, then the");
            Log.Comment(" values of the corresponding constants begin at zero and");
            if (Enum_TestClass_enum35.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum36_Test()
        {
            Log.Comment("If no enumerator-definitions with = appear, then the");
            Log.Comment(" values of the corresponding constants begin at zero and");
            if (Enum_TestClass_enum36.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum37_Test()
        {
            Log.Comment("If no enumerator-definitions with = appear, then the");
            Log.Comment(" values of the corresponding constants begin at zero and");
            if (Enum_TestClass_enum37.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum38_Test()
        {
            Log.Comment("Enums can be declared in any scopt that a class can be declared in.");
            if (Enum_TestClass_enum38.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum39_Test()
        {
            Log.Comment("If the constant-expression initilizing an enumerator is of integral type,");
            Log.Comment("it must be within the range of values that can be represented by the underlying type.");
            if (Enum_TestClass_enum39.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum40_Test()
        {
            Log.Comment("If the constant-expression initilizing an enumerator is of integral type,");
            Log.Comment("it must be within the range of values that can be represented by the underlying type.");
            if (Enum_TestClass_enum40.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum41_Test()
        {
            Log.Comment("If the constant-expression initilizing an enumerator is of integral type,");
            Log.Comment("it must be within the range of values that can be represented by the underlying type.");
            if (Enum_TestClass_enum41.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum42_Test()
        {
            Log.Comment("If the constant-expression initilizing an enumerator is of integral type,");
            Log.Comment("it must be within the range of values that can be represented by the underlying type.");
            if (Enum_TestClass_enum42.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum43_Test()
        {
            Log.Comment("If the constant-expression initilizing an enumerator is of integral type,");
            Log.Comment("it must be within the range of values that can be represented by the underlying type.");
            if (Enum_TestClass_enum43.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum43u_Test()
        {
            Log.Comment("If the constant-expression initilizing an enumerator is of integral type,");
            Log.Comment("it must be within the range of values that can be represented by the underlying type.");
            if (Enum_TestClass_enum43u.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum44_Test()
        {
            Log.Comment("If the constant-expression initilizing an enumerator is of integral type,");
            Log.Comment("it must be within the range of values that can be represented by the underlying type.");
            if (Enum_TestClass_enum44.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum45_Test()
        {
            Log.Comment("If the constant-expression initilizing an enumerator is of integral type,");
            Log.Comment("it must be within the range of values that can be represented by the underlying type.");
            if (Enum_TestClass_enum45.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum46_Test()
        {
            Log.Comment("If the constant-expression initilizing an enumerator is of integral type,");
            Log.Comment("it must be within the range of values that can be represented by the underlying type.");
            if (Enum_TestClass_enum46.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum46u_Test()
        {
            Log.Comment("If the constant-expression initilizing an enumerator is of integral type,");
            Log.Comment("it must be within the range of values that can be represented by the underlying type.");
            if (Enum_TestClass_enum46u.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum47_Test()
        {
            Log.Comment("If the constant-expression initilizing an enumerator is of integral type,");
            Log.Comment("it must be within the range of values that can be represented by the underlying type.");
            if (Enum_TestClass_enum47.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum47u_Test()
        {
            Log.Comment("If the constant-expression initilizing an enumerator is of integral type,");
            Log.Comment("it must be within the range of values that can be represented by the underlying type.");
            if (Enum_TestClass_enum47u.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum48_Test()
        {
            Log.Comment("If the constant-expression initilizing an enumerator is of integral type,");
            Log.Comment("it must be within the range of values that can be represented by the underlying type.");
            if (Enum_TestClass_enum48.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum48u_Test()
        {
            Log.Comment("If the constant-expression initilizing an enumerator is of integral type,");
            Log.Comment("it must be within the range of values that can be represented by the underlying type.");
            if (Enum_TestClass_enum48u.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum54_Test()
        {
            Log.Comment("++ and -- operators can be used with objects of enumeration type.  Check postfix form.");
            if (Enum_TestClass_enum54.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum55_Test()
        {
            Log.Comment("++ and -- operators can be used with objects of enumeration type.  Check postfix form.");
            if (Enum_TestClass_enum55.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum56_Test()
        {
            Log.Comment("++ and -- operators can be used with objects of enumeration type.  Check postfix form.");
            if (Enum_TestClass_enum56.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum57_Test()
        {
            Log.Comment("++ and -- operators can be used with objects of enumeration type.  Check postfix form.");
            if (Enum_TestClass_enum57.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum58_Test()
        {
            Log.Comment("Bitwise operators AND, OR, XOR, and NOT can be used with objects of enumeration type.");

            if (Enum_TestClass_enum58.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum62_Test()
        {
            if (Enum_TestClass_enum62.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum63_Test()
        {
            Log.Comment("Make sure that a basic enum declaration, definition, and assignment work.");
            if (Enum_TestClass_enum63.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum64_Test()
        {
            Log.Comment("Make sure that a basic enum declaration, definition, and assignment work.");
            if (Enum_TestClass_enum64.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum65_Test()
        {
            Log.Comment("Make sure that a basic enum declaration, definition, and assignment work.");
            if (Enum_TestClass_enum65.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum66_Test()
        {
            Log.Comment("Make sure that a basic enum declaration, definition, and assignment work.");
            if (Enum_TestClass_enum66.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum67_Test()
        {
            Log.Comment("Make sure that a basic enum declaration, definition, and assignment work.");
            if (Enum_TestClass_enum67.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum68_Test()
        {
            Log.Comment("Make sure that a basic enum declaration, definition, and assignment work.");
            if (Enum_TestClass_enum68.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum69_Test()
        {
            Log.Comment("Make sure that a basic enum declaration, definition, and assignment work.");
            if (Enum_TestClass_enum69.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum70_Test()
        {
            Log.Comment("Make sure that a basic enum declaration, definition, and assignment work.");
            if (Enum_TestClass_enum70.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum71_Test()
        {
            Log.Comment("Make sure that a basic enum declaration, definition, and assignment work.");
            Log.Comment("This test is expeced to fail");
            if (Enum_TestClass_enum71.testMethod())
            {
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;
        }
        [TestMethod]
        public MFTestResults Enum_enum72_Test()
        {
            Log.Comment("Enum_TestClass_? bitwise and on enums");
            if (Enum_TestClass_enum72.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum73_Test()
        {
            Log.Comment("Enum_TestClass_? bitwise or on enums");
            if (Enum_TestClass_enum73.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum74_Test()
        {
            Log.Comment("Enum_TestClass_? bitwise xor on enums");
            if (Enum_TestClass_enum74.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum75_Test()
        {
            Log.Comment("Enum_TestClass_? bitwise not on enums");
            if (Enum_TestClass_enum75.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum77_Test()
        {
            Log.Comment("Enum_TestClass_? bitwise not on enums");
            if (Enum_TestClass_enum77.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum78_Test()
        {
            if (Enum_TestClass_enum78.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum83_Test()
        {
            Log.Comment("Enum member list can end with a comma");
            if (Enum_TestClass_enum83.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum86_Test()
        {
            Log.Comment("[Access] modifiers of an enum declaration have the same meaning");
            Log.Comment("as those of a class declaration.");
            if (Enum_TestClass_enum86.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum93_Test()
        {
            Log.Comment("Example from Enums chapter in CLS");
            if (Enum_TestClass_enum93.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum94_Test()
        {
            Log.Comment("...any value of the underlying type of an enum can be cast to the enum type.");
            if (Enum_TestClass_enum94.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum_flags01_Test()
        {
            Log.Comment("check FlagAttribute with enum");
            if (Enum_TestClass_enum_flags01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum_flags02_Test()
        {
            Log.Comment("check FlagAttribute with enum");
            if (Enum_TestClass_enum_flags02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum_flags03_Test()
        {
            Log.Comment("check FlagAttribute with enum");
            if (Enum_TestClass_enum_flags03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Enum_enum_flags04_Test()
        {
            Log.Comment("check FlagAttribute with enum with conversion");
            if (Enum_TestClass_enum_flags04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        //Compiled Test Cases 
        public class Enum_TestClass_enum01
        {
            public enum e1 { one = 1, two = 2, three = 3 };
            public static int Main_old()
            {
                e1 v_e1 = e1.two;
                Log.Comment("v_e1 == ");
                Log.Comment(v_e1.ToString());
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };
        public class Enum_TestClass_enum02
        {
            public enum e1 : long { one = 1, two = 2, three = 3 };
            public enum e2 : int { one = 1, two = 2, three = 3 };
            public enum e3 : short { one = 1, two = 2, three = 3 };
            public enum e4 : byte { one = 1, two = 2, three = 3 };
            public static int Main_old()
            {
                e1 v_e1 = e1.two;
                e2 v_e2 = e2.two;
                e3 v_e3 = e3.two;
                e4 v_e4 = e4.two;
                Log.Comment("v_e1 == ");
                Log.Comment(v_e1.ToString());
                Log.Comment("v_e2 == ");
                Log.Comment(v_e2.ToString());
                Log.Comment("v_e3 == ");
                Log.Comment(v_e3.ToString());
                Log.Comment("v_e4 == ");
                Log.Comment(v_e4.ToString());
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public class Enum_TestClass_enum07
        {
            public enum e1 { one = 1, two = 2, three = 3 };
            public static int Main_old()
            {
                int i = (int)e1.two;
                Log.Comment("i == ");
                Log.Comment(i.ToString());
                return i - 2;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };


        public class Enum_TestClass_enum09
        {
            public enum e1 : int { one = 1, two = 2, three = 3 };
            public static int Main_old()
            {
                int i = (int)e1.two;
                Log.Comment("i == ");
                Log.Comment(i.ToString());
                return i - 2;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };


        public class Enum_TestClass_enum10
        {
            public enum e1 : short { one = 1, two = 2, three = 3 };
            public static int Main_old()
            {
                int i = (short)e1.two;
                Log.Comment("i == ");
                Log.Comment(i.ToString());
                return i - 2;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };


        public class Enum_TestClass_enum11
        {
            public enum e1 : byte { one = 1, two = 2, three = 3 };
            public static int Main_old()
            {
                int i = (byte)e1.two;
                Log.Comment("i == ");
                Log.Comment(i.ToString());
                return i - 2;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public class Enum_TestClass_enum27
        {
            public static int retval = 0;

            public enum Enum_TestClass_enum27_Enum1 { zero, one, three = 3, four, minus7 = -7, minus6 };
            public static int Main_old()
            {
                if ((int)Enum_TestClass_enum27_Enum1.zero != 0)
                {
                    Log.Comment("Enumerator zero = ");
                    Log.Comment((Enum_TestClass_enum27_Enum1.zero).ToString());
                    retval |= 0x01;
                }

                if ((int)Enum_TestClass_enum27_Enum1.one != 1)
                {
                    Log.Comment("Enumerator one = ");
                    Log.Comment((Enum_TestClass_enum27_Enum1.one).ToString());
                    retval |= 0x02;
                }

                if ((int)Enum_TestClass_enum27_Enum1.three != 3)
                {
                    Log.Comment("Enumerator three = ");
                    Log.Comment((Enum_TestClass_enum27_Enum1.three).ToString());
                    retval |= 0x04;
                }

                if ((int)Enum_TestClass_enum27_Enum1.four != 4)
                {
                    Log.Comment("Enumerator four = ");
                    Log.Comment((Enum_TestClass_enum27_Enum1.four).ToString());
                    retval |= 0x08;
                }

                if ((int)Enum_TestClass_enum27_Enum1.minus7 != -7)
                {
                    Log.Comment("Enumerator minus7 = ");
                    Log.Comment((Enum_TestClass_enum27_Enum1.minus7).ToString());
                    retval |= 0x10;
                }

                if ((int)Enum_TestClass_enum27_Enum1.minus6 != -6)
                {
                    Log.Comment("Enumerator minus6 = ");
                    Log.Comment((Enum_TestClass_enum27_Enum1.minus6).ToString());
                    retval |= 0x20;
                }
                if (retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public class Enum_TestClass_enum28
        {
            public static int retval = 0;

            public enum Enum_TestClass_enum28_Enum1 { zero, one, three = 3, four, minus7 = -7, minus6 };
            public static int Main_old()
            {
                if (!((Enum_TestClass_enum28_Enum1)Enum_TestClass_enum28_Enum1.zero == (Enum_TestClass_enum28_Enum1)0))
                {
                    Log.Comment("Enumerator zero = ");
                    Log.Comment((Enum_TestClass_enum28_Enum1.zero).ToString());
                    retval |= 0x01;
                }

                if (Enum_TestClass_enum28_Enum1.one != (Enum_TestClass_enum28_Enum1)1)
                {
                    Log.Comment("Enumerator one = ");
                    Log.Comment((Enum_TestClass_enum28_Enum1.one).ToString());
                    retval |= 0x02;
                }

                if (Enum_TestClass_enum28_Enum1.three != (Enum_TestClass_enum28_Enum1)3)
                {
                    Log.Comment("Enumerator three = ");
                    Log.Comment((Enum_TestClass_enum28_Enum1.three).ToString());
                    retval |= 0x04;
                }

                if (Enum_TestClass_enum28_Enum1.four != (Enum_TestClass_enum28_Enum1)4)
                {
                    Log.Comment("Enumerator four = ");
                    Log.Comment((Enum_TestClass_enum28_Enum1.four).ToString());
                    retval |= 0x08;
                }

                if (Enum_TestClass_enum28_Enum1.minus7 != (Enum_TestClass_enum28_Enum1)(-7))
                {
                    Log.Comment("Enumerator minus7 = ");
                    Log.Comment((Enum_TestClass_enum28_Enum1.minus7).ToString());
                    retval |= 0x10;
                }

                if (Enum_TestClass_enum28_Enum1.minus6 != (Enum_TestClass_enum28_Enum1)(-6))
                {
                    Log.Comment("Enumerator minus6 = ");
                    Log.Comment((Enum_TestClass_enum28_Enum1.minus6).ToString());
                    retval |= 0x20;
                }
                if (retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public class Enum_TestClass_enum29
        {
            public static int retval = 0;

            public enum Enum_TestClass_enum29_Enum1 { zero = 0, one = 0 };
            public static int Main_old()
            {
                if (Enum_TestClass_enum29_Enum1.one != Enum_TestClass_enum29_Enum1.zero)
                {
                    Log.Comment("Enumerator zero and one not synonymous");
                    retval = 1;
                }

                if (retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public class Enum_TestClass_enum30
        {
            public static int retval = 0;

            public enum Enum_TestClass_enum30_Enum1 { zero, three = (int)zero + 3 };
            public static int Main_old()
            {
                if ((int)Enum_TestClass_enum30_Enum1.three != 3)
                {
                    Log.Comment("Enumerator zero and one not synonymous");
                    retval = 1;
                }

                if (retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public class Enum_TestClass_enum31
        {
            public static int retval = 0;

            public enum Enum_TestClass_enum31_Enum1 { };
            public static int Main_old()
            {
                Log.Comment("PASS");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };
        enum Enum_TestClass_enum33_Enum1 { yes = 1, no = yes - 1 };

        public class Enum_TestClass_enum33
        {
            public static int retval = 0;

            public enum Enum_TestClass_enum33_Enum1 { yes = 1, no = yes - 1 };
            public static int Main_old()
            {
                Log.Comment("PASS");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public class Enum_TestClass_enum34
        {
            public static int retval = 0;
            public enum color { red, yellow, green = 20, blue };

            public static int Main_old()
            {
                int i = (int)color.yellow; //ok: yellow converted to int value 1
                //by integral promotion"
                Log.Comment("PASS");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public class Enum_TestClass_enum35
        {
            public static int retval = 0x3F;
            public enum E { a, b, c, d = 7, e, f = 3 };

            public static int Main_old()
            {
                if ((int)E.a == 0)
                    retval -= 0x20;
                if ((int)E.b == 1)
                    retval -= 0x10;
                if ((int)E.c == 2)
                    retval -= 0x08;
                if ((int)E.d == 7)
                    retval -= 0x04;
                if ((int)E.e == 8)
                    retval -= 0x02;
                if ((int)E.f == 3)
                    retval -= 0x01;

                if (retval == 0) Log.Comment("PASS");
                else
                {
                    Log.Comment("FAIL\nretval == ");
                    Log.Comment((retval).ToString());
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public class Enum_TestClass_enum36_Enum
        {
            public enum E { a, b, c, d = 7, e, f = 3 };
        }

        public class Enum_TestClass_enum36
        {
            public static int retval = 0x3F;

            public static int Main_old()
            {
                if ((int)Enum_TestClass_enum36_Enum.E.a == 0)
                    retval -= 0x20;
                if ((int)Enum_TestClass_enum36_Enum.E.b == 1)
                    retval -= 0x10;
                if ((int)Enum_TestClass_enum36_Enum.E.c == 2)
                    retval -= 0x08;
                if ((int)Enum_TestClass_enum36_Enum.E.d == 7)
                    retval -= 0x04;
                if ((int)Enum_TestClass_enum36_Enum.E.e == 8)
                    retval -= 0x02;
                if ((int)Enum_TestClass_enum36_Enum.E.f == 3)
                    retval -= 0x01;

                if (retval == 0) Log.Comment("PASS");
                else
                {
                    Log.Comment("FAIL\nretval == ");
                    Log.Comment((retval).ToString());
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public class Enum_TestClass_enum37_Enum
        {
            public enum E { a, b, c, d = 7, e, f = 3 };
        }

        public class Enum_TestClass_enum37
        {
            public static int retval = 0x3F;

            public static int Main_old()
            {
                Enum_TestClass_enum37_Enum f = new Enum_TestClass_enum37_Enum();

                if ((int)Enum_TestClass_enum37_Enum.E.a == 0)
                    retval -= 0x20;
                if ((int)Enum_TestClass_enum37_Enum.E.b == 1)
                    retval -= 0x10;
                if ((int)Enum_TestClass_enum37_Enum.E.c == 2)
                    retval -= 0x08;
                if ((int)Enum_TestClass_enum37_Enum.E.d == 7)
                    retval -= 0x04;
                if ((int)Enum_TestClass_enum37_Enum.E.e == 8)
                    retval -= 0x02;
                if ((int)Enum_TestClass_enum37_Enum.E.f == 3)
                    retval -= 0x01;

                if (retval == 0) Log.Comment("PASS");
                else
                {
                    Log.Comment("FAIL\nretval == ");
                    Log.Comment((retval).ToString());
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public enum Enum_TestClass_enum38_Enum1 { a, b, c, d = 7, e, f = 3 };
        public class Enum_TestClass_enum38_Enum
        {
            public enum E { a, b, c, d = 7, e, f = 3 };
        }

        public class Enum_TestClass_enum38
        {
            public static int retval = 0x3F;

            public static int Main_old()
            {
                if ((int)Enum_TestClass_enum38_Enum1.a == 0)
                    retval -= 0x20;
                if ((int)Enum_TestClass_enum38_Enum1.b == 1)
                    retval -= 0x10;
                if ((int)Enum_TestClass_enum38_Enum1.c == 2)
                    retval -= 0x08;
                if ((int)Enum_TestClass_enum38_Enum1.d == 7)
                    retval -= 0x04;
                if ((int)Enum_TestClass_enum38_Enum1.e == 8)
                    retval -= 0x02;
                if ((int)Enum_TestClass_enum38_Enum1.f == 3)
                    retval -= 0x01;

                if (retval == 0) Log.Comment("PASS");
                else
                {
                    Log.Comment("FAIL\nretval == ");
                    Log.Comment((retval).ToString());
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public enum Enum_TestClass_enum39_Enum1 : byte { a, b, c, d = 253, e, f };
        public class Enum_TestClass_enum39
        {
            public static int retval = 0x3F;

            public static int Main_old()
            {
                if ((int)Enum_TestClass_enum39_Enum1.a == 0)
                    retval -= 0x20;
                if ((int)Enum_TestClass_enum39_Enum1.b == 1)
                    retval -= 0x10;
                if ((int)Enum_TestClass_enum39_Enum1.c == 2)
                    retval -= 0x08;
                if ((int)Enum_TestClass_enum39_Enum1.d == 253)
                    retval -= 0x04;
                if ((int)Enum_TestClass_enum39_Enum1.e == 254)
                    retval -= 0x02;
                if ((int)Enum_TestClass_enum39_Enum1.f == 255)
                    retval -= 0x01;

                if (retval == 0) Log.Comment("PASS");
                else
                {
                    Log.Comment("FAIL\nretval == ");
                    Log.Comment((retval).ToString());
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public enum Enum_TestClass_enum40_Enum1 : short { a, b, c, d = 32765, e, f };
        public class Enum_TestClass_enum40
        {
            public static int retval = 0x3F;

            public static int Main_old()
            {
                if ((int)Enum_TestClass_enum40_Enum1.a == 0)
                    retval -= 0x20;
                if ((int)Enum_TestClass_enum40_Enum1.b == 1)
                    retval -= 0x10;
                if ((int)Enum_TestClass_enum40_Enum1.c == 2)
                    retval -= 0x08;
                if ((int)Enum_TestClass_enum40_Enum1.d == 32765)
                    retval -= 0x04;
                if ((int)Enum_TestClass_enum40_Enum1.e == 32766)
                    retval -= 0x02;
                if ((int)Enum_TestClass_enum40_Enum1.f == 32767)
                    retval -= 0x01;

                if (retval == 0) Log.Comment("PASS");
                else
                {
                    Log.Comment("FAIL\nretval == ");
                    Log.Comment((retval).ToString());
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public enum Enum_TestClass_enum41_Enum1 : int { a, b, c, d = (int)0x7FFFFFFD, e, f };
        public class Enum_TestClass_enum41
        {
            public static int retval = 0x3F;

            public static int Main_old()
            {
                if ((int)Enum_TestClass_enum41_Enum1.a == 0)
                    retval -= 0x20;
                if ((int)Enum_TestClass_enum41_Enum1.b == 1)
                    retval -= 0x10;
                if ((int)Enum_TestClass_enum41_Enum1.c == 2)
                    retval -= 0x08;
                if ((int)Enum_TestClass_enum41_Enum1.d == 0x7FFFFFFD)
                    retval -= 0x04;
                if ((int)Enum_TestClass_enum41_Enum1.e == 0x7FFFFFFE)
                    retval -= 0x02;
                if ((int)Enum_TestClass_enum41_Enum1.f == 0x7FFFFFFF)
                    retval -= 0x01;

                if (retval == 0) Log.Comment("PASS");
                else
                {
                    Log.Comment("FAIL\nretval == ");
                    Log.Comment((retval).ToString());
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public enum Enum_TestClass_enum42_Enum1 : long { a, b, c, d = (long)0x7FFFFFFFFFFFFFFDL, e, f };
        public class Enum_TestClass_enum42
        {
            public static int retval = 0x3F;

            public static int Main_old()
            {
                if ((long)Enum_TestClass_enum42_Enum1.a == 0)
                    retval -= 0x20;
                if ((long)Enum_TestClass_enum42_Enum1.b == 1)
                    retval -= 0x10;
                if ((long)Enum_TestClass_enum42_Enum1.c == 2)
                    retval -= 0x08;
                if ((long)Enum_TestClass_enum42_Enum1.d == 0x7FFFFFFFFFFFFFFD)
                    retval -= 0x04;
                if ((long)Enum_TestClass_enum42_Enum1.e == 0x7FFFFFFFFFFFFFFE)
                    retval -= 0x02;
                if ((long)Enum_TestClass_enum42_Enum1.f == 0x7FFFFFFFFFFFFFFF)
                    retval -= 0x01;

                if (retval == 0) Log.Comment("PASS");
                else
                {
                    Log.Comment("FAIL\nretval == ");
                    Log.Comment((retval).ToString());
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public enum Enum_TestClass_enum43_Enum1 : int { a, b, c, d = (int)0x7FFFFFFD, e, f };
        public enum Enum_TestClass_enum43_Enum2 : short { a = (short)Enum_TestClass_enum43_Enum1.a, b, c, d, e, f };
        public class Enum_TestClass_enum43
        {
            public static int retval = 0x3F;

            public static int Main_old()
            {
                if ((int)Enum_TestClass_enum43_Enum2.a == 0)
                    retval -= 0x20;
                if ((int)Enum_TestClass_enum43_Enum2.b == 1)
                    retval -= 0x10;
                if ((int)Enum_TestClass_enum43_Enum2.c == 2)
                    retval -= 0x08;
                if ((int)Enum_TestClass_enum43_Enum2.d == 3)
                    retval -= 0x04;
                if ((int)Enum_TestClass_enum43_Enum2.e == 4)
                    retval -= 0x02;
                if ((int)Enum_TestClass_enum43_Enum2.f == 5)
                    retval -= 0x01;

                if (retval == 0) Log.Comment("PASS");
                else
                {
                    Log.Comment("FAIL\nretval == ");
                    Log.Comment((retval).ToString());
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public enum Enum_TestClass_enum43u_Enum1 : int { a, b, c, d = (int)0x7FFFFFFD, e, f };
        public enum Enum_TestClass_enum43u_Enum2 : ushort { a = (ushort)Enum_TestClass_enum43u_Enum1.a, b, c, d, e, f };
        public class Enum_TestClass_enum43u
        {
            public static int retval = 0x3F;

            public static int Main_old()
            {
                if ((int)Enum_TestClass_enum43u_Enum2.a == 0)
                    retval -= 0x20;
                if ((int)Enum_TestClass_enum43u_Enum2.b == 1)
                    retval -= 0x10;
                if ((int)Enum_TestClass_enum43u_Enum2.c == 2)
                    retval -= 0x08;
                if ((int)Enum_TestClass_enum43u_Enum2.d == 3)
                    retval -= 0x04;
                if ((int)Enum_TestClass_enum43u_Enum2.e == 4)
                    retval -= 0x02;
                if ((int)Enum_TestClass_enum43u_Enum2.f == 5)
                    retval -= 0x01;

                if (retval == 0) Log.Comment("PASS");
                else
                {
                    Log.Comment("FAIL\nretval == ");
                    Log.Comment((retval).ToString());
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public enum Enum_TestClass_enum44_Enum1 : int { a, b, c, d = (int)0x7FFFFFFD, e, f };
        public enum Enum_TestClass_enum44_Enum2 : byte { a = Enum_TestClass_enum44_Enum1.a, b, c, d, e, f };
        public class Enum_TestClass_enum44
        {
            public static int retval = 0x3F;

            public static int Main_old()
            {
                if ((int)Enum_TestClass_enum44_Enum2.a == 0)
                    retval -= 0x20;
                if ((int)Enum_TestClass_enum44_Enum2.b == 1)
                    retval -= 0x10;
                if ((int)Enum_TestClass_enum44_Enum2.c == 2)
                    retval -= 0x08;
                if ((int)Enum_TestClass_enum44_Enum2.d == 3)
                    retval -= 0x04;
                if ((int)Enum_TestClass_enum44_Enum2.e == 4)
                    retval -= 0x02;
                if ((int)Enum_TestClass_enum44_Enum2.f == 5)
                    retval -= 0x01;

                if (retval == 0) Log.Comment("PASS");
                else
                {
                    Log.Comment("FAIL\nretval == ");
                    Log.Comment((retval).ToString());
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public enum Enum_TestClass_enum45_Enum1 : long { a, b, c, d = (long)0x7FFFFFFFFFFFFFFDL, e, f };
        public enum Enum_TestClass_enum45_Enum2 : int { a = (int)Enum_TestClass_enum45_Enum1.a, b, c, d, e, f };
        public class Enum_TestClass_enum45
        {
            public static int retval = 0x3F;

            public static int Main_old()
            {
                if ((int)Enum_TestClass_enum45_Enum2.a == 0)
                    retval -= 0x20;
                if ((int)Enum_TestClass_enum45_Enum2.b == 1)
                    retval -= 0x10;
                if ((int)Enum_TestClass_enum45_Enum2.c == 2)
                    retval -= 0x08;
                if ((int)Enum_TestClass_enum45_Enum2.d == 3)
                    retval -= 0x04;
                if ((int)Enum_TestClass_enum45_Enum2.e == 4)
                    retval -= 0x02;
                if ((int)Enum_TestClass_enum45_Enum2.f == 5)
                    retval -= 0x01;

                if (retval == 0) Log.Comment("PASS");
                else
                {
                    Log.Comment("FAIL\nretval == ");
                    Log.Comment((retval).ToString());
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public enum Enum_TestClass_enum46_Enum2 : short { a = Enum_TestClass_enum46_Enum1.a, b, c, d, e, f };
        public enum Enum_TestClass_enum46_Enum1 : int { a, b, c, d = (int)0x7FFFFFFD, e, f };
        public class Enum_TestClass_enum46
        {
            public static int retval = 0x3F;

            public static int Main_old()
            {
                if ((int)Enum_TestClass_enum46_Enum2.a == 0)
                    retval -= 0x20;
                if ((int)Enum_TestClass_enum46_Enum2.b == 1)
                    retval -= 0x10;
                if ((int)Enum_TestClass_enum46_Enum2.c == 2)
                    retval -= 0x08;
                if ((int)Enum_TestClass_enum46_Enum2.d == 3)
                    retval -= 0x04;
                if ((int)Enum_TestClass_enum46_Enum2.e == 4)
                    retval -= 0x02;
                if ((int)Enum_TestClass_enum46_Enum2.f == 5)
                    retval -= 0x01;

                if (retval == 0) Log.Comment("PASS");
                else
                {
                    Log.Comment("FAIL\nretval == ");
                    Log.Comment((retval).ToString());
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public enum Enum_TestClass_enum46u_Enum1 : uint { a, b, c, d = 0xFFFFFFFD, e, f };
        public class Enum_TestClass_enum46u
        {
            public static int retval = 0x3F;

            public static int Main_old()
            {
                if ((int)Enum_TestClass_enum46u_Enum1.a == 0)
                    retval -= 0x20;
                if ((int)Enum_TestClass_enum46u_Enum1.b == 1)
                    retval -= 0x10;
                if ((int)Enum_TestClass_enum46u_Enum1.c == 2)
                    retval -= 0x08;
                if ((uint)Enum_TestClass_enum46u_Enum1.d == 0xFFFFFFFD)
                    retval -= 0x04;
                if ((uint)Enum_TestClass_enum46u_Enum1.e == 0xFFFFFFFE)
                    retval -= 0x02;
                if ((uint)Enum_TestClass_enum46u_Enum1.f == 0xFFFFFFFF)
                    retval -= 0x01;

                if (retval == 0) Log.Comment("PASS");
                else
                {
                    Log.Comment("FAIL\nretval == ");
                    Log.Comment((retval).ToString());
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public enum Enum_TestClass_enum47_Enum2 : byte { a = Enum_TestClass_enum47_Enum1.a, b, c, d, e, f };
        public enum Enum_TestClass_enum47_Enum1 : int { a, b, c, d = (int)0x7FFFFFFD, e, f };
        public class Enum_TestClass_enum47
        {
            public static int retval = 0x3F;

            public static int Main_old()
            {
                if ((int)Enum_TestClass_enum47_Enum2.a == 0)
                    retval -= 0x20;
                if ((int)Enum_TestClass_enum47_Enum2.b == 1)
                    retval -= 0x10;
                if ((int)Enum_TestClass_enum47_Enum2.c == 2)
                    retval -= 0x08;
                if ((int)Enum_TestClass_enum47_Enum2.d == 3)
                    retval -= 0x04;
                if ((int)Enum_TestClass_enum47_Enum2.e == 4)
                    retval -= 0x02;
                if ((int)Enum_TestClass_enum47_Enum2.f == 5)
                    retval -= 0x01;

                if (retval == 0) Log.Comment("PASS");
                else
                {
                    Log.Comment("FAIL\nretval == ");
                    Log.Comment((retval).ToString());
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public enum Enum_TestClass_enum47u_Enum2 : sbyte { a = (sbyte)Enum_TestClass_enum47u_Enum1.a, b, c, d, e, f };
        public enum Enum_TestClass_enum47u_Enum1 : int { a, b, c, d = 0x7FFFFFFD, e, f };
        public class Enum_TestClass_enum47u
        {
            public static int retval = 0x3F;

            public static int Main_old()
            {
                if ((int)Enum_TestClass_enum47u_Enum2.a == 0)
                    retval -= 0x20;
                if ((int)Enum_TestClass_enum47u_Enum2.b == 1)
                    retval -= 0x10;
                if ((int)Enum_TestClass_enum47u_Enum2.c == 2)
                    retval -= 0x08;
                if ((int)Enum_TestClass_enum47u_Enum2.d == 3)
                    retval -= 0x04;
                if ((int)Enum_TestClass_enum47u_Enum2.e == 4)
                    retval -= 0x02;
                if ((int)Enum_TestClass_enum47u_Enum2.f == 5)
                    retval -= 0x01;

                if (retval == 0) Log.Comment("PASS");
                else
                {
                    Log.Comment("FAIL\nretval == ");
                    Log.Comment((retval).ToString());
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public enum Enum_TestClass_enum48_Enum2 : int { a = (int)Enum_TestClass_enum48_Enum1.a, b, c, d, e, f };
        public enum Enum_TestClass_enum48_Enum1 : long { a, b, c, d = (long)0x7FFFFFFFFFFFFFFDL, e, f };
        public class Enum_TestClass_enum48
        {
            public static int retval = 0x3F;

            public static int Main_old()
            {
                if ((int)Enum_TestClass_enum48_Enum2.a == 0)
                    retval -= 0x20;
                if ((int)Enum_TestClass_enum48_Enum2.b == 1)
                    retval -= 0x10;
                if ((int)Enum_TestClass_enum48_Enum2.c == 2)
                    retval -= 0x08;
                if ((int)Enum_TestClass_enum48_Enum2.d == 3)
                    retval -= 0x04;
                if ((int)Enum_TestClass_enum48_Enum2.e == 4)
                    retval -= 0x02;
                if ((int)Enum_TestClass_enum48_Enum2.f == 5)
                    retval -= 0x01;

                if (retval == 0) Log.Comment("PASS");
                else
                {
                    Log.Comment("FAIL\nretval == ");
                    Log.Comment((retval).ToString());
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public enum Enum_TestClass_enum48u_Enum2 : int { a = (int)Enum_TestClass_enum48u_Enum1.a, b, c, d, e, f };
        public enum Enum_TestClass_enum48u_Enum1 : ulong { a, b, c, d = 0xFFFFFFFFFFFFFFFDL, e, f };
        public class Enum_TestClass_enum48u
        {
            public static int retval = 0x3F;

            public static int Main_old()
            {
                if ((ulong)Enum_TestClass_enum48u_Enum1.a == 0)
                    retval -= 0x20;
                if ((ulong)Enum_TestClass_enum48u_Enum1.b == 1)
                    retval -= 0x10;
                if ((ulong)Enum_TestClass_enum48u_Enum1.c == 2)
                    retval -= 0x08;
                if ((ulong)Enum_TestClass_enum48u_Enum1.d == 0xFFFFFFFFFFFFFFFDL)
                    retval -= 0x04;
                if ((ulong)Enum_TestClass_enum48u_Enum1.e == 0xFFFFFFFFFFFFFFFEL)
                    retval -= 0x02;
                if ((ulong)Enum_TestClass_enum48u_Enum1.f == 0xFFFFFFFFFFFFFFFFL)
                    retval -= 0x01;

                if (retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL\nretval == " + retval.ToString());
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public enum Enum_TestClass_enum54_Enum1 { a, b, c, d, e, f };
        public enum Enum_TestClass_enum54_Enum2 : byte { a, b, c, d, e, f };
        public enum Enum_TestClass_enum54_Enum2sb : sbyte { a, b, c, d, e, f };
        public enum Enum_TestClass_enum54_Enum3 : short { a, b, c, d, e, f };
        public enum Enum_TestClass_enum54_Enum3us : ushort { a, b, c, d, e, f };
        public enum Enum_TestClass_enum54_Enum4 : int { a, b, c, d, e, f };
        public enum Enum_TestClass_enum54_Enum4ui : uint { a, b, c, d, e, f };
        public enum Enum_TestClass_enum54_Enum5 : long { a, b, c, d, e, f };
        public enum Enum_TestClass_enum54_Enum5ul : ulong { a, b, c, d, e, f };
        public class Enum_TestClass_enum54
        {
            public static int retval = 0x7FFFFFF;

            public static int Main_old()
            {
                Enum_TestClass_enum54_Enum1 e1 = Enum_TestClass_enum54_Enum1.a;
                e1++; e1++; e1++; e1--; e1--; e1--; e1++; e1--; e1--; e1++; e1++; e1++; e1++; e1++; e1++; e1++;
                if ((int)e1 == (int)Enum_TestClass_enum54_Enum1.f + 1)
                    retval -= 0x01;
                e1--; e1--; e1--; e1--; e1--; e1--;
                if (e1 == Enum_TestClass_enum54_Enum1.a)
                    retval -= 0x02;
                e1--;
                if ((int)e1 == (int)Enum_TestClass_enum54_Enum1.a - 1)
                    retval -= 0x04;

                Enum_TestClass_enum54_Enum2 e2 = Enum_TestClass_enum54_Enum2.a;
                e2++; e2++; e2++; e2--; e2--; e2--; e2++; e2--; e2--; e2++; e2++; e2++; e2++; e2++; e2++; e2++;
                if ((int)e2 == (int)Enum_TestClass_enum54_Enum2.f + 1)
                    retval -= 0x08;
                e2--; e2--; e2--; e2--; e2--; e2--;
                if (e2 == Enum_TestClass_enum54_Enum2.a)
                    retval -= 0x10;
                e2--;
                if ((int)e2 == 255)  //This has to be a manifest constant because byte is UNSIGNED
                    retval -= 0x20;

                Enum_TestClass_enum54_Enum2sb e2sb = Enum_TestClass_enum54_Enum2sb.a;
                e2sb++; e2sb++; e2sb++; e2sb--; e2sb--; e2sb--; e2sb++; e2sb--; e2sb--; e2sb++; e2sb++; e2sb++; e2sb++; e2sb++; e2sb++; e2sb++;
                if ((int)e2sb == (int)Enum_TestClass_enum54_Enum2sb.f + 1)
                    retval -= 0x8000;
                e2sb--; e2sb--; e2sb--; e2sb--; e2sb--; e2sb--;
                if (e2sb == Enum_TestClass_enum54_Enum2sb.a)
                    retval -= 0x10000;
                e2sb--;
                if ((int)e2sb == -1)
                    retval -= 0x20000;

                Enum_TestClass_enum54_Enum3 e3 = Enum_TestClass_enum54_Enum3.a;
                e3++; e3++; e3++; e3--; e3--; e3--; e3++; e3--; e3--; e3++; e3++; e3++; e3++; e3++; e3++; e3++;
                if ((int)e3 == (int)Enum_TestClass_enum54_Enum3.f + 1)
                    retval -= 0x40;
                e3--; e3--; e3--; e3--; e3--; e3--;
                if (e3 == Enum_TestClass_enum54_Enum3.a)
                    retval -= 0x80;
                e3--;
                if ((int)e3 == (int)Enum_TestClass_enum54_Enum3.a - 1)
                    retval -= 0x100;

                Enum_TestClass_enum54_Enum3us e3us = Enum_TestClass_enum54_Enum3us.a;
                e3us++; e3us++; e3us++; e3us--; e3us--; e3us--; e3us++; e3us--; e3us--; e3us++; e3us++; e3us++; e3us++; e3us++; e3us++; e3us++;
                if ((int)e3us == (int)Enum_TestClass_enum54_Enum3us.f + 1)
                    retval -= 0x40000;
                e3us--; e3us--; e3us--; e3us--; e3us--; e3us--;
                if (e3us == Enum_TestClass_enum54_Enum3us.a)
                    retval -= 0x80000;
                e3us--;
                if ((int)e3us == 65535)
                    retval -= 0x100000;

                Enum_TestClass_enum54_Enum4 e4 = Enum_TestClass_enum54_Enum4.a;
                e4++; e4++; e4++; e4--; e4--; e4--; e4++; e4--; e4--; e4++; e4++; e4++; e4++; e4++; e4++; e4++;
                if ((int)e4 == (int)Enum_TestClass_enum54_Enum4.f + 1)
                    retval -= 0x200;
                e4--; e4--; e4--; e4--; e4--; e4--;
                if (e4 == Enum_TestClass_enum54_Enum4.a)
                    retval -= 0x400;
                e4--;
                if ((int)e4 == (int)Enum_TestClass_enum54_Enum4.a - 1)
                    retval -= 0x800;

                Enum_TestClass_enum54_Enum4ui e4ui = Enum_TestClass_enum54_Enum4ui.a;
                e4ui++; e4ui++; e4ui++; e4ui--; e4ui--; e4ui--; e4ui++; e4ui--; e4ui--; e4ui++; e4ui++; e4ui++; e4ui++; e4ui++; e4ui++; e4ui++;
                if ((int)e4ui == (int)Enum_TestClass_enum54_Enum4ui.f + 1)
                    retval -= 0x200000;
                e4ui--; e4ui--; e4ui--; e4ui--; e4ui--; e4ui--;
                if (e4ui == Enum_TestClass_enum54_Enum4ui.a)
                    retval -= 0x400000;
                e4ui--;
                if ((int)e4ui == (int)Enum_TestClass_enum54_Enum4ui.a - 1)
                    retval -= 0x800000;

                Enum_TestClass_enum54_Enum5 e5 = Enum_TestClass_enum54_Enum5.a;
                e5++; e5++; e5++; e5--; e5--; e5--; e5++; e5--; e5--; e5++; e5++; e5++; e5++; e5++; e5++; e5++;
                if ((int)e5 == (int)Enum_TestClass_enum54_Enum5.f + 1)
                    retval -= 0x01000;
                e5--; e5--; e5--; e5--; e5--; e5--;
                if (e5 == Enum_TestClass_enum54_Enum5.a)
                    retval -= 0x02000;
                e5--;
                if ((int)e5 == (int)Enum_TestClass_enum54_Enum5.a - 1)
                    retval -= 0x04000;
                Enum_TestClass_enum54_Enum5ul e5ul = Enum_TestClass_enum54_Enum5ul.a;
                e5ul++; e5ul++; e5ul++; e5ul--; e5ul--; e5ul--; e5ul++; e5ul--; e5ul--; e5ul++; e5ul++; e5ul++; e5ul++; e5ul++; e5ul++; e5ul++;
                if ((int)e5ul == (int)Enum_TestClass_enum54_Enum5ul.f + 1)
                    retval -= 0x01000000;
                e5ul--; e5ul--; e5ul--; e5ul--; e5ul--; e5ul--;
                if (e5ul == Enum_TestClass_enum54_Enum5ul.a)
                    retval -= 0x02000000;
                e5ul--;
                if ((int)e5ul == (int)Enum_TestClass_enum54_Enum5ul.a - 1)
                    retval -= 0x04000000;
                if (retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL, 0x{0:X} " + retval.ToString());
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public enum Enum_TestClass_enum55_Enum1 { a, b, c, d, e, f };
        public enum Enum_TestClass_enum55_Enum2 : byte { a, b, c, d, e, f };
        public enum Enum_TestClass_enum55_Enum2sb : sbyte { a, b, c, d, e, f };
        public enum Enum_TestClass_enum55_Enum3 : short { a, b, c, d, e, f };
        public enum Enum_TestClass_enum55_Enum3us : ushort { a, b, c, d, e, f };
        public enum Enum_TestClass_enum55_Enum4 : int { a, b, c, d, e, f };
        public enum Enum_TestClass_enum55_Enum4ui : uint { a, b, c, d, e, f };
        public enum Enum_TestClass_enum55_Enum5 : long { a, b, c, d, e, f };
        public enum Enum_TestClass_enum55_Enum5ul : ulong { a, b, c, d, e, f };
        public class Enum_TestClass_enum55
        {
            public static int retval = 0x7FFFFFF;

            public static int Main_old()
            {
                Enum_TestClass_enum55_Enum1 e1 = Enum_TestClass_enum55_Enum1.a;
                e1++; e1++; e1++; --e1; --e1; --e1; e1++; --e1; --e1; e1++; e1++; e1++; e1++; e1++; e1++;
                if ((int)e1-- == (int)Enum_TestClass_enum55_Enum1.f)
                    retval -= 0x01;
                --e1; --e1; --e1; --e1;
                if (e1 == Enum_TestClass_enum55_Enum1.a)
                    retval -= 0x02;
                --e1;
                if ((int)e1 == (int)Enum_TestClass_enum55_Enum1.a - 1)
                    retval -= 0x04;

                Enum_TestClass_enum55_Enum2 e2 = Enum_TestClass_enum55_Enum2.a;
                e2++; e2++; e2++; --e2; --e2; --e2; e2++; --e2; --e2; e2++; e2++; e2++; e2++; e2++; e2++; e2++;
                if ((int)e2-- == (int)Enum_TestClass_enum55_Enum2.f + 1)
                    retval -= 0x08;
                --e2; --e2; --e2; --e2;
                if (e2 == Enum_TestClass_enum55_Enum2.b)
                    retval -= 0x10;
                --e2; --e2;
                if ((int)e2 == 255) //This has to be a manifest constant because byte is UNSIGNED
                    retval -= 0x20;

                Enum_TestClass_enum55_Enum2sb e2sb = Enum_TestClass_enum55_Enum2sb.a;
                e2sb++; e2sb++; e2sb++; --e2sb; --e2sb; --e2sb; e2sb++; --e2sb; --e2sb; e2sb++; e2sb++; e2sb++; e2sb++; e2sb++; e2sb++; e2sb++;
                if ((int)e2sb-- == (int)Enum_TestClass_enum55_Enum2sb.f + 1)
                    retval -= 0x8000;
                --e2sb; --e2sb; --e2sb; --e2sb;
                if (e2sb == Enum_TestClass_enum55_Enum2sb.b)
                    retval -= 0x10000;
                --e2sb; --e2sb;
                if ((int)e2sb == -1)
                    retval -= 0x20000;

                Enum_TestClass_enum55_Enum3 e3 = Enum_TestClass_enum55_Enum3.a;
                e3++; e3++; e3++; --e3; --e3; --e3; e3++; --e3; --e3; e3++; e3++; e3++; e3++; e3++; e3++; e3++;
                if ((int)e3 == (int)Enum_TestClass_enum55_Enum3.f + 1)
                    retval -= 0x40;
                --e3; --e3; --e3; --e3; --e3; --e3;
                if (e3 == Enum_TestClass_enum55_Enum3.a)
                    retval -= 0x80;
                --e3;
                if ((int)e3 == (int)Enum_TestClass_enum55_Enum3.a - 1)
                    retval -= 0x100;

                Enum_TestClass_enum55_Enum3us e3us = Enum_TestClass_enum55_Enum3us.a;
                e3us++; e3us++; e3us++; --e3us; --e3us; --e3us; e3us++; --e3us; --e3us; e3us++; e3us++; e3us++; e3us++; e3us++; e3us++; e3us++;
                if ((int)e3us == (int)Enum_TestClass_enum55_Enum3us.f + 1)
                    retval -= 0x40000;
                --e3us; --e3us; --e3us; --e3us; --e3us; --e3us;
                if (e3us == Enum_TestClass_enum55_Enum3us.a)
                    retval -= 0x80000;
                --e3us;
                if ((int)e3us == 65535)
                    retval -= 0x100000;

                Enum_TestClass_enum55_Enum4 e4 = Enum_TestClass_enum55_Enum4.a;
                e4++; e4++; e4++; --e4; --e4; --e4; e4++; --e4; --e4; e4++; e4++; e4++; e4++; e4++; e4++; e4++;
                if ((int)e4-- == (int)Enum_TestClass_enum55_Enum4.f + 1)
                    retval -= 0x200;
                --e4; --e4; --e4; --e4; --e4;
                if (e4 == Enum_TestClass_enum55_Enum4.a)
                    retval -= 0x400;
                --e4;
                if ((int)e4 == (int)Enum_TestClass_enum55_Enum4.a - 1)
                    retval -= 0x800;

                Enum_TestClass_enum55_Enum4ui e4ui = Enum_TestClass_enum55_Enum4ui.a;
                e4ui++; e4ui++; e4ui++; --e4ui; --e4ui; --e4ui; e4ui++; --e4ui; --e4ui; e4ui++; e4ui++; e4ui++; e4ui++; e4ui++; e4ui++; e4ui++;
                if ((int)e4ui-- == (int)Enum_TestClass_enum55_Enum4ui.f + 1)
                    retval -= 0x200000;
                --e4ui; --e4ui; --e4ui; --e4ui; --e4ui;
                if (e4ui == Enum_TestClass_enum55_Enum4ui.a)
                    retval -= 0x400000;
                --e4ui;
                if ((int)e4ui == (int)Enum_TestClass_enum55_Enum4ui.a - 1)
                    retval -= 0x800000;

                Enum_TestClass_enum55_Enum5 e5 = Enum_TestClass_enum55_Enum5.a;
                e5++; e5++; e5++; --e5; --e5; --e5; e5++; --e5; --e5; e5++; e5++; e5++; e5++; e5++; e5++; e5++;
                if ((int)e5-- == (int)Enum_TestClass_enum55_Enum5.f + 1)
                    retval -= 0x1000;
                --e5; --e5; --e5; --e5; --e5;
                if (e5-- == Enum_TestClass_enum55_Enum5.a)
                    retval -= 0x2000;
                --e5;
                if ((int)e5 == (int)Enum_TestClass_enum55_Enum5.a - 2)
                    retval -= 0x4000;
                Enum_TestClass_enum55_Enum5ul e5ul = Enum_TestClass_enum55_Enum5ul.a;
                e5ul++; e5ul++; e5ul++; --e5ul; --e5ul; --e5ul; e5ul++; --e5ul; --e5ul; e5ul++; e5ul++; e5ul++; e5ul++; e5ul++; e5ul++; e5ul++;
                if ((int)e5ul-- == (int)Enum_TestClass_enum55_Enum5ul.f + 1)
                    retval -= 0x1000000;
                --e5ul; --e5ul; --e5ul; --e5ul; --e5ul;
                if (e5ul-- == Enum_TestClass_enum55_Enum5ul.a)
                    retval -= 0x2000000;
                --e5ul;
                if ((int)e5ul == (int)Enum_TestClass_enum55_Enum5ul.a - 2)
                    retval -= 0x4000000;
                Log.Comment((retval).ToString());
                if (retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public enum Enum_TestClass_enum56_Enum1 { a, b, c, d, e, f };
        public enum Enum_TestClass_enum56_Enum2 : byte { a, b, c, d, e, f };
        public enum Enum_TestClass_enum56_Enum3 : short { a, b, c, d, e, f };
        public enum Enum_TestClass_enum56_Enum4 : int { a, b, c, d, e, f };
        public enum Enum_TestClass_enum56_Enum5 : long { a, b, c, d, e, f };
        public enum Enum_TestClass_enum56_Enum2sb : sbyte { a, b, c, d, e, f };
        public enum Enum_TestClass_enum56_Enum3us : ushort { a, b, c, d, e, f };
        public enum Enum_TestClass_enum56_Enum4ui : uint { a, b, c, d, e, f };
        public enum Enum_TestClass_enum56_Enum5ul : ulong { a, b, c, d, e, f };
        public class Enum_TestClass_enum56
        {
            public static int retval = 0x7FFFFFF;

            public static int Main_old()
            {
                Enum_TestClass_enum56_Enum1 e1 = Enum_TestClass_enum56_Enum1.a;
                ++e1; e1++; ++e1; --e1; --e1; --e1; e1++; --e1; --e1; ++e1; e1++; ++e1; ++e1; ++e1; ++e1; ++e1;
                if ((int)e1 == (int)Enum_TestClass_enum56_Enum1.f + 1)
                    retval -= 0x01;
                --e1; --e1; --e1; --e1;
                if (e1 == Enum_TestClass_enum56_Enum1.c)
                    retval -= 0x02;
                --e1; --e1; --e1;
                if ((int)e1 == (int)Enum_TestClass_enum56_Enum1.a - 1)
                    retval -= 0x04;

                Enum_TestClass_enum56_Enum2 e2 = Enum_TestClass_enum56_Enum2.a;
                ++e2; e2++; ++e2; --e2; --e2; --e2; e2++; --e2; --e2; ++e2; e2++; ++e2; ++e2; ++e2; ++e2; ++e2;
                if ((int)e2 == (int)Enum_TestClass_enum56_Enum2.f + 1)
                    retval -= 0x08;
                --e2; --e2; --e2; --e2;
                if (e2 == Enum_TestClass_enum56_Enum2.c)
                    retval -= 0x10;
                --e2; --e2; --e2;
                if ((int)e2 == 255)  //This has to be a manifest constant because byte is UNSIGNED
                    retval -= 0x20;

                Enum_TestClass_enum56_Enum3 e3 = Enum_TestClass_enum56_Enum3.a;
                ++e3; e3++; ++e3; --e3; --e3; --e3; e3++; --e3; --e3; ++e3; e3++; ++e3; ++e3; ++e3; ++e3; ++e3;
                if ((int)e3 == (int)Enum_TestClass_enum56_Enum3.f + 1)
                    retval -= 0x40;
                --e3; --e3; --e3; --e3;
                if (e3 == Enum_TestClass_enum56_Enum3.c)
                    retval -= 0x80;
                --e3; --e3; --e3;
                if ((int)e3 == (int)Enum_TestClass_enum56_Enum3.a - 1)
                    retval -= 0x100;

                Enum_TestClass_enum56_Enum4 e4 = Enum_TestClass_enum56_Enum4.a;
                ++e4; e4++; ++e4; --e4; --e4; --e4; e4++; --e4; --e4; ++e4; e4++; ++e4; ++e4; ++e4; ++e4; ++e4;
                if ((int)e4 == (int)Enum_TestClass_enum56_Enum4.f + 1)
                    retval -= 0x200;
                --e4; --e4; --e4; --e4;
                if (e4 == Enum_TestClass_enum56_Enum4.c)
                    retval -= 0x400;
                --e4; --e4; --e4;
                if ((int)e4 == (int)Enum_TestClass_enum56_Enum4.a - 1)
                    retval -= 0x800;

                Enum_TestClass_enum56_Enum5 e5 = Enum_TestClass_enum56_Enum5.a;
                ++e5; e5++; ++e5; --e5; --e5; --e5; e5++; --e5; --e5; ++e5; e5++; ++e5; ++e5; ++e5; ++e5; ++e5;
                if ((int)e5 == (int)Enum_TestClass_enum56_Enum5.f + 1)
                    retval -= 0x1000;
                --e5; --e5; --e5; --e5;
                if (e5 == Enum_TestClass_enum56_Enum5.c)
                    retval -= 0x2000;
                --e5; --e5; --e5;
                if ((int)e5 == (int)Enum_TestClass_enum56_Enum5.a - 1)
                    retval -= 0x4000;

                Enum_TestClass_enum56_Enum2sb e2sb = Enum_TestClass_enum56_Enum2sb.a;
                ++e2sb; e2sb++; ++e2sb; --e2sb; --e2sb; --e2sb; e2sb++; --e2sb; --e2sb; ++e2sb; e2sb++; ++e2sb; ++e2sb; ++e2sb; ++e2sb; ++e2sb;
                if ((int)e2sb == (int)Enum_TestClass_enum56_Enum2sb.f + 1)
                    retval -= 0x8000;
                --e2sb; --e2sb; --e2sb; --e2sb;
                if (e2sb == Enum_TestClass_enum56_Enum2sb.c)
                    retval -= 0x10000;
                --e2sb; --e2sb; --e2sb;
                if ((int)e2sb == -1)  //This has to be a manifest constant because byte is UNSIGNED
                    retval -= 0x20000;

                Enum_TestClass_enum56_Enum3us e3us = Enum_TestClass_enum56_Enum3us.a;
                ++e3us; e3us++; ++e3us; --e3us; --e3us; --e3us; e3us++; --e3us; --e3us; ++e3us; e3us++; ++e3us; ++e3us; ++e3us; ++e3us; ++e3us;
                if ((int)e3us == (int)Enum_TestClass_enum56_Enum3us.f + 1)
                    retval -= 0x40000;
                --e3us; --e3us; --e3us; --e3us;
                if (e3us == Enum_TestClass_enum56_Enum3us.c)
                    retval -= 0x80000;
                --e3us; --e3us; --e3us;
                if ((int)e3us == 65535)
                    retval -= 0x100000;

                Enum_TestClass_enum56_Enum4ui e4ui = Enum_TestClass_enum56_Enum4ui.a;
                ++e4ui; e4ui++; ++e4ui; --e4ui; --e4ui; --e4ui; e4ui++; --e4ui; --e4ui; ++e4ui; e4ui++; ++e4ui; ++e4ui; ++e4ui; ++e4ui; ++e4ui;
                if ((int)e4ui == (int)Enum_TestClass_enum56_Enum4ui.f + 1)
                    retval -= 0x200000;
                --e4ui; --e4ui; --e4ui; --e4ui;
                if (e4ui == Enum_TestClass_enum56_Enum4ui.c)
                    retval -= 0x400000;
                --e4ui; --e4ui; --e4ui;
                if ((int)e4ui == (int)Enum_TestClass_enum56_Enum4ui.a - 1)
                    retval -= 0x800000;

                Enum_TestClass_enum56_Enum5ul e5ul = Enum_TestClass_enum56_Enum5ul.a;
                ++e5ul; e5ul++; ++e5ul; --e5ul; --e5ul; --e5ul; e5ul++; --e5ul; --e5ul; ++e5ul; e5ul++; ++e5ul; ++e5ul; ++e5ul; ++e5ul; ++e5ul;
                if ((int)e5ul == (int)Enum_TestClass_enum56_Enum5ul.f + 1)
                    retval -= 0x1000000;
                --e5ul; --e5ul; --e5ul; --e5ul;
                if (e5ul == Enum_TestClass_enum56_Enum5ul.c)
                    retval -= 0x2000000;
                --e5ul; --e5ul; --e5ul;
                if ((int)e5ul == (int)Enum_TestClass_enum56_Enum5ul.a - 1)
                    retval -= 0x4000000;

                Log.Comment((retval).ToString());
                if (retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public enum Enum_TestClass_enum57_Enum1 { a, b, c, d, e, f };
        public enum Enum_TestClass_enum57_Enum2 : byte { a, b, c, d, e, f };
        public enum Enum_TestClass_enum57_Enum3 : short { a, b, c, d, e, f };
        public enum Enum_TestClass_enum57_Enum4 : int { a, b, c, d, e, f };
        public enum Enum_TestClass_enum57_Enum5 : long { a, b, c, d, e, f };
        public enum Enum_TestClass_enum57_Enum2sb : sbyte { a, b, c, d, e, f };
        public enum Enum_TestClass_enum57_Enum3us : ushort { a, b, c, d, e, f };
        public enum Enum_TestClass_enum57_Enum4ui : uint { a, b, c, d, e, f };
        public enum Enum_TestClass_enum57_Enum5ul : ulong { a, b, c, d, e, f };
        public class Enum_TestClass_enum57
        {
            public static int retval = 0x7FFFFFF;

            public static int Main_old()
            {
                Enum_TestClass_enum57_Enum1 e1 = Enum_TestClass_enum57_Enum1.a;
                ++e1; e1++; ++e1; e1--; e1--; e1--; e1++; e1--; e1--; ++e1; e1++; ++e1; ++e1; ++e1; e1++; ++e1;
                if ((int)e1-- == (int)Enum_TestClass_enum57_Enum1.f + 1)
                    retval -= 0x01;
                e1--; e1--; e1--;
                if (e1++ == Enum_TestClass_enum57_Enum1.c)
                    retval -= 0x02;
                e1--; e1--; e1--; e1--;
                if ((int)e1 == (int)Enum_TestClass_enum57_Enum1.a - 1)
                    retval -= 0x04;

                Enum_TestClass_enum57_Enum2 e2 = Enum_TestClass_enum57_Enum2.a;
                ++e2; e2++; ++e2; e2--; e2--; e2--; e2++; e2--; e2--; ++e2; e2++; ++e2; ++e2; ++e2; e2++; ++e2;
                if ((int)e2-- == (int)Enum_TestClass_enum57_Enum1.f + 1)
                    retval -= 0x08;
                e2--; e2--; e2--;
                if (e2++ == Enum_TestClass_enum57_Enum2.c)
                    retval -= 0x10;
                e2--; e2--; e2--; e2--;
                if ((int)e2 == 255)  //This has to be a manifest constant because byte is UNSIGNED
                    retval -= 0x20;

                Enum_TestClass_enum57_Enum3 e3 = Enum_TestClass_enum57_Enum3.a;
                ++e3; e3++; ++e3; e3--; e3--; e3--; e3++; e3--; e3--; ++e3; e3++; ++e3; ++e3; ++e3; e3++; ++e3;
                if ((int)e3-- == (int)Enum_TestClass_enum57_Enum3.f + 1)
                    retval -= 0x40;
                e3--; e3--; e3--;
                if (e3++ == Enum_TestClass_enum57_Enum3.c)
                    retval -= 0x80;
                e3--; e3--; e3--; e3--;
                if ((int)e3 == (int)Enum_TestClass_enum57_Enum3.a - 1)
                    retval -= 0x100;

                Enum_TestClass_enum57_Enum4 e4 = Enum_TestClass_enum57_Enum4.a;
                ++e4; e4++; ++e4; e4--; e4--; e4--; e4++; e4--; e4--; ++e4; e4++; ++e4; ++e4; ++e4; e4++; ++e4;
                if ((int)e4-- == (int)Enum_TestClass_enum57_Enum4.f + 1)
                    retval -= 0x200;
                e4--; e4--; e4--;
                if (e4++ == Enum_TestClass_enum57_Enum4.c)
                    retval -= 0x400;
                e4--; e4--; e4--; e4--;
                if ((int)e4 == (int)Enum_TestClass_enum57_Enum4.a - 1)
                    retval -= 0x800;

                Enum_TestClass_enum57_Enum5 e5 = Enum_TestClass_enum57_Enum5.a;
                ++e5; e5++; ++e5; e5--; e5--; e5--; e5++; e5--; e5--; ++e5; e5++; ++e5; ++e5; ++e5; e5++; ++e5;
                if ((int)e5-- == (int)Enum_TestClass_enum57_Enum5.f + 1)
                    retval -= 0x1000;
                e5--; e5--; e5--;
                if (e5++ == Enum_TestClass_enum57_Enum5.c)
                    retval -= 0x2000;
                e5--; e5--; e5--; e5--;
                if ((int)e5 == (int)Enum_TestClass_enum57_Enum5.a - 1)
                    retval -= 0x4000;
                Enum_TestClass_enum57_Enum2sb e2sb = Enum_TestClass_enum57_Enum2sb.a;
                ++e2sb; e2sb++; ++e2sb; e2sb--; e2sb--; e2sb--; e2sb++; e2sb--; e2sb--; ++e2sb; e2sb++; ++e2sb; ++e2sb; ++e2sb; e2sb++; ++e2sb;
                if ((int)e2sb-- == (int)Enum_TestClass_enum57_Enum1.f + 1)
                    retval -= 0x8000;
                e2sb--; e2sb--; e2sb--;
                if (e2sb++ == Enum_TestClass_enum57_Enum2sb.c)
                    retval -= 0x10000;
                e2sb--; e2sb--; e2sb--; e2sb--;
                if ((int)e2sb == -1)
                    retval -= 0x20000;

                Enum_TestClass_enum57_Enum3us e3us = Enum_TestClass_enum57_Enum3us.a;
                ++e3us; e3us++; ++e3us; e3us--; e3us--; e3us--; e3us++; e3us--; e3us--; ++e3us; e3us++; ++e3us; ++e3us; ++e3us; e3us++; ++e3us;
                if ((int)e3us-- == (int)Enum_TestClass_enum57_Enum3us.f + 1)
                    retval -= 0x40000;
                e3us--; e3us--; e3us--;
                if (e3us++ == Enum_TestClass_enum57_Enum3us.c)
                    retval -= 0x80000;
                e3us--; e3us--; e3us--; e3us--;
                if ((int)e3us == 65535)
                    retval -= 0x100000;

                Enum_TestClass_enum57_Enum4ui e4ui = Enum_TestClass_enum57_Enum4ui.a;
                ++e4ui; e4ui++; ++e4ui; e4ui--; e4ui--; e4ui--; e4ui++; e4ui--; e4ui--; ++e4ui; e4ui++; ++e4ui; ++e4ui; ++e4ui; e4ui++; ++e4ui;
                if ((int)e4ui-- == (int)Enum_TestClass_enum57_Enum4ui.f + 1)
                    retval -= 0x200000;
                e4ui--; e4ui--; e4ui--;
                if (e4ui++ == Enum_TestClass_enum57_Enum4ui.c)
                    retval -= 0x400000;
                e4ui--; e4ui--; e4ui--; e4ui--;
                if ((int)e4ui == (int)Enum_TestClass_enum57_Enum4ui.a - 1)
                    retval -= 0x800000;

                Enum_TestClass_enum57_Enum5ul e5ul = Enum_TestClass_enum57_Enum5ul.a;
                ++e5ul; e5ul++; ++e5ul; e5ul--; e5ul--; e5ul--; e5ul++; e5ul--; e5ul--; ++e5ul; e5ul++; ++e5ul; ++e5ul; ++e5ul; e5ul++; ++e5ul;
                if ((int)e5ul-- == (int)Enum_TestClass_enum57_Enum5ul.f + 1)
                    retval -= 0x1000000;
                e5ul--; e5ul--; e5ul--;
                if (e5ul++ == Enum_TestClass_enum57_Enum5ul.c)
                    retval -= 0x2000000;
                e5ul--; e5ul--; e5ul--; e5ul--;
                if ((int)e5ul == (int)Enum_TestClass_enum57_Enum5ul.a - 1)
                    retval -= 0x4000000;
                if (retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL, 0x{0:X} " + retval.ToString());
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public enum Enum_TestClass_enum58_Enum1 { a = 0x01, b = 0x02, c = 0x04, d = 0x08, e = 0x10, f = 0x20 };
        public enum Enum_TestClass_enum58_Enum2 : byte { a = Enum_TestClass_enum58_Enum1.a, b = Enum_TestClass_enum58_Enum1.b, c = Enum_TestClass_enum58_Enum1.c, d = Enum_TestClass_enum58_Enum1.d, e = Enum_TestClass_enum58_Enum1.e, f = Enum_TestClass_enum58_Enum1.f };
        public enum Enum_TestClass_enum58_Enum3 : short { a = Enum_TestClass_enum58_Enum1.a, b = Enum_TestClass_enum58_Enum1.b, c = Enum_TestClass_enum58_Enum1.c, d = Enum_TestClass_enum58_Enum1.d, e = Enum_TestClass_enum58_Enum1.e, f = Enum_TestClass_enum58_Enum1.f };
        public enum Enum_TestClass_enum58_Enum4 : int { a = Enum_TestClass_enum58_Enum1.a, b = Enum_TestClass_enum58_Enum1.b, c = Enum_TestClass_enum58_Enum1.c, d = Enum_TestClass_enum58_Enum1.d, e = Enum_TestClass_enum58_Enum1.e, f = Enum_TestClass_enum58_Enum1.f };
        public enum Enum_TestClass_enum58_Enum5 : long { a = Enum_TestClass_enum58_Enum1.a, b = Enum_TestClass_enum58_Enum1.b, c = Enum_TestClass_enum58_Enum1.c, d = Enum_TestClass_enum58_Enum1.d, e = Enum_TestClass_enum58_Enum1.e, f = Enum_TestClass_enum58_Enum1.f };
        public enum Enum_TestClass_enum58_Enum2sb : sbyte { a = Enum_TestClass_enum58_Enum1.a, b = Enum_TestClass_enum58_Enum1.b, c = Enum_TestClass_enum58_Enum1.c, d = Enum_TestClass_enum58_Enum1.d, e = Enum_TestClass_enum58_Enum1.e, f = Enum_TestClass_enum58_Enum1.f };
        public enum Enum_TestClass_enum58_Enum3us : ushort { a = Enum_TestClass_enum58_Enum1.a, b = Enum_TestClass_enum58_Enum1.b, c = Enum_TestClass_enum58_Enum1.c, d = Enum_TestClass_enum58_Enum1.d, e = Enum_TestClass_enum58_Enum1.e, f = Enum_TestClass_enum58_Enum1.f };
        public enum Enum_TestClass_enum58_Enum4ui : uint { a = Enum_TestClass_enum58_Enum1.a, b = Enum_TestClass_enum58_Enum1.b, c = Enum_TestClass_enum58_Enum1.c, d = Enum_TestClass_enum58_Enum1.d, e = Enum_TestClass_enum58_Enum1.e, f = Enum_TestClass_enum58_Enum1.f };
        public enum Enum_TestClass_enum58_Enum5ul : ulong { a = Enum_TestClass_enum58_Enum1.a, b = Enum_TestClass_enum58_Enum1.b, c = Enum_TestClass_enum58_Enum1.c, d = Enum_TestClass_enum58_Enum1.d, e = Enum_TestClass_enum58_Enum1.e, f = Enum_TestClass_enum58_Enum1.f };
        public class Enum_TestClass_enum58
        {
            public static int retval = 0x1FF;

            public static int Main_old()
    		{
    		Enum_TestClass_enum58_Enum1 e1 = Enum_TestClass_enum58_Enum1.a;
    		e1 = e1 ^ Enum_TestClass_enum58_Enum1.a;
    		e1 ^= Enum_TestClass_enum58_Enum1.b;
    		e1 = e1 | Enum_TestClass_enum58_Enum1.f;
    		e1 |= Enum_TestClass_enum58_Enum1.e;
    		e1 = e1 & Enum_TestClass_enum58_Enum1.b;
    		e1 &= ~Enum_TestClass_enum58_Enum1.b;
    		if ((int)e1 == 0)
    			retval -= 0x001;
    		Enum_TestClass_enum58_Enum2 e2 = Enum_TestClass_enum58_Enum2.a;
    		e2 = e2 ^ Enum_TestClass_enum58_Enum2.a;
    		e2 ^= Enum_TestClass_enum58_Enum2.b;
    		e2 = e2 | Enum_TestClass_enum58_Enum2.f;
    		e2 |= Enum_TestClass_enum58_Enum2.e;
    		e2 = e2 & Enum_TestClass_enum58_Enum2.b;
    		e2 &= ~Enum_TestClass_enum58_Enum2.b;
    		if ((int)e2 == 0)
    			retval -= 0x002;
    		Enum_TestClass_enum58_Enum2sb e2sb = Enum_TestClass_enum58_Enum2sb.a;
    		e2sb = e2sb ^ Enum_TestClass_enum58_Enum2sb.a;
    		e2sb ^= Enum_TestClass_enum58_Enum2sb.b;
    		e2sb = e2sb | Enum_TestClass_enum58_Enum2sb.f;
    		e2sb |= Enum_TestClass_enum58_Enum2sb.e;
    		e2sb = e2sb & Enum_TestClass_enum58_Enum2sb.b;
    		e2sb &= ~Enum_TestClass_enum58_Enum2sb.b;
    		if ((int)e2sb == 0)
    			retval -= 0x004;
    		Enum_TestClass_enum58_Enum3 e3 = Enum_TestClass_enum58_Enum3.a;
    		e3 = e3 ^ Enum_TestClass_enum58_Enum3.a;
    		e3 ^= Enum_TestClass_enum58_Enum3.b;
    		e3 = e3 | Enum_TestClass_enum58_Enum3.f;
    		e3 |= Enum_TestClass_enum58_Enum3.e;
    		e3 = e3 & Enum_TestClass_enum58_Enum3.b;
    		e3 &= ~Enum_TestClass_enum58_Enum3.b;
    		if ((int)e3 == 0)
    			retval -= 0x008;
    		Enum_TestClass_enum58_Enum3us e3us = Enum_TestClass_enum58_Enum3us.a;
    		e3us = e3us ^ Enum_TestClass_enum58_Enum3us.a;
    		e3us ^= Enum_TestClass_enum58_Enum3us.b;
    		e3us = e3us | Enum_TestClass_enum58_Enum3us.f;
    		e3us |= Enum_TestClass_enum58_Enum3us.e;
    		e3us = e3us & Enum_TestClass_enum58_Enum3us.b;
    		e3us &= ~Enum_TestClass_enum58_Enum3us.b;
    		if ((int)e3us == 0)
    			retval -= 0x010;
    		Enum_TestClass_enum58_Enum4 e4 = Enum_TestClass_enum58_Enum4.a;
    		e4 = e4 ^ Enum_TestClass_enum58_Enum4.a;
    		e4 ^= Enum_TestClass_enum58_Enum4.b;
    		e4 = e4 | Enum_TestClass_enum58_Enum4.f;
    		e4 |= Enum_TestClass_enum58_Enum4.e;
    		e4 = e4 & Enum_TestClass_enum58_Enum4.b;
    		e4 &= ~Enum_TestClass_enum58_Enum4.b;
    		if ((int)e4 == 0)
    			retval -= 0x020;
    		Enum_TestClass_enum58_Enum4ui e4ui = Enum_TestClass_enum58_Enum4ui.a;
    		e4ui = e4ui ^ Enum_TestClass_enum58_Enum4ui.a;
    		e4ui ^= Enum_TestClass_enum58_Enum4ui.b;
    		e4ui = e4ui | Enum_TestClass_enum58_Enum4ui.f;
    		e4ui |= Enum_TestClass_enum58_Enum4ui.e;
    		e4ui = e4ui & Enum_TestClass_enum58_Enum4ui.b;
    		e4ui &= ~Enum_TestClass_enum58_Enum4ui.b;
    		if ((int)e4ui == 0)
    			retval -= 0x040;
    		Enum_TestClass_enum58_Enum5 e5 = Enum_TestClass_enum58_Enum5.a;
    		e5 = e5 ^ Enum_TestClass_enum58_Enum5.a;
    		e5 ^= Enum_TestClass_enum58_Enum5.b;
    		e5 = e5 | Enum_TestClass_enum58_Enum5.f;
    		e5 |= Enum_TestClass_enum58_Enum5.e;
    		e5 = e5 & Enum_TestClass_enum58_Enum5.b;
    		e5 &= ~Enum_TestClass_enum58_Enum5.b;
    		if ((int)e5 == 0)
    			retval -= 0x080;
    		Enum_TestClass_enum58_Enum5ul e5ul = Enum_TestClass_enum58_Enum5ul.a;
    		e5ul = e5ul ^ Enum_TestClass_enum58_Enum5ul.a;
    		e5ul ^= Enum_TestClass_enum58_Enum5ul.b;
    		e5ul = e5ul | Enum_TestClass_enum58_Enum5ul.f;
    		e5ul |= Enum_TestClass_enum58_Enum5ul.e;
    		e5ul = e5ul & Enum_TestClass_enum58_Enum5ul.b;
    		e5ul &= ~Enum_TestClass_enum58_Enum5ul.b;
    		if ((int)e5ul == 0)
    			retval -= 0x100;
            if (0 == retval) Log.Comment ("PASS");
            else Log.Comment ("FAIL");
            return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };


        public enum Enum_TestClass_enum62_E
        {
            Type1 = 100,
            Type2,
            Type3
        }
        public class Enum_TestClass_enum62
        {
            public static int Main_old()
            {
                Enum_TestClass_enum62_E[] tt = new Enum_TestClass_enum62_E[2];
                tt[0] = Enum_TestClass_enum62_E.Type1;
                tt[1] = Enum_TestClass_enum62_E.Type1;
                Log.Comment(tt[0].ToString());
                Log.Comment(tt[1].ToString());
                int nInt = (int)tt[1];
                Log.Comment(nInt.ToString());
                int i = (int)tt[0] + (int)tt[1] + nInt - 300;
                if (0 == i) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return i;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Enum_TestClass_enum63
        {
            public enum e1 { one = 1, two = 2, three = 3 };
            public static int Main_old()
            {
                int retval = 1;
                e1 v_e1 = e1.two;
                e1 v = e1.three;
                Log.Comment("v_e1 == " + ((int)v_e1).ToString() + ", v == " + ((int)v).ToString());
                if (v_e1 < v)
                    retval = 0;
                if (0 == retval) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };
        public class Enum_TestClass_enum64
        {
            public enum e1 { one = 1, two = 2, three = 3 };
            public static int Main_old()
            {
                int retval = 1;
                e1 v_e1 = e1.three;
                e1 v = e1.two;
                Log.Comment("v_e1 == " + ((int)v_e1).ToString() + ", v == " + ((int)v).ToString());
                if (v_e1 > v)
                    retval = 0;
                if (0 == retval) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };
        public class Enum_TestClass_enum65
        {
            public enum e1 { one = 1, two = 2, three = 3 };
            public static int Main_old()
            {
                int retval = 7;
                e1 v_e1 = e1.three;
                e1 v = e1.two;
                Log.Comment("v_e1 == " + ((int)v_e1).ToString() + ", v == " + ((int)v).ToString());
                if (v_e1 >= v)
                    retval -= 1;
                v_e1 = e1.three;
                v = e1.three;
                Log.Comment("v_e1 == " + ((int)v_e1).ToString() + ", v == " + ((int)v).ToString());
                if (v_e1 >= v)
                    retval -= 2;
                if (v_e1 == v)
                    retval -= 4;
                if (0 == retval) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };
        public class Enum_TestClass_enum66
        {
            public enum e1 { one = 1, two = 2, three = 3 };
            public static int Main_old()
            {
                int retval = 7;
                e1 v_e1 = e1.two;
                e1 v = e1.three;
                Log.Comment("v_e1 == " + ((int)v_e1).ToString() + ", v == " + ((int)v).ToString());
                if (v_e1 <= v)
                    retval -= 1;
                if (v_e1 != v)
                    retval -= 2;
                v_e1 = e1.three;
                v = e1.three;
                Log.Comment("v_e1 == " + ((int)v_e1).ToString() + ", v == " + ((int)v).ToString());
                if (v_e1 <= v)
                    retval -= 4;
                if (0 == retval) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };
        public class Enum_TestClass_enum67
        {
            public enum e1 { one = 1, two = 2, three = 3 };
            public static int Main_old()
            {
                int retval = 1;
                if (e1.two < e1.three)
                    retval = 0;
                if (0 == retval) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };
        public class Enum_TestClass_enum68
        {
            public enum e1 { one = 1, two = 2, three = 3 };
            public static int Main_old()
            {
                int retval = 1;
                if (e1.three > e1.two)
                    retval = 0;
                if (0 == retval) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };
        public class Enum_TestClass_enum69
        {
            public enum e1 { one = 1, two = 2, three = 3 };
            public static int Main_old()
            {
                // check ordering of values with >= operator
                if (e1.one >= e1.two
                 || e1.one >= e1.three
                 || e1.two >= e1.three
                  )
                {
                    Log.Comment("FAIL");
                    return -1;
                }

                Log.Comment("PASS");
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };
        public class Enum_TestClass_enum70
        {
            public enum e1 { one = 1, two = 2, three = 3 };
            public static int Main_old()
            {
                 // check ordering of values with <= operator
                if( e1.three <= e1.two
                 || e1.three <= e1.one
                 || e1.two <= e1.one
                  )
                {
                    Log.Comment("FAIL");
                    return -1;
                }

                Log.Comment("PASS");
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };
        public class Enum_TestClass_enum71
        {
            public enum e1 { one = 1, two = 2, three = 3 };
            public static int Main_old()
            {
                int retval = 1;
                e1 v_e1 = e1.two;
                e1 v = e1.three;
                string s1 = v_e1.ToString();
                string s2 = v.ToString();

                Log.Comment("v_e1 == " + s1.ToString() + ", v == " + s2.ToString());

                if (s1.Equals("two") && s2.Equals("three"))
                    retval = 0;
                if (0 == retval) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };
        public class Enum_TestClass_enum72
        {
            public enum e1 { zero, one = 1, two = 2, three = 3 };
            public static int Main_old()
            {
                int retval = 0x3F;
                e1 v_e1 = e1.two;
                e1 v = e1.one;
                if ((int)(v_e1 & v) == 0)
                    retval -= 1;

                if ((int)(e1.three & v) == 1)
                    retval -= 2;

                if ((int)(e1.three & e1.two) == 2)
                    retval -= 4;

                if ((v_e1 & v) == e1.zero)
                    retval -= 8;

                if ((e1.three & v) == e1.one)
                    retval -= 0x10;

                if ((e1.three & e1.two) == e1.two)
                    retval -= 0x20;

                if (0 == retval) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };
        public class Enum_TestClass_enum73
        {
            public enum e1 { zero, one = 1, two = 2, three = 3 };
            public static int Main_old()
            {
                int retval = 0x3F;
                e1 v_e1 = e1.two;
                e1 v = e1.one;
                if ((int)(v_e1 | v) == 3)
                    retval -= 1;

                if ((int)(e1.three | v) == 3)
                    retval -= 2;

                if ((int)(e1.three | e1.two) == 3)
                    retval -= 4;

                if ((v_e1 | v) == e1.three)
                    retval -= 8;

                if ((e1.three | v) == e1.three)
                    retval -= 0x10;

                if ((e1.three | e1.two) == e1.three)
                    retval -= 0x20;

                if (0 == retval) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };
        public class Enum_TestClass_enum74
        {
            public enum e1 { zero, one = 1, two = 2, three = 3 };
            public static int Main_old()
            {
                int retval = 0x3F;
                e1 v_e1 = e1.two;
                e1 v = e1.one;
                if ((int)(v_e1 ^ v) == 3)
                    retval -= 1;

                if ((int)(e1.three ^ v) == 2)
                    retval -= 2;

                if ((int)(e1.three ^ e1.two) == 1)
                    retval -= 4;

                if ((v_e1 ^ v) == e1.three)
                    retval -= 8;

                if ((e1.three ^ v) == e1.two)
                    retval -= 0x10;

                if ((e1.three ^ e1.two) == e1.one)
                    retval -= 0x20;

                if (0 == retval) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };
        public class Enum_TestClass_enum75
        {
            public enum e1 { zero, one = 1, two = 2, three = 3 };
            public static int Main_old()
            {
                int retval = 7;
                e1 v_e1 = e1.two;
                if ((int)(~v_e1) == -3)
                    retval -= 1;

                if ((int)(~e1.three) == -4)
                    retval -= 2;

                if ((~(e1.one | e1.two) & e1.three) == e1.zero)
                    retval -= 4;

                if (0 == retval) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };
        public class Enum_TestClass_enum77
        {
            public enum e1 { zero, one = 1, two = 2, three = 3 };
            public static int Main_old()
            {
                int retval = 0x3F;
                e1 v_e1 = e1.two;
                e1 v = e1.one;
                if ((v + (int)v_e1) == e1.three)
                    retval -= 1;

                if ((e1.three - e1.two) == 1)
                    retval -= 2;

                if ((1 + e1.two) == e1.three)
                    retval -= 4;

                if ((e1.one + 2) == e1.three)
                    retval -= 8;

                if ((1 + v_e1) == e1.three)
                    retval -= 0x10;

                if ((v + 2) == e1.three)
                    retval -= 0x20;

                if (0 == retval) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };
        public class Enum_TestClass_enum78
        {
            public static int Main_old(String[] s)
            {
                Enum_TestClass_enum78_Enum f;

                try
                {
                    object[] v = new object[1];
                    v[0] = Enum_TestClass_enum78_Enum.Second;


                    f = (Enum_TestClass_enum78_Enum)v[0];
                }
                catch (System.Exception e)
                {
                    Log.Comment("Caught System.Exception: Failed");
                    Log.Comment(e.ToString());
                    return 1;
                }
                Log.Comment("No System.Exception: Passed");
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old(null) == 0);
            }
        }
        public enum Enum_TestClass_enum78_Enum
        {
            First = 1,
            Second = -1,
            Third = -2

        }


        public class Enum_TestClass_enum83
        {
            public enum Enum_TestClass_enum83_Enum1 { a, b, c, };
            public enum Enum_TestClass_enum83_Enum2 : int { a, b, c, };
            public enum Enum_TestClass_enum83_Enum3 : uint { a, b, c, };
            public enum Enum_TestClass_enum83_Enum4 : byte { a, b, c, };
            public enum Enum_TestClass_enum83_Enum5 : sbyte { a, b, c, };
            public enum Enum_TestClass_enum83_Enum6 : short { a, b, c, };
            public enum Enum_TestClass_enum83_Enum7 : ushort { a, b, c, };
            public enum Enum_TestClass_enum83_Enum8 : long { a, b, c, };
            public enum Enum_TestClass_enum83_Enum9 : ulong { a, b, c, };
            public static int Main_old()
            {
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };

        public class Enum_TestClass_enum86_Base
        {
            public enum Enum_TestClass_enum86_Enum1_1 { a = -1, b = -2, c = -3, };
            public class Enum_TestClass_enum86_Enum1_2
            {
            }
            public struct Enum_TestClass_enum86_Enum1_3
            {
            }
            protected enum Enum_TestClass_enum86_Enum3 : uint { a = 1, b = 6, c = 7, };
            internal enum Enum_TestClass_enum86_Enum4 : byte { a = 2, b = 5, c = 8 };
            private enum Enum_TestClass_enum86_Enum5 : sbyte { a = 3, b = 4, c = 9 };
            protected bool CheckE5()
            {
                return ((int)Enum_TestClass_enum86_Enum5.b == 4);
            }
        }

        public class Enum_TestClass_enum86 : Enum_TestClass_enum86_Base
        {
            new enum Enum_TestClass_enum86_Enum1_1 { a, b = 10, c, };
            new enum Enum_TestClass_enum86_Enum1_2 { a, b = 11, c, };
            new enum Enum_TestClass_enum86_Enum1_3 { a, b = 12, c, };
            public static int Main_old()
            {
                int retval = 0xFF;

                Enum_TestClass_enum86_Base b = new Enum_TestClass_enum86_Base();
                Enum_TestClass_enum86 d = new Enum_TestClass_enum86();
                if ((int)Enum_TestClass_enum86_Base.Enum_TestClass_enum86_Enum1_1.b == -2)
                    retval -= 0x01;
                if ((int)Enum_TestClass_enum86.Enum_TestClass_enum86_Enum1_1.b == 10)
                    retval -= 0x02;
                if ((int)Enum_TestClass_enum86.Enum_TestClass_enum86_Enum1_2.b == 11)
                    retval -= 0x04;
                if ((int)Enum_TestClass_enum86.Enum_TestClass_enum86_Enum1_3.b == 12)
                    retval -= 0x08;
                if ((int)Enum_TestClass_enum86_Enum3.b == 6)
                    retval -= 0x10;
                if ((int)Enum_TestClass_enum86_Enum4.b == 5)
                    retval -= 0x20;
                if ((int)Enum_TestClass_enum86_Base.Enum_TestClass_enum86_Enum4.b == 5)
                    retval -= 0x40;
                if (d.CheckE5())
                    retval -= 0x80;
                if (0 == retval) Log.Comment("PASS");
                else Log.Comment("FAIL, 0x{0:X} " + retval.ToString());
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        };
        enum Enum_TestClass_enum93_Color
        {
            Red,
            Green = 10,
            Blue
        }
        class Enum_TestClass_enum93
        {
            static void Main_old()
            {
                Log.Comment(StringFromColor(Enum_TestClass_enum93_Color.Red));
                Log.Comment(StringFromColor(Enum_TestClass_enum93_Color.Green));
                Log.Comment(StringFromColor(Enum_TestClass_enum93_Color.Blue));
                Log.Comment(StringFromColor(Enum_TestClass_enum93_Color.Blue + 5));
            }
            static string StringFromColor(Enum_TestClass_enum93_Color c)
            {
                return c + " = " + ((int)c).ToString();
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        enum Enum_TestClass_enum94_Color
        {
            Red,
            Green = 10,
            Blue
        }
        enum Enum_TestClass_enum94_ColorB : byte
        {
            Red,
            Green = 10,
            Blue
        }
        enum Enum_TestClass_enum94_ColorSB : sbyte
        {
            Red,
            Green = 10,
            Blue
        }
        enum Enum_TestClass_enum94_ColorS : short
        {
            Red,
            Green = 10,
            Blue
        }
        enum Enum_TestClass_enum94_ColorUS : ushort
        {
            Red,
            Green = 10,
            Blue
        }
        enum Enum_TestClass_enum94_ColorI : int
        {
            Red,
            Green = 10,
            Blue
        }
        enum Enum_TestClass_enum94_ColorUI : uint
        {
            Red,
            Green = 10,
            Blue
        }
        enum Enum_TestClass_enum94_ColorL : long
        {
            Red,
            Green = 10,
            Blue
        }
        enum Enum_TestClass_enum94_ColorUL : ulong
        {
            Red,
            Green = 10,
            Blue
        }
        class Enum_TestClass_enum94
        {
            static void Main_old()
            {
                Log.Comment(StringFromColor((Enum_TestClass_enum94_Color)(-5)));
                Log.Comment(StringFromColor((Enum_TestClass_enum94_Color)1000));
                Log.Comment(StringFromColor((Enum_TestClass_enum94_ColorB)255));
                Log.Comment(StringFromColor((Enum_TestClass_enum94_ColorSB)127));
                Log.Comment(StringFromColor((Enum_TestClass_enum94_ColorSB)(-128)));
                Log.Comment(StringFromColor((Enum_TestClass_enum94_ColorS)(-32768)));
                Log.Comment(StringFromColor((Enum_TestClass_enum94_ColorS)32767));
                Log.Comment(StringFromColor((Enum_TestClass_enum94_ColorUS)65535));
                Log.Comment(StringFromColor((Enum_TestClass_enum94_ColorI)(-32768)));
                Log.Comment(StringFromColor((Enum_TestClass_enum94_ColorI)32767));
                Log.Comment(StringFromColor((Enum_TestClass_enum94_ColorUI)65535));
                Log.Comment(StringFromColor((Enum_TestClass_enum94_ColorL)(-32768)));
                Log.Comment(StringFromColor((Enum_TestClass_enum94_ColorL)32767));
                Log.Comment(StringFromColor((Enum_TestClass_enum94_ColorUL)65535));
            }
            static string StringFromColor(Enum c)
            {
                return c + " = " + c.ToString();
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        [Flags]
        enum Enum_TestClass_enum_flags01_Flag
        {
            Zero = 0x0000,
            First = 0x0001,
            Second = 0x0002,
            Third = 0x0004,
            Fourth = 0x0008,
        }
        public class Enum_TestClass_enum_flags01
        {
            public static int Main_old()
	{
		Enum_TestClass_enum_flags01_Flag r = Enum_TestClass_enum_flags01_Flag.First | Enum_TestClass_enum_flags01_Flag.Fourth;
		Log.Comment(r.ToString());
		return (int)r == (int)0x0009 ? 0 : 1;
	}
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        [Flags]
        enum Enum_TestClass_enum_flags02_Flag
        {
            Zero = 0x0000,
            First = 0x0001,
            Second = 0x0002,
            Third = 0x0004,
            Fourth = 0x0008,
        }
        public class Enum_TestClass_enum_flags02
        {
            public static int Main_old()
	{
		Enum_TestClass_enum_flags02_Flag r = Enum_TestClass_enum_flags02_Flag.First  | Enum_TestClass_enum_flags02_Flag.Fourth;
		r = r | Enum_TestClass_enum_flags02_Flag.Second  | Enum_TestClass_enum_flags02_Flag.Third;

		Log.Comment(r.ToString());
		return (int)r == (int)0x000f ? 0 : 1;
	}
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        [Flags]
        enum Enum_TestClass_enum_flags03_Flag
        {
            Zero = 0x0000,
            First = 0x0001,
            Second = 0x0002,
            Third = 0x0004,
            Fourth = 0x0008,
        }
        public class Enum_TestClass_enum_flags03
        {
            public static int Main_old()
	{
		Enum_TestClass_enum_flags03_Flag r = Enum_TestClass_enum_flags03_Flag.First | Enum_TestClass_enum_flags03_Flag.Fourth;
		r+= 0x00f;	// out of range
		Log.Comment(r.ToString());
		return (int)r == (int)24 ? 0 : 1;
	}
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        [Flags]
        enum Enum_TestClass_enum_flags04_Flag
        {
            Zero = 0x0000,
            First = 0x0001,
            Second = 0x0002,
            Third = 0x0004,
            Fourth = 0x0008,
        }
        public class Enum_TestClass_enum_flags04
        {
            public static int Main_old()
	{
		int i = 0x0f;
		Enum_TestClass_enum_flags04_Flag r  = (Enum_TestClass_enum_flags04_Flag)i;
		Log.Comment(r.ToString());
		return (int)r == (int)0x000f ? 0 : 1;
	}
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }


    }
}
