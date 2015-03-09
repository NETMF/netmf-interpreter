////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\Core\Core.h"

#include <TinyCLR_Jitter.h>

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_JITTER)

////////////////////////////////////////////////////////////////////////////////////////////////////

const CLR_UINT32 ArmProcessor::Opcode::c_op[] = {
    c_op_MRS                   ,
    c_op_MSR_1                 ,
    c_op_MSR_2                 ,
    c_op_DataProcessing_1      ,
    c_op_DataProcessing_2      ,
    c_op_DataProcessing_3      ,
    c_op_Multiply              ,
    c_op_MultiplyLong          ,
    c_op_SingleDataSwap        ,
    c_op_BranchAndExchange     ,
    c_op_HalfwordDataTransfer_1,
    c_op_HalfwordDataTransfer_2,
    c_op_SingleDataTransfer_1  ,
    c_op_SingleDataTransfer_2  ,
    c_op_SingleDataTransfer_3  ,
    c_op_Undefined             ,
    c_op_BlockDataTransfer     ,
    c_op_Branch                ,
    c_op_CoprocDataTransfer    ,
    c_op_CoprocDataOperation   ,
    c_op_CoprocRegisterTransfer,
    c_op_SoftwareInterrupt     ,
};

const CLR_UINT32 ArmProcessor::Opcode::c_opmask[] = {
    c_opmask_MRS                   ,
    c_opmask_MSR_1                 ,
    c_opmask_MSR_2                 ,
    c_opmask_DataProcessing_1      ,
    c_opmask_DataProcessing_2      ,
    c_opmask_DataProcessing_3      ,
    c_opmask_Multiply              ,
    c_opmask_MultiplyLong          ,
    c_opmask_SingleDataSwap        ,
    c_opmask_BranchAndExchange     ,
    c_opmask_HalfwordDataTransfer_1,
    c_opmask_HalfwordDataTransfer_2,
    c_opmask_SingleDataTransfer_1  ,
    c_opmask_SingleDataTransfer_2  ,
    c_opmask_SingleDataTransfer_3  ,
    c_opmask_Undefined             ,
    c_opmask_BlockDataTransfer     ,
    c_opmask_Branch                ,
    c_opmask_CoprocDataTransfer    ,
    c_opmask_CoprocDataOperation   ,
    c_opmask_CoprocRegisterTransfer,
    c_opmask_SoftwareInterrupt     ,
};

////////////////////////////////////////////////////////////////////////////////////////////////////

void ArmProcessor::State::Initialize( LoadFtn loadFtn, StoreFtn storeFtn )
{
    TINYCLR_CLEAR(*this);

    m_psr      = ArmProcessor::c_psr_I | ArmProcessor::c_psr_F | ArmProcessor::c_psr_mode_SVC;

    m_loadFtn  = loadFtn;
    m_storeFtn = storeFtn;


    //--//

    //
    // Precompute a lookup table for condition codes.
    //
    {
        for(int value=0; value<ArmProcessor::c_psr_cc_num; value++)
        {
            CLR_UINT32 res = 0;
            CLR_UINT32 psr = value << ArmProcessor::c_psr_bit_V;

            if(Zero    ( psr ) != 0                                       ) res |= 1 << ArmProcessor::c_cond_EQ;
            if(Zero    ( psr ) == 0                                       ) res |= 1 << ArmProcessor::c_cond_NE;
            if(Carry   ( psr ) != 0                                       ) res |= 1 << ArmProcessor::c_cond_CS;
            if(Carry   ( psr ) == 0                                       ) res |= 1 << ArmProcessor::c_cond_CC;
            if(Negative( psr ) != 0                                       ) res |= 1 << ArmProcessor::c_cond_MI;
            if(Negative( psr ) == 0                                       ) res |= 1 << ArmProcessor::c_cond_PL;
            if(Overflow( psr ) != 0                                       ) res |= 1 << ArmProcessor::c_cond_VS;
            if(Overflow( psr ) == 0                                       ) res |= 1 << ArmProcessor::c_cond_VC;
            if(Zero    ( psr ) == 0 &&  Carry   ( psr ) != 0              ) res |= 1 << ArmProcessor::c_cond_HI;
            if(Zero    ( psr ) != 0 ||  Carry   ( psr ) == 0              ) res |= 1 << ArmProcessor::c_cond_LS;
            if(                         Negative( psr ) == Overflow( psr )) res |= 1 << ArmProcessor::c_cond_GE;
            if(                         Negative( psr ) != Overflow( psr )) res |= 1 << ArmProcessor::c_cond_LT;
            if(Zero    ( psr ) == 0 &&  Negative( psr ) == Overflow( psr )) res |= 1 << ArmProcessor::c_cond_GT;
            if(Zero    ( psr ) != 0 ||  Negative( psr ) != Overflow( psr )) res |= 1 << ArmProcessor::c_cond_LE;
                                                                            res |= 1 << ArmProcessor::c_cond_AL;
            m_lookupConditions[ value ] = res;
        }
    }

}

bool ArmProcessor::State::GetShadowRegisters( CLR_UINT32 mode, CLR_UINT32*& ptr, CLR_UINT32& num )
{
    switch(mode & ArmProcessor::c_psr_mode)
    {
    case ArmProcessor::c_psr_mode_USER :
    case ArmProcessor::c_psr_mode_SYS  : ptr = m_registers_Mode_USER ; num = 7; return true;
    case ArmProcessor::c_psr_mode_FIQ  : ptr = m_registers_Mode_FIQ  ; num = 7; return true;
    case ArmProcessor::c_psr_mode_IRQ  : ptr = m_registers_Mode_IRQ  ; num = 2; return true;
    case ArmProcessor::c_psr_mode_SVC  : ptr = m_registers_Mode_SVC  ; num = 2; return true;
    case ArmProcessor::c_psr_mode_ABORT: ptr = m_registers_Mode_ABORT; num = 2; return true;
    case ArmProcessor::c_psr_mode_UNDEF: ptr = m_registers_Mode_UNDEF; num = 2; return true;
    }

    return false;
}

