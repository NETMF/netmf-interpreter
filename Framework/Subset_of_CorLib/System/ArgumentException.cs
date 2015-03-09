////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{

    using System;
    // The ArgumentException is thrown when an argument does not meet
    // the contract of the method.  Ideally it should give a meaningful error
    // message describing what was wrong and which parameter is incorrect.
    //
    [Serializable()]
    public class ArgumentException : SystemException
    {
        private String m_paramName;

        // Creates a new ArgumentException with its message
        // string set to the empty string.
        public ArgumentException()
            : base()
        {
        }

        // Creates a new ArgumentException with its message
        // string set to message.
        //
        public ArgumentException(String message)
            : base(message)
        {
        }

        public ArgumentException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ArgumentException(String message, String paramName, Exception innerException)
            : base(message, innerException)
        {
            m_paramName = paramName;
        }

        public ArgumentException(String message, String paramName)

            : base(message)
        {
            m_paramName = paramName;
        }

        public override String Message
        {
            get
            {
                String s = base.Message;
                if (!((m_paramName == null) ||
                       (m_paramName.Length == 0)))
                    return s + "\n" + "Invalid argument " + "'" + m_paramName + "'";
                else
                    return s;
            }
        }

        public virtual String ParamName
        {
            get { return m_paramName; }
        }

    }
}


