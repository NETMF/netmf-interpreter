using System;
using System.IO;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.SPOT.WiX;
using System.Xml;
using System.Text.RegularExpressions;

using Task = Microsoft.Build.Utilities.Task;

namespace Microsoft.SPOT.Tasks.Internal
{
    public class CreateAssemblyFragment : Task
    {
        string assemblyName;
        string assemblyEndian;
        string assemblyShortcut;
        Guid componentGuid = Guid.Empty;
        ITaskItem[] componentFiles;
        ITaskItem[] includeFiles;
        ITaskItem[] postIncludeFiles;
        ITaskItem[] componentIncludeFiles;
        string directoryRef;
        string fragmentId = null;
        string componentId = null;
        string fragmentFileName = null;

        public override bool Execute()
        {
            try
            {
                //
                // Do some setup
                //
                this.Log.LogMessage("Create Assembly Fragment Task");

                string assemblyRoot = Path.GetFileNameWithoutExtension(assemblyName);

                if (fragmentId == null)
                {
                    fragmentId = "Fragment" + assemblyEndian + assemblyName;
                }

                componentId = "Component" + assemblyEndian + assemblyName;

                //
                // Build WiX file
                //
                Fragment fragment = new Fragment(fragmentId);

                if (includeFiles != null)
                {
                    foreach (ITaskItem item in includeFiles)
                    {
                        if (item == null) continue;
                        fragment.PrependInclude(item.ItemSpec);
                    }
                }

                DirectoryRef dirref = new DirectoryRef(
                    fragment,
                    directoryRef);


                Component fileComponent = null;

                // Create Component and add files
                if (componentGuid != Guid.Empty)
                {
                    // Generate new GUID for BE files, else BE files will be stranded upon uninstall
                    if ( assemblyEndian == "_be_" )
                    {
                        componentGuid = Guid.NewGuid();
                    }
 
                    fileComponent = new Component(
                        dirref,
                        componentId,
                        componentGuid);

                    fragment.PrependDefine(
                        string.Format("COMPONENTID=\"{0}\"", fileComponent.Id));

                    foreach (ITaskItem item in componentFiles)
                    {
                        if (item == null) continue;
                        string fileName = item.ItemSpec;

                        string assemblyType = item.GetMetadata("AssemblyType");
                        assemblyType = String.IsNullOrEmpty(assemblyType) ? "" : assemblyType.ToLower();

                        if ( assemblyType != "" )
                        {
                            if ( assemblyType != ".net" && assemblyType != "win32" && assemblyType != "no" )
                            {
                                throw new ApplicationException("Invalid assemblyType \"" + assemblyType + "\" in file metadata ");
                            }
                        }

                        Microsoft.SPOT.WiX.File file = new Microsoft.SPOT.WiX.File(
                            fileComponent,
                            item.GetMetadata("Name"),
                            fileName,
                            false);

                        file.Id = file.Id + assemblyEndian;

                        fragment.PrependDefine(
                                string.Format("ID{0}=\"{1}\"", file.Name.Replace('.', '_'), file.Id));

                        if(!string.IsNullOrEmpty(assemblyShortcut) && fileName.ToLower().EndsWith(".exe"))
                        {
                            Shortcut sc = new Shortcut(file, assemblyShortcut, new DirectoryRef(fragment,"ProgramMenuDir"));
                        }
                    }

                    if (componentIncludeFiles != null)
                    {
                        foreach (ITaskItem item in componentIncludeFiles)
                        {
                            if (item == null) continue;
                            fileComponent.AppendInclude(item.ItemSpec);
                        }
                    }
                }

                if (postIncludeFiles != null)
                {
                    foreach (ITaskItem item in postIncludeFiles)
                    {
                        if (item == null) continue;
                        fragment.AppendInclude(item.ItemSpec);
                    }
                }

                // Save Fragment File
                string fragmentFileDirectory = Path.GetDirectoryName(fragmentFileName);

                if (!System.IO.Directory.Exists(fragmentFileDirectory))
                {
                    System.IO.Directory.CreateDirectory(fragmentFileDirectory);
                }

                fragment.Element.OwnerDocument.Save(fragmentFileName);

                return true;
            }
            catch (Exception e)
            {
                this.Log.LogErrorFromException(e);
                return false;
            }
        }

        private string AddExtension(string fileName, string newExtension)
        {
            if (fileName == null || fileName.Length == 0) return null;

            return fileName + newExtension;
        }

        public string ComponentShortcut
        {
            get { return assemblyShortcut; }
            set { assemblyShortcut = value; }
        }

        [Required]
        public string AssemblyName
        {
            get { return assemblyName; }
            set { assemblyName = value; }
        }

        [Required]
        public string ComponentGuid
        {
            set { componentGuid = new Guid(value); }
        }

        [Required]
        public string AssemblyEndian
        {
            get { return assemblyEndian; }
            set { assemblyEndian = value; }
        }

        [Required]
        public string DirectoryRef
        {
            get { return directoryRef; }
            set { directoryRef = value; }
        }

        public string ShortName
        {
            set { Log.LogWarning("ShortName input is deprecated and will be ignored"); }
        }

        [Required]
        public string FragmentFileName
        {
            get { return fragmentFileName; }
            set { fragmentFileName = value; }
        }

        [Required]
        public ITaskItem[] ComponentFiles
        {
            get { return componentFiles; }
            set { componentFiles = value; }
        }

        public ITaskItem[] FragmentIncludeFiles
        {
            get { return includeFiles; }
            set { includeFiles = value; }
        }

        public ITaskItem[] PostFragmentIncludeFiles
        {
            get { return postIncludeFiles; }
            set { postIncludeFiles = value; }
        }

        public ITaskItem[] ComponentIncludeFiles
        {
            get { return componentIncludeFiles; }
            set { componentIncludeFiles = value; }
        }

    }
}
