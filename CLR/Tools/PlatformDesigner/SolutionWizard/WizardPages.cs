using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using ComponentObjectModel;
using XsdInventoryFormatObject;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Reflection;
using System.Configuration;

namespace SolutionWizard
{
    public partial class SolutionWizardForm : Form
    {
        Inventory m_inv;
        InventoryHelper m_helper;
        MsBuildWrapper m_bw;
        string m_spoClientPath;
        MFSolution m_solution = null;
        MFSolution m_solutionOpened = null;
        bool m_IsSolutionCloned = false;
        bool m_HasClrProj = false;
        List<MFComponentDescriptor> m_solutionRefs;
        MFComponentDescriptor m_selectedSolution = null;
        bool m_Finished = false;

        string m_prevSolutionGuid = "";

        BackgroundWorker m_worker;


        internal enum ConditionIndex
        {
            NoCondition = 0,
            RTM,
            NotRTM,
            Other,
        };

        private ConditionIndex ConvertConditionStringToIndex(string condition)
        {
            if (condition == null) return ConditionIndex.NoCondition;

            condition = condition.ToLower();

            if (condition.Contains("'$(flavor)'=='debug'"))
            {
                return ConditionIndex.NotRTM;
            }
            else if (condition.Contains("'$(flavor)'=='release'"))
            {
                return ConditionIndex.NotRTM;
            }
            else if (condition.Contains("'$(flavor)'=='rtm'"))
            {
                return ConditionIndex.RTM;
            }
            else if (condition.Contains("'$(flavor)'!='rtm'"))
            {
                return ConditionIndex.NotRTM;
            }

            return ConditionIndex.NoCondition;
        }

        private string ConvertConditionIndexToString(ConditionIndex index)
        {
            string cond = "";

            switch(index)
            {
                case ConditionIndex.RTM:
                    cond = "'$(FLAVOR)'=='" + Enum.GetName(typeof(ConditionIndex), index) + "'";
                    break;

                case ConditionIndex.NotRTM:
                    cond = "'$(FLAVOR)'!='RTM'";
                    break;

                case ConditionIndex.NoCondition:
                default:
                    cond = cbLibraryCond.Text;
                    break;
            }

            return cond;
        }

        internal class ComponentTreeNode
        {
            internal ComponentTreeNode(MFComponent comp, bool check)
            {
                Visible = true;
                Checked = check;
                Comp = comp;

                if (check)
                {
                    Comp.RefCount++;
                }
            }

            //internal TreeNode Node;
            internal ComponentTreeNode Parent = null;
            internal List<ComponentTreeNode> Dependents   = new List<ComponentTreeNode>();
            internal List<ComponentTreeNode> Dependencies = new List<ComponentTreeNode>();
            internal bool Visible;
            internal bool Checked;
            internal MFComponent Comp;
        }

        public class ProjectComboData
        {
            internal ProjectComboData()
            {
            }
            internal ProjectComboData(string name, MFProject proj)
            {
                Name = name;
                Proj = proj;
                CompLookup = new Dictionary<string,ComponentTreeNode>();
            }
            public string DisplayName { get { return Name; } set { Name = value; } }
            internal string Name = "";
            internal MFProject Proj;
            internal Dictionary<string, ComponentTreeNode> CompLookup;
        }

        public SolutionWizardForm()
        {
            InitializeComponent();

            m_inv = new Inventory();
            m_helper = new InventoryHelper(m_inv);
            m_bw = new MsBuildWrapper(m_inv);
            m_spoClientPath = "";
            m_worker = new BackgroundWorker();
            m_worker.DoWork += new DoWorkEventHandler(m_worker_DoWork);
            m_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(m_worker_RunWorkerCompleted);
        }

        enum BackgroundWorkerType
        {
            LoadFeatures,
            LoadSolutions,
            LoadSolution,
            LoadProjects,
            LoadLibraries,
            LoadProcessors,
        };

