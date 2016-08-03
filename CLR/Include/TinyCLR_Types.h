////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// FIXME GJS - need to consolidate the places where the 16/32/64 but loads occur and endian handle them once
// Where is the data stream that these get pulled from? Shouldn't it be possible to uncompress->recompress the stream in MDP when the Be PE is generated?
#ifndef _TINYCLR_TYPES_H_
#define _TINYCLR_TYPES_H_

#include <TinyClr_PlatformDef.h>

///////////////////////////////////////////////////////////////////////////////////////////////////

#define FAULT_ON_NULL_HR(ptr,hr) if(!(ptr)                             ) TINYCLR_SET_AND_LEAVE((hr)               )
#define CHECK_ALLOCATION(ptr)    if(!(ptr)                             ) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_MEMORY)
#define CHECK_SIZE_RANGE(len)    if( len > CLR_RT_HeapBlock::HB_MaxSize) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE )

#define FIMPLIES(x,y) ((!(x)) || (y))
#define FINRANGE(val,lo,hi) (((val) >= (lo)) && ((val) <= (hi)))

///////////////////////////////////////////////////////////////////////////////////////////////////

#define TINYCLR_INTEROP_STUB_RETURN(stack) return stack.NotImplementedStub()
#define TINYCLR_SYSTEM_STUB_RETURN()       return S_OK
#define TINYCLR_FEATURE_STUB_RETURN()      return CLR_E_NOTIMPL

//////////////////////////////////////////////////

#define TINYCLR_PARAMCHECK_BEGIN()                   \
{                                                    \
    HRESULT hrInner = S_OK;

#define TINYCLR_PARAMCHECK_POINTER(ptr)              \
    {                                                \
        if(ptr == NULL)                              \
        {                                            \
            hrInner = CLR_E_NULL_REFERENCE;          \
        }                                            \
    }

#define TINYCLR_PARAMCHECK_POINTER_AND_SET(ptr,val)  \
    {                                                \
        if(ptr == NULL)                              \
        {                                            \
            hrInner = CLR_E_NULL_REFERENCE;          \
        }                                            \
        else                                         \
        {                                            \
            *ptr = val;                              \
        }                                            \
    }

#define TINYCLR_PARAMCHECK_NOTNULL(ptr)              \
    {                                                \
        if(ptr == NULL)                              \
        {                                            \
            hrInner = CLR_E_INVALID_PARAMETER;        \
        }                                            \
    }

#define TINYCLR_PARAMCHECK_STRING_NOT_EMPTY(ptr)     \
    {                                                \
        if(ptr == NULL || ptr[ 0 ] == 0)               \
        {                                            \
            hrInner = CLR_E_INVALID_PARAMETER;        \
        }                                            \
    }

#define TINYCLR_PARAMCHECK_END()                     \
    { TINYCLR_CHECK_HRESULT(hrInner); }              \
}


///////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(_MSC_VER)
#pragma pack(push, TINYCLR_TYPES_H, 4)
#endif

enum CLR_OPCODE
{
#define OPDEF(c,s,pop,push,args,type,l,s1,s2,ctrl) c,
#include <opcode.def>
#undef OPDEF
CEE_COUNT,        /* number of instructions and macros pre-defined */
};

#if defined(__ADSPBLACKFIN__) || defined (__GNUC__) || defined(_ARC) || defined(__RENESAS__)
#define __int64 long long
#endif

typedef unsigned char    CLR_UINT8;
typedef unsigned short   CLR_UINT16;
typedef unsigned int     CLR_UINT32;
typedef unsigned __int64 CLR_UINT64;
typedef signed char      CLR_INT8;
typedef signed short     CLR_INT16;
typedef signed int       CLR_INT32;
typedef signed __int64   CLR_INT64;

typedef CLR_UINT16       CLR_OFFSET;
typedef CLR_UINT32       CLR_OFFSET_LONG;
typedef CLR_UINT16       CLR_IDX;
typedef CLR_UINT16       CLR_STRING;
typedef CLR_UINT16       CLR_SIG;
typedef const CLR_UINT8* CLR_PMETADATA;


//--//
//may need to change later
typedef CLR_INT64        CLR_INT64_TEMP_CAST;
typedef CLR_UINT64       CLR_UINT64_TEMP_CAST;

#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
typedef double           CLR_DOUBLE_TEMP_CAST;
#else
typedef CLR_INT64        CLR_DOUBLE_TEMP_CAST;
#endif 

#define CLR_SIG_INVALID  0xFFFF

enum CLR_LOGICAL_OPCODE
{
    LO_Not                       = 0x00,
    LO_And                       = 0x01,
    LO_Or                        = 0x02,
    LO_Xor                       = 0x03,
    LO_Shl                       = 0x04,
    LO_Shr                       = 0x05,

    LO_Neg                       = 0x06,
    LO_Add                       = 0x07,
    LO_Sub                       = 0x08,
    LO_Mul                       = 0x09,
    LO_Div                       = 0x0A,
    LO_Rem                       = 0x0B,

    LO_Box                       = 0x0C,
    LO_Unbox                     = 0x0D,

    LO_Branch                    = 0x0E,
    LO_Set                       = 0x0F,
    LO_Switch                    = 0x10,

    LO_LoadFunction              = 0x11,
    LO_LoadVirtFunction          = 0x12,

    LO_Call                      = 0x13,
    LO_CallVirt                  = 0x14,
    LO_Ret                       = 0x15,

    LO_NewObject                 = 0x16,
    LO_CastClass                 = 0x17,
    LO_IsInst                    = 0x18,

    LO_Dup                       = 0x19,
    LO_Pop                       = 0x1A,

    LO_Throw                     = 0x1B,
    LO_Rethrow                   = 0x1C,
    LO_Leave                     = 0x1D,
    LO_EndFinally                = 0x1E,

    LO_Convert                   = 0x1F,

    LO_StoreArgument             = 0x20,
    LO_LoadArgument              = 0x21,
    LO_LoadArgumentAddress       = 0x22,

    LO_StoreLocal                = 0x23,
    LO_LoadLocal                 = 0x24,
    LO_LoadLocalAddress          = 0x25,

    LO_LoadConstant_I4           = 0x26,
    LO_LoadConstant_I8           = 0x27,
    LO_LoadConstant_R4           = 0x28,
    LO_LoadConstant_R8           = 0x29,

    LO_LoadNull                  = 0x2A,
    LO_LoadString                = 0x2B,
    LO_LoadToken                 = 0x2C,

    LO_NewArray                  = 0x2D,
    LO_LoadLength                = 0x2E,

    LO_StoreElement              = 0x2F,
    LO_LoadElement               = 0x30,
    LO_LoadElementAddress        = 0x31,

    LO_StoreField                = 0x32,
    LO_LoadField                 = 0x33,
    LO_LoadFieldAddress          = 0x34,

    LO_StoreStaticField          = 0x35,
    LO_LoadStaticField           = 0x36,
    LO_LoadStaticFieldAddress    = 0x37,

    LO_StoreIndirect             = 0x38,
    LO_LoadIndirect              = 0x39,

    LO_InitObject                = 0x3A,
    LO_LoadObject                = 0x3B,
    LO_CopyObject                = 0x3C,
    LO_StoreObject               = 0x3D,

    LO_Nop                       = 0x3E,

    LO_EndFilter                 = 0x3F,

    LO_Unsupported               = 0x40,

    LO_FIRST                     = LO_Not,
    LO_LAST                      = LO_EndFilter,
};

