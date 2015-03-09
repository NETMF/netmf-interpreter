////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Globalization;

namespace CLRProfiler
{
    internal enum InterestLevel
    {
        Ignore              = 0,
        Display             = 1<<0,
        Interesting         = 1<<1,
        Parents             = 1<<2,
        Children            = 1<<3,
        InterestingParents  = Interesting | Parents,
        InterestingChildren = Interesting | Children,
        ParentsChildren     = Parents | Children,
    }

    /// <summary>
    /// Summary description for FilterForm.
    /// </summary>
    public class FilterForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox typeFilterTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox parentsCheckBox;
        private System.Windows.Forms.CheckBox childrenCheckBox;
        private System.Windows.Forms.CheckBox caseInsensitiveCheckBox;
        private System.Windows.Forms.CheckBox onlyFinalizableTypesCheckBox;
        internal System.Windows.Forms.TextBox methodFilterTextBox;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private string[] typeFilters = new string[0];
        internal string[] methodFilters = new string[0];
        internal string[] signatureFilters = new string[0];
        internal ulong[] addressFilters = new ulong[0];
        private bool showChildren = true;
        private bool showParents = true;
        private bool caseInsensitive = true;
        private bool onlyFinalizableTypes = false;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox signatureFilterTextBox;
        internal int filterVersion;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox addressFilterTextBox;
        private static int versionCounter;

        internal InterestLevel InterestLevelOfName(string name, string[] typeFilters)
        {
            if (name == "<root>" || typeFilters.Length == 0)
                return InterestLevel.Interesting;
            string bestFilter = "";
            InterestLevel bestLevel = InterestLevel.Ignore;
            foreach (string filter in typeFilters)
            {
                InterestLevel level = InterestLevel.Interesting;
                string realFilter = filter.Trim();
                if (realFilter.Length > 0 && (realFilter[0] == '~' || realFilter[0] == '!'))
                {
                    level = InterestLevel.Ignore;
                    realFilter = realFilter.Substring(1).Trim();
                }
                else
                {
                    if (showParents)
                        level |= InterestLevel.Parents;
                    if (showChildren)
                        level |= InterestLevel.Children;
                }

                // Check if the filter is a prefix of the name
                if (string.Compare(name, 0, realFilter, 0, realFilter.Length, caseInsensitive, CultureInfo.InvariantCulture) == 0)
                {
                    // This filter matches the type name
                    // Let's see if it's the most specific (i.e. LONGEST) one so far.
                    if (realFilter.Length > bestFilter.Length)
                    {
                        bestFilter = realFilter;
                        bestLevel = level;
                    }
                }
            }
            return bestLevel;
        }

        internal InterestLevel InterestLevelOfAddress(ulong thisAddress)
        {
            if (thisAddress == 0 || addressFilters.Length == 0)
                return InterestLevel.Interesting;
            foreach (ulong address in addressFilters)
            {
                InterestLevel level = InterestLevel.Interesting;
                if (showParents)
                    level |= InterestLevel.Parents;
                if (showChildren)
                    level |= InterestLevel.Children;

                if (address == thisAddress)
                    return level;
            }
            return InterestLevel.Ignore;
        }

        private InterestLevel InterestLevelOfSignature(string signature, string[] signatureFilters)
        {
            if (signature != null && signature != "" && signatureFilters.Length != 0)
                return InterestLevelOfName(signature, signatureFilters);
            else
                return InterestLevel.Interesting | InterestLevel.Parents | InterestLevel.Children;
        }

        internal InterestLevel InterestLevelOfTypeName(string typeName, string signature, bool typeIsFinalizable)
        {
            if (onlyFinalizableTypes && !typeIsFinalizable && typeName != "<root>")
                return InterestLevel.Ignore;
            else
            {
                return InterestLevelOfName(typeName, typeFilters) &
                       InterestLevelOfSignature(signature, signatureFilters);
            }
        }

        internal InterestLevel InterestLevelOfMethodName(string methodName, string signature)
        {
            return InterestLevelOfName(methodName, methodFilters) &
                   InterestLevelOfSignature(signature, signatureFilters);
        }