bool ArmProcessor::State::SwitchMode( CLR_UINT32 mode )
{
    CLR_UINT32* fromPtr;
    CLR_UINT32  fromNum;

    CLR_UINT32* toPtr;
    CLR_UINT32  toNum;

    if(!GetShadowRegisters( m_psr , fromPtr, fromNum )) return false;
    if(!GetShadowRegisters(   mode,   toPtr,   toNum )) return false;

    if(fromPtr != toPtr)
    {
        CLR_INT32 diff;

        memcpy( fromPtr, &m_registers[ 15-fromNum ], fromNum * sizeof(CLR_UINT32) );

        diff = toNum - fromNum;

        //
        // For example, switching from IRQ to USER, we should need to restore 2 registers, not 7.
        //
        if(diff > 0)
        {
            toPtr += diff;
            toNum -= diff;
        }

        memcpy( toPtr, &m_registers[ 15-toNum ], toNum * sizeof(CLR_UINT32) );
    }

    return true;
}

bool ArmProcessor::State::EnterMode( CLR_UINT32 mode )
{
    CLR_UINT32 currentPSR = m_psr;

    if(!SwitchMode( mode )) return false;

    switch(mode & ArmProcessor::c_psr_mode)
    {
    case ArmProcessor::c_psr_mode_USER : break;
    case ArmProcessor::c_psr_mode_FIQ  : break;
    case ArmProcessor::c_psr_mode_IRQ  : break;
    case ArmProcessor::c_psr_mode_SVC  : break;
    case ArmProcessor::c_psr_mode_ABORT: break;
    case ArmProcessor::c_psr_mode_UNDEF: break;
    case ArmProcessor::c_psr_mode_SYS  : break;
    }

    return true;
}

//--//--//--//--//--//--//--//--//--//--//--//--//

CLR_UINT32 ArmProcessor::Opcode::Formats::Encode( Format fmt ) const
{
    switch(fmt)
    {
        case MRS                   : return m_MRS                   .Encode();
        case MSR_1                 : return m_MSR_1                 .Encode();
        case MSR_2                 : return m_MSR_2                 .Encode();
        case DataProcessing_1      : return m_DataProcessing_1      .Encode();
        case DataProcessing_2      : return m_DataProcessing_2      .Encode();
        case DataProcessing_3      : return m_DataProcessing_3      .Encode();
        case Multiply              : return m_Multiply              .Encode();
        case MultiplyLong          : return m_MultiplyLong          .Encode();
        case SingleDataSwap        : return m_SingleDataSwap        .Encode();
        case BranchAndExchange     : return m_BranchAndExchange     .Encode();
        case HalfwordDataTransfer_1: return m_HalfwordDataTransfer_1.Encode();
        case HalfwordDataTransfer_2: return m_HalfwordDataTransfer_2.Encode();
        case SingleDataTransfer_1  : return m_SingleDataTransfer_1  .Encode();
        case SingleDataTransfer_2  : return m_SingleDataTransfer_2  .Encode();
        case SingleDataTransfer_3  : return m_SingleDataTransfer_3  .Encode();
        case Undefined             : return c_op_Undefined                   ;
        case BlockDataTransfer     : return m_BlockDataTransfer     .Encode();
        case Branch                : return m_Branch                .Encode();
        case CoprocDataTransfer    : return c_op_CoprocDataTransfer          ;
        case CoprocDataOperation   : return c_op_CoprocDataOperation         ;
        case CoprocRegisterTransfer: return c_op_CoprocRegisterTransfer      ;
        case SoftwareInterrupt     : return m_SoftwareInterrupt     .Encode();
    }

    return c_op_Undefined;
}

LPCSTR ArmProcessor::Opcode::DumpInstructionFormat( size_t fmt )
{
    switch(fmt)
    {
    case MRS                   : return "MRS"                   ;
    case MSR_1                 : return "MSR_1"                 ;
    case MSR_2                 : return "MSR_2"                 ;
    case DataProcessing_1      : return "DataProcessing_1"      ;
    case DataProcessing_2      : return "DataProcessing_2"      ;
    case DataProcessing_3      : return "DataProcessing_3"      ;
    case Multiply              : return "Multiply"              ;
    case MultiplyLong          : return "MultiplyLong"          ;
    case SingleDataSwap        : return "SingleDataSwap"        ;
    case BranchAndExchange     : return "BranchAndExchange"     ;
    case HalfwordDataTransfer_1: return "HalfwordDataTransfer_1";
    case HalfwordDataTransfer_2: return "HalfwordDataTransfer_2";
    case SingleDataTransfer_1  : return "SingleDataTransfer_1"  ;
    case SingleDataTransfer_2  : return "SingleDataTransfer_2"  ;
    case SingleDataTransfer_3  : return "SingleDataTransfer_3"  ;
    case Undefined             : return "Undefined"             ;
    case BlockDataTransfer     : return "BlockDataTransfer"     ;
    case Branch                : return "Branch"                ;
    case CoprocDataTransfer    : return "CoprocDataTransfer"    ;
    case CoprocDataOperation   : return "CoprocDataOperation"   ;
    case CoprocRegisterTransfer: return "CoprocRegisterTransfer";
    case SoftwareInterrupt     : return "SoftwareInterrupt"     ;
    }

    return "??";
}

LPCSTR ArmProcessor::Opcode::DumpOperation( CLR_UINT32 alu  )
{
    switch(alu)
    {
    case ArmProcessor::c_operation_AND: return "AND";
    case ArmProcessor::c_operation_EOR: return "EOR";
    case ArmProcessor::c_operation_SUB: return "SUB";
    case ArmProcessor::c_operation_RSB: return "RSB";
    case ArmProcessor::c_operation_ADD: return "ADD";
    case ArmProcessor::c_operation_ADC: return "ADC";
    case ArmProcessor::c_operation_SBC: return "SBC";
    case ArmProcessor::c_operation_RSC: return "RSC";
    case ArmProcessor::c_operation_TST: return "TST";
    case ArmProcessor::c_operation_TEQ: return "TEQ";
    case ArmProcessor::c_operation_CMP: return "CMP";
    case ArmProcessor::c_operation_CMN: return "CMN";
    case ArmProcessor::c_operation_ORR: return "ORR";
    case ArmProcessor::c_operation_MOV: return "MOV";
    case ArmProcessor::c_operation_BIC: return "BIC";
    case ArmProcessor::c_operation_MVN: return "MVN";
    }

    return "??";
}

