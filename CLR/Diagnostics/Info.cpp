////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Diagnostics.h"


////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(PLATFORM_WINDOWS) 

static std::string* s_redirectedString = NULL;

void CLR_Debug::RedirectToString( std::string* str )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    s_redirectedString = str;
}

#endif

#if defined(PLATFORM_WINDOWS) || defined(PLATFORM_WINCE)

HRESULT TINYCLR_DEBUG_PROCESS_EXCEPTION( HRESULT hr, LPCSTR szFunc, LPCSTR szFile, int line )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    switch(hr)
    {
    case CLR_E_ENTRY_NOT_FOUND:
    case CLR_E_PROCESS_EXCEPTION:
    case CLR_E_THREAD_WAITING:
    case CLR_E_RESTART_EXECUTION:
    case CLR_E_RESCHEDULE:
    case CLR_E_OUT_OF_MEMORY:
        return hr;
    }

    if(s_CLR_RT_fTrace_StopOnFAILED >= c_CLR_RT_Trace_Info)
    {
        if(::IsDebuggerPresent())
        {
            ::DebugBreak();
        }
    }
    return hr;
}

#else

#if defined(TINYCLR_TRACE_HRESULT)
HRESULT TINYCLR_DEBUG_PROCESS_EXCEPTION( HRESULT hr, LPCSTR szFunc, LPCSTR szFile, int line )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    switch(hr)
    {
    case CLR_E_ENTRY_NOT_FOUND:

    case CLR_E_PROCESS_EXCEPTION:
    case CLR_E_THREAD_WAITING:
    case CLR_E_RESTART_EXECUTION:
        return hr;
    }

    CLR_Debug::Printf( "HRESULT %08x: %s %s:%d\r\n", hr, szFunc, szFile, line );
    return hr;
}
#endif

#endif

//--//

bool CLR_SafeSprintfV( LPSTR& szBuffer, size_t& iBuffer, LPCSTR format, va_list arg )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    int  chars = hal_vsnprintf( szBuffer, iBuffer, format, arg );
    bool fRes  = (chars >= 0);

    if(fRes == false) chars = (int)iBuffer;

    szBuffer += chars; szBuffer[ 0 ] = 0;
    iBuffer  -= chars;

    return fRes;
}

bool CLR_SafeSprintf( LPSTR& szBuffer, size_t& iBuffer, LPCSTR format, ... )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    va_list arg;
    bool    fRes;

    va_start( arg, format );

    fRes = CLR_SafeSprintfV( szBuffer, iBuffer, format, arg );

    va_end( arg );

    return fRes;
}


//--//

void CLR_Debug::Flush()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    DebuggerPort_Flush( HalSystemConfig.DebugTextPort );
}

void CLR_Debug::Emit( const char *text, int len )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    static char s_buffer[ 128 ];
    static int  s_chars = 0;

    if(CLR_EE_DBG_IS( RebootPending)) return;

    if(len == -1) len = (int)hal_strlen_s( text );

#if defined(PLATFORM_WINDOWS)
    if(s_redirectedString)
    {
        s_redirectedString->append( text, len );
        return;
    }

    if(s_CLR_RT_fTrace_RedirectOutput.size())
    {
        static HANDLE hFile = INVALID_HANDLE_VALUE;
        static int    lines = 0;
        static int    num   = 0;

        if(hFile == INVALID_HANDLE_VALUE)
        {
            std::wstring file = s_CLR_RT_fTrace_RedirectOutput;

            if(s_CLR_RT_fTrace_RedirectLinesPerFile)
            {
                WCHAR rgBuf[ 64 ];

                swprintf( rgBuf, ARRAYSIZE(rgBuf), L".%08d", num++ );

                file.append( rgBuf );
            }

            hFile = ::CreateFileW( file.c_str(), GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, 0, CREATE_ALWAYS, 0, 0 );

            lines = 0;
        }

        if(hFile != INVALID_HANDLE_VALUE)
        {
            DWORD dwWritten;

            ::WriteFile( hFile, text, (DWORD)len, &dwWritten, NULL );

            if(s_CLR_RT_fTrace_RedirectLinesPerFile)
            {
                while((text = strchr( text, '\n' )) != NULL)
                {
                    lines++;
                    text++;

                    if(text[ 0 ] == 0)
                    {
                        if(lines > s_CLR_RT_fTrace_RedirectLinesPerFile)
                        {
                            ::CloseHandle( hFile ); hFile = INVALID_HANDLE_VALUE;
                        }

                        break;
                    }
                }
            }

            return;
        }
    }
