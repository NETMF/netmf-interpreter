////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ClassStructTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");

            Log.Comment("Adding set up for the tests");
            Log.Comment("These tests examine the conversion (casting) of classes to structs");
            Log.Comment("The is a tree of classes SDer : S : SBase and a struct T");
            Log.Comment("The names of the tests describe the tests by listing which two objects will be converted between");
            Log.Comment("Followed by which (The source or the destination) will contain a cast definition");
            Log.Comment("Followed further by 'i's or 'e's to indicate which of the cast definition and the actual cast are");
            Log.Comment("implicit or explicit.");
            Log.Comment("");
            Log.Comment("For example, SBase_T_Source_i_e tests the conversion of SBase to T, with an implicit definition");
            Log.Comment("of the cast in the SBase class, and an explicit cast in the body of the method.");
            // Add your functionality here.                

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
        }

        //ClassStruct Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\ClassStruct

        //Test Case Calls 
        [TestMethod]
        public MFTestResults ClassStruct_SBase_T_Source_i_i_Test()
        {
            if (ClassStructTestClass_SBase_T_Source_i_i.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ClassStruct_SBase_T_Source_i_e_Test()
        {
            if (ClassStructTestClass_SBase_T_Source_i_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ClassStruct_SBase_T_Source_e_e_Test()
        {
            if (ClassStructTestClass_SBase_T_Source_e_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ClassStruct_SBase_T_Dest_i_i_Test()
        {
            if (ClassStructTestClass_SBase_T_Dest_i_i.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ClassStruct_SBase_T_Dest_i_e_Test()
        {
            if (ClassStructTestClass_SBase_T_Dest_i_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ClassStruct_SBase_T_Dest_e_e_Test()
        {
            if (ClassStructTestClass_SBase_T_Dest_e_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ClassStruct_S_T_Source_i_i_Test()
        {
            if (ClassStructTestClass_S_T_Source_i_i.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ClassStruct_S_T_Source_i_e_Test()
        {
            if (ClassStructTestClass_S_T_Source_i_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ClassStruct_S_T_Source_e_e_Test()
        {
            if (ClassStructTestClass_S_T_Source_e_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ClassStruct_S_T_Dest_i_i_Test()
        {
            if (ClassStructTestClass_S_T_Dest_i_i.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ClassStruct_S_T_Dest_i_e_Test()
        {
            if (ClassStructTestClass_S_T_Dest_i_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ClassStruct_S_T_Dest_e_e_Test()
        {
            if (ClassStructTestClass_S_T_Dest_e_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ClassStruct_SDer_T_Dest_i_e_Test()
        {
            if (ClassStructTestClass_SDer_T_Dest_i_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ClassStruct_SDer_T_Dest_e_e_Test()
        {
            if (ClassStructTestClass_SDer_T_Dest_e_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ClassStruct_null_to_struct_e_Test()
        {
            if (ClassStructTestClass_null_to_struct_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults ClassStruct_null_to_struct_i_Test()
        {
            if (ClassStructTestClass_null_to_struct_i.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        //Compiled Test Cases 
        class ClassStructTestClass_SBase_T_Source_i_i_SBase
        {
            static public implicit operator ClassStructTestClass_SBase_T_Source_i_i_T(ClassStructTestClass_SBase_T_Source_i_i_SBase foo)
            {
                Log.Comment(" ClassStructTestClass_SBase_T_Source_i_i_SBase to  ClassStructTestClass_SBase_T_Source_i_i_T Source implicit");
                return new ClassStructTestClass_SBase_T_Source_i_i_T();
            }
        }
        class ClassStructTestClass_SBase_T_Source_i_i_S : ClassStructTestClass_SBase_T_Source_i_i_SBase
        {
        }
        class ClassStructTestClass_SBase_T_Source_i_i_SDer : ClassStructTestClass_SBase_T_Source_i_i_S
        {
        }
        struct ClassStructTestClass_SBase_T_Source_i_i_T
        {
        }
        class ClassStructTestClass_SBase_T_Source_i_i
        {
            public static void Main_old()
            {
                ClassStructTestClass_SBase_T_Source_i_i_S s = new ClassStructTestClass_SBase_T_Source_i_i_S();
                ClassStructTestClass_SBase_T_Source_i_i_T t;
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
        class ClassStructTestClass_SBase_T_Source_i_e_SBase
        {
            static public implicit operator ClassStructTestClass_SBase_T_Source_i_e_T(ClassStructTestClass_SBase_T_Source_i_e_SBase foo)
            {
                Log.Comment(" ClassStructTestClass_SBase_T_Source_i_e_SBase to  ClassStructTestClass_SBase_T_Source_i_e_T Source implicit");
                return new ClassStructTestClass_SBase_T_Source_i_e_T();
            }
        }
        class ClassStructTestClass_SBase_T_Source_i_e_S : ClassStructTestClass_SBase_T_Source_i_e_SBase
        {
        }
        class ClassStructTestClass_SBase_T_Source_i_e_SDer : ClassStructTestClass_SBase_T_Source_i_e_S
        {
        }
        struct ClassStructTestClass_SBase_T_Source_i_e_T
        {
        }
        class ClassStructTestClass_SBase_T_Source_i_e
        {
            public static void Main_old()
            {
                ClassStructTestClass_SBase_T_Source_i_e_S s = new ClassStructTestClass_SBase_T_Source_i_e_S();
                ClassStructTestClass_SBase_T_Source_i_e_T t;
                try
                {
                    t = (ClassStructTestClass_SBase_T_Source_i_e_T)s;
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
        class ClassStructTestClass_SBase_T_Source_e_e_SBase
        {
            static public explicit operator ClassStructTestClass_SBase_T_Source_e_e_T(ClassStructTestClass_SBase_T_Source_e_e_SBase foo)
            {
                Log.Comment(" ClassStructTestClass_SBase_T_Source_e_e_SBase to  ClassStructTestClass_SBase_T_Source_e_e_T Source explicit");
                return new ClassStructTestClass_SBase_T_Source_e_e_T();
            }
        }
        class ClassStructTestClass_SBase_T_Source_e_e_S : ClassStructTestClass_SBase_T_Source_e_e_SBase
        {
        }
        class ClassStructTestClass_SBase_T_Source_e_e_SDer : ClassStructTestClass_SBase_T_Source_e_e_S
        {
        }
        struct ClassStructTestClass_SBase_T_Source_e_e_T
        {
        }
        class ClassStructTestClass_SBase_T_Source_e_e
        {
            public static void Main_old()
            {
                ClassStructTestClass_SBase_T_Source_e_e_S s = new ClassStructTestClass_SBase_T_Source_e_e_S();
                ClassStructTestClass_SBase_T_Source_e_e_T t;
                try
                {
                    t = (ClassStructTestClass_SBase_T_Source_e_e_T)s;
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
        class ClassStructTestClass_SBase_T_Dest_i_i_SBase
        {
        }
        class ClassStructTestClass_SBase_T_Dest_i_i_S : ClassStructTestClass_SBase_T_Dest_i_i_SBase
        {
        }
        class ClassStructTestClass_SBase_T_Dest_i_i_SDer : ClassStructTestClass_SBase_T_Dest_i_i_S
        {
        }
        struct ClassStructTestClass_SBase_T_Dest_i_i_T
        {
            static public implicit operator ClassStructTestClass_SBase_T_Dest_i_i_T(ClassStructTestClass_SBase_T_Dest_i_i_SBase foo)
            {
                Log.Comment(" ClassStructTestClass_SBase_T_Dest_i_i_SBase to  ClassStructTestClass_SBase_T_Dest_i_i_T Dest implicit");
                return new ClassStructTestClass_SBase_T_Dest_i_i_T();
            }
        }
        class ClassStructTestClass_SBase_T_Dest_i_i
        {
            public static void Main_old()
            {
                ClassStructTestClass_SBase_T_Dest_i_i_S s = new ClassStructTestClass_SBase_T_Dest_i_i_S();
                ClassStructTestClass_SBase_T_Dest_i_i_T t;
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
        class ClassStructTestClass_SBase_T_Dest_i_e_SBase
        {
        }
        class ClassStructTestClass_SBase_T_Dest_i_e_S : ClassStructTestClass_SBase_T_Dest_i_e_SBase
        {
        }
        class ClassStructTestClass_SBase_T_Dest_i_e_SDer : ClassStructTestClass_SBase_T_Dest_i_e_S
        {
        }
        struct ClassStructTestClass_SBase_T_Dest_i_e_T
        {
            static public implicit operator ClassStructTestClass_SBase_T_Dest_i_e_T(ClassStructTestClass_SBase_T_Dest_i_e_SBase foo)
            {
                Log.Comment(" ClassStructTestClass_SBase_T_Dest_i_e_SBase to  ClassStructTestClass_SBase_T_Dest_i_e_T Dest implicit");
                return new ClassStructTestClass_SBase_T_Dest_i_e_T();
            }
        }
        class ClassStructTestClass_SBase_T_Dest_i_e
        {
            public static void Main_old()
            {
                ClassStructTestClass_SBase_T_Dest_i_e_S s = new ClassStructTestClass_SBase_T_Dest_i_e_S();
                ClassStructTestClass_SBase_T_Dest_i_e_T t;
                try
                {
                    t = (ClassStructTestClass_SBase_T_Dest_i_e_T)s;
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
        class ClassStructTestClass_SBase_T_Dest_e_e_SBase
        {
        }
        class ClassStructTestClass_SBase_T_Dest_e_e_S : ClassStructTestClass_SBase_T_Dest_e_e_SBase
        {
        }
        class ClassStructTestClass_SBase_T_Dest_e_e_SDer : ClassStructTestClass_SBase_T_Dest_e_e_S
        {
        }
        struct ClassStructTestClass_SBase_T_Dest_e_e_T
        {
            static public explicit operator ClassStructTestClass_SBase_T_Dest_e_e_T(ClassStructTestClass_SBase_T_Dest_e_e_SBase foo)
            {
                Log.Comment(" ClassStructTestClass_SBase_T_Dest_e_e_SBase to  ClassStructTestClass_SBase_T_Dest_e_e_T Dest explicit");
                return new ClassStructTestClass_SBase_T_Dest_e_e_T();
            }
        }
        class ClassStructTestClass_SBase_T_Dest_e_e
        {
            public static void Main_old()
            {
                ClassStructTestClass_SBase_T_Dest_e_e_S s = new ClassStructTestClass_SBase_T_Dest_e_e_S();
                ClassStructTestClass_SBase_T_Dest_e_e_T t;
                try
                {
                    t = (ClassStructTestClass_SBase_T_Dest_e_e_T)s;
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
        class ClassStructTestClass_S_T_Source_i_i_SBase
        {
        }
        class ClassStructTestClass_S_T_Source_i_i_S : ClassStructTestClass_S_T_Source_i_i_SBase
        {
            static public implicit operator ClassStructTestClass_S_T_Source_i_i_T(ClassStructTestClass_S_T_Source_i_i_S foo)
            {
                Log.Comment(" ClassStructTestClass_S_T_Source_i_i_S to  ClassStructTestClass_S_T_Source_i_i_T Source implicit");
                return new ClassStructTestClass_S_T_Source_i_i_T();
            }
        }
        class ClassStructTestClass_S_T_Source_i_i_SDer : ClassStructTestClass_S_T_Source_i_i_S
        {
        }
        struct ClassStructTestClass_S_T_Source_i_i_T
        {
        }
        class ClassStructTestClass_S_T_Source_i_i
        {
            public static void Main_old()
            {
                ClassStructTestClass_S_T_Source_i_i_S s = new ClassStructTestClass_S_T_Source_i_i_S();
                ClassStructTestClass_S_T_Source_i_i_T t;
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
        class ClassStructTestClass_S_T_Source_i_e_SBase
        {
        }
        class ClassStructTestClass_S_T_Source_i_e_S : ClassStructTestClass_S_T_Source_i_e_SBase
        {
            static public implicit operator ClassStructTestClass_S_T_Source_i_e_T(ClassStructTestClass_S_T_Source_i_e_S foo)
            {
                Log.Comment(" ClassStructTestClass_S_T_Source_i_e_S to  ClassStructTestClass_S_T_Source_i_e_T Source implicit");
                return new ClassStructTestClass_S_T_Source_i_e_T();
            }
        }
        class ClassStructTestClass_S_T_Source_i_e_SDer : ClassStructTestClass_S_T_Source_i_e_S
        {
        }
        struct ClassStructTestClass_S_T_Source_i_e_T
        {
        }
        class ClassStructTestClass_S_T_Source_i_e
        {
            public static void Main_old()
            {
                ClassStructTestClass_S_T_Source_i_e_S s = new ClassStructTestClass_S_T_Source_i_e_S();
                ClassStructTestClass_S_T_Source_i_e_T t;
                try
                {
                    t = (ClassStructTestClass_S_T_Source_i_e_T)s;
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
        class ClassStructTestClass_S_T_Source_e_e_SBase
        {
        }
        class ClassStructTestClass_S_T_Source_e_e_S : ClassStructTestClass_S_T_Source_e_e_SBase
        {
            static public explicit operator ClassStructTestClass_S_T_Source_e_e_T(ClassStructTestClass_S_T_Source_e_e_S foo)
            {
                Log.Comment(" ClassStructTestClass_S_T_Source_e_e_S to  ClassStructTestClass_S_T_Source_e_e_T Source explicit");
                return new ClassStructTestClass_S_T_Source_e_e_T();
            }
        }
        class ClassStructTestClass_S_T_Source_e_e_SDer : ClassStructTestClass_S_T_Source_e_e_S
        {
        }
        struct ClassStructTestClass_S_T_Source_e_e_T
        {
        }
        class ClassStructTestClass_S_T_Source_e_e
        {
            public static void Main_old()
            {
                ClassStructTestClass_S_T_Source_e_e_S s = new ClassStructTestClass_S_T_Source_e_e_S();
                ClassStructTestClass_S_T_Source_e_e_T t;
                try
                {
                    t = (ClassStructTestClass_S_T_Source_e_e_T)s;
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
        class ClassStructTestClass_S_T_Dest_i_i_SBase
        {
        }
        class ClassStructTestClass_S_T_Dest_i_i_S : ClassStructTestClass_S_T_Dest_i_i_SBase
        {
        }
        class ClassStructTestClass_S_T_Dest_i_i_SDer : ClassStructTestClass_S_T_Dest_i_i_S
        {
        }
        struct ClassStructTestClass_S_T_Dest_i_i_T
        {
            static public implicit operator ClassStructTestClass_S_T_Dest_i_i_T(ClassStructTestClass_S_T_Dest_i_i_S foo)
            {
                Log.Comment(" ClassStructTestClass_S_T_Dest_i_i_S to  ClassStructTestClass_S_T_Dest_i_i_T Dest implicit");
                return new ClassStructTestClass_S_T_Dest_i_i_T();
            }
        }
        class ClassStructTestClass_S_T_Dest_i_i
        {
            public static void Main_old()
            {
                ClassStructTestClass_S_T_Dest_i_i_S s = new ClassStructTestClass_S_T_Dest_i_i_S();
                ClassStructTestClass_S_T_Dest_i_i_T t;
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
        class ClassStructTestClass_S_T_Dest_i_e_SBase
        {
        }
        class ClassStructTestClass_S_T_Dest_i_e_S : ClassStructTestClass_S_T_Dest_i_e_SBase
        {
        }
        class ClassStructTestClass_S_T_Dest_i_e_SDer : ClassStructTestClass_S_T_Dest_i_e_S
        {
        }
        struct ClassStructTestClass_S_T_Dest_i_e_T
        {
            static public implicit operator ClassStructTestClass_S_T_Dest_i_e_T(ClassStructTestClass_S_T_Dest_i_e_S foo)
            {
                Log.Comment(" ClassStructTestClass_S_T_Dest_i_e_S to  ClassStructTestClass_S_T_Dest_i_e_T Dest implicit");
                return new ClassStructTestClass_S_T_Dest_i_e_T();
            }
        }
        class ClassStructTestClass_S_T_Dest_i_e
        {
            public static void Main_old()
            {
                ClassStructTestClass_S_T_Dest_i_e_S s = new ClassStructTestClass_S_T_Dest_i_e_S();
                ClassStructTestClass_S_T_Dest_i_e_T t;
                try
                {
                    t = (ClassStructTestClass_S_T_Dest_i_e_T)s;
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
        class ClassStructTestClass_S_T_Dest_e_e_SBase
        {
        }
        class ClassStructTestClass_S_T_Dest_e_e_S : ClassStructTestClass_S_T_Dest_e_e_SBase
        {
        }
        class ClassStructTestClass_S_T_Dest_e_e_SDer : ClassStructTestClass_S_T_Dest_e_e_S
        {
        }
        struct ClassStructTestClass_S_T_Dest_e_e_T
        {
            static public explicit operator ClassStructTestClass_S_T_Dest_e_e_T(ClassStructTestClass_S_T_Dest_e_e_S foo)
            {
                Log.Comment(" ClassStructTestClass_S_T_Dest_e_e_S to  ClassStructTestClass_S_T_Dest_e_e_T Dest explicit");
                return new ClassStructTestClass_S_T_Dest_e_e_T();
            }
        }
        class ClassStructTestClass_S_T_Dest_e_e
        {
            public static void Main_old()
            {
                ClassStructTestClass_S_T_Dest_e_e_S s = new ClassStructTestClass_S_T_Dest_e_e_S();
                ClassStructTestClass_S_T_Dest_e_e_T t;
                try
                {
                    t = (ClassStructTestClass_S_T_Dest_e_e_T)s;
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
        class ClassStructTestClass_SDer_T_Dest_i_e_SBase
        {
        }
        class ClassStructTestClass_SDer_T_Dest_i_e_S : ClassStructTestClass_SDer_T_Dest_i_e_SBase
        {
        }
        class ClassStructTestClass_SDer_T_Dest_i_e_SDer : ClassStructTestClass_SDer_T_Dest_i_e_S
        {
        }
        struct ClassStructTestClass_SDer_T_Dest_i_e_T
        {
            static public implicit operator ClassStructTestClass_SDer_T_Dest_i_e_T(ClassStructTestClass_SDer_T_Dest_i_e_SDer foo)
            {
                Log.Comment(" ClassStructTestClass_SDer_T_Dest_i_e_SDer to  ClassStructTestClass_SDer_T_Dest_i_e_T Dest implicit");
                return new ClassStructTestClass_SDer_T_Dest_i_e_T();
            }
        }
        class ClassStructTestClass_SDer_T_Dest_i_e
        {
            public static void Main_old()
            {
                ClassStructTestClass_SDer_T_Dest_i_e_S s = new ClassStructTestClass_SDer_T_Dest_i_e_S();
                ClassStructTestClass_SDer_T_Dest_i_e_T t;
                try
                {
                    t = (ClassStructTestClass_SDer_T_Dest_i_e_T)s;
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
        class ClassStructTestClass_SDer_T_Dest_e_e_SBase
        {
        }
        class ClassStructTestClass_SDer_T_Dest_e_e_S : ClassStructTestClass_SDer_T_Dest_e_e_SBase
        {
        }
        class ClassStructTestClass_SDer_T_Dest_e_e_SDer : ClassStructTestClass_SDer_T_Dest_e_e_S
        {
        }
        struct ClassStructTestClass_SDer_T_Dest_e_e_T
        {
            static public explicit operator ClassStructTestClass_SDer_T_Dest_e_e_T(ClassStructTestClass_SDer_T_Dest_e_e_SDer foo)
            {
                Log.Comment(" ClassStructTestClass_SDer_T_Dest_e_e_SDer to  ClassStructTestClass_SDer_T_Dest_e_e_T Dest explicit");
                return new ClassStructTestClass_SDer_T_Dest_e_e_T();
            }
        }
        class ClassStructTestClass_SDer_T_Dest_e_e
        {
            public static void Main_old()
            {
                ClassStructTestClass_SDer_T_Dest_e_e_S s = new ClassStructTestClass_SDer_T_Dest_e_e_S();
                ClassStructTestClass_SDer_T_Dest_e_e_T t;
                try
                {
                    t = (ClassStructTestClass_SDer_T_Dest_e_e_T)s;
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
        public class ClassStructTestClass_null_to_struct_e_C { }
        public struct ClassStructTestClass_null_to_struct_e_S
        {
            public ClassStructTestClass_null_to_struct_e_C c;

            public ClassStructTestClass_null_to_struct_e_S(ClassStructTestClass_null_to_struct_e_C c)
            {
                this.c = c;
            }
            public static explicit operator ClassStructTestClass_null_to_struct_e_S(ClassStructTestClass_null_to_struct_e_C c)
            {
                return new ClassStructTestClass_null_to_struct_e_S(c);
            }
        }
        public class ClassStructTestClass_null_to_struct_e
        {

            public static int Main_old()
            {
                ClassStructTestClass_null_to_struct_e_S s = (ClassStructTestClass_null_to_struct_e_S)null;
                return ((s.c == null) ? 0 : 1);
            }
            public static bool testMethod()
            {
                if (Main_old() != 0)
                    return false;
                else
                    return true;
            }
        }

        public class ClassStructTestClass_null_to_struct_i_C { }
        public struct ClassStructTestClass_null_to_struct_i_S
        {
            public ClassStructTestClass_null_to_struct_i_C c;

            public ClassStructTestClass_null_to_struct_i_S(ClassStructTestClass_null_to_struct_i_C c)
            {
                this.c = c;
            }
            public static implicit operator ClassStructTestClass_null_to_struct_i_S(ClassStructTestClass_null_to_struct_i_C c)
            {
                return new ClassStructTestClass_null_to_struct_i_S(c);
            }
        }
        public class ClassStructTestClass_null_to_struct_i
        {

            public static int Main_old()
	        {
		        ClassStructTestClass_null_to_struct_i_S s = null;
		        return ((s.c == null) ? 0 : 1);
	        }
            public static bool testMethod()
            {
                if (Main_old() != 0)
                    return false;
                else
                    return true;
            }
        }


    }
}
