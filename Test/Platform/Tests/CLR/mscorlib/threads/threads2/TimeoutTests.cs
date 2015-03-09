////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Reflection;
using System.Threading;

namespace Microsoft.SPOT.Platform.Tests
{
    public class TimeoutTests : IMFTestInterface
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
            Thread.Sleep(1000);
        }

        class Work
        {           
            public void DoWorkInfinite()
            {
                Thread.Sleep(300);
                Log.Comment("Instance thread procedure DoWorkInfinite.");
                Thread.Sleep(Timeout.Infinite);
            }
            public static void DoWorkInfiniteStatic()
            {
                Thread.Sleep(300);
                Log.Comment("Static thread procedure DoWorkInfiniteStatic.");
                Thread.Sleep(Timeout.Infinite);
            }
        }

        [TestMethod]
        public MFTestResults Timeout1_Infinite_Test()
        {
            /// <summary>
            /// 1. Starts two threads which have an Infinite execution time one static
            /// 2. Calls Abort on the Infinite threads
            /// 3. Verifies that the aborted threads stop immediately
            /// </summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Starts two threads one infinitely long, aborts one and passes.");
            Log.Comment("This may erroneously fail for extremely slow devices.");

            Log.Comment("Starting the two threads");
            Thread newThread1 = new Thread(Work.DoWorkInfiniteStatic);
            newThread1.Start();
            Work w = new Work();           
            Thread newThread2 = new Thread(w.DoWorkInfinite);
            newThread2.Start();
            Thread.Sleep(1);

            Log.Comment("Waiting for 1000msec and verifying both threads are alive");
            int slept = 0;
            while ((newThread1.IsAlive || newThread2.IsAlive) && slept < 1000)
            {
                Thread.Sleep(100);
                slept += 100;
            }
            if (!newThread1.IsAlive || !newThread2.IsAlive)
            {
                Log.Comment("Failure : Both threads were suppose to be sleeping for Timeout.Infinite");
                Log.Comment("IsAlive ? Thread1 = '" + newThread1.IsAlive + "' and Thread2 = '" + newThread2.IsAlive + "'");
                testResult = MFTestResults.Fail;
            }

            Log.Comment("Aborting both threds and Verifying abort.");
            newThread2.Abort();
            newThread1.Abort();
            Thread.Sleep(10);
            if (newThread1.IsAlive || newThread2.IsAlive)
            {
                Log.Comment("Upon Abort both thread should be dead but");
                Log.Comment("IsAlive ? Thread1 = '" + newThread1.IsAlive + "' and Thread2 = '" + newThread2.IsAlive + "'");
                testResult = MFTestResults.Fail;
            }
            
            return testResult;
        }
    }
}
