using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;

using Task = Microsoft.Build.Utilities.Task;

namespace Microsoft.SPOT.Tasks.Internal
{
    public class TransmorgifyTemplateFiles : Task
    {
        private ITaskItem[] destinationFiles;
        private ITaskItem[] sourceFiles;

        public override bool Execute()
        {
            if (sourceFiles == null || sourceFiles.Length == 0)
            {
                destinationFiles = new TaskItem[0];
                return true;
            }
            if (!ValidateInputs())
            {
                return false;
            }

            bool result = true;
            for (int i = 0; i < sourceFiles.Length; i++)
            {
                bool doReplacements;
                string rootNamespace;
                if (!Boolean.TryParse(sourceFiles[i].GetMetadata("DoReplacements"), out doReplacements))
                {
                    Log.LogWarning("File {0} was passed to TransmorgifyTemplateFiles despite 'DoReplacements' metadata being unset.",
                        sourceFiles[i].ItemSpec);
                    continue;
                }
                else if (doReplacements == false)
                {
                    Log.LogWarning("File {0} was passed to TransmorgifyTemplateFiles despite 'DoReplacements' metadata explicitly set to 'false'.",
                        sourceFiles[i].ItemSpec);
                    continue;
                }
                rootNamespace = sourceFiles[i].GetMetadata("RootNamespace");
                if (string.IsNullOrEmpty(rootNamespace))
                {
                    Log.LogWarning("File {0} was passed to TransmorgifyTemplateFiles without RootNamespace metadata; transmorgification may be incomplete.",
                        sourceFiles[i].ItemSpec);
                }

                string fullPath = sourceFiles[i].GetMetadata("FullPath");

                try
                {
                    string directoryName = Path.GetDirectoryName(destinationFiles[i].ItemSpec);
                    if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
                    {
                        Log.LogMessage(MessageImportance.Normal, "Creating directory \"{0}\".", directoryName);
                        Directory.CreateDirectory(directoryName);
                    }
                    
                    string fileContents = null;
                    System.Text.Encoding fileEncoding = System.Text.UTF8Encoding.UTF8;

                    using (StreamReader sr = new StreamReader(sourceFiles[i].ItemSpec, true))
                    {
                        fileContents = sr.ReadToEnd();
                        fileEncoding = sr.CurrentEncoding;
                    }

                    TransmorgificationUtilities.MakeTemplateReplacements(false, true, rootNamespace, fullPath, ref fileContents);

                    using (StreamWriter sw = new StreamWriter(destinationFiles[i].ItemSpec, false, fileEncoding))
                    {
                        sw.Write(fileContents);
                    }

                    sourceFiles[i].CopyMetadataTo(destinationFiles[i]);

                }
                catch (Exception e)
                {
                    result = false;
                    Log.LogErrorFromException(e);
                }
            }

            return result;
        }

        private bool ValidateInputs()
        {
            if (destinationFiles.Length != sourceFiles.Length)
            {
                Log.LogError("DestinationFiles and SourceFiles must have the same length.");
                return false;
            }
            return true;
        }

        #region MSBuild Task Parameters
        [Required]
        [Output]
        public ITaskItem[] DestinationFiles
        {
            get { return destinationFiles; }
            set { destinationFiles = value; }
        }

        [Required]
        public ITaskItem[] SourceFiles
        {
            get { return sourceFiles; }
            set { sourceFiles = value; }
        }
        #endregion
    }
}
