////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\Core\Core.h"

#include <TinyCLR_Jitter.h>

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_JITTER)

////////////////////////////////////////////////////////////////////////////////////////////////////

void ArmProcessor::Opcode::Encoder_MRS::Decode( CLR_UINT32 op )
{
    m_useSPSR = get_StatusRegister_IsSPSR( op );
    m_Rd      = get_Register2            ( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_MRS::Encode() const
{
    CLR_UINT32 op = ArmProcessor::Opcode::c_op_MRS;

    op |= set_StatusRegister_IsSPSR( m_useSPSR );
    op |= set_Register2            ( m_Rd      );

    return op;
}

void ArmProcessor::Opcode::Encoder_MRS::Print( Opcode& op ) const
{
    op.AppendMnemonic( "MRS%s", DumpCondition( op.m_conditionCodes ) );

    op.AppendText( "%s,%s", DumpRegister( m_Rd ), m_useSPSR ? "SPSR" : "CPSR" );
}

//--//

void ArmProcessor::Opcode::Encoder_MSR::Decode( CLR_UINT32 op )
{
    m_useSPSR = get_StatusRegister_IsSPSR( op );
    m_fields  = get_StatusRegister_Fields( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_MSR::Encode( CLR_UINT32 op ) const
{
    op |= set_StatusRegister_IsSPSR( m_useSPSR );
    op |= set_StatusRegister_Fields( m_fields  );

    return op;
}

void ArmProcessor::Opcode::Encoder_MSR::Print( Opcode& op ) const
{
    op.AppendMnemonic( "MSR%s", DumpCondition( op.m_conditionCodes ) );

    op.AppendText( "%s_%s%s%s%s",  m_useSPSR                               ? "SPSR" : "CPSR",
                                  (m_fields & ArmProcessor::c_psr_field_c) ? "c"    : ""    ,
                                  (m_fields & ArmProcessor::c_psr_field_x) ? "x"    : ""    ,
                                  (m_fields & ArmProcessor::c_psr_field_s) ? "s"    : ""    ,
                                  (m_fields & ArmProcessor::c_psr_field_f) ? "f"    : ""    );
}

//--//

void ArmProcessor::Opcode::Encoder_MSR_1::Decode( CLR_UINT32 op )
{
    m_Rm = get_Register4( op );

    Encoder_MSR::Decode( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_MSR_1::Encode() const
{
    CLR_UINT32 op = ArmProcessor::Opcode::c_op_MSR_1;

    op |= set_Register4( m_Rm );

    return Encoder_MSR::Encode( op );
}

void ArmProcessor::Opcode::Encoder_MSR_1::Print( Opcode& op ) const
{
    Encoder_MSR::Print( op );

    op.AppendText( ",%s", DumpRegister( m_Rm ) );
}

//--//

void ArmProcessor::Opcode::Encoder_MSR_2::Decode( CLR_UINT32 op )
{
    m_imm = get_DataProcessing_ImmediateSeed    ( op );
    m_rot = get_DataProcessing_ImmediateRotation( op );

    Encoder_MSR::Decode( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_MSR_2::Encode() const
{
    CLR_UINT32 op = ArmProcessor::Opcode::c_op_MSR_2;

    op |= set_DataProcessing_ImmediateSeed    ( m_imm );
    op |= set_DataProcessing_ImmediateRotation( m_rot );

    return Encoder_MSR::Encode( op );
}

void ArmProcessor::Opcode::Encoder_MSR_2::Print( Opcode& op ) const
{
    CLR_UINT32 value = get_DataProcessing_ImmediateValue( m_imm, m_rot );

    Encoder_MSR::Print( op );

    op.AppendText( ",#0x%x", value );
}

//--//

void ArmProcessor::Opcode::Encoder_DataProcessing::Decode( CLR_UINT32 op )
{
    m_Rn    = get_Register1               ( op );
    m_Rd    = get_Register2               ( op );
    m_alu   = get_DataProcessing_Operation( op );
    m_setCC = get_ShouldSetConditions     ( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_DataProcessing::Encode( CLR_UINT32 op ) const
{
    op |= set_Register1               ( m_Rn    );
    op |= set_Register2               ( m_Rd    );
    op |= set_DataProcessing_Operation( m_alu   );
    op |= set_ShouldSetConditions     ( m_setCC );

    return op;
}

void ArmProcessor::Opcode::Encoder_DataProcessing::Print( Opcode& op ) const
{
    bool fMov = false;
    bool fTst = false;

    switch(m_alu)
    {
    case ArmProcessor::c_operation_MOV:
    case ArmProcessor::c_operation_MVN:
        fMov = true;
        break;

    case ArmProcessor::c_operation_TEQ:
    case ArmProcessor::c_operation_TST:
    case ArmProcessor::c_operation_CMP:
    case ArmProcessor::c_operation_CMN:
        fTst = true;
        break;
    }

    op.AppendMnemonic( "%s%s%c", DumpOperation( m_alu ), DumpCondition( op.m_conditionCodes ), !fTst && m_setCC ? 'S' : ' ' );

    if(fMov)
    {
        op.AppendText( "%s", DumpRegister( m_Rd ) );
    }
    else if(fTst)
    {
        op.AppendText( "%s", DumpRegister( m_Rn ) );
    }
    else
    {
        op.AppendText( "%s,%s", DumpRegister( m_Rd ), DumpRegister( m_Rn ) );
    }
}

//--//

void ArmProcessor::Opcode::Encoder_DataProcessing_1::Decode( CLR_UINT32 op )
{
    m_imm = get_DataProcessing_ImmediateSeed    ( op );
    m_rot = get_DataProcessing_ImmediateRotation( op );

    Encoder_DataProcessing::Decode( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_DataProcessing_1::Encode() const
{
    CLR_UINT32 op = ArmProcessor::Opcode::c_op_DataProcessing_1;

    op |= set_DataProcessing_ImmediateSeed    ( m_imm );
    op |= set_DataProcessing_ImmediateRotation( m_rot );

    return Encoder_DataProcessing::Encode( op );
}

void ArmProcessor::Opcode::Encoder_DataProcessing_1::Print( Opcode& op ) const
{
    CLR_UINT32 value = get_DataProcessing_ImmediateValue( m_imm, m_rot );

    Encoder_DataProcessing::Print( op );

    op.AppendText( ",#0x%x", value );

    if(m_Rn == 15)
    {
        switch(m_alu)
        {
        case ArmProcessor::c_operation_ADD:
            op.AppendText( " ; 0x%x", op.m_dump_address + 8 + value );
            break;

        case ArmProcessor::c_operation_SUB:
            op.AppendText( " ; 0x%x", op.m_dump_address + 8 - value );
            break;
        }
    }
}

//--/

void ArmProcessor::Opcode::Encoder_DataProcessing_23::Decode( CLR_UINT32 op )
{
    m_Rm        = get_Register4 ( op );
    m_shiftType = get_Shift_Type( op );

    Encoder_DataProcessing::Decode( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_DataProcessing_23::Encode( CLR_UINT32 op, CLR_UINT32 shiftType ) const
{
    op |= set_Register4 ( m_Rm      );
    op |= set_Shift_Type( shiftType );

    return Encoder_DataProcessing::Encode( op );
}

void ArmProcessor::Opcode::Encoder_DataProcessing_23::Print( Opcode& op ) const
{
    Encoder_DataProcessing::Print( op );

    op.AppendText( ",%s", DumpRegister( m_Rm ) );
}

//--//

void ArmProcessor::Opcode::Encoder_DataProcessing_2::Decode( CLR_UINT32 op )
{
    m_shiftValue = get_Shift_Immediate( op );

    Encoder_DataProcessing_23::Decode( op );

    ARM_FIX_SHIFT_IN(m_shiftValue,m_shiftType);
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_DataProcessing_2::Encode() const
{
    CLR_UINT32 op         = ArmProcessor::Opcode::c_op_DataProcessing_2;
    CLR_UINT32 shiftValue = m_shiftValue;
    CLR_UINT32 shiftType  = m_shiftType;

    ARM_FIX_SHIFT_OUT(shiftValue,shiftType);

    op |= set_Shift_Immediate( shiftValue );

    return Encoder_DataProcessing_23::Encode( op, shiftType );
}

void ArmProcessor::Opcode::Encoder_DataProcessing_2::Print( Opcode& op ) const
{
    Encoder_DataProcessing_23::Print( op );

    if(m_shiftValue)
    {
        op.AppendText( ",%s #%d", DumpShiftType( m_shiftType ), m_shiftValue );
    }
}

//--//

void ArmProcessor::Opcode::Encoder_DataProcessing_3::Decode( CLR_UINT32 op )
{
    m_Rs = get_Register3( op );

    Encoder_DataProcessing_23::Decode( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_DataProcessing_3::Encode() const
{
    CLR_UINT32 op = ArmProcessor::Opcode::c_op_DataProcessing_3;

    op |= set_Register3( m_Rs );

    return Encoder_DataProcessing_23::Encode( op, m_shiftType );
}

void ArmProcessor::Opcode::Encoder_DataProcessing_3::Print( Opcode& op ) const
{
    Encoder_DataProcessing_23::Print( op );

    op.AppendText( ",%s %s", DumpShiftType( m_shiftType ), DumpRegister( m_Rs ) );
}

//--//

void ArmProcessor::Opcode::Encoder_Multiply::Decode( CLR_UINT32 op )
{
    m_Rd           = get_Register1            ( op );
    m_Rn           = get_Register2            ( op );
    m_Rs           = get_Register3            ( op );
    m_Rm           = get_Register4            ( op );
    m_setCC        = get_ShouldSetConditions  ( op );
    m_isAccumulate = get_Multiply_IsAccumulate( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_Multiply::Encode() const
{
    CLR_UINT32 op = ArmProcessor::Opcode::c_op_Multiply;

    op |= set_Register1            ( m_Rd           );
    op |= set_Register2            ( m_Rn           );
    op |= set_Register3            ( m_Rs           );
    op |= set_Register4            ( m_Rm           );
    op |= set_ShouldSetConditions  ( m_setCC        );
    op |= set_Multiply_IsAccumulate( m_isAccumulate );

    return op;
}

void ArmProcessor::Opcode::Encoder_Multiply::Print( Opcode& op ) const
{
    op.AppendMnemonic( "%s%s%c", m_isAccumulate ? "MLA" : "MUL", DumpCondition( op.m_conditionCodes ), m_setCC ? 'S' : ' ' );

    op.AppendText( "%s,%s,%s", DumpRegister( m_Rd ), DumpRegister( m_Rm ), DumpRegister( m_Rs ) );

    if(m_isAccumulate)
    {
        op.AppendText( ",%s", DumpRegister( m_Rn ) );
    }
}

//--//

void ArmProcessor::Opcode::Encoder_MultiplyLong::Decode( CLR_UINT32 op )
{
    m_RdHi         = get_Register1            ( op );
    m_RdLo         = get_Register2            ( op );
    m_Rs           = get_Register3            ( op );
    m_Rm           = get_Register4            ( op );
    m_setCC        = get_ShouldSetConditions  ( op );
    m_isAccumulate = get_Multiply_IsAccumulate( op );
    m_isSigned     = get_Multiply_IsSigned    ( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_MultiplyLong::Encode() const
{
    CLR_UINT32 op = ArmProcessor::Opcode::c_op_MultiplyLong;

    op |= set_Register1            ( m_RdHi         );
    op |= set_Register2            ( m_RdLo         );
    op |= set_Register3            ( m_Rs           );
    op |= set_Register4            ( m_Rm           );
    op |= set_ShouldSetConditions  ( m_setCC        );
    op |= set_Multiply_IsAccumulate( m_isAccumulate );
    op |= set_Multiply_IsSigned    ( m_isSigned     );

    return op;
}

void ArmProcessor::Opcode::Encoder_MultiplyLong::Print( Opcode& op ) const
{
    op.AppendMnemonic( "%c%s%s%c", m_isSigned ? 'S' : 'U', m_isAccumulate ? "MLAL" : "MULL", DumpCondition( op.m_conditionCodes ), m_setCC ? 'S' : ' ' );

    op.AppendText( "%s,%s,%s,%s", DumpRegister( m_RdLo ), DumpRegister( m_RdHi ), DumpRegister( m_Rm ), DumpRegister( m_Rs ) );
}

//--//

void ArmProcessor::Opcode::Encoder_SingleDataSwap::Decode( CLR_UINT32 op )
{
    m_Rn     = get_Register1                  ( op );
    m_Rd     = get_Register2                  ( op );
    m_Rm     = get_Register4                  ( op );
    m_isByte = get_DataTransfer_IsByteTransfer( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_SingleDataSwap::Encode() const
{
    CLR_UINT32 op = ArmProcessor::Opcode::c_op_SingleDataSwap;

    op |= set_Register1                  ( m_Rn     );
    op |= set_Register2                  ( m_Rd     );
    op |= set_Register4                  ( m_Rm     );
    op |= set_DataTransfer_IsByteTransfer( m_isByte );

    return op;
}

void ArmProcessor::Opcode::Encoder_SingleDataSwap::Print( Opcode& op ) const
{
    op.AppendMnemonic( "%s%s%s", "SWP", DumpCondition( op.m_conditionCodes ), m_isByte ? "B" : "" );

    op.AppendText( "%s,%s,[%s]", DumpRegister( m_Rd ), DumpRegister( m_Rm ), DumpRegister( m_Rn ) );
}

//--//

void ArmProcessor::Opcode::Encoder_Branch::Decode( CLR_UINT32 op )
{
    m_offset = get_Branch_Offset( op );
    m_isLink = get_Branch_IsLink( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_Branch::Encode() const
{
    CLR_UINT32 op = ArmProcessor::Opcode::c_op_Branch;

    op |= set_Branch_Offset( m_offset );
    op |= set_Branch_IsLink( m_isLink );

    return op;
}

void ArmProcessor::Opcode::Encoder_Branch::Print( Opcode& op ) const
{
    op.AppendMnemonic( "B%s%s", m_isLink ? "L" : "", DumpCondition( op.m_conditionCodes ) );

    op.AppendText( "0x%x", op.m_dump_address + 8 + m_offset );
}

//--//

void ArmProcessor::Opcode::Encoder_BranchAndExchange::Decode( CLR_UINT32 op )
{
    m_Rn = get_Register4( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_BranchAndExchange::Encode() const
{
    CLR_UINT32 op = ArmProcessor::Opcode::c_op_BranchAndExchange;

    op |= set_Register4( m_Rn );

    return op;
}

void ArmProcessor::Opcode::Encoder_BranchAndExchange::Print( Opcode& op ) const
{
    op.AppendMnemonic( "%s%s", "BX", DumpCondition( op.m_conditionCodes ) );

    op.AppendText( "%s", DumpRegister( m_Rn ) );
}

//--//

void ArmProcessor::Opcode::Encoder_DataTransfer::Decode( CLR_UINT32 op )
{
    m_Rn        = get_Register1                   ( op );
    m_load      = get_DataTransfer_IsLoad         ( op );
    m_preIndex  = get_DataTransfer_IsPreIndexing  ( op );
    m_up        = get_DataTransfer_IsUp           ( op );
    m_writeBack = get_DataTransfer_ShouldWriteBack( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_DataTransfer::Encode( CLR_UINT32 op ) const
{
    op |= set_Register1                   ( m_Rn        );
    op |= set_DataTransfer_IsLoad         ( m_load      );
    op |= set_DataTransfer_IsPreIndexing  ( m_preIndex  );
    op |= set_DataTransfer_IsUp           ( m_up        );
    op |= set_DataTransfer_ShouldWriteBack( m_writeBack );

    return op;
}

//--//

void ArmProcessor::Opcode::Encoder_WordDataTransfer::Decode( CLR_UINT32 op )
{
    m_Rd = get_Register2( op );

    Encoder_DataTransfer::Decode( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_WordDataTransfer::Encode( CLR_UINT32 op ) const
{
    op |= set_Register2( m_Rd );

    return Encoder_DataTransfer::Encode( op );
}

//--//

void ArmProcessor::Opcode::Encoder_HalfwordDataTransfer::Decode( CLR_UINT32 op )
{
    m_kind = get_HalfWordDataTransfer_Kind( op );

    Encoder_WordDataTransfer::Decode( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_HalfwordDataTransfer::Encode( CLR_UINT32 op ) const
{
    op |= set_HalfWordDataTransfer_Kind( m_kind );

    return Encoder_WordDataTransfer::Encode( op );
}

void ArmProcessor::Opcode::Encoder_HalfwordDataTransfer::PrintPre( Opcode& op ) const
{
    op.AppendMnemonic( "%s%s%s", m_load ? "LDR" : "STR", DumpCondition( op.m_conditionCodes ), DumpHalfWordKind( m_kind ) );

    op.AppendText( "%s,[%s", DumpRegister( m_Rd ), DumpRegister( m_Rn ) );

    if(m_preIndex == 0) op.AppendText( "]" );
}

void ArmProcessor::Opcode::Encoder_HalfwordDataTransfer::PrintPost( Opcode& op ) const
{
    if(m_preIndex) op.AppendText( "]" );

    if(m_writeBack)
    {
        op.AppendText( "!" );
    }
}

//--//

void ArmProcessor::Opcode::Encoder_HalfwordDataTransfer_1::Decode( CLR_UINT32 op )
{
    m_Rm = get_Register4( op );

    Encoder_HalfwordDataTransfer::Decode( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_HalfwordDataTransfer_1::Encode() const
{
    CLR_UINT32 op = ArmProcessor::Opcode::c_op_HalfwordDataTransfer_1;

    op |= set_Register4( m_Rm );

    return Encoder_HalfwordDataTransfer::Encode( op );
}

void ArmProcessor::Opcode::Encoder_HalfwordDataTransfer_1::Print( Opcode& op ) const
{
    Encoder_HalfwordDataTransfer::PrintPre( op );

    op.AppendText( ",%s%s", m_up ? "" : "-", DumpRegister( m_Rm ) );

    Encoder_HalfwordDataTransfer::PrintPost( op );
}

//--//

void ArmProcessor::Opcode::Encoder_HalfwordDataTransfer_2::Decode( CLR_UINT32 op )
{
    m_offset = get_HalfWordDataTransfer_Offset( op );

    Encoder_HalfwordDataTransfer::Decode( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_HalfwordDataTransfer_2::Encode() const
{
    CLR_UINT32 op = ArmProcessor::Opcode::c_op_HalfwordDataTransfer_2;

    op |= set_HalfWordDataTransfer_Offset( m_offset );

    return Encoder_HalfwordDataTransfer::Encode( op );
}

void ArmProcessor::Opcode::Encoder_HalfwordDataTransfer_2::Print( Opcode& op ) const
{
    Encoder_HalfwordDataTransfer::PrintPre( op );

    op.AppendText( ",#%s%d", m_up ? "" : "-", m_offset );

    Encoder_HalfwordDataTransfer::PrintPost( op );

    if(m_Rn == 15)
    {
        CLR_UINT32 address = op.m_dump_address + 8;

        if(m_preIndex)
        {
            address += (m_up ? +(CLR_INT32)m_offset : -(CLR_INT32)m_offset);
        }

        op.AppendText( " ; 0x%x", address );
    }
}

//--//

void ArmProcessor::Opcode::Encoder_SingleDataTransfer::Decode( CLR_UINT32 op )
{
    m_isByte = get_DataTransfer_IsByteTransfer( op );

    Encoder_WordDataTransfer::Decode( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_SingleDataTransfer::Encode( CLR_UINT32 op ) const
{
    op |= set_DataTransfer_IsByteTransfer( m_isByte );

    return Encoder_WordDataTransfer::Encode( op );
}

void ArmProcessor::Opcode::Encoder_SingleDataTransfer::PrintPre( Opcode& op ) const
{
    op.AppendMnemonic( "%s%s%s", m_load ? "LDR" : "STR", DumpCondition( op.m_conditionCodes ), m_isByte ? "B" : "" );

    op.AppendText( "%s,[%s", DumpRegister( m_Rd ), DumpRegister( m_Rn ) );

    if(m_preIndex == 0) op.AppendText( "]" );
}

void ArmProcessor::Opcode::Encoder_SingleDataTransfer::PrintPost( Opcode& op ) const
{
    if(m_preIndex) op.AppendText( "]" );

    if(m_writeBack)
    {
        op.AppendText( "!" );
    }
}

//--//

void ArmProcessor::Opcode::Encoder_SingleDataTransfer_1::Decode( CLR_UINT32 op )
{
    m_offset = get_DataTransfer_Offset( op );

    Encoder_SingleDataTransfer::Decode( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_SingleDataTransfer_1::Encode() const
{
    CLR_UINT32 op = ArmProcessor::Opcode::c_op_SingleDataTransfer_1;

    op |= set_DataTransfer_Offset( m_offset );

    return Encoder_SingleDataTransfer::Encode( op );
}

void ArmProcessor::Opcode::Encoder_SingleDataTransfer_1::Print( Opcode& op ) const
{
    Encoder_SingleDataTransfer::PrintPre( op );

    if(m_offset)
    {
        op.AppendText( ",#%s0x%x", m_up ? "" : "-", m_offset );
    }

    Encoder_SingleDataTransfer::PrintPost( op );

    if(m_Rn == 15)
    {
        CLR_UINT32 address = op.m_dump_address + 8;

        if(m_preIndex)
        {
            address += (m_up ? +(CLR_INT32)m_offset : -(CLR_INT32)m_offset);
        }

        op.AppendText( " ; 0x%x", address );
    }
}

//--//

void ArmProcessor::Opcode::Encoder_SingleDataTransfer_23::Decode( CLR_UINT32 op )
{
    m_Rm        = get_Register4 ( op );
    m_shiftType = get_Shift_Type( op );

    Encoder_SingleDataTransfer::Decode( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_SingleDataTransfer_23::Encode( CLR_UINT32 op, CLR_UINT32 shiftType ) const
{
    op |= set_Register4 ( m_Rm      );
    op |= set_Shift_Type( shiftType );

    return Encoder_SingleDataTransfer::Encode( op );
}

void ArmProcessor::Opcode::Encoder_SingleDataTransfer_23::PrintPre( Opcode& op ) const
{
    Encoder_SingleDataTransfer::PrintPre( op );

    op.AppendText( ",%s%s", m_up ? "" : "-", DumpRegister( m_Rm ) );
}

void ArmProcessor::Opcode::Encoder_SingleDataTransfer_23::PrintPost( Opcode& op ) const
{
    Encoder_SingleDataTransfer::PrintPost( op );
}

//--//

void ArmProcessor::Opcode::Encoder_SingleDataTransfer_2::Decode( CLR_UINT32 op )
{
    m_shiftValue = get_Shift_Immediate( op );

    Encoder_SingleDataTransfer_23::Decode( op );

    ARM_FIX_SHIFT_IN(m_shiftValue,m_shiftType);
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_SingleDataTransfer_2::Encode() const
{
    CLR_UINT32 op         = ArmProcessor::Opcode::c_op_SingleDataTransfer_2;
    CLR_UINT32 shiftValue = m_shiftValue;
    CLR_UINT32 shiftType  = m_shiftType;

    ARM_FIX_SHIFT_OUT(shiftValue,shiftType);

    op |= set_Shift_Immediate( shiftValue );

    return Encoder_SingleDataTransfer_23::Encode( op, shiftType );
}

void ArmProcessor::Opcode::Encoder_SingleDataTransfer_2::Print( Opcode& op ) const
{
    Encoder_SingleDataTransfer_23::PrintPre( op );

    if(m_shiftValue)
    {
        op.AppendText( ",%s #%d", DumpShiftType( m_shiftType ), m_shiftValue );
    }

    Encoder_SingleDataTransfer_23::PrintPost( op );
}

//--//

void ArmProcessor::Opcode::Encoder_SingleDataTransfer_3::Decode( CLR_UINT32 op )
{
    m_Rs = get_Register3( op );

    Encoder_SingleDataTransfer_23::Decode( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_SingleDataTransfer_3::Encode() const
{
    CLR_UINT32 op = ArmProcessor::Opcode::c_op_SingleDataTransfer_3;

    op |= set_Register3( m_Rs );

    return Encoder_SingleDataTransfer_23::Encode( op, m_shiftType );
}

void ArmProcessor::Opcode::Encoder_SingleDataTransfer_3::Print( Opcode& op ) const
{
    Encoder_SingleDataTransfer_23::PrintPre( op );

    op.AppendText( ",%s %s", DumpShiftType( m_shiftType ), DumpRegister( m_Rs ) );

    Encoder_SingleDataTransfer_23::PrintPost( op );
}

//--//

void ArmProcessor::Opcode::Encoder_BlockDataTransfer::Decode( CLR_UINT32 op )
{
    m_Lst     = get_BlockDataTransfer_RegisterList( op );
    m_loadPSR = get_BlockDataTransfer_LoadPSR     ( op );

    Encoder_DataTransfer::Decode( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_BlockDataTransfer::Encode() const
{
    CLR_UINT32 op = ArmProcessor::Opcode::c_op_BlockDataTransfer;

    op |= set_BlockDataTransfer_RegisterList( m_Lst     );
    op |= set_BlockDataTransfer_LoadPSR     ( m_loadPSR );

    return Encoder_DataTransfer::Encode( op );
}

void ArmProcessor::Opcode::Encoder_BlockDataTransfer::Print( Opcode& op ) const
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

    op.AppendMnemonic( "%s%s%c%c", m_load ? "LDM" : "STM", DumpCondition( op.m_conditionCodes ), c1, c2 );

    op.AppendText( "%s%s,{", DumpRegister( m_Rn ), m_writeBack ? "!" : "" );

    CLR_UINT32 Rd  = 0;
    CLR_UINT32 Lst = m_Lst;

    while(Lst)
    {
        while((Lst & 1) == 0)
        {
            Lst >>= 1;
            Rd++;
        }

        CLR_UINT32 Rfirst = Rd;

        while(Lst & 1)
        {
            Lst >>= 1;
            Rd++;
        }

        op.AppendText( "%s", DumpRegister( Rfirst ) );

        if(Rd - Rfirst > 1)
        {
            op.AppendText( "-%s", DumpRegister( Rd-1 ) );
        }

        if(Lst)
        {
            op.AppendText( "," );
        }
    }

    op.AppendText( "}" );

    if(m_loadPSR)
    {
        op.AppendText( "^" );
    }
}

//--//

void ArmProcessor::Opcode::Encoder_SoftwareInterrupt::Decode( CLR_UINT32 op )
{
    m_value = get_SoftwareInterrupt_Immediate( op );
}

CLR_UINT32 ArmProcessor::Opcode::Encoder_SoftwareInterrupt::Encode() const
{
    CLR_UINT32 op = ArmProcessor::Opcode::c_op_SoftwareInterrupt;

    op |= set_SoftwareInterrupt_Immediate( m_value );

    return op;
}

void ArmProcessor::Opcode::Encoder_SoftwareInterrupt::Print( Opcode& op ) const
{
    op.AppendMnemonic( "SWI" );

    op.AppendText( "#0x%x", m_value );
}

////////////////////////////////////////////////////////////////////////////////////////////////////

#endif
