////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

//this method simply informs if Serialization is enabled
//the stub version of this method returns False
bool CLR_RT_BinaryFormatter::SerializationEnabled()
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    return true;
}

CLR_RT_HeapBlock* CLR_RT_BinaryFormatter::TypeHandler::FixDereference( CLR_RT_HeapBlock* v )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    return (v && v->DataType() == DATATYPE_OBJECT) ? v->Dereference() : v;
}

CLR_RT_HeapBlock* CLR_RT_BinaryFormatter::TypeHandler::FixNull( CLR_RT_HeapBlock* v )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    if(v && v->DataType() == DATATYPE_OBJECT && v->Dereference() == NULL) return NULL;

    return v;
}

//--//

HRESULT CLR_RT_BinaryFormatter::TypeHandler::TypeHandler_Initialize( CLR_RT_BinaryFormatter* bf, SerializationHintsAttribute* hints, CLR_RT_TypeDescriptor* expected )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    TINYCLR_CLEAR(*this);

    m_bf = bf;

    if(hints)
    {
        m_hints = *hints;
    }

    if(expected)
    {
        m_typeExpected = &m_typeExpected_tmp;

        *m_typeExpected = *expected;
    }

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT CLR_RT_BinaryFormatter::TypeHandler::SetValue( CLR_RT_HeapBlock* v )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    m_value = NULL;
    m_type  = NULL;

    v = TypeHandler::FixNull( v );
    if(v)
    {
        m_value = &m_value_tmp;
        m_type  = &m_type_tmp;

        m_value->Assign( *v );

        TINYCLR_CHECK_HRESULT(m_type->InitializeFromObject( *v ));

#if defined(TINYCLR_APPDOMAINS)
        if(m_bf->m_flags & CLR_RT_BinaryFormatter::c_Flags_Marshal)
        {
           //MarshalByRefObjects are special for the AppDomain marshaler, and need to get marshaled regardless of whether or not they
           //are serializable

           if((m_type->m_handlerCls.CrossReference().m_flags & CLR_RT_TypeDef_CrossReference::TD_CR_IsMarshalByRefObject) ||
               m_value->IsTransparentProxy())
           {
                m_fIsMarshalByRefObject = true;
           }
        }
            
#endif

        if((m_type->m_handlerCls.m_target->flags & CLR_RECORD_TYPEDEF::TD_Serializable) == 0)
        {
#if defined(TINYCLR_APPDOMAINS)
                
            if(m_fIsMarshalByRefObject) TINYCLR_SET_AND_LEAVE(S_OK);

            //For marshaling, we want to throw an exception if we can't correctly marshal an object across AD boundaries
            //In fact, this should probably be turned on for all serialization, but I don't know what other implications that
            //would have

            if(m_bf->m_flags & CLR_RT_BinaryFormatter::c_Flags_Marshal)
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_APPDOMAIN_MARSHAL_EXCEPTION);
            }
#endif
            m_value = NULL;
            m_type  = NULL;
        }        
    }

    TINYCLR_NOCLEANUP();
}

//--//

int CLR_RT_BinaryFormatter::TypeHandler::SignatureRequirements()
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    int                    res = c_Signature_Header | c_Signature_Type | c_Signature_Length;
    CLR_RT_TypeDescriptor* td;
    CLR_RT_TypeDescriptor  sub;

    if(m_hints.m_arraySize != 0)
    {
        res &= ~c_Signature_Length;
    }

    m_typeForced = NULL;

    if(m_typeExpected != NULL)
    {
        switch(m_typeExpected->m_flags)
        {
        case CLR_RT_DataTypeLookup::c_Primitive:
        case CLR_RT_DataTypeLookup::c_Enum     :
        case CLR_RT_DataTypeLookup::c_ValueType:
            res = 0;
            break;

        default:
            if(m_hints.m_flags & SF_FixedType)
            {
                res &= ~c_Signature_Type;
                break;
            }

            if(m_typeExpected->m_flags & CLR_RT_DataTypeLookup::c_Array)
            {
                sub.InitializeFromType( m_typeExpected->m_reflex.m_data.m_type );

                td = &sub;
            }
            else
            {
                td = m_typeExpected;
            }

            if(td->m_handlerCls.m_target->flags & CLR_RECORD_TYPEDEF::TD_Sealed)
            {
                res &= ~c_Signature_Type;
                break;
            }
        }
    }

    if((res & c_Signature_Type) == 0)
    {
        m_typeForced  = &m_typeForced_tmp;
        *m_typeForced = *m_typeExpected;

        if(m_hints.m_flags & SF_PointerNeverNull)
        {
            res &= ~c_Signature_Header;
        }
    }

    return res;
}

bool CLR_RT_BinaryFormatter::TypeHandler::CompareTypes( CLR_RT_TypeDescriptor* left, CLR_RT_TypeDescriptor* right )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    if(!left && !right) return true;
    if(!left || !right) return false;

    return memcmp( left, right, sizeof(*left) ) == 0;
}

CLR_DataType CLR_RT_BinaryFormatter::TypeHandler::GetDataType( CLR_RT_TypeDescriptor* type )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    return (CLR_DataType)type->m_handlerCls.m_target->dataType;
}

CLR_UINT32 CLR_RT_BinaryFormatter::TypeHandler::GetSizeOfType( CLR_RT_TypeDescriptor* type )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    return c_CLR_RT_DataTypeLookup[ type->GetDataType() ].m_sizeInBits;
}

bool CLR_RT_BinaryFormatter::TypeHandler::GetSignOfType( CLR_RT_TypeDescriptor* type )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    return (c_CLR_RT_DataTypeLookup[ type->GetDataType() ].m_flags & CLR_RT_DataTypeLookup::c_Signed) != 0;
}

//--//

HRESULT CLR_RT_BinaryFormatter::TypeHandler::EmitSignature( int& res )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    CLR_UINT32 idx;
    int        mask = SignatureRequirements();

    if((mask & c_Signature_Type) == 0)
    {
        if(m_type && CompareTypes( m_type, m_typeForced ) == false)
        {
            while(true)
            {
                if(m_typeForced->m_flags == CLR_RT_DataTypeLookup::c_Enum)
                {
                    break;
                }

                if(m_typeForced->m_handlerCls.IsATypeHandler() &&
                   m_type      ->m_handlerCls.IsATypeHandler()  )
                {
                    break;
                }

                TINYCLR_SET_AND_LEAVE(CLR_E_SERIALIZATION_VIOLATION);
            }
        }
    }

    if(m_value == NULL)
    {
        if(mask == 0)
        {
            //
            // Special case for null strings (strings don't emit an hash): send a string of length -1.
            //
            if(m_typeExpected->GetDataType() == DATATYPE_STRING)
            {

                TINYCLR_CHECK_HRESULT(m_bf->WriteCompressedUnsigned( 0xFFFFFFFF ));

                res = c_Action_None; TINYCLR_SET_AND_LEAVE(S_OK);
            }

            TINYCLR_SET_AND_LEAVE(CLR_E_SERIALIZATION_VIOLATION);
        }


        TINYCLR_CHECK_HRESULT(m_bf->WriteBits( TE_L1_Null, TE_L1 ));

        res = c_Action_None; TINYCLR_SET_AND_LEAVE(S_OK);
    }

    //--//