#endif

    while(len > 0)
    {
        int avail = MAXSTRLEN(s_buffer) - s_chars;

        if(len < avail) avail = len;

        memcpy( &s_buffer[ s_chars ], text, avail );

        s_chars += avail;
        text    += avail;
        len     -= avail;
        s_buffer[ s_chars ] = 0;

        if(s_chars > 80 || strchr( s_buffer, '\n' ))
        {
            ::Watchdog_ResetCounter();

#if defined(PLATFORM_WINDOWS) || defined(PLATFORM_WINCE)
            HAL_Windows_Debug_Print( s_buffer );
#endif

            if(CLR_EE_DBG_IS( Enabled ) && !CLR_EE_DBG_IS( Quiet ))
            {
                CLR_EE_DBG_EVENT_BROADCAST( CLR_DBG_Commands::c_Monitor_Message, s_chars, s_buffer, WP_Flags::c_NonCritical | WP_Flags::c_NoCaching );
            }

            if(!CLR_EE_DBG_IS( Enabled ) || HalSystemConfig.DebugTextPort != HalSystemConfig.DebuggerPorts[ 0 ])
            {
#if !defined(PLATFORM_WINDOWS) && !defined(PLATFORM_WINCE)
                DebuggerPort_Write( HalSystemConfig.DebugTextPort, s_buffer, s_chars, 0 ); // skip null terminator and don't bother retrying
                DebuggerPort_Flush( HalSystemConfig.DebugTextPort );                    // skip null terminator
#endif
            }

            s_chars = 0;
        }
    }
}

int CLR_Debug::PrintfV( const char *format, va_list arg )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();

    char   buffer[512];

    LPSTR  szBuffer =           buffer;
    size_t iBuffer  = MAXSTRLEN(buffer);

    bool fRes = CLR_SafeSprintfV( szBuffer, iBuffer, format, arg );

    _ASSERTE(fRes);

    iBuffer = MAXSTRLEN(buffer) - iBuffer;

    Emit( buffer, (int)iBuffer );

    return (int)iBuffer;
}

int CLR_Debug::Printf( const char *format, ... )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    va_list arg;
    int     chars;

    va_start( arg, format );

    chars = CLR_Debug::PrintfV( format, arg );

    va_end( arg );

    return chars;
}

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(PLATFORM_WINDOWS)

const CLR_UINT8 c_CLR_opParamSize[] =
{
    4, // CLR_OpcodeParam_Field
    4, // CLR_OpcodeParam_Method
    4, // CLR_OpcodeParam_Type
    4, // CLR_OpcodeParam_String
    4, // CLR_OpcodeParam_Tok
    4, // CLR_OpcodeParam_Sig
    4, // CLR_OpcodeParam_BrTarget
    1, // CLR_OpcodeParam_ShortBrTarget
    4, // CLR_OpcodeParam_I
    8, // CLR_OpcodeParam_I8
    0, // CLR_OpcodeParam_None
    8, // CLR_OpcodeParam_R
    4, // CLR_OpcodeParam_Switch
    2, // CLR_OpcodeParam_Var
    1, // CLR_OpcodeParam_ShortI
    4, // CLR_OpcodeParam_ShortR
    1, // CLR_OpcodeParam_ShortVar
};

const CLR_UINT8 c_CLR_opParamSizeCompressed[] =
{
    2, // CLR_OpcodeParam_Field
    2, // CLR_OpcodeParam_Method
    2, // CLR_OpcodeParam_Type
    2, // CLR_OpcodeParam_String
    4, // CLR_OpcodeParam_Tok
    4, // CLR_OpcodeParam_Sig
    2, // CLR_OpcodeParam_BrTarget
    1, // CLR_OpcodeParam_ShortBrTarget
    4, // CLR_OpcodeParam_I
    8, // CLR_OpcodeParam_I8
    0, // CLR_OpcodeParam_None
    8, // CLR_OpcodeParam_R
    1, // CLR_OpcodeParam_Switch
    2, // CLR_OpcodeParam_Var
    1, // CLR_OpcodeParam_ShortI
    4, // CLR_OpcodeParam_ShortR
    1, // CLR_OpcodeParam_ShortVar
};

