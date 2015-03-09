////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class CategoriesTests : IMFTestInterface
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

        //Categories Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\Categories
        //static01,static02,static03,static04,static05,static06,static07,static09,static11,static12,static13,static14,static15,static16,static17,static18,static19,static20,static21,inst018,inst019,inst020,inst021,inst022,inst023,inst024,inst026,inst028,inst029,inst030,inst031,inst032,inst033,inst034,inst035,inst036,inst037,inst038,inst039,inst040,inst041,inst042,inst043,inst044,inst046,inst048,inst049,inst050,inst051,inst052,inst053,inst054

        //Test Case Calls 
        
        [TestMethod]
        public MFTestResults Categories_static01_Test()
        {
            Log.Comment("Section 5.1.1");
            Log.Comment(" Categories_TestClass_?_A field declared with the static modifier is called a");
            Log.Comment("static variable.   Categories_TestClass_?_A static variable comes into existence");
            Log.Comment("when the type in which it is declared is loaded, and ");
            Log.Comment("ceases to exist when the type in which it is declared");
            Log.Comment("is unloaded.");
            if (Categories_TestClass_static01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_static02_Test()
        {
            Log.Comment("Section 5.1.1");
            Log.Comment(" Categories_TestClass_?_A field declared with the static modifier is called a");
            Log.Comment("static variable.   Categories_TestClass_?_A static variable comes into existence");
            Log.Comment("when the type in which it is declared is loaded, and ");
            Log.Comment("ceases to exist when the type in which it is declared");
            Log.Comment("is unloaded.");
            if (Categories_TestClass_static02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_static03_Test()
        {
            Log.Comment("Section 5.1.1");
            Log.Comment(" Categories_TestClass_?_A field declared with the static modifier is called a");
            Log.Comment("static variable.   Categories_TestClass_?_A static variable comes into existence");
            Log.Comment("when the type in which it is declared is loaded, and ");
            Log.Comment("ceases to exist when the type in which it is declared");
            Log.Comment("is unloaded.");
            if (Categories_TestClass_static03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_static04_Test()
        {
            Log.Comment("Section 5.1.1");
            Log.Comment(" Categories_TestClass_?_A field declared with the static modifier is called a");
            Log.Comment("static variable.   Categories_TestClass_?_A static variable comes into existence");
            Log.Comment("when the type in which it is declared is loaded, and ");
            Log.Comment("ceases to exist when the type in which it is declared");
            Log.Comment("is unloaded.");
            if (Categories_TestClass_static04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_static05_Test()
        {
            Log.Comment("Section 5.1.1");
            Log.Comment(" Categories_TestClass_?_A field declared with the static modifier is called a");
            Log.Comment("static variable.   Categories_TestClass_?_A static variable comes into existence");
            Log.Comment("when the type in which it is declared is loaded, and ");
            Log.Comment("ceases to exist when the type in which it is declared");
            Log.Comment("is unloaded.");
            if (Categories_TestClass_static05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        
        [TestMethod]
        public MFTestResults Categories_static06_Test()
        {
            Log.Comment("Section 5.1.1");
            Log.Comment(" Categories_TestClass_?_A field declared with the static modifier is called a");
            Log.Comment("static variable.   Categories_TestClass_?_A static variable comes into existence");
            Log.Comment("when the type in which it is declared is loaded, and ");
            Log.Comment("ceases to exist when the type in which it is declared");
            Log.Comment("is unloaded.");
            if (Categories_TestClass_static06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_static07_Test()
        {
            Log.Comment("Section 5.1.1");
            Log.Comment(" Categories_TestClass_?_A field declared with the static modifier is called a");
            Log.Comment("static variable.   Categories_TestClass_?_A static variable comes into existence");
            Log.Comment("when the type in which it is declared is loaded, and ");
            Log.Comment("ceases to exist when the type in which it is declared");
            Log.Comment("is unloaded.");
            if (Categories_TestClass_static07.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_static09_Test()
        {
            Log.Comment("Section 5.1.1");
            Log.Comment(" Categories_TestClass_?_A field declared with the static modifier is called a");
            Log.Comment("static variable.   Categories_TestClass_?_A static variable comes into existence");
            Log.Comment("when the type in which it is declared is loaded, and ");
            Log.Comment("ceases to exist when the type in which it is declared");
            Log.Comment("is unloaded.");
            if (Categories_TestClass_static09.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_static11_Test()
        {
            Log.Comment("Section 5.1.1");
            Log.Comment(" Categories_TestClass_?_A field declared with the static modifier is called a");
            Log.Comment("static variable.   Categories_TestClass_?_A static variable comes into existence");
            Log.Comment("when the type in which it is declared is loaded, and ");
            Log.Comment("ceases to exist when the type in which it is declared");
            Log.Comment("is unloaded.");
            if (Categories_TestClass_static11.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        
        [TestMethod]
        public MFTestResults Categories_static12_Test()
        {
            Log.Comment("Section 5.1.1");
            Log.Comment(" Categories_TestClass_?_A field declared with the static modifier is called a");
            Log.Comment("static variable.   Categories_TestClass_?_A static variable comes into existence");
            Log.Comment("when the type in which it is declared is loaded, and ");
            Log.Comment("ceases to exist when the type in which it is declared");
            Log.Comment("is unloaded.");
            if (Categories_TestClass_static12.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_static13_Test()
        {
            Log.Comment("Section 5.1.1");
            Log.Comment(" Categories_TestClass_?_A field declared with the static modifier is called a");
            Log.Comment("static variable.   Categories_TestClass_?_A static variable comes into existence");
            Log.Comment("when the type in which it is declared is loaded, and ");
            Log.Comment("ceases to exist when the type in which it is declared");
            Log.Comment("is unloaded.");
            if (Categories_TestClass_static13.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_static14_Test()
        {
            Log.Comment("Section 5.1.1");
            Log.Comment(" Categories_TestClass_?_A field declared with the static modifier is called a");
            Log.Comment("static variable.   Categories_TestClass_?_A static variable comes into existence");
            Log.Comment("when the type in which it is declared is loaded, and ");
            Log.Comment("ceases to exist when the type in which it is declared");
            Log.Comment("is unloaded.");
            if (Categories_TestClass_static14.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_static15_Test()
        {
            Log.Comment("Section 5.1.1");
            Log.Comment(" Categories_TestClass_?_A field declared with the static modifier is called a");
            Log.Comment("static variable.   Categories_TestClass_?_A static variable comes into existence");
            Log.Comment("when the type in which it is declared is loaded, and ");
            Log.Comment("ceases to exist when the type in which it is declared");
            Log.Comment("is unloaded.");
            if (Categories_TestClass_static15.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_static16_Test()
        {
            Log.Comment("Section 5.1.1");
            Log.Comment(" Categories_TestClass_?_A field declared with the static modifier is called a");
            Log.Comment("static variable.   Categories_TestClass_?_A static variable comes into existence");
            Log.Comment("when the type in which it is declared is loaded, and ");
            Log.Comment("ceases to exist when the type in which it is declared");
            Log.Comment("is unloaded.");
            if (Categories_TestClass_static16.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_static17_Test()
        {
            Log.Comment("Section 5.1.1");
            Log.Comment(" Categories_TestClass_?_A field declared with the static modifier is called a");
            Log.Comment("static variable.   Categories_TestClass_?_A static variable comes into existence");
            Log.Comment("when the type in which it is declared is loaded, and ");
            Log.Comment("ceases to exist when the type in which it is declared");
            Log.Comment("is unloaded.");
            if (Categories_TestClass_static17.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        
        [TestMethod]
        public MFTestResults Categories_static18_Test()
        {
            Log.Comment("Section 5.1.1");
            Log.Comment("The initial value of a static variable is the default value");
            Log.Comment("of the variable's type.");
            if (Categories_TestClass_static18.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_static19_Test()
        {
            Log.Comment("Section 5.1.1");
            Log.Comment("The initial value of a static variable is the default value");
            Log.Comment("of the variable's type.");
            if (Categories_TestClass_static19.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        
        [TestMethod]
        public MFTestResults Categories_static20_Test()
        {
            Log.Comment("Section 5.1.1");
            Log.Comment("The initial value of a static variable is the default value");
            Log.Comment("of the variable's type.");
            if (Categories_TestClass_static20.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        /*
        [TestMethod]
        public MFTestResults Categories_static21_Test()
        {
            Log.Comment("Section 5");
            if (Categories_TestClass_static21.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        */
        [TestMethod]
        public MFTestResults Categories_inst018_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a class comes into existence when ");
            Log.Comment("a new instance of that class is created, and ceases to exist");
            Log.Comment("when there are no references to that instance and the finalizer");
            Log.Comment("of the instance has executed.");
            if (Categories_TestClass_inst018.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst019_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a class comes into existence when ");
            Log.Comment("a new instance of that class is created, and ceases to exist");
            Log.Comment("when there are no references to that instance and the finalizer");
            Log.Comment("of the instance has executed.");
            if (Categories_TestClass_inst019.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst020_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a class comes into existence when ");
            Log.Comment("a new instance of that class is created, and ceases to exist");
            Log.Comment("when there are no references to that instance and the finalizer");
            Log.Comment("of the instance has executed.");
            if (Categories_TestClass_inst020.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst021_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a class comes into existence when ");
            Log.Comment("a new instance of that class is created, and ceases to exist");
            Log.Comment("when there are no references to that instance and the finalizer");
            Log.Comment("of the instance has executed.");
            if (Categories_TestClass_inst021.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst022_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a class comes into existence when ");
            Log.Comment("a new instance of that class is created, and ceases to exist");
            Log.Comment("when there are no references to that instance and the finalizer");
            Log.Comment("of the instance has executed.");
            if (Categories_TestClass_inst022.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst023_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a class comes into existence when ");
            Log.Comment("a new instance of that class is created, and ceases to exist");
            Log.Comment("when there are no references to that instance and the finalizer");
            Log.Comment("of the instance has executed.");
            if (Categories_TestClass_inst023.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst024_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a class comes into existence when ");
            Log.Comment("a new instance of that class is created, and ceases to exist");
            Log.Comment("when there are no references to that instance and the finalizer");
            Log.Comment("of the instance has executed.");
            if (Categories_TestClass_inst024.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst026_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a class comes into existence when ");
            Log.Comment("a new instance of that class is created, and ceases to exist");
            Log.Comment("when there are no references to that instance and the finalizer");
            Log.Comment("of the instance has executed.");
            if (Categories_TestClass_inst026.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst028_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a class comes into existence when ");
            Log.Comment("a new instance of that class is created, and ceases to exist");
            Log.Comment("when there are no references to that instance and the finalizer");
            Log.Comment("of the instance has executed.");
            if (Categories_TestClass_inst028.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst029_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a class comes into existence when ");
            Log.Comment("a new instance of that class is created, and ceases to exist");
            Log.Comment("when there are no references to that instance and the finalizer");
            Log.Comment("of the instance has executed.");
            if (Categories_TestClass_inst029.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst030_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a class comes into existence when ");
            Log.Comment("a new instance of that class is created, and ceases to exist");
            Log.Comment("when there are no references to that instance and the finalizer");
            Log.Comment("of the instance has executed.");
            if (Categories_TestClass_inst030.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst031_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a class comes into existence when ");
            Log.Comment("a new instance of that class is created, and ceases to exist");
            Log.Comment("when there are no references to that instance and the finalizer");
            Log.Comment("of the instance has executed.");
            if (Categories_TestClass_inst031.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst032_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a class comes into existence when ");
            Log.Comment("a new instance of that class is created, and ceases to exist");
            Log.Comment("when there are no references to that instance and the finalizer");
            Log.Comment("of the instance has executed.");
            if (Categories_TestClass_inst032.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst033_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a class comes into existence when ");
            Log.Comment("a new instance of that class is created, and ceases to exist");
            Log.Comment("when there are no references to that instance and the finalizer");
            Log.Comment("of the instance has executed.");
            if (Categories_TestClass_inst033.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst034_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a class comes into existence when ");
            Log.Comment("a new instance of that class is created, and ceases to exist");
            Log.Comment("when there are no references to that instance and the finalizer");
            Log.Comment("of the instance has executed.");
            if (Categories_TestClass_inst034.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst035_Test()
        {
            Log.Comment("Section 5.1.1");
            Log.Comment("The initial value of an instance variable is the default value");
            Log.Comment("of the variable's type.");
            if (Categories_TestClass_inst035.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst036_Test()
        {
            Log.Comment("Section 5.1.1");
            Log.Comment("The initial value of an instance variable is the default value");
            Log.Comment("of the variable's type.");
            if (Categories_TestClass_inst036.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst037_Test()
        {
            Log.Comment("Section 5.1.1");
            Log.Comment("The initial value of an instance variable is the default value");
            Log.Comment("of the variable's type.");
            if (Categories_TestClass_inst037.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst038_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a struct has exactly the");
            Log.Comment("same lifetime as the struct variable to which it");
            Log.Comment("belongs. In other words, when a variable of a ");
            Log.Comment("struct type comes into existence or ceases to ");
            Log.Comment("exist, so do the instance variables of the struct.");
            if (Categories_TestClass_inst038.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst039_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a struct has exactly the");
            Log.Comment("same lifetime as the struct variable to which it");
            Log.Comment("belongs. In other words, when a variable of a ");
            Log.Comment("struct type comes into existence or ceases to ");
            Log.Comment("exist, so do the instance variables of the struct.");
            if (Categories_TestClass_inst039.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst040_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a struct has exactly the");
            Log.Comment("same lifetime as the struct variable to which it");
            Log.Comment("belongs. In other words, when a variable of a ");
            Log.Comment("struct type comes into existence or ceases to ");
            Log.Comment("exist, so do the instance variables of the struct.");
            if (Categories_TestClass_inst040.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst041_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a struct has exactly the");
            Log.Comment("same lifetime as the struct variable to which it");
            Log.Comment("belongs. In other words, when a variable of a ");
            Log.Comment("struct type comes into existence or ceases to ");
            Log.Comment("exist, so do the instance variables of the struct.");
            if (Categories_TestClass_inst041.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst042_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a struct has exactly the");
            Log.Comment("same lifetime as the struct variable to which it");
            Log.Comment("belongs. In other words, when a variable of a ");
            Log.Comment("struct type comes into existence or ceases to ");
            Log.Comment("exist, so do the instance variables of the struct.");
            if (Categories_TestClass_inst042.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst043_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a struct has exactly the");
            Log.Comment("same lifetime as the struct variable to which it");
            Log.Comment("belongs. In other words, when a variable of a ");
            Log.Comment("struct type comes into existence or ceases to ");
            Log.Comment("exist, so do the instance variables of the struct.");
            if (Categories_TestClass_inst043.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst044_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a struct has exactly the");
            Log.Comment("same lifetime as the struct variable to which it");
            Log.Comment("belongs. In other words, when a variable of a ");
            Log.Comment("struct type comes into existence or ceases to ");
            Log.Comment("exist, so do the instance variables of the struct.");
            if (Categories_TestClass_inst044.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst046_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a struct has exactly the");
            Log.Comment("same lifetime as the struct variable to which it");
            Log.Comment("belongs. In other words, when a variable of a ");
            Log.Comment("struct type comes into existence or ceases to ");
            Log.Comment("exist, so do the instance variables of the struct.");
            if (Categories_TestClass_inst046.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst048_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a struct has exactly the");
            Log.Comment("same lifetime as the struct variable to which it");
            Log.Comment("belongs. In other words, when a variable of a ");
            Log.Comment("struct type comes into existence or ceases to ");
            Log.Comment("exist, so do the instance variables of the struct.");
            if (Categories_TestClass_inst048.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst049_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a struct has exactly the");
            Log.Comment("same lifetime as the struct variable to which it");
            Log.Comment("belongs. In other words, when a variable of a ");
            Log.Comment("struct type comes into existence or ceases to ");
            Log.Comment("exist, so do the instance variables of the struct.");
            if (Categories_TestClass_inst049.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst050_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a struct has exactly the");
            Log.Comment("same lifetime as the struct variable to which it");
            Log.Comment("belongs. In other words, when a variable of a ");
            Log.Comment("struct type comes into existence or ceases to ");
            Log.Comment("exist, so do the instance variables of the struct.");
            if (Categories_TestClass_inst050.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst051_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a struct has exactly the");
            Log.Comment("same lifetime as the struct variable to which it");
            Log.Comment("belongs. In other words, when a variable of a ");
            Log.Comment("struct type comes into existence or ceases to ");
            Log.Comment("exist, so do the instance variables of the struct.");
            if (Categories_TestClass_inst051.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst052_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a struct has exactly the");
            Log.Comment("same lifetime as the struct variable to which it");
            Log.Comment("belongs. In other words, when a variable of a ");
            Log.Comment("struct type comes into existence or ceases to ");
            Log.Comment("exist, so do the instance variables of the struct.");
            if (Categories_TestClass_inst052.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst053_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a struct has exactly the");
            Log.Comment("same lifetime as the struct variable to which it");
            Log.Comment("belongs. In other words, when a variable of a ");
            Log.Comment("struct type comes into existence or ceases to ");
            Log.Comment("exist, so do the instance variables of the struct.");
            if (Categories_TestClass_inst053.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Categories_inst054_Test()
        {
            Log.Comment("Section 5.1.2");
            Log.Comment("An instance variable of a struct has exactly the");
            Log.Comment("same lifetime as the struct variable to which it");
            Log.Comment("belongs. In other words, when a variable of a ");
            Log.Comment("struct type comes into existence or ceases to ");
            Log.Comment("exist, so do the instance variables of the struct.");
            if (Categories_TestClass_inst054.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        

        //Compiled Test Cases 
        
        public class Categories_TestClass_static01_1
        {
            public static byte b1 = 1;
        }
        public class Categories_TestClass_static01
        {
            public static byte b1 = 2;
            public static int Main_old()
            {
                if (Categories_TestClass_static01_1.b1 != (byte)1)
                {
                    return 1;
                }
                if (Categories_TestClass_static01.b1 != (byte)2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Categories_TestClass_static02_1
        {
            public static char c1 = 'a';
        }
        public class Categories_TestClass_static02
        {
            public static char c1 = 'b';
            public static int Main_old()
            {
                if (Categories_TestClass_static02_1.c1 != 'a')
                {
                    return 1;
                }
                if (Categories_TestClass_static02.c1 != 'b')
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Categories_TestClass_static03_1
        {
            public static short s1 = 1;
        }
        public class Categories_TestClass_static03
        {
            public static short s1 = 2;
            public static int Main_old()
            {
                if (Categories_TestClass_static03_1.s1 != (short)1)
                {
                    return 1;
                }
                if (Categories_TestClass_static03.s1 != (short)2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Categories_TestClass_static04_1
        {
            public static int i1 = 1;
        }
        public class Categories_TestClass_static04
        {
            public static int i1 = 2;
            public static int Main_old()
            {
                if (Categories_TestClass_static04_1.i1 != 1)
                {
                    return 1;
                }
                if (Categories_TestClass_static04.i1 != 2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Categories_TestClass_static05_1
        {
            public static long l1 = 1L;
        }
        public class Categories_TestClass_static05
        {
            public static long l1 = 2L;
            public static int Main_old()
            {
                if (Categories_TestClass_static05_1.l1 != 1L)
                {
                    return 1;
                }
                if (Categories_TestClass_static05.l1 != 2L)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        
        public class Categories_TestClass_static06_1
        {
            public static float f1 = 1f;
        }
        public class Categories_TestClass_static06
        {
            public static float f1 = 2f;
            public static int Main_old()
            {
                if (Categories_TestClass_static06_1.f1 != 1f)
                {
                    return 1;
                }
                if (Categories_TestClass_static06.f1 != 2f)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Categories_TestClass_static07_1
        {
            public static double d1 = 1d;
        }
        public class Categories_TestClass_static07
        {
            public static double d1 = 2d;
            public static int Main_old()
            {
                if (Categories_TestClass_static07_1.d1 != 1d)
                {
                    return 1;
                }
                if (Categories_TestClass_static07.d1 != 2d)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Categories_TestClass_static09_1
        {
            public static bool b1 = true;
        }
        public class Categories_TestClass_static09
        {
            public static bool b1 = false;
            public static int Main_old()
            {
                if (Categories_TestClass_static09_1.b1 != true)
                {
                    return 1;
                }
                if (Categories_TestClass_static09.b1 != false)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Categories_TestClass_static11_1
        {
            public static string s1 = "string1";
        }
        public class Categories_TestClass_static11
        {
            public static string s1 = "string2";
            public static int Main_old()
            {
                if (Categories_TestClass_static11_1.s1.Equals("string1") != true)
                {
                    return 1;
                }
                if (Categories_TestClass_static11.s1.Equals("string2") != true)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        
        public struct Categories_TestClass_static12_Str
        {
            public Categories_TestClass_static12_Str(int intJ)
            {
                intI = intJ;
            }
            public int intI;
        }
        public class Categories_TestClass_static12_1
        {
            public static Categories_TestClass_static12_Str s1 = new Categories_TestClass_static12_Str(1);
        }
        public class Categories_TestClass_static12
        {
            public static Categories_TestClass_static12_Str s1 = new Categories_TestClass_static12_Str(2);
            public static int Main_old()
            {
                if (Categories_TestClass_static12_1.s1.intI != 1)
                {
                    return 1;
                }
                if (Categories_TestClass_static12.s1.intI != 2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public enum Categories_TestClass_static13_En { a = 1, b = 2 }
        public class Categories_TestClass_static13_1
        {
            public static Categories_TestClass_static13_En E = Categories_TestClass_static13_En.a;
        }
        public class Categories_TestClass_static13
        {
            public static Categories_TestClass_static13_En E = Categories_TestClass_static13_En.b;
            public static int Main_old()
            {
                if (Categories_TestClass_static13_1.E != Categories_TestClass_static13_En.a)
                {
                    return 1;
                }
                if (Categories_TestClass_static13.E != Categories_TestClass_static13_En.b)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Categories_TestClass_static14_C
        {
            public Categories_TestClass_static14_C(int intJ)
            {
                intI = intJ;
            }
            public int intI;
        }
        public class Categories_TestClass_static14_1
        {
            public static Categories_TestClass_static14_C c1 = new Categories_TestClass_static14_C(1);
        }
        public class Categories_TestClass_static14
        {
            public static Categories_TestClass_static14_C c1 = new Categories_TestClass_static14_C(2);
            public static int Main_old()
            {
                if (Categories_TestClass_static14_1.c1.intI != 1)
                {
                    return 1;
                }
                if (Categories_TestClass_static14.c1.intI != 2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public interface Categories_TestClass_static15_Inter
        {
            int intRet();
        }
        public class Categories_TestClass_static15_2 : Categories_TestClass_static15_Inter
        {
            public Categories_TestClass_static15_2(int intJ)
            {
                intI = intJ;
            }
            public int intI;
            public int intRet()
            {
                return intI;
            }
        }
        public class Categories_TestClass_static15_1
        {
            public static Categories_TestClass_static15_Inter i1 = (Categories_TestClass_static15_Inter)new Categories_TestClass_static15_2(1);
        }
        public class Categories_TestClass_static15
        {
            public static Categories_TestClass_static15_Inter i1 = (Categories_TestClass_static15_Inter)new Categories_TestClass_static15_2(2);
            public static int Main_old()
            {
                if (Categories_TestClass_static15_1.i1.intRet() != 1)
                {
                    return 1;
                }
                if (Categories_TestClass_static15.i1.intRet() != 2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Categories_TestClass_static16_1
        {
            public static int[] i = new int[] { 1 };
        }
        public class Categories_TestClass_static16
        {
            public static int[] i = new int[] { 2 };
            public static int Main_old()
            {
                if (Categories_TestClass_static16_1.i[0] != 1)
                {
                    return 1;
                }
                if (Categories_TestClass_static16.i[0] != 2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public delegate int Categories_TestClass_static17_Del();
        public class Categories_TestClass_static17_1
        {
            public static int RetInt()
            {
                return 1;
            }
            public static Categories_TestClass_static17_Del d = new Categories_TestClass_static17_Del(RetInt);
        }
        public class Categories_TestClass_static17
        {
            public static int RetInt()
            {
                return 2;
            }
            public static Categories_TestClass_static17_Del d = new Categories_TestClass_static17_Del(RetInt);
            public static int Main_old()
            {
                if (Categories_TestClass_static17_1.d() != 1)
                {
                    return 1;
                }
                if (Categories_TestClass_static17.d() != 2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        
        public class Categories_TestClass_static18
        {
            public static byte b1;
            public static char c1;
            public static short s1;
            public static int i1;
            public static long l1;
            public static float f1;
            public static double d1;
            public static bool b2;
            public static int Main_old()
            {
                if (Categories_TestClass_static18.b1 != (byte)0)
                {
                    return 1;
                }
                if (Categories_TestClass_static18.c1 != '\x0000')
                {
                    return 2;
                }
                if (Categories_TestClass_static18.s1 != (short)0)
                {
                    return 3;
                }
                if (Categories_TestClass_static18.i1 != 0)
                {
                    return 4;
                }
                if (Categories_TestClass_static18.l1 != 0L)
                {
                    return 5;
                }
                if (Categories_TestClass_static18.f1 != 0f)
                {
                    return 6;
                }
                if (Categories_TestClass_static18.d1 != 0d)
                {
                    return 7;
                }
                if (Categories_TestClass_static18.b2 != false)
                {
                    return 8;
                }
                return 0;
            }
            public static bool testMethod()
            {
                int ret = 0;
                if ((ret = Main_old()) != 0)
                {
                    Log.Comment("f1=" + f1.ToString() + " 0f=" + (0f).ToString());
                    Log.Comment(ret.ToString());
                    return false;
                }
                else
                    return true;
            }
        }
        public enum Categories_TestClass_static19_En { a = 1 }
        public struct Categories_TestClass_static19_Str
        {
            public byte b1;
            public char c1;
            public short s1;
            public int i1;
            public long l1;
            public float f1;
            public double d1;
            public bool b2;
        }
        public class Categories_TestClass_static19
        {
            static Categories_TestClass_static19_Str MS;
            static Categories_TestClass_static19_En ME;
            public static int Main_old()
            {
                if (MS.b1 != (byte)0)
                {
                    return 1;
                }
                if (MS.c1 != '\x0000')
                {
                    return 1;
                }
                if (MS.s1 != (short)0)
                {
                    return 1;
                }
                if (MS.i1 != 0)
                {
                    return 1;
                }
                if (MS.l1 != 0L)
                {
                    return 1;
                }
                if (MS.f1 != 0f)
                {
                    return 1;
                }
                if (MS.d1 != 0d)
                {
                    return 1;
                }
                if (MS.b2 != false)
                {
                    return 1;
                }
                if (ME != (Categories_TestClass_static19_En)0)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        
        public interface Categories_TestClass_static20_Inter { }
        public class Categories_TestClass_static20_1 { }
        public delegate int Categories_TestClass_static20_Del();
        public class Categories_TestClass_static20
        {
            public static string MS;//string
            public static Categories_TestClass_static20_Inter MI;//interface
            public static Categories_TestClass_static20 MC;//class
            public static int[] MA;//array
            public static Categories_TestClass_static20_Del MD;//delegate
            public static int Main_old()
            {
                if (Categories_TestClass_static20.MS != null)
                {
                    return 1;
                }
                if (Categories_TestClass_static20.MI != null)
                {
                    return 1;
                }
                if (Categories_TestClass_static20.MC != null)
                {
                    return 1;
                }
                if (Categories_TestClass_static20.MA != null)
                {
                    return 1;
                }
                if (Categories_TestClass_static20.MD != null)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        /*
        public class Categories_TestClass_static21
        {
            const string s = null;
            public static int Main_old()
            {
                if (s == null)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        */
        
        public class Categories_TestClass_inst018_1
        {
            public byte b1 = 1;
        }
        public class Categories_TestClass_inst018
        {
            public byte b1 = 2;
            public static int Main_old()
            {
                Categories_TestClass_inst018_1 Test1 = new Categories_TestClass_inst018_1();
                Categories_TestClass_inst018 Test2 = new Categories_TestClass_inst018();
                if (Test1.b1 != (byte)1)
                {
                    return 1;
                }
                if (Test2.b1 != (byte)2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Categories_TestClass_inst019_1
        {
            public char c1 = 'a';
        }
        public class Categories_TestClass_inst019
        {
            public char c1 = 'b';
            public static int Main_old()
            {
                Categories_TestClass_inst019_1 Test1 = new Categories_TestClass_inst019_1();
                Categories_TestClass_inst019 Test2 = new Categories_TestClass_inst019();
                if (Test1.c1 != 'a')
                {
                    return 1;
                }
                if (Test2.c1 != 'b')
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Categories_TestClass_inst020_1
        {
            public short s1 = 1;
        }
        public class Categories_TestClass_inst020
        {
            public short s1 = 2;
            public static int Main_old()
            {
                Categories_TestClass_inst020_1 Test1 = new Categories_TestClass_inst020_1();
                Categories_TestClass_inst020 Test2 = new Categories_TestClass_inst020();
                if (Test1.s1 != (short)1)
                {
                    return 1;
                }
                if (Test2.s1 != (short)2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Categories_TestClass_inst021_1
        {
            public int i1 = 1;
        }
        public class Categories_TestClass_inst021
        {
            public int i1 = 2;
            public static int Main_old()
            {
                Categories_TestClass_inst021_1 Test1 = new Categories_TestClass_inst021_1();
                Categories_TestClass_inst021 Test2 = new Categories_TestClass_inst021();
                if (Test1.i1 != 1)
                {
                    return 1;
                }
                if (Test2.i1 != 2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Categories_TestClass_inst022_1
        {
            public long l1 = 1L;
        }
        public class Categories_TestClass_inst022
        {
            public long l1 = 2L;
            public static int Main_old()
            {
                Categories_TestClass_inst022_1 Test1 = new Categories_TestClass_inst022_1();
                Categories_TestClass_inst022 Test2 = new Categories_TestClass_inst022();
                if (Test1.l1 != 1L)
                {
                    return 1;
                }
                if (Test2.l1 != 2L)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Categories_TestClass_inst023_1
        {
            public float f1 = 1f;
        }
        public class Categories_TestClass_inst023
        {
            public float f1 = 2f;
            public static int Main_old()
            {
                Categories_TestClass_inst023_1 Test1 = new Categories_TestClass_inst023_1();
                Categories_TestClass_inst023 Test2 = new Categories_TestClass_inst023();
                if (Test1.f1 != 1f)
                {
                    return 1;
                }
                if (Test2.f1 != 2f)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Categories_TestClass_inst024_1
        {
            public double d1 = 1d;
        }
        public class Categories_TestClass_inst024
        {
            public double d1 = 2d;
            public static int Main_old()
            {
                Categories_TestClass_inst024_1 Test1 = new Categories_TestClass_inst024_1();
                Categories_TestClass_inst024 Test2 = new Categories_TestClass_inst024();
                if (Test1.d1 != 1d)
                {
                    return 1;
                }
                if (Test2.d1 != 2d)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Categories_TestClass_inst026_1
        {
            public bool b1 = true;
        }
        public class Categories_TestClass_inst026
        {
            public bool b1 = false;
            public static int Main_old()
            {
                Categories_TestClass_inst026_1 Test1 = new Categories_TestClass_inst026_1();
                Categories_TestClass_inst026 Test2 = new Categories_TestClass_inst026();
                if (Test1.b1 != true)
                {
                    return 1;
                }
                if (Test2.b1 != false)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Categories_TestClass_inst028_1
        {
            public string s1 = "string1";
        }
        public class Categories_TestClass_inst028
        {
            public string s1 = "string2";
            public static int Main_old()
            {
                Categories_TestClass_inst028_1 Test1 = new Categories_TestClass_inst028_1();
                Categories_TestClass_inst028 Test2 = new Categories_TestClass_inst028();
                if (Test1.s1.Equals("string1") != true)
                {
                    return 1;
                }
                if (Test2.s1.Equals("string2") != true)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public struct Categories_TestClass_inst029_Str
        {
            public Categories_TestClass_inst029_Str(int intJ)
            {
                intI = intJ;
            }
            public int intI;
        }
        public class Categories_TestClass_inst029_1
        {
            public Categories_TestClass_inst029_Str s1 = new Categories_TestClass_inst029_Str(1);
        }
        public class Categories_TestClass_inst029
        {
            public Categories_TestClass_inst029_Str s1 = new Categories_TestClass_inst029_Str(2);
            public static int Main_old()
            {
                Categories_TestClass_inst029_1 Test1 = new Categories_TestClass_inst029_1();
                Categories_TestClass_inst029 Test2 = new Categories_TestClass_inst029();
                if (Test1.s1.intI != 1)
                {
                    return 1;
                }
                if (Test2.s1.intI != 2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public enum Categories_TestClass_inst030_En { a = 1, b = 2 }
        public class Categories_TestClass_inst030_1
        {
            public Categories_TestClass_inst030_En E = Categories_TestClass_inst030_En.a;
        }
        public class Categories_TestClass_inst030
        {
            public Categories_TestClass_inst030_En E = Categories_TestClass_inst030_En.b;
            public static int Main_old()
            {
                Categories_TestClass_inst030_1 Test1 = new Categories_TestClass_inst030_1();
                Categories_TestClass_inst030 Test2 = new Categories_TestClass_inst030();
                if (Test1.E != Categories_TestClass_inst030_En.a)
                {
                    return 1;
                }
                if (Test2.E != Categories_TestClass_inst030_En.b)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Categories_TestClass_inst031_2
        {
            public Categories_TestClass_inst031_2(int intJ)
            {
                intI = intJ;
            }
            public int intI;
        }
        public class Categories_TestClass_inst031_1
        {
            public Categories_TestClass_inst031_2 c1 = new Categories_TestClass_inst031_2(1);
        }
        public class Categories_TestClass_inst031
        {
            public Categories_TestClass_inst031_2 c1 = new Categories_TestClass_inst031_2(2);
            public static int Main_old()
            {
                Categories_TestClass_inst031_1 Test1 = new Categories_TestClass_inst031_1();
                Categories_TestClass_inst031 Test2 = new Categories_TestClass_inst031();
                if (Test1.c1.intI != 1)
                {
                    return 1;
                }
                if (Test2.c1.intI != 2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public interface Categories_TestClass_inst032_Inter
        {
            int intRet();
        }
        public class Categories_TestClass_inst032_2 : Categories_TestClass_inst032_Inter
        {
            public Categories_TestClass_inst032_2(int intJ)
            {
                intI = intJ;
            }
            public int intI;
            public int intRet()
            {
                return intI;
            }
        }
        public class Categories_TestClass_inst032_1
        {
            public Categories_TestClass_inst032_Inter i1 = (Categories_TestClass_inst032_Inter)new Categories_TestClass_inst032_2(1);
        }
        public class Categories_TestClass_inst032
        {
            public Categories_TestClass_inst032_Inter i1 = (Categories_TestClass_inst032_Inter)new Categories_TestClass_inst032_2(2);
            public static int Main_old()
            {
                Categories_TestClass_inst032_1 Test1 = new Categories_TestClass_inst032_1();
                Categories_TestClass_inst032 Test2 = new Categories_TestClass_inst032();
                if (Test1.i1.intRet() != 1)
                {
                    return 1;
                }
                if (Test2.i1.intRet() != 2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Categories_TestClass_inst033_1
        {
            public int[] i = new int[] { 1 };
        }
        public class Categories_TestClass_inst033
        {
            public int[] i = new int[] { 2 };
            public static int Main_old()
            {
                Categories_TestClass_inst033_1 Test1 = new Categories_TestClass_inst033_1();
                Categories_TestClass_inst033 Test2 = new Categories_TestClass_inst033();
                if (Test1.i[0] != 1)
                {
                    return 1;
                }
                if (Test2.i[0] != 2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public delegate int Categories_TestClass_inst034_Del();
        public class Categories_TestClass_inst034_1
        {
            public static int RetInt()
            {
                return 1;
            }
            public Categories_TestClass_inst034_Del d = new Categories_TestClass_inst034_Del(RetInt);
        }
        public class Categories_TestClass_inst034
        {
            public static int RetInt()
            {
                return 2;
            }
            public Categories_TestClass_inst034_Del d = new Categories_TestClass_inst034_Del(RetInt);
            public static int Main_old()
            {
                Categories_TestClass_inst034_1 Test1 = new Categories_TestClass_inst034_1();
                Categories_TestClass_inst034 Test2 = new Categories_TestClass_inst034();
                if (Test1.d() != 1)
                {
                    return 1;
                }
                if (Test2.d() != 2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Categories_TestClass_inst035
        {
            public byte b1;
            public char c1;
            public short s1;
            public int i1;
            public long l1;
            public float f1;
            public double d1;
            //public double m1;
            public bool b2;
            public static int Main_old()
            {
                Categories_TestClass_inst035 Test = new Categories_TestClass_inst035();
                if (Test.b1 != (byte)0)
                {
                    return 1;
                }
                if (Test.c1 != '\x0000')
                {
                    return 1;
                }
                if (Test.s1 != (short)0)
                {
                    return 1;
                }
                if (Test.i1 != 0)
                {
                    return 1;
                }
                if (Test.l1 != 0L)
                {
                    return 1;
                }
                if (Test.f1 != 0f)
                {
                    return 1;
                }
                if (Test.d1 != 0d)
                {
                    return 1;
                }
                if (Test.b2 != false)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public interface Categories_TestClass_inst036_Inter { }
        public class Categories_TestClass_inst036_2 { }
        public delegate int Categories_TestClass_inst036_Del();
        public class Categories_TestClass_inst036
        {
            public string MS;//string
            public Categories_TestClass_inst036_Inter MI;//interface
            public Categories_TestClass_inst036 MC;//class
            public int[] MA;//array
            public Categories_TestClass_inst036_Del MD;//delegate
            public static int Main_old()
            {
                Categories_TestClass_inst036 Test = new Categories_TestClass_inst036();
                if (Test.MS != null)
                {
                    return 1;
                }
                if (Test.MI != null)
                {
                    return 1;
                }
                if (Test.MC != null)
                {
                    return 1;
                }
                if (Test.MA != null)
                {
                    return 1;
                }
                if (Test.MD != null)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public enum Categories_TestClass_inst037_En { a = 1 }
        public struct Categories_TestClass_inst037_Str
        {
            public byte b1;
            public char c1;
            public short s1;
            public int i1;
            public long l1;
            public float f1;
            public double d1;
            public bool b2;
        }
        public class Categories_TestClass_inst037
        {
            Categories_TestClass_inst037_Str MS;
            Categories_TestClass_inst037_En ME;
            public static int Main_old()
            {
                Categories_TestClass_inst037 Test = new Categories_TestClass_inst037();
                if (Test.MS.b1 != (byte)0)
                {
                    return 1;
                }
                if (Test.MS.c1 != '\x0000')
                {
                    return 1;
                }
                if (Test.MS.s1 != (short)0)
                {
                    return 1;
                }
                if (Test.MS.i1 != 0)
                {
                    return 1;
                }
                if (Test.MS.l1 != 0L)
                {
                    return 1;
                }
                if (Test.MS.f1 != 0f)
                {
                    return 1;
                }
                if (Test.MS.d1 != 0d)
                {
                    return 1;
                }
                if (Test.MS.b2 != false)
                {
                    return 1;
                }
                if (Test.ME != (Categories_TestClass_inst037_En)0)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public struct Categories_TestClass_inst038_Str1
        {
            public Categories_TestClass_inst038_Str1(byte b2)
            {
                b1 = b2;
            }
            public byte b1;
        }
        public struct Categories_TestClass_inst038
        {
            public Categories_TestClass_inst038(byte b2)
            {
                b1 = b2;
            }
            public byte b1;
            public static int Main_old()
            {
                Categories_TestClass_inst038_Str1 Test1 = new Categories_TestClass_inst038_Str1((byte)1);
                Categories_TestClass_inst038 Test2 = new Categories_TestClass_inst038((byte)2);
                if (Test1.b1 != (byte)1)
                {
                    return 1;
                }
                if (Test2.b1 != (byte)2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public struct Categories_TestClass_inst039_Str1
        {
            public Categories_TestClass_inst039_Str1(char c2)
            {
                c1 = c2;
            }
            public char c1;
        }
        public struct Categories_TestClass_inst039
        {
            public Categories_TestClass_inst039(char c2)
            {
                c1 = c2;
            }
            public char c1;
            public static int Main_old()
            {
                Categories_TestClass_inst039_Str1 Test1 = new Categories_TestClass_inst039_Str1('a');
                Categories_TestClass_inst039 Test2 = new Categories_TestClass_inst039('b');
                if (Test1.c1 != 'a')
                {
                    return 1;
                }
                if (Test2.c1 != 'b')
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public struct Categories_TestClass_inst040_Str1
        {
            public Categories_TestClass_inst040_Str1(short s2)
            {
                s1 = s2;
            }
            public short s1;
        }
        public struct Categories_TestClass_inst040
        {
            public Categories_TestClass_inst040(short s2)
            {
                s1 = s2;
            }
            public short s1;
            public static int Main_old()
            {
                Categories_TestClass_inst040_Str1 Test1 = new Categories_TestClass_inst040_Str1((short)1);
                Categories_TestClass_inst040 Test2 = new Categories_TestClass_inst040((short)2);
                if (Test1.s1 != (short)1)
                {
                    return 1;
                }
                if (Test2.s1 != (short)2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public struct Categories_TestClass_inst041_Str1
        {
            public Categories_TestClass_inst041_Str1(int i2)
            {
                i1 = i2;
            }
            public int i1;
        }
        public struct Categories_TestClass_inst041
        {
            public Categories_TestClass_inst041(int i2)
            {
                i1 = i2;
            }
            public int i1;
            public static int Main_old()
            {
                Categories_TestClass_inst041_Str1 Test1 = new Categories_TestClass_inst041_Str1(1);
                Categories_TestClass_inst041 Test2 = new Categories_TestClass_inst041(2);
                if (Test1.i1 != 1)
                {
                    return 1;
                }
                if (Test2.i1 != 2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public struct Categories_TestClass_inst042_Str1
        {
            public Categories_TestClass_inst042_Str1(long l2)
            {
                l1 = l2;
            }
            public long l1;
        }
        public struct Categories_TestClass_inst042
        {
            public Categories_TestClass_inst042(long l2)
            {
                l1 = l2;
            }
            public long l1;
            public static int Main_old()
            {
                Categories_TestClass_inst042_Str1 Test1 = new Categories_TestClass_inst042_Str1(1L);
                Categories_TestClass_inst042 Test2 = new Categories_TestClass_inst042(2L);
                if (Test1.l1 != 1L)
                {
                    return 1;
                }
                if (Test2.l1 != 2L)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public struct Categories_TestClass_inst043_Str1
        {
            public Categories_TestClass_inst043_Str1(float f2)
            {
                f1 = f2;
            }
            public float f1;
        }
        public struct Categories_TestClass_inst043
        {
            public Categories_TestClass_inst043(float f2)
            {
                f1 = f2;
            }
            public float f1;
            public static int Main_old()
            {
                Categories_TestClass_inst043_Str1 Test1 = new Categories_TestClass_inst043_Str1(1f);
                Categories_TestClass_inst043 Test2 = new Categories_TestClass_inst043(2f);
                if (Test1.f1 != 1f)
                {
                    return 1;
                }
                if (Test2.f1 != 2f)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public struct Categories_TestClass_inst044_Str1
        {
            public Categories_TestClass_inst044_Str1(double d2)
            {
                d1 = d2;
            }
            public double d1;
        }
        public struct Categories_TestClass_inst044
        {
            public Categories_TestClass_inst044(double d2)
            {
                d1 = d2;
            }
            public double d1;
            public static int Main_old()
            {
                Categories_TestClass_inst044_Str1 Test1 = new Categories_TestClass_inst044_Str1(1d);
                Categories_TestClass_inst044 Test2 = new Categories_TestClass_inst044(2d);
                if (Test1.d1 != 1d)
                {
                    return 1;
                }
                if (Test2.d1 != 2d)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public struct Categories_TestClass_inst046_Str1
        {
            public Categories_TestClass_inst046_Str1(bool b2)
            {
                b1 = b2;
            }
            public bool b1;
        }
        public struct Categories_TestClass_inst046
        {
            public Categories_TestClass_inst046(bool b2)
            {
                b1 = b2;
            }
            public bool b1;
            public static int Main_old()
            {
                Categories_TestClass_inst046_Str1 Test1 = new Categories_TestClass_inst046_Str1(true);
                Categories_TestClass_inst046 Test2 = new Categories_TestClass_inst046(false);
                if (Test1.b1 != true)
                {
                    return 1;
                }
                if (Test2.b1 != false)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public struct Categories_TestClass_inst048_Str1
        {
            public Categories_TestClass_inst048_Str1(string s2)
            {
                s1 = s2;
            }
            public string s1;
        }
        public struct Categories_TestClass_inst048
        {
            public Categories_TestClass_inst048(string s2)
            {
                s1 = s2;
            }
            public string s1;
            public static int Main_old()
            {
                Categories_TestClass_inst048_Str1 Test1 = new Categories_TestClass_inst048_Str1("string1");
                Categories_TestClass_inst048 Test2 = new Categories_TestClass_inst048("string2");
                if (Test1.s1.Equals("string1") != true)
                {
                    return 1;
                }
                if (Test2.s1.Equals("string2") != true)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public struct Categories_TestClass_inst049_Str
        {
            public Categories_TestClass_inst049_Str(int intJ)
            {
                intI = intJ;
            }
            public int intI;
        }
        public struct Categories_TestClass_inst049_Str1
        {
            public Categories_TestClass_inst049_Str1(int intI)
            {
                s1 = new Categories_TestClass_inst049_Str(1);
            }
            public Categories_TestClass_inst049_Str s1;
        }
        public struct Categories_TestClass_inst049
        {
            public Categories_TestClass_inst049(int intI)
            {
                s1 = new Categories_TestClass_inst049_Str(2);
            }
            public Categories_TestClass_inst049_Str s1;
            public static int Main_old()
            {
                Categories_TestClass_inst049_Str1 Test1 = new Categories_TestClass_inst049_Str1(0);
                Categories_TestClass_inst049 Test2 = new Categories_TestClass_inst049(0);
                if (Test1.s1.intI != 1)
                {
                    return 1;
                }
                if (Test2.s1.intI != 2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public enum Categories_TestClass_inst050_En { a = 1, b = 2 }
        public struct Categories_TestClass_inst050_Str1
        {
            public Categories_TestClass_inst050_Str1(int intI)
            {
                E = Categories_TestClass_inst050_En.a;
            }
            public Categories_TestClass_inst050_En E;
        }
        public struct Categories_TestClass_inst050
        {
            public Categories_TestClass_inst050(int intI)
            {
                E = Categories_TestClass_inst050_En.b;
            }
            public Categories_TestClass_inst050_En E;
            public static int Main_old()
            {
                Categories_TestClass_inst050_Str1 Test1 = new Categories_TestClass_inst050_Str1(0);
                Categories_TestClass_inst050 Test2 = new Categories_TestClass_inst050(0);
                if (Test1.E != Categories_TestClass_inst050_En.a)
                {
                    return 1;
                }
                if (Test2.E != Categories_TestClass_inst050_En.b)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Categories_TestClass_inst051_C
        {
            public Categories_TestClass_inst051_C(int intJ)
            {
                intI = intJ;
            }
            public int intI;
        }
        public struct Categories_TestClass_inst051_Str1
        {
            public Categories_TestClass_inst051_Str1(int intI)
            {
                c1 = new Categories_TestClass_inst051_C(1);
            }
            public Categories_TestClass_inst051_C c1;
        }
        public struct Categories_TestClass_inst051
        {
            public Categories_TestClass_inst051(int intI)
            {
                c1 = new Categories_TestClass_inst051_C(2);
            }
            public Categories_TestClass_inst051_C c1;
            public static int Main_old()
            {
                Categories_TestClass_inst051_Str1 Test1 = new Categories_TestClass_inst051_Str1(0);
                Categories_TestClass_inst051 Test2 = new Categories_TestClass_inst051(0);
                if (Test1.c1.intI != 1)
                {
                    return 1;
                }
                if (Test2.c1.intI != 2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public interface Categories_TestClass_inst052_Inter
        {
            int intRet();
        }
        public class Categories_TestClass_inst052_C : Categories_TestClass_inst052_Inter
        {
            public Categories_TestClass_inst052_C(int intJ)
            {
                intI = intJ;
            }
            public int intI;
            public int intRet()
            {
                return intI;
            }
        }
        public struct Categories_TestClass_inst052_Str1
        {
            public Categories_TestClass_inst052_Str1(int intI)
            {
                i1 = (Categories_TestClass_inst052_Inter)new Categories_TestClass_inst052_C(1);
            }

            public Categories_TestClass_inst052_Inter i1;
        }
        public struct Categories_TestClass_inst052
        {
            public Categories_TestClass_inst052(int intI)
            {
                i1 = (Categories_TestClass_inst052_Inter)new Categories_TestClass_inst052_C(2);
            }
            public Categories_TestClass_inst052_Inter i1;
            public static int Main_old()
            {
                Categories_TestClass_inst052_Str1 Test1 = new Categories_TestClass_inst052_Str1(0);
                Categories_TestClass_inst052 Test2 = new Categories_TestClass_inst052(0);
                if (Test1.i1.intRet() != 1)
                {
                    return 1;
                }
                if (Test2.i1.intRet() != 2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public struct Categories_TestClass_inst053_Str1
        {
            public Categories_TestClass_inst053_Str1(int intI)
            {
                i = new int[] { 1 };
            }

            public int[] i;
        }
        public struct Categories_TestClass_inst053
        {
            public Categories_TestClass_inst053(int intI)
            {
                i = new int[] { 2 };
            }
            public int[] i;
            public static int Main_old()
            {
                Categories_TestClass_inst053_Str1 Test1 = new Categories_TestClass_inst053_Str1(0);
                Categories_TestClass_inst053 Test2 = new Categories_TestClass_inst053(0);
                if (Test1.i[0] != 1)
                {
                    return 1;
                }
                if (Test2.i[0] != 2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public delegate int Categories_TestClass_inst054_Del();
        public struct Categories_TestClass_inst054_Str1
        {
            public Categories_TestClass_inst054_Str1(int intI)
            {
                d = new Categories_TestClass_inst054_Del(RetInt);
            }

            public Categories_TestClass_inst054_Del d;
            public static int RetInt()
            {
                return 1;
            }
        }
        public struct Categories_TestClass_inst054
        {
            public Categories_TestClass_inst054(int intI)
            {
                d = new Categories_TestClass_inst054_Del(RetInt);
            }
            public Categories_TestClass_inst054_Del d;
            public static int RetInt()
            {
                return 2;
            }
            public static int Main_old()
            {
                Categories_TestClass_inst054_Str1 Test1 = new Categories_TestClass_inst054_Str1(0);
                Categories_TestClass_inst054 Test2 = new Categories_TestClass_inst054(0);
                if (Test1.d() != 1)
                {
                    return 1;
                }
                if (Test2.d() != 2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }


        


    }
}
