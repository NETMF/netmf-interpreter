////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class StructStructTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");
            Log.Comment("These tests examine the conversion (casting) of two structs");
            Log.Comment("There are two structs S and T");
            Log.Comment("The names of the tests describe the tests by listing which two objects will be converted between");
            Log.Comment("Followed by which (The source or the destination) will contain a cast definition");
            Log.Comment("Followed further by 'i's or 'e's to indicate which of the cast definition and the actual cast are");
            Log.Comment("implicit or explicit.");
            Log.Comment("");
            Log.Comment("For example, S_T_Source_i_e tests the conversion of S to T, with an implicit definition");
            Log.Comment("of the cast in the S struct, and an explicit cast in the body of the method.");   

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
        }

        //StructStruct Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\StructStruct

        //Test Case Calls 
        [TestMethod]
        public MFTestResults StructStruct_S_T_Source_i_i_Test()
        {
            if (StructStructTestClass_S_T_Source_i_i.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults StructStruct_S_T_Source_i_e_Test()
        {
            if (StructStructTestClass_S_T_Source_i_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults StructStruct_S_T_Source_e_e_Test()
        {
            if (StructStructTestClass_S_T_Source_e_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults StructStruct_S_T_Dest_i_i_Test()
        {
            if (StructStructTestClass_S_T_Dest_i_i.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults StructStruct_S_T_Dest_i_e_Test()
        {
            if (StructStructTestClass_S_T_Dest_i_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults StructStruct_S_T_Dest_e_e_Test()
        {
            if (StructStructTestClass_S_T_Dest_e_e.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }


        //Compiled Test Cases 
        struct StructStructTestClass_S_T_Source_i_i_S
        {
            static public implicit operator StructStructTestClass_S_T_Source_i_i_T(StructStructTestClass_S_T_Source_i_i_S foo)
            {
                Log.Comment(" StructStructTestClass_S_T_Source_i_i_S to  StructStructTestClass_S_T_Source_i_i_T Source implicit");
                return new StructStructTestClass_S_T_Source_i_i_T();
            }
        }
        struct StructStructTestClass_S_T_Source_i_i_T
        {
        }
        class StructStructTestClass_S_T_Source_i_i
        {
            public static void Main_old()
            {
                StructStructTestClass_S_T_Source_i_i_S s = new StructStructTestClass_S_T_Source_i_i_S();
                StructStructTestClass_S_T_Source_i_i_T t;
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
        struct StructStructTestClass_S_T_Source_i_e_S
        {
            static public implicit operator StructStructTestClass_S_T_Source_i_e_T(StructStructTestClass_S_T_Source_i_e_S foo)
            {
                Log.Comment(" StructStructTestClass_S_T_Source_i_e_S to  StructStructTestClass_S_T_Source_i_e_T Source implicit");
                return new StructStructTestClass_S_T_Source_i_e_T();
            }
        }
        struct StructStructTestClass_S_T_Source_i_e_T
        {
        }
        class StructStructTestClass_S_T_Source_i_e
        {
            public static void Main_old()
            {
                StructStructTestClass_S_T_Source_i_e_S s = new StructStructTestClass_S_T_Source_i_e_S();
                StructStructTestClass_S_T_Source_i_e_T t;
                try
                {
                    t = (StructStructTestClass_S_T_Source_i_e_T)s;
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
        struct StructStructTestClass_S_T_Source_e_e_S
        {
            static public explicit operator StructStructTestClass_S_T_Source_e_e_T(StructStructTestClass_S_T_Source_e_e_S foo)
            {
                Log.Comment(" StructStructTestClass_S_T_Source_e_e_S to  StructStructTestClass_S_T_Source_e_e_T Source explicit");
                return new StructStructTestClass_S_T_Source_e_e_T();
            }
        }
        struct StructStructTestClass_S_T_Source_e_e_T
        {
        }
        class StructStructTestClass_S_T_Source_e_e
        {
            public static void Main_old()
            {
                StructStructTestClass_S_T_Source_e_e_S s = new StructStructTestClass_S_T_Source_e_e_S();
                StructStructTestClass_S_T_Source_e_e_T t;
                try
                {
                    t = (StructStructTestClass_S_T_Source_e_e_T)s;
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
        struct StructStructTestClass_S_T_Dest_i_i_S
        {
        }
        struct StructStructTestClass_S_T_Dest_i_i_T
        {
            static public implicit operator StructStructTestClass_S_T_Dest_i_i_T(StructStructTestClass_S_T_Dest_i_i_S foo)
            {
                Log.Comment(" StructStructTestClass_S_T_Dest_i_i_S to  StructStructTestClass_S_T_Dest_i_i_T Dest implicit");
                return new StructStructTestClass_S_T_Dest_i_i_T();
            }
        }
        class StructStructTestClass_S_T_Dest_i_i
        {
            public static void Main_old()
            {
                StructStructTestClass_S_T_Dest_i_i_S s = new StructStructTestClass_S_T_Dest_i_i_S();
                StructStructTestClass_S_T_Dest_i_i_T t;
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
        struct StructStructTestClass_S_T_Dest_i_e_S
        {
        }
        struct StructStructTestClass_S_T_Dest_i_e_T
        {
            static public implicit operator StructStructTestClass_S_T_Dest_i_e_T(StructStructTestClass_S_T_Dest_i_e_S foo)
            {
                Log.Comment(" StructStructTestClass_S_T_Dest_i_e_S to  StructStructTestClass_S_T_Dest_i_e_T Dest implicit");
                return new StructStructTestClass_S_T_Dest_i_e_T();
            }
        }
        class StructStructTestClass_S_T_Dest_i_e
        {
            public static void Main_old()
            {
                StructStructTestClass_S_T_Dest_i_e_S s = new StructStructTestClass_S_T_Dest_i_e_S();
                StructStructTestClass_S_T_Dest_i_e_T t;
                try
                {
                    t = (StructStructTestClass_S_T_Dest_i_e_T)s;
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
        struct StructStructTestClass_S_T_Dest_e_e_S
        {
        }
        struct StructStructTestClass_S_T_Dest_e_e_T
        {
            static public explicit operator StructStructTestClass_S_T_Dest_e_e_T(StructStructTestClass_S_T_Dest_e_e_S foo)
            {
                Log.Comment(" StructStructTestClass_S_T_Dest_e_e_S to  StructStructTestClass_S_T_Dest_e_e_T Dest explicit");
                return new StructStructTestClass_S_T_Dest_e_e_T();
            }
        }
        class StructStructTestClass_S_T_Dest_e_e
        {
            public static void Main_old()
            {
                StructStructTestClass_S_T_Dest_e_e_S s = new StructStructTestClass_S_T_Dest_e_e_S();
                StructStructTestClass_S_T_Dest_e_e_T t;
                try
                {
                    t = (StructStructTestClass_S_T_Dest_e_e_T)s;
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
