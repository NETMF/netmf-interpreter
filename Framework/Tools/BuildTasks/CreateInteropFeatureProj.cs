using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Reflection;
using System.Threading;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using _EVAL=Microsoft.Build.Evaluation;
using Microsoft.Build.Construction;
using Microsoft.Build.Tasks;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Configuration;
using System.Security;
using System.Xml;
using System.Text;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace Microsoft.SPOT.Tasks
{
    public class CreateInteropFeatureProj : Task
    {
        private string m_stubsPath;
        [Required]
        public string StubsPath
        {
            set { m_stubsPath = value; }
            get { return m_stubsPath; }
        }

        private string m_name;
        [Required]
        public string Name
        {
            set { m_name = value; }
            get { return m_name; }
        }

        private string m_assemblyName;
        [Required]
        public string AssemblyName
        {
            set { m_assemblyName = Path.GetFileName(value); }
            get { return m_assemblyName; }
        }

        private string m_nativeProjectFile;
        [Required]
        public string NativeProjectFile
        {
            set { m_nativeProjectFile = value; }
            get { return m_nativeProjectFile; }
        }

        private string m_managedProjectFile;
        [Required]
        public string ManagedProjectFile
        {
            set { m_managedProjectFile = value; }
            get { return m_managedProjectFile; }
        }

        public override bool Execute()
        {

            bool result = true;

            string file = Path.Combine(m_stubsPath, m_name + ".featureproj");

            try
            {

                _EVAL.Project proj = new _EVAL.Project();

                Microsoft.Build.Evaluation.ProjectCollection  projCol = proj.ProjectCollection;

                ProjectPropertyGroupElement bpg = proj.Xml.AddPropertyGroup(); 
                bpg.AddProperty("FeatureName", m_name);
                bpg.AddProperty("Guid", System.Guid.NewGuid().ToString("B"));
                bpg.AddProperty("Description", "<Add Feature Description Here>");
                bpg.AddProperty("Groups", "");
                bpg.AddProperty( m_name.ToUpper().Replace(".", "_") + "_FEATUREPROJ", "True");

                ProjectItemGroupElement big = proj.Xml.AddItemGroup();
                big.AddItem("InteropFeature", Path.GetFileNameWithoutExtension(m_assemblyName).Replace('.', '_'));
                big.AddItem("DriverLibs", Path.GetFileNameWithoutExtension(m_assemblyName).Replace('.', '_') + ".$(LIB_EXT)");
                big.AddItem("MMP_DAT_CreateDatabase", "$(BUILD_TREE_CLIENT)\\pe\\$(ENDIANNESS)\\" + m_assemblyName);
                big.AddItem("RequiredProjects", Path.Combine(m_stubsPath, m_nativeProjectFile));

            
                proj.Save(file);

                // after save, unload the project, so that if rebuilds it is able to regenerate the project file.   
                ProjectRootElement pre = proj.Xml;
                projCol.UnloadProject(proj);
                projCol.UnloadProject(pre);

            }
            catch (Exception e)
            {
                Log.LogError("Error trying to create feature project file \"" + file + "\": " + e.Message);
                result = false;
            }               

            return result;
        }
    }
}
