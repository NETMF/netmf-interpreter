using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;

using Task = Microsoft.Build.Utilities.Task;

namespace Microsoft.SPOT.Tasks.Internal
{
    public class CreateSymbolRequest : Task
    {
        private ITaskItem[] files;
        private ITaskItem[] fileTypes;
        private Dictionary<string, object> fileTypeTable = new Dictionary<string, object>();
        private string workingDirectory;
        private string[] inputFiles;
        private string root;
        private List<string> indexFiles = new List<string>();
        private string fileListText = "";
        private string requestFile;
        private string requestFileContent = "<SymbolRequest>\r\n<FileList>filelist.txt</FileList>\r\n{0}</SymbolRequest>";

        public override bool Execute()
        {
            try
            {
                foreach(ITaskItem fileType in fileTypes)
                {
                    fileTypeTable.Add(fileType.ItemSpec, null);
                }

                foreach(ITaskItem file in files)
                {
                    if(fileTypeTable.ContainsKey(file.GetMetadata("FileType")))
                    {
                        AddFile(file.ItemSpec);

                        string directory = Path.GetDirectoryName(file.ItemSpec);
                        string fileNameWOExtension = Path.GetFileNameWithoutExtension(file.ItemSpec);
                        string pdbFile = directory + "\\" + fileNameWOExtension + ".pdb";

                        if (File.Exists(pdbFile))
                        {
                            AddFile(pdbFile);
                        }
                    }
                }

                if (!Directory.Exists(workingDirectory))
                {
                    Directory.CreateDirectory(workingDirectory);
                }

                StreamWriter writer = new StreamWriter(workingDirectory + "\\filelist.txt");
                writer.Write(fileListText);
                writer.Close();

                string requestInputFiles = "";

                foreach (string inputFile in inputFiles)
                {
                    StreamReader reader = new StreamReader(inputFile);
                    string contents = reader.ReadToEnd();
                    writer = new StreamWriter(workingDirectory + "\\" + Path.GetFileName(inputFile));
                    writer.Write(contents);
                    writer.Close();

                    requestInputFiles += "<InputFile>" + Path.GetFileName(inputFile) + "</InputFile>\r\n";
                }

                requestFileContent = string.Format(requestFileContent, requestInputFiles);

                writer = new StreamWriter(workingDirectory + "\\" + requestFile);
                writer.Write(requestFileContent);
                writer.Close();

                return true;
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
                return false;
            }
        }

        private void AddFile(string file)
        {
            file = file.Remove(0, root.Length);

            if (file.StartsWith("\\"))
            {
                file = file.Remove(0, 1);
            }

            if (!indexFiles.Contains(file))
            {
                indexFiles.Add(file);
                fileListText += file + "\r\n";
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
        public string SymbolWorkingDirectory
        {
            set { workingDirectory = value; }
        }

        [Required]
        public string[] InputFiles
        {
            set { inputFiles = value; }
        }

        [Required]
        public string Root
        {
            set { root = value.ToLower(); }
        }

        [Required]
        public string RequestFileName
        {
            set { requestFile = value; }
        }
    }
}
