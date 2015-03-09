////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Text;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public interface ITest
    {
    }

    public class GuidTests : IMFTestInterface, ITest
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        [TestMethod]
        public MFTestResults Guid_Test()
        {
            /// <summary>
            /// 1. Creates a Guid with Guid.NewGuid()  
            /// 2. Verifies the Guid is with Byte Array length 16
            /// 3. Creates same Guid 
            /// 4. Verifies the two Guids are equal with CompareTo
            /// 5. Verifies the two Guids are equal with Equals
            /// 6. Creates another Guid with Guid.NewGuid()
            /// 7. Verifies the new Guid is not equal to the previous Guid with CompareTo
            /// 8. Verifies the new Guid is not equal to the previous Guid with Equal
            /// </summary>
            ///
            try
            {
                Guid theGuid = Guid.NewGuid();

                byte[] bGuid1 = theGuid.ToByteArray();

                if (bGuid1.Length != 16)
                {
                    Log.Comment("Expected length '16' but got '" + bGuid1.Length + "'");
                    return MFTestResults.Fail;
                }

                Guid theSameGuid = new Guid(bGuid1);

                // must be the same
                if (theGuid.CompareTo(theSameGuid) != 0)
                {
                    Log.Comment("theGuid.CompareTo(theSameGuid) Failed");
                    return MFTestResults.Fail;
                }
                if (!theGuid.Equals(theSameGuid))
                {
                    Log.Comment("theGuid.Equals(theSameGuid) Failed");
                    return MFTestResults.Fail;
                }

                Guid anotherGuid = Guid.NewGuid();

                // must be the different
                if (theGuid.CompareTo(anotherGuid) == 0)
                {
                    Log.Comment("theGuid.CompareTo(anotherGuid) Failed");
                    return MFTestResults.Fail;
                }
                if (theGuid.Equals(anotherGuid))
                {
                    Log.Comment("theGuid.Equals(anotherGuid) Failed");
                    return MFTestResults.Fail;
                }

            }
            catch (Exception ex)
            {
                Log.Comment("Caught Exception : " + ex.Message + " creating Guid with Guid.NewGuid()");
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults ByteArrayConstructor_Test2()
        {
            /// <summary>
            /// 1. Creates a Guid with byte Array of size 16 and random byte values
            /// 2. Verifies exception is not thrown
            /// </summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            Byte[] guid16 = MFUtilities.GetRandomBytes(16);
            try
            {
                Guid myGuid1 = new Guid(guid16);
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults ArgumentException_Test3()
        {
            /// <summary>
            /// 1. Creates a Guid with byte Array of random size b/n 0 to 100 but not 16 
            /// 2. Verifies ArgumentException is thrown
            /// </summary>
            ///

            MFTestResults testResult = MFTestResults.Fail;
            int size = 16;

            //size could be set to any Random number b/n 0 and 2147483647
            //System.OutOfMemoryException will be thrown
            Random random = new Random();
            while (size == 16)
            {
                size = random.Next(100);
            }
            Byte[] guidNot16 = MFUtilities.GetRandomBytes(size);
            try
            {
                Guid myGuid1 = new Guid(guidNot16);
            }
            catch (ArgumentException ex)
            {
                Log.Comment("Caught : " + ex.Message + " when trying to create Guid with " + size + " bytes long");
                testResult = MFTestResults.Pass;
            }
            catch (Exception e)
            {
                Log.Comment("Expecting ArgumentException got " + e.Message);
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults ArgumentNullException_Test4()
        {
            /// <summary>
            /// 1. Creates a Guid with byte Array of null  
            /// 2. Verifies ArgumentNullException is thrown
            /// </summary>
            ///

            MFTestResults testResult = MFTestResults.Fail;
            Byte[] nullByte = null;
            try
            {
                Guid myGuid1 = new Guid(nullByte);
            }
            catch (ArgumentNullException ex)
            {
                Log.Comment("Caught : " + ex.Message + " when trying to create Guid with null bytes");
                testResult = MFTestResults.Pass;
            }
            catch (Exception e)
            {
                Log.Comment("Expecting ArgumentNullException got " + e.Message);
            }

            return testResult;
        }

        public static Guid GetGuid()
        {
            return new Guid(0x4dff36b5, 0x9dde, 0x4f76, 0x9a, 0x2a, 0x96, 0x43, 0x50, 0x47, 0x06, 0x3d);
        }

        [TestMethod]
        public MFTestResults Reflection_Unboxing_Test5()
        {
            /// <summary>
            /// 1. Creates a Guid using a method
            /// 2. Invokes the method using Reflection
            /// 3. Casts the returned obj from the method back to Guid
            /// 4. Verifies Exception is not thrown when castin a Guid obj back to Guid
            /// </summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            Guid[] a = new Guid[] { GetGuid(), GetGuid(), GetGuid() };

            IList list = (IList)a;

            Guid g = (Guid)list[1];

            Log.Comment(g.ToString());
            Type t = Type.GetType("Microsoft.SPOT.Platform.Tests.ITest");

            Type[] apps = Reflection.GetTypesImplementingInterface(t);

            foreach (Type app in apps)
            {
                MethodInfo method = app.GetMethod("GetGuid", BindingFlags.Static | BindingFlags.Public);
                if (method != null)
                {
                    object o = method.Invoke(null, null);
                    try
                    {
                        Guid guid = (Guid)o;
                    }
                    catch (Exception ex)
                    {
                        Log.Comment("Caught : " + ex.Message);
                        testResult = MFTestResults.Fail;
                    }
                }
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults Int_Short_Constructor_Test6()
        {
            /// <summary>
            /// 1. Creates Guid(int, short, short, byte, byte ...) 
            /// 2. Verifies exception is not thrown 
            /// </summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            Random random = new Random();
            int _int = random.Next(Int32.MaxValue);
            short _short1 = (short)random.Next(32768);
            short _short2 = (short)random.Next(32768);
            Byte[] _bArr = MFUtilities.GetRandomBytes(8);

            try
            {
                Guid _guid = new Guid(_int, _short1, _short2, _bArr[0], _bArr[1], _bArr[2], _bArr[3], _bArr[4], _bArr[5], _bArr[6], _bArr[7]);
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message + "when creating Guid(" + _int +
                             ", " + _short1 + ", " + _short2 + ", " + _bArr[0] +
                             ", " + _bArr[1] + ", " + _bArr[2] + ", " + _bArr[3] + ", " +
                             _bArr[4] + ", " + _bArr[5] + ", " + _bArr[6] + ", " + _bArr[7] + ")");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults UInt_Ushort_Constructor_Test7()
        {
            /// <summary>
            /// 1. Creates a Guid(uint, ushort, byte, byte ...)
            /// 2. Verifies exception is not thrown 
            /// </summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            Random random = new Random();
            int randoInt = random.Next(Int32.MaxValue);
            uint _uInt = (uint)(randoInt * 2);
            ushort _uShort1 = (ushort)random.Next(65536);
            ushort _uShort2 = (ushort)random.Next(65536);
            Byte[] _bArr = MFUtilities.GetRandomBytes(8);

            try
            {
                Guid _guid = new Guid(_uInt, _uShort1, _uShort1, _bArr[0], _bArr[1], _bArr[2], _bArr[3], _bArr[4], _bArr[5], _bArr[6], _bArr[7]);
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message + "when creating Guid(" + _uInt +
                            ", " + _uShort1 + ", " + _uShort1 + ", " + _bArr[0] +
                            ", " + _bArr[1] + ", " + _bArr[2] + ", " + _bArr[3] + ", " +
                            _bArr[4] + ", " + _bArr[5] + ", " + _bArr[6] + ", " + _bArr[7] + ")");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Guid_Empty_Test8()
        {
            /// <summary>
            /// 1. Creates an Empty Guid with Guid.Empty
            /// 2. Extracts all the bytes in the Guid
            /// 3. Verifies all bytes have zero (0) value
            /// </summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            Guid guid = Guid.Empty;
            Byte[] _bArr = guid.ToByteArray();
            for (int i = 0; i < 16; i++)
            {
                if (_bArr[i] != 0)
                {
                    Log.Comment("Expecting '0' got '" + _bArr[i] + "' at index " + i.ToString());
                    testResult = MFTestResults.Fail;
                    break;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Guid_CompareTo_Test9()
        {
            /// <summary>
            /// 1. Creates Guids with different values 
            /// 2. Verifies their equality using CompareTo
            /// </summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            Guid guid1 = Guid.Empty;
            Log.Comment("Verifing any instance of Guid, regardless of its value, is greater than null");
            if (guid1.CompareTo(null) <= 0)
            {
                Log.Comment("Expected : " + guid1.ToString() + " is greater than null");
                testResult = MFTestResults.Fail;
            }
            Byte[] _bArr = new Byte[16];
            Log.Comment("Creating a Guid with all bytes zero");
            Guid guid2 = new Guid(_bArr);
            if (guid1.CompareTo(guid2) != 0)
            {
                Log.Comment("Expected : " + guid1.ToString() + " equals " + guid2.ToString());
                testResult = MFTestResults.Fail;
            }
            Guid guid3 = new Guid(0x4dff36b5, 0x9dde, 0x4f76, 0x9a, 0x2a, 0x96, 0x43, 0x50, 0x47, 0x06, 0x3d);
            if (guid3.CompareTo(guid1) <= 0)
            {
                Log.Comment("Expected : " + guid3.ToString() + " is greater than " + guid1.ToString());
                testResult = MFTestResults.Fail;
            }
            Guid guid4 = new Guid(0x4dff36b5, 0x9dde, 0x4f76, 0x9a, 0x2a, 0x96, 0x43, 0x50, 0x47, 0x06, 0x3d);
            if (guid4.CompareTo(guid3) != 0)
            {
                Log.Comment("Expected : " + guid4.ToString() + " is equal to " + guid3.ToString());
                testResult = MFTestResults.Fail;
            }
            Guid guid5 = new Guid(0x4dff36b5, 0x9dde, 0x4f76, 0x9a, 0x2a, 0x96, 0x43, 0x50, 0x47, 0x06, 0x3e);
            if (guid5.CompareTo(guid4) <= 0)
            {
                Log.Comment("Expected : " + guid5.ToString() + " is greater than " + guid4.ToString());
                testResult = MFTestResults.Fail;
            }
            if (guid4.CompareTo(guid5) >= 0)
            {
                Log.Comment("Expected : " + guid4.ToString() + " is less than " + guid5.ToString());
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Guid_ToString_Test10()
        {
            /// <summary>
            /// 1. Creates 4 Guids and Converts them ToString 
            /// 2. Verifies the Conversion of Guids to string is correct
            /// </summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;

            String[] strArr1 = new String[] { "00000000-0000-0000-0000-000000000000",
                "00000000-0000-0000-0000-000000000000",
                "4dff36b5-9dde-4f76-9a2a-96435047063d",
                "ffffffff-ffff-ffff-ffff-ffffffffffff"};
            Guid guid1 = Guid.Empty;
            Byte[] _byteArr1 = new Byte[16];
            Guid guid2 = new Guid(_byteArr1);
            Guid guid3 = new Guid(0x4dff36b5, 0x9dde, 0x4f76, 0x9a, 0x2a, 0x96, 0x43, 0x50, 0x47, 0x06, 0x3d);
            Byte[] _byteArr2 = new Byte[16];
            for (int i = 0; i < _byteArr2.Length; i++)
            {
                _byteArr2[i] = Byte.MaxValue;
            }
            Guid guid4 = new Guid(_byteArr2);
            String[] strArr2 = new String[] { guid1.ToString(), guid2.ToString(), guid3.ToString(), guid4.ToString() };
            for (int i = 0; i < strArr1.Length; i++)
            {
                Log.Comment(strArr1[i]);
                if (String.Compare(strArr1[i], strArr2[i]) != 0)
                {
                    Log.Comment("Expecting '" + strArr1[i] + "' got '" + strArr2[i] + "'");
                    testResult = MFTestResults.Fail;
                    break;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Guid_Equals_Test11()
        {
            /// <summary>
            /// 1. Creates 3 Arrays of Guids with different constructors same value 
            /// 2. Verifies the Guids are equal
            /// </summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;

            Log.Comment("Creating 3 Guids with Guid.Empty and hex values");
            Guid guid11 = Guid.Empty;
            Guid guid12 = new Guid(0x4dff36b5, 0x9dde, 0x4f76, 0x9a, 0x2a, 0x96, 0x43, 0x50, 0x47, 0x06, 0x3d);
            Guid guid13 = new Guid(0x7FFFFFFF, 0x7FFF, 0x7FFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
            Guid[] gArr1 = new Guid[] { guid11, guid12, guid13 };

            Log.Comment("Creating Guids with 16 bytes constructor");
            Byte[] _bArr1 = new Byte[16];
            Guid guid21 = new Guid(_bArr1);
            Byte[] _bArr2 = new Byte[] { 181, 54, 255, 77, 222, 157, 118, 79, 154, 42, 150, 67, 80, 71, 6, 61 };
            Guid guid22 = new Guid(_bArr2);
            Byte[] _bArr3 = new Byte[] { 255, 255, 255, 127, 255, 127, 255, 127, 255, 255, 255, 255, 255, 255, 255, 255 };
            Guid guid23 = new Guid(_bArr3);
            Guid[] gArr2 = new Guid[] { guid21, guid22, guid23 };

            Log.Comment("Creating 3 Guids with Guid(int, short, short, byte ....) constructor");
            Guid guid31 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            Guid guid32 = new Guid(1308571317, 40414, 20342, 154, 42, 150, 67, 80, 71, 6, 61);
            Guid guid33 = new Guid(int.MaxValue, short.MaxValue, short.MaxValue, byte.MaxValue, byte.MaxValue,
                byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
            Guid[] gArr3 = new Guid[] { guid31, guid32, guid33 };

            for (int i = 0; i < 3; i++)
            {
                if ((!gArr1[i].Equals(gArr2[i])) || (gArr1[i].GetHashCode() != gArr2[i].GetHashCode()))
                {
                    Log.Comment(i + " Expecting : " + gArr1[i].ToString() + " equals " + gArr2[i].ToString() + " comparing 1 and 2");
                    Log.Comment(" Expecting :  hashcode " + gArr1[i].GetHashCode().ToString() + " equals " + gArr2[i].GetHashCode().ToString());
                    testResult = MFTestResults.Fail;
                }
                if ((!gArr1[i].Equals(gArr3[i])) || (gArr1[i].GetHashCode() != gArr3[i].GetHashCode()))
                {
                    Log.Comment(i + " Expecting : " + gArr1[i].ToString() + " equals " + gArr3[i].ToString() + " comparing 1 and 3");
                    Log.Comment(" Expecting :  hashcode " + gArr1[i].GetHashCode().ToString() + " equals " + gArr3[i].GetHashCode().ToString());
                    testResult = MFTestResults.Fail;
                }
                if ((!gArr2[i].Equals(gArr3[i])) || (gArr2[i].GetHashCode() != gArr3[i].GetHashCode()))
                {
                    Log.Comment(i + " Expecting : " + gArr2[i].ToString() + " equals " + gArr3[i].ToString() + " comparing 2 and 3");
                    Log.Comment(" Expecting :  hashcode " + gArr2[i].GetHashCode().ToString() + " equals " + gArr3[i].GetHashCode().ToString());
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }      
    }
}
