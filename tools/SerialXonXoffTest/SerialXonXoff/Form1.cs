using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace SerialXonXoff
{
    public partial class Form1 : Form
    {
        bool m_bStarted = true;
        bool m_connected = false;
        GenericAsyncStream m_gas;
        Thread m_readTh;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (m_gas == null) return;
            if (m_bStarted)
            {
                button1.Text = "Start";
                m_gas.WriteByte((byte)19);
            }
            else
            {
                button1.Text = "Stop";
                richTextBox1.Invoke(new MethodInvoker(delegate
                {
                    richTextBox1.AppendText("\r\n\r\n");
                }));
                m_gas.WriteByte((byte)17);
            }
            m_bStarted = !m_bStarted;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.DataSource = PortDefinition.Enumerate(PortFilter.Serial);
            comboBox1.ValueMember = "DisplayName";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (m_connected)
            {
                button2.Text = "Connect";
                m_readTh.Abort();
                m_readTh.Join();
                m_readTh = null;
                m_gas.Close();
                m_gas.Dispose();
                m_gas = null;
            }
            else
            {
                PortDefinition pd = comboBox1.SelectedItem as PortDefinition;
                m_gas = pd.Open();
                m_readTh = new Thread(new ThreadStart(ReadThread));
                m_readTh.Start();
                button2.Text = "Disconnect";
            }
            m_connected = !m_connected;
        }

        private void ReadThread()
        {
            byte[] buffer = new byte[20];
            while (true)
            {
                int cnt = m_gas.Read(buffer, 0, buffer.Length);
                richTextBox1.Invoke(new MethodInvoker(delegate
                {
                    richTextBox1.AppendText(ASCIIEncoding.ASCII.GetString(buffer, 0, cnt));
                }));
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_readTh != null && m_readTh.IsAlive)
            {
                m_readTh.Abort();
                m_readTh = null;
            }
            if (m_gas != null)
            {
                m_gas.Close();
                m_gas.Dispose();
                m_gas = null;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox1.Invoke(new MethodInvoker(delegate
            {
                richTextBox1.Clear();
            }));

        }
    }
}