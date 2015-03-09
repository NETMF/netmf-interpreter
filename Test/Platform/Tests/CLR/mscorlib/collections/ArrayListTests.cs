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
    public class ArrayListTests : IMFTestInterface
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
        public MFTestResults AddAndGetSetItemTest()
        {
            try
            {
                ArrayList list = new ArrayList();

                list.Add(TestObjects.u1);
                list.Add(TestObjects.s1);
                list.Add(TestObjects.u2);
                list.Add(TestObjects.s2);
                list.Add(TestObjects.u4);
                list.Add(TestObjects.s4);
                list.Add(TestObjects.u8);
                list.Add(TestObjects.s8);
                list.Add(TestObjects.f4);
                list.Add(TestObjects.f8);
                list.Add(TestObjects.c2);
                list.Add(TestObjects.str);
                list.Add(TestObjects.dt);
                list.Add(TestObjects.ts);
                list.Add(TestObjects.st);
                list.Add(TestObjects.cl);
                list.Add(TestObjects.o);
                list.Add(TestObjects.nul);
                list.Add(TestObjects.en);

                Assert.AreEqual(TestObjects.u1, list[0]);
                Assert.AreEqual(TestObjects.s1, list[1]);
                Assert.AreEqual(TestObjects.u2, list[2]);
                Assert.AreEqual(TestObjects.s2, list[3]);
                Assert.AreEqual(TestObjects.u4, list[4]);
                Assert.AreEqual(TestObjects.s4, list[5]);
                Assert.AreEqual(TestObjects.u8, list[6]);
                Assert.AreEqual(TestObjects.s8, list[7]);
                Assert.AreEqual(TestObjects.f4, list[8]);
                Assert.AreEqual(TestObjects.f8, list[9]);
                Assert.AreEqual(TestObjects.c2, list[10]);
                Assert.AreEqual(TestObjects.str, list[11]);
                //Assert.AreEqual(TestObjects.dt, list[12]); // Throws CLR_E_WRONG_TYPE exception known bug# 18505
                Assert.AreEqual(TestObjects.ts, list[13]);
                Assert.AreEqual(TestObjects.st, list[14]);
                Assert.AreEqual(TestObjects.cl, list[15]);
                Assert.AreEqual(TestObjects.o, list[16]);
                Assert.AreEqual(TestObjects.nul, list[17]);
                Assert.AreEqual(TestObjects.en, list[18]);

                Assert.AreEqual(19, list.Count, "ArrayList.Count is incorrect");

                list[0] = list[18];
                list[1] = list[17];
                list[2] = list[16];
                list[3] = list[15];
                list[4] = list[14];
                list[5] = list[13];
                list[6] = list[12];
                list[7] = list[11];
                list[8] = list[10];
                list[9] = TestObjects.f8;
                list[10] = TestObjects.f4;
                list[11] = TestObjects.s8;
                list[12] = TestObjects.u8;
                list[13] = TestObjects.s4;
                list[14] = TestObjects.u4;
                list[15] = TestObjects.s2;
                list[16] = TestObjects.u2;
                list[17] = TestObjects.s1;
                list[18] = TestObjects.u1;

                Assert.AreEqual(TestObjects.u1, list[18]);
                Assert.AreEqual(TestObjects.s1, list[17]);
                Assert.AreEqual(TestObjects.u2, list[16]);
                Assert.AreEqual(TestObjects.s2, list[15]);
                Assert.AreEqual(TestObjects.u4, list[14]);
                Assert.AreEqual(TestObjects.s4, list[13]);
                Assert.AreEqual(TestObjects.u8, list[12]);
                Assert.AreEqual(TestObjects.s8, list[11]);
                Assert.AreEqual(TestObjects.f4, list[10]);
                Assert.AreEqual(TestObjects.f8, list[9]);
                Assert.AreEqual(TestObjects.c2, list[8]);
                Assert.AreEqual(TestObjects.str, list[7]);
                //Assert.AreEqual(TestObjects.dt, list[6]); // Throws CLR_E_WRONG_TYPE exception known bug# 18505
                Assert.AreEqual(TestObjects.ts, list[5]);
                Assert.AreEqual(TestObjects.st, list[4]);
                Assert.AreEqual(TestObjects.cl, list[3]);
                Assert.AreEqual(TestObjects.o, list[2]);
                Assert.AreEqual(TestObjects.nul, list[1]);
                Assert.AreEqual(TestObjects.en, list[0]);

                Assert.AreEqual(19, list.Count, "ArrayList.Count is incorrect");
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults AddNullTest()
        {
            try
            {
                ArrayList list = new ArrayList();

                for (int i = 0; i < 20; i++)
                {
                    list.Add(null);
                }

                Assert.AreEqual(20, list.Count, "ArrayList.Count is incorrect");

                for (int i = 0; i < 20; i++)
                {
                    Assert.AreEqual(null, list[i]);
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
                ArrayList list = new ArrayList();

                for (int i = 0; i < 20; i++)
                {
                    list.Add(i.ToString());
                }

                Assert.AreEqual(20, list.Count, "ArrayList.Count is incorrect");

                list.Clear();

                Assert.AreEqual(0, list.Count, "ArrayList.Count is incorrect");
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults InsertTest()
        {
            try
            {
                ArrayList list = new ArrayList();

                for (int i = 1; i <= 10; i++)
                {
                    if (i == 5) continue;
                    list.Add(i);
                }

                list.Insert(0, 0);
                list.Insert(10, 11);
                list.Insert(5, 5);

                Assert.AreEqual(12, list.Count, "ArrayList.Count is incorrect");

                for (int j = 0; j <= 11; j++)
                {
                    Assert.AreEqual(j, list[j]);
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
        public MFTestResults RemoveAtTest1()
        {
            try
            {
                ArrayList list = new ArrayList();

                for (int i = 0; i < 20; i++)
                {
                    list.Add(i);
                }

                list.RemoveAt(19);
                list.RemoveAt(0);
                list.RemoveAt(4);

                Assert.AreEqual(17, list.Count, "ArrayList.Count is incorrect");

                int k = 0;
                for (int j = 0; j < 20; j++)
                {
                    if (j == 0 || j == 5 || j == 19) continue;

                    Assert.AreEqual(j, list[k++]);
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
        public MFTestResults RemoveAtTest2()
        {
            try
            {
                ArrayList list = CreateFullList();

                int count = list.Count;

                for (int i = 0; i < count; i++)
                {
                    list.RemoveAt(0);
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
        public MFTestResults SetCapacityTest()
        {
            try
            {
                ArrayList list = new ArrayList();

                for (int i = 0; i < 20; i++)
                {
                    list.Add(i);
                }

                for (int i = 0; i < 15; i++)
                {
                    list.RemoveAt(list.Count - 1);
                }

                Assert.AreEqual(5, list.Count, "ArrayList.Count is incorrect");

                list.Capacity = 5;

                for (int i = 0; i < 5; i++)
                {
                    Assert.AreEqual(i, list[i]);
                }

                list.Capacity = 20;

                for (int i = 0; i < 5; i++)
                {
                    Assert.AreEqual(i, list[i]);
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
                ArrayList list = new ArrayList();

                for (int i = 0; i < 20; i++)
                {
                    list.Add(i);
                }

                int j = 0;
                foreach (int i in list)
                {
                    Assert.AreEqual(j++, i);
                }

                list = new ArrayList();

                for (int i = 0; i < 20; i++)
                {
                    list.Add(i.ToString());
                }

                j = 0;
                foreach (String s in list)
                {
                    Assert.AreEqual(j.ToString(), s);
                    j++;
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
        public MFTestResults IListTest()
        {
            try
            {
                ArrayList list = new ArrayList();

                for (int i = 0; i < 20; i++)
                {
                    list.Add(i);
                }

                IList ilist = (IList)list;

                for (int i = 0; i < 20; i++)
                {
                    Assert.Equals(i, ilist[i]);
                }

                list = new ArrayList();

                for (int i = 0; i < 20; i++)
                {
                    list.Add(i.ToString());
                }

                ilist = (IList)list;

                for (int i = 0; i < 20; i++)
                {
                    Assert.Equals(i.ToString(), ilist[i]);
                }
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        /// <summary>
        /// Creates an array list that's at capacity
        /// </summary>
        /// <returns>an ArrayList that is at capacity</returns>
        private ArrayList CreateFullList()
        {
            ArrayList list = new ArrayList();
            int capacity = list.Capacity;

            for (int i = 0; i < capacity; i++)
            {
                list.Add(i);
            }

            Assert.AreEqual(list.Capacity, list.Count);

            return list;
        }
    }
}
    