LPCSTR ArmProcessor::Opcode::DumpCondition( CLR_UINT32 cond )
{
    switch(cond)
    {
    case ArmProcessor::c_cond_EQ: return "EQ";
    case ArmProcessor::c_cond_NE: return "NE";
    case ArmProcessor::c_cond_CS: return "CS";
    case ArmProcessor::c_cond_CC: return "CC";
    case ArmProcessor::c_cond_MI: return "MI";
    case ArmProcessor::c_cond_PL: return "PL";
    case ArmProcessor::c_cond_VS: return "VS";
    case ArmProcessor::c_cond_VC: return "VC";
    case ArmProcessor::c_cond_HI: return "HI";
    case ArmProcessor::c_cond_LS: return "LS";
    case ArmProcessor::c_cond_GE: return "GE";
    case ArmProcessor::c_cond_LT: return "LT";
    case ArmProcessor::c_cond_GT: return "GT";
    case ArmProcessor::c_cond_LE: return "LE";
    case ArmProcessor::c_cond_AL: return "";
    }

    return "??";
}

LPCSTR ArmProcessor::Opcode::DumpRegister( CLR_UINT32 reg )
{
    switch(reg)
    {
    case ArmProcessor::c_register_r0 : return "r0";
    case ArmProcessor::c_register_r1 : return "r1";
    case ArmProcessor::c_register_r2 : return "r2";
    case ArmProcessor::c_register_r3 : return "r3";
    case ArmProcessor::c_register_r4 : return "r4";
    case ArmProcessor::c_register_r5 : return "r5";
    case ArmProcessor::c_register_r6 : return "r6";
    case ArmProcessor::c_register_r7 : return "r7";
    case ArmProcessor::c_register_r8 : return "r8";
    case ArmProcessor::c_register_r9 : return "r9";
    case ArmProcessor::c_register_r10: return "r10";
    case ArmProcessor::c_register_r11: return "r11";
    case ArmProcessor::c_register_r12: return "r12";
    case ArmProcessor::c_register_sp : return "sp";
    case ArmProcessor::c_register_lr : return "lr";
    case ArmProcessor::c_register_pc : return "pc";
    }

    return "??";
}

LPCSTR ArmProcessor::Opcode::DumpShiftType( CLR_UINT32 stype )
{
    switch(stype)
    {
    case ArmProcessor::c_shift_LSL: return "LSL";
    case ArmProcessor::c_shift_LSR: return "LSR";
    case ArmProcessor::c_shift_ASR: return "ASR";
    case ArmProcessor::c_shift_ROR: return "ROR";
    case ArmProcessor::c_shift_RRX: return "RRX";
    }

    return "???";
}

LPCSTR ArmProcessor::Opcode::DumpHalfWordKind( CLR_UINT32 kind )
{
    switch(kind)
    {
    case ArmProcessor::c_halfwordkind_SWP: return "SWP";
    case ArmProcessor::c_halfwordkind_U2 : return "H";
    case ArmProcessor::c_halfwordkind_I1 : return "SB";
    case ArmProcessor::c_halfwordkind_I2 : return "SH";
    }

    return "??";
}

//--//

void ArmProcessor::Opcode::InitText( LPSTR szBuffer, size_t iBuffer, CLR_UINT32 address )
{
    m_dump_address  = address;
    m_dump_szBuffer = szBuffer;
    m_dump_iBuffer  = iBuffer;

    szBuffer[ 0 ] = 0;
}

void ArmProcessor::Opcode::AppendText( LPCSTR format, ... )
{
    va_list arg;

    va_start( arg, format );

    CLR_SafeSprintfV( m_dump_szBuffer, m_dump_iBuffer, format, arg );

    va_end( arg );
}

void ArmProcessor::Opcode::AppendMnemonic( LPCSTR format, ... )
{
    va_list arg;
    char    buffer[ 64 ];
    LPSTR   szBuffer =           buffer;
    size_t  iBuffer  = MAXSTRLEN(buffer);

    va_start( arg, format );

    CLR_SafeSprintfV( szBuffer, iBuffer, format, arg );

    AppendText( "%-8s ", buffer );
}

//--//

#define SIMULATE_PIPELINE_MISS(st)                                       \
    {                                                                    \
        CLR_UINT32 wasteCycle;                                           \
                                                                         \
        if(!st.Load( st.m_pc  , wasteCycle, DATATYPE_U4 )) return false; \
        if(!st.Load( st.m_pc+4, wasteCycle, DATATYPE_U4 )) return false; \
    }

