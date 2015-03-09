using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Text.RegularExpressions;

namespace TextTests
{
    class RegexpOptionsTest : IMFTestInterface
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
        public MFTestResults RegExpTest_4_RegexpOptions_Test_1_IgnoreCase()
        {
            bool testResult = false;

            Regex regex;

            try
            {
                Log.Comment(" Test RegexOptions.IgnoreCase");
                regex = new Regex("abc(\\w*)");
                Log.Comment("RegexOptions.IgnoreCase abc(\\w*)");
                regex.Options = RegexOptions.IgnoreCase;
                Log.Comment("abc(d*)");
                if (!regex.IsMatch("abcddd"))
                {
                    Log.Comment("Did not match 'abcddd'.");
                    testResult = false;
                }
                else
                {
                    Log.Comment("abcddd = true");
                    TestTestsHelper.ShowParens(ref regex);
                    testResult = true;
                }

                if (!regex.IsMatch("aBcDDdd"))
                {
                    Log.Comment("Did not match 'aBcDDdd'.");
                    testResult = false;
                }
                else
                {
                    Log.Comment("aBcDDdd = true");
                    TestTestsHelper.ShowParens(ref regex);
                    testResult = true;
                }

                if (!regex.IsMatch("ABCDDDDD"))
                {
                    Log.Comment("Did not match 'ABCDDDDD'.");
                    testResult = false;
                }
                else
                {
                    Log.Comment("ABCDDDDD = true");
                    TestTestsHelper.ShowParens(ref regex);
                    testResult = true;
                }

                regex = new Regex("(A*)b\\1");
                regex.Options = RegexOptions.IgnoreCase;
                if (!regex.IsMatch("AaAaaaBAAAAAA"))
                {
                    Log.Comment("Did not match 'AaAaaaBAAAAAA'.");
                    testResult = false;
                }
                else
                {
                    Log.Comment("AaAaaaBAAAAAA = true");
                    TestTestsHelper.ShowParens(ref regex);
                    testResult = true;
                }

                regex = new Regex("[A-Z]*");
                regex.Options = RegexOptions.IgnoreCase;
                if (!regex.IsMatch("CaBgDe12"))
                {
                    Log.Comment("Did not match 'CaBgDe12'.");
                    testResult = false;
                }
                else
                {
                    Log.Comment("CaBgDe12 = true");
                    TestTestsHelper.ShowParens(ref regex);
                    testResult = true;
                }

            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                testResult = false;
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults RegExpTest_4_RegexpOptions_Test_2_EOL_BOL()
        {
            bool testResult = true;

            Regex regex;

            try
            {
                Log.Comment("Test for eol/bol symbols.");
                regex = new Regex("^abc$");
                if (regex.IsMatch("\nabc"))
                {
                    Log.Comment("\"\\nabc\" matches \"^abc$\"");
                    testResult = false;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                testResult = false;
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults RegExpTest_4_RegexpOptions_Test_3_MultiLine()
        {
            bool testResult = true;

            Regex regex;

            try
            {
                Log.Comment("Test RE.MATCH_MULTILINE. Test for eol/bol symbols.");
                regex = new Regex("^abc$", RegexOptions.Multiline);
                if (!regex.IsMatch("\nabc"))
                {
                    Log.Comment("\"\\nabc\" doesn't match \"^abc$\"");
                    testResult = false;
                }
                if (!regex.IsMatch("\rabc"))
                {
                    Log.Comment("\"\\rabc\" doesn't match \"^abc$\"");
                    testResult = false;
                }
                if (!regex.IsMatch("\r\nabc"))
                {
                    Log.Comment("\"\\r\\nabc\" doesn't match \"^abc$\"");
                    testResult = false;
                }
                if (!regex.IsMatch("\u0085abc"))
                {
                    Log.Comment("\"\\u0085abc\" doesn't match \"^abc$\"");
                    testResult = false;
                }
                if (!regex.IsMatch("\u2028abc"))
                {
                    Log.Comment("\"\\u2028abc\" doesn't match \"^abc$\"");
                    testResult = false;
                }
                if (!regex.IsMatch("\u2029abc"))
                {
                    Log.Comment("\"\\u2029abc\" doesn't match \"^abc$\"");
                    testResult = false;
                }

                Log.Comment("Test RE.MATCH_MULTILINE. Test that '.' does not matches new line.");
                regex = new Regex("^a.*b$", RegexOptions.Multiline);
                if (regex.IsMatch("a\nb"))
                {
                    Log.Comment("\"a\\nb\" matches \"^a.*b$\"");
                    testResult = false;
                }
                if (regex.IsMatch("a\rb"))
                {
                    Log.Comment("\"a\\rb\" matches \"^a.*b$\"");
                    testResult = false;
                }
                if (regex.IsMatch("a\r\nb"))
                {
                    Log.Comment("\"a\\r\\nb\" matches \"^a.*b$\"");
                    testResult = false;
                }
                if (regex.IsMatch("a\u0085b"))
                {
                    Log.Comment("\"a\\u0085b\" matches \"^a.*b$\"");
                    testResult = false;
                }
                if (regex.IsMatch("a\u2028b"))
                {
                    Log.Comment("\"a\\u2028b\" matches \"^a.*b$\"");
                    testResult = false;
                }
                if (regex.IsMatch("a\u2029b"))
                {
                    Log.Comment("\"a\\u2029b\" matches \"^a.*b$\"");
                    testResult = false;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                testResult = false;
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults RegExpTest_4_RegexpOptions_Test_4_LargeProgram()
        {
#if DEBUG
            bool testResult = false;

            // Bug 38331: Large program
            try
            {
                RegexDebugCompiler c = new RegexDebugCompiler();
                c.Compile("(a{8192})?");                
                Log.Comment("(a{8192})? should FAIL to compile.");
                c.dumpProgram();
                testResult = false;
            }
            catch (RegexpSyntaxException)
            {
                Log.Comment("expected failure");
                testResult = true;
            }
                       

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
#else
            return MFTestResults.Skip;
#endif
        }
    }
}
