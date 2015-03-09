////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class BitConverterTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("BitConverter tests initialized.");
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after BitConverter tests.");
        }

        //Test Case Calls
        [TestMethod]
        public MFTestResults BitConverterTest_DoubleToInt64Bits()
        {
            try
            {
                Helper.DoubleToLongBits(1.0, 0x3FF0000000000000);
                Helper.DoubleToLongBits(15.0, 0x402E000000000000);
                Helper.DoubleToLongBits(255.0, 0x406FE00000000000);
                Helper.DoubleToLongBits(4294967295.0, 0x41EFFFFFFFE00000);
                Helper.DoubleToLongBits(0.00390625, 0x3F70000000000000);
                Helper.DoubleToLongBits(0.00000000023283064365386962890625, 0x3DF0000000000000);
                Helper.DoubleToLongBits(1.234567890123E-300, 0x01AA74FE1C1E7E45);
                Helper.DoubleToLongBits(1.23456789012345E-150, 0x20D02A36586DB4BB);
                Helper.DoubleToLongBits(1.2345678901234565, 0x3FF3C0CA428C59FA);
                Helper.DoubleToLongBits(1.2345678901234567, 0x3FF3C0CA428C59FB);
                Helper.DoubleToLongBits(1.2345678901234569, 0x3FF3C0CA428C59FC);
                Helper.DoubleToLongBits(1.23456789012345678E+150, 0x5F182344CD3CDF9F);
                Helper.DoubleToLongBits(1.234567890123456789E+300, 0x7E3D7EE8BCBBD352);
                Helper.DoubleToLongBits(double.MinValue, unchecked((long)0xFFEFFFFFFFFFFFFF));
                Helper.DoubleToLongBits(double.MaxValue, 0x7FEFFFFFFFFFFFFF);
                Helper.DoubleToLongBits(double.Epsilon, 0x0000000000000001);
                Helper.DoubleToLongBits(double.NaN, unchecked((long)0xFFF8000000000000));
                Helper.DoubleToLongBits(double.NegativeInfinity, unchecked((long)0xFFF0000000000000));
                Helper.DoubleToLongBits(double.PositiveInfinity, 0x7FF0000000000000);
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_GetBytesBool()
        {
            try
            {
                Helper.GetBytesBool(true, new byte[] { 1 });
                Helper.GetBytesBool(false, new byte[] { 0 });
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_GetBytesChar()
        {
            try
            {
                Helper.GetBytesChar('\0', new byte[] { 0x00, 0x00 });
                Helper.GetBytesChar(' ', new byte[] { 0x20, 0x00 });
                Helper.GetBytesChar('*', new byte[] { 0x2A, 0x00 });
                Helper.GetBytesChar('3', new byte[] { 0x33, 0x00 });
                Helper.GetBytesChar('A', new byte[] { 0x41, 0x00 });
                Helper.GetBytesChar('[', new byte[] { 0x5B, 0x00 });
                Helper.GetBytesChar('a', new byte[] { 0x61, 0x00 });
                Helper.GetBytesChar('{', new byte[] { 0x7B, 0x00 });
                Helper.GetBytesChar('测', new byte[] { 0x4B, 0x6D });
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_GetBytesDouble()
        {
            try
            {
                Helper.GetBytesDouble(0.0, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                Helper.GetBytesDouble(1.0, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F });
                Helper.GetBytesDouble(255.0, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0xE0, 0x6F, 0x40 });
                Helper.GetBytesDouble(4294967295.0, new byte[] { 0x00, 0x00, 0xE0, 0xFF, 0xFF, 0xFF, 0xEF, 0x41 });
                Helper.GetBytesDouble(0.00390625, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x70, 0x3F });
                Helper.GetBytesDouble(0.00000000023283064365386962890625, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3D });
                Helper.GetBytesDouble(1.23456789012345E-300, new byte[] { 0xDF, 0x88, 0x1E, 0x1C, 0xFE, 0x74, 0xAA, 0x01 });
                Helper.GetBytesDouble(1.2345678901234565, new byte[] { 0xFA, 0x59, 0x8C, 0x42, 0xCA, 0xC0, 0xF3, 0x3F });
                Helper.GetBytesDouble(1.2345678901234567, new byte[] { 0xFB, 0x59, 0x8C, 0x42, 0xCA, 0xC0, 0xF3, 0x3F });
                Helper.GetBytesDouble(1.2345678901234569, new byte[] { 0xFC, 0x59, 0x8C, 0x42, 0xCA, 0xC0, 0xF3, 0x3F });
                Helper.GetBytesDouble(1.23456789012345678E+300, new byte[] { 0x52, 0xD3, 0xBB, 0xBC, 0xE8, 0x7E, 0x3D, 0x7E });
                Helper.GetBytesDouble(double.MinValue, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xEF, 0xFF });
                Helper.GetBytesDouble(double.MaxValue, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xEF, 0x7F });
                Helper.GetBytesDouble(double.Epsilon, new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                Helper.GetBytesDouble(double.NaN, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF8, 0xFF });
                Helper.GetBytesDouble(double.NegativeInfinity, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0xFF });
                Helper.GetBytesDouble(double.PositiveInfinity, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x7F });
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_GetBytesInt16()
        {
            try
            {
                Helper.GetBytesInt16(0, new byte[] { 0x00, 0x00 });
                Helper.GetBytesInt16(15, new byte[] { 0x0F, 0x00 });
                Helper.GetBytesInt16(-15, new byte[] { 0xF1, 0xFF });
                Helper.GetBytesInt16(10000, new byte[] { 0x10, 0x27 });
                Helper.GetBytesInt16(-10000, new byte[] { 0xF0, 0xD8 });
                Helper.GetBytesInt16(short.MinValue, new byte[] { 0x00, 0x80 });
                Helper.GetBytesInt16(short.MaxValue, new byte[] { 0xFF, 0x7F });
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_GetBytesInt32()
        {
            try
            {
                Helper.GetBytesInt32(0, new byte[] { 0x00, 0x00, 0x00, 0x00 });
                Helper.GetBytesInt32(15, new byte[] { 0x0F, 0x00, 0x00, 0x00 });
                Helper.GetBytesInt32(-15, new byte[] { 0xF1, 0xFF, 0xFF, 0xFF });
                Helper.GetBytesInt32(1048576, new byte[] { 0x00, 0x00, 0x10, 0x00 });
                Helper.GetBytesInt32(-1048576, new byte[] { 0x00, 0x00, 0xF0, 0xFF });
                Helper.GetBytesInt32(1000000000, new byte[] { 0x00, 0xCA, 0x9A, 0x3B });
                Helper.GetBytesInt32(-1000000000, new byte[] { 0x00, 0x36, 0x65, 0xC4 });
                Helper.GetBytesInt32(int.MinValue, new byte[] { 0x00, 0x00, 0x00, 0x80 });
                Helper.GetBytesInt32(int.MaxValue, new byte[] { 0xFF, 0xFF, 0xFF, 0x7F });
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_GetBytesInt64()
        {
            try
            {
                Helper.GetBytesInt64(0, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                Helper.GetBytesInt64(16777215, new byte[] { 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00 });
                Helper.GetBytesInt64(-16777215, new byte[] { 0x01, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                Helper.GetBytesInt64(1000000000, new byte[] { 0x00, 0xCA, 0x9A, 0x3B, 0x00, 0x00, 0x00, 0x00 });
                Helper.GetBytesInt64(-1000000000, new byte[] { 0x00, 0x36, 0x65, 0xC4, 0xFF, 0xFF, 0xFF, 0xFF });
                Helper.GetBytesInt64(4294967296, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 });
                Helper.GetBytesInt64(-4294967296, new byte[] { 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF });
                Helper.GetBytesInt64(187649984473770, new byte[] { 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0x00, 0x00 });
                Helper.GetBytesInt64(-187649984473770, new byte[] { 0x56, 0x55, 0x55, 0x55, 0x55, 0x55, 0xFF, 0xFF });
                Helper.GetBytesInt64(1000000000000000000, new byte[] { 0x00, 0x00, 0x64, 0xA7, 0xB3, 0xB6, 0xE0, 0x0D });
                Helper.GetBytesInt64(-1000000000000000000, new byte[] { 0x00, 0x00, 0x9C, 0x58, 0x4C, 0x49, 0x1F, 0xF2 });
                Helper.GetBytesInt64(long.MinValue, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 });
                Helper.GetBytesInt64(long.MaxValue, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F });
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_GetBytesSingle()
        {
            try
            {
                Helper.GetBytesSingle(0.0F, new byte[] { 0x00, 0x00, 0x00, 0x00 });
                Helper.GetBytesSingle(1.0F, new byte[] { 0x00, 0x00, 0x80, 0x3F });
                Helper.GetBytesSingle(15.0F, new byte[] { 0x00, 0x00, 0x70, 0x41 });
                Helper.GetBytesSingle(65535.0F, new byte[] { 0x00, 0xFF, 0x7F, 0x47 });
                Helper.GetBytesSingle(0.00390625F, new byte[] { 0x00, 0x00, 0x80, 0x3B });
                Helper.GetBytesSingle(0.00000000023283064365386962890625F, new byte[] { 0x00, 0x00, 0x80, 0x2F });
                Helper.GetBytesSingle(1.2345E-35F, new byte[] { 0x49, 0x46, 0x83, 0x05 });
                Helper.GetBytesSingle(1.2345671F, new byte[] { 0x4B, 0x06, 0x9E, 0x3F });
                Helper.GetBytesSingle(1.2345673F, new byte[] { 0x4D, 0x06, 0x9E, 0x3F });
                Helper.GetBytesSingle(1.2345677F, new byte[] { 0x50, 0x06, 0x9E, 0x3F });
                Helper.GetBytesSingle(1.23456789E+35F, new byte[] { 0x1E, 0x37, 0xBE, 0x79 });
                Helper.GetBytesSingle(float.MinValue, new byte[] { 0xFF, 0xFF, 0x7F, 0xFF });
                Helper.GetBytesSingle(float.MaxValue, new byte[] { 0xFF, 0xFF, 0x7F, 0x7F });
                Helper.GetBytesSingle(float.Epsilon, new byte[] { 0x01, 0x00, 0x00, 0x00 });
                Helper.GetBytesSingle(0.0F / 0.0F, new byte[] { 0x00, 0x00, 0xC0, 0xFF });
                Helper.GetBytesSingle(-1.0F / 0.0F, new byte[] { 0x00, 0x00, 0x80, 0xFF });
                Helper.GetBytesSingle(1.0F / 0.0F, new byte[] { 0x00, 0x00, 0x80, 0x7F });
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_GetBytesUInt16()
        {
            try
            {
                Helper.GetBytesUInt16(15, new byte[] { 0x0F, 0x00 });
                Helper.GetBytesUInt16(1023, new byte[] { 0xFF, 0x03 });
                Helper.GetBytesUInt16(10000, new byte[] { 0x10, 0x27 });
                Helper.GetBytesUInt16(ushort.MinValue, new byte[] { 0x00, 0x00 });
                Helper.GetBytesUInt16((ushort)short.MaxValue, new byte[] { 0xFF, 0x7F });
                Helper.GetBytesUInt16(ushort.MaxValue, new byte[] { 0xFF, 0xFF });
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_GetBytesUInt32()
        {
            try
            {
                Helper.GetBytesUInt32(15, new byte[] { 0x0F, 0x00, 0x00, 0x00 });
                Helper.GetBytesUInt32(1023, new byte[] { 0xFF, 0x03, 0x00, 0x00 });
                Helper.GetBytesUInt32(1048576, new byte[] { 0x00, 0x00, 0x10, 0x00 });
                Helper.GetBytesUInt32(1000000000, new byte[] { 0x00, 0xCA, 0x9A, 0x3B });
                Helper.GetBytesUInt32(uint.MinValue, new byte[] { 0x00, 0x00, 0x00, 0x00 });
                Helper.GetBytesUInt32(int.MaxValue, new byte[] { 0xFF, 0xFF, 0xFF, 0x7F });
                Helper.GetBytesUInt32(uint.MaxValue, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_GetBytesUInt64()
        {
            try
            {
                Helper.GetBytesUInt64(16777215, new byte[] { 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00 });
                Helper.GetBytesUInt64(1000000000, new byte[] { 0x00, 0xCA, 0x9A, 0x3B, 0x00, 0x00, 0x00, 0x00 });
                Helper.GetBytesUInt64(4294967296, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 });
                Helper.GetBytesUInt64(187649984473770, new byte[] { 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0x00, 0x00 });
                Helper.GetBytesUInt64(1000000000000000000, new byte[] { 0x00, 0x00, 0x64, 0xA7, 0xB3, 0xB6, 0xE0, 0x0D });
                Helper.GetBytesUInt64(10000000000000000000, new byte[] { 0x00, 0x00, 0xE8, 0x89, 0x04, 0x23, 0xC7, 0x8A });
                Helper.GetBytesUInt64(ulong.MinValue, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                Helper.GetBytesUInt64(long.MaxValue, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F });
                Helper.GetBytesUInt64(ulong.MaxValue, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_LongBitsToDouble()
        {
            try
            {
                Helper.LongBitsToDouble(0x0000000000000000, 0.0000000000000000E+000);
                Helper.LongBitsToDouble(0x3FF0000000000000, 1.0000000000000000E+000);
                Helper.LongBitsToDouble(0x402E000000000000, 1.5000000000000000E+001);
                Helper.LongBitsToDouble(0x406FE00000000000, 2.5500000000000000E+002);
                Helper.LongBitsToDouble(0x41EFFFFFFFE00000, 4.2949672950000000E+009);
                Helper.LongBitsToDouble(0x3F70000000000000, 3.9062500000000000E-003);
                Helper.LongBitsToDouble(0x3DF0000000000000, 2.3283064365386963E-010);
                Helper.LongBitsToDouble(0x0000000000000001, 4.9406564584124654E-324);
                Helper.LongBitsToDouble(0x000000000000FFFF, 3.2378592100206092E-319);
                Helper.LongBitsToDouble(0x0000FFFFFFFFFFFF, 1.3906711615669959E-309);
                Helper.LongBitsToDouble(unchecked((long)0xFFFFFFFFFFFFFFFF), double.NaN);
                Helper.LongBitsToDouble(unchecked((long)0xFFF0000000000000), double.NegativeInfinity);
                Helper.LongBitsToDouble(0x7FF0000000000000, double.PositiveInfinity);
                Helper.LongBitsToDouble(unchecked((long)0xFFEFFFFFFFFFFFFF), double.MinValue);
                Helper.LongBitsToDouble(0x7FEFFFFFFFFFFFFF, double.MaxValue);
                Helper.LongBitsToDouble(long.MinValue, 0.0000000000000000E+000);
                Helper.LongBitsToDouble(long.MaxValue, double.NaN);
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_ToBoolean()
        {
            try
            {
                var byteArray = new byte[] { 0, 1, 2, 4, 8, 16, 32, 64, 128, 255 };

                Helper.BAToBool(byteArray, 0, false);
                Helper.BAToBool(byteArray, 1, true);
                Helper.BAToBool(byteArray, 3, true);
                Helper.BAToBool(byteArray, 5, true);
                Helper.BAToBool(byteArray, 8, true);
                Helper.BAToBool(byteArray, 9, true);

                Helper.BAToBoolThrow(byteArray, int.MinValue, typeof(ArgumentOutOfRangeException));
                Helper.BAToBoolThrow(byteArray, -1, typeof(ArgumentOutOfRangeException));
                Helper.BAToBoolThrow(byteArray, byteArray.Length, typeof(ArgumentOutOfRangeException));
                Helper.BAToBoolThrow(byteArray, int.MaxValue, typeof(ArgumentOutOfRangeException));
                Helper.BAToBoolThrow(new byte[] { }, 1, typeof(ArgumentOutOfRangeException));
                Helper.BAToBoolThrow(null, 1, typeof(ArgumentNullException));
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_ToChar()
        {
            try
            {
                var byteArray = new byte[] { 32, 0, 0, 42, 0, 65, 0, 125, 0, 197, 0, 168, 3, 41, 4, 172, 32 };

                Helper.BAToChar(byteArray, 0, ' ');
                Helper.BAToChar(byteArray, 1, '\0');
                Helper.BAToChar(byteArray, 3, '*');
                Helper.BAToChar(byteArray, 5, 'A');
                Helper.BAToChar(byteArray, 7, '}');
                Helper.BAToChar(byteArray, 9, 'Å');
                Helper.BAToChar(byteArray, 11, 'Ψ');
                Helper.BAToChar(byteArray, 13, 'Щ');
                Helper.BAToChar(byteArray, 15, '€');

                Helper.BAToCharThrow(byteArray, int.MinValue, typeof(ArgumentOutOfRangeException));
                Helper.BAToCharThrow(byteArray, -1, typeof(ArgumentOutOfRangeException));
                Helper.BAToCharThrow(byteArray, byteArray.Length - 1, typeof(ArgumentException));
                Helper.BAToCharThrow(byteArray, byteArray.Length, typeof(ArgumentOutOfRangeException));
                Helper.BAToCharThrow(byteArray, int.MaxValue, typeof(ArgumentOutOfRangeException));
                Helper.BAToCharThrow(new byte[] { }, 1, typeof(ArgumentOutOfRangeException));
                Helper.BAToCharThrow(null, 1, typeof(ArgumentNullException));
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_ToDouble()
        {
            try
            {
                var byteArray = new byte[]{
                  0,   0,   0,   0,   0,   0,   0,   0, 240,  63, 
                  0,   0,   0,   0,   0, 224, 111,  64,   0,   0, 
                224, 255, 255, 255, 239,  65,   0,   0,   0,   0, 
                  0,   0, 112,  63,   0,   0,   0,   0,   0,   0, 
                240,  61, 223, 136,  30,  28, 254, 116, 170,   1, 
                250,  89, 140,  66, 202, 192, 243,  63, 251,  89, 
                140,  66, 202, 192, 243,  63, 252,  89, 140,  66, 
                202, 192, 243,  63,  82, 211, 187, 188, 232, 126, 
                 61, 126, 255, 255, 255, 255, 255, 255, 239, 255, 
                255, 255, 255, 255, 255, 239, 127,   1,   0,   0, 
                  0,   0,   0,   0,   0, 248, 255,   0,   0,   0, 
                  0,   0,   0, 240, 255,   0,   0,   0,   0,   0, 
                  0, 240, 127 };

                Helper.BAToDouble(byteArray, 0, 0.0000000000000000E+000);
                Helper.BAToDouble(byteArray, 2, 1.0000000000000000E+000);
                Helper.BAToDouble(byteArray, 10, 2.5500000000000000E+002);
                Helper.BAToDouble(byteArray, 18, 4.2949672950000000E+009);
                Helper.BAToDouble(byteArray, 26, 3.9062500000000000E-003);
                Helper.BAToDouble(byteArray, 34, 2.3283064365386963E-010);
                Helper.BAToDouble(byteArray, 42, 1.2345678901234500E-300);
                Helper.BAToDouble(byteArray, 50, 1.2345678901234565E+000);
                Helper.BAToDouble(byteArray, 58, 1.2345678901234567E+000);
                Helper.BAToDouble(byteArray, 66, 1.2345678901234569E+000);
                Helper.BAToDouble(byteArray, 74, 1.2345678901234569E+300);
                Helper.BAToDouble(byteArray, 82, double.MinValue);
                Helper.BAToDouble(byteArray, 89, double.MaxValue);
                Helper.BAToDouble(byteArray, 97, double.Epsilon);
                Helper.BAToDouble(byteArray, 99, double.NaN);
                Helper.BAToDouble(byteArray, 107, double.NegativeInfinity);
                Helper.BAToDouble(byteArray, 115, double.PositiveInfinity);

                Helper.BAToDoubleThrow(byteArray, int.MinValue, typeof(ArgumentOutOfRangeException));
                Helper.BAToDoubleThrow(byteArray, -1, typeof(ArgumentOutOfRangeException));
                Helper.BAToDoubleThrow(byteArray, byteArray.Length - 7, typeof(ArgumentException));
                Helper.BAToDoubleThrow(byteArray, byteArray.Length - 1, typeof(ArgumentException));
                Helper.BAToDoubleThrow(byteArray, byteArray.Length, typeof(ArgumentOutOfRangeException));
                Helper.BAToDoubleThrow(byteArray, int.MaxValue, typeof(ArgumentOutOfRangeException));
                Helper.BAToDoubleThrow(new byte[] { }, 1, typeof(ArgumentOutOfRangeException));
                Helper.BAToDoubleThrow(null, 1, typeof(ArgumentNullException));
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_ToInt16()
        {
            try
            {
                var byteArray = new byte[] { 15, 0, 0, 128, 16, 39, 240, 216, 241, 255, 127 };

                Helper.BAToInt16(byteArray, 1, 0);
                Helper.BAToInt16(byteArray, 0, 15);
                Helper.BAToInt16(byteArray, 8, -15);
                Helper.BAToInt16(byteArray, 4, 10000);
                Helper.BAToInt16(byteArray, 6, -10000);
                Helper.BAToInt16(byteArray, 9, short.MaxValue);
                Helper.BAToInt16(byteArray, 2, short.MinValue);

                Helper.BAToInt16Throw(byteArray, int.MinValue, typeof(ArgumentOutOfRangeException));
                Helper.BAToInt16Throw(byteArray, -1, typeof(ArgumentOutOfRangeException));
                Helper.BAToInt16Throw(byteArray, byteArray.Length - 1, typeof(ArgumentException));
                Helper.BAToInt16Throw(byteArray, byteArray.Length, typeof(ArgumentOutOfRangeException));
                Helper.BAToInt16Throw(byteArray, int.MaxValue, typeof(ArgumentOutOfRangeException));
                Helper.BAToInt16Throw(new byte[] { }, 1, typeof(ArgumentOutOfRangeException));
                Helper.BAToInt16Throw(null, 1, typeof(ArgumentNullException));
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_Int32()
        {
            try
            {
                var byteArray = new byte[]{
                 15,   0,   0,   0,   0, 128,   0,   0,  16,   0, 
                  0, 240, 255,   0, 202, 154,  59,   0,  54, 101, 
                196, 241, 255, 255, 255, 127 };

                Helper.BAToInt32(byteArray, 1, 0);
                Helper.BAToInt32(byteArray, 0, 15);
                Helper.BAToInt32(byteArray, 21, -15);
                Helper.BAToInt32(byteArray, 6, 1048576);
                Helper.BAToInt32(byteArray, 9, -1048576);
                Helper.BAToInt32(byteArray, 13, 1000000000);
                Helper.BAToInt32(byteArray, 17, -1000000000);
                Helper.BAToInt32(byteArray, 22, int.MaxValue);
                Helper.BAToInt32(byteArray, 2, int.MinValue);

                Helper.BAToInt32Throw(byteArray, int.MinValue, typeof(ArgumentOutOfRangeException));
                Helper.BAToInt32Throw(byteArray, -1, typeof(ArgumentOutOfRangeException));
                Helper.BAToInt32Throw(byteArray, byteArray.Length - 3, typeof(ArgumentException));
                Helper.BAToInt32Throw(byteArray, byteArray.Length - 1, typeof(ArgumentException));
                Helper.BAToInt32Throw(byteArray, byteArray.Length, typeof(ArgumentOutOfRangeException));
                Helper.BAToInt32Throw(byteArray, int.MaxValue, typeof(ArgumentOutOfRangeException));
                Helper.BAToInt32Throw(new byte[] { }, 1, typeof(ArgumentOutOfRangeException));
                Helper.BAToInt32Throw(null, 1, typeof(ArgumentNullException));
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_Int64()
        {
            try
            {
                var byteArray = new byte[]{
                  0,  54, 101, 196, 255, 255, 255, 255,   0,   0,
                  0,   0,   0,   0,   0,   0, 128,   0, 202, 154, 
                 59,   0,   0,   0,   0,   1,   0,   0,   0,   0, 
                255, 255, 255, 255,   1,   0,   0, 255, 255, 255, 
                255, 255, 255, 255, 127,  86,  85,  85,  85,  85, 
                 85, 255, 255, 170, 170, 170, 170, 170, 170,   0, 
                  0, 100, 167, 179, 182, 224,  13,   0,   0, 156, 
                 88,  76,  73,  31, 242 };

                Helper.BAToInt64(byteArray, 8, 0);
                Helper.BAToInt64(byteArray, 5, 16777215);
                Helper.BAToInt64(byteArray, 34, -16777215);
                Helper.BAToInt64(byteArray, 17, 1000000000);
                Helper.BAToInt64(byteArray, 0, -1000000000);
                Helper.BAToInt64(byteArray, 21, 4294967296);
                Helper.BAToInt64(byteArray, 26, -4294967296);
                Helper.BAToInt64(byteArray, 53, 187649984473770);
                Helper.BAToInt64(byteArray, 45, -187649984473770);
                Helper.BAToInt64(byteArray, 59, 1000000000000000000);
                Helper.BAToInt64(byteArray, 67, -1000000000000000000);
                Helper.BAToInt64(byteArray, 37, long.MaxValue);
                Helper.BAToInt64(byteArray, 9, long.MinValue);

                Helper.BAToInt64Throw(byteArray, int.MinValue, typeof(ArgumentOutOfRangeException));
                Helper.BAToInt64Throw(byteArray, -1, typeof(ArgumentOutOfRangeException));
                Helper.BAToInt64Throw(byteArray, byteArray.Length - 7, typeof(ArgumentException));
                Helper.BAToInt64Throw(byteArray, byteArray.Length - 1, typeof(ArgumentException));
                Helper.BAToInt64Throw(byteArray, byteArray.Length, typeof(ArgumentOutOfRangeException));
                Helper.BAToInt64Throw(byteArray, int.MaxValue, typeof(ArgumentOutOfRangeException));
                Helper.BAToInt64Throw(new byte[] { }, 1, typeof(ArgumentOutOfRangeException));
                Helper.BAToInt64Throw(null, 1, typeof(ArgumentNullException));
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_ToSingle()
        {
            try
            {
                var byteArray = new byte[]{
                  0,   0,   0,   0, 128,  63,   0,   0, 112,  65, 
                  0, 255, 127,  71,   0,   0, 128,  59,   0,   0, 
                128,  47,  73,  70, 131,   5,  75,   6, 158,  63, 
                 77,   6, 158,  63,  80,   6, 158,  63,  30,  55, 
                190, 121, 255, 255, 127, 255, 255, 127, 127,   1, 
                  0,   0,   0, 192, 255,   0,   0, 128, 255,   0, 
                  0, 128, 127 };

                Helper.BAToSingle(byteArray, 0, 0.0000000E+000F);
                Helper.BAToSingle(byteArray, 2, 1.0000000E+000F);
                Helper.BAToSingle(byteArray, 6, 1.5000000E+001F);
                Helper.BAToSingle(byteArray, 10, 6.5535000E+004F);
                Helper.BAToSingle(byteArray, 14, 3.9062500E-003F);
                Helper.BAToSingle(byteArray, 18, 2.3283064E-010F);
                Helper.BAToSingle(byteArray, 22, 1.2345000E-035F);
                Helper.BAToSingle(byteArray, 26, 1.2345671E+000F);
                Helper.BAToSingle(byteArray, 30, 1.2345673E+000F);
                Helper.BAToSingle(byteArray, 34, 1.2345676E+000F);
                Helper.BAToSingle(byteArray, 38, 1.2345679E+035F);
                Helper.BAToSingle(byteArray, 42, float.MinValue);
                Helper.BAToSingle(byteArray, 45, float.MaxValue);
                Helper.BAToSingle(byteArray, 49, float.Epsilon);
                Helper.BAToSingle(byteArray, 51, 0.0F / 0.0F);
                Helper.BAToSingle(byteArray, 55, -1.0F / 0.0F);
                Helper.BAToSingle(byteArray, 59, 1.0F / 0.0F);

                Helper.BAToSingleThrow(byteArray, int.MinValue, typeof(ArgumentOutOfRangeException));
                Helper.BAToSingleThrow(byteArray, -1, typeof(ArgumentOutOfRangeException));
                Helper.BAToSingleThrow(byteArray, byteArray.Length - 3, typeof(ArgumentException));
                Helper.BAToSingleThrow(byteArray, byteArray.Length - 1, typeof(ArgumentException));
                Helper.BAToSingleThrow(byteArray, byteArray.Length, typeof(ArgumentOutOfRangeException));
                Helper.BAToSingleThrow(byteArray, int.MaxValue, typeof(ArgumentOutOfRangeException));
                Helper.BAToSingleThrow(new byte[] { }, 1, typeof(ArgumentOutOfRangeException));
                Helper.BAToSingleThrow(null, 1, typeof(ArgumentNullException));
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_ToUInt16()
        {
            try
            {
                var byteArray = new byte[] { 15, 0, 0, 255, 3, 16, 39, 255, 255, 127 };

                Helper.BAToUInt16(byteArray, 1, 0);
                Helper.BAToUInt16(byteArray, 0, 15);
                Helper.BAToUInt16(byteArray, 3, 1023);
                Helper.BAToUInt16(byteArray, 5, 10000);
                Helper.BAToUInt16(byteArray, 8, (ushort)short.MaxValue);
                Helper.BAToUInt16(byteArray, 7, ushort.MaxValue);

                Helper.BAToUInt16Throw(byteArray, int.MinValue, typeof(ArgumentOutOfRangeException));
                Helper.BAToUInt16Throw(byteArray, -1, typeof(ArgumentOutOfRangeException));
                Helper.BAToUInt16Throw(byteArray, byteArray.Length - 1, typeof(ArgumentException));
                Helper.BAToUInt16Throw(byteArray, byteArray.Length, typeof(ArgumentOutOfRangeException));
                Helper.BAToUInt16Throw(byteArray, int.MaxValue, typeof(ArgumentOutOfRangeException));
                Helper.BAToUInt16Throw(new byte[] { }, 1, typeof(ArgumentOutOfRangeException));
                Helper.BAToUInt16Throw(null, 1, typeof(ArgumentNullException));
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_ToUInt32()
        {
            try
            {
                var byteArray = new byte[]{
                 15,   0,   0,   0,   0,  16,   0, 255,   3,   0, 
                  0, 202, 154,  59, 255, 255, 255, 255, 127 };

                Helper.BAToUInt32(byteArray, 1, 0);
                Helper.BAToUInt32(byteArray, 0, 15);
                Helper.BAToUInt32(byteArray, 7, 1023);
                Helper.BAToUInt32(byteArray, 3, 1048576);
                Helper.BAToUInt32(byteArray, 10, 1000000000);
                Helper.BAToUInt32(byteArray, 15, int.MaxValue);
                Helper.BAToUInt32(byteArray, 14, uint.MaxValue);

                Helper.BAToUInt32Throw(byteArray, int.MinValue, typeof(ArgumentOutOfRangeException));
                Helper.BAToUInt32Throw(byteArray, -1, typeof(ArgumentOutOfRangeException));
                Helper.BAToUInt32Throw(byteArray, byteArray.Length - 3, typeof(ArgumentException));
                Helper.BAToUInt32Throw(byteArray, byteArray.Length - 1, typeof(ArgumentException));
                Helper.BAToUInt32Throw(byteArray, byteArray.Length, typeof(ArgumentOutOfRangeException));
                Helper.BAToUInt32Throw(byteArray, int.MaxValue, typeof(ArgumentOutOfRangeException));
                Helper.BAToUInt32Throw(new byte[] { }, 1, typeof(ArgumentOutOfRangeException));
                Helper.BAToUInt32Throw(null, 1, typeof(ArgumentNullException));
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_ToUInt64()
        {
            try
            {
                var byteArray = new byte[]{
                255, 255, 255,   0,   0,   0,   0,   0,   0,   0, 
                  0,   1,   0,   0,   0, 100, 167, 179, 182, 224, 
                 13,   0, 202, 154,  59,   0,   0,   0,   0, 170, 
                170, 170, 170, 170, 170,   0,   0, 232, 137,   4, 
                 35, 199, 138, 255, 255, 255, 255, 255, 255, 255, 
                255, 127 };

                Helper.BAToUInt64(byteArray, 3, 0);
                Helper.BAToUInt64(byteArray, 0, 16777215);
                Helper.BAToUInt64(byteArray, 21, 1000000000);
                Helper.BAToUInt64(byteArray, 7, 4294967296);
                Helper.BAToUInt64(byteArray, 29, 187649984473770);
                Helper.BAToUInt64(byteArray, 13, 1000000000000000000);
                Helper.BAToUInt64(byteArray, 35, 10000000000000000000);
                Helper.BAToUInt64(byteArray, 44, long.MaxValue);
                Helper.BAToUInt64(byteArray, 43, ulong.MaxValue);

                Helper.BAToUInt64Throw(byteArray, int.MinValue, typeof(ArgumentOutOfRangeException));
                Helper.BAToUInt64Throw(byteArray, -1, typeof(ArgumentOutOfRangeException));
                Helper.BAToUInt64Throw(byteArray, byteArray.Length - 7, typeof(ArgumentException));
                Helper.BAToUInt64Throw(byteArray, byteArray.Length - 1, typeof(ArgumentException));
                Helper.BAToUInt64Throw(byteArray, byteArray.Length, typeof(ArgumentOutOfRangeException));
                Helper.BAToUInt64Throw(byteArray, int.MaxValue, typeof(ArgumentOutOfRangeException));
                Helper.BAToUInt64Throw(new byte[] { }, 1, typeof(ArgumentOutOfRangeException));
                Helper.BAToUInt64Throw(null, 1, typeof(ArgumentNullException));
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_ToString()
        {
            try
            {
                Helper.WriteByteArray(new byte[] { 0, 1, 2, 4, 8, 16, 32, 64, 128, 255 },
                    "00-01-02-04-08-10-20-40-80-FF");

                Helper.WriteByteArray(new byte[] {
                32,   0,   0,  42,   0,  65,   0, 125,   0, 197,
                 0, 168,   3,  41,   4, 172,  32 },
                    "20-00-00-2A-00-41-00-7D-00-C5-00-A8-03-29-04-AC-20");

                Helper.WriteByteArray(new byte[] { 15, 0, 0, 128, 16, 39, 240, 216, 241, 255, 127 },
                    "0F-00-00-80-10-27-F0-D8-F1-FF-7F");

                Helper.WriteByteArray(new byte[] {
                15,   0,   0,   0,   0,  16,   0, 255,   3,   0, 
                 0, 202, 154,  59, 255, 255, 255, 255, 127 },
                     "0F-00-00-00-00-10-00-FF-03-00-00-CA-9A-3B-FF-FF-FF-FF-7F");

                Helper.WriteMultiLineByteArray(new byte[]{
                  0,   0,   0,   0, 128,  63,   0,   0, 112,  65, 
                  0, 255, 127,  71,   0,   0, 128,  59,   0,   0, 
                128,  47,  73,  70, 131,   5,  75,   6, 158,  63, 
                 77,   6, 158,  63,  80,   6, 158,  63,  30,  55, 
                190, 121, 255, 255, 127, 255, 255, 127, 127,   1, 
                  0,   0,   0, 192, 255,   0,   0, 128, 255,   0, 
                  0, 128, 127 },
@"00-00-00-00-80-3F-00-00-70-41-00-FF-7F-47-00-00-80-3B-00-00
80-2F-49-46-83-05-4B-06-9E-3F-4D-06-9E-3F-50-06-9E-3F-1E-37
BE-79-FF-FF-7F-FF-FF-7F-7F-01-00-00-00-C0-FF-00-00-80-FF-00
00-80-7F");

                Helper.WriteMultiLineByteArray(new byte[]{
                255, 255, 255,   0,   0,  20,   0,  33,   0,   0, 
                  0,   1,   0,   0,   0, 100, 167, 179, 182, 224, 
                 13,   0, 202, 154,  59,   0, 143,  91,   0, 170, 
                170, 170, 170, 170, 170,   0,   0, 232, 137,   4, 
                 35, 199, 138, 255, 232, 244, 255, 252, 205, 255, 
                255, 129 },
@"FF-FF-FF-00-00-14-00-21-00-00-00-01-00-00-00-64-A7-B3-B6-E0
0D-00-CA-9A-3B-00-8F-5B-00-AA-AA-AA-AA-AA-AA-00-00-E8-89-04
23-C7-8A-FF-E8-F4-FF-FC-CD-FF-FF-81");

                Helper.WriteMultiLineByteArray(new byte[]{
                  0, 222,   0,   0,   0, 224, 111,  64,   0,   0, 
                224, 255, 255, 255, 239,  65,   0,   0, 131,   0, 
                  0,   0, 112,  63,   0, 143,   0, 100,   0,   0, 
                240,  61, 223, 136,  30,  28, 254, 116, 170,   1, 
                250,  89, 140,  66, 202, 192, 243,  63, 251,  89, 
                140,  66, 202, 192, 243,  63, 252,  89, 140,  66, 
                202, 192, 243,  63,  82, 211, 187, 188, 232, 126, 
                255, 255, 255, 244, 255, 239, 127,   1,   0,   0, 
                  0,  10,  17,   0,   0, 248, 255,   0,  88,   0, 
                 91,   0,   0, 240, 255,   0,   0, 240, 157 },
@"00-DE-00-00-00-E0-6F-40-00-00-E0-FF-FF-FF-EF-41-00-00-83-00
00-00-70-3F-00-8F-00-64-00-00-F0-3D-DF-88-1E-1C-FE-74-AA-01
FA-59-8C-42-CA-C0-F3-3F-FB-59-8C-42-CA-C0-F3-3F-FC-59-8C-42
CA-C0-F3-3F-52-D3-BB-BC-E8-7E-FF-FF-FF-F4-FF-EF-7F-01-00-00
00-0A-11-00-00-F8-FF-00-58-00-5B-00-00-F0-FF-00-00-F0-9D");
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_ToStringEmpty()
        {
            var empty = new byte[] { };

            if (BitConverter.ToString(empty) != string.Empty)
            {
                Log.Comment("BitConverter.ToString({}) doesn't return empty string");
                return MFTestResults.Fail;
            }

            if (BitConverter.ToString(empty, 0) != string.Empty)
            {
                Log.Comment("BitConverter.ToString({}, 0) doesn't return empty string");
                return MFTestResults.Fail;
            }

            if (BitConverter.ToString(empty, 0, 0) != string.Empty)
            {
                Log.Comment("BitConverter.ToString({}, 0, 0) doesn't return empty string");
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults BitConverterTest_ToStringThrow()
        {
            try
            {
                var bytes = new byte[] { 0, 1, 2, 4, 8, 16, 32, 64, 128, 255 };

                Helper.ToStringThrow(null, typeof(ArgumentNullException));
                Helper.ToStringThrow(null, 0, typeof(ArgumentNullException));
                Helper.ToStringThrow(null, 0, 0, typeof(ArgumentNullException));

                Helper.ToStringThrow(bytes, int.MinValue, typeof(ArgumentOutOfRangeException));
                Helper.ToStringThrow(bytes, -1, typeof(ArgumentOutOfRangeException));
                Helper.ToStringThrow(bytes, bytes.Length, typeof(ArgumentOutOfRangeException));
                Helper.ToStringThrow(bytes, int.MaxValue, typeof(ArgumentOutOfRangeException));

                Helper.ToStringThrow(bytes, int.MinValue, 1, typeof(ArgumentOutOfRangeException));
                Helper.ToStringThrow(bytes, -1, 1, typeof(ArgumentOutOfRangeException));
                Helper.ToStringThrow(bytes, bytes.Length, 1, typeof(ArgumentOutOfRangeException));
                Helper.ToStringThrow(bytes, int.MaxValue, 1, typeof(ArgumentOutOfRangeException));
                Helper.ToStringThrow(bytes, 0, -1, typeof(ArgumentOutOfRangeException));
                Helper.ToStringThrow(bytes, 0, int.MinValue, typeof(ArgumentOutOfRangeException));
                Helper.ToStringThrow(bytes, 0, bytes.Length + 1, typeof(ArgumentException));
                Helper.ToStringThrow(bytes, 0, int.MaxValue, typeof(ArgumentException));
                Helper.ToStringThrow(bytes, 1, bytes.Length, typeof(ArgumentException));
                Helper.ToStringThrow(bytes, 1, int.MaxValue, typeof(ArgumentException));
                Helper.ToStringThrow(bytes, bytes.Length - 1, 2, typeof(ArgumentException));
                Helper.ToStringThrow(bytes, bytes.Length - 1, int.MaxValue, typeof(ArgumentException));

                var empty = new byte[] { };
                Helper.ToStringThrow(empty, 1, typeof(ArgumentOutOfRangeException));
                Helper.ToStringThrow(empty, 1, 0, typeof(ArgumentOutOfRangeException));
                Helper.ToStringThrow(empty, 0, 1, typeof(ArgumentOutOfRangeException));
            }
            catch (TestFailException e)
            {
                Log.Comment(e.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }
    }
}
