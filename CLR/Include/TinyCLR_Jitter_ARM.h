////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _TINYCLR_JITTER_ARM_H_
#define _TINYCLR_JITTER_ARM_H_

////////////////////////////////////////////////////////////////////////////////////////////////////

struct ArmProcessor
{
#define OPCODE_DECODE_MASK(len) ((1U << (len))-1U)

#define OPCODE_DECODE_INSERTFIELD(val,bitPos,bitLen) (((val)               & OPCODE_DECODE_MASK(bitLen)) << (bitPos))
#define OPCODE_DECODE_EXTRACTFIELD(op,bitPos,bitLen) (((op ) >> (bitPos) ) & OPCODE_DECODE_MASK(bitLen)             )

#define OPCODE_DECODE_SETFLAG(val,bitPos)            ((val) ? (1 << (bitPos)) : 0 )
#define OPCODE_DECODE_CHECKFLAG(op,bitPos)           (((op) & (1 << (bitPos))) != 0)

    //--//

    static const CLR_UINT32 c_psr_bit_T      = 5;
    static const CLR_UINT32 c_psr_bit_F      = 6;
    static const CLR_UINT32 c_psr_bit_I      = 7;
    static const CLR_UINT32 c_psr_bit_V      = 28;
    static const CLR_UINT32 c_psr_bit_C      = 29;
    static const CLR_UINT32 c_psr_bit_Z      = 30;
    static const CLR_UINT32 c_psr_bit_N      = 31;


    static const CLR_UINT32 c_psr_T          = 1U << c_psr_bit_T;
    static const CLR_UINT32 c_psr_F          = 1U << c_psr_bit_F;
    static const CLR_UINT32 c_psr_I          = 1U << c_psr_bit_I;
    static const CLR_UINT32 c_psr_V          = 1U << c_psr_bit_V;
    static const CLR_UINT32 c_psr_C          = 1U << c_psr_bit_C;
    static const CLR_UINT32 c_psr_Z          = 1U << c_psr_bit_Z;
    static const CLR_UINT32 c_psr_N          = 1U << c_psr_bit_N;


    static const CLR_UINT32 c_psr_cc_shift   =                     c_psr_bit_V;
    static const CLR_UINT32 c_psr_cc_num     = 1 << (c_psr_bit_N - c_psr_bit_V + 1);
    static const CLR_UINT32 c_psr_cc_mask    = c_psr_cc_num - 1;


    static const CLR_UINT32 c_psr_mode       = 0x0000001F;
    static const CLR_UINT32 c_psr_mode_USER  = 0x00000010;
    static const CLR_UINT32 c_psr_mode_FIQ   = 0x00000011;
    static const CLR_UINT32 c_psr_mode_IRQ   = 0x00000012;
    static const CLR_UINT32 c_psr_mode_SVC   = 0x00000013;
    static const CLR_UINT32 c_psr_mode_ABORT = 0x00000017;
    static const CLR_UINT32 c_psr_mode_UNDEF = 0x0000001B;
    static const CLR_UINT32 c_psr_mode_SYS   = 0x0000001F;

    //--//

    /////////////////////////////////////////////
    static const CLR_UINT32 c_cond_EQ     = 0x0; //  Z set equal
    static const CLR_UINT32 c_cond_NE     = 0x1; //  Z clear not equal
    static const CLR_UINT32 c_cond_CS     = 0x2; //  C set unsigned higher or same
    static const CLR_UINT32 c_cond_CC     = 0x3; //  C clear unsigned lower
    static const CLR_UINT32 c_cond_MI     = 0x4; //  N set negative
    static const CLR_UINT32 c_cond_PL     = 0x5; //  N clear positive or zero
    static const CLR_UINT32 c_cond_VS     = 0x6; //  V set overflow
    static const CLR_UINT32 c_cond_VC     = 0x7; //  V clear no overflow
    static const CLR_UINT32 c_cond_HI     = 0x8; //  C set and Z clear unsigned higher
    static const CLR_UINT32 c_cond_LS     = 0x9; //  C clear or Z set unsigned lower or same
    static const CLR_UINT32 c_cond_GE     = 0xA; //  N equals V greater or equal
    static const CLR_UINT32 c_cond_LT     = 0xB; //  N not equal to V less than
    static const CLR_UINT32 c_cond_GT     = 0xC; //  Z clear AND (N equals V) greater than
    static const CLR_UINT32 c_cond_LE     = 0xD; //  Z set OR (N not equal to V) less than or equal
    static const CLR_UINT32 c_cond_AL     = 0xE; //  (ignored) always
    static const CLR_UINT32 c_cond_UNUSED = 0xF; //
    /////////////////////////////////////////////
    static const CLR_UINT32 c_cond_NUM    = 0x10;

    ///////////////////////////////////////////////
    static const CLR_UINT32 c_operation_AND = 0x0; // operand1 AND operand2
    static const CLR_UINT32 c_operation_EOR = 0x1; // operand1 EOR operand2
    static const CLR_UINT32 c_operation_SUB = 0x2; // operand1 - operand2
    static const CLR_UINT32 c_operation_RSB = 0x3; // operand2 - operand1
    static const CLR_UINT32 c_operation_ADD = 0x4; // operand1 + operand2
    static const CLR_UINT32 c_operation_ADC = 0x5; // operand1 + operand2 + carry
    static const CLR_UINT32 c_operation_SBC = 0x6; // operand1 - operand2 + carry - 1
    static const CLR_UINT32 c_operation_RSC = 0x7; // operand2 - operand1 + carry - 1
    static const CLR_UINT32 c_operation_TST = 0x8; // as AND, but result is not written
    static const CLR_UINT32 c_operation_TEQ = 0x9; // as EOR, but result is not written
    static const CLR_UINT32 c_operation_CMP = 0xA; // as SUB, but result is not written
    static const CLR_UINT32 c_operation_CMN = 0xB; // as ADD, but result is not written
    static const CLR_UINT32 c_operation_ORR = 0xC; // operand1 OR operand2
    static const CLR_UINT32 c_operation_MOV = 0xD; // operand2(operand1 is ignored)
    static const CLR_UINT32 c_operation_BIC = 0xE; // operand1 AND NOT operand2(Bit clear)
    static const CLR_UINT32 c_operation_MVN = 0xF; // NOT operand2(operand1 is ignored)
    ///////////////////////////////////////////////

    //--//

    //////////////////////////////////////////////////
    static const CLR_UINT32 c_shift_LSL = 0x0; // logical shift left
    static const CLR_UINT32 c_shift_LSR = 0x1; // logical shift right
    static const CLR_UINT32 c_shift_ASR = 0x2; // arithmetic shift right
    static const CLR_UINT32 c_shift_ROR = 0x3; // rotate right
    static const CLR_UINT32 c_shift_RRX = 0x4; // rotate right with extend
    //////////////////////////////////////////////////

#define ARM_FIX_SHIFT_IN(amount,type)              \
    if(amount == 0)                                \
    {                                              \
        switch(type)                               \
        {                                          \
        case ArmProcessor::c_shift_LSR:            \
        case ArmProcessor::c_shift_ASR:            \
            amount = 32;                           \
            break;                                 \
                                                   \
        case ArmProcessor::c_shift_ROR:            \
            amount = 1;                            \
            type   = ArmProcessor::c_shift_RRX;    \
            break;                                 \
        }                                          \
    }

#define ARM_FIX_SHIFT_OUT(amount,type)             \
    switch(type)                                   \
    {                                              \
    case ArmProcessor::c_shift_LSR:                \
    case ArmProcessor::c_shift_ASR:                \
        amount %= 32;                              \
        break;                                     \
                                                   \
    case ArmProcessor::c_shift_RRX:                \
        amount = 0;                                \
        type   = ArmProcessor::c_shift_ROR;        \
        break;                                     \
    }

    //--//

    //////////////////////////////////////////////////
    static const CLR_UINT32 c_halfwordkind_SWP = 0x0; //
    static const CLR_UINT32 c_halfwordkind_U2  = 0x1; //
    static const CLR_UINT32 c_halfwordkind_I1  = 0x2; //
    static const CLR_UINT32 c_halfwordkind_I2  = 0x3; //
    //////////////////////////////////////////////////

    //--//

    /////////////////////////////////////////////
    static const CLR_UINT32 c_psr_field_c = 0x1; // the control field   PSR[ 7: 0]
    static const CLR_UINT32 c_psr_field_x = 0x2; // the extension field PSR[15: 8]
    static const CLR_UINT32 c_psr_field_s = 0x4; // the status field    PSR[23:16]
    static const CLR_UINT32 c_psr_field_f = 0x8; // the flags field     PSR[31:24]
    /////////////////////////////////////////////

