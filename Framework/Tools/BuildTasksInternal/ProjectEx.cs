using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Xsl;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.BuildEngine;
using _BE=Microsoft.Build.Evaluation;

namespace Microsoft.SPOT.Tasks.Internal
{
    public class ProjectEx //: _BE.Project
    {
        protected _BE.Project m_project;

        protected _BE.ProjectLoadSettings m_settings;
        protected string m_xmlPath;

        public static Microsoft.Build.Utilities.Task Task;

        private static string[] fileGroups = { "Compile", "EmbeddedResource", "Content", "None" };
        private string[] buildFlavors = { "Debug", "Release" };
        private Hashtable overloadedProperties = new Hashtable();
        private bool deploy = false;
        private List<string> extraFiles = new List<string>();
        private Dictionary<string, string> externalFiles = new Dictionary<string, string>();
        string targetToolsVersion = null;
        private string m_origProjPath;

        private Version version = null;


        private static string[] terminals = new string[0];


        public ProjectEx(
            Microsoft.Build.Framework.ITaskItem projectItem
            )
            : this(projectItem, new Version(0,0,0,0), "")
        {
        }

        public ProjectEx(
            Microsoft.Build.Framework.ITaskItem projectItem,
            Version version,
            string origProjectPath
            )
        {
            this.m_xmlPath = projectItem.ItemSpec;
            this.version = version;
            m_origProjPath = origProjectPath;

            string deployMetadataValue = projectItem.GetMetadata("Deploy");
            if (deployMetadataValue == "true")
            {
                this.Deploy = true;
            }

            System.Text.RegularExpressions.Regex regex =
                new System.Text.RegularExpressions.Regex(" xmlns=\"http://{0,1}[a-zA-Z0-9/.]*\"+");

            string extraFilesBody = regex.Replace(projectItem.GetMetadata("ExtraFiles"), "");

            string extraFilesXml = string.Format("{0}{1}{2}",
                "<ExtraFiles>",
                extraFilesBody,
                "</ExtraFiles>");

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(extraFilesXml);

            foreach (System.Xml.XmlNode extraFileNode in doc.SelectNodes("ExtraFiles/File"))
            {
                this.AddExtraFile(extraFileNode.Attributes["Name"].Value);
            }
        }

        public void Load()
        {
            m_project = new _BE.Project(m_xmlPath, null, TargetToolsVersion, new _BE.ProjectCollection(), m_settings);
        }

        public string ProjectDirectory
        {
            get
            {
                string localProjectPath = Path.GetDirectoryName(this.FullFileName);
                if (!localProjectPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    localProjectPath += Path.DirectorySeparatorChar.ToString();
                }

                return localProjectPath;
            }
        }

        public bool Deploy
        {
            get { return deploy; }
            set { deploy = value; }
        }

        public string[] BuildFlavors
        {
            get { return buildFlavors; }
        }

        public string FullFileName
        {
            get
            {
                if (overloadedProperties.Contains("FullFileName"))
                {
                    return overloadedProperties["FullFileName"] as string;
                }
                else
                {
                    if (m_project != null)
                    {
                        return this.m_project.FullPath;
                    }
                    else
                    {
                        return m_xmlPath;
                    }
                }
            }
        }

        public string SolutionRelativePath
        {
            get
            {
                return string.Format("{0}\\{1}",
                    this.GetEvaluatedProperty("MSBuildProjectName"),
                    this.GetEvaluatedProperty("MSBuildProjectFile"));
            }
        }

        public string PathToSolution
        {
            get { return "..\\"; }
        }

        public static string[] FileGroups
        {
            get { return fileGroups; }
        }

        public List<string> ExtraFiles
        {
            get { return extraFiles; }
        }

        public void AddExtraFile(string extraFile)
        {
            string relativePath = GetRelativePath(extraFile);

            if (relativePath != null)
            {
                extraFiles.Add(relativePath);
            }
            else if (File.Exists(extraFile))
            {
                extraFile = BuildTaskUtility.ExpandEnvironmentVariables(extraFile);
                string fileName = Path.GetFileName(extraFile);
                extraFiles.Add(fileName);
                externalFiles.Add(fileName, extraFile);
            }
            else if(Task != null)
            {
                Task.Log.LogWarning("Ignoring missing ExtraFile \"{0}\"", extraFile);
            }
        }

