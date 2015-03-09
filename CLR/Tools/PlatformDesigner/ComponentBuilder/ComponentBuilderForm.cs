using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using XsdInventoryFormatObject;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
//using XsdInventoryFormatObject;
using ComponentObjectModel;
using Microsoft.Build.Construction;

namespace ComponentBuilder
{
    public partial class ComponentBuilderForm : Form
    {
        enum LibraryIncludeEnum
        {
            Include,
            Exclude,
            DebugOnly,
        };

        List<Color> c_TextColors = new List<Color>(new Color[]{ Color.Blue, Color.Black, Color.Red, Color.Purple, Color.Brown, Color.Gray, Color.DarkGreen, Color.YellowGreen } );
        int m_colorIndex = 0;
        const string c_defaultInventoryKey = "____internal____";

        Dictionary<string, Inventory> m_fileToInventory  = new Dictionary<string,Inventory>();
        Dictionary<string, Color> m_fileToColorTable = new Dictionary<string,Color>();

        string m_spoClient = "";

        ImageList m_imageList = new ImageList();

        MsBuildWrapper m_bw;

        public Inventory DefaultInventory
        {
            get
            {
                return m_fileToInventory[c_defaultInventoryKey] as Inventory;
            }
            set
            {
                m_fileToInventory[c_defaultInventoryKey] = value;
            }
        }

        public Inventory GetInventory(string fileName)
        {
            if (fileName == null || fileName.Length == 0)
            {
                return DefaultInventory;
            }

            return m_fileToInventory[fileName.ToLower()] as Inventory;
        }

        public ComponentBuilderForm()
        {
            InitializeComponent();
        }

        private void ComponentBuilderForm_Load(object sender, EventArgs e)
        {
            treeViewInventory.Nodes[0].Expand();

            m_fileToInventory[c_defaultInventoryKey] = new Inventory();

            m_bw = new MsBuildWrapper(DefaultInventory);

            if (m_colorIndex >= c_TextColors.Count) m_colorIndex--; // same color as last opened.
            m_fileToColorTable[c_defaultInventoryKey] = c_TextColors[m_colorIndex++];

            m_spoClient = Environment.GetEnvironmentVariable("SPOCLIENT");
        }