        void m_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            switch ((BackgroundWorkerType)e.Result)
            {
                case BackgroundWorkerType.LoadFeatures:
                    TreeNode root = tv_FeatureView.Nodes.Add(Properties.Resources.FeatureList);

                    foreach (Feature f in m_inv.Features)
                    {
                        if (!f.Required && f.IsSolutionWizardVisible)
                        {
                            TreeNode fn = root.Nodes.Add(f.Guid.ToLower(), f.Name);
                            fn.Tag = new MFComponent(MFComponentType.Feature, f.Name, f.Guid, f.ProjectPath);
                        }
                    }
                    tv_FeatureView.Sort();
                    tv_FeatureView.SelectedNode = root;
                    tv_FeatureView.ExpandAll();

                    LoadProjectFeatureData();
                    rtb_FeatureDescription.Text = "";
                    if (cbProjectSelect_Feature.Items.Count > 0)
                    {
                        cbProjectSelect_Feature.SelectedIndex = 0;
                        cbProjectSelect_Feature.Enabled = true;
                    }
                    cbProjectSelect_SelectedIndexChanged(null, null);
                    Wiz.NextEnabled = true;
                    Wiz.BackEnabled = true;
                    break;

                case BackgroundWorkerType.LoadSolution:
                    if (Wiz.Page == wpSolutionCfg)
                    {
                        wpSolutionCfg_UpdateView();
                        Wiz.NextEnabled = true;
                    }
                    else
                    {
                        tb_PlatformDescription.Text = "";
                        tb_PlatformDescription.Enabled = true;
                        Wiz.NextEnabled = !String.IsNullOrEmpty(tb_PlatformName.Text);
                    }
                    
                    Wiz.BackEnabled = true;
                    break;

                case BackgroundWorkerType.LoadProcessors:
                    cbProcessor.Items.Clear();
                    cbProcessor.DisplayMember = "Name";

                    foreach (Processor prc in m_inv.Processors)
                    {
                        if (prc.IsSolutionWizardVisible)
                        {
                            cbProcessor.Items.Add(prc);
                        }
                    }
                    cbProcessor.Focus();
                    cbProcessor.Enabled = true;
                    Wiz.NextEnabled = true;
                    Wiz.BackEnabled = true;
                    break;
                case BackgroundWorkerType.LoadProjects:
                    TreeNode tnNative = tv_SelectedProjects.Nodes.Add("NativeProjects", Properties.Resources.NativeProjects); //create root for the treeview
                    TreeNode tnCLR = tv_SelectedProjects.Nodes.Add("ClrProjects", Properties.Resources.CLRProjects); //create root for the treeview

                    Dictionary<string, TreeNode> projLookup = new Dictionary<string, TreeNode>();

                    foreach (MFProject proj in m_inv.ProjectTemplates)
                    {
                        if (proj.IsSolutionWizardVisible)
                        {
                            TreeNode tn;
                            if (proj.IsClrProject)
                            {
                                tn = tnCLR.Nodes.Add(proj.Name.ToLower(), proj.Name);
                                tn.Tag = proj;
                            }
                            else
                            {
                                tn = tnNative.Nodes.Add(proj.Name.ToLower(), proj.Name);
                                tn.Tag = proj;
                            }

                            projLookup[proj.Name.ToLower()] = tn;
                        }
                    }
                    if (m_solution.m_cloneSolution != null)
                    {
                        foreach (MFProject prj in m_solution.m_cloneSolution.Projects)
                        {
                            if (projLookup.ContainsKey(prj.Name.ToLower()))
                            {
                                projLookup[prj.Name.ToLower()].Checked = true;
                            }
                        }
                    }

                    tv_SelectedProjects.Sort();
                    tnNative.Expand(); //expand tree to display all nodes
                    tnCLR.Expand();

                    wpChooseProjects_UpdateProjects();

                    Wiz.NextEnabled = true;
                    rtb_ProjectDescription.Text = "";
                    Wiz.BackEnabled = true;
                    break;
                case BackgroundWorkerType.LoadSolutions:
                    TreeNode tnRootS = tv_PlatformList.Nodes.Add(Properties.Resources.Solutions, Properties.Resources.SolutionList);

                    foreach (MFComponentDescriptor sol in m_solutionRefs)
                    {
                        TreeNode child = tnRootS.Nodes.Add(sol.Component.Guid, sol.Component.Name);
                        child.Tag = sol;
                    }

                    tv_PlatformList.Sort();
                    tv_PlatformList.ExpandAll();

                    rtb_PlatformFeatures.Text = "";
                    Wiz.NextEnabled = tv_PlatformList.SelectedNode != null;
                    Wiz.BackEnabled = true;
                    break;
                case BackgroundWorkerType.LoadLibraries:
                    LoadAllProjectLibraryData();
                   
                    ShowLibraryCategories(cbShowAllLibCatChoices.Checked);

                    cbProjectSelect_Library.SelectedIndex = 0;
                    rtb_LibraryDescription.Text = "";
                    cbProjectSelect_Library.Enabled = true;
                    cbShowAllLibCatChoices.Enabled = true;
                    Wiz.NextEnabled = true;
                    Wiz.BackEnabled = true;
                    break;
            }

            this.UseWaitCursor = false;
            // invalidate to make the cursor change from the wait cursor
            this.Invalidate();
        }

