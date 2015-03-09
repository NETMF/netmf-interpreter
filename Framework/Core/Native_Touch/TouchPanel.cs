////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.Touch
{
    public class TouchPanel
    {

        public bool Enabled
        {
            get
            {
                return _enabled;
            }

            set
            {
                EnableInternal(value);
                _enabled = value;
            }
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern void SetCalibration(int cCalibrationPoints,
                short[] screenXBuffer,
                short[] screenYBuffer,
                short[] uncalXBuffer,
                short[] uncalYBuffer);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern void GetCalibrationPointCount(ref int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern void StartCalibration();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern void GetCalibrationPoint(int index, ref int x, ref int y);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void EnableInternal(bool enable);

        private bool _enabled = false;
    }
}


