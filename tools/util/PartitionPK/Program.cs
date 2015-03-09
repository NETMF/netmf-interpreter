using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Build.BuildEngine;

namespace Microsoft.SPOT.Tools.Internal
{  
    class Tree
    {
        string m_name;
        DirectoryInfo m_root;
        int m_root_len;
        List<string> m_allFiles;
        
        public static int s_sequenceNumber = 0;
        public static int s_compId = 0;
        
        public Tree(string name, DirectoryInfo root, bool fCreateCache)
        {
            m_name = name;
            m_root = root;
            m_root_len = m_root.FullName.Length;

            m_allFiles = new List<string>();
            if (fCreateCache)
            {
                foreach (FileInfo f in m_root.GetFiles("*", SearchOption.AllDirectories))
                {
                    m_allFiles.Add(this.RelativeName(f));
                }
            }
        }

        public Tree(string name, DirectoryInfo root)
            : this(name, root, true)
        {
        }
     
        public string Name
        {
            get { return m_name; }
        }
        
        public bool Exists
        {
            get { return m_root.Exists; }
        }

        public DirectoryInfo Root
        {
            get { return m_root; }
        }
       
        public int NumberOfFiles
        {
            get
            {
                return m_allFiles.Count;
            }
        }

        public string[] Files
        {
            get
            {
                string[] fa = new string[m_allFiles.Count];
                m_allFiles.CopyTo(fa);
                return fa;
            }
        }
        
        public string RelativeName(FileInfo f)
        {
            try
            {
                return f.FullName.Substring(m_root.FullName.Length + 1);
            }
            catch
            {
                return "";
            }
        }

        public uint IdFromDi(DirectoryInfo di)
        {
            return (uint)(@"$" + di.FullName.Substring(m_root_len)).GetHashCode();
        }

        public string FileIdFromFile(DirectoryInfo di, FileInfo f)
        {
            return String.Format(@"F{0}.{1}.{2}", m_name, IdFromDi(di), (uint)f.Name.GetHashCode());
        }
        
        public string ComponentIdFromDi(DirectoryInfo di)
        {
            return String.Format(@"C{0}.{1}", m_name, IdFromDi(di));
        }

        public string DirectoryIdFromDi(DirectoryInfo di)
        {
            return String.Format(@"D{0}.{1}", m_name, IdFromDi(di));
        }

        public bool ContainsFile(string f)
        {
            return m_allFiles.Contains(f);
        }        

        public void RemoveFile(string f)
        {
            m_allFiles.Remove(f);
        }
        
        public void AddFile(string f)
        {
            m_allFiles.Add(f);
        }

        public List<string> DumpTree(string outputPath)
        {
            using (StreamWriter sw = new StreamWriter(Path.Combine(outputPath, this.Name + ".wxs")))
            {
                return this.DumpTree(sw);
            }            
        }

        public List<string> DumpTree()
        {
            return this.DumpTree(Console.Out);
        }

