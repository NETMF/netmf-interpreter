using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Text.RegularExpressions;

namespace TextTests
{
    internal static class TestTestsHelper
    {
        /// <summary>
        ///  Dump parenthesized subexpressions found by a regular expression matcher object         
        /// </summary>
        /// <param name="r">Regex object with results to show</param>
        internal static void ShowParens(ref Regex r)
        {
            // Loop through each paren
            for (int i = 0, e = r.matchCount; i < e; i++)
            {
                // Show paren register
                Log.Comment("$" + i + " = " + r.Group(i));
            }
        }

        internal static bool AssertEquals(ref String message, ref String expected, ref String actual)
        {
            if (expected != null && !expected.Equals(actual)
                || actual != null && !actual.Equals(expected))
            {
                Log.Comment(message + " (expected \"" + expected + "\", actual \"" + actual + "\")");
                return false;
            }
            return true;
        }

        internal static bool AssertEquals(ref String message, ref int expected, ref int actual)
        {
            if (expected != actual)
            {
                Log.Comment(message + " (expected \"" + expected + "\", actual \"" + actual + "\")");
                return false;
            }
            return true;
        }

        internal static string NotExpected = "Wrong splitted part";
        internal static string WrongNumerSplit = "Wrong number of splitted parts";
        internal static string WrongNumberGrep = "Wrong number of String found by grep";
        internal static string GrepFailString = "Grep fails";

        internal static void AssertEquals(ref string[] expected, ref string[] actual, out bool result)
        {
            int el = expected.Length;
            int al = actual.Length;
            for (int i = 0; i < el && i < al; i++) if (!(result = AssertEquals(ref NotExpected, ref expected[i], ref actual[i]))) return;
            result = AssertEquals(ref WrongNumerSplit, ref el, ref al);
        }

    }
}
