////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _TINYCLR_JITTER_H_
#define _TINYCLR_JITTER_H_

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_JITTER)

#if !defined(BUILD_RTM)
#define TINYCLR_DUMP_JITTER_INLINE
#define TINYCLR_JITTER_STATISTICS
#endif

////////////////////////////////////////////////////////////////////////////////////////////////////

#define CLR_E_JITTER_OPCODE_UNSUPPORTED                      MAKE_HRESULT( SEVERITY_ERROR, 0x8000, 0x0000 )
#define CLR_E_JITTER_OPCODE_INVALID_SIGNATURE                MAKE_HRESULT( SEVERITY_ERROR, 0x8100, 0x0000 )
#define CLR_E_JITTER_OPCODE_INVALID_TOKEN__TYPE              MAKE_HRESULT( SEVERITY_ERROR, 0x8200, 0x0000 )
#define CLR_E_JITTER_OPCODE_INVALID_TOKEN__METHOD            MAKE_HRESULT( SEVERITY_ERROR, 0x8300, 0x0000 )
#define CLR_E_JITTER_OPCODE_INVALID_TOKEN__FIELD             MAKE_HRESULT( SEVERITY_ERROR, 0x8400, 0x0000 )
#define CLR_E_JITTER_OPCODE_INVALID_OFFSET                   MAKE_HRESULT( SEVERITY_ERROR, 0x8500, 0x0000 )
#define CLR_E_JITTER_OPCODE_INVALID_EVALUATION_STACK         MAKE_HRESULT( SEVERITY_ERROR, 0x8600, 0x0000 )

////////////////////////////////////////////////////////////////////////////////////////////////////

class TypedArrayGeneric
{
protected:
    void*  m_ptr;
    size_t m_num;

public:
    void    Initialize(                        );
    void    Release   (                        );
    HRESULT Allocate  ( size_t num, size_t len );

    //--//

    bool operator!() const { return m_ptr == NULL; }

    size_t Length() const { return m_num; }

    TypedArrayGeneric() {}

private:
    TypedArrayGeneric( const TypedArrayGeneric&  );
    TypedArrayGeneric& operator=( const TypedArrayGeneric& );
};

//////////////////////////////////////////////////

template <typename T> class TypedArray : public TypedArrayGeneric
{
public:
    TypedArray() {}

    operator T*() const { return (T*)m_ptr; }

    HRESULT Allocate( size_t num )
    {
        return TypedArrayGeneric::Allocate( num, sizeof(T) );
    }


private:
    TypedArray( const TypedArray&  );
    TypedArray& operator=( const TypedArray& );
};

//////////////////////////////////////////////////

class TypedQueueGeneric
{
    static const size_t c_default = 16;

protected:
    void*  m_ptr;
    size_t m_size;
    size_t m_num;
    size_t m_pos;

public:
    void  Initialize( size_t size           );
    void  Release   (                       );
    void* Reserve   ( size_t reserve        );
    void  Clear     (                       );
    void* Insert    ( size_t pos            );
    bool  Remove    ( size_t pos, void* ptr );
    void* Top       (                       );
    bool  Pop       (             void* ptr ) { return Remove( m_pos-1, ptr ); }
    void* Push      (                       ) { return Insert( m_pos        ); }

    //--//

    size_t Size() const { return m_pos; }

    void* GetElement( size_t pos ) const;

    TypedQueueGeneric() {}

private:
    TypedQueueGeneric( const TypedQueueGeneric&  );
    TypedQueueGeneric& operator=( const TypedQueueGeneric& );
};

//////////////////////////////////////////////////

template <typename T> class TypedQueue : public TypedQueueGeneric
{
public:
    TypedQueue() {}

    void Initialize(                    ) {            TypedQueueGeneric::Initialize( sizeof(T) ); }
    T*   Insert    ( size_t pos         ) { return (T*)TypedQueueGeneric::Insert    ( pos       ); }
    bool Remove    ( size_t pos, T* ptr ) { return     TypedQueueGeneric::Remove    ( pos, ptr  ); }
    T*   Top       (                    ) { return (T*)TypedQueueGeneric::Top       (           ); }
    bool Pop       (             T* ptr ) { return     TypedQueueGeneric::Pop       (      ptr  ); }
    T*   Push      (                    ) { return (T*)TypedQueueGeneric::Push      (           ); }

    operator T*() const { return (T*)m_ptr; }

private:
    TypedQueue( const TypedQueue&  );
    TypedQueue& operator=( const TypedQueue& );
};

////////////////////////////////////////////////////////////////////////////////////////////////////

struct JitterExternalCalls
{
    CLR_INT32         (                   *m_fpn__CLR_RT_HeapBlock__Compare_Values        )( const CLR_RT_HeapBlock& left, const CLR_RT_HeapBlock& right, bool fSigned                            ); // = &CLR_RT_HeapBlock     ::Compare_Values_Impl

