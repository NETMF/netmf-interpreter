////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class MembersTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");

            // Add your functionality here.       

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
        }

        //Members Tests
        //The following tests ported from folder current\test\cases\client\CLR\Conformance\10_classes\Members
        //023,024,025,026,027,028
        //The following tests ported from folder current\test\cases\client\CLR\Conformance\10_classes\Members\Inheritance
        //001,002,003,004,005,006,007,008,018,019,020,021,022,023,024,025,026,027,028,029,030,031,032,033,034,035,036,037,038,039,040,041,042,043,044,045,046,047,053,054,057,058,059
        //The following tests ported from folder current\test\cases\client\CLR\Conformance\10_classes\Members\Modifiers
        //01,02,03,04,05,06,07,08,12,23,24,25,26,27
        //All tests that were not ported did not appear in the Baseline Document

        [TestMethod]
        public MFTestResults Members23_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" The inherited members of a class are specifically");
            Log.Comment(" not part of the declaration space of a class.  Thus, a ");
            Log.Comment(" derived class is allowed to declare a member with the same ");
            Log.Comment(" name or signature as an inherited member (which in effect");
            Log.Comment(" hides the inherited member).");
            if (MembersTestClass023.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Members24_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" The inherited members of a class are specifically");
            Log.Comment(" not part of the declaration space of a class.  Thus, a ");
            Log.Comment(" derived class is allowed to declare a member with the same ");
            Log.Comment(" name or signature as an inherited member (which in effect");
            Log.Comment(" hides the inherited member).");
            if (MembersTestClass024.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Members25_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" The inherited members of a class are specifically");
            Log.Comment(" not part of the declaration space of a class.  Thus, a ");
            Log.Comment(" derived class is allowed to declare a member with the same ");
            Log.Comment(" name or signature as an inherited member (which in effect");
            Log.Comment(" hides the inherited member).");
            if (MembersTestClass025.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Members26_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" The inherited members of a class are specifically");
            Log.Comment(" not part of the declaration space of a class.  Thus, a ");
            Log.Comment(" derived class is allowed to declare a member with the same ");
            Log.Comment(" name or signature as an inherited member (which in effect");
            Log.Comment(" hides the inherited member).");
            if (MembersTestClass026.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Members27_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" The inherited members of a class are specifically");
            Log.Comment(" not part of the declaration space of a class.  Thus, a ");
            Log.Comment(" derived class is allowed to declare a member with the same ");
            Log.Comment(" name or signature as an inherited member (which in effect");
            Log.Comment(" hides the inherited member).");
            if (MembersTestClass027.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Members28_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" The inherited members of a class are specifically");
            Log.Comment(" not part of the declaration space of a class.  Thus, a ");
            Log.Comment(" derived class is allowed to declare a member with the same ");
            Log.Comment(" name or signature as an inherited member (which in effect");
            Log.Comment(" hides the inherited member).");
            if (MembersTestClass028.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        //Test Case Calls 
        [TestMethod]
        public MFTestResults MembersInheritance001_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" Inheritance is transitive. If C is derived from");
            Log.Comment(" B, and B is derived from A, then C inherits the");
            Log.Comment(" members declared in B as well as the members");
            if (MembersInheritanceTestClass001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance002_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" Inheritance is transitive. If C is derived from");
            Log.Comment(" B, and B is derived from A, then C inherits the");
            Log.Comment(" members declared in B as well as the members");
            if (MembersInheritanceTestClass002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance003_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" Inheritance is transitive. If C is derived from");
            Log.Comment(" B, and B is derived from A, then C inherits the");
            Log.Comment(" members declared in B as well as the members");
            if (MembersInheritanceTestClass003.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance004_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" Inheritance is transitive. If C is derived from");
            Log.Comment(" B, and B is derived from A, then C inherits the");
            Log.Comment(" members declared in B as well as the members");
            if (MembersInheritanceTestClass004.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance005_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" Inheritance is transitive. If C is derived from");
            Log.Comment(" B, and B is derived from A, then C inherits the");
            Log.Comment(" members declared in B as well as the members");
            if (MembersInheritanceTestClass005.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance006_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" Inheritance is transitive. If C is derived from");
            Log.Comment(" B, and B is derived from A, then C inherits the");
            Log.Comment(" members declared in B as well as the members");
            if (MembersInheritanceTestClass006.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance007_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" Inheritance is transitive. If C is derived from");
            Log.Comment(" B, and B is derived from A, then C inherits the");
            Log.Comment(" members declared in B as well as the members");
            if (MembersInheritanceTestClass007.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance008_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" Inheritance is transitive. If C is derived from");
            Log.Comment(" B, and B is derived from A, then C inherits the");
            Log.Comment(" members declared in B as well as the members");
            if (MembersInheritanceTestClass008.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance018_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" Constructors and destructors are not inherited, but all");
            Log.Comment(" other members are, regardless of their declared accessibility.");
            Log.Comment(" However, depending on their declared accessibility, inherited");
            Log.Comment(" members may not be accessible in the derived class.");
            if (MembersInheritanceTestClass018.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance019_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" Constructors and destructors are not inherited, but all");
            Log.Comment(" other members are, regardless of their declared accessibility.");
            Log.Comment(" However, depending on their declared accessibility, inherited");
            Log.Comment(" members may not be accessible in the derived class.");
            if (MembersInheritanceTestClass019.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance020_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" Constructors and destructors are not inherited, but all");
            Log.Comment(" other members are, regardless of their declared accessibility.");
            Log.Comment(" However, depending on their declared accessibility, inherited");
            Log.Comment(" members may not be accessible in the derived class.");
            if (MembersInheritanceTestClass020.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance021_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" Constructors and destructors are not inherited, but all");
            Log.Comment(" other members are, regardless of their declared accessibility.");
            Log.Comment(" However, depending on their declared accessibility, inherited");
            Log.Comment(" members may not be accessible in the derived class.");
            if (MembersInheritanceTestClass021.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance022_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" Constructors and destructors are not inherited, but all");
            Log.Comment(" other members are, regardless of their declared accessibility.");
            Log.Comment(" However, depending on their declared accessibility, inherited");
            Log.Comment(" members may not be accessible in the derived class.");
            if (MembersInheritanceTestClass022.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance023_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" Constructors and destructors are not inherited, but all");
            Log.Comment(" other members are, regardless of their declared accessibility.");
            Log.Comment(" However, depending on their declared accessibility, inherited");
            Log.Comment(" members may not be accessible in the derived class.");
            if (MembersInheritanceTestClass023.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance024_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" Constructors and destructors are not inherited, but all");
            Log.Comment(" other members are, regardless of their declared accessibility.");
            Log.Comment(" However, depending on their declared accessibility, inherited");
            Log.Comment(" members may not be accessible in the derived class.");
            if (MembersInheritanceTestClass024.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance025_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" Constructors and destructors are not inherited, but all");
            Log.Comment(" other members are, regardless of their declared accessibility.");
            Log.Comment(" However, depending on their declared accessibility, inherited");
            Log.Comment(" members may not be accessible in the derived class.");
            if (MembersInheritanceTestClass025.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance026_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" An instance of a class contains a copy of all instance fields");
            Log.Comment(" declared in the class and its base classes, and an implicit");
            Log.Comment(" conversion exists from a derived class type to any of its base ");
            Log.Comment(" class types.  Thus, a reference to a derived class instance");
            Log.Comment(" can be treated as a reference to a base class instance.");
            if (MembersInheritanceTestClass026.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance027_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" An instance of a class contains a copy of all instance fields");
            Log.Comment(" declared in the class and its base classes, and an implicit");
            Log.Comment(" conversion exists from a derived class type to any of its base ");
            Log.Comment(" class types.  Thus, a reference to a derived class instance");
            Log.Comment(" can be treated as a reference to a base class instance.");
            if (MembersInheritanceTestClass027.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance028_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" A class can declare virtual methods, properties,");
            Log.Comment(" and indexers, and derived classes can override the ");
            Log.Comment(" implementation of these function members.  This enables");
            Log.Comment(" classes to exhibit polymorphic behavior wherein the ");
            Log.Comment(" actions performed by a function member invocation");
            Log.Comment(" varies depending on the run-time type of the instance");
            Log.Comment(" through which the member is invoked.");
            if (MembersInheritanceTestClass028.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance029_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" A class can declare virtual methods, properties,");
            Log.Comment(" and indexers, and derived classes can override the ");
            Log.Comment(" implementation of these function members.  This enables");
            Log.Comment(" classes to exhibit polymorphic behavior wherein the ");
            Log.Comment(" actions performed by a function member invocation");
            Log.Comment(" varies depending on the run-time type of the instance");
            Log.Comment(" through which the member is invoked.");
            if (MembersInheritanceTestClass029.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance030_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" A class can declare virtual methods, properties,");
            Log.Comment(" and indexers, and derived classes can override the ");
            Log.Comment(" implementation of these function members.  This enables");
            Log.Comment(" classes to exhibit polymorphic behavior wherein the ");
            Log.Comment(" actions performed by a function member invocation");
            Log.Comment(" varies depending on the run-time type of the instance");
            Log.Comment(" through which the member is invoked.");
            if (MembersInheritanceTestClass030.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance031_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" A class can declare virtual methods, properties,");
            Log.Comment(" and indexers, and derived classes can override the ");
            Log.Comment(" implementation of these function members.  This enables");
            Log.Comment(" classes to exhibit polymorphic behavior wherein the ");
            Log.Comment(" actions performed by a function member invocation");
            Log.Comment(" varies depending on the run-time type of the instance");
            Log.Comment(" through which the member is invoked.");
            if (MembersInheritanceTestClass031.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance032_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" A class can declare virtual methods, properties,");
            Log.Comment(" and indexers, and derived classes can override the ");
            Log.Comment(" implementation of these function members.  This enables");
            Log.Comment(" classes to exhibit polymorphic behavior wherein the ");
            Log.Comment(" actions performed by a function member invocation");
            Log.Comment(" varies depending on the run-time type of the instance");
            Log.Comment(" through which the member is invoked.");
            if (MembersInheritanceTestClass032.testMethod())
            {
                Log.Comment("This failure indicates a test is now passing that previously failed by design.");
                Log.Comment("It previously marked as known failure because of bug # 21562");
                Log.Comment("The Test owner needs to verify that the change was intentional and remove the known failure.");
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;
        }
        [TestMethod]
        public MFTestResults MembersInheritance033_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" A class can declare virtual methods, properties,");
            Log.Comment(" and indexers, and derived classes can override the ");
            Log.Comment(" implementation of these function members.  This enables");
            Log.Comment(" classes to exhibit polymorphic behavior wherein the ");
            Log.Comment(" actions performed by a function member invocation");
            Log.Comment(" varies depending on the run-time type of the instance");
            Log.Comment(" through which the member is invoked.");
            if (MembersInheritanceTestClass033.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance034_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" A class can declare virtual methods, properties,");
            Log.Comment(" and indexers, and derived classes can override the ");
            Log.Comment(" implementation of these function members.  This enables");
            Log.Comment(" classes to exhibit polymorphic behavior wherein the ");
            Log.Comment(" actions performed by a function member invocation");
            Log.Comment(" varies depending on the run-time type of the instance");
            Log.Comment(" through which the member is invoked.");
            if (MembersInheritanceTestClass034.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance035_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" A class can declare virtual methods, properties,");
            Log.Comment(" and indexers, and derived classes can override the ");
            Log.Comment(" implementation of these function members.  This enables");
            Log.Comment(" classes to exhibit polymorphic behavior wherein the ");
            Log.Comment(" actions performed by a function member invocation");
            Log.Comment(" varies depending on the run-time type of the instance");
            Log.Comment(" through which the member is invoked.");
            if (MembersInheritanceTestClass035.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance036_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" A class can declare virtual methods, properties,");
            Log.Comment(" and indexers, and derived classes can override the ");
            Log.Comment(" implementation of these function members.  This enables");
            Log.Comment(" classes to exhibit polymorphic behavior wherein the ");
            Log.Comment(" actions performed by a function member invocation");
            Log.Comment(" varies depending on the run-time type of the instance");
            Log.Comment(" through which the member is invoked.");
            if (MembersInheritanceTestClass036.testMethod())
            {
                Log.Comment("This failure indicates a test is now passing that previously failed by design.");
                Log.Comment("It previously marked as known failure because of bug # 21562");
                Log.Comment("The Test owner needs to verify that the change was intentional and remove the known failure.");
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;
        }
        [TestMethod]
        public MFTestResults MembersInheritance037_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" A class-member-declaration is permitted to declare");
            Log.Comment(" a member with the same name or signature as an ");
            Log.Comment(" inherited member.  When this occurs, the derived");
            Log.Comment(" class member is said to hide the base class member.");
            if (MembersInheritanceTestClass037.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance038_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" A class-member-declaration is permitted to declare");
            Log.Comment(" a member with the same name or signature as an ");
            Log.Comment(" inherited member.  When this occurs, the derived");
            Log.Comment(" class member is said to hide the base class member.");
            if (MembersInheritanceTestClass038.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance039_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" A class-member-declaration is permitted to declare");
            Log.Comment(" a member with the same name or signature as an ");
            Log.Comment(" inherited member.  When this occurs, the derived");
            Log.Comment(" class member is said to hide the base class member.");
            if (MembersInheritanceTestClass039.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance040_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" A class-member-declaration is permitted to declare");
            Log.Comment(" a member with the same name or signature as an ");
            Log.Comment(" inherited member.  When this occurs, the derived");
            Log.Comment(" class member is said to hide the base class member.");
            if (MembersInheritanceTestClass040.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance041_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" If a new modifier is included in a declaration");
            Log.Comment(" that doesn't hide an inherited member, a warning ");
            Log.Comment(" is issued to that effect.");
            if (MembersInheritanceTestClass041.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance042_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" If a new modifier is included in a declaration");
            Log.Comment(" that doesn't hide an inherited member, a warning ");
            Log.Comment(" is issued to that effect.");
            if (MembersInheritanceTestClass042.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance043_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" If a new modifier is included in a declaration");
            Log.Comment(" that doesn't hide an inherited member, a warning ");
            Log.Comment(" is issued to that effect.");
            if (MembersInheritanceTestClass043.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance044_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" If a new modifier is included in a declaration");
            Log.Comment(" that doesn't hide an inherited member, a warning ");
            Log.Comment(" is issued to that effect.");
            if (MembersInheritanceTestClass044.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance045_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" If a new modifier is included in a declaration");
            Log.Comment(" that doesn't hide an inherited member, a warning ");
            Log.Comment(" is issued to that effect.");
            if (MembersInheritanceTestClass045.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance046_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" If a new modifier is included in a declaration");
            Log.Comment(" that doesn't hide an inherited member, a warning ");
            Log.Comment(" is issued to that effect.");
            if (MembersInheritanceTestClass046.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance047_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" If a new modifier is included in a declaration");
            Log.Comment(" that doesn't hide an inherited member, a warning ");
            Log.Comment(" is issued to that effect.");
            if (MembersInheritanceTestClass047.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance053_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" If a new modifier is included in a declaration");
            Log.Comment(" that doesn't hide an inherited member, a warning ");
            Log.Comment(" is issued to that effect.");
            if (MembersInheritanceTestClass053.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance054_Test()
        {
            Log.Comment("Testing that protected members can be passed to a grandchild class");
            if (MembersInheritanceTestClass054.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance057_Test()
        {
            Log.Comment("Testing that you can inherit from a member class");

            if (MembersInheritanceTestClass057.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance058_Test()
        {
            Log.Comment("Testing that you can inherit from a class declared later in the file");
            if (MembersInheritanceTestClass058.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersInheritance059_Test()
        {
            Log.Comment("Testing that an inner class inherit from another class");
            if (MembersInheritanceTestClass059.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        //Test Case Calls 
        [TestMethod]
        public MFTestResults MembersModifiers01_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" A class-member-declaration may include one of the ");
            Log.Comment(" four access modifiers: public, protected, internal,");
            Log.Comment(" or private. It is an error to specify more than one");
            Log.Comment(" access modifier. When a class-member-declaration ");
            Log.Comment(" does not include an access modifier, the declaration");
            Log.Comment(" defaults to private.");
            if (MembersModifiersTestClass01.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersModifiers02_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" A class-member-declaration may include one of the ");
            Log.Comment(" four access modifiers: public, protected, internal,");
            Log.Comment(" or private. It is an error to specify more than one");
            Log.Comment(" access modifier. When a class-member-declaration ");
            Log.Comment(" does not include an access modifier, the declaration");
            Log.Comment(" defaults to private.");
            if (MembersModifiersTestClass02.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersModifiers03_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" A class-member-declaration may include one of the ");
            Log.Comment(" four access modifiers: public, protected, internal,");
            Log.Comment(" or private. It is an error to specify more than one");
            Log.Comment(" access modifier. When a class-member-declaration ");
            Log.Comment(" does not include an access modifier, the declaration");
            Log.Comment(" defaults to private.");
            if (MembersModifiersTestClass03.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersModifiers04_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" A class-member-declaration may include one of the ");
            Log.Comment(" four access modifiers: public, protected, internal,");
            Log.Comment(" or private. It is an error to specify more than one");
            Log.Comment(" access modifier. When a class-member-declaration ");
            Log.Comment(" does not include an access modifier, the declaration");
            Log.Comment(" defaults to private.");
            if (MembersModifiersTestClass04.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersModifiers05_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" A class-member-declaration may include one of the ");
            Log.Comment(" four access modifiers: public, protected, internal,");
            Log.Comment(" or private. It is an error to specify more than one");
            Log.Comment(" access modifier. When a class-member-declaration ");
            Log.Comment(" does not include an access modifier, the declaration");
            Log.Comment(" defaults to private.");
            if (MembersModifiersTestClass05.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersModifiers06_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" A class-member-declaration may include one of the ");
            Log.Comment(" four access modifiers: public, protected, internal,");
            Log.Comment(" or private. It is an error to specify more than one");
            Log.Comment(" access modifier. When a class-member-declaration ");
            Log.Comment(" does not include an access modifier, the declaration");
            Log.Comment(" defaults to private.");
            if (MembersModifiersTestClass06.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersModifiers07_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" A class-member-declaration may include one of the ");
            Log.Comment(" four access modifiers: public, protected, internal,");
            Log.Comment(" or private. It is an error to specify more than one");
            Log.Comment(" access modifier. When a class-member-declaration ");
            Log.Comment(" does not include an access modifier, the declaration");
            Log.Comment(" defaults to private.");
            if (MembersModifiersTestClass07.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersModifiers08_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" A class-member-declaration may include one of the ");
            Log.Comment(" four access modifiers: public, protected, internal,");
            Log.Comment(" or private. It is an error to specify more than one");
            Log.Comment(" access modifier. When a class-member-declaration ");
            Log.Comment(" does not include an access modifier, the declaration");
            Log.Comment(" defaults to private.");
            if (MembersModifiersTestClass08.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersModifiers12_Test()
        {
            Log.Comment(" Section 10.2");
            Log.Comment(" A class-member-declaration may include one of the ");
            Log.Comment(" four access modifiers: public, protected, internal,");
            Log.Comment(" or private. It is an error to specify more than one");
            Log.Comment(" access modifier. When a class-member-declaration ");
            Log.Comment(" does not include an access modifier, the declaration");
            Log.Comment(" defaults to private.");
            if (MembersModifiersTestClass12.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersModifiers23_Test()
        {
            if (MembersModifiersTestClass23.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersModifiers24_Test()
        {
            if (MembersModifiersTestClass24.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersModifiers25_Test()
        {
            if (MembersModifiersTestClass25.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersModifiers26_Test()
        {
            if (MembersModifiersTestClass26.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults MembersModifiers27_Test()
        {
            if (MembersModifiersTestClass27.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }



        //Compiled Test Cases 
        class MembersTestClass_Base023
        {
            public const int intI = 1;
        }
        class MembersTestClass023 : MembersTestClass_Base023
        {
            public const int intI = 2;
            public static bool testMethod()
            {
                if (intI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        class MembersTestClass_Base024
        {
            public int intI = 1;
        }
        class MembersTestClass024 : MembersTestClass_Base024
        {
            public int intI = 2;
            public static bool testMethod()
            {
                MembersTestClass024 test = new MembersTestClass024();
                if (test.intI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        class MembersTestClass_Base025
        {
            public int intI
            {
                get
                {
                    return 1;
                }
            }
        }
        class MembersTestClass025 : MembersTestClass_Base025
        {
            public int intI
            {
                get
                {
                    return 2;
                }
            }
            public static bool testMethod()
            {
                MembersTestClass025 test = new MembersTestClass025();
                if (test.intI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        class MembersTestClass_Base026
        {
            public class MyInner
            {
                public static int intI = 1;
            }
        }
        class MembersTestClass026 : MembersTestClass_Base026
        {
            public class MyInner
            {
                public static int intI = 2;
            }
            public static bool testMethod()
            {
                if (MyInner.intI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        class MembersTestClass_Base027
        {
            public int intI()
            {
                return 1;
            }
        }
        class MembersTestClass027 : MembersTestClass_Base027
        {
            public int intI()
            {
                return 2;
            }
            public static bool testMethod()
            {
                MembersTestClass027 test = new MembersTestClass027();
                if (test.intI() == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        class MembersTestClass_Base028
        {
            public int this[int I]
            {
                get
                {
                    return 1;
                }
            }
        }
        class MembersTestClass028 : MembersTestClass_Base028
        {
            public int this[int I]
            {
                get
                {
                    return 2;
                }
            }
            public static bool testMethod()
            {
                MembersTestClass028 test = new MembersTestClass028();
                if (test[9] == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        //Compiled Test Cases 

        class MembersInheritanceTestClass001_SubA {
        }
        class MembersInheritanceTestClass001_SubB : MembersInheritanceTestClass001_SubA {
	        public int intI = 1;
	        protected String strS = "Class B";
	        protected int intJ() {return 2;}
	        public static int intK = 3;
        }
        class MembersInheritanceTestClass001 : MembersInheritanceTestClass001_SubB {
	        public static bool testMethod() {
                MembersInheritanceTestClass001 test = new MembersInheritanceTestClass001();
		        if ((test.intI ==1) && (test.intJ() == 2) && (intK == 3) && (test.strS.Equals("Class B"))) {
			        return true;
		        }
		        else {
			        return false;
		        }			
	        }
        }

        class MembersInheritanceTestClass002_SubA {
	        public int intI = 1;
	        protected int intJ() {return 2;}
	        public static int intK = 3;
	        protected String strS = "Class A";
        }
        class MembersInheritanceTestClass002_SubB : MembersInheritanceTestClass002_SubA {
        }
        class MembersInheritanceTestClass002 : MembersInheritanceTestClass002_SubB {
	        public static bool testMethod() {
                MembersInheritanceTestClass002 test = new MembersInheritanceTestClass002();
		        if ((test.intI ==1) && (test.intJ() == 2) && (intK == 3) && (test.strS.Equals("Class A"))) {
			        return true;
		        }
		        else {
			        return false;
		        }			
	        }
        }
        class MembersInheritanceTestClass003_SubA {
        }
        class MembersInheritanceTestClass003_SubB : MembersInheritanceTestClass003_SubA {
	        protected struct MyStruct {
		        public int intI;
		        public MyStruct(int intJ) {
			        intI = intJ;
		        }
	        }
        }
        class MembersInheritanceTestClass003 : MembersInheritanceTestClass003_SubB {
	        public static bool testMethod() {
		        MyStruct test = new MyStruct(3);		
		        if (test.intI == 3) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        class MembersInheritanceTestClass004_SubA {
	        protected struct MyStruct {
		        public int intI;
		        public MyStruct(int intJ) {
			        intI = intJ;
		        }
	        }
        }
        class MembersInheritanceTestClass004_SubB : MembersInheritanceTestClass004_SubA {
        }
        class MembersInheritanceTestClass004 : MembersInheritanceTestClass004_SubB {
	        public static bool testMethod() {
		        MyStruct test = new MyStruct(3);		
		        if (test.intI == 3) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        class MembersInheritanceTestClass005_SubA {
        }
        class MembersInheritanceTestClass005_SubB : MembersInheritanceTestClass005_SubA {
	        protected enum AA {zero, one}
        }
        class MembersInheritanceTestClass005 : MembersInheritanceTestClass005_SubB {
	        public static bool testMethod() {
		        AA MyEnum = AA.one;	
		        if ((int)MyEnum == 1) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        class MembersInheritanceTestClass006_SubA {
	        protected enum AA {zero, one}
        }
        class MembersInheritanceTestClass006_SubB : MembersInheritanceTestClass006_SubA {
        }
        class MembersInheritanceTestClass006 : MembersInheritanceTestClass006_SubB {
	        public static bool testMethod() {
		        AA MyEnum = AA.one;	
		        if ((int)MyEnum == 1) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        class MembersInheritanceTestClass007_SubA {
        }
        class MembersInheritanceTestClass007_SubB : MembersInheritanceTestClass007_SubA {
	        protected class MembersInheritanceTestClass007 {
		        public int intI;
		        public MembersInheritanceTestClass007(int intJ) {
			        intI = intJ;
		        }
	        }
        }
        class MembersInheritanceTestClass007 : MembersInheritanceTestClass007_SubB {
	        public static bool testMethod() {
		        MembersInheritanceTestClass007 test = new MembersInheritanceTestClass007(3);		
		        if (test.intI == 3) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        class MembersInheritanceTestClass008_SubA {
	        protected class MembersInheritanceTestClass008 {
		        public int intI;
		        public MembersInheritanceTestClass008(int intJ) {
			        intI = intJ;
		        }
	        }
        }
        class MembersInheritanceTestClass008_SubB : MembersInheritanceTestClass008_SubA {
        }
        class MembersInheritanceTestClass008 : MembersInheritanceTestClass008_SubB {
	        public static bool testMethod() {
		        MembersInheritanceTestClass008 test = new MembersInheritanceTestClass008(3);		
		        if (test.intI == 3) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        class MembersInheritanceTestClass018_Base {
	        public int intI = 1;
	        protected String strS = "MembersInheritanceTestClass018_Base";
	        protected int intJ() {return 2;}
	        public static int intK = 3;
        }
        class MembersInheritanceTestClass018 : MembersInheritanceTestClass018_Base {
	        public static bool testMethod() {
		        MembersInheritanceTestClass018 test = new MembersInheritanceTestClass018();
		        if ((test.intI ==1) && (test.intJ() == 2) && (intK == 3) && (test.strS.Equals("MembersInheritanceTestClass018_Base"))) {
			        return true;
		        }
		        else {
			        return false;
		        }			
	        }
        }
        class MembersInheritanceTestClass019_Base {
	        public int intI = 1;
        }
        class MembersInheritanceTestClass019 : MembersInheritanceTestClass019_Base {
	        new public int intI = 2;
	        public static bool testMethod() {
		        MembersInheritanceTestClass019 test = new MembersInheritanceTestClass019();
		        if (test.intI == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        class MembersInheritanceTestClass020_Base {
	        protected int intI() {return 2;}
        }
        class MembersInheritanceTestClass020 : MembersInheritanceTestClass020_Base {
	        new protected int intI() {return 3;}
	        public static bool testMethod() {
		        MembersInheritanceTestClass020 test = new MembersInheritanceTestClass020();
		        if (test.intI() == 3) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        class MembersInheritanceTestClass021_Base {
	        public static int intI = 1;
        }
        class MembersInheritanceTestClass021 : MembersInheritanceTestClass021_Base {
	        new public static int intI = 2;
	        public static bool testMethod() {
		        MembersInheritanceTestClass021 test = new MembersInheritanceTestClass021();
		        if (intI == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        class MembersInheritanceTestClass022_Base {
	        public String strS = "MembersInheritanceTestClass022_Base";
        }
        class MembersInheritanceTestClass022 : MembersInheritanceTestClass022_Base {
	        new public static String strS = "MembersInheritanceTestClass022";
	        public static bool testMethod() {
		        MembersInheritanceTestClass022 test = new MembersInheritanceTestClass022();
		        if (strS.Equals("MembersInheritanceTestClass022")) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        class MembersInheritanceTestClass023_Base {
	        public struct MyStruct {
		        public static int intI = 1;
	        }
        }
        class MembersInheritanceTestClass023 : MembersInheritanceTestClass023_Base {
	        new struct MyStruct {
		        public static int intI = 2;
	        }
	        public static bool testMethod() {
		        if (MyStruct.intI == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }		
	        }
        }
        class MembersInheritanceTestClass024_Base {
	        public enum AA {one, two}
        }
        class MembersInheritanceTestClass024 : MembersInheritanceTestClass024_Base {
	        new public enum AA {zero, one}
	        public static bool testMethod() {
        
		        AA MyEnum = AA.one;
		        if ((int)MyEnum == 1) {
			        return true;
		        }
		        else {
			        return false;
		        }		
	        }
        }
        class MembersInheritanceTestClass025_Base {
	        public class MyInner {
		        public static int intI = 1;
	        }
        }
        class MembersInheritanceTestClass025 : MembersInheritanceTestClass025_Base {
	        new class MyInner {
		        public static int intI = 2;
	        }
	        public static bool testMethod() {
		        if (MyInner.intI == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }		
	        }
        }
        class MembersInheritanceTestClass026_Base {
	        public int intI = 1;
        }
        class MembersInheritanceTestClass026 : MembersInheritanceTestClass026_Base {
	        public static bool testMethod() {
		        MembersInheritanceTestClass026_Base test = new MembersInheritanceTestClass026();
		        if (test.intI == 1) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        class MembersInheritanceTestClass027_Base1 {
	        public int intI = 1;
        }
        class MembersInheritanceTestClass027_Base2 : MembersInheritanceTestClass027_Base1 {
        }
        class MembersInheritanceTestClass027 : MembersInheritanceTestClass027_Base2 {
	        public static bool testMethod() {
		        MembersInheritanceTestClass027_Base1 test = new MembersInheritanceTestClass027();
		        if (test.intI == 1) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        class MembersInheritanceTestClass028_Base {
	        public virtual int retInt() {
		        return 1;
	        }
        }
        class MembersInheritanceTestClass028 : MembersInheritanceTestClass028_Base {
	        public override int retInt() {
		        return 2;
	        }
	        public static bool testMethod() {
		        MembersInheritanceTestClass028_Base test = new MembersInheritanceTestClass028();
		        if (test.retInt() == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        class MembersInheritanceTestClass029_Base {
	        public int intI = 0;
	        public virtual int retInt {
		        get {
			        return -2;			
		        }
		        set {
			        intI = -1;
		        }
	        }
        }
        class MembersInheritanceTestClass029 : MembersInheritanceTestClass029_Base {
	        public override int retInt {
		        get {
			        return intI;			
		        }
		        set {
			        intI = value;
		        }
	        }
	        public static bool testMethod() {
		        MembersInheritanceTestClass029_Base test = new MembersInheritanceTestClass029();
		        test.retInt = 2;
		        if (test.retInt == 2) {
			        return true;
		        }		
		        else {
			        return false;
		        }
	        }
        }

        class MembersInheritanceTestClass030_Base {
	        public int intI = 0;
	        public virtual int retInt {
		        get {
			        return -2;			
		        }
		        set {
			        intI = -1;
		        }
	        }
        }
        class MembersInheritanceTestClass030 : MembersInheritanceTestClass030_Base {
	        public override int retInt {
		        set {
			        intI = value;
		        }
	        }
	        public static bool testMethod() {
		        MembersInheritanceTestClass030_Base test = new MembersInheritanceTestClass030();
		        test.retInt = 2;
		        if ((test.intI == 2) && (test.retInt == -2)) {
			        return true;
		        }		
		        else {
			        return false;
		        }
	        }
        }

        class MembersInheritanceTestClass031_Base {
	        public int intI = 0;
	        public virtual int retInt {
		        get {
			        return -2;			
		        }
		        set {
			        intI = -1;
		        }
	        }
        }
        class MembersInheritanceTestClass031 : MembersInheritanceTestClass031_Base {
	        public override int retInt {
		        get {
			        return intI;			
		        }
	        }
	        public static bool testMethod() {
		        MembersInheritanceTestClass031_Base test = new MembersInheritanceTestClass031();
		        test.retInt = 2;
		        if ((test.intI == -1) && (test.retInt == -1)) {
			        return true;
		        }		
		        else {
			        return false;
		        }
	        }
        }

        class MembersInheritanceTestClass032_Base {
	        public int intI = 0;
	        public virtual int retInt {
		        get {
			        return -2;			
		        }
		        set {
			        intI = -1;
		        }
	        }
        }
        class MembersInheritanceTestClass032 : MembersInheritanceTestClass032_Base {
	        public new int retInt {
		        get {
			        return intI;			
		        }
		        set {
			        intI = value;
		        }
	        }
	        public static bool testMethod() {
		        MembersInheritanceTestClass032_Base test = new MembersInheritanceTestClass032();
		        test.retInt = 2;
		        if ((test.intI == -1) && (test.retInt == -2)) {
			        return true;
		        }		
		        else {
			        return false;
		        }
	        }
        }

        class MembersInheritanceTestClass033_Base {
	        public int intI = 0;
	        public virtual int this[int i] {
		        get {
			        return -2;
		        }
		        set {
			        intI = -1;
		        }
	        }
        }
        class MembersInheritanceTestClass033 : MembersInheritanceTestClass033_Base {
	        public override int this[int i] {
		        get {
			        return intI;
		        }
		        set {
			        intI = i + value;
		        }
	        }
	        public static bool testMethod() {
		        MembersInheritanceTestClass033_Base test = new MembersInheritanceTestClass033();
		        test[2] = 2;
        
		        if (test[2] == 4) {
			        return true;
		        }		
		        else {
			        return false;
		        } 
	        }
        }

        class MembersInheritanceTestClass034_Base {
	        public int intI = 0;
	        public virtual int this[int i] {
		        get {
			        return -2;
		        }
		        set {
			        intI = -1;
		        }
	        }
        }
        class MembersInheritanceTestClass034 : MembersInheritanceTestClass034_Base {
	        public override int this[int i] {
		        set {
			        intI = i + value;
		        }
	        }
	        public static bool testMethod() {
		        MembersInheritanceTestClass034_Base test = new MembersInheritanceTestClass034();
		        test[2] = 2;
        
		        if ((test[2] == -2)&&(test.intI == 4)) {
			        return true;
		        }		
		        else {
			        return false;
		        } 
	        }
        }

        class MembersInheritanceTestClass035_Base {
	        public int intI = 0;
	        public virtual int this[int i] {
		        get {
			        return -2;
		        }
		        set {
			        intI = -1;
		        }
	        }
        }
        class MembersInheritanceTestClass035 : MembersInheritanceTestClass035_Base {
	        public override int this[int i] {
		        get {
			        return intI;
		        }
	        }
	        public static bool testMethod() {
		        MembersInheritanceTestClass035_Base test = new MembersInheritanceTestClass035();
		        test[2] = 2;
        
		        if ((test[2] == -1)&&(test.intI == -1)) {
			        return true;
		        }		
		        else {
			        return false;
		        } 
	        }
        }

        class MembersInheritanceTestClass036_Base {
	        public int intI = 0;
	        public virtual int this[int i] {
		        get {
			        return -2;
		        }
		        set {
			        intI = -1;
		        }
	        }
        }
        class MembersInheritanceTestClass036 : MembersInheritanceTestClass036_Base {
	        public new int this[int i] {
		        get {
			        return intI;
		        }
		        set {
			        intI = i + value;
		        }
	        }
	        public static bool testMethod() {
		        MembersInheritanceTestClass036_Base test = new MembersInheritanceTestClass036();
		        test[2] = 2;
        
		        if ((test[2] == -2)&&(test.intI == -1)) {
			        return true;
		        }		
		        else {
			        return false;
		        } 
	        }
        }

        class MembersInheritanceTestClass037_Base {
	        public int intI = 1;
	        public String strS = "MembersInheritanceTestClass037_Base";
	        public int intJ() {return 2;}
	        public static int intK = 3;
        }
        class MembersInheritanceTestClass037 : MembersInheritanceTestClass037_Base {
	        new public int intI = 4;
	        new public String strS = "MembersInheritanceTestClass037";
	        new public int intJ() {return 5;}
	        new public static int intK = 6;
	        public static bool testMethod() {
		        MembersInheritanceTestClass037 testc = new MembersInheritanceTestClass037();
		        MembersInheritanceTestClass037_Base testb = (MembersInheritanceTestClass037_Base) testc;

                if (testb.intI != 1) return false;
                if (testb.strS.Equals("MembersInheritanceTestClass037_Base") != true) return false;
                if (testb.intJ() != 2) return false; 
		        if (MembersInheritanceTestClass037_Base.intK != 3) return false;
                if (testc.intI != 4) return false;
                if (testc.strS.Equals("MembersInheritanceTestClass037") != true) return false;
                if (testc.intJ() != 5) return false; 
		        if (MembersInheritanceTestClass037.intK != 6) return false;
        
		        return true;
	        }
        }

        class MembersInheritanceTestClass038_Base {
	        public struct MyStruct {
		        public int retInt() {
			        return 1;
		        }
	        }
        }
        class MembersInheritanceTestClass038 : MembersInheritanceTestClass038_Base {
	        new public struct MyStruct {
		        public int retInt() {
			        return 2;
		        }
	        }
	        public static bool testMethod() {
		        MyStruct test = new MyStruct();
		        if (test.retInt() == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        class MembersInheritanceTestClass039_Base {
	        public enum MyEnum {zero, one}
        }
        class MembersInheritanceTestClass039 : MembersInheritanceTestClass039_Base {
	        new public enum MyEnum {one, two}
	        public static bool testMethod() {
		        MyEnum test = MyEnum.one;
		        if ((int)test == 0) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        class MembersInheritanceTestClass040_Base {
	        public class MyInner {
		        public int retInt() {
			        return 1;
		        }
	        }
        }
        class MembersInheritanceTestClass040 : MembersInheritanceTestClass040_Base {
	        new public class MyInner {
		        public int retInt() {
			        return 2;
		        }
	        }
	        public static bool testMethod() {
		        MyInner test = new MyInner();
		        if (test.retInt() == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        class MembersInheritanceTestClass041_Base {}
        class MembersInheritanceTestClass041 : MembersInheritanceTestClass041_Base {
        
	        new public int intI = 1;
	        public static bool testMethod() {
		        MembersInheritanceTestClass041 test = new MembersInheritanceTestClass041();
		        if (test.intI == 1) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        class MembersInheritanceTestClass042_Base {}
        class MembersInheritanceTestClass042 : MembersInheritanceTestClass042_Base {
        
	        new public String strS = "MembersInheritanceTestClass042";
	        public static bool testMethod() {
		        MembersInheritanceTestClass042 test = new MembersInheritanceTestClass042();
		        if (test.strS.Equals("MembersInheritanceTestClass042")) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        class MembersInheritanceTestClass043_Base {}
        class MembersInheritanceTestClass043 : MembersInheritanceTestClass043_Base {
        
	        new public int intJ() {return 2;}
	        public static bool testMethod() {
		        MembersInheritanceTestClass043 test = new MembersInheritanceTestClass043();
		        if (test.intJ() == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        class MembersInheritanceTestClass044_Base {}
        class MembersInheritanceTestClass044 : MembersInheritanceTestClass044_Base {
        
	        new public static int intK = 3;
	        public static bool testMethod() {
		        if (MembersInheritanceTestClass044.intK == 3) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        class MembersInheritanceTestClass045_Base {}
        class MembersInheritanceTestClass045 : MembersInheritanceTestClass045_Base {
        
	        new public struct MyStruct {
		        public int intRet() {return 1;}
	        }
	        public static bool testMethod() {
		        MyStruct test = new MyStruct();
		        if (test.intRet() == 1) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        class MembersInheritanceTestClass046_Base {}
        class MembersInheritanceTestClass046 : MembersInheritanceTestClass046_Base {
        
	        new enum MyEnum {one, two}
	        public static bool testMethod() {
		        MyEnum test = MyEnum.one;
		        if ((int)test == 0) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        class MembersInheritanceTestClass047_Base {}
        class MembersInheritanceTestClass047 : MembersInheritanceTestClass047_Base {
        
	        new public class MyInner {
		        public int intRet() {return 1;}
	        }
	        public static bool testMethod() {
		        MyInner test = new MyInner();
		        if (test.intRet() == 1) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        public class MembersInheritanceTestClass053_Base {
	        protected int intI = 2;
	        protected int MyMeth() {
		        return 4;
	        }
	        int intJ;
        
	        protected int PropInt {
		        get {
			        return intJ;
		        }
		        set {
			        intJ = value;
		        }	
	        }
        }
        public class MembersInheritanceTestClass053_Derived : MembersInheritanceTestClass053_Base
        {
        }
        class MembersInheritanceTestClass053 : MembersInheritanceTestClass053_Derived
        {
	        public int Test() {
		        PropInt = 3;
		        if ((intI == 2) && (PropInt == 3) && (MyMeth() == 4)) {
			        return 0;
		        }
		        else {
			        return 1;
		        }
	        }
        
	        public static bool testMethod() {
		        MembersInheritanceTestClass053 mc = new MembersInheritanceTestClass053();
        
		        return (mc.Test() == 0);	
	        }
        }

        public class MembersInheritanceTestClass054_Base {
	        protected int intI = 2;
	        protected int MyMeth() {
		        return 4;
	        }
	        int intJ;
        
	        protected int PropInt {
		        get {
			        return intJ;
		        }
		        set {
			        intJ = value;
		        }	
	        }
        }
        public class MembersInheritanceTestClass054_Derived : MembersInheritanceTestClass054_Base
        {
        }
        class MembersInheritanceTestClass054 : MembersInheritanceTestClass054_Derived
        {
        
	        public static bool testMethod() {
		        MembersInheritanceTestClass054 mc = new MembersInheritanceTestClass054();
		        mc.PropInt = 3;
		        if ((mc.intI == 2) && (mc.PropInt == 3) && (mc.MyMeth() == 4)) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        class MembersInheritanceTestClass057 : MembersInheritanceTestClass057_SubB.N {
	        public static bool testMethod() {
                MembersInheritanceTestClass057 MC = new MembersInheritanceTestClass057();
		        if (MC.intI == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        class MembersInheritanceTestClass057_SubB { 
	        internal class N {
		        internal int intI = 2;
	        } 
        }

        class MembersInheritanceTestClass058 : MembersInheritanceTestClass058_SubB {
	        public static bool testMethod() {
                MembersInheritanceTestClass058 MC = new MembersInheritanceTestClass058();
		        if (MC.intI == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        class MembersInheritanceTestClass058_SubB {
	        public int intI = 2;
        }

        public class MembersInheritanceTestClass059 {
	        public int intI = 2;
	        public class MyInner : MembersInheritanceTestClass059 {}
	        public static bool testMethod() {
		        MyInner MI = new MyInner();
		        if (MI.intI == 2) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }

        class MembersModifiersTestClass01
        {
            //public
            public int intI = 1;
            public String strS = "MembersModifiersTestClass01";
            public int intJ() { return 2; }
            public static int intK = 3;
            public static bool testMethod()
            {
                MembersModifiersTestClass01 test = new MembersModifiersTestClass01();
                if ((test.intI == 1) && (test.intJ() == 2) && (intK == 3) && (test.strS.Equals("MembersModifiersTestClass01")))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        class MembersModifiersTestClass02 {
	        //protected
	        protected int intI = 1;
	        protected String strS = "MembersModifiersTestClass02";
	        protected int intJ() {return 2;}
	        protected static int intK = 3;
	        public static bool testMethod() {
		        MembersModifiersTestClass02 test = new MembersModifiersTestClass02();
		        if ((test.intI ==1) && (test.intJ() == 2) && (intK == 3) && (test.strS.Equals("MembersModifiersTestClass02"))) {
			        return true;
		        }
		        else {
			        return false;
		        }	
	        }
        }
        class MembersModifiersTestClass03 {
	        //internal
	        internal int intI = 1;
	        internal String strS = "MembersModifiersTestClass03";
	        internal int intJ() {return 2;}
	        internal static int intK = 3;
	        public static bool testMethod() {
		        MembersModifiersTestClass03 test = new MembersModifiersTestClass03();
		        if ((test.intI ==1) && (test.intJ() == 2) && (intK == 3) && (test.strS.Equals("MembersModifiersTestClass03"))) {
			        return true;
		        }
		        else {
			        return false;
		        }	
	        }
        }
        class MembersModifiersTestClass04 {
	        //private
	        private int intI = 1;
	        private String strS = "MembersModifiersTestClass04";
	        private int intJ() {return 2;}
	        private static int intK = 3;
	        public static bool testMethod() {
		        MembersModifiersTestClass04 test = new MembersModifiersTestClass04();
		        if ((test.intI ==1) && (test.intJ() == 2) && (intK == 3) && (test.strS.Equals("MembersModifiersTestClass04"))) {
			        return true;
		        }
		        else {
			        return false;
		        }	
	        }
        }
        class MembersModifiersTestClass05 {
	        //public
	        public struct MyStruct {
		        public static int intI = 1;
	        }
	        public enum AA {zero, one}
	        public class MyInner {
		        public static int intI = 2;
	        }
	        public static bool testMethod() {
		        AA MyEnum = AA.zero;
		        if ((MyStruct.intI == 1) && (MyEnum == AA.zero) && (MyInner.intI ==2)) {
			        return true;
		        }
		        else {
			        return false;
		        }	
	        }
        }
        class MembersModifiersTestClass06 {
	        //protected
	        protected struct MyStruct {
		        public static int intI = 1;
	        }
	        protected enum AA {zero, one}
	        protected class MyInner {
		        public static int intI = 2;
	        }
	        public static bool testMethod() {
		        AA MyEnum = AA.zero;
		        if ((MyStruct.intI == 1) && (MyEnum == AA.zero) && (MyInner.intI ==2)) {
			        return true;
		        }
		        else {
			        return false;
		        }	
	        }
        }
        class MembersModifiersTestClass07 {
	        //internal
	        internal struct MyStruct {
		        public static int intI = 1;
	        }
	        internal enum AA {zero, one}
	        internal class MyInner {
		        public static int intI = 2;
	        }
	        public static bool testMethod() {
		        AA MyEnum = AA.zero;
		        if ((MyStruct.intI == 1) && (MyEnum == AA.zero) && (MyInner.intI ==2)) {
			        return true;
		        }
		        else {
			        return false;
		        }	
	        }
        }
        class MembersModifiersTestClass08 {
	        //private
	        private struct MyStruct {
		        public static int intI = 1;
	        }
	        private enum AA {zero, one}
	        private class MyInner {
		        public static int intI = 2;
	        }
	        public static bool testMethod() {
		        AA MyEnum = AA.zero;
		        if ((MyStruct.intI == 1) && (MyEnum == AA.zero) && (MyInner.intI ==2)) {
			        return true;
		        }
		        else {
			        return false;
		        }	
	        }
        }
        class MembersModifiersTestClass12 {
	        protected internal int intI = 1;
	        public static bool testMethod() {
		        MembersModifiersTestClass12 test = new MembersModifiersTestClass12();
		        if (test.intI == 1) {
			        return true;
		        }
		        else {
			        return false;
		        }
	        }
        }
        sealed class MembersModifiersTestClass23 {
	        protected int intI = 0;

	        public static bool testMethod() {
		        return true;
	        }
        }
        sealed class MembersModifiersTestClass24 {
	        protected int intI {
		        get {return 1;}
		        set {}
	        }

	        public static bool testMethod() {
		        return true;
	        }
        }
        sealed class MembersModifiersTestClass25 {
	        protected void MyMeth() {}

	        public static bool testMethod() {
		        return true;
	        }
        }
        sealed class MembersModifiersTestClass26 {
	        protected int this[int intI] {
		        get {
			        return 1;
		        }
		        set {}
	        }

	        public static bool testMethod() {
		        return true;
	        }
        }
        sealed class MembersModifiersTestClass27 {
	        protected class MyNested {}

	        public static bool testMethod() {
		        return true;
	        }
        }

    }
}
