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
        private void tv_PlatformList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //GetPlatformDetails(e.Node.Name, Inventory);

            MFComponentDescriptor sol = e.Node.Tag as MFComponentDescriptor;

            if (sol != null)
            {
                DisplayDescription(sol, rtb_PlatformFeatures);

                Wiz.NextEnabled = true;
            }
            else
            {
                DisplayDescription(null, rtb_PlatformFeatures);

                Wiz.NextEnabled = false;
            }
        }
        private void b_Browse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlgOpen = new OpenFileDialog();

            // set properties for the dialog
            dlgOpen.Title = Properties.Resources.SelectExistingSolution;
            dlgOpen.ShowReadOnly = true;
            dlgOpen.Filter = Properties.Resources.SolutionFiles + " (*.settings)|*.settings";
            dlgOpen.InitialDirectory = Application.StartupPath;


            if (dlgOpen.ShowDialog() == DialogResult.OK)
            {
                //TODO: ADD SOLUTION

                b_Browse.Enabled = false;
                rtb_PlatformFeatures.Text = Properties.Resources.LoadingSolution;

                m_solutionOpened = m_bw.LoadSolutionProj(dlgOpen.FileName, "");

                tb_SelectedPlatform.Text = m_solutionOpened.Name;

                DisplayDescription(m_solutionOpened, rtb_PlatformFeatures);

                Wiz.NextEnabled = true;

                b_Browse.Enabled = true;
            }
        }

        private void wpSelectExisting_ShowFromNext(object sender, EventArgs e)
        {
            //The wpSelectExisting Paqe displays a list of existing platforms 
            //in the Inventory.xml or lets the user choose a Platform_selector file
            //to edit

            if (tv_PlatformList.Nodes.Count == 0)
            {
                Wiz.NextEnabled = false;
                Wiz.BackEnabled = false;
                rb_SelectFromList.Checked = true;
                tb_SelectedPlatform.Enabled = false;
                b_Browse.Enabled = false;

                if (tv_PlatformList.Nodes.Count == 0 && !m_worker.IsBusy)
                {
                    //tv_PlatformList.CheckBoxes = true;

                    rtb_PlatformFeatures.Text = Properties.Resources.LoadingSolutions;

                    m_worker.RunWorkerAsync(BackgroundWorkerType.LoadSolutions);
                }
            }
        }

        private void wpSelectExisting_CloseFromNext(object sender, Gui.Wizard.PageEventArgs e)
        {
            if (rb_SelectFromList.Checked)
            {
                m_selectedSolution = tv_PlatformList.SelectedNode.Tag as MFComponentDescriptor;

                // if the user came back to this page and changed the solution, then we will have to 
                // re-load the solution
                if (m_solution != null && 0 != string.Compare(m_solution.Guid, m_selectedSolution.Component.Guid, true))
                {
                    cbProcessor.Items.Clear();
                    tv_SelectedProjects.Nodes.Clear();
                    m_solution = null;
                }

                if (rbEditSolution.Checked)
                {
                    m_HasClrProj = true;

                    if (m_selectedSolution.SolutionProcessor != null && string.Compare(m_selectedSolution.SolutionProcessor.Name, "windows", true) == 0)
                    {
                        e.Page = wpChooseFeatures;
                    }
                    else
                    {
                        e.Page = wpSolutionCfg;
                    }
                }
                else
                {
                    e.Page = wpCreatePlatform;
                }
            }
            else
            {
                m_solution = m_solutionOpened;
                e.Page = wpCreatePlatform;
            }

            if (m_solution != null)
            {
                m_solution.Description = tb_PlatformDescription.Text;
                m_solution.Author = tb_SolutionAuthor.Text;
            }

            m_IsSolutionCloned = rbCloneSolution.Checked;
        }

        private void rb_BrowsePlatformFile_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_BrowsePlatformFile.Checked)
            {
                b_Browse.Enabled = true;
                tb_SelectedPlatform.Enabled = true;

                DisplayDescription(m_solutionOpened, rtb_PlatformFeatures);

                Wiz.NextEnabled = m_solutionOpened != null;
            }
            else
            {
                b_Browse.Enabled = false;
                tb_SelectedPlatform.Enabled = false;
            }
        }



        private void wpSelectExisting_CloseFromBack(object sender, Gui.Wizard.PageEventArgs e)
        {
            e.Page = wpChooseTask;
        }
    }
}