        public List<string> DumpTree(TextWriter tw)
        {
            tw.WriteLine("<Wix xmlns=\"http://schemas.microsoft.com/wix/2006/wi\">");
            tw.WriteLine("<Fragment><DirectoryRef Id=\"INSTALLDIR\">");

            ///
            /// The PK installer has been split into separate installers: Main PK intaller, Crypto installer,
            /// Network (ARM) installer, Network (Thumb) intaller and Network (Thumb2) installer.  The
            /// new installers use subdirectories from the porting kit root directory.  Therefore, in order
            /// to install them in their correct place in the PK tree we need to assure the parent directories
            /// are listed in the wxs file.  The following code finds the parent directores and adds them to the 
            /// wxs file.
            /// 
            DirectoryInfo parentDir = m_root.Parent;
            string baseDir = PartitionPK.m_tgtdir.FullName.ToLower() + "\\";
            baseDir = baseDir.Replace("\\\\", "\\");
            int depth = 0;
            List<DirectoryInfo> subDirs = new List<DirectoryInfo>();

            ///
            /// We stop collecting parent directories when we reach the directory 2 levels up from the base directory.
            /// This is because the PK build produces folders for target and branch (Template3.1 and client_v4_0_dev
            /// for example).  However the install target does not include these directories, so we will ignore them 
            /// here as well.
            /// 
            /// eg. 
            ///     PK source -> C:\ports\client_v4_0_dev_PortingKits\Template3.1\client_v4_0_dev\...
            ///     PK install-> d:\MicroFrameworkPK_v4_0\...
            /// 
            while (parentDir != null && parentDir.Parent.Parent.FullName.ToLower().Contains(baseDir))
            {
                subDirs.Add(parentDir);

                parentDir = parentDir.Parent;
            }

            ///
            /// m_root_len is used for calculating the wxs component hashes (using the subdirectory and file name), 
            /// if we are adding parent directories then the root length is actuall less than we originall presumed
            /// 
            if (parentDir != null)
            {
                m_root_len = parentDir.Parent.FullName.Length;
            }

            ///
            /// Add the directories in the wxs file, so that the files will be properly nested in the PK.
            /// 
            for (int i = subDirs.Count - 1; i >= 0; i--)
            {
                tw.WriteLine("{0}<Directory Id=\"{1}\" Name=\"{2}\" >",
                    Padding(++depth), DirectoryIdFromDi(subDirs[i]), subDirs[i].Name);
            }
     
            List<string> components = this.DumpDirectory(tw, m_root, depth);

            ///
            /// Add the trailing close tags for the parent directories
            /// 
            while (depth-- > 0)
            {
                tw.WriteLine("{0}</Directory>", Padding(depth));
            }

            tw.WriteLine("</DirectoryRef></Fragment></Wix>");
            tw.Flush();
            
            return components;
        }

        private static string Padding(int d)
        {
            return new string(' ', d);
        }
        
        private static string EightDotThree(string filename)
        {
            return String.Format("f{0}.000", s_sequenceNumber++);
        }

        private List<string> DumpDirectory(TextWriter tw, DirectoryInfo currDi, int depth)
        {
            List<string> components = new List<string>();

            ///
            /// Ignore exclusion paths identified by the caller
            /// 
            if (PartitionPK.s_excludePaths.ContainsKey(currDi.FullName.ToLower()))
            {
                return components;
            }

            FileInfo[] foundfiles = currDi.GetFiles();
            DirectoryInfo[] directories = currDi.GetDirectories();
            List<FileInfo> containedFiles = new List<FileInfo>();
            
            foreach (FileInfo f in foundfiles)
            {
                if (this.m_allFiles.Contains(RelativeName(f)))
                    containedFiles.Add(f);
            }

            StringWriter sw = new StringWriter();
            if (directories.Length > 0)
            {
                foreach (DirectoryInfo di in currDi.GetDirectories())
                {
                    components.AddRange(this.DumpDirectory(sw, di, depth + 1));
                }
            }

            if (containedFiles.Count + components.Count() > 0)
            {
                if (depth > 0)
                {
                    tw.WriteLine("{0}<Directory Id=\"{1}\" Name=\"{2}\" >",
                        Padding(depth), DirectoryIdFromDi(currDi), currDi.Name);
                }

                if (containedFiles.Count > 0)
                {
                    string componentId = ComponentIdFromDi(currDi);
                    components.Add(componentId);
                    
                    tw.WriteLine("{0}<Component Id=\"{1}\" Guid=\"{2}\" >",
                        Padding(depth + 1), 
                        componentId, 
                        Guid.NewGuid().ToString()
                        );

                    foreach (FileInfo f in containedFiles)
                    {
                        tw.WriteLine("{0}<File Id=\"{1}\" Name=\"{2}\" DiskId=\"{4}\" Source=\"{3}\" />",
                            Padding(depth + 2), FileIdFromFile(currDi, f), f.Name, f.FullName, PartitionPK.s_mediaAllocator.DiskId(f)); 
                    }

                    tw.WriteLine("{0}</Component>", Padding(depth+1));
                }
                
                tw.Write(sw.GetStringBuilder().ToString());
                
                if (depth > 0)
                {
                    tw.WriteLine("{0}</Directory>", Padding(depth));
                }            
                tw.Flush();
            }
            
            return components;
        }
    }
    