///////////////////////////////////////////////////////////////////////////////////////////////////

static const CLR_IDX    CLR_EmptyIndex                   = 0xFFFF;
static const CLR_UINT32 CLR_EmptyToken                   = 0xFFFFFFFF;
static const size_t     CLR_MaxStreamSize_AssemblyRef    = 0x0000FFFF;
static const size_t     CLR_MaxStreamSize_TypeRef        = 0x0000FFFF;
static const size_t     CLR_MaxStreamSize_FieldRef       = 0x0000FFFF;
static const size_t     CLR_MaxStreamSize_MethodRef      = 0x0000FFFF;
static const size_t     CLR_MaxStreamSize_TypeDef        = 0x0000FFFF;
static const size_t     CLR_MaxStreamSize_FieldDef       = 0x0000FFFF;
static const size_t     CLR_MaxStreamSize_MethodDef      = 0x0000FFFF;
static const size_t     CLR_MaxStreamSize_Attributes     = 0x0000FFFF;
static const size_t     CLR_MaxStreamSize_TypeSpec       = 0x0000FFFF;
static const size_t     CLR_MaxStreamSize_Resources      = 0x0000FFFF;
static const size_t     CLR_MaxStreamSize_ResourcesData  = 0xFFFFFFFF;
static const size_t     CLR_MaxStreamSize_Strings        = 0x0000FFFF;
static const size_t     CLR_MaxStreamSize_Signatures     = 0x0000FFFF;
static const size_t     CLR_MaxStreamSize_ByteCode       = 0x0000FFFF;
static const size_t     CLR_MaxStreamSize_ResourcesFiles = 0x0000FFFF;

enum CLR_OpcodeParam
{
    CLR_OpcodeParam_Field         =  0,
    CLR_OpcodeParam_Method        =  1,
    CLR_OpcodeParam_Type          =  2,
    CLR_OpcodeParam_String        =  3,
    CLR_OpcodeParam_Tok           =  4,
    CLR_OpcodeParam_Sig           =  5,
    CLR_OpcodeParam_BrTarget      =  6,
    CLR_OpcodeParam_ShortBrTarget =  7,
    CLR_OpcodeParam_I             =  8,
    CLR_OpcodeParam_I8            =  9,
    CLR_OpcodeParam_None          = 10,
    CLR_OpcodeParam_R             = 11,
    CLR_OpcodeParam_Switch        = 12,
    CLR_OpcodeParam_Var           = 13,
    CLR_OpcodeParam_ShortI        = 14,
    CLR_OpcodeParam_ShortR        = 15,
    CLR_OpcodeParam_ShortVar      = 16,
};

#define CanCompressOpParamToken(opParam) (opParam >= CLR_OpcodeParam_Field && opParam <= CLR_OpcodeParam_String)
#define IsOpParamToken(opParam) (opParam >= CLR_OpcodeParam_Field && opParam <= CLR_OpcodeParam_Sig)

//--//

enum CLR_FlowControl
{
    CLR_FlowControl_NEXT        = 0,
    CLR_FlowControl_CALL        = 1,
    CLR_FlowControl_RETURN      = 2,
    CLR_FlowControl_BRANCH      = 3,
    CLR_FlowControl_COND_BRANCH = 4,
    CLR_FlowControl_THROW       = 5,
    CLR_FlowControl_BREAK       = 6,
    CLR_FlowControl_META        = 7,
};

//--//

#define c_CLR_StringTable_Version 1

///////////////////////////////////////////////////////////////////////////////////////////////////

enum CLR_TABLESENUM
{
    TBL_AssemblyRef    = 0x00000000,
    TBL_TypeRef        = 0x00000001,
    TBL_FieldRef       = 0x00000002,
    TBL_MethodRef      = 0x00000003,
    TBL_TypeDef        = 0x00000004,
    TBL_FieldDef       = 0x00000005,
    TBL_MethodDef      = 0x00000006,
    TBL_Attributes     = 0x00000007,
    TBL_TypeSpec       = 0x00000008,
    TBL_Resources      = 0x00000009,
    TBL_ResourcesData  = 0x0000000A,
    TBL_Strings        = 0x0000000B,
    TBL_Signatures     = 0x0000000C,
    TBL_ByteCode       = 0x0000000D,
    TBL_ResourcesFiles = 0x0000000E,
    TBL_EndOfAssembly  = 0x0000000F,
    TBL_Max            = 0x00000010,        
};

enum CLR_CorCallingConvention
{
    /////////////////////////////////////////////////////////////////////////////////////////////
    //
    // This is based on CorCallingConvention.
    //
    PIMAGE_CEE_CS_CALLCONV_DEFAULT       = 0x0,

    PIMAGE_CEE_CS_CALLCONV_VARARG        = 0x5,
    PIMAGE_CEE_CS_CALLCONV_FIELD         = 0x6,
    PIMAGE_CEE_CS_CALLCONV_LOCAL_SIG     = 0x7,
    PIMAGE_CEE_CS_CALLCONV_PROPERTY      = 0x8,
    PIMAGE_CEE_CS_CALLCONV_UNMGD         = 0x9,
    PIMAGE_CEE_CS_CALLCONV_GENERICINST   = 0xa,  // generic method instantiation
    PIMAGE_CEE_CS_CALLCONV_NATIVEVARARG  = 0xb,  // used ONLY for 64bit vararg PInvoke calls
    PIMAGE_CEE_CS_CALLCONV_MAX           = 0xc,  // first invalid calling convention
        
    // The high bits of the calling convention convey additional info
    PIMAGE_CEE_CS_CALLCONV_MASK          = 0x0f, // Calling convention is bottom 4 bits
    PIMAGE_CEE_CS_CALLCONV_HASTHIS       = 0x20, // Top bit indicates a 'this' parameter
    PIMAGE_CEE_CS_CALLCONV_EXPLICITTHIS  = 0x40, // This parameter is explicitly in the signature
    PIMAGE_CEE_CS_CALLCONV_GENERIC       = 0x10, // Generic method sig with explicit number of type arguments (precedes ordinary parameter count)
    //
    // End of overlap with CorCallingConvention.
    //
    /////////////////////////////////////////////////////////////////////////////////////////////
};

enum CLR_DataType // KEEP IN SYNC WITH Microsoft.SPOT.DataType!!
{
    DATATYPE_VOID                       , // 0 bytes

    DATATYPE_BOOLEAN                    , // 1 byte
    DATATYPE_I1                         , // 1 byte
    DATATYPE_U1                         , // 1 byte

    DATATYPE_CHAR                       , // 2 bytes
    DATATYPE_I2                         , // 2 bytes
    DATATYPE_U2                         , // 2 bytes

    DATATYPE_I4                         , // 4 bytes
    DATATYPE_U4                         , // 4 bytes
    DATATYPE_R4                         , // 4 bytes

    DATATYPE_I8                         , // 8 bytes
    DATATYPE_U8                         , // 8 bytes
    DATATYPE_R8                         , // 8 bytes
    DATATYPE_DATETIME                   , // 8 bytes     // Shortcut for System.DateTime
    DATATYPE_TIMESPAN                   , // 8 bytes     // Shortcut for System.TimeSpan
    DATATYPE_STRING                     ,

