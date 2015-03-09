using System;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace Microsoft.SPOT.WiX
{
    // FOR MSI COMPLIANCE YOU NEED TO ADD A "RemoveFile" element for every directory/file added by the install.
    // TODO: make sure all files/dirs have an associated removefile
    public class RemoveFile : WiXElement
    {
        private void Init(string name)
        {
            WiXElement.AppendAttribute(
                thisElement,
                "Name",
                Path.GetFileName(name));

            WiXElement.AppendAttribute(
                thisElement,
                "Id",
                "Remove" + Guid.NewGuid().ToString("b").Trim('{', '}').Replace("-",""));

            WiXElement.AppendAttribute(
                thisElement,
                "On",
                "uninstall");
        }

        public RemoveFile(XmlElement parent, string name)
            : base(parent, "RemoveFile")
        {
            Init(name);
        }

        public RemoveFile(XmlDocument doc, string name)
            : base(doc, "RemoveFile")
        {
            Init(name);
        }
    }

    public class File : WiXElement
    {
        public enum AssemblyType { NET, NO, WIN32 };
        static int fileCount = 0;

        public File(Component parent, string source, bool numberedID)
            : this(parent, null, source, numberedID)
        {
        }

        public File(Component parent, string source)
            : this(parent, null, source, false)
        {
        }

        public File(Component parent, string name, string source)
            : this(parent, name, source, false)
        {
        }

        public File(Component parent, string name, string source, bool numberedID)
            : base(parent.Element, "File", "REPLACE_ID")
        {
            if (string.IsNullOrEmpty(name))
            {
                name = source;
            }

            string filePart = Path.GetFileNameWithoutExtension(name);

            string ext = Path.GetExtension(name);

            if(ext.Length > 0) ext = ext.Substring(1);

            thisElement.Attributes["Id"].Value =
                    numberedID
                    ? "file" + fileCount++
                    : IdFromNameAndComponent(source, parent);

            WiXElement.AppendAttribute(
                thisElement,
                "Name",
                Path.GetFileName(name));

            WiXElement.AppendAttribute(
                thisElement, "Source", source);

            WiXElement.AppendAttribute(
                thisElement, "DiskId", "1");


            /* FIX THIS TO ADD REMOVEFILE FOR EACH FILE */
            if (parent.Element.OwnerDocument != null)
            {
                new RemoveFile(parent.Element, name);
            }
        }

        private static string IdFromNameAndComponent(string name, Component component)
        {
            string prefix = Environment.GetEnvironmentVariable(@"SPOCLIENT");
            if ( !String.IsNullOrEmpty(prefix) )
            {
                prefix += @"\";
                if ( name.StartsWith(prefix) )
                    name = name.Substring(prefix.Length);
            }
            return "id" + ((UInt32)(component.Guid.GetHashCode())).ToString() + ((UInt32)name.GetHashCode()).ToString();
        }

        public string Name
        {
            get { return thisElement.Attributes["Name"].Value; }
            set { thisElement.Attributes["Name"].Value = value; }
        }

        public string Source
        {
            get { return thisElement.Attributes["Source"].Value; }
        }

        public string DiskId
        {
            get { return thisElement.Attributes["DiskId"].Value; }
            set { thisElement.Attributes["DiskId"].Value = value; }
        }

        public AssemblyType Assembly
        {
            get
            {
                XmlAttribute attribute = thisElement.Attributes["Assembly"];

                string type = null;
                if (attribute != null)
                {
                    type = attribute.Value;
                }

                if (string.IsNullOrEmpty(type) || type == "no")
                {
                    return AssemblyType.NO;
                }

                if (type == ".net")
                {
                    return AssemblyType.NET;
                }

                if (type == "win32")
                {
                    return AssemblyType.WIN32;
                }

                // Invalid type.  Can't really happen.
                thisElement.Attributes.Remove(attribute);
                return AssemblyType.NO;
            }

            set
            {
                XmlAttribute attribute = thisElement.Attributes["Assembly"];

                if (attribute == null)
                {
                    attribute = thisElement.OwnerDocument.CreateAttribute("Assembly");
                    thisElement.Attributes.Append(attribute);
                }

                switch (value)
                {
                    case AssemblyType.NET:
                        attribute.Value = ".net";
                        break;
                    case AssemblyType.NO:
                        attribute.Value = "no";
                        break;
                    case AssemblyType.WIN32:
                        attribute.Value = "win32";
                        break;
                }
            }
        }

        public void CopyFile(string destinationDirectory)
        {
            new CopyFile(this, destinationDirectory);
        }

        public void CopyFile(string destinationDirectory, string destinationName)
        {
            new CopyFile(this, destinationDirectory, destinationName);
        }
    }

    public class CopyFile : WiXElement
    {
        static int copyIdCount = 0;
        public CopyFile(File file, string destinationDirectory)
            :
            this(file.Element.OwnerDocument, "CopyFile_" + file.Id + "_" + copyIdCount++)
        {
            AppendAttribute("DestinationDirectory", destinationDirectory);
            file.Element.AppendChild(thisElement);
        }

        public CopyFile(File file, string destinationDirectory, string destinationName)
            : this(file, destinationDirectory)
        {
            AppendAttribute("DestinationName", destinationName);
        }

        private CopyFile(XmlDocument doc, string id)
            : base(doc, "CopyFile", id)
        {
        }
    }
}