CLR_UINT32 CLR_ReadTokenCompressed( const CLR_UINT8*& ip, CLR_OPCODE opcode )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    CLR_UINT32       arg;
    const CLR_UINT8* ptr = ip;

    switch(c_CLR_RT_OpcodeLookup[ opcode ].m_opParam)
    {
        case CLR_OpcodeParam_Field : TINYCLR_READ_UNALIGNED_COMPRESSED_FIELDTOKEN ( arg, ptr ); break;
        case CLR_OpcodeParam_Method: TINYCLR_READ_UNALIGNED_COMPRESSED_METHODTOKEN( arg, ptr ); break;
        case CLR_OpcodeParam_Type  : TINYCLR_READ_UNALIGNED_COMPRESSED_TYPETOKEN  ( arg, ptr ); break;
        case CLR_OpcodeParam_String: TINYCLR_READ_UNALIGNED_COMPRESSED_STRINGTOKEN( arg, ptr ); break;
        case CLR_OpcodeParam_Tok   :
        case CLR_OpcodeParam_Sig   : TINYCLR_READ_UNALIGNED_UINT32                ( arg, ptr ); break;
        default                    : arg = 0;                                                   break;
    }

    ip = ptr;

    return arg;
}

//
// SkipBodyOfOpcode skips past the opcode argument & returns the address of the
// next instruction in the instruction stream.  Note that this is
// not necessarily the next instruction which will be executed.
//
// ip   -> address of argument for the opcode specified in
//          the instruction stream.  Note that this is not an
//          instruction boundary, it is past the opcode.
//
const CLR_UINT8* CLR_SkipBodyOfOpcode( const CLR_UINT8* ip, CLR_OPCODE opcode )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    CLR_OpcodeParam opParam = c_CLR_RT_OpcodeLookup[ opcode ].m_opParam;

    if(opParam == CLR_OpcodeParam_Switch)
    {
        CLR_UINT32 numcases; TINYCLR_READ_UNALIGNED_UINT32( numcases, ip );

        ip += numcases * sizeof(CLR_UINT32);
    }
    else
    {
        ip += c_CLR_opParamSize[ opParam ];
    }

    return ip;
}

const CLR_UINT8* CLR_SkipBodyOfOpcodeCompressed( const CLR_UINT8* ip, CLR_OPCODE opcode )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    CLR_OpcodeParam opParam = c_CLR_RT_OpcodeLookup[ opcode ].m_opParam;

    if(opParam == CLR_OpcodeParam_Switch)
    {
        CLR_UINT32 numcases; TINYCLR_READ_UNALIGNED_UINT8(numcases, ip);

        ip += numcases * sizeof(CLR_UINT16);
    }
    else
    {
        ip += c_CLR_opParamSizeCompressed[ opParam ];
    }

    return ip;
}

#endif // defined(PLATFORM_WINDOWS)

////////////////////////////////////////////////////////////////////////////////////////////////////
#define LOOKUP_ELEMENT(idx,tblName,tblNameUC) \
    const CLR_RECORD_##tblNameUC* p = Get##tblName( idx )

#define LOOKUP_ELEMENT_REF(idx,tblName,tblNameUC,tblName2) \
    const CLR_RECORD_##tblNameUC*    p = Get##tblName( idx );\
    const CLR_RT_##tblName2##_Index* s = &m_pCrossReference_##tblName[ idx ].m_target; if(s->m_data == 0) s = NULL

#define LOOKUP_ELEMENT_IDX(idx,tblName,tblNameUC) \
    const CLR_RECORD_##tblNameUC*    p = Get##tblName( idx );\
    CLR_RT_##tblName##_Index         s; s.Set( m_idx, idx )

#if defined(TINYCLR_TRACE_INSTRUCTIONS)

