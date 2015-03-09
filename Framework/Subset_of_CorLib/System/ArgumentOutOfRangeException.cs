////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{

    using System;
    // The ArgumentOutOfRangeException is thrown when an argument
    // is outside the legal range for that argument.  This may often be caused
    // by
    //
    [Serializable()]
    public class ArgumentOutOfRangeException : ArgumentException
    {

        // Creates a new ArgumentOutOfRangeException with its message
        // string set to a default message explaining an argument was out of range.
        public ArgumentOutOfRangeException()
            : this(null)
        {
        }

        public ArgumentOutOfRangeException(String paramName)
            : base(null, paramName)
        {
        }

        public ArgumentOutOfRangeException(String paramName, String message)
            : base(message, paramName)
        {
        }

        // We will not use this in the classlibs, but we'll provide it for
        // anyone that's really interested so they don't have to stick a bunch
        // of printf's in their code.

        // Gets the value of the argument that caused the exception.
        // Note - we don't set this anywhere in the class libraries in
        // version 1, but it might come in handy for other developers who
        // want to avoid sticking printf's in their code.

    }
}