    class MediaAllocator
    {
        static int CABCOMPRESSIONFACTOR = 2;
        
        int m_currentDiskId;
        long m_maxCabSize;
        long m_currentTotalFileSize;
        TextWriter m_mediaElementWriter;
        
        public MediaAllocator(int maxCabSizeMegabytes, string path)
        {
            m_maxCabSize = maxCabSizeMegabytes * 1024 * 1024;
            m_currentTotalFileSize = 0;
            m_currentDiskId = 2;
            m_mediaElementWriter = new StreamWriter(path);
            m_mediaElementWriter.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            m_mediaElementWriter.WriteLine("<Include>");
            m_mediaElementWriter.WriteLine("    <Media Id=\"{0}\" Cabinet=\"MFPK{0}.cab\" EmbedCab=\"no\" />", m_currentDiskId);
        }

        public void Close()
        {
            m_mediaElementWriter.WriteLine("</Include>");    
            m_mediaElementWriter.Flush();
            m_mediaElementWriter.Close();
        }
        
        public int DiskId(FileInfo f)
        {
            int diskId = m_currentDiskId;
            m_currentTotalFileSize += f.Length;
            if (m_currentTotalFileSize > (m_maxCabSize * CABCOMPRESSIONFACTOR) )
            {
                m_currentDiskId++;
                m_currentTotalFileSize = 0;
                m_mediaElementWriter.WriteLine("    <Media Id=\"{0}\" Cabinet=\"MFPK{0}.cab\" EmbedCab=\"no\" />", m_currentDiskId);
            }
            return diskId;
        }
    }
   
    class PartitionPK
    {
        public class SyntaxError : Exception
        {
            public SyntaxError(string msg)
                : base(msg)
            {
            }
            
            public string Usage
            {
                get
                {
                    StringWriter sw = new StringWriter();

                    sw.WriteLine(@"usage: PartitionPK <product wxs> <product name> <output directory> <properties> <exclude directories> <architecture>[, <architecture>]");
                    sw.WriteLine(@"   where <product wxs> is <WXS product file>");
                    sw.WriteLine(@"   where <product name> is <product name>");
                    sw.WriteLine(@"   where <output directory> is <OutputPath>");
                    sw.WriteLine(@"   where <properties> is <PropertyName>=<PropertyValue>[;<PropertyName>=<PropertyValue]");
                    sw.WriteLine(@"   where <exclude directories> is [NONE|<ExcludePath>][;<ExcludePath>]");
                    sw.WriteLine(@"   where <architecture> is <FeatureName>=<PortClientDirectory>");
                    //sw.WriteLine(@"   and max-cab-file-size is an integer number of megabytes.");
                    sw.WriteLine(@"   eg 'PartitionPK PKProduct\PKProduct.wxs .\myDir prop=value;prop2=value2 Crypto\LIB;DeviceCode\PAL\rtip\LIB ARM=C:\kits\Template\client_v3_0");
                    
                    return sw.ToString();
                }
            }
        }
        
        internal static DirectoryInfo m_tgtdir;
        Tree m_common_tree;
        List<Tree> m_trees;
        Dictionary<string, string> m_props = new Dictionary<string, string>();

        public static MediaAllocator s_mediaAllocator;

        internal static Hashtable s_excludePaths = new Hashtable();
        string m_outpuPath;
        static string s_productWxs;
        static string s_productName;
        static string s_srcdir;
        static string s_wixdir;
        static string s_wixtools;
        //static string s_wixcadir;
       