bool ArmProcessor::Opcode::Execute( CLR_UINT32 op, State& st )
{
    CLR_UINT32   Operand2;
    CLR_UINT32   shifterCarry;
    CLR_UINT32   shiftType;
    CLR_UINT32*  dataPointer;
    CLR_DataType dataType;

    {
        CLR_UINT32 pcNext = st.m_pc + 4;

        st.m_pc                                       = pcNext;
        st.m_registers[ ArmProcessor::c_register_pc ] = pcNext + 4;
    }

    switch(OPCODE_DECODE_EXTRACTFIELD(op,25,3))
    {
    case 0: // 00000000[0E000000]
    //PARTIAL HIT MRS:
    //PARTIAL HIT MSR_1:
    //PARTIAL HIT DataProcessing_2:
    //PARTIAL HIT DataProcessing_3:
    //PARTIAL HIT Multiply:
    //PARTIAL HIT MultiplyLong:
    //PARTIAL HIT SingleDataSwap:
    //PARTIAL HIT BranchAndExchange:
    //PARTIAL HIT HalfwordDataTransfer_1:
    //PARTIAL HIT HalfwordDataTransfer_2:
        switch(OPCODE_DECODE_EXTRACTFIELD(op,4,4))
        {
            case 0: // 00000000[0e0000f0]
            //PARTIAL HIT MRS:
            //PARTIAL HIT MSR_1:
            //PARTIAL HIT DataProcessing_2:
                if((op & c_opmask_MRS  ) == c_op_MRS  ) goto parse_MRS;
                if((op & c_opmask_MSR_1) == c_op_MSR_1) goto parse_MSR_1;

                goto parse_DataProcessing_2;


            case 1: // 00000010[0e0000f0]
            //PARTIAL HIT DataProcessing_3:
            //PARTIAL HIT BranchAndExchange:
                if((op & c_opmask_BranchAndExchange) == c_op_BranchAndExchange) goto parse_BranchAndExchange;

                goto parse_DataProcessing_3;


            case 2: // 00000020[0e0000f0]
            case 4: // 00000040[0e0000f0]
            case 6: // 00000060[0e0000f0]
            case 8: // 00000080[0e0000f0]
            case 10: // 000000a0[0e0000f0]
            case 12: // 000000c0[0e0000f0]
            case 14: // 000000e0[0e0000f0]
            //HIT DataProcessing_2:
                goto parse_DataProcessing_2;


            case 3: // 00000030[0e0000f0]
            case 5: // 00000050[0e0000f0]
            case 7: // 00000070[0e0000f0]
            //HIT DataProcessing_3:
                goto parse_DataProcessing_3;


            case 9: // 00000090[0e0000f0]
            //PARTIAL HIT Multiply:
            //PARTIAL HIT MultiplyLong:
            //PARTIAL HIT SingleDataSwap:
                // +---------+---+---+---#########---+---+---+---------+---------+---------+---+---+---+---+---------+
                // | Cond    | 0 | 0 | 0 # 0 | 0 # 0 | A | S | Rd      | Rn      | Rs      | 1 | 0 | 0 | 1 | Rm      | Multiply
                // +---------+---+---+---#---+---#---+---+---+---------+---------+---------+---+---+---+---+---------+
                // | Cond    | 0 | 0 | 0 # 0 | 1 # U | A | S | RdHi    | RdLo    | Rn      | 1 | 0 | 0 | 1 | Rm      | Multiply Long
                // +---------+---+---+---#---+---#---+---+---+---------+---------+---------+---+---+---+---+---------+
                // | Cond    | 0 | 0 | 0 # 1 | 0 # B | 0 | 0 | Rn      | Rd      | 0 0 0 0 | 1 | 0 | 0 | 1 | Rm      | Single Data Swap
                // +---------+---+---+---#########---+---+---+---------+---------+---------+---+---+---+---+---------+
                switch(OPCODE_DECODE_EXTRACTFIELD(op,23,2))
                {
                case 0: // 00000090[0f8000f0]
                //HIT Multiply:
                    goto parse_Multiply;


                case 1: // 00800090[0f8000f0]
                //HIT MultiplyLong:
                    goto parse_MultiplyLong;


                case 2: // 01000090[0f8000f0]
                //HIT SingleDataSwap:
                    goto parse_SingleDataSwap;


                case 3: // 01800090[0f8000f0]
                //UNDEFINED
                    break;
                }
                break;

            case 11: // 000000b0[0e0000f0]
            case 13: // 000000d0[0e0000f0]
            case 15: // 000000f0[0e0000f0]
            //PARTIAL HIT HalfwordDataTransfer_1:
            //PARTIAL HIT HalfwordDataTransfer_2:
                {
                    // +---------+---+---+---+---+---#####---+---+---------+---------+---------+---+---+---+---+---------+
                    // | Cond    | 0 | 0 | 0 | P | U # 0 # W | L | Rn      | Rd      | 0 0 0 0 | 1 | S | H | 1 | Rm      | Halfword Data Transfer: register offset
                    // +---------+---+---+---+---+---#####---+---+---------+---------+---------+---+---+---+---+---------+
                    // | Cond    | 0 | 0 | 0 | P | U # 1 # W | L | Rn      | Rd      | Offset  | 1 | S | H | 1 | Offset  | Halfword Data Transfer: immediate offset
                    // +---------+---+---+---+---+---#####---+---+---------+---------+---------+---+---+---+---+---------+
                    if(OPCODE_DECODE_CHECKFLAG(op,22))
                    {
                        Operand2 = get_HalfWordDataTransfer_Offset( op );
                    }
                    else
                    {
                        Operand2 = st.m_registers[ get_Register4( op ) ];
                    }

                    goto parse_HalfwordDataTransfer;
                }
        }
        break;

    case 1: // 02000000[0E000000]
    //PARTIAL HIT MSR_2:
    //PARTIAL HIT DataProcessing_1:
        if((op & c_opmask_MSR_2) == c_op_MSR_2) goto parse_MSR_2;

        goto parse_DataProcessing_1;


    case 2: // 04000000[0E000000]
    //HIT SingleDataTransfer_1:
        goto parse_SingleDataTransfer_1;


    case 3: // 06000000[0E000000]
    //PARTIAL HIT SingleDataTransfer_2:
    //PARTIAL HIT SingleDataTransfer_3:
    //PARTIAL HIT Undefined:
        if((op & c_opmask_SingleDataTransfer_2) == c_op_SingleDataTransfer_2) goto parse_SingleDataTransfer_2;
        if((op & c_opmask_SingleDataTransfer_3) == c_op_SingleDataTransfer_3) goto parse_SingleDataTransfer_3;
        break;

    case 4: // 08000000[0E000000]
    //HIT BlockDataTransfer:
        goto parse_BlockDataTransfer;


    case 5: // 0A000000[0E000000]
    //HIT Branch:
        goto parse_Branch;


    case 6: // 0C000000[0E000000]
    //HIT CoprocDataTransfer:
        // Not Supported
        break;

    case 7: // 0E000000[0E000000]
    //PARTIAL HIT CoprocDataOperation:
    //PARTIAL HIT CoprocRegisterTransfer:
    //PARTIAL HIT SoftwareInterrupt:
        // Not Supported
        break;
    }

    return false;

    //--//

parse_MRS:
    {
        CLR_UINT32 res;

        if(get_StatusRegister_IsSPSR( op ))
        {
            switch(st.m_psr & ArmProcessor::c_psr_mode)
            {
            case ArmProcessor::c_psr_mode_FIQ  : res = st.m_registers_Mode_FIQ  [ 7 ]; break;
            case ArmProcessor::c_psr_mode_IRQ  : res = st.m_registers_Mode_IRQ  [ 2 ]; break;
            case ArmProcessor::c_psr_mode_SVC  : res = st.m_registers_Mode_SVC  [ 2 ]; break;
            case ArmProcessor::c_psr_mode_ABORT: res = st.m_registers_Mode_ABORT[ 2 ]; break;
            case ArmProcessor::c_psr_mode_UNDEF: res = st.m_registers_Mode_UNDEF[ 2 ]; break;

            default: return false;
            }
        }
        else
        {
            res = st.m_psr;
        }

        st.m_registers[ get_Register2( op ) ] = res;

        return true;
    }

parse_MSR_1:
    {
        Operand2 = st.m_registers[ get_Register4( op ) ];

        goto parse_MSR;
    }

parse_MSR_2:
    {
        Operand2 = get_DataProcessing_ImmediateValue( op );

        goto parse_MSR;
    }

parse_MSR:
    {
        CLR_UINT32* res;
        CLR_UINT32  mask;
        CLR_UINT32  fields = get_StatusRegister_Fields( op );

        if(get_StatusRegister_IsSPSR( op ))
        {
            switch(st.m_psr & ArmProcessor::c_psr_mode)
            {
            case ArmProcessor::c_psr_mode_FIQ  : res = &st.m_registers_Mode_FIQ  [ 7 ]; break;
            case ArmProcessor::c_psr_mode_IRQ  : res = &st.m_registers_Mode_IRQ  [ 2 ]; break;
            case ArmProcessor::c_psr_mode_SVC  : res = &st.m_registers_Mode_SVC  [ 2 ]; break;
            case ArmProcessor::c_psr_mode_ABORT: res = &st.m_registers_Mode_ABORT[ 2 ]; break;
            case ArmProcessor::c_psr_mode_UNDEF: res = &st.m_registers_Mode_UNDEF[ 2 ]; break;

            default: return false;
            }
        }
        else
        {
            res = &st.m_psr;

            if((st.m_psr & ArmProcessor::c_psr_mode) == ArmProcessor::c_psr_mode_USER)
            {
                //
                // Things not allowed in user mode.
                //
                if(fields & (ArmProcessor::c_psr_field_c | ArmProcessor::c_psr_field_x | ArmProcessor::c_psr_field_s)) return false;
            }
        }

        mask = 0;

        if(fields & ArmProcessor::c_psr_field_c) mask |= 0x000000FF;
        if(fields & ArmProcessor::c_psr_field_x) mask |= 0x0000FF00;
        if(fields & ArmProcessor::c_psr_field_s) mask |= 0x00FF0000;
        if(fields & ArmProcessor::c_psr_field_f) mask |= 0xFF000000;

        Operand2 = (*res & ~mask) | (Operand2 & mask);

        if(!st.SwitchMode( Operand2 )) return false;

        *res = Operand2;

        return true;
    }

    //--//

parse_DataProcessing_1:
    {
        Operand2     = get_DataProcessing_ImmediateValue   ( op );
        shifterCarry = get_DataProcessing_ImmediateRotation( op ) ? (Operand2 >> 31) : st.Carry();

        goto parse_DataProcessing;
    }

parse_DataProcessing_2:
    {
        shiftType = get_Shift_Type     ( op );
        Operand2  = get_Shift_Immediate( op );

        ARM_FIX_SHIFT_IN(Operand2,shiftType);

        goto parse_DataProcessing_2and3;
    }

parse_DataProcessing_3:
    {
        shiftType =                 get_Shift_Type    ( op );
        Operand2  = st.m_registers[ get_Shift_Register( op ) ];

        //
        // Extra internal cycle.
        //
        st.m_clockTicks++;

        goto parse_DataProcessing_2and3;
    }

parse_DataProcessing_2and3:
    {
        CLR_UINT32 shift = Operand2;

        Operand2     = st.m_registers[ get_Register4( op ) ];
        shifterCarry = st.Carry();

        if(shift)
        {
            switch(shiftType)
            {
            case ArmProcessor::c_shift_LSL:
                if(shift < 32)
                {
                    shifterCarry = (Operand2 >> (32 - shift)) & 1;
                    Operand2     = (Operand2 <<       shift )    ;
                }
                else if(shift == 32)
                {
                    shifterCarry = Operand2 & 1;
                    Operand2     = 0;
                }
                else
                {
                    shifterCarry = 0;
                    Operand2     = 0;
                }
                break;

            case ArmProcessor::c_shift_LSR:
                if(shift < 32)
                {
                    shifterCarry = (Operand2 >> (shift - 1)) & 1;
                    Operand2     = (Operand2 >>  shift     )    ;
                }
                else if(shift == 32)
                {
                    shifterCarry = (Operand2 >> 31) & 1;
                    Operand2     = 0;
                }
                else
                {
                    shifterCarry = 0;
                    Operand2     = 0;
                }
                break;

            case ArmProcessor::c_shift_ASR:
                if(shift < 32)
                {
                    shifterCarry = (                        Operand2  >> (shift - 1)) & 1;
                    Operand2     = (CLR_UINT32)(((CLR_INT32)Operand2) >>  shift     )    ;
                }
                else
                {
                    shifterCarry = (Operand2 >> 31) & 1;
                    Operand2     = shifterCarry ? 0xFFFFFFFF : 0x0;
                }
                break;

            case ArmProcessor::c_shift_ROR:
                shift %= 32;

                shifterCarry = (Operand2 >> (shift - 1)) & 1;
                Operand2     = (Operand2 >>  shift     )     | (Operand2 << (32 - shift));
                break;

            case ArmProcessor::c_shift_RRX:
                shifterCarry =                                 (Operand2  & 1);
                Operand2     = (st.Carry() ? 0x80000000 : 0) | (Operand2 >> 1);
                break;
            }
        }

        goto parse_DataProcessing;
    }

parse_DataProcessing:
    {
        CLR_UINT32 Operand1 = st.m_registers[ get_Register1( op ) ];
        CLR_INT32  dst      =                 get_Register2( op );
        CLR_UINT32 overflow = st.Overflow();
        CLR_UINT32 carry    = st.Carry   ();
        CLR_UINT32 res;

        switch(get_DataProcessing_Operation( op ))
        {
        case ArmProcessor::c_operation_TST: dst   = 16;
        case ArmProcessor::c_operation_AND: res   = Operand1 & Operand2; carry = shifterCarry; break;

        case ArmProcessor::c_operation_TEQ: dst   = 16;
        case ArmProcessor::c_operation_EOR: res   = Operand1 ^ Operand2; carry = shifterCarry; break;

        case ArmProcessor::c_operation_CMP: dst   = 16;
        case ArmProcessor::c_operation_SUB: carry = 1;
        case ArmProcessor::c_operation_SBC:
            {
                CLR_UINT64 res64 = (CLR_UINT64)Operand1 - (CLR_UINT64)Operand2; if(!carry) res64--;

                carry = (CLR_UINT32)(res64 >> 32) ^ 1;
                res   = (CLR_UINT32) res64;

                overflow = ((Operand1 ^ Operand2) & (Operand1 ^ res)) >> 31;
            }
            break;

        case ArmProcessor::c_operation_RSB: carry = 1;
        case ArmProcessor::c_operation_RSC:
            {
                CLR_UINT64 res64 = (CLR_UINT64)Operand2 - (CLR_UINT64)Operand1; if(!carry) res64--;

                carry = (CLR_UINT32)(res64 >> 32) ^ 1;
                res   = (CLR_UINT32) res64;

                overflow = ((Operand1 ^ Operand2) & (Operand2 ^ res)) >> 31;
            }
            break;

        case ArmProcessor::c_operation_CMN: dst   = 16;
        case ArmProcessor::c_operation_ADD: carry = 0;
        case ArmProcessor::c_operation_ADC:
            {
                CLR_UINT64 res64 = (CLR_UINT64)Operand2 + (CLR_UINT64)Operand1; if(carry) res64++;

                carry = (CLR_UINT32)(res64 >> 32);
                res   = (CLR_UINT32) res64;

                overflow = (~(Operand1 ^ Operand2) & (Operand1 ^ res)) >> 31;
            }
            break;

        case ArmProcessor::c_operation_ORR: res = Operand1 |  Operand2; carry = shifterCarry; break;
        case ArmProcessor::c_operation_BIC: res = Operand1 & ~Operand2; carry = shifterCarry; break;

        case ArmProcessor::c_operation_MOV: res =        Operand2; carry = shifterCarry; break;
        case ArmProcessor::c_operation_MVN: res =       ~Operand2; carry = shifterCarry; break;

        default: return false;
        }

        st.m_registers[ dst ] = res;

        if(dst == ArmProcessor::c_register_pc)
        {
            SIMULATE_PIPELINE_MISS(st);

            st.m_pc = res;
        }

        if(get_ShouldSetConditions( op ))
        {
            CLR_UINT32 psr = st.m_psr & ~(ArmProcessor::c_psr_V | ArmProcessor::c_psr_C | ArmProcessor::c_psr_Z | ArmProcessor::c_psr_N);

            if((CLR_INT32)res <  0) psr |= ArmProcessor::c_psr_N;
            if(           res == 0) psr |= ArmProcessor::c_psr_Z;
            if(carry    & 1       ) psr |= ArmProcessor::c_psr_C;
            if(overflow & 1       ) psr |= ArmProcessor::c_psr_V;

            st.m_psr = psr;
        }

        return true;
    }

    //--//

parse_Multiply:
    {
        CLR_UINT32 Op1 = st.m_registers[ get_Register4( op ) ];
        CLR_UINT32 Op2 = st.m_registers[ get_Register3( op ) ];
        CLR_UINT32 res;

        {
            CLR_UINT32 mCycles;

            if     ((Op1 & 0xFFFFFF00) == 0) mCycles = 1;
            else if((Op1 & 0xFFFF0000) == 0) mCycles = 2;
            else if((Op1 & 0xFF000000) == 0) mCycles = 3;
            else                             mCycles = 4;

            st.m_clockTicks += mCycles;
        }

        res = Op1 * Op2;

        if(get_Multiply_IsAccumulate( op ))
        {
            res += st.m_registers[ get_Register2( op ) ];

            //
            // Extra internal cycle.
            //
            st.m_clockTicks++;
        }

        st.m_registers[ get_Register1( op ) ] = res;

        if(get_ShouldSetConditions( op ))
        {
            CLR_UINT32 psr = st.m_psr & ~(ArmProcessor::c_psr_Z | ArmProcessor::c_psr_N);

            if((CLR_INT32)res <  0) psr |= ArmProcessor::c_psr_N;
            if(           res == 0) psr |= ArmProcessor::c_psr_Z;

            st.m_psr = psr;
        }

        return true;
    }

parse_MultiplyLong:
    {
        CLR_UINT32 Op1 = st.m_registers[ get_Register4( op ) ];
        CLR_UINT32 Op2 = st.m_registers[ get_Register3( op ) ];
        CLR_UINT64 res;

        {
            CLR_UINT32 mCycles;

            if     ((Op1 & 0xFFFFFF00) == 0) mCycles = 2;
            else if((Op1 & 0xFFFF0000) == 0) mCycles = 3;
            else if((Op1 & 0xFF000000) == 0) mCycles = 4;
            else                             mCycles = 5;

            st.m_clockTicks += mCycles;
        }

        if(get_Multiply_IsSigned( op ))
        {
            res = (CLR_INT64)(CLR_INT32)Op1 * (CLR_INT64)(CLR_INT32)Op2;
        }
        else
        {
            res = (CLR_UINT64)Op1 * (CLR_UINT64)Op2;
        }

        CLR_UINT32& RdHi = st.m_registers[ get_Register1( op ) ];
        CLR_UINT32& RdLo = st.m_registers[ get_Register2( op ) ];

        if(get_Multiply_IsAccumulate( op ))
        {
            res += (CLR_UINT64)RdHi << 32 | (CLR_UINT64)RdLo;

            //
            // Extra internal cycle.
            //
            st.m_clockTicks++;
        }

        RdHi = (CLR_UINT32)(res >> 32);
        RdLo = (CLR_UINT32) res;

        if(get_ShouldSetConditions( op ))
        {
            CLR_UINT32 psr = st.m_psr & ~(ArmProcessor::c_psr_Z | ArmProcessor::c_psr_N);

            if((CLR_INT64)res <  0) psr |= ArmProcessor::c_psr_N;
            if(           res == 0) psr |= ArmProcessor::c_psr_Z;

            st.m_psr = psr;
        }

        return true;
    }

    //--//

parse_BranchAndExchange:
    {
        // Not supported for now.
        return false;
    }

parse_Branch:
    {
        CLR_UINT32 pc = st.m_registers[ ArmProcessor::c_register_pc ];

        if(get_Branch_IsLink( op ))
        {
            st.m_registers[ ArmProcessor::c_register_lr ] = pc - 4; // R15 contains PC+8, LR has to point to PC+4;
        }

        pc += get_Branch_Offset( op );


        SIMULATE_PIPELINE_MISS(st);

        st.m_pc = pc;

        return true;
    }

    //--//

parse_SingleDataSwap:
    {
        // Not supported for now.
        return false;
    }

parse_SingleDataTransfer_1:
    {
        Operand2 = get_DataTransfer_Offset( op );

        goto parse_SingleDataTransfer;
    }

parse_SingleDataTransfer_2:
    {
        shiftType = get_Shift_Type     ( op );
        Operand2  = get_Shift_Immediate( op );

        ARM_FIX_SHIFT_IN(Operand2,shiftType);

        goto parse_SingleDataTransfer_2and3;
    }

parse_SingleDataTransfer_3:
    {
        shiftType =                 get_Shift_Type    ( op );
        Operand2  = st.m_registers[ get_Shift_Register( op ) ];

        goto parse_SingleDataTransfer_2and3;
    }

parse_SingleDataTransfer_2and3:
    {
        CLR_UINT32 shift = Operand2;

        Operand2 = st.m_registers[ get_Register4( op ) ];

        if(shift)
        {
            switch(shiftType)
            {
            case ArmProcessor::c_shift_LSL: Operand2 =             (            Operand2  << shift )                             ; break;
            case ArmProcessor::c_shift_LSR: Operand2 =             (            Operand2  >> shift )                             ; break;
            case ArmProcessor::c_shift_ASR: Operand2 = (CLR_UINT32)(((CLR_INT32)Operand2) >> shift )                             ; break;
            case ArmProcessor::c_shift_ROR: Operand2 =             (            Operand2  >> shift ) | (Operand2 << (32 - shift)); break;
            }
        }

        goto parse_SingleDataTransfer;
    }

parse_SingleDataTransfer:
    {
        dataPointer = &st.m_registers[ get_Register2                  ( op ) ];
        dataType    =                  get_DataTransfer_IsByteTransfer( op ) ? DATATYPE_U1 : DATATYPE_U4;

        goto parse_DataTransfer;
    }

parse_BlockDataTransfer:
    {
        CLR_UINT32 address = st.m_registers[ get_Register1( op ) ];
        CLR_UINT32 addressNext;

        CLR_UINT32 Rd  = 0;
        CLR_UINT32 Num = 0;
        CLR_UINT32 Lst;

        Lst = get_BlockDataTransfer_RegisterList( op );
        while(Lst)
        {
            if(Lst & 1) Num++;

            Lst >>= 1;
        }

        bool load     = get_DataTransfer_IsLoad       ( op );
        bool preIndex = get_DataTransfer_IsPreIndexing( op );
        bool up       = get_DataTransfer_IsUp         ( op );

        if(up)
        {
            addressNext = address;

            if(preIndex) addressNext += 4;
        }
        else
        {
            addressNext = address - Num * 4;

            if(!preIndex) addressNext += 4;
        }

        if(load)
        {
            //
            // Extra internal cycle.
            //
            st.m_clockTicks++;
        }

        Lst = get_BlockDataTransfer_RegisterList( op );
        while(Lst)
        {
            if(Lst & 1)
            {
                if(load)
                {
                    if(!st.Load( addressNext, st.m_registers[ Rd ], DATATYPE_U4 )) return false;

                    if(Rd == ArmProcessor::c_register_pc)
                    {
                        SIMULATE_PIPELINE_MISS(st);

                        st.m_pc = st.m_registers[ ArmProcessor::c_register_pc ];
                    }
                }
                else
                {
                    if(!st.Store( addressNext, st.m_registers[ Rd ], DATATYPE_U4 )) return false;
                }

                addressNext += 4;
            }

            Rd++;
            Lst >>= 1;
        }

        if(get_BlockDataTransfer_LoadPSR( op ))
        {
            // Not supported for now.
            return false;
        }

        if(get_DataTransfer_ShouldWriteBack( op ))
        {
            if(up) address += 4 * Num;
            else   address -= 4 * Num;

            st.m_registers[ get_Register1( op ) ] = address;
        }

        return true;
    }

parse_HalfwordDataTransfer:
    {
        switch(get_HalfWordDataTransfer_Kind( op ))
        {
        case ArmProcessor::c_halfwordkind_U2: dataType = DATATYPE_U2; break;
        case ArmProcessor::c_halfwordkind_I1: dataType = DATATYPE_I1; break;
        case ArmProcessor::c_halfwordkind_I2: dataType = DATATYPE_I2; break;
        default: return false;
        }

        dataPointer = &st.m_registers[ get_Register2( op ) ];

        goto parse_DataTransfer;
    }

parse_DataTransfer:
    {
        CLR_UINT32 address = st.m_registers[ get_Register1( op ) ];
        CLR_UINT32 addressPost;

        bool load     = get_DataTransfer_IsLoad       ( op );
        bool preIndex = get_DataTransfer_IsPreIndexing( op );
        bool up       = get_DataTransfer_IsUp         ( op );

        if(up) addressPost = address + Operand2;
        else   addressPost = address - Operand2;

        if(preIndex) address = addressPost;

        if(load)
        {
            if(!st.Load( address, *dataPointer, dataType )) return false;

            //
            // Extra internal cycle.
            //
            st.m_clockTicks++;

            if(dataPointer == &st.m_registers[ ArmProcessor::c_register_pc ])
            {
                SIMULATE_PIPELINE_MISS(st);

                st.m_pc = *dataPointer;
            }
        }
        else
        {
            if(!st.Store( address, *dataPointer, dataType )) return false;
        }

        if(get_DataTransfer_ShouldWriteBack( op ) || preIndex == 0)
        {
            st.m_registers[ get_Register1( op ) ] = addressPost;
        }

        return true;
    }
}