void CLR_RT_Assembly::DumpToken( CLR_UINT32 tk )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    CLR_UINT32 idx = CLR_DataFromTk( tk );

    switch(CLR_TypeFromTk(tk))
    {
    case TBL_AssemblyRef: { LOOKUP_ELEMENT    ( idx, AssemblyRef, ASSEMBLYREF           );                                           { CLR_Debug::Printf( "[%s]" ,                            GetString( p->name ) ); } break; }
    case TBL_TypeRef    : { LOOKUP_ELEMENT_REF( idx, TypeRef    , TYPEREF    , TypeDef  ); if(s) { CLR_RT_DUMP::TYPE  ( *s ); } else { CLR_Debug::Printf( "%s.%s", GetString( p->nameSpace ), GetString( p->name ) ); } break; }
    case TBL_FieldRef   : { LOOKUP_ELEMENT_REF( idx, FieldRef   , FIELDREF   , FieldDef ); if(s) { CLR_RT_DUMP::FIELD ( *s ); } else { CLR_Debug::Printf( "%s"   ,                            GetString( p->name ) ); } break; }
    case TBL_MethodRef  : { LOOKUP_ELEMENT_REF( idx, MethodRef  , METHODREF  , MethodDef); if(s) { CLR_RT_DUMP::METHOD( *s ); } else { CLR_Debug::Printf( "%s"   ,                            GetString( p->name ) ); } break; }
    case TBL_TypeDef    : { LOOKUP_ELEMENT_IDX( idx, TypeDef    , TYPEDEF               );         CLR_RT_DUMP::TYPE  (  s );                                                                                           break; }
    case TBL_FieldDef   : { LOOKUP_ELEMENT_IDX( idx, FieldDef   , FIELDDEF              );         CLR_RT_DUMP::FIELD (  s );                                                                                           break; }
    case TBL_MethodDef  : { LOOKUP_ELEMENT_IDX( idx, MethodDef  , METHODDEF             );         CLR_RT_DUMP::METHOD(  s );                                                                                           break; }
    case TBL_Strings    : { LPCSTR p = GetString( idx );                                                                               CLR_Debug::Printf( "'%s'" ,            p                                    );   break; }

    default:
        CLR_Debug::Printf( "[%08x]", tk );
    }
}

void CLR_RT_Assembly::DumpSignature( CLR_SIG sig )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    const CLR_UINT8* p = GetSignature( sig );
    CLR_UINT32       len;

    CLR_CorCallingConvention cc = (CLR_CorCallingConvention)*p++;

    switch(cc & PIMAGE_CEE_CS_CALLCONV_MASK)
    {
    case PIMAGE_CEE_CS_CALLCONV_FIELD:
        CLR_Debug::Printf( "FIELD " );
        DumpSignature( p );
        break;

    case PIMAGE_CEE_CS_CALLCONV_LOCAL_SIG:
        break;

    case PIMAGE_CEE_CS_CALLCONV_DEFAULT:
        len = *p++;

        CLR_Debug::Printf( "METHOD " );
        DumpSignature( p );
        CLR_Debug::Printf( "(" );

        while(len-- > 0)
        {
            CLR_Debug::Printf( " " );
            DumpSignature( p );
            if(len) CLR_Debug::Printf( "," );
            else    CLR_Debug::Printf( " " );
        }
        CLR_Debug::Printf( ")" );
        break;
    }
}

void CLR_RT_Assembly::DumpSignature( const CLR_UINT8*& p )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    CLR_DataType opt = CLR_UncompressElementType( p );

    switch(opt)
    {
        case DATATYPE_VOID      : CLR_Debug::Printf( "VOID"       );                          break;
        case DATATYPE_BOOLEAN   : CLR_Debug::Printf( "BOOLEAN"    );                          break;
        case DATATYPE_CHAR      : CLR_Debug::Printf( "CHAR"       );                          break;
        case DATATYPE_I1        : CLR_Debug::Printf( "I1"         );                          break;
        case DATATYPE_U1        : CLR_Debug::Printf( "U1"         );                          break;
        case DATATYPE_I2        : CLR_Debug::Printf( "I2"         );                          break;
        case DATATYPE_U2        : CLR_Debug::Printf( "U2"         );                          break;
        case DATATYPE_I4        : CLR_Debug::Printf( "I4"         );                          break;
        case DATATYPE_U4        : CLR_Debug::Printf( "U4"         );                          break;
        case DATATYPE_I8        : CLR_Debug::Printf( "I8"         );                          break;
        case DATATYPE_U8        : CLR_Debug::Printf( "U8"         );                          break;
        case DATATYPE_R4        : CLR_Debug::Printf( "R4"         );                          break;
        case DATATYPE_R8        : CLR_Debug::Printf( "R8"         );                          break;
        case DATATYPE_STRING    : CLR_Debug::Printf( "STRING"     );                          break;
        case DATATYPE_BYREF     : CLR_Debug::Printf( "BYREF "     ); DumpSignature     ( p ); break;
        case DATATYPE_VALUETYPE : CLR_Debug::Printf( "VALUETYPE " ); DumpSignatureToken( p ); break;
        case DATATYPE_CLASS     : CLR_Debug::Printf( "CLASS "     ); DumpSignatureToken( p ); break;
        case DATATYPE_OBJECT    : CLR_Debug::Printf( "OBJECT"     );                          break;
        case DATATYPE_SZARRAY   : CLR_Debug::Printf( "SZARRAY "   ); DumpSignature     ( p ); break;

        default                 : CLR_Debug::Printf( "[UNKNOWN: %08x]", opt );                break;
    }

}

