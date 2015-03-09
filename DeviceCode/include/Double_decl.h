////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#ifndef _NATIVE_DOUBLE_PAL_H_
#define _NATIVE_DOUBLE_PAL_H_

#include <tinyhal.h>

namespace System
{
    struct Double
    {
        static INT32 CompareTo( double d, double val );
        static bool IsInfinity( double d );
        static bool IsNaN( double d );
        static bool IsNegativeInfinity( double d );
        static bool IsPositiveInfinity( double d );
    };
    
    struct Math
    {
        static double Acos(double d);
        static double Asin(double d);
        static double Atan(double d);
        static double Atan2(double x, double y);
        static double Ceiling(double d);
        static double Cos(double d);
        static double Cosh(double d);
        static double IEEERemainder(double x, double y );
        static double Exp(double d);
        static double Floor(double d);
        static double Fmod(double x, double y );
        static double Log(double d);
        static double Log10(double d);
        static double Pow(double x, double y);
        static double Round(double d);
        static INT32  Sign(double d);
        static double Sin(double d);
        static double Sinh(double d);
        static double Sqrt(double d);
        static double Tan(double d);
        static double Tanh(double d);
        static double Truncate(double d, double& integralValue);
    };
}
#endif  //_NATIVE_DOUBLE_PAL_H_
