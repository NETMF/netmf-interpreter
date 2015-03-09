using System;

namespace System.Text.RegularExpressions
{
    /// <summary>
    /// The SpecialEscape bit masks and range values
    /// </summary>
    internal enum SpecialEscape
    {
        Mask = 0xffff0,         // Escape complexity mask
        BackReference = 0xfffff, // Escape is really a backreference
        Complex = 0xffffe,      // Escape isn't really a true character
        CharacterClass = 0xffffd,        // Escape represents a whole class of characters
    }
}
