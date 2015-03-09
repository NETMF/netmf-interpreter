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

namespace SolutionWizard
{
    public partial class SolutionWizardForm : Form
    {
        private void wpChooseFeatures_CloseFromBack(object sender, Gui.Wizard.PageEventArgs e)
        {
            if (string.Compare(m_solution.Processor.Name, "windows", true) == 0)
            {
                e.Page = wpCreatePlatform;
            }
        }
        private void wpChooseFeatures_ShowFromNext(object sender, EventArgs e)
        {
            //Populate feature selection menu with features from the Inventory.xml
            if (!m_worker.IsBusy)
            {
                if (tv_FeatureView.Nodes.Count == 0)
                {
                    Wiz.NextEnabled = false;
                    Wiz.BackEnabled = false;

                    cbProjectSelect_Feature.Enabled = false;

                    this.UseWaitCursor = true;

                    rtb_FeatureDescription.Text = Properties.Resources.LoadingFeatures;
                    m_worker.RunWorkerAsync(BackgroundWorkerType.LoadFeatures);
                }
                else
                {
                    cbProjectSelect_Feature.Items.Clear();

                    foreach (MFProject proj in m_solution.Projects)
                    {
                        if (proj.IsClrProject && proj.Guid != m_allProjectsGuid)
                        {
                            ProjectComboData pcd = new ProjectComboData(proj.Name, proj);
                            cbProjectSelect_Feature.Items.Add(pcd);
                        }
                    }

                    cbProjectSelect_Feature.SelectedItem = cbProjectSelect_Feature.Items[0];
                }
            }
        }
        private void cbProjectSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            ProjectComboData pcd = cbProjectSelect_Feature.SelectedItem as ProjectComboData;

            if (pcd == null) return;

            TreeNode root = tv_FeatureView.Nodes[0];

            pcd.Proj.AnalyzeFeatures(m_helper);

            UpdateFeatureSelections(pcd);

            root.Expand(); //expand tree to display all nodes
            tv_FeatureView.Sort();
            tv_FeatureView.SelectedNode = root;
        }

        private void LoadProjectFeatureData()
        {
            cbProjectSelect_Feature.Items.Clear();
            cbProjectSelect_Feature.ValueMember = "DisplayName";

            foreach (MFProject proj in m_solution.Projects)
            {
                if (proj.IsClrProject && proj.Guid != m_allProjectsGuid)
                {
                    ProjectComboData pcd = new ProjectComboData(proj.Name, proj);

                    cbProjectSelect_Feature.Items.Add(pcd);
                }
            }
            cbProjectSelect_Feature.Invalidate();
        }

        private void wpChooseFeatures_CloseFromNext(object sender, Gui.Wizard.PageEventArgs e)
        {
            if (string.Compare(m_solution.Processor.Name, "windows", true) == 0)
            {
                foreach (ProjectComboData pcd in cbProjectSelect_Feature.Items)
                {
                    List<LibraryCategory> unresolved = new List<LibraryCategory>();
                    List<LibraryCategory> removed = new List<LibraryCategory>();

                    pcd.Proj.AnalyzeLibraries(m_helper, m_solution, unresolved, removed);

                    foreach (LibraryCategory lc in unresolved)
                    {
                        if (lc.Level == LibraryLevel.CLR)
                        {
                            foreach (Library l in m_helper.GetLibrariesOfType(lc))
                            {
                                if (!l.IsStub)
                                {
                                    pcd.Proj.Libraries.Add(new MFComponent(MFComponentType.Library, l.Name, l.Guid, l.ProjectPath));
                                    break;
                                }
                            }
                        }
                    }
                }

                e.Page = wpFinish;
            }
        }

        private void UpdateFeatureSelections(ProjectComboData pcd)
        {
            TreeNode root = tv_FeatureView.Nodes[0];

            Dictionary<string, MFComponent> featList = new Dictionary<string, MFComponent>();

            foreach (MFComponent cmp in pcd.Proj.Features)
            {
                featList[cmp.Guid.ToLower()] = cmp;
            }

            Dictionary<TreeNode,bool> nodesToUpdate = new Dictionary<TreeNode, bool>();

            foreach (TreeNode tn in root.Nodes)
            {
                if (tn != null)
                {
                    MFComponent c = tn.Tag as MFComponent;

                    if (featList.ContainsKey(c.Guid.ToLower()))
                    {
                        if (!tn.Checked)
                        {
                            nodesToUpdate[tn] = true;
                        }
                    }
                    else
                    {
                        if (tn.Checked)
                        {
                            nodesToUpdate[tn] = false;
                        }
                    }
                }
            }

            System.Threading.Interlocked.Increment(ref m_CheckRefCount);
            foreach (TreeNode tn in nodesToUpdate.Keys)
            {
                tn.Checked = nodesToUpdate[tn];
            }
            System.Threading.Interlocked.Decrement(ref m_CheckRefCount);
        }

        private int m_CheckRefCount = 0;

        private void tv_FeatureView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            ProjectComboData pcd = cbProjectSelect_Feature.SelectedItem as ProjectComboData;
            MFComponent feat = e.Node.Tag as MFComponent;
            TreeNode root = tv_FeatureView.Nodes[0];

            if (feat != null)
            {
                List<TreeNode> updateNodes = new List<TreeNode>();

                if (e.Node == root)
                {
                    foreach(TreeNode tn in root.Nodes)
                    {
                        updateNodes.Add(tn);
                    }
                }
                else
                {
                    updateNodes.Add(e.Node);
                }

                foreach (TreeNode tn in updateNodes)
                {
                    if (tn.Checked != e.Node.Checked || tn == e.Node)
                    {
                        if (tn.Checked != e.Node.Checked)
                        {
                            tn.Checked = e.Node.Checked;
                        }

                        if (pcd != null)
                        {
                            if (e.Node.Checked)
                            {
                                pcd.Proj.Features.Add(new MFComponent(MFComponentType.Feature, feat.Name, feat.Guid, feat.ProjectPath));
                            }
                            else
                            {
                                Feature f = m_helper.FindFeature(feat.Guid);

                                Dictionary<string, MFComponent> dependants = f.GetDependants(m_helper);

                                dependants[feat.Guid.ToLower()] = feat;

                                List<MFComponent> remFeatures = new List<MFComponent>();
                                foreach (MFComponent fCmp in pcd.Proj.Features)
                                {
                                    if(dependants.ContainsKey(fCmp.Guid.ToLower()))
                                    {
                                        remFeatures.Add(fCmp);
                                    }
                                }

                                foreach(MFComponent cmpFeat in remFeatures)
                                {
                                    pcd.Proj.Features.Remove(cmpFeat);
                                }
                            }
                        }
                    }
                }

                if (m_CheckRefCount == 0)
                {
                    pcd.Proj.AnalyzeFeatures(m_helper);

                    System.Threading.Interlocked.Increment(ref m_CheckRefCount);

                    UpdateFeatureSelections(pcd);

                    System.Threading.Interlocked.Decrement(ref m_CheckRefCount);
                }

                return;
            }
        }
        private void tv_FeatureView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //When the user clicks the name of a particular feature
            //we derive all its relevant information from the Inventory
            //and display in the RichTextBox 'rtb_FeatureDescription'

            DisplayDescription(e.Node.Tag, rtb_FeatureDescription);

        }
    }
}