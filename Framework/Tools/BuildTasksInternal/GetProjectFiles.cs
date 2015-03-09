using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.Build.BuildEngine;
using _BE=Microsoft.Build.Evaluation;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;

using Task = Microsoft.Build.Utilities.Task;

namespace Microsoft.SPOT.Tasks.Internal
{
    public class GetProjectChildFiles : Task
    {
        private ITaskItem[] projectFiles;
        private ITaskItem[] projectContents;
        private ITaskItem[] projectDependencies;
        private ITaskItem[] guidReplacements;

        public override bool Execute()
        {
            try
            {
                List<ITaskItem> contentsList = new List<ITaskItem>();
                List<ITaskItem> dependencyList = new List<ITaskItem>();
                List<ITaskItem> templateList = new List<ITaskItem>();
                HashSet<Guid>   guids = new HashSet<Guid>();

                ProjectEx.Task = this;

                foreach (ITaskItem projectItem in projectFiles)
                {
                    string currentDirectory = Environment.CurrentDirectory;

                    ProjectEx project = new ProjectEx(projectItem);

                    project.Load();

                    Environment.CurrentDirectory = Path.GetDirectoryName(
                        project.FullFileName);

                    //Retrieve the list of Guids defined in the project, for later replacement.
                    string projectGuid = project.GetEvaluatedProperty("ProjectGuid");
                    if (!string.IsNullOrEmpty(projectGuid))
                    {
                        try
                        {
                            Guid guidProject = new Guid(projectGuid);
                            if (!guids.Contains(guidProject))
                            {
                                guids.Add(guidProject);
                            }
                        }
                        catch (FormatException)
                        {
                            Log.LogWarning("Project {0} has specified an ProjectGuid property not in the format of a Guid.", projectItem.ItemSpec);
                        }
                    }

                    string emulatorId = project.GetEvaluatedProperty("EmulatorId");
                    if (!string.IsNullOrEmpty(emulatorId))
                    {
                        try
                        {
                            Guid guidEmulator = new Guid(emulatorId);
                            if (!guids.Contains(guidEmulator))
                            {
                                guids.Add(guidEmulator);
                            }
                        }
                        catch (FormatException)
                        {
                            Log.LogWarning("Project {0} has specified an EmulatorId property not in the format of a Guid.", projectItem.ItemSpec);
                        }
                    }

                    //Select all the files referenced by the project.
                    foreach (string groupName in ProjectEx.FileGroups)
                    {
                        foreach (_BE.ProjectItem buildItem in project.MsBuildProject.GetItemsIgnoringCondition(groupName))
                        {
                            if (TransmorgificationUtilities.IsInRestrictedList(buildItem.Xml.Include))
                            {
                                Log.LogMessage("Skipping restricted file {0} in project {1}", buildItem.EvaluatedInclude, projectItem.ItemSpec);
                                continue;
                            }
                            else if (!File.Exists(buildItem.EvaluatedInclude)) // .GetMetadata("FullPath").EvaluatedValue))
                            {
                                Log.LogWarning("Cannot find file {0} referenced in project {1}", buildItem.EvaluatedInclude, projectItem.ItemSpec);
                                continue;
                            }

                            string fileName = buildItem.EvaluatedInclude;
                            if (Path.IsPathRooted(fileName))
                            {
                                Log.LogWarning("Project {0} references file {1} by absolute path, which is unsuitable for samples and templates", projectItem.ItemSpec, fileName);
                            }

                            TaskItem file = new TaskItem(fileName);
                            bool doReplacements = TransmorgificationUtilities.ValidMimeTypeForReplacements(buildItem.Xml.Include);
                            file.CopyMetadata(buildItem);
                            file.SetMetadata("DoReplacements", doReplacements.ToString().ToLowerInvariant());
                            file.SetMetadata("ItemCollection", buildItem.ItemType);
                            file.SetMetadata("ParentProject", projectItem.ItemSpec);
                            file.SetMetadata("ProjectDir", projectItem.GetMetadata("RelativeDir"));
                            string rootNamespace = project.GetEvaluatedProperty("RootNamespace");
                            if (rootNamespace == null)
                            {
                                rootNamespace = "";
                            }
                            file.SetMetadata("RootNamespace", rootNamespace);

                            contentsList.Add(file);
                        }
                    }

                    string templateIconFile = project.GetEvaluatedProperty("TemplateIconFile");
                    if (!string.IsNullOrEmpty(templateIconFile))
                    {
                        TaskItem file = CreateExtraFile(templateIconFile, projectItem);
                        if (file != null)
                        {
                            contentsList.Add(file);
                        }
                    }

                    foreach (string extraFile in project.ExtraFiles)
                    {
                        TaskItem file = CreateExtraFile(extraFile, projectItem);
                        if (file != null)
                        {
                            contentsList.Add(file);
                        }
                    }

                    /*
                    if (project.PreTransform != null)
                    {
                        dependencyList.Add(new TaskItem(project.PreTransform));
                    }

                    if (project.PostTransform != null)
                    {
                        dependencyList.Add(new TaskItem(project.PostTransform));
                    }
                    */

                    Environment.CurrentDirectory = currentDirectory;
                }


                List<ITaskItem> replacements = new List<ITaskItem>();
                int guidNum = 1;
                foreach (Guid guid in guids)
                {
                    TaskItem guidItem = new TaskItem(guid.ToString("D"));
                    guidItem.SetMetadata("ReplaceWith", "$guid" + guidNum.ToString() + "$");
                    replacements.Add(guidItem);
                    guidNum++;
                    if (guidNum > 10)
                    {
                        break;
                    }
                }

                projectContents = contentsList.ToArray();
                projectDependencies = dependencyList.ToArray();
                guidReplacements = replacements.ToArray();
                return true;
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e, true, true, null);
                return false;
            }
        }

