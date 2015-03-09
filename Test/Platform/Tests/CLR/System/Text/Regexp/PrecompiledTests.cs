using System;
using System.Text.RegularExpressions;
using Microsoft.SPOT.Platform.Test;
namespace TextTests
{
    class PrecompiledTests : IMFTestInterface
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
        public MFTestResults RegExpTest_0_PrecompiledMatch()
        {
            bool testResult = false;

            try
            {
                Log.Comment("Pre-compiled regular expression \"a*b\"");
                char[] re1instructions =
                {
                    (char)0x007c, (char)0x0000, (char)0x001a, (char)0x007c, (char)0x0000, (char)0x000d, (char)0x0041,
                    (char)0x0001, (char)0x0004, (char)0x0061, (char)0x007c, (char)0x0000, (char)0x0003, (char)0x0047,
                    (char)0x0000, (char)0xfff6, (char)0x007c, (char)0x0000, (char)0x0003, (char)0x004e, (char)0x0000,
                    (char)0x0003, (char)0x0041, (char)0x0001, (char)0x0004, (char)0x0062, (char)0x0045, (char)0x0000,
                    (char)0x0000,
                };

                //need to make internals visible to
                RegexProgram re1 = new RegexProgram(re1instructions);                

                // Simple test of pre-compiled regular expressions
                Regex r = new Regex(re1);

                Log.Comment("Matching Precompiled Expression a*b");                
                testResult = r.IsMatch("aaab");
                Log.Comment("aaab = " + testResult);
                TestTestsHelper.ShowParens(ref r);
                if (!testResult)
                {
                    Log.Comment("\"aaab\" doesn't match to precompiled \"a*b\"");                    
                }

                testResult = r.IsMatch("b");
                Log.Comment("b = " + testResult);
                TestTestsHelper.ShowParens(ref r);
                if (!testResult)
                {
                    Log.Comment("\"b\" doesn't match to precompiled \"a*b\"");
                }

                testResult = r.IsMatch("c");
                Log.Comment("c = " + testResult);
                TestTestsHelper.ShowParens(ref r);
                if (testResult)
                {
                    Log.Comment("\"c\" matches to precompiled \"a*b\"");
                    testResult = false;
                }

                testResult = r.IsMatch("ccccaaaaab");
                Log.Comment("ccccaaaaab = " + testResult);
                TestTestsHelper.ShowParens(ref r);
                if (!testResult)
                {
                    Log.Comment("\"ccccaaaaab\" doesn't match to precompiled \"a*b\"");
                }

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
