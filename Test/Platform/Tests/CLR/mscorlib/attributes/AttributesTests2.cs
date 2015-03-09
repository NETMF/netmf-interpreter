////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#define DEBUG
#define DEBUG
using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Microsoft.SPOT.Platform.Tests
{
    public class AttributesTests2 : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");

            // Add your functionality here.                

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
        }

        //Attributes Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\Attributes
        //attrib000,attrib000_2,attrib001,attrib002,attrib017_7,attrib017_7a,attrib017_8c,attrib017_9b,attrib021_2,attrib021_4,attrib029_8,attrib029_9,attrib029_a,attrib029_b,attrib031_4,attrib032_2,attrib033_2,attrib035_12,attrib035_22,attrib036_1,attrib038_1,attrib047_4,attrib047_5,attrib049_4,attrib054,attrib062


        //Test Case Calls 
        
        [TestMethod]
        public MFTestResults Attrib_attrib017_7_Test()
        {
            Log.Comment("17.4.5 Testing Conditional with DEBUG defined.");
            if (Attrib_TestClass_attrib017_7.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Attrib_attrib017_9b_Test()
        {
            Log.Comment("17.4.5 - Conditional not valid on delegate creation.");
            if (Attrib_TestClass_attrib017_9b.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }


        public class Attrib_TestClass_attrib017_7_C1
        {
            [Conditional("DEBUG")]
            public static void M()
            {
                Attrib_TestClass_attrib017_7.retval++;
                Log.Comment("Executed Attrib_TestClass_attrib017_7_C1.M");
            }
        }
        public class Attrib_TestClass_attrib017_7_C2
        {
            public static void Attrib_TestClass_attrib017_7()
            {
                Attrib_TestClass_attrib017_7_C1.M();
            }
        }
        public class Attrib_TestClass_attrib017_7
        {
            public static int retval = 0;
            public static int Main_old()
            {
                Attrib_TestClass_attrib017_7_C2.Attrib_TestClass_attrib017_7();
                if (retval != 0)
                {
                    Log.Comment("PASS");
                    return 0;
                }
                Log.Comment("FAIL");
                return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }

        class Attrib_TestClass_attrib017_9b_C1
        {
            [Conditional("DEBUG")]
            public virtual void M()
            {
                Log.Comment("Class1.M executed");
            }
        }
        class Attrib_TestClass_attrib017_9b_C2 : Attrib_TestClass_attrib017_9b_C1
        {
            public override void M()
            {
                Log.Comment("Class2.M executed");
                base.M();						// base.M is not called!
            }
        }


        class Attrib_TestClass_attrib017_9b_C3
        {
            public static void Attrib_TestClass_attrib017_9b()
            {
                Attrib_TestClass_attrib017_9b_C2 c = new Attrib_TestClass_attrib017_9b_C2();
                c.M();							// Attrib_TestClass_attrib017_9b_C2.M() is called, but Attrib_TestClass_attrib017_9b_C1.M() is not!
                Attrib_TestClass_attrib017_9b_C1 c1 = new Attrib_TestClass_attrib017_9b_C1();
                c1.M();                         // But this time Attrib_TestClass_attrib017_9b_C1.M() is called.
            }
        }
        public class Attrib_TestClass_attrib017_9b
        {
            public static int Main_old()
            {
                Attrib_TestClass_attrib017_9b_C3.Attrib_TestClass_attrib017_9b();
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }


    }
}