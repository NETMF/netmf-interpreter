using System;
using System.Text;
using Microsoft.SPOT;

namespace Microsoft.SPOT.Platform.Tests
{
    class TestFailException : Exception
    {
        static string GenerateArgList(object[] args)
        {
            StringBuilder builder = new StringBuilder();

            foreach (object arg in args)
            {
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(arg.ToString());
            }

            return builder.ToString();
        }

        static string GenerateMessage(string sMethod, object ret, object expected, object[] args)
        {
            return sMethod + "(" + GenerateArgList(args) + ") returns " + ret.ToString() + " rather than " + expected.ToString();
        }

        static string GenerateMessage(string sMethod, Exception exception, Type expectedExceptionType, object[] args)
        {
            if (exception == null)
            {
                return sMethod + "(" + GenerateArgList(args) + ") throw no exception rather than " + expectedExceptionType.Name;
            }
            else
            {
                return sMethod + "(" + GenerateArgList(args) + ") throw exception " + exception.GetType().Name + " rather than " + expectedExceptionType.Name;
            }
        }

        public TestFailException(string sMethod, object ret, object expected, params object[] args) :
            base(GenerateMessage(sMethod, ret, expected, args))
        {
        }

        public TestFailException(string sMethod, Exception exception, Type expectedExceptionType, params object[] args) :
            base(GenerateMessage(sMethod, exception, expectedExceptionType, args))
        {
        }
    }
}
