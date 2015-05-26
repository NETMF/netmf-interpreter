////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

#include "corhdr_private.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

#define DT_NA    CLR_RT_DataTypeLookup::c_NA
#define DT_VS    CLR_RT_DataTypeLookup::c_VariableSize

#define DT_PRIM  CLR_RT_DataTypeLookup::c_Primitive
#define DT_ITF   CLR_RT_DataTypeLookup::c_Interface
#define DT_CLASS CLR_RT_DataTypeLookup::c_Class
#define DT_VALUE CLR_RT_DataTypeLookup::c_ValueType
#define DT_ENUM  CLR_RT_DataTypeLookup::c_Enum

#define DT_ARRAY CLR_RT_DataTypeLookup::c_Array
#define DT_ARLST CLR_RT_DataTypeLookup::c_ArrayList

#define DT_REF   CLR_RT_DataTypeLookup::c_Reference
#define DT_NUM   CLR_RT_DataTypeLookup::c_Numeric
#define DT_INT   CLR_RT_DataTypeLookup::c_Integer
#define DT_SGN   CLR_RT_DataTypeLookup::c_Signed
#define DT_DIR   CLR_RT_DataTypeLookup::c_Direct
#define DT_OPT   CLR_RT_DataTypeLookup::c_OptimizedValueType
#define DT_MT    CLR_RT_DataTypeLookup::c_ManagedType

#define DT_U1 sizeof(CLR_UINT8)
#define DT_I1 sizeof(CLR_INT8)
#define DT_U2 sizeof(CLR_UINT16)
#define DT_I2 sizeof(CLR_INT16)
#define DT_U4 sizeof(CLR_UINT32)
#define DT_I4 sizeof(CLR_INT32)
#define DT_U8 sizeof(CLR_UINT64)
#define DT_I8 sizeof(CLR_INT64)
#define DT_BL sizeof(CLR_RT_HeapBlock)

#define DT_T(x)   (CLR_UINT8)DATATYPE_##x
#define DT_CNV(x) (CLR_UINT8)ELEMENT_TYPE_##x
#define DT_CLS(x) &g_CLR_RT_WellKnownTypes.x

#define DT_NOREL(x)  NULL
#define DT_REL(x)    (CLR_RT_HeapBlockRelocate)&x

#if defined(PLATFORM_WINDOWS) || defined(TINYCLR_TRACE_MEMORY_STATS)
#define DT_OPT_NAME(x) , #x
#else
#define DT_OPT_NAME(x)
#endif

#define DATATYPE_NOT_SUPPORTED { DT_NA, DT_NA, DT_NA, DT_T(FIRST_INVALID), DT_CNV(END), NULL, DT_NOREL(CLR_RT_HeapBlock) DT_OPT_NAME(NOT_SUPPORTED) },

