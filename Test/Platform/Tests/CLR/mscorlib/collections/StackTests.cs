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
    public class StackTests : IMFTestInterface
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

        [TestMethod]
        public MFTestResults PushPopPeekTest1()
        {
            try
            {
                Stack stack = new Stack();

                stack.Push(TestObjects.u1);
                stack.Push(TestObjects.s1);
                stack.Push(TestObjects.u2);
                stack.Push(TestObjects.s2);
                stack.Push(TestObjects.u4);
                stack.Push(TestObjects.s4);
                stack.Push(TestObjects.u8);
                stack.Push(TestObjects.s8);
                stack.Push(TestObjects.f4);
                stack.Push(TestObjects.f8);
                stack.Push(TestObjects.c2);
                stack.Push(TestObjects.str);
                stack.Push(TestObjects.dt);
                stack.Push(TestObjects.ts);
                stack.Push(TestObjects.st);
                stack.Push(TestObjects.cl);
                stack.Push(TestObjects.o);
                stack.Push(TestObjects.nul);
                stack.Push(TestObjects.en);

                Assert.AreEqual(19, stack.Count, "Stack.Count is incorrect");

                Assert.AreEqual(TestObjects.en, stack.Peek());
                Assert.AreEqual(TestObjects.en, stack.Pop());
                Assert.AreEqual(TestObjects.nul, stack.Peek());
                Assert.AreEqual(TestObjects.nul, stack.Pop());
                Assert.AreEqual(TestObjects.o, stack.Peek());
                Assert.AreEqual(TestObjects.o, stack.Pop());
                Assert.AreEqual(TestObjects.cl, stack.Peek());
                Assert.AreEqual(TestObjects.cl, stack.Pop());
                Assert.AreEqual(TestObjects.st, stack.Peek());
                Assert.AreEqual(TestObjects.st, stack.Pop());
                Assert.AreEqual(TestObjects.ts, stack.Peek());
                Assert.AreEqual(TestObjects.ts, stack.Pop());
                //Assert.AreEqual(TestObjects.dt, stack.Peek()); // Throws CLR_E_WRONG_TYPE exception known bug# 18505
                /*Assert.AreEqual(TestObjects.dt,*/stack.Pop()/*)*/; // Throws CLR_E_WRONG_TYPE exception known bug# 18505
                Assert.AreEqual(TestObjects.str, stack.Peek());
                Assert.AreEqual(TestObjects.str, stack.Pop());
                Assert.AreEqual(TestObjects.c2, stack.Peek());
                Assert.AreEqual(TestObjects.c2, stack.Pop());
                Assert.AreEqual(TestObjects.f8, stack.Peek());
                Assert.AreEqual(TestObjects.f8, stack.Pop());
                Assert.AreEqual(TestObjects.f4, stack.Peek());
                Assert.AreEqual(TestObjects.f4, stack.Pop());
                Assert.AreEqual(TestObjects.s8, stack.Peek());
                Assert.AreEqual(TestObjects.s8, stack.Pop());
                Assert.AreEqual(TestObjects.u8, stack.Peek());
                Assert.AreEqual(TestObjects.u8, stack.Pop());
                Assert.AreEqual(TestObjects.s4, stack.Peek());
                Assert.AreEqual(TestObjects.s4, stack.Pop());
                Assert.AreEqual(TestObjects.u4, stack.Peek());
                Assert.AreEqual(TestObjects.u4, stack.Pop());
                Assert.AreEqual(TestObjects.s2, stack.Peek());
                Assert.AreEqual(TestObjects.s2, stack.Pop());
                Assert.AreEqual(TestObjects.u2, stack.Peek());
                Assert.AreEqual(TestObjects.u2, stack.Pop());
                Assert.AreEqual(TestObjects.s1, stack.Peek());
                Assert.AreEqual(TestObjects.s1, stack.Pop());
                Assert.AreEqual(TestObjects.u1, stack.Peek());
                Assert.AreEqual(TestObjects.u1, stack.Pop());

                Assert.AreEqual(0, stack.Count, "Stack.Count is incorrect");
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults PushPopPeekTest2()
        {
            try
            {
                Stack stack = new Stack();

                for (int i = 0; i < 8; i++)
                {
                    stack.Push(i);
                }

                // in stack [ 7, 6, 5, 4, 3, 2, 1, 0 ]

                Assert.AreEqual(8, stack.Count, "Stack.Count is incorrect");

                for (int i = 7; i >= 4; i--)
                {
                    Assert.AreEqual(i, stack.Peek());
                    Assert.AreEqual(i, stack.Pop());
                }

                // in stack [ 3, 2, 1, 0 ]

                Assert.AreEqual(4, stack.Count, "Stack.Count is incorrect");

                for (int i = 8; i < 12; i++)
                {
                    stack.Push(i);
                }

                // in stack [ 11, 10, 9, 8, 3, 2, 1, 0 ]

                Assert.AreEqual(8, stack.Count, "Stack.Count is incorrect");

                for (int i = 11; i >= 10; i--)
                {
                    Assert.AreEqual(i, stack.Peek());
                    Assert.AreEqual(i, stack.Pop());
                }

                // in stack [ 9, 8, 3, 2, 1, 0 ]

                Assert.AreEqual(6, stack.Count, "Stack.Count is incorrect");

                for (int i = 12; i < 16; i++)
                {
                    stack.Push(i);
                }

                // in stack [ 15, 14, 13, 12, 9, 8, 3, 2, 1, 0 ]

                Assert.AreEqual(10, stack.Count, "Stack.Count is incorrect");

                for (int i = 15; i >= 12; i--)
                {
                    Assert.AreEqual(i, stack.Peek());
                    Assert.AreEqual(i, stack.Pop());
                }
                for (int i = 9; i >= 8; i--)
                {
                    Assert.AreEqual(i, stack.Peek());
                    Assert.AreEqual(i, stack.Pop());
                }
                for (int i = 3; i >= 0; i--)
                {
                    Assert.AreEqual(i, stack.Peek());
                    Assert.AreEqual(i, stack.Pop());
                }

                Assert.AreEqual(0, stack.Count, "Stack.Count is incorrect");
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults PushPopNullTest()
        {
            try
            {
                Stack stack = new Stack();

                for (int i = 0; i < 20; i++)
                {
                    stack.Push(null);
                }

                Assert.AreEqual(20, stack.Count, "Stack.Count is incorrect");

                for (int i = 0; i < 20; i++)
                {
                    Assert.AreEqual(null, stack.Pop());
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
                Stack stack = BuildStack();

                stack.Clear();

                Assert.AreEqual(0, stack.Count, "Stack.Count is incorrect");
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        private Stack BuildStack()
        {
            Stack stack = new Stack();

            for (int i = 0; i < 10; i++)
            {
                stack.Push(i);
            }

            return stack;
        }

        [TestMethod]
        public MFTestResults CloneTest()
        {
            try
            {
                Stack stack = BuildStack();
                Stack clone = (Stack)stack.Clone();

                Assert.AreEqual(10, stack.Count, "Stack.Count is incorrect");
                Assert.AreEqual(10, clone.Count, "Stack.Count is incorrect");

                for (int i = 9; i >= 0; i--)
                {
                    Assert.AreEqual(i, stack.Pop());
                }

                Assert.AreEqual(0, stack.Count, "Stack.Count is incorrect");
                Assert.AreEqual(10, clone.Count, "Stack.Count is incorrect");

                for (int i = 9; i >= 0; i--)
                {
                    Assert.AreEqual(i, clone.Pop());
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
                Stack stack = BuildStack();
                int[] intArray = new int[10];
                Object[] objArray = new Object[10];

                stack.CopyTo(intArray, 0);
                stack.CopyTo(objArray, 0);

                for (int i = 0; i < 10; i++)
                {
                    Assert.AreEqual(9 - i, intArray[i]);
                    Assert.AreEqual(9 - i, (int)objArray[i]);
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
                Stack stack = BuildStack();

                int j = 9;

                foreach (int i in stack)
                {
                    Assert.AreEqual(j--, i);
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
                Stack stack = BuildStack();
                int i;

                for (i = 0; i < 10; i++)
                {
                    Assert.AreEqual(true, stack.Contains(i));
                }

                for (; i < 20; i++)
                {
                    Assert.AreEqual(false, stack.Contains(i));
                }

                Assert.AreEqual(false, stack.Contains(null));

                stack.Push(null);

                Assert.AreEqual(true, stack.Contains(null));
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
                Object[] objArray = BuildStack().ToArray();

                int i = 9;
                foreach (Object o in objArray)
                {
                    Assert.AreEqual(i--, (int)o);
                }
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }
    }
}