    HRESULT           (CLR_RT_HeapBlock ::*m_fpn__CLR_RT_HeapBlock__NumericMul            )( const CLR_RT_HeapBlock& right                                                                        ); // = &CLR_RT_HeapBlock     ::NumericMul_Impl
    HRESULT           (CLR_RT_HeapBlock ::*m_fpn__CLR_RT_HeapBlock__NumericDiv            )( const CLR_RT_HeapBlock& right                                                                        ); // = &CLR_RT_HeapBlock     ::NumericDiv_Impl
    HRESULT           (CLR_RT_HeapBlock ::*m_fpn__CLR_RT_HeapBlock__NumericDivUn          )( const CLR_RT_HeapBlock& right                                                                        ); // = &CLR_RT_HeapBlock     ::NumericDivUn_Impl
    HRESULT           (CLR_RT_HeapBlock ::*m_fpn__CLR_RT_HeapBlock__NumericRem            )( const CLR_RT_HeapBlock& right                                                                        ); // = &CLR_RT_HeapBlock     ::NumericRem_Impl
    HRESULT           (CLR_RT_HeapBlock ::*m_fpn__CLR_RT_HeapBlock__NumericRemUn          )( const CLR_RT_HeapBlock& right                                                                        ); // = &CLR_RT_HeapBlock     ::NumericRemUn_Impl
    HRESULT           (CLR_RT_HeapBlock ::*m_fpn__CLR_RT_HeapBlock__NumericShl            )( const CLR_RT_HeapBlock& right                                                                        ); // = &CLR_RT_HeapBlock     ::NumericShl_Impl
    HRESULT           (CLR_RT_HeapBlock ::*m_fpn__CLR_RT_HeapBlock__NumericShr            )( const CLR_RT_HeapBlock& right                                                                        ); // = &CLR_RT_HeapBlock     ::NumericShr_Impl
    HRESULT           (CLR_RT_HeapBlock ::*m_fpn__CLR_RT_HeapBlock__NumericShrUn          )( const CLR_RT_HeapBlock& right                                                                        ); // = &CLR_RT_HeapBlock     ::NumericShrUn_Impl
    bool              (CLR_RT_HeapBlock ::*m_fpn__CLR_RT_HeapBlock__InitObject            )(                                                                                                      ); // = &CLR_RT_HeapBlock     ::InitObject
    HRESULT           (CLR_RT_HeapBlock ::*m_fpn__CLR_RT_HeapBlock__Convert               )( CLR_DataType et, bool fOverflow, bool fUnsigned                                                      ); // = &CLR_RT_HeapBlock     ::Convert

    HRESULT           (                   *m_fpn__MethodCompilerHelpers__HandleBoxing     )( CLR_RT_HeapBlock& ref, CLR_UINT32 type, bool fBox                                                    ); // = &MethodCompilerHelpers::HandleBoxing
    HRESULT           (                   *m_fpn__MethodCompilerHelpers__HandleCasting    )( CLR_RT_HeapBlock& ref, CLR_UINT32 type, CLR_UINT32 levels, bool fIsInst                              ); // = &MethodCompilerHelpers::HandleCasting
    HRESULT           (                   *m_fpn__MethodCompilerHelpers__CopyValueType    )( CLR_RT_HeapBlock& refDst, const CLR_RT_HeapBlock& refSrc                                             ); // = &MethodCompilerHelpers::CopyValueType
    HRESULT           (                   *m_fpn__MethodCompilerHelpers__CloneValueType   )( CLR_RT_HeapBlock& ref                                                                                ); // = &MethodCompilerHelpers::CloneValueType
    HRESULT           (                   *m_fpn__MethodCompilerHelpers__LoadFunction     )( CLR_RT_StackFrame* stack, CLR_UINT32 method, CLR_RT_HeapBlock& refDlg, CLR_RT_HeapBlock* ptrInstance ); // = &MethodCompilerHelpers::LoadFunction
    HRESULT           (                   *m_fpn__MethodCompilerHelpers__LoadString       )( CLR_RT_StackFrame* stack, CLR_UINT32 string, CLR_RT_HeapBlock& refStr                                ); // = &MethodCompilerHelpers::LoadString
    HRESULT           (                   *m_fpn__MethodCompilerHelpers__NewArray         )( CLR_RT_HeapBlock& ref, CLR_UINT32 type, CLR_UINT32 levels, CLR_UINT32 size                           ); // = &MethodCompilerHelpers::NewArray

    HRESULT           (                   *m_fpn__MethodCompilerHelpers__Call             )( CLR_RT_StackFrame* stack, CLR_UINT32 method, CLR_RT_HeapBlock* firstArgument                         ); // = &MethodCompilerHelpers::Call
    HRESULT           (                   *m_fpn__MethodCompilerHelpers__CallVirtual      )( CLR_RT_StackFrame* stack, CLR_UINT32 method, CLR_RT_HeapBlock* firstArgument                         ); // = &MethodCompilerHelpers::CallVirtual
    HRESULT           (                   *m_fpn__MethodCompilerHelpers__NewObject        )( CLR_RT_StackFrame* stack, CLR_UINT32 method, CLR_RT_HeapBlock* firstArgument                         ); // = &MethodCompilerHelpers::NewObject
    HRESULT           (                   *m_fpn__MethodCompilerHelpers__NewDelegate      )( CLR_RT_StackFrame* stack, CLR_UINT32 method, CLR_RT_HeapBlock* firstArgument                         ); // = &MethodCompilerHelpers::NewObject
    void              (                   *m_fpn__MethodCompilerHelpers__Pop              )( CLR_RT_StackFrame* stack                                                                             ); // = &MethodCompilerHelpers::NewObject

