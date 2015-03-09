using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.SPOT.Emulator.Update
{
    public interface IUpdateValidationDriver
    {
        bool AuthCommand   (MFUpdate_Emu update, uint   cmd        , IntPtr pArgs, int argsLen, IntPtr pResponse, ref int responseLen );
        bool Authenticate  (MFUpdate_Emu update, IntPtr pAuth      , int authLen);
        bool ValidatePacket(MFUpdate_Emu update, IntPtr packet     , int packetLen, IntPtr validationData, int validationLen);
        bool ValidateUpdate(MFUpdate_Emu update, IntPtr pValidation, int validationLen);
    }
}
