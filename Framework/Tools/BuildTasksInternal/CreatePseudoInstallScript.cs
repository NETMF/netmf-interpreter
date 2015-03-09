using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

using Microsoft.Tools.WindowsInstallerXml;
using Microsoft.Tools.WindowsInstallerXml.Msi;

namespace Microsoft.SPOT.Tasks.Internal
{

    public class CreatePseudoInstallScript : Task
    {
        private StreamWriter m_sw = null;
        private Database m_db = null;

        internal class KeyData
        {
            internal KeyData(string root, string key, string name, string type, string value)
            {
                m_root = root;
                m_key = key;
                m_name = name == null ? "" : name;
                m_type = type;
                m_value = value;
            }
            public string m_root;
            public string m_key;
            public string m_name;
            public string m_type;
            public string m_value;
        }

        private List<KeyData> m_installReg = null;


        private ITaskItem m_outputScript = null;
        [Required]
        public ITaskItem OutputScript
        {
            set { m_outputScript = value; }
            get { return m_outputScript; }
        }


        private ITaskItem m_installerFile = null;
        [Required]
        public ITaskItem InstallerFile
        {
            set { m_installerFile = value; }
            get { return m_installerFile; }
        }


        private ITaskItem[] m_replacements = new ITaskItem[0];
        private Dictionary<string,string> m_replacementsDictionary;
        public ITaskItem[] Replacements
        {
            set
            {
                try
                {
                    m_replacementsDictionary = new Dictionary<string,string>();
                    foreach(ITaskItem replaceItem in value)
                    {
                        string replaceWith = replaceItem.GetMetadata("ReplaceWith");
                        if ( null == replaceWith ) throw new Exception("Replacements items must have \"ReplaceWith\" metadata");
                        m_replacementsDictionary.Add(replaceItem.ItemSpec, replaceWith);
                    }
                    m_replacements = value;
                }
                catch (Exception ex)
                {
                    Log.LogError("CreatePseudoInstallScript error: " + ex.Message + " At: " + ex.StackTrace);
                }
            }
            get { return m_replacements; }
        }


        private ITaskItem[] m_registrySettings = new ITaskItem[0];
        public ITaskItem[] RegistrySettings
        {
            set
            { m_registrySettings = value; }
            get { return m_registrySettings; }
        }

        private ITaskItem[] m_ignoreKeys = new ITaskItem[0];
        public ITaskItem[] IgnoreKeys
        {
            set { m_ignoreKeys = value; }
            get { return m_ignoreKeys; }
        }

        private ITaskItem[] m_keyReplacements = new ITaskItem[0];
        public ITaskItem[] KeyReplacements
        {
            set { m_keyReplacements = value; }
            get { return m_keyReplacements; }
        }


        private bool m_verbose = false;
        public bool Verbose
        {
            set { m_verbose = value; }
            get { return m_verbose; }
        }

        public CreatePseudoInstallScript()
        {
        }



        string GetComponentFromFile(string fileKey, out string fileName)
        {
            fileName = @"C:\temp\temp.txt";
            string query = string.Format("SELECT Component_, FileName FROM File WHERE File = '{0}'", fileKey);
            View view = m_db.OpenExecuteView(query);

            Record record = view.Fetch();

            if ( null == record )
            {
                Log.LogWarning(String.Format("Could not fetch record for {0} from database", fileKey));
                return null;
            }
            string comp = record[1];
            string file =  record[2];

            //parse file by |??

            string[] fileParts = file.Split('|');

            if(fileParts.Length > 2)
                throw new ApplicationException();

            fileName = file;

            if(fileParts.Length > 1)
                fileName = fileParts[1];

            //record = view.Fetch();

            //if(record == null)
            //    throw new ApplicationException();

            return comp;
        }

