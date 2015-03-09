////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class StructClassTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");
            Log.Comment("Adding set up for the tests");
            Log.Comment("These tests examine the conversion (casting) of structs to classes ");
            Log.Comment("There is a tree of classes are TDer : T : TBase and a struct S");
            Log.Comment("The names of the tests describe the tests by listing which two objects will be converted between");
            Log.Comment("Followed by which (The source or the destination) will contain a cast definition");
            Log.Comment("Followed further by 'i's or 'e's to indicate which of the cast definition and the actual cast are");
            Log.Comment("implicit or explicit.");
            Log.Comment("");
            Log.Comment("For example, TBase_S_Source_i_e tests the conversion of TBase to S, with an implicit definition");
            Log.Comment("of the cast in the TBase class, and an explicit cast in the body of the method.");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
        }

        //StructClass Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\StructClass
        //S_TBase_Source_i_e,S_TBase_Source_e_e,S_TBase_Dest_i_e,S_TBase_Dest_e_e,S_T_Source_i_i,S_T_Source_i_e,S_T_Source_e_e,S_T_Dest_i_i,S_T_Dest_i_e,S_T_Dest_e_e,S_TDer_Source_i_i,S_TDer_Source_i_e,S_TDer_Source_e_e.cs) -- passed



        //Test Case Calls 
        [TestMethod]
        public MFTestResults StructClass_S_TBase_Source_i_e_Test()
        {
            if (StructClassTestClass_S_TBase_Source_i_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults StructClass_S_TBase_Source_e_e_Test()
        {
            if (StructClassTestClass_S_TBase_Source_e_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults StructClass_S_TBase_Dest_i_e_Test()
        {
            if (StructClassTestClass_S_TBase_Dest_i_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults StructClass_S_TBase_Dest_e_e_Test()
        {
            if (StructClassTestClass_S_TBase_Dest_e_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults StructClass_S_T_Source_i_i_Test()
        {
            if (StructClassTestClass_S_T_Source_i_i.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults StructClass_S_T_Source_i_e_Test()
        {
            if (StructClassTestClass_S_T_Source_i_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults StructClass_S_T_Source_e_e_Test()
        {
            if (StructClassTestClass_S_T_Source_e_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults StructClass_S_T_Dest_i_i_Test()
        {
            if (StructClassTestClass_S_T_Dest_i_i.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults StructClass_S_T_Dest_i_e_Test()
        {
            if (StructClassTestClass_S_T_Dest_i_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults StructClass_S_T_Dest_e_e_Test()
        {
            if (StructClassTestClass_S_T_Dest_e_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults StructClass_S_TDer_Source_i_i_Test()
        {
            if (StructClassTestClass_S_TDer_Source_i_i.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults StructClass_S_TDer_Source_i_e_Test()
        {
            if (StructClassTestClass_S_TDer_Source_i_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults StructClass_S_TDer_Source_e_e_Test()
        {
            if (StructClassTestClass_S_TDer_Source_e_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        //Compiled Test Cases 
        struct StructClassTestClass_S_TBase_Source_i_e_S
        {
            static public implicit operator StructClassTestClass_S_TBase_Source_i_e_TBase(StructClassTestClass_S_TBase_Source_i_e_S foo)
            {
                Log.Comment(" StructClassTestClass_S_TBase_Source_i_e_S to  StructClassTestClass_S_TBase_Source_i_e_TBase Source implicit");
                return new StructClassTestClass_S_TBase_Source_i_e_TBase();
            }
        }
        class StructClassTestClass_S_TBase_Source_i_e_TBase
        {
        }
        class StructClassTestClass_S_TBase_Source_i_e_T : StructClassTestClass_S_TBase_Source_i_e_TBase
        {
        }
        class StructClassTestClass_S_TBase_Source_i_e_TDer : StructClassTestClass_S_TBase_Source_i_e_T
        {
        }
        class StructClassTestClass_S_TBase_Source_i_e
        {
            public static void Main_old()
            {
                StructClassTestClass_S_TBase_Source_i_e_S s = new StructClassTestClass_S_TBase_Source_i_e_S();
                StructClassTestClass_S_TBase_Source_i_e_T t;
                try
                {
                    t = (StructClassTestClass_S_TBase_Source_i_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception Caught");
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        struct StructClassTestClass_S_TBase_Source_e_e_S
        {
            static public explicit operator StructClassTestClass_S_TBase_Source_e_e_TBase(StructClassTestClass_S_TBase_Source_e_e_S foo)
            {
                Log.Comment(" StructClassTestClass_S_TBase_Source_e_e_S to  StructClassTestClass_S_TBase_Source_e_e_TBase Source explicit");
                return new StructClassTestClass_S_TBase_Source_e_e_TBase();
            }
        }
        class StructClassTestClass_S_TBase_Source_e_e_TBase
        {
        }
        class StructClassTestClass_S_TBase_Source_e_e_T : StructClassTestClass_S_TBase_Source_e_e_TBase
        {
        }
        class StructClassTestClass_S_TBase_Source_e_e_TDer : StructClassTestClass_S_TBase_Source_e_e_T
        {
        }
        class StructClassTestClass_S_TBase_Source_e_e
        {
            public static void Main_old()
            {
                StructClassTestClass_S_TBase_Source_e_e_S s = new StructClassTestClass_S_TBase_Source_e_e_S();
                StructClassTestClass_S_TBase_Source_e_e_T t;
                try
                {
                    t = (StructClassTestClass_S_TBase_Source_e_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception Caught");
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        struct StructClassTestClass_S_TBase_Dest_i_e_S
        {
        }
        class StructClassTestClass_S_TBase_Dest_i_e_TBase
        {
            static public implicit operator StructClassTestClass_S_TBase_Dest_i_e_TBase(StructClassTestClass_S_TBase_Dest_i_e_S foo)
            {
                Log.Comment(" StructClassTestClass_S_TBase_Dest_i_e_S to  StructClassTestClass_S_TBase_Dest_i_e_TBase Dest implicit");
                return new StructClassTestClass_S_TBase_Dest_i_e_TBase();
            }
        }
        class StructClassTestClass_S_TBase_Dest_i_e_T : StructClassTestClass_S_TBase_Dest_i_e_TBase
        {
        }
        class StructClassTestClass_S_TBase_Dest_i_e_TDer : StructClassTestClass_S_TBase_Dest_i_e_T
        {
        }
        class StructClassTestClass_S_TBase_Dest_i_e
        {
            public static void Main_old()
            {
                StructClassTestClass_S_TBase_Dest_i_e_S s = new StructClassTestClass_S_TBase_Dest_i_e_S();
                StructClassTestClass_S_TBase_Dest_i_e_T t;
                try
                {
                    t = (StructClassTestClass_S_TBase_Dest_i_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception Caught");
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        struct StructClassTestClass_S_TBase_Dest_e_e_S
        {
        }
        class StructClassTestClass_S_TBase_Dest_e_e_TBase
        {
            static public explicit operator StructClassTestClass_S_TBase_Dest_e_e_TBase(StructClassTestClass_S_TBase_Dest_e_e_S foo)
            {
                Log.Comment(" StructClassTestClass_S_TBase_Dest_e_e_S to  StructClassTestClass_S_TBase_Dest_e_e_TBase Dest explicit");
                return new StructClassTestClass_S_TBase_Dest_e_e_TBase();
            }
        }
        class StructClassTestClass_S_TBase_Dest_e_e_T : StructClassTestClass_S_TBase_Dest_e_e_TBase
        {
        }
        class StructClassTestClass_S_TBase_Dest_e_e_TDer : StructClassTestClass_S_TBase_Dest_e_e_T
        {
        }
        class StructClassTestClass_S_TBase_Dest_e_e
        {
            public static void Main_old()
            {
                StructClassTestClass_S_TBase_Dest_e_e_S s = new StructClassTestClass_S_TBase_Dest_e_e_S();
                StructClassTestClass_S_TBase_Dest_e_e_T t;
                try
                {
                    t = (StructClassTestClass_S_TBase_Dest_e_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception Caught");
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        struct StructClassTestClass_S_T_Source_i_i_S
        {
            static public implicit operator StructClassTestClass_S_T_Source_i_i_T(StructClassTestClass_S_T_Source_i_i_S foo)
            {
                Log.Comment(" StructClassTestClass_S_T_Source_i_i_S to  StructClassTestClass_S_T_Source_i_i_T Source implicit");
                return new StructClassTestClass_S_T_Source_i_i_T();
            }
        }
        class StructClassTestClass_S_T_Source_i_i_TBase
        {
        }
        class StructClassTestClass_S_T_Source_i_i_T : StructClassTestClass_S_T_Source_i_i_TBase
        {
        }
        class StructClassTestClass_S_T_Source_i_i_TDer : StructClassTestClass_S_T_Source_i_i_T
        {
        }
        class StructClassTestClass_S_T_Source_i_i
        {
            public static void Main_old()
            {
                StructClassTestClass_S_T_Source_i_i_S s = new StructClassTestClass_S_T_Source_i_i_S();
                StructClassTestClass_S_T_Source_i_i_T t;
                try
                {
                    t = s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception Caught");
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        struct StructClassTestClass_S_T_Source_i_e_S
        {
            static public implicit operator StructClassTestClass_S_T_Source_i_e_T(StructClassTestClass_S_T_Source_i_e_S foo)
            {
                Log.Comment(" StructClassTestClass_S_T_Source_i_e_S to  StructClassTestClass_S_T_Source_i_e_T Source implicit");
                return new StructClassTestClass_S_T_Source_i_e_T();
            }
        }
        class StructClassTestClass_S_T_Source_i_e_TBase
        {
        }
        class StructClassTestClass_S_T_Source_i_e_T : StructClassTestClass_S_T_Source_i_e_TBase
        {
        }
        class StructClassTestClass_S_T_Source_i_e_TDer : StructClassTestClass_S_T_Source_i_e_T
        {
        }
        class StructClassTestClass_S_T_Source_i_e
        {
            public static void Main_old()
            {
                StructClassTestClass_S_T_Source_i_e_S s = new StructClassTestClass_S_T_Source_i_e_S();
                StructClassTestClass_S_T_Source_i_e_T t;
                try
                {
                    t = (StructClassTestClass_S_T_Source_i_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception Caught");
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        struct StructClassTestClass_S_T_Source_e_e_S
        {
            static public explicit operator StructClassTestClass_S_T_Source_e_e_T(StructClassTestClass_S_T_Source_e_e_S foo)
            {
                Log.Comment(" StructClassTestClass_S_T_Source_e_e_S to  StructClassTestClass_S_T_Source_e_e_T Source explicit");
                return new StructClassTestClass_S_T_Source_e_e_T();
            }
        }
        class StructClassTestClass_S_T_Source_e_e_TBase
        {
        }
        class StructClassTestClass_S_T_Source_e_e_T : StructClassTestClass_S_T_Source_e_e_TBase
        {
        }
        class StructClassTestClass_S_T_Source_e_e_TDer : StructClassTestClass_S_T_Source_e_e_T
        {
        }
        class StructClassTestClass_S_T_Source_e_e
        {
            public static void Main_old()
            {
                StructClassTestClass_S_T_Source_e_e_S s = new StructClassTestClass_S_T_Source_e_e_S();
                StructClassTestClass_S_T_Source_e_e_T t;
                try
                {
                    t = (StructClassTestClass_S_T_Source_e_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception Caught");
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        struct StructClassTestClass_S_T_Dest_i_i_S
        {
        }
        class StructClassTestClass_S_T_Dest_i_i_TBase
        {
        }
        class StructClassTestClass_S_T_Dest_i_i_T : StructClassTestClass_S_T_Dest_i_i_TBase
        {
            static public implicit operator StructClassTestClass_S_T_Dest_i_i_T(StructClassTestClass_S_T_Dest_i_i_S foo)
            {
                Log.Comment(" StructClassTestClass_S_T_Dest_i_i_S to  StructClassTestClass_S_T_Dest_i_i_T Dest implicit");
                return new StructClassTestClass_S_T_Dest_i_i_T();
            }
        }
        class StructClassTestClass_S_T_Dest_i_i_TDer : StructClassTestClass_S_T_Dest_i_i_T
        {
        }
        class StructClassTestClass_S_T_Dest_i_i
        {
            public static void Main_old()
            {
                StructClassTestClass_S_T_Dest_i_i_S s = new StructClassTestClass_S_T_Dest_i_i_S();
                StructClassTestClass_S_T_Dest_i_i_T t;
                try
                {
                    t = s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception Caught");
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        struct StructClassTestClass_S_T_Dest_i_e_S
        {
        }
        class StructClassTestClass_S_T_Dest_i_e_TBase
        {
        }
        class StructClassTestClass_S_T_Dest_i_e_T : StructClassTestClass_S_T_Dest_i_e_TBase
        {
            static public implicit operator StructClassTestClass_S_T_Dest_i_e_T(StructClassTestClass_S_T_Dest_i_e_S foo)
            {
                Log.Comment(" StructClassTestClass_S_T_Dest_i_e_S to  StructClassTestClass_S_T_Dest_i_e_T Dest implicit");
                return new StructClassTestClass_S_T_Dest_i_e_T();
            }
        }
        class StructClassTestClass_S_T_Dest_i_e_TDer : StructClassTestClass_S_T_Dest_i_e_T
        {
        }
        class StructClassTestClass_S_T_Dest_i_e
        {
            public static void Main_old()
            {
                StructClassTestClass_S_T_Dest_i_e_S s = new StructClassTestClass_S_T_Dest_i_e_S();
                StructClassTestClass_S_T_Dest_i_e_T t;
                try
                {
                    t = (StructClassTestClass_S_T_Dest_i_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception Caught");
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        struct StructClassTestClass_S_T_Dest_e_e_S
        {
        }
        class StructClassTestClass_S_T_Dest_e_e_TBase
        {
        }
        class StructClassTestClass_S_T_Dest_e_e_T : StructClassTestClass_S_T_Dest_e_e_TBase
        {
            static public explicit operator StructClassTestClass_S_T_Dest_e_e_T(StructClassTestClass_S_T_Dest_e_e_S foo)
            {
                Log.Comment(" StructClassTestClass_S_T_Dest_e_e_S to  StructClassTestClass_S_T_Dest_e_e_T Dest explicit");
                return new StructClassTestClass_S_T_Dest_e_e_T();
            }
        }
        class StructClassTestClass_S_T_Dest_e_e_TDer : StructClassTestClass_S_T_Dest_e_e_T
        {
        }
        class StructClassTestClass_S_T_Dest_e_e
        {
            public static void Main_old()
            {
                StructClassTestClass_S_T_Dest_e_e_S s = new StructClassTestClass_S_T_Dest_e_e_S();
                StructClassTestClass_S_T_Dest_e_e_T t;
                try
                {
                    t = (StructClassTestClass_S_T_Dest_e_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception Caught");
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        struct StructClassTestClass_S_TDer_Source_i_i_S
        {
            static public implicit operator StructClassTestClass_S_TDer_Source_i_i_TDer(StructClassTestClass_S_TDer_Source_i_i_S foo)
            {
                Log.Comment(" StructClassTestClass_S_TDer_Source_i_i_S to  StructClassTestClass_S_TDer_Source_i_i_TDer Source implicit");
                return new StructClassTestClass_S_TDer_Source_i_i_TDer();
            }
        }
        class StructClassTestClass_S_TDer_Source_i_i_TBase
        {
        }
        class StructClassTestClass_S_TDer_Source_i_i_T : StructClassTestClass_S_TDer_Source_i_i_TBase
        {
        }
        class StructClassTestClass_S_TDer_Source_i_i_TDer : StructClassTestClass_S_TDer_Source_i_i_T
        {
        }
        class StructClassTestClass_S_TDer_Source_i_i
        {
            public static void Main_old()
            {
                StructClassTestClass_S_TDer_Source_i_i_S s = new StructClassTestClass_S_TDer_Source_i_i_S();
                StructClassTestClass_S_TDer_Source_i_i_T t;
                try
                {
                    t = s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception Caught");
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        struct StructClassTestClass_S_TDer_Source_i_e_S
        {
            static public implicit operator StructClassTestClass_S_TDer_Source_i_e_TDer(StructClassTestClass_S_TDer_Source_i_e_S foo)
            {
                Log.Comment(" StructClassTestClass_S_TDer_Source_i_e_S to  StructClassTestClass_S_TDer_Source_i_e_TDer Source implicit");
                return new StructClassTestClass_S_TDer_Source_i_e_TDer();
            }
        }
        class StructClassTestClass_S_TDer_Source_i_e_TBase
        {
        }
        class StructClassTestClass_S_TDer_Source_i_e_T : StructClassTestClass_S_TDer_Source_i_e_TBase
        {
        }
        class StructClassTestClass_S_TDer_Source_i_e_TDer : StructClassTestClass_S_TDer_Source_i_e_T
        {
        }
        class StructClassTestClass_S_TDer_Source_i_e
        {
            public static void Main_old()
            {
                StructClassTestClass_S_TDer_Source_i_e_S s = new StructClassTestClass_S_TDer_Source_i_e_S();
                StructClassTestClass_S_TDer_Source_i_e_T t;
                try
                {
                    t = (StructClassTestClass_S_TDer_Source_i_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception Caught");
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        struct StructClassTestClass_S_TDer_Source_e_e_S
        {
            static public explicit operator StructClassTestClass_S_TDer_Source_e_e_TDer(StructClassTestClass_S_TDer_Source_e_e_S foo)
            {
                Log.Comment(" StructClassTestClass_S_TDer_Source_e_e_S to  StructClassTestClass_S_TDer_Source_e_e_TDer Source explicit");
                return new StructClassTestClass_S_TDer_Source_e_e_TDer();
            }
        }
        class StructClassTestClass_S_TDer_Source_e_e_TBase
        {
        }
        class StructClassTestClass_S_TDer_Source_e_e_T : StructClassTestClass_S_TDer_Source_e_e_TBase
        {
        }
        class StructClassTestClass_S_TDer_Source_e_e_TDer : StructClassTestClass_S_TDer_Source_e_e_T
        {
        }
        class StructClassTestClass_S_TDer_Source_e_e
        {
            public static void Main_old()
            {
                StructClassTestClass_S_TDer_Source_e_e_S s = new StructClassTestClass_S_TDer_Source_e_e_S();
                StructClassTestClass_S_TDer_Source_e_e_T t;
                try
                {
                    t = (StructClassTestClass_S_TDer_Source_e_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception Caught");
                }
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }

    }
}

