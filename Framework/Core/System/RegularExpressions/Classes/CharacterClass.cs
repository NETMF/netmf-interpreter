/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * Ported to C# for the .Net Micro Framework by <a href="mailto:juliusfriedman@gmail.com">Julius Friedman</a>
 * http://netmf.codeplex.com/
 * 
 * Engineered utilizing plumbing from the <a href="http://jakarta.apache.org/regexp/">Jakarta Regexp Project</a>
 * to be compatible with the conventions from the Full .Net Framework by <a href="mailto:juliusfriedman@gmail.com">Julius Friedman</a>
 * 
*/
namespace System.Text.RegularExpressions
{
    static class CharacterClass
    {

        internal enum CharacterClassification
        {
            Unassigned,
            UppercaseLetter,
            LowercaseLetter,
            TitlecaseLetter,
            ModifierLetter,
            OtherLetter,
            NonSpacingMark,
            EnclosingMark,
            CombiningSpacingMark,
            DecimalDigitNumber,
            LetterNumber,
            OtherNumber,
            SpaceSeperator,
            LineSeperator,
            ParagraphSeperator,
            Control,
            Format,
            PrivateUse,
            Surrogate,
            DashPunctuation,
            StartPunctuation,
            EndPunctuation,
            ConnectorPunctuation,
            OtherPunctuation,
            MathSymbol,
            CurrencySymbol,
            ModifierSymbol,
            OtherSymbol
        }

        internal static bool IsEmpty(ref string charClass)
        {
            return (((charClass[2] == '\0') && (charClass[0] == '\0')) && ((charClass[1] == '\0') && !IsSubtraction(ref charClass)));
        }

        internal static bool IsMergeable(ref string charClass)
        {
            return (!IsNegated(ref charClass) && !IsSubtraction(ref charClass));
        }

        internal static bool IsNegated(ref string set)
        {
            return ((set != null) && (set[0] == '\x0001'));
        }

        internal static bool IsSingleton(ref string set)
        {
            if ((((set[0] != '\0') || (set[2] != '\0')) || ((set[1] != '\x0002') || IsSubtraction(ref set))) || ((set[3] != 0xffff) && ((set[3] + '\x0001') != set[4])))
            {
                return false;
            }
            return true;
        }

        internal static bool IsSingletonInverse(ref string set)
        {
            if ((((set[0] != '\x0001') || (set[2] != '\0')) || ((set[1] != '\x0002') || IsSubtraction(ref set))) || ((set[3] != 0xffff) && ((set[3] + '\x0001') != set[4])))
            {
                return false;
            }
            return true;
        }

        internal static bool IsSubtraction(ref string charClass)
        {
            return (charClass.Length > (('\x0003' + charClass[1]) + charClass[2]));
        }

        internal static char SingletonChar(ref string set)
        {
            return set[3];
        }

        static internal byte[] CharacterCategories = new byte[] { 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 0, 2, 2, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            2, 0, 0, 3, 4, 0, 0, 0, 4, 4, 5, 5, 0, 0, 4, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 4, 0, 4, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 4, 0, 0, 0
        };

        internal const char DefaultChar = default(char);

        internal static bool IsMetachar(ref char ch)
        {
            return ((ch <= '|') && (CharacterCategories[ch] >= 1));
        }

        internal static bool IsWhitespace(ref char c)
        {
            CharacterClassification charClass = (CharacterClassification)GetCharacterType(ref c);
            return ((charClass == CharacterClassification.SpaceSeperator || charClass == CharacterClassification.LineSeperator || charClass == CharacterClassification.ParagraphSeperator)
            && !(c == 0x00A0 || c == 0x2007 || c == 0x202F)) || c == 0x0009 || c == 0x000A || c == 0x000B ||
            c == 0x000C || c == 0x000D || c == 0x0009 || c == 0x001C || c == 0x001D || c == 0x001E || c == 0x001F;
        }

        internal static bool IsDigit(ref char c)
        {
            CharacterClassification charClass = (CharacterClassification)GetCharacterType(ref c);
            return charClass == CharacterClassification.DecimalDigitNumber;
        }

