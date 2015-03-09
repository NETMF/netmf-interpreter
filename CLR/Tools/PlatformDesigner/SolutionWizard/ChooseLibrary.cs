using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using ComponentObjectModel;
using XsdInventoryFormatObject;

namespace SolutionWizard
{
    public partial class SolutionWizardForm : Form
    {
        private string    m_allProjectsGuid = System.Guid.NewGuid().ToString("B");
        // we map all LibraryCategory "Generate Template" components to the same GUID
        private Dictionary<string, Library> m_lcGuidToGenGuid = new Dictionary<string, Library>();
        private Processor m_solutionProc = null;
        readonly string c_GenerateTemplateString = Properties.Resources.GenerateTemplate;
        private int m_prevLibraryProjectIndex = 0;
        private ProjectComboData m_pcdAll = null;
        private bool m_fResolving = false;

        /// <summary>
        /// structure for storing information about a template generation selection
        /// The LibraryCategory node holds the template source code and project definitions,
        /// but we also need the component which tells us project specific info
        /// </summary>
        internal class TemplateGenerationData
        {
            internal TemplateGenerationData(LibraryCategory lc, MFComponent cmp)
            {
                LibraryCat = lc;
                Comp       = cmp;
            }
            internal LibraryCategory LibraryCat;
            internal MFComponent Comp;
        }
        
        /// <summary>
        /// Identifies a components relation ship with a feature.  Used to determine where a library should
        /// be selected or its stub.
        /// </summary>
        internal enum FeatureAssociation
        {
            None,
            Selected,
            NotSelected
        }
        
        /// <summary>
        /// This is the list of "Generate Template" nodes the use chose.  We will need to handle these in a 
        /// special manner later (during Solution creation)
        /// </summary>
        private List<TemplateGenerationData> m_generateComponentList = new List<TemplateGenerationData>();

        /// <summary>
        /// Moving from Choose Libraries to Create Solution page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wpChooseLibraries_CloseFromNext(object sender, Gui.Wizard.PageEventArgs e)
        {
            m_prevLibraryProjectIndex = cbProjectSelect_Library.SelectedIndex;

            m_generateComponentList.Clear();

            // loop through each project
            foreach (ProjectComboData pcd in cbProjectSelect_Library.Items)
            {
                // skip the "all projects" combo box item
                if (pcd == m_pcdAll) continue;

                Dictionary<string, MFComponent> libCatLookup = new Dictionary<string, MFComponent>();

                foreach (MFComponent cmpLib in pcd.Proj.Libraries)
                {
                    Library lib = m_helper.FindLibrary(cmpLib);

                    // If the selected library has a library category associated with it, then
                    // we will need to note that we processed it for verification later
                    if (lib != null && lib.HasLibraryCategory)
                    {
                        libCatLookup[lib.LibraryCategory.Guid.ToLower()] = cmpLib;

                        // Rename the generate template component (to an appropriate name)
                        // and add it to the template generation list 
                        if (lib.Name == c_GenerateTemplateString)
                        {
                            cmpLib.Name = lib.LibraryCategory.Name + "_" + m_solution.Name;
                            LibraryCategory lc = m_helper.FindLibraryCategory(lib.LibraryCategory.Guid);

                            m_generateComponentList.Add(new TemplateGenerationData(lc, cmpLib));
                        }
                    }
                }

                ShowLibraryCategories(cbShowAllLibCatChoices.Checked);

                List<LibraryCategory> unresolvedItems = new List<LibraryCategory>();
                List<LibraryCategory> removedItems = new List<LibraryCategory>();

                UpdateProjectDependencies(pcd, unresolvedItems, removedItems);

                if (unresolvedItems.Count > 0)
                {
                    MessageBox.Show(Properties.Resources.NotAllLibrariesResolved, Properties.Resources.SolutionWizard);
                    if (cbShowAllLibCatChoices.Checked)
                    {
                        cbShowAllLibCatChoices.Checked = false;
                    }
                    if (!pcd.Proj.IsClrProject)
                    {
                        m_prevLibraryProjectIndex = cbProjectSelect_Library.SelectedIndex;
                        cbProjectSelect_Library.SelectedItem = pcd;
                    }
                    e.Page = wpChooseLibraries;
                    return;
                }
            }
        }

