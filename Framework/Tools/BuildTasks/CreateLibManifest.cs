#region Using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Reflection;
using System.Threading;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using _EVAL = Microsoft.Build.Evaluation;
using Microsoft.Build.Construction;
using Microsoft.Build.Tasks;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Configuration;
using System.Security;
using System.Xml;
using System.Text;
using System.Globalization;
using System.Runtime.Serialization;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

#endregion


namespace Microsoft.SPOT.Tasks
{
    public class CreateLibManifest : Task
    {
        private ITaskItem m_manifestFile;
        [Required]
        public ITaskItem ManifestFile
        {
            set { m_manifestFile = value; }
            get { return m_manifestFile; }
        }

        private string m_name;
        [Required]
        public string Name
        {
            set { m_name = value; }
            get { return m_name; }
        }

        private ITaskItem[] m_objectFiles = new ITaskItem[0];
        [Required]
        public ITaskItem[] ObjectFiles
        {
            set { m_objectFiles = value; }
            get { return m_objectFiles; }
        }

        private string m_projectFile;
        [Required]
        public string Project
        {
            set { m_projectFile = value; }
            get { return m_projectFile; }
        }

        private string m_version;
        [Required]
        public string Version
        {
            set { m_version = value; }
            get { return m_version; }
        }

        private string m_platform = null;
        public string Platform
        {
            set { m_platform = value; }
            get { return m_platform; }
        }

        private string m_platformFamily = null;
        public string PlatformFamily
        {
            set { m_platformFamily = value; }
            get { return m_platformFamily; }
        }

        private string m_armType = null;
        public string ArmType
        {
            set { m_armType = value; }
            get { return m_armType; }
        }

        private string m_instructionSet = null;
        public string InstructionSet
        {
            set { m_instructionSet = value; }
            get { return m_instructionSet; }
        }

        private string m_compiler = null;
        public string Compiler
        {
            set { m_compiler = value; }
            get { return m_compiler; }
        }

        private string m_compilerVersion = null;
        public string CompilerVersion
        {
            set { m_compilerVersion = value; }
            get { return m_compilerVersion; }
        }

        private string m_targetLocation = null;
        public string TargetLocation
        {
            set { m_targetLocation = value; }
            get { return m_targetLocation; }
        }

        private string m_memoryFlavor = null;
        public string MemoryFlavor
        {
            set { m_memoryFlavor = value; }
            get { return m_memoryFlavor; }
        }

        private string m_processor = null;
        public string Processor
        {
            set { m_processor = value; }
            get { return m_processor; }
        }

        private string m_targetPlatform = null;
        public string TargetPlatform
        {
            set { m_targetPlatform = value; }
            get { return m_targetPlatform; }
        }

        private string m_codebase = null;
        public string Codebase
        {
            set { m_codebase = value; }
            get { return m_codebase; }
        }

        private string m_codebaseType = null;
        public string CodebaseType
        {
            set { m_codebaseType = value; }
            get { return m_codebaseType; }
        }



        private string m_company = null;
        public string Company
        {
            set { m_company = value; }
            get { return m_company; }
        }

        private string m_copyright = null;
        public string Copyright
        {
            set { m_copyright = value; }
            get { return m_copyright; }
        }

        private string m_description = null;
        public string Description
        {
            set { m_description = value; }
            get { return m_description; }
        }

        private string m_fileVersion = null;
        public string FileVersion
        {
            set { m_fileVersion = value; }
            get { return m_fileVersion; }
        }

        private string m_configuration = null;
        public string Configuration
        {
            set { m_configuration = value; }
            get { return m_configuration; }
        }

        private string m_product = null;
        public string Product
        {
            set { m_product = value; }
            get { return m_product; }
        }

        private string m_trademark = null;
        public string Trademark
        {
            set { m_trademark = value; }
            get { return m_trademark; }
        }


