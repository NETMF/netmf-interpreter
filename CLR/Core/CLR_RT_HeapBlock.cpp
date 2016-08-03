////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_RT_HeapBlock::InitializeToZero()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_Memory::ZeroFill( &m_data, this->DataSize() * sizeof(*this) - offsetof(CLR_RT_HeapBlock,m_data) );
}

//--//--//--//--//--//

#if defined(TINYCLR_EMULATED_FLOATINGPOINT)

HRESULT CLR_RT_HeapBlock::SetFloatIEEE754( const CLR_UINT32 arg )
{

    CLR_INT32  res;
    CLR_UINT32 mantissa =      ( arg        & 0x007FFFFF) | 0x00800000;
    int        exponent = (int)((arg >> 23) & 0x000000FF) - 127;

    exponent -= (23 - CLR_RT_HeapBlock::HB_FloatShift);

    if(arg == 0)
    {
        res = 0;
    }
    else if(exponent <= -31)
    {
        res = 0;
        
        //
        // Uncomment to produce an overflow exception for emulated floating points
        //
        // return CLR_E_OUT_OF_RANGE;
    }
    else if(exponent >= 31)
    {
        res = 0x7FFFFFFF;

        //
        // Uncomment to produce an overflow exception for emulated floating points
        //
        // return CLR_E_OUT_OF_RANGE;
    }
    else
    {
        if(exponent > 0)
        {
            CLR_UINT64 tmpRes;

            tmpRes = ((CLR_UINT64)mantissa) << exponent;
            
            if(0 != (tmpRes >> 31))
            {
                res = 0x7FFFFFFF;
                
                //
                // Uncomment to produce an overflow exception for emulated floating points
                //
                // return CLR_E_OUT_OF_RANGE;
            }
            else
            {
                res = (CLR_UINT32)tmpRes;
            }
        }
        else if(exponent < 0) res = mantissa >> (-exponent);
        else                  res = mantissa;
    }

    if(arg & 0x80000000) res = -res;

    SetFloat( res );

    return S_OK;
}

HRESULT CLR_RT_HeapBlock::SetDoubleIEEE754( const CLR_UINT64& arg )
{

    CLR_INT64  res;
    CLR_UINT64 mantissa =      ( arg        & ULONGLONGCONSTANT(0x000FFFFFFFFFFFFF)) | ULONGLONGCONSTANT(0x0010000000000000);
    int        exponent = (int)((arg >> 52) & ULONGLONGCONSTANT(0x00000000000007FF)) - 1023;

    CLR_UINT64 mask = ULONGLONGCONSTANT(0xFFFFFFFFFFFFFFFF);

    exponent -= (52 - CLR_RT_HeapBlock::HB_DoubleShift);

    if(arg == 0)
    {
        res = 0;
    }
    else if(exponent <= -63)
    {
        res = 0;
        
        //
        // Uncomment to produce an overflow exception for emulated floating points
        //
        // return CLR_E_OUT_OF_RANGE;
    }
    else if(exponent >= 63)
    {
        res = ULONGLONGCONSTANT(0x7FFFFFFFFFFFFFFF);

        //
        // Uncomment to produce an overflow exception for emulated floating points
        //
        // return CLR_E_OUT_OF_RANGE;
    }
    else
    {
        if(exponent > 0)
        {
            mask <<= (63 - exponent);

            if(0 != (mask & mantissa))
            {
                res = ULONGLONGCONSTANT(0x7FFFFFFFFFFFFFFF);
                
                //
                // Uncomment to produce an overflow exception for emulated floating points
                //
                // return CLR_E_OUT_OF_RANGE;
            }
            else
            {
                res = mantissa << exponent;
            }
        }
        else if(exponent < 0) res = mantissa >> (-exponent);
        else                  res = mantissa;
    }

    if(arg & ULONGLONGCONSTANT(0x8000000000000000)) res = -res;

    SetDouble( res );

    return S_OK;
}

#endif



HRESULT CLR_RT_HeapBlock::EnsureObjectReference( CLR_RT_HeapBlock*& obj )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    switch(this->DataType())
    {
    case DATATYPE_OBJECT:
    case DATATYPE_BYREF:
        {
            obj = Dereference(); FAULT_ON_NULL(obj);

#if defined(TINYCLR_APPDOMAINS)
            if(obj->DataType() == DATATYPE_TRANSPARENT_PROXY)
            {
                TINYCLR_CHECK_HRESULT(obj->TransparentProxyValidate());
                obj = obj->TransparentProxyDereference(); FAULT_ON_NULL(obj);
            }
#endif
            switch(obj->DataType())
            {
            case DATATYPE_CLASS    :
            case DATATYPE_VALUETYPE:

            case DATATYPE_DATETIME: // Special case.
            case DATATYPE_TIMESPAN: // Special case.

                TINYCLR_SET_AND_LEAVE(S_OK);
            }
        }
        break;

    case DATATYPE_DATETIME: // Special case.
    case DATATYPE_TIMESPAN: // Special case.
        obj = this;

        TINYCLR_SET_AND_LEAVE(S_OK);
    }

    TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT CLR_RT_HeapBlock::SetReflection( const CLR_RT_ReflectionDef_Index& reflex )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    m_id.raw          = CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_REFLECTION,0,1);
    m_data.reflection = reflex;

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT CLR_RT_HeapBlock::SetReflection( const CLR_RT_Assembly_Index& assm )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    m_id.raw                        = CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_REFLECTION,0,1);
    m_data.reflection.m_kind        = REFLECTION_ASSEMBLY;
    m_data.reflection.m_levels      = 0;
    m_data.reflection.m_data.m_assm = assm;

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT CLR_RT_HeapBlock::SetReflection( const CLR_RT_TypeSpec_Index& sig )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_TypeDescriptor desc;

    TINYCLR_CHECK_HRESULT(desc.InitializeFromTypeSpec( sig ));

    m_id.raw          = CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_REFLECTION,0,1);
    m_data.reflection = desc.m_reflex;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock::SetReflection( const CLR_RT_TypeDef_Index& cls )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    m_id.raw                        = CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_REFLECTION,0,1);
    m_data.reflection.m_kind        = REFLECTION_TYPE;
    m_data.reflection.m_levels      = 0;
    m_data.reflection.m_data.m_type = cls;

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT CLR_RT_HeapBlock::SetReflection( const CLR_RT_FieldDef_Index& fd )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    m_id.raw                         = CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_REFLECTION,0,1);
    m_data.reflection.m_kind         = REFLECTION_FIELD;
    m_data.reflection.m_levels       = 0;
    m_data.reflection.m_data.m_field = fd;

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT CLR_RT_HeapBlock::SetReflection( const CLR_RT_MethodDef_Index& md )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_MethodDef_Instance inst;

    if(inst.InitializeFromIndex( md ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    m_id.raw                          = CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_REFLECTION,0,1);
    m_data.reflection.m_kind          = (inst.m_target->flags & CLR_RECORD_METHODDEF::MD_Constructor) ? REFLECTION_CONSTRUCTOR : REFLECTION_METHOD;
    m_data.reflection.m_levels        = 0;
    m_data.reflection.m_data.m_method = md;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock::SetObjectCls( const CLR_RT_TypeDef_Index& cls )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_TypeDef_Instance inst;

    if(inst.InitializeFromIndex( cls ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    m_data.objectHeader.cls  = cls;
    m_data.objectHeader.lock = NULL;

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT CLR_RT_HeapBlock::InitializeArrayReference( CLR_RT_HeapBlock& ref, int index )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* array;

    if(ref.DataType() != DATATYPE_OBJECT)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    array = ref.DereferenceArray(); FAULT_ON_NULL(array);

    if(array->DataType() != DATATYPE_SZARRAY)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    if(index < 0 || index >= (CLR_INT32)array->m_numOfElements)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INDEX_OUT_OF_RANGE);
    }

    InitializeArrayReferenceDirect( *array, index );

    TINYCLR_NOCLEANUP();
}

void CLR_RT_HeapBlock::InitializeArrayReferenceDirect( CLR_RT_HeapBlock_Array& array, int index )
{
    NATIVE_PROFILE_CLR_CORE();
    m_id.raw                    = CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_ARRAY_BYREF,0,1);
    m_data.arrayReference.array = &array;
    m_data.arrayReference.index = index;
}

