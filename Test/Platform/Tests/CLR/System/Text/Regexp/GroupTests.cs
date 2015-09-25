using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Text.RegularExpressions;

namespace TextTests
{
    class GroupTests : IMFTestInterface
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
        public MFTestResults RegExpTest_6_Group_Test_0()
        {
            /*
            // The example displays the following output:
            //       Match: This is one sentence.
            //          Group 1: 'This is one sentence.'
            //             Capture 1: 'This is one sentence.'
            //          Group 2: 'sentence'
            //             Capture 1: 'This '
            //             Capture 2: 'is '
            //             Capture 3: 'one '
            //             Capture 4: 'sentence'
            //          Group 3: 'sentence'
            //             Capture 1: 'This'
            //             Capture 2: 'is'
            //             Capture 3: 'one'
            //             Capture 4: 'sentence'
             */
            string pattern = @"(\b(\w+?)[,:;]?\s?)+[?.!]";
            string input = "This is one sentence. This is a second sentence.";

            Match match = Regex.Match(input, pattern);
            Log.Comment("Match: " + match.Value);
            int groupCtr = 0;
            foreach (Group group in match.Groups)
            {
                groupCtr++;
                Log.Comment("   Group "+groupCtr+": '"+group.Value+"'");
                int captureCtr = 0;
                foreach (Capture capture in group.Captures)
                {
                    captureCtr++;
                    Log.Comment("      Capture "+captureCtr+": '"+capture.Value+"'");
                }
            }

            return match.Groups.Count == 3 && match.Groups[0].ToString() == "This is one sentence." && match.Groups[1].ToString() == "sentence" && match.Groups[2].ToString() == "sentence" ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults RegExpTest_6_Group_Test_1_GroupCollection()
        {
            /*
            // The example displays the following output:
            //       ®: Microsoft
            //       ®: Excel
            //       ®: Access
            //       ®: Outlook
            //       ®: PowerPoint
            //       ™: Silverlight
            // Found 6 trademarks or registered trademarks.
            */
            bool result = true;
            int expectedCount = 6;

            string pattern = @"\b(\w+?)([®™])";
            string input = "Microsoft® Office Professional Edition combines several office " +
                           "productivity products, including Word, Excel®, Access®, Outlook®, " +
                           "PowerPoint®, and several others. Some guidelines for creating " +
                           "corporate documents using these productivity tools are available " +
                           "from the documents created using Silverlight™ on the corporate " +
                           "intranet site.";
            Regex test = new Regex(pattern);
            MatchCollection matches = test.Matches(input);
            foreach (Match match in matches)
            {
                GroupCollection groups = match.Groups;
                Log.Comment(groups[2] + ": " + groups[1]);
            }
            
            Log.Comment("Found "+matches.Count+" trademarks or registered trademarks.");

            if (matches.Count != expectedCount)
            {
                result = false;
            }

            if (matches[0].ToString() != "Microsoft®")
            {
                result = false;
            }
            else if (matches[1].ToString() != "Excel®")
            {
                result = false;
            }
            else if (matches[2].ToString() != "Access®")
            {
                result = false;
            }
            else if (matches[3].ToString() != "Outlook®")
            {
                result = false;
            }
            else if (matches[4].ToString() != "PowerPoint®")
            {
                result = false;
            }
            else if (matches[5].ToString() != "Silverlight™")
            {
                result = false;
            }

            return result ? MFTestResults.Pass : MFTestResults.Fail;
        }
    }
}
