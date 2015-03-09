////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace System
{

    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
    public struct Decimal
    {
        //The VB IDE starts to run amuck when it tries to do syntax checking on sources that contain Decimal
        //and causes the compiler to repeatedly crash unless it finds a constructor.
        public Decimal(int value) { }

        [CLSCompliant(false)]
        public Decimal(uint value) { }

        public Decimal(long value) { }

        [CLSCompliant(false)]
        public Decimal(ulong value) { }

        public Decimal(float value) { }

        public Decimal(double value) { }

        //internal Decimal(Currency value) { }

        public Decimal(int[] bits) { }
        public Decimal(int lo, int mid, int hi, bool isNegative, byte scale) { }
    }
}