    CLR_RT_HeapBlock* (                   *m_fpn__MethodCompilerHelpers__AccessStaticField)( CLR_UINT32 field                                                                                     ); // = &MethodCompilerHelpers::AccessStaticField

    HRESULT           (                   *m_fpn__MethodCompilerHelpers__Throw            )( CLR_RT_StackFrame* stack, CLR_RT_HeapBlock* ex                                                       ); // = &MethodCompilerHelpers::Throw
    HRESULT           (                   *m_fpn__MethodCompilerHelpers__Rethrow          )( CLR_RT_StackFrame* stack                                                                             ); // = &MethodCompilerHelpers::Rethrow
    CLR_PMETADATA     (                   *m_fpn__MethodCompilerHelpers__Leave            )( CLR_RT_StackFrame* stack, CLR_PMETADATA from, CLR_PMETADATA to                                       ); // = &MethodCompilerHelpers::Leave
    HRESULT           (                   *m_fpn__MethodCompilerHelpers__EndFinally       )( CLR_RT_StackFrame* stack                                                                             ); // = &MethodCompilerHelpers::EndFinally

    HRESULT           (                   *m_fpn__MethodCompilerHelpers__LoadIndirect     )( CLR_RT_HeapBlock& ref                                                                                ); // = &MethodCompilerHelpers::LoadIndirect
    HRESULT           (                   *m_fpn__MethodCompilerHelpers__StoreIndirect    )( CLR_RT_HeapBlock& refDst, CLR_RT_HeapBlock& refVal, CLR_UINT32 op                                    ); // = &MethodCompilerHelpers::StoreIndirect

    HRESULT           (                   *m_fpn__MethodCompilerHelpers__LoadObject       )( CLR_RT_HeapBlock& ref                                                                                ); // = &MethodCompilerHelpers::LoadObject
    HRESULT           (                   *m_fpn__MethodCompilerHelpers__CopyObject       )( CLR_RT_HeapBlock& refDst, CLR_RT_HeapBlock& refSrc                                                   ); // = &MethodCompilerHelpers::CopyObject
    HRESULT           (                   *m_fpn__MethodCompilerHelpers__StoreObject      )( CLR_RT_HeapBlock& refDst, CLR_RT_HeapBlock& refVal                                                   ); // = &MethodCompilerHelpers::StoreObject

};

#define JITTER_HELPER_FPN(seq,cls,method) (*(CLR_UINT32*)&s_JitterExternalCalls.m_fpn__##cls##__##method)

extern JitterExternalCalls s_JitterExternalCalls;

////////////////////////////////////////////////////////////////////////////////////////////////////

struct JitterThunkTable
{
    CLR_UINT32 m_address__Internal_Initialize                     ;
    CLR_UINT32 m_address__Internal_Restart                        ;
    CLR_UINT32 m_address__Internal_Error                          ;
    CLR_UINT32 m_address__Internal_ErrorNoFlush                   ;
    CLR_UINT32 m_address__Internal_ReturnFromMethod               ;

    CLR_UINT32 m_address__CLR_RT_HeapBlock__Compare_Values        ;

    CLR_UINT32 m_address__CLR_RT_HeapBlock__NumericMul            ;
    CLR_UINT32 m_address__CLR_RT_HeapBlock__NumericDiv            ;
    CLR_UINT32 m_address__CLR_RT_HeapBlock__NumericDivUn          ;
    CLR_UINT32 m_address__CLR_RT_HeapBlock__NumericRem            ;
    CLR_UINT32 m_address__CLR_RT_HeapBlock__NumericRemUn          ;
    CLR_UINT32 m_address__CLR_RT_HeapBlock__NumericShl            ;
    CLR_UINT32 m_address__CLR_RT_HeapBlock__NumericShr            ;
    CLR_UINT32 m_address__CLR_RT_HeapBlock__NumericShrUn          ;
    CLR_UINT32 m_address__CLR_RT_HeapBlock__InitObject            ;
    CLR_UINT32 m_address__CLR_RT_HeapBlock__Convert               ;

    CLR_UINT32 m_address__MethodCompilerHelpers__HandleBoxing     ;
    CLR_UINT32 m_address__MethodCompilerHelpers__HandleIsInst     ;
    CLR_UINT32 m_address__MethodCompilerHelpers__HandleCasting    ;
    CLR_UINT32 m_address__MethodCompilerHelpers__CopyValueType    ;
    CLR_UINT32 m_address__MethodCompilerHelpers__CloneValueType   ;
    CLR_UINT32 m_address__MethodCompilerHelpers__LoadFunction     ;
    CLR_UINT32 m_address__MethodCompilerHelpers__LoadString       ;
    CLR_UINT32 m_address__MethodCompilerHelpers__NewArray         ;

    CLR_UINT32 m_address__MethodCompilerHelpers__Call             ;
    CLR_UINT32 m_address__MethodCompilerHelpers__CallVirtual      ;
    CLR_UINT32 m_address__MethodCompilerHelpers__NewObject        ;
    CLR_UINT32 m_address__MethodCompilerHelpers__NewDelegate      ;

