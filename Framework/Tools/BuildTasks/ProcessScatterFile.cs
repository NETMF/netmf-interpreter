#region Using directives

using System;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Win32;
using System.Diagnostics;
using System.Collections.Generic;

using Microsoft.SPOT.Tasks.ScatterFile;

#endregion

namespace Microsoft.SPOT.Tasks
{
    public sealed class ProcessScatterFile : Task, IEnvironment
    {
        #region Properties

        private ITaskItem m_definitionFile = null;

        private ITaskItem m_outputFile = null;

        private ITaskItem[] m_properties = new ITaskItem[0];
        
        private Dictionary<string,string> m_propertyMap;


        [Required]
        public ITaskItem DefinitionFile
        {
            set { m_definitionFile = value; }
            get { return m_definitionFile; }
        }

        [Required]
        public ITaskItem OutputFile
        {
            set { m_outputFile = value; }
            get { return m_outputFile; }
        }


        public ITaskItem[] Properties
        {
            set
            { 
                m_properties = value;
                m_propertyMap = new Dictionary<string,string>();
                
                Char[] splitSet = new Char [] {'='};
                foreach (ITaskItem i in m_properties)
                {
                    string[] ss = i.ItemSpec.Split(splitSet);
                    m_propertyMap[ss[0]] = ss[1];
                }

            }
            get { return m_properties; }
        }

        public string GetVariable(string name)
        {
            try
            {
                return m_propertyMap[name];
            }
            catch (Exception )
            {
                return null;
            }            
        }

        #endregion

        #region ITask Members

        public override bool Execute()
        {
            try
            {
                Document doc = Document.Load( m_definitionFile.ItemSpec, this );

                string[] res = doc.Execute();

                using(StreamWriter sw = new StreamWriter( m_outputFile.ItemSpec ))
                {
                    foreach(string line in res)
                    {
                        sw.WriteLine( "{0}", line );
                    }
                }
            }
            catch (Exception e)
            {
			    Log.LogError("Error trying to process scatterfile " + m_definitionFile.ItemSpec + " into " + m_outputFile.ItemSpec + ": " + e.Message);
                return false;
            }
            return true;
        }

        #endregion
    }
}
