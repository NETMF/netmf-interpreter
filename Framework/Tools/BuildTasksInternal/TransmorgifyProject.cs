using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using _BE=Microsoft.Build.Evaluation;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;

using Task = Microsoft.Build.Utilities.Task;
using System.Xml.Xsl;
using System.Text.RegularExpressions;
using System.Collections;

namespace Microsoft.SPOT.Tasks.Internal
{
    public class TransmorgifyProject : Task
    {
        private ITaskItem[] srcProjects;
        private ITaskItem[] destProjects;
        private string[] terminalTargets;
        private ITaskItem[] guidReplacements;

        private struct ProjectToTransform
        {
            public ProjectEx Project;
            public string DestinationFile;
        }

        private Dictionary<string, ProjectToTransform> projects = new Dictionary<string, ProjectToTransform>();
        private Dictionary<Guid, string> guidDictionary = new Dictionary<Guid, string>();

        private string solutionFile;
        private ITaskItem[] extraSlnFiles;

        private Guid componentGuid;
        private string wxsFile;
        private WiXSolution.ShortcutType shortcutType = WiXSolution.ShortcutType.NONE;
        private string parentDirectoryRef = "SamplesDir";

        public override bool Execute()
        {
            if (!ValidateInputs())
            {
                return false;
            }

            try
            {
                ProjectEx.Task = this;
                ProjectEx.Terminals = terminalTargets;
                IEnumerator<KeyValuePair<string, ProjectToTransform>> ienum;
                Version version = new Version(major, minor, build, revision);

                BuildGuidDictionary();
                
                //Load all of the specified projects.
                for (int i = 0; i < srcProjects.Length; i++)
                {
                    ITaskItem srcProject = srcProjects[i];
                    ITaskItem destProject = destProjects[i];

                    if (RunSDKTransform)
                    {
                        string preTransform = srcProject.GetMetadata("PreTransform");
                        if (!string.IsNullOrEmpty(preTransform))
                        {
                            if (!File.Exists(preTransform))
                            {
                                Log.LogWarning("PreTransform \"{0}\" does not exist", preTransform);
                                preTransform = null;
                            }
                        }

                        string postTransform = srcProject.GetMetadata("PostTransform");
                        if (string.IsNullOrEmpty(postTransform))
                        {
                            postTransform = null;
                        }
                        else if (!File.Exists(postTransform))
                        {
                            Log.LogWarning("PostTransform \"{0}\" does not exist", postTransform);

                            postTransform = null;
                        }

                        string transformMetadataValue = srcProject.GetMetadata("Transform");
                        string transformType = "Client";
                        if (transformMetadataValue != null && transformMetadataValue.Equals("Server", StringComparison.OrdinalIgnoreCase))
                        {
                            transformType = "Server";
                        }

                        TransFormProject transformProj = new TransFormProject(transformType, version, srcProject.ItemSpec, destProject.ItemSpec, TargetToolsVersion);

                        transformProj.Transform(preTransform, postTransform);
                    }
                    else if(srcProject.GetMetadata("FullPath") != destProject.GetMetadata("FullPath"))
                    {
                        File.Copy(srcProject.GetMetadata("FullPath"), destProject.GetMetadata("FullPath"));
                    }

                    ProjectEx project = new ProjectEx(destProject, version, srcProject.GetMetadata("FullPath"));
                    project.TargetToolsVersion = TargetToolsVersion;

                    // only load after transform
                    project.Load();

                    RemoveSourceControlAnnotations(project);

                    string assemblyName = project.GetEvaluatedProperty("AssemblyName");
                    projects.Add(assemblyName, new ProjectToTransform { Project = project, DestinationFile = destProject.ItemSpec });

                    //Transforms all paths into relative paths... This probably isn't necessary, as all paths should be relative already...
                    //and a warning is spit out in GetProjectChildFiles now.
                    project.NormalizePaths(true);
                }


                if (RunSDKTransform)
                {
                    ienum = projects.GetEnumerator();
                    while (ienum.MoveNext())
                    {
                        ProjectEx project = ienum.Current.Value.Project;

                        List<Build.Construction.ProjectElement> groupsToRemove = new List<Build.Construction.ProjectElement>();

                        foreach (Build.Construction.ProjectPropertyGroupElement bpg in project.MsBuildProject.Xml.PropertyGroups)
                        {
                            if (bpg != null && !string.IsNullOrEmpty(bpg.Condition) && bpg.Condition.ToLower().Contains("$(spoclient)"))
                            {
                                Console.WriteLine("------ remove property group --- " + bpg.Condition);
                                groupsToRemove.Add(bpg);
                            }
                        }

                        foreach (Build.Construction.ProjectImportElement imp in project.MsBuildProject.Xml.Imports)
                        {
                            if (!string.IsNullOrEmpty(imp.Condition) && imp.Condition.ToLower().Contains("$(spoclient)"))
                            {
                                groupsToRemove.Add(imp);
                            }
                        }

                        foreach (Build.Construction.ProjectElement propertyG in groupsToRemove)
                        {
                            project.MsBuildProject.Xml.RemoveChild(propertyG);
                        }
                    }
                }

                if (RunTemplateTransform)
                {
                    ienum = projects.GetEnumerator();
                    while (ienum.MoveNext())
                    {
                        ProjectEx project = ienum.Current.Value.Project;
                        List<_BE.ProjectProperty> toRemove = new List<_BE.ProjectProperty>();

                        foreach (_BE.ProjectProperty property in project.MsBuildProject.AllEvaluatedProperties)
                        {
                            if (property.Name == "TemplateNameString" ||
                                property.Name == "TemplateNamePackage" ||
                                property.Name == "TemplateNameResID" ||
                                property.Name == "TemplateDescriptionString" ||
                                property.Name == "TemplateDescriptionPackage" ||
                                property.Name == "TeplateDescriptionResID" ||
                                property.Name == "TemplateIconFile" ||
                                property.Name == "TemplateIconPackage" ||
                                property.Name == "TemplateIconResID" ||
                                property.Name == "TemplateID" ||
                                property.Name == "TemplateDefaultName" ||
                                property.Name == "TemplateProjectType" ||
                                property.Name == "TemplateProjectSubType" ||
                                property.Name == "TemplateRequiredFrameworkVersion" ||
                                property.Name == "TemplateSortOrder")
                            {
                                toRemove.Add(property);
                            }
                        }

                        foreach (_BE.ProjectProperty prop in toRemove)
                        {
                            project.MsBuildProject.RemoveProperty(prop);
                        }

                        ReplaceGuidPropertyIfExists(project, "ProjectGuid");
                        ReplaceGuidPropertyIfExists(project, "EmulatorId");

                        foreach (_BE.ProjectItem item in project.MsBuildProject.GetItemsIgnoringCondition("ProjectReference"))
                        {
                            ReplaceGuidMetadataIfExists(item, "Project");
                        }

                        string rootNamepace = project.GetEvaluatedProperty("RootNamespace");
                        TransmorgificationUtilities.MakeTemplateReplacements(true, true, rootNamepace, project.FullFileName, project.MsBuildProject);
                    }
                }
                

                ienum = projects.GetEnumerator();
                while (ienum.MoveNext())
                {
                    /* Projects cannot be saved until multiple passes have been performed over all the projects to allow
                     * template transforms to replace all GUIDs with autogenerated GUID identifiers, and ProjectReferences
                     * to be updated to reflect those modified GUIDs */
                    ProjectEx project = ienum.Current.Value.Project;

                    //project.DefaultToolsVersion = TargetToolsVersion;
                    project.TargetToolsVersion = TargetToolsVersion;
                    string assemblyName = project.GetEvaluatedProperty("AssemblyName");
                    project.MsBuildProject.Save(ienum.Current.Value.DestinationFile, Encoding.UTF8);
                }

                if (!string.IsNullOrEmpty(solutionFile))
                {
                    WiXSolution sln = new WiXSolution();
                    sln.Shortcut = shortcutType;
                    sln.ParentDirectoryRef = parentDirectoryRef;
                    sln.FragmentIncludeFiles = fragmentIncludeFiles;
                    sln.ComponentIncludeFiles = componentIncludeFiles;

                    ienum = projects.GetEnumerator();
                    while (ienum.MoveNext())
                    {
                        sln.Add(new SolutionObject(ienum.Current.Value.Project));
                    }

                    if (extraSlnFiles != null && extraSlnFiles.Length > 0)
                    {
                        foreach (ITaskItem extraSlnFile in extraSlnFiles)
                        {
                            sln.Add(new SolutionObject(new SolutionFile(extraSlnFile)));
                        }
                    }

                    if (string.IsNullOrEmpty(wxsFile))
                    {
                        sln.Save(solutionFile);
                    }
                    else
                    {
                        if (componentGuid == default(Guid))
                        {
                            Log.LogError("ComponentGuid is required when a WiX output is specified");
                        }
                        else
                        {
                            sln.Save(solutionFile, wxsFile, componentGuid);
                        }
                    }
                }
                else
                {
                    //Warn that certain properties are ignored if a solution isn't specified.
                    if(!string.IsNullOrEmpty(wxsFile))
                    {
                        Log.LogWarning("Not generating WiX source {0}; WiX files are not generated unless a Visual Studio Solution is also generated.",
                            wxsFile);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                this.Log.LogErrorFromException(e, true, true, null);
                return false;
            }
        }

        private bool ValidateInputs()
        {
            if (srcProjects.Length != destProjects.Length)
            {
                Log.LogError("DestinationFiles and SourceFiles must have the same length.");
                return false;
            }
            return true;
        }

        private void BuildGuidDictionary()
        {
            if (guidReplacements != null)
            {
                foreach (ITaskItem r in guidReplacements)
                {
                    try
                    {
                        Guid guid = new Guid(r.ItemSpec);
                        string replaceWith = r.GetMetadata("ReplaceWith");
                        if (string.IsNullOrEmpty(replaceWith))
                        {
                            Log.LogWarning("Not performing Guid replacements for Guid: \"{0}\"; RecplaceWith metadata not specified.", r.ItemSpec);
                        }
                        else
                        {
                            guidDictionary.Add(guid, replaceWith);
                        }
                    }
                    catch (FormatException)
                    {
                        Log.LogWarning("Not performing Guid replacements for ill-formatted Guid: \"{0}\".", r.ItemSpec);
                    }
                }
            }
        }

        private void ReplaceGuidPropertyIfExists(ProjectEx project, string propertyName)
        {
            string value = project.GetEvaluatedProperty(propertyName);
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    Guid g = new Guid(value);
                    string replaceWith;
                    if (guidDictionary.TryGetValue(g, out replaceWith))
                    {
                        //VS Guid replacements don't include braces
                        replaceWith = "{" + replaceWith + "}";
                        project.MsBuildProject.SetProperty(propertyName, replaceWith.ToUpper());
                    }
                }
                catch (FormatException)
                {
                    Log.LogWarning("Project {0} has specified an {1} property not in the format of a Guid.", project.FullFileName, propertyName);
                }
            }
        }

