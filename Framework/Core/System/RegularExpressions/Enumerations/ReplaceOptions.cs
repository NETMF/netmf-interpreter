using System;
using Microsoft.SPOT;

namespace System.Text.RegularExpressions
{
    [Flags]
    public enum ReplaceOptions
    {
        /**
         * Flag bit that indicates that subst should replace all occurrences of this
         * regular expression.
         */
        ReplaceAll = 0x0000,

        /**
         * Flag bit that indicates that subst should only replace the first occurrence
         * of this regular expression.
         */
        ReplaceFirst = 0x0001,

        /**
         * Flag bit that indicates that subst should replace backreferences
         */
        ReplaceBackrefrences = 0x0002
    }
}
