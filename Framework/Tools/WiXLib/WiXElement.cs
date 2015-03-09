using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Microsoft.SPOT.WiX
{
    public abstract class WiXElement
    {
        protected XmlElement thisElement;

        internal WiXElement(XmlDocument doc, string name)
        {
            this.thisElement = CreateWiXElement(doc, name);
        }

        internal WiXElement(XmlDocument doc, string name, string Id)
        {
            this.thisElement = CreateWiXElement(doc, name, Id);
        }

        internal WiXElement(XmlElement parent, string name)
        {
            thisElement = CreateWiXElement(parent.OwnerDocument, name);
            parent.AppendChild(thisElement);
        }

        internal WiXElement(XmlElement parent, string name, string Id)
        {
            thisElement = CreateWiXElement(parent.OwnerDocument, name, Id);
            parent.AppendChild(thisElement);
        }

        internal WiXElement(XmlElement element)
        {
            thisElement = element;
            thisElement.SetAttribute("xmlns", "http://schemas.microsoft.com/wix/2006/wi");
        }

        internal void AppendAttribute(string name, string value)
        {
            AppendAttribute(thisElement, name, value);
        }

        internal void SetOrClearAttribute(string name, string value)
        {
            if (value == null || value == "")
            {
                if (thisElement.Attributes[name] != null)
                {
                    thisElement.Attributes.Remove(
                        thisElement.Attributes[name]);
                }
            }
            else
            {
                if (thisElement.Attributes[name] == null)
                {
                    AppendAttribute(thisElement, name, value);
                }
                else
                {
                    thisElement.Attributes[name].Value = value;
                }
            }
        }

        internal string GetAttribute(string name)
        {
            if (thisElement.Attributes[name] == null) return "";

            return thisElement.Attributes[name].Value;
        }

        public XmlElement Element
        {
            get { return thisElement; }
        }

        public string Id
        {
            get { return thisElement.Attributes["Id"].Value; }
            set { thisElement.Attributes["Id"].Value = value; }
        }

        internal static XmlElement CreateWiXElement(XmlDocument doc, string name)
        {
            XmlElement thisElement = doc.CreateElement(name, "http://schemas.microsoft.com/wix/2006/wi");
            return thisElement;
        }

        internal static XmlElement CreateWiXElement(XmlDocument doc, string name, string Id)
        {
            XmlElement thisElement = doc.CreateElement(name, "http://schemas.microsoft.com/wix/2006/wi");
            thisElement.SetAttribute("Id", Id);
            return thisElement;
        }

        internal static void AppendAttribute(XmlElement element, string name, string value)
        {
            element.SetAttribute(name, value);
        }
    }
}
