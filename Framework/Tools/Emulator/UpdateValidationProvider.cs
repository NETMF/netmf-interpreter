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
    internal class UpdateValidationProvider : HalDriver<IUpdateDriver>, IUpdateValidationDriver
    {
        public enum MFUpdateType : uint
        {
            FirmwareUpdate,
            AssemblyUpdate,
            KeyUpdate,
            UserDefined = 0x80000000
        }

        #region IUpdateProvider Members

        public bool AuthCommand(MFUpdate_Emu update, uint cmd, IntPtr pArgs, int argsLen, IntPtr pResponse, ref int responseLen)
        {
            return true;
        }

        public bool Authenticate(MFUpdate_Emu update, IntPtr pAuth, int authLen)
        {
            return true;
        }

        public bool ValidatePacket(MFUpdate_Emu update, IntPtr packet, int packetLen, IntPtr validationData, int validationLen)
        {
            bool valid = false;
            switch((MFUpdateType)update.Header.UpdateType)
            {
                case MFUpdateType.AssemblyUpdate:
                    HashAlgorithm alg = null;

                    switch (validationLen)
                    {
                        case 512 / 8:
                            alg = new SHA512Cng();
                            break;

                        case 256 / 8:
                            alg = new SHA256Cng();
                            break;

                        case 384 / 8:
                            alg = new SHA384Cng();
                            break;

                        case 160 / 8:
                            alg = new SHA1Cng();
                            break;
                    }

                    if(alg != null)
                    {
                        byte[] data = new byte[packetLen];
                        Marshal.Copy(packet, data, 0, packetLen);

                        byte[] hash = alg.ComputeHash(data);

                        if(hash.Length == validationLen)
                        {
                            valid = true;
                            byte[] vHash = new byte[validationLen];
                            Marshal.Copy(validationData, vHash, 0, validationLen);
    
                            for(int i=0; i<validationLen; i++)
                            {
                                if (vHash[i] != hash[i])
                                {
                                    valid = false;
                                    break;
                                }
                            }
                        }
                    }
                    break;

                case MFUpdateType.FirmwareUpdate:
                    valid = true;
                    break;

                case MFUpdateType.KeyUpdate:
                    // TODO:
                    break;
            }
            return valid;
        }

        public bool ValidateUpdate(MFUpdate_Emu update, IntPtr pValidation, int validationLen)
        {
            // todo: add signature check
            return true;
        }

        #endregion

    }
}
