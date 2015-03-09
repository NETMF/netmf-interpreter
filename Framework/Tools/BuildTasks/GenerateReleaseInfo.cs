using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Build.BuildEngine;
using _BE=Microsoft.Build.Evaluation;
using System.Text.RegularExpressions;
using Microsoft.Build.Construction;

namespace Microsoft.SPOT.Tasks
{
    public sealed class GenerateReleaseInfo : Task
    {
        private string m_inputFile;
        private string m_outputFile;
        private string m_buildNumber;

        [Required]
        public string BuildNumber
        {
            set { m_buildNumber = value; }
            get { return m_buildNumber; }
        }

        [Required]
        public string InputFile
        {
            set { m_inputFile = value; }
            get { return m_inputFile; }
        }

        [Required]
        public string OutputFile
        {
            set { m_outputFile = value; }
            get { return m_outputFile; }
        }

        public override bool Execute()
        {
            try
            {
                _BE.Project input = new _BE.Project(m_inputFile);

                bool fFound = false;

                foreach (ProjectPropertyGroupElement bpg in input.Xml.PropertyGroups)
                {
                    foreach (ProjectPropertyElement bp in bpg.Properties)
                    {
                        if (string.Compare(bp.Name, "BuildNumber", true) == 0)
                        {
                            bpg.SetProperty(bp.Name, m_buildNumber);
                            fFound = true;
                            break;
                        }
                    }
                    if (fFound) break;
                }

                input.Save(m_outputFile, Encoding.ASCII);
            }
            catch (Exception e)
            {
                Log.LogError("Error trying to create release info file {0} at {1}: ({2})", m_inputFile, m_outputFile, e.Message);
                return false;
            }
            return true;
        }

    }
}
