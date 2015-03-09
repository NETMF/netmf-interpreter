using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;

using Task = Microsoft.Build.Utilities.Task;

namespace Microsoft.SPOT.Tasks.Internal
{
    public class SubmitSymbols : Task
    {
        private string requestSearchPath;
        private ITaskItem[] replacementValues;
        //private string submitCmd = @"\\symbols\tools\createrequest.cmd";
        private bool archive = false;
        private string requestFile;
        private Dictionary<string,string> replacmentDictionary = new Dictionary<string,string>();
        private string[] userNames = null;

        private string root = null;

        public override bool Execute()
        {
            try
            {
                if(replacementValues != null)
                {
                    foreach(ITaskItem replacementValue in replacementValues)
                    {
                        if (replacementValue.ItemSpec == "<Root>")
                        {
                            root = replacementValue.GetMetadata("Value");

                            if (!root.EndsWith("\\"))
                            {
                                root += "\\";
                            }
                        }
                        replacmentDictionary.Add(replacementValue.ItemSpec, replacementValue.GetMetadata("Value"));
                    }
                }
                Search(requestSearchPath);
                return true;
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
                return false;
            }
        }

        private void Search(string directory)
        {
            Submit(directory);

            foreach (string subdir in Directory.GetDirectories(directory))
            {
                Search(subdir);
            }
        }

        private void Submit(string directory)
        {
            string requestFilePath = directory + "\\" + requestFile;
            if (!File.Exists(requestFilePath))
            {
                return;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(requestFilePath);

            XmlNode fileListNode = doc.SelectSingleNode("SymbolRequest/FileList");

            string fileList = null;
            if (fileListNode != null)
            {
                fileList = fileListNode.InnerText;
            }

            string indexFile = null;
            string archiveFile = null;
            foreach(XmlNode inputFile in doc.SelectNodes("SymbolRequest/InputFile"))
            {
                if(inputFile.InnerText.ToLower().EndsWith(".index"))
                {
                    indexFile = inputFile.InnerText;
                }

                if (inputFile.InnerText.ToLower().EndsWith(".archive"))
                {
                    archiveFile = inputFile.InnerText;
                }
            }

            if (fileList == null || indexFile == null)
            {
                return;
            }

            FinishFileList(directory + "\\" + fileList);

            if (!replacmentDictionary.ContainsKey("<FileList>"))
            {
                replacmentDictionary.Add("<FileList>", fileList);
            }
            else
            {
                replacmentDictionary["<FileList>"] = fileList;
            }

            string iniFile = "request.ini";

            StreamWriter writer = new StreamWriter(directory + "\\" + iniFile);
            ReadWriteIniFile(directory + "\\" + indexFile, writer);

            string cmdArgs = string.Format("-i {0} -d . -c", iniFile);

            if (archive && archiveFile != null)
            {
                cmdArgs += " -a";

                ReadWriteIniFile(directory + "\\" + archiveFile, writer);
            }
            else
            {
                cmdArgs += " -s";
            }
            writer.Close();
        }

        private void ReadWriteIniFile(string iniFile, StreamWriter outWriter)
        {
            StreamReader reader = new StreamReader(iniFile);
            string line = null;
            while ((line = reader.ReadLine()) != null)
            {
                Regex regex = new Regex("[<]\\w+[>]", RegexOptions.IgnoreCase);

                Match m = regex.Match(line);

                while (m.Success)
                {
                    if (replacmentDictionary.ContainsKey(m.Value))
                    {
                        line = line.Replace(m.Value, replacmentDictionary[m.Value]);
                    }
                    m = m.NextMatch();
                }

                if (line.StartsWith("UserName=", StringComparison.OrdinalIgnoreCase))
                {
                    userNames = line.Substring("UserName=".Length).Split(new char[] { ';' });
                }

                outWriter.WriteLine(line);
            }

            reader.Close();
        }

        private void FinishFileList(string fileList)
        {
            string tempFile = Path.GetDirectoryName(fileList) + "\\" + Path.GetFileNameWithoutExtension(fileList) + "_temp.txt";
            StreamReader reader = new StreamReader(fileList);

            StreamWriter writer = new StreamWriter(tempFile);

            string line = null;

            while ((line = reader.ReadLine()) != null)
            {
                writer.WriteLine(root + line);
            }

            reader.Close();
            writer.Close();
            File.Delete(fileList);
            File.Move(tempFile, fileList);
        }

        [Required]
        public string RequestSearchPath
        {
            set { requestSearchPath = value; }
        }

        public ITaskItem[] ReplacementValues
        {
            set { replacementValues = value; }
        }

        [Required]
        public string RequestFileName
        {
            set { requestFile = value; }
        }

        public bool Archive
        {
            set { archive = value; }
        }
    }
}
