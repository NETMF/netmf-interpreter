// get the designer to work by commenting this out???
//#define DESIGN_TIME

using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.SPOT.Debugger
{
    public class PropertyPageProjectOptions :
#if DESIGN_TIME
        Microsoft.VisualStudio.Editors.PropertyPages.PropPageUserControlBase
#else
        PropertyPage
#endif
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;
        private Label label1;
        private Label label3;
        private GroupBox deploymentGroupBox;
        private ComboBox m_cbDeployPort;
        private ComboBox m_cbDeployDevice;

        private PropertyDeployPort m_propertyDeployPort;
        private PropertyDeployDevice m_propertyDeployDevice;
        private DebugPort m_selectedDeployPort;

        private ArrayList m_genStubsControlsArray;
        private GroupBox stubGenGroupBox;
        private CheckBox generateNativeStubsForInternalMethodsCheckBox;
        private Button setStubGenDirectoryButton;
        private TextBox createStubFilesInThisDirectoryTextBox;
        private FolderBrowserDialog stubGenFolderBrowser;
        private Label createStubFilesInThisDirectoryLabel;
        private Label rootNameForNativeStubFilesLabel;
        private TextBox rootNameForNativeStubFilesTextBox;

        private PropertyGenStubsChkBox m_propertyGenerateStubsChkBox;
        private PropertyGenStubsRootNameTextBox m_propertyRootStubNameTextBox;
        private CheckBox enablePermiscuousModeCheckBox;
        private PropertyGenStubsDirectoryTextBox m_propertyStubGenDirTextBox;

        private class ComboBoxItem
        {
            public readonly string DisplayName;
            public readonly string PersistName;
            public readonly bool   OnlyEnumeratedValues;

            public ComboBoxItem(string displayName, string persistName, bool onlyEnumValues)
            {
                this.DisplayName = displayName;
                this.PersistName = persistName;
                this.OnlyEnumeratedValues = onlyEnumValues;
            }

            public ComboBoxItem(string name) : this(name, name, false)
            {
            }

            public override string ToString()
            {
                return this.DisplayName;
            }
        }

        private class ComboBoxItemPort : ComboBoxItem
        {
            public readonly DebugPort m_port;

            public ComboBoxItemPort(DebugPort port) : base(port.Name)
            {
                m_port = port;
            }
        }

        private class ComboBoxItemDevice : ComboBoxItem
        {
            public readonly PortDefinition m_pd;
            //emulator stuff

            public ComboBoxItemDevice( Debugger.PortDefinition pd)
                : base(pd.DisplayName, pd.PersistName, pd is PortDefinition_Tcp)
            {
                m_pd = pd;
            }

            public ComboBoxItemDevice(string displayName) : base(displayName, string.Empty, false)
            {
            }
        }

        private DebugPort SelectedDeployPort
        {
            get
            {
                ComboBoxItemPort cbi = (ComboBoxItemPort)this.m_cbDeployPort.SelectedItem;

                if (cbi == null)
                    return null;

                cbi.m_port.EnablePermiscuousWinUSB = this.enablePermiscuousModeCheckBox.Checked;
                return cbi.m_port;
            }
        }

        private Debugger.PortDefinition SelectedPortDefinition
        {
            get
            {
                ComboBoxItemDevice cbi = (ComboBoxItemDevice)this.m_cbDeployDevice.SelectedItem;

                if (cbi == null)
                    return null;

                return cbi.m_pd;
            }
        }

#if DESIGN_TIME
        public object[] Objects { get { return null; } }
#endif

        public PropertyPageProjectOptions()
        {
#if DESIGN_TIME
            Debug.Assert(false);
#endif
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            m_genStubsControlsArray = new ArrayList();
            m_genStubsControlsArray.Add(setStubGenDirectoryButton);
            m_genStubsControlsArray.Add(createStubFilesInThisDirectoryTextBox);
            m_genStubsControlsArray.Add(createStubFilesInThisDirectoryLabel);
            m_genStubsControlsArray.Add(rootNameForNativeStubFilesLabel);
            m_genStubsControlsArray.Add(rootNameForNativeStubFilesTextBox);

            //
            // need to add any constructor code after InitializeComponent call
            //

            m_propertyDeployPort = new PropertyDeployPort(m_cbDeployPort);
            m_propertyDeployDevice = new PropertyDeployDevice(m_cbDeployDevice);

            m_propertyGenerateStubsChkBox = new PropertyGenStubsChkBox(generateNativeStubsForInternalMethodsCheckBox);
            m_propertyRootStubNameTextBox = new PropertyGenStubsRootNameTextBox(rootNameForNativeStubFilesTextBox);
            m_propertyStubGenDirTextBox = new PropertyGenStubsDirectoryTextBox(createStubFilesInThisDirectoryTextBox);

#if !DESIGN_TIME
            AddProperty(m_propertyDeployPort);
            AddProperty(m_propertyDeployDevice);

            AddProperty(m_propertyGenerateStubsChkBox);
            AddProperty(m_propertyRootStubNameTextBox);
            AddProperty(m_propertyStubGenDirTextBox);
#endif
            this.EnableGenStubs();
        }

        private void EnableGenStubs()
        {
            foreach (Control c in m_genStubsControlsArray)
            {
                c.Enabled = generateNativeStubsForInternalMethodsCheckBox.Checked;
            }
        }

        private void InitializeDeployPort()
        {
            m_cbDeployPort.Items.Clear();

            PlatformInfo platformInfo = this.VsProjectFlavorCfg.PlatformInfo;

            foreach (DebugPort port in new DebugPortSupplier().Ports)
            {
                ComboBoxItemPort cbi = new ComboBoxItemPort(port);

                this.m_cbDeployPort.Items.Add(cbi);
            }
        }

        private void InitializeDeployDevice( Debugger.PortDefinition selected)
        {
            //What about EmulatorExe????

            int iSelected = -1;
            DebugPort port = this.SelectedDeployPort;

            if (port == null)
                return;

            PortDefinition[] portDefinitions;

            if (port.IsLocalPort)
            {
                PlatformInfo platformInfo = this.VsProjectFlavorCfg.PlatformInfo;

                PlatformInfo.Emulator[] emulators = platformInfo.Emulators;

                portDefinitions = new PortDefinition[emulators.Length];

                for (int i = 0; i < emulators.Length; i++)
                {
                    portDefinitions[i] = new PlatformInfo.PortDefinition_PeristableEmulator(emulators[i]);
                }
            }
            else
            {
                portDefinitions = port.GetPersistablePortDefinitions();
            }

            m_cbDeployDevice.Items.Clear();

            for (int iPortDefinition = 0; iPortDefinition < portDefinitions.Length; iPortDefinition++)
            {
                PortDefinition pd = portDefinitions[iPortDefinition];

                ComboBoxItemDevice cbi = new ComboBoxItemDevice(pd);
                m_cbDeployDevice.Items.Add(cbi);

                if (Object.Equals(selected, pd))
                {
                    iSelected = m_cbDeployDevice.Items.Count - 1;
                }
            }

            if (m_cbDeployDevice.Items.Count == 0)
            {
                if(selected != null && port.PortFilter == PortFilter.TcpIp && selected is PortDefinition_Tcp)
                {
                    m_cbDeployDevice.Items.Add(new ComboBoxItemDevice(selected));
                }
                else
                {
                    ComboBoxItemDevice cbi = new ComboBoxItemDevice("<none>");
                    m_cbDeployDevice.Items.Insert(0, cbi);
                }
                iSelected = 0;
            }

            if(port.PortFilter != PortFilter.TcpIp)
            {
                iSelected = 0;
            }

            if(iSelected != -1)
            {
                m_cbDeployDevice.SelectedIndex = iSelected;
            }
                
        }

        internal VsProjectFlavorCfg VsProjectFlavorCfgFromObject(object o)
        {
            IVsProjectCfg2 vsProjectCfg = (IVsProjectCfg2)o;
            Guid guid = typeof(IVsDebuggableProjectCfg).GUID;
            IntPtr ip;

            vsProjectCfg.get_CfgType(ref guid, out ip);
            return Marshal.GetObjectForIUnknown(ip) as VsProjectFlavorCfg;
        }

        private VsProjectFlavorCfg[] VsProjectFlavorCfgs
        {
            get
            {
                VsProjectFlavorCfg[] cfgs = new VsProjectFlavorCfg[this.Objects.Length];

                for(int iCfg = 0; iCfg < this.Objects.Length; iCfg++)
                {
                    cfgs[iCfg] = VsProjectFlavorCfgFromObject(this.Objects[iCfg]);
                }

                return cfgs;
            }
        }

        private VsProjectFlavorCfg VsProjectFlavorCfg
        {
            get
            {
                VsProjectFlavorCfg[] cfgs = this.VsProjectFlavorCfgs;

                return cfgs[0];
            }
        }


#if !DESIGN_TIME
        protected override void InitializeProperties()
        {

            VsProjectFlavorCfg vsProjectFlavorCfg = (VsProjectFlavorCfg)m_propertyDeployPort.GetCfgObjectFromObject(this.Objects[0]);

            InitializeDeployPort();
            InitializeDeployDevice(null);

            base.InitializeProperties();
        }
#endif

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

#region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label3 = new System.Windows.Forms.Label();
            this.m_cbDeployPort = new System.Windows.Forms.ComboBox();
            this.deploymentGroupBox = new System.Windows.Forms.GroupBox();
            this.m_cbDeployDevice = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.stubGenGroupBox = new System.Windows.Forms.GroupBox();
            this.rootNameForNativeStubFilesLabel = new System.Windows.Forms.Label();
            this.rootNameForNativeStubFilesTextBox = new System.Windows.Forms.TextBox();
            this.createStubFilesInThisDirectoryLabel = new System.Windows.Forms.Label();
            this.setStubGenDirectoryButton = new System.Windows.Forms.Button();
            this.createStubFilesInThisDirectoryTextBox = new System.Windows.Forms.TextBox();
            this.generateNativeStubsForInternalMethodsCheckBox = new System.Windows.Forms.CheckBox();
            this.stubGenFolderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            this.enablePermiscuousModeCheckBox = new System.Windows.Forms.CheckBox();
            this.deploymentGroupBox.SuspendLayout();
            this.stubGenGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label3.Location = new System.Drawing.Point(17, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 19);
            this.label3.TabIndex = 3;
            this.label3.Text = "Transport:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // m_cbDeployPort
            // 
            this.m_cbDeployPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_cbDeployPort.FormattingEnabled = true;
            this.m_cbDeployPort.Location = new System.Drawing.Point(14, 36);
            this.m_cbDeployPort.Name = "m_cbDeployPort";
            this.m_cbDeployPort.Size = new System.Drawing.Size(255, 21);
            this.m_cbDeployPort.TabIndex = 4;
            this.m_cbDeployPort.SelectedIndexChanged += new System.EventHandler(this.OnDeployPortChanged);
            this.m_cbDeployPort.Click += new System.EventHandler(this.OnDeployDeviceDropDown);
            // 
            // deploymentGroupBox
            // 
            this.deploymentGroupBox.Controls.Add(this.m_cbDeployPort);
            this.deploymentGroupBox.Controls.Add(this.m_cbDeployDevice);
            this.deploymentGroupBox.Controls.Add(this.label1);
            this.deploymentGroupBox.Controls.Add(this.label3);
            this.deploymentGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.deploymentGroupBox.Location = new System.Drawing.Point(1, 5);
            this.deploymentGroupBox.Name = "deploymentGroupBox";
            this.deploymentGroupBox.Size = new System.Drawing.Size(433, 126);
            this.deploymentGroupBox.TabIndex = 1;
            this.deploymentGroupBox.TabStop = false;
            this.deploymentGroupBox.Text = "Deployment";
            // 
            // m_cbDeployDevice
            // 
            this.m_cbDeployDevice.FormattingEnabled = true;
            this.m_cbDeployDevice.Location = new System.Drawing.Point(17, 91);
            this.m_cbDeployDevice.Name = "m_cbDeployDevice";
            this.m_cbDeployDevice.Size = new System.Drawing.Size(252, 21);
            this.m_cbDeployDevice.TabIndex = 6;
            this.m_cbDeployDevice.SelectedIndexChanged += new System.EventHandler(this.m_cbDeployDevice_SelectedIndexChanged);
            this.m_cbDeployDevice.Click += new System.EventHandler(this.OnDeployDeviceDropDown);
            this.m_cbDeployDevice.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OnDeployDeviceKeyPress);
            this.m_cbDeployDevice.Leave += new System.EventHandler(this.OnDeployDeviceLeave);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label1.Location = new System.Drawing.Point(17, 72);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 19);
            this.label1.TabIndex = 5;
            this.label1.Text = "Device:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // stubGenGroupBox
            // 
            this.stubGenGroupBox.Controls.Add(this.rootNameForNativeStubFilesLabel);
            this.stubGenGroupBox.Controls.Add(this.rootNameForNativeStubFilesTextBox);
            this.stubGenGroupBox.Controls.Add(this.createStubFilesInThisDirectoryLabel);
            this.stubGenGroupBox.Controls.Add(this.setStubGenDirectoryButton);
            this.stubGenGroupBox.Controls.Add(this.createStubFilesInThisDirectoryTextBox);
            this.stubGenGroupBox.Location = new System.Drawing.Point(1, 137);
            this.stubGenGroupBox.Name = "stubGenGroupBox";
            this.stubGenGroupBox.Size = new System.Drawing.Size(608, 126);
            this.stubGenGroupBox.TabIndex = 2;
            this.stubGenGroupBox.TabStop = false;
            // 
            // rootNameForNativeStubFilesLabel
            // 
            this.rootNameForNativeStubFilesLabel.AutoSize = true;
            this.rootNameForNativeStubFilesLabel.Location = new System.Drawing.Point(17, 22);
            this.rootNameForNativeStubFilesLabel.Name = "rootNameForNativeStubFilesLabel";
            this.rootNameForNativeStubFilesLabel.Size = new System.Drawing.Size(153, 13);
            this.rootNameForNativeStubFilesLabel.TabIndex = 4;
            this.rootNameForNativeStubFilesLabel.Text = "Root name for native stub files:";
            // 
            // rootNameForNativeStubFilesTextBox
            // 
            this.rootNameForNativeStubFilesTextBox.Location = new System.Drawing.Point(17, 40);
            this.rootNameForNativeStubFilesTextBox.Name = "rootNameForNativeStubFilesTextBox";
            this.rootNameForNativeStubFilesTextBox.Size = new System.Drawing.Size(252, 20);
            this.rootNameForNativeStubFilesTextBox.TabIndex = 3;
            this.rootNameForNativeStubFilesTextBox.TextChanged += new System.EventHandler(this.OnRootNameForNativeStubFilesTextBoxChanged);
            // 
            // createStubFilesInThisDirectoryLabel
            // 
            this.createStubFilesInThisDirectoryLabel.AutoSize = true;
            this.createStubFilesInThisDirectoryLabel.Location = new System.Drawing.Point(17, 72);
            this.createStubFilesInThisDirectoryLabel.Name = "createStubFilesInThisDirectoryLabel";
            this.createStubFilesInThisDirectoryLabel.Size = new System.Drawing.Size(158, 13);
            this.createStubFilesInThisDirectoryLabel.TabIndex = 2;
            this.createStubFilesInThisDirectoryLabel.Text = "Create stub files in this directory:";
            // 
            // setStubGenDirectoryButton
            // 
            this.setStubGenDirectoryButton.Location = new System.Drawing.Point(578, 87);
            this.setStubGenDirectoryButton.Name = "setStubGenDirectoryButton";
            this.setStubGenDirectoryButton.Size = new System.Drawing.Size(24, 23);
            this.setStubGenDirectoryButton.TabIndex = 1;
            this.setStubGenDirectoryButton.Text = "...";
            this.setStubGenDirectoryButton.UseVisualStyleBackColor = true;
            this.setStubGenDirectoryButton.Click += new System.EventHandler(this.setStubGenDirectoryButton_Click);
            // 
            // createStubFilesInThisDirectoryTextBox
            // 
            this.createStubFilesInThisDirectoryTextBox.Location = new System.Drawing.Point(17, 89);
            this.createStubFilesInThisDirectoryTextBox.Name = "createStubFilesInThisDirectoryTextBox";
            this.createStubFilesInThisDirectoryTextBox.Size = new System.Drawing.Size(555, 20);
            this.createStubFilesInThisDirectoryTextBox.TabIndex = 0;
            this.createStubFilesInThisDirectoryTextBox.TextChanged += new System.EventHandler(this.OnStubGenDirectoryTextBoxChanged);
            // 
            // generateNativeStubsForInternalMethodsCheckBox
            // 
            this.generateNativeStubsForInternalMethodsCheckBox.AutoSize = true;
            this.generateNativeStubsForInternalMethodsCheckBox.Location = new System.Drawing.Point(9, 136);
            this.generateNativeStubsForInternalMethodsCheckBox.Name = "generateNativeStubsForInternalMethodsCheckBox";
            this.generateNativeStubsForInternalMethodsCheckBox.Size = new System.Drawing.Size(225, 17);
            this.generateNativeStubsForInternalMethodsCheckBox.TabIndex = 3;
            this.generateNativeStubsForInternalMethodsCheckBox.Text = "Generate native stubs for internal methods";
            this.generateNativeStubsForInternalMethodsCheckBox.UseVisualStyleBackColor = true;
            this.generateNativeStubsForInternalMethodsCheckBox.CheckedChanged += new System.EventHandler(this.generateNativeStubsForInternalMethodsCheckBox_CheckedChanged);
            // 
            // enablePermiscuousModeCheckBox
            // 
            this.enablePermiscuousModeCheckBox.AutoSize = true;
            this.enablePermiscuousModeCheckBox.Location = new System.Drawing.Point(288, 41);
            this.enablePermiscuousModeCheckBox.Name = "enablePermiscuousModeCheckBox";
            this.enablePermiscuousModeCheckBox.Size = new System.Drawing.Size(137, 17);
            this.enablePermiscuousModeCheckBox.TabIndex = 4;
            this.enablePermiscuousModeCheckBox.Text = "Enable legacy WinUSB";
            this.enablePermiscuousModeCheckBox.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.enablePermiscuousModeCheckBox.UseVisualStyleBackColor = true;
            // 
            // PropertyPageProjectOptions
            // 
            this.Controls.Add(this.enablePermiscuousModeCheckBox);
            this.Controls.Add(this.generateNativeStubsForInternalMethodsCheckBox);
            this.Controls.Add(this.stubGenGroupBox);
            this.Controls.Add(this.deploymentGroupBox);
            this.Name = "PropertyPageProjectOptions";
            this.Size = new System.Drawing.Size(614, 364);
            this.deploymentGroupBox.ResumeLayout(false);
            this.stubGenGroupBox.ResumeLayout(false);
            this.stubGenGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public abstract class BuildProperty : PropertyPage.Property
        {
            protected PropertyPageProjectOptions PropertyPageProjectOptions
            {
                #if DESIGN_TIME
                    get { return null; }
                #else
                    get { return this.Page as PropertyPageProjectOptions; }
                #endif
            }

            public override object GetCfgObjectFromObject (object o)
            {
                return this.PropertyPageProjectOptions.VsProjectFlavorCfgFromObject(o);
            }
        }

        public abstract class PropertyComboBox : BuildProperty
        {
            private ComboBox m_cb;

            public PropertyComboBox (ComboBox cb)
            {
                m_cb = cb;

                m_cb.SelectedIndexChanged += new EventHandler (DirtyEventHandler);
                m_cb.SelectionChangeCommitted += new EventHandler(DirtyEventHandler);
                m_cb.TextUpdate += new EventHandler(DirtyEventHandler);
            }

            public override void InitializeFromValue (object val)
            {
                ProjectBuildProperty bp = (ProjectBuildProperty)val;
                string persistedValue = bp.Value;
                bool enumOnly = false;

                for (int iItem = 0; iItem < m_cb.Items.Count; iItem++)
                {
                    ComboBoxItem item = (ComboBoxItem)m_cb.Items[iItem];

                    enumOnly = item.OnlyEnumeratedValues;

                    if (item.PersistName == persistedValue)
                    {
                        m_cb.SelectedIndex = iItem;
                        return;
                    }
                }

                if(!enumOnly)
                {
                    m_cb.Text = bp.Value;
                }
            }

            public override void SaveToValue(object val)
            {
                ProjectBuildProperty bp = (ProjectBuildProperty)val;

                ComboBoxItem cbi = (ComboBoxItem)m_cb.SelectedItem;
                string value;

                if (cbi != null)
                {
                    value = cbi.PersistName;
                }
                else
                {
                    value = m_cb.Text;
                }

                bp.Value = value;
            }

            public override void SetIndeterminate ()
            {
                m_cb.Text = "";
            }
        }

        public abstract class PropertyTextBox : BuildProperty
        {
            protected TextBox m_tb;

            public PropertyTextBox(TextBox tb)
            {
                m_tb = tb;
            }

            public override void InitializeFromValue(object val)
            {
                m_tb.Text = ((ProjectBuildProperty)val).Value;
            }

            public override void SaveToValue(object val)
            {
                ((ProjectBuildProperty)val).Value = m_tb.Text;
            }

            public override void SetIndeterminate()
            {
                m_tb.Text = "";
            }
        }

        public abstract class PropertyCheckBox : BuildProperty
        {
            private CheckBox m_cb;

            public PropertyCheckBox (CheckBox cb)
            {
                m_cb = cb;
                m_cb.CheckStateChanged += new EventHandler (OnCheckStateChanged);
            }

            public override void InitializeFromValue (object val)
            {
                ProjectBuildPropertyBool bpb = (ProjectBuildPropertyBool)val;
                m_cb.Checked = bpb.Value;
                m_cb.ThreeState = false;
            }

            public override void SaveToValue (object val)
            {
                ProjectBuildPropertyBool bpb = (ProjectBuildPropertyBool)val;
                bpb.Value = m_cb.Checked;
            }

            public override void SetIndeterminate ()
            {
                m_cb.ThreeState = true;
                m_cb.CheckState = CheckState.Indeterminate;
            }

            private void OnCheckStateChanged (object sender, EventArgs args)
            {
                if (!InInit)
                {
                    m_cb.ThreeState = false;
                    Dirty ();
                }
            }
        }

        public class PropertyDeployPort : PropertyComboBox
        {
            public PropertyDeployPort(ComboBox cb)
                : base(cb)
            {
            }

            public override object GetValueFromCfgObject (object cfgObject)
            {
                return ((VsProjectFlavorCfg)cfgObject).DeployPropertyPort;
            }
        }

        public class PropertyDeployDevice : PropertyComboBox
        {
            public PropertyDeployDevice(ComboBox cb)
                : base(cb)
            {
            }

            public override object GetValueFromCfgObject(object cfgObject)
            {
                return ((VsProjectFlavorCfg)cfgObject).DeployPropertyDevice;
            }
        }

        public class PropertyGenStubsChkBox : PropertyCheckBox
        {
            public PropertyGenStubsChkBox(CheckBox cb) : base(cb) {}

            public override object GetValueFromCfgObject(object cfgObject)
            {
                return ((VsProjectFlavorCfg)cfgObject).GenerateStubsFlag;
            }
        }

        public class PropertyGenStubsRootNameTextBox : PropertyTextBox
        {
            public PropertyGenStubsRootNameTextBox(TextBox tb) : base(tb) {}

            public override object GetValueFromCfgObject(object cfgObject)
            {
                return ((VsProjectFlavorCfg)cfgObject).GenerateStubsRootName;
            }
        }

        public class PropertyGenStubsDirectoryTextBox : PropertyTextBox
        {
            public PropertyGenStubsDirectoryTextBox(TextBox tb) : base(tb) {}

            public override object GetValueFromCfgObject(object cfgObject)
            {
                return ((VsProjectFlavorCfg)cfgObject).GenerateStubsDirectory;
            }

            public override void SaveToValue(object val)
            {
                string path = m_tb.Text;
                
                if (!path.EndsWith("\\"))
                    path += "\\";
                
                ((ProjectBuildProperty)val).Value = path;
            }
        }

        private bool m_DeployPortInitialized = false;

        private void OnDeployPortChanged(object sender, EventArgs e)
        {
            DebugPort selectedDeployPort = this.SelectedDeployPort;

            Cursor old = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            this.m_selectedDeployPort = selectedDeployPort;
            this.InitializeDeployDevice(null);

            Cursor.Current = old;

            // make sure if the user selected the TCP/IP port that we default to the first item if the cached item was not available
            if(selectedDeployPort.PortFilter == PortFilter.TcpIp)
            {
                if(m_cbDeployDevice.SelectedItem == null)
                {
                    if(m_DeployPortInitialized)
                    {
                        m_cbDeployDevice.SelectedIndex = 0;
                    }
                    else
                    {
                        m_cbDeployDevice.Text = "";
                    }
                }
            }
            
            m_DeployPortInitialized = true;
        }

        void EnsureDeployDevice()
        {
            string deviceName = m_cbDeployDevice.Text;

            ComboBoxItemDevice cbi = (ComboBoxItemDevice)this.m_cbDeployDevice.SelectedItem;

            if (cbi == null)
            {
                //Time to add the item to the dropdown...
                bool fAdd = false;
                CorDebugProcess process = null;

                //try to add the device
                DebugPort port = this.SelectedDeployPort;

                fAdd = (port.PortFilter == PortFilter.TcpIp);

                if (fAdd)
                {
                    process = port.TryAddProcess(this.m_cbDeployDevice.Text);
                }

                if (process != null)
                {
                    InitializeDeployDevice(process.PortDefinition);

                    this.m_propertyDeployDevice.Dirty();
                }
            }
        }

        void OnDeployDeviceLeave(object sender, EventArgs e)
        {
            EnsureDeployDevice();
        }

        void OnDeployDeviceKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                EnsureDeployDevice();
            }
        }

        void OnDeployDeviceDropDown(object sender, EventArgs e)
        {
            switch(m_selectedDeployPort.PortFilter)
            {
                case PortFilter.Usb:
                    InitializeDeployDevice(this.SelectedPortDefinition);
                    break;
            }
        }

        private void setStubGenDirectoryButton_Click(object sender, EventArgs e)
        {
            stubGenFolderBrowser.ShowDialog();
            if ( stubGenFolderBrowser.ShowDialog() == DialogResult.OK )
            {
                createStubFilesInThisDirectoryTextBox.Text = stubGenFolderBrowser.SelectedPath;
                m_propertyStubGenDirTextBox.Dirty();
            }
        }

        private void OnStubGenDirectoryTextBoxChanged(object sender, EventArgs e)
        {
            m_propertyStubGenDirTextBox.Dirty();
        }

        private void OnRootNameForNativeStubFilesTextBoxChanged(object sender, EventArgs e)
        {
            m_propertyRootStubNameTextBox.Dirty();
        }

        private void generateNativeStubsForInternalMethodsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.EnableGenStubs();
        }

        private void m_cbDeployDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnsureDeployDevice();
        }
    }
}

