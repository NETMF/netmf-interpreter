////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using System.Collections;

namespace System.IO
{

    [Serializable]
    public abstract class TextWriter : MarshalByRefObject, IDisposable
    {
        private const String InitialNewLine = "\r\n";

        //--//

        protected char[] CoreNewLine = new char[] { '\r', '\n' };

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

        public virtual void Flush()
        {
        }

        public abstract Encoding Encoding
        {
            get;
        }

        public virtual String NewLine
        {
            get { return new String(CoreNewLine); }
            set
            {
                if (value == null)
                    value = InitialNewLine;
                CoreNewLine = value.ToCharArray();
            }
        }

        public virtual void Write(char value)
        {
        }

        public virtual void Write(char[] buffer)
        {
            if (buffer != null) Write(buffer, 0, buffer.Length);
        }

        public virtual void Write(char[] buffer, int index, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException();
            if (index < 0)
                throw new ArgumentOutOfRangeException();
            if (count < 0)
                throw new ArgumentOutOfRangeException();
            if (buffer.Length - index < count)
                throw new ArgumentException();

            for (int i = 0; i < count; i++) Write(buffer[index + i]);
        }

        public virtual void Write(bool value)
        {
            Write(value);
        }

        public virtual void Write(int value)
        {
            Write(value.ToString());
        }

        public virtual void Write(uint value)
        {
            Write(value.ToString());
        }

        public virtual void Write(long value)
        {
            Write(value.ToString());
        }

        public virtual void Write(ulong value)
        {
            Write(value.ToString());
        }

        public virtual void Write(float value)
        {
            Write(value.ToString());
        }

        public virtual void Write(double value)
        {
            Write(value.ToString());
        }

        public virtual void Write(String value)
        {
            if (value != null) Write(value.ToCharArray());
        }

        public virtual void Write(Object value)
        {
            if (value != null)
            {
                Write(value.ToString());
            }
        }

        public virtual void WriteLine()
        {
            Write(CoreNewLine);
        }

        public virtual void WriteLine(char value)
        {
            Write(value);
            WriteLine();
        }

        public virtual void WriteLine(char[] buffer)
        {
            Write(buffer);
            WriteLine();
        }

        public virtual void WriteLine(char[] buffer, int index, int count)
        {
            Write(buffer, index, count);
            WriteLine();
        }

        public virtual void WriteLine(bool value)
        {
            Write(value);
            WriteLine();
        }

        public virtual void WriteLine(int value)
        {
            Write(value);
            WriteLine();
        }

        public virtual void WriteLine(uint value)
        {
            Write(value);
            WriteLine();
        }

        public virtual void WriteLine(long value)
        {
            Write(value);
            WriteLine();
        }

        public virtual void WriteLine(ulong value)
        {
            Write(value);
            WriteLine();
        }

        public virtual void WriteLine(float value)
        {
            Write(value);
            WriteLine();
        }

        public virtual void WriteLine(double value)
        {
            Write(value);
            WriteLine();
        }

        public virtual void WriteLine(String value)
        {
            Write(value);
            WriteLine();
        }

        public virtual void WriteLine(Object value)
        {
            if (value == null)
            {
                WriteLine();
            }
            else
            {
                WriteLine(value.ToString());
            }
        }
    }
}


