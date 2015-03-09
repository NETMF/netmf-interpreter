////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

#include <TinyCLR_Jitter.h>

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_JITTER_ARMEMULATION)

//--//

ArmEmulator::ArmEmulator( CLR_RT_SymbolToAddressMap& symdef, CLR_RT_AddressToSymbolMap& symdef_Inverse, ArmProcessor::State::LoadFtn fpnLoad, ArmProcessor::State::StoreFtn fpnStore ) : m_symdef(symdef), m_symdef_Inverse(symdef_Inverse)
{
    if(!fpnLoad ) fpnLoad  = (ArmProcessor::State::LoadFtn )&ArmEmulator::LoadImpl;
    if(!fpnStore) fpnStore = (ArmProcessor::State::StoreFtn)&ArmEmulator::StoreImpl;

    Initialize( fpnLoad, fpnStore );

    m_fMonitorMemory    = 0 ? true : false;
    m_fMonitorRegisters = 0 ? true : false;
    m_fMonitorOpcodes   = 0 ? true : false;
    m_fMonitorCalls     = 0 ? true : false;

    m_fStopExecution    = false;
}

void ArmEmulator::ApplyInterop( TrackInterop& ti )
{
    CLR_UINT32 mem;

    if(MemoryLoad( ti.m_pc, mem, DATATYPE_U4 ) && mem != c_InteropOpcode)
    {
        ti.m_op = mem;

        MemoryStore( ti.m_pc, c_InteropOpcode, DATATYPE_U4 );
    }
}

void ArmEmulator::RemoveInterop( CLR_UINT32 pc )
{
    AddressToHandlerMapIter it = m_callInterop.find( pc );

    if(it != m_callInterop.end())
    {
        TrackInterop ti = it->second;

        m_callInterop.erase( it );

        if(ti.m_op != c_InteropOpcode)
        {
            MemoryStore( ti.m_pc, ti.m_op, DATATYPE_U4 );
        }
    }
}

void ArmEmulator::SetInterop( CLR_UINT32 pc, ProcessCall ftn )
{
    AddressToHandlerMapIter it = m_callInterop.find( pc );

    if(it != m_callInterop.end())
    {
        RemoveInterop( it->second.m_pc );
    }

    //--//

    TrackInterop& ti = m_callInterop[ pc ];

    ti.m_pc   = pc;
    ti.m_op   = c_InteropOpcode;
    ti.m_call = ftn;

    ApplyInterop( ti );
}

void ArmEmulator::SetInterop( LPCWSTR name, ProcessCall ftn )
{
    CLR_RT_SymbolToAddressMapIter it = m_symdef.find( name );

    if(it != m_symdef.end())
    {
        SetInterop( it->second, ftn );
    }
}

bool ArmEmulator::GetContext( CLR_UINT32 address, CLR_UINT32& context )
{
    CLR_RT_AddressToSymbolMapIter it = m_symdef_Inverse.upper_bound( address );

    if(it == m_symdef_Inverse.end()) return false;

    if(it->first > address)
    {
        if(it == m_symdef_Inverse.begin()) return false;

        it--;
    }

    context = it->first;

    return true;
}

LPCWSTR ArmEmulator::GetContext()
{
    CLR_UINT32 context;

    if(GetContext( m_pc, context ))
    {
        return m_symdef_Inverse[ context ].c_str();
    }
    else
    {
        return L"??";
    }
}

//--//