#if defined(TINYCLR_APPDOMAINS)
    if(m_fIsMarshalByRefObject)
    {
        CLR_RT_HeapBlock* valPtr = m_value->Dereference();

        TINYCLR_CHECK_HRESULT(m_bf->WriteBits( TE_L1_Other, TE_L1 ));
        TINYCLR_CHECK_HRESULT(m_bf->WriteBits( TE_L2_Other, TE_L2 ));
        TINYCLR_CHECK_HRESULT(m_bf->WriteBits( TE_L3_MBRO , TE_L3 ));

        {            
            /*
                The bit stream contains pointers to the MBRO and its AppDomain.  Only the header is used, and no
                duplicate detection is necessary -- all proxies pointing at the same object are equal.
                which will get marshaled into either a proxy or MBRO, depending on the destination AppDomain.
                Live pointers are stored, but this is ok since it is for marshaling only.  The objects cannot 
                can collected during a GC due to the live object that is being marshaled.                  
            */

            _ASSERTE(CLR_EE_DBG_IS(NoCompaction));

            CLR_RT_HeapBlock* ptr;
            CLR_RT_AppDomain* appDomain;

            if(valPtr->DataType() == DATATYPE_TRANSPARENT_PROXY)
            {
                ptr       = valPtr->TransparentProxyDereference();
                appDomain = valPtr->TransparentProxyAppDomain  ();
            }
            else
            {
                ptr       = valPtr;
                appDomain = g_CLR_RT_ExecutionEngine.GetCurrentAppDomain();
            }
                            
            TINYCLR_CHECK_HRESULT(m_bf->WriteBits( (CLR_UINT32)(size_t)ptr, 32));
            TINYCLR_CHECK_HRESULT(m_bf->WriteBits( (CLR_UINT32)(size_t)appDomain, 32));
        }

        res = c_Action_None; TINYCLR_SET_AND_LEAVE(S_OK);
    }
