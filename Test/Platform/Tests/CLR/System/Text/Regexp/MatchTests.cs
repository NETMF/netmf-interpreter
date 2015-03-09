using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Text.RegularExpressions;

namespace TextTests.RegExTests
{
    class MatchTests : IMFTestInterface
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
        public MFTestResults RegExpTest_7_Match_Test_0()
        {
            //MSDN Test from http://msdn.microsoft.com/en-us/library/system.text.regularexpressions.match.aspx
            string input = "int[] values = { 1, 2, 3 };\n" +
                    "for (int ctr = values.GetLowerBound(1); ctr <= values.GetUpperBound(1); ctr++)\n" +
                    "{\n" +
                    "   Console.Write(values[ctr]);\n" +
                    "   if (ctr < values.GetUpperBound(1))\n" +
                    "      Console.Write(\", \");\n" +
                    "}\n" +
                    "Console.WriteLine();\n";
            string pattern = "Console.Write(Line)?";
            Match match = Regex.Match(input, pattern);
            int matchCount = 0;
            string expected0 = null;
            string expected1 = null;
            while (match.Success)
            {
                Log.Comment("'" + match.Value + "' found in the source code at position " + match.Index + ".");
                if (matchCount == 0) expected0 = match.ToString();
                else if (matchCount == 2) expected1 = match.ToString();
                match = match.NextMatch();
                ++matchCount;
            }            
            return matchCount == 3 && expected0 == "Console.Write" && expected1 == "Console.WriteLine" ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults RegExpTest_7_Match_Test_01()
        {
            //MSDN Test from http://msdn.microsoft.com/en-us/library/system.text.regularexpressions.match.aspx
            // Search for a pattern that is not found in the input string.
            string pattern = "dog";
            string input = "The cat saw the other cats playing in the back yard.";
            Match match = Regex.Match(input, pattern);
            if (match.Success)
                // Report position as a one-based integer.
                Log.Comment("'" + match.Value + "' was found at position " + match.Index + 1 + " in '" + input + "'.");
            else
                Log.Comment("The pattern '" + pattern + "' was not found in '{" + input + "}'.");
            return !match.Success ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults RegExpTest_8_MatchCollection_Test_1()
        {

            /* http://msdn.microsoft.com/en-us/library/system.text.regularexpressions.matchcollection.aspx
            // The example produces the following output to the console:
            //       3 matches found in:
            //          The the quick brown fox  fox jumped over the lazy dog dog.
            //       'The' repeated at positions 0 and 4
            //       'fox' repeated at positions 20 and 25
            //       'dog' repeated at positions 50 and 54
            */
            try
            {
                // Define a regular expression for repeated words.
                Regex rx = new Regex(@"\b(?<word>\w+)\s+(\k<word>)\b",
                  RegexOptions.Compiled | RegexOptions.IgnoreCase);

                // Define a test string.        
                string text = "The the quick brown fox  fox jumped over the lazy dog dog.";

                // Find matches.
                MatchCollection matches = rx.Matches(text);

                // Report the number of matches found.
                Log.Comment(matches.Count + " matches found in:\n  " + text);

                // Report on each match.
                foreach (Match match in matches)
                {
                    GroupCollection groups = match.Groups;
                    Log.Comment("'" + groups["word"].Value + "'" + " repeated at positions " + groups[0].Index + " and " + groups[1].Index);
                }

                //Required Named Capture
                return MFTestResults.Pass;
            }
            catch
            {
                //Known to Fail until Named Capture is implemented
                //See http://www.regular-expressions.info/named.html
                return MFTestResults.KnownFailure;
            }
        }

        [TestMethod]
        public MFTestResults RegExpTest_8_MatchCollection_Test_2()
        {
            //Attempting to show you can get the same results without the Named Caputure using a different Group Ordering... needs some work
            //Eg should ouput the same as Test_1 with the same results without named capture but with a modified pattern
            /*
            // The example produces the following output to the console:
            //       3 matches found in:
            //          The the quick brown fox  fox jumped over the lazy dog dog.
            //       'The' repeated at positions 0 and 4
            //       'fox' repeated at positions 20 and 25
            //       'dog' repeated at positions 50 and 54
            */
            try
            {
                // Define a regular expression for repeated words.
                //Was trying to add ?: to the second group to force its presence atleast once but you cant use back references when you do this with this engine, there must be another way!
                //Has correct Index's but extra matches!!!
                //@"\b(?:\w+)\s+(\1)\b"
                //pattern = @"\b(?:\w+)\s+((\s+(\1))\b)";

                ////Only 1 Match 
                //pattern = @"\b(\w+)\s+\w+(\k\1)\b";

                ////Almost perfect
                //pattern = @"\b(?:\w+)\s+(\w+)\1\b";

                ////Correct Positions
                //pattern = @"\b(?:\w+)\s+(\w+\1)\b";

                ////Correct Positions extra matches
                //pattern = @"\b(\w+)\b"
                //pattern = @"\b\w+\s+\b(\1)"

                //These both work in Jakarta Applet... check for bugs
                //http://jakarta.apache.org/regexp/applet.html
                string pattern = @"\b(?:(\w+))\s+(\1)\b";
                //string pattern = @"\b(\w+)\s+\1\b";
                Regex rx = new Regex(pattern, RegexOptions.IgnoreCase);
                
                // Define a test string.        
                string text = "The the quick brown fox  fox jumped over the lazy dog dog.";
                //Need dumpProgram to determine if my compiler compiled the same... I think the problem lies in the Match object... I am not passing something correclty to the concstructor....
                // Find matches.
                MatchCollection matches = rx.Matches(text);

                TestTestsHelper.ShowParens(ref rx);

                // Report the number of matches found.
                Log.Comment(matches.Count + " matches found in:\n  " + text);

                // Report on each match.
                foreach (Match match in matches)
                {
                    GroupCollection groups = match.Groups;
                    Log.Comment("'" + groups[0].Value + "'" + " repeated at positions " + groups[0].Index + " and " + groups[1].Index);
                }

                //May be bugs in engine but is likely due to differences in Regex engines because I have no implemented Transfer Caputre, this is known to Fail for now so long as no exception is thrown.
                return MFTestResults.KnownFailure;
            }
            catch
            {
                return MFTestResults.Fail;
            }
        }

        [TestMethod]
        public MFTestResults RegExpTest_8_MatchCollection_Test_3_Timed()
        {
            int catchCount = 0;
            //Should match for a long time and throw the Timing Excpetion
            //This could be changed up a bit to show partial matching
            string pattern = @"\b(?:(\w+))\s+(\1)\b+";
            Regex rx = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Timed);
            rx.MaxTick = ushort.MaxValue * 2;
            // Define a test string.        
            string text = "The the quick brown fox  fox jumped over the lazy dog dog.";
            int start = 0;
            TryToMatch:
            try
            {                
                // Find matches.
                MatchCollection matches = rx.Matches(text, start);

                Log.Comment("Found Matches:" + matches.Count);

                //If we get here you  have a really fast processor and timing is not working as expected
                return MFTestResults.Fail;
            }
            catch (RegexExecutionTimeException ex)
            {                
                if (++catchCount > 5)
                    return MFTestResults.Pass;
                else
                {
                    //Try to match again at the index
                    //Could check index or access the Groups up the Index here but in this example we never matched.
                    start = ex.Index;
                    goto TryToMatch;
                }
            }
            catch
            {
                return MFTestResults.Fail;
            }
        }

    }
}