        public PartitionPK(string[] args)
        {
            // Validate arguments, create target directory tree, and initialize target subdir members
            if ( args.Length < 6 )
                throw new SyntaxError(String.Format("incorrect number ({0}) of arguments provided", args.Length));

            s_productWxs = args[0];

            if (!File.Exists(s_productWxs))
            {
                if (File.Exists(Path.Combine(s_srcdir, s_productWxs)))
                {
                    s_productWxs = Path.Combine(s_srcdir, s_productWxs);
                }
                else
                {
                    throw new ApplicationException(string.Format("Could not find Product: {0}", s_productWxs));
                }
            }

            s_productName = args[1];

            m_tgtdir = new DirectoryInfo(args[2]);
            if (!m_tgtdir.Exists)
                throw new Exception(String.Format("Target directory {0} does not exist", m_tgtdir.FullName));

            m_outpuPath = Path.Combine(m_tgtdir.FullName, Path.GetFileNameWithoutExtension(s_productWxs));

            if (!Directory.Exists(m_outpuPath))
            {
                Directory.CreateDirectory(m_outpuPath);
            }
#if false   
            // uncomment this later, when it's more convenient to add an argument to the command line
            int maxCabSize = 0; 
            try
            {
                maxCabSize = int.Parse(args[1]);
            }
            catch(Exception /*ex*/)
            {
                throw new SyntaxError("\"{0}\" is not a valid cab-file size");
            }
#else
            int maxCabSize = 100;
#endif

            s_mediaAllocator = new MediaAllocator(maxCabSize, Path.Combine(m_outpuPath, "MoreMedia.wxi"));

            ///
            /// Setup properties used for Candle and Lite
            ///
            string[] props = args[3].Split(new char[]{';'}, StringSplitOptions.RemoveEmptyEntries);
            if (props != null && props.Length > 0)
            {
                foreach (string prop in props)
                {
                    string []nv = prop.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                    if (nv != null && nv.Length >= 2)
                    {
                        if (nv[1].Trim().Contains(' '))
                        {
                            m_props[nv[0]] = "\"" + nv[1] + "\"";
                        }
                        else
                        {
                            m_props[nv[0]] = nv[1];
                        }
                    }
                }
            }

            ///
            /// Add PK flavors
            ///
            m_trees = new List<Tree>();
            IEnumerable<string> eArgs = args.Skip(5);
            char[] equalsChar = new char[1] { '=' };
            foreach (string arg in eArgs)
            {
                string[] pair = arg.Split(equalsChar, 2);
                m_trees.Add(new Tree(pair[0], new DirectoryInfo(pair[1])));
            }
            
            if (m_trees.Count < 1)
                throw new Exception("too few source trees provided");
            
            foreach (Tree t in m_trees)
            {            
                if (!t.Exists)
                    throw new Exception(String.Format("{0} directory {1} does not exist", t.Name, t.Root.FullName));
            }

            m_common_tree = new Tree("Common", m_trees[0].Root, false);

            ///
            /// Set up the list of directories that will be excluded from this installation.
            /// 
            string[] excludeDirs = args[4].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (excludeDirs != null && excludeDirs.Length > 0)
            {
                foreach (string exclude in excludeDirs)
                {
                    ///
                    /// NONE indicates that there are no exclusion directories (it is needed because this is not an optional
                    /// parameter)
                    /// 
                    if (string.Compare(exclude, "NONE", true) == 0)
                    {
                        break;
                    }

                    string exPath = exclude;
                    if (!Path.IsPathRooted(exclude))
                    {
                        exPath = Path.Combine(m_common_tree.Root.FullName, exclude);
                    }
                    if (!Directory.Exists(exPath))
                    {
                        Console.WriteLine("Warning!!! exclude directory does not exist: " + exPath);
                    }
                    s_excludePaths[exPath.ToLower()] = 1;
                }
            }
        }
        
        ~PartitionPK()
        {
        }
        
