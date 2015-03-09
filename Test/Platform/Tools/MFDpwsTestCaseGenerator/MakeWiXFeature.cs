using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Evaluation;
using System.IO;

namespace MFDpwsTestCaseGenerator
{
    public class MakeWiXFeature : Task
    {
        public override bool Execute()
        {
            try
            {
                XmlDocument doc = new XmlDocument();

                doc.LoadXml(Resources.MergeFragment);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace(
                    "ns",
                    doc.DocumentElement.Attributes["xmlns"].Value);

                XmlNode featureNode = doc.SelectSingleNode("/descendant::ns:Wix/ns:Module", nsmgr);

                foreach (string componentid in FeatureComponents)
                {
                    var componentref = doc.CreateElement("ComponentRef", "http://schemas.microsoft.com/wix/2006/wi");
                    var id = doc.CreateAttribute("Id");
                    id.Value = componentid;
                    componentref.Attributes.Append(id);
                    featureNode.AppendChild(componentref);
                    Log.LogMessage("Adding {0} to feature", componentid); 
                }

                doc.Save(FeatureFileName);

                Project wixproj = new Project(XmlTextReader.Create(new StringReader(Resources.WixProjTemplate)));
                wixproj.AddItem("WiXSource", FeatureFileName);
                wixproj.Save(
                    Path.GetDirectoryName(FeatureFileName) + "\\" + Path.GetFileNameWithoutExtension(FeatureFileName) + ".wixproj");

                return true;
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
                return false;
            }
        }

        [Required]
        public string[] FeatureComponents
        { get; set; }

        [Required]
        public string FeatureFileName
        { get; set; }

    }
}
