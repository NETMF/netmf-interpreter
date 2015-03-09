using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace Microsoft.SPOT.Tasks.NativeBuild
{
    public class MakeVCProject : Task
    {
        private string targetName;

        [Required]
        public string TargetName
        {
            get { return targetName; }
            set { targetName = value; }
        }

        private string projectName;

        [Required]
        public string ProjectName
        {
            get { return projectName; }
            set { projectName = value; }
        }

        private string pchfile;

        [Required]
        public string PCH
        {
            get { return pchfile; }
            set { pchfile = value; }
        }

        private string projectGuid;

        [Required]
        public string Guid
        {
            get { return projectGuid; }
            set { projectGuid = value; }
        }

        private string noOpt = null;

        public string NoOpt
        {
            get { return noOpt; }
            set { noOpt = value; }
        }

        private string noOptOptimization;

        [Required]
        public string NoOptOptimization
        {
            get { return noOptOptimization; }
            set { noOptOptimization = value; }
        }

        private string includePaths = "";

        public string IncludePaths
        {
            get { return includePaths; }
            set { includePaths = value; }
        }

        private string cfiles = "";

        public string CFiles
        {
            get { return cfiles; }
            set { cfiles = value; }
        }

        private string hfiles = "";

        public string HFiles
        {
            get { return hfiles; }
            set { hfiles = value; }
        }

        private string resfiles = "";

        public string ResFiles
        {
            get { return resfiles; }
            set { resfiles = value; }
        }

        private string customfiles = "";

        public string CustomFiles
        {
            get { return customfiles; }
            set { customfiles = value; }
        }

        private string targetType = "Library";

        public string TargetType
        {
            get { return targetType; }
            set { targetType = value; }
        }

        private string subsystem = "WINDOWS";

        public string Subsystem
        {
            get { return subsystem; }
            set { subsystem = value; }
        }

        private string solutionDir;

        public string SolutionDir
        {
            get { return solutionDir; }
            set { solutionDir = value; }
        }

        private string charSet = "1";

        public string CharSet
        {
            get { return charSet; }
            set { charSet = value; }
        }

        private string nameSpace = "";

        public string NameSpace
        {
            get { return nameSpace; }
            set { nameSpace = value; }
        }

        private string extraLibs = "";

        public string ExtraLibs
        {
            get { return extraLibs; }
            set { extraLibs = value; }
        }

        private string ignoreLibs = "";

        public string IgnoreLibs
        {
            get { return ignoreLibs; }
            set { ignoreLibs = value; }
        }

        private string libDirs = "";

        public string LibDirs
        {
            get { return libDirs; }
            set { libDirs = value; }
        }

        private string delayLoadedDlls = "";

        public string DelayLoadedDlls
        {
            get { return delayLoadedDlls; }
            set { delayLoadedDlls = value; }
        }

        private bool IsExecutable
        {
            get
            {
                return (TargetType == "Executable" || TargetType == "Command");
            }
        }

        private string TypeDefine
        {
            get
            {
                if (IsExecutable)
                {
                    return "_" + Subsystem;
                }
                else
                {
                    return "_LIB";
                }
            }
        }

        private string ConfigurationType
        {
            get
            {
                if (IsExecutable)
                {
                    return "1";
                }
                else
                {
                    return "4";
                }
            }
        }

        private string LibrarianToolPart(string targetName)
        {
            if (IsExecutable)
            {
                return "";
            }
            return
                "    <Tool\r\n" +
                "         Name=\"VCLibrarianTool\"\r\n" +
                ((ExtraLibs.Length > 0) ? "         AdditionalDependencies=\"" + ExtraLibs + "\"\r\n" : "") +
                "         OutputFile=\"$(OutDir)/" + targetName + ".lib\"\r\n" +
                "    />\r\n";
        }

        private string LinkerToolPart(string targetName, bool debugBuild)
        {
            if (!IsExecutable)
            {
                return "";
            }
            return
                "    <Tool\r\n" +
                "         Name=\"VCLinkerTool\"\r\n" +
                ((ExtraLibs.Length > 0) ? "         AdditionalDependencies=\"" + ExtraLibs + "\"\r\n" : "") +
                ((targetType == "Executable") ? "         OutputFile=\"$(OutDir)/" + targetName + ".exe\"\r\n" : "") +
                ((targetType == "Command") ? "         OutputFile=\"$(OutDir)/" + targetName + ".com\"\r\n" : "") +
                "         LinkIncremental=\"" + (debugBuild ? "2" : "1") + "\"\r\n" +
                ((LibDirs.Length > 0) ? "         AdditionalLibraryDirectories=\"" + ExpandList("$(SolutionDir)", LibDirs) + "\"\r\n" : "") +
                ((IgnoreLibs.Length > 0) ? "         IgnoreDefaultLibraryNames=\"" + IgnoreLibs + "\"\r\n" : "") +
                ((DelayLoadedDlls.Length > 0) ? "         DelayLoadDLLs=\"" + DelayLoadedDlls + "\"\r\n" : "") +
                "         GenerateDebugInformation=\"true\"\r\n" +
                "         ProgramDatabaseFile=\"$(OutDir)/" + targetName + ".pdb\"\r\n" +
                ((subsystem == "WINDOWS") ? "         SubSystem=\"2\"\r\n" : "") +
                ((subsystem == "CONSOLE") ? "         SubSystem=\"1\"\r\n" : "") +
                (!debugBuild ? "         OptimizeReferences=\"2\"\r\n" : "") +
                (!debugBuild ? "         EnableCOMDATFolding=\"2\"\r\n" : "") +
                "         TargetMachine=\"1\"\r\n" +
                "    />\r\n";
        }

        private string ManifestToolPart()
        {
            if (!IsExecutable)
            {
                return "";
            }
            return
                "    <Tool\r\n" +
                "         Name=\"VCManifestTool\"\r\n" +
                "    />\r\n";
        }

        private string AppVerifierToolPart()
        {
            if (!IsExecutable)
            {
                return "";
            }
            return
                "    <Tool\r\n" +
                "         Name=\"VCAppVerifierTool\"\r\n" +
                "    />\r\n";
        }

        private string WebDeploymentToolPart()
        {
            if (!IsExecutable)
            {
                return "";
            }
            return
                "    <Tool\r\n" +
                "         Name=\"VCWebDeploymentTool\"\r\n" +
                "    />\r\n";
        }

        private string Configuration(
            string name,
            string targetName,
            string debugDefine,
            string extraDefines,
            string optimization,
            bool inlineFunctionExpansion,
            bool omitFramePointers,
            bool minimalRebuild,
            bool stringPooling,
            bool enableFunctionLevelLinking,
            bool basicRuntimeChecks,
            string runTimeLibrary,
            string debugInformationFormat)
        {
            return "  <Configuration\r\n" +
                "         Name=\"" + name + "|Win32\"\r\n" +
#if USE_EXPLICIT_NAME
                "         OutputDirectory=\"" + name + "\"\r\n" +
                "         IntermediateDirectory=\""+name+"\"\r\n" +
#else
 "         OutputDirectory=\"$(ConfigurationName)\"\r\n" +
                "         IntermediateDirectory=\"$(ConfigurationName)\"\r\n" +
#endif
 "         ConfigurationType=\"" + ConfigurationType + "\"\r\n" +
                "         InheritedPropertySheets=\"UpgradeFromVC71.vsprops\"\r\n" +
                "         CharacterSet=\"" + CharSet + "\"\r\n" +
                "    >\r\n" +
                "    <Tool\r\n" +
                "         Name=\"VCPreBuildEventTool\"\r\n" +
                "    />\r\n" +
                "    <Tool\r\n" +
                "         Name=\"VCCustomBuildTool\"\r\n" +
                "    />\r\n" +
                "    <Tool\r\n" +
                "         Name=\"VCXMLDataGeneratorTool\"\r\n" +
                "    />\r\n" +
                "    <Tool\r\n" +
                "         Name=\"VCWebServiceProxyGeneratorTool\"\r\n" +
                "    />\r\n" +
                "    <Tool\r\n" +
                "         Name=\"VCMIDLTool\"\r\n" +
                "    />\r\n" +
                "    <Tool\r\n" +
                "      Name=\"VCCLCompilerTool\"\r\n" +
                "      Optimization=\"" + optimization + "\"\r\n" +
                (inlineFunctionExpansion ? "      InlineFunctionExpansion=\"1\"\r\n" : "") +
                (omitFramePointers ? "      OmitFramePointers=\"true\"\r\n" : "") +
                ((IncludePaths.Length > 0) ? "      AdditionalIncludeDirectories=\"" + QuotedIncludePaths() + "\"\r\n" : "") +
                "      PreprocessorDefinitions=\"WIN32;" + debugDefine + ";" + TypeDefine + (extraDefines.Length > 0 ? (";" + extraDefines) : "") + "\"\r\n" +
                (minimalRebuild ? "      MinimalRebuild=\"true\"\r\n" : "") +
                (basicRuntimeChecks ? "      BasicRuntimeChecks=\"3\"\r\n" : "") +
                (stringPooling ? "      StringPooling=\"true\"\r\n" : "") +
                "      RuntimeLibrary=\"" + runTimeLibrary + "\"\r\n" +
                (enableFunctionLevelLinking ? "      EnableFunctionLevelLinking=\"true\"\r\n" : "") +
                ((PCH != "none") ? "      UsePrecompiledHeader=\"2\"\r\n" : "") +
                ((PCH != "stdafx.h" && PCH != "none") ? "      PrecompiledHeaderThrough=\"" + PCH + "\"\r\n" : "") +
                "      WarningLevel=\"3\"\r\n" +
                "      Detect64BitPortabilityProblems=\"true\"\r\n" +
                "      DebugInformationFormat=\"" + debugInformationFormat + "\"\r\n" +
                "    />\r\n" +
                "    <Tool\r\n" +
                "         Name=\"VCManagedResourceCompilerTool\"\r\n" +
                "    />\r\n" +
                "    <Tool\r\n" +
                "         Name=\"VCResourceCompilerTool\"\r\n" +
                "    />\r\n" +
                "    <Tool\r\n" +
                "         Name=\"VCPreLinkEventTool\"\r\n" +
                "    />\r\n" +
                LibrarianToolPart(targetName) +
                LinkerToolPart(targetName, debugDefine == "_DEBUG") +
                "	 <Tool\r\n" +
                "         Name=\"VCALinkTool\"\r\n" +
                "    />\r\n" +
                ManifestToolPart() +
                "    <Tool\r\n" +
                "         Name=\"VCXDCMakeTool\"\r\n" +
                "    />\r\n" +
                "    <Tool\r\n" +
                "         Name=\"VCBscMakeTool\"\r\n" +
                "    />\r\n" +
                "    <Tool\r\n" +
                "         Name=\"VCFxCopTool\"\r\n" +
                "    />\r\n" +
                AppVerifierToolPart() +
                WebDeploymentToolPart() +
                "    <Tool\r\n" +
                "         Name=\"VCPostBuildEventTool\"\r\n" +
                "    />\r\n" +
                "  </Configuration>\r\n";
        }

        private string DebugConfiguration(string name, string targetName, string extraDefines)
        {
            return Configuration(name, targetName, "_DEBUG", extraDefines, "0", false, false, true, false, false, true, "1", "4");
        }

        private string ReleaseConfiguration(string name, string targetName, string extraDefines)
        {
            return Configuration(name, targetName, "NDEBUG", extraDefines, "2", true, true, false, true, true, false, "0", "3");
        }

        private string NoOptConfiguration(string extraDefines)
        {
            if (NoOpt == "Debug")
            {
                return Configuration("Release_NoOptForParser", TargetName, "_DEBUG", extraDefines, "0", false, false, true, false, false, true, "1", "4");
            }
            else if (NoOpt == "Release")
            {
                if (NoOptOptimization == "true")
                {
                    return Configuration("Release_NoOptForParser", TargetName, "NDEBUG", extraDefines, "2", true, true, false, true, true, false, "0", "3");
                }
                else
                {
                    return Configuration("Release_NoOptForParser", TargetName, "NDEBUG", extraDefines, "0", false, false, false, true, true, false, "0", "3");
                }
            }
            return "";
        }

        private void SaveString(string path, string text)
        {
            if (File.Exists(path)) File.Delete(path);
            StreamWriter fs = File.CreateText(path);
            fs.Write(text);
            fs.Close();
        }

        public override bool Execute()
        {
            Console.WriteLine("ExtraLibs=" + ExtraLibs);
            SaveString(TargetName + "-test.vcproj",
                "<?xml version=\"1.0\" encoding=\"Windows-1252\"?>\r\n" +
                "<VisualStudioProject\r\n" +
                "  ProjectType=\"Visual C++\"\r\n" +
                "  Version=\"8.00\"\r\n" +
                "  Name=\"" + ProjectName + "\"\r\n" +
                "  ProjectGUID=\"{" + Guid + "}\"\r\n" +
                ((NameSpace.Length > 0) ? "  RootNamespace=\"" + NameSpace + "\"\r\n" : "") +
                "  Keyword=\"Win32Proj\"\r\n" +
                "  SignManifests=\"true\"\r\n>\r\n" +
                "  <Platforms>\r\n" +
                "    <Platform\r\n    Name=\"Win32\"\r\n  />\r\n" +
                "  </Platforms>\r\n" +
                "  <ToolFiles>\r\n</ToolFiles>\r\n" +
                "  <Configurations>\r\n" +
                DebugConfiguration("Debug", TargetName, "") +
                ReleaseConfiguration("Release", TargetName, "") +
                NoOptConfiguration("") +
                "  </Configurations>\r\n" +
                "  <References>\r\n" +
                "  </References>\r\n" +
                "  <Files>\r\n" +
                CustomFileFilters() +
                Filter("Source Files", "cpp;c;cxx;def;odl;idl;hpj;bat;asm", SourceFilePart()) +
                Filter("Header Files", "h;hpp;hxx;hm;inl;inc", HeaderFilePart()) +
                Filter("Resource Files", "rc;ico;cur;bmp;dlg;rc2;rct;bin;rgs;gif;jpg;jpeg;jpe", ResourceFilePart()) +
                "  </Files>\r\n" +
                "  <Globals>\r\n" +
                "  </Globals>\r\n" +
                "</VisualStudioProject>\r\n");
            return true;
        }

        private string CustomFileFilters()
        {
            // This is intended for handling the ARM code but is incomplete;
            // in particular it doesn't handle the nesting of the filter
            // groups (mapping the directory hierarchy) properly

            if (CustomFiles == null || CustomFiles.Length == 0) return "";
            string rtn = "";
            // Custom files must be grouped together by paths
            string[] files = CustomFiles.Split(new char[1] { ';' });
            int ppos = files[0].LastIndexOf('\\');
            string path = (ppos > 0) ? files[0].Substring(0, ppos) : "";
            string fileGroup = "";
            foreach (string f in files)
            {
                ppos = f.LastIndexOf('\\');
                string myPath = (ppos > 0) ? f.Substring(0, ppos) : "";
                if (myPath == path)
                {
                    fileGroup += ";" + f.Substring(ppos + 1);
                }
                else
                {
                    // end of group - dump 'em and start a new group
                    rtn += Filter(path, "", fileGroup);
                    fileGroup = f.Substring(ppos + 1);
                    path = myPath;
                }
            }
            return rtn;
        }

        private string Filter(string name, string extensions, string files)
        {
            return
                "  <Filter\r\n" +
                "    Name=\"" + name + "\"\r\n" +
                "    Filter=\"" + extensions + "\"\r\n" +
                "  >\r\n" +
                files +
                "  </Filter>\r\n";
        }

        private string PCHSourceFileConfigurationPart(string releaseName)
        {
            return
                "    <FileConfiguration\r\n" +
                "      Name=\"" + releaseName + "|Win32\"\r\n" +
                "      >\r\n" +
                "      <Tool\r\n" +
                "        Name=\"VCCLCompilerTool\"\r\n" +
                "        UsePrecompiledHeader=\"1\"\r\n" +
                ((PCH != "stdafx.h" && PCH != "none") ? "        PrecompiledHeaderThrough=\"" + PCH + "\"\r\n" : "") +
                "      />\r\n" +
                "    </FileConfiguration>\r\n";
        }

        private string ExpandList(string prefix, string itemList)
        {
            string rtn = "";
            string sep = "";
            string[] items = itemList.Split(new char[1] { ';' });
            foreach (string i in items)
            {
                if (i.Length > 0)
                {
                    rtn += sep + prefix + i;
                    sep = ";";
                }
            }
            return rtn;
        }

        private string FilePart(string fileList)
        {
            string rtn = "";
            string[] files = fileList.Split(new char[1] { ';' });
            foreach (string f in files)
            {
                if (f.Length > 0)
                {
                    rtn += "  <File\r\n    RelativePath=\"" + f + "\"\r\n  >\r\n  </File>\r\n";
                }
            }
            return rtn;
        }

        private string SourceFilePart()
        {
            if (PCH == null || PCH.Length == 0 || PCH == "none")
            {
                return FilePart(CFiles);
            }
            int lpos = PCH.LastIndexOf('.');
            string pcc = PCH.Substring(0, lpos) + ".cpp";
            string pcclower = pcc.ToLower();

            string rtn = "";
            string[] files = CFiles.Split(new char[1] { ';' });
            foreach (string f in files)
            {
                if (f.Length > 0)
                {
                    if (f.ToLower() == pcclower)
                    {
                        rtn +=
                            "  <File\r\n" +
                            "    RelativePath=\"" + pcc + "\"\r\n" +
                            "  >\r\n" +
                            PCHSourceFileConfigurationPart("Debug") +
                            PCHSourceFileConfigurationPart("Release") +
                            ((NoOpt == "Release") ? PCHSourceFileConfigurationPart("Release_NoOptForParser") : "") +
                            ((NoOpt == "Debug") ? PCHSourceFileConfigurationPart("Release_NoOptForParser") : "") +
                            "  </File>\r\n";
                    }
                    else
                    {
                        rtn += "  <File\r\n    RelativePath=\"" + f + "\"\r\n  >\r\n  </File>\r\n";
                    }
                }
            }
            return rtn;
        }

        private string HeaderFilePart()
        {
            return FilePart(hfiles);
        }

        private string ResourceFilePart()
        {
            string rtn = "";
            string[] files = resfiles.Split(new char[1] { ';' });
            foreach (string f in files)
            {
                if (f.Length > 0)
                {
                    rtn += "  <File\r\n    RelativePath=\"" + f + "\"\r\n  >\r\n  </File>\r\n";
                }
            }
            return rtn;
        }

        private string QuotedIncludePaths()
        {
            string sd = SolutionDir.ToLower();
            string rtn = "";
            string[] includes = IncludePaths.Split(new char[1] { ';' });
            foreach (string p in includes)
            {
                rtn += "&quot;";
                if (p.Length > sd.Length && p.Substring(0, sd.Length).ToLower() == sd)
                {
                    string rp = p.Substring(sd.Length); // relative path
                    if (rp[0] == '\\')
                    {
                        rp = rp.Substring(1);
                    }
                    rtn += "$(SolutionDir)" + rp;
                }
                else
                {
                    rtn += p;
                }
                rtn += "&quot;;";
            }
            // strip off the last semicolon to match VS
            if (rtn.Length > 0) rtn = rtn.Substring(0, rtn.Length - 1);
            return rtn;
        }
    }
}