bool ArmProcessor::Opcode::Decode( CLR_UINT32 op )
{
    for(size_t fmt=FIRST_FORMAT; fmt<=LAST_FORMAT; fmt++)
    {
        if((op & c_opmask[ fmt ]) == c_op[ fmt ])
        {
            m_op             = op;
            m_fmt            = (Format)fmt;
            m_conditionCodes = get_ConditionCodes( op );

            switch(fmt)
            {
                case MRS                   : m_formats.m_MRS                   .Decode( op ); return true;
                case MSR_1                 : m_formats.m_MSR_1                 .Decode( op ); return true;
                case MSR_2                 : m_formats.m_MSR_2                 .Decode( op ); return true;
                case DataProcessing_1      : m_formats.m_DataProcessing_1      .Decode( op ); return true;
                case DataProcessing_2      : m_formats.m_DataProcessing_2      .Decode( op ); return true;
                case DataProcessing_3      : m_formats.m_DataProcessing_3      .Decode( op ); return true;
                case Multiply              : m_formats.m_Multiply              .Decode( op ); return true;
                case MultiplyLong          : m_formats.m_MultiplyLong          .Decode( op ); return true;
                case SingleDataSwap        : m_formats.m_SingleDataSwap        .Decode( op ); return true;
                case BranchAndExchange     : m_formats.m_BranchAndExchange     .Decode( op ); return true;
                case HalfwordDataTransfer_1: m_formats.m_HalfwordDataTransfer_1.Decode( op ); return true;
                case HalfwordDataTransfer_2: m_formats.m_HalfwordDataTransfer_2.Decode( op ); return true;
                case SingleDataTransfer_1  : m_formats.m_SingleDataTransfer_1  .Decode( op ); return true;
                case SingleDataTransfer_2  : m_formats.m_SingleDataTransfer_2  .Decode( op ); return true;
                case SingleDataTransfer_3  : m_formats.m_SingleDataTransfer_3  .Decode( op ); return true;
                case Undefined             :                                                  return true;
                case BlockDataTransfer     : m_formats.m_BlockDataTransfer     .Decode( op ); return true;
                case Branch                : m_formats.m_Branch                .Decode( op ); return true;
                case CoprocDataTransfer    :                                                  return true;
                case CoprocDataOperation   :                                                  return true;
                case CoprocRegisterTransfer:                                                  return true;
                case SoftwareInterrupt     : m_formats.m_SoftwareInterrupt     .Decode( op ); return true;
            }
        }
    }

    return false;
}