    CLR_UINT32 m_address__MethodCompilerHelpers__AccessStaticField;

    CLR_UINT32 m_address__MethodCompilerHelpers__Throw            ;
    CLR_UINT32 m_address__MethodCompilerHelpers__Rethrow          ;
    CLR_UINT32 m_address__MethodCompilerHelpers__Leave            ;
    CLR_UINT32 m_address__MethodCompilerHelpers__EndFinally       ;

    CLR_UINT32 m_address__MethodCompilerHelpers__LoadIndirect     ;
    CLR_UINT32 m_address__MethodCompilerHelpers__StoreIndirect    ;

    CLR_UINT32 m_address__MethodCompilerHelpers__LoadObject       ;
    CLR_UINT32 m_address__MethodCompilerHelpers__CopyObject       ;
    CLR_UINT32 m_address__MethodCompilerHelpers__StoreObject      ;
};

extern JitterThunkTable g_thunkTable;

////////////////////////////////////////////////////////////////////////////////////////////////////

#include <TinyCLR_Jitter_ARM.h>

#if defined(TINYCLR_JITTER_ARMEMULATION)
#include <TinyCLR_Jitter_ARM_Emulation.h>
#endif

////////////////////////////////////////////////////////////////////////////////////////////////////

struct MethodCompilerHelpers
{
    static HRESULT HandleBoxing( CLR_RT_HeapBlock& ref, CLR_UINT32 type, bool fBox );

    //--//

    static HRESULT HandleCasting( CLR_RT_HeapBlock& ref, CLR_UINT32 type, CLR_UINT32 levels, bool fIsInst );

    //--//

    static HRESULT CopyValueType( CLR_RT_HeapBlock& refDst, const CLR_RT_HeapBlock& refSrc );

    //--//

    static HRESULT CloneValueType( CLR_RT_HeapBlock& ref );

    //--//

    static HRESULT LoadFunction( CLR_RT_StackFrame* stack, CLR_UINT32 method, CLR_RT_HeapBlock& refDlg, CLR_RT_HeapBlock* ptrInstance );

    //--//

    static HRESULT LoadString( CLR_RT_StackFrame* stack, CLR_UINT32 string, CLR_RT_HeapBlock& refStr );

    //--//

    static HRESULT NewArray( CLR_RT_HeapBlock& ref, CLR_UINT32 type, CLR_UINT32 levels, CLR_UINT32 size );

    //--//

    static HRESULT Call       ( CLR_RT_StackFrame* stack, CLR_UINT32 method, CLR_RT_HeapBlock* firstArgument );
    static HRESULT CallVirtual( CLR_RT_StackFrame* stack, CLR_UINT32 method, CLR_RT_HeapBlock* firstArgument );
    static HRESULT NewObject  ( CLR_RT_StackFrame* stack, CLR_UINT32 method, CLR_RT_HeapBlock* firstArgument );
    static HRESULT NewDelegate( CLR_RT_StackFrame* stack, CLR_UINT32 method, CLR_RT_HeapBlock* firstArgument );
    static void    Pop        ( CLR_RT_StackFrame* stack                                                     );

    //--//

    static CLR_RT_HeapBlock* AccessStaticField( CLR_UINT32 field );

    //--//

    static HRESULT       Throw     ( CLR_RT_StackFrame* stack, CLR_RT_HeapBlock* ex                 );
    static HRESULT       Rethrow   ( CLR_RT_StackFrame* stack                                       );
    static CLR_PMETADATA Leave     ( CLR_RT_StackFrame* stack, CLR_PMETADATA from, CLR_PMETADATA to );
    static HRESULT       EndFinally( CLR_RT_StackFrame* stack                                       );

    //--//

    static HRESULT LoadIndirect ( CLR_RT_HeapBlock& ref                                             );
    static HRESULT StoreIndirect( CLR_RT_HeapBlock& refDst, CLR_RT_HeapBlock& refVal, CLR_UINT32 op );

    //--//

    static HRESULT LoadObject ( CLR_RT_HeapBlock& ref                              );
    static HRESULT CopyObject ( CLR_RT_HeapBlock& refDst, CLR_RT_HeapBlock& refSrc );
    static HRESULT StoreObject( CLR_RT_HeapBlock& refDst, CLR_RT_HeapBlock& refVal );
};

struct MethodCompiler
{
    struct OpcodeSlot
    {
        static const CLR_UINT8 c_EntryPoint       = 0x01;
        static const CLR_UINT8 c_ExceptionHandler = 0x02;
        static const CLR_UINT8 c_BranchForward    = 0x04;
        static const CLR_UINT8 c_BranchBackward   = 0x08;
        static const CLR_UINT8 c_UNUSED_0010      = 0x10;
        static const CLR_UINT8 c_UNUSED_0020      = 0x20;
        static const CLR_UINT8 c_UNUSED_0040      = 0x40;
        static const CLR_UINT8 c_UNUSED_0080      = 0x80;

        static const CLR_UINT8 c_Target           = c_EntryPoint | c_ExceptionHandler | c_BranchForward | c_BranchBackward;

        static const size_t    c_MaxDepth         = 127;

        //--//