        void Create()
        {
            View view = m_db.OpenExecuteView("SELECT * FROM Registry");
            Record record;
            while ((record = view.Fetch()) != null)
            {
                string registry = record[1];
                string root = record[2];
                string key = record[3];
                string name = record[4];
                string val = record[5];
                string original_val = val;
                string component = record[6];

                if ( m_verbose )
                    {
                    string msg = "Examining registry record |";
                    for (int i=0; i<7; i++)
                        msg += record[i] + "|";
                    Log.LogMessage(msg);
                    }

                string rootKey = null;

                switch (int.Parse(root))
                {
                    case 0:
                        rootKey = "HKCR";
                        break;
                    case 1:
                        rootKey = "HKCU";
                        break;
                    case 2:
                        rootKey = "HKLM";
                        break;
                    case 3:
                        rootKey = "HKUS";
                        break;
                    default:
                        throw new ApplicationException();
                }

                string type = "SZ";

                if (string.IsNullOrEmpty(val))
                {
                }
                else if(val.StartsWith("#x"))
                {
                    val = val.Remove(0, 2);
                    type = "BINARY";
                }
                else if (val.StartsWith("#%"))
                {
                    val = val.Remove(0, 2);
                    type = "EXPAND_SZ";
                }
                else if(val.StartsWith("#"))
                {
                    val = val.Remove(0, 1);
                    if(!val.StartsWith("#"))
                        type = "DWORD";
                }
                else if (val.IndexOf("[~]") >= 0)
                {
                    throw new ApplicationException("No multi-sz support");
                }
                else if(val.Contains("[") && val.Contains("]"))
                {
                    bool fFile = val.Contains("[#");
                    int startIdx = val.IndexOf('[');
                    int endIdx = val.IndexOf(']');

                    if (endIdx > startIdx)
                    {
                        string lookup = "";

                        if (m_verbose)
                            Log.LogMessage(String.Format("Looking for a replacement for key:{0} value:\"{1}\"", rootKey, val));

                        string fileName = null;

                        if (val[startIdx + 1] == '#' || val[startIdx + 1] == '$')
                        {
                            int st = startIdx + 2;

                            lookup = val.Substring(st, endIdx - st);
                        }
                        else
                        {
                            int st = startIdx + 1;

                            lookup = val.Substring(st, endIdx - st);
                        }

                        val = val.Remove(startIdx, endIdx - startIdx + 1);

                        //It's a file, need to check for replacement value

                        if (fFile)
                        {
                            //need to find the right component
                            if (m_verbose)
                                Log.LogMessage(String.Format("Looking looking up real file for key:{0} value:\"{1}\"", rootKey, lookup));

                            string newVal = GetComponentFromFile(lookup, out fileName);
                            if (null != newVal)
                            {
                                if (m_verbose)
                                    Log.LogMessage(String.Format("File ID \"{0}\" corresponds to file \"{1}\"", lookup, newVal));

                                lookup = newVal;
                            }
                            else
                            {
                                if (m_verbose)
                                    Log.LogWarning(String.Format("Using dummy filename \"{0}\" for \"{1}\"", fileName, lookup));
                            }
                        }

                        string replacement;
                        if (m_replacementsDictionary.TryGetValue(lookup, out replacement))
                        {
                            if (m_verbose)
                                Log.LogMessage(String.Format("   Replacing data for \"{0}\" with \"{1}\"", lookup, replacement));
                            val = val.Insert(startIdx, replacement);
                        }
                        else
                        {
                            if (m_verbose)
                                Log.LogWarning(String.Format("There is no replacement declared for {0}:{1}", rootKey, lookup));
                        }

                        if (fFile)
                        {
                            val = Path.Combine(val, fileName);
                        }
                    }
                }

                if (name == "*")
                {
                    name = "";
                }

                SaveReg("REPLACEMENT", rootKey, key, name, type, val);
            }
        }

        void Raw()
        {
            foreach (ITaskItem rawItem in m_registrySettings)
            {
                SaveReg(rawItem.ItemSpec,
                    rawItem.GetMetadata("Root"),
                    rawItem.GetMetadata("Key"),
                    rawItem.GetMetadata("Name"),
                    rawItem.GetMetadata("Type"),
                    rawItem.GetMetadata("Value"));
            }
        }

