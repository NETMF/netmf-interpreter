////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class MethodsTests : IMFTestInterface
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

        //Methods Test methods
        //All test methods ported from folder current\test\cases\client\CLR\Conformance\10_classes\Methods
        //The following tests were removed because they were build failure tests:
        //5,8,12,14-16,18,21,27,28,31,32,62-65,72-74,76,77,81-83,86-91,96-102,113-115,
        //122,123,126-131,135-141,143-147,151-153,208,209,211-22,225-228,234-239
        //189 was removed because it required external files, it failed in the Baseline document
        //11,23,67,70,79,104,107,110,112,134,189,195,
        //Skip 17
        [TestMethod]
        public MFTestResults Methods1_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A method-declaration may include set of attributes,");
            Log.Comment(" a new modifier, one of four access modifiers,");
            Log.Comment(" one of the static, virtual, override, or abstract ");
            Log.Comment(" modifiers, and an extern modifier.");
            if (MethodsTestClass1.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods2_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A method-declaration may include set of attributes,");
            Log.Comment(" a new modifier, one of four access modifiers,");
            Log.Comment(" one of the static, virtual, override, or abstract ");
            Log.Comment(" modifiers, and an extern modifier.");
            if (MethodsTestClass2.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods3_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A method-declaration may include set of attributes,");
            Log.Comment(" a new modifier, one of four access modifiers,");
            Log.Comment(" one of the static, virtual, override, or abstract ");
            Log.Comment(" modifiers, and an extern modifier.");
            if (MethodsTestClass3.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;

        }

        [TestMethod]
        public MFTestResults Methods4_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A method-declaration may include set of attributes,");
            Log.Comment(" a new modifier, one of four access modifiers,");
            Log.Comment(" one of the static, virtual, override, or abstract ");
            Log.Comment(" modifiers, and an extern modifier.");
            if (MethodsTestClass4.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods6_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A method-declaration may include set of attributes,");
            Log.Comment(" a new modifier, one of four access modifiers,");
            Log.Comment(" one of the static, virtual, override, or abstract ");
            Log.Comment(" modifiers, and an extern modifier.");
            if (MethodsTestClass6.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods7_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A method-declaration may include set of attributes,");
            Log.Comment(" a new modifier, one of four access modifiers,");
            Log.Comment(" one of the static, virtual, override, or abstract ");
            Log.Comment(" modifiers, and an extern modifier.");
            if (MethodsTestClass7.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods9_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A method-declaration may include set of attributes,");
            Log.Comment(" a new modifier, one of four access modifiers,");
            Log.Comment(" one of the static, virtual, override, or abstract ");
            Log.Comment(" modifiers, and an extern modifier.");
            if (MethodsTestClass9.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods10_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A method-declaration may include set of attributes,");
            Log.Comment(" a new modifier, one of four access modifiers,");
            Log.Comment(" one of the static, virtual, override, or abstract ");
            Log.Comment(" modifiers, and an extern modifier.");
            if (MethodsTestClass10.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods11_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A method-declaration may include set of attributes,");
            Log.Comment(" a new modifier, one of four access modifiers,");
            Log.Comment(" one of the static, virtual, override, or abstract ");
            Log.Comment(" modifiers, and an extern modifier.");
            if (MethodsTestClass11.testMethod())
            {
                Log.Comment("This failure indicates a test is now passing that previously failed by design.");
                Log.Comment("It previously marked as known failure because of bug # 21563");
                Log.Comment("The Test owner needs to verify that the change was intentional and remove the known failure.");
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults Methods13_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A method-declaration may include set of attributes,");
            Log.Comment(" a new modifier, one of four access modifiers,");
            Log.Comment(" one of the static, virtual, override, or abstract ");
            Log.Comment(" modifiers, and an extern modifier.");
            if (MethodsTestClass13.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods17_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A method-declaration may include set of attributes,");
            Log.Comment(" a new modifier, one of four access modifiers,");
            Log.Comment(" one of the static, virtual, override, or abstract ");
            Log.Comment(" modifiers, and an extern modifier.");
            if (MethodsTestClass17.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods19_Test()
        {
            Log.Comment(" The return-type of a method declaration specifies");
            Log.Comment(" the type of the value computed and returned by the");
            Log.Comment(" method. The return type is void if the method does");
            Log.Comment(" not return a value.");
            if (MethodsTestClass19.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods20_Test()
        {
            Log.Comment(" The return-type of a method declaration specifies");
            Log.Comment(" the type of the value computed and returned by the");
            Log.Comment(" method. The return type is void if the method does");
            Log.Comment(" not return a value.");
            if (MethodsTestClass20.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods22_Test()
        {
            Log.Comment(" The return-type of a method declaration specifies");
            Log.Comment(" the type of the value computed and returned by the");
            Log.Comment(" method. The return type is void if the method does");
            Log.Comment(" not return a value.");
            if (MethodsTestClass22.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods23_Test()
        {
            Log.Comment(" The return-type of a method declaration specifies");
            Log.Comment(" the type of the value computed and returned by the");
            Log.Comment(" method. The return type is void if the method does");
            Log.Comment(" not return a value.");
            if (MethodsTestClass23.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods24_Test()
        {
            Log.Comment(" The return-type of a method declaration specifies");
            Log.Comment(" the type of the value computed and returned by the");
            Log.Comment(" method. The return type is void if the method does");
            Log.Comment(" not return a value.");
            if (MethodsTestClass24.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods25_Test()
        {
            Log.Comment(" The return-type of a method declaration specifies");
            Log.Comment(" the type of the value computed and returned by the");
            Log.Comment(" method. The return type is void if the method does");
            Log.Comment(" not return a value.");
            if (MethodsTestClass25.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods26_Test()
        {
            Log.Comment(" The return-type of a method declaration specifies");
            Log.Comment(" the type of the value computed and returned by the");
            Log.Comment(" method. The return type is void if the method does");
            Log.Comment(" not return a value.");
            if (MethodsTestClass26.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods29_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" The member-name specifies the name of the method.");
            Log.Comment(" Unless the method is an explicit interface member");
            Log.Comment(" implementation, the member-name is simply an ");
            Log.Comment(" identifier.  For an explicit interface member ");
            Log.Comment(" implementation, the member-name consists of an");
            Log.Comment(" interface-type followed by a . and an identifier.");
            if (MethodsTestClass29.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods30_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" The member-name specifies the name of the method.");
            Log.Comment(" Unless the method is an explicit interface member");
            Log.Comment(" implementation, the member-name is simply an ");
            Log.Comment(" identifier.  For an explicit interface member ");
            Log.Comment(" implementation, the member-name consists of an");
            Log.Comment(" interface-type followed by a . and an identifier.");
            if (MethodsTestClass30.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods33_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" The optional formal-parameter-list specifies");
            Log.Comment(" the parameters of the method.");
            if (MethodsTestClass33.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods34_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" The optional formal-parameter-list specifies");
            Log.Comment(" the parameters of the method.");
            if (MethodsTestClass34.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods35_Test()
        {
            Log.Comment("Testing method call with 10 parameters");
            if (MethodsTestClass35.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods56_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A method declaration creates a separate space");
            Log.Comment(" for parameters and local variables.  Names are introduced");
            Log.Comment(" into this declaration space by the formal parameter");
            Log.Comment(" list of the method and by the local variable declarations");
            Log.Comment(" in the block of the method.  All names in the declaration");
            Log.Comment(" space of a method must be unique.  Thus it is an error");
            Log.Comment(" for a parameter or local variable to have the same name");
            Log.Comment(" as another parameter or local variable.");
            if (MethodsTestClass56.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods57_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A method declaration creates a separate space");
            Log.Comment(" for parameters and local variables.  Names are introduced");
            Log.Comment(" into this declaration space by the formal parameter");
            Log.Comment(" list of the method and by the local variable declarations");
            Log.Comment(" in the block of the method.  All names in the declaration");
            Log.Comment(" space of a method must be unique.  Thus it is an error");
            Log.Comment(" for a parameter or local variable to have the same name");
            Log.Comment(" as another parameter or local variable.");
            if (MethodsTestClass57.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods58_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" When a formal parameter is a value parameter,");
            Log.Comment(" the corresponding argument in the method invocation");
            Log.Comment(" must be an expression of a type that is implicitly");
            Log.Comment(" convertible to the formal parameter type.");
            if (MethodsTestClass58.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods59_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" When a formal parameter is a value parameter,");
            Log.Comment(" the corresponding argument in the method invocation");
            Log.Comment(" must be an expression of a type that is implicitly");
            Log.Comment(" convertible to the formal parameter type.");
            if (MethodsTestClass59.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods60_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" When a formal parameter is a value parameter,");
            Log.Comment(" the corresponding argument in the method invocation");
            Log.Comment(" must be an expression of a type that is implicitly");
            Log.Comment(" convertible to the formal parameter type.");
            if (MethodsTestClass60.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods61_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" When a formal parameter is a value parameter,");
            Log.Comment(" the corresponding argument in the method invocation");
            Log.Comment(" must be an expression of a type that is implicitly");
            Log.Comment(" convertible to the formal parameter type.");
            if (MethodsTestClass61.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods66_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A method is permitted to assign new values to ");
            Log.Comment(" a value parameter.  Such assignments only affect");
            Log.Comment(" the local storage location represented by the ");
            Log.Comment(" value parameter--they have no effect on the actual ");
            Log.Comment(" argument given in the method invocation.");
            if (MethodsTestClass66.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods67_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A method is permitted to assign new values to ");
            Log.Comment(" a value parameter.  Such assignments only affect");
            Log.Comment(" the local storage location represented by the ");
            Log.Comment(" value parameter--they have no effect on the actual ");
            Log.Comment(" argument given in the method invocation.");
            if (MethodsTestClass67.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods68_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A method is permitted to assign new values to ");
            Log.Comment(" a value parameter.  Such assignments only affect");
            Log.Comment(" the local storage location represented by the ");
            Log.Comment(" value parameter--they have no effect on the actual ");
            Log.Comment(" argument given in the method invocation.");
            if (MethodsTestClass68.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods69_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A parameter declared with a ref modifier is a ");
            Log.Comment(" reference parameter.  Unlike a value parameter,");
            Log.Comment(" a reference parameter does not create a new ");
            Log.Comment(" storage location.  Instead, a reference parameter");
            Log.Comment(" represents the same storage location as the");
            Log.Comment(" variable given as the argument in the method");
            Log.Comment(" invocation.");
            if (MethodsTestClass69.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods70_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A parameter declared with a ref modifier is a ");
            Log.Comment(" reference parameter.  Unlike a value parameter,");
            Log.Comment(" a reference parameter does not create a new ");
            Log.Comment(" storage location.  Instead, a reference parameter");
            Log.Comment(" represents the same storage location as the");
            Log.Comment(" variable given as the argument in the method");
            Log.Comment(" invocation.");
            if (MethodsTestClass70.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods71_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A parameter declared with a ref modifier is a ");
            Log.Comment(" reference parameter.  Unlike a value parameter,");
            Log.Comment(" a reference parameter does not create a new ");
            Log.Comment(" storage location.  Instead, a reference parameter");
            Log.Comment(" represents the same storage location as the");
            Log.Comment(" variable given as the argument in the method");
            Log.Comment(" invocation.");
            if (MethodsTestClass71.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods75_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" When a formal parameter is a reference parameter,");
            Log.Comment(" the corresponding argument in a method invocation");
            Log.Comment(" must consist of the keyword ref followed by a ");
            Log.Comment(" variable-reference of the same type as the formal");
            Log.Comment(" parameter.  A variable must be definitely assigned");
            Log.Comment(" before it can be passed as a reference parameter.");
            if (MethodsTestClass75.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods78_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A parameter declared with an out modifier is an output");
            Log.Comment(" parameter.  Similar to a reference parameter, an output");
            Log.Comment(" parameter does not create a new storage location.  Instead,");
            Log.Comment(" an output parameter represents the same storage location");
            Log.Comment(" as the variable given as the argument in the method invocation.");
            if (MethodsTestClass78.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods79_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A parameter declared with an out modifier is an output");
            Log.Comment(" parameter.  Similar to a reference parameter, an output");
            Log.Comment(" parameter does not create a new storage location.  Instead,");
            Log.Comment(" an output parameter represents the same storage location");
            Log.Comment(" as the variable given as the argument in the method invocation.");
            if (MethodsTestClass79.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods80_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A parameter declared with an out modifier is an output");
            Log.Comment(" parameter.  Similar to a reference parameter, an output");
            Log.Comment(" parameter does not create a new storage location.  Instead,");
            Log.Comment(" an output parameter represents the same storage location");
            Log.Comment(" as the variable given as the argument in the method invocation.");
            if (MethodsTestClass80.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods84_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" When a formal parameter is an output parameter,");
            Log.Comment(" the corresponding argument in a method invocation");
            Log.Comment(" must consist of the keyword out followed by a ");
            Log.Comment(" variable-reference of the same type as the formal ");
            Log.Comment(" parameter.  A variable need not be definitely");
            Log.Comment(" assigned before it can be passed as an output");
            Log.Comment(" parameter, but following an invocation where a ");
            Log.Comment(" variable was passed as an output parameter, the");
            Log.Comment(" variable is considered definitely assigned.");
            if (MethodsTestClass84.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods85_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" When a formal parameter is an output parameter,");
            Log.Comment(" the corresponding argument in a method invocation");
            Log.Comment(" must consist of the keyword out followed by a ");
            Log.Comment(" variable-reference of the same type as the formal ");
            Log.Comment(" parameter.  A variable need not be definitely");
            Log.Comment(" assigned before it can be passed as an output");
            Log.Comment(" parameter, but following an invocation where a ");
            Log.Comment(" variable was passed as an output parameter, the");
            Log.Comment(" variable is considered definitely assigned.");
            if (MethodsTestClass85.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods92_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" Every output parameter of a method must be");
            Log.Comment(" definitely assigned before the method returns.");
            if (MethodsTestClass92.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods93_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" Every output parameter of a method must be");
            Log.Comment(" definitely assigned before the method returns.");
            if (MethodsTestClass93.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods94_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" Every output parameter of a method must be");
            Log.Comment(" definitely assigned before the method returns.");
            if (MethodsTestClass94.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods95_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" Every output parameter of a method must be");
            Log.Comment(" definitely assigned before the method returns.");
            if (MethodsTestClass95.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods103_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" The implementation of a non-virtual method");
            Log.Comment(" is invariant: The implementation is the same ");
            Log.Comment(" whether the method is invoked on an instance");
            Log.Comment(" of the class in which it is declared or an ");
            Log.Comment(" instance of the derived class.");
            if (MethodsTestClass103.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;

        }

        [TestMethod]
        public MFTestResults Methods104_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" The implementation of a non-virtual method");
            Log.Comment(" is invariant: The implementation is the same ");
            Log.Comment(" whether the method is invoked on an instance");
            Log.Comment(" of the class in which it is declared or an ");
            Log.Comment(" instance of the derived class.");
            if (MethodsTestClass104.testMethod())
            {
                Log.Comment("This failure indicates a test is now passing that previously failed by design.");
                Log.Comment("It previously marked as known failure because of bug # 21563");
                Log.Comment("The Test owner needs to verify that the change was intentional and remove the known failure.");
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;

        }

        [TestMethod]
        public MFTestResults Methods105_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" The implementation of a non-virtual method");
            Log.Comment(" is invariant: The implementation is the same ");
            Log.Comment(" whether the method is invoked on an instance");
            Log.Comment(" of the class in which it is declared or an ");
            Log.Comment(" instance of the derived class.");
            if (MethodsTestClass105.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods106_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" For every virtual method declaration of M,");
            Log.Comment(" there exists a most derived implementation");
            Log.Comment(" of the method with respect to the class.");
            Log.Comment(" The most derived implementation of a ");
            Log.Comment(" virtual method M with respectto a class");
            Log.Comment(" R is determined as follows:");
            Log.Comment("");
            Log.Comment(" If R contains the introducing virtual");
            Log.Comment(" declaration of M, then this is the most");
            Log.Comment(" derived implementation of M.");
            if (MethodsTestClass106.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods107_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" For every virtual method declaration of M,");
            Log.Comment(" there exists a most derived implementation");
            Log.Comment(" of the method with respect to the class.");
            Log.Comment(" The most derived implementation of a ");
            Log.Comment(" virtual method M with respectto a class");
            Log.Comment(" R is determined as follows:");
            Log.Comment("");
            Log.Comment(" If R contains the introducing virtual");
            Log.Comment(" declaration of M, then this is the most");
            Log.Comment(" derived implementation of M.");
            if (MethodsTestClass107.testMethod())
            {
                Log.Comment("This failure indicates a test is now passing that previously failed by design.");
                Log.Comment("It previously marked as known failure because of bug # 21563");
                Log.Comment("The Test owner needs to verify that the change was intentional and remove the known failure.");
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults Methods108_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" For every virtual method declaration of M,");
            Log.Comment(" there exists a most derived implementation");
            Log.Comment(" of the method with respect to the class.");
            Log.Comment(" The most derived implementation of a ");
            Log.Comment(" virtual method M with respectto a class");
            Log.Comment(" R is determined as follows:");
            Log.Comment("");
            Log.Comment(" If R contains the introducing virtual");
            Log.Comment(" declaration of M, then this is the most");
            Log.Comment(" derived implementation of M.");
            Log.Comment("");
            Log.Comment(" Otherwise, if R contains an override of M,");
            Log.Comment(" then this is the most derived implementation");
            Log.Comment(" of M.");
            if (MethodsTestClass108.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods109_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" For every virtual method declaration of M,");
            Log.Comment(" there exists a most derived implementation");
            Log.Comment(" of the method with respect to the class.");
            Log.Comment(" The most derived implementation of a ");
            Log.Comment(" virtual method M with respectto a class");
            Log.Comment(" R is determined as follows:");
            Log.Comment("");
            Log.Comment(" If R contains the introducing virtual");
            Log.Comment(" declaration of M, then this is the most");
            Log.Comment(" derived implementation of M.");
            Log.Comment("");
            Log.Comment(" Otherwise, if R contains an override of M,");
            Log.Comment(" then this is the most derived implementation");
            Log.Comment(" of M.");
            if (MethodsTestClass109.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods110_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" For every virtual method declaration of M,");
            Log.Comment(" there exists a most derived implementation");
            Log.Comment(" of the method with respect to the class.");
            Log.Comment(" The most derived implementation of a ");
            Log.Comment(" virtual method M with respectto a class");
            Log.Comment(" R is determined as follows:");
            Log.Comment("");
            Log.Comment(" If R contains the introducing virtual");
            Log.Comment(" declaration of M, then this is the most");
            Log.Comment(" derived implementation of M.");
            Log.Comment("");
            Log.Comment(" Otherwise, if R contains an override of M,");
            Log.Comment(" then this is the most derived implementation");
            Log.Comment(" of M.");
            if (MethodsTestClass110.testMethod())
            {
                Log.Comment("This failure indicates a test is now passing that previously failed by design.");
                Log.Comment("It previously marked as known failure because of bug # 21563");
                Log.Comment("The Test owner needs to verify that the change was intentional and remove the known failure.");
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults Methods111_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" For every virtual method declaration of M,");
            Log.Comment(" there exists a most derived implementation");
            Log.Comment(" of the method with respect to the class.");
            Log.Comment(" The most derived implementation of a ");
            Log.Comment(" virtual method M with respectto a class");
            Log.Comment(" R is determined as follows:");
            Log.Comment("");
            Log.Comment(" If R contains the introducing virtual");
            Log.Comment(" declaration of M, then this is the most");
            Log.Comment(" derived implementation of M.");
            Log.Comment("");
            Log.Comment(" Otherwise, if R contains an override of M,");
            Log.Comment(" then this is the most derived implementation");
            Log.Comment(" of M.");
            if (MethodsTestClass111.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods112_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" Because methods are allowed to hide inherited");
            Log.Comment(" methods, it is possible for a class to contain");
            Log.Comment(" several virtual methods with the same signature.");
            Log.Comment(" This does not provide an ambiguity problem, since");
            Log.Comment(" all but the most derived method are hidden.");
            if (MethodsTestClass112.testMethod())
            {
                Log.Comment("This failure indicates a test is now passing that previously failed by design.");
                Log.Comment("It previously marked as known failure because of bug # 21563");
                Log.Comment("The Test owner needs to verify that the change was intentional and remove the known failure.");
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults Methods116_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" It is an error for an override method declaration");
            Log.Comment(" to include any one of the new, static, virtual, or ");
            Log.Comment(" abstract modifiers.");
            if (MethodsTestClass116.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods117_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" For purposes of locating the overridden base");
            Log.Comment(" method, a method is considered accessible if ");
            Log.Comment(" it is public, if it is protected, if it is ");
            Log.Comment(" internal and declared in the same project as ");
            Log.Comment(" C, or if it is private and declared in a class");
            Log.Comment(" containing the declaration of C.");
            if (MethodsTestClass117.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods119_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" For purposes of locating the overridden base");
            Log.Comment(" method, a method is considered accessible if ");
            Log.Comment(" it is public, if it is protected, if it is ");
            Log.Comment(" internal and declared in the same project as ");
            Log.Comment(" C, or if it is private and declared in a class");
            Log.Comment(" containing the declaration of C.");
            if (MethodsTestClass119.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods120_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" For purposes of locating the overridden base");
            Log.Comment(" method, a method is considered accessible if ");
            Log.Comment(" it is public, if it is protected, if it is ");
            Log.Comment(" internal and declared in the same project as ");
            Log.Comment(" C, or if it is private and declared in a class");
            Log.Comment(" containing the declaration of C.");
            if (MethodsTestClass120.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods121_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" For purposes of locating the overridden base");
            Log.Comment(" method, a method is considered accessible if ");
            Log.Comment(" it is public, if it is protected, if it is ");
            Log.Comment(" internal and declared in the same project as ");
            Log.Comment(" C, or if it is private and declared in a class");
            Log.Comment(" containing the declaration of C.");
            if (MethodsTestClass121.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods124_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A compile time-error occurs unless all");
            Log.Comment(" of the following are true for an override");
            Log.Comment(" declaration:");
            Log.Comment("");
            Log.Comment(" An overriddden base method can be located");
            Log.Comment(" as described above.");
            Log.Comment("");
            Log.Comment(" The overridden base method is virtual,");
            Log.Comment(" abstract, or override method.  In other");
            Log.Comment(" words, the overridden base method cannot");
            Log.Comment(" be static or non-virtual.");
            if (MethodsTestClass124.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods125_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A compile time-error occurs unless all");
            Log.Comment(" of the following are true for an override");
            Log.Comment(" declaration:");
            Log.Comment("");
            Log.Comment(" An overriddden base method can be located");
            Log.Comment(" as described above.");
            Log.Comment("");
            Log.Comment(" The overridden base method is virtual,");
            Log.Comment(" abstract, or override method.  In other");
            Log.Comment(" words, the overridden base method cannot");
            Log.Comment(" be static or non-virtual.");
            if (MethodsTestClass125.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods132_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" An override declaration can access the overridden ");
            Log.Comment(" base method using a base-access.");
            if (MethodsTestClass132.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods133_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" An override declaration can access the overridden ");
            Log.Comment(" base method using a base-access.");
            if (MethodsTestClass133.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods134_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" Only by including an override modifier can");
            Log.Comment(" a method override another method.  In all other");
            Log.Comment(" cases, a method with the same signature as an");
            Log.Comment(" inherited method simply hides the inherited ");
            Log.Comment(" member.");
            if (MethodsTestClass134.testMethod())
            {
                Log.Comment("This failure indicates a test is now passing that previously failed by design.");
                Log.Comment("It previously marked as known failure because of bug # 21563");
                Log.Comment("The Test owner needs to verify that the change was intentional and remove the known failure.");
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults Methods142_Test()
        {
            if (MethodsTestClass142.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods148_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" If execution of the method body of a void");
            Log.Comment(" method completes normally (that is, if control");
            Log.Comment(" flows off the end of the method body), the ");
            Log.Comment(" method simply returns to the caller.");
            if (MethodsTestClass148.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods149_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" When the return type of a method is not void,");
            Log.Comment(" each return statement in the method body must");
            Log.Comment(" specify an expression of a type that is implicitly");
            Log.Comment(" covertable to the return type.");
            if (MethodsTestClass149.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods150_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" When the return type of a method is not void,");
            Log.Comment(" each return statement in the method body must");
            Log.Comment(" specify an expression of a type that is implicitly");
            Log.Comment(" covertable to the return type.");
            if (MethodsTestClass150.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods154_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" When the return type of a method is not void,");
            Log.Comment(" each return statement in the method body must");
            Log.Comment(" specify an expression of a type that is implicitly");
            Log.Comment(" covertable to the return type.");
            if (MethodsTestClass154.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods159_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" When the return type of a method is not void,");
            Log.Comment(" each return statement in the method body must");
            Log.Comment(" specify an expression of a type that is implicitly");
            Log.Comment(" covertable to the return type.  Execution of the ");
            Log.Comment(" method body of a value returning method is required");
            Log.Comment(" to terminate in a return statement that specifies");
            Log.Comment(" an expression or in a throw statement that throws");
            Log.Comment(" an System.Exception.");
            if (MethodsTestClass159.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods160_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" When the return type of a method is not void,");
            Log.Comment(" each return statement in the method body must");
            Log.Comment(" specify an expression of a type that is implicitly");
            Log.Comment(" covertable to the return type.  Execution of the ");
            Log.Comment(" method body of a value returning method is required");
            Log.Comment(" to terminate in a return statement that specifies");
            Log.Comment(" an expression or in a throw statement that throws");
            Log.Comment(" an System.Exception.");
            if (MethodsTestClass160.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods161_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" When the return type of a method is not void,");
            Log.Comment(" each return statement in the method body must");
            Log.Comment(" specify an expression of a type that is implicitly");
            Log.Comment(" covertable to the return type.  Execution of the ");
            Log.Comment(" method body of a value returning method is required");
            Log.Comment(" to terminate in a return statement that specifies");
            Log.Comment(" an expression or in a throw statement that throws");
            Log.Comment(" an System.Exception.");
            if (MethodsTestClass161.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods163_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A parameter declared with a params modifier is");
            Log.Comment(" a params parameter. A params parameter must be");
            Log.Comment(" the last parameter in the formal parameter list,");
            Log.Comment(" and the type of a params parameter must be a ");
            Log.Comment(" single-dimension array type. For example, the");
            Log.Comment(" types int[] and int[][] can be used as the type of");
            Log.Comment(" a params parameter, but the type int[,] cannot be");
            Log.Comment(" used in this way.");
            if (MethodsTestClass163.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods164_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A parameter declared with a params modifier is");
            Log.Comment(" a params parameter. A params parameter must be");
            Log.Comment(" the last parameter in the formal parameter list,");
            Log.Comment(" and the type of a params parameter must be a ");
            Log.Comment(" single-dimension array type. For example, the");
            Log.Comment(" types int[] and int[][] can be used as the type of");
            Log.Comment(" a params parameter, but the type int[,] cannot be");
            Log.Comment(" used in this way.");
            if (MethodsTestClass164.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods169_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A params parameter enables a caller to supply values");
            Log.Comment(" in one of two ways.");
            Log.Comment(" The caller may specify an expression of a type that");
            Log.Comment(" is implicitly convertible to the formal parameter type.");
            Log.Comment(" In this case, the params parameter acts precisely like");
            Log.Comment(" a value parameter.");
            if (MethodsTestClass169.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods172_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A params parameter enables a caller to supply values");
            Log.Comment(" in one of two ways.");
            Log.Comment(" Alternately, the caller may specify zero or more expressions,");
            Log.Comment(" where the type of each expression is implicitly convertible");
            Log.Comment(" to the element type of the formal parameter type. In this case,");
            Log.Comment(" params parameter is initialized with an array fo the formal");
            Log.Comment(" parameter type that contains the value or values provided by");
            Log.Comment(" the caller.");
            if (MethodsTestClass172.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods173_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A params parameter enables a caller to supply values");
            Log.Comment(" in one of two ways.");
            Log.Comment(" Alternately, the caller may specify zero or more expressions,");
            Log.Comment(" where the type of each expression is implicitly convertible");
            Log.Comment(" to the element type of the formal parameter type. In this case,");
            Log.Comment(" params parameter is initialized with an array fo the formal");
            Log.Comment(" parameter type that contains the value or values provided by");
            Log.Comment(" the caller.");
            if (MethodsTestClass173.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods174_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A params parameter enables a caller to supply values");
            Log.Comment(" in one of two ways.");
            Log.Comment(" Alternately, the caller may specify zero or more expressions,");
            Log.Comment(" where the type of each expression is implicitly convertible");
            Log.Comment(" to the element type of the formal parameter type. In this case,");
            Log.Comment(" params parameter is initialized with an array fo the formal");
            Log.Comment(" parameter type that contains the value or values provided by");
            Log.Comment(" the caller.");
            if (MethodsTestClass174.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods175_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A params parameter enables a caller to supply values");
            Log.Comment(" in one of two ways.");
            Log.Comment(" Alternately, the caller may specify zero or more expressions,");
            Log.Comment(" where the type of each expression is implicitly convertible");
            Log.Comment(" to the element type of the formal parameter type. In this case,");
            Log.Comment(" params parameter is initialized with an array fo the formal");
            Log.Comment(" parameter type that contains the value or values provided by");
            Log.Comment(" the caller.");
            if (MethodsTestClass175.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods179_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A params parameter enables a caller to supply values");
            Log.Comment(" in one of two ways.");
            Log.Comment(" Alternately, the caller may specify zero or more expressions,");
            Log.Comment(" where the type of each expression is implicitly convertible");
            Log.Comment(" to the element type of the formal parameter type. In this case,");
            Log.Comment(" params parameter is initialized with an array fo the formal");
            Log.Comment(" parameter type that contains the value or values provided by");
            Log.Comment(" the caller.");
            if (MethodsTestClass179.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods180_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A params parameter enables a caller to supply values");
            Log.Comment(" in one of two ways.");
            Log.Comment(" Alternately, the caller may specify zero or more expressions,");
            Log.Comment(" where the type of each expression is implicitly convertible");
            Log.Comment(" to the element type of the formal parameter type. In this case,");
            Log.Comment(" params parameter is initialized with an array fo the formal");
            Log.Comment(" parameter type that contains the value or values provided by");
            Log.Comment(" the caller.");
            if (MethodsTestClass180.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods181_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A params parameter enables a caller to supply values");
            Log.Comment(" in one of two ways.");
            Log.Comment(" Alternately, the caller may specify zero or more expressions,");
            Log.Comment(" where the type of each expression is implicitly convertible");
            Log.Comment(" to the element type of the formal parameter type. In this case,");
            Log.Comment(" params parameter is initialized with an array fo the formal");
            Log.Comment(" parameter type that contains the value or values provided by");
            Log.Comment(" the caller.");
            if (MethodsTestClass181.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods182_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A params parameter enables a caller to supply values");
            Log.Comment(" in one of two ways.");
            Log.Comment(" Alternately, the caller may specify zero or more expressions,");
            Log.Comment(" where the type of each expression is implicitly convertible");
            Log.Comment(" to the element type of the formal parameter type. In this case,");
            Log.Comment(" params parameter is initialized with an array fo the formal");
            Log.Comment(" parameter type that contains the value or values provided by");
            Log.Comment(" the caller.");
            if (MethodsTestClass182.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods183_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A params parameter enables a caller to supply values");
            Log.Comment(" in one of two ways.");
            Log.Comment(" Alternately, the caller may specify zero or more expressions,");
            Log.Comment(" where the type of each expression is implicitly convertible");
            Log.Comment(" to the element type of the formal parameter type. In this case,");
            Log.Comment(" params parameter is initialized with an array fo the formal");
            Log.Comment(" parameter type that contains the value or values provided by");
            Log.Comment(" the caller.");
            if (MethodsTestClass183.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods184_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A params parameter enables a caller to supply values");
            Log.Comment(" in one of two ways.");
            Log.Comment(" Alternately, the caller may specify zero or more expressions,");
            Log.Comment(" where the type of each expression is implicitly convertible");
            Log.Comment(" to the element type of the formal parameter type. In this case,");
            Log.Comment(" params parameter is initialized with an array fo the formal");
            Log.Comment(" parameter type that contains the value or values provided by");
            Log.Comment(" the caller.");
            if (MethodsTestClass184.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods185_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A method is permitted to assign new values");
            Log.Comment(" to a params parameter. Such assignments only");
            Log.Comment(" affect the local storage location represented");
            Log.Comment(" by the params parameter.");
            if (MethodsTestClass185.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods186_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A params parameter can be passed along to another");
            Log.Comment(" params parameter.");
            if (MethodsTestClass186.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods187_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A params parameter can be passed along to another");
            Log.Comment(" params parameter.");
            if (MethodsTestClass187.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods188_Test()
        {
            Log.Comment("Testing explicit base method call to a child class whose parent implements");
            if (MethodsTestClass188.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods190_Test()
        {
            Log.Comment("Testing implicit base method calls to protected methods in parent class");
            if (MethodsTestClass190.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods191_Test()
        {
            Log.Comment("Testing implicit base method calls to internal methods in parent class");
            if (MethodsTestClass191.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods192_Test()
        {
            Log.Comment("Testing implicit base method calls to protected internal methods in parent class");
            if (MethodsTestClass192.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods193_Test()
        {
            Log.Comment("Testing implicit base method calls to private methods in parent class");
            if (MethodsTestClass193.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods194_Test()
        {
            Log.Comment("Testing implicit base method calls to public virtual methods in parent class");
            if (MethodsTestClass194.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods195_Test()
        {
            Log.Comment("Tests if a new method does not overwrite a virtual method in a base class");
            if (MethodsTestClass195.testMethod())
            {
                Log.Comment("This failure indicates a test is now passing that previously failed by design.");
                Log.Comment("It previously marked as known failure because of bug # 21563");
                Log.Comment("The Test owner needs to verify that the change was intentional and remove the known failure.");
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults Methods196_Test()
        {
            Log.Comment("Tests if a new method does overwrite an abstract method in a base class");
            if (MethodsTestClass196.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods197_Test()
        {
            Log.Comment("Tests the calling of an empty delegate");
            if (MethodsTestClass197.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods199_Test()
        {
            Log.Comment("Tests if a sealed method overwrites a virtual method in a base class");
            if (MethodsTestClass199.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods200_Test()
        {
            Log.Comment("Tests large number of assignments inside a public method");
            if (MethodsTestClass200.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods201_Test()
        {
            Log.Comment("Tests large number of assignments inside a public static method");
            if (MethodsTestClass201.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods204_Test()
        {
            Log.Comment("Tests a method with explicit, params signature");
            if (MethodsTestClass204.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods205_Test()
        {
            Log.Comment("Tests a method with a mixed explicit and params signature");
            if (MethodsTestClass205.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods206_Test()
        {
            Log.Comment("Tests method overloading between params and explicit signatures (static)");
            if (MethodsTestClass206.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods207_Test()
        {
            Log.Comment("Tests method overloading between params and explicit signatures");
            if (MethodsTestClass207.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods210_Test()
        {
            Log.Comment(" Section 10.5 If the declaration includes the sealed modifier, then the ");
            Log.Comment(" declaration must also include the override modifier.");
            if (MethodsTestClass210.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Methods223_Test()
        {
            Log.Comment(" Section 10.5 The ref and out parameters are part of a method's signature, but the params modifier is not.");
        if (MethodsTestClass223.testMethod())
        {
        return MFTestResults.Pass;
        }
        return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Methods224_Test()
        {
            Log.Comment(" Section 10.5 The ref and out parameters are part of a method's signature, but the params modifier is not.");
        if (MethodsTestClass224.testMethod())
        {
        return MFTestResults.Pass;
        }
        return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Methods229_Test()
        {
            Log.Comment(" error CS0114: 'function1' hides inherited member 'function2'.");
            Log.Comment(" To make the current method override that implementation, add ");
            Log.Comment(" the override keyword. Otherwise add the new keyword.");
        if (MethodsTestClass229.testMethod())
        {
        return MFTestResults.Pass;
        }
        return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Methods230_Test()
        {
            Log.Comment(" error CS0114: 'function1' hides inherited member 'function2'.");
            Log.Comment(" To make the current method override that implementation, add ");
            Log.Comment(" the override keyword. Otherwise add the new keyword.");
        if (MethodsTestClass230.testMethod())
        {
        return MFTestResults.Pass;
        }
        return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Methods231_Test()
        {
            Log.Comment(" error CS0114: 'function1' hides inherited member 'function2'.");
            Log.Comment(" To make the current method override that implementation, add ");
            Log.Comment(" the override keyword. Otherwise add the new keyword.");
        if (MethodsTestClass231.testMethod())
        {
        return MFTestResults.Pass;
        }
        return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Methods232_Test()
        {
            Log.Comment(" error CS0114: 'function1' hides inherited member 'function2'.");
            Log.Comment(" To make the current method override that implementation, add ");
            Log.Comment(" the override keyword. Otherwise add the new keyword.");
            if (MethodsTestClass232.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Methods233_Test()
        {
            Log.Comment(" Section 10.5");
            Log.Comment(" A method-declaration may include set of attributes,");
            Log.Comment(" a new modifier, one of four access modifiers,");
            Log.Comment(" one of the static, virtual, override, or abstract ");
            Log.Comment(" modifiers, and an extern modifier.");
            if (MethodsTestClass233.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }


        public class MethodsTestClass_Base1
        {
            public static bool testMethod()
            {
                return false;
            }
        }

        public class MethodsTestClass1 : MethodsTestClass_Base1
        {
            //new modifier
            new public int MyMeth()
            {
                return 2;
            }
            public static bool testMethod()
            {
                MethodsTestClass1 test = new MethodsTestClass1();
                if (test.MyMeth() == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class MethodsTestClass2
        {
            //new modifier
            new public int MyMeth()
            {
                return 2;
            }
            public static bool testMethod()
            {
                MethodsTestClass2 test = new MethodsTestClass2();
                if (test.MyMeth() == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class MethodsTestClass3
        {
            //public modifier
            public int MyMeth()
            {
                return 2;
            }
            public static bool testMethod()
            {
                MethodsTestClass3 test = new MethodsTestClass3();
                if (test.MyMeth() == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class MethodsTestClass_Base4
        {
            //protected modifier
            protected int MyMeth()
            {
                return 2;
            }
        }
        public class MethodsTestClass4 : MethodsTestClass_Base4
        {
            public static bool testMethod()
            {
                MethodsTestClass4 test = new MethodsTestClass4();
                if (test.MyMeth() == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class MethodsTestClass6
        {
            //internal modifier
            internal int MyMeth()
            {
                return 2;
            }
            public static bool testMethod()
            {
                MethodsTestClass6 test = new MethodsTestClass6();
                if (test.MyMeth() == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class MethodsTestClass7
        {
            //private modifier
            private int MyMeth()
            {
                return 2;
            }
            public static bool testMethod()
            {
                MethodsTestClass7 test = new MethodsTestClass7();
                if (test.MyMeth() == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class MethodsTestClass9
        {
            //static modifier
            static int MyMeth()
            {
                return 2;
            }
            public static bool testMethod()
            {
                if (MyMeth() == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class MethodsTestClass_Base10 {
	        //virtual modifier
	        public virtual int MyMeth(){
		        return 1;
	        }
        }

        public class MethodsTestClass10 : MethodsTestClass_Base10 {	
	        //override modifier
	        public override int MyMeth(){
		        return 2;
	        }	
	        public static bool testMethod() {
		        MethodsTestClass_Base10 test = new MethodsTestClass10();
		        if (test.MyMeth() == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass_Base11 {
	        //virtual modifier
	        public virtual int MyMeth(){
		        return 1;
	        }		
        }
        public class MethodsTestClass11 : MethodsTestClass_Base11 {	
	        //new modifier
	        new int MyMeth(){
		        return 2;
	        }	
	        public static bool testMethod() {
		        MethodsTestClass_Base11 test = new MethodsTestClass11();
		        if (test.MyMeth() == 1) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        abstract class MethodsTestClass_Base13 {
	        //abstract modifier
	        public abstract int MyMeth();
        }
        class MethodsTestClass13 : MethodsTestClass_Base13 {	
	        public override int MyMeth() {
		        return 2;
	        }
	        public static bool testMethod() {
		        MethodsTestClass_Base13 test = new MethodsTestClass13();
		        if (test.MyMeth() == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class Test {
	        //extern modifier
	        extern public int MyMeth();
        }
        public class MethodsTestClass17 {	
	        public static bool testMethod() {
		        return true;
	        }
        }

        public class MethodsTestClass19 {	
	        int intI = 1;
	        //void return type
	        void MyMeth(){
		        intI = 2;
	        }
	        public static bool testMethod() {
		        MethodsTestClass19 test = new MethodsTestClass19();
		        test.MyMeth();
		        if (test.intI == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass20 {	
	        int intI = 1;
	        //void return type
	        void MyMeth(){
		        intI = 2;
		        return;
	        }
	        public static bool testMethod() {
		        MethodsTestClass20 test = new MethodsTestClass20();
		        test.MyMeth();
		        if (test.intI == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass22 {	
	        //simple return type
	        int MyMeth(){
		        return 2;
	        }
	        public static bool testMethod() {
		        MethodsTestClass22 test = new MethodsTestClass22();
		        if (test.MyMeth() == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass23 {	
	        //string return type
	        string MyMeth(){
		        return "MyMessage";
	        }
	        public static bool testMethod() {
		        MethodsTestClass23 test = new MethodsTestClass23();
		        if (test.MyMeth().Equals("MyMessage")) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        struct MyStruct {
	        public MyStruct(int intI) {
		        intTest = intI;
	        }
	        public int intTest;
        }
        public class MethodsTestClass24 {	
	        //struct return type
	        MyStruct MyMeth(){
		        return new MyStruct(3);
	        }
	        public static bool testMethod() {
		        MethodsTestClass24 test = new MethodsTestClass24();
		        if (test.MyMeth().intTest == 3) {
			        return true;
		        }
		        else {
			        Microsoft.SPOT.Debug.Print(test.MyMeth().intTest.ToString());
			        return false;
		        }
	        }
        }
        enum MethodsTestClass26_Enum {a = 1, b = 2}
        public class MethodsTestClass25 {	
	        //enum return type
	        MethodsTestClass26_Enum MyMeth(){
		        return MethodsTestClass26_Enum.a;
	        }
	        public static bool testMethod() {
		        MethodsTestClass25 test = new MethodsTestClass25();
		        if (test.MyMeth() == MethodsTestClass26_Enum.a) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        class MethodsTestClass26_C {
	        public MethodsTestClass26_C(int intI) {
		        intTest = intI;
	        }
	        public int intTest;
        }
        public class MethodsTestClass26 {	
	        //class return type
	        MethodsTestClass26_C MyMeth(){
		        return new MethodsTestClass26_C(3);
	        }
	        public static bool testMethod() {
		        MethodsTestClass26 test = new MethodsTestClass26();
		        if (test.MyMeth().intTest == 3) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public interface MethodsTestClass29_Interface
        {
            int RetInt();
        }

        public class MethodsTestClass29 : MethodsTestClass29_Interface
        {

            int MethodsTestClass29_Interface.RetInt()
            {
                return 2;
            }

            public static bool testMethod()
            {
                try
                {
                    MethodsTestClass29_Interface test = new MethodsTestClass29();
                    if (test.RetInt() == 2)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }
        public interface MethodsTestClass30_Interface {
	        int RetInt();
        }
        public interface MethodsTestClass30_Interface2 {
	        int RetInt();
        }
        public class MethodsTestClass30 : MethodsTestClass30_Interface, MethodsTestClass30_Interface2 {	
	        int MethodsTestClass30_Interface.RetInt(){
		        return 2;
	        }
	        int MethodsTestClass30_Interface2.RetInt(){
		        return 3;
	        }
            public static bool testMethod()
            {
                try
                {
                    MethodsTestClass30_Interface test1 = new MethodsTestClass30();
                    MethodsTestClass30_Interface2 test2 = new MethodsTestClass30();
                    if ((test1.RetInt() == 2) && (test2.RetInt() == 3))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        public class MethodsTestClass33 {	
	        //1 parameter
	        int RetInt(int MyInt){
		        return MyInt;
	        }
	        public static bool testMethod() {
		        MethodsTestClass33 test = new MethodsTestClass33();
		        if (test.RetInt(3) == 3) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass34 {	
	        //2 parameters
	        int RetInt(int MyInt1, int MyInt2){
		        return (MyInt1 + MyInt2);
	        }
	        public static bool testMethod() {
		        MethodsTestClass34 test = new MethodsTestClass34();
		        if (test.RetInt(3,4) == 7) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass35 {	
	        //multiple parameters
	        int RetInt(int MyInt1, int MyInt2, int MyInt3, int MyInt4, int MyInt5, int MyInt6, int MyInt7, int MyInt8, int MyInt9, int MyInt10 ){
		        return (MyInt1 + MyInt2 + MyInt3 + MyInt4 + MyInt5 + MyInt6 + MyInt7 + MyInt8 + MyInt9 + MyInt10);
	        }
	        public static bool testMethod() {
		        MethodsTestClass35 test = new MethodsTestClass35();
		        if (test.RetInt(2,2,2,2,2,2,2,2,2,2) == 20) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass56 {	
	        public int intI = 1;
	        public int TestMeth(int intI) {
		        return intI;
	        }
	        public static bool testMethod() {
		        MethodsTestClass56 test = new MethodsTestClass56();
		        if (test.TestMeth(2) == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }		
	        }
        }
        public class MethodsTestClass57 {	
	        public int intI = 1;
	        public int TestMeth() {
		        int intI = 2;
		        return intI;
	        }
	        public static bool testMethod() {
		        MethodsTestClass57 test = new MethodsTestClass57();
		        if (test.TestMeth() == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }		
	        }
        }
        public class MethodsTestClass58 {	
	        public int TestMeth(int intI) {
		        return intI;
	        }
	        public static bool testMethod() {	
		        short s = 2;
		        MethodsTestClass58 test = new MethodsTestClass58();
		        if (test.TestMeth(s) == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass_Base59 {
	        public int intI = 2;
        }
        public class MyDerived : MethodsTestClass_Base59 {}
        public class MethodsTestClass59 {	
	        public int TestMeth(MethodsTestClass_Base59 tc) {
		        return tc.intI;
	        }
	        public static bool testMethod() {	
		        MethodsTestClass59 test = new MethodsTestClass59();
		        MyDerived md = new MyDerived();
		        if (test.TestMeth(md) == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public interface MethodsTestClass_Interface60 {
	        int intRet();
        }
        public class MethodsTestClass_Derived60 : MethodsTestClass_Interface60
        {
	        public int intRet() {
		        return 2;
	        }
        }
        public class MethodsTestClass60 {	
	        public int TestMeth(MethodsTestClass_Interface60 ti) {
		        return ti.intRet();
	        }
	        public static bool testMethod() {	
		        MethodsTestClass60 test = new MethodsTestClass60();
                MethodsTestClass_Derived60 md = new MethodsTestClass_Derived60();
		        if (test.TestMeth(md) == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class C1 {
	        public int intI = 2;	
        }
        public class C2 {
	        public static implicit operator C1(C2 MyC) {
		        return new C1();
	        }
        }
        public class MethodsTestClass61 {	
	        public int TestMeth(C1 c) {
		        return c.intI;
	        }
	        public static bool testMethod() {	
		        MethodsTestClass61 test = new MethodsTestClass61();
		        C2 MyC2 = new C2();
		        if (test.TestMeth(MyC2) == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        
        public class MethodsTestClass66 {	
	        int int1;
	        public void TestMeth(int intI) {
		        intI = 3;
		        int1 = intI;
	        }
	        public static bool testMethod() {	
		        int intJ = 2;
		        MethodsTestClass66 test = new MethodsTestClass66();
		        test.TestMeth(intJ);
		        if ((test.int1 == 3) && (intJ == 2)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass67 {	
	        string strS;
	        public void TestMeth(string s) {
		        s = "string1";
		        strS = s;
	        }
	        public static bool testMethod() {	
		        string strtest = "string0";
		        MethodsTestClass67 test = new MethodsTestClass67();
		        test.TestMeth(strtest);
		        if ((test.strS.Equals("string1")) && (strtest.Equals("string0"))) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass_Sub68 {
	        public int testint;
	        public MethodsTestClass_Sub68(int intI) {
		        testint = intI;
	        }
        }
        public class MethodsTestClass68 {	
	        MethodsTestClass_Sub68 tc;
	        public void TestMeth(MethodsTestClass_Sub68 t) {
		        t = new MethodsTestClass_Sub68(3);
		        tc = t;		
	        }
	        public static bool testMethod() {	
		        MethodsTestClass_Sub68 tc2 = new MethodsTestClass_Sub68(2);
		        MethodsTestClass68 test = new MethodsTestClass68();
		        test.TestMeth(tc2);
		        if ((tc2.testint == 2) && (test.tc.testint == 3)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass69 {	
	        int int1;
	        public void TestMeth(ref int intI) {
		        intI = 3;
		        int1 = intI;
	        }
	        public static bool testMethod() {	
		        int intJ = 2;
		        MethodsTestClass69 test = new MethodsTestClass69();
		        test.TestMeth(ref intJ);
		        if ((test.int1 == 3) && (intJ == 3)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass70 {	
	        string strS;
	        public void TestMeth(ref string s) {
		        s = "string1";
		        strS = s;
	        }
	        public static bool testMethod() {	
		        string strtest = "string0";
		        MethodsTestClass70 test = new MethodsTestClass70();
		        test.TestMeth(ref strtest);
		        if ((test.strS.Equals("string1")) && (strtest.Equals("string1"))) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass_Sub71 {
	        public int testint;
	        public MethodsTestClass_Sub71(int intI) {
		        testint = intI;
	        }
        }
        public class MethodsTestClass71 {	
	        MethodsTestClass_Sub71 tc;
	        public void TestMeth(ref MethodsTestClass_Sub71 t) {
		        t = new MethodsTestClass_Sub71(3);
		        tc = t;		
	        }
	        public static bool testMethod() {	
		        MethodsTestClass_Sub71 tc2 = new MethodsTestClass_Sub71(2);
		        MethodsTestClass71 test = new MethodsTestClass71();
		        test.TestMeth(ref tc2);
		        if ((tc2.testint == 3) && (test.tc.testint == 3)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        class MethodsTestClass_Base75 {}

        class MethodsTestClass_Derived75 : MethodsTestClass_Base75 { }
        public class MethodsTestClass75 {	
	        int intI;
	        public int TestMeth(ref int I1) {
		        return I1;	
	        }
	        public static bool testMethod() {	
		        MethodsTestClass75 test = new MethodsTestClass75();
		        if (test.TestMeth(ref test.intI) == 0) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }


        public class MethodsTestClass78 {	
	        int int1;
	        public void TestMeth(out int intI) {
		        intI = 3;
		        int1 = intI;
	        }
	        public static bool testMethod() {	
		        int intJ = 2;
		        MethodsTestClass78 test = new MethodsTestClass78();
		        test.TestMeth(out intJ);
		        if ((test.int1 == 3) && (intJ == 3)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass79 {	
	        string strS;
	        public void TestMeth(out string s) {
		        s = "string1";
		        strS = s;
	        }
	        public static bool testMethod() {	
		        string strtest = "string0";
		        MethodsTestClass79 test = new MethodsTestClass79();
		        test.TestMeth(out strtest);
		        if ((test.strS.Equals("string1")) && (strtest.Equals("string1"))) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass_Sub80 {
	        public int testint;
	        public MethodsTestClass_Sub80(int intI) {
		        testint = intI;
	        }
        }
        public class MethodsTestClass80 {	
	        MethodsTestClass_Sub80 tc;
	        public void TestMeth(out MethodsTestClass_Sub80 t) {
		        t = new MethodsTestClass_Sub80(3);
		        tc = t;		
	        }
	        public static bool testMethod() {	
		        MethodsTestClass_Sub80 tc2 = new MethodsTestClass_Sub80(2);
		        MethodsTestClass80 test = new MethodsTestClass80();
		        test.TestMeth(out tc2);
		        if ((tc2.testint == 3) && (test.tc.testint == 3)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass84 {	
	        public void TestMeth(out int intI) {
		        intI = 2;
	        }
	        public static bool testMethod() {	
		        int intJ;
		        MethodsTestClass84 test = new MethodsTestClass84();
		        test.TestMeth(out intJ);
		        if (intJ == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass_Sub85 {
	        public int intI;
	        public MethodsTestClass_Sub85(int intJ) {
		        intI = intJ;
	        }
        }
        public class MethodsTestClass85 {	
	        public void TestMeth(out MethodsTestClass_Sub85 tc) {
		        tc = new MethodsTestClass_Sub85(2);
	        }
	        public static bool testMethod() {	
		        MethodsTestClass_Sub85 tctest;
		        MethodsTestClass85 test = new MethodsTestClass85();
		        test.TestMeth(out tctest);
		        if (tctest.intI == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        
        public class MethodsTestClass92 {	
	        public void TestMeth(out int intI, out int intJ) {
		        intI = 2;
		        intJ = 3;
	        }
	        public static bool testMethod() {	
		        MethodsTestClass92 test = new MethodsTestClass92();
		        int i1, i2;
		        test.TestMeth(out i1, out i2);
		        if ((i1 == 2) && (i2 == 3)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass93 {	
	        public void TestMeth(ref int intI, out int intJ) {
		        intJ = 3;
	        }
	        public static bool testMethod() {	
		        MethodsTestClass93 test = new MethodsTestClass93();
		        int i1 = 2;
		        int i2;
		        test.TestMeth(ref i1, out i2);
		        if ((i1 == 2) && (i2 == 3)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass94 {	
	        public void TestMeth(int intI, out int intJ) {
		        intI = 4;
		        intJ = 3;
	        }
	        public static bool testMethod() {	
		        MethodsTestClass94 test = new MethodsTestClass94();
		        int i1 = 2;
		        int i2;
		        test.TestMeth(i1, out i2);
		        if ((i1 == 2) && (i2 == 3)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass95 {	
	        public void TestMeth(int intI, ref int intJ) {
		        intI = 4;
		        intJ = 3;
	        }
	        public static bool testMethod() {	
		        MethodsTestClass95 test = new MethodsTestClass95();
		        int i1 = 2;
		        int i2 = 5;
		        test.TestMeth(i1, ref i2);
		        if ((i1 == 2) && (i2 == 3)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass103_Sub {
	        public int Test() {
		        return 2;
	        }
        }
        public class MethodsTestClass103 : MethodsTestClass103_Sub {	
	        public int Test() {
		        return 3;
	        }
	        public static bool testMethod() {	
		        MethodsTestClass103_Sub TC = new MethodsTestClass103();
		        if (TC.Test() == 2) {
			        return true;
		        }		
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass_Sub104 {
	        public virtual int Test() {
		        return 102;
	        }
        }
        public class MethodsTestClass104 : MethodsTestClass_Sub104 {	
	        public new int Test() {
		        return 103;
	        }
	        public static bool testMethod() {	
		        MethodsTestClass_Sub104 TC = new MethodsTestClass104();
		        if (TC.Test() == 102) {
			        return true;
		        }		
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass_Sub105 {
	        public int Test() {
		        return 102;
	        }
        }
        public class MethodsTestClass105 : MethodsTestClass_Sub105 {	
	        public new virtual int Test() {
		        return 103;
	        }
	        public static bool testMethod() {	
		        MethodsTestClass_Sub105 TC = new MethodsTestClass105();
		        if (TC.Test() == 102) {
			        return true;
		        }		
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass106 {	
	        public virtual int Test() {
		        return 103;
	        }
	        public static bool testMethod() {	
		        MethodsTestClass106 TC = new MethodsTestClass106();
		        if (TC.Test() == 103) {
			        return true;
		        }		
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass_Sub107 {
	        public virtual int Test() {
		        return 102;
	        }
        }
        public class MethodsTestClass107 : MethodsTestClass_Sub107 {	
	        public new virtual int Test() {
		        return 103;
	        }
	        public static bool testMethod() {	
		        MethodsTestClass_Sub107 TC = new MethodsTestClass107();
		        if (TC.Test() == 102) {
			        return true;
		        }		
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass_Sub108 {
	        public virtual int Test() {
		        return 102;
	        }
        }
        public class MethodsTestClass108 : MethodsTestClass_Sub108 {	
	        public override int Test() {
		        return 103;
	        }
	        public static bool testMethod() {	
		        MethodsTestClass_Sub108 TC = new MethodsTestClass108();
		        if (TC.Test() == 103) {
			        return true;
		        }		
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass_Sub109 {
	        public virtual int Test() {
		        return 102;
	        }
        }
        public class MethodsTestClass_Sub109_2 : MethodsTestClass_Sub109 {
	        public override int Test() {
		        return 103;
	        }
        }
        public class MethodsTestClass109 : MethodsTestClass_Sub109_2 {	
	        public override int Test() {
		        return 104;
	        }
	        public static bool testMethod() {	
		        MethodsTestClass_Sub109 TC = new MethodsTestClass109();
		        MethodsTestClass_Sub109_2 TC102 = new MethodsTestClass109();
		        if ((TC.Test() == 104) && (TC.Test() == 104)) {
			        return true;
		        }		
		        else {
			        return false;
		        }
	        }
        }

                //Compiled Test Cases 
        public class MethodsTestClass_Sub110 {
	        public virtual int Test() {
		        return 2;
	        }
        }
        public class MethodsTestClass_Sub1102 : MethodsTestClass_Sub110 {
	        new public virtual int Test() {
		        return 3;
	        }
        }
        public class MethodsTestClass110 : MethodsTestClass_Sub1102 {	
	        public static bool testMethod() {	
		        MethodsTestClass_Sub110 TC = new MethodsTestClass110();
		        MethodsTestClass_Sub1102 TC2 = new MethodsTestClass110();
		        if ((TC.Test() == 2) && (TC2.Test() == 3)) {
			        return true;
		        }		
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass_Sub111 {
	        public virtual int Test() {
		        return 2;
	        }
        }
        public class MethodsTestClass_Sub1112 : MethodsTestClass_Sub111 {
	        public override int Test() {
		        return 3;
	        }
        }
        public class MethodsTestClass111 : MethodsTestClass_Sub1112 {	
	        public static bool testMethod() {	
		        MethodsTestClass_Sub111 TC = new MethodsTestClass111();
		        MethodsTestClass_Sub1112 TC2 = new MethodsTestClass111();
		        if ((TC.Test() == 3) && (TC.Test() == 3)) {
			        return true;
		        }		
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass_Sub112A {
	        public virtual int Test() {return 1;}
        }
        public class MethodsTestClass_Sub112B : MethodsTestClass_Sub112A {
	        public override int Test() {return 2;}
        }
        public class MethodsTestClass_Sub112C : MethodsTestClass_Sub112B {
	        public new virtual int Test() {return 3;}
        }
        public class MethodsTestClass_Sub112D : MethodsTestClass_Sub112C {
	        public override int Test() {return 4;}
        }
        public class MethodsTestClass112 : MethodsTestClass_Sub112D {	
	        public static bool testMethod() {	
		        MethodsTestClass_Sub112D d = new MethodsTestClass_Sub112D();
		        MethodsTestClass_Sub112A a = d;
		        MethodsTestClass_Sub112B b = d;
		        MethodsTestClass_Sub112C c = d;
		        if((d.Test() == 4) && (c.Test() == 4) && (b.Test() == 2) && (a.Test() == 2)) {
			        return true;
		        }	
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass_Base116 {
	        public virtual void Test() {}
        }
        public abstract class MethodsTestClass_Base116_2 : MethodsTestClass_Base116 {
	        public override abstract void Test();
        }
        public class MethodsTestClass116 : MethodsTestClass_Base116_2 {
	        public override void Test() {}
	        public static bool testMethod() {
		        MethodsTestClass116 tc = new MethodsTestClass116();
		        tc.Test();
		        return true;
	        }
        }
        public class MethodsTestClass_Base117 {
	        public virtual int Test() {
		        return 1;
	        }
        }
        public class MethodsTestClass117 : MethodsTestClass_Base117 {
	        public override int Test() {
		        return 2;
	        }
	        public static bool testMethod() {
		        MethodsTestClass_Base117 BC = new MethodsTestClass117();
		        if (BC.Test() == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass_Base118 {
	        public int GetVal() {
		        return Test();
	        }	
	        protected virtual int Test() {
		        return 1;
	        }
        }
        public class MethodsTestClass118 : MethodsTestClass_Base118 {
	        protected override int Test() {
		        return 2;
	        }
	        public static bool testMethod() {
		        MethodsTestClass_Base118 BC = new MethodsTestClass118();
		        if (BC.GetVal() == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass_Base119 {
	        public int GetVal() {
		        return Test();
	        }	
	        internal virtual int Test() {
		        return 1;
	        }
        }
        public class MethodsTestClass119 : MethodsTestClass_Base119 {
	        internal override int Test() {
		        return 2;
	        }
	        public static bool testMethod() {
		        MethodsTestClass_Base119 BC = new MethodsTestClass119();
		        if (BC.GetVal() == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass_Base120 {
	        public int GetVal() {
		        return Test();
	        }	
	        protected internal virtual int Test() {
		        return 1;
	        }
        }
        public class MethodsTestClass120 : MethodsTestClass_Base120 {
	        protected internal override int Test() {
		        return 2;
	        }
	        public static bool testMethod() {
		        MethodsTestClass_Base120 BC = new MethodsTestClass120();
		        if (BC.GetVal() == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass_Base121 {
	        public virtual int Test() {
		        return 1;
	        }
        }
        public class MethodsTestClass_Base121_2 : MethodsTestClass_Base121 {
	        new private int Test() {
		        return 2;
	        }
        }
        public class MethodsTestClass121 : MethodsTestClass_Base121_2 {
	        public override int Test() {
		        return 3;
	        }
	        public static bool testMethod() {
		        MethodsTestClass_Base121 BC = new MethodsTestClass121();
		        if (BC.Test() == 3) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public abstract class MethodsTestClass_Base124 {
	        public abstract int Test();
        }
        public class MethodsTestClass124 : MethodsTestClass_Base124 {
	        public override int Test() {
		        return 1;
	        }
	        public static bool testMethod() {
		        MethodsTestClass_Base124 BC = new MethodsTestClass124();
		        if (BC.Test() == 1) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass_Base125 {
	        public virtual int Test() {
		        return 1;
	        }
        }
        public class MethodsTestClass_Base125_2 : MethodsTestClass_Base125 {
	        public override int Test() {
		        return 2;
	        }
        }
        public class MethodsTestClass125 : MethodsTestClass_Base125_2 {
	        public override int Test() {
		        return 3;
	        }
	        public static bool testMethod() {
		        MethodsTestClass_Base125 BC = new MethodsTestClass125();
		        MethodsTestClass_Base125_2 BC2 = new MethodsTestClass125();
		        if ((BC.Test() == 3) && (BC2.Test() ==3)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass_Base132
        {
            public virtual int Test()
            {
                return 1;
            }
        }

        public class MethodsTestClass132 : MethodsTestClass_Base132 {
	        public override int Test() {
		        return 2;
	        }
	        public int RetVal() {
		        return base.Test();
	        }
	        public static bool testMethod() {
		        MethodsTestClass132 MC = new MethodsTestClass132();
		        if ((MC.Test() == 2) && (MC.RetVal() == 1)) {
			        return true;
		        }
		        else {
			        return false;
		        }		
	        }
        }

        public class MethodsTestClass_Base133 {
	        public virtual int Test() {
		        return 1;
	        }
        }
        public class MethodsTestClass133 : MethodsTestClass_Base133 {
	        public override int Test() {
		        return 2;
	        }
	        public int RetVal() {
		        return ((MethodsTestClass_Base133)this).Test();
	        }
	        public static bool testMethod() {
		        MethodsTestClass133 MC = new MethodsTestClass133();
		        if ((MC.Test() == 2) && (MC.RetVal() == 2)) {
			        return true;
		        }
		        else {
			        return false;
		        }		
	        }
        }
        public class MethodsTestClass_Base134 {
	        public virtual int Test() {
		        return 1;
	        }
        }
        public class MethodsTestClass134 : MethodsTestClass_Base134 {
	        public int Test() {
		        return 2;
	        }
	        public static bool testMethod() {
		        MethodsTestClass_Base134 BC = new MethodsTestClass134();
		        if (BC.Test() == 1) {
			        return true;
		        }
		        else {
			        return false;
		        }		
	        }
        }

        public abstract class MethodsTestClass_Base142 {
	        public abstract int Test();
        }
        public class MethodsTestClass142 : MethodsTestClass_Base142 {
	        public override int Test() {
		        return 2;
	        }
	        public static bool testMethod() {
		        MethodsTestClass_Base142 BC = new MethodsTestClass142();
		        if (BC.Test() == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass148 {
	        int intI = 2;
	        public void Test() {
		        return;
		        intI = 3;
	        }
	        public static bool testMethod() {
		        MethodsTestClass148 test = new MethodsTestClass148();
		        test.Test();
		        if (test.intI == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass149 {
	        public long RetVal() {
		        int ret = 2;
		        return ret;
	        }
	        public static bool testMethod() {
		        MethodsTestClass149 test = new MethodsTestClass149();
		        if (test.RetVal() == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass_Sub150 {
	        public int IntI = 2;
	        public static implicit operator int(MethodsTestClass_Sub150 t) {
		        return t.IntI;
	        }	
        }
        public class MethodsTestClass150 {
	        public long RetVal() {
		        MethodsTestClass_Sub150 tc = new MethodsTestClass_Sub150();
		        return tc;
	        }
	        public static bool testMethod() {
		        MethodsTestClass150 test = new MethodsTestClass150();
		        if (test.RetVal() == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        public class MethodsTestClass154 {
	        public long RetVal(bool b) {

		        long ret = 2;

		        if (b == true) {
			        return ret;
		        }
		        else {
			        return (ret + 1);
		        }
	        }
	        public static bool testMethod() {
		        MethodsTestClass154 test = new MethodsTestClass154();
		        if ((test.RetVal(true) == 2) && (test.RetVal(false) == 3)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }


        public class MethodsTestClass159 {
	        public long RetVal() {
		        throw new System.Exception();
	        }
	        public static bool testMethod() {
		        bool val = false;
		        MethodsTestClass159 test = new MethodsTestClass159();
		        try {
			        test.RetVal();
		        }
		        catch {
			        val = true;
		        }
		        return val;
	        }
        }

        public class MethodsTestClass160 {
	        public long RetVal(bool b) {
         
		        long ret = 2;
         
		        if (b == true) {
			        return ret;
		        }
		        else {
			        throw new System.Exception();
		        }
	        }
	        public static bool testMethod() {
		        bool val = false;
		        MethodsTestClass160 test = new MethodsTestClass160();
		        if (test.RetVal(true) != 2) {
			        return false;
		        }
		        try {
			        test.RetVal(false);		
		        }
		        catch (System.Exception e) {
			        val = true;
		        }
		        return val;
	        }
        }

        public class MethodsTestClass161 {
	        public long RetVal(bool b) {
         
		        long ret = 2;
         
		        if (b == true) {
			        throw new System.Exception();
		        }
		        else {
			        return ret;
		        }
	        }
	        public static bool testMethod() {
		        bool val = false;
		        MethodsTestClass161 test = new MethodsTestClass161();
		        if (test.RetVal(false) != 2) {
			        return false;
		        }
		        try {
			        test.RetVal(true);		
		        }
		        catch (System.Exception e) {
			        val = true;
		        }
		        return val;
	        }
        }

        public class MethodsTestClass163 {
	        public int MyMeth1(params int[] values) {
		        return values[0] + values[1] + values [2];
	        }
	        public static int MyMeth2(params int[] values) {
		        return values[0] + values[1] + values [2];
	        }
	        public static bool testMethod() {
		        int intI = 1;
		        int intJ = 2;
		        int intK = 3;
		        MethodsTestClass163 mc = new MethodsTestClass163();
		        if ((mc.MyMeth1(intI, intJ, intK) == 6) && (MyMeth2(intI, intJ, intK) == 6)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass164 {
	        public int MyMeth1(params int[][] values) {
		        return values[0][0] + values[1][0] + values [2][0];
	        }
	        public static int MyMeth2(params int[][] values) {
		        return values[0][0] + values[1][0] + values [2][0];
	        }
	        public static bool testMethod() {
		        int[] intI = {1};
		        int[] intJ = {2};
		        int[] intK = {3};
		        MethodsTestClass164 mc = new MethodsTestClass164();
		        if ((mc.MyMeth1(intI, intJ, intK) == 6) && (MyMeth2(intI, intJ, intK) == 6)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass169 {
	        public int MyMeth1(params int[] values) {
		        return values[0] + values[1] + values [2];
	        }
	        public static int MyMeth2(params int[] values) {
		        return values[0] + values[1] + values [2];
	        }
	        public static bool testMethod() {
		        MethodsTestClass169 mc = new MethodsTestClass169();
		        if ((mc.MyMeth1(new int[]{1,2,3}) == 6) && (MyMeth2(new int[]{1,2,3}) == 6)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MyType {
	        public int intI = 2;
        }

        public class MethodsTestClass172 {
	        public int MyMeth1(params int[] values) {
		        return 2;
	        }
	        public static int MyMeth2(params int[] values) {
		        return 2;
	        }
	        public static bool testMethod() {
		        MethodsTestClass172 mc = new MethodsTestClass172();
		        if ((mc.MyMeth1() == 2) && (MyMeth2() == 2)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass173 {
	        public int MyMeth1(params int[] values) {
		        return values[0];
	        }
	        public static int MyMeth2(params int[] values) {
		        return values[0];
	        }
	        public static bool testMethod() {
		        int intI = 2;
		        MethodsTestClass173 mc = new MethodsTestClass173();
		        if ((mc.MyMeth1(intI) == 2) && (MyMeth2(intI) == 2)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass174 {
	        public int MyMeth1(params int[] values) {
		        return values[0];
	        }
	        public static int MyMeth2(params int[] values) {
		        return values[0];
	        }
	        public static bool testMethod() {
		        int i = 1;
		        short s = 2;
		        byte b = 3;
		        MethodsTestClass174 mc = new MethodsTestClass174();
		        if ((mc.MyMeth1(i,s,b) == 1) && (MyMeth2(i,s,b) == 1)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass175 {
	        public int MyMeth1(params int[] values) {
		        return values[0];
	        }
	        public static int MyMeth2(params int[] values) {
		        return values[0];
	        }
	        public static bool testMethod() {
		        short s = 2;
		        MethodsTestClass175 mc = new MethodsTestClass175();
		        if ((mc.MyMeth1(s) == 2) && (MyMeth2(s) == 2)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass179 {
	        public int MyMeth1(int intI, params int[] values) {
		        return values[0] + values[1] + intI;
	        }
	        public static int MyMeth2(int intI, params int[] values) {
		        return values[0] + values[1] + intI;
	        }
	        public static bool testMethod() {
		        int intI = 1;
		        int intJ = 2;
		        int intK = 3;
		        MethodsTestClass179 mc = new MethodsTestClass179();
		        if ((mc.MyMeth1(intI, intJ, intK) == 6) && (MyMeth2(intI, intJ, intK) == 6)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass180 {
	        public int MyMeth1(int intI, int intJ, int intK) {
		        return 1;
	        }
	        public int MyMeth1(params int[] values) {
		        return 2;
	        }
	        public static int MyMeth2(int intI, int intJ, int intK) {
		        return 1;
	        }
	        public static int MyMeth2(params int[] values) {
		        return 2;
	        }
	        public static bool testMethod() {
		        MethodsTestClass180 mc = new MethodsTestClass180();
		        if ((mc.MyMeth1(1,2) == 2) && (mc.MyMeth1(1,2,3) == 1) && (MyMeth2(1,2) == 2) && (MyMeth2(1,2,3) == 1)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass181 {
	        public int MyMeth1(int intI, int intJ, int intK) {
		        return 1;
	        }
	        public int MyMeth1(params int[] values) {
		        return 2;
	        }
	        public static int MyMeth2(int intI, int intJ, int intK) {
		        return 1;
	        }
	        public static int MyMeth2(params int[] values) {
		        return 2;
	        }
	        public static bool testMethod() {
		        MethodsTestClass181 mc = new MethodsTestClass181();
		        if ((mc.MyMeth1(1,2,3,4) == 2) && (mc.MyMeth1(1,2,3) == 1) && (MyMeth2(1,2,3,4) == 2) && (MyMeth2(1,2,3) == 1)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass182 {
	        public int MyMeth1(params short [] values) {
		        return 1;
	        }
	        public int MyMeth1(params int[] values) {
		        return 2;
	        }
	        public static int MyMeth2(params short [] values) {
		        return 1;
	        }
	        public static int MyMeth2(params int[] values) {
		        return 2;
	        }
	        public static bool testMethod() {
		        MethodsTestClass182 mc = new MethodsTestClass182();
		        short s1=1,s2=1,s3=1;
		        int i1=2,i2=2,i3=2;
		        if ((mc.MyMeth1(s1,s2,s3)==1) && (mc.MyMeth1(i1,i2,i3)==2) && (MyMeth2(s1,s2,s3)==1) && (MyMeth2(i1,i2,i3)==2))
		        {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass183 {
	        public int MyMeth1(params short[] values) {
		        return 1;
	        }
	        public int MyMeth1(params int[] values) {
		        return 2;
	        }
	        public static int MyMeth2(params short[] values) {
		        return 1;
	        }
	        public static int MyMeth2(params int[] values) {
		        return 2;
	        }
	        public static bool testMethod() {
		        MethodsTestClass183 mc = new MethodsTestClass183();
		        short s1 = 1;
		        int i1 = 2;
		        if ((mc.MyMeth1(s1,i1) == 2) && (MyMeth2(s1,i1) == 2)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass184 {
	        public int MyMeth1(params long[] values) {
		        return 1;
	        }
	        public int MyMeth1(params int[] values) {
		        return 2;
	        }
	        public static int MyMeth2(params long[] values) {
		        return 1;
	        }
	        public static int MyMeth2(params int[] values) {
		        return 2;
	        }
	        public static bool testMethod() {
		        MethodsTestClass184 mc = new MethodsTestClass184();
		        short s1 = 1;
		        byte b1 = 2;
		        if ((mc.MyMeth1(s1,b1) == 2) && (MyMeth2(s1,b1) == 2)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass185 {
	        public int MyMeth1(params int[] values) {
		        values[0] = 3;
		        return values[0];
	        }
	        public static int MyMeth2(params int[] values) {
		        values[0] = 4;
		        return values[0];
	        }
	        public static bool testMethod() {
		        MethodsTestClass185 mc = new MethodsTestClass185();
		        int intI = 2;
		        if ((mc.MyMeth1(intI) == 3) && (MyMeth2(intI) == 4) && (intI == 2)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass186 {
	        public int MyMeth1(params int[] values) {
		        return MyMeth2(values);
	        }
	        public int MyMeth2(params int[] values) {
		        return 3;
	        }
	        public static bool testMethod() {
		        MethodsTestClass186 mc = new MethodsTestClass186();
		        int intI = 2;
		        if (mc.MyMeth1(intI) == 3) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass187 {
	        public int MyMeth1(params object[] values) {
		        return MyMeth2((object)values);
	        }
	        public int MyMeth2(params object[] values) {
		        return values.Length;
	        }
	        public static bool testMethod() {
		        MethodsTestClass187 mc = new MethodsTestClass187();
		        if (mc.MyMeth1(2,3) == 1) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass_Base188 {
	        public int MyMeth() {
		        return 2;
	        }
        }
        public class MethodsTestClass_Derived188 : MethodsTestClass_Base188
        {
        }
        public class MethodsTestClass188 : MethodsTestClass_Derived188
        {
	        public int MyTest() {
		        return base.MyMeth();
	        }
	        public static bool testMethod() {
		        MethodsTestClass188 mc = new MethodsTestClass188();
		        if (mc.MyTest() == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }


        public class MethodsTestClass_Base190 {
	        protected int MyMeth1(params int[] values) {
		        return values[0] + values[1] + values [2];
	        }
	        protected static int MyMeth2(params int[] values) {
		        return values[0] + values[1] + values [2];
	        }
        }
        public class MethodsTestClass190 : MethodsTestClass_Base190 {
	        public static bool testMethod() {
		        int intI = 1;
		        int intJ = 2;
		        int intK = 3;
		        MethodsTestClass190 mc = new MethodsTestClass190();
		        if ((mc.MyMeth1(intI, intJ, intK) == 6) && (MyMeth2(intI, intJ, intK) == 6)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MyTest {
	        internal int MyMeth1(params int[] values) {
		        return values[0] + values[1] + values [2];
	        }
	        internal static int MyMeth2(params int[] values) {
		        return values[0] + values[1] + values [2];
	        }
        }
        public class MethodsTestClass191 {
	        public static bool testMethod() {
		        int intI = 1;
		        int intJ = 2;
		        int intK = 3;
		        MyTest mc = new MyTest();
		        if ((mc.MyMeth1(intI, intJ, intK) == 6) && (MyTest.MyMeth2(intI, intJ, intK) == 6)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass192_Test
        {
	        protected internal int MyMeth1(params int[] values) {
		        return values[0] + values[1] + values [2];
	        }
	        protected internal static int MyMeth2(params int[] values) {
		        return values[0] + values[1] + values [2];
	        }
        }
        public class MethodsTestClass192 {
	        public static bool testMethod() {
		        int intI = 1;
		        int intJ = 2;
		        int intK = 3;
		        MyTest mc = new MyTest();
		        if ((mc.MyMeth1(intI, intJ, intK) == 6) && (MyTest.MyMeth2(intI, intJ, intK) == 6)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass193 {
	        private int MyMeth1(params int[] values) {
		        return values[0] + values[1] + values [2];
	        }
	        private static int MyMeth2(params int[] values) {
		        return values[0] + values[1] + values [2];
	        }
	        public static bool testMethod() {
		        int intI = 1;
		        int intJ = 2;
		        int intK = 3;
		        MethodsTestClass193 mc = new MethodsTestClass193();
		        if ((mc.MyMeth1(intI, intJ, intK) == 6) && (MyMeth2(intI, intJ, intK) == 6)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass_Base194 {
	        public virtual int MyMeth1(params int[] values) {
		        return values[0] + values[1] + values [2];
	        }
        }
        public class MethodsTestClass194 : MethodsTestClass_Base194{
	        public override int MyMeth1(params int[] values) {
		        return -1;
	        }
	        public static bool testMethod() {
		        int intI = 1;
		        int intJ = 2;
		        int intK = 3;
		        MethodsTestClass_Base194 mc = new MethodsTestClass194();
		        if (mc.MyMeth1(intI, intJ, intK) == -1) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }	
        }

        public class MethodsTestClass_Base195 {
	        public virtual int MyMeth1(params int[] values) {
		        return values[0] + values[1] + values [2];
	        }
        }
        public class MethodsTestClass195 : MethodsTestClass_Base195{
	        public new int MyMeth1(params int[] values) {
		        return -1;
	        }
	        public static bool testMethod() {
		        int intI = 1;
		        int intJ = 2;
		        int intK = 3;
		        MethodsTestClass_Base195 mc = new MethodsTestClass195();
		        if (mc.MyMeth1(intI, intJ, intK) == 6) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }	
        }

        public abstract class MethodsTestClass_Base196 {
	        public abstract int MyMeth1(params int[] values);
        }
        public class MethodsTestClass196 : MethodsTestClass_Base196{
	        public override int MyMeth1(params int[] values) {
		        return values[0] + values[1] + values [2];
	        }
	        public static bool testMethod() {
		        int intI = 1;
		        int intJ = 2;
		        int intK = 3;
		        MethodsTestClass_Base196 mc = new MethodsTestClass196();
		        if (mc.MyMeth1(intI, intJ, intK) == 6) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }	
        }

        public delegate int MyDelegate(params int[] values);
        public class MethodsTestClass197 {
	        public int MyMeth1(params int[] values) {
		        return values[0] + values[1] + values [2];
	        }
	        public MyDelegate md;
	        public static bool testMethod() {
		        int intI = 1;
		        int intJ = 2;
		        int intK = 3;
		        MethodsTestClass197 mc = new MethodsTestClass197();
		        mc.md = new MyDelegate(mc.MyMeth1);
		        if (mc.md(intI, intJ, intK) == 6) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }	
        }

        public class MethodsTestClass_Base199 {
	        public virtual int RetInt() {
		        return 1;
	        }
        }
        public class MethodsTestClass199 : MethodsTestClass_Base199 {
	        public sealed override int RetInt() {
		        return 2;
	        }
	        public static bool testMethod() {
		        MethodsTestClass199 MC = new MethodsTestClass199();
		        if (MC.RetInt() == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
                //Compiled Test Cases 

        public class MethodsTestClass200
        {
            public int MyMeth()
            {

                int int1 = 1;
                int int2 = 2;
                int int3 = 3;
                int int4 = 4;
                int int5 = 5;
                int int6 = 6;
                int int7 = 7;
                int int8 = 8;
                int int9 = 9;
                int int10 = 10;
                int int11 = 11;
                int int12 = 12;
                int int13 = 13;
                int int14 = 14;
                int int15 = 15;
                int int16 = 16;
                int int17 = 17;
                int int18 = 18;
                int int19 = 19;
                int int20 = 20;
                int int21 = 21;
                int int22 = 22;
                int int23 = 23;
                int int24 = 24;
                int int25 = 25;
                int int26 = 26;
                int int27 = 27;
                int int28 = 28;
                int int29 = 29;
                int int30 = 30;
                int int31 = 31;
                int int32 = 32;
                int int33 = 33;

                int intRet = int1 + int2 + int3 + int4 + int5 + int6 + int7 + int8 + int9 + int10 +
                    int11 + int12 + int13 + int14 + int15 + int16 + int17 + int18 + int19 + int20 +
                    int21 + int22 + int23 + int24 + int25 + int26 + int27 + int28 + int29 + int30 +
                    int31 + int32 + int33;

                return intRet;

            }

            public static bool testMethod()
            {

                MethodsTestClass200 MC = new MethodsTestClass200();

                if (MC.MyMeth() == 561)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class MethodsTestClass201 {
	        public static int MyMeth() {
		        int int1 = 1;
		        int int2 = 2;
		        int int3 = 3;
		        int int4 = 4;
		        int int5 = 5;
		        int int6 = 6;
		        int int7 = 7;
		        int int8 = 8;
		        int int9 = 9;
		        int int10 = 10;
		        int int11 = 11;
		        int int12 = 12;
		        int int13 = 13;
		        int int14 = 14;
		        int int15 = 15;
		        int int16 = 16;
		        int int17 = 17;
		        int int18 = 18;
		        int int19 = 19;
		        int int20 = 20;
		        int int21 = 21;
		        int int22 = 22;
		        int int23 = 23;
		        int int24 = 24;
		        int int25 = 25;
		        int int26 = 26;
		        int int27 = 27;
		        int int28 = 28;
		        int int29 = 29;
		        int int30 = 30;
		        int int31 = 31;
		        int int32 = 32;
		        int int33 = 33;
		        int intRet = int1 + int2 + int3 + int4 + int5 + int6 + int7 + int8 + int9 + int10 +
			        int11 + int12 + int13 + int14 + int15 + int16 + int17 + int18 + int19 + int20 +
			        int21 + int22 + int23 + int24 + int25 + int26 + int27 + int28 + int29 + int30 +
			        int31 + int32 + int33;
         
		        return intRet;
	        }
	        public static bool testMethod() {
         
		        if (MyMeth() == 561) {
			        return true;
		        }
		        else {
			        return false;
		        }		
	        }
        }

        public class MethodsTestClass204 {
	        public int MyMeth(int i, int j, params int[] k) {
		        return i + j;
	        }
	        public static bool testMethod() {
		        MethodsTestClass204 MC = new MethodsTestClass204();
		        if (MC.MyMeth(1,2) == 3) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass205 {
	        public static int MyMeth(int i, int j, params int[] k) {
		        return i + j;
	        }
	        public static bool testMethod() {
		        if (MyMeth(1,2) == 3) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass_Sub206
        {
	        private static int MyMeth(int intI) {
		        return 1;
	        }	
	        public static int MyMeth(params int[] intI) {
		        return 202;
	        }
        }
        public class MethodsTestClass206 {
	        public static bool testMethod() {
                if (MethodsTestClass_Sub206.MyMeth(201) == 202)
                {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass_Sub207
        {
	        private int MyMeth(int intI) {
		        return 1;
	        }	
	        public int MyMeth(params int[] intI) {
		        return 202;
	        }
        }
        public class MethodsTestClass207 {
	        public static bool testMethod() {
                MethodsTestClass_Sub207 MC = new MethodsTestClass_Sub207();
		        if (MC.MyMeth(201) == 202) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

                //Compiled Test Cases 
 
        public class MyMethodsTestClass_Base210 
        {
	        public virtual int MyMeth(int intI) 
	        {
		        return 1;
	        }	
        }
        public class MethodsTestClass210_sub : MyMethodsTestClass_Base210
        {
	        public sealed override int MyMeth(int intI) 
	        {
		        return 3;
	        }	
        }
        public class MethodsTestClass210 
        {
	        public static bool testMethod() {
		        MethodsTestClass210_sub MC = new MethodsTestClass210_sub();
		        if (MC.MyMeth(1) == 3) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MethodsTestClass223_Sub
        {
	        public int MyMeth(ref int mbc) 
	        {
		        return 1;
	        }	
	        public int MyMeth(int mbc) 
	        {
		        return 2;
	        }	
        }
        public class MethodsTestClass223
        {
	        public static bool testMethod() {
		        int retval = 3;
                MethodsTestClass223_Sub mc = new MethodsTestClass223_Sub();
		        retval -= mc.MyMeth (1);
		        int i = 1;
		        retval -= mc.MyMeth (ref i);
		        return (retval == 0) ;
	        }
        }


        public class MethodsTestClass224_Sub
        {
	        public int MyMeth(out int mbc) 
	        {
		        mbc = 666;
		        return 1;
	        }	
	        public int MyMeth(int mbc) 
	        {
		        return 2;
	        }	
        }
        public class MethodsTestClass224
        {
	        public static bool testMethod() {
		        int retval = 3;
                MethodsTestClass224_Sub mc = new MethodsTestClass224_Sub();
		        retval -= mc.MyMeth (1);
		        int i;
		        retval -= mc.MyMeth (out i);
		        return (retval == 0);
	        }
        }

        public class MethodsTestClass229_SubB
        {
           public virtual void f() {}
        }

        public class MethodsTestClass229 : MethodsTestClass229_SubB
        {
            public void f() // CS0114
            {
            }
            public static bool testMethod()
            {
                return true;
            }

        }

        public class MethodsTestClass230_Base
        {
           virtual public object f(int x, string y) {return null;}
        }

        public class MethodsTestClass230 : MethodsTestClass230_Base
        {
           object f(int x, string y) // CS0114
           {
                return null;
           }

           public static bool testMethod() 
           {
               return true;
           }
        }

        public class MethodsTestClass231_Base
        {
           virtual public object f
           {
                get { return null; }
                set {}
           }
        }

        public class MethodsTestClass231 : MethodsTestClass231_Base
        {
           object f // CS0114
           {
                get { return null; }
                set {}
           }

           public static bool testMethod() 
           {
                return true;
           }
        }


        public delegate void MethodsTestClass232_Del();

        public class MethodsTestClass232_B
        {
           public MethodsTestClass232_Del fooDel;

           public virtual event MethodsTestClass232_Del fooEv
           {
            add {}
            remove {}
           }
        }

        public class MethodsTestClass232 : MethodsTestClass232_B
        {
           event MethodsTestClass232_Del fooEv // CS0114
           {
                add {}
                remove {}
           }

           public static bool testMethod()
           {
                return true;
           }
        }


        public class MethodsTestClass233_Base {
	        public int MyMeth(){
		        return 1;
	        }
        }
        public class MethodsTestClass233 : MethodsTestClass233_Base {	
	        //new modifier
	        new public int MyMeth(){
		        return 2;
	        }	
	        public static bool testMethod() {
		        MethodsTestClass233 test = new MethodsTestClass233();
		        if (test.MyMeth() == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

    }
}