#endif

    idx = m_bf->SearchDuplicate( m_value );
    if(idx != (CLR_UINT32)-1)
    {
        //
        // No duplicates allowed for fixed-type objects.
        //
        if((mask & c_Signature_Header) == 0)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_SERIALIZATION_VIOLATION);
        }

        TINYCLR_CHECK_HRESULT(m_bf->WriteBits              ( TE_L1_Duplicate, TE_L1 ));
        TINYCLR_CHECK_HRESULT(m_bf->WriteCompressedUnsigned( idx                    ));

        res = c_Action_None; TINYCLR_SET_AND_LEAVE(S_OK);
    }

    //--//

    TINYCLR_CHECK_HRESULT(EmitSignature_Inner( mask, m_type, m_value ));

    res = c_Action_ObjectData; TINYCLR_SET_AND_LEAVE(S_OK);


    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_BinaryFormatter::TypeHandler::EmitSignature_Inner( int mask, CLR_RT_TypeDescriptor* type, CLR_RT_HeapBlock* value )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    // Unbox reflection types
    if(value && value->DataType() == DATATYPE_OBJECT)
    {
        CLR_RT_HeapBlock* pObj = value->Dereference();
            
        if(pObj && pObj->DataType() == DATATYPE_REFLECTION)
        {
            value = pObj;
        }
    }

    if(value && value->DataType() == DATATYPE_REFLECTION)
    {
        switch(value->ReflectionDataConst().m_kind)
        {
        case REFLECTION_TYPE:
        case REFLECTION_TYPE_DELAYED:                    
            if((mask & c_Signature_Header) != 0)
            {
                TINYCLR_CHECK_HRESULT(m_bf->WriteBits( TE_L1_Other, TE_L1 ));
                TINYCLR_CHECK_HRESULT(m_bf->WriteBits( TE_L2_Other, TE_L2 ));
                TINYCLR_CHECK_HRESULT(m_bf->WriteBits( TE_L3_Type , TE_L3 ));
            }
            break;

#if defined(TINYCLR_APPDOMAINS)
        case REFLECTION_ASSEMBLY:
        case REFLECTION_CONSTRUCTOR:
        case REFLECTION_METHOD:
        case REFLECTION_FIELD:
            if((m_bf->m_flags & CLR_RT_BinaryFormatter::c_Flags_Marshal) == 0) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

            if((mask & c_Signature_Header) != 0)
            {
                TINYCLR_CHECK_HRESULT(m_bf->WriteBits( TE_L1_Other     , TE_L1 ));
                TINYCLR_CHECK_HRESULT(m_bf->WriteBits( TE_L2_Other     , TE_L2 ));
                TINYCLR_CHECK_HRESULT(m_bf->WriteBits( TE_L3_Reflection, TE_L3 ));  
            }
            break;
#endif

        default:
            TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
        }        
    }
    else if(value && (type->m_flags & (CLR_RT_DataTypeLookup::c_Array | CLR_RT_DataTypeLookup::c_ArrayList)))
    {
        CLR_RT_HeapBlock_Array* array;
        int                     sizeReal = -1;

        if(type->m_flags & CLR_RT_DataTypeLookup::c_Array)
        {

            if((mask & c_Signature_Header) != 0)
            {
                TINYCLR_CHECK_HRESULT(m_bf->WriteBits( TE_L1_Other, TE_L1 ));
                TINYCLR_CHECK_HRESULT(m_bf->WriteBits( TE_L2_Array, TE_L2 ));
            }

            if((mask & c_Signature_Type) != 0)
            {
                TINYCLR_CHECK_HRESULT(m_bf->WriteBits( (CLR_UINT32)type->m_reflex.m_levels, TE_ArrayDepth ));

                CLR_RT_TypeDescriptor typeSub; typeSub.InitializeFromType( type->m_reflex.m_data.m_type );

                TINYCLR_CHECK_HRESULT(EmitSignature_Inner( c_Signature_Header | c_Signature_Type, &typeSub, NULL ));
            }


            sizeReal = value->DereferenceArray()->m_numOfElements;
        }
        else
        {
            int capacity;

            if((mask & c_Signature_Header) != 0)
            {
                TINYCLR_CHECK_HRESULT(m_bf->WriteBits( TE_L1_Other    , TE_L1 ));
                TINYCLR_CHECK_HRESULT(m_bf->WriteBits( TE_L2_ArrayList, TE_L2 ));
            }

            TINYCLR_CHECK_HRESULT(CLR_RT_ArrayListHelper::ExtractArrayFromArrayList( *value, array, sizeReal, capacity ));
        }

        if((mask & c_Signature_Length) != 0)
        {
            int bitsMax = m_hints.m_bitPacked;

            if(bitsMax != 0)
            {
                if(sizeReal >= (1 << bitsMax))
                {
                    TINYCLR_SET_AND_LEAVE(CLR_E_SERIALIZATION_VIOLATION);
                }

                TINYCLR_SET_AND_LEAVE(m_bf->WriteBits( (CLR_UINT32)sizeReal, bitsMax ));
            }
            else
            {
                TINYCLR_SET_AND_LEAVE(m_bf->WriteCompressedUnsigned( (CLR_UINT32)sizeReal ));
            }
        }
        else
        {
            int sizeExpected = m_hints.m_arraySize;

            if(sizeExpected > 0 && sizeExpected != sizeReal)
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_SERIALIZATION_VIOLATION);
            }
        }
    }
    else if(type->m_flags == CLR_RT_DataTypeLookup::c_Primitive ||
            type->m_flags == CLR_RT_DataTypeLookup::c_Enum       )
    {
        if((mask & c_Signature_Header) != 0)
        {
            TINYCLR_CHECK_HRESULT(m_bf->WriteBits( TE_L1_Other    , TE_L1 ));
            TINYCLR_CHECK_HRESULT(m_bf->WriteBits( TE_L2_Primitive, TE_L2 ));
        }

        if((mask & c_Signature_Type) != 0)
        {
            TINYCLR_SET_AND_LEAVE(m_bf->WriteBits( CLR_RT_TypeSystem::MapDataTypeToElementType( type->GetDataType() ), TE_ElementType ));
        }
    }
    else
    {
        if((mask & c_Signature_Header) != 0)
        {
            TINYCLR_CHECK_HRESULT(m_bf->WriteBits( TE_L1_Reference, TE_L1 ));
        }

        if((mask & c_Signature_Type) != 0)
        {
            TINYCLR_SET_AND_LEAVE(m_bf->WriteType( type->m_handlerCls.CrossReference().m_hash ));
        }
    }

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT CLR_RT_BinaryFormatter::TypeHandler::ReadSignature( int& res )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    int mask = SignatureRequirements();

    m_value = NULL;
    m_type  = NULL;

    if(m_typeForced)
    {
        m_type = &m_type_tmp;

        *m_type = *m_typeForced;
    }

    if((mask & c_Signature_Header) != 0)
    {
        CLR_UINT32 levelOne;
        CLR_UINT32 levelTwo;
        CLR_UINT32 levelThree;


        TINYCLR_CHECK_HRESULT(m_bf->ReadBits( levelOne, TE_L1 ));

        if(levelOne == TE_L1_Null)
        {
            if(m_hints.m_flags & SF_PointerNeverNull)
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_SERIALIZATION_VIOLATION);
            }

            res = c_Action_None; TINYCLR_SET_AND_LEAVE(S_OK);
        }

        if(levelOne == TE_L1_Duplicate)
        {
            CLR_RT_HeapBlock* dup;
            CLR_UINT32        idx;

            TINYCLR_CHECK_HRESULT(m_bf->ReadCompressedUnsigned( idx ));

            dup = m_bf->GetDuplicate( idx );
            if(dup == NULL)
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_SERIALIZATION_VIOLATION);
            }

            m_value = &m_value_tmp;
            m_value->SetObjectReference( dup );

            m_type = &m_type_tmp;
            TINYCLR_CHECK_HRESULT(m_type->InitializeFromObject( *m_value ));

            res = c_Action_None; TINYCLR_SET_AND_LEAVE(S_OK);
        }

        if(levelOne == TE_L1_Reference)
        {
            if((mask & c_Signature_Type) != 0)
            {
                CLR_RT_ReflectionDef_Index idx;

                TINYCLR_CHECK_HRESULT(m_bf->ReadType( idx ));

                m_type = &m_type_tmp;
                TINYCLR_CHECK_HRESULT(m_type->InitializeFromReflection( idx ));
            }
        }
        else
        {
            TINYCLR_CHECK_HRESULT(m_bf->ReadBits( levelTwo, TE_L2 ));
            if(levelTwo == TE_L2_Primitive)
            {
                if((mask & c_Signature_Type) != 0)
                {
                    CLR_UINT32 et;

                    TINYCLR_CHECK_HRESULT(m_bf->ReadBits( et, TE_ElementType ));

                    m_type = &m_type_tmp;
                    TINYCLR_CHECK_HRESULT(m_type->InitializeFromDataType( CLR_RT_TypeSystem::MapElementTypeToDataType( et ) ));
                }
            }
            else if(levelTwo == TE_L2_Array)
            {
                if((mask & c_Signature_Type) != 0)
                {
                    CLR_UINT32 depth;

                    TINYCLR_CHECK_HRESULT(m_bf->ReadBits( depth, TE_ArrayDepth ));

                    TINYCLR_CHECK_HRESULT(m_bf->ReadBits( levelOne, TE_L1 ));

                    if(levelOne == TE_L1_Reference)
                    {
                        CLR_RT_ReflectionDef_Index idx;

                        TINYCLR_CHECK_HRESULT(m_bf->ReadType( idx ));

                        m_type = &m_type_tmp;
                        TINYCLR_CHECK_HRESULT(m_type->InitializeFromReflection( idx ));
                    }
                    else if(levelOne == TE_L1_Other)
                    {
                        TINYCLR_CHECK_HRESULT(m_bf->ReadBits( levelTwo, TE_L2 ));

                        if(levelTwo == TE_L2_Primitive)
                        {
                            CLR_UINT32 et;

                            TINYCLR_CHECK_HRESULT(m_bf->ReadBits( et, TE_ElementType ));

                            m_type = &m_type_tmp;
                            TINYCLR_CHECK_HRESULT(m_type->InitializeFromDataType( CLR_RT_TypeSystem::MapElementTypeToDataType( et ) ));
                        }
                        else
                        {
                            TINYCLR_SET_AND_LEAVE(CLR_E_SERIALIZATION_BADSTREAM);
                        }
                    }
                    else
                    {
                        TINYCLR_SET_AND_LEAVE(CLR_E_SERIALIZATION_BADSTREAM);
                    }

                    m_type->m_reflex.m_levels = depth;
                    m_type->ConvertToArray();
                }
            }
            else if(levelTwo == TE_L2_ArrayList)
            {
                if((mask & c_Signature_Type) != 0)
                {
                    m_type = &m_type_tmp;
                    TINYCLR_CHECK_HRESULT(m_type->InitializeFromType( g_CLR_RT_WellKnownTypes.m_ArrayList ));
                }
            }
            else if(levelTwo == TE_L2_Other)
            {
                TINYCLR_CHECK_HRESULT(m_bf->ReadBits( levelThree, TE_L3 ));
                if(levelThree == TE_L3_Type)
                {
                    m_type = &m_type_tmp;
                    TINYCLR_CHECK_HRESULT(m_type->InitializeFromType( g_CLR_RT_WellKnownTypes.m_Type ));
                }
#if defined(TINYCLR_APPDOMAINS)
                else if (levelThree == TE_L3_Reflection)
                {
                    CLR_RT_ReflectionDef_Index value;
                    CLR_UINT64                 data;

                    _ASSERTE(m_bf->m_flags & CLR_RT_BinaryFormatter::c_Flags_Marshal);

                    TINYCLR_CHECK_HRESULT(m_bf->ReadBits( data, 64 ));

                    value.SetRawData( data );
                    
                    m_value = &m_value_tmp;
                    TINYCLR_CHECK_HRESULT(m_value->SetReflection( value ));

                    res = c_Action_None; TINYCLR_SET_AND_LEAVE(S_OK);
                }
                else if (levelThree == TE_L3_MBRO)
                {
                    CLR_UINT32 uintPtr;
                    CLR_UINT32 uintAppDomain;
                    CLR_RT_HeapBlock* ptr;
                    CLR_RT_AppDomain* appDomain;                    
     
                    TINYCLR_CHECK_HRESULT(m_bf->ReadBits( uintPtr      , 32 ));
                    TINYCLR_CHECK_HRESULT(m_bf->ReadBits( uintAppDomain, 32 ));

                    _ASSERTE(CLR_EE_DBG_IS( NoCompaction ));

                    ptr       = (CLR_RT_HeapBlock*)(size_t)uintPtr;
                    appDomain = (CLR_RT_AppDomain*)(size_t)uintAppDomain;
                    
                    m_type = &m_type_tmp;
                    TINYCLR_CHECK_HRESULT(m_type->InitializeFromObject( *ptr ));

                    m_value = &m_value_tmp;
                    if(appDomain != g_CLR_RT_ExecutionEngine.GetCurrentAppDomain())
                    {
                        //The MarshalByRefObject lives in a separate AppDomain.
                        //We need to allocate a TRANSPARENT_PROXY object on the stack
                                
                        CLR_RT_HeapBlock* proxy = g_CLR_RT_ExecutionEngine.ExtractHeapBlocksForObjects( DATATYPE_TRANSPARENT_PROXY, 0, 1 ); CHECK_ALLOCATION(proxy);

                        proxy->SetTransparentProxyReference( appDomain, ptr );
                        
                        ptr = proxy;
                    }

                    //Set the value reference to either the MBRO, or the proxy
                    m_value->SetObjectReference( ptr );  

                    res = c_Action_None; TINYCLR_SET_AND_LEAVE(S_OK);                       
                }
#endif
                else
                {
                    TINYCLR_SET_AND_LEAVE(CLR_E_SERIALIZATION_BADSTREAM);
                }
            }
        }
    }

    if(m_type->m_flags & (CLR_RT_DataTypeLookup::c_Array | CLR_RT_DataTypeLookup::c_ArrayList))
    {
        CLR_UINT32 len;

        if(mask & c_Signature_Length)
        {
            int bitsMax = m_hints.m_bitPacked;

            if(bitsMax != 0)
            {
                TINYCLR_CHECK_HRESULT(m_bf->ReadBits( len, bitsMax ));
            }
            else
            {
                TINYCLR_CHECK_HRESULT(m_bf->ReadCompressedUnsigned( len ));
            }
        }
        else
        {
            CLR_RT_TypeDescriptor sub;

            if(m_type->m_reflex.m_levels != 1)
            {
                //
                // Only simple arrays can have variable size.
                //
                TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
            }

            sub.InitializeFromType( m_type->m_reflex.m_data.m_type );

            len = m_hints.m_arraySize;

            if(len == (CLR_UINT32)-1)
            {
                len = m_bf->BitsAvailable();

                switch(TypeHandler::GetSizeOfType( &sub ))
                {
                case  1:            break;
                case  8: len /=  8; break;
                case 16: len /= 16; break;
                case 32: len /= 32; break;
                case 64: len /= 64; break;
                default:
                    TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                }
            }
        }

        if(m_type->m_flags & CLR_RT_DataTypeLookup::c_ArrayList)
        {
            CLR_RT_HeapBlock_Array* array;

            m_value = &m_value_tmp;
            TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewArrayList( *m_value, len, array ));
        }
        else
        {
            m_value = &m_value_tmp;
             TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( *m_value, len, m_type->m_reflex ));
        }
    }
    else
    {
        m_value = &m_value_tmp;

        if(m_type->m_handlerCls.IsATypeHandler())
        {
            CLR_RT_ReflectionDef_Index idx;

            TINYCLR_CHECK_HRESULT(m_bf->ReadType( idx ));

            TINYCLR_CHECK_HRESULT(m_value->SetReflection( idx ));

            res = c_Action_None; TINYCLR_SET_AND_LEAVE(S_OK);
        }

        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObject( *m_value, m_type->m_handlerCls ));
    }

    res = c_Action_ObjectData; TINYCLR_SET_AND_LEAVE(S_OK);

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT CLR_RT_BinaryFormatter::TypeHandler::EmitValue( int& res )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* value = m_value->FixBoxingReference();
    CLR_UINT64        val;
    CLR_UINT32        bits;
    bool              fSigned;

    // unbox reflection types
    if(value->DataType() == DATATYPE_OBJECT)
    {
        CLR_RT_HeapBlock* obj = value->Dereference();
        
        if(obj && obj->DataType() == DATATYPE_REFLECTION)
        {
            value = obj;
        }
    }

    if(value->DataType() == DATATYPE_REFLECTION)
    {
        switch(value->ReflectionDataConst().m_kind)
        {
        case REFLECTION_TYPE:
        case REFLECTION_TYPE_DELAYED:
            TINYCLR_CHECK_HRESULT(m_bf->WriteType( value->ReflectionDataConst() ));
            break;        
#if defined(TINYCLR_APPDOMAINS)
        case REFLECTION_ASSEMBLY:
        case REFLECTION_CONSTRUCTOR:
        case REFLECTION_METHOD:
        case REFLECTION_FIELD:
            {   
                CLR_UINT64 data;

                _ASSERTE(m_bf->m_flags & CLR_RT_BinaryFormatter::c_Flags_Marshal);

                data = value->ReflectionDataConst().GetRawData();

                TINYCLR_CHECK_HRESULT(m_bf->WriteBits( data, 64 ));
            }
            break;            
#endif                    
        }

        res = c_Action_None; TINYCLR_SET_AND_LEAVE(S_OK);
    }

    if(m_type->m_handlerCls.m_data == g_CLR_RT_WellKnownTypes.m_DateTime.m_data)
    {
        CLR_INT64* pVal = Library_corlib_native_System_DateTime::GetValuePtr( *value ); FAULT_ON_NULL(pVal);

        val     = *(CLR_UINT64*)pVal;
        bits    = 64;
        fSigned = false;
    }
    else if(m_type->m_handlerCls.m_data == g_CLR_RT_WellKnownTypes.m_TimeSpan.m_data)
    {
        CLR_INT64* pVal = Library_corlib_native_System_TimeSpan::GetValuePtr( *value ); FAULT_ON_NULL(pVal);

        val     = *(CLR_UINT64*)pVal;
        bits    = 64;
        fSigned = true;
    }
    else if(m_type->m_flags == CLR_RT_DataTypeLookup::c_Primitive ||
            m_type->m_flags == CLR_RT_DataTypeLookup::c_Enum       )
    {
        bits = TypeHandler::GetSizeOfType( m_type );
        if(bits == CLR_RT_DataTypeLookup::c_VariableSize)
        {
            LPCSTR     szText = value->RecoverString();
            CLR_UINT32 len    = szText ? (CLR_UINT32)hal_strlen_s( szText ) : 0xFFFFFFFF;

            TINYCLR_CHECK_HRESULT(m_bf->WriteCompressedUnsigned( len ));

            if(len != (CLR_UINT32)-1)
            {
                TINYCLR_CHECK_HRESULT(m_bf->WriteArray( (CLR_UINT8*)szText, len ));
            }

            res = c_Action_None; TINYCLR_SET_AND_LEAVE(S_OK);
        }
        else if(bits == CLR_RT_DataTypeLookup::c_NA)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
        }

        CLR_RT_HeapBlock* v = FixDereference( value ); FAULT_ON_NULL(v);

        val = v->NumericByRefConst().u8;

        fSigned = TypeHandler::GetSignOfType( m_type );
    }

