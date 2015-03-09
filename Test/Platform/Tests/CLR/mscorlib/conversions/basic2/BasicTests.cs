////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class BasicTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");
            Log.Comment("The Basic tests examine conversion between two types");
            Log.Comment("The tests are named for the types they convert between");
            Log.Comment("Some of them are also list with E or I for the cast being explicit or implicit");
            Log.Comment("In the case of a number of tests being run, 0, 1, maxValue and minValue are tested");

            // Add your functionality here.                

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
        }

        //Basic Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\Basic
        //byte_byte_I_0,byte_byte_E_0,byte_short_I_0,byte_short_E_0,byte_ushort_I_0,byte_ushort_E_0,byte_int_I_0,byte_int_E_0,byte_uint_I_0,byte_uint_E_0,byte_long_I_0,byte_long_E_0,byte_ulong_I_0,byte_ulong_E_0,byte_float_I_0,byte_float_E_0,byte_double_I_0,byte_double_E_0,byte_sbyte_E_1,byte_sbyte_E_2,byte_char_E_1,char_char_I_0,char_char_E_0,char_ushort_I_0,char_ushort_E_0,char_int_I_0,char_int_E_0,char_uint_I_0,char_uint_E_0,char_long_I_0,char_long_E_0,char_ulong_I_0,char_ulong_E_0,char_float_I_0,char_float_E_0,char_double_I_0,char_double_E_0,char_sbyte_E_1,char_sbyte_E_2,char_byte_E_1,char_byte_E_2,char_byte_E_3,char_short_E_1,char_short_E_2,double_double_I_0,double_double_E_0,double_byte_E_1,double_byte_E_2,double_byte_E_3,double_sbyte_E_1,double_sbyte_E_2,double_sbyte_E_3,double_char_E_1,double_char_E_2,double_char_E_3,double_short_E_1,double_short_E_2,double_short_E_3,double_ushort_E_1,double_ushort_E_2,double_ushort_E_3,double_int_E_1,double_int_E_2,double_int_E_3,double_uint_E_1,double_uint_E_2,double_uint_E_3,double_long_E_1,double_ulong_E_1,double_float_E_1,double_float_E_2,double_float_E_3,float_float_I_0,float_float_E_0,float_double_I_0,float_double_E_0,float_byte_E_1,float_byte_E_2,float_byte_E_3,float_sbyte_E_1,float_sbyte_E_2,float_sbyte_E_3,float_char_E_1,float_char_E_2,float_char_E_3,float_short_E_1,float_short_E_2,float_short_E_3,float_ushort_E_1,float_ushort_E_2,float_ushort_E_3,float_int_E_1,float_uint_E_1,float_long_E_1,float_ulong_E_1,int_int_I_0,int_int_E_0,int_long_I_0,int_long_E_0,int_float_I_0,int_float_E_0,int_double_I_0,int_double_E_0,int_sbyte_E_1,int_sbyte_E_2,int_sbyte_E_3,int_sbyte_E_4,int_byte_E_1,int_byte_E_2,int_byte_E_3,int_byte_E_4,int_short_E_1,int_short_E_2,int_short_E_3,int_short_E_4,int_ushort_E_1,int_ushort_E_2,int_ushort_E_3,int_ushort_E_4,int_uint_E_1,int_uint_E_2,int_uint_E_3,int_ulong_E_1,int_char_E_1,int_char_E_2,int_char_E_3,int_char_E_4,long_long_I_0,long_long_E_0,long_float_I_0,long_float_E_0,long_double_I_0,long_double_E_0,long_sbyte_E_1,long_sbyte_E_2,long_sbyte_E_3,long_sbyte_E_4,long_byte_E_1,long_byte_E_2,long_byte_E_3,long_byte_E_4,long_short_E_1,long_short_E_2,long_short_E_3,long_short_E_4,long_ushort_E_1,long_ushort_E_2,long_ushort_E_3,long_ushort_E_4,long_int_E_1,long_int_E_2,long_int_E_3,long_int_E_4,long_uint_E_1,long_uint_E_2,long_uint_E_3,long_uint_E_4,long_ulong_E_1,long_ulong_E_2,long_char_E_1,long_char_E_2,long_char_E_3,long_char_E_4,sbyte_sbyte_I_0,sbyte_sbyte_E_0,sbyte_short_I_0,sbyte_short_E_0,sbyte_int_I_0,sbyte_int_E_0,sbyte_long_I_0,sbyte_long_E_0,sbyte_float_I_0,sbyte_float_E_0,sbyte_double_I_0,sbyte_double_E_0,sbyte_byte_E_1,sbyte_byte_E_2,sbyte_byte_E_3,sbyte_ushort_E_1,sbyte_uint_E_1,sbyte_ulong_E_1,sbyte_char_E_1,short_short_I_0,short_short_E_0,short_int_I_0,short_int_E_0,short_long_I_0,short_long_E_0,short_float_I_0,short_float_E_0,short_double_I_0,short_double_E_0,short_short_I_1,short_short_E_1,short_int_I_1,short_int_E_1,short_long_I_1,short_long_E_1,short_float_I_1,short_float_E_1,short_double_I_1,short_double_E_1,short_decimal_I_1,short_decimal_E_1,short_sbyte_E_1,short_sbyte_E_2,short_sbyte_E_3,short_sbyte_E_4,short_byte_E_1,short_byte_E_2,short_byte_E_3,short_byte_E_4,short_ushort_E_1,short_ushort_E_2,short_ushort_E_3,short_uint_E_1,short_ulong_E_1,short_char_E_1,short_char_E_2,short_char_E_3,uint_uint_I_0,uint_uint_E_0,uint_long_I_0,uint_long_E_0,uint_ulong_I_0,uint_ulong_E_0,uint_float_I_0,uint_float_E_0,uint_double_I_0,uint_double_E_0,uint_sbyte_E_1,uint_sbyte_E_2,uint_byte_E_1,uint_byte_E_2,uint_byte_E_3,uint_short_E_1,uint_short_E_2,uint_ushort_E_1,uint_ushort_E_2,uint_ushort_E_3,uint_int_E_1,uint_int_E_2,uint_char_E_1,uint_char_E_2,uint_char_E_3,ulong_float_I_0,ulong_float_E_0,ulong_double_I_0,ulong_double_E_0,ulong_ulong_I_0,ulong_ulong_E_0,ulong_sbyte_E_1,ulong_sbyte_E_2,ulong_byte_E_1,ulong_byte_E_2,ulong_byte_E_3,ulong_short_E_1,ulong_short_E_2,ulong_ushort_E_1,ulong_ushort_E_2,ulong_ushort_E_3,ulong_int_E_1,ulong_int_E_2,ulong_uint_E_1,ulong_uint_E_2,ulong_uint_E_3,ulong_long_E_1,ulong_long_E_2,ulong_char_E_1,ulong_char_E_2,ulong_char_E_3,ushort_ushort_I_0,ushort_ushort_E_0,ushort_int_I_0,ushort_int_E_0,ushort_uint_I_0,ushort_uint_E_0,ushort_long_I_0,ushort_long_E_0,ushort_ulong_I_0,ushort_ulong_E_0,ushort_float_I_0,ushort_float_E_0,ushort_double_I_0,ushort_double_E_0,ushort_sbyte_E_1,ushort_sbyte_E_2,ushort_byte_E_1,ushort_byte_E_2,ushort_byte_E_3,ushort_short_E_1,ushort_short_E_2,ushort_char_E_1,ushort_char_E_2,ushort_char_E_3

        //Test Case Calls 

        [TestMethod]
        public MFTestResults Basic_ulong_float_I_0_Test()
        {
            if (BasicTestClass_ulong_float_I_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_float_E_0_Test()
        {
            if (BasicTestClass_ulong_float_E_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_double_I_0_Test()
        {
            if (BasicTestClass_ulong_double_I_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_double_E_0_Test()
        {
            if (BasicTestClass_ulong_double_E_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_ulong_I_0_Test()
        {
            if (BasicTestClass_ulong_ulong_I_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_ulong_E_0_Test()
        {
            if (BasicTestClass_ulong_ulong_E_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_sbyte_E_1_Test()
        {
            if (BasicTestClass_ulong_sbyte_E_1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_sbyte_E_2_Test()
        {
            if (BasicTestClass_ulong_sbyte_E_2.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_byte_E_1_Test()
        {
            if (BasicTestClass_ulong_byte_E_1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_byte_E_2_Test()
        {
            if (BasicTestClass_ulong_byte_E_2.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_byte_E_3_Test()
        {
            if (BasicTestClass_ulong_byte_E_3.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_short_E_1_Test()
        {
            if (BasicTestClass_ulong_short_E_1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_short_E_2_Test()
        {
            if (BasicTestClass_ulong_short_E_2.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_ushort_E_1_Test()
        {
            if (BasicTestClass_ulong_ushort_E_1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_ushort_E_2_Test()
        {
            if (BasicTestClass_ulong_ushort_E_2.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_ushort_E_3_Test()
        {
            if (BasicTestClass_ulong_ushort_E_3.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_int_E_1_Test()
        {
            if (BasicTestClass_ulong_int_E_1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_int_E_2_Test()
        {
            if (BasicTestClass_ulong_int_E_2.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_uint_E_1_Test()
        {
            if (BasicTestClass_ulong_uint_E_1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_uint_E_2_Test()
        {
            if (BasicTestClass_ulong_uint_E_2.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_uint_E_3_Test()
        {
            if (BasicTestClass_ulong_uint_E_3.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_long_E_1_Test()
        {
            if (BasicTestClass_ulong_long_E_1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_long_E_2_Test()
        {
            if (BasicTestClass_ulong_long_E_2.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_char_E_1_Test()
        {
            if (BasicTestClass_ulong_char_E_1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_char_E_2_Test()
        {
            if (BasicTestClass_ulong_char_E_2.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ulong_char_E_3_Test()
        {
            if (BasicTestClass_ulong_char_E_3.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        
        [TestMethod]
        public MFTestResults Basic_ushort_ushort_I_0_Test()
        {
            if (BasicTestClass_ushort_ushort_I_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ushort_ushort_E_0_Test()
        {
            if (BasicTestClass_ushort_ushort_E_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ushort_int_I_0_Test()
        {
            if (BasicTestClass_ushort_int_I_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ushort_int_E_0_Test()
        {
            if (BasicTestClass_ushort_int_E_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ushort_uint_I_0_Test()
        {
            if (BasicTestClass_ushort_uint_I_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ushort_uint_E_0_Test()
        {
            if (BasicTestClass_ushort_uint_E_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ushort_long_I_0_Test()
        {
            if (BasicTestClass_ushort_long_I_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ushort_long_E_0_Test()
        {
            if (BasicTestClass_ushort_long_E_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ushort_ulong_I_0_Test()
        {
            if (BasicTestClass_ushort_ulong_I_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ushort_ulong_E_0_Test()
        {
            if (BasicTestClass_ushort_ulong_E_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ushort_float_I_0_Test()
        {
            if (BasicTestClass_ushort_float_I_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ushort_float_E_0_Test()
        {
            if (BasicTestClass_ushort_float_E_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ushort_double_I_0_Test()
        {
            if (BasicTestClass_ushort_double_I_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ushort_double_E_0_Test()
        {
            if (BasicTestClass_ushort_double_E_0.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ushort_sbyte_E_1_Test()
        {
            if (BasicTestClass_ushort_sbyte_E_1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ushort_sbyte_E_2_Test()
        {
            if (BasicTestClass_ushort_sbyte_E_2.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ushort_byte_E_1_Test()
        {
            if (BasicTestClass_ushort_byte_E_1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ushort_byte_E_2_Test()
        {
            if (BasicTestClass_ushort_byte_E_2.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ushort_byte_E_3_Test()
        {
            if (BasicTestClass_ushort_byte_E_3.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ushort_short_E_1_Test()
        {
            if (BasicTestClass_ushort_short_E_1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ushort_short_E_2_Test()
        {
            if (BasicTestClass_ushort_short_E_2.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ushort_char_E_1_Test()
        {
            if (BasicTestClass_ushort_char_E_1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ushort_char_E_2_Test()
        {
            if (BasicTestClass_ushort_char_E_2.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Basic_ushort_char_E_3_Test()
        {
            if (BasicTestClass_ushort_char_E_3.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        
        //Compiled Test Cases 

        
        
        public class BasicTestClass_ulong_float_I_0
        {
            public static int Main_old()
            {
                ulong source;
                float dest;
                source = 1;
                dest = source;
                if (dest == 1F)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_float_E_0
        {
            public static int Main_old()
            {
                ulong source;
                float dest;
                source = 1;
                dest = (float)source;
                if (dest == 1F)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_double_I_0
        {
            public static int Main_old()
            {
                ulong source;
                double dest;
                source = 1;
                dest = source;
                if (dest == 1.0)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_double_E_0
        {
            public static int Main_old()
            {
                ulong source;
                double dest;
                source = 1;
                dest = (double)source;
                if (dest == 1.0)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_ulong_I_0
        {
            public static int Main_old()
            {
                ulong source;
                ulong dest;
                source = 1;
                dest = source;
                if (dest == 1)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_ulong_E_0
        {
            public static int Main_old()
            {
                ulong source;
                ulong dest;
                source = 1;
                dest = (ulong)source;
                if (dest == 1)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_sbyte_E_1
        {
            public static int Main_old()
            {
                ulong source;
                sbyte dest;
                source = 1;
                dest = (sbyte)source;
                if (dest == 1)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_sbyte_E_2
        {
            public static int Main_old()
            {
                ulong source;
                sbyte dest;
                source = 127;
                dest = (sbyte)source;
                if (dest == 127)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_byte_E_1
        {
            public static int Main_old()
            {
                ulong source;
                byte dest;
                source = 1;
                dest = (byte)source;
                if (dest == 1)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_byte_E_2
        {
            public static int Main_old()
            {
                ulong source;
                byte dest;
                source = 255;
                dest = (byte)source;
                if (dest == 255)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_byte_E_3
        {
            public static int Main_old()
            {
                ulong source;
                byte dest;
                source = 0;
                dest = (byte)source;
                if (dest == 0)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_short_E_1
        {
            public static int Main_old()
            {
                ulong source;
                short dest;
                source = 1;
                dest = (short)source;
                if (dest == 1)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_short_E_2
        {
            public static int Main_old()
            {
                ulong source;
                short dest;
                source = 32767;
                dest = (short)source;
                if (dest == 32767)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_ushort_E_1
        {
            public static int Main_old()
            {
                ulong source;
                ushort dest;
                source = 1;
                dest = (ushort)source;
                if (dest == 1)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_ushort_E_2
        {
            public static int Main_old()
            {
                ulong source;
                ushort dest;
                source = 65535;
                dest = (ushort)source;
                if (dest == 65535)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_ushort_E_3
        {
            public static int Main_old()
            {
                ulong source;
                ushort dest;
                source = 0;
                dest = (ushort)source;
                if (dest == 0)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_int_E_1
        {
            public static int Main_old()
            {
                ulong source;
                int dest;
                source = 1;
                dest = (int)source;
                if (dest == 1)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_int_E_2
        {
            public static int Main_old()
            {
                ulong source;
                int dest;
                source = 2147483647;
                dest = (int)source;
                if (dest == 2147483647)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_uint_E_1
        {
            public static int Main_old()
            {
                ulong source;
                uint dest;
                source = 1;
                dest = (uint)source;
                if (dest == 1)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_uint_E_2
        {
            public static int Main_old()
            {
                ulong source;
                uint dest;
                source = 4294967295;
                dest = (uint)source;
                if (dest == 4294967295)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_uint_E_3
        {
            public static int Main_old()
            {
                ulong source;
                uint dest;
                source = 0;
                dest = (uint)source;
                if (dest == 0)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_long_E_1
        {
            public static int Main_old()
            {
                ulong source;
                long dest;
                source = 1;
                dest = (long)source;
                if (dest == 1)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_long_E_2
        {
            public static int Main_old()
            {
                ulong source;
                long dest;
                source = 9223372036854775807;
                dest = (long)source;
                if (dest == 9223372036854775807)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_char_E_1
        {
            public static int Main_old()
            {
                ulong source;
                char dest;
                source = 1;
                dest = (char)source;
                if (dest == '\x1')
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_char_E_2
        {
            public static int Main_old()
            {
                ulong source;
                char dest;
                source = 65535;
                dest = (char)source;
                if (dest == '\xffff')
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ulong_char_E_3
        {
            public static int Main_old()
            {
                ulong source;
                char dest;
                source = 0;
                dest = (char)source;
                if (dest == '\x0')
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        
        public class BasicTestClass_ushort_ushort_I_0
        {
            public static int Main_old()
            {
                ushort source;
                ushort dest;
                source = 1;
                dest = source;
                if (dest == 1)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ushort_ushort_E_0
        {
            public static int Main_old()
            {
                ushort source;
                ushort dest;
                source = 1;
                dest = (ushort)source;
                if (dest == 1)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ushort_int_I_0
        {
            public static int Main_old()
            {
                ushort source;
                int dest;
                source = 1;
                dest = source;
                if (dest == 1)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ushort_int_E_0
        {
            public static int Main_old()
            {
                ushort source;
                int dest;
                source = 1;
                dest = (int)source;
                if (dest == 1)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ushort_uint_I_0
        {
            public static int Main_old()
            {
                ushort source;
                uint dest;
                source = 1;
                dest = source;
                if (dest == 1)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ushort_uint_E_0
        {
            public static int Main_old()
            {
                ushort source;
                uint dest;
                source = 1;
                dest = (uint)source;
                if (dest == 1)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ushort_long_I_0
        {
            public static int Main_old()
            {
                ushort source;
                long dest;
                source = 1;
                dest = source;
                if (dest == 1)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ushort_long_E_0
        {
            public static int Main_old()
            {
                ushort source;
                long dest;
                source = 1;
                dest = (long)source;
                if (dest == 1)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ushort_ulong_I_0
        {
            public static int Main_old()
            {
                ushort source;
                ulong dest;
                source = 1;
                dest = source;
                if (dest == 1)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ushort_ulong_E_0
        {
            public static int Main_old()
            {
                ushort source;
                ulong dest;
                source = 1;
                dest = (ulong)source;
                if (dest == 1)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ushort_float_I_0
        {
            public static int Main_old()
            {
                ushort source;
                float dest;
                source = 1;
                dest = source;
                if (dest == 1F)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ushort_float_E_0
        {
            public static int Main_old()
            {
                ushort source;
                float dest;
                source = 1;
                dest = (float)source;
                if (dest == 1F)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ushort_double_I_0
        {
            public static int Main_old()
            {
                ushort source;
                double dest;
                source = 1;
                dest = source;
                if (dest == 1.0)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ushort_double_E_0
        {
            public static int Main_old()
            {
                ushort source;
                double dest;
                source = 1;
                dest = (double)source;
                if (dest == 1.0)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ushort_sbyte_E_1
        {
            public static int Main_old()
            {
                ushort source;
                sbyte dest;
                source = 1;
                dest = (sbyte)source;
                if (dest == 1)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ushort_sbyte_E_2
        {
            public static int Main_old()
            {
                ushort source;
                sbyte dest;
                source = 127;
                dest = (sbyte)source;
                if (dest == 127)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ushort_byte_E_1
        {
            public static int Main_old()
            {
                ushort source;
                byte dest;
                source = 1;
                dest = (byte)source;
                if (dest == 1)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ushort_byte_E_2
        {
            public static int Main_old()
            {
                ushort source;
                byte dest;
                source = 255;
                dest = (byte)source;
                if (dest == 255)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ushort_byte_E_3
        {
            public static int Main_old()
            {
                ushort source;
                byte dest;
                source = 0;
                dest = (byte)source;
                if (dest == 0)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ushort_short_E_1
        {
            public static int Main_old()
            {
                ushort source;
                short dest;
                source = 1;
                dest = (short)source;
                if (dest == 1)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ushort_short_E_2
        {
            public static int Main_old()
            {
                ushort source;
                short dest;
                source = 32767;
                dest = (short)source;
                if (dest == 32767)
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ushort_char_E_1
        {
            public static int Main_old()
            {
                ushort source;
                char dest;
                source = 1;
                dest = (char)source;
                if (dest == '\x1')
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ushort_char_E_2
        {
            public static int Main_old()
            {
                ushort source;
                char dest;
                source = 65535;
                dest = (char)source;
                if (dest == '\xffff')
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class BasicTestClass_ushort_char_E_3
        {
            public static int Main_old()
            {
                ushort source;
                char dest;
                source = 0;
                dest = (char)source;
                if (dest == '\x0')
                    return 0;
                else
                    return 1;

            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        
    
    }
}
