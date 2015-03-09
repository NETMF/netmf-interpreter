using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;

using Task   = Microsoft.Build.Utilities.Task;

namespace Microsoft.SPOT.Tasks
{
    public class MetaDataProcessor : ToolTask //Microsoft.Build.Tasks.ToolTaskExtension
    {
        private string m_tempFile = null;
        private Hashtable m_bag;
        private ArrayList m_filesWritten = new ArrayList();

        public MetaDataProcessor()
        {
            var debugTasks = Environment.GetEnvironmentVariable( "MSBUILD_DEBUG_TASKS" );
            if( !string.IsNullOrWhiteSpace( debugTasks ) && debugTasks.Contains( GetType().FullName ) )
                System.Diagnostics.Debugger.Launch( );

            m_bag = new Hashtable( );
            this.LegacySkeletonInterop = false;
        }

        private object GetWithDefault(string name, object val)
        {
            object ret = m_bag[name];

            if (ret == null)
            {
                ret = val;
            }

            return ret;
        }

        private bool GetWithDefault(string name, bool val)
        {
            return (bool)GetWithDefault(name, (object)val);
        }

        public bool Minimize
        {
            get { return GetWithDefault("Minimize", false); }
            set { m_bag["Minimize"] = value; }
        }
        
        public bool Verbose
        {
            get { return GetWithDefault("Verbose", false); }
            set { m_bag["Verbose"] = value; }
        }
        
        public bool Resolve
        {
            get { return GetWithDefault("Resolve", false); }
            set { m_bag["Resolve"] = value; }
        }
       
        public string Parse
        {
            get { return (string)m_bag["Parse"]; }
            set { m_bag["Parse"] = value; }
        }

        public bool VerboseMinimize
        {
            get { return GetWithDefault("VerboseMinimize", false); }
            set { m_bag["VerboseMinimize"] = value; }
        }

        public bool NoByteCode
        {
            get { return GetWithDefault("NoByteCode", false); }
            set { m_bag["NoByteCode"] = value; }
        }

        public bool NoAttributes
        {
            get { return GetWithDefault("NoAttributes", false); }
            set { m_bag["NoAttributes"] = value; }
        }

        public string SaveStrings
        {
            get { return (string) m_bag["SaveStrings"]; }
            set { m_bag["SaveStrings"] = value; }
        }

        public string LoadStrings
        {
            get { return (string) m_bag["LoadStrings"]; }
            set { m_bag["LoadStrings"] = value; }
        }

        public string GenerateStringsTable
        {
            get { return (string) m_bag["GenerateStringsTable"]; }
            set { m_bag["GenerateStringsTable"] = value; }
        }

        public string Compile
        {
            get { return (string)m_bag["Compile"]; }
            set { m_bag["Compile"] = value; }
        }

        public string DumpAll
        {
            get { return (string) m_bag["DumpAll"]; }
            set { m_bag["DumpAll"] = value; }
        }

        public string DumpExports
        {
            get { return (string) m_bag["DumpExports"]; }
            set { m_bag["DumpExports"] = value; }
        }

        public string GenerateSkeletonFile
        {
            get { return (string) m_bag["GenerateSkeletonFile"]; }
            set { m_bag["GenerateSkeletonFile"] = value; }
        }

        public string GenerateSkeletonName
        {
            get { return (string) m_bag["GenerateSkeletonName"]; }
            set { m_bag["GenerateSkeletonName"] = value; }
        }

        public string GenerateSkeletonProject
        {
            get { return (string) m_bag["GenerateSkeletonProject"]; }
            set { m_bag["GenerateSkeletonProject"] = value; }
        }

        public bool LegacySkeletonInterop
        {
            get { return (bool) m_bag["LegacySkeletonInterop"]; }
            set { m_bag["LegacySkeletonInterop"] = value; }
        }

        public string GenerateDependency
        {
            get { return (string) m_bag["GenerateDependency"]; }
            set { m_bag["GenerateDependency"] = value; }
        }

        public ITaskItem[] CreateDatabase
        {
            get { return (ITaskItem[])m_bag["CreateDatabase"]; }
            set { m_bag["CreateDatabase"] = value; }
        }
                
        public ITaskItem[] ImportResources
        {
            get { return (ITaskItem[])m_bag["ImportResources"]; }
            set { m_bag["ImportResources"] = value; }
        }

        public string RefreshAssemblyName
        {
            get { return (string) m_bag["RefreshAssemblyName"]; }
            set { m_bag["RefreshAssemblyName"] = value; }
        }