#if defined(TINYCLR_APPDOMAINS)
    else if(m_fIsMarshalByRefObject)
    {
        CLR_RT_HeapBlock* valPtr    = value->Dereference();
        CLR_RT_AppDomain* appDomain = g_CLR_RT_ExecutionEngine.GetCurrentAppDomain();
                
        if(valPtr->DataType() == DATATYPE_TRANSPARENT_PROXY)
        {
            appDomain = valPtr->TransparentProxyAppDomain();
            valPtr    = valPtr->TransparentProxyDereference();
        }

        TINYCLR_CHECK_HRESULT(m_bf->WriteBits( (CLR_UINT32)(size_t)valPtr   , 32 ));
        TINYCLR_CHECK_HRESULT(m_bf->WriteBits( (CLR_UINT32)(size_t)appDomain, 32 ));

        TINYCLR_SET_AND_LEAVE(S_OK);
    }
#endif

    else
    {
        TINYCLR_SET_AND_LEAVE(TrackObject( res ));
    }

    {
        CLR_UINT32 shift = 64 - bits;

        if(shift)
        {
            val <<= shift;

            if(fSigned) val = (CLR_UINT64)((CLR_INT64 )val >> shift);
            else        val = (CLR_UINT64)((CLR_UINT64)val >> shift);
        }
    }

    if(m_hints.m_bitPacked) bits = m_hints.m_bitPacked;

    val -= m_hints.m_rangeBias;

    if(fSigned)
    {
        CLR_INT64 valS = (CLR_INT64)val;

        if(m_hints.m_scale != 0) valS /= (CLR_INT64)m_hints.m_scale;

        if(bits != 64)
        {
            CLR_INT64 maxVal = (LONGLONGCONSTANT(1) << (bits-1)) - 1;

            if(valS < (-maxVal-1) || valS > maxVal)
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_SERIALIZATION_VIOLATION);
            }
        }

        val = (CLR_UINT64)valS;
    }
    else
    {
        CLR_UINT64 valU = (CLR_UINT64)val;

        if(m_hints.m_scale != 0) valU /= (CLR_UINT64)m_hints.m_scale;

        if(bits != 64)
        {
            CLR_UINT64 maxVal = (ULONGLONGCONSTANT(1) << bits) - 1;

            if(valU > maxVal)
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_SERIALIZATION_VIOLATION);
            }
        }

        val = (CLR_UINT64)valU;
    }

    TINYCLR_CHECK_HRESULT(m_bf->WriteBits( val, bits ));

    res = c_Action_None; TINYCLR_SET_AND_LEAVE(S_OK);

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_BinaryFormatter::TypeHandler::ReadValue( int& res )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    CLR_UINT64* dst;
    CLR_UINT64  val;
    CLR_UINT32  bits;
    bool        fSigned;

    if(m_type->m_handlerCls.m_data == g_CLR_RT_WellKnownTypes.m_DateTime.m_data)
    {
        CLR_INT64* pVal = Library_corlib_native_System_DateTime::GetValuePtr( *m_value ); FAULT_ON_NULL(pVal);

        dst     = (CLR_UINT64*)pVal;
        bits    = 64;
        fSigned = false;
    }
    else if(m_type->m_handlerCls.m_data == g_CLR_RT_WellKnownTypes.m_TimeSpan.m_data)
    {
        CLR_INT64* pVal = Library_corlib_native_System_TimeSpan::GetValuePtr( *m_value ); FAULT_ON_NULL(pVal);

        dst     = (CLR_UINT64*)pVal;
        bits    = 64;
        fSigned = true;
    }
    else if(m_type->m_flags == CLR_RT_DataTypeLookup::c_Primitive ||
            m_type->m_flags == CLR_RT_DataTypeLookup::c_Enum       )
    {
        bits = TypeHandler::GetSizeOfType( m_type );

        if(bits == CLR_RT_DataTypeLookup::c_VariableSize)
        {
            CLR_UINT32 len;

            TINYCLR_CHECK_HRESULT(m_bf->ReadCompressedUnsigned( len ));

            if(len == 0xFFFFFFFF)
            {
                m_value->SetObjectReference( NULL );
            }
            else
            {
                CLR_RT_HeapBlock_String* str = CLR_RT_HeapBlock_String::CreateInstance( *m_value, len ); CHECK_ALLOCATION(str);
                LPSTR                    dst = (LPSTR)str->StringText();

                TINYCLR_CHECK_HRESULT(m_bf->ReadArray( (CLR_UINT8*)dst, len ));
                dst[ len ] = 0;
            }

            res = c_Action_None; TINYCLR_SET_AND_LEAVE(S_OK);
        }
        else if(bits == CLR_RT_DataTypeLookup::c_NA)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
        }
        else
        {
            m_value->SetDataId( CLR_RT_HEAPBLOCK_RAW_ID(m_type->GetDataType(),0,1) );
            m_value->ClearData(                                                    );

            dst = (CLR_UINT64*)&m_value->NumericByRef().u8;

            fSigned = TypeHandler::GetSignOfType( m_type );
        }
    }
    else
    {
        TINYCLR_SET_AND_LEAVE(TrackObject( res ));
    }

    if(m_hints.m_bitPacked) bits = m_hints.m_bitPacked;

    TINYCLR_CHECK_HRESULT(m_bf->ReadBits( val, bits ));

    if(bits != 64)
    {
        int revBits = 64 - bits;

        val <<= revBits;

        if(fSigned) val = (CLR_UINT64)((CLR_INT64)val >> revBits);
        else        val =             (           val >> revBits);
    }

    if(m_hints.m_scale != 0)
    {
        val *= m_hints.m_scale;
    }

    *dst = val + m_hints.m_rangeBias;

    res = c_Action_None; TINYCLR_SET_AND_LEAVE(S_OK);

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT CLR_RT_BinaryFormatter::TypeHandler::TrackObject( int& res )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    if(m_type->m_flags & (CLR_RT_DataTypeLookup::c_Array | CLR_RT_DataTypeLookup::c_ArrayList))
    {
        res = c_Action_ObjectElements;
    }
    else
    {
        res = c_Action_ObjectFields;

        if(m_typeExpected && m_typeExpected->m_flags == CLR_RT_DataTypeLookup::c_ValueType)
        {
            TINYCLR_SET_AND_LEAVE(S_OK);
        }
    }

    TINYCLR_SET_AND_LEAVE(m_bf->TrackDuplicate( m_value ));

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_RT_BinaryFormatter::State::CreateInstance( CLR_RT_BinaryFormatter* parent, SerializationHintsAttribute* hints, CLR_RT_HeapBlock* type )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    CLR_RT_TypeDef_Instance     inst;
    CLR_RT_TypeDescriptor       desc;
    CLR_RT_TypeDescriptor*      pDesc;
    SerializationHintsAttribute hintsTmp;

    if(type && CLR_RT_ReflectionDef_Index::Convert( *type, inst, NULL ))
    {
        TINYCLR_CHECK_HRESULT(desc.InitializeFromType( inst ));

        pDesc = &desc;

        if(hints == NULL)
        {
            hints = &hintsTmp;

            TINYCLR_CLEAR(*hints);

            hints->m_flags = (SerializationFlags)(SF_PointerNeverNull | SF_FixedType);
        }
    }
    else
    {
        pDesc = NULL;
    }

    TINYCLR_SET_AND_LEAVE(CreateInstance( parent, hints, pDesc ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_BinaryFormatter::State::CreateInstance( CLR_RT_BinaryFormatter* parent, SerializationHintsAttribute* hints, CLR_RT_TypeDescriptor* type )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    State* ptr = EVENTCACHE_EXTRACT_NODE_INITTOZERO(g_CLR_RT_EventCache,State,DATATYPE_SERIALIZER_STATE);

    CHECK_ALLOCATION(ptr);

    parent->m_states.LinkAtBack( ptr );

    ptr->m_parent               = parent;
    ptr->m_value_NeedProcessing = true;
    ptr->m_value.TypeHandler_Initialize( parent, hints, type );

    TINYCLR_NOCLEANUP();
}

void CLR_RT_BinaryFormatter::State::DestroyInstance()
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    g_CLR_RT_EventCache.Append_Node( this );
}

