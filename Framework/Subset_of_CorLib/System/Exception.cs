////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Reflection;

    [Serializable()]
    public class Exception
    {
        private string _message;
        private Exception m_innerException;
        private object m_stackTrace;
        protected int m_HResult;

        public Exception()
        {
        }

        public Exception(String message)
        {
            _message = message;
        }

        public Exception(String message, Exception innerException)
        {
            _message = message;
            m_innerException = innerException;
        }

        public virtual String Message
        {
            get
            {
                if (_message == null)
                {
                    return "Exception was thrown: " + this.GetType().FullName;
                }
                else
                {
                    return _message;
                }
            }
        }

        public Exception InnerException
        {
            get { return m_innerException; }
        }

        public extern virtual String StackTrace
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public override String ToString()
        {
            String message = Message;
            String s = base.ToString();

            if (message != null && message.Length > 0)
            {
                s += ": " + message;
            }

            return s;
        }

    }

}


