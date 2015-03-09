////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ImprefTests : IMFTestInterface
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
            Log.Comment("Cleaning up after the tests");
        }

        //Impref Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\Impref
        //ref_obj,class_class,class_inter,struct_inter,array_array,array_cloneable,null_ref,delegate_to_System,Delegate
        //All of these tests passed in the Baseline document
        //delegate_to_System.ICloneable was not ported it was skipped in the Baseline document

        //Test Case Calls 
        [TestMethod]
        public MFTestResults Impref_ref_obj_Test()
        {
            Log.Comment(" Converting from a reference object to 'object'");
            if (ImprefTestClass_ref_obj.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Impref_class_class_Test()
        {
            Log.Comment(" Tests that you can convert from a class to a base class. ");
            if (ImprefTestClass_class_class.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Impref_class_inter_Test()
        {
            Log.Comment(" Tests that you can convert from a class to an interface that it implements. ");
            if (ImprefTestClass_class_inter.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Impref_struct_inter_Test()
        {
            Log.Comment(" Tests that you can convert from a struct to an interface that it implements. ");
            if (ImprefTestClass_struct_inter.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Impref_array_array_Test()
        {
            Log.Comment(" Tests that you can convert from an array of one class to an array of another class...");
            if (ImprefTestClass_array_array.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Impref_array_cloneable_Test()
        {
            Log.Comment(" Tests that you can convert from an array to System.ICloneable;");
            if (ImprefTestClass_array_cloneable.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Impref_null_ref_Test()
        {
            Log.Comment(" Tests that you can convert from null to several reference types");
            if (ImprefTestClass_null_ref.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Impref_delegate_to_SystemDotDelegate_Test()
        {
            Log.Comment(" Tests that you can convert from a delegate type to System.Delegate");
            if (ImprefTestClass_delegate_to_SystemDotDelegate.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }



        //Compiled Test Cases 
        public class ImprefTestClass_ref_obj_Sub
        {
	        int func() {return(1);}
        }
        public class ImprefTestClass_ref_obj 
        {
            public static bool testMethod() 
            {
		        ImprefTestClass_ref_obj_Sub	test = new ImprefTestClass_ref_obj_Sub();
		        object	obj;
		        obj = test;
                return true;
            } 
        }

	        class	ImprefTestClass_class_class_Der1
	        {
		        void	i() {}
	        }

	        class	ImprefTestClass_class_class_Der2 : ImprefTestClass_class_class_Der1
	        {
		        void	j() {}
	        }
	        public class ImprefTestClass_class_class 
	        {
	            public static bool testMethod() 
	            {
			        ImprefTestClass_class_class_Der2	derivedClass = new ImprefTestClass_class_class_Der2();

			        ImprefTestClass_class_class_Der1	ImprefTestClass_class_class_Base;
			        ImprefTestClass_class_class_Base = derivedClass;
                    return true;
	            } 
	        }

	        interface	ImprefTestClass_class_inter_Interface1
	        {
		        void	i();
	        }
	        interface	ImprefTestClass_class_inter_Interface2
	        {
		        void	j();
	        }

	        class	ImprefTestClass_class_inter_Sub : ImprefTestClass_class_inter_Interface1, ImprefTestClass_class_inter_Interface2
	        {
		        public void	i() {}
		        public void	j() {}
	        }
	        public class ImprefTestClass_class_inter 
	        {
	            public static bool testMethod() 
	            {
			        ImprefTestClass_class_inter_Sub	ImprefTestClass_class_inter_Sub = new ImprefTestClass_class_inter_Sub();
			        ImprefTestClass_class_inter_Interface2		inter2;
			        ImprefTestClass_class_inter_Interface1		inter1;
			        inter1 = ImprefTestClass_class_inter_Sub;
			        inter2 = ImprefTestClass_class_inter_Sub;
                    return true;
	            } 
                
            }

	        interface	ImprefTestClass_struct_inter_Interface1
	        {
		        void	i();
	        }
	        interface	ImprefTestClass_struct_inter_Interface2
	        {
		        void	j();
	        }

	        struct	ImprefTestClass_struct_inter_Sub : ImprefTestClass_struct_inter_Interface1, ImprefTestClass_struct_inter_Interface2
	        {
		        public void	i() {}
		        public void	j() {}
	        }
	        public class ImprefTestClass_struct_inter 
	        {
	            public static bool testMethod() 
	            {
			        ImprefTestClass_struct_inter_Sub	ImprefTestClass_struct_inter_Sub = new ImprefTestClass_struct_inter_Sub();
			        ImprefTestClass_struct_inter_Interface2		inter2;
			        ImprefTestClass_struct_inter_Interface1		inter1;
			        inter2 = ImprefTestClass_struct_inter_Sub;
			        inter1 = ImprefTestClass_struct_inter_Sub;
	                return true;
                }
            }

	        class	ImprefTestClass_array_array_Base1
	        {
		        public void	i() {}
	        }

	        class	ImprefTestClass_array_array_Base2 : ImprefTestClass_array_array_Base1
	        {
		        public void	j() {}
	        }
	        public class ImprefTestClass_array_array 
	        {
	            public static bool testMethod() 
	            {
			        ImprefTestClass_array_array_Base2[]	arrDer = new ImprefTestClass_array_array_Base2[1];
			        ImprefTestClass_array_array_Base2	element = new ImprefTestClass_array_array_Base2();
			        arrDer[0] = element;
			        ImprefTestClass_array_array_Base1[]	arrBase = new ImprefTestClass_array_array_Base1[1];
			        arrBase = arrDer;
			        element.j();
			        arrBase[0].i();
                    return true;
	            } 
            }

            class ImprefTestClass_array_cloneable_Derived
	        {
		        public void	i() {}
	        }

	        public class ImprefTestClass_array_cloneable 
	        {
	            public static bool testMethod() 
	            {
                    ImprefTestClass_array_cloneable_Derived[] arrBase = new ImprefTestClass_array_cloneable_Derived[1];
			        ICloneable	clone;
			        clone = arrBase;
                    return true;
	            } 
            }

	    class	ImprefTestClass_null_ref_Derived
	    {
		    public void	i() {}
	    }
	    public class ImprefTestClass_null_ref 
	    {
	        public static bool testMethod() 
	        {
			    ImprefTestClass_null_ref_Derived	classDer1;
			    String		string1;
			    classDer1 = null;
			    string1 = null;
                return true;
	        } 
        }

        delegate void ImprefTestClass_delegate_to_SystemDotDelegate_Delegate();
        class ImprefTestClass_delegate_to_SystemDotDelegate
        {
	        public static void DoNothing() { }
	        public static bool testMethod()
	        {
                ImprefTestClass_delegate_to_SystemDotDelegate_Delegate src = new ImprefTestClass_delegate_to_SystemDotDelegate_Delegate(ImprefTestClass_delegate_to_SystemDotDelegate.DoNothing);
		        System.Delegate dst = src;
		        src();
                return true;
	        }
        }


    }
}
