////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Microsoft.SPOT.Emulator.PKCS11
{
    internal class RandomDriver : HalDriver<IRandomDriver>, IRandomDriver
    {
        static Random s_rand = new Random();

        #region IRandomDriver Members

        bool IRandomDriver.GenerateRandom(int session, IntPtr Data, int RandomLen)
        {
            try
            {
                byte[] data = new byte[RandomLen];

                s_rand.NextBytes(data);

                Marshal.Copy(data, 0, Data, RandomLen);
            }
            catch
            {
                return false;
            }

            return true;
        }

        bool IRandomDriver.SeedRandom(int session, IntPtr Seed, int SeedLen)
        {
            try
            {
                int[] seed = new int[1];

                Marshal.Copy(Seed, seed, 0, 1);

                s_rand = new Random(seed[0]);
            }
            catch
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}