        internal static bool IsDigit(ref string str, ref int charIndex)
        {
            char c = str[charIndex];
            return IsDigit(ref c);
        }

        internal static bool IsLetter(ref char c)
        {
            CharacterClassification charClass = (CharacterClassification)GetCharacterType(ref c);
            return charClass >= CharacterClassification.UppercaseLetter && charClass <= CharacterClassification.OtherLetter;
            //return charClass == CharacterClass.LowercaseLetter || charClass == CharacterClass.UppercaseLetter || charClass == CharacterClass.TitlecaseLetter || charClass == CharacterClass.OtherLetter;
        }

        internal static bool IsLetterOrDigit(ref char c)
        {
            return CharacterClass.IsDigit(ref c) || CharacterClass.IsLetter(ref c);
        }

        internal static bool IsSpaceChar(ref char c)
        {
            CharacterClassification charClass = (CharacterClassification)GetCharacterType(ref c);
            return charClass >= CharacterClassification.SpaceSeperator || charClass <= CharacterClassification.ParagraphSeperator;
            //return charClass == CharacterClassification.SpaceSeperator || charClass == CharacterClassification.LineSeperator || charClass == CharacterClassification.ParagraphSeperator;            
        }

        internal const char Hyphen = '-';
        internal const char Underscore = '_';

        internal static bool IsJavaIdentifierStart(ref char c)
        {
            CharacterClassification charClass = (CharacterClassification)GetCharacterType(ref c);
            return IsLetter(ref c) || charClass == CharacterClassification.LetterNumber || c == OpCode.EndOfLine || c == Underscore;
        }

        internal static bool IsJavaIdentifierPart(ref char c)
        {
            return IsJavaIdentifierStart(ref c) || CharacterClass.IsDigit(ref c);
        }

        internal static CharacterClassification GetCharacterType(ref char c)
        {
            if (c < CharacterClasses.Length) return (CharacterClassification)CharacterClasses[c];
            return CharacterClassification.Unassigned;
        }

        internal const char NewLine = '\n';
        internal const char LineReturn = '\r';
        internal const char Tab = '\t';
        internal const char u0085 = '\u0085';
        internal const char u2028 = '\u2028';
        internal const char u2029 = '\u2029';

        /// <summary>                 
        /// </summary>        
        /// <returns>true if the given character is a newline</returns>
        internal static bool IsNewline(ref char nextChar)
        {
            return nextChar == NewLine || nextChar == LineReturn || nextChar == u0085 || nextChar == u2028 || nextChar == u2029;
        }

        /// <summary>
        /// Compares two characters
        /// </summary>
        /// <param name="a">first character to compare.</param>
        /// <param name="b">second character to compare.</param>
        /// <param name="caseIndependent">whether comparision is case insensitive or not.</param>
        /// <returns>negative, 0, or positive integer as the first character less than, equal to, or greater then the second.</returns>
        internal static int CompareChars(ref char a, ref char b, bool caseIndependent)
        {                      
            if (caseIndependent)
            {
                ToLowerAsciiInvariant(ref a);
                ToLowerAsciiInvariant(ref b);
            }
            return a - b;
        }

        internal static byte[] CharacterClasses = 
        {
            15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 12, 23, 23, 23,
            25, 23, 23, 23, 20, 21, 23, 24, 23, 19, 23, 23, 9, 9, 9, 9, 9, 9, 9, 9, 9,
            9, 23, 23, 24, 24, 24, 23, 23, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 20, 23, 21, 26, 22, 26, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 20, 24, 21,
            24, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 12, 23,
            25, 25, 25, 25, 27, 27, 26, 27, 2, 28, 24, 16, 27, 26, 27, 24, 11, 11, 26,
            2, 27, 23, 26, 11, 2, 29, 11, 11, 11, 23, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 24, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 24, 2, 2, 2,
            2, 2, 2, 2, 2
        };

        internal static void ToLowerAsciiInvariant(ref char c)
        {
            if ('A' <= c && c <= 'Z') c |= ' ';            
        }

        internal static void ToUpperAsciiInvariant(ref char c)
        {
            if ('a' <= c && c <= 'z') c ^= ' ';    
        }

    }
}
