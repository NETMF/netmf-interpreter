//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace Microsoft.SPOT.Debugger
{
    /// <include file='doc\ArmDisassembler.uex' path='docs/doc[@for="ArmDisassembler"]/*' />
    public static class ArmDisassembler
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
        public const uint op_MRS                    = 0x010F0000; public const uint opmask_MRS                    = 0x0FBF0FFF;
        public const uint op_MSR_1                  = 0x0120F000; public const uint opmask_MSR_1                  = 0x0FB0FFF0;
        public const uint op_MSR_2                  = 0x0320F000; public const uint opmask_MSR_2                  = 0x0FB0F000;
        public const uint op_DataProcessing_1       = 0x02000000; public const uint opmask_DataProcessing_1       = 0x0E000000;
        public const uint op_DataProcessing_2       = 0x00000000; public const uint opmask_DataProcessing_2       = 0x0E000010;
        public const uint op_DataProcessing_3       = 0x00000010; public const uint opmask_DataProcessing_3       = 0x0E000090;
        public const uint op_Multiply               = 0x00000090; public const uint opmask_Multiply               = 0x0FC000F0;
        public const uint op_MultiplyLong           = 0x00800090; public const uint opmask_MultiplyLong           = 0x0F8000F0;
        public const uint op_SingleDataSwap         = 0x01000090; public const uint opmask_SingleDataSwap         = 0x0FB00FF0;
        public const uint op_BranchAndExchange      = 0x012FFF10; public const uint opmask_BranchAndExchange      = 0x0FFFFFF0;
        public const uint op_HalfwordDataTransfer_1 = 0x00000090; public const uint opmask_HalfwordDataTransfer_1 = 0x0E400F90;
        public const uint op_HalfwordDataTransfer_2 = 0x00400090; public const uint opmask_HalfwordDataTransfer_2 = 0x0E400090;
        public const uint op_SingleDataTransfer_1   = 0x04000000; public const uint opmask_SingleDataTransfer_1   = 0x0E000000;
        public const uint op_SingleDataTransfer_2   = 0x06000000; public const uint opmask_SingleDataTransfer_2   = 0x0E000010;
        public const uint op_SingleDataTransfer_3   = 0x06000010; public const uint opmask_SingleDataTransfer_3   = 0x0E000090;
        public const uint op_Undefined              = 0x06000010; public const uint opmask_Undefined              = 0x0E000010;
        public const uint op_BlockDataTransfer      = 0x08000000; public const uint opmask_BlockDataTransfer      = 0x0E000000;
        public const uint op_Branch                 = 0x0A000000; public const uint opmask_Branch                 = 0x0E000000;
        public const uint op_CoprocDataTransfer     = 0x0C000000; public const uint opmask_CoprocDataTransfer     = 0x0E000000;
        public const uint op_CoprocDataOperation    = 0x0E000000; public const uint opmask_CoprocDataOperation    = 0x0F000010;
        public const uint op_CoprocRegisterTransfer = 0x0E000010; public const uint opmask_CoprocRegisterTransfer = 0x0F000010;
        public const uint op_SoftwareInterrupt      = 0x0F000000; public const uint opmask_SoftwareInterrupt      = 0x0F000000;

        //--//

        public const uint c_psr_bit_T      = 5;
        public const uint c_psr_bit_F      = 6;
        public const uint c_psr_bit_I      = 7;
        public const uint c_psr_bit_V      = 28;
        public const uint c_psr_bit_C      = 29;
        public const uint c_psr_bit_Z      = 30;
        public const uint c_psr_bit_N      = 31;


        public const uint c_psr_T          = 1U << (int)c_psr_bit_T;
        public const uint c_psr_F          = 1U << (int)c_psr_bit_F;
        public const uint c_psr_I          = 1U << (int)c_psr_bit_I;
        public const uint c_psr_V          = 1U << (int)c_psr_bit_V;
        public const uint c_psr_C          = 1U << (int)c_psr_bit_C;
        public const uint c_psr_Z          = 1U << (int)c_psr_bit_Z;
        public const uint c_psr_N          = 1U << (int)c_psr_bit_N;


        public const uint c_psr_cc_shift   =                          c_psr_bit_V;
        public const uint c_psr_cc_num     = 1 << (int)(c_psr_bit_N - c_psr_bit_V + 1);
        public const uint c_psr_cc_mask    = c_psr_cc_num - 1;


        public const uint c_psr_mode       = 0x0000001F;
        public const uint c_psr_mode_USER  = 0x00000010;
        public const uint c_psr_mode_FIQ   = 0x00000011;
        public const uint c_psr_mode_IRQ   = 0x00000012;
        public const uint c_psr_mode_SVC   = 0x00000013;
        public const uint c_psr_mode_ABORT = 0x00000017;
        public const uint c_psr_mode_UNDEF = 0x0000001B;
        public const uint c_psr_mode_SYS   = 0x0000001F;

        //--//

        /////////////////////////////////////////
        public const uint c_cond_EQ     = 0x0; //  Z set equal
        public const uint c_cond_NE     = 0x1; //  Z clear not equal
        public const uint c_cond_CS     = 0x2; //  C set unsigned higher or same
        public const uint c_cond_CC     = 0x3; //  C clear unsigned lower
        public const uint c_cond_MI     = 0x4; //  N set negative
        public const uint c_cond_PL     = 0x5; //  N clear positive or zero
        public const uint c_cond_VS     = 0x6; //  V set overflow
        public const uint c_cond_VC     = 0x7; //  V clear no overflow
        public const uint c_cond_HI     = 0x8; //  C set and Z clear unsigned higher
        public const uint c_cond_LS     = 0x9; //  C clear or Z set unsigned lower or same
        public const uint c_cond_GE     = 0xA; //  N equals V greater or equal
        public const uint c_cond_LT     = 0xB; //  N not equal to V less than
        public const uint c_cond_GT     = 0xC; //  Z clear AND (N equals V) greater than
        public const uint c_cond_LE     = 0xD; //  Z set OR (N not equal to V) less than or equal
        public const uint c_cond_AL     = 0xE; //  (ignored) always
        public const uint c_cond_UNUSED = 0xF; //
        /////////////////////////////////////////
        public const uint c_cond_NUM    = 0x10;

        ///////////////////////////////////////////
        public const uint c_operation_AND = 0x0; // operand1 AND operand2
        public const uint c_operation_EOR = 0x1; // operand1 EOR operand2
        public const uint c_operation_SUB = 0x2; // operand1 - operand2
        public const uint c_operation_RSB = 0x3; // operand2 - operand1
        public const uint c_operation_ADD = 0x4; // operand1 + operand2
        public const uint c_operation_ADC = 0x5; // operand1 + operand2 + carry
        public const uint c_operation_SBC = 0x6; // operand1 - operand2 + carry - 1
        public const uint c_operation_RSC = 0x7; // operand2 - operand1 + carry - 1
        public const uint c_operation_TST = 0x8; // as AND, but result is not written
        public const uint c_operation_TEQ = 0x9; // as EOR, but result is not written
        public const uint c_operation_CMP = 0xA; // as SUB, but result is not written
        public const uint c_operation_CMN = 0xB; // as ADD, but result is not written
        public const uint c_operation_ORR = 0xC; // operand1 OR operand2
        public const uint c_operation_MOV = 0xD; // operand2(operand1 is ignored)
        public const uint c_operation_BIC = 0xE; // operand1 AND NOT operand2(Bit clear)
        public const uint c_operation_MVN = 0xF; // NOT operand2(operand1 is ignored)
        ///////////////////////////////////////////

        //--//

        ///////////////////////////////////////
        public const uint c_shift_LSL = 0x0; // logical shift left
        public const uint c_shift_LSR = 0x1; // logical shift right
        public const uint c_shift_ASR = 0x2; // arithmetic shift right
        public const uint c_shift_ROR = 0x3; // rotate right
        public const uint c_shift_RRX = 0x4; // rotate right with extend
        ///////////////////////////////////////

        //--//

        //////////////////////////////////////////////
        public const uint c_halfwordkind_SWP = 0x0; //
        public const uint c_halfwordkind_U2  = 0x1; //
        public const uint c_halfwordkind_I1  = 0x2; //
        public const uint c_halfwordkind_I2  = 0x3; //
        //////////////////////////////////////////////

        //--//

        /////////////////////////////////////////
        public const uint c_psr_field_c = 0x1; // the control field   PSR[ 7: 0]
        public const uint c_psr_field_x = 0x2; // the extension field PSR[15: 8]
        public const uint c_psr_field_s = 0x4; // the status field    PSR[23:16]
        public const uint c_psr_field_f = 0x8; // the flags field     PSR[31:24]
        /////////////////////////////////////////

        //--//

        /////////////////////////////////////////
        public const uint c_register_r0  =  0; //
        public const uint c_register_r1  =  1; //
        public const uint c_register_r2  =  2; //
        public const uint c_register_r3  =  3; //
        public const uint c_register_r4  =  4; //
        public const uint c_register_r5  =  5; //
        public const uint c_register_r6  =  6; //
        public const uint c_register_r7  =  7; //
        public const uint c_register_r8  =  8; //
        public const uint c_register_r9  =  9; //
        public const uint c_register_r10 = 10; //
        public const uint c_register_r11 = 11; //
        public const uint c_register_r12 = 12; //
        public const uint c_register_r13 = 13; //
        public const uint c_register_r14 = 14; //
        public const uint c_register_r15 = 15; //
        public const uint c_register_sp  = 13; //
        public const uint c_register_lr  = 14; //
        public const uint c_register_pc  = 15; //
        /////////////////////////////////////////

        public const uint c_register_lst_r0  = 1U << (int)c_register_r0 ;
        public const uint c_register_lst_r1  = 1U << (int)c_register_r1 ;
        public const uint c_register_lst_r2  = 1U << (int)c_register_r2 ;
        public const uint c_register_lst_r3  = 1U << (int)c_register_r3 ;
        public const uint c_register_lst_r4  = 1U << (int)c_register_r4 ;
        public const uint c_register_lst_r5  = 1U << (int)c_register_r5 ;
        public const uint c_register_lst_r6  = 1U << (int)c_register_r6 ;
        public const uint c_register_lst_r7  = 1U << (int)c_register_r7 ;
        public const uint c_register_lst_r8  = 1U << (int)c_register_r8 ;
        public const uint c_register_lst_r9  = 1U << (int)c_register_r9 ;
        public const uint c_register_lst_r10 = 1U << (int)c_register_r10;
        public const uint c_register_lst_r11 = 1U << (int)c_register_r11;
        public const uint c_register_lst_r12 = 1U << (int)c_register_r12;
        public const uint c_register_lst_r13 = 1U << (int)c_register_r13;
        public const uint c_register_lst_r14 = 1U << (int)c_register_r14;
        public const uint c_register_lst_r15 = 1U << (int)c_register_r15;
        public const uint c_register_lst_sp  = 1U << (int)c_register_sp ;
        public const uint c_register_lst_lr  = 1U << (int)c_register_lr ;
        public const uint c_register_lst_pc  = 1U << (int)c_register_pc ;

        //--//

        public enum Format
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

        //--//

        static public uint OPCODE_DECODE_MASK( int len )
        {
            return (1U << len) - 1U;
        }

        static public uint OPCODE_DECODE_INSERTFIELD( uint val, int bitPos, int bitLen)
        {
            return (val & OPCODE_DECODE_MASK( bitLen )) << bitPos;
        }

        static public uint OPCODE_DECODE_EXTRACTFIELD( uint op, int bitPos, int bitLen )
        {
            return (op >> bitPos) & OPCODE_DECODE_MASK( bitLen );
        }

        static public uint OPCODE_DECODE_SETFLAG( bool val, int bitPos )
        {
            return val ? (1U << bitPos) : 0U;
        }

        static public bool OPCODE_DECODE_CHECKFLAG( uint op, int bitPos )
        {
            return (op & (1U << bitPos)) != 0;
        }
        
        //--//

        static public uint get_ConditionCodes     ( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op, 28, 4 ); }
        static public bool get_ShouldSetConditions( uint op  ) { return OPCODE_DECODE_CHECKFLAG   ( op, 20    ); }

        //--//

        static public uint get_Register1( uint op ) { return OPCODE_DECODE_EXTRACTFIELD( op, 16, 4 ); }
        static public uint get_Register2( uint op ) { return OPCODE_DECODE_EXTRACTFIELD( op, 12, 4 ); }
        static public uint get_Register3( uint op ) { return OPCODE_DECODE_EXTRACTFIELD( op,  8, 4 ); }
        static public uint get_Register4( uint op ) { return OPCODE_DECODE_EXTRACTFIELD( op,  0, 4 ); }

        //--//

        static public bool get_Multiply_IsAccumulate( uint op ) { return OPCODE_DECODE_CHECKFLAG( op, 21 ); }
        static public bool get_Multiply_IsSigned    ( uint op ) { return OPCODE_DECODE_CHECKFLAG( op, 22 ); }

        //--//

        static public bool get_StatusRegister_IsSPSR( uint op ) { return OPCODE_DECODE_CHECKFLAG   ( op, 22    ); }
        static public uint get_StatusRegister_Fields( uint op ) { return OPCODE_DECODE_EXTRACTFIELD( op, 16, 4 ); }

        //--//

        static public uint get_Shift_Type     ( uint op ) { return OPCODE_DECODE_EXTRACTFIELD( op, 5, 2 ); }
        static public uint get_Shift_Immediate( uint op ) { return OPCODE_DECODE_EXTRACTFIELD( op, 7, 5 ); }
        static public uint get_Shift_Register ( uint op ) { return OPCODE_DECODE_EXTRACTFIELD( op, 8, 4 ); }

        //--//

        static public uint get_DataProcessing_Operation( uint op ) { return OPCODE_DECODE_EXTRACTFIELD( op, 21, 4 ); }

        static public uint get_DataProcessing_ImmediateSeed    ( uint op ) { return OPCODE_DECODE_EXTRACTFIELD( op, 0, 8 ); }
        static public uint get_DataProcessing_ImmediateRotation( uint op ) { return OPCODE_DECODE_EXTRACTFIELD( op, 8, 4 ); }

        static public uint get_DataProcessing_ImmediateValue( uint op )
        {
            return get_DataProcessing_ImmediateValue( get_DataProcessing_ImmediateSeed( op ), get_DataProcessing_ImmediateRotation( op ) );
        }

        static public uint get_DataProcessing_ImmediateValue( uint imm, uint rot )
        {
            rot *= 2;

            return (imm >> (int)rot) | (imm << (int)(32 - rot));
        }

        //--//

        static public bool get_DataTransfer_IsLoad         ( uint op ) { return OPCODE_DECODE_CHECKFLAG   ( op, 20     ); }
        static public bool get_DataTransfer_ShouldWriteBack( uint op ) { return OPCODE_DECODE_CHECKFLAG   ( op, 21     ); }
        static public bool get_DataTransfer_IsByteTransfer ( uint op ) { return OPCODE_DECODE_CHECKFLAG   ( op, 22     ); }
        static public bool get_DataTransfer_IsUp           ( uint op ) { return OPCODE_DECODE_CHECKFLAG   ( op, 23     ); }
        static public bool get_DataTransfer_IsPreIndexing  ( uint op ) { return OPCODE_DECODE_CHECKFLAG   ( op, 24     ); }
        static public uint get_DataTransfer_Offset         ( uint op ) { return OPCODE_DECODE_EXTRACTFIELD( op,  0, 12 ); }

        //--//

        static public uint get_HalfWordDataTransfer_Kind  ( uint op ) { return  OPCODE_DECODE_EXTRACTFIELD( op, 5, 2 )                                               ; }
        static public uint get_HalfWordDataTransfer_Offset( uint op ) { return (OPCODE_DECODE_EXTRACTFIELD( op, 8, 4 ) << 4) | OPCODE_DECODE_EXTRACTFIELD( op, 0, 4 ); }

        //--//

        static public bool get_BlockDataTransfer_LoadPSR     ( uint op ) { return OPCODE_DECODE_CHECKFLAG   ( op, 22     ); }
        static public uint get_BlockDataTransfer_RegisterList( uint op ) { return OPCODE_DECODE_EXTRACTFIELD( op, 0 , 16 ); }

        //--//

        static public bool get_Branch_IsLink( uint op ) { return OPCODE_DECODE_CHECKFLAG( op, 24 ); }
        static public int  get_Branch_Offset( uint op ) { return ((int)(op << 8)) >> 6; }

        //--//

        static public bool get_CoprocRegisterTransfer_IsMRC( uint op ) { return OPCODE_DECODE_CHECKFLAG   ( op, 20    ); }
        static public uint get_CoprocRegisterTransfer_Op1  ( uint op ) { return OPCODE_DECODE_EXTRACTFIELD( op, 21, 3 ); }
        static public uint get_CoprocRegisterTransfer_Op2  ( uint op ) { return OPCODE_DECODE_EXTRACTFIELD( op,  5, 3 ); }
        static public uint get_CoprocRegisterTransfer_CpNum( uint op ) { return OPCODE_DECODE_EXTRACTFIELD( op,  8, 4 ); }
        static public uint get_CoprocRegisterTransfer_CRn  ( uint op ) { return OPCODE_DECODE_EXTRACTFIELD( op, 16, 4 ); }
        static public uint get_CoprocRegisterTransfer_CRm  ( uint op ) { return OPCODE_DECODE_EXTRACTFIELD( op,  0, 4 ); }
        static public uint get_CoprocRegisterTransfer_Rd   ( uint op ) { return OPCODE_DECODE_EXTRACTFIELD( op, 12, 4 ); }

        //--//

        static public uint get_SoftwareInterrupt_Immediate( uint op ) { return OPCODE_DECODE_EXTRACTFIELD( op, 0, 24 ); }

        //--//

        public abstract class Decoder
        {
            public uint m_conditionCodes;
            public uint m_address_PC;
            public uint m_address_Target;
            public bool m_address_TargetIsCode;

            protected Decoder( uint op )
            {
                m_conditionCodes = get_ConditionCodes( op );
            }

            public abstract void Print( System.Text.StringBuilder str );

            //--//

            protected void PrintMnemonic( System.Text.StringBuilder str, string format, params object[] args )
            {
                int start = str.Length;

                str.AppendFormat( format, args );

                int len = str.Length - start;

                if(len < 9)
                {
                    str.Append( new string( ' ', 9 - len ) );
                }
            }

            //--//

            protected string DumpCondition()
            {
                switch(m_conditionCodes)
                {
                    case ArmDisassembler.c_cond_EQ: return "EQ";
                    case ArmDisassembler.c_cond_NE: return "NE";
                    case ArmDisassembler.c_cond_CS: return "CS";
                    case ArmDisassembler.c_cond_CC: return "CC";
                    case ArmDisassembler.c_cond_MI: return "MI";
                    case ArmDisassembler.c_cond_PL: return "PL";
                    case ArmDisassembler.c_cond_VS: return "VS";
                    case ArmDisassembler.c_cond_VC: return "VC";
                    case ArmDisassembler.c_cond_HI: return "HI";
                    case ArmDisassembler.c_cond_LS: return "LS";
                    case ArmDisassembler.c_cond_GE: return "GE";
                    case ArmDisassembler.c_cond_LT: return "LT";
                    case ArmDisassembler.c_cond_GT: return "GT";
                    case ArmDisassembler.c_cond_LE: return "LE";
                    case ArmDisassembler.c_cond_AL: return "";
                }

                return "??";
            }

            //--//

            static protected string DumpOperation( uint alu )
            {
                switch(alu)
                {
                    case ArmDisassembler.c_operation_AND: return "AND";
                    case ArmDisassembler.c_operation_EOR: return "EOR";
                    case ArmDisassembler.c_operation_SUB: return "SUB";
                    case ArmDisassembler.c_operation_RSB: return "RSB";
                    case ArmDisassembler.c_operation_ADD: return "ADD";
                    case ArmDisassembler.c_operation_ADC: return "ADC";
                    case ArmDisassembler.c_operation_SBC: return "SBC";
                    case ArmDisassembler.c_operation_RSC: return "RSC";
                    case ArmDisassembler.c_operation_TST: return "TST";
                    case ArmDisassembler.c_operation_TEQ: return "TEQ";
                    case ArmDisassembler.c_operation_CMP: return "CMP";
                    case ArmDisassembler.c_operation_CMN: return "CMN";
                    case ArmDisassembler.c_operation_ORR: return "ORR";
                    case ArmDisassembler.c_operation_MOV: return "MOV";
                    case ArmDisassembler.c_operation_BIC: return "BIC";
                    case ArmDisassembler.c_operation_MVN: return "MVN";
                }

                return "??";
            }

            static protected string DumpRegister( uint reg )
            {
                switch(reg)
                {
                    case ArmDisassembler.c_register_r0 : return "r0";
                    case ArmDisassembler.c_register_r1 : return "r1";
                    case ArmDisassembler.c_register_r2 : return "r2";
                    case ArmDisassembler.c_register_r3 : return "r3";
                    case ArmDisassembler.c_register_r4 : return "r4";
                    case ArmDisassembler.c_register_r5 : return "r5";
                    case ArmDisassembler.c_register_r6 : return "r6";
                    case ArmDisassembler.c_register_r7 : return "r7";
                    case ArmDisassembler.c_register_r8 : return "r8";
                    case ArmDisassembler.c_register_r9 : return "r9";
                    case ArmDisassembler.c_register_r10: return "r10";
                    case ArmDisassembler.c_register_r11: return "r11";
                    case ArmDisassembler.c_register_r12: return "r12";
                    case ArmDisassembler.c_register_sp : return "sp";
                    case ArmDisassembler.c_register_lr : return "lr";
                    case ArmDisassembler.c_register_pc : return "pc";
                }

                return "??";
            }

            static protected string DumpShiftType( uint stype )
            {
                switch(stype)
                {
                    case ArmDisassembler.c_shift_LSL: return "LSL";
                    case ArmDisassembler.c_shift_LSR: return "LSR";
                    case ArmDisassembler.c_shift_ASR: return "ASR";
                    case ArmDisassembler.c_shift_ROR: return "ROR";
                    case ArmDisassembler.c_shift_RRX: return "RRX";
                }

                return "???";
            }

            static protected string DumpHalfWordKind( uint kind )
            {
                switch(kind)
                {
                    case ArmDisassembler.c_halfwordkind_SWP: return "SWP";
                    case ArmDisassembler.c_halfwordkind_U2 : return "H";
                    case ArmDisassembler.c_halfwordkind_I1 : return "SB";
                    case ArmDisassembler.c_halfwordkind_I2 : return "SH";
                }

                return "??";
            }
        }

        public class Decoder_MRS : Decoder
        {
            public bool m_useSPSR;
            public uint m_Rd;

            public Decoder_MRS( uint op ) : base( op )
            {
                m_useSPSR = get_StatusRegister_IsSPSR( op );
                m_Rd      = get_Register2            ( op );
            }

            public override void Print( System.Text.StringBuilder str )
            {
                PrintMnemonic( str, "MRS{0}", DumpCondition() );

                str.AppendFormat( "{0},{1}", DumpRegister( m_Rd ), m_useSPSR ? "SPSR" : "CPSR" );
            }
        }

        public abstract class Decoder_MSR : Decoder
        {
            public bool m_useSPSR;
            public uint m_fields;

            protected Decoder_MSR( uint op ) : base( op )
            {
                m_useSPSR = get_StatusRegister_IsSPSR( op );
                m_fields  = get_StatusRegister_Fields( op );
            }

            protected void PrintPre( System.Text.StringBuilder str )
            {
                PrintMnemonic( str, "MSR{0}", DumpCondition() );

                str.AppendFormat( "{0}_{1}{2}{3}{4}",  m_useSPSR                                      ? "SPSR" : "CPSR",
                    (m_fields & ArmDisassembler.c_psr_field_c) != 0 ? "c"    : ""    ,
                    (m_fields & ArmDisassembler.c_psr_field_x) != 0 ? "x"    : ""    ,
                    (m_fields & ArmDisassembler.c_psr_field_s) != 0 ? "s"    : ""    ,
                    (m_fields & ArmDisassembler.c_psr_field_f) != 0 ? "f"    : ""    );
            }
        }

        public class Decoder_MSR_1 : Decoder_MSR
        {
            public uint m_Rm;

            public Decoder_MSR_1( uint op ) : base( op )
            {
                m_Rm = get_Register4( op );
            }

            public override void Print( System.Text.StringBuilder str )
            {
                PrintPre( str );

                str.AppendFormat( ",{0}", DumpRegister( m_Rm ) );
            }
        }

        public class Decoder_MSR_2 : Decoder_MSR
        {
            public uint m_imm;
            public uint m_rot;

            public Decoder_MSR_2( uint op ) : base( op )
            {
                m_imm = get_DataProcessing_ImmediateSeed    ( op );
                m_rot = get_DataProcessing_ImmediateRotation( op );
            }

            public override void Print( System.Text.StringBuilder str )
            {
                PrintPre( str );

                str.AppendFormat( ",#0x{0:X}", get_DataProcessing_ImmediateValue( m_imm, m_rot ) );
            }
        }

        //--//

        public abstract class Decoder_DataProcessing : Decoder
        {
            public uint m_Rn;
            public uint m_Rd;
            public uint m_alu;
            public bool m_setCC;

            protected Decoder_DataProcessing( uint op ) : base( op )
            {
                m_Rn    = get_Register1               ( op );
                m_Rd    = get_Register2               ( op );
                m_alu   = get_DataProcessing_Operation( op );
                m_setCC = get_ShouldSetConditions     ( op );
            }

            protected void PrintPre( System.Text.StringBuilder str )
            {
                bool fMov = false;
                bool fTst = false;

                switch(m_alu)
                {
                    case ArmDisassembler.c_operation_MOV:
                    case ArmDisassembler.c_operation_MVN:
                        fMov = true;
                        break;

                    case ArmDisassembler.c_operation_TEQ:
                    case ArmDisassembler.c_operation_TST:
                    case ArmDisassembler.c_operation_CMP:
                    case ArmDisassembler.c_operation_CMN:
                        fTst = true;
                        break;
                }

                PrintMnemonic( str, "{0}{1}{2}", DumpOperation( m_alu ), DumpCondition(), !fTst && m_setCC ? 'S' : ' ' );

                if(fMov)
                {
                    str.AppendFormat( "{0}", DumpRegister( m_Rd ) );
                }
                else if(fTst)
                {
                    str.AppendFormat( "{0}", DumpRegister( m_Rn ) );
                }
                else
                {
                    str.AppendFormat( "{0},{1}", DumpRegister( m_Rd ), DumpRegister( m_Rn ) );
                }
            }
        }

        public class Decoder_DataProcessing_1 : Decoder_DataProcessing
        {
            public uint m_imm;
            public uint m_rot;

            public Decoder_DataProcessing_1( uint op ) : base( op )
            {
                m_imm = get_DataProcessing_ImmediateSeed    ( op );
                m_rot = get_DataProcessing_ImmediateRotation( op );
            }

            public override void Print( System.Text.StringBuilder str )
            {
                uint offset = get_DataProcessing_ImmediateValue( m_imm, m_rot );

                PrintPre( str );

                str.AppendFormat( ",#0x{0:X}", offset );

                if(m_Rn == 15)
                {
                    switch(m_alu)
                    {
                        case ArmDisassembler.c_operation_ADD:
                            m_address_Target = (uint)(m_address_PC + 8 + offset);
                            break;

                        case ArmDisassembler.c_operation_SUB:
                            m_address_Target = (uint)(m_address_PC + 8 - offset);
                            break;
                    }
                }
            }
        }

        public abstract class Decoder_DataProcessing_23 : Decoder_DataProcessing
        {
            public uint m_Rm;
            public uint m_shiftType;

            protected Decoder_DataProcessing_23( uint op ) : base( op )
            {
                m_Rm        = get_Register4 ( op );
                m_shiftType = get_Shift_Type( op );
            }

            protected new void PrintPre( System.Text.StringBuilder str )
            {
                base.PrintPre( str );

                str.AppendFormat( ",{0}", DumpRegister( m_Rm ) );
            }
        }

        public class Decoder_DataProcessing_2 : Decoder_DataProcessing_23
        {
            public uint m_shiftValue;

            public Decoder_DataProcessing_2( uint op ) : base( op )
            {
                m_shiftValue = get_Shift_Immediate( op );

                if(m_shiftValue == 0)
                {
                    switch(m_shiftType)
                    {
                        case ArmDisassembler.c_shift_LSR:
                        case ArmDisassembler.c_shift_ASR:
                            m_shiftValue = 32;
                            break;

                        case ArmDisassembler.c_shift_ROR:
                            m_shiftValue = 1;
                            m_shiftType  = ArmDisassembler.c_shift_RRX;
                            break;
                    }
                }
            }

            public override void Print( System.Text.StringBuilder str )
            {
                PrintPre( str );

                if(m_shiftValue != 0)
                {
                    str.AppendFormat( ",{0} #{1}", DumpShiftType( m_shiftType ), m_shiftValue );
                }
            }
        }

        public class Decoder_DataProcessing_3 : Decoder_DataProcessing_23
        {
            public uint m_Rs;

            public Decoder_DataProcessing_3( uint op ) : base( op )
            {
                m_Rs = get_Register3( op );
            }

            public override void Print( System.Text.StringBuilder str )
            {
                PrintPre( str );

                str.AppendFormat( ",{0} {1}", DumpShiftType( m_shiftType ), DumpRegister( m_Rs ) );
            }
        }

        //--//

        public class Decoder_Multiply : Decoder
        {
            public uint m_Rd;
            public uint m_Rn;
            public uint m_Rs;
            public uint m_Rm;
            public bool m_setCC;
            public bool m_isAccumulate;

            public Decoder_Multiply( uint op ) : base( op )
            {
                m_Rd           = get_Register1            ( op );
                m_Rn           = get_Register2            ( op );
                m_Rs           = get_Register3            ( op );
                m_Rm           = get_Register4            ( op );
                m_setCC        = get_ShouldSetConditions  ( op );
                m_isAccumulate = get_Multiply_IsAccumulate( op );
            }

            public override void Print( System.Text.StringBuilder str )
            {
                PrintMnemonic( str, "{0}{1}{2}", m_isAccumulate ? "MLA" : "MUL", DumpCondition(), m_setCC ? 'S' : ' ' );

                str.AppendFormat( "{0},{1},{2}", DumpRegister( m_Rd ), DumpRegister( m_Rm ), DumpRegister( m_Rs ) );

                if(m_isAccumulate)
                {
                    str.AppendFormat( ",{0}", DumpRegister( m_Rn ) );
                }
            }
        }

        public class Decoder_MultiplyLong : Decoder
        {
            public uint m_RdHi;
            public uint m_RdLo;
            public uint m_Rs;
            public uint m_Rm;
            public bool m_setCC;
            public bool m_isAccumulate;
            public bool m_isSigned;

            public Decoder_MultiplyLong( uint op ) : base( op )
            {
                m_RdHi         = get_Register1            ( op );
                m_RdLo         = get_Register2            ( op );
                m_Rs           = get_Register3            ( op );
                m_Rm           = get_Register4            ( op );
                m_setCC        = get_ShouldSetConditions  ( op );
                m_isAccumulate = get_Multiply_IsAccumulate( op );
                m_isSigned     = get_Multiply_IsSigned    ( op );
            }

            public override void Print( System.Text.StringBuilder str )
            {
                PrintMnemonic( str, "{0}{1}{2}{3}", m_isSigned ? 'S' : 'U', m_isAccumulate ? "MLAL" : "MULL", DumpCondition(), m_setCC ? 'S' : ' ' );

                str.AppendFormat( "{0},{1},{2},{3}", DumpRegister( m_RdLo ), DumpRegister( m_RdHi ), DumpRegister( m_Rm ), DumpRegister( m_Rs ) );
            }
        }

        //--//

        public class Decoder_SingleDataSwap : Decoder
        {
            public uint m_Rn;
            public uint m_Rd;
            public uint m_Rm;
            public bool m_isByte;

            public Decoder_SingleDataSwap( uint op ) : base( op )
            {
                m_Rn     = get_Register1                  ( op );
                m_Rd     = get_Register2                  ( op );
                m_Rm     = get_Register4                  ( op );
                m_isByte = get_DataTransfer_IsByteTransfer( op );
            }

            public override void Print( System.Text.StringBuilder str )
            {
                PrintMnemonic( str, "{0}{1}{2}", "SWP", DumpCondition(), m_isByte ? "B" : "" );

                str.AppendFormat( "{0},{1},[{2}]", DumpRegister( m_Rd ), DumpRegister( m_Rm ), DumpRegister( m_Rn ) );
            }
        }

        //--//

        public class Decoder_Branch : Decoder
        {
            public int  m_offset;
            public bool m_isLink;

            public Decoder_Branch( uint op ) : base( op )
            {
                m_offset = get_Branch_Offset( op );
                m_isLink = get_Branch_IsLink( op );
            }

            public override void Print( System.Text.StringBuilder str )
            {
                PrintMnemonic( str, "B{0}{1}", m_isLink ? "L" : "", DumpCondition() );

                m_address_Target       = (uint)(m_address_PC + 8 + m_offset);
                m_address_TargetIsCode = true;

                str.AppendFormat( "0x{0:X}", m_address_Target );
            }
        }

        public class Decoder_BranchAndExchange : Decoder
        {
            public uint m_Rn;

            public Decoder_BranchAndExchange( uint op ) : base( op )
            {
                m_Rn = get_Register4( op );
            }

            public override void Print( System.Text.StringBuilder str )
            {
                PrintMnemonic( str, "BX{0}", DumpCondition() );

                str.AppendFormat( "{0}", DumpRegister( m_Rn ) );
            }
        }

        //--//

        public abstract class Decoder_DataTransfer : Decoder
        {
            public uint m_Rn;
            public bool m_load;
            public bool m_preIndex;
            public bool m_up;
            public bool m_writeBack;

            protected Decoder_DataTransfer( uint op ) : base( op )
            {
                m_Rn        = get_Register1                   ( op );
                m_load      = get_DataTransfer_IsLoad         ( op );
                m_preIndex  = get_DataTransfer_IsPreIndexing  ( op );
                m_up        = get_DataTransfer_IsUp           ( op );
                m_writeBack = get_DataTransfer_ShouldWriteBack( op );
            }
        }

        public abstract class Decoder_WordDataTransfer : Decoder_DataTransfer
        {
            public uint m_Rd;

            protected Decoder_WordDataTransfer( uint op ) : base( op )
            {
                m_Rd = get_Register2( op );
            }
        }

        public abstract class Decoder_HalfwordDataTransfer : Decoder_WordDataTransfer
        {
            public uint m_kind;

            protected Decoder_HalfwordDataTransfer( uint op ) : base( op )
            {
                m_kind = get_HalfWordDataTransfer_Kind( op );
            }

            protected void PrintPre( System.Text.StringBuilder str )
            {
                PrintMnemonic( str, "{0}{1}{2}", m_load ? "LDR" : "STR", DumpCondition(), DumpHalfWordKind( m_kind ) );

                str.AppendFormat( "{0},[{1}", DumpRegister( m_Rd ), DumpRegister( m_Rn ) );

                if(m_preIndex == false) str.Append( "]" );
            }

            protected void PrintPost( System.Text.StringBuilder str )
            {
                if(m_preIndex) str.Append( "]" );

                if(m_writeBack)
                {
                    str.Append( "!" );
                }
            }
        }

        public class Decoder_HalfwordDataTransfer_1 : Decoder_HalfwordDataTransfer
        {
            public uint m_Rm;

            public Decoder_HalfwordDataTransfer_1( uint op ) : base( op )
            {
                m_Rm = get_Register4( op );
            }

            public override void Print( System.Text.StringBuilder str )
            {
                PrintPre( str );

                str.AppendFormat( ",{0}{1}", m_up ? "" : "-", DumpRegister( m_Rm ) );

                PrintPost( str );
            }
        }

        public class Decoder_HalfwordDataTransfer_2 : Decoder_HalfwordDataTransfer
        {
            public uint m_offset;

            public Decoder_HalfwordDataTransfer_2( uint op ) : base( op )
            {
                m_offset = get_HalfWordDataTransfer_Offset( op );
            }

            public override void Print( System.Text.StringBuilder str )
            {
                PrintPre( str );

                str.AppendFormat( ",#{0}{1}", m_up ? "" : "-", m_offset );

                PrintPost( str );

                if(m_Rn == 15)
                {
                    int address = (int)(m_address_PC + 8);

                    if(m_preIndex)
                    {
                        address += (m_up ? +(int)m_offset : -(int)m_offset);
                    }

                    m_address_Target = (uint)address;
                }
            }
        }

        public abstract class Decoder_SingleDataTransfer : Decoder_WordDataTransfer
        {
            public bool m_isByte;

            protected Decoder_SingleDataTransfer( uint op ) : base( op )
            {
                m_isByte = get_DataTransfer_IsByteTransfer( op );
            }

            protected void PrintPre( System.Text.StringBuilder str )
            {
                PrintMnemonic( str, "{0}{1}{2}", m_load ? "LDR" : "STR", DumpCondition(), m_isByte ? "B" : "" );

                str.AppendFormat( "{0},[{1}", DumpRegister( m_Rd ), DumpRegister( m_Rn ) );

                if(m_preIndex == false) str.Append( "]" );
            }

            protected void PrintPost( System.Text.StringBuilder str )
            {
                if(m_preIndex) str.Append( "]" );

                if(m_writeBack)
                {
                    str.Append( "!" );
                }
            }
        }

        public class Decoder_SingleDataTransfer_1 : Decoder_SingleDataTransfer
        {
            public uint m_offset;

            public Decoder_SingleDataTransfer_1( uint op ) : base( op )
            {
                m_offset = get_DataTransfer_Offset( op );
            }

            public override void Print( System.Text.StringBuilder str )
            {
                PrintPre( str );

                if(m_offset != 0)
                {
                    str.AppendFormat( ",#{0}0x{1:X}", m_up ? "" : "-", m_offset );
                }

                PrintPost( str );

                if(m_Rn == 15)
                {
                    int address = (int)(m_address_PC + 8);

                    if(m_preIndex)
                    {
                        address += (m_up ? +(int)m_offset : -(int)m_offset);
                    }

                    m_address_Target = (uint)address;
                }
            }
        }

        public abstract class Decoder_SingleDataTransfer_23 : Decoder_SingleDataTransfer
        {
            public uint m_Rm;
            public uint m_shiftType;

            protected Decoder_SingleDataTransfer_23( uint op ) : base( op )
            {
                m_Rm        = get_Register4 ( op );
                m_shiftType = get_Shift_Type( op );
            }

            protected new void PrintPre( System.Text.StringBuilder str )
            {
                base.PrintPre( str );

                str.AppendFormat( ",{0}{1}", m_up ? "" : "-", DumpRegister( m_Rm ) );
            }

            protected new void PrintPost( System.Text.StringBuilder str )
            {
                base.PrintPost( str );
            }
        }

        public class Decoder_SingleDataTransfer_2 : Decoder_SingleDataTransfer_23
        {
            public uint m_shiftValue;

            public Decoder_SingleDataTransfer_2( uint op ) : base( op )
            {
                m_shiftValue = get_Shift_Immediate( op );

                if(m_shiftValue == 0)
                {
                    switch(m_shiftType)
                    {
                        case ArmDisassembler.c_shift_LSR:
                        case ArmDisassembler.c_shift_ASR:
                            m_shiftValue = 32;
                            break;

                        case ArmDisassembler.c_shift_ROR:
                            m_shiftValue = 1;
                            m_shiftType  = ArmDisassembler.c_shift_RRX;
                            break;
                    }
                }
            }

            public override void Print( System.Text.StringBuilder str )
            {
                PrintPre( str );

                if(m_shiftValue != 0)
                {
                    str.AppendFormat( ",{0} #{1}", DumpShiftType( m_shiftType ), m_shiftValue );
                }

                PrintPost( str );
            }
        }

        public class Decoder_SingleDataTransfer_3 : Decoder_SingleDataTransfer_23
        {
            public uint m_Rs;

            public Decoder_SingleDataTransfer_3( uint op ) : base( op )
            {
                m_Rs = get_Register3( op );
            }

            public override void Print( System.Text.StringBuilder str )
            {
                PrintPre( str );

                str.AppendFormat( ",{0} {1}", DumpShiftType( m_shiftType ), DumpRegister( m_Rs ) );

                PrintPost( str );
            }
        }

        public class Decoder_BlockDataTransfer : Decoder_DataTransfer
        {
            public uint m_Lst;
            public bool m_loadPSR;

            public Decoder_BlockDataTransfer( uint op ) : base( op )
            {
                m_Lst     = get_BlockDataTransfer_RegisterList( op );
                m_loadPSR = get_BlockDataTransfer_LoadPSR     ( op );
            }

            public override void Print( System.Text.StringBuilder str )
            {
                char c1;
                char c2;

                if(m_Rn == 13)
                {
                    if(m_load)
                    {
                        c1 = m_preIndex ? 'E' : 'F';
                        c2 = m_up       ? 'D' : 'A';
                    }
                    else
                    {
                        c1 = m_preIndex ? 'F' : 'E';
                        c2 = m_up       ? 'A' : 'D';
                    }
                }
                else
                {
                    c1 = m_up       ? 'I' : 'D';
                    c2 = m_preIndex ? 'B' : 'A';
                }

                PrintMnemonic( str, "{0}{1}{2}{3}", m_load ? "LDM" : "STM", DumpCondition(), c1, c2 );

                str.AppendFormat( "{0}{1},", DumpRegister( m_Rn ), m_writeBack ? "!" : "" );

                uint Rd  = 0;
                uint Lst = m_Lst;

                str.Append( "{" );

                while(Lst != 0)
                {
                    while((Lst & 1) == 0)
                    {
                        Lst >>= 1;
                        Rd++;
                    }

                    uint Rfirst = Rd;

                    while((Lst & 1) != 0)
                    {
                        Lst >>= 1;
                        Rd++;
                    }

                    str.AppendFormat( "{0}", DumpRegister( Rfirst ) );

                    if(Rd - Rfirst > 1)
                    {
                        str.AppendFormat( "-{0}", DumpRegister( Rd-1 ) );
                    }

                    if(Lst != 0)
                    {
                        str.Append( "," );
                    }
                }

                str.Append( "}" );

                if(m_loadPSR)
                {
                    str.Append( "^" );
                }
            }
        }

        public class Decoder_CoprocRegisterTransfer : Decoder
        {
            public bool m_IsMRC;
            public uint m_Op1;
            public uint m_Op2;
            public uint m_CpNum;
            public uint m_CRn;  
            public uint m_CRm;  
            public uint m_Rd;

            public Decoder_CoprocRegisterTransfer( uint op ) : base( op )
            {
                m_IsMRC = get_CoprocRegisterTransfer_IsMRC( op );
                m_Op1   = get_CoprocRegisterTransfer_Op1  ( op );
                m_Op2   = get_CoprocRegisterTransfer_Op2  ( op );
                m_CpNum = get_CoprocRegisterTransfer_CpNum( op );
                m_CRn   = get_CoprocRegisterTransfer_CRn  ( op );
                m_CRm   = get_CoprocRegisterTransfer_CRm  ( op );
                m_Rd    = get_CoprocRegisterTransfer_Rd   ( op );
            }

            public override void Print( System.Text.StringBuilder str )
            {
                if(m_IsMRC)
                {
                    PrintMnemonic( str, "MRC{0}", DumpCondition() );
                }
                else
                {
                    PrintMnemonic( str, "MCR{0}", DumpCondition() );
                }

                str.AppendFormat( "p{0},0x{1},r{2},c{3},c{4},0x{5}", m_CpNum, m_Op1, m_Rd, m_CRn, m_CRm, m_Op2 );
            }
        }

        public class Decoder_SoftwareInterrupt : Decoder
        {
            public uint m_value;

            public Decoder_SoftwareInterrupt( uint op ) : base( op )
            {
                m_value = get_SoftwareInterrupt_Immediate( op );
            }

            public override void Print( System.Text.StringBuilder str )
            {
                PrintMnemonic( str, "SWI{0}", DumpCondition() );

                str.AppendFormat( "#0x{0,6,X6}", m_value );
            }
        }

        //--//

        static public Decoder Decode( uint op )
        {
            if((op & opmask_MRS                   ) == op_MRS                   ) return new Decoder_MRS                   ( op );
            if((op & opmask_MSR_1                 ) == op_MSR_1                 ) return new Decoder_MSR_1                 ( op );
            if((op & opmask_MSR_2                 ) == op_MSR_2                 ) return new Decoder_MSR_2                 ( op );
            if((op & opmask_DataProcessing_1      ) == op_DataProcessing_1      ) return new Decoder_DataProcessing_1      ( op );
            if((op & opmask_DataProcessing_2      ) == op_DataProcessing_2      ) return new Decoder_DataProcessing_2      ( op );
            if((op & opmask_DataProcessing_3      ) == op_DataProcessing_3      ) return new Decoder_DataProcessing_3      ( op );
            if((op & opmask_Multiply              ) == op_Multiply              ) return new Decoder_Multiply              ( op );
            if((op & opmask_MultiplyLong          ) == op_MultiplyLong          ) return new Decoder_MultiplyLong          ( op );
            if((op & opmask_SingleDataSwap        ) == op_SingleDataSwap        ) return new Decoder_SingleDataSwap        ( op );
            if((op & opmask_BranchAndExchange     ) == op_BranchAndExchange     ) return new Decoder_BranchAndExchange     ( op );
            if((op & opmask_HalfwordDataTransfer_1) == op_HalfwordDataTransfer_1) return new Decoder_HalfwordDataTransfer_1( op );
            if((op & opmask_HalfwordDataTransfer_2) == op_HalfwordDataTransfer_2) return new Decoder_HalfwordDataTransfer_2( op );
            if((op & opmask_SingleDataTransfer_1  ) == op_SingleDataTransfer_1  ) return new Decoder_SingleDataTransfer_1  ( op );
            if((op & opmask_SingleDataTransfer_2  ) == op_SingleDataTransfer_2  ) return new Decoder_SingleDataTransfer_2  ( op );
            if((op & opmask_SingleDataTransfer_3  ) == op_SingleDataTransfer_3  ) return new Decoder_SingleDataTransfer_3  ( op );
            if((op & opmask_Undefined             ) == op_Undefined             ) return null;
            if((op & opmask_BlockDataTransfer     ) == op_BlockDataTransfer     ) return new Decoder_BlockDataTransfer     ( op );
            if((op & opmask_Branch                ) == op_Branch                ) return new Decoder_Branch                ( op );
            if((op & opmask_CoprocDataTransfer    ) == op_CoprocDataTransfer    ) return null;
            if((op & opmask_CoprocDataOperation   ) == op_CoprocDataOperation   ) return null;
            if((op & opmask_CoprocRegisterTransfer) == op_CoprocRegisterTransfer) return new Decoder_CoprocRegisterTransfer( op );
            if((op & opmask_SoftwareInterrupt     ) == op_SoftwareInterrupt     ) return new Decoder_SoftwareInterrupt     ( op );

            return null;
        }

        static public string DecodeAndPrint( uint address, uint op, ref uint target, ref bool targetIsCode )
        {
            Decoder dec = Decode( op );

            if(dec == null)
            {
                target       = 0;
                targetIsCode = false;
                return "";
            }
            else
            {
                System.Text.StringBuilder str = new System.Text.StringBuilder();

                dec.m_address_PC           = address;
                dec.m_address_Target       = 0xFFFFFFFF;
                dec.m_address_TargetIsCode = false;

                dec.Print( str );

                target       = dec.m_address_Target;
                targetIsCode = dec.m_address_TargetIsCode;

                return str.ToString();
            }
        }
    }
}
