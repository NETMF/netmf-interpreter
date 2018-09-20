using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using XsdInventoryFormatObject;
//using Microsoft.Build.BuildEngine;

[assembly: InternalsVisibleTo("ComponentBuilder, PublicKey=0024000004800000940000000602000000240000525341310004000001000100B72BE28059C8C300C866887A820EAB7FD9E8F7D41179917EA660A5AC8FA46AC492358A9E4A64591F7C279D77E56844ADDD3762F2F539D493A01631B82AE1A255110E0143856C079976A3396CC30D1E81EA2748F04D198BB273BD721C7FF461A514182C2775B7D8658B529DB2BD11319AB024FAABD7272B3C2F6196184EB666B3")]
[assembly: InternalsVisibleTo("SolutionWizard, PublicKey=0024000004800000940000000602000000240000525341310004000001000100B72BE28059C8C300C866887A820EAB7FD9E8F7D41179917EA660A5AC8FA46AC492358A9E4A64591F7C279D77E56844ADDD3762F2F539D493A01631B82AE1A255110E0143856C079976A3396CC30D1E81EA2748F04D198BB273BD721C7FF461A514182C2775B7D8658B529DB2BD11319AB024FAABD7272B3C2F6196184EB666B3")]
[assembly: InternalsVisibleTo("PKStudio, PublicKey=0024000004800000940000000602000000240000525341310004000001000100B72BE28059C8C300C866887A820EAB7FD9E8F7D41179917EA660A5AC8FA46AC492358A9E4A64591F7C279D77E56844ADDD3762F2F539D493A01631B82AE1A255110E0143856C079976A3396CC30D1E81EA2748F04D198BB273BD721C7FF461A514182C2775B7D8658B529DB2BD11319AB024FAABD7272B3C2F6196184EB666B3")]
namespace ComponentObjectModel
{
    public class MFComponentDescriptor
    {
        internal MFComponentDescriptor(MFComponent cmp, string desc, string docs, string projPath, Processor processor)
        {
            Component = cmp;
            Description = desc;
            Documentation = docs;
            ProjectPath = projPath;
            SolutionProcessor = processor;
        }

        public readonly MFComponent Component;
        public readonly string Description;
        public readonly string Documentation;
        public readonly string ProjectPath;
        public readonly Processor SolutionProcessor;
    };

    public class MsBuildWrapper
    {
        InventoryHelper m_helper;
        //bool m_forceLoadDependencies = false;

        #region TAG_STRINGS
        const string RelativeTargetPath = "tools\\targets\\";
        const string RelativePlatformPath = "DeviceCode\\targets\\";
        const string MicrosoftSpotNamespace = "Microsoft.SPOT.";
        const string TargetFileExtension = ".Targets";
        const string CCompilerTag = "CC";
        const string CppCompilerTag = "CPP";
        const string AsmCompilerTag = "AS";
        const string LinkerTag = "LINK";
        const string ArchiverTag = "AR";
        const string FromElfTag = "FROMELF";
        const string ToolWrapperTag = "WRAPPER";
        const string AdsWrapperTag = "ADS_WRAPPER";
        const string AdiWrapperTag = "ADI_WRAPPER";
        const string ArcWrapperTag = "ARC_WRAPPER";

        const string ArmCCompilerTargetTag = "ArmCompileC";
        const string ArmCppCompilerTargetTag = "ArmCompileCPP";
        const string ArmAsmTargetTag = "ArmAssemble";
        const string ArmLibTargetTag = "ArmBuildLib";
        const string ArmExeTargetTag = "BuildAXF";

        const string AdiCCompilerTargetTag = "ADICompileC";
        const string AdiCppCompilerTargetTag = "ADICompileCPP";
        const string AdiAsmTargetTag = "ADIAssemble";
        const string AdiLibTargetTag = "ADIBuildLib";
        const string AdiExeTargetTag = "BuildDXE";

        const string ArcCCompilerTargetTag = "ARCCompileC";
        const string ArcCppCompilerTargetTag = "ARCCompileCPP";
        const string ArcAsmTargetTag = "ARCAssemble";
        const string ArcLibTargetTag = "ARCBuildLib";
        const string ArcExeTargetTag = "ARCBuildDXE";

        const string CCompilerTargetTag = "CompileC";
        const string CppCompilerTargetTag = "CompileCpp";
        const string AsmCompilerTargetTag = "CompileAsm";
        const string LibTargetTag = "BuildLib";
        const string ExeTargetTag = "BuildExe";
        const string TargetTaskTag = "Exec";
        const string ScatterFileTargetTag = "BuildScatterfile";

        const string CpuNamesTag = "CpuNames";
        const string ISAsTag = "ISAName";

        const string PKUI_CpuNamesTag = "PKUI_CpuNames";
        const string PKUI_ISAsTag = "PKUI_ISAs";
        const string PKUI_LibCatTag = "PKUI_LibraryCategory";

        const string CommonToolFlagTag = "AS_CC_CPP_COMMON_FLAGS";
        const string CCppTargFlagTag = "CC_CPP_TARGETTYPE_FLAGS";
        const string CCppCommonFlagTag = "CC_CPP_COMMON_FLAGS";
        const string CFlagTag = "CC_FLAGS";
        const string CppFlagTag = "CPP_FLAGS";
        const string AsmFlagTag = "AS_FLAGS";
        const string AsmArmFlagTag = "ASFLAGS";
        const string LinkerFlagTag = "LINK_FLAGS";
        const string ArchiverFlagTag = "AR_FLAGS";
        const string ArchiverArmFlagTag = "ARFLAGS";
        const string ArchiverPlatFlagTag = "AR_PLATFORM_FLAGS";
        const string FromElfFlagTag = "FROMELF_FLAGS";
        const string ScatterFlagTag = "SCATTER_FLAG";

        const string SetEnvScriptTag = "SetEnvironmentVariable";

        const string ObjExtFlag = "OBJ_EXT";
        const string LibExtFlag = "LIB_EXT";
        const string ExeExtFlag = "EXE_EXT";
        const string ScatterExtFlag = "SCATTER_EXT";

        const string TaskScriptAttributeName = "Command";

        const string c_MSBuildDefaultSchema = @"http://schemas.microsoft.com/developer/msbuild/2003";
        const string c_XmlHeaderString = "<?xml version=\"1.0\" encoding=\"utf-16\"?>";

        #endregion //TAG_STRINGS

        internal class MFComponentHash
        {
            public MFComponentHash(Library lib, LibraryCategory type, MFComponent comp)
            {
                Library = lib;
                LibraryCategory = type;
                MFComponent = comp;
            }
            public Library Library;
            public LibraryCategory LibraryCategory;
            public MFComponent MFComponent;
        };

        private void Init(List<Inventory> invs)
        {
            // TODO: add support for parameterization of these vars (not critical because it is just used for project loading)
            Environment.SetEnvironmentVariable("OBJDIR", "obj");
            string compiler = Environment.GetEnvironmentVariable("COMPILER_TOOL");
            if (string.IsNullOrEmpty(compiler))
            {
                Environment.SetEnvironmentVariable("AS_SUBDIR", "RVD_S");
            }
            else
            {
                switch (compiler.ToUpper())
                {
                    case "GCC":
                        Environment.SetEnvironmentVariable("AS_SUBDIR", "GNU_S");
                        break;
                    case "RVDS":
                    default:
                        // default to RVDS
                        Environment.SetEnvironmentVariable("AS_SUBDIR", "RVD_S");
                        break;
                }
            }
            Environment.SetEnvironmentVariable("ARMMODE", "ARM");
            Environment.SetEnvironmentVariable("INSTRUCTION_SET", "ARM");

            m_helper = new InventoryHelper(invs);
        }

        public MsBuildWrapper(Inventory inv)
        {
            //Engine.GlobalEngine.BinPath = Path.GetDirectoryName(Environment.GetEnvironmentVariable("MSBUILD_EXE"));
            List<Inventory> list = new List<Inventory>();
            list.Add(inv);

            Init(list);
        }

        public MsBuildWrapper(List<Inventory> invs)
        {
            Init(invs);
        }

        public void LoadAllComponents(string pkClientDirectory)
        {
            LoadDefaultBuildTargets(pkClientDirectory);
            LoadDefaultProcessors(pkClientDirectory);
            LoadDefaultLibraryCategories(pkClientDirectory);
            LoadDefaultLibraries(pkClientDirectory);
            LoadDefaultAssemblies(pkClientDirectory);
            LoadDefaultFeatures(pkClientDirectory);
            LoadSolutions(Path.Combine(pkClientDirectory, "Solutions"));
        }

        public void LoadDefaultBuildTargets(string pkClientDirectory)
        {
            LoadBuildToolFromTargetFile(Path.Combine(pkClientDirectory, "tools\\targets\\Microsoft.Spot.system.rvds.Targets"));
            LoadBuildToolFromTargetFile(Path.Combine(pkClientDirectory, "tools\\targets\\Microsoft.Spot.system.gcc.Targets"));
            LoadBuildToolFromTargetFile(Path.Combine(pkClientDirectory, "tools\\targets\\Microsoft.Spot.system.mdk.Targets"));
            // not supported yet
            //LoadBuildToolFromTargetFile(Path.Combine(pkClientDirectory, "tools\\targets\\Microsoft.Spot.system.arc.Targets"));
            //LoadBuildToolFromTargetFile(Path.Combine(pkClientDirectory, "tools\\targets\\Microsoft.Spot.system.blackfin.Targets"));
        }

        internal BuildTool LoadBuildToolFromTargetFile(string fileName)
        {
            BuildTool tool = new BuildTool();


            string fullpath = ExpandEnvVars(fileName, "");

            if (!File.Exists(fullpath)) return null;

            //TODO: Load processor/proctype specific data into processor/procType build options

            tool.Name = Path.GetFileNameWithoutExtension(fullpath).ToUpper().Replace("MICROSOFT.SPOT.SYSTEM.", "");

            string settingsFile = Path.Combine(Path.GetDirectoryName(fullpath), Path.GetFileNameWithoutExtension(fullpath) + ".settings");

            if (File.Exists(settingsFile))
            {
                LoadTargetFile(tool, settingsFile);
            }

            LoadTargetFile(tool, fullpath);

            m_helper.DefaultInventory.BuildTools.Add(tool);

            return tool;
        }

        public void LoadDefaultFeatures(string pkClientDirectory)
        {
            LoadFeatures(Path.Combine(pkClientDirectory, "Framework\\Features"));
        }

        public void LoadFeatures(string featureDirectory)
        {
            string fullpath = ExpandEnvVars(featureDirectory, "");
            foreach (string file in Directory.GetFiles(fullpath, "*.FeatureProj"))
            {
                //Console.WriteLine("FeatureFile: " + file);
                LoadFeatureProj(file, fullpath);
            }
        }

        public void LoadDefaultLibraryCategories(string pkClientDirectory)
        {
            LoadLibraryCategories(Path.Combine(pkClientDirectory, "Framework\\Features"));
        }

        public void LoadLibraryCategories(string libCatDirectory)
        {
            string fullpath = ExpandEnvVars(libCatDirectory, "");

            foreach (string libcat in Directory.GetFiles(fullpath, "*.libcatproj"))
            {
                //Console.WriteLine("LibCatFile: " + libcat);
                LoadLibraryCategoryProj(libcat, libcat);
            }
        }

        public void LoadDefaultAssemblies(string pkClientDirectory)
        {
            LoadAssemblies(Path.Combine(pkClientDirectory, "framework"));
            //LoadAssemblies(Path.Combine(pkClientDirectory, "Product\\AuxDisplay"));
        }

        public void LoadAssemblies(string asmRootDirectory)
        {
            string fullpath = ExpandEnvVars(asmRootDirectory, "");
            foreach (string file in Directory.GetFiles(fullpath, "*.csproj"))
            {
                //Console.WriteLine("AssemblyFile: " + file);
                LoadAssemblyProj(file, fullpath);
            }
            foreach (string subdir in Directory.GetDirectories(fullpath))
            {
                LoadAssemblies(subdir);
            }
        }

        public void LoadDefaultLibraries(string pkClientDirectory)
        {
            LoadLibraries(Path.Combine(pkClientDirectory, "Crypto"));
            LoadLibraries(Path.Combine(pkClientDirectory, "Support\\CRC"));
            LoadLibraries(Path.Combine(pkClientDirectory, "Support\\WireProtocol"));
            LoadLibraries(Path.Combine(pkClientDirectory, "CLR\\Core"));
            LoadLibraries(Path.Combine(pkClientDirectory, "CLR\\Debugger"));
            LoadLibraries(Path.Combine(pkClientDirectory, "CLR\\Diagnostics"));
            LoadLibraries(Path.Combine(pkClientDirectory, "CLR\\Graphics"));
            LoadLibraries(Path.Combine(pkClientDirectory, "CLR\\Libraries"));
            LoadLibraries(Path.Combine(pkClientDirectory, "CLR\\Messaging"));
            LoadLibraries(Path.Combine(pkClientDirectory, "CLR\\StartupLib"));
            LoadLibraries(Path.Combine(pkClientDirectory, "DeviceCode"));
        }

        public void LoadLibraries(string libRootDirectory)
        {
            string fullpath = ExpandEnvVars(libRootDirectory, "");

            if (!Directory.Exists(fullpath)) return;

            foreach (string file in Directory.GetFiles(fullpath, "*.proj"))
            {
                //Console.WriteLine("LibraryFile: " + file);
                LoadLibraryProj(file, fullpath);
            }
            foreach (string subdir in Directory.GetDirectories(fullpath))
            {
                LoadLibraries(subdir);
            }
        }

        public void LoadDefaultManifestFiles(string pkClientDirectory)
        {
            string compiler = Environment.GetEnvironmentVariable("COMPILER_TOOL");

            LoadManifestFiles(Path.Combine(pkClientDirectory, "BuildOutput\\ARM\\RVDS3.1"));
            LoadManifestFiles(Path.Combine(pkClientDirectory, "BuildOutput\\THUMB\\RVDS3.1"));
            LoadManifestFiles(Path.Combine(pkClientDirectory, "BuildOutput\\THUMB2\\RVDS3.1"));
            LoadManifestFiles(Path.Combine(pkClientDirectory, "BuildOutput\\ARM\\RVDS4.1"));
            LoadManifestFiles(Path.Combine(pkClientDirectory, "BuildOutput\\THUMB\\RVDS4.1"));
            LoadManifestFiles(Path.Combine(pkClientDirectory, "BuildOutput\\THUMB2\\RVDS4.1"));
            LoadManifestFiles(Path.Combine(pkClientDirectory, "BuildOutput\\ADI5.0"));
            LoadManifestFiles(Path.Combine(pkClientDirectory, "BuildOutput\\SH2A"));
            LoadManifestFiles(Path.Combine(pkClientDirectory, "BuildOutput\\Windows"));
        }

        public void LoadManifestFiles(string manifestRootDirectory)
        {
            string fullpath = ExpandEnvVars(manifestRootDirectory, "");

            if (Directory.Exists(fullpath))
            {
                foreach (string dir in Directory.GetDirectories(fullpath))
                {
                    // only process debug
                    if (fullpath.ToLower().Contains("\\release\\") || fullpath.ToLower().Contains("\\rtm\\"))
                    {
                        continue;
                    }

                    if (fullpath.ToLower().Contains("\\obj\\"))
                    {
                        continue;
                    }

                    LoadManifestFiles(dir);
                }

                foreach (string file in Directory.GetFiles(fullpath, "*.manifest"))
                {
                    LoadLibraryFromManifest(file);
                }
            }
        }

        public void LoadDefaultProcessors(string pkClientDirectory)
        {
            LoadProcessors(Path.Combine(pkClientDirectory, "devicecode\\Targets\\Native"));
            LoadProcessors(Path.Combine(pkClientDirectory, "devicecode\\Targets\\OS"));
        }

        public void LoadProcessors(string procRootDirectory)
        {
            string fullpath = ExpandEnvVars(procRootDirectory, "");

            foreach (string subdir in Directory.GetDirectories(fullpath))
            {
                bool fFoundProc = false;
                foreach (string file in Directory.GetFiles(subdir, "*.settings"))
                {
                    //Console.WriteLine("ProcessorFile: " + file);
                    LoadProcessorProj(file, fullpath);
                    fFoundProc = true;
                }
                if (!fFoundProc)
                {
                    Processor proc = new Processor();
                    proc.Name = Path.GetFileName(subdir);
                    proc.Guid = System.Guid.NewGuid().ToString("B");
                    proc.ProjectPath = ConvertPathToEnv(Path.Combine(subdir, proc.Name + ".settings"));

                    m_helper.DefaultInventory.Processors.Add(proc);
                }
            }
        }

        public void LoadSolutions(string solutionRootDirectory)
        {
            string fullpath = ExpandEnvVars(solutionRootDirectory, "");

            foreach (string subdir in Directory.GetDirectories(fullpath))
            {
                foreach (string solution in Directory.GetFiles(subdir, "*.settings"))
                {
                    //Console.WriteLine("SolutionFile: " + solution);
                    LoadSolutionProj(solution, fullpath);
                }
            }
        }

        public void LoadTemplateProjects(string templateProjectDirectory)
        {
            string fullpath = ExpandEnvVars(templateProjectDirectory, "");

            foreach (string subdir in Directory.GetDirectories(fullpath))
            {
                foreach (string projFile in Directory.GetFiles(subdir, "*.proj"))
                {
                    MFProject proj = LoadProjectProj(projFile, fullpath);

                    m_helper.DefaultInventory.ProjectTemplates.Add(proj);
                }
            }
        }

        //-- private methods --//

