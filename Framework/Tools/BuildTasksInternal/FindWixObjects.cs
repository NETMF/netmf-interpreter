using System;
using System.Collections;
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
    public class FindWixObjects : Task
    {
        private List<string> m_InputObjects;
        private Dictionary<string, string> m_FoundComponents = new Dictionary<string, string>();
        private Dictionary<string, string> m_FoundComponentGroups = new Dictionary<string, string>();
        private Dictionary<string, List<ITaskItem>> m_FoundFilesDictionary = new Dictionary<string, List<ITaskItem>>();
        private string[] m_SearchPaths;
        private string[] m_ExcludePaths;
        private List<string> m_FoundObjects = new List<string>();
        private List<ITaskItem> m_FoundFiles = new List<ITaskItem>();
        private ITaskItem[] m_FileTypes;
        private bool m_PrintFiles = false;
        private string m_WorkingDirectory = null;

        public override bool Execute()
        {
            // System.Diagnostics.Debugger.Launch();

            try
            {
                // Load the object database
                // The purpose is to look in all the directories specified for any wixobj load it so
                // that it is easy to find later from the source wixobj files
                Stack<string> dirs = new Stack<string>(m_SearchPaths);
                Hashtable excludeDirs = new Hashtable();

                if (m_ExcludePaths != null)
                {
                    foreach (string root in m_SearchPaths)
                    {
                        for (int i = 0; i < m_ExcludePaths.Length; i++)
                        {
                            excludeDirs[root.ToLower() + m_ExcludePaths[i].ToLower()] = 1;
                        }
                    }
                }

                while(dirs.Count > 0)
                {
                    string dir = dirs.Pop();

                    foreach(string file in Directory.GetFiles(dir, "*.wixobj", SearchOption.TopDirectoryOnly))
                    {
                        UpdateObjectDictionary(file, true, m_FoundComponents);
                        UpdateObjectDictionary(file, false, m_FoundComponentGroups);
                    }

                    foreach (string nextDir in Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly))
                    {
                        if (!excludeDirs.ContainsKey(nextDir.ToLower()))
                        {
                            dirs.Push(nextDir);
                        }
                    }
                }

                // Go through the input wixobj files and match componentref objects to
                // the parent wixobj of the referenced component.
                foreach (string targetObject in m_InputObjects)
                {
                    List<string> itemsFound = new List<string>(), itemsNotFound = new List<string>();
                    UpdateFoundObjects(targetObject, true, itemsNotFound, itemsFound);
                    UpdateFoundObjects(targetObject, false, itemsNotFound, itemsFound);

/*
                    if (itemsNotFound.Count > 0)
                    {
                        foreach (string cmpId in itemsNotFound)
                        {
                            Log.LogWarning("Did not find component '{0}' referenced in '{1}' in component database", cmpId, targetObject);
                        }
                    }
*/                    
                }

                if (m_PrintFiles)
                {
                    foreach (TaskItem foundFile in FoundFiles)
                    {
                        Log.LogMessage(foundFile.ItemSpec);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
                return false;
            }
        }

        private void UpdateObjectDictionary(string objectFile, bool singleComponents, Dictionary<string, string> dictionary)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(objectFile);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace(
                    "objects",
                    doc.DocumentElement.Attributes["xmlns"].Value);

                string xpath = string.Format(
                    "/descendant::objects:table[@name=\"{0}\"]/objects:row/objects:field[1]",
                    singleComponents ? "Component" : "WixComponentGroup");

                foreach (XmlNode comp in doc.SelectNodes(xpath, nsmgr))
                {
                    XmlElement componentField = comp as XmlElement;

                    if (dictionary.ContainsKey(componentField.InnerText))
                    {
                        Log.LogError("The component dictionary already contains a component with the key {0}.\r\nExisting Component {1}: {2}\r\nNew Component {3}: {4}",
                            componentField.InnerText,
                            componentField.InnerText, dictionary[componentField.InnerText],
                            componentField.InnerText, objectFile);
                    }
                    else
                    {
                        dictionary.Add(componentField.InnerText, objectFile);
                    }
                }

                //xpath = "/descendant::objects:table[@name=\"WixGroup\"]/objects:row/objects:field[4]";
                //foreach (XmlNode comp in doc.SelectNodes(xpath, nsmgr))
                //{
                //    XmlElement componentField = comp as XmlElement;

                //    string obj = singleComponents ? "Component" : "ComponentGroup";

                //    if (componentField.InnerText == obj)
                //    {
                //        componentField = componentField.PreviousSibling as XmlElement;

                //        if (!dictionary.ContainsKey(componentField.InnerText))
                //        {
                //            dictionary.Add(componentField.InnerText, objectFile);
                //        }
                //    }
                //}

                /*
                xpath = "/descendant::objects:table[@name=\"WixSimpleReference\"]/objects:row/objects:field[1]";
                foreach (XmlNode comp in doc.SelectNodes(xpath, nsmgr))
                {
                    XmlElement componentField = comp as XmlElement;

                    string obj = singleComponents ? "Component" : "WixComponentGroup";

                    if (componentField.InnerText == obj)
                    {
                        componentField = componentField.NextSibling as XmlElement;

                        if (!dictionary.ContainsKey(componentField.InnerText))
                        {
                            dictionary.Add(componentField.InnerText, objectFile);
                        }
                    }
                }
                */

                if (m_FileTypes != null)
                {
                    string[] fileSearchPath =
                    {
                        "/descendant::objects:table[@name=\"WixFile\"]/objects:row/objects:field[7]",
                        "/descendant::objects:table[@name=\"Binary\"]/objects:row/objects:field[2]"
                    };

                    foreach (string searchPath in fileSearchPath)
                    {
                        foreach (XmlNode pathField in doc.SelectNodes(searchPath, nsmgr))
                        {
                            foreach (ITaskItem fileType in m_FileTypes)
                            {
                                Regex regex = new Regex(fileType.GetMetadata("RegularExpression"), RegexOptions.IgnoreCase);

                                if (regex.Match(pathField.InnerText).Success)
                                {
                                    if (!m_FoundFilesDictionary.ContainsKey(objectFile))
                                    {
                                        m_FoundFilesDictionary.Add(objectFile, new List<ITaskItem>());
                                    }

                                    var foundFileItem = new TaskItem(pathField.InnerText);
                                    foundFileItem.SetMetadata("FileType", fileType.ItemSpec);
                                    m_FoundFilesDictionary[objectFile].Add(foundFileItem);
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Log.LogWarning("Exception while looking for wix object: " + e.Message); 
            }
        }

        private bool UpdateFoundObjects(string targetObject, bool singleComponents, List<string> itemsNotFound, List<string> itemsFound )
        {
            bool retval = true;
            Dictionary<string, string> targetDictionary = new Dictionary<string, string>();
            Dictionary<string, string> foundDictionary =
                singleComponents ? m_FoundComponents : m_FoundComponentGroups;

            UpdateObjectDictionary(targetObject, singleComponents, targetDictionary);

            // If any files were found in this object add them to the foundFiles list
            // since this object is a top level object it is part of the installer package
            if (m_FoundFilesDictionary.ContainsKey(targetObject))
            {
                foreach (ITaskItem targetObjectFoundFile in m_FoundFilesDictionary[targetObject])
                {
                    if (!m_FoundFiles.Contains(targetObjectFoundFile))
                    {
                        m_FoundFiles.Add(targetObjectFoundFile);
                    }
                }
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(targetObject);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace(
                    "objects",
                    doc.DocumentElement.Attributes["xmlns"].Value);

                string xpath = "/descendant::objects:table[@name=\"WixComplexReference\"]/objects:row/objects:field[4]";

                foreach (XmlNode compRef in doc.SelectNodes(xpath, nsmgr))
                {
                    XmlElement componentRef = compRef as XmlElement;

                    string componentId = componentRef.InnerText;

                    if (targetDictionary.ContainsKey(componentId))
                    {
                        itemsFound.Add(componentId);
                        if (itemsNotFound.Contains(componentId))
                        {
                            itemsNotFound.Remove(componentId);
                        }
                        continue;
                    }

                    if (foundDictionary.ContainsKey(componentId))
                    {
                        string foundObject = foundDictionary[componentId];

                        if (m_WorkingDirectory != null)
                        {
                            if (foundObject.ToLower().StartsWith(m_WorkingDirectory.ToLower()))
                            {
                                foundObject = foundObject.Substring(m_WorkingDirectory.Length);
                            }
                        }

                        if (!m_InputObjects.Contains(foundObject) && !m_FoundObjects.Contains(foundObject))
                        {
                            m_FoundObjects.Add(foundObject);

                            if (m_FoundFilesDictionary.ContainsKey(foundObject))
                            {
                                foreach (ITaskItem foundObjectFile in m_FoundFilesDictionary[foundObject])
                                {
                                    if (!m_FoundFiles.Contains(foundObjectFile))
                                    {
                                        m_FoundFiles.Add(foundObjectFile);
                                    }
                                }
                            }
                        }

                        itemsFound.Add(componentId);
                        if (itemsNotFound.Contains(componentId))
                        {
                            itemsNotFound.Remove(componentId);
                        }
                    }
                    else
                    {
                        //System.Diagnostics.Debugger.Launch();
                        //Log.LogWarning("Did not find component '{0}' referenced in '{1}' in component database", componentId, targetObject);
                        if (!itemsFound.Contains(componentId))
                        {
                            itemsNotFound.Add(componentId);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogWarning("Exception while looking for wix object: " + e.Message);
            }

            return retval;
        }

        [Required]
        public string[] InputWixObjects
        {
            set
            {
                m_InputObjects = new List<string>();

                foreach (string val in value)
                {
                    m_InputObjects.Add(val);
                }
            }
        }

        public string[] ExcludePaths
        {
            set { m_ExcludePaths = value; }
        }

        [Required]
        public string[] SearchPaths
        {
            set { m_SearchPaths = value; }
        }

        public ITaskItem[] FileTypes
        {
            set { m_FileTypes = value; }
        }

        [Output]
        public string[] FoundObjects
        {
            get
            {
                return m_FoundObjects.ToArray();
            }
        }

        [Output]
        public ITaskItem[] FoundFiles
        {
            get
            {
                return m_FoundFiles.ToArray();
            }
        }

        public bool PrintFiles
        {
            set
            {
                m_PrintFiles = value;
            }
        }

        public string WorkingDirectory
        {
            set
            {
                m_WorkingDirectory = value;
            }
        }
    }
}
