////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;


namespace Microsoft.SPOT.Platform.Tests
{
    public class SystemTypeTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            // Add your functionality here.   

            Log.Comment("These tests examine the System.Type methods and properties");    
            
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
        }

        //SystemType Test methods
        interface iEmpty{ }

        class TestObject1{ }

        class TestObject2 : iEmpty
        {
            public TestObject2(Int32 i)
            {
                m_data = (int)i;
            }
            public int m_data;
            class m_subTestObject
            { 
            
            }
            public void Method1()
            { 
            
            }
            public Int32 Method2(Int32 I)
            {
                return I;
            }
        }

        [TestMethod]
        public MFTestResults Number_ToString_Test()
        {
            bool bRet = true;

            // if the negative number can not be truncated to the range specified it
            // will display its full hex value
            bRet &=               "FE" == ((sbyte )                -2).ToString("x02");
            bRet &=               "36" == ((byte  )              0x36).ToString("x02");
            bRet &=               "FF" == ((byte  )               255).ToString("X2");
            bRet &=             "FFFD" == ((short )                -3).ToString("x04");
            bRet &=             "3049" == ((ushort)            0x3049).ToString("x4");
            bRet &=             "FC00" == ((short )             -1024).ToString("x02");
            bRet &=         "FFFFFFFC" == ((int   )                -4).ToString("x8");
            bRet &=         "00004494" == ((uint  )            0x4494).ToString("x8");
            bRet &=         "FFFFFFFC" == ((int   )                -4).ToString("x04");
            bRet &= "FFFFFFFFFFFFFFFB" == ((long  )                -5).ToString("x016");
            bRet &= "1234567890123456" == ((ulong )0x1234567890123456).ToString("x16");
            // you should not be able to truncate the value only leading zeros
            bRet &= "1234567890123456" == ((ulong )0x1234567890123456).ToString("x06");
            bRet &=   "34567890123456" == ((ulong )0x0034567890123456).ToString("x14");

            string tst = 3210.ToString("D");
            bRet &= "3210" == tst;

            tst = (-3210).ToString("d");
            bRet &= "-3210" == tst;

            tst = 3210.ToString("d06");
            bRet &= "003210" == tst;

            tst = (-3210).ToString("d06");
            bRet &= "-003210" == tst;

            tst = 3210.ToString("d1");
            bRet &= "3210" == tst;

            tst = (-3210).ToString("d1");
            bRet &= "-3210" == tst;

            tst = 3210.ToString("g");
            bRet &= "3210" == tst;

            tst = (-3210).ToString("g");
            bRet &= "-3210" == tst;

            bRet &= "NaN" == ((float)0f / 0f).ToString();
            bRet &= "Infinity" == ((float)1f / 0f).ToString();
            bRet &= "-Infinity" == ((float)-1f / 0f).ToString();

            bRet &= "NaN" == ((double)0f / 0f).ToString();
            bRet &= "Infinity" == ((double)1f / 0f).ToString();
            bRet &= "-Infinity" == ((double)-1f / 0f).ToString();

            if (bRet)
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults SystemType1_GetType_Test()
        {
            /// <summary>
            ///  1. Tests the GetType(String) method
            ///  2. Tests the GetType() method
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                Int32 testInt32 = 0;

                Assembly Int32Assm = Assembly.Load("mscorlib");

                Log.Comment("This tests the Assembly.GetType(String) by passing \"Namespace.Class\"");
                Type myType0 = Int32Assm.GetType("System.Int32");
                Log.Comment("The full name is " + myType0.FullName);
                testResult &= (myType0 == testInt32.GetType());

                Log.Comment("This tests the Type.GetType(String) by passing \"Namespace.Class\"");
                Type myType1 = Type.GetType("System.Int32");
                Log.Comment("The full name is " + myType1.FullName);
                testResult &= (myType1 == testInt32.GetType());

                Log.Comment("This tests the Type.GetType(String) by passing \"Namespace.Class, assembly\"");
                Type myType2 = Type.GetType("System.Int32, mscorlib");
                Log.Comment("The full name is " + myType2.FullName);
                testResult &= (myType2 == testInt32.GetType());

                Log.Comment("This tests the Type.GetType(String) by passing \"Namespace.Class, assembly, Version=\"a.b.c.d\"\"");
                string typeName3 = "System.Int32, mscorlib, Version=" + Int32Assm.GetName().Version.ToString();
                Type myType3 = Type.GetType(typeName3);
                Log.Comment("The full name is " + myType3.FullName);
                testResult &= (myType3 == testInt32.GetType());


                Log.Comment("This tests the Type.GetType() method for nested classes");
                TestObject1 testTestObject1 = new TestObject1();
                Type myType4 = testTestObject1.GetType();
                Log.Comment("The full name is " + myType4.FullName);
                testResult &= (myType4 == Type.GetType("Microsoft.SPOT.Platform.Tests.SystemTypeTests+TestObject1"));


                Log.Comment("Since NoneSuch does not exist in this assembly, ");
                Log.Comment("GetType throws a TypeLoadException.");
                Type myType5 = Type.GetType("NoneSuch");
                Log.Comment("The full name is " + myType5.FullName);
            }
            catch (Exception e)
            {
                Log.Comment("Typeless " + e.Message);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SystemType1_Type_Names_Test()
        {
            /// <summary>
            ///  1. Tests the Name(String) method
            ///  2. Tests the AssemblyQualifiedName() method
            /// </summary>
            ///

            bool testResult = false;
            try
            {
                Assembly Int32Assm = Assembly.Load("mscorlib");

                // types must be the same whereexver they come from
                Type myType0 = Int32Assm.GetType("System.Int32");            
                Type myType1 = Type.GetType("System.Int32");

                if (myType0 == myType1)
                {
                    // names must be compatible and composable
                    if (myType0.Name == myType1.Name)
                    {
                    // names must be compatible and composable
                        if (myType0.FullName == myType1.FullName)
                        {
                            // assembly must be the same
                            if (myType0.Assembly == myType1.Assembly)
                            {
                                // type must come from assembly it is supposed to come from
                                if (Int32Assm == myType0.Assembly)
                                {
                                    // verify that the long name is corrent
                                    string fullAssmName = Int32Assm.FullName;

                                    if (myType0.AssemblyQualifiedName == (myType0.FullName + ", " + fullAssmName))
                                    {
                                        testResult = true;
                                    }
                                }
                            }
                        }
                    }
                }
            
            }
            catch (Exception e)
            {
                Log.Comment("Exception caught " + e.Message);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SystemType2_Assembly_Test()
        {
            /// <summary>
            ///  1. Tests the Assembly property for system and user defined types
            /// Fails if exception thrown
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                Log.Comment("This tests the Assembly property");
                
                //Assigned and manipulated to avoid compiler warning
                Int32 testInt32 = -1;
                testInt32++;

                Type myType1 = Type.GetType("System.Int32");
                Log.Comment("The full name is " + myType1.Assembly.FullName);
                testResult &= (myType1.Assembly.FullName == "mscorlib, Version=" + myType1.Assembly.GetName().Version.ToString());

                TestObject1 testTestObject1 = new TestObject1();
                Type myType2 = testTestObject1.GetType();
                Log.Comment("The full name is " + myType2.Assembly.FullName);
                testResult &= (myType2.Assembly.FullName == "Microsoft.SPOT.Platform.Tests.Systemlib2, Version=" + myType2.Assembly.GetName().Version.ToString());
            }
            catch (Exception e)
            {
                Log.Comment("Typeless " + e.Message); 
                testResult = false;
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SystemType3_BaseType_Test()
        {
            /// <summary>
            ///  1. Tests the BaseType property for system and user defined types
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                Log.Comment("This tests the BaseType() method");
                
                //Assigned and manipulated to avoid compiler warning
                Int32 testInt32 = -1;
                testInt32++;
                Type myType1 = Type.GetType("System.Int32");
                Log.Comment("The full name is " + myType1.FullName);
                testResult &= (myType1.BaseType == Type.GetType("System.ValueType"));

                TestObject1 testTestObject1 = new TestObject1();
                Type myType2 = testTestObject1.GetType();
                Log.Comment("The full name is " + myType2.FullName);
                testResult &= (myType2.BaseType == Type.GetType("System.Object"));

            }
            catch (Exception e)
            {
                Log.Comment("Typeless " + e.Message); 
                testResult = false;
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SystemType4_DeclaringType_Test()
        {
            /// <summary>
            ///  1. Tests the BaseType DeclaringType for system and user defined types
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                Log.Comment("This tests the DeclaringType property");
                
                //Assigned and manipulated to avoid compiler warning
                Int32 testInt32 = -1;
                testInt32++;

                Type myType1 = Type.GetType("System.Int32");
                Log.Comment("The full name is " + myType1.FullName);
                testResult &= (myType1.DeclaringType == null);

                TestObject1 testTestObject1 = new TestObject1();
                Type myType2 = testTestObject1.GetType();
                Log.Comment("The full name is " + myType2.FullName);
                Type myType3 = this.GetType();
                testResult &= (myType2.DeclaringType ==  myType3);
            }
            catch (Exception e)
            {
                Log.Comment("Typeless " + e.Message); 
                testResult = false;
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SystemType5_GetConstructor_Test()
        {
            /// <summary>
            ///  1. Tests the GetConstructor(Type[]) method for a user defined type
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                Log.Comment("This tests the GetConstructor(Type[]) method");
                TestObject2 testTestObject2 = new TestObject2(5);
                Type myType2 = testTestObject2.GetType();
                Log.Comment("The full name is " + myType2.FullName);
                Type[] typeOfInt32Arr = new Type[] { Type.GetType("System.Int32") };
                object[] value5Arr = new object[] { 5 };
                TestObject2 testTestObject3 = 
                    (TestObject2)myType2.GetConstructor(typeOfInt32Arr).Invoke(value5Arr);
                testResult &= (testTestObject2.m_data == testTestObject3.m_data);

            }
            catch (Exception e)
            {
                Log.Comment("Typeless " + e.Message); 
                testResult = false;
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SystemType6_GetElementType_Test()
        {
            /// <summary>
            ///  1. Tests the GetElementType() method for a user defined type
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                Log.Comment("This tests the GetElementType() method");
                
                //Assigned and manipulated to avoid compiler warning
                Int32 testInt32 = -1;
                testInt32++;

                Type myType1 = Type.GetType("System.Int32");
                Log.Comment("The full name is " + myType1.FullName);
                Int32[] int32Arr = new Int32[] { };
                Type int32ArrType = int32Arr.GetType();
                testResult &= (myType1 == int32ArrType.GetElementType());

                testResult &= (myType1.GetElementType() == null); 
            }
            catch (Exception e)
            {
                Log.Comment("Typeless " + e.Message); 
                testResult = false;
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SystemType7_GetField_Test()
        {
            /// <summary>
            ///  1. Test the GetField(String) method
            ///  2. Test the GetField(String,BindingFlags) method
            ///  TODO: Expand test #2 once bug 17246 is resolved
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                Log.Comment("This tests the GetField(String) ");
                Log.Comment("and the GetField(String,BindingFlags) methods)");
                Log.Comment("Currently this test fails, see 17246 for more details.");

                Type myType1 = Type.GetType("System.Int32");
                Log.Comment("The full name is " + myType1.FullName);
                testResult &= (myType1.GetField("m_data") == null);

                Log.Comment(" TestObject2 type has one data member \"m_data\" of type Int32.");
                TestObject2 testTestObject2 = new TestObject2(5);
                Type myType2 = testTestObject2.GetType();           
                Log.Comment("The full name is " + myType2.FullName);
                
                Log.Comment(" Check that type of m_data is Int32");
                testResult &= (myType2.GetField("m_data", BindingFlags.GetField | 
                    BindingFlags.Public | BindingFlags.Instance).FieldType == myType1);

                Log.Comment(" Check that value in m_data is 5 ( becuase we called new TestObject2(5))");
                testResult &= ((int)myType2.GetField("m_data", BindingFlags.IgnoreCase | 
                    BindingFlags.GetField | BindingFlags.Public | 
                    BindingFlags.Instance).GetValue(testTestObject2) == 5);
                if ( !testResult ) { Log.Exception( " Last test failed " ); }
                                                                          
                Log.Comment(" Check that m_data  is a field");
                testResult &= (myType2.GetField("m_data").MemberType == MemberTypes.Field);
                if (!testResult) { Log.Exception(" Last test failed "); }

                Log.Comment(" Check that field m_data has Name \"m_data\"");
                testResult &= (myType2.GetField("m_data").Name == "m_data");
                if (!testResult) { Log.Exception(" Last test failed "); }

                Log.Comment(" Check that  misspelling of m_data return NULL.");
                testResult &= null == myType2.GetField("data");
                if (!testResult) { Log.Exception(" Last test failed "); }

                Log.Comment(" Checks that case misspelling of m_data return NULL if flag BindingFlags.IgnoreCase not specified."); 
                testResult &= null == myType2.GetField("m_Data");
                if (!testResult) { Log.Exception(" Last test failed "); }

                Log.Comment("Check retrieval with BindingFlags.IgnoreCase. If flag BindingFlags.IgnoreCase is ised, then the case should be ignored. We should get the same type information."); 
                FieldInfo fInfo_m_Data = myType2.GetField("m_Data", BindingFlags.IgnoreCase | BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance);
                FieldInfo fInfo_m_data = myType2.GetField("m_data");
                testResult &= fInfo_m_Data != null && fInfo_m_data != null;
                testResult &= fInfo_m_Data.Name.Equals( fInfo_m_data.Name );
                if (!testResult) { Log.Exception(" Last test failed "); }

                Log.Comment(" Indirectly set m_data in testTestObject2 to 6 and then check it.");
                myType2.GetField("m_data").SetValue(testTestObject2, 6);
                testResult &= ((int)myType2.GetField("m_data").GetValue(testTestObject2) == 6);
                testResult &= testTestObject2.m_data == 6;
                if (!testResult) { Log.Exception(" Last test failed "); }
            }
            // In case of exceptions we log it and set testResult = false;
            catch (NullReferenceException e)
            {
                Log.Comment("NullRef " + e.Message);
                testResult = false;
            }
            catch (Exception e)
            {
                Log.Comment("Typeless " + e.Message); 
                testResult = false;
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SystemType8_GetFields_Test()
        {
            /// <summary>
            ///  1. Test the GetFields(String) method
            ///  2. Test the GetFields(String,BindingFlags) method
            ///  This test is currently a stub
            ///  TODO: Write test #2 once bug 17246 is resolved
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                Log.Comment("This tests the GetFields(String) method");
                Log.Comment("This test must be re-written once BindingFlags is working, ");
                Log.Comment("see 17246 for more details.");

                Type myType1 = Type.GetType("System.Int32");
                Log.Comment("The full name is " + myType1.FullName);
                testResult &= (myType1.GetField("m_data") == null);
                TestObject2 testTestObject2 = new TestObject2(5);
                Type myType2 = testTestObject2.GetType();
                Log.Comment("The full name is " + myType2.FullName);
                testResult &= (myType2.GetField("m_data").FieldType == myType1);
                testResult &= ((int)myType2.GetField("m_data").GetValue(testTestObject2) == 5);
                testResult &= (myType2.GetField("m_data").MemberType == MemberTypes.Field);
                testResult &= (myType2.GetField("m_data").Name == "m_data");
                myType2.GetField("m_data").SetValue(testTestObject2, 6);
                testResult &= ((int)myType2.GetField("m_data").GetValue(testTestObject2) == 6);
            }
            catch (Exception e)
            {
                Log.Comment("Typeless " + e.Message); testResult = false;
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }


        [TestMethod]
        public MFTestResults SystemType9_GetInterfaces_Test()
        {
            /// <summary>
            ///  1. Test the GetInterfaces(String) method
            ///  2. Test the GetInterfaces(String,BindingFlags) method
            ///  This test is currently a stub
            ///  TODO: Write test #2 once bug 17246 is resolved
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                Log.Comment("This tests the GetInterfaces() method");
                Log.Comment("This test must be re-written once BindingFlags is working, ");
                Log.Comment("see 17246 for more details.");

                Type myType1 = Type.GetType("System.Int32");
                Log.Comment("The full name is " + myType1.FullName);
                testResult &= (myType1.GetInterfaces().Length == 0);
                TestObject2 testTestObject2 = new TestObject2(5);
                Type myType2 = testTestObject2.GetType();
                Log.Comment("The full name is " + myType2.FullName);
                Type myType3 =
                    Type.GetType("Microsoft.SPOT.Platform.Tests.SystemTypeTests+iEmpty");
                testResult &= (myType2.GetInterfaces()[0] == myType3);
            }
            catch (Exception e)
            {
                Log.Comment("Typeless " + e.Message); testResult = false;
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SystemType10_GetMethod_Test()
        {
            /// <summary>
            ///  1. Test the GetMethod(String) method
            ///  2. Test the GetMethod(String,BindingFlags) method
            ///  This test is currently a stub
            ///  TODO: Write test #2 once bug 17246 is resolved
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                Log.Comment("This tests the GetMethod(String) method");
                Log.Comment("This test must be re-written once BindingFlags is working, ");
                Log.Comment("see 17246 for more details.");

                Int32 I = 0;
                I++;
                TestObject2 testTestObject2 = new TestObject2(5);
                Type myType2 = testTestObject2.GetType();
                Log.Comment("The full name is " + myType2.FullName);
                MethodInfo methodInfo1 = myType2.GetMethod("Method2");
                testResult &= (methodInfo1.IsAbstract == false);
                testResult &= (methodInfo1.IsFinal == false);
                testResult &= (methodInfo1.IsPublic == true);
                testResult &= (methodInfo1.IsStatic == false);
                testResult &= (methodInfo1.IsVirtual == false);
                testResult &= (methodInfo1.MemberType == MemberTypes.Method);
                testResult &= (methodInfo1.Name == "Method2");
                testResult &= (methodInfo1.ReturnType == I.GetType());
                testResult &= (methodInfo1.DeclaringType == myType2);
                testResult &=
                    ((int)(methodInfo1.Invoke(testTestObject2, new object[] { 1 })) == 1);
            }
            catch (Exception e)
            {
                Log.Comment("Typeless " + e.Message); testResult = false;
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
        [TestMethod]
        public MFTestResults SystemType11_GetMethods_Test()
        {
            /// <summary>
            ///  1. Test the GetMethods(String) method
            ///  2. Test the GetMethods(String,BindingFlags) method
            ///  This test is currently a stub
            ///  TODO: Write test #2 once bug 17246 is resolved
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                Log.Comment("This tests the GetMethods() method");
                Log.Comment("This test must be re-written once BindingFlags is working, ");
                Log.Comment("see 17246 for more details.");

                //Assigned and manipulated to avoid compiler warning
                Int32 I = 0;
                I++;

                TestObject2 testTestObject2 = new TestObject2(5);
                Type myType2 = testTestObject2.GetType();
                Log.Comment("The full name is " + myType2.FullName);
                MethodInfo[] methodInfoArr1 = myType2.GetMethods();
                MethodInfo methodInfo1 = null;
                if (methodInfoArr1[0].Name == "Method2")
                {
                    methodInfo1 = methodInfoArr1[0];
                    Log.Comment("Method2 found in position 0");
                }
                else if (methodInfoArr1[1].Name == "Method2")
                {
                    methodInfo1 = methodInfoArr1[1];
                    Log.Comment("Method2 found in position 1");
                }
                testResult &= (methodInfo1.IsAbstract == false);
                testResult &= (methodInfo1.IsFinal == false);
                testResult &= (methodInfo1.IsPublic == true);
                testResult &= (methodInfo1.IsStatic == false);
                testResult &= (methodInfo1.IsVirtual == false);
                testResult &= (methodInfo1.MemberType == MemberTypes.Method);
                testResult &= (methodInfo1.Name == "Method2");
                testResult &= (methodInfo1.ReturnType == I.GetType());
                testResult &= (methodInfo1.DeclaringType == myType2);
                testResult &=
                    ((int)(methodInfo1.Invoke(testTestObject2, new object[] { 1 })) == 1);
            }
            catch (Exception e)
            {
                Log.Comment("Typeless " + e.Message); testResult = false;
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
        [TestMethod]
        public MFTestResults SystemType12_InvokeMember_Test()
        {
            /// <summary>
            ///  1. Test the InvokeMember(String,BindingFlags) method
            ///  This test is currently a stub
            ///  TODO: expand test #1 once bug 17246 is resolved
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                Log.Comment("This tests the InvokeMember(String,BindingFlags) method");
                Log.Comment("This test must be re-written once BindingFlags is working, ");
                Log.Comment("see 17246 for more details.");
                //Assigned and manipulated to avoid compiler warning
                Int32 I = 0;
                I++;
                TestObject2 testTestObject2 = new TestObject2(5);
                Type myType2 = testTestObject2.GetType();
                Log.Comment("The full name is " + myType2.FullName);
                testResult &= ((int)myType2.InvokeMember("Method2", BindingFlags.Default
                    | BindingFlags.InvokeMethod, null, testTestObject2,
                    new object[] { -6 }) == -6);
            }
            catch (NotImplementedException)
            {
                return MFTestResults.KnownFailure;
            }
            catch (Exception e)
            {
                Log.Comment("Typeless " + e.Message); testResult = false;
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
    }
}
