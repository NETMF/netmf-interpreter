////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class DestructorsTests : IMFTestInterface
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

        //Destructors Test methods
        //All test methods ported from folder current\test\cases\client\CLR\Conformance\10_classes\Destructors
        //The following tests were removed because they were build failure tests:
        //1,2,5,6,8-10

        [TestMethod]
        public MFTestResults Destructors3_Test()
        {
            //Ported from Destructors3.cs
            Log.Comment(" Section 10.11");
            Log.Comment(" Destructors implement the actions required to ");
            Log.Comment(" destruct the instances of a class.");
            Log.Comment("");
            Log.Comment("Note: This test may fail due to lengthy garbage collection, look for Destructor messages in later logs");
            if (DestructorsTestClass3.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;

        }

        [TestMethod]
        public MFTestResults Destructors4_Test()
        {
            //Ported from Destructors4.cs
            Log.Comment(" Section 10.11");
            Log.Comment(" Destructors implement the actions required to ");
            Log.Comment(" destruct the instances of a class.");
            Log.Comment("");
            Log.Comment("Note: This test may fail due to lengthy garbage collection, look for Destructor messages in later logs");
            if (DestructorsTestClass4.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Destructors7_Test()
        {
            //Ported from Destructors7.cs
            Log.Comment(" Section 10.12");
            Log.Comment(" Destructors are not inherited. Thus, a class");
            Log.Comment(" has no other destructors than those that are ");
            Log.Comment(" actually declared in the class.");
            Log.Comment("");
            Log.Comment("Note: This test may fail due to lengthy garbage collection, look for Destructor messages in later logs");
            if (DestructorsTestClass7.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        class DestructorsTestClass3
        {

            static int intI = 1;

            ~DestructorsTestClass3()
            {
                //Log.Comment("Calling Destructor for Test Class 3");
                intI = 2;
            }

            public static bool testMethod()
            {
                DestructorsTestClass3 mc = new DestructorsTestClass3();
                mc = null;
                Microsoft.SPOT.Debug.GC(true);
                int sleepTime = 5000;
                int slept = 0;
                while (intI != 2 && slept < sleepTime)
                {
                    System.Threading.Thread.Sleep(10);
                    slept += 10;
                }
                Log.Comment("Thread has slept for");
                Log.Comment(slept.ToString());
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


        class DestructorsTestClass4_Base
        {
            public static int intI = 2;
            ~DestructorsTestClass4_Base()
            {
                intI = intI * 2;
                //Log.Comment("Calling Destructor for Test Class 4 Base");
            }
        }

        class DestructorsTestClass4 : DestructorsTestClass4_Base
        {

            ~DestructorsTestClass4()
            {
                intI = intI + 2;
                //Log.Comment("Calling Destructor for Test Class 4");
            }

            public static bool testMethod()
            {
                DestructorsTestClass4 mc = new DestructorsTestClass4();
                mc = null;
                Microsoft.SPOT.Debug.GC(true); 
                int sleepTime = 5000;
                int slept = 0;
                while (intI != 8 && slept < sleepTime)
                {
                    System.Threading.Thread.Sleep(10);
                    slept += 10;
                }
                Log.Comment("Thread has slept for");
                Log.Comment(slept.ToString());
                if (intI == 8)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class DestructorsTestClass7_Base
        {
            public static int intI = 2;
        }

        class DestructorsTestClass7 : DestructorsTestClass7_Base
        {

            ~DestructorsTestClass7()
            {
                intI = 3;
                //Log.Comment("Calling Destructor for Test Class 7");
            }

            public static bool testMethod()
            {
                DestructorsTestClass7 mc = new DestructorsTestClass7();
                mc = null;
                Microsoft.SPOT.Debug.GC(true); 
                int sleepTime = 5000;
                int slept = 0;
                while (intI != 3 && slept < sleepTime)
                {
                    System.Threading.Thread.Sleep(10);
                    slept += 10;
                }
                Log.Comment("Thread has slept for");
                Log.Comment(slept.ToString());
                if (intI == 3)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