//--//

HRESULT CLR_RT_BinaryFormatter::State::FindHints( SerializationHintsAttribute& hints, const CLR_RT_TypeDef_Instance& cls )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    TINYCLR_CLEAR(hints);

    if(cls.m_target->flags & CLR_RECORD_TYPEDEF::TD_HasAttributes)
    {
        CLR_RT_TypeDef_Instance    inst; inst.InitializeFromIndex( g_CLR_RT_WellKnownTypes.m_SerializationHintsAttribute );
        CLR_RT_AttributeEnumerator en;   en.Initialize( cls );

        if(en.MatchNext( &inst, NULL ))
        {
            CLR_RT_AttributeParser parser;

            TINYCLR_CHECK_HRESULT(parser.Initialize( en ));

            while(true)
            {
                CLR_RT_AttributeParser::Value* val;

                TINYCLR_CHECK_HRESULT(parser.Next( val ));

                if(val == NULL) break;
            }
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_BinaryFormatter::State::FindHints( SerializationHintsAttribute& hints, const CLR_RT_FieldDef_Instance& fld )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    TINYCLR_CLEAR(hints);

    if(fld.m_target->flags & CLR_RECORD_FIELDDEF::FD_HasAttributes)
    {
        CLR_RT_TypeDef_Instance    inst; inst.InitializeFromIndex( g_CLR_RT_WellKnownTypes.m_SerializationHintsAttribute );
        CLR_RT_AttributeEnumerator en;   en.Initialize( fld );

        if(en.MatchNext( &inst, NULL ))
        {
            CLR_RT_AttributeParser parser;

            TINYCLR_CHECK_HRESULT(parser.Initialize( en ));

            while(true)
            {
                CLR_RT_AttributeParser::Value* val;

                TINYCLR_CHECK_HRESULT(parser.Next( val ));

                if(val == NULL)
                {
                    break;
                }
                else
                {
                    if     (!strcmp( val->m_name, "Flags"     )) hints.m_flags     = (SerializationFlags)val->m_value.NumericByRef().u4;
                    else if(!strcmp( val->m_name, "ArraySize" )) hints.m_arraySize =                     val->m_value.NumericByRef().s4;
                    else if(!strcmp( val->m_name, "BitPacked" )) hints.m_bitPacked =                     val->m_value.NumericByRef().s4;
                    else if(!strcmp( val->m_name, "RangeBias" )) hints.m_rangeBias =                     val->m_value.NumericByRef().s8;
                    else if(!strcmp( val->m_name, "Scale"     )) hints.m_scale     =                     val->m_value.NumericByRef().u8;
                }
            }
        }
    }

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT CLR_RT_BinaryFormatter::State::AssignAndFixBoxing( CLR_RT_HeapBlock& dst )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* src               = m_value.m_value;
    CLR_DataType      dt                = dst.DataType();
    CLR_DataType      dt2               = (dt == DATATYPE_ARRAY_BYREF) ? (CLR_DataType)dst.DereferenceArray()->m_typeOfElement : dt;
    
    if(c_CLR_RT_DataTypeLookup[ dt2 ].m_flags & CLR_RT_DataTypeLookup::c_OptimizedValueType)
    {
        if(src && (c_CLR_RT_DataTypeLookup[ src->DataType() ].m_flags & CLR_RT_DataTypeLookup::c_OptimizedValueType) == 0)
        {
            src = TypeHandler::FixNull( src );
        }

        if(src == NULL || (c_CLR_RT_DataTypeLookup[ src->DataType() ].m_flags & CLR_RT_DataTypeLookup::c_OptimizedValueType) == 0)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
        }
    }
    else
    {
        if(src)
        {  
            bool fBox = false;

            if(c_CLR_RT_DataTypeLookup[ src->DataType() ].m_flags & CLR_RT_DataTypeLookup::c_OptimizedValueType)
            {
                fBox = true;
            }
            else if(src->IsAValueType())
            {
                if(dt == DATATYPE_ARRAY_BYREF)
                {
                    fBox = (dt2 != DATATYPE_VALUETYPE);
                }
                else
                {
                    fBox = !dst.IsAValueType();
                }
            }
            //
            // Box reflection types
            //
            else if(src->DataType() == DATATYPE_REFLECTION)
            {
                const CLR_RT_ReflectionDef_Index* reflex;
                const CLR_RT_TypeDef_Index* cls;
                CLR_RT_HeapBlock *pDst = &dst;

                reflex = &(src->ReflectionDataConst());
                
                switch(reflex->m_kind)
                {
                case REFLECTION_ASSEMBLY    : cls = &g_CLR_RT_WellKnownTypes.m_Assembly       ; break;
                case REFLECTION_TYPE        : cls = &g_CLR_RT_WellKnownTypes.m_Type           ; break;
                case REFLECTION_TYPE_DELAYED: cls = &g_CLR_RT_WellKnownTypes.m_Type           ; break;
                case REFLECTION_CONSTRUCTOR : cls = &g_CLR_RT_WellKnownTypes.m_ConstructorInfo; break;
                case REFLECTION_METHOD      : cls = &g_CLR_RT_WellKnownTypes.m_MethodInfo     ; break;
                case REFLECTION_FIELD       : cls = &g_CLR_RT_WellKnownTypes.m_FieldInfo      ; break;
                }

                // 
                // Dereference array element and create boxed reflection type
                // 
                if(dt == DATATYPE_ARRAY_BYREF)
                {
                    CLR_RT_HeapBlock_Array* array = dst.DereferenceArray(); FAULT_ON_NULL(array);

                    pDst = (CLR_RT_HeapBlock*)array->GetElement( dst.ArrayIndex() );
                }

                //
                // Create boxed object reference for reflection type
                //
                g_CLR_RT_ExecutionEngine.NewObjectFromIndex(*pDst, *cls);


                //
                // Set reflection type
                //
                pDst->Dereference()->SetReflection(*reflex);

                TINYCLR_SET_AND_LEAVE(S_OK);
            }

            if(fBox)
            {
                TINYCLR_CHECK_HRESULT(m_value.m_value->PerformBoxing( m_value.m_type->m_handlerCls ));
            }
        }
        else
        {
            src = &m_value.m_value_tmp; src->SetObjectReference( NULL );
        }
    }

    if(dt == DATATYPE_ARRAY_BYREF)
    {
        TINYCLR_CHECK_HRESULT(src->StoreToReference( dst, 0 ));
    }
    else
    {
        dst.Assign( *src );
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_BinaryFormatter::State::GetValue()
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    State* prev = (State*)Prev();
    if(prev->Prev() == NULL)
    {
        TINYCLR_SET_AND_LEAVE(m_value.SetValue( &m_parent->m_value ));
    }

    if(prev->m_fields_NeedProcessing)
    {
        TINYCLR_SET_AND_LEAVE(m_value.SetValue( prev->m_fields_Pointer ));
    }

    if(prev->m_array_NeedProcessing)
    {
        CLR_RT_HeapBlock ref; ref.InitializeArrayReferenceDirect( *prev->m_array, prev->m_array_CurrentPos-1 );
        CLR_RT_HeapBlock val;

        TINYCLR_CHECK_HRESULT(val.LoadFromReference( ref ));

        TINYCLR_SET_AND_LEAVE(m_value.SetValue( &val ));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_BinaryFormatter::State::SetValueAndDestroyInstance()
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    if(m_parent->m_fDeserialize)
    {
        State* prev = (State*)Prev();

        if(prev->Prev() == NULL)
        {
            TINYCLR_CHECK_HRESULT(AssignAndFixBoxing( m_parent->m_value ));
        }
        else
        {
            if(prev->m_fields_NeedProcessing)
            {
                TINYCLR_CHECK_HRESULT(AssignAndFixBoxing( *prev->m_fields_Pointer ));
            }

            if(prev->m_array_NeedProcessing)
            {
                CLR_RT_HeapBlock ref; ref.InitializeArrayReferenceDirect( *prev->m_array, prev->m_array_CurrentPos-1 );

                TINYCLR_CHECK_HRESULT(AssignAndFixBoxing( ref ));
            }
        }
    }

    DestroyInstance();

    TINYCLR_NOCLEANUP();
}

//////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_RT_BinaryFormatter::State::Advance()
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    if(m_value_NeedProcessing)
    {
        int res;

        m_value_NeedProcessing = false;

        if(m_parent->m_fDeserialize)
        {
            TINYCLR_CHECK_HRESULT(m_value.ReadSignature( res ));

#if defined(TINYCLR_APPDOMAINS)
            //Make sure that the type being deserialized is loaded in the current appDomain
            {
                if(m_parent->m_flags & CLR_RT_BinaryFormatter::c_Flags_Marshal)
                {
                    CLR_RT_TypeDef_Index idx; idx.Clear();
                    CLR_RT_HeapBlock* value = m_value.m_value;

                    if(value && value->DataType() == DATATYPE_REFLECTION)
                    {                                        
                        switch(value->ReflectionDataConst().m_kind)
                        {
                            case REFLECTION_TYPE:
                                idx.m_data = value->ReflectionDataConst().m_data.m_type.m_data;
                                break;
                            case REFLECTION_TYPE_DELAYED:                                
                                //should this be allowed for appdomain+marshaling???
                                break;                        
                            case REFLECTION_ASSEMBLY:
                                {
                                    CLR_RT_Assembly_Instance inst; 
                                    
                                    if(!inst.InitializeFromIndex( value->ReflectionDataConst().m_data.m_assm ))
                                    {
                                        _ASSERTE(FALSE);
                                    }
                                    
                                    if(!g_CLR_RT_ExecutionEngine.GetCurrentAppDomain()->FindAppDomainAssembly( inst.m_assm )) 
                                    {
                                        TINYCLR_SET_AND_LEAVE(CLR_E_APPDOMAIN_MARSHAL_EXCEPTION);
                                    }                                
                                }
                                break;
                            case REFLECTION_CONSTRUCTOR:     
                            case REFLECTION_METHOD:
                                {
                                    CLR_RT_MethodDef_Instance inst;

                                    if(!inst.InitializeFromIndex( value->ReflectionDataConst().m_data.m_method ))
                                    {
                                        _ASSERTE(FALSE);
                                    }

                                    idx.Set( inst.Assembly(), inst.CrossReference().GetOwner() );
                                }
                                break;
                            case REFLECTION_FIELD:
                                {
                                    CLR_RT_FieldDef_Instance inst;
                                    CLR_RT_TypeDescriptor desc;
                                                                    
                                    if(!inst.InitializeFromIndex( value->ReflectionDataConst().m_data.m_field ))
                                    {
                                        _ASSERTE(FALSE);
                                    }

                                    TINYCLR_CHECK_HRESULT(desc.InitializeFromFieldDefinition( inst ));
                                    
                                    idx = desc.m_handlerCls;
                                }
                                break;
                        }
                    }
                    else if(m_value.m_type)
                    {
                        idx = m_value.m_type->m_handlerCls;                    
                    }

                    if(idx.m_data != 0)
                    {
                        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.GetCurrentAppDomain()->VerifyTypeIsLoaded( idx ));
                    }
                }
            }
#endif

        }
        else
        {
            TINYCLR_CHECK_HRESULT(GetValue());

            TINYCLR_CHECK_HRESULT(m_value.EmitSignature( res ));
        }

        if(res != TypeHandler::c_Action_None)
        {
            if(m_parent->m_fDeserialize)
            {
                TINYCLR_CHECK_HRESULT(m_value.ReadValue( res ));

            }
            else
            {
                TINYCLR_CHECK_HRESULT(m_value.EmitValue( res ));
            }

            switch(res)
            {
            case TypeHandler::c_Action_None:
                break;

            case TypeHandler::c_Action_ObjectFields:
                {
                    m_fields_NeedProcessing = true;
                    m_fields_CurrentClass   = m_value.m_type->m_handlerCls;
                    m_fields_CurrentField   = 0;
                    m_fields_Pointer        = NULL;
                    break;
                }

            case TypeHandler::c_Action_ObjectElements:
                {
                    m_array_NeedProcessing = true;
                    m_array_CurrentPos     = 0;

                    if(m_value.m_type->m_flags & CLR_RT_DataTypeLookup::c_ArrayList)
                    {
                        int capacity;

                        TINYCLR_CHECK_HRESULT(CLR_RT_ArrayListHelper::ExtractArrayFromArrayList( *m_value.m_value, m_array, m_array_LastPos, capacity ));

                        m_array_ExpectedType = NULL;
                    }
                    else
                    {
                        m_array         = m_value.m_value->DereferenceArray();
                        m_array_LastPos = m_array->m_numOfElements;

                        m_array_ExpectedType = &m_array_ExpectedType_Tmp;
                        m_value.m_type->GetElementType( *m_array_ExpectedType );
                    }

                    break;
                }
            }
        }
    }

    if(m_fields_NeedProcessing)
    {
        TINYCLR_SET_AND_LEAVE(AdvanceToTheNextField());
    }

    if(m_array_NeedProcessing)
    {
        TINYCLR_SET_AND_LEAVE(AdvanceToTheNextElement());
    }

    TINYCLR_SET_AND_LEAVE(SetValueAndDestroyInstance());


    TINYCLR_NOCLEANUP();
}

