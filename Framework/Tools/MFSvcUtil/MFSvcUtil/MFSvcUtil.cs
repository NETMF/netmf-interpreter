////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// 
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Ws.SvcExporter;
using Ws.SvcImporter;
using Ws.SvcUtilCodeGen;

namespace MFSvcUtil
{
    class Program
    {

        private enum TargetOutputType
        {
            None = 0,
            Wsdl = 1,
            Code = 2
        }

        /// <summary>
        /// Application used to generate Microframework data contract, data contract serializers,
        /// hosted service and client proxy source files from a Wsdl file.
        /// </summary>
        /// <param name="args">An array of command line arguments.</param>
        static void Main(string[] args)
        {
            MFSvcImporter svcImporter = new MFSvcImporter();
            MFMetadataExporter metadataExporter = new MFMetadataExporter();
            bool rpcBindingStyleSupport = false;
            bool wsiCompliant = true;
            TargetOutputType target = TargetOutputType.Wsdl;
            string refPath = null;

            if (args.Length < 1 || args.Length > 7)
            {
                DisplayUsage();
                return;
            }
            else
            {
                TargetPlatform m_targetPlatform = TargetPlatform.MicroFramework;
                Logger.Verbose = false;

                // Seperate filename parameter from the options and build a list of externally referenced schema files
                List<string> options = new List<string>();
                string filename = null;
                List<string> schemaRefs = null;
                foreach (string cmdArg in args)
                {
                    if (cmdArg[0] == '/')
                        options.Add(cmdArg);
                    else if (filename == null && (target = GetTargetType(cmdArg)) != TargetOutputType.None)
                        filename = cmdArg;
                    else if (filename == null && (target = GetTargetType(cmdArg)) != TargetOutputType.None)
                        filename = cmdArg;
                    else
                    {
                        if (cmdArg.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (schemaRefs == null)
                                schemaRefs = new List<string>();
                            schemaRefs.Add(cmdArg);
                        }
                        else
                        {
                            string pathName = Path.GetDirectoryName(cmdArg);
                            if (pathName == "")
                                pathName = Directory.GetCurrentDirectory();


                            pathName = Path.GetFullPath(pathName);
                            string refFileName = Path.GetFileName(cmdArg);
                            string[] fileList = Directory.GetFiles(pathName, refFileName);
                            if (fileList.Length == 0)
                            {
                                break;
                            }
                            foreach (string refFilename in fileList)
                            {
                                if (refFilename.EndsWith(".xsd", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    if (schemaRefs == null)
                                        schemaRefs = new List<string>();
                                    if (!schemaRefs.Contains(refFilename))
                                        schemaRefs.Add(refFilename);
                                }
                            }
                        }
                    }
                }

                // Check for minimum required parameters
                if ((target = GetTargetType(filename)) == TargetOutputType.None)
                {
                    DisplayUsage();
                    return;
                }
                else
                {
                    // Trim filename extension
                    string assemblyFilename = target == TargetOutputType.Code ? filename : null;
                    string wsdlFilename = target == TargetOutputType.Wsdl ? filename : null;

                    filename = Path.GetFileNameWithoutExtension(filename);
                    string DestinationDirectory = "";
                    string outOverride = null;

                    // Check for random arguments
                    foreach (string option in options)
                    {
                        string[] arg = option.Split(':');
                        if (arg.Length > 2)
                            for (int i = 2; i < arg.Length; ++i)
                                arg[1] = arg[1] + ":" + arg[i];
                        switch (arg[0].ToLower())
                        {
                            case "/platform":
                            case "/p":
                                if (arg[1] == "win32")
                                    m_targetPlatform = TargetPlatform.Win32;
                                break;
                            case "/verbose":
                            case "/v":
                                Logger.Verbose = true;
                                break;
                            case "/out":
                            case "/o":
                                outOverride = arg[1];
                                break;
                            case "/directory":
                            case "/d":
                                DestinationDirectory = arg[1];
                                break;
                            case "/reference":
                            case "/r":
                                refPath = arg[1];
                                break;
                            case "/rpcbindingstyle":
                                rpcBindingStyleSupport = true;
                                break;
                            case "/wsicompliant":
                                if (arg[1] == "false")
                                    wsiCompliant = false;
                                break;
                            default:
                                DisplayUsage();
                                return;
                        }
                    }

                    if (DestinationDirectory != "" && DestinationDirectory[DestinationDirectory.Length - 1] != '\\')
                        DestinationDirectory += "\\";
                    filename = outOverride == null ? filename : outOverride;
                    string ContractFilename = filename + ".cs";
                    ContractFilename = DestinationDirectory + ((ContractFilename.LastIndexOf(".cs") == ContractFilename.Length - 3) ? ContractFilename : ContractFilename + ".cs");
                    string HostedServiceFilename = DestinationDirectory + filename + "HostedService.cs";
                    string ClientProxyFilename = DestinationDirectory + filename + "ClientProxy.cs";
                    try
                    {
                        svcImporter.WsiCompliant = wsiCompliant;
                        svcImporter.RpcBindingStyleSupport = rpcBindingStyleSupport;
                        Logger.WriteLine("MfSvcUtil.exe (c) Microsoft 2008", LogLevel.Normal);
                        Logger.WriteLine("", LogLevel.Normal);
                        if (target == TargetOutputType.Wsdl)
                        {
                            svcImporter.ParseWsdl(wsdlFilename, schemaRefs);
                            svcImporter.CreateSourceFiles(ContractFilename, HostedServiceFilename, ClientProxyFilename, m_targetPlatform);
                        }
                        else
                        {
                            metadataExporter.ParseCode(assemblyFilename, refPath, outOverride, null, DestinationDirectory, false);
                        }
                        Logger.WriteLine("Processing complete.", LogLevel.Normal);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Program Error: " + e.Message);
                        Console.WriteLine();
                        DisplayUsage();
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Displays the application usage message.
        /// </summary>
        private static void DisplayUsage()
        {
            Console.WriteLine();
            Console.WriteLine(".Net Micro Framework Dpws Source Code Utility");
            Console.WriteLine("Copyright (c) Microsoft Corporation.  All rights reserved.");
            Console.WriteLine();
            Console.WriteLine("Uses:");
            Console.WriteLine("Generate C# .Net Micro Framework DPWS source code from a wsdl.");
            Console.WriteLine("Generate a wsdl from an attributed assembly.");
            Console.WriteLine();
            Console.WriteLine("*** Generate code from Wsdl ***");
            Console.WriteLine("Usage: MFSvcUtil [WsdlFilename] [SchemaFileReferences] Options");
            Console.WriteLine("    Where [WsdlFilename] = A valid WSDL path\\filename.");
            Console.WriteLine("    Where [SchemaFileReferences] = Optional path to external schema");
            Console.WriteLine("       file reference(s) (xsd)");
            Console.WriteLine("    Where Options =");
            Console.WriteLine("      /D:[DestinationDirectory] or /Directory:[DestinationDirectory] to");
            Console.WriteLine("         specify the name of a directory where output files are generated.");
            Console.WriteLine("      /O:[ContractSourceName] or /Out:[ContractSourceName] to specify the");
            Console.WriteLine("         name of generated source file(s). Note: The name is used as the");
            Console.WriteLine("         contract source file name and as the prefix for Hosted Service and");
            Console.WriteLine("         Client Proxy source file names. If not specified the name of the");
            Console.WriteLine("         wsdl file is used.");
            Console.WriteLine("      /P:[TargetPlatform] or /Platform:[TargetPlatform] to specify");
            Console.WriteLine("         Microframework or Win32 target runtime platform.");
            Console.WriteLine("      /V or /Verbose to display extended information.");
            Console.WriteLine();
            Console.WriteLine("*** Generate a wsdl from an assembly ***");
            Console.WriteLine("Usage: MFSvcUtil [AssemblyFilename] Options");
            Console.WriteLine("    Where [AssemblyFilename] = A valid assembly path\\filename.");
            Console.WriteLine("    Where Options =");
            Console.WriteLine("      /D:[DestinationDirectory] or /Directory:[DestinationDirectory] to");
            Console.WriteLine("         specify the name of a directory where the output files are stored.");
            Console.WriteLine("      /O:[Filename] or /Out:[Filename] to specify the name of the Wsdl");
            Console.WriteLine("         file generated. The default filename is the name of the assembly that");
            Console.WriteLine("         contains the contract used to generate the wsdl.");
            Console.WriteLine("      /R:[ReferencesPath] or /Reference:[ReferencesPath] to specify");
            Console.WriteLine("         a directory used to resolve external type references.");
            Console.WriteLine("      /V or /Verbose to display extended information.");
        }

        /// <summary>
        /// Determine the type of output to produce based a filename extension.
        /// </summary>
        /// <param name="filename">A string containing the filename command line parameter.</param>
        /// <returns>Code for .dll or .exe extensions, Wsdl for .wsdl extensions or None if unsupported.</returns>
        private static TargetOutputType GetTargetType(string filename)
        {
            TargetOutputType target = TargetOutputType.None;

            if (filename != null)
            {
                if (filename.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase))
                {
                    target = TargetOutputType.Wsdl;
                }
                else
                {
                    // Determine the target type based on the file name extension
                    string ext = Path.GetExtension(filename);

                    switch (ext)
                    {
                        case ".wsdl":
                            target = TargetOutputType.Wsdl;
                            break;
                        case ".exe":
                        case ".dll":
                            target = TargetOutputType.Code;
                            break;
                    }
                }
            }
            return target;
        }
    }
}
