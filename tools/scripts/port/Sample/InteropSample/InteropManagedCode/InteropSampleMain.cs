////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Threading;
using Microsoft.SPOT.Interop;
using Microsoft.SPOT.Hardware;
using System;



namespace Microsoft.SPOT.Sample
{
    public class HelloWorldSample
    {
        // Tests that adding of static array to managed code produces valid marshaling code.
        // There was a bug that static array here created invalid Interop generated code.
        public static byte[] staticArrayTest = new byte[] { 0x73, 0x03, 0xb1, 0x88, 0xc5, 0x2d, 0x06, 0xe7, 0xd0, 0x12, 0x4e, 0x75, 0xa6, 0x20, 0x64, 0x53, 0xf9, 0x9c, 0xaf, 0xfb };


        
        
        
        static bool AreFloatValuesEqaul( double a, double b )
        {
            if( a - 0.0001 < b && a + 0.0001 > b )
            {
                return true;
            }
            return false;
        }
                
        static bool TestSignedValues()
        {
            bool bSuccess = true;

            // Declare parameters for test functions.
            char param1 = 'a';
            sbyte param2 = 1;
            short param3 = 2;
            int param4 = 3;
            long param5 = 4;

            long correctVal = param1 + param2 + param3 + param4 + param5;

            long retVal = TestBasicTypes.Add_SignedIntegralValuesByRef( ref param1, ref param2, ref param3, ref param4, ref param5 );
            if( correctVal != retVal )
            {
                Debug.Print( "Error in Add_SignedIntegralValuesByRef - rRetVal is " + retVal + " Should be " + correctVal + "\n" );
                bSuccess = false;
            }
            else
            {
                Debug.Print( "Add_SignedIntegralValuesByRef - succeeded\n" );
            }

            retVal = TestBasicTypes.Add_SignedIntegralValues( 'a', 1, 2, 3, 4 );
            if( correctVal != retVal )
            {
                Debug.Print( "Error in Add_SignedIntegralValues - rRetVal is " + retVal + " Should be " + correctVal + "\n" );
                bSuccess = false;
            }
            else
            {
                Debug.Print( "Add_SignedIntegralValues - succeeded\n" );
            }

            // Circle data passed by reference in varialbles
            TestBasicTypes.Circle_SignedIntegralValues( ref param1, ref param2, ref param3, ref param4, ref param5 );

            // Should be circular shifted data
            if( param1 != 1 || param2 != 2 || param3 != 3 || param4 != 4 || param5 != 'a' )
            {
                Debug.Print( "Error in Circle_SignedIntegralValues\n" );
                bSuccess = false;
            }
            else
            {
                Debug.Print( "Circle_SignedIntegralValues - succeeded\n" );
            }

            return bSuccess;
        }

        static bool TestUnsignedValues()
        {
            bool bSuccess = true;

            // Declare parameters for test functions.
            bool param1 = true;
            byte param2 = 1;
            ushort param3 = 2;
            uint param4 = 3;
            ulong param5 = 4;

            ulong correctVal = (param1 ? 1u : 0u) + param2 + param3 + param4 + param5;
            ulong retVal = (ulong)TestBasicTypes.Add_UnsignedIntegralValuesByRef( ref param1, ref param2, ref param3, ref param4, ref param5 );
            if( correctVal != retVal )
            {
                Debug.Print( "Error in Add_UnsignedIntegralValues by refecrence - rRetVal is " + retVal + " Should be " + correctVal + "\n" );
                bSuccess = false;
            }
            else
            {
                Debug.Print( "Add_UnsignedIntegralValues by refecrence - succeeded\n" );
            }

            correctVal = ( param1 ? 1u : 0u ) + param2 + param3 + param4 + param5;
            
            
            retVal = TestBasicTypes.Add_UnsignedIntegralValues( true, 1, 2, 3, 4 );
            
            // Tests passing of data from typed variables. The data type for param4 turns out to be different.
            ulong retVal1 = TestBasicTypes.Add_UnsignedIntegralValues( param1, param2, param3, param4, param5 );
            
            if( correctVal != retVal || correctVal != retVal1 )
            {
                Debug.Print( "Error in Add_UnsignedIntegralValues - rRetVal is " + retVal + " Should be " + correctVal + "\n" );
                bSuccess = false;
            }
            else
            {
                Debug.Print( "Add_UnsignedIntegralValues - succeeded\n" );
            }

            //             Circle data passed by reference in varialbles


            TestBasicTypes.Circle_UnsignedIntegralValues( ref param1, ref param2, ref param3, ref param4, ref param5 );

            // Should be circular shifted data
            if( param2 != 2 || param3 != 3 || param4 != 4 || param5 != 1 )
            {
                Debug.Print( "Error in Circle_SignedIntegralValues\n" );
                bSuccess = false;
            }
            else
            {
                Debug.Print( "Circle_SignedIntegralValues - succeeded\n" );
            }

            return bSuccess;
        }

