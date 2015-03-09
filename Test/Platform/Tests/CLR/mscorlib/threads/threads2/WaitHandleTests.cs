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
    public class WaitHandleTests : IMFTestInterface
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

        static WaitHandle[] waitHandles = new WaitHandle[] 
        {
            new AutoResetEvent(false),
            new AutoResetEvent(false)
        };


        static void DoTask1()
        {
            AutoResetEvent are = (AutoResetEvent)waitHandles[0];
            int time = 10000;
            Thread.Sleep(time);
            are.Set();
        }

        static void DoTask2()
        {
            AutoResetEvent are = (AutoResetEvent)waitHandles[1];
            int time = 500;
            Thread.Sleep(time);
            are.Set();
        }

        [TestMethod]
        public MFTestResults WaitHandle1_WaitAll_WaitAny_Test()
        {
            /// <summary>
            /// 1. Starts two threads
            /// 2. Calls WaitAll with respect to those threads
            /// 3. Verifies that both threads have executed
            /// 4. Restarts the threads
            /// 5. Calls WaitAny with respect to those threads
            /// 6. Verifies that only one threads has executed
            /// </summary>
            ///
            Log.Comment("This test may erroneously pass or fail due to machine speed");
            Log.Comment("Tests the WaitAll method");
            MFTestResults testResult = MFTestResults.Pass;

            try
            {
                Log.Comment("Create threads");
                Thread t1 = new Thread(DoTask1);
                Thread t2 = new Thread(DoTask2);
                t1.Start();
                t2.Start();
                Thread.Sleep(1);

                Log.Comment("Waiting for all tasks to complete and verifying");
                if (!WaitHandle.WaitAll(waitHandles))
                {
                    Log.Comment("Not all waithandles has received the signal");
                    testResult = MFTestResults.Fail;
                }
                bool r = t1.IsAlive, s = t2.IsAlive;
                if (r || s)
                {
                    Log.Comment("Not all threads are dead");
                    if (r)
                    {
                        Log.Comment("t1 is Alive");
                    }
                    if (s)
                    {
                        Log.Comment("t2 is Alive");
                    }
                    testResult = MFTestResults.Fail;
                }

                Log.Comment("Re-create threads");
                t1 = new Thread(DoTask1);
                t2 = new Thread(DoTask2);
                t1.Start();
                t2.Start();
                Thread.Sleep(1);

                Log.Comment("Waiting for either task to complete and verifying");
                Log.Comment("The WaitHandle with index " + WaitHandle.WaitAny(waitHandles).ToString() + " satisfied the wait");
                Log.Comment("Doing t1 XOR t2 to verify only one but not both are alive or dead");
                Log.Comment("t1 XOR t2 = ~((p&q)||(~p & ~q))");
                bool p = t1.IsAlive, q = t2.IsAlive;
                if ((p && q) || ((!p) && (!q)))
                {
                    Log.Comment("Not both but either one should have finished");
                    testResult = MFTestResults.Fail;
                }
                Log.Comment(p == true ? "t1 is Alive " : "t1 is Dead ");
                Log.Comment(q == true ? "t2 is Alive " : "t2 is Dead ");               
            }
            catch (Exception e)
            {              
                Log.Comment("Caught: " + e.Message);
                testResult = MFTestResults.Fail;
            }
            return testResult;
        }

        static AutoResetEvent ready = new AutoResetEvent(false);
        static AutoResetEvent go = new AutoResetEvent(false);
        static int counter = 0;

        static void Work()
        {
            while (true)
            {
                ready.Set();                          // Indicate that we're ready
                go.WaitOne();                         // Wait to be kicked off...
                if (counter == 0) return;             // Gracefully exit
                Log.Comment("Counter = " + counter);
            }
        }

        [TestMethod]
        public MFTestResults WaitHandle2_WatiOne_Test()
        {
            /// <summary>
            /// 1. Starts a thread
            /// 2. waits until the thread is ready, calling WaitOne
            /// 3. when the thread signals it's ready, it continues executing
            /// 4. worker thread waits on the other hand till it's signaled
            /// 5. worker thread continues execution when signaled
            /// 6. Verifies thread waits, and executes when signaled.
            /// </summary>
            ///

            Log.Comment("This test verifies thread waits, and executes when signaled");
            Log.Comment("Tests WaitOne method");
            MFTestResults testResult = MFTestResults.Pass;
            Thread newThread1 = new Thread(Work);
            newThread1.Start();

            Log.Comment("Signal the worker 5 times");
            for (int i = 1; i <= 5; i++)
            {
                Log.Comment("First wait until worker is ready");
                ready.WaitOne();
                Log.Comment("Doing task");
                counter++;
                Log.Comment("Tell worker to go!");
                go.Set();
            }
            if (counter != 5)
            {
                Log.Comment("expected signaling '5' but got '" + counter + "'");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Tell the worker to end by reseting counter to 0(zero)");
            ready.WaitOne();
            counter = 0;
            go.Set();
            return testResult;
        }
    }
}

