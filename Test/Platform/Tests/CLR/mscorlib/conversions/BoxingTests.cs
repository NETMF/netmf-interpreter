////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class BoxingTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            Log.Comment("The Boxing tests determine if a type's data can survive being stored in");
            Log.Comment("an object and then re-cast as their original type.");
            Log.Comment("The tests are named for the type they test.");

            // Add your functionality here.                

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
        }

        //Boxing Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\Boxing
        //byte,char,double,float,int,long,sbyte,short,uint,ulong,ushort,struct_to_ValType,ValType_to_struct

        //Test Case Calls 
        [TestMethod]
        public MFTestResults Boxingbyte_Test()
        {
            if (BoxingTestClassbyte.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Boxingchar_Test()
        {
            if (BoxingTestClasschar.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Boxingdouble_Test()
        {
            if (BoxingTestClassdouble.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Boxingfloat_Test()
        {
            if (BoxingTestClassfloat.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Boxingint_Test()
        {
            if (BoxingTestClassint.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Boxinglong_Test()
        {
            if (BoxingTestClasslong.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Boxingsbyte_Test()
        {
            if (BoxingTestClasssbyte.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Boxingshort_Test()
        {
            if (BoxingTestClassshort.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Boxinguint_Test()
        {
            if (BoxingTestClassuint.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Boxingulong_Test()
        {
            if (BoxingTestClassulong.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Boxingushort_Test()
        {
            if (BoxingTestClassushort.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Boxingstruct_to_ValType_Test()
        {
            if (BoxingTestClassStruct_to_ValType.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults BoxingValType_to_struct_Test()
        {
            if (BoxingTestClassValType_to_struct.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }


        //Compiled Test Cases 

        public class BoxingTestClassbyte 
        {
            public static bool testMethod() 
            {
	        byte	value = 1;
	        object	obj;
	        obj = value;					// box
	        byte	value2;
	        value2 = (byte) obj;		// unbox
	        if (value2 == value)
		        return true;
	        else
		        return false;
                
            } 
        }


        public class BoxingTestClasschar 
        {
            public static bool testMethod() 
            {
	        char	value = '\x1';
	        object	obj;
	        obj = value;					// box
	        char	value2;
	        value2 = (char) obj;		// unbox
	        if (value2 == value)
		        return true;
	        else
		        return false;
                
            } 
        }


        public class BoxingTestClassdouble 
        {
            public static bool testMethod() 
            {
	        double	value = 1.0;
	        object	obj;
	        obj = value;					// box
	        double	value2;
	        value2 = (double) obj;		// unbox
	        if (value2 == value)
		        return true;
	        else
		        return false;
                
            } 
        }


        public class BoxingTestClassfloat 
        {
            public static bool testMethod() 
            {
	        float	value = 1F;
	        object	obj;
	        obj = value;					// box
	        float	value2;
	        value2 = (float) obj;		// unbox
	        if (value2 == value)
		        return true;
	        else
		        return false;
                
            } 
        }


        public class BoxingTestClassint 
        {
            public static bool testMethod() 
            {
	        int	value = 1;
	        object	obj;
	        obj = value;					// box
	        int	value2;
	        value2 = (int) obj;		// unbox
	        if (value2 == value)
		        return true;
	        else
		        return false;
                
            } 
        }


        public class BoxingTestClasslong 
        {
            public static bool testMethod() 
            {
	        long	value = 1;
	        object	obj;
	        obj = value;					// box
	        long	value2;
	        value2 = (long) obj;		// unbox
	        if (value2 == value)
		        return true;
	        else
		        return false;
                
            } 
        }


        public class BoxingTestClasssbyte 
        {
            public static bool testMethod() 
            {
	        sbyte	value = 1;
	        object	obj;
	        obj = value;					// box
	        sbyte	value2;
	        value2 = (sbyte) obj;		// unbox
	        if (value2 == value)
		        return true;
	        else
		        return false;
                
            } 
        }


        public class BoxingTestClassshort 
        {
            public static bool testMethod() 
            {
	        short	value = 1;
	        object	obj;
	        obj = value;					// box
	        short	value2;
	        value2 = (short) obj;		// unbox
	        if (value2 == value)
		        return true;
	        else
		        return false;
                
            } 
        }


        public class BoxingTestClassuint 
        {
            public static bool testMethod() 
            {
	        uint	value = 1;
	        object	obj;
	        obj = value;					// box
	        uint	value2;
	        value2 = (uint) obj;		// unbox
	        if (value2 == value)
		        return true;
	        else
		        return false;
                
            } 
        }


        public class BoxingTestClassulong 
        {
            public static bool testMethod() 
            {
	        ulong	value = 1;
	        object	obj;
	        obj = value;					// box
	        ulong	value2;
	        value2 = (ulong) obj;		// unbox
	        if (value2 == value)
		        return true;
	        else
		        return false;
            } 
        }


        public class BoxingTestClassushort 
        {
            public static bool testMethod() 
            {
	        ushort	value = 1;
	        object	obj;
	        obj = value;					// box
	        ushort	value2;
	        value2 = (ushort) obj;		// unbox
	        if (value2 == value)
                return true;
	        else
                return false;
            } 
        }

        struct BoxingTestClassStruct_to_ValTypeTest_struct { }
        class BoxingTestClassStruct_to_ValType
        {
            public static bool testMethod()
            {
                BoxingTestClassStruct_to_ValTypeTest_struct src = new BoxingTestClassStruct_to_ValTypeTest_struct();
                System.ValueType dst = src;
                return true;
            }
        }

        struct BoxingTestClassValType_to_struct_struct { }
        class BoxingTestClassValType_to_struct
        {
	        public static bool testMethod()
	        {
		        System.ValueType src = new BoxingTestClassValType_to_struct_struct();
		        BoxingTestClassValType_to_struct_struct dst = (BoxingTestClassValType_to_struct_struct) src;
                return true;
            }
        }

    }
}