    DATATYPE_LAST_NONPOINTER            = DATATYPE_TIMESPAN, // This is the last type that doesn't need to be relocated.
    DATATYPE_LAST_PRIMITIVE_TO_PRESERVE = DATATYPE_R8      , // All the above types don't need fix-up on assignment.
#if defined(TINYCLR_NO_ASSEMBLY_STRINGS)    
    DATATYPE_LAST_PRIMITIVE_TO_MARSHAL  = DATATYPE_STRING,   // All the above types can be marshaled by assignment.
#else    
    DATATYPE_LAST_PRIMITIVE_TO_MARSHAL  = DATATYPE_TIMESPAN, // All the above types can be marshaled by assignment.
#endif
    DATATYPE_LAST_PRIMITIVE             = DATATYPE_STRING  , // All the above types don't need fix-up on assignment.

    DATATYPE_OBJECT                     , // Shortcut for System.Object
    DATATYPE_CLASS                      , // CLASS <class Token>
    DATATYPE_VALUETYPE                  , // VALUETYPE <class Token>
    DATATYPE_SZARRAY                    , // Shortcut for single dimension zero lower bound array SZARRAY <type>
    DATATYPE_BYREF                      , // BYREF <type>

    ////////////////////////////////////////

    DATATYPE_FREEBLOCK                  ,
    DATATYPE_CACHEDBLOCK                ,
    DATATYPE_ASSEMBLY                   ,
    DATATYPE_WEAKCLASS                  ,
    DATATYPE_REFLECTION                 ,
    DATATYPE_ARRAY_BYREF                ,
    DATATYPE_DELEGATE_HEAD              ,
    DATATYPE_DELEGATELIST_HEAD          ,
    DATATYPE_OBJECT_TO_EVENT            ,
    DATATYPE_BINARY_BLOB_HEAD           ,

    DATATYPE_THREAD                     ,
    DATATYPE_SUBTHREAD                  ,
    DATATYPE_STACK_FRAME                ,
    DATATYPE_TIMER_HEAD                 ,
    DATATYPE_LOCK_HEAD                  ,
    DATATYPE_LOCK_OWNER_HEAD            ,
    DATATYPE_LOCK_REQUEST_HEAD          ,
    DATATYPE_WAIT_FOR_OBJECT_HEAD       ,
    DATATYPE_FINALIZER_HEAD             ,
    DATATYPE_MEMORY_STREAM_HEAD         , // SubDataType?
    DATATYPE_MEMORY_STREAM_DATA         , // SubDataType?

    DATATYPE_SERIALIZER_HEAD            , // SubDataType?
    DATATYPE_SERIALIZER_DUPLICATE       , // SubDataType?
    DATATYPE_SERIALIZER_STATE           , // SubDataType?

    DATATYPE_ENDPOINT_HEAD              ,

    //These constants are shared by Debugger.dll, and cannot be conditionally compiled away.
    //This adds a couple extra bytes to the lookup table.  But frankly, the lookup table should probably 
    //be shrunk to begin with.  Most of the datatypes are used just to tag memory.
    //For those datatypes, perhaps we should use a subDataType instead (probably what the comments above are about).

    DATATYPE_RADIO_LAST                 = DATATYPE_ENDPOINT_HEAD + 3,

    DATATYPE_IO_PORT                    ,
    DATATYPE_IO_PORT_LAST               = DATATYPE_RADIO_LAST + 1,

    DATATYPE_VTU_PORT_LAST              = DATATYPE_IO_PORT_LAST + 1,

    DATATYPE_I2C_XACTION                ,
    DATATYPE_I2C_XACTION_LAST           = DATATYPE_VTU_PORT_LAST + 1,

#if defined(TINYCLR_APPDOMAINS)
    DATATYPE_APPDOMAIN_HEAD             ,
    DATATYPE_TRANSPARENT_PROXY          ,
    DATATYPE_APPDOMAIN_ASSEMBLY         ,
#endif
    DATATYPE_APPDOMAIN_LAST             = DATATYPE_I2C_XACTION_LAST + 3,

    DATATYPE_FIRST_INVALID              ,

    // Type modifies. This is exact copy of VALUES ELEMENT_TYPE_* from CorHdr.h
    // 
    
    DATATYPE_TYPE_MODIFIER       = 0x40,
    DATATYPE_TYPE_SENTINEL       = 0x01 | DATATYPE_TYPE_MODIFIER, // sentinel for varargs
    DATATYPE_TYPE_PINNED         = 0x05 | DATATYPE_TYPE_MODIFIER,
    DATATYPE_TYPE_R4_HFA         = 0x06 | DATATYPE_TYPE_MODIFIER, // used only internally for R4 HFA types
    DATATYPE_TYPE_R8_HFA         = 0x07 | DATATYPE_TYPE_MODIFIER, // used only internally for R8 HFA types
};

enum CLR_ReflectionType
{
    REFLECTION_INVALID      = 0x00,
    REFLECTION_ASSEMBLY     = 0x01,
    REFLECTION_TYPE         = 0x02,
    REFLECTION_TYPE_DELAYED = 0x03,
    REFLECTION_CONSTRUCTOR  = 0x04,
    REFLECTION_METHOD       = 0x05,
    REFLECTION_FIELD        = 0x06,
};

////////////////////////////////////////////////////////////////////////////////////////////////////
inline CLR_UINT32     CLR_DataFromTk( CLR_UINT32 tk ) { return                  tk & 0x00FFFFFF; }
inline CLR_TABLESENUM CLR_TypeFromTk( CLR_UINT32 tk ) { return (CLR_TABLESENUM)(tk >> 24);       }
inline CLR_UINT32     CLR_TkFromType( CLR_TABLESENUM tbl, CLR_UINT32 data ) { return ((((CLR_UINT32)tbl) << 24) & 0xFF000000) | (data & 0x00FFFFFF); }
#if 0
// Used on LE host to target BE
inline CLR_UINT32     CLR_TkFromType( CLR_TABLESENUM tbl, CLR_UINT32 data ) { return ( ((CLR_UINT32)(tbl) & 0xFF) | (data & 0xFFFFFF00)); }
inline CLR_UINT32     CLR_DataFromTk( CLR_UINT32 tk ) { return                  tk & 0xFFFFFF00; }
inline CLR_TABLESENUM CLR_TypeFromTk( CLR_UINT32 tk ) { return (CLR_TABLESENUM)(tk&0xFF);        }
#endif
//--//

inline CLR_UINT32 CLR_UncompressStringToken( CLR_UINT32 tk )
{
    return CLR_TkFromType( TBL_Strings, tk );   
}

inline CLR_UINT32 CLR_UncompressTypeToken( CLR_UINT32 tk )
{
    static const CLR_TABLESENUM c_lookup[3] = { TBL_TypeDef, TBL_TypeRef, TBL_TypeSpec };
    return CLR_TkFromType( c_lookup[(tk >> 14) & 3], 0x3fff & tk );
}

inline CLR_UINT32 CLR_UncompressFieldToken( CLR_UINT32 tk )
{
    static const CLR_TABLESENUM c_lookup[2] = { TBL_FieldDef, TBL_FieldRef };
    return CLR_TkFromType( c_lookup[(tk >> 15) & 1], 0x7fff & tk );
}

inline CLR_UINT32 CLR_UncompressMethodToken( CLR_UINT32 tk )
{
    static const CLR_TABLESENUM c_lookup[ 2 ] = { TBL_MethodDef, TBL_MethodRef };
    return CLR_TkFromType( c_lookup[ (tk >> 15) & 1 ], 0x7fff & tk );
}

#if defined(TINYCLR_JITTER) || defined(_WIN32)

CLR_UINT32 CLR_ReadTokenCompressed( CLR_PMETADATA& ip, CLR_OPCODE opcode );