        void m_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            lock (m_bw)
            {
                switch((BackgroundWorkerType)e.Argument)
                {
                    case BackgroundWorkerType.LoadFeatures:
                        if (m_solution == null && m_selectedSolution != null)
                        {
                            m_solution = m_bw.LoadSolutionProj(m_selectedSolution.Component.ProjectPath, "");
                        }
                        if (IsWindowsSolution(m_solution))
                        {
                            m_bw.LoadDefaultLibraries(m_spoClientPath);
                            m_bw.LoadDefaultLibraryCategories(m_spoClientPath);
                            //m_bw.LoadDefaultManifestFiles(m_spoClientPath);
                        }
                        m_bw.LoadDefaultFeatures(m_spoClientPath);
                        break;
                    case BackgroundWorkerType.LoadSolution:
                        m_solution = m_bw.LoadSolutionProj(m_selectedSolution.Component.ProjectPath, "");
                        m_bw.LoadLibraries( m_solution.ProjectPath);
                        m_bw.LoadDefaultLibraryCategories(m_spoClientPath);
                        break;
                    case BackgroundWorkerType.LoadProjects:
                        if (m_inv.ProjectTemplates.Count == 0)
                        {
                            m_bw.LoadTemplateProjects(m_spoClientPath + "\\ProjectTemplates");
                        }
                        break;
                    case BackgroundWorkerType.LoadSolutions:
                        m_solutionRefs = m_bw.GetAvailableSolutions(m_spoClientPath);
                        break;
                    case BackgroundWorkerType.LoadLibraries:
                        m_bw.LoadDefaultLibraries(m_spoClientPath);
                        //m_bw.LoadDefaultManifestFiles(m_spoClientPath);
                        m_bw.LoadDefaultLibraryCategories(m_spoClientPath);
                        m_bw.LoadDefaultFeatures(m_spoClientPath);
                        break;
                    case BackgroundWorkerType.LoadProcessors:
                        m_bw.LoadProcessors(m_spoClientPath + "\\DeviceCode\\Targets\\Native");
                        m_bw.LoadProcessors(m_spoClientPath + "\\DeviceCode\\Targets\\OS");
                        break;
                }
            }
            e.Result = e.Argument;
        }

        private void wpChooseTask_CloseFromNext(object sender, Gui.Wizard.PageEventArgs e)
        {
            if (rbCreatePlatform.Checked == true)
                e.Page = wpCreatePlatform;
            else
                e.Page = wpSelectExisting;
        }

        private void rtb_FeatureDescription_VisibleChanged(object sender, EventArgs e)
        {
            rtb_FeatureDescription.Clear();
        }

        private void wp_CreatePlatform_ShowFromNext(object sender, EventArgs e)
        {
            if (m_solution == null && m_selectedSolution != null)
            {
                tb_PlatformDescription.Enabled = false;
                Wiz.NextEnabled = false;
                Wiz.BackEnabled = false;

                this.UseWaitCursor = true;

                tb_PlatformDescription.Text = Properties.Resources.LoadingClonedSolution;
                m_worker.RunWorkerAsync(BackgroundWorkerType.LoadSolution);
            }
            else
            {
                Wiz.NextEnabled = (tb_PlatformName.Text.Length > 0);
            }

            tb_PlatformName.Focus();
        }

        private void tb_PlatformName_TextChanged(object sender, EventArgs e)
        {
            if (tb_PlatformName.Text.Length > 0 && !m_worker.IsBusy)
            {
                Wiz.NextEnabled = true;
            }
            else
            {
                Wiz.NextEnabled = false;
            }
        }
        
        private void GenerateSolution()
        {
            try
            {
                if (string.IsNullOrEmpty(m_solution.ProjectPath))
                {
                    m_solution.ProjectPath = "$(SPOCLIENT)\\Solutions\\" + m_solution.Name + "\\" + m_solution.Name + ".settings";
                }

                //
                // Copy the cloned solution's device code and projects
                //
                if (m_solution.m_cloneSolution != null)
                {
                    string oldP = m_spoClientPath + "\\Solutions\\" + m_solution.m_cloneSolution.Name + "\\DeviceCode\\";
                    string newP = m_spoClientPath + "\\Solutions\\" + m_solution.Name                 + "\\DeviceCode\\";


                    m_bw.CopyClonedFiles(oldP, newP, m_solution.m_cloneSolution.Name, m_solution.Name);
                }

                Dictionary<string, MFComponent> genLookup = new Dictionary<string, MFComponent>();

                foreach (TemplateGenerationData tgd in m_generateComponentList)
                {
                    string key = tgd.LibraryCat.Guid.ToLower();

                    MFComponent cmpGen = null;

                    if (!genLookup.ContainsKey(key))
                    {
                        bool fCopied = false;
                        if (m_solution.m_cloneSolution != null)
                        {
                            //
                            // Set the CLR Project to be the default project if it exists
                            //
                            MFProject defProj = m_solution.m_cloneSolution.Projects[0];

                            foreach (MFProject prj in m_solution.m_cloneSolution.Projects)
                            {
                                if (prj.IsClrProject)
                                {
                                    defProj = prj;
                                }
                            }

                            foreach (MFComponent cmpClone in defProj.Libraries)
                            {
                                Library libClone = m_helper.FindLibrary(cmpClone);

                                if (libClone != null && 0 == string.Compare(libClone.LibraryCategory.Guid, tgd.LibraryCat.Guid, true))
                                {
                                    string fullpath = MsBuildWrapper.ExpandEnvVars(cmpClone.ProjectPath, "");
                                    string pathOld = Path.GetDirectoryName(fullpath);
                                    string pathNew = fullpath.Replace("\\" + m_solution.m_cloneSolution.Name + "\\", "\\" + m_solution.Name + "\\");

                                    m_bw.CopyClonedFiles(pathOld, Path.GetDirectoryName(pathNew), m_solution.m_cloneSolution.Name, m_solution.Name);
                                    tgd.Comp.ProjectPath =  m_bw.ConvertPathToEnv(pathNew);
                                    m_bw.LoadLibraryProj(pathNew, "", true);

                                    fCopied = true;
                                    break;
                                }
                            }
                        }
                        if(!fCopied)
                        {
                            m_bw.CopyTemplateFiles(tgd.LibraryCat, m_solution, tgd.Comp);
                        }


                        genLookup[key] = tgd.Comp;
                    }
                    else
                    {
                        cmpGen = genLookup[key];

                        cmpGen.CopyTo(tgd.Comp);
                    }
                }

                m_bw.SaveSolutionProj(m_solution);

                m_bw.CreateSolutionDirProj(m_solution);

                foreach (MFProject proj in m_solution.Projects)
                {
                    if (m_pcdAll != null && proj == m_pcdAll.Proj) continue;

                    m_bw.SaveProjectProj(proj);

                    m_bw.CopyProjFilesFromClone(proj, m_solution);
                }

                string logFile = m_spoClientPath + "\\Solutions\\" + m_solution.Name + "\\SolutionSummary.txt";

                using (TextWriter tw = File.CreateText(logFile))
                {
                    tw.Write(rtb_Finish.Text.Replace("\n", "\r\n"));
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, "An error occured during the creation of the Solution.  The Solution may not build properly. " + ex.ToString(), Properties.Resources.SolutionWizard, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Wiz.NextEnabled = true;
        }

        private void AddPropLine(StringBuilder sb, string name, string value)
        {
            sb.Append(string.Format("    {0,-35}\t {1}\n", name + ":", value));
        }

        private void rb_SelectFromList_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_SelectFromList.Checked)
            {
                tv_PlatformList.Enabled = true;
            }
            else
            {
                tv_PlatformList.Enabled = false;
            }
        }

        private void wpCreatePlatform_CloseFromBack(object sender, Gui.Wizard.PageEventArgs e)
        {
            if (rbCreatePlatform.Checked)
            {
                e.Page = wpChooseTask;
            }
        }

        private void Wiz_Load(object sender, EventArgs e)
        {
            cbRuntimeMemCfg.SelectedIndex = 2;
        }

        private void buttonSpoClientBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlgOpen = new FolderBrowserDialog();

            // set properties for the dialog
            dlgOpen.ShowNewFolderButton = false;
            dlgOpen.Description = Properties.Resources.SelectMfPkDirectory;

            if (dlgOpen.ShowDialog() == DialogResult.OK)
            {
                textBoxSpoClientPath.Text = dlgOpen.SelectedPath;
            }
        }

        private void ipWelcome_Load(object sender, EventArgs e)
        {
            string spoclient = GetPortingKitRegistryValue("", "InstallRoot");

            if (string.IsNullOrEmpty(spoclient) || !Directory.Exists(spoclient))
            {
                spoclient = System.Environment.GetEnvironmentVariable("SPOCLIENT");
            }

            if (!string.IsNullOrEmpty(spoclient) && Directory.Exists(spoclient))
            {
                textBoxSpoClientPath.Text = spoclient;
            }


            pidLabel.Text = "Product ID: " + GetProductId();
        }

        private string GetPortingKitRegistryValue(string keySubPath, string key)
        {
            try
            {
                // Get the version of the executing assembly (that is, this assembly).
                Assembly asm = Assembly.GetEntryAssembly();
                AssemblyName asmName = asm.GetName();
                System.Version appVersion = asmName.Version;

                // Rooted at HKLM\Software\Microsoft\.NETMicroFrameworkPortingKit\v$(var.ProdVer)\Registration\DigitalProductID\, 
                // the PKConfig outputs are four values:
                // ProductID, a 20-digit number stored as a string. 

                using (RegistryKey pkKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\.NETMicroFrameworkPortingKit"))
                {
                    if (pkKey == null)
                        return c_xpid;

                    /// Logic here is to find the highest version where the major matches with
                    /// this applications major. In case no such version could be found, just
                    /// use highest version available (even if that is with a different major).
                    System.Version maxVersion = new System.Version(0, 0, 0, 0);
                    System.Version maxVersionWithSameMajor = new System.Version(0, 0, 0, 0);

                    foreach (string keyName in pkKey.GetSubKeyNames())
                    {
                        if (keyName.Length < 2)
                            continue;

                        string versionStr = keyName.Substring(1);
                        System.Version keyVersion = new System.Version(versionStr);

                        if (keyVersion.Major == appVersion.Major)
                        {
                            if (keyVersion > maxVersionWithSameMajor)
                            {
                                maxVersionWithSameMajor = keyVersion;
                            }
                        }

                        if (keyVersion > maxVersion)
                        {
                            maxVersion = keyVersion;
                        }
                    }

                    string maxVersionKeyName = null;
                    if (maxVersionWithSameMajor.Major != 0)
                    {
                        maxVersionKeyName = "v" + maxVersionWithSameMajor.ToString();
                    }
                    else if (maxVersion.Major != 0)
                    {
                        maxVersionKeyName = "v" + maxVersion.ToString();
                    }

                    if (!String.IsNullOrEmpty(maxVersionKeyName))
                    {
                        string pidKeyName = maxVersionKeyName + "\\" + keySubPath;
                        using (RegistryKey pidKey = pkKey.OpenSubKey(pidKeyName))
                        {
                            if (pidKey == null)
                                return c_xpid;

                            return (string)pidKey.GetValue( key );
                        }
                    }
                }
            }
            catch (FormatException)
            {
            }
            catch (ArgumentException)
            {
            }

            return null;

        }

        private const string c_xpid = "xxxxx-xxx-xxxxxxx-xxxxx";
        private string GetProductId()
        {
            string pid = GetPortingKitRegistryValue(@"Registration\DigitalProductID", "ProductID");

            if (string.IsNullOrEmpty(pid))
            {
                pid = c_xpid;
            }

            return pid;
        }

        private void wpWelcome_CloseFromNext(object sender, Gui.Wizard.PageEventArgs e)
        {
            m_spoClientPath = textBoxSpoClientPath.Text;
            System.Environment.SetEnvironmentVariable("SPOCLIENT", m_spoClientPath);

            if (!Directory.Exists(m_spoClientPath) || !Directory.Exists(m_spoClientPath + "\\DeviceCode\\Targets\\"))
            {
                MessageBox.Show(this, Properties.Resources.InvalidPkDirectory, Properties.Resources.SolutionWizard, MessageBoxButtons.OK, MessageBoxIcon.Error);

                textBoxSpoClientPath.Select(0, -1);

                e.Page = wpWelcome;
            }
        }

        private bool IsWindowsSolution(MFSolution solution)
        {
            return (0 == string.Compare(solution.Processor.Name, "windows", true));
        }

        private void wpCreatePlatform_CloseFromNext(object sender, Gui.Wizard.PageEventArgs e)
        {
            MFSolution sol = null;

            Regex expUnsupportedChars = new Regex("[^\\w\\d_]+");

            if (expUnsupportedChars.IsMatch(tb_PlatformName.Text))
            {
                MessageBox.Show(this, Properties.Resources.InvalidSolutionName, Properties.Resources.SolutionWizard, MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Page = wpCreatePlatform;
                return;
            }

            if (m_IsSolutionCloned && (m_solution == null || m_solution.m_cloneSolution == null))
            {
                sol = new MFSolution();

                m_solution.CopyTo(sol, tb_PlatformName.Text);
            }
            else if (m_solution != null)
            {
                sol = m_solution;
            }
            else
            {
                sol = new MFSolution();
            }

            sol.Name = tb_PlatformName.Text;
            sol.Description = tb_PlatformDescription.Text;
            sol.Author = tb_SolutionAuthor.Text;

            m_solution = sol;

            if (m_selectedSolution != null && IsWindowsSolution(m_solution))
            {
                Processor proc = m_bw.LoadProcessorProj(m_spoClientPath + "\\devicecode\\Targets\\OS\\Windows\\Windows.settings", "");
                if(proc != null)
                {
                    m_solution.Processor = new MFComponent(MFComponentType.Processor, proc.Name, proc.Guid, proc.ProjectPath);
                }

                m_solution.Projects[0].Name = m_solution.Name;

                e.Page = wpChooseFeatures;
            }


            Wiz.NextEnabled = true;
        }

        private void wpChooseProjects_UpdateProjects()
        {
            if (m_solution != null)
            {
                foreach (MFProject proj in m_solution.Projects)
                {
                    TreeNode[] items = tv_SelectedProjects.Nodes.Find(proj.Name.ToLower(), true);

                    if (items != null && items.Length > 0)
                    {
                        items[0].Checked = true;
                    }
                }
            }
        }

        private void wpChooseProjects_ShowFromNext(object sender, EventArgs e)
        {
            //Wiz.NextEnabled = (tv_SelectedProjects.SelectedNode != null);

            if (!m_worker.IsBusy)
            {
                if (tv_SelectedProjects.Nodes.Count == 0)
                {
                    Wiz.NextEnabled = false;
                    Wiz.BackEnabled = false;

                    this.UseWaitCursor = true;

                    rtb_ProjectDescription.Text = Properties.Resources.LoadingProjects;

                    m_worker.RunWorkerAsync(BackgroundWorkerType.LoadProjects);
                }
                else
                {
                    wpChooseProjects_UpdateProjects();
                }
            }
        }

        private void wpChooseProjects_CloseFromBack(object sender, Gui.Wizard.PageEventArgs e)
        {
        }

        private void wpChooseProjects_CloseFromNext(object sender, Gui.Wizard.PageEventArgs e)
        {
            m_HasClrProj = false;

            Dictionary<string, MFProject> projLookup = new Dictionary<string, MFProject>();
            List<MFProject> projRemoveList = new List<MFProject>();

            foreach (MFProject prj in m_solution.Projects)
            {
                projLookup[prj.Name.ToLower()] = prj;
            }
            //m_solution.Projects.Clear();

            string solutionRoot = "$(SPOCLIENT)\\Solutions\\" + m_solution.Name + "\\";

            foreach (TreeNode tn in tv_SelectedProjects.Nodes["NativeProjects"].Nodes)
            {
                MFProject projClone = tn.Tag as MFProject;
                string key = projClone.Name.ToLower();

                if (tn.Checked)
                {
                    if (projLookup.ContainsKey(key))
                    {
                        continue;
                    }

                    MFProject proj = new MFProject();

                    projClone.CopyTo(proj, m_solution.Name);

                    if (proj != null)
                    {
                        proj.ProjectPath = solutionRoot + Path.GetFileName(proj.Directory) + "\\" + Path.GetFileName(proj.ProjectPath);

                        proj.SettingsFile = solutionRoot + m_solution.Name + ".settings";

                        m_solution.Projects.Add(proj);

                        projLookup[key] = proj;
                    }
                }
                else if (projLookup.ContainsKey(key))
                {
                    projRemoveList.Add(projLookup[key]);
                    projLookup.Remove(key);
                }
            }

            foreach (TreeNode tn in tv_SelectedProjects.Nodes["ClrProjects"].Nodes)
            {
                MFProject projClone = tn.Tag as MFProject;
                string key = projClone.Name.ToLower();

                if (tn.Checked)
                {
                    if (projLookup.ContainsKey(key))
                    {
                        m_HasClrProj = true;
                        continue;
                    }

                    MFProject proj = new MFProject();

                    projClone.CopyTo(proj, m_solution.Name);

                    if (proj != null)
                    {
                        proj.ProjectPath = solutionRoot + Path.GetFileName(proj.Directory) + "\\" + Path.GetFileName(proj.ProjectPath);

                        proj.SettingsFile = solutionRoot + m_solution.Name + ".settings";

                        m_solution.Projects.Add(proj);

                        m_HasClrProj = true;

                        projLookup[key] = proj;
                    }
                }
                else if (projLookup.ContainsKey(key))
                {
                    projRemoveList.Add(projLookup[key]);
                    projLookup.Remove(key);
                }
            }

            foreach (MFProject prj in projRemoveList)
            {
                m_solution.Projects.Remove(prj);
            }

            MFProject projAll = new MFProject();
            projAll.Name = Properties.Resources.AllProjects;
            projAll.Guid = m_allProjectsGuid;
            projAll.IsClrProject = true;

            m_solution.Projects.Insert(0, projAll); 

            if (m_solution.m_cloneSolution != null)
            {
                MFProject defProj = null;

                foreach (MFProject prj in m_solution.m_cloneSolution.Projects)
                {
                    string key = prj.Name.ToLower();

                    if (0 == string.Compare(key, "tinyclr"))
                    {
                        defProj = prj;
                    }

                    if (projLookup.ContainsKey(key))
                    {
                        projLookup[key].Features.AddRange(prj.Features);
                        projLookup[key].Libraries.AddRange(prj.Libraries);
                    }
                }

                if (defProj != null)
                {
                    foreach (MFProject proj in m_solution.Projects)
                    {
                        if (proj.IsClrProject && proj.Features.Count == 0)
                        {
                            foreach (MFComponent f in defProj.Features)
                            {
                                MFComponent fCopy = new MFComponent();
                                f.CopyTo(fCopy);
                                proj.Features.Add(fCopy);
                            }
                        }
                        // tinybooter has 1 library to start with
                        if (proj.Libraries.Count < 2)
                        {
                            foreach (MFComponent l in defProj.Libraries)
                            {
                                MFComponent lCopy = new MFComponent();
                                l.CopyTo(lCopy);
                                proj.Libraries.Add(lCopy);
                            }
                        }
                    }
                }
            }

            //tv_LibraryView.Nodes.Clear();
            //cbProjectSelect_Library.Items.Clear();

            if (m_HasClrProj)
            {
                tv_FeatureView.Nodes.Clear();
                cbProjectSelect_Feature.Items.Clear();

                e.Page = wpChooseFeatures;
            }
            else if (m_solution.Projects.Count > 1)
            {
                e.Page = wpChooseLibraries;
            }
            else
            {
                MessageBox.Show(this, Properties.Resources.ErrorNoProjects, Properties.Resources.SolutionWizard, MessageBoxButtons.OK, MessageBoxIcon.Error );
                e.Page = wpChooseProjects;
            }
        }
        private void tv_SelectedProjects_AfterSelect(object sender, TreeViewEventArgs e)
        {
            DisplayDescription(e.Node.Tag, rtb_ProjectDescription);
        }

        private void DisplayDescription(object obj, RichTextBox rtbDesc)
        {
            rtbDesc.Clear();

            string name = "", desc = "", docs = "", project="", extra = "";

            MFComponent comp = obj as MFComponent;
            MFProject proj = obj as MFProject;
            MFComponentDescriptor sol = obj as MFComponentDescriptor;

            if (comp != null)
            {
                switch (comp.ComponentType)
                {
                    case MFComponentType.Feature:
                        Feature f = m_helper.FindFeature(comp.Guid);
                        name = f.Name;
                        desc = f.Description;
                        docs = f.Documentation;
                        project = f.ProjectPath;
                        break;
                    case MFComponentType.Library:
                        Library l = m_helper.FindLibrary(comp);
                        if (l != null)
                        {
                            name = l.Name;
                            desc = l.Description;
                            docs = l.Documentation;
                            project = l.ProjectPath;
                        }
                        break;
                    case MFComponentType.LibraryCategory:
                        LibraryCategory lc = m_helper.FindLibraryCategory(comp.Guid);
                        name = lc.Name;
                        desc = lc.Description;
                        docs = lc.Documentation;
                        project = lc.ProjectPath;
                        break;
                }
            }
            else if (obj is Feature)
            {
                name = ((Feature)obj).Name;
                desc = ((Feature)obj).Description;
                docs = ((Feature)obj).Documentation;
                project = ((Feature)obj).ProjectPath;
            }
            else if (proj != null)
            {
                name = proj.Name;
                desc = proj.Description;
                docs = proj.Documentation;
                project = proj.ProjectPath;
            }
            else if (sol != null)
            {
                name = sol.Component.Name;
                desc = sol.Description;
                docs = sol.Documentation;
                project = sol.ProjectPath;
                extra = "Processor Type: " + sol.SolutionProcessor.Name;
            }

            if (name != "")
            {
                rtbDesc.AppendText(Properties.Resources.Name + ": ");
                rtbDesc.AppendText(name + "\n\n");
                rtbDesc.AppendText(Properties.Resources.Description + ":\n");
                rtbDesc.AppendText(desc + "\n\n");
                if (docs.Length > 0)
                {
                    rtbDesc.AppendText(Properties.Resources.Documentation + ":\n");
                    rtbDesc.AppendText(docs + "\n\n");
                }
                rtbDesc.AppendText(Properties.Resources.ProjectPath + ":\n");
                rtbDesc.AppendText(project + "\n\n");

                if (extra.Length > 0)
                {
                    rtbDesc.AppendText(extra);
                }
            }
            else
            {
                rtbDesc.AppendText(Properties.Resources.SelectToViewDescr);
            }
        }

        private bool ParseHexInt(string text, out int val)
        {
            bool fRet = true;

            val = 0;

            text = text.Trim().ToLower();

            try
            {
                if (text.StartsWith("0x"))
                {
                    text = text.Remove(0, 2);

                    val = int.Parse(text, System.Globalization.NumberStyles.HexNumber);
                }
                else
                {
                    val = int.Parse(text);
                }
            }
            catch
            {
                fRet = false;
            }

            return fRet;
        }

        private bool SolutionCfg_ValidateData(out int ramBase, out int ramSize, out int flashBase, out int flashSize, out int clockSpeed, out int slowClock)
        {
            ramBase = 0;
            ramSize = 0;
            flashBase = 0;
            flashSize = 0;
            clockSpeed = 0;
            slowClock = 0;

            return ((cbProcessor.SelectedIndex != -1) && (tbRamBase.Text.Length) > 0 && (tbRamSize.Text.Length > 0) &&
                (tbFlashBase.Text.Length > 0) && (tbFlashSize.Text.Length > 0) && (tbClockSpeed.Text.Length > 0) &&
                ParseHexInt(tbFlashBase.Text, out flashBase) && ParseHexInt(tbFlashSize.Text, out flashSize) &&
                ParseHexInt(tbRamBase.Text, out ramBase) && ParseHexInt(tbRamSize.Text, out ramSize) && 
                ParseHexInt(tbClockSpeed.Text, out clockSpeed) && ParseHexInt(tbSlowClock.Text, out slowClock)
                );
        }

        private void wpSolutionCfg_CloseFromNext(object sender, Gui.Wizard.PageEventArgs e)
        {
            int ramBase, ramSize, flashBase, flashSize, clockSpeed, slowClock;

            if (SolutionCfg_ValidateData(out ramBase, out ramSize, out flashBase, out flashSize, out clockSpeed, out slowClock))
            {
                Processor prc = cbProcessor.SelectedItem as Processor;

                m_solution.Processor = new MFComponent(MFComponentType.Processor, prc.Name, prc.Guid, prc.ProjectPath);

                // default to COM1
                string dbgPort = "COM1";
                
                ComboBoxItem cbi = cbDbgPort.SelectedItem as ComboBoxItem;

                if(cbi == null)
                {
                    MessageBox.Show(this, Properties.Resources.InvalidData, Properties.Resources.SolutionWizard, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Page = wpSolutionCfg;
                    return;
                }

                string portType = cbi.Name.ToUpper();

                if(portType.StartsWith("USART"))
                {
                    dbgPort = "COM1";
                }
                else if(portType.StartsWith("USB"))
                {
                    dbgPort = "USB1";
                }
                else if(portType.StartsWith("SOCKETS"))
                {
                    dbgPort = "COM_SOCKET_DBG";
                }

                m_solution.TransportType = cbi._libCat;

                string memProfile = "medium";

                if(cbRuntimeMemCfg.SelectedItem == null)
                {
                    MessageBox.Show(this, Properties.Resources.InvalidData, Properties.Resources.SolutionWizard, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Page = wpSolutionCfg;
                    return;
                }

                switch (((string)cbRuntimeMemCfg.SelectedItem).ToUpper())
                {
                    case "MINIMAL":
                        memProfile = "extrasmall";
                        break;
                    case "SMALL":
                        memProfile = "small";
                        break;
                    case "MEDIUM":
                        memProfile = "medium";
                        break;
                    case "LARGE":
                        memProfile = "large";
                        break;
                }
                m_solution.MemoryProfile = memProfile;
                m_solution.DebuggerPort = dbgPort;
                m_solution.RamBase = ramBase;
                m_solution.RamLength = ramSize;
                m_solution.FlashBase = flashBase;
                m_solution.FlashLength = flashSize;
                m_solution.SystemClockSpeed = clockSpeed;
                m_solution.SlowClockSpeed = slowClock;

                // todo save RAM/FLASH addrs in scatterfiles
            }
            else
            {
                MessageBox.Show(this, Properties.Resources.InvalidData, Properties.Resources.SolutionWizard, MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Page = wpSolutionCfg;
            }
        }

        private void InitializeTransports()
        {
            if (cbDbgPort.Items.Count == 0)
            {
                if (m_helper.LibraryCategories.Length == 0)
                {
                    m_bw.LoadDefaultLibraryCategories(m_spoClientPath);
                }

                foreach (LibraryCategory lc in m_helper.GetTransports())
                {
                    ComboBoxItem cbi = new ComboBoxItem(lc);

                    cbDbgPort.DisplayMember = "DisplayName";
                    int idx = cbDbgPort.Items.Add(cbi);
                }
            }
        }

        private void wpSolutionCfg_UpdateView()
        {
            cbProcessor.Items.Clear();
            cbProcessor.Items.Add(m_helper.FindProcessor(m_solution.Processor.Guid));
            cbProcessor.DisplayMember = "Name";
            cbProcessor.SelectedIndex = 0;
            cbProcessor.Enabled = false;

            switch (m_solution.MemoryProfile.ToLower())
            {
                case "extrasmall":
                    cbRuntimeMemCfg.SelectedIndex = 0;
                    break;
                case "small":
                    cbRuntimeMemCfg.SelectedIndex = 1;
                    break;
                case "medium":
                    cbRuntimeMemCfg.SelectedIndex = 2;
                    break;
                case "large":
                    cbRuntimeMemCfg.SelectedIndex = 3;
                    break;
            }

            string portName = "";
            string dbgPort = m_solution.DebuggerPort.ToUpper();

            if(dbgPort.StartsWith("COM"))
            {
                portName = "USART";
            }
            else if(dbgPort.StartsWith("USB"))
            {
                portName = "USB";
            }
            else if(dbgPort.StartsWith("COM_SOCKET_DBG"))
            {
                portName = "SOCKETS";
            }

            foreach (ComboBoxItem cbi in cbDbgPort.Items)
            {
                if (cbi.Name == portName)
                {
                    cbDbgPort.SelectedItem = cbi;
                    break;
                }
            }

            cbDbgPort.Enabled = !rbEditSolution.Checked;

            tbClockSpeed.Text = m_solution.SystemClockSpeed.ToString();
            tbSlowClock.Text = m_solution.SlowClockSpeed.ToString();
            tbRamBase.Text = "0x" + m_solution.RamBase.ToString("X08");
            tbRamSize.Text = "0x" + m_solution.RamLength.ToString("X08");
            tbFlashBase.Text = "0x" + m_solution.FlashBase.ToString("X08");
            tbFlashSize.Text = "0x" + m_solution.FlashLength.ToString("X08");
        }

        internal class ComboBoxItem
        {
            internal ComboBoxItem(LibraryCategory lc)
            {
                Name = lc.Name.ToUpper().Replace("_PAL", "");
                _libCat = lc;
            }
            public string Name;
            public string DisplayName
            {
                get { return Name; }
            }
            internal LibraryCategory _libCat;
        }

        private void wpSolutionCfg_ShowFromNext(object sender, EventArgs e)
        {
            if (cbDbgPort.Items.Count == 0) InitializeTransports();

            if (rbCreatePlatform.Checked)
            {
                if (cbProcessor.Items.Count == 0 && !m_worker.IsBusy)
                {
                    Wiz.NextEnabled = false;
                    Wiz.BackEnabled = false;

                    cbProcessor.Items.Add(Properties.Resources.LoadingProcessors);
                    cbProcessor.SelectedIndex = 0;
                    cbProcessor.Enabled = false;

                    this.UseWaitCursor = true;

                    m_worker.RunWorkerAsync(BackgroundWorkerType.LoadProcessors);
                }
            }
            else if (m_solution == null)
            {
                Wiz.NextEnabled = false;
                Wiz.BackEnabled = false;

                cbProcessor.Items.Clear();
                cbProcessor.Items.Add(Properties.Resources.LoadingSolution);
                cbProcessor.SelectedIndex = 0;
                cbProcessor.Enabled = false;

                this.UseWaitCursor = true;

                m_worker.RunWorkerAsync(BackgroundWorkerType.LoadSolution);
            }
            else
            {
                wpSolutionCfg_UpdateView();
            }
        }

        private void wpSolutionCfg_CloseFromBack(object sender, Gui.Wizard.PageEventArgs e)
        {
            if (rbCreatePlatform.Checked || rbCloneSolution.Checked)
            {
                e.Page = wpCreatePlatform;
            }
            else
            {
                e.Page = wpSelectExisting;
            }
        }

        private string ParseProcessorDefaultString(string name, System.Collections.Specialized.StringCollection defaults)
        {
            Regex exp = new Regex(name + "\\s*=\\s*([\\w]+)", RegexOptions.IgnoreCase);

            foreach (string s in defaults)
            {
                Match m = exp.Match(s);

                if (m.Success)
                {
                    return m.Groups[1].Value;
                }
            }

            return "";
        }

        private string ParseProcessorDefaultNumeric(string name, System.Collections.Specialized.StringCollection defaults)
        {
            Regex exp = new Regex(name + "\\s*=\\s*([\\dxXA-Fa-f]+)", RegexOptions.IgnoreCase);

            foreach (string s in defaults)
            {
                Match m = exp.Match(s);

                if (m.Success)
                {
                    return m.Groups[1].Value;
                }
            }

            return "";
        }

        private void cbProcessor_SelectedIndexChanged(object sender, EventArgs e)
        {
            Processor p = cbProcessor.SelectedItem as Processor;

            if (p != null)
            {
                System.Collections.Specialized.StringCollection props = null;

                try
                {
                    props = Properties.Settings.Default[p.Name.ToUpper() + "_Defaults"] as System.Collections.Specialized.StringCollection;
                }
                catch( System.Configuration.SettingsPropertyNotFoundException )
                {
                    //
                    // Try looking for processor defaults in the user settings of the application configuration file
                    //
                    Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                    UserSettingsGroup usg = cfg.SectionGroups["userSettings"] as UserSettingsGroup;

                    if (usg != null)
                    {
                        ClientSettingsSection css = usg.Sections[0] as ClientSettingsSection;

                        if (css != null)
                        {
                            SettingElement se = css.Settings.Get(p.Name.ToUpper() + "_Defaults");

                            if (se != null)
                            {
                                System.Xml.Serialization.XmlSerializer xSer = new System.Xml.Serialization.XmlSerializer(typeof(System.Collections.Specialized.StringCollection));

                                try
                                {
                                    StringReader sr = new StringReader( "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + se.Value.ValueXml.InnerXml);
                                    props = xSer.Deserialize(sr) as System.Collections.Specialized.StringCollection;
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                }

                if (props != null && props.Count > 0)
                {
                    string memCfg = ParseProcessorDefaultString("MemCfg", props).ToUpper();

                    if (cbRuntimeMemCfg.Items.Contains(memCfg))
                    {
                        cbRuntimeMemCfg.SelectedItem = memCfg;
                    }
                    else
                    {
                        cbRuntimeMemCfg.SelectedIndex = 2; // medium
                    }

                    string dbgPort = ParseProcessorDefaultString("DbgPort", props).ToUpper();

                    foreach(ComboBoxItem cbi in cbDbgPort.Items)
                    {
                        if (cbi.DisplayName == dbgPort)
                        {
                            cbDbgPort.SelectedItem = cbi;
                            break;
                        }
                    }

                    tbRamBase.Text = ParseProcessorDefaultNumeric("RamBase", props);
                    tbRamSize.Text = ParseProcessorDefaultNumeric("RamSize", props);
                    tbFlashBase.Text = ParseProcessorDefaultNumeric("FlashBase", props);
                    tbFlashSize.Text = ParseProcessorDefaultNumeric("FlashSize", props);
                    tbClockSpeed.Text = ParseProcessorDefaultNumeric("ClockSpeed", props);
                    tbSlowClock.Text = ParseProcessorDefaultNumeric("SlowClock", props);
                }
                else
                {
                    tbRamBase.Text = "";
                    tbRamSize.Text = "";
                    tbFlashBase.Text = "";
                    tbFlashSize.Text = "";
                    tbClockSpeed.Text = "";
                    tbSlowClock.Text = "";
                }

            }
        }

        private void tv_SelectedProjects_AfterCheck(object sender, TreeViewEventArgs e)
        {
            tv_SelectedProjects.SelectedNode = e.Node;

            foreach (TreeNode child in e.Node.Nodes)
            {
                child.Checked = e.Node.Checked;
            }
        }

        private void tv_SelectedProjects_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            //tv_SelectedProjects.SelectedNode = e.Node;
        }

        private void tv_FeatureView_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            //tv_FeatureView.SelectedNode = e.Node;
        }

        private void tv_LibraryView_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            //tv_LibraryView.SelectedNode = e.Node;
        }

        private void SolutionWizardForm_Load(object sender, EventArgs e)
        {

        }

        private void wpFinish_ShowFromNext(object sender, EventArgs e)
        {
            Wiz.NextEnabled = false;
            this.UseWaitCursor = true;

            rtb_Finish.Text = "";

            StringBuilder sb = new StringBuilder();

            // TODO: Have docs write this description:
            sb.Append(string.Format(Properties.Resources.FinalTitle + "\n\n", m_solution.Name));
            sb.Append("\t" + m_spoClientPath + "\\Solutions\\" + m_solution.Name + "\n\n");

            sb.Append(Properties.Resources.FinalDescription + "\n\n");

            AddPropLine(sb, Properties.Resources.Solution, m_solution.Name);
            AddPropLine(sb, Properties.Resources.Author, m_solution.Author);
            AddPropLine(sb, Properties.Resources.Description, m_solution.Description);
            AddPropLine(sb, Properties.Resources.Processor, m_solution.Processor.Name);
            AddPropLine(sb, Properties.Resources.DebuggerPort, m_solution.DebuggerPort);

            foreach (MFProject proj in m_solution.Projects)
            {
                if (proj.Guid == m_allProjectsGuid) continue;

                sb.Append("\n\n" + Properties.Resources.Project + " " + proj.Name + "\n");
                AddPropLine(sb, Properties.Resources.Description, proj.Description);
                AddPropLine(sb, Properties.Resources.Path, proj.Directory);

                if (proj.IsClrProject)
                {
                    sb.Append("\n" + Properties.Resources.Features + ":\n");
                    foreach (MFComponent feat in proj.Features)
                    {
                        Feature f = m_helper.FindFeature(feat.Guid);
                        AddPropLine(sb, feat.Name, f.Description);
                    }
                }
                sb.Append("\n" + Properties.Resources.Libraries + ":\n");
                foreach (MFComponent lib in proj.Libraries)
                {
                    Library l = m_helper.FindLibrary(lib);
                    AddPropLine(sb, lib.Name, l.Description);
                }
            }

            rtb_Finish.Text = sb.ToString();

            this.UseWaitCursor = false;
            Wiz.NextEnabled = true;
            // invalidate to make the cursor change from the wait cursor
            this.Invalidate();
        }

        private void wpFinish_CloseFromBack(object sender, Gui.Wizard.PageEventArgs e)
        {
            if (string.Compare(m_solution.Processor.Name, "windows", true) == 0)
            {
                e.Page = wpChooseFeatures;
            }
        }

        private void wpFinish_CloseFromNext(object sender, Gui.Wizard.PageEventArgs e)
        {
            Wiz.NextEnabled = false;
            GenerateSolution();
            m_Finished = true;
        }

        private void SolutionWizardForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!m_Finished && MessageBox.Show(this,
                Properties.Resources.AreYouSureYouWantToExit,
                Properties.Resources.WizardCancelled, MessageBoxButtons.YesNo
                ) == DialogResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}