        public string GetRelativePath(string path)
        {
            string relativePath = null;
            string previousDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = this.ProjectDirectory;
            string filePath =
                Path.GetFullPath(
                    BuildTaskUtility.ExpandEnvironmentVariables(path));

            if (File.Exists(filePath) || Directory.Exists(filePath))
            {
                relativePath = BuildTaskUtility.GetRelativePath(
                    this.ProjectDirectory,
                    filePath);
            }

            Environment.CurrentDirectory = previousDirectory;
            return relativePath;
        }

        public string GetFullPath(string path)
        {
            // If the true path to the file is
            // external to the project, return the
            // true path.
            if (externalFiles.ContainsKey(path))
            {
                return externalFiles[path];
            }

            string previousDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = this.ProjectDirectory;

            string absolutePath = BuildTaskUtility.ExpandEnvironmentVariables(
                Path.GetFullPath(path));

            Environment.CurrentDirectory = previousDirectory;

            return absolutePath;
        }

        public _BE.Project MsBuildProject
        {
            get 
            {
                if (m_project == null)
                {
                    Load();
                }
                return m_project; 
            }
        }

        private void OverrideProperties(string fullFileName)
        {
            if (fullFileName == null || fullFileName == "" || !File.Exists(fullFileName))
            {
                return;
            }

            overloadedProperties = new Hashtable();
            overloadedProperties.Add("FullFileName", fullFileName);
            overloadedProperties.Add("MSBuildProjectFullPath", fullFileName);
            overloadedProperties.Add("MSBuildProjectDirectory", Path.GetDirectoryName(fullFileName));
            overloadedProperties.Add("MSBuildProjectFile", Path.GetFileName(fullFileName));
            overloadedProperties.Add("MSBuildProjectExtension", Path.GetExtension(fullFileName));
            overloadedProperties.Add("MSBuildProjectName", Path.GetFileNameWithoutExtension(fullFileName));
        }

        public string GetEvaluatedProperty(string propertyName)
        {
            string originalPropertyName = propertyName;
            if (overloadedProperties.ContainsKey(propertyName))
            {
                return overloadedProperties[propertyName] as string;
            }

            string propertyValue = null;

            if (m_project != null)
            {
                propertyValue = m_project.GetPropertyValue(propertyName);
            }

            if (!string.IsNullOrEmpty(propertyValue))
            {
                BuildTaskUtility.RegexProperties(
                    ref propertyValue,
                    PropertyMatch);
            }

            return propertyValue;
        }

        private void PropertyMatch(ref string source, System.Text.RegularExpressions.Capture capture)
        {
            string propertyName = BuildTaskUtility.PropertyNameFromCapture(capture);

            string propertyValue = GetEvaluatedProperty(propertyName);

            source.Replace(capture.Value, propertyValue);
        }

        public static string[] Terminals
        {
            get { return terminals; }
            set
            {
                terminals = value;
                Array.Sort(terminals);

                for (int i = 0; i < terminals.Length; i++)
                {
                    terminals[i] = BuildTaskUtility.ExpandEnvironmentVariables(terminals[i]);
                }
            }
        }


        private XmlDocument ExpandProject(XmlDocument projectDocument)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(projectDocument.NameTable);
            nsmgr.AddNamespace(
                "msbuild",
                projectDocument.DocumentElement.Attributes["xmlns"].Value);

            XmlNode node = projectDocument.SelectSingleNode(
                "msbuild:Project/msbuild:PropertyGroup/msbuild:ImportToSDK",
                nsmgr);

            if (node != null && node.InnerText.ToLower() != "true")
            {
                return null;
            }

            Hashtable importReplacements = new Hashtable();

            foreach (XmlNode importNode in projectDocument.SelectNodes(
                "msbuild:Project/msbuild:Import", nsmgr))
            {
                string importProjFilePath =
                    BuildTaskUtility.ExpandEnvironmentVariables(importNode.Attributes["Project"].Value);
                if (!File.Exists(importProjFilePath))
                {
                    continue;
                }

                int searchResult = Array.BinarySearch<string>(
                    terminals,
                    importProjFilePath,
                    StringComparer.OrdinalIgnoreCase);

                if(searchResult >= 0)
                {
                    continue;
                }

                XmlDocument importDocument = new XmlDocument();
                importDocument.Load(importProjFilePath);

                string currentDirectory = Environment.CurrentDirectory;
                if (Directory.Exists(Path.GetDirectoryName(importProjFilePath)))
                {
                    Environment.CurrentDirectory = Path.GetDirectoryName(importProjFilePath);
                }
                importDocument = ExpandProject(importDocument);
                Environment.CurrentDirectory = currentDirectory;

                if (importDocument != null)
                {
                    importReplacements.Add(importNode, importDocument);
                }
            }

