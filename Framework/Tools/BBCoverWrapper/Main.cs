////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;

using Microsoft.SPOT.Tasks.Internal;

namespace Microsoft.SPOT.Tools
{
    class BBCoverWrapper
    {
        private class MyLogger : MiniLogger
        {
            public MyLogger()
            {
            }

            public override void Say(String msg)
            {
                Console.WriteLine(msg);
            }

            public override void Warn(String msg)
            {
                Console.WriteLine("BBCOVER: Warning: " + msg);
            }

            public override void Complain(String msg)
            {
                Console.WriteLine("BBCOVER: ERROR: " + msg);
            }

            public override void Complain(Exception e)
            {
                Console.WriteLine("BBCOVER: ERROR: " + e.ToString());
            }

            public override void CommandLine(String cmdLine)
            {
                Console.WriteLine(cmdLine);
            }
        }

        MyLogger _logger;
        BBCover _bbcoverTask;

        BBCoverWrapper()
        {
            _logger = new MyLogger();
            _bbcoverTask = new BBCover(_logger);
        }

        void Usage()
        {
            _logger.Say(
                "BBCoverWrapper <options>:\n"
                + "\n Required arguments:\n\n"
                + "  /BinaryFile:<binaryFile>   : The file to instrument\n"
                + "  /OriginalsDirectory:<dir>  : Where to put the uninstrumented original files\n"
                + "  /BuildID:<id>              : Build ID or so-called \"custom version\" string\n"
                + "\n Optional arguments\n\n"
                + "  /Verbose                   : Be noisy.\n"
                + "  /DB:<info>                 : Database connection info\n"
                + "  /CertificateFile:<file>    : Signing key file\n"
                + "  /InstrumentationMode:(UserMode | KernelMode | NoImportDLLMode | Default)\n"
                + "                               Overrides automatic binary type detection and specifies\n"
                + "                               what the target is.\n"
                + "  /IgnorableWarnings         : Semi-colon-separated list of BBCover warnings that will\n"
                + "                               not cause this command to report failure if they occur\n"
                + "  /CmdFile:<BBTF file>       : File containing BBTF directives\n"
                + "  /IgnoreAsm                 : Ignore any code blocks generated from .asm source code\n"
                + "  /OmitFixupRecords          : Omit fixup records when emitting instrumented image\n"
                + "  /OptimizeArcProbes         : Reduces the number of probes inserted into the binary \n"
                + "                               by analyzing which arcs can actually be inferred as hit\n"
                + "                               when certain probed arcs are hit. Average of 40% probe\n"
                + "                               reduction and 20% smaller file. Not supported for x64.\n"
                + "  /RelationalOperatorCoverage: like BBCover 'relop'. Instruments for Relational Operator\n"
                + "                               Coverage in addition to other instrumentation. This cover-\n"
                + "                               age will not be stored to the database, but reports can be\n"
                + "                               produced with BBCovRpt.exe using the /RelopReport option.\n"
                + "                               Not supported for x64 binaries.\n"
                + "  /VulcanFriendly            : Assure that the output binary can be processed further\n"
                + "                               by other Vulcan-based tools\n"
                + "  /SessionSpaceBinary        : like BBCover '/TsGlobal'. Specify that binary is a session-\n"
                + "                               space binary like video drivers\n"
                + "  /EntryFunction             : Rather than instrumenting the binary's normal entry point\n"
                + "                               to notify the coverage monitoring service when the binary\n"
                + "                               is loaded, instrument this function to perform the notifi-\n"
                + "                               cation.\n"
                + "  /UnloadFunction            : Instrument this function to notify the coverage monitoring\n"
                + "                               service when the binary is unloaded so that its coverage\n"
                + "                               can be saved.\n"
                + "  /NoHookUnload              : For /KernelMode instrumentation, do not insert code to hook\n"
                + "                               the DriverObject->DriverUnload routine.\n"
                + "  /RegistrationHook:(Entry | Exports | Internal | None | Default)\n"
                + "                               Adds registration code to functions to notify the\n"
                + "                               coverage monitoring service when the binary is loaded.\n"
                + "        Entry   : entrypoint function - ie: mainCRTStartup\n"
                + "        Exports : export functions\n"
                + "        All     : all functions - entrypoint, exports, internal functions\n\n"
                + "        Default : entrypoint and exports for native binaries; All for mixed-mode\n\n"
                );
        }