HRESULT ArmEmulator::Execute( CLR_UINT64 steps )
{
    TINYCLR_HEADER();

    //
    // Prepare Interop detours.
    //
    {
        for(AddressToHandlerMapIter it = m_callInterop.begin(); it != m_callInterop.end(); it++)
        {
            ApplyInterop( it->second );
        }
    }

    m_fStopExecution = false;

    for(CLR_UINT64 i=0; i<steps; i++)
    {
        CLR_UINT32 pc = m_pc;
        CLR_UINT32 instruction;
        bool       fRes;

        //
        // Special case: if we pass by the reset vector, reload the interop breakpoints.
        //
        if(pc == 0)
        {
            for(AddressToHandlerMapIter it = m_callInterop.begin(); it != m_callInterop.end(); it++)
            {
                ApplyInterop( it->second );
            }
        }

        {
            if(MemoryLoad( pc, instruction, DATATYPE_U4 ) == false)
            {
                CLR_Debug::Printf( "FETCH ABORT at 0x%08x %I64d\r\n", pc, i );

                TINYCLR_SET_AND_LEAVE(CLR_E_FAIL)
            }

       }

#if defined(ARMEMULATOR_MONITOR_CALLS)
        if(m_fMonitorCalls)
        {
            CLR_RT_AddressToSymbolMapIter it;
            size_t                        pos = m_callQueue.size();
            CLR_UINT32                    sp  = m_registers[ ArmProcessor::c_register_sp ];

            for(size_t pos2=pos; pos2-- > 0; )
            {
                TrackCall& tc = m_callQueue[ pos2 ];

                if((tc.m_lr == pc && tc.m_sp == sp) || (tc.m_sp && tc.m_sp < sp))
                {
                    while(pos-- > pos2)
                    {
                        TrackCall& tc2 = m_callQueue[ pos ];

                        tc2.m_te.End( this );

                        CLR_Debug::Printf( "%*s<<<< CALL %S (%s)\r\n", pos, "", tc2.m_name->c_str(), tc2.m_te.ToString() );

                        m_callQueue.pop_back();
                    }
                    pos++;
                }
                else
                {
                    break;
                }
            }

            it = m_symdef_Inverse.find( pc );
            if(it != m_symdef_Inverse.end())
            {
                while(true)
                {
                    if(it->second == L"NoClearZI_ER_RAM") break;
                    if(it->second == L"ClearZI_ER_RAM"  ) break;
                    if(it->second == L"ARM_Vectors"     ) break;

                    m_callQueue.resize( pos + 1 );

                    TrackCall& tc = m_callQueue[ pos ];

                    tc.m_pc    = pc;
                    tc.m_lr    = m_registers[ ArmProcessor::c_register_lr ];
                    tc.m_sp    = sp;
                    tc.m_name  = &it->second;

                    tc.m_te.Start( this );

                    CLR_Debug::Printf( "%*s>>>> CALL %S\r\n", pos, "", tc.m_name->c_str() );
                    break;
                }
            }
        }
#endif

        //--//--//--//--//--//--//--//--//--//--//--//

        if(instruction == c_InteropOpcode)
        {
            //
            // Copy, not reference, the entry can be removed from the map by the interop code.
            //
            TrackInterop ti = m_callInterop[ pc ];

            if(m_fStopExecution)
            {
                break;
            }

            if(ti.m_call)
            {
                if((this->*(ti.m_call))()) continue;

                instruction = ti.m_op;
            }

            if(m_fStopExecution)
            {
                break;
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//

#if defined(ARMEMULATOR_MONITOR_OPCODES)
        if(m_fMonitorOpcodes)
        {
            ArmProcessor::Opcode op;
            char                 rgBuf[ 128 ];

            op.InitText( rgBuf, MAXSTRLEN(rgBuf), pc );

            if(op.Decode( instruction ) == false)
            {
                CLR_Debug::Printf( "DECODE ABORT at 0x%08x %I64d\r\n", pc, i );

                TINYCLR_SET_AND_LEAVE(CLR_E_FAIL)
            }

            fRes = op.CheckConditions( *this );

            op.AppendText( "%-9I64d %-9I64d 0x%08x:  %08x  %c%c%c%c %c ", i, m_clockTicks, pc, instruction, Negative() ? 'N' : '-', Zero() ? 'Z' : '-', Carry() ? 'C' : '-', Overflow() ? 'V' : '-', fRes ? ' ' : '*' );

            op.Print();

            CLR_Debug::Printf( "%s\r\n", rgBuf );
        }
#endif

        {
            if(CheckConditions( ArmProcessor::Opcode::get_ConditionCodes( instruction ) ) == false)
            {
                m_pc += 4;
            }
            else
            {
#if defined(ARMEMULATOR_MONITOR_REGISTERS)
                if(m_fMonitorRegisters)
                {
                    ArmProcessor::State old = *this;

                    fRes = ArmProcessor::Opcode::Execute( instruction, *this );

                    for(CLR_UINT32 reg=0; reg<15; reg++)
                    {
                        if(old.m_registers[ reg ] != m_registers[ reg ])
                        {
                            CLR_Debug::Printf( " %*s %-4s : 0x%08x -> 0x%08x\r\n", 80, "", ArmProcessor::Opcode::DumpRegister( reg ), old.m_registers[ reg ], m_registers[ reg ] );
                        }
                    }
                }
                else
#endif
                {
                    fRes = ArmProcessor::Opcode::Execute( instruction, *this );
                }

                if(fRes == false)
                {
                    CLR_Debug::Printf( "EXECUTION ABORT at 0x%08x %I64d\r\n", pc, i );

                    TINYCLR_SET_AND_LEAVE(CLR_E_FAIL)
                }
            }
        }

    }

    TINYCLR_NOCLEANUP();
}

//--//

#if defined(ARMEMULATOR_MONITOR_MEMORY)
#define VERIFY_ALIGNMENT(address,mod) { if(address % mod) ::DebugBreak(); }
#else
#define VERIFY_ALIGNMENT(address,mod)
#endif

bool ArmEmulator::LoadImpl( CLR_UINT32 address, CLR_UINT32& value, CLR_DataType kind )
{
    try
    {
        void* target = (void*)(size_t)address;

        switch(kind)
        {
            case DATATYPE_U1:                              value = (CLR_UINT32)*(CLR_UINT8 *)target; return true;
            case DATATYPE_U2: VERIFY_ALIGNMENT(address,2); value = (CLR_UINT32)*(CLR_UINT16*)target; return true;
            case DATATYPE_U4: VERIFY_ALIGNMENT(address,4); value = (CLR_UINT32)*(CLR_UINT32*)target; return true;
            case DATATYPE_I1:                              value = (CLR_INT32 )*(CLR_INT8  *)target; return true;
            case DATATYPE_I2: VERIFY_ALIGNMENT(address,2); value = (CLR_INT32 )*(CLR_INT16 *)target; return true;
        }
    }
    catch(...)
    {
    }

    value = 0xBAADF00D;

    return false;
}

bool ArmEmulator::StoreImpl( CLR_UINT32 address, CLR_UINT32 value, CLR_DataType kind )
{
    try
    {
        void* target = (void*)(size_t)address;

#if defined(ARMEMULATOR_MONITOR_MEMORY)
        if(m_fMonitorMemory)
        {
            CLR_UINT32 oldValue;

            MemoryLoad( address, oldValue, kind );

            if(oldValue != value)
            {
                CLR_Debug::Printf( " %*s MEM  : 0x%08x -> 0x%08x [%08x]\r\n", 80, "", oldValue, value, address );
            }
        }
#endif

        switch(kind)
        {
            case DATATYPE_U1:                              *(CLR_UINT8 *)target = (CLR_UINT8 )value; return true;
            case DATATYPE_U2: VERIFY_ALIGNMENT(address,2); *(CLR_UINT16*)target = (CLR_UINT16)value; return true;
            case DATATYPE_U4: VERIFY_ALIGNMENT(address,4); *(CLR_UINT32*)target = (CLR_UINT32)value; return true;
            case DATATYPE_I1:                              *(CLR_INT8  *)target = (CLR_INT8  )value; return true;
            case DATATYPE_I2: VERIFY_ALIGNMENT(address,2); *(CLR_INT16 *)target = (CLR_INT16 )value; return true;
        }
    }
    catch(...)
    {
    }

    return false;
}

bool ArmEmulator::Interop_GenericSkipCall()
{
    //
    // Force return from function.
    //
    m_pc = m_registers[ ArmProcessor::c_register_lr ];
    return true;
}

////////////////////////////////////////////////////////////////////////////////////////////////////

#endif
