////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Text;
using System.Collections;

namespace System.IO
{
    [Serializable()]
    public abstract class TextReader : MarshalByRefObject, IDisposable
    {
        protected TextReader() { }

        public virtual void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public virtual int Peek()
        {
            return -1;
        }

        public virtual int Read()
        {
            return -1;
        }

        public virtual int Read(char[] buffer, int index, int count)
        {
            return -1;
        }

        public virtual int ReadBlock(char[] buffer, int index, int count)
        {
            int i, n = 0;
            do
            {
                n += (i = Read(buffer, index + n, count - n));
            } while (i > 0 && n < count);
            return n;
        }

        public virtual String ReadToEnd()
        {
            return null;
        }

        public virtual String ReadLine()
        {
            return null;
        }

    }
}


