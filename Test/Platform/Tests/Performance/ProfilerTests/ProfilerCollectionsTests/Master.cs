////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;

//--//

namespace Microsoft.SPOT.Platform.Tests
{
    public class Master_ProfilerColectionsTests
    {
        static readonly int c_Iterations = 200;
        static readonly int c_In = 128;
        static readonly int c_Out = c_In - 1;

        //--//

        Stack stack = new Stack();
        Queue queue = new Queue();
        ArrayList arrayList = new ArrayList();

        object obj = new object();


        //--//

        public static void Main()
        {
            Master_ProfilerColectionsTests tests = new Master_ProfilerColectionsTests();

            tests.TestStack();
            tests.TestQueue();
            tests.TestArrayList();

        }

        public void TestStack()
        {
            object o1;
            int i, j;

            for (i = 0; i < c_Iterations; ++i)
            {
                for (j = 0; j < c_In; ++j)
                {
                    stack.Push(obj);
                }

                for (j = 0; j < c_Out; ++j)
                {
                    o1 = stack.Pop();
                }
            }

            stack.Push(obj);
            stack.Push(obj);
        }

        public void TestQueue()
        {
            object o1;
            int i, j;

            for (i = 0; i < c_Iterations; ++i)
            {
                for (j = 0; j < c_In; ++j) 
                {
                    this.queue.Enqueue(obj);
                }

                for (j = 0; j < c_Out; ++j)
                {
                    o1 = this.queue.Dequeue();
                }
            }

            this.queue.Enqueue(obj);
            this.queue.Enqueue(obj);
        }

        public void TestArrayList()
        {
            int i, j;

            for (i = 0; i < c_Iterations; ++i)
            {
                for (j = 0; j < c_In; ++j)
                {
                    if (j % 3 == 0)
                    {
                        this.arrayList.Insert(0, obj);
                    }
                    else if (j % 3 == 1)
                    {
                        this.arrayList.Insert(this.arrayList.Count / 2, obj);
                    }
                    else
                    {
                        this.arrayList.Insert(this.arrayList.Count - 1, obj);
                    }
                }

                for (j = 0; j < c_Out; ++j)
                {
                    if (j % 3 == 0)
                    {
                        this.arrayList.RemoveAt(0);
                    }
                    else if (j % 3 == 1)
                    {
                        this.arrayList.RemoveAt(this.arrayList.Count / 2);
                    }
                    else
                    {
                        this.arrayList.RemoveAt(this.arrayList.Count - 1);
                    }
                }
            }

            this.arrayList.Insert(0, obj);
            this.arrayList.Insert(0, obj);
        }
    }
}