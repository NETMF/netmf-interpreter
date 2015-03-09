////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ConvertTests : IMFTestInterface
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
        }

        //Test Case Calls
        [TestMethod]
        public MFTestResults Cast_FloatingPoint()
        {
            MFTestResults res = MFTestResults.Pass;

            uint u1;
            uint u2;
            double d1;
            float f1;
            long l1;
            long l2;
            double d2;
            Random rand = new Random();

            for (int i = 0; i < 100; i++)
            {
                u1 = (uint)rand.Next();

                d1 = (double)u1; // Does not work correctly (d1 is close to 0)
                u2 = (uint)d1;
                if (d1 != u1 || u2 != u1)
                {
                    Log.Comment("Cast from uint to double failed");
                    res = MFTestResults.Fail;
                }

                f1 = (float)u1; // Same problem
                if (f1 != u1)
                {
                    Log.Comment("Cast from uint to float failed");
                    res = MFTestResults.Fail;
                }

                l1 = (long)u1;
                u2 = (uint)l1;
                if (l1 != u1 || u2 != u1)
                {
                    Log.Comment("Cast from uint to long failed");
                    res = MFTestResults.Fail;
                }

                d2 = l1; // Same problem
                l2 = (long)d2;
                if (d2 != l1 || l2 != l1)
                {
                    Log.Comment("Cast from long to double failed");
                    res = MFTestResults.Fail;
                }
            }

            return res;
        }

        //Test Case Calls
        [TestMethod]
        public MFTestResults Convert_Positive()
        {
            string number = "12";
            int actualNumber = 12;

            SByte value_sb = Convert.ToSByte(number);

            if (value_sb != (byte)12)
            {
                return MFTestResults.Fail;
            }

            //--//

            Byte value_b = Convert.ToByte(number);

            if (value_b != (byte)12)
            {
                return MFTestResults.Fail;
            }

            //--//

            Int16 value_s16 = Convert.ToInt16(number);

            if (value_s16 != (short)12)
            {
                return MFTestResults.Fail;
            }

            //--//

            UInt16 value_u16 = Convert.ToUInt16(number);

            if (value_u16 != (ushort)12)
            {
                return MFTestResults.Fail;
            }

            //--//

            Int32 value_s32 = Convert.ToInt32(number);

            if (value_s32 != (int)12)
            {
                return MFTestResults.Fail;
            }

            //--//

            UInt32 value_u32 = Convert.ToUInt32(number);

            if (value_u32 != (uint)12)
            {
                return MFTestResults.Fail;
            }

            //--//

            Int64 value_s64 = Convert.ToInt32(number);

            if (value_s64 != (long)12)
            {
                return MFTestResults.Fail;
            }

            //--//

            UInt64 value_u64 = Convert.ToUInt64(number);

            if (value_u64 != (ulong)12)
            {
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        //Test Case Calls
        [TestMethod]
        public MFTestResults Convert_PositivePlus()
        {
            string number = "+12";
            int actualNumber = 12;

            SByte value_sb = Convert.ToSByte(number);

            if (value_sb != (byte)12)
            {
                return MFTestResults.Fail;
            }

            //--//

            Byte value_b = Convert.ToByte(number);

            if (value_b != (byte)12)
            {
                return MFTestResults.Fail;
            }

            //--//

            Int16 value_s16 = Convert.ToInt16(number);

            if (value_s16 != (short)12)
            {
                return MFTestResults.Fail;
            }

            //--//

            UInt16 value_u16 = Convert.ToUInt16(number);

            if (value_u16 != (ushort)12)
            {
                return MFTestResults.Fail;
            }

            //--//

            Int32 value_s32 = Convert.ToInt32(number);

            if (value_s32 != (int)12)
            {
                return MFTestResults.Fail;
            }

            //--//

            UInt32 value_u32 = Convert.ToUInt32(number);

            if (value_u32 != (uint)12)
            {
                return MFTestResults.Fail;
            }

            //--//

            Int64 value_s64 = Convert.ToInt32(number);

            if (value_s64 != (long)12)
            {
                return MFTestResults.Fail;
            }

            //--//

            UInt64 value_u64 = Convert.ToUInt64(number);

            if (value_u64 != (ulong)12)
            {
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }


        [TestMethod]
        public MFTestResults Convert_Negative()
        {
            string number = "-12";
            int actualNumber = -12;

            SByte value_sb = Convert.ToSByte(number);

            if (value_sb != (sbyte)actualNumber)
            {
                return MFTestResults.Fail;
            }

            //--//

            try
            {
                Byte value_b = Convert.ToByte(number);
                return MFTestResults.Fail;
            }
            catch
            {
            }

            //--//

            Int16 value_s16 = Convert.ToInt16(number);

            if (value_s16 != (short)actualNumber)
            {
                return MFTestResults.Fail;
            }

            //--//

            try
            {
                UInt16 value_u16 = Convert.ToUInt16(number);
                return MFTestResults.Fail;
            }
            catch
            {
            }

            //--//

            Int32 value_s32 = Convert.ToInt32(number);

            if (value_s32 != (int)actualNumber)
            {
                return MFTestResults.Fail;
            }

            //--//

            try
            {
                UInt32 value_u32 = Convert.ToUInt32(number);
                return MFTestResults.Fail;
            }
            catch
            {
            }

            //--//

            Int64 value_s64 = Convert.ToInt32(number);

            if (value_s64 != (long)actualNumber)
            {
                return MFTestResults.Fail;
            }

            //--//

            try
            {
                UInt64 value_u64 = Convert.ToUInt64(number);
                return MFTestResults.Fail;
            }
            catch
            {
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults Convert_Double()
        {
            string number = "36.123456";
            double actualNumber = 36.123456;

            double value_dd = Convert.ToDouble(number);

            if (value_dd != actualNumber)
            {
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }


        [TestMethod]
        public MFTestResults Convert_Plus()
        {
            string number = "+36.123456";
            double actualNumber = 36.123456;

            double value_dd = Convert.ToDouble(number);

            if (value_dd != actualNumber)
            {
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }


        [TestMethod]
        public MFTestResults Convert_Neg()
        {
            string number = "-36.123456";
            double actualNumber = -36.123456;

            double value_dd = Convert.ToDouble(number);

            if (value_dd != actualNumber)
            {
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;

        }

        [TestMethod]
        public MFTestResults Convert_Whitespace()
        {
            string intnum = " -3484  ";
            string number = " +36.123456   ";
            long actualInt = -3484;
            double actualNumber = 36.123456;

            if (actualInt != Convert.ToInt16(intnum))
            {
                return MFTestResults.Fail;
            }

            if (actualInt != Convert.ToInt32(intnum))
            {
                return MFTestResults.Fail;
            }

            if (actualInt != Convert.ToInt64(intnum))
            {
                return MFTestResults.Fail;
            }

            double value_dd = Convert.ToDouble(number);

            if (value_dd != actualNumber)
            {
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults Convert_DoubleNormalizeNeg()
        {
            string number = "-3600030383448481.123456";
            double actualNumber = -3600030383448481.123456;

            double value_dd = Convert.ToDouble(number);

            if (value_dd != actualNumber)
            {
                return MFTestResults.Fail;
            }

            number = "+0.00000000484874758559e-3";
            actualNumber = 4.84874758559e-12;

            if (actualNumber != Convert.ToDouble(number))
            {
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults Convert_HexInt()
        {
            string number = "0x01234567";
            int actualNumber = 0x01234567;

            int value = Convert.ToInt32(number, 16);

            if (value != actualNumber)
            {
                return MFTestResults.Fail;
            }

            number = "0x89abcdef";
            unchecked
            {
                actualNumber = (int)0x89abcdef;
            }
            if (actualNumber != Convert.ToInt32(number, 16))
            {
                return MFTestResults.Fail;
            }

            number = "0x0AbF83C";
            actualNumber = 0xAbF83C;

            if (actualNumber != Convert.ToInt32(number, 16))
            {
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults Convert_BoundaryValues()
        {
            double valMax = double.MaxValue;
            string numMax = valMax.ToString();
            double valMin = double.MinValue;
            string numMin = valMin.ToString();

            if(valMax != Convert.ToDouble(numMax)) return MFTestResults.Fail;
            if(valMin != Convert.ToDouble(numMin)) return MFTestResults.Fail;
            
            valMax = float.MaxValue;
            numMax = valMax.ToString();
            valMin = float.MinValue;
            numMin = valMin.ToString();

            if(valMax != Convert.ToDouble(numMax)) return MFTestResults.Fail;
            if(valMin != Convert.ToDouble(numMin)) return MFTestResults.Fail;

            long   lMax = long.MaxValue;
            numMax = lMax.ToString();
            long   lMin = long.MinValue;
            numMin = lMin.ToString();

            if(lMax != Convert.ToInt64(numMax)) return MFTestResults.Fail;
            if(lMin != Convert.ToInt64(numMin)) return MFTestResults.Fail;

            ulong ulMax = ulong.MaxValue;
            numMax = ulMax.ToString();
            ulong ulMin = ulong.MinValue;
            numMin = ulMin.ToString();

            if(ulMax != Convert.ToUInt64(numMax)) return MFTestResults.Fail;
            if(ulMin != Convert.ToUInt64(numMin)) return MFTestResults.Fail;
            
            
            long   iMax = int.MaxValue;
            numMax      = iMax.ToString();
            long   iMin = int.MinValue;
            numMin      = iMin.ToString();

            if(iMax != Convert.ToInt32(numMax)) return MFTestResults.Fail;
            if(iMin != Convert.ToInt32(numMin)) return MFTestResults.Fail;

            uint uiMax = uint.MaxValue;
            numMax     = uiMax.ToString();
            uint uiMin = uint.MinValue;
            numMin     = uiMin.ToString();

            if(uiMax != Convert.ToUInt32(numMax)) return MFTestResults.Fail;
            if(uiMin != Convert.ToUInt32(numMin)) return MFTestResults.Fail;

            short   sMax = short.MaxValue;
            numMax       = sMax.ToString();
            short   sMin = short.MinValue;
            numMin       = sMin.ToString();
            
            if(sMax != Convert.ToInt16(numMax)) return MFTestResults.Fail;
            if(sMin != Convert.ToInt16(numMin)) return MFTestResults.Fail;
            
            ushort usMax = ushort.MaxValue;
            numMax       = usMax.ToString();
            ushort usMin = ushort.MinValue;
            numMin       = usMin.ToString();
            
            if(usMax != Convert.ToUInt16(numMax)) return MFTestResults.Fail;
            if(usMin != Convert.ToUInt16(numMin)) return MFTestResults.Fail;

            sbyte sbMax = sbyte.MaxValue;
            numMax      = sbMax.ToString();
            sbyte sbMin = sbyte.MinValue;
            numMin      = sbMin.ToString();
            
            if(sbMax != Convert.ToSByte(numMax)) return MFTestResults.Fail;
            if(sbMin != Convert.ToSByte(numMin)) return MFTestResults.Fail;
            
            byte bMax = byte.MaxValue;
            numMax    = bMax.ToString();
            byte bMin = byte.MinValue;
            numMin    = bMin.ToString();
            
            if(bMax != Convert.ToByte(numMax)) return MFTestResults.Fail;
            if(bMin != Convert.ToByte(numMin)) return MFTestResults.Fail;

            return MFTestResults.Pass;
        }


        [TestMethod]
        public MFTestResults Convert_LeadingZeroValues()
        {
            string number = "00000000";

            if(0 != Convert.ToInt16(number)) return MFTestResults.Fail;
            if(0 != Convert.ToInt32(number)) return MFTestResults.Fail;
            if(0 != Convert.ToInt64(number)) return MFTestResults.Fail;

            number = "+00000000000";

            if(0 != Convert.ToInt16(number)) return MFTestResults.Fail;
            if(0 != Convert.ToInt32(number)) return MFTestResults.Fail;
            if(0 != Convert.ToInt64(number)) return MFTestResults.Fail;

            number = "-00000000000";
            
            if(0 != Convert.ToInt16(number)) return MFTestResults.Fail;
            if(0 != Convert.ToInt32(number)) return MFTestResults.Fail;
            if(0 != Convert.ToInt64(number)) return MFTestResults.Fail;

            return MFTestResults.Pass;
        }
        
        [TestMethod]
        public MFTestResults Convert_LeadingZeros()
        {
            string number = "000003984";
            int actualNumber = 3984;

            if ((short)actualNumber != Convert.ToInt16(number))
            {
                return MFTestResults.Fail;
            }

            if (actualNumber != Convert.ToInt32(number))
            {
                return MFTestResults.Fail;
            }

            if (actualNumber != Convert.ToInt64(number))
            {
                return MFTestResults.Fail;
            }

            number = "-00000003489";
            actualNumber = -3489;

            if ((short)actualNumber != Convert.ToInt16(number))
            {
                return MFTestResults.Fail;
            }

            if (actualNumber != Convert.ToInt32(number))
            {
                return MFTestResults.Fail;
            }

            if (actualNumber != Convert.ToInt64(number))
            {
                return MFTestResults.Fail;
            }

            number = "+00000003489";
            actualNumber = 3489;

            if ((short)actualNumber != Convert.ToInt16(number))
            {
                return MFTestResults.Fail;
            }

            if (actualNumber != Convert.ToInt32(number))
            {
                return MFTestResults.Fail;
            }

            if (actualNumber != Convert.ToInt64(number))
            {
                return MFTestResults.Fail;
            }

            number = "+000000043984.00048850000";
            double numD = 4.39840004885;

            if (numD == Convert.ToDouble(number))
            {
                return MFTestResults.Fail;
            }

            number = "-000000043984.00048850000";
            numD = -4.39840004885;

            if (numD == Convert.ToDouble(number))
            {
                return MFTestResults.Fail;
            }

            number = "000000043984.000488500e-006";
            numD = 4.39840004885e2;

            if (numD == Convert.ToDouble(number))
            {
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults Convert_64ParsePerf()
        {
            string number = "-7446744073709551615";
            long val = 0;

            DateTime start = DateTime.Now;
            for (int i = 0; i < 0x1000; i++)
            {
                val = Convert.ToInt64(number);
            }
            Log.Comment("Time: " + (DateTime.Now - start).ToString());

            return val == -7446744073709551615L ? MFTestResults.Pass : MFTestResults.Fail;
        }
    }    
}
