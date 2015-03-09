using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using _BE=Microsoft.Build.Evaluation;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;

using Task = Microsoft.Build.Utilities.Task;

namespace Microsoft.SPOT.Tasks.Internal
{
    public class GenerateVSTemplate : Task
    {
        private class Folder
        {
            public string Name;

            public Dictionary<string, Folder> SubFolders = new Dictionary<string, Folder>();
            public List<ITaskItem> Files = new List<ITaskItem>();
        }

        private ITaskItem[] projectFiles;
        private ITaskItem[] projectContents;
        private ITaskItem[] vstemplateFiles;

        private readonly char[] DirectorySeparators = new char[] { Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar };

        /// <summary>
        /// Generates a VSTemplate file for the given projects, using select properties and ItemMetaData to
        /// define the resulting output.
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            if (projectFiles == null || projectFiles.Length == 0)
            {
                vstemplateFiles = new TaskItem[0];
                return true;
            }
            if (!ValidateInputs())
            {
                return false;
            }

            bool result = true;
            for (int i = 0; i < projectFiles.Length; i++)
            {
                //TODO... all paths here need to be relative to the project file... The code currently assumes
                //that the vstproj is in the same directory as the csproj file.

                //Get the list of files that came from this project:
                IEnumerable<ITaskItem> contents = from file in projectContents
                                       where file.GetMetadata("ParentProject") == projectFiles[i].ItemSpec
                                       select file;
                
                //Construct a file system hierarchy of folders and files that are part of the project
                Folder hierarchy = new Folder();
                foreach (ITaskItem file in contents)
                {
                    AddFileToHierarchy(hierarchy, projectFiles[i], file);
                }

                if(!GenerateVSTemplateFile(projectFiles[i], vstemplateFiles[i].ItemSpec, hierarchy))
                {
                    result = false;
                }
            }

            return result;
        }

        private bool ValidateInputs()
        {
            if (vstemplateFiles.Length != projectFiles.Length)
            {
                Log.LogError("DestinationFiles and SourceFiles must have the same length.");
                return false;
            }
            return true;
        }

        private void AddFileToHierarchy(Folder hierarchy, ITaskItem project, ITaskItem file)
        {
            Folder folder = hierarchy;
            if (Path.IsPathRooted(file.ItemSpec))
            {
                Log.LogWarning("GenerateVSTemplate expects relative paths, but was passed file {0} in project {1}. Skipping.",
                    file.ItemSpec, project.ItemSpec);
                return;
            }

            string[] components = file.ItemSpec.Split(DirectorySeparators);

            for (int j = 0; j < components.Length - 1; j++)
            {
                Folder subfolder;
                if (folder.SubFolders.ContainsKey(components[j]))
                {
                    subfolder = folder.SubFolders[components[j]];
                }
                else
                {
                    subfolder = new Folder();
                    subfolder.Name = components[j];
                    folder.SubFolders.Add(components[j], subfolder);
                }
                folder = subfolder;
            }
            folder.Files.Add(file);
        }

        #region MSBuild Task Parameters
        [Required]
        public ITaskItem[] ProjectContents
        {
            get { return projectContents; }
            set { projectContents = value; }
        }

        [Required]
        public ITaskItem[] ProjectFiles
        {
            get { return projectFiles; }
            set { projectFiles = value; }
        }

        [Required]
        public ITaskItem[] VSTemplateFiles
        {
            get { return vstemplateFiles; }
            set { vstemplateFiles = value; }
        }
        #endregion

