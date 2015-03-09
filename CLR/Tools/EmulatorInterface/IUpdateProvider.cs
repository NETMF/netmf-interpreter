using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.SPOT.Emulator.Update
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MFUpdate_Version
    {
        public ushort Major;
        public ushort Minor;
        public ushort Build;
        public ushort Revision;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MFUpdate_Header
    {
        public int UpdateID;
        public MFUpdate_Version Version;
        public ushort UpdateType;
        public ushort UpdateSubType;
        public uint UpdateSize;
        public uint PacketSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MFUpdate_Emu
    {
        public MFUpdate_Header Header;
        public int StorageHandle;
        public uint Flags;
    }


    public interface IUpdateDriver
    {
        bool InitializeUpdate(IntPtr updateHeader);
        bool GetProperty     (MFUpdate_Emu update, string propertyName, IntPtr propValue, ref int propSize);
        bool SetProperty     (MFUpdate_Emu update, string propertyName, IntPtr propValue, int propSize);
        bool InstallUpdate   (MFUpdate_Emu update, IntPtr pValidation , int validationLen);
    }
}
