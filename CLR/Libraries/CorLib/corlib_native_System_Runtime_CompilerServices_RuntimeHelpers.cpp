////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CorLib.h"
#include <TinyCLR_Endian.h>

HRESULT Library_corlib_native_System_Runtime_CompilerServices_RuntimeHelpers::InitializeArray___STATIC__VOID__SystemArray__SystemRuntimeFieldHandle( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_FieldDef_Instance inst;
    CLR_RT_HeapBlock_Array*  array = stack.Arg0().DereferenceArray(); FAULT_ON_NULL(array);

    if(array->m_fReference)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    if(CLR_RT_ReflectionDef_Index::Convert( stack.Arg1(), inst ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    if((inst.m_target->flags & CLR_RECORD_FIELDDEF::FD_HasFieldRVA) == 0 || inst.m_target->defaultValue == CLR_EmptyIndex)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    {
        CLR_PMETADATA ptrSrc  = inst.m_assm->GetSignature( inst.m_target->defaultValue );
        CLR_UINT32    lenSrc; TINYCLR_READ_UNALIGNED_UINT16( lenSrc, ptrSrc );

        CLR_UINT8*    ptrDst  = array->GetFirstElement();
        CLR_UINT32    lenDst  = array->m_numOfElements;
        CLR_UINT32    sizeDst = array->m_sizeOfElement;

        lenSrc /= sizeDst; if(lenSrc > lenDst) lenSrc = lenDst;
#if !defined(NETMF_TARGET_BIG_ENDIAN)
        memcpy( ptrDst, ptrSrc, lenSrc * sizeDst );

#if defined(TINYCLR_EMULATED_FLOATINGPOINT)
        switch(array->m_typeOfElement)
        {
        case DATATYPE_R4:
            {
                CLR_RT_HeapBlock tmp;
                CLR_UINT32*      ptr = (CLR_UINT32*)ptrDst;

                for(;lenSrc; lenSrc--, ptr++)
                {
                    TINYCLR_CHECK_HRESULT(tmp.SetFloatIEEE754( *ptr ));

                    *ptr = tmp.NumericByRef().u4;
                }
            }
            break;

        case DATATYPE_R8:
            {
                CLR_RT_HeapBlock tmp;
                CLR_UINT64*      ptr = (CLR_UINT64*)ptrDst;

                for(;lenSrc; lenSrc--, ptr++)
                {
                    TINYCLR_CHECK_HRESULT(tmp.SetDoubleIEEE754( *ptr ));

                    *ptr = tmp.NumericByRef().u8;
                }
            }
            break;
        }
#endif

#else
        // FIXME GJS - WOuld it be possible to move the endian swap to pe compile time to get rid of this?
        // If this is a numeric dataype of datatype size other than a byte then byteswap the entries
        // Unaligned reads handle endianess, just use them. FIXME GJS - this could be the subject of much optimization
        switch(array->m_typeOfElement)
        {
        case DATATYPE_CHAR:
        case DATATYPE_I2:
        case DATATYPE_U2:
            {
                CLR_UINT32  count=0;
                CLR_UINT16  d;
                CLR_UINT16  *p16 = (CLR_UINT16*)ptrDst;
                while( count < lenDst )
                {
                    TINYCLR_READ_UNALIGNED_UINT16( d, ptrSrc );
                    *p16++ = SwapEndian(d);
                    count++;
                }
            }
            break;

        case DATATYPE_I4:
        case DATATYPE_U4:
        case DATATYPE_R4:
             {
                CLR_UINT32  count=0;
                CLR_UINT32  d;
                CLR_UINT32  *p32 = (CLR_UINT32*)ptrDst;
                while( count < lenDst )
                {
                    TINYCLR_READ_UNALIGNED_UINT32( d, ptrSrc );
                    *p32++ = SwapEndian(d);
                    count++;
                }
            }
            break;
        case DATATYPE_I8:
        case DATATYPE_U8:
        case DATATYPE_R8:
            {
                CLR_UINT32  count=0;
                CLR_UINT64  d;
                CLR_UINT64  *p64 = (CLR_UINT64*)ptrDst;

                while( count < lenDst )
                {
                    TINYCLR_READ_UNALIGNED_UINT64( d, ptrSrc );
                    *p64++ = SwapEndian( d );
                    count++;
                }
            }
            break;
        default:
            memcpy( ptrDst, ptrSrc, lenSrc * sizeDst );
            break;
        }



#if defined(TINYCLR_EMULATED_FLOATINGPOINT)
        switch(array->m_typeOfElement)
        {
        case DATATYPE_R4:
            {
                CLR_RT_HeapBlock tmp;
                CLR_UINT32*      ptr = (CLR_UINT32*)ptrDst;

                for(;lenSrc; lenSrc--, ptr++)
                {
                    TINYCLR_CHECK_HRESULT(tmp.SetFloatIEEE754( *ptr ));

                    *ptr = tmp.NumericByRef().u4;
                }
            }
            break;

        case DATATYPE_R8:
            {
                CLR_RT_HeapBlock tmp;
                CLR_UINT64*      ptr = (CLR_UINT64*)ptrDst;

                for(;lenSrc; lenSrc--, ptr++)
                {
                    TINYCLR_CHECK_HRESULT(tmp.SetDoubleIEEE754( *ptr ));

                    *ptr = tmp.NumericByRef().u8;
                }
            }
            break;
        }
#endif        
#endif //NETMF_TARGET_BIG_ENDIAN
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Runtime_CompilerServices_RuntimeHelpers::GetObjectValue___STATIC__OBJECT__OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock& top = stack.PushValueAndClear();
    CLR_RT_HeapBlock* src = stack.Arg0().Dereference();

    if(src && src->DataType() == DATATYPE_VALUETYPE && src->IsBoxed())
    {
        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.CloneObject( top, *src ));
    }
    else
    {
        top.SetObjectReference( src );
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Runtime_CompilerServices_RuntimeHelpers::RunClassConstructor___STATIC__VOID__SystemRuntimeTypeHandle( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(CLR_E_NOT_SUPPORTED);

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Runtime_CompilerServices_RuntimeHelpers::get_OffsetToStringData___STATIC__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(CLR_E_NOT_SUPPORTED);

    TINYCLR_NOCLEANUP();
}
