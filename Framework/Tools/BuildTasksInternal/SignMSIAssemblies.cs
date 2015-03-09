using System;
using System.Collections.Generic;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.SPOT.AutomatedBuild.BuildSigner;

using Task = Microsoft.Build.Utilities.Task;

namespace Microsoft.SPOT.Tasks.Internal
{
    public class SignMSIAssemblies : Task
    {
        private ITaskItem[] files;
        private ITaskItem[] fileTypes;
        private string stagingDirectory;
        private SignInfo signInfo = new SignInfo();

        public override bool Execute()
        {
            var debugTasks=Environment.GetEnvironmentVariable("MSBUILD_DEBUG_TASKS");
            if( !string.IsNullOrWhiteSpace( debugTasks ) && debugTasks.Contains("Microsoft.SPOT.Tasks.Internal.SignMsiAssemblies") )
                System.Diagnostics.Debugger.Launch();
 
            try
            {
                Dictionary<string, string> typeDictionary = new Dictionary<string, string>();

                foreach (ITaskItem fileType in fileTypes)
                {
                    if (!typeDictionary.ContainsKey(fileType.ItemSpec))
                    {
                        typeDictionary.Add(fileType.ItemSpec, null);
                    }
                }

                List<string> signableFiles = new List<string>();

                foreach (ITaskItem file in files)
                {
                    if (typeDictionary.ContainsKey(file.GetMetadata("FileType")))
                    {
                        signableFiles.Add(file.ItemSpec);
                    }
                }


                return BuildSigner.SignFiles(signableFiles.ToArray(), stagingDirectory, signInfo, this);
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
                return false;
            }
        }

        [Required]
        public ITaskItem[] Files
        {
            set { files = value; }
        }

        [Required]
        public ITaskItem[] FileTypes
        {
            set { fileTypes = value; }
        }

        [Required]
        public string StageDirectory
        {
            set
            {
                stagingDirectory = value;
            }
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
