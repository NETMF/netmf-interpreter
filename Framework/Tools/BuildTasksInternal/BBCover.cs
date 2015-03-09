using System;
using System.Net.Mail;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;

using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;

using Task = Microsoft.Build.Utilities.Task;

namespace Microsoft.SPOT.Tasks.Internal
{
    public class BBCover : Task
    {
        private class MsbuildDfltLogger : MiniLogger
        {
            Task _task;

            public MsbuildDfltLogger(Task t)
            {
                _task = t;
            }

            public override void Say(String msg)
            {
                _task.Log.LogMessageFromText(msg, MessageImportance.Normal);
            }

            public override void Warn(String msg)
            {
                _task.Log.LogWarning(msg);
            }

            public override void Complain(String msg)
            {
                _task.Log.LogError(msg);
            }

            public override void Complain(Exception e)
            {
                _task.Log.LogErrorFromException(e);
            }

            public override void CommandLine(String cmdLine)
            {
                _task.Log.LogCommandLine(cmdLine);
            }
        }

        public enum InstrumentationModeKind
        {
            UserMode, KernelMode, NoImportDLLMode, Default
        }

        public enum RegistrationHookLocaleKind
        {
            None, Entry, Exports, Internal, Default
        }

        MiniLogger _logger = null;

        FileInfo binaryFile;
        DirectoryInfo originalsDirectory;

        string db;
        string customVer;

        bool ignoreAsm = false;

        string bbtfCmdFile;
        string entryName;
        InstrumentationModeKind iMode = InstrumentationModeKind.Default;
        bool noHookUnload = false;
        bool pdbNoFixups = false;
        bool probeOpt = false;
        RegistrationHookLocaleKind registrationHookLocale = RegistrationHookLocaleKind.Default; // TODO: this turns out to be a collection of flags, rather than a simple alternation in the latest version.
        bool relop = false;
        bool rereadable = false;
        bool tsGlobal = false;
        string unloadFunction;
        bool verbose = false;
        string ignorableWarnings;
        FileInfo certificateFile = null;


        public BBCover()
        {
            _logger = new MsbuildDfltLogger(this);
        }

        public BBCover(MiniLogger ml)
        {
            _logger = ml;
        }


        [Required]
        public string BinaryFile
        {
            get { return binaryFile.FullName; }
            set {
                binaryFile = new FileInfo(value);
                if ( !binaryFile.Exists ) throw new Exception(value + " does not exist");
            }
        }

        [Required]
        public string OriginalsDirectory
        {
            get { return originalsDirectory.FullName; }
            set { originalsDirectory = System.IO.Directory.CreateDirectory(value); }
        }

        public string CertificateFile
        {
            get { return certificateFile.FullName; }
            set {
                certificateFile = new FileInfo(value);
                if (!certificateFile.Exists ) throw new Exception(value + " does not exist");
            }
        }

        public String IgnorableWarnings
        {
            get { return ignorableWarnings; }
            set { ignorableWarnings = value; }
        }

        public InstrumentationModeKind InstrumentationMode
        {
            get { return iMode; }
            set { iMode = value; }
        }

        public RegistrationHookLocaleKind RegistrationHook
        {
            get { return registrationHookLocale; }
            set { registrationHookLocale = value; }
        }


        public string DatabaseInfo
        {
            get { return db; }
            set { db = value; }
        }

        [Required]
        public string BuildID // corresponds to BBCover's /customver option, but users always find that name confusing and hard to remember
        {
            get { return customVer; }
            set { customVer = value; }
        }

        public string BbtfCmdFile
        {
            get { return bbtfCmdFile; }
            set { bbtfCmdFile = value; } // Consider putting validation code here to check that the file at this path actually exists
        }

        public string EntryFunction
        {
            get { return entryName; }
            set { entryName = value; }
        }

        public string UnloadFunction
        {
            get { return unloadFunction; }
            set { unloadFunction = value; }
        }

        public bool IgnoreAsm
        {
            get { return ignoreAsm; }
            set { ignoreAsm = value; }
        }


        public bool NoHookUnload
        {
            get { return noHookUnload; }
            set { noHookUnload = value; }
        }


        public bool OmitFixupRecords // corresponds to BBCover's poorly-named /pdbNoFixups option
        {
            get { return pdbNoFixups; }
            set { pdbNoFixups = value; }
        }


        public bool OptimizeArcProbes // this is BBCover's /ProbeOpt option
        {
            get { return probeOpt; }
            set { probeOpt = value; }
        }


        public bool RelationalOperatorCoverage
        {
            get { return relop; }
            set { relop = value; }
        }


