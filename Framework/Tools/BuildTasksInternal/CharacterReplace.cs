using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace Microsoft.SPOT.Tasks.Internal
{
    public class CharacterReplace : Task
    {
        public override bool Execute()
        {
            if (String.IsNullOrEmpty(to) || String.IsNullOrEmpty(from))
                return false;

            output = input.Replace(from, to);
            return true;
        }

        private string input = String.Empty;
        [Required]
        public string Input
        {
            set { input = value; }
        }

        private string from = String.Empty;
        [Required]
        public string From
        {
            set { from = value; }
        }

        private string to = String.Empty;
        [Required]
        public string To
        {
            set { to = value; }
        }

        private string output = String.Empty;
        [Output]
        public string Output
        {
            get { return output; }
        }
    }
}
