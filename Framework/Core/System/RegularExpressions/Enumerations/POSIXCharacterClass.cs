using System;

namespace System.Text.RegularExpressions
{
    /// <summary>
    /// Character Classes for POSIX Characters
    /// </summary>
    internal sealed class POSIXCharacterClass
    {
        public const char Alphanumeric = 'w';  // Alphanumerics
        public const char Alphabetica = 'a';  // Alphanumerics
        public const char Blank = 'b';  // Alphanumerics
        public const char Control = 'c';  // Control characters
        public const char Digit = 'd';  // Digits
        public const char GraphicCharacter = 'g';  // Graphic characters
        public const char LowerCase = 'l';  // Lowercase characters
        public const char Printable = 'p';  // Printable characters
        public const char Punctuation = '!';  // Punctuation
        public const char Spaces = 's';  // Spaces
        public const char UpperCase = 'u';  // Uppercase characters
        public const char Hexadecimal = 'x';  // Hexadecimal digits
        public const char JavaIdentifierStart = 'j';  // Java identifier start
        public const char JavaIdentifierPart = 'k';  // Java identifier part
    }
}