//--//

HRESULT CLR_RT_BinaryFormatter::State::AdvanceToTheNextField()
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    while(TINYCLR_INDEX_IS_VALID(m_fields_CurrentClass))
    {
        if(m_fields_CurrentField < m_fields_CurrentClass.m_target->iFields_Num)
        {
            int                      offset = m_fields_CurrentClass.m_target->iFields_First + m_fields_CurrentField++;
            CLR_RT_FieldDef_Index    idx ; idx.Set( m_fields_CurrentClass.Assembly(), offset );
            CLR_RT_FieldDef_Instance inst; inst.InitializeFromIndex( idx );

            m_fields_Pointer = m_value.m_value->Dereference() + inst.CrossReference().m_offset;

            if((inst.m_target->flags & CLR_RECORD_FIELDDEF::FD_NotSerialized) == 0)
            {
                SerializationHintsAttribute hints;
                CLR_RT_TypeDescriptor       desc;

                if(m_value.m_type->m_flags & CLR_RT_DataTypeLookup::c_Enum)
                {
                    hints = m_value.m_hints;
                }
                else
                {
                    TINYCLR_CHECK_HRESULT(FindHints( hints, inst ));
                }

                TINYCLR_CHECK_HRESULT(desc.InitializeFromFieldDefinition( inst ));

                TINYCLR_SET_AND_LEAVE(State::CreateInstance( m_parent, &hints, &desc ));
            }
        }
        else
        {
            m_fields_CurrentClass.SwitchToParent();
            m_fields_CurrentField = 0;
        }
    }

    TINYCLR_SET_AND_LEAVE(SetValueAndDestroyInstance());

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT CLR_RT_BinaryFormatter::State::AdvanceToTheNextElement()
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    if(m_array_ExpectedType && (c_CLR_RT_DataTypeLookup[ m_array->m_typeOfElement ].m_flags & CLR_RT_DataTypeLookup::c_Numeric))
    {
        CLR_UINT8* ptr          = m_array->GetFirstElement();
        CLR_UINT32 bits         = TypeHandler::GetSizeOfType( m_array_ExpectedType );
        int        count        = m_array_LastPos;
        bool       fDeserialize = m_parent->m_fDeserialize;
        CLR_UINT32 size;
        CLR_UINT64 val;

        size = bits / 8; if(size == 0) size = 1;

        while(count > 0)
        {
            if(fDeserialize)
            {
                TINYCLR_CHECK_HRESULT(m_parent->ReadBits( val, bits ));

#if !defined(NETMF_TARGET_BIG_ENDIAN)
                memcpy( ptr, &val, size );
#else
                memcpy( ptr, ((unsigned char*)&val)+(sizeof(val)-size), size );
#endif
            }
            else
            {
                val = 0; memcpy( &val, ptr, size );

                TINYCLR_CHECK_HRESULT(m_parent->WriteBits( val, bits ));
            }

            ptr += size;

            count--;
        }
    }
    else if(m_array_CurrentPos < m_array_LastPos)
    {
        m_array_CurrentPos++;

        SerializationHintsAttribute* hints;

        if(m_value.m_hints.m_flags & (SF_FixedType | SF_PointerNeverNull))
        {
            hints = &m_value.m_hints;
        }
        else
        {
            hints = NULL;
        }

        TINYCLR_SET_AND_LEAVE(State::CreateInstance( m_parent, hints, m_array_ExpectedType ));
    }

    TINYCLR_SET_AND_LEAVE(SetValueAndDestroyInstance());

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_RT_BinaryFormatter::CreateInstance( CLR_UINT8* buf, int len, CLR_RT_BinaryFormatter*& res )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    CLR_RT_BinaryFormatter* ptr = EVENTCACHE_EXTRACT_NODE(g_CLR_RT_EventCache,CLR_RT_BinaryFormatter,DATATYPE_SERIALIZER_HEAD);

    res = ptr; CHECK_ALLOCATION(ptr);

    ptr->m_stream       = NULL;                      // CLR_RT_HeapBlock_MemoryStream* m_stream;
    ptr->m_idx          = 0;                         // CLR_UINT32                     m_idx;
    ptr->m_lastTypeRead = 0;                         // CLR_UINT32                     m_lastTypeRead;
    ptr->m_duplicates.DblLinkedList_Initialize();    // CLR_RT_DblLinkedList           m_duplicates;            // EVENT HEAP - NO RELOCATION - list of CLR_RT_BinaryFormatter::DuplicateTracker
    ptr->m_states    .DblLinkedList_Initialize();    // CLR_RT_DblLinkedList           m_states;                // EVENT HEAP - NO RELOCATION - list of CLR_RT_BinaryFormatter::State
                                                     //
    ptr->m_fDeserialize = (buf != NULL);             // bool                           m_fDeserialize;
    ptr->m_value.SetObjectReference( NULL );         // CLR_RT_HeapBlock               m_value;
    ptr->m_value_desc.TypeDescriptor_Initialize();   // CLR_RT_TypeDescriptor          m_value_desc;


    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_MemoryStream::CreateInstance( ptr->m_stream, buf, len ));

    TINYCLR_NOCLEANUP();
}

