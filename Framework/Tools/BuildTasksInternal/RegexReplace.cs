using System;
using System.Text.RegularExpressions;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;

using Task = Microsoft.Build.Utilities.Task;

namespace Microsoft.SPOT.Tasks.Internal
{
    public class RegexReplace : Task
    {
        string pattern;
        string input;
        string replacement;
        string result;

        int count = -1;
        int startat = 0;

        public override bool Execute()
        {
            try
            {
                Regex regex = new Regex(pattern);

                result = regex.Replace(input, replacement, count, startat);

                return true;
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
                return false;
            }
        }

        [Required]
        public string Pattern
        {
            get { return pattern; }
            set { pattern = value; }
        }

        [Required]
        public string Input
        {
            get { return input; }
            set { input = value; }
        }

        [Required]
        public string Replacement
        {
            get { return replacement; }
            set { replacement = value; }
        }

        [Output]
        public string Result
        {
            get { return result; }
            set { result = value; }
        }

        public int Count
        {
            get { return count; }
            set { count = value; }
        }

        public int StartAt
        {
            get { return startat; }
            set { startat = value; }
        }
    }
}
