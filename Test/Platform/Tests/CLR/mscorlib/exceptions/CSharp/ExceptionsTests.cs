////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ExceptionTests : IMFTestInterface
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

        //Exception Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\Exception
        //excep01,excep02,excep04,excep05,excep06,excep07,excep09,excep10,excep11,excep27,excep28,excep30,excep31,excep33,excep34,excep35,excep40,excep41,excep42,excep42b,excep43,excep56,excep57,excep58,excep59,excep60,excep61,excep62,excep63,excep64,excep65,

        //Test Case Calls 
        [TestMethod]
        public MFTestResults Exc_excep01_Test()
        {
            Log.Comment("This test will confirm that the the thrown exception is caught by the matching catch");
            if (Exc_TestClass_excep01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep02_Test()
        {
            Log.Comment("This test will confirm that the the thrown exception is caught by the matching catch, not the base class catch blocks");
            if (Exc_TestClass_excep02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep04_Test()
        {
            Log.Comment("This test will confirm that the the thrown exception is caught and the error code can be set");
            if (Exc_TestClass_excep04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep05_Test()
        {
            Log.Comment("This test will confirm that the the thrown exception is caught by the matching catch, not the base class catch");
            if (Exc_TestClass_excep05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep06_Test()
        {
            Log.Comment("This test will confirm that the the thrown exception is caught by the base class catch");
            if (Exc_TestClass_excep06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep07_Test()
        {
            Log.Comment("This test will confirm that the the thrown exception is caught by the base class catch()");
            if (Exc_TestClass_excep07.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep09_Test()
        {
            Log.Comment("This test will confirm that the catch() functions.");
            if (Exc_TestClass_excep09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep10_Test()
        {
            Log.Comment("This test will confirm that the thrown exception is handled in the catch()");
            Log.Comment("when no matching catch is available.");
            if (Exc_TestClass_excep10.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep11_Test()
        {
            Log.Comment("This test will confirm that the the thrown exception is caught by the matching catch, not the catch()");
            if (Exc_TestClass_excep11.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep27_Test()
        {
            Log.Comment("Throwing an exception transfers control to a handler.");
            if (Exc_TestClass_excep27.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep28_Test()
        {
            Log.Comment("When an exception is thrown, control is transferred to a handler");
            if (Exc_TestClass_excep28.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep30_Test()
        {
            Log.Comment("A throw-expression with no operand rethrows the exception being handled.");
            if (Exc_TestClass_excep30.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep31_Test()
        {
            Log.Comment("A throw-expression with no operand does not copy the exception being handled.");
            if (Exc_TestClass_excep31.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep33_Test()
        {
            Log.Comment("The exception thrown by a rethrow is the one most recently caught and not finished.");
            if (Exc_TestClass_excep33.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep34_Test()
        {
            Log.Comment("When initialization is complete for the formal parameter of a catch clause, an exception is considered caught.");
            if (Exc_TestClass_excep34.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep35_Test()
        {
            Log.Comment("A handler is not allowed to catch an expression thrown outside");
            Log.Comment("its function-try-block and any function called from its function-");
            Log.Comment("try-block.");
            if (Exc_TestClass_excep35.testMethod())
            {   
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep40_Test()
        {
            Log.Comment("If no match is found among the handlers for a try-block, the");
            Log.Comment("search for a matching handler continues in a dynamically");
            Log.Comment("surrounding try-block.");
            if (Exc_TestClass_excep40.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep41_Test()
        {
            Log.Comment("If no match is found among the handlers for a try-block, the");
            Log.Comment("search for a matching handler continues in a dynamically");
            Log.Comment("surrounding try-block.");
            if (Exc_TestClass_excep41.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        /*
         * This test is excluded because it causes a Metadata processor crash, it failed in the baseline
         * 
        [TestMethod]
        public MFTestResults Exc_excep42_Test()
        {
            Log.Comment("Handle throws up to 255 levels deep.");
            if (Exc_TestClass_excep42.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
          */ 
        [TestMethod]
        public MFTestResults Exc_excep42b_Test()
        {
            Log.Comment("Handle throws up to 33 levels deep.");
            if (Exc_TestClass_excep42b.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        /*
         * This test is excluded because it causes a Metadata processor crash, it failed in the baseline
         * 
        [TestMethod]
        public MFTestResults Exc_excep43_Test()
        {
            Log.Comment("Handle throws up to 255 levels deep, but don't catch.  VM should not die.");
            if (Exc_TestClass_excep43.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
         */ 
        [TestMethod]
        public MFTestResults Exc_excep56_Test()
        {
            Log.Comment("Should get unreachable code warning, but nothing more.");
            if (Exc_TestClass_excep56.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep57_Test()
        {
            Log.Comment("Should get unreachable code warning, but nothing more.");
            if (Exc_TestClass_excep57.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep58_Test()
        {
            Log.Comment("Any finally clauses associated with try statements will be executed before catch clause execution");
            if (Exc_TestClass_excep58.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep59_Test()
        {
            Log.Comment("Any finally clauses associated with try statements will be executed before catch clause execution");
            if (Exc_TestClass_excep59.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep60_Test()
        {
            Log.Comment("Inner exceptions can be chained");
            if (Exc_TestClass_excep60.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep61_Test()
        {
            Log.Comment("Inner exceptions can be chained to arbitrary length");
            if (Exc_TestClass_excep61.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep62_Test()
        {
            Log.Comment("Any finally clauses associated with try statements will be executed before catch clause execution");
            if (Exc_TestClass_excep62.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Exc_excep63_Test()
        {
            Log.Comment("If a catch search reaches a static ctor then a Exception is thrown,");
            Log.Comment("at the point of static ctor invocation. The inner exception is the original exception.");

            Log.Comment("an exception thrown in a static constructor brings up a dialog box in Visual Studio.");
            Log.Comment("Disable this test so that it doesn't hose VS until it is fixed.");

            if (Exc_TestClass_excep63.testMethod())
            {
                Log.Comment("This is bug number: 21724	Resolved By Design.");
                Log.Comment("This is bug number: 21724	If a catch search reaches a static ctor then a Exception is thrown at the point of static ctor invocation. The inner exception is the original exception. ");
                Log.Comment("When this bug is fixed change this back to pass and chang eht known failure to fail");
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;
        }
        [TestMethod]
        public MFTestResults Exc_excep64_Test()
        {
            Log.Comment("If a catch search reaches a static field initializer then a Exception is thrown,");
            Log.Comment("at the point of static ctor invocation. The inner exception is the original exception.");

            Log.Comment("an exception thrown in a static constructor brings up a dialog box in Visual Studio.");
            Log.Comment("Disable this test so that it doesn't hose VS until it is fixed.");

            if (Exc_TestClass_excep64.testMethod())
            {
                Log.Comment("This is bug number: 21724	Resolved By Design.");
                Log.Comment("This is bug number: 21724	If a catch search reaches a static ctor then a Exception is thrown at the point of static ctor invocation. The inner exception is the original exception. ");
                Log.Comment("When this bug is fixed change this back to pass and chang eht known failure to fail");
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;
        }
        /*
         * This test is excluded because it throws an exception, it succeeded in the baseline
         * It however currently fails in the desktop environment, in both cases the sleep is not sufficient to provoke GC
         * and when GC is called, the dtor does not exit or call its base class' dtor when it throws an exception.
         * 
        [TestMethod]
        public MFTestResults Exc_excep65_Test()
        {
            Log.Comment("If a catch search reaches a static ctor then a Exception is thrown,");
            Log.Comment("at the point of static ctor invocation. The inner exception is the original exception.");

            if (Exc_TestClass_excep65.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
         */ 





        //Compiled Test Cases 
        public class Exc_TestClass_excep01
        {
            private static int retval = 1;
            public static int Main_old()
            {
                try
                {
                    Log.Comment("In try block, ready to throw.");
                    throw new Exception("An exception has occurred");
                }
                catch (Exception s)
                {
                    Log.Comment("In catch block.");
                    Log.Comment(s.Message);
                    retval = 0;
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Exc_TestClass_excep02_E1 : Exception
        {
            public Exc_TestClass_excep02_E1(String str)
                : base(str)
            { }
        }
        public class Exc_TestClass_excep02
        {
            private static int retval = 1;
            public static int Main_old()
            {
                try
                {
                    Log.Comment("In try block, ready to throw.");
                    throw new Exc_TestClass_excep02_E1("An exception has occurred");
                }
                catch (Exc_TestClass_excep02_E1 s)
                {
                    Log.Comment("In Exc_TestClass_excep02_E1 catch block.");
                    Log.Comment(s.Message);
                    retval = 0;
                }
                catch (Exception)
                {
                    Log.Comment("FAIL - In Exception catch block.");
                    retval = 2;
                }
                //catch (Exception e)
                //{
                //    Log.Comment ("FAIL - In Exception catch block.");
                //    retval = 3;
                //}
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Exc_TestClass_excep04
        {
            private static int retval = 3;
            public static int Main_old()
            {
                try
                {
                    try
                    {
                        Log.Comment("In try block, ready to throw.");
                        throw new Exception("An exception has occurred");
                    }
                    catch (Exception s)
                    {
                        Log.Comment("In catch block.");
                        Log.Comment(s.Message);
                        retval -= 1;
                    }
                }
                finally
                {
                    Log.Comment("Entering finally block");
                    retval -= 2;
                }
                Log.Comment("Ready to return.");
                if (0 == retval) Log.Comment("PASS");
                else Log.Comment("FAIL, retval=={0} " + retval.ToString());
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Exc_TestClass_excep05_E1 : Exception
        {
            public Exc_TestClass_excep05_E1(String str)
                : base(str)
            { }
        }
        public class Exc_TestClass_excep05
        {
            private static int retval = 2;
            public static int Main_old()
            {
                try
                {
                    try
                    {
                        Log.Comment("In try block, ready to throw.");
                        throw new Exc_TestClass_excep05_E1("An exception has occurred");
                    }
                    catch (Exc_TestClass_excep05_E1 s)
                    {
                        Log.Comment("In catch block.");
                        Log.Comment(s.Message);
                        retval--;
                    }
                    catch (Exception)
                    {
                        Log.Comment("FAIL -- Should not enter catch (Exception) block");
                        retval++;
                    }
                }
                finally
                {
                    Log.Comment("In finally block");
                    --retval;
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Exc_TestClass_excep06_E1 : Exception
        {
            public Exc_TestClass_excep06_E1(String str)
                : base(str)
            { }
        }
        public class Exc_TestClass_excep06
        {
            private static int retval = 1;
            public static int Main_old()
            {
                try
                {
                    Log.Comment("In try block, ready to throw.");
                    throw new Exc_TestClass_excep06_E1("An exception has occurred");
                }
                catch (Exception s)
                {
                    Log.Comment("In catch block.");
                    Log.Comment(s.Message);
                    retval = 0;
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Exc_TestClass_excep07_E1 : Exception
        {
            public Exc_TestClass_excep07_E1(String str)
                : base(str)
            { }
        }
        public class Exc_TestClass_excep07
        {
            private static int retval = 1;
            public static int Main_old()
            {
                try
                {
                    try
                    {
                        Log.Comment("In try block, ready to throw.");
                    }
                    catch (Exception s)
                    {
                        Log.Comment("In catch block.");
                        Log.Comment(s.Message);
                        retval++;
                    }
                }
                finally
                {
                    Log.Comment("In finally block");
                    retval--;
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Exc_TestClass_excep09
        {
            private static int retval = 1;
            public static int Main_old()
            {
                try
                {
                    Log.Comment("In try block, ready to throw.");
                    throw new Exception("An exception has occurred");
                }
                catch
                {
                    Log.Comment("In catch block.");
                    retval = 0;
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Exc_TestClass_excep10_E1 : Exception
        {
            public Exc_TestClass_excep10_E1(String str)
                : base(str)
            { }
        }
        public class Exc_TestClass_excep10
        {
            private static int retval = 1;
            public static int Main_old()
            {
                try
                {
                    Log.Comment("In try block, ready to throw.");
                    throw new Exception("An exception has occurred");
                }
                catch (Exc_TestClass_excep10_E1)
                {
                    Log.Comment("FAIL -- Should not enter catch (Exception a) block.");
                    retval = 1;
                }
                catch
                {
                    Log.Comment("In catch block.");
                    retval -= 1;
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Exc_TestClass_excep11
        {
            private static int retval = 1;
            public static int Main_old()
            {
                try
                {
                    Log.Comment("In try block, ready to throw.");
                    throw new Exception("An exception has occurred");
                }
                catch (Exception r)
                {
                    Log.Comment("In catch (Exception r) block.");
                    Log.Comment(r.Message);
                    retval -= 1;
                }
                catch
                {
                    Log.Comment("FAIL -- Should not enter catch () block.");
                    retval += 1;
                }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Exc_TestClass_excep27_B
        {
            public void toss() { throw new Exception("Exception thrown in function toss()."); }
        }
        public class Exc_TestClass_excep27
        {
            private static int retval = 5;
            public static int Main_old()
            {
                try
                {
                    Exc_TestClass_excep27_B b = new Exc_TestClass_excep27_B();
                    b.toss();
                }
                catch (Exception e) { retval -= 5; Log.Comment(e.Message); }
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Exc_TestClass_excep28_B
        {
            public int toss(int Where)
            {
                int r = 99;
                try
                {
                    if (Where == 0) throw new Exception("Where == 0");
                }
                catch (Exception)
                {
                    r = 0;
                }
                try
                {
                    if (Where == 1) throw new Exception("Where == 1");
                }
                catch (Exception)
                {
                    r = 1;
                }
                try
                {
                    if (Where == 2) throw new Exception("Where == 2");
                }
                catch (Exception)
                {
                    r = 2;
                }
                return r;
            }
        }
        public class Exc_TestClass_excep28
        {
            private static int retval = 7;
            private static int tossval = -1;
            public static int Main_old()
            {
                Exc_TestClass_excep28_B b = new Exc_TestClass_excep28_B();
                tossval = b.toss(0);
                if (tossval != 0)
                {
                    Log.Comment("toss(0) returned ");
                    Log.Comment(tossval.ToString());
                    Log.Comment(" instead of 0.");
                }
                else
                {
                    retval -= 4;
                }
                tossval = b.toss(1);
                if (tossval != 1)
                {
                    Log.Comment("toss(1) returned ");
                    Log.Comment(tossval.ToString());
                    Log.Comment(" instead of 1.");
                }
                else
                {
                    retval -= 2;
                }
                tossval = b.toss(2);
                if (tossval != 2)
                {
                    Log.Comment("toss(2) returned ");
                    Log.Comment(tossval.ToString());
                    Log.Comment(" instead of 2.");
                }
                else
                {
                    retval -= 1;
                }
                if (retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Exc_TestClass_excep30_C1
        {
            static int s = 1;
            public int rethrow()
            {
                try
                {
                    throw new Exception();
                }
                catch (Exception)
                {
                    s = 20;
                    throw;		// rethrow float
                }
                catch
                {
                    s = 99;
                }
                return s;
            }
            public int f00()
            {
                try
                {
                    rethrow();
                }
                catch (Exception)
                {
                    s += 10;
                }
                return s;
            }
        }
        public class Exc_TestClass_excep30
        {
            private static int retval = 3;
            public static int Main_old()
            {
                Exc_TestClass_excep30_C1 t = new Exc_TestClass_excep30_C1();
                if (t.f00() == 30)  //  If the throw was handled properly...
                    retval = 0;
                if (retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Exc_TestClass_excep31_E1 : Exception
        {
            public int i = 99;
            public Exc_TestClass_excep31_E1(int ii) { i = ii; }
        }
        public class Exc_TestClass_excep31_C1
        {
            static int s = 1;
            public int rethrow()
            {
                try
                {
                    throw new Exc_TestClass_excep31_E1(1);
                }
                catch (Exc_TestClass_excep31_E1 e)
                {
                    e.i = 20;
                    throw;		// rethrow float
                }
                catch
                {
                    s = 99;
                }
                return s;
            }
            public int f00()
            {
                try
                {
                    rethrow();
                }
                catch (Exc_TestClass_excep31_E1 e)
                {
                    if (e.i == 20)
                        s = 0;
                }
                return s;
            }
        }
        public class Exc_TestClass_excep31
        {
            private static int retval = 3;
            public static int Main_old()
            {
                Exc_TestClass_excep31_C1 t = new Exc_TestClass_excep31_C1();
                if (t.f00() == 0)  //  If the throw was handled properly...
                    retval = 0;
                if (retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Exc_TestClass_excep33_E2 : Exception
        {
        }
        public class Exc_TestClass_excep33_E3 : Exception
        {
        }
        public class Exc_TestClass_excep33_E4 : Exception
        {
        }
        public class Exc_TestClass_excep33
        {
            private static int retval = 2;
            public static int Main_old()
            {
                Exc_TestClass_excep33_E2 s0 = new Exc_TestClass_excep33_E2();
                Exc_TestClass_excep33_E3 s1 = new Exc_TestClass_excep33_E3();
                Exc_TestClass_excep33_E4 s2 = new Exc_TestClass_excep33_E4();
                try
                {
                    try
                    {
                        throw s0;
                    }
                    catch (Exc_TestClass_excep33_E2)
                    {
                        try
                        {
                            throw s1;
                        }
                        catch (Exc_TestClass_excep33_E3)
                        {
                            try
                            {
                                throw s2;
                            }
                            catch (Exc_TestClass_excep33_E4)
                            {
                                throw;
                            }
                        }
                    }
                }
                catch (Exc_TestClass_excep33_E2)
                {
                    Log.Comment("Unexpected Exc_TestClass_excep33_E2 catch.");
                }
                catch (Exc_TestClass_excep33_E3)
                {
                    Log.Comment("Unexpected Exc_TestClass_excep33_E3 catch.");
                }
                catch (Exc_TestClass_excep33_E4)
                {
                    Log.Comment("Caught in Exc_TestClass_excep33_E4, as expected.");
                    retval = 0;
                }
                catch
                {
                    Log.Comment("Unexpected ... catch.");
                }
                if (retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }

        }
        public class Exc_TestClass_excep34_E2 : Exception
        {
            public char c = ' ';
        }
        public class Exc_TestClass_excep34_E3 : Exception
        {
            public float f = 1.3f;
        }
        public class Exc_TestClass_excep34
        {
            private static int retval = 2;
            public static int Main_old()
            {
                Exc_TestClass_excep34_E2 s0 = new Exc_TestClass_excep34_E2();
                Exc_TestClass_excep34_E3 s1 = new Exc_TestClass_excep34_E3();
                try
                {
                    try
                    {
                        throw s0;
                    }
                    catch (Exc_TestClass_excep34_E2)
                    {
                        try
                        {
                            throw s1;
                        }
                        catch (Exc_TestClass_excep34_E3)
                        {
                            throw;
                        }
                    }
                }
                catch (Exc_TestClass_excep34_E2)
                {
                    Log.Comment("Unexpected Exc_TestClass_excep34_E2 catch.\n");
                }
                catch (Exc_TestClass_excep34_E3)
                {
                    Log.Comment("Caught in Exc_TestClass_excep34_E3 as expected.\n");
                    retval = 0;
                }
                catch
                {
                    Log.Comment("Unexpected ... catch.\n");
                }
                if (retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
    
    public class Exc_TestClass_excep35_V
    {
        public static int counter = 0;
    }
    public class Exc_TestClass_excep35_E1 : Exception
    {
        public Exc_TestClass_excep35_E1()
        {
            try { }
            catch (Exc_TestClass_excep35_E1)
            {
                ++Exc_TestClass_excep35_V.counter;
            }
        }
    }

        public class Exc_TestClass_excep35
        {
            private static int retval = 2;
            public static int Main_old()
            {
                Exc_TestClass_excep35_E1 s0 = new Exc_TestClass_excep35_E1();
                try
                {
                    throw new Exc_TestClass_excep35_E1();
                }
                catch
                {
                }
                retval = Exc_TestClass_excep35_V.counter;
                if (retval == 0) Log.Comment("PASS");
                else Log.Comment("FAIL");
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }




        public class Exc_TestClass_excep40_E1 : Exception
        {
        }
        public class Exc_TestClass_excep40_E3 : Exception
        {
        }
        public class Exc_TestClass_excep40_F
        {
            public static int xpath = 0;
            public void f01()
            {
                try
                {
                    throw new Exc_TestClass_excep40_E3();
                }
                catch (Exc_TestClass_excep40_E1)
                {
                    xpath = 5;
                }
            }
            public void f00()
            {
                try
                {
                    f01();
                }
                catch (Exception)
                {
                    xpath = 1;
                }
            }
            public void f02()
            {
                try
                {
                    throw new Exc_TestClass_excep40_E3();
                }
                catch (Exc_TestClass_excep40_E1)
                {
                    xpath = 5;
                }
            }
            public void f03()
            {
                try
                {
                    f01();
                }
                catch (Exc_TestClass_excep40_E3)
                {
                    xpath = 1;
                }
            }
        }
            public class Exc_TestClass_excep40
            {
                private static int retval = 3;
                public static int Main_old()
                {
                    Exc_TestClass_excep40_F f = new Exc_TestClass_excep40_F();
                    f.f00();
                    if (Exc_TestClass_excep40_F.xpath == 1)
                        retval--;
                    Exc_TestClass_excep40_F.xpath = 0;
                    f.f03();
                    if (Exc_TestClass_excep40_F.xpath == 1)
                        retval -= 2;
                    if (retval == 0) Log.Comment("PASS");
                    else Log.Comment("FAIL");
                    return retval;
                }
                public static bool testMethod()
                {
                    return (Main_old() == 0);
                }
            }
        
        public class Exc_TestClass_excep41_E1 : Exception
        {
        }
        public class Exc_TestClass_excep41_E3 : Exception
        {
        }
        public class Exc_TestClass_excep41_E4 : Exception
        {
        }
        public class Exc_TestClass_excep41_F
        {
            public static int xpath = 0;
            public void f01()
            {
                try
                {
                    Log.Comment("In f01(), throwing an Exc_TestClass_excep41_E3...");
                    throw new Exc_TestClass_excep41_E3();
                    Log.Comment("After throw in f01().  SHOULD NOT BE HERE!");
                }
                catch (Exc_TestClass_excep41_E1)
                {
                    Log.Comment("In catch in f01()");
                    xpath += 4;
                }
            }
            public void f00()
            {
                try
                {
                    Log.Comment("Calling f01()...");
                    f01();
                    Log.Comment("Returned from f01()");
                }
                catch (Exc_TestClass_excep41_E4)
                {
                    Log.Comment("In catch in f00()");
                    xpath += 2;
                }
            }
        }
            public class Exc_TestClass_excep41
            {
                private static int retval = 1;
                public static int Main_old()
                {
                    Exc_TestClass_excep41_F f = new Exc_TestClass_excep41_F();
                    try
                    {
                        Log.Comment("Calling f00()...");
                        f.f00();
                        Log.Comment("Returned from f00()...");
                    }
                    catch (Exc_TestClass_excep41_E3)
                    {
                        Log.Comment("In catch in f00()");
                        Exc_TestClass_excep41_F.xpath += 1;
                    }
                    if (Exc_TestClass_excep41_F.xpath == 1)
                        retval--;
                    if (retval == 0) Log.Comment("PASS");
                    else Log.Comment("FAIL, xpath=={0} " + (Exc_TestClass_excep41_F.xpath).ToString());
                    return retval;
                }
                public static bool testMethod()
                {
                    return (Main_old() == 0);
                }
            }
        /*
        public class Exc_TestClass_excep42_E1 : Exception
        {

        }
        public class Exc_TestClass_excep42_E3 : Exception
        {
        }
        public class Exc_TestClass_excep42_E4 : Exception
        {
        }
        public class Exc_TestClass_excep42_F
        {
            public static int tb = -1;
            public void f255(int i)
            {
                Exc_TestClass_excep42_E1 s = new Exc_TestClass_excep42_E1();
                Exc_TestClass_excep42_E3 s1 = new Exc_TestClass_excep42_E3();
                Log.Comment("i==" + i.ToString());
                if (i == 255)
                {
                    Log.Comment("Throwing Exc_TestClass_excep42_E1");
                    tb = 0;
                    throw s;
                }
                else
                    throw s1;
            }
            public void f254(int i) { try { f255(++i); } catch (Exc_TestClass_excep42_E4) { tb = 254; } catch { throw; } }
            public void f253(int i) { try { f254(++i); } catch (Exc_TestClass_excep42_E4) { tb = 253; } catch { throw; } }
            public void f252(int i) { try { f253(++i); } catch (Exc_TestClass_excep42_E4) { tb = 252; } catch { throw; } }
            public void f251(int i) { try { f252(++i); } catch (Exc_TestClass_excep42_E4) { tb = 251; } catch { throw; } }
            public void f250(int i) { try { f251(++i); } catch (Exc_TestClass_excep42_E4) { tb = 250; } catch { throw; } }
            public void f249(int i) { try { f250(++i); } catch (Exc_TestClass_excep42_E4) { tb = 249; } catch { throw; } }
            public void f248(int i) { try { f249(++i); } catch (Exc_TestClass_excep42_E4) { tb = 248; } catch { throw; } }
            public void f247(int i) { try { f248(++i); } catch (Exc_TestClass_excep42_E4) { tb = 247; } catch { throw; } }
            public void f246(int i) { try { f247(++i); } catch (Exc_TestClass_excep42_E4) { tb = 246; } catch { throw; } }
            public void f245(int i) { try { f246(++i); } catch (Exc_TestClass_excep42_E4) { tb = 245; } catch { throw; } }
            public void f244(int i) { try { f245(++i); } catch (Exc_TestClass_excep42_E4) { tb = 244; } catch { throw; } }
            public void f243(int i) { try { f244(++i); } catch (Exc_TestClass_excep42_E4) { tb = 243; } catch { throw; } }
            public void f242(int i) { try { f243(++i); } catch (Exc_TestClass_excep42_E4) { tb = 242; } catch { throw; } }
            public void f241(int i) { try { f242(++i); } catch (Exc_TestClass_excep42_E4) { tb = 241; } catch { throw; } }
            public void f240(int i) { try { f241(++i); } catch (Exc_TestClass_excep42_E4) { tb = 240; } catch { throw; } }
            public void f239(int i) { try { f240(++i); } catch (Exc_TestClass_excep42_E4) { tb = 239; } catch { throw; } }
            public void f238(int i) { try { f239(++i); } catch (Exc_TestClass_excep42_E4) { tb = 238; } catch { throw; } }
            public void f237(int i) { try { f238(++i); } catch (Exc_TestClass_excep42_E4) { tb = 237; } catch { throw; } }
            public void f236(int i) { try { f237(++i); } catch (Exc_TestClass_excep42_E4) { tb = 236; } catch { throw; } }
            public void f235(int i) { try { f236(++i); } catch (Exc_TestClass_excep42_E4) { tb = 235; } catch { throw; } }
            public void f234(int i) { try { f235(++i); } catch (Exc_TestClass_excep42_E4) { tb = 234; } catch { throw; } }
            public void f233(int i) { try { f234(++i); } catch (Exc_TestClass_excep42_E4) { tb = 233; } catch { throw; } }
            public void f232(int i) { try { f233(++i); } catch (Exc_TestClass_excep42_E4) { tb = 232; } catch { throw; } }
            public void f231(int i) { try { f232(++i); } catch (Exc_TestClass_excep42_E4) { tb = 231; } catch { throw; } }
            public void f230(int i) { try { f231(++i); } catch (Exc_TestClass_excep42_E4) { tb = 230; } catch { throw; } }
            public void f229(int i) { try { f230(++i); } catch (Exc_TestClass_excep42_E4) { tb = 229; } catch { throw; } }
            public void f228(int i) { try { f229(++i); } catch (Exc_TestClass_excep42_E4) { tb = 228; } catch { throw; } }
            public void f227(int i) { try { f228(++i); } catch (Exc_TestClass_excep42_E4) { tb = 227; } catch { throw; } }
            public void f226(int i) { try { f227(++i); } catch (Exc_TestClass_excep42_E4) { tb = 226; } catch { throw; } }
            public void f225(int i) { try { f226(++i); } catch (Exc_TestClass_excep42_E4) { tb = 225; } catch { throw; } }
            public void f224(int i) { try { f225(++i); } catch (Exc_TestClass_excep42_E4) { tb = 224; } catch { throw; } }
            public void f223(int i) { try { f224(++i); } catch (Exc_TestClass_excep42_E4) { tb = 223; } catch { throw; } }
            public void f222(int i) { try { f223(++i); } catch (Exc_TestClass_excep42_E4) { tb = 222; } catch { throw; } }
            public void f221(int i) { try { f222(++i); } catch (Exc_TestClass_excep42_E4) { tb = 221; } catch { throw; } }
            public void f220(int i) { try { f221(++i); } catch (Exc_TestClass_excep42_E4) { tb = 220; } catch { throw; } }
            public void f219(int i) { try { f220(++i); } catch (Exc_TestClass_excep42_E4) { tb = 219; } catch { throw; } }
            public void f218(int i) { try { f219(++i); } catch (Exc_TestClass_excep42_E4) { tb = 218; } catch { throw; } }
            public void f217(int i) { try { f218(++i); } catch (Exc_TestClass_excep42_E4) { tb = 217; } catch { throw; } }
            public void f216(int i) { try { f217(++i); } catch (Exc_TestClass_excep42_E4) { tb = 216; } catch { throw; } }
            public void f215(int i) { try { f216(++i); } catch (Exc_TestClass_excep42_E4) { tb = 215; } catch { throw; } }
            public void f214(int i) { try { f215(++i); } catch (Exc_TestClass_excep42_E4) { tb = 214; } catch { throw; } }
            public void f213(int i) { try { f214(++i); } catch (Exc_TestClass_excep42_E4) { tb = 213; } catch { throw; } }
            public void f212(int i) { try { f213(++i); } catch (Exc_TestClass_excep42_E4) { tb = 212; } catch { throw; } }
            public void f211(int i) { try { f212(++i); } catch (Exc_TestClass_excep42_E4) { tb = 211; } catch { throw; } }
            public void f210(int i) { try { f211(++i); } catch (Exc_TestClass_excep42_E4) { tb = 210; } catch { throw; } }
            public void f209(int i) { try { f210(++i); } catch (Exc_TestClass_excep42_E4) { tb = 209; } catch { throw; } }
            public void f208(int i) { try { f209(++i); } catch (Exc_TestClass_excep42_E4) { tb = 208; } catch { throw; } }
            public void f207(int i) { try { f208(++i); } catch (Exc_TestClass_excep42_E4) { tb = 207; } catch { throw; } }
            public void f206(int i) { try { f207(++i); } catch (Exc_TestClass_excep42_E4) { tb = 206; } catch { throw; } }
            public void f205(int i) { try { f206(++i); } catch (Exc_TestClass_excep42_E4) { tb = 205; } catch { throw; } }
            public void f204(int i) { try { f205(++i); } catch (Exc_TestClass_excep42_E4) { tb = 204; } catch { throw; } }
            public void f203(int i) { try { f204(++i); } catch (Exc_TestClass_excep42_E4) { tb = 203; } catch { throw; } }
            public void f202(int i) { try { f203(++i); } catch (Exc_TestClass_excep42_E4) { tb = 202; } catch { throw; } }
            public void f201(int i) { try { f202(++i); } catch (Exc_TestClass_excep42_E4) { tb = 201; } catch { throw; } }
            public void f200(int i) { try { f201(++i); } catch (Exc_TestClass_excep42_E4) { tb = 200; } catch { throw; } }
            public void f199(int i) { try { f200(++i); } catch (Exc_TestClass_excep42_E4) { tb = 199; } catch { throw; } }
            public void f198(int i) { try { f199(++i); } catch (Exc_TestClass_excep42_E4) { tb = 198; } catch { throw; } }
            public void f197(int i) { try { f198(++i); } catch (Exc_TestClass_excep42_E4) { tb = 197; } catch { throw; } }
            public void f196(int i) { try { f197(++i); } catch (Exc_TestClass_excep42_E4) { tb = 196; } catch { throw; } }
            public void f195(int i) { try { f196(++i); } catch (Exc_TestClass_excep42_E4) { tb = 195; } catch { throw; } }
            public void f194(int i) { try { f195(++i); } catch (Exc_TestClass_excep42_E4) { tb = 194; } catch { throw; } }
            public void f193(int i) { try { f194(++i); } catch (Exc_TestClass_excep42_E4) { tb = 193; } catch { throw; } }
            public void f192(int i) { try { f193(++i); } catch (Exc_TestClass_excep42_E4) { tb = 192; } catch { throw; } }
            public void f191(int i) { try { f192(++i); } catch (Exc_TestClass_excep42_E4) { tb = 191; } catch { throw; } }
            public void f190(int i) { try { f191(++i); } catch (Exc_TestClass_excep42_E4) { tb = 190; } catch { throw; } }
            public void f189(int i) { try { f190(++i); } catch (Exc_TestClass_excep42_E4) { tb = 189; } catch { throw; } }
            public void f188(int i) { try { f189(++i); } catch (Exc_TestClass_excep42_E4) { tb = 188; } catch { throw; } }
            public void f187(int i) { try { f188(++i); } catch (Exc_TestClass_excep42_E4) { tb = 187; } catch { throw; } }
            public void f186(int i) { try { f187(++i); } catch (Exc_TestClass_excep42_E4) { tb = 186; } catch { throw; } }
            public void f185(int i) { try { f186(++i); } catch (Exc_TestClass_excep42_E4) { tb = 185; } catch { throw; } }
            public void f184(int i) { try { f185(++i); } catch (Exc_TestClass_excep42_E4) { tb = 184; } catch { throw; } }
            public void f183(int i) { try { f184(++i); } catch (Exc_TestClass_excep42_E4) { tb = 183; } catch { throw; } }
            public void f182(int i) { try { f183(++i); } catch (Exc_TestClass_excep42_E4) { tb = 182; } catch { throw; } }
            public void f181(int i) { try { f182(++i); } catch (Exc_TestClass_excep42_E4) { tb = 181; } catch { throw; } }
            public void f180(int i) { try { f181(++i); } catch (Exc_TestClass_excep42_E4) { tb = 180; } catch { throw; } }
            public void f179(int i) { try { f180(++i); } catch (Exc_TestClass_excep42_E4) { tb = 179; } catch { throw; } }
            public void f178(int i) { try { f179(++i); } catch (Exc_TestClass_excep42_E4) { tb = 178; } catch { throw; } }
            public void f177(int i) { try { f178(++i); } catch (Exc_TestClass_excep42_E4) { tb = 177; } catch { throw; } }
            public void f176(int i) { try { f177(++i); } catch (Exc_TestClass_excep42_E4) { tb = 176; } catch { throw; } }
            public void f175(int i) { try { f176(++i); } catch (Exc_TestClass_excep42_E4) { tb = 175; } catch { throw; } }
            public void f174(int i) { try { f175(++i); } catch (Exc_TestClass_excep42_E4) { tb = 174; } catch { throw; } }
            public void f173(int i) { try { f174(++i); } catch (Exc_TestClass_excep42_E4) { tb = 173; } catch { throw; } }
            public void f172(int i) { try { f173(++i); } catch (Exc_TestClass_excep42_E4) { tb = 172; } catch { throw; } }
            public void f171(int i) { try { f172(++i); } catch (Exc_TestClass_excep42_E4) { tb = 171; } catch { throw; } }
            public void f170(int i) { try { f171(++i); } catch (Exc_TestClass_excep42_E4) { tb = 170; } catch { throw; } }
            public void f169(int i) { try { f170(++i); } catch (Exc_TestClass_excep42_E4) { tb = 169; } catch { throw; } }
            public void f168(int i) { try { f169(++i); } catch (Exc_TestClass_excep42_E4) { tb = 168; } catch { throw; } }
            public void f167(int i) { try { f168(++i); } catch (Exc_TestClass_excep42_E4) { tb = 167; } catch { throw; } }
            public void f166(int i) { try { f167(++i); } catch (Exc_TestClass_excep42_E4) { tb = 166; } catch { throw; } }
            public void f165(int i) { try { f166(++i); } catch (Exc_TestClass_excep42_E4) { tb = 165; } catch { throw; } }
            public void f164(int i) { try { f165(++i); } catch (Exc_TestClass_excep42_E4) { tb = 164; } catch { throw; } }
            public void f163(int i) { try { f164(++i); } catch (Exc_TestClass_excep42_E4) { tb = 163; } catch { throw; } }
            public void f162(int i) { try { f163(++i); } catch (Exc_TestClass_excep42_E4) { tb = 162; } catch { throw; } }
            public void f161(int i) { try { f162(++i); } catch (Exc_TestClass_excep42_E4) { tb = 161; } catch { throw; } }
            public void f160(int i) { try { f161(++i); } catch (Exc_TestClass_excep42_E4) { tb = 160; } catch { throw; } }
            public void f159(int i) { try { f160(++i); } catch (Exc_TestClass_excep42_E4) { tb = 159; } catch { throw; } }
            public void f158(int i) { try { f159(++i); } catch (Exc_TestClass_excep42_E4) { tb = 158; } catch { throw; } }
            public void f157(int i) { try { f158(++i); } catch (Exc_TestClass_excep42_E4) { tb = 157; } catch { throw; } }
            public void f156(int i) { try { f157(++i); } catch (Exc_TestClass_excep42_E4) { tb = 156; } catch { throw; } }
            public void f155(int i) { try { f156(++i); } catch (Exc_TestClass_excep42_E4) { tb = 155; } catch { throw; } }
            public void f154(int i) { try { f155(++i); } catch (Exc_TestClass_excep42_E4) { tb = 154; } catch { throw; } }
            public void f153(int i) { try { f154(++i); } catch (Exc_TestClass_excep42_E4) { tb = 153; } catch { throw; } }
            public void f152(int i) { try { f153(++i); } catch (Exc_TestClass_excep42_E4) { tb = 152; } catch { throw; } }
            public void f151(int i) { try { f152(++i); } catch (Exc_TestClass_excep42_E4) { tb = 151; } catch { throw; } }
            public void f150(int i) { try { f151(++i); } catch (Exc_TestClass_excep42_E4) { tb = 150; } catch { throw; } }
            public void f149(int i) { try { f150(++i); } catch (Exc_TestClass_excep42_E4) { tb = 149; } catch { throw; } }
            public void f148(int i) { try { f149(++i); } catch (Exc_TestClass_excep42_E4) { tb = 148; } catch { throw; } }
            public void f147(int i) { try { f148(++i); } catch (Exc_TestClass_excep42_E4) { tb = 147; } catch { throw; } }
            public void f146(int i) { try { f147(++i); } catch (Exc_TestClass_excep42_E4) { tb = 146; } catch { throw; } }
            public void f145(int i) { try { f146(++i); } catch (Exc_TestClass_excep42_E4) { tb = 145; } catch { throw; } }
            public void f144(int i) { try { f145(++i); } catch (Exc_TestClass_excep42_E4) { tb = 144; } catch { throw; } }
            public void f143(int i) { try { f144(++i); } catch (Exc_TestClass_excep42_E4) { tb = 143; } catch { throw; } }
            public void f142(int i) { try { f143(++i); } catch (Exc_TestClass_excep42_E4) { tb = 142; } catch { throw; } }
            public void f141(int i) { try { f142(++i); } catch (Exc_TestClass_excep42_E4) { tb = 141; } catch { throw; } }
            public void f140(int i) { try { f141(++i); } catch (Exc_TestClass_excep42_E4) { tb = 140; } catch { throw; } }
            public void f139(int i) { try { f140(++i); } catch (Exc_TestClass_excep42_E4) { tb = 139; } catch { throw; } }
            public void f138(int i) { try { f139(++i); } catch (Exc_TestClass_excep42_E4) { tb = 138; } catch { throw; } }
            public void f137(int i) { try { f138(++i); } catch (Exc_TestClass_excep42_E4) { tb = 137; } catch { throw; } }
            public void f136(int i) { try { f137(++i); } catch (Exc_TestClass_excep42_E4) { tb = 136; } catch { throw; } }
            public void f135(int i) { try { f136(++i); } catch (Exc_TestClass_excep42_E4) { tb = 135; } catch { throw; } }
            public void f134(int i) { try { f135(++i); } catch (Exc_TestClass_excep42_E4) { tb = 134; } catch { throw; } }
            public void f133(int i) { try { f134(++i); } catch (Exc_TestClass_excep42_E4) { tb = 133; } catch { throw; } }
            public void f132(int i) { try { f133(++i); } catch (Exc_TestClass_excep42_E4) { tb = 132; } catch { throw; } }
            public void f131(int i) { try { f132(++i); } catch (Exc_TestClass_excep42_E4) { tb = 131; } catch { throw; } }
            public void f130(int i) { try { f131(++i); } catch (Exc_TestClass_excep42_E4) { tb = 130; } catch { throw; } }
            public void f129(int i) { try { f130(++i); } catch (Exc_TestClass_excep42_E4) { tb = 129; } catch { throw; } }
            public void f128(int i) { try { f129(++i); } catch (Exc_TestClass_excep42_E4) { tb = 128; } catch { throw; } }
            public void f127(int i) { try { f128(++i); } catch (Exc_TestClass_excep42_E4) { tb = 127; } catch { throw; } }
            public void f126(int i) { try { f127(++i); } catch (Exc_TestClass_excep42_E4) { tb = 126; } catch { throw; } }
            public void f125(int i) { try { f126(++i); } catch (Exc_TestClass_excep42_E4) { tb = 125; } catch { throw; } }
            public void f124(int i) { try { f125(++i); } catch (Exc_TestClass_excep42_E4) { tb = 124; } catch { throw; } }
            public void f123(int i) { try { f124(++i); } catch (Exc_TestClass_excep42_E4) { tb = 123; } catch { throw; } }
            public void f122(int i) { try { f123(++i); } catch (Exc_TestClass_excep42_E4) { tb = 122; } catch { throw; } }
            public void f121(int i) { try { f122(++i); } catch (Exc_TestClass_excep42_E4) { tb = 121; } catch { throw; } }
            public void f120(int i) { try { f121(++i); } catch (Exc_TestClass_excep42_E4) { tb = 120; } catch { throw; } }
            public void f119(int i) { try { f120(++i); } catch (Exc_TestClass_excep42_E4) { tb = 119; } catch { throw; } }
            public void f118(int i) { try { f119(++i); } catch (Exc_TestClass_excep42_E4) { tb = 118; } catch { throw; } }
            public void f117(int i) { try { f118(++i); } catch (Exc_TestClass_excep42_E4) { tb = 117; } catch { throw; } }
            public void f116(int i) { try { f117(++i); } catch (Exc_TestClass_excep42_E4) { tb = 116; } catch { throw; } }
            public void f115(int i) { try { f116(++i); } catch (Exc_TestClass_excep42_E4) { tb = 115; } catch { throw; } }
            public void f114(int i) { try { f115(++i); } catch (Exc_TestClass_excep42_E4) { tb = 114; } catch { throw; } }
            public void f113(int i) { try { f114(++i); } catch (Exc_TestClass_excep42_E4) { tb = 113; } catch { throw; } }
            public void f112(int i) { try { f113(++i); } catch (Exc_TestClass_excep42_E4) { tb = 112; } catch { throw; } }
            public void f111(int i) { try { f112(++i); } catch (Exc_TestClass_excep42_E4) { tb = 111; } catch { throw; } }
            public void f110(int i) { try { f111(++i); } catch (Exc_TestClass_excep42_E4) { tb = 110; } catch { throw; } }
            public void f109(int i) { try { f110(++i); } catch (Exc_TestClass_excep42_E4) { tb = 109; } catch { throw; } }
            public void f108(int i) { try { f109(++i); } catch (Exc_TestClass_excep42_E4) { tb = 108; } catch { throw; } }
            public void f107(int i) { try { f108(++i); } catch (Exc_TestClass_excep42_E4) { tb = 107; } catch { throw; } }
            public void f106(int i) { try { f107(++i); } catch (Exc_TestClass_excep42_E4) { tb = 106; } catch { throw; } }
            public void f105(int i) { try { f106(++i); } catch (Exc_TestClass_excep42_E4) { tb = 105; } catch { throw; } }
            public void f104(int i) { try { f105(++i); } catch (Exc_TestClass_excep42_E4) { tb = 104; } catch { throw; } }
            public void f103(int i) { try { f104(++i); } catch (Exc_TestClass_excep42_E4) { tb = 103; } catch { throw; } }
            public void f102(int i) { try { f103(++i); } catch (Exc_TestClass_excep42_E4) { tb = 102; } catch { throw; } }
            public void f101(int i) { try { f102(++i); } catch (Exc_TestClass_excep42_E4) { tb = 101; } catch { throw; } }
            public void f100(int i) { try { f101(++i); } catch (Exc_TestClass_excep42_E4) { tb = 100; } catch { throw; } }
            public void f099(int i) { try { f100(++i); } catch (Exc_TestClass_excep42_E4) { tb = 99; } catch { throw; } }
            public void f098(int i) { try { f099(++i); } catch (Exc_TestClass_excep42_E4) { tb = 98; } catch { throw; } }
            public void f097(int i) { try { f098(++i); } catch (Exc_TestClass_excep42_E4) { tb = 97; } catch { throw; } }
            public void f096(int i) { try { f097(++i); } catch (Exc_TestClass_excep42_E4) { tb = 96; } catch { throw; } }
            public void f095(int i) { try { f096(++i); } catch (Exc_TestClass_excep42_E4) { tb = 95; } catch { throw; } }
            public void f094(int i) { try { f095(++i); } catch (Exc_TestClass_excep42_E4) { tb = 94; } catch { throw; } }
            public void f093(int i) { try { f094(++i); } catch (Exc_TestClass_excep42_E4) { tb = 93; } catch { throw; } }
            public void f092(int i) { try { f093(++i); } catch (Exc_TestClass_excep42_E4) { tb = 92; } catch { throw; } }
            public void f091(int i) { try { f092(++i); } catch (Exc_TestClass_excep42_E4) { tb = 91; } catch { throw; } }
            public void f090(int i) { try { f091(++i); } catch (Exc_TestClass_excep42_E4) { tb = 90; } catch { throw; } }
            public void f089(int i) { try { f090(++i); } catch (Exc_TestClass_excep42_E4) { tb = 89; } catch { throw; } }
            public void f088(int i) { try { f089(++i); } catch (Exc_TestClass_excep42_E4) { tb = 88; } catch { throw; } }
            public void f087(int i) { try { f088(++i); } catch (Exc_TestClass_excep42_E4) { tb = 87; } catch { throw; } }
            public void f086(int i) { try { f087(++i); } catch (Exc_TestClass_excep42_E4) { tb = 86; } catch { throw; } }
            public void f085(int i) { try { f086(++i); } catch (Exc_TestClass_excep42_E4) { tb = 85; } catch { throw; } }
            public void f084(int i) { try { f085(++i); } catch (Exc_TestClass_excep42_E4) { tb = 84; } catch { throw; } }
            public void f083(int i) { try { f084(++i); } catch (Exc_TestClass_excep42_E4) { tb = 83; } catch { throw; } }
            public void f082(int i) { try { f083(++i); } catch (Exc_TestClass_excep42_E4) { tb = 82; } catch { throw; } }
            public void f081(int i) { try { f082(++i); } catch (Exc_TestClass_excep42_E4) { tb = 81; } catch { throw; } }
            public void f080(int i) { try { f081(++i); } catch (Exc_TestClass_excep42_E4) { tb = 80; } catch { throw; } }
            public void f079(int i) { try { f080(++i); } catch (Exc_TestClass_excep42_E4) { tb = 79; } catch { throw; } }
            public void f078(int i) { try { f079(++i); } catch (Exc_TestClass_excep42_E4) { tb = 78; } catch { throw; } }
            public void f077(int i) { try { f078(++i); } catch (Exc_TestClass_excep42_E4) { tb = 77; } catch { throw; } }
            public void f076(int i) { try { f077(++i); } catch (Exc_TestClass_excep42_E4) { tb = 76; } catch { throw; } }
            public void f075(int i) { try { f076(++i); } catch (Exc_TestClass_excep42_E4) { tb = 75; } catch { throw; } }
            public void f074(int i) { try { f075(++i); } catch (Exc_TestClass_excep42_E4) { tb = 74; } catch { throw; } }
            public void f073(int i) { try { f074(++i); } catch (Exc_TestClass_excep42_E4) { tb = 73; } catch { throw; } }
            public void f072(int i) { try { f073(++i); } catch (Exc_TestClass_excep42_E4) { tb = 72; } catch { throw; } }
            public void f071(int i) { try { f072(++i); } catch (Exc_TestClass_excep42_E4) { tb = 71; } catch { throw; } }
            public void f070(int i) { try { f071(++i); } catch (Exc_TestClass_excep42_E4) { tb = 70; } catch { throw; } }
            public void f069(int i) { try { f070(++i); } catch (Exc_TestClass_excep42_E4) { tb = 69; } catch { throw; } }
            public void f068(int i) { try { f069(++i); } catch (Exc_TestClass_excep42_E4) { tb = 68; } catch { throw; } }
            public void f067(int i) { try { f068(++i); } catch (Exc_TestClass_excep42_E4) { tb = 67; } catch { throw; } }
            public void f066(int i) { try { f067(++i); } catch (Exc_TestClass_excep42_E4) { tb = 66; } catch { throw; } }
            public void f065(int i) { try { f066(++i); } catch (Exc_TestClass_excep42_E4) { tb = 65; } catch { throw; } }
            public void f064(int i) { try { f065(++i); } catch (Exc_TestClass_excep42_E4) { tb = 64; } catch { throw; } }
            public void f063(int i) { try { f064(++i); } catch (Exc_TestClass_excep42_E4) { tb = 63; } catch { throw; } }
            public void f062(int i) { try { f063(++i); } catch (Exc_TestClass_excep42_E4) { tb = 62; } catch { throw; } }
            public void f061(int i) { try { f062(++i); } catch (Exc_TestClass_excep42_E4) { tb = 61; } catch { throw; } }
            public void f060(int i) { try { f061(++i); } catch (Exc_TestClass_excep42_E4) { tb = 60; } catch { throw; } }
            public void f059(int i) { try { f060(++i); } catch (Exc_TestClass_excep42_E4) { tb = 59; } catch { throw; } }
            public void f058(int i) { try { f059(++i); } catch (Exc_TestClass_excep42_E4) { tb = 58; } catch { throw; } }
            public void f057(int i) { try { f058(++i); } catch (Exc_TestClass_excep42_E4) { tb = 57; } catch { throw; } }
            public void f056(int i) { try { f057(++i); } catch (Exc_TestClass_excep42_E4) { tb = 56; } catch { throw; } }
            public void f055(int i) { try { f056(++i); } catch (Exc_TestClass_excep42_E4) { tb = 55; } catch { throw; } }
            public void f054(int i) { try { f055(++i); } catch (Exc_TestClass_excep42_E4) { tb = 54; } catch { throw; } }
            public void f053(int i) { try { f054(++i); } catch (Exc_TestClass_excep42_E4) { tb = 53; } catch { throw; } }
            public void f052(int i) { try { f053(++i); } catch (Exc_TestClass_excep42_E4) { tb = 52; } catch { throw; } }
            public void f051(int i) { try { f052(++i); } catch (Exc_TestClass_excep42_E4) { tb = 51; } catch { throw; } }
            public void f050(int i) { try { f051(++i); } catch (Exc_TestClass_excep42_E4) { tb = 50; } catch { throw; } }
            public void f049(int i) { try { f050(++i); } catch (Exc_TestClass_excep42_E4) { tb = 49; } catch { throw; } }
            public void f048(int i) { try { f049(++i); } catch (Exc_TestClass_excep42_E4) { tb = 48; } catch { throw; } }
            public void f047(int i) { try { f048(++i); } catch (Exc_TestClass_excep42_E4) { tb = 47; } catch { throw; } }
            public void f046(int i) { try { f047(++i); } catch (Exc_TestClass_excep42_E4) { tb = 46; } catch { throw; } }
            public void f045(int i) { try { f046(++i); } catch (Exc_TestClass_excep42_E4) { tb = 45; } catch { throw; } }
            public void f044(int i) { try { f045(++i); } catch (Exc_TestClass_excep42_E4) { tb = 44; } catch { throw; } }
            public void f043(int i) { try { f044(++i); } catch (Exc_TestClass_excep42_E4) { tb = 43; } catch { throw; } }
            public void f042(int i) { try { f043(++i); } catch (Exc_TestClass_excep42_E4) { tb = 42; } catch { throw; } }
            public void f041(int i) { try { f042(++i); } catch (Exc_TestClass_excep42_E4) { tb = 41; } catch { throw; } }
            public void f040(int i) { try { f041(++i); } catch (Exc_TestClass_excep42_E4) { tb = 40; } catch { throw; } }
            public void f039(int i) { try { f040(++i); } catch (Exc_TestClass_excep42_E4) { tb = 39; } catch { throw; } }
            public void f038(int i) { try { f039(++i); } catch (Exc_TestClass_excep42_E4) { tb = 38; } catch { throw; } }
            public void f037(int i) { try { f038(++i); } catch (Exc_TestClass_excep42_E4) { tb = 37; } catch { throw; } }
            public void f036(int i) { try { f037(++i); } catch (Exc_TestClass_excep42_E4) { tb = 36; } catch { throw; } }
            public void f035(int i) { try { f036(++i); } catch (Exc_TestClass_excep42_E4) { tb = 35; } catch { throw; } }
            public void f034(int i) { try { f035(++i); } catch (Exc_TestClass_excep42_E4) { tb = 34; } catch { throw; } }
            public void f033(int i) { try { f034(++i); } catch (Exc_TestClass_excep42_E4) { tb = 33; } catch { throw; } }
            public void f032(int i) { try { f033(++i); } catch (Exc_TestClass_excep42_E4) { tb = 32; } catch { throw; } }
            public void f031(int i) { try { f032(++i); } catch (Exc_TestClass_excep42_E4) { tb = 31; } catch { throw; } }
            public void f030(int i) { try { f031(++i); } catch (Exc_TestClass_excep42_E4) { tb = 30; } catch { throw; } }
            public void f029(int i) { try { f030(++i); } catch (Exc_TestClass_excep42_E4) { tb = 29; } catch { throw; } }
            public void f028(int i) { try { f029(++i); } catch (Exc_TestClass_excep42_E4) { tb = 28; } catch { throw; } }
            public void f027(int i) { try { f028(++i); } catch (Exc_TestClass_excep42_E4) { tb = 27; } catch { throw; } }
            public void f026(int i) { try { f027(++i); } catch (Exc_TestClass_excep42_E4) { tb = 26; } catch { throw; } }
            public void f025(int i) { try { f026(++i); } catch (Exc_TestClass_excep42_E4) { tb = 25; } catch { throw; } }
            public void f024(int i) { try { f025(++i); } catch (Exc_TestClass_excep42_E4) { tb = 24; } catch { throw; } }
            public void f023(int i) { try { f024(++i); } catch (Exc_TestClass_excep42_E4) { tb = 23; } catch { throw; } }
            public void f022(int i) { try { f023(++i); } catch (Exc_TestClass_excep42_E4) { tb = 22; } catch { throw; } }
            public void f021(int i) { try { f022(++i); } catch (Exc_TestClass_excep42_E4) { tb = 21; } catch { throw; } }
            public void f020(int i) { try { f021(++i); } catch (Exc_TestClass_excep42_E4) { tb = 20; } catch { throw; } }
            public void f019(int i) { try { f020(++i); } catch (Exc_TestClass_excep42_E4) { tb = 19; } catch { throw; } }
            public void f018(int i) { try { f019(++i); } catch (Exc_TestClass_excep42_E4) { tb = 18; } catch { throw; } }
            public void f017(int i) { try { f018(++i); } catch (Exc_TestClass_excep42_E4) { tb = 17; } catch { throw; } }
            public void f016(int i) { try { f017(++i); } catch (Exc_TestClass_excep42_E4) { tb = 16; } catch { throw; } }
            public void f015(int i) { try { f016(++i); } catch (Exc_TestClass_excep42_E4) { tb = 15; } catch { throw; } }
            public void f014(int i) { try { f015(++i); } catch (Exc_TestClass_excep42_E4) { tb = 14; } catch { throw; } }
            public void f013(int i) { try { f014(++i); } catch (Exc_TestClass_excep42_E4) { tb = 13; } catch { throw; } }
            public void f012(int i) { try { f013(++i); } catch (Exc_TestClass_excep42_E4) { tb = 12; } catch { throw; } }
            public void f011(int i) { try { f012(++i); } catch (Exc_TestClass_excep42_E4) { tb = 11; } catch { throw; } }
            public void f010(int i) { try { f011(++i); } catch (Exc_TestClass_excep42_E4) { tb = 10; } catch { throw; } }
            public void f009(int i) { try { f010(++i); } catch (Exc_TestClass_excep42_E4) { tb = 9; } catch { throw; } }
            public void f008(int i) { try { f009(++i); } catch (Exc_TestClass_excep42_E4) { tb = 8; } catch { throw; } }
            public void f007(int i) { try { f008(++i); } catch (Exc_TestClass_excep42_E4) { tb = 7; } catch { throw; } }
            public void f006(int i) { try { f007(++i); } catch (Exc_TestClass_excep42_E4) { tb = 6; } catch { throw; } }
            public void f005(int i) { try { f006(++i); } catch (Exc_TestClass_excep42_E4) { tb = 5; } catch { throw; } }
            public void f004(int i) { try { f005(++i); } catch (Exc_TestClass_excep42_E4) { tb = 4; } catch { throw; } }
            public void f003(int i) { try { f004(++i); } catch (Exc_TestClass_excep42_E4) { tb = 3; } catch { throw; } }
            public void f002(int i) { try { f003(++i); } catch (Exc_TestClass_excep42_E4) { tb = 2; } catch { throw; } }
            public void f001(int i) { try { f002(++i); } catch (Exc_TestClass_excep42_E4) { tb = 1; } catch { throw; } }
        }
            public class Exc_TestClass_excep42
            {
                private static int retval = 1;
                public static int Main_old()
                {
                    Exc_TestClass_excep42_F f = new Exc_TestClass_excep42_F();
                    try
                    {
                        f.f001(1);
                    }
                    catch (Exc_TestClass_excep42_E1)
                    {
                        Log.Comment("Caught Exc_TestClass_excep42_E1");
                        retval = Exc_TestClass_excep42_F.tb;
                    }
                    catch
                    {
                        Log.Comment("Did not catch Exc_TestClass_excep42_E1");
                        retval = -1;
                    }
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
        */
        public class Exc_TestClass_excep42b_E1 : Exception
        {

        }
        public class Exc_TestClass_excep42b_E3 : Exception
        {
        }
        public class Exc_TestClass_excep42b_E4 : Exception
        {
        }
        public class Exc_TestClass_excep42b_F
        {
            public static int tb = -1;
            public void f033(int i)
            {
                Exc_TestClass_excep42b_E1 s = new Exc_TestClass_excep42b_E1();
                Exc_TestClass_excep42b_E3 s1 = new Exc_TestClass_excep42b_E3();
                Log.Comment("i==" + i.ToString());
                if (i == 33)
                {
                    Log.Comment("Throwing Exc_TestClass_excep42b_E1");
                    tb = 0;
                    throw s;
                }
                else
                    throw s1;
            }
            public void f032(int i) { try { f033(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 32; } catch { throw; } }
            public void f031(int i) { try { f032(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 31; } catch { throw; } }
            public void f030(int i) { try { f031(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 30; } catch { throw; } }
            public void f029(int i) { try { f030(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 29; } catch { throw; } }
            public void f028(int i) { try { f029(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 28; } catch { throw; } }
            public void f027(int i) { try { f028(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 27; } catch { throw; } }
            public void f026(int i) { try { f027(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 26; } catch { throw; } }
            public void f025(int i) { try { f026(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 25; } catch { throw; } }
            public void f024(int i) { try { f025(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 24; } catch { throw; } }
            public void f023(int i) { try { f024(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 23; } catch { throw; } }
            public void f022(int i) { try { f023(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 22; } catch { throw; } }
            public void f021(int i) { try { f022(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 21; } catch { throw; } }
            public void f020(int i) { try { f021(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 20; } catch { throw; } }
            public void f019(int i) { try { f020(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 19; } catch { throw; } }
            public void f018(int i) { try { f019(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 18; } catch { throw; } }
            public void f017(int i) { try { f018(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 17; } catch { throw; } }
            public void f016(int i) { try { f017(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 16; } catch { throw; } }
            public void f015(int i) { try { f016(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 15; } catch { throw; } }
            public void f014(int i) { try { f015(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 14; } catch { throw; } }
            public void f013(int i) { try { f014(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 13; } catch { throw; } }
            public void f012(int i) { try { f013(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 12; } catch { throw; } }
            public void f011(int i) { try { f012(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 11; } catch { throw; } }
            public void f010(int i) { try { f011(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 10; } catch { throw; } }
            public void f009(int i) { try { f010(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 9; } catch { throw; } }
            public void f008(int i) { try { f009(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 8; } catch { throw; } }
            public void f007(int i) { try { f008(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 7; } catch { throw; } }
            public void f006(int i) { try { f007(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 6; } catch { throw; } }
            public void f005(int i) { try { f006(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 5; } catch { throw; } }
            public void f004(int i) { try { f005(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 4; } catch { throw; } }
            public void f003(int i) { try { f004(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 3; } catch { throw; } }
            public void f002(int i) { try { f003(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 2; } catch { throw; } }
            public void f001(int i) { try { f002(++i); } catch (Exc_TestClass_excep42b_E4) { tb = 1; } catch { throw; } }
        }
            public class Exc_TestClass_excep42b
            {
                private static int retval = 1;
                public static int Main_old()
                {
                    Exc_TestClass_excep42b_F f = new Exc_TestClass_excep42b_F();
                    try
                    {
                        f.f001(1);
                    }
                    catch (Exc_TestClass_excep42b_E1)
                    {
                        Log.Comment("Caught Exc_TestClass_excep42b_E1");
                        retval = Exc_TestClass_excep42b_F.tb;
                    }
                    catch
                    {
                        Log.Comment("Did not catch Exc_TestClass_excep42b_E1");
                        retval = -1;
                    }
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
        /*
        public class Exc_TestClass_excep43_E1 : Exception
        {
        }
        public class Exc_TestClass_excep43_E3 : Exception
        {
        }
        public class Exc_TestClass_excep43_E4 : Exception
        {
        }
        public class Exc_TestClass_excep43_F
        {
            public static int tb = -1;
            public void f255(int i)
            {
                Exc_TestClass_excep43_E1 s = new Exc_TestClass_excep43_E1();
                Exc_TestClass_excep43_E3 s1 = new Exc_TestClass_excep43_E3();
                if (i == 0)
                    throw s;
                else
                    throw s1;
            }
            public void f254(int i) { try { f255(i); } catch (Exc_TestClass_excep43_E4) { tb = 254; } }
            public void f253(int i) { try { f254(i); } catch (Exc_TestClass_excep43_E4) { tb = 253; } }
            public void f252(int i) { try { f253(i); } catch (Exc_TestClass_excep43_E4) { tb = 252; } }
            public void f251(int i) { try { f252(i); } catch (Exc_TestClass_excep43_E4) { tb = 251; } }
            public void f250(int i) { try { f251(i); } catch (Exc_TestClass_excep43_E4) { tb = 250; } }
            public void f249(int i) { try { f250(i); } catch (Exc_TestClass_excep43_E4) { tb = 249; } }
            public void f248(int i) { try { f249(i); } catch (Exc_TestClass_excep43_E4) { tb = 248; } }
            public void f247(int i) { try { f248(i); } catch (Exc_TestClass_excep43_E4) { tb = 247; } }
            public void f246(int i) { try { f247(i); } catch (Exc_TestClass_excep43_E4) { tb = 246; } }
            public void f245(int i) { try { f246(i); } catch (Exc_TestClass_excep43_E4) { tb = 245; } }
            public void f244(int i) { try { f245(i); } catch (Exc_TestClass_excep43_E4) { tb = 244; } }
            public void f243(int i) { try { f244(i); } catch (Exc_TestClass_excep43_E4) { tb = 243; } }
            public void f242(int i) { try { f243(i); } catch (Exc_TestClass_excep43_E4) { tb = 242; } }
            public void f241(int i) { try { f242(i); } catch (Exc_TestClass_excep43_E4) { tb = 241; } }
            public void f240(int i) { try { f241(i); } catch (Exc_TestClass_excep43_E4) { tb = 240; } }
            public void f239(int i) { try { f240(i); } catch (Exc_TestClass_excep43_E4) { tb = 239; } }
            public void f238(int i) { try { f239(i); } catch (Exc_TestClass_excep43_E4) { tb = 238; } }
            public void f237(int i) { try { f238(i); } catch (Exc_TestClass_excep43_E4) { tb = 237; } }
            public void f236(int i) { try { f237(i); } catch (Exc_TestClass_excep43_E4) { tb = 236; } }
            public void f235(int i) { try { f236(i); } catch (Exc_TestClass_excep43_E4) { tb = 235; } }
            public void f234(int i) { try { f235(i); } catch (Exc_TestClass_excep43_E4) { tb = 234; } }
            public void f233(int i) { try { f234(i); } catch (Exc_TestClass_excep43_E4) { tb = 233; } }
            public void f232(int i) { try { f233(i); } catch (Exc_TestClass_excep43_E4) { tb = 232; } }
            public void f231(int i) { try { f232(i); } catch (Exc_TestClass_excep43_E4) { tb = 231; } }
            public void f230(int i) { try { f231(i); } catch (Exc_TestClass_excep43_E4) { tb = 230; } }
            public void f229(int i) { try { f230(i); } catch (Exc_TestClass_excep43_E4) { tb = 229; } }
            public void f228(int i) { try { f229(i); } catch (Exc_TestClass_excep43_E4) { tb = 228; } }
            public void f227(int i) { try { f228(i); } catch (Exc_TestClass_excep43_E4) { tb = 227; } }
            public void f226(int i) { try { f227(i); } catch (Exc_TestClass_excep43_E4) { tb = 226; } }
            public void f225(int i) { try { f226(i); } catch (Exc_TestClass_excep43_E4) { tb = 225; } }
            public void f224(int i) { try { f225(i); } catch (Exc_TestClass_excep43_E4) { tb = 224; } }
            public void f223(int i) { try { f224(i); } catch (Exc_TestClass_excep43_E4) { tb = 223; } }
            public void f222(int i) { try { f223(i); } catch (Exc_TestClass_excep43_E4) { tb = 222; } }
            public void f221(int i) { try { f222(i); } catch (Exc_TestClass_excep43_E4) { tb = 221; } }
            public void f220(int i) { try { f221(i); } catch (Exc_TestClass_excep43_E4) { tb = 220; } }
            public void f219(int i) { try { f220(i); } catch (Exc_TestClass_excep43_E4) { tb = 219; } }
            public void f218(int i) { try { f219(i); } catch (Exc_TestClass_excep43_E4) { tb = 218; } }
            public void f217(int i) { try { f218(i); } catch (Exc_TestClass_excep43_E4) { tb = 217; } }
            public void f216(int i) { try { f217(i); } catch (Exc_TestClass_excep43_E4) { tb = 216; } }
            public void f215(int i) { try { f216(i); } catch (Exc_TestClass_excep43_E4) { tb = 215; } }
            public void f214(int i) { try { f215(i); } catch (Exc_TestClass_excep43_E4) { tb = 214; } }
            public void f213(int i) { try { f214(i); } catch (Exc_TestClass_excep43_E4) { tb = 213; } }
            public void f212(int i) { try { f213(i); } catch (Exc_TestClass_excep43_E4) { tb = 212; } }
            public void f211(int i) { try { f212(i); } catch (Exc_TestClass_excep43_E4) { tb = 211; } }
            public void f210(int i) { try { f211(i); } catch (Exc_TestClass_excep43_E4) { tb = 210; } }
            public void f209(int i) { try { f210(i); } catch (Exc_TestClass_excep43_E4) { tb = 209; } }
            public void f208(int i) { try { f209(i); } catch (Exc_TestClass_excep43_E4) { tb = 208; } }
            public void f207(int i) { try { f208(i); } catch (Exc_TestClass_excep43_E4) { tb = 207; } }
            public void f206(int i) { try { f207(i); } catch (Exc_TestClass_excep43_E4) { tb = 206; } }
            public void f205(int i) { try { f206(i); } catch (Exc_TestClass_excep43_E4) { tb = 205; } }
            public void f204(int i) { try { f205(i); } catch (Exc_TestClass_excep43_E4) { tb = 204; } }
            public void f203(int i) { try { f204(i); } catch (Exc_TestClass_excep43_E4) { tb = 203; } }
            public void f202(int i) { try { f203(i); } catch (Exc_TestClass_excep43_E4) { tb = 202; } }
            public void f201(int i) { try { f202(i); } catch (Exc_TestClass_excep43_E4) { tb = 201; } }
            public void f200(int i) { try { f201(i); } catch (Exc_TestClass_excep43_E4) { tb = 200; } }
            public void f199(int i) { try { f200(i); } catch (Exc_TestClass_excep43_E4) { tb = 199; } }
            public void f198(int i) { try { f199(i); } catch (Exc_TestClass_excep43_E4) { tb = 198; } }
            public void f197(int i) { try { f198(i); } catch (Exc_TestClass_excep43_E4) { tb = 197; } }
            public void f196(int i) { try { f197(i); } catch (Exc_TestClass_excep43_E4) { tb = 196; } }
            public void f195(int i) { try { f196(i); } catch (Exc_TestClass_excep43_E4) { tb = 195; } }
            public void f194(int i) { try { f195(i); } catch (Exc_TestClass_excep43_E4) { tb = 194; } }
            public void f193(int i) { try { f194(i); } catch (Exc_TestClass_excep43_E4) { tb = 193; } }
            public void f192(int i) { try { f193(i); } catch (Exc_TestClass_excep43_E4) { tb = 192; } }
            public void f191(int i) { try { f192(i); } catch (Exc_TestClass_excep43_E4) { tb = 191; } }
            public void f190(int i) { try { f191(i); } catch (Exc_TestClass_excep43_E4) { tb = 190; } }
            public void f189(int i) { try { f190(i); } catch (Exc_TestClass_excep43_E4) { tb = 189; } }
            public void f188(int i) { try { f189(i); } catch (Exc_TestClass_excep43_E4) { tb = 188; } }
            public void f187(int i) { try { f188(i); } catch (Exc_TestClass_excep43_E4) { tb = 187; } }
            public void f186(int i) { try { f187(i); } catch (Exc_TestClass_excep43_E4) { tb = 186; } }
            public void f185(int i) { try { f186(i); } catch (Exc_TestClass_excep43_E4) { tb = 185; } }
            public void f184(int i) { try { f185(i); } catch (Exc_TestClass_excep43_E4) { tb = 184; } }
            public void f183(int i) { try { f184(i); } catch (Exc_TestClass_excep43_E4) { tb = 183; } }
            public void f182(int i) { try { f183(i); } catch (Exc_TestClass_excep43_E4) { tb = 182; } }
            public void f181(int i) { try { f182(i); } catch (Exc_TestClass_excep43_E4) { tb = 181; } }
            public void f180(int i) { try { f181(i); } catch (Exc_TestClass_excep43_E4) { tb = 180; } }
            public void f179(int i) { try { f180(i); } catch (Exc_TestClass_excep43_E4) { tb = 179; } }
            public void f178(int i) { try { f179(i); } catch (Exc_TestClass_excep43_E4) { tb = 178; } }
            public void f177(int i) { try { f178(i); } catch (Exc_TestClass_excep43_E4) { tb = 177; } }
            public void f176(int i) { try { f177(i); } catch (Exc_TestClass_excep43_E4) { tb = 176; } }
            public void f175(int i) { try { f176(i); } catch (Exc_TestClass_excep43_E4) { tb = 175; } }
            public void f174(int i) { try { f175(i); } catch (Exc_TestClass_excep43_E4) { tb = 174; } }
            public void f173(int i) { try { f174(i); } catch (Exc_TestClass_excep43_E4) { tb = 173; } }
            public void f172(int i) { try { f173(i); } catch (Exc_TestClass_excep43_E4) { tb = 172; } }
            public void f171(int i) { try { f172(i); } catch (Exc_TestClass_excep43_E4) { tb = 171; } }
            public void f170(int i) { try { f171(i); } catch (Exc_TestClass_excep43_E4) { tb = 170; } }
            public void f169(int i) { try { f170(i); } catch (Exc_TestClass_excep43_E4) { tb = 169; } }
            public void f168(int i) { try { f169(i); } catch (Exc_TestClass_excep43_E4) { tb = 168; } }
            public void f167(int i) { try { f168(i); } catch (Exc_TestClass_excep43_E4) { tb = 167; } }
            public void f166(int i) { try { f167(i); } catch (Exc_TestClass_excep43_E4) { tb = 166; } }
            public void f165(int i) { try { f166(i); } catch (Exc_TestClass_excep43_E4) { tb = 165; } }
            public void f164(int i) { try { f165(i); } catch (Exc_TestClass_excep43_E4) { tb = 164; } }
            public void f163(int i) { try { f164(i); } catch (Exc_TestClass_excep43_E4) { tb = 163; } }
            public void f162(int i) { try { f163(i); } catch (Exc_TestClass_excep43_E4) { tb = 162; } }
            public void f161(int i) { try { f162(i); } catch (Exc_TestClass_excep43_E4) { tb = 161; } }
            public void f160(int i) { try { f161(i); } catch (Exc_TestClass_excep43_E4) { tb = 160; } }
            public void f159(int i) { try { f160(i); } catch (Exc_TestClass_excep43_E4) { tb = 159; } }
            public void f158(int i) { try { f159(i); } catch (Exc_TestClass_excep43_E4) { tb = 158; } }
            public void f157(int i) { try { f158(i); } catch (Exc_TestClass_excep43_E4) { tb = 157; } }
            public void f156(int i) { try { f157(i); } catch (Exc_TestClass_excep43_E4) { tb = 156; } }
            public void f155(int i) { try { f156(i); } catch (Exc_TestClass_excep43_E4) { tb = 155; } }
            public void f154(int i) { try { f155(i); } catch (Exc_TestClass_excep43_E4) { tb = 154; } }
            public void f153(int i) { try { f154(i); } catch (Exc_TestClass_excep43_E4) { tb = 153; } }
            public void f152(int i) { try { f153(i); } catch (Exc_TestClass_excep43_E4) { tb = 152; } }
            public void f151(int i) { try { f152(i); } catch (Exc_TestClass_excep43_E4) { tb = 151; } }
            public void f150(int i) { try { f151(i); } catch (Exc_TestClass_excep43_E4) { tb = 150; } }
            public void f149(int i) { try { f150(i); } catch (Exc_TestClass_excep43_E4) { tb = 149; } }
            public void f148(int i) { try { f149(i); } catch (Exc_TestClass_excep43_E4) { tb = 148; } }
            public void f147(int i) { try { f148(i); } catch (Exc_TestClass_excep43_E4) { tb = 147; } }
            public void f146(int i) { try { f147(i); } catch (Exc_TestClass_excep43_E4) { tb = 146; } }
            public void f145(int i) { try { f146(i); } catch (Exc_TestClass_excep43_E4) { tb = 145; } }
            public void f144(int i) { try { f145(i); } catch (Exc_TestClass_excep43_E4) { tb = 144; } }
            public void f143(int i) { try { f144(i); } catch (Exc_TestClass_excep43_E4) { tb = 143; } }
            public void f142(int i) { try { f143(i); } catch (Exc_TestClass_excep43_E4) { tb = 142; } }
            public void f141(int i) { try { f142(i); } catch (Exc_TestClass_excep43_E4) { tb = 141; } }
            public void f140(int i) { try { f141(i); } catch (Exc_TestClass_excep43_E4) { tb = 140; } }
            public void f139(int i) { try { f140(i); } catch (Exc_TestClass_excep43_E4) { tb = 139; } }
            public void f138(int i) { try { f139(i); } catch (Exc_TestClass_excep43_E4) { tb = 138; } }
            public void f137(int i) { try { f138(i); } catch (Exc_TestClass_excep43_E4) { tb = 137; } }
            public void f136(int i) { try { f137(i); } catch (Exc_TestClass_excep43_E4) { tb = 136; } }
            public void f135(int i) { try { f136(i); } catch (Exc_TestClass_excep43_E4) { tb = 135; } }
            public void f134(int i) { try { f135(i); } catch (Exc_TestClass_excep43_E4) { tb = 134; } }
            public void f133(int i) { try { f134(i); } catch (Exc_TestClass_excep43_E4) { tb = 133; } }
            public void f132(int i) { try { f133(i); } catch (Exc_TestClass_excep43_E4) { tb = 132; } }
            public void f131(int i) { try { f132(i); } catch (Exc_TestClass_excep43_E4) { tb = 131; } }
            public void f130(int i) { try { f131(i); } catch (Exc_TestClass_excep43_E4) { tb = 130; } }
            public void f129(int i) { try { f130(i); } catch (Exc_TestClass_excep43_E4) { tb = 129; } }
            public void f128(int i) { try { f129(i); } catch (Exc_TestClass_excep43_E4) { tb = 128; } }
            public void f127(int i) { try { f128(i); } catch (Exc_TestClass_excep43_E4) { tb = 127; } }
            public void f126(int i) { try { f127(i); } catch (Exc_TestClass_excep43_E4) { tb = 126; } }
            public void f125(int i) { try { f126(i); } catch (Exc_TestClass_excep43_E4) { tb = 125; } }
            public void f124(int i) { try { f125(i); } catch (Exc_TestClass_excep43_E4) { tb = 124; } }
            public void f123(int i) { try { f124(i); } catch (Exc_TestClass_excep43_E4) { tb = 123; } }
            public void f122(int i) { try { f123(i); } catch (Exc_TestClass_excep43_E4) { tb = 122; } }
            public void f121(int i) { try { f122(i); } catch (Exc_TestClass_excep43_E4) { tb = 121; } }
            public void f120(int i) { try { f121(i); } catch (Exc_TestClass_excep43_E4) { tb = 120; } }
            public void f119(int i) { try { f120(i); } catch (Exc_TestClass_excep43_E4) { tb = 119; } }
            public void f118(int i) { try { f119(i); } catch (Exc_TestClass_excep43_E4) { tb = 118; } }
            public void f117(int i) { try { f118(i); } catch (Exc_TestClass_excep43_E4) { tb = 117; } }
            public void f116(int i) { try { f117(i); } catch (Exc_TestClass_excep43_E4) { tb = 116; } }
            public void f115(int i) { try { f116(i); } catch (Exc_TestClass_excep43_E4) { tb = 115; } }
            public void f114(int i) { try { f115(i); } catch (Exc_TestClass_excep43_E4) { tb = 114; } }
            public void f113(int i) { try { f114(i); } catch (Exc_TestClass_excep43_E4) { tb = 113; } }
            public void f112(int i) { try { f113(i); } catch (Exc_TestClass_excep43_E4) { tb = 112; } }
            public void f111(int i) { try { f112(i); } catch (Exc_TestClass_excep43_E4) { tb = 111; } }
            public void f110(int i) { try { f111(i); } catch (Exc_TestClass_excep43_E4) { tb = 110; } }
            public void f109(int i) { try { f110(i); } catch (Exc_TestClass_excep43_E4) { tb = 109; } }
            public void f108(int i) { try { f109(i); } catch (Exc_TestClass_excep43_E4) { tb = 108; } }
            public void f107(int i) { try { f108(i); } catch (Exc_TestClass_excep43_E4) { tb = 107; } }
            public void f106(int i) { try { f107(i); } catch (Exc_TestClass_excep43_E4) { tb = 106; } }
            public void f105(int i) { try { f106(i); } catch (Exc_TestClass_excep43_E4) { tb = 105; } }
            public void f104(int i) { try { f105(i); } catch (Exc_TestClass_excep43_E4) { tb = 104; } }
            public void f103(int i) { try { f104(i); } catch (Exc_TestClass_excep43_E4) { tb = 103; } }
            public void f102(int i) { try { f103(i); } catch (Exc_TestClass_excep43_E4) { tb = 102; } }
            public void f101(int i) { try { f102(i); } catch (Exc_TestClass_excep43_E4) { tb = 101; } }
            public void f100(int i) { try { f101(i); } catch (Exc_TestClass_excep43_E4) { tb = 100; } }
            public void f099(int i) { try { f100(i); } catch (Exc_TestClass_excep43_E4) { tb = 99; } }
            public void f098(int i) { try { f099(i); } catch (Exc_TestClass_excep43_E4) { tb = 98; } }
            public void f097(int i) { try { f098(i); } catch (Exc_TestClass_excep43_E4) { tb = 97; } }
            public void f096(int i) { try { f097(i); } catch (Exc_TestClass_excep43_E4) { tb = 96; } }
            public void f095(int i) { try { f096(i); } catch (Exc_TestClass_excep43_E4) { tb = 95; } }
            public void f094(int i) { try { f095(i); } catch (Exc_TestClass_excep43_E4) { tb = 94; } }
            public void f093(int i) { try { f094(i); } catch (Exc_TestClass_excep43_E4) { tb = 93; } }
            public void f092(int i) { try { f093(i); } catch (Exc_TestClass_excep43_E4) { tb = 92; } }
            public void f091(int i) { try { f092(i); } catch (Exc_TestClass_excep43_E4) { tb = 91; } }
            public void f090(int i) { try { f091(i); } catch (Exc_TestClass_excep43_E4) { tb = 90; } }
            public void f089(int i) { try { f090(i); } catch (Exc_TestClass_excep43_E4) { tb = 89; } }
            public void f088(int i) { try { f089(i); } catch (Exc_TestClass_excep43_E4) { tb = 88; } }
            public void f087(int i) { try { f088(i); } catch (Exc_TestClass_excep43_E4) { tb = 87; } }
            public void f086(int i) { try { f087(i); } catch (Exc_TestClass_excep43_E4) { tb = 86; } }
            public void f085(int i) { try { f086(i); } catch (Exc_TestClass_excep43_E4) { tb = 85; } }
            public void f084(int i) { try { f085(i); } catch (Exc_TestClass_excep43_E4) { tb = 84; } }
            public void f083(int i) { try { f084(i); } catch (Exc_TestClass_excep43_E4) { tb = 83; } }
            public void f082(int i) { try { f083(i); } catch (Exc_TestClass_excep43_E4) { tb = 82; } }
            public void f081(int i) { try { f082(i); } catch (Exc_TestClass_excep43_E4) { tb = 81; } }
            public void f080(int i) { try { f081(i); } catch (Exc_TestClass_excep43_E4) { tb = 80; } }
            public void f079(int i) { try { f080(i); } catch (Exc_TestClass_excep43_E4) { tb = 79; } }
            public void f078(int i) { try { f079(i); } catch (Exc_TestClass_excep43_E4) { tb = 78; } }
            public void f077(int i) { try { f078(i); } catch (Exc_TestClass_excep43_E4) { tb = 77; } }
            public void f076(int i) { try { f077(i); } catch (Exc_TestClass_excep43_E4) { tb = 76; } }
            public void f075(int i) { try { f076(i); } catch (Exc_TestClass_excep43_E4) { tb = 75; } }
            public void f074(int i) { try { f075(i); } catch (Exc_TestClass_excep43_E4) { tb = 74; } }
            public void f073(int i) { try { f074(i); } catch (Exc_TestClass_excep43_E4) { tb = 73; } }
            public void f072(int i) { try { f073(i); } catch (Exc_TestClass_excep43_E4) { tb = 72; } }
            public void f071(int i) { try { f072(i); } catch (Exc_TestClass_excep43_E4) { tb = 71; } }
            public void f070(int i) { try { f071(i); } catch (Exc_TestClass_excep43_E4) { tb = 70; } }
            public void f069(int i) { try { f070(i); } catch (Exc_TestClass_excep43_E4) { tb = 69; } }
            public void f068(int i) { try { f069(i); } catch (Exc_TestClass_excep43_E4) { tb = 68; } }
            public void f067(int i) { try { f068(i); } catch (Exc_TestClass_excep43_E4) { tb = 67; } }
            public void f066(int i) { try { f067(i); } catch (Exc_TestClass_excep43_E4) { tb = 66; } }
            public void f065(int i) { try { f066(i); } catch (Exc_TestClass_excep43_E4) { tb = 65; } }
            public void f064(int i) { try { f065(i); } catch (Exc_TestClass_excep43_E4) { tb = 64; } }
            public void f063(int i) { try { f064(i); } catch (Exc_TestClass_excep43_E4) { tb = 63; } }
            public void f062(int i) { try { f063(i); } catch (Exc_TestClass_excep43_E4) { tb = 62; } }
            public void f061(int i) { try { f062(i); } catch (Exc_TestClass_excep43_E4) { tb = 61; } }
            public void f060(int i) { try { f061(i); } catch (Exc_TestClass_excep43_E4) { tb = 60; } }
            public void f059(int i) { try { f060(i); } catch (Exc_TestClass_excep43_E4) { tb = 59; } }
            public void f058(int i) { try { f059(i); } catch (Exc_TestClass_excep43_E4) { tb = 58; } }
            public void f057(int i) { try { f058(i); } catch (Exc_TestClass_excep43_E4) { tb = 57; } }
            public void f056(int i) { try { f057(i); } catch (Exc_TestClass_excep43_E4) { tb = 56; } }
            public void f055(int i) { try { f056(i); } catch (Exc_TestClass_excep43_E4) { tb = 55; } }
            public void f054(int i) { try { f055(i); } catch (Exc_TestClass_excep43_E4) { tb = 54; } }
            public void f053(int i) { try { f054(i); } catch (Exc_TestClass_excep43_E4) { tb = 53; } }
            public void f052(int i) { try { f053(i); } catch (Exc_TestClass_excep43_E4) { tb = 52; } }
            public void f051(int i) { try { f052(i); } catch (Exc_TestClass_excep43_E4) { tb = 51; } }
            public void f050(int i) { try { f051(i); } catch (Exc_TestClass_excep43_E4) { tb = 50; } }
            public void f049(int i) { try { f050(i); } catch (Exc_TestClass_excep43_E4) { tb = 49; } }
            public void f048(int i) { try { f049(i); } catch (Exc_TestClass_excep43_E4) { tb = 48; } }
            public void f047(int i) { try { f048(i); } catch (Exc_TestClass_excep43_E4) { tb = 47; } }
            public void f046(int i) { try { f047(i); } catch (Exc_TestClass_excep43_E4) { tb = 46; } }
            public void f045(int i) { try { f046(i); } catch (Exc_TestClass_excep43_E4) { tb = 45; } }
            public void f044(int i) { try { f045(i); } catch (Exc_TestClass_excep43_E4) { tb = 44; } }
            public void f043(int i) { try { f044(i); } catch (Exc_TestClass_excep43_E4) { tb = 43; } }
            public void f042(int i) { try { f043(i); } catch (Exc_TestClass_excep43_E4) { tb = 42; } }
            public void f041(int i) { try { f042(i); } catch (Exc_TestClass_excep43_E4) { tb = 41; } }
            public void f040(int i) { try { f041(i); } catch (Exc_TestClass_excep43_E4) { tb = 40; } }
            public void f039(int i) { try { f040(i); } catch (Exc_TestClass_excep43_E4) { tb = 39; } }
            public void f038(int i) { try { f039(i); } catch (Exc_TestClass_excep43_E4) { tb = 38; } }
            public void f037(int i) { try { f038(i); } catch (Exc_TestClass_excep43_E4) { tb = 37; } }
            public void f036(int i) { try { f037(i); } catch (Exc_TestClass_excep43_E4) { tb = 36; } }
            public void f035(int i) { try { f036(i); } catch (Exc_TestClass_excep43_E4) { tb = 35; } }
            public void f034(int i) { try { f035(i); } catch (Exc_TestClass_excep43_E4) { tb = 34; } }
            public void f033(int i) { try { f034(i); } catch (Exc_TestClass_excep43_E4) { tb = 33; } }
            public void f032(int i) { try { f033(i); } catch (Exc_TestClass_excep43_E4) { tb = 32; } }
            public void f031(int i) { try { f032(i); } catch (Exc_TestClass_excep43_E4) { tb = 31; } }
            public void f030(int i) { try { f031(i); } catch (Exc_TestClass_excep43_E4) { tb = 30; } }
            public void f029(int i) { try { f030(i); } catch (Exc_TestClass_excep43_E4) { tb = 29; } }
            public void f028(int i) { try { f029(i); } catch (Exc_TestClass_excep43_E4) { tb = 28; } }
            public void f027(int i) { try { f028(i); } catch (Exc_TestClass_excep43_E4) { tb = 27; } }
            public void f026(int i) { try { f027(i); } catch (Exc_TestClass_excep43_E4) { tb = 26; } }
            public void f025(int i) { try { f026(i); } catch (Exc_TestClass_excep43_E4) { tb = 25; } }
            public void f024(int i) { try { f025(i); } catch (Exc_TestClass_excep43_E4) { tb = 24; } }
            public void f023(int i) { try { f024(i); } catch (Exc_TestClass_excep43_E4) { tb = 23; } }
            public void f022(int i) { try { f023(i); } catch (Exc_TestClass_excep43_E4) { tb = 22; } }
            public void f021(int i) { try { f022(i); } catch (Exc_TestClass_excep43_E4) { tb = 21; } }
            public void f020(int i) { try { f021(i); } catch (Exc_TestClass_excep43_E4) { tb = 20; } }
            public void f019(int i) { try { f020(i); } catch (Exc_TestClass_excep43_E4) { tb = 19; } }
            public void f018(int i) { try { f019(i); } catch (Exc_TestClass_excep43_E4) { tb = 18; } }
            public void f017(int i) { try { f018(i); } catch (Exc_TestClass_excep43_E4) { tb = 17; } }
            public void f016(int i) { try { f017(i); } catch (Exc_TestClass_excep43_E4) { tb = 16; } }
            public void f015(int i) { try { f016(i); } catch (Exc_TestClass_excep43_E4) { tb = 15; } }
            public void f014(int i) { try { f015(i); } catch (Exc_TestClass_excep43_E4) { tb = 14; } }
            public void f013(int i) { try { f014(i); } catch (Exc_TestClass_excep43_E4) { tb = 13; } }
            public void f012(int i) { try { f013(i); } catch (Exc_TestClass_excep43_E4) { tb = 12; } }
            public void f011(int i) { try { f012(i); } catch (Exc_TestClass_excep43_E4) { tb = 11; } }
            public void f010(int i) { try { f011(i); } catch (Exc_TestClass_excep43_E4) { tb = 10; } }
            public void f009(int i) { try { f010(i); } catch (Exc_TestClass_excep43_E4) { tb = 9; } }
            public void f008(int i) { try { f009(i); } catch (Exc_TestClass_excep43_E4) { tb = 8; } }
            public void f007(int i) { try { f008(i); } catch (Exc_TestClass_excep43_E4) { tb = 7; } }
            public void f006(int i) { try { f007(i); } catch (Exc_TestClass_excep43_E4) { tb = 6; } }
            public void f005(int i) { try { f006(i); } catch (Exc_TestClass_excep43_E4) { tb = 5; } }
            public void f004(int i) { try { f005(i); } catch (Exc_TestClass_excep43_E4) { tb = 4; } }
            public void f003(int i) { try { f004(i); } catch (Exc_TestClass_excep43_E4) { tb = 3; } }
            public void f002(int i) { try { f003(i); } catch (Exc_TestClass_excep43_E4) { tb = 2; } }
            public void f001(int i) { try { f002(i); } catch (Exc_TestClass_excep43_E4) { tb = 1; } }
        }
            public class Exc_TestClass_excep43
            {
                private static int retval = 1;
                public static int Main_old()
                {
                    Exc_TestClass_excep43_F f = new Exc_TestClass_excep43_F();
                    try
                    {
                        f.f001(1);
                    }
                    catch (Exc_TestClass_excep43_E1)
                    {
                        retval += Exc_TestClass_excep43_F.tb;
                    }
                    catch (Exc_TestClass_excep43_E3)
                    {
                        retval = 0;
                    }

                    if (retval == 0) Log.Comment("PASS");
                    else Log.Comment("FAIL, retval=={0 }" + retval.ToString());
                    return retval;
                }
                public static bool testMethod()
                {
                    return (Main_old() == 0);
                }
            }
         */ 
        
        public class Exc_TestClass_excep56
        {
            private static int retval = 2;
            public static int Main_old()
            {
                try
                {
                    Log.Comment("In try block.");
                }
                finally
                {
                    goto lab1;  //This should cause an error
                    Log.Comment("Entering finally block");
                    retval -= 1;
                lab1:
                    retval -= 2;
                }
                Log.Comment("Ready to return.");
                if (0 == retval) Log.Comment("PASS");
                else Log.Comment("FAIL, retval=={0} " + retval.ToString());
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Exc_TestClass_excep57
        {
            private static int retval = 5;
            public static int Main_old()
            {
                try
                {
                    Log.Comment("In try block.");
                }
                finally
                {
                    goto lab4;  //This should cause an error
                    Log.Comment("Entering finally block");
                    retval -= 1;
                lab1:
                    retval -= 2;
                lab2:
                    retval -= 3;
                lab3:
                    retval -= 4;
                lab4:
                    retval -= 5;
                }
                Log.Comment("Ready to return.");
                if (0 == retval) Log.Comment("PASS");
                else Log.Comment("FAIL, retval=={0}" + retval.ToString());
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Exc_TestClass_excep58_E1 : Exception
        {
            public Exc_TestClass_excep58_E1(String str)
                : base(str)
            { }
        }
        public class Exc_TestClass_excep58
        {
            private static int retval = 0x07;
            public static int Main_old()
            {
                try
                {
                    try
                    {
                        Log.Comment("In try block, ready to throw.");
                        throw new Exc_TestClass_excep58_E1("An exception has occurred");
                    }
                    //catch (Exception s){
                    //    Log.Comment ("In catch block.");
                    //    Log.Comment (s.Message);
                    //}
                    finally
                    {
                        Log.Comment("In inner finally block");
                        retval ^= 0x01;
                    }
                }
                catch (Exc_TestClass_excep58_E1 s)
                {
                    if (0 == (retval & 0x01))
                        retval ^= 0x02;
                    Log.Comment("In catch block.");
                    Log.Comment(s.Message);
                }
                catch (Exception)
                {
                    Log.Comment("FAIL -- Should not enter catch (Exception) block");
                    retval++;
                }
                finally
                {
                    Log.Comment("In outer finally block");
                    if (0 == (retval & 0x03))
                        retval ^= 0x04;
                }
                if (0 == retval) Log.Comment("PASS");
                else Log.Comment("FAIL, retval == " + retval.ToString());
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Exc_TestClass_excep59_E1 : Exception
        {
            public Exc_TestClass_excep59_E1(String str)
                : base(str)
            { }
        }
        public class Exc_TestClass_excep59
        {
            private static int retval = 0x0F;
            public static int Main_old()
            {
                try
                {
                    try
                    {
                        try
                        {
                            Log.Comment("In try block, ready to throw.");
                            throw new Exc_TestClass_excep59_E1("An exception has occurred");
                        }
                        //catch (Exception s)
                        //{
                        //	Log.Comment ("In catch block.");
                        //	Log.Comment (s.Message);
                        //}
                        finally
                        {
                            Log.Comment("In innermost finally block");
                            retval ^= 0x01;
                        }
                    }
                    //catch (Exception s){
                    //    Log.Comment ("In catch block.");
                    //    Log.Comment (s.Message);
                    //}
                    finally
                    {
                        Log.Comment("In middle finally block");
                        if (0 == (retval & 0x01))
                            retval ^= 0x02;
                    }
                }
                catch (Exc_TestClass_excep59_E1 s)
                {
                    if (0 == (retval & 0x03))
                        retval ^= 0x04;
                    Log.Comment("In catch block.");
                    Log.Comment(s.Message);
                }
                catch (Exception)
                {
                    Log.Comment("FAIL -- Should not enter catch (Exception) block");
                    retval++;
                }
                finally
                {
                    Log.Comment("In outer finally block");
                    if (0 == (retval & 0x07))
                        retval ^= 0x08;
                }
                if (0 == retval) Log.Comment("PASS");
                else Log.Comment("FAIL, retval == " + retval.ToString());
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Exc_TestClass_excep60_E1 : Exception
        {
            public Exc_TestClass_excep60_E1(String str)
                : base(str)
            { }
        }
        public class Exc_TestClass_excep60
        {
            private static int retval = 0x3F;
            public static int Main_old()
            {
                try
                {
                    try
                    {
                        try
                        {
                            Log.Comment("In try block, ready to throw.");
                            throw new Exc_TestClass_excep60_E1("An exception has occurred");
                        }
                        //catch (Exception s)
                        //{
                        //	Log.Comment ("In catch block.");
                        //	Log.Comment (s.Message);
                        //}
                        finally
                        {
                            Log.Comment("In innermost finally block");
                            retval ^= 0x01;
                        }
                    }
                    catch (Exc_TestClass_excep60_E1 s)
                    {
                        Log.Comment("In catch block.");
                        Log.Comment(s.Message);
                        if (0 == (retval & 0x01))
                            retval ^= 0x02;
                        throw new Exception("Setting InnerException", s);
                    }
                    finally
                    {
                        Log.Comment("In middle finally block");
                        if (0 == (retval & 0x03))
                            retval ^= 0x04;
                    }
                }
                catch (Exception s)
                {
                    if (0 == (retval & 0x07))
                        retval ^= 0x08;
                    Log.Comment("In outer catch block.");
                    if (typeof(Exc_TestClass_excep60_E1) == s.InnerException.GetType())
                        retval ^= 0x10;
                }
                //catch (Exception) {
                //	Log.Comment ("FAIL -- Should not enter catch (Exception) block");
                //	retval ++;
                //}
                finally
                {
                    Log.Comment("In outer finally block");
                    if (0 == (retval & 0x1F))
                        retval ^= 0x20;
                }
                if (0 == retval) Log.Comment("PASS");
                else Log.Comment("FAIL, retval == 0x" + retval.ToString());
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Exc_TestClass_excep61_E1 : Exception
        {
            public Exc_TestClass_excep61_E1(String str)
                : base(str)
            { }
        }
        public class Exc_TestClass_excep61
        {
            private static int retval = 0x7F;
            public static int Main_old()
            {
                try
                {
                    try
                    {
                        try
                        {
                            Log.Comment("In try block, ready to throw.");
                            throw new Exc_TestClass_excep61_E1("An exception has occurred");
                        }
                        catch (Exc_TestClass_excep61_E1 s)
                        {
                            Log.Comment("In catch block.");
                            Log.Comment(s.Message);
                            throw new Exception("Setting InnerException #1", s);
                        }
                        finally
                        {
                            Log.Comment("In innermost finally block");
                            retval ^= 0x01;
                        }
                    }
                    catch (Exception s)
                    {
                        Log.Comment("In catch block.");
                        Log.Comment(s.Message);
                        if (0 == (retval & 0x01))
                            retval ^= 0x02;
                        throw new Exception("Setting InnerException 2", s);
                    }
                    finally
                    {
                        Log.Comment("In middle finally block");
                        if (0 == (retval & 0x03))
                            retval ^= 0x04;
                    }
                }
                catch (Exception s)
                {
                    if (0 == (retval & 0x07))
                        retval ^= 0x08;
                    Log.Comment("In outer catch block.");
                    if (typeof(Exception) == s.InnerException.GetType())
                        retval ^= 0x10;
                    if (typeof(Exc_TestClass_excep61_E1) == s.InnerException.InnerException.GetType())
                        retval ^= 0x20;
                }
                //catch (Exception) {
                //	Log.Comment ("FAIL -- Should not enter catch (Exception) block");
                //	retval ++;
                //}
                finally
                {
                    Log.Comment("In outer finally block");
                    if (0 == (retval & 0x1F))
                        retval ^= 0x40;
                }
                if (0 == retval) Log.Comment("PASS");
                else Log.Comment("FAIL, retval == 0x" + retval.ToString());
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Exc_TestClass_excep62_E1 : Exception
        {
            public Exc_TestClass_excep62_E1(String str)
                : base(str)
            { }
        }
        public class Exc_TestClass_excep62
        {
            private static int retval = 0x03;
            public static int Main_old()
            {
                try
                {
                    Log.Comment("In try block, ready to throw.");
                    throw new Exception("An exception has occurred");
                }
                catch (Exception s)
                {
                    if (null == s.InnerException)
                        retval ^= 0x01;
                }
                //catch (Exception) {
                //	Log.Comment ("FAIL -- Should not enter catch (Exception) block");
                //	retval ++;
                //}
                finally
                {
                    Log.Comment("In outer finally block");
                    if (0 == (retval & 0x01))
                        retval ^= 0x02;
                }
                if (0 == retval) Log.Comment("PASS");
                else Log.Comment("FAIL, retval == 0x" + retval.ToString());
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Exc_TestClass_excep63_E1 : Exception
        {
            public Exc_TestClass_excep63_E1(String str)
                : base(str)
            { }
        }
        public class Exc_TestClass_excep63_SC
        {
            static Exc_TestClass_excep63_SC()
            {
                InitHelper();
            }
            public static void InitHelper()
            {
                Log.Comment("This is bug number: 21724	Resolved By Design.");
                Log.Comment("This is bug number: 21724	If a catch search reaches a static ctor then a Exception is thrown at the point of static ctor invocation. The inner exception is the original exception. ");
                Log.Comment("When this bug is fixed change remove the comment below.");

//                throw new Exception("Exception in InitHelper");
            }
        }
        public class Exc_TestClass_excep63
        {
            private static int retval = 0x01;
            public static int Main_old()
            {
                try
                {
                    Exc_TestClass_excep63_SC sc = new Exc_TestClass_excep63_SC();
                }
                catch (Exception t)
                {
                    if (t.InnerException.GetType() == typeof(Exception))
                        retval ^= 0x01;
                }
                if (0 == retval) Log.Comment("PASS");
                else Log.Comment("FAIL, retval == 0x" + retval.ToString());
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Exc_TestClass_excep64_E1 : Exception
        {
            public Exc_TestClass_excep64_E1(String str)
                : base(str)
            { }
        }
        public class Exc_TestClass_excep64_SC
        {
            public static int StaticInt = InitHelper();
            public static int InitHelper()
            {
                Log.Comment("This is bug number: 21724	Resolved By Design.");
                Log.Comment("This is bug number: 21724	If a catch search reaches a static ctor then a Exception is thrown at the point of static ctor invocation. The inner exception is the original exception. ");
                Log.Comment("When this bug is fixed change this back to pass and chang eht known failure to fail");

//                throw new Exception("Exception in InitHelper");
                return 0;
            }
        }
        public class Exc_TestClass_excep64
        {
            private static int retval = 0x01;
            public static int Main_old()
            {
                try
                {
                    Exc_TestClass_excep64_SC sc = new Exc_TestClass_excep64_SC();
                }
                catch (Exception t)
                {
                    if (t.InnerException.GetType() == typeof(Exception))
                        retval ^= 0x01;
                }
                if (0 == retval) Log.Comment("PASS");
                else Log.Comment("FAIL, retval == 0x" + retval.ToString());
                return retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        /*
        public class Exc_TestClass_excep65_E1 : Exception
        {
            public Exc_TestClass_excep65_E1(String str)
                : base(str)
            { }
        }
        public class Exc_TestClass_excep65_C1
        {
            public static int retval = 0x01;
            ~Exc_TestClass_excep65_C1()
            {
                retval ^= 0x01;
                Log.Comment("retval == " + retval.ToString());
                Log.Comment("In// Exc_TestClass_excep65_C1()");
            }
        }
        public class Exc_TestClass_excep65_C2 : Exc_TestClass_excep65_C1
        {
        }
        public class Derived : Exc_TestClass_excep65_C2
        {
            ~Derived()
            {
                Log.Comment("In ~Derived()");
                DtorHelper();
                Log.Comment("FAIL, did not exit dtor when exception thrown");
                Exc_TestClass_excep65_C1.retval |= 0x02;
            }
            public static void DtorHelper()
            {
                throw new Exception("Exception in DtorHelper");
            }
        }
        public class Exc_TestClass_excep65
        {
            public static int Main_old()
            {
                Derived sc = new Derived();
                sc = null;
                Microsoft.SPOT.Debug.GC(true);
                System.Threading.Thread.Sleep(100);
                return Exc_TestClass_excep65_C1.retval;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
         */ 
    }
}