    //--//

    ///////////////////////////////////////////////
    static const CLR_UINT32 c_register_r0  =  0; //
    static const CLR_UINT32 c_register_r1  =  1; //
    static const CLR_UINT32 c_register_r2  =  2; //
    static const CLR_UINT32 c_register_r3  =  3; //
    static const CLR_UINT32 c_register_r4  =  4; //
    static const CLR_UINT32 c_register_r5  =  5; //
    static const CLR_UINT32 c_register_r6  =  6; //
    static const CLR_UINT32 c_register_r7  =  7; //
    static const CLR_UINT32 c_register_r8  =  8; //
    static const CLR_UINT32 c_register_r9  =  9; //
    static const CLR_UINT32 c_register_r10 = 10; //
    static const CLR_UINT32 c_register_r11 = 11; //
    static const CLR_UINT32 c_register_r12 = 12; //
    static const CLR_UINT32 c_register_r13 = 13; //
    static const CLR_UINT32 c_register_r14 = 14; //
    static const CLR_UINT32 c_register_r15 = 15; //
    static const CLR_UINT32 c_register_sp  = 13; //
    static const CLR_UINT32 c_register_lr  = 14; //
    static const CLR_UINT32 c_register_pc  = 15; //
    ///////////////////////////////////////////////

    static const CLR_UINT32 c_register_lst_r0  = 1 << c_register_r0 ;
    static const CLR_UINT32 c_register_lst_r1  = 1 << c_register_r1 ;
    static const CLR_UINT32 c_register_lst_r2  = 1 << c_register_r2 ;
    static const CLR_UINT32 c_register_lst_r3  = 1 << c_register_r3 ;
    static const CLR_UINT32 c_register_lst_r4  = 1 << c_register_r4 ;
    static const CLR_UINT32 c_register_lst_r5  = 1 << c_register_r5 ;
    static const CLR_UINT32 c_register_lst_r6  = 1 << c_register_r6 ;
    static const CLR_UINT32 c_register_lst_r7  = 1 << c_register_r7 ;
    static const CLR_UINT32 c_register_lst_r8  = 1 << c_register_r8 ;
    static const CLR_UINT32 c_register_lst_r9  = 1 << c_register_r9 ;
    static const CLR_UINT32 c_register_lst_r10 = 1 << c_register_r10;
    static const CLR_UINT32 c_register_lst_r11 = 1 << c_register_r11;
    static const CLR_UINT32 c_register_lst_r12 = 1 << c_register_r12;
    static const CLR_UINT32 c_register_lst_r13 = 1 << c_register_r13;
    static const CLR_UINT32 c_register_lst_r14 = 1 << c_register_r14;
    static const CLR_UINT32 c_register_lst_r15 = 1 << c_register_r15;
    static const CLR_UINT32 c_register_lst_sp  = 1 << c_register_sp ;
    static const CLR_UINT32 c_register_lst_lr  = 1 << c_register_lr ;
    static const CLR_UINT32 c_register_lst_pc  = 1 << c_register_pc ;

    //--//

    static const bool c_SetCC       = true;
    static const bool c_IgnoreCC    = false;

    static const bool c_PostIndex   = false;
    static const bool c_PreIndex    = true;

    static const bool c_NoWriteBack = false;
    static const bool c_WriteBack   = true;

    static const bool c_Load        = true;
    static const bool c_Store       = false;

    static const bool c_Up          = true;
    static const bool c_Down        = false;

    static const bool c_Byte        = true;
    static const bool c_Word        = false;

    static const bool c_Link        = true;
    static const bool c_NoLink      = false;

    //--//

    struct State
    {
        typedef bool (State::*LoadFtn )( CLR_UINT32 address, CLR_UINT32& value, CLR_DataType kind );
        typedef bool (State::*StoreFtn)( CLR_UINT32 address, CLR_UINT32  value, CLR_DataType kind );

        //--//

        CLR_UINT64  m_clockTicks;
        CLR_UINT64  m_busAccess_Read;
        CLR_UINT64  m_busAccess_Write;
        CLR_UINT64  m_busAccess_WaitStates;

        CLR_UINT32  m_pc;
        CLR_UINT32  m_registers[ 16+1 ]; // The 17th register is only used to fake write-back.
        CLR_UINT32  m_psr;

        //--//

        CLR_UINT32  m_registers_Mode_USER [ 7   ]; //  R8-R14
        CLR_UINT32  m_registers_Mode_FIQ  [ 7+1 ]; //  R8-R14 PSR
        CLR_UINT32  m_registers_Mode_IRQ  [ 2+1 ]; // R13-R14 PSR
        CLR_UINT32  m_registers_Mode_SVC  [ 2+1 ]; // R13-R14 PSR
        CLR_UINT32  m_registers_Mode_ABORT[ 2+1 ]; // R13-R14 PSR
        CLR_UINT32  m_registers_Mode_UNDEF[ 2+1 ]; // R13-R14 PSR

        //--//

        CLR_UINT16  m_lookupConditions[ ArmProcessor::c_psr_cc_num ];

        //--//

        LoadFtn     m_loadFtn;
        StoreFtn    m_storeFtn;

        //--//

        void Initialize( LoadFtn loadFtn, StoreFtn storeFtn );

        bool SwitchMode( CLR_UINT32 mode );
        bool EnterMode ( CLR_UINT32 mode );

        //--//

        bool CheckConditions( CLR_UINT32 conditionField ) const
        {
            return (m_lookupConditions[ (m_psr >> ArmProcessor::c_psr_cc_shift) & ArmProcessor::c_psr_cc_mask ] & (1 << conditionField )) != 0;
        }

        static CLR_UINT32 Negative( CLR_UINT32 psr ) { return OPCODE_DECODE_EXTRACTFIELD(psr,ArmProcessor::c_psr_bit_N,1); }
        static CLR_UINT32 Zero    ( CLR_UINT32 psr ) { return OPCODE_DECODE_EXTRACTFIELD(psr,ArmProcessor::c_psr_bit_Z,1); }
        static CLR_UINT32 Carry   ( CLR_UINT32 psr ) { return OPCODE_DECODE_EXTRACTFIELD(psr,ArmProcessor::c_psr_bit_C,1); }
        static CLR_UINT32 Overflow( CLR_UINT32 psr ) { return OPCODE_DECODE_EXTRACTFIELD(psr,ArmProcessor::c_psr_bit_V,1); }

        CLR_UINT32 Negative() const { return Negative( m_psr ); }
        CLR_UINT32 Zero    () const { return Zero    ( m_psr ); }
        CLR_UINT32 Carry   () const { return Carry   ( m_psr ); }
        CLR_UINT32 Overflow() const { return Overflow( m_psr ); }

        //--//

        bool Load ( CLR_UINT32 address, CLR_UINT32& value, CLR_DataType kind ) { return (this->*m_loadFtn )( address, value, kind ); }
        bool Store( CLR_UINT32 address, CLR_UINT32  value, CLR_DataType kind ) { return (this->*m_storeFtn)( address, value, kind ); }

        //--//

    private:
        bool GetShadowRegisters( CLR_UINT32 mode, CLR_UINT32*& ptr, CLR_UINT32& num );
    };

