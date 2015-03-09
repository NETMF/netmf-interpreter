using System;

namespace System.Text.RegularExpressions
{
    /// <summary>
    /// Options which can be applied to a RegularExpression
    /// </summary>
    [Flags]    
    public enum RegexOptions
    {
        Compiled = 8,
        CultureInvariant = 0x200,
        ECMAScript = 0x100,
        ExplicitCapture = 4,
        IgnoreCase = 1,
        IgnorePatternWhitespace = 0x20,
        Multiline = 2,
        None = 0,        
        Singleline = 0x10,
        Timed = 0x400
    }
}
