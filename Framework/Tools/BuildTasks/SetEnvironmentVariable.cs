using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Threading;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;

using Task = Microsoft.Build.Utilities.Task;

namespace Microsoft.SPOT.Tasks
{
    public class SetEnvironmentVariable : Task
    {
        private string variableName;
        private string variableValue;

        [Required]
        public string Name
        {
            get { return variableName; }
            set { variableName = value; }
        }

        public string Value
        {
            get { return variableValue; }
            set { variableValue = value; }
        }

        public override bool Execute()
        {
            if (variableName == null || variableName.Length == 0)
                return false;
            if (variableValue == null || variableValue.Length == 0)
                return false;

            // Set the process environment
            Environment.SetEnvironmentVariable(this.variableName, this.variableValue);
            return true;
        }
    }

    public class GetEnvironmentVariable : Task
    {
        private string variableName;
        private ITaskItem variableValue = null;

        [Required]
        public string Name
        {
            get { return variableName; }
            set { variableName = value; }
        }
        
        [Output]
        public ITaskItem Value
        {
            get { return variableValue; }
        }

        public override bool Execute()
        {
            if (variableName == null || variableName.Length == 0)
                return false;

            // Get the process environment
            string val = Environment.GetEnvironmentVariable(this.variableName);
            if(val == null) val = "";
            this.variableValue = new TaskItem(val);
            return true;
        }
    }

    public class UnsetEnvironmentVariable : Task
    {
        private string variableName;

        [Required]
        public string Name
        {
            get { return variableName; }
            set { variableName = value; }
        }

        public override bool Execute()
        {
            if (variableName == null || variableName.Length == 0)
                return false;
            // Set the process environment
            Environment.SetEnvironmentVariable(variableName, null);
            return true;
        }
    }
}
