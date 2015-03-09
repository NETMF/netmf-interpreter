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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
    public partial class DeploymentStatusDialog : Form
    {
        private MFDevice         m_dev;
        private bool             m_fEraseCmd = false;
        private ReadOnlyCollection<string> m_files;
        private string []        m_sigFiles;
        AutoResetEvent           m_evtDevice = null;
        ManualResetEvent         m_evtShutdown = new ManualResetEvent(false);
        AutoResetEvent           m_evtDeviceFinished = new AutoResetEvent(false);
        Thread                   m_thread;
        EraseOptions[]           m_eraseBlocks;
        string                   m_pwd;

        private void OnStatus(long value, long total, string status)
        {
            if (m_evtShutdown.WaitOne(0, false)) return;

            textBox1.Invoke((MethodInvoker)delegate
            {
                textBox1.Text = status;
                textBox1.Invalidate();
                progressBar1.Maximum = (int)total;
                progressBar1.Value = (int)value;
                progressBar1.Invalidate();
                this.Update();
            });
        }

        public DeploymentStatusDialog(MFDevice dev, ReadOnlyCollection<string> files, string[] sig_files)
            : this(dev, files, sig_files, "")
        {
        }

        public DeploymentStatusDialog(MFDevice dev, ReadOnlyCollection<string> files, string[] sig_files, string pwd)
        {
            m_dev       = dev;
            m_files     = files;
            m_sigFiles  = sig_files;
            m_pwd       = pwd;

            InitializeComponent();
        }

        public DeploymentStatusDialog(MFDevice dev, params EraseOptions[] eraseBlocks)
            : this(dev, null, null)
        {
            m_fEraseCmd = true;
            m_eraseBlocks = eraseBlocks;
            this.Text = Properties.Resources.DeploymentStatusTitleErase;
        }

        private void ThreadProc()
        {
            bool fAbort = false;

            try
            {
                m_dev.OnProgress += new MFDevice.OnProgressHandler(OnStatus);

                if (m_evtShutdown.WaitOne(0, false)) return;

                m_evtDevice = m_dev.EventCancel;

                bool fMicroBooter = (m_dev.Ping() == PingConnectionType.MicroBooter);

                if (!fMicroBooter)
                {
                    m_dev.DbgEngine.PauseExecution();
                }

                if (m_fEraseCmd)
                {
                    if (!m_dev.Erase(m_eraseBlocks))
                    {
                        throw new MFDeployEraseFailureException();
                    }
                }
                else
                {
                    int cnt = 0;

                    List<uint> executionPoints = new List<uint>();

                    foreach (string file in m_files)
                    {
                        uint entry = 0;

                        if (Path.GetExtension(file).ToLower() == ".nmf")
                        {
                            if (!m_dev.DeployUpdate(file)) throw new MFDeployDeployFailureException();

                            return;
                        }
                        else if (!m_dev.Deploy(file, m_sigFiles[cnt++], ref entry))
                        {
                            throw new MFDeployDeployFailureException();
                        }

                        if (entry != 0)
                        {
                            executionPoints.Add(entry);
                        }
                    }

                    executionPoints.Add(0);

                    if (!fMicroBooter)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            if (m_dev.Connect(500, true)) break;
                        }
                    }
                    OnStatus(100, 100, "Executing Application");
                    foreach (uint addr in executionPoints)
                    {
                        if (m_dev.Execute(addr)) break;
                    }
                }

                //if (!m_dev.CheckForMicroBooter() && m_dev.DbgEngine != null && m_dev.DbgEngine.IsConnected &&
                //    m_dev.DbgEngine.ConnectionSource == SPOT.Debugger.ConnectionSource.TinyCLR)
                //{
                //    m_dev.DbgEngine.ResumeExecution();
                //}

            }
            catch (ThreadAbortException)
            {
                fAbort = true;
            }
            catch (MFUserExitException)
            {
                fAbort = true;
            }
            catch (Exception e)
            {
                if (!m_evtShutdown.WaitOne(0, false))
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        MessageBox.Show(this, Properties.Resources.ErrorPrefix + e.Message, Properties.Resources.ApplicationTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
                }
            }
            finally
            {
                m_evtDeviceFinished.Set();

                if (m_dev.DbgEngine != null)
                {
                    try
                    {
                        if (m_dev.IsConnected && m_dev.DbgEngine.ConnectionSource == SPOT.Debugger.ConnectionSource.TinyCLR)
                        {
                            m_dev.DbgEngine.ResumeExecution();
                        }
                    }
                    catch
                    {
                    }
                }
                
                m_dev.OnProgress -= new MFDevice.OnProgressHandler(OnStatus);

                if (!fAbort && !m_evtShutdown.WaitOne(100, false))
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        Close();
                    });
                }
            }
        }

        private void DeploymentStatusDialog_Load(System.Object sender, System.EventArgs e)
        {
            m_thread = new Thread(new ThreadStart(ThreadProc));
            m_thread.Start();
        }

        private void Shutdown()
        {
            if (m_evtDevice != null)
            {
                m_evtDevice.Set();
            }
            if (!m_evtDeviceFinished.WaitOne(5000, false))
            {
                if (m_thread != null && m_thread.IsAlive)
                {
                    m_thread.Abort();
                    m_thread = null;
                }
            }
        }

        private void button1_Click(System.Object sender, System.EventArgs e)
        {
            m_evtShutdown.Set();

            Shutdown();
        }

        private void DeploymentStatusDialog_FormClosing(System.Object sender, FormClosingEventArgs e)
        {
        }
    }
    [Serializable]
    public class MFDeployEraseFailureException : Exception
    {
        public override string Message { get { return Properties.Resources.ErrorErase; } }
    }

    [Serializable]
    public class MFDeployDeployFailureException : Exception
    {
        public override string Message { get { return Properties.Resources.ErrorDeployment; } }
    }
}
