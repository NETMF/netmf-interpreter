////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.CompilerServices;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Platform.Test;

// error CS0414: The field '<identifier>' is assigned but its value is never used
#pragma warning disable 0414

namespace Microsoft.SPOT.Platform.Test.Interop
{
 
 public enum    TestEnumDefaultType             { FirstValue = 5, SecondValue };
 public enum    TestEnumByteType    : byte      { FirstValue = 5, SecondValue };
 public enum    TestEnumUshortType  : ushort    { FirstValue = 5, SecondValue };
 
    
 // Current release of Interop does not support marshaling of struct and class
 // in parameters.
 // We place these type and test functions in order to verify code generation. 
 public struct TestStructType
 {
     public int FirstStructValue;
     public ushort SecondStructValue;
 };

 public struct TestClassType
 {
     public int FirstClassValue;
     public ushort SecondClassValue;
 };


   
 public static class TestBasicTypes
  {
     
    [MethodImplAttribute( MethodImplOptions.InternalCall )]
    extern static public long VerifyEnumType(TestEnumDefaultType a, TestEnumUshortType b, TestEnumByteType c);
     
     //-------- Test all signed types - reference and value types.
    [MethodImplAttribute( MethodImplOptions.InternalCall )]
    extern static public long Add_SignedIntegralValues( char a, sbyte b, short c, int d, long e );

    [MethodImplAttribute( MethodImplOptions.InternalCall )]
    extern static public long Add_SignedIntegralValuesByRef( ref char a, ref sbyte b, ref  short c, ref int d, ref long e );

    [MethodImplAttribute( MethodImplOptions.InternalCall )]
    extern static public void Circle_SignedIntegralValues( ref char a, ref sbyte b, ref short c, ref int d, ref long e );
    
    //-------- Test all unsigned types - reference and value types.
    [MethodImplAttribute( MethodImplOptions.InternalCall )]
    extern static public ulong Add_UnsignedIntegralValues( bool a, byte b, ushort c, uint d, ulong e );

    [MethodImplAttribute( MethodImplOptions.InternalCall )]
    extern static public long Add_UnsignedIntegralValuesByRef( ref bool a, ref byte b, ref ushort c, ref uint d, ref ulong e );
    
    [MethodImplAttribute( MethodImplOptions.InternalCall )]
    extern static public void Circle_UnsignedIntegralValues( ref bool a, ref byte b, ref ushort c, ref uint d, ref ulong e );
    
    // Test float point - passing by value and references
    [MethodImplAttribute( MethodImplOptions.InternalCall )]
    extern static public System.Double Add_FloatPointValues( float a, ref float b, double c, ref double d );

    [MethodImplAttribute( MethodImplOptions.InternalCall )]
    extern static public void Circle_FloatPointValues( ref float a, ref double b );

    // Test passing of string.
    [MethodImplAttribute( MethodImplOptions.InternalCall )]
    extern static public int String_GetLength( string str );

    // Test return of string and also function call with no arguments
    [MethodImplAttribute( MethodImplOptions.InternalCall )]
    extern static public string GetVersion();

    // Test passing of values in array.
    [MethodImplAttribute( MethodImplOptions.InternalCall )]
    extern static public int Add_Values_Array_Bool( bool[] arrBool );

    [MethodImplAttribute( MethodImplOptions.InternalCall )]
    extern static public int Add_Values_Array_Byte( System.Byte[] arrByte );

    [MethodImplAttribute( MethodImplOptions.InternalCall )]
    extern static public int Add_Values_Array_Int64( System.Int64[] arrInt64 );

    [MethodImplAttribute( MethodImplOptions.InternalCall )]
    extern static public float Add_Values_Array_float( float[] arrInt64 );

    [MethodImplAttribute( MethodImplOptions.InternalCall )]
    extern static public double Add_Values_Array_double( double[] arrInt64 );

    // Retrieves native address of array buffer. Returns address as uint32 varialble. 
    // Implicitly assumes that native system is 32 bit.
    // This function is used to test "fixed" keyword, verifies that buffer does not move.
    [MethodImplAttribute( MethodImplOptions.InternalCall )]
    extern static public uint Get_Buffer_Address_Array( int[] arrInt );

    [MethodImplAttribute(MethodImplOptions.InternalCall)]
    extern static public long VerifyStructType(TestClassType a);

    [MethodImplAttribute(MethodImplOptions.InternalCall)]
    extern static public long VerifyClassType(TestClassType a);
    
    [MethodImplAttribute(MethodImplOptions.InternalCall)]
    extern static public long VerifyStructType(ref TestClassType a);

    [MethodImplAttribute(MethodImplOptions.InternalCall)]
    extern static public long VerifyClassType(ref TestClassType a);
  }


  public class TestCallback : NativeEventDispatcher       
  {
      int m_CallbackReceived;
      int m_CallbackRequired;


      // Pass the name to the base so it connects to driver 
      public TestCallback( string strDrvName, ulong drvData, int callbackCount )
          : base( strDrvName, drvData )
      {
          m_CallbackReceived = 0;
          m_CallbackRequired = callbackCount;
      }

