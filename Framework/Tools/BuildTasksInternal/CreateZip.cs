using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using System.CodeDom.Compiler;

using Task   = Microsoft.Build.Utilities.Task;

/*
 * Reserved Properties
    MSBuild provides the following reserved properties:
	MSBuildProjectDirectory - The absolute path of the directory where the project file is located, for example, C:\MyCompany\MyProduct
	MSBuildProjectFilename - The complete file name of the project file, including the file name extension, for example, MyApp.proj
	MSBuildProjectExtension - The file name extension of the project file, including the period, for example, .proj
	MSBuildProjectFullPath - The absolute path and complete file name of the project file, for example, C:\MyCompany\MyProduct\MyApp.proj
	MSBuildProjectName - The file name of the project file without the file name extension, for example, MyApp
	MSBuildBinPath - The absolute path of the directory where the MSBuild binaries that are currently being used are located, for example, C:\Windows\Microsoft.Net\Framework\v2.0.31230.00. This property is useful if you need to refer to files in the MSBuild directory.
*/

namespace Microsoft.SPOT.Tasks.Internal
{
    public class CreateZip : ToolTask
    {
        private string m_outputFile;
        private ITaskItem[] m_inputFiles;
        private ITaskItem[] m_inputDirectories;
        private string m_workingDirectory = null;

        public ITaskItem[] InputFiles
        {
            get { return m_inputFiles; }
            set { m_inputFiles = value; }
        }

        public ITaskItem[] InputDirectories
        {
            get { return m_inputDirectories; }
            set { m_inputDirectories = value; }
        }

        public string WorkingDirectory
        {
            set
            {
                if (!Directory.Exists(value))
                {
                    throw new DirectoryNotFoundException(value + " does not exist");
                }

                m_workingDirectory = value;
            }
        }

        [Required]
        public string OutputFile
        {
            get { return m_outputFile; }
            set { m_outputFile = value; }
        }

        protected override string ToolName
        {
            get {return "zip.exe";}
        }

        override protected string GenerateCommandLineCommands()
        {
            CommandLineBuilderExtension commandLineBuiler = new CommandLineBuilderExtension();
            AddCommandLineCommands(commandLineBuiler);
            return commandLineBuiler.ToString();
        }

        private void AddCommandLineCommands(CommandLineBuilderExtension commandLine)
        {
            string outputDirectory = Path.GetDirectoryName(this.OutputFile);

            if(!Directory.Exists( outputDirectory ))
                Directory.CreateDirectory( outputDirectory );

            commandLine.AppendSwitch( "-q" );
            if (m_inputDirectories != null)
            {
                commandLine.AppendSwitch("-r");
            }
            commandLine.AppendFileNameIfNotNull( this.m_outputFile );

            if (m_inputFiles != null)
            {
                foreach (ITaskItem inputFile in InputFiles)
                {
                    Log.LogMessage("Adding file {0}", inputFile.ItemSpec);
                    commandLine.AppendFileNameIfNotNull(inputFile.ItemSpec);
                }
            }

            if (m_inputDirectories != null)
            {
                foreach (ITaskItem inputDirectory in InputDirectories)
                {
                    Log.LogMessage("Adding directory {0}", inputDirectory.ItemSpec);
                    commandLine.AppendFileNameIfNotNull(inputDirectory.ItemSpec);
                }
            }
        }

        protected override string GenerateFullPathToTool()
        {
            string sporoot = System.Environment.GetEnvironmentVariable("SPOROOT");

            string toolPath = null;

            if (sporoot != null)
            {
                toolPath = Path.Combine(sporoot, @"tools\x86\bin\zip.exe");
            }

            if(toolPath == null || !File.Exists(toolPath))
            {
                return FindTool(Path.GetDirectoryName(this.BuildEngine.ProjectFileOfTaskNode));
            }

            return toolPath;
        }

        protected override string GetWorkingDirectory()
        {
            if (string.IsNullOrEmpty(m_workingDirectory))
            {
                return base.GetWorkingDirectory();
            }

            return m_workingDirectory;
        }

        private string FindTool(string directory)
        {
            string candidate = Path.Combine(directory, @"tools\x86\bin\zip.exe");

            if (!File.Exists(candidate) && directory != Path.GetPathRoot(directory))
            {
                return FindTool(Path.GetDirectoryName(directory));
            }

            return candidate;
        }
    }
}