    struct Opcode
    {
        //
        // +---------+---+---+---+---------------+---+---------+---------+---------+---------------+---------+
        // | 3 3 2 2 | 2 | 2 | 2 | 2   2   2   2 | 2 | 1 1 1 1 | 1 1 1 1 | 1 1 9 8 | 7   6   5   4 | 3 2 1 0 |
        // | 1 0 9 8 | 7 | 6 | 5 | 4   3   2   1 | 0 | 9 8 7 6 | 5 4 3 2 | 1 0     |               |         |
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+---------------+---------+
        // | Cond    | 0   0   0   1   0 | Ps| 0   0   1 1 1 1 | Rd      | 0 0 0 0   0   0   0   0 | 0 0 0 0 | MRS
        // +---------+-------------------+---+-----------------+---------+-------------------------+---------+
        // | Cond    | 0   0   0   1   0 | Pd| 1   0   S S S S | 1 1 1 1 | 0 0 0 0   0   0   0   0 | Rm      | MSR
        // +---------+-------------------+---+-----------------+---------+---------+---------------+---------+
        // | Cond    | 0   0   1   1   0 | Pd| 1   0   S S S S | 1 1 1 1 | Rotate  | Immediate               | MSR
        // +---------+---+---+---+-------+---+---+---+---------+---------+---------+-------------------------+
        // | Cond    | 0 | 0 | 1 |    Opcode     | S | Rn      | Rd      | Rotate  | Immediate               | Data Processing / PSR Transfer
        // +---------+---+---+---+---------------+---+---------+---------+---------+---+-------+---+---------+
        // | Cond    | 0 | 0 | 0 |    Opcode     | S | Rn      | Rd      | Shift Imm   | SType | 0 | Rm      | Data Processing / PSR Transfer
        // +---------+---+---+---+---------------+---+---------+---------+---------+---+-------+---+---------+
        // | Cond    | 0 | 0 | 0 |    Opcode     | S | Rn      | Rd      | Shift Rp| 0 | SType | 1 | Rm      | Data Processing / PSR Transfer
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+---+---+---+---+---------+
        // | Cond    | 0 | 0 | 0 | 0 | 0 | 0 | A | S | Rd      | Rn      | Rs      | 1 | 0 | 0 | 1 | Rm      | Multiply
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+---+---+---+---+---------+
        // | Cond    | 0 | 0 | 0 | 0 | 1 | U | A | S | RdHi    | RdLo    | Rn      | 1 | 0 | 0 | 1 | Rm      | Multiply Long
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+---+---+---+---+---------+
        // | Cond    | 0 | 0 | 0 | 1 | 0 | B | 0 | 0 | Rn      | Rd      | 0 0 0 0 | 1 | 0 | 0 | 1 | Rm      | Single Data Swap
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+---+---+---+---+---------+
        // | Cond    | 0 | 0 | 0 | 1 | 0 | 0 | 1 | 0 | 1 1 1 1 | 1 1 1 1 | 1 1 1 1 | 0 | 0 | 0 | 1 | Rn      | Branch and Exchange
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+---+---+---+---+---------+
        // | Cond    | 0 | 0 | 0 | P | U | 0 | W | L | Rn      | Rd      | 0 0 0 0 | 1 | S | H | 1 | Rm      | Halfword Data Transfer: register offset
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+---+---+---+---+---------+
        // | Cond    | 0 | 0 | 0 | P | U | 1 | W | L | Rn      | Rd      | Offset  | 1 | S | H | 1 | Offset  | Halfword Data Transfer: immediate offset
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+---+---+---+---+---------+
        // | Cond    | 0 | 1 | 0 | P | U | B | W | L | Rn      | Rd      |          Immediate Offset         | Single Data Transfer
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+---+---+---+---+---------+
        // | Cond    | 0 | 1 | 1 | P | U | B | W | L | Rn      | Rd      | Shift Imm   | SType | 0 | Rm      | Single Data Transfer
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+---+-------+---+---------+
        // | Cond    | 0 | 1 | 1 | P | U | B | W | L | Rn      | Rd      | Shift Rp| 0 | SType | 1 | Rm      | Single Data Transfer
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+-----------+---+---------+
        // | Cond    | 0 | 1 | 1 | XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX | 1 | XXXXXX  | Undefined
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+-----------+---+---------+
        // | Cond    | 1 | 0 | 0 | P | U | S | W | L | Rn      |             Register List                   | Block Data Transfer
        // +---------+---+---+---+---+---+---+---+---+---------+---------------------------------------------+
        // | Cond    | 1 | 0 | 1 | L |                               Offset                                  | Branch
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+-------------------------+
        // | Cond    | 1 | 1 | 0 | P | U | N | W | L | Rn      | CRd     | CP#     |       Offset            | Coprocessor Data Transfer
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+-----------+---+---------+
        // | Cond    | 1 | 1 | 1 | 0 | CP Opc        | CRn     | CRd     | CP#     |     CP    | 0 | CRm     | Coprocessor Data Operation
        // +---------+---+---+---+---+-----------+---+---------+---------+---------+-----------+---+---------+
        // | Cond    | 1 | 1 | 1 | 0 | CP Opc    | L | CRn     | Rd      | CP#     |     CP    | 1 | CRm     | Coprocessor Register Transfer
        // +---------+---+---+---+---+-----------+---+---------+---------+---------+-----------+---+---------+
        // | Cond    | 1 | 1 | 1 | 1 |                       Ignored by processor                            | Software Interrupt
        // +---------+---+---+---+---+-----------------------------------------------------------------------+
        //

        enum Format
        {
            MRS                    ,
            MSR_1                  ,
            MSR_2                  ,
            DataProcessing_1       ,
            DataProcessing_2       ,
            DataProcessing_3       ,
            Multiply               ,
            MultiplyLong           ,
            SingleDataSwap         ,
            BranchAndExchange      ,
            HalfwordDataTransfer_1 ,
            HalfwordDataTransfer_2 ,
            SingleDataTransfer_1   ,
            SingleDataTransfer_2   ,
            SingleDataTransfer_3   ,
            Undefined              ,
            BlockDataTransfer      ,
            Branch                 ,
            CoprocDataTransfer     ,
            CoprocDataOperation    ,
            CoprocRegisterTransfer ,
            SoftwareInterrupt      ,

            FIRST_FORMAT   = MRS,
            LAST_FORMAT    = SoftwareInterrupt,
            NUM_OF_FORMATS = (SoftwareInterrupt - MRS + 1)
        };

        static const CLR_UINT32 c_op    [ NUM_OF_FORMATS ];
        static const CLR_UINT32 c_opmask[ NUM_OF_FORMATS ];

        //--//

#define INSTRUCTION_ENCODING( name, b31, b30, b29, b28, b27, b26, b25, b24, b23, b22, b21, b20, b19, b18, b17, b16, b15, b14, b13, b12, b11, b10, b9, b8, b7, b6, b5, b4, b3, b2, b1, b0 )                                                                  \
                                                                                                                                                                                                                                                            \
        static const CLR_UINT32 c_op_##name     = ((b31 >  0 ? 1<<31 : 0) | (b30 >  0 ? 1<<30 : 0) | (b29 >  0 ? 1<<29 : 0) | (b28 >  0 ? 1<<28 : 0) | (b27 >  0 ? 1<<27 : 0) | (b26 >  0 ? 1<<26 : 0) | (b25 >  0 ? 1<<25 : 0) | (b24 >  0 ? 1<<24 : 0) |  \
                                                   (b23 >  0 ? 1<<23 : 0) | (b22 >  0 ? 1<<22 : 0) | (b21 >  0 ? 1<<21 : 0) | (b20 >  0 ? 1<<20 : 0) | (b19 >  0 ? 1<<19 : 0) | (b18 >  0 ? 1<<18 : 0) | (b17 >  0 ? 1<<17 : 0) | (b16 >  0 ? 1<<16 : 0) |  \
                                                   (b15 >  0 ? 1<<15 : 0) | (b14 >  0 ? 1<<14 : 0) | (b13 >  0 ? 1<<13 : 0) | (b12 >  0 ? 1<<12 : 0) | (b11 >  0 ? 1<<11 : 0) | (b10 >  0 ? 1<<10 : 0) | (b9  >  0 ? 1<< 9 : 0) | (b8  >  0 ? 1<< 8 : 0) |  \
                                                   (b7  >  0 ? 1<< 7 : 0) | (b6  >  0 ? 1<< 6 : 0) | (b5  >  0 ? 1<< 5 : 0) | (b4  >  0 ? 1<< 4 : 0) | (b3  >  0 ? 1<< 3 : 0) | (b2  >  0 ? 1<< 2 : 0) | (b1  >  0 ? 1<< 1 : 0) | (b0  >  0 ? 1<< 0 : 0) ); \
                                                                                                                                                                                                                                                            \
        static const CLR_UINT32 c_opmask_##name = ((b31 >= 0 ? 1<<31 : 0) | (b30 >= 0 ? 1<<30 : 0) | (b29 >= 0 ? 1<<29 : 0) | (b28 >= 0 ? 1<<28 : 0) | (b27 >= 0 ? 1<<27 : 0) | (b26 >= 0 ? 1<<26 : 0) | (b25 >= 0 ? 1<<25 : 0) | (b24 >= 0 ? 1<<24 : 0) |  \
                                                   (b23 >= 0 ? 1<<23 : 0) | (b22 >= 0 ? 1<<22 : 0) | (b21 >= 0 ? 1<<21 : 0) | (b20 >= 0 ? 1<<20 : 0) | (b19 >= 0 ? 1<<19 : 0) | (b18 >= 0 ? 1<<18 : 0) | (b17 >= 0 ? 1<<17 : 0) | (b16 >= 0 ? 1<<16 : 0) |  \
                                                   (b15 >= 0 ? 1<<15 : 0) | (b14 >= 0 ? 1<<14 : 0) | (b13 >= 0 ? 1<<13 : 0) | (b12 >= 0 ? 1<<12 : 0) | (b11 >= 0 ? 1<<11 : 0) | (b10 >= 0 ? 1<<10 : 0) | (b9  >= 0 ? 1<< 9 : 0) | (b8  >= 0 ? 1<< 8 : 0) |  \
                                                   (b7  >= 0 ? 1<< 7 : 0) | (b6  >= 0 ? 1<< 6 : 0) | (b5  >= 0 ? 1<< 5 : 0) | (b4  >= 0 ? 1<< 4 : 0) | (b3  >= 0 ? 1<< 3 : 0) | (b2  >= 0 ? 1<< 2 : 0) | (b1  >= 0 ? 1<< 1 : 0) | (b0  >= 0 ? 1<< 0 : 0) )

