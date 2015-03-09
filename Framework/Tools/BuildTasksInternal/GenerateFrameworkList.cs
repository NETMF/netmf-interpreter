using System;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using _BE = Microsoft.Build.Evaluation;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;

using Task = Microsoft.Build.Utilities.Task;

namespace Microsoft.SPOT.Tasks.Internal
{
    public class GenerateFrameworkList : Task
    {
        private string m_outputPath;
        private string m_frameListFile;
        private string m_version;
        private string m_toolsVersion;
        private string m_token;

        #region MSBuild Task Parameters
        [Required]
        public string BuildOutputDirectory
        {
            get { return m_outputPath; }
            set { m_outputPath = value; }
        }

        [Required]
        public string FrameworkListFile
        {
            get { return m_frameListFile; }
            set { m_frameListFile = value; }
        }

        [Required]
        public string FrameworkVersion
        {
            get { return m_version; }
            set { m_version = value; }
        }

        [Required]
        public string ToolsVersion
        {
            get { return m_toolsVersion; }
            set { m_toolsVersion = value; }
        }

        [Required]
        public string PublicKeyToken
        {
            get { return m_token; }
            set { m_token = value; }
        }

        #endregion

        public override bool Execute()
        {
            bool result = true;

            if (File.Exists(m_frameListFile)) File.Delete(m_frameListFile);

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = UTF8Encoding.UTF8;
            settings.Indent = true;
            settings.NewLineChars = "\r\n";
            settings.NewLineHandling = NewLineHandling.Entitize;

            string path = Path.GetDirectoryName(m_frameListFile);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            using (XmlWriter xml = XmlWriter.Create(m_frameListFile, settings))
            {
                xml.WriteStartDocument();
                xml.WriteStartElement("FileList");
                xml.WriteAttributeString("Name", ".NET Micro Framework " + m_version);
                xml.WriteAttributeString("Redist", "Microsoft-Windows-CLRCoreComp-MicroFramework-" + m_version);
                xml.WriteAttributeString("ToolsVersion", m_toolsVersion);

                Log.LogMessage("Adding assemblies to framework list: " + m_outputPath);

                foreach (string ext in new string[] { "*.exe", "*.dll" })
                {
                    foreach (string file in Directory.GetFiles(m_outputPath, ext, SearchOption.TopDirectoryOnly))
                    {
                        try
                        {
                            byte[] fileIL = File.ReadAllBytes(file);

                            Assembly asm = Assembly.Load(fileIL);
                            AssemblyName name = new AssemblyName(asm.FullName);

                            xml.WriteStartElement("File");
                            xml.WriteAttributeString("AssemblyName", name.Name);
                            xml.WriteAttributeString("Version", name.Version.ToString(4));
                            xml.WriteAttributeString("PublicKeyToken", m_token);
                            xml.WriteAttributeString("Culture", "neutral");
                            xml.WriteAttributeString("ProcessorArchitecture", "MSIL");
                            xml.WriteAttributeString("InGac", "false");
                            xml.WriteEndElement();
                        }
                        catch
                        {
                            Log.LogWarning("Exception loading assembly: " + file);
                        }
                    }
                }

                xml.WriteEndElement();
                xml.WriteEndDocument();
            }

            return result;
        }
    }
}
