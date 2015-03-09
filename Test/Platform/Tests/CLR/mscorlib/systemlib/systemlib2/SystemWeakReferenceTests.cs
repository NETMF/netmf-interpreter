////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;
using System.Threading;

namespace Microsoft.SPOT.Platform.Tests
{
    public class SystemWeakReferenceTests : IMFTestInterface
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

        //WeakRef Test methods
        static bool hasFinalized1 = false;
        public class WeakRefClass
        {
            public int data = 5;
            public WeakRefClass(){ }
            ~WeakRefClass()
            {
                Log.Comment("Finalized.");
                hasFinalized1 = true;
            }
            void MakeSomeGarbage()
            {
                // Create objects and release them
                // to fill up memory with unused objects.
                object junk;
                for (int i = 0; i < 10000; i++)
                {
                    junk = new object();
                }
            }
        }

        [TestMethod]
        public MFTestResults WeakRef1_Test()
        {
            /// <summary>
            ///  1. Create an object with strong ref
            ///  2. Create a short weak ref to the onject
            ///  3. Allow for GC
            ///  4. Verify & Remove Strong reference
            ///  5. Allow for GC
            ///  6. If weak ref surivived verify its data
            /// </summary>
            ///
            bool testResult = true;
            Log.Comment("Create an object with strong ref");
            WeakRefClass WRC1 = new WeakRefClass();

            Log.Comment("Create a short weak ref to the onject");
            WeakReference wr = new WeakReference(WRC1);
            wr.Target = WRC1;

            Log.Comment("Allow for GC");
            SPOT.Debug.GC(true);
            int sleepTime = 1000;
            int slept = 0;
            while (!hasFinalized1 && slept < sleepTime)
            {
                System.Threading.Thread.Sleep(10);
                slept += 10;
            }
            Log.Comment("GC took " + slept);

            testResult &= (!hasFinalized1);

            Log.Comment("Verify & Remove Strong reference");
            testResult &= (((WeakRefClass)wr.Target).data == 5);
            WRC1 = null;
            if (WRC1 == null)
            {
                Log.Comment("After setting WRC1 to null: WRC1 is null.");
            }
            else
            {
                testResult = false;
            }

            Log.Comment("Allow for GC");
            SPOT.Debug.GC(true);
            sleepTime = 1000;
            slept = 0;
            while (!hasFinalized1 && slept < sleepTime)
            {
                System.Threading.Thread.Sleep(10);
                slept += 10;
            }
            Log.Comment("GC took " + slept);
            testResult &= (hasFinalized1);
            testResult &= (WRC1 == null);

            if (wr.IsAlive)
            {
                testResult &= (((WeakRefClass)wr.Target).data == 5);
                Log.Comment("Weak Reference survived.");
            }
            else 
            {
                Log.Comment("Weak Reference has been collected");
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
    }
}
