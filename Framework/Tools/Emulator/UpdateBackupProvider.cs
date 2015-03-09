using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.SPOT.Emulator.Update
{
    internal class UpdateBackupProvider : HalDriver<IUpdateBackupDriver>, IUpdateBackupDriver
    {
        #region IUpdateBackupProvider Members

        public bool CreateBackup(ushort updateType, ushort updateSubType)
        {
            return false;
        }

        public bool DeleteBackup(ushort updateType, ushort updateSubType)
        {
            return false;
        }

        public bool RestoreBackup(ushort updateType, ushort updateSubType)
        {
            return false;
        }

        #endregion
    }
}