void CLR_RT_BinaryFormatter::DestroyInstance()
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    m_states    .DblLinkedList_PushToCache();
    m_duplicates.DblLinkedList_PushToCache();

    CLR_RT_HeapBlock_MemoryStream::DeleteInstance( m_stream );

    g_CLR_RT_EventCache.Append_Node( this );
}

HRESULT CLR_RT_BinaryFormatter::Advance()
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    while(true)
    {
        State* top = (State*)m_states.LastNode(); if(top->Prev() == NULL) break;

        TINYCLR_CHECK_HRESULT(top->Advance());
    }

    TINYCLR_NOCLEANUP();
}

//--//

void CLR_RT_BinaryFormatter::PrepareForGC( void* data )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    if(data != NULL)
    {
        CLR_RT_BinaryFormatter* bf = (CLR_RT_BinaryFormatter*)data;

        g_CLR_RT_GarbageCollector.CheckSingleBlock_Force( &bf->m_value );

        TINYCLR_FOREACH_NODE(CLR_RT_BinaryFormatter::State,state,bf->m_states)
        {
            g_CLR_RT_GarbageCollector.CheckSingleBlock_Force( state->m_value.m_value );
        }
        TINYCLR_FOREACH_NODE_END();
    }
}

HRESULT CLR_RT_BinaryFormatter::Serialize( CLR_RT_HeapBlock& refData, CLR_RT_HeapBlock& object, CLR_RT_HeapBlock* cls, CLR_UINT32 flags )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    CLR_RT_BinaryFormatter* bf = NULL;
    CLR_RT_ProtectFromGC    pgc( (void**)&bf, CLR_RT_BinaryFormatter::PrepareForGC );

    refData.SetObjectReference( NULL );

    TINYCLR_CHECK_HRESULT(CLR_RT_BinaryFormatter::CreateInstance( NULL, 0, bf ));

    TINYCLR_CHECK_HRESULT(State::CreateInstance( bf, NULL, cls ));

    bf->m_flags = flags;
    bf->m_value.Assign( object );        

    TINYCLR_CHECK_HRESULT(bf->Advance());

    TINYCLR_CHECK_HRESULT(bf->m_stream->ToArray( refData ));

    TINYCLR_CLEANUP();

    if(bf)
    {
        bf->DestroyInstance();

        bf = NULL;
    }

    TINYCLR_CLEANUP_END();
}