        private TaskItem CreateExtraFile(string extraFile, ITaskItem projectItem)
        {
            string path = BuildTaskUtility.ExpandEnvironmentVariables(extraFile);
            string fullPath = Path.GetFullPath(path);
            if (!File.Exists(fullPath))
            {
                Log.LogWarning("Cannot find extra file {0} for project {1}", extraFile, projectItem.ItemSpec);
                return null;
            }

            TaskItem file = new TaskItem(path);
            bool doReplacements = TransmorgificationUtilities.ValidMimeTypeForReplacements(extraFile);
            file.SetMetadata("DoReplacements", doReplacements.ToString().ToLowerInvariant());
            file.SetMetadata("ItemCollection", "Extra");
            file.SetMetadata("ParentProject", projectItem.ItemSpec);
            file.SetMetadata("ProjectDir", projectItem.GetMetadata("RelativeDir"));

            return file;
        }

        [Required]
        [Output]
        public ITaskItem[] ProjectFiles
        {
            get { return projectFiles; }
            set { projectFiles = value; }
        }

        [Output]
        public ITaskItem[] ProjectContents
        {
            get { return projectContents; }
        }

        [Output]
        public ITaskItem[] ProjectDependentFiles
        {
            get { return projectDependencies; }
        }

        [Output]
        public ITaskItem[] GuidReplacements
        {
            get { return guidReplacements; }
        }
    }

    internal static class TaskItemExtensions
    {
        /// <summary>
        /// Copies all metadata from the specified buildItem
        /// </summary>
        /// <param name="item">The destination for the metadata copy</param>
        /// <param name="buildItem">The source for the metadata copy</param>
        public static void CopyMetadata(this TaskItem item, _BE.ProjectItem buildItem)
        {
            foreach (_BE.ProjectMetadata md in buildItem.Metadata)
            {
                item.SetMetadata(md.Name, md.EvaluatedValue);
            }
        }
    }
}
