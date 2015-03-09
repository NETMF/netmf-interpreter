using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections;
using System.Threading;
using System.Reflection;
using System.Globalization;
using System.Diagnostics;
using Microsoft.SPOT.Tools;
using System.IO.Ports;

namespace Microsoft.NetMicroFramework.Tools.NativeProfilerViewer
{
    public partial class NativeProfilerViewer : Form
    {
        Stream m_tOutput;
        bool receivingData = false;
        NativeProfiler profiler;
        Hashtable treeNodes;
        TreeNode rootNode;
        TreeNode lastSelectedNode;
        bool checkBlocksBeginEnd = true;
        bool displayData = true;
        Hashtable addedTwoLevels;
        private SerialPort port = null;
        DateTime timeStart, timeEnd, totalTime;
        String tempFileName = null;
        MemoryStream localStream;

        void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] buf = null;
            try
            {
                if (port.BytesToRead != 0)
                {
                    buf = new byte[port.BytesToRead];
                    port.Read(buf, 0, buf.Length);
                    m_tOutput.Write(buf, 0, buf.Length);
                    // Record last time we read something from the port
                    timeStart = DateTime.Now;
                }
            }
            catch
            {
                
            }
        }
        public NativeProfilerViewer()
        {
            addedTwoLevels = new Hashtable();
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Check if fileName exists;
            try
            {
                FileStream testOpen = new FileStream(this.textBox1.Text, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                testOpen.Close();              
                if (receivingData == false)
                {
                    try
                    {
                        Stop_Connection();
                        if (m_tOutput != null) m_tOutput.Close();
                        tempFileName = @"temp.raw";
                        m_tOutput = new FileStream(tempFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                        port = new SerialPort(textBox3.Text, 115200, Parity.None, 8, StopBits.One);
                        if (port.IsOpen == false)
                        {
                            for(uint index = 0; index < 2; index++)
                            {
                                try
                                {
                                    Stop_Connection();
                                    port.Open();
                                    port.DiscardInBuffer();
                                    port.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(port_DataReceived);
                                    timeStart = DateTime.Now;

                                    receivingData = true;
                                    this.button1.Text = "Disconnect";
                                    break;
                                }
                                catch(Exception exception1)
                                {
                                    if (index == 1)
                                    {
                                        MessageBox.Show("Cannot connect to device: " + this.textBox3.Text + " " + exception1.ToString());
                                    }
                                }
                            }
                        }
                    }
                    catch(Exception exception2)
                    {
                        MessageBox.Show("Cannot connect to device: " + this.textBox3.Text + " " + exception2.ToString());
                    }
                }
                else
                {
                    Stop_Connection();
                    this.button1.Text = "Connect to Device";
                    try
                    {
                        localStream = new MemoryStream((int)(m_tOutput.Length));
                        NativeProfiler.ReadWriteStream(m_tOutput, localStream);
                        if (m_tOutput != null) m_tOutput.Close();
                        m_tOutput = localStream;
                    }
                    catch (OutOfMemoryException)
                    {

                    }
                   
                    LoadRootTree();
                }
            }
            catch
            {
                MessageBox.Show("Cannot not open function names map file " + this.textBox1.Text);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = this.openFileDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    try
                    {
                        FileStream testOpen = new FileStream(this.openFileDialog1.FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                        testOpen.Close();
                        this.textBox1.Text = this.openFileDialog1.FileName;
                        if (receivingData == false && m_tOutput != null)
                        {
                            LoadRootTree();
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Cannot not open function names map file " + this.textBox1.Text);
                    }
                }
            }
            catch
            {             
            }
        }

        private void treeView1_AfterSelect_1(object sender, TreeViewEventArgs e)
        {
            if (displayData == false) return;
            progressBarControl1.progressBar1.Value = 0;
            progressBarControl1.Show();
            progressBarControl1.Refresh();

            TreeNode node = treeView1.SelectedNode;

            if (treeView1.SelectedNode != rootNode)
            {
                NativeProfilerUnit parentUnit = (NativeProfilerUnit)treeNodes[node];
                uint parentLevel = parentUnit.NestingLevel;
                uint parentIndex = parentUnit.NItem;
                uint startTime = parentUnit.EntryTime;
                uint endTime = parentUnit.ReturnTime;
                uint level = parentLevel;
                uint index = parentUnit.NItem;
                uint parentInclTime = profiler.GetUnitInclTime(index);
                if (parentUnit != null && lastSelectedNode != treeView1.SelectedNode) // If the node has child nodes it was already processed, and we should not add there the same nodes.
                {
                    dataGridView1.DataSource = null;
                    dataGridView1.Rows.Clear();
                    dataGridView1.Refresh();
                    bool needUpdate = false;
                    uint nodesCount = 0;
                    while (profiler.GetUnit(index + nodesCount) != null && profiler.GetUnit(index + nodesCount).EntryTime <= endTime)
                    {
                        nodesCount++;
                    }
                    while (profiler.GetUnit(index) != null && profiler.GetUnit(index).EntryTime <= endTime)
                    {
                        NativeProfilerUnit unit = profiler.GetUnit(index);
                        uint percentage = (uint)((long)100 * (index - parentUnit.NItem) / (long)nodesCount);
                        if (percentage % 5 == 0 && needUpdate == false)
                        {
                            needUpdate = true;
                        }
                        else if (percentage % 5 != 0 && needUpdate == true)
                        {
                            needUpdate = false;
                            progressBarControl1.progressBar1.Value = (int)(0.5 * progressBarControl1.progressBar1.Maximum * ((double)(index - parentUnit.NItem) / (double)nodesCount));
                            progressBarControl1.progressBar1.Refresh();
                        }

                        String functionName = "";
                        for (uint count = 0; count < unit.NestingLevel - parentLevel; count++)
                        {
                            functionName += "    ";
                        }
                        functionName += profiler.GetUnitFunctionName(index);
                        uint inclTime = profiler.GetUnitInclTime(index);
                        uint exclTime = profiler.GetUnitExclTime(index);
                        double inclTimePer = 10000.00 * (double)inclTime / (double)(parentInclTime);
                        double exclTimePer = 10000.00 * (double)exclTime / (double)(parentInclTime);

                        inclTimePer = Math.Truncate(inclTimePer) / 100;
                        exclTimePer = Math.Truncate(exclTimePer) / 100;

                        String[] row;

                        if (parentInclTime == 0)
                        {
                            row = new String[] { (index - parentIndex).ToString(), "", "", (profiler.GetUnitNestingLevel(index) - parentLevel).ToString(), functionName, "", "" };
                        }
                        else if (inclTime != exclTime)
                        {
                            row = new String[] { (index - parentIndex).ToString(), inclTimePer.ToString(), exclTimePer.ToString(), (profiler.GetUnitNestingLevel(index) - parentLevel).ToString(), functionName, inclTime.ToString(), exclTime.ToString() };
                        }
                        else
                        {
                            row = new String[] { (index - parentIndex).ToString(), "", exclTimePer.ToString(), (profiler.GetUnitNestingLevel(index) - parentLevel).ToString(), functionName, "", exclTime.ToString() };
                        }
                        dataGridView1.Rows.Add(row);
                        index++;
                    }

                    if (addedTwoLevels[node] == null)
                    {
                        addedTwoLevels[node] = true;
                        node.Nodes.Clear();
                        index = parentUnit.NItem + 1;
                        needUpdate = false;
                        while (profiler.GetUnit(index) != null && profiler.GetUnit(index).EntryTime <= endTime)
                        {
                            NativeProfilerUnit unit = profiler.GetUnit(index);
                            uint percentage = (uint)((long)100 * (index - parentUnit.NItem) / (long)nodesCount);
                            if (percentage % 5 == 0 && needUpdate == false)
                            {
                                needUpdate = true;
                            }
                            else if (percentage % 5 != 0 && needUpdate == true)
                            {
                                needUpdate = false;
                                progressBarControl1.progressBar1.Value = (int)(progressBarControl1.progressBar1.Maximum / 2 + 0.5 * progressBarControl1.progressBar1.Maximum * ((double)(index - parentUnit.NItem) / (double)nodesCount));
                                progressBarControl1.progressBar1.Refresh();
                            }

                            if (unit.NestingLevel <= (parentLevel + 2))
                            {
                                if (profiler.GetUnitExclTime(unit.NItem) > 0)
                                {
                                    if (unit.NestingLevel <= level)
                                    {
                                        for (int count = 0; count <= level - unit.NestingLevel; count++)
                                        {
                                            node = node.Parent;
                                        }
                                        uint inclTime = profiler.GetUnitInclTime(unit.NItem);
                                        TreeNode tempNode = new TreeNode();
                                        tempNode.Text = string.Format("{0,-80}{1}", profiler.GetUnitFunctionName(index), inclTime + " microseconds");
                                        tempNode.Name = string.Format("{0,-80}{1}", profiler.GetUnitFunctionName(index), inclTime + " microseconds");
                                        node.Nodes.Add(tempNode);
                                        node = tempNode;
                                    }
                                    else
                                    {
                                        for (int count = 0; count < unit.NestingLevel - level; count++)
                                        {
                                            TreeNode tempNode = new TreeNode();
                                            tempNode.Text = "unknown function time";
                                            tempNode.Name = "unknown function time";
                                            node.Nodes.Add(tempNode);
                                            node = tempNode;
                                        }
                                        uint inclTime = profiler.GetUnitInclTime(unit.NItem);
                                        node.Text = string.Format("{0,-80}{1}", profiler.GetUnitFunctionName(unit), inclTime + " microseconds");
                                        node.Name = string.Format("{0,-80}{1}", profiler.GetUnitFunctionName(unit), inclTime + " microseconds");
                                    }
                                }
                                level = profiler.GetUnitNestingLevel(index);
                                treeNodes[node] = unit;
                            }
                            index++;
                        }
                    }
                    lastSelectedNode = treeView1.SelectedNode;
                }
            }
            else if (lastSelectedNode != rootNode)
            {
                uint index = 0;
                dataGridView1.DataSource = null;
                dataGridView1.Rows.Clear();
                dataGridView1.Refresh();

                bool needUpdate = false;
                while (profiler.GetUnit(index) != null)
                {
                    NativeProfilerUnit unit = profiler.GetUnit(index);

                    int percentage = (int)((long)100 * (long)index / (long)profiler.GetNMax());
                    if (percentage % 5 == 0 && needUpdate == false)
                    {
                        needUpdate = true;
                    }
                    else if (percentage % 5 != 0 && needUpdate == true)
                    {
                        needUpdate = false;
                        progressBarControl1.progressBar1.Value = (int)(progressBarControl1.progressBar1.Maximum * ((double)index / (double)profiler.GetNMax()));
                        progressBarControl1.progressBar1.Refresh();
                    } 

                    String functionName = "";
                    for (uint count = 0; count < unit.NestingLevel; count++)
                    {
                        functionName += "    ";
                    }
                    functionName += profiler.GetUnitFunctionName(index);
                    uint inclTime = profiler.GetUnitInclTime(index);
                    uint exclTime = profiler.GetUnitExclTime(index);
                    double inclTimePer = 10000.00 * (double)inclTime / (double)profiler.GetTotalRuntime();
                    double exclTimePer = 10000.00 * (double)exclTime / (double)profiler.GetTotalRuntime();

                    inclTimePer = Math.Truncate(inclTimePer) / 100;
                    exclTimePer = Math.Truncate(exclTimePer) / 100;

                    String[] row;
                    if (profiler.GetTotalRuntime() == 0)
                    {
                        row = new String[] { index.ToString(), "", "", (profiler.GetUnitNestingLevel(index)).ToString(), functionName, "", "" };
                    }
                    else if (inclTime != exclTime)
                    {
                        row = new String[] { index.ToString(), inclTimePer.ToString(), exclTimePer.ToString(), (profiler.GetUnitNestingLevel(index)).ToString(), functionName, inclTime.ToString(), exclTime.ToString() };
                    }
                    else
                    {
                        row = new String[] { index.ToString(), "", exclTimePer.ToString(), (profiler.GetUnitNestingLevel(index)).ToString(), functionName, "", exclTime.ToString() };
                    }
                    dataGridView1.Rows.Add(row);
                    index++;
                }
                lastSelectedNode = rootNode;
            }

            progressBarControl1.Hide();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
             this.openFileDialog1.FileName = this.textBox1.Text;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (receivingData == true)
                {
                    float length = (float) m_tOutput.Length;
                    if (length > 1024 * 1024)
                    {
                        length /= (1024 * 1024);
                        this.label1.Text = "Download " + String.Format("{0:f2}", length) + " Mb";
                    }
                    else if(length > 999)
                    {
                        length /= 1024;
                        this.label1.Text = "Download " + String.Format("{0:f1}", length) + " Kb";
                    }
                    else
                    {
                        this.label1.Text = "Download " + m_tOutput.Length.ToString() + " bytes";
                    }
                }
                if (receivingData == true && port != null && port.BytesToRead == 0)
                {
                    timeEnd = DateTime.Now;
                    totalTime = timeEnd.Subtract(System.TimeSpan.FromTicks(timeStart.Ticks));
                    if (totalTime.Millisecond > 500)
                    {
                        receivingData = false;
                        Stop_Connection();

                        // Try reinitialize the port
                        port = new SerialPort(textBox3.Text, 115200, Parity.None, 8, StopBits.One);
                        port.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(port_DataReceived);
                        port.Open();
                        timeStart = DateTime.Now;
                        receivingData = true;
                    }
                }
            }
            catch
            {

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {   
            if (receivingData == false)
            {
                this.saveFileDialog1.ShowDialog();
            }
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            Regex rx = new Regex(@"[\w\W]+\.opf");
            MatchCollection matches = rx.Matches(this.saveFileDialog1.FileName);
            try
            {
                if (matches.Count == 1)
                {
                    TreeNode node = treeView1.SelectedNode;
                    if (node == rootNode || node == null)
                    {
                        profiler.WriteOffViewFormat(this.saveFileDialog1.FileName);
                    }
                    else
                    {
                        NativeProfilerUnit unit = (NativeProfilerUnit)treeNodes[treeView1.SelectedNode];
                        profiler.WriteOffViewFormat(this.saveFileDialog1.FileName, unit);
                    }
                }
                else
                {
                    Stream newFile = new FileStream(this.saveFileDialog1.FileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                    m_tOutput.Seek(0, SeekOrigin.Begin);
                    NativeProfiler.ReadWriteStream(m_tOutput, newFile);
                    m_tOutput.Seek(0, SeekOrigin.End);
                    newFile.Close();
                }
            }                    
            catch(Exception localException)
            {
                System.Windows.Forms.MessageBox.Show(localException.ToString());
            }
            this.saveFileDialog1.FileName = "";
        }
        private void Stop_Connection()
        {
            byte[] buf = null;
            try
            {
                receivingData = false;
                // Save last bytes of data first
                buf = new byte[port.BytesToRead];
                port.Read(buf, 0, buf.Length);
                m_tOutput.Write(buf, 0, buf.Length);
                port.Close();
                if (port.BytesToRead != 0)
                {
                    float length = (float)m_tOutput.Length;
                    if (length > 1024 * 1024)
                    {
                        length /= (1024 * 1024);
                        this.label1.Text = "Download " + String.Format("{0:f2}", length) + " Mb";
                    }
                    else if (length > 999)
                    {
                        length /= 1024;
                        this.label1.Text = "Download " + String.Format("{0:f1}", length) + " Kb";
                    }
                    else
                    {
                        this.label1.Text = "Download " + m_tOutput.Length.ToString() + " bytes";
                    }
                    // Record last time we read something from the port
                    timeStart = DateTime.Now;
                }

            }
            catch
            {

            }

            try
            {
                if (port.IsOpen == true) port.Close();
            }
            catch
            {
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            Stop_Connection();
            this.button1.Text = "Connect to Device";
            try
            {
                Stream testOpen = new FileStream(this.textBox1.Text, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                testOpen.Close();
                try
                {
                    this.openFileDialog2.ShowDialog();
                    float length = (float)m_tOutput.Length;
                    if (length > 1024 * 1024)
                    {
                        length /= (1024 * 1024);
                        this.label1.Text = String.Format("{0:f2}", length) + " Mb in input";
                    }
                    else if (length > 999)
                    {
                        length /= 1024;
                        this.label1.Text = String.Format("{0:f1}", length) + " Kb in input";
                    }
                    else
                    {
                        this.label1.Text = m_tOutput.Length.ToString() + " bytes in input";
                    }
                    LoadRootTree();
                }
                catch
                {
                }
            }
            catch
            {
                MessageBox.Show("Cannot not open map file " + this.textBox1.Text);
            }
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                this.textBox2.Text = this.openFileDialog2.FileName;
                m_tOutput = new FileStream(this.textBox2.Text, FileMode.Open, FileAccess.Read, FileShare.Read);
                this.Refresh();
            }
            catch
            {
                MessageBox.Show("Cannot open imported data file " + this.textBox2.Text);
            }
        }

        public void LoadRootTree()
        {
            profiler = new NativeProfiler(m_tOutput, this.textBox1.Text, checkBlocksBeginEnd);
            if (displayData == false) return;
            progressBarControl1.progressBar1.Value = 0;
            progressBarControl1.Show();
            progressBarControl1.Refresh();

            TreeNode node;
            rootNode = new TreeNode();
            TreeNodeCollection nodes = treeView1.Nodes;
            nodes.Clear();
            nodes.Add(rootNode);
            node = rootNode;
            uint index = 0;
            treeNodes = new Hashtable();
            uint totalInclRuntime = profiler.GetTotalRuntime();

            bool needUpdate = false;
            while (profiler.GetUnit(index) != null)
            {
                NativeProfilerUnit unit = profiler.GetUnit(index);
                uint percentage = (uint)((long)100 * (long)index / (long)profiler.GetNMax());
                if (percentage % 2 == 0 && needUpdate == false)
                {
                    needUpdate = true;  
                }
                else if (percentage % 2 != 0 && needUpdate == true)
                {
                    needUpdate = false;
                    progressBarControl1.progressBar1.Value = (int)(0.5 * progressBarControl1.progressBar1.Maximum * ((double)index / (double)profiler.GetNMax()));
                    progressBarControl1.progressBar1.Refresh();
                }

                uint level = profiler.GetUnitNestingLevel(unit.NItem);
                if (level <= 1 && profiler.GetUnitExclTime(unit.NItem) > 0)
                {
                    if (level != 0 && node.GetNodeCount(true) == 0)
                    {
                        uint inclTime = profiler.GetUnitInclTime(unit.NItem);
                        TreeNode newNode = new TreeNode();
                        newNode.Text = string.Format("{0,-80}{1}", profiler.GetUnitFunctionName(unit), inclTime + " microseconds");
                        newNode.Name = string.Format("{0,-80}{1}", profiler.GetUnitFunctionName(unit), inclTime + " microseconds");
                        node.Nodes.Add(newNode);
                        treeNodes[newNode] = unit;
                    }
                    else if (level == 0)
                    {
                        uint inclTime = profiler.GetUnitInclTime(unit.NItem);
                        TreeNode newNode = new TreeNode();
                        newNode.Text = string.Format("{0,-80}{1}", profiler.GetUnitFunctionName(unit), inclTime + " microseconds");
                        newNode.Name = string.Format("{0,-80}{1}", profiler.GetUnitFunctionName(unit), inclTime + " microseconds");
                        rootNode.Nodes.Add(newNode);
                        node = newNode;
                        treeNodes[node] = unit;
                    }
                }
                index++;
            }

            progressBarControl1.Refresh();
            rootNode.Text = string.Format("{0,-80}{1}", "All functions", totalInclRuntime + " microseconds");
            rootNode.Name = string.Format("{0,-80}{1}", "All functions", totalInclRuntime + " microseconds");
            index = 0;
            dataGridView1.DataSource = null;
            dataGridView1.Rows.Clear();

            needUpdate = false;
            while (profiler.GetUnit(index) != null)
            {
                NativeProfilerUnit unit = profiler.GetUnit(index);
                int percentage = (int)((long)100 * (long)index / (long)profiler.GetNMax());
                if (percentage % 5 == 0 && needUpdate == false)
                {
                    needUpdate = true;
                }
                else if (percentage % 5 != 0 && needUpdate == true)
                {
                    needUpdate = false;
                    progressBarControl1.progressBar1.Value = (int)(progressBarControl1.progressBar1.Maximum / 2 + 0.5 * progressBarControl1.progressBar1.Maximum * ((double)index / (double)profiler.GetNMax()));
                    progressBarControl1.progressBar1.Refresh();
                }

                String functionName = "";
                for (uint count = 0; count < profiler.GetUnitNestingLevel(unit.NItem); count++)
                {
                    functionName += "    ";
                }
                functionName += profiler.GetUnitFunctionName(index);
                uint inclTime = profiler.GetUnitInclTime(index);
                uint exclTime = profiler.GetUnitExclTime(index);
                double inclTimePer = 10000.00 * (double)inclTime / (double)profiler.GetTotalRuntime();
                double exclTimePer = 10000.00 * (double)exclTime / (double)profiler.GetTotalRuntime();
 
                inclTimePer = Math.Truncate(inclTimePer) / 100;
                exclTimePer = Math.Truncate(exclTimePer) / 100;
 
                String[] row;
                if (profiler.GetTotalRuntime() == 0)
                {
                    row = new String[] { index.ToString(), "", "", (profiler.GetUnitNestingLevel(index)).ToString(), functionName, "", "" };
                }
                else if (inclTime != exclTime)
                {
                    row = new String[] { index.ToString(), inclTimePer.ToString(), exclTimePer.ToString(), (profiler.GetUnitNestingLevel(index)).ToString(), functionName, inclTime.ToString(), exclTime.ToString() };
                }
                else
                {
                    row = new String[] { index.ToString(), "", exclTimePer.ToString(), (profiler.GetUnitNestingLevel(index)).ToString(), functionName, "", exclTime.ToString() };
                }
                dataGridView1.Rows.Add(row);
                index++;
            }
            lastSelectedNode = rootNode;
            dataGridView1.Refresh();

            progressBarControl1.Hide();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            checkBlocksBeginEnd = !checkBlocksBeginEnd;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            displayData = !displayData;
        }
    }
}