const CLR_RT_DataTypeLookup c_CLR_RT_DataTypeLookup[] =
{
////  m_flags, m_sizeInBits, m_sizeInBytes, m_promoteTo, m_convertToElementType, m_cls, m_relocate, m_name
////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    { DT_NA                                                                   , DT_NA, DT_NA, DT_T(VOID                ), DT_CNV(VOID     ), NULL                       , DT_NOREL(CLR_RT_HeapBlock                                 ) DT_OPT_NAME(VOID                ) }, // DATATYPE_VOID
                                                                                                                                                                                                                                                                   //
    { DT_NUM | DT_INT          | DT_PRIM             | DT_DIR | DT_OPT | DT_MT,     1, DT_U1, DT_T(I4                  ), DT_CNV(BOOLEAN  ), DT_CLS(m_Boolean          ), DT_NOREL(CLR_RT_HeapBlock                                 ) DT_OPT_NAME(BOOLEAN             ) }, // DATATYPE_BOOLEAN
    { DT_NUM | DT_INT | DT_SGN | DT_PRIM             | DT_DIR | DT_OPT | DT_MT,     8, DT_I1, DT_T(I4                  ), DT_CNV(I1       ), DT_CLS(m_Int8             ), DT_NOREL(CLR_RT_HeapBlock                                 ) DT_OPT_NAME(I1                  ) }, // DATATYPE_I1
    { DT_NUM | DT_INT          | DT_PRIM             | DT_DIR | DT_OPT | DT_MT,     8, DT_U1, DT_T(I4                  ), DT_CNV(U1       ), DT_CLS(m_UInt8            ), DT_NOREL(CLR_RT_HeapBlock                                 ) DT_OPT_NAME(U1                  ) }, // DATATYPE_U1
                                                                                                                                                                                                                                                                   //
    { DT_NUM | DT_INT          | DT_PRIM             | DT_DIR | DT_OPT | DT_MT,    16, DT_U2, DT_T(I4                  ), DT_CNV(CHAR     ), DT_CLS(m_Char             ), DT_NOREL(CLR_RT_HeapBlock                                 ) DT_OPT_NAME(CHAR                ) }, // DATATYPE_CHAR
    { DT_NUM | DT_INT | DT_SGN | DT_PRIM             | DT_DIR | DT_OPT | DT_MT,    16, DT_I2, DT_T(I4                  ), DT_CNV(I2       ), DT_CLS(m_Int16            ), DT_NOREL(CLR_RT_HeapBlock                                 ) DT_OPT_NAME(I2                  ) }, // DATATYPE_I2
    { DT_NUM | DT_INT          | DT_PRIM             | DT_DIR | DT_OPT | DT_MT,    16, DT_U2, DT_T(I4                  ), DT_CNV(U2       ), DT_CLS(m_UInt16           ), DT_NOREL(CLR_RT_HeapBlock                                 ) DT_OPT_NAME(U2                  ) }, // DATATYPE_U2
                                                                                                                                                                                                                                                                   //
    { DT_NUM | DT_INT | DT_SGN | DT_PRIM             | DT_DIR | DT_OPT | DT_MT,    32, DT_I4, DT_T(I4                  ), DT_CNV(I4       ), DT_CLS(m_Int32            ), DT_NOREL(CLR_RT_HeapBlock                                 ) DT_OPT_NAME(I4                  ) }, // DATATYPE_I4
    { DT_NUM | DT_INT          | DT_PRIM             | DT_DIR | DT_OPT | DT_MT,    32, DT_U4, DT_T(I4                  ), DT_CNV(U4       ), DT_CLS(m_UInt32           ), DT_NOREL(CLR_RT_HeapBlock                                 ) DT_OPT_NAME(U4                  ) }, // DATATYPE_U4
    { DT_NUM |          DT_SGN | DT_PRIM             | DT_DIR | DT_OPT | DT_MT,    32, DT_I4, DT_T(R4                  ), DT_CNV(R4       ), DT_CLS(m_Single           ), DT_NOREL(CLR_RT_HeapBlock                                 ) DT_OPT_NAME(R4                  ) }, // DATATYPE_R4
                                                                                                                                                                                                                                                                   //
    { DT_NUM | DT_INT | DT_SGN | DT_PRIM             | DT_DIR | DT_OPT | DT_MT,    64, DT_I8, DT_T(I8                  ), DT_CNV(I8       ), DT_CLS(m_Int64            ), DT_NOREL(CLR_RT_HeapBlock                                 ) DT_OPT_NAME(I8                  ) }, // DATATYPE_I8
    { DT_NUM | DT_INT          | DT_PRIM             | DT_DIR | DT_OPT | DT_MT,    64, DT_U8, DT_T(I8                  ), DT_CNV(U8       ), DT_CLS(m_UInt64           ), DT_NOREL(CLR_RT_HeapBlock                                 ) DT_OPT_NAME(U8                  ) }, // DATATYPE_U8
    { DT_NUM |          DT_SGN | DT_PRIM             | DT_DIR | DT_OPT | DT_MT,    64, DT_I8, DT_T(R8                  ), DT_CNV(R8       ), DT_CLS(m_Double           ), DT_NOREL(CLR_RT_HeapBlock                                 ) DT_OPT_NAME(R8                  ) }, // DATATYPE_R8
    {          DT_INT | DT_SGN | DT_VALUE            | DT_DIR | DT_OPT | DT_MT,    64, DT_BL, DT_T(DATETIME            ), DT_CNV(END      ), DT_CLS(m_DateTime         ), DT_NOREL(CLR_RT_HeapBlock                                 ) DT_OPT_NAME(DATETIME            ) }, // DATATYPE_DATETIME
    {          DT_INT | DT_SGN | DT_VALUE            | DT_DIR | DT_OPT | DT_MT,    64, DT_BL, DT_T(TIMESPAN            ), DT_CNV(END      ), DT_CLS(m_TimeSpan         ), DT_NOREL(CLR_RT_HeapBlock                                 ) DT_OPT_NAME(TIMESPAN            ) }, // DATATYPE_TIMESPAN
    { DT_REF                   | DT_PRIM             | DT_DIR          | DT_MT, DT_VS, DT_BL, DT_T(STRING              ), DT_CNV(STRING   ), DT_CLS(m_String           ), DT_REL  (CLR_RT_HeapBlock              ::Relocate_String  ) DT_OPT_NAME(STRING              ) }, // DATATYPE_STRING
                                                                                                                                                                                                                                                                   //
    { DT_REF                                         | DT_DIR          | DT_MT, DT_NA, DT_BL, DT_T(OBJECT              ), DT_CNV(OBJECT   ), DT_CLS(m_Object           ), DT_REL  (CLR_RT_HeapBlock              ::Relocate_Obj     ) DT_OPT_NAME(OBJECT              ) }, // DATATYPE_OBJECT
    { DT_REF                   | DT_CLASS                              | DT_MT, DT_NA, DT_BL, DT_T(CLASS               ), DT_CNV(CLASS    ), NULL                       , DT_REL  (CLR_RT_HeapBlock              ::Relocate_Cls     ) DT_OPT_NAME(CLASS               ) }, // DATATYPE_CLASS
    { DT_REF                   | DT_VALUE                              | DT_MT, DT_NA, DT_BL, DT_T(VALUETYPE           ), DT_CNV(VALUETYPE), NULL                       , DT_REL  (CLR_RT_HeapBlock              ::Relocate_Cls     ) DT_OPT_NAME(VALUETYPE           ) }, // DATATYPE_VALUETYPE
    { DT_REF                   | DT_CLASS | DT_ARRAY                   | DT_MT, DT_NA, DT_BL, DT_T(SZARRAY             ), DT_CNV(SZARRAY  ), DT_CLS(m_Array            ), DT_REL  (CLR_RT_HeapBlock_Array        ::Relocate         ) DT_OPT_NAME(SZARRAY             ) }, // DATATYPE_SZARRAY
    { DT_REF                                                           | DT_MT, DT_NA, DT_NA, DT_T(BYREF               ), DT_CNV(BYREF    ), NULL                       , DT_REL  (CLR_RT_HeapBlock              ::Relocate_Ref     ) DT_OPT_NAME(BYREF               ) }, // DATATYPE_BYREF
                                                                                                                                                                                                                                                                   //
                                                                                                                                                                                                                                                                   ////////////////////////////////////
                                                                                                                                                                                                                                                                   //
    { DT_NA                                                                   , DT_NA, DT_NA, DT_T(FREEBLOCK           ), DT_CNV(END      ), NULL                       , DT_NOREL(CLR_RT_HeapBlock                                 ) DT_OPT_NAME(FREEBLOCK           ) }, // DATATYPE_FREEBLOCK
    { DT_NA                                                                   , DT_NA, DT_NA, DT_T(CACHEDBLOCK         ), DT_CNV(END      ), NULL                       , DT_NOREL(CLR_RT_HeapBlock_Node                            ) DT_OPT_NAME(CACHEDBLOCK         ) }, // DATATYPE_CACHEDBLOCK
    { DT_REF                                                                  , DT_NA, DT_NA, DT_T(ASSEMBLY            ), DT_CNV(END      ), NULL                       , DT_REL  (CLR_RT_Assembly               ::Relocate         ) DT_OPT_NAME(ASSEMBLY            ) }, // DATATYPE_ASSEMBLY
    { DT_REF                                                           | DT_MT, DT_NA, DT_BL, DT_T(WEAKCLASS           ), DT_CNV(END      ), NULL                       , DT_REL  (CLR_RT_HeapBlock_WeakReference::Relocate         ) DT_OPT_NAME(WEAKCLASS           ) }, // DATATYPE_WEAKCLASS
    {                                                                    DT_MT, DT_NA, DT_NA, DT_T(REFLECTION          ), DT_CNV(END      ), NULL                       , DT_NOREL(CLR_RT_HeapBlock                                 ) DT_OPT_NAME(REFLECTION          ) }, // DATATYPE_REFLECTION
    {                                                                    DT_MT, DT_NA, DT_NA, DT_T(ARRAY_BYREF         ), DT_CNV(END      ), NULL                       , DT_REL  (CLR_RT_HeapBlock              ::Relocate_ArrayRef) DT_OPT_NAME(ARRAY_BYREF         ) }, // DATATYPE_ARRAY_BYREF
    { DT_REF                                                           | DT_MT, DT_NA, DT_BL, DT_T(DELEGATE_HEAD       ), DT_CNV(END      ), NULL                       , DT_REL  (CLR_RT_HeapBlock_Delegate     ::Relocate         ) DT_OPT_NAME(DELEGATE_HEAD       ) }, // DATATYPE_DELEGATE_HEAD
    { DT_REF                                                           | DT_MT, DT_NA, DT_NA, DT_T(DELEGATELIST_HEAD   ), DT_CNV(END      ), NULL                       , DT_REL  (CLR_RT_HeapBlock_Delegate_List::Relocate         ) DT_OPT_NAME(DELEGATELIST_HEAD   ) }, // DATATYPE_DELEGATELIST_HEAD
    { DT_REF                                                                  , DT_NA, DT_NA, DT_T(OBJECT_TO_EVENT     ), DT_CNV(END      ), NULL                       , DT_REL  (CLR_RT_ObjectToEvent_Source   ::Relocate         ) DT_OPT_NAME(OBJECT_TO_EVENT     ) }, // DATATYPE_OBJECT_TO_EVENT
    { DT_REF                                                                  , DT_NA, DT_NA, DT_T(BINARY_BLOB_HEAD    ), DT_CNV(END      ), NULL                       , DT_REL  (CLR_RT_HeapBlock_BinaryBlob   ::Relocate         ) DT_OPT_NAME(BINARY_BLOB_HEAD    ) }, // DATATYPE_BINARY_BLOB_HEAD
                                                                                                                                                                                                                                                                   //
    { DT_REF                                                                  , DT_NA, DT_NA, DT_T(THREAD              ), DT_CNV(END      ), NULL                       , DT_REL  (CLR_RT_Thread                 ::Relocate         ) DT_OPT_NAME(THREAD              ) }, // DATATYPE_THREAD
    { DT_REF                                                                  , DT_NA, DT_NA, DT_T(SUBTHREAD           ), DT_CNV(END      ), NULL                       , DT_REL  (CLR_RT_SubThread              ::Relocate         ) DT_OPT_NAME(SUBTHREAD           ) }, // DATATYPE_SUBTHREAD
    { DT_REF                                                                  , DT_NA, DT_NA, DT_T(STACK_FRAME         ), DT_CNV(END      ), NULL                       , DT_REL  (CLR_RT_StackFrame             ::Relocate         ) DT_OPT_NAME(STACK_FRAME         ) }, // DATATYPE_STACK_FRAME
    { DT_REF                                                                  , DT_NA, DT_NA, DT_T(TIMER_HEAD          ), DT_CNV(END      ), NULL                       , DT_NOREL(CLR_RT_HeapBlock_Timer                           ) DT_OPT_NAME(TIMER_HEAD          ) }, // DATATYPE_TIMER_HEAD
    { DT_REF                                                                  , DT_NA, DT_NA, DT_T(LOCK_HEAD           ), DT_CNV(END      ), NULL                       , DT_REL  (CLR_RT_HeapBlock_Lock         ::Relocate         ) DT_OPT_NAME(LOCK_HEAD           ) }, // DATATYPE_LOCK_HEAD
    { DT_REF                                                                  , DT_NA, DT_NA, DT_T(LOCK_OWNER_HEAD     ), DT_CNV(END      ), NULL                       , DT_REL  (CLR_RT_HeapBlock_Lock         ::Relocate_Owner   ) DT_OPT_NAME(LOCK_OWNER_HEAD     ) }, // DATATYPE_LOCK_OWNER_HEAD
    { DT_REF                                                                  , DT_NA, DT_NA, DT_T(LOCK_REQUEST_HEAD   ), DT_CNV(END      ), NULL                       , DT_REL  (CLR_RT_HeapBlock_LockRequest  ::Relocate         ) DT_OPT_NAME(LOCK_REQUEST_HEAD   ) }, // DATATYPE_LOCK_REQUEST_HEAD
    { DT_REF                                                                  , DT_NA, DT_NA, DT_T(WAIT_FOR_OBJECT_HEAD), DT_CNV(END      ), NULL                       , DT_REL  (CLR_RT_HeapBlock_WaitForObject::Relocate         ) DT_OPT_NAME(WAIT_FOR_OBJECT_HEAD) }, // DATATYPE_WAIT_FOR_OBJECT_HEAD
    { DT_REF                                                                  , DT_NA, DT_NA, DT_T(FINALIZER_HEAD      ), DT_CNV(END      ), NULL                       , DT_REL  (CLR_RT_HeapBlock_Finalizer    ::Relocate         ) DT_OPT_NAME(FINALIZER_HEAD      ) }, // DATATYPE_FINALIZER_HEAD
    { DT_NA                                                                   , DT_NA, DT_NA, DT_T(MEMORY_STREAM_HEAD  ), DT_CNV(END      ), NULL                       , DT_NOREL(CLR_RT_HeapBlock_MemoryStream                    ) DT_OPT_NAME(MEMORY_STREAM_HEAD  ) }, // DATATYPE_MEMORY_STREAM_HEAD
    { DT_NA                                                                   , DT_NA, DT_NA, DT_T(MEMORY_STREAM_DATA  ), DT_CNV(END      ), NULL                       , DT_NOREL(CLR_RT_HeapBlock_MemoryStream                    ) DT_OPT_NAME(MEMORY_STREAM_DATA  ) }, // DATATYPE_MEMORY_STREAM_DATA
                                                                                                                                                                                                                                                                   //
    { DT_NA                                                                   , DT_NA, DT_NA, DT_T(SERIALIZER_HEAD     ), DT_CNV(END      ), NULL                       , DT_NOREL(CLR_RT_BinaryFormatter                           ) DT_OPT_NAME(SERIALIZER_HEAD     ) }, // DATATYPE_SERIALIZER_HEAD
    { DT_NA                                                                   , DT_NA, DT_NA, DT_T(SERIALIZER_DUPLICATE), DT_CNV(END      ), NULL                       , DT_NOREL(CLR_RT_BinaryFormatter                           ) DT_OPT_NAME(SERIALIZER_DUPLICATE) }, // DATATYPE_SERIALIZER_DUPLICATE
    { DT_NA                                                                   , DT_NA, DT_NA, DT_T(SERIALIZER_STATE    ), DT_CNV(END      ), NULL                       , DT_NOREL(CLR_RT_BinaryFormatter                           ) DT_OPT_NAME(SERIALIZER_STATE    ) }, // DATATYPE_SERIALIZER_STATE
                                                                                                                                                                                                                                                                   //
    { DT_REF                                                                  , DT_NA, DT_NA, DT_T(ENDPOINT_HEAD       ), DT_CNV(END      ), NULL                       , DT_NOREL(CLR_RT_HeapBlock_EndPoint                        ) DT_OPT_NAME(ENDPOINT_HEAD       ) }, // DATATYPE_ENDPOINT_HEAD
                                                                                                                                                                                                                                                                   //
    DATATYPE_NOT_SUPPORTED
    DATATYPE_NOT_SUPPORTED
    DATATYPE_NOT_SUPPORTED

    { DT_REF                                                                  , DT_NA, DT_NA, DT_T(IO_PORT             ), DT_CNV(END      ), NULL                       , DT_NOREL(CLR_RT_HeapBlock_NativeEventDispatcher                          ) DT_OPT_NAME(IO_PORT             ) }, // DATATYPE_IO_PORT

    DATATYPE_NOT_SUPPORTED  //VTU_PORT

    { DT_REF                                                                  , DT_NA, DT_NA, DT_T(I2C_XACTION         ), DT_CNV(END      ), NULL                       , DT_NOREL(CLR_RT_HeapBlock_I2CXAction                      ) DT_OPT_NAME(I2C_XACTION         ) }, // DATATYPE_I2C_XACTION

#if defined(TINYCLR_APPDOMAINS)
    { DT_REF                                                                  , DT_NA, DT_NA, DT_T(APPDOMAIN_HEAD      ), DT_CNV(END      ), NULL                       , DT_REL  (CLR_RT_AppDomain::Relocate                       ) DT_OPT_NAME(APPDOMAIN_HEAD      ) }, // DATATYPE_APPDOMAIN_HEAD
    { DT_REF                                                           | DT_MT, DT_NA, DT_NA, DT_T(TRANSPARENT_PROXY   ), DT_CNV(END      ), NULL                       , DT_REL  (CLR_RT_HeapBlock::Relocate_TransparentProxy      ) DT_OPT_NAME(TRANSPARENT_PROXY   ) }, // DATATYPE_TRANSPARENT_PROXY
    { DT_REF                                                                  , DT_NA, DT_NA, DT_T(APPDOMAIN_ASSEMBLY  ), DT_CNV(END      ), NULL                       , DT_REL  (CLR_RT_AppDomainAssembly::Relocate               ) DT_OPT_NAME(APPDOMAIN_ASSEMBLY  ) }, // DATATYPE_APPDOMAIN_ASSEMBLY
#else
    DATATYPE_NOT_SUPPORTED
    DATATYPE_NOT_SUPPORTED
    DATATYPE_NOT_SUPPORTED
#endif
    
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
};