        /// <summary>
        /// Moving backwards to previous page.  This changes depending on the projects selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wpChooseLibraries_CloseFromBack(object sender, Gui.Wizard.PageEventArgs e)
        {
            m_prevLibraryProjectIndex = cbProjectSelect_Library.SelectedIndex;

            if (m_HasClrProj)
            {
                e.Page = wpChooseFeatures;
            }
            else
            {
                e.Page = wpChooseProjects;
            }
        }

        /// <summary>
        /// Moving to this page from the previous page.  The user can move back from here, so we
        /// may already be initialized.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wpChooseLibraries_ShowFromNext(object sender, EventArgs e)
        {
            if (!m_worker.IsBusy)
            {
                // If we have a new solution guid then we need to rebuild the list
                if (0 != string.Compare(m_prevSolutionGuid, m_solution.Guid))
                {
                    // we use the solution's processor to filter library nodes
                    m_solutionProc = m_helper.FindProcessor(m_solution.Processor.Guid);
                    m_prevSolutionGuid = m_solution.Guid;

                    if (tv_LibraryView.Nodes.Count != 0)
                    {
                        tv_LibraryView.Nodes.Clear();
                    }

                    // disable forward/back while we process the libraries
                    Wiz.NextEnabled = false;
                    Wiz.BackEnabled = false;
                    cbShowAllLibCatChoices.Enabled = false;
                    cbProjectSelect_Library.Enabled = false;

                    rtb_LibraryDescription.Text = Properties.Resources.LoadingLibraries;

                    this.UseWaitCursor = true;

                    // load all MF libraries
                    m_worker.RunWorkerAsync(BackgroundWorkerType.LoadLibraries);
                }
                else if (!m_fResolving)
                {
                    tv_LibraryView.Nodes.Clear();

                    LoadAllProjectLibraryData();

                    ShowLibraryCategories(cbShowAllLibCatChoices.Checked);

                    //LoadProjectLibraryData();
                    if (cbProjectSelect_Library.SelectedIndex == -1 && cbProjectSelect_Library.Items.Count > 0)
                    {
                        if (m_prevLibraryProjectIndex < cbProjectSelect_Library.Items.Count)
                        {
                            cbProjectSelect_Library.SelectedItem = cbProjectSelect_Library.Items[m_prevLibraryProjectIndex];
                        }
                        else
                        {
                            cbProjectSelect_Library.SelectedIndex = 0;
                        }
                    }
                }
                m_fResolving = false;
            }
        }

        /// <summary>
        /// Adds a "Generate Template" node to the library category node if supported
        /// </summary>
        /// <param name="pcd"></param>
        /// <param name="lc"></param>
        private Library AddGenerateTemplateNode(ProjectComboData pcd, LibraryCategory lc)
        {
            // There are no CLR or Support libraries that can be overriden
            if (lc.Level != LibraryLevel.CLR && (lc.Templates.Count > 0))
            {
                Library lib = null;

                string key = lc.Guid.ToLower();

                // Since each project may have its own set of libraries (and generate template nodes)
                // we need to synchronize the component guids for like categories
                if (m_lcGuidToGenGuid.ContainsKey(key))
                {
                    lib = m_lcGuidToGenGuid[key];
                }
                else
                {
                    lib = new Library();
                    lib.Name = c_GenerateTemplateString;
                    lib.Level = lc.Level;
                    lib.Description = "Generates template code in your solution's DeviceCode directory for the given Library Category." + 
                                      "The project will be generated in Solutions\\" + m_solution.Name + "\\DeviceCode\\" + lc.Name;
                    lib.LibraryCategory = new MFComponent(MFComponentType.LibraryCategory, lc.Name, lc.Guid, lc.ProjectPath);
                    m_lcGuidToGenGuid[key] = lib;
                }

                lib.ProjectPath = m_spoClientPath + "\\Solutions\\" + m_solution.Name + "\\DeviceCode\\" + lc.Name + "\\dotnetmf.proj";

                // add the library to the inventory
                if (null == m_helper.FindLibrary(lib.Guid))
                {
                    m_helper.AddLibraryToInventory(lib, false, m_inv);
                }

                return lib;
            }

            return null;
        }

