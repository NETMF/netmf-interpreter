////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _TINYCLR_CHECKS_H_
#define _TINYCLR_CHECKS_H_

#include <TinyClr_Runtime.h>

struct CLR_RT_DUMP
{
#undef DECL_POSTFIX
#if defined(TINYCLR_TRACE_ERRORS)
#define DECL_POSTFIX
#else
#define DECL_POSTFIX {}
#endif

    static void TYPE  ( const CLR_RT_TypeDef_Index&       cls                 ) DECL_POSTFIX;
    static void TYPE  ( const CLR_RT_ReflectionDef_Index& reflex              ) DECL_POSTFIX;
    static void METHOD( const CLR_RT_MethodDef_Index&     method              ) DECL_POSTFIX;
    static void FIELD ( const CLR_RT_FieldDef_Index&      field               ) DECL_POSTFIX;
    static void OBJECT(       CLR_RT_HeapBlock*           ptr   , LPCSTR text ) DECL_POSTFIX;

    //--//

#undef DECL_POSTFIX
#if defined(TINYCLR_TRACE_EXCEPTIONS)
#define DECL_POSTFIX
#else
#define DECL_POSTFIX {}
#endif
    static void EXCEPTION             ( CLR_RT_StackFrame& stack, CLR_RT_HeapBlock& ref ) DECL_POSTFIX;
    static void POST_PROCESS_EXCEPTION( CLR_RT_HeapBlock& ref                           ) DECL_POSTFIX;

    static LPCSTR GETERRORMESSAGE( HRESULT hrError );
};

////////////////////////////////////////////////////////////////////////////////////////////////////

struct CLR_Checks
{
    static HRESULT VerifyStackOK( CLR_RT_StackFrame& stack, CLR_RT_HeapBlock* top, int num ) { return S_OK; }

    static HRESULT VerifyObject                ( CLR_RT_HeapBlock& top );

    static HRESULT VerifyArrayReference        ( CLR_RT_HeapBlock& ref );

    static HRESULT VerifyUnknownInstruction    ( CLR_OPCODE op         );
    static HRESULT VerifyUnsupportedInstruction( CLR_OPCODE op         );
};

////////////////////////////////////////////////////////////////////////////////////////////////////

#endif // _TINYCLR_CHECKS_H_

