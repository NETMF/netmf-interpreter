////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ExprefTests : IMFTestInterface
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
            Log.Comment("Cleaning up after the tests");
        }

        //Expref Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\Expref
        //obj_ref,obj_ref_exc,class_class,class_class_exc,inter_struct,inter_struct_exc,class_inter,class_inter_exc,inter_class,inter_class2,inter_class2_exc1,inter_class2_exc2,inter_class_exc,inter_class_sealed,inter_class_sealed_exc,inter_inter,inter_inter_exc
        //Inter_Struct will not compile

        //Test Case Calls 
        [TestMethod]
        public MFTestResults Expref_obj_ref_Test()
        {   
            
            Log.Comment(" Converting from 'object' to a reference object. ");
            if (ExprefTestClass_obj_ref.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Expref_obj_ref_exc_Test()
        {
            
            Log.Comment(" Converting from 'object' to a reference object. ");
            if (ExprefTestClass_obj_ref_exc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Expref_class_class_Test()
        {
            
            Log.Comment(" Tests that you can convert from a base class to a derived class");
            if (ExprefTestClass_class_class.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Expref_class_class_exc_Test()
        {
            
            Log.Comment(" Tests that you can convert from a base class to a derived class");
            if (ExprefTestClass_class_class_exc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Expref_inter_struct_exc_Test()
        {
            
            Log.Comment(" Tests that you can convert from an interface to a struct that implements it.");
            if (ExprefTestClass_inter_struct_exc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Expref_class_inter_Test()
        {
            Log.Comment(" Tests converting from a class to an interface that the class does not implement (but a derived class might).");
            if (ExprefTestClass_class_inter.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Expref_class_inter_exc_Test()
        {
            Log.Comment(" Tests converting from a class to an interface that the class does not implement (but a derived class might).");
            if (ExprefTestClass_class_inter_exc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Expref_inter_class_Test()
        {
            Log.Comment(" Tests converting from an interface to a class");
            Log.Comment("From any interface-type S to any class-type T, provided T is not sealed, or provided T implements S.");
            Log.Comment("If T implements S:");
            if (ExprefTestClass_inter_class.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Expref_inter_class2_Test()
        {
            Log.Comment(" Tests converting from an interface to a class");
            if (ExprefTestClass_inter_class2.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Expref_inter_class2_exc1_Test()
        {
            Log.Comment(" Tests converting from an interface to a class");
            if (ExprefTestClass_inter_class2_exc1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Expref_inter_class2_exc2_Test()
        {

            Log.Comment(" Tests converting from an interface to a class");
            if (ExprefTestClass_inter_class2_exc2.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Expref_inter_class_exc_Test()
        {

            Log.Comment(" Tests converting from an interface to a class");
            if (ExprefTestClass_inter_class_exc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Expref_inter_class_sealed_Test()
        {
            Log.Comment(" Tests converting from an interface to a class");
            if (ExprefTestClass_inter_class_sealed.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Expref_inter_class_sealed_exc_Test()
        {
            Log.Comment(" Tests converting from an interface to a class");
            if (ExprefTestClass_inter_class_sealed_exc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Expref_inter_inter_Test()
        {
            Log.Comment(" Tests converting from an interface to an interface");
            if (ExprefTestClass_inter_inter.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Expref_inter_inter_exc_Test()
        {
            Log.Comment(" Tests converting from an interface to an interface");
            if (ExprefTestClass_inter_inter_exc.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        //Compiled Test Cases 
        public class ExprefTestClass_obj_ref_Sub1
        {
            public void func() {/*Old Print*/}
        }
        public class ExprefTestClass_obj_ref_Sub2
        {
            public void func() {/*Old Print*/}
        }
        public class ExprefTestClass_obj_ref 
        {
            public static bool testMethod() 
            {
	            ExprefTestClass_obj_ref_Sub1	test = new ExprefTestClass_obj_ref_Sub1();
	            object	obj;
	            obj = test;		// implicit setup
	            test = (ExprefTestClass_obj_ref_Sub1) obj;
	            test.func();
                return true;
            } 
        }

        public class ExprefTestClass_obj_ref_exc_Sub1
        {
            int func() {return(1);}
        }
        public class ExprefTestClass_obj_ref_exc_Sub2
        {
            int func() {return(1);}
        }
        public class ExprefTestClass_obj_ref_exc 
        {
            public static bool testMethod() 
            {
	            ExprefTestClass_obj_ref_exc_Sub1	test = new ExprefTestClass_obj_ref_exc_Sub1();
	            ExprefTestClass_obj_ref_exc_Sub2	test2;
	            object	obj;
	            obj = test;		// implicit setup
	            try
	            {
		            test2 = (ExprefTestClass_obj_ref_exc_Sub2) obj;	// obj is *not* a test2
	            }
	            catch (System.Exception e)
	            {
		            //Old Print		
	            }
                return true;
            } 
        }

        class	ExprefTestClass_class_class_Base1
        {
	        void	i() {}
        }

        class	ExprefTestClass_class_class_Base2 : ExprefTestClass_class_class_Base1
        {
	        void	j() {}
        }
        public class ExprefTestClass_class_class 
        {
            public static bool testMethod() 
            {
		        ExprefTestClass_class_class_Base2	derivedClass = new ExprefTestClass_class_class_Base2();

                ExprefTestClass_class_class_Base1 myBase;
		        myBase = derivedClass;		// implicit conversion
                derivedClass = (ExprefTestClass_class_class_Base2)myBase;		// test conversion
                return true;
            } 
        }

	    class	ExprefTestClass_class_class_exc_Base
	    {
		    void	i() {}
	    }

	    class	ExprefTestClass_class_class_exc_Der1 : ExprefTestClass_class_class_exc_Base
	    {
		    void	j() {}
	    }
	    class	ExprefTestClass_class_class_exc_Der2 : ExprefTestClass_class_class_exc_Base
	    {
		    void	k() {}
	    }
	    public class ExprefTestClass_class_class_exc 
	    {
	        public static bool testMethod() 
	        {
			    ExprefTestClass_class_class_exc_Der1	derivedClass = new ExprefTestClass_class_class_exc_Der1();

                ExprefTestClass_class_class_exc_Base myBase;
			    ExprefTestClass_class_class_exc_Der2	derivedClass3;
			    myBase = derivedClass;		// implicit conversion
			    try
			    {
                    derivedClass3 = (ExprefTestClass_class_class_exc_Der2)myBase;		// test conversion
			    }
			    catch (System.Exception e)
			    {
				    //Old Print		
			    }
                return true;
	        }
        }

	    interface	ExprefTestClass_inter_struct_exc_Interface1
	    {
		    void	i();
	    }
	    public interface	ExprefTestClass_inter_struct_exc_Interface2
	    {
		    void	j();
	    }

        struct ExprefTestClass_inter_struct_exc_struct1 : ExprefTestClass_inter_struct_exc_Interface1
	    {
		    public void	i() {//Old Print
            }
	    }
	    struct TheStruct2 : ExprefTestClass_inter_struct_exc_Interface1
	    {
		    public void	i() {//Old Print
            }
	    }

	    public class ExprefTestClass_inter_struct_exc 
	    {
	        public static bool testMethod() 
	        {
			    //Old Print
			    ExprefTestClass_inter_struct_exc_struct1	theStruct1 = new ExprefTestClass_inter_struct_exc_struct1();
			    //Old Print
			    return true;
			    theStruct1.i();
			    ExprefTestClass_inter_struct_exc_Interface1		ExprefTestClass_inter_struct_exc_Interface1;
			    ExprefTestClass_inter_struct_exc_Interface1 = theStruct1;
			    theStruct1 = (ExprefTestClass_inter_struct_exc_struct1) ExprefTestClass_inter_struct_exc_Interface1;
			    theStruct1.i();
			    TheStruct2	theStruct2;
			    theStruct2 = (TheStruct2) ExprefTestClass_inter_struct_exc_Interface1;
			    theStruct2.i();
			    //ExprefTestClass_inter_struct_exc.TestRoutine(ExprefTestClass_inter_struct_exc_Interface1);
				    // NOTE: Currently detects this during compile time; try passing ExprefTestClass_inter_struct_exc_Interface1 to a method
				    // to see if that will defeat the compile-time flow analysis. 
	        } 
        }
	    interface	ExprefTestClass_class_inter_I1
	    {
		    void	i();
	    }
	    interface	ExprefTestClass_class_inter_I2
	    {
		    void	j();
	    }

	    class ExprefTestClass_class_inter_C1: ExprefTestClass_class_inter_I1
	    {
		    public void	i() {//Old Print
            }
	    }
	    class ExprefTestClass_class_inter_C2: ExprefTestClass_class_inter_C1, ExprefTestClass_class_inter_I2
	    {
		    public void	j() {//Old Print
            }
	    }
	    public class ExprefTestClass_class_inter 
	    {
	        public static bool testMethod() 
	        {
			    ExprefTestClass_class_inter_C1	thebase = new ExprefTestClass_class_inter_C2();
			    try
			    {
				    ExprefTestClass_class_inter_I2	i2 = (ExprefTestClass_class_inter_I2) thebase;
				    i2.j();
			    }
			    catch (System.Exception e)
			    {
				    //Old Print
			    }
                return true;
	        } 
        }
	interface	ExprefTestClass_class_inter_exc_I1
	{
		void	i();
	}
	interface	ExprefTestClass_class_inter_exc_I2
	{
		void	j();
	}

	class ExprefTestClass_class_inter_exc_C1: ExprefTestClass_class_inter_exc_I1
	{
		public void	i() {//Old Print
        }
	}
	class ExprefTestClass_class_inter_exc_C2: ExprefTestClass_class_inter_exc_C1
	{
	}
	public class ExprefTestClass_class_inter_exc 
	{
	    public static bool testMethod() 
	    {
			ExprefTestClass_class_inter_exc_C1	thebase = new ExprefTestClass_class_inter_exc_C2();
			try
			{
				ExprefTestClass_class_inter_exc_I2	i2 = (ExprefTestClass_class_inter_exc_I2) thebase;
				i2.j();
			}
			catch (System.Exception e)
			{
				//Old Print
			}
            return true;
	    } 
    }
	interface	ExprefTestClass_inter_class_I1
	{
		void	i();
	}
	class ExprefTestClass_inter_class_C1: ExprefTestClass_inter_class_I1
	{
		public void	i() {/*Old Print*/}
	}
	class ExprefTestClass_inter_class_C2: ExprefTestClass_inter_class_I1
	{
		public void	i() {/*Old Print*/}
	}
	public class ExprefTestClass_inter_class 
	{
	    public static bool testMethod() 
	    {
			ExprefTestClass_inter_class_I1	inter = new ExprefTestClass_inter_class_C1();
			try
			{
				ExprefTestClass_inter_class_C1	c1 = (ExprefTestClass_inter_class_C1) inter;
				c1.i();
			}
			catch (System.Exception)
			{
				//Old Print
			}
	    return true;
        }
    }
	interface	ExprefTestClass_inter_class2_I1
	{
		void	i();
	}
	interface	ExprefTestClass_inter_class2_I2
	{
		void	j();
	}
	class ExprefTestClass_inter_class2_C1: ExprefTestClass_inter_class2_I1
	{
		public void	i() {//Old Print
        }
	}
	class ExprefTestClass_inter_class2_C2: ExprefTestClass_inter_class2_I1
	{
		public void	i() {//Old Print
        }
	}
	class ExprefTestClass_inter_class2_C3: ExprefTestClass_inter_class2_C1, ExprefTestClass_inter_class2_I2
	{
		public void j() {//Old Print
        }
	}
	public class ExprefTestClass_inter_class2 
	{
	    public static bool testMethod() 
	    {
            ExprefTestClass_inter_class2_I2 inter = new ExprefTestClass_inter_class2_C3();
			try
			{
                ExprefTestClass_inter_class2_C1 c1 = (ExprefTestClass_inter_class2_C1)inter;
				c1.i();
			}
			catch (System.Exception e)
			{
				//Old Print
			}
	        return true;
        } 
    }
	interface	ExprefTestClass_inter_class2_exc1_I1
	{
		void	i();
	}
	interface	ExprefTestClass_inter_class2_exc1_I2
	{
		void	j();
	}
	class ExprefTestClass_inter_class2_exc1_C1: ExprefTestClass_inter_class2_exc1_I1
	{
		public void	i() {//Old Print
        }
	}
        class ExprefTestClass_inter_class2_exc1_C2 : ExprefTestClass_inter_class2_exc1_I2
	{
		public void	j() {//Old Print
        }
	}
	public class ExprefTestClass_inter_class2_exc1 
	{
	    public static bool testMethod() 
	    {
			ExprefTestClass_inter_class2_exc1_I2	inter = new ExprefTestClass_inter_class2_exc1_C2();
			try
			{
                ExprefTestClass_inter_class2_exc1_C1 c1 = (ExprefTestClass_inter_class2_exc1_C1)inter;
				c1.i();
			}
			catch (System.Exception e)
			{
				//Old Print
			}
            return true;
	    } 
    }
	interface	ExprefTestClass_inter_class2_exc2_I1
	{
		void	i();
	}
	interface	ExprefTestClass_inter_class2_exc2_I2
	{
		void	j();
	}
	class ExprefTestClass_inter_class2_exc2_C1: ExprefTestClass_inter_class2_exc2_I1
	{
		public void	i() {//Old Print
        }
	}
	class ExprefTestClass_inter_class2_exc2_C2: ExprefTestClass_inter_class2_exc2_I2
	{
		public void	j() {//Old Print
        }
	}
	class ExprefTestClass_inter_class2_exc2_C3: ExprefTestClass_inter_class2_exc2_C1
	{
	}
        public class ExprefTestClass_inter_class2_exc2
        {
            public static bool testMethod()
            {
                ExprefTestClass_inter_class2_exc2_I2 inter = new ExprefTestClass_inter_class2_exc2_C2();
                try
                {
                    ExprefTestClass_inter_class2_exc2_C1 c1 = (ExprefTestClass_inter_class2_exc2_C1)inter;
                    c1.i();
                }
                catch (System.Exception e)
                {
                    //Old Print
                }
                return true;
            }
        }
	interface	ExprefTestClass_inter_class_exc_I1
	{
		void	i();
	}
	class ExprefTestClass_inter_class_exc_C1: ExprefTestClass_inter_class_exc_I1
	{
		public void	i() {//Old Print
        }
	}
	class ExprefTestClass_inter_class_exc_C2: ExprefTestClass_inter_class_exc_I1
	{
		public void	i() {//Old Print
        }
	}
	public class ExprefTestClass_inter_class_exc 
	{
	    public static bool testMethod() 
	    {
			ExprefTestClass_inter_class_exc_I1	inter = new ExprefTestClass_inter_class_exc_C1();
			try
			{
				ExprefTestClass_inter_class_exc_C2	c2 = (ExprefTestClass_inter_class_exc_C2) inter;
			}
			catch (System.Exception e)
			{
				//Old Print
			}
            return true;
	    } 
    }
	interface	ExprefTestClass_inter_class_sealed_I1
	{
		void	i();
	}
	sealed class ExprefTestClass_inter_class_sealed_C1: ExprefTestClass_inter_class_sealed_I1
	{
		public void	i() {/*Old Print*/}
	}
	class ExprefTestClass_inter_class_sealed_C2: ExprefTestClass_inter_class_sealed_I1
	{
		public void	i() {/*Old Print*/}
	}
	public class ExprefTestClass_inter_class_sealed 
	{
	    public static bool testMethod() 
	    {
			ExprefTestClass_inter_class_sealed_I1	inter = new ExprefTestClass_inter_class_sealed_C1();
			try
			{
				ExprefTestClass_inter_class_sealed_C1	c1 = (ExprefTestClass_inter_class_sealed_C1) inter;
				c1.i();
			}
			catch (System.Exception e)
			{
				//Old Print
			}
            return true;
	    } 
    }
	interface	ExprefTestClass_inter_class_sealed_exc_I1
	{
		void	i();
	}
	sealed class ExprefTestClass_inter_class_sealed_exc_C1: ExprefTestClass_inter_class_sealed_exc_I1
	{
		public void	i() {/*Old Print*/}
	}
	class ExprefTestClass_inter_class_sealed_exc_C2: ExprefTestClass_inter_class_sealed_exc_I1
	{
		public void	i() {/*Old Print*/}
	}
	public class ExprefTestClass_inter_class_sealed_exc 
	{
	    public static bool testMethod() 
	    {
			ExprefTestClass_inter_class_sealed_exc_I1	inter = new ExprefTestClass_inter_class_sealed_exc_C1();
			try
			{
				ExprefTestClass_inter_class_sealed_exc_C2	c2 = (ExprefTestClass_inter_class_sealed_exc_C2) inter;
			}
			catch (System.Exception e)
			{
				//Old Print
			}
            return true;
	    } 
    }
	interface	ExprefTestClass_inter_inter_I1
	{
		void	i();
	}
	interface	ExprefTestClass_inter_inter_I2
	{
		void	j();
	}

	class ExprefTestClass_inter_inter_C1: ExprefTestClass_inter_inter_I1
	{
		public void	i() {/*Old Print*/}
	}
	class ExprefTestClass_inter_inter_C2: ExprefTestClass_inter_inter_C1, ExprefTestClass_inter_inter_I2
	{
		public void	j() {/*Old Print*/}
	}
	public class ExprefTestClass_inter_inter 
	{
	    public static bool testMethod() 
	    {
			ExprefTestClass_inter_inter_I2	i2 = (ExprefTestClass_inter_inter_I2) new ExprefTestClass_inter_inter_C2();
			try
			{
				ExprefTestClass_inter_inter_I1	i1 = (ExprefTestClass_inter_inter_I1) i2;
				i1.i();
			}
			catch (System.Exception e)
			{
				//Old Print
			}
            return true;

	    } 
    }
        interface ExprefTestClass_inter_inter_exc_I1
	{
		void	i();
	}
        interface ExprefTestClass_inter_inter_exc_I2
	{
		void	j();
	}
        interface ExprefTestClass_inter_inter_exc_I3
	{
		void	k();
	}

        class ExprefTestClass_inter_inter_exc_C1 : ExprefTestClass_inter_inter_exc_I1
	{
		public void	i() {/*Old Print*/}
	}
        class ExprefTestClass_inter_inter_exc_C2 : ExprefTestClass_inter_inter_exc_C1, ExprefTestClass_inter_inter_exc_I2
	{
		public void	j() {/*Old Print*/}
	}
	public class ExprefTestClass_inter_inter_exc 
	{
	    public static bool testMethod() 
	    {
            ExprefTestClass_inter_inter_exc_I2 i2 = (ExprefTestClass_inter_inter_exc_I2)new ExprefTestClass_inter_inter_exc_C2();
			try
			{
                ExprefTestClass_inter_inter_exc_I3 i3 = (ExprefTestClass_inter_inter_exc_I3)i2;
			}
			catch (System.Exception e)
			{
				//Old Print
			}
            return true;
        } 
    }
    }
}
