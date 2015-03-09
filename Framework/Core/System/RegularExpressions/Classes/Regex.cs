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
 * RE is an efficient, lightweight regular expression evaluator/matcher
 * class. Regular expressions are pattern descriptions which enable
 * sophisticated matching of strings.  In addition to being able to
 * match a string against a pattern, you can also extract parts of the
 * match.  This is especially useful in text parsing! Details on the
 * syntax of regular expression patterns are given below.
 *
 * <p>
 * To compile a regular expression (RE), you can simply construct an RE
 * matcher object from the string specification of the pattern, like this:
 *
 * <pre>
 *  RE r = new RE("a*b");
 * </pre>
 *
 * <p>
 * Once you have done this, you can call either of the RE.match methods to
 * perform matching on a String.  For example:
 *
 * <pre>
 *  boolean matched = r.match("aaaab");
 * </pre>
 *
 * will cause the boolean matched to be set to true because the
 * pattern "a*b" matches the string "aaaab".
 *
 * <p>
 * If you were interested in the <i>number</i> of a's which matched the
 * first part of our example expression, you could change the expression to
 * "(a*)b".  Then when you compiled the expression and matched it against
 * something like "xaaaab", you would get results like this:
 *
 * <pre>
 *  RE r = new RE("(a*)b");                  // Compile expression
 *  boolean matched = r.match("xaaaab");     // Match against "xaaaab"
 *
 *  String wholeExpr = r.getParen(0);        // wholeExpr will be 'aaaab'
 *  String insideParens = r.getParen(1);     // insideParens will be 'aaaa'
 *
 *  int startWholeExpr = r.getParenStart(0); // startWholeExpr will be index 1
 *  int endWholeExpr = r.getParenEnd(0);     // endWholeExpr will be index 6
 *  int lenWholeExpr = r.getParenLength(0);  // lenWholeExpr will be 5
 *
 *  int startInside = r.getParenStart(1);    // startInside will be index 1
 *  int endInside = r.getParenEnd(1);        // endInside will be index 5
 *  int lenInside = r.getParenLength(1);     // lenInside will be 4
 * </pre>
 *
 * You can also refer to the contents of a parenthesized expression
 * within a regular expression itself.  This is called a
 * 'backreference'.  The first backreference in a regular expression is
 * denoted by \1, the second by \2 and so on.  So the expression:
 *
 * <pre>
 *  ([0-9]+)=\1
 * </pre>
 *
 * will match any string of the form n=n (like 0=0 or 2=2).
 *
 * <p>
 * The full regular expression syntax accepted by RE is described here:
 *
 * <pre>
 *
 *  <b><font face=times roman>Characters</font></b>
 *
 *    <i>unicodeChar</i>   Matches any identical unicode character
 *    \                    Used to quote a meta-character (like '*')
 *    \\                   Matches a single '\' character
 *    \0nnn                Matches a given octal character
 *    \xhh                 Matches a given 8-bit hexadecimal character
 *    \\uhhhh              Matches a given 16-bit hexadecimal character
 *    \t                   Matches an ASCII tab character
 *    \n                   Matches an ASCII newline character
 *    \r                   Matches an ASCII return character
 *    \f                   Matches an ASCII form feed character
 *
 *
 *  <b><font face=times roman>Character Classes</font></b>
 *
 *    [abc]                Simple character class
 *    [a-zA-Z]             Character class with ranges
 *    [^abc]               Negated character class
 * </pre>
 *
 * <b>NOTE:</b> Incomplete ranges will be interpreted as &quot;starts
 * from zero&quot; or &quot;ends with last character&quot;.
 * <br>
 * I.e. [-a] is the same as [\\u0000-a], and [a-] is the same as [a-\\uFFFF],
 * [-] means &quot;all characters&quot;.
 *
 * <pre>
 *
 *  <b><font face=times roman>Standard POSIX Character Classes</font></b>
 *
 *    [:alnum:]            Alphanumeric characters.
 *    [:alpha:]            Alphabetic characters.
 *    [:blank:]            Space and tab characters.
 *    [:cntrl:]            Control characters.
 *    [:digit:]            Numeric characters.
 *    [:graph:]            Characters that are printable and are also visible.
 *                         (A space is printable, but not visible, while an
 *                         `a' is both.)
 *    [:lower:]            Lower-case alphabetic characters.
 *    [:print:]            Printable characters (characters that are not
 *                         control characters.)
 *    [:punct:]            Punctuation characters (characters that are not letter,
 *                         digits, control characters, or space characters).
 *    [:space:]            Space characters (such as space, tab, and formfeed,
 *                         to name a few).
 *    [:upper:]            Upper-case alphabetic characters.
 *    [:xdigit:]           Characters that are hexadecimal digits.
 *
 *
 *  <b><font face=times roman>Non-standard POSIX-style Character Classes</font></b>
 *
 *    [:javastart:]        Start of a Java identifier
 *    [:javapart:]         Part of a Java identifier
 *
 *
 *  <b><font face=times roman>Predefined Classes</font></b>
 *
 *    .         Matches any character other than newline
 *    \w        Matches a "word" character (alphanumeric plus "_")
 *    \W        Matches a non-word character
 *    \s        Matches a whitespace character
 *    \S        Matches a non-whitespace character
 *    \d        Matches a digit character
 *    \D        Matches a non-digit character
 *
 *
 *  <b><font face=times roman>Boundary Matchers</font></b>
 *
 *    ^         Matches only at the beginning of a line
 *    $         Matches only at the end of a line
 *    \b        Matches only at a word boundary
 *    \B        Matches only at a non-word boundary
 *
 *
 *  <b><font face=times roman>Greedy Closures</font></b>
 *
 *    A*        Matches A 0 or more times (greedy)
 *    A+        Matches A 1 or more times (greedy)
 *    A?        Matches A 1 or 0 times (greedy)
 *    A{n}      Matches A exactly n times (greedy)
 *    A{n,}     Matches A at least n times (greedy)
 *    A{n,m}    Matches A at least n but not more than m times (greedy)
 *
 *
 *  <b><font face=times roman>Reluctant Closures</font></b>
 *
 *    A*?       Matches A 0 or more times (reluctant)
 *    A+?       Matches A 1 or more times (reluctant)
 *    A??       Matches A 0 or 1 times (reluctant)
 *
 *
 *  <b><font face=times roman>Logical Operators</font></b>
 *
 *    AB        Matches A followed by B
 *    A|B       Matches either A or B
 *    (A)       Used for subexpression grouping
 *   (?:A)      Used for subexpression clustering (just like grouping but
 *              no backrefs)
 *
 *
 *  <b><font face=times roman>Backreferences</font></b>
 *
 *    \1    Backreference to 1st parenthesized subexpression
 *    \2    Backreference to 2nd parenthesized subexpression
 *    \3    Backreference to 3rd parenthesized subexpression
 *    \4    Backreference to 4th parenthesized subexpression
 *    \5    Backreference to 5th parenthesized subexpression
 *    \6    Backreference to 6th parenthesized subexpression
 *    \7    Backreference to 7th parenthesized subexpression
 *    \8    Backreference to 8th parenthesized subexpression
 *    \9    Backreference to 9th parenthesized subexpression
 * </pre>
 *
 * <p>
 * All closure operators (+, *, ?, {m,n}) are greedy by default, meaning
 * that they match as many elements of the string as possible without
 * causing the overall match to fail.  If you want a closure to be
 * reluctant (non-greedy), you can simply follow it with a '?'.  A
 * reluctant closure will match as few elements of the string as
 * possible when finding matches.  {m,n} closures don't currently
 * support reluctancy.
 *
 * <p>
 * <b><font face="times roman">Line terminators</font></b>
 * <br>
 * A line terminator is a one- or two-character sequence that marks
 * the end of a line of the input character sequence. The following
 * are recognized as line terminators:
 * <ul>
 * <li>A newline (line feed) character ('\n'),</li>
 * <li>A carriage-return character followed immediately by a newline character ("\r\n"),</li>
 * <li>A standalone carriage-return character ('\r'),</li>
 * <li>A next-line character ('\u0085'),</li>
 * <li>A line-separator character ('\u2028'), or</li>
 * <li>A paragraph-separator character ('\u2029).</li>
 * </ul>
 *
 * <p>
 * RE runs programs compiled by the RECompiler class.  But the RE
 * matcher class does not include the actual regular expression compiler
 * for reasons of efficiency.  In fact, if you want to pre-compile one
 * or more regular expressions, the 'recompile' class can be invoked
 * from the command line to produce compiled output like this:
 *
 * <pre>
 *    // Pre-compiled regular expression "a*b"
 *    char[] re1Instructions =
 *    {
 *        0x007c, 0x0000, 0x001a, 0x007c, 0x0000, 0x000d, 0x0041,
 *        0x0001, 0x0004, 0x0061, 0x007c, 0x0000, 0x0003, 0x0047,
 *        0x0000, 0xfff6, 0x007c, 0x0000, 0x0003, 0x004e, 0x0000,
 *        0x0003, 0x0041, 0x0001, 0x0004, 0x0062, 0x0045, 0x0000,
 *        0x0000,
 *    };
 *
 *
 *    REProgram re1 = new REProgram(re1Instructions);
 * </pre>
 *
 * You can then construct a regular expression matcher (RE) object from
 * the pre-compiled expression re1 and thus avoid the overhead of
 * compiling the expression at runtime. If you require more dynamic
 * regular expressions, you can construct a single RECompiler object and
 * re-use it to compile each expression. Similarly, you can change the
 * program run by a given matcher object at any time. However, RE and
 * RECompiler are not threadsafe (for efficiency reasons, and because
 * requiring thread safety in this class is deemed to be a rare
 * requirement), so you will need to construct a separate compiler or
 * matcher object for each thread (unless you do thread synchronization
 * yourself). Once expression compiled into the REProgram object, REProgram
 * can be safely shared across multiple threads and RE objects.
 *
 * <br><p><br>
 *
 * <font color="red">
 * <i>ISSUES:</i>
 *
 * <ul>
 *  <li>com.weusours.util.re is not currently compatible with all
 *      standard POSIX regcomp flags</li>
 *  <li>com.weusours.util.re does not support POSIX equivalence classes
 *      ([=foo=] syntax) (I18N/locale issue)</li>
 *  <li>com.weusours.util.re does not support nested POSIX character
 *      classes (definitely should, but not completely trivial)</li>
 *  <li>com.weusours.util.re Does not support POSIX character collation
 *      concepts ([.foo.] syntax) (I18N/locale issue)</li>
 *  <li>Should there be different matching styles (simple, POSIX, Perl etc?)</li>
 *  <li>Should RE support character iterators (for backwards RE matching!)?</li>
 *  <li>Should RE support reluctant {m,n} closures (does anyone care)?</li>
 *  <li>Not *all* possibilities are considered for greediness when backreferences
 *      are involved (as POSIX suggests should be the case).  The POSIX RE
 *      "(ac*)c*d[ac]*\1", when matched against "acdacaa" should yield a match
 *      of acdacaa where \1 is "a".  This is not the case in this RE package,
 *      and actually Perl doesn't go to this extent either!  Until someone
 *      actually complains about this, I'm not sure it's worth "fixing".
 *      If it ever is fixed, test #137 in RETest.txt should be updated.</li>
 * </ul>
 *
 * </font>
 *
 * @see recompile 
 * @see RECompiler -> RegexCompiler
 *
 * @author <a href="mailto:jonl@muppetlabs.com">Jonathan Locke</a>
 * @author <a href="mailto:ts@sch-fer.de">Tobias Sch&auml;fer</a>
 * @version $Id: RE.java 518156 2007-03-14 14:31:26Z vgritsenko $
 */
using System;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Microsoft.SPOT.Platform.Tests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Microsoft.SPOT.Platform.Tests.TextTests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Microsoft.SPOT.Platform.Tests.RegExTests")]

namespace System.Text.RegularExpressions
{
    /// <summary>
    /// A lightweight Regular Expression Engine and Parser. Contributed by Julius Friedman
    /// </summary>
    public sealed class Regex
    {
        #region Constants

        /// <summary>
        /// Maximum number of nodes in a program
        /// </summary>
        const int MaximumNodes = 65536;

        /// <summary>
        /// Number of paren pairs (only 9 can be backrefs)
        /// </summary>
        const int MaximumSubExpressions = 16;

        #region [Node Layout]

        /// <summary>
        /// Opcode offset (first character)
        /// </summary>
        internal const int offsetOpcode = 0;

        /// <summary>
        /// Opdata offset (second char)
        /// </summary>
        internal const int offsetOpdata = 1;

        /// <summary>
        /// Next index offset (third char)
        /// </summary>
        internal const int offsetNext = 2;

        /// <summary>
        /// Node size (in chars)
        /// </summary>
        internal const int nodeSize = 3;

        #endregion

        #endregion

        #region Fields

        #region[State of current program]

        /// <summary>
        /// Compiled regular expression 'program'
        /// </summary>
        internal RegexProgram program;
        /// <summary>
        /// The string being matched against indirectly through an interface. 
        /// If unsafe access becomes supported this can become more optomized in the implementation
        /// </summary>
        internal ICharacterIterator search;           
        /// <summary>
        /// Match behaviour flags
        /// </summary>
        internal RegexOptions matchFlags;
        /// <summary>
        /// The maximum amount of matches to perform -1 
        /// </summary>
        internal int maxMatches = MaximumSubExpressions;
        /// <summary>
        /// Are we timing? If this is true the engine will ensure not alot of ticks have passed when matching each node.
        /// If it has so determined that it is taking a long time a RegexExecutionException is generated and will be thrown. 
        /// Developers can catch this exception to check if anything has been matched thus far in the Regex. 
        /// They can then continue the match at the index from which the exception was thrown by using IsMatch(someInput, ex.Index)
        /// </summary>
        internal bool timed;
        /// <summary>
        /// The time we started matching at Tick wise
        /// </summary>
        internal long startTicks;
        /// <summary>
        /// EXPERIMENTAL USE ONLY! 
        /// Regex must have RegexOption.Timed Applied before calling a method either manually or through Compilation!
        /// The maximum amount of ticks this Regex will wait before throwing a ExecutionException allowing the developer to evaluate and continue.
        /// Other interrups will also be allows to process in regions of the controller at this time due to this break if handled correctly by the developer.
        /// With this Default value ALL TESTS SUCCESSFULLY PASS IF TIMED! Could be set based on input length to be more specific but would cost an operation.
        /// </summary>
        internal long MaxTick = long.MaxValue / 2;

        #endregion

        // Parenthesized subexpressions
        internal int matchCount;                     // Number of subexpressions matched (num open parens + 1)
        internal int start0;                         // Cache of start[0]
        internal int end0;                           // Cache of end[0]
        internal int start1;                         // Cache of start[1]
        internal int end1;                           // Cache of end[1]
        internal int start2;                         // Cache of start[2]
        internal int end2;                           // Cache of end[2]
        internal int[] starts;                       // Lazy-alloced array of sub-expression starts
        internal int[] ends;                         // Lazy-alloced array of sub-expression ends

        // Backreferences
        internal int[] startBackref;                 // Lazy-alloced array of backref starts
        internal int[] endBackref;                   // Lazy-alloced array of backref ends        

        //Compilation Cache
        static internal int cacheSize = 25;
        static internal System.Collections.Stack regexpCache = new System.Collections.Stack();

        #endregion

        #region Properties

        /// <summary>
        /// The size of the Regex Cache indicating how many RegexProgram's will be stored in the Stack for later use.
        /// </summary>
        public static int CacheSize
        {
            get
            {
                return cacheSize;
            }
            set
            {
                ///If the value is 0 then we must clear the cache
                if ((cacheSize = value) <= 0)
                {
                    Regex.Cache.Clear();
                }
            }
        }

        /// <summary>
        /// The Stack of RegexPrograms which have been cached by compiling Regular Expressions. 
        /// Potentially there may be lots of threads accessing the cache. Since we do not have a concurrent collection this critical section provides synchronization
        /// </summary>        
        static internal System.Collections.Stack Cache
        {
            get
            {
                lock (regexpCache.SyncRoot)
                {
                    return regexpCache;
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a regular expression matcher from a String by compiling it
        /// using a new instance of RECompiler.  If you will be compiling many
        /// expressions, you may prefer to use a single RECompiler object instead.
        /// </summary>
        /// <param name="pattern">he regular expression pattern to compile.</param>
        public Regex(String pattern)
            : this(pattern, RegexOptions.None) { }

       /// <summary>
       /// Constructs a regular expression matcher from a String by compiling it
       /// using a new instance of RECompiler.  If you will be compiling many
       /// expressions, you may prefer to use a single RECompiler object instead.
       /// </summary>
        /// <param name="pattern">The regular expression pattern to compile.</param>
       /// <param name="matchFlags">The MatchOptions</param>
        public Regex(String pattern, RegexOptions matchFlags)
            : this(new RegexCompiler().Compile(pattern), matchFlags) { }

        /// <summary>
        /// Internal use only.
        /// Construct a matcher for a pre-compiled regular expression from program
        /// bytecode) data.  Permits special flags to be passed in to modify matching
        /// behaviour.
        /// RegexOptions.Normal              // Normal (case-sensitive) matching
        /// RegexOptions.CaseIndependant     // Case folded comparisons
        /// RegexOptions.Multiline           // Newline matches as BOL/EOL
        /// </summary>
        /// <param name="program">Compiled regular expression program (see RECompiler)</param>
        /// <param name="matchFlags">One or more of the MatchOptions</param>
        internal Regex(RegexProgram program, RegexOptions matchFlags)
        {
            Program = program;
            Options = matchFlags;
        }

        /// <summary>
        /// Internal use only.
        /// Construct a matcher for a pre-compiled regular expression from program
        /// (bytecode) data. Internal use only
        /// </summary>
        /// <param name="program">Compiled regular expression program</param>
        internal Regex(RegexProgram program)
            : this(program, RegexOptions.None) { }

        /// <summary>
        /// Internal use only.
        /// Constructs a regular expression matcher with no initial program.
        /// This is likely to be an uncommon practice, but is still supported.
        /// </summary>
        public Regex()
            : this((RegexProgram)null, RegexOptions.None) { }

        #endregion

        #region Properties

        /// <summary>
        /// The Match Options of this Regular Expression
        /// </summary>
        public RegexOptions Options
        {
            get
            {
                return matchFlags;
            }
            set
            {
                matchFlags = value;
            }
        }

        /// <summary>
        /// The compiled RegularExpression
        /// </summary>
        internal RegexProgram Program
        {
            get
            {
                return program;
            }
            set
            {
                program = value;
                if (program != null && program.MaximumMatches != -1) maxMatches = program.MaximumMatches;
                else maxMatches = MaximumSubExpressions;                
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts a 'simplified' regular expression to a full regular expression
        /// </summary>
        /// <param name="simplePattern">The pattern to convert</param>
        /// <returns>The full regular expression</returns>
        public static String ToFullRegularExpression(String simplePattern)
        {
            StringBuilder buf = new StringBuilder();
            for (int i = 0, e = simplePattern.Length; i < e; i++)
            {
                char c = simplePattern[i];
                switch (c)
                {
                    case CharacterClass.DefaultChar:
                        {
                            buf.Append(c);
                            break;
                        }
                    case '*':
                        {
                            buf.Append(".*");
                            break;
                        }
                    case '.':
                    case '[':
                    case ']':
                    case OpCode.Escape:
                    case OpCode.Plus:
                    case '?':
                    case '{':
                    case '}':
                    case OpCode.EndOfLine:
                    case '^':
                    case '|':
                    case OpCode.Open:
                    case OpCode.Close:
                        {
                            buf.Append(OpCode.Escape);
                            goto case CharacterClass.DefaultChar;
                        }
                }
            }
            return buf.ToString();
        }

        /// <summary>
        /// Matches the current regular expression program against a character array,starting at a given index.
        /// </summary>
        /// <param name="search">String to match against</param>
        /// <param name="i">Index to start searching at</param>
        /// <returns>True if string matched</returns>
        public bool IsMatch(String search, int i)
        {
            return IsMatch(new StringCharacterIterator(search), i);
        }

        /// <summary>
        /// Matches the current regular expression program against a String.
        /// </summary>
        /// <param name="search">String to match against</param>
        /// <returns>True if search string matched</returns>
        public bool IsMatch(String search)
        {
            return IsMatch(search, 0);
        }

        #region [Engine Helper Methods]

        /// <summary>
        ///  Sets the start of a paren level
        /// </summary>
        /// <param name="which">Which paren level</param>
        /// <param name="i">i Index in input array</param>
        void SetSubExpressionStart(int which, int i)
        {
            if (which < matchCount)
            {
                switch (which)
                {
                    case 0:
                        {
                            start0 = i;
                            break;
                        }
                    case 1:
                        {
                            start1 = i;
                            break;
                        }
                    case 2:
                        {
                            start2 = i;
                            break;
                        }
                    default:
                        {
                            if (starts == null) AllocateSubExpressions();
                            starts[which] = i;
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Sets the end index of a subexpression.
        /// </summary>
        /// <param name="which">Which subexpression</param>
        /// <param name="i">Index in input ICharacterStream</param>
        void SetSubExpressionEnd(int which, int i)
        {
            if (which < matchCount)
            {
                switch (which)
                {
                    case 0:
                        {
                            end0 = i;
                            break;
                        }
                    case 1:
                        {
                            end1 = i;
                            break;
                        }
                    case 2:
                        {
                            end2 = i;
                            break;
                        }
                    default:
                        {
                            if (ends == null) AllocateSubExpressions();
                            ends[which] = i;
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Throws an Error representing an internal error condition probably resulting
        /// from a bug in the regular expression compiler (or possibly data corruption).
        /// In practice, this should be very rare.
        /// </summary>
        /// <param name="s">Error description</param>
        void InternalError(String s)
        {            
            throw new Exception(string.Concat("Regexp Internal Error: ", s));
        }

        /// <summary>
        /// Performs lazy allocation of subexpression arrays
        /// </summary>
        void AllocateSubExpressions()
        {
            // Allocate arrays for subexpressions
            starts = new int[maxMatches];
            ends = new int[maxMatches];

            // Set sub-expression pointers to invalid values
            for (int i = 0; i < maxMatches; ++i) starts[i] = ends[i] = -1;
        }

        /// <summary>
        /// Gets the contents of a parenthesized subexpression after a successful match.
        /// </summary>
        /// <param name="which">Nesting level of subexpression</param>
        /// <returns>String Subexpression if present, otherwise null</returns>
        internal String Group(int which)
        {
            int start;
            //If the subexpression is present then move the start index to the start of the subexpression
            //If the result of moving the startindex is positive then return the range
            if (which < matchCount && (start = GroupStart(which)) >= 0) return search.Range(start, GroupEnd(which));
            //return null otherwise
            return null;
        }

        /// <summary>
        /// Returns the start index of a given subexpression.
        /// </summary>
        /// <param name="which">Nesting level of subexpression</param>
        /// <returns>String index</returns>
        internal int GroupStart(int which)
        {
            if (which < matchCount)
            {
                switch (which)
                {
                    case 0:
                        {
                            return start0;
                        }
                    case 1:
                        {
                            return start1;
                        }
                    case 2:
                        {
                            return start2;
                        }
                    default:
                        {
                            if (starts == null) AllocateSubExpressions();
                            return starts[which];
                        }
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the end index of a given subexpression.
        /// </summary>
        /// <param name="which">Nesting level of subexpression</param>
        /// <returns>index in input ICharacterStream</returns>
        internal int GroupEnd(int which)
        {
            if (which < matchCount)
            {
                switch (which)
                {
                    case 0:
                        {
                            return end0;
                        }
                    case 1:
                        {
                            return end1;
                        }
                    case 2:
                        {
                            return end2;
                        }
                    default:
                        {
                            if (ends == null) AllocateSubExpressions();
                            return ends[which];
                        }
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the Length of a given subexpression level.
        /// </summary>
        /// <param name="which">Nesting level of subexpression</param>
        /// <returns>The length of the subexpression if it exists otherwise -1</returns>
        internal int GroupLength(int which)
        {
            if (which < matchCount) return GroupEnd(which) - GroupStart(which);
            return -1;
        }

        #endregion

        #region [Internal Matching Logic]

        /// <summary>
        /// Matches the current regular expression program against a character array,starting at a given index.
        /// </summary>
        /// <param name="search">String to match against</param>
        /// <param name="i">Index to start searching at</param>
        /// <returns>True if search string matched</returns>
        bool IsMatch(ICharacterIterator search, int i)
        {
            // There is no compiled program to search with!
            if (program == null)
            {
                // This should be uncommon enough to be an error case rather
                // than an exception (which would have to be handled everywhere)
                InternalError("No RE program to run!");
            }

            // Save string to search
            this.search = search;

            // Can we optimize the search by looking for new lines?
            if ((program.Flags & ProgramOptions.HasBeginOfLine) == ProgramOptions.HasBeginOfLine)
            {
                // Non multi-line matching with BOL: Must match at '0' index
                if ((matchFlags & RegexOptions.Multiline) == 0) return i == 0 && MatchAt(i);

                // Multi-line matching with BOL: Seek to next line
                for (; !search.IsEnd(i); ++i)
                {
                    char currentChar = search.CharAt(i);
                    // Skip if we are at the beginning of the line
                    if (CharacterClass.IsNewline(ref currentChar)) continue;

                    // Match at the beginning of the line
                    if (MatchAt(i)) return true;

                    // Skip to the end of line
                    for (; !search.IsEnd(i); ++i) if (CharacterClass.IsNewline(ref currentChar)) break;
                }

                return false;
            }

            // Can we optimize the search by looking for a prefix string?
            if (program.Prefix == null)
            {
                // Unprefixed matching must try for a match at each character                
                for (; !search.IsEnd(i - 1); ++i) if (MatchAt(i)) return true;// Try a match at index i
                return false;
            }
            else
            {
                // Prefix-anchored matching is possible
                bool caseIndependent = (matchFlags & RegexOptions.IgnoreCase) != 0;
                char[] prefix = program.Prefix;
                int prefixLength = prefix.Length;
                for (; !search.IsEnd(i + prefixLength - 1); ++i)
                {
                    int j = i;
                    int k = 0;

                    bool match;
                    do
                    {
                        char currentChar = search.CharAt(j++);
                        char nextChar = prefix[k++];
                        // If there's a mismatch of any character in the prefix, give up
                        match = (CharacterClass.CompareChars(ref currentChar, ref nextChar, caseIndependent) == 0);
                    } while (match && k < prefixLength);

                    // See if the whole prefix string matched
                    if (k == prefixLength)
                        if (MatchAt(i)) return true;// We matched the full prefix at firstChar, so try it
                }
                return false;
            }
        }

        /// <summary>
        /// Try to match a string against a subset of nodes in the program
        /// </summary>
        /// <param name="firstNode">firstNode Node to start at in program</param>
        /// <param name="lastNode">lastNode  Last valid node (used for matching a subexpression without matching the rest of the program as well).</param>
        /// <param name="idxStart">idxStart  Starting position in character array</param>
        /// <returns> input ICharacterStream index if match succeeded, otherwise -1.</returns>
        int MatchNodes(int firstNode, int lastNode, int idxStart)
        {

            //If we are keeping time and the time we started matching at is more then MaxTick ticks ago something went wrong allow for a break - JRF
            if (timed && (DateTime.Now.Ticks - startTicks) >= MaxTick) throw new RegexExecutionTimeException(idxStart);
            else startTicks = DateTime.Now.Ticks;

            // Our current place in the string
            int idx = idxStart;

            // Loop while node is valid
            int next, opcode = 0, opdata;
            int idxNew;
            char[] instructions = program.Instructions;
            for (int node = firstNode; node < lastNode; )
            {
                opcode = instructions[node /* + offsetOpcode */];
                next = node + (short)instructions[node + offsetNext];
                opdata = instructions[node + offsetOpdata];

                switch (opcode)
                {
                    default:
                        // Corrupt program                        
                        break;

                    #region Maybe / Star

                    case OpCode.Maybe:
                    case OpCode.Star:
                        {
                            // Try to match the following subexpr. If it matches:
                            //   MAYBE:  Continues matching rest of the expression
                            //    STAR:  Points back here to repeat subexpr matching
                            if ((idxNew = MatchNodes(node + nodeSize, MaximumNodes, idx)) != -1)
                            {
                                return idxNew;
                            }

                            // If failed, just continue with the rest of expression
                            break;
                        }
                    #endregion

                    #region Plus
                    case OpCode.Plus:
                        {
                            // Try to match the subexpr again (and again (and ...
                            if ((idxNew = MatchNodes(next, MaximumNodes, idx)) != -1)
                            {
                                return idxNew;
                            }

                            // If failed, just continue with the rest of expression
                            // Rest is located at the next pointer of the next instruction
                            // (which must be OP_CONTINUE)
                            node = next + (short)instructions[next + offsetNext];
                            continue;
                        }
                    #endregion

                    #region Reluctant Maybe / Start / Plus

                    case OpCode.ReluctantMaybe:
                    case OpCode.ReluctantStar:
                        {
                            // Try to match the rest without using the reluctant subexpr
                            if ((idxNew = MatchNodes(next, MaximumNodes, idx)) != -1)
                            {
                                return idxNew;
                            }

                            // Try reluctant subexpr. If it matches:
                            //   RELUCTANTMAYBE: Continues matching rest of the expression
                            //    RELUCTANTSTAR: Points back here to repeat reluctant star matching
                            return MatchNodes(node + nodeSize, next, idx);
                        }
                    case OpCode.ReluctantPlus:
                        {
                            // Continue matching the rest without using the reluctant subexpr
                            if ((idxNew = MatchNodes(next + (short)instructions[next + offsetNext], MaximumNodes, idx)) != -1)
                            {
                                return idxNew;
                            }

                            // Try to match subexpression again
                            break;
                        }

                    #endregion

                    #region Open

                    case OpCode.Open:

                        // Match subexpression
                        if ((program.Flags & ProgramOptions.HasBackrefrence) != 0)
                        {
                            startBackref[opdata] = idx;
                        }
                        if ((idxNew = MatchNodes(next, MaximumNodes, idx)) != -1)
                        {
                            // Increase valid paren count
                            if (opdata >= matchCount)
                            {
                                matchCount = opdata + 1;
                            }

                            // Don't set paren if already set later on
                            if (GroupStart(opdata) == -1)
                            {
                                SetSubExpressionStart(opdata, idx);
                            }
                        }
                        return idxNew;

                    #endregion

                    #region Close

                    case OpCode.Close:

                        // Done matching subexpression
                        if ((program.Flags & ProgramOptions.HasBackrefrence) != 0)
                        {
                            endBackref[opdata] = idx;
                        }
                        if ((idxNew = MatchNodes(next, MaximumNodes, idx)) != -1)
                        {
                            // Increase valid paren count
                            if (opdata >= matchCount)
                            {
                                matchCount = opdata + 1;
                            }

                            // Don't set paren if already set later on
                            if (GroupEnd(opdata) == -1)
                            {
                                SetSubExpressionEnd(opdata, idx);
                            }
                        }
                        return idxNew;

                    #endregion

                    #region BackRef
                    case OpCode.BackRef:
                        {
                            // Get the start and end of the backref
                            int s = startBackref[opdata];
                            int e = endBackref[opdata];

                            // We don't know the backref yet
                            if (s == -1 || e == -1)
                            {
                                return -1;
                            }

                            // The backref is empty size
                            if (s == e)
                            {
                                //s = startBackref[Math.Max(opdata - 1, 0)];
                                break;
                            }

                            // Get the Length of the backref
                            int l = e - s;

                            // If there's not enough input left, give up.
                            if (search.IsEnd(idx + l - 1))
                            {
                                return -1;
                            }

                            // Case fold the backref?
                            bool caseFold = ((matchFlags & RegexOptions.IgnoreCase) != 0);

                            // Compare backref to input
                            for (int i = 0; i < l; ++i)
                            {
                                char currentChar = search.CharAt(idx++);
                                char nextChar = search.CharAt(s + i);
                                //if (CompareChars(search.CharAt(idx++), search.CharAt(s + i), caseFold) != 0)                                
                                if (CharacterClass.CompareChars(ref currentChar, ref nextChar, caseFold) != 0)
                                {
                                    return -1;
                                }
                            }
                        }
                        break;
                    #endregion

                    #region BeginOfLine

                    case OpCode.BeginOfLine:

                        // Fail if we're not at the start of the string
                        if (idx != 0)
                        {
                            // If we're multiline matching, we could still be at the start of a line
                            if ((matchFlags & RegexOptions.Multiline) == RegexOptions.Multiline)
                            {
                                char currentChar = search.CharAt(idx - 1);
                                // Continue if at the start of a line
                                if (CharacterClass.IsNewline(ref currentChar)) break;
                            }
                            return -1;
                        }
                        break;
                    #endregion

                    #region EndOfLine

                    case OpCode.EndOfLine:

                        // If we're not at the end of string
                        if (!search.IsEnd(0) && !search.IsEnd(idx))
                        {
                            // If we're multi-line matching
                            if ((matchFlags & RegexOptions.Multiline) == RegexOptions.Multiline)
                            {
                                char currentChar = search.CharAt(idx);
                                // Continue if we're at the end of a line
                                if (CharacterClass.IsNewline(ref currentChar))
                                {
                                    break;
                                }
                            }
                            return -1;
                        }
                        break;

                    #endregion

                    #region Any

                    case OpCode.Any:

                        if ((matchFlags & RegexOptions.Singleline) == RegexOptions.Singleline)
                        {
                            // Match anything
                            if (search.IsEnd(idx)) return -1;
                        }
                        else
                        {
                            // Match anything but a newline
                            if (search.IsEnd(idx)) return -1;
                            char currentChar = search.CharAt(idx);
                            if (CharacterClass.IsNewline(ref currentChar)) return -1;
                        }
                        ++idx;
                        break;

                    #endregion

                    #region AnyOf
                    case OpCode.AnyOf:
                        {
                            // Out of input?
                            if (search.IsEnd(idx))
                            {
                                return -1;
                            }

                            // Get character to match against character class and maybe casefold
                            char c = search.CharAt(idx);
                            bool caseFold = (matchFlags & RegexOptions.IgnoreCase) != 0;

                            // Loop through character class checking our match character
                            int idxRange = node + nodeSize;
                            int idxEnd = idxRange + (opdata * 2);
                            bool match = false;

                            for (int i = idxRange; !match && i < idxEnd; )
                                match = ((CharacterClass.CompareChars(ref c, ref instructions[i++], caseFold) >= 0) && (CharacterClass.CompareChars(ref c, ref instructions[i++], caseFold) <= 0));

                            // Fail if we didn't match the character class
                            if (!match)
                            {
                                return -1;
                            }
                            ++idx;
                        }
                        break;
                    #endregion

                    #region Branch
                    case OpCode.Branch:
                        {
                            // Check for choices
                            // FIXME Dead code - only reason to keep is backward compat with pre-compiled exprs. Remove?
                            if (instructions[next /* + offsetOpcode */] != OpCode.Branch)
                            {
                                // If there aren't any other choices, just evaluate this branch.
                                node += nodeSize;
                                continue;
                            }

                            // Try all available branches
                            int nextBranch;
                            do
                            {
                                // Try matching the branch against the string
                                if ((idxNew = MatchNodes(node + nodeSize, MaximumNodes, idx)) != -1)
                                {
                                    return idxNew;
                                }

                                // Go to next branch (if any)
                                nextBranch = (short)instructions[node + offsetNext];
                                node += nextBranch;
                            } while (nextBranch != 0 && (instructions[node /* + offsetOpcode */] == OpCode.Branch));

                            // Failed to match any branch!
                            return -1;
                        }
                    #endregion

                    #region Open / Close Cluster

                    case OpCode.OpenCluster:
                    case OpCode.CloseCluster:
                    // starting or ending the matching of a subexpression which has no backref.

                    #endregion

                    #region Nothing

                    case OpCode.Nothing:
                    case OpCode.GoTo:

                        // Just advance to the next node without doing anything
                        break;

                    #endregion

                    #region Continue

                    case OpCode.Continue:

                        // Advance to the following node
                        node += nodeSize;
                        continue;

                    #endregion

                    #region EndProgram

                    case OpCode.EndProgram:

                        // Match has succeeded!
                        SetSubExpressionEnd(0, idx);
                        return idx;

                    #endregion

                    #region Atom

                    case OpCode.Atom:
                        {
                            // Match an atom value
                            if (search.IsEnd(idx))
                            {
                                return -1;
                            }

                            // Get Length of atom and starting index
                            // int lenAtom = opdata;
                            int startAtom = node + nodeSize;

                            // Give up if not enough input remains to have a match
                            if (search.IsEnd(opdata + idx - 1))
                            {
                                return -1;
                            }

                            // Match atom differently depending on compareCase flag
                            bool compareCase = ((matchFlags & RegexOptions.IgnoreCase) != 0);

                            for (int i = 0; i < opdata; i++)
                            {
                                char currentChar = search.CharAt(idx++);
                                char nextChar = instructions[startAtom + i];
                                if (CharacterClass.CompareChars(ref currentChar, ref nextChar, compareCase) != 0) return -1;
                            }

                        }
                        break;

                    #endregion

                    #region Escape
                    case OpCode.Escape:
                        // Which escape?
                        switch (opdata)
                        {
                            // Word boundary match
                            case EscapeCode.NonWordBoundry:
                            case EscapeCode.WordBoundry:
                                {
                                    char cLast = ((idx == 0) ? CharacterClass.NewLine : search.CharAt(idx - 1));
                                    char cNext = ((search.IsEnd(idx)) ? CharacterClass.NewLine : search.CharAt(idx));
                                    if ((CharacterClass.IsLetterOrDigit(ref cLast) == CharacterClass.IsLetterOrDigit(ref cNext)) == (opdata == EscapeCode.WordBoundry))
                                    {
                                        return -1;
                                    }
                                }
                                break;

                            // Alpha-numeric, digit, space, javaLetter, javaLetterOrDigit
                            case EscapeCode.Alphanumeric:
                            case EscapeCode.NonAlphanumeric:
                            case EscapeCode.Digit:
                            case EscapeCode.NonDigit:
                            case EscapeCode.Whitespace:
                            case EscapeCode.NonWhitespace:

                                // Give up if out of input
                                if (search.IsEnd(idx))
                                {
                                    return -1;
                                }

                                char c = search.CharAt(idx);

                                // Switch on escape
                                switch (opdata)
                                {
                                    case EscapeCode.Alphanumeric:
                                    case EscapeCode.NonAlphanumeric:
                                        if (!((CharacterClass.IsLetterOrDigit(ref c) || c == '_') == (opdata == EscapeCode.Alphanumeric)))
                                        {
                                            return -1;
                                        }
                                        break;

                                    case EscapeCode.Digit:
                                    case EscapeCode.NonDigit:
                                        if (!(CharacterClass.IsDigit(ref c) == (opdata == EscapeCode.Digit)))
                                        {
                                            return -1;
                                        }
                                        break;

                                    case EscapeCode.Whitespace:
                                    case EscapeCode.NonWhitespace:
                                        if (!(CharacterClass.IsWhitespace(ref c) == (opdata == EscapeCode.Whitespace)))
                                        {
                                            return -1;
                                        }
                                        break;
                                }
                                ++idx;
                                break;

                            default:
                                InternalError("Unrecognized escape '" + opdata + "'");
                                break;
                        }
                        break;
                    #endregion

                    #region PosixClass
                    case OpCode.PosixClass:
                        {
                            // Out of input?
                            if (search.IsEnd(idx))
                            {
                                return -1;
                            }

                            char currentChar = search.CharAt(idx);

                            switch (opdata)
                            {
                                case POSIXCharacterClass.Alphanumeric:
                                    if (!CharacterClass.IsLetterOrDigit(ref currentChar))
                                    {
                                        return -1;
                                    }
                                    break;

                                case POSIXCharacterClass.Alphabetica:
                                    if (!CharacterClass.IsLetter(ref currentChar))
                                    {
                                        return -1;
                                    }
                                    break;

                                case POSIXCharacterClass.Digit:
                                    if (!CharacterClass.IsDigit(ref currentChar))
                                    {
                                        return -1;
                                    }
                                    break;

                                case POSIXCharacterClass.Blank: // JWL - bugbug: is this right??
                                    if (!CharacterClass.IsSpaceChar(ref currentChar))
                                    {
                                        return -1;
                                    }
                                    break;

                                case POSIXCharacterClass.Spaces:
                                    if (!CharacterClass.IsWhitespace(ref currentChar))
                                    {
                                        return -1;
                                    }
                                    break;

                                case POSIXCharacterClass.Control:
                                    if (CharacterClass.GetCharacterType(ref currentChar) != CharacterClass.CharacterClassification.Control)
                                    {
                                        return -1;
                                    }
                                    break;

                                case POSIXCharacterClass.GraphicCharacter: // JWL - bugbug???
                                    switch (CharacterClass.GetCharacterType(ref currentChar))
                                    {
                                        case CharacterClass.CharacterClassification.MathSymbol:
                                        case CharacterClass.CharacterClassification.CurrencySymbol:
                                        case CharacterClass.CharacterClassification.ModifierSymbol:
                                        case CharacterClass.CharacterClassification.OtherSymbol:
                                            break;

                                        default:
                                            return -1;
                                    }
                                    break;

                                case POSIXCharacterClass.LowerCase:
                                    if (CharacterClass.GetCharacterType(ref currentChar) != CharacterClass.CharacterClassification.LowercaseLetter)
                                    {
                                        return -1;
                                    }
                                    break;

                                case POSIXCharacterClass.UpperCase:
                                    if (CharacterClass.GetCharacterType(ref currentChar) != CharacterClass.CharacterClassification.UppercaseLetter)
                                    {
                                        return -1;
                                    }
                                    break;

                                case POSIXCharacterClass.Printable:
                                    if (CharacterClass.GetCharacterType(ref currentChar) == CharacterClass.CharacterClassification.Control)
                                    {
                                        return -1;
                                    }
                                    break;

                                case POSIXCharacterClass.Punctuation:
                                    {
                                        switch (CharacterClass.GetCharacterType(ref currentChar))
                                        {
                                            case CharacterClass.CharacterClassification.DashPunctuation:
                                            case CharacterClass.CharacterClassification.StartPunctuation:
                                            case CharacterClass.CharacterClassification.EndPunctuation:
                                            case CharacterClass.CharacterClassification.ConnectorPunctuation:
                                            case CharacterClass.CharacterClassification.OtherPunctuation:
                                                break;

                                            default:
                                                return -1;
                                        }
                                    }
                                    break;

                                case POSIXCharacterClass.Hexadecimal: // JWL - bugbug??
                                    {
                                        bool isXDigit = ((search.CharAt(idx) >= '0' && search.CharAt(idx) <= '9') ||
                                                (search.CharAt(idx) >= 'a' && search.CharAt(idx) <= 'f') ||
                                                (search.CharAt(idx) >= 'A' && search.CharAt(idx) <= 'F'));
                                        if (!isXDigit)
                                        {
                                            return -1;
                                        }
                                    }
                                    break;

                                case POSIXCharacterClass.JavaIdentifierStart:
                                    if (!CharacterClass.IsJavaIdentifierStart(ref currentChar))
                                    {
                                        return -1;
                                    }
                                    break;

                                case POSIXCharacterClass.JavaIdentifierPart:
                                    if (!CharacterClass.IsJavaIdentifierPart(ref currentChar))
                                    {
                                        return -1;
                                    }
                                    break;

                                default:
                                    InternalError("Bad posix class");
                                    break;
                            }

                            // Matched.
                            ++idx;
                        }
                        break;
                    #endregion

                }

                // Advance to the next node in the program
                node = next;
            }
            // We "should" never end up here            
            InternalError("Corrupt program" + "Invalid opcode '" + opcode + "'");
            return -1;
        }

        /// <summary>
        /// Match the current regular expression program against the current
        /// input string, starting at index i of the input string.  This method
        /// is only meant for internal use.
        /// </summary>
        /// <param name="i">The input string index to start matching at</param>
        /// <returns>True if the input matched the expression</returns>
        bool MatchAt(int i)
        {
            // Initialize start pointer, paren cache and paren count
            start0 = -1;
            end0 = -1;
            start1 = -1;
            end1 = -1;
            start2 = -1;
            end2 = -1;
            starts = null;
            ends = null;
            matchCount = 1;
            SetSubExpressionStart(0, i);

            // Allocate backref arrays (unless optimizations indicate otherwise)
            if ((program.Flags & ProgramOptions.HasBackrefrence) != 0) startBackref = endBackref = new int[maxMatches];

            // Match against string
            int idx;
            if ((idx = MatchNodes(0, MaximumNodes, i)) != -1)
            {
                SetSubExpressionEnd(0, idx);
                return true;
            }

            // Didn't match
            matchCount = 0;
            return false;
        }

        #endregion

        #region [Split]

        /// <summary>
        /// Splits a string into an array of strings on regular expression boundaries.
        /// This function works the same way as the Perl function of the same name.
        /// Given a regular expression of "[ab]+" and a string to split of
        /// "xyzzyababbayyzabbbab123", the result would be the array of Strings
        /// "[xyzzy, yyz, 123]".
        /// <p>Please note that the first string in the resulting array may be an empty
        /// string. This happens when the very first character of input string is
        /// matched by the pattern.</p>
        /// </summary>
        /// <param name="s">String to split on this regular exression</param>
        /// <param name="maxMatches">The maximum number of matches to split</param>
        /// <param name="start">The offset in the string to start at</param>
        /// <returns>Array of strings</returns>
        public String[] Split(String s, int maxMatches, int start, int length)
        {
            if (maxMatches < 0) maxMatches = int.MaxValue;

            // Create new System.Collections.ArrayList
            System.Collections.ArrayList v = new System.Collections.ArrayList();

            // Start at position 0 and search the whole string
            int pos = start;
            //int len = s.Length;
            int len = length;
            int matchCount = 0;
            // Try a match at each position
            while (pos < len && IsMatch(s, pos) && matchCount < maxMatches)
            {
                //increment matches
                ++matchCount;

                // Get start of match
                start = start0;

                // Get end of match
                int newpos = end0;

                // Check if no progress was made
                if (newpos == pos)
                {
                    
                    v.Add(s.Substring(pos, start + 1));
                    ++newpos;
                }
                else v.Add(s.Substring(pos, start - pos));

                // Move to new position
                pos = newpos;
            }

            // Push remainder if it's not empty
            String remainder = s.Substring(pos);
            if (remainder.Length != 0) v.Add(remainder);

            // Return System.Collections.ArrayList as an array of strings
            String[] ret = new String[v.Count];
            v.CopyTo(ret);
            return ret;
        }

        /// Splits a string into an array of strings on regular expression boundaries.
        /// This function works the same way as the Perl function of the same name.
        /// Given a regular expression of "[ab]+" and a string to split of
        /// "xyzzyababbayyzabbbab123", the result would be the array of Strings
        /// "[xyzzy, yyz, 123]".
        /// <p>Please note that the first string in the resulting array may be an empty
        /// string. This happens when the very first character of input string is
        /// matched by the pattern.</p>
        /// </summary>
        /// <param name="s">String to split on this regular exression</param>
        /// <param name="maxMatches">The maximum number of matches to split</param>
        /// <returns>Array of strings</returns>
        public String[] Split(String s, int maxMatches)
        {
            return Split(s, maxMatches, 0, s.Length);
        }

        /// Splits a string into an array of strings on regular expression boundaries.
        /// This function works the same way as the Perl function of the same name.
        /// Given a regular expression of "[ab]+" and a string to split of
        /// "xyzzyababbayyzabbbab123", the result would be the array of Strings
        /// "[xyzzy, yyz, 123]".
        /// <p>Please note that the first string in the resulting array may be an empty
        /// string. This happens when the very first character of input string is
        /// matched by the pattern.</p>
        /// </summary>
        /// <param name="s">String to split on this regular exression</param>
        /// <returns>Array of strings</returns>
        public String[] Split(String s)
        {
            return Split(s, -1, 0, s.Length);
        }

        #endregion

        #region [Replace]

        /// <summary>
        /// Substitutes a string for this regular expression in another string.
        /// This method works like the Perl function of the same name.
        /// Given a regular expression of "a*b", a String to substituteIn of
        /// "aaaabfooaaabgarplyaaabwackyb" and the substitution String "-", the
        /// resulting String returned by Substring would be "-foo-garply-wacky-".
        /// </summary>
        /// <param name="substituteIn">String to substitute within</param>
        /// <param name="substitution">String to substitute for all matches of this regular expression.</param>
        /// <returns>The string substituteIn with zero or more occurrences of the current
        /// regular expression replaced with the substitution String (if this regular
        /// expression object doesn't match at any position, the original String is returned unchanged).
        /// </returns>
        public String Replace(String substituteIn, String substitution)
        {
            return Replace(substituteIn, substitution, -1, 0);
        }

       /// <summary>
        /// Substitutes a string for this regular expression in another string.
        /// This method works like the Perl function of the same name.
        /// Given a regular expression of "a*b", a String to substituteIn of
        /// "aaaabfooaaabgarplyaaabwackyb" and the substitution String "-", the
        /// resulting String returned by subst would be "-foo-garply-wacky-".
        /// <p>
        /// It is also possible to reference the contents of a parenthesized expression
        /// with $0, $1, ... $9. A regular expression of "http://[\\.\\w\\-\\?/~_@&=%]+",
        /// a String to substituteIn of "visit us: http://www.apache.org!" and the
        /// substitution String "&lt;a href=\"$0\"&gt;$0&lt;/a&gt;", the resulting String
        /// returned by subst would be
        /// "visit us: &lt;a href=\"http://www.apache.org\"&gt;http://www.apache.org&lt;/a&gt;!".
        /// <p>
        /// <i>Note:</i> $0 represents the whole match.
        /// </summary>
        /// <param name="substituteIn">String to substitute within</param>
        /// <param name="substitution">String to substitute for matches of this regular expression</param>
        /// <param name="flags"> One or more bitwise flags from ReplaceOptions.  If the ReplaceFirstOnly
        /// flag bit is set, only the first occurrence of this regular expression is replaced.
        /// If the bit is not set (ReplaceAll), all occurrences of this pattern will be
        /// replaced. If the flag ReplaceBackrefrences is set, all backreferences will be processed.</param>
        /// <returns>The string substituteIn with zero or more occurrences of the current
        /// regular expression replaced with the substitution String (if this regular
        /// expression object doesn't match at any position, the original String is returned
        /// unchanged).
        /// </returns>
        public String Replace(String substituteIn, String substitution, int maxOccurances, int start)
        {

            int matchCount = 0;
            if (maxOccurances < 0) maxOccurances = int.MaxValue;
            // String to return
            StringBuilder ret = new StringBuilder();
            bool backRef = (program.Flags & ProgramOptions.HasBackrefrence) != ProgramOptions.HasBackrefrence;//substitution.IndexOf(CharacterClass.Dollar) != -1;

            // Start at position 0 and search the whole string
            int searchPosition = start;
            int maxLength = substituteIn.Length;

            // Try a match at each position
            while (searchPosition < maxLength && IsMatch(substituteIn, searchPosition) && matchCount < maxOccurances)
            {
                // Append string before match
                //ret.Append(StringExtensions.Range(ref substituteIn, searchPosition, start0));
                ret.Append(substituteIn.Substring(searchPosition, start0 - searchPosition));

                if (backRef)
                {

                    // Process backreferences
                    int currentPosition = 0;
                    int lastPosition = -2;
                    int length = substitution.Length;

                    while ((currentPosition = substitution.IndexOf("$", currentPosition)) >= 0)
                    {
                        if ((currentPosition == 0 || substitution[currentPosition - 1] != OpCode.Escape) && currentPosition + 1 < length)
                        {
                            char c = substitution[currentPosition + 1];
                            if (c >= '0' && c <= '9')
                            {
                                //cache lastPosition
                                lastPosition = lastPosition + 2;

                                // Append everything between the last and the current $ sign
                                ret.Append(substitution.Substring(lastPosition, currentPosition - (lastPosition)));
                                //ret.Append(StringExtensions.Range(ref substitution, lastPosition + 2, currentPosition));
                                
                                // Append the parenthesized expression, if present
                                ret.Append(Group(c - '0'));

                                // Update our LastPosition to the CurrentPosition
                                lastPosition = currentPosition;
                            }
                        }

                        // Move forward, skipping past match
                        ++currentPosition;
                    }

                    //cache lastPosition
                    lastPosition = lastPosition + 2;

                    // Append everything after the last $ sign                
                    ret.Append(substitution.Substring(lastPosition, length - lastPosition));
                    //ret.Append(StringExtensions.Range(ref substitution, (lastPosition + 2), length));
                }
                else ret.Append(substitution);                    

                // Move forward, skipping past match
                int newpos = end0;

                // We always want to make progress!
                if (newpos == searchPosition) ++newpos;

                // Try new position
                searchPosition = newpos;

                //increment the matches
                ++matchCount;

            }

            // If there's remaining input, Append it
            if (searchPosition < maxLength) ret.Append(substituteIn.Substring(searchPosition));

            // Return string buffer as string
            return ret.ToString();
        }    

        #endregion

        #region [Matches]

        /// <summary>
        /// Returns an array of Strings, whose ToString representation matches a regular
        /// expression. This method works like the Perl function of the same name.  Given
        /// a regular expression of "a*b" and an array of String objects of [foo, aab, zzz,
        /// aaaab], the array of Strings returned by grep would be [aab, aaaab].
        /// </summary>
        /// <param name="search">Array of string to search</param>
        /// <returns>Array of Strings whose value matches this regular expression</returns>
        public String[] GetMatches(string[] search)
        {
            return GetMatches(search, 0, -1);
        }

        /// <summary>
        /// Returns an array of Strings, whose ToString representation matches a regular
        /// expression. This method works like the Perl function of the same name.  Given
        /// a regular expression of "a*b" and an array of String objects of [foo, aab, zzz,
        /// aaaab], the array of Strings returned by grep would be [aab, aaaab].
        /// </summary>
        /// <param name="search">string to search</param>
        /// <returns>Array of Strings whose value matches this regular expression</returns>
        public String[] GetMatches(string search)
        {
            return GetMatches(new string[] { search }, 0, -1);
        }

        /// <summary>
        /// Returns an array of Strings, whose ToString representation matches a regular
        /// expression. This method works like the Perl function of the same name.  Given
        /// a regular expression of "a*b" and an array of String objects of [foo, aab, zzz,
        /// aaaab], the array of Strings returned by grep would be [aab, aaaab].
        /// </summary>
        /// <param name="search">Array of string to search</param>
        /// <returns>Array of Strings whose value matches this regular expression</returns>
        public String[] GetMatches(string[] search, int start, int length)
        {
            if (start < 0) start = 0;
            if (start > search.Length) return null;
            if (length < 0 || length > search.Length) length = search.Length;

            // Create new System.Collections.ArrayList to hold return items
            System.Collections.ArrayList v = new System.Collections.ArrayList();

            // Traverse array of objects
            for (int i = start, e = length; i < e; i++)
            {
                // Get next object as a string
                String s = search[i];

                // If it matches this regexp, add it to the list
                if (IsMatch(s)) v.Add(s);
            }

            // Return System.Collections.ArrayList as an array of strings
            String[] ret = new String[v.Count];
            v.ToArray(typeof(string)).CopyTo(ret, 0);
            return ret;
        }

        #endregion

        #region [Full Framework Compatibility]

        #region [TODO - Not Implemented Completely]

        /// <summary>
        /// Scans a Regex to find a Match using Full Fx Matching Style
        /// </summary>
        /// <param name="regex"></param>
        /// <param name="text"></param>
        /// <param name="textbeg"></param>
        /// <param name="textend"></param>
        /// <param name="textstart"></param>
        /// <param name="prevlen"></param>
        /// <param name="quick"></param>
        /// <returns></returns>
        internal Match Scan(Regex regex, string text, int textbeg, int textend, int textstart, int prevlen, bool quick)
        {
            //bool flag = false;
            Regex runregex = regex;
            string runtext = text;
            int runtextbeg = textstart;
            int runtextend = textend;
            int runtextstart = textstart;
            int runtextpos = textstart;
            int num = 1; //runregex.RightToLeft ? -1 : 1;
            int num2 = runtextend; // runregex.RightToLeft ? runtextbeg: runtextend;
            if (prevlen == 0)
            {
                if (runtextpos == num2)
                {
                    return System.Text.RegularExpressions.Match.Empty;
                }
                runtextpos += num;
            }
            Match runmatch = System.Text.RegularExpressions.Match.Empty;
            return runmatch;
            //while (true)
            //{
            //    if (this.FindFirstChar())
            //    {
            //        if (!flag)
            //        {
            //            this.InitMatch();
            //            flag = true;
            //        }
            //        this.Go();
            //        if (runmatch._matchcount[0] > 0)
            //        {
            //            return runmatch.TidyMatch(quick);
            //        }
            //        runtrackpos = this.runtrack.Length;
            //        runstackpos = this.runstack.Length;
            //        runcrawlpos = this.runcrawl.Length;
            //    }
            //    if (this.runtextpos == num2)
            //    {
            //        this.TidyMatch(quick);
            //        return Match.Empty;
            //    }
            //    this.runtextpos += num;
            //}
        }

        /// <summary>
        /// Adds a match to a Match
        /// </summary>
        /// <param name="capnum"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        internal void Capture(Match match, int capnum, int start, int end)
        {
            if (end < start)
            {
                int num = end;
                end = start;
                start = num;
            }
            //this.Crawl(capnum);
            match.AddMatch(capnum, start, end - start);
        }

        /// <summary>
        /// Transfers a Capture to a Match
        /// </summary>
        /// <param name="match"></param>
        /// <param name="capnum"></param>
        /// <param name="uncapnum"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        internal void TransferCapture(Match match, int capnum, int uncapnum, int start, int end)
        {
            if (end < start)
            {
                int numx = end;
                end = start;
                start = numx;
            }
            int num = match.MatchIndex(uncapnum);
            int num2 = match.MatchLength(uncapnum);
            if (start >= num2)
            {
                end = start;
                start = num2;
            }
            else if (end <= num)
            {
                start = num;
            }
            else
            {
                if (end > num2)
                {
                    end = num2;
                }
                if (num > start)
                {
                    start = num;
                }
            }
            //this.Crawl(uncapnum);
            match.BalanceMatch(uncapnum);
            if (capnum != -1)
            {
                //this.Crawl(capnum);
                match.AddMatch(capnum, start, end - start);
            }
        }
      
        #endregion

        /// <summary>
        /// Searches the specified input string for the first occurrence of the regular expression specified in the Regex constructor.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>A Match </returns>
        public Match Match(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            //return this.Run(false, -1, input, 0, input.Length, this.UseOptionR() ? input.Length : 0);            
            return Match(input, 0, input.Length);      
        }

        /// <summary>
        /// Searches the input string for the first occurrence of a regular expression, beginning at the specified starting position in the string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="startat"></param>
        /// <returns></returns>
        public Match Match(string input, int startat)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            return Match(input, startat, input.Length);    
        }

        /// <summary>
        /// Searches the specified input string for the first occurrence of the specified regular expression.
        /// </summary>
        /// <param name="input">The input string to match at</param>
        /// <param name="beginning">The place to start matching</param>
        ///<param name="length">NOT YET USED</param>
        /// <returns>The Match resulting from the input against the compiled pattern</returns>
        internal Match Match(string input, int beginning, int length)
        {            
            //Sanity checks
            if(beginning < 0 || beginning > input.Length) throw new ArgumentException("beginning");
            //Set timing flag
            //If the RegexOption is set OR the timed flag passed
            this.timed = (this.Options & RegexOptions.Timed) == RegexOptions.Timed;
            //Attempt a match
            Match result = RegularExpressions.Match.Empty;            
            if (IsMatch(input, beginning))
            {
                result = new Match(this, matchCount, input, start0, end0 - start0, beginning);
                //Must call AddMatch to ensure the start and ends are allocated to result.groups[i].caps
                for(int i = 0; i < matchCount; ++i)
                {
                    int start = GroupStart(i), end = GroupEnd(i), len = end - start;
                    result.AddMatch(i, start, len);
                }
                //Clean up the result using the members calculated when AddMatch was called
                result._index = result._matches[0][0];
                result._length = result._matches[0][1];                            
                result._capcount = result._matchcount[0];
                //Set the position of the result which is the end of the first subexpression
                //subsequent matches will proceed from this index
                result._textpos = end0;    
            }
            return result;
        }

        /// <summary>
        /// Searches the specified input string for all occurrences of a regular expression.
        /// </summary>
        /// <param name="input">The input string to match against</param>
        /// <returns>A Collection of Matches from the input</returns>
        public MatchCollection Matches(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            return new MatchCollection(this, input, input.Length, 0);
        }

        /// <summary>
        /// Searches the specified input string for all occurrences of a regular expression, beginning at the specified starting position in the string.
        /// </summary>
        /// <param name="input">The string to match against</param>
        /// <param name="startat">The position to start matching at</param>
        /// <returns>A Collection of Matches from the input</returns>
        public MatchCollection Matches(string input, int startat)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            return new MatchCollection(this, input, input.Length, startat);
        }

        /// <summary>
        /// Splits the input string at the positions defined by a specified regular expression pattern. Specified options modify the matching operation.
        /// </summary>
        /// <param name="regex">The Regex to use to Split the input string</param>
        /// <param name="input">The input string</param>
        /// <param name="count">The amount of Splits to perform</param>
        /// <param name="startat">The start index in the input</param>
        /// <returns>The result of splitting the input string against the Regexp</returns>
        public static string[] Split(Regex regex, string input, int count, int startat)
        {
            ///Sanity checks
            if (count < 0) throw new ArgumentOutOfRangeException("count");
            if (startat < 0 || startat > input.Length) throw new ArgumentOutOfRangeException("startat");
            //The result of splitting a string is... the String
            if (count == 1) return new string[] { input };
            //Get a match, if it's not successful then return the split which is the input
            Match match = regex.Match(input, startat);
            if (!match.Success) return new string[] { input };
            //Make an array 1 less then count because count is not zero based
            string[] resultsTemp = new string[--count];
            //Current count
            int current = 0;
            //if(regex.RightToLeft) Reverse String basically or just do this in reverse by doing math on startAt and inputLength
            int startIndex = 0;
        AddMatch:
            //Add the match
            resultsTemp[current] = input.Substring(startIndex, match.Index - startIndex);
            //Increment the count
            ++current;
            //Move the index
            startIndex = match.Index + match.Length;
            //Add any subsequent matches in the group
            for (int i = 1; i < match.Groups.Count; ++i) if (match.IsMatched(i))
                {
                    //Set the value of the result
                    resultsTemp[current] = match.Groups[i].ToString();
                    //Increment the count
                    ++current;
                }

            //If there can still be a match
            if (--count != 0)
            {
                //See if we can match it
                match = match.NextMatch();
                //If the match is successful do everything again
                if (match.Success) goto AddMatch;
            }
            //Add the final match and increment the count
            resultsTemp[current++] = input.Substring(startIndex, input.Length - startIndex);
            //Get the real results from the temporary array
            string[] results;
            if (current == count) return resultsTemp;
            results = new string[current];
            Array.Copy(resultsTemp, 0, results, 0, current);
            //return them
            return results;
        }

        /// <summary>
        /// Splits the input string at the positions defined by a specified regular expression pattern. Specified options modify the matching operation.
        /// </summary>
        /// <param name="pattern">The pattern to match against</param>
        /// <param name="split">The string to split</param>
        /// <param name="options">The options to utilize during matching</param>
        /// <returns>The result of splitting the input string against the pattern</returns>
        public static string[] Split(string pattern, string split, RegexOptions options)
        {
            return new Regex(pattern, options).Split(split);
        }

        /// <summary>
        /// Splits the input string at the positions defined by a specified regular expression pattern. Specified options modify the matching operation.
        /// </summary>
        /// <param name="pattern">The pattern to match against</param>
        /// <param name="options">The options to utilize during matching</param
        /// <param name="split">The string to split</param>
        /// <param name="start">the start position in the input</param>
        /// <param name="maxOccurances">the maximum amount of splits to perform</param>
        /// <returns></returns>
        public static string[] Split(string pattern, RegexOptions options, string split, int start, int maxOccurances)
        {
            return new Regex(pattern, options).Split(split, maxOccurances, start, split.Length);
        }

        /// <summary>
        /// Searches the specified input string for the first occurrence of the specified regular expression.
        /// </summary>
        /// <param name="input">The string to match against</param>
        /// <param name="pattern">The pattern to match</param>
        /// <returns>A Match representing the first match from the input against the pattern</returns>
        public static Match Match(string input, string pattern)
        {
            return new Regex(pattern, RegexOptions.None).Match(input);
        }

        /// <summary>
        /// Searches the specified input string for the first occurrence of the specified regular expression.
        /// </summary>
        /// <param name="input">The string to match against</param>
        /// <param name="pattern">The pattern to match</param>
        /// <param name="options">RegexOptions to utilize during the Match process</param>
        /// <returns>A Match representing the first match from the input against the pattern with the specified options</returns>
        public static Match Match(string input, string pattern, RegexOptions options)
        {
            return new Regex(pattern, options).Match(input);
        }

        /// <summary>
        /// Searches the specified input string for all occurrences of a regular expression.
        /// </summary>
        /// <param name="input">The string to match against</param>
        /// <param name="pattern">The pattern to match</param>
        /// <returns>A Collection of Matches from the input</returns>
        public static MatchCollection Matches(string input, string pattern)
        {
            return new Regex(pattern, 0).Matches(input);
        }

        /// <summary>
        /// Searches the specified input string for all occurrences of a regular expression, beginning at the specified starting position in the string.
        /// </summary>
        /// <param name="input">The string to match against</param>
        /// <param name="pattern">The pattern to match</param>
        /// <param name="options">RegexOptions to utilize during the Match process</param>
        /// <returns>A Collection of Matches from the input</returns>
        public static MatchCollection Matches(string input, string pattern, RegexOptions options)
        {
            return new Regex(pattern, options).Matches(input);
        }

        /// <summary>
        /// Escapes a minimal set of characters (\, *, +, ?, |, {, [, (,), ^, $,., #, and white space) by replacing them with their escape codes. 
        /// This instructs the regular expression engine to interpret these characters literally rather than as metacharacters.
        /// </summary>
        /// <param name="input">The input string to escape</param>
        /// <returns>The escaped input string</returns>
        internal static string InternalEscape(ref string input)
        {
            for (int i = 0, e = input.Length; i < e; ++i)
            {
                char currentChar = input[i];
                if (!CharacterClass.IsMetachar(ref currentChar)) continue;
                StringBuilder builder = new StringBuilder();
                char ch = input[i];
                builder.Append(input, 0, i);
                do
                {
                    builder.Append(OpCode.Escape);
                    switch (ch)
                    {
                        case CharacterClass.Tab:
                            ch = 't';
                            break;

                        case CharacterClass.NewLine:
                            ch = 'n';
                            break;

                        case '\f':
                            ch = 'f';
                            break;

                        case CharacterClass.LineReturn:
                            ch = 'r';
                            break;
                    }
                    builder.Append(ch);
                    ++i;
                    int startIndex = i;
                    while (i < input.Length)
                    {
                        ch = input[i];
                        if (CharacterClass.IsMetachar(ref ch)) break;
                        ++i;
                    }
                    builder.Append(input, startIndex, i - startIndex);
                }
                while (i < e);
                return builder.ToString();
            }
            return input;
        }

        /// <summary>
        /// Escapes a minimal set of characters (\, *, +, ?, |, {, [, (,), ^, $,., #, and white space) by replacing them with their escape codes. 
        /// This instructs the regular expression engine to interpret these characters literally rather than as metacharacters.
        /// </summary>
        /// <param name="input">The input string to escape</param>
        /// <returns>The escaped input string</returns>
        public static string Escape(string input)
        {
            return InternalEscape(ref input);
        }

        #endregion

        #region [Object.Overrides]

        /// <summary>
        /// Provides a string representation of the pattern this Regular Expression is matching
        /// </summary>
        /// <returns>The Pattern of this Regular Expression</returns>
        public override string ToString()
        {
            return ToFullRegularExpression(program.pattern);
            //return Program.Pattern;
        }

        #endregion

        #endregion      
    }
}
