using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.SPOT.WiX
{
    enum ShortcutShowType { normal, minimized, maximized };

    public class Shortcut : WiXElement
    {
        static int shortcutCount = 0;

        public Shortcut(Component component, Directory target, string name, DirectoryRef directory)
            :this(component, (WiXElement)target, name, directory)
        {
        }

        public Shortcut(Component component, File target, string name, DirectoryRef directory)
            : this(component, (WiXElement)target, name, directory)
        {
        }

        private Shortcut(Component component, WiXElement target, string name, DirectoryRef directory)
            : this(component, name, directory)
        {
            Target = string.Format("[{0}]", target.Id);
        }

        public Shortcut(CreateFolder createFolder, string name, DirectoryRef directory)
            : this((WiXElement)createFolder, name, directory)
        {
        }

        public Shortcut(File file, string name, DirectoryRef directory)
            : this((WiXElement)file, name, directory)
        {     
        }

        private Shortcut(WiXElement parent, string name, DirectoryRef directory)
            : base(parent.Element, "Shortcut", "shortcut" + shortcutCount++)
        {
            Directory = directory.Id;
            Name = name;
        }

        public string Target
        {
            get { return base.GetAttribute("Target"); }
            set { base.SetOrClearAttribute("Target", value); }
        }

        public string Name
        {
            get { return base.GetAttribute("Name"); }
            set { base.SetOrClearAttribute("Name", value); }
        }

        public string Directory
        {
            get { return base.GetAttribute("Directory"); }
            set { base.SetOrClearAttribute("Directory", value); }
        }

        public string WorkingDirectory
        {
            get { return base.GetAttribute("WorkingDirectory"); }
            set { base.SetOrClearAttribute("WorkingDirectory", value); }
        }

        public string Arguments
        {
            get { return base.GetAttribute("Arguments"); }
            set { base.SetOrClearAttribute("Arguments", value); }
        }
    }
}