      public void OnNativeEvent( uint data1, uint data2, DateTime time )
      {
          m_CallbackReceived++;
          Log.Comment( "Callback Received: " + m_CallbackReceived );
      }

      public bool TestCallbacks()
      {
          for ( int clbPosted = 0; clbPosted< m_CallbackRequired; clbPosted++ )
          {
              GenerateInterrupt();
              Log.Comment( "Callback posted: " + ( clbPosted + 1 ) );
          }

          // Allows all callbacks to complete.
          System.Threading.Thread.Sleep( 1000 );

          Log.Comment( "Total Posted: " + m_CallbackRequired + ", Total Received: " + m_CallbackReceived );

          if ( m_CallbackRequired == m_CallbackReceived )
          {
              Log.Comment( "Testing of callbacks succeded\n" );
          }
          else
          {
              Log.Comment( "Error in testing of callbacks\n" );
          }
          // Return true if all posted callbacks were received.
          return m_CallbackRequired == m_CallbackReceived;
      }

      // This function communicates with test native driver code 
      // and simulates call to Interrup Service Routine. 
      [MethodImplAttribute( MethodImplOptions.InternalCall )]
      extern static public void GenerateInterrupt(); 
  }

  public class TestNonStaticFunctions
  {
      // Test data members. 
      // Put one of each type. Validate data members generated in native code.
      TestEnumDefaultType   enum_default_type_data ;
      TestEnumByteType      enum_byte_type_data;
      TestEnumUshortType    enum_ushort_type_data;
      TestStructType        struct_type_data;
      TestClassType         class_type_data;
      public char           char_data;
      public sbyte          sbyte_data;     
      public short          short_data;
      public readonly int   int_data;     
      public long           long_data;     
      public bool           bool_data;     
      public byte           byte_data;     
      public ushort         ushort_data;     
      public uint           uint_data;     
      public ulong          ulong_data;     
      public float          float_data;     
      public double         double_data;


      // Does some operations on TestStructType and TestClassType to avoid warnings. 
      // THis code is not executed. 
      // TestStructType and TestClassType exists for testing of marshaling code generation,
      // basically for testing of metadata processor.
      public void EliminateWarningsForUnUsedTypes()
      {
          struct_type_data.FirstStructValue  = 1;
          struct_type_data.SecondStructValue = 2;
          class_type_data = new TestClassType();
          class_type_data.FirstClassValue   = 1;
          class_type_data.SecondClassValue = 2;

          enum_default_type_data = TestEnumDefaultType.FirstValue;
          enum_byte_type_data = TestEnumByteType.FirstValue;
          enum_ushort_type_data = TestEnumUshortType.FirstValue;

      }   
      
      
      // Test Native Constructor. 
      [MethodImplAttribute( MethodImplOptions.InternalCall )]
      extern public TestNonStaticFunctions( sbyte initValue );

      // Test Increasing of all member variables by one.
      [MethodImplAttribute( MethodImplOptions.InternalCall )]
      extern public int IncreaseAllMembers( int incValue );

      // Test code generaration for passing of varialbe by refence.
      // Retrieves values of member varialbes.
      [MethodImplAttribute( MethodImplOptions.InternalCall )]
      extern public void RetrieveMemberVariables
          ( ref char char_var, ref sbyte sbyte_var, ref short short_var, ref int int_var,    ref long long_var,
            ref bool bool_var, ref byte byte_var, ref ushort ushort_var, ref uint uint_var,  ref ulong ulong_var,
            ref float float_var, ref double double_var );

      // Test code generaration for function with 2 parameters and void return type.
      [MethodImplAttribute( MethodImplOptions.InternalCall )]
      extern public void TestTwoParams( int int_value, double double_value );

      // Test code generation for properties
      extern public double prop_double_data
      {
          [MethodImplAttribute( MethodImplOptions.InternalCall )]
          set;
          [MethodImplAttribute( MethodImplOptions.InternalCall )]
          get;
      }

      
      // Test by comparing that members are equal to testValue.
      public bool TestIfMembersEqual( int testValue )
      {
          if(  bool_data != true ||
               byte_data != testValue ||
               sbyte_data != testValue ||
               short_data != testValue ||
               int_data != testValue ||
               long_data != testValue ||
               byte_data != testValue ||
               ushort_data != testValue ||
               uint_data != testValue ||
               ulong_data != (ulong)testValue ||
               float_data != testValue ||
               double_data != testValue ||
               enum_default_type_data != (TestEnumDefaultType)testValue ||
               enum_byte_type_data !=    (TestEnumByteType)testValue ||
               enum_ushort_type_data !=  (TestEnumUshortType)testValue
             )
          {
              return false;
          }
          return true;
      }

      class TestInternalType
      {
          public long m_value = 1;
          // Test code generaration for function with 2 parameters and void return type.
          [MethodImplAttribute(MethodImplOptions.InternalCall)]
          extern public long GetValue();
      }
  } 
}   