        CLR_OPCODE m_op;
        CLR_OFFSET m_ipOffset;
        //
        CLR_UINT8  m_flags;
        CLR_UINT8  m_stackPop;
        CLR_UINT8  m_stackPush;
        CLR_INT8   m_stackDepth;
        //
        CLR_OFFSET m_stackIdx;
        CLR_UINT8  m_branchesOut;
        CLR_UINT8  m_branchesIn;
        //
        CLR_OFFSET m_branchesOutIdx;
        CLR_OFFSET m_branchesInIdx;
    };

    struct Opcode
    {
        CLR_OPCODE                 m_op;
        const CLR_RT_OpcodeLookup* m_ol;

        CLR_UINT32                 m_ipOffset;
        CLR_UINT32                 m_ipLength;

        CLR_INT32                  m_stackPop;
        CLR_INT32                  m_stackPush;

        CLR_UINT32                 m_token;
        CLR_RT_HeapBlock           m_value;

        CLR_UINT32                 m_numTargets;

        CLR_RT_TypeDef_Instance    m_tdInst;
        int                        m_tdInstLevels;
        CLR_RT_MethodDef_Instance  m_mdInst;
        CLR_RT_FieldDef_Instance   m_fdInst;

        //--//

        HRESULT Initialize( MethodCompiler* mc, CLR_OFFSET offset, CLR_OFFSET* targets );

        CLR_UINT32 Index() const { return m_value.NumericByRefConst().u4; }
    };

    //--//

    struct TypeDescriptor
    {
        static const CLR_UINT16 c_ByRef      = 0x0001;
        static const CLR_UINT16 c_ByRefArray = 0x0002;
        static const CLR_UINT16 c_Boxed      = 0x0004;
        static const CLR_UINT16 c_Null       = 0x0008;

        struct Promotion
        {
            CLR_DataType m_dt;
            CLR_UINT32   m_size;
            bool         m_fSigned;
        };

        //--//

        CLR_RT_TypeDef_Index m_cls;
        CLR_INT16            m_levels;
        CLR_UINT16           m_flags;

        //--//

        HRESULT Parse( CLR_RT_SignatureParser& parser );

        bool NeedsCloning  (                            );
        bool NeedsPromotion( Promotion& pr, bool fStore );

        void InitializeFromIndex   ( CLR_RT_TypeDef_Index& idx );
        void InitializeFromDataType( CLR_DataType          dt  );

        bool ConvertFromTypeDescriptor( const CLR_RT_TypeDescriptor& td );
        bool ConvertToTypeDescriptor  (       CLR_RT_TypeDescriptor& td );

        static bool IsCompatible( TypeDescriptor* left, TypeDescriptor* right );

    private:
        bool IsInstanceOf( TypeDescriptor* target );
    };

    //--//

#if defined(TINYCLR_JITTER_STATISTICS)
    struct Statistics
    {
        CLR_UINT32 m_methods;
        CLR_UINT32 m_size;

        CLR_UINT32 m_opcodes[ LO_LAST-LO_FIRST+1 ];

        CLR_UINT32 m_exceptionHandlers;
        CLR_UINT32 m_localVariables;
        CLR_UINT32 m_methodsWithValueTypeLocals;

        void Dump( MethodCompiler& mc, Statistics& tot );
        void Dump(                                     );
    };

    static Statistics s_statistics;

#define TINYCLR_JITTER_STATISTICS_EXECUTE(cmd) cmd

#else

#define TINYCLR_JITTER_STATISTICS_EXECUTE(cmd)

#endif

    //--//

    static const CLR_UINT32 c_pushlist = ArmProcessor::c_register_lst_r4 | ArmProcessor::c_register_lst_r5 | ArmProcessor::c_register_lst_r6 | ArmProcessor::c_register_lst_r7 | ArmProcessor::c_register_lst_r8 | ArmProcessor::c_register_lst_lr;
    static const CLR_UINT32 c_poplist  = ArmProcessor::c_register_lst_r4 | ArmProcessor::c_register_lst_r5 | ArmProcessor::c_register_lst_r6 | ArmProcessor::c_register_lst_r7 | ArmProcessor::c_register_lst_r8 | ArmProcessor::c_register_lst_pc;

    static const CLR_UINT32 c_sizeOfTimeQuantumCode = 4 * sizeof(CLR_UINT32);

    //--//

    CLR_RT_MethodDef_Instance    m_mdInst;
    CLR_RT_TypeDef_Instance      m_clsInst;

    TypeDescriptor               m_returnValue;
    TypedArray<TypeDescriptor>   m_arguments;
    TypedArray<TypeDescriptor>   m_locals;

    //--//

    CLR_PMETADATA                m_ipStart;
    CLR_PMETADATA                m_ipEnd;

    size_t                       m_numOpcodes;
    TypedArray<OpcodeSlot>       m_opcodeSlots;

    size_t                       m_numBranches;
    TypedArray<CLR_OFFSET>       m_opcodeBranches;

    TypedArray<CLR_RECORD_EH>    m_EHs;

    //--//

    TypedQueue<TypeDescriptor>   m_stackTypes;
    TypedQueue<TypeDescriptor>   m_stackStatus;

    //--//