        static bool TestFloatPointValues()
        {
            bool bSuccess = true;

            float param0 = 0.1111F;
            float param1 = 1.1111F;
            double param2 = 3.3333;
            double param3 = 4.4444;

            double correctVal = param0 + param1 + param2 + param3;
            double retVal = TestBasicTypes.Add_FloatPointValues( param0, ref param1, param2, ref param3 );

            if( !AreFloatValuesEqaul( correctVal, retVal ) )
            {
                Debug.Print( "Error in Add_FloatPointValues - rRetVal is " + retVal + " Should be " + correctVal + "\n" );
                bSuccess = false;
            }
            else
            {
                Debug.Print( "Add_FloatPointValues - succeeded\n" );
            }

            // Exchange parameters and check if it works;
            TestBasicTypes.Circle_FloatPointValues( ref param1, ref param2 );
            if( !AreFloatValuesEqaul( param1, 3.3333 ) || !AreFloatValuesEqaul( param2, 1.1111 ) )
            {
                Debug.Print( "Error in Circle_FloatPointValues\n" );
                bSuccess = false;
            }
            else
            {
                Debug.Print( "Circle_FloatPointValues - succeeded\n" );
            }
            return bSuccess;
        }

        static bool TestStringValues()
        {
            bool bSuccess = true;

            string stringTest = "Hello World";
            int stringLength1 = TestBasicTypes.String_GetLength( "Hello World" );
            int stringLength2 = TestBasicTypes.String_GetLength( stringTest );

            if( stringLength1 != stringLength2 || stringLength1 != stringTest.Length )
            {
                Debug.Print( "Error in String_GetLength: " + stringLength1 + " " + stringLength1 + "stringTest.Length is " + stringTest.Length );
                bSuccess = false;
            }
            else
            {
                Debug.Print( "String_GetLength - succeeded\n" );
            }

            string strVersion = TestBasicTypes.GetVersion();

            if( strVersion != "Interop Version 1.0" )
            {
                Debug.Print( "Error in GetVersion: " + strVersion + " should be \"Interop Version 1.0\"" );
                bSuccess = false;
            }
            else
            {
                Debug.Print( "GetVersion - succeeded\n" );
            }

            return bSuccess;
        }

        static bool TestArrayValues()
        {
            bool bSuccess = true;

            bool[] boolArray = new bool[4];// { true, true, false, true };
            // Init boolean somehow to get 3 trues;
            boolArray[0] = boolArray[1] = boolArray[3] = true;
            boolArray[2] = false;

            byte[] byteArray = new byte[10];
            long[] longArray = new long[10]; 
            float[] floatArrray = new float[10];
            double[] doubleArray = new double[10];

            for( int i = 0; i < 10; i++ )
            {
                byteArray[i] =   (byte)i;
                longArray[i] =   i * 10;
                floatArrray[i] = (float)(i * 2.5);
                doubleArray[i] = i * 3.5;
            }


            int retboolSum = TestBasicTypes.Add_Values_Array_Bool( boolArray );
            int retbyteSum = TestBasicTypes.Add_Values_Array_Byte( byteArray );
            int retlongSum = TestBasicTypes.Add_Values_Array_Int64( longArray );
            float retfloatSum = TestBasicTypes.Add_Values_Array_float( floatArrray );
            double retdoubleSum = TestBasicTypes.Add_Values_Array_double( doubleArray );

            if( retbyteSum != 45 || 
                retlongSum != retbyteSum * 10  ||
                retfloatSum != retbyteSum * 2.5 ||
                retdoubleSum != retbyteSum * 3.5 )
            {
                Debug.Print( "Error in testing of arrays\n" );
                bSuccess = false;
            }
            else
            {
                Debug.Print( "Testing of arrays succeded\n" );
                bSuccess = true;

            }

            return bSuccess;
        }



