using System;
using System.Collections;
using Microsoft.SPOT;

namespace MFDpwsTestFixtureUtilities
{
    public class Stack
    {
        private StackElement top = null;
        private StackElement bottom = null;
        private int count = 0;

        private System.Threading.ManualResetEvent mre = new System.Threading.ManualResetEvent(false);

        public void Clear()
        {
            top = null;
            bottom = null;
            count = 0;
        }

        public void Push(object o)
        {
            var elem = new StackElement(o);

            if (bottom == null)
            {
                bottom = elem;
                top = elem;
            }
            else
            {
                bottom.Next = elem;
                bottom = elem;
            }
            count++;
        }

        public object Pop()
        {
            if (top == null) return null;

            var elem = top;

            top = elem.Next;

            if (top == null)
            {
                bottom = null;
            }
            count--;
            return elem.Item;
        }

        public int Count
        {
            get { return count; }
        }

        public void Wait()
        {
            mre.WaitOne();
        }

        public void Set()
        {
            mre.Set();
        }

        public void Reset()
        {
            mre.Reset();
        }

        class StackElement
        {
            private object item;
            private StackElement next = null;

            public StackElement(object i)
            {
                item = i;
            }

            public object Item
            {
                get { return item; }
            }

            public StackElement Next
            {
                get { return next; }
                set { next = value; }
            }
        }
    }
}
