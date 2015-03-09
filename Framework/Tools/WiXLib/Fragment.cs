using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Microsoft.SPOT.WiX
{
    public class Fragment : WiXElement
    {
        public Fragment(string Id) : base(new XmlDocument(), "Fragment", Id)
        {
            thisElement.OwnerDocument.CreateXmlDeclaration("1.0", "utf-8", null);
            thisElement.OwnerDocument.AppendChild(thisElement.OwnerDocument.CreateElement("Wix", "http://schemas.microsoft.com/wix/2006/wi"));
            thisElement.OwnerDocument.DocumentElement.SetAttribute("xmlns", "http://schemas.microsoft.com/wix/2006/wi");

            thisElement.OwnerDocument.DocumentElement.AppendChild(thisElement);
        }

        public void AppendInclude(string includeFile)
        {
            thisElement.AppendChild(
                thisElement.OwnerDocument.CreateProcessingInstruction(
                    "include", includeFile));
        }

        public void PrependInclude(string includeFile)
        {
            thisElement.PrependChild(
                thisElement.OwnerDocument.CreateProcessingInstruction(
                    "include", includeFile));
        }

        public void PrependDefine(string definition)
        {
            thisElement.PrependChild(
                thisElement.OwnerDocument.CreateProcessingInstruction(
                    "define", definition));
        }
    }
}
