using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Microsoft.SPOT.WiX
{
    public class DirectoryRef : WiXElement
    {
        public DirectoryRef(Fragment parent, string Id)
            : base(parent.Element, "DirectoryRef", Id)
        {
        }

        public DirectoryRef(Fragment parent, Directory source)
            : base(parent.Element, "DirectoryRef", source.Id)
        {
        }

        protected DirectoryRef(XmlElement parent, string name, string Id)
            : base(parent, name, Id)
        {
        }

        protected DirectoryRef(XmlElement element)
            : base(element)
        {
        }
    }

    public class Directory : DirectoryRef
    {
        static int directoryCount = 0;

        public Directory(Directory parent, string name)
            : this(parent.Element, name)
        {
        }

        public Directory(Directory parent, string name, string Id)
            : this(parent.Element, name, Id)
        {
        }

        public Directory(DirectoryRef parent, string name)
            : this(parent.Element, name)
        {
        }

        public Directory(DirectoryRef parent, string name, string Id)
            : this(parent.Element, name, Id)
        {
        }

        public Directory(Fragment parent, string name)
            : this(parent.Element, name)
        {
        }

        public Directory(Fragment parent, string name, string Id)
            : this(parent.Element, name, Id)
        {
        }

        private Directory(XmlElement parent, string name)
            : this(parent, name, "directory" + directoryCount++)
        {
        }

        private Directory(XmlElement parent, string name, string Id)
            : base(parent, "Directory", Id)
        {
            thisElement.SetAttribute("Name", name);
        }

        internal Directory(XmlElement element)
            : base(element)
        {
        }

        public Directory GetDirectoryFromId(string Id)
        {
            return GetDirectory("Id", Id);
        }

        public Directory GetDirectoryFromName(string Name)
        {
            return GetDirectory("Name", Name);
        }

        private Directory GetDirectory(string name, string value)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(thisElement.OwnerDocument.NameTable);
            nsmgr.AddNamespace(
                "ns",
                thisElement.OwnerDocument.DocumentElement.Attributes["xmlns"].Value);

            XmlElement childDirectory = thisElement.SelectSingleNode(
                string.Format("/descendant::ns:Directory[@{0}='{1}']", name, value), nsmgr) as XmlElement;

            if(childDirectory == null) return null;

            return new Directory(childDirectory);
        }
    }
}
