////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#include "CorLib.h"

#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
#include "Double_decl.h"

HRESULT Library_corlib_native_System_Math::Acos___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double d = stack.Arg0().NumericByRefConst().r8;
    double res = System::Math::Acos( d );

    stack.SetResult_R8( res );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Math::Asin___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double d = stack.Arg0().NumericByRefConst().r8;
    double res = System::Math::Asin( d );

    stack.SetResult_R8( res );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Math::Atan___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double d = stack.Arg0().NumericByRefConst().r8;
    double res = System::Math::Atan( d );

    stack.SetResult_R8( res );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Math::Atan2___STATIC__R8__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double y = stack.Arg0().NumericByRefConst().r8;
    double x = stack.Arg1().NumericByRefConst().r8;
    double res = System::Math::Atan2( y, x );

    stack.SetResult_R8( res );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Math::Ceiling___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double d = stack.Arg0().NumericByRefConst().r8;
    double res = System::Math::Ceiling( d );

    stack.SetResult_R8( res );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Math::Cos___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double d = stack.Arg0().NumericByRefConst().r8;
    double res = System::Math::Cos( d );

    stack.SetResult_R8( res );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Math::Cosh___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double d = stack.Arg0().NumericByRefConst().r8;
    double res = System::Math::Cosh( d );

    stack.SetResult_R8( res );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Math::IEEERemainder___STATIC__R8__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double x = stack.Arg0().NumericByRefConst().r8;
    double y = stack.Arg1().NumericByRefConst().r8;
    double res = System::Math::IEEERemainder(x, y);

    stack.SetResult_R8( res );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Math::Exp___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double d = stack.Arg0().NumericByRefConst().r8;
    double res = System::Math::Exp( d );

    stack.SetResult_R8( res );

    TINYCLR_NOCLEANUP_NOLABEL();;
}

HRESULT Library_corlib_native_System_Math::Floor___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double d = stack.Arg0().NumericByRefConst().r8;
    double res = System::Math::Floor( d );

    stack.SetResult_R8( res );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Math::Log___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double d = stack.Arg0().NumericByRefConst().r8;
    double res = System::Math::Log( d );

    stack.SetResult_R8( res );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Math::Log10___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double d = stack.Arg0().NumericByRefConst().r8;
    double res = System::Math::Log10( d );

    stack.SetResult_R8( res );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Math::Pow___STATIC__R8__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double x = stack.Arg0().NumericByRefConst().r8;
    double y = stack.Arg1().NumericByRefConst().r8;

    double res = System::Math::Pow( x, y );

    stack.SetResult_R8( res );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Math::Round___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double d = stack.Arg0().NumericByRefConst().r8;
    double hi = d + 0.5;
    double res = System::Math::Floor( hi );

    //If the number was in the middle of two integers, we need to round to the even one.
    if(res==hi)
    {
        if(System::Math::Fmod( res, 2.0 ) != 0)
        {
            //Rounding up made the number odd so we should round down.
            res -= 1.0;
        }
    }

    stack.SetResult_R8( res );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Math::Sign___STATIC__I4__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double d = stack.Arg0().NumericByRefConst().r8;
    INT32 res;
    if (d < 0) res =  -1;
    else if (d > 0) res =  +1;
    else res = 0;

    stack.SetResult_I4( res );

    TINYCLR_NOCLEANUP_NOLABEL();;
}

HRESULT Library_corlib_native_System_Math::Sin___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double d = stack.Arg0().NumericByRefConst().r8;
    double res = System::Math::Sin( d );

    stack.SetResult_R8( res );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Math::Sinh___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double d = stack.Arg0().NumericByRefConst().r8;
    double res = System::Math::Sinh( d );

    stack.SetResult_R8( res );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Math::Sqrt___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double d = stack.Arg0().NumericByRefConst().r8;
    double res = System::Math::Sqrt( d );

    stack.SetResult_R8( res );

    TINYCLR_NOCLEANUP_NOLABEL();

}

HRESULT Library_corlib_native_System_Math::Tan___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double d = stack.Arg0().NumericByRefConst().r8;
    double res = System::Math::Tan( d );

    stack.SetResult_R8( res );

    TINYCLR_NOCLEANUP_NOLABEL();

}

HRESULT Library_corlib_native_System_Math::Tanh___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double d = stack.Arg0().NumericByRefConst().r8;
    double res = System::Math::Tanh( d );

    stack.SetResult_R8( res );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Math::Truncate___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double d = stack.Arg0().NumericByRefConst().r8;
    double res = 0.0;
    double retVal = System::Math::Truncate(d, res);

    stack.SetResult_R8( res );

    TINYCLR_NOCLEANUP_NOLABEL();

}

#else

/// No floating point 

HRESULT Library_corlib_native_System_Math::Acos___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Math::Asin___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Math::Atan___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Math::Atan2___STATIC__R8__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Math::Ceiling___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_INT64  d = stack.Arg0().NumericByRefConst().r8;

    CLR_INT64 res = (CLR_INT64)(d + (CLR_INT64)CLR_RT_HeapBlock::HB_DoubleMask) & (~CLR_RT_HeapBlock::HB_DoubleMask);

    stack.SetResult_I8( res );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Math::Cos___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Math::Cosh___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Math::IEEERemainder___STATIC__R8__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Math::Exp___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Math::Floor___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_INT64 d = stack.Arg0().NumericByRefConst().r8;
    CLR_INT64 res = (CLR_INT64)( d  & (~CLR_RT_HeapBlock::HB_DoubleMask) );
    stack.SetResult_I8( res );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Math::Log___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Math::Log10___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Math::Pow___STATIC__R8__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Math::Round___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_INT64 d = stack.Arg0().NumericByRefConst().r8;

    //for negative number we have to be banker's round, if -0.5, round to 0, but 0.5 to 0
    if (d <0) d =d + 1; 
    CLR_INT64 res = (CLR_INT64)(d + (CLR_INT64)(CLR_RT_HeapBlock::HB_DoubleMask>>1) ) & (~CLR_RT_HeapBlock::HB_DoubleMask);

    stack.SetResult_I8( res );


    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Math::Sign___STATIC__I4__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Math::Sin___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Math::Sinh___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Math::Sqrt___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Math::Tan___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Math::Tanh___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Math::Truncate___STATIC__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

#endif