        bool Process( string[] args )
        {
            if(args.Length == 0)
            {
                Usage();
                return false;
            }

            foreach (string a in args)
            {
                string[] tokens = a.Split(new Char[]{':'}, 2);

                switch (tokens.Length)
                {
                    case 1:
                        switch (tokens[0].ToLower())
                        {
                            case "/verbose":
                                _bbcoverTask.Verbose = true;
                                break;

                            case "/ignoreasm":
                                _bbcoverTask.IgnoreAsm = true;
                                break;

                            case "/nohookunload":
                                _bbcoverTask.NoHookUnload = true;
                                break;

                            case "/omitfixuprecords":
                                _bbcoverTask.OmitFixupRecords = true;
                                break;

                            case "/optimizearcprobes":
                                _bbcoverTask.OptimizeArcProbes = true;
                                break;

                            case "/relationaloperatorcoverage":
                                _bbcoverTask.RelationalOperatorCoverage = true;
                                break;

                            case "/rereadable":
                                _bbcoverTask.Rereadable = true;
                                break;

                            case "/vulcanfriendly":
                                _bbcoverTask.VulcanFriendly = true;
                                break;

                            case "/sessionspacebinary":
                                _bbcoverTask.SessionSpaceBinary = true;
                                break;

                            default:
                                throw new Exception(string.Format("BBCoverWrapper syntax error: invalid argument \"{0}\".", a));
                        }
                        break;

                    case 2:
                        string value = tokens[1];
                        switch (tokens[0].ToLower())
                        {
                            case "/binaryfile":
                                _bbcoverTask.BinaryFile = value;
                                break;

                            case "/originalsdirectory":
                                _bbcoverTask.OriginalsDirectory = value;
                                break;

                            case "/buildid":
                                _bbcoverTask.BuildID = value;
                                break;

                            case "/certificatefile":
                                _bbcoverTask.CertificateFile = value;
                                break;

                            case "/db":
                                _bbcoverTask.DatabaseInfo = value;
                                break;

                            case "/cmdfile":
                                _bbcoverTask.BbtfCmdFile = value;
                                break;

                            case "/entryfunction":
                                _bbcoverTask.EntryFunction = value;
                                break;

                            case "/unloadfunction":
                                _bbcoverTask.UnloadFunction = value;
                                break;

                            case "/ignorablewarnings":
                                _bbcoverTask.IgnorableWarnings = value;
                                break;

                            case "/instrumentationmode":
                                switch (value.ToLower())
                                {
                                    case "usermode":
                                        _bbcoverTask.InstrumentationMode = BBCover.InstrumentationModeKind.UserMode;
                                        break;

                                    case "kernelmode":
                                        _bbcoverTask.InstrumentationMode = BBCover.InstrumentationModeKind.KernelMode;
                                        break;

                                    case "noimportdllmode":
                                        _bbcoverTask.InstrumentationMode = BBCover.InstrumentationModeKind.NoImportDLLMode;
                                        break;

                                    case "default":
                                        _bbcoverTask.InstrumentationMode = BBCover.InstrumentationModeKind.Default;
                                        break;

                                    default:
                                        throw new Exception("BBCoverWrapper error: InstrumentationMode argument \"" + a + "\" is not one of UserMode, KernelMode, NoImportDLLMode, or Default.");
                                }
                                break;


                            case "/registrationhook":
                                switch (value.ToLower())
                                {
                                    case "default":
                                        _bbcoverTask.RegistrationHook = BBCover.RegistrationHookLocaleKind.Default;
                                        break;

                                    case "entry":
                                        _bbcoverTask.RegistrationHook = BBCover.RegistrationHookLocaleKind.Entry;
                                        break;

                                    case "exports":
                                        _bbcoverTask.RegistrationHook = BBCover.RegistrationHookLocaleKind.Exports;
                                        break;

                                    case "internal":
                                        _bbcoverTask.RegistrationHook = BBCover.RegistrationHookLocaleKind.Internal;
                                        break;

                                    case "none":
                                        _bbcoverTask.RegistrationHook = BBCover.RegistrationHookLocaleKind.None;
                                        break;

                                    default:
                                        throw new Exception("BBCoverWrapper error: RegistrationHook argument \"" + a + "\" is not one of aaa, bbb, or ccc");
                                }
                                break;


                            default:
                                throw new Exception("BBCoverWrapper error: invalid argument \"" + a + "\".");
                        }
                        break;

                    default:
                        throw new Exception("BBCoverWrapper error: invalid argument \"" + a + "\".");
                }

            }

            return _bbcoverTask.Execute();
        }

        //--//

        static int Main( string[] args )
        {
            try
            {
                BBCoverWrapper wrapper = new BBCoverWrapper();

                return wrapper.Process( args ) ? 0 : 1;
            }
            catch(Exception e)
            {
                Console.WriteLine( "{0}", e.ToString() );
                return 2;
            }
        }

    }
}
