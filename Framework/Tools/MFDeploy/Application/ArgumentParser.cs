//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
    internal class ArgumentParser
    {
        private CommandMap[]        m_commandMap;
        private InterfaceMap[]      m_interfaceMap;
        private MFPortDefinition[]  m_transports  = new MFPortDefinition[2];
        private Commands            m_cmd         = (Commands)(-1);
        private string[]            m_flashFiles  = new string[0];
        private bool                m_fWarmReboot = false;

        internal MFPortDefinition   Interface       { get { return m_transports[0]; } }
        internal MFPortDefinition   TinybtrInterface{ get { return m_transports[1]; } }
        internal Commands           Command         { get { return m_cmd;           } }

        internal enum Commands
        {
            Erase,
            Deploy,
            Ping,
            Reboot,
            List,
            Help,
        }

        internal class CommandMap
        {
            internal CommandMap(Commands cmd, string cmdArgs, string hlp)
            {
                Command        = cmd;
                CommandArgExpr = cmdArgs;
                HelpString     = hlp;
            }
            internal Commands Command;
            internal string   CommandArgExpr;
            internal string   HelpString;
        }

        internal class InterfaceMap
        {
            internal InterfaceMap(TransportType itf, string itfArgs, string itfHelp)
            {
                Interface        = itf;
                InterfaceArgExpr = itfArgs;
                InterfaceHelp    = itfHelp;
            }

            internal TransportType Interface;
            internal string     InterfaceArgExpr;
            internal string     InterfaceHelp;
        }

        internal ArgumentParser()
        {
            string c_FileDesc = "((\"[\\w\\W]+\")*|([\\\\d\\w\\._\\-\\:]+))";
            string c_MultipleFiles = c_FileDesc + "(" + FileSeparator + c_FileDesc + ")*";

            m_commandMap = new CommandMap[]{
                new CommandMap( Commands.Erase  , ""                                    , string.Format("{0,-35}{1}", "Erase"                                      , Properties.Resources.HelpCommandErase  ) ),
                new CommandMap( Commands.Deploy , ArgSeparator + c_MultipleFiles        , string.Format("{0,-35}{1}", "Deploy:<image file>[;<image file>]*"        , Properties.Resources.HelpCommandFlash  ) ),
                new CommandMap( Commands.Ping   , ""                                    , string.Format("{0,-35}{1}", "Ping"                                       , Properties.Resources.HelpCommandPing   ) ),
                new CommandMap( Commands.Reboot , "(" + ArgSeparator + @"warm" + ")?"   , string.Format("{0,-35}{1}", "Reboot[:Warm]"                              , Properties.Resources.HelpCommandReboot ) ),
                new CommandMap( Commands.List   , ""                                    , string.Format("{0,-35}{1}", "List"                                       , Properties.Resources.HelpCommandList   ) ),
                new CommandMap( Commands.Help   , ""                                    , string.Format("{0,-35}{1}", "Help"                                       , Properties.Resources.HelpCommandHelp   ) )
            };

            m_interfaceMap = new InterfaceMap[]{
                new InterfaceMap( TransportType.Serial, ArgSeparator + @"\d+"                                   , string.Format("{0,-35}{1}", "Serial:<port_num>"          , Properties.Resources.HelpInterfaceCom  ) ),
                new InterfaceMap( TransportType.USB   , ArgSeparator + @"[\W\w]+"                               , string.Format("{0,-35}{1}", "USB:<usb_name>"             , Properties.Resources.HelpInterfaceUsb  ) ),
                new InterfaceMap( TransportType.TCPIP , ArgSeparator + @"\d\d?\d?\.\d\d?\d?\.\d\d?\d?\.\d\d?\d?", string.Format("{0,-35}{1}", "TCPIP:<ip_addr>"            , Properties.Resources.HelpInterfaceTcpIp) ),
            };
        }

        internal const string FlagPrefix    = @"[/-]";
        internal const char   ArgSeparator  = ':';
        internal const char   FileSeparator = ';';

        internal string ValidCommandExpression
        {
            get
            {
                int idx = 0;
                StringBuilder expr = new StringBuilder();

                // add commands (and command args)
                expr.Append(@"(");

                for (idx = 0;idx < m_commandMap.Length;idx++)
                {
                    if (idx > 0) expr.Append("|");
                    expr.Append("(");
                    expr.Append(m_commandMap[idx].Command.ToString());
                    expr.Append(m_commandMap[idx].CommandArgExpr);
                    expr.Append(")");
                }
                expr.Append(")");

                return expr.ToString();
            }
        }

        internal string ValidInterfaceExpression
        {
            get
            {
                int idx = 0;
                StringBuilder expr = new StringBuilder();

                expr.Append(FlagPrefix);
                expr.Append('I');
                expr.Append(ArgSeparator+@"(");

                for (idx = 0;idx < m_interfaceMap.Length;idx++)
                {
                    if (idx > 0) expr.Append("|");
                    expr.Append("(");
                    expr.Append(m_interfaceMap[idx].Interface.ToString());
                    expr.Append(m_interfaceMap[idx].InterfaceArgExpr);
                    expr.Append(")");
                }
                expr.Append(@"){1}");
                
                return expr.ToString();
            }
        }

        internal bool ValidateArgs( string args, out string error )
        {
            bool ret = true;
            error = "";

            try
            {
                Regex rx = new Regex(ValidCommandExpression, RegexOptions.IgnoreCase);
                MatchCollection mc = rx.Matches(args);
                if (mc.Count != 1)
                {
                    // special case /? and /h for help commands
                    rx = new Regex(FlagPrefix + @"\?|" + FlagPrefix + "h", RegexOptions.IgnoreCase);
                    if (rx.IsMatch(args))
                    {
                        m_cmd = Commands.Help;
                    }
                    else
                    {
                        error += mc.Count == 0 ? Properties.Resources.ErrorCommandMissing : Properties.Resources.ErrorCommandsMultiple + "\r\n";
                    }
                    // don't return false, because we may be invoking the UI with a given interface
                }
                else
                {
                    if (mc[0].Groups[0].Success)
                    {
                        string[] data = mc[0].Groups[0].Value.Trim().Split(new char[] { ArgSeparator }, 2);

                        m_cmd = (Commands)Enum.Parse(typeof(Commands), data[0], true);
                        switch (m_cmd)
                        {
                            case Commands.Deploy:
                                data[1] = data[1].Replace("\"", "");
                                m_flashFiles = data[1].Trim().Split(new char[] { FileSeparator }, StringSplitOptions.RemoveEmptyEntries);
                                break;
                            case Commands.Reboot:
                                m_fWarmReboot = data.Length > 1;
                                break;
                        }
                        args = args.Replace(mc[0].Groups[0].Value, "");
                    }
                }

                if (m_cmd != Commands.Help && m_cmd != Commands.List)
                {
                    rx = new Regex(ValidInterfaceExpression, RegexOptions.IgnoreCase);
                    mc = rx.Matches(args);
                    if (mc.Count <= 0 || mc.Count > 2)
                    {
                        error = mc.Count == 0 ? Properties.Resources.ErrorInterfaceMissing : Properties.Resources.ErrorInterfaceMultiple + "\r\n";
                        ret = false;
                    }
                    else
                    {
                        for (int i = 0;i < mc.Count;i++)
                        {
                            if (mc[i].Groups[0].Success)
                            {
                                string[] data = mc[i].Groups[0].Value.Trim().Split(ArgSeparator);

                                TransportType type = (TransportType)Enum.Parse(typeof(TransportType), data[1], true);

                                switch (type)
                                {
                                    case TransportType.Serial:
                                        m_transports[i] = new MFSerialPort(@"COM" + data[2], @"\\.\COM" + data[2]);
                                        break;
                                    case TransportType.USB:
                                        m_transports[i] = new MFUsbPort(data[2]);
                                        break;
                                    case TransportType.TCPIP:
                                        m_transports[i] = new MFTcpIpPort(data[2], "");
                                        break;
                                }
                            }
                            args = args.Replace(mc[i].Groups[0].Value, "");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ret = false;
                error += e.ToString() + "\r\n";
            }

            if (m_cmd != Commands.Help && m_cmd != Commands.List && ret)
            {
                args = args.Trim();
                if (args.Length != 0)
                {
                    ret = false;
                    error = Properties.Resources.ErrorArgumentsInvalid + args;
                }
            }

            return ret;
        }
        internal void OnStatus(long current, long total, string txt)
        {
            Console.WriteLine(txt);
        }

        internal void OnDeployStatus(long current, long total, string txt)
        {
            double perc = 100.0 * ((double)current / (double)total);
            Console.Write( "\r{0}%", (int)perc );
        }

        internal void Execute()
        {
            MFDeploy deploy = new MFDeploy();
            MFDevice port   = null;

            try
            {
                switch (m_cmd)
                {
                    case Commands.Help:
                        Console.WriteLine();
                        Console.WriteLine(Properties.Resources.HelpBanner);
                        Console.WriteLine();
                        Console.WriteLine(Properties.Resources.HelpDescription);
                        Console.WriteLine();
                        Console.WriteLine(Properties.Resources.HelpUsage);
                        Console.WriteLine();
                        Console.WriteLine(Properties.Resources.HelpCommand);
                        foreach (CommandMap cm in m_commandMap)
                        {
                            Console.WriteLine("  " + cm.HelpString);
                        }
                        Console.WriteLine();
                        Console.WriteLine(Properties.Resources.HelpInterface);
                        foreach (InterfaceMap im in m_interfaceMap)
                        {
                            Console.WriteLine("  " + im.InterfaceHelp);
                        }
                        Console.WriteLine();
                        Console.WriteLine(Properties.Resources.HelpInterfaceSpecial);
                        Console.WriteLine();
                        break;
                    case Commands.List:
                        foreach (MFPortDefinition pd in deploy.DeviceList)
                        {
                            Console.WriteLine(pd.Name);
                        }
                        break;
                    case Commands.Ping:
                        Console.Write(Properties.Resources.StatusPinging);
                        try
                        {
                            port = deploy.Connect(m_transports[0]);
                            Console.WriteLine(port.Ping().ToString());
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(Properties.Resources.ErrorPrefix + e.Message);
                        }
                        break;
                    case Commands.Erase:
                        Console.Write(Properties.Resources.StatusErasing);
                        try
                        {
                            port = deploy.Connect(m_transports[0]);
                            port.OnProgress += new MFDevice.OnProgressHandler(OnStatus);
                            Console.WriteLine((port.Erase() ? Properties.Resources.ResultSuccess : Properties.Resources.ResultFailure));
                            port.OnProgress -= new MFDevice.OnProgressHandler(OnStatus);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(Properties.Resources.ErrorPrefix + e.Message);
                        }
                        break;
                    case Commands.Deploy:
                        try
                        {
                            bool fOK = false;
                            uint entrypoint = 0;

                            port = deploy.Connect(m_transports[0]);

                            foreach (string file in m_flashFiles)
                            {
                                uint entry = 0;
                                FileInfo fi = new FileInfo(file);
                                string signature_file = file;

                                if (fi.Extension != null || fi.Extension.Length > 0)
                                {
                                    int index = file.LastIndexOf(fi.Extension);
                                    signature_file = file.Remove(index, fi.Extension.Length);
                                }

                                signature_file += ".sig";

                                if (!File.Exists(file))
                                {
                                    Console.WriteLine(string.Format(Properties.Resources.ErrorFileNotFound, file));
                                    break;
                                }

                                if (!File.Exists(signature_file))
                                {
                                    Console.WriteLine(string.Format(Properties.Resources.ErrorFileNotFound, signature_file));
                                    break;
                                }

                                Console.WriteLine(string.Format(Properties.Resources.StatusFlashing, file));
                                port.OnProgress += new MFDevice.OnProgressHandler(OnDeployStatus); 
                                fOK = port.Deploy(file, signature_file, ref entry);
                                port.OnProgress -= new MFDevice.OnProgressHandler(OnDeployStatus);
                                Console.WriteLine();
                                Console.WriteLine((fOK ? Properties.Resources.ResultSuccess : Properties.Resources.ResultFailure));
                                if (entry != 0 && entrypoint == 0 || file.ToLower().Contains("\\er_flash"))
                                {
                                    entrypoint = entry;
                                }
                            }
                            if (fOK)
                            {
                                Console.WriteLine(string.Format(Properties.Resources.StatusExecuting, entrypoint));
                                port.Execute(entrypoint);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(Properties.Resources.ErrorPrefix + e.Message);
                        }
                        break;
                    case Commands.Reboot:
                        try
                        {
                            port = deploy.Connect(m_transports[0]);
                            Console.WriteLine(Properties.Resources.StatusRebooting);
                            port.Reboot(!m_fWarmReboot);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(Properties.Resources.ErrorPrefix + e.Message);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format(Properties.Resources.ErrorFailure, e.Message));
            }
            finally
            {
                if (deploy != null)
                {
                    deploy.Dispose();
                    deploy = null;
                }
            }
        }
    }
}