        public string BuildFlavor
        {
            get { return (string)m_bag["BuildFlavor"]; }
            set { m_bag["BuildFlavor"] = value; }
        }

        public string RefreshAssemblyOutput
        {
            get { return (string) m_bag["RefreshAssemblyOutput"]; }
            set { m_bag["RefreshAssemblyOutput"] = value; }
        }

        public string CreateDatabaseFile
        {
            get { return (string) m_bag["CreateDatabaseFile"]; }
            set { m_bag["CreateDatabaseFile"] = value; }
        }

        public ITaskItem[] LoadHints
        {
            get { return (ITaskItem[]) m_bag["LoadHints"]; }
            set { m_bag["LoadHints"] = value; }
        }

        public ITaskItem[] IgnoreAssembly
        {
            get { return (ITaskItem[]) m_bag["IgnoreAssembly"]; }
            set { m_bag["IgnoreAssembly"] = value; }
        }

        public ITaskItem[] Load
        {
            get { return (ITaskItem[]) m_bag["Load"]; }
            set { m_bag["Load"] = value; }
        }

        public ITaskItem[] LoadDatabase
        {
            get { return (ITaskItem[]) m_bag["LoadDatabase"]; }
            set { m_bag["LoadDatabase"] = value; }
        }

        public ITaskItem[] ExcludeClassByName
        {
            get { return (ITaskItem[]) m_bag["ExcludeClassByName"]; }
            set { m_bag["ExcludeClassByName"] = value; }
        }

        public string Endianness
        {
            get { return (string)m_bag["Endianness"]; }
            set { m_bag["Endianness"] = value; }
        }

        public string TargetFrameworkVersion
        {
            get { return (string)m_bag["TargetFrameworkVersion"]; }
            set { m_bag["TargetFrameworkVersion"] = value; }
        }

        [Output]
        public ITaskItem[] FilesWritten
        {
            get
            {
                return (ITaskItem[])m_filesWritten.ToArray(typeof(ITaskItem));
            }
        }

        private bool IsNonEmptyString(string s)
        {
            return s != null && s.Trim().Length > 0;
        }    

        private bool ValidateParameterPair(string s1, string s2, string name1, string name2)
        {
            bool b1 = IsNonEmptyString(s1);
            bool b2 = IsNonEmptyString(s2);

            if (b1 != b2)
            {
                Log.LogMessage( MessageImportance.Normal, string.Format("Only one of '{0}' and '{1}' are set", name1, name2));

                return false;
            }

            return true;
        }

        private bool ValidateParameterTriple(string s1, string s2, string s3, string name1, string name2, string name3)
        {
            bool b1 = IsNonEmptyString(s1);
            bool b2 = IsNonEmptyString(s2);
            bool b3 = IsNonEmptyString(s3);

            if (b1 != b2 || b1 != b3)
            {
                Log.LogMessage( MessageImportance.Normal, string.Format("Only some of '{0}', '{1}', and '{2}' are set", name1, name2, name3));
                return false;
            }

            return true;
        }

        protected override string GenerateFullPathToTool()
        {
            // try Porting Kit path first
            string spoclient = Environment.GetEnvironmentVariable("BUILD_TREE_SERVER");
            string filenameMDP = "";

            if (!string.IsNullOrEmpty(spoclient))
            {
                filenameMDP = Path.Combine(spoclient, "dll\\MetaDataProcessor.exe");
            }

            if (string.IsNullOrEmpty(filenameMDP) || !File.Exists(filenameMDP))
            {

                filenameMDP = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "MetaDataProcessor.exe");

                if (!File.Exists(filenameMDP))
                {
                    string path = Path.Combine( Environment.GetEnvironmentVariable( "ProgramFiles" ), @"Microsoft .NET Micro Framework" );

                    if(Directory.Exists(path))
                    {
                        string ver = m_bag.ContainsKey("TargetFrameworkVersion") ? (string)m_bag["TargetFrameworkVersion"] : "v4.3";

                        filenameMDP = Path.Combine( Path.Combine( Path.Combine( path, ver ), "Tools"), "MetaDataProcessor.exe");

                        if(!File.Exists(filenameMDP))
                        {
                            GetDeviceFrameworkPaths getDevice = new GetDeviceFrameworkPaths();

                            if (getDevice.Execute())
                            {
                                filenameMDP = Path.Combine(getDevice.ToolsPath, "MetaDataProcessor.exe");
                            }
                        }
                    }
                }
            }
            
            if (!File.Exists(filenameMDP))
            {
                Log.LogError( string.Format("No MetadataProcessor tool found at \"{0}\"", filenameMDP) );
            }
            return filenameMDP;
        }
        