        private void ReplaceGuidMetadataIfExists(_BE.ProjectItem item, string metadataName)
        {
            string value = item.GetMetadata(metadataName).EvaluatedValue;
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    Guid g = new Guid(value);
                    string replaceWith;
                    if (guidDictionary.TryGetValue(g, out replaceWith))
                    {
                        //VS Guid replacements don't include braces
                        replaceWith = "{" + replaceWith + "}";
                        item.SetMetadataValue(metadataName, replaceWith);
                    }
                }
                catch (FormatException)
                {
                    Log.LogWarning("Item {0} has specified {1} metadata not in the format of a Guid.", item.EvaluatedInclude, metadataName);
                }
            }
        }

        internal class TransFormProject
        {
            string m_transformType;
            Version m_version;
            string m_fromProjPath;
            string m_toProjPath;
            string m_targetToolsVersion;
            XsltArgumentList m_argList;

            internal TransFormProject(string transformType, Version version, string fromProjectPath, string toProjectPath, string targetToolsVersion)
            {
                m_transformType = transformType;
                m_version = version;
                m_fromProjPath = fromProjectPath;
                m_toProjPath = toProjectPath;
                m_targetToolsVersion = targetToolsVersion;
                m_argList = ConstructXslParams();
            }

            private void ApplyXsltFile(string xsltFile)
            {
                ApplyTransform(new XmlTextReader(new StreamReader(xsltFile)));
            }

            private void ApplyXsltString(string xsltString)
            {
                ApplyTransform(new XmlTextReader(new StringReader(xsltString)));
            }

            private XsltArgumentList ConstructXslParams()
            {
                XsltArgumentList argList = new XsltArgumentList();

                argList.AddParam("TransformType", "",  m_transformType);
                argList.AddParam("ToolsVersion", "",   m_targetToolsVersion);

                argList.AddParam("MajorVersion", "",   m_version.Major.ToString());
                argList.AddParam("MinorVersion", "",   m_version.Minor.ToString());
                argList.AddParam("BuildNumber", "",    m_version.Build.ToString());
                argList.AddParam("RevisionNumber", "", m_version.Revision.ToString());
                argList.AddParam("ShortVersion", "",   m_version.ToString(2));
                argList.AddParam("FullVersion", "",    m_version.ToString(4));

                return argList;
            }

            private void ApplyTransform(XmlReader xslReader)
            {
                XmlDocument document = new XmlDocument();
                document.Load(xslReader);

                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load(document);

                XmlTextReader fromProject = new XmlTextReader(m_fromProjPath);

                StringWriter stringWriter = new StringWriter();
                XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);

                xslt.Transform(
                    fromProject,
                    m_argList,
                    xmlWriter);

                xmlWriter.Close();
                fromProject.Close();
                //stringWriter.Close();
                //projReader.Close();

                // The XslCompiledTransform will add empty xmlns attributes
                // to new elements.  This hack will remove those.
                Regex regex = new Regex(" xmlns=\"\"");
                string outputXmlString = regex.Replace(stringWriter.ToString(), "");

                stringWriter.Close();

                // This hack will remove empty elements created
                // by the transform.  It will also format the
                // document in a pretty way.
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(outputXmlString);

                RemoveEmptyElements(xdoc.DocumentElement);

                xdoc.Save(m_toProjPath);
            }

            private void RemoveEmptyElements(XmlElement elementNode)
            {
                List<XmlElement> removeChildren = new List<XmlElement>();

                foreach (XmlNode child in elementNode.ChildNodes)
                {
                    if (child is System.Xml.XmlElement)
                    {
                        if ((child.Name == "PropertyGroup" || child.Name == "ItemGroup") &&
                            (child.ChildNodes == null || child.ChildNodes.Count == 0))
                        {
                            removeChildren.Add(child as XmlElement);
                        }
                        else
                        {
                            RemoveEmptyElements(child as XmlElement);
                        }
                    }
                }

                foreach (XmlElement element in removeChildren)
                {
                    elementNode.RemoveChild(element);
                }
            }

            public void Transform(string preTransform, string postTransform)
            {
                if (!string.IsNullOrEmpty(preTransform))
                {
                    ApplyXsltFile(preTransform);
                }

                if (m_fromProjPath.Contains("vbproj"))
                {
                    ApplyXsltString(BuildTaskResource.depot2sdk_vbproj);
                }
                else
                {
                    ApplyXsltString(BuildTaskResource.depot2sdk_csproj);
                }

                if (!string.IsNullOrEmpty(postTransform))
                {
                    ApplyXsltFile(postTransform);
                }
            }
        }


        #region MSBuild Task Parameters
        [Required]
        public string TargetToolsVersion { get; set; }

        [Required]
        public bool RunSDKTransform { get; set; }

        [Required]
        public bool RunTemplateTransform { get; set; }

        [Required]
        public ITaskItem[] SourceProjects
        {
            get { return srcProjects; }
            set { srcProjects = value; }
        }

        [Required]
        public ITaskItem[] DestinationProjects
        {
            get { return destProjects; }
            set { destProjects = value; }
        }

        public string[] TerminalTargets
        {
            get { return terminalTargets; }
            set { terminalTargets = value; }
        }

        public ITaskItem[] GuidReplacements
        {
            get { return guidReplacements; }
            set { guidReplacements = value; }
        }

        #region VersionRegion
        int major;
        int minor;
        int build;
        int revision;

        [Required]
        public int MajorVersion
        {
            set { major = value; }
        }

        [Required]
        public int MinorVersion
        {
            set { minor = value; }
        }

        [Required]
        public int BuildNumber
        {
            set { build = value; }
        }

        [Required]
        public int RevisionNumber
        {
            set { revision = value; }
        }
        #endregion

        public string ComponentGuid
        {
            get { return componentGuid.ToString(); }
            set { componentGuid = new Guid(value); }
        }

        public ITaskItem[] ExtraSolutionFiles
        {
            get { return extraSlnFiles; }
            set { extraSlnFiles = value; }
        }

        public string SolutionFile
        {
            get { return solutionFile; }
            set { solutionFile = value; }
        }

        public string WxsFile
        {
            get { return wxsFile; }
            set { wxsFile = value; }
        }

        public string ShortcutType
        {
            get { return shortcutType.ToString(); }
            set
            {
                if (value.ToUpper() == "FOLDER")
                {
                    shortcutType = WiXSolution.ShortcutType.FOLDER;
                }
                else if (value.ToUpper() == "DOTSLN")
                {
                    shortcutType = WiXSolution.ShortcutType.DOTSLN;
                }
                else if (value.ToUpper() == "NONE")
                {
                    shortcutType = WiXSolution.ShortcutType.NONE;

                    Log.LogWarning("Unexpected ShortcutType \"{0}\".  Defaulting to \"NONE\"", value);
                }
                else
                {
                    shortcutType = WiXSolution.ShortcutType.NONE;
                }
            }
        }

        public string ParentDirectoryRef
        {
            set { parentDirectoryRef = value; }
        }

        #region IncludeRegion
        ITaskItem[] fragmentIncludeFiles;
        ITaskItem[] componentIncludeFiles;

        public ITaskItem[] FragmentIncludeFiles
        {
            set { fragmentIncludeFiles = value; }
        }

        public ITaskItem[] ComponentIncludeFiles
        {
            set { componentIncludeFiles = value; }
        }
        #endregion
        #endregion

        /// <summary>
        /// Convert any Assembly References to Project References. If a project references and assembly that is built by
        /// another project in this solution.  Then replace the assembly reference by a project reference.
        /// </summary>
        /// <param name="project">The project for which to construct ProjectReferences</param>
        private void FixReferences(ProjectEx project)
        {
            foreach (_BE.ProjectItem buildItem in project.MsBuildProject.GetItems("Reference"))
            {
                if (projects.ContainsKey(buildItem.Xml.Include))
                {
                    ProjectEx referencedProject = projects[buildItem.Xml.Include].Project;

                    project.MsBuildProject.RemoveItem(buildItem);

                    string rpName = referencedProject.GetEvaluatedProperty("MSBuildProjectName");
                    string rpFullName = Path.GetFileName(referencedProject.FullFileName);
                    Dictionary<string, string> meta = new Dictionary<string,string>();

                    meta.Add("Project", referencedProject.GetEvaluatedProperty("ProjectGuid"));
                    meta.Add("Name", rpName);

                    project.MsBuildProject.AddItem("ProjectReference", project.PathToSolution + referencedProject.SolutionRelativePath, meta);
                }
            }
        }

        #region VS Source Control Plugin Metadata Removal
        private void RemoveSourceControlAnnotations(ProjectEx project)
        {
            List<_BE.ProjectProperty> rem = new List<_BE.ProjectProperty>();

            foreach (_BE.ProjectProperty prop in project.MsBuildProject.Properties)
            {
                switch (prop.Name)
                {
                    case "SccProjectName":
                    case "SccAuxPath":
                    case "SccLocalPath":
                    case "SccProvider":
                        rem.Add(prop);
                        break;
                }
            }
        }
        #endregion
    }
}