////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////

#define InlineBrTarget              CLR_OpcodeParam_BrTarget
#define InlineField                 CLR_OpcodeParam_Field
#define InlineI                     CLR_OpcodeParam_I
#define InlineI8                    CLR_OpcodeParam_I8
#define InlineMethod                CLR_OpcodeParam_Method
#define InlineNone                  CLR_OpcodeParam_None
#define InlineR                     CLR_OpcodeParam_R
#define InlineSig                   CLR_OpcodeParam_Sig
#define InlineString                CLR_OpcodeParam_String
#define InlineSwitch                CLR_OpcodeParam_Switch
#define InlineTok                   CLR_OpcodeParam_Tok
#define InlineType                  CLR_OpcodeParam_Type
#define InlineVar                   CLR_OpcodeParam_Var
#define ShortInlineBrTarget         CLR_OpcodeParam_ShortBrTarget
#define ShortInlineI                CLR_OpcodeParam_ShortI
#define ShortInlineR                CLR_OpcodeParam_ShortR
#define ShortInlineVar              CLR_OpcodeParam_ShortVar

#define Pop0     0
#define Pop1     1
#define PopI     1
#define PopI8    1
#define PopR4    1
#define PopR8    1
#define PopRef   1

#define Push0    0
#define Push1    1
#define PushI    1
#define PushI8   1
#define PushR4   1
#define PushR8   1
#define PushRef  1

#define VarPop   0
#define VarPush  0

#if defined(TINYCLR_OPCODE_NAMES)
#define OPDEF_NAME(name) name,
#else
#define OPDEF_NAME(name)
#endif

#if defined(TINYCLR_OPCODE_STACKCHANGES)
#define OPDEF_STACKCHANGE(pop,push) ((pop) << 4) | (push),
#else
#define OPDEF_STACKCHANGE(pop,push)
#endif

#if defined(TINYCLR_OPCODE_PARSER)
#define OPDEF_EX(lo,dt,index,flags) , lo, dt, index, flags
#else
#define OPDEF_EX(lo,dt,index,flags)
#endif

#if defined(TINYCLR_JITTER)
#define OPDEF(name,string,pop,push,oprType,opcType,l,s1,s2,ctrl) OPDEF_NAME(string) OPDEF_STACKCHANGE(pop,push) oprType, CLR_FlowControl_##ctrl
#else
#define OPDEF(name,string,pop,push,oprType,opcType,l,s1,s2,ctrl) OPDEF_NAME(string) OPDEF_STACKCHANGE(pop,push) oprType
#endif

//--//

