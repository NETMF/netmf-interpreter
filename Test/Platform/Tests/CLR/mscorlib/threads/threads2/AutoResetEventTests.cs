////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Threading;
using System.Reflection;

namespace Microsoft.SPOT.Platform.Tests
{
    public class AutoResetEventTests : IMFTestInterface
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
            Log.Comment("Cleaning up after the tests.");
            Thread.Sleep(1000);
            // TODO: Add your clean up steps here.
        }

        class Waiter
        {
            public static AutoResetEvent are1 = new AutoResetEvent(false);
            public static AutoResetEvent are2 = new AutoResetEvent(true);
            public bool flag;
            public int wait;
            public DateTime t1;
            public DateTime t2;

            public void DoWait()
            {
                Log.Comment("Waiting...");
                t1 = DateTime.Now;
                are1.WaitOne();
                t2 = DateTime.Now;
                flag = true;
                Log.Comment("Notified");
            }

            public void DoWaitTimeOut()
            {
                Log.Comment("Waiting...");               
                are1.WaitOne(wait, false);
                flag = true;            
            }

            public void DoWaitReset()
            {
                Log.Comment("Waiting...");             
                are2.WaitOne();
                flag = true;
            }
        }

        [TestMethod]
        public MFTestResults AutoReset1_WaitOne_Set_Test()
        {
            ///<summary>
            ///Start a thread and make it wait by calling WaitOne until signaled by another thread.
            ///Verify that it continues executing when Set by the other thread.
            ///</summary>
            /// 

            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Starts a thread and calls WaitOne inside the thread ");
            Log.Comment("Signals The thread to continue execution");
            Log.Comment("Verifies the thread has waited and continued execution upon calling Set");

            Log.Comment("Starting Thread");
            Waiter newWaiter1 = new Waiter();
            Thread newThread1 = new Thread(newWaiter1.DoWait);
            newThread1.Start();

            Log.Comment("Waiting and Signaling after 1000msec.");
            Thread.Sleep(1000);

            Log.Comment("wake it up");
            DateTime t3 = DateTime.Now;
            Waiter.are1.Set();

            Log.Comment("Waiting the thread to finish and also to get t2");
            Thread.Sleep(10);
            TimeSpan duration1 = newWaiter1.t2 - newWaiter1.t1;
            TimeSpan duration2 = newWaiter1.t2 - t3;

            // Assuming atleast the thread should wait for 750ms           
            if (duration1.CompareTo(new TimeSpan(0, 0, 0, 0, 750)) <= 0)
            {
                Log.Comment("The thread should have waited atleast 750 msec.");
                testResult = MFTestResults.Fail;
            }
            if (duration2.CompareTo(new TimeSpan(0, 0, 0, 0, 0)) < 0)
            {
                Log.Comment("The thread continued executing before it's signaled");
                testResult = MFTestResults.Fail;
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults AutoReset2_Set_WaitOne_Set_Test()
        {
            ///<summary>
            ///Calls Set repeatedly while no thread was blocked
            ///Starts two threads and call WaitOne on both. 
            ///Call Set by a third unblocked thread with access to the AutoResetEvent and
            ///verify that only one blocked thread is released.
            ///</summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Calls Set repeatedly while no thread was blocked");
            Log.Comment("Starts two threads and calls WaitOne on both threads ");
            Log.Comment("Signals The thread to continue execution calling Set once");
            Log.Comment("Verifies only one blocked thread is released");

            Waiter.are1.Set();
            Waiter.are1.Set();

            Waiter newWaiter1 = new Waiter();
            newWaiter1.flag = false;
            Thread newThread1 = new Thread(newWaiter1.DoWait);
            Waiter newWaiter2 = new Waiter();
            newWaiter2.flag = false;
            Thread newThread2 = new Thread(newWaiter2.DoWait);
            Log.Comment("Starting The threads");
            newThread1.Start();
            newThread2.Start();

            Log.Comment("Waiting and verifying only one of the threads is signaled");
            Thread.Sleep(1000);
            Log.Comment("flag1 XOR flag2 =  ~((flag1 & flag2)||(~flag1 & ~flag2))");
            if ((newWaiter1.flag && newWaiter2.flag) || ((!newWaiter1.flag) && (!newWaiter2.flag)))
            {
                Log.Comment("Only One of the threads should have been signaled");
                testResult = MFTestResults.Fail;
            }
            Waiter.are1.Set();

            return testResult;
        }

        [TestMethod]
        public MFTestResults AutoReset3_TwoWaitOne_TwoSet_Test()
        {
            ///<summary>
            ///Start two threads and call WaitOne on both. 
            ///Call Set twice by a third unblocked thread with access to the AutoResetEvent and
            ///verify that both blocked threads are released.
            ///</summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Starts two threads and calls WaitOne on both threads ");
            Log.Comment("Signals The threads to continue execution calling Set twice");
            Log.Comment("Verifies both blocked threads are released");

            Waiter newWaiter1 = new Waiter();
            Thread newThread1 = new Thread(newWaiter1.DoWait);
            newWaiter1.flag = false;
            Waiter newWaiter2 = new Waiter();
            Thread newThread2 = new Thread(newWaiter2.DoWait);
            newWaiter2.flag = false;
            Log.Comment("Starting threads, waiting and verifying both are waiting");
            newThread1.Start();
            newThread2.Start();
            Thread.Sleep(1000);
            if (newWaiter1.flag || newWaiter2.flag)
            {
                Log.Comment("Failure: One or both threads are not waiting, Thread1 = '" +
                    newWaiter1.flag + "' Thread2 = '" + newWaiter2.flag + "'");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Signaling twice, waiting and verifying both threads are signaled");
            Waiter.are1.Set();
            Waiter.are1.Set();
            Thread.Sleep(1000);
            if (!newWaiter1.flag || !newWaiter2.flag)
            {
                Log.Comment("Not both threads are signaled, Thread1 = '" + newWaiter1.flag +
                    "' and Thread2 = '" + newWaiter2.flag + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults AutoReset4_WaitOne_TimeOut_Test()
        {
            ///<summary>
            ///Starts a thread and call WaitOne passing timeout parameter
            ///Verify that the wait will end because of a timeout rather than obtaining a signal.
            ///</summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Starts a thread and call WatiOne passing timeout parameter");
            Log.Comment("verifies the wait will end because of timeout");
            Log.Comment("Starts a 2nd thread and call WatiOne passing Timeout.Infinite");
            Log.Comment("verifies the wait will not end for 3 sec (assumed Infinite)");

            Waiter newWaiter1 = new Waiter();
            newWaiter1.wait = 100;
            newWaiter1.flag = false;
            Thread newThread1 = new Thread(newWaiter1.DoWaitTimeOut);
            Log.Comment("Starting thread, waiting and verifying wait timeouts");
            newThread1.Start();
            Thread.Sleep(500);
            if (!newWaiter1.flag)
            {
                Log.Comment("Waited for 500msec. but Thread should have timeouted in " + newWaiter1.wait + "");
                testResult = MFTestResults.Fail;
            }
            Waiter newWaiter2 = new Waiter();
            newWaiter2.wait = -1;
            newWaiter2.flag = false;
            Thread newThread2 = new Thread(newWaiter2.DoWaitTimeOut);
            Log.Comment("Starting thread, waiting for Timeout.Infinite and verifying");
            newThread2.Start();
            Thread.Sleep(3000);
            if (newWaiter2.flag)
            {
                Log.Comment("Failure: thread didn't wait for Infinite.Timeout");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("finally signaling the Infinite.Timeout thread");
            Waiter.are1.Set();

            return testResult;
        }

        [TestMethod]
        public MFTestResults AutoReset5_Reset_Test()
        {
            ///<summary>
            ///Creates an AutoResetEvent having an initial state signaled
            ///Starts a thread, calls WaitOne and verifies the thread is not blocked
            ///Starts another thread, calls WaitOne and verifies it's auto reset (nonsignaled)
            ///calls Set and verify it's set (Signaled)
            ///calls Set, calls Reset, starts a thread and calls WaitOne on the thread
            ///Verifies that the thread remains blocked
            ///</summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Creates an AutoResetEvent having an initial state signaled");
            Log.Comment("Start a thread, call WaitOne and verify the thread is not blocked");
            Log.Comment("Starts a 2nd thread, call WaitOne and verify it's auto reset (thread is blocked)");
            Log.Comment("call Set and verify it's set (signaled)");
            Log.Comment("call Set, call Reset, starts a thread and call WaitOne on the thread");
            Log.Comment("Verify the thread remains blocked");

            Waiter newWaiter1 = new Waiter();
            newWaiter1.flag = false;
            Thread newThread1 = new Thread(newWaiter1.DoWaitReset);
            Log.Comment("Starting thread, waiting and verifying thread not blocked if initial state is signaled");
            newThread1.Start();
            Thread.Sleep(100);
            if (!newWaiter1.flag)
            {
                Log.Comment("Faiure : AutoResetEvent initial state signaled but blocked thread");
                testResult = MFTestResults.Fail;
            }

            Waiter newWaiter2 = new Waiter();
            newWaiter2.flag = false;
            Thread newThread2 = new Thread(newWaiter2.DoWaitReset);
            Log.Comment("Starting thread, waiting and verifying autoreset blocks the thread");
            newThread2.Start();
            Thread.Sleep(100);
            if (newWaiter2.flag)
            {
                Log.Comment("Failure : AutoResetEvent not autoreseted");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Signaling, waiting and verifying");
            Waiter.are2.Set();
            Thread.Sleep(100);
            if (!newWaiter2.flag)
            {
                Log.Comment("Failure : AutoResetEvent signaled but thread blocked");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Set, Reset, Start a thread, waiting and verifying thread remain blocked");
            Waiter newWaiter3 = new Waiter();
            newWaiter3.flag = false;
            Thread newThread3 = new Thread(newWaiter3.DoWaitReset);
              Waiter.are2.Set();
            Waiter.are2.Reset();
               newThread3.Start();
               Thread.Sleep(100);
               if (newWaiter3.flag)
               {
                   Log.Comment("Failure: a Reseted AutoResetEvent didn't block thread");
                   testResult = MFTestResults.Fail;
               }                
          
            Log.Comment("Finally Setting the reseted AutoResetEvent");
            Waiter.are2.Set();

            return testResult;
        }
    }
}