    TypedArray<CLR_UINT32>       m_indexToArmOpcode;

    //--//

    JitterThunkTable*            m_Arm_Thunks;

    CLR_UINT32                   m_Arm_BaseAddress;

    TypedQueue<CLR_UINT32>       m_Arm_Opcodes;
    TypedQueue<CLR_UINT32>       m_Arm_ROData;
    CLR_UINT32                   m_Arm_ROData_Base;
    ArmProcessor::Opcode         m_Arm_Op;
    bool                         m_Arm_shiftUseReg;
    CLR_UINT32                   m_Arm_shiftType;
    CLR_UINT32                   m_Arm_shiftValue;
    CLR_UINT32                   m_Arm_shiftReg;
    bool                         m_Arm_setCC;

#if defined(TINYCLR_DUMP_JITTER_INLINE)
    bool                         m_fDump_JitterInline;
#endif

    //--//

    MethodCompiler() {}

    HRESULT Initialize( const CLR_RT_MethodDef_Index* md, CLR_UINT32 baseAddress );
    void    Release   (                                                          );

    //--//

    HRESULT CreateThunks ( JitterThunkTable* tbl );
    void    ReferToThunks( JitterThunkTable* tbl );

    //--//

    HRESULT OffsetToIndex( CLR_OFFSET& idx, CLR_UINT32 offset );

    HRESULT ProcessEvaluationStack( CLR_OFFSET pos, CLR_UINT32 flags );

    HRESULT SaveStackStatus   ( OpcodeSlot& ess, bool& fSame );
    HRESULT RestoreStackStatus( OpcodeSlot& ess              );

    //--//

    HRESULT ParseByteCode ();
    HRESULT ParseArguments();
    HRESULT ParseLocals   ();
    HRESULT ParseEvalStack();

    HRESULT GenerateCode();

    //--//

    void DumpOpcode      ( size_t pos );
    void DumpOpcodes     (            );
    void DumpJitterOutput(            );

    //--//

    TypeDescriptor* GetFirstStackElement( const OpcodeSlot* os ) { return &m_stackTypes[ os->m_stackIdx                                     ]; }
    TypeDescriptor* GetLastStackElement ( const OpcodeSlot* os ) { return &m_stackTypes[ os->m_stackIdx + os->m_stackDepth - 1              ]; }
    TypeDescriptor* GetFirstOperand     ( const OpcodeSlot* os ) { return &m_stackTypes[ os->m_stackIdx + os->m_stackDepth - os->m_stackPop ]; }

    //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

    HRESULT    Arm_SetShift_Immediate( CLR_UINT32 shiftType, CLR_UINT32 shiftValue );
    HRESULT    Arm_SetShift_Register ( CLR_UINT32 shiftType, CLR_UINT32 shiftReg   );
    void       Arm_SetCond           ( CLR_UINT32 cond                             ) {        m_Arm_Op.m_conditionCodes = cond; }
    CLR_UINT32 Arm_GetCond           (                                             ) { return m_Arm_Op.m_conditionCodes;        }
    void       Arm_SetCC             (                                             ) {        m_Arm_setCC = true;               }

    CLR_UINT32 Arm_AbsoluteOffset   ( CLR_UINT32 offset ) { return offset - Arm_CurrentAbsolutePC();                        }
    CLR_UINT32 Arm_RelativeOffset   ( CLR_UINT32 offset ) { return offset - Arm_CurrentRelativePC();                        }
    CLR_UINT32 Arm_CurrentAbsolutePC(                   ) { return m_Arm_BaseAddress + Arm_CurrentRelativePC();             }
    CLR_UINT32 Arm_CurrentRelativePC(                   ) { return (CLR_UINT32)(m_Arm_Opcodes.Size() * sizeof(CLR_UINT32)); }

    void    Arm_Reset     (                                      );
    HRESULT Arm_Emit      ( CLR_UINT32 data                      );
    HRESULT Arm_EmitData  ( CLR_UINT32 data                      );
    HRESULT Arm_EmitOpcode(                                      );
    HRESULT Arm_AddData   ( CLR_UINT32 data, CLR_UINT32& address );

    //--//

    CLR_INT32 Arm_GetEndOfEvalStack     ( OpcodeSlot* osPtr                                                    ) { return Arm_GetOpcodeOperandOffset( osPtr, osPtr->m_stackPop, 0 ); }
    CLR_INT32 Arm_GetOpcodeOperandOffset( OpcodeSlot* osPtr, CLR_UINT32 idx,                  CLR_INT32 offset );
    HRESULT   Arm_GetOpcodeOperand      ( OpcodeSlot* osPtr, CLR_UINT32 idx, CLR_UINT32 Rdst, CLR_INT32 offset );

    //--//

    HRESULT Arm_Alu    ( CLR_UINT32 alu, CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_UINT32 Rop2 );
    HRESULT Arm_Alu_IMM( CLR_UINT32 alu, CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_INT32  Vop2 );

    //--//

    HRESULT Arm_STMFD_SP( CLR_UINT32 lst );
    HRESULT Arm_LDMFD_SP( CLR_UINT32 lst );