HRESULT CLR_RT_BinaryFormatter::Deserialize( CLR_RT_HeapBlock& refData, CLR_RT_HeapBlock& object, CLR_RT_HeapBlock* cls, CLR_UINT32* unknownType, CLR_UINT32 flags )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* array = object.DereferenceArray();

    refData.SetObjectReference( NULL );

    if(array != NULL)
    {
        TINYCLR_SET_AND_LEAVE(CLR_RT_BinaryFormatter::Deserialize( refData, array->GetFirstElement(), array->m_numOfElements, cls, unknownType, flags ));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_BinaryFormatter::Deserialize( CLR_RT_HeapBlock& refData, CLR_UINT8* data, CLR_UINT32 size, CLR_RT_HeapBlock* cls, CLR_UINT32* unknownType, CLR_UINT32 flags )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    CLR_RT_BinaryFormatter* bf    = NULL;
    CLR_RT_ProtectFromGC    pgc( (void**)&bf, CLR_RT_BinaryFormatter::PrepareForGC );

    refData.SetObjectReference( NULL );

    TINYCLR_CHECK_HRESULT(CLR_RT_BinaryFormatter::CreateInstance( data, size, bf ));

    TINYCLR_CHECK_HRESULT(State::CreateInstance( bf, NULL, cls ));

    bf->m_flags = flags;

    TINYCLR_CHECK_HRESULT(bf->Advance());

    refData.Assign( bf->m_value );


    TINYCLR_CLEANUP();

    if(bf)
    {
        if(unknownType)
        {
            *unknownType = (hr == CLR_E_WRONG_TYPE) ? bf->m_lastTypeRead : 0;
        }

        bf->DestroyInstance();

        bf = NULL;
    }

    TINYCLR_CLEANUP_END();
}

//--//

HRESULT CLR_RT_BinaryFormatter::TrackDuplicate( CLR_RT_HeapBlock* object )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    DuplicateTracker* ptr = EVENTCACHE_EXTRACT_NODE(g_CLR_RT_EventCache,DuplicateTracker,DATATYPE_SERIALIZER_DUPLICATE);

    CHECK_ALLOCATION(ptr);

    m_duplicates.LinkAtBack( ptr );

    ptr->m_ptr = TypeHandler::FixDereference( object );
    ptr->m_idx = m_idx++;

    TINYCLR_NOCLEANUP();
}

CLR_UINT32 CLR_RT_BinaryFormatter::SearchDuplicate( CLR_RT_HeapBlock* object )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    object = TypeHandler::FixDereference( object );

    TINYCLR_FOREACH_NODE(DuplicateTracker,ptr,m_duplicates)
    {
        if(ptr->m_ptr == object)
        {
            return ptr->m_idx;
        }
    }
    TINYCLR_FOREACH_NODE_END();

    return (CLR_UINT32)-1;
}

CLR_RT_HeapBlock* CLR_RT_BinaryFormatter::GetDuplicate( CLR_UINT32 idx )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FOREACH_NODE(DuplicateTracker,ptr,m_duplicates)
    {
        if(ptr->m_idx == idx)
        {
            return ptr->m_ptr;
        }
    }
    TINYCLR_FOREACH_NODE_END();

    return NULL;
}

//--//--//

int     CLR_RT_BinaryFormatter::BitsAvailable(                                  ) { return m_stream->BitsAvailable(            ); }

HRESULT CLR_RT_BinaryFormatter::ReadBits     (       CLR_UINT32& res, int bits  ) { return m_stream->ReadBits     ( res, bits  ); }
HRESULT CLR_RT_BinaryFormatter::WriteBits    (       CLR_UINT32  val, int bits  ) { return m_stream->WriteBits    ( val, bits  ); }

HRESULT CLR_RT_BinaryFormatter::ReadBits     (       CLR_UINT64& res, int bits  ) { return m_stream->ReadBits     ( res, bits  ); }
HRESULT CLR_RT_BinaryFormatter::WriteBits    (       CLR_UINT64  val, int bits  ) { return m_stream->WriteBits    ( val, bits  ); }

HRESULT CLR_RT_BinaryFormatter::ReadArray    (       CLR_UINT8*  buf, int bytes ) { return m_stream->ReadArray    ( buf, bytes ); }
HRESULT CLR_RT_BinaryFormatter::WriteArray   ( const CLR_UINT8*  buf, int bytes ) { return m_stream->WriteArray   ( buf, bytes ); }

HRESULT CLR_RT_BinaryFormatter::ReadCompressedUnsigned( CLR_UINT32& val )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    CLR_UINT32 extra;

    TINYCLR_CHECK_HRESULT(m_stream->ReadBits( val, 8 ));

    if(val == 0xFF)
    {
        val = 0xFFFFFFFF;
    }
    else
    {
        int bits = 0;

        switch(val & 0xC0)
        {
        case 0x00: bits =  0; break;
        case 0x40: bits =  0; break;
        case 0x80: bits =  8; break;
        case 0xC0: bits = 24; break;
        }

        if(bits)
        {
            hr = m_stream->ReadBits( extra, bits );

            val = ((val & ~0xC0) << bits) | extra;
        }
    }


    TINYCLR_CLEANUP();

    TINYCLR_CLEANUP_END();
}

HRESULT CLR_RT_BinaryFormatter::WriteCompressedUnsigned( CLR_UINT32 val )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    int bits;

    if(val == 0xFFFFFFFF)
    {
        bits = 8;
    }
    else if(val < 0x80)
    {
        bits = 8;
    }
    else if(val < 0x3F00)
    {
        val  |= 0x8000;
        bits  = 16;
    }
    else if(val < 0x3F000000)
    {
        val  |= 0xC0000000;
        bits  = 32;
    }
    else
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
    }

    TINYCLR_CHECK_HRESULT(m_stream->WriteBits( val, bits ));


    TINYCLR_CLEANUP();

    TINYCLR_CLEANUP_END();
}

//--//

HRESULT CLR_RT_BinaryFormatter::ReadType( CLR_RT_ReflectionDef_Index& val )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    TINYCLR_CHECK_HRESULT(m_stream->ReadBits( m_lastTypeRead, 32 ));

    val.InitializeFromHash( m_lastTypeRead );

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_BinaryFormatter::WriteType( const CLR_RT_ReflectionDef_Index& val )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(WriteType( val.GetTypeHash() ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_BinaryFormatter::WriteType( CLR_UINT32 hash )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    if(hash == 0xFFFFFFFF || hash == 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    TINYCLR_SET_AND_LEAVE(m_stream->WriteBits( hash, 32 ));

    TINYCLR_NOCLEANUP();
}