        protected override string ToolName
        {
            get { return "MetaDataProcessor"; }
        }

        protected override bool ValidateParameters()
        {
            return
            ValidateParameterTriple(this.GenerateSkeletonProject, this.GenerateSkeletonFile, this.GenerateSkeletonName, "GenerateSkeletonProject", "GenerateSkeletonFile", "GenerateSkeletonName");
            
        //Database?
        }

        private void AppendSwitchIfTrue(CommandLineBuilder commandLine, string switchName, bool f)
        {
            if (f)
            {
                commandLine.AppendSwitch(switchName);
            }
        }

        private string ReplaceStringCaseInsensitive(string text, string search, string replace)
        {
            int idx;

            if ((idx = text.ToLower().IndexOf(search.ToLower())) != -1)
            {
                text = text.Remove(idx, search.Length);
                text = text.Insert(idx, replace);
            }

            return text;
        }

        private string GetProperBuildFlavor(string file)
        {
            if(!m_bag.ContainsKey("BuildFlavor")) return file;

            string repl = "\\public\\" + (string)m_bag["BuildFlavor"] + "\\";

           
            file = ReplaceStringCaseInsensitive(file, "\\public\\debug\\"  , repl);
            file = ReplaceStringCaseInsensitive(file, "\\public\\rtm\\"    , repl);
            file = ReplaceStringCaseInsensitive(file, "\\public\\release\\", repl);

            return file;
        }

        private void AppendSwitch(CommandLineBuilder commandLine, string switchName, ITaskItem[] items)
        {
            if (items != null)
            {
                foreach (ITaskItem item in items)
                {
                    commandLine.AppendSwitch(switchName);
                    commandLine.AppendFileNameIfNotNull(GetProperBuildFlavor(item.ItemSpec));
                }                
            }
        }

        private void AppendLoadHints(CommandLineBuilder commandLine)
        {
            ITaskItem[] items = this.LoadHints;

            if (items != null)
            {
                foreach (ITaskItem item in items)
                {

                    string reference = item.GetMetadata("FullPath");
                    //string reference = item.GetAttribute("FullPath");
        
                    //AppendSwitch(commandLine, "-loadHints", Path.GetFileNameWithoutExtension(reference), reference);
                    commandLine.AppendSwitch("-loadHints");
                    commandLine.AppendSwitch(Path.GetFileNameWithoutExtension(reference));
                    commandLine.AppendFileNameIfNotNull(reference);
                }                
            }
        }

        private void AppendSwitchFiles(CommandLineBuilder commandLine, string switchName, params string[] files)
        {
            //only check the first parameter
            if (files != null && files.Length > 0 && files[0] != null && files[0].Length > 0)
            {
                commandLine.AppendSwitch(switchName);
                foreach (string file in files)
                {
                    commandLine.AppendFileNameIfNotNull(GetProperBuildFlavor(file));
                    //commandLine.AppendSwitch(" ", param);                    
                }
            }
        }

        private void AppendSwitchFileStrings(CommandLineBuilder commandLine, string switchName, string file, params string[] strings)
        {
            if(IsNonEmptyString(file))
            {
                commandLine.AppendSwitch(switchName);
                commandLine.AppendFileNameIfNotNull(file);
                foreach (string val in strings)
                {
                    commandLine.AppendSwitch(val);
                }
            }
        }

        private void AppendSwitchString(CommandLineBuilder commandLine, string switchName, string val)
        {
            if (IsNonEmptyString(val))
            {
                commandLine.AppendSwitch(switchName);
                commandLine.AppendSwitch(val);
            }
        }

        private void AppendRefreshAssemblyCommand(CommandLineBuilder commandLine)
        {
            if (IsNonEmptyString(this.RefreshAssemblyName) || IsNonEmptyString(this.RefreshAssemblyOutput))
            {
                if (!(IsNonEmptyString(this.RefreshAssemblyName) && IsNonEmptyString(this.RefreshAssemblyOutput)))
                {
                    throw new Exception("RefreshAssembly requires both RefreshAssemblyName and RefreshAssemblyOutput properties to be provided");
                }
                else
                {
                    commandLine.AppendSwitch("-refresh_assembly");
                    commandLine.AppendSwitch(this.RefreshAssemblyName);
                    commandLine.AppendFileNameIfNotNull(this.RefreshAssemblyOutput);
                }
            }
        }
        
        override protected string GenerateCommandLineCommands()
        {
            CommandLineBuilderExtension commandLineBuilder = new CommandLineBuilderExtension();            
            AddCommandLineCommands(commandLineBuilder);
            return commandLineBuilder.ToString();
        }

