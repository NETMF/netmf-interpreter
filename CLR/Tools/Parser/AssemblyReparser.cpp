////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"

#define NOTCOMPATIBLE_MASKED(src,dst,mask) ((src) & (mask)) != ((dst) & (mask))

////////////////////////////////////////////////////////////////////////////////////////////////////

MetaData::Reparser::TypeSignaturePtr::TypeSignaturePtr()
{
    m_ptr = NULL;
}

MetaData::Reparser::TypeSignaturePtr::TypeSignaturePtr( const TypeSignaturePtr& ots )
{
    m_ptr = NULL;

    *this = ots;
}

MetaData::Reparser::TypeSignaturePtr& MetaData::Reparser::TypeSignaturePtr::operator=( const TypeSignaturePtr& ots )
{
    delete m_ptr; m_ptr = NULL;

    if(ots.m_ptr)
    {
        m_ptr = new TypeSignature();

        *m_ptr = *ots.m_ptr;
    }

    return *this;
}

MetaData::Reparser::TypeSignaturePtr::~TypeSignaturePtr()
{
    delete m_ptr;
}

void MetaData::Reparser::TypeSignaturePtr::Allocate()
{
    delete m_ptr; m_ptr = new TypeSignature();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void MetaData::Reparser::TypeSignature::Parse( Assembly* holder, CLR_RT_Assembly* assm, CLR_PMETADATA& pSigBlob )
{
    m_holder = holder;
    m_opt    = CLR_UncompressElementType( pSigBlob );

    switch(m_opt)
    {
    case DATATYPE_BYREF    : ParseSubType( assm, pSigBlob ); break;
    case DATATYPE_VALUETYPE: ParseToken  ( assm, pSigBlob ); break;
    case DATATYPE_CLASS    : ParseToken  ( assm, pSigBlob ); break;
    case DATATYPE_SZARRAY  : ParseSubType( assm, pSigBlob ); break;
    }
}

void MetaData::Reparser::TypeSignature::ParseToken( CLR_RT_Assembly* assm, CLR_PMETADATA& pSigBlob )
{
    m_token = m_holder->Parse( assm, CLR_TkFromStream( pSigBlob ) );
}

void MetaData::Reparser::TypeSignature::ParseSubType( CLR_RT_Assembly* assm, CLR_PMETADATA& pSigBlob )
{
    m_sub.Allocate();

    m_sub->Parse( m_holder, assm, pSigBlob );
}

//--//

void MetaData::Reparser::TypeSignature::BuildString()
{
    switch(m_opt)
    {
        case DATATYPE_VOID      : m_displayString = "VOID"      ; break;
        case DATATYPE_BOOLEAN   : m_displayString = "BOOLEAN"   ; break;
        case DATATYPE_CHAR      : m_displayString = "CHAR"      ; break;
        case DATATYPE_I1        : m_displayString = "I1"        ; break;
        case DATATYPE_U1        : m_displayString = "U1"        ; break;
        case DATATYPE_I2        : m_displayString = "I2"        ; break;
        case DATATYPE_U2        : m_displayString = "U2"        ; break;
        case DATATYPE_I4        : m_displayString = "I4"        ; break;
        case DATATYPE_U4        : m_displayString = "U4"        ; break;
        case DATATYPE_I8        : m_displayString = "I8"        ; break;
        case DATATYPE_U8        : m_displayString = "U8"        ; break;
        case DATATYPE_R4        : m_displayString = "R4"        ; break;
        case DATATYPE_R8        : m_displayString = "R8"        ; break;
        case DATATYPE_STRING    : m_displayString = "STRING"    ; break;
        case DATATYPE_BYREF     : m_displayString = "BYREF "    ; break;
        case DATATYPE_VALUETYPE : m_displayString = "VALUETYPE "; break;
        case DATATYPE_CLASS     : m_displayString = "CLASS "    ; break;
        case DATATYPE_OBJECT    : m_displayString = "OBJECT"    ; break;
        case DATATYPE_SZARRAY   : m_displayString = "SZARRAY "  ; break;
        default: throw std::string( "Unknown DataType" );
    }

    switch(m_opt)
    {
        case DATATYPE_BYREF  :
        case DATATYPE_SZARRAY:
            m_displayString += m_sub->GetDisplayString();
            break;

        case DATATYPE_VALUETYPE:
        case DATATYPE_CLASS    :
            m_displayString += m_token->GetDisplayString();
            break;
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void MetaData::Reparser::MethodSignature::Parse( Assembly* holder, CLR_RT_Assembly* assm, CLR_PMETADATA& pSigBlob )
{
    m_holder = holder;
    m_flags  = *pSigBlob++;

    ULONG count = *pSigBlob++;

    m_retValue.Parse( holder, assm, pSigBlob );

    for(ULONG i=0; i<count; i++)
    {
        TypeSignature param;

        param.Parse( holder, assm, pSigBlob );

        m_lstParams.push_back( param );
    }
}

//--//

void MetaData::Reparser::MethodSignature::BuildString()
{
    bool fFirst = true;

    m_displayString = m_retValue.GetDisplayString();

    for(TypeSignatureIter it=m_lstParams.begin(); it!=m_lstParams.end(); it++)
    {
        m_displayString += fFirst ? " ( " : ", ";
        m_displayString += it->GetDisplayString();

        fFirst = false;
    }

    m_displayString += fFirst ? " ()" : " )";
}

//--//

bool MetaData::Reparser::MethodSignature::operator==( MethodSignature& sig )
{
    return this->GetDisplayString() == sig.GetDisplayString();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void MetaData::Reparser::LocalVarSignature::Parse( Assembly* holder, CLR_RT_Assembly* assm, CLR_PMETADATA& pSigBlob, CLR_UINT32 num )
{
    m_holder = holder;

    while(num--)
    {
        TypeSignature var;

        var.Parse( holder, assm, pSigBlob );

        m_lstVars.push_back( var );
    }
}

//--//

void MetaData::Reparser::LocalVarSignature::BuildString()
{
    bool fFirst = true;

    for(TypeSignatureIter it = m_lstVars.begin(); it != m_lstVars.end(); it++)
    {
        if(!fFirst)
        {
            m_displayString += " # ";

            fFirst = false;
        }

        m_displayString += it->GetDisplayString();
    }
}

//--//

bool MetaData::Reparser::LocalVarSignature::operator==( LocalVarSignature& sig )
{
    return this->GetDisplayString() == sig.GetDisplayString();
}

////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////

inline mdToken SelectTokenOrNil( CLR_UINT16 src, CLR_UINT32 tk ) { return (src != CLR_EmptyIndex) ? tk : CLR_EmptyToken; }

//--//

MetaData::Reparser::BaseElement::BaseElement()
{
    m_holder = NULL;
}

bool MetaData::Reparser::BaseElement::operator==( BaseElement& be )
{
    return this->GetDisplayString() == be.GetDisplayString();
}

const std::string& MetaData::Reparser::BaseElement::GetDisplayString()
{
    if(m_displayString.size() == 0)
    {
        BuildString();
    }

    return m_displayString;
}

//--//

void MetaData::Reparser::AssemblyRef::Parse( BaseToken* container, CLR_RT_Assembly* assm, const CLR_RECORD_ASSEMBLYREF* src )
{
    m_name    = assm->GetString( src->name );
    m_version =                  src->version;
}

void MetaData::Reparser::AssemblyRef::BuildString()
{
    MetaData::Reparser::Assembly::BuildString( m_displayString, m_name.c_str(), m_version );
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void MetaData::Reparser::TypeRef::Parse( BaseToken* container, CLR_RT_Assembly* assm, const CLR_RECORD_TYPEREF* src )
{
    m_name = assm->GetString( src->name );

    if(src->scope & 0x8000) // Flag for TypeRef
    {
        m_scope = m_holder->Parse( assm, CLR_TkFromType( TBL_TypeRef, src->scope & 0x7FFF ) );
    }
    else
    {
        m_scope = m_holder->Parse( assm, CLR_TkFromType( TBL_AssemblyRef, src->scope ) );

        m_nameSpace = assm->GetString( src->nameSpace );
    }
}

void MetaData::Reparser::TypeRef::BuildString()
{
    m_displayString = m_scope->GetDisplayString();

    if(m_scope->GetTableOfInstance() == TBL_AssemblyRef)
    {
        m_displayString += "::";

        if(m_nameSpace.size() > 0)
        {
            m_displayString += m_nameSpace;
            m_displayString += ".";
        }

        m_displayString += m_name;
    }
    else
    {
        m_displayString += "+";
        m_displayString += m_name;
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void MetaData::Reparser::FieldRef::Parse( BaseToken* container, CLR_RT_Assembly* assm, const CLR_RECORD_FIELDREF* src )
{
    CLR_PMETADATA sig;

    m_name      = assm->GetString(                                    src->name        );
    m_container = m_holder->Parse( assm, CLR_TkFromType( TBL_TypeRef, src->container ) );

    sig = assm->GetSignature( src->sig ); sig++; // Skip field indicator.
    m_sig.Parse( m_holder, assm, sig );
}

void MetaData::Reparser::FieldRef::BuildString()
{
    m_displayString  = m_container->GetDisplayString();
    m_displayString += "::";
    m_displayString += m_name;
    m_displayString += " ";
    m_displayString += m_sig.GetDisplayString();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void MetaData::Reparser::MethodRef::Parse( BaseToken* container, CLR_RT_Assembly* assm, const CLR_RECORD_METHODREF* src )
{
    CLR_PMETADATA sig;

    m_name      = assm->GetString(                                    src->name        );
    m_container = m_holder->Parse( assm, CLR_TkFromType( TBL_TypeRef, src->container ) );

    sig = assm->GetSignature( src->sig ); m_sig.Parse( m_holder, assm, sig );
}

void MetaData::Reparser::MethodRef::BuildString()
{
    m_displayString  = m_container->GetDisplayString();
    m_displayString += "::";
    m_displayString += m_name;
    m_displayString += " ";
    m_displayString += m_sig.GetDisplayString();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void MetaData::Reparser::TypeDef::Parse( BaseToken* container, CLR_RT_Assembly* assm, const CLR_RECORD_TYPEDEF* src )
{
    CLR_PMETADATA sig;
    CLR_IDX       pos;
    CLR_UINT32    num;

    m_name          = assm->GetString( src->name      );
    m_nameSpace     = assm->GetString( src->nameSpace );

    m_extends       = m_holder->Parse( assm, SelectTokenOrNil( src->extends      , CLR_TkFromType( ((src->extends & 0x8000) ? TBL_TypeRef : TBL_TypeDef), src->extends       & 0x7FFF ) ) );
    m_enclosingType = m_holder->Parse( assm, SelectTokenOrNil( src->enclosingType, CLR_TkFromType(                            TBL_TypeDef               , src->enclosingType          ) ) );

    pos             = src->methods_First;
    num             = src->vMethods_Num; while(num--) ParseMethod( m_methods_Virtual , assm, pos++ );
    num             = src->iMethods_Num; while(num--) ParseMethod( m_methods_Instance, assm, pos++ );
    num             = src->sMethods_Num; while(num--) ParseMethod( m_methods_Static  , assm, pos++ );

    m_dataType      = src->dataType;

    pos             = src->sFields_First;
    num             = src->sFields_Num; while(num--) ParseField( m_fields_Static  , assm, pos++ );
    pos             = src->iFields_First;
    num             = src->iFields_Num; while(num--) ParseField( m_fields_Instance, assm, pos++ );

    m_flags         = src->flags;

    if(src->interfaces != CLR_EmptyIndex)
    {
        sig = assm->GetSignature( src->interfaces );
        num = (*sig++);

        while(num--)
        {
            m_interfaces.push_back( m_holder->Parse( assm, CLR_TkFromStream( sig ) ) );
        }
    }
}

void MetaData::Reparser::TypeDef::ParseMethod( BaseTokenPtrList& lst, CLR_RT_Assembly* assm, CLR_IDX pos )
{
    BaseTokenPtr ptr = m_holder->Parse( assm, CLR_TkFromType( TBL_MethodDef, pos ), this );

    lst.push_back( ptr );
}

void MetaData::Reparser::TypeDef::ParseField( BaseTokenPtrList& lst, CLR_RT_Assembly* assm, CLR_IDX pos )
{
    BaseTokenPtr ptr = m_holder->Parse( assm, CLR_TkFromType( TBL_FieldDef, pos ), this );

    lst.push_back( ptr );
}

//--//

bool MetaData::Reparser::TypeDef::IsCompatible( TypeDef* ptr )
{
    //
    // Compare by display string, this include enclosingType, nameSpace, and name.
    //
    if(*this != *ptr) return false;

    if(m_dataType != ptr->m_dataType) return false;

    //
    // Scope difference doesn't trigger an incompatibility.
    //
    if(NOTCOMPATIBLE_MASKED(m_flags,ptr->m_flags,~CLR_RECORD_TYPEDEF::TD_Scope_Mask)) return false;

    if(Assembly::CompareObject ( m_extends   , ptr->m_extends    ) == false) return false;
    if(Assembly::CompareObjects( m_interfaces, ptr->m_interfaces ) == false) return false;

    return true;
}

//--//

void MetaData::Reparser::TypeDef::BuildString()
{
    if(!m_enclosingType)
    {
        m_displayString  = m_holder->GetDisplayString();
        m_displayString += "::";

        if(m_nameSpace.size())
        {
            m_displayString += m_nameSpace;
            m_displayString += ".";
        }

        m_displayString += m_name;
    }
    else
    {
        m_displayString  = m_enclosingType->GetDisplayString();
        m_displayString += "+";
        m_displayString += m_name;
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void MetaData::Reparser::FieldDef::Parse( BaseToken* container, CLR_RT_Assembly* assm, const CLR_RECORD_FIELDDEF* src )
{
    CLR_PMETADATA sig;

    m_container = container;
    m_name      = assm->GetString( src->name  );
    m_flags     =                  src->flags  ;

    sig = assm->GetSignature( src->sig ); sig++; // Skip field indicator.
    m_sig.Parse( m_holder, assm, sig );

    if(src->defaultValue != CLR_EmptyIndex)
    {
        CLR_PMETADATA ptrSrc  = assm->GetSignature( src->defaultValue );
        CLR_UINT32    lenSrc; TINYCLR_READ_UNALIGNED_UINT16( lenSrc, ptrSrc );

        m_defaultValue.resize( lenSrc ); memcpy( &m_defaultValue[0], ptrSrc, lenSrc );
    }
}

bool MetaData::Reparser::FieldDef::IsCompatible( FieldDef* ptr )
{
    //
    // Scope difference doesn't trigger an incompatibility.
    //
    if(NOTCOMPATIBLE_MASKED(m_flags,ptr->m_flags,~CLR_RECORD_FIELDDEF::FD_Scope_Mask)) return false;

    if(m_flags != ptr->m_flags ) return false;;

    return true;
}

void MetaData::Reparser::FieldDef::BuildString()
{
    m_displayString  = m_container->GetDisplayString();
    m_displayString += "::";
    m_displayString += m_name;
    m_displayString += " ";
    m_displayString += m_sig.GetDisplayString();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void MetaData::Reparser::MethodDef::Parse( BaseToken* container, CLR_RT_Assembly* assm, const CLR_RECORD_METHODDEF* src )
{
    CLR_PMETADATA sig;

    m_container       = container;
    m_name            = assm->GetString( src->name );

    m_flags           = src->flags;

    m_retVal          = src->retVal         ;
    m_numArgs         = src->numArgs        ;
    m_numLocals       = src->numLocals      ;
    m_lengthEvalStack = src->lengthEvalStack;

    if(src->locals != CLR_EmptyIndex)
    {
        sig = assm->GetSignature( src->locals ); m_locals.Parse( m_holder, assm, sig, src->numLocals );
    }

    if(src->sig != CLR_EmptyIndex)
    {
        sig = assm->GetSignature( src->sig ); m_sig.Parse( m_holder, assm, sig );
    }

    if(src->RVA != CLR_EmptyIndex)
    {
        CLR_OFFSET start;
        CLR_OFFSET end;

        if(assm->FindMethodBoundaries( CLR_DataFromTk( m_tk ), start, end ))
        {
            CLR_PMETADATA pStart = assm->GetByteCode( start );
            CLR_PMETADATA pEnd   = assm->GetByteCode( end   );

            if(src->flags & CLR_RECORD_METHODDEF::MD_HasExceptionHandlers)
            {
                const CLR_RECORD_EH* ptrEh;
                CLR_UINT32           numEh;

                pEnd = CLR_RECORD_EH::ExtractEhFromByteCode( pEnd, ptrEh, numEh );

                while(numEh--)
                {
                    CLR_RECORD_EH eh; memcpy( &eh, ptrEh++, sizeof(eh) );
                    Eh&           rec = *(m_eh.insert( m_eh.end(), Eh() ));

                    rec.m_mode         =                         eh.mode;
                    rec.m_classToken   = m_holder->Parse( assm, (eh.mode == CLR_RECORD_EH::EH_Catch) ? eh.GetToken() : CLR_EmptyToken );
                    rec.m_tryStart     =                         eh.tryStart;
                    rec.m_tryEnd       =                         eh.tryEnd;
                    rec.m_handlerStart =                         eh.handlerStart;
                    rec.m_handlerEnd   =                         eh.handlerEnd;
                }
            }

            m_byteCode.resize( pEnd - pStart ); memcpy( &m_byteCode[0], pStart, pEnd - pStart );
        }
    }
}

bool MetaData::Reparser::MethodDef::IsCompatible( MethodDef* ptr )
{
    //
    // Compare by display string, this include container, name, sig.
    //
    if(*this != *ptr) return false;

    //
    // Scope difference doesn't trigger an incompatibility.
    //
    if(NOTCOMPATIBLE_MASKED(m_flags,ptr->m_flags,~CLR_RECORD_METHODDEF::MD_Scope_Mask)) return false;

    if(m_retVal          != ptr->m_retVal         ) return false;
    if(m_numArgs         != ptr->m_numArgs        ) return false;
    if(m_numLocals       != ptr->m_numLocals      ) return false;
    if(m_lengthEvalStack != ptr->m_lengthEvalStack) return false;

    //--//

    if(m_locals != ptr->m_locals) return false;

    //--//

    if(m_byteCode.size() != ptr->m_byteCode.size()) return false;

    if(m_byteCode.size())
    {
        CLR_PMETADATA ipThis    = (CLR_PMETADATA)&     m_byteCode[0                ];
        CLR_PMETADATA ipThisEnd = (CLR_PMETADATA)&     m_byteCode[m_byteCode.size()];
        CLR_PMETADATA ipOther   = (CLR_PMETADATA)&ptr->m_byteCode[0                ];

        while(ipThis < ipThisEnd)
        {
            CLR_OPCODE opThis  = CLR_ReadNextOpcodeCompressed( ipThis  );
            CLR_OPCODE opOther = CLR_ReadNextOpcodeCompressed( ipOther );

            if(opThis != opOther) return false;

            if(IsOpParamToken( c_CLR_RT_OpcodeLookup[opThis].m_opParam ))
            {
                if(Assembly::CompareToken( m_holder, CLR_ReadTokenCompressed( ipThis , opThis ), ptr->m_holder, CLR_ReadTokenCompressed( ipOther, opOther ) ) == false) return false;
            }
            else
            {
                CLR_PMETADATA ipThis2  = ipThis ; ipThis  = CLR_SkipBodyOfOpcodeCompressed( ipThis , opThis  );
                CLR_PMETADATA ipOther2 = ipOther; ipOther = CLR_SkipBodyOfOpcodeCompressed( ipOther, opOther );

                if((ipThis - ipThis2) != (ipOther - ipOther2)) return false;

                if(memcmp( ipThis2, ipOther2, (ipThis - ipThis2) )) return false;
            }
        }
    }

    //--//

    if(m_eh.size() != ptr->m_eh.size()) return false;

    if(m_eh.size())
    {
        MethodDef::EhIter itThis  =      m_eh.begin();
        MethodDef::EhIter itOther = ptr->m_eh.begin();

        while(itThis != m_eh.end())
        {
            MethodDef::Eh& ehThis  = *itThis ++;
            MethodDef::Eh& ehOther = *itOther++;

            if(ehThis.m_mode         != ehOther.m_mode        ) return false;
            if(ehThis.m_tryStart     != ehOther.m_tryStart    ) return false;
            if(ehThis.m_tryEnd       != ehOther.m_tryEnd      ) return false;
            if(ehThis.m_handlerStart != ehOther.m_handlerStart) return false;
            if(ehThis.m_handlerEnd   != ehOther.m_handlerEnd  ) return false;

            if(ehThis.m_mode != CLR_RECORD_EH::EH_Filter && ehOther.m_mode != CLR_RECORD_EH::EH_Filter)
            {
                if(Assembly::CompareObject( ehThis.m_classToken, ehOther.m_classToken ) == false) return false;
            }
        }
    }

    return true;
}

void MetaData::Reparser::MethodDef::BuildString()
{
    m_displayString  = m_container->GetDisplayString();
    m_displayString += "::";
    m_displayString += m_name;
    m_displayString += " ";
    m_displayString += m_sig.GetDisplayString();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

MetaData::Reparser::Attribute::Reader::Reader( CLR_RT_Assembly* assm, CLR_PMETADATA sig )
{
    m_assm = assm;
    m_sig  = sig;
}

void MetaData::Reparser::Attribute::Reader::Read( CLR_RT_Buffer& val, size_t len )
{
    val.resize( len );

    memcpy( &val[0], m_sig, len ); m_sig += len;
}

void MetaData::Reparser::Attribute::Reader::Read( std::string& val )
{
    CLR_UINT16 idx; Read( idx );

    val = m_assm->GetString( idx );
}

void MetaData::Reparser::Attribute::Reader::Read( CorSerializationType& val )
{
    val = (CorSerializationType)*m_sig++;
}

void MetaData::Reparser::Attribute::Reader::Read( CLR_UINT16& val )
{
    memcpy( &val, m_sig, sizeof(val) ); m_sig += sizeof(val);
}

//--//

void MetaData::Reparser::Attribute::Name::Parse( Reader& reader )
{
    reader.Read( m_opt  );
    reader.Read( m_text );
}

void MetaData::Reparser::Attribute::Value::Parse( Reader& reader )
{
    reader.Read( m_opt );

    switch(m_opt)
    {
    case SERIALIZATION_TYPE_BOOLEAN: reader.Read( m_numeric, 1 ); break;
    case SERIALIZATION_TYPE_CHAR   : reader.Read( m_numeric, 2 ); break;
    case SERIALIZATION_TYPE_I1     : reader.Read( m_numeric, 1 ); break;
    case SERIALIZATION_TYPE_U1     : reader.Read( m_numeric, 1 ); break;
    case SERIALIZATION_TYPE_I2     : reader.Read( m_numeric, 2 ); break;
    case SERIALIZATION_TYPE_U2     : reader.Read( m_numeric, 2 ); break;
    case SERIALIZATION_TYPE_I4     : reader.Read( m_numeric, 4 ); break;
    case SERIALIZATION_TYPE_U4     : reader.Read( m_numeric, 4 ); break;
    case SERIALIZATION_TYPE_I8     : reader.Read( m_numeric, 8 ); break;
    case SERIALIZATION_TYPE_U8     : reader.Read( m_numeric, 8 ); break;
    case SERIALIZATION_TYPE_R4     : reader.Read( m_numeric, 4 ); break;
    case SERIALIZATION_TYPE_R8     : reader.Read( m_numeric, 8 ); break;
    case SERIALIZATION_TYPE_STRING : reader.Read( m_text       ); break;
    case SERIALIZATION_TYPE_TYPE   : reader.Read( m_text       ); break;
    case SERIALIZATION_TYPE_ENUM   : reader.Read( m_numeric, 4 ); break;
    default                        : throw std::string( "Bad attribute value" );
    }
}

//--//

void MetaData::Reparser::Attribute::Parse( BaseToken* container, CLR_RT_Assembly* assm, const CLR_RECORD_ATTRIBUTE* src )
{
    m_owner       = m_holder->Parse( assm, CLR_TkFromType( (CLR_TABLESENUM)src->ownerType, src->ownerIdx                                            ) );
    m_constructor = m_holder->Parse( assm, CLR_TkFromType( ((src->constructor & 0x8000) ? TBL_MethodRef : TBL_MethodDef), src->constructor & 0x7FFF ) );

    //--//

    Reader     reader( assm, assm->GetSignature( src->data ) );
    CLR_UINT16 num;
    size_t     params;
    size_t     i;

    if(src->constructor & 0x8000)
    {
        MethodRef* mr = m_constructor.CastTo( MethodRef() );

        params = mr->m_sig.m_lstParams.size();
    }
    else
    {
        MethodDef* md = m_constructor.CastTo( MethodDef() );

        params = md->m_sig.m_lstParams.size();
    }

    for(i=0; i<params; i++)
    {
        Value val; val.Parse( reader );

        m_valuesFixed.push_back( val );
    }

    reader.Read( num );

    for(i=0; i<num; i++)
    {
        Name  name; name.Parse( reader );
        Value val;  val .Parse( reader );

        m_valuesVariable[name] = val;
    }
}

void MetaData::Reparser::Attribute::BuildString()
{
    m_displayString  = "ATTRIBUTE: ";
    m_displayString += m_owner->GetDisplayString();
    m_displayString += " - ";
    m_displayString += m_constructor->GetDisplayString();
}

bool MetaData::Reparser::Attribute::IsCompatible( Attribute* ptr )
{
    if(Assembly::CompareObject( m_owner      , ptr->m_owner       ) == false) return false;
    if(Assembly::CompareObject( m_constructor, ptr->m_constructor ) == false) return false;


    if(m_valuesFixed.size() != ptr->m_valuesFixed.size()) return false;
    ValueListIter itLst  = this->m_valuesFixed.begin();
    ValueListIter itLst2 = ptr ->m_valuesFixed.begin();

    while(itLst != m_valuesFixed.end())
    {
        if(*itLst++ != *itLst2++) return false;
    }


    if(m_valuesVariable.size() != ptr->m_valuesVariable.size()) return false;
    ValueMapIter itMap  = this->m_valuesVariable.begin();
    ValueMapIter itMap2 = ptr ->m_valuesVariable.begin();

    while(itMap != m_valuesVariable.end())
    {
        if(itMap->first  != itMap2->first ) return false;
        if(itMap->second != itMap2->second) return false;

        itMap ++;
        itMap2++;
    }

    return true;
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void MetaData::Reparser::TypeSpec::Parse( BaseToken* container, CLR_RT_Assembly* assm, const CLR_RECORD_TYPESPEC* src )
{
    CLR_PMETADATA sig = assm->GetSignature( src->sig ); m_sig.Parse( m_holder, assm, sig );
}

void MetaData::Reparser::TypeSpec::BuildString()
{
    m_displayString  = "TYPESPEC ";
    m_displayString += m_sig.GetDisplayString();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void MetaData::Reparser::String::Parse( BaseToken* container, CLR_RT_Assembly* assm, LPCSTR src )
{
    m_value = src ? src : "";
}

bool MetaData::Reparser::String::IsCompatible( String* ptr )
{
    return (m_value == ptr->m_value);
}

void MetaData::Reparser::String::BuildString()
{
    m_displayString  = "STRING: ";
    m_displayString += m_value;
}

////////////////////////////////////////////////////////////////////////////////////////////////////

static LPCSTR TokenToString( mdToken tk )
{
    static char buf[256];
    LPCSTR      prefix = "??";

    switch(CLR_TypeFromTk( tk ))
    {
    case TBL_AssemblyRef: prefix = "AssemblyRef"; break;
    case TBL_TypeRef    : prefix = "TypeRef"    ; break;
    case TBL_FieldRef   : prefix = "FieldRef"   ; break;
    case TBL_MethodRef  : prefix = "MethodRef"  ; break;
    case TBL_TypeDef    : prefix = "TypeDef"    ; break;
    case TBL_FieldDef   : prefix = "FieldDef"   ; break;
    case TBL_MethodDef  : prefix = "MethodDef"  ; break;
    case TBL_Attributes : prefix = "Attributes" ; break;
    case TBL_TypeSpec   : prefix = "TypeSpec"   ; break;
    case TBL_Resources  : prefix = "Resources"  ; break;
    case TBL_Strings    : prefix = "Strings"    ; break;
    }

    sprintf_s( buf, MAXSTRLEN(buf), "%s:%04x", prefix, CLR_DataFromTk( tk ) );

    return buf;
}

//--//

void MetaData::Reparser::Assembly::BuildString( std::string& value, LPCSTR szName, const CLR_RECORD_VERSION& ver )
{
    value = szName;

    //char buf[256];
    //sprintf( buf, "(%d.%d.%d.%d)", ver.iMajorVersion, ver.iMinorVersion, ver.iBuildNumber, ver.iRevisionNumber );
    //value  = szName;
    //value += buf;
}

//--//

HRESULT MetaData::Reparser::Assembly::Load( LPCWSTR szFile )
{
    TINYCLR_HEADER();

    CLR_RT_Buffer        buffer;
    CLR_RECORD_ASSEMBLY* header;
    CLR_RT_Assembly*     assm = NULL;

    TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::LoadFile( szFile, buffer ));

    m_assemblyFile = szFile;

    header = (CLR_RECORD_ASSEMBLY*)&buffer[0];

    if(header->GoodAssembly() == false)
    {
        wprintf( L"Invalid assembly format for '%s': ", szFile );
        for(int i=0; i<sizeof(header->marker); i++)
        {
            wprintf( L"%02x", header->marker[i] );
        }
        wprintf( L"\n" );

        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    TINYCLR_CHECK_HRESULT(CLR_RT_Assembly::CreateInstance( header, assm ));

    m_name    = assm->m_szName;
    m_version = assm->m_header->version;

    //--//

#define REPARSER_TABLE_PARSE(dst)                                 \
    {                                                             \
        CLR_TABLESENUM tbl = dst::GetTable();                     \
        int            num = assm->m_pTablesSize[tbl];            \
                                                                  \
        for(int i=0; i<num; i++)                                  \
        {                                                         \
            if(Parse( assm, CLR_TkFromType( tbl, i ) ) == NULL)   \
            {                                                     \
                TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);                    \
            }                                                     \
        }                                                         \
    }

    REPARSER_TABLE_PARSE(TypeDef  );
    //REPARSER_TABLE_PARSE(Resource );
    REPARSER_TABLE_PARSE(Attribute);

#undef REPARSER_TABLE_PARSE

    //--//

    //
    // Analyze bytecode.
    //
    {
        for(StringToObjectIter it=m_lookupStringToObject.begin(); it!=m_lookupStringToObject.end(); it++)
        {
            MethodDef* md = it->second.CastTo( MethodDef() );
            if(md)
            {
                CLR_PMETADATA pStart =          &md->m_byteCode[0];
                CLR_PMETADATA pEnd   = pStart +  md->m_byteCode.size();

                CLR_PMETADATA ip = pStart;
                while(ip < pEnd)
                {
                    CLR_OPCODE op = CLR_ReadNextOpcodeCompressed( ip );

                    if(IsOpParamToken( c_CLR_RT_OpcodeLookup[op].m_opParam ))
                    {
                        size_t  index = (ip - pStart);
                        mdToken tk    = CLR_ReadTokenCompressed( ip, op );

                        md->m_tokens[index] = Parse( assm, tk );
                    }
                    else
                    {
                        ip = CLR_SkipBodyOfOpcodeCompressed( ip, op );
                    }
                }
            }
        }
    }

    //--//

    TINYCLR_CLEANUP();

    if(assm)
    {
        assm->MarkDead();
    }

    TINYCLR_CLEANUP_END();
}

//--//

void MetaData::Reparser::Assembly::Dump()
{
    for(TokenToObjectIter it=m_lookupTokenToObject.begin(); it!=m_lookupTokenToObject.end(); it++)
    {
        printf( "%s : %s\n", TokenToString( (CLR_UINT32)it->first ), it->second->GetDisplayString().c_str() );
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT MetaData::Reparser::Assembly::Compare_TypeDef( Assembly* other, StringSet* equal, StringSet* changed, StringSet* missing )
{
    TINYCLR_HEADER();

    std::string value;
    mdTokenSet  res;

    if(equal  ) equal  ->clear();
    if(changed) changed->clear();
    if(missing) missing->clear();

    REPARSER_TABLE_ENUM_BEGIN(this,TypeDef)
    {
        const std::string& str  = rec->GetDisplayString();
        TypeDef*           rec2 = other->FindObject( str, TypeDef() );

        if(rec2 == NULL)
        {
            if(missing) missing->insert( str );
        }
        else
        {
            if(rec->IsCompatible( rec2 ) == false)
            {
                if(changed) changed->insert( str );
            }
            else
            {
                if(equal) equal->insert( str );
            }
        }
    }
    REPARSER_TABLE_ENUM_END();

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT MetaData::Reparser::Assembly::Compare_FieldDef( Assembly* other, StringSet* equal, StringSet* changed, StringSet* missing )
{
    TINYCLR_HEADER();

    std::string value;
    mdTokenSet  res;

    if(equal  ) equal  ->clear();
    if(changed) changed->clear();
    if(missing) missing->clear();

    REPARSER_TABLE_ENUM_BEGIN(this,FieldDef)
    {
        const std::string& str  = rec->GetDisplayString();
        FieldDef*          rec2 = other->FindObject( str, FieldDef() );

        if(rec2 == NULL)
        {
            if(missing) missing->insert( str );
        }
        else
        {
            if(rec->IsCompatible( rec2 ) == false)
            {
                if(changed) changed->insert( str );
            }
            else
            {
                if(equal) equal->insert( str );
            }
        }
    }
    REPARSER_TABLE_ENUM_END();

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT MetaData::Reparser::Assembly::Compare_MethodDef( Assembly* other, StringSet* equal, StringSet* changed, StringSet* missing )
{
    TINYCLR_HEADER();

    std::string value;
    mdTokenSet  res;

    if(equal  ) equal  ->clear();
    if(changed) changed->clear();
    if(missing) missing->clear();

    REPARSER_TABLE_ENUM_BEGIN(this,MethodDef)
    {
        const std::string& str  = rec->GetDisplayString();
        MethodDef*         rec2 = other->FindObject( str, MethodDef() );

        if(rec2 == NULL)
        {
            if(missing) missing->insert( str );
        }
        else
        {
            if(rec->IsCompatible( rec2 ) == false)
            {
                if(changed) changed->insert( str );
            }
            else
            {
                if(equal) equal->insert( str );
            }
        }
    }
    REPARSER_TABLE_ENUM_END();

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT MetaData::Reparser::Assembly::Compare_Attribute( Assembly* other, StringSet* equal, StringSet* changed, StringSet* missing )
{
    TINYCLR_HEADER();

    std::string value;
    mdTokenSet  res;

    if(equal  ) equal  ->clear();
    if(changed) changed->clear();
    if(missing) missing->clear();

    REPARSER_TABLE_ENUM_BEGIN(this,Attribute)
    {
        const std::string& str  = rec->GetDisplayString();
        Attribute*         rec2 = other->FindObject( str, Attribute() );

        if(rec2 == NULL)
        {
            if(missing) missing->insert( str );
        }
        else
        {
            if(rec->IsCompatible( rec2 ) == false)
            {
                if(changed) changed->insert( str );
            }
            else
            {
                if(equal) equal->insert( str );
            }
        }
    }
    REPARSER_TABLE_ENUM_END();

    TINYCLR_NOCLEANUP_NOLABEL();
}

//--//

MetaData::Reparser::BaseToken* MetaData::Reparser::Assembly::Parse( CLR_RT_Assembly* assm, mdToken tk, BaseToken* container )
{
    TokenToObjectConstIter it = m_lookupTokenToObject.find( tk );

    if(it != m_lookupTokenToObject.end())
    {
        return (BaseToken*)(it->second);
    }


    CLR_TABLESENUM tbl = CLR_TypeFromTk(tk);

#define REPARSER_TABLE_PARSE(dst,src)                                           \
    if(tbl == dst::GetTable())                                                  \
    {                                                                           \
        const CLR_RECORD_##src* p = assm->Get##dst( CLR_DataFromTk( tk ) );     \
                                                                                \
        dst* rec = new dst; rec->Init( this, tk );                              \
                                                                                \
        m_lookupTokenToObject[tk] = rec;                                        \
                                                                                \
        rec->Parse( container, assm, p );                                       \
                                                                                \
        m_lookupStringToObject[rec->GetDisplayString()] = rec;                  \
                                                                                \
        return rec;                                                             \
    }

    REPARSER_TABLE_PARSE(AssemblyRef,ASSEMBLYREF);
    REPARSER_TABLE_PARSE(TypeRef    ,TYPEREF    );
    REPARSER_TABLE_PARSE(FieldRef   ,FIELDREF   );
    REPARSER_TABLE_PARSE(MethodRef  ,METHODREF  );
    REPARSER_TABLE_PARSE(TypeDef    ,TYPEDEF    );
    REPARSER_TABLE_PARSE(FieldDef   ,FIELDDEF   );
    REPARSER_TABLE_PARSE(MethodDef  ,METHODDEF  );
    REPARSER_TABLE_PARSE(Attribute  ,ATTRIBUTE  );
    REPARSER_TABLE_PARSE(TypeSpec   ,TYPESPEC   );

    if(tbl == String::GetTable())
    {
        LPCSTR str = assm->GetString( CLR_DataFromTk( tk ) );

        String* rec = new String; rec->Init( this, tk );

        m_lookupTokenToObject[tk] = rec;

        rec->Parse( container, assm, str );

        m_lookupStringToObject[rec->GetDisplayString()] = rec;

        return rec;
    }

#undef REPARSER_TABLE_PARSE

    return NULL;
}

const MetaData::Reparser::BaseTokenPtr& MetaData::Reparser::Assembly::FindObject( mdToken tk ) const
{
    TokenToObjectConstIter it = m_lookupTokenToObject.find( tk );

    if(it != m_lookupTokenToObject.end())
    {
        return it->second;
    }
    else
    {
        static BaseTokenPtr empty;

        return empty;
    }
}

//--//

const MetaData::Reparser::BaseTokenPtr& MetaData::Reparser::Assembly::FindObject( const std::string& value ) const
{
    StringToObjectConstIter it = m_lookupStringToObject.find( value );

    if(it != m_lookupStringToObject.end())
    {
        return it->second;
    }
    else
    {
        static BaseTokenPtr empty;

        return empty;
    }
}

//--//

void MetaData::Reparser::Assembly::DumpError( LPCSTR fmt, StringSet& set ) const
{
    printf( fmt, set.size() );

    for(StringSetIter it = set.begin(); it != set.end(); it++)
    {
        printf( "  %s\n", it->c_str() );
    }

    printf( "\n" );
}

//--//

HRESULT MetaData::Reparser::Assembly::DiffData::Compute( CompareFtn ftn, Assembly* orig, Assembly* patched )
{
    TINYCLR_HEADER();

    TINYCLR_CHECK_HRESULT((orig   ->*ftn)( patched, &m_setEqual, &m_setChanged, &m_setDeleted ));
    TINYCLR_CHECK_HRESULT((patched->*ftn)( orig   , NULL       , NULL         , &m_setAdded   ));

    TINYCLR_NOCLEANUP();
}

HRESULT MetaData::Reparser::Assembly::CreateDiff( Assembly* orig, Assembly* patched, bool fForceAssemblyRef )
{
    TINYCLR_HEADER();

    DiffData diffTypes;
    DiffData diffFields;
    DiffData diffMethods;
    DiffData diffResources;
    DiffData diffAttributes;
    bool     fFail = false;

    //--//

    printf( "\n\n" );

    m_assemblyOrig    = orig;
    m_assemblyPatched = patched;

    //
    // Compare Types.
    //
    TINYCLR_CHECK_HRESULT(diffTypes.Compute( &MetaData::Reparser::Assembly::Compare_TypeDef, orig, patched ));

    if(diffTypes.m_setChanged.size() > 0) { orig   ->DumpError( "FAILURE: %d type(s) changed:\n", diffTypes.m_setChanged ); fFail = true; }
    if(diffTypes.m_setDeleted.size() > 0) { orig   ->DumpError( "FAILURE: %d type(s) deleted:\n", diffTypes.m_setDeleted ); fFail = true; }
    if(diffTypes.m_setAdded  .size() > 0) { patched->DumpError(          "%d type(s) added:\n"  , diffTypes.m_setAdded   );               }

    if(fFail) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

    //--//

    //
    // Compare Fields.
    //
    TINYCLR_CHECK_HRESULT(diffFields.Compute( &MetaData::Reparser::Assembly::Compare_FieldDef, orig, patched ));

    //
    // Allow new fields in new types.
    //
    {
        for(StringSetIter it = diffFields.m_setAdded.begin(); it != diffFields.m_setAdded.end(); )
        {
            StringSetIter it2 = it++;
            FieldDef*     fd  = patched->FindObject( *it2, FieldDef() );

            if(diffTypes.m_setAdded.find( fd->m_container->GetDisplayString() ) != diffTypes.m_setAdded.end())
            {
                diffFields.m_setAdded.erase( it2 );
            }
        }
    }

    if(diffFields.m_setChanged.size() > 0) { orig   ->DumpError( "FAILURE: %d field(s) changed:\n", diffFields.m_setChanged ); fFail = true; }
    if(diffFields.m_setDeleted.size() > 0) { orig   ->DumpError( "FAILURE: %d field(s) deleted:\n", diffFields.m_setDeleted ); fFail = true; }
    if(diffFields.m_setAdded  .size() > 0) { patched->DumpError( "FAILURE: %d field(s) added:\n"  , diffFields.m_setAdded   ); fFail = true; }

    if(fFail) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

    //--//

    //
    // Compare Methods.
    //
    TINYCLR_CHECK_HRESULT(diffMethods.Compute( &MetaData::Reparser::Assembly::Compare_MethodDef, orig, patched ));

    //
    // Allow new methods in new types.
    //
    {
        for(StringSetIter it = diffMethods.m_setAdded.begin(); it != diffMethods.m_setAdded.end(); )
        {
            StringSetIter it2 = it++;
            MethodDef*    md  = patched->FindObject( *it2, MethodDef() );

            if(diffTypes.m_setAdded.find( md->m_container->GetDisplayString() ) != diffTypes.m_setAdded.end())
            {
                diffMethods.m_setAdded.erase( it2 );
            }
        }
    }

    if(diffMethods.m_setChanged.size() > 0) { orig   ->DumpError(          "%d method(s) changed:\n", diffMethods.m_setChanged );               }
    if(diffMethods.m_setDeleted.size() > 0) { orig   ->DumpError( "FAILURE: %d method(s) deleted:\n", diffMethods.m_setDeleted ); fFail = true; }
    if(diffMethods.m_setAdded  .size() > 0) { patched->DumpError( "FAILURE: %d method(s) added:\n"  , diffMethods.m_setAdded   ); fFail = true; }

    if(fFail) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

    //--//
    
    if(fFail) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

    //--//

    //
    // Compare Attributes.
    //
    TINYCLR_CHECK_HRESULT(diffAttributes.Compute( &MetaData::Reparser::Assembly::Compare_Attribute, orig, patched ));

    //
    // Allow new attributes in new types.
    //
    {
        for(StringSetIter it = diffAttributes.m_setAdded.begin(); it != diffAttributes.m_setAdded.end(); )
        {
            StringSetIter it2  = it++;
            Attribute*    attr = patched->FindObject( *it2, Attribute() );
            std::string   str;

            switch(attr->m_owner->GetTableOfInstance())
            {
            case TBL_TypeDef:
                {
                    TypeDef* td = attr->m_owner.CastTo( TypeDef() );

                    str = td->GetDisplayString();
                }
                break;

            case TBL_FieldDef:
                {
                    FieldDef* fd = attr->m_owner.CastTo( FieldDef() );

                    str = fd->m_container->GetDisplayString();
                }
                break;

            case TBL_MethodDef:
                {
                    MethodDef* md = attr->m_owner.CastTo( MethodDef() );

                    str = md->m_container->GetDisplayString();
                }
                break;
            }

            if(diffTypes.m_setAdded.find( str ) != diffTypes.m_setAdded.end())
            {
                diffAttributes.m_setAdded.erase( it2 );
            }
        }
    }


    if(diffAttributes.m_setChanged.size() > 0) { orig   ->DumpError( "FAILURE: %d attribute(s) changed:\n", diffAttributes.m_setChanged ); fFail = true; }
    if(diffAttributes.m_setDeleted.size() > 0) { orig   ->DumpError( "FAILURE: %d attribute(s) deleted:\n", diffAttributes.m_setDeleted ); fFail = true; }
    if(diffAttributes.m_setAdded  .size() > 0) { patched->DumpError( "FAILURE: %d attribute(s) added:\n"  , diffAttributes.m_setAdded   ); fFail = true; }

    if(fFail) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

    //--//

    //printf( "\n\nORIG:\n\n" );
    //orig->Dump();

    //printf( "\n\nPATCHED:\n\n" );
    //patched->Dump();

    this->m_name    = patched->m_name;
    this->m_version = patched->m_version;

    //
    // Create the objects that need to be added to the patch assembly, for now just as place holders.
    //
    {
        for(StringSetIter it = diffTypes.m_setAdded.begin(); it != diffTypes.m_setAdded.end(); it++)
        {
            TypeDef*   td     = patched->FindObject( *it, TypeDef() );
            BaseToken* ptrNew = AddToCreate( td, NULL );

            AddToCreate( td->m_methods_Virtual , ptrNew );
            AddToCreate( td->m_methods_Instance, ptrNew );
            AddToCreate( td->m_methods_Static  , ptrNew );
            AddToCreate( td->m_fields_Static   , ptrNew );
            AddToCreate( td->m_fields_Instance , ptrNew );
        }
    }

    //
    // Create the objects that need to be changed in the patch assembly, for now just as place holders.
    //
    {
        for(StringSetIter it = diffMethods.m_setChanged.begin(); it != diffMethods.m_setChanged.end(); it++)
        {
            MethodDef* md = patched->FindObject( *it, MethodDef() );

            AddToPatch( md );
        }
    }

    if(fForceAssemblyRef)
    {
        //
        // We need at least the assembly reference.
        //
        (void)Clone( orig );
    }

    Resolve( orig, patched );

    //--//

    TINYCLR_NOCLEANUP();
}

//--//

bool MetaData::Reparser::Assembly::CompareToken( Assembly* orig, mdToken tkOrig, Assembly* patched, mdToken tkPatched )
{
    return CompareObject( orig->FindObject( tkOrig ), patched->FindObject( tkPatched ) );
}

bool MetaData::Reparser::Assembly::CompareObject( const BaseTokenPtr& ptrOrig, const BaseTokenPtr& ptrPatched )
{
    if(!ptrOrig)
    {
        return (!ptrPatched);
    }
    else
    {
        if(!ptrPatched) return false;

        return ptrOrig->GetDisplayString() == ptrPatched->GetDisplayString();
    }
}

bool MetaData::Reparser::Assembly::CompareObjects( const BaseTokenPtrList& lstOrig, const BaseTokenPtrList& lstPatched )
{
    if(lstOrig.size() != lstPatched.size()) return false;

    BaseTokenPtrConstIter itOrig    = lstOrig   .begin();
    BaseTokenPtrConstIter itPatched = lstPatched.begin();

    while(itOrig != lstOrig.end())
    {
        if(CompareObject( *itOrig++, *itPatched++ ) == false) return false;
    }

    return true;
}

////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////

MetaData::Reparser::BaseToken* MetaData::Reparser::Assembly::AddToCreate( BaseToken* ptr, BaseToken* container )
{
    const std::string& str    = ptr->GetDisplayString();
    BaseToken*         ptrNew = NULL;

    switch(ptr->GetTableOfInstance())
    {
    case TBL_TypeDef:
        {
            TypeDef* tdOld = (TypeDef*)ptr;
            TypeDef* tdNew = new TypeDef();


            tdNew->m_name          = tdOld->m_name;
            tdNew->m_nameSpace     = tdOld->m_nameSpace;

        ////tdNew->m_extends;
        ////tdNew->m_enclosingType;

        ////tdNew->m_interfaces;

        ////tdNew->m_methods_Virtual;
        ////tdNew->m_methods_Instance;
        ////tdNew->m_methods_Static;
            tdNew->m_dataType      = tdOld->m_dataType;

        ////tdNew->m_fields_Static;
        ////tdNew->m_fields_Instance;

            tdNew->m_flags         = tdOld->m_flags;


            ptrNew = tdNew;
        }
        break;

    case TBL_FieldDef:
        {
            FieldDef* fdOld = (FieldDef*)ptr;
            FieldDef* fdNew = new FieldDef();


            fdNew->m_container    = container;

            fdNew->m_name         = fdOld->m_name;
            fdNew->m_flags        = fdOld->m_flags;

        ////fdNew->m_sig;

            fdNew->m_defaultValue = fdOld->m_defaultValue;


            ptrNew = fdNew;
        }
        break;

    case TBL_MethodDef:
        {
            MethodDef* mdOld = (MethodDef*)ptr;
            MethodDef* mdNew = new MethodDef();


            mdNew->m_container       = container;

            mdNew->m_name            = mdOld->m_name;
        ////mdNew->m_byteCode;
        ////mdNew->m_tokens;
        ////mdNew->m_eh;

            mdNew->m_flags           = mdOld->m_flags;

            mdNew->m_retVal          = mdOld->m_retVal;
            mdNew->m_numArgs         = mdOld->m_numArgs;
            mdNew->m_numLocals       = mdOld->m_numLocals;
            mdNew->m_lengthEvalStack = mdOld->m_lengthEvalStack;

        ////mdNew->m_locals;
        ////mdNew->m_sig;


            ptrNew = mdNew;
        }
        break;

    case TBL_Attributes:
        {
            Attribute* attrNew = new Attribute();


            ptrNew = attrNew;
        }
        break;

    default:
        throw (std::string( "Patch of non-supported element: " ) + str );
    }

    ptrNew->Init( this, CLR_TkFromType( ptrNew->GetTableOfInstance(), (CLR_UINT32)m_lookupTokenToObject.size() ) );

    ptrNew->m_displayString = str;
    ptrNew->m_patched       = ptr;

    m_lookupTokenToObject         [ ptrNew->m_tk ] = ptrNew;
    m_lookupStringToObject        [ str          ] = ptrNew;
    m_lookupStringToObjectToCreate[ str          ] = ptrNew;

    m_objectsToProcess.push_back( ptrNew );

    return ptrNew;
}

void MetaData::Reparser::Assembly::AddToCreate( const BaseTokenPtrList& lst, BaseToken* container )
{
    for(BaseTokenPtrConstIter it = lst.begin(); it != lst.end(); it++)
    {
        AddToCreate( *it, container );
    }
}

//--//

MetaData::Reparser::TypeDef* MetaData::Reparser::Assembly::AddToPatch( TypeDef* ptr )
{
    if(!ptr) return NULL;

    const std::string&      str = ptr->GetDisplayString();
    StringToObjectConstIter it  = m_lookupStringToObjectToPatch.find( str );
    if(it != m_lookupStringToObjectToPatch.end())
    {
        return it->second.CastTo( TypeDef() );
    }

    TypeDef* tdEnclosing    = ptr->m_enclosingType.CastTo( TypeDef() );
    TypeDef* tdEnclosingNew = AddToPatch( tdEnclosing );
    TypeDef* tdNew          = new TypeDef(); tdNew->Init( this, CLR_TkFromType( TBL_TypeDef, (CLR_UINT32)m_lookupTokenToObject.size() ) );


    tdNew->m_patched       = ptr;

    tdNew->m_name          = "$" + ptr->m_name;
    tdNew->m_nameSpace     = ptr->m_nameSpace;

////tdNew->m_extends;
    tdNew->m_enclosingType = tdEnclosingNew;

////tdNew->m_interfaces;

////tdNew->m_methods_Virtual;
////tdNew->m_methods_Instance;
////tdNew->m_methods_Static;
    tdNew->m_dataType      = ptr->m_dataType;

////tdNew->m_fields_Static;
////tdNew->m_fields_Instance;

    tdNew->m_flags         = ptr->m_flags | CLR_RECORD_TYPEDEF::TD_Patched;

    m_lookupTokenToObject        [ tdNew->m_tk ] = tdNew;
    m_lookupStringToObjectToPatch[ str         ] = tdNew;

    m_objectsToProcess.push_back( tdNew );

    return tdNew;
}

MetaData::Reparser::MethodDef* MetaData::Reparser::Assembly::AddToPatch( MethodDef* ptr )
{
    if(!ptr) return NULL;

    const std::string&      str = ptr->GetDisplayString();
    StringToObjectConstIter it  = m_lookupStringToObjectToPatch.find( str );
    if(it != m_lookupStringToObjectToPatch.end())
    {
        return it->second.CastTo( MethodDef() );
    }

    TypeDef* td = ptr->m_container.CastTo( TypeDef() );

    TypeDef*   tdNew = AddToPatch( td );
    MethodDef* mdNew = new MethodDef(); mdNew->Init( this, CLR_TkFromType( TBL_MethodDef, (CLR_UINT32)m_lookupTokenToObject.size() ) );


    mdNew->m_patched         = ptr;

    mdNew->m_container       = tdNew;

    mdNew->m_name            = "$" + ptr->m_name;
////mdNew->m_byteCode;
////mdNew->m_tokens;
////mdNew->m_eh;

    mdNew->m_flags           = ptr->m_flags | CLR_RECORD_METHODDEF::MD_Patched;

    mdNew->m_retVal          = ptr->m_retVal;
    mdNew->m_numArgs         = ptr->m_numArgs;
    mdNew->m_numLocals       = ptr->m_numLocals;
    mdNew->m_lengthEvalStack = ptr->m_lengthEvalStack;

////mdNew->m_locals;
////mdNew->m_sig;


    m_lookupTokenToObject        [ mdNew->m_tk ] = mdNew;
    m_lookupStringToObjectToPatch[ str         ] = mdNew;

    m_objectsToProcess.push_back( mdNew );

    return mdNew;
}

bool MetaData::Reparser::Assembly::IsAPatch( BaseToken* ptr ) const
{
    for(StringToObjectConstIter it = m_lookupStringToObjectToPatch.begin(); it != m_lookupStringToObjectToPatch.end(); it++)
    {
        if(it->second == ptr) return true;
    }

    return false;
}

bool MetaData::Reparser::Assembly::IsAFullObject( BaseToken* ptr ) const
{
    for(StringToObjectConstIter it = m_lookupStringToObjectToCreate.begin(); it != m_lookupStringToObjectToCreate.end(); it++)
    {
        if(it->second == ptr) return true;
    }

    return false;
}


//--//

void MetaData::Reparser::Assembly::Resolve( Assembly* orig, Assembly* patched )
{
    while(m_objectsToProcess.size() > 0)
    {
        BaseToken* ptr = m_objectsToProcess.front(); m_objectsToProcess.pop_front();

        bool isPatch      = IsAPatch     ( ptr );
        bool isFullObject = IsAFullObject( ptr );

        switch(ptr->GetTableOfInstance())
        {
        case TBL_TypeDef:
            {
                TypeDef* tdPatch = ptr->m_patched.CastTo( TypeDef() );
                TypeDef* td      = (TypeDef*)ptr;

                if(!td->m_extends)
                {
                    if(isPatch)
                    {
                        td->m_extends = Clone( tdPatch );
                    }
                    else
                    {
                        td->m_extends = Clone( tdPatch->m_extends );
                    }
                }

                if(!td->m_enclosingType)
                {
                    td->m_enclosingType = Clone( tdPatch->m_enclosingType );
                }

                if(isFullObject)
                {
                    Clone( td->m_interfaces      , tdPatch->m_interfaces       );

                    Clone( td->m_methods_Virtual , tdPatch->m_methods_Virtual  );
                    Clone( td->m_methods_Instance, tdPatch->m_methods_Instance );
                    Clone( td->m_methods_Static  , tdPatch->m_methods_Static   );

                    Clone( td->m_fields_Static   , tdPatch->m_fields_Static    );
                    Clone( td->m_fields_Instance , tdPatch->m_fields_Instance  );
                }
            }
            break;

        case TBL_FieldDef:
            {
                FieldDef* fdPatch = ptr->m_patched.CastTo( FieldDef() );
                FieldDef* fd      = (FieldDef*)ptr;

                Clone( fd->m_sig, fdPatch->m_sig );
            }
            break;

        case TBL_MethodDef:
            {
                MethodDef* mdPatch = ptr->m_patched.CastTo( MethodDef() );
                MethodDef* md      = (MethodDef*)ptr;

                if(!md->m_container)
                {
                    md->m_container = FindObject( mdPatch->m_container->GetDisplayString() );
                }

                Clone( md->m_locals, mdPatch->m_locals );
                Clone( md->m_sig   , mdPatch->m_sig    );
            }
            break;

        case TBL_Attributes:
            {
                Attribute* attrPatch = ptr->m_patched.CastTo( Attribute() );
                Attribute* attr      = (Attribute*)ptr;

                attr->m_owner          = Clone( attrPatch->m_owner       );
                attr->m_constructor    = Clone( attrPatch->m_constructor );

                attr->m_valuesFixed    = attrPatch->m_valuesFixed;
                attr->m_valuesVariable = attrPatch->m_valuesVariable;
            }
            break;

        default:
            //throw (std::string( "Patch of non-supported element: " ) + it->first.c_str());
            throw (std::string( "Patch of non-supported element: " ));
        }

        m_lookupStringToObject[ ptr->GetDisplayString() ] = ptr;
    }


    //
    // Copy bytecode.
    //
    {
        for(StringToObjectIter it=m_lookupStringToObject.begin(); it!=m_lookupStringToObject.end(); it++)
        {
            MethodDef* md = it->second.CastTo( MethodDef() );
            if(md)
            {
                MethodDef* mdPatch = md->m_patched.CastTo( MethodDef() );

                md->m_byteCode = mdPatch->m_byteCode;

                for(MethodDef::EhIter it2 = mdPatch->m_eh.begin(); it2 != mdPatch->m_eh.end(); it2++)
                {
                    MethodDef::Eh& ehSrc = *it2;
                    MethodDef::Eh  ehDst;

                    ehDst              =        ehSrc;
                    ehDst.m_classToken = Clone( ehSrc.m_classToken );

                    md->m_eh.push_back( ehDst );
                }

                for(MethodDef::TokenIter it3 = mdPatch->m_tokens.begin(); it3 != mdPatch->m_tokens.end(); it3++)
                {
                    md->m_tokens[ it3->first ] = Clone( it3->second );
                }
            }
        }
    }

    //
    // Link fields and methods to types.
    //
    {
        for(StringToObjectIter it=m_lookupStringToObject.begin(); it!=m_lookupStringToObject.end(); it++)
        {
            TypeDef* td = it->second.CastTo( TypeDef() );

            if(td)
            {
                td->m_fields_Static  .clear();
                td->m_fields_Instance.clear();

                td->m_methods_Static  .clear();
                td->m_methods_Virtual .clear();
                td->m_methods_Instance.clear();

                for(StringToObjectIter it2=m_lookupStringToObject.begin(); it2!=m_lookupStringToObject.end(); it2++)
                {
                    MethodDef* md = it2->second.CastTo( MethodDef() );
                    if(md)
                    {
                        if(md->m_container == td)
                        {
                            if(md->m_flags & CLR_RECORD_METHODDEF::MD_Static)
                            {
                                td->m_methods_Static.push_back( md );
                            }
                            else if(md->m_flags & CLR_RECORD_METHODDEF::MD_Virtual)
                            {
                                td->m_methods_Virtual.push_back( md );
                            }
                            else
                            {
                                td->m_methods_Instance.push_back( md );
                            }
                        }
                    }

                    FieldDef* fd = it2->second.CastTo( FieldDef() );
                    if(fd)
                    {
                        if(fd->m_container == td)
                        {
                            if(fd->m_flags & CLR_RECORD_FIELDDEF::FD_Static)
                            {
                                td->m_fields_Static.push_back( fd );
                            }
                            else
                            {
                                td->m_fields_Instance.push_back( fd );
                            }
                        }
                    }
                }
            }
        }
    }

    Dump();
}

//--//

MetaData::Reparser::BaseToken* MetaData::Reparser::Assembly::Clone( BaseToken* ptr )
{
    if(!ptr) return NULL;

    const std::string&      str    = ptr->GetDisplayString();
    StringToObjectConstIter it     = m_lookupStringToObject.find( str );
    BaseToken*              ptrNew = NULL;

    if(it != m_lookupStringToObject.end())
    {
        return it->second;
    }

    //printf( "Cloning: %s\n", str.c_str() );

    //--//

    switch(ptr->GetTableOfInstance())
    {
    case TBL_AssemblyRef:
        {
            AssemblyRef* arOld = (AssemblyRef*)ptr;
            AssemblyRef* arNew = new AssemblyRef();

            arNew->m_name    = arOld->m_name;
            arNew->m_version = arOld->m_version;

            //
            // Reference to the patched assembly? Point to the OLD version.
            //
            if(this->m_name == arNew->m_name && memcmp( &this->m_version, &arNew->m_version, sizeof(this->m_version) ) == 0)
            {
                arNew->m_version = m_assemblyOrig->m_version;
            }

            ptrNew = arNew;
        }
        break;

    case TBL_TypeRef:
        {
            TypeRef* trOld = (TypeRef*)ptr;
            TypeRef* trNew = new TypeRef();

            trNew->m_name      = trOld->m_name;
            trNew->m_nameSpace = trOld->m_nameSpace;
            trNew->m_scope     = Clone( trOld->m_scope );

            ptrNew = trNew;
        }
        break;

    case TBL_TypeDef:
        {
            TypeDef* tdOld = (TypeDef*)ptr;
            TypeRef* trNew = new TypeRef();

            trNew->m_name      = tdOld->m_name;
            trNew->m_nameSpace = tdOld->m_nameSpace;

            if(!tdOld->m_enclosingType)
            {
                trNew->m_scope = Clone( ptr->m_holder );
            }
            else
            {
                trNew->m_scope = Clone( tdOld->m_enclosingType );
            }

            ptrNew = trNew;
        }
        break;

        //--//

    case TBL_FieldRef:
        {
            FieldRef* frOld = (FieldRef*)ptr;
            FieldRef* frNew = new FieldRef();

            frNew->m_name      =        frOld->m_name       ;
            frNew->m_container = Clone( frOld->m_container );

            Clone( frNew->m_sig, frOld->m_sig );

            ptrNew = frNew;
        }
        break;

    case TBL_FieldDef:
        {
            FieldDef* fdOld = (FieldDef*)ptr;
            FieldRef* frNew = new FieldRef();

            frNew->m_name      =        fdOld->m_name       ;
            frNew->m_container = Clone( fdOld->m_container );

            Clone( frNew->m_sig, fdOld->m_sig );

            ptrNew = frNew;
        }
        break;

        //--//

    case TBL_MethodRef:
        {
            MethodRef* mrOld = (MethodRef*)ptr;
            MethodRef* mrNew = new MethodRef();

            mrNew->m_name      =        mrOld->m_name       ;
            mrNew->m_container = Clone( mrOld->m_container );

            Clone( mrNew->m_sig, mrOld->m_sig );

            ptrNew = mrNew;
        }
        break;

    case TBL_MethodDef:
        {
            MethodDef* mdOld = (MethodDef*)ptr;
            MethodRef* mrNew = new MethodRef();

            mrNew->m_name      =        mdOld->m_name       ;
            mrNew->m_container = Clone( mdOld->m_container );

            Clone( mrNew->m_sig, mdOld->m_sig );

            ptrNew = mrNew;
        }
        break;

    case TBL_TypeSpec:
        {
            TypeSpec* tsOld = (TypeSpec*)ptr;
            TypeSpec* tsNew = new TypeSpec();

            Clone( tsNew->m_sig, tsOld->m_sig );

            ptrNew = tsNew;
        }
        break;

    case TBL_Strings:
        {
            String* sOld = (String*)ptr;
            String* sNew = new String();

            sNew->m_value = sOld->m_value;

            ptrNew = sNew;
        }
        break;

    default:
        throw (std::string( "Patch of non-supported element: " ) + str );
    }

    ptrNew->Init( this, CLR_TkFromType( ptrNew->GetTableOfInstance(), (CLR_UINT32)m_lookupTokenToObject.size() ) );

    ptrNew->m_displayString = str;
    ptrNew->m_patched       = ptr;

    m_lookupTokenToObject [ ptrNew->m_tk ] = ptrNew;
    m_lookupStringToObject[ str          ] = ptrNew;

    return ptrNew;
}

void MetaData::Reparser::Assembly::Clone( BaseTokenPtrList & lstDst, const BaseTokenPtrList & lstSrc )
{
    for(BaseTokenPtrConstIter it = lstSrc.begin(); it != lstSrc.end(); it++)
    {
        lstDst.push_back( Clone( *it ) );
    }
}

void MetaData::Reparser::Assembly::Clone( TypeSignatureList& lstDst, const TypeSignatureList& lstSrc )
{
    for(TypeSignatureConstIter it = lstSrc.begin(); it != lstSrc.end(); it++)
    {
        TypeSignature var;

        Clone( var, *it );

        lstDst.push_back( var );
    }
}

void MetaData::Reparser::Assembly::Clone( TypeSignature& sigDst, const TypeSignature& sigSrc )
{
    sigDst.m_opt = sigSrc.m_opt;

    if(!!sigSrc.m_token)
    {
        sigDst.m_token = Clone( sigSrc.m_token );
    }

    if(!!sigSrc.m_sub)
    {
        sigDst.m_sub.Allocate();

        Clone( *sigDst.m_sub.m_ptr, *sigSrc.m_sub.m_ptr );
    }
}

void MetaData::Reparser::Assembly::Clone( MethodSignature& sigDst, const MethodSignature& sigSrc )
{
           sigDst.m_flags    = sigSrc.m_flags      ;
    Clone( sigDst.m_retValue , sigSrc.m_retValue  );
    Clone( sigDst.m_lstParams, sigSrc.m_lstParams );
}

void MetaData::Reparser::Assembly::Clone( LocalVarSignature& sigDst, const LocalVarSignature& sigSrc )
{
    Clone( sigDst.m_lstVars, sigSrc.m_lstVars );
}
