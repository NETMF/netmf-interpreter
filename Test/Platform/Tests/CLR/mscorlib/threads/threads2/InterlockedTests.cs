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
    public class InterlockedTests : IMFTestInterface
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

        public static int interCount = 0;
        static public void ThreadIncrementor()
        {
            Thread.Sleep(new Random().Next(10));
            Interlocked.Increment(ref interCount);
        }

        static public void ThreadDecrementor()
        {
            Thread.Sleep(new Random().Next(10));
            Interlocked.Decrement(ref interCount);
        }

        static public void ThreadIncrementorStarter()
        {
            Thread[] threadArrayInc = new Thread[4];
            for (int i = 0; i < 4; i++)
            {
                Log.Comment("Attempting to start inc thread " + i);
                threadArrayInc[i] = new Thread(ThreadIncrementor);
                threadArrayInc[i].Start();
                Thread.Sleep(1);
            }
            Thread.Sleep(100);
            for (int i = 0; i < 4; i++)
            {
                threadArrayInc[i].Join();
            }
        }

        static public void ThreadDecrementorStarter()
        {
            Thread[] threadArrayDec = new Thread[5];
            for (int i = 0; i < 5; i++)
            {
                Log.Comment("Attempting to start dec thread " + i);
                threadArrayDec[i] = new Thread(ThreadDecrementor);
                threadArrayDec[i].Start();
                Thread.Sleep(1);
            }
            Thread.Sleep(100);
            for (int i = 0; i < 5; i++)
            {
                threadArrayDec[i].Join();
            }
        }

        static public void ThreadIncrementor2Starter()
        {

            Thread[] threadArrayInc2 = new Thread[6];
            for (int i = 0; i < 6; i++)
            {
                Log.Comment("Attempting to start inc2 thread " + i);
                threadArrayInc2[i] = new Thread(ThreadIncrementor);
                threadArrayInc2[i].Start();
                Thread.Sleep(1);
            }
            Thread.Sleep(100);
            for (int i = 0; i < 6; i++)
            {
                threadArrayInc2[i].Join();
            }
        }

        [TestMethod]
        public MFTestResults Interlocked1_Inc_Dec_Test()
        {
            /// <summary>
            /// 1. Starts 4 threads that run asynchronously
            /// 2. Each thread calls Interlocked.Increment or Decrement.
            /// 3. Waits for execution and then verifies that all expected operations completed
            /// </summary>
            ///
            Log.Comment("Starts several async threads that call interlocked ");
            Log.Comment("Increment and Decrement, this may erroneously pass.");
            Log.Comment("This may erroneously fail for extremely slow devices.");
            MFTestResults testResult = MFTestResults.Pass;

            Log.Comment("Starting several threads, incrementing, decrementing,");
            Log.Comment("waiting till all threads finish and verifying");
            Thread incThread = new Thread(ThreadIncrementorStarter);
            incThread.Start();
            Thread.Sleep(1);

            Thread decThread = new Thread(ThreadDecrementorStarter);
            decThread.Start();
            Thread.Sleep(1);

            Thread inc2Thread = new Thread(ThreadIncrementor2Starter);
            inc2Thread.Start();
            Thread.Sleep(1);

            Thread lastThread = new Thread(ThreadDecrementor);
            lastThread.Start();
            Thread.Sleep(1);

            Log.Comment("Waiting for execution");
            int slept = 0;
            while ((incThread.IsAlive || decThread.IsAlive ||
                inc2Thread.IsAlive || lastThread.IsAlive) && slept < 5000)
            {
                Thread.Sleep(100);
                slept += 100;
            }

            Log.Comment("Verifying all increment/decrement operations done correctly");
            if (interCount != 4)
            {
                Log.Comment("expected final increment/decrement result = '4' but got '" + interCount + "'");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Verifying all threads are finished");
            if (slept >= 5000)
            {
                Log.Comment("Expcted all threads be done in '5000' msec but took '" + slept + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Interlocked2_Compare_Exchange_Test()
        {
            /// <summary>
            /// 1. Starts 4 threads that run asynchronously
            /// 2. Each thread calls Interlocked.Compare or CompareExchange
            /// 3. Waits for execution and then verifies that all expected operations completed
            /// </summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Starts 4 async threads that call interlocked Compare, CompareExchange");
            Log.Comment("This may erroneously pass.");
            Log.Comment("this may erroneously fail for extremely slow devices.");

            Log.Comment("Starting the 4 threads");
            Thread incThread = new Thread(ThreadIncrementorStarter);
            incThread.Start();

            Thread decThread = new Thread(ThreadDecrementorStarter);
            decThread.Start();

            Thread inc2Thread = new Thread(ThreadIncrementor2Starter);
            inc2Thread.Start();

            Thread lastThread = new Thread(ThreadDecrementor);
            lastThread.Start();
            Thread.Sleep(1);
            int comp = 10;
            Log.Comment("Waiting for execution");
            while ((0 != Interlocked.CompareExchange(ref interCount, 0, comp)) ||
                (incThread.IsAlive || decThread.IsAlive || inc2Thread.IsAlive ||
                lastThread.IsAlive))
            {
                Log.Comment(
                    Interlocked.Exchange(ref interCount, 0).ToString() + " " +
                    incThread.IsAlive.ToString() + " " +
                    decThread.IsAlive.ToString() + " " +
                        inc2Thread.IsAlive.ToString() + " " +
                            lastThread.IsAlive.ToString());
                Log.Comment("Setting Count to 0");
                Interlocked.Exchange(ref interCount, 0);
                Thread.Sleep(10);
            }
            int slept = 0;
            while ((incThread.IsAlive || decThread.IsAlive || inc2Thread.IsAlive ||
                lastThread.IsAlive) && slept < 5000)
            {
                Thread.Sleep(100);
                slept += 100;
            }
            Log.Comment("Verifying all Interlocked.Compare/CompareExchange operations done correctly");
            if (interCount != 0)
            {
                Log.Comment("expected final Compare/CompareExchange result = '0' but got '" + interCount + "'");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Verifying all threads are finished");
            if (slept >= 5000)
            {
                Log.Comment("Expcted all threads be done in '5000' msec but took '" + slept + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Interlocked3_Exchange_Boundary_Test()
        {
            /// <summary>
            /// 1. creates 3 int variables initialized to the boundary values of signed int 32
            /// 2. passes all possible combinations of the values to Interlocked.Exchange
            /// 3. verifies Exchange and also verifies original value is returned
            /// </summary>
            ///

            Log.Comment("This is Fixed, see 20323 for details");
            MFTestResults testResult = MFTestResults.Pass;
            int[] value = new int[] { -2147483648, 0, 2147483647 };
            int temp1, temp2;
            Log.Comment("Verification of original value returned needs temp1 = temp2");
            Log.Comment("Verifies Exchange ");

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    temp1 = value[i];
                    temp2 = Interlocked.Exchange(ref value[i], value[j]);
                    if (temp1 != temp2)
                    {
                        Log.Comment("Failure : expected Interlocked.Exchange returns" +
                            " the original value '" + temp1 + "' but got '" + temp2 + "'");
                        break;
                    }
                    if (value[i] != value[j])
                    {
                        Log.Comment("Failure : ");
                        Log.Comment(value[j] + " is not stored at location1");
                        break;
                    }
                    value[i] = temp1;
                }
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults Interlocked4_CompareExchange_Boundary_Test()
        {
            /// <summary>
            /// 1. creates 3 int variables initialized to the boundary values of signed int 32
            /// 2. passes all posible combinations of the values to Interlocked.CompareExchange
            /// 3. verifies Exchange upon equality and also verifies original value is returned
            /// </summary>
            ///

            Log.Comment("This is Fixed, see 20323 for details");
            MFTestResults testResult = MFTestResults.Pass;
            int[] value = new int[] { -2147483648, 0, 2147483647 };
            int temp1, temp2;

            Log.Comment("Verifies the original value is returned at all time");
            Log.Comment("Verifies value is stored in location1 upon equality");
            for (int r = 0; r < 3; r++)
            {
                for (int p = 0; p < 3; p++)
                {
                    for (int q = 0; q < 3; q++)
                    {
                        temp1 = value[r];
                        temp2 = Interlocked.CompareExchange(ref value[r], value[p], value[q]);

                        if (temp1 != temp2)
                        {
                            Log.Comment("Failure : expected Interlocked.CompareExchange returns" +
                                " the original vaue '" + temp1 + "' but returned '" + temp2 + "'");
                            break;
                        }
                        if (r == q)
                        {

                            if (value[r] != value[p])
                            {
                                Log.Comment("Failure : expected Interlocked.CompareExchange replaces the original value '" +
                                    temp1 + "' upon equality of the two comparands '" + value[r] + "' and '" + value[q] + "'");
                                Log.Comment(value[p] + " is not stored at location1");
                                break;
                            }
                        }
                        value[r] = temp1;
                    }
                }
            }
            return testResult;
        }
    }
}