        private TreeNode AddTreeElement(TreeNode parent, string name, object item, bool isNew, object owner, string filePath)
        {
            TreeNode tn = new TreeNode(name);
            filePath = filePath.ToLower();

            tn.Tag = new TreeNodeData( item, isNew, name, owner, filePath );

            if (!m_fileToColorTable.ContainsKey(filePath))
            {
                if (m_colorIndex >= c_TextColors.Count)
                {
                    m_colorIndex--; // the last color
                }
                m_fileToColorTable[filePath] = c_TextColors[m_colorIndex++];
            }

            tn.ForeColor = (Color)m_fileToColorTable[filePath];

            TreeNode folder = parent;

            if (!isNew)
            {
                //add feature folders
                Feature f = item as Feature;
                if (f != null && f.Groups != null && f.Groups.Length > 0)
                {
                    string path = f.Groups.Split(';')[0];

                    foreach (string p in path.Split('\\'))
                    {
                        if (folder.Nodes.ContainsKey(p))
                        {
                            folder = folder.Nodes[p];
                        }
                        else
                        {
                            folder = folder.Nodes.Add(p, p);
                        }
                    }
                }

                // add library folders
                Library lib = item as Library;
                if (lib != null)
                {
                    if (folder.Nodes.ContainsKey(lib.Level.ToString()))
                    {
                        folder = folder.Nodes[lib.Level.ToString()];
                    }
                    else
                    {
                        folder = folder.Nodes.Add(lib.Level.ToString(), lib.Level.ToString());
                    }

                    if (lib.Groups != null && lib.Groups.Length > 0)
                    {
                        string path = lib.Groups.Split(';')[0];

                        foreach (string p in path.Split('\\'))
                        {
                            if (folder.Nodes.ContainsKey(p))
                            {
                                folder = folder.Nodes[p];
                            }
                            else
                            {
                                folder = folder.Nodes.Add(p, p);
                            }
                        }
                    }
                }

                // add library folders
                LibraryCategory libType = item as LibraryCategory;
                if (libType != null)
                {
                    if (folder.Nodes.ContainsKey(libType.Level.ToString()))
                    {
                        folder = folder.Nodes[libType.Level.ToString()];
                    }
                    else
                    {
                        folder = folder.Nodes.Add(libType.Level.ToString(), libType.Level.ToString());
                    }
                }

            }
            // finally add the real node
            folder.Nodes.Add(tn);

            if (item == null) return tn;

            if (item is MFSolution) tn.ContextMenuStrip = contextMenuStripProject;
            else if (item is List<MiscBuildTool>) tn.ContextMenuStrip = contextMenuStripAddMiscTool;
            else if (item is List<MFComponent>) tn.ContextMenuStrip = contextMenuStripDependency;
            else if (item is List<BuildToolRef>) tn.ContextMenuStrip = contextMenuStripAddBuildToolsRef;
            else if (item is Library || item is LibraryCategory || item is MFAssembly ||
                    item is BuildTool || item is Processor || item is MFProject ||
                    item is Feature)
            {
                tn.ContextMenuStrip = contextMenuStripSave;
            }
            if (item is List<BuildParameter> || item is List<MFBuildFile> || item is List<string> || item is List<MFProperty> ||
                item is List<ToolFlag> || item is List<BuildScript> || item is List<ApiTemplate>)
            {
                return tn;
            }

            if (item is List<BuildToolRef>)
            {
                List<string> dtNames = new List<string>();
                string ptName = "";
                string isaGuid = "";
                List<BuildToolRef> btsr = item as List<BuildToolRef>;

                if (owner is ISA)
                {
                    isaGuid = (owner as ISA).Guid;
                }
                else if (owner is Processor)
                {
                    foreach (BuildToolRef btr in btsr)
                    {
                        if (!string.IsNullOrEmpty(btr.BuildOptions.DeviceType))
                        {
                            dtNames.Add(btr.BuildOptions.DeviceType.ToLower());
                        }
                    }
                    
                    ptName = ((Processor)owner).CpuName;
                }

                Dictionary<string, BuildToolRef> hash = new Dictionary<string, BuildToolRef>();

                foreach (BuildToolRef btr in btsr)
                {
                    hash[btr.Guid.ToLower()] = btr;
                    AddTreeElement(tn, btr.Name, btr, isNew, item, filePath);
                }
                if (dtNames.Count > 0 || !string.IsNullOrEmpty(ptName) || !string.IsNullOrEmpty(isaGuid))
                {
                    foreach (Inventory inv in m_fileToInventory.Values)
                    {
                        foreach (BuildTool bt in inv.BuildTools)
                        {
                            if (!string.IsNullOrEmpty(isaGuid))
                            {
                                foreach (ISA ptr in bt.SupportedISAs)
                                {
                                    if (ptr.Guid == isaGuid && !hash.ContainsKey(bt.Guid.ToLower()))
                                    {
                                        BuildToolRef newBtr = new BuildToolRef();
                                        newBtr.Guid = bt.Guid;
                                        newBtr.Name = bt.Name;

                                        btsr.Add(newBtr);
                                        AddTreeElement(tn, newBtr.Name, newBtr, isNew, item, filePath);
                                    }
                                }
                            }
                            if (dtNames.Count > 0)
                            {
                                foreach (string supportedPT in bt.SupportedCpuNames)
                                {
                                    if (dtNames.Contains(supportedPT.ToLower()) && !hash.ContainsKey(bt.Guid.ToLower()))
                                    {
                                        BuildToolRef newBtr = new BuildToolRef();
                                        newBtr.Guid = bt.Guid;
                                        newBtr.Name = bt.Name;

                                        btsr.Add(newBtr);
                                        AddTreeElement(tn, newBtr.Name, newBtr, isNew, item, filePath);

                                        hash[bt.Guid.ToLower()] = newBtr;
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(ptName))
                            {
                                foreach (string supportedPT in bt.SupportedCpuNames)
                                {
                                    if (0 == string.Compare(supportedPT, ptName, true) && !hash.ContainsKey(bt.Guid.ToLower()))
                                    {
                                        BuildToolRef newBtr = new BuildToolRef();
                                        newBtr.Guid = bt.Guid;
                                        newBtr.Name = bt.Name;

                                        btsr.Add(newBtr);
                                        AddTreeElement(tn, newBtr.Name, newBtr, isNew, item, filePath);

                                        hash[bt.Guid.ToLower()] = newBtr;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            else if (item is IList)
            {
                foreach (object o in (IList)item)
                {
                    PropertyInfo pi = o.GetType().GetProperty("Name");
                    if (pi != null)
                    {
                        AddTreeElement(tn, (string)pi.GetValue(o, null), o, isNew, item, filePath);
                    }
                    else
                    {
                        Console.WriteLine("Warning: not expanding node: " + o.GetType().Name);
                    }
                }
            }
            else if (item is ProjectElementContainer)
            {
            }
            else
            {
                foreach (PropertyInfo pi in item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    //if (pi.PropertyType != typeof(List<MFComponent>) && pi.PropertyType != typeof(MFComponent))
                    {
                        if ((pi.Name != "Item") && pi.CanWrite && !pi.PropertyType.IsValueType && !(pi.PropertyType == typeof(string)))
                        {
                            object o = pi.GetValue(item, null);
                            if (o == null)
                            {
                                o = pi.PropertyType.GetConstructor(new Type[] { }).Invoke(null);
                                pi.SetValue(item, o, null);
                            }
                            TreeNode child = AddTreeElement(tn, pi.Name, o, isNew, item, filePath);
                        }
                    }
                }
            }

            return tn;
        }

        Dictionary<string, PropertyInfo> m_NameToPropertyInfoTable = new Dictionary<string, PropertyInfo>();

        public class TreeNodeData
        {
            public TreeNodeData(object tag, bool isNew, string propName, object owner, string filePath)
            {
                Data = tag;
                IsNew = isNew;
                PropName = propName;
                Owner = owner;
                FilePath = filePath;
            }

            public object Data;
            public bool IsNew;
            public string PropName;
            public object Owner;
            public string FilePath;
        }

        public class ContextMenuItemData
        {
            public ContextMenuItemData(DataGridViewRow row, object tag, string name)
            {
                Row = row;
                Data = tag;
                Name = name;
            }
            private ContextMenuItemData()
            {
            }

            public DataGridViewRow Row;
            public object Data;
            public string Name;
        }

        private void OnEnumContextMenuClick(object sender, ToolStripItemClickedEventArgs e)
        {
            ContextMenuItemData cd = e.ClickedItem.Tag as ContextMenuItemData;

            PropertyInfo pi = cd.Data.GetType().GetProperty(cd.Name);

            object val = Enum.Parse(pi.PropertyType, e.ClickedItem.Text);

            pi.SetValue(cd.Data, val, null);

            cd.Row.Cells[1].Value = val;
        }

        private void AddNodeEnumToPropertyGrid(string title, string value, object tag, TreeNode parent, ToolStripItemClickedEventHandler handler)
        {
            AddNodeEnumToPropertyGrid(title, value, tag, parent, handler, false);
        }

        private void AddChildrenToEnum(TreeNode parent, ContextMenuStrip menu, DataGridViewRow row)
        {
            foreach (TreeNode node in parent.Nodes)
            {
                if (node.Tag == null)
                {
                    AddChildrenToEnum(node, menu, row);
                }
                else
                {
                    int idx = 0;
                    for(idx = 0; idx<menu.Items.Count; idx++)
                    {
                        if (0 < string.Compare(menu.Items[idx].Text, node.Text))
                        {
                            break;
                        }
                    }
                    ToolStripItem tsi = new ToolStripMenuItem(node.Text);
                    menu.Items.Insert(idx, tsi);
                    tsi.Tag = new ContextMenuItemData(row, ((TreeNodeData)node.Tag).Data, node.Text);
                }
            }
        }

//        private void AddNodeEnumToPropertyGrid(string title, string value, object tag, ArrayList list, ToolStripItemClickedEventHandler handler, bool addEmtpy)
        //{
        //}

        private void AddNodeEnumToPropertyGrid(string title, string value, object tag, TreeNode parent, ToolStripItemClickedEventHandler handler, bool addEmtpy)
        {
            int row = dataGridView.Rows.Add(title, (value != null && value.Length > 0 ? value : "<Right Click For Options>"));

            dataGridView.Rows[row].Tag = tag;

            DataGridViewCell dgc = dataGridView[1, row];

            dgc.ReadOnly = true;

            if (dgc.ContextMenuStrip == null) dgc.ContextMenuStrip = new ContextMenuStrip();

            dgc.ContextMenuStrip.Items.Clear();

            dgc.ToolTipText = "Right Click For Options";

            dgc.ContextMenuStrip.ItemClicked += handler;

            AddChildrenToEnum(parent, dgc.ContextMenuStrip, dataGridView.Rows[row]);

            if (addEmtpy)
            {
                ToolStripItem tsi = dgc.ContextMenuStrip.Items.Add("<No Selection>");
                tsi.Tag = new ContextMenuItemData(dataGridView.Rows[row], null, "<No Selection>");
            }
        }

        private void AddEnumToPropertyGrid(ContextMenuItemData menuData, Type enumType)
        {
            DataGridViewRow row = menuData.Row;

            if (row.Cells[1].ContextMenuStrip == null) row.Cells[1].ContextMenuStrip = new ContextMenuStrip();

            row.Cells[1].ToolTipText = "Right Click For Options";

            row.Cells[1].ContextMenuStrip.Items.Clear();

            row.Cells[1].ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler(OnEnumContextMenuClick);

            row.Cells[1].ReadOnly = true;

            foreach (string name in Enum.GetNames(enumType))
            {
                ToolStripItem tsi = row.Cells[1].ContextMenuStrip.Items.Add(name);
                tsi.Tag = menuData;
            }
        }

        private void AddPropertiesToGrid(object item)
        {
            if (item is IList)
            {
                dataGridView.AllowUserToAddRows = true;
                foreach (object ob in (IList)item)
                {
                    int idx = dataGridView.Rows.Add(ob);
                }
            }
            else
            {
                foreach (PropertyInfo pi in item.GetType().GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance))
                {
                    if ((pi.PropertyType.IsValueType ||
                       (pi.PropertyType == typeof(string))))
                    {
                        object o = pi.GetValue(item, null);

                        TreeNode parent = treeViewInventory.SelectedNode.Parent;

                        if (GetParentNode(parent) == null &&
                            IsPlatformBuilderRootItem(treeViewInventory.SelectedNode) &&
                            pi.Name == "Name" &&
                            string.IsNullOrEmpty(o as string))
                        {
                            o = treeViewInventory.SelectedNode.Text;
                            pi.SetValue(item, o, null);
                        }

                        int idx = dataGridView.Rows.Add(pi.Name, o);

                        DataGridViewRow dgr = dataGridView.Rows[idx];

                        dgr.ReadOnly = !pi.CanWrite;

                        m_NameToPropertyInfoTable[pi.Name] = pi;

                        if (pi.PropertyType.IsEnum)
                        {
                            AddEnumToPropertyGrid(new ContextMenuItemData(dgr, item, pi.Name), pi.PropertyType);
                        }
                    }
                }
            }
        }

        private void OnLibraryCategoryRefContextMenuItem(object sender, ToolStripItemClickedEventArgs e)
        {
            ContextMenuItemData cd = e.ClickedItem.Tag as ContextMenuItemData;

            LibraryCategory bt = cd.Data as LibraryCategory;
            MFComponent btRef = ((TreeNodeData)treeViewInventory.SelectedNode.Tag).Data as MFComponent;

            if (bt != null && btRef != null)
            {
                btRef.Guid = bt.Guid;
                btRef.Name = bt.Name;
            }
            else
            {
                btRef.Guid = null;
                btRef.Name = null;
            }

            cd.Row.Cells[1].Value = e.ClickedItem.Text;
        }

        private void OnBuildToolContextMenuItem(object sender, ToolStripItemClickedEventArgs e)
        {
            ContextMenuItemData cd = e.ClickedItem.Tag as ContextMenuItemData;

            BuildTool bt = cd.Data as BuildTool;
            BuildToolRef btRef = ((TreeNodeData)treeViewInventory.SelectedNode.Tag).Data as BuildToolRef;
            
            if (bt != null && btRef != null)
            {
                btRef.Guid = bt.Guid;
                btRef.Name = bt.Name;
            }

            cd.Row.Cells[1].Value = e.ClickedItem.Text;
        }

        private void OnLibraryCategoryContextMenuItem(object sender, ToolStripItemClickedEventArgs e)
        {
            ContextMenuItemData cd = e.ClickedItem.Tag as ContextMenuItemData;

            LibraryCategory lc = cd.Data as LibraryCategory;
            MFComponent comp = ((TreeNodeData)treeViewInventory.SelectedNode.Tag).Data as MFComponent;
            
            if (lc != null && comp != null)
            {
                comp.Guid = lc.Guid;
                comp.Name = lc.Name;
                comp.ProjectPath = lc.ProjectPath;
                comp.ComponentType = MFComponentType.LibraryCategory;
            }

            cd.Row.Cells[1].Value = e.ClickedItem.Text;
        }

        private void OnISAContextMenuItem(object sender, ToolStripItemClickedEventArgs e)
        {
            ContextMenuItemData cd = e.ClickedItem.Tag as ContextMenuItemData;

            ISA pt = cd.Data as ISA;
            TreeNodeData tnd = treeViewInventory.SelectedNode.Tag as TreeNodeData;

            if (pt != null && tnd.Owner != null)
            {
                (tnd.Data as MFComponent).Guid = pt.Guid;
                (tnd.Data as MFComponent).Name = pt.Name;
            }

            cd.Row.Cells[1].Value = e.ClickedItem.Text;
        }
        private void OnProcessorContextMenuItem(object sender, ToolStripItemClickedEventArgs e)
        {
            ContextMenuItemData cd = e.ClickedItem.Tag as ContextMenuItemData;

            Processor pt = cd.Data as Processor;
            TreeNodeData tnd = treeViewInventory.SelectedNode.Tag as TreeNodeData;

            if (pt != null && tnd.Owner != null)
            {
                (tnd.Data as MFComponent).Guid = pt.Guid;
                (tnd.Data as MFComponent).Name = pt.Name;
            }

            cd.Row.Cells[1].Value = e.ClickedItem.Text;
        }

        private void treeViewInventory_AfterSelect(object sender, TreeViewEventArgs e)
        {
            dataGridView.Tag = null;
            dataGridView.Rows.Clear();
            dataGridView.Columns[1].Visible = true;
            dataGridView.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView.Columns[0].ReadOnly = true;
            dataGridView.Columns[1].ReadOnly = false;
            //dataGridView.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.Columns[0].HeaderText = "Property";
            dataGridView.Columns[1].HeaderText = "Value";
            //dataGridView.MultiSelect = false;

            while (dataGridView.Columns.Count > 2)
            {
                dataGridView.Columns.RemoveAt(2);
            }

            if (e.Node.Tag != null)
            {
                TreeNodeData tnd = e.Node.Tag as TreeNodeData;

                dataGridView.Tag = tnd.Data;

                /*
                if (tnd.Data is MFComponent)
                {
                    AddNodeEnumToPropertyGrid("Processor", ((MFComponent)tnd.Data).Name, tnd.Data, treeViewInventory.Nodes[0].Nodes["Processors"], new ToolStripItemClickedEventHandler(OnProcessorContextMenuItem), true);
                }
                */
                /*
                else if (tnd.Data is ISARef)
                {
                    AddNodeEnumToPropertyGrid("ISA", ((ISARef)tnd.Data).Name, tnd.Data, treeViewInventory.Nodes[0].Nodes["ISAs"], new ToolStripItemClickedEventHandler(OnISAContextMenuItem));
                }
                */
                if (tnd.Data is BuildToolRef)
                {
                    AddNodeEnumToPropertyGrid("BuildTool", ((BuildToolRef)tnd.Data).Name, tnd.Data, treeViewInventory.Nodes[0].Nodes["BuildTools"], new ToolStripItemClickedEventHandler(OnBuildToolContextMenuItem));
                }
                else if (tnd.Data is MFComponent)
                {
                    MFComponent cmp = (MFComponent)tnd.Data;

                    switch (cmp.ComponentType)
                    {
                        case MFComponentType.LibraryCategory:
                            AddNodeEnumToPropertyGrid("LibraryCategory", cmp.Name, tnd.Data, treeViewInventory.Nodes[0].Nodes["LibraryCategories"], new ToolStripItemClickedEventHandler(OnLibraryCategoryContextMenuItem), true);
                            break;
                        case MFComponentType.Processor:
                            AddNodeEnumToPropertyGrid("Processor", cmp.Name, tnd.Data, treeViewInventory.Nodes[0].Nodes["Processors"], new ToolStripItemClickedEventHandler(OnProcessorContextMenuItem),true);
                            break;
                        case MFComponentType.ISA:
                        case MFComponentType.Library:
                            dataGridView.Columns[0].HeaderText = "Name";
                            dataGridView.Columns[1].HeaderText = "Condition";
                            dataGridView.Rows.Add(cmp.Name, cmp.Conditional);
                            break;
                        case MFComponentType.Feature:
                            if (e.Node.Text == "FeatureAssociation")
                            {
                                AddNodeEnumToPropertyGrid("Feature", cmp.Name, cmp, treeViewInventory.Nodes[0].Nodes["Features"], new ToolStripItemClickedEventHandler(OnFeatureSelect), true);
                            }
                            break;
                    }
                }
                /*
                else if (tnd.Data is LibraryCategoryRef)
                {
                    AddNodeEnumToPropertyGrid("LibraryCategory", ((LibraryCategoryRef)tnd.Data).Name, tnd.Data, treeViewInventory.Nodes[0].Nodes["LibraryCategories"], new ToolStripItemClickedEventHandler(OnLibraryCategoryRefContextMenuItem), true);
                }
                */
                else if (tnd.Data is List<ToolFlag>)
                {
                    dataGridView.Columns[0].HeaderText = "Conditional";
                    dataGridView.Columns[1].HeaderText = "Flags";

                    List<ToolFlag> tfs = (tnd.Data as List<ToolFlag>);
                    if (tfs.Count == 0)
                    {
                        ToolFlag tf = new ToolFlag();
                        tf.Conditional = "'$(FLAVOR)'=='Debug'";
                        tfs.Add(tf);

                        tf = new ToolFlag();
                        tf.Conditional = "'$(FLAVOR)'=='Release'";
                        tfs.Add(tf);

                        tf = new ToolFlag();
                        tf.Conditional = "'$(FLAVOR)'=='RTM'";
                        tfs.Add(tf);
                    }

                    foreach (ToolFlag var in tfs)
                    {
                        dataGridView.Rows.Add(var.Conditional, var.Flag);
                    }

                    dataGridView.Columns[0].ReadOnly = false;
                    //dataGridView.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
                    dataGridView.AllowUserToDeleteRows = true;
                    dataGridView.AllowUserToAddRows = true;
                    //dataGridView.MultiSelect = true;
                }
                else if (tnd.Data is IList<MFProject>)
                {
                }
                else if (tnd.Data is IList<ISA>)
                {
                }
                else if (tnd.Data is IList<BuildToolRef>)
                {
                }
                else if (tnd.Data is List<MemorySymbol>)
                {
                    dataGridView.Columns.Add("Description", "Description");
                    dataGridView.Columns.Add("Conditional", "Conditional");
                    dataGridView.Columns[0].HeaderText = "Name";
                    dataGridView.Columns[1].HeaderText = "Options";
                    dataGridView.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

                    foreach (MemorySymbol var in (tnd.Data as List<MemorySymbol>))
                    {
                        dataGridView.Rows.Add(var.Name, var.Options, var.Description, var.Conditional);
                    }

                    dataGridView.Columns[0].ReadOnly = false;
                    //dataGridView.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
                    dataGridView.AllowUserToDeleteRows = true;
                    dataGridView.AllowUserToAddRows = true;
                }
                else if (tnd.Data is List<BuildParameter>)
                {
                    dataGridView.Columns.Add("Description", "Description");
                    dataGridView.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

                    foreach (BuildParameter var in (tnd.Data as List<BuildParameter>))
                    {
                        dataGridView.Rows.Add(var.Parameter, var.Action, var.Description);
                    }

                    dataGridView.Columns[0].ReadOnly = false;
                    //dataGridView.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
                    dataGridView.AllowUserToDeleteRows = true;
                    dataGridView.AllowUserToAddRows = true;
                }
                else if (tnd.Data is List<BuildParameter>)
                {
                    dataGridView.Columns.Add("Description", "Description");
                    dataGridView.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

                    foreach (BuildParameter var in (tnd.Data as List<BuildParameter>))
                    {
                        dataGridView.Rows.Add(var.Parameter, var.Action, var.Description);
                    }

                    dataGridView.Columns[0].ReadOnly = false;
                    //dataGridView.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
                    dataGridView.AllowUserToDeleteRows = true;
                    dataGridView.AllowUserToAddRows = true;
                }
                else if (tnd.Data is List<MFBuildFile>)
                {
                    dataGridView.Columns[0].HeaderText = "File";
                    dataGridView.Columns[0].ReadOnly = false;
                    dataGridView.Columns[1].HeaderText = "Condition";
                    dataGridView.Columns[1].ReadOnly = false;
                    dataGridView.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    foreach (MFBuildFile file in (List<MFBuildFile>)tnd.Data)
                    {
                        dataGridView.Rows.Add(file.File, file.Condition);
                    }
                    dataGridView.AllowUserToDeleteRows = true;
                    dataGridView.AllowUserToAddRows = true;
                }
                else if (tnd.Data is List<MFProperty>)
                {
                    dataGridView.Columns.Add("Conditional", "Conditional");
                    dataGridView.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

                    foreach (MFProperty prop in (List<MFProperty>)tnd.Data)
                    {
                        dataGridView.Rows.Add(prop.Name, prop.Value, prop.Condition);
                    }

                    dataGridView.Columns[0].ReadOnly = false;
                    dataGridView.AllowUserToDeleteRows = true;
                    dataGridView.AllowUserToAddRows = true;
                }
                else if (tnd.Data is EnvVars)
                {
                    dataGridView.Columns.Add("Conditional", "Conditional");
                    dataGridView.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

                    foreach (EnvVar var in (tnd.Data as EnvVars).EnvVarCollection)
                    {
                        dataGridView.Rows.Add(var.Name, var.Value, var.Conditional);
                    }

                    dataGridView.Columns[0].ReadOnly = false;
                    //dataGridView.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
                    dataGridView.AllowUserToDeleteRows = true;
                    dataGridView.AllowUserToAddRows = true;
                }
                /*
                else if (tnd.Data is List<XsdInventoryFormatObject.MFComponent>)
                {
                    dataGridView.Columns[1].ReadOnly = true;

                    List<XsdInventoryFormatObject.MFComponent> ptrc = (tnd.Data as List<XsdInventoryFormatObject.MFComponent>);

                    dataGridView.Tag = ptrc;

                    Type t = (e.Node.Parent.Tag as TreeNodeData).Data.GetType();

                    foreach (Inventory inv in m_fileToInventory.Values)
                    {
                        //todo:
                        foreach (ISA pt in inv.ISAs)
                        {
                            XsdInventoryFormatObject.MFComponent theProcType = null;

                            foreach (XsdInventoryFormatObject.MFComponent ptr in ptrc)
                            {
                                if (ptr.Guid == pt.Guid)
                                {
                                    theProcType = ptr;
                                    break;
                                }
                            }

                            int idx = dataGridView.Rows.Add(pt.Name, theProcType != null);

                            if (theProcType == null)
                            {
                                theProcType = new XsdInventoryFormatObject.MFComponent();
                                theProcType.Guid = pt.Guid;
                                theProcType.Name = pt.Name;
                            }

                            dataGridView.Rows[idx].Tag = theProcType;
                            dataGridView.Rows[idx].Cells[1].ToolTipText = "Double-Click to Change";
                        }
                    }
                }
                */
                else if (tnd.Data is List<MFComponent>)
                {
                    //dataGridView.Columns[1].DataPropertyName = "Name";

                    if (e.Node.Text == "FeatureAssociations")
                    {
                        dataGridView.AllowUserToDeleteRows = true;
                        foreach (MFComponent featCmp in (List<MFComponent>)tnd.Data)
                        {
                            AddNodeEnumToPropertyGrid("Feature", featCmp.Name, featCmp, treeViewInventory.Nodes[0].Nodes["Features"], new ToolStripItemClickedEventHandler (OnFeatureSelect), true);
                        }
                    }

                    /*
                    dataGridView.Columns.Add();
                    dataGridView.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

                    dataGridView.Columns[2].HeaderText = "Condition";
                    dataGridView.Columns[2].ReadOnly = false;

                    foreach (MFComponent c in (List<MFComponent>)tnd.Data)
                    {
                        dataGridView.Rows.Add(c.Name, c.Conditional);
                    }
                    */

                    /*
                    List<MFComponent> cc = (tnd.Data as List<MFComponent>);


                    Type t = (e.Node.Parent.Tag as TreeNodeData).Data.GetType();
                    TreeNodeData parentData = (e.Node.Parent.Tag as TreeNodeData);
                    if ((t == typeof(Feature) && e.Node.Text == "Libraries") ||
                        (t == typeof(MFSolution) && e.Node.Text == "Include") ||
                        (t == typeof(Library)) || (t == typeof(LibraryCategory)))
                    {
                        int colIdx = dataGridView.Columns.Add("Conditional", "Conditional");
                        dataGridView.Columns[colIdx].ReadOnly = false;
                        dataGridView.Columns[colIdx].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                        colIdx = dataGridView.Columns.Add("Type", "Type");
                        dataGridView.Columns[colIdx].ReadOnly = true;
                        dataGridView.Columns[colIdx].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                        colIdx = dataGridView.Columns.Add("Description", "Description");
                        dataGridView.Columns[colIdx].ReadOnly = true;
                        dataGridView.Columns[colIdx].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

                        foreach (Inventory inv in m_fileToInventory.Values)
                        {
                            if (t != typeof(MFSolution))
                            {
                                foreach (LibraryCategory libType in inv.LibraryCategories)
                                {
                                    MFComponent theComp = null;
                                    foreach (MFComponent c in cc)
                                    {
                                        if (c.Guid == libType.Guid)
                                        {
                                            theComp = c;
                                            break;
                                        }
                                    }

                                    string conditional = "";
                                    if (theComp != null)
                                    {
                                        conditional = theComp.Conditional;
                                    }

                                    int idx = dataGridView.Rows.Add(libType.Name, theComp != null, conditional, "LibaryType: " + libType.Level, libType.Description);

                                    if (theComp == null)
                                    {
                                        theComp = new MFComponent(MFComponentType.LibraryCategory);
                                        theComp.Guid = libType.Guid;
                                        theComp.Name = libType.Name;
                                    }

                                    dataGridView.Rows[idx].Tag = theComp;
                                    dataGridView.Rows[idx].Cells[1].ToolTipText = "Double-Click to Change";
                                }
                            }

                            if (e.Node.Text != "LibraryCategories")
                            {
                                foreach (Library lib in inv.Libraries)
                                {
                                    // do not alow circular dependencies
                                    if (t == typeof(Library) && ((Library)((TreeNodeData)e.Node.Parent.Tag).Data).Guid.Equals(lib.Guid))
                                    {
                                        continue;
                                    }
                                    // nothing execept for a MFSolution should have a dependency on a HAL library
                                    if (t != typeof(MFSolution) && lib.Level == LibraryLevel.HAL)
                                    {
                                        continue;
                                    }

                                    // only show library types as only platforms should be able to include explicit libraries.
                                    if (t != typeof(MFSolution) && (lib.LibraryCategory != null) && !string.IsNullOrEmpty(lib.LibraryCategory.Guid))
                                    {
                                        continue;
                                    }

                                    MFComponent theComp = null;
                                    foreach (MFComponent c in cc)
                                    {
                                        if (c.Guid == lib.Guid)
                                        {
                                            theComp = c;
                                            break;
                                        }
                                    }

                                    string conditional = "";
                                    if (theComp != null)
                                    {
                                        conditional = theComp.Conditional;
                                    }

                                    int idx = dataGridView.Rows.Add(lib.Name, theComp != null || (t == typeof(MFSolution) && lib.Required), conditional, lib.Level, lib.Description);

                                    if (theComp == null)
                                    {
                                        theComp = new MFComponent(MFComponentType.Library);
                                        theComp.Guid = lib.Guid;
                                        theComp.Name = lib.Name;

                                        if (t == typeof(MFSolution) && lib.Required)
                                        {
                                            (tnd.Data as List<MFComponent>).Add(theComp);
                                        }
                                    }

                                    dataGridView.Rows[idx].Tag = theComp;
                                    dataGridView.Rows[idx].Cells[1].ToolTipText = "Double-Click to Change";
                                }
                            }
                        }
                    }
                    if ((t == typeof(Feature) && e.Node.Text == "Dependencies") ||
                        (t == typeof(MFSolution) && e.Node.Text == "Features"))
                    {
                        int colIdx = dataGridView.Columns.Add("Conditional", "Conditional");
                        dataGridView.Columns[colIdx].ReadOnly = false;
                        dataGridView.Columns[colIdx].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                        colIdx = dataGridView.Columns.Add("Description", "Description");
                        dataGridView.Columns[colIdx].ReadOnly = true;
                        dataGridView.Columns[colIdx].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

                        foreach (Inventory inv in m_fileToInventory.Values)
                        {
                            foreach (Feature feat in inv.Features)
                            {
                                if (t == typeof(Feature) && ((Feature)((TreeNodeData)e.Node.Parent.Tag).Data).Guid.Equals(feat.Guid))
                                {
                                    continue;
                                }

                                MFComponent theComp = null;
                                foreach (MFComponent c in cc)
                                {
                                    if (c.Guid == feat.Guid)
                                    {
                                        theComp = c;
                                        break;
                                    }
                                }

                                string conditional = "";
                                if (theComp != null)
                                {
                                    conditional = theComp.Conditional;
                                }

                                int idx = dataGridView.Rows.Add(feat.Name, theComp != null, conditional, feat.Description);

                                if (theComp == null)
                                {
                                    theComp = new MFComponent(MFComponentType.Feature);
                                    theComp.Guid = feat.Guid;
                                    theComp.Name = feat.Name;
                                }

                                dataGridView.Rows[idx].Tag = theComp;
                                dataGridView.Rows[idx].Cells[1].ToolTipText = "Double-Click to Change";

                            }
                        }
                    }
                     */
                }
                else if (tnd.Data.GetType() == typeof(List<BuildScript>))
                {
                    dataGridView.Columns[0].HeaderText = "Conditional";
                    dataGridView.Columns[1].HeaderText = "Build Commands";

                    foreach (BuildScript bs in (tnd.Data as List<BuildScript>))
                    {
                        dataGridView.Rows.Add(bs.Conditional, bs.Script);
                    }
                    dataGridView.Columns[0].ReadOnly = false;
                    //dataGridView.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
                    dataGridView.AllowUserToDeleteRows = true;
                    dataGridView.AllowUserToAddRows = true;
                }
                else if (tnd.Data.GetType() == typeof(List<ApiTemplate>))
                {
                    dataGridView.Columns[0].HeaderText = "File";
                    dataGridView.Columns[1].HeaderText = "Description";

                    foreach (ApiTemplate bs in (tnd.Data as List<ApiTemplate>))
                    {
                        dataGridView.Rows.Add(bs.FilePath, bs.Description);
                    }
                    dataGridView.Columns[0].ReadOnly = false;
                    //dataGridView.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
                    dataGridView.AllowUserToDeleteRows = true;
                    dataGridView.AllowUserToAddRows = true;
                }
                /*
            else if (tnd.Data is MFComponent)
            {
                // stub library
                TreeNodeData parentTnd = e.Node.Parent.Tag as TreeNodeData;
                XsdInventoryFormatObject.Library lib = parentTnd.Data as XsdInventoryFormatObject.Library;
                XsdInventoryFormatObject.LibraryCategory type = parentTnd.Data as XsdInventoryFormatObject.LibraryCategory;

                if (lib != null || type != null)
                {
                    if (lib == null || !lib.IsStub)
                    {
                        string typeGuid = type != null ? type.Guid : (lib.LibraryCategory != null) ? lib.LibraryCategory.Guid : "";
                        string stubGuid = ((MFComponent)tnd.Data).Guid;

                        bool fGuidCheck = !string.IsNullOrEmpty(typeGuid);

                        dataGridView.Columns[0].HeaderText = "Stub Library";
                        dataGridView.Columns[1].HeaderText = "Value";
                        dataGridView.Columns.Add("FileColumn", "File");

                        foreach (Inventory inv in m_fileToInventory.Values)
                        {
                            foreach (Library stub in inv.Libraries)
                            {
                                if (stub.IsStub)
                                {
                                    bool check = (0 == string.Compare(stubGuid, stub.Guid, true));

                                    if (fGuidCheck)
                                    {
                                        if (stub.LibraryCategory != null && (0 == string.Compare(typeGuid, stub.LibraryCategory.Guid, true)))
                                        {
                                            int row = dataGridView.Rows.Add(stub.Name, check, stub.LibraryFile);
                                            dataGridView.Rows[row].Tag = stub;
                                        }
                                    }
                                    else
                                    {
                                        int row = dataGridView.Rows.Add(stub.Name, check, stub.LibraryFile);
                                        dataGridView.Rows[row].Tag = stub;
                                    }
                                }
                            }
                        }

                        //AddNodeEnumToPropertyGrid("Stub Library", lib != null? lib.Name: type.Name, tnd.Data, treeViewInventory.Nodes[0].Nodes["Processors"], new ToolStripItemClickedEventHandler(OnProcessorContextMenuItem), true);
                    }
                }
            }
            */
                else
                {
                    AddPropertiesToGrid((e.Node.Tag as TreeNodeData).Data);
                }
            }
            dataGridView.Invalidate();
            if (dataGridView.Rows.Count > 0)
            {
                dataGridView.Sort(dataGridView.Columns[0], ListSortDirection.Ascending);
            }
        }
        void OnFeatureSelect(object sender, ToolStripItemClickedEventArgs args)
        {
            ContextMenuItemData cd = args.ClickedItem.Tag as ContextMenuItemData;

            Feature feat = cd.Data as Feature;

            if (feat != null)
            {
                TreeNode tn = treeViewInventory.SelectedNode;
                MFComponent feature = ((TreeNodeData)tn.Tag).Data as MFComponent;

                if (feature != null)
                {
                    feature.Guid = feat.Guid;
                    feature.Name = feat.Name;
                    feature.ProjectPath = feat.ProjectPath;
                    cd.Row.Cells[1].Value = feat.Name;
                }
                
                List<MFComponent> features = ((TreeNodeData)tn.Tag).Data as List<MFComponent>;

                if (features != null)
                {
                    int row = cd.Row.Index;
                    features[row].Guid = feat.Guid;
                    features[row].Name = feat.Name;
                    features[row].ProjectPath = feat.ProjectPath;
                    //dataGridView.Columns[1].DataPropertyName = "Name";
                    cd.Row.Cells[1].Value = feat.Name;
                }
            }
        }

        void SaveGridView()
        {
            dataGridView.EndEdit();
            if (dataGridView.Tag is EnvVars)
            {
                List<EnvVar> evc = (dataGridView.Tag as EnvVars).EnvVarCollection;

                evc.Clear();
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (row.Cells[0].Value != null)
                    {
                        EnvVar env = new EnvVar();
                        env.Name = row.Cells[0].Value.ToString();
                        env.Value = (row.Cells[1].Value == null ? "" : row.Cells[1].Value.ToString());
                        env.Conditional = row.Cells[2].Value == null? "" : row.Cells[2].Value.ToString();
                        evc.Add(env);
                    }
                }
            }
            else if (dataGridView.Tag is List<ToolFlag>)
            {
                List<ToolFlag> flags = (dataGridView.Tag as List<ToolFlag>);

                flags.Clear();
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (row.Cells[0].Value != null || row.Cells[1].Value != null)
                    {
                        ToolFlag flag = new ToolFlag();
                        flag.Conditional = (row.Cells[0].Value == null ? "" : row.Cells[0].Value.ToString());
                        flag.Flag = (row.Cells[1].Value == null ? "" : row.Cells[1].Value.ToString());
                        flags.Add(flag);
                    }
                }
            }
            else if (dataGridView.Tag is List<BuildParameter>)
            {
                List<BuildParameter> parms = (dataGridView.Tag as List<BuildParameter>);

                parms.Clear();
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (row.Cells[0].Value != null && row.Cells[1].Value != null)
                    {
                        BuildParameter parm = new BuildParameter();
                        parm.Parameter   = (string)row.Cells[0].Value;
                        parm.Action      = (string)row.Cells[1].Value;
                        parm.Description = (string)row.Cells[2].Value;
                        parms.Add(parm);
                    }
                }
            }
            else if (dataGridView.Tag is List<MemorySymbol>)
            {
                List<MemorySymbol> msc = dataGridView.Tag as List<MemorySymbol>;

                msc.Clear();
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (row.Cells[0].Value != null && row.Cells[1].Value != null)
                    {
                        MemorySymbol parm = new MemorySymbol();
                        parm.Name        = (string)row.Cells[0].Value;
                        parm.Options     = (string)row.Cells[1].Value;
                        parm.Description = (string)row.Cells[2].Value;
                        parm.Conditional = (string)row.Cells[3].Value;
                        msc.Add(parm);
                    }
                }
            }
            else if (dataGridView.Tag is List<BuildScript>)
            {
                List<BuildScript> al = dataGridView.Tag as List<BuildScript>;
                al.Clear();

                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (row.Cells[1].Value != null)
                    {
                        BuildScript bs = new BuildScript();
                        bs.Conditional = row.Cells[0].Value as string;
                        bs.Script = row.Cells[1].Value as string;

                        al.Add(bs);
                    }
                }
            }
            else if (dataGridView.Tag is List<ApiTemplate>)
            {
                List<ApiTemplate> al = dataGridView.Tag as List<ApiTemplate>;
                al.Clear();

                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (!string.IsNullOrEmpty(row.Cells[0].Value as string))
                    {
                        ApiTemplate at = new ApiTemplate();
                        at.FilePath = row.Cells[0].Value as string;
                        at.Description = row.Cells[1].Value as string;

                        al.Add(at);
                    }
                }
            }
            else if (dataGridView.Tag is MFComponent)
            {
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    /*
                    if ((bool)row.Cells[1].Value)
                    {
                        Library lib = row.Tag as Library;
                        MFComponent comp = dataGridView.Tag as MFComponent;
                        comp.Guid = lib.Guid;
                        comp.Name = lib.Name;
                        break;
                    }
                    */
                }
            }
        }

        void dataGridView_Leave(object sender, EventArgs e)
        {
            SaveGridView();
        }

        private void addPlatformToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode parent = treeViewInventory.Nodes[0].Nodes["Solutions"];
            MFSolution newPlat = new MFSolution();

            newPlat.Guid = Guid.NewGuid().ToString("B").ToUpper();

            TreeNode newNode = AddTreeElement(parent, "<New Solution>", newPlat, true, DefaultInventory.Solutions, c_defaultInventoryKey);

            DefaultInventory.Solutions.Add(newPlat);

            treeViewInventory.SelectedNode = newNode;
            newNode.BeginEdit();
        }

