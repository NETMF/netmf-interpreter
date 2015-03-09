using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.SPOT.Tasks.Internal
{
    public class DirectorySearch : Task
    {
        private string[] directories = null;
        private string[] wildcards = null;
        private ITaskItem[] foundItems = null;
        private bool useSubdirs = true;

        public override bool Execute()
        {
            if (directories == null || directories.Length == 0)
            {
                Log.LogError("No directories specified");
                return false;
            }

            if (wildcards == null || wildcards.Length == 0)
            {
                wildcards = new string[1];
                wildcards[0] = "*";
            }

            ArrayList masterFileList = new ArrayList();

            foreach (string directory in directories)
            {
                if (!Directory.Exists(directory))
                {
                    Log.LogWarning("Directory {0} does not exist", directory);
                    continue;
                }

                foreach (string wildcard in wildcards)
                {
                    try
                    {
                        string[] fileList = Directory.GetFiles(
                            directory,
                            wildcard,
                            this.useSubdirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                        foreach (string file in fileList)
                        {
                            masterFileList.Add(new TaskItem(file));
                        }

                    }
                    catch (Exception e)
                    {
                        Log.LogError("Error scanning directory {0} with wildcard {1}: {2}",
                            directory,
                            wildcard,
                            e.Message);
                        return false;
                    }
                }
            }

            foundItems = (ITaskItem[])masterFileList.ToArray(typeof(ITaskItem));
            return true;
        }

        [Required]
        public string[] Directories
        {
            get { return directories; }
            set { directories = value; }
        }

        public string[] Wildcards
        {
            get { return wildcards; }
            set { wildcards = value; }
        }

        public bool UseSubdirectories
        {
            get { return useSubdirs; }
            set { useSubdirs = value; }
        }

        [Output]
        public ITaskItem[] FoundFiles
        {
            get { return foundItems; }
        }
    }
}