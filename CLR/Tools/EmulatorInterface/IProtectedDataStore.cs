////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace Microsoft.SPOT.Emulator.PKCS11
{
    public interface ICryptokiObjectDriver
    {
        bool CreateObject     (int session, IntPtr pTemplate, int ulCount, out int phObject);
        bool CopyObject       (int session, int hObject, IntPtr pTemplate, int ulCount, out int phNewObject);
        bool DestroyObject    (int session, int hObject);
        bool GetObjectSize    (int session, int hObject, out int pulSize);
        bool GetAttributeValue(int session, int hObject, IntPtr pTemplate, int ulCount);
        bool SetAttributeValue(int session, int hObject, IntPtr pTemplate, int ulCount);    

        bool FindObjectsInit  (int session, IntPtr pTemplate, int ulCount);
        bool FindObjects      (int session, IntPtr phObjects, int ulMaxCount, out int pulObjectCount);
        bool FindObjectsFinal (int session);
    }
}