        private bool RecurseAdddotNetMFProj(DirectoryInfo path)
        {
            const string c_SubDirectoriesTag = "SubDirectories";
            const string c_dotNetMFProj = @"dotNetMF.proj";

            DirectoryInfo[] dirs = path.GetDirectories();

            if (dirs == null || dirs.Length == 0)
            {
                return File.Exists(Path.Combine(path.FullName, c_dotNetMFProj));
            }

            ArrayList projDirs = new ArrayList();

            foreach (DirectoryInfo dir in dirs)
            {
                if (RecurseAdddotNetMFProj(dir))
                {
                    projDirs.Add(dir);
                }
            }
            // only create dotNetMF.proj files for paths that contain a dotNetMF.proj
            if (projDirs.Count == 0) return false;

            Project p;

            if (File.Exists(Path.Combine(path.FullName, c_dotNetMFProj)))
            {
                p = LoadProject(Path.Combine(path.FullName, c_dotNetMFProj));

                foreach (DirectoryInfo dir in projDirs)
                {
                    bool fFoundProj = false;
                    ProjectItemGroupElement lastGroup = null;

                    foreach (ProjectItemGroupElement ig in p.Xml.ItemGroups)
                    {
                        lastGroup = ig;
                        foreach (ProjectItemElement bi in ig.Items)
                        {
                            if ((0 == string.Compare(bi.ItemType, c_SubDirectoriesTag)) &&
                               (0 == string.Compare(bi.Include, dir.Name)))
                            {
                                fFoundProj = true;
                                break;
                            }
                        }
                        if (fFoundProj) break;
                    }

                    if (!fFoundProj)
                    {
                        if (lastGroup == null)
                        {
                            lastGroup = p.Xml.AddItemGroup();
                        }
                        lastGroup.AddItem(c_SubDirectoriesTag, dir.Name);
                    }
                }
            }
            else
            {
                p = new Project();
                p.Xml.DefaultTargets = "Build";

                ProjectPropertyGroupElement pg = p.Xml.AddPropertyGroup();

                int idx = path.FullName.IndexOf(@"\DeviceCode\");
                pg.AddProperty("Directory", path.FullName.Remove(0, idx + 1));

                p.Xml.AddImport(@"$(SPOCLIENT)\tools\targets\Microsoft.SPOT.System.Settings");

                ProjectItemGroupElement ig = p.Xml.AddItemGroup();

                foreach (DirectoryInfo dir in projDirs)
                {
                    ig.AddItem(c_SubDirectoriesTag, dir.Name);
                }

                p.Xml.AddImport(@"$(SPOCLIENT)\tools\targets\Microsoft.SPOT.System.Targets");
            }

            p.Save(Path.Combine(path.FullName, c_dotNetMFProj));

            return true;
        }

        private void AdddotNetMFProjects(string platformRoot)
        {
            DirectoryInfo di = new DirectoryInfo(platformRoot);

            // we didn't have any code to add
            if (!di.Exists) return;

            string parentDirProj = Path.Combine(di.Parent.Parent.FullName, "dotNetMF.proj");
            if (File.Exists(parentDirProj))
            {
                Project p = LoadProject(parentDirProj);

                bool fFound = false;
                foreach (ProjectItemGroupElement big in p.Xml.ItemGroups)
                {
                    foreach (ProjectItemElement bi in big.Items)
                    {
                        if (bi.ItemType == "SubDirectories" &&
                            bi.Include == di.Parent.Name)
                        {
                            fFound = true;
                        }
                    }
                }
                if (!fFound)
                {
                    ProjectItemGroupElement ig = p.Xml.AddItemGroup();
                    ig.AddItem("SubDirectories", di.Parent.Name);

                    p.Save(parentDirProj);
                }
            }

            RecurseAdddotNetMFProj(di.Parent);
        }

        internal void CopyTemplateFiles(LibraryCategory libType, MFSolution solution, MFComponent compLibTemplate)
        {
            const string c_FastCompile = @"fastcompile.cpp";
            const string c_FastCompileTag = @"FastCompileCPPFile";

            string dstPath = Path.Combine(ExpandEnvVars(Path.GetDirectoryName(solution.ProjectPath), ""), "DeviceCode\\");

            List<string> reloadProjs = new List<string>();

            //Regex expRelPath = new Regex("([\\w\\W]*\\\\)([^\\\\]+\\\\[^\\\\]+)");
            //Regex expRemoveDeviceCode = new Regex("\\\\DeviceCode\\\\", RegexOptions.IgnoreCase);
            //Regex expRemoveStubDir = new Regex("\\\\stub[s]?\\\\", RegexOptions.IgnoreCase);
            Regex expStubReplace = new Regex("(?<!Is)(stub[s]?)", RegexOptions.IgnoreCase);
            //string relPath = "";

            foreach (ApiTemplate api in libType.Templates)
            {
                string src = ExpandEnvVars(api.FilePath, Environment.GetEnvironmentVariable("SPOCLIENT"));

                //
                // Make relative path based on path after \DeviceCode\ if it exists, otherwise up to two directories deep
                //
                //relPath = Path.GetDirectoryName(src);
                //string devCode = "\\devicecode\\";

                /*
                int idx = relPath.ToLower().IndexOf(devCode);
                
                if (idx != -1)
                {
                    idx += devCode.Length;

                    relPath = relPath.Substring(idx, relPath.Length - idx);
                }
                else
                {
                    Match m = expRelPath.Match(Path.GetDirectoryName(api.FilePath));
                    relPath = m.Groups[1].Value.ToLower();
                }
                */

                string dst = dstPath + libType.Name + "\\" + Path.GetFileName(api.FilePath);

                bool isProcFile = src.ToLower().Contains("\\processor\\");

                try
                {
                    // ignore fast compiles
                    if (-1 != dst.IndexOf(c_FastCompile, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    //dst = expRemoveStubDir.Replace(dst, "\\");

                    if (isProcFile)
                    {
                        dst = expStubReplace.Replace(dst, solution.Processor.Name);
                    }
                    else
                    {
                        dst = expStubReplace.Replace(dst, solution.Name);
                    }

                    if (!Directory.Exists(Path.GetDirectoryName(dst)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(dst));
                    }

                    if (File.Exists(dst))
                    {
                        continue;
                    }

                    //
                    // The source file may refer to "stubs" for function names or includes, we need to replace this
                    // with the solution (or processor) name
                    //
                    TextWriter tw = File.CreateText(dst);
                    TextReader tr = File.OpenText(src);

                    try
                    {
                        string txt = tr.ReadToEnd();

                        if (isProcFile)
                        {
                            txt = expStubReplace.Replace(txt, solution.Processor.Name);
                        }
                        else
                        {
                            txt = expStubReplace.Replace(txt, solution.Name);
                        }

                        Regex exp = new Regex("<IsStub>([\\w\\W]+)</IsStub>", RegexOptions.IgnoreCase);

                        txt = exp.Replace(txt, "<IsStub>false</IsStub>");

                        tw.Write(txt);
                    }
                    finally
                    {
                        tr.Close();
                        tw.Close();
                    }

                    File.SetAttributes(dst, FileAttributes.Normal);

                    if (0 == string.Compare(".proj", Path.GetExtension(dst), true))
                    {
                        Project p = LoadProject(dst);
                        p.Xml.DefaultTargets = "Build";

                        compLibTemplate.ProjectPath = ConvertPathToEnv(dst);

                        foreach (ProjectPropertyGroupElement pg in p.Xml.PropertyGroups)
                        {
                            foreach (ProjectPropertyElement bp in pg.Properties)
                            {
                                switch (bp.Name.ToLower())
                                {
                                    case "directory":
                                        bp.Value = RemoveSpoClient(Path.GetDirectoryName(dst));
                                        break;
                                    case "assemblyname":
                                        bp.Value = compLibTemplate.Name;
                                        break;
                                    case "projectpath":
                                        bp.Value = ConvertPathToEnv(dst);
                                        break;
                                    case "projectguid":
                                        bp.Value = compLibTemplate.Guid;
                                        break;
                                    case "libraryfile":
                                        bp.Value = compLibTemplate.Name + ".$(LIB_EXT)";
                                        break;
                                    case "manifestfile":
                                        bp.Value = compLibTemplate.Name + ".$(LIB_EXT).manifest";
                                        break;

                                }
                            }
                        }
                        foreach (ProjectItemGroupElement ig in p.Xml.ItemGroups)
                        {
                            ArrayList remove_list = new ArrayList();
                            foreach (ProjectItemElement bi in ig.Items)
                            {
                                if (bi.ItemType == c_FastCompileTag)
                                {
                                    remove_list.Add(bi);
                                    continue;
                                }

                                //dst = expRemoveStubDir.Replace(dst, "\\");

                                if (-1 != src.IndexOf("\\processor\\", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    bi.Include = expStubReplace.Replace(bi.Include, solution.Processor.Name);
                                }
                                else
                                {
                                    bi.Include = expStubReplace.Replace(bi.Include, solution.Name);
                                }
                            }
                            foreach (ProjectItemElement bi in remove_list)
                            {
                                ig.RemoveChild(bi);
                            }
                        }
                        p.Save(dst);

                        reloadProjs.Add(dst);
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Print(e.ToString());
                    System.Diagnostics.Debug.Print("Unable to copy file: " + src + " to " + dst);
                }
            }
            foreach (string prj in reloadProjs)
            {
                LoadLibraryProj(prj, "", true);
            }
        }

        private string CombineConditionals(string comp1cond, string comp2cond)
        {
            string cond = comp1cond;

            if (string.IsNullOrEmpty(cond))
            {
                cond = comp2cond;
            }
            else if (!string.IsNullOrEmpty(comp2cond))
            {
                if (comp2cond.Contains(cond))
                {
                    cond = comp2cond;
                }
                else if (!cond.Contains(comp2cond))
                {
                    cond += " AND " + comp2cond;
                }
            }

            if (string.IsNullOrEmpty(cond))
            {
                cond = "";
            }
            // TODO: REMOVE DUPLICATES???
            //else
            //{
            //    cond.Split('|', StringSplitOptions.RemoveEmptyEntries);
            //}
            return cond;
        }

        private void AddEnvVarCollection(List<EnvVar> vars, Project proj)
        {

            //lets make it pretty
            Dictionary<string, List<EnvVar>> condToEnv = new Dictionary<string, List<EnvVar>>();
            foreach (EnvVar var in vars)
            {
                if (condToEnv.ContainsKey(var.Conditional))
                {
                    condToEnv[var.Conditional].Add(var);
                }
                else
                {
                    List<EnvVar> list = new List<EnvVar>();
                    list.Add(var);
                    condToEnv[var.Conditional] = list;
                }
            }

            ProjectPropertyGroupElement global = null;

            foreach (string cond in condToEnv.Keys)
            {
                IList list = condToEnv[cond] as IList;

                if (list.Count > 1)
                {
                    ProjectPropertyGroupElement group = proj.Xml.AddPropertyGroup();
                    group.Condition = cond;

                    foreach (EnvVar var in list)
                    {
                        group.AddProperty(var.Name, var.Value);
                    }
                }
                else if (list.Count == 1)
                {
                    if (global == null)
                    {
                        global = proj.Xml.AddPropertyGroup();
                    }
                    EnvVar var = list[0] as EnvVar;
                    ProjectPropertyElement prop = global.AddProperty(var.Name, var.Value);
                    prop.Condition = var.Conditional;

                }
            }

            //foreach (EnvVar var in vars)
            //{
            //    ProjectPropertyElement prop = group.AddProperty(var.Name, var.Value);
            //    prop.Condition = var.Conditional;
            //}
        }

        private void AddBuildScript(string targName, string targConditional, string buildToolWrapper, ToolOptions options, Project proj, params string[] propTags)
        {
            ProjectTargetElement targ;
            ProjectTaskElement task;

            targ = proj.Xml.AddTarget(targName);
            targ.Condition = targConditional;

            foreach (BuildScript p in options.BuildToolParameters.PreBuild)
            {
                task = targ.AddTask(TargetTaskTag);
                task.Condition = p.Conditional;
                task.SetParameter("Command", p.Script);
            }
            foreach (BuildScript p in options.BuildToolParameters.Parameters)
            {
                task = targ.AddTask(TargetTaskTag);
                task.Condition = p.Conditional;
                task.SetParameter("Command", buildToolWrapper + " " + p.Script + " " + string.Join(" ", propTags));
            }
            foreach (BuildScript p in options.BuildToolParameters.PostBuild)
            {
                task = targ.AddTask(TargetTaskTag);
                task.Condition = p.Conditional;
                task.SetParameter("Command", p.Script);
            }
        }

        private void AddBuildToolProperties(BuildTool buildTool, Project proj)
        {
            ProjectPropertyGroupElement group = proj.Xml.AddPropertyGroup();

            if (!string.IsNullOrEmpty(buildTool.BuildToolWrapper))
            {
                group.AddProperty(ToolWrapperTag, buildTool.BuildToolWrapper);
            }

            group.AddProperty(CCompilerTag, buildTool.ToolPath + buildTool.CCompiler.Exec);
            group.AddProperty(CppCompilerTag, buildTool.ToolPath + buildTool.CppCompiler.Exec);
            group.AddProperty(AsmCompilerTag, buildTool.ToolPath + buildTool.AsmCompiler.Exec);
            group.AddProperty(LinkerTag, buildTool.ToolPath + buildTool.Linker.Exec);
            group.AddProperty(ArchiverTag, buildTool.ToolPath + buildTool.Archiver.Exec);
            group.AddProperty(FromElfTag, buildTool.ToolPath + buildTool.FromELF.Exec);

            foreach (MiscBuildTool tool in buildTool.MiscTools)
            {
                group.AddProperty(tool.Name, tool.ToolPath + tool.BuildTool.Exec);
            }
        }

        private void AddBuildTools(BuildTool buildTool, Project proj)
        {
            foreach (MiscBuildTool tool in buildTool.MiscTools)
            {
                AddBuildScript(tool.Name, tool.BuildTool.Conditional, ToEnvVar(ToolWrapperTag), tool.BuildToolOptions, proj, ToEnvVar(tool.Name + "_FLAGS"));
            }

            AddBuildScript(CCompilerTargetTag, buildTool.CCompiler.Conditional, ToEnvVar(ToolWrapperTag), buildTool.BuildOptions.CFlags, proj, ToEnvVar(CommonToolFlagTag), ToEnvVar(CFlagTag), ToEnvVar(CCppCommonFlagTag));
            AddBuildScript(CppCompilerTargetTag, buildTool.CppCompiler.Conditional, ToEnvVar(ToolWrapperTag), buildTool.BuildOptions.CppFlags, proj, ToEnvVar(CommonToolFlagTag), ToEnvVar(CppFlagTag), ToEnvVar(CCppCommonFlagTag));
            AddBuildScript(AsmCompilerTargetTag, buildTool.AsmCompiler.Conditional, ToEnvVar(ToolWrapperTag), buildTool.BuildOptions.AsmFlags, proj, ToEnvVar(CommonToolFlagTag), ToEnvVar(AsmFlagTag));
            AddBuildScript(LibTargetTag, buildTool.Archiver.Conditional, ToEnvVar(ToolWrapperTag), buildTool.BuildOptions.ArchiverFlags, proj, ToEnvVar(CommonToolFlagTag), ToEnvVar(ArchiverFlagTag));
            AddBuildScript(ExeTargetTag, buildTool.Linker.Conditional, ToEnvVar(ToolWrapperTag), buildTool.BuildOptions.LinkerFlags, proj, ToEnvVar(CommonToolFlagTag), ToEnvVar(LinkerFlagTag));

        }

        private string ToEnvVar(string name)
        {
            return "$(" + name + ") ";
        }

        private void AddMiscToolFlags(List<MiscBuildTool> tools, Project proj)
        {
            ProjectPropertyGroupElement group;
            ProjectPropertyElement prop;

            foreach (MiscBuildTool tool in tools)
            {
                group = proj.Xml.AddPropertyGroup();
                foreach (ToolFlag flag in tool.BuildToolOptions.ToolFlags)
                {
                    string envName = tool.Name + "_FLAGS";
                    prop = group.AddProperty(envName, ToEnvVar(envName) + flag.Flag);
                    prop.Condition = flag.Conditional;
                }
            }
        }

        private void AddFormattedPropGroup(string flagTag, List<ToolFlag> flags, Project proj)
        {
            //ProjectPropertyGroupElement group;
            //ProjectPropertyElement prop;


            //lets make it pretty
            Dictionary<string, List<ToolFlag>> condToFlag = new Dictionary<string, List<ToolFlag>>();
            foreach (ToolFlag flag in flags)
            {
                if (condToFlag.ContainsKey(flag.Conditional))
                {
                    condToFlag[flag.Conditional].Add(flag);
                }
                else
                {
                    List<ToolFlag> list = new List<ToolFlag>();
                    list.Add(flag);
                    condToFlag[flag.Conditional] = list;
                }
            }

            ProjectPropertyGroupElement global = null;

            foreach (string cond in condToFlag.Keys)
            {
                List<ToolFlag> list = condToFlag[cond];

                if (list.Count > 1)
                {
                    ProjectPropertyGroupElement group = proj.Xml.AddPropertyGroup();
                    group.Condition = cond;

                    foreach (ToolFlag flag in list)
                    {
                        group.AddProperty(flagTag, ToEnvVar(flagTag) + flag.Flag);
                    }
                }
                else if (list.Count == 1)
                {
                    if (global == null)
                    {
                        global = proj.Xml.AddPropertyGroup();
                    }

                    ToolFlag flag = list[0] as ToolFlag;
                    ProjectPropertyElement prop = global.AddProperty(flagTag, ToEnvVar(flagTag) + flag.Flag);
                    prop.Condition = flag.Conditional;

                }
            }

            /*
            group = proj.Xml.AddPropertyGroup();
            foreach (ToolFlag flag in options.CommonFlags)
            {
                prop = group.AddProperty(CommonToolFlagTag, ToEnvVar(CommonToolFlagTag) + flag.Flag);
                prop.Condition = flag.Conditional;
            }
            */

        }

        private void AddFormattedPropGroup(string flagTag, List<ToolFlag> flags, Project proj, ProjectPropertyGroupElement group)
        {
            if (group == null)
            {
                AddFormattedPropGroup(flagTag, flags, proj);
            }
            else
            {
                foreach (ToolFlag flag in flags)
                {
                    group.AddProperty(flagTag, ToEnvVar(flagTag) + flag.Flag);
                }
            }
        }


        private void AddBuildToolFlags(ToolChainOptions options, Project proj, ProjectPropertyGroupElement group)
        {
            //ProjectPropertyGroupElement group;
            //ProjectPropertyElement prop;

            AddFormattedPropGroup(CommonToolFlagTag, options.CommonFlags, proj, group);

            //group = proj.Xml.AddPropertyGroup();
            //foreach(ToolFlag flag in options.CommonFlags)
            //{
            //    prop = group.AddProperty(CommonToolFlagTag, ToEnvVar(CommonToolFlagTag) + flag.Flag);
            //    prop.Condition = flag.Conditional;
            //}

            AddFormattedPropGroup(CCppCommonFlagTag, options.C_CppFlags, proj, group);

            //group = proj.Xml.AddPropertyGroup();
            //foreach (ToolFlag flag in options.C_CppFlags)
            //{
            //    prop = group.AddProperty(CCppCommonFlagTag, ToEnvVar(CCppCommonFlagTag) + flag.Flag);
            //    prop.Condition = flag.Conditional;
            //}

            AddFormattedPropGroup(CFlagTag, options.CFlags.ToolFlags, proj, group);

            //group = proj.Xml.AddPropertyGroup();
            //foreach (ToolFlag flag in options.CFlags.ToolFlags)
            //{
            //    prop = group.AddProperty(CFlagTag, ToEnvVar(CFlagTag) + flag.Flag);
            //    prop.Condition = flag.Conditional;
            //}

            AddFormattedPropGroup(CppFlagTag, options.CppFlags.ToolFlags, proj, group);

            //group = proj.Xml.AddPropertyGroup();
            //foreach (ToolFlag flag in options.CppFlags.ToolFlags)
            //{
            //    prop = group.AddProperty(CppFlagTag, ToEnvVar(CppFlagTag) + flag.Flag);
            //    prop.Condition = flag.Conditional;
            //}

            AddFormattedPropGroup(AsmFlagTag, options.AsmFlags.ToolFlags, proj, group);

            //group = proj.Xml.AddPropertyGroup();
            //foreach (ToolFlag flag in options.AsmFlags.ToolFlags)
            //{
            //    prop = group.AddProperty(AsmFlagTag, ToEnvVar(AsmFlagTag) + flag.Flag);
            //    prop.Condition = flag.Conditional;
            //}

            AddFormattedPropGroup(ArchiverFlagTag, options.ArchiverFlags.ToolFlags, proj, group);

            //group = proj.Xml.AddPropertyGroup();
            //foreach (ToolFlag flag in options.ArchiverFlags.ToolFlags)
            //{
            //    prop = group.AddProperty(ArchiverFlagTag, ToEnvVar(ArchiverFlagTag) + flag.Flag);
            //    prop.Condition = flag.Conditional;
            //}

            AddFormattedPropGroup(LinkerFlagTag, options.LinkerFlags.ToolFlags, proj, group);

            //group = proj.Xml.AddPropertyGroup();
            //foreach (ToolFlag flag in options.LinkerFlags.ToolFlags)
            //{
            //    prop = group.AddProperty(LinkerFlagTag, ToEnvVar(LinkerFlagTag) + flag.Flag);
            //    prop.Condition = flag.Conditional;
            //}
        }

        private ToolChainOptions LoadToolChainOptions(ProjectPropertyGroupElement grp)
        {
            ToolChainOptions opts = new ToolChainOptions();
            List<ProjectPropertyElement> remList = new List<ProjectPropertyElement>();

            foreach (ProjectPropertyElement bp in grp.Properties)
            {
                ToolFlag flag;
                remList.Add(bp);

                switch (bp.Name)
                {
                    case "ToolName":
                        opts.ToolName = bp.Value;
                        break;
                    case "DEVICE_TYPE":
                        opts.DeviceType = bp.Value;
                        break;
                    case CommonToolFlagTag:
                        flag = new ToolFlag();
                        flag.Flag = RemoveCommonFlags(bp.Value);
                        flag.Conditional = bp.Condition;
                        opts.CommonFlags.Add(flag);
                        break;
                    case CCppCommonFlagTag:
                    case CCppTargFlagTag:
                        flag = new ToolFlag();
                        flag.Flag = RemoveCommonFlags(bp.Value);
                        flag.Conditional = bp.Condition;
                        opts.C_CppFlags.Add(flag);
                        break;
                    case CppFlagTag:
                        flag = new ToolFlag();
                        flag.Flag = RemoveCommonFlags(bp.Value);
                        flag.Conditional = bp.Condition;
                        opts.CppFlags.ToolFlags.Add(flag);
                        break;
                    case AsmFlagTag:
                    case AsmArmFlagTag:
                        flag = new ToolFlag();
                        flag.Flag = RemoveCommonFlags(bp.Value);
                        flag.Conditional = bp.Condition;
                        opts.AsmFlags.ToolFlags.Add(flag);
                        break;
                    case ArchiverFlagTag:
                    case ArchiverArmFlagTag:
                    case ArchiverPlatFlagTag:
                        flag = new ToolFlag();
                        flag.Flag = RemoveCommonFlags(bp.Value);
                        flag.Conditional = bp.Condition;
                        opts.ArchiverFlags.ToolFlags.Add(flag);
                        break;
                    case LinkerFlagTag:
                        flag = new ToolFlag();
                        flag.Flag = RemoveCommonFlags(bp.Value);
                        flag.Conditional = bp.Condition;
                        opts.LinkerFlags.ToolFlags.Add(flag);
                        break;

                    default:
                        {
                            string uc = bp.Name.ToUpper();

                            if (uc.Contains("CC_CPP_"))
                            {
                                flag = new ToolFlag();
                                flag.Flag = RemoveCommonFlags(bp.Value);
                                flag.Conditional = bp.Condition;
                                opts.C_CppFlags.Add(flag);
                            }
                            else if (uc.Contains("CC_"))
                            {
                                flag = new ToolFlag();
                                flag.Flag = RemoveCommonFlags(bp.Value);
                                flag.Conditional = bp.Condition;
                                opts.CFlags.ToolFlags.Add(flag);
                            }
                            else if (uc.Contains("AS_"))
                            {
                                flag = new ToolFlag();
                                flag.Flag = RemoveCommonFlags(bp.Value);
                                flag.Conditional = bp.Condition;
                                opts.AsmFlags.ToolFlags.Add(flag);
                            }
                            else
                            {
                                remList.Remove(bp);
                            }
                        }
                        break;
                }
            }
            foreach (ProjectPropertyElement bp in remList)
            {
                grp.RemoveChild(bp);
            }

            return opts;
        }

        private List<BuildToolRef> LoadBuildToolFlags(Project proj)
        {
            List<BuildToolRef> toolRefs = new List<BuildToolRef>();

            Dictionary<ProjectPropertyElement, ProjectPropertyGroupElement> remList = new Dictionary<ProjectPropertyElement, ProjectPropertyGroupElement>();

            foreach (ProjectPropertyGroupElement pg in proj.Xml.PropertyGroups)
            {
                Regex exp = new Regex("\\s*'\\$\\(COMPILER_TOOL\\)'\\s*==\\s*'([\\w\\W]+)'");
                BuildToolRef btr = null;

                if (exp.IsMatch(pg.Condition))
                {
                    btr = new BuildToolRef();
                    toolRefs.Add(btr);
                    btr.Name = exp.Match(pg.Condition).Groups[1].Value;
                }
                else
                {
                    continue;
                }

                btr.BuildOptions = LoadToolChainOptions(pg);

                foreach (ProjectPropertyElement bp in pg.Properties)
                {
                    remList[bp] = pg;

                    switch (bp.Name)
                    {
                        case "BUILD_TOOL_GUID":
                            btr.Guid = bp.Value;
                            break;
                        case "DEVICE_TYPE":
                            btr.BuildOptions.DeviceType = bp.Value;
                            break;

                        default:
                            remList.Remove(bp);
                            break;
                    }
                }
            }

            foreach (ProjectPropertyElement bp in remList.Keys)
            {
                remList[bp].RemoveChild(bp);
            }

            return toolRefs;
        }


        private Project GenerateBuildToolTargetFile(BuildTool buildTool)
        {
            Project proj = new Project();
            proj.Xml.DefaultTargets = "Build";

            ProjectPropertyGroupElement mainGrp = proj.Xml.AddPropertyGroup();
            mainGrp.AddProperty("BuildToolName", buildTool.Name);
            mainGrp.AddProperty("BuildToolGuid", buildTool.Guid);
            mainGrp.AddProperty("Documentation", buildTool.Documentation);
            if (buildTool.SupportedCpuNames.Count > 0)
            {
                mainGrp.AddProperty(CpuNamesTag, string.Join(";", buildTool.SupportedCpuNames.ToArray()));
            }
            if (buildTool.SupportedISAs.Count > 0)
            {
                foreach (ISA isa in buildTool.SupportedISAs)
                {
                    ProjectPropertyGroupElement bgISA = proj.Xml.AddPropertyGroup();

                    bgISA.Condition = "'$(INSTRUCTION_SET)'=='" + isa.Name + "'";

                    bgISA.AddProperty(ISAsTag, isa.Name);

                    AddBuildToolFlags(isa.BuildToolOptions, proj, bgISA);
                }
            }

            AddBuildToolProperties(buildTool, proj);

            if (buildTool.Properties.Count > 0)
            {
                ProjectPropertyGroupElement bpg = proj.Xml.AddPropertyGroup();
                foreach (MFProperty prop in buildTool.Properties)
                {
                    ProjectPropertyElement bp = bpg.AddProperty(prop.Name, prop.Value);
                    bp.Condition = prop.Condition;
                }
            }

            AddEnvVarCollection(buildTool.BuildOptions.EnvironmentVariables, proj);
            AddBuildToolFlags(buildTool.BuildOptions, proj, null);
            AddMiscToolFlags(buildTool.MiscTools, proj);


            if (buildTool.Items.Count > 0)
            {
                ProjectItemGroupElement big = proj.Xml.AddItemGroup();
                foreach (MFProperty prop in buildTool.Items)
                {
                    ProjectItemElement bi = big.AddItem(prop.Name, prop.Value);
                    bi.Condition = prop.Condition;
                }
            }

            AddBuildTools(buildTool, proj);

            return proj;
        }

        private Project GenerateBuildOptionTargetFile(ToolChainOptions ToolChainOptions)
        {
            Project proj = new Project();
            proj.Xml.DefaultTargets = "Build";

            AddEnvVarCollection(ToolChainOptions.EnvironmentVariables, proj);
            AddBuildToolFlags(ToolChainOptions, proj, null);

            return proj;
        }

        private string RemoveCommonFlags(string script)
        {
            string[] typeFlags = {
                CommonToolFlagTag,
                CCppTargFlagTag,
                CCppCommonFlagTag,
                CFlagTag,
                CppFlagTag,
                AsmFlagTag,
                LinkerFlagTag,
                ArchiverFlagTag,
                ArchiverArmFlagTag,
                ScatterFlagTag,
                AsmArmFlagTag,
                ArchiverPlatFlagTag,
            };

            int idx = -1;
            foreach (string flag in typeFlags)
            {
                string flagEnv = ToEnvVar(flag).Trim();
                if ((idx = script.ToLower().IndexOf(flagEnv.ToLower())) >= 0)
                {
                    script = script.Remove(idx, flagEnv.Length);
                }
            }

            return script.Trim();
        }

        private string RemoveWrapper(string script)
        {
            string wrapperCmd = ToEnvVar(ToolWrapperTag.ToLower()).Trim();
            string adsWrapper = ToEnvVar(AdsWrapperTag.ToLower()).Trim();
            string adiWrapper = ToEnvVar(AdiWrapperTag.ToLower()).Trim();
            string arcWrapper = ToEnvVar(ArcWrapperTag.ToLower()).Trim();

            script = script.TrimStart('"');
            script = script.TrimEnd('"');

            int idx = -1;

            if ((idx = script.ToLower().IndexOf(wrapperCmd)) >= 0)
            {
                script = script.Remove(idx, wrapperCmd.Length);
            }
            else if ((idx = script.ToLower().IndexOf(adsWrapper)) >= 0)
            {
                script = script.Remove(idx, adsWrapper.Length);
            }
            else if ((idx = script.ToLower().IndexOf(adiWrapper)) >= 0)
            {
                script = script.Remove(idx, adiWrapper.Length);
            }

            return script;
        }

        private string RemoveFlagsAndWrapper(string script, ToolOptions toolOptions, params List<ToolFlag>[] commonFlags)
        {
            int idx = -1;
            string scriptLower = script.ToLower();

            foreach (ToolFlag flag in toolOptions.ToolFlags)
            {
                scriptLower = script.ToLower();
                if ((idx = scriptLower.IndexOf(flag.Flag.ToLower())) >= 0)
                {
                    script = script.Remove(idx, flag.Flag.Length);
                }
            }

            if (commonFlags != null)
            {
                foreach (List<ToolFlag> flags in commonFlags)
                {
                    foreach (ToolFlag flag in flags)
                    {
                        scriptLower = script.ToLower();
                        if ((idx = scriptLower.IndexOf(flag.Flag.ToLower())) >= 0)
                        {
                            script = script.Remove(idx, flag.Flag.Length);
                        }
                    }
                }
            }

            script = RemoveWrapper(script);

            script = RemoveCommonFlags(script);

            return script.Trim();
        }

        private void LoadTargetTasks(ProjectTargetElement target, List<EnvVar> vars, BuildToolDefine tool, ToolOptions toolOptions, params List<ToolFlag>[] commonFlags)
        {
            foreach (ProjectTaskElement task in target.Tasks)
            {
                BuildScript script = new BuildScript();

                string exeCmdEnv = ToEnvVar(tool.Description.ToLower()).Trim();
                string exeCmd = tool.Description.ToLower();

                int idx = -1;
                script.Conditional = task.Condition;
                script.Script = task.GetParameter(TaskScriptAttributeName);
                if (!string.IsNullOrEmpty(script.Script))
                {
                    if ((idx = script.Script.ToLower().IndexOf(exeCmdEnv)) >= 0)
                    {
                        // remove exe command and 
                        script.Script = script.Script.Remove(idx, exeCmdEnv.Length);

                        script.Script = RemoveFlagsAndWrapper(script.Script, toolOptions, commonFlags);

                        toolOptions.BuildToolParameters.Parameters.Add(script);
                    }
                    else if ((idx = script.Script.ToLower().IndexOf(exeCmd)) >= 0)
                    {
                        // remove exe command and 
                        script.Script = script.Script.Remove(idx, exeCmd.Length);

                        script.Script = RemoveFlagsAndWrapper(script.Script, toolOptions, commonFlags);

                        toolOptions.BuildToolParameters.Parameters.Add(script);
                    }
                    else if (toolOptions.BuildToolParameters.Parameters.Count > 0)
                    {
                        toolOptions.BuildToolParameters.PostBuild.Add(script);
                    }
                    else
                    {
                        toolOptions.BuildToolParameters.PreBuild.Add(script);
                    }
                }
                else if (string.Compare(task.Name, SetEnvScriptTag) == 0)
                {
                    EnvVar var = new EnvVar();
                    var.Name = task.GetParameter("Name");
                    var.Conditional = task.Condition;
                    var.Value = task.GetParameter("Value");
                    vars.Add(var);
                }
            }
        }

        private string ExtractToolPath(BuildTool tool, string exeLocation)
        {
            if (string.IsNullOrEmpty(exeLocation)) return "";

            exeLocation = exeLocation.Trim();

            exeLocation = exeLocation.TrimStart(' ', '"', '\\');
            exeLocation = exeLocation.TrimEnd(' ', '"');

            exeLocation = exeLocation.Replace(ToEnvVar(AdiWrapperTag), "");
            exeLocation = exeLocation.Replace(ToEnvVar(ArcWrapperTag), "");
            exeLocation = exeLocation.Replace(ToEnvVar(AdsWrapperTag), "");

            if (string.IsNullOrEmpty(tool.ToolPath))
            {
                string tmp = exeLocation.Replace("\"", "");

                tool.ToolPath = Path.GetDirectoryName(tmp) + "\\";
                exeLocation = exeLocation.Remove(0, tool.ToolPath.Length).TrimStart(' ', '\\', '"');
            }
            else if (exeLocation.ToLower().StartsWith(tool.ToolPath.ToLower()))
            {
                exeLocation = exeLocation.Remove(0, tool.ToolPath.Length).TrimStart(' ', '\\', '"');
            }
            else
            {
                do
                {
                    tool.ToolPath = Path.GetDirectoryName(tool.ToolPath);
                }
                while (tool.ToolPath.Length > 0 && exeLocation.ToLower().StartsWith(tool.ToolPath.ToLower()));

                if (tool.ToolPath.Length > 0)
                {
                    exeLocation = exeLocation.Remove(0, tool.ToolPath.Length);
                }
            }

            return exeLocation.TrimStart('\\', ' ', '"');
        }

        private string ExpandVars(string text, string path)
        {
            text = text.Replace("$(", "%");
            text = text.Replace(")", "%");

            text = Environment.ExpandEnvironmentVariables(text);

            string var = "%spoclient%";
            if (text.ToLower().StartsWith(var))
            {
                text = text.Replace(text.Substring(0, var.Length), Path.Combine(path, "..\\..\\"));
            }
            var = "%spo_sdk%";
            if (text.ToLower().StartsWith(var))
            {
                text = text.Replace(text.Substring(0, var.Length), Path.Combine(path, "..\\..\\"));
            }
            var = "%spo_root%";
            if (text.ToLower().StartsWith(var))
            {
                text = text.Replace(text.Substring(0, var.Length), Path.Combine(path, "..\\..\\..\\"));
            }

            return text;
        }

        internal static string ExpandEnvVars(string text, string path)
        {
            Regex exp = new Regex("\\$\\(([^\\)]*)\\)");

            if (string.IsNullOrEmpty(text)) return "";

            MatchCollection match = exp.Matches(text);

            if (match.Count > 0)
            {
                for (int i = match.Count - 1; i >= 0; i--)
                {
                    text = text.Replace("$(" + match[i].Groups[1].Value + ")", "%" + match[i].Groups[1].Value + "%");
                }
            }

            text = Environment.ExpandEnvironmentVariables(text);

            if (!File.Exists(text) && !Directory.Exists(text) && !string.IsNullOrEmpty(path) && !Path.IsPathRooted(path))
            {
                text = Path.Combine(path.Trim(), text.Trim());
            }

            bool isNetworkDrive = text.StartsWith("\\\\");

            text = text.Replace("\\\\", "\\");

            if (isNetworkDrive) text = "\\" + text;

            return text;
        }

        internal string ConvertPathToEnv(string fullpath)
        {
            string spoclient = Environment.GetEnvironmentVariable("SPOCLIENT");
            if (!string.IsNullOrEmpty(spoclient))
            {
                if (fullpath.ToLower().StartsWith(spoclient.ToLower()))
                {
                    fullpath = fullpath.Remove(0, spoclient.Length).TrimStart('\\');
                    fullpath = "$(SPOCLIENT)\\" + fullpath;
                }
            }
            return fullpath;
        }

        private string SerializeXml(object obj)
        {
            XmlSerializer xser = new XmlSerializer(obj.GetType());

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            xser.Serialize(sw, obj);

            sb.Remove(0, sb.ToString().IndexOf("?>") + 2);

            return sb.ToString();
        }

        private object DeserializeXml(string data, Type type)
        {
            object ret = null;

            StringBuilder sb = new StringBuilder(data);

            sb.Insert(0, c_XmlHeaderString);

            XmlSerializer xser = new XmlSerializer(type);

            using (StringReader sr = new StringReader(sb.ToString()))
            {
                ret = xser.Deserialize(sr);
            }

            return ret;
        }

        private ProjectPropertyGroupElement SaveStringProps(Project proj, object obj, Dictionary<string, string> tbl)
        {
            ProjectPropertyGroupElement bpg = proj.Xml.AddPropertyGroup();

            foreach (PropertyInfo pi in obj.GetType().GetProperties())
            {
                if (pi.PropertyType == typeof(string))
                {
                    string val = (string)pi.GetValue(obj, null);

                    if (val == null) val = "";

                    string name = pi.Name;

                    if (tbl.ContainsKey(name)) name = tbl[name];
                    else tbl[name] = val;

                    bpg.AddProperty(name, val);
                }
                else if (pi.PropertyType == typeof(List<string>))
                {
                    string list = "";
                    string name = pi.Name;
                    List<string> vals = (List<string>)pi.GetValue(obj, null);

                    foreach (string val in vals)
                    {
                        if (val == null) continue;

                        list += val + ";";
                    }
                    if (tbl.ContainsKey(name)) name = tbl[name];
                    else tbl[name] = list;

                    bpg.AddProperty(name, list);
                }
                else if (pi.PropertyType == typeof(bool))
                {
                    bpg.AddProperty(pi.Name, pi.GetValue(obj, null).ToString());
                }
                else if (pi.PropertyType == typeof(MFComponent))
                {
                    MFComponent cmp = (MFComponent)pi.GetValue(obj, null);
                    if (cmp != null && !string.IsNullOrEmpty(cmp.Guid))
                    {
                        bpg.AddProperty(pi.Name, SerializeXml(cmp));
                    }
                }
                else if (pi.PropertyType == typeof(LibraryLevel))
                {
                    bpg.AddProperty(pi.Name, pi.GetValue(obj, null).ToString());
                }
            }

            return bpg;
        }

        private void LoadStringProps(Project proj, object obj, Dictionary<string, string> tbl)
        {
            foreach (ProjectPropertyGroupElement bpg in proj.Xml.PropertyGroups)
            {
                foreach (ProjectPropertyElement bp in bpg.Properties)
                {
                    string name = bp.Name;

                    if (tbl.ContainsKey(name)) name = tbl[name];

                    PropertyInfo pi = obj.GetType().GetProperty(name);

                    if (pi != null)
                    {
                        if (pi.PropertyType == typeof(string))
                        {
                            pi.SetValue(obj, bp.Value, null);
                        }
                        else if (pi.PropertyType == typeof(List<string>))
                        {
                            string[] items = ((string)bp.Value).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            pi.SetValue(obj, new List<string>(items), null);
                        }
                        else if (pi.PropertyType == typeof(bool))
                        {
                            pi.SetValue(obj, bool.Parse(bp.Value), null);
                        }
                        else if (pi.PropertyType == typeof(MFComponent) && !string.IsNullOrEmpty(bp.Value))
                        {
                            MFComponent comp = null;
                            try
                            {
                                comp = (MFComponent)DeserializeXml(bp.Value, typeof(MFComponent));
                            }
                            catch
                            {
                                comp = new MFComponent(MFComponentType.Unknown, bp.Value);
                            }

                            if (pi.Name == "LibraryCategory") comp.ComponentType = MFComponentType.LibraryCategory;
                            if (pi.Name == "ISASpecific") comp.ComponentType = MFComponentType.ISA;
                            if (pi.Name == "ProcessorSpecific") comp.ComponentType = MFComponentType.Processor;
                            if (pi.Name == "Processor") comp.ComponentType = MFComponentType.Processor;
                            if (pi.Name == "DefaultISA") comp.ComponentType = MFComponentType.ISA;

                            pi.SetValue(obj, comp, null);
                        }
                        else if (pi.PropertyType == typeof(LibraryLevel))
                        {
                            pi.SetValue(obj, Enum.Parse(typeof(LibraryLevel), bp.Value), null);
                        }
                    }
                }
            }
        }

        private void SaveCompileItems(Project proj, params List<MFBuildFile>[] collections)
        {
            ProjectItemGroupElement big = proj.Xml.AddItemGroup();

            foreach (List<MFBuildFile> files in collections)
            {
                foreach (MFBuildFile file in files)
                {
                    ProjectItemElement bi = big.AddItem(file.ItemName, file.File);
                    bi.Condition = file.Condition;
                }
            }
        }

        private void LoadCompileItems(Project proj, object obj, string path)
        {
            foreach (ProjectItemGroupElement big in proj.Xml.ItemGroups)
            {
                foreach (ProjectItemElement bi in big.Items)
                {
                    string cond = CombineConditionals(big.Condition, bi.Condition);

                    switch (bi.ItemType)
                    {
                        case "FastCompileCPPFile":
                            {
                                PropertyInfo pi = obj.GetType().GetProperty("FastCompileFiles");

                                if (pi != null)
                                {
                                    List<MFBuildFile> list = (List<MFBuildFile>)pi.GetValue(obj, null);

                                    MFBuildFile f = new MFBuildFile();
                                    f.Condition = cond;
                                    f.File = bi.Include;
                                    f.ItemName = bi.ItemType;

                                    list.Add(f);
                                }
                            }
                            break;
                        case "Compile":
                            {
                                PropertyInfo pi = obj.GetType().GetProperty("SourceFiles");

                                if (pi != null)
                                {
                                    List<MFBuildFile> list = (List<MFBuildFile>)pi.GetValue(obj, null);

                                    MFBuildFile f = new MFBuildFile();
                                    f.Condition = cond;
                                    f.File = bi.Include;
                                    f.ItemName = bi.ItemType;

                                    list.Add(f);
                                }
                            }
                            break;
                        case "HFiles":
                            {
                                PropertyInfo pi = obj.GetType().GetProperty("HeaderFiles");

                                if (pi != null)
                                {
                                    List<MFBuildFile> list = (List<MFBuildFile>)pi.GetValue(obj, null);

                                    MFBuildFile f = new MFBuildFile();
                                    f.Condition = cond;
                                    f.File = bi.Include;
                                    f.ItemName = bi.ItemType;

                                    list.Add(f);
                                }
                            }
                            break;
                        case "IncludePaths":
                            {
                                PropertyInfo pi = obj.GetType().GetProperty("IncludePaths");

                                if (pi != null)
                                {
                                    List<MFBuildFile> list = (List<MFBuildFile>)pi.GetValue(obj, null);

                                    MFBuildFile f = new MFBuildFile();
                                    f.Condition = cond;
                                    f.File = bi.Include;
                                    f.ItemName = bi.ItemType;

                                    list.Add(f);
                                }
                            }
                            break;
                    }
                }
            }

        }

        private void SaveDependentLibs(Project proj, List<MFComponent> deps)
        {
            foreach (MFComponent comp in deps)
            {
                if (comp.ComponentType == MFComponentType.Library)
                {
                    ProjectItemGroupElement big = proj.Xml.AddItemGroup();
                    Library lib = null;

                    if (!string.IsNullOrEmpty(comp.ProjectPath) && Path.GetExtension(comp.ProjectPath).ToUpper() == ".PROJ")
                    {
                        lib = LoadLibraryProj(comp.ProjectPath, Path.GetDirectoryName(proj.FullPath), true);
                    }

                    if (lib == null)
                    {
                        lib = m_helper.FindLibrary(comp);
                    }

                    if (lib != null)
                    {
                        string tag = "DriverLibs";
                        string condProj = comp.Conditional;
                        string condLib = comp.Conditional;

                        if (lib.PlatformIndependent)
                        {
                            tag = "PlatformIndependentLibs";
                        }

                        // normalize the path (no xxx\..\yyy in the path)
                        if (string.IsNullOrEmpty(lib.ProjectPath))
                        {
                            lib.ProjectPath = comp.ProjectPath;
                        }

                        string fullpath = ExpandEnvVars(lib.ProjectPath, "");
                        fullpath = Path.GetFullPath(fullpath);
                        fullpath = ConvertPathToEnv(fullpath);

                        ProjectItemElement bi2 = big.AddItem("RequiredProjects", fullpath);
                        bi2.Condition = condProj;

                        ProjectItemElement bi = big.AddItem(tag, lib.LibraryFile);
                        bi.Condition = condLib;

                        proj.Save(proj.FullPath);
                    }
                    else
                    {
                        Console.WriteLine("Error: unknown library dependency while saving project file " + comp.Name);
                    }
                }
                else
                {
                    Console.WriteLine("Error: unexpected component dependency while saving project file " + comp.Name);
                }
            }
        }

        private void UpdatePropertyGroups(Project proj, object obj, string[] projNames, string[] objNames)
        {
            Dictionary<string, ProjectPropertyElement> lookup = new Dictionary<string, ProjectPropertyElement>();

            foreach (ProjectPropertyGroupElement bpg in proj.Xml.PropertyGroups)
            {
                foreach (ProjectPropertyElement bp in bpg.Properties)
                {
                    lookup[bp.Name.ToUpper()] = bp;
                }
            }
            ProjectPropertyGroupElement buildGrp = null;

            for (int i = 0; i < projNames.Length; i++)
            {
                string prop = projNames[i];
                string objName = (objNames == null || objNames.Length <= i || objNames[i].Length == 0) ? prop : objNames[i];

                if (lookup.ContainsKey(prop.ToUpper()))
                {
                    string val = (string)obj.GetType().GetProperty(objName).GetValue(obj, null);

                    if (val == null) val = "";

                    lookup[prop.ToUpper()].Value = val;
                }
                else
                {
                    if (buildGrp == null) buildGrp = proj.Xml.AddPropertyGroup();

                    PropertyInfo pi = obj.GetType().GetProperty(objName);

                    object val = pi.GetValue(obj, null);

                    if (pi.PropertyType == typeof(string))
                    {
                        if (val == null) val = "";
                    }

                    buildGrp.AddProperty(prop, val.ToString());
                }
            }
        }

        private void LoadPropertyGroups(Project proj, object obj, string[] projNames, string[] objNames)
        {
            Dictionary<string, ProjectPropertyElement> lookup = new Dictionary<string, ProjectPropertyElement>();

            foreach (ProjectPropertyGroupElement bpg in proj.Xml.PropertyGroups)
            {
                foreach (ProjectPropertyElement bp in bpg.Properties)
                {
                    lookup[bp.Name.ToUpper()] = bp;
                }
            }

            for (int i = 0; i < projNames.Length; i++)
            {
                string prop = projNames[i];
                string objName = (objNames == null || objNames.Length <= i || objNames[i].Length == 0) ? prop : objNames[i];

                if (lookup.ContainsKey(prop.ToUpper()))
                {
                    obj.GetType().GetProperty(objName).SetValue(obj, lookup[prop.ToUpper()].Value, null);
                }
                else
                {
                    //Console.WriteLine("Warning! Unable to find property " + prop);
                }
            }
        }

        // -- //

        internal void SaveTargetFile(BuildTool tool)
        {
            string fullpath = ExpandEnvVars(tool.ProjectPath, "");
            try
            {
                Project proj = GenerateBuildToolTargetFile(tool);
                proj.Save(fullpath);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: saving target file: " + fullpath + "\r\n", e.Message);
            }
        }

        private ISA LoadISASettings(ProjectPropertyGroupElement isaGrp)
        {
            ISA isa = new ISA();

            isa.BuildToolOptions = LoadToolChainOptions(isaGrp);

            foreach (ProjectPropertyElement prop in isaGrp.Properties)
            {
                switch (prop.Name)
                {
                    case ISAsTag:
                        isa.Name = prop.Value;
                        break;
                }
            }
            return isa;
        }

        internal void LoadTargetFile(BuildTool tool, string fileName)
        {
            Project proj;
            string fullpath = ExpandEnvVars(fileName, "");

            try
            {
                proj = LoadProject(fullpath);

                foreach (ProjectImportElement import in proj.Xml.Imports)
                {
                    if (string.IsNullOrEmpty(import.Condition))
                    {
                        LoadTargetFile(tool, ExpandVars(import.Project, Path.GetDirectoryName(fileName)));
                    }
                }

                Regex expISA = new Regex(@"'\$\(INSTRUCTION_SET\)'=='", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

                foreach (ProjectPropertyGroupElement pg in proj.Xml.PropertyGroups)
                {
                    if (expISA.IsMatch(pg.Condition))
                    {
                        ISA isa = LoadISASettings(pg);
                        if (isa != null)
                        {
                            tool.SupportedISAs.Add(isa);
                        }
                        continue;
                    }

                    tool.BuildOptions = LoadToolChainOptions(pg);

                    foreach (ProjectPropertyElement bp in pg.Properties)
                    {
                        string cond = CombineConditionals(pg.Condition, bp.Condition);

                        switch (bp.Name)
                        {
                            case "BuildToolName":
                                tool.Name = bp.Value;
                                break;
                            case "Documentation":
                                tool.Documentation = bp.Value;
                                break;
                            case "BuildToolGuid":
                                tool.Guid = bp.Value;
                                break;
                            case ObjExtFlag:
                                tool.ObjExt = bp.Value;
                                break;
                            case LibExtFlag:
                                tool.LibExt = bp.Value;
                                break;
                            case ExeExtFlag:
                                tool.BinExt = bp.Value;
                                break;
                            case ScatterExtFlag:
                                tool.ScatterExt = bp.Value;
                                break;
                            case CCompilerTag:
                                tool.CCompiler.Exec = RemoveWrapper(bp.Value);
                                break;
                            case CppCompilerTag:
                                tool.CppCompiler.Exec = RemoveWrapper(bp.Value);
                                break;
                            case AsmCompilerTag:
                                tool.AsmCompiler.Exec = RemoveWrapper(bp.Value);
                                break;
                            case ArchiverTag:
                                tool.Archiver.Exec = RemoveWrapper(bp.Value);
                                break;
                            case LinkerTag:
                                tool.Linker.Exec = RemoveWrapper(bp.Value);
                                break;
                            case FromElfTag:
                                tool.FromELF.Exec = RemoveWrapper(bp.Value);
                                break;
                            case ToolWrapperTag:
                            case AdsWrapperTag:
                            case AdiWrapperTag:
                            case ArcWrapperTag:
                                tool.BuildToolWrapper = RemoveWrapper(bp.Value);
                                break;
                            case PKUI_CpuNamesTag:
                                {
                                    List<string> pts = (List<string>)this.DeserializeXml(bp.Value, typeof(List<string>));
                                    tool.SupportedCpuNames.AddRange(pts);
                                }
                                break;
                            case "ProcessorTypes":
                            case CpuNamesTag:
                                {
                                    foreach (string sType in bp.Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                                    {
                                        tool.SupportedCpuNames.Add(sType);
                                    }
                                }
                                break;
                            case PKUI_ISAsTag:
                                {
                                    List<ISA> isas = (List<ISA>)this.DeserializeXml(bp.Value, typeof(List<ISA>));
                                    tool.SupportedISAs.AddRange(isas);
                                }
                                break;
                            case ISAsTag:
                                {
                                    //legacy
                                    if (bp.Value.Contains(";"))
                                    {
                                        foreach (string sISA in bp.Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                                        {
                                            ISA isa = new ISA();
                                            isa.Name = sISA;
                                            isa.Guid = System.Guid.NewGuid().ToString("B");

                                            tool.SupportedISAs.Add(isa);
                                        }
                                    }
                                }
                                break;

                            default:
                                {
                                    MFProperty prop = new MFProperty();
                                    prop.Name = bp.Name;
                                    prop.Value = bp.Value;
                                    prop.Condition = cond;

                                    tool.Properties.Add(prop);
                                }
                                break;
                        }
                    }
                }

                foreach (ProjectItemGroupElement big in proj.Xml.ItemGroups)
                {
                    foreach (ProjectItemElement bi in big.Items)
                    {
                        string cond = CombineConditionals(big.Condition, bi.Condition);

                        MFProperty prop = new MFProperty();
                        prop.Name = bi.ItemType;
                        prop.Value = bi.Include;
                        prop.Condition = cond;

                        tool.Items.Add(prop);
                    }
                }

                foreach (ProjectTargetElement targ in proj.Xml.Targets)
                {
                    switch (targ.Name)
                    {
                        case AdiAsmTargetTag:
                        case ArmAsmTargetTag:
                        case ArcAsmTargetTag:
                        case AsmCompilerTargetTag:
                            tool.AsmCompiler.Conditional = targ.Condition;
                            tool.AsmCompiler.Description = AsmCompilerTag;
                            LoadTargetTasks(targ, tool.BuildOptions.EnvironmentVariables, tool.AsmCompiler, tool.BuildOptions.AsmFlags, tool.BuildOptions.CommonFlags);
                            break;
                        case AdiCCompilerTargetTag:
                        case ArmCCompilerTargetTag:
                        case ArcCCompilerTargetTag:
                        case CCompilerTargetTag:
                            tool.CCompiler.Conditional = targ.Condition;
                            tool.CCompiler.Description = CCompilerTag;
                            LoadTargetTasks(targ, tool.BuildOptions.EnvironmentVariables, tool.CCompiler, tool.BuildOptions.CFlags, tool.BuildOptions.C_CppFlags, tool.BuildOptions.CommonFlags);
                            break;
                        case AdiCppCompilerTargetTag:
                        case ArmCppCompilerTargetTag:
                        case ArcCppCompilerTargetTag:
                        case CppCompilerTargetTag:
                            tool.CppCompiler.Conditional = targ.Condition;
                            tool.CppCompiler.Description = CppCompilerTag;
                            LoadTargetTasks(targ, tool.BuildOptions.EnvironmentVariables, tool.CppCompiler, tool.BuildOptions.CppFlags, tool.BuildOptions.C_CppFlags, tool.BuildOptions.CommonFlags);
                            break;
                        case AdiLibTargetTag:
                        case ArmLibTargetTag:
                        case ArcLibTargetTag:
                        case LibTargetTag:
                            tool.Archiver.Conditional = targ.Condition;
                            tool.Archiver.Description = ArchiverTag;
                            LoadTargetTasks(targ, tool.BuildOptions.EnvironmentVariables, tool.Archiver, tool.BuildOptions.ArchiverFlags);
                            break;
                        case AdiExeTargetTag:
                        case ArmExeTargetTag:
                        case ArcExeTargetTag:
                        case ExeTargetTag:
                            tool.Linker.Conditional = targ.Condition;
                            tool.Linker.Description = LinkerTag;
                            LoadTargetTasks(targ, tool.BuildOptions.EnvironmentVariables, tool.Linker, tool.BuildOptions.LinkerFlags);
                            break;
                        default:
                            MiscBuildTool misc = new MiscBuildTool();
                            misc.BuildTool.Conditional = targ.Condition;
                            misc.BuildTool.Exec = targ.Name;
                            misc.BuildTool.Description = targ.Name;
                            misc.Name = targ.Name;
                            LoadTargetTasks(targ, misc.EnvironmentVariables, misc.BuildTool, misc.BuildToolOptions);
                            tool.MiscTools.Add(misc);
                            break;
                    }
                }

                BuildToolDefine[] tools = new BuildToolDefine[]
            {
                tool.Archiver,
                tool.AsmCompiler,
                tool.CCompiler,
                tool.CppCompiler,
                tool.Linker,
                tool.FromELF,
            };

                //first time to determine path
                foreach (BuildToolDefine td in tools)
                {
                    ExtractToolPath(tool, td.Exec);
                }
                //second time to remove path from exec command
                foreach (BuildToolDefine td in tools)
                {
                    td.Exec = ExtractToolPath(tool, td.Exec);
                }

                tool.ProjectPath = ConvertPathToEnv(fileName);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: loading target file: " + fullpath + "\r\n", e.Message);
            }
        }

        internal void SaveLibraryProj(Library lib)
        {
            if (!string.IsNullOrEmpty(lib.ProjectPath))
            {
                string fullpath = ExpandEnvVars(lib.ProjectPath, "");

                Project proj = LoadProject(fullpath);

                proj.ProjectCollection.UnloadProject(proj);

                proj = new Project();
                proj.Xml.DefaultTargets = "Build";

                try
                {
                    // save properties first
                    Dictionary<string, string> tbl = new Dictionary<string, string>();
                    tbl["Name"] = "AssemblyName";
                    tbl["Guid"] = "ProjectGuid";

                    ProjectPropertyGroupElement bpg = SaveStringProps(proj, lib, tbl);

                    List<MFProperty> delayedProps = new List<MFProperty>();
                    foreach (MFProperty prop in lib.Properties)
                    {
                        if (!prop.Name.Contains("$(") && !prop.Condition.Contains("$("))
                        {
                            ProjectPropertyElement bp = bpg.AddProperty(prop.Name, prop.Value);
                            bp.Condition = prop.Condition;
                        }
                        else
                        {
                            delayedProps.Add(prop);
                        }
                    }
                    bpg.AddProperty("PlatformIndependentBuild", lib.PlatformIndependent.ToString().ToLower());
                    bpg.AddProperty("Version", lib.Version.Major + "." + lib.Version.Minor + "." + lib.Version.Revision + "." + lib.Version.Build);

                    // add standard import
                    proj.Xml.AddImport(@"$(SPOCLIENT)\tools\targets\Microsoft.SPOT.System.Settings");

                    proj.Save(fullpath);

                    bpg = proj.Xml.AddPropertyGroup();
                    foreach (MFProperty prop in delayedProps)
                    {
                        ProjectPropertyElement bp = bpg.AddProperty(prop.Name, prop.Value);
                        bp.Condition = prop.Condition;
                    }

                    // add item group
                    SaveCompileItems(proj, lib.FastCompileFiles, lib.HeaderFiles, lib.SourceFiles, lib.OtherFiles, lib.IncludePaths);

                    ProjectItemGroupElement big = proj.Xml.AddItemGroup();
                    foreach (MFComponent cmp in lib.Dependencies)
                    {
                        switch (cmp.ComponentType)
                        {
                            case MFComponentType.Library:
                                Library l = m_helper.FindLibrary(cmp);
                                if (l != null)
                                {
                                    ProjectItemElement bi = big.AddItem("RequiredProjects", ConvertPathToEnv(l.ProjectPath));
                                    bi.Condition = cmp.Conditional;
                                }
                                else if (!string.IsNullOrEmpty(cmp.ProjectPath))
                                {
                                    ProjectItemElement bi = big.AddItem("RequiredProjects", ConvertPathToEnv(cmp.ProjectPath));
                                    bi.Condition = cmp.Conditional;
                                }
                                break;
                            case MFComponentType.LibraryCategory:
                                ProjectImportElement pie = proj.Xml.AddImport(cmp.ProjectPath);
                                pie.Condition = cmp.Conditional;
                                break;
                            default:
                                System.Diagnostics.Debug.Assert(false);
                                break;
                        }
                    }

                    foreach (ProjectTargetElement targ in lib.Targets)
                    {
                        ProjectTargetElement t = proj.Xml.AddTarget(targ.Name);

                        t.Condition = targ.Condition;
                        t.DependsOnTargets = targ.DependsOnTargets;
                        t.Inputs = targ.Inputs;
                        t.Outputs = targ.Outputs;

                        foreach (ProjectTaskElement task in targ.Tasks)
                        {
                            ProjectTaskElement tsk = t.AddTask(task.Name);
                            tsk.Condition = task.Condition;
                            tsk.ContinueOnError = task.ContinueOnError;

                            foreach (KeyValuePair<string, string> param in task.Parameters)
                            {
                                tsk.SetParameter(param.Key, param.Value);
                            }
                        }
                    }

                    proj.Xml.AddImport(@"$(SPOCLIENT)\tools\targets\Microsoft.SPOT.System.Targets");

                    proj.Save(fullpath);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Error: saving Library file: " + fullpath + "\r\n", e.Message);
                }
            }
        }

        internal Library LoadLibraryFromManifest(string libManifestFile)
        {
            Project proj;
            Library lib = null;

            string fullpath = ExpandEnvVars(libManifestFile, "");

            try
            {
                if (File.Exists(fullpath))
                {
                    lib = new Library();

                    proj = LoadProject(fullpath);

                    Dictionary<string, string> tbl = new Dictionary<string, string>();
                    tbl["AssemblyName"] = "Name";
                    tbl["ProjectGuid"] = "Guid";

                    LoadStringProps(proj, lib, tbl);

                    Library dbLib = m_helper.FindLibrary(lib.Guid);
                    if (dbLib != null)
                    {
                        System.Diagnostics.Debug.Assert(0 == string.Compare(lib.Guid, dbLib.Guid, true));
                        return dbLib;
                    }

                    foreach (ProjectImportElement imp in proj.Xml.Imports)
                    {
                        switch (Path.GetExtension(imp.Project).ToUpper().Trim())
                        {
                            case ".LIBCATPROJ":
                                LibraryCategory lc = LoadLibraryCategoryProj(imp.Project, Path.GetDirectoryName(fullpath));
                                if (lc != null)
                                {
                                    MFComponent cmp = new MFComponent(MFComponentType.LibraryCategory, lc.Name, lc.Guid, lc.ProjectPath);
                                    lib.Dependencies.Add(cmp);
                                }
                                else
                                {
                                    System.Diagnostics.Debug.Assert(false);
                                }
                                break;
                        }
                    }

                    m_helper.AddLibraryToInventory(lib, true, m_helper.DefaultInventory);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: loading Library file: " + fullpath + "\r\n", e.Message);
                lib = null;
            }

            return lib;
        }

        internal Library LoadLibraryProj(string libProjFile, string path)
        {
            return LoadLibraryProj(libProjFile, path, false);
        }

        internal Library LoadLibraryProj(string libProjFile, string path, bool fForceReload)
        {
            Project proj;
            string fullpath = ExpandEnvVars(libProjFile, path);
            Library lib = null;

            try
            {
                lib = m_helper.FindLibraryByProject(fullpath);
                Library dbLib = null;

                if (!fForceReload)
                {
                    if (lib != null)
                    {
                        return lib;
                    }
                }

                if (!File.Exists(fullpath))
                {
                    return null;
                }

                if (lib == null)
                {
                    lib = new Library();
                }


                proj = LoadProject(fullpath);

                path = Path.GetDirectoryName(fullpath);

                lib.ProjectPath = ConvertPathToEnv(fullpath);

                if (fullpath.ToUpper().Contains("\\CLR\\"))
                {
                    lib.Level = LibraryLevel.CLR;
                }
                else if (fullpath.ToUpper().Contains("\\SUPPORT\\"))
                {
                    lib.Level = LibraryLevel.Support;
                }
                else if (fullpath.ToUpper().Contains("\\PAL\\"))
                {
                    lib.Level = LibraryLevel.PAL;
                }
                else
                {
                    lib.Level = LibraryLevel.HAL;
                }

                if (Path.GetFileName(fullpath).ToUpper().Contains("_STUB") ||
                    fullpath.ToUpper().Contains("\\STUBS"))
                {
                    lib.IsStub = true;
                }

                Dictionary<string, string> tbl = new Dictionary<string, string>();
                tbl["AssemblyName"] = "Name";
                tbl["ProjectGuid"] = "Guid";

                LoadStringProps(proj, lib, tbl);

                if (!fForceReload)
                {
                    dbLib = m_helper.FindLibrary(lib.Guid);

                    if (dbLib != null)
                    {
                        if (!string.Equals(dbLib.Name, lib.Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            System.Diagnostics.Debug.WriteLine("WARNING!!! Loaded library name doesn't match database library (possible bad project file)");
                            System.Diagnostics.Debug.WriteLine(dbLib.Name + " != " + lib.Name);
                        }
                        return dbLib;
                    }

                    dbLib = m_helper.FindLibraryByName(lib.Name);
                    if (dbLib != null) return dbLib;
                }

                if (string.IsNullOrEmpty(lib.LibraryFile)) lib.LibraryFile = lib.Name + ".$(LIB_EXT)";
                if (string.IsNullOrEmpty(lib.ManifestFile)) lib.ManifestFile = lib.LibraryFile + ".manifest";

                foreach (ProjectPropertyGroupElement pg in proj.Xml.PropertyGroups)
                {
                    foreach (ProjectPropertyElement bp in pg.Properties)
                    {
                        string cond = CombineConditionals(pg.Condition, bp.Condition);

                        switch (bp.Name)
                        {
                            /*
                        case "OutputType":
                            // don't allow projects to be loaded as library
                            if (0 == string.Compare(bp.Value.Trim(), "Executable", true))
                            {
                                return null;
                            }
                            break;
                            */
                            case "Version":
                                string[] vers = bp.Value.Split('.');
                                if (vers != null)
                                {
                                    if (vers.Length > 0) lib.Version.Major = vers[0];
                                    if (vers.Length > 1) lib.Version.Minor = vers[1];
                                    if (vers.Length > 2) lib.Version.Revision = vers[2];
                                    if (vers.Length > 3) lib.Version.Build = vers[3];
                                }
                                break;
                            case "PlatformIndependentBuild":
                                {
                                    bool fPlatIndep = false;
                                    bool.TryParse(bp.Value, out fPlatIndep);
                                    lib.PlatformIndependent = fPlatIndep;
                                }
                                break;
                            default:
                                {
                                    MFProperty prop = new MFProperty();
                                    prop.Name = bp.Name;
                                    prop.Value = bp.Value;
                                    prop.Condition = cond;
                                    lib.Properties.Add(prop);
                                }
                                break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(lib.Name))
                {
                    string dirName = Path.GetFileName(Path.GetDirectoryName(fullpath));
                    if (string.Compare(dirName, "GlobalLock") == 0)
                    {
                        lib.Name = dirName;
                    }
                    else
                    {
                        return null;
                    }
                }


                LoadCompileItems(proj, lib, path);

                if (string.IsNullOrEmpty(lib.Guid))
                {
                    lib.Guid = System.Guid.NewGuid().ToString("B");
                }

                foreach (ProjectItemGroupElement big in proj.Xml.ItemGroups)
                {
                    foreach (ProjectItemElement bi in big.Items)
                    {
                        string cond = CombineConditionals(big.Condition, bi.Condition);

                        switch (bi.ItemType)
                        {
                            case "RequiredProjects":
                                if (bi.Include.Trim().ToUpper().EndsWith(".PROJ"))
                                {
                                    Library lib2 = LoadLibraryProj(bi.Include, path);

                                    if (lib2 != null)
                                    {
                                        lib.Dependencies.Add(new MFComponent(MFComponentType.Library, lib2.Name, lib2.Guid, bi.Include));
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine("Warning! unknown library " + bi.Include);
                                        //System.Diagnostics.Debug.Assert(false);
                                    }
                                }
                                break;
                            case "LibraryCategories":
                                LibraryCategory libCat = LoadLibraryCategoryProj(bi.Include, path);
                                lib.Dependencies.Add(new MFComponent(MFComponentType.LibraryCategory, libCat.Name, libCat.Guid, bi.Include, cond));
                                break;
                            case "FastCompileCPPFile":
                            case "Compile":
                            case "HFiles":
                            case "IncludePaths":
                                // handled by LoadCompileItems
                                break;

                            case "SubDirectories":
                                break;

                            default:
                                {
                                    MFBuildFile f = new MFBuildFile();
                                    f.Condition = cond;
                                    f.File = bi.Include;
                                    f.ItemName = bi.ItemType;
                                    lib.OtherFiles.Add(f);
                                }
                                break;
                        }
                    }
                }

                if (lib.IsStub && lib.LibraryCategory != null && !string.IsNullOrEmpty(lib.LibraryCategory.Guid))
                {
                    LibraryCategory lc = m_helper.FindLibraryCategory(lib.LibraryCategory.Guid);
                    if (lc != null)
                    {
                        lc.StubLibrary = new MFComponent(MFComponentType.Library, lib.Name, lib.Guid, lib.ProjectPath);

                        if (lc.Level != LibraryLevel.CLR && lc.Level != LibraryLevel.Support && lc.Templates.Count == 0)
                        {
                            lc.Templates.Clear();

                            ApiTemplate api = new ApiTemplate();
                            api.FilePath = ConvertPathToEnv(fullpath);
                            lc.Templates.Add(api);

                            foreach (MFBuildFile bf in lib.SourceFiles)
                            {
                                api = new ApiTemplate();
                                api.FilePath = ConvertPathToEnv(Path.Combine(path, bf.File));
                                lc.Templates.Add(api);
                            }
                            foreach (MFBuildFile bf in lib.HeaderFiles)
                            {
                                if (Path.IsPathRooted(MsBuildWrapper.ExpandEnvVars(bf.File, "")) || bf.File.Contains("..")) continue;

                                api = new ApiTemplate();
                                api.FilePath = ConvertPathToEnv(Path.Combine(path, bf.File));
                                lc.Templates.Add(api);
                            }
                            foreach (MFBuildFile bf in lib.FastCompileFiles)
                            {
                                api = new ApiTemplate();
                                api.FilePath = ConvertPathToEnv(Path.Combine(path, bf.File));
                                lc.Templates.Add(api);
                            }
                        }

                    }
                }

                foreach (ProjectTargetElement targ in proj.Xml.Targets)
                {
                    lib.Targets.Add(targ);
                }

                foreach (ProjectImportElement imp in proj.Xml.Imports)
                {
                    switch (Path.GetExtension(imp.Project).ToUpper().Trim())
                    {
                        case ".LIBCATPROJ":
                            LibraryCategory lc = LoadLibraryCategoryProj(imp.Project, path);
                            if (lc != null)
                            {
                                MFComponent cmp = new MFComponent(MFComponentType.LibraryCategory, lc.Name, lc.Guid, lc.ProjectPath);
                                lib.Dependencies.Add(cmp);
                            }
                            else
                            {
                                System.Diagnostics.Debug.Assert(false);
                            }
                            break;
                    }
                }

                m_helper.AddLibraryToInventory(lib, false, m_helper.DefaultInventory);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: loading Library file: " + fullpath + "\r\n", e.Message);
                lib = null;
            }

            return lib;
        }

        internal void SaveAssemblyProj(MFAssembly asm)
        {
            if (!string.IsNullOrEmpty(asm.ProjectPath))
            {
                string fullpath = ExpandEnvVars(asm.ProjectPath, "");
                try
                {
                    Project proj = LoadProject(fullpath);

                    foreach (ProjectPropertyGroupElement bpg in proj.Xml.PropertyGroups)
                    {
                        foreach (ProjectPropertyElement bp in bpg.Properties)
                        {
                            switch (bp.Name)
                            {
                                case "Description":
                                    bp.Value = asm.Description;
                                    break;
                                case "Groups":
                                    bp.Value = asm.Groups;
                                    break;
                            }
                        }
                    }

                    proj.Save(proj.FullPath);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Error: saving asm file: " + fullpath + "\r\n", e.Message);
                }
            }
        }

        internal MFAssembly LoadAssemblyProj(string asmProjFile, string path)
        {

            Project proj;
            string projname = Path.GetFileNameWithoutExtension(asmProjFile);
            string fullpath = ExpandEnvVars(asmProjFile, path);

            MFAssembly asm = m_helper.FindAssemblyByProject(fullpath);

            try
            {
                if (asm != null) return asm;

                if (!File.Exists(fullpath))
                {
                    // TODO: ERROR LOGGING
                    return null;
                }

                asm = new MFAssembly();

                asm.ProjectPath = ConvertPathToEnv(fullpath);

                proj = LoadProject(fullpath);

                path = Path.GetDirectoryName(fullpath);

                if (path.ToLower().Contains("\\framework\\tools\\")) return null;

                foreach (ProjectImportElement imp in proj.Xml.Imports)
                {
                    // no server assemblies
                    if (imp.Project.ToUpper().Contains("MICROSOFT.SPOT.CSHARP.HOST.TARGETS"))
                    {
                        return null;
                    }
                }

                foreach (ProjectPropertyGroupElement pg in proj.Xml.PropertyGroups)
                {
                    foreach (ProjectPropertyElement bp in pg.Properties)
                    {
                        switch (bp.Name)
                        {
                            case "AssemblyName":
                                string asmName = bp.Value;
                                asmName = asmName.Replace("$(MSBuildProjectName)", projname);
                                asm.Name = asmName;
                                asm.AssemblyFile = @"$(BUILD_TREE_CLIENT)\pe\" + asmName + ".pe";
                                break;
                            case "TinyCLR_Platform":
                                if (string.Compare(bp.Value, "Server", true) == 0)
                                {
                                    // we don't want to process server tools for now
                                    return null;
                                }
                                break;
                            case "ProjectGuid":
                                asm.Guid = bp.Value;
                                break;
                            case "Groups":
                                asm.Groups = bp.Value;
                                break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(asm.Name)) asm.Name = projname;
                if (string.IsNullOrEmpty(asm.Guid)) asm.Guid = System.Guid.NewGuid().ToString("B");

                foreach (ProjectItemGroupElement big in proj.Xml.ItemGroups)
                {
                    foreach (ProjectItemElement bi in big.Items)
                    {
                        string cond = CombineConditionals(big.Condition, bi.Condition);

                        if (bi.ItemType == "Reference")
                        {
                            MFComponent asmRef = new MFComponent(MFComponentType.MFAssembly);
                            asmRef.Name = bi.Include;
                            asmRef.Conditional = cond;

                            asm.References.Add(asmRef);
                        }
                    }
                }

                MFAssembly dbAsm = m_helper.FindAssemblyByName(asm.Name);
                if (null == dbAsm)
                {
                    m_helper.DefaultInventory.Assemblies.Add(asm);
                }
                else
                {
                    asm = dbAsm;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: loading assembly file: " + fullpath + "\r\n", e.Message);
                asm = null;
            }

            return asm;
        }

        internal void SaveFeatureProj(Feature feature)
        {
            if (!string.IsNullOrEmpty(feature.ProjectPath))
            {
                try
                {
                    Project proj;

                    string path = ExpandEnvVars(feature.ProjectPath, "");

                    if (File.Exists(path))
                    {
                        proj = LoadProject(path);
                    }
                    else
                    {
                        proj = new Project();
                        proj.FullPath = path;
                    }

                    UpdatePropertyGroups(proj, feature,
                        new string[]{
                            "FeatureName",
                            "Filter",
                            "Description",
                            "Guid",
                            "Groups",
                            "Documentation",
                            "IsSolutionWizardVisible"
                        },
                            new string[]{
                            "Name",
                        });

                    // TODO: Save imports (libcatproj and featureproj files)

                    proj.Save(proj.FullPath);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Error: failure saving feature file " + feature.ProjectPath + "\r\n" + e.Message);
                }
            }
        }

        internal Feature LoadFeatureProj(string featureProjFile, string path)
        {
            Project proj;
            string fullpath = ExpandEnvVars(featureProjFile, path);
            Feature feat = null;

            try
            {
                feat = m_helper.FindFeatureByProject(fullpath);

                if (feat != null) return feat;

                feat = new Feature();

                if (!File.Exists(fullpath))
                {
                    // TODO: add logging support
                    return null;
                }

                proj = LoadProject(fullpath);

                path = Path.GetDirectoryName(fullpath);
                feat.ProjectPath = ConvertPathToEnv(featureProjFile);

                Dictionary<string, string> tbl = new Dictionary<string, string>();
                tbl["FeatureName"] = "Name";
                tbl["Filter"] = "Filter";

                LoadStringProps(proj, feat, tbl);

                if (string.IsNullOrEmpty(feat.Guid)) feat.Guid = System.Guid.NewGuid().ToString("B");


                foreach (ProjectItemGroupElement big in proj.Xml.ItemGroups)
                {
                    foreach (ProjectItemElement bi in big.Items)
                    {
                        string cond = CombineConditionals(big.Condition, bi.Condition);

                        switch (bi.ItemType)
                        {
                            case "RequiredManagedProjects":
                                {
                                    MFAssembly asm = LoadAssemblyProj(bi.Include, path);

                                    if (asm != null)
                                    {
                                        MFComponent asmRef = new MFComponent(MFComponentType.MFAssembly);

                                        asmRef.Name = asm.Name;
                                        asmRef.Guid = asm.Guid;
                                        asmRef.ProjectPath = asm.ProjectPath;
                                        asmRef.Conditional = cond;

                                        feat.Assemblies.Add(asmRef);
                                    }
                                }
                                break;
                            case "RequiredProjects":
                                string test = Path.GetExtension(bi.Include).ToUpper();

                                switch (test)
                                {
                                    case ".FEATUREPROJ":
                                        System.Diagnostics.Debug.Assert(false, "Use ProjectImportElement for featureproj files");
                                        break;
                                    case ".LIBCATPROJ":
                                        System.Diagnostics.Debug.Assert(false, "Use ProjectImportElement for libcatproj files");
                                        break;
                                    case ".PROJ":
                                        Library lib = LoadLibraryProj(bi.Include, path);
                                        if (lib != null)
                                        {
                                            feat.ComponentDependencies.Add(new MFComponent(MFComponentType.Library, lib.Name, lib.Guid, bi.Include, cond));
                                        }
                                        else
                                        {
                                            //TODO: ERROR LOGGING
                                        }
                                        break;
                                }
                                break;
                            // ignore
                            case "MMP_DAT_CreateDatabase":
                            case "InteropFeature":
                                break;
                            default:
                                Console.WriteLine("Warning: Unknown ProjectItemElement " + bi.ItemType);
                                break;

                        }
                    }

                }

                // featureproj and libcatproj files are imported
                foreach (ProjectImportElement imp in proj.Xml.Imports)
                {
                    switch (Path.GetExtension(imp.Project).ToUpper())
                    {
                        case ".FEATUREPROJ":
                            Feature f2 = LoadFeatureProj(imp.Project, path);
                            feat.FeatureDependencies.Add(new MFComponent(MFComponentType.Feature, f2.Name, f2.Guid, imp.Project, imp.Condition));
                            break;
                        case ".LIBCATPROJ":
                            LibraryCategory libcat = LoadLibraryCategoryProj(imp.Project, path);
                            feat.ComponentDependencies.Add(new MFComponent(MFComponentType.LibraryCategory, libcat.Name, libcat.Guid, libcat.ProjectPath));
                            break;
                    }
                }

                Feature dbFeat = null;

                if (!string.IsNullOrEmpty(feat.Guid)) dbFeat = m_helper.FindFeature(feat.Guid);

                if (dbFeat == null) dbFeat = m_helper.FindFeatureByName(feat.Name);

                if (null == dbFeat)
                {
                    m_helper.DefaultInventory.Features.Add(feat);
                }
                else
                {
                    feat = dbFeat;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: loading feature file: " + fullpath + "\r\n", e.Message);
                feat = null;
            }

            return feat;
        }

        internal void SaveBuildToolRef(List<BuildToolRef> refs, Project proj)
        {
            foreach (BuildToolRef btr in refs)
            {
                ProjectPropertyGroupElement bpg = proj.Xml.AddPropertyGroup();
                bpg.Condition = "'$(COMPILER_TOOL)'=='" + btr.Name + "'";

                if (!string.IsNullOrEmpty(btr.BuildOptions.DeviceType))
                {
                    ProjectPropertyElement bp = bpg.AddProperty("DEVICE_TYPE", btr.BuildOptions.DeviceType);
                    bp.Condition = "'$(DEVICE_TYPE)' == ''";
                }
                bpg.AddProperty("BUILD_TOOL_GUID", btr.Guid);

                AddBuildToolFlags(btr.BuildOptions, proj, bpg);
            }
        }

        private string RemoveSpoClient(string path)
        {
            Regex exp = new Regex(@"\$\(SPOCLIENT\)\\", RegexOptions.IgnoreCase);

            path = exp.Replace(path, "");

            exp = new Regex(Environment.GetEnvironmentVariable("SPOCLIENT").TrimEnd('\\').Replace("\\", "\\\\") + "\\\\", RegexOptions.IgnoreCase);

            path = exp.Replace(path, "");

            return path;
        }

        internal void SaveProcessorProj(Processor proc)
        {
            if (!string.IsNullOrEmpty(proc.ProjectPath))
            {
                try
                {
                    string fullpath = ExpandEnvVars(proc.ProjectPath, "");

                    Project proj = LoadProject(fullpath);
                    proj.ProjectCollection.UnloadProject(proj);

                    proj = new Project();
                    proj.Xml.DefaultTargets = "Build";

                    Dictionary<string, string> tbl = new Dictionary<string, string>();
                    tbl["PlatformFamily"] = "PLATFORM_FAMILY";

                    ProjectPropertyGroupElement bpg = SaveStringProps(proj, proc, tbl);

                    if (!string.IsNullOrEmpty(proc.DefaultISA))
                    {
                        ProjectPropertyElement bp = bpg.AddProperty("INSTRUCTION_SET", proc.DefaultISA);
                        bp.Condition = "'$(INSTRUCTION_SET)'==''";
                    }


                    foreach (MFProperty prop in proc.Properties)
                    {
                        ProjectPropertyElement bp = bpg.AddProperty(prop.Name, prop.Value);
                        bp.Condition = prop.Condition;
                    }

                    bpg.AddProperty("TARGETPROCESSOR", proc.Name);
                    bpg.AddProperty("TARGETCODEBASE", proc.Name);
                    bpg.AddProperty("TARGETCODEBASETYPE", "Native");

                    SaveBuildToolRef(proc.BuildToolOptions, proj);

                    ProjectItemGroupElement big = proj.Xml.AddItemGroup();
                    big.AddItem("IncludePaths", RemoveSpoClient(Path.GetDirectoryName(proc.ProjectPath)));

                    //ProjectPropertyGroupElement bpg = proj.Xml.AddPropertyGroup();

                    //bpg.AddProperty("PKUI_Processor", SerializeXml(proc));

                    proj.Save(fullpath);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Error: Unable to save procesor file " + proc.ProjectPath + "\r\n" + e.Message);
                }
            }
        }

        internal Project LoadProject(string fullpath)
        {
            ICollection<Project> col = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(fullpath);

            if (col.Count > 0)
            {
                IEnumerator<Project> en = col.GetEnumerator();

                en.MoveNext();

                return en.Current;
            }

            if (File.Exists(fullpath))
            {
                return new Project(fullpath);
            }
            else
            {
                Project prj = new Project();
                prj.FullPath = fullpath;

                return prj;
            }
        }


        internal Processor LoadProcessorProj(string procProjFile, string path)
        {
            Processor proc = new Processor();
            Project proj;
            string fullpath = ExpandEnvVars(procProjFile, path);

            try
            {
                proj = LoadProject(fullpath);

                path = Path.GetDirectoryName(fullpath);

                proc.BuildToolOptions = LoadBuildToolFlags(proj);

                Dictionary<string, string> tbl = new Dictionary<string, string>();
                tbl["PLATFORM_FAMILY"] = "PlatformFamily";
                tbl["INSTRUCTION_SET"] = "DefaultISA";

                LoadStringProps(proj, proc, tbl);

                Processor dbProc = m_helper.FindProcessorByName(proc.Name);
                if (null == dbProc)
                {
                    m_helper.DefaultInventory.Processors.Add(proc);
                }
                else
                {
                    proc = dbProc;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: loading processor file: " + fullpath + "\r\n", e.Message);
                proc = null;
            }

            return proc;
        }

        internal void SaveProjectProj(MFProject mfproj)
        {
            if (!string.IsNullOrEmpty(mfproj.ProjectPath))
            {
                try
                {
                    string fullpath = ExpandEnvVars(mfproj.ProjectPath, "");
                    string dir = Path.GetDirectoryName(fullpath);

                    // normalize the path (no xxx\..\yyy)
                    fullpath = Path.GetFullPath(fullpath);
                    mfproj.ProjectPath = ConvertPathToEnv(fullpath);

                    Project proj = LoadProject(fullpath);

                    // clear the project
                    proj.ProjectCollection.UnloadProject(proj);

                    proj = new Project();
                    proj.Xml.DefaultTargets = "Build";

                    proj.FullPath = fullpath;

                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    mfproj.Directory = RemoveSpoClient(Path.GetDirectoryName(fullpath));

                    Dictionary<string, string> tbl = new Dictionary<string, string>();
                    tbl["Name"] = "AssemblyName";
                    tbl["SettingsFile"] = "MFSettingsFile";
                    tbl["Guid"] = "ProjectGuid";

                    ProjectPropertyGroupElement bpg = SaveStringProps(proj, mfproj, tbl);

                    var reducesizeProp = mfproj.Properties.FirstOrDefault(p => p.Name == "reducesize");
                    if (reducesizeProp != null)
                    {
                        bpg.AddProperty("reducesize", reducesizeProp.Value);
                    }

                    ProjectImportElement pi = proj.Xml.AddImport(@"$(SPOCLIENT)\tools\targets\Microsoft.SPOT.System.Settings");

                    if (mfproj.IsClrProject)
                    {
                        proj.Xml.AddImport(@"$(SPOCLIENT)\tools\Targets\Microsoft.SPOT.Build.Targets");
                    }

                    proj.Save(fullpath);

                    bpg = proj.Xml.AddPropertyGroup();
                    foreach (MFProperty prop in mfproj.Properties)
                    {
                        if (!tbl.ContainsKey(prop.Name) || tbl[prop.Name] != prop.Value)
                        {
                            ProjectPropertyElement bp = bpg.AddProperty(prop.Name, prop.Value);
                            bp.Condition = prop.Condition;
                            tbl[prop.Name] = prop.Value;
                        }
                    }

                    if (mfproj.BuildTool != null)
                    {
                        List<BuildToolRef> lst = new List<BuildToolRef>();
                        lst.Add(mfproj.BuildTool);
                        SaveBuildToolRef(lst, proj);
                    }

                    if (mfproj.ScatterFile != null)
                    {
                        bpg.AddProperty("EXEScatterFileDefinition", mfproj.ScatterFile.File);
                    }

                    ProjectItemGroupElement bigCompile = proj.Xml.AddItemGroup();

                    // copy from cloned solution if necessary
                    SaveCompileItems(proj, mfproj.FastCompileFiles, mfproj.HeaderFiles, mfproj.SourceFiles, mfproj.OtherFiles, mfproj.IncludePaths);

                    ProjectItemGroupElement big = null;
                    foreach (MFComponent feat in mfproj.Features)
                    {
                        ProjectImportElement pie = proj.Xml.AddImport(feat.ProjectPath);
                        pie.Condition = feat.Conditional;
                    }

                    proj.Save(fullpath);

                    big = null;
                    foreach (string interop in mfproj.InteropFeatures)
                    {
                        if (big == null)
                        {
                            big = proj.Xml.AddItemGroup();
                        }

                        big.AddItem("InteropFeature", interop);
                    }
                    if (mfproj.IsClrProject)
                    {
                        proj.Xml.AddImport(@"$(SPOCLIENT)\tools\targets\Microsoft.SPOT.System.Interop.Settings");
                    }

                    proj.Save(fullpath);

                    SaveDependentLibs(proj, mfproj.Libraries);

                    big = null;
                    foreach (string asm in mfproj.ExtraAssemblies)
                    {
                        if (big == null)
                        {
                            big = proj.Xml.AddItemGroup();
                        }

                        big.AddItem("MMP_DAT_CreateDatabase", asm);
                    }

                    foreach (ProjectTargetElement targ in mfproj.Targets)
                    {
                        ProjectTargetElement t = proj.Xml.AddTarget(targ.Name);

                        t.Condition = targ.Condition;
                        t.DependsOnTargets = targ.DependsOnTargets;
                        t.Inputs = targ.Inputs;
                        t.Outputs = targ.Outputs;

                        foreach (ProjectTaskElement task in targ.Tasks)
                        {
                            ProjectTaskElement tsk = t.AddTask(task.Name);
                            tsk.Condition = task.Condition;
                            tsk.ContinueOnError = task.ContinueOnError;

                            foreach (KeyValuePair<string, string> param in task.Parameters)
                            {
                                tsk.SetParameter(param.Key, param.Value);
                            }
                        }
                    }

                    proj.Xml.AddImport(@"$(SPOCLIENT)\tools\targets\Microsoft.SPOT.System.Targets");

                    proj.Save(fullpath);

                    // post process project file to assure proper placement of imports
                    XmlDocument doc = new XmlDocument();
                    doc.Load(fullpath);

                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                    nsmgr.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");

                    XmlNode root = doc.DocumentElement;

                    string expr = "descendant::ns:PropertyGroup";
                    XmlNode dst = root.SelectSingleNode(expr, nsmgr);
                    expr = @"descendant::ns:Import";
                    XmlNodeList imports = root.SelectNodes(expr, nsmgr);

                    XmlNode sysSettings = null;
                    XmlNode bldTargs = null;
                    XmlNode intSettings = null;
                    List<XmlNode> featureProjs = new List<XmlNode>();

                    foreach (XmlNode n in imports)
                    {
                        string prj = n.Attributes["Project"].InnerText;

                        if (prj.Equals(@"$(SPOCLIENT)\tools\targets\Microsoft.SPOT.System.Settings", StringComparison.OrdinalIgnoreCase))
                        {
                            sysSettings = n;
                        }
                        else if (prj.Equals(@"$(SPOCLIENT)\tools\Targets\Microsoft.SPOT.Build.Targets", StringComparison.OrdinalIgnoreCase))
                        {
                            bldTargs = n;
                        }
                        else if (prj.Equals(@"$(SPOCLIENT)\tools\targets\Microsoft.SPOT.System.Interop.Settings", StringComparison.OrdinalIgnoreCase))
                        {
                            intSettings = n;
                        }
                        else if (prj.EndsWith(".featureproj", StringComparison.OrdinalIgnoreCase))
                        {
                            featureProjs.Add(n);
                        }
                    }

                    //[@ns:Project='$(SPOCLIENT)\tools\targets\Microsoft.SPOT.System.Settings']
                    root.RemoveChild(sysSettings);
                    dst = root.InsertAfter(sysSettings, dst);

                    if (mfproj.IsClrProject)
                    {
                        //expr = @"descendant::ns:Import[@ns:Project='$(SPOCLIENT)\tools\Targets\Microsoft.SPOT.Build.Targets']";
                        //src = root.SelectSingleNode(expr, nsmgr);
                        root.RemoveChild(bldTargs);
                        root.InsertAfter(bldTargs, dst);

                        expr = "descendant::ns:ItemGroup/ns:Compile";
                        dst = root.SelectSingleNode(expr, nsmgr).ParentNode;
                        //expr = "descendant::ns:Import[@ns:Project='*.featureproj']";
                        //XmlNodeList list = root.SelectNodes(expr, nsmgr);
                        foreach (XmlNode n in featureProjs)
                        {
                            root.RemoveChild(n);
                            dst = root.InsertAfter(n, dst);
                        }
                        //expr = @"descendant::ns:Import[@ns:Project='$(SPOCLIENT)\tools\targets\Microsoft.SPOT.System.Interop.Settings']";
                        //src = root.SelectSingleNode(expr, nsmgr);
                        root.RemoveChild(intSettings);
                        root.InsertAfter(intSettings, dst);
                    }

                    doc.Save(fullpath);
                }
                catch
                {
                    Console.WriteLine("Error: Invalid project file " + mfproj.ProjectPath);
                }
            }
        }

        internal MFProject LoadProjectProj(string projFile, string path)
        {
            MFProject mfproj = new MFProject();
            Project proj;
            string fullpath = ExpandEnvVars(projFile, path);

            List<MFComponent> driverLibCheck = new List<MFComponent>();

            try
            {
                proj = LoadProject(fullpath);

                path = Path.GetDirectoryName(fullpath);

                mfproj.ProjectPath = ConvertPathToEnv(projFile);

                Dictionary<string, string> tbl = new Dictionary<string, string>();
                tbl["AssemblyName"] = "Name";
                tbl["MFSettingsFile"] = "SettingsFile";
                tbl["ProjectGuid"] = "Guid";

                LoadStringProps(proj, mfproj, tbl);

                foreach (ProjectPropertyGroupElement pg in proj.Xml.PropertyGroups)
                {
                    foreach (ProjectPropertyElement bp in pg.Properties)
                    {
                        string cond = CombineConditionals(pg.Condition, bp.Condition);

                        switch (bp.Name)
                        {
                            case "AssemblyName":
                            case "MFSettingsFile":
                            case "ProjectGuid":
                            case "Description":
                            case "Documentation":
                                // handled by loadstringprops
                                break;
                            case "IsClrProject":
                                mfproj.IsClrProject = Boolean.Parse(bp.Value);
                                break;
                            case "EXEScatterFileDefinition":
                                {
                                    MFBuildFile file = new MFBuildFile();

                                    file.Condition = cond;
                                    file.File = bp.Value;
                                    file.ItemName = bp.Name;

                                    mfproj.ScatterFile = file;
                                }
                                break;
                            default:
                                {
                                    MFProperty prop = new MFProperty();
                                    prop.Name = bp.Name;
                                    prop.Value = bp.Value;
                                    prop.Condition = cond;
                                    mfproj.Properties.Add(prop);
                                }
                                break;
                        }
                    }
                }

                LoadCompileItems(proj, mfproj, path);

                Dictionary<string, MFComponent> libLookup = new Dictionary<string, MFComponent>();
                foreach (ProjectItemGroupElement big in proj.Xml.ItemGroups)
                {
                    foreach (ProjectItemElement bi in big.Items)
                    {
                        string cond = CombineConditionals(big.Condition, bi.Condition);

                        switch (bi.ItemType)
                        {
                            case "RequiredProjects":
                                string ext = Path.GetExtension(bi.Include).ToUpper();
                                if (ext == ".FEATUREPROJ" || ext == ".LIBCATPROJ")
                                {
                                    System.Diagnostics.Debug.Assert(false, ".FeatureProj and .LibCatProj files can only be imported");
                                }
                                else if (ext == ".PROJ")
                                {
                                    Library lib = m_helper.FindLibraryByProject(ExpandEnvVars(bi.Include, path));
                                    MFComponent comp = null;

                                    if (lib == null)
                                    {
                                        lib = LoadLibraryProj(bi.Include, path);
                                    }

                                    if (lib != null)
                                    {
                                        if (libLookup.ContainsKey(lib.Guid.ToLower()))
                                        {
                                            comp = libLookup[lib.Guid.ToLower()];

                                            if (string.IsNullOrEmpty(comp.Conditional))
                                            {
                                                comp.Conditional = cond;
                                            }
                                            break;
                                        }

                                        comp = new MFComponent(MFComponentType.Library, lib.Name, lib.Guid, bi.Include);
                                        comp.Conditional = cond;
                                    }

                                    if (comp != null)
                                    {
                                        mfproj.Libraries.Add(comp);

                                        libLookup[comp.Guid.ToLower()] = comp;
                                    }
                                    else
                                    {
                                        // we should pick this up in the driverlibs/platformlibs
                                        /*
                                        string name = Path.GetFileName(Path.GetDirectoryName(bi.Include));
                                        comp = new MFComponent(MFComponentType.Library, name, System.Guid.NewGuid().ToString("B"), bi.Include);
                                        comp.Conditional = cond;

                                        mfproj.Libraries.Add(comp);

                                        libLookup[comp.Guid.ToLower()] = comp;

                                        Console.WriteLine("Error: Library not found " + bi.Include);
                                        */
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Warning! Skipping dependency item " + bi.Include);
                                }
                                break;
                            case "InteropFeature":
                                mfproj.InteropFeatures.Add(bi.Include);
                                break;
                            case "PlatformIndependentLibs":
                                goto case "DriverLibs";
                            case "DriverLibs":
                                driverLibCheck.Add(new MFComponent(MFComponentType.Unknown, "", "", bi.Include, cond));
                                break;
                            case "MMP_DAT_CreateDatabase":
                                if (!mfproj.ExtraAssemblies.Contains(bi.Include))
                                {
                                    mfproj.ExtraAssemblies.Add(bi.Include);
                                }
                                break;
                            case "FastCompileCPPFile":
                            case "Compile":
                            case "HFiles":
                            case "IncludePaths":
                                // handled by LoadCompileItems
                                break;

                            // todo: do we want to get rid of subdirectories?
                            //case "SubDirectories":
                            //break;

                            default:
                                {
                                    MFBuildFile f = new MFBuildFile();
                                    f.Condition = cond;
                                    f.File = bi.Include;
                                    f.ItemName = bi.ItemType;
                                    mfproj.OtherFiles.Add(f);
                                }
                                break;
                        }
                    }
                }

                Hashtable featLookup = new Hashtable();

                foreach (ProjectImportElement imp in proj.Xml.Imports)
                {
                    string ext = Path.GetExtension(imp.Project).ToUpper();

                    if (ext == ".FEATUREPROJ")
                    {
                        Feature feat = LoadFeatureProj(imp.Project, path);
                        MFComponent comp = null;

                        if (feat == null)
                        {
                            feat = m_helper.FindFeatureByName(Path.GetFileNameWithoutExtension(imp.Project));
                        }

                        if (feat == null)
                        {
                            string name = Path.GetFileNameWithoutExtension(imp.Project);
                            comp = new MFComponent(MFComponentType.Feature, name, System.Guid.NewGuid().ToString("B"), imp.Project);
                            comp.Conditional = imp.Condition;

                            mfproj.Features.Add(comp);

                            featLookup.Add(comp.Guid.ToLower(), comp);

                            //Console.WriteLine("Error: Feature not found " + imp.Project);
                        }
                        else if (!featLookup.ContainsKey(feat.Guid.ToLower()))
                        {
                            comp = new MFComponent(MFComponentType.Feature, feat.Name, feat.Guid, imp.Project);
                            comp.Conditional = imp.Condition;

                            mfproj.Features.Add(comp);

                            featLookup.Add(feat.Guid.ToLower(), comp);
                        }
                    }
                    else if (ext == ".LIBCATPROJ")
                    {
                        LibraryCategory libcat = LoadLibraryCategoryProj(imp.Project, path);
                        MFComponent comp = null;

                        if (libcat == null)
                        {
                            libcat = m_helper.FindLibraryCategoryByName(Path.GetFileNameWithoutExtension(imp.Project));
                        }

                        if (libcat != null)
                        {
                            comp = new MFComponent(MFComponentType.LibraryCategory, libcat.Name, libcat.Guid, imp.Project);
                            comp.Conditional = imp.Condition;

                            mfproj.LibraryCategories.Add(comp);
                        }
                        else
                        {
                            string name = Path.GetFileNameWithoutExtension(imp.Project);
                            comp = new MFComponent(MFComponentType.LibraryCategory, name, System.Guid.NewGuid().ToString("B"), imp.Project);
                            comp.Conditional = imp.Condition;

                            mfproj.LibraryCategories.Add(comp);

                            Console.WriteLine("Error: LibraryCategory not found " + imp.Project);
                        }
                    }
                }


                ScatterfileWrapper sw = new ScatterfileWrapper(mfproj);
                string scatter = "";

                if (mfproj.ScatterFile != null && !string.IsNullOrEmpty(mfproj.ScatterFile.File))
                {
                    string tmp = ExpandEnvVars(mfproj.ScatterFile.File, "");
                    if (File.Exists(tmp))
                    {
                        scatter = tmp;
                    }
                }

                if (scatter == "")
                {
                    foreach (string scatterfile in Directory.GetFiles(Path.GetDirectoryName(projFile), "scatter*.xml"))
                    {
                        if (scatterfile.ToLower().Contains("ram_functions")) continue;
                        if (scatterfile.ToLower().Contains("_gcc.")) continue;

                        List<MemoryMap> maps = sw.LoadFromFile(scatterfile);

                        if (maps != null && maps.Count > 0)
                        {
                            mfproj.MemoryMap = maps[0];
                        }
                    }
                }
                else
                {
                    // todo add support for GCC?
                    if (!scatter.ToLower().Contains("_gcc"))
                    {
                        mfproj.MemoryMap = sw.LoadFromFile(scatter)[0];
                    }
                }

                foreach (ProjectTargetElement targ in proj.Xml.Targets)
                {
                    mfproj.Targets.Add(targ);
                }

                foreach (MFComponent comp in driverLibCheck)
                {
                    Library lib = m_helper.FindLibraryByFile(comp.ProjectPath);

                    if (lib == null)
                    {
                        lib = m_helper.FindLibraryByName(Path.GetFileNameWithoutExtension(comp.ProjectPath));
                    }

                    if (lib == null)
                    {
                        mfproj.Libraries.Add(comp);

                        libLookup[comp.Guid.ToLower()] = comp;

                        Console.WriteLine("Warning: Library not found " + comp.ProjectPath + ". Delay loading...");
                    }
                }

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: loading project file: " + fullpath + "\r\n", e.Message);
                mfproj = null;
            }

            return mfproj;
        }

        private void CopyClonedFile(string oldFile, string newFile, string oldName, string newName)
        {
            if (oldName == newName)
            {
                File.Copy(oldFile, newFile);
                return;
            }

            string text = "";

            using (TextReader tr = File.OpenText(oldFile))
            {
                text = tr.ReadToEnd();
            }

            text = CopyHelper.ReplaceText(text, oldName, newName);

            if (Path.GetExtension(oldFile).ToLower() == ".proj")
            {
                Regex exp = new Regex("<ProjectGuid>([\\w\\W]+)</ProjectGuid>", RegexOptions.IgnoreCase);

                text = exp.Replace(text, "<ProjectGuid>" + Guid.NewGuid().ToString("B") + "</ProjectGuid>");
            }

            using (TextWriter tw = File.CreateText(newFile))
            {
                tw.Write(text);
            }
        }
        internal void CopyClonedFiles(string oldPath, string newPath, string oldName, string newName)
        {
            CopyClonedFiles(oldPath, newPath, oldName, newName, true);
        }

        internal void CopyClonedFiles(string oldPath, string newPath, string oldName, string newName, bool fIncludeSubDirs)
        {
            if (string.IsNullOrEmpty(newPath) || string.IsNullOrEmpty(oldPath) || !Directory.Exists(oldPath))
                return;

            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }

            if (!oldPath.EndsWith("\\")) oldPath += "\\";
            if (!newPath.EndsWith("\\")) newPath += "\\";

            foreach (string fileOld in Directory.GetFiles(oldPath))
            {
                string fileNew = newPath + CopyHelper.ReplaceText(Path.GetFileName(fileOld), oldName, newName);

                if (!File.Exists(fileNew))
                {
                    CopyClonedFile(fileOld, fileNew, oldName, newName);
                }
            }

            if (fIncludeSubDirs)
            {
                foreach (string subdir in Directory.GetDirectories(oldPath))
                {
                    string newSubdir = newPath + CopyHelper.ReplaceText(Path.GetFileName(subdir), oldName, newName);

                    CopyClonedFiles(subdir + "\\", newSubdir + "\\", oldName, newName);
                }
            }
        }

        internal void SaveSolutionProj(MFSolution solution)
        {
            if (!string.IsNullOrEmpty(solution.ProjectPath))
            {
                try
                {
                    //if(File.Exists(solution.ProjectPath)
                    string fullpath = ExpandEnvVars(solution.ProjectPath, "");

                    Project proj = LoadProject(fullpath);
                    proj.ProjectCollection.UnloadProject(proj);

                    proj = new Project();
                    proj.Xml.DefaultTargets = "Build";

                    string dir = Path.GetDirectoryName(fullpath);

                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    ProjectPropertyGroupElement mainGrp = proj.Xml.AddPropertyGroup();

                    mainGrp.AddProperty("Author", solution.Author);
                    mainGrp.AddProperty("Description", solution.Description);
                    mainGrp.AddProperty("Documentation", solution.Documentation);
                    mainGrp.AddProperty("PlatformGuid", solution.Guid);

                    if (solution.DefaultISA != null && solution.DefaultISA.Name != null)
                    {
                        ProjectPropertyElement bp = mainGrp.AddProperty("INSTRUCTION_SET", solution.DefaultISA.Name);
                        bp.Condition = solution.DefaultISA.Conditional;
                    }
                    mainGrp.AddProperty("TARGETPLATFORM", solution.Name);
                    mainGrp.AddProperty("PLATFORM", solution.Name);
                    mainGrp.AddProperty("IsSolutionWizardVisible", solution.IsSolutionWizardVisible.ToString());
                    mainGrp.AddProperty("ENDIANNESS", solution.ENDIANNESS.ToString());

                    Dictionary<string, object> nameLookup = new Dictionary<string, object>();

                    foreach (MFProperty prop in solution.Properties)
                    {
                        switch (prop.Name.ToLower())
                        {
                            case "author":
                            case "description":
                            case "documentation":
                            case "platformguid":
                            case "targetplatform":
                            case "platform":
                            case "issolutionwizardvisible":
                            case "endianness":
                            case "tcp_ip_stack":
                            case "instruction_set":
                                // these properties are reset in this method - this prevents duplication
                                break;

                            default:
                                string key = prop.Name.ToLower();
                                if (!nameLookup.ContainsKey(key))
                                {
                                    ProjectPropertyElement bp = mainGrp.AddProperty(prop.Name, prop.Value);
                                    bp.Condition = prop.Condition;
                                    nameLookup[key] = bp;
                                }
                                break;
                        }
                    }


                    MFProject defProj = null;

                    foreach (MFProject prj in solution.Projects)
                    {
                        if (prj.IsClrProject)
                        {
                            defProj = prj;
                            break;
                        }
                    }

                    if (defProj != null)
                    {
                        ///
                        /// If we have the LWIP or LWIP OS feature project then we need to add the following property to the 
                        /// solutions settings file
                        /// 
                        bool AddLWIP = false;
                        bool AddLWIPOS = false;
                        foreach (MFComponent f in defProj.Features)
                        {
                            if (f.Name.Equals("Network (Emulator)", StringComparison.InvariantCultureIgnoreCase))
                            {
                                AddLWIP = false;
                                AddLWIPOS = false;
                                break;
                            }
                            else if (f.Name.Equals("Network (LWIP)", StringComparison.InvariantCultureIgnoreCase))
                            {
                                AddLWIP = true;
                                AddLWIPOS = false;
                                break;
                            }
                            else if (f.Name.Equals("Network (LWIP OS)", StringComparison.InvariantCultureIgnoreCase))
                            {
                                AddLWIP = false;
                                AddLWIPOS = true;
                                break;
                            }
                        }
                        if (AddLWIP)
                        {
                            mainGrp.AddProperty("TCP_IP_STACK", "LWIP");
                        }
                        else if (AddLWIPOS)
                        {
                            mainGrp.AddProperty("TCP_IP_STACK", "LWIP_1_4_1_OS");
                        }
                    }

                    ProjectItemGroupElement big = proj.Xml.AddItemGroup();

                    string solPath = RemoveSpoClient(Path.GetDirectoryName(solution.ProjectPath));
                    bool fFoundSolPath = false;

                    foreach (MFBuildFile item in solution.Items)
                    {
                        if (string.Equals(item.ItemName, "IncludePaths", StringComparison.InvariantCultureIgnoreCase) &&
                            string.Equals(item.File, solPath, StringComparison.InvariantCultureIgnoreCase))
                        {
                            fFoundSolPath = true;
                        }

                        ProjectItemElement bi = big.AddItem(item.ItemName, item.File);
                        bi.Condition = item.Condition;
                    }

                    if (!fFoundSolPath)
                    {
                        big.AddItem("IncludePaths", solPath);
                    }

                    Processor prc = null;

                    if (!string.IsNullOrEmpty(solution.Processor.Guid))
                    {
                        prc = m_helper.FindProcessor(solution.Processor.Guid);
                    }
                    else
                    {
                        prc = m_helper.FindProcessorByName(solution.Processor.Name);
                        if (prc != null)
                        {
                            solution.Processor.Guid = prc.Guid;
                            solution.Processor.Name = prc.Name;
                            solution.Processor.ProjectPath = prc.ProjectPath;
                        }
                    }

                    if (prc != null)
                    {
                        proj.Xml.AddImport(prc.ProjectPath);
                    }

                    // TODO: this is a hack so that if an OS library is selected, the corresponding target of that OS is imported.
                    //       for any os library, a clause to the below ifelse should be added to capture the additional target import.
                    if (defProj != null)
                    {
                        foreach (MFComponent l in defProj.Libraries)
                        {
                            if (l.Name.Equals("CMSIS_RTX", StringComparison.InvariantCultureIgnoreCase))
                            {
                                var im = proj.Xml.AddImport(@"$(SPOCLIENT)\devicecode\Targets\OS\CMSIS_RTOS\CMSIS_RTOS.settings");
                                im.Condition = @"'$(reducesize)' == 'false'";
                            }
                        }
                    }

                    proj.Save(fullpath);


                    if (solution.m_cloneSolution != null)
                    {
                        string cloneRoot = Path.GetDirectoryName(ExpandEnvVars(solution.m_cloneSolution.ProjectPath, "")) + "\\";
                        string newRoot = Path.GetDirectoryName(ExpandEnvVars(solution.ProjectPath, "")) + "\\";
                        CopyClonedFile(cloneRoot + "platform_selector.h", newRoot + "platform_selector.h", solution.m_cloneSolution.Name, solution.Name);

                        ///
                        /// Copy the LWIP_selector file for the cloned solution
                        /// 
                        if (File.Exists(cloneRoot + "lwip_selector.h"))
                        {
                            CopyClonedFile(cloneRoot + "lwip_selector.h", newRoot + "lwip_selector.h", solution.m_cloneSolution.Name, solution.Name);
                        }

                        ///
                        /// Copy the lwipopts file for the cloned solution
                        /// 
                        if (File.Exists(cloneRoot + "lwipopts.h"))
                        {
                            CopyClonedFile(cloneRoot + "lwipopts.h", newRoot + "lwipopts.h", solution.m_cloneSolution.Name, solution.Name);
                        }

                        //CopyClonedFile(cloneRoot + "dotnetmf.proj", newRoot + "dotnetmf.proj", solution.m_cloneSolution.Name, solution.Name);
                        //CopyClonedFiles(cloneRoot + "DeviceCode\\", newRoot + "DeviceCode\\", solution.m_cloneSolution.Name, solution.Name);
                    }

                    ///
                    /// copy the generic lwip_selector file in the case the cloned solution doesn't have one or we are creating a
                    /// new solution with LWIP
                    /// 
                    if (!File.Exists(Path.Combine(dir, "lwip_selector.h")))
                    {
                        string lwipSel = ExpandEnvVars("$(SPOCLIENT)\\DeviceCode\\PAL\\LWIP\\Config\\lwip_selector.h", "");
                        if (File.Exists(lwipSel) && defProj != null)
                        {
                            foreach (MFComponent f in defProj.Features)
                            {
                                if (f.Name.Equals("Network (LWIP)", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    CopyClonedFile(lwipSel, Path.Combine(dir, "lwip_selector.h"), "<TEMPLATE>", solution.Name);
                                    break;
                                }
                            }
                        }
                    }

                    ///
                    /// copy the generic lwip_selector file in the case the cloned solution doesn't have one or we are creating a
                    /// new solution with LWIP_OS
                    /// 
                    if (!File.Exists(Path.Combine(dir, "lwipopts.h")))
                    {
                        string lwipSel = ExpandEnvVars("$(SPOCLIENT)\\DeviceCode\\PAL\\lwip_1_4_1_os\\config\\lwipopts.h", "");
                        if (File.Exists(lwipSel) && defProj != null)
                        {
                            foreach (MFComponent f in defProj.Features)
                            {
                                if (f.Name.Equals("Network (LWIP OS)", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    CopyClonedFile(lwipSel, Path.Combine(dir, "lwipopts.h"), "<TEMPLATE>", solution.Name);
                                    break;
                                }
                            }
                        }
                    }

                    string platSelectorFile = Path.Combine(dir, "platform_selector.h");
                    bool fNewSelector = false;

                    if (!File.Exists(platSelectorFile))
                    {
                        Processor proc = m_helper.FindProcessor(solution.Processor.Guid);

                        if (proc != null)
                        {
                            string template = Path.GetDirectoryName(ExpandEnvVars(proc.ProjectPath, "")) + "\\" + proc.Name + "_template_selector.h";
                            string newRoot = Path.GetDirectoryName(ExpandEnvVars(solution.ProjectPath, "")) + "\\";

                            if (File.Exists(template))
                            {
                                CopyClonedFile(template, newRoot + "platform_selector.h", "<TEMPLATE>", solution.Name);
                                fNewSelector = true;
                            }
                        }
                    }

                    // only modify if we are cloning or creating a new solution
                    if (File.Exists(platSelectorFile))
                    {
                        if (fNewSelector || (solution.m_cloneSolution != null))
                        {
                            FileAttributes attribs = File.GetAttributes(platSelectorFile);

                            if (0 != (attribs & FileAttributes.ReadOnly))
                            {
                                throw new Exception("File Access Exception: File " + platSelectorFile + " is not accessible");
                            }

                            string tmpFile = Path.GetTempFileName();
                            using (TextWriter tw = File.CreateText(tmpFile))
                            {
                                using (TextReader tr = File.OpenText(platSelectorFile))
                                {
                                    string data = tr.ReadToEnd();

                                    Regex rx = new Regex("#define\\s+SYSTEM_CLOCK_HZ\\s+[^\\r^\\n]+", RegexOptions.Singleline);
                                    data = rx.Replace(data, "#define SYSTEM_CLOCK_HZ                 " + solution.SystemClockSpeed.ToString());

                                    rx = new Regex("#define\\s+SLOW_CLOCKS_PER_SECOND\\s+[^\\r^\\n]+", RegexOptions.Singleline);
                                    data = rx.Replace(data, "#define SLOW_CLOCKS_PER_SECOND          " + solution.SlowClockSpeed.ToString());

                                    rx = new Regex("#define\\s+SRAM1_MEMORY_Base\\s+[^\\r^\\n]+", RegexOptions.Singleline);
                                    data = rx.Replace(data, "#define SRAM1_MEMORY_Base   0x" + solution.RamBase.ToString("X08"));

                                    rx = new Regex("#define\\s+SRAM1_MEMORY_Size\\s+[^\\r^\\n]+", RegexOptions.Singleline);
                                    data = rx.Replace(data, "#define SRAM1_MEMORY_Size   0x" + solution.RamLength.ToString("X08"));

                                    rx = new Regex("#define\\s+FLASH_MEMORY_Base\\s+[^\\r^\\n]+", RegexOptions.Singleline);
                                    data = rx.Replace(data, "#define FLASH_MEMORY_Base   0x" + solution.FlashBase.ToString("X08"));

                                    rx = new Regex("#define\\s+FLASH_MEMORY_Size\\s+[^\\r^\\n]+", RegexOptions.Singleline);
                                    data = rx.Replace(data, "#define FLASH_MEMORY_Size   0x" + solution.FlashLength.ToString("X08"));

                                    rx = new Regex("#define\\s+DEBUG_TEXT_PORT\\s+[^\\s]+", RegexOptions.Singleline);
                                    data = rx.Replace(data, "#define DEBUG_TEXT_PORT    " + solution.DebuggerPort);

                                    rx = new Regex("#define\\s+STDIO\\s+[\\w\\d_]+");
                                    data = rx.Replace(data, "#define STDIO              " + solution.DebuggerPort);

                                    rx = new Regex("#define\\s+DEBUGGER_PORT\\s+[^\\s]+", RegexOptions.Singleline);
                                    data = rx.Replace(data, "#define DEBUGGER_PORT      " + solution.DebuggerPort);

                                    rx = new Regex("#define\\s+MESSAGING_PORT\\s+[^\\s]+", RegexOptions.Singleline);
                                    data = rx.Replace(data, "#define MESSAGING_PORT     " + solution.DebuggerPort);

                                    rx = new Regex("#define\\s+RUNTIME_MEMORY_PROFILE__[\\w]+", RegexOptions.Singleline);
                                    data = rx.Replace(data, "#define RUNTIME_MEMORY_PROFILE__" + solution.MemoryProfile);

                                    tw.Write(data);
                                }
                            }

                            File.Delete(platSelectorFile);
                            File.Move(tmpFile, platSelectorFile);
                        }
                    }
                    else
                    {
                        using (TextWriter tw = File.CreateText(platSelectorFile))
                        {
                            tw.WriteLine(string.Format("#ifndef _PLATFORM_{0}_SELECTOR_H_\r\n#define _PLATFORM_{0}_SELECTOR_H_ 1", solution.Name));

                            tw.WriteLine(string.Format("#define HAL_SYSTEM_NAME                 \"{0}\"", solution.Name));
                            tw.WriteLine(string.Format("#define PLATFORM_{0}    ", prc.Name.ToUpper()));
                            tw.WriteLine(string.Format("#define PLATFORM_{0}_{1}", prc.PlatformFamily.ToUpper(), prc.Name.ToUpper()));
                            tw.WriteLine(string.Format("#define PLATFORM_{0}_{1}", prc.PlatformFamily.ToUpper(), solution.Name.ToUpper()));
                            if (0 == string.Compare(prc.Name, "MC9328", true))
                            {
                                tw.WriteLine("#define PLATFORM_ARM_MC9328MXL");
                            }

                            tw.WriteLine("#define PLATFORM_SUPPORTS_SOFT_REBOOT   TRUE");

                            tw.WriteLine(string.Format("#define SYSTEM_CLOCK_HZ                 {0}", solution.SystemClockSpeed));
                            tw.WriteLine(string.Format("#define SLOW_CLOCKS_PER_SECOND          {0}", solution.SlowClockSpeed));
                            tw.WriteLine("#define CLOCK_COMMON_FACTOR             1");
                            tw.WriteLine("#define SLOW_CLOCKS_TEN_MHZ_GCD         1");
                            tw.WriteLine("#define SLOW_CLOCKS_MILLISECOND_GCD     1");

                            tw.WriteLine(string.Format("#define SRAM1_MEMORY_Base               0x{0:X08}", solution.RamBase));
                            tw.WriteLine(string.Format("#define SRAM1_MEMORY_Size               0x{0:X08}", solution.RamLength));
                            tw.WriteLine(string.Format("#define FLASH_MEMORY_Base               0x{0:X08}", solution.FlashBase));
                            tw.WriteLine(string.Format("#define FLASH_MEMORY_Size               0x{0:X08}", solution.FlashLength));

                            tw.WriteLine(@"
#define TXPROTECTRESISTOR               RESISTOR_DISABLED
#define RXPROTECTRESISTOR               RESISTOR_DISABLED
#define CTSPROTECTRESISTOR              RESISTOR_DISABLED
#define RTSPROTECTRESISTOR              RESISTOR_DISABLED
");

                            tw.WriteLine(@"
#define GLOBAL_LOCK(x)             SmartPtr_IRQ x
#define DISABLE_INTERRUPTS()       SmartPtr_IRQ::ForceDisabled()
#define ENABLE_INTERRUPTS()        SmartPtr_IRQ::ForceEnabled()
#define INTERRUPTS_ENABLED_STATE() SmartPtr_IRQ::GetState()
#define GLOBAL_LOCK_SOCKETS(x)     SmartPtr_IRQ x

#if defined(_DEBUG)
#define ASSERT_IRQ_MUST_BE_OFF()   ASSERT(!SmartPtr_IRQ::GetState())
#define ASSERT_IRQ_MUST_BE_ON()    ASSERT( SmartPtr_IRQ::GetState())
#else
#define ASSERT_IRQ_MUST_BE_OFF()
#define ASSERT_IRQ_MUST_BE_ON()
#endif

#define TOTAL_USART_PORT       1
#define COM1                   ConvertCOM_ComHandle(0)
#define COM2                   ConvertCOM_ComHandle(1)

#define TOTAL_USB_CONTROLLER   1
#define USB1                   ConvertCOM_UsbHandle(0)

#define TOTAL_SOCK_PORT        0

#define TOTAL_DEBUG_PORT       1
#define COM_DEBUG              ConvertCOM_DebugHandle(0)

#define COM_MESSAGING          ConvertCOM_MessagingHandle(0)

#define USART_TX_IRQ_INDEX(x)       ( (x) ? 0 : 0 )     // TODO set right indexes
#define USART_DEFAULT_PORT          COM1
#define USART_DEFAULT_BAUDRATE      115200

#define USB_IRQ_INDEX               0                   // TODO set right index


#define PLATFORM_DEPENDENT_TX_USART_BUFFER_SIZE    512  // there is one TX for each usart port
#define PLATFORM_DEPENDENT_RX_USART_BUFFER_SIZE    512  // there is one RX for each usart port
#define PLATFORM_DEPENDENT_USB_QUEUE_PACKET_COUNT  32  // there is one queue for each pipe of each endpoint and the size of a single packet is sizeof(USB_PACKET64) == 68 bytes
");


                            tw.WriteLine(string.Format("#define DEBUG_TEXT_PORT         {0}", solution.DebuggerPort));
                            tw.WriteLine(string.Format("#define STDIO                   {0}", solution.DebuggerPort));
                            tw.WriteLine(string.Format("#define DEBUGGER_PORT           {0}", solution.DebuggerPort));
                            tw.WriteLine(string.Format("#define MESSAGING_PORT          {0}", solution.DebuggerPort));

                            tw.WriteLine(string.Format("#define RUNTIME_MEMORY_PROFILE__{0} 1", solution.MemoryProfile));

                            tw.WriteLine("#include <processor_selector.h>");

                            tw.WriteLine(string.Format("#endif // _PLATFORM_{0}_SELECTOR_H_ 1", solution.Name));
                        }
                    }

                    //foreach (MFProject prj in solution.Projects)
                    //{
                    //    SaveProjectProj(prj);
                    //}
                }
                catch
                {
                    Console.WriteLine("Error: Unable to save solution file " + solution.ProjectPath);
                }
            }
        }

        public List<MFComponentDescriptor> GetAvailableSolutions(string spoClientRoot)
        {
            List<MFComponentDescriptor> list = new List<MFComponentDescriptor>();
            string fullpath = ExpandEnvVars(Path.Combine(spoClientRoot, "Solutions"), "");

            foreach (string subdir in Directory.GetDirectories(fullpath))
            {
                foreach (string solution in Directory.GetFiles(subdir, "*.settings"))
                {
                    try
                    {
                        Project proj = LoadProject(solution);
                        Processor proc = null;

                        MFSolution sol = new MFSolution();

                        Dictionary<string, string> tbl = new Dictionary<string, string>();
                        tbl["PLATFORM"] = "Name";
                        tbl["PlatformGuid"] = "Guid";

                        LoadStringProps(proj, sol, tbl);

                        foreach (ProjectImportElement imp in proj.Xml.Imports)
                        {
                            if (imp.Project.ToLower().Contains(@"\devicecode\targets\"))
                            {
                                proc = LoadProcessorProj(imp.Project, "");
                            }
                        }

                        if (sol.IsSolutionWizardVisible)
                        {
                            list.Add(new MFComponentDescriptor(new MFComponent(MFComponentType.MFSolution, sol.Name, sol.Guid, solution), sol.Description, sol.Documentation, proj.FullPath, proc));
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("Error: loading solution file: " + solution + "\r\n", e.Message);
                    }
                }
            }
            return list;
        }

        private int ParseHexInt(string text, string key)
        {
            int val = -1;

            Regex rx = new Regex("\\s" + key + "\\s+([aAbBcCdDeEfFxX\\d]+)");
            Match m = rx.Match(text);
            if (m.Success)
            {
                string sval = m.Groups[1].Value.ToLower();

                if (sval.StartsWith("0x"))
                {
                    sval = sval.Remove(0, 2);
                    val = int.Parse(sval, System.Globalization.NumberStyles.HexNumber);
                }
                else
                {
                    val = int.Parse(sval);
                }
            }
            else
            {
                rx = new Regex("\\s" + key + "\\s+\\(([\\s\\d\\*]+)\\)");
                m = rx.Match(text);

                if (m.Success)
                {
                    string[] vals = m.Groups[1].Value.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);

                    val = 1;
                    foreach (string num in vals)
                    {
                        val *= int.Parse(num);
                    }
                }
            }

            return val;
        }

        internal MFSolution LoadSolutionProj(string solutionProjFile, string path)
        {
            MFSolution sol = new MFSolution();
            Project proj;
            string fullpath = ExpandEnvVars(solutionProjFile, path);

            string codebase = "";
            string codebasetype = "";
            string processor = "";

            try
            {
                proj = LoadProject(fullpath);

                path = Path.GetDirectoryName(fullpath);

                // load solution libraries

                LoadLibraries(Path.Combine(path, "DeviceCode"));

                Dictionary<string, string> tbl = new Dictionary<string, string>();
                tbl["PLATFORM"] = "Name";
                tbl["PlatformGuid"] = "Guid";

                LoadStringProps(proj, sol, tbl);

                foreach (ProjectItemGroupElement big in proj.Xml.ItemGroups)
                {
                    foreach (ProjectItemElement bi in big.Items)
                    {
                        MFBuildFile bf = new MFBuildFile();
                        bf.File = bi.Include;
                        bf.ItemName = bi.ItemType;
                        bf.Condition = bi.Condition;

                        sol.Items.Add(bf);
                    }
                }

                foreach (ResolvedImport imp in proj.Imports)
                {
                    if (imp.ImportedProject.FullPath.ToLower().Contains("\\devicecode\\targets\\"))
                    {
                        Processor proc = LoadProcessorProj(imp.ImportedProject.FullPath, "");

                        if (proc != null)
                        {
                            sol.Processor = new MFComponent(MFComponentType.Processor, proc.Name, proc.Guid, proc.ProjectPath);
                        }
                    }
                }

                if (sol.Processor == null || string.IsNullOrEmpty(sol.Processor.Name))
                {
                    foreach (ProjectProperty prop in proj.Properties)
                    {
                        if (prop.IsImported)
                        {
                            if (prop.Name == "TARGETPROCESSOR")
                            {
                                Processor proc = m_helper.FindProcessorByName(prop.EvaluatedValue);
                                if (proc != null)
                                {
                                    sol.Processor = new MFComponent(MFComponentType.Processor, proc.Name, proc.Guid);
                                }
                                else
                                {
                                    foreach (ProjectImportElement imp in proj.Xml.Imports)
                                    {
                                        if (imp.Project.ToUpper().Contains("\\DEVICECODE\\TARGETS\\"))
                                        {
                                            proc = LoadProcessorProj(imp.Project, "");
                                            break;
                                        }
                                    }
                                }

                                if (proc == null)
                                {
                                    sol.Processor = new MFComponent(MFComponentType.Processor, prop.EvaluatedValue);
                                    sol.Processor.Guid = "";
                                }
                                else
                                {
                                    sol.Processor = new MFComponent(MFComponentType.Processor, proc.Name, proc.Guid, proc.ProjectPath);
                                }
                            }
                        }
                    }
                }

                foreach (ProjectPropertyGroupElement pg in proj.Xml.PropertyGroups)
                {
                    foreach (ProjectPropertyElement bp in pg.Properties)
                    {
                        string cond = CombineConditionals(pg.Condition, bp.Condition);

                        switch (bp.Name)
                        {
                            case "Description":
                                sol.Description = bp.Value;
                                break;
                            case "Documentation":
                                sol.Documentation = bp.Value;
                                break;
                            case "INSTRUCTION_SET":
                                ISA isa = m_helper.FindISAByName(bp.Value);
                                if (isa == null)
                                {
                                    sol.DefaultISA = new MFComponent(MFComponentType.ISA, bp.Value);
                                }
                                else
                                {
                                    sol.DefaultISA = new MFComponent(MFComponentType.ISA, isa.Name, isa.Guid);
                                }
                                break;
                            case "PLATFORM":
                            case "TARGETPLATFORM":
                                sol.Name = bp.Value;
                                break;
                            case "PlatformGuid":
                                sol.Guid = bp.Value;
                                break;
                            case "TARGETPROCESSOR":
                                Processor proc = m_helper.FindProcessorByName(bp.Value);
                                if (proc != null)
                                {
                                    sol.Processor = new MFComponent(MFComponentType.Processor, proc.Name, proc.Guid);
                                }
                                else
                                {
                                    processor = bp.Value;
                                }
                                break;
                            //obsolete props (moved to processor settings)
                            case "PLATFORM_FAMILY":
                            case "ARM_TYPE":
                            case "MDK_DEVICE_TYPE":
                            case "CPUName":
                                break;
                            case "TARGETCODEBASE":
                                codebase = bp.Value;
                                break;
                            case "TARGETCODEBASETYPE":
                                codebasetype = bp.Value;
                                break;
                            default:
                                MFProperty prop = new MFProperty();
                                prop.Name = bp.Name;
                                prop.Value = bp.Value;
                                prop.Condition = cond;
                                sol.Properties.Add(prop);
                                break;
                        }
                    }
                }

                // for legacy settings files
                if (sol.Processor == null)
                {
                    string procFile = Path.Combine(Environment.GetEnvironmentVariable("SPOCLIENT"), "DeviceCode\\Targets\\" + codebasetype + "\\" + codebase + "\\" + processor + ".settings");

                    if (File.Exists(procFile))
                    {
                        Processor proc = LoadProcessorProj(procFile, "");

                        sol.Processor = new MFComponent(MFComponentType.Processor, proc.Name, proc.Guid);
                    }
                    else // delay load
                    {
                        sol.Processor = new MFComponent(MFComponentType.Processor, processor);
                        sol.Processor.Guid = "";
                    }
                }

                if (string.IsNullOrEmpty(sol.Name)) sol.Name = Path.GetFileNameWithoutExtension(solutionProjFile);
                if (string.IsNullOrEmpty(sol.Guid)) sol.Guid = System.Guid.NewGuid().ToString("B");

                sol.ProjectPath = ConvertPathToEnv(solutionProjFile);

                foreach (string subdir in Directory.GetDirectories(Path.GetDirectoryName(fullpath)))
                {
                    if (subdir.EndsWith("DeviceCode", StringComparison.OrdinalIgnoreCase)) continue;
                    if (subdir.EndsWith("ManagedCode", StringComparison.OrdinalIgnoreCase)) continue;

                    foreach (string projFile in Directory.GetFiles(subdir, "*.proj"))
                    {
                        MFProject mfproj = LoadProjectProj(projFile, Path.GetDirectoryName(projFile));

                        if (mfproj != null) sol.Projects.Add(mfproj);
                    }
                }

                // load platform selector file for memory data
                string platSelectorFile = Path.Combine(path, "platform_selector.h");
                if (File.Exists(platSelectorFile))
                {
                    string data = "";

                    using (TextReader tr = File.OpenText(platSelectorFile))
                    {
                        data = tr.ReadToEnd();
                    }


                    sol.SystemClockSpeed = ParseHexInt(data, "SYSTEM_CLOCK_HZ");
                    sol.SlowClockSpeed = ParseHexInt(data, "SLOW_CLOCKS_PER_SECOND");

                    Regex rx = null;
                    Match m = null;

                    if (sol.SlowClockSpeed == -1)
                    {
                        rx = new Regex("\\sSLOW_CLOCKS_PER_SECOND\\s+SYSTEM_CLOCK_HZ");
                        if (rx.IsMatch(data))
                        {
                            sol.SlowClockSpeed = sol.SystemClockSpeed;
                        }
                    }

                    sol.RamBase = ParseHexInt(data, "SRAM1_MEMORY_Base");
                    sol.RamLength = ParseHexInt(data, "SRAM1_MEMORY_Size");
                    sol.FlashBase = ParseHexInt(data, "FLASH_MEMORY_Base");
                    sol.FlashLength = ParseHexInt(data, "FLASH_MEMORY_Size");

                    rx = new Regex("#define\\s+DEBUGGER_PORT\\s+([\\w\\d_]+)", RegexOptions.IgnoreCase);
                    m = rx.Match(data);
                    if (m.Success)
                    {
                        sol.DebuggerPort = m.Groups[1].Value;
                    }
                    else
                    {
                        sol.DebuggerPort = "COM1";
                    }

                    rx = new Regex("\\sRUNTIME_MEMORY_PROFILE__([\\w]+)");
                    m = rx.Match(data);
                    if (m.Success)
                    {
                        sol.MemoryProfile = m.Groups[1].Value;
                    }
                    else
                    {
                        sol.MemoryProfile = "medium";
                    }
                }

                MFSolution dbSol = m_helper.FindSolutionByName(sol.Name);

                if (null == dbSol)
                {
                    m_helper.DefaultInventory.Solutions.Add(sol);
                }
                else
                {
                    sol = dbSol;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: Exception in load solution " + e.Message);
                sol = null;
            }

            return sol;
        }

        internal bool SaveLibraryCategoryProj(LibraryCategory libCat)
        {
            string fullpath = ExpandEnvVars(libCat.ProjectPath, "");

            Project proj = LoadProject(fullpath);

            proj.ProjectCollection.UnloadProject(proj);

            proj = new Project();
            proj.Xml.DefaultTargets = "Build";
            bool fRet = false;

            try
            {
                if (string.IsNullOrEmpty(fullpath))
                {
                    fullpath = Path.Combine(Environment.GetEnvironmentVariable("SPOCLIENT"), @"framework\features\" + libCat.Name + ".libcatproj");
                }

                if (libCat.LibraryProjCache.Count > 0)
                {
                    ProjectItemGroupElement big = proj.Xml.AddItemGroup();

                    foreach (string libRef in libCat.LibraryProjCache)
                    {
                        big.AddItem("LibraryCollection", libRef);
                    }
                }

                List<string> save = libCat.LibraryProjCache;
                libCat.LibraryProjCache = null;

                ProjectPropertyGroupElement bpg = proj.Xml.AddPropertyGroup();

                bpg.AddProperty(PKUI_LibCatTag, SerializeXml(libCat));

                libCat.LibraryProjCache = save;

                proj.Save(fullpath);

                fRet = true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: failure saving LibCat file: " + fullpath + "\r\n", e.Message);
            }
            return fRet;
        }

        internal LibraryCategory LoadLibraryCategoryProj(string libCatProjFile, string path)
        {
            Project proj;
            LibraryCategory libcat = null;

            string fullpath = ExpandEnvVars(libCatProjFile, path);

            try
            {
                proj = LoadProject(fullpath);

                foreach (ProjectPropertyGroupElement bpg in proj.Xml.PropertyGroups)
                {
                    foreach (ProjectPropertyElement bp in bpg.Properties)
                    {
                        if (bp.Name == PKUI_LibCatTag)
                        {
                            libcat = (LibraryCategory)DeserializeXml(bp.Value, typeof(LibraryCategory));
                            libcat.ProjectPath = ConvertPathToEnv(fullpath);
                        }
                    }
                }

                if (libcat == null)
                {
                    libcat = new LibraryCategory();
                    libcat.Name = Path.GetFileNameWithoutExtension(libCatProjFile);
                    libcat.Guid = System.Guid.NewGuid().ToString("B");
                    libcat.ProjectPath = ConvertPathToEnv(fullpath);
                }
                else
                {
                    LibraryCategory lcDb = m_helper.FindLibraryCategory(libcat.Guid);
                    if (null == lcDb)
                    {
                        m_helper.DefaultInventory.LibraryCategories.Add(libcat);
                    }
                    else
                    {
                        return lcDb;
                    }
                }

                foreach (ProjectItemGroupElement big in proj.Xml.ItemGroups)
                {
                    foreach (ProjectItemElement bi in big.Items)
                    {
                        libcat.LibraryProjCache.Add(bi.Include);

                        Library lib = LoadLibraryProj(bi.Include, path);
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: loading LibCat file: " + fullpath + "\r\n", e.Message);
            }


            return libcat;
        }

        internal void CreateSolutionDirProj(MFSolution solution)
        {
            string solutionDir = @"$(SPOCLIENT)\Solutions\" + solution.Name;
            string fullpath = ExpandEnvVars(Path.Combine(solutionDir, "dotnetmf.proj"), "");

            Project proj = LoadProject(fullpath);

            proj.ProjectCollection.UnloadProject(proj);

            proj = new Project();
            proj.Xml.DefaultTargets = "Build";

            try
            {
                ProjectPropertyGroupElement bpg = proj.Xml.AddPropertyGroup();
                bpg.AddProperty("Directory", RemoveSpoClient(solutionDir));
                bpg.AddProperty("MFSettingsFile", Path.Combine(solutionDir, solution.Name + ".settings"));

                ProjectItemGroupElement big = proj.Xml.AddItemGroup();
                foreach (MFProject mfproj in solution.Projects)
                {
                    if (!string.IsNullOrEmpty(mfproj.Directory) && !string.IsNullOrEmpty(mfproj.ProjectPath))
                    {
                        ProjectItemElement bi = big.AddItem("RequiredProjects", Path.Combine(Path.GetFileName(mfproj.Directory), Path.GetFileName(mfproj.ProjectPath)));
                    }
                }

                proj.Xml.AddImport(@"$(SPOCLIENT)\tools\targets\Microsoft.SPOT.System.Settings");
                proj.Xml.AddImport(@"$(SPOCLIENT)\tools\targets\Microsoft.SPOT.System.Targets");

                proj.Save(fullpath);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: loading LibCat file: " + fullpath + "\r\n", e.Message);
            }
        }

        internal void CopyProjFilesFromClone(MFProject proj, MFSolution solution)
        {
            if (proj.m_cloneProj != null)
            {
                string dirSource = MsBuildWrapper.ExpandEnvVars(Path.GetDirectoryName(proj.m_cloneProj.ProjectPath), "");
                string dirTarg = MsBuildWrapper.ExpandEnvVars(Path.GetDirectoryName(proj.ProjectPath), "");
                string dirSourceRoot = Path.GetDirectoryName(dirSource);
                string dirTargRoot = Path.GetDirectoryName(dirTarg);

                try
                {
                    CopyClonedFiles(dirSourceRoot, dirTargRoot, solution.m_cloneSolution == null ? "$(PLATFORM)" : solution.m_cloneSolution.Name, solution.Name, false);
                    CopyClonedFiles(dirSource, dirTarg, solution.m_cloneSolution == null ? "$(PLATFORM)" : solution.m_cloneSolution.Name, solution.Name, true);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Error: Copying clone files\r\n\tsrc:" + dirSource + "\r\n\tdst:" + dirTarg + "\r\n", e.Message);
                }

            }
        }
    }
}
