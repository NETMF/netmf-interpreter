using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;

namespace Microsoft.SPOT.Emulator.Update
{
    internal class UpdateStorageProvider : HalDriver<IUpdateStorageDriver>, IUpdateStorageDriver
    {
        internal class UpdateClass
        {
            internal UpdateClass(MFUpdate_Header header)
            {
                Header = header;
                Data = new byte[header.UpdateSize];
            }

            internal MFUpdate_Header Header;
            internal byte[] Data;
        }
        private Dictionary<int, UpdateClass> m_updateStorage = new Dictionary<int,UpdateClass>();

        #region IUpdateStorageProvider Members

        public int Create(MFUpdate_Header storageHeader)
        {
            if(m_updateStorage.ContainsKey(storageHeader.UpdateID)) return -1;

            m_updateStorage[storageHeader.UpdateID] = new UpdateClass(storageHeader);

            return (int)storageHeader.UpdateID;
        }

        public int Open(int storageID, ushort storageType, ushort storageSubType)
        {
            if (m_updateStorage.ContainsKey(storageID))
            {
                return (int)storageID;
            }

            return -1;
        }

        public void Close(int handle)
        {
        }

        public bool Delete(int storageID, ushort storageType, ushort storageSubType)
        {
            if (m_updateStorage.ContainsKey(storageID))
            {
                m_updateStorage.Remove(storageID);
                return true;
            }

            return false;
        }

        public bool GetFiles(ushort storageType, IntPtr storageIDs, ref int storageCount)
        {
            bool fRet = false;
            IntPtr ptrTmp = storageIDs;

            if (storageIDs == IntPtr.Zero)
            {
                storageCount = 0;
            }

            for (int i = 0; i < m_updateStorage.Keys.Count; i++)
            {
                int key = m_updateStorage.Keys.ElementAt(i);

                if (m_updateStorage[key].Header.UpdateType == storageType)
                {
                    fRet = true;
                    if (storageIDs != IntPtr.Zero)
                    {
                        Marshal.WriteInt32(ptrTmp, (int)storageIDs);
                        ptrTmp += 1;
                    }
                    else
                    {
                        storageCount++;
                    }
                }
            }

            return fRet;
        }

        public bool IsErased(int handle, int fileOffset, int len)
        {
            bool fErased = true;
            if (!m_updateStorage.ContainsKey(handle)) return false;

            byte[] data = m_updateStorage[handle].Data;

            if(data.Length < (fileOffset + len)) return false;

            while (len-- > 0)
            {
                if (data[fileOffset++] != 0)
                {
                    fErased = false;
                    break;
                }
            }
            return fErased;
        }

        public int Read(int handle, int fileOffset, IntPtr pData, int len)
        {
            if (!m_updateStorage.ContainsKey(handle)) return -1;

            byte[] data = m_updateStorage[handle].Data;

            if (data.Length < (fileOffset + len)) len = data.Length - fileOffset;

            if (len <= 0) return -1;

            Marshal.Copy(data, fileOffset, pData, len);

            return len;
        }

        public int Write(int handle, int fileOffset, IntPtr pData, int len)
        {
            if (!m_updateStorage.ContainsKey(handle)) return -1;

            byte[] data = m_updateStorage[handle].Data;

            if (data.Length < (fileOffset + len)) len = data.Length - fileOffset;

            if (len <= 0) return -1;

            Marshal.Copy(pData, data, fileOffset, len);

            return len;
        }

        public bool GetHeader(int handle, ref MFUpdate_Header header)
        {
            if (!m_updateStorage.ContainsKey(handle)) return false;

            header = m_updateStorage[handle].Header;

            return true;
        }

        #endregion
    }
}
