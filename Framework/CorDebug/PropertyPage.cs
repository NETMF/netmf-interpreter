using System;
using System.Runtime.InteropServices;
using System.Drawing;
using Debug = System.Diagnostics.Debug;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;

namespace Microsoft.SPOT.Debugger
{    
    [ComVisible(true)]
    [Guid("88A29501-7C71-4577-94A4-BF7871040B01")]    
    public class PropertyPage : Microsoft.VisualStudio.Editors.PropertyPages.PropPageUserControlBase
    {        
#region Property
        public abstract class Property
        {
            private bool m_fDirty;
            private PropertyPage m_page;
            private bool m_fInInit;
            
            internal PropertyPage Page
            {
                set 
                { 
                    Debug.Assert (m_page == null);  
                    m_page = value; 
                }
                get
                {
                    return m_page;
                }
            }

            protected bool InInit { get { return m_fInInit; } }

            public void Initialize ()
            {
                m_fInInit = true;
                m_fDirty = false;

                object[] objs = m_page.Objects;

                Debug.Assert (objs.Length > 0);

                object cfgObject = GetCfgObjectFromObject (objs[0]);
                object val1 = GetValueFromCfgObject (cfgObject);

                InitializeFromValue (val1);
                for (int iCfg = 1; iCfg < objs.Length; iCfg++)
                {
                    cfgObject = GetCfgObjectFromObject (objs[iCfg]);

                    object val2 = GetValueFromCfgObject (cfgObject);

                    if(!val1.Equals(val2))
                    {
                        SetIndeterminate ();
                    }
                }

                m_fInInit = false;
            }

            public void Apply ()
            {
                if (m_fDirty)
                {
                    object[] objs = m_page.Objects;

                    for (int iCfg = 0; iCfg < objs.Length; iCfg++)
                    {
                        object cfgObject = GetCfgObjectFromObject (objs[iCfg]);
                        object val = GetValueFromCfgObject (cfgObject);

                        SaveToValue (val);
                    }

                    m_fDirty = false;
                }
            }

            public abstract object GetCfgObjectFromObject (object o);

            public abstract void InitializeFromValue (object val);

            public abstract void SaveToValue (object val);

            public abstract void SetIndeterminate ();

            public abstract object GetValueFromCfgObject (object cfgObject);

            public void Dirty ()
            {
                if (!InInit)
                {
                    m_fDirty = true;
                    m_page.Dirty ();
                }
            }

            public void DirtyEventHandler (object sender, EventArgs e)
            {                
                Dirty ();
            }
        }
#endregion

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        
        private ArrayList m_properties;
              
        public PropertyPage()
        {
            // Required for Windows Form Designer support
            InitializeComponent();
            
            // Need to add any constructor code after InitializeComponent call
            m_properties = new ArrayList ();
        
            this.Size = new System.Drawing.Size (600, 400);
        }

        public object[] Objects { get { return m_Objects; } }

        public override void SetObjects(object[] objects)
        {
            base.SetObjects(objects);
        }

        protected virtual void InitializeProperties () 
        {
            foreach (Property prop in m_properties)
            {
                prop.Initialize ();
            }
        }

        protected override void PreInitPage()
        {
            InitializeProperties();
            base.PreInitPage();
        }    

        protected virtual void ApplyProperties () 
        {
            foreach (Property prop in m_properties)
            {
                prop.Apply ();
            }
            DTEProject.Save();
        }
        
        protected void AddProperty (Property prop)
        {
            prop.Page = this;
            m_properties.Add (prop);
        }

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
            this.ClientSize = new System.Drawing.Size (592, 366);
            this.Name = "PropertyPage";
        }
        #endregion

        #region IPropertyPage Members        

        protected override void ApplyPageChanges()
        {
            ApplyProperties();
            base.ApplyPageChanges();
        }

        protected override bool IsPageDirty()
        {
            return base.IsPageDirty() || m_IsDirty;
        }

        #endregion

        public void Dirty()
        {
            this.IsDirty = true;
        }

        protected void DirtyEventHandler (object sender, EventArgs args)
        {
            Dirty ();
        }
    }   
}