#endif

//--//

HRESULT CLR_CompressTokenHelper( const CLR_TABLESENUM *tables, CLR_UINT16 cTables, CLR_UINT32& tk, bool fBigEndian );

inline HRESULT CLR_CompressStringToken( CLR_UINT32& tk, bool fBigEndian )
{
   static const CLR_TABLESENUM c_lookup[ 1 ] = { TBL_Strings };

   return CLR_CompressTokenHelper( c_lookup, ARRAYSIZE(c_lookup), tk, fBigEndian );
}

inline HRESULT CLR_CompressTypeToken( CLR_UINT32& tk, bool fBigEndian )
{
    static const CLR_TABLESENUM c_lookup[ 3 ] = { TBL_TypeDef, TBL_TypeRef, TBL_TypeSpec };

    return CLR_CompressTokenHelper( c_lookup, ARRAYSIZE(c_lookup), tk, fBigEndian );
}

inline HRESULT CLR_CompressFieldToken( CLR_UINT32& tk, bool fBigEndian )
{
    static const CLR_TABLESENUM c_lookup[ 2 ] = { TBL_FieldDef, TBL_FieldRef };

    return CLR_CompressTokenHelper( c_lookup, ARRAYSIZE(c_lookup), tk, fBigEndian );
}

inline HRESULT CLR_CompressMethodToken( CLR_UINT32& tk, bool fBigEndian )
{
    static const CLR_TABLESENUM c_lookup[ 2 ] = { TBL_MethodDef, TBL_MethodRef };

    return CLR_CompressTokenHelper( c_lookup, ARRAYSIZE(c_lookup), tk, fBigEndian );
}

//--//

inline bool CLR_CompressData( CLR_UINT32 val, CLR_UINT8*& p )
{
    CLR_UINT8* ptr = p;

    if(val <= 0x7F)
    {
        *ptr++ = (CLR_UINT8)(val);
    }
    else if(val <= 0x3FFF)
    {
        *ptr++ = (CLR_UINT8)((val >> 8) | 0x80);
        *ptr++ = (CLR_UINT8)((val     )       );
    }
    else if(val <= 0x1FFFFFFF)
    {
        *ptr++ = (CLR_UINT8)((val >> 24) | 0xC0);
        *ptr++ = (CLR_UINT8)((val >> 16)       );
        *ptr++ = (CLR_UINT8)((val >>  8)       );
        *ptr++ = (CLR_UINT8)((val      )       );
    }
    else
    {
        return false;
    }

    p = ptr;

    return true;
}

inline CLR_UINT32 CLR_UncompressData( const CLR_UINT8*& p )
{
    CLR_PMETADATA ptr = p;
    CLR_UINT32    val = *ptr++;
    // Handle smallest data inline.
    if((val & 0x80) == 0x00)        // 0??? ????
    {
    }
    else if((val & 0xC0) == 0x80)  // 10?? ????
    {
        val  =             (val & 0x3F) << 8;
        val |= (CLR_UINT32)*ptr++           ;
    }
    else // 110? ????
    {
        val  =             (val & 0x1F) << 24;
        val |= (CLR_UINT32)*ptr++       << 16;
        val |= (CLR_UINT32)*ptr++       <<  8;
        val |= (CLR_UINT32)*ptr++       <<  0;
    }
#if 0
    // Handle smallest data inline.
    if((val & 0x80) == 0x00)        // 0??? ????
    {
    }
    else if((val & 0xC0) == 0x80)  // 10?? ????
    {
        val  =             (val & 0x3F);
        val |= ((CLR_UINT32)*ptr++ <<8);
    }
    else // 110? ????
    {
        val  =             (val & 0x1F)       ;
        val |= (CLR_UINT32)*ptr++       <<   8;
        val |= (CLR_UINT32)*ptr++       <<  16;
        val |= (CLR_UINT32)*ptr++       <<  24;
    }

#endif

    p = ptr;

    return val;
}

inline CLR_DataType CLR_UncompressElementType( const CLR_UINT8*& p )
{
    return (CLR_DataType)*p++;
}

inline CLR_UINT32 CLR_TkFromStream( const CLR_UINT8*& p )
{
    static const CLR_TABLESENUM c_lookup[ 4 ] = { TBL_TypeDef, TBL_TypeRef, TBL_TypeSpec, TBL_Max };

    CLR_UINT32 data = CLR_UncompressData( p );

    return CLR_TkFromType( c_lookup[ data & 3 ], data >> 2 );
}

//--//--//--//

#if defined(PLATFORM_BLACKFIN) || defined(__GNUC__) || defined(__RENESAS__)

#define TINYCLR_READ_UNALIGNED_UINT8(arg,ip)  arg = *(const CLR_UINT8 *)ip; ip += sizeof(CLR_UINT8 )
template<typename T> __inline void TINYCLR_READ_UNALIGNED_UINT16(T& arg, CLR_PMETADATA& ip) 
{
#if !defined(NETMF_TARGET_BIG_ENDIAN)
    arg  = (CLR_UINT16)(*(const CLR_UINT8 *)ip)     ; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT16)(*(const CLR_UINT8 *)ip) << 8; ip += sizeof(CLR_UINT8);
#else
    arg  = (CLR_UINT16)(*(const CLR_UINT8 *)ip) << 8    ; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT16)(*(const CLR_UINT8 *)ip)         ; ip += sizeof(CLR_UINT8);
#endif
}
template<typename T> __inline void TINYCLR_READ_UNALIGNED_UINT32(T& arg, CLR_PMETADATA& ip) 
{
#if !defined(NETMF_TARGET_BIG_ENDIAN)
    arg  = (CLR_UINT32)(*(const CLR_UINT8 *)ip)      ; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT32)(*(const CLR_UINT8 *)ip) <<  8; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT32)(*(const CLR_UINT8 *)ip) << 16; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT32)(*(const CLR_UINT8 *)ip) << 24; ip += sizeof(CLR_UINT8);
#else
    arg  = (CLR_UINT32)(*(const CLR_UINT8 *)ip) << 24; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT32)(*(const CLR_UINT8 *)ip) << 16; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT32)(*(const CLR_UINT8 *)ip) <<  8; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT32)(*(const CLR_UINT8 *)ip)      ; ip += sizeof(CLR_UINT8);
#endif //NETMF_TARGET_BIG_ENDIAN
}
template<typename T> __inline void TINYCLR_READ_UNALIGNED_UINT64(T& arg, CLR_PMETADATA& ip) 
{
#if !defined(NETMF_TARGET_BIG_ENDIAN)
    arg  = (CLR_UINT64)(*(const CLR_UINT8 *)ip)      ; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) <<  8; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 16; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 24; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 32; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 40; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 48; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 56; ip += sizeof(CLR_UINT8);
#else
    arg  = (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 56; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 48; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 40; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 32; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 24; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 16; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) <<  8; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip)      ; ip += sizeof(CLR_UINT8);
#endif //NETMF_TARGET_BIG_ENDIAN
}

#define TINYCLR_READ_UNALIGNED_INT8(arg,ip)   arg = *(const CLR_INT8 * )ip; ip += sizeof(CLR_INT8  )
template<typename T> __inline void TINYCLR_READ_UNALIGNED_INT16(T& arg, CLR_PMETADATA& ip) 
{
#if !defined(NETMF_TARGET_BIG_ENDIAN)
    arg  = (CLR_UINT16)(*(const CLR_UINT8 *)ip)      ; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT16)(*(const CLR_UINT8 *)ip) <<  8; ip += sizeof(CLR_UINT8);
