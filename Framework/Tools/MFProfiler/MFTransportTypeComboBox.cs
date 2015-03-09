using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using Microsoft.SPOT.Debugger;

namespace Microsoft.NetMicroFramework.Tools.MFProfilerTool
{
    internal class TransportTypeItem
    {
        public TransportTypeItem(PortFilter id, string text)
        {
            m_id = id;
            m_text = text;
        }

        private PortFilter m_id;
        private string m_text;

        public string Text
        {
            get { return m_text; }
        }

        public PortFilter Id
        {
            get { return m_id; }
        }
    }

    [System.ComponentModel.DefaultEvent("SelectedValueChanged"),
     System.ComponentModel.Designer (typeof( MFPortFilterComboBoxDesigner )),
     System.ComponentModel.Description( ".NET MF Transport Type Selector Control" )]
    public partial class MFPortFilterComboBox : System.Windows.Forms.ComboBox
    {
        private const string c_DesignText = ".NETMF Transport Filter";

        public MFPortFilterComboBox() : base()
        {
            base.ValueMember = "Id";
            base.DisplayMember = "Text";
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            base.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            base.FormattingEnabled = true;

            //this.Site is null in the constructor; wait for the OnCreateControl method to determine Design/Run-time mode.
            if (this.DesignMode)
            {
                base.Items.Add(c_DesignText);
                base.SelectedIndex = 0;
            }
            else
            {
                ArrayList ar = new ArrayList();
                ar.AddRange(new TransportTypeItem[] {
                new TransportTypeItem(PortFilter.Serial, "Serial"),
                new TransportTypeItem(PortFilter.Usb, "USB"),
                new TransportTypeItem(PortFilter.TcpIp, "TCP/IP"),
                new TransportTypeItem(PortFilter.Emulator, "Emulator")});
                base.DataSource = ar;
            }
        }

        [Browsable(false)]
        public PortFilter[] Filter
        {
            get
            {
                if (base.SelectedValue == null)
                {
                    return null;
                }
                else
                {
                    return new PortFilter[] { (PortFilter)base.SelectedValue };
                }
            }
        }
    }

    public class MFPortFilterComboBoxDesigner : System.Windows.Forms.Design.ControlDesigner
    {
        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            base.AutoResizeHandles = true;
        }

        protected override void PostFilterProperties(IDictionary properties)
        {
            properties.Remove("AutoCustomSource");
            properties.Remove("AutoCompleteMode");
            properties.Remove("AutoCompleteSource");
            properties.Remove("BindingContext");
            properties.Remove("DataBindings");
            properties.Remove("DataSource");
            properties.Remove("DisplayMember");
            properties.Remove("DropDownStyle");
            properties.Remove("Items");
            properties.Remove("ValueMember");
        }

        public override SelectionRules SelectionRules
        {
            get
            {
                SelectionRules selectionRules = base.SelectionRules;
                return (selectionRules & ~(SelectionRules.BottomSizeable | SelectionRules.TopSizeable));
            }
        }
    }
}

