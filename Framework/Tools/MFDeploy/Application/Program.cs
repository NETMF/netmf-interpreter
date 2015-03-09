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
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;

[assembly: ComVisible(false)]
[assembly: CLSCompliantAttribute(false)]

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
    internal class NativeMethods
    {
        private NativeMethods()
        {
        }

        private const String KERNEL32 = "kernel32.dll";
        internal const uint ATTACH_PARENT_PROCESS = 0x0ffffffff;

        [DllImport(KERNEL32, SetLastError = true, ExactSpelling = true)]
        internal static extern bool AttachConsole(uint dwProcessId);

        [DllImport(KERNEL32, SetLastError = true, ExactSpelling = true)]
        internal static extern bool FreeConsole();

        [DllImport(KERNEL32, SetLastError = true, ExactSpelling = true)]
        internal static extern bool AllocConsole();

    }

    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(params string[] args)
        {
            bool fRunUI = true;
            MFPortDefinition transport = null;
            MFPortDefinition transportTinyBooter = null;

            if (args.Length > 0)
            {
                try
                {
                    string error;
                    string all_args = NormalizeArgs( args );

                    ArgumentParser parser = new ArgumentParser();
                    // error condition
                    if (!parser.ValidateArgs(all_args, out error))
                    {
                        fRunUI = false;
                        NativeMethods.AllocConsole( );
                        Console.WriteLine(Properties.Resources.ErrorPrefix + error);
                    }
                    else if (parser.Command != (ArgumentParser.Commands)(-1))
                    {   // valid command, no UI
                        fRunUI = false;
                        NativeMethods.AllocConsole( );
                        parser.Execute();
                    }
                    else if (parser.Interface != null && parser.Interface.Transport != (TransportType)(-1))
                    {    // no command, but interface defined, so update UI transport field
                        transport = parser.Interface;
                        transportTinyBooter = parser.TinybtrInterface;
                    }
                }
                catch (Exception e)
                {
                    NativeMethods.AllocConsole( );
                    Console.WriteLine(Properties.Resources.ErrorPrefix + e.Message);
                }
            }

            if (fRunUI)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Form1 form = new Form1();
                form.Transport = transport;
                form.TransportTinyBooter = transportTinyBooter;
                Application.Run(form);
            }
        }

        private static string NormalizeArgs( string[ ] args )
        {
            StringBuilder sb = new StringBuilder( );
            string all_args = "";


            foreach( string arg in args )
            {
                // take care of spaces in path names
                if( arg.ToUpper( ).StartsWith( ArgumentParser.Commands.Deploy.ToString( ).ToUpper( ) + ArgumentParser.ArgSeparator ) )
                {
                    string tmp = arg;
                    // place double quoutes around files
                    tmp = tmp.Replace( ArgumentParser.ArgSeparator.ToString( ), ArgumentParser.ArgSeparator + "\"" );
                    tmp = tmp.Replace( ArgumentParser.FileSeparator.ToString( ), "\"" + ArgumentParser.FileSeparator + "\"" );
                    sb.AppendFormat( "{0}\"", tmp );
                }
                else
                {
                    sb.AppendFormat( "{0} ", arg );
                }
            }
            all_args = sb.ToString( ).Trim( );
            return all_args;
        }
    }
}