#else
    arg  = (CLR_UINT16)(*(const CLR_UINT8 *)ip) <<  8; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT16)(*(const CLR_UINT8 *)ip)      ; ip += sizeof(CLR_UINT8);
#endif //NETMF_TARGET_BIG_ENDIAN
    arg  = (CLR_INT16)arg;
}
template<typename T> __inline void TINYCLR_READ_UNALIGNED_INT32(T& arg, CLR_PMETADATA& ip) 
{
#if !defined(NETMF_TARGET_BIG_ENDIAN)
    arg  = (CLR_UINT32)(*(const CLR_UINT8 *)ip)      ; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT32)(*(const CLR_UINT8 *)ip) <<  8; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT32)(*(const CLR_UINT8 *)ip) << 16; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT32)(*(const CLR_UINT8 *)ip) << 24; ip += sizeof(CLR_UINT8);
#else
    arg  = (CLR_UINT32)(*(const CLR_UINT8 *)ip) << 24; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT32)(*(const CLR_UINT8 *)ip) << 16; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT32)(*(const CLR_UINT8 *)ip) <<  8; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT32)(*(const CLR_UINT8 *)ip)      ; ip += sizeof(CLR_UINT8);
#endif //NETMF_TARGET_BIG_ENDIAN
    arg  = (CLR_INT32)arg;
}
template<typename T> __inline void TINYCLR_READ_UNALIGNED_INT64(T& arg, CLR_PMETADATA& ip) 
{
#if !defined(NETMF_TARGET_BIG_ENDIAN)
    arg  = (CLR_UINT64)(*(const CLR_UINT8 *)ip)      ; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) <<  8; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 16; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 24; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 32; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 40; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 48; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 56; ip += sizeof(CLR_UINT8);
#else
    arg  = (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 56; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 48; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 40; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 32; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 24; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) << 16; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip) <<  8; ip += sizeof(CLR_UINT8);
    arg |= (CLR_UINT64)(*(const CLR_UINT8 *)ip)      ; ip += sizeof(CLR_UINT8);
#endif //NETMF_TARGET_BIG_ENDIAN
    arg  = (CLR_INT64)arg;
}


#elif defined(_MSC_VER)

#define TINYCLR_READ_UNALIGNED_UINT8(arg,ip)  arg = *(__declspec(align(1)) const CLR_UINT8 *)ip; ip += sizeof(CLR_UINT8 )
#define TINYCLR_READ_UNALIGNED_UINT16(arg,ip) arg = *(__declspec(align(1)) const CLR_UINT16*)ip; ip += sizeof(CLR_UINT16)
#define TINYCLR_READ_UNALIGNED_UINT32(arg,ip) arg = *(__declspec(align(1)) const CLR_UINT32*)ip; ip += sizeof(CLR_UINT32)
#define TINYCLR_READ_UNALIGNED_UINT64(arg,ip) arg = *(__declspec(align(1)) const CLR_UINT64*)ip; ip += sizeof(CLR_UINT64)

#define TINYCLR_READ_UNALIGNED_INT8(arg,ip)   arg = *(__declspec(align(1)) const CLR_INT8 * )ip; ip += sizeof(CLR_INT8  )
#define TINYCLR_READ_UNALIGNED_INT16(arg,ip)  arg = *(__declspec(align(1)) const CLR_INT16* )ip; ip += sizeof(CLR_INT16 )
#define TINYCLR_READ_UNALIGNED_INT32(arg,ip)  arg = *(__declspec(align(1)) const CLR_INT32* )ip; ip += sizeof(CLR_INT32 )
#define TINYCLR_READ_UNALIGNED_INT64(arg,ip)  arg = *(__declspec(align(1)) const CLR_INT64* )ip; ip += sizeof(CLR_INT64 )

//--//

#define TINYCLR_WRITE_UNALIGNED_UINT8(ip,arg)  *(__declspec(align(1)) CLR_UINT8 *)ip = arg; ip += sizeof(CLR_UINT8 )
#define TINYCLR_WRITE_UNALIGNED_UINT16(ip,arg) *(__declspec(align(1)) CLR_UINT16*)ip = arg; ip += sizeof(CLR_UINT16)
#define TINYCLR_WRITE_UNALIGNED_UINT32(ip,arg) *(__declspec(align(1)) CLR_UINT32*)ip = arg; ip += sizeof(CLR_UINT32)
#define TINYCLR_WRITE_UNALIGNED_UINT64(ip,arg) *(__declspec(align(1)) CLR_UINT64*)ip = arg; ip += sizeof(CLR_UINT64)

#define TINYCLR_WRITE_UNALIGNED_INT8(ip,arg)   *(__declspec(align(1)) CLR_INT8 * )ip = arg; ip += sizeof(CLR_INT8  )
#define TINYCLR_WRITE_UNALIGNED_INT16(ip,arg)  *(__declspec(align(1)) CLR_INT16* )ip = arg; ip += sizeof(CLR_INT16 )
#define TINYCLR_WRITE_UNALIGNED_INT32(ip,arg)  *(__declspec(align(1)) CLR_INT32* )ip = arg; ip += sizeof(CLR_INT32 )
#define TINYCLR_WRITE_UNALIGNED_INT64(ip,arg)  *(__declspec(align(1)) CLR_INT64* )ip = arg; ip += sizeof(CLR_INT64 )

#else // TODO: __packed is compiler specific... Which compiler is this for?

#define TINYCLR_READ_UNALIGNED_UINT8(arg,ip)  arg = *(__packed CLR_UINT8 *)ip; ip += sizeof(CLR_UINT8 )
#define TINYCLR_READ_UNALIGNED_UINT16(arg,ip) arg = *(__packed CLR_UINT16*)ip; ip += sizeof(CLR_UINT16)
#define TINYCLR_READ_UNALIGNED_UINT32(arg,ip) arg = *(__packed CLR_UINT32*)ip; ip += sizeof(CLR_UINT32)
#define TINYCLR_READ_UNALIGNED_UINT64(arg,ip) arg = *(__packed CLR_UINT64*)ip; ip += sizeof(CLR_UINT64)
#define TINYCLR_READ_UNALIGNED_INT8(arg,ip)   arg = *(__packed CLR_INT8 * )ip; ip += sizeof(CLR_INT8  )
#define TINYCLR_READ_UNALIGNED_INT16(arg,ip)  arg = *(__packed CLR_INT16* )ip; ip += sizeof(CLR_INT16 )
#define TINYCLR_READ_UNALIGNED_INT32(arg,ip)  arg = *(__packed CLR_INT32* )ip; ip += sizeof(CLR_INT32 )
#define TINYCLR_READ_UNALIGNED_INT64(arg,ip)  arg = *(__packed CLR_INT64* )ip; ip += sizeof(CLR_INT64 )

//--//

#define TINYCLR_WRITE_UNALIGNED_UINT8(ip,arg)  *(__packed CLR_UINT8 *)ip = arg; ip += sizeof(CLR_UINT8 )
#define TINYCLR_WRITE_UNALIGNED_UINT16(ip,arg) *(__packed CLR_UINT16*)ip = arg; ip += sizeof(CLR_UINT16)
#define TINYCLR_WRITE_UNALIGNED_UINT32(ip,arg) *(__packed CLR_UINT32*)ip = arg; ip += sizeof(CLR_UINT32)
#define TINYCLR_WRITE_UNALIGNED_UINT64(ip,arg) *(__packed CLR_UINT64*)ip = arg; ip += sizeof(CLR_UINT64)