void CLR_RT_HeapBlock::FixArrayReferenceForValueTypes()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock_Array* array = m_data.arrayReference.array;

    //
    // ValueTypes are implemented as pointers to objects,
    // so getting an array reference to a ValueType has to be treated like assigning a pointer!
    //
    // DateTime and TimeSpan are optimized as primitive types,
    // so getting an array reference to them is like getting a reference to them.
    //
    switch(array->m_typeOfElement)
    {
    case DATATYPE_VALUETYPE:
        this->SetReference( *(CLR_RT_HeapBlock*)array->GetElement( m_data.arrayReference.index ) );
        break;

    case DATATYPE_DATETIME:
    case DATATYPE_TIMESPAN:
        m_id.raw                   =                    CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_BYREF,0,1);
        m_data.objectReference.ptr = (CLR_RT_HeapBlock*)array->GetElement( m_data.arrayReference.index );
        break;
    }
}

HRESULT CLR_RT_HeapBlock::LoadFromReference( CLR_RT_HeapBlock& ref )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock  tmp;
    CLR_RT_HeapBlock* obj;
    CLR_DataType      dt = ref.DataType();

    if(dt == DATATYPE_ARRAY_BYREF)
    {
        CLR_RT_HeapBlock_Array* array = ref.m_data.arrayReference.array; FAULT_ON_NULL(array);
        CLR_UINT8*              src   = array->GetElement( ref.m_data.arrayReference.index );
        CLR_UINT32              size  = array->m_sizeOfElement;

        if(!array->m_fReference)
        {
            CLR_UINT32 second = 0;
            CLR_UINT32 first;

            SetDataId( CLR_RT_HEAPBLOCK_RAW_ID(array->m_typeOfElement,0,1) );

            if     (size == 4) { first = ((CLR_UINT32*)src)[ 0 ];                                   }
            else if(size == 8) { first = ((CLR_UINT32*)src)[ 0 ]; second = ((CLR_UINT32*)src)[ 1 ]; }
            else if(size == 1) { first = ((CLR_UINT8 *)src)[ 0 ];                                   }
            else               { first = ((CLR_UINT16*)src)[ 0 ];                                   }

#if !defined(NETMF_TARGET_BIG_ENDIAN)
            ((CLR_UINT32*)&NumericByRef())[ 0 ] = first;
            ((CLR_UINT32*)&NumericByRef())[ 1 ] = second;
#else
            if(size==8)
            {
                ((CLR_UINT32*)&NumericByRef())[ 0 ] = first;
                ((CLR_UINT32*)&NumericByRef())[ 1 ] = second;
            }
            else
            {
                ((CLR_UINT32*)&NumericByRef())[ 0 ] = second;
                ((CLR_UINT32*)&NumericByRef())[ 1 ] = first;
            }
#endif //NETMF_TARGET_BIG_ENDIAN

            TINYCLR_SET_AND_LEAVE(S_OK);
        }

        //
        // It's a pointer to a full CLR_RT_HeapBlock.
        //
        obj = (CLR_RT_HeapBlock*)src;
    }
    else if(dt == DATATYPE_BYREF)
    {
        obj = ref.Dereference(); FAULT_ON_NULL(obj);

        if(obj->DataType() == DATATYPE_VALUETYPE)
        {
            tmp.SetObjectReference( obj );

            obj = &tmp;
        }
    }
    else if(c_CLR_RT_DataTypeLookup[ dt ].m_flags & CLR_RT_DataTypeLookup::c_Direct)
    {
        obj = &ref;

        if(dt == DATATYPE_OBJECT)
        {
            CLR_RT_HeapBlock* objT = ref.Dereference();

            if(objT && objT->IsBoxed())
            {
                CLR_RT_TypeDef_Instance inst;
                if (objT->DataType() != DATATYPE_VALUETYPE) { TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE); }

                if(!inst.InitializeFromIndex( objT->ObjectCls() )) { TINYCLR_SET_AND_LEAVE(CLR_E_TYPE_UNAVAILABLE); }

                if(inst.m_target->dataType != DATATYPE_VALUETYPE) // It's a boxed primitive/enum type.
                {
                    obj = &objT[ 1 ];
                }
            }
        }
    }
    else
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    if(obj->IsAValueType())
    {
        TINYCLR_SET_AND_LEAVE(g_CLR_RT_ExecutionEngine.CloneObject( *this, *obj ));
    }

    this->Assign( *obj );

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock::StoreToReference( CLR_RT_HeapBlock& ref, int size )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* obj;
    CLR_DataType      dt = ref.DataType();

    if(dt == DATATYPE_ARRAY_BYREF)
    {
        CLR_RT_HeapBlock_Array* array = ref.m_data.arrayReference.array; FAULT_ON_NULL(array);
        CLR_UINT8*              dst   = array->GetElement( ref.m_data.arrayReference.index );

        if(!array->m_fReference)
        {
            CLR_UINT32 sizeArray = array->m_sizeOfElement;

            //
            // Cannot copy NULL reference to a primitive type array.
            //
            obj = FixBoxingReference(); FAULT_ON_NULL(obj);
                        
            if(size == -1)
            {
                //size == -1 tells StoreToReference to allow the value 'this' to have more precision than the dest
                //array.  This fixes the following bug.
                //  :  conv.u1
                //  :  stobj      [mscorlib]System.Byte
                // The conv.u1 will convert the top of the eval stack to a u1.  But since the eval stack is required
                // to contain at least 4 byte values, this heap block will be promoted to an I4.
                // stobj ignores the type token (System.Byte) and calls Reassign, which calls StoreToReference.

                if(c_CLR_RT_DataTypeLookup[ this->DataType() ].m_sizeInBytes < sizeArray)
                {
                    //Not enough precision here.
                    TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                }
                   
#if defined(_DEBUG)
                {                    
                    CLR_DataType dtElem  = (CLR_DataType)array->m_typeOfElement;
                    CLR_RT_HeapBlock blk;   blk.Assign( *this );

                    TINYCLR_CHECK_HRESULT(blk.Convert( dtElem, false, (c_CLR_RT_DataTypeLookup[ dtElem ].m_flags & CLR_RT_DataTypeLookup::c_Signed) == 0));

                    switch(sizeArray)
                    {
                    case 1: _ASSERTE(blk.NumericByRefConst().u1 == this->NumericByRefConst().u1); break;
                    case 2: _ASSERTE(blk.NumericByRefConst().u2 == this->NumericByRefConst().u2); break;
                    case 4: _ASSERTE(blk.NumericByRefConst().u4 == this->NumericByRefConst().u4); break;
                    case 8: _ASSERTE(blk.NumericByRefConst().u8 == this->NumericByRefConst().u8); break;
                    }
                }
#endif
            }
            else if(size == 0)
            {                
                if(obj->DataType() != array->m_typeOfElement)
                {
                    TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                }
            }
            else
            {
                if(size != sizeArray)
                {
                    TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                }
            }

#if !defined(NETMF_TARGET_BIG_ENDIAN)
            CLR_UINT32 first  = ((CLR_UINT32*)&obj->NumericByRef())[ 0 ];
            CLR_UINT32 second = ((CLR_UINT32*)&obj->NumericByRef())[ 1 ];
#else
            CLR_UINT32 first;
            CLR_UINT32 second;

            if(size==8)
            {
                first   = ((CLR_UINT32*)&obj->NumericByRef())[ 0 ];
                second  = ((CLR_UINT32*)&obj->NumericByRef())[ 1 ];
            }
            else
            {
                first   = ((CLR_UINT32*)&obj->NumericByRef())[ 1 ];
                second  = ((CLR_UINT32*)&obj->NumericByRef())[ 0 ];
            }
#endif //NETMF_TARGET_BIG_ENDIAN

            if     (sizeArray == 4) { ((CLR_UINT32*)dst)[ 0 ] = (CLR_UINT32)first;                                               }
            else if(sizeArray == 8) { ((CLR_UINT32*)dst)[ 0 ] = (CLR_UINT32)first; ((CLR_UINT32*)dst)[ 1 ] = (CLR_UINT32)second; }
            else if(sizeArray == 1) { ((CLR_UINT8 *)dst)[ 0 ] = (CLR_UINT8 )first;                                               }
            else                    { ((CLR_UINT16*)dst)[ 0 ] = (CLR_UINT16)first;                                               }

            TINYCLR_SET_AND_LEAVE(S_OK);
        }
        else
        {
            //
            // If the source is not null, make sure the types are compatible.
            //
            if(this->DataType() == DATATYPE_OBJECT && this->Dereference())
            {
                CLR_RT_TypeDescriptor descSrc;
                CLR_RT_TypeDescriptor descDst;
                CLR_RT_TypeDescriptor descDstSub;

                TINYCLR_CHECK_HRESULT(descSrc.InitializeFromObject( *this  ));
                TINYCLR_CHECK_HRESULT(descDst.InitializeFromObject( *array )); descDst.GetElementType( descDstSub );

                if(CLR_RT_ExecutionEngine::IsInstanceOf( descSrc, descDstSub ) == false)
                {
                    TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                }
            }

            obj = (CLR_RT_HeapBlock*)dst;

            TINYCLR_SET_AND_LEAVE(obj->Reassign( *this ));            
        }
    }
    else if(dt == DATATYPE_BYREF)
    {
        obj = ref.Dereference(); FAULT_ON_NULL(obj);

        if(obj->DataType() == DATATYPE_VALUETYPE)
        {
            TINYCLR_SET_AND_LEAVE(ref.Reassign( *this ));
        }
    }
    else if(c_CLR_RT_DataTypeLookup[ dt ].m_flags & CLR_RT_DataTypeLookup::c_Direct)
    {
        obj = &ref;
    }
    else
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    obj->Assign( *this );

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock::Reassign( const CLR_RT_HeapBlock& value )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* obj;
    CLR_RT_HeapBlock  ref;

    if(this->DataType() == DATATYPE_BYREF)
    {
        obj = this->Dereference(); FAULT_ON_NULL(obj);

        //
        // Real value types can be passed as references.
        //
        if(obj->DataType() == DATATYPE_VALUETYPE)
        {
            ref.SetObjectReference( obj );

            obj = &ref;
        }

        TINYCLR_SET_AND_LEAVE(obj->Reassign( value ));
    }   
    else if(value.DataType() == DATATYPE_BYREF)
    {
        obj = value.Dereference(); FAULT_ON_NULL(obj);

        //
        // Real value types can be passed as references.
        //
        if(obj->DataType() == DATATYPE_VALUETYPE)
        {
            ref.SetObjectReference( obj );

            obj = &ref;
        }

        TINYCLR_SET_AND_LEAVE(this->Reassign( *obj ));
    }
    else if(this->DataType() == DATATYPE_ARRAY_BYREF)
    {        
        TINYCLR_CHECK_HRESULT(ref.LoadFromReference( *this     ));
        TINYCLR_CHECK_HRESULT(ref.Reassign         ( value     ));
        TINYCLR_SET_AND_LEAVE(ref.StoreToReference ( *this, -1 ));                
    }
    else if(value.DataType() == DATATYPE_ARRAY_BYREF)
    {
        _ASSERTE( FALSE ); //not tested

        CLR_RT_HeapBlock valueT; valueT.Assign( value );

        TINYCLR_CHECK_HRESULT(ref.LoadFromReference( valueT ));
        TINYCLR_SET_AND_LEAVE(this->Reassign       ( ref   ));
    }
    else
    {
        bool fDestination = this->IsAValueType();
        bool fSource      = value.IsAValueType();
        
        if(fSource != fDestination)
        {
            // For value type objects we don't care if the source item is boxed because
            // CopyValueType will take care of the boxing/unboxing
            if(fDestination != value.IsAReferenceOfThisType(DATATYPE_VALUETYPE))
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
            }
        }

        if(fDestination)
        {
            TINYCLR_SET_AND_LEAVE(g_CLR_RT_ExecutionEngine.CopyValueType( this->Dereference(), value.Dereference() ));
        }

        this->Assign( value );
    }

    TINYCLR_NOCLEANUP();
}

