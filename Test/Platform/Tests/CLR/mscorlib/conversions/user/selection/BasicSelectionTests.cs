////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class BasicSelectionTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");
            Log.Comment("The Basic Selection tests examine casting a basic type as a user class");
            Log.Comment("And then extracting the value.");
            Log.Comment("They are named for the type they convert from, along with Is and Es");
            Log.Comment("For whether each cast is Implicit or Explicit, and with FC or TC");
            Log.Comment("Which indicates whether the Conversion From, or the Conversion back To");
            Log.Comment("the listed type is performed with a function call rather than a simple assignment");

            // Add your functionality here.                

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
        }

        //BasicSelection Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\BasicSelection
        //byte_i_i_tc,byte_i_e_tc,byte_e_e_tc,byte_i_i_fc,byte_i_e_fc,byte_e_e_fc,char_i_i_tc,char_i_e_tc,char_e_e_tc,char_i_i_fc,char_i_e_fc,char_e_e_fc,double_i_i_tc,double_i_e_tc,double_e_e_tc,double_i_i_fc,double_i_e_fc,double_e_e_fc,float_i_i_tc,float_i_e_tc,float_e_e_tc,float_i_i_fc,float_i_e_fc,float_e_e_fc,int_i_i_tc,int_i_e_tc,int_e_e_tc,int_i_i_fc,int_i_e_fc,int_e_e_fc,long_i_i_tc,long_i_e_tc,long_e_e_tc,long_i_i_fc,long_i_e_fc,long_e_e_fc,sbyte_i_i_tc,sbyte_i_e_tc,sbyte_e_e_tc,sbyte_i_i_fc,sbyte_i_e_fc,sbyte_e_e_fc,short_i_i_tc,short_i_e_tc,short_e_e_tc,short_i_i_fc,short_i_e_fc,short_e_e_fc,uint_i_i_tc,uint_i_e_tc,uint_e_e_tc,uint_i_i_fc,uint_i_e_fc,uint_e_e_fc,ulong_i_i_tc,ulong_i_e_tc,ulong_e_e_tc,ulong_i_i_fc,ulong_i_e_fc,ulong_e_e_fc,ushort_i_i_tc,ushort_i_e_tc,ushort_e_e_tc,ushort_i_i_fc,ushort_i_e_fc,ushort_e_e_fc

        //Test Case Calls 
        [TestMethod]
        public MFTestResults BasicSelection_byte_i_i_tc_Test()
        {
            if (BasicSelectionTestClass_byte_i_i_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        
        [TestMethod]
        public MFTestResults BasicSelection_byte_i_e_tc_Test()
        {
            if (BasicSelectionTestClass_byte_i_e_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_byte_e_e_tc_Test()
        {
            if (BasicSelectionTestClass_byte_e_e_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        
        [TestMethod]
        public MFTestResults BasicSelection_byte_i_i_fc_Test()
        {
            if (BasicSelectionTestClass_byte_i_i_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_byte_i_e_fc_Test()
        {
            if (BasicSelectionTestClass_byte_i_e_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        
        [TestMethod]
        public MFTestResults BasicSelection_byte_e_e_fc_Test()
        {
            if (BasicSelectionTestClass_byte_e_e_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_char_i_i_tc_Test()
        {
            if (BasicSelectionTestClass_char_i_i_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_char_i_e_tc_Test()
        {
            if (BasicSelectionTestClass_char_i_e_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_char_e_e_tc_Test()
        {
            if (BasicSelectionTestClass_char_e_e_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        
        [TestMethod]
        public MFTestResults BasicSelection_char_i_i_fc_Test()
        {
            if (BasicSelectionTestClass_char_i_i_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_char_i_e_fc_Test()
        {
            if (BasicSelectionTestClass_char_i_e_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_char_e_e_fc_Test()
        {
            if (BasicSelectionTestClass_char_e_e_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_double_i_i_tc_Test()
        {
            if (BasicSelectionTestClass_double_i_i_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        
        [TestMethod]
        public MFTestResults BasicSelection_double_i_e_tc_Test()
        {
            if (BasicSelectionTestClass_double_i_e_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_double_e_e_tc_Test()
        {
            if (BasicSelectionTestClass_double_e_e_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_double_i_i_fc_Test()
        {
            if (BasicSelectionTestClass_double_i_i_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_double_i_e_fc_Test()
        {
            if (BasicSelectionTestClass_double_i_e_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        
        [TestMethod]
        public MFTestResults BasicSelection_double_e_e_fc_Test()
        {
            if (BasicSelectionTestClass_double_e_e_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_float_i_i_tc_Test()
        {
            if (BasicSelectionTestClass_float_i_i_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_float_i_e_tc_Test()
        {
            if (BasicSelectionTestClass_float_i_e_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_float_e_e_tc_Test()
        {
            if (BasicSelectionTestClass_float_e_e_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        
        [TestMethod]
        public MFTestResults BasicSelection_float_i_i_fc_Test()
        {
            if (BasicSelectionTestClass_float_i_i_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_float_i_e_fc_Test()
        {
            if (BasicSelectionTestClass_float_i_e_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_float_e_e_fc_Test()
        {
            if (BasicSelectionTestClass_float_e_e_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_int_i_i_tc_Test()
        {
            if (BasicSelectionTestClass_int_i_i_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        
        
        [TestMethod]
        public MFTestResults BasicSelection_long_i_i_fc_Test()
        {
            if (BasicSelectionTestClass_long_i_i_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_long_i_e_fc_Test()
        {
            if (BasicSelectionTestClass_long_i_e_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_long_e_e_fc_Test()
        {
            if (BasicSelectionTestClass_long_e_e_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_sbyte_i_i_tc_Test()
        {
            if (BasicSelectionTestClass_sbyte_i_i_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }



        [TestMethod]
        public MFTestResults BasicSelection_int_i_e_tc_Test()
        {
            if (BasicSelectionTestClass_int_i_e_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_int_e_e_tc_Test()
        {
            if (BasicSelectionTestClass_int_e_e_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_int_i_i_fc_Test()
        {
            if (BasicSelectionTestClass_int_i_i_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_int_i_e_fc_Test()
        {
            if (BasicSelectionTestClass_int_i_e_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults BasicSelection_int_e_e_fc_Test()
        {
            if (BasicSelectionTestClass_int_e_e_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_long_i_i_tc_Test()
        {
            if (BasicSelectionTestClass_long_i_i_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_long_i_e_tc_Test()
        {
            if (BasicSelectionTestClass_long_i_e_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults BasicSelection_long_e_e_tc_Test()
        {
            if (BasicSelectionTestClass_long_e_e_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults BasicSelection_sbyte_i_e_tc_Test()
        {
            if (BasicSelectionTestClass_sbyte_i_e_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_sbyte_e_e_tc_Test()
        {
            if (BasicSelectionTestClass_sbyte_e_e_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_sbyte_i_i_fc_Test()
        {
            if (BasicSelectionTestClass_sbyte_i_i_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_sbyte_i_e_fc_Test()
        {
            if (BasicSelectionTestClass_sbyte_i_e_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_sbyte_e_e_fc_Test()
        {
            if (BasicSelectionTestClass_sbyte_e_e_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_short_i_i_tc_Test()
        {
            if (BasicSelectionTestClass_short_i_i_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_short_i_e_tc_Test()
        {
            if (BasicSelectionTestClass_short_i_e_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_short_e_e_tc_Test()
        {
            if (BasicSelectionTestClass_short_e_e_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_short_i_i_fc_Test()
        {
            if (BasicSelectionTestClass_short_i_i_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_short_i_e_fc_Test()
        {
            if (BasicSelectionTestClass_short_i_e_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_short_e_e_fc_Test()
        {
            if (BasicSelectionTestClass_short_e_e_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_uint_i_i_tc_Test()
        {
            if (BasicSelectionTestClass_uint_i_i_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_uint_i_e_tc_Test()
        {
            if (BasicSelectionTestClass_uint_i_e_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_uint_e_e_tc_Test()
        {
            if (BasicSelectionTestClass_uint_e_e_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_uint_i_i_fc_Test()
        {
            if (BasicSelectionTestClass_uint_i_i_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_uint_i_e_fc_Test()
        {
            if (BasicSelectionTestClass_uint_i_e_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_uint_e_e_fc_Test()
        {
            if (BasicSelectionTestClass_uint_e_e_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_ulong_i_i_tc_Test()
        {
            if (BasicSelectionTestClass_ulong_i_i_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_ulong_i_e_tc_Test()
        {
            if (BasicSelectionTestClass_ulong_i_e_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_ulong_e_e_tc_Test()
        {
            if (BasicSelectionTestClass_ulong_e_e_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_ulong_i_i_fc_Test()
        {
            if (BasicSelectionTestClass_ulong_i_i_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_ulong_i_e_fc_Test()
        {
            if (BasicSelectionTestClass_ulong_i_e_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_ulong_e_e_fc_Test()
        {
            if (BasicSelectionTestClass_ulong_e_e_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_ushort_i_i_tc_Test()
        {
            if (BasicSelectionTestClass_ushort_i_i_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_ushort_i_e_tc_Test()
        {
            if (BasicSelectionTestClass_ushort_i_e_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_ushort_e_e_tc_Test()
        {
            if (BasicSelectionTestClass_ushort_e_e_tc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_ushort_i_i_fc_Test()
        {
            if (BasicSelectionTestClass_ushort_i_i_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_ushort_i_e_fc_Test()
        {
            if (BasicSelectionTestClass_ushort_i_e_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BasicSelection_ushort_e_e_fc_Test()
        {
            if (BasicSelectionTestClass_ushort_e_e_fc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        

        //Compiled Test Cases 
        class BasicSelectionTestClass_byte_i_i_tc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_byte_i_i_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_byte_i_i_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_byte_i_i_tc_Conv temp = new BasicSelectionTestClass_byte_i_i_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_byte_i_i_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_byte_i_i_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_byte_i_i_tc_Conv temp = new BasicSelectionTestClass_byte_i_i_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_byte_i_i_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_byte_i_i_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_byte_i_i_tc_Conv temp = new BasicSelectionTestClass_byte_i_i_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_byte_i_i_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_byte_i_i_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_byte_i_i_tc_Conv temp = new BasicSelectionTestClass_byte_i_i_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_byte_i_i_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_byte_i_i_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_byte_i_i_tc_Conv temp = new BasicSelectionTestClass_byte_i_i_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_byte_i_i_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_byte_i_i_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_byte_i_i_tc_Conv temp = new BasicSelectionTestClass_byte_i_i_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_byte_i_i_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_byte_i_i_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_byte_i_i_tc_Conv temp = new BasicSelectionTestClass_byte_i_i_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_byte_i_i_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_byte_i_i_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_byte_i_i_tc_Conv temp = new BasicSelectionTestClass_byte_i_i_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_byte_i_i_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_byte_i_i_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_byte_i_i_tc_Conv temp = new BasicSelectionTestClass_byte_i_i_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_byte_i_i_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_byte_i_i_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_byte_i_i_tc_Conv temp = new BasicSelectionTestClass_byte_i_i_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_byte_i_i_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_byte_i_i_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_byte_i_i_tc_Conv temp = new BasicSelectionTestClass_byte_i_i_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_byte_i_i_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_byte_i_i_tc_Conv cl = new BasicSelectionTestClass_byte_i_i_tc_Conv();
                byte val = 1;
                cl = val;
                if (cl.GetValuebyte() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        
        class BasicSelectionTestClass_byte_i_e_tc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_byte_i_e_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_byte_i_e_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_byte_i_e_tc_Conv temp = new BasicSelectionTestClass_byte_i_e_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_byte_i_e_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_byte_i_e_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_byte_i_e_tc_Conv temp = new BasicSelectionTestClass_byte_i_e_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_byte_i_e_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_byte_i_e_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_byte_i_e_tc_Conv temp = new BasicSelectionTestClass_byte_i_e_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_byte_i_e_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_byte_i_e_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_byte_i_e_tc_Conv temp = new BasicSelectionTestClass_byte_i_e_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_byte_i_e_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_byte_i_e_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_byte_i_e_tc_Conv temp = new BasicSelectionTestClass_byte_i_e_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_byte_i_e_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_byte_i_e_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_byte_i_e_tc_Conv temp = new BasicSelectionTestClass_byte_i_e_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_byte_i_e_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_byte_i_e_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_byte_i_e_tc_Conv temp = new BasicSelectionTestClass_byte_i_e_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_byte_i_e_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_byte_i_e_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_byte_i_e_tc_Conv temp = new BasicSelectionTestClass_byte_i_e_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_byte_i_e_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_byte_i_e_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_byte_i_e_tc_Conv temp = new BasicSelectionTestClass_byte_i_e_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_byte_i_e_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_byte_i_e_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_byte_i_e_tc_Conv temp = new BasicSelectionTestClass_byte_i_e_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_byte_i_e_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_byte_i_e_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_byte_i_e_tc_Conv temp = new BasicSelectionTestClass_byte_i_e_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_byte_i_e_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_byte_i_e_tc_Conv cl = new BasicSelectionTestClass_byte_i_e_tc_Conv();
                byte val = 1;
                cl = (BasicSelectionTestClass_byte_i_e_tc_Conv)val;
                if (cl.GetValuebyte() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_byte_e_e_tc_Conv
        {
            private byte value_byte;
            static public explicit operator byte(BasicSelectionTestClass_byte_e_e_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public explicit operator BasicSelectionTestClass_byte_e_e_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_byte_e_e_tc_Conv temp = new BasicSelectionTestClass_byte_e_e_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public explicit operator char(BasicSelectionTestClass_byte_e_e_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public explicit operator BasicSelectionTestClass_byte_e_e_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_byte_e_e_tc_Conv temp = new BasicSelectionTestClass_byte_e_e_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public explicit operator double(BasicSelectionTestClass_byte_e_e_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public explicit operator BasicSelectionTestClass_byte_e_e_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_byte_e_e_tc_Conv temp = new BasicSelectionTestClass_byte_e_e_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public explicit operator float(BasicSelectionTestClass_byte_e_e_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public explicit operator BasicSelectionTestClass_byte_e_e_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_byte_e_e_tc_Conv temp = new BasicSelectionTestClass_byte_e_e_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public explicit operator int(BasicSelectionTestClass_byte_e_e_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public explicit operator BasicSelectionTestClass_byte_e_e_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_byte_e_e_tc_Conv temp = new BasicSelectionTestClass_byte_e_e_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public explicit operator long(BasicSelectionTestClass_byte_e_e_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public explicit operator BasicSelectionTestClass_byte_e_e_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_byte_e_e_tc_Conv temp = new BasicSelectionTestClass_byte_e_e_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public explicit operator sbyte(BasicSelectionTestClass_byte_e_e_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public explicit operator BasicSelectionTestClass_byte_e_e_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_byte_e_e_tc_Conv temp = new BasicSelectionTestClass_byte_e_e_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public explicit operator short(BasicSelectionTestClass_byte_e_e_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public explicit operator BasicSelectionTestClass_byte_e_e_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_byte_e_e_tc_Conv temp = new BasicSelectionTestClass_byte_e_e_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public explicit operator uint(BasicSelectionTestClass_byte_e_e_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public explicit operator BasicSelectionTestClass_byte_e_e_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_byte_e_e_tc_Conv temp = new BasicSelectionTestClass_byte_e_e_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public explicit operator ulong(BasicSelectionTestClass_byte_e_e_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public explicit operator BasicSelectionTestClass_byte_e_e_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_byte_e_e_tc_Conv temp = new BasicSelectionTestClass_byte_e_e_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public explicit operator ushort(BasicSelectionTestClass_byte_e_e_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public explicit operator BasicSelectionTestClass_byte_e_e_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_byte_e_e_tc_Conv temp = new BasicSelectionTestClass_byte_e_e_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        
        class BasicSelectionTestClass_byte_e_e_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_byte_e_e_tc_Conv cl = new BasicSelectionTestClass_byte_e_e_tc_Conv();
                byte val = 1;
                cl = (BasicSelectionTestClass_byte_e_e_tc_Conv)val;
                if (cl.GetValuebyte() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        
        class BasicSelectionTestClass_byte_i_i_fc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_byte_i_i_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_byte_i_i_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_byte_i_i_fc_Conv temp = new BasicSelectionTestClass_byte_i_i_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_byte_i_i_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_byte_i_i_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_byte_i_i_fc_Conv temp = new BasicSelectionTestClass_byte_i_i_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_byte_i_i_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_byte_i_i_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_byte_i_i_fc_Conv temp = new BasicSelectionTestClass_byte_i_i_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_byte_i_i_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_byte_i_i_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_byte_i_i_fc_Conv temp = new BasicSelectionTestClass_byte_i_i_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_byte_i_i_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_byte_i_i_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_byte_i_i_fc_Conv temp = new BasicSelectionTestClass_byte_i_i_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_byte_i_i_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_byte_i_i_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_byte_i_i_fc_Conv temp = new BasicSelectionTestClass_byte_i_i_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_byte_i_i_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_byte_i_i_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_byte_i_i_fc_Conv temp = new BasicSelectionTestClass_byte_i_i_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_byte_i_i_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_byte_i_i_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_byte_i_i_fc_Conv temp = new BasicSelectionTestClass_byte_i_i_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_byte_i_i_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_byte_i_i_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_byte_i_i_fc_Conv temp = new BasicSelectionTestClass_byte_i_i_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_byte_i_i_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_byte_i_i_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_byte_i_i_fc_Conv temp = new BasicSelectionTestClass_byte_i_i_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_byte_i_i_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_byte_i_i_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_byte_i_i_fc_Conv temp = new BasicSelectionTestClass_byte_i_i_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_byte_i_i_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_byte_i_i_fc_Conv cl = new BasicSelectionTestClass_byte_i_i_fc_Conv();
                cl.SetValuebyte(1);
                byte value = cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_byte_i_e_fc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_byte_i_e_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_byte_i_e_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_byte_i_e_fc_Conv temp = new BasicSelectionTestClass_byte_i_e_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_byte_i_e_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_byte_i_e_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_byte_i_e_fc_Conv temp = new BasicSelectionTestClass_byte_i_e_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_byte_i_e_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_byte_i_e_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_byte_i_e_fc_Conv temp = new BasicSelectionTestClass_byte_i_e_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_byte_i_e_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_byte_i_e_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_byte_i_e_fc_Conv temp = new BasicSelectionTestClass_byte_i_e_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_byte_i_e_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_byte_i_e_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_byte_i_e_fc_Conv temp = new BasicSelectionTestClass_byte_i_e_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_byte_i_e_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_byte_i_e_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_byte_i_e_fc_Conv temp = new BasicSelectionTestClass_byte_i_e_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_byte_i_e_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_byte_i_e_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_byte_i_e_fc_Conv temp = new BasicSelectionTestClass_byte_i_e_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_byte_i_e_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_byte_i_e_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_byte_i_e_fc_Conv temp = new BasicSelectionTestClass_byte_i_e_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_byte_i_e_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_byte_i_e_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_byte_i_e_fc_Conv temp = new BasicSelectionTestClass_byte_i_e_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_byte_i_e_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_byte_i_e_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_byte_i_e_fc_Conv temp = new BasicSelectionTestClass_byte_i_e_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_byte_i_e_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_byte_i_e_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_byte_i_e_fc_Conv temp = new BasicSelectionTestClass_byte_i_e_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_byte_i_e_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_byte_i_e_fc_Conv cl = new BasicSelectionTestClass_byte_i_e_fc_Conv();
                cl.SetValuebyte(1);
                byte value = (byte)cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
         
        class BasicSelectionTestClass_byte_e_e_fc_Conv
        {
            private byte value_byte;
            static public explicit operator byte(BasicSelectionTestClass_byte_e_e_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public explicit operator BasicSelectionTestClass_byte_e_e_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_byte_e_e_fc_Conv temp = new BasicSelectionTestClass_byte_e_e_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public explicit operator char(BasicSelectionTestClass_byte_e_e_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public explicit operator BasicSelectionTestClass_byte_e_e_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_byte_e_e_fc_Conv temp = new BasicSelectionTestClass_byte_e_e_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public explicit operator double(BasicSelectionTestClass_byte_e_e_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public explicit operator BasicSelectionTestClass_byte_e_e_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_byte_e_e_fc_Conv temp = new BasicSelectionTestClass_byte_e_e_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public explicit operator float(BasicSelectionTestClass_byte_e_e_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public explicit operator BasicSelectionTestClass_byte_e_e_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_byte_e_e_fc_Conv temp = new BasicSelectionTestClass_byte_e_e_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public explicit operator int(BasicSelectionTestClass_byte_e_e_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public explicit operator BasicSelectionTestClass_byte_e_e_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_byte_e_e_fc_Conv temp = new BasicSelectionTestClass_byte_e_e_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public explicit operator long(BasicSelectionTestClass_byte_e_e_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public explicit operator BasicSelectionTestClass_byte_e_e_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_byte_e_e_fc_Conv temp = new BasicSelectionTestClass_byte_e_e_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public explicit operator sbyte(BasicSelectionTestClass_byte_e_e_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public explicit operator BasicSelectionTestClass_byte_e_e_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_byte_e_e_fc_Conv temp = new BasicSelectionTestClass_byte_e_e_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public explicit operator short(BasicSelectionTestClass_byte_e_e_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public explicit operator BasicSelectionTestClass_byte_e_e_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_byte_e_e_fc_Conv temp = new BasicSelectionTestClass_byte_e_e_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public explicit operator uint(BasicSelectionTestClass_byte_e_e_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public explicit operator BasicSelectionTestClass_byte_e_e_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_byte_e_e_fc_Conv temp = new BasicSelectionTestClass_byte_e_e_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public explicit operator ulong(BasicSelectionTestClass_byte_e_e_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public explicit operator BasicSelectionTestClass_byte_e_e_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_byte_e_e_fc_Conv temp = new BasicSelectionTestClass_byte_e_e_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public explicit operator ushort(BasicSelectionTestClass_byte_e_e_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public explicit operator BasicSelectionTestClass_byte_e_e_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_byte_e_e_fc_Conv temp = new BasicSelectionTestClass_byte_e_e_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_byte_e_e_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_byte_e_e_fc_Conv cl = new BasicSelectionTestClass_byte_e_e_fc_Conv();
                cl.SetValuebyte(1);
                byte value = (byte)cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_char_i_i_tc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_char_i_i_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_char_i_i_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_char_i_i_tc_Conv temp = new BasicSelectionTestClass_char_i_i_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_char_i_i_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_char_i_i_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_char_i_i_tc_Conv temp = new BasicSelectionTestClass_char_i_i_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_char_i_i_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_char_i_i_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_char_i_i_tc_Conv temp = new BasicSelectionTestClass_char_i_i_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_char_i_i_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_char_i_i_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_char_i_i_tc_Conv temp = new BasicSelectionTestClass_char_i_i_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_char_i_i_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_char_i_i_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_char_i_i_tc_Conv temp = new BasicSelectionTestClass_char_i_i_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_char_i_i_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_char_i_i_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_char_i_i_tc_Conv temp = new BasicSelectionTestClass_char_i_i_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_char_i_i_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_char_i_i_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_char_i_i_tc_Conv temp = new BasicSelectionTestClass_char_i_i_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_char_i_i_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_char_i_i_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_char_i_i_tc_Conv temp = new BasicSelectionTestClass_char_i_i_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_char_i_i_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_char_i_i_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_char_i_i_tc_Conv temp = new BasicSelectionTestClass_char_i_i_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_char_i_i_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_char_i_i_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_char_i_i_tc_Conv temp = new BasicSelectionTestClass_char_i_i_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_char_i_i_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_char_i_i_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_char_i_i_tc_Conv temp = new BasicSelectionTestClass_char_i_i_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_char_i_i_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_char_i_i_tc_Conv cl = new BasicSelectionTestClass_char_i_i_tc_Conv();
                char val = '\x1';
                cl = val;
                if (cl.GetValuechar() != '\x1')
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_char_i_e_tc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_char_i_e_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_char_i_e_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_char_i_e_tc_Conv temp = new BasicSelectionTestClass_char_i_e_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_char_i_e_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_char_i_e_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_char_i_e_tc_Conv temp = new BasicSelectionTestClass_char_i_e_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_char_i_e_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_char_i_e_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_char_i_e_tc_Conv temp = new BasicSelectionTestClass_char_i_e_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_char_i_e_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_char_i_e_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_char_i_e_tc_Conv temp = new BasicSelectionTestClass_char_i_e_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_char_i_e_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_char_i_e_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_char_i_e_tc_Conv temp = new BasicSelectionTestClass_char_i_e_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_char_i_e_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_char_i_e_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_char_i_e_tc_Conv temp = new BasicSelectionTestClass_char_i_e_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_char_i_e_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_char_i_e_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_char_i_e_tc_Conv temp = new BasicSelectionTestClass_char_i_e_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_char_i_e_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_char_i_e_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_char_i_e_tc_Conv temp = new BasicSelectionTestClass_char_i_e_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_char_i_e_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_char_i_e_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_char_i_e_tc_Conv temp = new BasicSelectionTestClass_char_i_e_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_char_i_e_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_char_i_e_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_char_i_e_tc_Conv temp = new BasicSelectionTestClass_char_i_e_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_char_i_e_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_char_i_e_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_char_i_e_tc_Conv temp = new BasicSelectionTestClass_char_i_e_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_char_i_e_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_char_i_e_tc_Conv cl = new BasicSelectionTestClass_char_i_e_tc_Conv();
                char val = '\x1';
                cl = (BasicSelectionTestClass_char_i_e_tc_Conv)val;
                if (cl.GetValuechar() != '\x1')
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_char_e_e_tc_Conv
        {
            private byte value_byte;
            static public explicit operator byte(BasicSelectionTestClass_char_e_e_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public explicit operator BasicSelectionTestClass_char_e_e_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_char_e_e_tc_Conv temp = new BasicSelectionTestClass_char_e_e_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public explicit operator char(BasicSelectionTestClass_char_e_e_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public explicit operator BasicSelectionTestClass_char_e_e_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_char_e_e_tc_Conv temp = new BasicSelectionTestClass_char_e_e_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public explicit operator double(BasicSelectionTestClass_char_e_e_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public explicit operator BasicSelectionTestClass_char_e_e_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_char_e_e_tc_Conv temp = new BasicSelectionTestClass_char_e_e_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public explicit operator float(BasicSelectionTestClass_char_e_e_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public explicit operator BasicSelectionTestClass_char_e_e_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_char_e_e_tc_Conv temp = new BasicSelectionTestClass_char_e_e_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public explicit operator int(BasicSelectionTestClass_char_e_e_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public explicit operator BasicSelectionTestClass_char_e_e_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_char_e_e_tc_Conv temp = new BasicSelectionTestClass_char_e_e_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public explicit operator long(BasicSelectionTestClass_char_e_e_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public explicit operator BasicSelectionTestClass_char_e_e_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_char_e_e_tc_Conv temp = new BasicSelectionTestClass_char_e_e_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public explicit operator sbyte(BasicSelectionTestClass_char_e_e_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public explicit operator BasicSelectionTestClass_char_e_e_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_char_e_e_tc_Conv temp = new BasicSelectionTestClass_char_e_e_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public explicit operator short(BasicSelectionTestClass_char_e_e_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public explicit operator BasicSelectionTestClass_char_e_e_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_char_e_e_tc_Conv temp = new BasicSelectionTestClass_char_e_e_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public explicit operator uint(BasicSelectionTestClass_char_e_e_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public explicit operator BasicSelectionTestClass_char_e_e_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_char_e_e_tc_Conv temp = new BasicSelectionTestClass_char_e_e_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public explicit operator ulong(BasicSelectionTestClass_char_e_e_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public explicit operator BasicSelectionTestClass_char_e_e_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_char_e_e_tc_Conv temp = new BasicSelectionTestClass_char_e_e_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public explicit operator ushort(BasicSelectionTestClass_char_e_e_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public explicit operator BasicSelectionTestClass_char_e_e_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_char_e_e_tc_Conv temp = new BasicSelectionTestClass_char_e_e_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_char_e_e_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_char_e_e_tc_Conv cl = new BasicSelectionTestClass_char_e_e_tc_Conv();
                char val = '\x1';
                cl = (BasicSelectionTestClass_char_e_e_tc_Conv)val;
                if (cl.GetValuechar() != '\x1')
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        
        class BasicSelectionTestClass_char_i_i_fc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_char_i_i_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_char_i_i_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_char_i_i_fc_Conv temp = new BasicSelectionTestClass_char_i_i_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_char_i_i_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_char_i_i_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_char_i_i_fc_Conv temp = new BasicSelectionTestClass_char_i_i_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_char_i_i_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_char_i_i_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_char_i_i_fc_Conv temp = new BasicSelectionTestClass_char_i_i_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_char_i_i_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_char_i_i_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_char_i_i_fc_Conv temp = new BasicSelectionTestClass_char_i_i_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_char_i_i_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_char_i_i_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_char_i_i_fc_Conv temp = new BasicSelectionTestClass_char_i_i_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_char_i_i_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_char_i_i_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_char_i_i_fc_Conv temp = new BasicSelectionTestClass_char_i_i_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_char_i_i_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_char_i_i_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_char_i_i_fc_Conv temp = new BasicSelectionTestClass_char_i_i_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_char_i_i_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_char_i_i_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_char_i_i_fc_Conv temp = new BasicSelectionTestClass_char_i_i_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_char_i_i_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_char_i_i_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_char_i_i_fc_Conv temp = new BasicSelectionTestClass_char_i_i_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_char_i_i_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_char_i_i_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_char_i_i_fc_Conv temp = new BasicSelectionTestClass_char_i_i_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_char_i_i_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_char_i_i_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_char_i_i_fc_Conv temp = new BasicSelectionTestClass_char_i_i_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_char_i_i_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_char_i_i_fc_Conv cl = new BasicSelectionTestClass_char_i_i_fc_Conv();
                cl.SetValuechar('\x1');
                char value = cl;
                if (value != '\x1')
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_char_i_e_fc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_char_i_e_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_char_i_e_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_char_i_e_fc_Conv temp = new BasicSelectionTestClass_char_i_e_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_char_i_e_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_char_i_e_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_char_i_e_fc_Conv temp = new BasicSelectionTestClass_char_i_e_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_char_i_e_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_char_i_e_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_char_i_e_fc_Conv temp = new BasicSelectionTestClass_char_i_e_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_char_i_e_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_char_i_e_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_char_i_e_fc_Conv temp = new BasicSelectionTestClass_char_i_e_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_char_i_e_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_char_i_e_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_char_i_e_fc_Conv temp = new BasicSelectionTestClass_char_i_e_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_char_i_e_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_char_i_e_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_char_i_e_fc_Conv temp = new BasicSelectionTestClass_char_i_e_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_char_i_e_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_char_i_e_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_char_i_e_fc_Conv temp = new BasicSelectionTestClass_char_i_e_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_char_i_e_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_char_i_e_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_char_i_e_fc_Conv temp = new BasicSelectionTestClass_char_i_e_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_char_i_e_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_char_i_e_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_char_i_e_fc_Conv temp = new BasicSelectionTestClass_char_i_e_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_char_i_e_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_char_i_e_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_char_i_e_fc_Conv temp = new BasicSelectionTestClass_char_i_e_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_char_i_e_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_char_i_e_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_char_i_e_fc_Conv temp = new BasicSelectionTestClass_char_i_e_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_char_i_e_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_char_i_e_fc_Conv cl = new BasicSelectionTestClass_char_i_e_fc_Conv();
                cl.SetValuechar('\x1');
                char value = (char)cl;
                if (value != '\x1')
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_char_e_e_fc_Conv
        {
            private byte value_byte;
            static public explicit operator byte(BasicSelectionTestClass_char_e_e_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public explicit operator BasicSelectionTestClass_char_e_e_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_char_e_e_fc_Conv temp = new BasicSelectionTestClass_char_e_e_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public explicit operator char(BasicSelectionTestClass_char_e_e_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public explicit operator BasicSelectionTestClass_char_e_e_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_char_e_e_fc_Conv temp = new BasicSelectionTestClass_char_e_e_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public explicit operator double(BasicSelectionTestClass_char_e_e_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public explicit operator BasicSelectionTestClass_char_e_e_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_char_e_e_fc_Conv temp = new BasicSelectionTestClass_char_e_e_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public explicit operator float(BasicSelectionTestClass_char_e_e_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public explicit operator BasicSelectionTestClass_char_e_e_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_char_e_e_fc_Conv temp = new BasicSelectionTestClass_char_e_e_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public explicit operator int(BasicSelectionTestClass_char_e_e_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public explicit operator BasicSelectionTestClass_char_e_e_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_char_e_e_fc_Conv temp = new BasicSelectionTestClass_char_e_e_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public explicit operator long(BasicSelectionTestClass_char_e_e_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public explicit operator BasicSelectionTestClass_char_e_e_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_char_e_e_fc_Conv temp = new BasicSelectionTestClass_char_e_e_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public explicit operator sbyte(BasicSelectionTestClass_char_e_e_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public explicit operator BasicSelectionTestClass_char_e_e_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_char_e_e_fc_Conv temp = new BasicSelectionTestClass_char_e_e_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public explicit operator short(BasicSelectionTestClass_char_e_e_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public explicit operator BasicSelectionTestClass_char_e_e_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_char_e_e_fc_Conv temp = new BasicSelectionTestClass_char_e_e_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public explicit operator uint(BasicSelectionTestClass_char_e_e_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public explicit operator BasicSelectionTestClass_char_e_e_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_char_e_e_fc_Conv temp = new BasicSelectionTestClass_char_e_e_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public explicit operator ulong(BasicSelectionTestClass_char_e_e_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public explicit operator BasicSelectionTestClass_char_e_e_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_char_e_e_fc_Conv temp = new BasicSelectionTestClass_char_e_e_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public explicit operator ushort(BasicSelectionTestClass_char_e_e_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public explicit operator BasicSelectionTestClass_char_e_e_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_char_e_e_fc_Conv temp = new BasicSelectionTestClass_char_e_e_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_char_e_e_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_char_e_e_fc_Conv cl = new BasicSelectionTestClass_char_e_e_fc_Conv();
                cl.SetValuechar('\x1');
                char value = (char)cl;
                if (value != '\x1')
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_double_i_i_tc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_double_i_i_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_double_i_i_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_double_i_i_tc_Conv temp = new BasicSelectionTestClass_double_i_i_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_double_i_i_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_double_i_i_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_double_i_i_tc_Conv temp = new BasicSelectionTestClass_double_i_i_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_double_i_i_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_double_i_i_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_double_i_i_tc_Conv temp = new BasicSelectionTestClass_double_i_i_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_double_i_i_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_double_i_i_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_double_i_i_tc_Conv temp = new BasicSelectionTestClass_double_i_i_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_double_i_i_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_double_i_i_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_double_i_i_tc_Conv temp = new BasicSelectionTestClass_double_i_i_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_double_i_i_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_double_i_i_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_double_i_i_tc_Conv temp = new BasicSelectionTestClass_double_i_i_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_double_i_i_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_double_i_i_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_double_i_i_tc_Conv temp = new BasicSelectionTestClass_double_i_i_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_double_i_i_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_double_i_i_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_double_i_i_tc_Conv temp = new BasicSelectionTestClass_double_i_i_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_double_i_i_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_double_i_i_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_double_i_i_tc_Conv temp = new BasicSelectionTestClass_double_i_i_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_double_i_i_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_double_i_i_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_double_i_i_tc_Conv temp = new BasicSelectionTestClass_double_i_i_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_double_i_i_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_double_i_i_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_double_i_i_tc_Conv temp = new BasicSelectionTestClass_double_i_i_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_double_i_i_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_double_i_i_tc_Conv cl = new BasicSelectionTestClass_double_i_i_tc_Conv();
                double val = 1.0;
                cl = val;
                if (cl.GetValuedouble() != 1.0)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        
        class BasicSelectionTestClass_double_i_e_tc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_double_i_e_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_double_i_e_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_double_i_e_tc_Conv temp = new BasicSelectionTestClass_double_i_e_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_double_i_e_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_double_i_e_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_double_i_e_tc_Conv temp = new BasicSelectionTestClass_double_i_e_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_double_i_e_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_double_i_e_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_double_i_e_tc_Conv temp = new BasicSelectionTestClass_double_i_e_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_double_i_e_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_double_i_e_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_double_i_e_tc_Conv temp = new BasicSelectionTestClass_double_i_e_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_double_i_e_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_double_i_e_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_double_i_e_tc_Conv temp = new BasicSelectionTestClass_double_i_e_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_double_i_e_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_double_i_e_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_double_i_e_tc_Conv temp = new BasicSelectionTestClass_double_i_e_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_double_i_e_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_double_i_e_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_double_i_e_tc_Conv temp = new BasicSelectionTestClass_double_i_e_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_double_i_e_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_double_i_e_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_double_i_e_tc_Conv temp = new BasicSelectionTestClass_double_i_e_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_double_i_e_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_double_i_e_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_double_i_e_tc_Conv temp = new BasicSelectionTestClass_double_i_e_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_double_i_e_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_double_i_e_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_double_i_e_tc_Conv temp = new BasicSelectionTestClass_double_i_e_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_double_i_e_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_double_i_e_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_double_i_e_tc_Conv temp = new BasicSelectionTestClass_double_i_e_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_double_i_e_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_double_i_e_tc_Conv cl = new BasicSelectionTestClass_double_i_e_tc_Conv();
                double val = 1.0;
                cl = (BasicSelectionTestClass_double_i_e_tc_Conv)val;
                if (cl.GetValuedouble() != 1.0)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_double_e_e_tc_Conv
        {
            private byte value_byte;
            static public explicit operator byte(BasicSelectionTestClass_double_e_e_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public explicit operator BasicSelectionTestClass_double_e_e_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_double_e_e_tc_Conv temp = new BasicSelectionTestClass_double_e_e_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public explicit operator char(BasicSelectionTestClass_double_e_e_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public explicit operator BasicSelectionTestClass_double_e_e_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_double_e_e_tc_Conv temp = new BasicSelectionTestClass_double_e_e_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public explicit operator double(BasicSelectionTestClass_double_e_e_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public explicit operator BasicSelectionTestClass_double_e_e_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_double_e_e_tc_Conv temp = new BasicSelectionTestClass_double_e_e_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public explicit operator float(BasicSelectionTestClass_double_e_e_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public explicit operator BasicSelectionTestClass_double_e_e_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_double_e_e_tc_Conv temp = new BasicSelectionTestClass_double_e_e_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public explicit operator int(BasicSelectionTestClass_double_e_e_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public explicit operator BasicSelectionTestClass_double_e_e_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_double_e_e_tc_Conv temp = new BasicSelectionTestClass_double_e_e_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public explicit operator long(BasicSelectionTestClass_double_e_e_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public explicit operator BasicSelectionTestClass_double_e_e_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_double_e_e_tc_Conv temp = new BasicSelectionTestClass_double_e_e_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public explicit operator sbyte(BasicSelectionTestClass_double_e_e_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public explicit operator BasicSelectionTestClass_double_e_e_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_double_e_e_tc_Conv temp = new BasicSelectionTestClass_double_e_e_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public explicit operator short(BasicSelectionTestClass_double_e_e_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public explicit operator BasicSelectionTestClass_double_e_e_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_double_e_e_tc_Conv temp = new BasicSelectionTestClass_double_e_e_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public explicit operator uint(BasicSelectionTestClass_double_e_e_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public explicit operator BasicSelectionTestClass_double_e_e_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_double_e_e_tc_Conv temp = new BasicSelectionTestClass_double_e_e_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public explicit operator ulong(BasicSelectionTestClass_double_e_e_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public explicit operator BasicSelectionTestClass_double_e_e_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_double_e_e_tc_Conv temp = new BasicSelectionTestClass_double_e_e_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public explicit operator ushort(BasicSelectionTestClass_double_e_e_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public explicit operator BasicSelectionTestClass_double_e_e_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_double_e_e_tc_Conv temp = new BasicSelectionTestClass_double_e_e_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_double_e_e_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_double_e_e_tc_Conv cl = new BasicSelectionTestClass_double_e_e_tc_Conv();
                double val = 1.0;
                cl = (BasicSelectionTestClass_double_e_e_tc_Conv)val;
                if (cl.GetValuedouble() != 1.0)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_double_i_i_fc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_double_i_i_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_double_i_i_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_double_i_i_fc_Conv temp = new BasicSelectionTestClass_double_i_i_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_double_i_i_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_double_i_i_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_double_i_i_fc_Conv temp = new BasicSelectionTestClass_double_i_i_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_double_i_i_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_double_i_i_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_double_i_i_fc_Conv temp = new BasicSelectionTestClass_double_i_i_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_double_i_i_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_double_i_i_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_double_i_i_fc_Conv temp = new BasicSelectionTestClass_double_i_i_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_double_i_i_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_double_i_i_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_double_i_i_fc_Conv temp = new BasicSelectionTestClass_double_i_i_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_double_i_i_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_double_i_i_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_double_i_i_fc_Conv temp = new BasicSelectionTestClass_double_i_i_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_double_i_i_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_double_i_i_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_double_i_i_fc_Conv temp = new BasicSelectionTestClass_double_i_i_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_double_i_i_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_double_i_i_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_double_i_i_fc_Conv temp = new BasicSelectionTestClass_double_i_i_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_double_i_i_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_double_i_i_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_double_i_i_fc_Conv temp = new BasicSelectionTestClass_double_i_i_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_double_i_i_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_double_i_i_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_double_i_i_fc_Conv temp = new BasicSelectionTestClass_double_i_i_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_double_i_i_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_double_i_i_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_double_i_i_fc_Conv temp = new BasicSelectionTestClass_double_i_i_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_double_i_i_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_double_i_i_fc_Conv cl = new BasicSelectionTestClass_double_i_i_fc_Conv();
                cl.SetValuedouble(1.0);
                double value = cl;
                if (value != 1.0)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_double_i_e_fc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_double_i_e_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_double_i_e_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_double_i_e_fc_Conv temp = new BasicSelectionTestClass_double_i_e_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_double_i_e_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_double_i_e_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_double_i_e_fc_Conv temp = new BasicSelectionTestClass_double_i_e_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_double_i_e_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_double_i_e_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_double_i_e_fc_Conv temp = new BasicSelectionTestClass_double_i_e_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_double_i_e_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_double_i_e_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_double_i_e_fc_Conv temp = new BasicSelectionTestClass_double_i_e_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_double_i_e_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_double_i_e_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_double_i_e_fc_Conv temp = new BasicSelectionTestClass_double_i_e_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_double_i_e_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_double_i_e_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_double_i_e_fc_Conv temp = new BasicSelectionTestClass_double_i_e_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_double_i_e_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_double_i_e_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_double_i_e_fc_Conv temp = new BasicSelectionTestClass_double_i_e_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_double_i_e_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_double_i_e_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_double_i_e_fc_Conv temp = new BasicSelectionTestClass_double_i_e_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_double_i_e_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_double_i_e_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_double_i_e_fc_Conv temp = new BasicSelectionTestClass_double_i_e_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_double_i_e_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_double_i_e_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_double_i_e_fc_Conv temp = new BasicSelectionTestClass_double_i_e_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_double_i_e_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_double_i_e_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_double_i_e_fc_Conv temp = new BasicSelectionTestClass_double_i_e_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_double_i_e_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_double_i_e_fc_Conv cl = new BasicSelectionTestClass_double_i_e_fc_Conv();
                cl.SetValuedouble(1.0);
                double value = (double)cl;
                if (value != 1.0)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        
        class BasicSelectionTestClass_double_e_e_fc_Conv
        {
            private byte value_byte;
            static public explicit operator byte(BasicSelectionTestClass_double_e_e_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public explicit operator BasicSelectionTestClass_double_e_e_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_double_e_e_fc_Conv temp = new BasicSelectionTestClass_double_e_e_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public explicit operator char(BasicSelectionTestClass_double_e_e_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public explicit operator BasicSelectionTestClass_double_e_e_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_double_e_e_fc_Conv temp = new BasicSelectionTestClass_double_e_e_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public explicit operator double(BasicSelectionTestClass_double_e_e_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public explicit operator BasicSelectionTestClass_double_e_e_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_double_e_e_fc_Conv temp = new BasicSelectionTestClass_double_e_e_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public explicit operator float(BasicSelectionTestClass_double_e_e_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public explicit operator BasicSelectionTestClass_double_e_e_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_double_e_e_fc_Conv temp = new BasicSelectionTestClass_double_e_e_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public explicit operator int(BasicSelectionTestClass_double_e_e_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public explicit operator BasicSelectionTestClass_double_e_e_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_double_e_e_fc_Conv temp = new BasicSelectionTestClass_double_e_e_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public explicit operator long(BasicSelectionTestClass_double_e_e_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public explicit operator BasicSelectionTestClass_double_e_e_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_double_e_e_fc_Conv temp = new BasicSelectionTestClass_double_e_e_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public explicit operator sbyte(BasicSelectionTestClass_double_e_e_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public explicit operator BasicSelectionTestClass_double_e_e_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_double_e_e_fc_Conv temp = new BasicSelectionTestClass_double_e_e_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public explicit operator short(BasicSelectionTestClass_double_e_e_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public explicit operator BasicSelectionTestClass_double_e_e_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_double_e_e_fc_Conv temp = new BasicSelectionTestClass_double_e_e_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public explicit operator uint(BasicSelectionTestClass_double_e_e_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public explicit operator BasicSelectionTestClass_double_e_e_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_double_e_e_fc_Conv temp = new BasicSelectionTestClass_double_e_e_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public explicit operator ulong(BasicSelectionTestClass_double_e_e_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public explicit operator BasicSelectionTestClass_double_e_e_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_double_e_e_fc_Conv temp = new BasicSelectionTestClass_double_e_e_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public explicit operator ushort(BasicSelectionTestClass_double_e_e_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public explicit operator BasicSelectionTestClass_double_e_e_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_double_e_e_fc_Conv temp = new BasicSelectionTestClass_double_e_e_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_double_e_e_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_double_e_e_fc_Conv cl = new BasicSelectionTestClass_double_e_e_fc_Conv();
                cl.SetValuedouble(1.0);
                double value = (double)cl;
                if (value != 1.0)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_float_i_i_tc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_float_i_i_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_float_i_i_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_float_i_i_tc_Conv temp = new BasicSelectionTestClass_float_i_i_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_float_i_i_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_float_i_i_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_float_i_i_tc_Conv temp = new BasicSelectionTestClass_float_i_i_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_float_i_i_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_float_i_i_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_float_i_i_tc_Conv temp = new BasicSelectionTestClass_float_i_i_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_float_i_i_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_float_i_i_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_float_i_i_tc_Conv temp = new BasicSelectionTestClass_float_i_i_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_float_i_i_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_float_i_i_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_float_i_i_tc_Conv temp = new BasicSelectionTestClass_float_i_i_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_float_i_i_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_float_i_i_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_float_i_i_tc_Conv temp = new BasicSelectionTestClass_float_i_i_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_float_i_i_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_float_i_i_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_float_i_i_tc_Conv temp = new BasicSelectionTestClass_float_i_i_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_float_i_i_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_float_i_i_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_float_i_i_tc_Conv temp = new BasicSelectionTestClass_float_i_i_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_float_i_i_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_float_i_i_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_float_i_i_tc_Conv temp = new BasicSelectionTestClass_float_i_i_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_float_i_i_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_float_i_i_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_float_i_i_tc_Conv temp = new BasicSelectionTestClass_float_i_i_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_float_i_i_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_float_i_i_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_float_i_i_tc_Conv temp = new BasicSelectionTestClass_float_i_i_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_float_i_i_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_float_i_i_tc_Conv cl = new BasicSelectionTestClass_float_i_i_tc_Conv();
                float val = 1F;
                cl = val;
                if (cl.GetValuefloat() != 1F)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_float_i_e_tc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_float_i_e_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_float_i_e_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_float_i_e_tc_Conv temp = new BasicSelectionTestClass_float_i_e_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_float_i_e_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_float_i_e_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_float_i_e_tc_Conv temp = new BasicSelectionTestClass_float_i_e_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_float_i_e_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_float_i_e_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_float_i_e_tc_Conv temp = new BasicSelectionTestClass_float_i_e_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_float_i_e_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_float_i_e_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_float_i_e_tc_Conv temp = new BasicSelectionTestClass_float_i_e_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_float_i_e_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_float_i_e_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_float_i_e_tc_Conv temp = new BasicSelectionTestClass_float_i_e_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_float_i_e_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_float_i_e_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_float_i_e_tc_Conv temp = new BasicSelectionTestClass_float_i_e_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_float_i_e_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_float_i_e_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_float_i_e_tc_Conv temp = new BasicSelectionTestClass_float_i_e_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_float_i_e_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_float_i_e_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_float_i_e_tc_Conv temp = new BasicSelectionTestClass_float_i_e_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_float_i_e_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_float_i_e_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_float_i_e_tc_Conv temp = new BasicSelectionTestClass_float_i_e_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_float_i_e_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_float_i_e_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_float_i_e_tc_Conv temp = new BasicSelectionTestClass_float_i_e_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_float_i_e_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_float_i_e_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_float_i_e_tc_Conv temp = new BasicSelectionTestClass_float_i_e_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_float_i_e_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_float_i_e_tc_Conv cl = new BasicSelectionTestClass_float_i_e_tc_Conv();
                float val = 1F;
                cl = (BasicSelectionTestClass_float_i_e_tc_Conv)val;
                if (cl.GetValuefloat() != 1F)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_float_e_e_tc_Conv
        {
            private byte value_byte;
            static public explicit operator byte(BasicSelectionTestClass_float_e_e_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public explicit operator BasicSelectionTestClass_float_e_e_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_float_e_e_tc_Conv temp = new BasicSelectionTestClass_float_e_e_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public explicit operator char(BasicSelectionTestClass_float_e_e_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public explicit operator BasicSelectionTestClass_float_e_e_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_float_e_e_tc_Conv temp = new BasicSelectionTestClass_float_e_e_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public explicit operator double(BasicSelectionTestClass_float_e_e_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public explicit operator BasicSelectionTestClass_float_e_e_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_float_e_e_tc_Conv temp = new BasicSelectionTestClass_float_e_e_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public explicit operator float(BasicSelectionTestClass_float_e_e_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public explicit operator BasicSelectionTestClass_float_e_e_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_float_e_e_tc_Conv temp = new BasicSelectionTestClass_float_e_e_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public explicit operator int(BasicSelectionTestClass_float_e_e_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public explicit operator BasicSelectionTestClass_float_e_e_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_float_e_e_tc_Conv temp = new BasicSelectionTestClass_float_e_e_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public explicit operator long(BasicSelectionTestClass_float_e_e_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public explicit operator BasicSelectionTestClass_float_e_e_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_float_e_e_tc_Conv temp = new BasicSelectionTestClass_float_e_e_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public explicit operator sbyte(BasicSelectionTestClass_float_e_e_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public explicit operator BasicSelectionTestClass_float_e_e_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_float_e_e_tc_Conv temp = new BasicSelectionTestClass_float_e_e_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public explicit operator short(BasicSelectionTestClass_float_e_e_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public explicit operator BasicSelectionTestClass_float_e_e_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_float_e_e_tc_Conv temp = new BasicSelectionTestClass_float_e_e_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public explicit operator uint(BasicSelectionTestClass_float_e_e_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public explicit operator BasicSelectionTestClass_float_e_e_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_float_e_e_tc_Conv temp = new BasicSelectionTestClass_float_e_e_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public explicit operator ulong(BasicSelectionTestClass_float_e_e_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public explicit operator BasicSelectionTestClass_float_e_e_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_float_e_e_tc_Conv temp = new BasicSelectionTestClass_float_e_e_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public explicit operator ushort(BasicSelectionTestClass_float_e_e_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public explicit operator BasicSelectionTestClass_float_e_e_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_float_e_e_tc_Conv temp = new BasicSelectionTestClass_float_e_e_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_float_e_e_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_float_e_e_tc_Conv cl = new BasicSelectionTestClass_float_e_e_tc_Conv();
                float val = 1F;
                cl = (BasicSelectionTestClass_float_e_e_tc_Conv)val;
                if (cl.GetValuefloat() != 1F)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        
        class BasicSelectionTestClass_float_i_i_fc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_float_i_i_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_float_i_i_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_float_i_i_fc_Conv temp = new BasicSelectionTestClass_float_i_i_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_float_i_i_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_float_i_i_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_float_i_i_fc_Conv temp = new BasicSelectionTestClass_float_i_i_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_float_i_i_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_float_i_i_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_float_i_i_fc_Conv temp = new BasicSelectionTestClass_float_i_i_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_float_i_i_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_float_i_i_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_float_i_i_fc_Conv temp = new BasicSelectionTestClass_float_i_i_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_float_i_i_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_float_i_i_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_float_i_i_fc_Conv temp = new BasicSelectionTestClass_float_i_i_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_float_i_i_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_float_i_i_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_float_i_i_fc_Conv temp = new BasicSelectionTestClass_float_i_i_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_float_i_i_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_float_i_i_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_float_i_i_fc_Conv temp = new BasicSelectionTestClass_float_i_i_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_float_i_i_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_float_i_i_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_float_i_i_fc_Conv temp = new BasicSelectionTestClass_float_i_i_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_float_i_i_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_float_i_i_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_float_i_i_fc_Conv temp = new BasicSelectionTestClass_float_i_i_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_float_i_i_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_float_i_i_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_float_i_i_fc_Conv temp = new BasicSelectionTestClass_float_i_i_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_float_i_i_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_float_i_i_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_float_i_i_fc_Conv temp = new BasicSelectionTestClass_float_i_i_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_float_i_i_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_float_i_i_fc_Conv cl = new BasicSelectionTestClass_float_i_i_fc_Conv();
                cl.SetValuefloat(1F);
                float value = cl;
                if (value != 1F)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_float_i_e_fc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_float_i_e_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_float_i_e_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_float_i_e_fc_Conv temp = new BasicSelectionTestClass_float_i_e_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_float_i_e_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_float_i_e_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_float_i_e_fc_Conv temp = new BasicSelectionTestClass_float_i_e_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_float_i_e_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_float_i_e_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_float_i_e_fc_Conv temp = new BasicSelectionTestClass_float_i_e_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_float_i_e_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_float_i_e_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_float_i_e_fc_Conv temp = new BasicSelectionTestClass_float_i_e_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_float_i_e_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_float_i_e_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_float_i_e_fc_Conv temp = new BasicSelectionTestClass_float_i_e_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_float_i_e_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_float_i_e_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_float_i_e_fc_Conv temp = new BasicSelectionTestClass_float_i_e_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_float_i_e_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_float_i_e_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_float_i_e_fc_Conv temp = new BasicSelectionTestClass_float_i_e_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_float_i_e_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_float_i_e_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_float_i_e_fc_Conv temp = new BasicSelectionTestClass_float_i_e_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_float_i_e_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_float_i_e_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_float_i_e_fc_Conv temp = new BasicSelectionTestClass_float_i_e_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_float_i_e_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_float_i_e_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_float_i_e_fc_Conv temp = new BasicSelectionTestClass_float_i_e_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_float_i_e_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_float_i_e_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_float_i_e_fc_Conv temp = new BasicSelectionTestClass_float_i_e_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_float_i_e_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_float_i_e_fc_Conv cl = new BasicSelectionTestClass_float_i_e_fc_Conv();
                cl.SetValuefloat(1F);
                float value = (float)cl;
                if (value != 1F)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_float_e_e_fc_Conv
        {
            private byte value_byte;
            static public explicit operator byte(BasicSelectionTestClass_float_e_e_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public explicit operator BasicSelectionTestClass_float_e_e_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_float_e_e_fc_Conv temp = new BasicSelectionTestClass_float_e_e_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public explicit operator char(BasicSelectionTestClass_float_e_e_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public explicit operator BasicSelectionTestClass_float_e_e_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_float_e_e_fc_Conv temp = new BasicSelectionTestClass_float_e_e_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public explicit operator double(BasicSelectionTestClass_float_e_e_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public explicit operator BasicSelectionTestClass_float_e_e_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_float_e_e_fc_Conv temp = new BasicSelectionTestClass_float_e_e_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public explicit operator float(BasicSelectionTestClass_float_e_e_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public explicit operator BasicSelectionTestClass_float_e_e_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_float_e_e_fc_Conv temp = new BasicSelectionTestClass_float_e_e_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public explicit operator int(BasicSelectionTestClass_float_e_e_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public explicit operator BasicSelectionTestClass_float_e_e_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_float_e_e_fc_Conv temp = new BasicSelectionTestClass_float_e_e_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public explicit operator long(BasicSelectionTestClass_float_e_e_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public explicit operator BasicSelectionTestClass_float_e_e_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_float_e_e_fc_Conv temp = new BasicSelectionTestClass_float_e_e_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public explicit operator sbyte(BasicSelectionTestClass_float_e_e_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public explicit operator BasicSelectionTestClass_float_e_e_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_float_e_e_fc_Conv temp = new BasicSelectionTestClass_float_e_e_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public explicit operator short(BasicSelectionTestClass_float_e_e_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public explicit operator BasicSelectionTestClass_float_e_e_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_float_e_e_fc_Conv temp = new BasicSelectionTestClass_float_e_e_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public explicit operator uint(BasicSelectionTestClass_float_e_e_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public explicit operator BasicSelectionTestClass_float_e_e_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_float_e_e_fc_Conv temp = new BasicSelectionTestClass_float_e_e_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public explicit operator ulong(BasicSelectionTestClass_float_e_e_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public explicit operator BasicSelectionTestClass_float_e_e_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_float_e_e_fc_Conv temp = new BasicSelectionTestClass_float_e_e_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public explicit operator ushort(BasicSelectionTestClass_float_e_e_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public explicit operator BasicSelectionTestClass_float_e_e_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_float_e_e_fc_Conv temp = new BasicSelectionTestClass_float_e_e_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_float_e_e_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_float_e_e_fc_Conv cl = new BasicSelectionTestClass_float_e_e_fc_Conv();
                cl.SetValuefloat(1F);
                float value = (float)cl;
                if (value != 1F)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_int_i_i_tc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_int_i_i_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_int_i_i_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_int_i_i_tc_Conv temp = new BasicSelectionTestClass_int_i_i_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_int_i_i_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_int_i_i_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_int_i_i_tc_Conv temp = new BasicSelectionTestClass_int_i_i_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_int_i_i_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_int_i_i_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_int_i_i_tc_Conv temp = new BasicSelectionTestClass_int_i_i_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_int_i_i_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_int_i_i_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_int_i_i_tc_Conv temp = new BasicSelectionTestClass_int_i_i_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_int_i_i_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_int_i_i_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_int_i_i_tc_Conv temp = new BasicSelectionTestClass_int_i_i_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_int_i_i_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_int_i_i_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_int_i_i_tc_Conv temp = new BasicSelectionTestClass_int_i_i_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_int_i_i_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_int_i_i_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_int_i_i_tc_Conv temp = new BasicSelectionTestClass_int_i_i_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_int_i_i_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_int_i_i_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_int_i_i_tc_Conv temp = new BasicSelectionTestClass_int_i_i_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_int_i_i_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_int_i_i_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_int_i_i_tc_Conv temp = new BasicSelectionTestClass_int_i_i_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_int_i_i_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_int_i_i_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_int_i_i_tc_Conv temp = new BasicSelectionTestClass_int_i_i_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_int_i_i_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_int_i_i_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_int_i_i_tc_Conv temp = new BasicSelectionTestClass_int_i_i_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_int_i_i_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_int_i_i_tc_Conv cl = new BasicSelectionTestClass_int_i_i_tc_Conv();
                int val = 1;
                cl = val;
                if (cl.GetValueint() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        
        class BasicSelectionTestClass_long_i_i_fc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_long_i_i_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_long_i_i_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_long_i_i_fc_Conv temp = new BasicSelectionTestClass_long_i_i_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_long_i_i_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_long_i_i_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_long_i_i_fc_Conv temp = new BasicSelectionTestClass_long_i_i_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_long_i_i_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_long_i_i_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_long_i_i_fc_Conv temp = new BasicSelectionTestClass_long_i_i_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_long_i_i_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_long_i_i_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_long_i_i_fc_Conv temp = new BasicSelectionTestClass_long_i_i_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_long_i_i_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_long_i_i_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_long_i_i_fc_Conv temp = new BasicSelectionTestClass_long_i_i_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_long_i_i_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_long_i_i_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_long_i_i_fc_Conv temp = new BasicSelectionTestClass_long_i_i_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_long_i_i_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_long_i_i_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_long_i_i_fc_Conv temp = new BasicSelectionTestClass_long_i_i_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_long_i_i_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_long_i_i_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_long_i_i_fc_Conv temp = new BasicSelectionTestClass_long_i_i_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_long_i_i_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_long_i_i_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_long_i_i_fc_Conv temp = new BasicSelectionTestClass_long_i_i_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_long_i_i_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_long_i_i_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_long_i_i_fc_Conv temp = new BasicSelectionTestClass_long_i_i_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_long_i_i_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_long_i_i_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_long_i_i_fc_Conv temp = new BasicSelectionTestClass_long_i_i_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_long_i_i_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_long_i_i_fc_Conv cl = new BasicSelectionTestClass_long_i_i_fc_Conv();
                cl.SetValuelong(1);
                long value = cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_long_i_e_fc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_long_i_e_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_long_i_e_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_long_i_e_fc_Conv temp = new BasicSelectionTestClass_long_i_e_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_long_i_e_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_long_i_e_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_long_i_e_fc_Conv temp = new BasicSelectionTestClass_long_i_e_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_long_i_e_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_long_i_e_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_long_i_e_fc_Conv temp = new BasicSelectionTestClass_long_i_e_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_long_i_e_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_long_i_e_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_long_i_e_fc_Conv temp = new BasicSelectionTestClass_long_i_e_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_long_i_e_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_long_i_e_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_long_i_e_fc_Conv temp = new BasicSelectionTestClass_long_i_e_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_long_i_e_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_long_i_e_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_long_i_e_fc_Conv temp = new BasicSelectionTestClass_long_i_e_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_long_i_e_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_long_i_e_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_long_i_e_fc_Conv temp = new BasicSelectionTestClass_long_i_e_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_long_i_e_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_long_i_e_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_long_i_e_fc_Conv temp = new BasicSelectionTestClass_long_i_e_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_long_i_e_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_long_i_e_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_long_i_e_fc_Conv temp = new BasicSelectionTestClass_long_i_e_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_long_i_e_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_long_i_e_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_long_i_e_fc_Conv temp = new BasicSelectionTestClass_long_i_e_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_long_i_e_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_long_i_e_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_long_i_e_fc_Conv temp = new BasicSelectionTestClass_long_i_e_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_long_i_e_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_long_i_e_fc_Conv cl = new BasicSelectionTestClass_long_i_e_fc_Conv();
                cl.SetValuelong(1);
                long value = (long)cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_long_e_e_fc_Conv
        {
            private byte value_byte;
            static public explicit operator byte(BasicSelectionTestClass_long_e_e_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public explicit operator BasicSelectionTestClass_long_e_e_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_long_e_e_fc_Conv temp = new BasicSelectionTestClass_long_e_e_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public explicit operator char(BasicSelectionTestClass_long_e_e_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public explicit operator BasicSelectionTestClass_long_e_e_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_long_e_e_fc_Conv temp = new BasicSelectionTestClass_long_e_e_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public explicit operator double(BasicSelectionTestClass_long_e_e_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public explicit operator BasicSelectionTestClass_long_e_e_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_long_e_e_fc_Conv temp = new BasicSelectionTestClass_long_e_e_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public explicit operator float(BasicSelectionTestClass_long_e_e_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public explicit operator BasicSelectionTestClass_long_e_e_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_long_e_e_fc_Conv temp = new BasicSelectionTestClass_long_e_e_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public explicit operator int(BasicSelectionTestClass_long_e_e_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public explicit operator BasicSelectionTestClass_long_e_e_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_long_e_e_fc_Conv temp = new BasicSelectionTestClass_long_e_e_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public explicit operator long(BasicSelectionTestClass_long_e_e_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public explicit operator BasicSelectionTestClass_long_e_e_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_long_e_e_fc_Conv temp = new BasicSelectionTestClass_long_e_e_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public explicit operator sbyte(BasicSelectionTestClass_long_e_e_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public explicit operator BasicSelectionTestClass_long_e_e_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_long_e_e_fc_Conv temp = new BasicSelectionTestClass_long_e_e_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public explicit operator short(BasicSelectionTestClass_long_e_e_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public explicit operator BasicSelectionTestClass_long_e_e_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_long_e_e_fc_Conv temp = new BasicSelectionTestClass_long_e_e_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public explicit operator uint(BasicSelectionTestClass_long_e_e_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public explicit operator BasicSelectionTestClass_long_e_e_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_long_e_e_fc_Conv temp = new BasicSelectionTestClass_long_e_e_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public explicit operator ulong(BasicSelectionTestClass_long_e_e_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public explicit operator BasicSelectionTestClass_long_e_e_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_long_e_e_fc_Conv temp = new BasicSelectionTestClass_long_e_e_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public explicit operator ushort(BasicSelectionTestClass_long_e_e_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public explicit operator BasicSelectionTestClass_long_e_e_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_long_e_e_fc_Conv temp = new BasicSelectionTestClass_long_e_e_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_long_e_e_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_long_e_e_fc_Conv cl = new BasicSelectionTestClass_long_e_e_fc_Conv();
                cl.SetValuelong(1);
                long value = (long)cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_sbyte_i_i_tc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_sbyte_i_i_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_i_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_sbyte_i_i_tc_Conv temp = new BasicSelectionTestClass_sbyte_i_i_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_sbyte_i_i_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_i_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_sbyte_i_i_tc_Conv temp = new BasicSelectionTestClass_sbyte_i_i_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_sbyte_i_i_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_i_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_sbyte_i_i_tc_Conv temp = new BasicSelectionTestClass_sbyte_i_i_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_sbyte_i_i_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_i_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_sbyte_i_i_tc_Conv temp = new BasicSelectionTestClass_sbyte_i_i_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_sbyte_i_i_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_i_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_sbyte_i_i_tc_Conv temp = new BasicSelectionTestClass_sbyte_i_i_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_sbyte_i_i_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_i_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_sbyte_i_i_tc_Conv temp = new BasicSelectionTestClass_sbyte_i_i_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_sbyte_i_i_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_i_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_sbyte_i_i_tc_Conv temp = new BasicSelectionTestClass_sbyte_i_i_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_sbyte_i_i_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_i_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_sbyte_i_i_tc_Conv temp = new BasicSelectionTestClass_sbyte_i_i_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_sbyte_i_i_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_i_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_sbyte_i_i_tc_Conv temp = new BasicSelectionTestClass_sbyte_i_i_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_sbyte_i_i_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_i_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_sbyte_i_i_tc_Conv temp = new BasicSelectionTestClass_sbyte_i_i_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_sbyte_i_i_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_i_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_sbyte_i_i_tc_Conv temp = new BasicSelectionTestClass_sbyte_i_i_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_sbyte_i_i_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_sbyte_i_i_tc_Conv cl = new BasicSelectionTestClass_sbyte_i_i_tc_Conv();
                sbyte val = 1;
                cl = val;
                if (cl.GetValuesbyte() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }



        class BasicSelectionTestClass_int_i_e_tc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_int_i_e_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_int_i_e_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_int_i_e_tc_Conv temp = new BasicSelectionTestClass_int_i_e_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_int_i_e_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_int_i_e_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_int_i_e_tc_Conv temp = new BasicSelectionTestClass_int_i_e_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_int_i_e_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_int_i_e_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_int_i_e_tc_Conv temp = new BasicSelectionTestClass_int_i_e_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_int_i_e_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_int_i_e_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_int_i_e_tc_Conv temp = new BasicSelectionTestClass_int_i_e_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_int_i_e_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_int_i_e_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_int_i_e_tc_Conv temp = new BasicSelectionTestClass_int_i_e_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_int_i_e_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_int_i_e_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_int_i_e_tc_Conv temp = new BasicSelectionTestClass_int_i_e_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_int_i_e_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_int_i_e_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_int_i_e_tc_Conv temp = new BasicSelectionTestClass_int_i_e_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_int_i_e_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_int_i_e_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_int_i_e_tc_Conv temp = new BasicSelectionTestClass_int_i_e_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_int_i_e_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_int_i_e_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_int_i_e_tc_Conv temp = new BasicSelectionTestClass_int_i_e_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_int_i_e_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_int_i_e_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_int_i_e_tc_Conv temp = new BasicSelectionTestClass_int_i_e_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_int_i_e_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_int_i_e_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_int_i_e_tc_Conv temp = new BasicSelectionTestClass_int_i_e_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_int_i_e_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_int_i_e_tc_Conv cl = new BasicSelectionTestClass_int_i_e_tc_Conv();
                int val = 1;
                cl = (BasicSelectionTestClass_int_i_e_tc_Conv)val;
                if (cl.GetValueint() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {

                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_int_e_e_tc_Conv
        {
            private byte value_byte;
            static public explicit operator byte(BasicSelectionTestClass_int_e_e_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public explicit operator BasicSelectionTestClass_int_e_e_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_int_e_e_tc_Conv temp = new BasicSelectionTestClass_int_e_e_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public explicit operator char(BasicSelectionTestClass_int_e_e_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public explicit operator BasicSelectionTestClass_int_e_e_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_int_e_e_tc_Conv temp = new BasicSelectionTestClass_int_e_e_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public explicit operator double(BasicSelectionTestClass_int_e_e_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public explicit operator BasicSelectionTestClass_int_e_e_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_int_e_e_tc_Conv temp = new BasicSelectionTestClass_int_e_e_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public explicit operator float(BasicSelectionTestClass_int_e_e_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public explicit operator BasicSelectionTestClass_int_e_e_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_int_e_e_tc_Conv temp = new BasicSelectionTestClass_int_e_e_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public explicit operator int(BasicSelectionTestClass_int_e_e_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public explicit operator BasicSelectionTestClass_int_e_e_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_int_e_e_tc_Conv temp = new BasicSelectionTestClass_int_e_e_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public explicit operator long(BasicSelectionTestClass_int_e_e_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public explicit operator BasicSelectionTestClass_int_e_e_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_int_e_e_tc_Conv temp = new BasicSelectionTestClass_int_e_e_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public explicit operator sbyte(BasicSelectionTestClass_int_e_e_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public explicit operator BasicSelectionTestClass_int_e_e_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_int_e_e_tc_Conv temp = new BasicSelectionTestClass_int_e_e_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public explicit operator short(BasicSelectionTestClass_int_e_e_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public explicit operator BasicSelectionTestClass_int_e_e_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_int_e_e_tc_Conv temp = new BasicSelectionTestClass_int_e_e_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public explicit operator uint(BasicSelectionTestClass_int_e_e_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public explicit operator BasicSelectionTestClass_int_e_e_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_int_e_e_tc_Conv temp = new BasicSelectionTestClass_int_e_e_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public explicit operator ulong(BasicSelectionTestClass_int_e_e_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public explicit operator BasicSelectionTestClass_int_e_e_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_int_e_e_tc_Conv temp = new BasicSelectionTestClass_int_e_e_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public explicit operator ushort(BasicSelectionTestClass_int_e_e_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public explicit operator BasicSelectionTestClass_int_e_e_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_int_e_e_tc_Conv temp = new BasicSelectionTestClass_int_e_e_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_int_e_e_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_int_e_e_tc_Conv cl = new BasicSelectionTestClass_int_e_e_tc_Conv();
                int val = 1;
                cl = (BasicSelectionTestClass_int_e_e_tc_Conv)val;
                if (cl.GetValueint() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {

                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_int_i_i_fc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_int_i_i_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_int_i_i_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_int_i_i_fc_Conv temp = new BasicSelectionTestClass_int_i_i_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_int_i_i_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_int_i_i_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_int_i_i_fc_Conv temp = new BasicSelectionTestClass_int_i_i_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_int_i_i_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_int_i_i_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_int_i_i_fc_Conv temp = new BasicSelectionTestClass_int_i_i_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_int_i_i_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_int_i_i_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_int_i_i_fc_Conv temp = new BasicSelectionTestClass_int_i_i_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_int_i_i_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_int_i_i_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_int_i_i_fc_Conv temp = new BasicSelectionTestClass_int_i_i_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_int_i_i_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_int_i_i_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_int_i_i_fc_Conv temp = new BasicSelectionTestClass_int_i_i_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_int_i_i_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_int_i_i_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_int_i_i_fc_Conv temp = new BasicSelectionTestClass_int_i_i_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_int_i_i_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_int_i_i_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_int_i_i_fc_Conv temp = new BasicSelectionTestClass_int_i_i_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_int_i_i_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_int_i_i_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_int_i_i_fc_Conv temp = new BasicSelectionTestClass_int_i_i_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_int_i_i_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_int_i_i_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_int_i_i_fc_Conv temp = new BasicSelectionTestClass_int_i_i_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_int_i_i_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_int_i_i_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_int_i_i_fc_Conv temp = new BasicSelectionTestClass_int_i_i_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_int_i_i_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_int_i_i_fc_Conv cl = new BasicSelectionTestClass_int_i_i_fc_Conv();
                cl.SetValueint(1);
                int value = cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {

                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_int_i_e_fc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_int_i_e_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_int_i_e_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_int_i_e_fc_Conv temp = new BasicSelectionTestClass_int_i_e_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_int_i_e_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_int_i_e_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_int_i_e_fc_Conv temp = new BasicSelectionTestClass_int_i_e_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_int_i_e_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_int_i_e_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_int_i_e_fc_Conv temp = new BasicSelectionTestClass_int_i_e_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_int_i_e_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_int_i_e_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_int_i_e_fc_Conv temp = new BasicSelectionTestClass_int_i_e_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_int_i_e_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_int_i_e_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_int_i_e_fc_Conv temp = new BasicSelectionTestClass_int_i_e_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_int_i_e_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_int_i_e_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_int_i_e_fc_Conv temp = new BasicSelectionTestClass_int_i_e_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_int_i_e_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_int_i_e_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_int_i_e_fc_Conv temp = new BasicSelectionTestClass_int_i_e_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_int_i_e_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_int_i_e_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_int_i_e_fc_Conv temp = new BasicSelectionTestClass_int_i_e_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_int_i_e_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_int_i_e_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_int_i_e_fc_Conv temp = new BasicSelectionTestClass_int_i_e_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_int_i_e_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_int_i_e_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_int_i_e_fc_Conv temp = new BasicSelectionTestClass_int_i_e_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_int_i_e_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_int_i_e_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_int_i_e_fc_Conv temp = new BasicSelectionTestClass_int_i_e_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_int_i_e_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_int_i_e_fc_Conv cl = new BasicSelectionTestClass_int_i_e_fc_Conv();
                cl.SetValueint(1);
                int value = (int)cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {

                return (Main_old() == 0);
            }
        }

        class BasicSelectionTestClass_int_e_e_fc_Conv
        {
            private byte value_byte;
            static public explicit operator byte(BasicSelectionTestClass_int_e_e_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public explicit operator BasicSelectionTestClass_int_e_e_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_int_e_e_fc_Conv temp = new BasicSelectionTestClass_int_e_e_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public explicit operator char(BasicSelectionTestClass_int_e_e_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public explicit operator BasicSelectionTestClass_int_e_e_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_int_e_e_fc_Conv temp = new BasicSelectionTestClass_int_e_e_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public explicit operator double(BasicSelectionTestClass_int_e_e_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public explicit operator BasicSelectionTestClass_int_e_e_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_int_e_e_fc_Conv temp = new BasicSelectionTestClass_int_e_e_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public explicit operator float(BasicSelectionTestClass_int_e_e_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public explicit operator BasicSelectionTestClass_int_e_e_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_int_e_e_fc_Conv temp = new BasicSelectionTestClass_int_e_e_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public explicit operator int(BasicSelectionTestClass_int_e_e_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public explicit operator BasicSelectionTestClass_int_e_e_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_int_e_e_fc_Conv temp = new BasicSelectionTestClass_int_e_e_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public explicit operator long(BasicSelectionTestClass_int_e_e_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public explicit operator BasicSelectionTestClass_int_e_e_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_int_e_e_fc_Conv temp = new BasicSelectionTestClass_int_e_e_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public explicit operator sbyte(BasicSelectionTestClass_int_e_e_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public explicit operator BasicSelectionTestClass_int_e_e_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_int_e_e_fc_Conv temp = new BasicSelectionTestClass_int_e_e_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public explicit operator short(BasicSelectionTestClass_int_e_e_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public explicit operator BasicSelectionTestClass_int_e_e_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_int_e_e_fc_Conv temp = new BasicSelectionTestClass_int_e_e_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public explicit operator uint(BasicSelectionTestClass_int_e_e_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public explicit operator BasicSelectionTestClass_int_e_e_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_int_e_e_fc_Conv temp = new BasicSelectionTestClass_int_e_e_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public explicit operator ulong(BasicSelectionTestClass_int_e_e_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public explicit operator BasicSelectionTestClass_int_e_e_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_int_e_e_fc_Conv temp = new BasicSelectionTestClass_int_e_e_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public explicit operator ushort(BasicSelectionTestClass_int_e_e_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public explicit operator BasicSelectionTestClass_int_e_e_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_int_e_e_fc_Conv temp = new BasicSelectionTestClass_int_e_e_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_int_e_e_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_int_e_e_fc_Conv cl = new BasicSelectionTestClass_int_e_e_fc_Conv();
                cl.SetValueint(1);
                int value = (int)cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {

                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_long_i_i_tc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_long_i_i_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_long_i_i_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_long_i_i_tc_Conv temp = new BasicSelectionTestClass_long_i_i_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_long_i_i_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_long_i_i_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_long_i_i_tc_Conv temp = new BasicSelectionTestClass_long_i_i_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_long_i_i_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_long_i_i_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_long_i_i_tc_Conv temp = new BasicSelectionTestClass_long_i_i_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_long_i_i_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_long_i_i_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_long_i_i_tc_Conv temp = new BasicSelectionTestClass_long_i_i_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_long_i_i_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_long_i_i_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_long_i_i_tc_Conv temp = new BasicSelectionTestClass_long_i_i_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_long_i_i_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_long_i_i_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_long_i_i_tc_Conv temp = new BasicSelectionTestClass_long_i_i_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_long_i_i_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_long_i_i_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_long_i_i_tc_Conv temp = new BasicSelectionTestClass_long_i_i_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_long_i_i_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_long_i_i_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_long_i_i_tc_Conv temp = new BasicSelectionTestClass_long_i_i_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_long_i_i_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_long_i_i_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_long_i_i_tc_Conv temp = new BasicSelectionTestClass_long_i_i_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_long_i_i_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_long_i_i_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_long_i_i_tc_Conv temp = new BasicSelectionTestClass_long_i_i_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_long_i_i_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_long_i_i_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_long_i_i_tc_Conv temp = new BasicSelectionTestClass_long_i_i_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_long_i_i_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_long_i_i_tc_Conv cl = new BasicSelectionTestClass_long_i_i_tc_Conv();
                long val = 1;
                cl = val;
                if (cl.GetValuelong() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {

                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_long_i_e_tc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_long_i_e_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_long_i_e_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_long_i_e_tc_Conv temp = new BasicSelectionTestClass_long_i_e_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_long_i_e_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_long_i_e_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_long_i_e_tc_Conv temp = new BasicSelectionTestClass_long_i_e_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_long_i_e_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_long_i_e_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_long_i_e_tc_Conv temp = new BasicSelectionTestClass_long_i_e_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_long_i_e_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_long_i_e_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_long_i_e_tc_Conv temp = new BasicSelectionTestClass_long_i_e_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_long_i_e_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_long_i_e_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_long_i_e_tc_Conv temp = new BasicSelectionTestClass_long_i_e_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_long_i_e_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_long_i_e_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_long_i_e_tc_Conv temp = new BasicSelectionTestClass_long_i_e_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_long_i_e_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_long_i_e_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_long_i_e_tc_Conv temp = new BasicSelectionTestClass_long_i_e_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_long_i_e_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_long_i_e_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_long_i_e_tc_Conv temp = new BasicSelectionTestClass_long_i_e_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_long_i_e_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_long_i_e_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_long_i_e_tc_Conv temp = new BasicSelectionTestClass_long_i_e_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_long_i_e_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_long_i_e_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_long_i_e_tc_Conv temp = new BasicSelectionTestClass_long_i_e_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_long_i_e_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_long_i_e_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_long_i_e_tc_Conv temp = new BasicSelectionTestClass_long_i_e_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_long_i_e_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_long_i_e_tc_Conv cl = new BasicSelectionTestClass_long_i_e_tc_Conv();
                long val = 1;
                cl = (BasicSelectionTestClass_long_i_e_tc_Conv)val;
                if (cl.GetValuelong() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {

                return (Main_old() == 0);
            }
        }


        class BasicSelectionTestClass_long_e_e_tc_Conv
        {
            private byte value_byte;
            static public explicit operator byte(BasicSelectionTestClass_long_e_e_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public explicit operator BasicSelectionTestClass_long_e_e_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_long_e_e_tc_Conv temp = new BasicSelectionTestClass_long_e_e_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public explicit operator char(BasicSelectionTestClass_long_e_e_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public explicit operator BasicSelectionTestClass_long_e_e_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_long_e_e_tc_Conv temp = new BasicSelectionTestClass_long_e_e_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public explicit operator double(BasicSelectionTestClass_long_e_e_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public explicit operator BasicSelectionTestClass_long_e_e_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_long_e_e_tc_Conv temp = new BasicSelectionTestClass_long_e_e_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public explicit operator float(BasicSelectionTestClass_long_e_e_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public explicit operator BasicSelectionTestClass_long_e_e_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_long_e_e_tc_Conv temp = new BasicSelectionTestClass_long_e_e_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public explicit operator int(BasicSelectionTestClass_long_e_e_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public explicit operator BasicSelectionTestClass_long_e_e_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_long_e_e_tc_Conv temp = new BasicSelectionTestClass_long_e_e_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public explicit operator long(BasicSelectionTestClass_long_e_e_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public explicit operator BasicSelectionTestClass_long_e_e_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_long_e_e_tc_Conv temp = new BasicSelectionTestClass_long_e_e_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public explicit operator sbyte(BasicSelectionTestClass_long_e_e_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public explicit operator BasicSelectionTestClass_long_e_e_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_long_e_e_tc_Conv temp = new BasicSelectionTestClass_long_e_e_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public explicit operator short(BasicSelectionTestClass_long_e_e_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public explicit operator BasicSelectionTestClass_long_e_e_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_long_e_e_tc_Conv temp = new BasicSelectionTestClass_long_e_e_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public explicit operator uint(BasicSelectionTestClass_long_e_e_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public explicit operator BasicSelectionTestClass_long_e_e_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_long_e_e_tc_Conv temp = new BasicSelectionTestClass_long_e_e_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public explicit operator ulong(BasicSelectionTestClass_long_e_e_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public explicit operator BasicSelectionTestClass_long_e_e_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_long_e_e_tc_Conv temp = new BasicSelectionTestClass_long_e_e_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public explicit operator ushort(BasicSelectionTestClass_long_e_e_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public explicit operator BasicSelectionTestClass_long_e_e_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_long_e_e_tc_Conv temp = new BasicSelectionTestClass_long_e_e_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_long_e_e_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_long_e_e_tc_Conv cl = new BasicSelectionTestClass_long_e_e_tc_Conv();
                long val = 1;
                cl = (BasicSelectionTestClass_long_e_e_tc_Conv)val;
                if (cl.GetValuelong() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {

                return (Main_old() == 0);
            }
        }
          
        class BasicSelectionTestClass_sbyte_i_e_tc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_sbyte_i_e_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_e_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_sbyte_i_e_tc_Conv temp = new BasicSelectionTestClass_sbyte_i_e_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_sbyte_i_e_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_e_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_sbyte_i_e_tc_Conv temp = new BasicSelectionTestClass_sbyte_i_e_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_sbyte_i_e_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_e_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_sbyte_i_e_tc_Conv temp = new BasicSelectionTestClass_sbyte_i_e_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_sbyte_i_e_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_e_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_sbyte_i_e_tc_Conv temp = new BasicSelectionTestClass_sbyte_i_e_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_sbyte_i_e_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_e_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_sbyte_i_e_tc_Conv temp = new BasicSelectionTestClass_sbyte_i_e_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_sbyte_i_e_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_e_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_sbyte_i_e_tc_Conv temp = new BasicSelectionTestClass_sbyte_i_e_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_sbyte_i_e_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_e_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_sbyte_i_e_tc_Conv temp = new BasicSelectionTestClass_sbyte_i_e_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_sbyte_i_e_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_e_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_sbyte_i_e_tc_Conv temp = new BasicSelectionTestClass_sbyte_i_e_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_sbyte_i_e_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_e_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_sbyte_i_e_tc_Conv temp = new BasicSelectionTestClass_sbyte_i_e_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_sbyte_i_e_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_e_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_sbyte_i_e_tc_Conv temp = new BasicSelectionTestClass_sbyte_i_e_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_sbyte_i_e_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_e_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_sbyte_i_e_tc_Conv temp = new BasicSelectionTestClass_sbyte_i_e_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_sbyte_i_e_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_sbyte_i_e_tc_Conv cl = new BasicSelectionTestClass_sbyte_i_e_tc_Conv();
                sbyte val = 1;
                cl = (BasicSelectionTestClass_sbyte_i_e_tc_Conv)val;
                if (cl.GetValuesbyte() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_sbyte_e_e_tc_Conv
        {
            private byte value_byte;
            static public explicit operator byte(BasicSelectionTestClass_sbyte_e_e_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public explicit operator BasicSelectionTestClass_sbyte_e_e_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_sbyte_e_e_tc_Conv temp = new BasicSelectionTestClass_sbyte_e_e_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public explicit operator char(BasicSelectionTestClass_sbyte_e_e_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public explicit operator BasicSelectionTestClass_sbyte_e_e_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_sbyte_e_e_tc_Conv temp = new BasicSelectionTestClass_sbyte_e_e_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public explicit operator double(BasicSelectionTestClass_sbyte_e_e_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public explicit operator BasicSelectionTestClass_sbyte_e_e_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_sbyte_e_e_tc_Conv temp = new BasicSelectionTestClass_sbyte_e_e_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public explicit operator float(BasicSelectionTestClass_sbyte_e_e_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public explicit operator BasicSelectionTestClass_sbyte_e_e_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_sbyte_e_e_tc_Conv temp = new BasicSelectionTestClass_sbyte_e_e_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public explicit operator int(BasicSelectionTestClass_sbyte_e_e_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public explicit operator BasicSelectionTestClass_sbyte_e_e_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_sbyte_e_e_tc_Conv temp = new BasicSelectionTestClass_sbyte_e_e_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public explicit operator long(BasicSelectionTestClass_sbyte_e_e_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public explicit operator BasicSelectionTestClass_sbyte_e_e_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_sbyte_e_e_tc_Conv temp = new BasicSelectionTestClass_sbyte_e_e_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public explicit operator sbyte(BasicSelectionTestClass_sbyte_e_e_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public explicit operator BasicSelectionTestClass_sbyte_e_e_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_sbyte_e_e_tc_Conv temp = new BasicSelectionTestClass_sbyte_e_e_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public explicit operator short(BasicSelectionTestClass_sbyte_e_e_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public explicit operator BasicSelectionTestClass_sbyte_e_e_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_sbyte_e_e_tc_Conv temp = new BasicSelectionTestClass_sbyte_e_e_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public explicit operator uint(BasicSelectionTestClass_sbyte_e_e_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public explicit operator BasicSelectionTestClass_sbyte_e_e_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_sbyte_e_e_tc_Conv temp = new BasicSelectionTestClass_sbyte_e_e_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public explicit operator ulong(BasicSelectionTestClass_sbyte_e_e_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public explicit operator BasicSelectionTestClass_sbyte_e_e_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_sbyte_e_e_tc_Conv temp = new BasicSelectionTestClass_sbyte_e_e_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public explicit operator ushort(BasicSelectionTestClass_sbyte_e_e_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public explicit operator BasicSelectionTestClass_sbyte_e_e_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_sbyte_e_e_tc_Conv temp = new BasicSelectionTestClass_sbyte_e_e_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_sbyte_e_e_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_sbyte_e_e_tc_Conv cl = new BasicSelectionTestClass_sbyte_e_e_tc_Conv();
                sbyte val = 1;
                cl = (BasicSelectionTestClass_sbyte_e_e_tc_Conv)val;
                if (cl.GetValuesbyte() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_sbyte_i_i_fc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_sbyte_i_i_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_i_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_sbyte_i_i_fc_Conv temp = new BasicSelectionTestClass_sbyte_i_i_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_sbyte_i_i_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_i_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_sbyte_i_i_fc_Conv temp = new BasicSelectionTestClass_sbyte_i_i_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_sbyte_i_i_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_i_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_sbyte_i_i_fc_Conv temp = new BasicSelectionTestClass_sbyte_i_i_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_sbyte_i_i_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_i_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_sbyte_i_i_fc_Conv temp = new BasicSelectionTestClass_sbyte_i_i_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_sbyte_i_i_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_i_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_sbyte_i_i_fc_Conv temp = new BasicSelectionTestClass_sbyte_i_i_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_sbyte_i_i_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_i_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_sbyte_i_i_fc_Conv temp = new BasicSelectionTestClass_sbyte_i_i_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_sbyte_i_i_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_i_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_sbyte_i_i_fc_Conv temp = new BasicSelectionTestClass_sbyte_i_i_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_sbyte_i_i_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_i_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_sbyte_i_i_fc_Conv temp = new BasicSelectionTestClass_sbyte_i_i_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_sbyte_i_i_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_i_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_sbyte_i_i_fc_Conv temp = new BasicSelectionTestClass_sbyte_i_i_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_sbyte_i_i_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_i_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_sbyte_i_i_fc_Conv temp = new BasicSelectionTestClass_sbyte_i_i_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_sbyte_i_i_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_i_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_sbyte_i_i_fc_Conv temp = new BasicSelectionTestClass_sbyte_i_i_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_sbyte_i_i_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_sbyte_i_i_fc_Conv cl = new BasicSelectionTestClass_sbyte_i_i_fc_Conv();
                cl.SetValuesbyte(1);
                sbyte value = cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_sbyte_i_e_fc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_sbyte_i_e_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_e_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_sbyte_i_e_fc_Conv temp = new BasicSelectionTestClass_sbyte_i_e_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_sbyte_i_e_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_e_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_sbyte_i_e_fc_Conv temp = new BasicSelectionTestClass_sbyte_i_e_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_sbyte_i_e_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_e_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_sbyte_i_e_fc_Conv temp = new BasicSelectionTestClass_sbyte_i_e_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_sbyte_i_e_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_e_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_sbyte_i_e_fc_Conv temp = new BasicSelectionTestClass_sbyte_i_e_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_sbyte_i_e_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_e_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_sbyte_i_e_fc_Conv temp = new BasicSelectionTestClass_sbyte_i_e_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_sbyte_i_e_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_e_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_sbyte_i_e_fc_Conv temp = new BasicSelectionTestClass_sbyte_i_e_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_sbyte_i_e_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_e_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_sbyte_i_e_fc_Conv temp = new BasicSelectionTestClass_sbyte_i_e_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_sbyte_i_e_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_e_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_sbyte_i_e_fc_Conv temp = new BasicSelectionTestClass_sbyte_i_e_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_sbyte_i_e_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_e_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_sbyte_i_e_fc_Conv temp = new BasicSelectionTestClass_sbyte_i_e_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_sbyte_i_e_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_e_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_sbyte_i_e_fc_Conv temp = new BasicSelectionTestClass_sbyte_i_e_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_sbyte_i_e_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_sbyte_i_e_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_sbyte_i_e_fc_Conv temp = new BasicSelectionTestClass_sbyte_i_e_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_sbyte_i_e_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_sbyte_i_e_fc_Conv cl = new BasicSelectionTestClass_sbyte_i_e_fc_Conv();
                cl.SetValuesbyte(1);
                sbyte value = (sbyte)cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_sbyte_e_e_fc_Conv
        {
            private byte value_byte;
            static public explicit operator byte(BasicSelectionTestClass_sbyte_e_e_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public explicit operator BasicSelectionTestClass_sbyte_e_e_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_sbyte_e_e_fc_Conv temp = new BasicSelectionTestClass_sbyte_e_e_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public explicit operator char(BasicSelectionTestClass_sbyte_e_e_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public explicit operator BasicSelectionTestClass_sbyte_e_e_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_sbyte_e_e_fc_Conv temp = new BasicSelectionTestClass_sbyte_e_e_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public explicit operator double(BasicSelectionTestClass_sbyte_e_e_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public explicit operator BasicSelectionTestClass_sbyte_e_e_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_sbyte_e_e_fc_Conv temp = new BasicSelectionTestClass_sbyte_e_e_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public explicit operator float(BasicSelectionTestClass_sbyte_e_e_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public explicit operator BasicSelectionTestClass_sbyte_e_e_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_sbyte_e_e_fc_Conv temp = new BasicSelectionTestClass_sbyte_e_e_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public explicit operator int(BasicSelectionTestClass_sbyte_e_e_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public explicit operator BasicSelectionTestClass_sbyte_e_e_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_sbyte_e_e_fc_Conv temp = new BasicSelectionTestClass_sbyte_e_e_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public explicit operator long(BasicSelectionTestClass_sbyte_e_e_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public explicit operator BasicSelectionTestClass_sbyte_e_e_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_sbyte_e_e_fc_Conv temp = new BasicSelectionTestClass_sbyte_e_e_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public explicit operator sbyte(BasicSelectionTestClass_sbyte_e_e_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public explicit operator BasicSelectionTestClass_sbyte_e_e_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_sbyte_e_e_fc_Conv temp = new BasicSelectionTestClass_sbyte_e_e_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public explicit operator short(BasicSelectionTestClass_sbyte_e_e_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public explicit operator BasicSelectionTestClass_sbyte_e_e_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_sbyte_e_e_fc_Conv temp = new BasicSelectionTestClass_sbyte_e_e_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public explicit operator uint(BasicSelectionTestClass_sbyte_e_e_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public explicit operator BasicSelectionTestClass_sbyte_e_e_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_sbyte_e_e_fc_Conv temp = new BasicSelectionTestClass_sbyte_e_e_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public explicit operator ulong(BasicSelectionTestClass_sbyte_e_e_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public explicit operator BasicSelectionTestClass_sbyte_e_e_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_sbyte_e_e_fc_Conv temp = new BasicSelectionTestClass_sbyte_e_e_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public explicit operator ushort(BasicSelectionTestClass_sbyte_e_e_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public explicit operator BasicSelectionTestClass_sbyte_e_e_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_sbyte_e_e_fc_Conv temp = new BasicSelectionTestClass_sbyte_e_e_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_sbyte_e_e_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_sbyte_e_e_fc_Conv cl = new BasicSelectionTestClass_sbyte_e_e_fc_Conv();
                cl.SetValuesbyte(1);
                sbyte value = (sbyte)cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_short_i_i_tc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_short_i_i_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_short_i_i_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_short_i_i_tc_Conv temp = new BasicSelectionTestClass_short_i_i_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_short_i_i_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_short_i_i_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_short_i_i_tc_Conv temp = new BasicSelectionTestClass_short_i_i_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_short_i_i_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_short_i_i_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_short_i_i_tc_Conv temp = new BasicSelectionTestClass_short_i_i_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_short_i_i_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_short_i_i_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_short_i_i_tc_Conv temp = new BasicSelectionTestClass_short_i_i_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_short_i_i_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_short_i_i_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_short_i_i_tc_Conv temp = new BasicSelectionTestClass_short_i_i_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_short_i_i_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_short_i_i_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_short_i_i_tc_Conv temp = new BasicSelectionTestClass_short_i_i_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_short_i_i_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_short_i_i_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_short_i_i_tc_Conv temp = new BasicSelectionTestClass_short_i_i_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_short_i_i_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_short_i_i_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_short_i_i_tc_Conv temp = new BasicSelectionTestClass_short_i_i_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_short_i_i_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_short_i_i_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_short_i_i_tc_Conv temp = new BasicSelectionTestClass_short_i_i_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_short_i_i_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_short_i_i_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_short_i_i_tc_Conv temp = new BasicSelectionTestClass_short_i_i_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_short_i_i_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_short_i_i_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_short_i_i_tc_Conv temp = new BasicSelectionTestClass_short_i_i_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_short_i_i_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_short_i_i_tc_Conv cl = new BasicSelectionTestClass_short_i_i_tc_Conv();
                short val = 1;
                cl = val;
                if (cl.GetValueshort() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_short_i_e_tc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_short_i_e_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_short_i_e_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_short_i_e_tc_Conv temp = new BasicSelectionTestClass_short_i_e_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_short_i_e_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_short_i_e_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_short_i_e_tc_Conv temp = new BasicSelectionTestClass_short_i_e_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_short_i_e_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_short_i_e_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_short_i_e_tc_Conv temp = new BasicSelectionTestClass_short_i_e_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_short_i_e_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_short_i_e_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_short_i_e_tc_Conv temp = new BasicSelectionTestClass_short_i_e_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_short_i_e_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_short_i_e_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_short_i_e_tc_Conv temp = new BasicSelectionTestClass_short_i_e_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_short_i_e_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_short_i_e_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_short_i_e_tc_Conv temp = new BasicSelectionTestClass_short_i_e_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_short_i_e_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_short_i_e_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_short_i_e_tc_Conv temp = new BasicSelectionTestClass_short_i_e_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_short_i_e_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_short_i_e_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_short_i_e_tc_Conv temp = new BasicSelectionTestClass_short_i_e_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_short_i_e_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_short_i_e_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_short_i_e_tc_Conv temp = new BasicSelectionTestClass_short_i_e_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_short_i_e_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_short_i_e_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_short_i_e_tc_Conv temp = new BasicSelectionTestClass_short_i_e_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_short_i_e_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_short_i_e_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_short_i_e_tc_Conv temp = new BasicSelectionTestClass_short_i_e_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_short_i_e_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_short_i_e_tc_Conv cl = new BasicSelectionTestClass_short_i_e_tc_Conv();
                short val = 1;
                cl = (BasicSelectionTestClass_short_i_e_tc_Conv)val;
                if (cl.GetValueshort() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_short_e_e_tc_Conv
        {
            private byte value_byte;
            static public explicit operator byte(BasicSelectionTestClass_short_e_e_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public explicit operator BasicSelectionTestClass_short_e_e_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_short_e_e_tc_Conv temp = new BasicSelectionTestClass_short_e_e_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public explicit operator char(BasicSelectionTestClass_short_e_e_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public explicit operator BasicSelectionTestClass_short_e_e_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_short_e_e_tc_Conv temp = new BasicSelectionTestClass_short_e_e_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public explicit operator double(BasicSelectionTestClass_short_e_e_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public explicit operator BasicSelectionTestClass_short_e_e_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_short_e_e_tc_Conv temp = new BasicSelectionTestClass_short_e_e_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public explicit operator float(BasicSelectionTestClass_short_e_e_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public explicit operator BasicSelectionTestClass_short_e_e_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_short_e_e_tc_Conv temp = new BasicSelectionTestClass_short_e_e_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public explicit operator int(BasicSelectionTestClass_short_e_e_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public explicit operator BasicSelectionTestClass_short_e_e_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_short_e_e_tc_Conv temp = new BasicSelectionTestClass_short_e_e_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public explicit operator long(BasicSelectionTestClass_short_e_e_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public explicit operator BasicSelectionTestClass_short_e_e_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_short_e_e_tc_Conv temp = new BasicSelectionTestClass_short_e_e_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public explicit operator sbyte(BasicSelectionTestClass_short_e_e_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public explicit operator BasicSelectionTestClass_short_e_e_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_short_e_e_tc_Conv temp = new BasicSelectionTestClass_short_e_e_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public explicit operator short(BasicSelectionTestClass_short_e_e_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public explicit operator BasicSelectionTestClass_short_e_e_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_short_e_e_tc_Conv temp = new BasicSelectionTestClass_short_e_e_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public explicit operator uint(BasicSelectionTestClass_short_e_e_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public explicit operator BasicSelectionTestClass_short_e_e_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_short_e_e_tc_Conv temp = new BasicSelectionTestClass_short_e_e_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public explicit operator ulong(BasicSelectionTestClass_short_e_e_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public explicit operator BasicSelectionTestClass_short_e_e_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_short_e_e_tc_Conv temp = new BasicSelectionTestClass_short_e_e_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public explicit operator ushort(BasicSelectionTestClass_short_e_e_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public explicit operator BasicSelectionTestClass_short_e_e_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_short_e_e_tc_Conv temp = new BasicSelectionTestClass_short_e_e_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_short_e_e_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_short_e_e_tc_Conv cl = new BasicSelectionTestClass_short_e_e_tc_Conv();
                short val = 1;
                cl = (BasicSelectionTestClass_short_e_e_tc_Conv)val;
                if (cl.GetValueshort() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_short_i_i_fc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_short_i_i_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_short_i_i_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_short_i_i_fc_Conv temp = new BasicSelectionTestClass_short_i_i_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_short_i_i_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_short_i_i_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_short_i_i_fc_Conv temp = new BasicSelectionTestClass_short_i_i_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_short_i_i_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_short_i_i_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_short_i_i_fc_Conv temp = new BasicSelectionTestClass_short_i_i_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_short_i_i_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_short_i_i_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_short_i_i_fc_Conv temp = new BasicSelectionTestClass_short_i_i_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_short_i_i_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_short_i_i_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_short_i_i_fc_Conv temp = new BasicSelectionTestClass_short_i_i_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_short_i_i_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_short_i_i_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_short_i_i_fc_Conv temp = new BasicSelectionTestClass_short_i_i_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_short_i_i_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_short_i_i_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_short_i_i_fc_Conv temp = new BasicSelectionTestClass_short_i_i_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_short_i_i_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_short_i_i_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_short_i_i_fc_Conv temp = new BasicSelectionTestClass_short_i_i_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_short_i_i_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_short_i_i_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_short_i_i_fc_Conv temp = new BasicSelectionTestClass_short_i_i_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_short_i_i_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_short_i_i_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_short_i_i_fc_Conv temp = new BasicSelectionTestClass_short_i_i_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_short_i_i_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_short_i_i_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_short_i_i_fc_Conv temp = new BasicSelectionTestClass_short_i_i_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_short_i_i_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_short_i_i_fc_Conv cl = new BasicSelectionTestClass_short_i_i_fc_Conv();
                cl.SetValueshort(1);
                short value = cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_short_i_e_fc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_short_i_e_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_short_i_e_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_short_i_e_fc_Conv temp = new BasicSelectionTestClass_short_i_e_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_short_i_e_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_short_i_e_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_short_i_e_fc_Conv temp = new BasicSelectionTestClass_short_i_e_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_short_i_e_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_short_i_e_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_short_i_e_fc_Conv temp = new BasicSelectionTestClass_short_i_e_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_short_i_e_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_short_i_e_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_short_i_e_fc_Conv temp = new BasicSelectionTestClass_short_i_e_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_short_i_e_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_short_i_e_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_short_i_e_fc_Conv temp = new BasicSelectionTestClass_short_i_e_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_short_i_e_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_short_i_e_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_short_i_e_fc_Conv temp = new BasicSelectionTestClass_short_i_e_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_short_i_e_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_short_i_e_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_short_i_e_fc_Conv temp = new BasicSelectionTestClass_short_i_e_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_short_i_e_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_short_i_e_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_short_i_e_fc_Conv temp = new BasicSelectionTestClass_short_i_e_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_short_i_e_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_short_i_e_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_short_i_e_fc_Conv temp = new BasicSelectionTestClass_short_i_e_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_short_i_e_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_short_i_e_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_short_i_e_fc_Conv temp = new BasicSelectionTestClass_short_i_e_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_short_i_e_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_short_i_e_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_short_i_e_fc_Conv temp = new BasicSelectionTestClass_short_i_e_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_short_i_e_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_short_i_e_fc_Conv cl = new BasicSelectionTestClass_short_i_e_fc_Conv();
                cl.SetValueshort(1);
                short value = (short)cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_short_e_e_fc_Conv
        {
            private byte value_byte;
            static public explicit operator byte(BasicSelectionTestClass_short_e_e_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public explicit operator BasicSelectionTestClass_short_e_e_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_short_e_e_fc_Conv temp = new BasicSelectionTestClass_short_e_e_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public explicit operator char(BasicSelectionTestClass_short_e_e_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public explicit operator BasicSelectionTestClass_short_e_e_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_short_e_e_fc_Conv temp = new BasicSelectionTestClass_short_e_e_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public explicit operator double(BasicSelectionTestClass_short_e_e_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public explicit operator BasicSelectionTestClass_short_e_e_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_short_e_e_fc_Conv temp = new BasicSelectionTestClass_short_e_e_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public explicit operator float(BasicSelectionTestClass_short_e_e_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public explicit operator BasicSelectionTestClass_short_e_e_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_short_e_e_fc_Conv temp = new BasicSelectionTestClass_short_e_e_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public explicit operator int(BasicSelectionTestClass_short_e_e_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public explicit operator BasicSelectionTestClass_short_e_e_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_short_e_e_fc_Conv temp = new BasicSelectionTestClass_short_e_e_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public explicit operator long(BasicSelectionTestClass_short_e_e_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public explicit operator BasicSelectionTestClass_short_e_e_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_short_e_e_fc_Conv temp = new BasicSelectionTestClass_short_e_e_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public explicit operator sbyte(BasicSelectionTestClass_short_e_e_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public explicit operator BasicSelectionTestClass_short_e_e_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_short_e_e_fc_Conv temp = new BasicSelectionTestClass_short_e_e_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public explicit operator short(BasicSelectionTestClass_short_e_e_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public explicit operator BasicSelectionTestClass_short_e_e_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_short_e_e_fc_Conv temp = new BasicSelectionTestClass_short_e_e_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public explicit operator uint(BasicSelectionTestClass_short_e_e_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public explicit operator BasicSelectionTestClass_short_e_e_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_short_e_e_fc_Conv temp = new BasicSelectionTestClass_short_e_e_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public explicit operator ulong(BasicSelectionTestClass_short_e_e_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public explicit operator BasicSelectionTestClass_short_e_e_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_short_e_e_fc_Conv temp = new BasicSelectionTestClass_short_e_e_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public explicit operator ushort(BasicSelectionTestClass_short_e_e_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public explicit operator BasicSelectionTestClass_short_e_e_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_short_e_e_fc_Conv temp = new BasicSelectionTestClass_short_e_e_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_short_e_e_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_short_e_e_fc_Conv cl = new BasicSelectionTestClass_short_e_e_fc_Conv();
                cl.SetValueshort(1);
                short value = (short)cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_uint_i_i_tc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_uint_i_i_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_uint_i_i_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_uint_i_i_tc_Conv temp = new BasicSelectionTestClass_uint_i_i_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_uint_i_i_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_uint_i_i_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_uint_i_i_tc_Conv temp = new BasicSelectionTestClass_uint_i_i_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_uint_i_i_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_uint_i_i_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_uint_i_i_tc_Conv temp = new BasicSelectionTestClass_uint_i_i_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_uint_i_i_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_uint_i_i_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_uint_i_i_tc_Conv temp = new BasicSelectionTestClass_uint_i_i_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_uint_i_i_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_uint_i_i_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_uint_i_i_tc_Conv temp = new BasicSelectionTestClass_uint_i_i_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_uint_i_i_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_uint_i_i_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_uint_i_i_tc_Conv temp = new BasicSelectionTestClass_uint_i_i_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_uint_i_i_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_uint_i_i_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_uint_i_i_tc_Conv temp = new BasicSelectionTestClass_uint_i_i_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_uint_i_i_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_uint_i_i_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_uint_i_i_tc_Conv temp = new BasicSelectionTestClass_uint_i_i_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_uint_i_i_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_uint_i_i_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_uint_i_i_tc_Conv temp = new BasicSelectionTestClass_uint_i_i_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_uint_i_i_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_uint_i_i_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_uint_i_i_tc_Conv temp = new BasicSelectionTestClass_uint_i_i_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_uint_i_i_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_uint_i_i_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_uint_i_i_tc_Conv temp = new BasicSelectionTestClass_uint_i_i_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_uint_i_i_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_uint_i_i_tc_Conv cl = new BasicSelectionTestClass_uint_i_i_tc_Conv();
                uint val = 1;
                cl = val;
                if (cl.GetValueuint() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_uint_i_e_tc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_uint_i_e_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_uint_i_e_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_uint_i_e_tc_Conv temp = new BasicSelectionTestClass_uint_i_e_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_uint_i_e_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_uint_i_e_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_uint_i_e_tc_Conv temp = new BasicSelectionTestClass_uint_i_e_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_uint_i_e_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_uint_i_e_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_uint_i_e_tc_Conv temp = new BasicSelectionTestClass_uint_i_e_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_uint_i_e_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_uint_i_e_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_uint_i_e_tc_Conv temp = new BasicSelectionTestClass_uint_i_e_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_uint_i_e_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_uint_i_e_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_uint_i_e_tc_Conv temp = new BasicSelectionTestClass_uint_i_e_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_uint_i_e_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_uint_i_e_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_uint_i_e_tc_Conv temp = new BasicSelectionTestClass_uint_i_e_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_uint_i_e_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_uint_i_e_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_uint_i_e_tc_Conv temp = new BasicSelectionTestClass_uint_i_e_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_uint_i_e_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_uint_i_e_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_uint_i_e_tc_Conv temp = new BasicSelectionTestClass_uint_i_e_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_uint_i_e_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_uint_i_e_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_uint_i_e_tc_Conv temp = new BasicSelectionTestClass_uint_i_e_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_uint_i_e_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_uint_i_e_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_uint_i_e_tc_Conv temp = new BasicSelectionTestClass_uint_i_e_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_uint_i_e_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_uint_i_e_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_uint_i_e_tc_Conv temp = new BasicSelectionTestClass_uint_i_e_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_uint_i_e_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_uint_i_e_tc_Conv cl = new BasicSelectionTestClass_uint_i_e_tc_Conv();
                uint val = 1;
                cl = (BasicSelectionTestClass_uint_i_e_tc_Conv)val;
                if (cl.GetValueuint() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_uint_e_e_tc_Conv
        {
            private byte value_byte;
            static public explicit operator byte(BasicSelectionTestClass_uint_e_e_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public explicit operator BasicSelectionTestClass_uint_e_e_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_uint_e_e_tc_Conv temp = new BasicSelectionTestClass_uint_e_e_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public explicit operator char(BasicSelectionTestClass_uint_e_e_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public explicit operator BasicSelectionTestClass_uint_e_e_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_uint_e_e_tc_Conv temp = new BasicSelectionTestClass_uint_e_e_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public explicit operator double(BasicSelectionTestClass_uint_e_e_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public explicit operator BasicSelectionTestClass_uint_e_e_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_uint_e_e_tc_Conv temp = new BasicSelectionTestClass_uint_e_e_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public explicit operator float(BasicSelectionTestClass_uint_e_e_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public explicit operator BasicSelectionTestClass_uint_e_e_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_uint_e_e_tc_Conv temp = new BasicSelectionTestClass_uint_e_e_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public explicit operator int(BasicSelectionTestClass_uint_e_e_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public explicit operator BasicSelectionTestClass_uint_e_e_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_uint_e_e_tc_Conv temp = new BasicSelectionTestClass_uint_e_e_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public explicit operator long(BasicSelectionTestClass_uint_e_e_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public explicit operator BasicSelectionTestClass_uint_e_e_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_uint_e_e_tc_Conv temp = new BasicSelectionTestClass_uint_e_e_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public explicit operator sbyte(BasicSelectionTestClass_uint_e_e_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public explicit operator BasicSelectionTestClass_uint_e_e_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_uint_e_e_tc_Conv temp = new BasicSelectionTestClass_uint_e_e_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public explicit operator short(BasicSelectionTestClass_uint_e_e_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public explicit operator BasicSelectionTestClass_uint_e_e_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_uint_e_e_tc_Conv temp = new BasicSelectionTestClass_uint_e_e_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public explicit operator uint(BasicSelectionTestClass_uint_e_e_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public explicit operator BasicSelectionTestClass_uint_e_e_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_uint_e_e_tc_Conv temp = new BasicSelectionTestClass_uint_e_e_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public explicit operator ulong(BasicSelectionTestClass_uint_e_e_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public explicit operator BasicSelectionTestClass_uint_e_e_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_uint_e_e_tc_Conv temp = new BasicSelectionTestClass_uint_e_e_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public explicit operator ushort(BasicSelectionTestClass_uint_e_e_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public explicit operator BasicSelectionTestClass_uint_e_e_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_uint_e_e_tc_Conv temp = new BasicSelectionTestClass_uint_e_e_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_uint_e_e_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_uint_e_e_tc_Conv cl = new BasicSelectionTestClass_uint_e_e_tc_Conv();
                uint val = 1;
                cl = (BasicSelectionTestClass_uint_e_e_tc_Conv)val;
                if (cl.GetValueuint() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_uint_i_i_fc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_uint_i_i_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_uint_i_i_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_uint_i_i_fc_Conv temp = new BasicSelectionTestClass_uint_i_i_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_uint_i_i_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_uint_i_i_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_uint_i_i_fc_Conv temp = new BasicSelectionTestClass_uint_i_i_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_uint_i_i_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_uint_i_i_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_uint_i_i_fc_Conv temp = new BasicSelectionTestClass_uint_i_i_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_uint_i_i_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_uint_i_i_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_uint_i_i_fc_Conv temp = new BasicSelectionTestClass_uint_i_i_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_uint_i_i_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_uint_i_i_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_uint_i_i_fc_Conv temp = new BasicSelectionTestClass_uint_i_i_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_uint_i_i_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_uint_i_i_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_uint_i_i_fc_Conv temp = new BasicSelectionTestClass_uint_i_i_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_uint_i_i_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_uint_i_i_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_uint_i_i_fc_Conv temp = new BasicSelectionTestClass_uint_i_i_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_uint_i_i_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_uint_i_i_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_uint_i_i_fc_Conv temp = new BasicSelectionTestClass_uint_i_i_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_uint_i_i_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_uint_i_i_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_uint_i_i_fc_Conv temp = new BasicSelectionTestClass_uint_i_i_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_uint_i_i_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_uint_i_i_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_uint_i_i_fc_Conv temp = new BasicSelectionTestClass_uint_i_i_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_uint_i_i_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_uint_i_i_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_uint_i_i_fc_Conv temp = new BasicSelectionTestClass_uint_i_i_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_uint_i_i_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_uint_i_i_fc_Conv cl = new BasicSelectionTestClass_uint_i_i_fc_Conv();
                cl.SetValueuint(1);
                uint value = cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_uint_i_e_fc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_uint_i_e_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_uint_i_e_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_uint_i_e_fc_Conv temp = new BasicSelectionTestClass_uint_i_e_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_uint_i_e_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_uint_i_e_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_uint_i_e_fc_Conv temp = new BasicSelectionTestClass_uint_i_e_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_uint_i_e_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_uint_i_e_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_uint_i_e_fc_Conv temp = new BasicSelectionTestClass_uint_i_e_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_uint_i_e_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_uint_i_e_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_uint_i_e_fc_Conv temp = new BasicSelectionTestClass_uint_i_e_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_uint_i_e_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_uint_i_e_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_uint_i_e_fc_Conv temp = new BasicSelectionTestClass_uint_i_e_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_uint_i_e_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_uint_i_e_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_uint_i_e_fc_Conv temp = new BasicSelectionTestClass_uint_i_e_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_uint_i_e_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_uint_i_e_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_uint_i_e_fc_Conv temp = new BasicSelectionTestClass_uint_i_e_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_uint_i_e_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_uint_i_e_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_uint_i_e_fc_Conv temp = new BasicSelectionTestClass_uint_i_e_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_uint_i_e_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_uint_i_e_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_uint_i_e_fc_Conv temp = new BasicSelectionTestClass_uint_i_e_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_uint_i_e_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_uint_i_e_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_uint_i_e_fc_Conv temp = new BasicSelectionTestClass_uint_i_e_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_uint_i_e_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_uint_i_e_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_uint_i_e_fc_Conv temp = new BasicSelectionTestClass_uint_i_e_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_uint_i_e_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_uint_i_e_fc_Conv cl = new BasicSelectionTestClass_uint_i_e_fc_Conv();
                cl.SetValueuint(1);
                uint value = (uint)cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_uint_e_e_fc_Conv
        {
            private byte value_byte;
            static public explicit operator byte(BasicSelectionTestClass_uint_e_e_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public explicit operator BasicSelectionTestClass_uint_e_e_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_uint_e_e_fc_Conv temp = new BasicSelectionTestClass_uint_e_e_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public explicit operator char(BasicSelectionTestClass_uint_e_e_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public explicit operator BasicSelectionTestClass_uint_e_e_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_uint_e_e_fc_Conv temp = new BasicSelectionTestClass_uint_e_e_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public explicit operator double(BasicSelectionTestClass_uint_e_e_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public explicit operator BasicSelectionTestClass_uint_e_e_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_uint_e_e_fc_Conv temp = new BasicSelectionTestClass_uint_e_e_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public explicit operator float(BasicSelectionTestClass_uint_e_e_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public explicit operator BasicSelectionTestClass_uint_e_e_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_uint_e_e_fc_Conv temp = new BasicSelectionTestClass_uint_e_e_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public explicit operator int(BasicSelectionTestClass_uint_e_e_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public explicit operator BasicSelectionTestClass_uint_e_e_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_uint_e_e_fc_Conv temp = new BasicSelectionTestClass_uint_e_e_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public explicit operator long(BasicSelectionTestClass_uint_e_e_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public explicit operator BasicSelectionTestClass_uint_e_e_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_uint_e_e_fc_Conv temp = new BasicSelectionTestClass_uint_e_e_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public explicit operator sbyte(BasicSelectionTestClass_uint_e_e_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public explicit operator BasicSelectionTestClass_uint_e_e_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_uint_e_e_fc_Conv temp = new BasicSelectionTestClass_uint_e_e_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public explicit operator short(BasicSelectionTestClass_uint_e_e_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public explicit operator BasicSelectionTestClass_uint_e_e_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_uint_e_e_fc_Conv temp = new BasicSelectionTestClass_uint_e_e_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public explicit operator uint(BasicSelectionTestClass_uint_e_e_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public explicit operator BasicSelectionTestClass_uint_e_e_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_uint_e_e_fc_Conv temp = new BasicSelectionTestClass_uint_e_e_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public explicit operator ulong(BasicSelectionTestClass_uint_e_e_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public explicit operator BasicSelectionTestClass_uint_e_e_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_uint_e_e_fc_Conv temp = new BasicSelectionTestClass_uint_e_e_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public explicit operator ushort(BasicSelectionTestClass_uint_e_e_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public explicit operator BasicSelectionTestClass_uint_e_e_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_uint_e_e_fc_Conv temp = new BasicSelectionTestClass_uint_e_e_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_uint_e_e_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_uint_e_e_fc_Conv cl = new BasicSelectionTestClass_uint_e_e_fc_Conv();
                cl.SetValueuint(1);
                uint value = (uint)cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_ulong_i_i_tc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_ulong_i_i_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_ulong_i_i_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_ulong_i_i_tc_Conv temp = new BasicSelectionTestClass_ulong_i_i_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_ulong_i_i_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_ulong_i_i_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_ulong_i_i_tc_Conv temp = new BasicSelectionTestClass_ulong_i_i_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_ulong_i_i_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_ulong_i_i_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_ulong_i_i_tc_Conv temp = new BasicSelectionTestClass_ulong_i_i_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_ulong_i_i_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_ulong_i_i_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_ulong_i_i_tc_Conv temp = new BasicSelectionTestClass_ulong_i_i_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_ulong_i_i_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_ulong_i_i_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_ulong_i_i_tc_Conv temp = new BasicSelectionTestClass_ulong_i_i_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_ulong_i_i_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_ulong_i_i_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_ulong_i_i_tc_Conv temp = new BasicSelectionTestClass_ulong_i_i_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_ulong_i_i_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_ulong_i_i_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_ulong_i_i_tc_Conv temp = new BasicSelectionTestClass_ulong_i_i_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_ulong_i_i_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_ulong_i_i_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_ulong_i_i_tc_Conv temp = new BasicSelectionTestClass_ulong_i_i_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_ulong_i_i_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_ulong_i_i_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_ulong_i_i_tc_Conv temp = new BasicSelectionTestClass_ulong_i_i_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_ulong_i_i_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_ulong_i_i_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_ulong_i_i_tc_Conv temp = new BasicSelectionTestClass_ulong_i_i_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_ulong_i_i_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_ulong_i_i_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_ulong_i_i_tc_Conv temp = new BasicSelectionTestClass_ulong_i_i_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_ulong_i_i_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_ulong_i_i_tc_Conv cl = new BasicSelectionTestClass_ulong_i_i_tc_Conv();
                ulong val = 1;
                cl = val;
                if (cl.GetValueulong() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_ulong_i_e_tc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_ulong_i_e_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_ulong_i_e_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_ulong_i_e_tc_Conv temp = new BasicSelectionTestClass_ulong_i_e_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_ulong_i_e_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_ulong_i_e_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_ulong_i_e_tc_Conv temp = new BasicSelectionTestClass_ulong_i_e_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_ulong_i_e_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_ulong_i_e_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_ulong_i_e_tc_Conv temp = new BasicSelectionTestClass_ulong_i_e_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_ulong_i_e_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_ulong_i_e_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_ulong_i_e_tc_Conv temp = new BasicSelectionTestClass_ulong_i_e_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_ulong_i_e_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_ulong_i_e_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_ulong_i_e_tc_Conv temp = new BasicSelectionTestClass_ulong_i_e_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_ulong_i_e_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_ulong_i_e_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_ulong_i_e_tc_Conv temp = new BasicSelectionTestClass_ulong_i_e_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_ulong_i_e_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_ulong_i_e_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_ulong_i_e_tc_Conv temp = new BasicSelectionTestClass_ulong_i_e_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_ulong_i_e_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_ulong_i_e_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_ulong_i_e_tc_Conv temp = new BasicSelectionTestClass_ulong_i_e_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_ulong_i_e_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_ulong_i_e_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_ulong_i_e_tc_Conv temp = new BasicSelectionTestClass_ulong_i_e_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_ulong_i_e_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_ulong_i_e_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_ulong_i_e_tc_Conv temp = new BasicSelectionTestClass_ulong_i_e_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_ulong_i_e_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_ulong_i_e_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_ulong_i_e_tc_Conv temp = new BasicSelectionTestClass_ulong_i_e_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_ulong_i_e_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_ulong_i_e_tc_Conv cl = new BasicSelectionTestClass_ulong_i_e_tc_Conv();
                ulong val = 1;
                cl = (BasicSelectionTestClass_ulong_i_e_tc_Conv)val;
                if (cl.GetValueulong() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_ulong_e_e_tc_Conv
        {
            private byte value_byte;
            static public explicit operator byte(BasicSelectionTestClass_ulong_e_e_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public explicit operator BasicSelectionTestClass_ulong_e_e_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_ulong_e_e_tc_Conv temp = new BasicSelectionTestClass_ulong_e_e_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public explicit operator char(BasicSelectionTestClass_ulong_e_e_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public explicit operator BasicSelectionTestClass_ulong_e_e_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_ulong_e_e_tc_Conv temp = new BasicSelectionTestClass_ulong_e_e_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public explicit operator double(BasicSelectionTestClass_ulong_e_e_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public explicit operator BasicSelectionTestClass_ulong_e_e_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_ulong_e_e_tc_Conv temp = new BasicSelectionTestClass_ulong_e_e_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public explicit operator float(BasicSelectionTestClass_ulong_e_e_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public explicit operator BasicSelectionTestClass_ulong_e_e_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_ulong_e_e_tc_Conv temp = new BasicSelectionTestClass_ulong_e_e_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public explicit operator int(BasicSelectionTestClass_ulong_e_e_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public explicit operator BasicSelectionTestClass_ulong_e_e_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_ulong_e_e_tc_Conv temp = new BasicSelectionTestClass_ulong_e_e_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public explicit operator long(BasicSelectionTestClass_ulong_e_e_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public explicit operator BasicSelectionTestClass_ulong_e_e_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_ulong_e_e_tc_Conv temp = new BasicSelectionTestClass_ulong_e_e_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public explicit operator sbyte(BasicSelectionTestClass_ulong_e_e_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public explicit operator BasicSelectionTestClass_ulong_e_e_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_ulong_e_e_tc_Conv temp = new BasicSelectionTestClass_ulong_e_e_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public explicit operator short(BasicSelectionTestClass_ulong_e_e_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public explicit operator BasicSelectionTestClass_ulong_e_e_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_ulong_e_e_tc_Conv temp = new BasicSelectionTestClass_ulong_e_e_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public explicit operator uint(BasicSelectionTestClass_ulong_e_e_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public explicit operator BasicSelectionTestClass_ulong_e_e_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_ulong_e_e_tc_Conv temp = new BasicSelectionTestClass_ulong_e_e_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public explicit operator ulong(BasicSelectionTestClass_ulong_e_e_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public explicit operator BasicSelectionTestClass_ulong_e_e_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_ulong_e_e_tc_Conv temp = new BasicSelectionTestClass_ulong_e_e_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public explicit operator ushort(BasicSelectionTestClass_ulong_e_e_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public explicit operator BasicSelectionTestClass_ulong_e_e_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_ulong_e_e_tc_Conv temp = new BasicSelectionTestClass_ulong_e_e_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_ulong_e_e_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_ulong_e_e_tc_Conv cl = new BasicSelectionTestClass_ulong_e_e_tc_Conv();
                ulong val = 1;
                cl = (BasicSelectionTestClass_ulong_e_e_tc_Conv)val;
                if (cl.GetValueulong() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_ulong_i_i_fc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_ulong_i_i_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_ulong_i_i_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_ulong_i_i_fc_Conv temp = new BasicSelectionTestClass_ulong_i_i_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_ulong_i_i_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_ulong_i_i_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_ulong_i_i_fc_Conv temp = new BasicSelectionTestClass_ulong_i_i_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_ulong_i_i_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_ulong_i_i_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_ulong_i_i_fc_Conv temp = new BasicSelectionTestClass_ulong_i_i_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_ulong_i_i_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_ulong_i_i_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_ulong_i_i_fc_Conv temp = new BasicSelectionTestClass_ulong_i_i_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_ulong_i_i_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_ulong_i_i_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_ulong_i_i_fc_Conv temp = new BasicSelectionTestClass_ulong_i_i_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_ulong_i_i_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_ulong_i_i_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_ulong_i_i_fc_Conv temp = new BasicSelectionTestClass_ulong_i_i_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_ulong_i_i_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_ulong_i_i_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_ulong_i_i_fc_Conv temp = new BasicSelectionTestClass_ulong_i_i_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_ulong_i_i_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_ulong_i_i_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_ulong_i_i_fc_Conv temp = new BasicSelectionTestClass_ulong_i_i_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_ulong_i_i_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_ulong_i_i_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_ulong_i_i_fc_Conv temp = new BasicSelectionTestClass_ulong_i_i_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_ulong_i_i_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_ulong_i_i_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_ulong_i_i_fc_Conv temp = new BasicSelectionTestClass_ulong_i_i_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_ulong_i_i_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_ulong_i_i_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_ulong_i_i_fc_Conv temp = new BasicSelectionTestClass_ulong_i_i_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_ulong_i_i_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_ulong_i_i_fc_Conv cl = new BasicSelectionTestClass_ulong_i_i_fc_Conv();
                cl.SetValueulong(1);
                ulong value = cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_ulong_i_e_fc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_ulong_i_e_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_ulong_i_e_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_ulong_i_e_fc_Conv temp = new BasicSelectionTestClass_ulong_i_e_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_ulong_i_e_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_ulong_i_e_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_ulong_i_e_fc_Conv temp = new BasicSelectionTestClass_ulong_i_e_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_ulong_i_e_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_ulong_i_e_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_ulong_i_e_fc_Conv temp = new BasicSelectionTestClass_ulong_i_e_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_ulong_i_e_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_ulong_i_e_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_ulong_i_e_fc_Conv temp = new BasicSelectionTestClass_ulong_i_e_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_ulong_i_e_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_ulong_i_e_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_ulong_i_e_fc_Conv temp = new BasicSelectionTestClass_ulong_i_e_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_ulong_i_e_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_ulong_i_e_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_ulong_i_e_fc_Conv temp = new BasicSelectionTestClass_ulong_i_e_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_ulong_i_e_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_ulong_i_e_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_ulong_i_e_fc_Conv temp = new BasicSelectionTestClass_ulong_i_e_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_ulong_i_e_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_ulong_i_e_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_ulong_i_e_fc_Conv temp = new BasicSelectionTestClass_ulong_i_e_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_ulong_i_e_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_ulong_i_e_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_ulong_i_e_fc_Conv temp = new BasicSelectionTestClass_ulong_i_e_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_ulong_i_e_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_ulong_i_e_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_ulong_i_e_fc_Conv temp = new BasicSelectionTestClass_ulong_i_e_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_ulong_i_e_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_ulong_i_e_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_ulong_i_e_fc_Conv temp = new BasicSelectionTestClass_ulong_i_e_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_ulong_i_e_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_ulong_i_e_fc_Conv cl = new BasicSelectionTestClass_ulong_i_e_fc_Conv();
                cl.SetValueulong(1);
                ulong value = (ulong)cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_ulong_e_e_fc_Conv
        {
            private byte value_byte;
            static public explicit operator byte(BasicSelectionTestClass_ulong_e_e_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public explicit operator BasicSelectionTestClass_ulong_e_e_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_ulong_e_e_fc_Conv temp = new BasicSelectionTestClass_ulong_e_e_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public explicit operator char(BasicSelectionTestClass_ulong_e_e_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public explicit operator BasicSelectionTestClass_ulong_e_e_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_ulong_e_e_fc_Conv temp = new BasicSelectionTestClass_ulong_e_e_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public explicit operator double(BasicSelectionTestClass_ulong_e_e_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public explicit operator BasicSelectionTestClass_ulong_e_e_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_ulong_e_e_fc_Conv temp = new BasicSelectionTestClass_ulong_e_e_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public explicit operator float(BasicSelectionTestClass_ulong_e_e_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public explicit operator BasicSelectionTestClass_ulong_e_e_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_ulong_e_e_fc_Conv temp = new BasicSelectionTestClass_ulong_e_e_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public explicit operator int(BasicSelectionTestClass_ulong_e_e_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public explicit operator BasicSelectionTestClass_ulong_e_e_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_ulong_e_e_fc_Conv temp = new BasicSelectionTestClass_ulong_e_e_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public explicit operator long(BasicSelectionTestClass_ulong_e_e_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public explicit operator BasicSelectionTestClass_ulong_e_e_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_ulong_e_e_fc_Conv temp = new BasicSelectionTestClass_ulong_e_e_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public explicit operator sbyte(BasicSelectionTestClass_ulong_e_e_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public explicit operator BasicSelectionTestClass_ulong_e_e_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_ulong_e_e_fc_Conv temp = new BasicSelectionTestClass_ulong_e_e_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public explicit operator short(BasicSelectionTestClass_ulong_e_e_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public explicit operator BasicSelectionTestClass_ulong_e_e_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_ulong_e_e_fc_Conv temp = new BasicSelectionTestClass_ulong_e_e_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public explicit operator uint(BasicSelectionTestClass_ulong_e_e_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public explicit operator BasicSelectionTestClass_ulong_e_e_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_ulong_e_e_fc_Conv temp = new BasicSelectionTestClass_ulong_e_e_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public explicit operator ulong(BasicSelectionTestClass_ulong_e_e_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public explicit operator BasicSelectionTestClass_ulong_e_e_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_ulong_e_e_fc_Conv temp = new BasicSelectionTestClass_ulong_e_e_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public explicit operator ushort(BasicSelectionTestClass_ulong_e_e_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public explicit operator BasicSelectionTestClass_ulong_e_e_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_ulong_e_e_fc_Conv temp = new BasicSelectionTestClass_ulong_e_e_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_ulong_e_e_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_ulong_e_e_fc_Conv cl = new BasicSelectionTestClass_ulong_e_e_fc_Conv();
                cl.SetValueulong(1);
                ulong value = (ulong)cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_ushort_i_i_tc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_ushort_i_i_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_ushort_i_i_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_ushort_i_i_tc_Conv temp = new BasicSelectionTestClass_ushort_i_i_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_ushort_i_i_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_ushort_i_i_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_ushort_i_i_tc_Conv temp = new BasicSelectionTestClass_ushort_i_i_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_ushort_i_i_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_ushort_i_i_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_ushort_i_i_tc_Conv temp = new BasicSelectionTestClass_ushort_i_i_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_ushort_i_i_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_ushort_i_i_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_ushort_i_i_tc_Conv temp = new BasicSelectionTestClass_ushort_i_i_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_ushort_i_i_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_ushort_i_i_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_ushort_i_i_tc_Conv temp = new BasicSelectionTestClass_ushort_i_i_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_ushort_i_i_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_ushort_i_i_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_ushort_i_i_tc_Conv temp = new BasicSelectionTestClass_ushort_i_i_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_ushort_i_i_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_ushort_i_i_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_ushort_i_i_tc_Conv temp = new BasicSelectionTestClass_ushort_i_i_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_ushort_i_i_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_ushort_i_i_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_ushort_i_i_tc_Conv temp = new BasicSelectionTestClass_ushort_i_i_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_ushort_i_i_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_ushort_i_i_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_ushort_i_i_tc_Conv temp = new BasicSelectionTestClass_ushort_i_i_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_ushort_i_i_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_ushort_i_i_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_ushort_i_i_tc_Conv temp = new BasicSelectionTestClass_ushort_i_i_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_ushort_i_i_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_ushort_i_i_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_ushort_i_i_tc_Conv temp = new BasicSelectionTestClass_ushort_i_i_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_ushort_i_i_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_ushort_i_i_tc_Conv cl = new BasicSelectionTestClass_ushort_i_i_tc_Conv();
                ushort val = 1;
                cl = val;
                if (cl.GetValueushort() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_ushort_i_e_tc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_ushort_i_e_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_ushort_i_e_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_ushort_i_e_tc_Conv temp = new BasicSelectionTestClass_ushort_i_e_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_ushort_i_e_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_ushort_i_e_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_ushort_i_e_tc_Conv temp = new BasicSelectionTestClass_ushort_i_e_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_ushort_i_e_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_ushort_i_e_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_ushort_i_e_tc_Conv temp = new BasicSelectionTestClass_ushort_i_e_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_ushort_i_e_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_ushort_i_e_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_ushort_i_e_tc_Conv temp = new BasicSelectionTestClass_ushort_i_e_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_ushort_i_e_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_ushort_i_e_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_ushort_i_e_tc_Conv temp = new BasicSelectionTestClass_ushort_i_e_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_ushort_i_e_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_ushort_i_e_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_ushort_i_e_tc_Conv temp = new BasicSelectionTestClass_ushort_i_e_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_ushort_i_e_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_ushort_i_e_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_ushort_i_e_tc_Conv temp = new BasicSelectionTestClass_ushort_i_e_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_ushort_i_e_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_ushort_i_e_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_ushort_i_e_tc_Conv temp = new BasicSelectionTestClass_ushort_i_e_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_ushort_i_e_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_ushort_i_e_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_ushort_i_e_tc_Conv temp = new BasicSelectionTestClass_ushort_i_e_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_ushort_i_e_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_ushort_i_e_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_ushort_i_e_tc_Conv temp = new BasicSelectionTestClass_ushort_i_e_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_ushort_i_e_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_ushort_i_e_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_ushort_i_e_tc_Conv temp = new BasicSelectionTestClass_ushort_i_e_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_ushort_i_e_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_ushort_i_e_tc_Conv cl = new BasicSelectionTestClass_ushort_i_e_tc_Conv();
                ushort val = 1;
                cl = (BasicSelectionTestClass_ushort_i_e_tc_Conv)val;
                if (cl.GetValueushort() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_ushort_e_e_tc_Conv
        {
            private byte value_byte;
            static public explicit operator byte(BasicSelectionTestClass_ushort_e_e_tc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public explicit operator BasicSelectionTestClass_ushort_e_e_tc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_ushort_e_e_tc_Conv temp = new BasicSelectionTestClass_ushort_e_e_tc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public explicit operator char(BasicSelectionTestClass_ushort_e_e_tc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public explicit operator BasicSelectionTestClass_ushort_e_e_tc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_ushort_e_e_tc_Conv temp = new BasicSelectionTestClass_ushort_e_e_tc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public explicit operator double(BasicSelectionTestClass_ushort_e_e_tc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public explicit operator BasicSelectionTestClass_ushort_e_e_tc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_ushort_e_e_tc_Conv temp = new BasicSelectionTestClass_ushort_e_e_tc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public explicit operator float(BasicSelectionTestClass_ushort_e_e_tc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public explicit operator BasicSelectionTestClass_ushort_e_e_tc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_ushort_e_e_tc_Conv temp = new BasicSelectionTestClass_ushort_e_e_tc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public explicit operator int(BasicSelectionTestClass_ushort_e_e_tc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public explicit operator BasicSelectionTestClass_ushort_e_e_tc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_ushort_e_e_tc_Conv temp = new BasicSelectionTestClass_ushort_e_e_tc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public explicit operator long(BasicSelectionTestClass_ushort_e_e_tc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public explicit operator BasicSelectionTestClass_ushort_e_e_tc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_ushort_e_e_tc_Conv temp = new BasicSelectionTestClass_ushort_e_e_tc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public explicit operator sbyte(BasicSelectionTestClass_ushort_e_e_tc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public explicit operator BasicSelectionTestClass_ushort_e_e_tc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_ushort_e_e_tc_Conv temp = new BasicSelectionTestClass_ushort_e_e_tc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public explicit operator short(BasicSelectionTestClass_ushort_e_e_tc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public explicit operator BasicSelectionTestClass_ushort_e_e_tc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_ushort_e_e_tc_Conv temp = new BasicSelectionTestClass_ushort_e_e_tc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public explicit operator uint(BasicSelectionTestClass_ushort_e_e_tc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public explicit operator BasicSelectionTestClass_ushort_e_e_tc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_ushort_e_e_tc_Conv temp = new BasicSelectionTestClass_ushort_e_e_tc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public explicit operator ulong(BasicSelectionTestClass_ushort_e_e_tc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public explicit operator BasicSelectionTestClass_ushort_e_e_tc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_ushort_e_e_tc_Conv temp = new BasicSelectionTestClass_ushort_e_e_tc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public explicit operator ushort(BasicSelectionTestClass_ushort_e_e_tc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public explicit operator BasicSelectionTestClass_ushort_e_e_tc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_ushort_e_e_tc_Conv temp = new BasicSelectionTestClass_ushort_e_e_tc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_ushort_e_e_tc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_ushort_e_e_tc_Conv cl = new BasicSelectionTestClass_ushort_e_e_tc_Conv();
                ushort val = 1;
                cl = (BasicSelectionTestClass_ushort_e_e_tc_Conv)val;
                if (cl.GetValueushort() != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_ushort_i_i_fc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_ushort_i_i_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_ushort_i_i_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_ushort_i_i_fc_Conv temp = new BasicSelectionTestClass_ushort_i_i_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_ushort_i_i_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_ushort_i_i_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_ushort_i_i_fc_Conv temp = new BasicSelectionTestClass_ushort_i_i_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_ushort_i_i_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_ushort_i_i_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_ushort_i_i_fc_Conv temp = new BasicSelectionTestClass_ushort_i_i_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_ushort_i_i_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_ushort_i_i_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_ushort_i_i_fc_Conv temp = new BasicSelectionTestClass_ushort_i_i_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_ushort_i_i_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_ushort_i_i_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_ushort_i_i_fc_Conv temp = new BasicSelectionTestClass_ushort_i_i_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_ushort_i_i_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_ushort_i_i_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_ushort_i_i_fc_Conv temp = new BasicSelectionTestClass_ushort_i_i_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_ushort_i_i_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_ushort_i_i_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_ushort_i_i_fc_Conv temp = new BasicSelectionTestClass_ushort_i_i_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_ushort_i_i_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_ushort_i_i_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_ushort_i_i_fc_Conv temp = new BasicSelectionTestClass_ushort_i_i_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_ushort_i_i_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_ushort_i_i_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_ushort_i_i_fc_Conv temp = new BasicSelectionTestClass_ushort_i_i_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_ushort_i_i_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_ushort_i_i_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_ushort_i_i_fc_Conv temp = new BasicSelectionTestClass_ushort_i_i_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_ushort_i_i_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_ushort_i_i_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_ushort_i_i_fc_Conv temp = new BasicSelectionTestClass_ushort_i_i_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_ushort_i_i_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_ushort_i_i_fc_Conv cl = new BasicSelectionTestClass_ushort_i_i_fc_Conv();
                cl.SetValueushort(1);
                ushort value = cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_ushort_i_e_fc_Conv
        {
            private byte value_byte;
            static public implicit operator byte(BasicSelectionTestClass_ushort_i_e_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public implicit operator BasicSelectionTestClass_ushort_i_e_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_ushort_i_e_fc_Conv temp = new BasicSelectionTestClass_ushort_i_e_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public implicit operator char(BasicSelectionTestClass_ushort_i_e_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public implicit operator BasicSelectionTestClass_ushort_i_e_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_ushort_i_e_fc_Conv temp = new BasicSelectionTestClass_ushort_i_e_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public implicit operator double(BasicSelectionTestClass_ushort_i_e_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public implicit operator BasicSelectionTestClass_ushort_i_e_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_ushort_i_e_fc_Conv temp = new BasicSelectionTestClass_ushort_i_e_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public implicit operator float(BasicSelectionTestClass_ushort_i_e_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public implicit operator BasicSelectionTestClass_ushort_i_e_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_ushort_i_e_fc_Conv temp = new BasicSelectionTestClass_ushort_i_e_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public implicit operator int(BasicSelectionTestClass_ushort_i_e_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public implicit operator BasicSelectionTestClass_ushort_i_e_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_ushort_i_e_fc_Conv temp = new BasicSelectionTestClass_ushort_i_e_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public implicit operator long(BasicSelectionTestClass_ushort_i_e_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public implicit operator BasicSelectionTestClass_ushort_i_e_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_ushort_i_e_fc_Conv temp = new BasicSelectionTestClass_ushort_i_e_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public implicit operator sbyte(BasicSelectionTestClass_ushort_i_e_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public implicit operator BasicSelectionTestClass_ushort_i_e_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_ushort_i_e_fc_Conv temp = new BasicSelectionTestClass_ushort_i_e_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public implicit operator short(BasicSelectionTestClass_ushort_i_e_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public implicit operator BasicSelectionTestClass_ushort_i_e_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_ushort_i_e_fc_Conv temp = new BasicSelectionTestClass_ushort_i_e_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public implicit operator uint(BasicSelectionTestClass_ushort_i_e_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public implicit operator BasicSelectionTestClass_ushort_i_e_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_ushort_i_e_fc_Conv temp = new BasicSelectionTestClass_ushort_i_e_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public implicit operator ulong(BasicSelectionTestClass_ushort_i_e_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public implicit operator BasicSelectionTestClass_ushort_i_e_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_ushort_i_e_fc_Conv temp = new BasicSelectionTestClass_ushort_i_e_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public implicit operator ushort(BasicSelectionTestClass_ushort_i_e_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public implicit operator BasicSelectionTestClass_ushort_i_e_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_ushort_i_e_fc_Conv temp = new BasicSelectionTestClass_ushort_i_e_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_ushort_i_e_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_ushort_i_e_fc_Conv cl = new BasicSelectionTestClass_ushort_i_e_fc_Conv();
                cl.SetValueushort(1);
                ushort value = (ushort)cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        class BasicSelectionTestClass_ushort_e_e_fc_Conv
        {
            private byte value_byte;
            static public explicit operator byte(BasicSelectionTestClass_ushort_e_e_fc_Conv test)
            { Log.Comment("To byte"); return test.value_byte; }
            static public explicit operator BasicSelectionTestClass_ushort_e_e_fc_Conv(byte val)
            { Log.Comment("From byte"); BasicSelectionTestClass_ushort_e_e_fc_Conv temp = new BasicSelectionTestClass_ushort_e_e_fc_Conv(); temp.value_byte = val; return (temp); }
            public byte GetValuebyte()
            { return (value_byte); }
            public void SetValuebyte(byte val)
            { value_byte = val; }
            private char value_char;
            static public explicit operator char(BasicSelectionTestClass_ushort_e_e_fc_Conv test)
            { Log.Comment("To char"); return test.value_char; }
            static public explicit operator BasicSelectionTestClass_ushort_e_e_fc_Conv(char val)
            { Log.Comment("From char"); BasicSelectionTestClass_ushort_e_e_fc_Conv temp = new BasicSelectionTestClass_ushort_e_e_fc_Conv(); temp.value_char = val; return (temp); }
            public char GetValuechar()
            { return (value_char); }
            public void SetValuechar(char val)
            { value_char = val; }
            private double value_double;
            static public explicit operator double(BasicSelectionTestClass_ushort_e_e_fc_Conv test)
            { Log.Comment("To double"); return test.value_double; }
            static public explicit operator BasicSelectionTestClass_ushort_e_e_fc_Conv(double val)
            { Log.Comment("From double"); BasicSelectionTestClass_ushort_e_e_fc_Conv temp = new BasicSelectionTestClass_ushort_e_e_fc_Conv(); temp.value_double = val; return (temp); }
            public double GetValuedouble()
            { return (value_double); }
            public void SetValuedouble(double val)
            { value_double = val; }
            private float value_float;
            static public explicit operator float(BasicSelectionTestClass_ushort_e_e_fc_Conv test)
            { Log.Comment("To float"); return test.value_float; }
            static public explicit operator BasicSelectionTestClass_ushort_e_e_fc_Conv(float val)
            { Log.Comment("From float"); BasicSelectionTestClass_ushort_e_e_fc_Conv temp = new BasicSelectionTestClass_ushort_e_e_fc_Conv(); temp.value_float = val; return (temp); }
            public float GetValuefloat()
            { return (value_float); }
            public void SetValuefloat(float val)
            { value_float = val; }
            private int value_int;
            static public explicit operator int(BasicSelectionTestClass_ushort_e_e_fc_Conv test)
            { Log.Comment("To int"); return test.value_int; }
            static public explicit operator BasicSelectionTestClass_ushort_e_e_fc_Conv(int val)
            { Log.Comment("From int"); BasicSelectionTestClass_ushort_e_e_fc_Conv temp = new BasicSelectionTestClass_ushort_e_e_fc_Conv(); temp.value_int = val; return (temp); }
            public int GetValueint()
            { return (value_int); }
            public void SetValueint(int val)
            { value_int = val; }
            private long value_long;
            static public explicit operator long(BasicSelectionTestClass_ushort_e_e_fc_Conv test)
            { Log.Comment("To long"); return test.value_long; }
            static public explicit operator BasicSelectionTestClass_ushort_e_e_fc_Conv(long val)
            { Log.Comment("From long"); BasicSelectionTestClass_ushort_e_e_fc_Conv temp = new BasicSelectionTestClass_ushort_e_e_fc_Conv(); temp.value_long = val; return (temp); }
            public long GetValuelong()
            { return (value_long); }
            public void SetValuelong(long val)
            { value_long = val; }
            private sbyte value_sbyte;
            static public explicit operator sbyte(BasicSelectionTestClass_ushort_e_e_fc_Conv test)
            { Log.Comment("To sbyte"); return test.value_sbyte; }
            static public explicit operator BasicSelectionTestClass_ushort_e_e_fc_Conv(sbyte val)
            { Log.Comment("From sbyte"); BasicSelectionTestClass_ushort_e_e_fc_Conv temp = new BasicSelectionTestClass_ushort_e_e_fc_Conv(); temp.value_sbyte = val; return (temp); }
            public sbyte GetValuesbyte()
            { return (value_sbyte); }
            public void SetValuesbyte(sbyte val)
            { value_sbyte = val; }
            private short value_short;
            static public explicit operator short(BasicSelectionTestClass_ushort_e_e_fc_Conv test)
            { Log.Comment("To short"); return test.value_short; }
            static public explicit operator BasicSelectionTestClass_ushort_e_e_fc_Conv(short val)
            { Log.Comment("From short"); BasicSelectionTestClass_ushort_e_e_fc_Conv temp = new BasicSelectionTestClass_ushort_e_e_fc_Conv(); temp.value_short = val; return (temp); }
            public short GetValueshort()
            { return (value_short); }
            public void SetValueshort(short val)
            { value_short = val; }
            private uint value_uint;
            static public explicit operator uint(BasicSelectionTestClass_ushort_e_e_fc_Conv test)
            { Log.Comment("To uint"); return test.value_uint; }
            static public explicit operator BasicSelectionTestClass_ushort_e_e_fc_Conv(uint val)
            { Log.Comment("From uint"); BasicSelectionTestClass_ushort_e_e_fc_Conv temp = new BasicSelectionTestClass_ushort_e_e_fc_Conv(); temp.value_uint = val; return (temp); }
            public uint GetValueuint()
            { return (value_uint); }
            public void SetValueuint(uint val)
            { value_uint = val; }
            private ulong value_ulong;
            static public explicit operator ulong(BasicSelectionTestClass_ushort_e_e_fc_Conv test)
            { Log.Comment("To ulong"); return test.value_ulong; }
            static public explicit operator BasicSelectionTestClass_ushort_e_e_fc_Conv(ulong val)
            { Log.Comment("From ulong"); BasicSelectionTestClass_ushort_e_e_fc_Conv temp = new BasicSelectionTestClass_ushort_e_e_fc_Conv(); temp.value_ulong = val; return (temp); }
            public ulong GetValueulong()
            { return (value_ulong); }
            public void SetValueulong(ulong val)
            { value_ulong = val; }
            private ushort value_ushort;
            static public explicit operator ushort(BasicSelectionTestClass_ushort_e_e_fc_Conv test)
            { Log.Comment("To ushort"); return test.value_ushort; }
            static public explicit operator BasicSelectionTestClass_ushort_e_e_fc_Conv(ushort val)
            { Log.Comment("From ushort"); BasicSelectionTestClass_ushort_e_e_fc_Conv temp = new BasicSelectionTestClass_ushort_e_e_fc_Conv(); temp.value_ushort = val; return (temp); }
            public ushort GetValueushort()
            { return (value_ushort); }
            public void SetValueushort(ushort val)
            { value_ushort = val; }
        }
        class BasicSelectionTestClass_ushort_e_e_fc
        {
            public static int Main_old()
            {
                BasicSelectionTestClass_ushort_e_e_fc_Conv cl = new BasicSelectionTestClass_ushort_e_e_fc_Conv();
                cl.SetValueushort(1);
                ushort value = (ushort)cl;
                if (value != 1)
                    return 1;
                else
                    return 0;
            }
            public static bool testMethod()
            {
                
                return (Main_old() == 0);
            }
        }
        
    }
}