        private int Run()
        {
            try
            {
                IEnumerable<Tree> otherTrees = m_trees.Skip(1);
                string[] files = m_trees[0].Files;

                foreach (string f in files)
                {
                    bool fCommon = true;
                    foreach (Tree t in otherTrees)
                    {
                        if (!t.ContainsFile(f))
                            fCommon = false;
                    }
                    if (fCommon)
                    {
                        m_common_tree.AddFile(f);
                        foreach (Tree t in m_trees)
                        {
                            t.RemoveFile(f);
                        }
                    }
                };
                                
                List<string> commonComponents = m_common_tree.DumpTree(m_outpuPath);
                using (TextWriter commonComponentsFile = new StreamWriter(Path.Combine(m_outpuPath, "common-feature-component-list.wxi")))
                {
	                commonComponentsFile.WriteLine("<?xml version='1.0'?>");
	                commonComponentsFile.WriteLine("<Include>");
                    commonComponents.ForEach(new Action<string>(delegate(string c)
                    {
                        commonComponentsFile.WriteLine("    <ComponentRef Id='{0}' />", c);
                    }));
                    commonComponentsFile.WriteLine("</Include>");
                    commonComponentsFile.Flush();
                }

                using (TextWriter featureFile = new StreamWriter(Path.Combine(m_outpuPath, "platform-features.wxi")))
                {
                    featureFile.WriteLine("<?xml version='1.0'?>");
                    featureFile.WriteLine("<Include>");
                    
                    m_trees.ForEach(delegate(Tree t)
                    {
                        if (t.NumberOfFiles > 0)
                        {
                            List<string> featureComponents = t.DumpTree(m_outpuPath);
                            featureFile.WriteLine("<Feature Id='{0}_Feature'", t.Name);
                            featureFile.WriteLine("         Title='{0} Architecture'", t.Name);
                            featureFile.WriteLine("         Description='MF Solutions, device code, and libraries for porting the .NET Micro Framework to platforms based on the {0} architecture'", t.Name);
                            featureFile.WriteLine("         Level='2'");
                            featureFile.WriteLine("         AllowAdvertise='no'");
                            featureFile.WriteLine("         InstallDefault='followParent'");
                            featureFile.WriteLine("         TypicalDefault='install'");
                            featureFile.WriteLine("         Display='expand'");
                            featureFile.WriteLine("         >");
                            featureComponents.ForEach(new Action<string>(delegate(string c)
                            {
                                featureFile.WriteLine("    <ComponentRef Id='{0}' />", c);
                            }));
                            featureFile.WriteLine("</Feature>");
                        }
                    });
                    featureFile.WriteLine("</Include>");
                    featureFile.Flush();
                }

                int exitcode = 0;
                List<string> sources = new List<string>();
                /* PK documentation hasn't been tested in Document Explorer
                 sources.Add(Path.Combine(s_srcdir, @"Docs\NetMFCollection.wxs"));
                sources.Add(Path.Combine(s_srcdir, @"Docs\RCLPort.wxs"));*/
                sources.Add(s_productWxs);
                sources.Add(Path.Combine(m_outpuPath, (this.m_common_tree.Name + ".wxs")));
                m_trees.ForEach(new Action<Tree>(delegate(Tree t)
                {
                    if(t.NumberOfFiles > 0)
                    {
                        sources.Add(Path.Combine(m_outpuPath, (t.Name + ".wxs")));
                    }
                }));

                s_mediaAllocator.Close();
                
                foreach (string f in sources)
                {
                    exitcode = this.Candle(f);
                    if (exitcode != 0) return exitcode;
                };
                return this.Light();
            }
            finally
            {
                //Console.WriteLine("!!!!!!!!!!!!!!!! FINISHED !!!!!!!!!!!!!!!!!!");
                //Console.ReadLine();
            }
        }

        private static void PrintProcMessage(ProcessStartInfo psi)
        {
            Console.WriteLine("Executing \"{0} {1}\" in {2}", psi.FileName, psi.Arguments, psi.WorkingDirectory);
        }

        private int ExecuteCmd(string cmdFile, string arguments)
        {
            Process p = new Process();

            p.StartInfo.UseShellExecute = false;
            p.StartInfo.WorkingDirectory = Path.GetDirectoryName(s_productWxs);
            p.StartInfo.FileName = cmdFile;
            p.StartInfo.Arguments = arguments;

            PrintProcMessage(p.StartInfo);

            p.Start();
            p.WaitForExit();
            return p.ExitCode;
        }