#define TINYCLR_WRITE_UNALIGNED_INT8(ip,arg)   *(__packed CLR_INT8 * )ip = arg; ip += sizeof(CLR_INT8  )
#define TINYCLR_WRITE_UNALIGNED_INT16(ip,arg)  *(__packed CLR_INT16* )ip = arg; ip += sizeof(CLR_INT16 )
#define TINYCLR_WRITE_UNALIGNED_INT32(ip,arg)  *(__packed CLR_INT32* )ip = arg; ip += sizeof(CLR_INT32 )
#define TINYCLR_WRITE_UNALIGNED_INT64(ip,arg)  *(__packed CLR_INT64* )ip = arg; ip += sizeof(CLR_INT64 )

#endif

//--//
#define TINYCLR_READ_UNALIGNED_OPCODE(op,ip) op = CLR_OPCODE(*ip++); if(op == CEE_PREFIX1) { opcode = CLR_OPCODE(*ip++ + 256); }

#define TINYCLR_READ_UNALIGNED_COMPRESSED_FIELDTOKEN(arg,ip)  TINYCLR_READ_UNALIGNED_UINT16( arg, ip ); arg = CLR_UncompressFieldToken ( arg )
#define TINYCLR_READ_UNALIGNED_COMPRESSED_METHODTOKEN(arg,ip) TINYCLR_READ_UNALIGNED_UINT16( arg, ip ); arg = CLR_UncompressMethodToken( arg )
#define TINYCLR_READ_UNALIGNED_COMPRESSED_TYPETOKEN(arg,ip)   TINYCLR_READ_UNALIGNED_UINT16( arg, ip ); arg = CLR_UncompressTypeToken  ( arg )
#define TINYCLR_READ_UNALIGNED_COMPRESSED_STRINGTOKEN(arg,ip) TINYCLR_READ_UNALIGNED_UINT16( arg, ip ); arg = CLR_UncompressStringToken( arg )


//--//

inline CLR_OPCODE CLR_ReadNextOpcode( CLR_PMETADATA& ip )
{
    CLR_PMETADATA ptr    = ip;
    CLR_OPCODE       opcode = CLR_OPCODE(*ptr++);

    if(opcode == CEE_PREFIX1)
    {
        opcode = CLR_OPCODE(*ptr++ + 256);
    }

    ip = ptr;

    return opcode;
}

inline CLR_OPCODE CLR_ReadNextOpcodeCompressed( CLR_PMETADATA& ip )
{
    CLR_PMETADATA ptr    = ip;
    CLR_OPCODE       opcode = CLR_OPCODE(*ptr++);

    if(opcode == CEE_PREFIX1)
    {
        opcode = CLR_OPCODE(*ptr++ + 256);
    }

    ip = ptr;

    return opcode;
}

//--//

#define FETCH_ARG_UINT8(arg,ip)                   CLR_UINT32 arg; TINYCLR_READ_UNALIGNED_UINT8 ( arg, ip )
#define FETCH_ARG_UINT16(arg,ip)                  CLR_UINT32 arg; TINYCLR_READ_UNALIGNED_UINT16( arg, ip )
#define FETCH_ARG_UINT32(arg,ip)                  CLR_UINT32 arg; TINYCLR_READ_UNALIGNED_UINT32( arg, ip )
#define FETCH_ARG_UINT64(arg,ip)                  CLR_UINT64 arg; TINYCLR_READ_UNALIGNED_UINT64( arg, ip )

#define FETCH_ARG_INT8(arg,ip)                    CLR_INT32  arg; TINYCLR_READ_UNALIGNED_INT8 ( arg, ip )
#define FETCH_ARG_INT16(arg,ip)                   CLR_INT32  arg; TINYCLR_READ_UNALIGNED_INT16( arg, ip )
#define FETCH_ARG_INT32(arg,ip)                   CLR_INT32  arg; TINYCLR_READ_UNALIGNED_INT32( arg, ip )
#define FETCH_ARG_INT64(arg,ip)                   CLR_INT64  arg; TINYCLR_READ_UNALIGNED_INT64( arg, ip )

#define FETCH_ARG_COMPRESSED_STRINGTOKEN(arg,ip) CLR_UINT32 arg; TINYCLR_READ_UNALIGNED_COMPRESSED_STRINGTOKEN( arg, ip )
#define FETCH_ARG_COMPRESSED_FIELDTOKEN(arg,ip)  CLR_UINT32 arg; TINYCLR_READ_UNALIGNED_COMPRESSED_FIELDTOKEN ( arg, ip )
#define FETCH_ARG_COMPRESSED_TYPETOKEN(arg,ip)   CLR_UINT32 arg; TINYCLR_READ_UNALIGNED_COMPRESSED_TYPETOKEN  ( arg, ip )
#define FETCH_ARG_COMPRESSED_METHODTOKEN(arg,ip) CLR_UINT32 arg; TINYCLR_READ_UNALIGNED_COMPRESSED_METHODTOKEN( arg, ip )
#define FETCH_ARG_TOKEN(arg,ip)                  CLR_UINT32 arg; TINYCLR_READ_UNALIGNED_UINT32                ( arg, ip )

//--//

#if defined(TINYCLR_JITTER) || defined(_WIN32)

CLR_PMETADATA CLR_SkipBodyOfOpcode          ( CLR_PMETADATA ip, CLR_OPCODE opcode );
CLR_PMETADATA CLR_SkipBodyOfOpcodeCompressed( CLR_PMETADATA ip, CLR_OPCODE opcode );

#endif

////////////////////////////////////////////////////////////////////////////////////////////////////

extern bool CLR_SafeSprintfV( LPSTR& szBuffer, size_t& iBuffer, LPCSTR format, va_list arg );
extern bool CLR_SafeSprintf ( LPSTR& szBuffer, size_t& iBuffer, LPCSTR format, ...         );

#if !defined(BUILD_RTM)

#define TINYCLR_DEBUG_STOP() _ASSERTE(FALSE)

#else

#define TINYCLR_DEBUG_STOP() CPU_Reset()

#endif

//--//

struct CLR_Debug
{
    static int  PrintfV( const char *format, va_list arg );
    static int  Printf ( const char *format, ...         );
    static void Emit   ( const char *text, int len       );
    static void Flush  (                                 );

    //--//

    typedef int (*OutputHandler)( const char *format, ... );

#if defined(_WIN32)
    static void RedirectToString( std::string* str );
#endif
};

////////////////////////////////////////////////////////////////////////////////

struct CLR_RECORD_VERSION
{
    CLR_UINT16 iMajorVersion;
    CLR_UINT16 iMinorVersion;
    CLR_UINT16 iBuildNumber;
    CLR_UINT16 iRevisionNumber;
};

struct CLR_RECORD_ASSEMBLY
{
    static const CLR_UINT32 c_Flags_NeedReboot = 0x00000001;
    static const CLR_UINT32 c_Flags_Patch      = 0x00000002;
    static const CLR_UINT32 c_Flags_BigEndian  = 0x80000080;