#define NA -1
        INSTRUCTION_ENCODING( MRS                   , NA, NA, NA, NA, 0 , 0 , 0 , 1 , 0 , NA, 0 , 0 , 1 , 1 , 1 , 1 , NA, NA, NA, NA, 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0  );
        INSTRUCTION_ENCODING( MSR_1                 , NA, NA, NA, NA, 0 , 0 , 0 , 1 , 0 , NA, 1 , 0 , NA, NA, NA, NA, 1 , 1 , 1 , 1 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , NA, NA, NA, NA );
        INSTRUCTION_ENCODING( MSR_2                 , NA, NA, NA, NA, 0 , 0 , 1 , 1 , 0 , NA, 1 , 0 , NA, NA, NA, NA, 1 , 1 , 1 , 1 , NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA );
        INSTRUCTION_ENCODING( DataProcessing_1      , NA, NA, NA, NA, 0 , 0 , 1 , NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA );
        INSTRUCTION_ENCODING( DataProcessing_2      , NA, NA, NA, NA, 0 , 0 , 0 , NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, 0 , NA, NA, NA, NA );
        INSTRUCTION_ENCODING( DataProcessing_3      , NA, NA, NA, NA, 0 , 0 , 0 , NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, 0 , NA, NA, 1 , NA, NA, NA, NA );
        INSTRUCTION_ENCODING( Multiply              , NA, NA, NA, NA, 0 , 0 , 0 , 0 , 0 , 0 , NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, 1 , 0 , 0 , 1 , NA, NA, NA, NA );
        INSTRUCTION_ENCODING( MultiplyLong          , NA, NA, NA, NA, 0 , 0 , 0 , 0 , 1 , NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, 1 , 0 , 0 , 1 , NA, NA, NA, NA );
        INSTRUCTION_ENCODING( SingleDataSwap        , NA, NA, NA, NA, 0 , 0 , 0 , 1 , 0 , NA, 0 , 0 , NA, NA, NA, NA, NA, NA, NA, NA, 0 , 0 , 0 , 0 , 1 , 0 , 0 , 1 , NA, NA, NA, NA );
        INSTRUCTION_ENCODING( BranchAndExchange     , NA, NA, NA, NA, 0 , 0 , 0 , 1 , 0 , 0 , 1 , 0 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 0 , 0 , 0 , 1 , NA, NA, NA, NA );
        INSTRUCTION_ENCODING( HalfwordDataTransfer_1, NA, NA, NA, NA, 0 , 0 , 0 , NA, NA, 0 , NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, 0 , 0 , 0 , 0 , 1 , NA, NA, 1 , NA, NA, NA, NA );
        INSTRUCTION_ENCODING( HalfwordDataTransfer_2, NA, NA, NA, NA, 0 , 0 , 0 , NA, NA, 1 , NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, 1 , NA, NA, 1 , NA, NA, NA, NA );
        INSTRUCTION_ENCODING( SingleDataTransfer_1  , NA, NA, NA, NA, 0 , 1 , 0 , NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA );
        INSTRUCTION_ENCODING( SingleDataTransfer_2  , NA, NA, NA, NA, 0 , 1 , 1 , NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, 0 , NA, NA, NA, NA );
        INSTRUCTION_ENCODING( SingleDataTransfer_3  , NA, NA, NA, NA, 0 , 1 , 1 , NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, 0 , NA, NA, 1 , NA, NA, NA, NA );
        INSTRUCTION_ENCODING( Undefined             , NA, NA, NA, NA, 0 , 1 , 1 , NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, 1 , NA, NA, NA, NA );
        INSTRUCTION_ENCODING( BlockDataTransfer     , NA, NA, NA, NA, 1 , 0 , 0 , NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA );
        INSTRUCTION_ENCODING( Branch                , NA, NA, NA, NA, 1 , 0 , 1 , NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA );
        INSTRUCTION_ENCODING( CoprocDataTransfer    , NA, NA, NA, NA, 1 , 1 , 0 , NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA );
        INSTRUCTION_ENCODING( CoprocDataOperation   , NA, NA, NA, NA, 1 , 1 , 1 , 0 , NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, 0 , NA, NA, NA, NA );
        INSTRUCTION_ENCODING( CoprocRegisterTransfer, NA, NA, NA, NA, 1 , 1 , 1 , 0 , NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, 1 , NA, NA, NA, NA );
        INSTRUCTION_ENCODING( SoftwareInterrupt     , NA, NA, NA, NA, 1 , 1 , 1 , 1 , NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA );
#undef NA

