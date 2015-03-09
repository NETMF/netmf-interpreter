////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.SPOT.Platform.Test;
using System.Collections;

namespace Microsoft.SPOT.Platform.Tests
{
    public class QueueTests : IMFTestInterface
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

        //Test Case Calls 
        [TestMethod]
        public MFTestResults EnqueueDequeuePeekTest1()
        {
            try
            {
                Queue queue = new Queue();

                queue.Enqueue(TestObjects.u1);
                queue.Enqueue(TestObjects.s1);
                queue.Enqueue(TestObjects.u2);
                queue.Enqueue(TestObjects.s2);
                queue.Enqueue(TestObjects.u4);
                queue.Enqueue(TestObjects.s4);
                queue.Enqueue(TestObjects.u8);
                queue.Enqueue(TestObjects.s8);
                queue.Enqueue(TestObjects.f4);
                queue.Enqueue(TestObjects.f8);
                queue.Enqueue(TestObjects.c2);
                queue.Enqueue(TestObjects.str);
                queue.Enqueue(TestObjects.dt);
                queue.Enqueue(TestObjects.ts);
                queue.Enqueue(TestObjects.st);
                queue.Enqueue(TestObjects.cl);
                queue.Enqueue(TestObjects.o);
                queue.Enqueue(TestObjects.nul);
                queue.Enqueue(TestObjects.en);

                Assert.AreEqual(19, queue.Count, "Queue.Count is incorrect");

                Assert.AreEqual(TestObjects.u1, queue.Peek());
                Assert.AreEqual(TestObjects.u1, queue.Dequeue());
                Assert.AreEqual(TestObjects.s1, queue.Peek());
                Assert.AreEqual(TestObjects.s1, queue.Dequeue());
                Assert.AreEqual(TestObjects.u2, queue.Peek());
                Assert.AreEqual(TestObjects.u2, queue.Dequeue());
                Assert.AreEqual(TestObjects.s2, queue.Peek());
                Assert.AreEqual(TestObjects.s2, queue.Dequeue());
                Assert.AreEqual(TestObjects.u4, queue.Peek());
                Assert.AreEqual(TestObjects.u4, queue.Dequeue());
                Assert.AreEqual(TestObjects.s4, queue.Peek());
                Assert.AreEqual(TestObjects.s4, queue.Dequeue());
                Assert.AreEqual(TestObjects.u8, queue.Peek());
                Assert.AreEqual(TestObjects.u8, queue.Dequeue());
                Assert.AreEqual(TestObjects.s8, queue.Peek());
                Assert.AreEqual(TestObjects.s8, queue.Dequeue());
                Assert.AreEqual(TestObjects.f4, queue.Peek());
                Assert.AreEqual(TestObjects.f4, queue.Dequeue());
                Assert.AreEqual(TestObjects.f8, queue.Peek());
                Assert.AreEqual(TestObjects.f8, queue.Dequeue());
                Assert.AreEqual(TestObjects.c2, queue.Peek());
                Assert.AreEqual(TestObjects.c2, queue.Dequeue());
                Assert.AreEqual(TestObjects.str, queue.Peek());
                Assert.AreEqual(TestObjects.str, queue.Dequeue());
                //Assert.AreEqual(TestObjects.dt, queue.Peek()); // Throws CLR_E_WRONG_TYPE exception known bug# 18505
                /*Assert.AreEqual(TestObjects.dt,*/ queue.Dequeue()/*)*/; // Throws CLR_E_WRONG_TYPE exception known bug# 18505
                Assert.AreEqual(TestObjects.ts, queue.Peek());
                Assert.AreEqual(TestObjects.ts, queue.Dequeue());
                Assert.AreEqual(TestObjects.st, queue.Peek());
                Assert.AreEqual(TestObjects.st, queue.Dequeue());
                Assert.AreEqual(TestObjects.cl, queue.Peek());
                Assert.AreEqual(TestObjects.cl, queue.Dequeue());
                Assert.AreEqual(TestObjects.o, queue.Peek());
                Assert.AreEqual(TestObjects.o, queue.Dequeue());
                Assert.AreEqual(TestObjects.nul, queue.Peek());
                Assert.AreEqual(TestObjects.nul, queue.Dequeue());
                Assert.AreEqual(TestObjects.en, queue.Peek());
                Assert.AreEqual(TestObjects.en, queue.Dequeue());

                Assert.AreEqual(0, queue.Count, "Queue.Count is incorrect");
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults EnqueueDequeuePeekTest2()
        {
            try
            {
                Queue queue = new Queue();

                for (int i = 0; i < 8; i++)
                {
                    queue.Enqueue(i);
                }

                Assert.AreEqual(8, queue.Count, "Queue.Count is incorrect");

                for (int i = 0; i < 4; i++)
                {
                    Assert.AreEqual(i, queue.Peek());
                    Assert.AreEqual(i, queue.Dequeue());
                }

                Assert.AreEqual(4, queue.Count, "Queue.Count is incorrect");

                for (int i = 8; i < 12; i++)
                {
                    queue.Enqueue(i);
                }

                Assert.AreEqual(8, queue.Count, "Queue.Count is incorrect");

                for (int i = 4; i < 6; i++)
                {
                    Assert.AreEqual(i, queue.Peek());
                    Assert.AreEqual(i, queue.Dequeue());
                }

                Assert.AreEqual(6, queue.Count, "Queue.Count is incorrect");

                for (int i = 12; i < 16; i++)
                {
                    queue.Enqueue(i);
                }

                Assert.AreEqual(10, queue.Count, "Queue.Count is incorrect");

                for (int i = 6; i < 16; i++)
                {
                    Assert.AreEqual(i, queue.Peek());
                    Assert.AreEqual(i, queue.Dequeue());
                }

                Assert.AreEqual(0, queue.Count, "Queue.Count is incorrect");
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults EnqueueDequeueNullTest()
        {
            try
            {
                Queue queue = new Queue();

                for (int i = 0; i < 20; i++)
                {
                    queue.Enqueue(null);
                }

                Assert.AreEqual(20, queue.Count, "Queue.Count is incorrect");

                for (int i = 0; i < 20; i++)
                {
                    Assert.AreEqual(null, queue.Dequeue());
                }
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults ClearTest()
        {
            try
            {
                Queue queue = BuildNormalQueue();

                queue.Clear();

                Assert.AreEqual(0, queue.Count, "Queue.Count is incorrect");

                queue = BuildWrappedAroundQueue();

                queue.Clear();

                Assert.AreEqual(0, queue.Count, "Queue.Count is incorrect");
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults CloneTest()
        {
            try
            {
                Queue queue = BuildNormalQueue();
                Queue clone = (Queue)queue.Clone();

                Assert.AreEqual(6, queue.Count, "Queue.Count is incorrect");
                Assert.AreEqual(6, clone.Count, "Queue.Count is incorrect");

                for (int i = 4; i < 10; i++)
                {
                    Assert.AreEqual(i, queue.Dequeue());
                }

                Assert.AreEqual(0, queue.Count, "Queue.Count is incorrect");
                Assert.AreEqual(6, clone.Count, "Queue.Count is incorrect");

                for (int i = 4; i < 10; i++)
                {
                    Assert.AreEqual(i, clone.Dequeue());
                }


                queue = BuildWrappedAroundQueue();
                clone = (Queue)queue.Clone();

                Assert.AreEqual(6, queue.Count, "Queue.Count is incorrect");
                Assert.AreEqual(6, clone.Count, "Queue.Count is incorrect");

                for (int i = 4; i < 10; i++)
                {
                    Assert.AreEqual(i, queue.Dequeue());
                }

                Assert.AreEqual(0, queue.Count, "Queue.Count is incorrect");
                Assert.AreEqual(6, clone.Count, "Queue.Count is incorrect");

                for (int i = 4; i < 10; i++)
                {
                    Assert.AreEqual(i, clone.Dequeue());
                }
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults CopyToTest()
        {
            try
            {
                Queue queue = BuildNormalQueue();
                int[] intArray = new int[10];
                Object[] objArray = new Object[10];

                queue.CopyTo(intArray, 4);
                queue.CopyTo(objArray, 4);

                for (int i = 4; i < 10; i++)
                {
                    Assert.AreEqual(i, intArray[i]);
                    Assert.AreEqual(i, (int)objArray[i]);
                }

                queue = BuildWrappedAroundQueue();
                intArray = new int[10];
                objArray = new Object[10];

                queue.CopyTo(intArray, 4);
                queue.CopyTo(objArray, 4);

                for (int i = 4; i < 10; i++)
                {
                    Assert.AreEqual(i, intArray[i]);
                    Assert.AreEqual(i, (int)objArray[i]);
                }
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults EnumeratorTest()
        {
            try
            {
                Queue queue = BuildNormalQueue();

                int j = 4;

                foreach (int i in queue)
                {
                    Assert.AreEqual(j++, i);
                }

                queue = BuildWrappedAroundQueue();

                j = 4;

                foreach (int i in queue)
                {
                    Assert.AreEqual(j++, i);
                }
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults ContainsTest()
        {
            try
            {
                Queue queue = BuildNormalQueue();
                int i;

                for (i = 0; i < 4; i++)
                {
                    Assert.AreEqual(false, queue.Contains(i));
                }

                for (; i < 10; i++)
                {
                    Assert.AreEqual(true, queue.Contains(i));
                }

                queue = BuildWrappedAroundQueue();

                for (i = 0; i < 4; i++)
                {
                    Assert.AreEqual(false, queue.Contains(i));
                }

                for (; i < 10; i++)
                {
                    Assert.AreEqual(true, queue.Contains(i));
                }

                Assert.AreEqual(false, queue.Contains(null));

                queue.Enqueue(null);

                Assert.AreEqual(true, queue.Contains(null));
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults ToArrayTest()
        {
            try
            {
                Object[] objArray = BuildNormalQueue().ToArray();

                int i = 4;
                foreach(Object o in objArray)
                {
                    Assert.AreEqual(i++, (int)o);
                }

                objArray = BuildWrappedAroundQueue().ToArray();
                i = 4;
                foreach (Object o in objArray)
                {
                    Assert.AreEqual(i++, (int)o);
                }
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        private Queue BuildNormalQueue()
        {
            Queue queue = new Queue();

            for (int i = 2; i < 10; i++)
            {
                queue.Enqueue(i);
            }

            queue.Dequeue();
            queue.Dequeue();

            return queue;
        }

        private Queue BuildWrappedAroundQueue()
        {
            Queue queue = new Queue();

            for (int i = 0; i < 8; i++)
            {
                queue.Enqueue(i);
            }

            for (int i = 0; i < 4; i++)
            {
                queue.Dequeue();
            }

            for (int i = 8; i < 10; i++)
            {
                queue.Enqueue(i);
            }

            return queue;
        }
    }
}
    