    HRESULT Arm_LDMIA( CLR_UINT32 Raddress, CLR_INT32 offset, CLR_UINT32 lst, CLR_UINT32 Rtmp );
    HRESULT Arm_LDMIA( CLR_UINT32 Raddress,                   CLR_UINT32 lst                  );
    HRESULT Arm_STMIA( CLR_UINT32 Raddress, CLR_INT32 offset, CLR_UINT32 lst, CLR_UINT32 Rtmp );
    HRESULT Arm_STMIA( CLR_UINT32 Raddress,                   CLR_UINT32 lst                  );

    HRESULT Arm_LDMIB( CLR_UINT32 Raddress, CLR_INT32 offset, CLR_UINT32 lst, CLR_UINT32 Rtmp );
    HRESULT Arm_LDMIB( CLR_UINT32 Raddress,                   CLR_UINT32 lst                  );
    HRESULT Arm_STMIB( CLR_UINT32 Raddress, CLR_INT32 offset, CLR_UINT32 lst, CLR_UINT32 Rtmp );
    HRESULT Arm_STMIB( CLR_UINT32 Raddress,                   CLR_UINT32 lst                  );

    HRESULT Arm_LDR_REG( CLR_UINT32 Rvalue, CLR_UINT32 Raddress, CLR_UINT32 Rindex                                );
    HRESULT Arm_LDR    ( CLR_UINT32 Rvalue, CLR_UINT32 Raddress, CLR_INT32  offset                                );
    HRESULT Arm_LDR    ( CLR_UINT32 Rvalue, CLR_UINT32 Raddress, CLR_INT32  offset, CLR_UINT32 size, bool fSigned );
    HRESULT Arm_LDR_PC ( CLR_UINT32 Rvalue,                      CLR_UINT32 offset                                );

    HRESULT Arm_STR    ( CLR_UINT32 Rvalue, CLR_UINT32 Raddress, CLR_INT32  offset                                );
    HRESULT Arm_STR    ( CLR_UINT32 Rvalue, CLR_UINT32 Raddress, CLR_INT32  offset, CLR_UINT32 size, bool fSigned );

    HRESULT Arm_MUL3   ( CLR_UINT32 Rdst  , CLR_UINT32 Rop                                                        );
    HRESULT Arm_B      ( bool       fLink ,                      CLR_INT32  offset                                );

    HRESULT Arm_LongMovIMM             ( CLR_UINT32 Rdst, CLR_UINT32 value   );

    HRESULT Arm_LongBranch             ( CLR_UINT32 address, bool       fLink    );
    HRESULT Arm_BranchForwardOrBackward( size_t     posFrom, size_t     posTo    );
    HRESULT Arm_FaultOnNull            ( size_t     pos    , CLR_UINT32 Rd       );
    HRESULT Arm_CheckHResult           ( size_t     pos                          );
    HRESULT Arm_Ret                    ( size_t     pos    , bool       fRestart );
    HRESULT Arm_FlushEvalStackPointer  ( size_t     pos    , CLR_UINT32 Rtmp     );

    //--//

    HRESULT Arm_CheckArrayAccess  ( size_t pos, CLR_RT_TypeDescriptor& td, const CLR_RT_DataTypeLookup*& dtlRes );
    HRESULT Arm_FindArrayElement  ( size_t pos,                            const CLR_RT_DataTypeLookup*  dtl    );
    HRESULT Arm_LoadElementAddress( size_t pos,                            const CLR_RT_DataTypeLookup*  dtl    );
    HRESULT Arm_LoadElement       ( size_t pos,                            const CLR_RT_DataTypeLookup*  dtl    );
    HRESULT Arm_StoreElement      ( size_t pos,                            const CLR_RT_DataTypeLookup*  dtl    );

    //--//

