////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;
using System.Threading;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ThreadTests : IMFTestInterface
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

        //Threading Test methods 
        class Work
        {
            public static void DoWork()
            {
                Thread.Sleep(300);
                Log.Comment("Static thread procedure DoWork.");
                Thread.Sleep(300);
            }
            public int m_data = 0;
            public Thread currThread;
            public void DoMoreWork()
            {
                Log.Comment("Instance thread procedure DoMoreWork. Data= " + m_data);
                currThread = Thread.CurrentThread;
            }
            public static void DoWorkAbort()
            {
                Thread.Sleep(300);
                Log.Comment("Static thread procedure DoWorkAbort.");
                Thread.Sleep(10000);
            }
            public Thread m_toJoin = null;
            public void DoWorkJoin()
            {
                m_toJoin.Join();
                Log.Comment("Instance thread procedure DoWorkJoin.");
            }

            public static bool hasAborted = false;
            public static void DoWorkThreadAbortException()
            {
                while (!hasAborted)
                {
                    try
                    {
                        while (true) ;
                    }
                    catch (ThreadAbortException e)
                    {
                        hasAborted = true;
                        Log.Comment("Thread State = " + Thread.CurrentThread.ThreadState);
                        Log.Comment("verifying ThreadAbortException named " + e.ToString() + " is thrown");
                    }
                }
            }

            public static bool run = false;
            public void DoWorkThreadState()
            {
                while (run)
                    ;
            }
        }

        [TestMethod]
        public MFTestResults Threading_Basic_Test1()
        {
            /// <summary>
            /// 1. Starts two threads
            /// 2. Verifies that they execute in a reasonable time
            /// </summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Starting a thread without explicit declaration of ThreadStart Delegate");
            Log.Comment("Starts two threads, waits for them to complete and passes, ");
            Log.Comment("this may erroneously fail for extremely slow devices.");
            Log.Comment("All other threading tests are dependant on this, if this fails, ");
            Log.Comment("all other results are invalid.");

            Log.Comment("Starting thread 1");
            Thread newThread1 = new Thread(Work.DoWork);
            newThread1.Start();

            Log.Comment("Starting thread 2");
            Work w = new Work();
            w.m_data = 42;
            Thread newThread2 = new Thread(w.DoMoreWork);
            newThread2.Start();
            Thread.Sleep(1);

            Log.Comment("Waiting for them to finish");
            int slept = 0;
            while ((newThread1.ThreadState != ThreadState.Stopped
                || newThread2.ThreadState != ThreadState.Stopped) && slept < 1000)
            {
                Thread.Sleep(100);
                slept += 100;
            }
            if (!(newThread1.ThreadState == ThreadState.Stopped &&
                newThread2.ThreadState == ThreadState.Stopped))
            {
                Log.Comment("The threads took more than 1000msec to come to Stopped state");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Threading_ThreadStart_Test2()
        {
            /// <summary>
            /// 1. Starts two threads  with ThreadStart Delegate,
            /// 2. Verifies that they execute in a reasonable time
            /// </summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Starts two threads with ThreadStart Delegate,");
            Log.Comment("waits for them to complete and passes, ");
            Log.Comment("this may erroneously fail for extremely slow devices.");
            Log.Comment("This explicit declaration is not necessary as of .Net 2.0");

            ThreadStart threadDelegate = new ThreadStart(Work.DoWork);
            Thread newThread1 = new Thread(threadDelegate);
            Log.Comment("Starting thread 1");
            newThread1.Start();
            Work w = new Work();
            w.m_data = 42;
            threadDelegate = new ThreadStart(w.DoMoreWork);
            Thread newThread2 = new Thread(threadDelegate);
            Log.Comment("Starting thread 2");
            newThread2.Start();
            Thread.Sleep(1);

            Log.Comment("Waiting for them to complete");
            int slept = 0;
            while ((newThread1.ThreadState != ThreadState.Stopped
                || newThread2.ThreadState != ThreadState.Stopped) && slept < 1000)
            {
                Thread.Sleep(100);
                slept += 100;
            }
            if (!(newThread1.ThreadState == ThreadState.Stopped
                && newThread2.ThreadState == ThreadState.Stopped))
            {
                Log.Comment("The threads took more than 1000msec to come to Stopped state");
                testResult = MFTestResults.Fail;
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults Threading_Abort_Test3()
        {
            /// <summary>
            /// 1. Starts two threads one of which has a very long execution time
            /// 2. Calls Abort on the long thread
            /// 3. Verifies that the aborted thread stops immediately
            /// 4. Verifies that the short thread finishes normally
            /// </summary>
            ///
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Starting long thread");
            Thread newThread1 = new Thread(Work.DoWorkAbort);
            newThread1.Start();

            Log.Comment("Starting short thread");
            Work w = new Work();
            w.m_data = 42;
            Thread newThread2 = new Thread(w.DoMoreWork);
            newThread2.Start();
            Thread.Sleep(1);

            Log.Comment("Aborting long thread and verifying it's Aborted");
            newThread1.Abort();
            ThreadState tState = newThread1.ThreadState;
            if (tState != ThreadState.Aborted
                        && newThread1.ThreadState != ThreadState.Stopped)
            {
                Log.Comment("Expected long thread state Aborted/Stopped '" + ThreadState.Aborted +
                    "/" + ThreadState.Stopped + "' but got '" + tState + "'");
                testResult = MFTestResults.Fail;
            }
            int slept = 0;

            Log.Comment("Waiting for 1 or both threads to finish");
            while ((newThread1.ThreadState != ThreadState.Stopped ||
                newThread2.ThreadState != ThreadState.Stopped) && slept < 1000)
            {
                Thread.Sleep(100);
                slept += 100;
            }
            ThreadState tState1 = newThread1.ThreadState, tState2 = newThread2.ThreadState;
            if (tState1 != ThreadState.Stopped || tState2 != ThreadState.Stopped)
            {
                Log.Comment("Expected both threads in Stopped state '" + ThreadState.Stopped +
                    "' but got Thread1 in '" + tState1 + "' and Thread2 in '" + tState2 + "'");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("This is Fixed, see 17343 for details");

            return testResult;
        }

        [TestMethod]
        public MFTestResults Threading_IsAlive_Test4()
        {
            /// <summary>
            /// 1. Starts two threads one of which has a very long execution time
            /// 2. Calls Abort() on the long thread
            /// 3. Verifies that the aborted thread stops immediately using the IsAlive Property
            /// 4. Verifies that the short thread finishes normally using the IsAlive Property
            /// </summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Starts two threads, aborts one and verifies the IsAlive property, ");
            Log.Comment("this may erroneously fail for extremely slow devices.");
            Log.Comment("Starting long thread and verifying it's alive");
            Thread newThread1 = new Thread(Work.DoWorkAbort);
            newThread1.Start();

            Log.Comment("Starting short thread");
            Work w = new Work();
            w.m_data = 42;
            Thread newThread2 = new Thread(w.DoMoreWork);
            newThread2.Start();
            if (!newThread1.IsAlive)
            {
                Log.Comment("Long thread not alive");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Aborting long thread, waiting and verifying both threads are dead");
            newThread1.Abort();
            int slept = 0;
            while ((newThread1.IsAlive || newThread2.IsAlive) && slept < 1000)
            {
                Thread.Sleep(100);
                slept += 100;
            }

            if (newThread1.IsAlive || newThread2.IsAlive)
            {
                Log.Comment("Expected both threads dead but got long thread '" +
                    newThread1.ThreadState + "' and short thread '" + newThread2.ThreadState + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Threading_Join_Test5()
        {
            /// <summary>
            /// 1. Starts a thread
            /// 2. Starts a second thread that Join()s the first
            /// 3. Verifies that they finish in a reasonable amount of time
            /// </summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Starts two threads, the second thread Join()s the first");
            Log.Comment("Verifies they finish in a reasonable amount of time");
            Log.Comment("this may erroneously fail for extremely slow or fast devices.");

            Thread newThread1 = new Thread(Work.DoWork);
            newThread1.Start();

            Work w = new Work();
            w.m_data = 42;
            w.m_toJoin = newThread1;
            Thread newThread2 = new Thread(w.DoWorkJoin);
            newThread2.Start();
            Thread.Sleep(1);
            int slept = 0;
            while (newThread2.IsAlive && slept < 1000)
            {
                Thread.Sleep(100);
                slept += 100;
            }
            if (newThread1.IsAlive || newThread2.IsAlive)
            {
                Log.Comment("Expected both threads dead but got thread1 '" + newThread1.ThreadState +
                    "' and thread2 '" + newThread2.ThreadState + "'");
                testResult = MFTestResults.Fail;
            }
            return testResult;
        }

        static class PriorityTest
        {
            static public ManualResetEvent loopSwitch = new ManualResetEvent(false);
            static public ManualResetEvent startSecond = new ManualResetEvent(false);
            static public ManualResetEvent keepAlive = new ManualResetEvent(false);
            static public long resultLowest = 0;
            static public long resultBelow = 0;
            static public long resultNorm = 0;
            static public long resultAbove = 0;
            static public long resultHighest = 0;
            static public long resultControl = 0;

            static public long resultNewLowest = 0;
            static public long resultNewBelow = 0;
            static public long resultNewNorm = 0;
            static public long resultNewAbove = 0;
            static public long resultNewHighest = 0;
            static public long resultNewControl = 0;

            static public void ThreadMethodLowest()
            {
                long threadCount = 0;
                long fakeCount = 0;

                while (!loopSwitch.WaitOne(0, false))
                {
                    threadCount++;
                }
                resultLowest = threadCount;
                startSecond.WaitOne();
                while (!keepAlive.WaitOne(0, false))
                {
                    fakeCount++;
                }
                resultNewLowest = fakeCount;
            }
            static public void ThreadMethodBelow()
            {
                long threadCount = 0;
                long fakeCount = 0;

                while (!loopSwitch.WaitOne(0, false))
                {
                    threadCount++;
                }
                resultBelow = threadCount;
                startSecond.WaitOne();
                while (!keepAlive.WaitOne(0, false))
                {
                    fakeCount++;
                }
                resultNewBelow = fakeCount;
            }
            static public void ThreadMethodNorm()
            {
                long threadCount = 0;
                long fakeCount = 0;

                while (!loopSwitch.WaitOne(0, false))
                {
                    threadCount++;
                }
                resultNorm = threadCount;
                startSecond.WaitOne();
                while (!keepAlive.WaitOne(0, false))
                {
                    fakeCount++;
                }
                resultNewNorm = fakeCount;
            }
            static public void ThreadMethodAbove()
            {
                long threadCount = 0;
                long fakeCount = 0;

                while (!loopSwitch.WaitOne(0, false))
                {
                    threadCount++;
                }
                resultAbove = threadCount;
                startSecond.WaitOne();
                while (!keepAlive.WaitOne(0, false))
                {
                    fakeCount++;
                }
                resultNewAbove = fakeCount;
            }
            static public void ThreadMethodHighest()
            {
                long threadCount = 0;
                long fakeCount = 0;

                while (!loopSwitch.WaitOne(0, false))
                {
                    threadCount++;
                }
                resultHighest = threadCount;
                startSecond.WaitOne();
                while (!keepAlive.WaitOne(0, false))
                {
                    fakeCount++;
                }
                resultNewHighest = fakeCount;
            }
            static public void ThreadMethodControl()
            {
                long threadCount = 0;
                long fakeCount = 0;

                while (!loopSwitch.WaitOne(0, false))
                {
                    threadCount++;
                }
                resultControl = threadCount;
                startSecond.WaitOne();
                while (!keepAlive.WaitOne(0, false))
                {
                    fakeCount++;
                }
                resultNewControl = fakeCount;
            }
        }

        static Thread threadLowest = new Thread(PriorityTest.ThreadMethodLowest);
        static Thread threadBelow = new Thread(PriorityTest.ThreadMethodBelow);
        static Thread threadNorm = new Thread(PriorityTest.ThreadMethodNorm);
        static Thread threadAbove = new Thread(PriorityTest.ThreadMethodAbove);
        static Thread threadHighest = new Thread(PriorityTest.ThreadMethodHighest);
        static Thread threadControl = new Thread(PriorityTest.ThreadMethodControl);

        public double Tolerance(long level1, long level2)
        {
            long temp = System.Math.Abs((int)(level1 - level2));

            return (level2 == 0) ? 100.0 : (temp * 100) / level2;
        }

        [TestMethod]
        public MFTestResults Threading_Priority_Test6()
        {
            /// <summary>
            /// 1. Starts five threads of increasing priority
            /// 2. Waits for them to complete work
            /// 3. Verifies that they get increasing amounts of attention
            /// </summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            const double acceptedTolerance = 5.0; // 5% tolerance

            Log.Comment("Starts five threads of increasing priority and a control thread, priority not set ");
            Log.Comment("verifies that they get increasing amounts of attention");
            Log.Comment("This is Fixed, see 17201 for details");

            threadLowest.Priority = ThreadPriority.Lowest;
            threadBelow.Priority = ThreadPriority.BelowNormal;
            threadNorm.Priority = ThreadPriority.Normal;
            threadAbove.Priority = ThreadPriority.AboveNormal;
            threadHighest.Priority = ThreadPriority.Highest;

            Log.Comment("Starting Threads");
            threadHighest.Start();
            threadAbove.Start();
            threadNorm.Start();
            threadBelow.Start();
            threadLowest.Start();
            threadControl.Start();

            Log.Comment("Allow counting for 1 seconds.");
            Thread.Sleep(1000);
            PriorityTest.loopSwitch.Set();
            Thread.Sleep(1000);

            Log.Comment("Lowest         " + PriorityTest.resultLowest);
            Log.Comment("Below          " + PriorityTest.resultBelow);
            Log.Comment("Normal         " + PriorityTest.resultNorm);
            Log.Comment("Above          " + PriorityTest.resultAbove);
            Log.Comment("Highest        " + PriorityTest.resultHighest);
            Log.Comment("Control Thread " + PriorityTest.resultControl);

            Log.Comment("Verifies that each thread recieves attention less than or equal");
            Log.Comment("to higher priority threads.");
            Log.Comment("Accepted tolerance : " + acceptedTolerance + "%");

            PriorityTest.startSecond.Set();
            PriorityTest.keepAlive.Set();

            if ((PriorityTest.resultLowest <= 0) ||
                (Tolerance(2 * PriorityTest.resultLowest, PriorityTest.resultBelow) > acceptedTolerance) ||
                (Tolerance(2 * PriorityTest.resultBelow, PriorityTest.resultNorm) > acceptedTolerance) ||
                (Tolerance(2 * PriorityTest.resultNorm, PriorityTest.resultAbove) > acceptedTolerance) ||
                (Tolerance(2 * PriorityTest.resultAbove, PriorityTest.resultHighest) > acceptedTolerance) ||
                (Tolerance(PriorityTest.resultNorm, PriorityTest.resultControl) > acceptedTolerance))
            {
                Log.Comment("Lowest thread should execute at least once, got " + PriorityTest.resultLowest);
                Log.Comment("Deviation b/n 2*Lowest and Below " + Tolerance(2 * PriorityTest.resultLowest, PriorityTest.resultBelow));
                Log.Comment("Deviation b/n 2*Below and Normal " + Tolerance(2 * PriorityTest.resultBelow, PriorityTest.resultNorm));
                Log.Comment("Deviation b/n 2*Normal and Above " + Tolerance(2 * PriorityTest.resultNorm, PriorityTest.resultAbove));
                Log.Comment("Deviation b/n 2*Above and Highest " + Tolerance(2 * PriorityTest.resultAbove, PriorityTest.resultHighest));
                Log.Comment("Deviation b/n Normal and Control " + Tolerance(PriorityTest.resultNorm, PriorityTest.resultControl));
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Threading_Suspend_Resume_Test7()
        {
            /// <summary>
            /// 1. Starts two threads
            /// 2. Suspends them
            /// 3. Verifies that they do not terminate while suspended
            /// 4. Resumes them
            /// 5. Verifies that they finish in a reasonable amount of time
            /// </summary>
            ///
            Log.Comment("Starts two threads, suspends and resumes them, ");
            Log.Comment("this may erroneously fail for extremely slow devices.");
            MFTestResults testResult = MFTestResults.Pass;

            Thread newThread1 = new Thread(Work.DoWork);
            newThread1.Start();

            Work w = new Work();
            w.m_data = 42;
            w.m_toJoin = newThread1;
            Thread newThread2 = new Thread(w.DoWorkJoin);
            newThread2.Start();
            Thread.Sleep(1);

            newThread2.Suspend();
            newThread1.Suspend();
            ThreadState tState = newThread2.ThreadState;
            if ((int)tState != 96)
            {
                Log.Comment("expected Thread2 in WaitSleepJoin + Suspended ('96') but got '" +
                    tState + "'");
                testResult = MFTestResults.Fail;
            }
            newThread2.Resume();
            tState = newThread1.ThreadState;
            if ((int)tState != 96)
            {
                Log.Comment("expected Thread1 in WaitSleepJoin + Suspended ('96') but got '" +
                   tState + "'");
                testResult = MFTestResults.Fail;
            }
            newThread1.Resume();

            int slept = 0;
            while ((newThread1.IsAlive || newThread2.IsAlive) && slept < 1000)
            {
                Thread.Sleep(100);
                slept += 100;
            }
            if (newThread1.IsAlive || newThread2.IsAlive)
            {
                Log.Comment("expected both threads dead after 1000msec but got thread1 '" +
                    newThread1.ThreadState + "' and thread2 '" + newThread2.ThreadState + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        public static bool sleptCorrect(int Msec)
        {
            TimeSpan zeroDuration = new TimeSpan(0);
            const long TicksPerMillisecond = 10000;
            DateTime startTime = DateTime.Now;

            Thread.Sleep(Msec);
            TimeSpan sleptSpan = DateTime.Now - startTime;
            if ((sleptSpan.Ticks / TicksPerMillisecond) < Msec)
            {
                Log.Comment("Expected the thread slept for at least " + Msec + " Msec. but slept only for "
                    + (sleptSpan.Ticks / TicksPerMillisecond) + " Msec");
                return false;
            }
            Log.Comment(Msec + " Msec sleep success, slept for "
                + (sleptSpan.Ticks / TicksPerMillisecond) + " Msec");
            return true;
        }

        [TestMethod]
        public MFTestResults Threading_SleepApprox_Test8()
        {
            /// <summary>
            /// 1. Sleeps the main thread for increasing amounts of time
            /// 2. Verifies the thread sleeps at least for the time requested         
            /// </summary>
            ///          
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("This test verifies the thread slept at least for the amount of time required");
            Log.Comment("This is Fixed, see  20831 for details");
            int[] sleepTime = new int[] { 10, 100, 1000, 10000, 60000 };
            for (int i = 0; i < sleepTime.Length; i++)
            {
                if (!sleptCorrect(sleepTime[i]))
                {
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Threading_Suspend_Suspend_Test9()
        {
            /// <summary>
            /// 1. Starts two threads and suspends the first thread twice
            /// 2. Gets the state of the 1st thread
            /// 3. Resumes the 1st thread 
            /// 4. Verifies that calling Suspend for the 2nd time has no effect
            /// </summary>
            ///

            Log.Comment("Starts two threads and suspends the first thread twice");
            Log.Comment("Gets the state of the 1st thread");
            Log.Comment("Resumes the 1st thread ");
            Log.Comment("Verifies that calling Suspend for the 2nd time has no effect");
            Log.Comment("This is Fixed, see 20247 for details");
            MFTestResults testResult = MFTestResults.Pass;
            Work.run = true;
            Work w1 = new Work();
            Thread newThread1 = new Thread(w1.DoWorkThreadState);
            newThread1.Start();
            newThread1.Suspend();
            newThread1.Suspend();

            Work w2 = new Work();
            w2.m_data = 42;
            Thread newThread2 = new Thread(w2.DoMoreWork);
            newThread2.Start();

            ThreadState tState = newThread1.ThreadState;
            newThread1.Resume();
            newThread1.Abort();
            if (tState != ThreadState.Suspended)
            {
                Log.Comment("Suspending twice, expected thread state Suspended(" +
                    ThreadState.Suspended + ") but got '" + tState + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Threading_ThreadState_Unstarted_Running_WaitSleepJoin_Test10()
        {
            /// <summary>
            /// 1. Creates a thread and verifies its state is Unstarted
            /// 2. Starts a thread and verifies its state is Running
            /// 3. Sleeps a thread and verifies its state is WaitSleepJoin
            /// 4. Joins a thread and verifies its state is WaitSleepJoin
            /// </summary>
            ///

            Log.Comment("Creating a thread and verifing its state is Unstarted");
            MFTestResults testResult = MFTestResults.Pass;
            Thread newThread1 = new Thread(Work.DoWork);
            ThreadState tState = newThread1.ThreadState;
            if (tState != ThreadState.Unstarted)
            {
                Log.Comment("Expected thread state Unstarted(" + ThreadState.Unstarted +
                    ") but got '" + tState + "'");
                testResult = MFTestResults.Fail;
            }

            Log.Comment("Verifying the state of the current thread is Running");
            newThread1.Start();
            tState = Thread.CurrentThread.ThreadState;
            if (tState != ThreadState.Running)
            {
                Log.Comment("expected the state of current thread Running(" + ThreadState.Running +
                    ") but got '" + tState + "'");
                testResult = MFTestResults.Fail;
            }

            Log.Comment("Sleeping a thread and verifing its state is WaitSleepJoin");
            Thread.Sleep(100);
            tState = newThread1.ThreadState;
            if (tState != ThreadState.WaitSleepJoin)
            {
                Log.Comment("expected thread1 in WaitSleepJoin(" + ThreadState.WaitSleepJoin +
                    ") but got '" + newThread1.ThreadState + "'");
                testResult = MFTestResults.Fail;
            }
            Log.Comment(" Joining a thread and verifing its state is WaitSleepJoin");
            Work w = new Work();
            Thread newThread3 = new Thread(Work.DoWork);
            w.m_toJoin = newThread3;
            Thread newThread2 = new Thread(w.DoWorkJoin);
            newThread3.Start();
            newThread2.Start();
            Thread.Sleep(0);
            tState = newThread2.ThreadState;
            if (tState != ThreadState.WaitSleepJoin)
            {
                Log.Comment("expected a joined sleeping thread in WaitSleepJoin state(" +
                    ThreadState.WaitSleepJoin + ") but got '" + tState + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Threading_ThreadState_Suspend_Test11()
        {
            /// <summary>
            /// 1. Starts a thread and Suspends it immediately
            /// 2. Starts a second thread
            /// 3. Gets the state of the 1st thread and Resumes it
            /// 4. Verifies that the state of the 1st thread was Suspended
            /// </summary>
            ///

            Log.Comment("Starts a thread and Suspends it immediately");
            Log.Comment("Starts a second thread");
            Log.Comment("Gets the state of the 1st thread and Resumes it");
            Log.Comment("Verifies that the state of the 1st thread was Suspended");
            Log.Comment("This is Fixed, see 20249 for details");
            MFTestResults testResult = MFTestResults.Pass;
            Work.run = true;
            Work w1 = new Work();
            Thread newThread1 = new Thread(w1.DoWorkThreadState);
            newThread1.Start();
            newThread1.Suspend();

            Work w2 = new Work();
            w2.m_data = 42;
            Thread newThread2 = new Thread(w2.DoMoreWork);
            newThread2.Start();

            ThreadState tState = newThread1.ThreadState;
            newThread1.Resume();
            newThread1.Abort();
            if (tState != ThreadState.Suspended)
            {
                Log.Comment("expected state Suspended(" + ThreadState.Suspended + ") but got '" + tState + "'");
                testResult = MFTestResults.Fail;

            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults Threading_ThreadState_SuspendRequested_Test12()
        {
            /// <summary>
            /// 1. Starts 10 threads and Suspends all of them 
            /// 2. Immediately verifies the state is SuspendRequested
            /// 3. It's an approximate test
            /// </summary>
            ///

            Log.Comment("This currently fails, see 20737 for details");
            Log.Comment("Starting 10 Threads");
            MFTestResults testResult = MFTestResults.KnownFailure;
            Thread[] newThread = new Thread[10];
            Work[] w = new Work[10];
            int i = 0, k = 0;
            Work.run = true;
            while (i < 10)
            {
                w[i] = new Work();
                newThread[i] = new Thread(w[i].DoWorkThreadState);
                newThread[i].Start();
                i++;
            }
            Log.Comment("Suspending the Threads and checking for SuspendRequested");
            Log.Comment("At least one of the threads should be in SuspendRequested");
            while (k < 10)
            {
                while (newThread[k].ThreadState != ThreadState.Suspended)
                {
                    newThread[k].Suspend();
                    if (newThread[k].ThreadState == ThreadState.SuspendRequested)
                    {
                        testResult = MFTestResults.Pass;
                        break;
                    }
                }
                newThread[k].Resume();
                k++;
            }
            k--;
            while (k >= 0)
            {
                newThread[k].Abort();
                k--;
            }
            Work.run = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults Threading_ThreadState_Abort_Stopped_Test13()
        {
            /// <summary>
            /// 1. Starts a thread and Aborts it immediately
            /// 2. Starts a second thread
            /// 3. Gets the state of the 1st thread thread
            /// 4. Verifies the state of the 1st thread is Aborted
            /// </summary>
            ///

            Log.Comment("Starts a thread and Aborts it immediately");
            Log.Comment("Starts a second thread");
            Log.Comment("Gets the state of the 1st thread");
            Log.Comment("Verifies the state of the 1st thread is Aborted");
            Log.Comment("This is Fixed, see 20495 for details");
            MFTestResults testResult = MFTestResults.Pass;

            DateTime t1, t2;
            TimeSpan period;
            Thread newThread1 = new Thread(Work.DoWork);
            newThread1.Start();
            newThread1.Abort();
            t1 = DateTime.Now;

            Work w = new Work();
            w.m_data = 42;
            Thread newThread2 = new Thread(w.DoMoreWork);
            newThread2.Start();
            Log.Comment("Pass Requires either of the next two compared values to be equal");
            Log.Comment(newThread1.ThreadState + " Compare to " + ThreadState.Aborted);
            Log.Comment(newThread1.ThreadState + " Compare to " + ThreadState.Stopped);
            t2 = DateTime.Now;
            ThreadState tState = newThread1.ThreadState;
            period = t2 - t1;
            Log.Comment("Waited for at least " + period.Milliseconds.ToString() + " before checking the state");
            if (tState != ThreadState.Aborted && tState != ThreadState.Stopped)
            {
                Log.Comment("expected the thread to be in Aborted/Stopped(" + ThreadState.Aborted +
                    "/" + ThreadState.Stopped + ") state but got '" + tState + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Threading_Abort_ThreadStateException_Test14()
        {
            /// <summary>
            /// 1. Creates a thread, Aborts it without starting it
            /// 2. Verifies Exception is thrown.
            /// </summary>
            ///

            Log.Comment("The type of exception thrown should be ThreadStateException");
            MFTestResults testResult = MFTestResults.Fail;
            Thread newThread1 = new Thread(Work.DoWork);
            try
            {
                Log.Comment("Aborting a thread not started should throw exception");
                newThread1.Abort();
            }
            catch (Exception e)
            {
                Log.Comment("Correctly threw " + e.ToString() + " in an attempt to Abort Unstarted thread");
                testResult = MFTestResults.Pass;
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults Threading_Abort_ThreadAbortException_Test15()
        {
            /// <summary>
            /// 1. starts a thread and Aborts it immediately
            /// 2. verifies ThreadAbortException is thrown
            /// </summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            Work.hasAborted = false;
            Thread newThread1 = new Thread(Work.DoWorkThreadAbortException);
            Log.Comment("This is Fixed, see 20743 for details");
            Log.Comment("starting a thread and Aborting it immediately");
            newThread1.Start();
            Thread.Sleep(50);
            newThread1.Abort();
            Thread.Sleep(500);
            Log.Comment("Verifying");
            if (!Work.hasAborted)
            {
                Log.Comment("Aborting a Thread didn't throw ThreadAbortException");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Threading_Join_Timeout_Test16()
        {
            /// <summary>
            /// 1. Starts two threads
            /// 2. both threads joins the main thread using Join(int millisecondsTimeout) Join(TimeSpan timeout)
            /// 3. Verifies the calling thread is blocked until the specified amount of time elapses
            /// </summary>
            ///

            Log.Comment("Starts two threads");
            Log.Comment("Both threads Joins the main thread");
            Log.Comment("verify calling thread (main thread) is blocked for millisecondsTimeout");
            Log.Comment("This is fixed, see 20739 for details");
            MFTestResults testResult = MFTestResults.Pass;
            Work.run = true;
            Work w = new Work();

            Thread newThread1 = new Thread(w.DoWorkThreadState);
            newThread1.Start();
            Thread.Sleep(50);
            DateTime d1 = DateTime.Now;
            newThread1.Join(250);
            DateTime d2 = DateTime.Now;
            TimeSpan duration1 = d2 - d1;
            newThread1.Abort();
            Thread newThread2 = new Thread(w.DoWorkThreadState);
            newThread2.Start();
            Thread.Sleep(50);
            DateTime d3 = DateTime.Now;
            newThread1.Join(new TimeSpan(0, 0, 0, 0, 250));
            DateTime d4 = DateTime.Now;
            TimeSpan duration2 = d4 - d3;
            newThread2.Abort();
            int result1 = duration1.CompareTo(new TimeSpan(0, 0, 0, 0, 250));
            int result2 = duration1.CompareTo(new TimeSpan(0, 0, 0, 0, 250));
            if (result1 < 0 || result2 < 0)
            {
                Log.Comment("expected the main thread to be blocked at least for '250' msec. but was blocked for '" +
                    duration1.ToString() + "' by Thread1 and '" + duration2.ToString() + "' by Thread2");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Threading_Join_ArgumentOutOfRangeException_Test17()
        {
            /// <summary>
            /// 1. Start a thread that Join(TimeSpan timeout)s passing negative value 
            /// 2. Verify ArgumentOutOfRangeException is thrown
            /// </summary>

            MFTestResults testResult = MFTestResults.Fail;
            Thread newThread1 = new Thread(Work.DoWork);
            newThread1.Start();
            try
            {
                Log.Comment("Joining a thread passing -ve timeout value should throw ArgumentOutOfRangeException");
                newThread1.Join(-77);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Log.Comment("Correctly threw exception on -ve timeout value name " + e.ToString());
                testResult = MFTestResults.Pass;
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults Threading_Sleep_ArgumentOutOfRangeException_Test18()
        {
            /// <summary>
            /// 1. Sleep a thread passing a negative argument
            /// 2. Verify ArgumentOutOfRangeException exception is thrown
            /// </summary>
            /// 

            MFTestResults testResult = MFTestResults.Fail;
            Log.Comment("Verify ArgumentOutOfRangeException exception is thrown");
            Log.Comment("Why not for -1 ?");
            try
            {
                Log.Comment("Sleeping a thread passing negative time-out argument should throw ArgumentOutOfRangeException");
                Thread.Sleep(-2);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Log.Comment("Correctly threw excption on -ve timeout argument name " + e.ToString());
                testResult = MFTestResults.Pass;
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults Threading_Join_ThreadStateException_Test19()
        {
            /// <summary>
            /// 1. Join a thread that is not Started 
            /// 2. verify ThreadStateException is thrown
            /// </summary>
            /// 

            Log.Comment("The type of exception thrown should be ThreadStateException");
            MFTestResults testResult = MFTestResults.Fail;
            Thread newThread1 = new Thread(Work.DoWork);
            try
            {
                Log.Comment("Joining a thread not started should throw ThreadStateException exception");
                newThread1.Join();
            }
            catch (Exception e)
            {
                Log.Comment("Correctly threw " + e.ToString() + " in attempting to Join Unstarted thread");
                testResult = MFTestResults.Pass;
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults Threading_Suspend_ThreadStateException_Test20()
        {
            /// <summary>
            /// 1. Suspend a thread that is not Started
            /// 2. verify ThreadStateException is thrown
            /// </summary>
            /// 

            Log.Comment("The type of exception thrown should be ThreadStateException");
            MFTestResults testResult = MFTestResults.Fail;
            Thread newThread1 = new Thread(Work.DoWork);
            try
            {
                Log.Comment("Suspending a thread not started should throw exception");
                newThread1.Suspend();
            }
            catch (Exception e)
            {
                Log.Comment("Correctly threw " + e.ToString() + " in an attempt to Suspend Unstarted thread");
                testResult = MFTestResults.Pass;
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults Threading_Resume_ThreadStateException_Test21()
        {

            /// <summary>
            /// 1. Resume a thread that is not Started
            /// 2. verify ThreadStateException is thrown
            /// </summary>
            ///

            Log.Comment("The type of exception thrown should be ThreadStateException");
            MFTestResults testResult = MFTestResults.Fail;
            Thread newThread1 = new Thread(Work.DoWork);
            try
            {
                Log.Comment("Resuming a thread not Suspended should throw exception");
                newThread1.Resume();
            }
            catch (Exception e)
            {
                Log.Comment("Correctly threw " + e.ToString() + " in an attempt to Resume UnSuspended thread");
                testResult = MFTestResults.Pass;
            }
            return testResult;
        }

        static bool sleepZero = false;
        static bool sleepResult = false;
        public static void SleepTest()
        {
            while (sleepZero)
                sleepResult = true;
        }

        [TestMethod]
        public MFTestResults Threading_Sleep_Zero_Test22()
        {
            /// <summary>
            /// 1. Start a thread, 
            /// 2. Sleep the main thread for 0 (zero) milliseconds and 
            /// 3. verify that thread rescheduling is taken place right away
            /// </summary>
            /// 

            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Starting a thread , Thread.Sleep(0) on the main thread");
            Log.Comment("Verify the thread is immediately scheduled to execute");
            Log.Comment("This is fixed, see 20753 for details");
            sleepZero = true;
            Thread t1 = new Thread(new ThreadStart(SleepTest));
            t1.Start();
            Thread.Sleep(50);
            sleepResult = false;
            Thread.Sleep(0);
            if (!sleepResult)
            {
                Log.Comment("Test Thread.Sleep(0) Failed");
                testResult = MFTestResults.Fail;
            }
            else
            {
                Log.Comment("Test Thread.Sleep(0) Successful");
            }
            sleepZero = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults Threading_Priority_Change_Test23()
        {
            /// <summary>
            /// 1. Starts five threads of increasing priority
            /// 2. Change their priorities to different levels
            /// 3. Verifies that they get increasing amounts of attention
            /// 4. based on their new priority level.
            /// </summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            const double acceptedTolerance = 5.0; // 5% tolerance
            Log.Comment("Starts five threads of increasing priority and ");
            Log.Comment("a control thread priority not set ");
            Log.Comment("verifies that they get increasing amounts of attention");

            PriorityTest.loopSwitch.Reset();
            PriorityTest.startSecond.Reset();
            PriorityTest.keepAlive.Reset();

            Thread threadLowest = new Thread(PriorityTest.ThreadMethodLowest);
            Thread threadBelow = new Thread(PriorityTest.ThreadMethodBelow);
            Thread threadNorm = new Thread(PriorityTest.ThreadMethodNorm);
            Thread threadAbove = new Thread(PriorityTest.ThreadMethodAbove);
            Thread threadHighest = new Thread(PriorityTest.ThreadMethodHighest);
            Thread threadControl = new Thread(PriorityTest.ThreadMethodControl);

            threadLowest.Priority = ThreadPriority.Lowest;
            threadBelow.Priority = ThreadPriority.BelowNormal;
            threadNorm.Priority = ThreadPriority.Normal;
            threadAbove.Priority = ThreadPriority.AboveNormal;
            threadHighest.Priority = ThreadPriority.Highest;

            Log.Comment("Starting Threads");
            threadHighest.Start();
            threadAbove.Start();
            threadNorm.Start();
            threadBelow.Start();
            threadLowest.Start();
            threadControl.Start();

            Log.Comment("Allow counting for 1 seconds.");
            Thread.Sleep(1000);
            PriorityTest.loopSwitch.Set();
            Thread.Sleep(1000);

            Log.Comment("Lowest         " + PriorityTest.resultLowest);
            Log.Comment("Below          " + PriorityTest.resultBelow);
            Log.Comment("Normal         " + PriorityTest.resultNorm);
            Log.Comment("Above          " + PriorityTest.resultAbove);
            Log.Comment("Highest        " + PriorityTest.resultHighest);
            Log.Comment("Control Thread " + PriorityTest.resultControl);

            threadLowest.Priority = ThreadPriority.BelowNormal;
            threadBelow.Priority = ThreadPriority.Normal;
            threadNorm.Priority = ThreadPriority.AboveNormal;
            threadAbove.Priority = ThreadPriority.Highest;
            threadHighest.Priority = ThreadPriority.Lowest;
            Log.Comment("Thread Priorities of each thread changed");
            Log.Comment("Allow counting for 1 seconds.");

            PriorityTest.startSecond.Set();
            Thread.Sleep(1000);
            PriorityTest.keepAlive.Set();
            Thread.Sleep(1000);

            Log.Comment("Lowest - > Below   " + PriorityTest.resultNewLowest);
            Log.Comment("Below - > Normal   " + PriorityTest.resultNewBelow);
            Log.Comment("Normal - > Above   " + PriorityTest.resultNewNorm);
            Log.Comment("Above - > Highest  " + PriorityTest.resultNewAbove);
            Log.Comment("Highest - > Lowest " + PriorityTest.resultNewHighest);
            Log.Comment("Control Thread     " + PriorityTest.resultNewControl);

            Log.Comment("Verifies that each thread recieves attention less than or equal");
            Log.Comment("to higher priority threads based on the newly assigned priorities.");
            Log.Comment("Accepted Tolerance : " + acceptedTolerance + "%");

            if ((PriorityTest.resultNewHighest <= 0) ||
                (Tolerance(2 * PriorityTest.resultNewHighest, PriorityTest.resultNewLowest) > acceptedTolerance) ||
                (Tolerance(2 * PriorityTest.resultNewLowest, PriorityTest.resultNewBelow) > acceptedTolerance) ||
                (Tolerance(2 * PriorityTest.resultNewBelow, PriorityTest.resultNewNorm) > acceptedTolerance) ||
                (Tolerance(2 * PriorityTest.resultNewNorm, PriorityTest.resultNewAbove) > acceptedTolerance) ||
                (Tolerance(PriorityTest.resultNewBelow, PriorityTest.resultNewControl) > acceptedTolerance))
            {
                Log.Comment("NewHighest thread should execute at least once, got " + PriorityTest.resultNewHighest);
                Log.Comment("Deviation b/n 2*NewHighest and NewLowest " + Tolerance(2 * PriorityTest.resultNewHighest, PriorityTest.resultNewLowest));
                Log.Comment("Deviation b/n 2*NewLowest and NewBelow " + Tolerance(2 * PriorityTest.resultNewLowest, PriorityTest.resultNewBelow));
                Log.Comment("Deviation b/n 2*NewBelow and NewNorm " + Tolerance(2 * PriorityTest.resultNewBelow, PriorityTest.resultNewNorm));
                Log.Comment("Deviation b/n 2*NewNorm and NewAbove " + Tolerance(2 * PriorityTest.resultNewNorm, PriorityTest.resultNewAbove));
                Log.Comment("Deviation b/n NewBelow and Control " + Tolerance(PriorityTest.resultNewBelow, PriorityTest.resultNewControl));
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Threading_CurrentThread_Test24()
        {
            /// <summary>
            /// 1. Start a thread
            /// 2. Verify Thread.CurrentThread gives the thread itself
            /// </summary>
            /// 

            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Starting the Thread");
            Work w = new Work();
            w.m_data = 7;
            Thread newThread1 = new Thread(w.DoMoreWork);
            newThread1.Start();
            Thread.Sleep(1);
            Log.Comment("verifying Thread.CurrentThread gives the Thread itself");
            if (!newThread1.Equals(w.currThread))
            {
                Log.Comment("Comparing the Thread with its own (Thread.Equals(Thread.CurrentThread)) failed");
                testResult = MFTestResults.Fail;
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults Threading_GetDomain_Test25()
        {
            /// <summary>
            /// 1. Get the application domain in which the current thread is running
            /// 2. Verify it's the current domain
            /// </summary>
            /// 

            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Getting the AppDomain");
            AppDomain domain = Thread.GetDomain();
            Log.Comment("Verifying the domain");

            if (!domain.Equals(AppDomain.CurrentDomain))
            {
                Log.Comment("Thread.GetDomain().Equals(AppDomain.CurrentDomain) Failed");
                testResult = MFTestResults.Fail;
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults Threading_Thread_CurrentThread_Suspend_Test26()
        {
            MFTestResults testResult = MFTestResults.Pass;         
            int count = 0;
            Log.Comment("This is fixed, see 19911 for details");
            Thread newThread = new Thread(new ThreadStart(
                delegate
                {
                    count += 2;
                    Log.Comment("2) Worker thread started...");
                    Log.Comment("Suspending Worker thread...");
                    Thread.CurrentThread.Suspend();
                    Log.Comment("4) Worker thread resumed...");
                    count += 5;
                }
                ));

            if (count != 0)
            {
                Log.Comment("Failure verifying counter reset to zero before starting the thread");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("1) Starting worker...");
            newThread.Start();
            Thread.Sleep(3000);
            if (count != 2)
            {
                testResult = MFTestResults.Fail;
                Log.Comment("Failure : Worker Thread is not Suspended !");
            }
            Log.Comment("3) Wake up suspended thread...");
            newThread.Resume();
            Thread.Sleep(3000);
            Log.Comment("5) Main thread finished");
            if (count != 7)
            {
                Log.Comment("Worker thread not finished for 3000msec after resumed");
                testResult = MFTestResults.Fail;
            }
            return testResult;
        }
    }
}