        #region Project to Template transformation
        /// <summary>
        /// Adapted from VS2008 SP1's Export Template code that uses VS objects from
        /// env\CommunityTools\VSTemplateGen to pure MSBuild code. Also upgraded the results
        /// to VS2008 format rather than VS2005 format templates.
        /// </summary>
        /// <param name="proj"></param>
        /// <param name="templateName"></param>
        /// <param name="templateDescription"></param>
        /// <param name="templateLanguage">The language the template is implemented in; commonly "CSharp", "VisualBasic", or "Web"</param>
        /// <param name="templateSubType">The template subtype, usually not specified. For "Web" projects, it may be "CSharp" or "VisualBasic"</param>
        /// <returns></returns>
        private bool GenerateVSTemplateFile(ITaskItem project, string vstemplateFile, Folder projectContents)
        {
            bool result = true;
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode xmlRootNode;
            XmlAttribute xmlVersionAttribute;
            XmlAttribute xmlTypeAttribute;
            XmlNode xmlTemplateContentNode;
            XmlNode xmlProjectFileNode;
            XmlAttribute xmlReplaceParametersNode;
            XmlNode xmlHeaderNode;
            XmlNode xmlNameNode;
            XmlNode xmlDescriptionNode;
            XmlNode xmlIconNode;
            XmlNode xmlTemplateIDNode;
            XmlNode xmlProjectTypeNode;
            XmlNode xmlSubTypeNode;
            XmlNode xmlSortOrderNode;
            XmlNode xmlRequiresNewFolderNode;
            XmlNode xmlDefaultNameNode;
            XmlNode xmlInitializeNode;
            XmlNode xmlEnableEditOfLocationFieldNode;
            XmlNode xmlEnableBrowseButtonNode;

            //Load the source project to get some template description properties:
            string rootNamespace = null;
            string templateNameString = null;
            string templateNamePackage = null;
            string templateNameResID = null;
            string templateDescriptionString = null;
            string templateDescriptionPackage = null;
            string templateDescriptionResID = null;
            string templateIconFile = null;
            string templateIconPackage = null;
            string templateIconResID = null;
            string templateID = null;
            string templateDefaultName = null;
            string templateType = null;
            string templateSubType = null;
            string templateRequiredFrameworkVersion = null;
            string templateSortOrder = null;
            {
                _BE.Project proj = new _BE.Project(project.ItemSpec);

                rootNamespace = proj.GetPropertyValue("RootNamespace");
                templateNameString = proj.GetPropertyValue("TemplateNameString");
                templateNamePackage = proj.GetPropertyValue("TemplateNamePackage");
                templateNameResID = proj.GetPropertyValue("TemplateNameResID");
                templateDescriptionString = proj.GetPropertyValue("TemplateDescriptionString");
                templateDescriptionPackage = proj.GetPropertyValue("TemplateDescriptionPackage");
                templateDescriptionResID = proj.GetPropertyValue("TemplateDescriptionResID");
                templateIconFile = proj.GetPropertyValue("TemplateIconFile");
                templateIconPackage = proj.GetPropertyValue("TemplateIconPackage");
                templateIconResID = proj.GetPropertyValue("TemplateIconResID");
                templateID = proj.GetPropertyValue("TemplateID");
                templateDefaultName = proj.GetPropertyValue("TemplateDefaultName");
                templateType = proj.GetPropertyValue("TemplateProjectType");
                templateSubType = proj.GetPropertyValue("TemplateProjectSubType");
                templateRequiredFrameworkVersion = proj.GetPropertyValue("TemplateRequiredFrameworkVersion");
                templateSortOrder = proj.GetPropertyValue("TemplateSortOrder");
            }

            if (string.IsNullOrEmpty(rootNamespace))
            {
                Log.LogError("Project {0} doesn't have the <RootNamespace> property specified.", project.ItemSpec);
                result = false;
            }
            if (string.IsNullOrEmpty(templateID))
            {
                Log.LogError("Project {0} doesn't have the <TemplateID> property specified.", project.ItemSpec);
                result = false;
            }
            if (string.IsNullOrEmpty(templateNamePackage) ^ string.IsNullOrEmpty(templateNameResID))
            {
                Log.LogWarning("Project {0} didn't specify both <TemplateNamePackage> and <TemplateNameResID> properties", project.ItemSpec);
                templateNamePackage = templateNameResID = null;
            }
            if (string.IsNullOrEmpty(templateNameString))
            {
                Log.LogError("Project {0} didn't specify <TemplateNameString> property", project.ItemSpec);
                result = false;
            }
            if (string.IsNullOrEmpty(templateDescriptionPackage) ^ string.IsNullOrEmpty(templateDescriptionResID))
            {
                Log.LogWarning("Project {0} didn't specify both <TemplateDescriptionPackage> and <TemplateDescriptionResID> properties", project.ItemSpec);
                templateDescriptionPackage = templateDescriptionResID = null;
            }
            if (string.IsNullOrEmpty(templateDescriptionPackage) && string.IsNullOrEmpty(templateDescriptionResID) && string.IsNullOrEmpty(templateDescriptionString))
            {
                Log.LogWarning("Project {0} didn't specify <TemplateDescriptionString> property", project.ItemSpec);
            }
            if (string.IsNullOrEmpty(templateIconPackage) ^ string.IsNullOrEmpty(templateIconResID))
            {
                Log.LogWarning("Project {0} didn't specify both <TemplateIconPackage> and <TemplateIconResID> properties", project.ItemSpec);
                templateIconPackage = templateIconResID = null;
            }
            if (string.IsNullOrEmpty(templateIconPackage) && string.IsNullOrEmpty(templateIconResID) && string.IsNullOrEmpty(templateIconFile))
            {
                Log.LogWarning("Project {0} didn't specify <TemplateIconFile> property", project.ItemSpec);
            }
            if (string.IsNullOrEmpty(templateType))
            {
                templateType = @"CSharp";
                Log.LogWarning("Project {0} didn't specify <TemplateProjectType> property; defaulting to {1}", project.ItemSpec, templateType);
            }
            if (string.IsNullOrEmpty(templateSortOrder))
            {
                templateSortOrder = "1000";
                Log.LogWarning("Project {0} didn't specify <TemplateSortOrder> property; defaulting to {1}", project.ItemSpec, templateSortOrder);
            }
            
            //If we had errors in parameter checking, return here:
            if (!result)
            {
                return result;
            }

            xmlRootNode = xmlDoc.CreateElement("VSTemplate");
            xmlVersionAttribute = xmlDoc.CreateAttribute("Version");
            xmlVersionAttribute.Value = "3.0.0";
            xmlRootNode.Attributes.Append(xmlVersionAttribute);

            xmlTypeAttribute = xmlDoc.CreateAttribute("Type");
            xmlTypeAttribute.Value = "Project";
            xmlRootNode.Attributes.Append(xmlTypeAttribute);

            System.Xml.XmlAttribute xmlNSAttribute = xmlDoc.CreateAttribute("xmlns");
            xmlNSAttribute.Value = "http://schemas.microsoft.com/developer/vstemplate/2005";
            xmlRootNode.Attributes.Append(xmlNSAttribute);

            System.Xml.XmlAttribute xmlNSXSiAttribute = xmlDoc.CreateAttribute("xmlns:xsi");
            xmlNSXSiAttribute.Value = "http://www.w3c.org/2001/XMLSchema-instance";
            xmlRootNode.Attributes.Append(xmlNSXSiAttribute);

            System.Xml.XmlAttribute xmlXsiSchemaLocationAttribute = xmlDoc.CreateAttribute("xsi:schemaLocation");
            xmlXsiSchemaLocationAttribute.Value = "http://schemas.microsoft.com/developer/vstemplate/2005";
            xmlRootNode.Attributes.Append(xmlXsiSchemaLocationAttribute);

            xmlRootNode = xmlDoc.AppendChild(xmlRootNode);

            xmlHeaderNode = xmlDoc.CreateElement("TemplateData");
            xmlHeaderNode = xmlRootNode.AppendChild(xmlHeaderNode);

            xmlNameNode = xmlDoc.CreateElement("Name");
            if (!string.IsNullOrEmpty(templateNamePackage) && !string.IsNullOrEmpty(templateNameResID))
            {
                XmlAttribute package = xmlDoc.CreateAttribute("Package");
                package.Value = templateNamePackage;
                xmlNameNode.Attributes.Append(package);
                XmlAttribute id = xmlDoc.CreateAttribute("ID");
                id.Value = templateNameResID;
                xmlNameNode.Attributes.Append(id);
            }
            else
            {
                xmlNameNode.InnerText = templateNameString;
            }
            xmlNameNode = xmlHeaderNode.AppendChild(xmlNameNode);

            xmlDescriptionNode = xmlDoc.CreateElement("Description");
            if (!string.IsNullOrEmpty(templateDescriptionPackage) && !string.IsNullOrEmpty(templateDescriptionResID))
            {
                XmlAttribute package = xmlDoc.CreateAttribute("Package");
                package.Value = templateDescriptionPackage;
                xmlDescriptionNode.Attributes.Append(package);
                XmlAttribute id = xmlDoc.CreateAttribute("ID");
                id.Value = templateDescriptionResID;
                xmlDescriptionNode.Attributes.Append(id);
            }
            else
            {
                xmlDescriptionNode.InnerText = templateDescriptionString;
            }
            xmlDescriptionNode = xmlHeaderNode.AppendChild(xmlDescriptionNode);

            xmlIconNode = xmlDoc.CreateElement("Icon");
            if (!string.IsNullOrEmpty(templateDescriptionPackage) && !string.IsNullOrEmpty(templateDescriptionResID))
            {
                XmlAttribute package = xmlDoc.CreateAttribute("Package");
                package.Value = templateIconPackage;
                xmlIconNode.Attributes.Append(package);
                XmlAttribute id = xmlDoc.CreateAttribute("ID");
                id.Value = templateIconResID;
                xmlIconNode.Attributes.Append(id);
            }
            else
            {
                xmlIconNode.InnerText = templateIconFile;
            }
            xmlIconNode = xmlHeaderNode.AppendChild(xmlIconNode);

            xmlTemplateIDNode = xmlDoc.CreateElement("TemplateID");
            xmlTemplateIDNode.InnerText = templateID;
            xmlTemplateIDNode = xmlHeaderNode.AppendChild(xmlTemplateIDNode);

            xmlProjectTypeNode = xmlDoc.CreateElement("ProjectType");
            xmlProjectTypeNode.InnerText = templateType;
            xmlProjectTypeNode = xmlHeaderNode.AppendChild(xmlProjectTypeNode);

            if (!string.IsNullOrEmpty(templateSubType))
            {
                xmlSubTypeNode = xmlDoc.CreateElement("ProjectSubType");
                xmlSubTypeNode.InnerText = templateSubType;
                xmlSubTypeNode = xmlHeaderNode.AppendChild(xmlSubTypeNode);
            }

            //RequiredTargetFrameworkVersion

            xmlSortOrderNode = xmlDoc.CreateElement("SortOrder");
            xmlSortOrderNode.InnerText = templateSortOrder.ToString();
            xmlSortOrderNode = xmlHeaderNode.AppendChild(xmlSortOrderNode);

            //NumberOfParentCategoriesToRollUp

            xmlRequiresNewFolderNode = xmlDoc.CreateElement("CreateNewFolder");
            xmlRequiresNewFolderNode.InnerText = "true";
            xmlRequiresNewFolderNode = xmlHeaderNode.AppendChild(xmlRequiresNewFolderNode);

            xmlDefaultNameNode = xmlDoc.CreateElement("DefaultName");
            xmlDefaultNameNode.InnerText = string.IsNullOrEmpty(templateDefaultName) ? templateNameString : templateDefaultName;
            xmlDefaultNameNode = xmlHeaderNode.AppendChild(xmlDefaultNameNode);

            xmlInitializeNode = xmlDoc.CreateElement("ProvideDefaultName");
            xmlInitializeNode.InnerText = "true";
            xmlInitializeNode = xmlHeaderNode.AppendChild(xmlInitializeNode);

            //PromptForSaveOnCreation

            xmlEnableEditOfLocationFieldNode = xmlDoc.CreateElement("LocationField");
            xmlEnableEditOfLocationFieldNode.InnerText = "Enabled";
            xmlEnableEditOfLocationFieldNode = xmlHeaderNode.AppendChild(xmlEnableEditOfLocationFieldNode);

            //EnableEditOfLocationField

            xmlEnableBrowseButtonNode = xmlDoc.CreateElement("EnableLocationBrowseButton");
            xmlEnableBrowseButtonNode.InnerText = "true";
            xmlEnableBrowseButtonNode = xmlHeaderNode.AppendChild(xmlEnableBrowseButtonNode);

            xmlTemplateContentNode = xmlDoc.CreateElement("TemplateContent");
            xmlTemplateContentNode = xmlRootNode.AppendChild(xmlTemplateContentNode);

            xmlProjectFileNode = xmlDoc.CreateElement("Project");
            xmlProjectFileNode = xmlTemplateContentNode.AppendChild(xmlProjectFileNode);

            {
                XmlAttribute xmlFileAttribute = xmlDoc.CreateAttribute("File");
                xmlFileAttribute.Value = Path.GetFileName(project.ItemSpec);
                xmlFileAttribute = xmlProjectFileNode.Attributes.Append(xmlFileAttribute);
            }

            xmlReplaceParametersNode = xmlDoc.CreateAttribute("ReplaceParameters");
            xmlReplaceParametersNode.Value = "true";
            xmlReplaceParametersNode = xmlProjectFileNode.Attributes.Append(xmlReplaceParametersNode);
            {
                result = WalkProject(project, xmlDoc, xmlProjectFileNode, projectContents);
                if (result != true)
                {
                    return result;
                }
            }

            xmlDoc.Save(vstemplateFile);

            return result;
        }