const CLR_RT_OpcodeLookup c_CLR_RT_OpcodeLookup[] =
{
    {
        OPDEF(CEE_NOP,                        "nop",              Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x00,    NEXT)

        OPDEF_EX(LO_Nop, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER)
    },
    {
        OPDEF(CEE_BREAK,                      "break",            Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x01,    BREAK)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER)
    },
    {
        OPDEF(CEE_LDARG_0,                    "ldarg.0",          Pop0,               Push1,       InlineNone,         IMacro,      1,  0xFF,    0x02,    NEXT)

        OPDEF_EX(LO_LoadArgument, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_LDARG_1,                    "ldarg.1",          Pop0,               Push1,       InlineNone,         IMacro,      1,  0xFF,    0x03,    NEXT)

        OPDEF_EX(LO_LoadArgument, DATATYPE_FIRST_INVALID, 1, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_LDARG_2,                    "ldarg.2",          Pop0,               Push1,       InlineNone,         IMacro,      1,  0xFF,    0x04,    NEXT)

        OPDEF_EX(LO_LoadArgument, DATATYPE_FIRST_INVALID, 2, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_LDARG_3,                    "ldarg.3",          Pop0,               Push1,       InlineNone,         IMacro,      1,  0xFF,    0x05,    NEXT)

        OPDEF_EX(LO_LoadArgument, DATATYPE_FIRST_INVALID, 3, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_LDLOC_0,                    "ldloc.0",          Pop0,               Push1,       InlineNone,         IMacro,      1,  0xFF,    0x06,    NEXT)

        OPDEF_EX(LO_LoadLocal, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_LDLOC_1,                    "ldloc.1",          Pop0,               Push1,       InlineNone,         IMacro,      1,  0xFF,    0x07,    NEXT)

        OPDEF_EX(LO_LoadLocal, DATATYPE_FIRST_INVALID, 1, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_LDLOC_2,                    "ldloc.2",          Pop0,               Push1,       InlineNone,         IMacro,      1,  0xFF,    0x08,    NEXT)

        OPDEF_EX(LO_LoadLocal, DATATYPE_FIRST_INVALID, 2, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_LDLOC_3,                    "ldloc.3",          Pop0,               Push1,       InlineNone,         IMacro,      1,  0xFF,    0x09,    NEXT)

        OPDEF_EX(LO_LoadLocal, DATATYPE_FIRST_INVALID, 3, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_STLOC_0,                    "stloc.0",          Pop1,               Push0,       InlineNone,         IMacro,      1,  0xFF,    0x0A,    NEXT)

        OPDEF_EX(LO_StoreLocal, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_STLOC_1,                    "stloc.1",          Pop1,               Push0,       InlineNone,         IMacro,      1,  0xFF,    0x0B,    NEXT)

        OPDEF_EX(LO_StoreLocal, DATATYPE_FIRST_INVALID, 1, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_STLOC_2,                    "stloc.2",          Pop1,               Push0,       InlineNone,         IMacro,      1,  0xFF,    0x0C,    NEXT)

        OPDEF_EX(LO_StoreLocal, DATATYPE_FIRST_INVALID, 2, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_STLOC_3,                    "stloc.3",          Pop1,               Push0,       InlineNone,         IMacro,      1,  0xFF,    0x0D,    NEXT)

        OPDEF_EX(LO_StoreLocal, DATATYPE_FIRST_INVALID, 3, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_LDARG_S,                    "ldarg.s",          Pop0,               Push1,       ShortInlineVar,     IMacro,      1,  0xFF,    0x0E,    NEXT)

        OPDEF_EX(LO_LoadArgument, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_LDARGA_S,                   "ldarga.s",         Pop0,               PushI,       ShortInlineVar,     IMacro,      1,  0xFF,    0x0F,    NEXT)

        OPDEF_EX(LO_LoadArgumentAddress, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_STARG_S,                    "starg.s",          Pop1,               Push0,       ShortInlineVar,     IMacro,      1,  0xFF,    0x10,    NEXT)

        OPDEF_EX(LO_StoreArgument, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_LDLOC_S,                    "ldloc.s",          Pop0,               Push1,       ShortInlineVar,     IMacro,      1,  0xFF,    0x11,    NEXT)

        OPDEF_EX(LO_LoadLocal, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_LDLOCA_S,                   "ldloca.s",         Pop0,               PushI,       ShortInlineVar,     IMacro,      1,  0xFF,    0x12,    NEXT)

        OPDEF_EX(LO_LoadLocalAddress, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_STLOC_S,                    "stloc.s",          Pop1,               Push0,       ShortInlineVar,     IMacro,      1,  0xFF,    0x13,    NEXT)

        OPDEF_EX(LO_StoreLocal, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_LDNULL,                     "ldnull",           Pop0,               PushRef,     InlineNone,         IPrimitive,  1,  0xFF,    0x14,    NEXT)

        OPDEF_EX(LO_LoadNull, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER)
    },
    {
        OPDEF(CEE_LDC_I4_M1,                  "ldc.i4.m1",        Pop0,               PushI,       InlineNone,         IMacro,      1,  0xFF,    0x15,    NEXT)

        OPDEF_EX(LO_LoadConstant_I4, DATATYPE_FIRST_INVALID, -1, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_I4)
    },
    {
        OPDEF(CEE_LDC_I4_0,                   "ldc.i4.0",         Pop0,               PushI,       InlineNone,         IMacro,      1,  0xFF,    0x16,    NEXT)

        OPDEF_EX(LO_LoadConstant_I4, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_I4)
    },
    {
        OPDEF(CEE_LDC_I4_1,                   "ldc.i4.1",         Pop0,               PushI,       InlineNone,         IMacro,      1,  0xFF,    0x17,    NEXT)

        OPDEF_EX(LO_LoadConstant_I4, DATATYPE_FIRST_INVALID, 1, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_I4)
    },
    {
        OPDEF(CEE_LDC_I4_2,                   "ldc.i4.2",         Pop0,               PushI,       InlineNone,         IMacro,      1,  0xFF,    0x18,    NEXT)

        OPDEF_EX(LO_LoadConstant_I4, DATATYPE_FIRST_INVALID, 2, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_I4)
    },
    {
        OPDEF(CEE_LDC_I4_3,                   "ldc.i4.3",         Pop0,               PushI,       InlineNone,         IMacro,      1,  0xFF,    0x19,    NEXT)

        OPDEF_EX(LO_LoadConstant_I4, DATATYPE_FIRST_INVALID, 3, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_I4)
    },
    {
        OPDEF(CEE_LDC_I4_4,                   "ldc.i4.4",         Pop0,               PushI,       InlineNone,         IMacro,      1,  0xFF,    0x1A,    NEXT)

        OPDEF_EX(LO_LoadConstant_I4, DATATYPE_FIRST_INVALID, 4, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_I4)
    },
    {
        OPDEF(CEE_LDC_I4_5,                   "ldc.i4.5",         Pop0,               PushI,       InlineNone,         IMacro,      1,  0xFF,    0x1B,    NEXT)

        OPDEF_EX(LO_LoadConstant_I4, DATATYPE_FIRST_INVALID, 5, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_I4)
    },
    {
        OPDEF(CEE_LDC_I4_6,                   "ldc.i4.6",         Pop0,               PushI,       InlineNone,         IMacro,      1,  0xFF,    0x1C,    NEXT)

        OPDEF_EX(LO_LoadConstant_I4, DATATYPE_FIRST_INVALID, 6, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_I4)
    },
    {
        OPDEF(CEE_LDC_I4_7,                   "ldc.i4.7",         Pop0,               PushI,       InlineNone,         IMacro,      1,  0xFF,    0x1D,    NEXT)

        OPDEF_EX(LO_LoadConstant_I4, DATATYPE_FIRST_INVALID, 7, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_I4)
    },
    {
        OPDEF(CEE_LDC_I4_8,                   "ldc.i4.8",         Pop0,               PushI,       InlineNone,         IMacro,      1,  0xFF,    0x1E,    NEXT)

        OPDEF_EX(LO_LoadConstant_I4, DATATYPE_FIRST_INVALID, 8, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_I4)
    },
    {
        OPDEF(CEE_LDC_I4_S,                   "ldc.i4.s",         Pop0,               PushI,       ShortInlineI,       IMacro,      1,  0xFF,    0x1F,    NEXT)

        OPDEF_EX(LO_LoadConstant_I4, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_I4)
    },
    {
        OPDEF(CEE_LDC_I4,                     "ldc.i4",           Pop0,               PushI,       InlineI,            IPrimitive,  1,  0xFF,    0x20,    NEXT)

        OPDEF_EX(LO_LoadConstant_I4, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_I4)
    },
    {
        OPDEF(CEE_LDC_I8,                     "ldc.i8",           Pop0,               PushI8,      InlineI8,           IPrimitive,  1,  0xFF,    0x21,    NEXT)

        OPDEF_EX(LO_LoadConstant_I8, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_I8)
    },
    {
        OPDEF(CEE_LDC_R4,                     "ldc.r4",           Pop0,               PushR4,      ShortInlineR,       IPrimitive,  1,  0xFF,    0x22,    NEXT)

        OPDEF_EX(LO_LoadConstant_R4, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_R4)
    },
    {
        OPDEF(CEE_LDC_R8,                     "ldc.r8",           Pop0,               PushR8,      InlineR,            IPrimitive,  1,  0xFF,    0x23,    NEXT)

        OPDEF_EX(LO_LoadConstant_R8, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_R8)
    },
    {
        OPDEF(CEE_UNUSED49,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x24,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_DUP,                        "dup",              Pop1,               Push1+Push1, InlineNone,         IPrimitive,  1,  0xFF,    0x25,    NEXT)

        OPDEF_EX(LO_Dup, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER)
    },
    {
        OPDEF(CEE_POP,                        "pop",              Pop1,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x26,    NEXT)

        OPDEF_EX(LO_Pop, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER)
    },
    {
        OPDEF(CEE_JMP,                        "jmp",              Pop0,               Push0,       InlineMethod,       IPrimitive,  1,  0xFF,    0x27,    CALL)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_CALL,                       "call",             VarPop,             VarPush,     InlineMethod,       IPrimitive,  1,  0xFF,    0x28,    CALL)

        OPDEF_EX(LO_Call, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_CALLI,                      "calli",            VarPop,             VarPush,     InlineSig,          IPrimitive,  1,  0xFF,    0x29,    CALL)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_RET,                        "ret",              VarPop,             Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x2A,    RETURN)

        OPDEF_EX(LO_Ret, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_ALWAYS)
    },
    {
        OPDEF(CEE_BR_S,                       "br.s",             Pop0,               Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x2B,    BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_ALWAYS | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BRFALSE_S,                  "brfalse.s",        PopI,               Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x2C,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFFALSE | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BRTRUE_S,                   "brtrue.s",         PopI,               Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x2D,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFTRUE | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BEQ_S,                      "beq.s",            Pop1+Pop1,          Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x2E,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFEQUAL | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BGE_S,                      "bge.s",            Pop1+Pop1,          Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x2F,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFGREATEROREQUAL | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BGT_S,                      "bgt.s",            Pop1+Pop1,          Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x30,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFGREATER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BLE_S,                      "ble.s",            Pop1+Pop1,          Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x31,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFLESSOREQUAL | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BLT_S,                      "blt.s",            Pop1+Pop1,          Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x32,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFLESS | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BNE_UN_S,                   "bne.un.s",         Pop1+Pop1,          Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x33,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFNOTEQUAL | CLR_RT_OpcodeLookup::COND_UNSIGNED | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BGE_UN_S,                   "bge.un.s",         Pop1+Pop1,          Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x34,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFGREATEROREQUAL | CLR_RT_OpcodeLookup::COND_UNSIGNED | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BGT_UN_S,                   "bgt.un.s",         Pop1+Pop1,          Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x35,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFGREATER | CLR_RT_OpcodeLookup::COND_UNSIGNED | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BLE_UN_S,                   "ble.un.s",         Pop1+Pop1,          Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x36,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFLESSOREQUAL | CLR_RT_OpcodeLookup::COND_UNSIGNED | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BLT_UN_S,                   "blt.un.s",         Pop1+Pop1,          Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x37,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFLESS | CLR_RT_OpcodeLookup::COND_UNSIGNED | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BR,                         "br",               Pop0,               Push0,       InlineBrTarget,     IPrimitive,  1,  0xFF,    0x38,    BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_ALWAYS | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BRFALSE,                    "brfalse",          PopI,               Push0,       InlineBrTarget,     IPrimitive,  1,  0xFF,    0x39,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFFALSE | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BRTRUE,                     "brtrue",           PopI,               Push0,       InlineBrTarget,     IPrimitive,  1,  0xFF,    0x3A,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFTRUE | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BEQ,                        "beq",              Pop1+Pop1,          Push0,       InlineBrTarget,     IMacro,      1,  0xFF,    0x3B,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFEQUAL | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BGE,                        "bge",              Pop1+Pop1,          Push0,       InlineBrTarget,     IMacro,      1,  0xFF,    0x3C,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFGREATEROREQUAL | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BGT,                        "bgt",              Pop1+Pop1,          Push0,       InlineBrTarget,     IMacro,      1,  0xFF,    0x3D,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFGREATER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BLE,                        "ble",              Pop1+Pop1,          Push0,       InlineBrTarget,     IMacro,      1,  0xFF,    0x3E,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFLESSOREQUAL | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BLT,                        "blt",              Pop1+Pop1,          Push0,       InlineBrTarget,     IMacro,      1,  0xFF,    0x3F,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFLESS | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BNE_UN,                     "bne.un",           Pop1+Pop1,          Push0,       InlineBrTarget,     IMacro,      1,  0xFF,    0x40,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFNOTEQUAL | CLR_RT_OpcodeLookup::COND_UNSIGNED | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BGE_UN,                     "bge.un",           Pop1+Pop1,          Push0,       InlineBrTarget,     IMacro,      1,  0xFF,    0x41,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFGREATEROREQUAL | CLR_RT_OpcodeLookup::COND_UNSIGNED | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BGT_UN,                     "bgt.un",           Pop1+Pop1,          Push0,       InlineBrTarget,     IMacro,      1,  0xFF,    0x42,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFGREATER | CLR_RT_OpcodeLookup::COND_UNSIGNED | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BLE_UN,                     "ble.un",           Pop1+Pop1,          Push0,       InlineBrTarget,     IMacro,      1,  0xFF,    0x43,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFLESSOREQUAL | CLR_RT_OpcodeLookup::COND_UNSIGNED | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_BLT_UN,                     "blt.un",           Pop1+Pop1,          Push0,       InlineBrTarget,     IMacro,      1,  0xFF,    0x44,    COND_BRANCH)

        OPDEF_EX(LO_Branch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFLESS | CLR_RT_OpcodeLookup::COND_UNSIGNED | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_SWITCH,                     "switch",           PopI,               Push0,       InlineSwitch,       IPrimitive,  1,  0xFF,    0x45,    COND_BRANCH)

        OPDEF_EX(LO_Switch, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFMATCH | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    },
    {
        OPDEF(CEE_LDIND_I1,                   "ldind.i1",         PopI,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x46,    NEXT)

        OPDEF_EX(LO_LoadIndirect, DATATYPE_I1, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_LDIND_U1,                   "ldind.u1",         PopI,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x47,    NEXT)

        OPDEF_EX(LO_LoadIndirect, DATATYPE_U1, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_LDIND_I2,                   "ldind.i2",         PopI,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x48,    NEXT)

        OPDEF_EX(LO_LoadIndirect, DATATYPE_U2, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_LDIND_U2,                   "ldind.u2",         PopI,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x49,    NEXT)

        OPDEF_EX(LO_LoadIndirect, DATATYPE_U2, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_LDIND_I4,                   "ldind.i4",         PopI,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x4A,    NEXT)

        OPDEF_EX(LO_LoadIndirect, DATATYPE_I4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_LDIND_U4,                   "ldind.u4",         PopI,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x4B,    NEXT)

        OPDEF_EX(LO_LoadIndirect, DATATYPE_U4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_LDIND_I8,                   "ldind.i8",         PopI,               PushI8,      InlineNone,         IPrimitive,  1,  0xFF,    0x4C,    NEXT)

        OPDEF_EX(LO_LoadIndirect, DATATYPE_I8, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_LDIND_I,                    "ldind.i",          PopI,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x4D,    NEXT)

        OPDEF_EX(LO_LoadIndirect, DATATYPE_I4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_LDIND_R4,                   "ldind.r4",         PopI,               PushR4,      InlineNone,         IPrimitive,  1,  0xFF,    0x4E,    NEXT)

        OPDEF_EX(LO_LoadIndirect, DATATYPE_R4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_LDIND_R8,                   "ldind.r8",         PopI,               PushR8,      InlineNone,         IPrimitive,  1,  0xFF,    0x4F,    NEXT)

        OPDEF_EX(LO_LoadIndirect, DATATYPE_R8, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_LDIND_REF,                  "ldind.ref",        PopI,               PushRef,     InlineNone,         IPrimitive,  1,  0xFF,    0x50,    NEXT)

        OPDEF_EX(LO_LoadIndirect, DATATYPE_OBJECT, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_STIND_REF,                  "stind.ref",        PopI+PopI,          Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x51,    NEXT)

        OPDEF_EX(LO_StoreIndirect, DATATYPE_OBJECT, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_STIND_I1,                   "stind.i1",         PopI+PopI,          Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x52,    NEXT)

        OPDEF_EX(LO_StoreIndirect, DATATYPE_I1, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_STIND_I2,                   "stind.i2",         PopI+PopI,          Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x53,    NEXT)

        OPDEF_EX(LO_StoreIndirect, DATATYPE_I2, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_STIND_I4,                   "stind.i4",         PopI+PopI,          Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x54,    NEXT)

        OPDEF_EX(LO_StoreIndirect, DATATYPE_I4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_STIND_I8,                   "stind.i8",         PopI+PopI8,         Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x55,    NEXT)

        OPDEF_EX(LO_StoreIndirect, DATATYPE_I8, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_STIND_R4,                   "stind.r4",         PopI+PopR4,         Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x56,    NEXT)

        OPDEF_EX(LO_StoreIndirect, DATATYPE_R4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_STIND_R8,                   "stind.r8",         PopI+PopR8,         Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x57,    NEXT)

        OPDEF_EX(LO_StoreIndirect, DATATYPE_R8, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_ADD,                        "add",              Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x58,    NEXT)

        OPDEF_EX(LO_Add, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER)
    },
    {
        OPDEF(CEE_SUB,                        "sub",              Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x59,    NEXT)

        OPDEF_EX(LO_Sub, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER)
    },
    {
        OPDEF(CEE_MUL,                        "mul",              Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x5A,    NEXT)

        OPDEF_EX(LO_Mul, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER)
    },
    {
        OPDEF(CEE_DIV,                        "div",              Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x5B,    NEXT)

        OPDEF_EX(LO_Div, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER)
    },
    {
        OPDEF(CEE_DIV_UN,                     "div.un",           Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x5C,    NEXT)

        OPDEF_EX(LO_Div, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_UNSIGNED)
    },
    {
        OPDEF(CEE_REM,                        "rem",              Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x5D,    NEXT)

        OPDEF_EX(LO_Rem, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER)
    },
    {
        OPDEF(CEE_REM_UN,                     "rem.un",           Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x5E,    NEXT)

        OPDEF_EX(LO_Rem, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_UNSIGNED)
    },
    {
        OPDEF(CEE_AND,                        "and",              Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x5F,    NEXT)

        OPDEF_EX(LO_And, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER)
    },
    {
        OPDEF(CEE_OR,                         "or",               Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x60,    NEXT)

        OPDEF_EX(LO_Or, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER)
    },
    {
        OPDEF(CEE_XOR,                        "xor",              Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x61,    NEXT)

        OPDEF_EX(LO_Xor, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER)
    },
    {
        OPDEF(CEE_SHL,                        "shl",              Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x62,    NEXT)

        OPDEF_EX(LO_Shl, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER)
    },
    {
        OPDEF(CEE_SHR,                        "shr",              Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x63,    NEXT)

        OPDEF_EX(LO_Shr, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER)
    },
    {
        OPDEF(CEE_SHR_UN,                     "shr.un",           Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x64,    NEXT)

        OPDEF_EX(LO_Shr, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_UNSIGNED)
    },
    {
        OPDEF(CEE_NEG,                        "neg",              Pop1,               Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x65,    NEXT)

        OPDEF_EX(LO_Neg, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER)
    },
    {
        OPDEF(CEE_NOT,                        "not",              Pop1,               Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x66,    NEXT)

        OPDEF_EX(LO_Not, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER)
    },
    {
        OPDEF(CEE_CONV_I1,                    "conv.i1",          Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x67,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_I1, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_I2,                    "conv.i2",          Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x68,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_I2, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_I4,                    "conv.i4",          Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x69,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_I4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_I8,                    "conv.i8",          Pop1,               PushI8,      InlineNone,         IPrimitive,  1,  0xFF,    0x6A,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_I8, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_R4,                    "conv.r4",          Pop1,               PushR4,      InlineNone,         IPrimitive,  1,  0xFF,    0x6B,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_R4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_R8,                    "conv.r8",          Pop1,               PushR8,      InlineNone,         IPrimitive,  1,  0xFF,    0x6C,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_R8, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_U4,                    "conv.u4",          Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x6D,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_U4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_U8,                    "conv.u8",          Pop1,               PushI8,      InlineNone,         IPrimitive,  1,  0xFF,    0x6E,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_U8, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CALLVIRT,                   "callvirt",         VarPop,             VarPush,     InlineMethod,       IObjModel,   1,  0xFF,    0x6F,    CALL)

        OPDEF_EX(LO_CallVirt, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_CPOBJ,                      "cpobj",            PopI+PopI,          Push0,       InlineType,         IObjModel,   1,  0xFF,    0x70,    NEXT)

        OPDEF_EX(LO_CopyObject, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_LDOBJ,                      "ldobj",            PopI,               Push1,       InlineType,         IObjModel,   1,  0xFF,    0x71,    NEXT)

        OPDEF_EX(LO_LoadObject, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_LDSTR,                      "ldstr",            Pop0,               PushRef,     InlineString,       IObjModel,   1,  0xFF,    0x72,    NEXT)

        OPDEF_EX(LO_LoadString, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_NEWOBJ,                     "newobj",           VarPop,             PushRef,     InlineMethod,       IObjModel,   1,  0xFF,    0x73,    CALL)

        OPDEF_EX(LO_NewObject, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_CASTCLASS,                  "castclass",        PopRef,             PushRef,     InlineType,         IObjModel,   1,  0xFF,    0x74,    NEXT)

        OPDEF_EX(LO_CastClass, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_ISINST,                     "isinst",           PopRef,             PushI,       InlineType,         IObjModel,   1,  0xFF,    0x75,    NEXT)

        OPDEF_EX(LO_IsInst, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_CONV_R_UN,                  "conv.r.un",        Pop1,               PushR8,      InlineNone,         IPrimitive,  1,  0xFF,    0x76,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_R4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_UNSIGNED | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_UNUSED58,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x77,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED1,                    "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x78,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNBOX,                      "unbox",            PopRef,             PushI,       InlineType,         IPrimitive,  1,  0xFF,    0x79,    NEXT)

        OPDEF_EX(LO_Unbox, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_THROW,                      "throw",            PopRef,             Push0,       InlineNone,         IObjModel,   1,  0xFF,    0x7A,    THROW)

        OPDEF_EX(LO_Throw, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_ALWAYS | CLR_RT_OpcodeLookup::STACK_RESET)
    },
    {
        OPDEF(CEE_LDFLD,                      "ldfld",            PopRef,             Push1,       InlineField,        IObjModel,   1,  0xFF,    0x7B,    NEXT)

        OPDEF_EX(LO_LoadField, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_LDFLDA,                     "ldflda",           PopRef,             PushI,       InlineField,        IObjModel,   1,  0xFF,    0x7C,    NEXT)

        OPDEF_EX(LO_LoadFieldAddress, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_STFLD,                      "stfld",            PopRef+Pop1,        Push0,       InlineField,        IObjModel,   1,  0xFF,    0x7D,    NEXT)

        OPDEF_EX(LO_StoreField, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_LDSFLD,                     "ldsfld",           Pop0,               Push1,       InlineField,        IObjModel,   1,  0xFF,    0x7E,    NEXT)

        OPDEF_EX(LO_LoadStaticField, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_LDSFLDA,                    "ldsflda",          Pop0,               PushI,       InlineField,        IObjModel,   1,  0xFF,    0x7F,    NEXT)

        OPDEF_EX(LO_LoadStaticFieldAddress, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_STSFLD,                     "stsfld",           Pop1,               Push0,       InlineField,        IObjModel,   1,  0xFF,    0x80,    NEXT)

        OPDEF_EX(LO_StoreStaticField, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_STOBJ,                      "stobj",            PopI+Pop1,          Push0,       InlineType,         IPrimitive,  1,  0xFF,    0x81,    NEXT)

        OPDEF_EX(LO_StoreObject, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_CONV_OVF_I1_UN,             "conv.ovf.i1.un",   Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x82,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_I1, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW | CLR_RT_OpcodeLookup::COND_UNSIGNED | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_OVF_I2_UN,             "conv.ovf.i2.un",   Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x83,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_I2, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW | CLR_RT_OpcodeLookup::COND_UNSIGNED | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_OVF_I4_UN,             "conv.ovf.i4.un",   Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x84,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_I4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW | CLR_RT_OpcodeLookup::COND_UNSIGNED | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_OVF_I8_UN,             "conv.ovf.i8.un",   Pop1,               PushI8,      InlineNone,         IPrimitive,  1,  0xFF,    0x85,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_I8, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW | CLR_RT_OpcodeLookup::COND_UNSIGNED | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_OVF_U1_UN,             "conv.ovf.u1.un",   Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x86,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_U1, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW | CLR_RT_OpcodeLookup::COND_UNSIGNED | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_OVF_U2_UN,             "conv.ovf.u2.un",   Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x87,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_U2, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW | CLR_RT_OpcodeLookup::COND_UNSIGNED | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_OVF_U4_UN,             "conv.ovf.u4.un",   Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x88,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_U4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW | CLR_RT_OpcodeLookup::COND_UNSIGNED | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_OVF_U8_UN,             "conv.ovf.u8.un",   Pop1,               PushI8,      InlineNone,         IPrimitive,  1,  0xFF,    0x89,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_U8, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW | CLR_RT_OpcodeLookup::COND_UNSIGNED | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_OVF_I_UN,              "conv.ovf.i.un",    Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x8A,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_I4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW | CLR_RT_OpcodeLookup::COND_UNSIGNED | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_OVF_U_UN,              "conv.ovf.u.un",    Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x8B,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_U4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW | CLR_RT_OpcodeLookup::COND_UNSIGNED | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_BOX,                        "box",              Pop1,               PushRef,     InlineType,         IPrimitive,  1,  0xFF,    0x8C,    NEXT)

        OPDEF_EX(LO_Box, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_NEWARR,                     "newarr",           PopI,               PushRef,     InlineType,         IObjModel,   1,  0xFF,    0x8D,    NEXT)

        OPDEF_EX(LO_NewArray, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_LDLEN,                      "ldlen",            PopRef,             PushI,       InlineNone,         IObjModel,   1,  0xFF,    0x8E,    NEXT)

        OPDEF_EX(LO_LoadLength, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER)
    },
    {
        OPDEF(CEE_LDELEMA,                    "ldelema",          PopRef+PopI,        PushI,       InlineType,         IObjModel,   1,  0xFF,    0x8F,    NEXT)

        OPDEF_EX(LO_LoadElementAddress, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_LDELEM_I1,                  "ldelem.i1",        PopRef+PopI,        PushI,       InlineNone,         IObjModel,   1,  0xFF,    0x90,    NEXT)

        OPDEF_EX(LO_LoadElement, DATATYPE_I1, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_LDELEM_U1,                  "ldelem.u1",        PopRef+PopI,        PushI,       InlineNone,         IObjModel,   1,  0xFF,    0x91,    NEXT)

        OPDEF_EX(LO_LoadElement, DATATYPE_U1, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_LDELEM_I2,                  "ldelem.i2",        PopRef+PopI,        PushI,       InlineNone,         IObjModel,   1,  0xFF,    0x92,    NEXT)

        OPDEF_EX(LO_LoadElement, DATATYPE_I2, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_LDELEM_U2,                  "ldelem.u2",        PopRef+PopI,        PushI,       InlineNone,         IObjModel,   1,  0xFF,    0x93,    NEXT)

        OPDEF_EX(LO_LoadElement, DATATYPE_U2, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_LDELEM_I4,                  "ldelem.i4",        PopRef+PopI,        PushI,       InlineNone,         IObjModel,   1,  0xFF,    0x94,    NEXT)

        OPDEF_EX(LO_LoadElement, DATATYPE_I4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_LDELEM_U4,                  "ldelem.u4",        PopRef+PopI,        PushI,       InlineNone,         IObjModel,   1,  0xFF,    0x95,    NEXT)

        OPDEF_EX(LO_LoadElement, DATATYPE_U4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_LDELEM_I8,                  "ldelem.i8",        PopRef+PopI,        PushI8,      InlineNone,         IObjModel,   1,  0xFF,    0x96,    NEXT)

        OPDEF_EX(LO_LoadElement, DATATYPE_I8, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_LDELEM_I,                   "ldelem.i",         PopRef+PopI,        PushI,       InlineNone,         IObjModel,   1,  0xFF,    0x97,    NEXT)

        OPDEF_EX(LO_LoadElement, DATATYPE_I4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_LDELEM_R4,                  "ldelem.r4",        PopRef+PopI,        PushR4,      InlineNone,         IObjModel,   1,  0xFF,    0x98,    NEXT)

        OPDEF_EX(LO_LoadElement, DATATYPE_R4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_LDELEM_R8,                  "ldelem.r8",        PopRef+PopI,        PushR8,      InlineNone,         IObjModel,   1,  0xFF,    0x99,    NEXT)

        OPDEF_EX(LO_LoadElement, DATATYPE_R8, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_LDELEM_REF,                 "ldelem.ref",       PopRef+PopI,        PushRef,     InlineNone,         IObjModel,   1,  0xFF,    0x9A,    NEXT)

        OPDEF_EX(LO_LoadElement, DATATYPE_OBJECT, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_STELEM_I,                   "stelem.i",         PopRef+PopI+PopI,   Push0,       InlineNone,         IObjModel,   1,  0xFF,    0x9B,    NEXT)

        OPDEF_EX(LO_StoreElement, DATATYPE_I4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_STELEM_I1,                  "stelem.i1",        PopRef+PopI+PopI,   Push0,       InlineNone,         IObjModel,   1,  0xFF,    0x9C,    NEXT)

        OPDEF_EX(LO_StoreElement, DATATYPE_I1, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_STELEM_I2,                  "stelem.i2",        PopRef+PopI+PopI,   Push0,       InlineNone,         IObjModel,   1,  0xFF,    0x9D,    NEXT)

        OPDEF_EX(LO_StoreElement, DATATYPE_I2, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_STELEM_I4,                  "stelem.i4",        PopRef+PopI+PopI,   Push0,       InlineNone,         IObjModel,   1,  0xFF,    0x9E,    NEXT)

        OPDEF_EX(LO_StoreElement, DATATYPE_I4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_STELEM_I8,                  "stelem.i8",        PopRef+PopI+PopI8,  Push0,       InlineNone,         IObjModel,   1,  0xFF,    0x9F,    NEXT)

        OPDEF_EX(LO_StoreElement, DATATYPE_I8, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_STELEM_R4,                  "stelem.r4",        PopRef+PopI+PopR4,  Push0,       InlineNone,         IObjModel,   1,  0xFF,    0xA0,    NEXT)

        OPDEF_EX(LO_StoreElement, DATATYPE_R4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_STELEM_R8,                  "stelem.r8",        PopRef+PopI+PopR8,  Push0,       InlineNone,         IObjModel,   1,  0xFF,    0xA1,    NEXT)

        OPDEF_EX(LO_StoreElement, DATATYPE_R8, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_STELEM_REF,                 "stelem.ref",       PopRef+PopI+PopRef, Push0,       InlineNone,         IObjModel,   1,  0xFF,    0xA2,    NEXT)

        OPDEF_EX(LO_StoreElement, DATATYPE_OBJECT, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_LDELEM,                     "ldelem",           PopRef+PopI,        Push1,       InlineType,         IObjModel,   1,  0xFF,    0xA3,    NEXT)
        OPDEF_EX(LO_LoadElement, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_STELEM,                     "stelem",           PopRef+PopI+Pop1,   Push0,       InlineType,         IObjModel,   1,  0xFF,    0xA4,    NEXT)

        OPDEF_EX(LO_StoreElement, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_UNBOX_ANY,                  "unbox.any",        PopRef,             Push1,       InlineType,         IObjModel,   1,  0xFF,    0xA5,    NEXT)

        OPDEF_EX(LO_Unbox, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)        
    },
    {
        OPDEF(CEE_UNUSED5,                    "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xA6,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED6,                    "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xA7,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED7,                    "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xA8,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED8,                    "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xA9,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED9,                    "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xAA,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED10,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xAB,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED11,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xAC,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED12,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xAD,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED13,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xAE,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED14,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xAF,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED15,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xB0,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED16,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xB1,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED17,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xB2,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_CONV_OVF_I1,                "conv.ovf.i1",      Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xB3,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_I1, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_OVF_U1,                "conv.ovf.u1",      Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xB4,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_U1, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_OVF_I2,                "conv.ovf.i2",      Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xB5,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_I2, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_OVF_U2,                "conv.ovf.u2",      Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xB6,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_U2, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_OVF_I4,                "conv.ovf.i4",      Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xB7,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_I4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_OVF_U4,                "conv.ovf.u4",      Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xB8,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_U4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_OVF_I8,                "conv.ovf.i8",      Pop1,               PushI8,      InlineNone,         IPrimitive,  1,  0xFF,    0xB9,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_I8, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_OVF_U8,                "conv.ovf.u8",      Pop1,               PushI8,      InlineNone,         IPrimitive,  1,  0xFF,    0xBA,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_U8, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_UNUSED50,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xBB,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED18,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xBC,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED19,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xBD,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED20,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xBE,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED21,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xBF,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED22,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xC0,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED23,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xC1,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_REFANYVAL,                  "refanyval",        Pop1,               PushI,       InlineType,         IPrimitive,  1,  0xFF,    0xC2,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_CKFINITE,                   "ckfinite",         Pop1,               PushR8,      InlineNone,         IPrimitive,  1,  0xFF,    0xC3,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED24,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xC4,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED25,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xC5,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_MKREFANY,                   "mkrefany",         PopI,               Push1,       InlineType,         IPrimitive,  1,  0xFF,    0xC6,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED59,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xC7,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED60,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xC8,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED61,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xC9,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED62,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xCA,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED63,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xCB,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED64,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xCC,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED65,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xCD,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED66,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xCE,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED67,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xCF,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_LDTOKEN,                    "ldtoken",          Pop0,               PushI,       InlineTok,          IPrimitive,  1,  0xFF,    0xD0,    NEXT)

        OPDEF_EX(LO_LoadToken, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_CONV_U2,                    "conv.u2",          Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xD1,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_U2, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_U1,                    "conv.u1",          Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xD2,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_U1, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_I,                     "conv.i",           Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xD3,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_I4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_OVF_I,                 "conv.ovf.i",       Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xD4,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_I4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_OVF_U,                 "conv.ovf.u",       Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xD5,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_U4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_ADD_OVF,                    "add.ovf",          Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0xD6,    NEXT)

        OPDEF_EX(LO_Add, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW)
    },
    {
        OPDEF(CEE_ADD_OVF_UN,                 "add.ovf.un",       Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0xD7,    NEXT)

        OPDEF_EX(LO_Add, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW | CLR_RT_OpcodeLookup::COND_UNSIGNED)
    },
    {
        OPDEF(CEE_MUL_OVF,                    "mul.ovf",          Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0xD8,    NEXT)

        OPDEF_EX(LO_Mul, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW)
    },
    {
        OPDEF(CEE_MUL_OVF_UN,                 "mul.ovf.un",       Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0xD9,    NEXT)

        OPDEF_EX(LO_Mul, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW | CLR_RT_OpcodeLookup::COND_UNSIGNED)
    },
    {
        OPDEF(CEE_SUB_OVF,                    "sub.ovf",          Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0xDA,    NEXT)

        OPDEF_EX(LO_Sub, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW)
    },
    {
        OPDEF(CEE_SUB_OVF_UN,                 "sub.ovf.un",       Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0xDB,    NEXT)

        OPDEF_EX(LO_Sub, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::COND_OVERFLOW | CLR_RT_OpcodeLookup::COND_UNSIGNED)
    },
    {
        OPDEF(CEE_ENDFINALLY,                 "endfinally",       Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xDC,    RETURN)

        OPDEF_EX(LO_EndFinally, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_ALWAYS | CLR_RT_OpcodeLookup::STACK_RESET)
    },
    {
        OPDEF(CEE_LEAVE,                      "leave",            Pop0,               Push0,       InlineBrTarget,     IPrimitive,  1,  0xFF,    0xDD,    BRANCH)

        OPDEF_EX(LO_Leave, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_ALWAYS | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET | CLR_RT_OpcodeLookup::STACK_RESET)
    },
    {
        OPDEF(CEE_LEAVE_S,                    "leave.s",          Pop0,               Push0,       ShortInlineBrTarget,IPrimitive,  1,  0xFF,    0xDE,    BRANCH)

        OPDEF_EX(LO_Leave, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_ALWAYS | CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET | CLR_RT_OpcodeLookup::STACK_RESET)
    },
    {
        OPDEF(CEE_STIND_I,                    "stind.i",          PopI+PopI,          Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xDF,    NEXT)

        OPDEF_EX(LO_StoreIndirect, DATATYPE_I4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_CONV_U,                     "conv.u",           Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xE0,    NEXT)

        OPDEF_EX(LO_Convert, DATATYPE_U4, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_DT)
    },
    {
        OPDEF(CEE_UNUSED26,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xE1,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED27,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xE2,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED28,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xE3,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED29,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xE4,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED30,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xE5,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED31,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xE6,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED32,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xE7,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED33,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xE8,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED34,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xE9,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED35,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xEA,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED36,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xEB,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED37,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xEC,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED38,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xED,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED39,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xEE,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED40,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xEF,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED41,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xF0,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED42,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xF1,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED43,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xF2,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED44,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xF3,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED45,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xF4,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED46,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xF5,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED47,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xF6,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED48,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xF7,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_PREFIX7,                    "prefix7",          Pop0,               Push0,       InlineNone,         IInternal,   1,  0xFF,    0xF8,    META)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_PREFIX6,                    "prefix6",          Pop0,               Push0,       InlineNone,         IInternal,   1,  0xFF,    0xF9,    META)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_PREFIX5,                    "prefix5",          Pop0,               Push0,       InlineNone,         IInternal,   1,  0xFF,    0xFA,    META)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_PREFIX4,                    "prefix4",          Pop0,               Push0,       InlineNone,         IInternal,   1,  0xFF,    0xFB,    META)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_PREFIX3,                    "prefix3",          Pop0,               Push0,       InlineNone,         IInternal,   1,  0xFF,    0xFC,    META)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_PREFIX2,                    "prefix2",          Pop0,               Push0,       InlineNone,         IInternal,   1,  0xFF,    0xFD,    META)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_PREFIX1,                    "prefix1",          Pop0,               Push0,       InlineNone,         IInternal,   1,  0xFF,    0xFE,    META)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_PREFIXREF,                  "prefixref",        Pop0,               Push0,       InlineNone,         IInternal,   1,  0xFF,    0xFF,    META)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_ARGLIST,                    "arglist",          Pop0,               PushI,       InlineNone,         IPrimitive,  2,  0xFE,    0x00,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_CEQ,                        "ceq",              Pop1+Pop1,          PushI,       InlineNone,         IPrimitive,  2,  0xFE,    0x01,    NEXT)

        OPDEF_EX(LO_Set, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFEQUAL)
    },
    {
        OPDEF(CEE_CGT,                        "cgt",              Pop1+Pop1,          PushI,       InlineNone,         IPrimitive,  2,  0xFE,    0x02,    NEXT)

        OPDEF_EX(LO_Set, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFGREATER)
    },
    {
        OPDEF(CEE_CGT_UN,                     "cgt.un",           Pop1+Pop1,          PushI,       InlineNone,         IPrimitive,  2,  0xFE,    0x03,    NEXT)

        OPDEF_EX(LO_Set, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFGREATER | CLR_RT_OpcodeLookup::COND_UNSIGNED)
    },
    {
        OPDEF(CEE_CLT,                        "clt",              Pop1+Pop1,          PushI,       InlineNone,         IPrimitive,  2,  0xFE,    0x04,    NEXT)

        OPDEF_EX(LO_Set, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFLESS)
    },
    {
        OPDEF(CEE_CLT_UN,                     "clt.un",           Pop1+Pop1,          PushI,       InlineNone,         IPrimitive,  2,  0xFE,    0x05,    NEXT)

        OPDEF_EX(LO_Set, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_IFLESS | CLR_RT_OpcodeLookup::COND_UNSIGNED)
    },
    {
        OPDEF(CEE_LDFTN,                      "ldftn",            Pop0,               PushI,       InlineMethod,       IPrimitive,  2,  0xFE,    0x06,    NEXT)

        OPDEF_EX(LO_LoadFunction, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_LDVIRTFTN,                  "ldvirtftn",        PopRef,             PushI,       InlineMethod,       IPrimitive,  2,  0xFE,    0x07,    NEXT)

        OPDEF_EX(LO_LoadVirtFunction, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_UNUSED56,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  2,  0xFE,    0x08,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_LDARG,                      "ldarg",            Pop0,               Push1,       InlineVar,          IPrimitive,  2,  0xFE,    0x09,    NEXT)

        OPDEF_EX(LO_LoadArgument, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_LDARGA,                     "ldarga",           Pop0,               PushI,       InlineVar,          IPrimitive,  2,  0xFE,    0x0A,    NEXT)

        OPDEF_EX(LO_LoadArgumentAddress, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_STARG,                      "starg",            Pop1,               Push0,       InlineVar,          IPrimitive,  2,  0xFE,    0x0B,    NEXT)

        OPDEF_EX(LO_StoreArgument, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_LDLOC,                      "ldloc",            Pop0,               Push1,       InlineVar,          IPrimitive,  2,  0xFE,    0x0C,    NEXT)

        OPDEF_EX(LO_LoadLocal, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_LDLOCA,                     "ldloca",           Pop0,               PushI,       InlineVar,          IPrimitive,  2,  0xFE,    0x0D,    NEXT)

        OPDEF_EX(LO_LoadLocalAddress, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_STLOC,                      "stloc",            Pop1,               Push0,       InlineVar,          IPrimitive,  2,  0xFE,    0x0E,    NEXT)

        OPDEF_EX(LO_StoreLocal, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
    },
    {
        OPDEF(CEE_LOCALLOC,                   "localloc",         PopI,               PushI,       InlineNone,         IPrimitive,  2,  0xFE,    0x0F,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED57,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  2,  0xFE,    0x10,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_ENDFILTER,                  "endfilter",        PopI,               Push0,       InlineNone,         IPrimitive,  2,  0xFE,    0x11,    RETURN)

        OPDEF_EX(LO_EndFilter, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_ALWAYS | CLR_RT_OpcodeLookup::STACK_RESET)
    },
    {
        OPDEF(CEE_UNALIGNED,                  "unaligned.",       Pop0,               Push0,       ShortInlineI,       IPrefix,     2,  0xFE,    0x12,    META)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_VOLATILE,                   "volatile.",        Pop0,               Push0,       InlineNone,         IPrefix,     2,  0xFE,    0x13,    META)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_TAILCALL,                   "tail.",            Pop0,               Push0,       InlineNone,         IPrefix,     2,  0xFE,    0x14,    META)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_INITOBJ,                    "initobj",          PopI,               Push0,       InlineType,         IObjModel,   2,  0xFE,    0x15,    NEXT)

        OPDEF_EX(LO_InitObject, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
    },
    {
        OPDEF(CEE_CONSTRAINED,                "constrained.",     Pop0,               Push0,       InlineType,         IPrefix,     2,  0xFE,    0x16,    META)

        OPDEF_EX(LO_Nop, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_NEVER | CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)        
    },
    {
        OPDEF(CEE_CPBLK,                      "cpblk",            PopI+PopI+PopI,     Push0,       InlineNone,         IPrimitive,  2,  0xFE,    0x17,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_INITBLK,                    "initblk",          PopI+PopI+PopI,     Push0,       InlineNone,         IPrimitive,  2,  0xFE,    0x18,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED69,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  2,  0xFE,    0x19,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_RETHROW,                    "rethrow",          Pop0,               Push0,       InlineNone,         IObjModel,   2,  0xFE,    0x1A,    THROW)

        OPDEF_EX(LO_Rethrow, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_ALWAYS)
    },
    {
        OPDEF(CEE_UNUSED51,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  2,  0xFE,    0x1B,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_SIZEOF,                     "sizeof",           Pop0,               PushI,       InlineType,         IPrimitive,  2,  0xFE,    0x1C,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_REFANYTYPE,                 "refanytype",       Pop1,               PushI,       InlineNone,         IPrimitive,  2,  0xFE,    0x1D,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_READONLY,                   "readonly.",           Pop0,               Push0,       InlineNone,         IPrefix,     2,  0xFE,    0x1E,    META)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED53,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  2,  0xFE,    0x1F,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED54,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  2,  0xFE,    0x20,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED55,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  2,  0xFE,    0x21,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
    {
        OPDEF(CEE_UNUSED70,                   "unused",           Pop0,               Push0,       InlineNone,         IPrimitive,  2,  0xFE,    0x22,    NEXT)

        OPDEF_EX(LO_Unsupported, DATATYPE_FIRST_INVALID, 0, CLR_RT_OpcodeLookup::COND_BRANCH_THROW)
    },
};

