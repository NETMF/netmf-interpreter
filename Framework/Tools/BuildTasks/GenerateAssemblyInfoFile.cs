using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Text.RegularExpressions;

namespace Microsoft.SPOT.Tasks
{
    public sealed class GenerateAssemblyInfoFile : Task
    {
        enum OutputLanguages
        {
            CSharp,
            VisualBasic,
            RCFile,
        }
        
        class AttributeSubstitution
        {
            Regex m_regex;
            String m_replacement;
            
            public AttributeSubstitution(string pattern, string replacement)
            {
                m_regex = new Regex(pattern);
                m_replacement = replacement;
            }
            
            public Regex Regex
            {
                set { m_regex = value; }
                get { return m_regex; }
            }
            public String Replacement
            {
                set { m_replacement = value; }
                get { return m_replacement; }
            }
        }
        
        #region Properties

        private OutputLanguages m_outputLanguage;
        
        private ITaskItem m_outputFile = null;

        private Dictionary<string, AttributeSubstitution> m_attributesDict = null;
        
        [Required]
        public ITaskItem OutputFile
        {
            set { m_outputFile = value; }
            get { return m_outputFile; }
        }
        
        public string Language
        {
            set
            {
                if ( value == "C#" || value == "CSharp")
                {
                    m_outputLanguage = OutputLanguages.CSharp;
                }
                else if ( value == "Visual Basic" || value == "VisualBasic" )
                {
                    m_outputLanguage = OutputLanguages.VisualBasic;
                }
                else if (value == "RC")
                {
                    m_outputLanguage = OutputLanguages.RCFile;
                }
                else
                {
                    throw new Exception(String.Format("\"{0}\" is not a language supported by this GenerateAssemblyInfoFile task. Allowed languages are \"C#\" and \"VisualBasic\".", value));
                }                    
            }
        }

        private void AddOrReplaceProperty(string name, string pattern, string replacement)
        {
            if (m_attributesDict.ContainsKey(name))
            {
                m_attributesDict.Remove(name);
            }
            m_attributesDict.Add(name, new AttributeSubstitution(pattern, replacement));
        }
        
        public string Title
        {
            set
            {
                this.AddOrReplaceProperty("TITLE", "%TITLE%", value);
            }
        }
        
        public string Company
        {
            set
            {
                this.AddOrReplaceProperty("COMPANY", "%COMPANY%", value);
            }
        }

        public string Copyright
        {
            set
            {
                this.AddOrReplaceProperty("COPYRIGHT", "%COPYRIGHT%", value);
            }
        }

        public string Description
        {
            set
            {
                this.AddOrReplaceProperty("DESCRIPTION", "%DESCRIPTION%", value);
            }
        }

        public string AssemblyVersion
        {
            set
            {
                this.AddOrReplaceProperty("ASSEMBLYVERSION", "%ASSEMBLYVERSION%", value);
            }
        }

        public string ProductVersion
        {
            set
            {
                this.AddOrReplaceProperty("PRODUCTVERSION", "%PRODUCTVERSION%", value);
            }
        }

        public string ProductVersionCSV
        {
            set
            {
                this.AddOrReplaceProperty("PRODUCTVERSION_CSV", "%PRODUCTVERSION_CSV%", value);
            }
        }

        public string FileVersion
        {
            set
            {
                this.AddOrReplaceProperty("FILEVERSION", "%FILEVERSION%", value);
            }
        }

        public string FileVersionCSV
        {
            set
            {
                this.AddOrReplaceProperty("FILEVERSION_CSV", "%FILEVERSION_CSV%", value);
            }
        }

        public string Configuration
        {
            set
            {
                this.AddOrReplaceProperty("CONFIGURATION", "%CONFIGURATION%", value);
            }
        }

        public string Product
        {
            set
            {
                this.AddOrReplaceProperty("PRODUCT", "%PRODUCT%", value);
            }
        }

        public string Trademark
        {
            set
            {
                this.AddOrReplaceProperty("TRADEMARK", "%TRADEMARK%", value);
            }
        }

        public string InternalName
        {
            set
            {
                this.AddOrReplaceProperty("INTERNALNAME", "%INTERNALNAME%", value);
            }
        }

        #endregion

        private void InitAttribute(string attribName)
        {
            m_attributesDict.Add(attribName, new AttributeSubstitution("%" + attribName + "%", @""));
        }
        
        public GenerateAssemblyInfoFile()
        {
            m_outputLanguage = OutputLanguages.CSharp;
            
            m_attributesDict = new Dictionary<string,AttributeSubstitution>();
            
            // Must initialize all properties, because msbuild will treat setting one of these from the targets file to "" as a no-op
            this.InitAttribute(@"TITLE");
            this.InitAttribute(@"DESCRIPTION");
            this.InitAttribute(@"CONFIGURATION");
            this.InitAttribute(@"COMPANY");
            this.InitAttribute(@"PRODUCT");
            this.InitAttribute(@"COPYRIGHT");
            this.InitAttribute(@"TRADEMARK");
            this.InitAttribute(@"FILEVERSION");
            this.InitAttribute(@"FILEVERSION_CSV");
            this.InitAttribute(@"ASSEMBLYVERSION");
            this.InitAttribute(@"PRODUCTVERSION");
            this.InitAttribute(@"PRODUCTVERSION_CSV");
            this.InitAttribute(@"INTERNALNAME");
        }

        #region ITask Members

        private string TemplateString
        {
            get
            {
                switch(m_outputLanguage)
                {
                    case OutputLanguages.CSharp:
                        return BuildTaskResources.CsAssemblyInfoTemplate;
                        
                    case OutputLanguages.VisualBasic:
                        return BuildTaskResources.VisualBasicAssemblyInfoTemplate;

                    case OutputLanguages.RCFile:
                        return BuildTaskResources.RCAssemblyInfoTemplate;
                    default:
                        throw new Exception(String.Format("Internal error: unimplemented output language template for {0}", m_outputLanguage.ToString()) );
                }
             }
        }
        
        public override bool Execute()
        {
            try
            {
                using(StreamWriter sw = new StreamWriter( m_outputFile.ItemSpec ))
                {
                    using (StringReader sr = new StringReader(this.TemplateString))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            foreach (AttributeSubstitution subst in m_attributesDict.Values)
                            {
                                line = subst.Regex.Replace(line, subst.Replacement);
                            }
                            sw.WriteLine(line);
                        }
                    }
                }
            }
            catch (Exception e)
            {
			    Log.LogError("Error trying to create assemblyInfo file at {0}: ({1})", m_outputFile.ItemSpec, e.Message);
                return false;
            }
            return true;
        }

        #endregion
    }
}
