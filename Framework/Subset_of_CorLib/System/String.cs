////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{
    using System;
    using System.Threading;
    using System.Collections;
    using System.Runtime.CompilerServices;
    /**
     * <p>The <code>String</code> class represents a static string of characters.  Many of
     * the <code>String</code> methods perform some type of transformation on the current
     * instance and return the result as a new <code>String</code>. All comparison methods are
     * implemented as a part of <code>String</code>.</p>  As with arrays, character positions
     * (indices) are zero-based.
     *
     * <p>When passing a null string into a constructor in VJ and VC, the null should be
     * explicitly type cast to a <code>String</code>.</p>
     * <p>For Example:<br>
     * <pre>String s = new String((String)null);
     * Text.Out.WriteLine(s);</pre></p>
     *
     * @author Jay Roxe (jroxe)
     * @version
     */
    [Serializable]
    public sealed class String : IComparable
    {
        public static readonly String Empty = "";
        public override bool Equals(object obj)
        {
            String s = obj as String;
            if (s != null)
            {
                return String.Equals(this, s);
            }

            return false;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static bool Equals(String a, String b);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static bool operator ==(String a, String b);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static bool operator !=(String a, String b);

        [System.Runtime.CompilerServices.IndexerName("Chars")]
        public extern char this[int index]
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern char[] ToCharArray();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern char[] ToCharArray(int startIndex, int length);

        public extern int Length
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern String[] Split(params char[] separator);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern String[] Split(char[] separator, int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern String Substring(int startIndex);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern String Substring(int startIndex, int length);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern String Trim(params char[] trimChars);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern String TrimStart(params char[] trimChars);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern String TrimEnd(params char[] trimChars);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern String(char[] value, int startIndex, int length);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern String(char[] value);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern String(char c, int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static int Compare(String strA, String strB);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int CompareTo(Object value);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int CompareTo(String strB);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int IndexOf(char value);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int IndexOf(char value, int startIndex);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int IndexOf(char value, int startIndex, int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int IndexOfAny(char[] anyOf);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int IndexOfAny(char[] anyOf, int startIndex);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int IndexOfAny(char[] anyOf, int startIndex, int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int IndexOf(String value);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int IndexOf(String value, int startIndex);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int IndexOf(String value, int startIndex, int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int LastIndexOf(char value);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int LastIndexOf(char value, int startIndex);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int LastIndexOf(char value, int startIndex, int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int LastIndexOfAny(char[] anyOf);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int LastIndexOfAny(char[] anyOf, int startIndex);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int LastIndexOfAny(char[] anyOf, int startIndex, int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int LastIndexOf(String value);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int LastIndexOf(String value, int startIndex);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int LastIndexOf(String value, int startIndex, int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern String ToLower();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern String ToUpper();

        public override String ToString()
        {
            return this;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern String Trim();
        ////// This method contains the same functionality as StringBuilder Replace. The only difference is that
        ////// a new String has to be allocated since Strings are immutable
        public static String Concat(Object arg0)
        {
            if (arg0 == null)
            {
                return String.Empty;
            }

            return arg0.ToString();
        }

        public static String Concat(Object arg0, Object arg1)
        {
            if (arg0 == null)
            {
                arg0 = String.Empty;
            }

            if (arg1 == null)
            {
                arg1 = String.Empty;
            }

            return Concat(arg0.ToString(), arg1.ToString());
        }

        public static String Concat(Object arg0, Object arg1, Object arg2)
        {
            if (arg0 == null)
            {
                arg0 = String.Empty;
            }

            if (arg1 == null)
            {
                arg1 = String.Empty;
            }

            if (arg2 == null)
            {
                arg2 = String.Empty;
            }

            return Concat(arg0.ToString(), arg1.ToString(), arg2.ToString());
        }

        public static String Concat(params Object[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }

            int length = args.Length;
            String[] sArgs = new String[length];

            for (int i = 0; i < length; i++)
            {
                sArgs[i] = ((args[i] == null) ? (String.Empty) : (args[i].ToString()));
            }

            return String.Concat(sArgs);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static String Concat(String str0, String str1);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static String Concat(String str0, String str1, String str2);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static String Concat(String str0, String str1, String str2, String str3);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static String Concat(params String[] values);

        public static String Intern(String str)
        {
            // We don't support "interning" of strings. So simply return the string.
            return str;
        }

        public static String IsInterned(String str)
        {
            // We don't support "interning" of strings. So simply return the string.
            return str;
        }

    }
}