        const string regClearFlagsFormat        = "%reg32_exe% FLAGS \"{0}\" SET";
        const string regQueryFormat             = "%reg32_exe% QUERY \"{0}\" /v \"{1}\"";
        const string regQueryEmptyNameFormat    = "%reg32_exe% QUERY \"{0}\" /ve";
        const string regAddFormat               = "%reg32_exe% ADD \"{0}\" /v \"{1}\" /t REG_{2} /d \"{3}\" /f";
        const string regAddEmptyNameFormat      = "%reg32_exe% ADD \"{0}\" /ve /t REG_{1} /d \"{2}\" /f";
        const string regDelKeyFormat            = "%reg32_exe% DELETE \"{0}\" /f";
        const string regDelFormat               = "%reg32_exe% DELETE \"{0}\" /v \"{1}\" /f";
        const string regDelEmptyNameFormat      = "%reg32_exe% DELETE \"{0}\" /ve /f";

        private void SaveReg(
            string itemspec,
            string root,
            string key,
            string name,
            string type,
            string val)
        {
            string err = null;

            if ( root == null ) err = "root";
            if ( key == null ) err = "key";
            //if ( name == null ) err = "name";
            if ( type == null ) err = "type";
            //if ( val == null ) err = "val";
            if ( err != null ) throw new Exception(String.Format("Bad {1} for CreatePseudoInstallScript RawReg item {0}", itemspec, err));

            string fullKey = String.Format("{0}\\{1}", root, key);

            if ( m_verbose )
            {
                Log.LogMessage(String.Format("^Check whether to ignore \"{0}\"", fullKey));
            }
            foreach(ITaskItem ignoreKey in m_ignoreKeys)
            {
                string comment = ignoreKey.ItemSpec;
                string prefixToIgnore = ignoreKey.GetMetadata("Prefix").ToUpper();

                if ( m_verbose )
                {
                    Log.LogMessage(String.Format("   ??Checking against \"{1}\" ({0})", comment, prefixToIgnore));
                }
                if ( fullKey.ToUpper().StartsWith(prefixToIgnore) )
                {
                    if ( m_verbose )
                        Log.LogMessage(String.Format("   !!Ignoring key \"{0}\" ({1})", fullKey, comment));
                    return;
                }
            }

            if ( m_verbose )
                Log.LogMessage(String.Format("*Check whether to remap key \"{0}\"", fullKey));

            ITaskItem longestMatchingPrefixItem = null;
            foreach(ITaskItem replaceKey in m_keyReplacements )
            {
                string prefixToReplace = replaceKey.GetMetadata("Prefix");

                if ( m_verbose )
                    Log.LogMessage(String.Format("   ??Checking against \"{1}\" ({0})", replaceKey.ItemSpec, prefixToReplace));


                if ( fullKey.ToUpper().StartsWith(prefixToReplace.ToUpper()) )
                {
                    if ( longestMatchingPrefixItem == null )
                        longestMatchingPrefixItem = replaceKey;
                    else if ( prefixToReplace.Length > longestMatchingPrefixItem.GetMetadata("Prefix").Length )
                        longestMatchingPrefixItem = replaceKey;
                }
            }
            if ( longestMatchingPrefixItem != null )
            {
                string replacement = longestMatchingPrefixItem.GetMetadata("ReplaceWith") + fullKey.Substring(longestMatchingPrefixItem.GetMetadata("Prefix").Length);

                if ( m_verbose )
                    Log.LogMessage(String.Format("   !!Replacing key \"{0}\" with \"{1}\" ({2})", fullKey, replacement, longestMatchingPrefixItem.ItemSpec));
                fullKey = replacement;
            }
            else
            {
                if ( m_verbose )
                    Log.LogMessage(String.Format("   !!No matching prefix for \"{0}\"", fullKey));
            }

            KeyData keyData = new KeyData(root, fullKey, name, type, val);
            m_installReg.Add(keyData);
        }