        public bool Rereadable
        {
            get { return rereadable; }
            set { rereadable = value; }
        }


        public bool VulcanFriendly  // NB same thing as "Rereadable"
        {
            get { return rereadable; }
            set { rereadable = value; }
        }


        public bool SessionSpaceBinary  // corresponds to BBCover's /tsGlobal option
        {
            get { return tsGlobal; }
            set { tsGlobal = value; }
        }

        public bool Verbose
        {
            get { return verbose; }
            set { verbose = value; }
        }


        public override bool Execute()
        {
            String targetBinary = binaryFile.FullName;
            String targetPdb = System.IO.Path.ChangeExtension(binaryFile.FullName, "pdb");

            String backupBinary = Path.Combine(originalsDirectory.FullName, binaryFile.Name);
            String backupPdb = System.IO.Path.ChangeExtension(backupBinary, "pdb");

            try
            {
                // Move the originals into the new directory (it was created earlier, in setting the required property)
                if ( File.Exists(backupBinary) )
                    File.Delete(backupBinary);
                File.Move(targetBinary, backupBinary);

                if ( File.Exists(backupPdb) )
                    File.Delete(backupPdb);
                File.Move(targetPdb, backupPdb);

                // Run BBCover to create instrumented versions in the original location

                // handle the required args
                String bbcoverCmd = String.Format("/i \"{0}\" /o \"{1}\" /opdb \"{2}\" /customVer {3}", backupBinary, targetBinary, targetPdb, BuildID);

                // append the optional args
                if ( Verbose ) bbcoverCmd += " /verbose";
                if ( IgnoreAsm ) bbcoverCmd += " /ignoreAsm";
                if ( NoHookUnload ) bbcoverCmd += " /noHookUnload";
                if ( OmitFixupRecords ) bbcoverCmd += " /pdbNoFixups";
                if ( OptimizeArcProbes ) bbcoverCmd += " /probeOpt";
                if ( RelationalOperatorCoverage ) bbcoverCmd += " /relop";
                if ( Rereadable ) bbcoverCmd += " /rereadable";
                if ( SessionSpaceBinary ) bbcoverCmd += " /tsGlobal";

                if ( !String.IsNullOrEmpty(DatabaseInfo) ) bbcoverCmd += String.Format(" /db \"{0}\"", DatabaseInfo);
                if ( !String.IsNullOrEmpty(EntryFunction) ) bbcoverCmd += " /entryName " + EntryFunction;
                if ( !String.IsNullOrEmpty(BbtfCmdFile) ) bbcoverCmd += String.Format(" /cmd \"{0}\"", BbtfCmdFile);
                if ( !String.IsNullOrEmpty(UnloadFunction) ) bbcoverCmd += " /unload " + UnloadFunction;

                switch ( InstrumentationMode )
                {
                    case InstrumentationModeKind.Default:
                        break;

                    case InstrumentationModeKind.KernelMode:
                        bbcoverCmd += " /kernelmode";
                        break;

                    case InstrumentationModeKind.UserMode:
                        bbcoverCmd += " /usermode";
                        break;

                    case InstrumentationModeKind.NoImportDLLMode:
                        bbcoverCmd += " /noimportdll";
                        break;
                }

                switch ( RegistrationHook )
                {
                    case RegistrationHookLocaleKind.Default:
                        break;

                    case RegistrationHookLocaleKind.None:
                        bbcoverCmd += " /AutoRegister none";
                        break;

                    case RegistrationHookLocaleKind.Entry:
                        bbcoverCmd += " /AutoRegister entry";
                        break;

                    case RegistrationHookLocaleKind.Exports:
                        bbcoverCmd += " /AutoRegister exports";
                        break;

                    case RegistrationHookLocaleKind.Internal:
                        bbcoverCmd += " /AutoRegister internal";
                        break;
                }

                // TODO: support multiple /DontInstrument and /DontInstrumentSrc ranges

                return this.ExecuteCmd(bbcoverCmd);
            }
            catch (Exception e)
            {
                _logger.Complain(e);

                return false;
            }

        }


        public bool ExecuteCmd(String bbcoverArgs)
        {
            bool bOut = CommandRunner.Execute(_logger, "BBCOVER.EXE", bbcoverArgs, IgnorableWarnings == null ? null : IgnorableWarnings.Split(','), "MagCore: Error MAG");

            if ( bOut && certificateFile != null)
            {
                bOut = CommandRunner.Execute(_logger, "SN.EXE", String.Format("-Ra {0} {1}", binaryFile.FullName, certificateFile.FullName), null, null);
            }

            return bOut;
        }

    }
}