        private void addFeatureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode parent = treeViewInventory.Nodes[0].Nodes["Features"];
            Feature newFeat = new Feature();

            newFeat.Guid = Guid.NewGuid().ToString("B").ToUpper();

            TreeNode newNode = AddTreeElement(parent, "<New Feature>", newFeat, true, DefaultInventory.Features, c_defaultInventoryKey);

            DefaultInventory.Features.Add(newFeat);

            treeViewInventory.SelectedNode = newNode;
            newNode.BeginEdit();
        }

        private bool IsRootNodeOf(TreeNode root, TreeNode node, ref string path)
        {
            if( node == null) return false;

            path = "";

            while (node != null && node != root)
            {
                if (node.Tag == null)
                {
                    path = node.Text + "\\" + path;
                }
                node = node.Parent;
            }

            return node == root;
        }

        private void addLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode parent = treeViewInventory.Nodes[0].Nodes["Libraries"];
            TreeNode sel = treeViewInventory.SelectedNode;
            string path = "";

            if (IsRootNodeOf(parent, sel, ref path))
            {
                while (sel.Tag != null)
                {
                    sel = sel.Parent;
                }
                parent = sel;
            }

            Library newLib  = new Library();

            string []folders = path.Split('\\');
            switch (folders[0].ToUpper())
            {
                case "CLR":
                    newLib.Level = LibraryLevel.CLR;
                    break;
                case "HAL":
                    newLib.Level = LibraryLevel.HAL;
                    break;
                case "PAL":
                    newLib.Level = LibraryLevel.PAL;
                    break;
                case "SUPPORT":
                    newLib.Level = LibraryLevel.Support;
                    break;
            }

