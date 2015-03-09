using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Microsoft.SPOT.WiX
{
    public class Component : WiXElement
    {
        static int componentCount = 0;
        private Guid _guid;
        
        public Component(Directory parent)
            : this(parent, "component" + componentCount++, Guid.NewGuid())
        {
        }

        public Component(DirectoryRef parent)
            : this(parent, "component" + componentCount++, Guid.NewGuid())
        {
        }

        public Component(Directory parent, string Id, Guid guid) :
            this(parent.Element, Id, guid)
        {
        }

        public Component(DirectoryRef parent, string Id, Guid guid)
            :
            this(parent.Element, Id, guid)
        {
        }

        private Component(XmlElement parent, string Id, Guid guid)
            : base(parent, "Component", Id)
        {
            _guid = guid;
            AppendAttribute(
                "Guid",
                guid.ToString());
        }

        public void AppendInclude(string includeFile)
        {
            thisElement.AppendChild(
                thisElement.OwnerDocument.CreateProcessingInstruction(
                    "include", includeFile));
        }

        public void CreateFolder(string directoryId)
        {
            new CreateFolder(this, directoryId);
        }

        public void CreateFolder(Directory directory)
        {
            new CreateFolder(this, directory);
        }

        public Guid Guid
        {
            get { return _guid; }
        }
    }

    public class ComponentGroup : WiXElement
    {
        public ComponentGroup(Fragment parent, string Id) :
            base(parent.Element, "ComponentGroup", Id)
        {
        }

        public void AppendComponent(Component component)
        {
            new ComponentRef(this, component);
        }
    }

    public class ComponentRef : WiXElement
    {
        private Component component;

        public ComponentRef(ComponentGroup parent, Component component)
            : base(parent.Element, "ComponentRef", component.Id)
        {
            this.component = component;
        }

        public Component Component
        {
            get { return component; }
        }
    }

    public class CreateFolder : WiXElement
    {
        public CreateFolder(Component component, Directory directory)
            : this(component, directory.Id)
        {
        }

        public CreateFolder(Component component, string directoryId)
            :
            base(component.Element.OwnerDocument, "CreateFolder")
        {
            this.AppendAttribute("Directory", directoryId);
            component.Element.AppendChild(thisElement);
        }
    }
}