        public FilterForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
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
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.typeFilterTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.methodFilterTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.parentsCheckBox = new System.Windows.Forms.CheckBox();
            this.childrenCheckBox = new System.Windows.Forms.CheckBox();
            this.caseInsensitiveCheckBox = new System.Windows.Forms.CheckBox();
            this.onlyFinalizableTypesCheckBox = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.signatureFilterTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.addressFilterTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(488, 24);
            this.okButton.Name = "okButton";
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(488, 64);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            // 
            // typeFilterTextBox
            // 
            this.typeFilterTextBox.Location = new System.Drawing.Point(48, 48);
            this.typeFilterTextBox.Name = "typeFilterTextBox";
            this.typeFilterTextBox.Size = new System.Drawing.Size(392, 20);
            this.typeFilterTextBox.TabIndex = 2;
            this.typeFilterTextBox.Text = "";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(40, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(368, 23);
            this.label1.TabIndex = 3;
            this.label1.Text = "Show Types starting with (separate multiple entries with \";\"):";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(40, 96);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(328, 16);
            this.label2.TabIndex = 4;
            this.label2.Text = "Show Methods starting with (separate multiple entries with \";\"):";
            // 
            // methodFilterTextBox
            // 
            this.methodFilterTextBox.Location = new System.Drawing.Point(48, 120);
            this.methodFilterTextBox.Name = "methodFilterTextBox";
            this.methodFilterTextBox.Size = new System.Drawing.Size(392, 20);
            this.methodFilterTextBox.TabIndex = 5;
            this.methodFilterTextBox.Text = "";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(40, 312);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 16);
            this.label3.TabIndex = 6;
            this.label3.Text = "Options:";
            // 
            // parentsCheckBox
            // 
            this.parentsCheckBox.Checked = true;
            this.parentsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.parentsCheckBox.Location = new System.Drawing.Point(104, 312);
            this.parentsCheckBox.Name = "parentsCheckBox";
            this.parentsCheckBox.Size = new System.Drawing.Size(208, 16);
            this.parentsCheckBox.TabIndex = 7;
            this.parentsCheckBox.Text = "Show Callers/Referencing Objects";
            // 
            // childrenCheckBox
            // 
            this.childrenCheckBox.Checked = true;
            this.childrenCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.childrenCheckBox.Location = new System.Drawing.Point(104, 344);
            this.childrenCheckBox.Name = "childrenCheckBox";
            this.childrenCheckBox.Size = new System.Drawing.Size(216, 24);
            this.childrenCheckBox.TabIndex = 8;
            this.childrenCheckBox.Text = "Show Callees/Referenced Objects";
            // 
            // caseInsensitiveCheckBox
            // 
            this.caseInsensitiveCheckBox.Checked = true;
            this.caseInsensitiveCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.caseInsensitiveCheckBox.Location = new System.Drawing.Point(336, 344);
            this.caseInsensitiveCheckBox.Name = "caseInsensitiveCheckBox";
            this.caseInsensitiveCheckBox.Size = new System.Drawing.Size(168, 24);
            this.caseInsensitiveCheckBox.TabIndex = 9;
            this.caseInsensitiveCheckBox.Text = "Ignore Case";
            // 
            // onlyFinalizableTypesCheckBox
            // 
            this.onlyFinalizableTypesCheckBox.Location = new System.Drawing.Point(336, 312);
            this.onlyFinalizableTypesCheckBox.Name = "onlyFinalizableTypesCheckBox";
            this.onlyFinalizableTypesCheckBox.Size = new System.Drawing.Size(168, 16);
            this.onlyFinalizableTypesCheckBox.TabIndex = 10;
            this.onlyFinalizableTypesCheckBox.Text = "Show only finalizable Types";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(40, 168);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(328, 16);
            this.label4.TabIndex = 11;
            this.label4.Text = "Signatures (separate multiple entries with \";\"):";
            // 
            // signatureFilterTextBox
            // 
            this.signatureFilterTextBox.Location = new System.Drawing.Point(48, 192);
            this.signatureFilterTextBox.Name = "signatureFilterTextBox";
            this.signatureFilterTextBox.Size = new System.Drawing.Size(392, 20);
            this.signatureFilterTextBox.TabIndex = 12;
            this.signatureFilterTextBox.Text = "";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(40, 240);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(328, 16);
            this.label5.TabIndex = 13;
            this.label5.Text = "Object addresses (separate multiple entries with \";\"):";
            // 
            // addressFilterTextBox
            // 
            this.addressFilterTextBox.Location = new System.Drawing.Point(48, 264);
            this.addressFilterTextBox.Name = "addressFilterTextBox";
            this.addressFilterTextBox.Size = new System.Drawing.Size(392, 20);
            this.addressFilterTextBox.TabIndex = 14;
            this.addressFilterTextBox.Text = "";
            // 
            // FilterForm
            // 
            this.AcceptButton = this.okButton;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(584, 390);
            this.Controls.Add(this.addressFilterTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.signatureFilterTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.onlyFinalizableTypesCheckBox);
            this.Controls.Add(this.caseInsensitiveCheckBox);
            this.Controls.Add(this.childrenCheckBox);
            this.Controls.Add(this.parentsCheckBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.methodFilterTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.typeFilterTextBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Name = "FilterForm";
            this.Text = "Set Filters for Types and Methods";
            this.ResumeLayout(false);

        }
        #endregion

