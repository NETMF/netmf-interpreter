////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.SPOT.Hardware
{
    public static class Utility
    {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public uint ComputeCRC(byte[] buf, int offset, int length, uint crc);
        //--//

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public uint ExtractValueFromArray(byte[] data, int pos, int size);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public void InsertValueIntoArray(byte[] data, int pos, int size, uint val);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public byte[] ExtractRangeFromArray(byte[] data, int offset, int count);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public byte[] CombineArrays(byte[] src1, byte[] src2);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public byte[] CombineArrays(byte[] src1, int offset1, int count1, byte[] src2, int offset2, int count2);
        //--//

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public void SetLocalTime(DateTime dt);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public TimeSpan GetMachineTime();
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public void Piezo(uint frequency, uint duration);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public void Backlight(bool fStatus);
    }
}


