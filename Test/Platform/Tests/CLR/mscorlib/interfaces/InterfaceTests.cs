////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class InterfaceTests : IMFTestInterface
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

        //Interface Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\Interface
        //struct_impl_01,struct_impl_02,struct_impl_04,struct_impl_05,struct_impl_06,struct_impl_07,struct_impl_08,struct_impl_09,struct_explicit_01,struct_explicit_03,struct_explicit_04,struct_explicit_05,struct_explicit_06,struct_explicit_07,struct_explicit_09,struct_explicit_10,struct_explicit_11,struct_explicit_12,struct_explicit_13,struct_explicit_19,struct_explicit_20,struct_decl_01,struct_decl_02,struct_inherit_01,struct_modifier_01,struct_modifier_02,struct_modifier_03,struct_semicolon_01,struct_semicolon_02


        //Test Case Calls 
        [TestMethod]
        public MFTestResults Interface_base_06_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("If effective visibility is internal, public iface may have an internal base iface ");
            Log.Comment("(Bug 86453: has some details on this.  But it has to do with class  internal)");
            if (Interface_TestClass_base_06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_decl_01_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Simplest interface declared in global namespace");
            if (Interface_TestClass_decl_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Interface_decl_03_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Simplest interface declared in class in default global namespace");
            if (Interface_TestClass_decl_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Interface_decl_05_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Simple interface declaration with diff types of methods, args, and properties");
            if (Interface_TestClass_decl_05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_decl_06_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Simple interface declaration properties with only gets");
            if (Interface_TestClass_decl_06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_decl_07_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Simple interface declaration properties with only sets");
            if (Interface_TestClass_decl_07.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_inherit_01_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Verify that an interface can be hidden by 'new' member in derived class");
            if (Interface_TestClass_inherit_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_inherit_02_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Simplest form of interface inheritance");
            if (Interface_TestClass_inherit_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_inherit_03_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Simplest form of interface inheritance, inheriting multiple interfaces");
            if (Interface_TestClass_inherit_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_inherit_04_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Simplest form of interface inheritance, inheriting multiple interfaces");
            if (Interface_TestClass_inherit_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_inherit_08_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Verify that an interface hidden without using new results in warning");
            if (Interface_TestClass_inherit_08.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_modifier_01_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Verify public and internal interfaces declared in global namespace are valid");
            if (Interface_TestClass_modifier_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_modifier_05_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Verify public and internal interfaces are valid inside classes");
            if (Interface_TestClass_modifier_05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_modifier_06_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Verify private and protected interfaces are valid inside classes");
            if (Interface_TestClass_modifier_06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_modifier_07_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Verify new interface gens warning if not hiding anything");
            if (Interface_TestClass_modifier_07.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_modifier_08_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Verify new interface is valid inside classes when properly hiding an inherited member");
            if (Interface_TestClass_modifier_08.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_modifier_10_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Verify protected internal interfaces are valid inside classes");
            if (Interface_TestClass_modifier_10.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_semicolon_01_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Simplest interface declared (with semi-colon) in global namespace");
            if (Interface_TestClass_semicolon_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Interface_semicolon_03_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Simplest interface declared (with semi-colon) in  class in default global namespace");
            if (Interface_TestClass_semicolon_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }


        [TestMethod]
        public MFTestResults Interface_impl_04_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("A member of an interface-type can be assigned an instance of class that implements it");
            if (Interface_TestClass_impl_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_impl_05_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Warning gets generated when two methods implement an inherited method");
            if (Interface_TestClass_impl_05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_impl_06_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Using the 'override' keyword allows you to hide previous method implementation");
            if (Interface_TestClass_impl_06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_impl_07_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("A class that implements an interface with method, property, event, and indexer");
            if (Interface_TestClass_impl_07.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_impl_08_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Implementing method that was defined identically by two different base-interfaces should work");
            if (Interface_TestClass_impl_08.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_impl_09_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Implementing property that was defined identically by two different base-interfaces should work");
            if (Interface_TestClass_impl_09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_impl_10_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Implementing an event that was defined identically by two different base-interfaces should work");
            if (Interface_TestClass_impl_10.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_impl_11_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Implementing an indexer that was defined identically by two different base-interfaces should work");
            if (Interface_TestClass_impl_11.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_impl_12_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("The members of base class participate in interface mapping");
            if (Interface_TestClass_impl_12.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_impl_13_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Two interfaces on the same object must resolve to the same reference");
            if (Interface_TestClass_impl_13.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_implinherit_01_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Interface methods implementation inheritance map as expected (1st/2nd example in 13.4.3)");
            if (Interface_TestClass_implinherit_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_implinherit_02_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Interface methods implementation inheritance (w/virtual) map as expected (3rd/4th example in 13.4.3)");
            if (Interface_TestClass_implinherit_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }


        [TestMethod]
        public MFTestResults Interface_explicit_04_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Explicit interface member implementation of a method");
            if (Interface_TestClass_explicit_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_explicit_05_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Explicit interface member implementation of a property");
            if (Interface_TestClass_explicit_05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_explicit_06_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Explicit interface member implementation of an event (Bug 89766)");
            if (Interface_TestClass_explicit_06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_explicit_07_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Explicit interface member implementation of an indexer");
            if (Interface_TestClass_explicit_07.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Interface_explicit_10_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Explicitly implemented members (method) can be called through interface instances");
            if (Interface_TestClass_explicit_10.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_explicit_11_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Explicitly implemented members (property) can be called through interface instances");
            if (Interface_TestClass_explicit_11.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_explicit_12_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Explicitly implemented members (indexer) can be called through interface instances");
            if (Interface_TestClass_explicit_12.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_explicit_13_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Explicitly implemented members (event) can be called through interface instances  (Bug 89766)");
            if (Interface_TestClass_explicit_13.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Interface_explicit_21_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Hidden base interface members may be implemented in all of the ways used in this");
            if (Interface_TestClass_explicit_21.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_explicit_25_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Explicitly implementing overloaded methods should work ");
            if (Interface_TestClass_explicit_25.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_mdfrmeth_09_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("An abstract class can implement an interface's methods as abstract");
            if (Interface_TestClass_mdfrmeth_09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_ifreimp_01_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("A class that inherits an interface implementation is permitted to re-implement the interface by including it in the base class list");
            if (Interface_TestClass_ifreimp_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            else
            {
                Log.Comment("This in a known bug captured in Issue #95");
                return MFTestResults.KnownFailure;
            }
        }
        [TestMethod]
        public MFTestResults Interface_ifreimp_02_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Inherited public member declarations and inherited explicit interface member declarations participate in the interface mapping process for re-implemented interfaces (Bug 90165)");
            if (Interface_TestClass_ifreimp_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            else
            {
                Log.Comment("This in a known bug captured in Issue #95");
                return MFTestResults.KnownFailure;
            }
        }
        [TestMethod]
        public MFTestResults Interface_ifreimp_03_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Interface reimplementation also reimplements the interface's base interfaces.  Verify mapping is done correctly. (90165)");
            Log.Comment("This Test is an expected fail in the baseline, but has passed in recent testing");
            if (Interface_TestClass_ifreimp_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_abstract_01_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("An abstract class is permitted to map base-interface methods onto abstract methods");
            if (Interface_TestClass_abstract_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_abstract_02_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("The abstract methods implementing interface methods must be overridden by inheriting classes");
            if (Interface_TestClass_abstract_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_abstract_03_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Explicit interface member implementations are permitted to call abstract methods");
            if (Interface_TestClass_abstract_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_indexer_02_Test()
        {
            Log.Comment("Section 13.2 Interface members");
            Log.Comment("Simplest interface indexer declaration (int)");
            if (Interface_TestClass_indexer_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_indexer_03_Test()
        {
            Log.Comment("Section 13.2 Interface members");
            Log.Comment("Simplest interface indexer declaration (String)");
            if (Interface_TestClass_indexer_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_indexer_04_Test()
        {
            Log.Comment("Section 13.2 Interface members");
            Log.Comment("Simplest interface indexer declaration (user-defined struct)");
            if (Interface_TestClass_indexer_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_indexer_05_Test()
        {
            Log.Comment("Section 13.2 Interface members");
            Log.Comment("Interface indexer with multiple parameters in formal-index-parameter-list");
            if (Interface_TestClass_indexer_05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_indexer_06_Test()
        {
            Log.Comment("Section 13.2 Interface members");
            Log.Comment("Interface indexer with just get accessor");
            if (Interface_TestClass_indexer_06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_indexer_07_Test()
        {
            Log.Comment("Section 13.2 Interface members");
            Log.Comment("Interface indexer with just set accessor");
            if (Interface_TestClass_indexer_07.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_indexer_18_Test()
        {
            Log.Comment("Section 13.2 Interface members");
            Log.Comment("Using 'new' on an interface member (indexer) that doesn't hide an inherited member should gen warning ");
            Log.Comment("(Related Bug: 86609)");
            if (Interface_TestClass_indexer_18.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_meth_09_Test()
        {
            Log.Comment("Section 13.2 Interface members");
            Log.Comment("Using 'new' on an interface member (method) that doesn't hide an inherited member should gen warning (Bug: 86609)");
            if (Interface_TestClass_meth_09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_prop_09_Test()
        {
            Log.Comment("Section 13.2 Interface members");
            Log.Comment("Using 'new' on an interface member (property) that doesn't hide an inherited member should gen warning (Bug: 86609)");
            if (Interface_TestClass_prop_09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_namesig_02_Test()
        {
            Log.Comment("Section 13.2 Interface members");
            Log.Comment("Member functions of an interface may have same name if they have diff args");
            if (Interface_TestClass_namesig_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_namesig_03_Test()
        {
            Log.Comment("Section 13.2 Interface members");
            Log.Comment("Interface member that is hidden by an inheriting interface (without using new) gens warning");
            if (Interface_TestClass_namesig_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_namesig_04_Test()
        {
            Log.Comment("Section 13.2 Interface members");
            Log.Comment("Interface that inherits 2 other interfaces that have a member with same sig");
            if (Interface_TestClass_namesig_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_namesig_05_Test()
        {
            Log.Comment("Section 13.2 Interface members");
            Log.Comment("Interface member that is hidden by an inheriting interface (using new) works correctly");
            if (Interface_TestClass_namesig_05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_events_08_Test()
        {
            Log.Comment("Section 13.2 Interface members");
            Log.Comment("Using 'new' on an interface member (event) that doesn't hide an inherited member should gen warning (Bug: 118831)");
            if (Interface_TestClass_events_08.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_events_09_Test()
        {
            Log.Comment("Section 13.2 Interface members");
            Log.Comment("Simple Event declaration within an interface");
            if (Interface_TestClass_events_09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_maccess_01_Test()
        {
            Log.Comment("Section 13.2 Interface members");
            Log.Comment("Inherited members with same sig, from diff interfaces, can be accessed through casting");
            if (Interface_TestClass_maccess_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_maccess_03_Test()
        {
            Log.Comment("Section 13.2 Interface members");
            Log.Comment("Inherited members with same method names but diff args can be accessed by casting");
            if (Interface_TestClass_maccess_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_maccess_04_Test()
        {
            Log.Comment("Section 13.2 Interface members");
            Log.Comment("Inherited members with same method names but diff args should work if arg passed is not ambiguous (Regression bug: 90867)");
            if (Interface_TestClass_maccess_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Interface_modifier_02_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Verify private interfaces gen error when declared in global namespace");
            if (Interface_TestClass_modifier_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_modifier_03_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Verify protected interfaces gen error when declared in global namespace");
            if (Interface_TestClass_modifier_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        //Test Case Calls 
        [TestMethod]
        public MFTestResults Interface_struct_impl_01_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Simple struct implementation of two interfaces");
            if (Interface_TestClass_struct_impl_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_impl_02_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("A struct that implements an interface also implicitly implements all of the interface's base interfaces");
            if (Interface_TestClass_struct_impl_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_impl_04_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("A member of an interface-type can be assigned an instance of a struct that implements it");
            if (Interface_TestClass_struct_impl_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_impl_05_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("A struct that implements an interface with a method, property, event, and indexer");
            if (Interface_TestClass_struct_impl_05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_impl_06_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Implementing a method that was defined identically by two different base-interfaces should work");
            if (Interface_TestClass_struct_impl_06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_impl_07_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Implementing a property that was defined identically by two different base-interfaces should work");
            if (Interface_TestClass_struct_impl_07.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_impl_08_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Implementing an event that was defined identically by two different base-interfaces should work");
            if (Interface_TestClass_struct_impl_08.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_impl_09_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Implementing an indexer that was defined identically by two different base-interfaces should work");
            if (Interface_TestClass_struct_impl_09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_explicit_01_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Explicit interface member implementations");
            if (Interface_TestClass_struct_explicit_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_explicit_03_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Implementing an iface method in a struct that doesn't list the iface as a base iface but does list a iface that inherits the iface should work");
            if (Interface_TestClass_struct_explicit_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_explicit_04_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Explicit interface member implementation of a method");
            if (Interface_TestClass_struct_explicit_04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_explicit_05_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Explicit interface member implementation of a property");
            if (Interface_TestClass_struct_explicit_05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_explicit_06_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Explicit interface member implementation of an event (Bug: 89766)");
            if (Interface_TestClass_struct_explicit_06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_explicit_07_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Explicit interface member implementation of an indexer");
            if (Interface_TestClass_struct_explicit_07.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_explicit_09_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Multiple base interfaces and explicit interface member implementations work");
            if (Interface_TestClass_struct_explicit_09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_explicit_10_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Explicitly implemented members (method) can be called through interface instances");
            if (Interface_TestClass_struct_explicit_10.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_explicit_11_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Explicitly implemented members (property) can be called through interface instances (Bug 90570)");
            if (Interface_TestClass_struct_explicit_11.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_explicit_12_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Explicitly implemented members (indexer) can be called through interface instances (Bug 90570)");
            if (Interface_TestClass_struct_explicit_12.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_explicit_13_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Explicitly implemented members (event) can be called through interface instances (89766)");
            if (Interface_TestClass_struct_explicit_13.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_explicit_19_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Explicitly implementing member from interface not in base-list (but in base-list of a base interface) should work");
            if (Interface_TestClass_struct_explicit_19.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_explicit_20_Test()
        {
            Log.Comment("Section 13.4 Interface implementations");
            Log.Comment("Hidden base interface members may be implemented in all of the ways used in this");
            if (Interface_TestClass_struct_explicit_20.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_decl_01_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Simplest interface declared in a struct in default global namespace");
            if (Interface_TestClass_struct_decl_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_decl_02_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Simplest interface declared in a struct in a declared namespace");
            if (Interface_TestClass_decl_02_NS.Interface_TestClass_decl_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_inherit_01_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Simplest form of interface inheritance, inheriting multiple interfaces");
            if (Interface_TestClass_struct_inherit_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_modifier_01_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Verify public and internal interfaces are valid inside structs");
            if (Interface_TestClass_struct_modifier_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_modifier_02_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Verify private interfaces are valid inside structs");
            if (Interface_TestClass_struct_modifier_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_modifier_03_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Verify new interface gens warning if not hiding anything");
            if (Interface_TestClass_struct_modifier_03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_semicolon_01_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Simplest interface declared (with semi-colon) in a struct in default global namespace");
            if (Interface_TestClass_struct_semicolon_01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Interface_struct_semicolon_02_Test()
        {
            Log.Comment("Section 13.1 Interface declarations ");
            Log.Comment("Simplest interface declared (with semi-colon) in a struct in a declared namespace");
            if (Interface_TestClass_semicolon_02_NS.Interface_TestClass_semicolon_02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }



        //Compiled Test Cases 
        class Interface_TestClass_base_06
        {
            internal interface Interface_TestClass_base_06_I { }

            public interface Interface_TestClass_base_06_2 : Interface_TestClass_base_06_I { }

            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_decl_01_I
        {
        };
        class Interface_TestClass_decl_01
        {
            public static void Main_old()
            {
                Log.Comment("This works!");
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }

        class Interface_TestClass_decl_03
        {
            interface Interface_TestClass_decl_03_I
            {
            }
            public static void Main_old()
            {
                Log.Comment("This works!");
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }

        interface Interface_TestClass_decl_05_1
        {
            // methods returning various different types
            void meth1();
            int meth2();
            double meth3();
            bool meth4();
            char meth5();
            String meth6();
            float meth7();
            // methods accepting various different types as arguments
            void arg1(int x);
            void arg2(double x);
            void arg3(bool x);
            void arg4(char x);
            void arg5(String x);
            void arg6(float x);
            // properties of various different types
            int typ1 { get; set; }
            double typ2 { get; set; }
            bool typ3 { get; set; }
            char typ4 { get; set; }
            String typ5 { get; set; }
            float typ6 { get; set; }
        }
        class Interface_TestClass_decl_05
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_decl_06_1
        {
            // properties of various different types
            int typ1 { get; }
            double typ2 { get; }
            bool typ3 { get; }
            char typ4 { get; }
            String typ5 { get; }
            float typ6 { get; }
        }
        class Interface_TestClass_decl_06
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_decl_07_1
        {
            // properties of various different types
            int typ1 { set; }
            double typ2 { set; }
            bool typ3 { set; }
            char typ4 { set; }
            String typ5 { set; }
            float typ6 { set; }
        }
        class Interface_TestClass_decl_07
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        class Interface_TestClass_inherit_01_1
        {
            public interface m { }
        }
        class Interface_TestClass_inherit_01 : Interface_TestClass_inherit_01_1
        {
            new void m() { }

            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_inherit_02_I { }
        interface Interface_TestClass_inherit_02_2 : Interface_TestClass_inherit_02_I { }
        class Interface_TestClass_inherit_02
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_inherit_03_A { }
        interface Interface_TestClass_inherit_03_B { }
        interface Interface_TestClass_inherit_03_C { }
        interface Interface_TestClass_inherit_03_D : Interface_TestClass_inherit_03_A, Interface_TestClass_inherit_03_B, Interface_TestClass_inherit_03_C
        { }
        class Interface_TestClass_inherit_03
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_inherit_04_A
        {
            void m();
        }
        interface Interface_TestClass_inherit_04_B : Interface_TestClass_inherit_04_A { }
        interface Interface_TestClass_inherit_04_C : Interface_TestClass_inherit_04_B { }
        class Interface_TestClass_inherit_04 : Interface_TestClass_inherit_04_C
        {
            public void m()
            {
                Log.Comment("This works!");
            }

            public static void Main_old()
            {
                Interface_TestClass_inherit_04 Interface_TestClass_inherit_04_1 = new Interface_TestClass_inherit_04();
                Interface_TestClass_inherit_04_1.m();
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        class Interface_TestClass_inherit_08_1
        {
            public interface m { }
        }
        class Interface_TestClass_inherit_08 : Interface_TestClass_inherit_08_1
        {
            void m() { }

            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        public interface Interface_TestClass_modifier_01_I { }
        internal interface Interface_TestClass_modifier_01_I2 { }
        class Interface_TestClass_modifier_01
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        class Interface_TestClass_modifier_05
        {
            public interface Interface_TestClass_modifier_05_I { }
            internal interface Interface_TestClass_modifier_05_I2 { }
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        class Interface_TestClass_modifier_06
        {
            private interface Interface_TestClass_modifier_06_I { }
            protected interface Interface_TestClass_modifier_06_I2 { }
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        class Interface_TestClass_modifier_07
        {
            new interface Interface_TestClass_modifier_07_I { }

            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        class Interface_TestClass_modifier_08_1
        {
            public void m() { }
        }
        class Interface_TestClass_modifier_08 : Interface_TestClass_modifier_08_1
        {
            new interface m { }

            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        class Interface_TestClass_modifier_10
        {
            protected internal interface Interface_TestClass_modifier_10_I { }
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_semicolon_01_I
        {
        };
        class Interface_TestClass_semicolon_01
        {
            public static void Main_old()
            {
                Log.Comment("This works!");
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }

        class Interface_TestClass_semicolon_03
        {
            interface Interface_TestClass_semicolon_03_I
            {
            };
            public static void Main_old()
            {
                Log.Comment("This works!");
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        /*
        public class Interface_TestClass_implbyinherit_01b_1 : Interface_TestClass_implbyinherit_01b_2, Interface_TestClass_implbyinherit_01b_I1
        {
            public static void Main_old()
            {
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }*/
        interface Interface_TestClass_impl_01_I1
        {
            void F();
        }
        interface Interface_TestClass_impl_01_I2
        {
            void G();
        }
        class Interface_TestClass_impl_01_1 : Interface_TestClass_impl_01_I1, Interface_TestClass_impl_01_I2
        {
            public void F() { }
            public void G() { }
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_impl_02_I2
        {
            void Interface_TestClass_impl_02_B();
        }
        interface Interface_TestClass_impl_02_I3 : Interface_TestClass_impl_02_I2
        {
            void Interface_TestClass_impl_02_D();
        }
        class Interface_TestClass_impl_02_1 : Interface_TestClass_impl_02_I3
        {
            public void Interface_TestClass_impl_02_B() { }
            public void Interface_TestClass_impl_02_D() { }
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_impl_04_I1
        {
            void F();
        }
        class Interface_TestClass_impl_04_C : Interface_TestClass_impl_04_I1
        {
            public void F()
            {
                Log.Comment("Interface_TestClass_impl_04_C.F()");
            }
        }
        class Interface_TestClass_impl_04
        {
            public static void Main_old()
            {
                Interface_TestClass_impl_04_C cc = new Interface_TestClass_impl_04_C();
                Interface_TestClass_impl_04_I1 ic = cc;
                cc.F();
                ic.F();
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_impl_05_I1
        {
            void F();
        }
        class Interface_TestClass_impl_05_C : Interface_TestClass_impl_05_I1
        {
            public void F()
            {
                Log.Comment("Interface_TestClass_impl_05_C.F()");
            }
        }
        class Interface_TestClass_impl_05_D : Interface_TestClass_impl_05_C
        {
            public void F()
            {
                Log.Comment("Interface_TestClass_impl_05_D.F()");
            }
        }
        class Interface_TestClass_impl_05
        {
            public static void Main_old()
            {
                Interface_TestClass_impl_05_C cc = new Interface_TestClass_impl_05_C();
                Interface_TestClass_impl_05_D cd = new Interface_TestClass_impl_05_D();
                Interface_TestClass_impl_05_I1 ic = cc;
                Interface_TestClass_impl_05_I1 id = cd;
                cc.F();
                ic.F();
                id.F();
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_impl_06_I1
        {
            void F();
        }
        class Interface_TestClass_impl_06_C : Interface_TestClass_impl_06_I1
        {
            public virtual void F()
            {
                Log.Comment("Interface_TestClass_impl_06_C.F()");
            }
        }
        class Interface_TestClass_impl_06_D : Interface_TestClass_impl_06_C
        {
            public override void F()
            {
                Log.Comment("Interface_TestClass_impl_06_D.F()");
            }
        }
        class Interface_TestClass_impl_06
        {
            public static void Main_old()
            {
                Interface_TestClass_impl_06_C cc = new Interface_TestClass_impl_06_C();
                Interface_TestClass_impl_06_D cd = new Interface_TestClass_impl_06_D();
                Interface_TestClass_impl_06_I1 ic = cc;
                Interface_TestClass_impl_06_I1 id = cd;
                cc.F();
                ic.F();
                id.F();
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        public delegate void Interface_TestClass_impl_07_ev(object sender);
        interface Interface_TestClass_impl_07_I
        {
            void Method();
            int Property { get; set; }
            event Interface_TestClass_impl_07_ev Clicked;
            string this[int index] { get; set; }
        }
        class Interface_TestClass_impl_07_1 : Interface_TestClass_impl_07_I
        {
            private int num;

            public void Method()
            {
                Log.Comment("Method()");
            }
            public int Property
            {
                get
                {
                    return num;
                }
                set
                {
                    if (value >= 0 && value <= 10)
                        num = value;
                }
            }
            public event Interface_TestClass_impl_07_ev Clicked;
            public string this[int index]
            {
                get { return "Interface_TestClass_impl_07"; }
                set { }
            }
        }
        class Interface_TestClass_impl_07
        {
            public static void Main_old()
            {
                Interface_TestClass_impl_07 t = new Interface_TestClass_impl_07();
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_impl_08_I1
        {
            void Paint();
        }
        interface Interface_TestClass_impl_08_I2
        {
            void Paint();
        }
        class Interface_TestClass_impl_08 : Interface_TestClass_impl_08_I1, Interface_TestClass_impl_08_I2
        {
            public void Paint() { }

            public static void Main_old()
            {
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_impl_09_I1
        {
            int MyProp { get; set; }
        }
        interface Interface_TestClass_impl_09_I2
        {
            int MyProp { get; set; }
        }
        class Interface_TestClass_impl_09 : Interface_TestClass_impl_09_I1, Interface_TestClass_impl_09_I2
        {
            public int MyProp
            {
                get
                {
                    return 0;
                }
                set
                {
                }
            }

            public static void Main_old()
            {
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        public delegate void MyDelegate(object sender);
        interface Interface_TestClass_impl_10_I1
        {
            event MyDelegate Interface_TestClass_impl_10_ev;
        }
        interface Interface_TestClass_impl_10_I2
        {
            event MyDelegate Interface_TestClass_impl_10_ev;
        }
        class Interface_TestClass_impl_10 : Interface_TestClass_impl_10_I1, Interface_TestClass_impl_10_I2
        {
            public event MyDelegate Interface_TestClass_impl_10_ev;

            public static void Main_old()
            {
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_impl_11_I1
        {
            string this[int index] { get; set; }
        }
        interface Interface_TestClass_impl_11_I2
        {
            string this[int index] { get; set; }
        }
        class Interface_TestClass_impl_11 : Interface_TestClass_impl_11_I1, Interface_TestClass_impl_11_I2
        {
            public string this[int index]
            {
                get { return "Interface_TestClass_impl_11_1"; }
                set { }
            }

            public static void Main_old()
            {
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_impl_12_I1
        {
            void F();
        }
        class Interface_TestClass_impl_12_1
        {
            public void F() { }
            public void G() { }
        }
        class MyOtherClass : Interface_TestClass_impl_12_1, Interface_TestClass_impl_12_I1
        {
            public new void G() { }
        }
        class Interface_TestClass_impl_12
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_impl_13_I1
        {
            int foo();
        }
        interface Interface_TestClass_impl_13_I2
        {
            int foo();
        }
        class Interface_TestClass_impl_13_2 : Interface_TestClass_impl_13_I1, Interface_TestClass_impl_13_I2
        {
            public int foo() { return 0; }
        }
        class Interface_TestClass_impl_13
        {
            public static int Main_old()
            {
                Interface_TestClass_impl_13_2 x = new Interface_TestClass_impl_13_2();

                Interface_TestClass_impl_13_I1 Interface_TestClass_impl_13_2 = x;
                Interface_TestClass_impl_13_I2 b = x;

                if (Interface_TestClass_impl_13_2 == b)
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
        interface Interface_TestClass_implinherit_01_I1
        {
            void Paint();
        }
        class Interface_TestClass_implinherit_01_1 : Interface_TestClass_implinherit_01_I1
        {
            public void Paint()
            {
                Log.Comment("Interface_TestClass_implinherit_01_1.Paint()");
            }
        }
        class Interface_TestClass_implinherit_01_2 : Interface_TestClass_implinherit_01_1
        {
            new public void Paint()
            {
                Log.Comment("Interface_TestClass_implinherit_01_2.Paint()");
            }
        }
        class Interface_TestClass_implinherit_01
        {
            public static void Main_old()
            {
                Interface_TestClass_implinherit_01_1 c = new Interface_TestClass_implinherit_01_1();
                Interface_TestClass_implinherit_01_2 t = new Interface_TestClass_implinherit_01_2();
                Interface_TestClass_implinherit_01_I1 ic = c;
                Interface_TestClass_implinherit_01_I1 it = t;
                c.Paint();
                t.Paint();
                ic.Paint();
                it.Paint();
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_implinherit_02_I1
        {
            void Paint();
        }
        class Interface_TestClass_implinherit_02_1 : Interface_TestClass_implinherit_02_I1
        {
            public virtual void Paint()
            {
                Log.Comment("Interface_TestClass_implinherit_02_1.Paint()");
            }
        }
        class Interface_TestClass_implinherit_02_2 : Interface_TestClass_implinherit_02_1
        {
            public override void Paint()
            {
                Log.Comment("Interface_TestClass_implinherit_02_2.Paint()");
            }
        }
        class Interface_TestClass_implinherit_02
        {
            public static void Main_old()
            {
                Interface_TestClass_implinherit_02_1 c = new Interface_TestClass_implinherit_02_1();
                Interface_TestClass_implinherit_02_2 t = new Interface_TestClass_implinherit_02_2();
                Interface_TestClass_implinherit_02_I1 ic = c;
                Interface_TestClass_implinherit_02_I1 it = t;
                c.Paint();
                t.Paint();
                ic.Paint();
                it.Paint();
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_explicit_01_I1
        {
            void F();
        }
        interface Interface_TestClass_explicit_01_I2
        {
            void G();
        }
        class Interface_TestClass_explicit_01_C : Interface_TestClass_explicit_01_I1, Interface_TestClass_explicit_01_I2
        {
            void Interface_TestClass_explicit_01_I1.F() { }
            void Interface_TestClass_explicit_01_I2.G() { }
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_explicit_03_I1
        {
            void F();
        }
        interface Interface_TestClass_explicit_03_I2 : Interface_TestClass_explicit_03_I1
        {
            void G();
        }
        class Interface_TestClass_explicit_03_C : Interface_TestClass_explicit_03_I2
        {
            void Interface_TestClass_explicit_03_I1.F() { }
            void Interface_TestClass_explicit_03_I2.G() { }
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_explicit_04_1
        {
            void Method();
        }
        class Interface_TestClass_explicit_04_2 : Interface_TestClass_explicit_04_1
        {
            void Interface_TestClass_explicit_04_1.Method()
            {
                Log.Comment("Method()");
            }
        }
        class Interface_TestClass_explicit_04
        {
            public static void Main_old()
            {
                Interface_TestClass_explicit_04_2 t = new Interface_TestClass_explicit_04_2();
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_explicit_05_1
        {
            int Property { get; set; }
        }
        class Interface_TestClass_explicit_05_2 : Interface_TestClass_explicit_05_1
        {
            private int num;

            int Interface_TestClass_explicit_05_1.Property
            {
                get
                {
                    return num;
                }
                set
                {
                    if (value >= 0 && value <= 10)
                        num = value;
                }
            }
        }
        class Interface_TestClass_explicit_05
        {
            public static void Main_old()
            {
                Interface_TestClass_explicit_05_2 t = new Interface_TestClass_explicit_05_2();
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        public delegate void Interface_TestClass_explicit_06_ev(object sender);
        interface Interface_TestClass_explicit_06_1
        {
            event Interface_TestClass_explicit_06_ev Clicked;
        }
        class Interface_TestClass_explicit_06_2 : Interface_TestClass_explicit_06_1
        {
            event Interface_TestClass_explicit_06_ev Interface_TestClass_explicit_06_1.Clicked
            {
                add { }
                remove { }
            }
            public void MyHandler(object sender)
            {

            }
        }
        class Interface_TestClass_explicit_06
        {
            public static void Main_old()
            {
                Interface_TestClass_explicit_06_2 t = new Interface_TestClass_explicit_06_2();
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_explicit_07_1
        {
            string this[int index] { get; set; }
        }
        class Interface_TestClass_explicit_07_2 : Interface_TestClass_explicit_07_1
        {
            string Interface_TestClass_explicit_07_1.this[int index]
            {
                get { return "Interface_TestClass_explicit_07"; }
                set { }
            }
        }
        class Interface_TestClass_explicit_07
        {
            public static void Main_old()
            {
                Interface_TestClass_explicit_07_2 t = new Interface_TestClass_explicit_07_2();
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface ICloneable
        {
            object Clone();
        }
        interface Interface_TestClass_explicit_09_I2
        {
            int CompareTo(object other);
        }
        class Interface_TestClass_explicit_09_1 : ICloneable, Interface_TestClass_explicit_09_I2
        {
            object ICloneable.Clone()
            {
                return new object();
            }

            int Interface_TestClass_explicit_09_I2.CompareTo(object other)
            {
                return 0;
            }
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_explicit_10_C
        {
            object Clone();
        }
        interface Interface_TestClass_explicit_10_I2
        {
            int CompareTo(object other);
        }
        class Interface_TestClass_explicit_10_1 : Interface_TestClass_explicit_10_C, Interface_TestClass_explicit_10_I2
        {
            object Interface_TestClass_explicit_10_C.Clone()
            {
                return new object();
            }

            int Interface_TestClass_explicit_10_I2.CompareTo(object other)
            {
                return 0;
            }
        }
        class Interface_TestClass_explicit_10
        {
            public static void Main_old()
            {
                    Interface_TestClass_explicit_10_1 le = new Interface_TestClass_explicit_10_1();
                    object o = ((Interface_TestClass_explicit_10_C)le).Clone();

            }
            public static bool testMethod()
            {
                try
                {
                    Main_old();
                }
                catch (System.Exception)
                {
                    
                    return false;
                }
                return true;
            }
        }
        interface ICloneable_2
        {
            int Value { get; set; }
        }
        interface Interface_TestClass_explicit_11_I2
        {
            int CompareTo(object other);
        }
        class Interface_TestClass_explicit_11_1 : ICloneable_2, Interface_TestClass_explicit_11_I2
        {
            private int val;

            int ICloneable_2.Value
            {
                get { return val; }
                set { val = value; }
            }

            int Interface_TestClass_explicit_11_I2.CompareTo(object other)
            {
                return 0;
            }
        }
        class Interface_TestClass_explicit_11
        {
            public static int Main_old()
            {
                try
                {
                    Interface_TestClass_explicit_11_1 le = new Interface_TestClass_explicit_11_1();
                    ((ICloneable_2)le).Value = 5;
                    if (((ICloneable_2)le).Value == 5)
                        return 0;
                    else
                        return 1;
                }
                catch (System.Exception)
                {

                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        interface ICloneable_3
        {
            string this[string hashkey] { get; set; }
        }
        interface Interface_TestClass_explicit_12_I2
        {
            int CompareTo(object other);
        }
        class Interface_TestClass_explicit_12_1 : ICloneable_3, Interface_TestClass_explicit_12_I2
        {
            public string st = "";
            string ICloneable_3.this[string hashkey]
            {
                get
                {
                    return "success";
                }
                set
                {
                    st = value;
                }
            }

            int Interface_TestClass_explicit_12_I2.CompareTo(object other)
            {
                return 0;
            }
        }
        class Interface_TestClass_explicit_12
        {
            public static int Main_old()
            {
                try
                {
                    Interface_TestClass_explicit_12_1 le = new Interface_TestClass_explicit_12_1();
                    ((ICloneable_3)le)["Interface_TestClass_explicit_12_1"] = "correct";
                    string s = ((ICloneable_3)le)["Interface_TestClass_explicit_12_1"];
                    if (le.st == "correct" && s == "success")
                        return 0;
                    else
                        return 1;
                }
                catch (System.Exception)
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public delegate void Interface_TestClass_explicit_13_ev(object sender);
        interface Interface_TestClass_explicit_13_1
        {
            event Interface_TestClass_explicit_13_ev Clicked;
        }
        class Interface_TestClass_explicit_13_2 : Interface_TestClass_explicit_13_1
        {
            event Interface_TestClass_explicit_13_ev Interface_TestClass_explicit_13_1.Clicked
            {
                add { }
                remove { }
            }
            public void MyHandler(object sender)
            {

            }
        }
        class Interface_TestClass_explicit_13
        {
            public static void Main_old()
            {
                new Interface_TestClass_explicit_13().Run();
            }
            public void Run()
            {
                Interface_TestClass_explicit_13_2 t = new Interface_TestClass_explicit_13_2();
                ((Interface_TestClass_explicit_13_1)t).Clicked += new Interface_TestClass_explicit_13_ev(TheHandler);
            }
            public void TheHandler(object sender)
            {
            }
            public static bool testMethod()
            {
                try
                {
                    Main_old();
                }
                catch (System.Exception)
                {
                    return false;

                }
                return true;
            }
        }

        interface Interface_TestClass_explicit_20_I3 : ICloneable
        {
            string OtherMethod();
        }
        class Interface_TestClass_explicit_20_1 : Interface_TestClass_explicit_20_I3
        {
            string Interface_TestClass_explicit_20_I3.OtherMethod()
            {
                return "Interface_TestClass_explicit_20_1";
            }

            object ICloneable.Clone()
            {
                return new object();
            }

            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_explicit_21_I2
        {
            int P { get; }
        }
        interface Interface_TestClass_explicit_21_I3 : Interface_TestClass_explicit_21_I2
        {
            new int P();
        }
        class Interface_TestClass_explicit_21_1 : Interface_TestClass_explicit_21_I3
        {
            int Interface_TestClass_explicit_21_I2.P { get { return 1; } }
            int Interface_TestClass_explicit_21_I3.P() { return 1; }
        }
        class Interface_TestClass_explicit_21_2 : Interface_TestClass_explicit_21_I3
        {
            public int P { get { return 1; } }
            int Interface_TestClass_explicit_21_I3.P() { return 1; }
        }
        class Interface_TestClass_explicit_21_3 : Interface_TestClass_explicit_21_I3
        {
            int Interface_TestClass_explicit_21_I2.P { get { return 1; } }
            public int P() { return 1; }
        }
        class Interface_TestClass_explicit_21
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        class TypeA { }
        class TypeB { }
        interface Interface_TestClass_explicit_25_I3
        {
            int foo(TypeA Interface_TestClass_explicit_25_2);
            int foo(TypeB b);
        }
        class Interface_TestClass_explicit_25_A : Interface_TestClass_explicit_25_I3
        {
            int Interface_TestClass_explicit_25_I3.foo(TypeA Interface_TestClass_explicit_25_2)
            {
                return 1;
            }

            int Interface_TestClass_explicit_25_I3.foo(TypeB b)
            {
                return 2;
            }
        }
        class Interface_TestClass_explicit_25
        {
            static int Main_old()
            {
                try
                {
                    TypeA Interface_TestClass_explicit_25_2 = new TypeA();
                    TypeB b = new TypeB();
                    Interface_TestClass_explicit_25_A Interface_TestClass_explicit_25_1 = new Interface_TestClass_explicit_25_A();
                    if (((Interface_TestClass_explicit_25_I3)Interface_TestClass_explicit_25_1).foo(Interface_TestClass_explicit_25_2) != 1)
                        return 1;
                    if (((Interface_TestClass_explicit_25_I3)Interface_TestClass_explicit_25_1).foo(b) != 2)
                        return 1;
                    return 0;
                }
                catch (System.Exception)
                {
                    return 1;

                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        interface Interface_TestClass_mdfrmeth_09_I1
        {
            void F();
            void G();
        }
        abstract class Interface_TestClass_mdfrmeth_09_C : Interface_TestClass_mdfrmeth_09_I1
        {
            public abstract void F();
            public abstract void G();
        }
        class Interface_TestClass_mdfrmeth_09
        {
            public static void Main_old()
            {

            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_ifreimp_01_I1
        {
            void Paint();
        }
        class Interface_TestClass_ifreimp_01_1 : Interface_TestClass_ifreimp_01_I1
        {
            void Interface_TestClass_ifreimp_01_I1.Paint()
            {
                Log.Comment("Interface_TestClass_ifreimp_01_1.Interface_TestClass_ifreimp_01_I1.Paint()");
                throw new Exception("Incorrect method got called!");
            }
        }
        class Interface_TestClass_ifreimp_01_3 : Interface_TestClass_ifreimp_01_1, Interface_TestClass_ifreimp_01_I1
        {
            public void Paint()
            {
                Log.Comment("Interface_TestClass_ifreimp_01_3.Paint()");
            }
        }
        class Interface_TestClass_ifreimp_01
        {
            public static bool testMethod()
            {
                Interface_TestClass_ifreimp_01_I1 instance = new Interface_TestClass_ifreimp_01_3();
                try
                {
                    instance.Paint();
                }
                catch (Exception)
                {
                    return false;
                }

                return true;
            }
        }
        interface Interface_TestClass_ifreimp_02_I1
        {
            void F();
            void G();
            void H();
            void I();
        }
        class Interface_TestClass_ifreimp_02_2 : Interface_TestClass_ifreimp_02_I1
        {
            void Interface_TestClass_ifreimp_02_I1.F()
            {
                Log.Comment("Interface_TestClass_ifreimp_02_2.Interface_TestClass_ifreimp_02_I1.F()");
                throw new Exception("Incorrect method got called!");
            }

            void Interface_TestClass_ifreimp_02_I1.G()
            {
                Log.Comment("Interface_TestClass_ifreimp_02_2.Interface_TestClass_ifreimp_02_I1.G()");
            }

            public void H()
            {
                Log.Comment("Interface_TestClass_ifreimp_02_2.H()");
                throw new Exception("Incorrect method got called!");
            }

            public void I()
            {
                Log.Comment("Interface_TestClass_ifreimp_02_2.I()");
            }
        }
        class Interface_TestClass_ifreimp_02_1 : Interface_TestClass_ifreimp_02_2, Interface_TestClass_ifreimp_02_I1
        {
            public void F()
            {
                Log.Comment("Interface_TestClass_ifreimp_02_1.F()");
            }

            void Interface_TestClass_ifreimp_02_I1.H()
            {
                Log.Comment("Interface_TestClass_ifreimp_02_1.Interface_TestClass_ifreimp_02_I1.H()");
            }
        }
        class Interface_TestClass_ifreimp_02
        {
            public static bool testMethod()
            {
                try
                {
                    Interface_TestClass_ifreimp_02_I1 im = new Interface_TestClass_ifreimp_02_1(); ;
                    im.F();
                    im.G();
                    im.H();
                    im.I();
                }
                catch (Exception)
                {
                    return false;
                }

                return true;
            }
        }
        interface Interface_TestClass_ifreimp_03_I2
        {
            void F();
        }
        interface Interface_TestClass_ifreimp_03_I3 : Interface_TestClass_ifreimp_03_I2
        {
            void G();
        }
        class Interface_TestClass_ifreimp_03_C : Interface_TestClass_ifreimp_03_I3
        {
            void Interface_TestClass_ifreimp_03_I2.F()
            {
                Log.Comment("Interface_TestClass_ifreimp_03_C.Interface_TestClass_ifreimp_03_I2.F()");
            }
            void Interface_TestClass_ifreimp_03_I3.G()
            {
                Log.Comment("Interface_TestClass_ifreimp_03_C.Interface_TestClass_ifreimp_03_I3.G()");
            }
        }
        class Interface_TestClass_ifreimp_03_D : Interface_TestClass_ifreimp_03_C, Interface_TestClass_ifreimp_03_I3
        {
            public void F()
            {
                Log.Comment("Interface_TestClass_ifreimp_03_D.F()");
            }
            public void G()
            {
                Log.Comment("Interface_TestClass_ifreimp_03_D.G()");
            }
        }
        class Interface_TestClass_ifreimp_03
        {
            public static void Main_old()
            {
                Interface_TestClass_ifreimp_03_D d = new Interface_TestClass_ifreimp_03_D();
                Interface_TestClass_ifreimp_03_I2 ib = d;
                ib.F();
            }
            public static bool testMethod()
            {
                try
                {
                    Main_old();
                }
                catch (System.Exception)
                { 
                    return false;
                }
                return true;
            }
        }
        interface Interface_TestClass_abstract_01_I1
        {
            void F();
            void G();
        }
        abstract class Interface_TestClass_abstract_01_C : Interface_TestClass_abstract_01_I1
        {
            public abstract void F();
            public abstract void G();
        }
        class Interface_TestClass_abstract_01
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_abstract_02_I1
        {
            void F();
            void G();
        }
        abstract class Interface_TestClass_abstract_02_C : Interface_TestClass_abstract_02_I1
        {
            public abstract void F();
            public abstract void G();
        }
        class Interface_TestClass_abstract_02_D : Interface_TestClass_abstract_02_C
        {
            public override void F() { }
            public override void G() { }
        }
        class Interface_TestClass_abstract_02
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_abstract_03_I1
        {
            void F();
            void G();
        }
        abstract class Interface_TestClass_abstract_03_C : Interface_TestClass_abstract_03_I1
        {
            public void F() { FF(); }
            public void G() { GG(); }
            protected abstract void FF();
            protected abstract void GG();
        }
        class Interface_TestClass_abstract_03
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_indexer_02_1
        {
            int this[int ident1] { get; set; }
        }
        class Interface_TestClass_indexer_02
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_indexer_03_1
        {
            String this[String ident1] { get; set; }
        }
        class Interface_TestClass_indexer_03
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        struct Interface_TestClass_indexer_04_str
        {
        }
        interface Interface_TestClass_indexer_04_1
        {
            Interface_TestClass_indexer_04_str this[Interface_TestClass_indexer_04_str ident1] { get; set; }
        }
        class Interface_TestClass_indexer_04
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        struct Interface_TestClass_indexer_05_str
        {
        }
        interface Interface_TestClass_indexer_05_1
        {
            Interface_TestClass_indexer_05_str this[Interface_TestClass_indexer_05_str ident1, int x, double y] { get; set; }
        }
        class Interface_TestClass_indexer_05
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        struct Interface_TestClass_indexer_06_str
        {
        }
        interface Interface_TestClass_indexer_06_1
        {
            Interface_TestClass_indexer_06_str this[Interface_TestClass_indexer_06_str ident1, int x, double y] { get; }
        }
        class Interface_TestClass_indexer_06
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        struct Interface_TestClass_indexer_07_str
        {
        }
        interface Interface_TestClass_indexer_07_1
        {
            Interface_TestClass_indexer_07_str this[Interface_TestClass_indexer_07_str ident1, int x, double y] { set; }
        }
        class Interface_TestClass_indexer_07
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        public delegate void Interface_TestClass_indexer_18_ev(object sender);
        interface Interface_TestClass_indexer_18_2
        {
            new int this[int ident1] { get; set; }
        }
        class Interface_TestClass_indexer_18
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_meth_09_1
        {
            new void F(int x);
        }
        class Interface_TestClass_meth_09
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_prop_09_2
        {
            new int x
            {
                get;
                set;
            }
        }
        class Interface_TestClass_prop_09
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }

        interface Interface_TestClass_namesig_02_1
        {
            void F(int x);
            void F(String x);
        }
        class Interface_TestClass_namesig_02_c : Interface_TestClass_namesig_02_1
        {
            public void F(int x)
            {
                Log.Comment("F(int x) works!");
            }
            public void F(String x)
            {
                Log.Comment("F(String x) works!");
            }
        }
        class Interface_TestClass_namesig_02
        {
            public static void Main_old()
            {
                Interface_TestClass_namesig_02_c tc = new Interface_TestClass_namesig_02_c();
                tc.F(23);
                tc.F("Interface_TestClass_namesig_02");
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_namesig_03_1
        {
            void F(int x);
        }
        interface Interface_TestClass_namesig_03_2 : Interface_TestClass_namesig_03_1
        {
            int F(int x);
        }
        class Interface_TestClass_namesig_03
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_namesig_04_1
        {
            void F(int x);
        }
        interface Interface_TestClass_namesig_04_2
        {
            void F(int x);
        }
        interface Interface_TestClass_namesig_04_3 : Interface_TestClass_namesig_04_1, Interface_TestClass_namesig_04_2
        {
        }
        class Interface_TestClass_namesig_04
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_namesig_05_1
        {
            void F(int x);
        }
        interface Interface_TestClass_namesig_05_2 : Interface_TestClass_namesig_05_1
        {
            new int F(int x);
        }
        class Interface_TestClass_namesig_05
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        public delegate void Interface_TestClass_events_08_ev(object sender);
        interface Interface_TestClass_events_08_2
        {
            new event Interface_TestClass_events_08_ev Click;
        }
        class Interface_TestClass_events_08
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        public delegate void Interface_TestClass_events_09_EH(object sender);
        interface Interface_TestClass_events_09_1
        {
            event Interface_TestClass_events_09_EH Click;
        }
        class Interface_TestClass_events_09
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_maccess_01_I1
        {
            int Count { get; set; }
        }
        interface Interface_TestClass_maccess_01_I2
        {
            void Count(int i);
        }
        interface IListCounter : Interface_TestClass_maccess_01_I1, Interface_TestClass_maccess_01_I2 { }
        class Interface_TestClass_maccess_01
        {
            void Test(IListCounter x)
            {
                ((Interface_TestClass_maccess_01_I1)x).Count = 1;
                ((Interface_TestClass_maccess_01_I2)x).Count(1);
            }

            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_maccess_03_I1
        {
            void Add(int i);
        }
        interface Interface_TestClass_maccess_03_I2
        {
            void Add(double d);
        }
        interface Interface_TestClass_maccess_03_I3 : Interface_TestClass_maccess_03_I1, Interface_TestClass_maccess_03_I2 { }
        class Interface_TestClass_maccess_03
        {
            void Test(Interface_TestClass_maccess_03_I3 n)
            {
                ((Interface_TestClass_maccess_03_I1)n).Add(1);
                ((Interface_TestClass_maccess_03_I2)n).Add(1);
            }

            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_maccess_04_I1
        {
            void Add(int i);
        }
        interface Interface_TestClass_maccess_04_I2
        {
            void Add(double d);
        }
        interface Interface_TestClass_maccess_04_I3 : Interface_TestClass_maccess_04_I1, Interface_TestClass_maccess_04_I2 { }
        class Interface_TestClass_maccess_04
        {
            void Test(Interface_TestClass_maccess_04_I3 n)
            {
                n.Add(1.2);
            }

            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_maccess_06_I2
        {
            int F(int i);
        }
        interface Interface_TestClass_maccess_06_I4 : Interface_TestClass_maccess_06_I2
        {
            new string F(int i);
        }
        interface Interface_TestClass_maccess_06_I5 : Interface_TestClass_maccess_06_I2
        {
            void G();
        }
        interface Interface_TestClass_maccess_06_I3 : Interface_TestClass_maccess_06_I4, Interface_TestClass_maccess_06_I5 { }
        class Interface_TestClass_maccess_06_C
        {
            void Interface_TestClass_maccess_06(Interface_TestClass_maccess_06_I3 d)
            {
                // Calling d.F(1) should return Interface_TestClass_maccess_06_2 string, which proves that
                // it's calling Interface_TestClass_maccess_06_I4's F method as expected.
                string x = d.F(1);
            }
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }

        private interface Interface_TestClass_modifier_02_I { }
        class Interface_TestClass_modifier_02
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        protected interface Interface_TestClass_modifier_03_I { }
        class Interface_TestClass_modifier_03
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }

        //Compiled Test Cases 
        interface Interface_TestClass_struct_impl_01_I1
        {
            void F();
        }
        interface Interface_TestClass_struct_impl_01_I2
        {
            void G();
        }
        struct Interface_TestClass_struct_impl_01_1 : Interface_TestClass_struct_impl_01_I1, Interface_TestClass_impl_01_I2
        {
            public void F() { }
            public void G() { }
        }
        class Interface_TestClass_struct_impl_01
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_struct_impl_02_I2
        {
            void Interface_TestClass_struct_impl_02_B();
        }
        interface Interface_TestClass_struct_impl_02_I3 : Interface_TestClass_struct_impl_02_I2
        {
            void Interface_TestClass_struct_impl_02_D();
        }
        struct Interface_TestClass_struct_impl_02_1 : Interface_TestClass_struct_impl_02_I3
        {
            public void Interface_TestClass_struct_impl_02_B() { }
            public void Interface_TestClass_struct_impl_02_D() { }
        }
        class Interface_TestClass_struct_impl_02
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_struct_impl_04_I1
        {
            void F();
        }
        struct Interface_TestClass_struct_impl_04_C : Interface_TestClass_struct_impl_04_I1
        {
            public void F()
            {
                Log.Comment("Interface_TestClass_struct_impl_04_C.F()");
            }
        }
        class Interface_TestClass_struct_impl_04
        {
            public static void Main_old()
            {
                Interface_TestClass_struct_impl_04_C cc = new Interface_TestClass_struct_impl_04_C();
                Interface_TestClass_struct_impl_04_I1 ic = cc;
                cc.F();
                ic.F();
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        public delegate void Interface_TestClass_struct_impl_05_ev(object sender);
        interface Interface_TestClass_struct_impl_05_1
        {
            void Method();
            int Property { get; set; }
            event Interface_TestClass_struct_impl_05_ev Clicked;
            string this[int index] { get; set; }
        }
        struct Interface_TestClass_struct_impl_05_2 : Interface_TestClass_struct_impl_05_1
        {
            private int num;

            public void Method()
            {
                Log.Comment("Method()");
            }
            public int Property
            {
                get
                {
                    return num;
                }
                set
                {
                    if (value >= 0 && value <= 10)
                        num = value;
                }
            }
            public event Interface_TestClass_struct_impl_05_ev Clicked;
            public string this[int index]
            {
                get { return "Interface_TestClass_struct_impl_05"; }
                set { }
            }
        }
        class Interface_TestClass_struct_impl_05
        {
            public static void Main_old()
            {
                Interface_TestClass_struct_impl_05_2 t = new Interface_TestClass_struct_impl_05_2();
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_struct_impl_06_I1
        {
            void Paint();
        }
        interface Interface_TestClass_struct_impl_06_I2
        {
            void Paint();
        }
        struct Interface_TestClass_struct_impl_06_1 : Interface_TestClass_struct_impl_06_I1, Interface_TestClass_struct_impl_06_I2
        {
            public void Paint() { }
        }
        class Interface_TestClass_struct_impl_06
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_struct_impl_07_I1
        {
            int MyProp { get; set; }
        }
        interface Interface_TestClass_struct_impl_07_I2
        {
            int MyProp { get; set; }
        }
        struct Interface_TestClass_struct_impl_07_1 : Interface_TestClass_struct_impl_07_I1, Interface_TestClass_struct_impl_07_I2
        {
            public int MyProp
            {
                get
                {
                    return 0;
                }
                set
                {
                }
            }
        }
        class Interface_TestClass_struct_impl_07
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        public delegate void Interface_TestClass_struct_impl_08_Del (object sender);
        interface Interface_TestClass_struct_impl_08_I1
        {
            event Interface_TestClass_struct_impl_08_Del Interface_TestClass_struct_impl_08_ev;
        }
        interface Interface_TestClass_struct_impl_08_I2
        {
            event Interface_TestClass_struct_impl_08_Del Interface_TestClass_struct_impl_08_ev;
        }
        struct Interface_TestClass_struct_impl_08_1 : Interface_TestClass_struct_impl_08_I1, Interface_TestClass_struct_impl_08_I2
        {
            public event Interface_TestClass_struct_impl_08_Del Interface_TestClass_struct_impl_08_ev;
        }
        class Interface_TestClass_struct_impl_08
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_struct_impl_09_I1
        {
            string this[int index] { get; set; }
        }
        interface Interface_TestClass_struct_impl_09_I2
        {
            string this[int index] { get; set; }
        }
        struct Interface_TestClass_struct_impl_09_1 : Interface_TestClass_struct_impl_09_I1, Interface_TestClass_struct_impl_09_I2
        {
            public string this[int index]
            {
                get { return "Interface_TestClass_struct_impl_09_1"; }
                set { }
            }
        }
        class Interface_TestClass_struct_impl_09
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_struct_explicit_01_I1
        {
            void F();
        }
        interface Interface_TestClass_struct_explicit_01_I2
        {
            void G();
        }
        struct Interface_TestClass_struct_explicit_01_C : Interface_TestClass_struct_explicit_01_I1, Interface_TestClass_struct_explicit_01_I2
        {
            void Interface_TestClass_struct_explicit_01_I1.F() { }
            void Interface_TestClass_struct_explicit_01_I2.G() { }
        }
        class Interface_TestClass_struct_explicit_01
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_struct_explicit_03_I1
        {
            void F();
        }
        interface Interface_TestClass_struct_explicit_03_I2 : Interface_TestClass_struct_explicit_03_I1
        {
            void G();
        }
        struct Interface_TestClass_struct_explicit_03_C : Interface_TestClass_struct_explicit_03_I2
        {
            void Interface_TestClass_struct_explicit_03_I1.F() { }
            void Interface_TestClass_struct_explicit_03_I2.G() { }
        }
        class Interface_TestClass_struct_explicit_03
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_struct_explicit_04_1
        {
            void Method();
        }
        struct Interface_TestClass_struct_explicit_04_2 : Interface_TestClass_struct_explicit_04_1
        {
            void Interface_TestClass_struct_explicit_04_1.Method()
            {
                Log.Comment("Method()");
            }
        }
        class Interface_TestClass_struct_explicit_04
        {
            public static void Main_old()
            {
                Interface_TestClass_struct_explicit_04_2 t = new Interface_TestClass_struct_explicit_04_2();
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_struct_explicit_05_1
        {
            int Property { get; set; }
        }
        struct Interface_TestClass_struct_explicit_05_2 : Interface_TestClass_struct_explicit_05_1
        {
            private int num;

            int Interface_TestClass_struct_explicit_05_1.Property
            {
                get
                {
                    return num;
                }
                set
                {
                    if (value >= 0 && value <= 10)
                        num = value;
                }
            }
        }
        class Interface_TestClass_struct_explicit_05
        {
            public static void Main_old()
            {
                Interface_TestClass_struct_explicit_05_2 t = new Interface_TestClass_struct_explicit_05_2();
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        public delegate void Interface_TestClass_struct_explicit_06_ev(object sender);
        interface Interface_TestClass_struct_explicit_06_1
        {
            event Interface_TestClass_struct_explicit_06_ev Clicked;
        }
        struct Interface_TestClass_struct_explicit_06_2 : Interface_TestClass_struct_explicit_06_1
        {
            event Interface_TestClass_struct_explicit_06_ev Interface_TestClass_struct_explicit_06_1.Clicked
            {
                add { }
                remove { }
            }
            public void MyHandler(object sender)
            {

            }
        }
        class Interface_TestClass_struct_explicit_06
        {
            public static void Main_old()
            {
                Interface_TestClass_struct_explicit_06_2 t = new Interface_TestClass_struct_explicit_06_2();
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_struct_explicit_07_1
        {
            string this[int index] { get; set; }
        }
        struct Interface_TestClass_struct_explicit_07_2 : Interface_TestClass_struct_explicit_07_1
        {
            string Interface_TestClass_struct_explicit_07_1.this[int index]
            {
                get { return "Interface_TestClass_struct_explicit_07"; }
                set { }
            }
        }
        class Interface_TestClass_struct_explicit_07
        {
            public static void Main_old()
            {
                Interface_TestClass_struct_explicit_07_2 t = new Interface_TestClass_struct_explicit_07_2();
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_struct_explicit_09_I1
        {
            object Clone();
        }
        interface Interface_TestClass_struct_explicit_09_I2
        {
            int CompareTo(object other);
        }
        struct Interface_TestClass_struct_explicit_09_1 : Interface_TestClass_struct_explicit_09_I1, Interface_TestClass_struct_explicit_09_I2
        {
            object Interface_TestClass_struct_explicit_09_I1.Clone()
            {
                return new object();
            }

            int Interface_TestClass_struct_explicit_09_I2.CompareTo(object other)
            {
                return 0;
            }
        }
        class Interface_TestClass_struct_explicit_09
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_struct_explicit_10_I1
        {
            object Clone();
        }
        interface Interface_TestClass_struct_explicit_10_I2
        {
            int CompareTo(object other);
        }
        struct Interface_TestClass_struct_explicit_10_1 : Interface_TestClass_struct_explicit_10_I1, Interface_TestClass_struct_explicit_10_I2
        {
            object Interface_TestClass_struct_explicit_10_I1.Clone()
            {
                return new object();
            }

            int Interface_TestClass_struct_explicit_10_I2.CompareTo(object other)
            {
                return 0;
            }
        }
        class Interface_TestClass_struct_explicit_10
        {
            public static void Main_old()
            {
                Interface_TestClass_struct_explicit_10_1 le = new Interface_TestClass_struct_explicit_10_1();
                object o = ((Interface_TestClass_struct_explicit_10_I1)le).Clone();
            }
            public static bool testMethod()
            {
                try
                {
                    Main_old();
                }
                catch (System.Exception)
                {
                    return false;
                }
                return true;
            }
        }
        interface Interface_TestClass_struct_explicit_11_I1
        {
            int Value { get; set; }
        }
        interface Interface_TestClass_struct_explicit_11_I2
        {
            int CompareTo(object other);
        }
        struct Interface_TestClass_struct_explicit_11_1 : Interface_TestClass_struct_explicit_11_I1, Interface_TestClass_struct_explicit_11_I2
        {
            private int val;

            int Interface_TestClass_struct_explicit_11_I1.Value
            {
                get
                {
                    return val;
                }
                set
                {
                    val = value;
                    Log.Comment(val.ToString());
                }
            }

            int Interface_TestClass_struct_explicit_11_I2.CompareTo(object other)
            {
                return 0;
            }
        }
        class Interface_TestClass_struct_explicit_11
        {
            public static int Main_old()
            {
                try
                {
                    Interface_TestClass_struct_explicit_11_1 le = new Interface_TestClass_struct_explicit_11_1();
                    ((Interface_TestClass_struct_explicit_11_I1)le).Value = 5;

                    Log.Comment((((Interface_TestClass_struct_explicit_11_I1)le).Value).ToString());
                    return 0;
                }
                catch (System.Exception)
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        interface Interface_TestClass_struct_explicit_12_I1
        {
            string this[string hashkey] { get; set; }
        }
        interface Interface_TestClass_struct_explicit_12_I2
        {
            int CompareTo(object other);
        }
        struct Interface_TestClass_struct_explicit_12_1 : Interface_TestClass_struct_explicit_12_I1, Interface_TestClass_struct_explicit_12_I2
        {
            public string st;
            string Interface_TestClass_struct_explicit_12_I1.this[string hashkey]
            {
                get
                {
                    return "success";
                }
                set
                {
                    st = value;
                }
            }

            int Interface_TestClass_struct_explicit_12_I2.CompareTo(object other)
            {
                return 0;
            }
        }
        class Interface_TestClass_struct_explicit_12
        {
            public static int Main_old()
            {
                try
                {
                    Interface_TestClass_struct_explicit_12_1 le = new Interface_TestClass_struct_explicit_12_1();
                    ((Interface_TestClass_struct_explicit_12_I1)le)["Interface_TestClass_struct_explicit_12_1"] = "correct";
                    string s = ((Interface_TestClass_struct_explicit_12_I1)le)["Interface_TestClass_struct_explicit_12_1"];
                    if (s == "success")
                        return 0;
                    else
                        return 1;
                }
                catch (System.Exception)
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public delegate void Interface_TestClass_struct_explicit_13_ev(object sender);
        interface Interface_TestClass_struct_explicit_13_1
        {
            event Interface_TestClass_struct_explicit_13_ev Clicked;
        }
        struct Interface_TestClass_struct_explicit_13_2 : Interface_TestClass_struct_explicit_13_1
        {
            event Interface_TestClass_struct_explicit_13_ev Interface_TestClass_struct_explicit_13_1.Clicked
            {
                add { }
                remove { }
            }
            public void MyHandler(object sender)
            {
            }
        }
        class Interface_TestClass_struct_explicit_13
        {
            public static void Main_old()
            {
                new Interface_TestClass_struct_explicit_13().Run();
            }
            public void Run()
            {
                Interface_TestClass_struct_explicit_13_2 t = new Interface_TestClass_struct_explicit_13_2();
                ((Interface_TestClass_struct_explicit_13_1)t).Clicked += new Interface_TestClass_struct_explicit_13_ev(TheHandler);
            }
            public void TheHandler(object sender)
            {
            }
            public static bool testMethod()
            {
                try
                {
                    Main_old();
                }
                catch (System.Exception)
                {
                    return false;
                }
                return true;
            }
        }
        interface Interface_TestClass_struct_explicit_19_I1
        {
            object Clone();
        }
        interface Interface_TestClass_struct_explicit_19_I3 : Interface_TestClass_struct_explicit_19_I1
        {
            string OtherMethod();
        }
        class Interface_TestClass_struct_explicit_19_1 : Interface_TestClass_struct_explicit_19_I3
        {
            string Interface_TestClass_struct_explicit_19_I3.OtherMethod()
            {
                return "Interface_TestClass_struct_explicit_19_1";
            }

            object Interface_TestClass_struct_explicit_19_I1.Clone()
            {
                return new object();
            }
        }
        class Interface_TestClass_struct_explicit_19
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        interface Interface_TestClass_struct_explicit_20_I2
        {
            int P { get; }
        }
        interface Interface_TestClass_struct_explicit_20_I3 : Interface_TestClass_struct_explicit_20_I2
        {
            new int P();
        }
        struct Interface_TestClass_struct_explicit_20_1 : Interface_TestClass_struct_explicit_20_I3
        {
            int Interface_TestClass_struct_explicit_20_I2.P { get { return 1; } }
            int Interface_TestClass_struct_explicit_20_I3.P() { return 1; }
        }
        struct Interface_TestClass_struct_explicit_20_2 : Interface_TestClass_struct_explicit_20_I3
        {
            public int P { get { return 1; } }
            int Interface_TestClass_struct_explicit_20_I3.P() { return 1; }
        }
        struct Interface_TestClass_struct_explicit_20_3 : Interface_TestClass_struct_explicit_20_I3
        {
            int Interface_TestClass_struct_explicit_20_I2.P { get { return 1; } }
            public int P() { return 1; }
        }
        class Interface_TestClass_struct_explicit_20
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        struct Interface_TestClass_struct_decl_01_1
        {
            interface Interface_TestClass_struct_decl_01_I
            {
            }
        }
        class Interface_TestClass_struct_decl_01
        {
            public static void Main_old()
            {
                Log.Comment("This works!");
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }

        interface Interface_TestClass_struct_inherit_01_A
        {
            void m();
        }
        interface Interface_TestClass_struct_inherit_01_B : Interface_TestClass_struct_inherit_01_A { }
        interface Interface_TestClass_struct_inherit_01_C : Interface_TestClass_struct_inherit_01_B { }
        struct Interface_TestClass_struct_inherit_01_1 : Interface_TestClass_struct_inherit_01_C
        {
            public void m()
            {
                Log.Comment("This works!");
            }
        }
        class Interface_TestClass_struct_inherit_01
        {
            public static void Main_old()
            {
                Interface_TestClass_struct_inherit_01_1 t = new Interface_TestClass_struct_inherit_01_1();
                t.m();
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        struct Interface_TestClass_struct_modifier_01_1
        {
            public interface Interface_TestClass_struct_modifier_01_I { }
            internal interface Interface_TestClass_struct_modifier_01_I2 { }
        }
        class Interface_TestClass_struct_modifier_01
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        struct Interface_TestClass_struct_modifier_02_1
        {
            private interface Interface_TestClass_struct_modifier_02_I { }
        }
        class Interface_TestClass_struct_modifier_02
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        struct Interface_TestClass_struct_modifier_03_1
        {
            new interface Interface_TestClass_struct_modifier_03_I { }
        }
        class Interface_TestClass_struct_modifier_03
        {
            public static void Main_old() { }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }
        struct Interface_TestClass_struct_semicolon_01_1
        {
            interface Interface_TestClass_struct_semicolon_01_I
            {
            };
        }
        class Interface_TestClass_struct_semicolon_01
        {
            public static void Main_old()
            {
                Log.Comment("This works!");
            }
            public static bool testMethod()
            {
                Main_old();
                return true;
            }
        }




    }
}
namespace Interface_TestClass_struct_decl_02_NS
{

    struct Interface_TestClass_struct_decl_02_I
    {
    }

    class Interface_TestClass_struct_decl_02
    {
        public static void Main_old()
        {
            Log.Comment("This works!");
        }
        public static bool testMethod()
        {
            Main_old();
            return true;
        }
    }
}

namespace Interface_TestClass_struct_semicolon_02_NS
{

    struct Interface_TestClass_struct_semicolon_02_1
    {
        interface Interface_TestClass_struct_semicolon_02_I
        {
        };
    }
    class Interface_TestClass_struct_semicolon_02
    {
        public static void Main_old()
        {
            Log.Comment("This works!");
        }
        public static bool testMethod()
        {
            Main_old();
            return true;
        }
    }
}


namespace Interface_TestClass_decl_02_NS
{

    interface Interface_TestClass_decl_02_I
    {
    }
    class Interface_TestClass_decl_02
    {
        public static void Main_old()
        {
            Log.Comment("This works!");
        }
        public static bool testMethod()
        {
            Main_old();
            return true;
        }
    }
}


namespace Interface_TestClass_semicolon_04_NS
{

    class Interface_TestClass_semicolon_04
    {
        interface Interface_TestClass_semicolon_04_I
        {
        };
        public static void Main_old()
        {
            Log.Comment("This works!");
        }
        public static bool testMethod()
        {
            Main_old();
            return true;
        }
    }
}


namespace Interface_TestClass_semicolon_02_NS
{

    interface Interface_TestClass_semicolon_02_I
    {
    };
    class Interface_TestClass_semicolon_02
    {
        public static void Main_old()
        {
            Log.Comment("This works!");
        }
        public static bool testMethod()
        {
            Main_old();
            return true;
        }
    }
}


namespace Interface_TestClass_decl_04_NS
{

    class Interface_TestClass_decl_04
    {
        interface Interface_TestClass_decl_04_I
        {
        }
        public static void Main_old()
        {
            Log.Comment("This works!");
        }
        public static bool testMethod()
        {
            Main_old();
            return true;
        }
    }
}