        #region Project Walking
        private bool WalkProject(ITaskItem project, XmlDocument xmlDoc, XmlNode xmlFolderNode, Folder projectContents)
        {
            foreach (ITaskItem item in projectContents.Files)
            {
                XmlNode xmlItemNode = xmlDoc.CreateElement("ProjectItem");

                bool doReplacements;
                if (!Boolean.TryParse(item.GetMetadata("DoReplacements"), out doReplacements))
                {
                    doReplacements = false;
                    Log.LogWarning("File {0} from project {1} does not have DoReplacements metadata set; defaulting to no replacements.", item.ItemSpec, project.ItemSpec);
                }
                if (doReplacements)
                {
                    XmlAttribute xmlReplaceParametersNode = xmlDoc.CreateAttribute("ReplaceParameters");
                    xmlReplaceParametersNode.Value = doReplacements.ToString().ToLowerInvariant();
                    xmlItemNode.Attributes.Append(xmlReplaceParametersNode);
                }

                XmlAttribute xmlTargetFileNameNode = xmlDoc.CreateAttribute("TargetFileName");
                string rootNamespace = item.GetMetadata("RootNamespace");
                string targetName = Path.GetFileName(item.ItemSpec);
                xmlTargetFileNameNode.Value = targetName;
                xmlItemNode.Attributes.Append(xmlTargetFileNameNode);

                string openIn = item.GetMetadata("OpenIn");
                string openOrder = item.GetMetadata("OpenOrder");
                if (!string.IsNullOrEmpty(openIn))
                {
                    /////TODO TODO TODO TODO
                }

                xmlItemNode.InnerText = Path.GetFileName(item.ItemSpec);
                xmlItemNode = xmlFolderNode.AppendChild(xmlItemNode);
            }

            foreach (KeyValuePair<string, Folder> folder in projectContents.SubFolders)
            {
                XmlNode xmlSubFolderNode = xmlDoc.CreateElement("Folder");

                XmlAttribute xmlNameAttribute = xmlDoc.CreateAttribute("Name");
                xmlNameAttribute.Value = folder.Value.Name;
                xmlSubFolderNode.Attributes.Append(xmlNameAttribute);

                XmlAttribute xmlTargetFolderNameAttribute = xmlDoc.CreateAttribute("TargetFolderName");
                xmlTargetFolderNameAttribute.Value = folder.Value.Name;
                xmlSubFolderNode.Attributes.Append(xmlTargetFolderNameAttribute);

                if (!WalkProject(project, xmlDoc, xmlSubFolderNode, folder.Value))
                {
                    return false;
                }

                xmlSubFolderNode = xmlFolderNode.AppendChild(xmlSubFolderNode);
            }

            return true;
        }
        #endregion
        #endregion
    }
}