void CLR_RT_Assembly::DumpSignatureToken( const CLR_UINT8*& p )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    CLR_UINT32 tk = CLR_TkFromStream( p );

    CLR_Debug::Printf( "[%08x]", tk );
}

//--//

void CLR_RT_Assembly::DumpOpcode( CLR_RT_StackFrame* stack, CLR_PMETADATA ip )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    if(s_CLR_RT_fTrace_Instructions < c_CLR_RT_Trace_Info) return;

    CLR_RT_MethodDef_Instance inst;

    if(s_CLR_RT_fTrace_Instructions >= c_CLR_RT_Trace_Verbose)
    {
        inst = stack->m_call;
    }
    else
    {
        inst.Clear();
    }

    DumpOpcodeDirect( inst, ip, stack->m_IPstart, stack->m_owningThread->m_pid );
}

void CLR_RT_Assembly::DumpOpcodeDirect( CLR_RT_MethodDef_Instance& call, CLR_PMETADATA ip, CLR_PMETADATA ipStart, int pid )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    CLR_Debug::Printf( "    [%04x:%04x:%08x", pid, (int)(ip - ipStart), (size_t)ip );

    if(TINYCLR_INDEX_IS_VALID(call))
    {
        CLR_Debug::Printf( ":" );
        CLR_RT_DUMP::METHOD( call );
    }

    CLR_OPCODE      op      = CLR_ReadNextOpcodeCompressed( ip );
    CLR_OpcodeParam opParam = c_CLR_RT_OpcodeLookup[ op ].m_opParam;

    CLR_Debug::Printf( "] %-12s", c_CLR_RT_OpcodeLookup[ op ].m_name );

    if(IsOpParamToken( opParam ))
    {
        DumpToken( CLR_ReadTokenCompressed( ip, op ) );
    }
    else
    {
        CLR_UINT32 argLo;
        CLR_UINT32 argHi;

        switch(c_CLR_opParamSizeCompressed[ opParam ])
        {
        case 8: TINYCLR_READ_UNALIGNED_UINT32( argLo, ip ); TINYCLR_READ_UNALIGNED_UINT32( argHi, ip ); CLR_Debug::Printf( "%08X,%08X", argHi, argLo ); break;
        case 4: TINYCLR_READ_UNALIGNED_UINT32( argLo, ip );                                             CLR_Debug::Printf( "%08X"     ,        argLo ); break;
        case 2: TINYCLR_READ_UNALIGNED_UINT16( argLo, ip );                                             CLR_Debug::Printf( "%04X"     ,        argLo ); break;
        case 1: TINYCLR_READ_UNALIGNED_UINT8 ( argLo, ip );                                             CLR_Debug::Printf( "%02X"     ,        argLo ); break;
        }
    }

    CLR_Debug::Printf( "\r\n" );
}

#endif // defined(TINYCLR_TRACE_INSTRUCTIONS)

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_TRACE_ERRORS)

void CLR_RT_DUMP::TYPE( const CLR_RT_TypeDef_Index& cls )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    char   rgBuffer[ 512 ];
    LPSTR  szBuffer = rgBuffer;
    size_t iBuffer  = MAXSTRLEN(rgBuffer);

    g_CLR_RT_TypeSystem.BuildTypeName( cls, szBuffer, iBuffer ); rgBuffer[ MAXSTRLEN(rgBuffer) ] = 0;

    CLR_Debug::Printf( "%s", rgBuffer );
}

void CLR_RT_DUMP::TYPE( const CLR_RT_ReflectionDef_Index& reflex )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    CLR_RT_TypeDef_Instance inst;
    CLR_UINT32              levels;

    if(inst.InitializeFromReflection( reflex, &levels ))
    {
        CLR_RT_DUMP::TYPE( inst );

        while(levels-- > 0)
        {
            CLR_Debug::Printf( "[]" );
        }
    }
}

