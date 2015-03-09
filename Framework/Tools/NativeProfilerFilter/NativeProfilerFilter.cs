using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Runtime;

namespace NativeProfilerFilter
{
    public class NativeProfilerFilter
    {
        private string currentMacro;
        private string rootDirectory;
        private Hashtable fileNames;
        private Hashtable macroLevels;
        public NativeProfilerFilter(string filterFileName)
        {
            File.Delete("do_sd_revert.bat");
            TextReader m_input = null;
            m_input = File.OpenText(filterFileName);
            string sBuffer = null;
            string clientDirectory = System.Environment.GetEnvironmentVariable("SPOCLIENT") + "\\";
            this.rootDirectory = System.Environment.GetEnvironmentVariable("SPOROOT") + "\\";
            macroLevels = new Hashtable();

            while ((sBuffer = m_input.ReadLine()) != null) // Read until the end of file
            {
                fileNames = new Hashtable();
                Regex rx;
                MatchCollection matches;
                rx = new Regex(@"([++--\w\W]+),\s*([\w\W]+),\s*([\w\W]+),\s*([\w\W\\]+)\s*", RegexOptions.None);
                matches = rx.Matches(sBuffer);
                if (matches.Count != 0)
                {
                    GroupCollection groups = matches[0].Groups;
                    
                    string cmd   = groups[1].Value.ToUpper();
                    string level = groups[2].Value.Trim();
                    string path  = groups[4].Value.Trim();
                    currentMacro = groups[3].Value.Trim();

                    if (cmd[0] == '+')
                    {
                        if (!macroLevels.ContainsKey(level.ToUpper()))
                        {
                            macroLevels[level.ToUpper()] = new Hashtable();
                        }

                        ((Hashtable)macroLevels[level.ToUpper()])[currentMacro] = 1;
                    }

                    if (cmd == "+D")
                    {
                        Console.WriteLine("Adding macro " + currentMacro + " to functions in files in directory " + clientDirectory + path);
                        AddToDirectory(clientDirectory + path, true);
                    }
                    else if (cmd == "-D")
                    {
                        Console.WriteLine("Removing macro " + currentMacro + " from functions in files in directory " + clientDirectory + path);
                        RemoveFromDirectory(clientDirectory + path, true);
                    }
                    else if (cmd == "+F")
                    {
                        Console.WriteLine("Adding macro " + currentMacro + " to functions in file " + clientDirectory + path);

                        if (path.EndsWith("\\*"))
                        {
                            path = path.TrimEnd('*');
                            AddToDirectory(clientDirectory + path, false);
                        }
                        else
                        {
                            AddToFile(clientDirectory + path);
                        }
                    }
                    else if (cmd == "-F")
                    {
                        Console.WriteLine("Removing macro " + currentMacro + " from file " + clientDirectory + path);

                        if (path.EndsWith("\\*"))
                        {
                            path = path.TrimEnd('*');
                            RemoveFromDirectory(clientDirectory + path, false);
                        }
                        else
                        {
                            RemoveFromFile(clientDirectory + path);
                        }
                    }
                }
                rx = new Regex(@"([++--\w\W]+),\s*([\w\W]+),\s*([\w\W]+),\s*([\w\W\\]+),\s*([\w\W]+)\s*", RegexOptions.None);
                matches = rx.Matches(sBuffer);
                if (matches.Count != 0)
                {
                    GroupCollection groups = matches[0].Groups;
                    string level = groups[2].Value.Trim();
                    string path = groups[4].Value.Trim();
                    string func = groups[5].Value.Trim();
                    currentMacro = groups[3].Value;

                    if (groups[1].Value == "+M")
                    {
                        if (!macroLevels.ContainsKey(level.ToUpper()))
                        {
                            macroLevels[level.ToUpper()] = new Hashtable();
                        }

                        ((Hashtable)macroLevels[level.ToUpper()])[currentMacro] = 1;

                        Console.WriteLine("Adding macro " + currentMacro + " from method " + clientDirectory + path + @", " + func);
                        AddToMethod(clientDirectory + path, func);
                    }
                    else if (groups[1].Value == "-M")
                    {
                        Console.WriteLine("Removing macro " + currentMacro + " from method " + clientDirectory + path + @", " + func);
                        RemoveFromMethod(clientDirectory + path, func);
                    }
                }
                String listOfFiles = "";
                foreach (String fileName in fileNames.Keys)
                {
                    listOfFiles += " ";
                    listOfFiles += fileName;
                }

                if (listOfFiles != "")
                {
                    DoSourceDepotSetup(fileNames);
                    foreach (String fileName in fileNames.Keys)
                    {
                        File.Delete(fileName);
                        File.Move(fileName + ".tmp", fileName);
                    }
                }
            }
            m_input.Close();

            if (macroLevels.Count > 0)
            {
                string macroFile = filterFileName + ".macros";

                if (File.Exists(macroFile)) File.Delete(macroFile);

                TextWriter tw = File.CreateText(macroFile);
                uint macroLevelMask = 1;
                uint macroDefineMask = 1;

                StringBuilder sbDefines  = new StringBuilder();
                StringBuilder sbMacros   = new StringBuilder();
                StringBuilder sbEmptDefs = new StringBuilder();

                foreach (string key in macroLevels.Keys)
                {
                    tw.WriteLine(string.Format("#define NATIVE_PROFILE_LEVEL_{0,-6} 0x{1:X8}", key, macroLevelMask));
                    
                    macroLevelMask <<= 1;
                    macroDefineMask = 1;

                    sbDefines.AppendLine("//--//  " + key + " level");
                    sbDefines.AppendLine();
                    foreach (string str in ((Hashtable)macroLevels[key]).Keys)
                    {
                        sbEmptDefs.AppendLine(string.Format("#define {0}()", str));
                        sbDefines.AppendLine(string.Format("#define {0,-46} 0x{1:X8}", str + "__flag", macroDefineMask));
                        macroDefineMask <<= 1;
                        
                        sbMacros.AppendLine(string.Format("#if NATIVE_PROFILE_{0} & {1}__flag", key, str));
                        sbMacros.AppendLine(string.Format("    #define {0}() Native_Profiler profiler_obj", str));
                        sbMacros.AppendLine("#else");
                        sbMacros.AppendLine(string.Format("    #define {0}()", str));
                        sbMacros.AppendLine("#endif");
                        sbMacros.AppendLine();
                    }
                    sbDefines.AppendLine(string.Format("#define NATIVE_PROFILE_{0,-31} 0x{1:X8}", key + "__flag_ALL", macroDefineMask - 1));
                    sbDefines.AppendLine();
                }
                tw.WriteLine();
                tw.WriteLine(sbDefines.ToString());
                tw.WriteLine();
                tw.WriteLine(sbMacros.ToString());
                tw.WriteLine();
                tw.WriteLine(sbEmptDefs.ToString());
                tw.Close();
            }
        }
        public void AddToDirectory(string dir, bool searchSubDirs)
        {
            // Create a new ScanDictory object
            SearchFiles searchFiles = new SearchFiles();

            // Add a FileEvent to the class
            searchFiles.FileEvent += new SearchFiles.FileEventHandler(scanDirectory_FileEventAdd);
            searchFiles.searchPattern = "*.cpp";
            searchFiles.SearchDirectory(dir, searchSubDirs);
        }
        public void RemoveFromDirectory(string dir, bool searchSubDirs)
        {
            // Create a new ScanDictory object
            SearchFiles searchFiles = new SearchFiles();

            // Add a FileEvent to the class
            searchFiles.FileEvent += new SearchFiles.FileEventHandler(scanDirectory_FileEventRemove);
            searchFiles.searchPattern = "*.cpp";
            searchFiles.SearchDirectory(dir, searchSubDirs);
        }
        public void scanDirectory_FileEventAdd(object sender, FileInfo e)
        {
            Console.WriteLine("Found " + e.FullName);
            AddToFile(e.FullName);
        }
        public void scanDirectory_FileEventRemove(object sender, FileInfo e)
        {
            Console.WriteLine("Found " + e.FullName);
            RemoveFromFile(e.FullName);
        }
        private void AddToFile(string fileName)
        {
            bool fileChanged = false;
            string[] files = null;


            Regex exp = new Regex(@"([\w\W]*\\)([\w\W]*\*[\w\W]*)", RegexOptions.None);

            Match match = exp.Match(fileName);

            if (match.Success)
            {
                string directory = match.Groups[1].Value;

                fileName = match.Groups[2].Value;

                files = Directory.GetFiles(directory, fileName, SearchOption.TopDirectoryOnly);
            }
            else
            {
                files = new string[] { fileName };
            }

            foreach (string file in files)
            {
                // Set Encoding to default otherwise some characters like the copyright symbol are lost.
                StreamReader inputStream = new StreamReader(file, Encoding.Default);
                StreamWriter outputStream = new StreamWriter(file + ".tmp", false, Encoding.Default);
                string lastLine = "";
                string currentLine;

                while (!inputStream.EndOfStream)
                {
                    currentLine = inputStream.ReadLine();

                    if (currentLine == null) continue;

                    outputStream.WriteLine(currentLine);
                    Regex rx = new Regex(@"({[\s]*)");
                    MatchCollection matches = rx.Matches(currentLine);
                    if (matches.Count != 1 || (matches.Count == 1 && matches[0].Value.Length != currentLine.Length))
                    {
                        lastLine = currentLine;
                    }
                    else
                    {
                        rx = new Regex(@"\([\w\W]*\)");
                        matches = rx.Matches(lastLine);
                        if (matches.Count != 0)
                        {
                            currentLine = inputStream.ReadLine();
                            rx = new Regex(@"[\w\W]+" + "(" + currentMacro + ")" + @"[\w\W]+");
                            matches = rx.Matches(currentLine);
                            if (matches.Count == 0)
                            {
                                outputStream.WriteLine("    " + currentMacro + "();");
                                fileChanged = true;
                            }
                            outputStream.WriteLine(currentLine);
                        }
                    }
                }
                outputStream.Close();
                inputStream.Close();
                if (fileChanged == true) fileNames[file] = true;
                else File.Delete(file + ".tmp");
            }
        }
        private void RemoveFromFile(string fileName)
        {
            string[] files = null;


            Regex exp = new Regex(@"([\w\W]*\\)([\w\W]*\*[\w\W]*)", RegexOptions.None);

            Match match = exp.Match(fileName);

            if (match.Success)
            {
                string directory = match.Groups[1].Value;

                fileName = match.Groups[2].Value;

                files = Directory.GetFiles(directory, fileName, SearchOption.TopDirectoryOnly);
            }
            else
            {
                files = new string[] { fileName };
            }

            foreach (string file in files)
            {
                // Set Encoding to default otherwise some characters like the copyright symbol are lost.
                StreamReader inputStream = new StreamReader(file, Encoding.Default);
                StreamWriter outputStream = new StreamWriter(file + ".tmp", false, Encoding.Default);
                string lastLine = "";
                string currentLine;
                bool fileChanged = false;
                do
                {
                    currentLine = inputStream.ReadLine();
                    outputStream.WriteLine(currentLine);
                    Regex rx = new Regex(@"({[\s]*)");
                    MatchCollection matches = rx.Matches(currentLine);
                    if (matches.Count != 1 || (matches.Count == 1 && matches[0].Value.Length != currentLine.Length))
                    {
                        lastLine = currentLine;
                    }
                    else
                    {
                        currentLine = inputStream.ReadLine();
                        rx = new Regex(@"[\w\W]+" + "(" + currentMacro + ")" + @"[\w\W]+");
                        matches = rx.Matches(currentLine);
                        if (matches.Count == 0)
                        {
                            outputStream.WriteLine(currentLine);
                        }
                        else fileChanged = true;
                    }
                } while (inputStream.EndOfStream == false);
                outputStream.Close();
                inputStream.Close();
                if (fileChanged == true) fileNames[file] = true;
                else File.Delete(file + ".tmp");
            }
        }
        private void AddToMethod(string fileName, string methodName)
        {
            bool fileChanged = false;
            // Set Encoding to default otherwise some characters like the copyright symbol are lost.
            StreamReader inputStream = new StreamReader(fileName, Encoding.Default);
            StreamWriter outputStream = new StreamWriter(fileName + ".tmp", false, Encoding.Default);
            string lastLine = "";
            string currentLine;
            do
            {
                currentLine = inputStream.ReadLine();
                outputStream.WriteLine(currentLine);
                Regex rx = new Regex(@"({[\s]*)");
                MatchCollection matches = rx.Matches(currentLine);
                if(matches.Count != 1 || (matches.Count == 1 && matches[0].Value.Length != currentLine.Length))
                {
                    lastLine = currentLine;
                }
                else
                {
                    rx = new Regex(@"::([\w\W]+)");
                    matches = rx.Matches(methodName);
                    if (matches.Count == 0)
                    {
                        rx = new Regex(@"::([\w\W]+)\([\w\W]*\)");
                        matches = rx.Matches(lastLine);
                        if (matches.Count == 0) rx = new Regex(@"[\s]+([\w\W]+)\([\w\W]*\)");
                    }
                    else
                    {
                        rx = new Regex(@"[\s]+([\w\W]+)\([\w\W]*\)");
                    }
                    matches = rx.Matches(lastLine);
                    if (matches.Count != 0)
                    {
                        GroupCollection groups = matches[0].Groups;
                        if (groups[1].Value == methodName)
                        {
                            currentLine = inputStream.ReadLine();
                            rx = new Regex(@"[\w\W]+" + "(" + currentMacro + ")" + @"[\w\W]+");
                            matches = rx.Matches(currentLine);
                            if (matches.Count == 0)
                            {
                                outputStream.WriteLine("    " + currentMacro + "();");
                                fileChanged = true;
                            }
                            outputStream.WriteLine(currentLine);
                        }
                    }
                }
            } while (inputStream.EndOfStream == false);
            outputStream.Close();
            inputStream.Close();
            if (fileChanged == true) fileNames[fileName] = true;
            else File.Delete(fileName + ".tmp");
        }
        private void RemoveFromMethod(string fileName, string methodName)
        {
            bool fileChanged = false;
            // Set Encoding to default otherwise some characters like the copyright symbol are lost.
            StreamReader inputStream = new StreamReader(fileName, Encoding.Default);
            StreamWriter outputStream = new StreamWriter(fileName + ".tmp", false, Encoding.Default);
            string lastLine = "";
            string currentLine;
            do
            {
                currentLine = inputStream.ReadLine();
                outputStream.WriteLine(currentLine);
                Regex rx = new Regex(@"({[\s]*)");
                MatchCollection matches = rx.Matches(currentLine);
                if(matches.Count != 1 || matches.Count == 1 && matches[0].Value.Length != currentLine.Length)
                {
                    lastLine = currentLine;
                }
                else
                {
                    rx = new Regex(@"::([\w\W]+)");
                    matches = rx.Matches(methodName);
                    if (matches.Count == 0)
                    {
                        rx = new Regex(@"::([\w\W]+)\([\w\W]+\)");
                        matches = rx.Matches(lastLine);
                        if (matches.Count == 0) rx = new Regex(@"[\s]+([\w\W]+)\([\w\W]*\)");
                    }
                    else
                    {
                        rx = new Regex(@"[\s]+([\w\W]+)\([\w\W]*\)");
                    }
                    matches = rx.Matches(lastLine);
                    if (matches.Count != 0)
                    {
                        GroupCollection groups = matches[0].Groups;
                        if (groups[1].Value == methodName)
                        {
                            currentLine = inputStream.ReadLine();
                            rx = new Regex(@"[\w\W]+" + "(" + currentMacro + ")" + @"[\w\W]+");
                            matches = rx.Matches(currentLine);
                            if (matches.Count == 0)
                            {
                                outputStream.WriteLine(currentLine);
                            }
                            else fileChanged = true;
                        }
                    }
                }
            } while (inputStream.EndOfStream == false);
            outputStream.Close();
            inputStream.Close();
            if (fileChanged == true) fileNames[fileName] = true;
            else File.Delete(fileName + ".tmp");
        }
        void DoSourceDepotSetup(Hashtable listOfFiles)
        {
            StreamWriter outputStream1 = new StreamWriter("do_sd_edit_tmp.bat", false, Encoding.Default);
            StreamWriter outputStream2 = new StreamWriter("do_sd_revert.bat", true, Encoding.Default);
            foreach (String fileName in fileNames.Keys)
            {

                outputStream1.WriteLine(@"sd edit " + fileName);
                outputStream2.WriteLine(this.rootDirectory + @"\bin\sd.exe revert " + fileName);
            }
            outputStream1.Flush();
            outputStream1.Close();
            outputStream2.Flush();
            outputStream2.Close();

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.EnableRaisingEvents = false;
            proc.StartInfo.FileName = "do_sd_edit_tmp.bat";
            proc.StartInfo.Arguments = "";
            proc.Start();
            proc.WaitForExit();
            File.Delete("do_sd_edit_tmp.bat");
        }
    }
    class FilesFilter
    {
        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                NativeProfilerFilter filter = new NativeProfilerFilter(args[0]);
            }
            else
            {
                Console.WriteLine("Usage: NativeProfilerFilter filename");
            }
        }
    }
}
