using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.SPOT.Emulator.PKCS11;
using System.Reflection;
using System.IO;
using System.Windows.Forms;

namespace Microsoft.SPOT.Emulator.Update
{
    internal class UpdateProvider : HalDriver<IUpdateDriver>, IUpdateDriver
    {
        public enum MFUpdateType : ushort
        {
            FirmwareUpdate,
            AssemblyUpdate,
            KeyUpdate,
            UserDefined = 0x8000
        }

        [Flags]
        public enum MFUpdateSubType : ushort
        {
            Firmware_All = 0x0000,
            Firmware_CLR = 0x0001,
            Firmware_DAT = 0x0002,
            Firmware_CFG = 0x0003,

            Assembly_ADD = 0x0001,
            Assembly_REPLACE_DEPLOY = 0x0002,
        }

        #region IUpdateProvider Members

        public bool InitializeUpdate(IntPtr updateHeader)
        {
            bool supportedUpdate = false;
            MFUpdate_Header header = (MFUpdate_Header)Marshal.PtrToStructure(updateHeader, typeof(MFUpdate_Header));

            switch((MFUpdateType)header.UpdateType)
            {
                case MFUpdateType.AssemblyUpdate:
                case MFUpdateType.KeyUpdate:
                case MFUpdateType.FirmwareUpdate:
                    supportedUpdate = true;
                    break;

                default:
                    supportedUpdate = false;
                    break;
            }

            return supportedUpdate;
        }

        public bool GetProperty(MFUpdate_Emu update, string propertyName, IntPtr propValue, ref int propSize)
        {
            return false;
        }

        public bool SetProperty(MFUpdate_Emu update, string propertyName, IntPtr propValue, int propSize)
        {
            return false;
        }

        public bool InstallUpdate(MFUpdate_Emu update, IntPtr pValidation, int validationLen)
        {
            bool fValid = false;
            MFUpdate_Header header = new MFUpdate_Header();

            GCHandle h = GCHandle.Alloc(header, GCHandleType.Pinned);
            IntPtr ptr = h.AddrOfPinnedObject();
            Hal.UpdateStorage.Read(update.StorageHandle, 0, ptr, Marshal.SizeOf(header));
            header = (MFUpdate_Header)Marshal.PtrToStructure(ptr, typeof(MFUpdate_Header));
            h.Free();

            switch((MFUpdateType)header.UpdateType)
            {
                case MFUpdateType.AssemblyUpdate:
                    // signature check or strong name check
                    fValid = true;
                    if (fValid)
                    {
                        byte[] asmBytes = new byte[header.UpdateSize];

                        GCHandle hb = GCHandle.Alloc(asmBytes, GCHandleType.Pinned);
                        IntPtr gcBytes = h.AddrOfPinnedObject();

                        if (0 < Hal.UpdateStorage.Read(update.StorageHandle, Marshal.SizeOf(header), gcBytes, (int)header.UpdateSize))
                        {
                            Marshal.Copy(gcBytes, asmBytes, 0, asmBytes.Length);

                            string fTmp = Path.ChangeExtension(Path.GetTempFileName(), ".pe");
                            try
                            {
                                using (FileStream fs = File.OpenWrite(fTmp))
                                {
                                    fs.Write(asmBytes, 0, asmBytes.Length);
                                }

                                Emulator.LoadAssembly(fTmp);
                                Emulator.Run();

                                File.Delete(fTmp);
                            }
                            catch
                            {
                                return false;
                            }
                            finally
                            {
                                if (File.Exists(fTmp))
                                {
                                    File.Delete(fTmp);
                                }
                            }
                        }

                        hb.Free();
                    }
                    break;

                case MFUpdateType.KeyUpdate:
                    // unwrap and store
                    break;
            }

            return fValid;
        }

        #endregion
    }
}
