using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Text.RegularExpressions;

namespace TextTests
{
    class SubstringTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
        }

        [TestMethod]
        public MFTestResults RegExpTest_3_Substring_Test_0()
        {
            bool testResult = false;

            String expected, actual;

            Regex regex;

            string message;

            try
            {
                Log.Comment("Test subst()");
                regex = new Regex("a*b");
                expected = "-foo-garply-wacky-";
                actual = regex.Replace("aaaabfooaaabgarplyaaabwackyb", "-");
                message = "Wrong result of substitution in\"a*b\"";
                testResult = TestTestsHelper.AssertEquals(ref message, ref expected, ref actual);

                Log.Comment("Test subst() with backreferences");
                regex = new Regex("http://[.\\w?/~_@&=%]+");
                expected = "visit us: 1234<a href=\"http://www.apache.org\">http://www.apache.org</a>!";
                actual = regex.Replace("visit us: http://www.apache.org!", "1234<a href=\"$0\">$0</a>");
                message = "Wrong subst() result";
                testResult = TestTestsHelper.AssertEquals(ref message, ref expected, ref actual);

                Log.Comment("Test subst() with backreferences without leading characters before first backreference");
                regex = new Regex("(.*?)=(.*)");
                expected = "variable_test_value12";
                actual = regex.Replace("variable=value", "$1_test_$212");
                message = "Wrong subst() result";
                testResult = TestTestsHelper.AssertEquals(ref message, ref expected, ref actual);

                Log.Comment("Test subst() with NO backreferences");
                regex = new Regex("^a$");
                expected = "b";
                actual = regex.Replace("a", "b");
                message = "Wrong subst() result";
                testResult = TestTestsHelper.AssertEquals(ref message, ref expected, ref actual);

                Log.Comment(" Test subst() with NO backreferences");
                regex = new Regex("^a$", RegexOptions.Multiline);
                expected = "\r\nb\r\n";
                actual = regex.Replace("\r\na\r\n", "b");
                testResult = TestTestsHelper.AssertEquals(ref message, ref expected, ref actual);

                Log.Comment(" Test for Bug #36106 ");
                regex = new Regex("fo(o)");
                actual = regex.Replace("foo", "$1");
                expected = "o";
                testResult = TestTestsHelper.AssertEquals(ref message, ref expected, ref actual);

                Log.Comment(" Test for Bug #36405 ");
                regex = new Regex("^(.*?)(x)?$");
                actual = regex.Replace("abc", "$1-$2");
                expected = "abc-";
                testResult = TestTestsHelper.AssertEquals(ref message, ref expected, ref actual);

                regex = new Regex("^(.*?)(x)?$");
                actual = regex.Replace("abcx", "$1-$2");
                expected = "abc-x";
                testResult = TestTestsHelper.AssertEquals(ref message, ref expected, ref actual);

                regex = new Regex("([a-b]+?)([c-d]+)");
                actual = regex.Replace("zzabcdzz", "$1-$2");
                expected = "zzab-cdzz";
                testResult = TestTestsHelper.AssertEquals(ref message, ref expected, ref actual);

            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                testResult = false;
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }     

    }
}