        private void AddCommandLineCommands(CommandLineBuilderExtension commandLine)
        {
//Relative ordering1
//ExcludeClassByName 
//NoBitmapCompresion
//ExportLoc
//Parse
//Minimize
//DumpExports
//DumpAll
//Compile
//SaveStrings

//Relative ordering2
//Load
//DumpAll

//Relative ordering3
//Load
//Resolve
//DumpAll
//GenerateSkeleton
//RefreshAssembly

//Relative ordering4
//LoadHints

//Relative ordering5
//Load
//Resolve
//DumpAll
//GenerateSkeleton
//RefreshAssembly

            AppendLoadHints(commandLine);
            AppendSwitchString(commandLine, "-endian", this.Endianness); 
            AppendSwitch(commandLine, "-load", this.Load);
            AppendSwitch(commandLine, "-loadDatabase", this.LoadDatabase);
            AppendSwitchFiles(commandLine, "-loadStrings", this.LoadStrings);
            AppendSwitch(commandLine, "-excludeClassByName", this.ExcludeClassByName);            
            AppendSwitchFiles(commandLine, "-parse", this.Parse);
            AppendSwitchIfTrue(commandLine, "-minimize", this.Minimize);
            AppendSwitchIfTrue(commandLine, "-resolve", this.Resolve);
            AppendSwitchFiles(commandLine, "-dump_exports", this.DumpExports);
            AppendSwitchFiles(commandLine, "-dump_all", this.DumpAll);
            AppendSwitch(commandLine, "-importResource", this.ImportResources);
            AppendSwitchFileStrings(commandLine, "-compile", this.Compile);
            AppendSwitchFiles(commandLine, "-savestrings", this.SaveStrings);                 
            AppendSwitchIfTrue(commandLine, "-verbose", this.Verbose);
            AppendSwitchIfTrue(commandLine, "-verboseMinimize",  this.VerboseMinimize);
            AppendSwitchIfTrue(commandLine, "-noByteCode", this.NoByteCode);
            AppendSwitchIfTrue(commandLine, "-noAttributes", this.NoAttributes);   
            AppendSwitch(commandLine, "-ignoreAssembly", this.IgnoreAssembly);
            AppendSwitchFiles(commandLine, "-generateStringsTable", this.GenerateStringsTable);
            AppendSwitchFiles(commandLine, "-generate_dependency", this.GenerateDependency);
            AppendCreateDatabase(commandLine);
            AppendSwitchFileStrings(commandLine, "-generate_skeleton", this.GenerateSkeletonFile, this.GenerateSkeletonName, this.GenerateSkeletonProject, this.LegacySkeletonInterop ? "TRUE" : "FALSE");
            AppendRefreshAssemblyCommand(commandLine);
        }

        public override bool Execute()
        {
            bool fRet = false;

            try
            {
                fRet = base.Execute();

                RecordFilesWritten();
            }
            catch (Exception ex)
            {
                Log.LogError("MDP: " + ex.Message);
            }
            finally
            {
                Cleanup();
            }

            return fRet;
        }

        private void RecordFileWritten(string file)
        {
            if (!string.IsNullOrEmpty(file))
            {
                if (File.Exists(file))
                {
                    m_filesWritten.Add(new TaskItem(file));
                }
            }
        }
    
        private void RecordFilesWritten()
        {
            RecordFileWritten(this.SaveStrings);
            RecordFileWritten(this.GenerateStringsTable);
            RecordFileWritten(this.DumpAll);
            RecordFileWritten(this.DumpExports);
            RecordFileWritten(this.Compile);
            RecordFileWritten(Path.ChangeExtension(this.Compile, "pdbx"));
            RecordFileWritten(this.RefreshAssemblyOutput);
            RecordFileWritten(this.CreateDatabaseFile);
            RecordFileWritten(this.GenerateDependency);
        }

        private void Cleanup()
        {
            if(m_tempFile != null)
                File.Delete(m_tempFile);
        }

        private void AppendCreateDatabase(CommandLineBuilder commandLine)
        {
            if(this.CreateDatabase == null || this.CreateDatabase.Length == 0)
                return;

            m_tempFile = Path.GetTempFileName();
            using (StreamWriter sw = new StreamWriter(m_tempFile))
            {
                foreach(ITaskItem item in this.CreateDatabase)
                    sw.WriteLine(GetProperBuildFlavor(item.ItemSpec));
            }

            AppendSwitchFiles( commandLine, "-create_database", m_tempFile, this.CreateDatabaseFile );
        }
    }
}