        static int KeyDataComparison(KeyData left, KeyData right)
        {
            int comp = string.Compare(left.m_root, right.m_root);

            if (comp == 0)
            {
                comp = string.Compare(left.m_key, right.m_key);

                if (comp == 0)
                {
                    if (left.m_name != null && right.m_name != null)
                    {
                        comp = string.Compare(left.m_name, right.m_name);
                    }

                    if (comp == 0)
                    {
                        comp = string.Compare(left.m_value, right.m_value);
                    }
                }
            }

            return comp;
        }

        static int KeyDataComparisonShort(KeyData left, KeyData right)
        {
            int comp = string.Compare(left.m_root, right.m_root);

            if (comp == 0)
            {
                comp = string.Compare(left.m_key, right.m_key);

                if (comp == 0)
                {
                    if (left.m_name != null && right.m_name != null)
                    {
                        comp = string.Compare(left.m_name, right.m_name);
                    }
                }
            }

            return comp;
        }

        void Save()
        {
            m_installReg.Sort(new Comparison<KeyData>(KeyDataComparison));

            m_sw.WriteLine(@"SETLOCAL");
            m_sw.WriteLine();
            m_sw.WriteLine("IF \"%PROCESSOR_ARCHITECTURE%\"==\"AMD64\" (IF EXIST \"%SystemRoot%\\SysWOW64\\reg.exe\" SET reg32_exe=%SystemRoot%\\SysWOW64\\reg.exe)");
            m_sw.WriteLine("IF /i \"%reg32_exe%\"==\"\"                  SET reg32_exe=%SPOROOT%\\bin\\reg32.exe");
            m_sw.WriteLine(@"");
            m_sw.WriteLine(@"");
            m_sw.WriteLine("IF /i \"%1\"==\"/i\" (");
            m_sw.WriteLine(@"   CALL :INSTALL");
            m_sw.WriteLine(@"   GOTO the_end");
            m_sw.WriteLine(@") ELSE (");
            m_sw.WriteLine("	IF /i \"%1\"==\"/u\" (");
            m_sw.WriteLine(@"       CALL :UNINSTALL");
            m_sw.WriteLine(@"       GOTO the_end");
            m_sw.WriteLine(@"   )");
            m_sw.WriteLine(@")");
            m_sw.WriteLine();
            m_sw.WriteLine(@":usage");
            m_sw.WriteLine(@"ECHO usage: %0 [/u ^| /i]");
            m_sw.WriteLine(@"GOTO the_end");
            m_sw.WriteLine();
            m_sw.WriteLine(":INSTALL");

            for( int f=0; f<m_installReg.Count; f++)
            {
                KeyData kd = m_installReg[f];
                if (f + 1 < m_installReg.Count)
                {
                    if (0 == KeyDataComparisonShort(kd, m_installReg[f + 1]))
                    {
                        continue;
                    }
                }

                string installString;

                if (string.IsNullOrEmpty(kd.m_name))
                {
                    installString = string.Format(regAddEmptyNameFormat, kd.m_key, kd.m_type, kd.m_value);
                }
                else
                {
                    installString = string.Format(regAddFormat, kd.m_key, kd.m_name, kd.m_type, kd.m_value);
                }
                m_sw.WriteLine("ECHO " + installString);
                m_sw.WriteLine(installString);
            }

            m_sw.WriteLine(@"ECHO Installing the reference assembly list");
            foreach (string dir in Directory.GetDirectories(Path.Combine(Environment.GetEnvironmentVariable("SPOCLIENT"), @"framework\IDE\Targets")))
            {
                DirectoryInfo di = new DirectoryInfo(dir);

                if (Directory.Exists(Path.Combine(dir, @"Assemblies\RedistList\")))
                {
                    m_sw.WriteLine("IF EXIST \"%ProgramFiles%\\Reference Assemblies\\Microsoft\\Framework\"      call xcopy /y " + Path.Combine(dir, @"Assemblies\*.dll"           ) + " \"%ProgramFiles%\\Reference Assemblies\\Microsoft\\Framework\\.NETMicroFramework\\"      + di.Name + "\\*\"");
                    m_sw.WriteLine("IF EXIST \"%ProgramFiles(x86)%\\Reference Assemblies\\Microsoft\\Framework\" call xcopy /y " + Path.Combine(dir, @"Assemblies\*.dll"           ) + " \"%ProgramFiles(x86)%\\Reference Assemblies\\Microsoft\\Framework\\.NETMicroFramework\\" + di.Name + "\\*\"");

                    m_sw.WriteLine("IF EXIST \"%ProgramFiles%\\Reference Assemblies\\Microsoft\\Framework\"      call xcopy /y " + Path.Combine(dir, @"Assemblies\RedistList\*.xml") + " \"%ProgramFiles%\\Reference Assemblies\\Microsoft\\Framework\\.NETMicroFramework\\"      + di.Name + "\\RedistList\\*.xml\"");
                    m_sw.WriteLine("IF EXIST \"%ProgramFiles(x86)%\\Reference Assemblies\\Microsoft\\Framework\" call xcopy /y " + Path.Combine(dir, @"Assemblies\RedistList\*.xml") + " \"%ProgramFiles(x86)%\\Reference Assemblies\\Microsoft\\Framework\\.NETMicroFramework\\" + di.Name + "\\RedistList\\*.xml\"");
                }
                else
                {
                    m_sw.WriteLine("IF EXIST \"%ProgramFiles%\\Reference Assemblies\\Microsoft\\Framework\"      call xcopy /y %build_tree_client%\\dll\\*.dll             \"%ProgramFiles%\\Reference Assemblies\\Microsoft\\Framework\\.NETMicroFramework\\"      + di.Name + "\\*\"");
                    m_sw.WriteLine("IF EXIST \"%ProgramFiles(x86)%\\Reference Assemblies\\Microsoft\\Framework\" call xcopy /y %build_tree_client%\\dll\\*.dll             \"%ProgramFiles(x86)%\\Reference Assemblies\\Microsoft\\Framework\\.NETMicroFramework\\" + di.Name + "\\*\"");

                    m_sw.WriteLine("IF EXIST \"%ProgramFiles%\\Reference Assemblies\\Microsoft\\Framework\"      call xcopy /y %build_tree_client%\\dll\\redistlist\\*.xml \"%ProgramFiles%\\Reference Assemblies\\Microsoft\\Framework\\.NETMicroFramework\\"      + di.Name + "\\RedistList\\*.xml\"");
                    m_sw.WriteLine("IF EXIST \"%ProgramFiles(x86)%\\Reference Assemblies\\Microsoft\\Framework\" call xcopy /y %build_tree_client%\\dll\\redistlist\\*.xml \"%ProgramFiles(x86)%\\Reference Assemblies\\Microsoft\\Framework\\.NETMicroFramework\\" + di.Name + "\\RedistList\\*.xml\"");
                }
            }

            m_sw.WriteLine(@"ECHO Installing the Item/Project templates");
            m_sw.WriteLine("call xcopy /y %build_tree_server%\\templates\\ProjectTemplates\\CSharp\\MicroFramework\\*.zip         \"%DevEnvDir%ProjectTemplates\\CSharp\\Micro Framework\\\""         );
            m_sw.WriteLine("call xcopy /y %build_tree_server%\\templates\\ProjectTemplates\\VisualBasic\\MicroFramework\\*.zip    \"%DevEnvDir%ProjectTemplates\\VisualBasic\\Micro Framework\\\""    );
            m_sw.WriteLine("call xcopy /y %build_tree_server%\\templates\\ItemTemplates\\CSharp\\MicroFramework\\AssemblyInfo.zip \"%DevEnvDir%ItemTemplates\\CSharp\\Micro Framework\\General\\\""   );
            m_sw.WriteLine("call xcopy /y %build_tree_server%\\templates\\ItemTemplates\\CSharp\\MicroFramework\\Class.zip        \"%DevEnvDir%ItemTemplates\\CSharp\\Micro Framework\\Code\\\""      );
            m_sw.WriteLine("call xcopy /y %build_tree_server%\\templates\\ItemTemplates\\VisualBasic\\MicroFramework\\Class.zip   \"%DevEnvDir%ItemTemplates\\VisualBasic\\Micro Framework\\Code\\\"" );

            m_sw.WriteLine(@"ECHO Running VS Setup (Hold still this may take a while)");
            m_sw.WriteLine(@"call devenv.exe /setup");

            m_sw.WriteLine(@"goto :EOF");
            m_sw.WriteLine();
            m_sw.WriteLine(":UNINSTALL");

            for (int i = m_installReg.Count - 1; i >= 0; i--)
            {
                KeyData kd = m_installReg[i];

                string uninstallString;

                if (i > 0)
                {
                    if (0 == KeyDataComparisonShort(kd, m_installReg[i - 1]))
                    {
                        continue;
                    }
                }

                if (string.IsNullOrEmpty(kd.m_name))
                {
                    if (string.IsNullOrEmpty(kd.m_value))
                    {
                        uninstallString = string.Format(regDelKeyFormat, kd.m_key);
                    }
                    else
                    {
                        uninstallString = string.Format(regDelEmptyNameFormat, kd.m_key);
                    }
                }
                else
                {
                    uninstallString = string.Format(regDelFormat, kd.m_key, kd.m_name);
                }
                m_sw.WriteLine("ECHO " + uninstallString);
                m_sw.WriteLine(uninstallString);
            }

            m_sw.WriteLine(@"ECHO Uninstalling the reference assembly list");
            m_sw.WriteLine("rd /s /q \"%ProgramFiles%\\Reference Assemblies\\Microsoft\\Framework\\.NETMicroFramework\""     );
            m_sw.WriteLine("rd /s /q \"%ProgramFiles(x86)%\\Reference Assemblies\\Microsoft\\Framework\\.NETMicroFramework\"");

            m_sw.WriteLine(@"ECHO Uninstalling the Item/Project templates");
            m_sw.WriteLine("rd /s /q \"%DevEnvDir%ProjectTemplates\\CSharp\\Micro Framework\""     );
            m_sw.WriteLine("rd /s /q \"%DevEnvDir%ProjectTemplates\\VisualBasic\\Micro Framework\"");
            m_sw.WriteLine("rd /s /q \"%DevEnvDir%ItemTemplates\\CSharp\\MicroFramework\""         );
            m_sw.WriteLine("rd /s /q \"%DevEnvDir%ItemTemplates\\VisualBasic\\MicroFramework\""    );

            m_sw.WriteLine(@"ECHO Running VS Setup (Hold still this may take a while)");
            m_sw.WriteLine(@"call devenv.exe /setup");

            m_sw.WriteLine(@"goto :EOF");
            m_sw.WriteLine();
            m_sw.WriteLine(@":the_end");
            m_sw.WriteLine(@"ENDLOCAL");
        }

        void Init()
        {
            m_db = new Database( m_installerFile.ItemSpec, OpenDatabase.ReadOnly);

            m_sw = new StreamWriter(m_outputScript.ItemSpec);

            m_sw.AutoFlush = true;

            m_sw.WriteLine("@if /i \"%__echo_on%\" == \"\" echo off");
            m_sw.WriteLine(@"rem ###########################################################################");
            m_sw.WriteLine(@"rem AUTOGENERATED CODE BY Microsoft.SPOT.UtilityTasks.CreatePseudoInstallScript");
            m_sw.WriteLine(@"rem                      --- DO NOT MODIFY ---");
            m_sw.WriteLine(@"rem ");
            m_sw.WriteLine(@"rem Regenerate this script when you create a fresh MSI:");
            m_sw.WriteLine(@"rem C:\spo\client\> msbuild dotNetMF.proj /t:PrepForVs");
            m_sw.WriteLine(@"rem ###########################################################################");
            m_sw.WriteLine();
            m_sw.WriteLine();

            m_installReg = new List<KeyData>();
        }

        public override bool Execute()
        {
            try
            {
                this.Init();
                this.Create();
                this.Raw();
                this.Save();
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError("CreatePseudoInstallScript error: " + ex.Message + " At: " + ex.StackTrace);
            }
            return false;
        }

    }

}
