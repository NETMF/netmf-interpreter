using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.SPOT.Emulator.Update
{
    public interface IUpdateBackupDriver
    {
        bool CreateBackup (ushort updateType, ushort updateSubType);
        bool RestoreBackup(ushort updateType, ushort updateSubType);
        bool DeleteBackup (ushort updateType, ushort updateSubType);
    }
}