        static bool TestCallback()
        {
            TestCallback testCall = new TestCallback( "InteropSample_TestDriver", 12345, 10 );
            
            // Register callback
            NativeEventHandler   eventHandler = new NativeEventHandler( testCall.OnNativeEvent );
            testCall.OnInterrupt += eventHandler;

            // Now wait for 10 callbacks
            bool retVal = testCall.TestCallbacks();

            testCall.OnInterrupt -= eventHandler;

            testCall.Dispose();

            return retVal;
        }

        static bool TestMemberFunctions()
        {
            bool retVal = true;

            sbyte initVal = 10; 

            TestNonStaticFunctions objWithFields = new TestNonStaticFunctions( initVal );

            // Test that all members initialized to initVal.
            if ( !objWithFields.TestIfMembersEqual( initVal ) )
            {
                Debug.Print( "Failed in Constructor of TestNonStaticFunctions" );
                retVal = false;
            }
            // This function adds 20 to all fields.
            objWithFields.IncreaseAllMembers( 20 );
            initVal += 20;

            if ( !objWithFields.TestIfMembersEqual( initVal ) )
            {
                Debug.Print( "Failed in IncreaseAllMembers" );
                retVal = false;
            }

            // Test retrieval of member varialbes through function with reference parameters:
            char char_var = '0';    sbyte sbyte_var = 0;  short short_var = 0;    int int_var = 0;     long  long_var = 0;
            bool bool_var = false;  byte byte_var = 0;    ushort ushort_var = 0;  uint uint_var = 0;   ulong ulong_var = 0;
            float float_var = 0;    double double_var = 0;
            // Retrieve member vars
            objWithFields.RetrieveMemberVariables
                (ref char_var, ref sbyte_var, ref short_var, ref int_var, ref long_var, ref bool_var, ref byte_var, ref ushort_var, ref uint_var, ref ulong_var, ref float_var, ref double_var);

            if ( objWithFields.char_data      != char_var   ||
                 objWithFields.sbyte_data     != sbyte_var  ||
                 objWithFields.short_data     != short_var  ||
                 objWithFields.int_data       != int_var    ||
                 objWithFields.long_data      != long_var   ||
                 objWithFields.bool_data      != bool_var   ||
                 objWithFields.byte_data      != byte_var   ||
                 objWithFields.ushort_data    != ushort_var ||
                 objWithFields.uint_data      != uint_var   ||
                 objWithFields.ulong_data     != ulong_var  ||
                 objWithFields.float_data     != float_var  ||
                 objWithFields.double_data    != double_var 
                )
            {
                retVal = false;
                Debug.Print("Failed in test of passing variables by reference in non-static member function" );
            }
            else
            {
                Debug.Print("Success in test of passing variables by reference in non-static member functiona");
            }
 
            
            
            // Validates setting and retrieval of properties.
            if( objWithFields.prop_double_data != objWithFields.double_data )
            {
                retVal = false;
                Debug.Print( "Failed in retrieval of prop_double_data" );
            }
            else
            {
                Debug.Print( "Success in retrieval of prop_double_data" );
            }

            double testDoubleVal = 123.456;
            objWithFields.prop_double_data = testDoubleVal;
            if( testDoubleVal != objWithFields.double_data )
            {
                retVal = false;
                Debug.Print( "Failed in setting of prop_double_data" );
            }
            else
            {
                Debug.Print( "Success in setting of prop_double_data" );
            }

            if ( retVal )
            {
                Debug.Print( "Success in testing of non-static member function\n" );
            }
            return retVal;
        }


        static void RunAllocations(object[] arrObj)
        {
            for (int i = 1; i < arrObj.Length; i++)
           {
               // Creates referenced interger object, which stays in memory until InitiateCompactoin exits
               arrObj[i] = new int();
               // Tries to create larger object that would be later hole . This object could be garbage collected on each "i" cycle.
               int[] arr = new int[50 * i];
               // Creates some usage for arr, so it is not optimized out. 
               arr[0] = i;
               arr[1] = 50 * i;
               Debug.Print( "On Cycle " + arr[0] + " Array of  " + arr[1] + " was allocated" );
           }
        }
        
        
        // This function cause compaction to occur.
        // It is not so trivial as it need to fragment heap with referenced objects.      
        static void InitiateCompactoin()
        {
            // Large count, so compaction happens during RunAllocations.
            object[] arrayOfArrays = new object[1000];
            RunAllocations(arrayOfArrays);
        }
        
        
        // Test that buffer of array does not move during compaction if fixed keyword applied.
        static bool TestCompactionForNotFixedArray()
        {
            bool retVal = true;

            // First we create objects and holes that keeps some space that could be used by compaction.
            // Small count so compaction does not happen. 
            object[] arrayOfArrays = new object[10];
            RunAllocations(arrayOfArrays);
            
            // This is the array that we expect to move in during compaction.
            int[] testNativeBuffer = new int[100];
            // Fill it, so it is not optimized out
            for (int i = 0; i < testNativeBuffer.Length; i++)
            {
                testNativeBuffer[i] = i;
            }

            // Address before compaction
            uint addrBeforeCompaction = TestBasicTypes.Get_Buffer_Address_Array(testNativeBuffer);
            // Causes compaction. 
            InitiateCompactoin();
            // Address after compaction.
            uint addrAfterCompaction = TestBasicTypes.Get_Buffer_Address_Array(testNativeBuffer);
            // Should be different addresses.
            retVal = addrBeforeCompaction != addrAfterCompaction;

            if (retVal)
            {
                Debug.Print("Testing of buffer move during compaction succeeded");
            }
            else
            {
                Debug.Print("Testing of fixed keyword failed");
            }
            return retVal;
        }

