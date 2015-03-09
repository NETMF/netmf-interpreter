using System;
using System.Collections.Generic;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;

using Task = Microsoft.Build.Utilities.Task;

namespace Microsoft.SPOT.Tasks.Internal
{
    public class GetGuid : Task
    {
        [Output]
        public string Guid
        {
            get { return System.Guid.NewGuid().ToString(); }
        }

        public override bool Execute()
        {
            try
            {
                return true;
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
                return false;
            }
        }
    }
}
