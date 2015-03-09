using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using _DBG = Microsoft.SPOT.Debugger;

namespace Microsoft.NetMicroFramework.Tools.MFProfilerTool
{
    public class EmulatorLauncher
    {
        static string FullPathToPe(string file)
        {
            // Given a base path, and a file name which may be any of a simple file, a relative path, or an absolute path
            // return an absolute path to the PE file corresponding.

            string pePath = Path.ChangeExtension(file, "pe");
            //Make this work with our build system:
            if (!File.Exists(pePath))
            {
                pePath = Path.GetFullPath(Path.Combine(Path.Combine(Path.GetDirectoryName(file), @"le"), Path.GetFileName(pePath)));

                if(!File.Exists(pePath))
                {
                    pePath = Path.GetFullPath(Path.Combine(Path.Combine(Path.GetDirectoryName(file), @"..\pe\le"), Path.GetFileName(pePath)));

                    if (!File.Exists(pePath)) return null;
                }
            }

            return pePath;
        }

        static string FindFileInPaths(string file, ArrayList paths)
        {
            //Do we or don't we want to search for files with same name if given absolute path that doesn't exist?
            if (Path.IsPathRooted(file))
            {
                if (File.Exists(file)) { return file; }
                return null;
            }

            foreach (string path in paths)
            {
                string fullpath = Path.Combine(path, file);
                string lePath = Path.Combine(Path.Combine(path, "LE"), file);

                if (File.Exists(fullpath))
                {
                    return fullpath;
                }
                else if (File.Exists(lePath))
                {
                    return lePath;
                }
            }

            return null;
        }

        static string[] GetFilesToLoad(string exeToLoad)
        {
            _DBG.PlatformInfo pi = new Microsoft.SPOT.Debugger.PlatformInfo(null);
            ArrayList searchPaths = new ArrayList();

            //Goals: build a tree of referenced assemblies.
            //Traverse the tree post-order wise such that all assemblies loaded 

            ArrayList listFound = new ArrayList(); // list of Assembly names to avoid dups and an endless loop
            ArrayList results = new ArrayList();   // ArrayList of Info objects
            Stack stack = new Stack(); // stack of Assembly Info objects

            //Should file passed on command-line be searched in AssemblyFolders if not found in current directory?
            string fileName = Path.GetFullPath(exeToLoad);

            //First search the system assemblies, application path may have copies
            searchPaths.AddRange(pi.AssemblyFolders);
            searchPaths.Add(Path.GetDirectoryName(fileName));

            /* If the file passed isn't a .NET assembly, this next line throws an exception. */
            Assembly a;
            try
            {
                a = Assembly.LoadFile(fileName);   //Always pass full path to LoadFile so it doesn't try to use GAC?
                stack.Push(a.GetName());
                listFound.Add(a.GetName().ToString());
            }
            catch (Exception e)
            {
                throw new ApplicationException("Error: The application selected is not a .NET application.", e);
            }

            while (stack.Count > 0)
            {
                AssemblyName an = (AssemblyName)stack.Pop(); // get next assembly info
                string dllFile;


                //Does .NET check all search dirs for .dll first then all dirs for .exe or does it
                //search each dir for .dll then .exe, or one of those two with .exe first?
                dllFile = FindFileInPaths(an.Name + ".exe", searchPaths);
                if (!File.Exists(dllFile))
                {
                    dllFile = FindFileInPaths(an.Name + ".dll", searchPaths);
                    if (!File.Exists(dllFile))
                    {
                        throw new FileNotFoundException("File: " + dllFile);
                    }
                }

                string peFile = FullPathToPe(dllFile);
                if (string.IsNullOrEmpty(peFile))
                {
                    throw new ApplicationException(
                        String.Format("Cannot find PE file for {0}; File is most likely not a .NET Micro Framework assembly.",
                                      dllFile));
                }
                results.Add(peFile);

                AssemblyName[] subchild = Assembly.LoadFile(dllFile).GetReferencedAssemblies();
                for (int i = 0; i < subchild.Length; i++)
                {
                    if (!listFound.Contains(subchild[i].ToString()))
                    {
                        listFound.Add(subchild[i].ToString());
                        stack.Push(subchild[i]);
                    }
                }
            }

            return (string[])results.ToArray(typeof(string));

        }

        public static Process LaunchEmulator(_DBG.PortDefinition_Emulator pd, bool fWaitForDebugger, string program)
        {
            _DBG.PlatformInfo pi = new _DBG.PlatformInfo(null);
            _DBG.PlatformInfo.Emulator emu = pi.FindEmulator(pd.Port);

            _DBG.CommandLineBuilder cb = new _DBG.CommandLineBuilder();

            if (emu == null) { throw new ArgumentException(); }
            if (emu.legacyCommandLine) { throw new NotSupportedException("Legacy emulators not supported."); }

            if (!string.IsNullOrEmpty(emu.additionalOptions))
            {
                _DBG.CommandLineBuilder cbT = new _DBG.CommandLineBuilder(emu.additionalOptions);
                cb.AddArguments(cbT.Arguments);
            }

            if (!string.IsNullOrEmpty(emu.config))
            {
                cb.AddArguments("/config:" + emu.config);
            }

            if (fWaitForDebugger)
            {
                cb.AddArguments("/waitfordebugger");
            }

            string[] files = EmulatorLauncher.GetFilesToLoad(program);
            foreach (string pe in files)
            {
                cb.AddArguments("/load:" + pe);
            }

            string args = "";
            args = args.Trim();
            if (args.Length > 0)
            {
                cb.AddArguments("/commandlinearguments:" + args);
            }

            string commandLine = cb.ToString();
            commandLine = Environment.ExpandEnvironmentVariables(commandLine);

            Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = emu.application;
            p.StartInfo.Arguments = commandLine;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.WorkingDirectory = Path.GetDirectoryName(emu.application);

            try
            {
                p.Start();
            }
            catch (System.ComponentModel.Win32Exception we)
            {
                MessageBox.Show(string.Format("Failed to launch emulator: {0}", we.NativeErrorCode),
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            return p;
        }
    }
}