        // Test that buffer of array actually moves during compaction.
        unsafe static bool  TestCompactionForFixedArray()
        {
            bool retVal = true;

            // First we create objects and holes that keeps some space that could be used by compaction.
            // Small count so compaction does not happen. 
            object[] arrayOfArrays = new object[10];
            RunAllocations(arrayOfArrays);

            // This is the array that we expect to move in during compaction.
            int[] testNativeBuffer = new int[100];
            // Fill it, so it is not optimized out
            for (int i = 0; i < testNativeBuffer.Length; i++)
            {
                testNativeBuffer[i] = i;
            }

            fixed (int* pFixedArray = testNativeBuffer)
            {
                // Address before compaction
                uint addrBeforeCompaction = TestBasicTypes.Get_Buffer_Address_Array(testNativeBuffer);
                // Causes compaction. 
                InitiateCompactoin();
                // Address after compaction.
                uint addrAfterCompaction = TestBasicTypes.Get_Buffer_Address_Array(testNativeBuffer);
                // Should be equal addresses, becuase array was pinned.
                retVal = addrBeforeCompaction == addrAfterCompaction;

                // Check that access to the buffer through the reference is the same as through array
                for (int i = 0; i < testNativeBuffer.Length; i++)
                {
                    retVal = retVal && (pFixedArray[i] == testNativeBuffer[i]);
                }
                
                // Update the data of array through reference, then check that it was updated properly
                Random random = new Random();
                for (int i = 0; i < testNativeBuffer.Length; i++)
                {
                    int value = 2 * random.Next();
                    pFixedArray[i] = value;
                    // Check that value is set properly and can be accessed through reference and array
                    if (pFixedArray[i] != value || testNativeBuffer[i] != value)
                    {
                        retVal = false;
                    }
                }

                // Test that access through *pFixedArray is the same as pFixedArray[0];
                *pFixedArray = 2 * random.Next();
                if (pFixedArray[0] != *pFixedArray || testNativeBuffer[0] != *pFixedArray)
                {
                    retVal = false;
                }
            }

            // Since we exited "fixed" block, the testNativeBuffer is now movable. 
            // It is very important to see that now buffer moves, this is the prove that it was unpinned. .
            uint addrBeforeCompaction1 = TestBasicTypes.Get_Buffer_Address_Array(testNativeBuffer);
            InitiateCompactoin();
            uint addrAfterCompaction1 = TestBasicTypes.Get_Buffer_Address_Array(testNativeBuffer);
            // Verifies that now buffer moved.
            retVal = retVal && (addrBeforeCompaction1 != addrAfterCompaction1);

            if (retVal)
            {
                Debug.Print("Testing of fixed keyword succeded");
            }
            else
            {
                Debug.Print("Testing of fixed keyword failed");
            }
            return retVal;
        }

    
        public static void Main()
        {
            bool retVal = TestSignedValues();

            retVal = retVal && TestUnsignedValues();

            retVal = retVal && TestFloatPointValues();

            retVal = retVal && TestStringValues();

            retVal = retVal && TestArrayValues();

            retVal = retVal && TestCallback();

            retVal = retVal && TestMemberFunctions();

            retVal = retVal && TestCompactionForNotFixedArray();
            
            retVal = retVal && TestCompactionForFixedArray();
            

            if( retVal )
            {
                Debug.Print( "All Interop Tests succeded" );
            }
            else
            {
                Debug.Print( "One or more Interop tests failed" );
            }                
        }       
    }
}