        private void UpdateProjectDependencies(ProjectComboData pcd, List<LibraryCategory> unresolvedItems, List<LibraryCategory> removedItems)
        {
            bool fReanalyze = true;
            int retries = 20;

            List<TreeNode> nodeList = new List<TreeNode>();

            Dictionary<string, MFComponent> resolveMap = new Dictionary<string, MFComponent>();

            while (fReanalyze && retries-- > 0)
            {
                unresolvedItems.Clear();
                removedItems.Clear();

                //LoadProjectLibraryData(pcd, m_pcdAll, true);
                pcd.Proj.AnalyzeLibraries(m_helper, m_solution, unresolvedItems, removedItems);

                fReanalyze = (removedItems.Count > 0);

                foreach (LibraryCategory lc in unresolvedItems)
                {
                    if (!resolveMap.ContainsKey(lc.Guid))
                    {
                        AddGenerateTemplateNode(pcd, lc);

                        Library sel = AutoSelectLibrary(pcd, lc);

                        if (sel != null)
                        {
                            fReanalyze = true;

                            MFComponent cmpNew = new MFComponent(MFComponentType.Library, sel.Name, sel.Guid, sel.ProjectPath);

                            resolveMap[lc.Guid] = cmpNew;

                            ApplyToProject(pcd, cmpNew, true, unresolvedItems, removedItems);
                        }
                    }
                    else
                    {
                        ApplyToProject(pcd, resolveMap[lc.Guid], true, unresolvedItems, removedItems);
                    }
                }
            }
        }


        /// <summary>
        /// Only call this method from initialization, it will repopulate all project library nodes and 
        /// undo any user selections of libraries.
        /// </summary>
        private void LoadAllProjectLibraryData()
        {
            cbProjectSelect_Library.Items.Clear();
            cbProjectSelect_Library.ValueMember = "DisplayName";

            m_pcdAll = new ProjectComboData(m_solution.Projects[0].Name, m_solution.Projects[0]);

            m_pcdAll.Proj.Features.Clear();
            m_pcdAll.Proj.Libraries.Clear();
            m_pcdAll.Proj.LibraryCategories.Clear();
            m_pcdAll.Proj.IsClrProject = true;

            cbProjectSelect_Library.Items.Add(m_pcdAll);

            ProjectComboData pcdDef = null;

            foreach (MFProject proj in m_solution.Projects)
            {
                if (proj.Guid == m_allProjectsGuid) continue;

                ProjectComboData pcd = new ProjectComboData(proj.Name, proj);

                // always have the clr 2nd after "All Projects" 
                if (0 == string.Compare(proj.Name, "tinyclr", true))
                {
                    cbProjectSelect_Library.Items.Insert(1, pcd);

                    pcdDef = pcd;
                }
                else
                {
                    if (pcdDef == null) pcdDef = pcd;

                    cbProjectSelect_Library.Items.Add(pcd);
                }
            }

            if (pcdDef != null)
            {
                m_pcdAll.Proj.Features.AddRange(pcdDef.Proj.Features);
                m_pcdAll.Proj.Libraries.AddRange(pcdDef.Proj.Libraries);
                m_pcdAll.Proj.LibraryCategories.AddRange(pcdDef.Proj.LibraryCategories);
                m_pcdAll.Proj.m_cloneProj = pcdDef.Proj.m_cloneProj;
            }

            ProjectComboData pcdSelect = null;

            foreach (ProjectComboData pcd in cbProjectSelect_Library.Items)
            {
                List<LibraryCategory> unresolvedItems = new List<LibraryCategory>();
                List<LibraryCategory> removedItems = new List<LibraryCategory>();

                UpdateProjectDependencies(pcd, unresolvedItems, removedItems);

                if (unresolvedItems.Count > 0 && (pcdSelect == null || pcd == pcdDef))
                {
                    pcdSelect = pcd;
                }
            }

            if (pcdSelect != null && pcdSelect != pcdDef)
            {
                cbProjectSelect_Library.SelectedItem = pcdSelect;
            }
        }