CLR_UINT32 ArmProcessor::Opcode::Encode() const
{
    return set_ConditionCodes( m_conditionCodes ) | m_formats.Encode( m_fmt );
}

CLR_UINT32 ArmProcessor::Opcode::EncodeWithCondition( CLR_UINT32 cond )
{
    m_conditionCodes = cond;

    return Encode();
}

bool ArmProcessor::Opcode::CheckConditions( State& st )
{
    return st.CheckConditions( m_conditionCodes );
}

void ArmProcessor::Opcode::Print()
{
    switch(m_fmt)
    {
    case MRS                   : m_formats.m_MRS                   .Print( *this ); return;
    case MSR_1                 : m_formats.m_MSR_1                 .Print( *this ); return;
    case MSR_2                 : m_formats.m_MSR_2                 .Print( *this ); return;
    case DataProcessing_1      : m_formats.m_DataProcessing_1      .Print( *this ); return;
    case DataProcessing_2      : m_formats.m_DataProcessing_2      .Print( *this ); return;
    case DataProcessing_3      : m_formats.m_DataProcessing_3      .Print( *this ); return;
    case Multiply              : m_formats.m_Multiply              .Print( *this ); return;
    case MultiplyLong          : m_formats.m_MultiplyLong          .Print( *this ); return;
    case SingleDataSwap        : m_formats.m_SingleDataSwap        .Print( *this ); return;
    case BranchAndExchange     : m_formats.m_BranchAndExchange     .Print( *this ); return;
    case HalfwordDataTransfer_1: m_formats.m_HalfwordDataTransfer_1.Print( *this ); return;
    case HalfwordDataTransfer_2: m_formats.m_HalfwordDataTransfer_2.Print( *this ); return;
    case SingleDataTransfer_1  : m_formats.m_SingleDataTransfer_1  .Print( *this ); return;
    case SingleDataTransfer_2  : m_formats.m_SingleDataTransfer_2  .Print( *this ); return;
    case SingleDataTransfer_3  : m_formats.m_SingleDataTransfer_3  .Print( *this ); return;
    case BlockDataTransfer     : m_formats.m_BlockDataTransfer     .Print( *this ); return;
    case Branch                : m_formats.m_Branch                .Print( *this ); return;
    case SoftwareInterrupt     : m_formats.m_SoftwareInterrupt     .Print( *this ); return;
    }

    AppendText( "%s", DumpInstructionFormat( m_fmt ) );
}

