////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_RT_HeapBlock_Array::CreateInstance( CLR_RT_HeapBlock& reference, CLR_UINT32 length, const CLR_RT_ReflectionDef_Index& reflex )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* pArray;
    CLR_RT_TypeDef_Index    cls;
    CLR_RT_TypeDef_Instance inst;

    reference.SetObjectReference( NULL );

    if((CLR_INT32)length < 0) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    if(reflex.m_kind != REFLECTION_TYPE)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    if(reflex.m_levels == 1)
    {
        cls = reflex.m_data.m_type;
    }
    else
    {
        cls = g_CLR_RT_WellKnownTypes.m_Array;
    }

    if(inst.InitializeFromIndex( cls ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }
    else
    {
        CLR_DataType                 dt  = (CLR_DataType)inst.m_target->dataType;
        const CLR_RT_DataTypeLookup& dtl = c_CLR_RT_DataTypeLookup[ dt ];

        if(dtl.m_sizeInBytes == CLR_RT_DataTypeLookup::c_NA)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
        }

        pArray    = (CLR_RT_HeapBlock_Array*)g_CLR_RT_ExecutionEngine.ExtractHeapBlocksForArray( inst, length, reflex ); CHECK_ALLOCATION(pArray);

        reference.SetObjectReference( pArray );

        TINYCLR_SET_AND_LEAVE(pArray->ClearElements( 0, length ));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_Array::CreateInstance( CLR_RT_HeapBlock& reference, CLR_UINT32 length, const CLR_RT_TypeDef_Index& cls )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_ReflectionDef_Index reflex;

    reflex.m_kind        = REFLECTION_TYPE;
    reflex.m_levels      = 1;
    reflex.m_data.m_type = cls;

    TINYCLR_SET_AND_LEAVE(CreateInstance( reference, length, reflex ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_Array::CreateInstance( CLR_RT_HeapBlock& reference, CLR_UINT32 length, CLR_RT_Assembly* assm, CLR_UINT32 tk )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock         ref;
    CLR_RT_TypeDef_Instance  cls;
    CLR_RT_TypeSpec_Instance def;

    if(cls.ResolveToken( tk, assm ))
    {
        ref.SetReflection( cls );
    }
    else if(def.ResolveToken( tk, assm ))
    {
        TINYCLR_CHECK_HRESULT(ref.SetReflection( def ));
    }
    else
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    ref.ReflectionData().m_levels++;

    TINYCLR_SET_AND_LEAVE(CreateInstance( reference, length, ref.ReflectionData() ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_Array::ClearElements( int index, int length )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    const CLR_RT_ReflectionDef_Index& reflex = ReflectionDataConst();
    CLR_UINT8*                        data   = GetElement( index );

    CLR_RT_Memory::ZeroFill( data, length * m_sizeOfElement );

    if(m_fReference)
    {
        CLR_DataType      dt        = (CLR_DataType)m_typeOfElement;
        bool              fAllocate = (reflex.m_levels == 1 && dt == DATATYPE_VALUETYPE);
        CLR_RT_HeapBlock* ptr       = (CLR_RT_HeapBlock*)data;

        switch(dt)
        {
        case DATATYPE_VALUETYPE:
        case DATATYPE_CLASS:
        case DATATYPE_WEAKCLASS:
            dt = DATATYPE_OBJECT;
            break;
        }

        while(length > 0)
        {
            ptr->SetDataId( CLR_RT_HEAPBLOCK_RAW_ID(dt,0,1) );

            if(fAllocate)
            {
                TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( *ptr, reflex.m_data.m_type ));
            }

            ptr++; length--;
        }
    }

    TINYCLR_NOCLEANUP();
}

void CLR_RT_HeapBlock_Array::Relocate()
{
    NATIVE_PROFILE_CLR_CORE();
    //
    // If the array is full of reference types, relocate each of them.
    //
    if(m_fReference)
    {
        CLR_RT_GarbageCollector::Heap_Relocate( (CLR_RT_HeapBlock*)GetFirstElement(), m_numOfElements );
    }
}

bool CLR_RT_HeapBlock_Array::CheckRange( int index, int length, int numOfElements )
{
    NATIVE_PROFILE_CLR_CORE();
    if(         length  < 0                                    ) return false;
    if( index           < 0 ||  index           > numOfElements) return false;
    if((index + length) < 0 || (index + length) > numOfElements) return false;

    return true;
}

/*
    This is not the same functionality as System.Array.IndexOf.  CLR_RT_HeapBlock_Array::IndexOf does the search analogous
    to calling Object.ReferenceEquals, not Object.Equals, as System.Array.IndexOf demands.  This function is used by
    TrySzIndexOf
*/
HRESULT CLR_RT_HeapBlock_Array::IndexOf( CLR_RT_HeapBlock_Array* array, CLR_RT_HeapBlock& match, int start, int stop, bool fForward, int& index )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    int numElem = array->m_numOfElements;
    int count;

    if(stop == -1) stop = numElem;

    count = stop - start;

    if(CheckRange( start, count, numElem ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    if(count > 0)
    {
        CLR_UINT8* data     = array->GetFirstElement();
        CLR_UINT8  sizeElem = array->m_sizeOfElement;
        int        pos;
        int        incr;

        if(fForward) { pos = start ; incr =  1; }
        else         { pos = stop-1; incr = -1; }

        data += pos * sizeElem;

        if(!array->m_fReference)
        {
            CLR_RT_HeapBlock* matchPtr = match.FixBoxingReference(); FAULT_ON_NULL(matchPtr);

            if(matchPtr->DataType() <= DATATYPE_LAST_PRIMITIVE)
            {
                int incrElem = incr * sizeElem;

                while(true)
                {
#if !defined(BIG_ENDIAN)
                    if(memcmp( data, &matchPtr->NumericByRef(), sizeElem ) == 0)
                    {
                        index = pos;
                        TINYCLR_SET_AND_LEAVE(S_OK);
                    }
#else
                    CLR_UINT64 refNum;

                    switch(sizeElem)
                    {
                    case 1:
                        refNum = matchPtr->NumericByRef().u1;
                        break;
                    case 2:
                        refNum = matchPtr->NumericByRef().u2;
                        break;
                    case 4:
                        refNum = matchPtr->NumericByRef().u4;
                        break;
                    case 8:
                        refNum = matchPtr->NumericByRef().u8;
                        break;
                    }
                    if(memcmp( data, &refNum, sizeElem ) == 0)
                    {
                        index = pos;
                        TINYCLR_SET_AND_LEAVE(S_OK);
                    }
#endif


                    if(--count == 0) break;

                    pos  += incr;
                    data += incrElem;
                }
            }
        }
        else
        {
            CLR_RT_HeapBlock* dataPtr = (CLR_RT_HeapBlock*)data;

            while(true)
            {
                if(CLR_RT_HeapBlock::Compare_Unsigned_Values( *dataPtr, match ) == 0)
                {
                    index = pos;
                    TINYCLR_SET_AND_LEAVE(S_OK);
                }

                if(--count == 0) break;

                pos     += incr;
                dataPtr += incr;
            }
        }
    }

    index = -1;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_Array::Copy( CLR_RT_HeapBlock_Array* arraySrc, int indexSrc, CLR_RT_HeapBlock_Array* arrayDst, int indexDst, int length )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(length)
    {
        int numElemSrc = arraySrc->m_numOfElements;
        int numElemDst = arrayDst->m_numOfElements;

        if(length   < 0                   ||
           indexSrc < 0                   ||
           indexDst < 0                   ||
           length + indexSrc > numElemSrc ||
           length + indexDst > numElemDst  )
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
        }

        //
        // Copy of an array on itself.
        //
        if(arraySrc == arrayDst && indexSrc == indexDst) TINYCLR_SET_AND_LEAVE(S_OK);

        if(arraySrc->SameHeader( *arrayDst ))
        {
            CLR_UINT8* dataSrc  = arraySrc->GetFirstElement();
            CLR_UINT8* dataDst  = arrayDst->GetFirstElement();
            CLR_UINT8  sizeElem = arraySrc->m_sizeOfElement;

            dataSrc += indexSrc * sizeElem;
            dataDst += indexDst * sizeElem;

            if(!arraySrc->m_fReference)
            {
                memmove( dataDst, dataSrc, length * sizeElem );
            }
            else
            {
                CLR_RT_HeapBlock* ptrSrc = (CLR_RT_HeapBlock*)dataSrc;
                CLR_RT_HeapBlock* ptrDst = (CLR_RT_HeapBlock*)dataDst;
                int               incr;

                if(arraySrc == arrayDst && ptrSrc < ptrDst)
                {
                    incr    =       -1;
                    ptrSrc += length-1;
                    ptrDst += length-1;
                }
                else
                {
                    incr = 1;
                }

                for(int i=0; i<length; i++, ptrSrc += incr, ptrDst += incr)
                {
                    TINYCLR_CHECK_HRESULT(ptrDst->Reassign( *ptrSrc ));
                }
            }
        }
        else if(arraySrc->m_fReference && arrayDst->m_fReference)
        {
            CLR_RT_TypeDescriptor descSrc;
            CLR_RT_TypeDescriptor descDst;
            CLR_RT_HeapBlock*     ptrSrc   = (CLR_RT_HeapBlock*)arraySrc->GetElement( indexSrc );
            CLR_RT_HeapBlock*     ptrDst   = (CLR_RT_HeapBlock*)arrayDst->GetElement( indexDst );
            
            TINYCLR_CHECK_HRESULT(descDst.InitializeFromObject( *arrayDst )); descDst.GetElementType( descDst );

            for(int i=0; i<length; i++, ptrSrc++, ptrDst++)
            {
                if(ptrSrc->DataType() == DATATYPE_OBJECT && ptrSrc->Dereference() == NULL)
                {
                    ;
                }
                else
                {
                    TINYCLR_CHECK_HRESULT(descSrc.InitializeFromObject( *ptrSrc ));

                    if(CLR_RT_ExecutionEngine::IsInstanceOf( descSrc, descDst ) == false)
                    {
                        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_CAST);
                    }
                }

                TINYCLR_CHECK_HRESULT(ptrDst->Reassign( *ptrSrc ));
            }
        }
        else
        {
            CLR_RT_TypeDescriptor descSrc;
            CLR_RT_TypeDescriptor descDst;
            CLR_RT_HeapBlock      ref;
            CLR_RT_HeapBlock      elem; elem.SetObjectReference( NULL );
            CLR_RT_ProtectFromGC  gc( elem ); 

            TINYCLR_CHECK_HRESULT(descDst.InitializeFromObject( *arrayDst )); descDst.GetElementType( descDst );

            for(int i=0; i<length; i++)
            {
                ref.InitializeArrayReferenceDirect( *arraySrc, indexSrc++ ); TINYCLR_CHECK_HRESULT(elem.LoadFromReference( ref ));

                if(elem.DataType() == DATATYPE_OBJECT && elem.Dereference() == NULL)
                {
                    ;
                }
                else
                {
                    TINYCLR_CHECK_HRESULT(descSrc.InitializeFromObject( elem ));

                    if(CLR_RT_ExecutionEngine::IsInstanceOf( descSrc, descDst ) == false)
                    {
                        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_CAST);
                    }
                }

                ref.InitializeArrayReferenceDirect( *arrayDst, indexDst++ ); TINYCLR_CHECK_HRESULT(elem.StoreToReference( ref, 0 ));
            }
        }
    }

    TINYCLR_NOCLEANUP();
}
