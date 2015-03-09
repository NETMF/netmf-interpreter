using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace Microsoft.SPOT.Tasks.NativeBuild
{
    // Generic incremental build task

    public abstract class CompileTask : ToolTask
    {

        public class Dependency
        {
            private string filename;

            public string Filename
            {
                get { return filename; }
                set { filename = value; }
            }

            private DateTime modified;

            [XmlIgnore]
            public DateTime Modified
            {
                get { return modified; }
                set { modified = value; }
            }
        }

        public class DependencyGroup
        {
            [XmlElement(Type = typeof(Dependency))]
            public ArrayList dependencies;
        }

        private string inputFiles;

        [Required]
        public string InputFiles
        {
            get { return inputFiles; }
            set { inputFiles = value; }
        }

        private string outPath;

        [Required]
        public string OutputPath
        {
            get { return outPath; }
            set { outPath = value; }
        }

        private string flags = "";

        public string Flags
        {
            get { return flags; }
            set { flags = value; }
        }

        private string includes = "";

        public string IncludePaths
        {
            get { return includes; }
            set { includes = value; }
        }

        private string wrappedTool = "";

        public string WrappedTool
        {
            get { return wrappedTool; }
            set { wrappedTool = value; }
        }

        private string tool = null;

        public string Tool
        {
            get { return tool; }
            set { tool = value; }
        }

        private string m_headers = "";

        public string HeaderFiles
        {
            get { return m_headers; }
            set { m_headers = value; }
        }
     
        
        protected string outputExtension;

        string BaseName(string inputFile)
        {
            // if the input has a path component, strip it
            string basename = inputFile;
            int p = basename.LastIndexOf('\\');
            if (p >= 0) basename = basename.Substring(p + 1);
            return basename;
        }

        string OutputFileName(string inputFile)
        {
            string basename = BaseName(inputFile);
            // strip off the extension
            int p = basename.LastIndexOf('.');
            if (p < 0)
            {
                // log invalid input type
                return null;
            }
            basename = basename.Substring(0, p);
            return OutputPath + @"\" + basename + outputExtension;
        }

        string DependencyFileName(string inputFile)
        {
            string basename = BaseName(inputFile);
            // strip off the extension
            int p = basename.LastIndexOf('.');
            if (p >= 0)
            {
                basename = basename.Substring(0, p) + ',' + basename.Substring(p + 1);
            }
            return OutputPath + @"\" + basename + ".dep";
        }

        string ReadFileContents(string path)
        {
            StreamReader rd = null;

            try
            {
                rd = File.OpenText(path);
                return rd.ReadToEnd();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ReadFileContents(" + path + "): " + ex.Message);
                return "";
            }
            finally
            {
                if (rd != null)
                {
                    rd.Close();
                }
            }
        }

        string FindFile(string fname)
        {
            if (File.Exists(fname))
            {
                return fname;
            }
            // Search the include paths
            // iterate throught the input files
            string[] includeDirs = IncludePaths.Split(new char[1] { ';' });
            foreach (string includePath in includeDirs)
            {
                string fpath = includePath + @"\" + fname;
                if (File.Exists(fpath))
                {
                    return fpath;
                }
            }
            return null;
        }

        protected abstract string GetNextIncludedFile(string contents, ref int pos);

        bool RecursiveFileCheck(string filePath, DateTime targetDate)
        {
            if (File.GetLastWriteTime(filePath) > targetDate)
            {
                Console.WriteLine("Dependency is newer; must rebuild");
                return true; // need to rebuild
            }

            // recurse through any #included files

            string contents = ReadFileContents(filePath);
            // find all the #include tags
            int pos = 0;
            string fname;
            while ((fname = GetNextIncludedFile(contents, ref pos)) != null)
            {
                string fpath = FindFile(fname);
                if (fpath != null)
                {
                    if (RecursiveFileCheck(fpath, targetDate))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        ArrayList LoadDependencies(string infile)
        {
            FileStream fs = null;
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(DependencyGroup));
                string depFileName = DependencyFileName(infile);
                fs = new FileStream(depFileName, FileMode.Open);
                DependencyGroup depgroup = (DependencyGroup)ser.Deserialize(fs);
                ArrayList dependencies = depgroup.dependencies;
                // if any files are newer than the dependency file, it
                // is stale
                DateTime depdate = File.GetLastWriteTime(depFileName);
                for (int i = 0; i < dependencies.Count; i++)
                {
                    Dependency dep = dependencies[i] as Dependency;
                    if (!File.Exists(dep.Filename)) return null;
                    dep.Modified = File.GetLastWriteTime(dep.Filename);
                    if (dep.Modified > depdate) return null;
                }
                return dependencies;
            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }

        void RecursivelyAddDependencies(string file, ArrayList dependencies)
        {
            Dependency dep;

            // Don't add it if it is already present
            for (int i = 0; i < dependencies.Count; i++)
            {
                dep = dependencies[i] as Dependency;
                if (dep != null && dep.Filename == file)
                {
                    return;
                }
            }

            // Add the file as a dependency

            dep = new Dependency();
            dep.Filename = file;
            dependencies.Add(dep);

            // recurse through any #included files

            string contents = ReadFileContents(file);
            // find all the #include tags
            int pos = 0;
            string fname;
            while ((fname = GetNextIncludedFile(contents, ref pos)) != null)
            {
                string fpath = FindFile(fname);
                if (fpath != null)
                {
                    RecursivelyAddDependencies(fpath, dependencies);
                }
            }
        }

        ArrayList BuildDependencies(string infile)
        {
            FileStream fs = null;
            try
            {
                ArrayList dependencies = new ArrayList();
                RecursivelyAddDependencies(infile, dependencies);
                DependencyGroup depgroup = new DependencyGroup();
                depgroup.dependencies = dependencies;
                // save the data
                XmlSerializer ser = new XmlSerializer(typeof(DependencyGroup));
                string depFileName = DependencyFileName(infile);
                fs = new FileStream(depFileName, FileMode.Create);
                ser.Serialize(fs, depgroup);
		        return dependencies;
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }

        bool DependencyCheck(string outfile, ArrayList dependencies)
        {
            DateTime targetDate = File.GetLastWriteTime(outfile);
            for (int i = 0; i < dependencies.Count; i++)
            {
                Dependency dep = dependencies[i] as Dependency;
                if (dep != null && File.GetLastWriteTime(dep.Filename) > targetDate)
                {
                    Log.LogMessage(outfile+" is older than dependency "+dep.Filename+"; must rebuild");
                    return true;
                }
            }
            return false; // no rebuild needed
        }

        bool MustRebuild(string outfile, string infile)
        {
            // See if the output already exists. If so, check the timestamp
            // to see if it needs rebuilding

            if (File.Exists(outfile))
            {
#if NO_CACHING
                return RecursiveFileCheck(infile, File.GetLastWriteTime(outfile));
#else

                ArrayList dependencies = LoadDependencies(infile);
                if (dependencies == null) // no file or file is stale
                {
                    dependencies = BuildDependencies(infile);
                }
                return DependencyCheck(outfile, dependencies);
#endif
            }
            return true;
        }

        protected abstract string CommandArgs(string inFile, string outFile, bool firstFile);

        bool Process(string inFile, string outFile, bool firstFile)
        {
            try
            {
                string commandArgs = CommandArgs(inFile, outFile, firstFile);
                string args = "";
                //if (tool != null && tool.Length>0) cmd = tool+" ";
                if (tool == null || tool.Length == 0) { tool = wrappedTool; wrappedTool = ""; }
                if (wrappedTool != null && wrappedTool.Length > 0) args += wrappedTool + " ";
                args += commandArgs;

                LogToolCommand(tool + " " + args);
                return (ExecuteTool(tool, "", args) == 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("CompileTask.Process threw exception " + ex.Message);
                throw;
            }
        }

        public override bool Execute()
        {
            try
            {
                bool firstFile = true;
                // iterate throught the input files
                string[] inputs = InputFiles.Split(';' );
                string[] headers = HeaderFiles.Split(';');
                string[] paths = IncludePaths.Split(';');

                bool hdrsChanged = false;

                if (headers.Length > 0 && inputs.Length > 0 && File.Exists(inputs[0]))
                {
                    string outFile = OutputFileName(inputs[0]);

                    if (File.Exists(outFile))
                    {
                        DateTime last = File.GetLastWriteTime(outFile);

                        for (int i = headers.Length - 1; i >= 0; i--)
                        {
                            string hdr = headers[i];
                            if (!Path.IsPathRooted(hdr))
                            {
                                int cnt = paths.Length;
                                for (int j = 0; j < cnt; j++)
                                {
                                    string fp = Path.Combine(paths[j], hdr);

                                    if (File.Exists(fp))
                                    {
                                        hdr = fp;
                                        break;
                                    }
                                }
                            }

                            if (File.Exists(hdr))
                            {
                                if (File.GetLastWriteTime(hdr) > last)
                                {
                                    hdrsChanged = true;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        hdrsChanged = true;
                    }
                }


                foreach (string inputFile in inputs)
                {
                    if (!File.Exists(inputFile))
                    {
                        // Log non-existent file
                        Log.LogError("Cannot find file "+inputFile);
                        return false;
                    }

                    string outfile = OutputFileName(inputFile);
                    if (outfile == null)
                    {
                        Log.LogError("Invalid output file from input " + inputFile, null);
                        return false;
                    }

                    if (hdrsChanged || MustRebuild(outfile, inputFile))
                    {
                        File.Delete(outfile);
                        if (!Process(inputFile, outfile, firstFile))
                        {
                            Log.LogError("Build failed: " + inputFile, null);
                            return false;
                        }
                    }
                    else
                    {
                        //Log.LogMessage("Nothing to be done for " + outfile);
                    }
                    firstFile = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("CompileTask.Execute threw exception " + ex.Message);
                throw;
            }
            return true;
        }

        public CompileTask(string tool, string outputExtension)
        {
            this.tool = tool;
            this.outputExtension = outputExtension;
        }

        protected override string GenerateFullPathToTool()
        {
            return Tool;
        }

        protected override string ToolName
        {
            get { return Tool; }
        }
    }

    //-------------------------------------------------------------------------
    // Generic C compile task

    public abstract class CompileCTask : CompileTask
    {
        protected override string GetNextIncludedFile(string contents, ref int pos)
        {
            try
            {
                while ((pos = contents.IndexOf("#include", pos)) >= 0)
                {
                    pos += 8;
                    int qpos = contents.IndexOf('"', pos);
                    if (qpos >= 0)
                    {
                        ++qpos;
                        int bpos = contents.Substring(pos, qpos - pos).IndexOf('<');
                        if (bpos < 0)
                        {
                            int cpos = contents.IndexOf('"', qpos);
                            if (cpos >= 0)
                            {
                                string fname = contents.Substring(qpos, cpos - qpos);
                                return fname;
                            }
                        }
                        // else it was "#include <>" form which we don't care about
                    }
                    // else it was "#include <>" form which we don't care about
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetNextIncludedFile threw exception " + ex.Message);
                throw;
            }
            return null;
        }

        protected override abstract string CommandArgs(string inFile, string outFile, bool firstFile);

        public CompileCTask(string tool, string outputExtension)
            : base(tool, outputExtension)
        { }
    }

    //---------------------------------------------------------------------------
    // Visual Studio C Compile task

    public class VSCompileC : CompileCTask
    {
        protected override string CommandArgs(string inFile, string outFile, bool firstFile)
        {
            try
            {
                string commandArgs = "/c " + inFile + " " + Flags + " /Fo" + outFile;
                if (firstFile)
                {
                    int pos = commandArgs.IndexOf("/Yu");
                    if (pos >= 0)
                    {
                        // change use pch to create pch
                        commandArgs = commandArgs.Substring(0, pos + 2) + 'c' + commandArgs.Substring(pos + 3);
                    }
                }
                return commandArgs;
            }
            catch (Exception ex)
            {
                Console.WriteLine("VSCompileC threw exception: "+ex.Message);
                throw;
            }
        }

        public VSCompileC()
            : base("cl.exe", ".obj")
        { }
    }

    //---------------------------------------------------------------------------
    // ADS C/C++ Compile task base class

    public class CompileArmTask : CompileCTask
    {
        protected override string CommandArgs(string inFile, string outFile, bool firstFile)
        {
            string commandArgs = Flags + " -o" + outFile + " -c " + inFile;
            return commandArgs;
        }

        public CompileArmTask(string tool)
            : base(tool, ".obj")
        { }
    }

    //---------------------------------------------------------------------------
    // ADS C Compile task

    public class ADSCompileCC : CompileArmTask
    {
        public ADSCompileCC()
            : base("")
        { }
    }

    //---------------------------------------------------------------------------
    // ADS C++ Compile task

    public class ADSCompileCPP : CompileArmTask
    {
        public ADSCompileCPP()
            : base("")
        { }
    }

    //---------------------------------------------------------------------------
    // ADS TCC Compile task

    public class ADSCompileTCC : CompileArmTask
    {
        public ADSCompileTCC()
            : base("")
        {
            outputExtension = ".tobj";
        }
    }

    //----------------------------------------------------------------------------
    // ADS Assembler task

    public class ADSAssemble : CompileTask
    {
        protected override string GetNextIncludedFile(string contents, ref int pos)
        {
            // handle INCLUDE directives; TBD
            return null;
        }

        protected override string CommandArgs(string inFile, string outFile, bool firstFile)
        {
            //     $(AS) $(AS_FLAGS) $(POS_DEPENDENT) -LIST $*.txt -xref -o $@ $<

            string commandArgs = "-o " + outFile + " " + inFile;
            return commandArgs;
        }

        public ADSAssemble()
            : base("", ".obj")
        { }
    }

    //---------------------------------------------------------------------------
    // ADI C/C++ Compile task base class

    public class CompileADITask : CompileCTask
    {
        protected override string CommandArgs(string inFile, string outFile, bool firstFile)
        {
            string commandArgs = Flags + " -o " + outFile + " " + inFile;
            return commandArgs;
        }

        public CompileADITask(string tool)
            : base(tool, ".doj")
        { }
    }

    //---------------------------------------------------------------------------
    // ADI C Compile task

    public class ADICompileCC : CompileADITask
    {
        public ADICompileCC()
            : base("ccblkfn.exe")
        { }
    }

    //---------------------------------------------------------------------------
    // ADI C++ Compile task

    public class ADICompileCPP : CompileADITask
    {
        public ADICompileCPP()
            : base("ccblkfn.exe")
        { }
    }

    //----------------------------------------------------------------------------
    // ADI Assembler task

    public class ADIAssemble : CompileTask
    {
        protected override string GetNextIncludedFile(string contents, ref int pos)
        {
            // handle INCLUDE directives; TBD
            return null;
        }

        protected override string CommandArgs(string inFile, string outFile, bool firstFile)
        {
            //     $(AS) $(AS_FLAGS) $(POS_DEPENDENT) -LIST $*.txt -xref -o $@ $<

            string commandArgs = "-o " + outFile + " " + inFile;
            return commandArgs;
        }

        public ADIAssemble()
            : base("easmblkfn.exe", ".doj")
        { }
    }
}