void CLR_RT_HeapBlock::AssignAndPinReferencedObject( const CLR_RT_HeapBlock& value )
{
    // This is very special case that we have local variable with pinned attribute in metadata.
    // This code is called only if "fixed" keyword is present in the managed code. Executed on assigment to "fixed" pointer.
    // First check if there is object referenced by the local var. We unpin it, since the reference is replaced.
    if (m_data.objectReference.ptr != NULL && m_id.type.dataType == DATATYPE_ARRAY_BYREF || m_id.type.dataType == DATATYPE_BYREF)
    {   // Inpin the object that has been pointed by local variable. 
        m_data.objectReference.ptr->Unpin();
    }

    // Move the data.
    m_data = value.m_data;

    // Leave the same logic as in AssignAndPreserveType
    if(DataType() > DATATYPE_LAST_PRIMITIVE_TO_PRESERVE)
    { 
        m_id.type.dataType = value.m_id.type.dataType;
        m_id.type.size     = value.m_id.type.size;
        // We take new flags, but preserve "pinned" attribute
        m_id.type.flags    = m_id.type.flags | HB_Pinned;
    }

    // Pin the object refernced by local variable.  
    if (m_data.objectReference.ptr != NULL && m_id.type.dataType == DATATYPE_ARRAY_BYREF || m_id.type.dataType == DATATYPE_BYREF)
    { 
      m_data.objectReference.ptr->Pin(); 
    }
}


HRESULT CLR_RT_HeapBlock::PerformBoxingIfNeeded()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();


    // we need to box the optimized value types...
    bool fBox = (c_CLR_RT_DataTypeLookup[ this->DataType() ].m_flags & CLR_RT_DataTypeLookup::c_OptimizedValueType) != 0;

    // ... but also the value types under object types
    if(!fBox && this->DataType() == DATATYPE_OBJECT)
    {
        CLR_RT_HeapBlock* src = this->Dereference();

        if(src && src->DataType() == DATATYPE_VALUETYPE && !src->IsBoxed())
        {
            fBox = true;
        }
    }

    if(fBox)
    {
        CLR_RT_TypeDescriptor desc;

        TINYCLR_CHECK_HRESULT(desc.InitializeFromObject( *this ));

        TINYCLR_CHECK_HRESULT(PerformBoxing( desc.m_handlerCls ));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock::PerformBoxing( const CLR_RT_TypeDef_Instance& cls )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock  tmp;
    CLR_RT_HeapBlock* obj = this;
    CLR_DataType      dt  = obj->DataType();

    //
    // System.DateTime and System.TimeSpan are real value types, so sometimes they are passed by reference.
    //
    if(dt == DATATYPE_BYREF)
    {
        obj = obj->Dereference(); FAULT_ON_NULL(obj);
        dt  = obj->DataType();

        //
        // Real value types can be passed as references.
        //
        if(dt == DATATYPE_VALUETYPE)
        {
            tmp.SetObjectReference( obj ); obj = &tmp;

            dt = DATATYPE_OBJECT;
        }
    }

    {
        CLR_DataType                 dataType = (CLR_DataType)cls.m_target->dataType;
        const CLR_RT_DataTypeLookup& dtl      = c_CLR_RT_DataTypeLookup[ dataType ];

        if(dtl.m_flags & CLR_RT_DataTypeLookup::c_OptimizedValueType)
        {
            CLR_RT_HeapBlock* ptr = g_CLR_RT_ExecutionEngine.ExtractHeapBlocksForClassOrValueTypes( DATATYPE_VALUETYPE, HB_Boxed, cls, 2 ); FAULT_ON_NULL(ptr);

            switch(dataType)
            {
            case DATATYPE_DATETIME: // Special case.
            case DATATYPE_TIMESPAN: // Special case.
                dataType = DATATYPE_I8;
                break;
            }

            ptr[ 1 ].SetDataId ( CLR_RT_HEAPBLOCK_RAW_ID(dataType,0,1) );
            ptr[ 1 ].AssignData( *this                                 );

            this->SetObjectReference( ptr );
        }
        else if(dt == DATATYPE_OBJECT)
        {
            CLR_RT_HeapBlock* ptr = this->Dereference();

            if(ptr->IsBoxed() || ptr->DataType() != DATATYPE_VALUETYPE)
            {
                TINYCLR_SET_AND_LEAVE(S_FALSE); // Don't box twice...
            }

            TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.CloneObject( *this, *ptr ));

            this->Dereference()->Box();
        }
        else
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
        }
    }

    TINYCLR_NOCLEANUP();
}