        private void WriteProperty(StreamWriter sw, string property, string propertyName)
        {
            sw.WriteLine( "    <" + propertyName + ">" + (String.IsNullOrEmpty(property) ? "" : property) + "</" + propertyName + ">");
        }

        private void WritePropertyIfPresent(StreamWriter sw, string property, string propertyName)
        {
            if ( !String.IsNullOrEmpty(property) )
                sw.WriteLine( "    <" + propertyName + ">" + property + "</" + propertyName + ">");
        }

        public override bool Execute()
        {
            try
            {
                _EVAL.ProjectCollection.GlobalProjectCollection.UnloadAllProjects();

                _EVAL.Project proj = new _EVAL.Project(m_projectFile);

                using(StreamWriter sw = new StreamWriter( m_manifestFile.ItemSpec ))
                {
                    sw.WriteLine("<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\" ToolsVersion=\"4.0\">");
                    sw.WriteLine();
                    sw.WriteLine( "<PropertyGroup>" );

                    WriteProperty(sw, m_name, "Name");
                    WriteProperty(sw, m_projectFile, "ProjectFile");
                    WriteProperty(sw, m_version, "Version");

                    WritePropertyIfPresent(sw, m_platform, "Platform");
                    WritePropertyIfPresent(sw, m_platformFamily, "PlatformFamily");
                    WritePropertyIfPresent(sw, m_armType, "ArmType");
                    WritePropertyIfPresent(sw, m_instructionSet, "InstructionSet");
                    WritePropertyIfPresent(sw, m_compiler, "Compiler");
                    WritePropertyIfPresent(sw, m_compilerVersion, "CompilerVersion");
                    WritePropertyIfPresent(sw, m_targetLocation, "TargetLocation");
                    WritePropertyIfPresent(sw, m_memoryFlavor, "MemoryFlavor");
                    WritePropertyIfPresent(sw, m_processor, "Processor");
                    WritePropertyIfPresent(sw, m_targetPlatform, "TargetPlatform");
                    WritePropertyIfPresent(sw, m_codebase, "Codebase");
                    WritePropertyIfPresent(sw, m_codebaseType, "CodebaseType");

                    WritePropertyIfPresent(sw, m_company, "Company");
                    WritePropertyIfPresent(sw, m_copyright, "Copyright");
                    WritePropertyIfPresent(sw, m_description, "Description");
                    WritePropertyIfPresent(sw, m_fileVersion, "FileVersion");
                    WritePropertyIfPresent(sw, m_configuration, "Configuration");
                    WritePropertyIfPresent(sw, m_product, "Product");
                    WritePropertyIfPresent(sw, m_trademark, "Trademark");

                    foreach (ProjectPropertyGroupElement bpg in proj.Xml.PropertyGroups)
                    {
                        foreach (ProjectPropertyElement bp in bpg.Properties)
                        {
                            WritePropertyIfPresent(sw, bp.Value, bp.Name);
                        }
                    }

                    sw.WriteLine( "</PropertyGroup>" );
                    sw.WriteLine();

                    sw.WriteLine( "<ItemGroup>" );

                    foreach (ITaskItem i in m_objectFiles)
                    {
                        sw.WriteLine( "    <ObjectFile Include=\"" + i.ItemSpec + "\" />" );
                    }
                    sw.WriteLine( "</ItemGroup>" );
                    sw.WriteLine();

                    foreach (ProjectImportElement imp in proj.Xml.Imports)
                    {
                        if (Path.GetExtension(imp.Project).ToLower().Trim() == ".libcatproj")
                        {
                            sw.WriteLine("<Import Project=\"{0}\"/>", imp.Project);
                        }
                    }

                    sw.WriteLine( "</Project>" );
                }
            }
            catch (Exception e)
            {
			    Log.LogError("Error trying to create manifest \"" + m_manifestFile + "\": " + e.Message);
                return false;
            }
            return true;
        }

    }
}
