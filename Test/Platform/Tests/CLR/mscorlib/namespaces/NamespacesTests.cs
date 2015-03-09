////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

using NS_TestClass_compunit_04_first;
using NS_TestClass_compunit_04_second;

using NS_TestClass_alias_10_F = NS_TestClass_alias_10_Foo.Bar;


namespace Microsoft.SPOT.Platform.Tests
{
    [CLSCompliant(false)]
    public class NamespacesTests : IMFTestInterface
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

        //Namespaces Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\Namespaces
        //attribute_01,attribute_02,compunit_01,compunit_03,compunit_04,decl_01,decl_02,decl_03,decl_04,decl_05,decl_06,decl_08,decl_10,decl_11,decl_12,decl_13,decl_14,decl_15,decl_17,decl_20,decl_21,alias_01,alias_02,alias_03,alias_04,alias_10,alias_13,alias_14,direct_01,direct_02,direct_03,direct_05,typedecl_01,typedecl_06,typedecl_07,typedecl_08,typedecl_09,typedecl_10,typedecl_11,typedecl_12,typedecl_13,validaccess_01,validaccess_02,validaccess_03,validaccess_04,validaccess_05,contexts_025

        //Test Case Calls 
        [TestMethod]
        public MFTestResults NS_attribute_01_Test()
        {
            Log.Comment("Section 9.1 Compilation Units");
            Log.Comment("Verify that the CLSCompliant attribute is allowed at the assembly level in a comp. unit");
            if (NS_TestClass_attribute_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_attribute_02_Test()
        {
            Log.Comment("Section 9.1 Compilation Units");
            Log.Comment("Verify that the CLSCompliant attribute is allowed at the module level in a comp. unit (cause a warning)");
            if (NS_TestClass_attribute_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_compunit_01_Test()
        {
            Log.Comment("Section 9.1 Compilation Units");
            Log.Comment("Verify that a program with multiple files depending on each other works");
            if (NS_TestClass_compunit_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_compunit_03_Test()
        {
            Log.Comment("Section 9.1 Compilation Units");
            Log.Comment("Verify three source files all requiring each other in circular fashion compiles/runs successfully");
            if (NS_TestClass_compunit_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_compunit_04_Test()
        {
            Log.Comment("Section 9.1 Compilation Units");
            Log.Comment("Two source files with their own namespaces, each have a class named A, and can be used successfully by a third file");
            if (NS_TestClass_compunit_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_decl_01_Test()
        {
            Log.Comment("Section 9.2 Namespace Declarations");
            Log.Comment("Declare the simplest namespace possible (without semi-colon at the end)");
            if (NS_TestClass_decl_01.A.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_decl_02_Test()
        {
            Log.Comment("Section 9.2 Namespace Declarations");
            Log.Comment("Declare the simplest namespace possible (with semi-colon at the end)");
            if (NS_TestClass_decl_02.A.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_decl_03_Test()
        {
            Log.Comment("Section 9.2 Namespace Declarations");
            Log.Comment("Declare a namespace inside another namespace");
            if (NS_TestClass_decl_03.A.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_decl_04_Test()
        {
            Log.Comment("Section 9.2 Namespace Declarations");
            Log.Comment("Declare a namespace inside another namespace (with semicolon at end of nested ns decl)");
            if (NS_TestClass_decl_04.A.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_decl_05_Test()
        {
            Log.Comment("Section 9.2 Namespace Declarations");
            Log.Comment("Two separately declared namespaces in one compilation unit (file)");
            if (NS_TestClass_decl_05.A.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_decl_06_Test()
        {
            Log.Comment("Section 9.2 Namespace Declarations");
            Log.Comment("Two separately declared namespaces in one compilation unit (file)");
            if (NS_TestClass_decl_06.A.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_decl_08_Test()
        {
            Log.Comment("Section 9.2 Namespace Declarations");
            Log.Comment("Access class member declared in external namespace directly when adding a Using");
            if (NS_TestClass_decl_08.A.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_decl_10_Test()
        {
            Log.Comment("Section 9.2 Namespace Declarations");
            Log.Comment("Namespace identifier with . in it");
            if (NS_TestClass_decl_10.foo.A.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_decl_11_Test()
        {
            Log.Comment("Section 9.2 Namespace Declarations");
            Log.Comment("Namespace identifier with .'s in it and underscores, etc.");
            if (NS_TestClass_decl_11.foo.A.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_decl_12_Test()
        {
            Log.Comment("Section 9.2 Namespace Declarations");
            Log.Comment("Two separate namespace declarations contribute to same declaration space (with using)");
            if (NS_TestClass_decl_12.foo.A.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_decl_13_Test()
        {
            Log.Comment("Section 9.2 Namespace Declarations");
            Log.Comment("Two separate namespace declarations contribute to same declaration space (direct referencing)");
            if (NS_TestClass_decl_13.foo.A.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_decl_14_Test()
        {
            Log.Comment("Section 9.2 Namespace Declarations");
            Log.Comment("Two separate namespace declarations contribute to same declaration space (direct referencing)");
            Log.Comment("   namespace declarations are in two different compilation units (files)");
            if (NS_TestClass_decl_14.foo.A.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_decl_15_Test()
        {
            Log.Comment("Section 9.2 Namespace Declarations");
            Log.Comment("Two separate namespace declarations contribute to same declaration space (with using)");
            Log.Comment("   namespace declarations are in two different compilation units (files)");
            if (NS_TestClass_decl_15.foo.A.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_decl_17_Test()
        {
            Log.Comment("Section 9.2 Namespace Declarations");
            Log.Comment("Declare a namespace called _Foo");
            if (NS_TestClass_decl_17.main.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_decl_20_Test()
        {
            Log.Comment("Section 9.2 Namespace Declarations");
            Log.Comment("Declare an empty namespace");
            if (NS_TestClass_decl_20.main.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_decl_21_Test()
        {
            Log.Comment("Section 9.2 Namespace Declarations");
            Log.Comment("Declare ten empty nested namespaces (with and without semi-colons)");
            if (NS_TestClass_decl_21.main.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_alias_01_Test()
        {
            Log.Comment("Section 9.3.1 Using alias directives ");
            Log.Comment("Verify simplest form of alias reference to a namespace");
            if (NS_TestClass_alias_01.A.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_alias_02_Test()
        {
            Log.Comment("Section 9.3.1 Using alias directives ");
            Log.Comment("Verify simplest form of alias reference to a namespace with .'s");
            if (NS_TestClass_alias_02.A.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_alias_03_Test()
        {
            Log.Comment("Section 9.3.1 Using alias directives ");
            Log.Comment("Verify simplest form of alias reference to a specific type in a namespace");
            if (NS_TestClass_alias_03.A.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_alias_04_Test()
        {
            Log.Comment("Section 9.3.1 Using alias directives ");
            Log.Comment("Verify simplest form of alias reference to a specific type in a namespace");
            if (NS_TestClass_alias_04.B.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_alias_10_Test()
        {
            Log.Comment("Section 9.3.1 Using alias directives ");
            Log.Comment("Third code example in 9.3.1 - unique alias identifiers");
            if (NS_TestClass_alias_10.main.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_alias_13_Test()
        {
            Log.Comment("Section 9.3.1 Using alias directives ");
            Log.Comment("A using-alias-directive can create an alias for the namespace in which it appears");
            if (NS_TestClass_alias_13.main.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_alias_14_Test()
        {
            Log.Comment("Section 9.3.1 Using namespace directives ");
            Log.Comment("A using-alias-directive can create an alias for a type declared within the ns in which it appears ");
            if (NS_TestClass_alias_14.main.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_direct_01_Test()
        {
            Log.Comment("Section 9.3.2 Using namespace directives ");
            Log.Comment("The members of a namespace can be accessed directly post using ");
            if (NS_TestClass_direct_01.main.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_direct_02_Test()
        {
            Log.Comment("Section 9.3.2 Using namespace directives ");
            Log.Comment("A newly declared class will hide a class (with the same name) brought in with a using directive");
            if (NS_TestClass_direct_02.main.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_direct_03_Test()
        {
            Log.Comment("Section 9.3.2 Using namespace directives ");
            Log.Comment("Names imported by using-namespace-directive are hidden by same-named members ");
            if (NS_TestClass_direct_03.main.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_direct_05_Test()
        {
            Log.Comment("Section 9.3.2 Using namespace directives ");
            Log.Comment("Ambiguous reference is overridden by using alias statement");
            if (NS_TestClass_direct_05.main.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_typedecl_01_Test()
        {
            Log.Comment("Section 9.5 Type declarations ");
            Log.Comment("Ambiguous reference is overridden by using alias statement");
            if (NS_TestClass_typedecl_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_typedecl_06_Test()
        {
            Log.Comment("Section 9.5 Type declarations ");
            Log.Comment("Declaring a class member as public is okay");
            if (NS_TestClass_typedecl_06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_typedecl_07_Test()
        {
            Log.Comment("Section 9.5 Type declarations ");
            Log.Comment("Declaring a class member as protected internal is okay");
            if (NS_TestClass_typedecl_07.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_typedecl_08_Test()
        {
            Log.Comment("Section 9.5 Type declarations ");
            Log.Comment("Declaring a class member as protected is okay");
            if (NS_TestClass_typedecl_08.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_typedecl_09_Test()
        {
            Log.Comment("Section 9.5 Type declarations ");
            Log.Comment("Declaring a class member as internal is okay");
            if (NS_TestClass_typedecl_09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_typedecl_10_Test()
        {
            Log.Comment("Section 9.5 Type declarations ");
            Log.Comment("Declaring a class member as private is okay");
            if (NS_TestClass_typedecl_10.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_typedecl_11_Test()
        {
            Log.Comment("Section 9.5 Type declarations ");
            Log.Comment("Declaring a struct member as public is okay");
            if (NS_TestClass_typedecl_11.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_typedecl_12_Test()
        {
            Log.Comment("Section 9.5 Type declarations ");
            Log.Comment("Declaring a struct member as internal is okay");
            if (NS_TestClass_typedecl_12.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_typedecl_13_Test()
        {
            Log.Comment("Section 9.5 Type declarations ");
            Log.Comment("Declaring a struct member as private is okay");
            if (NS_TestClass_typedecl_13.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_validaccess_01_Test()
        {
            Log.Comment("Section 9.5 Type declarations ");
            Log.Comment("Verify all valid accessibility levels for class declarations are allowed");
            if (NS_TestClass_validaccess_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_validaccess_02_Test()
        {
            Log.Comment("Section 9.5 Type declarations ");
            Log.Comment("Verify all valid accessibility levels for struct declarations are allowed");
            if (NS_TestClass_validaccess_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_validaccess_03_Test()
        {
            Log.Comment("Section 9.5 Type declarations ");
            Log.Comment("Verify all valid accessibility levels for interface declarations are allowed");
            if (NS_TestClass_validaccess_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_validaccess_04_Test()
        {
            Log.Comment("Section 9.5 Type declarations ");
            Log.Comment("Verify all valid accessibility levels for enum declarations are allowed");
            if (NS_TestClass_validaccess_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_validaccess_05_Test()
        {
            Log.Comment("Section 9.5 Type declarations ");
            Log.Comment("Verify all valid accessibility levels for delegate declarations are allowed");
            if (NS_TestClass_validaccess_05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults NS_contexts_025_Test()
        {
            Log.Comment("Verify that you can use the qualifier in a cref attribute of xml docs tag");
            if (NS_TestClass_contexts_025.Test.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }


        class NS_TestClass_compunit_01
        {
            public static void Main_old(String[] args)
            {
                NS_TestClass_compunit_01A cA = new NS_TestClass_compunit_01A();
                NS_TestClass_compunit_01B cB = new NS_TestClass_compunit_01B();

                cA.printClassName();
                cB.printClassName();
                Log.Comment("Class C");
            }

            public static bool testMethod()
            {
                Main_old(null);
                return true;
            }
        }





        class NS_TestClass_compunit_04
        {
            public static void Main_old(String[] args)
            {
                NS_TestClass_compunit_04_first.NS_TestClass_compunit_04A cA = new NS_TestClass_compunit_04_first.NS_TestClass_compunit_04A();
                NS_TestClass_compunit_04_second.NS_TestClass_compunit_04A cB = new NS_TestClass_compunit_04_second.NS_TestClass_compunit_04A();

                cA.printClassName();
                cB.printClassName();
                Log.Comment("Class C");
            }

            public static bool testMethod()
            {
                Main_old(null);
                return true;
            }
        }
        //Compiled Test Cases 
        public class NS_TestClass_typedecl_01_A { }
        internal class B { }
        class NS_TestClass_typedecl_01
        {
            public static int Main_old()
            {
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class NS_TestClass_typedecl_06_A
        {
            public int a;
        }
        class NS_TestClass_typedecl_06
        {
            public static int Main_old()
            {
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class NS_TestClass_typedecl_07_A
        {
            protected internal int a;
        }
        class NS_TestClass_typedecl_07
        {
            public static int Main_old()
            {
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class NS_TestClass_typedecl_08_A
        {
            protected int a;
        }
        class NS_TestClass_typedecl_08
        {
            public static int Main_old()
            {
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class NS_TestClass_typedecl_09_A
        {
            internal int a;
        }
        class NS_TestClass_typedecl_09
        {
            public static int Main_old()
            {
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class NS_TestClass_typedecl_10_A
        {
            private int a;
            public void TheMethod()
            {
                // This code is here basically to avoid any "not using a" compiler warnings
                a = 5;
                if (a == 5)
                    a++;
            }
        }
        class NS_TestClass_typedecl_10
        {
            public static int Main_old()
            {
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        struct NS_TestClass_typedecl_11_A
        {
            public int a;
        }
        class NS_TestClass_typedecl_11
        {
            public static int Main_old()
            {
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        struct NS_TestClass_typedecl_12_A
        {
            internal int a;
        }
        class NS_TestClass_typedecl_12
        {
            public static int Main_old()
            {
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        struct NS_TestClass_typedecl_13_A
        {
            private int a;
            public void MyMethod()
            {
                // This code is here to avoid the "a not used" compiler warning
                a = 5;
                if (a == 5)
                    a++;
            }
        }
        class NS_TestClass_typedecl_13
        {
            public static int Main_old()
            {
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }


    }
}

class NS_TestClass_compunit_03
{
    public void printClassName()
    {
        NS_TestClass_compunit_03A cA = new NS_TestClass_compunit_03A();
        cA.showName();
    }

    public void showName()
    {
        Log.Comment("Class C");
    }

    public static void Main_old(String[] args)
    {
        NS_TestClass_compunit_03A mainA = new NS_TestClass_compunit_03A();
        NS_TestClass_compunit_03B mainB = new NS_TestClass_compunit_03B();
        NS_TestClass_compunit_03 mainC = new NS_TestClass_compunit_03();

        mainA.printClassName();
        mainB.printClassName();
        mainC.printClassName();
    }

    public static bool testMethod()
    {
        Main_old(null);
        return true;
    }
}

namespace NS_TestClass_decl_01
{
	using System;
	class A
	{
		public static void Main_old(String[] args)
		{
			Log.Comment("This worked!");
		}
public static bool testMethod()
{
	Main_old(null);
	return true;
}
	}
}
namespace NS_TestClass_decl_02
{
	using System;
	class A
	{
		public static void Main_old(String[] args)
		{
			Log.Comment("This worked!");
		}
public static bool testMethod()
{
	Main_old(null);
	return true;
}
	}
}
namespace NS_TestClass_decl_03
{
	using System;
	namespace innertest
	{
		class B
		{
			public int i;
		}
	}
	class A
	{
		public static void Main_old(String[] args)
		{
			innertest.B b = new innertest.B();
			b.i = 3;

			Log.Comment((b.i).ToString());
		}
public static bool testMethod()
{
	Main_old(null);
	return true;
}
	}
}
namespace NS_TestClass_decl_04
{
	using System;
	namespace innertest
	{
		class B
		{
			public int i;
		}
	};
	class A
	{
		public static void Main_old(String[] args)
		{
			innertest.B b = new innertest.B();
			b.i = 3;

			Log.Comment((b.i).ToString());
		}
public static bool testMethod()
{
	Main_old(null);
	return true;
}
	}
}
namespace NS_TestClass_decl_05_other
{
	class B 
	{
		public int i;
	}
}
namespace NS_TestClass_decl_05
{
	using System;
	class A
	{
		public static void Main_old(String[] args)
		{
			NS_TestClass_decl_05_other.B b = new NS_TestClass_decl_05_other.B();
			b.i = 500;
			Log.Comment((b.i).ToString());
		}
public static bool testMethod()
{
	Main_old(null);
	return true;
}
	}
}
namespace NS_TestClass_decl_06_other
{
	namespace inothertest
	{
		class B 
		{
			public int i;
		}
	}
}
namespace NS_TestClass_decl_06
{
	using System;
	class A
	{
		public static void Main_old(String[] args)
		{
			NS_TestClass_decl_06_other.inothertest.B b = new NS_TestClass_decl_06_other.inothertest.B();
			b.i = 500;
			Log.Comment((b.i).ToString());
		}
public static bool testMethod()
{
	Main_old(null);
	return true;
}
	}
}
namespace other
{
	class B
	{
		public int i;
	}
}
namespace NS_TestClass_decl_08
{
	using System;
	using other;
	class A
	{
		public static void Main_old(String[] args)
		{
			B b = new B();
			b.i = 3;
			Log.Comment((b.i).ToString());
		}
public static bool testMethod()
{
	Main_old(null);
	return true;
}
	}
}
namespace NS_TestClass_decl_10_one.NS_TestClass_decl_10_two.NS_TestClass_decl_10_three
{
	class B
	{
		public int i;
	}
}
namespace NS_TestClass_decl_10.foo
{
	using System;
	class A
	{
		public static void Main_old(String[] args)
		{
			NS_TestClass_decl_10_one.NS_TestClass_decl_10_two.NS_TestClass_decl_10_three.B b = new NS_TestClass_decl_10_one.NS_TestClass_decl_10_two.NS_TestClass_decl_10_three.B();
			b.i = 500;

			Log.Comment((b.i).ToString());
		}
public static bool testMethod()
{
	Main_old(null);
	return true;
}
	}
}
namespace NS_TestClass_decl_11_one._two._three_
{
	class B
	{
		public int i;
	}
}
namespace ___space.word_
{
	class C
	{
		public int j;
	}
}
namespace NS_TestClass_decl_11.foo
{
	using System;
	using ___space.word_;
	class A
	{
		public static void Main_old(String[] args)
		{
			NS_TestClass_decl_11_one._two._three_.B b = new NS_TestClass_decl_11_one._two._three_.B();
			C c = new C();

			b.i = 500;
			c.j = 300;
			Log.Comment((b.i).ToString());
			Log.Comment((c.j).ToString());
		}
public static bool testMethod()
{
	Main_old(null);
	return true;
}
	}
}
namespace NS_TestClass_decl_12_one.NS_TestClass_decl_12_two.NS_TestClass_decl_12_three
{
	class B
	{
		public int i;
	}
}
namespace NS_TestClass_decl_12_one.NS_TestClass_decl_12_two.NS_TestClass_decl_12_three
{
	class C
	{
		public int j;
	}
}
namespace NS_TestClass_decl_12.foo
{
	using System;
    using NS_TestClass_decl_12_one.NS_TestClass_decl_12_two.NS_TestClass_decl_12_three;
	class A
	{
		public static void Main_old(String[] args)
		{
			B b = new B();
			C c = new C();

			b.i = 500;
			c.j = 300;
			Log.Comment((b.i).ToString());
			Log.Comment((c.j).ToString());
		}
public static bool testMethod()
{
	Main_old(null);
	return true;
}
	}
}
namespace NS_TestClass_decl_13_one.NS_TestClass_decl_13_two.NS_TestClass_decl_13_three
{
	class B
	{
		public int i;
	}
}
namespace NS_TestClass_decl_13_one.NS_TestClass_decl_13_two.NS_TestClass_decl_13_three
{
	class C
	{
		public int j;
	}
}
namespace NS_TestClass_decl_13.foo
{
	using System;
	class A
	{
		public static void Main_old(String[] args)
		{
			NS_TestClass_decl_13_one.NS_TestClass_decl_13_two.NS_TestClass_decl_13_three.B b = new NS_TestClass_decl_13_one.NS_TestClass_decl_13_two.NS_TestClass_decl_13_three.B();
			NS_TestClass_decl_13_one.NS_TestClass_decl_13_two.NS_TestClass_decl_13_three.C c = new NS_TestClass_decl_13_one.NS_TestClass_decl_13_two.NS_TestClass_decl_13_three.C();

			b.i = 500;
			c.j = 300;
			Log.Comment((b.i).ToString());
			Log.Comment((c.j).ToString());
		}
public static bool testMethod()
{
	Main_old(null);
	return true;
}
	}
}
namespace NS_TestClass_decl_14_one.NS_TestClass_decl_14_two.NS_TestClass_decl_14_three
{
	class B
	{
		public int i;
	}
}

namespace NS_TestClass_decl_14.foo
{
	using System;

	class A
	{
		public static void Main_old(String[] args)
		{
			NS_TestClass_decl_14_one.NS_TestClass_decl_14_two.NS_TestClass_decl_14_three.B b = new NS_TestClass_decl_14_one.NS_TestClass_decl_14_two.NS_TestClass_decl_14_three.B();
			NS_TestClass_decl_14_one.NS_TestClass_decl_14_two.NS_TestClass_decl_14_three.C c = new NS_TestClass_decl_14_one.NS_TestClass_decl_14_two.NS_TestClass_decl_14_three.C();
			
			b.i = 500;
			c.j = 300;
			Log.Comment((b.i).ToString());
			Log.Comment((c.j).ToString());
		}

public static bool testMethod()
{
	Main_old(null);
	return true;
}
	}
}

namespace NS_TestClass_decl_15_one.NS_TestClass_decl_15_two.NS_TestClass_decl_15_three
{
	class B
	{
		public int i;
	}
}

namespace NS_TestClass_decl_15.foo
{
	using System;
	using NS_TestClass_decl_15_one.NS_TestClass_decl_15_two.NS_TestClass_decl_15_three;

	class A
	{
		public static void Main_old(String[] args)
		{
			B b = new B();
			C c = new C();
			
			b.i = 500;
			c.j = 300;
			Log.Comment((b.i).ToString());
			Log.Comment((c.j).ToString());
		}

public static bool testMethod()
{
	Main_old(null);
	return true;
}
	}
}


namespace NS_TestClass_decl_17_Foo 
{
	class A 
	{
		public void print() 
		{
			Log.Comment("Test");
		}
	}
}
namespace NS_TestClass_decl_17
{
	class main
	{
		public static void Main_old() 
		{
            NS_TestClass_decl_17_Foo.A a = new NS_TestClass_decl_17_Foo.A();
			a.print();
		}
public static bool testMethod()
{
	Main_old();
	return true;
}
	}
}
namespace NS_TestClass_decl_20_test
{
}
namespace NS_TestClass_decl_20
{
	class main 
	{
		public static void Main_old() {}
public static bool testMethod()
{
	Main_old();
	return true;
}
	}
}
namespace NS_TestClass_decl_21_test {
	namespace test2 {
		namespace test3 {
			namespace test4 {
				namespace test5 {
					namespace test6 {
						namespace test7 {
							namespace test8 {
								namespace test9 {
									namespace test10 {
										class TheClass 
										{ 
											public void SayHello() {
												Log.Comment("Hello");
											}
										}
									}
								};
							};
						}
					}
				};
			};
		}
	};
}
namespace NS_TestClass_decl_21 {
	class main {
		public static void Main_old() {
			NS_TestClass_decl_21_test.test2.test3.test4.test5.test6.test7.test8.test9.test10.TheClass tc;
			tc = new NS_TestClass_decl_21_test.test2.test3.test4.test5.test6.test7.test8.test9.test10.TheClass();
			tc.SayHello();
		}
public static bool testMethod()
{
	Main_old();
	return true;
}
	}
}

namespace NS_TestClass_alias_01_externalnamespace
{
    class B
    {
        public int i;
    }
}

namespace NS_TestClass_alias_01
{
    using System;
    using en = NS_TestClass_alias_01_externalnamespace;

    class A
    {

        public static void Main_old(String[] args)
        {
            en.B b = new en.B();
            b.i = 3;

            Log.Comment((b.i).ToString());
        }

        public static bool testMethod()
        {
            Main_old(null);
            return true;
        }
    }
}

namespace NS_TestClass_alias_02_test.nspace
{
    class B
    {
        public int i;
    }
}

namespace NS_TestClass_alias_02
{
    using System;
    using tn = NS_TestClass_alias_02_test.nspace;

    class A
    {

        public static void Main_old(String[] args)
        {
            tn.B b = new tn.B();
            b.i = 3;

            Log.Comment((b.i).ToString());
        }

        public static bool testMethod()
        {
            Main_old(null);
            return true;
        }
    }
}
namespace NS_TestClass_alias_03_test.nspace
{
    class B
    {
        public int i;
    }
}

namespace NS_TestClass_alias_03
{
    using System;
    using tn = NS_TestClass_alias_03_test.nspace.B;

    class A
    {

        public static void Main_old(String[] args)
        {
            tn b = new tn();
            b.i = 3;

            Log.Comment((b.i).ToString());
        }

        public static bool testMethod()
        {
            Main_old(null);
            return true;
        }
    }
}

namespace NS_TestClass_alias_04_Foo.Bar
{
    class A { }
}

namespace NS_TestClass_alias_04
{
    using F = NS_TestClass_alias_04_Foo.Bar;
    using System;

    class B : F.A
    {
        public static void Main_old(String[] args)
        {
            Log.Comment("Worked!");
        }

        public static bool testMethod()
        {
            Main_old(null);
            return true;
        }
    }
}



namespace NS_TestClass_alias_10_Foo.Bar
{
	class A {}
}

namespace NS_TestClass_alias_10_Baz
{
	class B : NS_TestClass_alias_10_F.A {}
}

namespace NS_TestClass_alias_10
{
	using System;

	class C : NS_TestClass_alias_10_F.A {}

	class main
	{
		public static int Main_old() 
		{
			return 0;
		}

public static bool testMethod()
{
	return (Main_old() == 0);
}
	}
}

namespace NS_TestClass_alias_13
{
    using tns = NS_TestClass_alias_13;
    using System;

    class main
    {
        public static int Main_old()
        {
            return 0;
        }

        public static bool testMethod()
        {
            return (Main_old() == 0);
        }
    }
}

namespace NS_TestClass_alias_14
{
    using tns = NS_TestClass_alias_14.main;
    using System;

    class main
    {
        public static int Main_old()
        {
            return 0;
        }

        public static bool testMethod()
        {
            return (Main_old() == 0);
        }
    }
}

namespace NS_TestClass_direct_01_Foo.Bar
{
    class A { }
}

namespace NS_TestClass_direct_01
{
    using NS_TestClass_direct_01_Foo.Bar;
    using System;

    class B : A { }

    class main
    {

        public static int Main_old()
        {
            return 0;
        }

        public static bool testMethod()
        {
            return (Main_old() == 0);
        }
    }
}

namespace NS_TestClass_direct_02_Foo.Bar
{
    class A
    {
        public void showMessage()
        {
            Log.Comment("Foo.Bar.A");
        }
    }
}

namespace NS_TestClass_direct_02
{
    using NS_TestClass_direct_02_Foo.Bar;
    using System;

    class A
    {
        public void showMessage()
        {
            Log.Comment("testns.A");
        }
    }

    class main
    {
        public static void Main_old()
        {
            A a = new A();
            a.showMessage();
        }

        public static bool testMethod()
        {
            Main_old();
            return true;
        }
    }
}

namespace NS_TestClass_direct_03_Foo.Bar
{
    class A { }
}

namespace NS_TestClass_direct_03
{
    using NS_TestClass_direct_03_Foo.Bar;
    using System;

    class A
    {
        public int i;
    }

    class main
    {

        public static int Main_old()
        {
            A a = new A();
            a.i = 25;

            Log.Comment("This works!");

            return 0;
        }

        public static bool testMethod()
        {
            return (Main_old() == 0);
        }
    }
}

namespace NS_TestClass_direct_05_Foo
{
    class A { }
}

namespace NS_TestClass_direct_05_Bar
{
    class A { }
}

namespace NS_TestClass_direct_05
{
    using NS_TestClass_direct_05_Foo;
    using NS_TestClass_direct_05_Bar;
    using A = NS_TestClass_direct_05_Foo.A;
    using System;

    class main
    {
        public static int Main_old()
        {
            A a = new A();

            Log.Comment("This works!");

            return 0;
        }

        public static bool testMethod()
        {
            return (Main_old() == 0);
        }
    }
}

class C1 { }
public class C2 { }
internal class C3 { }

namespace N
{
    class C4 { }
    public class C5 { }
    internal class C6 { }
}

class C
{
    class C7 { }
    public class C8 { }
    protected internal class C9 { }
    protected class C10 { }
    internal class C11 { }
    private class C12 { }
}

struct S
{
    class C13 { }
    public class C14 { }
    internal class C15 { }
    private class C16 { }
}

class NS_TestClass_validaccess_01
{
    static void Main_old() { }

    public static bool testMethod()
    {
        Main_old();
        return true;
    }
}

struct S1 { }
public struct S2 { }
internal struct S3 { }

namespace N
{
    struct S4 { }
    public struct S5 { }
    internal struct S6 { }
}

class C_02
{
    struct S7 { }
    public struct S8 { }
    protected internal struct S9 { }
    protected struct S10 { }
    internal struct S11 { }
    private struct S12 { }
}

struct S_02
{
    struct S13 { }
    public struct S14 { }
    internal struct S15 { }
    private struct S16 { }
}

class NS_TestClass_validaccess_02
{
    static void Main_old() { }

    public static bool testMethod()
    {
        Main_old();
        return true;
    }
}

interface I1 { }
public interface I2 { }
internal interface I3 { }

namespace N1
{
    interface I4 { }
    public interface I5 { }
    internal interface I6 { }
}

class C1_03
{
    interface I7 { }
    public interface I8 { }
    protected internal interface I9 { }
    protected interface I10 { }
    internal interface I11 { }
    private interface I12 { }
}

struct S1_03
{
    interface I13 { }
    public interface I14 { }
    internal interface I15 { }
    private interface I16 { }
}

class NS_TestClass_validaccess_03
{
    static void Main_old() { }

    public static bool testMethod()
    {
        Main_old();
        return true;
    }
}



enum E1 { }
public enum E2 { }
internal enum E3 { }

namespace N1
{
    enum E4 { }
    public enum E5 { }
    internal enum E6 { }
}

class C1_04
{
    enum E7 { }
    public enum E8 { }
    protected internal enum E9 { }
    protected enum E10 { }
    internal enum E11 { }
    private enum E12 { }
}

struct S1_04
{
    enum E13 { }
    public enum E14 { }
    internal enum E15 { }
    private enum E16 { }
}

class NS_TestClass_validaccess_04
{
    static void Main_old() { }

    public static bool testMethod()
    {
        Main_old();
        return true;
    }
}

delegate void D1();
public delegate void D2();
internal delegate void D3();

namespace N1
{
    delegate void D4();
    public delegate void D5();
    internal delegate void D6();
}

class C1_05
{
    delegate void D7();
    public delegate void D8();
    protected internal delegate void D9();
    protected delegate void D10();
    internal delegate void D11();
    private delegate void D12();
}

struct S1_05
{
    delegate void D13();
    public delegate void D14();
    internal delegate void D15();
    private delegate void D16();
}

class NS_TestClass_validaccess_05
{
    static void Main_old() { }

    public static bool testMethod()
    {
        Main_old();
        return true;
    }
}

namespace NS_TestClass_contexts_025
{
    class Test
    {
        /// <System.Exception cref="::System.Exception"></System.Exception>
        public static void M1()
        {
            throw new System.Exception();
        }

        static int Main_old()
        {
            try
            {
                NS_TestClass_contexts_025.Test.M1();
            }
            catch
            {
                return 0;
            }

            return 1;
        }

        public static bool testMethod()
        {
            return (Main_old() == 0);
        }
    }
}