/*******************************************************************************************
*  Thefunction CLR_RT_HeapBlock::PerformUnboxing is used during processing of unbox.any IL instruction.  
*  Example 
*  unbox.any  [mscorlib]System.Int32
*  unbox.any takes the value at the top of evaluation stack and performs unboxing into the type 
*  specified after the instruction. In this case the type is [mscorlib]System.Int32.
*  Function parameters:
*  1. this - Heap block at the top of evaluation stack.
*  2. cls  - Runtime Type Definition of the type specified after instruction.
*  The functoin takes the object pointed by top of ev. stack. Then it does 3 operatioins:
*  1. Dereferences the object 
*  2. Validates the type of data kept by object corresponds to type in cls.
*  3. Moves de-referenced date to top of evaluation stack.
*******************************************************************************************/

HRESULT CLR_RT_HeapBlock::PerformUnboxing( const CLR_RT_TypeDef_Instance& cls )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* src;

    if(this->DataType() != DATATYPE_OBJECT)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    // Finds the object that keeps the boxed type.
    src = this->Dereference(); FAULT_ON_NULL(src);

    // Validates that src keeps something boxed and the boxed value is VALUE type. 
    if(src->IsBoxed() == false || src->DataType() != DATATYPE_VALUETYPE)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    // Validates the type of data kept by object corresponds to type in cls.
    // If typedef indexes are the same, then skip and go to assigment of objects. 
    if(src->ObjectCls().m_data != cls.m_data)
    {
        // The typedef indexes are different, but src and cls may have identical basic data type. 
        // Need to check it. If identical - the unboxing is allowed.
        // This "if" compares underlying type in object and cls. Should be equal in order to continue. 
        if ( !( src->DataSize() > 1  && ( src[ 1 ].DataType() == cls.m_target->dataType ) ) )  
        {
            // No luck. The types in src object and specified by cls are different. Need to throw exceptioin.
            TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_CAST);
        }
    }

    if(cls.m_target->dataType == DATATYPE_VALUETYPE)
    {
        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.CloneObject( *this, *this ));

        this->Dereference()->Unbox();
    }
    else // It's a boxed primitive/enum type.
    {
        this->Assign( src[ 1 ] );

        this->ChangeDataType( cls.m_target->dataType );
    }

    TINYCLR_NOCLEANUP();
}

CLR_RT_HeapBlock* CLR_RT_HeapBlock::FixBoxingReference()
{
    NATIVE_PROFILE_CLR_CORE();
    //
    // Not boxed, nothing to do.
    //
    if(this->DataType() == DATATYPE_OBJECT)
    {
        CLR_RT_HeapBlock* src = this->Dereference(); if(!src) return src;

        if(src->DataType() == DATATYPE_VALUETYPE && src->IsBoxed())
        {
            CLR_RT_TypeDef_Instance inst;

            if(!inst.InitializeFromIndex( src->ObjectCls() )) return NULL;

            if(inst.m_target->dataType != DATATYPE_VALUETYPE) // It's a boxed primitive/enum type.
            {
                return &src[ 1 ];
            }
        }
    }

    return this;
}

//--//

bool CLR_RT_HeapBlock::IsZero() const
{
    NATIVE_PROFILE_CLR_CORE();
    switch(DataType())
    {
    case DATATYPE_OBJECT: return (m_data.objectReference.ptr == NULL);

    case DATATYPE_I8    :
    case DATATYPE_U8    : return (m_data.numeric.u8 == 0);


    case DATATYPE_R4    : return (m_data.numeric.r4 == 0);
    case DATATYPE_R8    : return (m_data.numeric.r8 == 0);


    default             : return (m_data.numeric.u4 == 0);
    }
}

//--//

void CLR_RT_HeapBlock::Promote()
{
    NATIVE_PROFILE_CLR_CORE();

    switch(DataType())
    {
        case DATATYPE_I1     : m_id.type.dataType = DATATYPE_I4; m_data.numeric.s4 = (CLR_INT32 )m_data.numeric.s1; break;

        case DATATYPE_I2     : m_id.type.dataType = DATATYPE_I4; m_data.numeric.s4 = (CLR_INT32 )m_data.numeric.s2; break;

        case DATATYPE_BOOLEAN:
        case DATATYPE_U1     : m_id.type.dataType = DATATYPE_I4; m_data.numeric.u4 = (CLR_UINT32)m_data.numeric.u1; break;

        case DATATYPE_CHAR   :
        case DATATYPE_U2     : m_id.type.dataType = DATATYPE_I4; m_data.numeric.u4 = (CLR_UINT32)m_data.numeric.u2; break;
    }
}

//--//

CLR_UINT32 CLR_RT_HeapBlock::GetHashCode( CLR_RT_HeapBlock* ptr, bool fRecurse, CLR_UINT32 crc = 0 )
{
    NATIVE_PROFILE_CLR_CORE();
    if(!ptr) return 0;

    switch(ptr->DataType())
    {
    case DATATYPE_OBJECT:
        crc = GetHashCode( ptr->Dereference(), fRecurse, crc );
        break;

    case DATATYPE_STRING:
        {
            LPCSTR szText = ptr->StringText();

            crc = SUPPORT_ComputeCRC( szText, (int)hal_strlen_s( szText ), crc );
        }
        break;

    case DATATYPE_CLASS:
    case DATATYPE_VALUETYPE:
        if(fRecurse)
        {
            CLR_RT_TypeDef_Instance cls; cls.InitializeFromIndex( ptr->ObjectCls() );
            int                    totFields = cls.CrossReference().m_totalFields;

            while(totFields-- > 0)
            {
                crc = GetHashCode( ++ptr, false, crc );
            }
        }
        break;

    case DATATYPE_DELEGATE_HEAD:
        {
            CLR_RT_HeapBlock_Delegate*    dlg = (CLR_RT_HeapBlock_Delegate*)ptr;
            const CLR_RT_MethodDef_Index& ftn = dlg->DelegateFtn();

            crc = GetHashCode( &dlg->m_object, false, crc );

            crc = SUPPORT_ComputeCRC( &ftn, sizeof(ftn), crc );
        }
        break;
        
    case DATATYPE_OBJECT_TO_EVENT:
        {
            CLR_RT_ObjectToEvent_Source* evtSrc = (CLR_RT_ObjectToEvent_Source*)ptr;

            crc = GetHashCode(evtSrc->m_eventPtr,  false, crc);
            crc = GetHashCode(evtSrc->m_objectPtr, false, crc);
        }
        break;

    default:
        crc = SUPPORT_ComputeCRC( &ptr->DataByRefConst(), ptr->GetAtomicDataUsedBytes(), crc );
        break;
    }

    return crc;
}

CLR_UINT32 CLR_RT_HeapBlock::GetAtomicDataUsedBytes() const

{
    switch( DataType() )
    {   
        case DATATYPE_BOOLEAN :  // Fall through, hashDataSize = 1
        case DATATYPE_U1      :  // Fall through, hashDataSize = 1 
        case DATATYPE_CHAR    : return 1; 
        
        case DATATYPE_I2      :  // Fall through, hashDataSize = 2
        case DATATYPE_U2      : return 2; break;
        
        case DATATYPE_I4      :
        case DATATYPE_U4      :
        case DATATYPE_R4      : return 4; break;
        
        case DATATYPE_I8      :  // Fall through, hashDataSize = 8
        case DATATYPE_U8      :  // Fall through, hashDataSize = 8 
        case DATATYPE_R8      :  // Fall through, hashDataSize = 8 
        case DATATYPE_DATETIME:  // Fall through, hashDataSize = 8 
        case DATATYPE_TIMESPAN: return 8; break;
        
        // Default full size of CLR_RT_HeapBlock_AtomicData
        default               : return sizeof(CLR_RT_HeapBlock_AtomicData);
    }
    // The same as default. This is never reached, but I put it to avoid potential compiler warning.
    return sizeof(CLR_RT_HeapBlock_AtomicData);
}