    HRESULT Arm_AND( CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_UINT32 Rop2 ) {                     return Arm_Alu( ArmProcessor::c_operation_AND, Rdst, Rop1, Rop2 ); }
    HRESULT Arm_EOR( CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_UINT32 Rop2 ) {                     return Arm_Alu( ArmProcessor::c_operation_EOR, Rdst, Rop1, Rop2 ); }
    HRESULT Arm_SUB( CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_UINT32 Rop2 ) {                     return Arm_Alu( ArmProcessor::c_operation_SUB, Rdst, Rop1, Rop2 ); }
    HRESULT Arm_RSB( CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_UINT32 Rop2 ) {                     return Arm_Alu( ArmProcessor::c_operation_RSB, Rdst, Rop1, Rop2 ); }
    HRESULT Arm_ADD( CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_UINT32 Rop2 ) {                     return Arm_Alu( ArmProcessor::c_operation_ADD, Rdst, Rop1, Rop2 ); }
    HRESULT Arm_ADC( CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_UINT32 Rop2 ) {                     return Arm_Alu( ArmProcessor::c_operation_ADC, Rdst, Rop1, Rop2 ); }
    HRESULT Arm_SBC( CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_UINT32 Rop2 ) {                     return Arm_Alu( ArmProcessor::c_operation_SBC, Rdst, Rop1, Rop2 ); }
    HRESULT Arm_RSC( CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_UINT32 Rop2 ) {                     return Arm_Alu( ArmProcessor::c_operation_RSC, Rdst, Rop1, Rop2 ); }
    HRESULT Arm_TST(                  CLR_UINT32 Rop1, CLR_UINT32 Rop2 ) { m_Arm_setCC = true; return Arm_Alu( ArmProcessor::c_operation_TST, 0   , Rop1, Rop2 ); }
    HRESULT Arm_TEQ(                  CLR_UINT32 Rop1, CLR_UINT32 Rop2 ) { m_Arm_setCC = true; return Arm_Alu( ArmProcessor::c_operation_TEQ, 0   , Rop1, Rop2 ); }
    HRESULT Arm_CMP(                  CLR_UINT32 Rop1, CLR_UINT32 Rop2 ) { m_Arm_setCC = true; return Arm_Alu( ArmProcessor::c_operation_CMP, 0   , Rop1, Rop2 ); }
    HRESULT Arm_CMN(                  CLR_UINT32 Rop1, CLR_UINT32 Rop2 ) { m_Arm_setCC = true; return Arm_Alu( ArmProcessor::c_operation_CMN, 0   , Rop1, Rop2 ); }
    HRESULT Arm_ORR( CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_UINT32 Rop2 ) {                     return Arm_Alu( ArmProcessor::c_operation_ORR, Rdst, Rop1, Rop2 ); }
    HRESULT Arm_MOV( CLR_UINT32 Rdst, CLR_UINT32 Rsrc                  ) {                     return Arm_Alu( ArmProcessor::c_operation_MOV, Rdst, 0   , Rsrc ); }
    HRESULT Arm_BIC( CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_UINT32 Rop2 ) {                     return Arm_Alu( ArmProcessor::c_operation_BIC, Rdst, Rop1, Rop2 ); }
    HRESULT Arm_MVN( CLR_UINT32 Rdst, CLR_UINT32 Rsrc                  ) {                     return Arm_Alu( ArmProcessor::c_operation_MVN, Rdst, 0   , Rsrc ); }

    //--//

    HRESULT Arm_AND_IMM( CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_INT32 Vop2 ) {                     return Arm_Alu_IMM( ArmProcessor::c_operation_AND, Rdst, Rop1, Vop2 ); }
    HRESULT Arm_EOR_IMM( CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_INT32 Vop2 ) {                     return Arm_Alu_IMM( ArmProcessor::c_operation_EOR, Rdst, Rop1, Vop2 ); }
    HRESULT Arm_SUB_IMM( CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_INT32 Vop2 );
    HRESULT Arm_RSB_IMM( CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_INT32 Vop2 ) {                     return Arm_Alu_IMM( ArmProcessor::c_operation_RSB, Rdst, Rop1, Vop2 ); }
    HRESULT Arm_ADD_IMM( CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_INT32 Vop2 );
    HRESULT Arm_ADC_IMM( CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_INT32 Vop2 ) {                     return Arm_Alu_IMM( ArmProcessor::c_operation_ADC, Rdst, Rop1, Vop2 ); }
    HRESULT Arm_SBC_IMM( CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_INT32 Vop2 ) {                     return Arm_Alu_IMM( ArmProcessor::c_operation_SBC, Rdst, Rop1, Vop2 ); }
    HRESULT Arm_RSC_IMM( CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_INT32 Vop2 ) {                     return Arm_Alu_IMM( ArmProcessor::c_operation_RSC, Rdst, Rop1, Vop2 ); }
    HRESULT Arm_TST_IMM(                  CLR_UINT32 Rop1, CLR_INT32 Vop2 ) { m_Arm_setCC = true; return Arm_Alu_IMM( ArmProcessor::c_operation_TST, 0   , Rop1, Vop2 ); }
    HRESULT Arm_TEQ_IMM(                  CLR_UINT32 Rop1, CLR_INT32 Vop2 ) { m_Arm_setCC = true; return Arm_Alu_IMM( ArmProcessor::c_operation_TEQ, 0   , Rop1, Vop2 ); }
    HRESULT Arm_CMP_IMM(                  CLR_UINT32 Rop1, CLR_INT32 Vop2 );
    HRESULT Arm_CMN_IMM(                  CLR_UINT32 Rop1, CLR_INT32 Vop2 ) { m_Arm_setCC = true; return Arm_Alu_IMM( ArmProcessor::c_operation_CMN, 0   , Rop1, Vop2 ); }
    HRESULT Arm_ORR_IMM( CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_INT32 Vop2 ) {                     return Arm_Alu_IMM( ArmProcessor::c_operation_ORR, Rdst, Rop1, Vop2 ); }
    HRESULT Arm_MOV_IMM( CLR_UINT32 Rdst,                  CLR_INT32 Vsrc );
    HRESULT Arm_BIC_IMM( CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_INT32 Vop2 ) {                     return Arm_Alu_IMM( ArmProcessor::c_operation_BIC, Rdst, Rop1, Vop2 ); }
    HRESULT Arm_MVN_IMM( CLR_UINT32 Rdst,                  CLR_INT32 Vsrc ) {                     return Arm_Alu_IMM( ArmProcessor::c_operation_MVN, Rdst, 0   , Vsrc ); }
};

////////////////////////////////////////////////////////////////////////////////////////////////////

#endif

////////////////////////////////////////////////////////////////////////////////////////////////////

#endif //  _TINYCLR_JITTER_H_