void ArmProcessor::Opcode::Print( CLR_UINT32 address, CLR_UINT32 op )
{
    ArmProcessor::Opcode encoder;
    char                 rgBuf[ 128 ];

    //--//

    encoder.InitText( rgBuf, MAXSTRLEN(rgBuf), address );

    encoder.AppendText( "0x%08x:  %08x    ", address, op );

    for(size_t pos=0; pos<4; pos++)
    {
        CLR_UINT32 c = (op >> (8*pos)) & 0xFF;

        encoder.AppendText( "%c", (c <= 0x1F || c >= 0x7F) ? '.' : c );
    }

    encoder.AppendText( "    " );

    //--//

    if(encoder.Decode( op ))
    {
        encoder.Print();

        if(op != encoder.Encode())
        {
            switch(encoder.m_fmt)
            {
            case Undefined             :
            case CoprocDataTransfer    :
            case CoprocDataOperation   :
            case CoprocRegisterTransfer:
                break; // Not fully supported.

            default:
                encoder.AppendText( "  ENCODING MISMATCH: %08X != %08X", op, encoder.Encode() );
                break;
            }
        }
    }
    else
    {
        encoder.AppendText( "???" );
    }

    CLR_Debug::Printf( "%s\r\n", rgBuf );
}

////////////////////////////////////////////////////////////////////////////////////////////////////

#endif
