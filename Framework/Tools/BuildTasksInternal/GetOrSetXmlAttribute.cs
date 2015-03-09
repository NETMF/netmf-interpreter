using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;

using Task = Microsoft.Build.Utilities.Task;

namespace Microsoft.SPOT.Tasks.Internal
{
    public class GetOrSetXmlAttribute : Task
    {
        string xmlFile;
        bool saveFile = true;
        string xpath;
        string newValue;
        string[] values = null;
        string outerXml;

        public override bool Execute()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlFile);

                List<string> valueList = new List<string>();
                foreach (XmlNode attributeNode in doc.SelectNodes(xpath))
                {
                    if (attributeNode is XmlAttribute)
                    {
                        XmlAttribute attribute = (XmlAttribute)attributeNode;

                        if (!string.IsNullOrEmpty(newValue))
                        {
                            attribute.Value = newValue;
                        }

                        valueList.Add(attribute.Value);
                    }
                    else
                    {
                        throw new ArgumentException("Xpath expression '" + xpath + "' selected non attribute node");
                    }
                }

                if (valueList.Count > 0)
                {
                    values = valueList.ToArray();
                }

                if (saveFile)
                {
                    doc.Save(xmlFile);
                }
                outerXml = doc.OuterXml;
                return true;
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
                return false;
            }
        }

        [Required]
        public string XmlFile
        {
            set { xmlFile = value; }
        }

        [Required]
        public string XPathExpression
        {
            set { xpath = value; }
        }

        public bool SaveFile
        {
            set { saveFile = value; }
        }

        public string NewValue
        {
            set { newValue = value; }
        }

        [Output]
        public string[] Values
        {
            get { return values; }
        }

        [Output]
        public string OuterXml
        {
            get { return outerXml; }
        }
    }
}
