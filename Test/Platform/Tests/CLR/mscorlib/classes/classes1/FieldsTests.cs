////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class FieldsTests : IMFTestInterface
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

        //Fields Test methods
        //The Fields*_testMethod() functions are ported from Fields\field*.cs files
        //The following Fields tests were removed because they were build failure tests:
        //9-12,,19,21,25-40,47,48,50,59-62,64
        //Fields Test 42 was removed because Microsoft.SPOT.Math does not contain the abs() function which it uses
        //It fails in the Baseline document
        //Fields Test 63 was removed because the function isVolatile() is not defined
        //It was not included in the Baseline document
        //The FieldsVolatile*_testMethod() functions would be ported from Fields\Volatile\volatile*.cs files
        //but none of them would build because they require 
        //System.Runtime.CompilerServices.isVolatile() which is not included in the Micro Framework
        //Also current Visual Studios docs indicate that all variables are considered volatile
        //They are not included in the Baseline document
        //22,23,24,42
        
        [TestMethod]
        public MFTestResults Fields1_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" A field-declaration may include set of attributes,");
            Log.Comment(" a new modifier, one of four access modifiers, a");
            Log.Comment(" static modifier, and a readonly modifier.  The ");
            Log.Comment(" attributes and modifiers apply to all of the ");
            Log.Comment(" members declared by the field-declaration.");
            if (FieldsTestClass1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Fields2_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" A field-declaration may include set of attributes,");
            Log.Comment(" a new modifier, one of four access modifiers, a");
            Log.Comment(" static modifier, and a readonly modifier.  The ");
            Log.Comment(" attributes and modifiers apply to all of the ");
            Log.Comment(" members declared by the field-declaration.");
            if (FieldsTestClass2.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Fields3_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" A field-declaration may include set of attributes,");
            Log.Comment(" a new modifier, one of four access modifiers, a");
            Log.Comment(" static modifier, and a readonly modifier.  The ");
            Log.Comment(" attributes and modifiers apply to all of the ");
            Log.Comment(" members declared by the field-declaration.");
            if (FieldsTestClass3.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;

        }

        [TestMethod]
        public MFTestResults Fields4_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" A field-declaration may include set of attributes,");
            Log.Comment(" a new modifier, one of four access modifiers, a");
            Log.Comment(" static modifier, and a readonly modifier.  The ");
            Log.Comment(" attributes and modifiers apply to all of the ");
            Log.Comment(" members declared by the field-declaration.");
            if (FieldsTestClass4.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Fields5_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" A field-declaration may include set of attributes,");
            Log.Comment(" a new modifier, one of four access modifiers, a");
            Log.Comment(" static modifier, and a readonly modifier.  The ");
            Log.Comment(" attributes and modifiers apply to all of the ");
            Log.Comment(" members declared by the field-declaration.");
            if (FieldsTestClass5.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Fields6_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" A field-declaration may include set of attributes,");
            Log.Comment(" a new modifier, one of four access modifiers, a");
            Log.Comment(" static modifier, and a readonly modifier.  The ");
            Log.Comment(" attributes and modifiers apply to all of the ");
            Log.Comment(" members declared by the field-declaration.");
            if (FieldsTestClass6.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Fields7_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" A field-declaration may include set of attributes,");
            Log.Comment(" a new modifier, one of four access modifiers, a");
            Log.Comment(" static modifier, and a readonly modifier.  The ");
            Log.Comment(" attributes and modifiers apply to all of the ");
            Log.Comment(" members declared by the field-declaration.");
            if (FieldsTestClass7.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Fields8_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" A field-declaration may include set of attributes,");
            Log.Comment(" a new modifier, one of four access modifiers, a");
            Log.Comment(" static modifier, and a readonly modifier.  The ");
            Log.Comment(" attributes and modifiers apply to all of the ");
            Log.Comment(" members declared by the field-declaration.");
            if (FieldsTestClass8.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Fields13_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" A field-declaration may include set of attributes,");
            Log.Comment(" a new modifier, one of four access modifiers, a");
            Log.Comment(" static modifier, and a readonly modifier.  The ");
            Log.Comment(" attributes and modifiers apply to all of the ");
            Log.Comment(" members declared by the field-declaration.");
            Log.Comment("");
            Log.Comment(" A field declaration that declares multiple fields");
            Log.Comment(" is equivalent to multiple declarations of single ");
            Log.Comment(" fields with the same attributes, modifiers, and type.");
            if (FieldsTestClass13.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Fields14_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" A static field identifies exactly on storage location.");
            Log.Comment(" No matter how many instances of a class are created,");
            Log.Comment(" there is only ever one copy of a static field.");
            if (FieldsTestClass14.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Fields15_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" A static field comes into existence when the ");
            Log.Comment(" type in which it is declared is loaded, and ");
            Log.Comment(" ceases to exist when the type in which it ");
            Log.Comment(" is declared in unloaded.");
            if (FieldsTestClass15.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Fields16_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" Every instance of a class contains a separate copy");
            Log.Comment(" of all instance fields of the class.  An instance ");
            Log.Comment(" field comes into existence when a new instance of ");
            Log.Comment(" its class is created, and ceases to exist when there ");
            Log.Comment(" are no references to that instance and the destructor");
            Log.Comment(" of the instance has executed.");
            if (FieldsTestClass16.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Fields17_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" Every instance of a class contains a separate copy");
            Log.Comment(" of all instance fields of the class.  An instance ");
            Log.Comment(" field comes into existence when a new instance of ");
            Log.Comment(" its class is created, and ceases to exist when there ");
            Log.Comment(" are no references to that instance and the destructor");
            Log.Comment(" of the instance has executed.");
            if (FieldsTestClass17.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Fields18_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" When a field is referenced in a member-access of");
            Log.Comment(" the form E.M, if M is a static field, E must denote");
            Log.Comment(" a type, and if M is an instance field, E must ");
            Log.Comment(" denote an instance.");
            if (FieldsTestClass18.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        
        [TestMethod]
        public MFTestResults Fields20_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" When a field is referenced in a member-access of");
            Log.Comment(" the form E.M, if M is a static field, E must denote");
            Log.Comment(" a type, and if M is an instance field, E must ");
            Log.Comment(" denote an instance.");
            if (FieldsTestClass20.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Fields22_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" When a field-declaration includes a readonly");
            Log.Comment(" modifier, assignments to the fields introduced");
            Log.Comment(" by the declaration can only occur as part of");
            Log.Comment(" the declaration or in a constructor in the");
            Log.Comment(" same class.");
            if (FieldsTestClass22.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Fields23_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" When a field-declaration includes a readonly");
            Log.Comment(" modifier, assignments to the fields introduced");
            Log.Comment(" by the declaration can only occur as part of");
            Log.Comment(" the declaration or in a constructor in the");
            Log.Comment(" same class.");
            if (FieldsTestClass23.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Fields24_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" When a field-declaration includes a readonly");
            Log.Comment(" modifier, assignments to the fields introduced");
            Log.Comment(" by the declaration can only occur as part of");
            Log.Comment(" the declaration or in a constructor in the");
            Log.Comment(" same class.");
            if (FieldsTestClass24.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        
        [TestMethod]
        public MFTestResults Fields41_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" A static readonly field is useful when a symbolic");
            Log.Comment(" name for a constant value is desired, but when the ");
            Log.Comment(" type of the value is not permitted in a const declaration");
            Log.Comment(" or when the value cannot be computed at compile-time");
            Log.Comment(" by a constant expression.");
            if (FieldsTestClass41.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        
        [TestMethod]
        public MFTestResults Fields42_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" Field declarations may include variable-initializers.");
            Log.Comment(" For static fields, varaible initializers correspond to");
            Log.Comment(" assignment statements that are executed when the class");
            Log.Comment(" is loaded. For instance fields, variable initializers");
            Log.Comment(" correspond to assignment statements that are executed");
            Log.Comment(" when an instance of the class is created.");
            Log.Comment("This test has been rewritten to avoid use of the Math.Abs function which the MF does not support");
            if (FieldsTestClass42.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        
        [TestMethod]
        public MFTestResults Fields43_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" The static field variable initializers of a class");
            Log.Comment(" correspond to a sequence of assignments that are ");
            Log.Comment(" executed immediately upon entry to the static");
            Log.Comment(" constructor of a class.  The variable initializers");
            Log.Comment(" are executed in the textual order they appear");
            Log.Comment(" in the class declaration.");
            if (FieldsTestClass43.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Fields44_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" The static field variable initializers of a class");
            Log.Comment(" correspond to a sequence of assignments that are ");
            Log.Comment(" executed immediately upon entry to the static");
            Log.Comment(" constructor of a class.  The variable initializers");
            Log.Comment(" are executed in the textual order they appear");
            Log.Comment(" in the class declaration.");
            if (FieldsTestClass44.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Fields45_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" The static field variable initializers of a class");
            Log.Comment(" correspond to a sequence of assignments that are ");
            Log.Comment(" executed immediately upon entry to the static");
            Log.Comment(" constructor of a class.  The variable initializers");
            Log.Comment(" are executed in the textual order they appear");
            Log.Comment(" in the class declaration.");
            if (FieldsTestClass45.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Fields46_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" The instance field variable initializers of a class");
            Log.Comment(" correspond to a sequence of assignments that are ");
            Log.Comment(" executed immediately upon entry to one of the instance");
            Log.Comment(" constructors of the class.");
            if (FieldsTestClass46.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Fields49_testMethod()
        {
            Log.Comment(" A variable initializer for an instance field");
            Log.Comment(" cannot reference the instance being created.");
            Log.Comment(" Thus, it is an error to reference this in a ");
            Log.Comment(" variable initializer, as it is an error for");
            Log.Comment(" a variable initialzer to reference any instance");
            Log.Comment(" member through a simple-name.");
            if (FieldsTestClass49.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
                
        [TestMethod]
        public MFTestResults Fields51_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" Specifically, assignments to a readonly field");
            Log.Comment(" are permitted only in the following context.");
            Log.Comment(" ...");
            Log.Comment(" For an instance field, in the instance constructors");
            Log.Comment(" of the class that contains the field declaration, or");
            Log.Comment(" for a static field, in the static constructor of the");
            Log.Comment(" class the contains the field declaration. These are also");
            Log.Comment(" contexts in which it is valid to pass a readonly field");
            Log.Comment(" as an out or ref parameter.");
            if (FieldsTestClass51.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
                
        [TestMethod]
        public MFTestResults Fields52_testMethod()
        {
            Log.Comment(" Section 10.4");
            Log.Comment(" Specifically, assignments to a readonly field");
            Log.Comment(" are permitted only in the following context."); 
            Log.Comment(" ...");
            Log.Comment(" For an instance field, in the instance constructors");
            Log.Comment(" of the class that contains the field declaration, or");
            Log.Comment(" for a static field, in the static constructor of the");
            Log.Comment(" class the contains the field declaration. These are also");
            Log.Comment(" contexts in which it is valid to pass a readonly field");
            Log.Comment(" as an out or ref parameter.");
            if (FieldsTestClass52.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
                
        [TestMethod]
        public MFTestResults Fields53_testMethod()
        {
            Log.Comment("Testing bools assigned with (x == y)");
            if (FieldsTestClass53.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
                
        [TestMethod]
        public MFTestResults Fields54_testMethod()
        {
            Log.Comment("Testing bools assigned with function calls");
            if (FieldsTestClass54.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
                
        [TestMethod]
        public MFTestResults Fields55_testMethod()
        {
            Log.Comment("Testing bools assigned with conditionals");
            if (FieldsTestClass55.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
                
        [TestMethod]
        public MFTestResults Fields56_testMethod()
        {
            Log.Comment("Testing ints assigned with function calls");
            if (FieldsTestClass56.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
                
        [TestMethod]
        public MFTestResults Fields57_testMethod()
        {
            Log.Comment("Testing strings assigned with \"x\" + \"y\"");
            if (FieldsTestClass57.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
                
        [TestMethod]
        public MFTestResults Fields58_testMethod()
        {
            Log.Comment("Testing strings assigned with function calls");
            if (FieldsTestClass58.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        class FieldsTestClass1
        {

            int intI = 2;

            public static bool testMethod()
            {

                FieldsTestClass1 test = new FieldsTestClass1();
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

        class FieldsTestClass2_Base
        {
            public int intI = 1;
        }

        class FieldsTestClass2 : FieldsTestClass2_Base
        {

            new int intI = 2;

            public static bool testMethod()
            {

                FieldsTestClass2 test = new FieldsTestClass2();
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

        class FieldsTestClass3
        {

            public int intI = 2;

            public static bool testMethod()
            {

                FieldsTestClass3 test = new FieldsTestClass3();
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

        class FieldsTestClass4
        {

            protected int intI = 2;

            public static bool testMethod()
            {

                FieldsTestClass4 test = new FieldsTestClass4();
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

        class FieldsTestClass5
        {

            internal int intI = 2;

            public static bool testMethod()
            {

                FieldsTestClass5 test = new FieldsTestClass5();
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

        class FieldsTestClass6
        {

            private int intI = 2;

            public static bool testMethod()
            {

                FieldsTestClass6 test = new FieldsTestClass6();
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

        class FieldsTestClass7
        {

            static int intI = 2;

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

        class FieldsTestClass8
        {

            readonly int intI = 2;

            public static bool testMethod()
            {

                FieldsTestClass8 test = new FieldsTestClass8();
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

        class FieldsTestClass13_Base
        {
            public int intI = 2, intJ = 3;
        }

        class FieldsTestClass13
        {

            public static bool testMethod()
            {

                FieldsTestClass13_Base test = new FieldsTestClass13_Base();
                if ((test.intI == 2) && (test.intJ == 3))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class FieldsTestClass14
        {

            static int intI = 1;

            public void ChangeInt(int intJ)
            {
                intI = intJ;
            }


            public static bool testMethod()
            {

                FieldsTestClass14 c1 = new FieldsTestClass14();
                c1.ChangeInt(2);
                FieldsTestClass14 c2 = new FieldsTestClass14();
                c1.ChangeInt(3);
                FieldsTestClass14 c3 = new FieldsTestClass14();
                c1.ChangeInt(4);

                if (intI == 4)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class FieldsTestClass15_Base
        {
            public static int intI = 1;
        }

        class FieldsTestClass15
        {

            public static bool testMethod()
            {

                if (FieldsTestClass15_Base.intI == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class FieldsTestClass16
        {


            int intI = 1;

            public void ChangeInt(int intJ)
            {
                intI = intJ;
            }

            public static bool testMethod()
            {

                FieldsTestClass16 c1 = new FieldsTestClass16();
                c1.ChangeInt(2);
                FieldsTestClass16 c2 = new FieldsTestClass16();
                c2.ChangeInt(3);
                FieldsTestClass16 c3 = new FieldsTestClass16();
                c3.ChangeInt(4);

                if ((c1.intI == 2) && (c2.intI == 3) && (c3.intI == 4))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class FieldsTestClass17_Base
        {
            public int intI = 1;
        }

        class FieldsTestClass17
        {

            FieldsTestClass17_Base tc;

            public static bool testMethod()
            {
                try
                {
                    bool RetVal = false;

                    FieldsTestClass17 test = new FieldsTestClass17();

                    try
                    {
                        int intJ = test.tc.intI; //MyTest hasn't been instantiated
                    }
                    catch (System.Exception e)
                    {
                        RetVal = true;
                    }
                    return RetVal;
                }
                catch { return false; }
            }
        }

        class FieldsTestClass18
        {

            static int intI = 2;

            public static bool testMethod()
            {
                if (FieldsTestClass18.intI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class FieldsTestClass20
        {

            int intI = 2;

            public static bool testMethod()
            {
                FieldsTestClass20 test = new FieldsTestClass20();
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

        enum FieldsTestClass22_Enum { a = 1, b = 2 }

        struct FieldsTestClass22_Struct
        {
            public FieldsTestClass22_Struct(int intI)
            {
                Test = intI;
            }
            public int Test;
        }

        struct FieldsTestClass22_Sub
        {
            public FieldsTestClass22_Sub(int intI)
            {
                Test = intI;
            }
            public int Test;
        }

        class FieldsTestClass22
        {

            readonly int intI = 2;
            readonly string strS = "MyString";
            readonly FieldsTestClass22_Enum enuE = FieldsTestClass22_Enum.a;
            readonly FieldsTestClass22_Struct sctS = new FieldsTestClass22_Struct(3);
            readonly FieldsTestClass22_Sub clsC = new FieldsTestClass22_Sub(4);

            public static bool testMethod()
            {

                FieldsTestClass22 MC = new FieldsTestClass22();

                if ((MC.intI == 2) && (MC.strS.Equals("MyString")) && (MC.enuE == FieldsTestClass22_Enum.a) && (MC.sctS.Test == 3) && (MC.clsC.Test == 4))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        enum FieldsTestClass23_Enum { a = 1, b = 2 }

        struct FieldsTestClass23_Struct
        {
            public FieldsTestClass23_Struct(int intI)
            {
                Test = intI;
            }
            public int Test;
        }

        struct FieldsTestClass23_Sub
        {
            public FieldsTestClass23_Sub(int intI)
            {
                Test = intI;
            }
            public int Test;
        }

        class FieldsTestClass23
        {

            public FieldsTestClass23()
            {
                intI = 2;
                strS = "MyString";
                enuE = FieldsTestClass23_Enum.a;
                sctS = new FieldsTestClass23_Struct(3);
                clsC = new FieldsTestClass23_Sub(4);
            }

            readonly int intI;
            readonly string strS;
            readonly FieldsTestClass23_Enum enuE;
            readonly FieldsTestClass23_Struct sctS;
            readonly FieldsTestClass23_Sub clsC;

            public static bool testMethod()
            {

                FieldsTestClass23 MC = new FieldsTestClass23();

                if ((MC.intI == 2) && (MC.strS.Equals("MyString")) && (MC.enuE == FieldsTestClass23_Enum.a) && (MC.sctS.Test == 3) && (MC.clsC.Test == 4))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        enum FieldsTestClass24_Enum { a = 1, b = 2 }

        struct FieldsTestClass24_Struct
        {
            public FieldsTestClass24_Struct(int intI)
            {
                Test = intI;
            }
            public int Test;
        }

        struct FieldsTestClass24_Sub
        {
            public FieldsTestClass24_Sub(int intI)
            {
                Test = intI;
            }
            public int Test;
        }

        class FieldsTestClass24
        {

            public FieldsTestClass24()
            {
                intI = 2;
                strS = "MyString";
                enuE = FieldsTestClass24_Enum.a;
                sctS = new FieldsTestClass24_Struct(3);
                clsC = new FieldsTestClass24_Sub(4);
            }

            readonly int intI = 3;
            readonly string strS = "FooBar";
            readonly FieldsTestClass24_Enum enuE = FieldsTestClass24_Enum.b;
            readonly FieldsTestClass24_Struct sctS = new FieldsTestClass24_Struct(2);
            readonly FieldsTestClass24_Sub clsC = new FieldsTestClass24_Sub(5);

            public static bool testMethod()
            {

                FieldsTestClass24 MC = new FieldsTestClass24();

                if ((MC.intI == 2) && (MC.strS.Equals("MyString")) && (MC.enuE == FieldsTestClass24_Enum.a) && (MC.sctS.Test == 3) && (MC.clsC.Test == 4))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        public class FieldsTestClass41
        {

            public static readonly FieldsTestClass41 Black = new FieldsTestClass41(0, 0, 0);
            public static readonly FieldsTestClass41 White = new FieldsTestClass41(255, 255, 255);
            public static readonly FieldsTestClass41 Red = new FieldsTestClass41(255, 0, 0);
            public static readonly FieldsTestClass41 Green = new FieldsTestClass41(0, 255, 0);
            public static readonly FieldsTestClass41 Blue = new FieldsTestClass41(0, 0, 255);

            private byte red, green, blue;

            public FieldsTestClass41(byte r, byte g, byte b)
            {
                red = r;
                green = g;
                blue = b;
            }

            public void getRGB(out byte r1, out byte g1, out byte b1)
            {
                r1 = red;
                g1 = green;
                b1 = blue;
            }

            public static bool testMethod()
            {
                FieldsTestClass41 wht = FieldsTestClass41.White;
                byte r2, g2, b2;
                wht.getRGB(out r2, out g2, out b2);
                if ((r2 == 255) && (g2 == 255) && (b2 == 255))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class FieldsTestClass42
        {
            static int x = Math.Cos(4-2) ;
            int i = 100;
            string s = "Hello";

            public static bool testMethod()
            {
                try
                {
                    FieldsTestClass42 t = new FieldsTestClass42();
                    if ((x == Math.Cos(2)) && (t.i == 100) && (t.s.Equals("Hello")))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch 
                {
                    return false;
                }
            }
        }
        public class FieldsTestClass43
        {

            static int intI;
            static int intJ = 2;

            //intJ should be initialized before we enter the static constructor
            static FieldsTestClass43()
            {
                intI = intJ;
            }

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

        public class FieldsTestClass44
        {

            static int intI = 2;
            //intI is initialized before intJ
            static int intJ = intI + 3;

            public static bool testMethod()
            {
                if (intJ == 5)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        public class FieldsTestClass45
        {

            //intI is initialized after intJ
            static int intJ = intI + 3;
            static int intI = 2;

            public static bool testMethod()
            {
                if (intJ == 3)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        public class FieldsTestClass46
        {

            int intI = 2;
            int intJ;
            int intK;

            public FieldsTestClass46()
            {
                //int I should already be initialized
                intJ = intI;
            }

            public FieldsTestClass46(int DummyInt)
            {
                //int I should already be initialized
                intK = intI;
            }

            public static bool testMethod()
            {
                FieldsTestClass46 test1 = new FieldsTestClass46();
                FieldsTestClass46 test2 = new FieldsTestClass46(0);
                if ((test1.intJ == 2) && (test2.intK == 2))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        public class FieldsTestClass49
        {

            public static int intI = 2;
            public int intK = intI;

            public static bool testMethod()
            {
                FieldsTestClass49 test = new FieldsTestClass49();
                if (test.intK == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class FieldsTestClass51
        {

            FieldsTestClass51()
            {
                MyMeth1(ref intI);
                MyMeth2(out intJ);
            }

            public static void MyMeth1(ref int i)
            {
                i = 2;
            }

            public static void MyMeth2(out int j)
            {
                j = 3;
            }


            public readonly int intI;
            public readonly int intJ;


            public static bool testMethod()
            {
                FieldsTestClass51 mc = new FieldsTestClass51();
                if ((mc.intI == 2) && (mc.intJ == 3))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class FieldsTestClass52
        {

            static FieldsTestClass52()
            {
                MyMeth1(ref intI);
                MyMeth2(out intJ);
            }

            public static void MyMeth1(ref int i)
            {
                i = 2;
            }

            public static void MyMeth2(out int j)
            {
                j = 3;
            }


            public static readonly int intI;
            public static readonly int intJ;


            public static bool testMethod()
            {
                if ((FieldsTestClass52.intI == 2) && (FieldsTestClass52.intJ == 3))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class FieldsTestClass53
        {

            public static bool b1 = (3 == 3);
            public static bool b2 = (3 == 4);
            public bool b3 = (3 == 3);
            public bool b4 = (3 == 4);

            public static bool testMethod()
            {

                FieldsTestClass53 mc = new FieldsTestClass53();

                if ((b1 == true) && (b2 == false) && (mc.b3 == true) && (mc.b4 == false))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class FieldsTestClass54
        {

            public static bool b1 = RetTrue();
            public static bool b2 = RetFalse();
            public bool b3 = RetTrue();
            public bool b4 = RetFalse();

            public static bool RetTrue()
            {
                return true;
            }
            public static bool RetFalse()
            {
                return false;
            }

            public static bool testMethod()
            {

                FieldsTestClass54 mc = new FieldsTestClass54();

                if ((b1 == true) && (b2 == false) && (mc.b3 == true) && (mc.b4 == false))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class FieldsTestClass55
        {

            public static int i1 = (3 & 6);
            public static int i2 = (3 | 6);
            public int i3 = (3 & 6);
            public int i4 = (3 | 6);

            public static bool testMethod()
            {

                FieldsTestClass55 mc = new FieldsTestClass55();

                if ((i1 == 2) && (i2 == 7) && (mc.i3 == 2) && (mc.i4 == 7))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class FieldsTestClass56
        {

            public static int i1 = Ret2();
            public static int i2 = Ret7();
            public int i3 = Ret2();
            public int i4 = Ret7();

            public static int Ret2()
            {
                return 2;
            }

            public static int Ret7()
            {
                return 7;
            }

            public static bool testMethod()
            {

                FieldsTestClass56 mc = new FieldsTestClass56();

                if ((i1 == 2) && (i2 == 7) && (mc.i3 == 2) && (mc.i4 == 7))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class FieldsTestClass57
        {

            public static string s1 = "foo" + "bar";
            public static string s2 = "bar" + "foo";
            public string s3 = "foo" + "bar";
            public string s4 = "bar" + "foo";

            public static bool testMethod()
            {

                FieldsTestClass57 mc = new FieldsTestClass57();

                if ((s1 == "foobar") && (s2 == "barfoo") && (mc.s3 == "foobar") && (mc.s4 == "barfoo"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class FieldsTestClass58
        {

            public static string s1 = Ret1();
            public static string s2 = Ret2();
            public string s3 = Ret1();
            public string s4 = Ret2();

            public static string Ret1()
            {
                return "foobar";
            }

            public static string Ret2()
            {
                return "barfoo";
            }

            public static bool testMethod()
            {
                try
                {
                    FieldsTestClass58 mc = new FieldsTestClass58();

                    if ((s1 == "foobar") && (s2 == "barfoo") && (mc.s3 == "foobar") && (mc.s4 == "barfoo"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch { return false; }
            }
        }
        
    }
}
       