        internal void SetFilterForm(string typeFilter, string methodFilter, string signatureFilter, string addressFilter,
            bool showAncestors, bool showDescendants, bool caseInsensitive, bool onlyFinalizableTypes)
        {
            typeFilter = typeFilter.Trim();
            if (typeFilter == "")
                typeFilters = new string[0];
            else
                typeFilters = typeFilter.Split(';');
            
            methodFilter = methodFilter.Trim();
            if (methodFilter == "")
                methodFilters = new string[0];
            else
                methodFilters = methodFilter.Split(';');
            
            signatureFilter = signatureFilter.Trim();
            if (signatureFilter == "")
                signatureFilters = new string[0];
            else
                signatureFilters = signatureFilter.Split(';');

            addressFilter = addressFilter.Trim();
            if (addressFilter == "")
                addressFilters = new ulong[0];
            else
            {
                string[] addressFilterStrings = addressFilter.Split(';');
                addressFilters = new ulong[addressFilterStrings.Length];
                for (int i = 0; i < addressFilterStrings.Length; i++)
                {
                    string thisAddressFilter = addressFilterStrings[i].Replace(".", "");
                    if (thisAddressFilter != "")
                    {
                        if (thisAddressFilter.StartsWith("0x") || thisAddressFilter.StartsWith("0X"))
                            addressFilters[i] = ulong.Parse(thisAddressFilter.Substring(2), NumberStyles.HexNumber);
                        else
                            addressFilters[i] = ulong.Parse(thisAddressFilter, NumberStyles.HexNumber);
                    }
                }
            }
            this.showParents = showAncestors;
            this.showChildren = showDescendants;
            this.caseInsensitive = caseInsensitive;
            this.onlyFinalizableTypes = onlyFinalizableTypes;

            this.filterVersion = ++versionCounter;

            typeFilterTextBox.Text = typeFilter;
            methodFilterTextBox.Text = methodFilter;
            signatureFilterTextBox.Text = signatureFilter;
            addressFilterTextBox.Text = addressFilter;

            parentsCheckBox.Checked = showParents;
            childrenCheckBox.Checked = showChildren;
            caseInsensitiveCheckBox.Checked = caseInsensitive;
            onlyFinalizableTypesCheckBox.Checked = onlyFinalizableTypes;

        }

        private void okButton_Click(object sender, System.EventArgs e)
        {
            SetFilterForm(typeFilterTextBox.Text, methodFilterTextBox.Text, signatureFilterTextBox.Text, addressFilterTextBox.Text,
                parentsCheckBox.Checked, childrenCheckBox.Checked, caseInsensitiveCheckBox.Checked, onlyFinalizableTypesCheckBox.Checked);
        }
    }
}