            newLib.Groups = string.Join("\\", folders, 1, folders.Length - 1).TrimEnd('\\'); 
            newLib.Guid = Guid.NewGuid().ToString("B").ToUpper();

            TreeNode newNode = AddTreeElement(parent, "<New Library>", newLib, true, DefaultInventory.Libraries, c_defaultInventoryKey);

            DefaultInventory.Libraries.Add(newLib);

            newNode.EnsureVisible();
            treeViewInventory.SelectedNode = newNode;
            newNode.BeginEdit();
        }

        private void addProcessorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode parent = treeViewInventory.Nodes[0].Nodes["Processors"];
            Processor newProcType = new Processor();

            newProcType.Guid = Guid.NewGuid().ToString("B").ToUpper();

            TreeNode newNode = AddTreeElement(parent, "<New Processor>", newProcType, true, DefaultInventory.Processors, c_defaultInventoryKey);

            DefaultInventory.Processors.Add(newProcType);

            treeViewInventory.SelectedNode = newNode;
            newNode.BeginEdit();
        }

        private void addBuildToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode parent = treeViewInventory.Nodes[0].Nodes["BuildTools"];
            BuildTool newTool = new BuildTool();

            newTool.Guid = Guid.NewGuid().ToString("B").ToUpper();

            TreeNode newNode = AddTreeElement(parent, "<New Build Tool>", newTool, true, DefaultInventory.BuildTools, c_defaultInventoryKey);

            DefaultInventory.BuildTools.Add(newTool);

            treeViewInventory.SelectedNode = newNode;
            newNode.BeginEdit();
        }

        private TreeNode GetParentNode(TreeNode tn)
        {
            TreeNode parent = tn.Parent;

            while (parent != null && parent.Tag == null)
            {
                parent = parent.Parent;
            }

            return parent;
        }

        private void treeViewInventory_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node.Tag != null)
            {
                TreeNodeData tnd = e.Node.Tag as TreeNodeData;
                TreeNodeData parentData = e.Node.Parent.Tag as TreeNodeData;

                if ((GetParentNode(e.Node) == null && 
                    IsPlatformBuilderRootItem(e.Node)) ||
                    (parentData != null && parentData.Data.GetType().IsSubclassOf(typeof(ArrayList))))
                {
                    return;
                }
            }
            e.CancelEdit = true;
        }

        private void treeViewInventory_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node.Tag != null && !string.IsNullOrEmpty(e.Label))
            {
                TreeNodeData tnd = e.Node.Tag as TreeNodeData;

                PropertyInfo pi = tnd.Data.GetType().GetProperty("Name");

                if (pi != null)
                {
                    if (tnd.Data is MFSolution)
                    {
                        ((MFSolution)tnd.Data).Rename(e.Label);
                    }

                    pi.SetValue(tnd.Data, e.Label, null);
                }

                treeViewInventory_AfterSelect(null, new TreeViewEventArgs(e.Node));
            }

        }

        private void dataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
        }

        private bool IsPlatformBuilderRootItem(TreeNode tn)
        {
            return (tn != null && tn.Parent != null && tn.Parent.Parent != null && tn.Parent.Parent.Parent == null);
        }

        private void dataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (dataGridView.Tag is List<MFComponent> && (e.ColumnIndex == 2))
            {
                MFComponent comp = dataGridView.Rows[e.RowIndex].Tag as MFComponent;

                if (comp != null)
                {
                    dataGridView.EndEdit();
                    comp.Conditional = (string)dataGridView[e.ColumnIndex, e.RowIndex].Value;
                }
            }
            else if (dataGridView.Rows[e.RowIndex].Tag == null && 
                     dataGridView.Columns[0].ReadOnly)
            {
                string propName = (string)dataGridView.Rows[e.RowIndex].Cells[0].Value;
                object propValue = dataGridView.Rows[e.RowIndex].Cells[1].Value;

                if (propName == null) return;

                PropertyInfo pi = m_NameToPropertyInfoTable[propName] as PropertyInfo;

                if (pi == null) return;

                if (propValue == null || pi.PropertyType == propValue.GetType())
                {
                    pi.SetValue((treeViewInventory.SelectedNode.Tag as TreeNodeData).Data, propValue, null);
                }
                else if (pi.PropertyType == typeof(int))
                {
                    pi.SetValue((treeViewInventory.SelectedNode.Tag as TreeNodeData).Data, int.Parse(propValue as string), null);
                }

                if (string.Compare(propName, "Name", true) == 0 && !string.IsNullOrEmpty(propValue as string))
                {
                    object o = (treeViewInventory.SelectedNode.Tag as TreeNodeData).Data;
                    TreeNode parent = treeViewInventory.SelectedNode.Parent;

                    if (o != null && (IsPlatformBuilderRootItem(treeViewInventory.SelectedNode) || (parent != null && parent.Tag != null && (parent.Tag as TreeNodeData).Data.GetType().IsSubclassOf(typeof(ArrayList)))))
                    {
                        if (o is MFSolution)
                        {
                            ((MFSolution)o).Rename(propValue.ToString());
                        }
                        treeViewInventory.SelectedNode.Text = propValue.ToString();
                    }
                }
            }
        }

        private void LoadInventory(Inventory inv, string fileName)
        {
            TreeNode tnc = treeViewInventory.Nodes[0].Nodes["Solutions"];
            foreach (MFSolution p in inv.Solutions)
            {
                AddTreeElement(tnc, p.Name, p, false, inv.Solutions, fileName);
            }
            tnc = treeViewInventory.Nodes[0].Nodes["Features"];
            foreach (Feature p in inv.Features)
            {
                AddTreeElement(tnc, p.Name, p, false, inv.Features, fileName);
            }
            tnc = treeViewInventory.Nodes[0].Nodes["Libraries"];
            foreach (Library p in inv.Libraries)
            {
                AddTreeElement(tnc, p.Name, p, false, inv.Libraries, fileName);
            }
            tnc = treeViewInventory.Nodes[0].Nodes["BuildTools"];
            foreach (BuildTool p in inv.BuildTools)
            {
                AddTreeElement(tnc, p.Name, p, false, inv.BuildTools, fileName);
            }
            tnc = treeViewInventory.Nodes[0].Nodes["Processors"];
            foreach (Processor p in inv.Processors)
            {
                AddTreeElement(tnc, p.Name, p, false, inv.Processors, fileName);
            }
            tnc = treeViewInventory.Nodes[0].Nodes["BuildParameters"];
            string name = fileName == c_defaultInventoryKey ? "Default" : Path.GetFileNameWithoutExtension(fileName);
            AddTreeElement(tnc, name, inv.BuildParameters, false, inv, fileName);

            tnc = treeViewInventory.Nodes[0].Nodes["LibraryCategories"];
            foreach (LibraryCategory p in inv.LibraryCategories)
            {
                AddTreeElement(tnc, p.Name, p, false, inv.LibraryCategories, fileName);
            }
            tnc = treeViewInventory.Nodes[0].Nodes["Assemblies"];
            foreach (MFAssembly p in inv.Assemblies)
            {
                AddTreeElement(tnc, p.Name, p, false, inv.Assemblies, fileName);
            }

            /*
            tnc = treeViewInventory.Nodes[0].Nodes["BSPs"];
            foreach (BSP p in inv.BSPs)
            {
                AddTreeElement(tnc, p.Name, p, false, inv.BSPs, fileName);
            }
            */

            treeViewInventory.Sort();
        }

        private void LoadFile(string file)
        {
            XmlSerializer xser = new XmlSerializer(typeof(Inventory));

            using (StreamReader sr = File.OpenText(file))
            {
                Inventory inv = xser.Deserialize(sr) as Inventory;

                m_fileToInventory[file.ToLower()] = inv;

                LoadInventory(inv, file);

                AddFileMenus(file);
            }
        }

        void OnSaveFileClick(object sender, EventArgs e)
        {
            ToolStripMenuItem tmi = sender as ToolStripMenuItem;

            if (tmi != null)
            {
                string file = (string)tmi.Tag;

                Inventory inv = m_fileToInventory[file.ToLower()] as Inventory;

                if (DefaultInventory.Libraries.Count    > 0 || DefaultInventory.Solutions.Count       > 0 ||
                    DefaultInventory.Features.Count     > 0 || DefaultInventory.Processors.Count      > 0 ||
                    DefaultInventory.BuildTools.Count   > 0 || 
                    DefaultInventory.LibraryCategories.Count > 0 || DefaultInventory.BuildParameters.Count > 0)
                {
                    DialogResult dr = MessageBox.Show(this, "Would you like to save new items to this file?", "Save New Items", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (DialogResult.Yes == dr)
                    {
                        inv = CollapseInventory(file, DefaultInventory, inv);
                    }
                    else if (DialogResult.Cancel == dr)
                    {
                        return;
                    }
                }

                if (!SaveFile(file, inv))
                {
                    MessageBox.Show(this, "Error: Unable to save to " + file, "Save File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        void AddFileMenus(string file)
        {
            ToolStripMenuItem tmiFile = menuStrip1.Items["fileToolStripMenuItem"] as ToolStripMenuItem;

            ToolStripMenuItem tmi = tmiFile.DropDownItems["saveToolStripMenuItem"] as ToolStripMenuItem;

            ToolStripMenuItem tmiNew = new ToolStripMenuItem(Path.GetFileName(file));

            tmiNew.Click += new EventHandler(OnSaveFileClick);
            tmiNew.Tag = file;
            tmiNew.Name = file.ToLower();
            tmi.DropDownItems.Insert(0, tmiNew);

            tmi = tmiFile.DropDownItems["closeToolStripMenuItem"] as ToolStripMenuItem;
            tmiNew = new ToolStripMenuItem(Path.GetFileName(file));

            tmiNew.Click += new EventHandler(OnCloseFileClick);
            tmiNew.Tag = file;
            tmiNew.Name = file.ToLower();
            tmi.DropDownItems.Insert(0, tmiNew);
        }

        void RemoveFileMenus(string file)
        {
            string key = file.ToLower();

            ToolStripMenuItem tmiFile = menuStrip1.Items["fileToolStripMenuItem"] as ToolStripMenuItem;

            ToolStripMenuItem tmi2 = tmiFile.DropDownItems["saveToolStripMenuItem"] as ToolStripMenuItem;
            tmi2.DropDownItems.RemoveByKey(key);

            tmi2 = tmiFile.DropDownItems["closeToolStripMenuItem"] as ToolStripMenuItem;
            tmi2.DropDownItems.RemoveByKey(key);
        }

        void RemoveInventory(string file)
        {
            string key = file.ToLower();

            m_fileToInventory.Remove(key);
            int idx = c_TextColors.IndexOf((Color)m_fileToColorTable[key]);
            m_fileToColorTable.Remove(key);

            Color tmp = c_TextColors[idx];
            c_TextColors[idx] = c_TextColors[m_colorIndex - 1];
            c_TextColors[m_colorIndex - 1] = tmp;
            m_colorIndex--;
        }

        void OnCloseFileClick(object sender, EventArgs e)
        {
            ToolStripMenuItem tmi = sender as ToolStripMenuItem;

            if (tmi != null)
            {
                string file = tmi.Tag as string;

                RemoveFileMenus(file);
                RemoveInventory(file);

                RefreshTree();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private List<Inventory> HashToInventoryList()
        {
            List<Inventory> invs = new List<Inventory>(m_fileToInventory.Values.Count);
            foreach (Inventory inv in m_fileToInventory.Values)
            {
                if (inv == DefaultInventory)
                {
                    invs.Insert(0, inv);
                }
                else
                {
                    invs.Add(inv);
                }
            }
            return invs;
        }

        private void generatePlatformToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            TreeNodeData tnd = treeViewInventory.SelectedNode.Tag as TreeNodeData;

            if(tnd != null)
            {

                FolderBrowserDialog fbd = new FolderBrowserDialog();

                //fbd.StartLocation = FolderBrowser.FolderID.MyComputer;
                //fbd.Title = "Please select the client root directory";
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPOCLIENT")))
                {
                    fbd.SelectedPath = Environment.GetEnvironmentVariable("SPOCLIENT");
                }
                fbd.ShowNewFolderButton = true;

                if (DialogResult.OK == fbd.ShowDialog(this))
                {
                    ComponentObjectModel.MsBuildWrapper wrapper = new ComponentObjectModel.MsBuildWrapper(HashToInventoryList());
                    wrapper.SaveSolution(tnd.Data as MFSolution, fbd.SelectedPath);

                    ComponentObjectModel.ScatterfileWrapper scatter = new ComponentObjectModel.ScatterfileWrapper(tnd.Data as MFProject);
                    scatter.GenerateScatterFile(fbd.SelectedPath + @"\tools\targets");

                    ComponentObjectModel.BuildScriptWrapper script = new ComponentObjectModel.BuildScriptWrapper(HashToInventoryList());
                    script.ProduceBuildScript(fbd.SelectedPath, "msbuild_dotNetMF.cmd");
                }
            }
            */
        }

        private Inventory CollapseInventory(string filePath, params Inventory[] invParams)
        {
            Inventory allInv = new Inventory();
            List<Inventory> invList;

            SaveGridView();

            if(invParams == null || invParams.Length == 0)
            {
                invList = new List<Inventory>();

                foreach (Inventory inv in m_fileToInventory.Values)
                {
                    invList.Add(inv);
                }
            }
            else
            {
                invList = new List<Inventory>(invParams);
            }

            foreach (Inventory inv in invList)
            {
                allInv.Solutions.AddRange(inv.Solutions);
                allInv.Features.AddRange(inv.Features);
                allInv.Libraries.AddRange(inv.Libraries);
                allInv.Processors.AddRange(inv.Processors);
                allInv.BuildTools.AddRange(inv.BuildTools);
                allInv.BuildParameters.AddRange(inv.BuildParameters);
                allInv.LibraryCategories.AddRange(inv.LibraryCategories);
                allInv.Assemblies.AddRange(inv.Assemblies);
            }

            // now lets update the tree with the new file assignment
            List<string> removeKeys = new List<string>();

            foreach (string key in m_fileToInventory.Keys)
            {
                if (string.Compare(key, filePath, true) != 0 &&
                    invList.Contains(m_fileToInventory[key] as Inventory))
                {
                    removeKeys.Add(key);
                }
            }
            foreach (string key in removeKeys)
            {
                if (key == c_defaultInventoryKey)
                {
                    m_fileToInventory[key] = new Inventory();
                }
                else
                {
                    RemoveFileMenus(key);
                    RemoveInventory(key);
                }
            }

            m_fileToInventory[filePath.ToLower()] = allInv;

            RefreshTree();

            return allInv;
        }

        private bool SaveFile(string filePath, Inventory inv)
        {
            try
            {
                SaveGridView();

                ComponentObjectModel.InventoryHelper helper = new ComponentObjectModel.InventoryHelper(inv);

                helper.ValidateData();

                XmlWriterSettings set = new XmlWriterSettings();
                set.Indent = true;
                set.NewLineChars = "\r\n";

                XmlWriter xw = XmlWriter.Create(filePath, set);

                XmlSerializer xser = new XmlSerializer(typeof(Inventory));

                xser.Serialize(xw, inv);

                xw.Close();
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void allToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            sfd.OverwritePrompt = true;

            if (DialogResult.OK == sfd.ShowDialog(this))
            {
                Inventory invAll = CollapseInventory(sfd.FileName);

                if (!SaveFile(sfd.FileName, invAll))
                {
                    MessageBox.Show(this, "Error: Unable to save to " + sfd.FileName, "Save File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    AddFileMenus(sfd.FileName);
                }
            }
        }

        private void RefreshTree()
        {
            treeViewInventory.Nodes[0].Nodes["Solutions"].Nodes.Clear();
            treeViewInventory.Nodes[0].Nodes["Features"].Nodes.Clear();
            treeViewInventory.Nodes[0].Nodes["Libraries"].Nodes.Clear();
            treeViewInventory.Nodes[0].Nodes["BuildTools"].Nodes.Clear();
            treeViewInventory.Nodes[0].Nodes["Processors"].Nodes.Clear();
            treeViewInventory.Nodes[0].Nodes["BuildParameters"].Nodes.Clear();
            treeViewInventory.Nodes[0].Nodes["LibraryCategories"].Nodes.Clear();
            treeViewInventory.Nodes[0].Nodes["Assemblies"].Nodes.Clear();
            //treeViewInventory.Nodes[0].Nodes["BSPs"].Nodes.Clear();

            foreach (string key in m_fileToInventory.Keys)
            {
                LoadInventory(m_fileToInventory[key] as Inventory, key);
            }

            treeViewInventory.Sort();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RefreshTree();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            sfd.OverwritePrompt = true;

            if (DialogResult.OK == sfd.ShowDialog(this))
            {
                if (!SaveFile(sfd.FileName, DefaultInventory))
                {
                    MessageBox.Show(this, "Error: Unable to save to " + sfd.FileName, "Save File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    m_fileToInventory[sfd.FileName.ToLower()] = DefaultInventory;
                    DefaultInventory = new Inventory();
                    AddFileMenus(sfd.FileName);
                }
            }

        }

        private void allToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            List<string> removeKeys = new List<string>();
            foreach (string file in m_fileToInventory.Keys)
            {
                if (file != c_defaultInventoryKey)
                {
                    removeKeys.Add(file);
                }
            }

            m_fileToInventory[c_defaultInventoryKey] = new Inventory();
            m_bw = new MsBuildWrapper(DefaultInventory);
            foreach (string file in removeKeys)
            {
                RemoveFileMenus(file);
                RemoveInventory(file);
            }
            RefreshTree();           
        }

        private void dataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 1) return;

            if (dataGridView[1, e.RowIndex].Value is bool)
            {
                dataGridView[1, e.RowIndex].Value = !(bool)dataGridView[1, e.RowIndex].Value;
                dataGridView.InvalidateCell(1, e.RowIndex);

                if (dataGridView.Tag is List<MFComponent>)
                {
                    MFComponent comp = dataGridView.Rows[e.RowIndex].Tag as MFComponent;
                    List<MFComponent> cl = dataGridView.Tag as List<MFComponent>;

                    if ((bool)dataGridView[1, e.RowIndex].Value)
                    {
                        cl.Add(comp);
                    }
                    else
                    {
                        cl.Remove(comp);
                    }
                }
                    /*
                else if (dataGridView.Tag is XsdInventoryFormatObject.ISARef)
                {
                    XsdInventoryFormatObject.ISARef ptf = dataGridView.Rows[e.RowIndex].Tag as XsdInventoryFormatObject.ISARef;

                    ISARefCollection ptrc = dataGridView.Tag as ISARefCollection;

                    if ((bool)dataGridView[1, e.RowIndex].Value)
                    {
                        ptrc.Add(ptf);
                    }
                    else
                    {
                        ptrc.Remove(ptf);
                    }
                }
                     */
            }
        }

        private void addMemoryRegionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode tn = treeViewInventory.SelectedNode;
            TreeNodeData tnd = tn.Tag as TreeNodeData;

            if (tnd != null)
            {
                MemoryRegion mr = new MemoryRegion();
                mr.Name = "<New Memory Region>";
//                mr.Order = tn.Nodes.Count;
                (tnd.Data as List<MemoryRegion>).Add(mr);
                TreeNode child = AddTreeElement(tn, mr.Name, mr, true, tnd.Data, tnd.FilePath);
                child.EnsureVisible();
                treeViewInventory.SelectedNode = child;
                child.BeginEdit();
            }
        }

        private void addMemorySectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode tn = treeViewInventory.SelectedNode;
            TreeNodeData tnd = treeViewInventory.SelectedNode.Tag as TreeNodeData;

            if (tnd != null)
            {
                MemorySection ms = new MemorySection();
                ms.Name = "<New Memory Section>";
                ms.Order = tn.Nodes.Count;
                (tnd.Data as List<MemorySection>).Add(ms);
                TreeNode child = AddTreeElement(tn, ms.Name, ms, true, tnd.Data, tnd.FilePath);
                child.EnsureVisible();
                treeViewInventory.SelectedNode = child;
                child.BeginEdit();
            }
        }

        private void treeViewInventory_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeViewInventory.SelectedNode = e.Node;
            }
        }

        private void addMemoryMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode tn = treeViewInventory.SelectedNode;
            TreeNodeData tnd = treeViewInventory.SelectedNode.Tag as TreeNodeData;

            if (tnd != null)
            {
                MemoryMap ms = new MemoryMap();
                ms.Name = "<New Memory Map>";
                (tnd.Data as List<MemoryMap>).Add(ms);
                TreeNode child = AddTreeElement(tn, ms.Name, ms, true, tnd.Data, tnd.FilePath);
                child.EnsureVisible();
                treeViewInventory.SelectedNode = child;
                child.BeginEdit();
            }
        }

        private void addEnvironmentalVariableSetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode tn = treeViewInventory.SelectedNode;
            TreeNodeData tnd = treeViewInventory.SelectedNode.Tag as TreeNodeData;

            if (tnd != null)
            {
                EnvVars ms = new EnvVars();
                ms.Name = "<New Env Var Set>";
                (tnd.Data as List<EnvVars>).Add(ms);
                TreeNode child = AddTreeElement(tn, ms.Name, ms, true, tnd.Data, tnd.FilePath);
                child.EnsureVisible();
                treeViewInventory.SelectedNode = child;
                child.BeginEdit();
            }
        }

        private void addLibraryCategoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode parent = treeViewInventory.Nodes[0].Nodes["LibraryCategories"];
            LibraryCategory newLibType = new LibraryCategory();

            newLibType.Guid = Guid.NewGuid().ToString("B").ToUpper();

            TreeNode newNode = AddTreeElement(parent, "<New Library Type>", newLibType, true, DefaultInventory.LibraryCategories, c_defaultInventoryKey);

            DefaultInventory.LibraryCategories.Add(newLibType);

            treeViewInventory.SelectedNode = newNode;
            newNode.BeginEdit();

        }

        private void treeViewInventory_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    if (treeViewInventory.SelectedNode != null && treeViewInventory.SelectedNode.Tag != null)
                    {
                        TreeNodeData tnd = treeViewInventory.SelectedNode.Tag as TreeNodeData;

                        if (tnd.Owner is ArrayList)
                        {
                            (tnd.Owner as ArrayList).Remove(tnd.Data);
                            treeViewInventory.Nodes.Remove(treeViewInventory.SelectedNode);
                            e.Handled = true;
                        }
                    }
                    break;
            }
        }

        private void dataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.V:
                    if (e.Modifiers == Keys.Control)
                    {
                        try
                        {
                            if (Clipboard.ContainsText())
                            {
                                string[] rows = Clipboard.GetText().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                                if (rows.Length > 0)
                                {
                                    string[] columns = rows[0].Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

                                    for (int i = 0; i < dataGridView.Columns.Count; i++)
                                    {
                                        if (i >= columns.Length) return;

                                        if (dataGridView.Columns[i].HeaderText != columns[i]) return;
                                    }

                                    for (int i = 1; i < rows.Length; i++)
                                    {
                                        // first char is always \t
                                        dataGridView.Rows.Add(rows[i].Substring(1, rows[i].Length - 1).Split('\t'));
                                    }
                                    e.Handled = true;
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                    break;
            }
        }

        private void addMiscToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNodeData tnd = treeViewInventory.SelectedNode.Tag as TreeNodeData;

            if (tnd != null)
            {
                List<MiscBuildTool> mtc = tnd.Data as List<MiscBuildTool>;

                MiscBuildTool tool = new MiscBuildTool();
                tool.Guid = Guid.NewGuid().ToString("B").ToUpper();
                tool.Name = "<New Misc Tool>";
                
                mtc.Add(tool);

                treeViewInventory.SelectedNode = AddTreeElement(treeViewInventory.SelectedNode, tool.Name, tool, true, mtc, tnd.FilePath);
            
                treeViewInventory.SelectedNode.BeginEdit();
            }
        }

        private void loadMemoryMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNodeData tnd = treeViewInventory.SelectedNode.Tag as TreeNodeData;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "Scatter Files (*.xml)|*.xml|All Files (*.*)|*.*";
            ofd.CheckFileExists = true;

            if (DialogResult.OK == ofd.ShowDialog(this))
            {
                ComponentObjectModel.ScatterfileWrapper scatter = new ComponentObjectModel.ScatterfileWrapper(tnd.Data as MFProject);
                List<MemoryMap> maps = scatter.LoadFromFile(ofd.FileName);
                MFSolution solution = tnd.Data as MFSolution;

                //TODO: FIX
                solution.Projects[0].MemoryMap = maps[0];

                this.RefreshTree();
            }
        }

        private void loadBuildToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNodeData tnd = treeViewInventory.SelectedNode.Tag as TreeNodeData;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Target File (*.targets)|*.targets|All Files (*.*)|*.*";
            ofd.Multiselect = false;
            ofd.CheckFileExists = true;

            if (DialogResult.OK == ofd.ShowDialog())
            {
                BuildTool tool = m_bw.LoadBuildToolFromTargetFile(ofd.FileName);

                //DefaultInventory.BuildTools.Add(tool);

                RefreshTree();
            }
        }

        private void ComponentBuilderForm_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void loadFeaturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Inventory> lst = HashToInventoryList();
            InventoryHelper invH = new InventoryHelper(lst);
            string spoclient = Environment.GetEnvironmentVariable("SPOCLIENT");

            m_bw.LoadDefaultFeatures(spoclient);

            RefreshTree();
        }

        private void saveLibraryCategoriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Inventory> lst = HashToInventoryList();
            InventoryHelper invH = new InventoryHelper(lst);

            foreach (LibraryCategory libc in invH.LibraryCategories)
            {
                m_bw.SaveLibraryCategoryProj(libc);
            }
        }

        private void addDependencyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode tn = treeViewInventory.SelectedNode;

            if (tn == null) return;

            TreeNodeData tnd = treeViewInventory.SelectedNode.Tag as TreeNodeData;

            if (tnd != null)
            {
                List<MFComponent> items = tnd.Data as List<MFComponent>;

                if (items != null)
                {
                    switch (tn.Text)
                    {
                        case "FeatureAssociations":
                            MFComponent cmp = new MFComponent(MFComponentType.Feature, "Select Feature");
                            items.Add(cmp);
                            AddNodeEnumToPropertyGrid("Feature", cmp.Name, cmp, treeViewInventory.Nodes[0].Nodes["Features"], new ToolStripItemClickedEventHandler(OnFeatureSelect));
                            break;
                    }
                }
            }
        }

        private void cloneSolutionToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            TreeNode tn = treeViewInventory.SelectedNode;

            if(tn == null) return;

            TreeNodeData tnd = (tn.Tag as TreeNodeData);

            if (tnd != null)
            {
                MFSolution sol = tnd.Data as MFSolution;

                if (sol != null)
                {
                    MFSolution solNew = new MFSolution();

                    sol.CopyTo(solNew, "<Cloned " + sol.Name + " Solution>");

                    this.DefaultInventory.Solutions.Add(solNew);

                    RefreshTree();
                }
            }
        }

        private void saveProcessorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_bw.SaveProcessorProj(((TreeNodeData)treeViewInventory.SelectedNode.Tag).Data as Processor);
        }

        private void loadProcessorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.Filter = "Processor Settings (*.settings)|*.settings";
            ofd.Title = "Load Processor Settings";

            if (DialogResult.OK == ofd.ShowDialog())
            {
                Processor proc = m_bw.LoadProcessorProj(ofd.FileName, "");

                RefreshTree();
            }
        }

        private void saveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            object tag = treeViewInventory.SelectedNode.Tag;

            if(tag != null && tag is TreeNodeData)
            {
                TreeNodeData tnd = (TreeNodeData)tag;

                     if(tnd.Data is Library)         m_bw.SaveLibraryProj((Library)tnd.Data);
                else if(tnd.Data is LibraryCategory) m_bw.SaveLibraryCategoryProj((LibraryCategory)tnd.Data);
                else if(tnd.Data is MFAssembly )     m_bw.SaveAssemblyProj((MFAssembly)tnd.Data);
                else if(tnd.Data is BuildTool)       m_bw.SaveTargetFile((BuildTool)tnd.Data);
                else if(tnd.Data is Processor)       m_bw.SaveProcessorProj((Processor)tnd.Data);
                else if(tnd.Data is MFProject)       m_bw.SaveProjectProj((MFProject)tnd.Data);
                else if(tnd.Data is Feature)         m_bw.SaveFeatureProj((Feature)tnd.Data);
            }
        }

        private void addBuildToolOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<BuildToolRef> refs = ((TreeNodeData)treeViewInventory.SelectedNode.Tag).Data as List<BuildToolRef>;

            if (refs != null)
            {
                BuildToolRef rf = new BuildToolRef();
                rf.Name = "<NEW>";
                refs.Add(rf);
                RefreshTree();
            }
        }

        private void loadSolutionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.Filter = "Solution Settings (*.settings)|*.settings";
            ofd.Title = "Load Solution Settings";

            if (DialogResult.OK == ofd.ShowDialog())
            {
                m_bw.LoadSolutionProj(ofd.FileName, "");

                RefreshTree();
            }
        }

        private void generateProjectToolStripMenuItem1_Click(object sender, EventArgs e)
        {
             object tag = treeViewInventory.SelectedNode.Tag;

             if (tag != null && tag is TreeNodeData)
             {
                 TreeNodeData tnd = (TreeNodeData)tag;

                 if (tnd.Data is MFSolution) m_bw.SaveSolutionProj((MFSolution)tnd.Data);
             }
        }

        private void loadLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.Filter = "Library Project (*.proj)|*.proj";
            ofd.Title = "Load Library Project";

            if (DialogResult.OK == ofd.ShowDialog())
            {
                m_bw.LoadLibraryProj(ofd.FileName, "");

                RefreshTree();
            }
        }

        private void loadLibraryCategoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.Filter = "Library Category Project (*.libcatproj)|*.libcatproj";
            ofd.Title = "Load Library Category Project";

            if (DialogResult.OK == ofd.ShowDialog())
            {
                m_bw.LoadLibraryCategoryProj(ofd.FileName, "");

                RefreshTree();
            }
        }

        private void loadLibraryCategoriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string spoclient = Environment.GetEnvironmentVariable("SPOCLIENT");

            m_bw.LoadDefaultLibraryCategories(spoclient);

            RefreshTree();
        }

        private void loadLibrariesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string spoclient = Environment.GetEnvironmentVariable("SPOCLIENT");

            m_bw.LoadDefaultLibraries(spoclient);

            RefreshTree();
        }

        private void loadAllBuildToolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string spoclient = Environment.GetEnvironmentVariable("SPOCLIENT");

            m_bw.LoadDefaultBuildTargets(spoclient);

            RefreshTree();
        }

        private void loadAllProcessorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string spoclient = Environment.GetEnvironmentVariable("SPOCLIENT");
            
            m_bw.LoadDefaultProcessors(spoclient);

            RefreshTree();
        }

        private void saveAssembliesToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void loadAllAssembliesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string spoclient = Environment.GetEnvironmentVariable("SPOCLIENT");

            m_bw.LoadDefaultAssemblies(spoclient);

            RefreshTree();
        }

        private void loadAllComponentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string spoclient = Environment.GetEnvironmentVariable("SPOCLIENT");

            m_bw.LoadAllComponents(spoclient);

            RefreshTree();
        }

        private void loadFromXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string spoclient = Environment.GetEnvironmentVariable("SPOCLIENT");

            if (string.IsNullOrEmpty(spoclient))
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.ShowNewFolderButton = false;
                fbd.Description = "Select Porting Kit Root Folder (eg. d:\\port\\client_v3_0)";

                if (DialogResult.OK == fbd.ShowDialog())
                {
                    Environment.SetEnvironmentVariable("SPOCLIENT", fbd.SelectedPath);
                    spoclient = fbd.SelectedPath;
                }
                else
                {
                    return;
                }
            }

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.CheckFileExists = true;
            ofd.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";

            if (DialogResult.OK == ofd.ShowDialog())
            {
                try
                {
                    LoadFile(ofd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Error: " + ex.Message, "Inventory Load Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void loadAllSolutionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string spoclient = Environment.GetEnvironmentVariable("SPOCLIENT");
            
            m_bw.LoadSolutions(spoclient + "\\Solutions");

            RefreshTree();
        }


    }
}