void CLR_RT_DUMP::METHOD( const CLR_RT_MethodDef_Index& method )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    char   rgBuffer[ 512 ];
    LPSTR  szBuffer = rgBuffer;
    size_t iBuffer  = MAXSTRLEN(rgBuffer);

    g_CLR_RT_TypeSystem.BuildMethodName( method, szBuffer, iBuffer );

    CLR_Debug::Printf( "%s", rgBuffer );
}

void CLR_RT_DUMP::FIELD( const CLR_RT_FieldDef_Index& field )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    char   rgBuffer[ 512 ];
    LPSTR  szBuffer = rgBuffer;
    size_t iBuffer  = MAXSTRLEN(rgBuffer);

    g_CLR_RT_TypeSystem.BuildFieldName( field, szBuffer, iBuffer );

    CLR_Debug::Printf( "%s", rgBuffer );
}

void CLR_RT_DUMP::OBJECT( CLR_RT_HeapBlock* ptr, LPCSTR text )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
#define PELEMENT_TO_STRING(elem) case DATATYPE_##elem: CLR_Debug::Printf( "%s", #elem ); break

    CLR_Debug::Printf( "%s - ", text );

    while(ptr->DataType() == DATATYPE_OBJECT && ptr->Dereference())
    {
        ptr = ptr->Dereference();

        CLR_Debug::Printf( "PTR " );
    }

    CLR_Debug::Printf( "%04x blocks at %08x [%02x] ", ptr->DataSize(), (int)(size_t)ptr, ptr->DataType() );

    switch(ptr->DataType())
    {
    case DATATYPE_CLASS:
    case DATATYPE_VALUETYPE:
        {
            CLR_RT_DUMP::TYPE( ptr->ObjectCls() );
        }
        break;

    case DATATYPE_STRING:
        {
            CLR_Debug::Printf( "'%s'", ptr->StringText() );
        }
        break;

    case DATATYPE_SZARRAY:
        {
            CLR_RT_HeapBlock_Array* array = (CLR_RT_HeapBlock_Array*)ptr;

            CLR_RT_DUMP::TYPE( array->ReflectionData() );
        }
        break;

    case DATATYPE_DELEGATE_HEAD:
        {
            CLR_RT_HeapBlock_Delegate* dlg = (CLR_RT_HeapBlock_Delegate*)ptr;

            CLR_RT_DUMP::METHOD( dlg->DelegateFtn() );
        }
        break;


    PELEMENT_TO_STRING(BOOLEAN);
    PELEMENT_TO_STRING(CHAR   );
    PELEMENT_TO_STRING(I1     );
    PELEMENT_TO_STRING(U1     );
    PELEMENT_TO_STRING(I2     );
    PELEMENT_TO_STRING(U2     );
    PELEMENT_TO_STRING(I4     );
    PELEMENT_TO_STRING(U4     );
    PELEMENT_TO_STRING(I8     );
    PELEMENT_TO_STRING(U8     );
    PELEMENT_TO_STRING(R4     );
    PELEMENT_TO_STRING(R8     );

    PELEMENT_TO_STRING(FREEBLOCK             );
    PELEMENT_TO_STRING(CACHEDBLOCK           );
    PELEMENT_TO_STRING(ASSEMBLY              );
    PELEMENT_TO_STRING(WEAKCLASS             );
    PELEMENT_TO_STRING(REFLECTION            );
    PELEMENT_TO_STRING(ARRAY_BYREF           );
    PELEMENT_TO_STRING(DELEGATELIST_HEAD     );
    PELEMENT_TO_STRING(OBJECT_TO_EVENT       );
    PELEMENT_TO_STRING(BINARY_BLOB_HEAD      );

    PELEMENT_TO_STRING(THREAD                );
    PELEMENT_TO_STRING(SUBTHREAD             );
    PELEMENT_TO_STRING(STACK_FRAME           );
    PELEMENT_TO_STRING(TIMER_HEAD            );
    PELEMENT_TO_STRING(LOCK_HEAD             );
    PELEMENT_TO_STRING(LOCK_OWNER_HEAD       );
    PELEMENT_TO_STRING(LOCK_REQUEST_HEAD     );
    PELEMENT_TO_STRING(WAIT_FOR_OBJECT_HEAD  );
    PELEMENT_TO_STRING(FINALIZER_HEAD        );
    PELEMENT_TO_STRING(MEMORY_STREAM_HEAD    );
    PELEMENT_TO_STRING(MEMORY_STREAM_DATA    );

    }

    CLR_Debug::Printf( "\r\n" );