#undef INSTRUCTION_ENCODING

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        struct Encoder_MRS
        {
            bool       m_useSPSR;
            CLR_UINT32 m_Rd;

            void       Decode( CLR_UINT32 op );
            CLR_UINT32 Encode(               ) const;
            void       Print ( Opcode&    op ) const;

            bool Prepare( bool useSPSR, CLR_UINT32 Rd )
            {
                m_useSPSR = useSPSR;
                m_Rd      = Rd;

                return true;
            }
        };

        struct Encoder_MSR
        {
            bool       m_useSPSR;
            CLR_UINT32 m_fields;

        protected:
            void       Decode( CLR_UINT32 op );
            CLR_UINT32 Encode( CLR_UINT32 op ) const;
            void       Print ( Opcode&    op ) const;

            bool Prepare( bool useSPSR, CLR_UINT32 fields )
            {
                m_useSPSR = useSPSR;
                m_fields  = fields;

                return true;
            }
        };

        struct Encoder_MSR_1 : public Encoder_MSR
        {
            CLR_UINT32 m_Rm;

            void       Decode( CLR_UINT32 op );
            CLR_UINT32 Encode(               ) const;
            void       Print ( Opcode&    op ) const;

            bool Prepare( bool useSPSR, CLR_UINT32 fields, CLR_UINT32 Rm )
            {
                if(!Encoder_MSR::Prepare( useSPSR, fields )) return false;

                m_Rm = Rm;

                return true;
            }
        };

        struct Encoder_MSR_2 : public Encoder_MSR
        {
            CLR_UINT32 m_imm;
            CLR_UINT32 m_rot;

            void       Decode( CLR_UINT32 op );
            CLR_UINT32 Encode(               ) const;
            void       Print ( Opcode&    op ) const;

            bool Prepare( bool useSPSR, CLR_UINT32 fields, CLR_UINT32 value )
            {
                if(!Encoder_MSR::Prepare( useSPSR, fields )) return false;

                if(!check_DataProcessing_ImmediateValue( value, m_imm, m_rot )) return false;

                return true;
            }
        };

        //--//

        struct Encoder_DataProcessing
        {
            CLR_UINT32 m_Rn;
            CLR_UINT32 m_Rd;
            CLR_UINT32 m_alu;
            bool       m_setCC;

        protected:
            void       Decode( CLR_UINT32 op );
            CLR_UINT32 Encode( CLR_UINT32 op ) const;
            void       Print ( Opcode&    op ) const;

            bool Prepare( CLR_UINT32 alu, bool setCC, CLR_UINT32 Rd, CLR_UINT32 Rn )
            {
                m_Rn    = Rn;
                m_Rd    = Rd;
                m_alu   = alu;
                m_setCC = setCC;

                return true;
            }
        };

        struct Encoder_DataProcessing_1 : public Encoder_DataProcessing
        {
            CLR_UINT32 m_imm;
            CLR_UINT32 m_rot;

            void       Decode( CLR_UINT32 op );
            CLR_UINT32 Encode(               ) const;
            void       Print ( Opcode&    op ) const;

            bool Prepare( CLR_UINT32 alu, bool setCC, CLR_UINT32 Rd, CLR_UINT32 Rn, CLR_UINT32 value )
            {
                if(!Encoder_DataProcessing::Prepare( alu, setCC, Rd, Rn )) return false;

                if(!check_DataProcessing_ImmediateValue( value, m_imm, m_rot )) return false;

                return true;
            }
        };

        struct Encoder_DataProcessing_23 : public Encoder_DataProcessing
        {
            CLR_UINT32 m_Rm;
            CLR_UINT32 m_shiftType;

        protected:
            void       Decode( CLR_UINT32 op                       );
            CLR_UINT32 Encode( CLR_UINT32 op, CLR_UINT32 shiftType ) const;
            void       Print ( Opcode&    op                       ) const;

            bool Prepare( CLR_UINT32 alu, bool setCC, CLR_UINT32 Rd, CLR_UINT32 Rn, CLR_UINT32 Rm, CLR_UINT32 shiftType )
            {
                if(!Encoder_DataProcessing::Prepare( alu, setCC, Rd, Rn )) return false;

                m_Rm        = Rm;
                m_shiftType = shiftType;

                return true;
            }
        };

        struct Encoder_DataProcessing_2 : public Encoder_DataProcessing_23
        {
            CLR_UINT32 m_shiftValue;

            void       Decode( CLR_UINT32 op );
            CLR_UINT32 Encode(               ) const;
            void       Print ( Opcode&    op ) const;

            bool Prepare( CLR_UINT32 alu, bool setCC, CLR_UINT32 Rd, CLR_UINT32 Rn, CLR_UINT32 Rm, CLR_UINT32 shiftType, CLR_UINT32 shiftValue )
            {
                if(!Encoder_DataProcessing_23::Prepare( alu, setCC, Rd, Rn, Rm, shiftType )) return false;

                m_shiftValue = shiftValue;

                return true;
            }
        };

        struct Encoder_DataProcessing_3 : public Encoder_DataProcessing_23
        {
            CLR_UINT32 m_Rs;

            void       Decode( CLR_UINT32 op );
            CLR_UINT32 Encode(               ) const;
            void       Print ( Opcode&    op ) const;

            bool Prepare( CLR_UINT32 alu, bool setCC, CLR_UINT32 Rd, CLR_UINT32 Rn, CLR_UINT32 Rm, CLR_UINT32 shiftType, CLR_UINT32 Rs )
            {
                if(!Encoder_DataProcessing_23::Prepare( alu, setCC, Rd, Rn, Rm, shiftType )) return false;

                m_Rs = Rs;

                return true;
            }
        };

        //--//

        struct Encoder_Multiply
        {
            CLR_UINT32 m_Rd;
            CLR_UINT32 m_Rn;
            CLR_UINT32 m_Rs;
            CLR_UINT32 m_Rm;
            bool       m_setCC;
            bool       m_isAccumulate;

            void       Decode( CLR_UINT32 op );
            CLR_UINT32 Encode(               ) const;
            void       Print ( Opcode&    op ) const;

            bool Prepare( CLR_UINT32 Rd, CLR_UINT32 Rn, CLR_UINT32 Rs, CLR_UINT32 Rm, bool setCC, bool isAccumulate )
            {
                m_Rd           = Rd;
                m_Rn           = Rn;
                m_Rs           = Rs;
                m_Rm           = Rm;
                m_setCC        = setCC;
                m_isAccumulate = isAccumulate;

                return true;
            }
        };

        struct Encoder_MultiplyLong
        {
            CLR_UINT32 m_RdHi;
            CLR_UINT32 m_RdLo;
            CLR_UINT32 m_Rs;
            CLR_UINT32 m_Rm;
            bool       m_setCC;
            bool       m_isAccumulate;
            bool       m_isSigned;

            void       Decode( CLR_UINT32 op );
            CLR_UINT32 Encode(               ) const;
            void       Print ( Opcode&    op ) const;

            bool Prepare( CLR_UINT32 RdHi, CLR_UINT32 RdLo, CLR_UINT32 Rs, CLR_UINT32 Rm, bool setCC, bool isAccumulate, bool isSigned )
            {
                m_RdHi         = RdHi;
                m_RdLo         = RdLo;
                m_Rs           = Rs;
                m_Rm           = Rm;
                m_setCC        = setCC;
                m_isAccumulate = isAccumulate;
                m_isSigned     = isSigned;

                return true;
            }
        };

        //--//

        struct Encoder_SingleDataSwap
        {
            CLR_UINT32 m_Rn;
            CLR_UINT32 m_Rd;
            CLR_UINT32 m_Rm;
            bool       m_isByte;

            void       Decode( CLR_UINT32 op );
            CLR_UINT32 Encode(               ) const;
            void       Print ( Opcode&    op ) const;

            bool Prepare( CLR_UINT32 Rd, CLR_UINT32 Rn, CLR_UINT32 Rm, bool isByte )
            {
                m_Rd     = Rd;
                m_Rn     = Rn;
                m_Rm     = Rm;
                m_isByte = isByte;

                return true;
            }
        };

        //--//

        struct Encoder_Branch
        {
            CLR_INT32 m_offset;
            bool      m_isLink;

            void       Decode( CLR_UINT32 op );
            CLR_UINT32 Encode(               ) const;
            void       Print ( Opcode&    op ) const;

            bool Prepare( CLR_INT32 offset, bool isLink )
            {
                m_offset = offset;
                m_isLink = isLink;

                return true;
            }
        };

        struct Encoder_BranchAndExchange
        {
            CLR_UINT32 m_Rn;

            void       Decode( CLR_UINT32 op );
            CLR_UINT32 Encode(               ) const;
            void       Print ( Opcode&    op ) const;

            bool Prepare( CLR_UINT32 Rn )
            {
                m_Rn = Rn;

                return true;
            }
        };

        //--//

        struct Encoder_DataTransfer
        {
            CLR_UINT32 m_Rn;
            bool       m_load;
            bool       m_preIndex;
            bool       m_up;
            bool       m_writeBack;

        protected:
            void       Decode( CLR_UINT32 op );
            CLR_UINT32 Encode( CLR_UINT32 op ) const;

            bool Prepare( CLR_UINT32 Rn, bool load, bool preIndex, bool up, bool writeBack )
            {
                m_Rn        = Rn;
                m_load      = load;
                m_preIndex  = preIndex;
                m_up        = up;
                m_writeBack = writeBack;

                return true;
            }
        };

        struct Encoder_WordDataTransfer : public Encoder_DataTransfer
        {
            CLR_UINT32 m_Rd;

        protected:
            void       Decode( CLR_UINT32 op );
            CLR_UINT32 Encode( CLR_UINT32 op ) const;

            bool Prepare( CLR_UINT32 Rn, bool load, bool preIndex, bool up, bool writeBack, CLR_UINT32 Rd )
            {
                if(!Encoder_DataTransfer::Prepare( Rn, load, preIndex, up, writeBack )) return false;

                m_Rd = Rd;

                return true;
            }
        };

        struct Encoder_HalfwordDataTransfer : public Encoder_WordDataTransfer
        {
            CLR_UINT32 m_kind;

        protected:
            void       Decode   ( CLR_UINT32 op );
            CLR_UINT32 Encode   ( CLR_UINT32 op ) const;
            void       PrintPre ( Opcode&    op ) const;
            void       PrintPost( Opcode&    op ) const;

            bool Prepare( CLR_UINT32 Rn, bool load, bool preIndex, bool up, bool writeBack, CLR_UINT32 Rd, CLR_UINT32 kind )
            {
                if(!Encoder_WordDataTransfer::Prepare( Rn, load, preIndex, up, writeBack, Rd )) return false;

                m_kind = kind;

                return true;
            }
        };

        struct Encoder_HalfwordDataTransfer_1 : public Encoder_HalfwordDataTransfer
        {
            CLR_UINT32 m_Rm;

            void       Decode( CLR_UINT32 op );
            CLR_UINT32 Encode(               ) const;
            void       Print ( Opcode&    op ) const;

            bool Prepare( CLR_UINT32 Rn, bool load, bool preIndex, bool up, bool writeBack, CLR_UINT32 Rd, CLR_UINT32 kind, CLR_UINT32 Rm )
            {
                if(!Encoder_HalfwordDataTransfer::Prepare( Rn, load, preIndex, up, writeBack, Rd, kind )) return false;

                m_Rm = Rm;

                return true;
            }
        };

        struct Encoder_HalfwordDataTransfer_2 : public Encoder_HalfwordDataTransfer
        {
            CLR_UINT32 m_offset;

            void       Decode( CLR_UINT32 op );
            CLR_UINT32 Encode(               ) const;
            void       Print ( Opcode&    op ) const;

            bool Prepare( CLR_UINT32 Rn, bool load, bool preIndex, bool up, bool writeBack, CLR_UINT32 Rd, CLR_UINT32 kind, CLR_UINT32 offset )
            {
                if(!Encoder_HalfwordDataTransfer::Prepare( Rn, load, preIndex, up, writeBack, Rd, kind )) return false;

                m_offset = offset;

                return true;
            }
        };

        struct Encoder_SingleDataTransfer : public Encoder_WordDataTransfer
        {
            bool m_isByte;

        protected:
            void       Decode   ( CLR_UINT32 op );
            CLR_UINT32 Encode   ( CLR_UINT32 op ) const;
            void       PrintPre ( Opcode&    op ) const;
            void       PrintPost( Opcode&    op ) const;

            bool Prepare( CLR_UINT32 Rn, bool load, bool preIndex, bool up, bool writeBack, CLR_UINT32 Rd, bool isByte )
            {
                if(!Encoder_WordDataTransfer::Prepare( Rn, load, preIndex, up, writeBack, Rd )) return false;

                m_isByte = isByte;

                return true;
            }
        };

        struct Encoder_SingleDataTransfer_1 : public Encoder_SingleDataTransfer
        {
            CLR_UINT32 m_offset;

            void       Decode( CLR_UINT32 op );
            CLR_UINT32 Encode(               ) const;
            void       Print ( Opcode&    op ) const;

            bool Prepare( CLR_UINT32 Rn, bool load, bool preIndex, bool up, bool writeBack, CLR_UINT32 Rd, bool isByte, CLR_UINT32 offset )
            {
                if(!Encoder_SingleDataTransfer::Prepare( Rn, load, preIndex, up, writeBack, Rd, isByte )) return false;

                m_offset = offset;

                return true;
            }
        };

        struct Encoder_SingleDataTransfer_23 : public Encoder_SingleDataTransfer
        {
            CLR_UINT32 m_Rm;
            CLR_UINT32 m_shiftType;

        protected:
            void       Decode   ( CLR_UINT32 op                       );
            CLR_UINT32 Encode   ( CLR_UINT32 op, CLR_UINT32 shiftType ) const;
            void       PrintPre ( Opcode&    op                       ) const;
            void       PrintPost( Opcode&    op                       ) const;

            bool Prepare( CLR_UINT32 Rn, bool load, bool preIndex, bool up, bool writeBack, CLR_UINT32 Rd, bool isByte, CLR_UINT32 Rm, CLR_UINT32 shiftType )
            {
                if(!Encoder_SingleDataTransfer::Prepare( Rn, load, preIndex, up, writeBack, Rd, isByte )) return false;

                m_Rm        = Rm;
                m_shiftType = shiftType;

                return true;
            }
        };

        struct Encoder_SingleDataTransfer_2 : public Encoder_SingleDataTransfer_23
        {
            CLR_UINT32 m_shiftValue;

            void       Decode( CLR_UINT32 op );
            CLR_UINT32 Encode(               ) const;
            void       Print ( Opcode&    op ) const;

            bool Prepare( CLR_UINT32 Rn, bool load, bool preIndex, bool up, bool writeBack, CLR_UINT32 Rd, bool isByte, CLR_UINT32 Rm, CLR_UINT32 shiftType, CLR_UINT32 shiftValue )
            {
                if(!Encoder_SingleDataTransfer_23::Prepare( Rn, load, preIndex, up, writeBack, Rd, isByte, Rm, shiftType )) return false;

                m_shiftValue = shiftValue;

                return true;
            }
        };

        struct Encoder_SingleDataTransfer_3 : public Encoder_SingleDataTransfer_23
        {
            CLR_UINT32 m_Rs;

            void       Decode( CLR_UINT32 op );
            CLR_UINT32 Encode(               ) const;
            void       Print ( Opcode&    op ) const;

            bool Prepare( CLR_UINT32 Rn, bool load, bool preIndex, bool up, bool writeBack, CLR_UINT32 Rd, bool isByte, CLR_UINT32 Rm, CLR_UINT32 shiftType, CLR_UINT32 Rs )
            {
                if(!Encoder_SingleDataTransfer_23::Prepare( Rn, load, preIndex, up, writeBack, Rd, isByte, Rm, shiftType )) return false;

                m_Rs = Rs;

                return true;
            }
        };

        struct Encoder_BlockDataTransfer : public Encoder_DataTransfer
        {
            CLR_UINT32 m_Lst;
            bool       m_loadPSR;

            void       Decode( CLR_UINT32 op );
            CLR_UINT32 Encode(               ) const;
            void       Print ( Opcode&    op ) const;

            bool Prepare( CLR_UINT32 Rn, bool load, bool preIndex, bool up, bool writeBack, CLR_UINT32 lst, bool loadPSR )
            {
                if(!Encoder_DataTransfer::Prepare( Rn, load, preIndex, up, writeBack )) return false;

                m_Lst     = lst;
                m_loadPSR = loadPSR;

                return true;
            }
        };

        struct Encoder_SoftwareInterrupt
        {
            CLR_UINT32 m_value;

            void       Decode( CLR_UINT32 op );
            CLR_UINT32 Encode(               ) const;
            void       Print ( Opcode&    op ) const;

            bool Prepare( CLR_UINT32 value )
            {
                m_value = value;

                return true;
            }
        };

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        CLR_UINT32 m_op;
        Format     m_fmt;
        CLR_UINT32 m_conditionCodes;

        union Formats
        {
            Encoder_MRS                     m_MRS;
            Encoder_MSR_1                   m_MSR_1;
            Encoder_MSR_2                   m_MSR_2;
            Encoder_DataProcessing_1        m_DataProcessing_1;
            Encoder_DataProcessing_2        m_DataProcessing_2;
            Encoder_DataProcessing_3        m_DataProcessing_3;
            Encoder_Multiply                m_Multiply;
            Encoder_MultiplyLong            m_MultiplyLong;
            Encoder_SingleDataSwap          m_SingleDataSwap;
            Encoder_Branch                  m_Branch;
            Encoder_BranchAndExchange       m_BranchAndExchange;
            Encoder_HalfwordDataTransfer_1  m_HalfwordDataTransfer_1;
            Encoder_HalfwordDataTransfer_2  m_HalfwordDataTransfer_2;
            Encoder_SingleDataTransfer_1    m_SingleDataTransfer_1;
            Encoder_SingleDataTransfer_2    m_SingleDataTransfer_2;
            Encoder_SingleDataTransfer_3    m_SingleDataTransfer_3;
            Encoder_BlockDataTransfer       m_BlockDataTransfer;
            Encoder_SoftwareInterrupt       m_SoftwareInterrupt;

            CLR_UINT32 Encode( Format fmt ) const;

        } m_formats;

        //--//

        static bool Execute( CLR_UINT32 op, State& st );

        bool       Decode             ( CLR_UINT32 op               );
        CLR_UINT32 Encode             (                             ) const;
        CLR_UINT32 EncodeWithCondition( CLR_UINT32 cond = c_cond_AL );
        bool       CheckConditions    ( State&     st               );
        void       Print              (                             );


        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        bool Prepare_MRS  ( bool useSPSR,                    CLR_UINT32 Rd    ) { m_fmt = MRS  ; return m_formats.m_MRS  .Prepare( useSPSR,         Rd    ); }
        bool Prepare_MSR_1( bool useSPSR, CLR_UINT32 fields, CLR_UINT32 Rm    ) { m_fmt = MSR_1; return m_formats.m_MSR_1.Prepare( useSPSR, fields, Rm    ); }
        bool Prepare_MSR_2( bool useSPSR, CLR_UINT32 fields, CLR_UINT32 value ) { m_fmt = MSR_2; return m_formats.m_MSR_2.Prepare( useSPSR, fields, value ); }

        //--//

        bool Prepare_DataProcessing_1( CLR_UINT32 alu, bool setCC, CLR_UINT32 Rd, CLR_UINT32 Rn, CLR_UINT32 value                                              ) { m_fmt = DataProcessing_1; return m_formats.m_DataProcessing_1.Prepare( alu, setCC, Rd, Rn, value                        ); }
        bool Prepare_DataProcessing_2( CLR_UINT32 alu, bool setCC, CLR_UINT32 Rd, CLR_UINT32 Rn, CLR_UINT32 Rm   , CLR_UINT32 shiftType, CLR_UINT32 shiftValue ) { m_fmt = DataProcessing_2; return m_formats.m_DataProcessing_2.Prepare( alu, setCC, Rd, Rn, Rm   , shiftType, shiftValue ); }
        bool Prepare_DataProcessing_3( CLR_UINT32 alu, bool setCC, CLR_UINT32 Rd, CLR_UINT32 Rn, CLR_UINT32 Rm   , CLR_UINT32 shiftType, CLR_UINT32 Rs         ) { m_fmt = DataProcessing_3; return m_formats.m_DataProcessing_3.Prepare( alu, setCC, Rd, Rn, Rm   , shiftType, Rs         ); }

        //--//

        bool Prepare_Multiply    ( CLR_UINT32 Rd  , CLR_UINT32 Rn  , CLR_UINT32 Rs, CLR_UINT32 Rm, bool setCC, bool isAccumulate                ) { m_fmt = Multiply    ; return m_formats.m_Multiply    .Prepare( Rd  , Rn  , Rs, Rm, setCC, isAccumulate           ); }
        bool Prepare_MultiplyLong( CLR_UINT32 RdHi, CLR_UINT32 RdLo, CLR_UINT32 Rs, CLR_UINT32 Rm, bool setCC, bool isAccumulate, bool isSigned ) { m_fmt = MultiplyLong; return m_formats.m_MultiplyLong.Prepare( RdHi, RdLo, Rs, Rm, setCC, isAccumulate, isSigned ); }

        //--//

        bool Prepare_SingleDataSwap( CLR_UINT32 Rd, CLR_UINT32 Rn, CLR_UINT32 Rm, bool isByte ) { m_fmt = SingleDataSwap; return m_formats.m_SingleDataSwap.Prepare( Rd, Rn, Rm, isByte ); }

        //--//

        bool Prepare_Branch           ( CLR_INT32  offset, bool isLink ) { m_fmt = Branch           ; return m_formats.m_Branch           .Prepare( offset - 8, isLink ); }
        bool Prepare_BranchAndExchange( CLR_UINT32 Rn                  ) { m_fmt = BranchAndExchange; return m_formats.m_BranchAndExchange.Prepare( Rn                 ); }

        //--//

        bool Prepare_HalfwordDataTransfer_1( CLR_UINT32 Rn, bool load, bool preIndex, bool up, bool writeBack, CLR_UINT32 Rd , CLR_UINT32 kind, CLR_UINT32 Rm                                                  ) { m_fmt = HalfwordDataTransfer_1; return m_formats.m_HalfwordDataTransfer_1.Prepare( Rn, load, preIndex, up, writeBack, Rd , kind   , Rm                             ); }
        bool Prepare_HalfwordDataTransfer_2( CLR_UINT32 Rn, bool load, bool preIndex, bool up, bool writeBack, CLR_UINT32 Rd , CLR_UINT32 kind, CLR_UINT32 offset                                              ) { m_fmt = HalfwordDataTransfer_2; return m_formats.m_HalfwordDataTransfer_2.Prepare( Rn, load, preIndex, up, writeBack, Rd , kind   , offset                         ); }
        bool Prepare_SingleDataTransfer_1  ( CLR_UINT32 Rn, bool load, bool preIndex, bool up, bool writeBack, CLR_UINT32 Rd , bool isByte    , CLR_UINT32 offset                                              ) { m_fmt = SingleDataTransfer_1  ; return m_formats.m_SingleDataTransfer_1  .Prepare( Rn, load, preIndex, up, writeBack, Rd , isByte , offset                         ); }
        bool Prepare_SingleDataTransfer_2  ( CLR_UINT32 Rn, bool load, bool preIndex, bool up, bool writeBack, CLR_UINT32 Rd , bool isByte    , CLR_UINT32 Rm    , CLR_UINT32 shiftType, CLR_UINT32 shiftValue ) { m_fmt = SingleDataTransfer_2  ; return m_formats.m_SingleDataTransfer_2  .Prepare( Rn, load, preIndex, up, writeBack, Rd , isByte , Rm    , shiftType,  shiftValue ); }
        bool Prepare_SingleDataTransfer_3  ( CLR_UINT32 Rn, bool load, bool preIndex, bool up, bool writeBack, CLR_UINT32 Rd , bool isByte    , CLR_UINT32 Rm    , CLR_UINT32 shiftType, CLR_UINT32 Rs         ) { m_fmt = SingleDataTransfer_3  ; return m_formats.m_SingleDataTransfer_3  .Prepare( Rn, load, preIndex, up, writeBack, Rd , isByte , Rm    , shiftType,  Rs         ); }
        bool Prepare_BlockDataTransfer     ( CLR_UINT32 Rn, bool load, bool preIndex, bool up, bool writeBack, CLR_UINT32 lst, bool loadPSR = false                                                            ) { m_fmt = BlockDataTransfer     ; return m_formats.m_BlockDataTransfer     .Prepare( Rn, load, preIndex, up, writeBack, lst, loadPSR                                 ); }

        bool Prepare_SoftwareInterrupt( CLR_UINT32 value ) { m_fmt = SoftwareInterrupt; return m_formats.m_SoftwareInterrupt.Prepare( value ); }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        CLR_UINT32 m_dump_address;
        LPSTR      m_dump_szBuffer;
        size_t     m_dump_iBuffer;

        void InitText      ( LPSTR  szBuffer, size_t iBuffer, CLR_UINT32 address );
        void AppendText    ( LPCSTR format, ...                                  );
        void AppendMnemonic( LPCSTR format, ...                                  );

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        static CLR_UINT32 get_ConditionCodes( CLR_UINT32 op  ) { return OPCODE_DECODE_EXTRACTFIELD(op ,28,4); }
        static CLR_UINT32 set_ConditionCodes( CLR_UINT32 val ) { return OPCODE_DECODE_INSERTFIELD (val,28,4); }

        static bool       get_ShouldSetConditions( CLR_UINT32 op  ) { return OPCODE_DECODE_CHECKFLAG(op ,20); }
        static CLR_UINT32 set_ShouldSetConditions( bool       val ) { return OPCODE_DECODE_SETFLAG  (val,20); }

        static CLR_UINT32 get_Register1( CLR_UINT32 op  ) { return OPCODE_DECODE_EXTRACTFIELD(op ,16,4); }
        static CLR_UINT32 set_Register1( CLR_UINT32 val ) { return OPCODE_DECODE_INSERTFIELD (val,16,4); }
        static CLR_UINT32 get_Register2( CLR_UINT32 op  ) { return OPCODE_DECODE_EXTRACTFIELD(op ,12,4); }
        static CLR_UINT32 set_Register2( CLR_UINT32 val ) { return OPCODE_DECODE_INSERTFIELD (val,12,4); }
        static CLR_UINT32 get_Register3( CLR_UINT32 op  ) { return OPCODE_DECODE_EXTRACTFIELD(op , 8,4); }
        static CLR_UINT32 set_Register3( CLR_UINT32 val ) { return OPCODE_DECODE_INSERTFIELD (val, 8,4); }
        static CLR_UINT32 get_Register4( CLR_UINT32 op  ) { return OPCODE_DECODE_EXTRACTFIELD(op , 0,4); }
        static CLR_UINT32 set_Register4( CLR_UINT32 val ) { return OPCODE_DECODE_INSERTFIELD (val, 0,4); }

        //--//

        static bool       get_Multiply_IsAccumulate( CLR_UINT32 op  ) { return OPCODE_DECODE_CHECKFLAG(op ,21); }
        static CLR_UINT32 set_Multiply_IsAccumulate( bool       val ) { return OPCODE_DECODE_SETFLAG  (val,21); }
        static bool       get_Multiply_IsSigned    ( CLR_UINT32 op  ) { return OPCODE_DECODE_CHECKFLAG(op ,22); }
        static CLR_UINT32 set_Multiply_IsSigned    ( bool       val ) { return OPCODE_DECODE_SETFLAG  (val,22); }

        //--//

        static bool       get_StatusRegister_IsSPSR( CLR_UINT32 op  ) { return OPCODE_DECODE_CHECKFLAG   (op ,22  ); }
        static CLR_UINT32 set_StatusRegister_IsSPSR( bool       val ) { return OPCODE_DECODE_SETFLAG     (val,22  ); }
        static CLR_UINT32 get_StatusRegister_Fields( CLR_UINT32 op  ) { return OPCODE_DECODE_EXTRACTFIELD(op ,16,4); }
        static CLR_UINT32 set_StatusRegister_Fields( CLR_UINT32 val ) { return OPCODE_DECODE_INSERTFIELD (val,16,4); }

        //--//

        static CLR_UINT32 get_Shift_Type     ( CLR_UINT32 op  ) { return OPCODE_DECODE_EXTRACTFIELD(op ,5,2); }
        static CLR_UINT32 set_Shift_Type     ( CLR_UINT32 val ) { return OPCODE_DECODE_INSERTFIELD (val,5,2); }
        static CLR_UINT32 get_Shift_Immediate( CLR_UINT32 op  ) { return OPCODE_DECODE_EXTRACTFIELD(op ,7,5); }
        static CLR_UINT32 set_Shift_Immediate( CLR_UINT32 val ) { return OPCODE_DECODE_INSERTFIELD (val,7,5); }
        static CLR_UINT32 get_Shift_Register ( CLR_UINT32 op  ) { return OPCODE_DECODE_EXTRACTFIELD(op ,8,4); }
        static CLR_UINT32 set_Shift_Register ( CLR_UINT32 val ) { return OPCODE_DECODE_INSERTFIELD (val,8,4); }

        //--//

        static CLR_UINT32 get_DataProcessing_Operation( CLR_UINT32 op  ) { return OPCODE_DECODE_EXTRACTFIELD(op  ,21,4); }
        static CLR_UINT32 set_DataProcessing_Operation( CLR_UINT32 val ) { return OPCODE_DECODE_INSERTFIELD (val ,21,4); }

        static CLR_UINT32 get_DataProcessing_ImmediateSeed    ( CLR_UINT32 op  ) { return OPCODE_DECODE_EXTRACTFIELD(op ,0,8); }
        static CLR_UINT32 set_DataProcessing_ImmediateSeed    ( CLR_UINT32 val ) { return OPCODE_DECODE_INSERTFIELD (val,0,8); }
        static CLR_UINT32 get_DataProcessing_ImmediateRotation( CLR_UINT32 op  ) { return OPCODE_DECODE_EXTRACTFIELD(op ,8,4); }
        static CLR_UINT32 set_DataProcessing_ImmediateRotation( CLR_UINT32 val ) { return OPCODE_DECODE_INSERTFIELD (val,8,4); }

        static CLR_UINT32 get_DataProcessing_ImmediateValue( CLR_UINT32 op )
        {
            return get_DataProcessing_ImmediateValue( get_DataProcessing_ImmediateSeed( op ), get_DataProcessing_ImmediateRotation( op ) );
        }

        static CLR_UINT32 get_DataProcessing_ImmediateValue( CLR_UINT32 imm, CLR_UINT32 rot )
        {
            rot *= 2;

            return (imm >> rot) | (imm << (32 - rot));
        }

        static bool check_DataProcessing_ImmediateValue( CLR_UINT32 val, CLR_UINT32& immRes, CLR_UINT32& rotRes )
        {
            CLR_UINT32 imm = val;
            CLR_UINT32 rot = 0;

            while(imm & ~0xFF)
            {
                if(rot == 16)
                {
                    return false;
                }

                imm = (imm << 2) | (imm >> 30);
                rot++;
            }

            immRes = imm;
            rotRes = rot;
            return true;
        }

        //--//

        static bool       get_DataTransfer_IsLoad         ( CLR_UINT32 op  ) { return OPCODE_DECODE_CHECKFLAG   (op ,20   ); }
        static CLR_UINT32 set_DataTransfer_IsLoad         ( bool       val ) { return OPCODE_DECODE_SETFLAG     (val,20   ); }
        static bool       get_DataTransfer_ShouldWriteBack( CLR_UINT32 op  ) { return OPCODE_DECODE_CHECKFLAG   (op ,21   ); }
        static CLR_UINT32 set_DataTransfer_ShouldWriteBack( bool       val ) { return OPCODE_DECODE_SETFLAG     (val,21   ); }
        static bool       get_DataTransfer_IsByteTransfer ( CLR_UINT32 op  ) { return OPCODE_DECODE_CHECKFLAG   (op ,22   ); }
        static CLR_UINT32 set_DataTransfer_IsByteTransfer ( bool       val ) { return OPCODE_DECODE_SETFLAG     (val,22   ); }
        static bool       get_DataTransfer_IsUp           ( CLR_UINT32 op  ) { return OPCODE_DECODE_CHECKFLAG   (op ,23   ); }
        static CLR_UINT32 set_DataTransfer_IsUp           ( bool       val ) { return OPCODE_DECODE_SETFLAG     (val,23   ); }
        static bool       get_DataTransfer_IsPreIndexing  ( CLR_UINT32 op  ) { return OPCODE_DECODE_CHECKFLAG   (op ,24   ); }
        static CLR_UINT32 set_DataTransfer_IsPreIndexing  ( bool       val ) { return OPCODE_DECODE_SETFLAG     (val,24   ); }
        static CLR_UINT32 get_DataTransfer_Offset         ( CLR_UINT32 op  ) { return OPCODE_DECODE_EXTRACTFIELD(op , 0,12); }
        static CLR_UINT32 set_DataTransfer_Offset         ( CLR_UINT32 val ) { return OPCODE_DECODE_INSERTFIELD (val, 0,12); }

        //--//

        static CLR_UINT32 get_HalfWordDataTransfer_Kind  ( CLR_UINT32 op  ) { return  OPCODE_DECODE_EXTRACTFIELD(op    ,5,2)                                            ; }
        static CLR_UINT32 set_HalfWordDataTransfer_Kind  ( CLR_UINT32 val ) { return  OPCODE_DECODE_INSERTFIELD (val   ,5,2)                                            ; }
        static CLR_UINT32 get_HalfWordDataTransfer_Offset( CLR_UINT32 op  ) { return (OPCODE_DECODE_EXTRACTFIELD(op    ,8,4) << 4) | OPCODE_DECODE_EXTRACTFIELD(op ,0,4); }
        static CLR_UINT32 set_HalfWordDataTransfer_Offset( CLR_UINT32 val ) { return  OPCODE_DECODE_INSERTFIELD (val>>4,8,4)       | OPCODE_DECODE_INSERTFIELD (val,0,4); }

        //--//

        static bool       get_BlockDataTransfer_LoadPSR     ( CLR_UINT32 op  ) { return OPCODE_DECODE_CHECKFLAG   (op ,22   ); }
        static CLR_UINT32 set_BlockDataTransfer_LoadPSR     ( bool       val ) { return OPCODE_DECODE_SETFLAG     (val,22   ); }
        static CLR_UINT32 get_BlockDataTransfer_RegisterList( CLR_UINT32 op  ) { return OPCODE_DECODE_EXTRACTFIELD(op ,0 ,16); }
        static CLR_UINT32 set_BlockDataTransfer_RegisterList( CLR_UINT32 val ) { return OPCODE_DECODE_INSERTFIELD (val,0 ,16); }

        //--//

        static bool       get_Branch_IsLink( CLR_UINT32 op  ) { return OPCODE_DECODE_CHECKFLAG(op ,24); }
        static CLR_UINT32 set_Branch_IsLink( bool       val ) { return OPCODE_DECODE_SETFLAG  (val,24); }

        static CLR_INT32  get_Branch_Offset( CLR_UINT32 op  ) { return ((CLR_INT32)(op << 8)) >> 6; }
        static CLR_UINT32 set_Branch_Offset( CLR_INT32  val ) { return OPCODE_DECODE_INSERTFIELD(val>>2,0,24); }

        //--//

        static CLR_UINT32 get_SoftwareInterrupt_Immediate( CLR_UINT32 op  ) { return OPCODE_DECODE_EXTRACTFIELD(op ,0,24); }
        static CLR_UINT32 set_SoftwareInterrupt_Immediate( CLR_UINT32 val ) { return OPCODE_DECODE_INSERTFIELD (val,0,24); }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        static void Print( CLR_UINT32 address, CLR_UINT32 op );

        static LPCSTR DumpInstructionFormat( size_t     fmt   );
        static LPCSTR DumpOperation        ( CLR_UINT32 alu   );
        static LPCSTR DumpCondition        ( CLR_UINT32 cond  );
        static LPCSTR DumpRegister         ( CLR_UINT32 reg   );
        static LPCSTR DumpShiftType        ( CLR_UINT32 stype );
        static LPCSTR DumpHalfWordKind     ( CLR_UINT32 kind  );

        static size_t FindInstructionFormat( CLR_UINT32 op );
    };
};

////////////////////////////////////////////////////////////////////////////////////////////////////

#endif //  _TINYCLR_JITTER_ARM_H_