    CLR_UINT8          marker[ 8 ];
    //
    CLR_UINT32         headerCRC;
    CLR_UINT32         assemblyCRC;
    CLR_UINT32         flags;
    //
    CLR_UINT32         nativeMethodsChecksum;
    CLR_UINT32         patchEntryOffset;
    //
    CLR_RECORD_VERSION version;
    //
    CLR_STRING         assemblyName;    // TBL_Strings
    CLR_UINT16         stringTableVersion;
    //
    CLR_OFFSET_LONG    startOfTables[ TBL_Max ];
    CLR_UINT32         numOfPatchedMethods;
    //             
    //For every table, a number of bytes that were padded to the end of the table
    //to align to DWORD.  Each table starts at a DWORD boundary, and ends 
    //at a DWORD boundary.  Some of these tables will, by construction, have
    //no padding, and all will have values in the range [0-3].  This isn't the most
    //compact form to hold this information, but it only costs 16 bytes/assembly.
    //Trying to only align some of the tables is just much more hassle than it's worth.
    //And, of course, this field also has to be DWORD-aligned.
    CLR_UINT8          paddingOfTables[ ((TBL_Max-1)+3)/4*4 ];
    //--//

    bool GoodHeader  () const;
    bool GoodAssembly() const;

#if defined(_WIN32)
    void ComputeCRC();
#endif

    CLR_OFFSET_LONG SizeOfTable( CLR_TABLESENUM tbl ) const { return startOfTables[ tbl+1 ] - startOfTables[ tbl ] - paddingOfTables[ tbl ]; }

    CLR_OFFSET_LONG TotalSize() const { return startOfTables[ TBL_EndOfAssembly ]; }

    //--//

    static CLR_UINT32 ComputeAssemblyHash( LPCSTR name, const CLR_RECORD_VERSION& ver );
};

struct CLR_RECORD_ASSEMBLYREF
{
    CLR_STRING         name;            // TBL_Strings
    CLR_UINT16         pad;
    //
    CLR_RECORD_VERSION version;
};

struct CLR_RECORD_TYPEREF
{
    CLR_STRING name;            // TBL_Strings
    CLR_STRING nameSpace;       // TBL_Strings
    //
    CLR_IDX    scope;           // TBL_AssemblyRef | TBL_TypeRef // 0x8000
    CLR_UINT16 pad;
};

struct CLR_RECORD_FIELDREF
{
    CLR_STRING name;            // TBL_Strings
    CLR_IDX    container;       // TBL_TypeRef
    //
    CLR_SIG    sig;             // TBL_Signatures
    CLR_UINT16 pad;
};

struct CLR_RECORD_METHODREF
{
    CLR_STRING name;            // TBL_Strings
    CLR_IDX    container;       // TBL_TypeRef
    //
    CLR_SIG    sig;             // TBL_Signatures
    CLR_UINT16 pad;
};

struct CLR_RECORD_TYPEDEF
{
    static const CLR_UINT16 TD_Scope_Mask              = 0x0007;
    static const CLR_UINT16 TD_Scope_NotPublic         = 0x0000; // Class is not public scope.
    static const CLR_UINT16 TD_Scope_Public            = 0x0001; // Class is public scope.
    static const CLR_UINT16 TD_Scope_NestedPublic      = 0x0002; // Class is nested with public visibility.
    static const CLR_UINT16 TD_Scope_NestedPrivate     = 0x0003; // Class is nested with private visibility.
    static const CLR_UINT16 TD_Scope_NestedFamily      = 0x0004; // Class is nested with family visibility.
    static const CLR_UINT16 TD_Scope_NestedAssembly    = 0x0005; // Class is nested with assembly visibility.
    static const CLR_UINT16 TD_Scope_NestedFamANDAssem = 0x0006; // Class is nested with family and assembly visibility.
    static const CLR_UINT16 TD_Scope_NestedFamORAssem  = 0x0007; // Class is nested with family or assembly visibility.

    static const CLR_UINT16 TD_Serializable            = 0x0008;

    static const CLR_UINT16 TD_Semantics_Mask          = 0x0030;
    static const CLR_UINT16 TD_Semantics_Class         = 0x0000;
    static const CLR_UINT16 TD_Semantics_ValueType     = 0x0010;
    static const CLR_UINT16 TD_Semantics_Interface     = 0x0020;
    static const CLR_UINT16 TD_Semantics_Enum          = 0x0030;

    static const CLR_UINT16 TD_Abstract                = 0x0040;
    static const CLR_UINT16 TD_Sealed                  = 0x0080;

    static const CLR_UINT16 TD_SpecialName             = 0x0100;
    static const CLR_UINT16 TD_Delegate                = 0x0200;
    static const CLR_UINT16 TD_MulticastDelegate       = 0x0400;

    static const CLR_UINT16 TD_Patched                 = 0x0800;

    static const CLR_UINT16 TD_BeforeFieldInit         = 0x1000;
    static const CLR_UINT16 TD_HasSecurity             = 0x2000;
    static const CLR_UINT16 TD_HasFinalizer            = 0x4000;
    static const CLR_UINT16 TD_HasAttributes           = 0x8000;


    CLR_STRING name;            // TBL_Strings
    CLR_STRING nameSpace;       // TBL_Strings
    //
    CLR_IDX    extends;         // TBL_TypeDef | TBL_TypeRef // 0x8000
    CLR_IDX    enclosingType;   // TBL_TypeDef
    //
    CLR_SIG    interfaces;      // TBL_Signatures
    CLR_IDX    methods_First;   // TBL_MethodDef
    //
    CLR_UINT8  vMethods_Num;
    CLR_UINT8  iMethods_Num;
    CLR_UINT8  sMethods_Num;
    CLR_UINT8  dataType;
    //
    CLR_IDX    sFields_First;   // TBL_FieldDef
    CLR_IDX    iFields_First;   // TBL_FieldDef
    //
    CLR_UINT8  sFields_Num;
    CLR_UINT8  iFields_Num;
    CLR_UINT16 flags;

    //--//

    bool IsEnum    () const { return (flags & (TD_Semantics_Mask                 )) == TD_Semantics_Enum; }
    bool IsDelegate() const { return (flags & (TD_Delegate | TD_MulticastDelegate)) != 0                ; }
};

struct CLR_RECORD_FIELDDEF
{
    static const CLR_UINT16 FD_Scope_Mask         = 0x0007;
    static const CLR_UINT16 FD_Scope_PrivateScope = 0x0000;     // Member not referenceable.
    static const CLR_UINT16 FD_Scope_Private      = 0x0001;     // Accessible only by the parent type.
    static const CLR_UINT16 FD_Scope_FamANDAssem  = 0x0002;     // Accessible by sub-types only in this Assembly.
    static const CLR_UINT16 FD_Scope_Assembly     = 0x0003;     // Accessibly by anyone in the Assembly.
    static const CLR_UINT16 FD_Scope_Family       = 0x0004;     // Accessible only by type and sub-types.
    static const CLR_UINT16 FD_Scope_FamORAssem   = 0x0005;     // Accessibly by sub-types anywhere, plus anyone in assembly.
    static const CLR_UINT16 FD_Scope_Public       = 0x0006;     // Accessibly by anyone who has visibility to this scope.

    static const CLR_UINT16 FD_NotSerialized      = 0x0008;     // Field does not have to be serialized when type is remoted.

    static const CLR_UINT16 FD_Static             = 0x0010;     // Defined on type, else per instance.
    static const CLR_UINT16 FD_InitOnly           = 0x0020;     // Field may only be initialized, not written to after init.
    static const CLR_UINT16 FD_Literal            = 0x0040;     // Value is compile time constant.

    static const CLR_UINT16 FD_SpecialName        = 0x0100;     // field is special.  Name describes how.
    static const CLR_UINT16 FD_HasDefault         = 0x0200;     // Field has default.
    static const CLR_UINT16 FD_HasFieldRVA        = 0x0400;     // Field has RVA.

