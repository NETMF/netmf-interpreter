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
    public class MonitorTests : IMFTestInterface
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

        public static int monCount = 0;
        public static object mutex = new object();
        static public void MonitoredThreadIncrementor()
        {
            Thread.Sleep(new Random().Next(10));
            Monitor.Enter(mutex);
            monCount++;
            Monitor.Exit(mutex);
        }
        static public void MonitoredThreadDecrementor()
        {
            Thread.Sleep(new Random().Next(10));
            Monitor.Enter(mutex);
            monCount--;
            Monitor.Exit(mutex);
        }
        static public void MonitoredThreadIncrementorStarter()
        {
            Thread[] threadArrayInc = new Thread[4];
            for (int i = 0; i < 4; i++)
            {
                Log.Comment("Attempting to start inc thread " + i);
                threadArrayInc[i] = new Thread(MonitoredThreadIncrementor);
                threadArrayInc[i].Start();
                Thread.Sleep(1);
            }
            Thread.Sleep(10);
            for (int i = 0; i < 4; i++)
            {
                threadArrayInc[i].Join();
            }
        }
        static public void MonitoredThreadDecrementorStarter()
        {
            Thread[] threadArrayDec = new Thread[5];
            for (int i = 0; i < 5; i++)
            {
                Log.Comment("Attempting to start dec thread " + i);
                threadArrayDec[i] = new Thread(MonitoredThreadDecrementor);
                threadArrayDec[i].Start();
                Thread.Sleep(1);
            }
            Thread.Sleep(10);
            for (int i = 0; i < 5; i++)
            {
                threadArrayDec[i].Join();
            }
        }
        static public void MonitoredThreadIncrementor2Starter()
        {
            Thread[] threadArrayInc2 = new Thread[6];
            for (int i = 0; i < 6; i++)
            {
                Log.Comment("Attempting to start inc2 thread " + i);
                threadArrayInc2[i] = new Thread(MonitoredThreadIncrementor);
                threadArrayInc2[i].Start();
                Thread.Sleep(1);
            }
            Thread.Sleep(10);
            for (int i = 0; i < 6; i++)
            {
                threadArrayInc2[i].Join();
            }
        }

        [TestMethod]
        public MFTestResults Monitor1_Basic_Test()
        {
            /// <summary>
            /// 1. Starts 4 threads that run asynchronously
            /// 2. Each thread increments or decrements while in a critical section
            /// 3. Waits for execution and then verifies that all expected operations completed
            /// </summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Starts several async threads that Enter and Exit critical sections.");
            Log.Comment("This may erroneously pass.");
            Log.Comment("This may erroneously fail for extremely slow devices.");
            Log.Comment("Starting the 4 threads");
            Thread incThread = new Thread(MonitoredThreadIncrementorStarter);
            incThread.Start();
            Thread decThread = new Thread(MonitoredThreadDecrementorStarter);
            decThread.Start();
            Thread inc2Thread = new Thread(MonitoredThreadIncrementor2Starter);
            inc2Thread.Start();
            Thread lastThread = new Thread(MonitoredThreadDecrementor);
            lastThread.Start();
            Thread.Sleep(1);
            Log.Comment("Joining All threads to main thread");
            incThread.Join();
            decThread.Join();
            inc2Thread.Join();
            lastThread.Join();
            Log.Comment("Verifying all operations completed successfully");
            if (monCount != 4)
            {
                Log.Comment("expected final result = '4' but got '" + monCount + "'");
                testResult = MFTestResults.Fail;
            }
            return testResult;
        }

        static object locker1 = new object();

        [TestMethod]
        public MFTestResults Monitor2_SynchronizationLockException_Test()
        {
            /// <summary>
            /// 1. Call Monitor.Exit without first calling Monitor.Enter on the same object
            /// 2. Verify SynchronizationLockException exception is thrown
            /// </summary>
            ///
            MFTestResults testResult = MFTestResults.Fail;
            Log.Comment("Verify SynchronizationLockException exception is thrown");
            Log.Comment("This currently fails, see 20281");
            try
            {
                Log.Comment("Calling Monitor.Exit without first calling Monitor.Enter should throw an exception");
                Monitor.Exit(locker1);
            }
            catch (Exception e)
            {
                Log.Comment("Threw excption name " + e.ToString());
                testResult = MFTestResults.Pass;
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults Monitor3_Enter_ArgumentNullException_Test()
        {
            /// <summary>
            /// 1. Call Monitor.Enter passing null reference obj parameter
            /// 2. verify ArgumentNullException exception is thrown
            /// </summary>
            ///

            MFTestResults testResult = MFTestResults.Fail;
            Log.Comment("verify ArgumentNullException exception is thrown ");
            Log.Comment("This is fixed, see 20730 for details");
            try
            {
                Log.Comment("Calling Monitor.Enter passing null reference parameter should throw exception");
                Monitor.Enter(null);
            }
            catch (ArgumentNullException e)
            {
                Log.Comment("Correctly threw " + e.ToString() + " in Monitor.Enter(null)");
                testResult = MFTestResults.Pass;
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults Monitor4_Exit_ArgumentNullException_Test()
        {
            /// <summary>
            /// 1. Call Monitor.Exit passing null reference obj parameter
            /// 2. verify ArgumentNullException exception is thrown
            /// </summary>
            ///
            MFTestResults testResult = MFTestResults.Fail;
            Log.Comment("verify ArgumentNullException exception is thrown ");
            Log.Comment("This is fixed, see 20731 for details");
            try
            {
                Log.Comment("Calling Monitor.Exit passing null reference parameter should throw exception");
                Monitor.Exit(null);
            }
            catch (ArgumentNullException e)
            {
                Log.Comment("Correctly threw " + e.ToString() + " in Monitor.Enter(null)");
                testResult = MFTestResults.Pass;
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
            }
            return testResult;
        }

        static ManualResetEvent flag = new ManualResetEvent(false);
        static bool lockResult = false;
        static object locker2 = new object();
        static void RepeatedLock()
        {
            Log.Comment("T1 = " + DateTime.Now);
            Monitor.Enter(locker2);
            try
            {
                lockResult = !lockResult;
                Log.Comment("I have the lock");
                Nest();
                Log.Comment("I still have the lock");
            }
            finally
            {
                if (flag.WaitOne(500, false))
                {
                    Monitor.Exit(locker2);
                    Log.Comment("Here the lock is released");
                }
            }
            Log.Comment("T4 = " + DateTime.Now);
        }
        static void Nest()
        {
            Log.Comment("T2 = " + DateTime.Now);
            Monitor.Enter(locker2);
            try
            {
                Log.Comment("Inside Lock");
            }
            finally
            {
                Monitor.Exit(locker2);
                Log.Comment("Released the lock? Not quite!");
            }
            Log.Comment("T3 = " + DateTime.Now);
        }

        [TestMethod]
        public MFTestResults Monitor5_Repeatedly_Lock_Unlock_Test()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Starts two Threads ");
            Log.Comment("Repeatedly locks an object by multiple calls to Monitor.Enter");
            Log.Comment("Verifies the object is unlocked only by a corresponding number of Monitor.Exit");
            Thread newThread1 = new Thread(RepeatedLock);
            Thread newThread2 = new Thread(RepeatedLock);
            Log.Comment("Starting two threads, repeatedly locking, waiting and verifying");          
            newThread1.Start();
            newThread2.Start();
            Thread.Sleep(100);
            if (!lockResult)
            {
                Log.Comment("Failure : both threads passed lock");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("unlocking the final lock and verifying the waiting thread continues");
            flag.Set();
            Thread.Sleep(500);
            if (lockResult)
            {
                Log.Comment("Failure : lock not released by equal number of unlocks");
                testResult = MFTestResults.Fail;
            }
            if (newThread1.IsAlive)
                newThread2.Abort();
            if (newThread2.IsAlive)
                newThread2.Abort();

            return testResult;
        }
    }
}
