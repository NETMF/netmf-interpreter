using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.SPOT.Emulator.Update
{
    public interface IUpdateStorageDriver
    {
        int  Create   (MFUpdate_Header storageHeader);
        int  Open     (int storageID, ushort storageType, ushort storageSubType);
        void Close    (int handle);
        bool Delete   (int storageID, ushort storageType, ushort storageSubType);
        bool GetFiles (ushort storageType, IntPtr storageIDs, ref int storageCount);
        bool IsErased (int handle, int fileOffset, int len);
        int  Write    (int handle, int fileOffset, IntPtr pData, int len);
        int  Read     (int handle, int fileOffset, IntPtr pData, int len);
        bool GetHeader(int handle, ref MFUpdate_Header header);
    }
}
