using System;
using System.Collections.Generic;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.SPOT.AutomatedBuild.BuildSigner;

using Task = Microsoft.Build.Utilities.Task;

namespace Microsoft.SPOT.Tasks.Internal
{
    public class SignMSI : Task
    {
        private string stagingDirectory;
        private string msi;
        private SignInfo signInfo = new SignInfo();

        public override bool Execute()
        {
            try
            {
                return BuildSigner.SignFiles(new string[] { msi }, stagingDirectory, signInfo, this);
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
                return false;
            }
        }

        [Required]
        public string StageDirectory
        {
            set { stagingDirectory = value; }
        }

        [Required]
        public string MSI
        {
            set { msi = value; }
        }

        [Required]
        public string JobDescription
        {
            set { signInfo.jobDescription = value; }
        }

        [Required]
        public string JobKeywords
        {
            set { signInfo.jobKeywords = value; }
        }

        [Required]
        public string Certificate
        {
            set { signInfo.certificate = value; }
        }

        [Required]
        public string[] Approvers
        {
            set { signInfo.Approvers = value; }
        }

        [Required]
        public string DisplayName
        {
            set { signInfo.displayName = value; }
        }

        [Required]
        public string DisplayURL
        {
            set { signInfo.displayURL = value; }
        }
    }
}
