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

    //--------------------------------------------------
    //  BASE CLASS OF THE LINK TASK....
    //
    //  This is just a wrapper for the link tool, so that
    //  we can keep the environment variables setup within
    //  the MSBuild system.  When performing the <EXEC task,
    //  environment information is lost by the child process,
    //  launched through "cmd.exe".  Custom tasks however, keep
    //  the environment setup.
    //
    public abstract class LinkTask : ToolTask
    {

        protected enum eTargetType { DLL, LIBRARY, EXE }

        //  ACCESSORS SETTING/GETTING THE LINKER SETTINGS
        //----------------------------------------------------------
        //

        protected string mToolPath = null;
        public string TOOLPATH
        {
            get { return mToolPath; }
            set { mToolPath = value; }
        }


        //  Linker /DEBUG flag...
        protected bool debug = true;
        public bool DEBUG
        {
            get { return debug; }
            set { debug = value; }
        }

        protected string linkIgnore = string.Empty;
        public string IGNORE
        {
            get { return linkIgnore; }
            set { linkIgnore = value; }
        }

        //  Linker /DEFAULTLIB variables
        protected ITaskItem[] deflibs;
        public ITaskItem[] DEFLIBS
        {
            get { return deflibs;}
            set { deflibs = value;}
        }


        //  Linker /DELAYLOAD libraries
        protected ITaskItem[] delayload = null;
        public ITaskItem[] DELAYLOAD
        {
            get { return delayload;}
            set { delayload = value;}
        }

        // Linker /IMPLIB:filename
        protected ITaskItem[] implib = null;
        public ITaskItem[] IMPLIB
        {
            get { return implib;}
            set { implib = value;}
        }


        //  Linker /INCREMENTAL flag...only available on W32 platforms
        //  Throws "unknown option" on CE builds
        protected bool incremental = false;
        public bool INCREMENTAL
        {
            get { return incremental; }
            set { incremental = value; }
        }



        //  Linker /LIBPATH:path variable
        protected ITaskItem[] libpath = null;
        public ITaskItem[] LIBPATH
        {
            set {libpath = value;}
        }



        //  Linker /MACHINE flag
        protected string machine = "X86";   //  default
        public string MACHINE
        {
            get { return machine; }
            set { machine = value; }
        }



        //  Linker /MANIFEST flag
        protected bool manifest = false;
        public bool MANIFEST
        {
            get { return manifest; }
            set { manifest = value; }
        }




        //  Linker /MANIFESTFILE
        protected ITaskItem manifestfile;
        public ITaskItem MANIFESTFILE
        {
            get { return manifestfile; }
            set {
                    manifestfile = value;
                    manifest = true;
                }
        }



        //  Linker /DELAYSIGN flag
        protected bool delaySign = false;
        public bool DELAYSIGN
        {
            get { return delaySign; }
            set { delaySign = value; }
        }

        //  Linker /KEYFILE
        protected ITaskItem keyFile;
        public ITaskItem KEYFILE
        {
            get { return keyFile; }
            set {
                    keyFile = value;
                }
        }



        //  Linker /NODEFAULTLIB:lib variable
        protected ITaskItem[] nodefaultlib = null;
        public ITaskItem[] NODEFAULTLIB
        {
            get { return nodefaultlib;}
            set {nodefaultlib = value;}
        }


        //  Linker /OPT:.. variables
        protected ITaskItem[] opt = null;
        public ITaskItem[] OPT
        {
            get { return opt;}
            set {opt = value;}
        }


        //  Linker /OUT:filename variable
        protected string outfilename;
        [Required]
        public string OUT
        {
            get { return outfilename;}
            set { outfilename = value;}
        }


        //  Linker /PDB:filename variable
        protected string pdb = null;
        public string PDB
        {
            get { return pdb; }
            set { pdb = value; }
        }


        //  Linker /SUBSYSTEM:
        protected string subsystem = "WINDOWS";
        public string SUBSYSTEM
        {
            get { return subsystem; }
            set { subsystem = value; }
        }



        //  Linker object files to include....
        protected ITaskItem[] objfiles = null;
        [Required]
        public ITaskItem[] OBJFILES
        {
            set { objfiles = value; }
        }

        protected eTargetType mTargetType;
        public string TARGETTYPE
        {
            get
            {
                try
                {
                    return mTargetType.ToString();
                }
                catch (Exception )
                {
                    return "UNDEFINED";
                }
            }

            set
            {
                string v = value.ToUpper();

                if (v == "DLL")
                    mTargetType = eTargetType.DLL;
                else if (v == "LIB" || v == "LIBRARY")
                    mTargetType = eTargetType.LIBRARY;
                else if (v == "EXE" || v == "PROGRAM")
                    mTargetType = eTargetType.EXE;
                else
                    throw new Exception(String.Format("Unknown link type \"{0}\"", value));
            }
        }

        protected ITaskItem moduleDefinitionFile = null;
        public ITaskItem ModuleDefinitionFile
        {
            get { return moduleDefinitionFile;  }
            set { moduleDefinitionFile = value; }
        }



        //  METHODS
        //-------------------------------------------------------------------------

        protected LinkTask (eTargetType tgtType)
        {
            mTargetType = tgtType;
        }


        #region ToolTask Members
        protected override string ToolName
        {
            get
            {
                switch (mTargetType)
                {
                    case eTargetType.DLL:
                    case eTargetType.EXE:
                        return "LINK.EXE";
                    case eTargetType.LIBRARY:
                        return "LIB.EXE";
                    default:
                        throw new Exception("TargetType accessed before being initialized");
                }
            }
        }

        protected string Extension
        {
            get
            {
                switch (mTargetType)
                {
                    case eTargetType.DLL:
                        return ".DLL";
                    case eTargetType.EXE:
                        return ".EXE";
                    case eTargetType.LIBRARY:
                        return ".LIB";
                    default:
                        throw new Exception("TargetType accessed before being initialized");
                }
            }
        }

        /// <summary>
        /// The VS Linker should be pointed to by the env variable VSINSTALLDIR,
        /// in the VC\bin subdirectory
        /// </summary>
        /// <returns></returns>
        protected override string GenerateFullPathToTool()
        {
            if (mToolPath != null)
            {
                return mToolPath;
            }

            String VSINSTALLDIR = Environment.GetEnvironmentVariable("VSINSTALLDIR");
            if (VSINSTALLDIR != null)
            {
                return Path.Combine(Path.Combine(Path.Combine(VSINSTALLDIR, "VC"), "bin"), ToolName);
            }
            throw new Exception("LINK task does not know where to find linker tool");
        }

        /// <summary>
        /// Construct the command line from the task properties by using the CommandLineBuilder
        /// </summary>
        /// <returns></returns>
        protected override string GenerateCommandLineCommands()
        {
            CommandLineBuilder builder = new CommandLineBuilder();

            builder.AppendSwitch("/nologo");
            builder.AppendSwitchIfNotNull("/ERRORREPORT:", "PROMPT");

            if ( mTargetType == eTargetType.DLL || mTargetType == eTargetType.EXE )
            {
                builder.AppendSwitch("/NXCOMPAT");
                builder.AppendSwitch("/DYNAMICBASE");
            }

            if (debug)
            {
                builder.AppendSwitch("/DEBUG");
                builder.AppendSwitch("/ASSEMBLYDEBUG");
            }

            if( !string.IsNullOrWhiteSpace( linkIgnore ) )
            {
                builder.AppendSwitch( string.Format( "/IGNORE:{0}", linkIgnore ) );
            }

            if (null != DEFLIBS)
            {
                //  Specify the libs we Do want to add
                foreach (ITaskItem toAdd in DEFLIBS)
                {
                    builder.AppendSwitchIfNotNull("/DEFAULTLIB:", toAdd);
                }
            }

            //  /DELAYLOAD files
            if (delayload != null)
            {
                //  Find the last index of the import libs...
                //  Insert also "Delayimp.lib" into this set as well
                //  This is required to make the /DELAYLOAD work properly
                //

                foreach (ITaskItem toAdd in delayload)
                {
                    builder.AppendSwitchIfNotNull("/DELAYLOAD:", toAdd);
                }
            }

            if (libpath != null)
            {
                foreach (ITaskItem toAdd in libpath)
                {
                    builder.AppendSwitchIfNotNull("/LIBPATH:", toAdd);
                }
            }

            //  /DLL
            if (mTargetType == eTargetType.DLL)
            {
                builder.AppendSwitch("/DLL");
            }

            if( mTargetType == eTargetType.EXE )
            {
                builder.AppendSwitch( "/MAP" );
            }

            //  /IMPLIB:filenames...
            if (implib != null)
            {
                foreach (ITaskItem toAdd in implib)
                {
                    builder.AppendSwitchIfNotNull("/IMPLIB:", toAdd);
                }
            }

            //  /MANIFEST and /MANIFESTFILE
            if (manifest)
            {
                builder.AppendSwitch("/MANIFEST");
                builder.AppendSwitchIfNotNull("/MANIFESTFILE:", manifestfile);
            }

            //  /KEYFILE
            builder.AppendSwitchIfNotNull("/KEYFILE:", keyFile);

            // /DELAYSIGN
            if (delaySign)
            {
                builder.AppendSwitch("/DELAYSIGN");
            }

            // MACHINE
            builder.AppendSwitchIfNotNull("/MACHINE:", machine);

            //  Specify the libs we DONT want
            if (nodefaultlib != null)
            {
                foreach (ITaskItem toAdd in nodefaultlib)
                {
                    builder.AppendSwitchIfNotNull("/NODEFAULTLIB:", toAdd.ItemSpec + ".lib");
                }
            }

            //  Specify the subsystem
            builder.AppendSwitchIfNotNull("/SUBSYSTEM:", subsystem);

            //  Add the OBJ files
            if (objfiles != null)
            {
                foreach (ITaskItem toAdd in objfiles)
                {
                    builder.AppendSwitch(toAdd.ItemSpec);
                }
            }

            //  Specify the PDB file if set...
            builder.AppendSwitchIfNotNull("/PDB:", pdb);

            // Specify the DEF file if set...
            builder.AppendSwitchIfNotNull("/DEF:", moduleDefinitionFile);

            //  Specify the output file name. We put this last to make it visually easier to find the
            //  name of the output when reviewing build log files.
            builder.AppendSwitchIfNotNull("/OUT:", outfilename);

            Log.LogMessage(MessageImportance.High, "Linking {0}", outfilename);

            return builder.ToString();
        }
        #endregion

    }



    //---------------------------------------------------------------------------------
    //  Create an EXE
    public class VSLinkExeTask : LinkTask
    {
        public VSLinkExeTask()
            : base(eTargetType.EXE)
        { }

    }


    //---------------------------------------------------------------------------------
    //  Create a LIB
    public class VSLinkLibTask : LinkTask
    {
        public VSLinkLibTask()
            : base(eTargetType.LIBRARY)
        { }

    }

    //---------------------------------------------------------------------------------
    //  Create a DLL
    public class VSLinkDllTask : LinkTask
    {
        public VSLinkDllTask()
            : base(eTargetType.DLL)
        {
        }
    }

}