#undef PELEMENT_TO_STRING
}

#endif // defined(TINYCLR_TRACE_ERRORS)

//--//


////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_TRACE_EXCEPTIONS)

void CLR_RT_DUMP::EXCEPTION( CLR_RT_StackFrame& stack, CLR_RT_HeapBlock& ref )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    LPCSTR msg;

    CLR_RT_HeapBlock* obj = Library_corlib_native_System_Exception::GetTarget( ref ); if(!obj) return;

    CLR_Debug::Printf( "    #### Exception " ); CLR_RT_DUMP::TYPE( obj->ObjectCls() ); CLR_Debug::Printf( " - %s (%d) ####\r\n", CLR_RT_DUMP::GETERRORMESSAGE( Library_corlib_native_System_Exception::GetHResult( obj ) ), stack.m_owningThread->m_pid );

    msg = Library_corlib_native_System_Exception::GetMessage( obj );
    
    CLR_Debug::Printf( "    #### Message: %s\r\n", msg == NULL ? "" : msg );

    CLR_UINT32                                          depth;
    Library_corlib_native_System_Exception::StackTrace* stackTrace = Library_corlib_native_System_Exception::GetStackTrace( obj, depth ); if(!stackTrace) return;

    while(depth-- > 0)
    {
        CLR_Debug::Printf( "    #### " ); CLR_RT_DUMP::METHOD( stackTrace->m_md ); CLR_Debug::Printf( " [IP: %04x] ####\r\n", stackTrace->m_IP );

        stackTrace++;
    }
}

void CLR_RT_DUMP::POST_PROCESS_EXCEPTION( CLR_RT_HeapBlock& ref )
{
    // socket exceptions have an extra field (ErrorCode), so lets display that as well
    if (CLR_RT_ExecutionEngine::IsInstanceOf( ref, g_CLR_RT_WellKnownTypes.m_SocketException ))
    {
        CLR_RT_HeapBlock* obj = ref.Dereference();
        if(obj != NULL)
        {
            CLR_INT32 errorCode = obj[ Library_system_sockets_System_Net_Sockets_SocketException::FIELD___errorCode ].NumericByRef().s4;
            CLR_Debug::Printf( "    #### SocketException ErrorCode = %d\r\n", errorCode ); 
        }
    }
    else if(CLR_RT_ExecutionEngine::IsInstanceOf( ref, g_CLR_RT_WellKnownTypes.m_CryptoException ))
    {
        CLR_RT_HeapBlock* obj = ref.Dereference();
        if(obj != NULL)
        {
            // m_errorCode field 
            CLR_INT32 errorCode = obj[5].NumericByRef().s4;
            CLR_Debug::Printf( "    #### CryptoException ErrorCode = %d\r\n", errorCode ); 
        }
    }
}

