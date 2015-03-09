using System;
using Microsoft.Build.Evaluation;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace MFDpwsTestCaseGenerator
{
    class ServiceProject : Project
    {
        private bool m_DevProj;

        public ServiceProject(
            CodeFile interfaceFile,
            CodeFile clientProxyFile,
            CodeFile hostedServiceFile,
            WsdlFileX wsdlFile)
            : base(new XmlTextReader(new StringReader(!LocatorFactory.IsDevEnvironment ? Resources.SDKProjectTemplate : Resources.DevProjectTemplate)))
        {
            m_DevProj = LocatorFactory.IsDevEnvironment;
            this.SetProperty("ProjectGuid", "{" + Guid.NewGuid().ToString().ToUpper() + "}");

            AddItem("Compile", interfaceFile.FileInfo.Name);
            AddItem("Compile", clientProxyFile.FileInfo.Name);
            AddItem("Compile", hostedServiceFile.FileInfo.Name);

            AddItem("None", wsdlFile.FileInfo.Name);
        }

        public bool IsDevProject { get { return m_DevProj; } }
        public bool IsSDKProject { get { return !m_DevProj; } }

        public string AssemblyName
        {
            get { return this.GetPropertyValue("AssemblyName"); }
            set { this.SetProperty("AssemblyName", value); }
        }

        public string RootNamespace
        {
            get { return this.GetPropertyValue("RootNamespace"); }
            set { this.SetProperty("RootNamespace", value); }
        }
    }

    class SlnProj : Project
    {
        string m_FileName;

        public SlnProj(string projFile, Guid componentGuid)
            : base(XmlTextReader.Create(new StringReader(Resources.SlnProjTemplate)))
        {
            m_FileName = System.IO.Path.GetFileNameWithoutExtension(projFile) + ".slnproj";

            this.SetProperty("ComponentGuid", componentGuid.ToString());

            Dictionary<string, string> meta = new Dictionary<string, string>();
            meta.Add("Transform", "Client");
            meta.Add("Deploy", "true");

            AddItem("ProjectFile", projFile, meta);
        }

        public new void Save()
        {
            this.Save(m_FileName);
        }
    }
}