bool CLR_RT_HeapBlock::ObjectsEqual( const CLR_RT_HeapBlock& pArgLeft, const CLR_RT_HeapBlock& pArgRight, bool fSameReference )
{
    NATIVE_PROFILE_CLR_CORE();
    if(&pArgLeft == &pArgRight) return true;

    if(pArgLeft.DataType() == pArgRight.DataType())
    {
        switch(pArgLeft.DataType())
        {
        case DATATYPE_VALUETYPE:
            if(pArgLeft.ObjectCls().m_data == pArgRight.ObjectCls().m_data)
            {
                const CLR_RT_HeapBlock* objLeft  = &pArgLeft ;
                const CLR_RT_HeapBlock* objRight = &pArgRight;
                CLR_UINT32              num      =  pArgLeft.DataSize();

                while(--num)
                {
                    if(ObjectsEqual( *++objLeft, *++objRight, false ) == false) return false;
                }

                return true;
            }
            break;

#if defined(TINYCLR_APPDOMAINS)
        case DATATYPE_TRANSPARENT_PROXY:
#endif
        case DATATYPE_OBJECT:
            {
                CLR_RT_HeapBlock* objLeft  = pArgLeft .Dereference();
                CLR_RT_HeapBlock* objRight = pArgRight.Dereference();
                if(objLeft == objRight) return true;

                if(objLeft && objRight)
                {
                    if(!fSameReference || (objLeft->DataType() == DATATYPE_REFLECTION)) return ObjectsEqual( *objLeft, *objRight, false );
                }
            }
            break;

        case DATATYPE_SZARRAY:
            if(fSameReference == false)
            {
                _ASSERTE(false); //can this code path ever be executed?

                CLR_RT_HeapBlock_Array* objLeft  = (CLR_RT_HeapBlock_Array*)&pArgLeft;
                CLR_RT_HeapBlock_Array* objRight = (CLR_RT_HeapBlock_Array*)&pArgRight;

                if(objLeft->m_numOfElements == objRight->m_numOfElements &&
                   objLeft->m_sizeOfElement == objRight->m_sizeOfElement &&
                   objLeft->m_typeOfElement == objRight->m_typeOfElement  )
                {
                    if(!objLeft->m_fReference)
                    {
                        if(memcmp( objLeft->GetFirstElement(), objRight->GetFirstElement(), objLeft->m_numOfElements * objLeft->m_sizeOfElement ) == 0)
                        {
                            return true;
                        }
                    }
                }
            }
            break;
        case DATATYPE_REFLECTION:
            if(pArgLeft.SameHeader( pArgRight )) return true;
            break;
        default:
            if(fSameReference == false)
            {
                const CLR_RT_DataTypeLookup& dtl = c_CLR_RT_DataTypeLookup[ pArgLeft.DataType() ];

                if((dtl.m_flags & CLR_RT_DataTypeLookup::c_Reference) == 0)
                {
                    CLR_UINT32 size = (dtl.m_sizeInBits + 7) / 8;

                    if(memcmp( &pArgLeft.DataByRefConst(), &pArgRight.DataByRefConst(), size ) == 0)
                    {
                        return true;
                    }
                }
            }
            break;
        }
    }

    return false;
}

//--//

static const CLR_RT_HeapBlock* FixReflectionForType( const CLR_RT_HeapBlock& src, CLR_RT_HeapBlock& tmp )
{
    NATIVE_PROFILE_CLR_CORE();
    const CLR_RT_ReflectionDef_Index& rd = src.ReflectionDataConst();

    if(rd.m_kind == REFLECTION_TYPE)
    {
        CLR_RT_TypeDef_Instance inst;
        CLR_UINT32              levels;

        if(inst.InitializeFromReflection( rd, &levels ) && levels == 0)
        {
            tmp.Assign( src );

            CLR_RT_ReflectionDef_Index& rd2 = tmp.ReflectionData();

            rd2.InitializeFromHash( inst.CrossReference().m_hash );

            return &tmp;
        }
    }

    return &src;
}

//--//

static inline int CompareValues_Numeric( CLR_INT32 left, CLR_INT32 right )
{
    NATIVE_PROFILE_CLR_CORE();
    if(left > right) return  1;
    if(left < right) return -1;
    /**************/ return  0;
}

static inline int CompareValues_Numeric( CLR_UINT32 left, CLR_UINT32 right )
{
    NATIVE_PROFILE_CLR_CORE();
    if(left > right) return  1;
    if(left < right) return -1;
    /**************/ return  0;
}

static int CompareValues_Numeric( const CLR_INT64 left, const CLR_INT64 right )
{
    NATIVE_PROFILE_CLR_CORE();

    if(left > right) return  1;
    if(left < right) return -1;
    /**************/ return  0;
}

static int CompareValues_Numeric( const CLR_UINT64 left, const CLR_UINT64 right )
{
    NATIVE_PROFILE_CLR_CORE();
    if(left > right) return  1;
    if(left < right) return -1;
    /**************/ return  0;
}

static int CompareValues_Numeric( const CLR_RT_HeapBlock& left, const CLR_RT_HeapBlock& right, bool fSigned, int bytes )
{
    NATIVE_PROFILE_CLR_CORE();

    switch(bytes)
    {
        case 4:
            if(fSigned) return CompareValues_Numeric( (CLR_INT32 )left.NumericByRefConst().s4, (CLR_INT32 )right.NumericByRefConst().s4 );
            else        return CompareValues_Numeric( (CLR_UINT32)left.NumericByRefConst().u4, (CLR_UINT32)right.NumericByRefConst().u4 );

        case 8:
            if(fSigned) return CompareValues_Numeric( (CLR_INT64 )left.NumericByRefConst().s8, (CLR_INT64 )right.NumericByRefConst().s8 );
            else        return CompareValues_Numeric( (CLR_UINT64)left.NumericByRefConst().u8, (CLR_UINT64)right.NumericByRefConst().u8 );

        case 2:
            if(fSigned) return CompareValues_Numeric( (CLR_INT32 )left.NumericByRefConst().s2, (CLR_INT32 )right.NumericByRefConst().s2 );
            else        return CompareValues_Numeric( (CLR_UINT32)left.NumericByRefConst().u2, (CLR_UINT32)right.NumericByRefConst().u2 );
            
        case 1:
            if(fSigned) return CompareValues_Numeric( (CLR_INT32 )left.NumericByRefConst().s1, (CLR_INT32 )right.NumericByRefConst().s1 );
            else        return CompareValues_Numeric( (CLR_UINT32)left.NumericByRefConst().u1, (CLR_UINT32)right.NumericByRefConst().u1 );

        default:
            return -1;
    }
}



static inline int CompareValues_Pointers( const CLR_RT_HeapBlock* left, const CLR_RT_HeapBlock* right )
{
    NATIVE_PROFILE_CLR_CORE();
    if(left > right) return  1;
    if(left < right) return -1;
    /**************/ return  0;
}