        private int Candle(string srcFile)
        {
            string argstring = "-nologo -v -wx";        // most verbose, most warnings, treat warnings as errors
            argstring += String.Format(" -I{0} -I{1}", Path.Combine(s_srcdir, "Include"), m_outpuPath);
            argstring += " -trace";		        // show source trace for errors, warnings, & verbose messages
            argstring += String.Format(" -out \"{0}\"", Path.Combine(m_outpuPath, Path.GetFileNameWithoutExtension(srcFile) + ".wixobj"));
            argstring += String.Format(" -dCommonDir={0}", m_common_tree.Root.FullName);
            foreach (string key in m_props.Keys)
            {
                argstring += " -d" + key + "=" + m_props[key];
            }
            argstring += " " + srcFile;
            
            return this.ExecuteCmd(Path.Combine(s_wixtools, "candle.exe"), argstring);
        }
        
        private int Light()
        {
            string argstring = " -nologo -v -wx"; // most verbose, most warnings, treat warnings as errors
            argstring += " -sw1076 -sw1079 -sw1056 -sw1055 -sice:ICE03 -sice:ICE18 -sice:ICE20 -sice:ICE30";
            argstring += " -sice:ICE31 -sice:ICE38 -sice:ICE43 -sice:ICE57 -sice:ICE60 -sice:ICE64";                 
	        //argstring += " " + Path.Combine(s_wixcadir, "wixca.wixlib");
            argstring += " -b " + Path.Combine(s_srcdir, "LIB");    //base path to locate all files
            argstring += " -out \"" + Path.Combine(m_outpuPath, s_productName + ".MSI") + "\"";
            argstring += " -loc " + Path.Combine(s_srcdir, "LIB\\WixUI_2.0.5805.0_en-us.wxl");
            //argstring += " " + Path.Combine(s_srcdir, "LIB\\wixui.wixlib");

            foreach (string key in m_props.Keys)
            {
                argstring += " -d" + key + "=" + m_props[key];
            }

            m_trees.ForEach(new Action<Tree>(delegate(Tree t)
            {
                if(t.NumberOfFiles > 0)
                {
                    argstring += " " + Path.Combine(m_outpuPath, t.Name + ".wixobj");
                }
            }));
            argstring += " \"" + Path.Combine(m_outpuPath, Path.GetFileNameWithoutExtension(s_productWxs) + ".wixobj") + "\"";
            /*PK Documentation hasn't been tested in Document Explorer
            argstring += " " + Path.Combine(m_outpuPath, "NetMFCollection.wixobj");
            argstring += " " + Path.Combine(m_outpuPath, "RCLPort.wixobj");*/
            argstring += " " + Path.Combine(m_outpuPath, "Common.wixobj");

	        return this.ExecuteCmd(Path.Combine(s_wixtools, "light.exe"), argstring);
        }
    
        static void InitEnvironment()
        {
            string sporoot = Environment.GetEnvironmentVariable("SPOROOT");
            if (String.IsNullOrEmpty(sporoot))
                throw new ApplicationException("SPOROOT environment variable is not defined");

            string spoclient = Environment.GetEnvironmentVariable("SPOCLIENT");
            if (String.IsNullOrEmpty(spoclient))
                throw new ApplicationException("SPOCLIENT environment variable is not defined");
            
            s_srcdir    = Path.Combine(spoclient, @"setup");
            s_wixdir    = Path.Combine(sporoot, @"tools\x86\WiX");
            s_wixtools = Path.Combine(s_wixdir, @"tools_3_5_1315_0");
            //s_wixcadir  = Path.Combine(s_wixdir, @"ca_2_0_3719_0");
        }
        
        static int Main(string[] args)
        {
            try
            {
                InitEnvironment();
                return (new PartitionPK(args)).Run();
            }
            catch (PartitionPK.SyntaxError ser)
            {
                Console.WriteLine(ser.Message);
                Console.WriteLine(ser.Usage);
                return -1;
            }
            catch (ApplicationException aex)
            {
                Console.Error.WriteLine("error:PartitionPK: " + aex.Message);
                return -1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("error:PartitionPK: Exception: {0}", ex.ToString());
                return -2;
            }
        }
    }
}
