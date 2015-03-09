////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class DelegatesTests : IMFTestInterface
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

        //Delegates Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\Delegates
        //delegate01,delegate02,delegate03,delegate04,delegate05,delegate06,delegate07,delegate08,delegate09,delegate10,delegate11,delegate12,delegate13,delegate14,delegate14a,delegate14b,delegate14c,delegate15,delegate16,delegate17,delegate18,delegate19,delegate20,delegate21,delegate23,delegate24,delegate25,delegate26,delegate28,delegate30,delegate31,delegate32,delegate34,delegate36,delegate60,delegate62,delegate64,delegate65,delegate66,delegate70,delegate71,delegate72,delegate73,delegate74,delegate75,delegate76,delegate77,delegate78,delegate79,delegate80,delegate81,delegate_modifier09,delegate_modifier10,delegate_modifier11,delegate_modifier12

        //Test Case Calls 
        [TestMethod]
        public MFTestResults Delegate_delegate01_Test()
        {
            Log.Comment(" Verify that both static and instance methods can be called from delegates.");
            if (Delegate_TestClass_delegate01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate02_Test()
        {
            Log.Comment(" Verify that delegate can be initialized to null.");
            if (Delegate_TestClass_delegate02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate03_Test()
        {
            Log.Comment(" Verify that delegate can be initialized to null.");
            if (Delegate_TestClass_delegate03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate04_Test()
        {
            Log.Comment(" Verify that delegate can be assigned by ternary operator.");
            if (Delegate_TestClass_delegate04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate05_Test()
        {
            Log.Comment(" Verify that delegate can be assigned by ternary operator.");
            if (Delegate_TestClass_delegate05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate06_Test()
        {
            Log.Comment(" Verify that delegate can be assigned by ternary operator.  Assign null instead of usable value.");
            if (Delegate_TestClass_delegate06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate07_Test()
        {
            Log.Comment(" Verify that delegate can be assigned by ternary operator.  Use null instead of a usable value.");
            if (Delegate_TestClass_delegate07.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate08_Test()
        {
            Log.Comment(" Verify that delegate can be assigned by ternary operator.  Assign null instead of usable value.");
            if (Delegate_TestClass_delegate08.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate09_Test()
        {
            Log.Comment(" Verify that delegates can be compared for equality.");
            if (Delegate_TestClass_delegate09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate10_Test()
        {
            Log.Comment(" Verify that delegates can be aggregated in arrays and compared for equality.");
            if (Delegate_TestClass_delegate10.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate11_Test()
        {
            Log.Comment(" Verify that delegates can be members of classes.");
            if (Delegate_TestClass_delegate11.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate12_Test()
        {
            Log.Comment(" Verify that both static and instance methods can be called from delegates.");
            if (Delegate_TestClass_delegate12.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate13_Test()
        {
            Log.Comment(" Verify that delegate can be initialized to null.");
            if (Delegate_TestClass_delegate13.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate14_Test()
        {
            Log.Comment(" Verify that delegate can be initialized to null.");
            if (Delegate_TestClass_delegate14.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate14a_Test()
        {
            Log.Comment(" Verify that delegate can be initialized to null.");
            if (Delegate_TestClass_delegate14a.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate14b_Test()
        {
            Log.Comment(" Verify that delegate can be initialized to null.");
            if (Delegate_TestClass_delegate14b.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate14c_Test()
        {
            Log.Comment(" Verify that delegate can be initialized to null.");
            if (Delegate_TestClass_delegate14c.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate15_Test()
        {
            Log.Comment(" Verify that delegate can be assigned by ternary operator.");
            if (Delegate_TestClass_delegate15.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate16_Test()
        {
            Log.Comment(" Verify that delegate can be assigned by ternary operator.");
            if (Delegate_TestClass_delegate16.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate17_Test()
        {
            Log.Comment(" Verify that delegate can be assigned by ternary operator.  Assign null instead of usable value.");
            if (Delegate_TestClass_delegate17.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate18_Test()
        {
            Log.Comment(" Make sure a delegate list filled with nulls is detectable as foo == null");
            if (Delegate_TestClass_delegate18.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate19_Test()
        {
            Log.Comment(" Verify that delegates can be aggregated in arrays and compared for equality.");
            if (Delegate_TestClass_delegate19.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate20_Test()
        {
            Log.Comment(" Verify that delegates can be compared for equality.");
            if (Delegate_TestClass_delegate20.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate21_Test()
        {
            Log.Comment(" Verify that delegates can be aggregated in arrays and compared for equality.");
            if (Delegate_TestClass_delegate21.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate23_Test()
        {
            Log.Comment(" Verify that delegates can be aggregated using the + operator;");
            if (Delegate_TestClass_delegate23.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate24_Test()
        {
            Log.Comment(" Verify that delegates can be aggregated using the + operator;");
            if (Delegate_TestClass_delegate24.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate25_Test()
        {
            Log.Comment(" Verify that delegates can be removed using the - operator;");
            if (Delegate_TestClass_delegate25.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Delegate_delegate26_Test()
        {
            Log.Comment("Bug 214780 - async delegates to methods with out parameters don't work");
            Log.Comment("This test is expected to fail.");
            if (Delegate_TestClass_delegate26.testMethod())
            {
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;
        }
        /* This test is skipped because it causes a Metadat processor error, it fails in the baseline.
        [TestMethod]
        public MFTestResults Delegate_delegate28_Test()
        {
            Log.Comment("Verify Delegate with 257 args.  This is because more than 257 causes the");
            Log.Comment("compiler to take a different code path.");
            if (Delegate_TestClass_delegate28.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
         */
        [TestMethod]
        public MFTestResults Delegate_delegate30_Test()
        {
            Log.Comment(" Verify that both static and instance struct methods can be called from delegates.");
            if (Delegate_TestClass_delegate30.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate31_Test()
        {
            Log.Comment(" Verify that virtual struct methods can be called from delegates.");
            if (Delegate_TestClass_delegate31.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        /*These are skipped due to their use of Volatile variables which are not supported
         *         
        [TestMethod]
        public MFTestResults Delegate_delegate32_Test()
        {
            Log.Comment("Delegate Invocation using BeginInvoke");
            if (Delegate_TestClass_delegate32.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate34_Test()
        {
            Log.Comment("Delegate Invocation using BeginInvoke");
            if (Delegate_TestClass_delegate34.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        */
        [TestMethod]
        public MFTestResults Delegate_delegate36_Test()
        {
            Log.Comment("params modifier should not be considered when matching a delegate with a method");
            if (Delegate_TestClass_delegate36.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate60_Test()
        {
            Log.Comment("A delegate declaration defines a class that derives from System.Delegate");
            if (Delegate_TestClass_delegate60.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate62_Test()
        {
            Log.Comment("The compiler is expected to warn that the new keyword id not rquired as we're not hiding an inherited member");
            if (Delegate_TestClass_delegate62.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate64_Test()
        {
            Log.Comment("Compiler is expected to warn when new is not used when hiding a member delegate of the base class");
            if (Delegate_TestClass_delegate64.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate65_Test()
        {
            Log.Comment("Make sure delegates can be hidden.");
            if (Delegate_TestClass_delegate65.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        /*These tests are skipped because the use the == operator in an unsupported way
         * 
        [TestMethod]
        public MFTestResults Delegate_delegate66_Test()
        {
            Log.Comment("Two compatible delegate types can be compared for equality.");
            if (Delegate_TestClass_delegate66.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate70_Test()
        {
            Log.Comment("Two compatible delegate types can be compared for equality (or inequality).");
            if (Delegate_TestClass_delegate70.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
         */
        [TestMethod]
        public MFTestResults Delegate_delegate71_Test()
        {
            Log.Comment("Verify simple +=");
            if (Delegate_TestClass_delegate71.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate72_Test()
        {
            Log.Comment(" Verify that delegates can be removed using the -= operator;");
            if (Delegate_TestClass_delegate72.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        /* These tests are skipped because they use == in an unsupported way
         * 
        [TestMethod]
        public MFTestResults Delegate_delegate73_Test()
        {
            Log.Comment("Verify equality and inequality after using += and -= on delegates");
            if (Delegate_TestClass_delegate73.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate74_Test()
        {
            Log.Comment("Verify ability to call members of System.Delegate on delegate types");
            if (Delegate_TestClass_delegate74.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate75_Test()
        {
            Log.Comment("Verify ability to call members of System.Delegate on delegate types");
            Log.Comment("and that ordinality is maintained in concatenated invocation lists");
            if (Delegate_TestClass_delegate75.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate76_Test()
        {
            Log.Comment("Verify ability to call members of System.Delegate on delegate types");
            Log.Comment("and that ordinality is maintained in concatenated invocation lists");
            if (Delegate_TestClass_delegate76.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
         */ 
        [TestMethod]
        public MFTestResults Delegate_delegate77_Test()
        {
            Log.Comment("Verify that ordinality is maintained in concatenated invocation lists");
            Log.Comment("and that the invocation list members are called synchronously");
            if (Delegate_TestClass_delegate77.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate78_Test()
        {
            Log.Comment("Verify that ordinality is maintained in concatenated invocation lists");
            Log.Comment("and that the invocation list members are called synchronously and");
            Log.Comment("that ref parameters are modified through the invocation chain.");
            if (Delegate_TestClass_delegate78.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate79_Test()
        {
            Log.Comment("Verify that ordinality is maintained in concatenated invocation lists");
            Log.Comment("and that the invocation list members are called synchronously and");
            Log.Comment("that out parameters are set by the last member in the invocation chain.");
            if (Delegate_TestClass_delegate79.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate80_Test()
        {
            Log.Comment("Verify that System.Exceptions not caught in invoked method are bubbled up");
            Log.Comment("and the remaining methods in the invocation list are not invoked.");
            if (Delegate_TestClass_delegate80.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate81_Test()
        {
            Log.Comment("Sample from section 15.3 of Delegate_TestClass_?_A#LS");
            if (Delegate_TestClass_delegate81.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate_modifier09_Test()
        {
            Log.Comment("only new, public, private, protected and internal are allowed as modifiers");
            if (Delegate_TestClass_delegate_modifier09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate_modifier10_Test()
        {
            Log.Comment("only new, public, private, protected and internal are allowed as modifiers");
            if (Delegate_TestClass_delegate_modifier10.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate_modifier11_Test()
        {
            Log.Comment("only new, public, private, protected and internal are allowed as modifiers");
            if (Delegate_TestClass_delegate_modifier11.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Delegate_delegate_modifier12_Test()
        {
            Log.Comment("only new, public, private, protected and internal are allowed as modifiers");
            if (Delegate_TestClass_delegate_modifier12.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }



        //Compiled Test Cases 
        delegate int Delegate_TestClass_delegate01_1();
        public class Delegate_TestClass_delegate01_2
        {
            public int bar() { Log.Comment("bar"); return 0x01; }
            static public int far()
            {
                Log.Comment("far");
                return 0x02;
            }
        }
        public class Delegate_TestClass_delegate01
        {
            static int retval = 0x03;

            static public int Main_old()
            {
                Delegate_TestClass_delegate01_2 p = new Delegate_TestClass_delegate01_2();
                Delegate_TestClass_delegate01_1 foo = new Delegate_TestClass_delegate01_1(p.bar);
                retval -= foo();
                foo = new Delegate_TestClass_delegate01_1(Delegate_TestClass_delegate01_2.far);
                retval -= foo();
                if (retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate int Delegate_TestClass_delegate02_1();
        public class Delegate_TestClass_delegate02_2
        {
            public int bar() { Log.Comment("bar"); return 0x01; }
            static public int far()
            {
                Log.Comment("far");
                return 0x02;
            }
        }
        public class Delegate_TestClass_delegate02
        {
            static int retval = 0x03;

            static public int Main_old()
            {
                Delegate_TestClass_delegate02_2 p = new Delegate_TestClass_delegate02_2();
                Delegate_TestClass_delegate02_1 foo = null;
                foo = new Delegate_TestClass_delegate02_1(p.bar);
                retval -= foo();
                foo = null;
                foo = new Delegate_TestClass_delegate02_1(Delegate_TestClass_delegate02_2.far);
                retval -= foo();
                if (retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate int Delegate_TestClass_delegate03_1();
        public class Delegate_TestClass_delegate03_2
        {
            public static int retval = 0x03;
            public int bar() { Log.Comment("bar"); return 0x01; }
            static public int far()
            {
                Log.Comment("far");
                return 0x02;
            }
        }
        public class Delegate_TestClass_delegate03
        {
            static public int Main_old()
            {
                Delegate_TestClass_delegate03_1 foo = null;
                try
                {
                    foo();
                }
                catch (System.Exception)
                {
                    Delegate_TestClass_delegate03_2.retval -= 0x03;
                }
                if (Delegate_TestClass_delegate03_2.retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return Delegate_TestClass_delegate03_2.retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate int Delegate_TestClass_delegate04_1();
        public class Delegate_TestClass_delegate04_2
        {
            public int bar() { Log.Comment("bar"); return 0x01; }
            static public int far()
            {
                Log.Comment("far");
                return 0x02;
            }
        }
        public class Delegate_TestClass_delegate04
        {
            static int retval = 0x03;
            static bool loo() { return true; }

            static public int Main_old()
        {
        Delegate_TestClass_delegate04_2 p = new Delegate_TestClass_delegate04_2();
        Delegate_TestClass_delegate04_1 foo = null;
        foo = loo() ? new Delegate_TestClass_delegate04_1(p.bar): new Delegate_TestClass_delegate04_1(Delegate_TestClass_delegate04_2.far);
        retval -= foo();
        foo = !loo() ? new Delegate_TestClass_delegate04_1(p.bar): new Delegate_TestClass_delegate04_1(Delegate_TestClass_delegate04_2.far);
        retval -= foo();
        if (retval == 0) Log.Comment ("PASS");
        else Log.Comment ("FAIL");
        return retval;
        }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate int Delegate_TestClass_delegate05_1();
        public class Delegate_TestClass_delegate05_2
        {
            public int bar() { Log.Comment("bar"); return 0x01; }
            static public int far()
            {
                Log.Comment("far");
                return 0x02;
            }
        }
        public class Delegate_TestClass_delegate05
        {
            static int retval = 0x03;
            static bool loo() { return true; }

            static public int Main_old()
        {
        Delegate_TestClass_delegate05_2 p = new Delegate_TestClass_delegate05_2();
        Delegate_TestClass_delegate05_1 foo = null;
        foo = loo() ? new Delegate_TestClass_delegate05_1(p.bar): null;
        retval -= foo();
        foo = !loo() ? null: new Delegate_TestClass_delegate05_1(Delegate_TestClass_delegate05_2.far);
        retval -= foo();
        if (retval == 0) Log.Comment ("PASS");
        else Log.Comment ("FAIL");
        return retval;
        }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate int Delegate_TestClass_delegate06_1();
        public class Delegate_TestClass_delegate06_2
        {
            public int bar() { Log.Comment("bar"); return 0x01; }
            static public int far()
            {
                Log.Comment("far");
                return 0x02;
            }
        }
        public class Delegate_TestClass_delegate06
        {
            static int retval = 0x03;
            static bool loo() { return true; }

            static public int Main_old()
        {
        Delegate_TestClass_delegate06_2 p = new Delegate_TestClass_delegate06_2();
        Delegate_TestClass_delegate06_1 foo = null;
        try
            {
            foo = loo() ? null: new Delegate_TestClass_delegate06_1(Delegate_TestClass_delegate06_2.far);
            retval -= foo();
            }
        catch (System.Exception n)
            {
            retval -= 0x01;
            }
        try
            {
            foo = !loo() ? new Delegate_TestClass_delegate06_1(p.bar): null;
            retval -= foo();
            }
        catch (System.Exception n)
            {
            retval -= 0x02;
            }
        if (retval == 0) Log.Comment ("PASS");
        else Log.Comment ("FAIL");
        return retval;
        }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate int Delegate_TestClass_delegate07_1();
        public class Delegate_TestClass_delegate07_2
        {
            public int bar() { Log.Comment("bar"); return 0x01; }
            static public int far()
            {
                Log.Comment("far");
                return 0x02;
            }
        }
        public class Delegate_TestClass_delegate07
        {
            static int retval = 0x03;
            static bool loo() { return true; }

            static public int Main_old()
        {
        Delegate_TestClass_delegate07_2 p = new Delegate_TestClass_delegate07_2();
        Delegate_TestClass_delegate07_1 foo = null;
        foo = loo() ? new Delegate_TestClass_delegate07_1(p.bar): new Delegate_TestClass_delegate07_1(Delegate_TestClass_delegate07_2.far);
        retval -= foo();
        try {
        foo = !loo() ? new Delegate_TestClass_delegate07_1(p.bar): null;
        retval -= foo();
        }
        catch (System.Exception n)
            {
            retval -= 0x02;
            }
        if (retval == 0) Log.Comment ("PASS");
        else Log.Comment ("FAIL");
        return retval;
        }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate int Delegate_TestClass_delegate08_1();
        public class Delegate_TestClass_delegate08_2
        {
            public int bar() { Log.Comment("bar"); return 0x01; }
            static public int far()
            {
                Log.Comment("far");
                return 0x02;
            }
        }
        public class Delegate_TestClass_delegate08
        {
            static int retval = 0x03;
            static bool loo() { return true; }

            static public int Main_old()
        {
        Delegate_TestClass_delegate08_2 p = new Delegate_TestClass_delegate08_2();
        Delegate_TestClass_delegate08_1 foo = null;
        foo = loo() ? null: new Delegate_TestClass_delegate08_1(Delegate_TestClass_delegate08_2.far);
        if (foo == null)
            foo = new Delegate_TestClass_delegate08_1(p.bar);
        retval -= foo();
        foo = !loo() ? new Delegate_TestClass_delegate08_1(p.bar): null;
        if (foo == null)
            foo = new Delegate_TestClass_delegate08_1(Delegate_TestClass_delegate08_2.far);
        retval -= foo();
        if (retval == 0) Log.Comment ("PASS");
        else Log.Comment ("FAIL");
        return retval;
        }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate int Delegate_TestClass_delegate09_1();
        public class Delegate_TestClass_delegate09_2
        {
            public int bar() { Log.Comment("bar"); return 0x01; }
            static public int far()
            {
                Log.Comment("far");
                return 0x02;
            }
        }
        public class Delegate_TestClass_delegate09
        {
            static int retval = 0x0F;
            static bool loo() { return true; }

            static public int Main_old()
            {
                Delegate_TestClass_delegate09_2 p = new Delegate_TestClass_delegate09_2();
                Delegate_TestClass_delegate09_1 foo1 = null;
                Delegate_TestClass_delegate09_1 foo2 = null;
                foo1 = new Delegate_TestClass_delegate09_1(Delegate_TestClass_delegate09_2.far);
                foo2 = new Delegate_TestClass_delegate09_1(Delegate_TestClass_delegate09_2.far);
                if (foo1 == foo2)
                    retval -= 0x04;
                retval -= foo1();
                foo1 = new Delegate_TestClass_delegate09_1(p.bar);
                foo2 = new Delegate_TestClass_delegate09_1(p.bar);
                if (foo1 == foo2)
                    retval -= 0x08;
                retval -= foo2();
                Log.Comment(retval.ToString());
                if (retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate int Delegate_TestClass_delegate10_1();
        public class Delegate_TestClass_delegate10_2
        {
            public int bar() { Log.Comment("bar"); return 0x01; }
            static public int far()
            {
                Log.Comment("far");
                return 0x02;
            }
        }
        public class Delegate_TestClass_delegate10
        {
            static int retval = 0x0F;
            static bool loo() { return true; }

            static public int Main_old()
            {
                Delegate_TestClass_delegate10_2 p = new Delegate_TestClass_delegate10_2();
                Delegate_TestClass_delegate10_1[] foo = { new Delegate_TestClass_delegate10_1(Delegate_TestClass_delegate10_2.far), new Delegate_TestClass_delegate10_1(Delegate_TestClass_delegate10_2.far) };
                if (foo[0] == foo[1])
                    retval -= 0x04;
                retval -= foo[1]();
                foo[0] = new Delegate_TestClass_delegate10_1(p.bar);
                foo[1] = new Delegate_TestClass_delegate10_1(p.bar);
                if (foo[0] == foo[1])
                    retval -= 0x08;
                retval -= foo[0]();
                Log.Comment(retval.ToString());
                if (retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public delegate int Delegate_TestClass_delegate11_1();
        public class Delegate_TestClass_delegate11_2
        {
            public int bar() { Log.Comment("bar"); return 0x01; }
            static public int far()
            {
                Log.Comment("far");
                return 0x02;
            }
            public Delegate_TestClass_delegate11_1 foo = null;
        }
        public class Delegate_TestClass_delegate11
        {
            static int retval = 0x03;
            static bool loo() { return true; }

            static public int Main_old()
            {
                Delegate_TestClass_delegate11_2 p = new Delegate_TestClass_delegate11_2();
                p.foo = new Delegate_TestClass_delegate11_1(Delegate_TestClass_delegate11_2.far);
                retval -= p.foo();
                p.foo = new Delegate_TestClass_delegate11_1(p.bar);
                retval -= p.foo();
                Log.Comment(retval.ToString());
                if (retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate void Delegate_TestClass_delegate12_1();
        public class Delegate_TestClass_delegate12_2
        {
            public static int retval = 0x06;
            public void bar() { Log.Comment("bar"); retval -= 0x01; }
            static public void far()
            {
                Log.Comment("far");
                retval -= 0x02;
            }
        }
        public class Delegate_TestClass_delegate12
        {


            static public int Main_old()
            {
                Delegate_TestClass_delegate12_2 p = new Delegate_TestClass_delegate12_2();
                Delegate_TestClass_delegate12_1 foo = new Delegate_TestClass_delegate12_1(p.bar);
                Delegate_TestClass_delegate12_1 multifoo = foo;
                multifoo += new Delegate_TestClass_delegate12_1(Delegate_TestClass_delegate12_2.far);
                multifoo();
                multifoo = new Delegate_TestClass_delegate12_1(Delegate_TestClass_delegate12_2.far);
                multifoo += foo;
                multifoo();
                if (Delegate_TestClass_delegate12_2.retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return Delegate_TestClass_delegate12_2.retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate void Delegate_TestClass_delegate13_1();
        public class Delegate_TestClass_delegate13_2
        {
            public static int retval = 0x04;
            public void bar() { Log.Comment("bar"); retval -= 0x01; }
            static public void far()
            {
                Log.Comment("far");
                retval -= 0x02;
            }
        }
        public class Delegate_TestClass_delegate13
        {

            static public int Main_old()
            {
                Delegate_TestClass_delegate13_2 p = new Delegate_TestClass_delegate13_2();
                Delegate_TestClass_delegate13_1 foo = null;
                foo = new Delegate_TestClass_delegate13_1(p.bar);
                foo();
                foo = new Delegate_TestClass_delegate13_1(Delegate_TestClass_delegate13_2.far);
                foo += new Delegate_TestClass_delegate13_1(p.bar);
                foo();
                if (Delegate_TestClass_delegate13_2.retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return Delegate_TestClass_delegate13_2.retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate void Delegate_TestClass_delegate14_1();
        public class Delegate_TestClass_delegate14_2
        {
            public static int retval = 0x03;
            public void bar() { Log.Comment("bar"); retval += 0x10; }
            static public void far()
            {
                Log.Comment("far");
                retval += 0x20;
            }
        }
        public class Delegate_TestClass_delegate14
        {
            static public int Main_old()
            {
                Delegate_TestClass_delegate14_1 foo = null;
                try
                {
                    foo();
                }
                catch (System.Exception)
                {
                    Delegate_TestClass_delegate14_2.retval -= 0x03;
                }
                if (Delegate_TestClass_delegate14_2.retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return Delegate_TestClass_delegate14_2.retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate void Delegate_TestClass_delegate14a_1();
        public class Delegate_TestClass_delegate14a_2
        {
            public static int retval = 0x02;
            public void bar() { Log.Comment("bar"); retval -= 0x01; }
            static public void far()
            {
                Log.Comment("far");
                retval -= 0x02;
            }
        }
        public class Delegate_TestClass_delegate14a
        {
            static public int Main_old()
            {
                Delegate_TestClass_delegate14a_1 foo = new Delegate_TestClass_delegate14a_1(Delegate_TestClass_delegate14a_2.far);
                foo += (Delegate_TestClass_delegate14a_1)null;
                foo();
                if (Delegate_TestClass_delegate14a_2.retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return Delegate_TestClass_delegate14a_2.retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate void Delegate_TestClass_delegate14b_1();
        public class Delegate_TestClass_delegate14b_2
        {
            public static int retval = 0x02;
            public void bar() { Log.Comment("bar"); retval -= 0x01; }
            static public void far()
            {
                Log.Comment("far");
                retval -= 0x02;
            }
        }
        public class Delegate_TestClass_delegate14b
        {
            static public int Main_old()
            {
                Delegate_TestClass_delegate14b_1 foo = null;
                foo += new Delegate_TestClass_delegate14b_1(Delegate_TestClass_delegate14b_2.far);
                foo();
                if (Delegate_TestClass_delegate14b_2.retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return Delegate_TestClass_delegate14b_2.retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate void Delegate_TestClass_delegate14c_1();
        public class Delegate_TestClass_delegate14c_2
        {
            public static int retval = 0x03;
            public void bar() { Log.Comment("bar"); retval -= 0x01; }
            static public void far()
            {
                Log.Comment("far");
                retval -= 0x02;
            }
        }
        public class Delegate_TestClass_delegate14c
        {
            static public int Main_old()
            {
                Delegate_TestClass_delegate14c_2 p = new Delegate_TestClass_delegate14c_2();
                Delegate_TestClass_delegate14c_1 foo = new Delegate_TestClass_delegate14c_1(p.bar);
                foo += (Delegate_TestClass_delegate14c_1)null;
                foo += new Delegate_TestClass_delegate14c_1(Delegate_TestClass_delegate14c_2.far);
                foo();
                if (Delegate_TestClass_delegate14c_2.retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return Delegate_TestClass_delegate14c_2.retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate void Delegate_TestClass_delegate15_1();
        public class Delegate_TestClass_delegate15_2
        {
            public static int retval = 63;
            public void bar1() { Log.Comment("bar1"); retval -= 0x01; }
            static public void far1()
            {
                Log.Comment("far1");
                retval -= 0x02;
            }
            public void bar2() { Log.Comment("bar2"); retval -= 10; }
            static public void far2()
            {
                Log.Comment("far2");
                retval -= 20;
            }
        }
        public class Delegate_TestClass_delegate15
        {
            static bool loo() { return true; }

            static public int Main_old()
        {
        Delegate_TestClass_delegate15_2 p = new Delegate_TestClass_delegate15_2();
        Delegate_TestClass_delegate15_1 foo = null;
		Delegate_TestClass_delegate15_1 left = null;
		Delegate_TestClass_delegate15_1 right = null;
        foo = loo() ? new Delegate_TestClass_delegate15_1(p.bar1): new Delegate_TestClass_delegate15_1(Delegate_TestClass_delegate15_2.far1);
        foo();
        foo = !loo() ? new Delegate_TestClass_delegate15_1(p.bar1): new Delegate_TestClass_delegate15_1(Delegate_TestClass_delegate15_2.far1);
        foo();
		left = new Delegate_TestClass_delegate15_1(p.bar1);
		right = new Delegate_TestClass_delegate15_1(Delegate_TestClass_delegate15_2.far2);
        foo = !loo() ? left += new Delegate_TestClass_delegate15_1(Delegate_TestClass_delegate15_2.far1): right += new Delegate_TestClass_delegate15_1(p.bar2);
        foo();
		right = new Delegate_TestClass_delegate15_1(p.bar1);
		left = new Delegate_TestClass_delegate15_1(Delegate_TestClass_delegate15_2.far2);
        foo = loo() ? left += new Delegate_TestClass_delegate15_1(p.bar2): right += new Delegate_TestClass_delegate15_1(Delegate_TestClass_delegate15_2.far1);
        foo();
        if (Delegate_TestClass_delegate15_2.retval == 0) Log.Comment ("PASS");
        else Log.Comment ("FAIL");
        return Delegate_TestClass_delegate15_2.retval;
        }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate void Delegate_TestClass_delegate16_1();
        public class Delegate_TestClass_delegate16_2
        {
            public static int retval = 0x09;
            public void bar() { Log.Comment("bar"); retval -= 0x01; }
            static public void far()
            {
                Log.Comment("far");
                retval -= 0x02;
            }
        }
        public class Delegate_TestClass_delegate16
        {
            static bool loo() { return true; }

            static public int Main_old()
        {
        Delegate_TestClass_delegate16_2 p = new Delegate_TestClass_delegate16_2();
        Delegate_TestClass_delegate16_1 foo = null;
		Delegate_TestClass_delegate16_1 left = null;
		Delegate_TestClass_delegate16_1 right = null;
        foo = loo() ? new Delegate_TestClass_delegate16_1(p.bar): null;
        foo();
        foo = !loo() ? null: new Delegate_TestClass_delegate16_1(Delegate_TestClass_delegate16_2.far);
        foo();
		left = new Delegate_TestClass_delegate16_1(p.bar);
        foo = loo() ? left += new Delegate_TestClass_delegate16_1(Delegate_TestClass_delegate16_2.far): null;
        foo();
		right = new Delegate_TestClass_delegate16_1(Delegate_TestClass_delegate16_2.far);
        foo = !loo() ? null: right += new Delegate_TestClass_delegate16_1(p.bar);
        foo();
        if (Delegate_TestClass_delegate16_2.retval == 0) Log.Comment ("PASS");
        else Log.Comment ("FAIL");
        return Delegate_TestClass_delegate16_2.retval;
        }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate void Delegate_TestClass_delegate17_1();
        public class Delegate_TestClass_delegate17_2
        {
            public static int retval = 13;
            public void bar() { Log.Comment("bar"); retval -= 0x01; }
            static public void far()
            {
                Log.Comment("far");
                retval -= 0x02;
            }
            public void bar2() { Log.Comment("bar2"); retval -= 10; }
            static public void far2()
            {
                Log.Comment("far2");
                retval -= 20;
            }
        }
        public class Delegate_TestClass_delegate17
        {
            static bool loo() { return true; }

            static public int Main_old()
        {
        Delegate_TestClass_delegate17_2 p = new Delegate_TestClass_delegate17_2();
        Delegate_TestClass_delegate17_1 foo = null;
        try
            {
            foo = loo() ? null: new Delegate_TestClass_delegate17_1(Delegate_TestClass_delegate17_2.far);
            foo();
            }
        catch (System.Exception n)
            {
            Delegate_TestClass_delegate17_2.retval -= 0x01;
            }
        try
            {
            foo = !loo() ? new Delegate_TestClass_delegate17_1(p.bar): null;
            foo();
            }
        catch (System.Exception n)
            {
            Delegate_TestClass_delegate17_2.retval -= 0x02;
            }
        try
            {
            foo = null;
			foo += (Delegate_TestClass_delegate17_1)null;
            foo();
            }
        catch (System.Exception n)
            {
            Delegate_TestClass_delegate17_2.retval -= 10;
            }
        if (Delegate_TestClass_delegate17_2.retval == 0) Log.Comment ("PASS");
        else Log.Comment ("FAIL");
        return Delegate_TestClass_delegate17_2.retval;
        }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate void Delegate_TestClass_delegate18_1();
        public class Delegate_TestClass_delegate18_2
        {
            public static int retval = 23;
            public void bar() { Log.Comment("bar"); retval -= 0x01; }
            static public void far()
            {
                Log.Comment("far");
                retval -= 0x02;
            }
            public void bar1() { Log.Comment("bar1"); retval -= 10; }
            static public void far1()
            {
                Log.Comment("far1");
                retval -= 20;
            }
        }
        public class Delegate_TestClass_delegate18
        {
            static bool loo() { return true; }

            static public int Main_old()
        {
        Delegate_TestClass_delegate18_2 p = new Delegate_TestClass_delegate18_2();
        Delegate_TestClass_delegate18_1 foo = null;
        foo = loo() ? null: new Delegate_TestClass_delegate18_1(Delegate_TestClass_delegate18_2.far);
        if (foo == null) 
		{
            foo += new Delegate_TestClass_delegate18_1(p.bar);
			foo += new Delegate_TestClass_delegate18_1(Delegate_TestClass_delegate18_2.far);
		}
        foo();
        foo = null;
		foo += (Delegate_TestClass_delegate18_1)null;
        if (foo == null) 
		{
			foo += new Delegate_TestClass_delegate18_1(Delegate_TestClass_delegate18_2.far1);
		}

        foo();
        if (Delegate_TestClass_delegate18_2.retval == 0) Log.Comment ("PASS");
        else Log.Comment ("FAIL");
        return Delegate_TestClass_delegate18_2.retval;
        }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate void Delegate_TestClass_delegate19_1();
        public class Delegate_TestClass_delegate19_2
        {
            public static int retval = 33;
            public void bar() { Log.Comment("bar"); retval -= 1; }
            static public void far()
            {
                Log.Comment("far");
                retval -= 2;
            }
        }
        public class Delegate_TestClass_delegate19
        {
            static bool loo() { return true; }

            static public int Main_old()
            {
                Delegate_TestClass_delegate19_2 p = new Delegate_TestClass_delegate19_2();
                Delegate_TestClass_delegate19_1[] foo = new Delegate_TestClass_delegate19_1[3];
                foo[1] = new Delegate_TestClass_delegate19_1(Delegate_TestClass_delegate19_2.far);
                foo[2] = new Delegate_TestClass_delegate19_1(p.bar);
                Delegate_TestClass_delegate19_1 foo1 = null;
                foreach (Delegate_TestClass_delegate19_1 b in foo)
                {
                    foo1 += b;
                }
                Delegate_TestClass_delegate19_1 foo2 = new Delegate_TestClass_delegate19_1(Delegate_TestClass_delegate19_2.far);
                foo2 += new Delegate_TestClass_delegate19_1(p.bar);
                if (foo1 == foo2)
                    Delegate_TestClass_delegate19_2.retval -= 10;
                foo[1] = foo1;
                foo[2] = foo2;
                if (foo[1] == foo[2])
                    Delegate_TestClass_delegate19_2.retval -= 20;
                foo[1]();
                if (Delegate_TestClass_delegate19_2.retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return Delegate_TestClass_delegate19_2.retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate void Delegate_TestClass_delegate20_1();
        public class Delegate_TestClass_delegate20_2
        {
            public static int retval = 1333;
            public void bar() { Log.Comment("bar"); retval -= 1; }
            static public void far()
            {
                Log.Comment("far");
                retval -= 2;
            }
            public void bar1() { Log.Comment("bar1"); retval -= 10; }
            static public void far1()
            {
                Log.Comment("far1");
                retval -= 20;
            }
        }
        public class Delegate_TestClass_delegate20
        {
            static bool loo() { return true; }

            static public int Main_old()
            {
                Delegate_TestClass_delegate20_2 p = new Delegate_TestClass_delegate20_2();
                Delegate_TestClass_delegate20_1 foo1 = null;
                Delegate_TestClass_delegate20_1 foo2 = null;
                foo1 = new Delegate_TestClass_delegate20_1(Delegate_TestClass_delegate20_2.far);
                foo2 = new Delegate_TestClass_delegate20_1(Delegate_TestClass_delegate20_2.far);
                if (foo1 == foo2)
                    Delegate_TestClass_delegate20_2.retval -= 100;
                foo1();
                foo1 = new Delegate_TestClass_delegate20_1(p.bar);
                foo2 = new Delegate_TestClass_delegate20_1(p.bar);
                if (foo1 == foo2)
                    Delegate_TestClass_delegate20_2.retval -= 200;
                foo2();
                foo1 = new Delegate_TestClass_delegate20_1(Delegate_TestClass_delegate20_2.far1);
                foo2 = new Delegate_TestClass_delegate20_1(p.bar1);
                Delegate_TestClass_delegate20_1 foo3 = foo1;
                foo3 += foo2;
                Delegate_TestClass_delegate20_1 foo4 = new Delegate_TestClass_delegate20_1(Delegate_TestClass_delegate20_2.far1);
                foo4 += new Delegate_TestClass_delegate20_1(p.bar1);
                if (foo3 == foo4)
                    Delegate_TestClass_delegate20_2.retval -= 1000;
                foo3();
                if (Delegate_TestClass_delegate20_2.retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return Delegate_TestClass_delegate20_2.retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate void Delegate_TestClass_delegate21_1();
        public class Delegate_TestClass_delegate21_2
        {
            public static int retval = 33;
            public void bar() { Log.Comment("bar"); retval -= 1; }
            static public void far()
            {
                Log.Comment("far");
                retval -= 2;
            }
        }
        public class Delegate_TestClass_delegate21
        {
            static bool loo() { return true; }

            static public int Main_old()
            {
                Delegate_TestClass_delegate21_2 p = new Delegate_TestClass_delegate21_2();
                Delegate_TestClass_delegate21_1[] foo = { new Delegate_TestClass_delegate21_1(Delegate_TestClass_delegate21_2.far), new Delegate_TestClass_delegate21_1(p.bar) };
                Delegate_TestClass_delegate21_1 foo1 = null;
                foreach (Delegate_TestClass_delegate21_1 b in foo)
                {
                    foo1 += b;
                }
                Delegate_TestClass_delegate21_1 foo2 = new Delegate_TestClass_delegate21_1(Delegate_TestClass_delegate21_2.far);
                foo2 += new Delegate_TestClass_delegate21_1(p.bar);
                if (foo1 == foo2)
                    Delegate_TestClass_delegate21_2.retval -= 10;
                foo[0] = foo1;
                foo[1] = foo2;
                if (foo[0] == foo[1])
                    Delegate_TestClass_delegate21_2.retval -= 20;
                foo[0]();
                if (Delegate_TestClass_delegate21_2.retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return Delegate_TestClass_delegate21_2.retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate void Delegate_TestClass_delegate23_Del();
        public class Delegate_TestClass_delegate23_A
        {
            public static int retval = 3;
            public void bar() { Log.Comment("bar"); retval -= 1; }
            static public void far()
            {
                Log.Comment("far");
                retval -= 2;
            }
        }
        public class Delegate_TestClass_delegate23
        {
            static bool loo() { return true; }
            static public int Main_old()
            {
                Delegate_TestClass_delegate23_A p = new Delegate_TestClass_delegate23_A();
                Delegate_TestClass_delegate23_Del foo1 = new Delegate_TestClass_delegate23_Del(Delegate_TestClass_delegate23_A.far);
                Delegate_TestClass_delegate23_Del foo2 = new Delegate_TestClass_delegate23_Del(p.bar);
                Delegate_TestClass_delegate23_Del foo3 = foo1 + foo2;
                foo3();
                if (Delegate_TestClass_delegate23_A.retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL, retval==" + Delegate_TestClass_delegate23_A.retval.ToString());
                return Delegate_TestClass_delegate23_A.retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate void Delegate_TestClass_delegate24_Del();
        public class Delegate_TestClass_delegate24_A
        {
            public static int retval = 0x0F;
            public void bar() { Log.Comment("bar"); retval -= 1; }
            static public void far()
            {
                Log.Comment("far");
                retval -= 2;
            }
            public void bar2() { Log.Comment("bar2"); retval -= 4; }
            static public void far2()
            {
                Log.Comment("far2");
                retval -= 8;
            }
        }
        public class Delegate_TestClass_delegate24
        {
            static bool loo() { return true; }
            static public int Main_old()
            {
                Delegate_TestClass_delegate24_A p = new Delegate_TestClass_delegate24_A();
                Delegate_TestClass_delegate24_Del foo1 = new Delegate_TestClass_delegate24_Del(Delegate_TestClass_delegate24_A.far) + new Delegate_TestClass_delegate24_Del(p.bar);
                Delegate_TestClass_delegate24_Del foo2 = new Delegate_TestClass_delegate24_Del(p.bar2) + new Delegate_TestClass_delegate24_Del(Delegate_TestClass_delegate24_A.far2);
                Delegate_TestClass_delegate24_Del foo3 = foo1 + foo2;
                foo3();
                if (Delegate_TestClass_delegate24_A.retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL, retval==" + Delegate_TestClass_delegate24_A.retval.ToString());
                return Delegate_TestClass_delegate24_A.retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate void Delegate_TestClass_delegate25_Del();
        public class Delegate_TestClass_delegate25_A
        {
            public static int retval = 0x3F;
            public void bar() { Log.Comment("bar"); retval -= 1; }
            static public void far()
            {
                Log.Comment("far");
                retval -= 2;
            }
            public void bar2() { Log.Comment("bar2"); retval -= 4; }
            static public void far2()
            {
                Log.Comment("far2");
                retval -= 8;
            }
        }
        public class Delegate_TestClass_delegate25
        {
            static bool loo() { return true; }
            static public int Main_old()
            {
                Delegate_TestClass_delegate25_A p = new Delegate_TestClass_delegate25_A();
                Delegate_TestClass_delegate25_Del foo1 = new Delegate_TestClass_delegate25_Del(Delegate_TestClass_delegate25_A.far) + new Delegate_TestClass_delegate25_Del(p.bar);
                Delegate_TestClass_delegate25_Del foo2 = new Delegate_TestClass_delegate25_Del(p.bar2) + new Delegate_TestClass_delegate25_Del(Delegate_TestClass_delegate25_A.far2);
                Delegate_TestClass_delegate25_Del foo3 = foo1 + foo2;
                foo3();
                Delegate_TestClass_delegate25_Del foo4 = foo3 - foo2;  //Should be the same as foo1
                if (foo4 == foo1)
                    Delegate_TestClass_delegate25_A.retval -= 0x10;
                Delegate_TestClass_delegate25_A.retval += 3;
                foo4();
                foo4 = foo3 - foo1;  //Should be the same as foo2
                if (foo4 == foo2)
                    Delegate_TestClass_delegate25_A.retval -= 0x20;
                Delegate_TestClass_delegate25_A.retval += 0x0C;
                foo4();
                if (Delegate_TestClass_delegate25_A.retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL, retval==" + Delegate_TestClass_delegate25_A.retval.ToString());
                return Delegate_TestClass_delegate25_A.retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        
        public class Delegate_TestClass_delegate26
        {
            public static int Main_old(String[] args)
            {
                try
                {
                    HD hd = new HD(Delegate_TestClass_delegate26.Hello);
                    int i = 1;
                    IAsyncResult ar = hd.BeginInvoke(out i, null, null);
                    i = 1;
                    hd.EndInvoke(out i, ar);
                    if (0 == i) Log.Comment("PASS");
                    else Log.Comment("FAIL, i==" + i.ToString());
                    return i;
                }
                catch (System.Exception)
                {
                    return 1;
                }
                return 0;
            }
            public delegate void HD(out int i);
            public static void Hello(out int i)
            {
                i = 0;
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        /*
        public class Delegate_TestClass_delegate28
        {
            public static int Main_old(String[] args)
            {
                HD hd = new HD(Delegate_TestClass_delegate28.Hello);
                int i = 1;
                i = hd(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223, 224, 225, 226, 227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 239, 240, 241, 242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253, 254, 255, 256, 257);
                if (0 == i) Log.Comment("PASS");
                else Log.Comment("FAIL, i==" + i.ToString());
                return i;
            }
            public delegate int HD(int i1, int i2, int i3, int i4, int i5, int i6, int i7, int i8, int i9, int i10, int i11, int i12, int i13, int i14, int i15, int i16, int i17, int i18, int i19, int i20, int i21, int i22, int i23, int i24, int i25, int i26, int i27, int i28, int i29, int i30, int i31, int i32, int i33, int i34, int i35, int i36, int i37, int i38, int i39, int i40, int i41, int i42, int i43, int i44, int i45, int i46, int i47, int i48, int i49, int i50, int i51, int i52, int i53, int i54, int i55, int i56, int i57, int i58, int i59, int i60, int i61, int i62, int i63, int i64, int i65, int i66, int i67, int i68, int i69, int i70, int i71, int i72, int i73, int i74, int i75, int i76, int i77, int i78, int i79, int i80, int i81, int i82, int i83, int i84, int i85, int i86, int i87, int i88, int i89, int i90, int i91, int i92, int i93, int i94, int i95, int i96, int i97, int i98, int i99, int i100, int i101, int i102, int i103, int i104, int i105, int i106, int i107, int i108, int i109, int i110, int i111, int i112, int i113, int i114, int i115, int i116, int i117, int i118, int i119, int i120, int i121, int i122, int i123, int i124, int i125, int i126, int i127, int i128, int i129, int i130, int i131, int i132, int i133, int i134, int i135, int i136, int i137, int i138, int i139, int i140, int i141, int i142, int i143, int i144, int i145, int i146, int i147, int i148, int i149, int i150, int i151, int i152, int i153, int i154, int i155, int i156, int i157, int i158, int i159, int i160, int i161, int i162, int i163, int i164, int i165, int i166, int i167, int i168, int i169, int i170, int i171, int i172, int i173, int i174, int i175, int i176, int i177, int i178, int i179, int i180, int i181, int i182, int i183, int i184, int i185, int i186, int i187, int i188, int i189, int i190, int i191, int i192, int i193, int i194, int i195, int i196, int i197, int i198, int i199, int i200, int i201, int i202, int i203, int i204, int i205, int i206, int i207, int i208, int i209, int i210, int i211, int i212,
                int i213, int i214, int i215, int i216, int i217, int i218, int i219, int i220, int i221, int i222, int i223, int i224, int i225, int i226, int i227, int i228, int i229, int i230, int i231, int i232, int i233, int i234, int i235, int i236, int i237, int i238, int i239, int i240, int i241, int i242, int i243, int i244, int i245, int i246, int i247, int i248, int i249, int i250, int i251, int i252, int i253, int i254, int i255, int i256, int i257);
            public static int Hello(int i1, int i2, int i3, int i4, int i5, int i6, int i7, int i8, int i9, int i10, int i11, int i12, int i13, int i14, int i15, int i16, int i17, int i18, int i19, int i20, int i21, int i22, int i23, int i24, int i25, int i26, int i27, int i28, int i29, int i30, int i31, int i32, int i33, int i34, int i35, int i36, int i37, int i38, int i39, int i40, int i41, int i42, int i43, int i44, int i45, int i46, int i47, int i48, int i49, int i50, int i51, int i52, int i53, int i54, int i55, int i56, int i57, int i58, int i59, int i60, int i61, int i62, int i63, int i64, int i65, int i66, int i67, int i68, int i69, int i70, int i71, int i72, int i73, int i74, int i75, int i76, int i77, int i78, int i79, int i80, int i81, int i82, int i83, int i84, int i85, int i86, int i87, int i88, int i89, int i90, int i91, int i92, int i93, int i94, int i95, int i96, int i97, int i98, int i99, int i100, int i101, int i102, int i103, int i104, int i105, int i106, int i107, int i108, int i109, int i110, int i111, int i112, int i113, int i114, int i115, int i116, int i117, int i118, int i119, int i120, int i121, int i122, int i123, int i124, int i125, int i126, int i127, int i128, int i129, int i130, int i131, int i132, int i133, int i134, int i135, int i136, int i137, int i138, int i139, int i140, int i141, int i142, int i143, int i144, int i145, int i146, int i147, int i148, int i149, int i150, int i151, int i152, int i153, int i154, int i155, int i156, int i157, int i158, int i159, int i160, int i161, int i162, int i163, int i164, int i165, int i166, int i167, int i168, int i169, int i170, int i171, int i172, int i173, int i174, int i175, int i176, int i177, int i178, int i179, int i180, int i181, int i182, int i183, int i184, int i185, int i186, int i187, int i188, int i189, int i190, int i191, int i192, int i193, int i194, int i195, int i196, int i197, int i198, int i199, int i200, int i201, int i202, int i203, int i204, int i205, int i206, int i207, int i208, int i209, int i210, int i211, int i212,
                int i213, int i214, int i215, int i216, int i217, int i218, int i219, int i220, int i221, int i222, int i223, int i224, int i225, int i226, int i227, int i228, int i229, int i230, int i231, int i232, int i233, int i234, int i235, int i236, int i237, int i238, int i239, int i240, int i241, int i242, int i243, int i244, int i245, int i246, int i247, int i248, int i249, int i250, int i251, int i252, int i253, int i254, int i255, int i256, int i257)
            {
                return i257 - 257;
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
         */
        delegate int Delegate_TestClass_delegate30_1();
        public struct Delegate_TestClass_delegate30_2
        {
            public int bar() { Log.Comment("bar"); return 0x01; }
            static public int far()
            {
                Log.Comment("far");
                return 0x02;
            }
        }
        public class Delegate_TestClass_delegate30
        {
            static int retval = 0x03;

            static public int Main_old()
            {

                Delegate_TestClass_delegate30_2 p = new Delegate_TestClass_delegate30_2();
                Delegate_TestClass_delegate30_1 foo = new Delegate_TestClass_delegate30_1(p.bar);
                retval -= foo();
                foo = new Delegate_TestClass_delegate30_1(Delegate_TestClass_delegate30_2.far);
                retval -= foo();
                if (retval == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate int Delegate_TestClass_delegate31_1();
        interface I
        {
            int bar();
        }
        public struct Delegate_TestClass_delegate31_2 : I
        {
            public int bar() { Log.Comment("bar"); return 0x01; }
        }
        public class Delegate_TestClass_delegate31
        {
            static int retval = 0x01;

            static public int Main_old()
            {
                Delegate_TestClass_delegate31_2 p = new Delegate_TestClass_delegate31_2();
                Delegate_TestClass_delegate31_1 foo = new Delegate_TestClass_delegate31_1(p.bar);
                retval -= foo();
                if (retval == 0)
                {
                    Log.Comment("PASS");
                }
                else
                {
                    Log.Comment("FAIL");
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        /*
        delegate void Delegate_TestClass_delegate32_Del(int i, params string[] p);
        class Delegate_TestClass_delegate32
        {
            static volatile int res;

            void m1(int i, params string[] p)
            {
                res = p.Length;
            }


            public static int Main_old()
            {
                Delegate_TestClass_delegate32 t = new Delegate_TestClass_delegate32();
                Delegate_TestClass_delegate32_Del d = new Delegate_TestClass_delegate32_Del(t.m1);
                int i = 7;
                string[] strArr = { "foo", "bar" };
                IAsyncResult ar = d.BeginInvoke(i, strArr, null, null);
                ar.AsyncWaitHandle.WaitOne();
                if (res == 2)
                    return 0;
                else
                    return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate void Delegate_TestClass_delegate34_Del(int i, params object[] p);
        class Delegate_TestClass_delegate34
        {
            static volatile int res;

            void m1(int i, params object[] p)
            {
                res = p.Length;
            }


            public static int Main_old()
            {
                Delegate_TestClass_delegate34 t = new Delegate_TestClass_delegate34();
                Delegate_TestClass_delegate34_Del d = new Delegate_TestClass_delegate34_Del(t.m1);
                int i = 7;
                object[] objArr = { 1, 2 };
                IAsyncResult ar = d.BeginInvoke(i, objArr, null, null);
                ar.AsyncWaitHandle.WaitOne();
                if (res == 2)
                    return 0;
                else
                    return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
         */
        class Delegate_TestClass_delegate36
        {
            public delegate int BarDelegate(int[] Arr);
            public static int Bar(params int[] Arr) { return Arr[0]; }
            public static int Main_old()
            {
                BarDelegate Delegate_TestClass_delegate36_B = new BarDelegate(Bar);
                return Delegate_TestClass_delegate36_B(new int[] { 1, 2, 3 }) - Delegate_TestClass_delegate36_B(new int[] { 1, 2 });
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public delegate double Delegate_TestClass_delegate60_Del(int integerPortion, float fraction);
        public class Delegate_TestClass_delegate60
        {
            private delegate double ClassDelegate(int integerPortion, float fraction);
            double DelegatedMethod(int intPart, float frac)
            {
                return intPart + frac;
            }
            public static int Main_old(String[] args)
            {
                int retval = 0x0F;

                Delegate_TestClass_delegate60 c = new Delegate_TestClass_delegate60();
                Delegate_TestClass_delegate60_Del nsd = new Delegate_TestClass_delegate60_Del(c.DelegatedMethod);
                if (nsd is System.Delegate)
                    retval -= 0x01;
                if (nsd is System.MulticastDelegate)
                    retval -= 0x02;
                ClassDelegate cd = new ClassDelegate(c.DelegatedMethod);
                if (cd is System.Delegate)
                    retval -= 0x04;
                if (cd is System.MulticastDelegate)
                    retval -= 0x08;
                if (0 == retval) Log.Comment("PASS");
                else
                {
                    Log.Comment("FAIL, 0x{0:X}");
                    Log.Comment(retval.ToString());
                }
                return retval;
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        delegate double Delegate_TestClass_delegate62_Del(int integerPortion, float fraction);
        public class Delegate_TestClass_delegate62
        {
            new delegate double Delegate_TestClass_delegate62_Del(int integerPortion, float fraction);
            double DelegatedMethod(int intPart, float frac)
            {
                return intPart + frac;
            }
            public static int Main_old(String[] args)
            {
                return 0;
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        delegate double Delegate_TestClass_delegate64_Del(int integerPortion, float fraction);
        public class Delegate_TestClass_delegate64_1
        {
            public delegate double Delegate_TestClass_delegate64_Del(int integerPortion, float fraction);
            double DelegatedMethod(int intPart, float frac)
            {
                return intPart + frac;
            }
        }
        public class Delegate_TestClass_delegate64 : Delegate_TestClass_delegate64_1
        {
            public delegate double Delegate_TestClass_delegate64_Del(int integerPortion, float fraction);
            public static int Main_old(String[] args)
            {
                return 0;
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        delegate double Delegate_TestClass_delegate65_Del(int integerPortion, float fraction);
        public class Delegate_TestClass_delegate65_1
        {
            public delegate double Delegate_TestClass_delegate65_Del(int integerPortion, float fraction);
            public double DelegatedMethod(int intPart, float frac)
            {
                return intPart + frac;
            }
        }
        public class Delegate_TestClass_delegate65 : Delegate_TestClass_delegate65_1
        {
            new public delegate double Delegate_TestClass_delegate65_Del(int integerPortion, float fraction);
            public new double DelegatedMethod(int intPart, float frac)
            {
                return intPart + frac + 5;
            }
            public static int Main_old(String[] args)
            {
                int retval = 0x07;

                Delegate_TestClass_delegate65 dc = new Delegate_TestClass_delegate65();
                Delegate_TestClass_delegate65_Del md = new Delegate_TestClass_delegate65_Del(dc.DelegatedMethod);
                if (md(2, 0.5f) == 7.5)
                    retval -= 0x01;
                Delegate_TestClass_delegate65_1.Delegate_TestClass_delegate65_Del bmd = new Delegate_TestClass_delegate65_1.Delegate_TestClass_delegate65_Del(((Delegate_TestClass_delegate65_1)dc).DelegatedMethod);
                if (md(2, 0.5f) == 7.5)
                    retval -= 0x02;
                Delegate_TestClass_delegate65_1 bc = new Delegate_TestClass_delegate65_1();
                bmd = new Delegate_TestClass_delegate65_1.Delegate_TestClass_delegate65_Del(bc.DelegatedMethod);
                if (bmd(2, 0.5f) == 2.5)
                    retval -= 0x04;
                if (0 == retval) Log.Comment("PASS");
                else
                {
                    Log.Comment("FAIL, 0x{0:X}");
                    Log.Comment(retval.ToString());
                }
                return retval;
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        /*
        delegate double Delegate_TestClass_delegate66_Del(int integerPortion, float fraction);
        public class Delegate_TestClass_delegate66
        {
            public delegate double MyDelegate2(int integerPortion, float fraction);
            public double DelegatedMethod(int intPart, float frac)
            {
                return intPart + frac + 5;
            }
            public static int Main_old(String[] args)
            {
                int retval = 0x03;

                Delegate_TestClass_delegate66 mc = new Delegate_TestClass_delegate66();
                Delegate_TestClass_delegate66_Del md1 = new Delegate_TestClass_delegate66_Del(mc.DelegatedMethod);
                MyDelegate2 md2 = new MyDelegate2(mc.DelegatedMethod);
                if (md1 == md2)
                    retval -= 0x01;
                if (md2 == md1)
                    retval -= 0x02;
                if (0 == retval) Log.Comment("PASS");
                else {Log.Comment("FAIL, 0x{0:X}");
                return retval;
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        delegate double Delegate_TestClass_delegate70_Del(int integerPortion, float fraction);
        public class Delegate_TestClass_delegate70
        {
            public delegate double MyDelegate2(int integerPortion, float fraction);
            public double DelegatedMethod1(int intPart, float frac)
            {
                return intPart + frac + 5;
            }
            public double DelegatedMethod2(int intPart, float frac)
            {
                return intPart + frac + 10;
            }
            public static int Main_old(String[] args)
            {
                int retval = 0x03;

                Delegate_TestClass_delegate70 mc = new Delegate_TestClass_delegate70();
                Delegate_TestClass_delegate70_Del md1 = new Delegate_TestClass_delegate70_Del(mc.DelegatedMethod1);
                MyDelegate2 md2 = new MyDelegate2(mc.DelegatedMethod2);
                if (md1 != md2)
                    retval -= 0x01;
                if (md2 != md1)
                    retval -= 0x02;
                if (0 == retval) Log.Comment("PASS");
                else {Log.Comment("FAIL, 0x{0:X}");
                return retval;
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
         */ 
        delegate double Delegate_TestClass_delegate71_Del(int integerPortion, float fraction);
        public class Delegate_TestClass_delegate71
        {
            public delegate double MyDelegate2(int integerPortion, float fraction);
            public double DelegatedMethod1(int intPart, float frac)
            {
                if (5 == intPart)
                    throw new System.Exception("My System.Exception");
                return intPart + frac + 5;
            }
            public double DelegatedMethod2(int intPart, float frac)
            {
                return intPart + frac + 10;
            }
            public static int Main_old(String[] args)
            {
                int retval = 0x01;

                Delegate_TestClass_delegate71 mc = new Delegate_TestClass_delegate71();
                Delegate_TestClass_delegate71_Del md1 = new Delegate_TestClass_delegate71_Del(mc.DelegatedMethod1);
                md1 += new Delegate_TestClass_delegate71_Del(mc.DelegatedMethod2);
                try
                {
                    double d = md1(5, .5f);
                    retval ^= 0x02;
                }
                catch (System.Exception)
                {
                    retval -= 0x01;
                }
                if (0 == retval) Log.Comment("PASS");
                else
                {
                    Log.Comment("FAIL, 0x{0:X}");
                    Log.Comment(retval.ToString());
                }
                return retval;
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        delegate void Delegate_TestClass_delegate72_Del();
        public class Delegate_TestClass_delegate72_A
        {
            public static int retval = 0x3F;
            public void bar() { Log.Comment("bar"); retval -= 1; }
            static public void far()
            {
                Log.Comment("far");
                retval -= 2;
            }
            public void bar2() { Log.Comment("bar2"); retval -= 4; }
            static public void far2()
            {
                Log.Comment("far2");
                retval -= 8;
            }
        }
        public class Delegate_TestClass_delegate72
        {
            static bool loo() { return true; }
            static public int Main_old()
            {
                Delegate_TestClass_delegate72_A p = new Delegate_TestClass_delegate72_A();
                Delegate_TestClass_delegate72_Del foo1 = new Delegate_TestClass_delegate72_Del(Delegate_TestClass_delegate72_A.far) + new Delegate_TestClass_delegate72_Del(p.bar);
                Delegate_TestClass_delegate72_Del foo2 = new Delegate_TestClass_delegate72_Del(p.bar2) + new Delegate_TestClass_delegate72_Del(Delegate_TestClass_delegate72_A.far2);
                Delegate_TestClass_delegate72_Del foo3 = foo1 + foo2;
                foo3();
                Delegate_TestClass_delegate72_Del foo4 = foo3;
                foo4 -= foo2;  //Should be the same as foo1
                if (foo4 == foo1)
                    Delegate_TestClass_delegate72_A.retval -= 0x10;
                Delegate_TestClass_delegate72_A.retval += 3;
                foo4();
                foo4 = foo3 - foo1;  //Should be the same as foo2
                if (foo4 == foo2)
                    Delegate_TestClass_delegate72_A.retval -= 0x20;
                Delegate_TestClass_delegate72_A.retval += 0x0C;
                foo4();
                if (Delegate_TestClass_delegate72_A.retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL, retval==" + Delegate_TestClass_delegate72_A.retval.ToString());
                return Delegate_TestClass_delegate72_A.retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        /*
        delegate double Delegate_TestClass_delegate73_Del(int integerPortion, float fraction);
        public class Delegate_TestClass_delegate73
        {
            public delegate double MyDelegate2(int integerPortion, float fraction);
            public double DelegatedMethod1(int intPart, float frac)
            {
                return intPart + frac + 5;
            }
            public double DelegatedMethod2(int intPart, float frac)
            {
                return intPart + frac + 10;
            }
            public static int Main_old(String[] args)
            {
                int retval = 0x01;

                Delegate_TestClass_delegate73 mc = null;
                try
                {
                    Delegate_TestClass_delegate73_Del md1 = new Delegate_TestClass_delegate73_Del(mc.DelegatedMethod1);
                    md1 += new Delegate_TestClass_delegate73_Del(mc.DelegatedMethod2);
                    MyDelegate2 md2 = new MyDelegate2(mc.DelegatedMethod1);
                    md2 += new MyDelegate2(mc.DelegatedMethod2);
                    if (md1 == md2)
                        retval ^= 0x01;
                    if (md2 == md1)
                        retval ^= 0x02;
                    md2 -= new MyDelegate2(mc.DelegatedMethod1);  //Remove from front of list...
                    md2 += new MyDelegate2(mc.DelegatedMethod1);  //Add back to list at end
                    if (md1 != md2)
                        retval ^= 0x04;
                    if (md2 != md1)
                        retval ^= 0x08;
                }
                catch (System.Exception)
                {
                    retval -= 0x01;
                }
                if (0 == retval) Log.Comment("PASS");
                else {Log.Comment("FAIL, 0x{0:X}");
                return retval;
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        delegate double Delegate_TestClass_delegate74_Del(int integerPortion, float fraction);
        public class Delegate_TestClass_delegate74
        {
            public delegate double MyDelegate2(int integerPortion, float fraction);
            public double DelegatedMethod1(int intPart, float frac)
            {
                return intPart + frac + 5;
            }
            public double DelegatedMethod2(int intPart, float frac)
            {
                return intPart + frac + 10;
            }
            public static int Main_old(String[] args)
            {
                int retval = 0x03;

                Delegate_TestClass_delegate74 mc = new Delegate_TestClass_delegate74();
                try
                {
                    Delegate_TestClass_delegate74_Del md1 = new Delegate_TestClass_delegate74_Del(mc.DelegatedMethod1);
                    md1 += new Delegate_TestClass_delegate74_Del(mc.DelegatedMethod2);
                    MyDelegate2 md2 = new MyDelegate2(mc.DelegatedMethod1);
                    md2 += new MyDelegate2(mc.DelegatedMethod2);
                    Delegate[] da1 = md1.GetInvocationList();
                    Delegate[] da2 = md2.GetInvocationList();
                    if (da1[1].Method == da2[1].Method)
                        retval ^= 0x01;
                    if (da1[0].Method == da2[0].Method)
                        retval ^= 0x02;
                }
                catch (System.Exception)
                {
                    retval -= 0x01;
                }
                if (0 == retval) Log.Comment("PASS");
                else {Log.Comment("FAIL, 0x{0:X}");
                return retval;
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        delegate double Delegate_TestClass_delegate75_Del(int integerPortion, float fraction);
        public class Delegate_TestClass_delegate75
        {
            public delegate double MyDelegate2(int integerPortion, float fraction);
            public double DelegatedMethod1(int intPart, float frac)
            {
                return intPart + frac + 5;
            }
            public double DelegatedMethod2(int intPart, float frac)
            {
                return intPart + frac + 10;
            }
            public double DelegatedMethod3(int intPart, float frac)
            {
                return intPart + frac + 15;
            }
            public double DelegatedMethod4(int intPart, float frac)
            {
                return intPart + frac + 20;
            }
            public static int Main_old(String[] args)
            {
                int retval = 0x3F;

                Delegate_TestClass_delegate75 mc = new Delegate_TestClass_delegate75();
                try
                {
                    Delegate_TestClass_delegate75_Del md1 = new Delegate_TestClass_delegate75_Del(mc.DelegatedMethod1);
                    md1 += new Delegate_TestClass_delegate75_Del(mc.DelegatedMethod2);
                    MyDelegate2 md2 = new MyDelegate2(mc.DelegatedMethod1);
                    md2 += new MyDelegate2(mc.DelegatedMethod2);
                    Delegate[] da1 = md1.GetInvocationList();
                    Delegate[] da2 = md2.GetInvocationList();
                    if (da1[1].Method == da2[1].Method)
                        retval ^= 0x01;
                    if (da1[0].Method == da2[0].Method)
                        retval ^= 0x02;
                    md2 = new MyDelegate2(mc.DelegatedMethod4);
                    md2 += new MyDelegate2(mc.DelegatedMethod3);
                    Delegate_TestClass_delegate75_Del md3 = new Delegate_TestClass_delegate75_Del(mc.DelegatedMethod4) + new Delegate_TestClass_delegate75_Del(mc.DelegatedMethod3);
                    Delegate_TestClass_delegate75_Del md4 = md1 + md3;
                    Delegate[] da3 = md4.GetInvocationList();
                    if (da3[0].Method == mc.GetType().GetMethod("DelegatedMethod1"))
                        retval ^= 0x04;
                    if (da3[1].Method == mc.GetType().GetMethod("DelegatedMethod2"))
                        retval ^= 0x08;
                    if (da3[2].Method == mc.GetType().GetMethod("DelegatedMethod4"))
                        retval ^= 0x10;
                    if (da3[3].Method == mc.GetType().GetMethod("DelegatedMethod3"))
                        retval ^= 0x20;
                }
                catch (System.Exception)
                {
                    retval -= 0x01;
                }
                if (0 == retval) Log.Comment("PASS");
                else {Log.Comment("FAIL, 0x{0:X}");
                return retval;
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        delegate double Delegate_TestClass_delegate76_Del(int integerPortion, float fraction);
        public class Delegate_TestClass_delegate76
        {
            public delegate double MyDelegate2(int integerPortion, float fraction);
            public double DelegatedMethod1(int intPart, float frac)
            {
                Log.Comment("DelegatedMethod1");
                return intPart + frac + 5;
            }
            public double DelegatedMethod2(int intPart, float frac)
            {
                Log.Comment("DelegatedMethod2");
                return intPart + frac + 10;
            }
            public double DelegatedMethod3(int intPart, float frac)
            {
                Log.Comment("DelegatedMethod3");
                return intPart + frac + 15;
            }
            public double DelegatedMethod4(int intPart, float frac)
            {
                Log.Comment("DelegatedMethod4");
                return intPart + frac + 20;
            }
            public static int Main_old(String[] args)
            {
                int retval = 0x1F;

                Delegate_TestClass_delegate76 mc = new Delegate_TestClass_delegate76();
                try
                {
                    Delegate_TestClass_delegate76_Del md1 = new Delegate_TestClass_delegate76_Del(mc.DelegatedMethod1);
                    md1 += new Delegate_TestClass_delegate76_Del(mc.DelegatedMethod2);
                    MyDelegate2 md2 = new MyDelegate2(mc.DelegatedMethod1);
                    md2 += new MyDelegate2(mc.DelegatedMethod2);
                    Delegate[] da1 = md1.GetInvocationList();
                    Delegate[] da2 = md2.GetInvocationList();
                    if (da1[1].Method == da2[1].Method)
                        retval ^= 0x01;
                    if (da1[0].Method == da2[0].Method)
                        retval ^= 0x02;
                    md2 = new MyDelegate2(mc.DelegatedMethod4);
                    md2 += new MyDelegate2(mc.DelegatedMethod3);
                    Delegate_TestClass_delegate76_Del md3 = new Delegate_TestClass_delegate76_Del(mc.DelegatedMethod4) + new Delegate_TestClass_delegate76_Del(mc.DelegatedMethod3);
                    Delegate_TestClass_delegate76_Del md4 = new Delegate_TestClass_delegate76_Del(md1) + md3;
                    Delegate[] da3 = md4.GetInvocationList();
                    if (da3[0].Method.Name == "Invoke")  //This is because was constructed by copying another delegate
                        retval ^= 0x04;
                    if (da3[1].Method.Name == mc.GetType().GetMethod("DelegatedMethod4").Name)
                        retval ^= 0x08;
                    if (da3[2].Method.Name == mc.GetType().GetMethod("DelegatedMethod3").Name)
                        retval ^= 0x10;
                }
                catch (System.Exception)
                {
                    retval -= 0x01;
                }
                if (0 == retval) Log.Comment("PASS");
                else {Log.Comment("FAIL, 0x{0:X}");
                return retval;
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        */ 
        delegate bool Delegate_TestClass_delegate77_Del(int integerPortion, float fraction);
        public class Delegate_TestClass_delegate77
        {
            public delegate bool MyDelegate2(int integerPortion, float fraction);
            private static int synchro = 31;
            private static int retval = 0x0F;
            public bool DelegatedMethod1(int intPart, float frac)
            {
                if ((31 == synchro++) && (5 == intPart) && (.25 == frac))
                {
                    retval ^= 0x01;
                    return true;
                }
                return false;
            }
            public bool DelegatedMethod2(int intPart, float frac)
            {
                if ((32 == synchro++) && (5 == intPart) && (.25 == frac))
                {
                    retval ^= 0x02;
                    return true;
                }
                return false;
            }
            public bool DelegatedMethod3(int intPart, float frac)
            {
                if ((34 == synchro++) && (5 == intPart) && (.25 == frac))
                {
                    retval ^= 0x08;
                    return true;
                }
                return false;
            }
            public bool DelegatedMethod4(int intPart, float frac)
            {
                if ((33 == synchro++) && (5 == intPart) && (.25 == frac))
                {
                    retval ^= 0x04;
                    return true;
                }
                return false;
            }
            public static int Main_old(String[] args)
            {
                Delegate_TestClass_delegate77 mc = new Delegate_TestClass_delegate77();
                try
                {
                    Delegate_TestClass_delegate77_Del md1 = new Delegate_TestClass_delegate77_Del(mc.DelegatedMethod1);
                    md1 += new Delegate_TestClass_delegate77_Del(mc.DelegatedMethod2);
                    MyDelegate2 md2 = new MyDelegate2(mc.DelegatedMethod4);
                    md2 += new MyDelegate2(mc.DelegatedMethod3);
                    Delegate_TestClass_delegate77_Del md3 = new Delegate_TestClass_delegate77_Del(mc.DelegatedMethod4) + new Delegate_TestClass_delegate77_Del(mc.DelegatedMethod3);
                    Delegate_TestClass_delegate77_Del md4 = new Delegate_TestClass_delegate77_Del(md1) + md3;
                    md4(5, .25f);
                }
                catch (System.Exception)
                {
                    retval -= 0x01;
                }
                if (0 == retval) Log.Comment("PASS");
                else {Log.Comment("FAIL, 0x{0:X}");
                    Log.Comment(retval.ToString());}
                return retval;
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        delegate bool Delegate_TestClass_delegate78_Del(ref int integerPortion, float fraction);
        public class Delegate_TestClass_delegate78
        {
            public delegate bool MyDelegate2(ref int integerPortion, float fraction);
            private static int synchro = 31;
            private static int retval = 0x1F;
            public bool DelegatedMethod1(ref int intPart, float frac)
            {
                if ((31 == synchro++) && (5 == intPart++) && (.25 == frac))
                {
                    retval ^= 0x01;
                    return true;
                }
                return false;
            }
            public bool DelegatedMethod2(ref int intPart, float frac)
            {
                if ((32 == synchro++) && (6 == intPart++) && (.25 == frac))
                {
                    retval ^= 0x02;
                    return true;
                }
                return false;
            }
            public bool DelegatedMethod3(ref int intPart, float frac)
            {
                if ((34 == synchro++) && (8 == intPart++) && (.25 == frac))
                {
                    retval ^= 0x08;
                    return true;
                }
                return false;
            }
            public bool DelegatedMethod4(ref int intPart, float frac)
            {
                if ((33 == synchro++) && (7 == intPart++) && (.25 == frac))
                {
                    retval ^= 0x04;
                    return true;
                }
                return false;
            }
            public static int Main_old(String[] args)
            {
                Delegate_TestClass_delegate78 mc = new Delegate_TestClass_delegate78();
                try
                {
                    Delegate_TestClass_delegate78_Del md1 = new Delegate_TestClass_delegate78_Del(mc.DelegatedMethod1);
                    md1 += new Delegate_TestClass_delegate78_Del(mc.DelegatedMethod2);
                    MyDelegate2 md2 = new MyDelegate2(mc.DelegatedMethod4);
                    md2 += new MyDelegate2(mc.DelegatedMethod3);
                    Delegate_TestClass_delegate78_Del md3 = new Delegate_TestClass_delegate78_Del(mc.DelegatedMethod4) + new Delegate_TestClass_delegate78_Del(mc.DelegatedMethod3);
                    Delegate_TestClass_delegate78_Del md4 = new Delegate_TestClass_delegate78_Del(md1) + md3;
                    int i = 5;
                    md4(ref i, .25f);
                    if (9 == i)
                        retval ^= 0x10;
                }
                catch (System.Exception)
                {
                    retval -= 0x01;
                }
                if (0 == retval) Log.Comment("PASS");
                else {Log.Comment("FAIL, 0x{0:X}");
                    Log.Comment(retval.ToString());}
                return retval;
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        delegate bool Delegate_TestClass_delegate79_Del(out int integerPortion, float fraction);
        public class Delegate_TestClass_delegate79
        {
            public delegate bool MyDelegate2(out int integerPortion, float fraction);
            private static int synchro = 31;
            private static int retval = 0x1F;
            public bool DelegatedMethod1(out int intPart, float frac)
            {
                intPart = 5;
                if ((31 == synchro++) && (.25 == frac))
                {
                    retval ^= 0x01;
                    return true;
                }
                return false;
            }
            public bool DelegatedMethod2(out int intPart, float frac)
            {
                intPart = 6;
                if ((32 == synchro++) && (.25 == frac))
                {
                    retval ^= 0x02;
                    return true;
                }
                return false;
            }
            public bool DelegatedMethod3(out int intPart, float frac)
            {
                intPart = 8;
                if ((34 == synchro++) && (.25 == frac))
                {
                    retval ^= 0x08;
                    return true;
                }
                return false;
            }
            public bool DelegatedMethod4(out int intPart, float frac)
            {
                intPart = 7;
                if ((33 == synchro++) && (.25 == frac))
                {
                    retval ^= 0x04;
                    return true;
                }
                return false;
            }
            public static int Main_old(String[] args)
            {
                Delegate_TestClass_delegate79 mc = new Delegate_TestClass_delegate79();
                try
                {
                    Delegate_TestClass_delegate79_Del md1 = new Delegate_TestClass_delegate79_Del(mc.DelegatedMethod1);
                    md1 += new Delegate_TestClass_delegate79_Del(mc.DelegatedMethod2);
                    MyDelegate2 md2 = new MyDelegate2(mc.DelegatedMethod4);
                    md2 += new MyDelegate2(mc.DelegatedMethod3);
                    Delegate_TestClass_delegate79_Del md3 = new Delegate_TestClass_delegate79_Del(mc.DelegatedMethod4) + new Delegate_TestClass_delegate79_Del(mc.DelegatedMethod3);
                    Delegate_TestClass_delegate79_Del md4 = new Delegate_TestClass_delegate79_Del(md1) + md3;
                    int i = 5;
                    md4(out i, .25f);
                    if (8 == i)
                        retval ^= 0x10;
                }
                catch (System.Exception)
                {
                    retval -= 0x01;
                }
                if (0 == retval) Log.Comment("PASS");
                else {Log.Comment("FAIL, 0x{0:X}");
                    Log.Comment(retval.ToString());}
                return retval;
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        delegate bool Delegate_TestClass_delegate80_Del(out int integerPortion, float fraction);
        public class Delegate_TestClass_delegate80
        {
            public delegate bool MyDelegate2(out int integerPortion, float fraction);
            private static int synchro = 31;
            private static int retval = 0x23;
            public bool DelegatedMethod1(out int intPart, float frac)
            {
                intPart = 5;
                if ((31 == synchro++) && (.25 == frac))
                {
                    retval ^= 0x01;
                    return true;
                }
                return false;
            }
            public bool DelegatedMethod2(out int intPart, float frac)
            {
                intPart = 6;
                if ((32 == synchro++) && (.25 == frac))
                {
                    retval ^= 0x02;
                    throw new System.Exception();
                    return true;
                }
                return false;
            }
            public bool DelegatedMethod3(out int intPart, float frac)
            {
                intPart = 8;
                if ((34 == synchro++) && (.25 == frac))
                {
                    retval ^= 0x08;
                    return true;
                }
                return false;
            }
            public bool DelegatedMethod4(out int intPart, float frac)
            {
                intPart = 7;
                if ((33 == synchro++) && (.25 == frac))
                {
                    retval ^= 0x04;
                    return true;
                }
                return false;
            }
            public static int Main_old(String[] args)
            {
                Delegate_TestClass_delegate80 mc = new Delegate_TestClass_delegate80();
                try
                {
                    Delegate_TestClass_delegate80_Del md1 = new Delegate_TestClass_delegate80_Del(mc.DelegatedMethod1);
                    md1 += new Delegate_TestClass_delegate80_Del(mc.DelegatedMethod2);
                    MyDelegate2 md2 = new MyDelegate2(mc.DelegatedMethod4);
                    md2 += new MyDelegate2(mc.DelegatedMethod3);
                    Delegate_TestClass_delegate80_Del md3 = new Delegate_TestClass_delegate80_Del(mc.DelegatedMethod4) + new Delegate_TestClass_delegate80_Del(mc.DelegatedMethod3);
                    Delegate_TestClass_delegate80_Del md4 = new Delegate_TestClass_delegate80_Del(md1) + md3;
                    int i = 5;
                    md4(out i, .25f);
                    if (8 == i)
                        retval ^= 0x10;
                }
                catch (System.Exception)
                {
                    retval ^= 0x20;
                }
                if (0 == retval) Log.Comment("PASS");
                else {Log.Comment("FAIL, 0x{0:X}");
                Log.Comment(retval.ToString());
            }
                return retval;
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        delegate void Delegate_TestClass_delegate81_B(int x);
        class Delegate_TestClass_delegate81_A
        {
            public static void M1(int i)
            {
                Log.Comment("Delegate_TestClass_delegate81_A.M1: " + i);
            }
            public static void M2(int i)
            {
                Log.Comment("Delegate_TestClass_delegate81_A.M2: " + i);
            }
            public void M3(int i)
            {
                Log.Comment("Delegate_TestClass_delegate81_A.M3: " + i);
            }
        }
        class Delegate_TestClass_delegate81
        {
            static void Main_old()
            {
                Delegate_TestClass_delegate81_B cd1 = new Delegate_TestClass_delegate81_B(Delegate_TestClass_delegate81_A.M1);
                cd1(-1);		// call M1
                Delegate_TestClass_delegate81_B cd2 = new Delegate_TestClass_delegate81_B(Delegate_TestClass_delegate81_A.M2);
                cd2(-2);		// call M2
                Delegate_TestClass_delegate81_B cd3 = cd1 + cd2;
                cd3(10);		// call M1 then M2
                cd3 += cd1;
                cd3(20);		// call M1, M2, then M1
                Delegate_TestClass_delegate81_A c = new Delegate_TestClass_delegate81_A();
                Delegate_TestClass_delegate81_B cd4 = new Delegate_TestClass_delegate81_B(c.M3);
                cd3 += cd4;
                cd3(30);		// call M1, M2, M1, then M3
                cd3 -= cd1;	// remove last M1
                cd3(40);		// call M1, M2, then M3
                cd3 -= cd4;
                cd3(50);		// call M1 then M2
                cd3 -= cd2;
                cd3(60);		// call M1
                cd3 -= cd2;	// impossible removal is benign
                cd3(60);		// call M1
                cd3 -= cd1;	// invocation list is empty
                cd3 -= cd1;	// impossible removal is benign
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        public class Delegate_TestClass_delegate_modifier09
        {
            public delegate void Delegate_TestClass_delegate_modifier09_B();

            public static int Main_old(String[] args)
            {
                return 0;
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Delegate_TestClass_delegate_modifier10
        {
            protected delegate void Delegate_TestClass_delegate_modifier10_B();

            public static int Main_old(String[] args)
            {
                return 0;
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Delegate_TestClass_delegate_modifier11
        {
            private delegate void Delegate_TestClass_delegate_modifier11_B();

            public static int Main_old(String[] args)
            {
                return 0;
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Delegate_TestClass_delegate_modifier12
        {
            internal delegate void Delegate_TestClass_delegate_modifier12_B();

            public static int Main_old(String[] args)
            {
                return 0;
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }

    
    }
}