CLR_INT32 CLR_RT_HeapBlock::Compare_Values( const CLR_RT_HeapBlock& left, const CLR_RT_HeapBlock& right, bool fSigned )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_DataType leftDataType  = left .DataType();
    CLR_DataType rightDataType = right.DataType();

    if(leftDataType == rightDataType)
    {
        switch(leftDataType)
        {
#if defined(TINYCLR_APPDOMAINS)
        case DATATYPE_TRANSPARENT_PROXY:
#endif
        case DATATYPE_OBJECT:
        case DATATYPE_BYREF:
            {
                CLR_RT_HeapBlock* leftObj  = left .Dereference();
                CLR_RT_HeapBlock* rightObj = right.Dereference();

                if(!leftObj)
                {
                    return !rightObj ? 0 : -1; // NULL references always compare smaller than non-NULL ones.
                }
                else if(!rightObj)
                {
                    return 1; // NULL references always compare smaller than non-NULL ones.
                }

                return Compare_Values( *leftObj, *rightObj, fSigned );
            }

        case DATATYPE_STRING:
            {
                CLR_RT_HeapBlock_String* leftStr  = (CLR_RT_HeapBlock_String*)&left ;
                CLR_RT_HeapBlock_String* rightStr = (CLR_RT_HeapBlock_String*)&right;

                return strcmp( leftStr->StringText(), rightStr->StringText() );
            }

        case DATATYPE_DELEGATELIST_HEAD:
            {
                CLR_RT_HeapBlock_Delegate_List* leftDlg  = (CLR_RT_HeapBlock_Delegate_List*)&left ;
                CLR_RT_HeapBlock_Delegate_List* rightDlg = (CLR_RT_HeapBlock_Delegate_List*)&right;
                CLR_RT_HeapBlock*               leftPtr  = leftDlg ->GetDelegates();
                CLR_RT_HeapBlock*               rightPtr = rightDlg->GetDelegates();
                CLR_UINT32                      leftLen  = leftDlg ->m_length;
                CLR_UINT32                      rightLen = rightDlg->m_length;

                while(leftLen > 0 && rightLen > 0)
                {
                    int res = CLR_RT_HeapBlock::Compare_Values( *leftPtr++, *rightPtr++, fSigned ); if(res) return res;

                    leftLen--;
                    rightLen--;
                }

                if(!leftLen)
                {
                    return !rightLen ? 0 : -1; // NULL references always compare smaller than non-NULL ones.
                }
                else // rightLen != 0 for sure.
                {
                    return 1; // NULL references always compare smaller than non-NULL ones.
                }
            }

        case DATATYPE_DELEGATE_HEAD:
            {
                CLR_RT_HeapBlock_Delegate* leftDlg   = (CLR_RT_HeapBlock_Delegate*)&left ;
                CLR_RT_HeapBlock_Delegate* rightDlg  = (CLR_RT_HeapBlock_Delegate*)&right;
                CLR_UINT32                 leftData  = leftDlg ->DelegateFtn().m_data;
                CLR_UINT32                 rightData = rightDlg->DelegateFtn().m_data;

                if(leftData > rightData) return  1;
                if(leftData < rightData) return -1;

                return Compare_Values( leftDlg->m_object, rightDlg->m_object, fSigned );
            }

        case DATATYPE_CLASS:
        case DATATYPE_VALUETYPE:
        case DATATYPE_SZARRAY:
        case DATATYPE_WEAKCLASS:
            return CompareValues_Pointers( &left, &right );

        case DATATYPE_REFLECTION:
            {
                const CLR_RT_HeapBlock* ptrLeft;
                const CLR_RT_HeapBlock* ptrRight;
                CLR_RT_HeapBlock        hbLeft;
                CLR_RT_HeapBlock        hbRight;

                if(left.ReflectionDataConst().m_kind != right.ReflectionDataConst().m_kind)
                {
                    ptrLeft  = FixReflectionForType( left , hbLeft  );
                    ptrRight = FixReflectionForType( right, hbRight );
                }
                else
                {
                    ptrLeft  = &left;
                    ptrRight = &right;
                }

                return CompareValues_Numeric( *ptrLeft, *ptrRight, false, 8 );
            }

            //--//
#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)

        case DATATYPE_R4:
            if(left.NumericByRefConst().r4 > right.NumericByRefConst().r4) return  1;
            if(left.NumericByRefConst().r4 < right.NumericByRefConst().r4) return -1;
            /************************************************************/ return  0;

        case DATATYPE_R8:

            if(left.NumericByRefConst().r8 > right.NumericByRefConst().r8) return  1;
            if(left.NumericByRefConst().r8 < right.NumericByRefConst().r8) return -1;
            /************************************************************/ return  0;

#else
        case DATATYPE_R4      :
        case DATATYPE_R8      :
            fSigned = true;
#endif

        case DATATYPE_BOOLEAN :
        case DATATYPE_I1      :
        case DATATYPE_U1      :

        case DATATYPE_CHAR    :
        case DATATYPE_I2      :
        case DATATYPE_U2      :

        case DATATYPE_I4      :
        case DATATYPE_U4      :

        case DATATYPE_I8      :
        case DATATYPE_U8      :
        case DATATYPE_DATETIME:
        case DATATYPE_TIMESPAN:
            return CompareValues_Numeric( left, right, fSigned, c_CLR_RT_DataTypeLookup[ leftDataType ].m_sizeInBytes );
        }
    }
    else
    {
        if(leftDataType == DATATYPE_STRING && rightDataType == DATATYPE_OBJECT)
        {
            CLR_RT_HeapBlock* rightObj = right.Dereference();

            if(!rightObj)
            {
                return 1; // NULL references always compare smaller than non-NULL ones.
            }

            return Compare_Values( left, *rightObj, fSigned );
        }

        if(leftDataType == DATATYPE_OBJECT && rightDataType == DATATYPE_STRING)
        {
            CLR_RT_HeapBlock* leftObj = left.Dereference();

            if(!leftObj)
            {
                return -1; // NULL references always compare smaller than non-NULL ones.
            }

            return Compare_Values( *leftObj, right, fSigned );
        }

        //--//

        const CLR_RT_DataTypeLookup& leftDtl  = c_CLR_RT_DataTypeLookup[ leftDataType  ];
        const CLR_RT_DataTypeLookup& rightDtl = c_CLR_RT_DataTypeLookup[ rightDataType ];

        if((leftDtl .m_flags & CLR_RT_DataTypeLookup::c_Numeric) &&
           (rightDtl.m_flags & CLR_RT_DataTypeLookup::c_Numeric)  )
        {
            if(leftDtl.m_sizeInBytes == rightDtl.m_sizeInBytes)
            {
                return CompareValues_Numeric( left, right, fSigned, leftDtl.m_sizeInBytes );
            }
            else
            {
                CLR_Debug::Printf( "\r\n\r\nRUNTIME ERROR: comparing two values of different size: %d vs. %d!!!\r\n\r\n\r\n", leftDataType, rightDataType );
#if defined(TINYCLR_PROFILE_NEW)
                g_CLR_PRF_Profiler.DumpHeap();
#endif
            }
        }
    }

    return -1; // Not comparable...
}

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_RT_HeapBlock::NumericAdd( const CLR_RT_HeapBlock& right )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    switch(DataType())
    {
    case DATATYPE_I4: m_data.numeric.s4 += right.m_data.numeric.s4; break;

    case DATATYPE_U4: m_data.numeric.u4 += right.m_data.numeric.u4; break;

    case DATATYPE_I8: m_data.numeric.s8 += right.m_data.numeric.s8; break;

    case DATATYPE_U8: m_data.numeric.u8 += right.m_data.numeric.u8; break;

    case DATATYPE_R4: 
#if defined(TINYCLR_EMULATED_FLOATINGPOINT)
    {
        CLR_INT32 orig = (CLR_INT32)m_data.numeric.r4;
        CLR_INT32 rhs  = (CLR_INT32)right.m_data.numeric.r4;
#endif
        m_data.numeric.r4 += right.m_data.numeric.r4; 

#if defined(TINYCLR_EMULATED_FLOATINGPOINT)
             if(rhs > 0 && orig > 0 && orig > (CLR_INT32)m_data.numeric.r4) { m_data.numeric.r4 =                        0x7FFFFFFF; /*return CLR_E_OUT_OF_RANGE*/ }
        else if(rhs < 0 && orig < 0 && orig < (CLR_INT32)m_data.numeric.r4) { m_data.numeric.r4 = (CLR_INT32)(CLR_UINT32)0x80000000; /*return CLR_E_OUT_OF_RANGE*/ }
    }
#endif
        break;

    case DATATYPE_R8: 
#if defined(TINYCLR_EMULATED_FLOATINGPOINT)
    {
        CLR_INT64 orig = (CLR_INT64)m_data.numeric.r8;
        CLR_INT64 rhs  = (CLR_INT64)right.m_data.numeric.r8;
#endif
        m_data.numeric.r8 += right.m_data.numeric.r8; 

#if defined(TINYCLR_EMULATED_FLOATINGPOINT)
             if(rhs > 0 && orig > 0 && orig > (CLR_INT64)m_data.numeric.r8) { m_data.numeric.r8 = (CLR_INT64)ULONGLONGCONSTANT(0x7FFFFFFFFFFFFFFF); /*return CLR_E_OUT_OF_RANGE*/ }
        else if(rhs < 0 && orig < 0 && orig < (CLR_INT64)m_data.numeric.r8) { m_data.numeric.r8 = (CLR_INT64)ULONGLONGCONSTANT(0x8000000000000000); /*return CLR_E_OUT_OF_RANGE*/ }
    }
#endif
        break;


    // Adding of value to array reference is like advancing the index in array.
    case DATATYPE_ARRAY_BYREF: 
    {
        // Retrieve refernced array. Test if it is not NULL
        CLR_RT_HeapBlock_Array* array = m_data.arrayReference.array; FAULT_ON_NULL(array);
        // Advance current index. C# on pointer operations multiplies the offset by object size. We need to reverse it.
        m_data.arrayReference.index += right.m_data.numeric.s4 / array->m_sizeOfElement;
    }
    break;

    default         : TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock::NumericSub( const CLR_RT_HeapBlock& right )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    switch(DataType())
    {
    case DATATYPE_U4: m_data.numeric.u4 -= right.m_data.numeric.u4; break;

    case DATATYPE_I4: m_data.numeric.s4 -= right.m_data.numeric.s4; break;

    case DATATYPE_U8: m_data.numeric.u8 -= right.m_data.numeric.u8; break;

    case DATATYPE_I8: m_data.numeric.s8 -= right.m_data.numeric.s8; break;

    case DATATYPE_R4: 
#if defined(TINYCLR_EMULATED_FLOATINGPOINT)
    {
        CLR_INT32 orig = (CLR_INT32)m_data.numeric.r8;
        CLR_INT32 rhs  = (CLR_INT32)right.m_data.numeric.r4;
#endif
        m_data.numeric.r4 -= right.m_data.numeric.r4; 

#if defined(TINYCLR_EMULATED_FLOATINGPOINT)
             if(rhs < 0 && orig > 0 && orig > (CLR_INT32)m_data.numeric.r4) { m_data.numeric.r4 =                        0x7FFFFFFF; /*return CLR_E_OUT_OF_RANGE*/ }
        else if(rhs > 0 && orig < 0 && orig < (CLR_INT32)m_data.numeric.r4) { m_data.numeric.r4 = (CLR_INT32)(CLR_UINT32)0x80000000; /*return CLR_E_OUT_OF_RANGE*/ }
    }
#endif

        break;

    case DATATYPE_R8: 
#if defined(TINYCLR_EMULATED_FLOATINGPOINT)
    {
        CLR_INT64 orig = (CLR_INT64)m_data.numeric.r8;
        CLR_INT64 rhs  = (CLR_INT64)right.m_data.numeric.r8;
#endif

        m_data.numeric.r8 -= right.m_data.numeric.r8; 

#if defined(TINYCLR_EMULATED_FLOATINGPOINT)
             if(rhs < 0 && orig > 0 && orig > (CLR_INT64)m_data.numeric.r8) { m_data.numeric.r8 = (CLR_INT64)ULONGLONGCONSTANT(0x7FFFFFFFFFFFFFFF); /*return CLR_E_OUT_OF_RANGE*/ }
        else if(rhs > 0 && orig < 0 && orig < (CLR_INT64)m_data.numeric.r8) { m_data.numeric.r8 = (CLR_INT64)ULONGLONGCONSTANT(0x8000000000000000); /*return CLR_E_OUT_OF_RANGE*/ }
    }
#endif

        break;


    // Substructing of value to array reference is like decreasing the index in array.
    case DATATYPE_ARRAY_BYREF: 
    {
        // Retrieve refernced array. Test if it is not NULL
        CLR_RT_HeapBlock_Array* array = m_data.arrayReference.array; FAULT_ON_NULL(array);
        // Advance current index. C# on pointer operations multiplies the offset by object size. We need to reverse it.
        m_data.arrayReference.index -= right.m_data.numeric.s4 / array->m_sizeOfElement;
    }
    break;
    default         : TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock::NumericMul( const CLR_RT_HeapBlock& right )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    switch(DataType())
    {
    case DATATYPE_U4: m_data.numeric.u4 = m_data.numeric.u4 * right.m_data.numeric.u4; break;

    case DATATYPE_I4: m_data.numeric.s4 = m_data.numeric.s4 * right.m_data.numeric.s4; break;

    case DATATYPE_U8: m_data.numeric.u8 = m_data.numeric.u8 * right.m_data.numeric.u8; break;

    case DATATYPE_I8: m_data.numeric.s8 = m_data.numeric.s8 * right.m_data.numeric.s8; break;

    case DATATYPE_R4: 
#if defined(TINYCLR_EMULATED_FLOATINGPOINT)
    {
        CLR_INT32 orig = (CLR_INT32)m_data.numeric.r4;
        CLR_INT32 rhs;
#endif
        m_data.numeric.r4 = m_data.numeric.r4 * right.m_data.numeric.r4; 

#if defined(TINYCLR_EMULATED_FLOATINGPOINT)
        rhs = (CLR_INT32)right.m_data.numeric.r4;

        if(orig != 0 && rhs != 0)
        {
            CLR_INT32 ret_value = (CLR_INT32)m_data.numeric.r4;
            bool      isNeg     = orig < 0;

            if(rhs < 0) isNeg = !isNeg;
        
                 if(!isNeg && (ret_value < 0 || ret_value < orig || ret_value < rhs)) { m_data.numeric.r4 =                        0x7FFFFFFF; /* return CLR_E_OUT_OF_RANGE; */ }
            else if( isNeg && (ret_value > 0 || ret_value > orig || ret_value > rhs)) { m_data.numeric.r4 = (CLR_INT32)(CLR_UINT32)0x80000000; /* return CLR_E_OUT_OF_RANGE; */ }
        }
    }
#endif
        break;

    case DATATYPE_R8: 
#if defined(TINYCLR_EMULATED_FLOATINGPOINT)
    {
        CLR_INT64 orig = (CLR_INT64)m_data.numeric.r8;
        CLR_INT64 rhs;
#endif
        m_data.numeric.r8 = m_data.numeric.r8 * right.m_data.numeric.r8; 

#if defined(TINYCLR_EMULATED_FLOATINGPOINT)
        rhs = (CLR_INT64)right.m_data.numeric.r8;

        if(orig != 0 && rhs != 0)
        {
            CLR_INT64 ret_value = (CLR_INT64)m_data.numeric.r8;
            bool      isNeg     = orig < 0;

            if(rhs < 0) isNeg = !isNeg;
        
                 if(!isNeg && (ret_value < 0 || ret_value < orig || ret_value < rhs)) { m_data.numeric.r8 = (CLR_INT64)ULONGLONGCONSTANT(0x7FFFFFFFFFFFFFFF); /* return CLR_E_OUT_OF_RANGE; */ }
            else if( isNeg && (ret_value > 0 || ret_value > orig || ret_value > rhs)) { m_data.numeric.r8 = (CLR_INT64)ULONGLONGCONSTANT(0x8000000000000000); /* return CLR_E_OUT_OF_RANGE; */ }
        }
    }
#endif
        break;

    default         : TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock::NumericDiv( const CLR_RT_HeapBlock& right )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(right.IsZero()) TINYCLR_SET_AND_LEAVE(CLR_E_DIVIDE_BY_ZERO);

    switch(DataType())
    {
    case DATATYPE_U4:
    case DATATYPE_I4: m_data.numeric.s4 = m_data.numeric.s4 / right.m_data.numeric.s4; break;

    case DATATYPE_U8:
    case DATATYPE_I8: m_data.numeric.s8 = m_data.numeric.s8 / right.m_data.numeric.s8; break;

    case DATATYPE_R4: m_data.numeric.r4 = m_data.numeric.r4 / right.m_data.numeric.r4; break;

    case DATATYPE_R8: m_data.numeric.r8 = m_data.numeric.r8 / right.m_data.numeric.r8; break;

    default         : TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock::NumericDivUn( const CLR_RT_HeapBlock& right )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(right.IsZero()) TINYCLR_SET_AND_LEAVE(CLR_E_DIVIDE_BY_ZERO);

    switch(DataType())
    {
    case DATATYPE_I4:
    case DATATYPE_U4: m_data.numeric.u4 =             m_data.numeric.u4 / right.m_data.numeric.u4; break;

    case DATATYPE_I8:
    case DATATYPE_U8:m_data.numeric.u8 = m_data.numeric.u8 / right.m_data.numeric.u8; break;

    default         : TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock::NumericRem( const CLR_RT_HeapBlock& right )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(right.IsZero()) TINYCLR_SET_AND_LEAVE(CLR_E_DIVIDE_BY_ZERO);

    switch(DataType())
    {
    case DATATYPE_U4: m_data.numeric.u4 %=                          right.m_data.numeric.u4  ; break;

    case DATATYPE_I4: m_data.numeric.s4 %=                          right.m_data.numeric.s4  ; break;

    case DATATYPE_U8: m_data.numeric.u8 %=                          right.m_data.numeric.u8  ; break;

    case DATATYPE_I8: m_data.numeric.s8 %=                          right.m_data.numeric.s8  ; break;

#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)

    case DATATYPE_R4: m_data.numeric.r4  = fmod( m_data.numeric.r4, right.m_data.numeric.r4 ); break;

    case DATATYPE_R8: m_data.numeric.r8  = fmod( (CLR_DOUBLE_TEMP_CAST)m_data.numeric.r8,(CLR_DOUBLE_TEMP_CAST) right.m_data.numeric.r8 ); break;

#else

    case DATATYPE_R4: m_data.numeric.r4 %=                           right.m_data.numeric.r4; break;

    case DATATYPE_R8: m_data.numeric.r8 %=                           right.m_data.numeric.r8; break;

#endif


    default         : TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock::NumericRemUn( const CLR_RT_HeapBlock& right )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(right.IsZero()) TINYCLR_SET_AND_LEAVE(CLR_E_DIVIDE_BY_ZERO);

    switch(DataType())
    {
    case DATATYPE_I4:
    case DATATYPE_U4: m_data.numeric.u4 %= right.m_data.numeric.u4; break;

    case DATATYPE_I8:
    case DATATYPE_U8: m_data.numeric.u8 %= right.m_data.numeric.u8; break;

    default         : TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock::NumericShl( const CLR_RT_HeapBlock& right )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    switch(DataType())
    {
    case DATATYPE_I4:
    case DATATYPE_U4: m_data.numeric.u4 <<= right.m_data.numeric.u4; break;

    case DATATYPE_I8:
    case DATATYPE_U8: m_data.numeric.u8 <<= right.m_data.numeric.u4; break;

    default         : TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock::NumericShr( const CLR_RT_HeapBlock& right )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    switch(DataType())
    {
    case DATATYPE_U4: m_data.numeric.u4 >>= right.m_data.numeric.u4; break;

    case DATATYPE_I4: m_data.numeric.s4 >>= right.m_data.numeric.u4; break;

    case DATATYPE_U8: m_data.numeric.u8 >>= right.m_data.numeric.u4; break;

    case DATATYPE_I8: m_data.numeric.s8 >>= right.m_data.numeric.u4; break;

    default         : TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock::NumericShrUn( const CLR_RT_HeapBlock& right )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    switch(DataType())
    {
    case DATATYPE_I4:
    case DATATYPE_U4: m_data.numeric.u4 >>= right.m_data.numeric.u4; break;

    case DATATYPE_I8:
    case DATATYPE_U8: m_data.numeric.u8 >>= right.m_data.numeric.u4; break;

    default         : TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock::NumericNeg()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    switch(DataType())
    {
    case DATATYPE_U4:
    case DATATYPE_I4: m_data.numeric.s4 = -m_data.numeric.s4; break;

    case DATATYPE_U8:
    case DATATYPE_I8: m_data.numeric.s8 = -m_data.numeric.s8; break;

    case DATATYPE_R4: m_data.numeric.r4 = -m_data.numeric.r4; break;

    case DATATYPE_R8: m_data.numeric.r8 = -m_data.numeric.r8; break;



    default         : TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

CLR_RT_HeapBlock* CLR_RT_HeapBlock::ExtractValueBlock( int offset )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock* ptr = Dereference();

    if(ptr)
    {
        ptr = &ptr[ offset ];
    }

    return ptr;
}

void CLR_RT_HeapBlock::ReadValue( CLR_INT64& val, int offset )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock* ptr = ExtractValueBlock( offset );

    if(ptr)
    {
        val = ptr->NumericByRefConst().s8;
    }
    else
    {
        CLR_INT32 val2 = 0;

        val = val2;
    }
}

void CLR_RT_HeapBlock::WriteValue( const CLR_INT64& val, int offset )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock* ptr = ExtractValueBlock( offset );

    if(ptr) ptr->NumericByRef().s8 = val;
}


////////////////////////////////////////////////////////////////////////////////////////////////////
#if defined(TINYCLR_APPDOMAINS)

void CLR_RT_HeapBlock::SetTransparentProxyReference( CLR_RT_AppDomain* appDomain, CLR_RT_HeapBlock* ptr)
    {
#if defined(_DEBUG)
        if(ptr)
        {                    
            //Make sure the data points to a MBRO.        
            CLR_RT_TypeDef_Instance inst;

            _ASSERTE(ptr->DataType() == DATATYPE_CLASS);            
                
            inst.InitializeFromIndex( ptr->ObjectCls() );
            _ASSERTE((inst.CrossReference().m_flags & CLR_RT_TypeDef_CrossReference::TD_CR_IsMarshalByRefObject) != 0);            
        }        
#endif

        m_data.transparentProxy.appDomain = appDomain;
        m_data.transparentProxy.ptr       = ptr;
    }

HRESULT CLR_RT_HeapBlock::TransparentProxyValidate() const
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_AppDomain* appDomain = TransparentProxyAppDomain  ();
    CLR_RT_HeapBlock* obj       = TransparentProxyDereference();
    
    if(appDomain == NULL || !appDomain->IsLoaded()) TINYCLR_SET_AND_LEAVE(CLR_E_APPDOMAIN_EXITED);
        
    FAULT_ON_NULL(obj);

    TINYCLR_NOCLEANUP();
}

#endif //TINYCLR_APPDOMAINS
////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_RT_HeapBlock::Relocate__HeapBlock()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HEAPBLOCK_RELOCATE(this);
}


void CLR_RT_HeapBlock::Relocate_String()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_data.string.m_text );
#if !defined(TINYCLR_NO_ASSEMBLY_STRINGS)
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_data.string.m_assm );
#endif
}

void CLR_RT_HeapBlock::Relocate_Obj()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_data.objectReference.ptr );
}

void CLR_RT_HeapBlock::Relocate_Cls()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_data.objectHeader.lock );

    CLR_RT_GarbageCollector::Heap_Relocate( this + 1, DataSize() - 1 );
}