LPCSTR CLR_RT_DUMP::GETERRORMESSAGE( HRESULT hrError )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
#define CASE_HRESULT_TO_STRING(hr) case hr: return #hr
    switch(hrError)
    {
        CASE_HRESULT_TO_STRING(CLR_E_UNKNOWN_INSTRUCTION);
        CASE_HRESULT_TO_STRING(CLR_E_UNSUPPORTED_INSTRUCTION);
        CASE_HRESULT_TO_STRING(CLR_E_STACK_OVERFLOW);
        CASE_HRESULT_TO_STRING(CLR_E_STACK_UNDERFLOW);
        CASE_HRESULT_TO_STRING(CLR_E_ENTRY_NOT_FOUND);
        CASE_HRESULT_TO_STRING(CLR_E_ASSM_WRONG_CHECKSUM);
        CASE_HRESULT_TO_STRING(CLR_E_ASSM_PATCHING_NOT_SUPPORTED);
        CASE_HRESULT_TO_STRING(CLR_E_SHUTTING_DOWN);
        CASE_HRESULT_TO_STRING(CLR_E_OBJECT_DISPOSED);
        CASE_HRESULT_TO_STRING(CLR_E_WATCHDOG_TIMEOUT);
        CASE_HRESULT_TO_STRING(CLR_E_NULL_REFERENCE);
        CASE_HRESULT_TO_STRING(CLR_E_WRONG_TYPE);
        CASE_HRESULT_TO_STRING(CLR_E_TYPE_UNAVAILABLE);
        CASE_HRESULT_TO_STRING(CLR_E_INVALID_CAST);
        CASE_HRESULT_TO_STRING(CLR_E_OUT_OF_RANGE);
        CASE_HRESULT_TO_STRING(CLR_E_SERIALIZATION_VIOLATION);
        CASE_HRESULT_TO_STRING(CLR_E_SERIALIZATION_BADSTREAM);
        CASE_HRESULT_TO_STRING(CLR_E_DIVIDE_BY_ZERO);
        CASE_HRESULT_TO_STRING(CLR_E_BUSY);
        CASE_HRESULT_TO_STRING(CLR_E_PROCESS_EXCEPTION);
        CASE_HRESULT_TO_STRING(CLR_E_THREAD_WAITING);
        CASE_HRESULT_TO_STRING(CLR_E_LOCK_SYNCHRONIZATION_EXCEPTION);
        CASE_HRESULT_TO_STRING(CLR_E_APPDOMAIN_EXITED);
        CASE_HRESULT_TO_STRING(CLR_E_APPDOMAIN_MARSHAL_EXCEPTION);
        CASE_HRESULT_TO_STRING(CLR_E_UNKNOWN_TYPE);
        CASE_HRESULT_TO_STRING(CLR_E_ARGUMENT_NULL);
        CASE_HRESULT_TO_STRING(CLR_E_IO);
        CASE_HRESULT_TO_STRING(CLR_E_ENTRYPOINT_NOT_FOUND);
        CASE_HRESULT_TO_STRING(CLR_E_DRIVER_NOT_REGISTERED);
        CASE_HRESULT_TO_STRING(CLR_E_PIN_UNAVAILABLE);
        CASE_HRESULT_TO_STRING(CLR_E_PIN_DEAD);
        CASE_HRESULT_TO_STRING(CLR_E_INVALID_OPERATION);
        CASE_HRESULT_TO_STRING(CLR_E_WRONG_INTERRUPT_TYPE);
        CASE_HRESULT_TO_STRING(CLR_E_NO_INTERRUPT);
        CASE_HRESULT_TO_STRING(CLR_E_DISPATCHER_ACTIVE);
        CASE_HRESULT_TO_STRING(CLR_E_FILE_IO);
        CASE_HRESULT_TO_STRING(CLR_E_INVALID_DRIVER);
        CASE_HRESULT_TO_STRING(CLR_E_FILE_NOT_FOUND);
        CASE_HRESULT_TO_STRING(CLR_E_DIRECTORY_NOT_FOUND);
        CASE_HRESULT_TO_STRING(CLR_E_VOLUME_NOT_FOUND);
        CASE_HRESULT_TO_STRING(CLR_E_PATH_TOO_LONG);
        CASE_HRESULT_TO_STRING(CLR_E_DIRECTORY_NOT_EMPTY);
        CASE_HRESULT_TO_STRING(CLR_E_UNAUTHORIZED_ACCESS);
        CASE_HRESULT_TO_STRING(CLR_E_PATH_ALREADY_EXISTS);
        CASE_HRESULT_TO_STRING(CLR_E_TOO_MANY_OPEN_HANDLES);
        CASE_HRESULT_TO_STRING(CLR_E_NOT_SUPPORTED);
        CASE_HRESULT_TO_STRING(CLR_E_RESCHEDULE);
        CASE_HRESULT_TO_STRING(CLR_E_OUT_OF_MEMORY);
        CASE_HRESULT_TO_STRING(CLR_E_RESTART_EXECUTION);
        CASE_HRESULT_TO_STRING(CLR_E_TIMEOUT);
        CASE_HRESULT_TO_STRING(CLR_E_FAIL);
        //--//
        CASE_HRESULT_TO_STRING(CLR_S_THREAD_EXITED);
        CASE_HRESULT_TO_STRING(CLR_S_QUANTUM_EXPIRED);
        CASE_HRESULT_TO_STRING(CLR_S_NO_READY_THREADS);
        CASE_HRESULT_TO_STRING(CLR_S_NO_THREADS);
        CASE_HRESULT_TO_STRING(CLR_S_RESTART_EXECUTION);
    }
#undef CASE_HRESULT_TO_STRING

    static char s_tmp[ 32 ];

    hal_snprintf( s_tmp, MAXSTRLEN(s_tmp), "0x%08x", hrError );

    return s_tmp;
}

#endif // defined(TINYCLR_TRACE_EXCEPTIONS)

//--//