            IDictionaryEnumerator ienum = importReplacements.GetEnumerator();
            while (ienum.MoveNext())
            {
                XmlNode replaceMe = ienum.Key as XmlNode;
                XmlDocument importDocument = ienum.Value as XmlDocument;

                foreach (XmlNode child in importDocument.DocumentElement.ChildNodes)
                {
                    replaceMe.ParentNode.InsertBefore(
                        projectDocument.ImportNode(child, true),
                        replaceMe);
                }

                replaceMe.ParentNode.RemoveChild(replaceMe);
            }

            return projectDocument;

        }

        private struct MyImportItems
        {
            public string name;
            public string include;
            public Dictionary<string, string> meta;
        }

        public void NormalizePaths(bool addImports)
        {
            List<_BE.ProjectItem> removeItems = new List<_BE.ProjectItem>();

            foreach (_BE.ProjectItem folderItem in m_project.GetItems("Folder"))
            {
                string path = folderItem.Xml.Include;

                if (!path.Equals(GetRelativePath(path), StringComparison.OrdinalIgnoreCase))
                {
                    removeItems.Add(folderItem);
                    continue;
                }

                if (!Directory.Exists(path))
                {
                    removeItems.Add(folderItem);
                    continue;
                }
            }

            foreach (string groupName in ProjectEx.FileGroups)
            {
                List<MyImportItems> importItems = new List<MyImportItems>();
                foreach (_BE.ProjectItem buildItem in m_project.GetItems(groupName))
                {
                    string projectRelativePath = GetRelativePath(buildItem.Xml.Include);

                    // The path has to be an absolute path to somewhere
                    // outside the project directory.
                    if (projectRelativePath == null)
                    {
                        string origPath = Path.Combine(Path.GetDirectoryName(m_origProjPath), buildItem.EvaluatedInclude);
                        if (File.Exists(origPath))
                        {
                            projectRelativePath = buildItem.Xml.Include;
                            externalFiles.Add(buildItem.Xml.Include, origPath);
                        }
                        else if (File.Exists(BuildTaskUtility.ExpandEnvironmentVariables(buildItem.Xml.Include)))
                        {
                            projectRelativePath = Path.GetFileName(buildItem.Xml.Include);
                            externalFiles.Add(
                                projectRelativePath,
                                BuildTaskUtility.ExpandEnvironmentVariables(buildItem.Xml.Include));
                        }
                        else if (!buildItem.IsImported)
                        {
                            // since the file doesn't even exist
                            // remove it from the project
                            removeItems.Add(buildItem);
                            continue;
                        }
                        else
                        {
                            // This is an imported item
                            // it can't be removed, so
                            // ignore it.
                            if (Task != null)
                            {
                                Task.Log.LogWarning("Ignoring missing imported build item, \"{0}, {1}\"", buildItem.ItemType, buildItem.Xml.Include);
                            }
                            continue;
                        }
                    }

                    if (!buildItem.IsImported)
                    {
                        // Reset the path to the file relative to
                        // the project
                        buildItem.Xml.Include = projectRelativePath;
                    }
                    else if(addImports)
                    {
                        MyImportItems newItem = new MyImportItems();
                        newItem.name = buildItem.ItemType;
                        newItem.include = projectRelativePath;
                        newItem.meta = new Dictionary<string,string>();

                        foreach (_BE.ProjectMetadata meta in buildItem.Metadata)
                        {
                            newItem.meta.Add(meta.Name, meta.UnevaluatedValue);
                        }
                        importItems.Add(newItem);
                    }
                }

                foreach (MyImportItems importItem in importItems)
                {
                    m_project.AddItem(importItem.name, importItem.include, importItem.meta);
                }
            }

            foreach (_BE.ProjectItem removeItem in removeItems)
            {
                Task.Log.LogWarning("Removing missing build item, \"{0}, {1}\"", removeItem.ItemType, removeItem.Xml.Include);
                m_project.RemoveItem(removeItem);
            }
        }


        public string TargetToolsVersion
        {
            get
            {
                if (string.IsNullOrEmpty(targetToolsVersion))
                {
                    if (m_project != null)
                    {
                        return m_project.ToolsVersion;
                    }
                    else
                    {
                        return "4.0";
                    }
                }
                else
                {
                    return targetToolsVersion;
                }
            }
            set { targetToolsVersion = value; }
        }
    }
}
