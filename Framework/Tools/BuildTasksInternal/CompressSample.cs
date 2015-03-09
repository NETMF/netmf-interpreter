using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Build.Framework;

using Task = Microsoft.Build.Utilities.Task;

namespace Microsoft.SPOT.Tasks.Internal
{
    public class CompressSample : Task
    {
        private static Dictionary<string, string> directoryDictionary = new Dictionary<string, string>();
        private string compressFileName = null;
        private string parentDirectoryRef = "SamplesDir";
        private string wixFile = null;

        [Required]
        public string CompressFile
        {
            set { compressFileName = value; }
        }

        [Required]
        public string ParentDirectoryRef
        {
            set { parentDirectoryRef = value; }
        }

        [Required]
        public string WiXFile
        {
            set { wixFile = value; }
        }


        public override bool Execute()
        {
            try
            {
                XmlDocument wiXDoc = new XmlDocument();
                wiXDoc.Load(new FileStream(wixFile, FileMode.Open, FileAccess.Read));

                string outputPath = Path.GetDirectoryName(compressFileName);
                if (!outputPath.EndsWith("\\"))
                {
                    outputPath += "\\";
                }

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(wiXDoc.NameTable);
                nsmgr.AddNamespace(
                    "ns",
                    wiXDoc.DocumentElement.Attributes["xmlns"].Value);

                XmlNode rootDirNode = wiXDoc.SelectSingleNode(
                    string.Format("/descendant::ns:Fragment/ns:DirectoryRef[@Id=\"{0}\"]/ns:Directory", parentDirectoryRef), nsmgr);

                string zipDirectory = CreateDirectory(rootDirNode, outputPath);

                string xPath = "/descendant::ns:Fragment/ns:DirectoryRef/ns:Directory/ns:Component/ns:File";

                foreach (XmlNode fileNode in wiXDoc.SelectNodes(xPath, nsmgr))
                {
                    CopyFile(fileNode);
                }

                string currentDirectory = Environment.CurrentDirectory;
                Environment.CurrentDirectory = Path.GetDirectoryName(outputPath);

                SetReadWrite(outputPath + zipDirectory);

                CreateZip zip = new CreateZip();
                zip.BuildEngine = BuildEngine;
                zip.HostObject = HostObject;
                zip.InputDirectories = new Microsoft.Build.Framework.ITaskItem[] { new Microsoft.Build.Utilities.TaskItem(zipDirectory) };
                zip.OutputFile = compressFileName;

                zip.Execute();

                Directory.Delete(outputPath + zipDirectory, true);

                Environment.CurrentDirectory = currentDirectory;
            }
            catch (Exception ex)
            {
                Console.WriteLine("***Exception: " + ex.ToString());
                return false;
            }

            return true;
        }

        private static string CreateDirectory(XmlNode directoryNode, string path)
        {
            string name = null;

            name = directoryNode.Attributes["Name"].Value;

            string fullPath = path + name;

            string id = directoryNode.Attributes["Id"].Value;

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            fullPath += "\\";

            if (id != null)
            {
                directoryDictionary.Add(id, fullPath);
            }

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(directoryNode.OwnerDocument.NameTable);
            nsmgr.AddNamespace(
                "ns",
                directoryNode.OwnerDocument.DocumentElement.Attributes["xmlns"].Value);

            foreach (XmlNode children in directoryNode.SelectNodes("ns:Directory", nsmgr))
            {
                CreateDirectory(children, fullPath);
            }

            return name;
        }

        private static void SetReadWrite(string directory)
        {
            foreach (string file in Directory.GetFiles(directory))
            {
                File.SetAttributes(
                    file,
                    File.GetAttributes(file) & ~FileAttributes.ReadOnly);
            }

            foreach (string subdirectory in Directory.GetDirectories(directory))
            {
                SetReadWrite(subdirectory);
            }
        }

        private static void CopyFile(XmlNode fileNode)
        {
            string src = fileNode.Attributes["Source"].Value;
            string name = null;

            name = fileNode.Attributes["Name"].Value;

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(fileNode.OwnerDocument.NameTable);
            nsmgr.AddNamespace(
                "ns",
                fileNode.OwnerDocument.DocumentElement.Attributes["xmlns"].Value);

            XmlNode copyNode = fileNode.SelectSingleNode("ns:CopyFile", nsmgr);

            string directoryId = copyNode.Attributes["DestinationDirectory"].Value;

            if (copyNode.Attributes["DestinationName"] != null)
            {
                name = copyNode.Attributes["DestinationName"].Value;
            }

            if (directoryDictionary.ContainsKey(directoryId))
            {
                string dest = directoryDictionary[directoryId] + name;
                if (!File.Exists(dest))
                {
                    File.Copy(src, dest);
                }
            }
        }
    }
}