    static const CLR_UINT16 FD_NoReflection       = 0x0800;     // field does not allow reflection

    static const CLR_UINT16 FD_HasAttributes      = 0x8000;


    CLR_STRING name;            // TBL_Strings
    CLR_SIG    sig;             // TBL_Signatures
    //
    CLR_SIG    defaultValue;    // TBL_Signatures
    CLR_UINT16 flags;
};

struct CLR_RECORD_METHODDEF
{
    static const CLR_UINT32 MD_Scope_Mask           = 0x00000007;
    static const CLR_UINT32 MD_Scope_PrivateScope   = 0x00000000;     // Member not referenceable.
    static const CLR_UINT32 MD_Scope_Private        = 0x00000001;     // Accessible only by the parent type.
    static const CLR_UINT32 MD_Scope_FamANDAssem    = 0x00000002;     // Accessible by sub-types only in this Assembly.
    static const CLR_UINT32 MD_Scope_Assem          = 0x00000003;     // Accessibly by anyone in the Assembly.
    static const CLR_UINT32 MD_Scope_Family         = 0x00000004;     // Accessible only by type and sub-types.
    static const CLR_UINT32 MD_Scope_FamORAssem     = 0x00000005;     // Accessibly by sub-types anywhere, plus anyone in assembly.
    static const CLR_UINT32 MD_Scope_Public         = 0x00000006;     // Accessibly by anyone who has visibility to this scope.

    static const CLR_UINT32 MD_Static               = 0x00000010;     // Defined on type, else per instance.
    static const CLR_UINT32 MD_Final                = 0x00000020;     // Method may not be overridden.
    static const CLR_UINT32 MD_Virtual              = 0x00000040;     // Method virtual.
    static const CLR_UINT32 MD_HideBySig            = 0x00000080;     // Method hides by name+sig, else just by name.

    static const CLR_UINT32 MD_VtableLayoutMask     = 0x00000100;
    static const CLR_UINT32 MD_ReuseSlot            = 0x00000000;     // The default.
    static const CLR_UINT32 MD_NewSlot              = 0x00000100;     // Method always gets a new slot in the vtable.
    static const CLR_UINT32 MD_Abstract             = 0x00000200;     // Method does not provide an implementation.
    static const CLR_UINT32 MD_SpecialName          = 0x00000400;     // Method is special.  Name describes how.
    static const CLR_UINT32 MD_NativeProfiled       = 0x00000800;

    static const CLR_UINT32 MD_Constructor          = 0x00001000;
    static const CLR_UINT32 MD_StaticConstructor    = 0x00002000;
    static const CLR_UINT32 MD_Finalizer            = 0x00004000;    

    static const CLR_UINT32 MD_DelegateConstructor  = 0x00010000;
    static const CLR_UINT32 MD_DelegateInvoke       = 0x00020000;
    static const CLR_UINT32 MD_DelegateBeginInvoke  = 0x00040000;
    static const CLR_UINT32 MD_DelegateEndInvoke    = 0x00080000;

    static const CLR_UINT32 MD_Synchronized         = 0x01000000;
    static const CLR_UINT32 MD_GloballySynchronized = 0x02000000;
    static const CLR_UINT32 MD_Patched              = 0x04000000;
    static const CLR_UINT32 MD_EntryPoint           = 0x08000000;
    static const CLR_UINT32 MD_RequireSecObject     = 0x10000000;     // Method calls another method containing security code.
    static const CLR_UINT32 MD_HasSecurity          = 0x20000000;     // Method has security associate with it.
    static const CLR_UINT32 MD_HasExceptionHandlers = 0x40000000;
    static const CLR_UINT32 MD_HasAttributes        = 0x80000000;


    CLR_STRING name;            // TBL_Strings
    CLR_OFFSET RVA;
    //
    CLR_UINT32 flags;
    //
    CLR_UINT8  retVal;
    CLR_UINT8  numArgs;
    CLR_UINT8  numLocals;
    CLR_UINT8  lengthEvalStack;
    //
    CLR_SIG    locals;          // TBL_Signatures
    CLR_SIG    sig;             // TBL_Signatures
};

struct CLR_RECORD_ATTRIBUTE
{
    CLR_UINT16 ownerType;       // one of TBL_TypeDef, TBL_MethodDef, or TBL_FieldDef.
    CLR_UINT16 ownerIdx;        // TBL_TypeDef | TBL_MethodDef | TBL_FielfDef
    CLR_UINT16 constructor;
    CLR_SIG    data;            // TBL_Signatures

    CLR_UINT32 Key() const { return *(CLR_UINT32*)&ownerType; }
};

struct CLR_RECORD_TYPESPEC
{
    CLR_SIG    sig;             // TBL_Signatures
    CLR_UINT16 pad;
};

struct CLR_RECORD_EH
{
    static const CLR_UINT16 EH_Catch    = 0x0000;
    static const CLR_UINT16 EH_CatchAll = 0x0001;
    static const CLR_UINT16 EH_Finally  = 0x0002;
    static const CLR_UINT16 EH_Filter   = 0x0003;

    //--//

    CLR_UINT16 mode;
    union {
      CLR_IDX    classToken;      // TBL_TypeDef | TBL_TypeRef
      CLR_OFFSET filterStart;
    };
    CLR_OFFSET tryStart;
    CLR_OFFSET tryEnd;
    CLR_OFFSET handlerStart;
    CLR_OFFSET handlerEnd;

    //--//

    static CLR_PMETADATA ExtractEhFromByteCode( CLR_PMETADATA ipEnd, const CLR_RECORD_EH*& ptrEh, CLR_UINT32& numEh );

    CLR_UINT32 GetToken() const;
};

CT_ASSERT_UNIQUE_NAME( sizeof(CLR_RECORD_EH) == 12, CLR_RECORD_EH )

struct CLR_RECORD_RESOURCE_FILE
{
    static const CLR_UINT32 CURRENT_VERSION = 2;

    CLR_UINT32 version;
    CLR_UINT32 sizeOfHeader;
    CLR_UINT32 sizeOfResourceHeader;
    CLR_UINT32 numberOfResources;
    CLR_STRING name;            // TBL_Strings
    CLR_UINT16 pad;
    CLR_UINT32 offset;          // TBL_Resource
};

struct CLR_RECORD_RESOURCE
{
    static const CLR_UINT8 RESOURCE_Invalid  = 0x00;
    static const CLR_UINT8 RESOURCE_Bitmap   = 0x01;
    static const CLR_UINT8 RESOURCE_Font     = 0x02;
    static const CLR_UINT8 RESOURCE_String   = 0x03;
    static const CLR_UINT8 RESOURCE_Binary   = 0x04;

    static const CLR_UINT8 FLAGS_PaddingMask = 0x03;
    static const CLR_INT16 SENTINEL_ID       = 0x7FFF;

    //
    // Sorted on id
    //
    CLR_INT16  id;
    CLR_UINT8  kind;
    CLR_UINT8  flags;
    CLR_UINT32 offset;
};


#if defined(_MSC_VER)
#pragma pack(pop, TINYCLR_TYPES_H)
#endif

#if defined(GCCOP_V4_2)

extern double fmod(double x, double y);
extern double floor (double x);

extern void *bsearch(const void *key, const void *base, size_t num, size_t width, int (*compare)(const void *, const void *)) ;
#endif

#endif // _TINYCLR_TYPES_H_
