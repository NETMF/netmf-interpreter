using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Text.RegularExpressions;

namespace TextTests
{
    class CaptureTests : IMFTestInterface
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
        public MFTestResults RegExpTest_5_Capture_Test_0()
        {
            /*
            // The example displays the following output:
            //       Match: Yes.
            //          Group 0: Yes.
            //             Capture 0: Yes.
            //          Group 1: Yes.
            //             Capture 0: Yes.
            //          Group 2: Yes
            //             Capture 0: Yes
            //       Match: This dog is very friendly.
            //          Group 0: This dog is very friendly.
            //             Capture 0: This dog is very friendly.
            //          Group 1: friendly.
            //             Capture 0: This
            //             Capture 1: dog
            //             Capture 2: is
            //             Capture 3: very
            //             Capture 4: friendly.
            //          Group 2: friendly
            //             Capture 0: This
            //             Capture 1: dog
            //             Capture 2: is
            //             Capture 3: very
            //             Capture 4: friendly
             */
            string input = "Yes. This dog is very friendly.";
            string pattern = @"((\w+)[\s.])+";
            //Regex test = new Regex(pattern);
            //string group = test.Group(0);


            MatchCollection results = Regex.Matches(input, pattern);

            foreach (Match match in results)
            {
                Log.Comment("Match: "+ match.Value);
                for (int groupCtr = 0; groupCtr < match.Groups.Count; groupCtr++)
                {
                    Group group = match.Groups[groupCtr];
                    Log.Comment("   Group "+groupCtr+": "+group.Value+"");
                    for (int captureCtr = 0; captureCtr < group.Captures.Count; captureCtr++)
                        Log.Comment("      Capture "+captureCtr+": "+group.Captures[captureCtr].Value);
                }
            }

            //This test will not return the same as the Example from MSDN until TransferCapture is used from the Regex to the Match... I am working on this but it is not ready yet

            return results.Count == 2 && results[0].Groups.Count == 3 && results[1].Groups.Count == 3  ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults RegExpTest_5_Catpure_Test_1_CaptureCollection()
        {

            /*
            // The example displays the following output:
            //    Pattern: \b\w+\W{1,2}
            //    Match: The
            //      Match.Captures: 1
            //        0: 'The '
            //      Match.Groups: 1
            //        Group 0: 'The '
            //        Group(0).Captures: 1
            //          Capture 0: 'The '           
             */

            try
            {
                bool result = true;
                int expectedCaptures = 1;
                int expectedGroups = 1;
                int Group0ExpectedCount = 1;
                
                string pattern;
                string input = "The young, hairy, and tall dog slowly walked across the yard.";
                Match match;

                // Match a word with a pattern that has no capturing groups.
                pattern = @"\b\w+\W{1,2}";
                match = Regex.Match(input, pattern);
                Log.Comment("Pattern: " + pattern);
                Log.Comment("Match: " + match.Value);
                Log.Comment("  Match.Captures: " + match.Captures.Count);
                for (int ctr = 0; ctr < match.Captures.Count; ctr++) Log.Comment("   " + ctr + ": '" + match.Captures[ctr].Value + "'");
                Log.Comment("  Match.Groups: " + match.Groups.Count);
                for (int groupCtr = 0; groupCtr < match.Groups.Count; groupCtr++)
                {
                    Log.Comment("    Group " + groupCtr + ": '" + match.Groups[groupCtr].Value + "'");
                    Log.Comment("    Group(" + groupCtr + ").Captures: " + match.Groups[groupCtr].Captures.Count);
                    for (int captureCtr = 0; captureCtr < match.Groups[groupCtr].Captures.Count; captureCtr++) Log.Comment("      Capture " + captureCtr + ": '" + match.Groups[groupCtr].Captures[captureCtr].Value + "'");
                }

                //Verify Results

                if (match.Captures.Count != expectedCaptures)
                {
                    result = false;
                }

                if (match.Captures[0].ToString() != "The ")
                {
                    result = false;
                }

                if (match.Groups.Count != expectedGroups)
                {
                    result = false;
                }

                if (match.Groups[0].Captures.Count != Group0ExpectedCount)
                {
                    result = false;
                }


                if (match.Groups[0].ToString() != "The ")
                {
                    result = false;
                }

                if (match.Groups[0].Captures[0].ToString() != "The ")
                {
                    result = false;
                }

                return result ? MFTestResults.Pass : MFTestResults.Fail;
            }
            catch
            {
                return MFTestResults.Fail;
            }
        }

        [TestMethod]
        public MFTestResults RegExpTest_5_Catpure_Test_2_CaptureCollection()
        {

            /*
            // The example displays the following output:
            //    Pattern: (\b\w+\W{1,2})+
            //    Match: The young, hairy, and tall dog slowly walked across the yard.
            //      Match.Captures: 1
            //        0: 'The young, hairy, and tall dog slowly walked across the yard.'
            //      Match.Groups: 2
            //        Group 0: 'The young, hairy, and tall dog slowly walked across the yard.'
            //        Group(0).Captures: 1
            //          Capture 0: 'The young, hairy, and tall dog slowly walked across the yard.'
            //        Group 1: 'yard.'
            //        Group(1).Captures: 11
            //          Capture 0: 'The '
            //          Capture 1: 'young, '
            //          Capture 2: 'hairy, '
            //          Capture 3: 'and '
            //          Capture 4: 'tall '
            //          Capture 5: 'dog '
            //          Capture 6: 'slowly '
            //          Capture 7: 'walked '
            //          Capture 8: 'across '
            //          Capture 9: 'the '
            //          Capture 10: 'yard.'
             */

            try
            {
                bool result = true;
                int expectedCaptures = 1;
                int expectedGroups = 1;

                string pattern;
                string input = "The young, hairy, and tall dog slowly walked across the yard.";
                Match match;

                result = true;
                expectedCaptures = 1;
                expectedGroups = 2;

                // Match a sentence with a pattern that has a quantifier that 
                // applies to the entire group.
                pattern = @"(\b\w+\W{1,2})+";
                match = Regex.Match(input, pattern);
                Log.Comment("Pattern: " + pattern);
                Log.Comment("Match: " + match.Value);
                Log.Comment("  Match.Captures: " + match.Captures.Count);
                for (int ctr = 0; ctr < match.Captures.Count; ctr++) Log.Comment("    " + ctr + ": '" + match.Captures[ctr].Value + "'");
                Log.Comment("  Match.Groups: " + match.Groups.Count);
                for (int groupCtr = 0; groupCtr < match.Groups.Count; groupCtr++)
                {
                    Log.Comment("    Group " + groupCtr + ": '" + match.Groups[groupCtr].Value + "'");
                    Log.Comment("    Group(" + groupCtr + ").Captures: " + match.Groups[groupCtr].Captures.Count);
                    for (int captureCtr = 0; captureCtr < match.Groups[groupCtr].Captures.Count; captureCtr++) Log.Comment("      Capture " + captureCtr + ": '" + match.Groups[groupCtr].Captures[captureCtr].Value + "'");
                }

                //Verify Results

                if (match.Captures.Count != expectedCaptures)
                {
                    result = false;
                }

                if (match.Groups.Count != expectedGroups)
                {
                    result = false;
                }

                if (match.Groups[0].ToString() != "The young, hairy, and tall dog slowly walked across the yard.")
                {
                    result = false;
                }

                if (match.Captures[0].ToString() != "The young, hairy, and tall dog slowly walked across the yard.")
                {
                    result = false;
                }

                if (match.Groups[1].ToString() != "yard.")
                {
                    result = false;
                }

                return result ? MFTestResults.Pass : MFTestResults.Fail;
            }
            catch
            {
                return MFTestResults.Fail;
            }
        }
    }
}
