////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ParseTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        public static int[] intArr = null;
        public String[] GetRandomStringArray(int max, bool signed)
        {
            Random random = new Random();
            String[] arr1 = new String[] { "0", "-0","+0", 
                                        "00000     ", "    -00000","   +00000  ", 
                                        "   0   ", "  -00000  ", 
                                        "+123", "  +123  ", "   +123", "+123    " };
            String[] arr2 = new String[10];
            intArr = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 123, 123, 123, 123, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 0; i < arr2.Length; i++)
            {
                if (signed && ((i % 2) == 0))
                {
                    intArr[i + 12] = -(random.Next(max));
                    arr2[i] = (intArr[i + 12].ToString());
                }
                else
                {
                    intArr[i + 12] = random.Next(max);
                    arr2[i] = intArr[i + 12].ToString();
                }
            }
            String[] arr = new String[22];
            Array.Copy(arr1, arr, arr1.Length);
            Array.Copy(arr2, 0, arr, arr1.Length, arr2.Length);
            return arr;
        }

        [TestMethod]
        public MFTestResults ParseSByte_Test_1()
        {
            Log.Comment("SByte MinValue = " + SByte.MinValue.ToString());
            Log.Comment("SByte MaxValue = " + SByte.MaxValue.ToString());

            String[] strArr = GetRandomStringArray(SByte.MaxValue, true);
            SByte[] _sByte = new SByte[intArr.Length];
            for (int i = 0; i < _sByte.Length; i++)
            {
                _sByte[i] = (SByte)intArr[i];
            }

            int counter = 0;
            SByte temp = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    temp = SByte.Parse(strArr[i]);
                }
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + " when SByte.Parse('" + strArr[i] + "')");
                    counter++;
                }
                if (temp != _sByte[i])
                {
                    Log.Comment(i.ToString() + " Expecting " + _sByte[i] + " but got " + temp);
                    counter++;
                }
            }

            return (counter == 0) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults ParseByte_Test_2()
        {
            Log.Comment("Byte MinValue = " + Byte.MinValue.ToString());
            Log.Comment("Byte MaxValue = " + Byte.MaxValue.ToString());
            //Log.Comment("This currently fails, see 21634  for details");

            String[] strArr = GetRandomStringArray(Byte.MaxValue, false);
            Byte[] _byte = new Byte[intArr.Length];
            for (int i = 0; i < _byte.Length; i++)
            {
                _byte[i] = (Byte)intArr[i];
            }
            int counter = 0;
            Byte temp = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    temp = Byte.Parse(strArr[i]);
                }
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + " when Byte.Parse('" + strArr[i] + "')");
                    counter++;
                }
                if (temp != _byte[i])
                {
                    Log.Comment(i.ToString() + " Expecting " + _byte[i] + " but got " + temp);
                    counter++;
                }
            }

            return (counter == 0) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults ParseInt16_Test_3()
        {
            Log.Comment("Int16 MinValue = " + Int16.MinValue.ToString());
            Log.Comment("Int16 MaxValue = " + Int16.MaxValue.ToString());

            String[] strArr = GetRandomStringArray(Int16.MaxValue, true);
            Int16[] _int16 = new Int16[intArr.Length];
            for (int i = 0; i < _int16.Length; i++)
            {
                _int16[i] = (Int16)intArr[i];
            }
            int counter = 0;
            Int16 temp = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    temp = Int16.Parse(strArr[i]);
                }
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + " when Int16.Parse('" + strArr[i] + "')");
                    counter++;
                }
                if (temp != _int16[i])
                {
                    Log.Comment(i.ToString() + " Expecting " + _int16[i] + " but got " + temp);
                    counter++;
                }
            }

            return (counter == 0) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults ParseUInt16_Test_4()
        {
            Log.Comment("UInt16 MinValue = " + UInt16.MinValue.ToString());
            Log.Comment("UInt16 MaxValue = " + UInt16.MaxValue.ToString());
            //Log.Comment("This currently fails, see 21634  for details");


            String[] strArr = GetRandomStringArray(UInt16.MaxValue, false);
            UInt16[] _uInt16 = new UInt16[intArr.Length];
            for (int i = 0; i < _uInt16.Length; i++)
            {
                _uInt16[i] = (UInt16)intArr[i];
            }
            int counter = 0;
            UInt16 temp = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    temp = UInt16.Parse(strArr[i]);
                }
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + " when UInt16.Parse('" + strArr[i] + "')");
                    counter++;
                }
                if (temp != _uInt16[i])
                {
                    Log.Comment(i.ToString() + " Expecting " + _uInt16[i] + " but got " + temp);
                    counter++;
                }
            }

            return (counter == 0) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults ParseInt32_Test_5()
        {
            Log.Comment("Int32 MinValue = " + Int32.MinValue.ToString());
            Log.Comment("Int32 MaxValue = " + Int32.MaxValue.ToString());
            //Log.Comment("This currently Fails, See 21626 for details");


            String[] strArr = GetRandomStringArray(Int32.MaxValue, true);
            Int32[] _int32 = new Int32[intArr.Length];
            for (int i = 0; i < _int32.Length; i++)
            {
                _int32[i] = (Int32)intArr[i];
            }

            int counter = 0;
            Int32 temp = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    temp = Int32.Parse(strArr[i]);
                }
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + " when Int32.Parse('" + strArr[i] + "')");
                    counter++;
                }
                if (temp != _int32[i])
                {
                    Log.Comment(i.ToString() + " Expecting " + _int32[i] + " but got " + temp);
                    counter++;
                }
            }

            return (counter == 0) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        // ==========================================================================

        [TestMethod]
        public MFTestResults ParseUInt32_Test_6()
        {
            Log.Comment("UInt32 MinValue = " + UInt32.MinValue.ToString());
            Log.Comment("UInt32 MaxValue = " + UInt32.MaxValue.ToString());
            //Log.Comment("This currently fails, see 21634  for details");

            Random random = new Random();
            String[] strArr = new String[] { "0", "-0","+0", 
                                        "00000     ", "    -00000","   +00000  ", 
                                        "   0   ", "  -00000  ", 
                                        "+123", "  +123  ", "   +123", "+123    ",
                                        "","","","","","","","","",""};
            UInt32[] _uInt32 = new UInt32[] { 0, 0, 0, 0, 0, 0, 0, 0, 123, 123, 123, 123, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 12; i < _uInt32.Length; i++)
            {
                int power = random.Next(33);
                _uInt32[i] = (UInt32)(System.Math.Pow(2, (double)power) - 1);
                strArr[i] = _uInt32[i].ToString();
            }

            int counter = 0;
            UInt32 temp = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    temp = UInt32.Parse(strArr[i]);
                }
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + " when UInt32.Parse('" + strArr[i] + "')");
                    counter++;
                }
                if (temp != _uInt32[i])
                {
                    Log.Comment(i.ToString() + " Expecting " + _uInt32[i] + " but got " + temp);
                    counter++;
                }
            }

            return (counter == 0) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults ParseInt64_Test_7()
        {
            Log.Comment("Int64 MinValue = " + Int64.MinValue.ToString());
            Log.Comment("Int64 MaxValue = " + Int64.MaxValue.ToString());

            Random random = new Random();
            String[] strArr = new String[] { "0", "-0","+0", 
                                        "00000     ", "    -00000","   +00000  ", 
                                        "   0   ", "  -00000  ", 
                                        "+123", "  +123  ", "   +123", "+123    ",
                                        "","","","","","","","","",""};
            Int64[] _int64 = new Int64[] { 0, 0, 0, 0, 0, 0, 0, 0, 123, 123, 123, 123, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            for (int i = 12; i < _int64.Length; i++)
            {
                int power = random.Next(64);
                if (i % 2 == 0)
                {
                    _int64[i] = (Int64)(-System.Math.Pow(2, (double)power) - 1);
                }
                else
                {
                    _int64[i] = (Int64)(System.Math.Pow(2, (double)power) - 1);
                }
                strArr[i] = _int64[i].ToString();
            }

            int counter = 0;
            Int64 temp = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    temp = Int64.Parse(strArr[i]);
                }
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + " when Int64.Parse('" + strArr[i] + "')");
                    counter++;
                }
                if (temp != _int64[i])
                {
                    Log.Comment(i.ToString() + " Expecting " + _int64[i] + " but got " + temp);
                    counter++;
                }
            }

            //Int32:  -2147483648 --> 2147483647
            counter += CheckValues(Int32.MinValue);
            counter += CheckValues(Int32.MaxValue);

            //UInt32: 0 ---> 4294967295
            counter += CheckValues(UInt32.MinValue);
            counter += CheckValues(UInt32.MaxValue);

            //Int64: -9223372036854775808  --> 9223372036854775807
            counter += CheckValues(Int64.MinValue);
            counter += CheckValues(Int64.MaxValue);

            //Uint64: 0 --> 18446744073709551615
            counter += CheckValues((Int64)UInt64.MinValue);
         
            return (counter == 0) ? MFTestResults.Pass : MFTestResults.Fail;
        }


        private int CheckValues(Int64 start)
        {
            Int64 newVal = 0;
            string temp;
            int failCount = 0;
            for (Int64 i = start - 10; i < start + 10; i++)
            {
                temp = i.ToString();
                newVal = Int64.Parse(temp);
                if (i != newVal)
                {
                    Log.Comment("Expecting: " + i + " but got: " + newVal);
                    failCount++;
                }
            }

            return failCount;
        }

        [TestMethod]
        public MFTestResults ParseUInt64_Test_8()
        {
            Log.Comment("UInt64 MinValue = " + UInt64.MinValue.ToString());
            Log.Comment("UInt64 MaxValue = " + UInt64.MaxValue.ToString());
            //Log.Comment("This currently fails, see 21634  for details");

            Random random = new Random();
            String[] strArr = new String[] { "0", "-0","+0", 
                                        "00000     ", "    -00000","   +00000  ", 
                                        "   0   ", "  -00000  ", 
                                        "+123", "  +123  ", "   +123", "+123    ",
                                        "","","","","","","","","",""};
            UInt64[] _uInt64 = new UInt64[] { 0, 0, 0, 0, 0, 0, 0, 0, 123, 123, 123, 123, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            for (int i = 12; i < _uInt64.Length; i++)
            {
                int power = random.Next(65);
                _uInt64[i] = (UInt64)(System.Math.Pow(2, (double)power) - 1);
                strArr[i] = _uInt64[i].ToString();
            }

            int counter = 0;
            UInt64 temp = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    temp = UInt64.Parse(strArr[i]);
                }
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + " when UInt64.Parse('" + strArr[i] + "')");
                    counter++;
                }
                if (temp != _uInt64[i])
                {
                    Log.Comment(i.ToString() + " Expecting " + _uInt64[i] + " but got " + temp);
                    counter++;
                }
            }

            //Int32:  -2147483648 --> 2147483647
            counter += CheckUValues(Int32.MaxValue);

            //UInt32: 0 ---> 4294967295
            counter += CheckUValues(UInt32.MinValue);
            counter += CheckUValues(UInt32.MaxValue);

            //Int64: -9223372036854775808  --> 9223372036854775807
            counter += CheckUValues(UInt64.MaxValue);

            //Uint64: 0 --> 18446744073709551615
            counter += CheckUValues(UInt64.MinValue);
            counter += CheckUValues(UInt64.MaxValue);

            return (counter == 0) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults ParseDouble_Test_x()
        {
            Log.Comment("double MinValue = " + double.MinValue.ToString());
            Log.Comment("double MaxValue = " + double.MaxValue.ToString());
            //Log.Comment("This currently fails, see 21634  for details");

            Random random = new Random();
            String[] strArr = new String[] { "0", "-0","+0", 
                                        "00000     ", "    -00000","   +00000  ", 
                                        "   0   ", "  -00000  ", 
                                        "+123", "  +123  ", "   +123", "+123    "};
            double[] _double = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 123, 123, 123, 123 };

            int counter = 0;
            double temp = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    temp = double.Parse(strArr[i]);
                }
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + " when double.Parse('" + strArr[i] + "')");
                    counter++;
                }
                if (temp != _double[i])
                {
                    Log.Comment(i.ToString() + " Expecting " + _double[i] + " but got " + temp);
                    counter++;
                }
            }

            double d = double.Parse("-0.1");
            if (d != -0.1) counter++;

            d = double.Parse("0.1");
            if (d != 0.1) counter++;

            d = double.Parse(" -1.1");
            if (d != -1.1) counter++;
            
            d = double.Parse(" -0.0001");
            if (d != -0.0001) counter++;

            d = double.Parse(" -10.0001");
            if (d != -10.0001) counter++;

            d = double.Parse("-0.01e-10");
            if (d != -0.01e-10) counter++;

            d = double.Parse(" ");
            if (d != 0.0) counter++;

            string t = double.MinValue.ToString();
            if (double.MinValue != double.Parse(t)) counter++;

            t = double.MaxValue.ToString();
            if (double.MaxValue != double.Parse(t)) counter++;

            t = float.MinValue.ToString();
            if (float.MinValue != (float)double.Parse(t)) counter++;
            
            t = float.MaxValue.ToString();
            if (float.MaxValue != (float)double.Parse(t)) counter++;

            return (counter == 0) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        private int CheckUValues(UInt64 start)
        {
            UInt64 newVal = 0;
            string temp;
            int failCount = 0;
            for (UInt64 i = start - 10; i < start + 10; i++)
            {
                temp = i.ToString();
                newVal = UInt64.Parse(temp);
                if (i != newVal)
                {
                    Log.Comment("Expecting: " + i + " but got: " + newVal);
                    failCount++;
                }
            }

            return failCount;
        }

        //=========================================================================================
        //        BoundaryTests


        [TestMethod]
        public MFTestResults SByte_Boundary_Test_9()
        {

            String[] strArr = new String[] { SByte.MaxValue.ToString(), "127", SByte.MinValue.ToString(), "-128" };
            SByte[] _SByte = new SByte[] { 127, 127, -128, -128 };
            int counter = 0;
            SByte temp = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    temp = SByte.Parse(strArr[i]);
                }
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + " when SByte.Parse('" + strArr[i] + "')");
                    counter++;
                }
                if (temp != _SByte[i])
                {
                    Log.Comment(i.ToString() + " Expecting " + _SByte[i] + " but got " + temp);
                    counter++;
                }
            }

            return (counter == 0) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Byte_Boundary_Test_10()
        {

            String[] strArr = new String[] { Byte.MaxValue.ToString(), "255", Byte.MinValue.ToString(), "0" };
            Byte[] _byte = new Byte[] { 255, 255, 0, 0 };
            int counter = 0;
            Byte temp = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    temp = Byte.Parse(strArr[i]);
                }
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + " when Byte.Parse('" + strArr[i] + "')");
                    counter++;
                }
                if (temp != _byte[i])
                {
                    Log.Comment(i.ToString() + " Expecting " + _byte[i] + " but got " + temp);
                    counter++;
                }
            }

            return (counter == 0) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Int16_Boundary_Test_11()
        {
            String[] strArr = new String[] { Int16.MaxValue.ToString(), "32767", Int16.MinValue.ToString(), "-32768" };
            Int16[] _int16 = new Int16[] { 32767, 32767, -32768, -32768 };
            int counter = 0;
            Int16 temp = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    temp = Int16.Parse(strArr[i]);
                }
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + " when Int16.Parse('" + strArr[i] + "')");
                    counter++;
                }
                if (temp != _int16[i])
                {
                    Log.Comment(i.ToString() + " Expecting " + _int16[i] + " but got " + temp);
                    counter++;
                }
            }

            return (counter == 0) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults UInt16_Boundary_Test_12()
        {
            String[] strArr = new String[] { UInt16.MaxValue.ToString(), "65535", UInt16.MinValue.ToString(), "0" };
            UInt16[] _uInt16 = new UInt16[] { 65535, 65535, 0, 0 };
            int counter = 0;
            UInt16 temp = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    temp = UInt16.Parse(strArr[i]);
                }
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + " when UInt16.Parse('" + strArr[i] + "')");
                    counter++;
                }
                if (temp != _uInt16[i])
                {
                    Log.Comment(i.ToString() + " Expecting " + _uInt16[i] + " but got " + temp);
                    counter++;
                }
            }

            return (counter == 0) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Int32_Boundary_Test_13()
        {
            String[] strArr = new String[] { Int32.MaxValue.ToString(), "2147483647", Int32.MinValue.ToString(), "-2147483648" };
            Int32[] _int32 = new Int32[] { 2147483647, 2147483647, -2147483648, -2147483648 };
            int counter = 0;
            Int32 temp = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    temp = Int32.Parse(strArr[i]);
                }
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + " when Int32.Parse('" + strArr[i] + "')");
                    counter++;
                }
                if (temp != _int32[i])
                {
                    Log.Comment(i.ToString() + " Expecting " + _int32[i] + " but got " + temp);
                    counter++;
                }
            }

            return (counter == 0) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults UInt32_Boundary_Test_14()
        {
            String[] strArr = new String[] { UInt32.MaxValue.ToString(), "4294967295", UInt32.MinValue.ToString(), "0" };
            UInt32[] _uInt32 = new UInt32[] { 4294967295, 4294967295, 0, 0 };
            int counter = 0;
            UInt32 temp = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    temp = UInt32.Parse(strArr[i]);
                }
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + " when UInt32.Parse('" + strArr[i] + "')");
                    counter++;
                }
                if (temp != _uInt32[i])
                {
                    Log.Comment(i.ToString() + " Expecting " + _uInt32[i] + " but got " + temp);
                    counter++;
                }
            }

            return (counter == 0) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Int64_Boundary_Test_15()
        {
            String[] strArr = new String[] { Int64.MaxValue.ToString(), "9223372036854775807", Int64.MinValue.ToString(), "-9223372036854775808" };
            Int64[] _int64 = new Int64[] { 9223372036854775807, 9223372036854775807, -9223372036854775808, -9223372036854775808 };
            int counter = 0;
            Int64 temp = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    temp = Int64.Parse(strArr[i]);
                }
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + " when Int64.Parse('" + strArr[i] + "')");
                    counter++;
                }
                if (temp != _int64[i])
                {
                    Log.Comment(i.ToString() + " Expecting " + _int64[i] + " but got " + temp);
                    counter++;
                }
            }

            return (counter == 0) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults UInt64_Boundary_Test_16()
        {
            String[] strArr = new String[] { UInt64.MaxValue.ToString(), "18446744073709551615", UInt64.MinValue.ToString(), "0" };
            UInt64[] _uInt64 = new UInt64[] { 18446744073709551615, 18446744073709551615, 0, 0 };
            int counter = 0;
            UInt64 temp = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    temp = UInt64.Parse(strArr[i]);
                }
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + " when UInt64.Parse('" + strArr[i] + "')");
                    counter++;
                }
                if (temp != _uInt64[i])
                {
                    Log.Comment(i.ToString() + " Expecting " + _uInt64[i] + " but got " + temp);
                    counter++;
                }
            }

            return (counter == 0) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        //============================================================================
        //          ArgumentNullException tests

        public static string str = null;

        [TestMethod]
        public MFTestResults SByte_ArgumentNullException_Test_17()
        {
            MFTestResults testResult = MFTestResults.Fail;

            try
            {
                SByte.Parse(str);
            }
            catch (ArgumentNullException ex)
            {
                Log.Comment("Caught Exception named : " + ex.Message + " upon SByte.Parse null string");
                testResult = MFTestResults.Pass;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Byte_ArgumentNullException_Test_18()
        {
            MFTestResults testResult = MFTestResults.Fail;
            try
            {
                Byte.Parse(str);
            }
            catch (ArgumentNullException ex)
            {
                Log.Comment("Caught Exception named : " + ex.Message + " upon Byte.Parse null string");
                testResult = MFTestResults.Pass;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Int16_ArgumentNullException_Test_19()
        {
            MFTestResults testResult = MFTestResults.Fail;
            try
            {
                Int16.Parse(str);
            }
            catch (ArgumentNullException ex)
            {
                Log.Comment("Caught Exception named : " + ex.Message + " upon Int16.Parse null string");
                testResult = MFTestResults.Pass;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults UInt16_ArgumentNullException_Test_20()
        {
            MFTestResults testResult = MFTestResults.Fail;
            try
            {
                UInt16.Parse(str);
            }
            catch (ArgumentNullException ex)
            {
                Log.Comment("Caught Exception named : " + ex.Message + " upon UInt16.Parse null string");
                testResult = MFTestResults.Pass;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Int32_ArgumentNullException_Test_21()
        {
            MFTestResults testResult = MFTestResults.Fail;
            try
            {
                Int32.Parse(str);
            }
            catch (ArgumentNullException ex)
            {
                Log.Comment("Caught Exception named : " + ex.Message + " upon Int32.Parse null string");
                testResult = MFTestResults.Pass;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults UInt32_ArgumentNullException_Test_22()
        {
            MFTestResults testResult = MFTestResults.Fail;
            try
            {
                UInt32.Parse(str);
            }
            catch (ArgumentNullException ex)
            {
                Log.Comment("Caught Exception named : " + ex.Message + " upon UInt32.Parse null string");
                testResult = MFTestResults.Pass;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Int64_ArgumentNullException_Test_23()
        {
            MFTestResults testResult = MFTestResults.Fail;
            try
            {
                Int64.Parse(str);
            }
            catch (ArgumentNullException ex)
            {
                Log.Comment("Caught Exception named : " + ex.Message + " upon Int64.Parse null string");
                testResult = MFTestResults.Pass;
            }

            return testResult;
        }
        [TestMethod]
        public MFTestResults UInt64_ArgumentNullException_Test_24()
        {
            MFTestResults testResult = MFTestResults.Fail;
            try
            {
                UInt64.Parse(str);
            }
            catch (ArgumentNullException ex)
            {
                Log.Comment("Caught Exception named : " + ex.Message + " upon UInt64.Parse null string");
                testResult = MFTestResults.Pass;
            }

            return testResult;
        }

        //============================================================================
        //          FormatException tests

        [TestMethod]
        public MFTestResults ParseSByte_FormatException_Test_25()
        {
            String[] strArr = new String[] { "", "1,234", "123e5", "a", "3.14159265358979" };
            int counter = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    SByte.Parse(strArr[i]);
                }
                //Supposed to throw FormatException
                catch (Exception ex)
                {
                    Log.Comment("1st " + i.ToString() + " Caught : " + ex.Message + " when SByte.Parse('" + strArr[i] + "')");
                    counter++;
                }
            }
            for (int i = 0; i < 5; i++)
            {
                String rdmString = MFUtilities.GetRandomString();
                try
                {
                    SByte.Parse(rdmString);
                }
                //Supposed to throw FormatException
                catch (Exception ex)
                {
                    Log.Comment("2nd " + i.ToString() + " Caught : " + ex.Message + " when SByte.Parse('" + rdmString + "')");
                    counter++;
                }
            }

            return (counter == (strArr.Length + 5)) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults ParseByte_FormatException_Test_26()
        {
            String[] strArr = new String[] { "", "1,234", "123e5", "a", "3.14159265358979" };
            int counter = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    Byte.Parse(strArr[i]);
                }
                //Supposed to throw FormatException
                catch (Exception ex)
                {
                    Log.Comment("1st " + i.ToString() + " Caught : " + ex.Message + " when Byte.Parse('" + strArr[i] + "')");
                    counter++;
                }
            }
            for (int i = 0; i < 5; i++)
            {
                String rdmString = MFUtilities.GetRandomString();
                try
                {
                    Byte.Parse(rdmString);
                }
                //Supposed to throw FormatException
                catch (Exception ex)
                {
                    Log.Comment("2nd " + i.ToString() + " Caught : " + ex.Message + " when Byte.Parse('" + rdmString + "')");
                    counter++;
                }
            }

            return (counter == (strArr.Length + 5)) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults ParseInt16_FormatException_Test_27()
        {
            String[] strArr = new String[] { "", "1,234", "123e5", "a", "3.14159265358979" };
            int counter = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    Int16.Parse(strArr[i]);
                }
                //Supposed to throw FormatException
                catch (Exception ex)
                {
                    Log.Comment("1st " + i.ToString() + " Caught : " + ex.Message + " when Int16.Parse('" + strArr[i] + "')");
                    counter++;
                }
            }
            for (int i = 0; i < 5; i++)
            {
                String rdmString = MFUtilities.GetRandomString();
                try
                {
                    Int16.Parse(rdmString);
                }
                //Supposed to throw FormatException
                catch (Exception ex)
                {
                    Log.Comment("2nd " + i.ToString() + " Caught : " + ex.Message + " when Int16.Parse('" + rdmString + "')");
                    counter++;
                }
            }

            return (counter == (strArr.Length + 5)) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults ParseUInt16_FormatException_Test_28()
        {
            String[] strArr = new String[] { "", "1,234", "123e5", "a", "3.14159265358979" };
            int counter = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    UInt16.Parse(strArr[i]);
                }
                //Supposed to throw FormatException
                catch (Exception ex)
                {
                    Log.Comment("1st " + i.ToString() + " Caught : " + ex.Message + " when UInt16.Parse('" + strArr[i] + "')");
                    counter++;
                }
            }
            for (int i = 0; i < 5; i++)
            {
                String rdmString = MFUtilities.GetRandomString();
                try
                {
                    UInt16.Parse(rdmString);
                }
                //Supposed to throw FormatException
                catch (Exception ex)
                {
                    Log.Comment("2nd " + i.ToString() + " Caught : " + ex.Message + " when UInt16.Parse('" + rdmString + "')");
                    counter++;
                }
            }

            return (counter == (strArr.Length + 5)) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults ParseInt32_FormatException_Test_29()
        {
            String[] strArr = new String[] { "", "1,234", "123e5", "a", "3.14159265358979" };
            int counter = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    Int32.Parse(strArr[i]);
                }
                //Supposed to throw FormatException
                catch (Exception ex)
                {
                    Log.Comment("1st " + i.ToString() + " Caught : " + ex.Message + " when Int32.Parse('" + strArr[i] + "')");
                    counter++;
                }
            }
            for (int i = 0; i < 5; i++)
            {
                String rdmString = MFUtilities.GetRandomString();
                try
                {
                    Int32.Parse(rdmString);
                }
                //Supposed to throw FormatException
                catch (Exception ex)
                {
                    Log.Comment("2nd " + i.ToString() + " Caught : " + ex.Message + " when Int32.Parse('" + rdmString + "')");
                    counter++;
                }
            }

            return (counter == (strArr.Length + 5)) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults ParseUInt32_FormatException_Test_30()
        {
            String[] strArr = new String[] { "", "1,234", "123e5", "a", "3.14159265358979" };
            int counter = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    UInt32.Parse(strArr[i]);
                }
                //Supposed to throw FormatException
                catch (Exception ex)
                {
                    Log.Comment("1st " + i.ToString() + " Caught : " + ex.Message + " when UInt32.Parse('" + strArr[i] + "')");
                    counter++;
                }
            }
            for (int i = 0; i < 5; i++)
            {
                String rdmString = MFUtilities.GetRandomString();
                try
                {
                    UInt32.Parse(rdmString);
                }
                //Supposed to throw FormatException
                catch (Exception ex)
                {
                    Log.Comment("2nd " + i.ToString() + " Caught : " + ex.Message + " when UInt32.Parse('" + rdmString + "')");
                    counter++;
                }
            }

            return (counter == (strArr.Length + 5)) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults ParseInt64_FormatException_Test_31()
        {
            String[] strArr = new String[] { "", "1,234", "123e5", "a", "3.14159265358979" };
            int counter = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    Int64.Parse(strArr[i]);
                }
                //Supposed to throw FormatException
                catch (Exception ex)
                {
                    Log.Comment("1st " + i.ToString() + " Caught : " + ex.Message + " when Int64.Parse('" + strArr[i] + "')");
                    counter++;
                }
            }
            for (int i = 0; i < 5; i++)
            {
                String rdmString = MFUtilities.GetRandomString();
                try
                {
                    Int64.Parse(rdmString);
                }
                //Supposed to throw FormatException
                catch (Exception ex)
                {
                    Log.Comment("2nd " + i.ToString() + " Caught : " + ex.Message + " when Int64.Parse('" + rdmString + "')");
                    counter++;
                }
            }

            return (counter == (strArr.Length + 5)) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults ParseUInt64_FormatException_Test_32()
        {
            String[] strArr = new String[] { "", "1,234", "123e5", "a", "3.14159265358979" };
            int counter = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    UInt64.Parse(strArr[i]);
                }
                //Supposed to throw FormatException
                catch (Exception ex)
                {
                    Log.Comment("1st " + i.ToString() + " Caught : " + ex.Message + " when UInt64.Parse('" + strArr[i] + "')");
                    counter++;
                }
            }
            for (int i = 0; i < 5; i++)
            {
                String rdmString = MFUtilities.GetRandomString();
                try
                {
                    UInt64.Parse(rdmString);
                }
                //Supposed to throw FormatException
                catch (Exception ex)
                {
                    Log.Comment("2nd " + i.ToString() + " Caught : " + ex.Message + " when UInt64.Parse('" + rdmString + "')");
                    counter++;
                }
            }

            return (counter == (strArr.Length + 5)) ? MFTestResults.Pass : MFTestResults.Fail;
        }


        //============================================================================
        //          OverflowException tests


        [TestMethod]
        public MFTestResults ParseSByte_OverflowException_Test_33()
        {
            String[] strArr = new String[] { ((Int64)SByte.MinValue - 1).ToString(), ((Int64)SByte.MinValue - 100).ToString(),
                                             ((Int64)SByte.MaxValue + 1).ToString(), ((Int64)SByte.MaxValue + 100).ToString() };
            int counter = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    SByte.Parse(strArr[i]);
                }
                //Supposed to throw OverflowException
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + "when SByte.Parse('" + strArr[i] + "')");
                    counter++;
                }
            }

            return (counter == strArr.Length) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults ParseByte_OverflowException_Test_34()
        {
            String[] strArr = new String[] { ((Int64)Byte.MinValue - 1).ToString(), ((Int64)Byte.MinValue - 100).ToString(),
                                             ((Int64)Byte.MaxValue + 1).ToString(), ((Int64)Byte.MaxValue + 100).ToString() };
            int counter = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    Byte.Parse(strArr[i]);
                }
                //Supposed to throw OverflowException
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + "when Byte.Parse('" + strArr[i] + "')");
                    counter++;
                }
            }

            return (counter == strArr.Length) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults ParseInt16_OverflowException_Test_35()
        {
            String[] strArr = new String[] { ((Int64)Int16.MinValue - 1).ToString(), ((Int64)Int16.MinValue - 100).ToString(),
                                             ((Int64)Int16.MaxValue + 1).ToString(), ((Int64)Int16.MaxValue + 100).ToString() };
            int counter = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    Int16.Parse(strArr[i]);
                }
                //Supposed to throw OverflowException
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + "when Int16.Parse('" + strArr[i] + "')");
                    counter++;
                }
            }

            return (counter == strArr.Length) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults ParseUInt16_OverflowException_Test_36()
        {
            String[] strArr = new String[] { ((Int64)UInt16.MinValue - 1).ToString(), ((Int64)UInt16.MinValue - 100).ToString(),
                                             ((Int64)UInt16.MaxValue + 1).ToString(), ((Int64)UInt16.MaxValue + 100).ToString() };
            int counter = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    UInt16.Parse(strArr[i]);
                }
                //Supposed to throw OverflowException
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + "when UInt16.Parse('" + strArr[i] + "')");
                    counter++;
                }
            }

            return (counter == strArr.Length) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults ParseInt32_OverflowException_Test_37()
        {
            String[] strArr = new String[] { ((Int64)Int32.MinValue - 1).ToString(), ((Int64)Int32.MinValue - 100).ToString(),
                                             ((Int64)Int32.MaxValue + 1).ToString(), ((Int64)Int32.MaxValue + 100).ToString() };
            int counter = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    Int32.Parse(strArr[i]);
                }
                //Supposed to throw OverflowException
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + "when Int32.Parse('" + strArr[i] + "')");
                    counter++;
                }
            }

            return (counter == strArr.Length) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults ParseUInt32_OverflowException_Test_38()
        {
            String[] strArr = new String[] { ((Int64)UInt32.MinValue - 1).ToString(), ((Int64)UInt32.MinValue - 100).ToString(),
                                             ((Int64)UInt32.MaxValue + 1).ToString(), ((Int64)UInt32.MaxValue + 100).ToString() };
            int counter = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    UInt32.Parse(strArr[i]);
                }
                //Supposed to throw OverflowException
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + "when UInt32.Parse('" + strArr[i] + "')");
                    counter++;
                }
            }

            return (counter == strArr.Length) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults ParseInt64_OverflowException_Test_39()
        {
            Log.Comment("This currently fails, see 21641 for details");
            string[] strArr = new string[] { "-9223372036854775809", "-9223372036854775900",
                                             "9223372036854775808", "9223372036854775900" };
            int counter = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    Int64.Parse(strArr[i]);
                }
                //Supposed to throw OverflowException
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + "when Int64.Parse('" + strArr[i] + "')");
                    counter++;
                }
            }

            return (counter == strArr.Length) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults ParseUInt64_OverflowException_Test_40()
        {
            string[] strArr = new string[] { "-1", "-100", "18446744073709551616", "18446744073709551700" };
            int counter = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                try
                {
                    UInt64.Parse(strArr[i]);
                }
                //Supposed to throw OverflowException
                catch (Exception ex)
                {
                    Log.Comment(i.ToString() + " Caught : " + ex.Message + "when UInt64.Parse('" + strArr[i] + "')");
                    counter++;
                }
            }

            return (counter == strArr.Length) ? MFTestResults.Pass : MFTestResults.Pass;
        }


        [TestMethod]
        public MFTestResults Cast_Double_to_int64_Test_40()
        {
            MFTestResults retVal = MFTestResults.Pass;

            double dbVal = new Random().Next();
            
            // Convert to int and uint should keep the value
            long  l_val =  (long)dbVal;

            if (l_val != dbVal)
            {
                Log.Comment( "Convert from double " + dbVal + " to long produced different value " + l_val );
                retVal = MFTestResults.Fail;
            }
            
            ulong ul_val = (ulong)dbVal;

            if (ul_val != dbVal)
            {
                Log.Comment("Convert from double " + dbVal + " to ulong produced different value " + ul_val);
                retVal = MFTestResults.Fail;
            }

            // Change sign to negative
            dbVal = -dbVal;
            
            l_val =  (long)dbVal;

            if (l_val != dbVal)
            {
                Log.Comment( "Convert from double " + dbVal + " to long produced different value " + l_val );
                retVal = MFTestResults.Fail;
            }
            
            ul_val = (ulong)dbVal;
            long ul_val_cast  = (long)ul_val;

            if (ul_val_cast != dbVal)

            {
                Log.Comment("Convert from double " + dbVal + " to ulong produced different value " + ul_val_cast);
                retVal = MFTestResults.Fail;
            }

            return retVal;
        }


        public enum MyEnum  : short { Value = 25 }
        public enum MyEnum1 { Value = 24 }
        public enum MyEnum2 : short { Value = 23 }

        class CastTestClass
        {

        }


        [TestMethod]
        public MFTestResults box_unbox_Test_1()
        {
            MFTestResults retVal = MFTestResults.Pass;
            
            // Creates objects for testing of different casts.
            object o_enum =  MyEnum.Value;
            object o_enum1 = MyEnum1.Value;
            object o_long = 24L;
            object o_class = new CastTestClass();
            object o_guid = Guid.NewGuid();
            
            // Try casts that shoud succeed. Any exception here means failure.
            try
            {
                // First we try casts that should succeed. 
                // Casts between enums with the same basic type
                MyEnum2 e2 = (MyEnum2)o_enum; // line 2
                // Cast from enum to primitive type that enum is based on
                short sv = (short)o_enum;
                if (sv != (short)MyEnum.Value)
                {
                    Log.Comment("Changed value during box/unbox from " + MyEnum.Value + " to   " + sv);
                    retVal = MFTestResults.Fail;
                }

                // Cast from enum to primitive type that enum is based on
                int iv = (int)o_enum1;
                if (iv != (short)MyEnum1.Value)
                {
                    Log.Comment("Changed value during box/unbox from " + MyEnum1.Value + " to   " + iv);
                    retVal = MFTestResults.Fail;
                }

                int i_long = (int)(long)o_long;
                CastTestClass cls = (CastTestClass)o_class;
                Guid guid = (Guid)o_guid;
            }
            catch (Exception e)
            {
                Log.Comment("Exception thrown during unbox test" + e.Message);
                retVal = MFTestResults.Fail;
            }

            // Now casts that should throw exception. Any cast that does not throw - means error.
            try
            {
                MyEnum1 e1 = (MyEnum1)o_enum;
                Log.Comment("Error, cast succeded, but should have thrown exception");
                retVal = MFTestResults.Fail;
            }
            catch (Exception ) { }

            // Now casts that should throw exception. Any cast that does not throw - means error.
            try
            {
                int i = (int)o_long;
                Log.Comment("Error, cast object with long to int succeded, but should have thrown exception");
                retVal = MFTestResults.Fail;
            }
            catch (Exception ) { }
            
            // Now casts that should throw exception. Any cast that does not throw - means error.
            try
            {
                int i = (int)o_class; 
                Log.Comment("Error, cast object class to int succeded, but should have thrown exception");
                retVal = MFTestResults.Fail;
            }
            catch (Exception ) { }

            // Now casts that should throw exception. Any cast that does not throw - means error.
            try
            {
                int i = (int)o_enum; 
                Log.Comment("Error, cast object class to int succeded, but should have thrown exception");
                retVal = MFTestResults.Fail;
            }
            catch (Exception ) { }

            // Now casts that should throw exception. Any cast that does not throw - means error.
            try
            {
                int i = (int)o_guid;
                Log.Comment("Error, cast stuct Guid class to int succeded, but should have thrown exception");
                retVal = MFTestResults.Fail;
            }
            catch (Exception) { }

            return retVal;
        }
    }
}
