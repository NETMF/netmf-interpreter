using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using _BE = Microsoft.Build.Evaluation;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.SPOT.WiX;

using WiXDirectory = Microsoft.SPOT.WiX.Directory;
using WiXFile = Microsoft.SPOT.WiX.File;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace Microsoft.SPOT.Tasks.Internal
{
    public class SolutionFile
    {
        private string file;
        private bool include = true;
        private bool dotsln = false;

        public SolutionFile(ITaskItem slnFileItem)
        {
            file = slnFileItem.ItemSpec;

            string excludeMetadataValue = slnFileItem.GetMetadata("ExcludeFromSolutionFile");
            if (excludeMetadataValue == "true")
            {
                include = false;
            }
        }

        public SolutionFile(string fileName, bool includeInSolution)
        {
            file = fileName;
            include = includeInSolution;
        }

        public static SolutionFile CreateDotSln(string fileName)
        {
            SolutionFile sln = new SolutionFile(fileName, false);
            sln.dotsln = true;
            return sln;
        }

        public string File
        {
            get { return file; }
        }

        public bool IncludeInSolution
        {
            get { return include; }
        }

        public bool IsDotSln
        {
            get { return dotsln; }
        }
    }

    public class SolutionObject
    {
        public ProjectEx project;
        public SolutionFile file;

        public SolutionObject(ProjectEx project)
        {
            this.project = project;
        }

        public SolutionObject(SolutionFile file)
        {
            this.file = file;
        }

        public object Object
        {
            get
            {
                if(project != null)
                {
                    return project;
                }

                return file;
            }
        }
    }

    public class Solution : List<SolutionObject>
    {
        public void Save(string solutionFile)
        {
            string allProjectDecl = "";
            string allConfig = "";
            string[] otherList = { "ActiveCfg", "Build.0", "Deploy.0" };

            string allSolutionDecl = "";

            foreach (SolutionObject slnObj in this)
            {
                if (slnObj.Object is ProjectEx)
                {
                    ProjectEx project = slnObj.Object as ProjectEx;
                    string projectDecl = BuildTaskResource.ProjectDeclarationTemplate;

                    string projectGuid = project.GetEvaluatedProperty("ProjectGuid").ToUpper();

                    projectDecl = projectDecl.Replace("<PROJECTNAME>", project.GetEvaluatedProperty("MSBuildProjectName"));
                    projectDecl = projectDecl.Replace("<PROJECTPATH>", project.SolutionRelativePath);
                    projectDecl = projectDecl.Replace("<PROJECTGUID>", projectGuid);

                    allProjectDecl += projectDecl;

                    foreach (string buildFlavor in project.BuildFlavors)
                    {
                        foreach (string other in otherList)
                        {
                            if (other == "Deploy.0" && !project.Deploy)
                            {
                                continue;
                            }

                            string projectConfig = BuildTaskResource.ProjectConfigurationPlatformTemplate;
                            projectConfig = projectConfig.Replace("<PROJECTGUID>", projectGuid);
                            projectConfig = projectConfig.Replace("<BUILDFLAVOR>", buildFlavor);
                            projectConfig = projectConfig.Replace("<OTHERINFO>", other);

                            allConfig += projectConfig;
                        }
                    }
                }
                else
                {
                    SolutionFile slnFileObj = slnObj.Object as SolutionFile;

                    if (slnFileObj.IncludeInSolution)
                    {
                        if (allSolutionDecl != "")
                        {
                            allSolutionDecl += "\r\n";
                        }
                        allSolutionDecl += BuildTaskResource.SolutionFileDeclarationTemplate.Replace(
                            "<SOLUTIONFILENAME>", Path.GetFileName(slnFileObj.File));
                    }
                }
            }

            if (allSolutionDecl != "")
            {
                string solutionProjectDecl = BuildTaskResource.SolutionProjectDeclarationTemplate;
                solutionProjectDecl = solutionProjectDecl.Replace(
                    "<SOLUTIONFILEDECLARATIONSECTION>",
                    allSolutionDecl);

                solutionProjectDecl = solutionProjectDecl.Replace(
                    "<SOLUTIONITEMSGUID>",
                    "{" + Guid.NewGuid().ToString().ToUpper() + "}");

                allProjectDecl += solutionProjectDecl;
            }

            string solutionBody = BuildTaskResource.SolutionTemplate;

            solutionBody = solutionBody.Replace("<PROJECTDECL>", allProjectDecl);
            solutionBody = solutionBody.Replace("<GLOBALPROJECTCONFIGURATIONPLATFORMS>", allConfig);

            StreamWriter writer = new StreamWriter(solutionFile);
            writer.Write(solutionBody);
            writer.Close();
        }
    }

    public class WiXSolution : Solution
    {
        public enum ShortcutType { NONE, FOLDER, DOTSLN };

        private ShortcutType shortcut = ShortcutType.NONE;
        private string solutionName;
        private string parentDirectoryRef = "SamplesDir";

        private Fragment m_SolutionFragment = null;

        public ShortcutType Shortcut
        {
            get { return shortcut; }
            set { shortcut = value; }
        }

        public string ParentDirectoryRef
        {
            get { return parentDirectoryRef; }
            set { parentDirectoryRef = value; }
        }

        public System.Xml.XmlDocument OwnerDocument
        {
            get
            {
                if (m_SolutionFragment == null)
                {
                    return null;
                }

                return m_SolutionFragment.Element.OwnerDocument;
            }
        }

        public void Save(
            string slnFile,
            string wxsFile,
            Guid componentGuid)
        {
            base.Save(slnFile);
            this.Add(new SolutionObject(SolutionFile.CreateDotSln(slnFile)));

            Hashtable componentFiles = new Hashtable();

            solutionName = Path.GetFileNameWithoutExtension(slnFile);

            Fragment fragment = new Fragment("Fragment_" + solutionName);
            m_SolutionFragment = fragment;

            if (fragmentIncludeFiles != null)
            {
                foreach (ITaskItem fragmentInclude in fragmentIncludeFiles)
                {
                    fragment.PrependInclude(fragmentInclude.ItemSpec);
                }
            }

            DirectoryRef samplesRef = new DirectoryRef(fragment, parentDirectoryRef);

            WiXDirectory solutionDir = new WiXDirectory(samplesRef, solutionName);
            solutionDir.Id = solutionName + "_" + solutionDir.Id;

            DirectoryRef tempRef = new DirectoryRef(fragment, "TEMPFOLDERINSTALLDIR");

            WiXDirectory tempSlnDir = new WiXDirectory(tempRef, solutionName);
            tempSlnDir.Id = solutionName + "_" + tempSlnDir.Id;

            Component component = new Component(tempSlnDir,
                "Component_" + solutionName,
                componentGuid);

            if (shortcut == ShortcutType.FOLDER)
            {
                Shortcut folderShortcut = new Shortcut(
                    component,
                    solutionDir,
                    solutionName,
                    new DirectoryRef(fragment, "ProgramMenuDir"));

                folderShortcut.Id = solutionName + "_" + folderShortcut.Id;
            }

            foreach (SolutionObject slnObj in this)
            {
                if (slnObj.Object is ProjectEx)
                {
                    #region ProjectEx
                    ProjectEx project = slnObj.Object as ProjectEx;

                    // Add the project folder to the WiX file
                    string projectFolderName = Path.GetFileNameWithoutExtension(project.FullFileName);
                    WiXDirectory projectDirectory = new WiXDirectory(solutionDir, projectFolderName);
                    projectDirectory.Id = solutionName + "_" + projectDirectory.Id;
                    component.CreateFolder(projectDirectory);

                    WiXFile projectFile = new WiXFile(component, Path.Combine(Path.GetDirectoryName(wxsFile), Path.GetFileName(project.FullFileName)));
                    projectFile.CopyFile(projectDirectory.Id);

                    // Add subfolders
                    foreach (_BE.ProjectItem folderItem in project.MsBuildProject.GetItems("Folder"))
                    {
                        string addDirPath = folderItem.Xml.Include;
                        if (addDirPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                        {
                            addDirPath = addDirPath.Substring(0, addDirPath.Length - 1);
                        }

                        AddFolder(
                                projectDirectory,
                                component,
                                addDirPath);

                    }

                    // Loop through all known item groups that map to files
                    // and add those files.

                    foreach (string groupName in ProjectEx.FileGroups)
                    {
                        foreach (_BE.ProjectItem buildItem in project.MsBuildProject.GetItems(groupName))
                        {
                            AddFile(
                                buildItem.Xml.Include,
                                project,
                                projectDirectory,
                                component,
                                componentFiles);
                        }
                    }

                    foreach (string extraFile in project.ExtraFiles)
                    {
                        AddFile(
                            extraFile,
                            project,
                            projectDirectory,
                            component,
                            componentFiles);
                    }
                    #endregion
                }
                else
                {
                    #region SolutionFile
                    SolutionFile slnFileObj = slnObj.Object as SolutionFile;
                    WiXFile slnFileElem = new WiXFile(component, slnFileObj.File);
                    slnFileElem.Id = solutionName + "_" + slnFileElem.Id;
                    slnFileElem.CopyFile(solutionDir.Id);

                    if (slnFileObj.IsDotSln && shortcut == ShortcutType.DOTSLN)
                    {
                        Shortcut dotSlnShortcut = new Shortcut(
                            slnFileElem,
                            Path.GetFileNameWithoutExtension(slnFileElem.Name),
                            new DirectoryRef(fragment, "ProgramMenuDir"));

                        dotSlnShortcut.Id = solutionName + "_" + dotSlnShortcut.Id;
                    }
                    #endregion
                }
            }

            if (componentIncludeFiles != null)
            {
                foreach (ITaskItem componentInclude in componentIncludeFiles)
                {
                    component.AppendInclude(componentInclude.ItemSpec);
                }
            }

            fragment.Element.OwnerDocument.Save(wxsFile);
        }

        private void AddFile(string fileName, ProjectEx project, WiXDirectory projectDirectory, Component component, Hashtable componentFiles)
        {
            string fullPath = project.GetFullPath(fileName);

            WiXDirectory fileParent = null;
            // If we have any project sub folders add them now.
            string addDirPath = Path.GetDirectoryName(fileName);

            fileParent = AddFolder(
                projectDirectory,
                component,
                addDirPath);

            WiXFile wixFile = null;
            if (componentFiles.ContainsKey(fullPath))
            {
                wixFile = componentFiles[fullPath] as WiXFile;
            }
            else
            {
                wixFile = new WiXFile(component, fullPath);
                componentFiles.Add(fullPath, wixFile);
            }

            // Have WiX copy the file to the final destination
            wixFile.CopyFile(fileParent.Id, Path.GetFileName(fileName));
        }

        private WiXDirectory AddFolder(WiXDirectory topDirectory, Component component, string path)
        {
            if (path == "") return topDirectory;

            // Recursion here:
            WiXDirectory parent = AddFolder(topDirectory, component, Path.GetDirectoryName(path));

            // If we already added this folder then reuse it.
            WiXDirectory thisFolder = parent.GetDirectoryFromName(Path.GetFileName(path));

            if (thisFolder == null)
            {
                thisFolder = new WiXDirectory(parent, Path.GetFileName(path));
                thisFolder.Id = solutionName + "_" + thisFolder.Id;
                component.CreateFolder(thisFolder);
            }

            return thisFolder;
        }

        #region IncludeRegion
        ITaskItem[] fragmentIncludeFiles;
        ITaskItem[] componentIncludeFiles;

        public ITaskItem[] FragmentIncludeFiles
        {
            get { return fragmentIncludeFiles; }
            set { fragmentIncludeFiles = value; }
        }

        public ITaskItem[] ComponentIncludeFiles
        {
            get { return componentIncludeFiles; }
            set { componentIncludeFiles = value; }
        }
        #endregion
    }
}