#undef OPDEF_NAME
#undef OPDEF_STACKCHANGE
#undef OPDEF_EX
#undef OPDEF

//--//

#undef LoadString // Legacy Windows macro, whatever...

#if defined(TINYCLR_OPCODE_NAMES)
#define OPDEF_NAME(name) #name,
#else
#define OPDEF_NAME(name)
#endif

#define OPDEF(name,flags) OPDEF_NAME(name) flags

#define VAL_NONE     0
#define VAL_RP       CLR_RT_LogicalOpcodeLookup::RESTARTPOINT_NEXT
#define VAL_EX       CLR_RT_LogicalOpcodeLookup::EXCEPTION
#define VAL_EX_ZERO  CLR_RT_LogicalOpcodeLookup::EXCEPTION_IF_ZERO
#define VAL_EX_OVF   CLR_RT_LogicalOpcodeLookup::EXCEPTION_IF_OVERFLOW

const CLR_RT_LogicalOpcodeLookup c_CLR_RT_LogicalOpcodeLookup[] =
{
    OPDEF(Not                   , VAL_NONE       ),    // LO_Not                       = 0x00,
    OPDEF(And                   , VAL_NONE       ),    // LO_And                       = 0x01,
    OPDEF(Or                    , VAL_NONE       ),    // LO_Or                        = 0x02,
    OPDEF(Xor                   , VAL_NONE       ),    // LO_Xor                       = 0x03,
    OPDEF(Shl                   , VAL_NONE       ),    // LO_Shl                       = 0x04,
    OPDEF(Shr                   , VAL_NONE       ),    // LO_Shr                       = 0x05,

    OPDEF(Neg                   , VAL_NONE       ),    // LO_Neg                       = 0x06,
    OPDEF(Add                   , VAL_EX_OVF     ),    // LO_Add                       = 0x07,
    OPDEF(Sub                   , VAL_EX_OVF     ),    // LO_Sub                       = 0x08,
    OPDEF(Mul                   , VAL_EX_OVF     ),    // LO_Mul                       = 0x09,
    OPDEF(Div                   , VAL_EX_ZERO    ),    // LO_Div                       = 0x0A,
    OPDEF(Rem                   , VAL_EX_ZERO    ),    // LO_Rem                       = 0x0B,

    OPDEF(Box                   , VAL_EX         ),    // LO_Box                       = 0x0C,
    OPDEF(Unbox                 , VAL_EX         ),    // LO_Unbox                     = 0x0D,

    OPDEF(Branch                , VAL_NONE       ),    // LO_Branch                    = 0x0E,
    OPDEF(Set                   , VAL_NONE       ),    // LO_Set                       = 0x0F,
    OPDEF(Switch                , VAL_NONE       ),    // LO_Switch                    = 0x10,

    OPDEF(LoadFunction          , VAL_EX         ),    // LO_LoadFunction              = 0x11,
    OPDEF(LoadVirtFunction      , VAL_EX         ),    // LO_LoadVirtFunction          = 0x12,

    OPDEF(Call                  , VAL_EX | VAL_RP),    // LO_Call                      = 0x13,
    OPDEF(CallVirt              , VAL_EX | VAL_RP),    // LO_CallVirt                  = 0x14,
    OPDEF(Ret                   , VAL_NONE       ),    // LO_Ret                       = 0x15,

    OPDEF(NewObject             , VAL_EX | VAL_RP),    // LO_NewObject                 = 0x16,
    OPDEF(CastClass             , VAL_EX         ),    // LO_CastClass                 = 0x17,
    OPDEF(IsInst                , VAL_NONE       ),    // LO_IsInst                    = 0x18,

    OPDEF(Dup                   , VAL_NONE       ),    // LO_Dup                       = 0x19,
    OPDEF(Pop                   , VAL_NONE       ),    // LO_Pop                       = 0x1A,

    OPDEF(Throw                 , VAL_EX         ),    // LO_Throw                     = 0x1B,
    OPDEF(Rethrow               , VAL_EX         ),    // LO_Rethrow                   = 0x1C,
    OPDEF(Leave                 , VAL_NONE       ),    // LO_Leave                     = 0x1D,
    OPDEF(EndFinally            , VAL_EX         ),    // LO_EndFinally                = 0x1E,

    OPDEF(Convert               , VAL_EX_OVF     ),    // LO_Convert                   = 0x1F,

    OPDEF(StoreArgument         , VAL_NONE       ),    // LO_StoreArgument             = 0x20,
    OPDEF(LoadArgument          , VAL_NONE       ),    // LO_LoadArgument              = 0x21,
    OPDEF(LoadArgumentAddress   , VAL_NONE       ),    // LO_LoadArgumentAddress       = 0x22,

    OPDEF(StoreLocal            , VAL_NONE       ),    // LO_StoreLocal                = 0x23,
    OPDEF(LoadLocal             , VAL_NONE       ),    // LO_LoadLocal                 = 0x24,
    OPDEF(LoadLocalAddress      , VAL_NONE       ),    // LO_LoadLocalAddress          = 0x25,

    OPDEF(LoadConstant_I4       , VAL_NONE       ),    // LO_LoadConstant_I4           = 0x26,
    OPDEF(LoadConstant_I8       , VAL_NONE       ),    // LO_LoadConstant_I8           = 0x27,
    OPDEF(LoadConstant_R4       , VAL_NONE       ),    // LO_LoadConstant_R4           = 0x28,
    OPDEF(LoadConstant_R8       , VAL_NONE       ),    // LO_LoadConstant_R8           = 0x29,

    OPDEF(LoadNull              , VAL_NONE       ),    // LO_LoadNull                  = 0x2A,
    OPDEF(LoadString            , VAL_EX         ),    // LO_LoadString                = 0x2B,
    OPDEF(LoadToken             , VAL_NONE       ),    // LO_LoadToken                 = 0x2C,

    OPDEF(NewArray              , VAL_EX         ),    // LO_NewArray                  = 0x2D,
    OPDEF(LoadLength            , VAL_EX         ),    // LO_LoadLength                = 0x2E,

    OPDEF(StoreElement          , VAL_EX         ),    // LO_StoreElement              = 0x2F,
    OPDEF(LoadElement           , VAL_EX         ),    // LO_LoadElement               = 0x30,
    OPDEF(LoadElementAddress    , VAL_EX         ),    // LO_LoadElementAddress        = 0x31,

    OPDEF(StoreField            , VAL_EX         ),    // LO_StoreField                = 0x32,
    OPDEF(LoadField             , VAL_EX         ),    // LO_LoadField                 = 0x33,
    OPDEF(LoadFieldAddress      , VAL_EX         ),    // LO_LoadFieldAddress          = 0x34,

    OPDEF(StoreStaticField      , VAL_NONE       ),    // LO_StoreStaticField          = 0x35,
    OPDEF(LoadStaticField       , VAL_NONE       ),    // LO_LoadStaticField           = 0x36,
    OPDEF(LoadStaticFieldAddress, VAL_NONE       ),    // LO_LoadStaticFieldAddress    = 0x37,

    OPDEF(StoreIndirect         , VAL_EX         ),    // LO_StoreIndirect             = 0x38,
    OPDEF(LoadIndirect          , VAL_EX         ),    // LO_LoadIndirect              = 0x39,

    OPDEF(InitObject            , VAL_NONE       ),    // LO_InitObject                = 0x3A,
    OPDEF(LoadObject            , VAL_EX         ),    // LO_LoadObject                = 0x3B,
    OPDEF(CopyObject            , VAL_EX         ),    // LO_CopyObject                = 0x3C,
    OPDEF(StoreObject           , VAL_EX         ),    // LO_StoreObject               = 0x3D,

    OPDEF(Nop                   , VAL_NONE       ),    // LO_Nop                       = 0x3E,

    OPDEF(EndFilter             , VAL_EX         ),    // LO_EndFilter                 = 0x3F,

    OPDEF(Unsupported           , VAL_NONE       ),    // LO_Unsupported               = 0x40,
};
