using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Text.RegularExpressions;

namespace TextTests
{
    class Split_Grep_Tests : IMFTestInterface
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
        public MFTestResults RegExpTest_1_Split_Test_0()
        {
            bool testResult = false;

            string[] expectedResults;

            string[] acutalResults;

            Regex regex;

            try
            {
                expectedResults = new string[]{ "xyzzy", "yyz", "123" };
                regex = new Regex("[ab]+");
                acutalResults = regex.Split("xyzzyababbayyzabbbab123");
                TestTestsHelper.AssertEquals(ref expectedResults, ref acutalResults, out testResult);

                expectedResults = new string[] { "xxxx", "xxxx", "yyyy", "zzz" };
                regex = new Regex("a*b");//match any amount of 'a' and 1 'b'
                acutalResults = regex.Split("xxxxaabxxxxbyyyyaaabzzz");
                TestTestsHelper.AssertEquals(ref expectedResults, ref acutalResults, out testResult);                

                // Grep Tests

                return RegExpTest_2_Grep_Test_0(ref acutalResults);

            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                testResult = false;
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// This test is entangled with RegExpTest_1_Split_Test_0
        /// Should the Split Logic Break the Grep Logic will break also.
        /// </summary>
        /// <param name="arg">The input to match against using grep</param>
        /// <returns></returns>
        internal MFTestResults RegExpTest_2_Grep_Test_0(ref string[] arg)
        {
            bool testResult = false;

            string[] expectedResults;

            string[] acutalResults;

            Regex regex;
            try
            {
                regex = new Regex("x+");
                expectedResults = new String[] { "xxxx", "xxxx" };
                acutalResults = regex.GetMatches(arg);

                int al = acutalResults.Length;
                int el = expectedResults.Length;

                for (int i = 0; i < el; i++)
                {
                    Log.Comment("Actual[" + i + "] = " + acutalResults[i]);
                    testResult = TestTestsHelper.AssertEquals(ref TestTestsHelper.GrepFailString, ref expectedResults[i], ref acutalResults[i]);
                }
                
                testResult = TestTestsHelper.AssertEquals(ref TestTestsHelper.WrongNumberGrep, ref el, ref el);
            }
            catch(Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                testResult = false;
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
            
        }
    }
}