void CLR_RT_HeapBlock::Relocate_Ref()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_data.objectReference.ptr );
}

void CLR_RT_HeapBlock::Relocate_ArrayRef()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_data.arrayReference.array );
}

#if defined(TINYCLR_APPDOMAINS)
void CLR_RT_HeapBlock::Relocate_TransparentProxy()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_data.transparentProxy.ptr ); 
}
#endif

//--//

#if defined(TINYCLR_FILL_MEMORY_WITH_DIRTY_PATTERN)

void CLR_RT_HeapBlock::Debug_CheckPointer() const
{
    NATIVE_PROFILE_CLR_CORE();
    if(m_id.type.dataType == DATATYPE_OBJECT)
    {
        Debug_CheckPointer( Dereference() );
    }
}

void CLR_RT_HeapBlock::Debug_CheckPointer( void* ptr )
{
    NATIVE_PROFILE_CLR_CORE();
    switch((size_t)ptr)
    {
    case 0xCFCFCFCF:
    case 0xCBCBCBCB:
    case 0xABABABAB:
    case 0xADADADAD:
    case 0xDFDFDFDF:
        TINYCLR_STOP();
        break;
    }
}

void CLR_RT_HeapBlock::Debug_ClearBlock( int data )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_UINT32 size = DataSize();

    if(size > 1)
    {
        CLR_RT_HeapBlock_Raw* ptr  = (CLR_RT_HeapBlock_Raw*)this;
        CLR_UINT32            raw1 = CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_OBJECT,0,1);
        CLR_UINT32            raw2;

        raw2 = data & 0xFF;
        raw2 = raw2 | (raw2 <<  8);
        raw2 = raw2 | (raw2 << 16);

        while(--size)
        {
            ptr++;

            ptr->data[ 0 ] = raw1;
            ptr->data[ 1 ] = raw2;
            ptr->data[ 2 ] = raw2;
        }
    }
}

#endif