        private void ApplyToProject(ProjectComboData pcd, MFComponent comp, bool fCheck, List<LibraryCategory> unresolvedItems, List<LibraryCategory>removedItems)
        {
            bool fActiveProj = (cbProjectSelect_Library.SelectedItem == pcd);

            bool fContainsLib = pcd.Proj.Libraries.Contains(comp);

            if (fCheck)
            {
                if (fContainsLib && pcd != m_pcdAll) return;
                pcd.Proj.Libraries.Add(comp);
            }
            else
            {
                if (!fContainsLib && pcd != m_pcdAll) return;
                pcd.Proj.RemoveLibrary(comp);
            }

            if (unresolvedItems == null || removedItems == null)
            {
                unresolvedItems = new List<LibraryCategory>();
                removedItems = new List<LibraryCategory>();

                pcd.Proj.AnalyzeLibraries(m_helper, m_solution, unresolvedItems, removedItems);
            }

            if (fActiveProj && tv_LibraryView.Nodes.Count > 0)
            {
                if (fCheck)
                {
                    foreach (LibraryCategory lc in unresolvedItems)
                    {
                        if (tv_LibraryView.Nodes.Find(lc.Guid.ToLower(), true).Length == 0)
                        {
                            AddGenerateTemplateNode(pcd, lc);

                            Library sel = AutoSelectLibrary(pcd, lc);

                            TreeNode lcNode = tv_LibraryView.Nodes[0].Nodes.Add(lc.Guid.ToLower(), lc.Name);
                            lcNode.Tag = new MFComponent(MFComponentType.LibraryCategory, lc.Name, lc.Guid, lc.ProjectPath);

                            foreach (Library lib in m_helper.GetLibrariesOfType(lc, pcd.Proj, m_solution))
                            {
                                if (pcd.Proj.ValidateLibrary(lib, m_solution, m_solutionProc, m_helper))
                                {
                                    TreeNode tnLib = lcNode.Nodes.Add(lib.Guid.ToLower(), lib.Name);
                                    tnLib.Tag = new MFComponent(MFComponentType.Library, lib.Name, lib.Guid, lib.ProjectPath);

                                    if (sel == lib)
                                    {
                                        tnLib.Checked = true;
                                    }
                                }
                            }

                            lcNode.Expand();
                        }
                    }
                }
                else
                {
                    foreach (LibraryCategory lc in removedItems)
                    {
                        TreeNode[] nds = tv_LibraryView.Nodes.Find(lc.Guid.ToLower(), true);
                        if (nds.Length > 0)
                        {
                            nds[0].Remove();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Apply user "check/uncheck" to all project nodes if "All Projects" is selected, otherwise
        /// just to the selected project.
        /// </summary>
        /// <param name="pcdAll"></param>
        /// <param name="compGuid"></param>
        /// <param name="fAdd"></param>
        private void ApplyToAllProjects(ProjectComboData pcd, MFComponent comp, bool fCheck)
        {
            ApplyToProject(pcd, comp, fCheck, null, null);

            if (pcd == m_pcdAll)
            {
                foreach (ProjectComboData pcd2 in cbProjectSelect_Library.Items)
                {
                    if (pcd2 != pcd)
                    {
                        // change the component to a bootloader component if it exists
                        if (pcd2.Proj.IsBootloaderProject())
                        {
                            Library lib = m_helper.FindLibrary(comp);

                            if (lib != null && lib.HasLibraryCategory)
                            {
                                LibraryCategory lc = m_helper.FindLibraryCategory(lib.LibraryCategory.Guid);

                                if (lc != null)
                                {
                                    foreach (Library li in m_helper.GetLibrariesOfType(lc))
                                    {
                                        if (li.IsBootloaderLibrary())
                                        {
                                            comp = new MFComponent(MFComponentType.Library, li.Name, li.Guid, li.ProjectPath, comp.Conditional);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        ApplyToProject(pcd2, comp, fCheck, null, null);
                    }
                }
            }
            else if(pcd.Name.ToLower() == "tinyclr")
            {
                ApplyToProject(m_pcdAll, comp, fCheck, null, null);
            }
        }

        /// <summary>
        /// Find a feature node in the feature view tree from the given root node (recursive)
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        TreeNode FindFeatureNode(MFComponent feature, TreeNode root)
        {
            ComponentTreeNode ctn = root.Tag as ComponentTreeNode;

            if(ctn != null && (string.Compare(ctn.Comp.Guid, feature.Guid, true) == 0))
            {
                return root;
            }

            foreach (TreeNode tn in root.Nodes)
            {
                TreeNode found = FindFeatureNode(feature, tn);

                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the feature association for a given library category.  A feature association determines
        /// weather a category is associated with a feature and if the feature is selected by the user.
        /// </summary>
        /// <param name="lc"></param>
        /// <returns></returns>
        private FeatureAssociation GetFeatureAssociation(LibraryCategory lc)
        {
            // default to true if there are no assocations for this library category (so the user must choose the library)
            FeatureAssociation assoc = FeatureAssociation.None;

            foreach (MFComponent comp in lc.FeatureAssociations)
            {
                TreeNode []nodes = tv_FeatureView.Nodes.Find(comp.Guid.ToLower(), true);

                if (nodes.Length != 0)
                {
                    if (nodes[0].Checked)
                    {
                        assoc = FeatureAssociation.Selected;
                        break;
                    }
                    else
                    {
                        assoc = FeatureAssociation.NotSelected;
                    }
                }
                else
                {
                    Feature feat = m_helper.FindFeature(comp.Guid);

                    if (feat != null)
                    {
                        if (m_pcdAll.Proj.ContainsFeature(comp))
                        {
                            assoc = FeatureAssociation.Selected;
                        }
                        else
                        {
                            assoc = feat.Required ? FeatureAssociation.Selected : FeatureAssociation.NotSelected;
                        }
                    }
                    else
                    {
                        assoc = FeatureAssociation.NotSelected;
                    }
                }
            }

            return assoc;
        }


        /// <summary>
        /// Populates the tree view.  Based on the Show All checkbox, this method will display all library
        /// categories or just the unresolved ones.
        /// </summary>
        /// <param name="showAll"></param>
        private void ShowLibraryCategories(bool showAll)
        {
            ShowLibraryCategories(showAll, false);
        }

        private Library AutoSelectLibrary(ProjectComboData pcd, LibraryCategory lc)
        {
            Library defaultLib = null;
            Library stubLib = null;
            Library bootLoaderLib = null;
            Library generateLib = null;
            Library clrLib = null;
            bool fTooManyLibs = false;
            MFProject proj = pcd.Proj;

            
            FeatureAssociation fa = GetFeatureAssociation(lc);

            foreach (Library lib in m_helper.GetLibrariesOfType(lc))
            {
                if (proj.ValidateLibrary(lib, m_solution, m_solutionProc, m_helper))
                {
                    if (m_pcdAll.Proj.Libraries.Contains(new MFComponent(MFComponentType.Library, lib.Name, lib.Guid))) clrLib = lib;

                    if (lib.IsStub) stubLib = lib;
                    else if (lib.IsBootloaderLibrary()) bootLoaderLib = lib;
                    else if (lib.Name == c_GenerateTemplateString) generateLib = lib;
                    else if (defaultLib == null)
                    {
                        defaultLib = lib;
                    }
                    else if (fa != FeatureAssociation.NotSelected)
                    {
                        // there are now too many libraries for us to choose from
                        // so we can not auto-select unless this is a cloned solution
                        // in which case we will look for common libraries.
                        fTooManyLibs = true;
                    }
                }
            }

            // if the solution is cloned then lets try to choose common libraries where possible
            if (m_solution.m_cloneSolution != null)
            {
                MFProject cloneProj = null;

                // for "all projects" choose the TinyCLR project
                bool fAllProj = (0 == string.Compare(proj.Name, Properties.Resources.AllProjects, true));

                // match libraries from the given project
                foreach(MFProject prj in m_solution.m_cloneSolution.Projects)
                {
                    if (fAllProj)
                    {
                        if (prj.IsClrProject)
                        {
                            cloneProj = prj;
                            break;
                        }
                    }
                    else if(0 == string.Compare(prj.Name, proj.Name , true))
                    {
                        cloneProj = prj;
                        break;
                    }
                }

                // Only match libraries from common projects
                if (cloneProj != null)
                {
                    // search the cloned solution for the common library category
                    foreach (MFComponent cloneLib in cloneProj.Libraries)
                    {
                        Library cl = m_helper.FindLibrary(cloneLib);

                        if (cl == null && m_solution.m_clonedLibraryMap.ContainsKey(cloneLib.Guid.ToUpper()))
                        {
                            cl = m_solution.m_clonedLibraryMap[cloneLib.Guid.ToUpper()];
                        }

                        if (cl != null)
                        {
                            if (cl.HasLibraryCategory && (0 == string.Compare(lc.Guid, cl.LibraryCategory.Guid, true)))
                            {
                                // if the library is a solution dependent library, then choose to generate it (which will 
                                // at a later stage copy the code from the clone solution).
                                if (cl.ProjectPath.ToLower().Contains("\\solutions\\" + m_solution.m_cloneSolution.Name.ToLower() + "\\"))
                                {
                                    if (generateLib == null)
                                    {
                                        // it is possible that we haven't created the "Generate Template" library yet.
                                        generateLib = AddGenerateTemplateNode(pcd, lc);
                                    }

                                    if (fa == FeatureAssociation.Selected || fa == FeatureAssociation.None)
                                    {
                                        return generateLib;
                                    }
                                    else
                                    {
                                        defaultLib = generateLib;
                                    }
                                }
                                ///
                                /// if the associated feature is not selected than use the cloned projects selection
                                /// as the default 
                                /// 
                                else if (fa != FeatureAssociation.Selected)
                                {
                                    defaultLib = cl;
                                }
                                ///
                                /// If the feature is selected use the cloned projects selection if it is not a stub
                                /// 
                                else if(!cl.IsStub)
                                {
                                    return cl;
                                }
                                break;
                            }
                        }
                    }
                }
            }

            switch (fa)
            {
                case FeatureAssociation.Selected:
                    if (proj.IsBootloaderProject())
                    {
                        if (bootLoaderLib != null)
                        {
                            return bootLoaderLib;
                        }
                        else if (lc.IsTransport && m_solution.m_transportType.Equals(lc))
                        {
                            if (!fTooManyLibs)
                            {
                                return defaultLib;
                            }
                        }
                        else if (stubLib != null)
                        {
                            return stubLib;
                        }
                        else if (!fTooManyLibs && defaultLib != null && (generateLib == null || !string.IsNullOrEmpty(defaultLib.CustomFilter) || !string.IsNullOrEmpty(defaultLib.ProcessorSpecific.Guid)))
                        {
                            return defaultLib;
                        }
                    }
                    else
                    {
                        if (!fTooManyLibs && defaultLib != null)
                        {
                            return defaultLib;
                        }
                        else if (clrLib != null)
                        {
                            return clrLib;
                        }
                    }
                    break;
                case FeatureAssociation.NotSelected:
                    if (!lc.IsTransport || !m_solution.m_transportType.Equals(lc))
                    {
                        return stubLib;
                    }
                    break;
                case FeatureAssociation.None:
                    // if we don't have a stub library then we should be able to select the appropriate library 
                    // automatically 
                    if (proj.IsBootloaderProject())
                    {
                        if (bootLoaderLib != null)
                        {
                            return bootLoaderLib;
                        }
                        else if (stubLib != null)
                        {
                            return stubLib;
                        }
                        else if (!fTooManyLibs)
                        {
                            return defaultLib;
                        }
                    }
                    else if (!fTooManyLibs && defaultLib != null && ((stubLib == null && generateLib == null) || !string.IsNullOrEmpty(defaultLib.CustomFilter) || !string.IsNullOrEmpty(defaultLib.ProcessorSpecific.Guid)))
                    {
                        return defaultLib;
                    }
                    else if (clrLib != null)
                    {
                        return clrLib;
                    }
                    break;
            }

            return null;
        }

        private void ShowLibraryCategories(bool showAll, bool noFilter)
        {
            ProjectComboData pcd = null;
            
            if(cbProjectSelect_Library.SelectedItem == null)
            {
                if (cbProjectSelect_Library.Items.Count > 0)
                {
                    pcd = cbProjectSelect_Library.Items[0] as ProjectComboData;
                }
            }
            else
            {
                pcd = cbProjectSelect_Library.SelectedItem as ProjectComboData;
            }

            if (pcd == null || pcd.Proj == null)
            {
                return;
            }

            tv_LibraryView.Nodes.Clear();

            TreeNode root = new TreeNode(Properties.Resources.LibraryCategories);

            List<LibraryCategory> unresolvedItems = new List<LibraryCategory>();
            List<LibraryCategory> removedItems = new List<LibraryCategory>();

            UpdateProjectDependencies(pcd, unresolvedItems, removedItems);

            if (showAll)
            {
                foreach (MFComponent lcCmp in pcd.Proj.LibraryCategories)
                {
                    LibraryCategory lc = m_helper.FindLibraryCategory(lcCmp.Guid);
                    if (!unresolvedItems.Contains(lc))
                    {
                        unresolvedItems.Add(lc);
                    }
                }
            }

            ///
            /// Show all generated code
            ///
            foreach (MFComponent lCmp in pcd.Proj.Libraries)
            {
                if (lCmp.Name == c_GenerateTemplateString)
                {
                    Library lib = m_helper.FindLibrary(lCmp);

                    if (lib != null && lib.HasLibraryCategory)
                    {
                        unresolvedItems.Add(m_helper.FindLibraryCategory(lib.LibraryCategory.Guid));
                    }
                }
            }

            Dictionary<string, TreeNode> libLookup = new Dictionary<string, TreeNode>();

            foreach (LibraryCategory lc in removedItems)
            {
                TreeNode []nds = root.Nodes.Find(lc.Guid.ToLower(), false);
                if (nds.Length > 0)
                {
                    nds[0].Remove();
                }
            }

            foreach (LibraryCategory lc in unresolvedItems)
            {
                TreeNode lcNode = new TreeNode(lc.Name);
                lcNode.Name = lc.Guid.ToLower();
                lcNode.Tag = new MFComponent(MFComponentType.LibraryCategory, lc.Name, lc.Guid, lc.ProjectPath);

                root.Nodes.Add(lcNode);

                AddGenerateTemplateNode(pcd, lc);

                foreach (Library lib in m_helper.GetLibrariesOfType(lc, pcd.Proj, m_solution))
                {
                    if (pcd.Proj.ValidateLibrary(lib, m_solution, m_solutionProc, m_helper))
                    {
                        TreeNode tnLib = lcNode.Nodes.Add(lib.Guid.ToLower(), lib.Name);
                        tnLib.Tag = new MFComponent(MFComponentType.Library, lib.Name, lib.Guid, lib.ProjectPath);

                        libLookup[lib.Guid.ToLower()] = tnLib;
                    }
                }
            }

            foreach(MFComponent cmp in pcd.Proj.Libraries)
            {
                if (libLookup.ContainsKey(cmp.Guid.ToLower()))
                {
                    libLookup[cmp.Guid.ToLower()].Checked = true;
                    (libLookup[cmp.Guid.ToLower()].Tag as MFComponent).Conditional = cmp.Conditional;
                }
            }

            if (root.Nodes.Count == 0)
            {
                root.Nodes.Add(Properties.Resources.NoUnresolvedLibCats);
            }

            tv_LibraryView.Nodes.Add(root);
            tv_LibraryView.Sort();
            root.ExpandAll();
            tv_LibraryView.SelectedNode = root;
        }

        private void tv_LibraryView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            MFComponent ctn = e.Node.Tag as MFComponent;
            ProjectComboData pcd = cbProjectSelect_Library.SelectedItem as ProjectComboData;

            if (ctn != null && ctn.ComponentType == MFComponentType.Library)
            {
                if (e.Node.Parent != null && e.Node.Checked)
                {
                    foreach (TreeNode sib in e.Node.Parent.Nodes)
                    {
                        if (sib != e.Node)
                        {
                            MFComponent ctnSib = sib.Tag as MFComponent;

                            if (sib.Checked && string.IsNullOrEmpty(ctnSib.Conditional))
                            {
                                sib.Checked = false;
                            }
                        }
                    }
                }

                ApplyToAllProjects(pcd, ctn, e.Node.Checked);

                if (e.Node.Checked && e.Node.Parent != null) e.Node.Parent.Checked = true;
            }

            //
            // Checking the root node will cause all "Generate Template" nodes to be checked.
            //
            if (e.Node.Parent == null)
            {
                foreach (TreeNode child in e.Node.Nodes)
                {
                    foreach (TreeNode sib in child.Nodes)
                    {
                        MFComponent ctnSib = sib.Tag as MFComponent;

                        if (ctnSib.Name == c_GenerateTemplateString)
                        {
                            if (sib.Checked != e.Node.Checked)
                            {
                                sib.Checked = e.Node.Checked;

                                ctn = sib.Tag as MFComponent;

                                ApplyToAllProjects(pcd, ctn, e.Node.Checked);
                            }
                        }
                    }
                }
            }
        }
        private void tv_LibraryView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            MFComponent ctn = e.Node.Tag as MFComponent;

            if (ctn != null)
            {
                DisplayDescription(ctn, rtb_LibraryDescription);

                cbLibraryCond.SelectedIndex = (int)ConvertConditionStringToIndex(ctn.Conditional);

                if (cbLibraryCond.SelectedIndex == 0)
                {
                    if (!string.IsNullOrEmpty(ctn.Conditional))
                    {
                        cbLibraryCond.Text = ctn.Conditional;
                    }
                    else
                    {
                        cbLibraryCond.Text = "";
                    }
                }
            }
            else
            {
                DisplayDescription(null, rtb_LibraryDescription);
            }
        }
        private void cbProjectSelect_Library_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowLibraryCategories(cbShowAllLibCatChoices.Checked);
        }
        private void cbShowAllLibCatChoices_CheckedChanged(object sender, EventArgs e)
        {
            ShowLibraryCategories(cbShowAllLibCatChoices.Checked);
        }
        private void cbLibraryCond_Leave(object sender, EventArgs e)
        {
            cbLibraryCond_SelectedIndexCommitted(sender, e);
        }
        private void cbLibraryCond_SelectedIndexCommitted(object sender, EventArgs e)
        {
            if (tv_LibraryView.SelectedNode == null) return;

            string cond = cbLibraryCond.Text;

            if (cbLibraryCond.SelectedIndex != -1)
            {
                cond = ConvertConditionIndexToString((ConditionIndex)cbLibraryCond.SelectedIndex);
            }

            MFComponent tnd = tv_LibraryView.SelectedNode.Tag as MFComponent;

            if (tnd != null)
            {
                tnd.Conditional = cond;

                if (cbProjectSelect_Library.SelectedItem == m_pcdAll)
                {
                    foreach (ProjectComboData pcd in cbProjectSelect_Library.Items)
                    {
                        if (pcd == m_pcdAll) continue;

                        int idx = pcd.Proj.Libraries.IndexOf(tnd);

                        if (idx >= 0)
                        {
                            pcd.Proj.Libraries[idx].Conditional = cond;
                        }
                    }
                }
                else
                {
                    ProjectComboData pcd = cbProjectSelect_Library.SelectedItem as ProjectComboData;

                    if (pcd != null)
                    {
                        int idx = pcd.Proj.Libraries.IndexOf(tnd);

                        if (idx >= 0)
                        {
                            pcd.Proj.Libraries[idx].Conditional = cond;
                        }
                    }
                }
            }
        }
    }
}