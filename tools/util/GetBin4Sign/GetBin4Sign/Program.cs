using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Microsoft.Tools.WindowsInstallerXml;
using Microsoft.Tools.WindowsInstallerXml.Msi;
using Microsoft.Tools.WindowsInstallerXml.Msi.Interop;

namespace GetBin4Sign
{
    class Program
    {
        static string outDir = null;
        static string installer = null;
        static List<string> binFiles = new List<string>();

        static void Main(string[] args)
        {
            foreach (string arg in args)
            {
                string [] argSplit = arg.Split(new char[] {':'}, 2);

                if (argSplit.Length != 2) Usage();

                argSplit[0] = argSplit[0].Substring(1).ToLower();

                if (argSplit[0] == "msi")
                {
                    installer = argSplit[1];
                }
                else if (argSplit[0] == "out")
                {
                    outDir = argSplit[1];
                }
                else
                {
                    Usage();
                }
            }

            if (installer == null) Usage();

            Database db = new Database(installer, OpenDatabase.ReadOnly);
            View view = db.OpenExecuteView("SELECT FileName FROM File");

            Record record;
            while (view.Fetch(out record))
            {
                string file = record[1];

                //parse file by |??

                string[] fileParts = file.Split('|');
                string name;

                if (fileParts.Length == 2)
                {
                    name = fileParts[1].ToLower();
                }
                else if (fileParts.Length == 1)
                {
                    name = fileParts[0].ToLower();
                }
                else
                {
                    continue;
                }

                string ext = Path.GetExtension(name);
                if (ext == ".dll" || ext == ".exe")
                {
                    if (!binFiles.Contains(name))
                    {
                        binFiles.Add(name);
                        Console.WriteLine(name);
                    }
                }
            }

            if (outDir == null) Environment.Exit(0);

            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);   
            }

            if (!Directory.Exists(outDir + "\\client"))
            {
                Directory.CreateDirectory(outDir + "\\client");
            }

            if (!Directory.Exists(outDir + "\\server"))
            {
                Directory.CreateDirectory(outDir + "\\server");
            }

            string clientDir = Environment.GetEnvironmentVariable("BUILD_TREE_CLIENT") + "\\dll";
            string serverDir = Environment.GetEnvironmentVariable("BUILD_TREE_SERVER") + "\\dll";

            if (!Directory.Exists(clientDir))
            {
                Console.WriteLine("Could not find {0}.  Did you set the environment", clientDir);
                Environment.Exit(2);
            }

            if (!Directory.Exists(serverDir))
            {
                Console.WriteLine("Could not find {0}.  Did you set the environment", serverDir);
                Environment.Exit(2);
            }

            foreach (string binFile in binFiles)
            {
                bool copyClient = false;
                if (File.Exists(clientDir + "\\" + binFile))
                {
                    copyClient = true;
                    File.Copy(clientDir + "\\" + binFile, outDir + "\\client\\" + binFile);
                }

                if (File.Exists(serverDir + "\\" + binFile))
                {
                    if (copyClient)
                    {
                        ConsoleColor fore = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Warning: {0} exists in both the client and server directories", binFile);
                        Console.ForegroundColor = fore;
                    }
                    File.Copy(serverDir + "\\" + binFile, outDir + "\\server\\" + binFile);
                }
            }
        }

        static void Usage()
        {
            Console.WriteLine(Resources.Usage);
            Environment.Exit(1);
        }
    }
}
